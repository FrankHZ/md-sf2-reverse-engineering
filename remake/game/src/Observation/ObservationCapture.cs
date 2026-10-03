using System.Collections;
using System.Reflection;
using System.Text.Json;
using Godot;

namespace Sf2.Remake.GodotAdapter.Observation;

// External evidence only. The game thread copies primitives; the worker owns bytes and I/O.
public sealed partial class ObservationCapture : Node
{
    [Signal] public delegate void CaptureFailedEventHandler(string reason);
    private int _ownerThread;
    private bool _failurePublished;
    public const long QueueLimit = 32L * 1024 * 1024;
    public const int RecordLimit = 1024 * 1024;
    public const long PendingLimit = 64L * 1024 * 1024;
    private readonly object _gate = new();
    private readonly Queue<Pending> _queue = new();
    private readonly SemaphoreSlim _ready = new(0);
    private readonly Dictionary<string, long> _counts = new(StringComparer.Ordinal);
    private Task? _writer;
    private FileStream? _output;
    private long _sequence, _queuedBytes, _pendingBytes, _queueHigh, _pendingHigh;
    private long _writtenBytes, _writtenRecords, _copyMicros, _serializeMicros;
    private string? _failure;
    private bool _finishing, _completed;
    private const long ReservedBuffers = 16L * 1024 * 1024;
    private readonly Dictionary<string, long> _descriptors = new(StringComparer.Ordinal);
    private long _descriptorSequence;
    private long _allocationStart, _mainAllocationStart;
    private sealed record Pending(long UpperBytes, long MemoryBytes, object Facts, bool Terminal);

    internal static ObservationCapture? Find(Node owner) => owner.GetTree()?.Root.GetNodeOrNull<ObservationCapture>("ObservationCapture");

    public bool Begin(string destination)
    {
        _ownerThread = System.Environment.CurrentManagedThreadId;
        if (_writer is not null) { Fail("capture-already-started"); return false; }
        try
        {
            // The probe performs the repository-local/link/freshness preflight before Begin.
            _output = new FileStream(destination, FileMode.CreateNew, System.IO.FileAccess.Write, FileShare.Read,
                64 * 1024, FileOptions.SequentialScan);
            _allocationStart = GC.GetTotalAllocatedBytes(false);
            _mainAllocationStart = GC.GetAllocatedBytesForCurrentThread();
            _writer = Task.Run(WriteLoop);
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException)
        { Fail("capture-open-failed:" + error.GetType().Name); return false; }
    }

    public bool IsAccepting() => Accepting;
    public long ResourceVisit { get; private set; }
    public void SetResourceVisit(long value) => ResourceVisit = value;
    private string? _resourceLifetime;
    private readonly HashSet<string> _requirements = new(StringComparer.Ordinal);
    private long _requirementBytes;
    internal void ResourceLifetime(string value)
    {
        if (value == _resourceLifetime) return;
        _resourceLifetime = value; _requirements.Clear(); _requirementBytes = 0;
    }
    internal bool NewRequirement(string key)
    {
        if (!Accepting || _requirements.Contains(key)) return false;
        long bytes = key.Length * 2L + 128;
        if (_requirements.Count >= 8192 || _requirementBytes + bytes > 4L * 1024 * 1024)
        { Fail("capture-requirement-memory-limit"); return false; }
        _requirements.Add(key); _requirementBytes += bytes;
        return true;
    }
    // Selectors contain only immutable project-owned primitives/arrays and belong to
    // an actual mounted texture. The resource object itself never reaches the worker.
    internal object Descriptor(string identity, object selector)
    {
        if (identity.Length is 0 or > 20 || !ulong.TryParse(identity, out _)) { Fail("capture-invalid-resource-identity"); return new { captureDescriptor = 0L }; }
        if (!_descriptors.TryGetValue(identity, out long number))
        {
            if (_descriptors.Count >= 4096) { Fail("capture-descriptor-limit"); return new { captureDescriptor = 0L }; }
            number = ++_descriptorSequence;
            if (!RecordFrozenFacts("resourceDescriptors", new { id = number, selector })) return new { captureDescriptor = 0L };
            _descriptors.Add(identity, number);
        }
        return new { captureDescriptor = number };
    }
    public long InputOrdinal { get; private set; }
    private Guid? _session;
    public void SetInputOrdinal(long ordinal) => InputOrdinal = ordinal;
    internal bool Accepting { get { lock (_gate) return _writer is not null && _failure is null && !_finishing; } }
    internal void BindSession(Guid session)
    {
        if (_session is { } old && old != session) { Fail("capture-session-replaced"); return; }
        _session = session;
    }
    internal void ViewDetached(string view)
    {
        if (Accepting)
        {
            _descriptors.Clear();
            RecordFacts("descriptorReset", new { view });
        }
        if (Accepting) RecordFacts("viewLifecycle", new { operation = "detached", view, sessionId = _session, processFrame = Engine.GetProcessFrames() });
    }
    internal bool RecordFacts(string channel, object facts) => RecordFrozenFacts(channel, facts);

    // Callers hand over newly built primitive facts whose collections will never be
    // changed after this call. Measure once; no JSON/Godot/JSON round trip or deep copy.
    internal bool RecordFrozenFacts(string channel, object facts)
    {
        if (!Accepting) return false;
        long started = System.Diagnostics.Stopwatch.GetTimestamp();
        try
        {
            lock (_gate)
            {
                if (_failure is not null || _finishing) return false;
                var budget = new CopyBudget(_queuedBytes, _pendingBytes);
                MeasureFacts(facts, budget);
                return QueueFacts(channel, facts, false, budget, started);
            }
        }
        catch (InvalidOperationException error) { Fail(error.Message + ":channel=" + channel); return false; }
    }
    // Resolve lazy/live-owned fact collections synchronously before handing ownership to
    // the writer. This path preserves the Variant transport's primitive representation
    // without allocating a Godot collection for every state/projection node.
    internal bool RecordSnapshotFacts(string channel, object facts)
    {
        if (!Accepting) return false;
        long started = System.Diagnostics.Stopwatch.GetTimestamp();
        try
        {
            lock (_gate)
            {
                if (_failure is not null || _finishing) return false;
                var budget = new CopyBudget(_queuedBytes, _pendingBytes);
                var snapshot = SnapshotFacts(facts, budget);
                return QueueFacts(channel, snapshot, false, budget, started);
            }
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException)
        { Fail(error.Message + ":channel=" + channel); return false; }
    }

    private static object? SnapshotFacts(object? value, CopyBudget budget, int depth = 0)
    {
        if (depth > 64) throw new InvalidOperationException("capture-payload-depth-limit");
        if (value is Variant variant) return CopyVariant(variant, budget, depth);
        if (value is Godot.Collections.Dictionary dictionary) return CopyVariant(dictionary, budget, depth);
        if (value is Godot.Collections.Array array) return CopyVariant(array, budget, depth);
        if (value is GodotObject) throw new InvalidOperationException("capture-live-godot-object");
        if (value is null) { budget.Add(4,16); return null; }
        if (value is Guid guid) value = guid.ToString();
        if (value is string text) { budget.Add(text.Length*6L+2,text.Length*2L+24); return text; }
        if (value is bool) { budget.Add(5,24); return value; }
        if (value is Enum or byte or sbyte or short or ushort or int or uint or long or ulong)
        { budget.Add(32,24); return Convert.ToInt64(value); }
        if (value is float or double or decimal) { budget.Add(32,24); return Convert.ToDouble(value); }
        if (value is IDictionary mapping)
        {
            budget.Add(2,128);
            var result = new Dictionary<string,object?>();
            foreach (DictionaryEntry entry in mapping)
            {
                string key = Convert.ToString(entry.Key)!;
                budget.Add(key.Length*6L+4,key.Length*2L+64);
                result.Add(key,SnapshotFacts(entry.Value,budget,depth+1));
            }
            return result;
        }
        if (value is IEnumerable sequence)
        {
            budget.Add(2,64);
            var result = new List<object?>();
            foreach (var item in sequence) { budget.Add(1,16); result.Add(SnapshotFacts(item,budget,depth+1)); }
            return result;
        }
        budget.Add(2,128);
        var row = new Dictionary<string,object?>();
        foreach (var property in FactProperties(value.GetType()))
        {
            budget.Add(property.Name.Length*6L+4,property.Name.Length*2L+64);
            row.Add(property.Name,SnapshotFacts(property.GetValue(value),budget,depth+1));
        }
        return row;
    }

    // Shallow selection only; callers must snapshot selected values before enqueue.
    internal static Dictionary<string,object?> FactFields(object? facts) => facts is null ? [] :
        facts is Dictionary<string,object?> fields ? fields :
        FactProperties(facts.GetType()).ToDictionary(property=>property.Name,property=>property.GetValue(facts));

    internal void AudioReceipt(object receipt)
    {
        if (Accepting) RecordFacts("audioReceipts", new { receipt, poll = new
        {
            sessionId = _session, inputOrdinal = InputOrdinal, hostUpdate = Engine.GetProcessFrames(), projectionStage = "audio-callback",
        } });
    }

    private Godot.Collections.Dictionary? ResourceReference(Godot.Collections.Dictionary payload)
    {
        if (payload.TryGetValue("resourceIdentity", out var identity))
        {
            if (identity.VariantType != Variant.Type.String || identity.AsString().Length is 0 or > 20 || !ulong.TryParse(identity.AsString(), out _))
            { Fail("capture-invalid-resource-identity"); return null; }
            string key = identity.AsString();
            var used = payload["used"].AsGodotDictionary();
            bool wrapped = used.ContainsKey("selector");
            var selector = wrapped ? used["selector"].AsGodotDictionary() : used;
            if (!_descriptors.TryGetValue(key, out long descriptor))
            {
                if (_descriptors.Count >= 4096) { Fail("capture-descriptor-limit"); return null; }
                descriptor = ++_descriptorSequence;
                if (!Enqueue("resourceDescriptors",new Godot.Collections.Dictionary { ["id"] = descriptor, ["selector"] = selector },false)) return null;
                _descriptors.Add(key,descriptor);
            }
            payload = payload.Duplicate();
            var reference = new Godot.Collections.Dictionary { ["captureDescriptor"] = descriptor };
            if (wrapped) { used = used.Duplicate(); used["selector"] = reference; payload["used"] = used; }
            else payload["used"] = reference;
        }
        return payload;
    }
    public bool Record(string channel, Godot.Collections.Dictionary payload)
    {
        if (!Accepting) return false;
        if (channel == "resourceUses")
        {
            var referenced = ResourceReference(payload);
            if (referenced is null) return false;
            payload = referenced;
        }
        return Enqueue(channel,payload,false);
    }
    public bool RecordDrawUses(Godot.Collections.Dictionary identity, Godot.Collections.Array uses)
    {
        if (!Accepting) return false;
        var referenced = new Godot.Collections.Array();
        foreach (var use in uses)
        {
            var row = ResourceReference(use.AsGodotDictionary());
            if (row is null) return false;
            referenced.Add(row);
        }
        return Enqueue("drawUses",new Godot.Collections.Dictionary { ["identity"] = identity, ["uses"] = referenced },false);
    }
    public bool ValidateControl(Godot.Collections.Array rows, string site = "retained-control")
    {
        var budget = new CopyBudget(0, 0, control: true);
        try { CopyVariant(rows, budget, copy: false); return true; }
        catch (InvalidOperationException error)
        {
            Fail($"{error.Message}:site={site}:bytes={budget.Bytes}:memory={budget.Memory}:rows={rows.Count}");
            return false;
        }
    }
    public bool Complete(Godot.Collections.Dictionary summary) => Enqueue("terminal", summary, true);
    public void Cancel(string reason) => Fail("capture-cancelled:" + reason);
    public bool IsFinished() { lock (_gate) return (_writer is null || _writer.IsCompleted) && (_completed || _failure is not null); }
    public bool Succeeded() { lock (_gate) return _completed && _failure is null; }
    public string Failure() { lock (_gate) return _failure ?? ""; }
    public long Count(string channel) { lock (_gate) return _counts.GetValueOrDefault(channel); }

    public string ReadStatusJson()
    {
        lock (_gate) return JsonSerializer.Serialize(new
        {
            completed = _completed, failure = _failure, sequence = _sequence,
            queuedBytes = _queuedBytes, pendingBytes = _pendingBytes,
            queueHighWater = _queueHigh, pendingHighWater = _pendingHigh,
            writtenBytes = _writtenBytes, writtenRecords = _writtenRecords,
            copyMicros = _copyMicros, serializeMicros = _serializeMicros,
            allocatedBytes = GC.GetTotalAllocatedBytes(false) - _allocationStart,
            mainAllocatedBytes = GC.GetAllocatedBytesForCurrentThread() - _mainAllocationStart,
            descriptors = _descriptors.Count, reservedBuffers = ReservedBuffers,
            queueLimit = QueueLimit, recordLimit = RecordLimit, pendingLimit = PendingLimit,
        });
    }

    private bool Enqueue(string channel, Godot.Collections.Dictionary payload, bool terminal)
    {
        long started = System.Diagnostics.Stopwatch.GetTimestamp();
        try
        {
            lock (_gate)
            {
                if (_writer is null || _failure is not null || _finishing)
                { FailLocked("capture-not-writable"); return false; }
                var budget = new CopyBudget(_queuedBytes, _pendingBytes);
                var copied = CopyVariant(payload, budget);
                return QueueFacts(channel, copied, terminal, budget, started);
            }
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException)
        { Fail(error.Message); return false; }
    }

    private bool QueueFacts(string channel, object? copied, bool terminal, CopyBudget budget, long started)
    {
        if (channel.Length is 0 or > 64 || !_counts.ContainsKey(channel) && _counts.Count >= 64)
            throw new InvalidOperationException("capture-channel-limit");
        long sequence = _sequence + 1;
        long index = _counts.GetValueOrDefault(channel);
        budget.Add(256 + channel.Length * 6L, 512 + channel.Length * 2L); // Envelope, sequence and channel plus its immutable string.
        var facts = new Dictionary<string, object?>
        {
            ["captureSequence"] = sequence, ["channel"] = channel,
            ["index"] = index, ["payload"] = copied,
        };
        long memory = Math.Max(budget.Memory, budget.Bytes * 2);
        if (budget.Bytes > RecordLimit) throw new InvalidOperationException("capture-record-too-large");
        if (_queuedBytes + budget.Bytes > QueueLimit) throw new InvalidOperationException("capture-queue-overflow");
        // Includes the in-flight record and the writer's fixed 1MiB serialization buffer.
        if (_pendingBytes + memory + ReservedBuffers > PendingLimit) throw new InvalidOperationException("capture-pending-overflow");
        _sequence = sequence; _counts[channel] = index + 1;
        _queuedBytes += budget.Bytes; _pendingBytes += memory;
        _queueHigh = Math.Max(_queueHigh, _queuedBytes); _pendingHigh = Math.Max(_pendingHigh, _pendingBytes + ReservedBuffers);
        _queue.Enqueue(new(budget.Bytes, memory, facts, terminal));
        _finishing = terminal;
        _copyMicros += (long)(System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds * 1000);
        _ready.Release();
        return true;
    }

    // The failure can originate on the writer. Godot notification is always on the
    // owning thread; callbacks also check IsAccepting before constructing any facts.
    public override void _Process(double delta) => PublishFailure();
    private void PublishFailure()
    {
        if (System.Environment.CurrentManagedThreadId != _ownerThread || _failurePublished) return;
        string? failure;
        lock (_gate) failure = _failure;
        if (failure is null) return;
        _failurePublished = true;
        EmitSignal(SignalName.CaptureFailed, failure);
    }

    private void Fail(string reason) { lock (_gate) FailLocked(reason); PublishFailure(); }
    private void FailLocked(string reason)
    {
        _failure ??= reason;
        if (_writer is not null) _ready.Release();
    }

    private async Task WriteLoop()
    {
        // No Godot object, Variant, node, engine clock or live session crosses this boundary.
        byte[] buffer = new byte[RecordLimit];
        bool terminalWritten = false;
        try
        {
            while (true)
            {
                await _ready.WaitAsync().ConfigureAwait(false);
                Pending? row;
                lock (_gate)
                {
                    if (_failure is not null) break;
                    row = _queue.Count == 0 ? null : _queue.Dequeue();
                }
                if (row is null) continue;
                long started = System.Diagnostics.Stopwatch.GetTimestamp();
                using var bytes = new MemoryStream(buffer, 0, buffer.Length, true, true);
                bytes.SetLength(0);
                try
                {
                    using var json = new Utf8JsonWriter(bytes);
                    JsonSerializer.Serialize(json, row.Facts);
                    json.Flush();
                    bytes.WriteByte((byte)'\n');
                }
                catch (NotSupportedException) { throw new InvalidOperationException("capture-record-too-large"); }
                int length = checked((int)bytes.Length);
                if (length > row.UpperBytes || length > RecordLimit) throw new InvalidOperationException("capture-record-budget-invalid");
                _output!.Write(buffer, 0, length);
                lock (_gate)
                {
                    _serializeMicros += (long)(System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds * 1000);
                    _writtenBytes += length; _writtenRecords++;
                    _queuedBytes -= row.UpperBytes; _pendingBytes -= row.MemoryBytes;
                }
                if (!row.Terminal) continue;
                _output.Flush(true);
                terminalWritten = true;
                break;
            }
        }
        catch (Exception error)
        { Fail(error is InvalidOperationException ? error.Message : "capture-write-failed:" + error.GetType().Name); }
        finally
        {
            try { _output?.Dispose(); }
            catch (IOException) { Fail("capture-close-failed"); }
            lock (_gate)
            {
                _queue.Clear(); _queuedBytes = 0; _pendingBytes = 0;
                if (terminalWritten && _failure is null) _completed = true;
            }
        }
    }

    public override void _ExitTree()
    {
        lock (_gate) if (!_completed) FailLocked("capture-detached-before-terminal");
    }

    private sealed class CopyBudget(long queued, long pending, bool control = false)
    {
        internal long Bytes, Memory;
        internal void Add(long bytes, long memory)
        {
            Bytes += bytes; Memory += memory;
            if (control)
            {
                if (Memory > 1024 * 1024) throw new InvalidOperationException("capture-control-memory-limit");
                return;
            }
            if (Bytes > RecordLimit) throw new InvalidOperationException("capture-record-too-large");
            if (queued + Bytes > QueueLimit) throw new InvalidOperationException("capture-queue-overflow");
            if (pending + Math.Max(Memory, Bytes * 2) + ReservedBuffers > PendingLimit)
                throw new InvalidOperationException("capture-pending-overflow");
        }
    }

    private static void MeasureFacts(object? value, CopyBudget budget, int depth = 0)
    {
        if (depth > 64) throw new InvalidOperationException("capture-payload-depth-limit");
        if (value is null) { budget.Add(4,16); return; }
        if (value is GodotObject or Variant) throw new InvalidOperationException("capture-live-godot-object");
        if (value is string text) { budget.Add(text.Length * 6L + 2, text.Length * 2L + 24); return; }
        if (value is Guid) { budget.Add(38,48); return; }
        if (value is bool) { budget.Add(5,24); return; }
        if (value is Enum or byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal)
        { budget.Add(32,24); return; }
        if (value is IEnumerable sequence)
        {
            budget.Add(2,64);
            foreach (var item in sequence) { budget.Add(1,16); MeasureFacts(item,budget,depth+1); }
            return;
        }
        budget.Add(2,128);
        foreach (var property in FactProperties(value.GetType()))
        {
            budget.Add(property.Name.Length * 6L + 4,64);
            MeasureFacts(property.GetValue(value),budget,depth+1);
        }
    }

    private static object? CopyVariant(Variant value, CopyBudget budget, int depth = 0, bool copy = true)
    {
        if (depth > 64) throw new InvalidOperationException("capture-payload-depth-limit");
        switch (value.VariantType)
        {
            case Variant.Type.Nil: budget.Add(4, 16); return null;
            case Variant.Type.Bool: budget.Add(5, 24); return value.AsBool();
            case Variant.Type.Int: budget.Add(32, 24); return value.AsInt64();
            case Variant.Type.Float: budget.Add(32, 24); return value.AsDouble();
            case Variant.Type.String:
                string text = value.AsString(); budget.Add(text.Length * 6L + 2, text.Length * 2L + 24); return text;
            case Variant.Type.Dictionary:
                var dictionary = value.AsGodotDictionary(); budget.Add(2, 128);
                var result = copy ? new Dictionary<string, object?>() : null;
                foreach (var pair in dictionary)
                {
                    if (pair.Key.VariantType != Variant.Type.String) throw new InvalidOperationException("capture-non-string-key");
                    string key = pair.Key.AsString(); budget.Add(key.Length * 6L + 4, key.Length * 2L + 64);
                    var item = CopyVariant(pair.Value, budget, depth + 1, copy);
                    result?.Add(key, item);
                }
                return result;
            case Variant.Type.Array:
                var array = value.AsGodotArray(); budget.Add(2, 64);
                var items = copy ? new List<object?>() : null;
                foreach (var item in array) { budget.Add(1, 16); var child = CopyVariant(item, budget, depth + 1, copy); items?.Add(child); }
                return items;
            default: throw new InvalidOperationException("capture-non-primitive-payload:" + value.VariantType);
        }
    }

    // Existing JSON-shaped adapter facts are copied on the owning thread, without serializing
    // and parsing the world. This converter is never called by the writer or on live nodes.
    internal static Godot.Collections.Dictionary ToDictionary(object facts) => ToVariant(facts).AsGodotDictionary();
    private static readonly Dictionary<Type, PropertyInfo[]> Properties = new();
    private static PropertyInfo[] FactProperties(Type type)
    {
        if (!Properties.TryGetValue(type, out var properties))
        {
            properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0).ToArray();
            Properties.Add(type, properties);
        }
        return properties;
    }
    private static Variant ToVariant(object? value)
    {
        if (value is null) return default;
        if (value is GodotObject) throw new InvalidOperationException("capture-live-godot-object");
        if (value is string text) return text;
        if (value is Guid id) return id.ToString();
        if (value is bool flag) return flag;
        if (value is Enum) return Convert.ToInt64(value);
        if (value is byte or sbyte or short or ushort or int or uint or long) return Convert.ToInt64(value);
        if (value is ulong number) return checked((long)number);
        if (value is float or double or decimal) return Convert.ToDouble(value);
        if (value is IDictionary dictionary)
        {
            var result = new Godot.Collections.Dictionary();
            foreach (DictionaryEntry pair in dictionary) result[Convert.ToString(pair.Key)!] = ToVariant(pair.Value);
            return result;
        }
        if (value is IEnumerable sequence)
        {
            var result = new Godot.Collections.Array();
            foreach (var item in sequence) result.Add(ToVariant(item));
            return result;
        }
        var properties = FactProperties(value.GetType());
        var row = new Godot.Collections.Dictionary();
        foreach (var property in properties) row[property.Name] = ToVariant(property.GetValue(value));
        return row;
    }
}

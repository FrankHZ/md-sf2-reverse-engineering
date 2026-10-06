using System.Numerics;
using static H4Comparison.Operands;

namespace H4Comparison;

internal sealed class ReportIntegrityOperands
{
    private sealed record TupleValue(List<object?> Items);
    private sealed record MappingValue(List<object?> Pairs)
    {
        public Dictionary<CounterKey, object?> Index { get; } = Pairs.Cast<List<object?>>()
            .ToDictionary(pair => new CounterKey(pair[0]), pair => pair[1]);
    }
    private static bool IsMap(object? value) => value is MappingValue or Dictionary<string, object?>;
    private static IEnumerable<(object? Key, object? Value)> Entries(object? value) => value is MappingValue map
        ? map.Pairs.Cast<List<object?>>().Select(pair => (pair[0], pair[1]))
        : ((Dictionary<string, object?>)value!).Select(pair => ((object?)pair.Key, pair.Value));
    private static int MapCount(object? value) => value is MappingValue map ? map.Index.Count : ((Dictionary<string, object?>)value!).Count;
    private static bool MapLookup(object? value, object? key, out object? found)
    {
        if (value is MappingValue map) return map.Index.TryGetValue(new CounterKey(key), out found);
        if (key is string text) return ((Dictionary<string, object?>)value!).TryGetValue(text, out found);
        found = null; return false;
    }
    public VisualSourceOperands Reader { get; } = new();
    private bool cancelled;
    public void Cancel() { cancelled = true; Reader.Cancel(); }
    private ValueTask Release(object? value, int cursor) => cancelled ? ValueTask.CompletedTask : Reader.Release(value, cursor);
    public object? Prepare(object? envelope)
    {
        var value = Reader.Prepare(At(envelope, "source"));
        return RestoreContainers(value, At(envelope, "containers"));
    }
    private static object? RestoreContainers(object? value, object? paths)
    {
        foreach (var raw in (List<object?>)paths!)
        {
            var path = (List<object?>)At(raw, "path")!;
            object Wrap(object? items) => At(raw, "kind") switch
            {
                "tuple" => new TupleValue((List<object?>)items!),
                "mapping" => new MappingValue((List<object?>)items!),
                _ => throw new InvalidDataException("Unknown report container type")
            };
            if (path.Count == 0) { value = Wrap(value); continue; }
            var parent = value;
            foreach (var key in path.Take(path.Count - 1))
                parent = key is string name ? At(parent, name) : ((List<object?>)parent!)[(int)(BigInteger)key!];
            if (path[^1] is string field)
            {
                var map = (Dictionary<string, object?>)parent!;
                map[field] = Wrap(map[field]);
            }
            else
            {
                var list = (List<object?>)parent!; var index = (int)(BigInteger)path[^1]!;
                list[index] = Wrap(list[index]);
            }
        }
        return value;
    }
    public void Reply(object? message)
    {
        // Reader.Reply prepares source references before resuming the pending read.
        // Restore native container types before resuming the same continuation.
        var envelope = At(message, "value");
        var prepared = Prepare(envelope);
        Reader.Reply(Dict(("sequence", At(message, "sequence")),
            ("value", Dict(("value", prepared), ("references", new List<object?>())))));
    }
    public static string TypeName(object? value) => value is TupleValue ? "tuple" : value is MappingValue ? "dict" : VisualSourceOperands.TypeName(value);
    public static bool IsDeferred(object? value) => value is not (null or bool or BigInteger or double or string
        or List<object?> or Dictionary<string, object?> or TupleValue or MappingValue);
    public static object KeyFact(object? value) => value is TupleValue tuple
        ? Dict(("tuple", tuple.Items.Select(KeyFact).ToList())) : Dict(("value", value));
    private static object? Items(object? value) => value is TupleValue tuple ? tuple.Items : value;
    public async ValueTask<bool> Same(object? a, object? b)
    {
        if (a is TupleValue || b is TupleValue)
            return a is TupleValue at && b is TupleValue bt && await Same(at.Items, bt.Items);
        if (IsMap(a) || IsMap(b))
        {
            if (!IsMap(a) || !IsMap(b) || MapCount(a) != MapCount(b)) return false;
            foreach (var (key, value) in Entries(a))
                if (!MapLookup(b, key, out var other) || !ReferenceEquals(value, other) && !await Same(value, other)) return false;
            return true;
        }
        var aList = a is List<object?>; var bList = b is List<object?>;
        var aSource = a is not (null or bool or BigInteger or double or string or List<object?> or Dictionary<string, object?>);
        var bSource = b is not (null or bool or BigInteger or double or string or List<object?> or Dictionary<string, object?>);
        if (!(aList || aSource) || !(bList || bSource)) return Equal(a, b);
        if (await Reader.Length(a) != await Reader.Length(b)) return false;
        // Match the spool's __eq__: its operand is traversed first even when on the right.
        if (!aSource && bSource) (a, b) = (b, a);
        var leftCursor = Reader.AllocateCursor(); var rightCursor = Reader.AllocateCursor();
        try
        {
            for (var i = 0; ; i++)
            {
                var left = await Reader.Next(a, i, leftCursor); var right = await Reader.Next(b, i, rightCursor);
                if (left.Done || right.Done)
                {
                    if (left.Done != right.Done) throw new OperandError("ValueError", left.Done
                        ? "zip() argument 2 is longer than argument 1" : "zip() argument 2 is shorter than argument 1");
                    return true;
                }
                if (!(aList && bList && ReferenceEquals(left.Value, right.Value)) && !await Same(left.Value, right.Value)) return false;
            }
        }
        finally
        {
            await Release(b, rightCursor);
            await Release(a, leftCursor);
        }
    }
    public static object? GetField(object? value, string key, object? fallback = null) => IsMap(value)
        ? MapLookup(value, key, out var found) ? found : fallback
        : throw new OperandError("AttributeError", $"'{TypeName(value)}' object has no attribute 'get'");
    public ValueTask<object?> Field(object? value, string key)
    {
        if (IsMap(value)) return MapLookup(value, key, out var found) ? ValueTask.FromResult(found)
            : throw new OperandError("KeyError", key);
        if (value is TupleValue) throw new OperandError("TypeError", "tuple indices must be integers or slices, not str");
        return Reader.Field(value, key);
    }
    public async IAsyncEnumerable<object?> Rows(object? value)
    {
        if (IsMap(value))
        {
            foreach (var (key, _) in Entries(value)) yield return key;
            yield break;
        }
        if (value is string text)
        {
            foreach (var rune in text.EnumerateRunes()) yield return rune.ToString();
            yield break;
        }
        if (value is null or BigInteger or double or bool)
            throw new OperandError("TypeError", $"'{TypeName(value)}' object is not iterable");
        value = Items(value);
        var cursor = Reader.AllocateCursor();
        try
        {
            for (var index = 0; ; index++)
            {
                var row = await Reader.Next(value, index, cursor);
                if (row.Done) yield break;
                yield return row.Value;
            }
        }
        finally { await Release(value, cursor); }
    }
    public async ValueTask<bool> TruthValue(object? value) => value is MappingValue map ? map.Index.Count != 0 : value is null or BigInteger or double or bool or string
        or List<object?> or Dictionary<string, object?> ? Truth(value) : await Reader.Length(Items(value)) != BigInteger.Zero;
    public async IAsyncEnumerable<object?> FirstThree(object? value)
    {
        if (IsMap(value)) throw new OperandError("AllySliceKeyError");
        if (value is null or BigInteger or double or bool)
            throw new OperandError("TypeError", $"'{TypeName(value)}' object is not subscriptable");
        value = Items(value);
        var count = BigInteger.Min(3, await Reader.Length(value));
        for (var index = BigInteger.Zero; index < count; index++) yield return await Reader.Index(value, index);
    }
    public static string Text(object? value) => value switch
    {
        TupleValue tuple => "(" + string.Join(", ", tuple.Items.Select(Repr)) + (tuple.Items.Count == 1 ? "," : "") + ")",
        List<object?> list => "[" + string.Join(", ", list.Select(Repr)) + "]",
        MappingValue or Dictionary<string, object?> => "{" + string.Join(", ", Entries(value).Select(p => Repr(p.Key) + ": " + Repr(p.Value))) + "}",
        _ => SceneOperands.Str(value)
    };
    private static string Repr(object? value) => value is string ? SceneOperands.Repr(value) : Text(value);
    public static string Concat(string prefix, object? value) => value is TupleValue
        ? throw new OperandError("TypeError", "can only concatenate str (not \"tuple\") to str")
        : value is MappingValue ? throw new OperandError("TypeError", "can only concatenate str (not \"dict\") to str")
        : VisualSourceOperands.AppendName(prefix, value);
    private static bool KeyEqual(object? a, object? b) => a is TupleValue || b is TupleValue
        ? a is TupleValue at && b is TupleValue bt && at.Items.Count == bt.Items.Count
            && at.Items.Zip(bt.Items).All(pair => ReferenceEquals(pair.First, pair.Second) || KeyEqual(pair.First, pair.Second))
        : Equal(a, b);
    private static int KeyHash(object? value)
    {
        if (value is MappingValue) throw new OperandError("TypeError", "unhashable type: 'dict'");
        if (value is not TupleValue tuple) return VisualSourceOperands.SourceKey(value).GetHashCode();
        var hash = new HashCode();
        foreach (var item in tuple.Items) hash.Add(KeyHash(item));
        return hash.ToHashCode();
    }

    internal readonly struct CounterKey(object? value) : IEquatable<CounterKey>
    {
        public object? Value { get; } = value;
        public bool Equals(CounterKey other) => ReferenceEquals(Value, other.Value) || KeyEqual(Value, other.Value);
        public override bool Equals(object? other) => other is CounterKey key && Equals(key);
        public override int GetHashCode() => KeyHash(Value);
    }
    internal sealed class Counter
    {
        private readonly Dictionary<CounterKey, BigInteger> counts = [];
        private readonly List<CounterKey> order = [];
        public void Add(object? value)
        {
            var key = new CounterKey(value);
            if (!counts.TryGetValue(key, out var count)) order.Add(key);
            counts[key] = count + BigInteger.One;
        }
        public BigInteger Count(object? value) => counts.GetValueOrDefault(new CounterKey(value));
        public IEnumerable<(object? Name, BigInteger Count)> Items => order.Select(key => (key.Value, counts[key]));
        public string Verdict => Count("FAIL") != 0 ? "FAIL" : Count("Unavailable") != 0 ? "Unavailable" : "PASS";
        public async ValueTask<bool> Matches(object? candidate, ReportIntegrityOperands operands)
        {
            if (candidate is null || !Truth(At(candidate, "dictionary"))) return false;
            var entries = (List<object?>)At(candidate, "entries")!;
            if (entries.Count != counts.Count) return false;
            foreach (var entry in entries)
            {
                var pair = (List<object?>)entry!;
                if (!counts.TryGetValue(new CounterKey(pair[0]), out var value) || !await operands.Same(pair[1], value)) return false;
            }
            return true;
        }
    }
}

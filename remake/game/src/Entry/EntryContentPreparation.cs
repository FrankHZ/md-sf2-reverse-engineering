using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Entry;

// One host-lifetime admitted definition, never a slot/session cache. All callers share
// the same serial reader; cancellation of a UI consumer cannot publish or cancel another's work.
internal sealed class EntryContentPreparation : IDisposable
{
    private readonly object _gate = new();
    private ScenarioReadResult? _result;
    private FileStamp[]? _files;
    private string[]? _selection;
    private int _disposed;
    internal bool IsDisposed => Volatile.Read(ref _disposed) != 0;
    internal bool HasResult => _result is not null;
    private sealed record FileStamp(string Path, long Length, long Modified);

    internal ScenarioReadResult Read(string[] paths, Func<ScenarioReadResult> read)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed != 0, this);
            var before = Snapshot(paths);
            if (_result is not null && _selection!.SequenceEqual(paths) && _files!.SequenceEqual(before)) return _result;
            _result = null; _files = null; _selection = null;
            var result = read();
            ObjectDisposedException.ThrowIf(_disposed != 0, this);
            if (!before.SequenceEqual(Snapshot(paths))) throw new ArgumentException("save-content-changed");
            // A transient admission failure (for example an exclusive file lock) must
            // remain retryable even when file metadata has not changed.
            if (result is ExplorationReadAccepted)
            { _files = before; _selection = paths; _result = result; }
            if (_disposed != 0) { _result = null; throw new OperationCanceledException(); }
            return result;
        }
    }

    // New Game still owns full party/name/seed initialization. Serialize that independent
    // admission with an in-flight entry read; it is not a restored or pre-mounted session.
    internal T Exclusive<T>(Func<T> work)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed != 0, this);
            return work();
        }
    }

    private static FileStamp[] Snapshot(string[] paths)
    {
        try { return Inspect(paths); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        { throw new ArgumentException("save-content-unavailable", error); }
    }

    private static FileStamp[] Inspect(string[] paths)
    {
        var selected = paths.Where(path => !string.IsNullOrWhiteSpace(path)).Select(Path.GetFullPath).ToHashSet(StringComparer.Ordinal);
        // Encounter JSON, HD terrain and external PCM are relative to the selected world.
        // Conservatively invalidate on any package-file change, including additions/removals.
        if (paths[0].Length > 0 && Path.GetDirectoryName(Path.GetFullPath(paths[0])) is { } directory && Directory.Exists(directory))
            foreach (var file in Directory.EnumerateFiles(directory, "*", new EnumerationOptions
            { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.ReparsePoint, IgnoreInaccessible = false }))
                selected.Add(file);
        return selected.Order(StringComparer.Ordinal).Select(path =>
        {
            var file = new FileInfo(path);
            return new FileStamp(path, file.Exists ? file.Length : -1, file.Exists ? file.LastWriteTimeUtc.Ticks : 0);
        }).ToArray();
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _disposed, 1);
        _result = null;
    }
}

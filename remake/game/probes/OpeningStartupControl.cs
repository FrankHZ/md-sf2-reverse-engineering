using System.Reflection;
using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.GodotAdapter;

// Controlled verification operands only. The ordinary executable has no seed override.
public partial class OpeningStartupControl : Node
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private object? _retiredOwner;
    private GameRoot? _entryHost;
    private System.IO.FileStream? _entryInputLock;
    public void HoldEntryInput()
    {
        string path = System.IO.Path.GetFullPath(System.Environment.GetEnvironmentVariable("SF2_PRIVATE_STATIC_DATA")!);
        string local = System.IO.Path.GetFullPath(ProjectSettings.GlobalizePath("res://../../local/"));
        if (!path.StartsWith(local, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Disposable local input required");
        _entryInputLock = new(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None);
    }
    public void ReleaseEntryInput() { _entryInputLock?.Dispose(); _entryInputLock = null; }
    public void RetainEntryHost(Node host) => _entryHost = (GameRoot)host;
    // Only managed observation fields are read after the native node has left the tree.
    public string RetiredEntryPreparation() => _entryHost!.ReadEntryPreparationObservationJson();
    public void RetainOwner(Node host) => _retiredOwner = typeof(GameRoot).GetField("_opening", Private)!.GetValue(host);
    public void DeliverRetiredCompletion(Node host)
    {
        if (_retiredOwner is null) throw new InvalidOperationException("No retained opening owner");
        typeof(GameRoot).GetMethod("FinishOpening", Private)!.Invoke(host, [_retiredOwner, "completed", null]);
    }
    public bool RetiredLoadCompleted() => _retiredOwner?.GetType().GetField("Loading", Private)!.GetValue(_retiredOwner)
        is not Task task || task.IsCompleted;
    public string CaptureSettledField(Node host)
    {
        var session = (GameSession)typeof(GameRoot).GetField("_session", Private)!.GetValue(host)!;
        var state = session.Current;
        var capture = session.CaptureFieldSnapshot(state.SessionId, state.Revision);
        if (capture.Data is null) throw new InvalidOperationException(capture.Error);
        return System.Text.Encoding.UTF8.GetString(OpeningSaveJson.WriteRecord(
            new FieldSnapshotRecord(1, 1, DateTimeOffset.UnixEpoch, capture.Data)));
    }
    public void SetNewGameSeeds(Node host)
    {
        if (host.FindChild("NameEntryView", true, false) is null)
            throw new InvalidOperationException("Set controlled seeds at the ordinary naming boundary");
        typeof(GameRoot).GetField("_newMainSeed", Private)!.SetValue(host, (uint?)0x12345678);
        typeof(GameRoot).GetField("_newThinkingSeed", Private)!.SetValue(host, (uint?)0x9abcdef0);
    }
}

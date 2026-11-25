using System.Diagnostics;
using Sf2.Remake.GodotAdapter.Entry;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private readonly EntryContentPreparation _entryPreparation = new();
    private sealed class EntrySource(Func<ScenarioReadResult> read) : IScenarioSource
    { public ScenarioReadResult Read() => read(); }

    private IScenarioSource RestoreSource()
    {
        string[] paths = [Selected("SF2_PRIVATE_EXPLORATION_CONTENT"), Selected("SF2_PRIVATE_BATTLE01_DATA"),
            Selected("SF2_PRIVATE_BATTLE01_SCENE"), Selected("SF2_PRIVATE_BATTLE01_TERRAIN"), Selected("SF2_PRIVATE_STATIC_DATA"),
            Selected("SF2_PRIVATE_ENEMY_DATA"), Selected("SF2_PRIVATE_ENEMY_GOLD"), Selected("SF2_PRIVATE_BATTLE_SCENE_CONTENT")];
        return new EntrySource(() => _entryPreparation.Read(paths, () => MeasureEntryRead(() =>
            new OpeningRestoreReader(paths[0], paths[1], paths[2], paths[3], paths[4], paths[5], paths[6],
                paths[7].Length == 0 ? null : paths[7]).Read())));
    }

    private void StartEntryPreparation()
    {
        var source = RestoreSource();
        var pending = Task.Run(source.Read);
        // Detached work may finish after quit. It never touches Godot or publishes a session.
        _ = pending.ContinueWith(task => { _ = task.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private int _entryReadCount;
    private long _entryReadTicks;
    private int _entryReadsCompleted;
    private ScenarioReadResult MeasureEntryRead(Func<ScenarioReadResult> read)
    {
        Interlocked.Increment(ref _entryReadCount);
        var timer = Stopwatch.StartNew();
        try { return read(); }
        finally { Interlocked.Add(ref _entryReadTicks, timer.ElapsedTicks); Interlocked.Increment(ref _entryReadsCompleted); }
    }
    public string ReadEntryPreparationObservationJson()
    {
        using var process = Process.GetCurrentProcess();
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            retired = _entryPreparation.IsDisposed, retained = _entryPreparation.HasResult,
            reads = Volatile.Read(ref _entryReadCount), completed = Volatile.Read(ref _entryReadsCompleted),
            readMilliseconds = Interlocked.Read(ref _entryReadTicks) * 1000.0 / Stopwatch.Frequency,
            managedBytes = GC.GetTotalMemory(false), peakWorkingSet = process.PeakWorkingSet64,
            gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2)
        });
    }
}

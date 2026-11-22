using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Persistence;

using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Content.Persistence;

// Closed disk variants. Field v3 protects scene-bearing writes from older v1 readers.
public abstract record SaveRecord(int FormatVersion, int Slot, long Sequence, DateTimeOffset SavedAt);
public sealed record OpeningSaveRecord(int FormatVersion, int Slot, long Sequence, DateTimeOffset SavedAt, OpeningSaveData Data)
    : SaveRecord(FormatVersion, Slot, Sequence, SavedAt);
public sealed record FieldSnapshotRecord(int Slot, long Sequence, DateTimeOffset SavedAt, FieldSnapshotData Data)
    : SaveRecord(CurrentVersion, Slot, Sequence, SavedAt)
{
    public const int CurrentVersion = 6;
}
public sealed record BattleSaveRecord(int Slot, long Sequence, DateTimeOffset SavedAt, BattleSaveData Data)
    : SaveRecord(Data.Battle.Actors.Any(a => (a.Status & BattleStatCounters.Mask) != 0 || a.LastRefreshCounters.HasValue) ? 5 : Data.HasClassTerms ? 4 : 2, Slot, Sequence, SavedAt);

public static class SaveRecordRestore
{
    public static SessionStartOutcome Restore(ScenarioDefinition definition, SaveRecord record, SessionRules rules) => record switch
    {
        OpeningSaveRecord field when field.FormatVersion is 1 or 3 => GameSession.Restore(definition, field.Data, rules),
        FieldSnapshotRecord field => GameSession.RestoreField(definition, field.Data, rules),
        BattleSaveRecord battle when battle.FormatVersion is 2 or 4 or 5 => GameSession.RestoreBattle(definition, battle.Data, rules),
        _ => new SessionStartFailed(new(SessionFailureKind.ContentError, "save-version", "save", "Unsupported save version.")),
    };

    // The source is configured by the caller; payloads never select files or initialize a new party.
    public static SessionStartOutcome Restore(IScenarioSource source, SaveRecord record, SessionRules rules) => source.Read() switch
    {
        ExplorationReadAccepted admitted => Restore(admitted.Definition, record, rules),
        ScenarioReadRejected rejected => new SessionStartFailed(rejected.Failure),
        _ => new SessionStartFailed(new(SessionFailureKind.ContentError, "save-content-required", "save", "Restore requires configured content.")),
    };
}

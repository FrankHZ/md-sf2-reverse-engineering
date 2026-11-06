using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Persistence;

namespace Sf2.Remake.Content.Persistence;

// Closed disk variants. Version1 retains its exact field payload and encoding.
public abstract record SaveRecord(int FormatVersion, int Slot, long Sequence, DateTimeOffset SavedAt);
public sealed record OpeningSaveRecord(int FormatVersion, int Slot, long Sequence, DateTimeOffset SavedAt, OpeningSaveData Data)
    : SaveRecord(FormatVersion, Slot, Sequence, SavedAt);
public sealed record BattleSaveRecord(int Slot, long Sequence, DateTimeOffset SavedAt, BattleSaveData Data)
    : SaveRecord(2, Slot, Sequence, SavedAt);

public static class SaveRecordRestore
{
    public static SessionStartOutcome Restore(ScenarioDefinition definition, SaveRecord record, SessionRules rules) => record switch
    {
        OpeningSaveRecord field when field.FormatVersion == 1 => GameSession.Restore(definition, field.Data, rules),
        BattleSaveRecord battle when battle.FormatVersion == 2 => GameSession.RestoreBattle(definition, battle.Data, rules),
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

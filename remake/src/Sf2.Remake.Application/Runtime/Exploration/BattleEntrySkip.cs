using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record BattleEntrySkipState(int Operations = 0);

// Delivery policy only. ProgramRunner and the ordinary wait reducers retain interpretation/service.
public static class BattleEntrySkip
{
    public const int BatchOperations = 32;
    public const int RequestOperations = 32768;
    public static bool Eligible(SessionSnapshot current) => current.Story.EnteringBattle is not null &&
        current.Story.Continuation is (ProgramContinuation.BeforeBattleFinished or
            ProgramContinuation.BattleLoadFinished or ProgramContinuation.BattleStartFinished) &&
        current.Story.Wait is not ChoiceWait &&
        current.StopReason is not (SessionStopReason.Unsupported or SessionStopReason.Faulted);

    internal static SessionSnapshot Disarm(SessionSnapshot current, List<SessionObservation> observations, string reason) =>
        current.Story.EntrySkip is null ? current : ProgramRunner.Stop(ProgramRunner.Commit(current, current.Active,
            current.Story.Copy(current.Story.Cursor, current.Story.Wait, clearEntrySkip: true), observations,
            "battle-entry-skip-stopped", reason), current.StopReason);

    internal static SessionResult Settle(SessionResult result)
    {
        if (result.Snapshot.Story.EntrySkip is null || (result.Failure is null or { Kind: SessionFailureKind.IllegalCommand }) && Eligible(result.Snapshot)) return result;
        var observations = result.Observations.ToList();
        return result with { Snapshot = Disarm(result.Snapshot, observations, result.Failure?.Code ?? "entry-boundary"),
            Observations = observations.AsReadOnly() };
    }

    internal static SessionResult Progress(SessionSnapshot before, SessionResult result)
    {
        result = Settle(result);
        if (result.Failure is not null || before.Story.EntrySkip is null || result.Snapshot.Revision == before.Revision ||
            result.Snapshot.Story.EntrySkip is not { } skip) return result;
        var current = result.Snapshot;
        var observations = result.Observations.ToList();
        current = skip.Operations + 1 >= RequestOperations ? Disarm(current, observations, "operation-budget") :
            current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait,
                entrySkip: skip with { Operations = skip.Operations + 1 }));
        return result with { Snapshot = current, Observations = observations.AsReadOnly() };
    }

    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current,
        BattleEntrySkipCommand command, SessionRules rules)
    {
        if (command is CancelBattleEntrySkip && current.Story.EntrySkip is not null)
        {
            List<SessionObservation> stopped = [];
            return ProgramRunner.Result(Disarm(current, stopped, "cancelled"), stopped);
        }
        if (!Eligible(current)) return BattleCommandDispatcher.Reject(current, "battle-entry-skip-unavailable", "command");
        if (command is SkipBattleEntry)
        {
            if (current.Story.EntrySkip is not null)
                return BattleCommandDispatcher.Reject(current, "battle-entry-skip-active", "command");
            List<SessionObservation> started = [];
            return ProgramRunner.Result(ProgramRunner.Stop(ProgramRunner.Commit(current, current.Active,
                current.Story.Copy(current.Story.Cursor, current.Story.Wait, entrySkip: new()), started,
                "battle-entry-skip-requested"), current.StopReason), started);
        }
        if (current.Story.EntrySkip is null)
            return BattleCommandDispatcher.Reject(current, "battle-entry-skip-inactive", "command");
        if (command is CompleteSkippedEntryPresentation presentation)
        {
            if (presentation.Kind == PresentationCueKind.BattleLoad)
                return BattleCommandDispatcher.Reject(current, "battle-load-requires-mount", "presentation");
            return Progress(current, ExplorationDispatcher.Submit(definition, current,
                new CompletePresentation(presentation.Wait, presentation.Kind), rules, skippedDelivery: true));
        }
        if (command is CompleteSkippedEntryMusic music)
            return Progress(current, ExplorationDispatcher.Submit(definition, current,
                new CompleteMusic(music.Generation, music.Cue), rules, skippedDelivery: true));
        if (command is not AdvanceBattleEntrySkip)
            return BattleCommandDispatcher.Reject(current, "battle-entry-skip-command", "command");

        List<SessionObservation> observations = [];
        List<ProgramControlRead> reads = [];
        for (int index = 0; index < BatchOperations && current.Story.EntrySkip is not null && Eligible(current); index++)
        {
            // Readiness may be requested by a background entity while another wait owns control.
            if (current.Exploration?.AllEntities.Any(entity => entity.WaitingForSprite && entity.SpriteReady != entity.SpriteRequest) == true) break;
            SessionCommand? next = current.Story.Wait switch
            {
                PresentationWait or EntitySpriteWait or EntitySetSpriteWait => null,
                NodWait { LogicalDone: true } or FullFadeWait { LogicalDone: true } or MusicWait { LogicalDone: true } => null,
                FieldTextWait { Revealed: false, LogicalDone: true } text => new CompleteTextReveal(text.Token),
                FieldTextWait { LogicalDone: true } text when current.CanWaitForText => new Acknowledge(text.Token),
                FieldTextWait { LogicalDone: true } => null,
                W1TextWait { Revealed: false } text => new CompleteTextReveal(text.Token),
                W1TextWait { AtInput: true } text => new Acknowledge(text.Token),
                DialogueWait text => new Acknowledge(text.Token),
                _ => new AdvanceSimulation(current.Story.Wait?.Token),
            };
            if (next is null) break;
            var result = Progress(current, ExplorationDispatcher.Submit(definition, current, next, rules, skippedDelivery: true));
            current = result.Snapshot;
            observations.AddRange(result.Observations);
            reads.AddRange(result.ProgramControlReads ?? []);
            if (result.Failure is not null)
                return result with { Snapshot = Disarm(current, observations, result.Failure.Code),
                    Observations = observations.AsReadOnly(), ProgramControlReads = reads.AsReadOnly() };
        }
        return ProgramRunner.Result(current, observations) with { ProgramControlReads = reads.AsReadOnly() };
    }
}

using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Runtime.Persistence;

namespace Sf2.Remake.Application.Runtime;

public sealed class GameSession
{
    private SessionSnapshot _current;
    private GameSession(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules) { Definition = definition; _current = current; Rules = rules; }
    public ScenarioDefinition Definition { get; }
    public SessionSnapshot Current => _current;
    public SessionRules Rules { get; }
    public SaveAvailability QuerySave() => OpeningSave.Query(Definition, Current, Rules);
    public SaveCapture CaptureSave(Guid session, long revision) => OpeningSave.Capture(Definition, Current, Rules, session, revision);

    public static SessionStartOutcome Restore(IScenarioSource definitions, OpeningSaveData data, SessionRules? rules = null)
    {
        var admitted = definitions.Read();
        if (admitted is ScenarioReadRejected rejected) return new SessionStartFailed(rejected.Failure);
        if (admitted is not ExplorationReadAccepted accepted)
            return new SessionStartFailed(new(SessionFailureKind.ContentError, "save-content-required", "save", "Restore requires opening content."));
        return Restore(accepted.Definition, data, rules ?? RuleCompositions.Sf2());
    }

    public static SessionStartOutcome Restore(ScenarioDefinition definition, OpeningSaveData data, SessionRules rules)
    {
        try
        {
            var result = OpeningRestore.Build(definition, data, rules);
            return new SessionStarted(new GameSession(definition, result.Snapshot, rules), result);
        }
        catch (BattleRuleException error)
        {
            return new SessionStartFailed(new(SessionFailureKind.ContentError, error.Code, error.Field, "Cannot restore this save: " + error.Code));
        }
    }
    public EntityRef? QueryExplorationInteraction() => ExplorationInteractionQuery.Query(Current);
    public PartyDetails? QueryPartyDetails() => PartyDetailsQuery.Query(Definition, Current);
    public BattleChoicesSnapshot QueryBattleChoices() =>
        BattleChoices.Query(Current, Rules, ActiveBattleScenes, Definition.PrivateDefinitions is not null);

    public BattleMovementRange? QueryBattleMovementRange() => BattleMovementQuery.Query(Current);

    public BattleSceneDefinition? ActiveBattleScenes => Current.Mode == SessionMode.Battle
        ? Definition.ScenesFor(Current.Battle.Definition.Encounter) : null;

    public static SessionStartOutcome Start(IScenarioSource source) => Start(source, RuleCompositions.Sf2());

    public static SessionStartOutcome Start(IScenarioSource source, SessionRules rules)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(rules);
        // Read/admit once: external input is not a second authority consulted during play.
        return source.Read() switch
        {
            ScenarioReadRejected rejected => new SessionStartFailed(rejected.Failure),
            ScenarioReadAccepted accepted => Start(accepted.Definition, accepted.Start, rules),
            ExplorationReadAccepted accepted => Start(accepted.Definition, accepted.Start, rules),
            _ => throw new InvalidOperationException("Unknown content admission result."),
        };
    }

    public static SessionStartOutcome Start(ScenarioDefinition definition, BattleStartInput start) => Start(definition, start, RuleCompositions.Sf2());

    public static SessionStartOutcome Start(ScenarioDefinition definition, BattleStartInput start, SessionRules rules)
    {
        ArgumentNullException.ThrowIfNull(definition); ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(start);
        if (start.Encounter is null || !definition.Encounters.TryGetValue(start.Encounter, out var encounter))
            return new SessionStartFailed(new(SessionFailureKind.ContentError, "missing-encounter", "start.encounter", "missing encounter"));
        try
        {
            definition.BattleStory?.RequireStart(encounter, start);
            rules = rules.Bind(definition.Encounters.Values, start.Actors);
            var result = ContinueStory(definition, BattleAdvancer.Start(encounter, start, rules), rules);
            if (encounter.Poison && result.Failure is null) result = BattleOutcome.Begin(definition, result, rules);
            return new SessionStarted(new GameSession(definition, result.Snapshot, rules), result);
        }
        catch (BattleDecisionRuleFault error)
        {
            return new SessionStartFailed(new(SessionFailureKind.InvariantFailure, "decision-rule-failure",
                "rules.decisions", $"{error.Identity}: {error.Operation}"));
        }
        catch (BattleRuleException error)
        {
            return new SessionStartFailed(new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
                error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    public static SessionStartOutcome Start(ScenarioDefinition definition, ExplorationStartInput start) => Start(definition, start, RuleCompositions.Sf2());

    public static SessionStartOutcome Start(ScenarioDefinition definition, ExplorationStartInput start, SessionRules rules)
    {
        ArgumentNullException.ThrowIfNull(definition); ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(start);
        try
        {
            rules = rules.Bind(definition.Encounters.Values, start.Party.Actors);
            var result = ExplorationDispatcher.Start(definition, start, rules);
            return new SessionStarted(new GameSession(definition, result.Snapshot, rules), result);
        }
        catch (BattleDecisionRuleFault error)
        {
            return new SessionStartFailed(new(SessionFailureKind.InvariantFailure, "decision-rule-failure",
                "rules.decisions", $"{error.Identity}: {error.Operation}"));
        }
        catch (BattleRuleException error)
        {
            return new SessionStartFailed(new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
                error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    private static SessionResult ContinueStory(ScenarioDefinition definition, SessionResult result, SessionRules rules)
    {
        if (result.Failure is not null || result.Snapshot.BattleRound is not { Program: true } || result.Snapshot.Story.Wait is not null)
            return result;
        return ProgramRunner.Run(definition, result.Snapshot, result.Observations.ToList(), rules);
    }

    public SessionResult Submit(CommandEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        var current = Current;
        if (envelope.SessionId != current.SessionId || envelope.ExpectedRevision != current.Revision)
            return BattleCommandDispatcher.Reject(current, "stale-input", "envelope");
        if (current.BattleResult is not null) return BattleCommandDispatcher.Reject(current, "battle-ended", "command");
        if (envelope.Command is null) return BattleCommandDispatcher.Reject(current, "missing-command", "command");
        if (envelope.Command is ObserveFieldMusic music)
        {
            var observed = OpeningSave.ObserveMusic(Definition, current, music);
            _current = observed.Snapshot;
            return observed;
        }
        if (envelope.Command is not AdvanceSimulation && envelope.Actor != current.Selection?.Actor)
            return BattleCommandDispatcher.Reject(current, "wrong-actor", "actor");
        if (envelope.Command is BattleEntrySkipCommand skip)
        {
            var skipped = BattleEntrySkip.Submit(Definition, current, skip, Rules);
            _current = skipped.Snapshot;
            return skipped;
        }
        bool discardActive = current.BattleDiscard is not null;
        bool discardInput = discardActive || envelope.Command is BeginDiscard or SelectDiscardItem ||
            current.Selection is { } selected && BattleDiscardSelection.Active(selected.Stage);
        bool giveActive = current.BattleGive is not null;
        bool roundActive = current.BattleRound is { Program: false };
        bool retreatActive = current.BattleRetreat is not null;
        bool sceneActive = current.BattleScene is not null;
        bool movementActive = current.BattleMovement is not null;
        bool programActive = !discardActive && !giveActive && !roundActive && !retreatActive && !sceneActive && !movementActive && !current.HasBattleControl;
        if (envelope.Command is WaitAtInput && (!programActive || current.Exploration is null))
            return BattleCommandDispatcher.Reject(current, "field-input-unavailable", "command");
        if (envelope.Command is WaitForText && (!programActive || current.Exploration is null))
            return BattleCommandDispatcher.Reject(current, "text-input-unavailable", "command");
        if (!programActive && !retreatActive && envelope.Command is AdvanceSimulation tick &&
            (tick.Ticks != 1 || (current.BattleScene is { Healing: not null } healingScene
                ? tick.Wait != healingScene.Token : tick.Wait is not null)))
            return BattleCommandDispatcher.Reject(current, "invalid-battle-tick", "command");
        var result = discardActive ? BattleDiscardContinuation.Submit(current, envelope.Command)
            : giveActive ? BattleGiveContinuation.Submit(current, envelope.Command, Rules)
            : roundActive ? BattleRoundContinuation.Submit(current, envelope.Command, Rules)
            : retreatActive ? BattleRetreatContinuation.Submit(Definition, current, envelope.Command, Rules)
            : movementActive ? BattleMovementContinuation.Submit(current, envelope.Command, Rules)
            : sceneActive ? BattleSceneContinuation.Submit(current, envelope.Command, Rules)
            : programActive ? ExplorationDispatcher.Submit(Definition, current, envelope.Command, Rules)
            : BattleCommandDispatcher.Submit(current, envelope.Command, Rules, ActiveBattleScenes, Definition.PrivateDefinitions is not null);
        result = ContinueStory(Definition, result, Rules);
        if (result.Failure is null && !discardInput && !programActive && !retreatActive && result.Snapshot.BattleGive is null) result = BattleOutcome.Begin(Definition, result, Rules);
        result = BattleEntrySkip.Progress(current, result);
        _current = result.Snapshot;
        return result;
    }
}

using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime;

public sealed class GameSession
{
    private SessionSnapshot _current;
    private GameSession(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules) { Definition = definition; _current = current; Rules = rules; }
    public ScenarioDefinition Definition { get; }
    public SessionSnapshot Current => _current;
    public SessionRules Rules { get; }
    public BattleChoicesSnapshot QueryBattleChoices() =>
        BattleChoices.Query(Current, Rules, Definition.BattleScenes, Definition.PrivateDefinitions is not null);

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
            var result = BattleAdvancer.Start(encounter, start);
            return new SessionStarted(new GameSession(definition, result.Snapshot, rules), result);
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
            var result = ExplorationDispatcher.Start(definition, start);
            return new SessionStarted(new GameSession(definition, result.Snapshot, rules), result);
        }
        catch (BattleRuleException error)
        {
            return new SessionStartFailed(new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
                error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    public SessionResult Submit(CommandEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        var current = Current;
        if (envelope.SessionId != current.SessionId || envelope.ExpectedRevision != current.Revision)
            return BattleCommandDispatcher.Reject(current, "stale-input", "envelope");
        if (envelope.Command is null) return BattleCommandDispatcher.Reject(current, "missing-command", "command");
        if (envelope.Command is not AdvanceSimulation && envelope.Actor != current.Selection?.Actor)
            return BattleCommandDispatcher.Reject(current, "wrong-actor", "actor");
        bool sceneActive = current.BattleScene is not null;
        bool movementActive = current.BattleMovement is not null;
        bool programActive = !sceneActive && !movementActive && !current.HasBattleControl;
        if (envelope.Command is WaitAtInput && (!programActive || current.Exploration is null))
            return BattleCommandDispatcher.Reject(current, "field-input-unavailable", "command");
        if (envelope.Command is WaitForText && (!programActive || current.Exploration is null))
            return BattleCommandDispatcher.Reject(current, "text-input-unavailable", "command");
        if (!programActive && envelope.Command is AdvanceSimulation tick &&
            (tick.Ticks != 1 || (current.BattleScene is { Healing: not null } healingScene
                ? tick.Wait != healingScene.Token : tick.Wait is not null)))
            return BattleCommandDispatcher.Reject(current, "invalid-battle-tick", "command");
        var result = movementActive ? BattleMovementContinuation.Submit(current, envelope.Command)
            : sceneActive ? BattleSceneContinuation.Submit(current, envelope.Command)
            : programActive ? ExplorationDispatcher.Submit(Definition, current, envelope.Command)
            : BattleCommandDispatcher.Submit(current, envelope.Command, Rules, Definition.BattleScenes, Definition.PrivateDefinitions is not null);
        if (!programActive && !ReferenceEquals(result.Snapshot, current))
            result = result with { Snapshot = result.Snapshot.WithStory(current.Story) };
        if (result.Failure is null && !programActive) result = BattleOutcome.Begin(Definition, result);
        _current = result.Snapshot;
        return result;
    }
}

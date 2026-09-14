using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime;

public sealed class GameSession
{
    private SessionSnapshot _current;
    private GameSession(ScenarioDefinition definition, SessionSnapshot current) { Definition = definition; _current = current; }
    public ScenarioDefinition Definition { get; }
    public SessionSnapshot Current => _current;

    public static SessionStartOutcome Start(IScenarioSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        // Read/admit once: external input is not a second authority consulted during play.
        return source.Read() switch
        {
            ScenarioReadRejected rejected => new SessionStartFailed(rejected.Failure),
            ScenarioReadAccepted accepted => Start(accepted.Definition, accepted.Start),
            ExplorationReadAccepted accepted => Start(accepted.Definition, accepted.Start),
            _ => throw new InvalidOperationException("Unknown content admission result."),
        };
    }

    public static SessionStartOutcome Start(ScenarioDefinition definition, BattleStartInput start)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(start);
        if (start.Encounter is null || !definition.Encounters.TryGetValue(start.Encounter, out var encounter))
            return new SessionStartFailed(new(SessionFailureKind.ContentError, "missing-encounter", "start.encounter", "missing encounter"));
        try
        {
            var result = BattleAdvancer.Start(encounter, start);
            return new SessionStarted(new GameSession(definition, result.Snapshot), result);
        }
        catch (BattleRuleException error)
        {
            return new SessionStartFailed(new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
                error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    public static SessionStartOutcome Start(ScenarioDefinition definition, ExplorationStartInput start)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(start);
        try
        {
            var result = ExplorationDispatcher.Start(definition, start);
            return new SessionStarted(new GameSession(definition, result.Snapshot), result);
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
        bool programActive = current.Mode == SessionMode.Exploration || current.Story.Cursor is not null || current.Story.Wait is not null;
        if (!programActive && envelope.Command is AdvanceSimulation { Wait: not null } or AdvanceSimulation { Ticks: not 1 })
            return BattleCommandDispatcher.Reject(current, "invalid-battle-tick", "command");
        var result = programActive ? ExplorationDispatcher.Submit(Definition, current, envelope.Command)
            : BattleCommandDispatcher.Submit(current, envelope.Command);
        if (!programActive && !ReferenceEquals(result.Snapshot, current))
            result = result with { Snapshot = result.Snapshot.WithStory(current.Story) };
        _current = result.Snapshot;
        return result;
    }
}

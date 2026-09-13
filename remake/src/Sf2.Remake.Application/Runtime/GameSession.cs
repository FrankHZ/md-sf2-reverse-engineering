using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime;

public sealed class GameSession
{
    private SessionSnapshot _current;
    private GameSession(SessionSnapshot current) { _current = current; }
    public SessionSnapshot Current => _current;

    public static SessionStartOutcome Start(IScenarioSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        // Read/admit once: external input is not a second authority consulted during play.
        return source.Read() switch
        {
            ScenarioReadRejected rejected => new SessionStartFailed(rejected.Failure),
            ScenarioReadAccepted accepted => Start(accepted.Definition, accepted.Start),
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
            return new SessionStarted(new GameSession(result.Snapshot), result);
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
        var result = BattleCommandDispatcher.Submit(current, envelope.Command);
        _current = result.Snapshot;
        return result;
    }
}

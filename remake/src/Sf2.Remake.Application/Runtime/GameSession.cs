using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;

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
            ScenarioReadAccepted accepted => StartCommon(accepted.Definition),
            _ => throw new InvalidOperationException("Unknown content admission result."),
        };
    }

    private static SessionStarted StartCommon(ScenarioDefinition definition)
    {
        var result = BattleAdvancer.Start(definition);
        return new(new GameSession(result.Snapshot), result);
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

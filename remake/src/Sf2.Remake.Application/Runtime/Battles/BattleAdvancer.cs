using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

internal static class BattleAdvancer
{
    internal static SessionResult Start(BattleDefinition definition, BattleStartInput start) => Advance(new(
        Guid.NewGuid(), 0, 0, BattleTurnFlow.Start(definition, start),
        null, SessionStopReason.SimulationWait), []);

    internal static SessionResult Advance(SessionSnapshot current, List<SessionObservation> observations)
    {
        var battle = current.Battle;
        long revision = current.Revision, sequence = current.ObservationSequence;
        // A bounded host tick yields real automatic work; the next frame resumes through the same facade.
        for (int steps = 0; steps < 256; steps++)
        {
            if (BattleTurnFlow.AtRoundEnd(battle))
            {
                var previous = battle;
                battle = BattleTurnFlow.GenerateRound(battle);
                revision++;
                observations.Add(new(++sequence, revision, "round-started", Before: previous.Round, After: battle.Round));
                observations.Add(new(++sequence, revision, "round-rng", Before: previous.MainSeed, After: battle.MainSeed));
                continue;
            }
            var actor = BattleTurnFlow.QueuedActor(battle);
            if (actor.Hp == 0)
            {
                battle = BattleTurnFlow.ConsumeEntry(battle); revision++;
                observations.Add(new(++sequence, revision, "dead-entry-skipped", actor.Actor));
                continue;
            }
            if (actor.Definition.Controller == BattleController.Player)
            {
                revision++;
                observations.Add(new(++sequence, revision, "player-control", actor.Actor));
                var selection = new BattleSelection(actor.Actor, BattleMovement.Preview(battle, actor.Actor, actor.Position!), BattleSelectionStage.Movement);
                var snapshot = new SessionSnapshot(current.SessionId, revision, sequence, battle, selection, SessionStopReason.PlayerInput);
                return new(snapshot, observations.AsReadOnly(), SessionStopReason.PlayerInput);
            }
            if (actor.Definition.Controller == BattleController.Commandset06Script3)
            {
                var beforeAction = new SessionSnapshot(current.SessionId, revision, sequence, battle, null, SessionStopReason.SimulationWait);
                try
                {
                    var action = EnemyCommandset06.Resolve(battle, actor.Actor);
                    var committed = BattleActionCommitter.Publish(beforeAction, action.Battle, actor.Actor,
                        action.Destination, action.Effects, observations);
                    battle = committed.Battle; revision = committed.Revision; sequence = committed.ObservationSequence;
                }
                catch (BattleRuleException error)
                {
                    // Earlier committed player/AI work remains published; this enemy ACTION
                    // keeps its queue entry, position, HP, rewards, last target and both seeds.
                    var reason = error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Faulted;
                    return new(new(current.SessionId, revision, sequence, battle, null, reason), observations.AsReadOnly(), reason,
                        new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.InvariantFailure,
                            error.Code, error.Field, error.Code.Replace('-', ' ')));
                }
                continue;
            }
            battle = BattleTurnFlow.ConsumeEntry(battle); revision++;
            observations.Add(new(++sequence, revision, "ai-stay", actor.Actor));
        }
        return new(new(current.SessionId, revision, sequence, battle, null, SessionStopReason.SimulationWait),
            observations.AsReadOnly(), SessionStopReason.SimulationWait);
    }
}

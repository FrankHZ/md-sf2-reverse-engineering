using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

internal static class BattleAdvancer
{
    internal static SessionResult Start(BattleDefinition definition, BattleStartInput start)
    {
        var battle = BattleTurnFlow.Start(definition, start);
        List<SessionObservation> observations = [];
        if (start.NewBattle is not null) observations.Add(new(1, 0, "new-battle-initialized"));
        var result = Advance(new(Guid.NewGuid(), 0, observations.Count, battle,
            null, SessionStopReason.SimulationWait), observations);
        // A private new-battle entry is one transaction through supported initial control.
        if (start.NewBattle is not null && result.Failure is { } failure)
            throw new BattleRuleException(failure.Code, failure.Field,
                failure.Kind == SessionFailureKind.UnsupportedCapability);
        return result;
    }

    internal static SessionResult Advance(SessionSnapshot current, List<SessionObservation> observations)
    {
        if (current.BattleScene is not null)
            return new(current, observations.AsReadOnly(), SessionStopReason.PresentationWait);
        var battle = current.Battle;
        long revision = current.Revision, sequence = current.ObservationSequence;
        // A bounded host tick yields real automatic work; the next frame resumes through the same facade.
        for (int steps = 0; steps < 256; steps++)
        {
            try
            {
                if (BattleOutcomeRules.Check(battle) is not null)
                    return new(new(current.SessionId, revision, sequence, battle, null, SessionStopReason.SimulationWait),
                        observations.AsReadOnly(), SessionStopReason.SimulationWait);
                if (BattleTurnFlow.AtRoundEnd(battle))
                {
                    var previous = battle;
                    battle = BattleTurnFlow.GenerateRound(battle);
                    revision++;
                    if (battle.Regions is { } regions)
                    {
                        observations.Add(new(++sequence, revision, "regions-tested", After: regions.Tested));
                        observations.Add(new(++sequence, revision, "region-program-none"));
                        observations.Add(new(++sequence, revision, "spawn-modes-admitted"));
                    }
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
                if (actor.Control == BattleControl.Player && actor.AiStrategy is null)
                {
                    var controlled = BattleControlRules.EnterPlayer(battle, actor.Actor);
                    var preview = BattleMovement.Preview(controlled, actor.Actor, actor.Position!);
                    battle = controlled;
                    revision++;
                    observations.Add(new(++sequence, revision, "player-control", actor.Actor));
                    var selection = new BattleSelection(actor.Actor, preview, BattleSelectionStage.Movement);
                    var snapshot = new SessionSnapshot(current.SessionId, revision, sequence, battle, selection, SessionStopReason.PlayerInput);
                    return new(snapshot, observations.AsReadOnly(), SessionStopReason.PlayerInput);
                }
                if (actor.Control == BattleControl.Automatic && actor.AiStrategy == BattleAiStrategy.Stay)
                {
                    battle = BattleTurnFlow.ConsumeEntry(battle); revision++;
                    observations.Add(new(++sequence, revision, "ai-stay", actor.Actor));
                    continue;
                }
                var beforeAction = new SessionSnapshot(current.SessionId, revision, sequence, battle, null, SessionStopReason.SimulationWait);
                if (actor.Control != BattleControl.Automatic)
                    throw new BattleRuleException("control-ai", "placements.control/aiStrategy", true);
                var action = actor.AiStrategy switch
                {
                    BattleAiStrategy.SourceOrders => SourceEnemyAi.Resolve(battle, actor.Actor),
                    BattleAiStrategy.AttackThenApproach => AttackThenApproachAi.Resolve(battle, actor.Actor),
                    _ => throw new BattleRuleException("control-ai", "placements.control/aiStrategy", true),
                };
                if (action.Scene is { } scene)
                    return BattleSceneContinuation.Begin(beforeAction.WithStory(current.Story), scene, observations, action.Effects);
                var committed = BattleActionCommitter.Publish(beforeAction, action.Battle, actor.Actor,
                    action.Destination, action.Effects, observations);
                battle = committed.Battle; revision = committed.Revision; sequence = committed.ObservationSequence;
            }
            catch (BattleRuleException error)
            {
                // Keep the last committed state. A rejected round, control admission or
                // enemy action cannot publish partial changes or random draws.
                var reason = error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Faulted;
                return new(new(current.SessionId, revision, sequence, battle, null, reason), observations.AsReadOnly(), reason,
                    new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.InvariantFailure,
                        error.Code, error.Field, error.Code.Replace('-', ' ')));
            }
        }
        return new(new(current.SessionId, revision, sequence, battle, null, SessionStopReason.SimulationWait),
            observations.AsReadOnly(), SessionStopReason.SimulationWait);
    }
}

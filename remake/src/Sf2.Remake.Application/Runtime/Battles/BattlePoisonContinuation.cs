using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime.Battles;

internal sealed record PoisonTurnNotice(ActorRef Actor, ushort HpBefore);

// The second death/count pass returns here, never through action publication or its hook.
internal static class BattlePoisonContinuation
{
    internal static SessionSnapshot Begin(SessionSnapshot current, ActorRef actor, List<SessionObservation> observations)
    {
        long sequence = current.ObservationSequence;
        observations.Add(new(++sequence, current.Revision, "poison-message", actor, Detail: "307"));
        return new(current.SessionId, current.Revision, sequence,
            new ActiveBattle(current.Battle, null, new(null, BattleScenePhase.PoisonDamageMessage, 0, new(sequence),
                poisonTurn: new(actor, current.Battle.GetActor(actor).Hp))), current.Story, SessionStopReason.PresentationWait);
    }

    internal static SessionResult Complete(SessionSnapshot current, SessionCommand command, SessionRules rules)
    {
        var scene = current.BattleScene!;
        if (command is not CompletePresentation completion || completion.Wait != scene.Token || completion.Kind != scene.CompletionKind)
            return BattleCommandDispatcher.Reject(current, "battle-scene-wait", "command");
        var notice = scene.PoisonTurn!;
        var actor = current.Battle.GetActor(notice.Actor);
        long revision = current.Revision + 1, sequence = current.ObservationSequence;
        List<SessionObservation> observations = [];
        if (scene.Phase == BattleScenePhase.PoisonFinalize) return Finish(current, observations, rules);
        if (scene.Phase == BattleScenePhase.PoisonDamageMessage)
        {
            if (!PoisonTurnRules.Eligible(actor) || actor.Hp != notice.HpBefore)
                return BattleCommandDispatcher.Reject(current, "poison-turn-context", "actor");
            ushort hp = PoisonTurnRules.RemainingHp(actor.Hp);
            var battle = current.Battle.With(actors: current.Battle.Actors.Select(value =>
                value.Actor == actor.Actor ? value.With(hp: hp) : value));
            observations.Add(new(++sequence, revision, "poison-hp", actor.Actor, actor.Hp, hp));
            if (hp == 0)
            {
                observations.Add(new(++sequence, revision, "poison-message", actor.Actor, Detail: "308"));
                var exhausted = new BattleSceneState(null, BattleScenePhase.PoisonExhaustedMessage, 0, new(sequence), poisonTurn: notice);
                return new(new(current.SessionId, revision, sequence, new ActiveBattle(battle, null, exhausted), current.Story,
                    SessionStopReason.PresentationWait), observations.AsReadOnly(), SessionStopReason.PresentationWait);
            }
            observations.Add(new(++sequence, revision, "after-turn", actor.Actor));
            var finalize = new BattleSceneState(null, BattleScenePhase.PoisonFinalize, 0, new(sequence), poisonTurn: notice);
            return Finish(new(current.SessionId, revision, sequence, new ActiveBattle(battle, null, finalize), current.Story,
                SessionStopReason.SimulationWait), observations, rules);
        }
        if (scene.Phase != BattleScenePhase.PoisonExhaustedMessage || actor.Hp != 0 || actor.Status != 2 || actor.Position is null)
            return BattleCommandDispatcher.Reject(current, "poison-turn-context", "actor");
        var credit = current.Battle.DeathCredit;
        if (credit is null || credit.Ally is { } first && !current.Battle.GetActor(first).IsAlly)
            return BattleCommandDispatcher.Reject(current, "death-first-ally-context", "battle.firstAlly", true);
        observations.Add(new(++sequence, revision, "after-turn", actor.Actor));
        var pending = new SessionSnapshot(current.SessionId, revision, sequence, new ActiveBattle(current.Battle, null),
            current.Story, SessionStopReason.SimulationWait);
        var waiting = BattleFieldDeathContinuation.Begin(pending,
            new(actor.Actor, actor.Position, credit, AfterTurn: true), new([actor.Actor]), observations);
        return new(waiting, observations.AsReadOnly(), waiting.StopReason);
    }

    internal static SessionResult Finish(SessionSnapshot current, List<SessionObservation> observations, SessionRules rules)
    {
        try
        {
            var outcome = BattleOutcomeSelection.Check(rules.Outcome, current.Battle);
            long revision = current.Revision + 1, sequence = current.ObservationSequence;
            if (current.BattleScene is { IsFieldDeath: true } field)
            {
                observations.Add(new(++sequence, revision, "scene-step-completed", field.Actor, Detail: field.Phase.ToString()));
                observations.Add(new(++sequence, revision, "field-death-ended", field.Actor));
            }
            if (outcome is not null) observations.Add(new(++sequence, revision, "battle-outcome", Detail: outcome.Value.ToString()));
            var ended = new SessionSnapshot(current.SessionId, revision, sequence,
                new ActiveBattle(outcome is null ? BattleTurnFlow.ConsumeEntry(current.Battle) : current.Battle, null),
                current.Story, SessionStopReason.SimulationWait);
            return BattleAdvancer.Advance(ended, observations, rules);
        }
        // A failure after HP/cleanup publication retains that accepted prefix.
        catch (BattlePolicyFault error) { return new(current, observations.AsReadOnly(), current.StopReason, BattleChoices.Failure(error)); }
        catch (BattleRuleException error)
        {
            var failure = BattleCommandDispatcher.Reject(current, error.Code, error.Field, error.Unsupported);
            return new(current, observations.AsReadOnly(), current.StopReason, failure.Failure);
        }
    }
}

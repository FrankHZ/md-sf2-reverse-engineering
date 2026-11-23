using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime.Battles;

// Pending source command operands, not another actor/turn state authority.
internal sealed record StunTurnNotice(ActorRef Actor, ushort Before, ushort After);
internal sealed record SleepTurnNotice(ActorRef Actor, ushort Before, ushort After, int Text);
internal sealed record CounterExpiryNotice(ActorRef Actor, ushort Before, ushort After);

internal static class BattleAfterTurnContinuation
{
    internal static SessionSnapshot Begin(SessionSnapshot current, ActorRef actor, List<SessionObservation> observations)
    {
        var unit = current.Battle.GetActor(actor);
        if ((unit.Status & 1) == 0) return CheckSleep(current, actor, observations);
        var draw = BattleRandom.NextMain(current.Battle.MainSeed, 2);
        bool cleared = draw.Value == 0;
        var notice = new StunTurnNotice(actor, unit.Status, cleared ? (ushort)(unit.Status & ~1) : unit.Status);
        long sequence = current.ObservationSequence, revision = current.Revision + 1;
        observations.Add(new(++sequence, revision, "rng-stun-recovery", actor, draw.Before, draw.After,
            RandomRange: draw.Range, RandomValue: draw.Value));
        var scene = new BattleSceneState(null, BattleScenePhase.StunTurnMessage, 0, new(sequence), stunTurn: notice);
        return new(current.SessionId, revision, sequence, new ActiveBattle(current.Battle.With(mainSeed: draw.After), null, scene),
            current.Story, SessionStopReason.PresentationWait);
    }

    private static SessionSnapshot CheckSleep(SessionSnapshot current, ActorRef actor, List<SessionObservation> observations)
    {
        if ((current.Battle.GetActor(actor).Status & 0xC0) == 0) return AfterSleep(current, actor, observations);
        var proposal = SleepTurnRules.Prepare(current.Battle.GetActor(actor), current.Battle.MainSeed)
            ?? throw new BattleRuleException("sleep-turn-context", "actor.status");
        long revision = current.Revision + 1, sequence = current.ObservationSequence;
        var draw = proposal.Draw;
        observations.Add(new(++sequence, revision, "rng-sleep-wake", actor, draw.Before, draw.After,
            RandomRange: draw.Range, RandomValue: draw.Value));
        observations.Add(new(++sequence, revision, "sleep-turn-message", actor, Detail: proposal.Text.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        var scene = new BattleSceneState(null, BattleScenePhase.SleepTurnMessage, 0, new(sequence),
            sleepTurn: new(actor, proposal.Before, proposal.After, proposal.Text));
        return new(current.SessionId, revision, sequence, new ActiveBattle(current.Battle.With(mainSeed: draw.After), null, scene),
            current.Story, SessionStopReason.PresentationWait);
    }

    internal static SessionResult Complete(SessionSnapshot current, SessionCommand command, SessionRules rules)
    {
        var scene = current.BattleScene!;
        if (command is not CompletePresentation completion || completion.Wait != scene.Token || completion.Kind != scene.CompletionKind)
            return BattleCommandDispatcher.Reject(current, "battle-scene-wait", "command");
        if (scene.StunTurn is { } stun)
        {
            var unit = current.Battle.GetActor(stun.Actor);
            if (unit.Hp == 0 || unit.Status != stun.Before)
                return BattleCommandDispatcher.Reject(current, "stun-turn-context", "actor.status");
            long nextSequence = current.ObservationSequence;
            var updatedBattle = current.Battle.With(actors: current.Battle.Actors.Select(a =>
                a.Actor == stun.Actor ? a.With(status: stun.After, facing: 3) : a));
            List<SessionObservation> changes = [new(++nextSequence, current.Revision + 1, "stun-status", stun.Actor, stun.Before, stun.After)];
            var updated = new SessionSnapshot(current.SessionId, current.Revision + 1, nextSequence,
                new ActiveBattle(updatedBattle, null), current.Story, SessionStopReason.SimulationWait);
            return BattleAdvancer.Advance(CheckSleep(updated, stun.Actor, changes), changes, rules);
        }
        if (scene.CounterExpiry is { } expiry)
        {
            var unit = current.Battle.GetActor(expiry.Actor);
            if (unit.Hp == 0 || unit.Status != expiry.Before)
                return BattleCommandDispatcher.Reject(current, "counter-turn-context", "actor.status");
            long nextSequence = current.ObservationSequence;
            var updatedBattle = current.Battle.With(actors: current.Battle.Actors.Select(a =>
                a.Actor == unit.Actor ? a.With(status: expiry.After) : a));
            string effect = scene.Phase switch
            {
                BattleScenePhase.MuddleTurnMessage => "muddle-status",
                BattleScenePhase.SlowExpiredMessage => "slow-status",
                BattleScenePhase.AttackExpiredMessage => "attack-status",
                BattleScenePhase.BoostExpiredMessage => "boost-status",
                _ => "silence-status",
            };
            List<SessionObservation> expired = [new(++nextSequence, current.Revision + 1, effect, unit.Actor, unit.Status, expiry.After)];
            var updated = new SessionSnapshot(current.SessionId, current.Revision + 1, nextSequence,
                new ActiveBattle(updatedBattle, null), current.Story, SessionStopReason.SimulationWait);
            var next = scene.Phase switch
            {
                BattleScenePhase.MuddleTurnMessage => AfterMuddle(updated, unit.Actor, expired),
                BattleScenePhase.SlowExpiredMessage => TickCounter(updated, unit.Actor, BattleScenePhase.AttackExpiredMessage, expired),
                BattleScenePhase.AttackExpiredMessage => TickCounter(updated, unit.Actor, BattleScenePhase.BoostExpiredMessage, expired),
                BattleScenePhase.BoostExpiredMessage => AfterCounters(updated, unit.Actor, expired),
                _ => AfterSilence(updated, unit.Actor, expired),
            };
            return BattleAdvancer.Advance(next, expired, rules);
        }
        var notice = scene.SleepTurn!;
        var actor = current.Battle.GetActor(notice.Actor);
        if (actor.Status != notice.Before || actor.Hp == 0)
            return BattleCommandDispatcher.Reject(current, "sleep-turn-context", "actor.status");
        var battle = current.Battle.With(actors: current.Battle.Actors.Select(value =>
            value.Actor == actor.Actor ? value.With(status: notice.After, facing: 3) : value));
        long revision = current.Revision + 1, sequence = current.ObservationSequence;
        List<SessionObservation> observations = [new(++sequence, revision, "sleep-status", actor.Actor, notice.Before, notice.After)];
        var consumed = new SessionSnapshot(current.SessionId, revision, sequence,
            new ActiveBattle(battle, null), current.Story, SessionStopReason.SimulationWait);
        return BattleAdvancer.Advance(AfterSleep(consumed, actor.Actor, observations), observations, rules);
    }

    private static SessionSnapshot AfterSleep(SessionSnapshot current, ActorRef actor, List<SessionObservation> observations)
    {
        var unit = current.Battle.GetActor(actor);
        if (MuddleRules.AfterTurn(unit, current.Battle.MainSeed) is not { } result)
            return AfterMuddle(current, actor, observations);
        long sequence = current.ObservationSequence, revision = current.Revision + 1;
        var draw = result.Draw;
        observations.Add(new(++sequence, revision, "rng-muddle-expiry", actor, draw.Before, draw.After,
            RandomRange: draw.Range, RandomValue: draw.Value));
        var battle = current.Battle.With(mainSeed: draw.After);
        if (!MuddleRules.Present(result.After))
        {
            observations.Add(new(++sequence, revision, "muddle-turn-message", actor, Detail: "355"));
            return new(current.SessionId, revision, sequence, new ActiveBattle(battle, null,
                new(null, BattleScenePhase.MuddleTurnMessage, 0, new(sequence),
                    counterExpiry: new(actor, unit.Status, result.After))), current.Story, SessionStopReason.PresentationWait);
        }
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == actor ? a.With(status: result.After) : a));
        observations.Add(new(++sequence, revision, "muddle-status", actor, unit.Status, result.After));
        return AfterMuddle(new(current.SessionId, revision, sequence, new ActiveBattle(battle, null), current.Story,
            SessionStopReason.SimulationWait), actor, observations);
    }

    private static SessionSnapshot AfterMuddle(SessionSnapshot current, ActorRef actor, List<SessionObservation> observations)
    {
        var result = SilenceRules.AfterTurn(current.Battle.GetActor(actor), current.Battle.MainSeed);
        if (result is null) return AfterSilence(current, actor, observations);
        long sequence = current.ObservationSequence, revision = current.Revision + 1;
        var draw = result.Draw;
        observations.Add(new(++sequence, revision, "rng-silence-expiry", actor, draw.Before, draw.After,
            RandomRange: draw.Range, RandomValue: draw.Value));
        var battle = current.Battle.With(mainSeed: draw.After);
        if (!SilenceRules.Present(result.After))
        {
            observations.Add(new(++sequence, revision, "silence-turn-message", actor, Detail: "351"));
            return new(current.SessionId, revision, sequence, new ActiveBattle(battle, null,
                new(null, BattleScenePhase.SilenceTurnMessage, 0, new(sequence),
                    counterExpiry: new(actor, result.Before, result.After))), current.Story, SessionStopReason.PresentationWait);
        }
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == actor ? a.With(status: result.After) : a));
        observations.Add(new(++sequence, revision, "silence-status", actor, result.Before, result.After));
        return AfterSilence(new(current.SessionId, revision, sequence, new ActiveBattle(battle, null), current.Story,
            SessionStopReason.SimulationWait), actor, observations);
    }

    private static SessionSnapshot AfterSilence(SessionSnapshot current, ActorRef actor, List<SessionObservation> observations) =>
        TickCounter(current, actor, BattleScenePhase.SlowExpiredMessage, observations);

    private static SessionSnapshot TickCounter(SessionSnapshot current, ActorRef actor, BattleScenePhase phase, List<SessionObservation> observations)
    {
        var unit = current.Battle.GetActor(actor);
        var (mask, effect, text) = phase switch
        {
            BattleScenePhase.SlowExpiredMessage => (BattleStatCounters.Slow, "slow", "349"),
            BattleScenePhase.AttackExpiredMessage => (BattleStatCounters.Attack, "attack", "350"),
            BattleScenePhase.BoostExpiredMessage => (BattleStatCounters.Boost, "boost", "348"),
            _ => throw new InvalidOperationException("Unknown stat counter."),
        };
        int count = unit.Status & mask;
        int step = mask / 3;
        long sequence = current.ObservationSequence;
        if (count == step)
        {
            observations.Add(new(++sequence, current.Revision + 1, effect + "-expired", actor, Detail: text));
            var scene = new BattleSceneState(null, phase, 0, new(sequence),
                counterExpiry: new(actor, unit.Status, (ushort)(unit.Status - step)));
            return new(current.SessionId, current.Revision + 1, sequence, new ActiveBattle(current.Battle, null, scene),
                current.Story, SessionStopReason.PresentationWait);
        }
        if (count != 0)
        {
            ushort after = (ushort)(unit.Status - step);
            var battle = current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == actor ? a.With(status: after) : a));
            observations.Add(new(++sequence, current.Revision, effect + "-status", actor, unit.Status, after));
            current = new(current.SessionId, current.Revision, sequence, new ActiveBattle(battle, null), current.Story,
                SessionStopReason.SimulationWait);
        }
        return phase switch
        {
            BattleScenePhase.SlowExpiredMessage => TickCounter(current, actor, BattleScenePhase.AttackExpiredMessage, observations),
            BattleScenePhase.AttackExpiredMessage => TickCounter(current, actor, BattleScenePhase.BoostExpiredMessage, observations),
            _ => AfterCounters(current, actor, observations),
        };
    }

    internal static EngineBattleState Refresh(EngineBattleState battle, ActorRef actor) =>
        !battle.GetActor(actor).NeedsStatRefresh ? battle :
            battle.With(actors: battle.Actors.Select(a => a.Actor == actor ? a.With(refreshStats: true) : a));

    private static SessionSnapshot AfterCounters(SessionSnapshot current, ActorRef actor, List<SessionObservation> observations)
    {
        if (current.Battle.Definition.Poison && PoisonTurnRules.Eligible(current.Battle.GetActor(actor)))
            return BattlePoisonContinuation.Begin(current, actor, observations);
        long sequence = current.ObservationSequence;
        observations.Add(new(++sequence, current.Revision, "after-turn", actor));
        return new(current.SessionId, current.Revision, sequence, new ActiveBattle(BattleTurnFlow.ConsumeEntry(Refresh(current.Battle, actor)), null),
            current.Story, SessionStopReason.SimulationWait);
    }
}

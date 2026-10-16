using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime.Battles;

// Pending source command operands, not another actor/turn state authority.
internal sealed record SleepTurnNotice(ActorRef Actor, ushort Before, ushort After, int Text);

internal static class BattleAfterTurnContinuation
{
    internal static SessionSnapshot Begin(SessionSnapshot current, ActorRef actor, List<SessionObservation> observations)
    {
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
        var notice = scene.SleepTurn!;
        var actor = current.Battle.GetActor(notice.Actor);
        if (actor.Status != notice.Before || actor.Hp == 0)
            return BattleCommandDispatcher.Reject(current, "sleep-turn-context", "actor.status");
        var battle = current.Battle.With(actors: current.Battle.Actors.Select(value =>
            value.Actor == actor.Actor ? value.With(status: notice.After, facing: 3) : value));
        long revision = current.Revision + 1, sequence = current.ObservationSequence;
        List<SessionObservation> observations = [new(++sequence, revision, "sleep-status", actor.Actor, notice.Before, notice.After),
            new(++sequence, revision, "after-turn", actor.Actor)];
        var consumed = new SessionSnapshot(current.SessionId, revision, sequence,
            new ActiveBattle(BattleTurnFlow.ConsumeEntry(battle), null), current.Story, SessionStopReason.SimulationWait);
        return BattleAdvancer.Advance(consumed, observations, rules);
    }
}

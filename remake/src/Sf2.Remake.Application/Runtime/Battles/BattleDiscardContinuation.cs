using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleDiscardState(ActorRef Actor, string Item, WaitToken Token, bool Revealed = false)
{
    public string Text => $"{Actor.Value} discarded {Item}.";
}

internal static class BattleDiscardContinuation
{
    internal static SessionResult Begin(SessionSnapshot current, BattleDiscardResult result)
    {
        var observation = new SessionObservation(current.ObservationSequence + 1, current.Revision + 1,
            "discard-committed", result.Actor, result.Removed.Word, result.Removed.Slot,
            Detail: result.Removed.Item.Rare ? "deals" : "ordinary");
        var next = new SessionSnapshot(current.SessionId, observation.Revision, observation.Sequence,
            new ActiveBattle(result.Battle, new(result.Actor, current.Selection!.Preview, BattleSelectionStage.ActionChoice),
                Discard: new(result.Actor, result.Removed.Item.Name, new(observation.Sequence))), current.Story, SessionStopReason.PresentationWait);
        return new(next, [observation], next.StopReason);
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command)
    {
        var discard = current.BattleDiscard!;
        if (current.StopReason is SessionStopReason.Faulted or SessionStopReason.Unsupported)
            return BattleCommandDispatcher.Reject(current, "session-stopped", "command");
        bool reveal = command is CompleteTextReveal delivery && delivery.Wait == discard.Token && !discard.Revealed;
        bool acknowledge = command is Acknowledge ack && ack.Wait == discard.Token && discard.Revealed;
        if (!reveal && !acknowledge)
            return BattleCommandDispatcher.Reject(current, "stale-or-wrong-discard-wait", "wait");
        var observation = new SessionObservation(current.ObservationSequence + 1, current.Revision + 1,
            reveal ? "discard-text-revealed" : "discard-completed", discard.Actor);
        var next = new SessionSnapshot(current.SessionId, observation.Revision, observation.Sequence,
            new ActiveBattle(current.Battle, current.Selection, Discard: reveal ? discard with { Revealed = true } : null),
            current.Story, reveal ? SessionStopReason.PresentationWait : SessionStopReason.PlayerInput);
        return new(next, [observation], next.StopReason);
    }
}

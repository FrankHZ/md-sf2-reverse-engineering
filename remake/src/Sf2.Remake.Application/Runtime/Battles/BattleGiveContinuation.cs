using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleGiveState(ActorRef Giver, ActorRef Recipient, string Item, string? Returned,
    WaitToken Token, bool Revealed = false)
{
    public string Text => Returned is null ? $"{Giver.Value} gave {Item} to {Recipient.Value}." :
        $"{Giver.Value} gave {Item} to {Recipient.Value} and received {Returned}.";
}

internal static class BattleGiveContinuation
{
    internal static SessionResult Begin(SessionSnapshot current, BattleGiveResult transfer)
    {
        long revision = current.Revision + 1, sequence = current.ObservationSequence;
        List<SessionObservation> observations = [];
        var destination = transfer.Battle.GetActor(transfer.Giver).Position;
        if (current.Battle.GetActor(transfer.Giver).Position != destination)
            observations.Add(new(++sequence, revision, "movement", transfer.Giver,
                From: current.Battle.GetActor(transfer.Giver).Position, To: destination));
        observations.Add(new(++sequence, revision, "give-committed", transfer.Giver,
            transfer.Given.Word, transfer.Given.Slot, Target: transfer.Recipient,
            Detail: transfer.Returned is null ? "transfer" : "exchange"));
        var wait = new BattleGiveState(transfer.Giver, transfer.Recipient, transfer.Given.Name, transfer.Returned?.Name, new(sequence));
        var next = new SessionSnapshot(current.SessionId, revision, sequence,
            new ActiveBattle(transfer.Battle, null, Give: wait), current.Story, SessionStopReason.PresentationWait);
        return new(next, observations.AsReadOnly(), next.StopReason);
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command, SessionRules rules)
    {
        var give = current.BattleGive!;
        if (current.StopReason is SessionStopReason.Faulted or SessionStopReason.Unsupported)
            return BattleCommandDispatcher.Reject(current, "session-stopped", "command");
        if (command is CompleteTextReveal reveal && reveal.Wait == give.Token && !give.Revealed)
        {
            var observation = new SessionObservation(current.ObservationSequence + 1, current.Revision + 1, "give-text-revealed", give.Giver);
            var next = new SessionSnapshot(current.SessionId, observation.Revision, observation.Sequence,
                new ActiveBattle(current.Battle, null, Give: give with { Revealed = true }), current.Story, current.StopReason);
            return new(next, [observation], next.StopReason);
        }
        if (command is not Acknowledge ack || ack.Wait != give.Token || !give.Revealed)
            return BattleCommandDispatcher.Reject(current, "stale-or-wrong-give-wait", "wait");
        try
        {
            List<SessionObservation> observations = [];
            // Same no-scene suffix as STAY. The transfer/movement prefix is already durable.
            var completed = BattleActionCommitter.Publish(current, current.Battle, give.Giver,
                current.Battle.GetActor(give.Giver).Position, [], observations, rules);
            return BattleAdvancer.Advance(completed, observations, rules);
        }
        catch (BattleRuleException error) { return Failed(BattleChoices.Failure(error)); }
        catch (BattlePolicyFault error) { return Failed(BattleChoices.Failure(error)); }

        SessionResult Failed(SessionFailure failure) => new(new(current.SessionId, current.Revision,
            current.ObservationSequence, current.Active, current.Story, SessionStopReason.Faulted), [], SessionStopReason.Faulted, failure);
    }
}

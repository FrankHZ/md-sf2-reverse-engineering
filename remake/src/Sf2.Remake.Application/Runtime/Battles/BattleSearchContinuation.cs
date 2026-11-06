using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BeginBattleSearch : SessionCommand;
public sealed record BattleSearchState(ActorRef Actor, BattleChest Chest, BattleSearchKind Kind,
    WaitToken Token, ActorRef? Occupant = null, bool Revealed = false)
{
    public bool EndsTurn => Kind is not (BattleSearchKind.Full or BattleSearchKind.Occupied);
}

internal static class BattleSearchContinuation
{
    internal static BattleChest? Query(SessionSnapshot current) => current.HasBattleControl && current.Selection is { } selection
        ? BattleSearch.Faced(current.Battle, selection.Actor, selection.Preview.Destination) : null;

    internal static SessionResult Select(SessionSnapshot current)
    {
        if (current.Selection is not { Stage: BattleSelectionStage.ActionChoice } selection || Query(current) is null)
            return BattleCommandDispatcher.Reject(current, "search-unavailable", "search");
        return BattleCommandDispatcher.Selected(current,
            new(selection.Actor, selection.Preview, BattleSelectionStage.SearchCommitReady), "search-selected");
    }

    internal static SessionResult Begin(SessionSnapshot current, SessionRules rules)
    {
        var selection = current.Selection!;
        var chest = Query(current) ?? throw new BattleRuleException("search-unavailable", "search");
        // Validate the ordinary Stay suffix before publishing any reward; execute it only after real result delivery.
        _ = BattleChoices.Prepare(rules, current.Battle, selection.Actor, selection.Preview.Destination,
            new(BattleActionKind.Stay), null);
        bool opened = current.Story.Flags.Contains(chest.Flag);
        var result = BattleSearch.Prepare(current.Battle, selection.Actor, chest, opened);
        bool claimed = !opened && result.Kind is not (BattleSearchKind.Full or BattleSearchKind.Occupied);
        var story = claimed ? current.Story.Copy(null, flags: current.Story.Flags.Append(chest.Flag).Order().ToArray()) : current.Story;
        long revision = current.Revision + 1, sequence = current.ObservationSequence;
        List<SessionObservation> observations = [];
        if (claimed) observations.Add(new(++sequence, revision, "battle-search-receipt", selection.Actor, After: chest.Flag));
        observations.Add(new(++sequence, revision, "battle-search-result", selection.Actor, Detail: result.Kind.ToString()));
        var next = new SessionSnapshot(current.SessionId, revision, sequence,
            new ActiveBattle(result.Battle, new(selection.Actor, selection.Preview, BattleSelectionStage.ActionChoice),
                Search: new(selection.Actor, chest, result.Kind, new(sequence), result.Occupant)), story, SessionStopReason.PresentationWait);
        return new(next, observations.AsReadOnly(), next.StopReason);
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command, SessionRules rules)
    {
        var search = current.BattleSearch!;
        if (current.StopReason is SessionStopReason.Faulted or SessionStopReason.Unsupported)
            return BattleCommandDispatcher.Reject(current, "session-stopped", "command");
        bool reveal = command is CompleteTextReveal text && text.Wait == search.Token && !search.Revealed;
        bool acknowledge = command is Acknowledge ack && ack.Wait == search.Token && search.Revealed;
        if (!reveal && !acknowledge) return BattleCommandDispatcher.Reject(current, "stale-or-wrong-search-wait", "wait");
        var observation = new SessionObservation(current.ObservationSequence + 1, current.Revision + 1,
            reveal ? "battle-search-revealed" : "battle-search-completed", search.Actor);
        var next = new SessionSnapshot(current.SessionId, observation.Revision, observation.Sequence,
            new ActiveBattle(current.Battle, current.Selection, Search: reveal ? search with { Revealed = true } : null),
            current.Story, reveal ? SessionStopReason.PresentationWait : SessionStopReason.PlayerInput);
        if (reveal || !search.EndsTurn) return new(next, [observation], next.StopReason);
        try
        {
            var selection = next.Selection!;
            var action = BattleChoices.Prepare(rules, next.Battle, selection.Actor, selection.Preview.Destination,
                new(BattleActionKind.Stay), null);
            List<SessionObservation> observations = [observation];
            return BattleAdvancer.Advance(BattleActionCommitter.Publish(next, action.Prepared, selection.Actor,
                selection.Preview.Destination, action.CompletionEffects, observations, rules), observations, rules);
        }
        catch (BattleRuleException error) { return Failed(BattleChoices.Failure(error)); }
        catch (BattlePolicyFault error) { return Failed(BattleChoices.Failure(error)); }
        catch (BattleActionRuleFault error) { return Failed(BattleChoices.Failure(error)); }

        // The committed grant and receipt survive a later callback failure, with the exact wait retained.
        SessionResult Failed(SessionFailure failure) => new(new(current.SessionId, current.Revision,
            current.ObservationSequence, current.Active, current.Story, SessionStopReason.Faulted), [], SessionStopReason.Faulted, failure);
    }
}

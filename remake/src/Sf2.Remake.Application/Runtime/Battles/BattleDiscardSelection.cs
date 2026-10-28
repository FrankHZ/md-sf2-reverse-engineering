using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleDiscardChoices(IReadOnlyList<BattleDiscardChoice> Items, SessionFailure? Failure = null);

internal static class BattleDiscardSelection
{
    internal static bool Active(BattleSelectionStage stage) => stage is BattleSelectionStage.DiscardItemChoice or
        BattleSelectionStage.DiscardCommitReady;

    internal static BattleDiscardChoices Query(SessionSnapshot current)
    {
        try { return new(BattleDiscard.Choices(current.Battle, current.Selection!.Actor)); }
        catch (BattleRuleException error) { return new([], BattleChoices.Failure(error)); }
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command)
    {
        var selection = current.Selection!;
        switch (command)
        {
            case BeginDiscard when selection.Stage == BattleSelectionStage.ActionChoice:
                var choices = BattleDiscard.Choices(current.Battle, selection.Actor);
                if (choices.Count == 0) return BattleCommandDispatcher.Reject(current, "discard-no-items", "items");
                return Select(choices[0].Slot);
            case SelectDiscardItem item when selection.Stage == BattleSelectionStage.DiscardItemChoice:
                return Select(item.Slot);
            case Confirm when selection.Stage == BattleSelectionStage.DiscardItemChoice:
                var selected = RequireSelected();
                if (selected.Item.Unsellable)
                    return BattleCommandDispatcher.Reject(current, "discard-unsellable", "item");
                return BattleCommandDispatcher.Selected(current, new(selection.Actor, selection.Preview,
                    BattleSelectionStage.DiscardCommitReady, itemSlot: selected.Slot, itemWord: selected.Word), "discard-confirmation");
            case Confirm when selection.Stage == BattleSelectionStage.DiscardCommitReady:
                var committedItem = RequireSelected();
                return BattleDiscardContinuation.Begin(current,
                    BattleDiscard.Prepare(current.Battle, selection.Actor, committedItem.Slot, committedItem.Word));
            case Cancel when Active(selection.Stage):
                return BattleCommandDispatcher.Selected(current, new(selection.Actor, selection.Preview,
                    BattleSelectionStage.ActionChoice), "discard-cancelled");
            default:
                return BattleCommandDispatcher.Reject(current, "invalid-discard-selection", "command");
        }

        SessionResult Select(int slot)
        {
            var item = BattleDiscard.RequireSlot(current.Battle, selection.Actor, slot);
            return BattleCommandDispatcher.Selected(current, new(selection.Actor, selection.Preview,
                BattleSelectionStage.DiscardItemChoice, itemSlot: slot, itemWord: item.Word), "discard-item-selected");
        }
        BattleDiscardChoice RequireSelected() => selection.ItemSlot is { } slot && selection.ItemWord is { } word
            ? BattleDiscard.RequireSelected(current.Battle, selection.Actor, slot, word)
            : throw new BattleRuleException("stale-discard-item", "item.slot");
    }
}

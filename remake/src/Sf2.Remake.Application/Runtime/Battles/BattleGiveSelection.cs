using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleGiveChoices(IReadOnlyList<BattleGiveItem> Items,
    IReadOnlyList<ActorRef> Targets, IReadOnlyList<BattleGiveItem> Returns, SessionFailure? Failure = null);

internal static class BattleGiveSelection
{
    internal static bool Active(BattleSelectionStage stage) => stage is BattleSelectionStage.GiveItemChoice or
        BattleSelectionStage.GiveTargetChoice or BattleSelectionStage.GiveExchangeChoice or BattleSelectionStage.GiveCommitReady;

    internal static BattleGiveChoices Query(SessionSnapshot current)
    {
        var selection = current.Selection!;
        try
        {
            var items = BattleGive.Items(current.Battle, selection.Actor);
            var targets = new List<ActorRef>();
            foreach (var actor in current.Battle.Actors)
                try { BattleGive.RequireTarget(current.Battle, selection.Actor, selection.Preview.Destination, actor.Actor); targets.Add(actor.Actor); }
                catch (BattleRuleException) { }
            var returns = selection.Target is { } target && selection.Stage is
                BattleSelectionStage.GiveExchangeChoice or BattleSelectionStage.GiveCommitReady &&
                current.Battle.GetActor(target).SourceLoadout!.Items.All(word => (word & 127) != 127)
                    ? BattleGive.Items(current.Battle, target, false) : [];
            return new(items, targets.AsReadOnly(), returns);
        }
        catch (BattleRuleException error) { return new([], [], [], BattleChoices.Failure(error)); }
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command)
    {
        var selection = current.Selection!;
        var battle = current.Battle;
        if (command is BeginGive && selection.Stage == BattleSelectionStage.ActionChoice)
        {
            var choices = Query(current);
            if (choices.Failure is { } failure) return new(current, [], current.StopReason, failure);
            if (choices.Items.Count == 0) return BattleCommandDispatcher.Reject(current, "give-no-items", "items");
            if (choices.Targets.Count == 0) return BattleCommandDispatcher.Reject(current, "give-no-target", "target");
            return Item(choices.Items[0].Slot);
        }
        switch (command)
        {
            case SelectGiveItem item when selection.Stage == BattleSelectionStage.GiveItemChoice:
                return Item(item.Slot);
            case Confirm when selection.Stage == BattleSelectionStage.GiveItemChoice:
                RequireSelected();
                return Selected(BattleSelectionStage.GiveTargetChoice);
            case SelectTarget target when selection.Stage == BattleSelectionStage.GiveTargetChoice:
                RequireSelected();
                BattleGive.RequireTarget(battle, selection.Actor, selection.Preview.Destination, target.Target);
                return Selected(BattleSelectionStage.GiveTargetChoice, target.Target);
            case Confirm when selection.Stage == BattleSelectionStage.GiveTargetChoice && selection.Target is { } target:
                RequireSelected();
                var recipient = BattleGive.RequireTarget(battle, selection.Actor, selection.Preview.Destination, target);
                return recipient.SourceLoadout!.Items.All(word => (word & 127) != 127)
                    ? Return(BattleGive.Items(battle, target, false)[0].Slot)
                    : Selected(BattleSelectionStage.GiveCommitReady, target);
            case SelectGiveReturn item when selection.Stage == BattleSelectionStage.GiveExchangeChoice:
                return Return(item.Slot);
            case Confirm when selection.Stage == BattleSelectionStage.GiveExchangeChoice:
                _ = Prepare();
                return Selected(BattleSelectionStage.GiveCommitReady, selection.Target, selection.ReturnSlot, selection.ReturnWord);
            case Confirm when selection.Stage == BattleSelectionStage.GiveCommitReady:
                return BattleGiveContinuation.Begin(current, Prepare());
            case Cancel when selection.Stage == BattleSelectionStage.GiveItemChoice:
                return BattleCommandDispatcher.Selected(current,
                    new(selection.Actor, selection.Preview, BattleSelectionStage.ActionChoice), "give-cancelled");
            case Cancel when selection.Stage == BattleSelectionStage.GiveTargetChoice:
                return Selected(BattleSelectionStage.GiveItemChoice);
            case Cancel when selection.Stage == BattleSelectionStage.GiveExchangeChoice:
                return Selected(BattleSelectionStage.GiveTargetChoice, selection.Target);
            case Cancel when selection.Stage == BattleSelectionStage.GiveCommitReady:
                return Selected(selection.ReturnSlot is null ? BattleSelectionStage.GiveTargetChoice : BattleSelectionStage.GiveExchangeChoice,
                    selection.Target, selection.ReturnSlot, selection.ReturnWord);
            default: return BattleCommandDispatcher.Reject(current, "wrong-selection-stage", "phase");
        }

        SessionResult Item(int slot)
        {
            var item = BattleGive.RequireSlot(battle, selection.Actor, slot);
            return BattleCommandDispatcher.Selected(current, new(selection.Actor, selection.Preview,
                BattleSelectionStage.GiveItemChoice, itemSlot: slot, itemWord: item.Word), "give-item-selected");
        }
        SessionResult Return(int slot)
        {
            RequireSelected();
            if (selection.Target is not { } target) throw new BattleRuleException("give-no-target", "target");
            var recipient = BattleGive.RequireTarget(battle, selection.Actor, selection.Preview.Destination, target);
            if (recipient.SourceLoadout!.Items.Any(word => (word & 127) == 127))
                throw new BattleRuleException("give-capacity-changed", "target.items");
            var item = BattleGive.RequireSlot(battle, target, slot, false);
            return Selected(BattleSelectionStage.GiveExchangeChoice, target, slot, item.Word);
        }
        void RequireSelected()
        {
            if (selection.ItemSlot is not { } slot || BattleGive.RequireSlot(battle, selection.Actor, slot).Word != selection.ItemWord)
                throw new BattleRuleException("stale-give-item", "item.slot");
        }
        BattleGiveResult Prepare()
        {
            RequireSelected();
            if (selection.Target is not { } target) throw new BattleRuleException("give-no-target", "target");
            return BattleGive.Prepare(battle, selection.Actor, selection.Preview.Destination, selection.ItemSlot!.Value,
                selection.ItemWord!.Value, target, selection.ReturnSlot, selection.ReturnWord);
        }
        SessionResult Selected(BattleSelectionStage stage, ActorRef? target = null, int? returnSlot = null, ushort? returnWord = null) =>
            BattleCommandDispatcher.Selected(current, new(selection.Actor, selection.Preview, stage,
                target: target, itemSlot: selection.ItemSlot, itemWord: selection.ItemWord,
                returnSlot: returnSlot, returnWord: returnWord), "give-selection");
    }
}

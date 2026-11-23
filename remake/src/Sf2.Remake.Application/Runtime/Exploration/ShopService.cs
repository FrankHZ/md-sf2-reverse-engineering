using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum ShopAction { Buy, Sell, Deals, Repair }
public enum ShopPhase { Menu, Stock, Member, Slot, Confirm, Result }
public enum ShopResult { Bought, Sold, Full, InsufficientGold, Unsellable, UnsupportedItem, Repaired, Undamaged, EquippedCurse }
public sealed record ShopOffer(ShopAction Action, ActorRef Member, byte Item, uint Price, int? Slot = null, ushort? Word = null);
public sealed record ShopWait(WaitToken Token, ShopDefinition Definition, ShopPhase Phase = ShopPhase.Menu,
    ShopAction? Action = null, byte? Item = null, ActorRef? Member = null, ShopOffer? Offer = null,
    ShopResult? Result = null) : ProgramWait(Token)
{
    public IReadOnlyList<byte> AvailableStock(BattleItemCustody? custody) => Action switch
    {
        ShopAction.Buy => Definition.Stock,
        ShopAction.Deals when custody is not null => custody.Deals
            .Where(entry => entry.Value > 0 && Definition.Items.ContainsKey(entry.Key) && !Definition.Stock.Contains(entry.Key))
            .Select(entry => entry.Key).Order().ToArray(),
        _ => [],
    };
}
public abstract record ShopCommand(WaitToken Wait) : SessionCommand;
public sealed record ChooseShop(WaitToken Wait, ShopAction Action) : ShopCommand(Wait);
public sealed record SelectShopItem(WaitToken Wait, byte Item) : ShopCommand(Wait);
public sealed record SelectShopMember(WaitToken Wait, ActorRef Member) : ShopCommand(Wait);
public sealed record SelectShopSlot(WaitToken Wait, int Slot, ushort Word) : ShopCommand(Wait);
public sealed record AnswerShop(WaitToken Wait, ShopOffer Offer, bool Accept) : ShopCommand(Wait);
public sealed record BackShop(WaitToken Wait) : ShopCommand(Wait);
public sealed record ContinueShop(WaitToken Wait) : ShopCommand(Wait);

// One immutable wait owns selection; confirmation publishes gold, loadout and custody together.
internal static class ShopService
{
    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current, ShopCommand command, SessionRules rules)
    {
        if (current.Exploration is not { } field || current.Story.Wait is not ShopWait wait || command.Wait != wait.Token)
            return BattleCommandDispatcher.Reject(current, "stale-or-wrong-service", "wait");
        try
        {
            var next = wait with { Token = new(current.ObservationSequence + 1) };
            var party = field.Party;
            var custody = party.ItemCustody;
            string kind = "shop-selection";
            switch (command)
            {
                case ChooseShop choose when wait.Phase == ShopPhase.Menu && Enum.IsDefined(choose.Action):
                    if (choose.Action == ShopAction.Deals && custody is null)
                        throw new BattleRuleException("shop-custody-unavailable", "party.itemCustody", true);
                    next = next with { Phase = choose.Action is ShopAction.Sell or ShopAction.Repair ? ShopPhase.Member : ShopPhase.Stock, Action = choose.Action }; break;
                case SelectShopItem item when wait.Phase == ShopPhase.Stock && wait.AvailableStock(custody).Contains(item.Item):
                    next = next with { Phase = ShopPhase.Member, Item = item.Item }; break;
                case SelectShopMember member when wait.Phase == ShopPhase.Member:
                    var selected = Member(definition, current, member.Member);
                    _ = Loadout(selected);
                    bool heldItem = wait.Action is ShopAction.Sell or ShopAction.Repair;
                    next = next with { Member = member.Member, Phase = heldItem ? ShopPhase.Slot : ShopPhase.Confirm,
                        Offer = heldItem ? null : Offer(wait, member.Member, wait.Item!.Value) }; break;
                case SelectShopSlot slot when wait.Phase == ShopPhase.Slot:
                    var loadout = Loadout(Member(definition, current, wait.Member!.Value));
                    if (slot.Slot < 0 || slot.Slot >= 4 || loadout.Items[slot.Slot] != slot.Word || (slot.Word & 127) == 127)
                        throw new BattleRuleException("shop-stale-slot", "item.slot");
                    byte id = (byte)(slot.Word & 127);
                    var terms = wait.Definition.Items.GetValueOrDefault(id);
                    var slotRefusal = wait.Action == ShopAction.Repair ? RepairRefusal(terms, slot.Word) : SaleRefusal(terms, slot.Word, custody);
                    if (slotRefusal is { } refusal)
                        next = next with { Phase = ShopPhase.Result, Result = refusal };
                    else next = next with { Phase = ShopPhase.Confirm, Item = id, Offer = Offer(wait, wait.Member.Value, id, slot.Slot, slot.Word) };
                    break;
                case AnswerShop answer when wait.Phase == ShopPhase.Confirm && answer.Offer == wait.Offer:
                    if (!answer.Accept) { next = Back(next); break; }
                    var offer = wait.Offer!;
                    if (offer != Offer(wait, offer.Member, offer.Item, offer.Slot, offer.Word))
                        throw new BattleRuleException("shop-stale-offer", "offer");
                    var owner = Member(definition, current, offer.Member);
                    var items = Loadout(owner);
                    uint gold = party.Gold ?? throw new BattleRuleException("service-gold-unavailable", "party.gold", true);
                    ShopResult result;
                    if (offer.Action is ShopAction.Buy or ShopAction.Deals)
                    {
                        if (!wait.AvailableStock(custody).Contains(offer.Item)) throw new BattleRuleException("shop-stock", "item");
                        if (items.Items.All(word => (word & 127) != 127)) result = ShopResult.Full;
                        else if (gold < offer.Price) result = ShopResult.InsufficientGold;
                        else
                        {
                            var purchased = FieldInventory.WithLoadout(owner, BattleInventory.Add(items, offer.Item));
                            party = FieldInventory.Replace(party, [purchased],
                                custody: offer.Action == ShopAction.Deals ? custody!.RemoveDeal(offer.Item) : custody,
                                gold: gold - offer.Price);
                            result = ShopResult.Bought;
                        }
                    }
                    else
                    {
                        if (offer.Slot is not { } index || index < 0 || index >= 4 || items.Items[index] != offer.Word)
                            throw new BattleRuleException("shop-stale-slot", "item.slot");
                        var heldTerms = wait.Definition.Items[offer.Item];
                        ushort word = items.Items[index];
                        if ((word & 127) != offer.Item)
                            throw new BattleRuleException("shop-stale-slot", "item.slot");
                        if (offer.Action == ShopAction.Repair)
                        {
                            result = RepairRefusal(heldTerms, word) ?? (gold < offer.Price ? ShopResult.InsufficientGold :
                                heldTerms.Cursed && (word & 128) != 0 ? ShopResult.EquippedCurse : ShopResult.Repaired);
                            if (result == ShopResult.Repaired)
                            {
                                var repaired = BattleInventory.SetWord(items, index, (ushort)(word & ~BattleInventory.Broken));
                                party = FieldInventory.Replace(party, [FieldInventory.WithLoadout(owner, repaired)], gold: gold - offer.Price);
                            }
                        }
                        else
                        {
                            if (SaleRefusal(heldTerms, word, custody) is not null)
                                throw new BattleRuleException("shop-item-unsupported", "item", true);
                            party = FieldInventory.Replace(party, [FieldInventory.WithLoadout(owner, BattleInventory.Remove(items, index)).RefreshStats()],
                                custody: heldTerms.Rare ? custody!.AddDeal(offer.Item) : custody,
                                gold: BattleRewards.Gold(gold, offer.Price));
                            result = ShopResult.Sold;
                        }
                    }
                    next = next with { Phase = ShopPhase.Result, Result = result }; kind = "shop-" + result.ToString().ToLowerInvariant(); break;
                case ContinueShop when wait.Phase == ShopPhase.Result:
                    next = wait.Action == ShopAction.Repair
                        ? new(next.Token, wait.Definition, ShopPhase.Member, ShopAction.Repair)
                        : new(next.Token, wait.Definition);
                    kind = "shop-continued"; break;
                case BackShop when wait.Phase != ShopPhase.Menu && wait.Phase != ShopPhase.Result:
                    next = Back(next); break;
                case BackShop when wait.Phase == ShopPhase.Menu:
                    List<SessionObservation> returned = [];
                    var closed = ProgramRunner.Commit(current, current.Active, current.Story.Copy(current.Story.Cursor), returned, "shop-closed");
                    return ProgramRunner.Run(definition, closed, returned, rules);
                default: return BattleCommandDispatcher.Reject(current, "invalid-service-selection", "service");
            }
            List<SessionObservation> observations = [];
            current = ProgramRunner.Commit(current, new ActiveExploration(field.WithParty(party)),
                current.Story.Copy(current.Story.Cursor, next), observations, kind);
            return ProgramRunner.Result(current, observations);
        }
        catch (BattleRuleException error)
        {
            return new(current, [], error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Rejected,
                new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
                    error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    private static PartyMemberDetails Member(ScenarioDefinition definition, SessionSnapshot current, ActorRef actor) =>
        PartyDetailsQuery.Query(definition, current)!.Members.SingleOrDefault(row => row.Carried.Actor == actor &&
            row.Deployment is not null && row.Carried.Initialized != false) ?? throw new BattleRuleException("shop-member", "member");

    private static BattleSourceLoadout Loadout(PartyMemberDetails member)
    {
        var loadout = FieldInventory.Loadout(member);
        if (loadout.Items.Count != 4 || loadout.Items.Any(word => word > 511)) throw new BattleRuleException("shop-loadout", "member.items");
        if (member.Deployment!.Definition.Equipment is not null) BattleEquipment.ValidateLoadout(member.Deployment.Definition, loadout);
        return loadout;
    }

    private static ShopOffer Offer(ShopWait wait, ActorRef member, byte item, int? slot = null, ushort? word = null)
    {
        if (!wait.Definition.Items.TryGetValue(item, out var terms)) throw new BattleRuleException("shop-item", "item");
        uint price = wait.Action switch
        {
            ShopAction.Sell => (uint)terms.Price * 3 / 4,
            ShopAction.Repair => (uint)terms.Price >> 2,
            _ => terms.Price,
        };
        return new(wait.Action!.Value, member, item, price, slot, word);
    }

    private static ShopResult? RepairRefusal(ShopItem? item, ushort word)
    {
        if (item is null) return ShopResult.UnsupportedItem;
        if ((word & BattleInventory.Broken) == 0) return ShopResult.Undamaged;
        return item.Repairable ? null : ShopResult.UnsupportedItem;
    }

    private static ShopResult? SaleRefusal(ShopItem? item, ushort word, BattleItemCustody? custody)
    {
        if (item is null) return ShopResult.UnsupportedItem;
        if (item.Unsellable) return ShopResult.Unsellable;
        if ((word & 128) != 0 && item.Cursed) return ShopResult.EquippedCurse;
        if ((word & BattleInventory.Broken) != 0 || item.Rare && custody is null)
            return ShopResult.UnsupportedItem;
        return null;
    }

    private static ShopWait Back(ShopWait wait) => wait.Phase switch
    {
        ShopPhase.Confirm => wait with { Phase = ShopPhase.Member, Member = null, Offer = null,
            Item = wait.Action == ShopAction.Repair ? null : wait.Item },
        ShopPhase.Slot => wait with { Phase = ShopPhase.Member, Member = null,
            Item = wait.Action == ShopAction.Repair ? null : wait.Item },
        ShopPhase.Member when wait.Action is ShopAction.Buy or ShopAction.Deals => wait with { Phase = ShopPhase.Stock, Item = null },
        _ => new(wait.Token, wait.Definition),
    };
}

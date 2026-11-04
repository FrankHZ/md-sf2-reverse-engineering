using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum ShopAction { Buy, Sell }
public enum ShopPhase { Menu, Stock, Member, Slot, Confirm, Result }
public enum ShopResult { Bought, Sold, Full, InsufficientGold, Unsellable, UnsupportedItem }
public sealed record ShopOffer(ShopAction Action, ActorRef Member, byte Item, uint Price, int? Slot = null, ushort? Word = null);
public sealed record ShopWait(WaitToken Token, ShopDefinition Definition, ShopPhase Phase = ShopPhase.Menu,
    ShopAction? Action = null, byte? Item = null, ActorRef? Member = null, ShopOffer? Offer = null,
    ShopResult? Result = null) : ProgramWait(Token);
public abstract record ShopCommand(WaitToken Wait) : SessionCommand;
public sealed record ChooseShop(WaitToken Wait, ShopAction Action) : ShopCommand(Wait);
public sealed record SelectShopItem(WaitToken Wait, byte Item) : ShopCommand(Wait);
public sealed record SelectShopMember(WaitToken Wait, ActorRef Member) : ShopCommand(Wait);
public sealed record SelectShopSlot(WaitToken Wait, int Slot, ushort Word) : ShopCommand(Wait);
public sealed record AnswerShop(WaitToken Wait, ShopOffer Offer, bool Accept) : ShopCommand(Wait);
public sealed record BackShop(WaitToken Wait) : ShopCommand(Wait);
public sealed record ContinueShop(WaitToken Wait) : ShopCommand(Wait);

// One immutable wait owns selection; only a revalidated confirmation publishes gold and loadout.
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
            string kind = "shop-selection";
            switch (command)
            {
                case ChooseShop choose when wait.Phase == ShopPhase.Menu && Enum.IsDefined(choose.Action):
                    next = next with { Phase = choose.Action == ShopAction.Buy ? ShopPhase.Stock : ShopPhase.Member, Action = choose.Action }; break;
                case SelectShopItem item when wait.Phase == ShopPhase.Stock && wait.Definition.Stock.Contains(item.Item):
                    next = next with { Phase = ShopPhase.Member, Item = item.Item }; break;
                case SelectShopMember member when wait.Phase == ShopPhase.Member:
                    var selected = Member(definition, current, member.Member);
                    _ = Loadout(selected);
                    next = next with { Member = member.Member, Phase = wait.Action == ShopAction.Buy ? ShopPhase.Confirm : ShopPhase.Slot,
                        Offer = wait.Action == ShopAction.Buy ? Offer(wait, member.Member, wait.Item!.Value) : null }; break;
                case SelectShopSlot slot when wait.Phase == ShopPhase.Slot:
                    var loadout = Loadout(Member(definition, current, wait.Member!.Value));
                    if (slot.Slot < 0 || slot.Slot >= 4 || loadout.Items[slot.Slot] != slot.Word || (slot.Word & 127) == 127)
                        throw new BattleRuleException("shop-stale-slot", "item.slot");
                    byte id = (byte)(slot.Word & 127);
                    if (!wait.Definition.Items.TryGetValue(id, out var terms))
                        next = next with { Phase = ShopPhase.Result, Result = ShopResult.UnsupportedItem };
                    else if (terms.Unsellable)
                        next = next with { Phase = ShopPhase.Result, Result = ShopResult.Unsellable };
                    else if (terms.Rare || (slot.Word & 256) != 0)
                        next = next with { Phase = ShopPhase.Result, Result = ShopResult.UnsupportedItem };
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
                    if (offer.Action == ShopAction.Buy)
                    {
                        if (!wait.Definition.Stock.Contains(offer.Item)) throw new BattleRuleException("shop-stock", "item");
                        if (items.Items.All(word => (word & 127) != 127)) result = ShopResult.Full;
                        else if (gold < offer.Price) result = ShopResult.InsufficientGold;
                        else
                        {
                            party = FieldInventory.Replace(party, [FieldInventory.WithLoadout(owner, BattleInventory.Add(items, offer.Item))], gold: gold - offer.Price);
                            result = ShopResult.Bought;
                        }
                    }
                    else
                    {
                        if (offer.Slot is not { } index || index < 0 || index >= 4 || items.Items[index] != offer.Word)
                            throw new BattleRuleException("shop-stale-slot", "item.slot");
                        var soldTerms = wait.Definition.Items[offer.Item];
                        if (soldTerms.Unsellable || soldTerms.Rare || (offer.Word!.Value & 256) != 0)
                            throw new BattleRuleException("shop-item-unsupported", "item", true);
                        party = FieldInventory.Replace(party, [FieldInventory.WithLoadout(owner, BattleInventory.Remove(items, index))],
                            gold: BattleRewards.Gold(gold, offer.Price));
                        result = ShopResult.Sold;
                    }
                    next = next with { Phase = ShopPhase.Result, Result = result }; kind = "shop-" + result.ToString().ToLowerInvariant(); break;
                case ContinueShop when wait.Phase == ShopPhase.Result:
                    next = new(next.Token, wait.Definition); kind = "shop-continued"; break;
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
        return new(wait.Action!.Value, member, item, wait.Action == ShopAction.Buy ? terms.Price : (uint)terms.Price * 3 / 4, slot, word);
    }

    private static ShopWait Back(ShopWait wait) => wait.Phase switch
    {
        ShopPhase.Confirm => wait with { Phase = ShopPhase.Member, Member = null, Offer = null },
        ShopPhase.Slot => wait with { Phase = ShopPhase.Member, Member = null },
        ShopPhase.Member when wait.Action == ShopAction.Buy => wait with { Phase = ShopPhase.Stock, Item = null },
        _ => new(wait.Token, wait.Definition),
    };
}

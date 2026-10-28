using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Battles;

public sealed record BattleDiscardItem(byte ItemId, string Name, bool Rare, bool Unsellable);
public sealed class BattleDiscardDefinition
{
    internal BattleDiscardDefinition(IEnumerable<BattleDiscardItem> items) =>
        Items = new ReadOnlyDictionary<byte, BattleDiscardItem>(items.ToDictionary(item => item.ItemId));
    public IReadOnlyDictionary<byte, BattleDiscardItem> Items { get; }
}
public sealed record BattleDiscardChoice(int Slot, ushort Word, BattleDiscardItem Item);
internal sealed record BattleDiscardResult(EngineBattleState Battle, ActorRef Actor, BattleDiscardChoice Removed);

public static class BattleDiscard
{
    public static IReadOnlyList<BattleDiscardChoice> Choices(EngineBattleState battle, ActorRef actorRef)
    {
        var definition = battle.Definition.Discard ?? throw new BattleRuleException("discard-unavailable", "battle.discard", true);
        var actor = battle.GetActor(actorRef);
        if (!actor.IsAlly || actor.Control != BattleControl.Player || actor.Hp == 0 || actor.Position is null ||
            !actor.Initialized || !battle.Definition.Contains(actor.Position) || ((actor.ActivationWord ?? 0) & 8) != 0)
            throw new BattleRuleException("discard-actor", "actor");
        if (!(actor.Status == 0 || battle.Definition.Poison && actor.Status == 2))
            throw new BattleRuleException("discard-status", "actor.status", true);
        if (actor.SourceLoadout is not { Items.Count: 4 } loadout)
            throw new BattleRuleException("discard-loadout", "actor.sourceLoadout", true);
        List<BattleDiscardChoice> choices = [];
        for (int slot = 0; slot < 4; slot++)
        {
            ushort word = loadout.Items[slot];
            if (word == 127) continue;
            if (word > 255 || !definition.Items.TryGetValue((byte)(word & 127), out var item))
                throw new BattleRuleException("discard-item-unsupported", "actor.items", true);
            if ((word & 128) != 0 && actor.Definition.Equipment?.Weapons.ContainsKey(item.ItemId) != true)
                throw new BattleRuleException("discard-equipped-item", "actor.items", true);
            if (battle.Definition.ItemDrops?.Items.TryGetValue(item.ItemId, out var drop) == true && drop.Rare != item.Rare)
                throw new BattleRuleException("discard-item-rarity", "battle.discard");
            choices.Add(new(slot, word, item));
        }
        if (actor.Definition.Equipment is not null) BattleEquipment.RequireActor(actor);
        return choices.AsReadOnly();
    }

    internal static BattleDiscardChoice RequireSlot(EngineBattleState battle, ActorRef actor, int slot) =>
        Choices(battle, actor).SingleOrDefault(choice => choice.Slot == slot)
            ?? throw new BattleRuleException("discard-item-slot", "item.slot");

    internal static BattleDiscardChoice RequireSelected(EngineBattleState battle, ActorRef actor, int slot, ushort word)
    {
        var selected = RequireSlot(battle, actor, slot);
        if (selected.Word != word) throw new BattleRuleException("stale-discard-item", "item.slot");
        return selected;
    }

    internal static BattleDiscardResult Prepare(EngineBattleState battle, ActorRef actor, int slot, ushort word)
    {
        var selected = RequireSelected(battle, actor, slot, word);
        if (selected.Item.Unsellable) throw new BattleRuleException("discard-unsellable", "item");
        if (selected.Item.Rare && battle.ItemCustody is null)
            throw new BattleRuleException("discard-custody-unavailable", "battle.itemCustody", true);
        var changed = BattleEquipment.RemoveItem(battle.GetActor(actor), slot);
        return new(battle.With(actors: battle.Actors.Select(old => old.Actor == actor ? changed : old),
            itemCustody: selected.Item.Rare ? battle.ItemCustody!.AddDeal(selected.Item.ItemId) : battle.ItemCustody), actor, selected);
    }
}

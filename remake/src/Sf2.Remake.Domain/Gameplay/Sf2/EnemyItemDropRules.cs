using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class EnemyItemDropRules
{
    // Pinned DropEnemyItem compares these identities, not ITEMTYPE_BIT_RARE.
    internal static bool RandomDrop(byte item) => item is 68 or 102 or 110;

    internal static (EngineBattleState Battle, BattleItemReceipt? Receipt, IReadOnlyList<BattleEffect> Effects)
        Prepare(EngineBattleState current, ActorRef actorRef, ActorRef targetRef, bool targetDies)
    {
        var actor = current.GetActor(actorRef); var target = current.GetActor(targetRef);
        var definition = current.Definition.ItemDrops;
        if (!actor.IsAlly || target.IsAlly || !targetDies || definition is null ||
            !definition.EnemySlots.TryGetValue(targetRef, out byte enemySlot)) return (current, null, []);
        var held = target.SourceLoadout ?? throw new BattleRuleException("drop-inventory-unknown", "target.items", true);
        var row = definition.Rows.FirstOrDefault(row => row.Battle == definition.Battle && row.EnemySlot == enemySlot &&
            held.Items.Any(word => (word & 127) == row.ItemId));
        if (row is null) return (current, null, []);
        var custody = current.ItemCustody ?? throw new BattleRuleException("drop-custody-unknown", "battle.itemCustody", true);
        List<BattleEffect> effects = [];
        if (RandomDrop(row.ItemId))
        {
            var draw = BattleRandom.NextMain(current.MainSeed, 32);
            effects.Add(new("rng-item-drop", actorRef, draw.Before, draw.After, 32, draw.Value, targetRef));
            current = current.With(mainSeed: draw.After);
            if (draw.Value != 0) return (current, null, effects.AsReadOnly());
        }
        if (custody.OnceFlags.Contains(row.OnceFlag)) return (current, null, effects.AsReadOnly());
        custody = custody.Mark(row.OnceFlag);
        effects.Add(new("drop-once-set", actorRef, 0, row.OnceFlag, Target: targetRef));
        int slot = held.Items.ToList().FindIndex(word => (word & 127) == row.ItemId);
        var remaining = held.Items.ToList(); remaining.RemoveAt(slot); remaining.Add(127);
        target = target.With(sourceLoadout: new(remaining, held.Spells));
        effects.Add(new("enemy-item-removed", targetRef, held.Items[slot], slot));
        var item = definition.Items[row.ItemId];
        BattleItemReceipt? receipt = null;
        int free = actor.SourceLoadout?.Items.ToList().FindIndex(word => (word & 127) == 127) ?? -1;
        if (actor.Hp > 0 && free >= 0)
        {
            var items = actor.SourceLoadout!.Items.ToArray();
            // DropEnemyItem passes the masked identity to AddItem; equipped, AI and
            // broken status from the enemy entry do not reach the received word.
            items[free] = row.ItemId;
            actor = actor.With(sourceLoadout: new(items, actor.SourceLoadout.Spells));
            receipt = new(actorRef, targetRef, item);
            effects.Add(new("item-received", actorRef, free, row.ItemId, Target: targetRef));
        }
        else if (item.Rare)
        {
            byte before = custody.Deals.GetValueOrDefault(row.ItemId);
            custody = custody.AddDeal(row.ItemId);
            effects.Add(new("item-to-deals", actorRef, before, custody.Deals[row.ItemId], Target: targetRef));
        }
        else effects.Add(new("item-discarded", actorRef, row.ItemId, Target: targetRef));
        return (current.With(actors: current.Actors.Select(value => value.Actor == actorRef ? actor :
            value.Actor == targetRef ? target : value), itemCustody: custody), receipt, effects.AsReadOnly());
    }
}

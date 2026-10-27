using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class EnemyItemDropReader
{
    internal static IReadOnlyDictionary<byte, EnemyDropItem> Items(JsonElement root)
    {
        var items = new Dictionary<byte, EnemyDropItem>();
        if (!root.TryGetProperty("dropItems", out _)) return items;
        foreach (var row in Array(root, "dropItems"))
        {
            Object(row, "dropItem", "id", "name", "rare");
            byte id = (byte)Number(row, "id", 0, 126);
            bool rare = Boolean(row, "rare");
            // Explicit finite admission, from pinned itemdefs.asm. Chance is a
            // separate source identity rule; Achilles is rare but never rolls.
            Require(id is 0 or 61 or 68 or 86 or 102 or 110,
                "drop-item-unsupported", "dropItems.id", true);
            Require(rare == (id is 61 or 68 or 102 or 110), "drop-item-rarity", "dropItems.rare");
            string name = Text(row, "name");
            Require(name.Length is > 0 and <= 64, "drop-item-name", "dropItems.name");
            Require(items.TryAdd(id, new(id, name, rare)), "duplicate-item", "dropItems.id");
        }
        return items;
    }

    internal static EnemyItemDropDefinition? Read(JsonElement encounter,
        IReadOnlyList<BattleDeploymentDefinition> deployments, IReadOnlyDictionary<byte, EnemyDropItem> items)
    {
        if (!encounter.TryGetProperty("itemDrops", out var binding)) return null;
        Object(binding, "itemDrops", "battle", "enemySlots", "rows");
        byte battle = (byte)Number(binding, "battle", 0, 44);
        var slots = new Dictionary<ActorRef, byte>();
        foreach (var row in Array(binding, "enemySlots"))
        {
            Object(row, "drop-enemy-slot", "actor", "slot");
            var actor = new ActorRef(Id(row, "actor"));
            byte slot = (byte)Number(row, "slot", 128, 159);
            Require(deployments.Any(value => value.Actor == actor && value.Faction == BattleFaction.Enemy),
                "drop-enemy", "itemDrops.enemySlots.actor");
            Require(!slots.ContainsValue(slot) && slots.TryAdd(actor, slot), "duplicate-source-slot", "itemDrops.enemySlots");
        }
        var rows = new List<EnemyDropRow>();
        foreach (var row in Array(binding, "rows"))
        {
            Object(row, "drop-row", "battle", "enemySlot", "item", "onceFlag");
            var value = new EnemyDropRow((byte)Number(row, "battle", 0, 44), (byte)Number(row, "enemySlot", 128, 159),
                (byte)Number(row, "item", 0, 126), (byte)Number(row, "onceFlag", 0, 29));
            Require(items.ContainsKey(value.ItemId), "missing-drop-item", "itemDrops.rows.item");
            Require(!rows.Any(previous => previous.OnceFlag == value.OnceFlag ||
                (previous.Battle, previous.EnemySlot, previous.ItemId) == (value.Battle, value.EnemySlot, value.ItemId)),
                "duplicate-drop-row", "itemDrops.rows");
            rows.Add(value);
        }
        Require(rows.Count is > 0 and <= 30, "drop-row-count", "itemDrops.rows");
        return new(battle, slots, rows, items.Values);
    }

    internal static BattleItemCustody? Start(JsonElement start, IReadOnlyDictionary<byte, EnemyDropItem> items)
    {
        if (!start.TryGetProperty("itemCustody", out var custody)) return null;
        Object(custody, "itemCustody", "onceFlags", "deals");
        var flags = new List<byte>();
        foreach (var flag in Array(custody, "onceFlags"))
        {
            Require(flag.ValueKind == JsonValueKind.Number && flag.TryGetByte(out byte value) && value <= 29 && !flags.Contains(value),
                "drop-once-flag", "itemCustody.onceFlags");
            flags.Add(flag.GetByte());
        }
        var deals = new Dictionary<byte, byte>();
        foreach (var row in Array(custody, "deals"))
        {
            Object(row, "deals", "item", "count");
            byte item = (byte)Number(row, "item", 0, 126);
            Require(items.TryGetValue(item, out var definition) && definition.Rare, "deals-item", "itemCustody.deals.item");
            Require(deals.TryAdd(item, (byte)Number(row, "count", 0, 15)), "duplicate-deals-item", "itemCustody.deals.item");
        }
        return new(flags, deals);
    }
}

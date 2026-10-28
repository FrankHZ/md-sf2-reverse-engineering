using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class BattleDiscardReader
{
    internal static BattleDiscardDefinition? Read(JsonElement root, bool connected,
        IReadOnlyDictionary<byte, EnemyDropItem> drops, IReadOnlyList<BattleWeaponDefinition> weapons,
        IEnumerable<HealingItemDefinition> healing, IEnumerable<RetreatItemDefinition> retreat)
    {
        if (!root.TryGetProperty("battleDiscard", out var binding)) return null;
        Require(!connected, "discard-standalone-opt-in", "battleDiscard", true);
        Object(binding, "battleDiscard", "items");
        List<BattleDiscardItem> items = [];
        foreach (var row in Array(binding, "items"))
        {
            Object(row, "discard-item", "id", "name", "rare", "unsellable");
            byte id = (byte)Number(row, "id", 0, 126);
            Require(id is 0 or 61 or 113, "discard-item-unsupported", "battleDiscard.items.id", true);
            bool rare = Boolean(row, "rare"), unsellable = Boolean(row, "unsellable");
            Require(rare == (id is 61 or 113) && unsellable == (id == 113),
                "discard-item-properties", "battleDiscard.items");
            string name = Text(row, "name");
            Require(name.Length is > 0 and <= 64, "discard-item-name", "battleDiscard.items.name");
            Require(items.All(item => item.ItemId != id), "duplicate-item", "battleDiscard.items.id");
            Require(!drops.TryGetValue(id, out var drop) || drop.Rare == rare,
                "discard-item-rarity", "battleDiscard.items");
            Require(id == 0 ? weapons.All(w => w.ItemId != id) && retreat.All(i => i.ItemId != id) :
                healing.All(i => i.ItemId != id) && retreat.All(i => i.ItemId != id),
                "conflicting-item-type", "battleDiscard.items");
            Require(id != 113 || weapons.All(w => w.ItemId != id), "conflicting-item-type", "battleDiscard.items");
            items.Add(new(id, name, rare, unsellable));
        }
        Require(items.Count is > 0 and <= 3, "discard-item-count", "battleDiscard.items");
        return new(items);
    }

    internal static void Validate(BattleActorDefinition actor, BattleDiscardDefinition? discard)
    {
        if (discard is null) return;
        Require(actor.SourceLoadout is { Items.Count: 4 } loadout && loadout.Items.All(word =>
            word == 127 || word <= 255 && discard.Items.ContainsKey((byte)(word & 127)) &&
            ((word & 128) == 0 || actor.Equipment?.Weapons.ContainsKey((byte)(word & 127)) == true)),
            "discard-item-unsupported", "actor.items", true);
    }

    internal static IReadOnlyDictionary<byte, EnemyDropItem> CustodyItems(IReadOnlyDictionary<byte, EnemyDropItem> drops,
        BattleDiscardDefinition? discard) => drops.Values.Concat(discard?.Items.Values
            .Where(item => !drops.ContainsKey(item.ItemId)).Select(item => new EnemyDropItem(item.ItemId, item.Name, item.Rare)) ?? [])
            .ToDictionary(item => item.ItemId);
}

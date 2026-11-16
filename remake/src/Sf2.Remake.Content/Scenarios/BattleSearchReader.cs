using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class BattleSearchReader
{
    internal static BattleDefinition BindShrine(BattleDefinition battle, ExplorationDefinition world, PrivateBattleDefinitions source)
    {
        var item = source.Items[9];
        Require(item.Code == "POWER_WATER" && item.ItemTypeExpression == "CONSUMABLE", "shrine-drop-item", "items");
        var slots = battle.Deployments.Where(d => d.Faction == BattleFaction.Enemy)
            .ToDictionary(d => d.Actor, d => (byte)d.ProcessingOrder);
        battle = battle.WithItemDrops(new(15, slots, [new(15, 143, item.Id, 29)], [new(item.Id, item.DisplayName, false)]));

        var region = battle.SourceMapRegion!;
        if (!world.Maps.TryGetValue(region.Map, out var map)) return battle;
        var chest = map.Search?.Chests.SingleOrDefault(row => row.Flag == 151);
        if (chest is null || (map.Layout[chest.Position.X, chest.Position.Y] & 0x3C00) != 0x1800) return battle;
        Require(chest.ItemWord == item.Id, "shrine-chest-item", "map.search");
        var position = new MapPosition(chest.Position.X - region.Origin.X, chest.Position.Y - region.Origin.Y);
        Require(battle.Contains(position), "shrine-battle-chest-position", "map.search");
        return battle.WithSearch(new([new(position, chest.Flag, item.Id, item.DisplayName, null)]));
    }

    internal static BattleSearchDefinition? Read(JsonElement encounter, int width, int height,
        IEnumerable<HealingItemDefinition> healing, IEnumerable<RetreatItemDefinition> retreat,
        IEnumerable<BattleWeaponDefinition> weapons)
    {
        if (!encounter.TryGetProperty("search", out var input)) return null;
        Object(input, "search", "chests");
        var items = healing.Select(i => (i.ItemId, i.Name)).Concat(retreat.Select(i => (i.ItemId, i.Name)))
            .Concat(weapons.Select(i => (i.ItemId, i.Name))).ToDictionary(i => i.ItemId, i => i.Name);
        List<BattleChest> chests = [];
        foreach (var row in Array(input, "chests"))
        {
            Object(row, "search.chest", "x", "y", "flag", "item", "gold");
            var position = new Sf2.Remake.Domain.Maps.MapPosition(Number(row, "x", 0, width - 1), Number(row, "y", 0, height - 1));
            int flag = Number(row, "flag", 0, 65535);
            byte? item = row.GetProperty("item").ValueKind == JsonValueKind.Null ? null : (byte)Number(row, "item", 0, 126);
            uint? gold = row.GetProperty("gold").ValueKind == JsonValueKind.Null ? null : (uint)Number(row, "gold", 1, 999999);
            Require(item is null || gold is null, "search-reward", "search.chest");
            Require(item is null || items.ContainsKey(item.Value), "search-item-unsupported", "search.chest.item", true);
            Require(chests.All(c => c.Position != position && c.Flag != flag), "duplicate-search-chest", "search.chests");
            chests.Add(new(position, flag, item, item is { } id ? items[id] : null, gold));
        }
        Require(chests.Count is > 0 and <= 64, "search-chest-count", "search.chests");
        return new(chests);
    }
}

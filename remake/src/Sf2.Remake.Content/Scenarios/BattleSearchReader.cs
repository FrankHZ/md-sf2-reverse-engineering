using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class BattleSearchReader
{
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

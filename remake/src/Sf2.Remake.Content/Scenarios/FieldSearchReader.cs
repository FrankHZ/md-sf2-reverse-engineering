using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class FieldSearchReader
{
    internal static FieldSearchDefinition Read(JsonElement row, IReadOnlyDictionary<int, string> texts)
    {
        Object(row, "search", "chests", "otherItems", "descriptions", "closedChest", "openChest");
        Require(new[] { 403, 404, 405, 408, 409, 410, 412, 423, 427 }.All(texts.ContainsKey),
            "search-text-required", "search.texts");
        FieldItemPlacement[] Items(string key) => Array(row, key).Select(item =>
        {
            Object(item, "search.item", "position", "flag", "itemWord", "gold");
            ushort? word = item.GetProperty("itemWord").ValueKind == JsonValueKind.Null ? null : (ushort)Number(item, "itemWord", 0, 511);
            uint? gold = item.GetProperty("gold").ValueKind == JsonValueKind.Null ? null : (uint)Number(item, "gold", 1, 999999);
            Require((word is null) != (gold is null), "search-reward", "search.item");
            return new FieldItemPlacement(ExplorationContentReader.Position(item.GetProperty("position")), Number(item, "flag", 0, 65535), word, gold);
        }).ToArray();
        var chests = Items("chests"); var other = Items("otherItems");
        Require(chests.Select(item => item.Position).Distinct().Count() == chests.Length &&
            other.Select(item => item.Position).Distinct().Count() == other.Length &&
            chests.Concat(other).Select(item => item.Flag).Distinct().Count() == chests.Length + other.Length,
            "search-duplicate-receipt", "search.items");
        var descriptions = Array(row, "descriptions").Select(description =>
        {
            Object(description, "search.description", "position", "menuOnly", "texts");
            var ids = Array(description, "texts").Select(value =>
            {
                Require(value.TryGetInt32(out int id) && texts.ContainsKey(id), "search-text", "search.description");
                return value.GetInt32();
            }).ToArray();
            Require(ids.Length == 2, "search-description-shape", "search.description");
            return new FieldAreaDescription(ExplorationContentReader.Position(description.GetProperty("position")), Boolean(description, "menuOnly"), System.Array.AsReadOnly(ids));
        }).ToArray();
        ushort closed = (ushort)Number(row, "closedChest", 0, 65535), open = (ushort)Number(row, "openChest", 0, 65535);
        Require(closed != open && (closed & 0x3C00) == 0x1800 && (open & 0x3C00) == 0x1800,
            "search-chest-blocks", "search");
        return new(chests, other, descriptions, closed, open);
    }
}

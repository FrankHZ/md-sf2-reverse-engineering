using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class IdentityIconReader
{
    internal static void Validate(IReadOnlyDictionary<string, ExplorationRaster> rasters)
    {
        foreach (var (key, raster) in rasters.Where(row => row.Key.StartsWith("identity-icon.", StringComparison.Ordinal)))
        {
            string[] parts = key.Split('.');
            Require(parts.Length == 4 && parts[1] is "item" or "spell" or "other" &&
                int.TryParse(parts[2], out int id) && parts[2] == id.ToString(System.Globalization.CultureInfo.InvariantCulture) &&
                (parts[1] == "item" ? id is >= 0 and <= 127 : parts[1] == "spell" ? id is >= 0 and <= 32 && id is not (16 or 17 or 18) : id == 128) &&
                parts[3] is "normal" or "selected", "identity-icon-key", "presentation.identityIcons");
            Require(raster.Width == 16 && raster.Height == 24, "identity-icon-shape", key);
            string peer = key[..key.LastIndexOf('.')] + (parts[3] == "normal" ? ".selected" : ".normal");
            Require(rasters.ContainsKey(peer), "identity-icon-pair", key);
        }
    }
    internal static void Bindings(IReadOnlyDictionary<string, ExplorationRaster>? rasters, PrivateBattleDefinitions definitions)
    {
        if (rasters is null || !rasters.Keys.Any(key => key.StartsWith("identity-icon.", StringComparison.Ordinal))) return;
        var identities = definitions.Items.Values.Where(item => item.ItemTypeExpression != "WEAPON" ||
                definitions.Classes.Values.Any(allowed => item.EquipFlags.Contains(allowed.Code)))
            .Select(item => $"identity-icon.item.{item.Id}")
            .Concat(definitions.Spells.Values.Where(spell => spell.Code is "HEAL" or "DETOX" or "EGRESS" or "BLAZE" or "BLAST" or "DISPEL")
                .Select(spell => $"identity-icon.spell.{spell.BaseId}"))
            .Append("identity-icon.other.128");
        foreach (string identity in identities.Distinct())
            Require(rasters.ContainsKey(identity + ".normal") && rasters.ContainsKey(identity + ".selected"),
                "identity-icon-binding", identity);
    }
    internal static IReadOnlyDictionary<string, ExplorationRaster>? Read(JsonElement presentation)
    {
        if (!presentation.TryGetProperty("identityIcons", out var value)) return null;
        Require(value.ValueKind == JsonValueKind.Object, "identity-icon-object", "presentation.identityIcons");
        var rasters = new Dictionary<string, ExplorationRaster>(StringComparer.Ordinal);
        foreach (var row in value.EnumerateObject())
            Require(row.Name.StartsWith("identity-icon.", StringComparison.Ordinal) && rasters.TryAdd(row.Name, ExplorationAssetReader.Raster(row.Value)),
                "identity-icon-key", "presentation.identityIcons");
        Require(rasters.Count > 0, "identity-icon-empty", "presentation.identityIcons");
        Validate(rasters);
        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, ExplorationRaster>(rasters);
    }

    // The first admitted HD family is one field item, separate from battle/base rasters.
    internal static IReadOnlyDictionary<string, ExplorationRaster>? ReadHd(JsonElement presentation,
        IReadOnlyDictionary<string, ExplorationRaster>? originals)
    {
        if (!presentation.TryGetProperty("hdIdentityIcons", out var value)) return null;
        string[] keys = ["identity-icon.item.0.normal", "identity-icon.item.0.selected"];
        Object(value, "presentation.hdIdentityIcons", keys);
        var rasters = keys.ToDictionary(key => key, key => ExplorationAssetReader.Raster(value.GetProperty(key)), StringComparer.Ordinal);
        Require(rasters.Values.All(raster => raster.Width == 64 && raster.Height == 96), "hd-identity-icon-shape", "presentation.hdIdentityIcons");
        Require(originals is not null && keys.All(originals.ContainsKey), "hd-identity-icon-baseline", "presentation.hdIdentityIcons");
        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, ExplorationRaster>(rasters);
    }
}

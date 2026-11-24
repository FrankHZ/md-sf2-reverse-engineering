using System.Collections.ObjectModel;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class MapTileAnimationReader
{
    internal static MapTileAnimation? Read(JsonElement visual, IReadOnlyList<IReadOnlyList<ushort>> blocks)
    {
        if (!visual.TryGetProperty("tileAnimation", out var table)) return null;
        const string field = "tileAnimation";
        Object(table, field, "target", "count", "capacity", "phases");
        int target = Number(table, "target", 256, 895), count = Number(table, "count", 1, 128);
        int capacity = Number(table, "capacity", count, 128);
        Require(target + count <= 896, "map-animation-target", field);
        var affected = blocks.Select(row => row.Any(word => (word & 1023) >= target && (word & 1023) < target + count)).ToArray();
        var rows = Array(table, "phases");
        Require(table.GetProperty("phases").GetArrayLength() is > 0 and <= 128, "map-animation-length", field);
        var phases = new List<MapTilePhase>();
        foreach (var row in rows)
        {
            Object(row, field, "source", "ticks", "blocks", "contexts");
            int source = Number(row, "source", 0, capacity - count), ticks = Number(row, "ticks", 1, 65535);
            var replacements = new Dictionary<int, MapTileReplacement>();
            foreach (var item in Array(row, "blocks"))
            {
                Object(item, field, "block", "original", "hd");
                int block = Number(item, "block", 0, blocks.Count - 1);
                var original = ExplorationAssetReader.Raster(item.GetProperty("original"));
                var hd = ExplorationAssetReader.Raster(item.GetProperty("hd"));
                Require(original.Width == 24 && original.Height == 24 && hd.Width == 96 && hd.Height == 96,
                    "map-animation-shape", field);
                Require(affected[block] && replacements.TryAdd(block, new(original, hd)), "map-animation-block", field);
            }
            var edges = HdTerrainBankReader.BlockContexts(row, blocks.Count, "contexts")!;
            foreach (var key in edges.Keys)
            {
                var (nw, n, ne, w, center, e, sw, s, se) = key;
                Require(new[] { nw, n, ne, w, center, e, sw, s, se }.Any(replacements.ContainsKey),
                    "map-animation-neighborhood", field);
            }
            phases.Add(new(source, ticks, new ReadOnlyDictionary<int, MapTileReplacement>(replacements), edges));
        }
        return new(target, count, phases.AsReadOnly());
    }
}

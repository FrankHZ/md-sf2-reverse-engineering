using System.Collections.ObjectModel;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class HdTerrainBankReader
{
    internal static (IReadOnlyList<ExplorationRaster>? Blocks, IReadOnlyDictionary<BlockNeighborhood, ExplorationRaster>? Contexts)
        Read(JsonElement map, MapId id, int count, string? directory)
    {
        if (!map.TryGetProperty("hdTerrainFile", out _)) return Frames(map);
        const string field = "hdTerrainFile";
        Require(!map.TryGetProperty("hdBlocks", out _) && !map.TryGetProperty("hdBlockContexts", out _),
            "mixed-hd-terrain", field);
        string name = Text(map, field);
        var parts = name.Split('/');
        Require(directory is not null && name.EndsWith(".json", StringComparison.Ordinal) &&
            !name.Contains('\\') && !name.Contains(':') && parts.All(part => part.Length > 0 && !part.EndsWith('.') && !part.EndsWith(' ')),
            "hd-terrain-path", field);
        string path = directory!;
        try
        {
            foreach (string part in parts)
            {
                path = Path.Combine(path, part);
                Require(!File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint), "hd-terrain-path", field);
            }
            using var stream = File.OpenRead(path);
            long size = stream.Length;
            Require(size is > 0 and <= 8 * 1024 * 1024, "document-size", field);
            var bytes = new byte[(int)size];
            stream.ReadExactly(bytes);
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 16 });
            var bank = document.RootElement;
            ObjectOptional(bank, "hd-terrain", ["map", "hdBlocks"], "hdBlockContexts");
            Require(Id(bank, "map") == id.Value, "hd-terrain-map", field);
            return Frames(bank);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        { throw new AdmissionIssue(new(SessionFailureKind.ContentError, "hd-terrain-read", field, "HD terrain file is unavailable.")); }
        catch (JsonException)
        { throw new AdmissionIssue(new(SessionFailureKind.ContentError, "json-syntax", field, "Invalid HD terrain JSON.")); }

        (IReadOnlyList<ExplorationRaster>?, IReadOnlyDictionary<BlockNeighborhood, ExplorationRaster>?) Frames(JsonElement bank) =>
            (ExplorationAssetReader.OptionalFrames(bank, "hdBlocks", count, 96, 96), BlockContexts(bank, count));
    }

    private static IReadOnlyDictionary<BlockNeighborhood, ExplorationRaster>? BlockContexts(JsonElement map, int count)
    {
        const string field = "hdBlockContexts";
        if (!map.TryGetProperty(field, out _)) return null;
        Require(map.TryGetProperty("hdBlocks", out _), "hd-context-without-blocks", field);
        var contexts = new Dictionary<BlockNeighborhood, ExplorationRaster>();
        foreach (var row in Array(map, field))
        {
            Object(row, field, "raster", "neighbors");
            var raster = ExplorationAssetReader.Raster(row.GetProperty("raster"));
            Require(raster.Width == 96 && raster.Height == 96, "hd-frame-shape", field);
            var neighbors = Array(row, "neighbors");
            Require(row.GetProperty("neighbors").GetArrayLength() > 0, "empty-hd-context", field);
            foreach (var ids in neighbors)
            {
                Require(ids.ValueKind == JsonValueKind.Array && ids.GetArrayLength() == 9, "hd-context-shape", field);
                int[] n = ids.EnumerateArray().Select(id => id.ValueKind == JsonValueKind.Number &&
                    id.TryGetInt32(out int value) ? value : -2).ToArray();
                Require(n.All(id => id >= -1 && id < count) && n[4] >= 0, "hd-context-block", field);
                var key = new BlockNeighborhood(n[0], n[1], n[2], n[3], n[4], n[5], n[6], n[7], n[8]);
                Require(contexts.TryAdd(key, raster), "duplicate-hd-context", field);
            }
        }
        return new ReadOnlyDictionary<BlockNeighborhood, ExplorationRaster>(contexts);
    }
}

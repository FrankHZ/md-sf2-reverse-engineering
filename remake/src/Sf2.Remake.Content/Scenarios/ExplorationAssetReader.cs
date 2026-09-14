using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class ExplorationAssetReader
{
    internal static ExplorationVisuals Read(JsonElement row)
    {
        Object(row, "presentation", "maps", "sprites", "portraits");
        var maps = new Dictionary<MapId, ExplorationMapVisual>();
        foreach (var map in Array(row, "maps"))
        {
            Object(map, "mapVisual", "map", "atlas", "scale", "blocks");
            var id = new MapId(Id(map, "map"));
            var blocks = Array(map, "blocks").Select(block =>
            {
                Require(block.ValueKind == JsonValueKind.Array && block.GetArrayLength() == 9, "block-tile-count", "presentation.blocks");
                return (IReadOnlyList<ushort>)System.Array.AsReadOnly(block.EnumerateArray().Select(word =>
                {
                    Require(word.ValueKind == JsonValueKind.Number && word.TryGetUInt16(out _), "block-word", "presentation.blocks");
                    return word.GetUInt16();
                }).ToArray());
            }).ToArray();
            Require(blocks.Length is > 0 and <= 1024, "block-count", "presentation.blocks");
            var atlas = Raster(map.GetProperty("atlas"));
            int scale = Number(map, "scale", 1, 4);
            Require(atlas.Width == 128 * scale && atlas.Height == 320 * scale, "map-atlas-shape", "presentation.atlas");
            Require(maps.TryAdd(id, new(id, atlas, scale, System.Array.AsReadOnly(blocks))), "duplicate-map-visual", "presentation.maps");
        }
        var sprites = new Dictionary<int, ExplorationSpriteVisual>();
        foreach (var sprite in Array(row, "sprites"))
        {
            Object(sprite, "spriteVisual", "sprite", "directions", "portrait", "speech");
            int id = Number(sprite, "sprite", 0, 239);
            var directions = Array(sprite, "directions").Select(Raster).ToArray();
            Require(directions.Length == 3 && directions.All(raster => raster.Width == 48 && raster.Height == 24), "sprite-shape", "presentation.sprites");
            int? portrait = sprite.GetProperty("portrait").ValueKind == JsonValueKind.Null ? null : Number(sprite, "portrait", 0, 55);
            Require(sprites.TryAdd(id, new(id, System.Array.AsReadOnly(directions), portrait, Number(sprite, "speech", 0, 255))), "duplicate-sprite-visual", "presentation.sprites");
        }
        var portraits = new Dictionary<int, ExplorationPortraitVisual>();
        foreach (var portrait in Array(row, "portraits"))
        {
            Object(portrait, "portraitVisual", "portrait", "raster");
            int id = Number(portrait, "portrait", 0, 55);
            var raster = Raster(portrait.GetProperty("raster"));
            Require(raster.Width == 64 && raster.Height == 64, "portrait-shape", "presentation.portraits");
            Require(portraits.TryAdd(id, new(id, raster)), "duplicate-portrait", "presentation.portraits");
        }
        Require(sprites.Values.All(sprite => sprite.Portrait is null || portraits.ContainsKey(sprite.Portrait.Value)), "missing-portrait", "presentation.sprites");
        return new(new ReadOnlyDictionary<MapId, ExplorationMapVisual>(maps), new ReadOnlyDictionary<int, ExplorationSpriteVisual>(sprites),
            new ReadOnlyDictionary<int, ExplorationPortraitVisual>(portraits));
    }

    private static ExplorationRaster Raster(JsonElement row)
    {
        Object(row, "raster", "width", "height", "format", "data", "sha256");
        int width = Number(row, "width", 1, 2048), height = Number(row, "height", 1, 2048);
        byte[] bytes;
        try { bytes = Convert.FromBase64String(Text(row, "data")); }
        catch (FormatException) { Require(false, "raster-encoding", "presentation.raster"); throw; }
        Require(bytes.Length is > 0 and <= 8 * 1024 * 1024 &&
            Convert.ToHexString(SHA256.HashData(bytes)).Equals(Text(row, "sha256"), StringComparison.OrdinalIgnoreCase), "raster-identity", "presentation.raster");
        string format = Text(row, "format");
        Require(format is "rgba8" or "png", "raster-format", "presentation.raster");
        if (format == "rgba8") Require(bytes.Length == width * height * 4, "raster-length", "presentation.raster");
        else Require(bytes.Length >= 33 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) &&
            BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4)) == width && BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4)) == height,
            "png-shape", "presentation.raster");
        return new(width, height, format, bytes);
    }
}

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
    internal static ExplorationVisuals Read(JsonElement row, string? worldDirectory = null)
    {
        ObjectOptional(row, "presentation", ["maps", "sprites", "portraits"], "audio", "churchMenu", "fieldMenu", "identityIcons", "itemMenu", "shopMenu", "hdIdentityIcons");
        var maps = new Dictionary<MapId, ExplorationMapVisual>();
        foreach (var map in Array(row, "maps"))
        {
            ObjectOptional(map, "mapVisual", ["map", "atlas", "scale", "blocks"], "music", "hdBlocks");
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
            var music = map.TryGetProperty("music", out _) ? Array(map, "music").Select(value =>
            {
                Object(value, "mapMusic", "field", "battle");
                return new ExplorationMapMusic(Number(value, "field", 0, 64), Number(value, "battle", 0, 64));
            }).ToArray() : [];
            Require(maps.TryAdd(id, new(id, atlas, scale, System.Array.AsReadOnly(blocks), System.Array.AsReadOnly(music),
                OptionalFrames(map, "hdBlocks", blocks.Length, 96, 96))), "duplicate-map-visual", "presentation.maps");
        }
        var sprites = new Dictionary<int, ExplorationSpriteVisual>();
        foreach (var sprite in Array(row, "sprites"))
        {
            ObjectOptional(sprite, "spriteVisual", ["sprite", "directions", "portrait", "speech"], "hdDirections");
            int id = Number(sprite, "sprite", 0, 239);
            var directions = Array(sprite, "directions").Select(Raster).ToArray();
            Require(directions.Length == 3 && directions.All(raster => raster.Width == 48 && raster.Height == 24), "sprite-shape", "presentation.sprites");
            int? portrait = sprite.GetProperty("portrait").ValueKind == JsonValueKind.Null ? null : Number(sprite, "portrait", 0, 55);
            Require(sprites.TryAdd(id, new(id, System.Array.AsReadOnly(directions), portrait, Number(sprite, "speech", 0, 255),
                OptionalFrames(sprite, "hdDirections", 3, 192, 96))), "duplicate-sprite-visual", "presentation.sprites");
        }
        var portraits = new Dictionary<int, ExplorationPortraitVisual>();
        foreach (var portrait in Array(row, "portraits"))
        {
            ObjectOptional(portrait, "portraitVisual", ["portrait", "raster"], "eyes", "mouth", "hdRaster");
            int id = Number(portrait, "portrait", 0, 55);
            var raster = Raster(portrait.GetProperty("raster"));
            Require(raster.Width == 64 && raster.Height == 64, "portrait-shape", "presentation.portraits");
            var eyes = PortraitChanges(portrait, "eyes");
            var mouth = PortraitChanges(portrait, "mouth");
            Require((eyes is null) == (mouth is null), "portrait-animation-binding", "presentation.portraits");
            var hd = portrait.TryGetProperty("hdRaster", out var value) ? Raster(value) : null;
            Require(hd is null || hd.Width == 256 && hd.Height == 256, "hd-portrait-shape", "hdRaster");
            Require(portraits.TryAdd(id, new(id, raster, eyes, mouth, hd)), "duplicate-portrait", "presentation.portraits");
        }
        Require(sprites.Values.All(sprite => sprite.Portrait is null || portraits.ContainsKey(sprite.Portrait.Value)), "missing-portrait", "presentation.sprites");
        var audio = new Dictionary<string, ExplorationAudio>(StringComparer.Ordinal);
        var commands = new HashSet<(int Command, int Timer)>();
        if (row.TryGetProperty("audio", out _))
            foreach (var sound in Array(row, "audio"))
            {
                bool external = sound.TryGetProperty("runtimeWav", out _);
                ObjectOptional(sound, "audio", "modernEndStep", ["cue", "command", "timerB", "sampleRate", "channels", "sampleFrames", external ? "runtimeWav" : "pcm16", "sha256", "loopBegin", "loopEnd"]);
                string cue = Text(sound, "cue");
                int command = Number(sound, "command", 1, 120);
                int timerB = Number(sound, "timerB", 0, 255);
                Require(!string.IsNullOrWhiteSpace(cue) && commands.Add((command, command < 65 ? 0 : timerB)), "audio-command-identity", "presentation.audio");
                int channels = Number(sound, "channels", 1, 2);
                int rate = Number(sound, "sampleRate", 8000, 192000);
                int frames = Number(sound, "sampleFrames", 1, 32 * 1024 * 1024);
                int? begin = sound.GetProperty("loopBegin").ValueKind == JsonValueKind.Null ? null : Number(sound, "loopBegin", 0, frames - 1);
                int? end = sound.GetProperty("loopEnd").ValueKind == JsonValueKind.Null ? null : Number(sound, "loopEnd", 1, frames);
                Require(begin.HasValue == end.HasValue && (begin is null || begin < end), "audio-loop", "presentation.audio");
                int? modernEnd = sound.TryGetProperty("modernEndStep", out _) ? Number(sound, "modernEndStep", 1, 1000000) : null;
                Require(modernEnd is null || command < 65 && begin is null, "finite-music-profile", "presentation.audio");
                byte[] pcm;
                if (external)
                {
                    Require(worldDirectory is not null && Boolean(sound, "runtimeWav"), "private-world-wav", "presentation.audio");
                    pcm = PrivateAudioWave.Read(worldDirectory!, command, timerB, rate, channels, frames);
                }
                else
                {
                    try { pcm = Convert.FromBase64String(Text(sound, "pcm16")); }
                    catch (FormatException) { Require(false, "audio-encoding", "presentation.audio"); throw; }
                }
                string digest = Convert.ToHexString(SHA256.HashData(pcm));
                Require(pcm.Length == frames * channels * 2 &&
                    digest.Equals(Text(sound, "sha256"), StringComparison.OrdinalIgnoreCase),
                    "audio-identity", "presentation.audio");
                Require(audio.TryAdd(cue, new(command, timerB, rate, channels, pcm, digest, begin, end, modernEnd)),
                    "duplicate-audio-cue", "presentation.audio");
            }
        var churchMenu = Menu(row, "churchMenu", ["raise", "cure", "promote", "save"]);
        var fieldMenu = Menu(row, "fieldMenu", ["member", "magic", "item", "search"]);
        var itemMenu = Menu(row, "itemMenu", ["use", "give", "equip", "drop"]);
        var shopMenu = Menu(row, "shopMenu", ["buy", "sell", "repair", "deals"]);
        var identityIcons = IdentityIconReader.Read(row);
        return new(new ReadOnlyDictionary<MapId, ExplorationMapVisual>(maps), new ReadOnlyDictionary<int, ExplorationSpriteVisual>(sprites),
            new ReadOnlyDictionary<int, ExplorationPortraitVisual>(portraits), new ReadOnlyDictionary<string, ExplorationAudio>(audio),
            churchMenu,
            fieldMenu, identityIcons,
            itemMenu, shopMenu, IdentityIconReader.ReadHd(row, identityIcons));
    }

    private static IReadOnlyList<ExplorationRaster>? OptionalFrames(JsonElement owner, string field, int count, int width, int height)
    {
        if (!owner.TryGetProperty(field, out _)) return null;
        var frames = Array(owner, field);
        Require(owner.GetProperty(field).GetArrayLength() == count, "hd-frame-count", field);
        var rasters = frames.Select(Raster).ToArray();
        Require(rasters.All(raster => raster.Width == width && raster.Height == height), "hd-frame-shape", field);
        return System.Array.AsReadOnly(rasters);
    }

    private static IReadOnlyDictionary<string, ExplorationRaster>? Menu(JsonElement row, string property, string[] names)
    {
        if (!row.TryGetProperty(property, out var menu)) return null;
        string[] keys = names.SelectMany(name => new[] { name + ".normal", name + ".selected" }).ToArray();
        Object(menu, "presentation." + property, keys);
        var rasters = keys.ToDictionary(key => key, key => Raster(menu.GetProperty(key)), StringComparer.Ordinal);
        Require(rasters.Values.All(raster => raster.Width == 24 && raster.Height == 24), property[..^4].ToLowerInvariant() + "-menu-shape", "presentation." + property);
        return new ReadOnlyDictionary<string, ExplorationRaster>(rasters);
    }

    internal static void BattlefieldMenu(IReadOnlyDictionary<string, ExplorationRaster> rasters)
    {
        const string prefix = "battlefield-menu.";
        var entries = rasters.Where(row => row.Key.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
        if (entries.Length == 0) return;
        string[] keys = new[] { "member", "map", "speed", "quit" }
            .SelectMany(name => new[] { prefix + name + ".normal", prefix + name + ".selected" }).ToArray();
        Require(entries.Length == keys.Length && keys.All(rasters.ContainsKey) &&
            entries.All(row => row.Value.Width == 24 && row.Value.Height == 24),
            "battlefield-menu-resources", "battleScenes.rasters");
    }

    private static IReadOnlyList<PortraitTileChange>? PortraitChanges(JsonElement portrait, string key)
    {
        if (!portrait.TryGetProperty(key, out _)) return null;
        return System.Array.AsReadOnly(Array(portrait, key).Select(row =>
        {
            Require(row.ValueKind == JsonValueKind.Array && row.GetArrayLength() == 4, "portrait-tile-change", key);
            var values = row.EnumerateArray().Select(value => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int n) && n is >= 0 and <= 7 ? n : -1).ToArray();
            Require(values.All(value => value >= 0), "portrait-tile-coordinate", key);
            return new PortraitTileChange((byte)values[0], (byte)values[1], (byte)values[2], (byte)values[3]);
        }).ToArray());
    }

    internal static ExplorationRaster Raster(JsonElement row)
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

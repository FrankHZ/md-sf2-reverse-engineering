using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Content.Scenarios;

// The current private selection is Battle01. Decode owns format semantics independently of that pin.
public sealed class PrivateBattleEncounterReader
{
    public const string Repository = "https://github.com/ShiningForceCentral/SF2DISASM.git";
    public const string UpstreamCommit = "c834c652b6862bc5679fd7f69a38a7093206efc6";
    public const string PlacementExportDigest = "32EEAE9AFB01DC38A1BAA99EE2E0B4C0B48F8A676F69771A5F5C52A2FC7E4C60";
    public const string SceneExportDigest = "DB9CCC8A40EBC1E0DB23C3EE5BDD67CAF026EF3EBD280B819FAB40E739BAD567";
    public const string CompressedTerrainDigest = "A0E6B0D4F656C7BD893923330148B3F9366CA7D839F5A0676272A2C95DAABC4A";
    private const string PlacementSourceDigest = "DBE89C24807B4577F741FC0EB6EDAE3386F1C47F8712CA2586AE945EDFEDD94B";
    private readonly string _placementPath, _scenePath, _terrainPath;

    public PrivateBattleEncounterReader(string placementPath, string scenePath, string terrainPath)
    {
        foreach (string path in new[] { placementPath, scenePath, terrainPath })
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
                throw new ArgumentException("Explicit fully qualified paths are required for all three private encounter inputs.");
        _placementPath = placementPath; _scenePath = scenePath; _terrainPath = terrainPath;
    }

    public BattleEncounterReadResult Read()
    {
        try
        {
            byte[] placement = ReadInput(_placementPath, PlacementExportDigest, "placement");
            byte[] scene = ReadInput(_scenePath, SceneExportDigest, "scene");
            byte[] stored = ReadInput(_terrainPath, CompressedTerrainDigest, "terrain", 284);
            byte[] terrain;
            try { terrain = StackCompressedGraphicsDecoder.Decode(stored, 2304).Output; }
            catch (InvalidDataException) { throw Invalid("terrain", "The selected terrain failed bounded Stack decoding."); }
            var result = Decode(placement, scene, terrain);
            if (result is BattleEncounterReadAccepted accepted) RequireSelectedSource(accepted.Definition);
            return result;
        }
        catch (EncounterReadException error) { return new BattleEncounterReadRejected(error.Diagnostic); }
    }

    // Same parser used after real private file/decoder admission. It does not assert private file trust.
    internal static BattleEncounterReadResult Decode(byte[] placement, byte[] scene, byte[] terrain)
    {
        try { return new BattleEncounterReadAccepted(Parse(placement, scene, terrain)); }
        catch (EncounterReadException error) { return new BattleEncounterReadRejected(error.Diagnostic); }
        catch (JsonException) { return new BattleEncounterReadRejected(new("document", "The encounter JSON is invalid.")); }
    }

    private static BattleEncounterDefinition Parse(byte[] placementBytes, byte[] sceneBytes, byte[] terrain)
    {
        Require(placementBytes.Length is > 0 and <= 65536, "placement", "The document length is invalid.");
        Require(sceneBytes.Length is > 0 and <= 65536, "scene", "The document length is invalid.");
        Require(terrain.Length == 2304 && terrain.All(value => value <= 8 || value == 255), "terrain", "Retain the complete raw 48-by-48 terrain.");
        using var placementDocument = JsonDocument.Parse(placementBytes, new JsonDocumentOptions { MaxDepth = 32 });
        using var sceneDocument = JsonDocument.Parse(sceneBytes, new JsonDocumentOptions { MaxDepth = 32 });
        var placement = placementDocument.RootElement; var scene = sceneDocument.RootElement;
        Properties(placement, "placement", "schemaVersion", "provenance", "battle", "romRange", "counts", "entities", "aiRegions", "aiPoints");
        Properties(scene, "scene", "schemaVersion", "provenance", "battle", "map", "scene", "terrain");
        Number(placement, "schemaVersion", 1, 1, "placement.schemaVersion");
        Number(scene, "schemaVersion", 1, 1, "scene.schemaVersion");
        var placementSource = Source(placement.GetProperty("provenance"), false);
        var terrainSource = Source(scene.GetProperty("provenance"), true);
        Require(placementSource.Repository == terrainSource.Repository, "provenance.repository", "Source repositories must agree.");
        Require(placementSource.Commit == terrainSource.Commit, "provenance.commit", "Source commits must agree.");
        var battle = Battle(placement.GetProperty("battle"));
        Require(battle == Battle(scene.GetProperty("battle")), "battle", "Placement and scene must select the same battle.");
        var placementRange = Range(placement.GetProperty("romRange"), "romRange");
        var map = scene.GetProperty("map");
        Properties(map, "map", "id", "x", "y", "width", "height", "triggerX", "triggerY");
        var area = new EncounterArea(Byte(map, "id"), Byte(map, "x"), Byte(map, "y"),
            (byte)Number(map, "width", 1, 48, "map.width"), (byte)Number(map, "height", 1, 48, "map.height"), Byte(map, "triggerX"), Byte(map, "triggerY"));
        bool Within(EncounterPoint point) => point.X < area.Width && point.Y < area.Height;
        var selection = scene.GetProperty("scene");
        Properties(selection, "scene.selection", "customBackgroundExpression", "enemyLeaderPresent", "halfExperience");
        var sceneData = new EncounterScene(Text(selection, "customBackgroundExpression"), Boolean(selection, "enemyLeaderPresent"), Boolean(selection, "halfExperience"));
        var counts = placement.GetProperty("counts");
        Properties(counts, "counts", "allies", "enemies", "aiRegions", "aiPoints");
        int allies = Number(counts, "allies", 0, 30, "counts.allies"), enemies = Number(counts, "enemies", 0, 32, "counts.enemies");
        int regionCount = Number(counts, "aiRegions", 0, 16, "counts.aiRegions"), pointCount = Number(counts, "aiPoints", 0, 255, "counts.aiPoints");
        var entities = Array(placement, "entities", allies + enemies).EnumerateArray().Select(entity =>
        {
            Properties(entity, "entities", "id", "kind", "identityExpression", "x", "y", "aiCommandsetExpression", "itemExpression", "behavior");
            var kind = Text(entity, "kind") switch { "ally" => EncounterEntityKind.Ally, "enemy" => EncounterEntityKind.Enemy,
                _ => throw Invalid("entities.kind", "Unknown entity kind.") };
            var behavior = entity.GetProperty("behavior");
            Properties(behavior, "entities.behavior", "primaryOrderExpression", "primaryRegion", "secondaryOrderExpression", "secondaryRegion", "filler", "spawnExpression");
            return new EncounterPlacement(Byte(entity, "id"), kind, Text(entity, "identityExpression"), Position(entity),
                Text(entity, "aiCommandsetExpression"), Text(entity, "itemExpression"),
                new(Text(behavior, "primaryOrderExpression"), Byte(behavior, "primaryRegion"), Text(behavior, "secondaryOrderExpression"),
                    Byte(behavior, "secondaryRegion"), Byte(behavior, "filler"), Text(behavior, "spawnExpression")));
        }).ToArray();
        Require(entities.Count(entity => entity.Kind == EncounterEntityKind.Ally) == allies, "counts.allies", "Declared side counts must match the entities.");
        Require(entities.Count(entity => entity.Kind == EncounterEntityKind.Enemy) == enemies, "counts.enemies", "Declared side counts must match the entities.");
        Require(entities.Select(entity => entity.Id).Distinct().Count() == entities.Length, "entities", "Entity IDs must be unique.");
        // Only this literal establishes deployed geometry. Other spawn expressions and their raw
        // coordinates survive for a future resolver; importing them does not execute a spawn rule.
        var starting = entities.Where(entity => entity.Behavior.SpawnExpression == "STARTING").ToArray();
        Require(starting.All(entity => Within(entity.Position)), "entities", "STARTING placements must lie within the battle area.");
        Require(starting.Select(entity => entity.Position).Distinct().Count() == starting.Length, "entities", "STARTING placements must not overlap.");
        var regions = Array(placement, "aiRegions", regionCount).EnumerateArray().Select(region =>
        {
            Properties(region, "aiRegions", "id", "vertexCount", "unknown", "vertices", "trailingBytes");
            Number(region, "vertexCount", 4, 4, "aiRegions.vertexCount");
            var vertices = Array(region, "vertices", 4).EnumerateArray().Select(point =>
            { Properties(point, "aiRegions.vertices", "x", "y"); return Position(point); }).ToArray();
            Require(vertices.All(Within) && vertices.Distinct().Count() == 4 &&
                Cross(vertices[0], vertices[1], vertices[3]) != 0 && Cross(vertices[2], vertices[1], vertices[3]) != 0,
                "regions", "Regions require four distinct bounded vertices and two nondegenerate triangles.");
            var trailing = Array(region, "trailingBytes", 2).EnumerateArray().Select(value => ByteValue(value, "aiRegions.trailingBytes")).ToArray();
            return new EncounterRegion((byte)Number(region, "id", 0, 15, "aiRegions.id"), Byte(region, "unknown"), vertices, trailing[0], trailing[1]);
        }).ToArray();
        Require(regions.Select(region => region.Id).Distinct().Count() == regions.Length, "regions", "Region IDs must be unique.");
        Require(entities.All(entity => ValidRegion(entity.Behavior.PrimaryRegion) && ValidRegion(entity.Behavior.SecondaryRegion)),
            "entities", "An assigned region must exist or be the absent-region value15.");
        bool ValidRegion(byte id) => id == 15 || regions.Any(region => region.Id == id);
        var points = Array(placement, "aiPoints", pointCount).EnumerateArray().Select(point =>
        { Properties(point, "aiPoints", "x", "y"); return Position(point); }).ToArray();
        Require(points.All(Within), "aiPoints", "AI points must lie within the battle area.");
        Require(placementRange.LengthBytes == 4 + entities.Length * 12 + regions.Length * 12 + points.Length * 2,
            "romRange", "The source range must match the declared record lengths.");
        var metadata = scene.GetProperty("terrain");
        Properties(metadata, "terrain", "compressedRange", "compressedSha256", "decompressedWidth", "decompressedHeight", "decompressedLengthBytes", "decompressedSha256", "valueCounts");
        var compressedRange = Range(metadata.GetProperty("compressedRange"), "terrain.compressedRange");
        string compressedHash = Digest(metadata, "compressedSha256"), terrainHash = Digest(metadata, "decompressedSha256");
        Require(compressedHash.Equals(terrainSource.SourceSha256, StringComparison.OrdinalIgnoreCase), "terrain.compressedSha256", "Compressed identity must join its source.");
        Number(metadata, "decompressedWidth", 48, 48, "terrain.decompressedWidth");
        Number(metadata, "decompressedHeight", 48, 48, "terrain.decompressedHeight");
        Number(metadata, "decompressedLengthBytes", 2304, 2304, "terrain.decompressedLengthBytes");
        Require(terrainHash.Equals(Convert.ToHexString(SHA256.HashData(terrain)), StringComparison.OrdinalIgnoreCase), "terrain.decompressedSha256", "Decoded terrain identity drifted.");
        var valueCounts = terrain.GroupBy(value => value).ToDictionary(group => group.Key, group => group.Count());
        var declaredCounts = metadata.GetProperty("valueCounts");
        Properties(declaredCounts, "terrain.valueCounts", valueCounts.Keys.Select(value => value.ToString(CultureInfo.InvariantCulture)).ToArray());
        foreach (var (value, count) in valueCounts) Number(declaredCounts, value.ToString(CultureInfo.InvariantCulture), count, count, "terrain.valueCounts");
        return new(placementSource, terrainSource, battle, placementRange, area, sceneData, entities, regions, points,
            compressedRange, compressedHash, terrainHash, terrain, valueCounts);
    }

    internal static void RequireSelectedSource(BattleEncounterDefinition definition)
    {
        foreach (var source in new[] { definition.PlacementSource, definition.TerrainSource })
        {
            Require(source.Repository == Repository, "provenance.repository", "The selected repository drifted.");
            Require(source.Commit == UpstreamCommit, "provenance.commit", "The selected commit drifted.");
        }
        Require(definition.PlacementSource.SourcePath == "data/battles/spritesets/spriteset01.asm", "provenance.sourcePath", "The selected placement source drifted.");
        Require(definition.PlacementSource.SourceSha256 == PlacementSourceDigest, "provenance.sourceSha256", "The selected placement identity drifted.");
        Require(definition.TerrainSource.SourcePath == "data/battles/entries/battle01/terrain.bin", "provenance.terrainSourcePath", "The selected terrain source drifted.");
        Require(definition.TerrainSource.SourceSha256 == CompressedTerrainDigest, "provenance.terrainSourceSha256", "The selected terrain identity drifted.");
        Require(definition.PlacementRange == new EncounterRange(1782498, 1782646, 148), "romRange", "The selected placement range drifted.");
        Require(definition.CompressedRange == new EncounterRange(1758020, 1758304, 284), "terrain.compressedRange", "The selected terrain range drifted.");
    }

    private static byte[] ReadInput(string path, string digest, string field, int? expectedLength = null)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Require(stream.Length is > 0 and <= 65536 && (expectedLength is null || stream.Length == expectedLength), field, "The selected input length is invalid.");
            byte[] bytes = new byte[(int)stream.Length]; stream.ReadExactly(bytes);
            Require(Convert.ToHexString(SHA256.HashData(bytes)) == digest, field, "The fixed selected input identity drifted.");
            return bytes;
        }
        catch (IOException) { throw Invalid(field, "The required private encounter input is unavailable."); }
        catch (UnauthorizedAccessException) { throw Invalid(field, "The required private encounter input cannot be read."); }
    }

    private static EncounterSource Source(JsonElement source, bool terrain)
    {
        string path = terrain ? "terrainSourcePath" : "sourcePath", hash = terrain ? "terrainSourceSha256" : "sourceSha256";
        Properties(source, "provenance", "repository", "commit", path, hash);
        string commit = Text(source, "commit");
        Require(commit.Length == 40 && commit.All(Uri.IsHexDigit), "provenance.commit", "A complete source revision is required.");
        return new(Text(source, "repository"), commit, Text(source, path), Digest(source, hash));
    }
    private static EncounterIdentity Battle(JsonElement battle)
    { Properties(battle, "battle", "id", "code"); return new(Byte(battle, "id"), Text(battle, "code")); }
    private static EncounterRange Range(JsonElement range, string field)
    {
        Properties(range, field, "start", "endExclusive", "lengthBytes");
        int start = Number(range, "start", 0, int.MaxValue, field), end = Number(range, "endExclusive", start, int.MaxValue, field);
        int length = Number(range, "lengthBytes", 1, int.MaxValue, field);
        Require(end - start == length, field, "Range endpoints and length must agree."); return new(start, end, length);
    }
    private static string Digest(JsonElement parent, string key)
    { string value = Text(parent, key); Require(value.Length == 64 && value.All(Uri.IsHexDigit), key, "A complete SHA256 identity is required."); return value; }
    private static void Properties(JsonElement value, string field, params string[] required)
    {
        Require(value.ValueKind == JsonValueKind.Object, field, "An object is required.");
        var names = value.EnumerateObject().Select(property => property.Name).ToArray();
        Require(names.Length == required.Length && names.Distinct(StringComparer.Ordinal).Count() == names.Length &&
            names.ToHashSet(StringComparer.Ordinal).SetEquals(required), field, "The closed schema properties are invalid.");
    }
    private static JsonElement Array(JsonElement parent, string key, int count)
    { var value = parent.GetProperty(key); Require(value.ValueKind == JsonValueKind.Array && value.GetArrayLength() == count, key, "The array must match its declared count."); return value; }
    private static string Text(JsonElement parent, string key)
    { var value = parent.GetProperty(key); Require(value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString()), key, "A nonempty string is required."); return value.GetString()!; }
    private static bool Boolean(JsonElement parent, string key)
    { var value = parent.GetProperty(key); Require(value.ValueKind is JsonValueKind.True or JsonValueKind.False, "scene.selection", "A boolean is required."); return value.GetBoolean(); }
    private static int Number(JsonElement parent, string key, int minimum, int maximum, string field)
    { var value = parent.GetProperty(key); if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int number) || number < minimum || number > maximum) throw Invalid(field, "The numeric value is invalid."); return number; }
    private static byte Byte(JsonElement parent, string key) => ByteValue(parent.GetProperty(key), key);
    private static byte ByteValue(JsonElement value, string field) => value.ValueKind == JsonValueKind.Number && value.TryGetByte(out byte result)
        ? result : throw Invalid(field, "An unsigned byte is required.");
    private static EncounterPoint Position(JsonElement point) => new(Byte(point, "x"), Byte(point, "y"));
    private static int Cross(EncounterPoint a, EncounterPoint b, EncounterPoint c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    private static void Require(bool condition, string field, string message) { if (!condition) throw Invalid(field, message); }
    private static EncounterReadException Invalid(string field, string message) => new(new(field, message));
    internal sealed class EncounterReadException(BattleEncounterDiagnostic diagnostic) : Exception
    { internal BattleEncounterDiagnostic Diagnostic { get; } = diagnostic; }
}

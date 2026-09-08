using System.Security.Cryptography;
using System.Text.Json;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Content;

public sealed class PrivateOriginalBattle01StartupReader : IOriginalBattle01StartupSource
{
    public const string CompressedTerrainDigest = "A0E6B0D4F656C7BD893923330148B3F9366CA7D839F5A0676272A2C95DAABC4A";
    private const string PlacementSourceDigest = "DBE89C24807B4577F741FC0EB6EDAE3386F1C47F8712CA2586AE945EDFEDD94B";
    private readonly string _placementPath;
    private readonly string _scenePath;
    private readonly string _terrainPath;

    public PrivateOriginalBattle01StartupReader(string placementPath, string scenePath, string terrainPath)
    {
        foreach (string path in new[] { placementPath, scenePath, terrainPath })
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
                throw new ArgumentException("Explicit fully qualified paths are required for all three private startup inputs.");
        _placementPath = placementPath; _scenePath = scenePath; _terrainPath = terrainPath;
    }

    public OriginalBattle01StartupImportResult Admit()
    {
        try
        {
            byte[] placement = ReadInput(_placementPath, OriginalBattle01StartupDefinition.PlacementExportDigest, "placement");
            byte[] scene = ReadInput(_scenePath, OriginalBattle01StartupDefinition.SceneExportDigest, "scene");
            byte[] storedTerrain = ReadInput(_terrainPath, CompressedTerrainDigest, "terrain", expectedLength: 284);
            byte[] terrain;
            try { terrain = StackCompressedGraphicsDecoder.Decode(storedTerrain, OriginalBattle01StartupDefinition.TerrainLength).Output; }
            catch (InvalidDataException) { throw Invalid("terrain", "The selected terrain failed bounded Stack decoding."); }
            OriginalBattle01StartupDefinition definition = Parse(placement, scene, terrain);
            return definition.GetAdmissionDiagnostic() is { } diagnostic
                ? new OriginalBattle01StartupImportRejected(diagnostic) : new OriginalBattle01StartupImported(definition);
        }
        catch (StartupReadException error) { return new OriginalBattle01StartupImportRejected(error.Diagnostic); }
        catch (JsonException) { return new OriginalBattle01StartupImportRejected(new("document", "The startup JSON is invalid.")); }
        catch (ArgumentException error) { return new OriginalBattle01StartupImportRejected(new(error.ParamName ?? "definition", "The startup semantic definition is invalid.")); }
    }

    // Exercises schema/joins and typed semantics using authored payloads; never a production import port.
    internal static OriginalBattle01StartupImportResult AdmitSemanticDocumentsForTests(
        byte[] placement, byte[] scene, byte[] decodedTerrain)
    {
        try { return new OriginalBattle01StartupImported(Parse(placement, scene, decodedTerrain)); }
        catch (StartupReadException error) { return new OriginalBattle01StartupImportRejected(error.Diagnostic); }
        catch (JsonException) { return new OriginalBattle01StartupImportRejected(new("document", "The startup JSON is invalid.")); }
        catch (ArgumentException error) { return new OriginalBattle01StartupImportRejected(new(error.ParamName ?? "definition", "The startup semantic definition is invalid.")); }
    }

    private static OriginalBattle01StartupDefinition Parse(byte[] placementBytes, byte[] sceneBytes, byte[] terrain)
    {
        using var placementDocument = JsonDocument.Parse(placementBytes);
        using var sceneDocument = JsonDocument.Parse(sceneBytes);
        var placement = placementDocument.RootElement;
        var scene = sceneDocument.RootElement;
        Properties(placement, "placement", "schemaVersion", "provenance", "battle", "romRange", "counts", "entities", "aiRegions", "aiPoints");
        Properties(scene, "scene", "schemaVersion", "provenance", "battle", "map", "scene", "terrain");
        ExactInt(placement, "schemaVersion", 1, "placement.schemaVersion");
        ExactInt(scene, "schemaVersion", 1, "scene.schemaVersion");
        ReadProvenance(placement.GetProperty("provenance"), false);
        ReadProvenance(scene.GetProperty("provenance"), true);
        ReadBattle(placement.GetProperty("battle")); ReadBattle(scene.GetProperty("battle"));
        ReadRange(placement.GetProperty("romRange"), "romRange", 1782498, 1782646, 148);
        var counts = placement.GetProperty("counts");
        Properties(counts, "counts", "allies", "enemies", "aiRegions", "aiPoints");
        ExactInt(counts, "allies", 3, "counts.allies"); ExactInt(counts, "enemies", 6, "counts.enemies");
        ExactInt(counts, "aiRegions", 3, "counts.aiRegions"); ExactInt(counts, "aiPoints", 0, "counts.aiPoints");
        var sourceEntities = Array(placement, "entities", 9);
        var entities = new List<OriginalBattle01Placement>();
        foreach (var entity in sourceEntities.EnumerateArray())
        {
            Properties(entity, "entities", "id", "kind", "identityExpression", "x", "y", "aiCommandsetExpression", "itemExpression", "behavior");
            var kind = Text(entity, "kind") switch {
                "ally" => OriginalBattle01EntityKind.Ally, "enemy" => OriginalBattle01EntityKind.Enemy,
                _ => throw Invalid("entities.kind", "Unsupported placement kind.") };
            string identityText = Text(entity, "identityExpression");
            byte identity = (kind, identityText) switch {
                (OriginalBattle01EntityKind.Ally, "0") => 0, (OriginalBattle01EntityKind.Ally, "1") => 1,
                (OriginalBattle01EntityKind.Ally, "2") => 2, (OriginalBattle01EntityKind.Enemy, "GIZMO") => 39,
                _ => throw Invalid("entities.identityExpression", "Unsupported selected combatant identity.") };
            var ai = Text(entity, "aiCommandsetExpression") switch {
                "HEALER1" => OriginalBattle01AiCommandSet.Healer1, "ATTACKER1" => OriginalBattle01AiCommandSet.Attacker1,
                "ATTACKER2" => OriginalBattle01AiCommandSet.Attacker2,
                _ => throw Invalid("entities.aiCommandsetExpression", "Unsupported selected AI commandset.") };
            ExactText(entity, "itemExpression", "NOTHING", "entities.itemExpression");
            var behavior = entity.GetProperty("behavior");
            Properties(behavior, "entities.behavior", "primaryOrderExpression", "primaryRegion", "secondaryOrderExpression", "secondaryRegion", "filler", "spawnExpression");
            ExactText(behavior, "primaryOrderExpression", "NONE", "entities.primaryOrder");
            ExactText(behavior, "secondaryOrderExpression", "NONE", "entities.secondaryOrder");
            ExactText(behavior, "spawnExpression", "STARTING", "entities.spawn");
            entities.Add(new(Byte(entity, "id"), kind, identity, Position(entity), ai, 127, 255,
                Byte(behavior, "primaryRegion"), 255, Byte(behavior, "secondaryRegion"), Byte(behavior, "filler"), OriginalBattle01Spawn.Starting));
        }
        var regions = new List<OriginalBattle01AiRegion>();
        foreach (var region in Array(placement, "aiRegions", 3).EnumerateArray())
        {
            Properties(region, "aiRegions", "id", "vertexCount", "unknown", "vertices", "trailingBytes");
            ExactInt(region, "vertexCount", 4, "aiRegions.vertexCount");
            var vertices = Array(region, "vertices", 4).EnumerateArray().Select(vertex => {
                Properties(vertex, "aiRegions.vertices", "x", "y"); return Position(vertex); }).ToArray();
            var trailing = Array(region, "trailingBytes", 2).EnumerateArray().Select(value => ByteValue(value, "aiRegions.trailingBytes")).ToArray();
            regions.Add(new(Byte(region, "id"), Byte(region, "unknown"), vertices, trailing[0], trailing[1]));
        }
        Array(placement, "aiPoints", 0);
        var map = scene.GetProperty("map");
        Properties(map, "map", "id", "x", "y", "width", "height", "triggerX", "triggerY");
        foreach (var (key, value) in new[] { ("id", 57), ("x", 0), ("y", 0), ("width", 16), ("height", 20), ("triggerX", 255), ("triggerY", 255) })
            ExactInt(map, key, value, "map." + key);
        var selection = scene.GetProperty("scene");
        Properties(selection, "scene.selection", "customBackgroundExpression", "enemyLeaderPresent", "halfExperience");
        ExactText(selection, "customBackgroundExpression", "TOWER_INTERIOR", "scene.customBackgroundExpression");
        if (selection.GetProperty("enemyLeaderPresent").ValueKind != JsonValueKind.False || selection.GetProperty("halfExperience").ValueKind != JsonValueKind.True)
            throw Invalid("scene.selection", "The selected global scene flags drifted.");
        var metadata = scene.GetProperty("terrain");
        Properties(metadata, "terrain", "compressedRange", "compressedSha256", "decompressedWidth", "decompressedHeight", "decompressedLengthBytes", "decompressedSha256", "valueCounts");
        ReadRange(metadata.GetProperty("compressedRange"), "terrain.compressedRange", 1758020, 1758304, 284);
        ExactText(metadata, "compressedSha256", CompressedTerrainDigest, "terrain.compressedSha256");
        ExactInt(metadata, "decompressedWidth", 48, "terrain.decompressedWidth");
        ExactInt(metadata, "decompressedHeight", 48, "terrain.decompressedHeight");
        ExactInt(metadata, "decompressedLengthBytes", 2304, "terrain.decompressedLengthBytes");
        ExactText(metadata, "decompressedSha256", Convert.ToHexString(SHA256.HashData(terrain)), "terrain.decompressedSha256");
        var valueCounts = metadata.GetProperty("valueCounts");
        var actualCounts = terrain.GroupBy(value => value).ToDictionary(group => group.Key.ToString(System.Globalization.CultureInfo.InvariantCulture), group => group.Count());
        Properties(valueCounts, "terrain.valueCounts", actualCounts.Keys.ToArray());
        foreach (var (key, count) in actualCounts) ExactInt(valueCounts, key, count, "terrain.valueCounts");
        return new(new(OriginalBattle01StartupDefinition.Repository, OriginalBattle01StartupDefinition.UpstreamCommit,
            OriginalBattle01StartupDefinition.PlacementExportDigest, OriginalBattle01StartupDefinition.SceneExportDigest), entities, regions, terrain);
    }

    private static void ReadProvenance(JsonElement provenance, bool terrain)
    {
        string pathField = terrain ? "terrainSourcePath" : "sourcePath";
        string hashField = terrain ? "terrainSourceSha256" : "sourceSha256";
        Properties(provenance, "provenance", "repository", "commit", pathField, hashField);
        ExactText(provenance, "repository", OriginalBattle01StartupDefinition.Repository, "provenance.repository");
        ExactText(provenance, "commit", OriginalBattle01StartupDefinition.UpstreamCommit, "provenance.commit");
        ExactText(provenance, pathField, terrain ? "data/battles/entries/battle01/terrain.bin" : "data/battles/spritesets/spriteset01.asm", "provenance." + pathField);
        ExactText(provenance, hashField, terrain ? CompressedTerrainDigest : PlacementSourceDigest, "provenance." + hashField);
    }
    private static void ReadBattle(JsonElement battle)
    {
        Properties(battle, "battle", "id", "code"); ExactInt(battle, "id", 1, "battle.id");
        ExactText(battle, "code", "INSIDE_ANCIENT_TOWER", "battle.code");
    }
    private static void ReadRange(JsonElement range, string field, int start, int end, int length)
    {
        Properties(range, field, "start", "endExclusive", "lengthBytes");
        ExactInt(range, "start", start, field); ExactInt(range, "endExclusive", end, field); ExactInt(range, "lengthBytes", length, field);
    }
    private static byte[] ReadInput(string path, string digest, string field, int? expectedLength = null)
    {
        try
        {
            long length = new FileInfo(path).Length;
            if (length <= 0 || length > 65536 || (expectedLength is not null && length != expectedLength))
                throw Invalid(field, "The selected input length is invalid.");
            byte[] bytes = File.ReadAllBytes(path);
            if (Convert.ToHexString(SHA256.HashData(bytes)) != digest)
                throw Invalid(field, "The fixed selected input identity drifted.");
            return bytes;
        }
        catch (IOException) { throw Invalid(field, "The required private startup input is unavailable."); }
        catch (UnauthorizedAccessException) { throw Invalid(field, "The required private startup input cannot be read."); }
    }
    private static void Properties(JsonElement value, string field, params string[] required)
    {
        if (value.ValueKind != JsonValueKind.Object) throw Invalid(field, "An object is required.");
        var names = value.EnumerateObject().Select(property => property.Name).ToArray();
        if (names.Length != required.Length || names.Distinct(StringComparer.Ordinal).Count() != names.Length ||
            !names.ToHashSet(StringComparer.Ordinal).SetEquals(required))
            throw Invalid(field, "The selected schema properties drifted.");
    }
    private static JsonElement Array(JsonElement parent, string key, int count)
    {
        var value = parent.GetProperty(key);
        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() != count)
            throw Invalid(key, "The selected array count drifted.");
        return value;
    }
    private static string Text(JsonElement parent, string key) => parent.GetProperty(key).ValueKind == JsonValueKind.String
        ? parent.GetProperty(key).GetString()! : throw Invalid(key, "A string is required.");
    private static void ExactText(JsonElement parent, string key, string expected, string field)
    {
        if (Text(parent, key) != expected) throw Invalid(field, "The selected source value drifted.");
    }
    private static void ExactInt(JsonElement parent, string key, int expected, string field)
    {
        if (parent.GetProperty(key).ValueKind != JsonValueKind.Number ||
            !parent.GetProperty(key).TryGetInt32(out int actual) || actual != expected)
            throw Invalid(field, "The selected numeric source value drifted.");
    }
    private static byte Byte(JsonElement parent, string key) => ByteValue(parent.GetProperty(key), key);
    private static byte ByteValue(JsonElement value, string field) => value.ValueKind == JsonValueKind.Number && value.TryGetByte(out byte result)
        ? result : throw Invalid(field, "An unsigned byte is required.");
    private static MapPosition Position(JsonElement parent) => new(Byte(parent, "x"), Byte(parent, "y"));
    private static StartupReadException Invalid(string field, string message) => new(new(field, message));
    private sealed class StartupReadException(OriginalBattle01StartupDiagnostic diagnostic) : Exception
    {
        public OriginalBattle01StartupDiagnostic Diagnostic { get; } = diagnostic;
    }
}

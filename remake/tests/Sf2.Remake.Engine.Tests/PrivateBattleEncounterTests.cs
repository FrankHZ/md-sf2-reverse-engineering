using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class PrivateBattleEncounterTests
{
    [Fact]
    public void IndependentEncounterRetainsExpressionsSourceJoinsAndDeclaredGeometry()
    {
        var (placement, scene, terrain) = Sample();
        var result = Accepted(placement, scene, terrain);
        Assert.Equal(new EncounterIdentity(7, "AUTHORED_ENCOUNTER"), result.Battle);
        Assert.Equal(new EncounterArea(22, 10, 12, 12, 11, 255, 0), result.Area);
        Assert.Equal(new EncounterScene("UNRESOLVED_BACKGROUND", true, false), result.Scene);
        Assert.Equal(new byte[] { 11, 7, 4 }, result.Placements.Select(p => p.Id));
        Assert.Equal((2, 1, 1, 2), (result.AllyCount, result.EnemyCount, result.Regions.Count, result.AiPoints.Count));
        var enemy = result.Placements[1];
        Assert.Equal("UNRESOLVED_ENEMY + 1", enemy.IdentityExpression);
        Assert.Equal("UNKNOWN_AI", enemy.AiCommandsetExpression);
        Assert.Equal("WEAPON|EQUIPPED", enemy.ItemExpression);
        Assert.Equal(new EncounterBehavior("MOVE_TO_POINT_2", 6, "NONE", 15, 96, "STARTING"), enemy.Behavior);
        Assert.Equal(new EncounterPoint(3, 2), enemy.Position);
        Assert.Equal((6, 211, 12, 255), ((int)result.Regions[0].Id, result.Regions[0].Unknown,
            result.Regions[0].TrailingByte0, result.Regions[0].TrailingByte1));
        Assert.Equal(new EncounterPoint[] { new(1, 2), new(8, 7) }, result.AiPoints);
        Assert.Equal(new EncounterRange(1000, 1056, 56), result.PlacementRange);
        Assert.Equal(new EncounterRange(2000, 2010, 10), result.CompressedRange);
        Assert.Equal("authored/placement.asm", result.PlacementSource.SourcePath);
        Assert.Equal("authored/terrain.bin", result.TerrainSource.SourcePath);
        Assert.Equal(new string('a', 40), result.PlacementSource.Commit);
        Assert.Equal("https://example.invalid/source", result.PlacementSource.Repository);
        Assert.Equal(new string('b', 64), result.PlacementSource.SourceSha256);
        Assert.Equal(new string('c', 64), result.CompressedSha256);
        Assert.Equal(result.CompressedSha256, result.TerrainSource.SourceSha256);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(terrain)), result.TerrainSha256);
        Assert.Equal((48, 48, 2304), (result.TerrainWidth, result.TerrainHeight, result.Terrain.Count));
        Assert.Equal(terrain, result.Terrain);
        Assert.Equal(2172, result.TerrainValueCounts[255]); Assert.Equal(131, result.TerrainValueCounts[2]);
        Assert.Equal(1, result.TerrainValueCounts[3]);
    }

    [Fact]
    public void CountsReferencesPositionsAndBattleSelectionComeFromData()
    {
        var (placement, scene, terrain) = Sample();
        placement["battle"]!["id"] = 19; scene["battle"]!["id"] = 19;
        placement["entities"]!.AsArray().RemoveAt(0); placement["counts"]!["allies"] = 1;
        placement["entities"]![1]!["id"] = 29; placement["entities"]![1]!["x"] = 10;
        placement["aiRegions"]![0]!["id"] = 3;
        foreach (var entity in placement["entities"]!.AsArray()) entity!["behavior"]!["primaryRegion"] = 3;
        placement["aiPoints"]!.AsArray().RemoveAt(0); placement["counts"]!["aiPoints"] = 1;
        placement["romRange"]!["endExclusive"] = 1042; placement["romRange"]!["lengthBytes"] = 42;
        var definition = Accepted(placement, scene, terrain);
        Assert.Equal((19, 1, 1), ((int)definition.Battle.Id, definition.AllyCount, definition.EnemyCount));
        Assert.Equal(new byte[] { 7, 29 }, definition.Placements.Select(p => p.Id));
        Assert.Equal(new EncounterPoint(10, 3), definition.Placements[1].Position);
        Assert.Equal(3, Assert.Single(definition.Regions).Id);
        Assert.Equal(new EncounterPoint(8, 7), Assert.Single(definition.AiPoints));
    }

    [Fact]
    public void UnresolvedSpawnKeepsRawCoordinatesWithoutInventingDeployment()
    {
        var (placement, scene, terrain) = Sample();
        placement["entities"]![1]!["behavior"]!["spawnExpression"] = "UNRESOLVED_SPAWN";
        placement["entities"]![1]!["x"] = 255; placement["entities"]![1]!["y"] = 255;
        var record = Accepted(placement, scene, terrain).Placements[1];
        Assert.Equal(new EncounterPoint(255, 255), record.Position);
        Assert.Equal("UNRESOLVED_SPAWN", record.Behavior.SpawnExpression);
    }

    [Fact]
    public void DefinitionOwnsItsCollectionsAfterCallerInputsChange()
    {
        var (placement, scene, terrain) = Sample(); var result = Accepted(placement, scene, terrain);
        terrain[0] = 8; placement["entities"]![0]!["x"] = 10; placement["aiRegions"]![0]!["vertices"]![0]!["x"] = 2;
        Assert.Equal(2, result.Terrain[0]); Assert.Equal(new EncounterPoint(1, 1), result.Placements[0].Position);
        Assert.Equal(new EncounterPoint(0, 0), result.Regions[0].Vertices[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<byte>)result.Terrain)[0] = 8);
        Assert.Throws<NotSupportedException>(() => ((IList<EncounterPoint>)result.Regions[0].Vertices)[0] = new(9, 9));
        Assert.Throws<NotSupportedException>(() => ((IList<EncounterPlacement>)result.Placements).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<EncounterPoint>)result.AiPoints).Clear());
        Assert.Throws<NotSupportedException>(() => ((IDictionary<byte, int>)result.TerrainValueCounts)[2] = 0);
    }

    [Theory]
    [InlineData("count", "entities")]
    [InlineData("side", "counts.allies")]
    [InlineData("duplicate-id", "entities")]
    [InlineData("overlap", "entities")]
    [InlineData("outside", "entities")]
    [InlineData("region-ref", "entities")]
    [InlineData("degenerate", "regions")]
    [InlineData("region-outside", "regions")]
    [InlineData("point-outside", "aiPoints")]
    [InlineData("range", "romRange")]
    [InlineData("record-length", "romRange")]
    [InlineData("battle-join", "battle")]
    [InlineData("repository-join", "provenance.repository")]
    [InlineData("revision-join", "provenance.commit")]
    [InlineData("hash-join", "terrain.compressedSha256")]
    [InlineData("terrain-hash", "terrain.decompressedSha256")]
    [InlineData("terrain-count", "terrain.valueCounts")]
    [InlineData("terrain-value", "terrain")]
    [InlineData("terrain-length", "terrain")]
    [InlineData("width", "terrain.decompressedWidth")]
    [InlineData("extra", "entities.behavior")]
    [InlineData("missing", "map")]
    [InlineData("schema", "placement.schemaVersion")]
    [InlineData("string-byte", "filler")]
    [InlineData("empty-expression", "identityExpression")]
    public void InvalidRecordsRejectAtTheirOwningField(string shape, string field)
    {
        var (placement, scene, terrain) = Sample(); var entities = placement["entities"]!;
        switch (shape)
        {
            case "count": placement["counts"]!["enemies"] = 2; break;
            case "side": entities[1]!["kind"] = "ally"; break;
            case "duplicate-id": entities[1]!["id"] = 11; break;
            case "overlap": entities[1]!["x"] = 1; entities[1]!["y"] = 1; break;
            case "outside": entities[1]!["x"] = 12; break;
            case "region-ref": entities[1]!["behavior"]!["primaryRegion"] = 4; break;
            case "degenerate": placement["aiRegions"]![0]!["vertices"]![1]!["x"] = 0; break;
            case "region-outside": placement["aiRegions"]![0]!["vertices"]![1]!["x"] = 12; break;
            case "point-outside": placement["aiPoints"]![0]!["y"] = 11; break;
            case "range": placement["romRange"]!["endExclusive"] = 1057; break;
            case "record-length": placement["romRange"]!["endExclusive"] = 1057; placement["romRange"]!["lengthBytes"] = 57; break;
            case "battle-join": scene["battle"]!["code"] = "OTHER"; break;
            case "repository-join": scene["provenance"]!["repository"] = "https://other.invalid"; break;
            case "revision-join": scene["provenance"]!["commit"] = new string('d', 40); break;
            case "hash-join": scene["terrain"]!["compressedSha256"] = new string('d', 64); break;
            case "terrain-hash": terrain[0] = 3; break;
            case "terrain-count": scene["terrain"]!["valueCounts"]!["2"] = 132; break;
            case "terrain-value": terrain[0] = 9; break;
            case "terrain-length": terrain = terrain[..2303]; break;
            case "width": scene["terrain"]!["decompressedWidth"] = 12; break;
            case "extra": entities[0]!["behavior"]!["ignored"] = true; break;
            case "missing": scene["map"]!.AsObject().Remove("height"); break;
            case "schema": placement["schemaVersion"] = 2; break;
            case "string-byte": entities[0]!["behavior"]!["filler"] = "1"; break;
            case "empty-expression": entities[0]!["identityExpression"] = " "; break;
        }
        Assert.Equal(field, Assert.IsType<BattleEncounterReadRejected>(Read(placement, scene, terrain)).Diagnostic.Field);
    }

    [Fact]
    public void DuplicatePropertiesRejectBeforeValueSelectionAtEveryObjectBoundary()
    {
        var (placement, scene, terrain) = Sample();
        foreach (var (token, field) in new[] { ("\"schemaVersion\":1", "placement"), ("\"filler\":96", "entities.behavior") })
        {
            string json = placement.ToJsonString().Replace(token, token + "," + token, StringComparison.Ordinal);
            var result = PrivateBattleEncounterReader.Decode(Encoding.UTF8.GetBytes(json), Encoding.UTF8.GetBytes(scene.ToJsonString()), terrain);
            Assert.Equal(field, Assert.IsType<BattleEncounterReadRejected>(result).Diagnostic.Field);
        }
    }

    [Fact]
    public void PrivateFilePortRejectsAbsentAuthoredAndOversizeBytesWithoutPathsOrFallback()
    {
        string file = Path.GetTempFileName();
        try
        {
            var absent = new PrivateBattleEncounterReader(file + ".missing", file, file).Read();
            var diagnostic = Assert.IsType<BattleEncounterReadRejected>(absent).Diagnostic;
            Assert.Equal("placement", diagnostic.Field); Assert.DoesNotContain(file, diagnostic.Message, StringComparison.Ordinal);
            var (placement, _, _) = Sample(); File.WriteAllText(file, placement.ToJsonString());
            diagnostic = Assert.IsType<BattleEncounterReadRejected>(new PrivateBattleEncounterReader(file, file, file).Read()).Diagnostic;
            Assert.Contains("identity", diagnostic.Message, StringComparison.Ordinal); Assert.DoesNotContain(file, diagnostic.Message, StringComparison.Ordinal);
            File.WriteAllBytes(file, new byte[65537]);
            diagnostic = Assert.IsType<BattleEncounterReadRejected>(new PrivateBattleEncounterReader(file, file, file).Read()).Diagnostic;
            Assert.Contains("length", diagnostic.Message, StringComparison.Ordinal);
            Assert.Throws<ArgumentException>(() => new PrivateBattleEncounterReader("relative.json", file, file));
        }
        finally { File.Delete(file); }
    }

    private static BattleEncounterDefinition Accepted(JsonNode placement, JsonNode scene, byte[] terrain) =>
        Assert.IsType<BattleEncounterReadAccepted>(Read(placement, scene, terrain)).Definition;
    private static BattleEncounterReadResult Read(JsonNode placement, JsonNode scene, byte[] terrain) =>
        PrivateBattleEncounterReader.Decode(Encoding.UTF8.GetBytes(placement.ToJsonString()), Encoding.UTF8.GetBytes(scene.ToJsonString()), terrain);

    private static (JsonNode Placement, JsonNode Scene, byte[] Terrain) Sample()
    {
        var terrain = Enumerable.Repeat((byte)255, 2304).ToArray();
        for (int y = 0; y < 11; y++) for (int x = 0; x < 12; x++) terrain[y * 48 + x] = 2;
        terrain[2 * 48 + 2] = 3;
        var placement = new JsonObject
        {
            ["schemaVersion"] = 1,
            ["provenance"] = new JsonObject { ["repository"] = "https://example.invalid/source", ["commit"] = new string('a', 40), ["sourcePath"] = "authored/placement.asm", ["sourceSha256"] = new string('b', 64) },
            ["battle"] = new JsonObject { ["id"] = 7, ["code"] = "AUTHORED_ENCOUNTER" },
            ["romRange"] = new JsonObject { ["start"] = 1000, ["endExclusive"] = 1056, ["lengthBytes"] = 56 },
            ["counts"] = new JsonObject { ["allies"] = 2, ["enemies"] = 1, ["aiRegions"] = 1, ["aiPoints"] = 2 },
            ["entities"] = new JsonArray(Entity(11, "ally", "24", 1, 1), Entity(7, "enemy", "UNRESOLVED_ENEMY + 1", 3, 2), Entity(4, "ally", "3", 4, 3)),
            ["aiRegions"] = new JsonArray(new JsonObject { ["id"] = 6, ["vertexCount"] = 4, ["unknown"] = 211,
                ["vertices"] = new JsonArray(Point(0, 0), Point(4, 0), Point(4, 4), Point(0, 4)), ["trailingBytes"] = new JsonArray(12, 255) }),
            ["aiPoints"] = new JsonArray(Point(1, 2), Point(8, 7)),
        };
        var scene = new JsonObject
        {
            ["schemaVersion"] = 1,
            ["provenance"] = new JsonObject { ["repository"] = "https://example.invalid/source", ["commit"] = new string('a', 40), ["terrainSourcePath"] = "authored/terrain.bin", ["terrainSourceSha256"] = new string('c', 64) },
            ["battle"] = placement["battle"]!.DeepClone(),
            ["map"] = new JsonObject { ["id"] = 22, ["x"] = 10, ["y"] = 12, ["width"] = 12, ["height"] = 11, ["triggerX"] = 255, ["triggerY"] = 0 },
            ["scene"] = new JsonObject { ["customBackgroundExpression"] = "UNRESOLVED_BACKGROUND", ["enemyLeaderPresent"] = true, ["halfExperience"] = false },
            ["terrain"] = new JsonObject { ["compressedRange"] = new JsonObject { ["start"] = 2000, ["endExclusive"] = 2010, ["lengthBytes"] = 10 },
                ["compressedSha256"] = new string('c', 64), ["decompressedWidth"] = 48, ["decompressedHeight"] = 48, ["decompressedLengthBytes"] = 2304,
                ["decompressedSha256"] = Convert.ToHexString(SHA256.HashData(terrain)), ["valueCounts"] = new JsonObject { ["2"] = 131, ["3"] = 1, ["255"] = 2172 } },
        };
        return (placement, scene, terrain);
        static JsonObject Point(int x, int y) => new() { ["x"] = x, ["y"] = y };
        static JsonObject Entity(int id, string kind, string identity, int x, int y) => new()
        {
            ["id"] = id, ["kind"] = kind, ["identityExpression"] = identity, ["x"] = x, ["y"] = y,
            ["aiCommandsetExpression"] = "UNKNOWN_AI", ["itemExpression"] = "WEAPON|EQUIPPED",
            ["behavior"] = new JsonObject { ["primaryOrderExpression"] = "MOVE_TO_POINT_2", ["primaryRegion"] = 6,
                ["secondaryOrderExpression"] = "NONE", ["secondaryRegion"] = 15, ["filler"] = 96, ["spawnExpression"] = "STARTING" },
        };
    }
}

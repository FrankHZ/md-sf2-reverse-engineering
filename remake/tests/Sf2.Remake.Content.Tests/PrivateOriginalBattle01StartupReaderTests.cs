using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Content.Tests;

public sealed class PrivateOriginalBattle01StartupReaderTests
{
    [Fact]
    public void AuthoredSemanticSamplePreservesTypedPlacementAndUnknownBytesWithoutClaimingCanonicalPixels()
    {
        var (placement, scene, terrain) = Sample();
        placement["entities"]![3]!["behavior"]!["filler"] = 96;
        placement["aiRegions"]![0]!["unknown"] = 9;
        placement["aiRegions"]![0]!["trailingBytes"]![1] = 7;
        var definition = Assert.IsType<OriginalBattle01StartupImported>(Admit(placement, scene, terrain)).Definition;
        Assert.Equal(9, definition.Entities.Count); Assert.Equal(3, definition.Regions.Count); Assert.Empty(definition.AiPoints);
        Assert.Equal(39, definition.Entities[3].Identity); Assert.Equal(128, definition.Entities[3].CombatantIndex);
        Assert.Equal(OriginalBattle01AiCommandSet.Attacker1, definition.Entities[3].AiCommandSet);
        Assert.Equal(255, definition.Entities[3].PrimaryOrder); Assert.Equal(127, definition.Entities[3].ItemWord);
        Assert.Equal(96, definition.Entities[3].Filler);
        Assert.Equal(9, definition.Regions[0].Unknown); Assert.Equal(7, definition.Regions[0].TrailingByte1);
        Assert.Equal(2, definition.TerrainAt(1, 1)); Assert.Equal(3, definition.TerrainAt(17, 0));
        Assert.Equal(new MapId("map57"), definition.Map); Assert.Equal((16, 20), (definition.AreaWidth, definition.AreaHeight));
        Assert.NotNull(definition.GetAdmissionDiagnostic()); // Authored payloads never pass canonical source-port admission.
    }

    [Theory]
    [InlineData("schema", "placement.schemaVersion")]
    [InlineData("numeric-type", "scene.schemaVersion")]
    [InlineData("extra-field", "placement")]
    [InlineData("repository", "provenance.repository")]
    [InlineData("commit", "provenance.commit")]
    [InlineData("source", "provenance.sourcePath")]
    [InlineData("source-digest", "provenance.sourceSha256")]
    [InlineData("terrain-source", "provenance.terrainSourcePath")]
    [InlineData("battle", "battle.id")]
    [InlineData("map", "map.id")]
    [InlineData("area", "map.width")]
    [InlineData("background", "scene.customBackgroundExpression")]
    [InlineData("leader", "scene.selection")]
    [InlineData("count", "counts.enemies")]
    [InlineData("duplicate-entity", "entities")]
    [InlineData("overlap", "entities")]
    [InlineData("position", "entities")]
    [InlineData("identity", "entities.identityExpression")]
    [InlineData("ai-command", "entities.aiCommandsetExpression")]
    [InlineData("primary-region", "entities")]
    [InlineData("item", "entities.itemExpression")]
    [InlineData("order", "entities.primaryOrder")]
    [InlineData("spawn", "entities.spawn")]
    [InlineData("byte-type", "filler")]
    [InlineData("region-count", "aiRegions")]
    [InlineData("region-id", "regions")]
    [InlineData("vertices", "vertices")]
    [InlineData("points", "aiPoints")]
    [InlineData("terrain-width", "terrain.decompressedWidth")]
    [InlineData("terrain-length", "terrain.decompressedLengthBytes")]
    [InlineData("terrain-counts", "terrain.valueCounts")]
    [InlineData("terrain-value", "terrain.decompressedSha256")]
    public void SemanticDriftReachesItsOwningValidationBoundary(string drift, string expectedField)
    {
        var (placement, scene, terrain) = Sample();
        var entity = placement["entities"]![3]!;
        switch (drift)
        {
            case "schema": placement["schemaVersion"] = 2; break;
            case "numeric-type": scene["schemaVersion"] = "1"; break;
            case "extra-field": placement["ignored"] = true; break;
            case "repository": placement["provenance"]!["repository"] = "https://example.invalid"; break;
            case "commit": placement["provenance"]!["commit"] = new string('0', 40); break;
            case "source": placement["provenance"]!["sourcePath"] = "spriteset02.asm"; break;
            case "source-digest": placement["provenance"]!["sourceSha256"] = new string('0', 64); break;
            case "terrain-source": scene["provenance"]!["terrainSourcePath"] = "terrain02.bin"; break;
            case "battle": placement["battle"]!["id"] = 2; break;
            case "map": scene["map"]!["id"] = 40; break;
            case "area": scene["map"]!["width"] = 48; break;
            case "background": scene["scene"]!["customBackgroundExpression"] = "DEFAULT"; break;
            case "leader": scene["scene"]!["enemyLeaderPresent"] = true; break;
            case "count": placement["counts"]!["enemies"] = 5; break;
            case "duplicate-entity": entity["id"] = 2; break;
            case "overlap": entity["x"] = 2; break;
            case "position": entity["x"] = 16; break;
            case "identity": entity["identityExpression"] = "OTHER"; break;
            case "ai-command": entity["aiCommandsetExpression"] = "OTHER"; break;
            case "primary-region": entity["behavior"]!["primaryRegion"] = 3; break;
            case "item": entity["itemExpression"] = "WOODEN_SWORD"; break;
            case "order": entity["behavior"]!["primaryOrderExpression"] = "MOVE"; break;
            case "spawn": entity["behavior"]!["spawnExpression"] = "HIDDEN"; break;
            case "byte-type": entity["behavior"]!["filler"] = "96"; break;
            case "region-count": placement["aiRegions"]!.AsArray().RemoveAt(2); break;
            case "region-id": placement["aiRegions"]![2]!["id"] = 1; break;
            case "vertices": placement["aiRegions"]![0]!["vertices"]!.AsArray().RemoveAt(3); break;
            case "points": placement["aiPoints"]!.AsArray().Add(new JsonObject { ["x"] = 1, ["y"] = 1 }); break;
            case "terrain-width": scene["terrain"]!["decompressedWidth"] = 16; break;
            case "terrain-length": scene["terrain"]!["decompressedLengthBytes"] = 320; break;
            case "terrain-counts": scene["terrain"]!["valueCounts"]!["0"] = 2304; break;
            case "terrain-value": terrain[49] = 1; break;
        }
        var rejected = Assert.IsType<OriginalBattle01StartupImportRejected>(Admit(placement, scene, terrain));
        Assert.Equal(expectedField, rejected.Diagnostic.Field);
    }

    [Fact]
    public void DuplicateJsonPropertiesCannotSilentlyReplaceSchemaValues()
    {
        var (placement, scene, terrain) = Sample();
        string duplicate = placement.ToJsonString().Replace("\"schemaVersion\":1", "\"schemaVersion\":1,\"schemaVersion\":1", StringComparison.Ordinal);
        var result = PrivateOriginalBattle01StartupReader.AdmitSemanticDocumentsForTests(
            System.Text.Encoding.UTF8.GetBytes(duplicate), JsonSerializer.SerializeToUtf8Bytes(scene), terrain);
        Assert.Equal("placement", Assert.IsType<OriginalBattle01StartupImportRejected>(result).Diagnostic.Field);
    }

    [Fact]
    public void FileAdmissionRejectsAbsentOrAuthoredInputsWithoutLeakingPaths()
    {
        string temporary = Path.GetTempFileName();
        try
        {
            var missingReader = new PrivateOriginalBattle01StartupReader(temporary + ".absent", temporary, temporary);
            var missing = Assert.IsType<OriginalBattle01StartupImportRejected>(missingReader.Admit());
            Assert.Equal("placement", missing.Diagnostic.Field);
            Assert.DoesNotContain(temporary, missing.Diagnostic.Message, StringComparison.Ordinal);
            File.WriteAllText(temporary, "{}");
            var reader = new PrivateOriginalBattle01StartupReader(temporary, temporary, temporary);
            var wrong = Assert.IsType<OriginalBattle01StartupImportRejected>(reader.Admit());
            Assert.Equal("placement", wrong.Diagnostic.Field);
            Assert.Contains("identity", wrong.Diagnostic.Message, StringComparison.Ordinal);
        }
        finally { File.Delete(temporary); }
    }

    [Sf2.Remake.TestSupport.PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN")]
    public void AcceptedSelectedBattle01InputsAreRequiredToExerciseTheRealReader()
    {
        string placement = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_DATA");
        string scene = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_SCENE");
        string terrain = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_TERRAIN");
        var definition = Assert.IsType<OriginalBattle01StartupImported>(new PrivateOriginalBattle01StartupReader(placement, scene, terrain).Admit()).Definition;
        Assert.Null(definition.GetAdmissionDiagnostic());
        Assert.Equal(OriginalBattle01StartupDefinition.AcceptedPlacementDigest, definition.PlacementDigest);
        Assert.Equal(OriginalBattle01StartupDefinition.AcceptedTerrainDigest, definition.TerrainDigest);
        Assert.Equal(2304, definition.Terrain.Count);
        Assert.Equal(new[] { (0, 102), (1, 108), (2, 11), (3, 5), (255, 2078) },
            definition.Terrain.GroupBy(value => value).OrderBy(group => group.Key).Select(group => ((int)group.Key, group.Count())));
        Assert.Equal(new[] { 0, 1, 2, 128, 129, 130, 131, 132, 133 }, definition.Entities.Select(entity => entity.CombatantIndex));
        Assert.All(definition.Entities, entity => Assert.Equal(OriginalBattle01Spawn.Starting, entity.Spawn));
        Assert.Equal(new[] { new MapPosition(8, 18), new(9, 18), new(7, 18) }, definition.Entities.Take(3).Select(entity => entity.Position));
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h3/map3-battle01-player-ready-v1.json"));
        using var fixture = JsonDocument.Parse(File.ReadAllText(path));
        var observed = fixture.RootElement.GetProperty("expectedObservation").GetProperty("records")[0]
            .GetProperty("scenario").GetProperty("combatants");
        foreach (var entity in definition.Entities)
        {
            var row = observed.EnumerateArray().Single(value => value.GetProperty("id").GetInt32() == entity.CombatantIndex);
            Assert.Equal(new MapPosition(row.GetProperty("x").GetInt32(), row.GetProperty("y").GetInt32()), entity.Position);
        }
        // Those positions corroborate placement only; activation bits/turn scores are deliberately not imported.
    }

    private static OriginalBattle01StartupImportResult Admit(JsonObject placement, JsonObject scene, byte[] terrain) =>
        PrivateOriginalBattle01StartupReader.AdmitSemanticDocumentsForTests(JsonSerializer.SerializeToUtf8Bytes(placement), JsonSerializer.SerializeToUtf8Bytes(scene), terrain);

    private static (JsonObject Placement, JsonObject Scene, byte[] Terrain) Sample()
    {
        // Fully authored placements/regions/terrain; only licensing-safe source identities are shared.
        byte[] terrain = new byte[2304]; terrain[49] = 2; terrain[17] = 3;
        var placement = JsonSerializer.SerializeToNode(new {
            schemaVersion = 1,
            provenance = new { repository = OriginalBattle01StartupDefinition.Repository, commit = OriginalBattle01StartupDefinition.UpstreamCommit,
                sourcePath = "data/battles/spritesets/spriteset01.asm", sourceSha256 = "DBE89C24807B4577F741FC0EB6EDAE3386F1C47F8712CA2586AE945EDFEDD94B" },
            battle = new { id = 1, code = "INSIDE_ANCIENT_TOWER" },
            romRange = new { start = 1782498, endExclusive = 1782646, lengthBytes = 148 },
            counts = new { allies = 3, enemies = 6, aiRegions = 3, aiPoints = 0 },
            entities = Enumerable.Range(0, 9).Select(index => new { id = index, kind = index < 3 ? "ally" : "enemy",
                identityExpression = index < 3 ? index.ToString(System.Globalization.CultureInfo.InvariantCulture) : "GIZMO", x = index, y = 1,
                aiCommandsetExpression = index < 3 ? "HEALER1" : "ATTACKER1", itemExpression = "NOTHING",
                behavior = new { primaryOrderExpression = "NONE", primaryRegion = 0, secondaryOrderExpression = "NONE", secondaryRegion = 0, filler = 0, spawnExpression = "STARTING" } }),
            aiRegions = Enumerable.Range(0, 3).Select(index => new { id = index, vertexCount = 4, unknown = 0,
                vertices = new[] { new { x = 0, y = 0 }, new { x = 1, y = 0 }, new { x = 1, y = 1 }, new { x = 0, y = 1 } }, trailingBytes = new[] { 0, 0 } }),
            aiPoints = System.Array.Empty<object>()
        })!.AsObject();
        var scene = JsonSerializer.SerializeToNode(new {
            schemaVersion = 1,
            provenance = new { repository = OriginalBattle01StartupDefinition.Repository, commit = OriginalBattle01StartupDefinition.UpstreamCommit,
                terrainSourcePath = "data/battles/entries/battle01/terrain.bin", terrainSourceSha256 = PrivateOriginalBattle01StartupReader.CompressedTerrainDigest },
            battle = new { id = 1, code = "INSIDE_ANCIENT_TOWER" },
            map = new { id = 57, x = 0, y = 0, width = 16, height = 20, triggerX = 255, triggerY = 255 },
            scene = new { customBackgroundExpression = "TOWER_INTERIOR", enemyLeaderPresent = false, halfExperience = true },
            terrain = new { compressedRange = new { start = 1758020, endExclusive = 1758304, lengthBytes = 284 },
                compressedSha256 = PrivateOriginalBattle01StartupReader.CompressedTerrainDigest,
                decompressedWidth = 48, decompressedHeight = 48, decompressedLengthBytes = 2304,
                decompressedSha256 = Convert.ToHexString(SHA256.HashData(terrain)),
                valueCounts = terrain.GroupBy(value => value).ToDictionary(group => group.Key.ToString(System.Globalization.CultureInfo.InvariantCulture), group => group.Count()) }
        })!.AsObject();
        return (placement, scene, terrain);
    }
}

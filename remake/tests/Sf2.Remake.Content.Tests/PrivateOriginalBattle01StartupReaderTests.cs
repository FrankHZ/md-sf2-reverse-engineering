using System.Security.Cryptography;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Battles;
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

    [Sf2.Remake.TestSupport.PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE",
        "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_CANONICAL_MAP_IMPORT")]
    public void AcceptedSelectedInputsInitializeRealNineUnitProjectionFromControlledPending()
    {
        string placement = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_DATA");
        string scene = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_SCENE");
        string terrain = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_TERRAIN");
        string canonical = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_CANONICAL_MAP_IMPORT");
        var session = SeedControlledPending(canonical);
        var prepared = Assert.IsType<PrivateOriginalBattle01StartupPrepared>(session.PreparePrivateOriginalBattle01Startup(
            session.PrivateOriginalBattle01Admission, new PrivateOriginalBattle01StartupReader(placement, scene, terrain),
            OriginalBattle01ControlledPartyPreset.PlayerAttackComparison));
        var initialized = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared)).Snapshot;
        var state = initialized.Battle;
        Assert.Equal(GameFlowStage.Battle, session.PrivateOriginalFlowStage);
        Assert.Equal(new MapId("map57"), session.PrivateOriginalCurrentMap);
        Assert.Equal(new MapId("map40"), initialized.SourceSnapshot.Map); Assert.Null(session.PrivateOriginalBattle01Admission);
        Assert.Equal(Battle01Phase.BeforeFirstRound, state.Phase); Assert.Equal(0x1234u, state.RandomSeedImage);
        Assert.Equal(9, state.Roster.Count); Assert.Equal(9, state.Occupancy.Count(index => index >= 0));
        Assert.Equal(prepared.Inputs.Terrain, state.Terrain); Assert.NotSame(prepared.Inputs.Terrain, state.Terrain);
        Assert.Equal(2304, state.Terrain.Count);
        Assert.Equal(0, state.ElapsedSeconds); Assert.True(state.IntroFlag451); Assert.True(state.UnlockFlag401);
        Assert.False(state.CompletedFlag501); Assert.False(state.SuspendedFlag88);
        Assert.All(state.RegionFlags90Through105, flag => Assert.False(flag));
        Assert.All(state.AiLastTargets, value => Assert.Equal(255, value)); Assert.All(state.AiMemory, value => Assert.Equal(0, value));
        string fixturePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h3/map3-battle01-player-ready-v1.json"));
        using var fixture = JsonDocument.Parse(File.ReadAllText(fixturePath));
        var observed = fixture.RootElement.GetProperty("expectedObservation").GetProperty("records")[0]
            .GetProperty("scenario").GetProperty("combatants");
        for (int index = 0; index < 9; index++)
        {
            var actual = state.Roster[index]; var source = prepared.Inputs.Entities[index];
            var comparison = observed.EnumerateArray().Single(row => row.GetProperty("id").GetInt32() == actual.Index);
            Assert.Equal(source.Position, actual.Position); Assert.Equal(actual.Index, state.OccupantAt(actual.Position));
            Assert.Equal(source.Ordinal, actual.Deployment.Ordinal);
            Assert.Equal((byte)source.AiCommandSet, actual.Deployment.AiCommandSet);
            Assert.Equal(source.ItemWord, actual.Deployment.ItemWord);
            Assert.Equal(source.PrimaryOrder, actual.Deployment.PrimaryOrder);
            Assert.Equal(source.PrimaryRegion, actual.Deployment.PrimaryRegion);
            Assert.Equal(source.SecondaryOrder, actual.Deployment.SecondaryOrder);
            Assert.Equal(source.SecondaryRegion, actual.Deployment.SecondaryRegion);
            Assert.Equal(source.Filler, actual.Deployment.SourceFiller); Assert.Equal((byte)source.Spawn, actual.Deployment.Spawn);
            Assert.Equal(comparison.GetProperty("hpMax").GetUInt16(), actual.Stats.HpMax);
            Assert.Equal(comparison.GetProperty("mpMax").GetByte(), actual.Stats.MpMax);
            Assert.Equal(actual.Stats.HpMax, actual.Stats.HpCurrent); Assert.Equal(actual.Stats.MpMax, actual.Stats.MpCurrent);
            Assert.Equal(comparison.GetProperty("attack").GetByte(), actual.Stats.Attack);
            Assert.Equal(comparison.GetProperty("defense").GetByte(), actual.Stats.Defense);
            Assert.Equal(comparison.GetProperty("agility").GetByte(), actual.Stats.Agility);
            Assert.Equal(comparison.GetProperty("move").GetByte(), actual.Stats.Move);
            Assert.Equal(0, actual.Stats.Status);
            if (index < 3)
            {
                Assert.Equal(prepared.Party.Allies[index].Items, actual.Stats.Items);
                Assert.Equal(prepared.Party.Allies[index].Spells, actual.Stats.Spells);
            }
            else
            {
                Assert.Equal(7, actual.EnemySource!.SourceStats.Attack); Assert.Equal(8, actual.Stats.Attack);
                // STARTING keeps the source initialization byte; it is not necessarily zero.
                Assert.Equal((ushort?)(0x2000 | source.Filler), actual.InitializationAiBitfield);
                Assert.Equal((byte?)((6 << 4) | (byte)source.AiCommandSet), actual.MovementTypeAndAiCommandSet);
                Assert.Equal((ushort?)0x40E3, actual.Resistance);
            }
        }
        for (int index = 0; index < 3; index++)
        {
            Assert.Equal(prepared.Inputs.Regions[index].Vertices, state.Regions[index].Vertices);
            Assert.Equal(prepared.Inputs.Regions[index].Unknown, state.Regions[index].SourceUnknown);
            Assert.Equal(prepared.Inputs.Regions[index].TrailingByte0, state.Regions[index].TrailingByte0);
            Assert.Equal(prepared.Inputs.Regions[index].TrailingByte1, state.Regions[index].TrailingByte1);
        }
        // H3 stats corroborate this bounded result; observed activation/order/consumed RNG are never imported.
        Assert.Equal("battle", Assert.IsType<PrivateOriginalBattle01InitializationRejected>(
            session.InitializePrivateOriginalBattle01(prepared)).Diagnostic.Field);
        Assert.Same(initialized, session.PrivateOriginalBattle01); Assert.Equal(7, prepared.Inputs.EnemyBaseline.BaseAttack);
        Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMap(new(ExplorationDirection.North)));

        var firstRound = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(
            session.EnterPrivateOriginalBattle01FirstRound(initialized)).Snapshot;
        var roundState = firstRound.Battle; var order = roundState.FirstRound!;
        var record = fixture.RootElement.GetProperty("expectedObservation").GetProperty("records")[0];
        var expectedTurns = record.GetProperty("turnState");
        Assert.Same(firstRound, session.PrivateOriginalBattle01); Assert.NotSame(initialized, firstRound);
        Assert.Same(initialized.Preparation, firstRound.Preparation);
        Assert.Same(initialized.SourceSnapshot, firstRound.SourceSnapshot);
        Assert.Same(initialized.SourceLocomotion, firstRound.SourceLocomotion);
        Assert.Same(initialized.SourceBridge, firstRound.SourceBridge);
        Assert.Same(state.Terrain, roundState.Terrain); Assert.Same(state.Occupancy, roundState.Occupancy);
        Assert.Equal(Battle01Phase.FirstRoundGenerated, roundState.Phase);
        Assert.Equal(Battle01Phase.BeforeFirstRound, state.Phase); Assert.Null(state.FirstRound);
        Assert.Equal(expectedTurns.GetProperty("entries").EnumerateArray().Select(row =>
            new Battle01TurnEntry(row.GetProperty("actor").GetByte(), row.GetProperty("score").GetByte())), order.Slots.Take(9));
        Assert.Equal(64, order.Slots.Count);
        Assert.All(order.Slots.Skip(9), slot => Assert.Equal(new Battle01TurnEntry(255, 255), slot));
        Assert.Equal(expectedTurns.GetProperty("currentTurnOffset").GetByte(), order.CurrentTurnOffset);
        Assert.Equal(expectedTurns.GetProperty("entries")[0].GetProperty("actor").GetByte(), order.FirstCandidate!.Value.CombatantIndex);
        Assert.Empty(order.RegionCutsceneRows); Assert.Empty(order.SpawnedCombatants);
        Assert.Equal(record.GetProperty("deterministicState").GetProperty("seeded").GetProperty("randomSeed").GetUInt32(), state.RandomSeedImage);
        Assert.Equal(record.GetProperty("deterministicState").GetProperty("ready").GetProperty("randomSeed").GetUInt32(), roundState.RandomSeedImage);
        Assert.Equal(0, state.GeneratorWord); Assert.Equal(0xA499, roundState.GeneratorWord);
        Assert.Equal(0x1234u, roundState.RandomSeedImage & 0xFFFFu);
        Assert.Equal(record.GetProperty("admission").GetProperty("regionFlags90Through105").EnumerateArray().Select(flag => flag.GetBoolean()),
            roundState.RegionFlags90Through105);
        Assert.Equal(7, roundState.NewlyTestedRegionMask); Assert.All(roundState.RegionFlags90Through105, flag => Assert.False(flag));
        Assert.Equal(9, roundState.Roster.Count);
        for (int index = 0; index < 9; index++)
        {
            var unit = roundState.Roster[index]; Assert.Same(state.Roster[index].Stats, unit.Stats);
            Assert.Same(state.Roster[index].Deployment, unit.Deployment);
            Assert.Equal(unit.Index, roundState.OccupantAt(unit.Position));
            if (unit.Index >= 128)
                Assert.Equal((ushort?)observed.EnumerateArray().Single(row => row.GetProperty("id").GetInt32() == unit.Index)
                    .GetProperty("activationBitfield").GetUInt16(), unit.AiBitfield);
        }
        // Compare only the shared state seam. This API never executes H3's actor-control entry or timing.
        Assert.Equal(0, roundState.ElapsedSeconds); Assert.Equal(GameFlowStage.Battle, session.PrivateOriginalFlowStage);
        Assert.Equal(new MapId("map57"), session.PrivateOriginalCurrentMap); Assert.Null(session.PrivateOriginalBattle01Admission);
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(
            session.EnterPrivateOriginalBattle01FirstRound(firstRound)).Diagnostic.Field);
        Assert.Same(firstRound, session.PrivateOriginalBattle01);

        int actorIndex = order.FirstCandidate!.Value.CombatantIndex;
        var actorRow = observed.EnumerateArray().Single(row => row.GetProperty("id").GetInt32() == actorIndex);
        var controlled = Assert.IsType<PrivateOriginalBattle01FirstControlEntered>(
            session.EnterPrivateOriginalBattle01FirstControl(firstRound, actorIndex)).Snapshot;
        var actor = controlled.Battle.Roster.Single(unit => unit.Index == actorIndex);
        var control = controlled.Battle.FirstControl!; var origin = actor.Position;
        Assert.Equal(Battle01Phase.PlayerMovementSelection, controlled.Battle.Phase);
        Assert.Equal((byte?)actorRow.GetProperty("class").GetByte(), actor.ClassId);
        Assert.Equal(actorRow.GetProperty("statusEffects").GetUInt16(), actor.Stats.Status);
        Assert.Equal(actor.Stats.Move * 2, control.Movement.Range.Budget); Assert.Equal(10, control.Movement.Range.Budget);
        Assert.Equal(12, control.Movement.Range.Profile.MovementType); Assert.Equal(4, control.Movement.Range.Profile.ClassId);
        // This zero is an explicit control-entry comparison policy, not inferred natural initialization.
        Assert.True(control.CandidateWordSupplied);
        Assert.Equal((ushort?)actorRow.GetProperty("activationBitfield").GetUInt16(), actor.AiBitfield);
        Assert.Null(firstRound.Battle.Roster.Single(unit => unit.Index == actorIndex).AiBitfield);
        Assert.All(controlled.Battle.Roster.Where(unit => unit.Index < 128 && unit.Index != actorIndex), unit => Assert.Null(unit.AiBitfield));
        Assert.False(control.Preset.AllyAutoBattle); Assert.False(control.Preset.OpponentControl);
        var destination = new MapPosition(origin.X, origin.Y - 1);
        Assert.Equal(2, control.Movement.Range.Grid.CostAt(destination));
        Assert.Equal("destination", Assert.IsType<PrivateOriginalBattle01PlayerMovementRejected>(
            session.SelectPrivateOriginalBattle01PlayerDestination(controlled, actorIndex, new(origin.X, 19))).Diagnostic.Field);
        Assert.Same(controlled, session.PrivateOriginalBattle01);
        var selected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(controlled, actorIndex, destination)).Snapshot;
        Assert.Equal(origin, selected.Battle.Roster.Single(unit => unit.Index == actorIndex).Position);
        Assert.Equal(new byte[] { 1, 255 }, selected.Battle.FirstControl!.Movement.Preview.Directions);
        Assert.Equal(new byte[] { 3, 255 }, selected.Battle.FirstControl.Movement.Preview.ReturnDirections);
        Assert.Equal(2, selected.Battle.FirstControl.Movement.Preview.Cost);
        var moved = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(selected, actorIndex)).Snapshot;
        Assert.Equal(Battle01Phase.PlayerActionChoice, moved.Battle.Phase);
        Assert.Equal(destination, moved.Battle.Roster.Single(unit => unit.Index == actorIndex).Position);
        Assert.Equal(origin, moved.Battle.Roster.Single(unit => unit.Index == actorIndex).Deployment.Position);
        Assert.Equal(-1, moved.Battle.OccupantAt(origin)); Assert.Equal(actorIndex, moved.Battle.OccupantAt(destination));
        var cancelled = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.CancelPrivateOriginalBattle01PlayerMovement(moved, actorIndex)).Snapshot;
        Assert.Same(cancelled, session.PrivateOriginalBattle01); Assert.Equal(origin, cancelled.Battle.FirstControl!.Movement.Cursor);
        Assert.Equal(origin, cancelled.Battle.Roster.Single(unit => unit.Index == actorIndex).Position);
        Assert.Equal(controlled.Battle.Occupancy, cancelled.Battle.Occupancy); Assert.Same(order, cancelled.Battle.FirstRound);
        Assert.Equal(roundState.RandomSeedImage, cancelled.Battle.RandomSeedImage); Assert.Equal(0, cancelled.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Same(prepared, cancelled.Preparation); Assert.Same(firstRound.SourceBridge, cancelled.SourceBridge);
        Assert.Same(actor.Stats, cancelled.Battle.Roster.Single(unit => unit.Index == actorIndex).Stats);
        Assert.Same(roundState.Terrain, cancelled.Battle.Terrain); Assert.Same(roundState.RegionFlags90Through105, cancelled.Battle.RegionFlags90Through105);
        Assert.Equal("cancel", Assert.IsType<PrivateOriginalBattle01PlayerMovementRejected>(
            session.CancelPrivateOriginalBattle01PlayerMovement(cancelled, actorIndex)).Diagnostic.Field);
        Assert.Same(cancelled, session.PrivateOriginalBattle01);
        var staySelected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(cancelled, actorIndex, destination)).Snapshot;
        var stayChoice = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(staySelected, actorIndex)).Snapshot;
        var completed = Assert.IsType<PrivateOriginalBattle01StayCommitted>(
            session.CommitPrivateOriginalBattle01Stay(stayChoice, actorIndex)).Snapshot;
        Assert.Same(completed, session.PrivateOriginalBattle01); Assert.Equal(1, actorIndex);
        Assert.Equal(Battle01Phase.PlayerTurnCompleted, completed.Battle.Phase); Assert.Null(completed.Battle.FirstControl);
        Assert.Equal(1, completed.Battle.TurnCompletion!.CompletedActorIndex);
        Assert.Equal(2, completed.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Equal((byte)2, completed.Battle.FirstRound.CurrentCandidate!.Value.CombatantIndex);
        Assert.Equal(order.Slots[1], completed.Battle.FirstRound.CurrentCandidate);
        Assert.Same(order.Slots, completed.Battle.FirstRound.Slots); Assert.Equal(64, completed.Battle.FirstRound.Slots.Count);
        Assert.Equal(0xA4991234u, completed.Battle.RandomSeedImage); Assert.Equal(9, completed.Battle.Roster.Count);
        Assert.Same(stayChoice.Battle.Roster, completed.Battle.Roster); Assert.Same(stayChoice.Battle.Occupancy, completed.Battle.Occupancy);
        Assert.Equal(destination, completed.Battle.Roster.Single(unit => unit.Index == actorIndex).Position);
        Assert.Equal(-1, completed.Battle.OccupantAt(origin)); Assert.Equal(actorIndex, completed.Battle.OccupantAt(destination));
        Assert.Equal(new Battle01FactionCounts(3, 6), completed.Battle.TurnCompletion.BeforeAfterTurn);
        Assert.Equal(completed.Battle.TurnCompletion.BeforeAfterTurn, completed.Battle.TurnCompletion.AfterAfterTurn);
        Assert.Same(prepared, completed.Preparation); Assert.Same(firstRound.SourceSnapshot, completed.SourceSnapshot);
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01PlayerMovementRejected>(
            session.CancelPrivateOriginalBattle01PlayerMovement(completed, actorIndex)).Diagnostic.Field);
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01TurnCompletionRejected>(
            session.CommitPrivateOriginalBattle01Stay(completed, actorIndex)).Diagnostic.Field);
        Assert.Same(completed, session.PrivateOriginalBattle01);
        var next = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(completed,
                completed.Battle.FirstRound.CurrentCandidate.Value.CombatantIndex)).Snapshot;
        Assert.Equal(2, next.Battle.FirstControl!.ActorIndex); Assert.True(next.Battle.FirstControl.CandidateWordSupplied);
        Assert.Same(Battle01MovementProfile.Centaur, next.Battle.FirstControl.Movement.Range.Profile);
        Assert.Equal(14, next.Battle.FirstControl.Movement.Range.Budget);
        Assert.Equal(new MapPosition(7, 18), next.Battle.FirstControl.Movement.Range.Origin);
        Assert.Same(completed.Battle.Occupancy, next.Battle.FirstControl.Movement.Range.OriginOccupancy);
        Assert.Null(completed.Battle.Roster[2].AiBitfield); Assert.Null(next.Battle.Roster[0].AiBitfield);
        Assert.Equal((ushort?)0, next.Battle.Roster[2].AiBitfield);
        Assert.Same(completed.Battle.TurnCompletion, next.Battle.TurnCompletion);
        var secondSelected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(next, 2, new(7, 17))).Snapshot;
        var secondMoved = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(secondSelected, 2)).Snapshot;
        var secondCancelled = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.CancelPrivateOriginalBattle01PlayerMovement(secondMoved, 2)).Snapshot;
        Assert.Equal(new MapPosition(7, 18), secondCancelled.Battle.Roster[2].Position);
        Assert.Equal(destination, secondCancelled.Battle.Roster[1].Position);
        Assert.Equal(completed.Battle.Occupancy, secondCancelled.Battle.Occupancy);
        secondSelected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(secondCancelled, 2, new(7, 17))).Snapshot;
        secondMoved = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(secondSelected, 2)).Snapshot;
        var secondCompleted = Assert.IsType<PrivateOriginalBattle01StayCommitted>(
            session.CommitPrivateOriginalBattle01Stay(secondMoved, 2)).Snapshot;
        Assert.Equal(2, secondCompleted.Battle.TurnCompletion!.CompletedActorIndex);
        Assert.Same(completed.Battle.TurnCompletion, secondCompleted.Battle.TurnCompletion.Previous);
        Assert.Equal(new Battle01FactionCounts(3, 6), secondCompleted.Battle.TurnCompletion.BeforeAfterTurn);
        Assert.Equal(secondCompleted.Battle.TurnCompletion.BeforeAfterTurn, secondCompleted.Battle.TurnCompletion.AfterAfterTurn);
        Assert.Equal(4, secondCompleted.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Equal(order.Slots[2], secondCompleted.Battle.FirstRound.CurrentCandidate);
        Assert.Equal((byte)128, secondCompleted.Battle.FirstRound.CurrentCandidate!.Value.CombatantIndex);
        var stopped = Assert.IsType<PrivateOriginalBattle01NextPlayerControlUnavailable>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(secondCompleted, 128));
        Assert.Equal(Battle01FirstControlAvailability.OpponentAi, stopped.Decision.Availability);
        Assert.Equal(128, stopped.Decision.ActorIndex); Assert.Same(secondCompleted, session.PrivateOriginalBattle01);
        Assert.Same(order.Slots, secondCompleted.Battle.FirstRound.Slots);
        Assert.Equal(0xA4991234u, secondCompleted.Battle.RandomSeedImage);
        Assert.Equal(9, secondCompleted.Battle.Roster.Count); Assert.Same(prepared, secondCompleted.Preparation);
        Assert.Equal(destination, secondCompleted.Battle.Roster[1].Position);
        Assert.Equal(new MapPosition(7, 17), secondCompleted.Battle.Roster[2].Position);
        Assert.Equal((ushort?)0x1234, initialized.Battle.RandomSeedCopy);
        Assert.Equal(initialized.Battle.RandomSeedCopy, secondCompleted.Battle.RandomSeedCopy);
        var enemyCompleted = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(
            session.CompletePrivateOriginalBattle01EnemyStandby(secondCompleted, 128)).Snapshot;
        var enemyState = enemyCompleted.Battle; var decision = enemyState.TurnCompletion!.EnemyStandby!;
        Assert.Same(enemyCompleted, session.PrivateOriginalBattle01); Assert.Same(prepared, enemyCompleted.Preparation);
        Assert.Same(secondCompleted.Battle.TurnCompletion, enemyState.TurnCompletion.Previous);
        Assert.Equal(Battle01Phase.EnemyTurnCompleted, enemyState.Phase); Assert.Null(enemyState.FirstControl);
        Assert.Equal(6, enemyState.FirstRound!.CurrentTurnOffset); Assert.Equal(order.Slots[3], enemyState.FirstRound.CurrentCandidate);
        Assert.Equal((byte)131, enemyState.FirstRound.CurrentCandidate!.Value.CombatantIndex); Assert.Same(order.Slots, enemyState.FirstRound.Slots);
        Assert.Equal(new MapPosition(6, 3), enemyState.Roster[3].Position); Assert.Equal(new MapPosition(7, 3), enemyState.Roster[3].Deployment.Position);
        Assert.Equal(-1, enemyState.OccupantAt(new(7, 3))); Assert.Equal(128, enemyState.OccupantAt(new(6, 3)));
        Assert.Equal(new byte[] { 2, 255 }, decision.MoveString); Assert.Equal(new byte[] { 8, 2, 1 }, decision.Rolls.Select(roll => roll.Range));
        Assert.Equal(new byte[] { 7, 0, 0 }, decision.Rolls.Select(roll => roll.Result));
        Assert.Equal(new[] { 61, 85, 1 }, decision.Rolls.Select(roll => roll.GeneratorSteps));
        Assert.Equal(new int?[] { 2, 2, null, 2 }, decision.Candidates.Select(candidate => candidate.GridCost));
        Assert.Equal(new int?[] { null, null, null, 131 }, decision.Candidates.Select(candidate => candidate.Occupant));
        Assert.Equal((ushort?)0x3934, enemyState.RandomSeedCopy); Assert.Equal(0xA4991234u, enemyState.RandomSeedImage);
        Assert.Equal(0x14, enemyState.AiMemory[0]); Assert.All(enemyState.AiMemory.Skip(1), value => Assert.Equal(0, value));
        Assert.Equal(0, enemyState.NewlyTestedRegionMask); Assert.All(enemyState.RegionFlags90Through105, flag => Assert.False(flag));
        Assert.Same(secondCompleted.Battle.AiLastTargets, enemyState.AiLastTargets); Assert.Null(enemyState.Roster[0].AiBitfield);
        for (int i = 0; i < 9; i++)
        {
            Assert.Same(secondCompleted.Battle.Roster[i].Stats, enemyState.Roster[i].Stats);
            Assert.Same(secondCompleted.Battle.Roster[i].Deployment, enemyState.Roster[i].Deployment);
            Assert.Equal(secondCompleted.Battle.Roster[i].AiBitfield, enemyState.Roster[i].AiBitfield);
            if (i != 3) Assert.Same(secondCompleted.Battle.Roster[i], enemyState.Roster[i]);
        }
        var current = enemyCompleted;
        int[] remaining = [131,133,129,130,132]; MapPosition[] destinations = [new(8,4),new(6,6),new(10,4),new(6,5),new(9,6)];
        int[][] generatorSteps = [[56,199,57],[114,85,57],[114,85,1],[56,199,57],[114,85,57]];
        byte[] finalBounds = [3,3,1,2,3]; int remainingSteps = 0;
        for (int i = 0; i < remaining.Length; i++)
        {
            var beforeEnemy = current;
            Assert.Equal(remaining[i], current.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex);
            current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(
                session.CompletePrivateOriginalBattle01EnemyStandby(current, remaining[i])).Snapshot;
            var result = current.Battle.TurnCompletion!.EnemyStandby!;
            Assert.Same(beforeEnemy.Battle.TurnCompletion, current.Battle.TurnCompletion.Previous);
            Assert.Equal(destinations[i], result.Destination); Assert.Equal(new byte[] { i == 2 ? (byte)0 : (byte)3, 255 }, result.MoveString);
            Assert.Equal(new byte[] { 8, 2, finalBounds[i] }, result.Rolls.Select(roll => roll.Range));
            Assert.Equal(generatorSteps[i], result.Rolls.Select(roll => roll.GeneratorSteps)); remainingSteps += result.Rolls.Sum(roll => roll.GeneratorSteps);
            Assert.Equal(beforeEnemy.Battle.RandomSeedCopy, (ushort?)result.SeedCopyBefore); Assert.Null(current.Battle.Roster[0].AiBitfield);
        }
        Assert.Equal(1336, remainingSteps); Assert.Equal(16, current.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Equal((ushort?)0x0134, current.Battle.RandomSeedCopy);
        Assert.Equal(new byte[] { 0x14,0x34,0x24,0x24,0x24,0x24 }, current.Battle.AiMemory.Take(6));
        var bowie = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current, 0)).Snapshot;
        Assert.Equal(0, bowie.Battle.FirstControl!.ActorIndex); Assert.Same(Battle01MovementProfile.Regular, bowie.Battle.FirstControl.Movement.Range.Profile);
        Assert.Equal(12, bowie.Battle.FirstControl.Movement.Range.Budget); Assert.Equal((ushort?)0, bowie.Battle.Roster[0].AiBitfield);
        var bowieSelected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(bowie, 0, new(8,17))).Snapshot;
        var bowieMoved = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(bowieSelected, 0)).Snapshot;
        var bowieCancelled = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.CancelPrivateOriginalBattle01PlayerMovement(bowieMoved, 0)).Snapshot;
        Assert.Equal(new MapPosition(8,18), bowieCancelled.Battle.Roster[0].Position); Assert.Equal(current.Battle.Occupancy, bowieCancelled.Battle.Occupancy);
        bowieSelected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(bowieCancelled, 0, new(8,17))).Snapshot;
        bowieMoved = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(bowieSelected, 0)).Snapshot;
        var exhausted = Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(bowieMoved, 0)).Snapshot;
        Assert.Equal(18, exhausted.Battle.FirstRound!.CurrentTurnOffset); Assert.Null(exhausted.Battle.FirstRound.CurrentCandidate);
        Assert.Same(order.Slots, exhausted.Battle.FirstRound.Slots); Assert.Equal(0xA4991234u, exhausted.Battle.RandomSeedImage);
        Assert.Equal((ushort?)0x0134, exhausted.Battle.RandomSeedCopy); Assert.Same(current.Battle.AiMemory, exhausted.Battle.AiMemory);
        Assert.Same(current.Battle.AiLastTargets, exhausted.Battle.AiLastTargets); Assert.Same(current.Battle.RegionFlags90Through105, exhausted.Battle.RegionFlags90Through105);
        Assert.Equal(new MapPosition(8,17), exhausted.Battle.Roster[0].Position);
        Assert.All(Enumerable.Range(0, 9), i => Assert.Same(state.Roster[i].Stats, exhausted.Battle.Roster[i].Stats));
        var completedActors = new List<int>(); for (var receipt = exhausted.Battle.TurnCompletion; receipt is not null; receipt = receipt.Previous) completedActors.Add(receipt.CompletedActorIndex);
        Assert.Equal(new[] { 0,132,130,129,133,131,128,2,1 }, completedActors);
        Assert.Equal(Battle01FirstControlAvailability.Sentinel, Assert.IsType<PrivateOriginalBattle01NextPlayerControlUnavailable>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(exhausted, 255)).Decision.Availability);
        Assert.Same(exhausted, session.PrivateOriginalBattle01);

        var secondRound = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(
            session.EnterPrivateOriginalBattle01NextRound(exhausted)).Snapshot;
        Assert.Equal(Battle01Phase.RoundGenerated,secondRound.Battle.Phase);
        Assert.Equal(2,secondRound.Battle.FirstRound!.RoundNumber);
        Assert.Equal(0xAA861234u,secondRound.Battle.RandomSeedImage);
        Assert.Equal(new byte[] {2,128,132,131,133,0,1,129,130},secondRound.Battle.FirstRound.Slots.Take(9).Select(slot=>slot.CombatantIndex));
        Assert.Equal(new byte[] {7,6,6,5,5,4,4,4,4},secondRound.Battle.FirstRound.Slots.Take(9).Select(slot=>slot.AlteredAgility));
        Assert.Equal(64,secondRound.Battle.FirstRound.Slots.Count);
        Assert.All(secondRound.Battle.FirstRound.Slots.Skip(9),slot=>Assert.Equal(new Battle01TurnEntry(255,255),slot));
        Assert.Same(exhausted.Battle.TurnCompletion,secondRound.Battle.TurnCompletion);
        Assert.Equal(7,secondRound.Battle.NewlyTestedRegionMask);
        var secondDecisions = new List<Battle01EnemyStandbyDecision>();
        for (int slot=0;slot<9;slot++)
        {
            var prior = session.PrivateOriginalBattle01!; int actual = prior.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
            if (actual>=128)
            {
                current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(
                    session.CompletePrivateOriginalBattle01EnemyStandby(prior,actual)).Snapshot;
                secondDecisions.Add(current.Battle.TurnCompletion!.EnemyStandby!);
            }
            else
            {
                var player = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
                    session.EnterPrivateOriginalBattle01NextPlayerControl(prior,actual)).Snapshot;
                if (actual==0)
                {
                    player = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
                        session.SelectPrivateOriginalBattle01PlayerDestination(player,0,new(11,15))).Snapshot;
                    Assert.Equal(new MapPosition(8,17),player.Battle.FirstControl!.Movement.Range.Origin);
                    Assert.Equal(10,player.Battle.FirstControl.Movement.GridCost);
                }
                // Bowie chooses the reachable region; other actual players confirm origin, then separately STAY.
                var originConfirmed = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
                    session.ConfirmPrivateOriginalBattle01PlayerMovement(player,actual)).Snapshot;
                current = Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(originConfirmed,actual)).Snapshot;
            }
            Assert.Equal(2,current.Battle.TurnCompletion!.RoundNumber);
            Assert.Equal(actual,current.Battle.TurnCompletion.CompletedActorIndex);
            Assert.Same(prior.Battle.TurnCompletion,current.Battle.TurnCompletion.Previous);
            Assert.Same(prepared,current.Preparation); Assert.Same(exhausted.SourceSnapshot,current.SourceSnapshot);
            Assert.All(Enumerable.Range(0,9),i=> {
                Assert.Same(state.Roster[i].Stats,current.Battle.Roster[i].Stats);
                Assert.Same(state.Roster[i].Deployment,current.Battle.Roster[i].Deployment);
            });
        }
        Assert.Equal(967,secondDecisions.Sum(item=>item.Rolls.Sum(roll=>roll.GeneratorSteps)));
        Assert.Equal(11,secondDecisions.Sum(item=>item.Rolls.Count));
        Assert.Equal(new[] {128,132,131,133,129,130},secondDecisions.Select(item=>item.ActorIndex));
        Assert.Equal(new MapPosition[] {new(7,2),new(10,5),new(8,4),new(6,4),new(9,3),new(6,3)},secondDecisions.Select(item=>item.Destination));
        Assert.Equal(new byte[] {4,4,4,0x24,0x34,4},current.Battle.AiMemory.Take(6));
        Assert.Equal((ushort?)0x0034,current.Battle.RandomSeedCopy); Assert.Equal(0,current.Battle.NewlyTestedRegionMask);
        Assert.All(current.Battle.AiMemory.Skip(6),value=>Assert.Equal(0,value));
        Assert.All(current.Battle.AiLastTargets,value=>Assert.Equal(255,value));
        Assert.All(current.Battle.RegionFlags90Through105,Assert.False);
        var third = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot;
        Assert.Equal(3,third.Battle.FirstRound!.RoundNumber); Assert.Equal(0x9BD71234u,third.Battle.RandomSeedImage);
        Assert.Equal(new byte[] {2,129,130,131,128,133,0,1,132},third.Battle.FirstRound.Slots.Take(9).Select(slot=>slot.CombatantIndex));
        Assert.Equal(new byte[] {8,6,6,6,5,5,4,4,4},third.Battle.FirstRound.Slots.Take(9).Select(slot=>slot.AlteredAgility));
        Assert.Equal(64,third.Battle.FirstRound.Slots.Count);
        Assert.All(third.Battle.FirstRound.Slots.Skip(9),slot=>Assert.Equal(new Battle01TurnEntry(255,255),slot));
        Assert.Same(current.Battle.TurnCompletion,third.Battle.TurnCompletion);
        var thirdReady = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(third,2)).Snapshot;
        Assert.Equal(new MapPosition(7,17),thirdReady.Battle.FirstControl!.Movement.Range.Origin);
        Assert.Equal(0,thirdReady.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Same(current.Battle.AiMemory,thirdReady.Battle.AiMemory);
        var history = new List<Battle01TurnCompletionReceipt>();
        for (var receipt=thirdReady.Battle.TurnCompletion;receipt is not null;receipt=receipt.Previous) history.Add(receipt);
        Assert.Equal(18,history.Count); Assert.All(history.Take(9),item=>Assert.Equal(2,item.RoundNumber));
        Assert.All(history.Skip(9),item=>Assert.Equal(1,item.RoundNumber));
        Assert.Equal(new[] {false,true,false},third.Battle.RegionFlags90Through105.Take(3));
        Assert.Equal(new ushort?[] {0x2060,0x2060,0x2060,0x2061,0x2071,0x2070},third.Battle.Roster.Skip(3).Select(unit=>unit.AiBitfield));

        // Continue this same real-reader/session chain; later players explicitly choose origin/STAY.
        foreach (var expected in new[] {
            (Number:3,Main:0x9BD71234u,Copy:(ushort)0x0234,Steps:958,Memory:new byte[] {0x14,0x24,0x14,0x24,0x34,0x34}),
            (Number:4,Main:0x51DC1234u,Copy:(ushort)0x0234,Steps:512,Memory:new byte[] {4,4,0x14,0x24,0x34,0x24}),
            (Number:5,Main:0xDE251234u,Copy:(ushort)0x5634,Steps:380,Memory:new byte[] {0x14,0x14,0x24,0x24,0x34,0x24}) })
        {
            var generation=session.PrivateOriginalBattle01!; int thinkingSteps=0;
            for(int slot=0;slot<9;slot++)
            {
                var prior=session.PrivateOriginalBattle01!; current=CompleteCurrentTurn(); var receipt=current.Battle.TurnCompletion!;
                Assert.Same(prior.Battle.TurnCompletion,receipt.Previous); Assert.Equal(expected.Number,receipt.RoundNumber);
                Assert.Equal(generation.Battle.FirstRound!.Slots[slot].CombatantIndex,receipt.CompletedActorIndex);
                Assert.Same(prepared,current.Preparation); Assert.Same(exhausted.SourceSnapshot,current.SourceSnapshot);
                Assert.Equal(expected.Main,current.Battle.RandomSeedImage); Assert.Same(prior.Battle.AiLastTargets,current.Battle.AiLastTargets);
                Assert.All(Enumerable.Range(0,9),i=> { Assert.Same(state.Roster[i].Stats,current.Battle.Roster[i].Stats); Assert.Same(state.Roster[i].Deployment,current.Battle.Roster[i].Deployment); });
                if(receipt.EnemyStandby is { } standby) thinkingSteps+=standby.Rolls.Sum(roll=>roll.GeneratorSteps);
                if(receipt.EnemyPursuit is { } pursuit)
                {
                    Assert.Null(receipt.EnemyStandby); Assert.Equal(prior.Battle.RandomSeedCopy,current.Battle.RandomSeedCopy);
                    Assert.Equal(prior.Battle.AiMemory,current.Battle.AiMemory); Assert.Equal(0,pursuit.TargetIndex);
                    Assert.Equal((byte)3,pursuit.Action); Assert.Equal(0,current.Battle.NewlyTestedRegionMask);
                    if(expected.Number==3 && pursuit.ActorIndex==131)
                    {
                        Assert.Equal(new MapPosition(9,5),pursuit.PreliminaryDestination);
                        Assert.Equal(new MapPosition(9,5),prior.Battle.Roster.Single(unit=>unit.Index==129).Position);
                        Assert.Equal(new MapPosition(5,4),prior.Battle.Roster.Single(unit=>unit.Index==130).Position);
                        Assert.Equal(new byte[] {0,3,255},pursuit.PreliminaryMoveString);
                        Assert.Equal(new MapPosition(9,4),pursuit.Destination); Assert.Equal(new byte[] {0,255},pursuit.MoveString);
                        Assert.Equal(new[] {28,28,28},pursuit.TargetCosts.Select(item=>item.Cost)); Assert.Equal(2,pursuit.GridCost);
                    }
                    if(expected.Number==3 && pursuit.ActorIndex==132)
                    {
                        Assert.Equal((byte)7,pursuit.CommandSet); Assert.Equal(new MapPosition(11,6),pursuit.Destination);
                        Assert.Equal(new byte[] {0,3,255},pursuit.MoveString); Assert.Equal(4,pursuit.GridCost);
                    }
                }
                Assert.All(current.Battle.Roster,unit=>Assert.Equal(unit.Index,current.Battle.OccupantAt(unit.Position)));
            }
            Assert.Equal(expected.Steps,thinkingSteps); Assert.Equal((ushort?)expected.Copy,current.Battle.RandomSeedCopy);
            Assert.Equal(expected.Memory,current.Battle.AiMemory.Take(6)); Assert.All(current.Battle.AiMemory.Skip(6),value=>Assert.Equal(0,value));
            Assert.Equal(18,current.Battle.FirstRound!.CurrentTurnOffset);
            current=Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot;
            Assert.Equal(64,current.Battle.FirstRound!.Slots.Count);
            Assert.All(current.Battle.FirstRound.Slots.Skip(9),slot=>Assert.Equal(new Battle01TurnEntry(255,255),slot));
            if(expected.Number==3)
            {
                Assert.Equal(new byte[] {1,2,130,132,129,131,0,128,133},current.Battle.FirstRound.Slots.Take(9).Select(slot=>slot.CombatantIndex));
                Assert.Equal(new byte[] {6,6,6,6,5,5,4,4,4},current.Battle.FirstRound.Slots.Take(9).Select(slot=>slot.AlteredAgility));
                current=Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current,1)).Snapshot;
                Assert.Equal(1,current.Battle.FirstControl!.ActorIndex);
            }
            if(expected.Number==4)
            {
                Assert.Equal(new byte[] {1,2,131,133,129,132,0,128,130},current.Battle.FirstRound!.Slots.Take(9).Select(slot=>slot.CombatantIndex));
                Assert.Equal(new byte[] {6,6,6,6,5,5,4,4,4},current.Battle.FirstRound.Slots.Take(9).Select(slot=>slot.AlteredAgility));
            }
        }
        Assert.Equal(new byte[] {2,129,130,1,128,132,0,131,133},current.Battle.FirstRound!.Slots.Take(9).Select(slot=>slot.CombatantIndex));
        Assert.Equal(new byte[] {7,6,6,5,5,5,4,4,4},current.Battle.FirstRound.Slots.Take(9).Select(slot=>slot.AlteredAgility));
        for(int slot=0;slot<5;slot++) current=CompleteCurrentTurn();
        string frozenBoundary=JsonSerializer.Serialize(current.Battle);
        var boundary=Assert.IsType<PrivateOriginalBattle01AttackSelectionRequired>(session.CompletePrivateOriginalBattle01EnemyPursuit(current,132));
        Assert.Equal(132,boundary.ActorIndex); Assert.Equal("attack.targets",boundary.Diagnostic.Field);
        Assert.Equal(new[] {new Battle01AttackCandidate(0,new(11,14),8)},boundary.Targets);
        Assert.Same(current,session.PrivateOriginalBattle01); Assert.Equal(frozenBoundary,JsonSerializer.Serialize(current.Battle));
        Assert.Equal(6,current.Battle.FirstRound!.RoundNumber); Assert.Equal(10,current.Battle.FirstRound.CurrentTurnOffset);
        Assert.Equal(new MapPosition(11,10),current.Battle.Roster.Single(unit=>unit.Index==132).Position);
        Assert.Equal(0x07821234u,current.Battle.RandomSeedImage); Assert.Equal((ushort?)0x0034,current.Battle.RandomSeedCopy);
        Assert.Equal(new byte[] {4,0x34,0x24,0x24,0x34,0x24},current.Battle.AiMemory.Take(6));
        Assert.Equal(0,current.Battle.NewlyTestedRegionMask); Assert.Equal(128,current.Battle.TurnCompletion!.CompletedActorIndex);
        history.Clear(); for(var receipt=current.Battle.TurnCompletion;receipt is not null;receipt=receipt.Previous) history.Add(receipt);
        Assert.Equal(50,history.Count); Assert.Equal(6,history.Count(receipt=>receipt.EnemyPursuit is not null));
        var attackInput=current;
        current=Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current,132)).Snapshot;
        var attack=current.Battle.TurnCompletion!.EnemyPhysicalAttack!;
        Assert.Equal(0,attack.TargetIndex); Assert.Equal(3,Assert.Single(attack.Priorities).Priority);
        Assert.Equal(57,attack.Priorities[0].Roll.GeneratorSteps);
        Assert.Equal(new ushort[] {32,32,1,1,32,32},attack.Effect.Rolls.Select(roll=>roll.Range));
        Assert.Equal(new ushort[] {12,30,0,0,11,21},attack.Effect.Rolls.Select(roll=>roll.Result));
        Assert.Equal(new byte[] {3,3,3,3,255},attack.MoveString); Assert.Equal(new MapPosition(11,14),attack.Destination);
        Assert.Equal((9,12,9),((int)attack.Effect.TemporaryHp,(int)attack.Effect.RestoredHp,(int)current.Battle.Roster[0].Stats.HpCurrent));
        Assert.Equal(0xAF881234u,current.Battle.RandomSeedImage); Assert.Equal((ushort?)0x0134,current.Battle.RandomSeedCopy);
        Assert.Equal(0,current.Battle.AiLastTargets[4]); Assert.Equal(attackInput.Battle.AiMemory,current.Battle.AiMemory);
        Assert.Same(attackInput.Battle.FirstRound!.Slots,current.Battle.FirstRound!.Slots);
        Assert.Same(attackInput.Preparation,current.Preparation); Assert.Equal(12,current.Preparation.Party.Allies[0].HpCurrent);
        Assert.Equal(12,current.Battle.FirstRound.CurrentTurnOffset);
        history.Clear(); for(var receipt=current.Battle.TurnCompletion;receipt is not null;receipt=receipt.Previous) history.Add(receipt);
        Assert.Equal(51,history.Count); Assert.Same(attackInput.Battle.TurnCompletion,current.Battle.TurnCompletion.Previous);
        current=Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current,0)).Snapshot;
        Assert.Equal(0,current.Battle.FirstControl!.ActorIndex); Assert.Equal(12,current.Battle.FirstControl.Movement.Range.Budget);
        current=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(current,0,new(12,15))).Snapshot;
        current=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current,0)).Snapshot;
        current=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.CancelPrivateOriginalBattle01PlayerMovement(current,0)).Snapshot;
        Assert.Equal(new MapPosition(11,15),current.Battle.Roster[0].Position); Assert.Equal(9,current.Battle.Roster[0].Stats.HpCurrent);
        current=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current,0)).Snapshot;
        current=Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(current,0)).Snapshot;
        Assert.Equal(new[] {132},current.Battle.FirstControl!.Movement.Attack!.Targets);
        var selectedPlayer=current;
        current=Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.ConfirmPrivateOriginalBattle01PlayerAttack(current,0)).Snapshot;
        var playerStrike=current.Battle.TurnCompletion!.PlayerPhysicalAttack!;
        Assert.Equal(new ushort[] {8,16,1,1,32,32,16,16},playerStrike.Effect.Rolls.Select(r=>r.Range));
        Assert.Equal(new ushort[] {7,14,0,0,12,31,15,14},playerStrike.Effect.Rolls.Select(r=>r.Result));
        Assert.Equal(new uint[] {0xE9EF1234,0xE12A1234,0x6F291234,0xA51C1234,0x62731234,0xFFDE1234,0xFE4D1234,0xE9F01234},playerStrike.Effect.Rolls.Select(r=>r.AfterImage));
        Assert.Equal((3,2,5,2), (playerStrike.Effect.Damage,(int)playerStrike.Effect.TemporaryHp,(int)playerStrike.Effect.RestoredHp,(int)current.Battle.Roster[7].Stats.HpCurrent));
        Assert.Equal((30,15,15), (playerStrike.AccumulatedExp,playerStrike.HalvedExp,playerStrike.AwardedExp));
        Assert.Equal((byte?)15,current.Battle.Roster[0].Stats.CurrentExp); Assert.Equal((byte?)0,current.Preparation.Party.Allies[0].CurrentExp);
        Assert.Equal(OriginalBattle01ControlledPartyPreset.PlayerAttackComparisonId,current.Preparation.Party.Id);
        Assert.Equal(selectedPlayer.Battle.RandomSeedCopy,current.Battle.RandomSeedCopy);
        Assert.Same(selectedPlayer.Battle.AiMemory,current.Battle.AiMemory); Assert.Same(selectedPlayer.Battle.AiLastTargets,current.Battle.AiLastTargets);
        Assert.Equal(9,current.Battle.Roster[0].Stats.HpCurrent); Assert.Equal(131,current.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex);
        Assert.Same(attack,current.Battle.TurnCompletion!.Previous!.EnemyPhysicalAttack);
        history.Clear(); for(var receipt=current.Battle.TurnCompletion;receipt is not null;receipt=receipt.Previous) history.Add(receipt);
        Assert.Equal(52,history.Count); Assert.Equal(14,current.Battle.FirstRound.CurrentTurnOffset);
        current=Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(session.CompletePrivateOriginalBattle01EnemyPursuit(current,131)).Snapshot;
        Assert.Equal(new MapPosition(11,8),current.Battle.Roster[6].Position);
        Assert.Equal(new byte[] {3,3,255},current.Battle.TurnCompletion!.EnemyPursuit!.MoveString);
        current=Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(session.CompletePrivateOriginalBattle01EnemyStandby(current,133)).Snapshot;
        var standby133=current.Battle.TurnCompletion!.EnemyStandby!;
        Assert.Equal(new MapPosition(7,5),standby133.Destination); Assert.Equal(new byte[] {1,0,255},standby133.MoveString);
        Assert.Equal(new byte[] {8,3},standby133.Rolls.Select(r=>r.Range)); Assert.Equal(new byte[] {7,2},standby133.Rolls.Select(r=>r.Result));
        Assert.Equal(new[] {114,19},standby133.Rolls.Select(r=>r.GeneratorSteps));
        Assert.Equal((ushort?)0x0234,current.Battle.RandomSeedCopy); Assert.Equal(new byte[] {4,52,36,36,52,52},current.Battle.AiMemory.Take(6));
        current=Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot;
        Assert.Equal(7,current.Battle.FirstRound!.RoundNumber); Assert.Equal(0xCF491234u,current.Battle.RandomSeedImage);
        Assert.Equal(new byte[] {2,131,132,133,0,1,129,128,130},current.Battle.FirstRound.Slots.Take(9).Select(s=>s.CombatantIndex));
        Assert.Equal(new byte[] {6,6,6,6,5,5,5,4,4},current.Battle.FirstRound.Slots.Take(9).Select(s=>s.AlteredAgility));
        Assert.Equal(64,current.Battle.FirstRound.Slots.Count); Assert.All(current.Battle.FirstRound.Slots.Skip(9),s=>Assert.Equal(new Battle01TurnEntry(255,255),s));
        current=Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current,2)).Snapshot;
        Assert.Equal(2,current.Battle.FirstControl!.ActorIndex); Assert.Equal(14,current.Battle.FirstControl.Movement.Range.Budget);
        Assert.Equal(new MapPosition(7,17),current.Battle.Roster[2].Position); Assert.Equal(11,current.Battle.Roster[2].Stats.HpCurrent);
        Assert.Equal((9,2,(byte?)15),((int)current.Battle.Roster[0].Stats.HpCurrent,(int)current.Battle.Roster[7].Stats.HpCurrent,current.Battle.Roster[0].Stats.CurrentExp));
        Assert.Same(selectedPlayer.Preparation,current.Preparation); Assert.Same(selectedPlayer.SourceSnapshot,current.SourceSnapshot);

        PrivateOriginalBattle01SessionSnapshot CompleteCurrentTurn()
        {
            var input=session.PrivateOriginalBattle01!; int id=input.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
            if(id>=128)
                return (input.Battle.Roster.Single(unit=>unit.Index==id).AiBitfield!.Value&1)!=0
                    ? Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(session.CompletePrivateOriginalBattle01EnemyPursuit(input,id)).Snapshot
                    : Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(session.CompletePrivateOriginalBattle01EnemyStandby(input,id)).Snapshot;
            var player=input.Battle.FirstControl is not null ? input :
                Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(input,id)).Snapshot;
            var moved=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(player,id)).Snapshot;
            return Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(moved,id)).Snapshot;
        }
    }

    private static GameSession SeedControlledPending(string canonical)
    {
        const string digest = "DDDA4FA05455DDBA9CDAF85497CEE0C1C89C6E625721A8FEAD301044C892E508";
        var session = Assert.IsType<PrivateOriginalMapGameSessionStarted>(GameSession.StartPrivateOriginalMap(
            new PrivateCanonicalMap3ImportReader(canonical),
            new(PrivateCanonicalMap3ImportReader.PackageId, ContentProfile.PrivateLocal, digest))).Session;
        var initial = session.PrivateOriginalMapSnapshot; var definition = initial.Definition;
        var runtime = definition.RuntimeCatalog.Resolve(new MapId("map40"));
        static T State<T>(string method, params object[] args) => (T)typeof(T)
            .GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, args)!;
        static T Receipt<T>(params object[] args) => (T)Activator.CreateInstance(typeof(T),
            BindingFlags.Instance | BindingFlags.NonPublic, null, args, null)!;
        // Explicit test-owned completed-route seed using accepted factories and the real canonical import.
        // This establishes the startup seam, not natural continuity or execution of the skipped programs.
        var entry = new PrivateOriginalMapSessionSnapshot(definition, initial.Receipt, runtime.WorkingLayout,
            5, new(14, 14), runtime.Traversal.TryMove(runtime.WorkingLayout, new(14, 15), ExplorationDirection.North),
            false, null,
            zone601: State<PrivateOriginalMapZone601State>("AstralZoneRepositioned", definition.Zone601!, definition.AstralZone!),
            sarah: State<PrivateOriginalMapSarahState>("MessengerFollowerReady", definition.Sarah!, definition.AstralZone!, definition.MessengerAcceptance!),
            entity142: State<PrivateOriginalMapEntity142State>("ReleaseRouteOccupancy", definition.Entity142!,
                State<PrivateOriginalMapEntity142State>("Acknowledged", definition.Entity142!, 1L), definition.MessengerAcceptance!),
            castleGate: State<PrivateOriginalMapCastleGateState>("Completed", definition.CastleGate!),
            currentRuntime: runtime,
            palaceFirstVisit: Receipt<PrivateOriginalMapPalaceFirstVisitReceipt>(definition.PalaceFirstVisit!, 2L),
            astralAcceptance: Receipt<PrivateOriginalMapAstralAcceptanceState>(definition.AstralAcceptance!, 3L),
            middleTowerGuard: Receipt<PrivateOriginalMapMiddleTowerGuardReceipt>(definition.MiddleTowerGuard!, 4L));
        typeof(GameSession).GetField("_privateOriginalMapSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, entry);
        typeof(GameSession).GetField("_privateOriginalMapPlayerLocomotion", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session,
            State<PrivateOriginalMapPlayerLocomotionSnapshot>("ControlledAdmission", entry.PlayerPosition));
        session.BeginPrivateOriginalMapPlayerLocomotion(new(ExplorationDirection.North));
        while (session.PrivateOriginalMapPlayerLocomotion.IsMoving) session.AdvancePrivateOriginalMapPlayerLocomotion();
        Assert.Equal(new MapPosition(14, 13), session.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(1, session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing);
        Assert.NotNull(session.ApplyPrivateOriginalMap(new(ExplorationDirection.North)).Battle01Admission);
        return session;
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

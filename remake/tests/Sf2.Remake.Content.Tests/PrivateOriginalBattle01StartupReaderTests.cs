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
        Assert.All(definition.Entities.Skip(3), entity => Assert.Equal(127, entity.ItemWord));
        Assert.Equal(5, definition.EnemyBaseline.HpMax);
        Assert.All(definition.EnemyBaseline.Items, item => Assert.Equal(127, item));
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
        => ReachRealRoundNineChester(OriginalBattle01ControlledPartyPreset.FirstDefeatComparison);

    private static GameSession ReachRealRoundNineChester(OriginalBattle01ControlledPartyPreset party)
    {
        string placement = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_DATA");
        string scene = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_SCENE");
        string terrain = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_TERRAIN");
        string canonical = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_CANONICAL_MAP_IMPORT");
        var session = SeedControlledPending(canonical);
        var prepared = Assert.IsType<PrivateOriginalBattle01StartupPrepared>(session.PreparePrivateOriginalBattle01Startup(
            session.PrivateOriginalBattle01Admission, new PrivateOriginalBattle01StartupReader(placement, scene, terrain),
            party));
        var initialized = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared)).Snapshot;
        var state = initialized.Battle;
        Assert.Equal(GameFlowStage.Battle, session.PrivateOriginalFlowStage);
        Assert.Equal(new MapId("map57"), session.PrivateOriginalCurrentMap);
        Assert.Equal(new MapId("map40"), initialized.SourceSnapshot.Map); Assert.Null(session.PrivateOriginalBattle01Admission);
        Assert.Equal(Battle01Phase.BeforeFirstRound, state.Phase); Assert.Equal(0x1234u, state.RandomSeedImage);
        Assert.Equal((uint?)0, state.CurrentGold); Assert.Equal((ushort?)0, state.Roster[0].Stats.CurrentKills);
        Assert.All(state.Roster.Skip(1), unit => Assert.Null(unit.Stats.CurrentKills));
        Assert.Equal(party.Allies.Select(ally => ally.CurrentExp), state.Roster.Take(3).Select(unit => unit.Stats.CurrentExp));
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
            Assert.Equal(source.Position, actual.Position); Assert.Equal(actual.Index, state.OccupantAt(Assert.IsType<MapPosition>(actual.Position)));
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
            Assert.Equal(unit.Index, roundState.OccupantAt(Assert.IsType<MapPosition>(unit.Position)));
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
        var control = controlled.Battle.FirstControl!; var origin = Assert.IsType<MapPosition>(actor.Position);
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
                Assert.All(current.Battle.Roster,unit=>Assert.Equal(unit.Index,current.Battle.OccupantAt(Assert.IsType<MapPosition>(unit.Position))));
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
        Assert.Equal(party.Id,current.Preparation.Party.Id);
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

        var roundSeven = current;
        current = CompleteCurrentTurn();
        Assert.Equal((byte)2, current.Battle.FirstRound!.CurrentTurnOffset);
        current = CompleteCurrentTurn();
        Assert.Equal(new MapPosition(11, 10), current.Battle.Roster[6].Position);
        Assert.Equal(new byte[] { 3, 3, 255 }, current.Battle.TurnCompletion!.EnemyPursuit!.MoveString);
        current = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current, 132)).Snapshot;
        var secondEnemyStrike = current.Battle.TurnCompletion!.EnemyPhysicalAttack!;
        Assert.Equal(new[] { 2, 3 }, secondEnemyStrike.Priorities.Select(p => p.PotentialDamage));
        Assert.Equal(new[] { 1, 19 }, secondEnemyStrike.Priorities.Select(p => p.Priority));
        Assert.Equal(new[] { 66, 57 }, secondEnemyStrike.Priorities.Select(p => p.Roll.GeneratorSteps));
        Assert.Equal(new ushort[] { 32, 32, 1, 1, 32, 32 }, secondEnemyStrike.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 16, 26, 0, 0, 2, 3 }, secondEnemyStrike.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0x86BC1234, 0xD7931234, 0xF27E1234, 0x506D1234, 0x15901234, 0x18571234 },
            secondEnemyStrike.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((6, 9, 6), ((int)secondEnemyStrike.Effect.TemporaryHp, (int)secondEnemyStrike.Effect.RestoredHp,
            (int)current.Battle.Roster[0].Stats.HpCurrent));
        Assert.Equal(11, current.Battle.Roster[1].Stats.HpCurrent); Assert.Equal(5, current.Battle.Roster[1].Stats.Defense);
        current = CompleteCurrentTurn();
        Assert.Equal(new MapPosition(6, 6), current.Battle.Roster[8].Position);
        Assert.Equal(new byte[] { 2, 3, 255 }, current.Battle.TurnCompletion!.EnemyStandby!.MoveString);
        Assert.Equal(new[] { 114, 19 }, current.Battle.TurnCompletion.EnemyStandby.Rolls.Select(r => r.GeneratorSteps));
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(
            session.BeginPrivateOriginalBattle01PlayerAttack(current, 0)).Snapshot;
        var lethalSelection = current; string frozenLethal = JsonSerializer.Serialize(current);
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerAttack(current, 0)).Snapshot;
        var defeatReceipt = current.Battle.TurnCompletion!; var lethal = defeatReceipt.PlayerPhysicalAttack!;
        Assert.Equal(new ushort[] { 8, 16, 1, 1, 16, 16 }, lethal.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 1, 1, 0, 0, 14, 15 }, lethal.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0x3C721234, 0x11D11234, 0xE7A41234, 0xC35B1234, 0xEBA61234, 0xF7751234 },
            lethal.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((3, 0, 2, 0), (lethal.Effect.Damage, (int)lethal.Effect.TemporaryHp, (int)lethal.Effect.RestoredHp,
            (int)current.Battle.Roster[7].Stats.HpCurrent));
        Assert.Equal(new Battle01PhysicalReaction(132, -3, 0, 0, 1), lethal.Effect.Reaction);
        Assert.Equal((49, 24, 24, (byte?)39), (lethal.AccumulatedExp, lethal.HalvedExp, lethal.AwardedExp,
            current.Battle.Roster[0].Stats.CurrentExp));
        Assert.Equal(((uint?)0, (uint?)60, (uint?)60, (ushort?)1),
            (lethal.GoldBefore, lethal.GoldAfter, current.Battle.CurrentGold, current.Battle.Roster[0].Stats.CurrentKills));
        Assert.Equal(new[] { 132 }, defeatReceipt.EnemyDefeat!.FirstWorklist);
        Assert.Empty(defeatReceipt.EnemyDefeat.AfterTurnWorklist);
        Assert.Equal((0, 0, 1), (defeatReceipt.EnemyDefeat.CreditedAlly, (int)defeatReceipt.EnemyDefeat.KillsBefore,
            (int)defeatReceipt.EnemyDefeat.KillsAfter));
        Assert.Equal(new Battle01FactionCounts(3, 5), defeatReceipt.BeforeAfterTurn);
        Assert.Equal(defeatReceipt.BeforeAfterTurn, defeatReceipt.AfterAfterTurn);
        Assert.Equal(9, current.Battle.Roster.Count); Assert.Equal(8, current.Battle.Occupancy.Count(id => id >= 0));
        Assert.Null(current.Battle.Roster[7].Position); Assert.Equal(-1, current.Battle.OccupantAt(new(11, 14)));
        Assert.Same(lethalSelection.Battle.Roster[7].Deployment, current.Battle.Roster[7].Deployment);
        Assert.Same(lethalSelection.Battle.Roster[7].EnemySource, current.Battle.Roster[7].EnemySource);
        Assert.Equal(5, current.Battle.Roster[7].EnemySource!.SourceStats.HpCurrent);
        Assert.All(current.Battle.Roster.Where(unit => unit.Stats.HpCurrent > 0),
            unit => Assert.Equal(unit.Index, current.Battle.OccupantAt(Assert.IsType<MapPosition>(unit.Position))));
        Assert.Same(roundSeven.Battle.FirstRound!.Slots, current.Battle.FirstRound!.Slots);
        history.Clear(); for (var receipt = defeatReceipt; receipt is not null; receipt = receipt.Previous) history.Add(receipt);
        Assert.Equal(59, history.Count); Assert.Equal((byte)10, current.Battle.FirstRound.CurrentTurnOffset);
        Assert.Equal(0xF7751234u, current.Battle.RandomSeedImage); Assert.Equal((ushort?)0x0234, current.Battle.RandomSeedCopy);
        Assert.Same(lethalSelection.Battle.AiMemory, current.Battle.AiMemory);
        Assert.Same(lethalSelection.Battle.AiLastTargets, current.Battle.AiLastTargets);
        Assert.Equal(frozenLethal, JsonSerializer.Serialize(lethalSelection));
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(current, 1)).Snapshot;
        Assert.Equal((1, 10, 11), (current.Battle.FirstControl!.ActorIndex, current.Battle.FirstControl.Movement.Range.Budget,
            (int)current.Battle.Roster[1].Stats.HpCurrent));
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(current, 1, new(10, 17))).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 1)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.CancelPrivateOriginalBattle01PlayerMovement(current, 1)).Snapshot;
        Assert.Equal(new MapPosition(9, 17), current.Battle.Roster[1].Position);
        Assert.Same(defeatReceipt, current.Battle.TurnCompletion);
        Assert.Equal(((uint?)60, (ushort?)1, (byte?)39), (current.Battle.CurrentGold,
            current.Battle.Roster[0].Stats.CurrentKills, current.Battle.Roster[0].Stats.CurrentExp));
        Assert.Same(prepared, current.Preparation); Assert.Equal((uint?)0, prepared.Party.CurrentGold);
        Assert.Equal((ushort?)0, prepared.Party.Allies[0].CurrentKills);
        Assert.Same(initialized.SourceSnapshot, current.SourceSnapshot);

        // Continue the same real input route and first-defeat preset; no post-hoc stat/seed injection.
        current = CompleteCurrentTurn(); // Actual R7 player1 confirms origin and STAY.
        foreach (int id in new[] { 129, 128, 130 })
        {
            Assert.Equal((byte?)id, current.Battle.FirstRound!.CurrentCandidate?.CombatantIndex);
            current = CompleteCurrentTurn();
        }
        current = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot;
        var roundEightSlots = current.Battle.FirstRound!.Slots;
        Assert.Equal(new[] { new Battle01TurnEntry(2, 8), new(133, 6), new(0, 5), new(1, 5),
            new(129, 5), new(128, 4), new(130, 4), new(131, 4) }.Concat(Enumerable.Repeat(new Battle01TurnEntry(255, 255), 56)), roundEightSlots);
        Assert.Equal((0xB28D1234u, (ushort?)0x5634), (current.Battle.RandomSeedImage, current.Battle.RandomSeedCopy));
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current, 2)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(current, 2, new(11, 14))).Snapshot;
        Assert.Equal(14, current.Battle.FirstControl!.Movement.GridCost);
        Assert.Equal(new byte[] { 0, 0, 0, 1, 0, 1, 1, 255 }, current.Battle.FirstControl.Movement.Preview.Directions);
        current = CompleteCurrentTurn(); current = CompleteCurrentTurn(); // Chester STAY, then actual133.
        foreach (int id in new[] { 0, 1, 129, 128, 130 })
        {
            Assert.Equal((byte?)id, current.Battle.FirstRound!.CurrentCandidate?.CombatantIndex);
            current = CompleteCurrentTurn(); // Bowie and player1 confirm origin; actual enemy standby.
        }
        var beforeChesterHit = current;
        Assert.Equal((8, (byte)14, (byte?)131), (current.Battle.FirstRound!.RoundNumber,
            current.Battle.FirstRound.CurrentTurnOffset, current.Battle.FirstRound.CurrentCandidate?.CombatantIndex));
        Assert.Equal((0xB28D1234u, (ushort?)0x0034), (current.Battle.RandomSeedImage, current.Battle.RandomSeedCopy));
        Assert.Equal(70, CountReceipts(current.Battle));
        var cohort = Assert.IsType<PrivateOriginalBattle01AttackSelectionRequired>(session.CompletePrivateOriginalBattle01EnemyPursuit(current, 131));
        Assert.Equal(2, Assert.Single(cohort.Targets).ActorIndex); Assert.Same(current, session.PrivateOriginalBattle01);
        current = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current, 131)).Snapshot;
        var chesterReceipt = current.Battle.TurnCompletion!; var chesterHit = chesterReceipt.EnemyPhysicalAttack!;
        var priority = Assert.Single(chesterHit.Priorities);
        Assert.Equal("battle01-class1-wooden-stick-effective-prowess3-v1", chesterHit.CombatProfile);
        Assert.Equal((131, 2, 6, 230, 2, 9, 7), (chesterHit.ActorIndex, chesterHit.TargetIndex, chesterHit.GridCost,
            priority.LandMultiplier, priority.PotentialDamage, priority.RemainingHp, priority.Priority));
        Assert.Equal(new MapPosition(11, 13), chesterHit.Destination); Assert.Equal(new byte[] { 3, 3, 3, 255 }, chesterHit.MoveString);
        Assert.Equal((57, (ushort)0x0034, (ushort)0x0134, (byte)1), (priority.Roll.GeneratorSteps,
            priority.Roll.BeforeSeedCopy, priority.Roll.AfterSeedCopy, priority.Roll.Result));
        Assert.Equal(new ushort[] { 32, 32, 1, 1, 32, 32 }, chesterHit.Effect.Rolls.Select(roll => roll.Range));
        Assert.Equal(new ushort[] { 2, 27, 0, 0, 25, 13 }, chesterHit.Effect.Rolls.Select(roll => roll.Result));
        Assert.Equal(new uint[] { 0x11301234, 0xDF771234, 0x59121234, 0x85F11234, 0xCD441234, 0x6C7B1234 }, chesterHit.Effect.Rolls.Select(roll => roll.AfterImage));
        Assert.Equal((9, 11, 9), ((int)chesterHit.Effect.TemporaryHp, (int)chesterHit.Effect.RestoredHp, (int)current.Battle.Roster[2].Stats.HpCurrent));
        Assert.Equal(new Battle01PhysicalReaction(2, -2, 0, 0, 1), chesterHit.Effect.Reaction);
        Assert.False(chesterHit.Effect.Dodged); Assert.False(chesterHit.Effect.Critical);
        Assert.Same(beforeChesterHit.Battle.Roster[2].Stats, chesterHit.Effect.BeforeStats);
        Assert.Equal(new Battle01FactionCounts(3, 5), chesterReceipt.BeforeAfterTurn); Assert.Equal(chesterReceipt.BeforeAfterTurn, chesterReceipt.AfterAfterTurn);
        Assert.Null(chesterReceipt.EnemyDefeat); Assert.Same(beforeChesterHit.Battle.TurnCompletion, chesterReceipt.Previous);
        Assert.Equal(71, CountReceipts(current.Battle)); Assert.Equal(0, current.Battle.NewlyTestedRegionMask);
        Assert.Equal((0x6C7B1234u, (ushort?)0x0134), (current.Battle.RandomSeedImage, current.Battle.RandomSeedCopy));
        Assert.Same(roundEightSlots, current.Battle.FirstRound!.Slots);
        Assert.Equal(new byte[] { 4, 52, 4, 36, 52, 52 }, current.Battle.AiMemory.Take(6));
        Assert.Equal(new byte[] { 255, 255, 255, 2, 0, 255 }, current.Battle.AiLastTargets.Take(6));
        current = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot;
        Assert.Equal(new[] { new Battle01TurnEntry(2, 8), new(128, 6), new(129, 6), new(131, 6),
            new(133, 5), new(1, 4), new(130, 4), new(0, 3) }.Concat(Enumerable.Repeat(new Battle01TurnEntry(255, 255), 56)), current.Battle.FirstRound!.Slots);
        Assert.Equal((0x71D31234u, (ushort?)0x0134), (current.Battle.RandomSeedImage, current.Battle.RandomSeedCopy));
        Assert.Equal(7, current.Battle.NewlyTestedRegionMask); Assert.Equal(new[] { false, true, false }, current.Battle.RegionFlags90Through105.Take(3));
        Assert.Equal(new ushort?[] { 0x2060, 0x2060, 0x2060, 0x2061, 0x2071, 0x2070 }, current.Battle.Roster.Skip(3).Select(u => u.AiBitfield));
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current, 2)).Snapshot;
        var chesterReady = current;
        Assert.Equal((9, (byte)0, 2, 14), (current.Battle.FirstRound!.RoundNumber, current.Battle.FirstRound.CurrentTurnOffset,
            current.Battle.FirstControl!.ActorIndex, current.Battle.FirstControl.Movement.Range.Budget));
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(current, 2, new(12, 14))).Snapshot;
        Assert.Equal(2, current.Battle.FirstControl!.Movement.GridCost);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 2)).Snapshot;
        Assert.Equal(new MapPosition(12, 14), current.Battle.Roster[2].Position);
        Assert.Equal(party.Allies[2].CurrentExp is null ? "attack.targetProfile" : "attack.emptyTargets",
            Assert.IsType<PrivateOriginalBattle01PlayerAttackRejected>(session.BeginPrivateOriginalBattle01PlayerAttack(current, 2)).Diagnostic.Field);
        Assert.Same(current, session.PrivateOriginalBattle01);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.CancelPrivateOriginalBattle01PlayerMovement(current, 2)).Snapshot;
        Assert.Equal(chesterReady.Battle.Occupancy, current.Battle.Occupancy); Assert.Same(chesterReceipt, current.Battle.TurnCompletion);
        Assert.Same(chesterReady.Battle.FirstRound, current.Battle.FirstRound);
        Assert.Equal(chesterReady.Battle.RandomSeedImage, current.Battle.RandomSeedImage); Assert.Equal(chesterReady.Battle.RandomSeedCopy, current.Battle.RandomSeedCopy);
        Assert.Equal(chesterReady.Battle.AiMemory, current.Battle.AiMemory); Assert.Equal(chesterReady.Battle.AiLastTargets, current.Battle.AiLastTargets);
        Assert.Equal(new MapPosition?[] { new(11, 15), new(9, 17), new(11, 14), new(7, 2), new(10, 4), new(6, 3), new(11, 13), null, new(7, 5) }, current.Battle.Roster.Select(u => u.Position));
        Assert.Equal(new ushort[] { 6, 11, 9, 5, 5, 5, 5, 0, 5 }, current.Battle.Roster.Select(u => u.Stats.HpCurrent));
        Assert.Equal(((uint?)60, (byte?)39, (ushort?)1), (current.Battle.CurrentGold, current.Battle.Roster[0].Stats.CurrentExp, current.Battle.Roster[0].Stats.CurrentKills));
        Assert.Equal(party.Allies[2].CurrentExp, current.Battle.Roster[2].Stats.CurrentExp); Assert.Null(current.Battle.Roster[2].Stats.CurrentKills);
        Assert.Equal(8, current.Battle.Occupancy.Count(id => id >= 0)); Assert.Same(prepared, current.Preparation);
        Assert.Equal(11, prepared.Party.Allies[2].HpCurrent); Assert.Equal(party.Allies[2].CurrentExp, prepared.Party.Allies[2].CurrentExp);
        Assert.Same(initialized.SourceLocomotion, current.SourceLocomotion); Assert.Same(initialized.SourceBridge, current.SourceBridge);
        return session;

        static int CountReceipts(Battle01InitializedState battle)
        {
            int count = 0; for (var receipt = battle.TurnCompletion; receipt is not null; receipt = receipt.Previous) count++;
            return count;
        }

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

    [Sf2.Remake.TestSupport.PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE",
        "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_CANONICAL_MAP_IMPORT")]
    public void AcceptedSelectedInputsContinueChesterPlayerAttackFromAuthoredExpZero()
        => ReachRealRoundNineSarah();

    private static GameSession ReachRealRoundNineSarah()
        => ReachRealRoundNineSarahWithParty(OriginalBattle01ControlledPartyPreset.ChesterPlayerAttackComparison);

    private static GameSession ReachRealRoundNineSarahWithParty(OriginalBattle01ControlledPartyPreset party)
    {
        var session = ReachRealRoundNineChester(party);
        var ready = session.PrivateOriginalBattle01!; var original = ready.Battle; var order = original.FirstRound!;
        var current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(ready, 2)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(current, 2)).Snapshot;
        Assert.Equal(new[] { 131 }, current.Battle.FirstControl!.Movement.Attack!.Targets);
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.CyclePrivateOriginalBattle01PlayerAttackTarget(current, 2, 1)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.CancelPrivateOriginalBattle01PlayerAttackTarget(current, 2)).Snapshot;
        Assert.Equal(Battle01Phase.PlayerActionChoice, current.Battle.Phase);
        Assert.Same(original.TurnCompletion, current.Battle.TurnCompletion);
        Assert.Equal((0x71D31234u, (ushort?)0x0134), (current.Battle.RandomSeedImage, current.Battle.RandomSeedCopy));
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(current, 2)).Snapshot;
        var selected = current;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.ConfirmPrivateOriginalBattle01PlayerAttack(current, 2)).Snapshot;
        var attackReceipt = current.Battle.TurnCompletion!; var d = attackReceipt.PlayerPhysicalAttack!;
        Assert.Equal("battle01-class1-wooden-stick-effective-prowess3-v1", d.CombatProfile);
        Assert.Equal((2, 131, 1, 230), (d.ActorIndex, d.TargetIndex, (int)d.TargetTerrain, d.LandMultiplier));
        Assert.Equal(new MapPosition(11, 14), d.RangeOrigin); Assert.Equal(d.RangeOrigin, d.MovementOrigin);
        Assert.Equal(new byte[] { 255 }, d.MoveString);
        Assert.Equal(new ushort[] { 8, 16, 1, 1, 32, 32, 16, 16 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 6, 2, 0, 0, 24, 1, 8, 10 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0xC7BE1234, 0x24AD1234, 0xDCD01234, 0x36971234, 0xC5B21234,
            0x0A111234, 0x82E41234, 0xA59B1234 }, d.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((2, 3, 5, 3, 20, 10, 10), (d.Effect.Damage, (int)d.Effect.TemporaryHp,
            (int)d.Effect.RestoredHp, (int)d.Effect.AfterStats.HpCurrent, d.AccumulatedExp, d.HalvedExp, d.AwardedExp));
        Assert.Equal(new Battle01PhysicalReaction(131, -2, 0, 0, 1), d.Effect.Reaction);
        Assert.False(d.Effect.Dodged); Assert.False(d.Effect.Critical); Assert.False(d.DefeatedTarget);
        Assert.Equal((byte?)0, d.Actor.Stats.CurrentExp); Assert.Equal((byte?)10, current.Battle.Roster[2].Stats.CurrentExp);
        Assert.Equal(9, current.Battle.Roster[2].Stats.HpCurrent); Assert.Null(current.Battle.Roster[2].Stats.CurrentKills);
        Assert.Same(Battle01PlayerPhysicalCompletionPolicy.ControlledNonlethalStrikeAndExp, attackReceipt.Policy);
        Assert.Same(original.TurnCompletion, attackReceipt.Previous); Assert.Null(attackReceipt.EnemyDefeat);
        Assert.Same(original.AiLastTargets, current.Battle.AiLastTargets); Assert.Same(original.AiMemory, current.Battle.AiMemory);
        Assert.Equal((byte)2, current.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01PlayerAttackRejected>(
            session.ConfirmPrivateOriginalBattle01PlayerAttack(selected, 2)).Diagnostic.Field);
        Assert.Same(current, session.PrivateOriginalBattle01);
        foreach (int actor in new[] { 128, 129 })
        {
            Assert.Equal(actor, current.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex);
            current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(
                session.CompletePrivateOriginalBattle01EnemyStandby(current, actor)).Snapshot;
            var standby = current.Battle.TurnCompletion!.EnemyStandby!;
            Assert.Equal(actor == 128 ? new MapPosition(8, 3) : new(9, 5), standby.Destination);
            Assert.Equal(actor == 128 ? new byte[] { 0, 3, 255 } : [3, 2, 255], standby.MoveString);
            Assert.Equal(actor == 128 ? new[] { 114, 1 } : [3, 15], standby.Rolls.Select(r => r.GeneratorSteps));
            Assert.Equal(actor == 128 ? new byte[] { 7, 0 } : [3, 2], standby.Rolls.Select(r => r.Result));
            Assert.Equal(actor == 128 ? (ushort)0x0434 : (ushort)0x0234, standby.SeedCopyAfter);
            Assert.Equal(0xA59B1234u, current.Battle.RandomSeedImage);
        }
        var beforeEnemy = current;
        var cohort = Assert.IsType<PrivateOriginalBattle01AttackSelectionRequired>(session.CompletePrivateOriginalBattle01EnemyPursuit(current, 131));
        Assert.Equal(new[] { 0, 1, 2 }, cohort.Targets.Select(t => t.ActorIndex)); Assert.Same(current, session.PrivateOriginalBattle01);
        current = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current, 131)).Snapshot;
        var enemy = current.Battle.TurnCompletion!.EnemyPhysicalAttack!;
        Assert.Equal("battle01-class0-wooden-sword-effective-prowess3-v1", enemy.CombatProfile);
        Assert.Equal(new[] { 2, 1, 0 }, enemy.Priorities.Select(p => p.Target.Index));
        Assert.Equal(new MapPosition[] { new(11, 13), new(9, 16), new(10, 15) }, enemy.Priorities.Select(p => p.Candidate.AttackPosition));
        Assert.Equal(new[] { 0, 10, 6 }, enemy.Priorities.Select(p => p.Candidate.GridCost));
        Assert.Equal(new[] { 230, 230, 230 }, enemy.Priorities.Select(p => p.LandMultiplier));
        Assert.Equal(new[] { 2, 2, 3 }, enemy.Priorities.Select(p => p.PotentialDamage));
        Assert.Equal(new[] { 7, 9, 3 }, enemy.Priorities.Select(p => p.RemainingHp));
        Assert.Equal(new[] { 1, 1, 7 }, enemy.Priorities.Select(p => p.Priority));
        Assert.Equal(new[] { 66, 57, 133 }, enemy.Priorities.Select(p => p.Roll.GeneratorSteps));
        Assert.Equal(new byte[] { 0, 1, 2 }, enemy.Priorities.Select(p => p.Roll.Result));
        Assert.Equal(new ushort[] { 0x0034, 0x0134, 0x0234 }, enemy.Priorities.Select(p => p.Roll.AfterSeedCopy));
        Assert.Equal((131, 0, 6), (enemy.ActorIndex, enemy.TargetIndex, enemy.GridCost));
        Assert.Equal(new MapPosition(10, 15), enemy.Destination); Assert.Equal(new byte[] { 2, 3, 3, 255 }, enemy.MoveString);
        Assert.Equal(new ushort[] { 32, 32, 1, 1, 32, 32 }, enemy.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 13, 10, 0, 0, 12, 4 }, enemy.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0x68E61234, 0x53B51234, 0x40381234, 0x42DF1234, 0x655A1234, 0x25991234 },
            enemy.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((3, 3, 6, 3), (enemy.Effect.Damage, (int)enemy.Effect.TemporaryHp, (int)enemy.Effect.RestoredHp,
            (int)current.Battle.Roster[0].Stats.HpCurrent));
        Assert.Equal(new Battle01PhysicalReaction(0, -3, 0, 0, 1), enemy.Effect.Reaction);
        Assert.False(enemy.Effect.Dodged); Assert.False(enemy.Effect.Critical);
        Assert.Same(beforeEnemy.Battle.Roster[6].Stats, current.Battle.Roster[6].Stats);
        Assert.Same(beforeEnemy.Battle.Roster[2].Stats, current.Battle.Roster[2].Stats);
        current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(
            session.CompletePrivateOriginalBattle01EnemyStandby(current, 133)).Snapshot;
        var last = current.Battle.TurnCompletion!.EnemyStandby!;
        Assert.Equal(new byte[] { 255 }, last.MoveString); Assert.Equal(new MapPosition(7, 5), last.Destination);
        Assert.Equal((8, 6, 12), ((int)Assert.Single(last.Rolls).Range, (int)last.Rolls[0].Result, last.Rolls[0].GeneratorSteps));
        Assert.Equal((ushort)0x0634, last.SeedCopyAfter);
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(current, 1)).Snapshot;
        var sarahReady = current;
        Assert.Equal((9, (byte)10, 1, 10), (current.Battle.FirstRound!.RoundNumber, current.Battle.FirstRound.CurrentTurnOffset,
            current.Battle.FirstControl!.ActorIndex, current.Battle.FirstControl.Movement.Range.Budget));
        Assert.Same(order.Slots, current.Battle.FirstRound.Slots);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(current, 1, new(10, 17))).Snapshot;
        Assert.Equal(2, current.Battle.FirstControl!.Movement.GridCost);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 1)).Snapshot;
        Assert.Equal(new MapPosition(10, 17), current.Battle.Roster[1].Position);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.CancelPrivateOriginalBattle01PlayerMovement(current, 1)).Snapshot;
        var end = current.Battle; Assert.Same(sarahReady.Battle.TurnCompletion, end.TurnCompletion);
        Assert.Same(sarahReady.Battle.FirstRound, end.FirstRound); Assert.Equal(sarahReady.Battle.Occupancy, end.Occupancy);
        Assert.Equal((0x25991234u, (ushort?)0x0634), (end.RandomSeedImage, end.RandomSeedCopy));
        Assert.Equal(new MapPosition?[] { new(11, 15), new(9, 17), new(11, 14), new(8, 3), new(9, 5),
            new(6, 3), new(10, 15), null, new(7, 5) }, end.Roster.Select(u => u.Position));
        Assert.Equal(new ushort[] { 3, 11, 9, 5, 5, 5, 3, 0, 5 }, end.Roster.Select(u => u.Stats.HpCurrent));
        Assert.Equal(new byte[] { 52, 36, 4, 36, 52, 52 }, end.AiMemory.Take(6));
        Assert.Equal(new byte[] { 255, 255, 255, 0, 0, 255 }, end.AiLastTargets.Take(6));
        Assert.Equal(original.RegionFlags90Through105, end.RegionFlags90Through105); Assert.Equal(0, end.NewlyTestedRegionMask);
        Assert.Equal(original.Roster.Select(u => u.AiBitfield), end.Roster.Select(u => u.AiBitfield));
        Assert.Equal(((uint?)60, (byte?)39, (ushort?)1, (byte?)10), (end.CurrentGold,
            end.Roster[0].Stats.CurrentExp, end.Roster[0].Stats.CurrentKills, end.Roster[2].Stats.CurrentExp));
        Assert.Null(end.Roster[2].Stats.CurrentKills); Assert.Equal(9, end.Roster.Count); Assert.Equal(8, end.Occupancy.Count(id => id >= 0));
        var receipts = new List<Battle01TurnCompletionReceipt>();
        for (var r = end.TurnCompletion; r is not null; r = r.Previous) receipts.Add(r);
        Assert.Equal(76, receipts.Count);
        Assert.Equal(new[] { 2, 128, 129, 131, 133 }, receipts.Take(5).Reverse().Select(r => r.CompletedActorIndex));
        foreach (var receipt in receipts.Take(5))
        {
            Assert.Equal(new Battle01FactionCounts(3, 5), receipt.BeforeAfterTurn);
            Assert.Equal(receipt.BeforeAfterTurn, receipt.AfterAfterTurn); Assert.Null(receipt.EnemyDefeat);
        }
        Assert.Same(ready.Preparation, current.Preparation); Assert.Equal((byte?)0, current.Preparation.Party.Allies[2].CurrentExp);
        Assert.Same(ready.SourceSnapshot, current.SourceSnapshot); Assert.Same(ready.SourceLocomotion, current.SourceLocomotion);
        Assert.Same(ready.SourceBridge, current.SourceBridge);
        return session;
    }

    [Sf2.Remake.TestSupport.PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE",
        "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_CANONICAL_MAP_IMPORT")]
    public void AcceptedSelectedInputsContinueSecondEnemyDefeatThroughRoundTenChesterControl()
        => ReachRealRoundTenChester(OriginalBattle01ControlledPartyPreset.ChesterPlayerAttackComparison);

    private static GameSession ReachRealRoundTenChester(OriginalBattle01ControlledPartyPreset party)
    {
        var session = ReachRealRoundNineSarahWithParty(party); var start = session.PrivateOriginalBattle01!;
        var current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(start, 1)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(current, 1)).Snapshot;
        Assert.Equal((byte)12, current.Battle.FirstRound!.CurrentTurnOffset); Assert.Equal(130, current.Battle.FirstRound.CurrentCandidate!.Value.CombatantIndex);
        Assert.Same(start.Battle.TurnCompletion, current.Battle.TurnCompletion!.Previous);
        Assert.Same(start.Battle.AiMemory, current.Battle.AiMemory); Assert.Same(start.Battle.AiLastTargets, current.Battle.AiLastTargets);
        current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(session.CompletePrivateOriginalBattle01EnemyStandby(current, 130)).Snapshot;
        var standby = current.Battle.TurnCompletion!.EnemyStandby!;
        Assert.Equal(new MapPosition(6, 3), standby.Origin); Assert.Equal(new MapPosition(5, 4), standby.Destination);
        Assert.Equal(new byte[] { 3, 2, 255 }, standby.MoveString);
        Assert.Equal(new byte[] { 5, 0 }, standby.Rolls.Select(r => r.Result));
        Assert.Equal(new byte[] { 8, 2 }, standby.Rolls.Select(r => r.Range));
        Assert.Equal(new[] { 11, 43 }, standby.Rolls.Select(r => r.GeneratorSteps));
        Assert.Equal(new ushort[] { 0x0534, 0x0034 }, standby.Rolls.Select(r => r.AfterSeedCopy));
        Assert.Equal((byte)20, standby.MemoryAfter); Assert.Equal(0x25991234u, current.Battle.RandomSeedImage);
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current, 0)).Snapshot;
        var bowieReady = current; var oldOrder = current.Battle.FirstRound!;
        Assert.Equal((9, (byte)14, 0, 12), (oldOrder.RoundNumber, oldOrder.CurrentTurnOffset,
            current.Battle.FirstControl!.ActorIndex, current.Battle.FirstControl.Movement.Range.Budget));
        Assert.Equal(new MapPosition(11, 15), current.Battle.Roster[0].Position);
        Assert.Equal(0, current.Battle.TerrainAt(new(10, 15))); Assert.Equal(1, current.Battle.TerrainAt(new(11, 15)));
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(current, 0)).Snapshot;
        Assert.Equal(new[] { 131 }, current.Battle.FirstControl!.Movement.Attack!.Targets);
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.CyclePrivateOriginalBattle01PlayerAttackTarget(current, 0, -1)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.CyclePrivateOriginalBattle01PlayerAttackTarget(current, 0, 1)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.CancelPrivateOriginalBattle01PlayerAttackTarget(current, 0)).Snapshot;
        Assert.Equal(Battle01Phase.PlayerActionChoice, current.Battle.Phase);
        Assert.Same(bowieReady.Battle.TurnCompletion, current.Battle.TurnCompletion);
        Assert.Equal((0x25991234u, (ushort?)0x0034), (current.Battle.RandomSeedImage, current.Battle.RandomSeedCopy));
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(current, 0)).Snapshot;
        var selected = current; var json = new JsonSerializerOptions { MaxDepth = 256 };
        string priorHistory = JsonSerializer.Serialize(selected.Battle.TurnCompletion, json);
        var prefix = new List<Battle01TurnCompletionReceipt>();
        for (var r = selected.Battle.TurnCompletion; r is not null; r = r.Previous) prefix.Add(r);
        Assert.Equal(78, prefix.Count);
        Assert.All(prefix, r => Assert.NotSame(Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat, r.Policy));
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.ConfirmPrivateOriginalBattle01PlayerAttack(selected, 0)).Snapshot;
        var receipt = current.Battle.TurnCompletion!; var d = receipt.PlayerPhysicalAttack!;
        Assert.Same(selected.Battle.TurnCompletion, receipt.Previous); Assert.Equal(priorHistory, JsonSerializer.Serialize(receipt.Previous, json));
        Assert.Same(Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat, receipt.Policy);
        Assert.Equal("battle01-class0-wooden-sword-effective-prowess3-v1", d.CombatProfile);
        Assert.Equal((0, 131, 0, 256), (d.ActorIndex, d.TargetIndex, (int)d.TargetTerrain, d.LandMultiplier));
        Assert.Equal(new byte[] { 255 }, d.MoveString); Assert.Equal(new MapPosition(11, 15), d.MovementOrigin);
        Assert.Equal(new MapPosition(10, 15), d.Target.Position);
        Assert.Equal(new ushort[] { 8, 16, 1, 1, 16, 16 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 7, 13, 0, 0, 9, 5 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0xE8CC1234, 0xD2631234, 0xAF0E1234, 0xE3BD1234, 0x90A01234, 0x58271234 },
            d.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((4, -1, 0, 3, 0, 49, 24, 24), (d.Effect.Damage, d.Target.Stats.HpCurrent - d.Effect.Damage,
            (int)d.Effect.TemporaryHp, (int)d.Effect.RestoredHp, (int)d.Effect.AfterStats.HpCurrent, d.AccumulatedExp, d.HalvedExp, d.AwardedExp));
        Assert.Equal(new Battle01PhysicalReaction(131, -4, 0, 0, 1), d.Effect.Reaction);
        Assert.False(d.Effect.Dodged); Assert.False(d.Effect.Critical); Assert.True(d.DefeatedTarget);
        Assert.Equal(((uint?)60, (uint?)120), (d.GoldBefore, d.GoldAfter));
        Assert.Equal(new[] { 131 }, receipt.EnemyDefeat!.FirstWorklist); Assert.Empty(receipt.EnemyDefeat.AfterTurnWorklist);
        Assert.Equal((0, (ushort)1, (ushort)2), (receipt.EnemyDefeat.CreditedAlly, receipt.EnemyDefeat.KillsBefore, receipt.EnemyDefeat.KillsAfter));
        Assert.Equal(new Battle01FactionCounts(3, 4), receipt.BeforeAfterTurn); Assert.Equal(receipt.BeforeAfterTurn, receipt.AfterAfterTurn);
        Assert.Same(selected.Battle.Roster[7], current.Battle.Roster[7]); Assert.Null(current.Battle.Roster[6].Position);
        Assert.Equal(-1, current.Battle.OccupantAt(new(10, 15))); Assert.Same(oldOrder.Slots, current.Battle.FirstRound!.Slots);
        Assert.Equal((byte)16, current.Battle.FirstRound.CurrentTurnOffset); Assert.Null(current.Battle.FirstRound.CurrentCandidate);
        Assert.Equal((0x58271234u, (ushort?)0x0034), (current.Battle.RandomSeedImage, current.Battle.RandomSeedCopy));
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01PlayerAttackRejected>(session.ConfirmPrivateOriginalBattle01PlayerAttack(selected, 0)).Diagnostic.Field);
        var afterAttack = current;
        current = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot;
        var generated = current;
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current, 2)).Snapshot;
        var ready = current;
        Assert.Equal((10, (byte)0, 2, 14), (current.Battle.FirstRound!.RoundNumber, current.Battle.FirstRound.CurrentTurnOffset,
            current.Battle.FirstControl!.ActorIndex, current.Battle.FirstControl.Movement.Range.Budget));
        Assert.Equal(new[] { new Battle01TurnEntry(2, 7), new(1, 6), new(128, 5), new(129, 5), new(130, 5), new(133, 5), new(0, 4) }
            .Concat(Enumerable.Repeat(new Battle01TurnEntry(255, 255), 57)), current.Battle.FirstRound.Slots);
        Assert.False(current.Battle.FirstControl.Movement.Range.CanStopAt(new(10, 15)));
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(current, 2, new(12, 14))).Snapshot;
        Assert.Equal(2, current.Battle.FirstControl!.Movement.GridCost);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 2)).Snapshot;
        Assert.Equal(new MapPosition(12, 14), current.Battle.Roster[2].Position);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.CancelPrivateOriginalBattle01PlayerMovement(current, 2)).Snapshot;
        var end = current.Battle;
        Assert.Equal(new MapPosition?[] { new(11, 15), new(9, 17), new(11, 14), new(8, 3), new(9, 5), new(5, 4), null, null, new(7, 5) }, end.Roster.Select(u => u.Position));
        Assert.Equal(new ushort[] { 3, 11, 9, 5, 5, 5, 0, 0, 5 }, end.Roster.Select(u => u.Stats.HpCurrent));
        Assert.Equal(((uint?)120, (byte?)63, (ushort?)2, (byte?)10), (end.CurrentGold, end.Roster[0].Stats.CurrentExp,
            end.Roster[0].Stats.CurrentKills, end.Roster[2].Stats.CurrentExp));
        Assert.Null(end.Roster[2].Stats.CurrentKills); Assert.Equal(9, end.Roster.Count); Assert.Equal(7, end.Occupancy.Count(id => id >= 0));
        Assert.Equal((0x9F861234u, (ushort?)0x0034, (ushort)7), (end.RandomSeedImage, end.RandomSeedCopy, end.NewlyTestedRegionMask));
        Assert.Equal(new byte[] { 52, 36, 20, 36, 52, 52 }.Concat(Enumerable.Repeat((byte)0, 42)), end.AiMemory);
        Assert.Equal(new byte[] { 255, 255, 255, 0, 0, 255 }.Concat(Enumerable.Repeat((byte)255, 42)), end.AiLastTargets);
        Assert.Equal(new ushort?[] { 0x2060, 0x2060, 0x2060, 0x2061, 0x2071, 0x2070 }, end.Roster.Skip(3).Select(u => u.AiBitfield));
        Assert.Equal(start.Battle.RegionFlags90Through105, end.RegionFlags90Through105);
        Assert.Same(afterAttack.Battle.TurnCompletion, end.TurnCompletion); Assert.Same(generated.Battle.FirstRound, end.FirstRound);
        Assert.Equal(ready.Battle.Occupancy, end.Occupancy); Assert.Same(ready.Battle.Roster[2].Stats, end.Roster[2].Stats);
        Assert.Same(start.Preparation, current.Preparation); Assert.Same(start.SourceSnapshot, current.SourceSnapshot);
        Assert.Same(start.SourceLocomotion, current.SourceLocomotion); Assert.Same(start.SourceBridge, current.SourceBridge);
        return session;
    }

    [Sf2.Remake.TestSupport.PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE",
        "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_CANONICAL_MAP_IMPORT")]
    public void AcceptedSelectedInputsContinueFirstAllyDefeatThroughRoundFourteenSarahControl()
    {
        var session = ReachRealFirstAllyDefeatBoundary();
        var before = session.PrivateOriginalBattle01!; var b = before.Battle;
        var json = new JsonSerializerOptions { MaxDepth = 256 };
        string history = JsonSerializer.Serialize(b.TurnCompletion, json);
        Assert.Equal((13, (byte)10, 133), (b.FirstRound!.RoundNumber, b.FirstRound.CurrentTurnOffset,
            b.FirstRound.CurrentCandidate!.Value.CombatantIndex));
        Assert.Equal(105, Count(b)); Assert.Equal((ushort?)0, b.Roster[2].Stats.CurrentDefeats);
        Assert.Equal((0x66531234u, (ushort?)0x0134), (b.RandomSeedImage, b.RandomSeedCopy));
        Assert.Equal(new MapPosition?[] { new(11,15), new(9,17), new(9,9), new(9,6), new(9,8), new(8,5), null, null, new(8,9) },
            b.Roster.Select(unit => unit.Position));
        Assert.Equal(new ushort[] { 3,11,1,5,5,5,0,0,5 }, b.Roster.Select(unit => unit.Stats.HpCurrent));
        Assert.Equal(new[] { new Battle01TurnEntry(2,7), new(1,6), new(128,6), new(0,5), new(129,5), new(133,5), new(130,4) }
            .Concat(Enumerable.Repeat(new Battle01TurnEntry(255,255),57)), b.FirstRound.Slots);
        Assert.Equal(new byte[] { 4,52,20,36,52,4 }.Concat(Enumerable.Repeat((byte)0,42)), b.AiMemory);
        Assert.Equal(new byte[] { 255,2,255,0,0,2 }.Concat(Enumerable.Repeat((byte)255,42)), b.AiLastTargets);
        Assert.Equal(new ushort?[] { 0x2061,0x2061,0x2061,0x2061,0x2071,0x2071 }, b.Roster.Skip(3).Select(unit => unit.AiBitfield));
        Assert.Equal(new[] { true,true,true }.Concat(Enumerable.Repeat(false,13)), b.RegionFlags90Through105);
        Assert.Equal(0,b.NewlyTestedRegionMask);
        Assert.Equal("attack.lethal", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01EnemyPhysicalAttack.CompleteNext(b,133,Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike)).ParamName);
        Assert.Same(before,session.PrivateOriginalBattle01);
        var current = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before,133)).Snapshot;
        var receipt = current.Battle.TurnCompletion!; var d = receipt.EnemyPhysicalAttack!;
        var priority = Assert.Single(d.Priorities); var cleanup = Assert.IsType<Battle01AllyDefeatCleanup>(receipt.AllyDefeat);
        Assert.Same(b.TurnCompletion,receipt.Previous); Assert.Equal(history,JsonSerializer.Serialize(receipt.Previous,json));
        Assert.Equal(106,Count(current.Battle)); Assert.Equal(12,current.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Same(Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat,receipt.Policy);
        Assert.Equal((133,2,0), (d.ActorIndex,d.TargetIndex,d.GridCost));
        Assert.Equal(new MapPosition(8,9),d.Destination); Assert.Equal(new byte[] {255},d.MoveString);
        Assert.Equal((230,2,0,19,133,(byte)2), (priority.LandMultiplier,priority.PotentialDamage,priority.RemainingHp,
            priority.Priority,priority.Roll.GeneratorSteps,priority.Roll.Result));
        Assert.Equal(((ushort)0x0134,(ushort)0x0234),(d.SeedCopyBefore,d.SeedCopyAfter));
        Assert.Equal(new ushort[] {32,32,1,1},d.Effect.Rolls.Select(roll=>roll.Range));
        Assert.Equal(new ushort[] {6,17,0,0},d.Effect.Rolls.Select(roll=>roll.Result));
        Assert.Equal(new uint[] {0x323E1234,0x8D2D1234,0x2B501234,0x33171234},d.Effect.Rolls.Select(roll=>roll.AfterImage));
        Assert.Equal((2,0,1,0),(d.Effect.Damage,(int)d.Effect.TemporaryHp,(int)d.Effect.RestoredHp,
            (int)current.Battle.Roster[2].Stats.HpCurrent));
        Assert.Equal(new Battle01PhysicalReaction(2,-2,0,0,1),d.Effect.Reaction);
        Assert.False(d.Effect.Dodged); Assert.False(d.Effect.Critical);
        Assert.Equal(new[] {2},cleanup.FirstWorklist); Assert.Empty(cleanup.AfterTurnWorklist);
        Assert.Equal((2,(ushort)0,(ushort)1),(cleanup.DefeatedAlly,cleanup.DefeatsBefore,cleanup.DefeatsAfter));
        Assert.Equal(new Battle01FactionCounts(2,4),receipt.BeforeAfterTurn); Assert.Equal(receipt.BeforeAfterTurn,receipt.AfterAfterTurn);
        Assert.Null(receipt.EnemyDefeat); Assert.Null(current.Battle.Roster[2].Position);
        Assert.Equal(-1,current.Battle.OccupantAt(new(9,9)));
        Assert.Same(b.Roster[6],current.Battle.Roster[6]); Assert.Same(b.Roster[7],current.Battle.Roster[7]);
        Assert.Equal("snapshot",Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before,133)).Diagnostic.Field);
        Assert.Same(current,session.PrivateOriginalBattle01);
        current = Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPursuit(current,130)).Snapshot;
        var pursuit = current.Battle.TurnCompletion!.EnemyPursuit!;
        Assert.Equal(new[] {new Battle01PursuitTargetCost(0,26),new(1,26)},pursuit.TargetCosts);
        Assert.Equal(0,pursuit.TargetIndex); Assert.Equal(new MapPosition(9,6),pursuit.PreliminaryDestination);
        Assert.Equal(new byte[] {0,3,255},pursuit.PreliminaryMoveString);
        Assert.Equal(new MapPosition(9,5),pursuit.Destination); Assert.Equal(new byte[] {0,255},pursuit.MoveString);
        Assert.Equal(2,pursuit.GridCost); Assert.Equal((0x33171234u,(ushort?)0x0234),(current.Battle.RandomSeedImage,current.Battle.RandomSeedCopy));
        current = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot;
        var generated = current;
        Assert.Equal(new[] {new Battle01TurnEntry(1,6),new(129,6),new(128,5),new(130,5),new(0,4),new(133,4)}
            .Concat(Enumerable.Repeat(new Battle01TurnEntry(255,255),58)),current.Battle.FirstRound!.Slots);
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(current,1)).Snapshot;
        Assert.Equal((14,(byte)0,1,10),(current.Battle.FirstRound!.RoundNumber,current.Battle.FirstRound.CurrentTurnOffset,
            current.Battle.FirstControl!.ActorIndex,current.Battle.FirstControl.Movement.Range.Budget));
        var ready = current;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(current,1,new(10,17))).Snapshot;
        Assert.Equal(2,current.Battle.FirstControl!.Movement.GridCost);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current,1)).Snapshot;
        Assert.Equal(new MapPosition(10,17),current.Battle.Roster[1].Position);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.CancelPrivateOriginalBattle01PlayerMovement(current,1)).Snapshot;
        var end = current.Battle;
        Assert.Equal(new MapPosition(9,17),end.Roster[1].Position);
        Assert.Equal((0x02A11234u,(ushort?)0x0234,(ushort)7),(end.RandomSeedImage,end.RandomSeedCopy,end.NewlyTestedRegionMask));
        Assert.Equal(107,Count(end)); Assert.Equal(9,end.Roster.Count); Assert.Equal(6,end.Occupancy.Count(actor=>actor>=0));
        Assert.Equal(((uint?)120,(byte?)63,(ushort?)2,(byte?)10,(ushort?)1),(end.CurrentGold,end.Roster[0].Stats.CurrentExp,
            end.Roster[0].Stats.CurrentKills,end.Roster[2].Stats.CurrentExp,end.Roster[2].Stats.CurrentDefeats));
        Assert.Null(end.Roster[2].Stats.CurrentKills); Assert.Equal(3,end.Roster[0].Stats.HpCurrent); Assert.Equal(11,end.Roster[1].Stats.HpCurrent);
        Assert.Equal(new[] {2,131,132},end.Roster.Where(unit=>unit.Position is null && unit.Stats.HpCurrent==0).Select(unit=>unit.Index));
        Assert.Equal(b.AiMemory,end.AiMemory); Assert.Equal(b.AiLastTargets,end.AiLastTargets);
        Assert.Equal(b.RegionFlags90Through105,end.RegionFlags90Through105);
        Assert.Same(generated.Battle.FirstRound,end.FirstRound); Assert.Same(generated.Battle.TurnCompletion,end.TurnCompletion);
        Assert.Equal(ready.Battle.Occupancy,end.Occupancy); Assert.Same(before.Preparation,current.Preparation);
        Assert.Same(before.SourceSnapshot,current.SourceSnapshot); Assert.Same(before.SourceLocomotion,current.SourceLocomotion);
        Assert.Same(before.SourceBridge,current.SourceBridge);
        static int Count(Battle01InitializedState state) {
            int count=0;for(var r=state.TurnCompletion;r is not null;r=r.Previous)count++;return count;
        }
    }

    [Sf2.Remake.TestSupport.PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE",
        "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_CANONICAL_MAP_IMPORT")]
    public void AcceptedSelectedInputsReachFirstLeaderDefeatPendingWithTheUnchanged119ReceiptPrefix()
    {
        var old = ReachRealLeaderDefeatBoundary(OriginalBattle01ControlledPartyPreset.ChesterDefeatComparison);
        var session = ReachRealLeaderDefeatBoundary(OriginalBattle01ControlledPartyPreset.LeaderDefeatComparison);
        var before = session.PrivateOriginalBattle01!; var b = before.Battle;
        var json = new JsonSerializerOptions { MaxDepth = 256 };
        // Only the explicitly supplied Bowie counter changes in all nested prior stat images.
        static string WithoutBowieCounter(Battle01InitializedState battle, JsonSerializerOptions options)
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(battle, options),
                documentOptions: new() { MaxDepth = 256 })!;
            void Visit(System.Text.Json.Nodes.JsonNode? value)
            {
                if (value is System.Text.Json.Nodes.JsonObject obj)
                {
                    if (obj["HpMax"]?.GetValue<int>() == 12 && obj["Items"] is System.Text.Json.Nodes.JsonArray items &&
                        items[0]?.GetValue<int>() == 199 && obj["CurrentDefeats"]?.GetValue<int>() == 0)
                        obj["CurrentDefeats"] = null;
                    foreach (var child in obj.ToArray()) Visit(child.Value);
                }
                else if (value is System.Text.Json.Nodes.JsonArray array) foreach (var child in array) Visit(child);
            }
            Visit(node); return node.ToJsonString(options);
        }
        Assert.Equal(WithoutBowieCounter(old.PrivateOriginalBattle01!.Battle, json), WithoutBowieCounter(b, json));
        Assert.Equal("attack.lethal", Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(
            old.CompletePrivateOriginalBattle01EnemyPhysicalAttack(old.PrivateOriginalBattle01, 129)).Diagnostic.Field);
        Assert.Equal((16, (byte)0, 129), (b.FirstRound!.RoundNumber, b.FirstRound.CurrentTurnOffset, b.FirstRound.CurrentCandidate!.Value.CombatantIndex));
        Assert.Equal((0x9B651234u, (ushort?)0x0234, (ushort)7), (b.RandomSeedImage, b.RandomSeedCopy, b.NewlyTestedRegionMask));
        Assert.Equal(new[] { new Battle01TurnEntry(129, 6), new(130, 6), new(0, 5), new(133, 5), new(1, 4), new(128, 4) }
            .Concat(Enumerable.Repeat(new Battle01TurnEntry(255, 255), 58)), b.FirstRound.Slots);
        int count = 0; for (var r = b.TurnCompletion; r is not null; r = r.Previous) count++;
        Assert.Equal(119, count); string frozen = JsonSerializer.Serialize(b, json);
        var after = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before, 129)).Snapshot;
        var end = after.Battle; var t = Assert.IsType<Battle01DefeatPendingReceipt>(end.DefeatPending); var d = t.Attack;
        Assert.Equal(Battle01Phase.DefeatPending, end.Phase); Assert.Same(b.FirstRound, end.FirstRound);
        Assert.Same(b.TurnCompletion, end.TurnCompletion); Assert.Same(b.TurnCompletion, t.Previous);
        Assert.Same(before.Preparation, after.Preparation); Assert.Same(before.SourceSnapshot, after.SourceSnapshot);
        Assert.Same(before.SourceBridge, after.SourceBridge); Assert.Same(before.SourceLocomotion, after.SourceLocomotion);
        Assert.Equal(new MapPosition?[] { null, new(9,17), null, new(11,8), new(11,14), new(11,7), null, null, new(10,11) }, end.Roster.Select(u => u.Position));
        Assert.Equal(new ushort[] { 0,11,0,5,5,5,0,0,5 }, end.Roster.Select(u => u.Stats.HpCurrent));
        Assert.Equal(new ushort?[] { 1,null,1,null,null,null,null,null,null }, end.Roster.Select(u => u.Stats.CurrentDefeats));
        Assert.Equal(new ushort[] { 28,18,0,0 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal((230,3,0,16,66), (d.Priorities[0].LandMultiplier,d.Priorities[0].PotentialDamage,
            d.Priorities[0].RemainingHp,d.Priorities[0].Priority,d.Priorities[0].Roll.GeneratorSteps));
        Assert.Equal((0x10491234u,(ushort?)0x0034,(ushort)0),(end.RandomSeedImage,end.RandomSeedCopy,end.NewlyTestedRegionMask));
        Assert.Equal(new Battle01FactionCounts(0,4),t.FirstOutcome); Assert.Equal(new[]{0},t.Cleanup.FirstWorklist);
        Assert.False(t.AfterTurnExecuted); Assert.False(t.TurnAdvanced);
        Assert.Equal(((uint?)120,(byte?)63,(ushort?)2,(byte?)10),(end.CurrentGold,end.Roster[0].Stats.CurrentExp,end.Roster[0].Stats.CurrentKills,end.Roster[2].Stats.CurrentExp));
        Assert.Equal(5,end.Occupancy.Count(i=>i>=0)); Assert.Equal(-1,end.OccupantAt(new(11,15)));
        Assert.Equal(frozen,JsonSerializer.Serialize(b,json));
        Assert.Equal("snapshot",Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before,129)).Diagnostic.Field);
        Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(session.EnterPrivateOriginalBattle01NextRound(after));
        Assert.Same(after,session.PrivateOriginalBattle01);
    }

    [Fact]
    public void AcceptedSelectedInputsApplyDefeatRecoveryOnceAfterTheReal119ReceiptTerminal()
    {
        var session = ReachRealLeaderDefeatBoundary(OriginalBattle01ControlledPartyPreset.LeaderDefeatComparison);
        var terminal = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(session.PrivateOriginalBattle01, 129)).Snapshot;
        var json = new JsonSerializerOptions { MaxDepth = 256 };
        string frozen = JsonSerializer.Serialize(terminal.Battle, json);
        var after = Assert.IsType<PrivateOriginalBattle01DefeatRecovered>(session.RecoverPrivateOriginalBattle01Defeat(terminal)).Snapshot;
        Assert.Same(terminal.Preparation, after.Preparation); Assert.Same(terminal.SourceSnapshot, after.SourceSnapshot);
        Assert.Same(terminal.Battle, after.Battle.DefeatRecovery!.Before);
        Assert.Same(terminal.Battle.DefeatPending, after.Battle.DefeatRecovery.Terminal);
        Assert.Same(terminal.Battle.TurnCompletion, after.Battle.TurnCompletion);
        var expected = JsonSerializer.SerializeToNode(terminal.Battle, json)!;
        expected["Roster"]![0]!["Stats"]!["HpCurrent"] = 12;
        expected["CurrentGold"] = 60; expected["Phase"] = (int)Battle01Phase.DefeatRecoveryPending;
        var actual = JsonSerializer.SerializeToNode(after.Battle, json)!; actual["DefeatRecovery"] = null;
        Assert.True(JsonNode.DeepEquals(expected, actual));
        Assert.Equal(frozen, JsonSerializer.Serialize(terminal.Battle, json));
        Assert.IsType<PrivateOriginalBattle01DefeatRecoveryRejected>(session.RecoverPrivateOriginalBattle01Defeat(after));
        Assert.IsType<PrivateOriginalBattle01DefeatRecoveryRejected>(session.RecoverPrivateOriginalBattle01Defeat(terminal));
        Assert.Same(after, session.PrivateOriginalBattle01);
    }

    private static GameSession ReachRealLeaderDefeatBoundary(OriginalBattle01ControlledPartyPreset preset)
    {
        var session = ReachRealFirstAllyDefeatBoundary(preset);
        for (int step = 0; step < 32; step++)
        {
            var current = session.PrivateOriginalBattle01!; var order = current.Battle.FirstRound!;
            if (order.RoundNumber == 16 && order.CurrentTurnOffset == 0) return session;
            if (order.CurrentCandidate is not { } candidate)
                Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current));
            else if (candidate.CombatantIndex < 128)
            {
                int actor = candidate.CombatantIndex;
                current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current, actor)).Snapshot;
                current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current, actor)).Snapshot;
                Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(current, actor));
            }
            else if (session.CompletePrivateOriginalBattle01EnemyPursuit(current, candidate.CombatantIndex) is PrivateOriginalBattle01AttackSelectionRequired)
                Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current, candidate.CombatantIndex));
        }
        throw new InvalidOperationException("The real-input four-STAY route must reach R16 enemy129.");
    }

    private static GameSession ReachRealFirstAllyDefeatBoundary(OriginalBattle01ControlledPartyPreset? preset = null)
    {
        var session=ReachRealRoundTenChester(preset ?? OriginalBattle01ControlledPartyPreset.ChesterDefeatComparison);
        for(int choice=0;choice<12;choice++)
        {
            var current=session.PrivateOriginalBattle01!;
            int actor=current.Battle.FirstControl!.ActorIndex;
            if(choice==0) current=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
                session.SelectPrivateOriginalBattle01PlayerDestination(current,actor,new(9,9))).Snapshot;
            current=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current,actor)).Snapshot;
            current=Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(current,actor)).Snapshot;
            for(int attempt=0;attempt<128;attempt++)
            {
                var order=current.Battle.FirstRound!;
                if(order.RoundNumber==13 && order.CurrentTurnOffset==10 && order.CurrentCandidate?.CombatantIndex==133)return session;
                if(order.CurrentCandidate is not {} candidate) {
                    current=Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot;
                    continue;
                }
                actor=candidate.CombatantIndex;
                if(actor<128) {
                    current=Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
                        session.EnterPrivateOriginalBattle01NextPlayerControl(current,actor)).Snapshot;
                    break;
                }
                if((current.Battle.Roster.Single(unit=>unit.Index==actor).AiBitfield & 1)==0)
                    current=Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(
                        session.CompletePrivateOriginalBattle01EnemyStandby(current,actor)).Snapshot;
                else {
                    var result=session.CompletePrivateOriginalBattle01EnemyPursuit(current,actor);
                    current=result is PrivateOriginalBattle01AttackSelectionRequired
                        ? Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
                            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current,actor)).Snapshot
                        : Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(result).Snapshot;
                }
            }
        }
        throw new InvalidOperationException("The explicit twelve-choice route must reach actual133 at receipt105.");
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

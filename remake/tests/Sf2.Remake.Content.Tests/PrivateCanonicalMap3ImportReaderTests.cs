using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Content.Tests;

public sealed class PrivateCanonicalMap3ImportReaderTests
{
    private const int SampleBlockCount = 0x90; // Authored zero blocks through the admitted doorway index0x8F.
    [Fact]
    public void ReturnEntryLoadRetainsCatalogResourcesAndSelectsChurchWithoutAWarp()
    {
        var definition = Assert.IsType<OriginalMapImportAccepted>(Admit(SampleDocument())).Definition;
        var load = Assert.IsType<OriginalMapReturnEntryLoadDefinition>(definition.ReturnEntryLoad);
        Assert.Same(definition.RuntimeCatalog.Resolve(new("map3")), load.Runtime);
        Assert.Same(load.Runtime.AreaCatalog.Records[0], load.Area);
        Assert.Equal(10, load.Roofs.Count); Assert.Equal(2, load.FlagCopies.Count); Assert.Single(load.Chests);
        Assert.Equal(new OriginalMapReturnRoofRow(8,32,15,255,255,5,6,30,41), load.SelectRoof(new(32,13)));
        Assert.Equal(new OriginalMapStepCopyIdentity(ContentProfile.PrivateLocal,new("map3"),"Map03s4_StepEvents",4),load.ChurchDoor.Identity);
        Assert.Equal(new MapPosition(32,15),load.ChurchDoor.Trigger);
        Assert.Equal(new WorkingMapBlockCopy(62,0,32,15,1,1),load.ChurchDoor.Copy);
        Assert.Equal(10,load.RoofActions.Records.Count);
        Assert.Equal(new MapCellCoordinate(32,15),load.RoofActions.Records[7].Trigger);
        Assert.False(OriginalMapRuntimeAdmission.HasExactAcceptedReturnEntryLoad(null, definition.RuntimeCatalog));
        var other = Assert.IsType<OriginalMapImportAccepted>(Admit(SampleDocument())).Definition;
        Assert.False(OriginalMapRuntimeAdmission.HasExactAcceptedReturnEntryLoad(load, other.RuntimeCatalog));
    }

    [Theory]
    [InlineData("roof8")] [InlineData("roof6")] [InlineData("roof-count")]
    [InlineData("flag")] [InlineData("chest")] [InlineData("missing-chest")]
    [InlineData("door-trigger")] [InlineData("door-source")] [InlineData("door-destination")]
    public void ReturnEntryResourceDriftRejectsTheCompleteImport(string mutation)
    {
        var doc = SampleDocument();
        switch (mutation)
        {
            case "roof8": RoofRecords(doc)[7]!["destination"]!["x"] = 31; break;
            case "roof6": RoofRecords(doc)[5]!["source"]!["x"] = 50; break;
            case "roof-count": RoofRecords(doc).RemoveAt(9); break;
            case "flag": ResourceArray(doc,"flagEventTables")[0]!["records"]![0]!["flag"] = 507; break;
            case "chest": ResourceArray(doc,"itemTables")[0]!["records"]![0]!["item"] = 1; break;
            case "missing-chest": ResourceArray(doc,"itemTables")[0]!.AsObject().Remove("records"); break;
            case "door-trigger": ResourceArray(doc,"stepEventTables")[0]!["records"]![3]!["trigger"]!["x"] = 31; break;
            case "door-source": ResourceArray(doc,"stepEventTables")[0]!["records"]![3]!["source"]!["x"] = 61; break;
            case "door-destination": ResourceArray(doc,"stepEventTables")[0]!["records"]![3]!["destination"]!["y"] = 16; break;
        }
        Assert.IsType<OriginalMapImportRejected>(Admit(doc));
    }

    [Theory]
    [InlineData(19, "palette")]
    [InlineData(20, "palette")]
    [InlineData(19, "tilesets")]
    [InlineData(20, "tilesets")]
    [InlineData(21, "palette")]
    [InlineData(21, "tilesets")]
    public void CastleVisualSelectionDriftFailsAdmission(int mapId, string field)
    {
        JsonObject document = SampleDocument();
        if (field == "palette") Map(document, mapId)[field] = 1;
        else Map(document, mapId)[field]!.AsArray()[4] = mapId == 21 ? 62 : 8;
        var rejected = Assert.IsType<OriginalMapImportRejected>(Admit(document));
        Assert.Equal(OriginalMapImportFailureCode.InvalidMapProjection, rejected.Diagnostic.Code);
    }

    private const string AcceptedCanonicalDigest =
        "DDDA4FA05455DDBA9CDAF85497CEE0C1C89C6E625721A8FEAD301044C892E508";
    private const string AcceptedDecodedLayoutDigest =
        "6BC4D0BF350242EA908A5ED00FFFDF68F6428E7A5189B23AE189CD24BC220446";
    private const string AcceptedCollisionDigest =
        "A9A7BACA8952DCC50CA90CD0985512C7F2393184FEA45F4E85E397422EAC9433";

    [Fact]
    public void SyntheticCanonicalSampleAdmitsExactPrivateMap3Projection()
    {
        JsonObject document = SampleDocument();
        byte[] bytes = DocumentBytes(document);
        OriginalMapImportResult result =
            PrivateCanonicalMap3ImportReader.AdmitSemanticDocumentForTests(bytes);
        Assert.True(
            result is OriginalMapImportAccepted,
            result is OriginalMapImportRejected rejected
                ? $"{rejected.Diagnostic.Code}:{rejected.Diagnostic.Field}:{rejected.Diagnostic.Message}"
                : "The semantic import returned an unknown result.");
        OriginalMapImportAccepted accepted = Assert.IsType<OriginalMapImportAccepted>(result);

        Assert.Equal(ContentProfile.PrivateLocal, accepted.Receipt.Profile);
        Assert.Equal(Digest(bytes), accepted.Receipt.ContentDigest);
        Assert.Equal(PrivateCanonicalMap3ImportReader.PackageId, accepted.Receipt.PackageId);
        Assert.Equal(PrivateCanonicalMap3ImportReader.CanonicalRomSha256,
            accepted.Receipt.Provenance.RomSha256);
        Assert.Equal(PrivateCanonicalMap3ImportReader.CanonicalCommit,
            accepted.Receipt.Provenance.UpstreamCommit);
        Assert.Equal(
            new[]
            {
                PrivateCanonicalMap3ImportReader.Capability,
                PrivateCanonicalMap3ImportReader.TraversalCapability,
                PrivateCanonicalMap3ImportReader.ControlledAdmissionCapability,
                PrivateCanonicalMap3ImportReader.ControlledStepCopyCapability,
                PrivateCanonicalMap3ImportReader.CurrentAreaDiagnosticCapability,
                PrivateCanonicalMap3ImportReader.AreaSourceRecordAdmissionCapability,
                PrivateCanonicalMap3ImportReader.SelectedSetupEntityPopulationCapability,
                PrivateCanonicalMap3ImportReader.BlocksetSourceAdmissionCapability,
                PrivateCanonicalMap3ImportReader.VisualReferenceAdmissionCapability,
                PrivateCanonicalMap3ImportReader.SameMapWarpAdmissionCapability,
                PrivateCanonicalMap3ImportReader.RoofOnLoadClearCapability,
                PrivateCanonicalMap3ImportReader.BowieDoorStepCopyCapability,
                PrivateCanonicalMap3ImportReader.SchoolDoorStepCopyCapability,
                PrivateCanonicalMap3ImportReader.Zone601InterceptionCapability,
                PrivateCanonicalMap3ImportReader.SarahRouteCapability,
                PrivateCanonicalMap3ImportReader.Entity142AcknowledgementCapability,
                PrivateCanonicalMap3ImportReader.AstralZoneHandoffCapability,
                PrivateCanonicalMap3ImportReader.MessengerAcceptanceCapability,
                PrivateCanonicalMap3ImportReader.CastleGateOpeningCapability,
                PrivateCanonicalMap3ImportReader.NorthMap19TransitionCapability,
                PrivateCanonicalMap3ImportReader.RoyalMap20TransitionCapability,
                OriginalMapRuntimeAdmission.PalaceFirstVisitCapability,
                OriginalMapRuntimeAdmission.RoyalReturnMap19TransitionCapability,
                OriginalMapRuntimeAdmission.WestTowerMap20TransitionCapability,
                OriginalMapRuntimeAdmission.MiddleTowerMap21TransitionCapability,
                OriginalMapRuntimeAdmission.MiddleTowerGuardCapability,
                OriginalMapRuntimeAdmission.NorthMap40TransitionCapability,
                OriginalBattle01AdmissionDefinition.Capability,
            },
            accepted.Receipt.Capabilities);
        Assert.Equal(new MapId("map3"), accepted.Definition.Map);
        Assert.Equal((byte)0, accepted.Definition.VisualResourceSelection.PaletteIndex);
        Assert.Equal(
            new byte[] { 0, 37, 43, 53, 66 },
            accepted.Definition.VisualResourceSelection.TilesetSlots);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedVisualReferenceProjectionDigest,
            accepted.Definition.VisualResourceSelection.ProjectionDigest);
        Assert.Equal(WorkingMapLayout.WordCount, accepted.Definition.WorkingLayout.Words.Count);
        Assert.Equal("Map03s0_Blocks", accepted.Definition.BlockCatalog.ResourceId);
        Assert.Equal(SampleBlockCount, accepted.Definition.BlockCatalog.Records.Count);
        Assert.Equal(
            0,
            accepted.Definition.BlockCatalog.Resolve(
                accepted.Definition.WorkingLayout,
                accepted.Definition.ControlledAdmission.Position)
                .Identity.ZeroBasedBlockIndex);
        Assert.Equal(new MapPosition(56, 3), accepted.Definition.ControlledAdmission.Position);
        Assert.Equal(3, accepted.Definition.Traversal.ActiveAreas.Count);
        Assert.Equal("Map03s2_Areas", accepted.Definition.AreaCatalog.ResourceId);
        Assert.Equal(3, accepted.Definition.AreaCatalog.Records.Count);
        Assert.Equal(
            new OriginalMapAreaWordPair(256, 256),
            accepted.Definition.AreaCatalog.Records[1].MainLayerParallax);
        Assert.Equal((byte)8, accepted.Definition.AreaCatalog.Records[1].DefaultMusic);
        Assert.Equal(
            2,
            accepted.Definition.Traversal.SelectActiveArea(
                accepted.Definition.ControlledAdmission.Position)!.OneBasedRecordOrdinal);
        Assert.Equal("ms_map3", accepted.Definition.ControlledAdmission.SelectedSetup.Value);
        Assert.Equal("ms_map3_InitFunction",
            accepted.Definition.ControlledAdmission.SelectedInitIdentity);
        Assert.True(accepted.Definition.ControlledAdmission.NoProgramRequest);
        Assert.Equal(new MapId("map3"), accepted.Definition.EntityPopulation.Map);
        Assert.Equal(new MapSetupId("ms_map3"), accepted.Definition.EntityPopulation.SelectedSetup);
        Assert.Equal("ms_map3_Entities", accepted.Definition.EntityPopulation.ResourceId);
        Assert.Equal(19, accepted.Definition.EntityPopulation.Records.Count);
        Assert.Equal(
            OriginalMapEntityRecordKind.Fixed,
            accepted.Definition.EntityPopulation.Records[0].Kind);
        Assert.Equal(
            OriginalMapEntityRecordKind.Walking,
            accepted.Definition.EntityPopulation.Records[1].Kind);
        Assert.Equal(
            accepted.Definition.EntityPopulation.Records[0].Position,
            accepted.Definition.EntityPopulation.Records[1].Position);
        Assert.Equal(
            new byte[] { 0, 4, 0x60, 0xCE },
            accepted.Definition.EntityPopulation.Records[0].OpaqueTail);
        Assert.Equal(
            new byte[] { 0xFF, 42, 8, 3 },
            accepted.Definition.EntityPopulation.Records[1].OpaqueTail);
        OriginalMapZone601Definition zone601 =
            Assert.IsType<OriginalMapZone601Definition>(accepted.Definition.Zone601);
        Assert.Equal(new MapPosition(4, 4), zone601.Trigger);
        Assert.Equal(7, zone601.Identity.OneBasedRecordOrdinal);
        Assert.Equal("Map3_ZoneEvent6", zone601.Identity.TargetIdentity);
        Assert.Equal(601, zone601.GateFlag);
        Assert.Equal("cs_5145C", zone601.BlockingSequenceIdentity);
        Assert.Equal(new MapPosition(5, 6), zone601.ActorInitialPosition);
        Assert.Equal(new MapPosition(5, 4), zone601.ActorBlockingEndPosition);
        Assert.Equal(new[] { 510, 511, 483 }, zone601.TextIds);
        Assert.Equal("eas_Walking", zone601.AmbientBehaviorIdentity);
        OriginalMapSarahDefinition sarah =
            Assert.IsType<OriginalMapSarahDefinition>(accepted.Definition.Sarah);
        Assert.Equal(new MapPosition(42, 8), sarah.ActorInitialPosition);
        Assert.Equal(new MapPosition(42, 9), sarah.PlayerInteractionPosition);
        Assert.Equal(new MapPosition(41, 7), sarah.FirstInteractionWaypoint);
        Assert.Equal(new[] { 512, 480, 481 }, sarah.FirstInteractionTextIds);
        Assert.Equal(new[] { 480, 481 }, sarah.RepeatInteractionTextIds);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedSarah(
            sarah,
            accepted.Definition.EntityPopulation,
            accepted.Definition.Traversal,
            accepted.Definition.WorkingLayout));
        OriginalMapEntity142Definition entity142 =
            Assert.IsType<OriginalMapEntity142Definition>(accepted.Definition.Entity142);
        Assert.Equal(new MapPosition(54, 17), entity142.ActorPosition);
        Assert.Equal(new MapPosition(55, 17), entity142.PlayerInteractionPosition);
        Assert.Equal(17, entity142.PhysicalActorSlot);
        Assert.Equal(new[] { 500, 501 }, entity142.FirstInteractionTextIds);
        Assert.Equal(new[] { 501 }, entity142.RepeatInteractionTextIds);
        OriginalMapAstralZoneDefinition astralZone =
            Assert.IsType<OriginalMapAstralZoneDefinition>(accepted.Definition.AstralZone);
        Assert.Equal(new MapPosition(58, 13), astralZone.Trigger);
        Assert.Equal(8, astralZone.Identity.OneBasedRecordOrdinal);
        Assert.Equal("Map3_ZoneEvent7", astralZone.Identity.TargetIdentity);
        Assert.Equal("cs_5148C", astralZone.PositionProgramIdentity);
        Assert.Equal(new[] { 514, 515, 516 }, astralZone.TextIds);
        Assert.Equal(new MapPosition(41, 10), astralZone.SarahDestination);
        Assert.Equal(new MapPosition(6, 4), astralZone.Zone601ActorDestination);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedAstralZone(
            astralZone,
            sarah,
            zone601,
            accepted.Definition.Traversal,
            accepted.Definition.WorkingLayout));
        OriginalMapMessengerAcceptanceDefinition messenger =
            Assert.IsType<OriginalMapMessengerAcceptanceDefinition>(
                accepted.Definition.MessengerAcceptance);
        Assert.Equal(new MapPosition(42, 10), messenger.Approach);
        Assert.Equal(ExplorationDirection.East, messenger.EntryDirection);
        Assert.Equal(new MapPosition(43, 10), messenger.Trigger);
        Assert.Equal("cs_5149A", messenger.MessengerProgramIdentity);
        Assert.Equal("cs_51614", messenger.AcceptedBranchProgramIdentity);
        Assert.Equal(OriginalMapRuntimeAdmission.MessengerTextIds, messenger.TextIds);
        Assert.Equal(OriginalMapRuntimeAdmission.MessengerSpeakerOperands,
            messenger.SpeakerOperands);
        Assert.Equal(OriginalMapRuntimeAdmission.MessengerStages, messenger.Stages);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedMessengerAcceptance(
            messenger,
            accepted.Definition.EntityPopulation,
            sarah,
            entity142,
            astralZone,
            accepted.Definition.Traversal,
            accepted.Definition.WorkingLayout));
        OriginalMapCastleGateDefinition castleGate =
            Assert.IsType<OriginalMapCastleGateDefinition>(accepted.Definition.CastleGate);
        Assert.Equal(new MapPosition(31, 6), castleGate.Approach);
        Assert.Equal(ExplorationDirection.North, castleGate.EntryDirection);
        Assert.Equal(new MapPosition(31, 5), castleGate.Trigger);
        Assert.Equal("cs_51652", castleGate.ProgramIdentity);
        Assert.Equal(537, castleGate.TextCursorId);
        Assert.Equal(604, castleGate.CompletionFlag);
        Assert.Equal(26, castleGate.SourceOperationCount);
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 25 },
            castleGate.ProjectionSourceOperationIndices);
        Assert.Equal(OriginalMapRuntimeAdmission.CastleGateGuardMoves, castleGate.GuardMoves);
        Assert.Equal(OriginalMapRuntimeAdmission.CastleGateStages, castleGate.Stages);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedCastleGate(
            castleGate,
            messenger,
            accepted.Definition.Traversal,
            accepted.Definition.WorkingLayout));
        Assert.Contains("natural-flags-setup-variant-selection",
            accepted.Definition.UnsupportedCapabilities);
        Assert.Equal(
            OriginalMapTraversal.CollisionMask,
            accepted.Definition.WorkingLayout[41, 13] & OriginalMapTraversal.CollisionMask);
        Assert.False(OriginalMapTraversal.IsBlocked(
            accepted.Definition.WorkingLayout,
            new MapPosition(62, 0)));
        OriginalMapStepCopyDefinition stepCopy =
            Assert.IsType<OriginalMapStepCopyDefinition>(
                accepted.Definition.ControlledStepCopy);
        Assert.Equal(ContentProfile.PrivateLocal, stepCopy.Identity.Profile);
        Assert.Equal(new MapId("map3"), stepCopy.Identity.Map);
        Assert.Equal("Map03s4_StepEvents", stepCopy.Identity.SourceResourceId);
        Assert.Equal(6, stepCopy.Identity.OneBasedRecordOrdinal);
        Assert.Equal(new MapPosition(41, 13), stepCopy.Trigger);
        Assert.Equal((62, 0, 41, 13, 1, 1), Geometry(stepCopy.Copy));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedSchoolDoorStepCopy(stepCopy));
        OriginalMapStepCopyDefinition bowieDoor =
            Assert.IsType<OriginalMapStepCopyDefinition>(
                accepted.Definition.BowieDoorStepCopy);
        Assert.Equal(1, bowieDoor.Identity.OneBasedRecordOrdinal);
        Assert.Equal(new MapPosition(4, 8), bowieDoor.Trigger);
        Assert.Equal((62, 0, 4, 8, 1, 1), Geometry(bowieDoor.Copy));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedBowieDoorStepCopy(bowieDoor));
        OriginalMapSameMapWarpCatalog warps = Assert.IsType<OriginalMapSameMapWarpCatalog>(
            accepted.Definition.SameMapWarps);
        Assert.Equal("Map03s6_WarpEvents", warps.ResourceId);
        Assert.Equal(new[] { 6, 9 },
            warps.Records.Select(record => record.Identity.OneBasedRecordOrdinal));
        Assert.Equal(new MapPosition(59, 12), warps.Records[0].Destination);
        Assert.Equal(new MapPosition(3, 3), warps.Records[1].Destination);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedSameMapWarps(warps));
        OriginalMapRoofOnLoadDefinition roof =
            Assert.IsType<OriginalMapRoofOnLoadDefinition>(
                accepted.Definition.RoofOnLoadClear);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedRoofOnLoadClear(roof));
        Assert.Equal(new MapPosition(4, 8), roof.SourceTrigger);
        Assert.Equal(new MapPosition(2, 32), roof.ClearDestination);
        Assert.Equal((7, 8), (roof.Width, roof.Height));
        Assert.Equal(5, accepted.Definition.RuntimeCatalog.Records.Count);
        Assert.Same(
            accepted.Definition.InitialRuntime,
            accepted.Definition.RuntimeCatalog.Resolve(new MapId("map3")));
        OriginalMapExplorationRuntimeDefinition map19Runtime =
            accepted.Definition.RuntimeCatalog.Resolve(new MapId("map19"));
        Assert.Equal("Map19s0_Blocks", map19Runtime.BlockCatalog.ResourceId);
        Assert.Equal("Map19s2_Areas", map19Runtime.AreaCatalog.ResourceId);
        Assert.Equal("ms_map19_Entities", map19Runtime.EntityPopulation.ResourceId);
        Assert.Equal(13, map19Runtime.EntityPopulation.Records.Count);
        Assert.Equal(new MapSetupId("ms_map19"), map19Runtime.SelectedSetup);
        Assert.Equal("ms_map19_InitFunction", map19Runtime.SelectedInitIdentity);
        OriginalMapCrossMapTransitionDefinition north =
            Assert.IsType<OriginalMapCrossMapTransitionDefinition>(
                accepted.Definition.NorthMap19Transition);
        Assert.Equal(1, north.Identity.OneBasedRecordOrdinal);
        Assert.Equal((byte)255, north.SourceTriggerX);
        Assert.Equal((byte)1, north.SourceTriggerY);
        Assert.Equal(new MapPosition(28, 2), north.AdmittedApproach);
        Assert.Equal(ExplorationDirection.North, north.AdmittedDirection);
        Assert.Equal(new MapPosition(28, 1), north.AdmittedTrigger);
        Assert.Equal(new MapId("map19"), north.DestinationMap);
        Assert.Equal(new MapPosition(26, 30), north.Destination);
        OriginalMapExplorationRuntimeDefinition map20Runtime =
            accepted.Definition.RuntimeCatalog.Resolve(new MapId("map20"));
        Assert.Equal("Map20s0_Blocks", map20Runtime.BlockCatalog.ResourceId);
        Assert.Equal("ms_map20_Entities", map20Runtime.EntityPopulation.ResourceId);
        Assert.Equal(8, map20Runtime.EntityPopulation.Records.Count);
        Assert.Equal(new MapSetupId("ms_map20"), map20Runtime.SelectedSetup);
        Assert.Equal("ms_map20_InitFunction", map20Runtime.SelectedInitIdentity);
        OriginalMapCrossMapTransitionDefinition royal = Assert.IsType<OriginalMapCrossMapTransitionDefinition>(
            accepted.Definition.RoyalMap20Transition);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedRoyalMap20Transition(royal));
        Assert.Equal(2, royal.Identity.OneBasedRecordOrdinal);
        Assert.Equal(new MapPosition(22, 4), royal.AdmittedApproach);
        Assert.Equal(new MapPosition(23, 3), royal.AdmittedTrigger);
        Assert.Equal(new MapPosition(23, 37), royal.Destination);
    }

    [Fact]
    public void WrongPackageProfileAndRawDigestFailBeforeSemanticAdmission()
    {
        byte[] bytes = DocumentBytes(SampleDocument());

        AssertCode(
            AdmitProduction(bytes, new OriginalMapImportRequest(
                "other-package",
                ContentProfile.PrivateLocal,
                Digest(bytes))),
            OriginalMapImportFailureCode.PackageIdentityMismatch);
        AssertCode(
            AdmitProduction(bytes, new OriginalMapImportRequest(
                PrivateCanonicalMap3ImportReader.PackageId,
                ContentProfile.PublicSynthetic,
                Digest(bytes))),
            OriginalMapImportFailureCode.ProfileMismatch);
        AssertCode(
            AdmitProduction(bytes, new OriginalMapImportRequest(
                PrivateCanonicalMap3ImportReader.PackageId,
                ContentProfile.PrivateLocal,
                new string('0', 64))),
            OriginalMapImportFailureCode.ContentDigestMismatch);
    }

    [Fact]
    public void RecomputedCallerDigestCannotAuthorizeStructurallyValidMutation()
    {
        byte[] bytes = DocumentBytes(SampleDocument());
        byte[] whitespaceMutation = [.. bytes, (byte)' '];
        AssertCode(
            AdmitProduction(whitespaceMutation, Request(Digest(whitespaceMutation))),
            OriginalMapImportFailureCode.ContentDigestMismatch);
    }

    [Fact]
    public void Zone601RecordActorAndBlockingProgramDriftFailSemanticAdmission()
    {
        JsonObject recordDrift = SampleDocument();
        ZoneRecords(recordDrift)[6]!.AsObject()["x"] = 5;
        AssertCode(Admit(recordDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject actorDrift = SampleDocument();
        EntityRecords(actorDrift)[2]!.AsObject()["actionValue"] = 1U;
        AssertCode(Admit(actorDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject programDrift = SampleDocument();
        ZoneProgramOperations(programDrift)[2]!.AsObject()["operandText"] = "3";
        AssertCode(Admit(programDrift), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void SarahEventActorAndBlockingProgramDriftFailSemanticAdmission()
    {
        JsonObject eventDrift = SampleDocument();
        EntityEventRecords(eventDrift)[0]!.AsObject()["flags"] = 2;
        AssertCode(Admit(eventDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject actorDrift = SampleDocument();
        EntityRecords(actorDrift)[0]!.AsObject()["x"] = 41;
        EntityRecords(actorDrift)[0]!.AsObject()["rawX"] = 41;
        AssertCode(Admit(actorDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject programDrift = SampleDocument();
        SarahProgramOperations(programDrift)[1]!.AsObject()["operandText"] = "2";
        AssertCode(Admit(programDrift), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void AstralZoneRecordAndPositionProgramDriftFailSemanticAdmission()
    {
        JsonObject recordDrift = SampleDocument();
        ZoneRecords(recordDrift)[7]!.AsObject()["resolvedTargetAddress"] = 331367;
        AssertCode(Admit(recordDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject programDrift = SampleDocument();
        AstralZoneProgramOperations(programDrift)[1]!.AsObject()["operandText"] =
            "128,7,4,UP";
        AssertCode(Admit(programDrift), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void MessengerEventActorsAndAcceptedProgramDriftFailSemanticAdmission()
    {
        JsonObject zoneDrift = SampleDocument();
        ZoneRecords(zoneDrift)[
            OriginalMapRuntimeAdmission.MessengerZoneEventRecordOrdinal - 1]!
            .AsObject()["relativeOffset"] =
                OriginalMapRuntimeAdmission.MessengerZoneEventRelativeOffset + 1;
        AssertCode(Admit(zoneDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject actorDrift = SampleDocument();
        EntityRecords(actorDrift)[
            OriginalMapRuntimeAdmission.MessengerActor143SourceRecordOrdinal - 1]!
            .AsObject()["mapSprite"] =
                OriginalMapRuntimeAdmission.MessengerActor143MapSprite - 1;
        AssertCode(Admit(actorDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject guardDrift = SampleDocument();
        EntityRecords(guardDrift)[
            OriginalMapRuntimeAdmission.MessengerGuard138SourceRecordOrdinal - 1]!
            .AsObject()["x"] = OriginalMapRuntimeAdmission.MessengerGuard138X - 1;
        AssertCode(Admit(guardDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject branchDrift = SampleDocument();
        MessengerAcceptedProgramOperations(branchDrift)[6]!
            .AsObject()["operandText"] = "129";
        AssertCode(Admit(branchDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject controlDrift = SampleDocument();
        MessengerMainProgramOperations(controlDrift)[103]!
            .AsObject()["targetAddresses"] = new JsonArray(333333);
        AssertCode(Admit(controlDrift), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void CastleGateEventAndProgramDriftFailSemanticAdmission()
    {
        JsonObject zoneDrift = SampleDocument();
        ZoneRecords(zoneDrift)[
            OriginalMapRuntimeAdmission.CastleGateZoneEventRecordOrdinal - 1]!
            .AsObject()["x"] = OriginalMapRuntimeAdmission.CastleGateTriggerX + 1;
        AssertCode(Admit(zoneDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject programDrift = SampleDocument();
        CastleGateProgramOperations(programDrift)[2]!
            .AsObject()["operandText"] = "2";
        AssertCode(Admit(programDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject restoredGuardDrift = SampleDocument();
        CastleGateProgramOperations(restoredGuardDrift)[21]!
            .AsObject()["operandText"] = "2";
        AssertCode(Admit(restoredGuardDrift),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void ProductionSurfaceExposesOnlyThePathBoundReader()
    {
        Type readerType = typeof(PrivateCanonicalMap3ImportReader);
        System.Reflection.ConstructorInfo constructor = Assert.Single(readerType.GetConstructors());
        Assert.Equal(
            new[] { typeof(string) },
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.DoesNotContain(
            readerType.GetMethods(),
            method => method.DeclaringType == readerType &&
                method.IsStatic && method.ReturnType == readerType);
    }

    [Fact]
    public void UnknownShapeAndProvenanceDriftFailClosed()
    {
        JsonObject unknownRoot = SampleDocument();
        unknownRoot["unexpected"] = true;
        AssertCode(Admit(unknownRoot), OriginalMapImportFailureCode.InvalidDocument);

        JsonObject unknownMap = SampleDocument();
        Map(unknownMap, 3)["unexpected"] = true;
        AssertCode(Admit(unknownMap), OriginalMapImportFailureCode.InvalidDocument);

        byte[] ordinaryBytes = DocumentBytes(SampleDocument());
        const string idProperty = "\"id\": \"sf2-canonical-map-import-v1\"";
        string ordinaryJson = Encoding.UTF8.GetString(ordinaryBytes);
        int idOffset = ordinaryJson.IndexOf(idProperty, StringComparison.Ordinal);
        Assert.True(idOffset >= 0);
        int insertionOffset = ordinaryJson.IndexOf(',', idOffset) + 1;
        Assert.True(insertionOffset > 0);
        string duplicatePropertyJson = ordinaryJson.Insert(
            insertionOffset,
            "\n  " + idProperty + ",");
        byte[] duplicatePropertyBytes = Encoding.UTF8.GetBytes(duplicatePropertyJson);
        AssertCode(
            PrivateCanonicalMap3ImportReader.AdmitSemanticDocumentForTests(
                duplicatePropertyBytes),
            OriginalMapImportFailureCode.DuplicateIdentity);

        JsonObject wrongRom = SampleDocument();
        wrongRom["romSha256"] = new string('0', 64);
        AssertCode(Admit(wrongRom), OriginalMapImportFailureCode.ProvenanceMismatch);

        JsonObject wrongCommit = SampleDocument();
        wrongCommit["upstream"]!.AsObject()["commit"] = new string('0', 40);
        AssertCode(Admit(wrongCommit), OriginalMapImportFailureCode.ProvenanceMismatch);
    }

    [Fact]
    public void Map3PaletteAndOrderedTilesetReferenceDriftFailsClosed()
    {
        JsonObject wrongPalette = SampleDocument();
        Map(wrongPalette, 3)["palette"] = 1;
        AssertCode(Admit(wrongPalette), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongTileset = SampleDocument();
        Map(wrongTileset, 3)["tilesets"]!.AsArray()[4] = 67;
        AssertCode(Admit(wrongTileset), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject reorderedTilesets = SampleDocument();
        JsonArray slots = Map(reorderedTilesets, 3)["tilesets"]!.AsArray();
        JsonNode? first = slots[0]!.DeepClone();
        slots[0] = slots[1]!.DeepClone();
        slots[1] = first;
        AssertCode(Admit(reorderedTilesets), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void DuplicateMissingAndDanglingIdentitiesFailClosed()
    {
        JsonObject duplicateMap = SampleDocument();
        Map(duplicateMap, 4)["id"] = 3;
        AssertCode(Admit(duplicateMap), OriginalMapImportFailureCode.DuplicateIdentity);

        JsonObject duplicateLayout = SampleDocument();
        JsonArray layouts = ResourceArray(duplicateLayout, "layouts");
        layouts.Add(layouts[0]!.DeepClone());
        AssertCode(Admit(duplicateLayout), OriginalMapImportFailureCode.DuplicateIdentity);

        JsonObject missingLayout = SampleDocument();
        ResourceArray(missingLayout, "layouts").Clear();
        AssertCode(Admit(missingLayout), OriginalMapImportFailureCode.MissingReference);

        JsonObject danglingSetup = SampleDocument();
        Map(danglingSetup, 3)["references"]!.AsObject()["setupRoute"] = "missing-route";
        AssertCode(Admit(danglingSetup), OriginalMapImportFailureCode.MissingReference);
    }

    [Fact]
    public void LayoutRangeBlockReferenceAndControlledSetupDriftFailClosed()
    {
        JsonObject wrongLength = SampleDocument();
        LayoutWords(wrongLength).RemoveAt(0);
        AssertCode(Admit(wrongLength), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject outOfRangeWord = SampleDocument();
        LayoutWords(outOfRangeWord)[0] = 65536;
        AssertCode(Admit(outOfRangeWord), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject missingBlock = SampleDocument();
        LayoutWords(missingBlock)[0] = SampleBlockCount;
        AssertCode(Admit(missingBlock), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongDefault = SampleDocument();
        ResourceArray(wrongDefault, "setupRoutes")[0]!.AsObject()["defaultSetup"] =
            "ms_map3_variant_a";
        AssertCode(Admit(wrongDefault), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void BlocksetIdentityShapeWordRangeAndLayoutCrossReferenceFailClosed()
    {
        JsonObject wrongResource = SampleDocument();
        ResourceArray(wrongResource, "blocksets")[0]!.AsObject()["id"] = "OtherBlocks";
        Map(wrongResource, 3)["references"]!.AsObject()["blockset"] = "OtherBlocks";
        AssertCode(Admit(wrongResource), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongShape = SampleDocument();
        BlockWords(wrongShape, 0).RemoveAt(0);
        AssertCode(Admit(wrongShape), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject outOfRangeWord = SampleDocument();
        BlockWords(outOfRangeWord, 0)[0] = 65536;
        AssertCode(Admit(outOfRangeWord), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject danglingLayout = SampleDocument();
        LayoutWords(danglingLayout)[0] = SampleBlockCount;
        AssertCode(Admit(danglingLayout), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void BowieAndSchoolDoorCopiesAndCurrentWordCollisionPolarityFailClosed()
    {
        JsonObject missingDoor = SampleDocument();
        ResourceArray(missingDoor, "stepEventTables")[0]!
            .AsObject()["records"]!.AsArray().Clear();
        AssertCode(Admit(missingDoor), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject passableDestination = SampleDocument();
        LayoutWords(passableDestination)[Index(41, 13)] = 0;
        AssertCode(Admit(passableDestination), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject passableBowieDestination = SampleDocument();
        LayoutWords(passableBowieDestination)[Index(4, 8)] = 0;
        AssertCode(
            Admit(passableBowieDestination),
            OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject blockedSource = SampleDocument();
        LayoutWords(blockedSource)[Index(62, 0)] = OriginalMapTraversal.CollisionMask;
        AssertCode(Admit(blockedSource), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongResource = SampleDocument();
        ResourceArray(wrongResource, "stepEventTables")[0]!.AsObject()["id"] =
            "OtherStepEvents";
        Map(wrongResource, 3)["references"]!.AsObject()["stepEventTable"] =
            "OtherStepEvents";
        AssertCode(Admit(wrongResource), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongOrdinal = SampleDocument();
        StepRecords(wrongOrdinal).RemoveAt(0);
        AssertCode(Admit(wrongOrdinal), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject duplicateDoor = SampleDocument();
        JsonArray duplicateRecords = StepRecords(duplicateDoor);
        duplicateRecords.Add(duplicateRecords[5]!.DeepClone());
        AssertCode(Admit(duplicateDoor), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongGeometry = SampleDocument();
        StepRecords(wrongGeometry)[5]!.AsObject()["destination"] =
            JsonSerializer.SerializeToNode(Point(42, 13));
        AssertCode(Admit(wrongGeometry), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongBowieGeometry = SampleDocument();
        StepRecords(wrongBowieGeometry)[0]!.AsObject()["destination"] =
            JsonSerializer.SerializeToNode(Point(5, 8));
        AssertCode(
            Admit(wrongBowieGeometry),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void AreaResourceIdentityCountOrderBoundsAndFullSourceRecordFailClosed()
    {
        JsonObject wrongResource = SampleDocument();
        ResourceArray(wrongResource, "areaTables")[0]!.AsObject()["id"] = "OtherAreas";
        Map(wrongResource, 3)["references"]!.AsObject()["areaTable"] = "OtherAreas";
        AssertCode(Admit(wrongResource), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongCount = SampleDocument();
        AreaRecords(wrongCount).RemoveAt(2);
        AssertCode(Admit(wrongCount), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject reordered = SampleDocument();
        JsonArray reorderedRecords = AreaRecords(reordered);
        JsonNode first = reorderedRecords[0]!.DeepClone();
        reorderedRecords[0] = reorderedRecords[1]!.DeepClone();
        reorderedRecords[1] = first;
        AssertCode(Admit(reordered), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject changedBounds = SampleDocument();
        AreaRecords(changedBounds)[0]!.AsObject()["mainLayerEnd"] =
            JsonSerializer.SerializeToNode(Point(49, 31));
        AssertCode(Admit(changedBounds), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject outOfBounds = SampleDocument();
        AreaRecords(outOfBounds)[2]!.AsObject()["mainLayerEnd"] =
            JsonSerializer.SerializeToNode(Point(64, 19));
        AssertCode(Admit(outOfBounds), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject changedParallax = SampleDocument();
        AreaRecords(changedParallax)[1]!.AsObject()["mainLayerParallax"] =
            JsonSerializer.SerializeToNode(Point(255, 256));
        AssertCode(Admit(changedParallax), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject changedOpaqueMusic = SampleDocument();
        AreaRecords(changedOpaqueMusic)[1]!.AsObject()["defaultMusic"] = 9;
        AssertCode(Admit(changedOpaqueMusic), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void SameMapWarpResourceRowsAndExactSelectedProjectionFailClosed()
    {
        JsonObject wrongResource = SampleDocument();
        ResourceArray(wrongResource, "warpEventTables")[0]!.AsObject()["id"] =
            "OtherWarpEvents";
        Map(wrongResource, 3)["references"]!.AsObject()["warpEventTable"] =
            "OtherWarpEvents";
        AssertCode(Admit(wrongResource), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongCount = SampleDocument();
        WarpRecords(wrongCount).RemoveAt(0);
        AssertCode(Admit(wrongCount), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongSourceKind = SampleDocument();
        ResourceArray(wrongSourceKind, "warpEventTables")[0]!.AsObject()["sourceKind"] =
            "otherWarpEvents";
        AssertCode(Admit(wrongSourceKind), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject unknownField = SampleDocument();
        WarpRecords(unknownField)[8]!.AsObject()["unexpected"] = true;
        AssertCode(Admit(unknownField), OriginalMapImportFailureCode.InvalidDocument);

        JsonObject reordered = SampleDocument();
        JsonArray reorderedRows = WarpRecords(reordered);
        JsonNode row5 = reorderedRows[5]!.DeepClone();
        reorderedRows[5] = reorderedRows[8]!.DeepClone();
        reorderedRows[8] = row5;
        AssertCode(Admit(reordered), OriginalMapImportFailureCode.InvalidMapProjection);

        foreach ((string field, JsonNode? value) in new[]
        {
            ("scrollMode", JsonValue.Create(1)),
            ("retainsCoordinates", JsonValue.Create(true)),
            ("scrollDirection", JsonValue.Create(2)),
            ("targetMap", JsonValue.Create(3)),
            ("facing", JsonValue.Create(1)),
            ("reserved", JsonValue.Create(1)),
        })
        {
            JsonObject drift = SampleDocument();
            WarpRecords(drift)[8]!.AsObject()[field] = value;
            AssertCode(Admit(drift), OriginalMapImportFailureCode.InvalidMapProjection);
        }

        JsonObject triggerDrift = SampleDocument();
        WarpRecords(triggerDrift)[8]!.AsObject()["trigger"] =
            JsonSerializer.SerializeToNode(Point(53, 3));
        AssertCode(Admit(triggerDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject destinationDrift = SampleDocument();
        WarpRecords(destinationDrift)[5]!.AsObject()["destination"] =
            JsonSerializer.SerializeToNode(Point(58, 12));
        AssertCode(Admit(destinationDrift), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void NorthMap19RuntimeAndOutboundWarpFailClosedOnReferenceSetupAndProjectionDrift()
    {
        JsonObject warpDrift = SampleDocument();
        WarpRecords(warpDrift)[0]!.AsObject()["targetMap"] = 20;
        AssertCode(Admit(warpDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject mapReferenceDrift = SampleDocument();
        Map(mapReferenceDrift, 19)["references"]!.AsObject()["layout"] = "missing-layout";
        AssertCode(Admit(mapReferenceDrift), OriginalMapImportFailureCode.MissingReference);

        JsonObject animationDrift = SampleDocument();
        Map(animationDrift, 19)["references"]!.AsObject()["animationTable"] =
            "Map03s9_Animations";
        AssertCode(Admit(animationDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject setupDrift = SampleDocument();
        ResourceById(setupDrift, "setupRoutes", "MapSetupRoute19")["defaultSetup"] =
            "ms_map19_flag501";
        AssertCode(Admit(setupDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject variantOrderDrift = SampleDocument();
        JsonArray variants = ResourceById(
            variantOrderDrift,
            "setupRoutes",
            "MapSetupRoute19")["flagVariants"]!.AsArray();
        JsonNode first = variants[0]!.DeepClone();
        variants[0] = variants[1]!.DeepClone();
        variants[1] = first;
        AssertCode(Admit(variantOrderDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject blockedDestination = SampleDocument();
        ResourceById(blockedDestination, "layouts", "Map19s1_Layout")["words"]!
            .AsArray()[Index(26, 30)] = OriginalMapTraversal.CollisionMask;
        AssertCode(Admit(blockedDestination), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject entityReferenceDrift = SampleDocument();
        ResourceById(entityReferenceDrift, "setupDefinitions", "ms_map19")
            ["references"]!.AsObject()["entities"] = "missing-map19-entities";
        AssertCode(Admit(entityReferenceDrift), OriginalMapImportFailureCode.MissingReference);
    }

    [Fact]
    public void RoofOnLoadResourceRowsAndExactClearProjectionFailClosed()
    {
        JsonObject wrongResource = SampleDocument();
        ResourceArray(wrongResource, "roofEventTables")[0]!.AsObject()["id"] =
            "OtherRoofEvents";
        Map(wrongResource, 3)["references"]!.AsObject()["roofEventTable"] =
            "OtherRoofEvents";
        AssertCode(Admit(wrongResource), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject wrongCount = SampleDocument();
        RoofRecords(wrongCount).RemoveAt(9);
        AssertCode(Admit(wrongCount), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject unknownField = SampleDocument();
        RoofRecords(unknownField)[0]!.AsObject()["unexpected"] = true;
        AssertCode(Admit(unknownField), OriginalMapImportFailureCode.InvalidDocument);

        JsonObject reordered = SampleDocument();
        JsonArray reorderedRows = RoofRecords(reordered);
        JsonNode first = reorderedRows[0]!.DeepClone();
        reorderedRows[0] = reorderedRows[1]!.DeepClone();
        reorderedRows[1] = first;
        AssertCode(Admit(reordered), OriginalMapImportFailureCode.InvalidMapProjection);

        foreach ((string field, JsonNode? value) in new[]
        {
            ("trigger", JsonSerializer.SerializeToNode(Point(5, 8))),
            ("source", JsonSerializer.SerializeToNode(Point(0, 0))),
            ("size", JsonSerializer.SerializeToNode(new { width = 6, height = 8 })),
            ("destination", JsonSerializer.SerializeToNode(Point(3, 32))),
        })
        {
            JsonObject drift = SampleDocument();
            RoofRecords(drift)[0]!.AsObject()[field] = value;
            AssertCode(Admit(drift), OriginalMapImportFailureCode.InvalidMapProjection);
        }
    }

    [Fact]
    public void MissingPrivatePathReturnsOnlyAPathFreeTypedDiagnostic()
    {
        string missing = Path.Combine(
            Path.GetTempPath(),
            "sf2-private-map-import-does-not-exist",
            "canonical-map-import.json");
        OriginalMapImportRejected rejected = Assert.IsType<OriginalMapImportRejected>(
            new PrivateCanonicalMap3ImportReader(missing).Admit(
                new OriginalMapImportRequest(
                    PrivateCanonicalMap3ImportReader.PackageId,
                    ContentProfile.PrivateLocal,
                    new string('0', 64))));

        Assert.Equal(OriginalMapImportFailureCode.PackageUnavailable, rejected.Diagnostic.Code);
        Assert.DoesNotContain(missing, rejected.Diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EntityPopulationShapeCoordinatesKindsAndSetupReferenceFailClosed()
    {
        JsonObject unknownField = SampleDocument();
        EntityRecords(unknownField)[0]!.AsObject()["unexpected"] = true;
        AssertCode(Admit(unknownField), OriginalMapImportFailureCode.InvalidDocument);

        JsonObject coordinateDrift = SampleDocument();
        EntityRecords(coordinateDrift)[0]!.AsObject()["x"] = 2;
        AssertCode(Admit(coordinateDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject kindDrift = SampleDocument();
        EntityRecords(kindDrift)[0]!.AsObject()["actionValue"] = 0xFF000000U;
        AssertCode(Admit(kindDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject referenceDrift = SampleDocument();
        SetupReferences(referenceDrift)["entities"] = "other-entities";
        AssertCode(Admit(referenceDrift), OriginalMapImportFailureCode.MissingReference);
    }

    [Sf2.Remake.TestSupport.PrivateInputFact("SF2_PRIVATE_CANONICAL_MAP_IMPORT")]
    public void AcceptedIgnoredCanonicalImportCanBeCheckedLocallyWithoutBecomingATestInput()
    {
        string path = Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput(
            "SF2_PRIVATE_CANONICAL_MAP_IMPORT");

        OriginalMapImportResult result = new PrivateCanonicalMap3ImportReader(path).Admit(
            new OriginalMapImportRequest(
                PrivateCanonicalMap3ImportReader.PackageId,
                ContentProfile.PrivateLocal,
                AcceptedCanonicalDigest));
        Assert.True(
            result is OriginalMapImportAccepted,
            result is OriginalMapImportRejected rejected
                ? $"{rejected.Diagnostic.Code}:{rejected.Diagnostic.Field}:{rejected.Diagnostic.Message}"
                : "The private import returned an unknown result.");
        OriginalMapImportAccepted accepted = Assert.IsType<OriginalMapImportAccepted>(result);

        Assert.Equal(AcceptedCanonicalDigest, accepted.Receipt.ContentDigest);
        Assert.Equal(AcceptedDecodedLayoutDigest, accepted.Receipt.DecodedLayoutDigest);
        Assert.Equal(AcceptedCollisionDigest, accepted.Receipt.CollisionProjectionDigest);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.CurrentAreaDiagnosticCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.AreaSourceRecordAdmissionCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.BlocksetSourceAdmissionCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.VisualReferenceAdmissionCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.SelectedSetupEntityPopulationCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.RoofOnLoadClearCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.Zone601InterceptionCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.SarahRouteCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.Entity142AcknowledgementCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.AstralZoneHandoffCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.MessengerAcceptanceCapability,
            accepted.Receipt.Capabilities);
        Assert.Contains(
            PrivateCanonicalMap3ImportReader.NorthMap19TransitionCapability,
            accepted.Receipt.Capabilities);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedRuntimeCatalog(
            accepted.Definition.RuntimeCatalog));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedNorthMap19Transition(
            accepted.Definition.NorthMap19Transition));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedEntity142(
            accepted.Definition.Entity142,
            accepted.Definition.EntityPopulation,
            accepted.Definition.Traversal,
            accepted.Definition.WorkingLayout));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedRoofOnLoadClear(
            accepted.Definition.RoofOnLoadClear));
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
            accepted.Definition.EntityPopulation.ResourceId);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedEntityRecordCount,
            accepted.Definition.EntityPopulation.Records.Count);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedFixedEntityRecordCount,
            accepted.Definition.EntityPopulation.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Fixed));
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedWalkingEntityRecordCount,
            accepted.Definition.EntityPopulation.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Walking));
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedSequencedEntityRecordCount,
            accepted.Definition.EntityPopulation.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Sequenced));
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedEntityProjectionDigest,
            accepted.Definition.EntityPopulation.ProjectionDigest);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedEntityPopulation(
            accepted.Definition.EntityPopulation));
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedBlocksetResourceId,
            accepted.Definition.BlockCatalog.ResourceId);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedBlockCount,
            accepted.Definition.BlockCatalog.Records.Count);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedBlocksetProjectionDigest,
            accepted.Definition.BlockCatalog.ProjectionDigest);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedBlocksetProjection(
            accepted.Definition.BlockCatalog));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedVisualResourceSelection(
            accepted.Definition.VisualResourceSelection));
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedVisualReferenceProjectionDigest,
            accepted.Definition.VisualResourceSelection.ProjectionDigest);
        Assert.Equal(3, accepted.Definition.Traversal.ActiveAreas.Count);
        Assert.Equal(
            2,
            accepted.Definition.Traversal.SelectActiveArea(
                accepted.Definition.ControlledAdmission.Position)!.OneBasedRecordOrdinal);
        Assert.Equal(
            "B60D96CC0359E390A8C26FDA9CE3313023ACB4774902CD99E12CB798041EB225",
            OriginalMapRuntimeAdmission.AcceptedAreaSourceProjectionDigest);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedAreaSourceProjection(
            accepted.Definition.AreaCatalog));
        ushort collisionClass = (ushort)(
            accepted.Definition.WorkingLayout[41, 13] & OriginalMapTraversal.CollisionMask);
        Assert.Equal(OriginalMapTraversal.CollisionMask, collisionClass);
        Assert.False(OriginalMapTraversal.IsBlocked(
            accepted.Definition.WorkingLayout,
            new MapPosition(62, 0)));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedZone601(
            accepted.Definition.Zone601,
            accepted.Definition.EntityPopulation,
            accepted.Definition.Traversal,
            accepted.Definition.WorkingLayout));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedSarah(
            accepted.Definition.Sarah,
            accepted.Definition.EntityPopulation,
            accepted.Definition.Traversal,
            accepted.Definition.WorkingLayout));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedAstralZone(
            accepted.Definition.AstralZone,
            accepted.Definition.Sarah,
            accepted.Definition.Zone601,
            accepted.Definition.Traversal,
            accepted.Definition.WorkingLayout));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedMessengerAcceptance(
            accepted.Definition.MessengerAcceptance,
            accepted.Definition.EntityPopulation,
            accepted.Definition.Sarah,
            accepted.Definition.Entity142,
            accepted.Definition.AstralZone,
            accepted.Definition.Traversal,
            accepted.Definition.WorkingLayout));
        OriginalMapStepCopyDefinition stepCopy =
            Assert.IsType<OriginalMapStepCopyDefinition>(
                accepted.Definition.ControlledStepCopy);
        Assert.Equal(6, stepCopy.Identity.OneBasedRecordOrdinal);
        Assert.Equal((62, 0, 41, 13, 1, 1), Geometry(stepCopy.Copy));
        OriginalMapStepCopyDefinition bowieDoor =
            Assert.IsType<OriginalMapStepCopyDefinition>(
                accepted.Definition.BowieDoorStepCopy);
        Assert.Equal(1, bowieDoor.Identity.OneBasedRecordOrdinal);
        Assert.Equal((62, 0, 4, 8, 1, 1), Geometry(bowieDoor.Copy));
        CheckRoyalRouteFromControlledMap19Entry(path);
    }

    [Fact]
    public void Map20SourceIdentityDriftReportsItsOwnField()
    {
        JsonObject document = SampleDocument();
        document["maps"]![20]!["sourceSymbol"] = "Map19";
        OriginalMapImportRejected rejected = Assert.IsType<OriginalMapImportRejected>(Admit(document));
        Assert.Equal(OriginalMapImportFailureCode.InvalidMapProjection, rejected.Diagnostic.Code);
        Assert.Equal("maps[20].sourceSymbol", rejected.Diagnostic.Field);
    }

    private static void CheckRoyalRouteFromControlledMap19Entry(string path)
    {
        GameSession session = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(
                new PrivateCanonicalMap3ImportReader(path), Request(AcceptedCanonicalDigest))).Session;
        MapScenarioAccepted publicPackage = Assert.IsType<MapScenarioAccepted>(
            new PublicSyntheticMap3PackageReader(Path.Combine(AppContext.BaseDirectory, "content"))
                .Admit(new MapScenarioRequest(
                    PublicSyntheticMap3PackageReader.PackageId, ContentProfile.PublicSynthetic)));
        PrivateOriginalMapBattleBridgeSnapshot bridge = Assert.IsType<PrivateOriginalMapBattleBridgeBound>(
            session.BindPrivateOriginalMapBattleBridge(
                Assert.Single(publicPackage.Scenario.MapContext!.PublicSyntheticBattles.Definitions))).Bridge;
        PrivateOriginalMapSessionSnapshot initial = session.PrivateOriginalMapSnapshot;
        OriginalMapImportDefinition definition = initial.Definition;
        OriginalMapExplorationRuntimeDefinition map19 = definition.RuntimeCatalog.Resolve(new MapId("map19"));

        // Explicit test-only entry seed, not a natural Map 3 route or an init execution claim.
        // Reuse the production state factories and validated snapshot without adding a runtime seed API.
        static T State<T>(string method, params object[] arguments) => (T)typeof(T)
            .GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, arguments)!;
        PrivateOriginalMapSessionSnapshot entry = new(
            definition, initial.Receipt, map19.WorkingLayout, 1, new MapPosition(26, 30),
            lastTraversal: null, controlledStepCopyApplied: false, lastLayoutMutation: null,
            zone601: State<PrivateOriginalMapZone601State>(
                "AstralZoneRepositioned", definition.Zone601!, definition.AstralZone!),
            sarah: State<PrivateOriginalMapSarahState>(
                "MessengerFollowerReady", definition.Sarah!, definition.AstralZone!, definition.MessengerAcceptance!),
            entity142: State<PrivateOriginalMapEntity142State>(
                "ReleaseRouteOccupancy", definition.Entity142!,
                State<PrivateOriginalMapEntity142State>("Acknowledged", definition.Entity142!, 1L),
                definition.MessengerAcceptance!),
            castleGate: State<PrivateOriginalMapCastleGateState>("Completed", definition.CastleGate!),
            currentRuntime: map19,
            lastCrossMapTransition: (PrivateOriginalMapCrossMapTransitionReceipt)Activator.CreateInstance(
                typeof(PrivateOriginalMapCrossMapTransitionReceipt), BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null, args: [definition.NorthMap19Transition!, 1L], culture: null)!);
        typeof(GameSession).GetField("_privateOriginalMapSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(session, entry);

        string fixturePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json"));
        using JsonDocument fixture = JsonDocument.Parse(File.ReadAllText(fixturePath));
        JsonElement route = fixture.RootElement.GetProperty("static").GetProperty("routeGraph")
            .GetProperty("segments").EnumerateArray()
            .Single(segment => segment.GetProperty("id").GetString() == "map19-entry-to-royal-warp");
        JsonElement inputs = route.GetProperty("inputs");
        JsonElement points = route.GetProperty("points");
        Assert.Equal(38, inputs.GetArrayLength());
        int zoneHits = 0;
        for (int index = 0; index < inputs.GetArrayLength(); index++)
        {
            ExplorationDirection direction = inputs[index].GetString() switch
            {
                "Up" => ExplorationDirection.North,
                "Down" => ExplorationDirection.South,
                "Left" => ExplorationDirection.West,
                "Right" => ExplorationDirection.East,
                _ => throw new InvalidOperationException("Unknown route input."),
            };
            Assert.Equal(new MapPosition(points[index][0].GetInt32(), points[index][1].GetInt32()),
                session.PrivateOriginalMapSnapshot.PlayerPosition);
            PrivateOriginalMapPlayerLocomotionStarted move = session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(direction));
            PrivateOriginalMapSessionSnapshot after = move.Move.Snapshot;
            Assert.Equal(entry.SimulationStep + index + 1, after.SimulationStep);
            Assert.Same(entry.Receipt, after.Receipt);
            Assert.Same(entry.Zone601, after.Zone601);
            Assert.Same(entry.Sarah, after.Sarah);
            Assert.Same(entry.Entity142, after.Entity142);
            Assert.Same(entry.MessengerAcceptance, after.MessengerAcceptance);
            Assert.Same(entry.CastleGate, after.CastleGate);
            Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
            Assert.Null(after.LastSarah);
            Assert.Null(after.LastEntity142Acknowledgement);
            Assert.Null(after.LastAstralZone);
            Assert.Null(after.LastMessengerAcceptance);
            Assert.Null(after.LastCastleGate);
            if (index < 37)
            {
                Assert.Same(map19, after.CurrentRuntime);
                Assert.Equal(new MapPosition(points[index + 1][0].GetInt32(), points[index + 1][1].GetInt32()),
                    after.PlayerPosition);
                if (after.PlayerPosition == new MapPosition(29, 15) || after.PlayerPosition == new MapPosition(25, 13))
                {
                    zoneHits++;
                    Assert.Equal(0x1400, after.WorkingLayout[after.PlayerPosition.X, after.PlayerPosition.Y] & 0x3C00);
                }
            }
            else
            {
                PrivateOriginalMapCrossMapTransitionReceipt receipt =
                    Assert.IsType<PrivateOriginalMapCrossMapTransitionReceipt>(move.Move.CrossMapTransition);
                Assert.Equal(new MapPosition(23, 3), receipt.Trigger);
                Assert.Equal(OriginalMapRuntimeAdmission.RoyalMap20TransitionCapability, receipt.Capability);
                Assert.Equal((byte)3, move.Animation.OpaqueFacing);
                Assert.Equal(new MapId("map20"), after.Map);
                Assert.Equal(new MapPosition(23, 37), after.PlayerPosition);
                Assert.Equal(2, after.CurrentArea.OneBasedRecordOrdinal);
                Assert.Same(definition.RuntimeCatalog.Resolve(after.Map), after.CurrentRuntime);
                Assert.Same(after.CurrentRuntime.WorkingLayout, after.WorkingLayout);
                Assert.Equal("ms_map20_InitFunction", after.CurrentRuntime.SelectedInitIdentity);
            }

            for (int tick = 0; tick < 13 && session.PrivateOriginalMapPlayerLocomotion.IsMoving; tick++)
            {
                session.AdvancePrivateOriginalMapPlayerLocomotion();
            }

            Assert.False(session.PrivateOriginalMapPlayerLocomotion.IsMoving);
        }

        Assert.Equal(2, zoneHits);
        Assert.True(entry.Zone601!.Flag601Set && entry.Sarah!.TemporaryRouteFlag256Set && entry.AstralZoneFlag260Set);
        Assert.True(entry.Entity142!.Flag261Set && entry.Entity142.Flag602Set);
        Assert.True(entry.MessengerAcceptance!.Accepted && entry.CastleGate!.Flag604Set);
        Assert.Equal(PrivateOriginalMapBattleBridgeStatus.Ready, bridge.Status);

        OriginalMapPalaceFirstVisitDefinition palace = Assert.IsType<OriginalMapPalaceFirstVisitDefinition>(
            definition.PalaceFirstVisit);
        Assert.Equal(OriginalMapRuntimeAdmission.PalaceScriptProjectionSha256, palace.ScriptProjectionSha256);
        Assert.NotEqual(OriginalMapRuntimeAdmission.PalaceSourceControlEffectSha256, palace.ScriptProjectionSha256);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedPalaceFirstVisit(palace, definition.RuntimeCatalog));
        PrivateOriginalMapSessionSnapshot preInit = session.PrivateOriginalMapSnapshot;
        Assert.Null(preInit.PalaceFirstVisit);
        Assert.Equal(new MapPosition(23, 37), preInit.PlayerPosition);
        var applied = Assert.IsType<PrivateOriginalMapPalaceFirstVisitApplied>(
            session.RequestPrivateOriginalMapInteraction(preInit.SimulationStep));
        Assert.Equal(preInit.SimulationStep + 1, applied.Snapshot.SimulationStep);
        Assert.Equal(new MapPosition(23, 39), applied.Snapshot.PlayerPosition);
        Assert.Equal((byte)3, session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing);
        Assert.Same(preInit.CurrentRuntime, applied.Snapshot.CurrentRuntime);
        Assert.Same(preInit.EntityPopulation, applied.Snapshot.EntityPopulation);
        Assert.Same(preInit.Zone601, applied.Snapshot.Zone601);
        Assert.Same(preInit.Sarah, applied.Snapshot.Sarah);
        Assert.Same(preInit.Entity142, applied.Snapshot.Entity142);
        Assert.Same(preInit.MessengerAcceptance, applied.Snapshot.MessengerAcceptance);
        Assert.Same(preInit.CastleGate, applied.Snapshot.CastleGate);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        Assert.True(applied.Receipt.CompletionFlag605Set && applied.Receipt.Entity130Hidden);
        Assert.Equal(OriginalMapPalaceFirstVisitPreset.ControlledClear605And507, applied.Receipt.Preset);
        Assert.Equal(new MapPosition(20, 39), applied.Receipt.Entity131Endpoint);
        var animation = session.PrivateOriginalMapPlayerLocomotion;
        var repeated = Assert.IsType<PrivateOriginalMapPalaceFirstVisitRejected>(
            session.RequestPrivateOriginalMapInteraction(applied.Snapshot.SimulationStep));
        Assert.Equal(PrivateOriginalMapPalaceFirstVisitFailureCode.AlreadyCompleted, repeated.Code);
        Assert.Same(applied.Snapshot, session.PrivateOriginalMapSnapshot);
        Assert.Same(animation, session.PrivateOriginalMapPlayerLocomotion);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        JsonElement returnRoute = fixture.RootElement.GetProperty("static").GetProperty("routeGraph")
            .GetProperty("segments").EnumerateArray()
            .Single(segment => segment.GetProperty("id").GetString() == "map20-palace-return-to-royal-warp");
        Assert.Equal(2, returnRoute.GetProperty("inputs").GetArrayLength());
        for (int index = 0; index < 2; index++)
        {
            Assert.Equal("Up", returnRoute.GetProperty("inputs")[index].GetString());
            Assert.Equal(new MapPosition(23, 39 - index), session.PrivateOriginalMapSnapshot.PlayerPosition);
            var move = session.BeginPrivateOriginalMapPlayerLocomotion(new MoveExplorationCommand(ExplorationDirection.North));
            Assert.Same(applied.Receipt, move.Move.Snapshot.PalaceFirstVisit);
            Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
            if (index == 0)
            {
                Assert.Null(move.Move.CrossMapTransition);
                Assert.Equal(new MapPosition(23, 38), move.Move.Snapshot.PlayerPosition);
            }
            else
            {
                var receipt = Assert.IsType<PrivateOriginalMapCrossMapTransitionReceipt>(move.Move.CrossMapTransition);
                Assert.Equal(OriginalMapRuntimeAdmission.RoyalReturnMap19TransitionCapability, receipt.Capability);
                Assert.Equal(5, receipt.RecordIdentity.OneBasedRecordOrdinal);
                Assert.Equal(new MapId("map19"), move.Move.Snapshot.Map);
                Assert.Equal(new MapPosition(23, 3), move.Move.Snapshot.PlayerPosition);
                Assert.Equal(1, move.Move.Snapshot.CurrentArea.OneBasedRecordOrdinal);
                Assert.Same(map19, move.Move.Snapshot.CurrentRuntime);
                Assert.Same(map19.WorkingLayout, move.Move.Snapshot.WorkingLayout);
                Assert.Equal((byte)2, receipt.DestinationOpaqueFacing);
                Assert.Equal((byte)2, move.Animation.OpaqueFacing);
                Assert.Same(move.Animation, session.PrivateOriginalMapPlayerLocomotion);
                Assert.Same(entry.Receipt, move.Move.Snapshot.Receipt);
                Assert.Same(entry.Zone601, move.Move.Snapshot.Zone601);
                Assert.Same(entry.Sarah, move.Move.Snapshot.Sarah);
                Assert.Same(entry.Entity142, move.Move.Snapshot.Entity142);
                Assert.Same(entry.MessengerAcceptance, move.Move.Snapshot.MessengerAcceptance);
                Assert.Same(entry.CastleGate, move.Move.Snapshot.CastleGate);
            }
            for (int tick = 0; tick < 13 && session.PrivateOriginalMapPlayerLocomotion.IsMoving; tick++)
                session.AdvancePrivateOriginalMapPlayerLocomotion();
            Assert.False(session.PrivateOriginalMapPlayerLocomotion.IsMoving);
        }
        Assert.Equal(applied.Snapshot.SimulationStep + 2, session.PrivateOriginalMapSnapshot.SimulationStep);
        JsonElement astralRoute = fixture.RootElement.GetProperty("static").GetProperty("routeGraph")
            .GetProperty("segments").EnumerateArray()
            .Single(row => row.GetProperty("id").GetString() == "map19-royal-return-to-astral");
        PrivateOriginalMapPlayerLocomotionStarted CompleteMove(ExplorationDirection direction)
        {
            var move = session.BeginPrivateOriginalMapPlayerLocomotion(new(direction));
            for (int tick = 0; tick < 13 && session.PrivateOriginalMapPlayerLocomotion.IsMoving; tick++)
                session.AdvancePrivateOriginalMapPlayerLocomotion();
            Assert.False(session.PrivateOriginalMapPlayerLocomotion.IsMoving);
            return move;
        }
        Assert.Equal(11, astralRoute.GetProperty("inputs").GetArrayLength());
        int pointIndex = 1;
        foreach (JsonElement input in astralRoute.GetProperty("inputs").EnumerateArray())
        {
            var direction = input.GetString() switch
            {
                "Up" => ExplorationDirection.North, "Down" => ExplorationDirection.South,
                "Left" => ExplorationDirection.West, "Right" => ExplorationDirection.East,
                _ => throw new InvalidOperationException("Unexpected accepted route input."),
            };
            var move = CompleteMove(direction);
            var point = astralRoute.GetProperty("points")[pointIndex++];
            Assert.Equal(new MapPosition(point[0].GetInt32(), point[1].GetInt32()), move.Move.Snapshot.PlayerPosition);
        }
        Assert.Equal((byte)2, session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing);
        Assert.True(session.PrivateOriginalMapSnapshot.AstralOccupiesRouteTile);
        var blocked = CompleteMove(ExplorationDirection.North);
        Assert.Equal(OriginalMapTraversalOutcome.BlockedByOccupiedEntity, blocked.Move.Traversal.Outcome);
        Assert.Equal(new MapPosition(16, 6), blocked.Move.Snapshot.PlayerPosition);
        Assert.Equal((byte)1, session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing);
        var astral = Assert.IsType<PrivateOriginalMapAstralAcceptanceApplied>(
            session.RequestPrivateOriginalMapInteraction(session.PrivateOriginalMapSnapshot.SimulationStep));
        var completion = Assert.IsType<PrivateOriginalMapAstralAcceptanceState>(astral.Snapshot.AstralAcceptance);
        Assert.True(completion.HandlerFlag607Set && completion.ProgramFlag608Set);
        Assert.False(astral.Snapshot.AstralOccupiesRouteTile);
        Assert.Same(applied.Receipt, astral.Snapshot.PalaceFirstVisit);
        Assert.Same(entry.MessengerAcceptance, astral.Snapshot.MessengerAcceptance);
        Assert.Same(entry.CastleGate, astral.Snapshot.CastleGate);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        Assert.Equal(PrivateOriginalMapAstralAcceptanceFailureCode.AlreadyCompleted,
            Assert.IsType<PrivateOriginalMapAstralAcceptanceRejected>(
                session.RequestPrivateOriginalMapInteraction(astral.Snapshot.SimulationStep)).Code);
        Assert.Same(astral.Snapshot, session.PrivateOriginalMapSnapshot);
        Assert.Equal(new MapPosition(16, 5), CompleteMove(ExplorationDirection.North).Move.Snapshot.PlayerPosition);
        Assert.Same(completion, session.PrivateOriginalMapSnapshot.AstralAcceptance);

        JsonElement towerRoute = fixture.RootElement.GetProperty("static").GetProperty("routeGraph")
            .GetProperty("segments").EnumerateArray()
            .Single(segment => segment.GetProperty("id").GetString() == "map19-astral-to-west-tower-warp");
        Assert.Equal(15, towerRoute.GetProperty("inputs").GetArrayLength());
        var released = session.PrivateOriginalMapSnapshot;
        for (int index = 1; index < 15; index++)
        {
            JsonElement from = towerRoute.GetProperty("points")[index];
            Assert.Equal(new MapPosition(from[0].GetInt32(), from[1].GetInt32()),
                session.PrivateOriginalMapSnapshot.PlayerPosition);
            ExplorationDirection direction = towerRoute.GetProperty("inputs")[index].GetString() switch
            {
                "Up" => ExplorationDirection.North,
                "Left" => ExplorationDirection.West,
                "Right" => ExplorationDirection.East,
                _ => throw new InvalidOperationException("Unexpected west-tower route input."),
            };
            var move = CompleteMove(direction).Move;
            Assert.Same(completion, move.Snapshot.AstralAcceptance);
            Assert.Same(applied.Receipt, move.Snapshot.PalaceFirstVisit);
            Assert.Same(released.Zone601, move.Snapshot.Zone601);
            Assert.Same(released.Sarah, move.Snapshot.Sarah);
            Assert.Same(released.Entity142, move.Snapshot.Entity142);
            Assert.Same(released.MessengerAcceptance, move.Snapshot.MessengerAcceptance);
            Assert.Same(released.CastleGate, move.Snapshot.CastleGate);
            Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
            if (index < 14)
            {
                Assert.Null(move.CrossMapTransition);
                Assert.Equal(new MapId("map19"), move.Snapshot.Map);
                JsonElement to = towerRoute.GetProperty("points")[index + 1];
                Assert.Equal(new MapPosition(to[0].GetInt32(), to[1].GetInt32()), move.Snapshot.PlayerPosition);
            }
            else
            {
                var warp = Assert.IsType<PrivateOriginalMapCrossMapTransitionReceipt>(move.CrossMapTransition);
                Assert.Equal(OriginalMapRuntimeAdmission.WestTowerMap20TransitionCapability, warp.Capability);
                Assert.Equal(1, warp.RecordIdentity.OneBasedRecordOrdinal);
                Assert.Equal(new MapPosition(6, 2), warp.Trigger);
                Assert.Equal(new MapId("map20"), move.Snapshot.Map);
                Assert.Equal(new MapPosition(6, 37), move.Snapshot.PlayerPosition);
                Assert.Equal(new OriginalMapAreaRecordIdentity("Map20s2_Areas", 2), move.Snapshot.CurrentAreaDefinition.Identity);
                Assert.Equal((byte)0, warp.DestinationOpaqueFacing);
                Assert.Equal((byte)0, session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing);
                Assert.Equal(PrivateOriginalMapPlayerLocomotionPhase.Relocated, session.PrivateOriginalMapPlayerLocomotion.Phase);
                Assert.False(session.PrivateOriginalMapPlayerLocomotion.IsMoving);
                Assert.Same(definition.RuntimeCatalog.Resolve(new MapId("map20")), move.Snapshot.CurrentRuntime);
            }
        }
        Assert.Equal(released.SimulationStep + 14, session.PrivateOriginalMapSnapshot.SimulationStep);
        var west = session.PrivateOriginalMapSnapshot;
        CompleteMove(ExplorationDirection.West);
        Assert.Equal(new MapPosition(5, 37), session.PrivateOriginalMapSnapshot.PlayerPosition);
        CompleteMove(ExplorationDirection.West);
        Assert.Equal(new MapPosition(4, 37), session.PrivateOriginalMapSnapshot.PlayerPosition);
        var middle = CompleteMove(ExplorationDirection.West).Move;
        var arrival = Assert.IsType<PrivateOriginalMapCrossMapTransitionReceipt>(middle.CrossMapTransition);
        Assert.Equal(OriginalMapRuntimeAdmission.MiddleTowerMap21TransitionCapability, arrival.Capability);
        Assert.Equal(4, arrival.RecordIdentity.OneBasedRecordOrdinal);
        Assert.Equal(new MapPosition(4, 37), arrival.Source);
        Assert.Equal(new MapPosition(3, 36), arrival.Trigger);
        Assert.Equal(new MapId("map21"), middle.Snapshot.Map);
        Assert.Equal(new MapPosition(3, 16), middle.Snapshot.PlayerPosition);
        Assert.Equal(new OriginalMapAreaRecordIdentity("Map21s2_Areas", 1), middle.Snapshot.CurrentAreaDefinition.Identity);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedMap21Runtime(middle.Snapshot.CurrentRuntime));
        Assert.Same(definition.RuntimeCatalog.Resolve(new("map21")), middle.Snapshot.CurrentRuntime);
        Assert.Same(middle.Snapshot.CurrentRuntime.WorkingLayout, middle.Snapshot.WorkingLayout);
        Assert.Equal((byte)0, arrival.DestinationOpaqueFacing);
        Assert.Equal((byte)0, session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing);
        Assert.Equal(PrivateOriginalMapPlayerLocomotionPhase.Relocated, session.PrivateOriginalMapPlayerLocomotion.Phase);
        Assert.Same(west.PalaceFirstVisit, middle.Snapshot.PalaceFirstVisit);
        Assert.Same(west.AstralAcceptance, middle.Snapshot.AstralAcceptance);
        Assert.Same(west.Zone601, middle.Snapshot.Zone601);
        Assert.Same(west.Sarah, middle.Snapshot.Sarah);
        Assert.Same(west.Entity142, middle.Snapshot.Entity142);
        Assert.Same(west.MessengerAcceptance, middle.Snapshot.MessengerAcceptance);
        Assert.Same(west.CastleGate, middle.Snapshot.CastleGate);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        CompleteMove(ExplorationDirection.East);
        Assert.Equal(new MapPosition(4, 16), session.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(OriginalMapTraversalOutcome.BlockedByOccupiedEntity, CompleteMove(ExplorationDirection.East).Move.Traversal.Outcome);
        var guardBefore = session.PrivateOriginalMapSnapshot;
        var guardAnimation = session.PrivateOriginalMapPlayerLocomotion;
        Assert.True(guardBefore.Sarah!.TemporaryRouteFlag256Set);
        var guardApplied = Assert.IsType<PrivateOriginalMapMiddleTowerGuardApplied>(
            session.RequestPrivateOriginalMapInteraction(guardBefore.SimulationStep));
        Assert.Same(guardAnimation, session.PrivateOriginalMapPlayerLocomotion);
        Assert.Equal((byte)0, session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing);
        Assert.Equal(new MapPosition(4, 16), guardApplied.Snapshot.PlayerPosition);
        Assert.Equal(new MapPosition(6, 16), guardApplied.Snapshot.MiddleTowerGuardPosition);
        Assert.True(guardApplied.Receipt.HandlerFlag256Set && guardApplied.Receipt.ProgramFlag401Set &&
            guardApplied.Receipt.ProgramStoryFlag1Set);
        Assert.Equal(new MapPosition(5, 16), definition.MiddleTowerGuard!.Actor.Position);
        Assert.Equal((byte)3, definition.MiddleTowerGuard.Actor.OpaqueFacing);
        CompleteMove(ExplorationDirection.East);
        Assert.Equal(new MapPosition(5, 16), session.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(OriginalMapTraversalOutcome.BlockedByOccupiedEntity, CompleteMove(ExplorationDirection.East).Move.Traversal.Outcome);
        CompleteMove(ExplorationDirection.North);
        var guardEndpoint = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(5, 15), guardEndpoint.PlayerPosition);
        Assert.Equal((byte)1, session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing);
        Assert.Same(guardApplied.Receipt, guardEndpoint.MiddleTowerGuard);
        Assert.Same(guardBefore.Receipt, guardEndpoint.Receipt);
        Assert.Same(guardBefore.PalaceFirstVisit, guardEndpoint.PalaceFirstVisit);
        Assert.Same(guardBefore.AstralAcceptance, guardEndpoint.AstralAcceptance);
        Assert.Same(guardBefore.Zone601, guardEndpoint.Zone601);
        Assert.Same(guardBefore.Sarah, guardEndpoint.Sarah);
        Assert.Same(guardBefore.Entity142, guardEndpoint.Entity142);
        Assert.Same(guardBefore.MessengerAcceptance, guardEndpoint.MessengerAcceptance);
        Assert.Same(guardBefore.CastleGate, guardEndpoint.CastleGate);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        var duplicate = Assert.IsType<PrivateOriginalMapMiddleTowerGuardRejected>(
            session.RequestPrivateOriginalMapInteraction(guardEndpoint.SimulationStep));
        Assert.Equal(PrivateOriginalMapMiddleTowerGuardFailureCode.AlreadyCompleted, duplicate.Code);
        Assert.Same(guardEndpoint, session.PrivateOriginalMapSnapshot);
        using var admissionFixture = JsonDocument.Parse(File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "../../../../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json"))));
        var extension = admissionFixture.RootElement.GetProperty("static").GetProperty("extensionRoute");
        var northRoute = extension.GetProperty("segments")[0];
        Assert.Equal("map21-terminal-to-north-exit", northRoute.GetProperty("id").GetString());
        Assert.Equal(18, northRoute.GetProperty("inputs").GetArrayLength());
        Assert.Equal(19, northRoute.GetProperty("points").GetArrayLength());
        for (int inputIndex = 0; inputIndex < 18; inputIndex++)
        {
            var point = northRoute.GetProperty("points")[inputIndex];
            Assert.Equal(new MapPosition(point[0].GetInt32(), point[1].GetInt32()), session.PrivateOriginalMapSnapshot.PlayerPosition);
            Assert.Equal(new MapId("map21"), session.PrivateOriginalMapSnapshot.Map);
            var direction = northRoute.GetProperty("inputs")[inputIndex].GetString() switch
            {
                "Up" => ExplorationDirection.North, "Right" => ExplorationDirection.East,
                _ => throw new InvalidOperationException("Unexpected north exit input."),
            };
            var move = CompleteMove(direction).Move;
            var target = northRoute.GetProperty("points")[inputIndex + 1];
            if (inputIndex < 17)
            {
                Assert.Null(move.CrossMapTransition);
                Assert.Equal(new MapPosition(target[0].GetInt32(), target[1].GetInt32()), move.Snapshot.PlayerPosition);
            }
            else
            {
                Assert.Equal(new MapPosition(target[0].GetInt32(), target[1].GetInt32()), move.CrossMapTransition!.Trigger);
                Assert.Equal(new MapPosition(9, 2), move.CrossMapTransition.Source);
            }
            Assert.Same(guardApplied.Receipt, move.Snapshot.MiddleTowerGuard);
        }
        var northArrival = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapId("map40"), northArrival.Map);
        Assert.Equal(new MapPosition(4, 30), northArrival.PlayerPosition);
        Assert.Equal((byte)1, session.PrivateOriginalMapPlayerLocomotion.OpaqueFacing);
        Assert.Equal(new OriginalMapAreaRecordIdentity("Map40s2_Areas", 1), northArrival.CurrentAreaDefinition.Identity);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedMap40Runtime(northArrival.CurrentRuntime));
        Assert.Equal("ms_map40_Entities", northArrival.CurrentRuntime.EntityPopulation.ResourceId);
        Assert.Empty(northArrival.CurrentRuntime.EntityPopulation.Records);
        Assert.Equal(OriginalMapRuntimeAdmission.Map40EntityProjectionDigest, northArrival.CurrentRuntime.EntityPopulation.ProjectionDigest);
        Assert.Equal((ushort)253, northArrival.WorkingLayout[4, 30]);
        Assert.Same(guardEndpoint.PalaceFirstVisit, northArrival.PalaceFirstVisit);
        Assert.Same(guardEndpoint.AstralAcceptance, northArrival.AstralAcceptance);
        Assert.Same(guardEndpoint.Receipt, northArrival.Receipt);
        Assert.Same(guardEndpoint.Zone601, northArrival.Zone601);
        Assert.Same(guardEndpoint.Sarah, northArrival.Sarah);
        Assert.Same(guardEndpoint.Entity142, northArrival.Entity142);
        Assert.Same(guardEndpoint.MessengerAcceptance, northArrival.MessengerAcceptance);
        Assert.Same(guardEndpoint.CastleGate, northArrival.CastleGate);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        Assert.Null(northArrival.MiddleTowerGuardPosition);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedBattle01Admission(northArrival.Definition.Battle01Admission));
        var battleRoute = extension.GetProperty("segments")[2];
        Assert.Equal("map40-entry-to-wildcard-battle-warp", battleRoute.GetProperty("id").GetString());
        Assert.Equal(28, battleRoute.GetProperty("inputs").GetArrayLength());
        Assert.Equal(29, battleRoute.GetProperty("points").GetArrayLength());
        for (int index = 0; index < 28; index++)
        {
            var point = battleRoute.GetProperty("points")[index];
            var beforeInput = session.PrivateOriginalMapSnapshot;
            var beforeAnimation = session.PrivateOriginalMapPlayerLocomotion;
            Assert.Equal(new MapPosition(point[0].GetInt32(), point[1].GetInt32()), beforeInput.PlayerPosition);
            var direction = battleRoute.GetProperty("inputs")[index].GetString() == "Up"
                ? ExplorationDirection.North : ExplorationDirection.East;
            var move = CompleteMove(direction).Move;
            var target = battleRoute.GetProperty("points")[index + 1];
            if (index < 27)
            {
                Assert.Null(move.Battle01Admission);
                Assert.Equal(new MapPosition(target[0].GetInt32(), target[1].GetInt32()), move.Snapshot.PlayerPosition);
                Assert.Null(move.Snapshot.LastCrossMapTransition);
            }
            else
            {
                var pending = Assert.IsType<PrivateOriginalBattle01PendingAdmission>(move.Battle01Admission);
                Assert.Equal(new MapPosition(target[0].GetInt32(), target[1].GetInt32()), pending.Trigger);
                Assert.Same(beforeInput, session.PrivateOriginalMapSnapshot);
                Assert.Same(beforeAnimation, session.PrivateOriginalMapPlayerLocomotion);
                Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
                Assert.Equal((byte)1, beforeAnimation.OpaqueFacing);
                Assert.Equal(new MapPosition(14, 13), beforeInput.PlayerPosition);
                Assert.Equal(new MapPosition(8, 18), pending.Definition.Destination);
                Assert.Throws<InvalidOperationException>(() => move.Traversal);
                Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMap(new(ExplorationDirection.North)));
                Assert.Throws<InvalidOperationException>(() => session.BeginPrivateOriginalMapPlayerLocomotion(new(ExplorationDirection.North)));
                session.RequestPrivateOriginalMapInteraction(beforeInput.SimulationStep);
                Assert.IsType<PrivateOriginalMapBattleBridgeRejected>(session.ApplyPrivateOriginalMapBattleBridge(
                    new RequestPrivateOriginalMapBattleBridgeCommand(bridge.Definition.Bridge, beforeInput.SimulationStep)));
                Assert.Same(beforeInput, session.PrivateOriginalMapSnapshot);
                Assert.Same(beforeAnimation, session.PrivateOriginalMapPlayerLocomotion);
                Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
                Assert.Same(pending, session.PrivateOriginalBattle01Admission);
            }
            Assert.Same(guardApplied.Receipt, session.PrivateOriginalMapSnapshot.MiddleTowerGuard);
            Assert.Equal(new MapId("map40"), session.PrivateOriginalMapSnapshot.Map);
        }
        var restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(GameSession.StartPrivateOriginalMap(
            new PrivateCanonicalMap3ImportReader(path), Request(AcceptedCanonicalDigest))).Session;
        Assert.Null(restarted.PrivateOriginalBattle01Admission);
        Assert.Equal(new MapId("map3"), restarted.PrivateOriginalMapSnapshot.Map);
        using var canonicalBodies = JsonDocument.Parse(File.ReadAllText(
            Environment.GetEnvironmentVariable("SF2_PRIVATE_CANONICAL_MAP_IMPORT")!));
        Assert.DoesNotContain(canonicalBodies.RootElement.GetProperty("resources").EnumerateObject()
                .SelectMany(collection => collection.Value.EnumerateArray()),
            resource => resource.GetProperty("id").GetString() is "Map21_EntityEvent0" or "cs_53EF4");
    }

    private static OriginalMapImportResult Admit(JsonObject document)
    {
        byte[] bytes = DocumentBytes(document);
        return PrivateCanonicalMap3ImportReader.AdmitSemanticDocumentForTests(bytes);
    }

    private static OriginalMapImportRequest Request(string expectedDigest) =>
        new(
            PrivateCanonicalMap3ImportReader.PackageId,
            ContentProfile.PrivateLocal,
            expectedDigest);

    private static OriginalMapImportResult AdmitProduction(
        byte[] bytes,
        OriginalMapImportRequest request)
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"sf2-private-map3-import-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllBytes(path, bytes);
            return new PrivateCanonicalMap3ImportReader(path).Admit(request);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static void AssertCode(
        OriginalMapImportResult result,
        OriginalMapImportFailureCode expected)
    {
        OriginalMapImportRejected rejected = Assert.IsType<OriginalMapImportRejected>(result);
        Assert.Equal(expected, rejected.Diagnostic.Code);
    }

    private static byte[] DocumentBytes(JsonObject document) =>
        Encoding.UTF8.GetBytes(
            document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");

    private static string Digest(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

    private static JsonObject SampleDocument()
    {
        int[] layoutWords = new int[WorkingMapLayout.WordCount];
        layoutWords[Index(32, 15)] = 0xC48F;
        layoutWords[Index(62, 0)] = 0x080E;
        layoutWords[Index(32, 16)] = 0x0C57;
        layoutWords[Index(41, 13)] = OriginalMapTraversal.CollisionMask;
        layoutWords[Index(4, 8)] = OriginalMapTraversal.CollisionMask;
        layoutWords[Index(3, 3)] = OriginalMapTraversal.LeftStairMask;
        layoutWords[Index(4, 4)] = OriginalMapTraversal.LeftStairMask;
        object[] maps = Enumerable.Range(0, 79)
            .Select(id => (object)new
            {
                id,
                sourceSymbol = id == 3 ? "Map03" : $"Map{id:00}",
                palette = 0,
                tilesets = id == 3
                    ? new[] { 0, 37, 43, 53, 66 }
                    : id is 19 or 20 ? new[] { 6, 23, 44, 53, 62 }
                    : id == 21 ? new[] { 6, 23, 44, 53, 8 }
                    : new[] { 0, 1, 2, 3, 4 },
                references = new
                {
                    blockset = id == 19 ? "Map19s0_Blocks" : "Map03s0_Blocks",
                    layout = id == 19 ? "Map19s1_Layout" : "Map03s1_Layout",
                    areaTable = id == 19 ? "Map19s2_Areas" : "Map03s2_Areas",
                    flagEventTable = id == 19 ? "Map19s3_FlagEvents" : "Map03s3_FlagEvents",
                    stepEventTable = id == 19 ? "Map19s4_StepEvents" : "Map03s4_StepEvents",
                    roofEventTable = id == 19 ? "Map19s5_RoofEvents" : "Map03s5_RoofEvents",
                    warpEventTable = id == 19 ? "Map19s6_WarpEvents" : "Map03s6_WarpEvents",
                    chestItemTable = id == 19 ? "Map19s7_ChestItems" : "Map03s7_ChestItems",
                    otherItemTable = id == 19 ? "Map19s8_OtherItems" : "Map03s8_OtherItems",
                    animationTable = id == 19 ? null : "Map03s9_Animations",
                    setupRoute = id == 19 ? "MapSetupRoute19" : "MapSetupRoute03",
                },
            })
            .ToArray();
        object setupReferences = new
        {
            entities = "ms_map3_Entities",
            entityEvents = "ms_map3_EntityEvents",
            zoneEvents = "ms_map3_ZoneEvents",
            areaDescriptions = "ms_map3_AreaDescriptions",
            itemEvents = "ms_map3_Section5",
            initFunction = "ms_map3_InitFunction",
        };
        object map19SetupReferences = new
        {
            entities = "ms_map19_Entities",
            entityEvents = "ms_map19_EntityEvents",
            zoneEvents = "ms_map19_ZoneEvents",
            areaDescriptions = "ms_map19_AreaDescriptions",
            itemEvents = "ms_map19_Section5",
            initFunction = "ms_map19_InitFunction",
        };
        JsonNode? node = JsonSerializer.SerializeToNode(new
        {
            schemaVersion = 1,
            id = PrivateCanonicalMap3ImportReader.PackageId,
            upstream = new
            {
                repository = PrivateCanonicalMap3ImportReader.CanonicalRepository,
                commit = PrivateCanonicalMap3ImportReader.CanonicalCommit,
            },
            romSha256 = PrivateCanonicalMap3ImportReader.CanonicalRomSha256,
            geometry = new
            {
                layoutWidth = 64,
                layoutHeight = 64,
                blockWidthTiles = 3,
                blockHeightTiles = 3,
                rawWordBits = 16,
                layoutBlockIndexMask = OriginalMapTraversal.LayoutBlockIndexMask,
                layoutFlagsMask = OriginalMapTraversal.LayoutFlagsMask,
            },
            table = new { },
            summary = new { },
            resourceCounts = new { },
            recordCounts = new { },
            setupFacts = new { },
            referenceFacts = new { },
            maps,
            resources = new
            {
                blocksets = new object[]
                {
                    new
                    {
                        id = "Map03s0_Blocks",
                        address = 1,
                        blocks = Enumerable.Range(0, SampleBlockCount).Select(_ => new int[9]).ToArray(),
                    },
                    new
                    {
                        id = "Map19s0_Blocks",
                        address = 101,
                        blocks = Enumerable.Range(0, 3).Select(_ => new int[9]).ToArray(),
                    },
                },
                layouts = new object[]
                {
                    new
                    {
                        id = "Map03s1_Layout",
                        address = 2,
                        width = 64,
                        height = 64,
                        words = layoutWords,
                    },
                    new
                    {
                        id = "Map19s1_Layout",
                        address = 102,
                        width = 64,
                        height = 64,
                        words = new int[WorkingMapLayout.WordCount],
                    },
                },
                areaTables = new object[]
                {
                    new
                    {
                        id = "Map03s2_Areas",
                        address = 3,
                        sourceKind = "areas",
                        records = new[]
                        {
                            AreaRecord(0, 0, 50, 31, secondForegroundY: 32),
                            AreaRecord(51, 0, 61, 9),
                            AreaRecord(51, 10, 61, 19),
                        },
                    },
                    new
                    {
                        id = "Map19s2_Areas",
                        address = 103,
                        sourceKind = "areas",
                        records = new[]
                        {
                            AreaRecord(0, 0, 40, 31, secondForegroundY: 32, defaultMusic: 38),
                        },
                    },
                },
                flagEventTables = new object[] {
                    new { id = "Map03s3_FlagEvents", records = new[] {
                        new { flag = 506, source = Point(23,23), size = new { width=1, height=2 }, destination=Point(28,22) },
                        new { flag = 506, source = Point(57,21), size = new { width=1, height=2 }, destination=Point(57,23) } } },
                    new { id = "Map19s3_FlagEvents" } },
                stepEventTables = new object[]
                {
                    new
                    {
                        id = "Map03s4_StepEvents",
                        address = 4,
                        sourceKind = "stepEvents",
                        records = new[]
                            {
                                new
                                {
                                    trigger = Point(4, 8),
                                    source = Point(62, 0),
                                    size = new { width = 1, height = 1 },
                                    destination = Point(4, 8),
                                },
                            }
                            .Concat(Enumerable.Range(1, 4)
                                .Select(index => new
                                {
                                    trigger = index == 3 ? Point(32, 15) : Point(index, 60),
                                    source = index == 3 ? Point(62, 0) : Point(index, 61),
                                    size = new { width = 1, height = 1 },
                                    destination = index == 3 ? Point(32, 15) : Point(index, 62),
                                }))
                            .Append(new
                            {
                                trigger = Point(41, 13),
                                source = Point(62, 0),
                                size = new { width = 1, height = 1 },
                                destination = Point(41, 13),
                            })
                            .ToArray(),
                    },
                    new { id = "Map19s4_StepEvents" },
                },
                roofEventTables = new object[]
                {
                    new
                    {
                        id = "Map03s5_RoofEvents",
                        address = 8,
                        sourceKind = "roofEvents",
                        records = RoofSourceRecords(),
                    },
                    new { id = "Map19s5_RoofEvents" },
                },
                warpEventTables = new object[]
                {
                    new
                    {
                        id = "Map03s6_WarpEvents",
                        address = 7,
                        sourceKind = "warpEvents",
                        records = WarpSourceRecords(),
                    },
                    new { id = "Map19s6_WarpEvents" },
                },
                itemTables = new object[]
                {
                    new { id = "Map03s7_ChestItems", records = new[] { new { x=6, y=18, flag=220, item=127 } } },
                    new { id = "Map03s8_OtherItems" },
                    new { id = "Map19s7_ChestItems" },
                    new { id = "Map19s8_OtherItems" },
                },
                animationTables = Resource("Map03s9_Animations"),
                setupRoutes = new object[]
                {
                    new
                    {
                        id = "MapSetupRoute03",
                        map = 3,
                        defaultSetup = "ms_map3",
                        flagVariants = new object[]
                        {
                            new { flag = 1, setup = "ms_map3_variant_a" },
                            new { flag = 2, setup = "ms_map3_variant_b" },
                            new { flag = 3, setup = "ms_map3_variant_c" },
                        },
                    },
                    new
                    {
                        id = "MapSetupRoute19",
                        map = 19,
                        defaultSetup = "ms_map19",
                        flagVariants = new object[]
                        {
                            new { flag = 501, setup = "ms_map19_flag501" },
                            new { flag = 609, setup = "ms_map19_flag609" },
                            new { flag = 506, setup = "ms_map19_flag506" },
                            new { flag = 507, setup = "ms_map19_flag507" },
                            new { flag = 543, setup = "ms_map19_flag543" },
                            new { flag = 982, setup = "ms_map19_flag982" },
                        },
                    },
                },
                setupDefinitions = new object[]
                {
                    new { id = "ms_map3", address = 5, references = setupReferences },
                    new { id = "ms_map3_variant_a" },
                    new { id = "ms_map3_variant_b" },
                    new { id = "ms_map3_variant_c" },
                    new { id = "ms_map19", address = 105, references = map19SetupReferences },
                    new { id = "ms_map19_flag501" },
                    new { id = "ms_map19_flag609" },
                    new { id = "ms_map19_flag506" },
                    new { id = "ms_map19_flag507" },
                    new { id = "ms_map19_flag543" },
                    new { id = "ms_map19_flag982" },
                },
                entityLists = new object[]
                {
                    new
                    {
                        id = "ms_map3_Entities",
                        address = 6,
                        records = EntitySourceRecords(),
                    },
                    new
                    {
                        id = "ms_map19_Entities",
                        address = 106,
                        records = Map19EntitySourceRecords(),
                    },
                },
                entityEventHandlers = new object[]
                {
                    new
                    {
                        id = "ms_map3_EntityEvents",
                        address = 331536,
                        kind = "table",
                        records = SarahEntityEventSourceRecords(),
                    },
                    new { id = "ms_map19_EntityEvents" },
                },
                zoneEventHandlers = new object[]
                {
                    new
                    {
                        id = "ms_map3_ZoneEvents",
                        address = 331084,
                        kind = "table",
                        records = ZoneSourceRecords(),
                    },
                    new { id = "ms_map19_ZoneEvents" },
                },
                itemEventHandlers = Resources("ms_map3_Section5", "ms_map19_Section5"),
                areaDescriptionHandlers = Resources(
                    "ms_map3_AreaDescriptions",
                    "ms_map19_AreaDescriptions"),
                initFunctions = Resources("ms_map3_InitFunction", "ms_map19_InitFunction"),
                standaloneScriptPrograms = new object[]
                {
                    new
                    {
                        id = "cs_513D6",
                        address = 332758,
                        path = "data/maps/entries/map03/mapsetups/scripts_1.asm",
                        kind = "cutscene",
                        operations = SarahBlockingOperations(),
                    },
                    new
                    {
                        id = "cs_5145C",
                        address = 332892,
                        path = "data/maps/entries/map03/mapsetups/scripts_1.asm",
                        kind = "cutscene",
                        operations = ZoneBlockingOperations(),
                    },
                    new
                    {
                        id = "cs_5148C",
                        address = 332940,
                        path = "data/maps/entries/map03/mapsetups/scripts_1.asm",
                        kind = "cutscene",
                        operations = AstralZonePositionOperations(),
                    },
                    new
                    {
                        id = "cs_5149A",
                        address = 332954,
                        path = "data/maps/entries/map03/mapsetups/scripts_1.asm",
                        kind = "cutscene",
                        operations = MessengerMainOperations(),
                    },
                    new
                    {
                        id = "cs_51614",
                        address = 333332,
                        path = "data/maps/entries/map03/mapsetups/scripts_1.asm",
                        kind = "cutscene",
                        operations = MessengerAcceptedOperations(),
                    },
                    new
                    {
                        id = "cs_51652",
                        address = 333394,
                        path = "data/maps/entries/map03/mapsetups/scripts_1.asm",
                        kind = "cutscene",
                        operations = CastleGateOperations(),
                    },
                },
                initSourcePrograms = Array.Empty<object>(),
            },
            runtimeQuestions = new[] { "unsupported-natural-runtime" },
        });
        JsonObject result = node!.AsObject();
        AddSyntheticRoyalPassage(result);
        AddSyntheticPalaceFirstVisit(result);
        AddSyntheticMiddleTower(result);
        AddSyntheticMap40(result);
        AddSyntheticMap57(result);
        JsonArray astralActors = ResourceArray(result, "entityLists").OfType<JsonObject>()
            .Single(row => row["id"]!.GetValue<string>() == "ms_map19_Entities")["records"]!.AsArray();
        astralActors[8] = astralActors[12]!.DeepClone();
        astralActors[12] = JsonSerializer.SerializeToNode(new
        {
            address = 338978, kind = "fixed", rawX = 16, rawY = 5, x = 16, y = 5,
            facing = 3, mapSprite = 209, actionValue = 286926,
        });
        JsonObject astralTable = ResourceArray(result, "entityEventHandlers").OfType<JsonObject>()
            .Single(row => row["id"]!.GetValue<string>() == "ms_map19_EntityEvents");
        astralTable["address"] = 0x52E02;
        astralTable["kind"] = "table";
        astralTable["records"] = JsonSerializer.SerializeToNode(Enumerable.Range(0, 14).Select(index => new
        {
            address = 0x52E02 + index * 4,
            kind = index == 13 ? "default" : "specific",
            relativeOffset = index == 12 ? 240 : 0,
            resolvedTargetAddress = index == 12 ? 0x52EF2 : 0x52E02,
            entity = index == 13 ? 253 : 128 + index,
            flags = 1,
        }));
        return result;
    }

    private static void AddSyntheticMiddleTower(JsonObject document)
    {
        JsonObject resources = document["resources"]!.AsObject();
        // Extend the public synthetic source shape, without importing private payloads.
        foreach ((string _, JsonNode? collection) in resources)
        {
            JsonArray rows = collection!.AsArray();
            foreach (JsonObject source in rows.OfType<JsonObject>().ToArray())
            {
                string id = source["id"]!.GetValue<string>();
                if (!id.Contains("Map20", StringComparison.Ordinal) && !id.Contains("map20", StringComparison.Ordinal) &&
                    id != "MapSetupRoute20") continue;
                rows.Add(JsonNode.Parse(source.ToJsonString()
                    .Replace("Map20", "Map21", StringComparison.Ordinal)
                    .Replace("map20", "map21", StringComparison.Ordinal)
                    .Replace("Route20", "Route21", StringComparison.Ordinal)));
            }
        }
        document["maps"]![21]!["references"] = JsonNode.Parse(document["maps"]![20]!["references"]!.ToJsonString()
            .Replace("Map20", "Map21", StringComparison.Ordinal).Replace("Route20", "Route21", StringComparison.Ordinal));
        PalaceResource(document, "setupRoutes", "MapSetupRoute21")["map"] = 21;
        PalaceResource(document, "areaTables", "Map21s2_Areas")["records"] = JsonSerializer.SerializeToNode(
            new[] { AreaRecord(0, 0, 11, 21, secondForegroundY: 22, defaultMusic: 38) });
        PalaceResource(document, "entityLists", "ms_map21_Entities")["address"] = 343672;
        var guardEvents = PalaceResource(document, "entityEventHandlers", "ms_map21_EntityEvents");
        guardEvents["address"] = 343698;
        guardEvents["kind"] = "table";
        guardEvents["records"] = JsonSerializer.SerializeToNode(new[]
        {
            new { address = 343698, kind = "specific", relativeOffset = 28, resolvedTargetAddress = 343726, entity = 128, flags = 3 },
            new { address = 343702, kind = "default", relativeOffset = 96, resolvedTargetAddress = 343794, entity = 253, flags = 0 },
        });
        PalaceResource(document, "entityLists", "ms_map21_Entities")["records"] = JsonSerializer.SerializeToNode(new[]
        {
            new { address = 343672, kind = "fixed", rawX = 5, rawY = 16, x = 5, y = 16,
                facing = 3, mapSprite = 206, actionValue = 286926 },
        });
        JsonArray words = PalaceResource(document, "layouts", "Map20s1_Layout")["words"]!.AsArray();
        words[Index(4, 37)] = OriginalMapTraversal.LeftStairMask;
        words[Index(3, 36)] = OriginalMapTraversal.LeftStairMask | 0x1000;
        PalaceResource(document, "warpEventTables", "Map20s6_WarpEvents")["records"]![3] =
            JsonSerializer.SerializeToNode(WarpRecord(3, 36, 21, 3, 16, 0));
    }

    private static void AddSyntheticMap57(JsonObject document)
    {
        foreach (string collection in new[] { "blocksets", "layouts", "areaTables", "flagEventTables",
            "stepEventTables", "roofEventTables", "warpEventTables", "itemTables" })
        {
            var rows = document["resources"]![collection]!.AsArray();
            foreach (var source in rows.OfType<JsonObject>().ToArray())
                if (source["id"]!.GetValue<string>().StartsWith("Map21s", StringComparison.Ordinal))
                    rows.Add(JsonNode.Parse(source.ToJsonString().Replace("Map21", "Map57", StringComparison.Ordinal)));
        }
        var map = Map(document, 57);
        map["references"] = JsonNode.Parse(Map(document, 21)["references"]!.ToJsonString().Replace("Map21", "Map57", StringComparison.Ordinal));
        map["references"]!["setupRoute"] = null;
        map["references"]!["animationTable"] = null;
        map["palette"] = 8;
        map["tilesets"] = JsonSerializer.SerializeToNode(new[] { 94, 98, 99, 255, 255 });
        var blocks = PalaceResource(document, "blocksets", "Map57s0_Blocks");
        blocks["address"] = 755240;
        blocks["blocks"] = JsonSerializer.SerializeToNode(Enumerable.Range(0, 120).Select(_ => new ushort[9]));
        var layout = PalaceResource(document, "layouts", "Map57s1_Layout");
        layout["address"] = 756002;
        layout["words"] = JsonSerializer.SerializeToNode(new ushort[WorkingMapLayout.WordCount]);
        var areas = PalaceResource(document, "areaTables", "Map57s2_Areas");
        areas["address"] = 755172;
        areas["records"] = JsonSerializer.SerializeToNode(new[] { AreaRecord(0, 0, 15, 19, defaultMusic: 34) });
        var area = areas["records"]![0]!;
        area["secondLayerForegroundStart"]!["x"] = 0;
        area["secondLayerForegroundStart"]!["y"] = 0;
        area["secondLayerBackgroundStart"]!["x"] = 0;
        area["secondLayerBackgroundStart"]!["y"] = 0;
        area["mainLayerType"] = 255;
        var warps = PalaceResource(document, "warpEventTables", "Map40s6_WarpEvents");
        warps["address"] = 723644;
        warps["records"] = JsonSerializer.SerializeToNode(new[] {
            WarpRecord(255, 12, 57, 8, 18, 1), WarpRecord(255, 31, 21, 9, 2, 3) });
    }

    private static void AddSyntheticMap40(JsonObject document)
    {
        var resources = document["resources"]!.AsObject();
        foreach ((string _, JsonNode? collection) in resources)
        {
            var rows = collection!.AsArray();
            foreach (var source in rows.OfType<JsonObject>().ToArray())
            {
                string id = source["id"]!.GetValue<string>();
                if (!id.Contains("Map21", StringComparison.Ordinal) && !id.Contains("map21", StringComparison.Ordinal) &&
                    id != "MapSetupRoute21") continue;
                rows.Add(JsonNode.Parse(source.ToJsonString().Replace("Map21", "Map40", StringComparison.Ordinal)
                    .Replace("map21", "map40", StringComparison.Ordinal).Replace("Route21", "Route40", StringComparison.Ordinal)));
            }
        }
        Map(document, 40)["references"] = JsonNode.Parse(Map(document, 21)["references"]!.ToJsonString()
            .Replace("Map21", "Map40", StringComparison.Ordinal).Replace("Route21", "Route40", StringComparison.Ordinal));
        Map(document, 40)["palette"] = 3;
        Map(document, 40)["tilesets"] = JsonSerializer.SerializeToNode(new[] {94, 95, 96, 97, 58});
        var route = PalaceResource(document, "setupRoutes", "MapSetupRoute40");
        route["map"] = 40;
        route["flagVariants"] = JsonSerializer.SerializeToNode(new[] {
            new { flag = 506, setup = "ms_map40_flag506" }, new { flag = 507, setup = "ms_map40" } });
        PalaceResource(document, "setupDefinitions", "ms_map40")["address"] = 343904;
        PalaceResource(document, "initFunctions", "ms_map40_InitFunction")["address"] = 344010;
        var entities = PalaceResource(document, "entityLists", "ms_map40_Entities");
        entities["address"] = 343952;
        entities["records"] = new JsonArray();
        PalaceResource(document, "blocksets", "Map40s0_Blocks")["address"] = 723670;
        PalaceResource(document, "layouts", "Map40s1_Layout")["address"] = 725526;
        var areas = PalaceResource(document, "areaTables", "Map40s2_Areas");
        areas["address"] = 723606;
        areas["records"] = JsonSerializer.SerializeToNode(new[] { AreaRecord(0, 0, 31, 31, defaultMusic: 38) });
        var area = areas["records"]![0]!;
        area["secondLayerForegroundStart"]!["y"] = 0;
        area["secondLayerBackgroundStart"]!["y"] = 32;
        area["secondLayerParallax"]!["x"] = 128;
        area["secondLayerParallax"]!["y"] = 128;
        area["mainLayerType"] = 255;
        var warps = PalaceResource(document, "warpEventTables", "Map21s6_WarpEvents");
        warps["address"] = 680440;
        warps["records"] = JsonSerializer.SerializeToNode(new[] { WarpRecord(1, 1, 20, 1, 1, 0), WarpRecord(9, 1, 40, 4, 30, 1) });
        PalaceResource(document, "layouts", "Map21s1_Layout")["words"]![Index(9, 1)] = 0x1007;
        var blocks = PalaceResource(document, "blocksets", "Map21s0_Blocks")["blocks"]!.AsArray();
        while (blocks.Count < 8) blocks.Add(JsonSerializer.SerializeToNode(new ushort[9]));
    }

    private static void AddSyntheticRoyalPassage(JsonObject document)
    {
        JsonObject resources = document["resources"]!.AsObject();
        // Clone only the already synthetic Map19 records, never original private payloads.
        foreach ((string _, JsonNode? collection) in resources)
        {
            JsonArray rows = collection!.AsArray();
            foreach (JsonObject source in rows.OfType<JsonObject>().ToArray())
            {
                string id = source["id"]!.GetValue<string>();
                if (!id.Contains("Map19", StringComparison.Ordinal) && !id.Contains("map19", StringComparison.Ordinal) &&
                    id != "MapSetupRoute19")
                {
                    continue;
                }

                JsonObject clone = JsonNode.Parse(source.ToJsonString()
                    .Replace("Map19", "Map20", StringComparison.Ordinal)
                    .Replace("map19", "map20", StringComparison.Ordinal)
                    .Replace("Route19", "Route20", StringComparison.Ordinal))!.AsObject();
                rows.Add(clone);
            }
        }

        JsonObject map20 = document["maps"]!.AsArray()[20]!.AsObject();
        map20["references"] = JsonNode.Parse(document["maps"]!.AsArray()[19]!["references"]!.ToJsonString()
            .Replace("Map19", "Map20", StringComparison.Ordinal)
            .Replace("Route19", "Route20", StringComparison.Ordinal));
        JsonObject Find(string collection, string id) => resources[collection]!.AsArray()
            .OfType<JsonObject>().Single(row => row["id"]!.GetValue<string>() == id);
        JsonObject setup = Find("setupRoutes", "MapSetupRoute20");
        setup["map"] = 20;
        setup["flagVariants"] = JsonSerializer.SerializeToNode(new[] { 501, 609, 506, 543 }
            .Select(flag => new { flag, setup = $"ms_map20_flag{flag}" }));
        JsonArray entityRows = Find("entityLists", "ms_map20_Entities")["records"]!.AsArray();
        JsonNode walking = entityRows[9]!.DeepClone();
        while (entityRows.Count > 7) entityRows.RemoveAt(7);
        entityRows.Add(walking);
        Find("areaTables", "Map20s2_Areas")["records"]!.AsArray()[0]!["mainLayerEnd"]!["y"] = 45;
        JsonArray words = Find("layouts", "Map19s1_Layout")["words"]!.AsArray();
        words[Index(22, 4)] = OriginalMapTraversal.RightStairMask;
        words[Index(23, 3)] = OriginalMapTraversal.RightStairMask | 0x1000;
        words[Index(5, 3)] = OriginalMapTraversal.RightStairMask;
        words[Index(6, 2)] = OriginalMapTraversal.RightStairMask | 0x1000;
        words[Index(29, 15)] = 0x1400;
        words[Index(25, 13)] = 0x1400;
        JsonObject warp = Find("warpEventTables", "Map19s6_WarpEvents");
        warp["address"] = 673358;
        warp["sourceKind"] = "warpEvents";
        warp["records"] = JsonSerializer.SerializeToNode(Enumerable.Range(0, 7).Select(index => new
        {
            trigger = Point(index == 0 ? 6 : index == 1 ? 23 : 60, index == 0 ? 2 : index == 1 ? 3 : index),
            scrollMode = 0,
            retainsCoordinates = false,
            scrollDirection = (int?)null,
            targetMap = 20,
            destination = Point(index == 0 ? 6 : 23, 37),
            facing = index == 0 ? 0 : 3,
            reserved = 0,
        }));
        JsonObject zone = Find("zoneEventHandlers", "ms_map19_ZoneEvents");
        zone["address"] = 339348;
        zone["kind"] = "table";
        zone["records"] = JsonSerializer.SerializeToNode(new[]
        {
            new { address = 339348, kind = "default", relativeOffset = 32,
                resolvedTargetAddress = 339380, x = 253, y = 0 },
        });
    }

    [Theory]
    [InlineData("targetMap", 21)]
    [InlineData("facing", 1)]
    [InlineData("scrollMode", 16)]
    [InlineData("reserved", 1)]
    public void RoyalWarpRejectsAlteredSourceOperands(string field, int value)
    {
        JsonObject document = SampleDocument();
        JsonObject table = document["resources"]!["warpEventTables"]!.AsArray().OfType<JsonObject>()
            .Single(row => row["id"]!.GetValue<string>() == "Map19s6_WarpEvents");
        table["records"]!.AsArray()[1]![field] = value;
        AssertCode(PrivateCanonicalMap3ImportReader.AdmitSemanticDocumentForTests(DocumentBytes(document)),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Theory]
    [InlineData("trigger")]
    [InlineData("destination")]
    [InlineData("targetMap")]
    [InlineData("facing")]
    [InlineData("scrollMode")]
    [InlineData("retainsCoordinates")]
    [InlineData("scrollDirection")]
    [InlineData("reserved")]
    [InlineData("record-order")]
    public void WestTowerEntryRejectsChangedWarpMeaningBeforeRuntimeAdmission(string drift)
    {
        JsonObject document = SampleDocument();
        JsonArray records = ResourceArray(document, "warpEventTables").OfType<JsonObject>()
            .Single(row => row["id"]!.GetValue<string>() == "Map19s6_WarpEvents")["records"]!.AsArray();
        JsonObject warp = records[0]!.AsObject();
        switch (drift)
        {
            case "trigger": warp["trigger"]!["x"] = 7; break;
            case "destination": warp["destination"]!["y"] = 38; break;
            case "targetMap": warp["targetMap"] = 21; break;
            case "facing": warp["facing"] = 3; break;
            case "scrollMode": warp["scrollMode"] = 16; break;
            case "retainsCoordinates": warp["retainsCoordinates"] = true; break;
            case "scrollDirection": warp["scrollDirection"] = 0; break;
            case "reserved": warp["reserved"] = 1; break;
            case "record-order": records[0] = records[1]!.DeepClone(); break;
        }
        // Bypass only the outer fixed digest, so these checks exercise consumed source operands.
        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void WestTowerEntryBindsTheAcceptedStaticRouteAndDistinctRoyalRecord()
    {
        var definition = Assert.IsType<OriginalMapImportAccepted>(Admit(SampleDocument())).Definition;
        var west = Assert.IsType<OriginalMapCrossMapTransitionDefinition>(definition.WestTowerMap20Transition);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedWestTowerMap20Transition(west));
        Assert.NotEqual(west.Identity, definition.RoyalMap20Transition!.Identity);
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json"));
        using JsonDocument fixture = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement segments = fixture.RootElement.GetProperty("static").GetProperty("routeGraph").GetProperty("segments");
        JsonElement edge = segments.EnumerateArray().Single(row => row.GetProperty("id").GetString() == "map19-to-map20-west-tower-warp");
        JsonElement route = segments.EnumerateArray().Single(row => row.GetProperty("id").GetString() == "map19-astral-to-west-tower-warp");
        Assert.Equal(15, route.GetProperty("inputs").GetArrayLength());
        Assert.Equal("Right", route.GetProperty("inputs")[14].GetString());
        Assert.Equal(west.AdmittedApproach.X, route.GetProperty("points")[14][0].GetInt32());
        Assert.Equal(west.AdmittedApproach.Y, route.GetProperty("points")[14][1].GetInt32());
        Assert.Equal(west.AdmittedTrigger.X, edge.GetProperty("from").GetProperty("point")[0].GetInt32());
        Assert.Equal(west.AdmittedTrigger.Y, edge.GetProperty("from").GetProperty("point")[1].GetInt32());
        Assert.Equal(20, edge.GetProperty("to").GetProperty("map").GetInt32());
        Assert.Equal(west.Destination.X, edge.GetProperty("to").GetProperty("point")[0].GetInt32());
        Assert.Equal(west.Destination.Y, edge.GetProperty("to").GetProperty("point")[1].GetInt32());
        Assert.Equal("RIGHT", edge.GetProperty("to").GetProperty("facing").GetString());
        Assert.Equal((byte)0, west.DestinationOpaqueFacing);
    }

    [Fact]
    public void RoyalWarpRejectsChangedDefaultZoneTarget()
    {
        JsonObject document = SampleDocument();
        JsonObject table = document["resources"]!["zoneEventHandlers"]!.AsArray().OfType<JsonObject>()
            .Single(row => row["id"]!.GetValue<string>() == "ms_map19_ZoneEvents");
        table["records"]!.AsArray()[0]!["resolvedTargetAddress"] = 339382;
        AssertCode(PrivateCanonicalMap3ImportReader.AdmitSemanticDocumentForTests(DocumentBytes(document)),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Theory]
    [InlineData("trigger/x", 22)]
    [InlineData("trigger/y", 36)]
    [InlineData("destination/x", 22)]
    [InlineData("destination/y", 4)]
    [InlineData("targetMap", 20)]
    [InlineData("facing", 3)]
    [InlineData("scrollMode", 16)]
    [InlineData("reserved", 1)]
    public void RoyalReturnRejectsAlteredExactWarpOperands(string field, int value)
    {
        JsonObject document = SampleDocument();
        JsonNode row = PalaceResource(document, "warpEventTables", "Map20s6_WarpEvents")["records"]![4]!;
        string[] parts = field.Split('/');
        if (parts.Length == 2) row[parts[0]]![parts[1]] = value;
        else row[field] = value;
        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Theory]
    [InlineData("address")]
    [InlineData("count")]
    [InlineData("retains")]
    [InlineData("scrollDirection")]
    [InlineData("missing")]
    [InlineData("reordered")]
    public void RoyalReturnRejectsChangedTableIdentityAndRecordShape(string drift)
    {
        JsonObject document = SampleDocument();
        JsonObject table = PalaceResource(document, "warpEventTables", "Map20s6_WarpEvents");
        JsonArray rows = table["records"]!.AsArray();
        switch (drift)
        {
            case "address": table["address"] = 676827; break;
            case "count": rows.RemoveAt(10); break;
            case "retains": rows[4]!["retainsCoordinates"] = true; break;
            case "scrollDirection": rows[4]!["scrollDirection"] = 0; break;
            case "missing": rows[4]!.AsObject().Remove("facing"); break;
            case "reordered": rows[4] = rows[3]!.DeepClone(); break;
        }
        AssertCode(Admit(document), drift == "missing"
            ? OriginalMapImportFailureCode.InvalidDocument
            : OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void RoyalReturnRawFacingAgreesWithTheSourceDerivedGraphAnnotation()
    {
        var definition = Assert.IsType<OriginalMapImportAccepted>(Admit(SampleDocument())).Definition;
        var transition = Assert.IsType<OriginalMapCrossMapTransitionDefinition>(definition.RoyalReturnMap19Transition);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedRoyalReturnMap19Transition(transition));
        Assert.Equal((byte)2, transition.DestinationOpaqueFacing);
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json"));
        using JsonDocument fixture = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement source = fixture.RootElement.GetProperty("static");
        Assert.Equal("LEFT", source.GetProperty("warps").GetProperty("map20Royal")[0].GetProperty("facing").GetString());
        JsonElement graphWarp = source.GetProperty("routeGraph").GetProperty("segments").EnumerateArray()
            .Single(row => row.GetProperty("id").GetString() == "map20-to-map19-royal-return");
        Assert.Equal(source.GetProperty("warps").GetProperty("map20Royal")[0].GetProperty("facing").GetString(),
            graphWarp.GetProperty("to").GetProperty("facing").GetString());
    }

    [Fact]
    public void AstralControlledResultConsumesTheAcceptedStaticContractWithoutImportingScriptBodies()
    {
        var document = SampleDocument();
        var definition = Assert.IsType<OriginalMapImportAccepted>(Admit(document)).Definition.AstralAcceptance!;
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json"));
        using JsonDocument fixture = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement source = fixture.RootElement.GetProperty("static");
        var interaction = source.GetProperty("routeGraph").GetProperty("segments").EnumerateArray()
            .Single(row => row.GetProperty("id").GetString() == "map19-astral-prompt-and-acceptance");
        Assert.Equal(interaction.GetProperty("entity").GetInt32(), definition.Actor.Identity.OneBasedRecordOrdinal + 127);
        Assert.Equal(new MapPosition(interaction.GetProperty("player")[0].GetInt32(),
            interaction.GetProperty("player")[1].GetInt32()), definition.InteractionPosition);
        Assert.Equal("UP", interaction.GetProperty("facing").GetString());
        Assert.Equal((byte)1, definition.InteractionOpaqueFacing);
        Assert.Equal(interaction.GetProperty("programs").EnumerateArray().Select(row => row.GetString()),
            new[] { definition.PromptProgramIdentity, definition.AcceptanceProgramIdentity });
        Assert.Equal(interaction.GetProperty("setFlags").EnumerateArray().Select(row => row.GetInt32()),
            new[] { definition.ProgramCompletionFlag, definition.HandlerCompletionFlag });
        var prompt = source.GetProperty("programs").GetProperty(definition.PromptProgramIdentity)
            .GetProperty("operations").EnumerateArray().Select(row => row.GetString()).ToArray();
        int promptIndex = Array.IndexOf(prompt, "yesNo");
        int branchIndex = Array.IndexOf(prompt, "jumpIfFlagSet 89,cs_52F40");
        Assert.True(promptIndex >= 0 && branchIndex >= 0);
        Assert.True(promptIndex < branchIndex);
        var accepted = source.GetProperty("programs").GetProperty(definition.AcceptanceProgramIdentity);
        Assert.Contains($"setPos 140,{definition.AcceptedActorEndpoint.X},{definition.AcceptedActorEndpoint.Y},LEFT",
            accepted.GetProperty("operations").EnumerateArray().Select(row => row.GetString()));
        Assert.Equal((byte)2, definition.AcceptedActorOpaqueFacing);
        Assert.Equal(new[] { definition.ProgramCompletionFlag },
            accepted.GetProperty("semantics").GetProperty("setFlags").EnumerateArray().Select(row => row.GetInt32()));
        Assert.DoesNotContain(ResourceArray(document, "standaloneScriptPrograms").OfType<JsonObject>(),
            row => row["id"]!.GetValue<string>() == definition.AcceptanceProgramIdentity);
    }

    [Theory]
    [InlineData("entity", 139)]
    [InlineData("flags", 0)]
    [InlineData("relativeOffset", 244)]
    public void AstralRejectsAnUnboundEventEntry(string field, int value)
    {
        var document = SampleDocument();
        var handler = ResourceArray(document, "entityEventHandlers").OfType<JsonObject>()
            .Single(row => row["id"]!.GetValue<string>() == "ms_map19_EntityEvents");
        handler["records"]![12]![field] = value;
        Assert.IsType<OriginalMapImportRejected>(Admit(document));
    }

    [Theory]
    [InlineData(0, "#$22803781,((ENTITY_DATA-$1000000)).w")]
    [InlineData(2, "608")]
    [InlineData(3, "ms_map20_flag501_InitFunction")]
    [InlineData(5, "606")]
    [InlineData(8, "508")]
    public void PalaceInitRejectsEntryAndBranchOperandDrift(int index, string operand)
    {
        JsonObject document = SampleDocument();
        PalaceResource(document, "initFunctions", "ms_map20_InitFunction")["operations"]![index]!["operandText"] = operand;
        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Theory]
    [InlineData("cs_53996", 1, "ALLY_BOWIE,23,38,DOWN")]
    [InlineData("cs_53996", 16, "2")]
    [InlineData("cs_53996", 69, "2")]
    [InlineData("cs_53996", 77, "2")]
    [InlineData("cs_53B60", 0, "131")]
    public void PalaceResultRejectsConsumedEndpointAndTailDrift(string program, int index, string operand)
    {
        JsonObject document = SampleDocument();
        PalaceResource(document, "initSourcePrograms", program)["operations"]![index]!["operandText"] = operand;
        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Theory]
    [InlineData("count")]
    [InlineData("branch")]
    [InlineData("tail")]
    [InlineData("malformed")]
    [InlineData("null")]
    public void PalaceSourceShapeRejectsTruncationBranchMetadataAndMalformedTokens(string drift)
    {
        JsonObject document = SampleDocument();
        JsonArray main = PalaceResource(document, "initSourcePrograms", "cs_53996")["operations"]!.AsArray();
        switch (drift)
        {
            case "count": main.RemoveAt(112); break;
            case "branch": PalaceResource(document, "initFunctions", "ms_map20_InitFunction")["operations"]![3]!["localBranchTargetIndex"] = 8; break;
            case "tail": PalaceResource(document, "initSourcePrograms", "cs_53B60")["operations"]![1]!["opcode"] = "csWait"; break;
            case "malformed": main[40]!["opcode"] = 7; break;
            case "null": main[40]!["operandText"] = null; break;
        }

        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void PalaceProjectionBindsUnconsumedTokensWithoutClaimingSourceTextIdentity()
    {
        JsonObject document = SampleDocument();
        var before = Assert.IsType<OriginalMapImportAccepted>(Admit(document)).Definition.PalaceFirstVisit!;
        PalaceResource(document, "initSourcePrograms", "cs_53996")["operations"]![40]!["operandText"] = "1";
        var after = Assert.IsType<OriginalMapImportAccepted>(Admit(document)).Definition.PalaceFirstVisit!;
        Assert.NotEqual(before.ScriptProjectionSha256, after.ScriptProjectionSha256);
        // Synthetic source-shape tests are not the pinned production canonical input.
        Assert.NotEqual(OriginalMapRuntimeAdmission.PalaceScriptProjectionSha256, before.ScriptProjectionSha256);
        Assert.NotEqual(OriginalMapRuntimeAdmission.PalaceSourceControlEffectSha256, before.ScriptProjectionSha256);
    }

    [Theory]
    [InlineData("targetMap", 20)]
    [InlineData("trigger/x", 4)]
    [InlineData("trigger/y", 37)]
    [InlineData("destination/x", 4)]
    [InlineData("destination/y", 15)]
    [InlineData("facing", 1)]
    [InlineData("scrollMode", 16)]
    [InlineData("reserved", 1)]
    public void MiddleTowerRejectsAlteredExactWarpOperands(string field, int value)
    {
        JsonObject document = SampleDocument();
        JsonNode row = PalaceResource(document, "warpEventTables", "Map20s6_WarpEvents")["records"]![3]!;
        string[] parts = field.Split('/');
        if (parts.Length == 2) row[parts[0]]![parts[1]] = value;
        else row[field] = value;
        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Theory]
    [InlineData("layout")]
    [InlineData("setup")]
    [InlineData("entities")]
    [InlineData("destination")]
    [InlineData("retains")]
    [InlineData("scrollDirection")]
    [InlineData("record")]
    public void MiddleTowerRejectsChangedRuntimeJoinsAndWarpShape(string drift)
    {
        var document = SampleDocument();
        var row = PalaceResource(document, "warpEventTables", "Map20s6_WarpEvents")["records"]![3]!;
        switch (drift)
        {
            case "layout": document["maps"]![21]!["references"]!["layout"] = "Map20s1_Layout"; break;
            case "setup": PalaceResource(document, "setupRoutes", "MapSetupRoute21")["defaultSetup"] = "ms_map20"; break;
            case "entities": PalaceResource(document, "setupDefinitions", "ms_map21")["references"]!["entities"] = "ms_map20_Entities"; break;
            case "destination": PalaceResource(document, "layouts", "Map21s1_Layout")["words"]![Index(3, 16)] = OriginalMapTraversal.CollisionMask; break;
            case "retains": row["retainsCoordinates"] = true; break;
            case "scrollDirection": row["scrollDirection"] = 0; break;
            case "record": PalaceResource(document, "warpEventTables", "Map20s6_WarpEvents")["records"]![3] =
                PalaceResource(document, "warpEventTables", "Map20s6_WarpEvents")["records"]![4]!.DeepClone(); break;
        }
        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void MiddleTowerBindsAcceptedStaticDestinationAndIndependentVisualSelection()
    {
        var definition = Assert.IsType<OriginalMapImportAccepted>(Admit(SampleDocument())).Definition;
        var transition = definition.MiddleTowerMap21Transition!;
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedMiddleTowerMap21Transition(transition));
        Assert.NotEqual(transition.Identity, definition.RoyalReturnMap19Transition!.Identity);
        var runtime = definition.RuntimeCatalog.Resolve(new("map21"));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedMap21VisualResourceSelection(runtime.VisualResourceSelection));
        Assert.False(OriginalMapRuntimeAdmission.HasExactAcceptedCastleVisualResourceSelection(runtime.VisualResourceSelection));
        Assert.Equal(new MapSetupId("ms_map21"), runtime.SelectedSetup);
        Assert.Equal("ms_map21_InitFunction", runtime.SelectedInitIdentity);
        Assert.Single(runtime.EntityPopulation.Records);
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json"));
        using var fixture = JsonDocument.Parse(File.ReadAllText(path));
        var edge = fixture.RootElement.GetProperty("static").GetProperty("routeGraph").GetProperty("segments")
            .EnumerateArray().Single(row => row.GetProperty("id").GetString() == "map20-to-map21-middle-tower-warp");
        Assert.Equal(transition.AdmittedTrigger.X, edge.GetProperty("from").GetProperty("point")[0].GetInt32());
        Assert.Equal(transition.AdmittedTrigger.Y, edge.GetProperty("from").GetProperty("point")[1].GetInt32());
        Assert.Equal(21, edge.GetProperty("to").GetProperty("map").GetInt32());
        Assert.Equal(transition.Destination.X, edge.GetProperty("to").GetProperty("point")[0].GetInt32());
        Assert.Equal(transition.Destination.Y, edge.GetProperty("to").GetProperty("point")[1].GetInt32());
        Assert.Equal("RIGHT", edge.GetProperty("to").GetProperty("facing").GetString());
    }

    [Theory]
    [InlineData(0, "address", 343702)]
    [InlineData(0, "relativeOffset", 29)]
    [InlineData(0, "resolvedTargetAddress", 343727)]
    [InlineData(0, "entity", 129)]
    [InlineData(0, "flags", 1)]
    [InlineData(1, "address", 343698)]
    [InlineData(1, "relativeOffset", 95)]
    [InlineData(1, "resolvedTargetAddress", 343795)]
    [InlineData(1, "entity", 128)]
    [InlineData(1, "flags", 3)]
    public void MiddleTowerGuardRejectsExactEventOperandDrift(int row, string field, int value)
    {
        var document = SampleDocument();
        PalaceResource(document, "entityEventHandlers", "ms_map21_EntityEvents")["records"]![row]![field] = value;
        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Theory]
    [InlineData("setup")]
    [InlineData("table-address")]
    [InlineData("table-kind")]
    [InlineData("missing-row")]
    [InlineData("row-kind")]
    [InlineData("unknown-field")]
    [InlineData("actor-address")]
    [InlineData("actor-x")]
    [InlineData("actor-facing")]
    [InlineData("actor-sprite")]
    [InlineData("actor-action")]
    public void MiddleTowerGuardRejectsSourceJoinAndActorDrift(string drift)
    {
        var document = SampleDocument();
        var table = PalaceResource(document, "entityEventHandlers", "ms_map21_EntityEvents");
        var actor = PalaceResource(document, "entityLists", "ms_map21_Entities")["records"]![0]!;
        switch (drift)
        {
            case "setup": PalaceResource(document, "setupDefinitions", "ms_map21")["references"]!["entityEvents"] = "ms_map20_EntityEvents"; break;
            case "table-address": table["address"] = 343700; break;
            case "table-kind": table["kind"] = "function"; break;
            case "missing-row": table["records"]!.AsArray().RemoveAt(1); break;
            case "row-kind": table["records"]![0]!["kind"] = "default"; break;
            case "unknown-field": table["records"]![0]!["unexpected"] = 0; break;
            case "actor-address": actor["address"] = 343680; break;
            case "actor-x": actor["rawX"] = 6; actor["x"] = 6; break;
            case "actor-facing": actor["facing"] = 0; break;
            case "actor-sprite": actor["mapSprite"] = 207; break;
            case "actor-action": actor["actionValue"] = 286925; break;
        }
        AssertCode(Admit(document), drift == "unknown-field"
            ? OriginalMapImportFailureCode.InvalidDocument : OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void MiddleTowerGuardBindsTheCompiledH2ContractWithoutInventingCanonicalScriptBodies()
    {
        var definition = Assert.IsType<OriginalMapImportAccepted>(Admit(SampleDocument())).Definition;
        var guard = Assert.IsType<OriginalMapMiddleTowerGuardDefinition>(definition.MiddleTowerGuard);
        Assert.Same(definition.RuntimeCatalog.Resolve(new("map21")).EntityPopulation.Records[0], guard.Actor);
        Assert.Equal("Map21_EntityEvent0", guard.HandlerIdentity);
        Assert.Equal(343726, guard.HandlerAddress);
        Assert.Equal(579, guard.TextId);
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json"));
        using var fixture = JsonDocument.Parse(File.ReadAllText(path));
        var program = fixture.RootElement.GetProperty("static").GetProperty("programs").GetProperty(guard.ProgramIdentity);
        Assert.Equal(guard.ProgramAddress, program.GetProperty("address").GetInt32());
        Assert.Equal(guard.ProgramControlEffectSha256, program.GetProperty("controlEffectSha256").GetString());
        Assert.Equal(new[] { "entityActionsWait 128", "moveRight 1", "endActions", "setFacing 135,DOWN", "setStoryFlag 1", "csc_end" },
            program.GetProperty("operations").EnumerateArray().Select(op => op.GetString()));
        Assert.Equal(guard.HandlerCompletionFlag, program.GetProperty("semantics").GetProperty("handlerSetFlag").GetInt32());
        Assert.Equal(guard.ProgramStoryFlag, program.GetProperty("semantics").GetProperty("setStoryFlag").GetInt32());
        Assert.Equal(guard.ProgramCompletionFlag, program.GetProperty("semantics").GetProperty("battleUnlockFlag").GetInt32());
        // No player endpoint/facing is inferred from the navigation segment or entity-135 operation.
    }

    [Theory]
    [InlineData("palette")]
    [InlineData("slots")]
    [InlineData("source")]
    [InlineData("layout-address")]
    [InlineData("setup-address")]
    [InlineData("init-address")]
    [InlineData("entity-address")]
    [InlineData("variant-order")]
    [InlineData("variant-target")]
    [InlineData("entity-join")]
    [InlineData("warp-facing")]
    [InlineData("warp-target")]
    [InlineData("warp-scroll")]
    [InlineData("warp-reserved")]
    [InlineData("warp-trigger")]
    [InlineData("warp-destination")]
    [InlineData("warp-count")]
    [InlineData("destination-blocked")]
    public void NorthMap40RejectsCanonicalSelectionRuntimeAndWarpDrift(string drift)
    {
        var document = SampleDocument();
        var warp = PalaceResource(document, "warpEventTables", "Map21s6_WarpEvents")["records"]![1]!;
        switch (drift)
        {
            case "palette": Map(document, 40)["palette"] = 0; break;
            case "slots": Map(document, 40)["tilesets"]![4] = 8; break;
            case "source": Map(document, 40)["sourceSymbol"] = "Map21"; break;
            case "layout-address": PalaceResource(document, "layouts", "Map40s1_Layout")["address"] = 1; break;
            case "setup-address": PalaceResource(document, "setupDefinitions", "ms_map40")["address"] = 1; break;
            case "init-address": PalaceResource(document, "initFunctions", "ms_map40_InitFunction")["address"] = 1; break;
            case "entity-address": PalaceResource(document, "entityLists", "ms_map40_Entities")["address"] = 1; break;
            case "variant-order": PalaceResource(document, "setupRoutes", "MapSetupRoute40")["flagVariants"]![0]!["flag"] = 507; break;
            case "variant-target": PalaceResource(document, "setupRoutes", "MapSetupRoute40")["flagVariants"]![1]!["setup"] = "ms_map40_flag506"; break;
            case "entity-join": PalaceResource(document, "setupDefinitions", "ms_map40")["references"]!["entities"] = "ms_map21_Entities"; break;
            case "warp-facing": warp["facing"] = 0; break;
            case "warp-target": warp["targetMap"] = 21; break;
            case "warp-scroll": warp["retainsCoordinates"] = true; break;
            case "warp-reserved": warp["reserved"] = 1; break;
            case "warp-trigger": warp["trigger"]!["x"] = 8; break;
            case "warp-destination": warp["destination"]!["y"] = 29; break;
            case "warp-count": PalaceResource(document, "warpEventTables", "Map21s6_WarpEvents")["records"]!.AsArray().RemoveAt(0); break;
            case "destination-blocked": PalaceResource(document, "layouts", "Map40s1_Layout")["words"]![Index(4, 30)] = OriginalMapTraversal.CollisionMask; break;
        }
        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void NorthMap40BindsItsIndependentPaletteAndEmptyEntityIdentity()
    {
        var definition = Assert.IsType<OriginalMapImportAccepted>(Admit(SampleDocument())).Definition;
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedNorthMap40Transition(definition.NorthMap40Transition));
        var runtime = definition.RuntimeCatalog.Resolve(new("map40"));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedMap40VisualResourceSelection(runtime.VisualResourceSelection));
        Assert.Empty(runtime.EntityPopulation.Records);
        Assert.Equal("ms_map40_Entities", runtime.EntityPopulation.ResourceId);
        Assert.Equal(OriginalMapRuntimeAdmission.Map40EntityProjectionDigest, runtime.EntityPopulation.ProjectionDigest);
        Assert.Equal("ms_map40_InitFunction", runtime.SelectedInitIdentity);
        Assert.False(OriginalMapRuntimeAdmission.HasExactAcceptedCastleVisualResourceSelection(runtime.VisualResourceSelection));
    }

    [Theory]
    [InlineData("palette")]
    [InlineData("slots")]
    [InlineData("source")]
    [InlineData("setup")]
    [InlineData("animation")]
    [InlineData("layout-join")]
    [InlineData("layout-address")]
    [InlineData("block-address")]
    [InlineData("area-address")]
    [InlineData("warp-address")]
    [InlineData("warp-count")]
    [InlineData("warp-trigger-x")]
    [InlineData("warp-trigger-y")]
    [InlineData("warp-scroll")]
    [InlineData("warp-retains")]
    [InlineData("warp-direction")]
    [InlineData("warp-target")]
    [InlineData("warp-destination")]
    [InlineData("warp-facing")]
    [InlineData("warp-reserved")]
    public void Battle01AdmissionRejectsDestinationAndSourceWarpDrift(string drift)
    {
        var document = SampleDocument();
        var map = Map(document, 57);
        var table = PalaceResource(document, "warpEventTables", "Map40s6_WarpEvents");
        var warp = table["records"]![0]!;
        switch (drift)
        {
            case "palette": map["palette"] = 3; break;
            case "slots": map["tilesets"]![3] = 0; break;
            case "source": map["sourceSymbol"] = "Map40"; break;
            case "setup": map["references"]!["setupRoute"] = "MapSetupRoute40"; break;
            case "animation": map["references"]!["animationTable"] = "Map40s0_Blocks"; break;
            case "layout-join": map["references"]!["layout"] = "Map40s1_Layout"; break;
            case "layout-address": PalaceResource(document, "layouts", "Map57s1_Layout")["address"] = 1; break;
            case "block-address": PalaceResource(document, "blocksets", "Map57s0_Blocks")["address"] = 1; break;
            case "area-address": PalaceResource(document, "areaTables", "Map57s2_Areas")["address"] = 1; break;
            case "warp-address": table["address"] = 1; break;
            case "warp-count": table["records"]!.AsArray().RemoveAt(1); break;
            case "warp-trigger-x": warp["trigger"]!["x"] = 14; break;
            case "warp-trigger-y": warp["trigger"]!["y"] = 31; break;
            case "warp-scroll": warp["scrollMode"] = 1; break;
            case "warp-retains": warp["retainsCoordinates"] = true; break;
            case "warp-direction": warp["scrollDirection"] = 0; break;
            case "warp-target": warp["targetMap"] = 21; break;
            case "warp-destination": warp["destination"]!["y"] = 17; break;
            case "warp-facing": warp["facing"] = 3; break;
            case "warp-reserved": warp["reserved"] = 1; break;
        }
        AssertCode(Admit(document), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void Battle01AdmissionBindsCheckBattleFactsAndExplicitPresetOutsideExploration()
    {
        var definition = Assert.IsType<OriginalMapImportAccepted>(Admit(SampleDocument())).Definition;
        var battle = Assert.IsType<OriginalBattle01AdmissionDefinition>(definition.Battle01Admission);
        Assert.Null(battle.SetupRouteReference);
        Assert.Null(battle.AnimationTableReference);
        Assert.Throws<KeyNotFoundException>(() => definition.RuntimeCatalog.Resolve(new("map57")));
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedBattle01VisualResourceSelection(battle.VisualResourceSelection));
        Assert.Equal(OriginalBattle01ControlledPreset.NewBattle, battle.Preset);
        Assert.False(battle.Preset.CompletedFlag501);
        Assert.False(battle.Preset.SuspendedFlag88);
        Assert.False(battle.Preset.IntroFlag451);
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json"));
        using var fixture = JsonDocument.Parse(File.ReadAllText(path));
        var admission = fixture.RootElement.GetProperty("static").GetProperty("admission");
        var check = admission.GetProperty("checkBattle");
        Assert.Equal(battle.BattleIndex, check.GetProperty("tableRowIndex").GetInt32());
        Assert.Equal(battle.BattleIndex, check.GetProperty("resultRegister").GetProperty("value").GetInt32());
        Assert.Equal(battle.DestinationMap.Value, "map" + check.GetProperty("map").GetInt32());
        Assert.Equal(battle.UnlockedFlag, check.GetProperty("unlockedFlag").GetInt32());
        Assert.Equal(battle.CompletedFlag, check.GetProperty("completedFlag").GetInt32());
        Assert.Equal(new[] { battle.BattleAreaX, battle.BattleAreaY, battle.BattleAreaWidth, battle.BattleAreaHeight },
            check.GetProperty("area").EnumerateArray().Select(value => value.GetInt32()));
        Assert.Equal(new[] { (int)battle.BattleTriggerX, battle.BattleTriggerY },
            check.GetProperty("trigger").EnumerateArray().Select(value => value.GetInt32()));
        Assert.Equal(88, admission.GetProperty("newBattle").GetProperty("suspendFlag").GetInt32());
        Assert.Equal(451, fixture.RootElement.GetProperty("static").GetProperty("constants")
            .GetProperty("BATTLE_INTRO_CUTSCENE_FLAGS_START").GetInt32() + battle.BattleIndex);
    }

    private static JsonObject PalaceResource(JsonObject document, string collection, string id) =>
        document["resources"]![collection]!.AsArray().OfType<JsonObject>()
            .Single(row => row["id"]!.GetValue<string>() == id);

    private static void AddSyntheticPalaceFirstVisit(JsonObject document)
    {
        JsonObject resources = document["resources"]!.AsObject();
        JsonObject Find(string collection, string id) => resources[collection]!.AsArray()
            .OfType<JsonObject>().Single(row => row["id"]!.GetValue<string>() == id);
        JsonObject returnTable = Find("warpEventTables", "Map20s6_WarpEvents");
        returnTable["address"] = 676826;
        returnTable["sourceKind"] = "warpEvents";
        returnTable["records"] = JsonSerializer.SerializeToNode(
            Enumerable.Range(0, 11).Select(index => index == 4
                ? WarpRecord(23, 37, 19, 23, 3, 2)
                : WarpRecord(0, 0, 255, 0, 0, 0)));
        JsonObject init = Find("initFunctions", "ms_map20_InitFunction");
        init["address"] = 342374;
        init["kind"] = "operationList";
        init["bodySha256"] = OriginalMapRuntimeAdmission.PalaceInitBodySha256;
        init["scriptTargets"] = JsonSerializer.SerializeToNode(new[] { "cs_53996", "cs_53B60", "cs_53FD8" });
        init["callTargets"] = new JsonArray();
        (string Opcode, string Operand)[] guards =
        [
            ("cmpi.l", "#$22803780,((ENTITY_DATA-$1000000)).w"),
            ("bne.s", "ms_map20_flag501_InitFunction"), ("chkFlg", "605"),
            ("bne.s", "byte_53982"), ("script", "cs_53996"), ("setFlg", "605"),
            ("bra.s", "ms_map20_flag501_InitFunction"), ("script", "cs_53B60"),
            ("chkFlg", "507"), ("beq.s", "return_53994"), ("script", "cs_53FD8"), ("rts", ""),
        ];
        init["operations"] = JsonSerializer.SerializeToNode(guards.Select((op, index) =>
        {
            int? target = index switch { 1 or 6 => 8, 3 => 7, 9 => 11, _ => null };
            return new
            {
                index,
                labels = index switch
                {
                    7 => new[] { "byte_53982" },
                    8 => new[] { "ms_map20_flag501_InitFunction" },
                    11 => new[] { "return_53994" },
                    _ => Array.Empty<string>()
                },
                opcode = op.Opcode,
                operandText = op.Operand,
                branchTargetSymbol = target is null ? null : op.Operand,
                branchTargetAddress = target switch { 8 => (int?)342408, 7 => 342402, 11 => 342420, _ => null },
                localBranchTargetIndex = target,
            };
        }));
        (string Opcode, string Operand)[] operations = Enumerable.Repeat(("csWait", "0"), 113).ToArray();
        foreach (var op in new (int Index, string Opcode, string Operand)[]
        {
            (0, "textCursor", "2176"), (1, "setPos", "ALLY_BOWIE,23,39,DOWN"),
            (15, "entityActionsWait", "131"), (16, "moveRight", "1"), (17, "endActions", ""),
            (68, "entityActionsWait", "131"), (69, "moveUp", "1"), (70, "endActions", ""),
            (76, "entityActionsWait", "131"), (77, "moveRight", "1"), (78, "endActions", ""),
        }) operations[op.Index] = (op.Opcode, op.Operand);
        foreach (var program in new[]
        {
            (Id: "cs_53996", Address: 342422, Operations: operations),
            (Id: "cs_53B60", Address: 342880, Operations: new[] { (Opcode: "hide", Operand: "130"), (Opcode: "csc_end", Operand: "") }),
        })
        {
            resources["initSourcePrograms"]!.AsArray().Add(JsonSerializer.SerializeToNode(new
            {
                id = program.Id,
                address = program.Address,
                path = "data/maps/entries/map20/mapsetups/s6_initfunction.asm",
                kind = "cutscene",
                operations = program.Operations.Select((op, index) => new
                { index, opcode = op.Opcode, operandText = op.Operand, targetSymbols = Array.Empty<string>(), targetAddresses = Array.Empty<int>() }),
            }));
        }

        JsonArray entities = Find("entityLists", "ms_map20_Entities")["records"]!.AsArray();
        foreach (var actor in new[] { (Index: 2, X: 19, Y: 39), (Index: 3, X: 18, Y: 40) })
        {
            entities[actor.Index]!["x"] = actor.X;
            entities[actor.Index]!["rawX"] = actor.X;
            entities[actor.Index]!["y"] = actor.Y;
            entities[actor.Index]!["rawY"] = actor.Y;
        }
    }

    private static object[] Resource(string id) => [new { id }];

    private static object[] Resources(params string[] ids) =>
        ids.Select(id => (object)new { id }).ToArray();

    private static object[] WarpSourceRecords() =>
    [
        WarpRecord(255, 1, 19, 26, 30, 1),
        WarpRecord(0, 255, 66, 29, 32, 3),
        WarpRecord(50, 23, 44, 1, 25, 0),
        WarpRecord(50, 24, 44, 1, 25, 0),
        WarpRecord(50, 25, 44, 1, 25, 0),
        WarpRecord(46, 7, 255, 59, 12, 2),
        WarpRecord(59, 12, 255, 46, 7, 3),
        WarpRecord(3, 3, 255, 54, 3, 0),
        WarpRecord(54, 3, 255, 3, 3, 0),
    ];

    private static object[] RoofSourceRecords() =>
    [
        RoofRecord(4, 8, 255, 255, 7, 8, 2, 32),
        RoofRecord(7, 22, 255, 255, 6, 6, 5, 48),
        RoofRecord(8, 22, 255, 255, 6, 6, 5, 48),
        RoofRecord(12, 12, 255, 255, 6, 6, 10, 38),
        RoofRecord(19, 12, 255, 255, 6, 5, 17, 39),
        RoofRecord(24, 26, 51, 20, 9, 7, 22, 51),
        RoofRecord(25, 26, 51, 20, 9, 7, 22, 51),
        RoofRecord(32, 15, 255, 255, 5, 6, 30, 41),
        RoofRecord(38, 24, 255, 255, 5, 5, 36, 51),
        RoofRecord(41, 13, 255, 255, 9, 8, 39, 37),
    ];

    private static object[] ZoneSourceRecords() =>
    [
        ZoneRecord(331084, "specific", 40, 331124, 2, 255),
        ZoneRecord(331088, "specific", 96, 331180, 27, 5),
        ZoneRecord(331092, "specific", 96, 331180, 28, 5),
        ZoneRecord(331096, "specific", 96, 331180, 29, 5),
        ZoneRecord(331100, "specific", 172, 331256, 30, 5),
        ZoneRecord(331104, "specific", 172, 331256, 31, 5),
        ZoneRecord(331108, "specific", 248, 331332, 4, 4),
        ZoneRecord(331112, "specific", 282, 331366, 58, 13),
        ZoneRecord(331116, "specific", 390, 331474, 43, 10),
        ZoneRecord(331120, "default", 412, 331496, 253, 0),
    ];

    private static object[] SarahEntityEventSourceRecords() =>
    [
        EntityEventRecord(331536, "specific", 68, 331604, 1, 3),
        EntityEventRecord(331540, "specific", 136, 331672, 2, 0),
        EntityEventRecord(331544, "specific", 170, 331706, 128, 1),
        EntityEventRecord(331548, "specific", 188, 331724, 129, 3),
        EntityEventRecord(331552, "specific", 198, 331734, 130, 1),
        EntityEventRecord(331556, "specific", 212, 331748, 131, 1),
        EntityEventRecord(331560, "specific", 218, 331754, 132, 0),
        EntityEventRecord(331564, "specific", 224, 331760, 133, 1),
        EntityEventRecord(331568, "specific", 238, 331774, 134, 1),
        EntityEventRecord(331572, "specific", 244, 331780, 137, 1),
        EntityEventRecord(331576, "specific", 254, 331790, 138, 1),
        EntityEventRecord(331580, "specific", 272, 331808, 139, 1),
        EntityEventRecord(331584, "specific", 290, 331826, 140, 1),
        EntityEventRecord(331588, "specific", 300, 331836, 141, 1),
        EntityEventRecord(331592, "specific", 238, 331774, 144, 1),
        EntityEventRecord(331596, "specific", 308, 331844, 142, 3),
        EntityEventRecord(331600, "default", 330, 331866, 253, 0),
    ];

    private static object[] EntitySourceRecords()
    {
        List<object> records =
        [
            new
            {
                address = 6,
                kind = "fixed",
                rawX = OriginalMapRuntimeAdmission.SarahActorInitialX,
                rawY = OriginalMapRuntimeAdmission.SarahActorInitialY,
                x = OriginalMapRuntimeAdmission.SarahActorInitialX,
                y = OriginalMapRuntimeAdmission.SarahActorInitialY,
                facing = 3,
                mapSprite = 1,
                actionValue = OriginalMapRuntimeAdmission.SarahActorInitialActionValue,
            },
            new
            {
                address = 14,
                kind = "walking",
                rawX = 0xEA,
                rawY = 8,
                x = 42,
                y = 8,
                facing = 1,
                mapSprite = 5,
                walking = new { originX = 42, originY = 8, range = 3 },
            },
            new
            {
                address = 22,
                kind = "fixed",
                rawX = 5,
                rawY = 6,
                x = 5,
                y = 6,
                facing = 0,
                mapSprite = 195,
                actionValue = OriginalMapRuntimeAdmission.Zone601ActorInitialActionValue,
            },
        ];
        for (int ordinal = 4; ordinal <= 19; ordinal++)
        {
            if (ordinal == OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal)
            {
                records.Add(new
                {
                    address = OriginalMapRuntimeAdmission.Entity142ActorSourceAddress,
                    kind = "fixed",
                    rawX = OriginalMapRuntimeAdmission.Entity142ActorX,
                    rawY = OriginalMapRuntimeAdmission.Entity142ActorY,
                    x = OriginalMapRuntimeAdmission.Entity142ActorX,
                    y = OriginalMapRuntimeAdmission.Entity142ActorY,
                    facing = OriginalMapRuntimeAdmission.Entity142ActorOpaqueFacing,
                    mapSprite = OriginalMapRuntimeAdmission.Entity142ActorMapSprite,
                    actionValue = OriginalMapRuntimeAdmission.Entity142ActorActionValue,
                });
                continue;
            }

            if (ordinal == OriginalMapRuntimeAdmission.MessengerActor143SourceRecordOrdinal)
            {
                records.Add(new
                {
                    address = OriginalMapRuntimeAdmission.MessengerActor143SourceAddress,
                    kind = "fixed",
                    rawX = OriginalMapRuntimeAdmission.MessengerActor143InitialX,
                    rawY = OriginalMapRuntimeAdmission.MessengerActor143InitialY,
                    x = OriginalMapRuntimeAdmission.MessengerActor143InitialX,
                    y = OriginalMapRuntimeAdmission.MessengerActor143InitialY,
                    facing = OriginalMapRuntimeAdmission.MessengerActor143InitialOpaqueFacing,
                    mapSprite = OriginalMapRuntimeAdmission.MessengerActor143MapSprite,
                    actionValue = OriginalMapRuntimeAdmission.MessengerActor143ActionValue,
                });
                continue;
            }

            if (ordinal is OriginalMapRuntimeAdmission.MessengerGuard138SourceRecordOrdinal or
                OriginalMapRuntimeAdmission.MessengerGuard139SourceRecordOrdinal)
            {
                bool first = ordinal ==
                    OriginalMapRuntimeAdmission.MessengerGuard138SourceRecordOrdinal;
                records.Add(new
                {
                    address = first
                        ? OriginalMapRuntimeAdmission.MessengerGuard138SourceAddress
                        : OriginalMapRuntimeAdmission.MessengerGuard139SourceAddress,
                    kind = "fixed",
                    rawX = first
                        ? OriginalMapRuntimeAdmission.MessengerGuard138X
                        : OriginalMapRuntimeAdmission.MessengerGuard139X,
                    rawY = first
                        ? OriginalMapRuntimeAdmission.MessengerGuard138Y
                        : OriginalMapRuntimeAdmission.MessengerGuard139Y,
                    x = first
                        ? OriginalMapRuntimeAdmission.MessengerGuard138X
                        : OriginalMapRuntimeAdmission.MessengerGuard139X,
                    y = first
                        ? OriginalMapRuntimeAdmission.MessengerGuard138Y
                        : OriginalMapRuntimeAdmission.MessengerGuard139Y,
                    facing = first
                        ? OriginalMapRuntimeAdmission.MessengerGuard138OpaqueFacing
                        : OriginalMapRuntimeAdmission.MessengerGuard139OpaqueFacing,
                    mapSprite = OriginalMapRuntimeAdmission.MessengerGuardMapSprite,
                    actionValue = OriginalMapRuntimeAdmission.MessengerGuardActionValue,
                });
                continue;
            }

            records.Add(new
            {
                address = 1000 + (ordinal * 8),
                kind = "fixed",
                rawX = ordinal,
                rawY = 1,
                x = ordinal,
                y = 1,
                facing = 0,
                mapSprite = ordinal,
                actionValue = 0U,
            });
        }

        return [.. records];
    }

    private static object[] Map19EntitySourceRecords()
    {
        List<object> records = [];
        for (int ordinal = 1; ordinal <= 9; ordinal++)
        {
            records.Add(new
            {
                address = 2000 + (ordinal * 8),
                kind = "fixed",
                rawX = ordinal,
                rawY = 1,
                x = ordinal,
                y = 1,
                facing = 0,
                mapSprite = ordinal,
                actionValue = 0U,
            });
        }

        for (int ordinal = 10; ordinal <= 13; ordinal++)
        {
            records.Add(new
            {
                address = 2000 + (ordinal * 8),
                kind = "walking",
                rawX = ordinal,
                rawY = 2,
                x = ordinal,
                y = 2,
                facing = 1,
                mapSprite = ordinal,
                walking = new { originX = ordinal, originY = 2, range = 1 },
            });
        }

        return [.. records];
    }

    [Fact]
    public void Entity142EventAndSourceActorDriftFailSemanticAdmission()
    {
        JsonObject eventDrift = SampleDocument();
        EntityEventRecords(eventDrift)[
            OriginalMapRuntimeAdmission.Entity142EventRecordOrdinal - 1]!
            .AsObject()["relativeOffset"] =
                OriginalMapRuntimeAdmission.Entity142EventRelativeOffset + 1;
        AssertCode(Admit(eventDrift), OriginalMapImportFailureCode.InvalidMapProjection);

        JsonObject actorDrift = SampleDocument();
        EntityRecords(actorDrift)[
            OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal - 1]!
            .AsObject()["mapSprite"] =
                OriginalMapRuntimeAdmission.Entity142ActorMapSprite - 1;
        AssertCode(Admit(actorDrift), OriginalMapImportFailureCode.InvalidMapProjection);
    }

    private static object EntityEventRecord(
        int address,
        string kind,
        int relativeOffset,
        int resolvedTargetAddress,
        int entity,
        int flags) =>
        new { address, kind, relativeOffset, resolvedTargetAddress, entity, flags };

    private static object ZoneRecord(
        int address,
        string kind,
        int relativeOffset,
        int resolvedTargetAddress,
        int x,
        int y) =>
        new { address, kind, relativeOffset, resolvedTargetAddress, x, y };

    private static object[] ZoneBlockingOperations()
    {
        (string Opcode, string Operand)[] values =
        [
            ("setActscriptWait", "128,eas_Init"),
            ("entityActionsWait", "128"),
            ("moveUp", "2"),
            ("faceLeft", "20"),
            ("endActions", ""),
            ("textCursor", "510"),
            ("nextText", "$0,128"),
            ("nextText", "$0,128"),
            ("textCursor", "483"),
            ("nextSingleText", "$0,128"),
            ("setActscriptWait", "128,eas_Init"),
            ("csc_end", ""),
        ];
        return values.Select((value, index) => (object)new
        {
            index,
            opcode = value.Opcode,
            operandText = value.Operand,
            targetSymbols = Array.Empty<string>(),
            targetAddresses = Array.Empty<int>(),
        }).ToArray();
    }

    private static object[] SarahBlockingOperations()
    {
        (string Opcode, string Operand)[] values =
        [
            ("entityActionsWait", "ALLY_SARAH"),
            ("moveLeft", "1"),
            ("moveUp", "1"),
            ("endActions", ""),
            ("csc_end", ""),
        ];
        return values.Select((value, index) => (object)new
        {
            index,
            opcode = value.Opcode,
            operandText = value.Operand,
            targetSymbols = Array.Empty<string>(),
            targetAddresses = Array.Empty<int>(),
        }).ToArray();
    }

    private static object[] AstralZonePositionOperations()
    {
        (string Opcode, string Operand)[] values =
        [
            ("setPos", "ALLY_SARAH,41,10,UP"),
            ("setPos", "128,6,4,UP"),
            ("csc_end", ""),
        ];
        return values.Select((value, index) => (object)new
        {
            index,
            opcode = value.Opcode,
            operandText = value.Operand,
            targetSymbols = Array.Empty<string>(),
            targetAddresses = Array.Empty<int>(),
        }).ToArray();
    }

    private static object[] MessengerMainOperations()
    {
        object[] operations = Enumerable.Range(0, 112)
            .Select(index => MessengerOperation(index, "noop", ""))
            .ToArray();
        operations[0] = MessengerOperation(0, "textCursor", "517");
        operations[102] = MessengerOperation(102, "yesNo", "");
        operations[103] = MessengerOperation(
            103,
            "jumpIfFlagSet",
            "89,cs_51614",
            ["cs_51614"],
            [333332]);
        operations[111] = MessengerOperation(
            111,
            "jump",
            "cs_51650",
            ["cs_51650"],
            [333392]);
        return operations;
    }

    private static object[] MessengerAcceptedOperations()
    {
        (string Opcode, string Operand)[] values =
        [
            ("textCursor", "535"),
            ("nextSingleText", "$0,ALLY_SARAH"),
            ("setFacing", "ALLY_CHESTER,LEFT"),
            ("nextSingleText", "$0,ALLY_CHESTER"),
            ("setF", "600"),
            ("setF", "66"),
            ("join", "128"),
            ("followEntity", "ALLY_SARAH,ALLY_BOWIE,2"),
            ("followEntity", "ALLY_CHESTER,ALLY_SARAH,2"),
            ("setPos", "138,27,3,DOWN"),
            ("setPos", "139,31,3,DOWN"),
        ];
        return values.Select((value, index) =>
            MessengerOperation(index, value.Opcode, value.Operand)).ToArray();
    }

    private static object[] CastleGateOperations()
    {
        (string Opcode, string Operand)[] values =
        [
            ("textCursor", "537"),
            ("entityActions", "138"),
            ("moveRight", "1"),
            ("endActions", ""),
            ("entityActionsWait", "139"),
            ("moveLeft", "1"),
            ("endActions", ""),
            ("setFacing", "138,DOWN"),
            ("setFacing", "139,DOWN"),
            ("nextSingleText", "$0,138"),
            ("setFacing", "ALLY_SARAH,UP"),
            ("setFacing", "ALLY_CHESTER,UP"),
            ("nextSingleText", "$C0,ALLY_SARAH"),
            ("nextSingleText", "$0,138"),
            ("nextSingleText", "$C0,ALLY_SARAH"),
            ("nextSingleText", "$0,138"),
            ("nextSingleText", "$0,139"),
            ("entityActions", "138"),
            ("moveLeft", "1"),
            ("endActions", ""),
            ("entityActionsWait", "139"),
            ("moveRight", "1"),
            ("endActions", ""),
            ("setFacing", "138,DOWN"),
            ("setFacing", "139,DOWN"),
            ("csc_end", ""),
        ];
        return values.Select((value, index) =>
            MessengerOperation(index, value.Opcode, value.Operand)).ToArray();
    }

    private static object MessengerOperation(
        int index,
        string opcode,
        string operandText,
        string[]? targetSymbols = null,
        int[]? targetAddresses = null) =>
        new
        {
            index,
            opcode,
            operandText,
            targetSymbols = targetSymbols ?? Array.Empty<string>(),
            targetAddresses = targetAddresses ?? Array.Empty<int>(),
        };

    private static object RoofRecord(
        int triggerX,
        int triggerY,
        int sourceX,
        int sourceY,
        int width,
        int height,
        int destinationX,
        int destinationY) =>
        new
        {
            trigger = Point(triggerX, triggerY),
            source = Point(sourceX, sourceY),
            size = new { width, height },
            destination = Point(destinationX, destinationY),
        };

    private static object WarpRecord(
        int triggerX,
        int triggerY,
        int targetMap,
        int destinationX,
        int destinationY,
        int facing) =>
        new
        {
            trigger = Point(triggerX, triggerY),
            scrollMode = 0,
            retainsCoordinates = false,
            scrollDirection = (int?)null,
            targetMap,
            destination = Point(destinationX, destinationY),
            facing,
            reserved = 0,
        };

    private static object Point(int x, int y) => new { x, y };

    private static object AreaRecord(
        int minimumX,
        int minimumY,
        int maximumX,
        int maximumY,
        int secondForegroundY = 0,
        int defaultMusic = 8) =>
        new
        {
            mainLayerStart = Point(minimumX, minimumY),
            mainLayerEnd = Point(maximumX, maximumY),
            secondLayerForegroundStart = Point(0, secondForegroundY),
            secondLayerBackgroundStart = Point(0, 0),
            mainLayerParallax = Point(256, 256),
            secondLayerParallax = Point(256, 256),
            mainLayerAutoscroll = Point(0, 0),
            secondLayerAutoscroll = Point(0, 0),
            mainLayerType = 0,
            defaultMusic,
        };

    private static JsonObject Map(JsonObject document, int mapId) =>
        document["maps"]!.AsArray()
            .Select(node => node!.AsObject())
            .Single(map => map["id"]!.GetValue<int>() == mapId);

    private static JsonArray ResourceArray(JsonObject document, string name) =>
        document["resources"]!.AsObject()[name]!.AsArray();

    private static JsonObject ResourceById(
        JsonObject document,
        string collection,
        string id) =>
        ResourceArray(document, collection)
            .Select(node => node!.AsObject())
            .Single(resource => string.Equals(
                resource["id"]!.GetValue<string>(),
                id,
                StringComparison.Ordinal));

    private static JsonArray AreaRecords(JsonObject document) =>
        ResourceArray(document, "areaTables")[0]!.AsObject()["records"]!.AsArray();

    private static JsonArray EntityRecords(JsonObject document) =>
        ResourceArray(document, "entityLists")[0]!.AsObject()["records"]!.AsArray();

    private static JsonObject SetupReferences(JsonObject document) =>
        ResourceArray(document, "setupDefinitions")[0]!.AsObject()["references"]!.AsObject();

    private static JsonArray LayoutWords(JsonObject document) =>
        ResourceArray(document, "layouts")[0]!.AsObject()["words"]!.AsArray();

    private static JsonArray BlockWords(JsonObject document, int zeroBasedBlockIndex) =>
        ResourceArray(document, "blocksets")[0]!
            .AsObject()["blocks"]!.AsArray()[zeroBasedBlockIndex]!.AsArray();

    private static JsonArray StepRecords(JsonObject document) =>
        ResourceArray(document, "stepEventTables")[0]!
            .AsObject()["records"]!.AsArray();

    private static JsonArray WarpRecords(JsonObject document) =>
        ResourceArray(document, "warpEventTables")[0]!
            .AsObject()["records"]!.AsArray();

    private static JsonArray RoofRecords(JsonObject document) =>
        ResourceArray(document, "roofEventTables")[0]!
            .AsObject()["records"]!.AsArray();

    private static JsonArray ZoneRecords(JsonObject document) =>
        ResourceArray(document, "zoneEventHandlers")[0]!
            .AsObject()["records"]!.AsArray();

    private static JsonArray ZoneProgramOperations(JsonObject document) =>
        ResourceArray(document, "standaloneScriptPrograms")[1]!
            .AsObject()["operations"]!.AsArray();

    private static JsonArray MessengerMainProgramOperations(JsonObject document) =>
        ResourceArray(document, "standaloneScriptPrograms")[3]!
            .AsObject()["operations"]!.AsArray();

    private static JsonArray MessengerAcceptedProgramOperations(JsonObject document) =>
        ResourceArray(document, "standaloneScriptPrograms")[4]!
            .AsObject()["operations"]!.AsArray();

    private static JsonArray CastleGateProgramOperations(JsonObject document) =>
        ResourceArray(document, "standaloneScriptPrograms")[5]!
            .AsObject()["operations"]!.AsArray();

    private static JsonArray EntityEventRecords(JsonObject document) =>
        ResourceArray(document, "entityEventHandlers")[0]!
            .AsObject()["records"]!.AsArray();

    private static JsonArray SarahProgramOperations(JsonObject document) =>
        ResourceArray(document, "standaloneScriptPrograms")[0]!
            .AsObject()["operations"]!.AsArray();

    private static JsonArray AstralZoneProgramOperations(JsonObject document) =>
        ResourceArray(document, "standaloneScriptPrograms")[2]!
            .AsObject()["operations"]!.AsArray();

    private static (int, int, int, int, int, int) Geometry(WorkingMapBlockCopy copy) =>
        (copy.SourceX, copy.SourceY, copy.DestinationX, copy.DestinationY,
            copy.Width, copy.Height);

    private static int Index(int x, int y) => (y * WorkingMapLayout.ColumnCount) + x;
}

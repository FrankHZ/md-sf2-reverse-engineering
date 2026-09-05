using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Content.Tests;

public sealed class PrivateCanonicalMap3ImportReaderTests
{
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
        Assert.Equal(3, accepted.Definition.BlockCatalog.Records.Count);
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
        Assert.Equal(2, accepted.Definition.RuntimeCatalog.Records.Count);
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
        LayoutWords(missingBlock)[0] = 3;
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
        LayoutWords(danglingLayout)[0] = 3;
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

    [Fact]
    public void AcceptedIgnoredCanonicalImportCanBeCheckedLocallyWithoutBecomingATestInput()
    {
        string? path = Environment.GetEnvironmentVariable("SF2_PRIVATE_CANONICAL_MAP_IMPORT");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

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
                        blocks = Enumerable.Range(0, 3).Select(_ => new int[9]).ToArray(),
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
                flagEventTables = Resources("Map03s3_FlagEvents", "Map19s3_FlagEvents"),
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
                                    trigger = Point(index, 60),
                                    source = Point(index, 61),
                                    size = new { width = 1, height = 1 },
                                    destination = Point(index, 62),
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
                    new { id = "Map03s7_ChestItems" },
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
        return node!.AsObject();
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
        RoofRecord(20, 20, 0, 0, 1, 1, 20, 20),
        RoofRecord(21, 20, 1, 1, 1, 1, 21, 20),
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

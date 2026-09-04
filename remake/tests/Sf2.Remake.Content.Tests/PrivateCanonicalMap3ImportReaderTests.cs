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
        OriginalMapImportAccepted accepted = Assert.IsType<OriginalMapImportAccepted>(
            PrivateCanonicalMap3ImportReader.AdmitSemanticDocumentForTests(bytes));

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
        Assert.Equal(2, accepted.Definition.EntityPopulation.Records.Count);
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
            new byte[] { 0, 0, 0, 1 },
            accepted.Definition.EntityPopulation.Records[0].OpaqueTail);
        Assert.Equal(
            new byte[] { 0xFF, 1, 2, 3 },
            accepted.Definition.EntityPopulation.Records[1].OpaqueTail);
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
                    blockset = "Map03s0_Blocks",
                    layout = "Map03s1_Layout",
                    areaTable = "Map03s2_Areas",
                    flagEventTable = "Map03s3_FlagEvents",
                    stepEventTable = "Map03s4_StepEvents",
                    roofEventTable = "Map03s5_RoofEvents",
                    warpEventTable = "Map03s6_WarpEvents",
                    chestItemTable = "Map03s7_ChestItems",
                    otherItemTable = "Map03s8_OtherItems",
                    animationTable = "Map03s9_Animations",
                    setupRoute = "MapSetupRoute03",
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
                },
                flagEventTables = Resource("Map03s3_FlagEvents"),
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
                },
                itemTables = new object[]
                {
                    new { id = "Map03s7_ChestItems" },
                    new { id = "Map03s8_OtherItems" },
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
                },
                setupDefinitions = new object[]
                {
                    new { id = "ms_map3", address = 5, references = setupReferences },
                    new { id = "ms_map3_variant_a" },
                    new { id = "ms_map3_variant_b" },
                    new { id = "ms_map3_variant_c" },
                },
                entityLists = new object[]
                {
                    new
                    {
                        id = "ms_map3_Entities",
                        address = 6,
                        records = new object[]
                        {
                            new
                            {
                                address = 6,
                                kind = "fixed",
                                rawX = 1,
                                rawY = 2,
                                x = 1,
                                y = 2,
                                facing = 3,
                                mapSprite = 4,
                                actionValue = 1U,
                            },
                            new
                            {
                                address = 14,
                                kind = "walking",
                                rawX = 0xC1,
                                rawY = 2,
                                x = 1,
                                y = 2,
                                facing = 1,
                                mapSprite = 5,
                                walking = new { originX = 1, originY = 2, range = 3 },
                            },
                        },
                    },
                },
                entityEventHandlers = Resource("ms_map3_EntityEvents"),
                zoneEventHandlers = Resource("ms_map3_ZoneEvents"),
                itemEventHandlers = Resource("ms_map3_Section5"),
                areaDescriptionHandlers = Resource("ms_map3_AreaDescriptions"),
                initFunctions = Resource("ms_map3_InitFunction"),
                standaloneScriptPrograms = Array.Empty<object>(),
                initSourcePrograms = Array.Empty<object>(),
            },
            runtimeQuestions = new[] { "unsupported-natural-runtime" },
        });
        return node!.AsObject();
    }

    private static object[] Resource(string id) => [new { id }];

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
        int secondForegroundY = 0) =>
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
            defaultMusic = 8,
        };

    private static JsonObject Map(JsonObject document, int mapId) =>
        document["maps"]!.AsArray()
            .Select(node => node!.AsObject())
            .Single(map => map["id"]!.GetValue<int>() == mapId);

    private static JsonArray ResourceArray(JsonObject document, string name) =>
        document["resources"]!.AsObject()[name]!.AsArray();

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

    private static (int, int, int, int, int, int) Geometry(WorkingMapBlockCopy copy) =>
        (copy.SourceX, copy.SourceY, copy.DestinationX, copy.DestinationY,
            copy.Width, copy.Height);

    private static int Index(int x, int y) => (y * WorkingMapLayout.ColumnCount) + x;
}

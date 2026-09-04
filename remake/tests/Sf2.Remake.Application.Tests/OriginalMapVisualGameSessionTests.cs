using System.Collections.ObjectModel;
using System.Reflection;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class OriginalMapVisualGameSessionTests
{
    [Fact]
    public void InvalidImportRequestInvokesNeitherSource()
    {
        CountingImportSource importSource = new(_ => ExactImportAccepted());
        CountingVisualSource visualSource = new(_ => ExactVisualAccepted());
        OriginalMapImportRequest invalid = new(
            OriginalMapRuntimeAdmission.PackageId,
            ContentProfile.PrivateLocal,
            new string('A', 64));

        PrivateOriginalMapVisualGameSessionImportRejected rejected =
            Assert.IsType<PrivateOriginalMapVisualGameSessionImportRejected>(
                GameSession.StartPrivateOriginalMapWithVisualPayload(
                    importSource,
                    invalid,
                    visualSource,
                    VisualRequest()));

        Assert.Equal(
            OriginalMapImportFailureCode.ContentDigestMismatch,
            rejected.Diagnostic.Code);
        Assert.Equal(0, importSource.AdmitCalls);
        Assert.Equal(0, visualSource.AdmitCalls);
    }

    [Fact]
    public void InvalidVisualRequestInvokesNeitherSource()
    {
        CountingImportSource importSource = new(_ => ExactImportAccepted());
        CountingVisualSource visualSource = new(_ => ExactVisualAccepted());
        OriginalMapVisualPayloadRequest invalid = new(
            "wrong-private-map3-visual-package",
            ContentProfile.PrivateLocal,
            ExactSelection(),
            OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
            OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
            OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest);

        PrivateOriginalMapVisualGameSessionPayloadRejected rejected =
            Assert.IsType<PrivateOriginalMapVisualGameSessionPayloadRejected>(
                GameSession.StartPrivateOriginalMapWithVisualPayload(
                    importSource,
                    ImportRequest(),
                    visualSource,
                    invalid));

        Assert.Equal(
            OriginalMapVisualPayloadFailureCode.PackageIdentityMismatch,
            rejected.Diagnostic.Code);
        Assert.Equal(0, importSource.AdmitCalls);
        Assert.Equal(0, visualSource.AdmitCalls);
    }

    [Fact]
    public void ImportRejectionAndAcceptedEnvelopeDriftShortCircuitVisualAdmission()
    {
        OriginalMapImportDiagnostic sourceDiagnostic = new(
            OriginalMapImportFailureCode.PackageUnavailable,
            "package",
            "The project-authored import source is unavailable.");
        CountingImportSource rejectedSource = new(
            _ => new OriginalMapImportRejected(sourceDiagnostic));
        CountingVisualSource visualAfterRejection = new(_ => ExactVisualAccepted());

        PrivateOriginalMapVisualGameSessionImportRejected sourceRejected =
            Assert.IsType<PrivateOriginalMapVisualGameSessionImportRejected>(
                Start(rejectedSource, visualAfterRejection));

        Assert.Same(sourceDiagnostic, sourceRejected.Diagnostic);
        Assert.Equal(1, rejectedSource.AdmitCalls);
        Assert.Equal(0, visualAfterRejection.AdmitCalls);

        CountingImportSource incompatibleSource = new(
            _ => ExactImportAccepted(
                receipt: ImportReceipt(packageId: "wrong-canonical-package")));
        CountingVisualSource visualAfterIncompatible = new(_ => ExactVisualAccepted());

        PrivateOriginalMapVisualGameSessionImportRejected incompatible =
            Assert.IsType<PrivateOriginalMapVisualGameSessionImportRejected>(
                Start(incompatibleSource, visualAfterIncompatible));

        Assert.Equal(
            OriginalMapImportFailureCode.PackageIdentityMismatch,
            incompatible.Diagnostic.Code);
        Assert.Equal(1, incompatibleSource.AdmitCalls);
        Assert.Equal(0, visualAfterIncompatible.AdmitCalls);
    }

    [Fact]
    public void ExactStructuralPortsStartOnceAndReturnInitialSessionAndImmutableBinding()
    {
        OriginalMapImportAccepted importAccepted = ExactImportAccepted();
        OriginalMapVisualPayloadAccepted visualAccepted = ExactVisualAccepted();
        CountingImportSource importSource = new(_ => importAccepted);
        CountingVisualSource visualSource = new(_ => visualAccepted);

        PrivateOriginalMapVisualGameSessionStarted started =
            Assert.IsType<PrivateOriginalMapVisualGameSessionStarted>(
                Start(importSource, visualSource));

        Assert.Equal(1, importSource.AdmitCalls);
        Assert.Equal(1, visualSource.AdmitCalls);
        Assert.Same(importAccepted.Receipt, started.ImportReceipt);
        Assert.Same(visualAccepted.Definition, started.Binding.Definition);
        Assert.Same(visualAccepted.Receipt, started.Binding.Receipt);
        Assert.Equal(
            PrivateOriginalMapVisualRuntimeAdmission.Capability,
            started.Binding.Capability);
        PrivateOriginalMapSessionSnapshot snapshot =
            started.Session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.MapId), snapshot.Map);
        Assert.Equal(
            new MapPosition(
                OriginalMapRuntimeAdmission.StartX,
                OriginalMapRuntimeAdmission.StartY),
            snapshot.PlayerPosition);
        Assert.Equal(0, snapshot.SimulationStep);
        Assert.Null(snapshot.LastTraversal);
        Assert.DoesNotContain(
            typeof(PrivateOriginalMapSessionSnapshot).GetProperties(),
            property => property.PropertyType == typeof(OriginalMapVisualPayloadDefinition) ||
                property.PropertyType == typeof(OriginalMapVisualPayloadReceipt));
        Assert.All(
            typeof(PrivateOriginalMapVisualRuntimeBinding).GetProperties(),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void VisualSourceRejectionPreventsSessionConstructionAndRetainsItsDiagnostic()
    {
        OriginalMapVisualPayloadDiagnostic diagnostic = new(
            OriginalMapVisualPayloadFailureCode.PackageUnavailable,
            "package",
            "The project-authored visual source is unavailable.");
        CountingImportSource importSource = new(_ => ExactImportAccepted());
        CountingVisualSource visualSource = new(
            _ => new OriginalMapVisualPayloadRejected(diagnostic));

        PrivateOriginalMapVisualGameSessionPayloadRejected rejected =
            Assert.IsType<PrivateOriginalMapVisualGameSessionPayloadRejected>(
                Start(importSource, visualSource));

        Assert.Same(diagnostic, rejected.Diagnostic);
        Assert.Equal(1, importSource.AdmitCalls);
        Assert.Equal(1, visualSource.AdmitCalls);
    }

    [Fact]
    public void FakeAcceptedImportEnvelopeDriftNeverInvokesVisualSource()
    {
        OriginalMapImportReceipt wrongProfile = ImportReceipt();
        SetAutoProperty(wrongProfile, nameof(OriginalMapImportReceipt.Profile),
            ContentProfile.PublicSynthetic);
        OriginalMapVisualResourceSelection reordered = new(
            new MapId(OriginalMapRuntimeAdmission.MapId),
            paletteIndex: 0,
            [37, 0, 43, 53, 66]);
        (OriginalMapImportAccepted Accepted, OriginalMapImportFailureCode Code)[] cases =
        [
            (ExactImportAccepted(receipt: ImportReceipt(
                packageId: "wrong-canonical-package")),
                OriginalMapImportFailureCode.PackageIdentityMismatch),
            (ExactImportAccepted(receipt: ImportReceipt(schemaVersion: 2)),
                OriginalMapImportFailureCode.UnsupportedSchema),
            (ExactImportAccepted(receipt: wrongProfile),
                OriginalMapImportFailureCode.ProfileMismatch),
            (ExactImportAccepted(receipt: ImportReceipt(
                romSha256: new string('A', 64))),
                OriginalMapImportFailureCode.ProvenanceMismatch),
            (ExactImportAccepted(definition: ImportDefinition(reordered)),
                OriginalMapImportFailureCode.InvalidMapProjection),
        ];

        foreach ((OriginalMapImportAccepted accepted, OriginalMapImportFailureCode code)
                 in cases)
        {
            CountingImportSource importSource = new(_ => accepted);
            CountingVisualSource visualSource = new(_ => ExactVisualAccepted());

            PrivateOriginalMapVisualGameSessionImportRejected rejected =
                Assert.IsType<PrivateOriginalMapVisualGameSessionImportRejected>(
                    Start(importSource, visualSource));

            Assert.Equal(code, rejected.Diagnostic.Code);
            Assert.Equal(1, importSource.AdmitCalls);
            Assert.Equal(0, visualSource.AdmitCalls);
        }
    }

    [Fact]
    public void FakeAcceptedVisualIdentityProfileAndProvenanceDriftPreventSession()
    {
        OriginalMapVisualPayloadReceipt wrongProfile = VisualReceipt();
        SetAutoProperty(wrongProfile, nameof(OriginalMapVisualPayloadReceipt.Profile),
            ContentProfile.PublicSynthetic);
        (OriginalMapVisualPayloadAccepted Accepted,
            PrivateOriginalMapVisualRuntimeFailureCode Code)[] cases =
        [
            (ExactVisualAccepted(receipt: VisualReceipt(
                packageId: "wrong-private-visual-package")),
                PrivateOriginalMapVisualRuntimeFailureCode.PackageIdentityMismatch),
            (ExactVisualAccepted(receipt: VisualReceipt(schemaVersion: 2)),
                PrivateOriginalMapVisualRuntimeFailureCode.UnsupportedSchema),
            (ExactVisualAccepted(receipt: wrongProfile),
                PrivateOriginalMapVisualRuntimeFailureCode.ProfileMismatch),
            (ExactVisualAccepted(receipt: VisualReceipt(
                capability: "wrong-private-visual-capability")),
                PrivateOriginalMapVisualRuntimeFailureCode.PackageIdentityMismatch),
            (ExactVisualAccepted(receipt: VisualReceipt(
                provenance: VisualProvenance(romSha256: new string('A', 64)))),
                PrivateOriginalMapVisualRuntimeFailureCode.ProvenanceMismatch),
            (ExactVisualAccepted(receipt: VisualReceipt(
                provenance: VisualProvenance(
                    tilesetMetadataDigest: new string('B', 64)))),
                PrivateOriginalMapVisualRuntimeFailureCode.ProvenanceMismatch),
        ];

        AssertVisualBindingRejections(cases);
    }

    [Fact]
    public void FakeAcceptedVisualEvidenceRowsAndDimensionsDriftPreventSession()
    {
        OriginalMapVisualPayloadReceipt missingOwner = VisualReceipt();
        SetField(
            missingOwner,
            "_evidenceOwnerIds",
            Array.AsReadOnly(
                OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners.Take(1).ToArray()));
        OriginalMapVisualPayloadReceipt extraOwner = VisualReceipt();
        SetField(
            extraOwner,
            "_evidenceOwnerIds",
            Array.AsReadOnly(
                OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners
                    .Append("project-authored-extra-owner")
                    .ToArray()));
        OriginalMapVisualPayloadReceipt duplicateOwner = VisualReceipt(
            evidenceOwnerIds:
            [
                .. OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners,
                OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners[0],
            ]);
        OriginalMapVisualPayloadReceipt wrongDimensions = VisualReceipt();
        SetAutoProperty(
            wrongDimensions,
            nameof(OriginalMapVisualPayloadReceipt.DecodedBytesPerTileset),
            OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset - 1);
        (OriginalMapVisualPayloadAccepted Accepted,
            PrivateOriginalMapVisualRuntimeFailureCode Code)[] cases =
        [
            (ExactVisualAccepted(receipt: missingOwner),
                PrivateOriginalMapVisualRuntimeFailureCode.EvidenceOwnerMismatch),
            (ExactVisualAccepted(receipt: extraOwner),
                PrivateOriginalMapVisualRuntimeFailureCode.EvidenceOwnerMismatch),
            (ExactVisualAccepted(receipt: duplicateOwner),
                PrivateOriginalMapVisualRuntimeFailureCode.EvidenceOwnerMismatch),
            (ExactVisualAccepted(receipt: wrongDimensions),
                PrivateOriginalMapVisualRuntimeFailureCode.InvalidShape),
        ];

        AssertVisualBindingRejections(cases);
    }

    [Fact]
    public void FakeAcceptedVisualSelectionAndSlotOrderDriftPreventSession()
    {
        OriginalMapVisualPayloadDefinition wrongPalette = VisualDefinition();
        SetAutoProperty(
            wrongPalette,
            nameof(OriginalMapVisualPayloadDefinition.Selection),
            new OriginalMapVisualResourceSelection(
                new MapId(OriginalMapRuntimeAdmission.MapId),
                paletteIndex: 1,
                [0, 37, 43, 53, 66]));
        OriginalMapVisualPayloadDefinition reordered = VisualDefinition();
        SetAutoProperty(
            reordered,
            nameof(OriginalMapVisualPayloadDefinition.Selection),
            new OriginalMapVisualResourceSelection(
                new MapId(OriginalMapRuntimeAdmission.MapId),
                paletteIndex: 0,
                [37, 0, 43, 53, 66]));
        (OriginalMapVisualPayloadAccepted Accepted,
            PrivateOriginalMapVisualRuntimeFailureCode Code)[] cases =
        [
            (ExactVisualAccepted(definition: wrongPalette),
                PrivateOriginalMapVisualRuntimeFailureCode.SelectionMismatch),
            (ExactVisualAccepted(definition: reordered),
                PrivateOriginalMapVisualRuntimeFailureCode.SelectionMismatch),
        ];

        AssertVisualBindingRejections(cases);
    }

    [Fact]
    public void UnknownSourceResultsRemainProgrammerInvariants()
    {
        CountingImportSource unknownImport = new(_ => new UnknownImportResult());
        CountingVisualSource visualAfterUnknownImport = new(_ => ExactVisualAccepted());

        Assert.Throws<InvalidOperationException>(() =>
            Start(unknownImport, visualAfterUnknownImport));
        Assert.Equal(1, unknownImport.AdmitCalls);
        Assert.Equal(0, visualAfterUnknownImport.AdmitCalls);

        CountingImportSource importSource = new(_ => ExactImportAccepted());
        CountingVisualSource unknownVisual = new(_ => new UnknownVisualResult());

        Assert.Throws<InvalidOperationException>(() => Start(importSource, unknownVisual));
        Assert.Equal(1, importSource.AdmitCalls);
        Assert.Equal(1, unknownVisual.AdmitCalls);
    }

    [Fact]
    public void BindingDiagnosticsArePathFreeAndTraversalOnlyStartRemainsAvailable()
    {
        PrivateOriginalMapVisualGameSessionBindingRejected rejected =
            Assert.IsType<PrivateOriginalMapVisualGameSessionBindingRejected>(
                Start(
                    new CountingImportSource(_ => ExactImportAccepted()),
                    new CountingVisualSource(_ => ExactVisualAccepted(
                        receipt: VisualReceipt(
                            capability: "wrong-private-visual-capability")))));

        Assert.DoesNotContain(
            new[] { "Path", "Address", "Symbol", "Payload", "Hash" },
            fragment => rejected.Diagnostic.Field.Contains(
                fragment,
                StringComparison.OrdinalIgnoreCase) ||
                rejected.Diagnostic.Message.Contains(
                    fragment,
                    StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(PrivateOriginalMapVisualRuntimeDiagnostic).GetProperties(),
            property => property.Name.Contains("Path", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("Address", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("Symbol", StringComparison.OrdinalIgnoreCase));

        PrivateOriginalMapGameSessionStarted traversalOnly =
            Assert.IsType<PrivateOriginalMapGameSessionStarted>(
                GameSession.StartPrivateOriginalMap(
                    new CountingImportSource(_ => ExactImportAccepted()),
                    ImportRequest()));
        Assert.Equal(0, traversalOnly.Session.PrivateOriginalMapSnapshot.SimulationStep);
    }

    private static void AssertVisualBindingRejections(
        IEnumerable<(OriginalMapVisualPayloadAccepted Accepted,
            PrivateOriginalMapVisualRuntimeFailureCode Code)> cases)
    {
        foreach ((OriginalMapVisualPayloadAccepted accepted,
                     PrivateOriginalMapVisualRuntimeFailureCode code) in cases)
        {
            CountingImportSource importSource = new(_ => ExactImportAccepted());
            CountingVisualSource visualSource = new(_ => accepted);

            PrivateOriginalMapVisualGameSessionBindingRejected rejected =
                Assert.IsType<PrivateOriginalMapVisualGameSessionBindingRejected>(
                    Start(importSource, visualSource));

            Assert.Equal(code, rejected.Diagnostic.Code);
            Assert.Equal(1, importSource.AdmitCalls);
            Assert.Equal(1, visualSource.AdmitCalls);
        }
    }

    private static PrivateOriginalMapVisualGameSessionStartResult Start(
        CountingImportSource importSource,
        CountingVisualSource visualSource) =>
        GameSession.StartPrivateOriginalMapWithVisualPayload(
            importSource,
            ImportRequest(),
            visualSource,
            VisualRequest());

    private static OriginalMapImportRequest ImportRequest() =>
        new(
            OriginalMapRuntimeAdmission.PackageId,
            ContentProfile.PrivateLocal,
            OriginalMapRuntimeAdmission.AcceptedContentDigest);

    private static OriginalMapVisualPayloadRequest VisualRequest() =>
        new(
            OriginalMapVisualPayloadAdmission.PackageId,
            ContentProfile.PrivateLocal,
            ExactSelection(),
            OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
            OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
            OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest);

    private static OriginalMapImportAccepted ExactImportAccepted(
        OriginalMapImportDefinition? definition = null,
        OriginalMapImportReceipt? receipt = null) =>
        new(definition ?? ImportDefinition(), receipt ?? ImportReceipt());

    private static OriginalMapVisualPayloadAccepted ExactVisualAccepted(
        OriginalMapVisualPayloadDefinition? definition = null,
        OriginalMapVisualPayloadReceipt? receipt = null) =>
        new(definition ?? VisualDefinition(), receipt ?? VisualReceipt());

    private static OriginalMapImportDefinition ImportDefinition(
        OriginalMapVisualResourceSelection? selection = null)
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        words[Index(
            OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
            OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY)] |=
            OriginalMapTraversal.CollisionMask;
        words[Index(
            OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationX,
            OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationY)] |=
            OriginalMapTraversal.CollisionMask;
        words[Index(3, 3)] = (ushort)(
            (words[Index(3, 3)] & ~OriginalMapTraversal.CollisionMask) |
            OriginalMapTraversal.LeftStairMask);
        words[Index(4, 4)] = (ushort)(
            (words[Index(4, 4)] & ~OriginalMapTraversal.CollisionMask) |
            OriginalMapTraversal.LeftStairMask);
        return new OriginalMapImportDefinition(
            map,
            new WorkingMapLayout(words),
            AcceptedBlockCatalog(),
            AcceptedAreaCatalog(),
            AcceptedEntityPopulation(map),
            selection ?? ExactSelection(),
            new OriginalMapControlledAdmission(
                map,
                new MapPosition(
                    OriginalMapRuntimeAdmission.StartX,
                    OriginalMapRuntimeAdmission.StartY),
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            ControlledStepCopy(map),
            AcceptedSameMapWarps(map),
            ["natural-route-and-effects-unknown"],
            AcceptedRoofOnLoadClear(map),
            BowieDoorStepCopy(map),
            AcceptedZone601(map),
            AcceptedSarah(map));
    }

    private static OriginalMapSameMapWarpCatalog AcceptedSameMapWarps(MapId map) =>
        new(
        [
            SameMapWarp(
                map,
                OriginalMapRuntimeAdmission.SchoolWarpRecordOrdinal,
                OriginalMapRuntimeAdmission.SchoolWarpTriggerX,
                OriginalMapRuntimeAdmission.SchoolWarpTriggerY,
                OriginalMapRuntimeAdmission.SchoolWarpDestinationX,
                OriginalMapRuntimeAdmission.SchoolWarpDestinationY,
                OriginalMapRuntimeAdmission.SchoolWarpOpaqueFacing),
            SameMapWarp(
                map,
                OriginalMapRuntimeAdmission.HouseWarpRecordOrdinal,
                OriginalMapRuntimeAdmission.HouseWarpTriggerX,
                OriginalMapRuntimeAdmission.HouseWarpTriggerY,
                OriginalMapRuntimeAdmission.HouseWarpDestinationX,
                OriginalMapRuntimeAdmission.HouseWarpDestinationY,
                OriginalMapRuntimeAdmission.HouseWarpOpaqueFacing),
        ]);

    private static OriginalMapSameMapWarpDefinition SameMapWarp(
        MapId map,
        int ordinal,
        int triggerX,
        int triggerY,
        int destinationX,
        int destinationY,
        byte opaqueFacing) =>
        new(
            new OriginalMapSameMapWarpIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.SameMapWarpResourceId,
                ordinal),
            new MapPosition(triggerX, triggerY),
            new MapPosition(destinationX, destinationY),
            opaqueFacing);

    private static OriginalMapRoofOnLoadDefinition AcceptedRoofOnLoadClear(MapId map) =>
        new(
            new OriginalMapRoofOnLoadIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.RoofOnLoadResourceId,
                OriginalMapRuntimeAdmission.HouseRoofOnLoadRecordOrdinal),
            new MapPosition(
                OriginalMapRuntimeAdmission.HouseRoofSourceTriggerX,
                OriginalMapRuntimeAdmission.HouseRoofSourceTriggerY),
            new MapPosition(
                OriginalMapRuntimeAdmission.HouseRoofClearDestinationX,
                OriginalMapRuntimeAdmission.HouseRoofClearDestinationY),
            OriginalMapRuntimeAdmission.HouseRoofClearWidth,
            OriginalMapRuntimeAdmission.HouseRoofClearHeight,
            new OriginalMapSameMapWarpIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.SameMapWarpResourceId,
                OriginalMapRuntimeAdmission.HouseWarpRecordOrdinal),
            new OriginalMapAreaRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedAreaResourceId,
                OriginalMapRuntimeAdmission.HouseRoofDestinationAreaOrdinal));

    private static OriginalMapImportReceipt ImportReceipt(
        string? packageId = null,
        int? schemaVersion = null,
        string? romSha256 = null) =>
        new(
            packageId ?? OriginalMapRuntimeAdmission.PackageId,
            schemaVersion ?? OriginalMapRuntimeAdmission.SchemaVersion,
            OriginalMapRuntimeAdmission.AcceptedContentDigest,
            OriginalMapRuntimeAdmission.AcceptedDecodedLayoutDigest,
            OriginalMapRuntimeAdmission.AcceptedCollisionProjectionDigest,
            ContentProfile.PrivateLocal,
            new OriginalMapImportProvenance(
                OriginalMapRuntimeAdmission.PackageId,
                romSha256 ?? OriginalMapRuntimeAdmission.AcceptedRomSha256,
                OriginalMapRuntimeAdmission.AcceptedUpstreamRepository,
                OriginalMapRuntimeAdmission.AcceptedUpstreamCommit),
            OriginalMapRuntimeAdmission.RequiredEvidenceOwners,
            OriginalMapRuntimeAdmission.RequiredCapabilities);

    private static OriginalMapVisualPayloadDefinition VisualDefinition()
    {
        OriginalMapVisualResourceSelection selection = ExactSelection();
        OriginalMapPalettePayload palette = new(selection.PaletteIndex, SourceWords());
        OriginalMapTilesetPayload[] tilesets = Enumerable.Range(0, 5)
            .Select(index => new OriginalMapTilesetPayload(
                index + 1,
                selection.TilesetSlots[index],
                Enumerable.Repeat(
                    checked((byte)(index + 1)),
                    OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset)))
            .ToArray();
        return new OriginalMapVisualPayloadDefinition(selection, palette, tilesets);
    }

    private static OriginalMapVisualPayloadReceipt VisualReceipt(
        string? packageId = null,
        int? schemaVersion = null,
        string? capability = null,
        OriginalMapVisualPayloadProvenance? provenance = null,
        IEnumerable<string>? evidenceOwnerIds = null) =>
        new(
            packageId ?? OriginalMapVisualPayloadAdmission.PackageId,
            schemaVersion ?? OriginalMapVisualPayloadAdmission.SchemaVersion,
            ContentProfile.PrivateLocal,
            capability ?? OriginalMapVisualPayloadAdmission.Capability,
            provenance ?? VisualProvenance(),
            evidenceOwnerIds ?? OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners,
            OriginalMapVisualPayloadAdmission.SelectedPaletteCount,
            OriginalMapVisualPayloadAdmission.PaletteWordCount,
            OriginalMapVisualPayloadAdmission.SelectedTilesetCount,
            OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset);

    private static OriginalMapVisualPayloadProvenance VisualProvenance(
        string? romSha256 = null,
        string? tilesetMetadataDigest = null) =>
        new(
            romSha256 ?? OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
            OriginalMapVisualPayloadAdmission.AcceptedUpstreamRepository,
            OriginalMapVisualPayloadAdmission.AcceptedUpstreamCommit,
            OriginalMapVisualPayloadAdmission.TilesetMetadataId,
            tilesetMetadataDigest ??
                OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
            OriginalMapVisualPayloadAdmission.PaletteMetadataId,
            OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest);

    private static OriginalMapVisualResourceSelection ExactSelection() =>
        new(
            new MapId(OriginalMapRuntimeAdmission.MapId),
            paletteIndex: 0,
            [0, 37, 43, 53, 66]);

    private static OriginalMapEntityPopulation AcceptedEntityPopulation(MapId map) =>
        new(
            map,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            Enumerable.Range(0, OriginalMapRuntimeAdmission.AcceptedEntityRecordCount)
                .Select(index => new OriginalMapEntityDefinition(
                    new OriginalMapEntityRecordIdentity(
                        OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                        index + 1),
                    rawX: index == 0
                        ? (byte)OriginalMapRuntimeAdmission.SarahActorInitialX
                        : index == 2
                        ? (byte)OriginalMapRuntimeAdmission.Zone601ActorInitialX
                        : checked((byte)index),
                    rawY: index == 0
                        ? (byte)OriginalMapRuntimeAdmission.SarahActorInitialY
                        : index == 2
                        ? (byte)OriginalMapRuntimeAdmission.Zone601ActorInitialY
                        : (byte)0,
                    opaqueFacing: index == 0
                        ? OriginalMapRuntimeAdmission.SarahActorInitialOpaqueFacing
                        : index == 2
                        ? OriginalMapRuntimeAdmission.Zone601ActorInitialOpaqueFacing
                        : (byte)3,
                    mapSprite: index == 2 ? (byte)195 : checked((byte)(index + 1)),
                    index == 0
                        ? [0, 4, 0x60, 0xCE]
                        : index == 2
                        ? [0, 4, 97, 2]
                        : index >= OriginalMapRuntimeAdmission.AcceptedFixedEntityRecordCount
                        ? [0xFF, checked((byte)index), 0, 1]
                        : [0, 0, 0, 0])),
            OriginalMapRuntimeAdmission.AcceptedEntityProjectionDigest);

    private static OriginalMapZone601Definition AcceptedZone601(MapId map) =>
        new(
            new OriginalMapZoneEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.Zone601ResourceId,
                OriginalMapRuntimeAdmission.Zone601RecordOrdinal,
                OriginalMapRuntimeAdmission.Zone601TargetIdentity),
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601TriggerX,
                OriginalMapRuntimeAdmission.Zone601TriggerY),
            OriginalMapRuntimeAdmission.Zone601GateFlag,
            OriginalMapRuntimeAdmission.Zone601BlockingSequenceIdentity,
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.Zone601ActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.Zone601LogicalActorId,
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601ActorInitialX,
                OriginalMapRuntimeAdmission.Zone601ActorInitialY),
            OriginalMapRuntimeAdmission.Zone601ActorInitialOpaqueFacing,
            OriginalMapRuntimeAdmission.Zone601ActorInitialBehaviorIdentity,
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601ActorBlockingEndX,
                OriginalMapRuntimeAdmission.Zone601ActorBlockingEndY),
            OriginalMapRuntimeAdmission.Zone601ActorBlockingEndOpaqueFacing,
            OriginalMapRuntimeAdmission.Zone601OpaqueFaceWaitOperand,
            OriginalMapRuntimeAdmission.Zone601TextIds,
            OriginalMapRuntimeAdmission.Zone601AmbientBehaviorIdentity,
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601AmbientCenterX,
                OriginalMapRuntimeAdmission.Zone601AmbientCenterY),
            OriginalMapRuntimeAdmission.Zone601AmbientRange,
            OriginalMapRuntimeAdmission.Zone601BlockingStages);

    private static OriginalMapSarahDefinition AcceptedSarah(MapId map) =>
        new(
            new OriginalMapSarahEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SarahEntityEventResourceId,
                OriginalMapRuntimeAdmission.SarahEntityEventRecordOrdinal,
                OriginalMapRuntimeAdmission.SarahEntityEventTargetIdentity,
                OriginalMapRuntimeAdmission.SarahEntityEventOpaqueFacing),
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.SarahActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.SarahLogicalActorId,
            new MapPosition(
                OriginalMapRuntimeAdmission.SarahActorInitialX,
                OriginalMapRuntimeAdmission.SarahActorInitialY),
            OriginalMapRuntimeAdmission.SarahActorInitialOpaqueFacing,
            new MapPosition(
                OriginalMapRuntimeAdmission.SarahPlayerInteractionX,
                OriginalMapRuntimeAdmission.SarahPlayerInteractionY),
            OriginalMapRuntimeAdmission.SarahPlayerInteractionOpaqueFacing,
            OriginalMapRuntimeAdmission.SarahLaterBranchFlag603,
            OriginalMapRuntimeAdmission.SarahLaterBranchFlag602,
            OriginalMapRuntimeAdmission.SarahTemporaryRouteFlag256,
            OriginalMapRuntimeAdmission.SarahBlockingSequenceIdentity,
            new MapPosition(
                OriginalMapRuntimeAdmission.SarahFirstWaypointX,
                OriginalMapRuntimeAdmission.SarahFirstWaypointY),
            OriginalMapRuntimeAdmission.SarahRestoredOpaqueFacing,
            OriginalMapRuntimeAdmission.SarahFirstTextIds,
            OriginalMapRuntimeAdmission.SarahRepeatTextIds,
            OriginalMapRuntimeAdmission.SarahFirstStages,
            OriginalMapRuntimeAdmission.SarahRepeatStages);

    private static OriginalMapBlockCatalog AcceptedBlockCatalog() =>
        new(
            Enumerable.Range(0, OriginalMapRuntimeAdmission.AcceptedBlockCount)
                .Select(index => new OriginalMapBlockDefinition(
                    new OriginalMapBlockRecordIdentity(
                        OriginalMapRuntimeAdmission.AcceptedBlocksetResourceId,
                        index),
                    new ushort[OriginalMapBlockDefinition.OpaqueWordCount])),
            OriginalMapRuntimeAdmission.AcceptedBlocksetProjectionDigest);

    private static OriginalMapAreaCatalog AcceptedAreaCatalog() =>
        new(AcceptedAreas().Select(
            (area, index) => new OriginalMapAreaDefinition(
                new OriginalMapAreaRecordIdentity(
                    OriginalMapRuntimeAdmission.AcceptedAreaResourceId,
                    index + 1),
                area,
                new OriginalMapAreaWordPair(
                    0,
                    index == 0 ? (ushort)32 : (ushort)0),
                new OriginalMapAreaWordPair(0, 0),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaBytePair(0, 0),
                new OriginalMapAreaBytePair(0, 0),
                mainLayerType: 0,
                defaultMusic: 8)));

    private static OriginalMapTraversalArea[] AcceptedAreas() =>
    [
        new OriginalMapTraversalArea(0, 0, 50, 31),
        new OriginalMapTraversalArea(51, 0, 61, 9),
        new OriginalMapTraversalArea(51, 10, 61, 19),
    ];

    private static OriginalMapStepCopyDefinition ControlledStepCopy(MapId map) =>
        new(
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                OriginalMapRuntimeAdmission.ControlledStepCopyRecordOrdinal),
            new MapPosition(
                OriginalMapRuntimeAdmission.ControlledStepCopyTriggerX,
                OriginalMapRuntimeAdmission.ControlledStepCopyTriggerY),
            new WorkingMapBlockCopy(
                OriginalMapRuntimeAdmission.ControlledStepCopySourceX,
                OriginalMapRuntimeAdmission.ControlledStepCopySourceY,
                OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
                OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY,
                OriginalMapRuntimeAdmission.ControlledStepCopyWidth,
                OriginalMapRuntimeAdmission.ControlledStepCopyHeight));

    private static OriginalMapStepCopyDefinition BowieDoorStepCopy(MapId map) =>
        new(
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyRecordOrdinal),
            new MapPosition(
                OriginalMapRuntimeAdmission.BowieDoorStepCopyTriggerX,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyTriggerY),
            new WorkingMapBlockCopy(
                OriginalMapRuntimeAdmission.BowieDoorStepCopySourceX,
                OriginalMapRuntimeAdmission.BowieDoorStepCopySourceY,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationX,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationY,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyWidth,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyHeight));

    private static ushort[] SourceWords() =>
    [
        0x0EEE, 0x0222, 0x0444, 0x0666,
        0x0888, 0x0AAA, 0x0CCC, 0x0000,
        0x0002, 0x0020, 0x0200, 0x000E,
        0x00E0, 0x0E00, 0x0246, 0x068A,
    ];

    private static int Index(int x, int y) =>
        (y * WorkingMapLayout.ColumnCount) + x;

    private static void SetAutoProperty<T>(T instance, string propertyName, object value)
        where T : class =>
        SetField(instance, $"<{propertyName}>k__BackingField", value);

    private static void SetField<T>(T instance, string fieldName, object value)
        where T : class
    {
        FieldInfo field = typeof(T).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new InvalidOperationException(
                $"Test-only fake port could not find field '{fieldName}'.");
        field.SetValue(instance, value);
    }

    private sealed class CountingImportSource(
        Func<OriginalMapImportRequest, OriginalMapImportResult> admit) :
        IOriginalMapImportSource
    {
        public int AdmitCalls { get; private set; }

        public OriginalMapImportResult Admit(OriginalMapImportRequest request)
        {
            AdmitCalls++;
            return admit(request);
        }
    }

    private sealed class CountingVisualSource(
        Func<OriginalMapVisualPayloadRequest, OriginalMapVisualPayloadResult> admit) :
        IOriginalMapVisualPayloadSource
    {
        public int AdmitCalls { get; private set; }

        public OriginalMapVisualPayloadResult Admit(OriginalMapVisualPayloadRequest request)
        {
            AdmitCalls++;
            return admit(request);
        }
    }

    private sealed record UnknownImportResult : OriginalMapImportResult;

    private sealed record UnknownVisualResult : OriginalMapVisualPayloadResult;
}

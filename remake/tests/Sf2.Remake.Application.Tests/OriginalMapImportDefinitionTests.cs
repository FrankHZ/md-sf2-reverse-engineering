using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class OriginalMapImportDefinitionTests
{
    private const string Digest =
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    [Fact]
    public void DefinitionOwnsAControlledPrivateImportWithoutRuntimeWiring()
    {
        MapId map = new("map3");
        OriginalMapAreaCatalog areaCatalog = AreaCatalog(
            new OriginalMapTraversalArea(0, 0, 63, 63));
        WorkingMapLayout layout = new(new ushort[WorkingMapLayout.WordCount]);
        OriginalMapVisualResourceSelection visualResourceSelection = VisualSelection(map);
        List<string> unsupported = ["natural-route", "presentation"];
        OriginalMapControlledAdmission admission = new(
            map,
            new MapPosition(56, 3),
            opaqueFacing: 3,
            new MapSetupId("ms_map3"),
            "ms_map3_InitFunction",
            noProgramRequest: true);

        OriginalMapImportDefinition definition = new(
            map,
            layout,
            BlockCatalog(),
            areaCatalog,
            visualResourceSelection,
            admission,
            unsupported);
        unsupported.Clear();

        Assert.Same(layout, definition.WorkingLayout);
        Assert.Same(areaCatalog, definition.AreaCatalog);
        Assert.Same(visualResourceSelection, definition.VisualResourceSelection);
        Assert.Same(areaCatalog.Traversal, definition.Traversal);
        Assert.Equal(new MapPosition(56, 3), definition.ControlledAdmission.Position);
        Assert.Equal((byte)3, definition.ControlledAdmission.OpaqueFacing);
        Assert.Equal("ms_map3", definition.ControlledAdmission.SelectedSetup.Value);
        Assert.Equal("ms_map3_InitFunction", definition.ControlledAdmission.SelectedInitIdentity);
        Assert.True(definition.ControlledAdmission.NoProgramRequest);
        Assert.Equal(new[] { "natural-route", "presentation" }, definition.UnsupportedCapabilities);
    }

    [Fact]
    public void DefinitionRejectsMapMismatchBlockedStartAndOpenCapabilityBoundary()
    {
        MapId map = new("map3");
        OriginalMapAreaCatalog areaCatalog = AreaCatalog(
            new OriginalMapTraversalArea(0, 0, 63, 63));
        OriginalMapControlledAdmission admission = Admission(map);
        ushort[] blockedWords = new ushort[WorkingMapLayout.WordCount];
        blockedWords[(3 * WorkingMapLayout.ColumnCount) + 56] =
            OriginalMapTraversal.CollisionMask;

        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                new MapId("other"),
                new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
                BlockCatalog(),
                areaCatalog,
                VisualSelection(map),
                admission,
                ["unknown"]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(blockedWords),
                BlockCatalog(),
                areaCatalog,
                VisualSelection(map),
                admission,
                ["unknown"]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
                BlockCatalog(),
                areaCatalog,
                VisualSelection(map),
                admission,
                []));

        ushort[] missingBlockWords = new ushort[WorkingMapLayout.WordCount];
        missingBlockWords[0] = 3;
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(missingBlockWords),
                BlockCatalog(),
                areaCatalog,
                VisualSelection(map),
                admission,
                ["unknown"]));
    }

    [Fact]
    public void VisualResourceSelectionOwnsFiveOrderedSlotsAndComputesTheAcceptedProjection()
    {
        byte[] slots = [0, 37, 43, 53, 66];
        OriginalMapVisualResourceSelection selection = new(
            new MapId("map3"),
            paletteIndex: 0,
            slots);
        slots[1] = byte.MaxValue;

        Assert.Equal((byte)0, selection.PaletteIndex);
        Assert.Equal(new byte[] { 0, 37, 43, 53, 66 }, selection.TilesetSlots);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedVisualReferenceProjectionDigest,
            selection.ProjectionDigest);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedVisualResourceSelection(selection));
        Assert.Throws<NotSupportedException>(
            () => ((IList<byte>)selection.TilesetSlots).Add(1));
        Assert.Throws<ArgumentException>(() => new OriginalMapVisualResourceSelection(
            new MapId("map3"),
            paletteIndex: 0,
            [0, 37, 43, 53]));
        Assert.Throws<ArgumentException>(() => new OriginalMapVisualResourceSelection(
            new MapId("map3"),
            paletteIndex: 0,
            [0, 37, 43, 53, 66, 67]));
    }

    [Fact]
    public void BlockCatalogDerivesOneResourceAndDefensivelyOwnsOrderedOpaqueWords()
    {
        ushort[] words = Enumerable.Range(0, OriginalMapBlockDefinition.OpaqueWordCount)
            .Select(value => checked((ushort)value))
            .ToArray();
        OriginalMapBlockDefinition original = new(
            new OriginalMapBlockRecordIdentity("project-authored-blocks", 0),
            words);
        List<OriginalMapBlockDefinition> records =
        [
            original,
            new(
                new OriginalMapBlockRecordIdentity("project-authored-blocks", 1),
                new ushort[OriginalMapBlockDefinition.OpaqueWordCount]),
        ];

        OriginalMapBlockCatalog catalog = new(records);
        words[0] = ushort.MaxValue;
        records.Clear();

        Assert.Equal("project-authored-blocks", catalog.ResourceId);
        Assert.Equal(2, catalog.Records.Count);
        Assert.NotSame(original, catalog.Records[0]);
        Assert.Equal((ushort)0, catalog.Records[0].OpaqueWords[0]);
        Assert.Equal(0, catalog.Resolve(0).Identity.ZeroBasedBlockIndex);

        ushort[] layoutWords = new ushort[WorkingMapLayout.WordCount];
        layoutWords[(3 * WorkingMapLayout.ColumnCount) + 2] = 1;
        Assert.Equal(
            1,
            catalog.Resolve(
                new WorkingMapLayout(layoutWords),
                new MapPosition(2, 3)).Identity.ZeroBasedBlockIndex);
    }

    [Fact]
    public void BlockCatalogRejectsEmptyNullMixedNonContiguousAndMalformedRecords()
    {
        Assert.Throws<ArgumentException>(() => new OriginalMapBlockCatalog([]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockCatalog([null!]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockDefinition(
                new OriginalMapBlockRecordIdentity("blocks", 0),
                new ushort[OriginalMapBlockDefinition.OpaqueWordCount - 1]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockCatalog(
            [
                Block("blocks", 0),
                Block("other-blocks", 1),
            ]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockCatalog(
            [
                Block("blocks", 0),
                Block("blocks", 0),
            ]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockCatalog(
            [
                Block("blocks", 0),
                Block("blocks", 2),
            ]));
    }

    [Fact]
    public void AreaCatalogOwnsOneOrderedSourceAndDerivesTheOnlyTraversal()
    {
        List<OriginalMapAreaDefinition> records =
        [
            AreaDefinition(1, new OriginalMapTraversalArea(0, 0, 31, 31)),
            AreaDefinition(2, new OriginalMapTraversalArea(32, 0, 63, 31)),
        ];
        OriginalMapAreaCatalog catalog = new(records);
        records.Clear();

        Assert.Equal("project-authored-area-table", catalog.ResourceId);
        Assert.Equal(2, catalog.Records.Count);
        Assert.Equal(
            catalog.Records[1],
            catalog.Resolve(new OriginalMapTraversalAreaSelection(
                2,
                catalog.Traversal.ActiveAreas[1])));
        Assert.Throws<ArgumentException>(() => new OriginalMapAreaCatalog(
        [
            AreaDefinition(2, new OriginalMapTraversalArea(0, 0, 31, 31)),
        ]));
        Assert.Throws<ArgumentException>(() => new OriginalMapAreaCatalog(
        [
            AreaDefinition(1, new OriginalMapTraversalArea(0, 0, 31, 31)),
            AreaDefinition(
                2,
                new OriginalMapTraversalArea(32, 0, 63, 31),
                "other-area-table"),
        ]));
        Assert.Throws<ArgumentException>(() => catalog.Resolve(
            new OriginalMapTraversalAreaSelection(
                2,
                new OriginalMapTraversalArea(32, 1, 63, 31))));
    }

    [Fact]
    public void RequestAndReceiptKeepDigestProfileAndClosedSetsExact()
    {
        OriginalMapImportRequest request = new(
            "sf2-canonical-map-import-v1",
            ContentProfile.PrivateLocal,
            Digest.ToLowerInvariant());
        List<string> owners = ["owner-a"];
        List<string> capabilities = ["capability-a"];
        OriginalMapImportReceipt receipt = new(
            request.PackageId,
            schemaVersion: 1,
            Digest,
            Digest,
            Digest,
            ContentProfile.PrivateLocal,
            new OriginalMapImportProvenance(
                "sf2-canonical-map-import-v1",
                Digest,
                "https://example.invalid/repository.git",
                "0123456789abcdef0123456789abcdef01234567"),
            owners,
            capabilities);
        owners.Clear();
        capabilities.Clear();

        Assert.Equal(Digest, request.ExpectedContentDigest);
        Assert.Equal(ContentProfile.PrivateLocal, receipt.Profile);
        Assert.Equal(Digest, receipt.ContentDigest);
        Assert.Equal(Digest, receipt.DecodedLayoutDigest);
        Assert.Equal(Digest, receipt.CollisionProjectionDigest);
        Assert.Equal(new[] { "owner-a" }, receipt.EvidenceOwnerIds);
        Assert.Equal(new[] { "capability-a" }, receipt.Capabilities);
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportRequest(
                "package",
                ContentProfile.PrivateLocal,
                "not-a-digest"));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportReceipt(
                "package",
                1,
                Digest,
                Digest,
                Digest,
                ContentProfile.PublicSynthetic,
                receipt.Provenance,
                ["owner"],
                ["capability"]));
    }

    [Fact]
    public void ResultAndDiagnosticRemainTypedAndPathFree()
    {
        OriginalMapImportDiagnostic diagnostic = new(
            OriginalMapImportFailureCode.ContentDigestMismatch,
            "contentDigest",
            "mismatch");
        OriginalMapImportRejected rejected = new(diagnostic);

        Assert.Same(diagnostic, rejected.Diagnostic);
        Assert.DoesNotContain(
            typeof(OriginalMapImportReceipt).GetProperties(),
            property => property.Name.Contains("Path", StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(OriginalMapImportDefinition).GetProperties(),
            property => property.Name.Contains("Path", StringComparison.Ordinal));
    }

    private static OriginalMapControlledAdmission Admission(MapId map) =>
        new(
            map,
            new MapPosition(56, 3),
            opaqueFacing: 3,
            new MapSetupId("ms_map3"),
            "ms_map3_InitFunction",
            noProgramRequest: true);

    private static OriginalMapVisualResourceSelection VisualSelection(MapId map) =>
        new(map, paletteIndex: 0, [0, 37, 43, 53, 66]);

    private static OriginalMapBlockCatalog BlockCatalog(int count = 3) =>
        new(Enumerable.Range(0, count).Select(index => Block(
            "project-authored-blocks",
            index)));

    private static OriginalMapBlockDefinition Block(
        string resourceId,
        int zeroBasedIndex) =>
        new(
            new OriginalMapBlockRecordIdentity(resourceId, zeroBasedIndex),
            new ushort[OriginalMapBlockDefinition.OpaqueWordCount]);

    private static OriginalMapAreaCatalog AreaCatalog(
        params OriginalMapTraversalArea[] activeAreas) =>
        new(activeAreas.Select((area, index) => AreaDefinition(index + 1, area)));

    private static OriginalMapAreaDefinition AreaDefinition(
        int oneBasedRecordOrdinal,
        OriginalMapTraversalArea area,
        string resourceId = "project-authored-area-table") =>
        new(
            new OriginalMapAreaRecordIdentity(resourceId, oneBasedRecordOrdinal),
            area,
            new OriginalMapAreaWordPair(0, 0),
            new OriginalMapAreaWordPair(0, 0),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaBytePair(0, 0),
            new OriginalMapAreaBytePair(0, 0),
            mainLayerType: 0,
            defaultMusic: 0);
}

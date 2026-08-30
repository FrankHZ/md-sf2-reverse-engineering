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
            areaCatalog,
            admission,
            unsupported);
        unsupported.Clear();

        Assert.Same(layout, definition.WorkingLayout);
        Assert.Same(areaCatalog, definition.AreaCatalog);
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
                areaCatalog,
                admission,
                ["unknown"]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(blockedWords),
                areaCatalog,
                admission,
                ["unknown"]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
                areaCatalog,
                admission,
                []));
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

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
        OriginalMapTraversal traversal = new(
            [new OriginalMapTraversalArea(0, 0, 63, 63)]);
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
            traversal,
            admission,
            unsupported);
        unsupported.Clear();

        Assert.Same(layout, definition.WorkingLayout);
        Assert.Same(traversal, definition.Traversal);
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
        OriginalMapTraversal traversal = new(
            [new OriginalMapTraversalArea(0, 0, 63, 63)]);
        OriginalMapControlledAdmission admission = Admission(map);
        ushort[] blockedWords = new ushort[WorkingMapLayout.WordCount];
        blockedWords[(3 * WorkingMapLayout.ColumnCount) + 56] =
            OriginalMapTraversal.CollisionMask;

        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                new MapId("other"),
                new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
                traversal,
                admission,
                ["unknown"]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(blockedWords),
                traversal,
                admission,
                ["unknown"]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
                traversal,
                admission,
                []));
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
}

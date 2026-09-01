using Sf2.Remake.Application.Content;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class LocalPresentationAssetPackDefinitionTests
{
    private const string Commit = "0123456789abcdef0123456789abcdef01234567";
    private const string Digest =
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    [Fact]
    public void ExactPackDefensivelyOwnsOrderedBucketAndAssetCollections()
    {
        LocalPresentationRasterBucket[] buckets = Buckets(8, 6);
        LocalPresentationRasterAssetDefinition first = new(
            "ui.action-confirm",
            new LocalPresentationLogicalSize(8, 6),
            buckets);
        LocalPresentationRasterAssetDefinition[] assets = [first];
        LocalPresentationAssetPackDefinition definition = new(
            LocalPresentationAssetPackAdmission.RepositoryId,
            new LocalPresentationLogicalSize(960, 540),
            assets);
        buckets[0] = buckets[1];
        assets[0] = new LocalPresentationRasterAssetDefinition(
            "other",
            new LocalPresentationLogicalSize(1, 1),
            Buckets(1, 1));

        Assert.Equal("ui.action-confirm", definition.Assets[0].AssetId);
        Assert.Equal(new[] { 2, 4 }, definition.Assets[0].Buckets.Select(item => item.Scale));
        Assert.Equal(new[] { 16, 32 }, definition.Assets[0].Buckets.Select(item => item.Width));
        Assert.Equal("image/png", definition.Assets[0].Buckets[0].MediaType);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<LocalPresentationRasterAssetDefinition>)definition.Assets).Add(first));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<LocalPresentationRasterBucket>)first.Buckets).Add(first.Buckets[0]));
    }

    [Fact]
    public void RequestRequiresCanonicalLowercaseCommitAndNormalizesManifestDigest()
    {
        LocalPresentationAssetPackRequest request = Request(Digest.ToLowerInvariant());

        Assert.Equal(Commit, request.ExpectedAssetRepositoryCommit);
        Assert.Equal(Digest, request.ExpectedManifestDigest);
        Assert.Throws<ArgumentException>(() => Request(Commit.ToUpperInvariant(), Digest));
        Assert.Throws<ArgumentException>(() => Request("not-a-commit", Digest));
        Assert.Throws<ArgumentException>(() => Request(Commit, "not-a-digest"));
    }

    [Fact]
    public void RasterDefinitionRejectsMissingReorderedWrongMediaAndDimensionDrift()
    {
        LocalPresentationLogicalSize logical = new(8, 6);
        LocalPresentationRasterBucket[] exact = Buckets(8, 6);

        Assert.Throws<ArgumentException>(() =>
            new LocalPresentationRasterAssetDefinition("asset", logical, exact.Take(1)));
        Assert.Throws<ArgumentException>(() =>
            new LocalPresentationRasterAssetDefinition("asset", logical, exact.Reverse()));
        Assert.Throws<ArgumentException>(() =>
            new LocalPresentationRasterAssetDefinition(
                "asset",
                logical,
                new[]
                {
                    new LocalPresentationRasterBucket(
                        2, 15, 12, "image/png", "nearest", false, false, "srgb", "straight"),
                    exact[1],
                }));
        Assert.Throws<ArgumentException>(() => new LocalPresentationRasterBucket(
            2, 16, 12, "image/jpeg", "nearest", false, false, "srgb", "straight"));
        Assert.Throws<ArgumentException>(() => new LocalPresentationRasterBucket(
            2, 16, 12, "image/png", "bilinear", false, false, "srgb", "straight"));
        Assert.Throws<ArgumentException>(() => new LocalPresentationRasterBucket(
            2, 16, 12, "image/png", "nearest", false, false, "linear", "straight"));
        Assert.Throws<ArgumentException>(() => new LocalPresentationRasterBucket(
            2, 16, 12, "image/png", "nearest", false, false, "srgb", "premultiplied"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocalPresentationRasterBucket(
            3, 24, 18, "image/png", "nearest", false, false, "srgb", "straight"));
        _ = new LocalPresentationRasterBucket(
            2, 16, 12, "image/png", "linear", false, false, "srgb", "straight");
    }

    [Theory]
    [InlineData("../ui.action-confirm")]
    [InlineData("ui/action-confirm")]
    [InlineData("ui\\action-confirm")]
    [InlineData("C:ui.action-confirm")]
    [InlineData("ui..action-confirm")]
    [InlineData("ui.-action-confirm")]
    [InlineData("UI.action-confirm")]
    [InlineData("ui.action confirm")]
    [InlineData(" ui.action-confirm")]
    public void PathLikeOrNonCanonicalSemanticIdsCannotEnterApplication(string assetId)
    {
        Assert.False(LocalPresentationAssetPackAdmission.IsCanonicalSemanticId(assetId));
        Assert.Throws<ArgumentException>(() => new LocalPresentationRasterAssetDefinition(
            assetId,
            new LocalPresentationLogicalSize(8, 6),
            Buckets(8, 6)));
    }

    [Fact]
    public void PackAndReceiptRejectDuplicateOrShapeDrift()
    {
        LocalPresentationRasterAssetDefinition asset = new(
            "ui.action-confirm",
            new LocalPresentationLogicalSize(8, 6),
            Buckets(8, 6));

        Assert.Throws<ArgumentException>(() => new LocalPresentationAssetPackDefinition(
            LocalPresentationAssetPackAdmission.RepositoryId,
            new LocalPresentationLogicalSize(960, 540),
            new[] { asset, asset }));
        Assert.Throws<ArgumentException>(() => new LocalPresentationAssetPackDefinition(
            "other-repository",
            new LocalPresentationLogicalSize(960, 540),
            new[] { asset }));
        Assert.Throws<ArgumentException>(() => new LocalPresentationAssetPackDefinition(
            LocalPresentationAssetPackAdmission.RepositoryId,
            new LocalPresentationLogicalSize(320, 180),
            new[] { asset }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Receipt(1, 1, new[] { 2, 4 }));
        Assert.Throws<ArgumentException>(() => Receipt(1, 2, new[] { 4, 2 }));
    }

    [Fact]
    public void ReceiptConstructorRejectsEveryV1EnvelopeIdentityDrift()
    {
        Assert.Throws<ArgumentException>(() => ReceiptWith(packageId: "other-package"));
        Assert.Throws<ArgumentOutOfRangeException>(() => ReceiptWith(schemaVersion: 2));
        Assert.Throws<ArgumentException>(() => ReceiptWith(profile: ContentProfile.PublicSynthetic));
        Assert.Throws<ArgumentException>(() => ReceiptWith(capability: "other-capability"));
        Assert.Throws<ArgumentException>(() => ReceiptWith(repositoryId: "other-repository"));
    }

    [Fact]
    public void ApplicationSurfaceCarriesNoFilesystemPathOrRasterPayload()
    {
        Type[] types =
        [
            typeof(LocalPresentationAssetPackDefinition),
            typeof(LocalPresentationRasterAssetDefinition),
            typeof(LocalPresentationRasterBucket),
            typeof(LocalPresentationAssetPackReceipt),
            typeof(LocalPresentationAssetPackDiagnostic),
        ];

        Assert.All(
            types.SelectMany(type => type.GetProperties()),
            property =>
            {
                Assert.DoesNotContain("Path", property.Name, StringComparison.Ordinal);
                Assert.DoesNotContain("Payload", property.Name, StringComparison.Ordinal);
                Assert.DoesNotContain("Bytes", property.Name, StringComparison.Ordinal);
                Assert.DoesNotContain("File", property.Name, StringComparison.Ordinal);
            });
        Assert.DoesNotContain(
            typeof(LocalPresentationAssetPackDefinition).Assembly.GetReferencedAssemblies(),
            assembly => assembly.Name == "Sf2.Remake.Content" ||
                assembly.Name == "GodotSharp" ||
                assembly.Name == "System.Text.Json");
    }

    private static LocalPresentationAssetPackRequest Request(string digest) =>
        Request(Commit, digest);

    private static LocalPresentationAssetPackRequest Request(string commit, string digest) =>
        new(
            LocalPresentationAssetPackAdmission.PackageId,
            ContentProfile.PrivateLocal,
            LocalPresentationAssetPackAdmission.RepositoryId,
            commit,
            digest);

    private static LocalPresentationAssetPackReceipt Receipt(
        int assetCount,
        int bucketCount,
        IEnumerable<int> scales) =>
        new(
            LocalPresentationAssetPackAdmission.PackageId,
            LocalPresentationAssetPackAdmission.SchemaVersion,
            ContentProfile.PrivateLocal,
            LocalPresentationAssetPackAdmission.Capability,
            LocalPresentationAssetPackAdmission.RepositoryId,
            Commit,
            Digest,
            assetCount,
            bucketCount,
            scales);

    private static LocalPresentationAssetPackReceipt ReceiptWith(
        string packageId = LocalPresentationAssetPackAdmission.PackageId,
        int schemaVersion = LocalPresentationAssetPackAdmission.SchemaVersion,
        ContentProfile profile = ContentProfile.PrivateLocal,
        string capability = LocalPresentationAssetPackAdmission.Capability,
        string repositoryId = LocalPresentationAssetPackAdmission.RepositoryId) =>
        new(
            packageId,
            schemaVersion,
            profile,
            capability,
            repositoryId,
            Commit,
            Digest,
            1,
            2,
            new[] { 2, 4 });

    private static LocalPresentationRasterBucket[] Buckets(int width, int height) =>
    [
        new(
            2,
            checked(width * 2),
            checked(height * 2),
            "image/png",
            "nearest",
            false,
            false,
            "srgb",
            "straight"),
        new(
            4,
            checked(width * 4),
            checked(height * 4),
            "image/png",
            "nearest",
            false,
            false,
            "srgb",
            "straight"),
    ];
}

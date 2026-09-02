using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PrivateLocalPresentationAssetCatalogTests
{
    private const string Commit = "0123456789abcdef0123456789abcdef01234567";
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(2.01, 4)]
    [InlineData(4, 4)]
    [InlineData(8, 4)]
    public void AcceptedEffectiveScaleRuleSelectsTwoOrFourX(
        double effectiveScale,
        int expectedBucket)
    {
        Assert.Equal(
            expectedBucket,
            PrivateLocalPresentationAssetCatalog.SelectBucketScale(effectiveScale));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveScaleRejects(double effectiveScale)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PrivateLocalPresentationAssetCatalog.SelectBucketScale(effectiveScale));
    }

    [Theory]
    [InlineData(1920, 1080, 2, 2)]
    [InlineData(3840, 2160, 4, 4)]
    [InlineData(2560, 1080, 2, 2)]
    [InlineData(1280, 960, 1.3333333333333333, 2)]
    [InlineData(5760, 3240, 6, 4)]
    public void SafeSixteenByNineFrameUsesTheLimitingPhysicalDimension(
        int windowWidth,
        int windowHeight,
        double expectedEffectiveScale,
        int expectedBucket)
    {
        double effective = PrivateLocalPresentationAssetCatalog.EffectivePhysicalScale(
            windowWidth,
            windowHeight);

        Assert.Equal(expectedEffectiveScale, effective, precision: 12);
        Assert.Equal(
            expectedBucket,
            PrivateLocalPresentationAssetCatalog.SelectBucketScale(effective));
    }

    [Theory]
    [InlineData(0, 540)]
    [InlineData(960, 0)]
    public void InvalidWindowDimensionsReject(int width, int height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PrivateLocalPresentationAssetCatalog.EffectivePhysicalScale(width, height));
    }

    [Fact]
    public void ExactPackMountsVerifiedTwoXPayloadAndOwnsItsCopy()
    {
        using TemporaryPreviewPack package = new();

        PrivateLocalPresentationAssetMounted mounted =
            Assert.IsType<PrivateLocalPresentationAssetMounted>(
                package.Catalog.MountPreview(
                    package.Request,
                    package.Accepted,
                    effectivePhysicalScale: 1));
        byte[] first = mounted.Asset.CopyPngBytes();
        first[0] = 0;

        Assert.Equal(2, mounted.Asset.Bucket.Scale);
        Assert.Equal(PrivateLocalPresentationAssetCatalog.PreviewLogicalWidth,
            mounted.Asset.Definition.LogicalSize.Width);
        Assert.Equal(137, mounted.Asset.CopyPngBytes()[0]);
        Assert.Equal(new global::Godot.Vector2(800, 27), PrivateLocalHudPreview.PreviewPosition);
        Assert.Equal(new global::Godot.Vector2(112, 24), PrivateLocalHudPreview.PreviewSize);
    }

    [Fact]
    public void ExactPackMountsVerifiedTacticalCursorAndOwnsItsCopy()
    {
        using TemporaryPreviewPack package = new();

        PrivateLocalPresentationAssetMounted mounted =
            Assert.IsType<PrivateLocalPresentationAssetMounted>(
                package.Catalog.MountTacticalCursor(
                    package.Request,
                    package.Accepted,
                    effectivePhysicalScale: 1));
        byte[] first = mounted.Asset.CopyPngBytes();
        first[0] = 0;

        Assert.Equal(PrivateLocalPresentationAssetCatalog.TacticalCursorAssetId,
            mounted.Asset.Definition.AssetId);
        Assert.Equal(2, mounted.Asset.Bucket.Scale);
        Assert.Equal(116, mounted.Asset.Bucket.Width);
        Assert.Equal(116, mounted.Asset.Bucket.Height);
        Assert.Equal(PrivateLocalPresentationAssetCatalog.TacticalCursorLogicalWidth,
            mounted.Asset.Definition.LogicalSize.Width);
        Assert.Equal(137, mounted.Asset.CopyPngBytes()[0]);
    }

    [Fact]
    public void Map3BaseAtlasBindingRequiresExactIdentityDimensionsBucketsAndNearestPolicy()
    {
        LocalPresentationRasterAssetDefinition exact = AtlasDefinition();
        Assert.All(
            exact.Buckets,
            bucket => Assert.True(
                PrivateLocalPresentationAssetCatalog.IsExactMap3BaseAtlasBinding(exact, bucket)));

        LocalPresentationRasterAssetDefinition wrongAsset = AtlasDefinition(assetId: "world.map3.other");
        Assert.False(PrivateLocalPresentationAssetCatalog.IsExactMap3BaseAtlasBinding(
            wrongAsset,
            wrongAsset.Buckets[0]));
        LocalPresentationRasterAssetDefinition wrongSize = AtlasDefinition(width: 127);
        Assert.False(PrivateLocalPresentationAssetCatalog.IsExactMap3BaseAtlasBinding(
            wrongSize,
            wrongSize.Buckets[0]));
        LocalPresentationRasterAssetDefinition wrongDigest = AtlasDefinition(
            twoXDigest: new string('A', 64));
        Assert.False(PrivateLocalPresentationAssetCatalog.IsExactMap3BaseAtlasBinding(
            wrongDigest,
            wrongDigest.Buckets[0]));
        LocalPresentationRasterAssetDefinition wrongFilter = AtlasDefinition(filter: "linear");
        Assert.False(PrivateLocalPresentationAssetCatalog.IsExactMap3BaseAtlasBinding(
            wrongFilter,
            wrongFilter.Buckets[0]));
        LocalPresentationRasterAssetDefinition mipmapped = AtlasDefinition(mipmaps: true);
        Assert.False(PrivateLocalPresentationAssetCatalog.IsExactMap3BaseAtlasBinding(
            mipmapped,
            mipmapped.Buckets[0]));
    }

    [Fact]
    public void GenericPresentationPackCannotSelfAuthorizeTheFixedMap3BaseAtlasTransaction()
    {
        using TemporaryPreviewPack package = new();

        AssertCode(
            package.Catalog.MountMap3BaseAtlas(
                package.Request,
                package.Accepted,
                effectivePhysicalScale: 1),
            PrivateLocalPresentationAssetMountFailureCode.InvalidBinding);
        AssertCode(
            package.Catalog.MountMap3BaseAtlas(
                package.RequestWith(
                    commit: PrivateLocalPresentationAssetCatalog.Map3BaseAtlasAssetRepositoryCommit,
                    manifestDigest: PrivateLocalPresentationAssetCatalog.Map3BaseAtlasManifestDigest),
                package.Accepted,
                effectivePhysicalScale: 1),
            PrivateLocalPresentationAssetMountFailureCode.InvalidBinding);
    }

    [Fact]
    public void ExactLocalMap3BaseAtlasMountCanBeCheckedWithoutBecomingATestInput()
    {
        string? root = Environment.GetEnvironmentVariable(
            "SF2_PRIVATE_PRESENTATION_ASSET_ROOT");
        if (string.IsNullOrWhiteSpace(root))
        {
            return;
        }

        LocalPresentationAssetPackReader reader = new(
            root,
            PrivateLocalPresentationAssetCatalog.Map3BaseAtlasAssetRepositoryCommit);
        LocalPresentationAssetPackRequest request = new(
            LocalPresentationAssetPackAdmission.PackageId,
            ContentProfile.PrivateLocal,
            LocalPresentationAssetPackAdmission.RepositoryId,
            PrivateLocalPresentationAssetCatalog.Map3BaseAtlasAssetRepositoryCommit,
            PrivateLocalPresentationAssetCatalog.Map3BaseAtlasManifestDigest);
        LocalPresentationAssetPackAccepted accepted =
            Assert.IsType<LocalPresentationAssetPackAccepted>(reader.Admit(request));
        PrivateLocalPresentationAssetCatalog catalog = new(reader);

        PrivateLocalPresentationAssetMounted twoX =
            Assert.IsType<PrivateLocalPresentationAssetMounted>(
                catalog.MountMap3BaseAtlas(request, accepted, 1));
        PrivateLocalPresentationAssetMounted fourX =
            Assert.IsType<PrivateLocalPresentationAssetMounted>(
                catalog.MountMap3BaseAtlas(request, accepted, 3));
        byte[] copied = twoX.Asset.CopyPngBytes();
        copied[0] = 0;

        Assert.Equal(2, twoX.Asset.Bucket.Scale);
        Assert.Equal(
            PrivateLocalPresentationAssetCatalog.Map3BaseAtlas2xDigest,
            twoX.Asset.Bucket.Sha256);
        Assert.Equal(4, fourX.Asset.Bucket.Scale);
        Assert.Equal(
            PrivateLocalPresentationAssetCatalog.Map3BaseAtlas4xDigest,
            fourX.Asset.Bucket.Sha256);
        Assert.Equal(137, twoX.Asset.CopyPngBytes()[0]);
    }

    [Fact]
    public void TacticalCursorMissingShapeDriftOrPayloadDriftRejectsFailClosed()
    {
        using TemporaryPreviewPack package = new();

        AssertCode(
            package.Catalog.MountTacticalCursor(
                package.Request,
                package.AcceptedWithoutTacticalCursor(),
                1),
            PrivateLocalPresentationAssetMountFailureCode.AssetUnavailable);
        AssertCode(
            package.Catalog.MountTacticalCursor(
                package.Request,
                package.AcceptedWithTacticalCursorLogicalSize(width: 57, height: 58),
                1),
            PrivateLocalPresentationAssetMountFailureCode.AssetUnavailable);

        File.WriteAllBytes(package.CursorTwoXPath, "mutated"u8.ToArray());
        AssertCode(
            package.Catalog.MountTacticalCursor(package.Request, package.Accepted, 1),
            PrivateLocalPresentationAssetMountFailureCode.PayloadMismatch);
    }

    [Fact]
    public void BattleEntryChoiceProjectionIsPendingOnlyAndChromeFallbackStaysVisible()
    {
        Assert.True(PrivateLocalHudPreview.IsInitiallyVisible(
            battleEntryChoiceEnabled: false));
        Assert.False(PrivateLocalHudPreview.IsInitiallyVisible(
            battleEntryChoiceEnabled: true));
        Assert.Equal("ENTER [N]", PrivateLocalHudPreview.EnterLabel);
        Assert.Equal("STAY", PrivateLocalHudPreview.StayActionLabel);
        Assert.Equal("[BACKSPACE]", PrivateLocalHudPreview.StayKeyLabel);
        Assert.Equal("STAY [BACKSPACE]", PrivateLocalHudPreview.StayLabel);
        Assert.Equal(6, PrivateLocalHudPreview.ChoiceFontSize);

        foreach (PrivateOriginalMapBattleBridgeStatus status in
                 Enum.GetValues<PrivateOriginalMapBattleBridgeStatus>())
        {
            Assert.Equal(
                status == PrivateOriginalMapBattleBridgeStatus.Pending,
                PrivateLocalHudPreview.IsBattleEntryChoiceVisible(status));
        }

        Assert.False(PrivateLocalHudPreview.IsBattleEntryChoiceVisible(status: null));
    }

    [Fact]
    public void ExistingBackActionRoutesOnlyPendingDeclineAndActiveTacticalCancel()
    {
        foreach (PrivateOriginalMapBattleBridgeStatus status in
                 Enum.GetValues<PrivateOriginalMapBattleBridgeStatus>())
        {
            PrivateBattleBridgeBackAction expected = status switch
            {
                PrivateOriginalMapBattleBridgeStatus.Pending =>
                    PrivateBattleBridgeBackAction.DeclineEntry,
                PrivateOriginalMapBattleBridgeStatus.Active =>
                    PrivateBattleBridgeBackAction.CancelTacticalSelection,
                _ => PrivateBattleBridgeBackAction.None,
            };
            Assert.Equal(expected, Map3Root.RoutePrivateBattleBridgeBackAction(status));
        }
    }

    [Fact]
    public void LargerEffectiveScaleSelectsVerifiedFourXPayload()
    {
        using TemporaryPreviewPack package = new();

        PrivateLocalPresentationAssetMounted mounted =
            Assert.IsType<PrivateLocalPresentationAssetMounted>(
                package.Catalog.MountPreview(
                    package.Request,
                    package.Accepted,
                    effectivePhysicalScale: 3));
        PrivateLocalPresentationAssetMounted cursor =
            Assert.IsType<PrivateLocalPresentationAssetMounted>(
                package.Catalog.MountTacticalCursor(
                    package.Request,
                    package.Accepted,
                    effectivePhysicalScale: 3));

        Assert.Equal(4, mounted.Asset.Bucket.Scale);
        Assert.Equal(448, mounted.Asset.Bucket.Width);
        Assert.Equal(96, mounted.Asset.Bucket.Height);
        Assert.Equal(4, cursor.Asset.Bucket.Scale);
        Assert.Equal(232, cursor.Asset.Bucket.Width);
        Assert.Equal(232, cursor.Asset.Bucket.Height);
    }

    [Fact]
    public void PostAdmissionPayloadDriftRejectsWithoutLeakingTheRoot()
    {
        using TemporaryPreviewPack package = new();
        File.WriteAllBytes(package.TwoXPath, "mutated"u8.ToArray());

        PrivateLocalPresentationAssetMountRejected rejected =
            Assert.IsType<PrivateLocalPresentationAssetMountRejected>(
                package.Catalog.MountPreview(
                    package.Request,
                    package.Accepted,
                    effectivePhysicalScale: 1));

        Assert.Equal(
            PrivateLocalPresentationAssetMountFailureCode.PayloadMismatch,
            rejected.Diagnostic.Code);
        Assert.DoesNotContain(package.Root, rejected.Diagnostic.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReceiptPinOrExactAssetDriftRejectsBeforePayloadUse()
    {
        using TemporaryPreviewPack package = new();

        AssertCode(
            package.Catalog.MountPreview(
                package.RequestWith(commit: new string('a', 40)),
                package.Accepted,
                1),
            PrivateLocalPresentationAssetMountFailureCode.InvalidBinding);
        AssertCode(
            package.Catalog.MountPreview(
                package.RequestWith(manifestDigest: new string('A', 64)),
                package.Accepted,
                1),
            PrivateLocalPresentationAssetMountFailureCode.InvalidBinding);

        LocalPresentationAssetPackAccepted wrongAsset = package.AcceptedWithAssetId("hud.other-frame");
        AssertCode(
            package.Catalog.MountPreview(package.Request, wrongAsset, 1),
            PrivateLocalPresentationAssetMountFailureCode.AssetUnavailable);
    }

    [Fact]
    public void ContentOwnedRelativeRootIsUnavailableAndGodotSurfaceOwnsNoPath()
    {
        using TemporaryPreviewPack package = new();
        PrivateLocalPresentationAssetMountRejected rejected =
            Assert.IsType<PrivateLocalPresentationAssetMountRejected>(
                new PrivateLocalPresentationAssetCatalog(
                    new LocalPresentationAssetPackReader("relative/assets", Commit)).MountPreview(
                    package.Request,
                    package.Accepted,
                    1));

        Assert.Equal(
            PrivateLocalPresentationAssetMountFailureCode.AssetUnavailable,
            rejected.Diagnostic.Code);
        Assert.DoesNotContain("relative/assets", rejected.Diagnostic.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(
            typeof(PrivateLocalPresentationAssetCatalog).GetFields(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic),
            field => field.FieldType == typeof(string) ||
                field.Name.Contains("Path", StringComparison.OrdinalIgnoreCase) ||
                field.Name.Contains("Root", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(PrivateLocalPresentationRasterMount).GetProperties(),
            property => property.Name.Contains("Root", StringComparison.Ordinal) ||
                property.Name.Contains("Absolute", StringComparison.Ordinal));
    }

    private static void AssertCode(
        PrivateLocalPresentationAssetMountResult result,
        PrivateLocalPresentationAssetMountFailureCode code) =>
        Assert.Equal(
            code,
            Assert.IsType<PrivateLocalPresentationAssetMountRejected>(result).Diagnostic.Code);

    private static LocalPresentationRasterAssetDefinition AtlasDefinition(
        string assetId = PrivateLocalPresentationAssetCatalog.Map3BaseAtlasAssetId,
        int width = PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalWidth,
        int height = PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalHeight,
        string twoXDigest = PrivateLocalPresentationAssetCatalog.Map3BaseAtlas2xDigest,
        string fourXDigest = PrivateLocalPresentationAssetCatalog.Map3BaseAtlas4xDigest,
        string filter = "nearest",
        bool mipmaps = false) =>
        new(
            assetId,
            new LocalPresentationLogicalSize(width, height),
            [
                new LocalPresentationRasterBucket(
                    2,
                    checked(width * 2),
                    checked(height * 2),
                    100,
                    twoXDigest,
                    "image/png",
                    filter,
                    mipmaps,
                    repeat: false,
                    colorSpace: "srgb",
                    alphaMode: "straight"),
                new LocalPresentationRasterBucket(
                    4,
                    checked(width * 4),
                    checked(height * 4),
                    200,
                    fourXDigest,
                    "image/png",
                    filter,
                    mipmaps,
                    repeat: false,
                    colorSpace: "srgb",
                    alphaMode: "straight"),
            ]);

    private sealed class TemporaryPreviewPack : IDisposable
    {
        private readonly byte[] _twoX = Png(224, 48, 0x24, 0x49, 0x92);
        private readonly byte[] _fourX = Png(448, 96, 0x92, 0x49, 0x00);
        private readonly byte[] _cursorTwoX = Png(116, 116, 0xEA, 0xF8, 0xFF);
        private readonly byte[] _cursorFourX = Png(232, 232, 0x11, 0x18, 0x27);

        public TemporaryPreviewPack()
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                "sf2-private-hud-preview-" + Guid.NewGuid().ToString("N"));
            TwoXPath = Path.Combine(Root, "runtime", "ui", "yes-no-window-frame@2x.png");
            string fourXPath = Path.Combine(
                Root,
                "runtime",
                "ui",
                "yes-no-window-frame@4x.png");
            CursorTwoXPath = Path.Combine(
                Root,
                "runtime",
                "ui",
                "tactical-selection-cursor@2x.png");
            string cursorFourXPath = Path.Combine(
                Root,
                "runtime",
                "ui",
                "tactical-selection-cursor@4x.png");
            Directory.CreateDirectory(Path.GetDirectoryName(TwoXPath)!);
            File.WriteAllBytes(TwoXPath, _twoX);
            File.WriteAllBytes(fourXPath, _fourX);
            File.WriteAllBytes(CursorTwoXPath, _cursorTwoX);
            File.WriteAllBytes(cursorFourXPath, _cursorFourX);
            string manifestPath = Path.Combine(
                Root,
                "manifests",
                "presentation-assets-v1.json");
            Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
            byte[] manifestBytes = Encoding.UTF8.GetBytes(CreateManifest().ToJsonString(
                new JsonSerializerOptions { WriteIndented = true }));
            File.WriteAllBytes(manifestPath, manifestBytes);
            ManifestDigest = Convert.ToHexString(SHA256.HashData(manifestBytes));
            Reader = new LocalPresentationAssetPackReader(Root, Commit);
            Request = RequestWith();
            Accepted = Assert.IsType<LocalPresentationAssetPackAccepted>(Reader.Admit(Request));
            Catalog = new PrivateLocalPresentationAssetCatalog(Reader);
        }

        public string Root { get; }

        public string TwoXPath { get; }

        public string CursorTwoXPath { get; }

        public string ManifestDigest { get; }

        public LocalPresentationAssetPackReader Reader { get; }

        public LocalPresentationAssetPackRequest Request { get; }

        public PrivateLocalPresentationAssetCatalog Catalog { get; }

        public LocalPresentationAssetPackAccepted Accepted { get; }

        public LocalPresentationAssetPackAccepted AcceptedWithAssetId(string assetId)
        {
            LocalPresentationRasterAssetDefinition admitted = Accepted.Definition.Assets[0];
            LocalPresentationRasterAssetDefinition asset = new(
                assetId,
                admitted.LogicalSize,
                admitted.Buckets);
            return new LocalPresentationAssetPackAccepted(
                new LocalPresentationAssetPackDefinition(
                    Accepted.Definition.RepositoryId,
                    Accepted.Definition.LogicalPresentation,
                    [asset]),
                Accepted.Receipt);
        }

        public LocalPresentationAssetPackAccepted AcceptedWithoutTacticalCursor() =>
            new(
                new LocalPresentationAssetPackDefinition(
                    Accepted.Definition.RepositoryId,
                    Accepted.Definition.LogicalPresentation,
                    [Accepted.Definition.Assets[0]]),
                Accepted.Receipt);

        public LocalPresentationAssetPackAccepted AcceptedWithTacticalCursorLogicalSize(
            int width,
            int height)
        {
            LocalPresentationRasterAssetDefinition admitted = Accepted.Definition.Assets[1];
            LocalPresentationRasterBucket[] buckets =
            [
                .. admitted.Buckets.Select(bucket => new LocalPresentationRasterBucket(
                    bucket.Scale,
                    checked(width * bucket.Scale),
                    checked(height * bucket.Scale),
                    bucket.ByteLength,
                    bucket.Sha256,
                    bucket.MediaType,
                    bucket.Filter,
                    bucket.Mipmaps,
                    bucket.Repeat,
                    bucket.ColorSpace,
                    bucket.AlphaMode)),
            ];
            LocalPresentationRasterAssetDefinition cursor = new(
                admitted.AssetId,
                new LocalPresentationLogicalSize(width, height),
                buckets);
            return new LocalPresentationAssetPackAccepted(
                new LocalPresentationAssetPackDefinition(
                    Accepted.Definition.RepositoryId,
                    Accepted.Definition.LogicalPresentation,
                    [Accepted.Definition.Assets[0], cursor]),
                Accepted.Receipt);
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);

        public LocalPresentationAssetPackRequest RequestWith(
            string? commit = null,
            string? manifestDigest = null) =>
            new(
                LocalPresentationAssetPackAdmission.PackageId,
                ContentProfile.PrivateLocal,
                LocalPresentationAssetPackAdmission.RepositoryId,
                commit ?? Commit,
                manifestDigest ?? ManifestDigest);

        private JsonObject CreateManifest() =>
            new()
            {
                ["schemaVersion"] = LocalPresentationAssetPackAdmission.SchemaVersion,
                ["packageId"] = LocalPresentationAssetPackAdmission.PackageId,
                ["repositoryId"] = LocalPresentationAssetPackAdmission.RepositoryId,
                ["profile"] = "private-local",
                ["capabilities"] = new JsonArray(LocalPresentationAssetPackAdmission.Capability),
                ["logicalPresentation"] = new JsonObject
                {
                    ["width"] = 960,
                    ["height"] = 540,
                },
                ["assets"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["assetId"] = PrivateLocalPresentationAssetCatalog.PreviewAssetId,
                        ["kind"] = "raster-image",
                        ["logicalSize"] = new JsonObject
                        {
                            ["width"] = 112,
                            ["height"] = 24,
                        },
                        ["source"] = new JsonObject
                        {
                            ["assetId"] = "source.hud.yes-no-window-frame",
                            ["sha256"] = Convert.ToHexString(SHA256.HashData(
                                "project-authored-source"u8.ToArray())),
                        },
                        ["derivation"] = new JsonObject
                        {
                            ["policyId"] = "project-authored-nearest-v1",
                            ["generatorId"] = "project-authored-test-generator",
                            ["generatorVersion"] = "1",
                            ["generatorArtifactSha256"] = Convert.ToHexString(
                                SHA256.HashData("generator"u8.ToArray())),
                        },
                        ["buckets"] = new JsonArray
                        {
                            Bucket(2, "runtime/ui/yes-no-window-frame@2x.png", 224, 48, _twoX),
                            Bucket(4, "runtime/ui/yes-no-window-frame@4x.png", 448, 96, _fourX),
                        },
                    },
                    new JsonObject
                    {
                        ["assetId"] = PrivateLocalPresentationAssetCatalog.TacticalCursorAssetId,
                        ["kind"] = "raster-image",
                        ["logicalSize"] = new JsonObject
                        {
                            ["width"] = 58,
                            ["height"] = 58,
                        },
                        ["source"] = new JsonObject
                        {
                            ["assetId"] = "source.hud.tactical-selection-cursor",
                            ["sha256"] = Convert.ToHexString(SHA256.HashData(
                                "project-authored-cursor-source"u8.ToArray())),
                        },
                        ["derivation"] = new JsonObject
                        {
                            ["policyId"] = "project-authored-nearest-v1",
                            ["generatorId"] = "project-authored-test-generator",
                            ["generatorVersion"] = "1",
                            ["generatorArtifactSha256"] = Convert.ToHexString(
                                SHA256.HashData("generator"u8.ToArray())),
                        },
                        ["buckets"] = new JsonArray
                        {
                            Bucket(
                                2,
                                "runtime/ui/tactical-selection-cursor@2x.png",
                                116,
                                116,
                                _cursorTwoX),
                            Bucket(
                                4,
                                "runtime/ui/tactical-selection-cursor@4x.png",
                                232,
                                232,
                                _cursorFourX),
                        },
                    },
                },
            };

        private static JsonObject Bucket(
            int scale,
            string runtimePath,
            int width,
            int height,
            byte[] bytes) =>
            new()
            {
                ["scale"] = scale,
                ["runtimePath"] = runtimePath,
                ["width"] = width,
                ["height"] = height,
                ["byteLength"] = bytes.Length,
                ["sha256"] = Convert.ToHexString(SHA256.HashData(bytes)),
                ["mediaType"] = "image/png",
                ["filter"] = "linear",
                ["mipmaps"] = false,
                ["repeat"] = false,
                ["colorSpace"] = "srgb",
                ["alphaMode"] = "straight",
            };
    }

    private static byte[] Png(int width, int height, byte red, byte green, byte blue)
    {
        using MemoryStream png = new();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        byte[] header = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0, 4), (uint)width);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), (uint)height);
        header[8] = 8;
        header[9] = 6;
        WriteChunk(png, "IHDR", header);

        using MemoryStream raw = new();
        byte[] row = new byte[checked(1 + (width * 4))];
        for (int x = 0; x < width; x++)
        {
            int offset = 1 + (x * 4);
            row[offset] = red;
            row[offset + 1] = green;
            row[offset + 2] = blue;
            row[offset + 3] = 255;
        }

        for (int y = 0; y < height; y++)
        {
            raw.Write(row);
        }

        using MemoryStream compressed = new();
        using (ZLibStream zlib = new(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            raw.Position = 0;
            raw.CopyTo(zlib);
        }

        WriteChunk(png, "IDAT", compressed.ToArray());
        WriteChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static void WriteChunk(Stream output, string type, byte[] data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, (uint)data.Length);
        output.Write(length);
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);
        output.Write(typeBytes);
        output.Write(data);
        byte[] crcInput = [.. typeBytes, .. data];
        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32(crcInput));
        output.Write(crc);
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        uint crc = uint.MaxValue;
        foreach (byte value in data)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc >> 1) ^ (0xEDB88320u & (uint)-(int)(crc & 1));
            }
        }

        return ~crc;
    }
}

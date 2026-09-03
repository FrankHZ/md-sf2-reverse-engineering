using System.Buffers.Binary;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;

namespace Sf2.Remake.GodotAdapter;

internal enum PrivateLocalPresentationAssetMountFailureCode
{
    InvalidBinding,
    AssetUnavailable,
    PayloadMismatch,
    TextureRejected,
}

internal sealed record PrivateLocalPresentationAssetMountDiagnostic(
    PrivateLocalPresentationAssetMountFailureCode Code,
    string Message);

internal abstract record PrivateLocalPresentationAssetMountResult;

internal sealed record PrivateLocalPresentationAssetMounted(
    PrivateLocalPresentationRasterMount Asset) : PrivateLocalPresentationAssetMountResult;

internal sealed record PrivateLocalPresentationAssetMountRejected(
    PrivateLocalPresentationAssetMountDiagnostic Diagnostic) :
    PrivateLocalPresentationAssetMountResult;

internal sealed class PrivateLocalPresentationRasterMount
{
    private readonly byte[] _pngBytes;

    internal PrivateLocalPresentationRasterMount(
        LocalPresentationRasterAssetDefinition definition,
        LocalPresentationRasterBucket bucket,
        byte[] pngBytes)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
        ArgumentNullException.ThrowIfNull(pngBytes);
        _pngBytes = [.. pngBytes];
    }

    internal LocalPresentationRasterAssetDefinition Definition { get; }

    internal LocalPresentationRasterBucket Bucket { get; }

    internal byte[] CopyPngBytes() => [.. _pngBytes];
}

internal abstract record PrivateLocalPlayerLocomotionMountResult;

internal sealed record PrivateLocalPlayerLocomotionMounted(
    PrivateLocalPlayerLocomotionMount Animation) :
    PrivateLocalPlayerLocomotionMountResult;

internal sealed record PrivateLocalPlayerLocomotionMountRejected(
    PrivateLocalPresentationAssetMountDiagnostic Diagnostic) :
    PrivateLocalPlayerLocomotionMountResult;

internal sealed record PrivateLocalPlayerLocomotionMount(
    PrivateLocalPresentationRasterMount Up,
    PrivateLocalPresentationRasterMount Horizontal,
    PrivateLocalPresentationRasterMount Down)
{
    internal PrivateLocalPresentationRasterMount Up { get; } =
        Up ?? throw new ArgumentNullException(nameof(Up));

    internal PrivateLocalPresentationRasterMount Horizontal { get; } =
        Horizontal ?? throw new ArgumentNullException(nameof(Horizontal));

    internal PrivateLocalPresentationRasterMount Down { get; } =
        Down ?? throw new ArgumentNullException(nameof(Down));
}

internal sealed class PrivateLocalPresentationAssetCatalog
{
    internal const string PreviewAssetId = "hud.yes-no-window-frame";
    internal const int PreviewLogicalWidth = 112;
    internal const int PreviewLogicalHeight = 24;
    internal const string TacticalCursorAssetId = "hud.tactical-selection-cursor";
    internal const int TacticalCursorLogicalWidth = 58;
    internal const int TacticalCursorLogicalHeight = 58;
    internal const string Map3BaseAtlasAssetId = "world.map3.base-tileset-atlas";
    internal const int Map3BaseAtlasLogicalWidth = 128;
    internal const int Map3BaseAtlasLogicalHeight = 320;
    internal const string Map3PlayerReferenceAssetId =
        "world.map3.player.initial-reference-frame";
    internal const int Map3PlayerReferenceLogicalWidth = 24;
    internal const int Map3PlayerReferenceLogicalHeight = 24;
    internal const string Map3PlayerLocomotionUpAssetId =
        "world.map3.player.locomotion.up";
    internal const string Map3PlayerLocomotionHorizontalAssetId =
        "world.map3.player.locomotion.horizontal";
    internal const string Map3PlayerLocomotionDownAssetId =
        "world.map3.player.locomotion.down";
    internal const int Map3PlayerLocomotionLogicalWidth = 48;
    internal const int Map3PlayerLocomotionLogicalHeight = 24;
    internal const string Map3AssetRepositoryCommit =
        "ec07c0168b9bf684dfc62419351e343baf273c35";
    internal const string Map3AssetManifestDigest =
        "39C18402BECB3CD541C293C2D5274E3B3E7A7932D49A1FDB47AEFB070F7E8151";
    internal const string Map3BaseAtlas2xDigest =
        "E974F59E15E493C29D871574299A46079EBA195BB4CC0B10FF37C2F310682A0A";
    internal const string Map3BaseAtlas4xDigest =
        "04947DE8A163C22699177794E18531E8A8983A2E070D29D346E1A25B8AED6867";
    internal const string Map3PlayerReference2xDigest =
        "2F88562FD4D90ADE537B812D2BFCCC084D1CCF1002575B3CFF89C7B1C2BDFC47";
    internal const string Map3PlayerReference4xDigest =
        "15ED10B9CB5F6EFA7C5E65CBB8726ABE54D8EBF742B35BD764A1B2FAAF2A3E2C";
    internal const string Map3PlayerLocomotionUp2xDigest =
        "F32D9E383A173B84EFFD99FB7FB25A55D1C88D39196EED1FFD2AEC10A94AB31F";
    internal const string Map3PlayerLocomotionUp4xDigest =
        "DA0B5849D1A332F1ACB73BED48273C8790B02F1B2DD00670A2B50F59EDA47B63";
    internal const string Map3PlayerLocomotionHorizontal2xDigest =
        "E4EDDA45328D28E9C84F2E699A8F7FC283739737DB50A618FC53EC49623976FE";
    internal const string Map3PlayerLocomotionHorizontal4xDigest =
        "D1F20ADB4AB8B6A46456390E72375F43E082DE8D84ADF6EE0CB412F4F972782C";
    internal const string Map3PlayerLocomotionDown2xDigest =
        "81A483E7E589F75C1909AA633D03AA548D3A8CE3E8C153763C538116156D0138";
    internal const string Map3PlayerLocomotionDown4xDigest =
        "BDFD78C073E64A957FF147BA385FED891A384111AFB1B2DDF3B795D2E4C91117";

    private static readonly byte[] PngSignature =
        [137, 80, 78, 71, 13, 10, 26, 10];

    private readonly LocalPresentationAssetPackReader _reader;

    internal PrivateLocalPresentationAssetCatalog(LocalPresentationAssetPackReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    internal PrivateLocalPresentationAssetMountResult MountPreview(
        LocalPresentationAssetPackRequest request,
        LocalPresentationAssetPackAccepted accepted,
        double effectivePhysicalScale) =>
        MountRaster(
            request,
            accepted,
            effectivePhysicalScale,
            PreviewAssetId,
            PreviewLogicalWidth,
            PreviewLogicalHeight,
            "private HUD preview");

    internal PrivateLocalPresentationAssetMountResult MountTacticalCursor(
        LocalPresentationAssetPackRequest request,
        LocalPresentationAssetPackAccepted accepted,
        double effectivePhysicalScale) =>
        MountRaster(
            request,
            accepted,
            effectivePhysicalScale,
            TacticalCursorAssetId,
            TacticalCursorLogicalWidth,
            TacticalCursorLogicalHeight,
            "private tactical selection cursor");

    internal PrivateLocalPresentationAssetMountResult MountMap3BaseAtlas(
        LocalPresentationAssetPackRequest request,
        LocalPresentationAssetPackAccepted accepted,
        double effectivePhysicalScale)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(accepted);
        if (!string.Equals(
                request.ExpectedAssetRepositoryCommit,
                Map3AssetRepositoryCommit,
                StringComparison.Ordinal) ||
            !string.Equals(
                request.ExpectedManifestDigest,
                Map3AssetManifestDigest,
                StringComparison.Ordinal))
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The Map 3 base-atlas mount requires the exact accepted local asset transaction.");
        }

        PrivateLocalPresentationAssetMountResult result = MountRaster(
            request,
            accepted,
            effectivePhysicalScale,
            Map3BaseAtlasAssetId,
            Map3BaseAtlasLogicalWidth,
            Map3BaseAtlasLogicalHeight,
            "private Map 3 base atlas");
        if (result is not PrivateLocalPresentationAssetMounted mounted)
        {
            return result;
        }

        LocalPresentationRasterBucket bucket = mounted.Asset.Bucket;
        if (!IsExactMap3BaseAtlasBinding(mounted.Asset.Definition, bucket))
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.PayloadMismatch,
                "The Map 3 base-atlas bucket identity or nearest-sampling policy drifted.");
        }

        return mounted;
    }

    internal PrivateLocalPresentationAssetMountResult MountMap3PlayerReference(
        LocalPresentationAssetPackRequest request,
        LocalPresentationAssetPackAccepted accepted,
        double effectivePhysicalScale)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(accepted);
        if (!string.Equals(
                request.ExpectedAssetRepositoryCommit,
                Map3AssetRepositoryCommit,
                StringComparison.Ordinal) ||
            !string.Equals(
                request.ExpectedManifestDigest,
                Map3AssetManifestDigest,
                StringComparison.Ordinal))
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The Map 3 player reference mount requires the exact accepted local asset transaction.");
        }

        PrivateLocalPresentationAssetMountResult result = MountRaster(
            request,
            accepted,
            effectivePhysicalScale,
            Map3PlayerReferenceAssetId,
            Map3PlayerReferenceLogicalWidth,
            Map3PlayerReferenceLogicalHeight,
            "private Map 3 player initial reference frame");
        if (result is not PrivateLocalPresentationAssetMounted mounted)
        {
            return result;
        }

        if (!IsExactMap3PlayerReferenceBinding(
                mounted.Asset.Definition,
                mounted.Asset.Bucket))
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.PayloadMismatch,
                "The Map 3 player reference bucket identity or nearest-sampling policy drifted.");
        }

        return mounted;
    }

    internal PrivateLocalPlayerLocomotionMountResult MountMap3PlayerLocomotion(
        LocalPresentationAssetPackRequest request,
        LocalPresentationAssetPackAccepted accepted,
        double effectivePhysicalScale)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(accepted);
        if (!string.Equals(
                request.ExpectedAssetRepositoryCommit,
                Map3AssetRepositoryCommit,
                StringComparison.Ordinal) ||
            !string.Equals(
                request.ExpectedManifestDigest,
                Map3AssetManifestDigest,
                StringComparison.Ordinal))
        {
            return RejectLocomotion(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The Map 3 player locomotion mount requires the exact accepted local asset transaction.");
        }

        PrivateLocalPresentationAssetMountResult upResult = MountRaster(
            request,
            accepted,
            effectivePhysicalScale,
            Map3PlayerLocomotionUpAssetId,
            Map3PlayerLocomotionLogicalWidth,
            Map3PlayerLocomotionLogicalHeight,
            "private Map 3 player up locomotion sheet");
        if (upResult is not PrivateLocalPresentationAssetMounted mountedUp)
        {
            return RejectLocomotion((PrivateLocalPresentationAssetMountRejected)upResult);
        }

        PrivateLocalPresentationAssetMountResult horizontalResult = MountRaster(
            request,
            accepted,
            effectivePhysicalScale,
            Map3PlayerLocomotionHorizontalAssetId,
            Map3PlayerLocomotionLogicalWidth,
            Map3PlayerLocomotionLogicalHeight,
            "private Map 3 player horizontal locomotion sheet");
        if (horizontalResult is not PrivateLocalPresentationAssetMounted mountedHorizontal)
        {
            return RejectLocomotion(
                (PrivateLocalPresentationAssetMountRejected)horizontalResult);
        }

        PrivateLocalPresentationAssetMountResult downResult = MountRaster(
            request,
            accepted,
            effectivePhysicalScale,
            Map3PlayerLocomotionDownAssetId,
            Map3PlayerLocomotionLogicalWidth,
            Map3PlayerLocomotionLogicalHeight,
            "private Map 3 player down locomotion sheet");
        if (downResult is not PrivateLocalPresentationAssetMounted mountedDown)
        {
            return RejectLocomotion((PrivateLocalPresentationAssetMountRejected)downResult);
        }

        PrivateLocalPresentationRasterMount[] sheets =
            [mountedUp.Asset, mountedHorizontal.Asset, mountedDown.Asset];
        if (sheets.Select(sheet => sheet.Bucket.Scale).Distinct().Count() != 1 ||
            sheets.Any(sheet => !IsExactMap3PlayerLocomotionBinding(
                sheet.Definition,
                sheet.Bucket)))
        {
            return RejectLocomotion(
                PrivateLocalPresentationAssetMountFailureCode.PayloadMismatch,
                "The Map 3 player locomotion sheets do not retain one exact bucket and asset family.");
        }

        return new PrivateLocalPlayerLocomotionMounted(
            new PrivateLocalPlayerLocomotionMount(
                mountedUp.Asset,
                mountedHorizontal.Asset,
                mountedDown.Asset));
    }

    internal static bool IsExactMap3BaseAtlasBinding(
        LocalPresentationRasterAssetDefinition definition,
        LocalPresentationRasterBucket bucket)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(bucket);
        string? expectedDigest = bucket.Scale switch
        {
            2 => Map3BaseAtlas2xDigest,
            4 => Map3BaseAtlas4xDigest,
            _ => null,
        };
        return string.Equals(
                definition.AssetId,
                Map3BaseAtlasAssetId,
                StringComparison.Ordinal) &&
            definition.LogicalSize.Width == Map3BaseAtlasLogicalWidth &&
            definition.LogicalSize.Height == Map3BaseAtlasLogicalHeight &&
            expectedDigest is not null &&
            string.Equals(bucket.Sha256, expectedDigest, StringComparison.Ordinal) &&
            string.Equals(bucket.Filter, "nearest", StringComparison.Ordinal) &&
            !bucket.Mipmaps &&
            !bucket.Repeat;
    }

    internal static bool IsExactMap3PlayerReferenceBinding(
        LocalPresentationRasterAssetDefinition definition,
        LocalPresentationRasterBucket bucket)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(bucket);
        string? expectedDigest = bucket.Scale switch
        {
            2 => Map3PlayerReference2xDigest,
            4 => Map3PlayerReference4xDigest,
            _ => null,
        };
        return string.Equals(
                definition.AssetId,
                Map3PlayerReferenceAssetId,
                StringComparison.Ordinal) &&
            definition.LogicalSize.Width == Map3PlayerReferenceLogicalWidth &&
            definition.LogicalSize.Height == Map3PlayerReferenceLogicalHeight &&
            expectedDigest is not null &&
            string.Equals(bucket.Sha256, expectedDigest, StringComparison.Ordinal) &&
            string.Equals(bucket.Filter, "nearest", StringComparison.Ordinal) &&
            !bucket.Mipmaps &&
            !bucket.Repeat;
    }

    internal static bool IsExactMap3PlayerLocomotionBinding(
        LocalPresentationRasterAssetDefinition definition,
        LocalPresentationRasterBucket bucket)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(bucket);
        string? expectedDigest = (definition.AssetId, bucket.Scale) switch
        {
            (Map3PlayerLocomotionUpAssetId, 2) => Map3PlayerLocomotionUp2xDigest,
            (Map3PlayerLocomotionUpAssetId, 4) => Map3PlayerLocomotionUp4xDigest,
            (Map3PlayerLocomotionHorizontalAssetId, 2) =>
                Map3PlayerLocomotionHorizontal2xDigest,
            (Map3PlayerLocomotionHorizontalAssetId, 4) =>
                Map3PlayerLocomotionHorizontal4xDigest,
            (Map3PlayerLocomotionDownAssetId, 2) => Map3PlayerLocomotionDown2xDigest,
            (Map3PlayerLocomotionDownAssetId, 4) => Map3PlayerLocomotionDown4xDigest,
            _ => null,
        };
        return definition.LogicalSize.Width == Map3PlayerLocomotionLogicalWidth &&
            definition.LogicalSize.Height == Map3PlayerLocomotionLogicalHeight &&
            expectedDigest is not null &&
            string.Equals(bucket.Sha256, expectedDigest, StringComparison.Ordinal) &&
            string.Equals(bucket.Filter, "nearest", StringComparison.Ordinal) &&
            !bucket.Mipmaps &&
            !bucket.Repeat;
    }

    private static PrivateLocalPlayerLocomotionMountRejected RejectLocomotion(
        PrivateLocalPresentationAssetMountRejected rejected) =>
        new(rejected.Diagnostic);

    private static PrivateLocalPlayerLocomotionMountRejected RejectLocomotion(
        PrivateLocalPresentationAssetMountFailureCode code,
        string message) =>
        new(new PrivateLocalPresentationAssetMountDiagnostic(code, message));

    private PrivateLocalPresentationAssetMountResult MountRaster(
        LocalPresentationAssetPackRequest request,
        LocalPresentationAssetPackAccepted accepted,
        double effectivePhysicalScale,
        string assetId,
        int logicalWidth,
        int logicalHeight,
        string assetDescription)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(accepted);
        if (!string.Equals(
                request.PackageId,
                LocalPresentationAssetPackAdmission.PackageId,
                StringComparison.Ordinal) ||
            request.Profile != ContentProfile.PrivateLocal ||
            !string.Equals(
                request.ExpectedRepositoryId,
                LocalPresentationAssetPackAdmission.RepositoryId,
                StringComparison.Ordinal) ||
            !string.Equals(
                accepted.Receipt.PackageId,
                LocalPresentationAssetPackAdmission.PackageId,
                StringComparison.Ordinal) ||
            accepted.Receipt.SchemaVersion != LocalPresentationAssetPackAdmission.SchemaVersion ||
            accepted.Receipt.Profile != ContentProfile.PrivateLocal ||
            !string.Equals(
                accepted.Receipt.Capability,
                LocalPresentationAssetPackAdmission.Capability,
                StringComparison.Ordinal) ||
            !string.Equals(
                accepted.Receipt.RepositoryId,
                LocalPresentationAssetPackAdmission.RepositoryId,
                StringComparison.Ordinal) ||
            !string.Equals(
                accepted.Receipt.MountedAssetRepositoryCommit,
                request.ExpectedAssetRepositoryCommit,
                StringComparison.Ordinal) ||
            !string.Equals(
                accepted.Receipt.ManifestDigest,
                request.ExpectedManifestDigest,
                StringComparison.Ordinal) ||
            !string.Equals(
                accepted.Definition.RepositoryId,
                LocalPresentationAssetPackAdmission.RepositoryId,
                StringComparison.Ordinal) ||
            accepted.Definition.LogicalPresentation.Width !=
                LocalPresentationAssetPackAdmission.LogicalWidth ||
            accepted.Definition.LogicalPresentation.Height !=
                LocalPresentationAssetPackAdmission.LogicalHeight)
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The local presentation pack binding is incompatible with the requested mount.");
        }

        LocalPresentationRasterAssetDefinition[] matches =
        [
            .. accepted.Definition.Assets.Where(asset => string.Equals(
                asset.AssetId,
                assetId,
                StringComparison.Ordinal)),
        ];
        if (matches.Length != 1 ||
            matches[0].LogicalSize.Width != logicalWidth ||
            matches[0].LogicalSize.Height != logicalHeight)
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.AssetUnavailable,
                $"The exact {assetDescription} asset is unavailable.");
        }

        int scale = SelectBucketScale(effectivePhysicalScale);
        LocalPresentationRasterBucket[] buckets =
        [.. matches[0].Buckets.Where(bucket => bucket.Scale == scale)];
        if (buckets.Length != 1)
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.AssetUnavailable,
                $"The required {assetDescription} bucket is unavailable.");
        }

        LocalPresentationRasterBucket selected = buckets[0];
        LocalPresentationRasterPayloadResult payloadResult = _reader.ReadRaster(
            request,
            accepted,
            assetId,
            scale);
        if (payloadResult is not LocalPresentationRasterPayloadAccepted payload)
        {
            LocalPresentationRasterPayloadRejected rejected =
                (LocalPresentationRasterPayloadRejected)payloadResult;
            return Reject(
                rejected.Diagnostic.Code switch
                {
                    LocalPresentationAssetPackFailureCode.PayloadMismatch or
                        LocalPresentationAssetPackFailureCode.ContentDigestMismatch =>
                        PrivateLocalPresentationAssetMountFailureCode.PayloadMismatch,
                    LocalPresentationAssetPackFailureCode.PackageUnavailable or
                        LocalPresentationAssetPackFailureCode.AssetPathRejected or
                        LocalPresentationAssetPackFailureCode.InvalidBucket or
                        LocalPresentationAssetPackFailureCode.MissingBucket =>
                        PrivateLocalPresentationAssetMountFailureCode.AssetUnavailable,
                    _ => PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                },
                "The Content-owned local presentation payload lookup was rejected.");
        }

        if (!string.Equals(payload.AssetId, assetId, StringComparison.Ordinal) ||
            payload.Scale != scale)
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The Content-owned local presentation payload lookup returned an incompatible semantic selection.");
        }

        byte[] bytes = payload.CopyBytes();
        if (!TryReadPngDimensions(bytes!, out int width, out int height) ||
            width != selected.Width ||
            height != selected.Height)
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.TextureRejected,
                $"The {assetDescription} payload is not the admitted PNG shape.");
        }

        return new PrivateLocalPresentationAssetMounted(
            new PrivateLocalPresentationRasterMount(matches[0], selected, bytes));
    }

    internal static int SelectBucketScale(double effectivePhysicalScale)
    {
        if (!double.IsFinite(effectivePhysicalScale) || effectivePhysicalScale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(effectivePhysicalScale));
        }

        return effectivePhysicalScale <= 2 ? 2 : 4;
    }

    internal static double EffectivePhysicalScale(int windowWidth, int windowHeight)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(windowWidth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(windowHeight, 1);
        return Math.Min(
            windowWidth / (double)LocalPresentationAssetPackAdmission.LogicalWidth,
            windowHeight / (double)LocalPresentationAssetPackAdmission.LogicalHeight);
    }

    private static bool TryReadPngDimensions(
        byte[] bytes,
        out int width,
        out int height)
    {
        width = 0;
        height = 0;
        if (bytes.Length < 33 ||
            !bytes.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature) ||
            BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(8, 4)) != 13 ||
            !bytes.AsSpan(12, 4).SequenceEqual("IHDR"u8))
        {
            return false;
        }

        uint pngWidth = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16, 4));
        uint pngHeight = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20, 4));
        if (pngWidth is 0 or > int.MaxValue || pngHeight is 0 or > int.MaxValue)
        {
            return false;
        }

        width = (int)pngWidth;
        height = (int)pngHeight;
        return true;
    }

    private static PrivateLocalPresentationAssetMountRejected Reject(
        PrivateLocalPresentationAssetMountFailureCode code,
        string message) =>
        new(new PrivateLocalPresentationAssetMountDiagnostic(code, message));
}

internal sealed partial class PrivateLocalHudPreview : Control
{
    internal const string EnterLabel = "ENTER [N]";
    internal const string StayActionLabel = "STAY";
    internal const string StayKeyLabel = "[BACKSPACE]";
    internal const string StayLabel = StayActionLabel + " " + StayKeyLabel;
    internal const int ChoiceFontSize = 6;
    internal static readonly Vector2 PreviewPosition = new(800, 27);
    internal static readonly Vector2 PreviewSize = new(
        PrivateLocalPresentationAssetCatalog.PreviewLogicalWidth,
        PrivateLocalPresentationAssetCatalog.PreviewLogicalHeight);

    private ImageTexture? _ownedTexture;
    private readonly bool _battleEntryChoiceEnabled;

    private PrivateLocalHudPreview(bool battleEntryChoiceEnabled)
    {
        _battleEntryChoiceEnabled = battleEntryChoiceEnabled;
    }

    internal bool BattleEntryChoiceEnabled => _battleEntryChoiceEnabled;

    internal static PrivateLocalHudPreview? TryAttach(
        Node parent,
        PrivateLocalPresentationRasterMount mount,
        bool battleEntryChoiceEnabled,
        out PrivateLocalPresentationAssetMountDiagnostic? diagnostic)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(mount);
        diagnostic = null;
        Image image = new();
        Error error = image.LoadPngFromBuffer(mount.CopyPngBytes());
        if (error != Error.Ok ||
            image.GetWidth() != mount.Bucket.Width ||
            image.GetHeight() != mount.Bucket.Height)
        {
            image.Dispose();
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.TextureRejected,
                "Godot rejected the admitted private HUD preview texture.");
            return null;
        }

        ImageTexture texture = ImageTexture.CreateFromImage(image);
        image.Dispose();
        PrivateLocalHudPreview preview = new(battleEntryChoiceEnabled)
        {
            Name = battleEntryChoiceEnabled
                ? "PrivateLocalBattleEntryChoicePanel"
                : "PrivateLocalHudYesNoWindowFramePreview",
            Position = PreviewPosition,
            Size = PreviewSize,
            MouseFilter = MouseFilterEnum.Ignore,
            ZIndex = 500,
            Visible = IsInitiallyVisible(battleEntryChoiceEnabled),
            ClipContents = true,
        };
        preview._ownedTexture = texture;
        TextureRect frame = new()
        {
            Name = "Frame",
            Position = Vector2.Zero,
            Size = PreviewSize,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore,
            TextureFilter = string.Equals(
                mount.Bucket.Filter,
                "nearest",
                StringComparison.Ordinal)
                ? CanvasItem.TextureFilterEnum.Nearest
                : CanvasItem.TextureFilterEnum.Linear,
            Texture = texture,
        };
        preview.AddChild(frame);
        if (battleEntryChoiceEnabled)
        {
            preview.AddChoiceLabel(
                "Enter",
                EnterLabel,
                x: 4,
                y: 4,
                width: 40,
                height: 16);
            preview.AddChoiceLabel(
                "Stay",
                StayActionLabel,
                x: 44,
                y: 0,
                width: 64,
                height: 10);
            preview.AddChoiceLabel(
                "StayKey",
                StayKeyLabel,
                x: 44,
                y: 6,
                width: 64,
                height: 10);
        }

        parent.AddChild(preview);
        return preview;
    }

    internal void ProjectBattleEntryChoice(
        PrivateOriginalMapBattleBridgeSnapshot? bridge)
    {
        if (!_battleEntryChoiceEnabled)
        {
            return;
        }

        Visible = IsBattleEntryChoiceVisible(bridge?.Status);
    }

    internal static bool IsBattleEntryChoiceVisible(
        PrivateOriginalMapBattleBridgeStatus? status) =>
        status == PrivateOriginalMapBattleBridgeStatus.Pending;

    internal static bool IsInitiallyVisible(bool battleEntryChoiceEnabled) =>
        !battleEntryChoiceEnabled;

    private void AddChoiceLabel(
        string name,
        string text,
        float x,
        float y,
        float width,
        float height)
    {
        Label label = new()
        {
            Name = name,
            Text = text,
            Position = new Vector2(x, y),
            Size = new Vector2(width, height),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        label.AddThemeFontSizeOverride("font_size", ChoiceFontSize);
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 1);
        AddChild(label);
    }
}

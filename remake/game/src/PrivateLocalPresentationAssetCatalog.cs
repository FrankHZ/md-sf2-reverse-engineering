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

internal sealed class PrivateLocalPresentationAssetCatalog
{
    internal const string PreviewAssetId = "hud.yes-no-window-frame";
    internal const int PreviewLogicalWidth = 112;
    internal const int PreviewLogicalHeight = 24;

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
        double effectivePhysicalScale)
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
                PreviewAssetId,
                StringComparison.Ordinal)),
        ];
        if (matches.Length != 1 ||
            matches[0].LogicalSize.Width != PreviewLogicalWidth ||
            matches[0].LogicalSize.Height != PreviewLogicalHeight)
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.AssetUnavailable,
                "The exact private HUD preview asset is unavailable.");
        }

        int scale = SelectBucketScale(effectivePhysicalScale);
        LocalPresentationRasterBucket[] buckets =
        [.. matches[0].Buckets.Where(bucket => bucket.Scale == scale)];
        if (buckets.Length != 1)
        {
            return Reject(
                PrivateLocalPresentationAssetMountFailureCode.AssetUnavailable,
                "The required private HUD preview bucket is unavailable.");
        }

        LocalPresentationRasterBucket selected = buckets[0];
        LocalPresentationRasterPayloadResult payloadResult = _reader.ReadRaster(
            request,
            accepted,
            PreviewAssetId,
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

        if (!string.Equals(payload.AssetId, PreviewAssetId, StringComparison.Ordinal) ||
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
                "The private HUD preview payload is not the admitted PNG shape.");
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

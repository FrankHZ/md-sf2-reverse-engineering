using System.Collections.ObjectModel;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal sealed record PrivateOriginalMapBaseViewProjection
{
    public const int ColumnCount = 12;
    public const int RowCount = 7;
    public const int TilePixelSize = 8;
    public const int BlockTileSide = 3;
    public const int BlockPixelSize = TilePixelSize * BlockTileSide;
    public const int PixelWidth = ColumnCount * BlockPixelSize;
    public const int PixelHeight = RowCount * BlockPixelSize;

    private const int TileBytes = 32;
    private const int TilesPerSlot = 128;
    private const ushort TileIndexMask = 0x03FF;
    private const ushort TileIndexOffset = 0x0100;
    private const ushort VerticalFlip = 0x1000;
    private const ushort HorizontalMirror = 0x0800;

    private readonly ReadOnlyCollection<byte> _rgbaBytes;

    private readonly record struct SourcePixel(byte Red, byte Green, byte Blue, byte Alpha);

    private PrivateOriginalMapBaseViewProjection(
        MapId map,
        int originX,
        int originY,
        int playerColumn,
        int playerRow,
        IEnumerable<byte> rgbaBytes)
    {
        Map = map;
        OriginX = originX;
        OriginY = originY;
        PlayerColumn = playerColumn;
        PlayerRow = playerRow;
        _rgbaBytes = Array.AsReadOnly(rgbaBytes.ToArray());
    }

    internal MapId Map { get; }

    internal int OriginX { get; }

    internal int OriginY { get; }

    internal int PlayerColumn { get; }

    internal int PlayerRow { get; }

    internal IReadOnlyList<byte> RgbaBytes => _rgbaBytes;

    internal static PrivateOriginalMapBaseViewProjection Create(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualPayloadDefinition visualDefinition)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(visualDefinition);
        if (!SameSelection(
                snapshot.Definition.VisualResourceSelection,
                visualDefinition.Selection))
        {
            throw new ArgumentException(
                "The private map snapshot and visual payload must retain the same admitted selection.",
                nameof(visualDefinition));
        }

        return CreateCore(
            snapshot,
            visualDefinition.Selection,
            (slot, localTile, row, column) =>
                ResolvePayloadPixel(visualDefinition, slot, localTile, row, column));
    }

    internal static PrivateOriginalMapBaseViewProjection CreateFromAtlas(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualResourceSelection selection,
        IReadOnlyList<byte> atlasRgbaBytes,
        int scale)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(atlasRgbaBytes);
        if (!LocalPresentationAssetPackAdmission.BucketScales.Contains(scale))
        {
            throw new ArgumentOutOfRangeException(nameof(scale));
        }

        int atlasWidth = checked(PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalWidth * scale);
        int atlasHeight = checked(PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalHeight * scale);
        if (atlasRgbaBytes.Count != checked(atlasWidth * atlasHeight * 4))
        {
            throw new ArgumentException(
                "The private Map 3 base-atlas RGBA payload shape drifted.",
                nameof(atlasRgbaBytes));
        }

        return CreateCore(
            snapshot,
            selection,
            (slot, localTile, row, column) => ResolveAtlasPixel(
                atlasRgbaBytes,
                scale,
                atlasWidth,
                slot,
                localTile,
                row,
                column));
    }

    private static PrivateOriginalMapBaseViewProjection CreateCore(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualResourceSelection selection,
        Func<int, int, int, int, SourcePixel> resolvePixel)
    {
        if (!SameSelection(snapshot.Definition.VisualResourceSelection, selection))
        {
            throw new ArgumentException(
                "The private map snapshot and visual payload must retain the same admitted selection.",
                nameof(selection));
        }

        int originX = Math.Clamp(
            snapshot.PlayerPosition.X - (ColumnCount / 2),
            0,
            WorkingMapLayout.ColumnCount - ColumnCount);
        int originY = Math.Clamp(
            snapshot.PlayerPosition.Y - (RowCount / 2),
            0,
            WorkingMapLayout.RowCount - RowCount);
        byte[] pixels = new byte[PixelWidth * PixelHeight * 4];
        FillBackground(pixels);

        for (int blockRow = 0; blockRow < RowCount; blockRow++)
        {
            for (int blockColumn = 0; blockColumn < ColumnCount; blockColumn++)
            {
                OriginalMapBlockDefinition block = snapshot.Definition.BlockCatalog.Resolve(
                    snapshot.WorkingLayout,
                    new MapPosition(originX + blockColumn, originY + blockRow));
                RenderBlock(
                    pixels,
                    blockColumn,
                    blockRow,
                    block,
                    resolvePixel);
            }
        }

        return new PrivateOriginalMapBaseViewProjection(
            snapshot.Map,
            originX,
            originY,
            snapshot.PlayerPosition.X - originX,
            snapshot.PlayerPosition.Y - originY,
            pixels);
    }

    private static void RenderBlock(
        byte[] pixels,
        int blockColumn,
        int blockRow,
        OriginalMapBlockDefinition block,
        Func<int, int, int, int, SourcePixel> resolvePixel)
    {
        for (int tileRow = 0; tileRow < BlockTileSide; tileRow++)
        {
            for (int tileColumn = 0; tileColumn < BlockTileSide; tileColumn++)
            {
                ushort word = block.OpaqueWords[(tileRow * BlockTileSide) + tileColumn];
                int tileNumber = word & TileIndexMask;
                if (tileNumber < TileIndexOffset)
                {
                    continue;
                }

                int selectedIndex = tileNumber - TileIndexOffset;
                int slot = selectedIndex / TilesPerSlot;
                int localTile = selectedIndex % TilesPerSlot;
                if (slot >= OriginalMapVisualResourceSelection.TilesetSlotCount)
                {
                    continue;
                }

                for (int pixelRow = 0; pixelRow < TilePixelSize; pixelRow++)
                {
                    for (int pixelColumn = 0; pixelColumn < TilePixelSize; pixelColumn++)
                    {
                        int sourceRow = (word & VerticalFlip) != 0
                            ? (TilePixelSize - 1) - pixelRow
                            : pixelRow;
                        int sourceColumn = (word & HorizontalMirror) != 0
                            ? (TilePixelSize - 1) - pixelColumn
                            : pixelColumn;
                        SourcePixel source = resolvePixel(
                            slot,
                            localTile,
                            sourceRow,
                            sourceColumn);
                        if (source.Alpha == 0)
                        {
                            continue;
                        }

                        int pixelX = (blockColumn * BlockPixelSize) +
                            (tileColumn * TilePixelSize) + pixelColumn;
                        int pixelY = (blockRow * BlockPixelSize) +
                            (tileRow * TilePixelSize) + pixelRow;
                        int destination = ((pixelY * PixelWidth) + pixelX) * 4;
                        pixels[destination] = source.Red;
                        pixels[destination + 1] = source.Green;
                        pixels[destination + 2] = source.Blue;
                        pixels[destination + 3] = source.Alpha;
                    }
                }
            }
        }
    }

    private static SourcePixel ResolvePayloadPixel(
        OriginalMapVisualPayloadDefinition visualDefinition,
        int slot,
        int localTile,
        int row,
        int column)
    {
        IReadOnlyList<byte> decoded = visualDefinition.Tilesets[slot].DecodedBytes;
        int tileByteOffset = localTile * TileBytes;
        byte packed = decoded[tileByteOffset + (row * 4) + (column / 2)];
        int paletteIndex = column % 2 == 0
            ? (packed >> 4) & 0x0F
            : packed & 0x0F;
        if (paletteIndex == 0)
        {
            return default;
        }

        ushort colorWord = visualDefinition.Palette.EffectiveWords[paletteIndex];
        return new SourcePixel(
            ExpandChannel((colorWord & 0x000E) >> 1),
            ExpandChannel((colorWord & 0x00E0) >> 5),
            ExpandChannel((colorWord & 0x0E00) >> 9),
            byte.MaxValue);
    }

    private static SourcePixel ResolveAtlasPixel(
        IReadOnlyList<byte> atlasRgbaBytes,
        int scale,
        int atlasWidth,
        int slot,
        int localTile,
        int row,
        int column)
    {
        int logicalX = ((localTile % 16) * TilePixelSize) + column;
        int logicalY = (slot * 64) + ((localTile / 16) * TilePixelSize) + row;
        int offset = checked((((logicalY * scale) * atlasWidth) + (logicalX * scale)) * 4);
        return new SourcePixel(
            atlasRgbaBytes[offset],
            atlasRgbaBytes[offset + 1],
            atlasRgbaBytes[offset + 2],
            atlasRgbaBytes[offset + 3]);
    }

    private static byte ExpandChannel(int value) =>
        checked((byte)((value << 5) | (value << 2) | (value >> 1)));

    private static void FillBackground(byte[] pixels)
    {
        for (int offset = 0; offset < pixels.Length; offset += 4)
        {
            pixels[offset] = 0x12;
            pixels[offset + 1] = 0x18;
            pixels[offset + 2] = 0x20;
            pixels[offset + 3] = byte.MaxValue;
        }
    }

    private static bool SameSelection(
        OriginalMapVisualResourceSelection left,
        OriginalMapVisualResourceSelection right) =>
        left.Map == right.Map &&
        left.PaletteIndex == right.PaletteIndex &&
        left.TilesetSlots.SequenceEqual(right.TilesetSlots) &&
        string.Equals(
            left.ProjectionDigest,
            right.ProjectionDigest,
            StringComparison.OrdinalIgnoreCase);
}

public sealed partial class PrivateOriginalMapBaseViewport : Node2D
{
    private static readonly Color PlayerColor = new("ffd166");

    public PrivateOriginalMapBaseViewport()
    {
        TextureFilter = RequiredTextureFilter;
        TextureRepeat = RequiredTextureRepeat;
    }

    internal static TextureFilterEnum RequiredTextureFilter =>
        TextureFilterEnum.Nearest;

    internal static TextureRepeatEnum RequiredTextureRepeat =>
        TextureRepeatEnum.Disabled;

    private ImageTexture? _texture;
    private PrivateOriginalMapBaseViewProjection? _projection;
    private byte[]? _atlasRgbaBytes;
    private int _atlasScale;
    private string? _atlasAssetId;
    private string? _atlasBucketDigest;

    internal PrivateOriginalMapBaseViewProjection? Projection => _projection;

    internal bool UsesLocalAtlas => _atlasRgbaBytes is not null;

    internal bool UsesRequiredTextureSampling => IsRequiredTextureSampling(
        TextureFilter,
        TextureRepeat);

    internal string? AtlasAssetId => _atlasAssetId;

    internal int? AtlasScale => UsesLocalAtlas ? _atlasScale : null;

    internal string? AtlasBucketDigest => _atlasBucketDigest;

    internal static bool IsRequiredTextureSampling(
        TextureFilterEnum filter,
        TextureRepeatEnum repeat) =>
        filter == RequiredTextureFilter && repeat == RequiredTextureRepeat;

    internal bool TryBindLocalAtlas(
        PrivateLocalPresentationRasterMount mount,
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualPayloadDefinition visualDefinition,
        out PrivateLocalPresentationAssetMountDiagnostic? diagnostic)
    {
        ArgumentNullException.ThrowIfNull(mount);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(visualDefinition);
        diagnostic = null;
        if (!PrivateLocalPresentationAssetCatalog.IsExactMap3BaseAtlasBinding(
                mount.Definition,
                mount.Bucket) ||
            mount.Bucket.Width != checked(
                PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalWidth *
                mount.Bucket.Scale) ||
            mount.Bucket.Height != checked(
                PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalHeight *
                mount.Bucket.Scale))
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The private Map 3 base-atlas mount is incompatible with the viewport.");
            return false;
        }

        Image image = new();
        Error error = image.LoadPngFromBuffer(mount.CopyPngBytes());
        if (error != Error.Ok ||
            image.GetWidth() != mount.Bucket.Width ||
            image.GetHeight() != mount.Bucket.Height ||
            image.GetFormat() != Image.Format.Rgba8)
        {
            image.Dispose();
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.TextureRejected,
                "Godot rejected the admitted private Map 3 base-atlas texture.");
            return false;
        }

        byte[] rgbaBytes = image.GetData();
        image.Dispose();
        if (rgbaBytes.Length != checked(mount.Bucket.Width * mount.Bucket.Height * 4))
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.TextureRejected,
                "Godot returned an incompatible private Map 3 base-atlas pixel shape.");
            return false;
        }

        PrivateOriginalMapBaseViewProjection payloadProjection =
            PrivateOriginalMapBaseViewProjection.Create(snapshot, visualDefinition);
        PrivateOriginalMapBaseViewProjection atlasProjection =
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                snapshot,
                visualDefinition.Selection,
                rgbaBytes,
                mount.Bucket.Scale);
        if (!payloadProjection.RgbaBytes.SequenceEqual(atlasProjection.RgbaBytes))
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.PayloadMismatch,
                "The admitted private Map 3 base atlas is not pixel-equivalent to the typed visual payload.");
            return false;
        }

        _atlasRgbaBytes = [.. rgbaBytes];
        _atlasScale = mount.Bucket.Scale;
        _atlasAssetId = mount.Definition.AssetId;
        _atlasBucketDigest = mount.Bucket.Sha256;
        return true;
    }

    public void Project(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualPayloadDefinition visualDefinition)
    {
        _projection = _atlasRgbaBytes is null
            ? PrivateOriginalMapBaseViewProjection.Create(snapshot, visualDefinition)
            : PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                snapshot,
                visualDefinition.Selection,
                _atlasRgbaBytes,
                _atlasScale);
        Image image = Image.CreateFromData(
            PrivateOriginalMapBaseViewProjection.PixelWidth,
            PrivateOriginalMapBaseViewProjection.PixelHeight,
            useMipmaps: false,
            Image.Format.Rgba8,
            _projection.RgbaBytes.ToArray());
        _texture = ImageTexture.CreateFromImage(image);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_projection is null || _texture is null)
        {
            return;
        }

        DrawTexture(_texture, Vector2.Zero);
        Rect2 player = new(
            new Vector2(
                (_projection.PlayerColumn *
                    PrivateOriginalMapBaseViewProjection.BlockPixelSize) + 6,
                (_projection.PlayerRow *
                    PrivateOriginalMapBaseViewProjection.BlockPixelSize) + 6),
            new Vector2(12, 12));
        DrawRect(player, PlayerColor);
    }
}

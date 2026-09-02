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
        int rasterScale,
        IEnumerable<byte> rgbaBytes)
    {
        Map = map;
        OriginX = originX;
        OriginY = originY;
        PlayerColumn = playerColumn;
        PlayerRow = playerRow;
        RasterScale = rasterScale;
        _rgbaBytes = Array.AsReadOnly(rgbaBytes.ToArray());
    }

    internal MapId Map { get; }

    internal int OriginX { get; }

    internal int OriginY { get; }

    internal int PlayerColumn { get; }

    internal int PlayerRow { get; }

    internal int RasterScale { get; }

    internal int RasterPixelWidth => checked(PixelWidth * RasterScale);

    internal int RasterPixelHeight => checked(PixelHeight * RasterScale);

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
            rasterScale: 1,
            (slot, localTile, row, column, _, _) =>
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
            scale,
            (slot, localTile, row, column, subpixelRow, subpixelColumn) =>
                ResolveAtlasPixel(
                atlasRgbaBytes,
                scale,
                atlasWidth,
                slot,
                localTile,
                row,
                column,
                subpixelRow,
                subpixelColumn));
    }

    internal static bool IsExactNearestReplication(
        PrivateOriginalMapBaseViewProjection logical,
        PrivateOriginalMapBaseViewProjection physical)
    {
        ArgumentNullException.ThrowIfNull(logical);
        ArgumentNullException.ThrowIfNull(physical);
        if (logical.RasterScale != 1 ||
            !LocalPresentationAssetPackAdmission.BucketScales.Contains(physical.RasterScale) ||
            logical.Map != physical.Map ||
            logical.OriginX != physical.OriginX ||
            logical.OriginY != physical.OriginY ||
            logical.PlayerColumn != physical.PlayerColumn ||
            logical.PlayerRow != physical.PlayerRow ||
            physical.RasterPixelWidth != checked(PixelWidth * physical.RasterScale) ||
            physical.RasterPixelHeight != checked(PixelHeight * physical.RasterScale))
        {
            return false;
        }

        int scale = physical.RasterScale;
        for (int logicalY = 0; logicalY < PixelHeight; logicalY++)
        {
            for (int logicalX = 0; logicalX < PixelWidth; logicalX++)
            {
                int logicalOffset = ((logicalY * PixelWidth) + logicalX) * 4;
                for (int subpixelY = 0; subpixelY < scale; subpixelY++)
                {
                    for (int subpixelX = 0; subpixelX < scale; subpixelX++)
                    {
                        int physicalOffset = ((((logicalY * scale) + subpixelY) *
                            physical.RasterPixelWidth) +
                            (logicalX * scale) + subpixelX) * 4;
                        for (int channel = 0; channel < 4; channel++)
                        {
                            if (logical.RgbaBytes[logicalOffset + channel] !=
                                physical.RgbaBytes[physicalOffset + channel])
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    internal static PrivateOriginalMapBaseViewProjection CreateEdgeScale2x(
        PrivateOriginalMapBaseViewProjection logical,
        int outputScale)
    {
        ArgumentNullException.ThrowIfNull(logical);
        if (logical.RasterScale != 1)
        {
            throw new ArgumentException(
                "Edge-scale2x requires the canonical logical projection.",
                nameof(logical));
        }

        if (!LocalPresentationAssetPackAdmission.BucketScales.Contains(outputScale))
        {
            throw new ArgumentOutOfRangeException(nameof(outputScale));
        }

        byte[] edge2x = ApplyEdgeScale2x(logical.RgbaBytes, PixelWidth, PixelHeight);
        byte[] output = outputScale == 2
            ? edge2x
            : ReplicateNearest(edge2x, PixelWidth * 2, PixelHeight * 2, factor: 2);
        return new PrivateOriginalMapBaseViewProjection(
            logical.Map,
            logical.OriginX,
            logical.OriginY,
            logical.PlayerColumn,
            logical.PlayerRow,
            outputScale,
            output);
    }

    internal static byte[] ApplyEdgeScale2x(
        IReadOnlyList<byte> rgbaBytes,
        int width,
        int height)
    {
        ArgumentNullException.ThrowIfNull(rgbaBytes);
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        if (rgbaBytes.Count != checked(width * height * 4))
        {
            throw new ArgumentException(
                "The edge-scale2x source RGBA shape drifted.",
                nameof(rgbaBytes));
        }

        int outputWidth = checked(width * 2);
        byte[] output = new byte[checked(outputWidth * height * 2 * 4)];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SourcePixel center = ReadPixel(rgbaBytes, width, x, y);
                SourcePixel above = y == 0
                    ? center
                    : ReadPixel(rgbaBytes, width, x, y - 1);
                SourcePixel left = x == 0
                    ? center
                    : ReadPixel(rgbaBytes, width, x - 1, y);
                SourcePixel right = x == width - 1
                    ? center
                    : ReadPixel(rgbaBytes, width, x + 1, y);
                SourcePixel below = y == height - 1
                    ? center
                    : ReadPixel(rgbaBytes, width, x, y + 1);

                SourcePixel topLeft = left == above && left != below && above != right
                    ? left
                    : center;
                SourcePixel topRight = above == right && above != left && right != below
                    ? right
                    : center;
                SourcePixel bottomLeft = left == below && left != above && below != right
                    ? left
                    : center;
                SourcePixel bottomRight = below == right && left != below && above != right
                    ? right
                    : center;
                int outputX = x * 2;
                int outputY = y * 2;
                WritePixel(output, outputWidth, outputX, outputY, topLeft);
                WritePixel(output, outputWidth, outputX + 1, outputY, topRight);
                WritePixel(output, outputWidth, outputX, outputY + 1, bottomLeft);
                WritePixel(output, outputWidth, outputX + 1, outputY + 1, bottomRight);
            }
        }

        return output;
    }

    private static byte[] ReplicateNearest(
        IReadOnlyList<byte> source,
        int sourceWidth,
        int sourceHeight,
        int factor)
    {
        int outputWidth = checked(sourceWidth * factor);
        byte[] output = new byte[checked(outputWidth * sourceHeight * factor * 4)];
        for (int sourceY = 0; sourceY < sourceHeight; sourceY++)
        {
            for (int sourceX = 0; sourceX < sourceWidth; sourceX++)
            {
                SourcePixel pixel = ReadPixel(source, sourceWidth, sourceX, sourceY);
                for (int y = 0; y < factor; y++)
                {
                    for (int x = 0; x < factor; x++)
                    {
                        WritePixel(
                            output,
                            outputWidth,
                            (sourceX * factor) + x,
                            (sourceY * factor) + y,
                            pixel);
                    }
                }
            }
        }

        return output;
    }

    private static SourcePixel ReadPixel(
        IReadOnlyList<byte> rgbaBytes,
        int width,
        int x,
        int y)
    {
        int offset = ((y * width) + x) * 4;
        return new SourcePixel(
            rgbaBytes[offset],
            rgbaBytes[offset + 1],
            rgbaBytes[offset + 2],
            rgbaBytes[offset + 3]);
    }

    private static void WritePixel(
        byte[] rgbaBytes,
        int width,
        int x,
        int y,
        SourcePixel pixel)
    {
        int offset = ((y * width) + x) * 4;
        rgbaBytes[offset] = pixel.Red;
        rgbaBytes[offset + 1] = pixel.Green;
        rgbaBytes[offset + 2] = pixel.Blue;
        rgbaBytes[offset + 3] = pixel.Alpha;
    }

    private static PrivateOriginalMapBaseViewProjection CreateCore(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualResourceSelection selection,
        int rasterScale,
        Func<int, int, int, int, int, int, SourcePixel> resolvePixel)
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
        int rasterPixelWidth = checked(PixelWidth * rasterScale);
        int rasterPixelHeight = checked(PixelHeight * rasterScale);
        byte[] pixels = new byte[checked(rasterPixelWidth * rasterPixelHeight * 4)];
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
                    rasterScale,
                    rasterPixelWidth,
                    resolvePixel);
            }
        }

        return new PrivateOriginalMapBaseViewProjection(
            snapshot.Map,
            originX,
            originY,
            snapshot.PlayerPosition.X - originX,
            snapshot.PlayerPosition.Y - originY,
            rasterScale,
            pixels);
    }

    private static void RenderBlock(
        byte[] pixels,
        int blockColumn,
        int blockRow,
        OriginalMapBlockDefinition block,
        int rasterScale,
        int rasterPixelWidth,
        Func<int, int, int, int, int, int, SourcePixel> resolvePixel)
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
                        for (int subpixelRow = 0; subpixelRow < rasterScale; subpixelRow++)
                        {
                            for (int subpixelColumn = 0;
                                subpixelColumn < rasterScale;
                                subpixelColumn++)
                            {
                                int destinationTilePixelY =
                                    (pixelRow * rasterScale) + subpixelRow;
                                int destinationTilePixelX =
                                    (pixelColumn * rasterScale) + subpixelColumn;
                                int sourceTilePixelY = (word & VerticalFlip) != 0
                                    ? (TilePixelSize * rasterScale) - 1 -
                                        destinationTilePixelY
                                    : destinationTilePixelY;
                                int sourceTilePixelX = (word & HorizontalMirror) != 0
                                    ? (TilePixelSize * rasterScale) - 1 -
                                        destinationTilePixelX
                                    : destinationTilePixelX;
                                SourcePixel source = resolvePixel(
                                    slot,
                                    localTile,
                                    sourceTilePixelY / rasterScale,
                                    sourceTilePixelX / rasterScale,
                                    sourceTilePixelY % rasterScale,
                                    sourceTilePixelX % rasterScale);
                                if (source.Alpha == 0)
                                {
                                    continue;
                                }

                                int pixelX = (blockColumn * BlockPixelSize * rasterScale) +
                                    (tileColumn * TilePixelSize * rasterScale) +
                                    destinationTilePixelX;
                                int pixelY = (blockRow * BlockPixelSize * rasterScale) +
                                    (tileRow * TilePixelSize * rasterScale) +
                                    destinationTilePixelY;
                                int destination = ((pixelY * rasterPixelWidth) + pixelX) * 4;
                                pixels[destination] = source.Red;
                                pixels[destination + 1] = source.Green;
                                pixels[destination + 2] = source.Blue;
                                pixels[destination + 3] = source.Alpha;
                            }
                        }
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
        int column,
        int subpixelRow,
        int subpixelColumn)
    {
        int logicalX = ((localTile % 16) * TilePixelSize) + column;
        int logicalY = (slot * 64) + ((localTile / 16) * TilePixelSize) + row;
        int offset = checked(((((logicalY * scale) + subpixelRow) * atlasWidth) +
            (logicalX * scale) + subpixelColumn) * 4);
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

    internal static readonly Rect2 LogicalTextureRect = new(
        Vector2.Zero,
        new Vector2(
            PrivateOriginalMapBaseViewProjection.PixelWidth,
            PrivateOriginalMapBaseViewProjection.PixelHeight));

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
    private PrivateMap3WorldTreatment _worldTreatment =
        PrivateMap3WorldTreatment.ExactNearest;

    internal PrivateOriginalMapBaseViewProjection? Projection => _projection;

    internal bool UsesLocalAtlas => _atlasRgbaBytes is not null;

    internal bool UsesRequiredTextureSampling => IsRequiredTextureSampling(
        TextureFilter,
        TextureRepeat);

    internal string? AtlasAssetId => _atlasAssetId;

    internal int? AtlasScale => UsesLocalAtlas ? _atlasScale : null;

    internal string? AtlasBucketDigest => _atlasBucketDigest;

    internal PrivateMap3WorldTreatment WorldTreatment => _worldTreatment;

    internal static bool IsRequiredTextureSampling(
        TextureFilterEnum filter,
        TextureRepeatEnum repeat) =>
        filter == RequiredTextureFilter && repeat == RequiredTextureRepeat;

    internal bool TryBindLocalAtlas(
        PrivateLocalPresentationRasterMount mount,
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualPayloadDefinition visualDefinition,
        PrivateMap3WorldTreatment worldTreatment,
        out PrivateLocalPresentationAssetMountDiagnostic? diagnostic)
    {
        ArgumentNullException.ThrowIfNull(mount);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(visualDefinition);
        if (worldTreatment is not PrivateMap3WorldTreatment.ExactNearest and
            not PrivateMap3WorldTreatment.EdgeScale2x)
        {
            throw new ArgumentOutOfRangeException(nameof(worldTreatment));
        }

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
        if (!PrivateOriginalMapBaseViewProjection.IsExactNearestReplication(
                payloadProjection,
                atlasProjection))
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.PayloadMismatch,
                "The admitted private Map 3 base atlas is not an exact nearest replication of the typed visual payload.");
            return false;
        }

        _atlasRgbaBytes = [.. rgbaBytes];
        _atlasScale = mount.Bucket.Scale;
        _atlasAssetId = mount.Definition.AssetId;
        _atlasBucketDigest = mount.Bucket.Sha256;
        _worldTreatment = worldTreatment;
        return true;
    }

    public void Project(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualPayloadDefinition visualDefinition)
    {
        _projection = _atlasRgbaBytes is null
            ? PrivateOriginalMapBaseViewProjection.Create(snapshot, visualDefinition)
            : _worldTreatment switch
            {
                PrivateMap3WorldTreatment.ExactNearest =>
                    PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                        snapshot,
                        visualDefinition.Selection,
                        _atlasRgbaBytes,
                        _atlasScale),
                PrivateMap3WorldTreatment.EdgeScale2x =>
                    PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(
                        PrivateOriginalMapBaseViewProjection.Create(
                            snapshot,
                            visualDefinition),
                        _atlasScale),
                _ => throw new InvalidOperationException(
                    "The admitted private Map 3 world treatment is unknown."),
            };
        Image image = Image.CreateFromData(
            _projection.RasterPixelWidth,
            _projection.RasterPixelHeight,
            useMipmaps: false,
            Image.Format.Rgba8,
            _projection.RgbaBytes.ToArray());
        _texture = ImageTexture.CreateFromImage(image);
        image.Dispose();
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_projection is null || _texture is null)
        {
            return;
        }

        DrawTextureRect(_texture, LogicalTextureRect, tile: false);
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

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
        bool staticOverlayDiagnostic,
        int? overlayAreaRecordOrdinal,
        int overlayDeltaX,
        int overlayDeltaY,
        IEnumerable<byte> rgbaBytes)
    {
        Map = map;
        OriginX = originX;
        OriginY = originY;
        PlayerColumn = playerColumn;
        PlayerRow = playerRow;
        RasterScale = rasterScale;
        StaticOverlayDiagnostic = staticOverlayDiagnostic;
        OverlayAreaRecordOrdinal = overlayAreaRecordOrdinal;
        OverlayDeltaX = overlayDeltaX;
        OverlayDeltaY = overlayDeltaY;
        _rgbaBytes = Array.AsReadOnly(rgbaBytes.ToArray());
    }

    internal MapId Map { get; }

    internal int OriginX { get; }

    internal int OriginY { get; }

    internal int PlayerColumn { get; }

    internal int PlayerRow { get; }

    internal int RasterScale { get; }

    internal bool StaticOverlayDiagnostic { get; }

    internal bool ShowsPlayerMarker => !StaticOverlayDiagnostic;

    internal int? OverlayAreaRecordOrdinal { get; }

    internal int OverlayDeltaX { get; }

    internal int OverlayDeltaY { get; }

    internal int RasterPixelWidth => checked(PixelWidth * RasterScale);

    internal int RasterPixelHeight => checked(PixelHeight * RasterScale);

    internal IReadOnlyList<byte> RgbaBytes => _rgbaBytes;

    internal static PrivateOriginalMapBaseViewProjection Create(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualPayloadDefinition visualDefinition,
        bool staticOverlayDiagnostic = false)
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
            staticOverlayDiagnostic,
            (slot, localTile, row, column, _, _) =>
                ResolvePayloadPixel(visualDefinition, slot, localTile, row, column));
    }

    internal static PrivateOriginalMapBaseViewProjection CreateFromAtlas(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualResourceSelection selection,
        IReadOnlyList<byte> atlasRgbaBytes,
        int scale,
        bool staticOverlayDiagnostic = false)
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
            staticOverlayDiagnostic,
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
            logical.StaticOverlayDiagnostic != physical.StaticOverlayDiagnostic ||
            logical.OverlayAreaRecordOrdinal != physical.OverlayAreaRecordOrdinal ||
            logical.OverlayDeltaX != physical.OverlayDeltaX ||
            logical.OverlayDeltaY != physical.OverlayDeltaY ||
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
            logical.StaticOverlayDiagnostic,
            logical.OverlayAreaRecordOrdinal,
            logical.OverlayDeltaX,
            logical.OverlayDeltaY,
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
        bool staticOverlayDiagnostic,
        Func<int, int, int, int, int, int, SourcePixel> resolvePixel)
    {
        if (!SameSelection(snapshot.Definition.VisualResourceSelection, selection))
        {
            throw new ArgumentException(
                "The private map snapshot and visual payload must retain the same admitted selection.",
                nameof(selection));
        }

        OriginalMapAreaDefinition? overlayArea = null;
        int overlayDeltaX = 0;
        int overlayDeltaY = 0;
        int originX;
        int originY;
        if (staticOverlayDiagnostic)
        {
            OriginalMapAreaDefinition[] candidates = snapshot.Definition.AreaCatalog.Records
                .Where(record =>
                    record.SecondLayerForegroundStart != record.SecondLayerBackgroundStart)
                .Take(2)
                .ToArray();
            if (candidates.Length != 1)
            {
                throw new ArgumentException(
                    "A static overlay diagnostic requires exactly one admitted non-zero layer offset.",
                    nameof(snapshot));
            }

            overlayArea = candidates[0];
            overlayDeltaX = checked(
                (int)overlayArea.SecondLayerForegroundStart.X -
                overlayArea.SecondLayerBackgroundStart.X);
            overlayDeltaY = checked(
                (int)overlayArea.SecondLayerForegroundStart.Y -
                overlayArea.SecondLayerBackgroundStart.Y);
            originX = overlayArea.MainLayerBounds.MinimumX;
            originY = overlayArea.MainLayerBounds.MinimumY;
            if (originX + ColumnCount - 1 > overlayArea.MainLayerBounds.MaximumX ||
                originY + RowCount - 1 > overlayArea.MainLayerBounds.MaximumY ||
                !IsWithinLayoutRegion(originX, originY) ||
                !IsWithinLayoutRegion(
                    checked(originX + overlayDeltaX),
                    checked(originY + overlayDeltaY)))
            {
                throw new ArgumentException(
                    "The admitted static overlay crop exceeds its area or working-layout bounds.",
                    nameof(snapshot));
            }
        }
        else
        {
            originX = Math.Clamp(
                snapshot.PlayerPosition.X - (ColumnCount / 2),
                0,
                WorkingMapLayout.ColumnCount - ColumnCount);
            originY = Math.Clamp(
                snapshot.PlayerPosition.Y - (RowCount / 2),
                0,
                WorkingMapLayout.RowCount - RowCount);
        }

        int rasterPixelWidth = checked(PixelWidth * rasterScale);
        int rasterPixelHeight = checked(PixelHeight * rasterScale);
        byte[] pixels = new byte[checked(rasterPixelWidth * rasterPixelHeight * 4)];
        FillBackground(pixels);

        RenderLayoutRegion(
            pixels,
            snapshot,
            originX,
            originY,
            rasterScale,
            rasterPixelWidth,
            resolvePixel);
        if (overlayArea is not null)
        {
            RenderLayoutRegion(
                pixels,
                snapshot,
                checked(originX + overlayDeltaX),
                checked(originY + overlayDeltaY),
                rasterScale,
                rasterPixelWidth,
                resolvePixel);
        }

        return new PrivateOriginalMapBaseViewProjection(
            snapshot.Map,
            originX,
            originY,
            snapshot.PlayerPosition.X - originX,
            snapshot.PlayerPosition.Y - originY,
            rasterScale,
            staticOverlayDiagnostic,
            overlayArea?.Identity.OneBasedRecordOrdinal,
            overlayDeltaX,
            overlayDeltaY,
            pixels);
    }

    private static void RenderLayoutRegion(
        byte[] pixels,
        PrivateOriginalMapSessionSnapshot snapshot,
        int sourceOriginX,
        int sourceOriginY,
        int rasterScale,
        int rasterPixelWidth,
        Func<int, int, int, int, int, int, SourcePixel> resolvePixel)
    {
        for (int blockRow = 0; blockRow < RowCount; blockRow++)
        {
            for (int blockColumn = 0; blockColumn < ColumnCount; blockColumn++)
            {
                OriginalMapBlockDefinition block = snapshot.Definition.BlockCatalog.Resolve(
                    snapshot.WorkingLayout,
                    new MapPosition(
                        sourceOriginX + blockColumn,
                        sourceOriginY + blockRow));
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
    }

    private static bool IsWithinLayoutRegion(int originX, int originY) =>
        originX >= 0 &&
        originY >= 0 &&
        originX + ColumnCount <= WorkingMapLayout.ColumnCount &&
        originY + RowCount <= WorkingMapLayout.RowCount;

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

    internal static Rect2 PlayerReferenceRect(
        PrivateOriginalMapBaseViewProjection projection)
    {
        ArgumentNullException.ThrowIfNull(projection);
        return new Rect2(
            new Vector2(
                projection.PlayerColumn *
                    PrivateOriginalMapBaseViewProjection.BlockPixelSize,
                projection.PlayerRow *
                    PrivateOriginalMapBaseViewProjection.BlockPixelSize),
            new Vector2(
                PrivateLocalPresentationAssetCatalog.Map3PlayerReferenceLogicalWidth,
                PrivateLocalPresentationAssetCatalog.Map3PlayerReferenceLogicalHeight));
    }

    internal static Rect2 PlayerLocomotionRect(
        PrivateOriginalMapBaseViewProjection projection,
        PrivateOriginalMapPlayerLocomotionSnapshot animation)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(animation);
        return PlayerLocomotionRect(
            projection,
            animation.SourcePosition,
            animation.OffsetXUnits,
            animation.OffsetYUnits);
    }

    internal static Rect2 PlayerLocomotionRect(
        PrivateOriginalMapBaseViewProjection projection,
        MapPosition sourcePosition,
        int offsetXUnits,
        int offsetYUnits)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(sourcePosition);
        float unitScale =
            (float)PrivateOriginalMapBaseViewProjection.BlockPixelSize /
            PrivateOriginalMapPlayerLocomotionSnapshot.SourceUnitsPerMapTile;
        return new Rect2(
            new Vector2(
                ((sourcePosition.X - projection.OriginX) *
                    PrivateOriginalMapBaseViewProjection.BlockPixelSize) +
                    (offsetXUnits * unitScale),
                ((sourcePosition.Y - projection.OriginY) *
                    PrivateOriginalMapBaseViewProjection.BlockPixelSize) +
                    (offsetYUnits * unitScale)),
            new Vector2(
                PrivateLocalPresentationAssetCatalog.Map3PlayerReferenceLogicalWidth,
                PrivateLocalPresentationAssetCatalog.Map3PlayerReferenceLogicalHeight));
    }

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
    private ImageTexture? _playerReferenceTexture;
    private string? _playerReferenceAssetId;
    private int _playerReferenceScale;
    private string? _playerReferenceBucketDigest;
    private Dictionary<(
        PrivateOriginalMapPlayerLocomotionSheet Sheet,
        int Half,
        bool HorizontalMirror), ImageTexture>? _playerLocomotionTextures;
    private PrivateOriginalMapPlayerLocomotionSnapshot? _playerLocomotion;
    private int _playerLocomotionScale;
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

    internal bool UsesLocalPlayerReference => _playerReferenceTexture is not null;

    internal string? PlayerReferenceAssetId => _playerReferenceAssetId;

    internal int? PlayerReferenceScale => UsesLocalPlayerReference
        ? _playerReferenceScale
        : null;

    internal string? PlayerReferenceBucketDigest => _playerReferenceBucketDigest;

    internal bool UsesLocalPlayerLocomotion => _playerLocomotionTextures is not null;

    internal int? PlayerLocomotionScale => UsesLocalPlayerLocomotion
        ? _playerLocomotionScale
        : null;

    internal PrivateOriginalMapPlayerLocomotionSnapshot? PlayerLocomotion =>
        _playerLocomotion;

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
        bool staticOverlayDiagnostic,
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
            PrivateOriginalMapBaseViewProjection.Create(
                snapshot,
                visualDefinition,
                staticOverlayDiagnostic);
        PrivateOriginalMapBaseViewProjection atlasProjection =
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                snapshot,
                visualDefinition.Selection,
                rgbaBytes,
                mount.Bucket.Scale,
                staticOverlayDiagnostic);
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

    internal bool TryBindLocalPlayerReference(
        PrivateLocalPresentationRasterMount mount,
        out PrivateLocalPresentationAssetMountDiagnostic? diagnostic)
    {
        ArgumentNullException.ThrowIfNull(mount);
        diagnostic = null;
        if (!PrivateLocalPresentationAssetCatalog.IsExactMap3PlayerReferenceBinding(
                mount.Definition,
                mount.Bucket) ||
            mount.Bucket.Width != checked(
                PrivateLocalPresentationAssetCatalog.Map3PlayerReferenceLogicalWidth *
                mount.Bucket.Scale) ||
            mount.Bucket.Height != checked(
                PrivateLocalPresentationAssetCatalog.Map3PlayerReferenceLogicalHeight *
                mount.Bucket.Scale))
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The private Map 3 player reference mount is incompatible with the viewport.");
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
                "Godot rejected the admitted private Map 3 player reference texture.");
            return false;
        }

        _playerReferenceTexture = ImageTexture.CreateFromImage(image);
        image.Dispose();
        _playerReferenceAssetId = mount.Definition.AssetId;
        _playerReferenceScale = mount.Bucket.Scale;
        _playerReferenceBucketDigest = mount.Bucket.Sha256;
        return true;
    }

    internal bool TryBindLocalPlayerLocomotion(
        PrivateLocalPlayerLocomotionMount mount,
        out PrivateLocalPresentationAssetMountDiagnostic? diagnostic)
    {
        ArgumentNullException.ThrowIfNull(mount);
        diagnostic = null;
        PrivateLocalPresentationRasterMount[] sheets =
            [mount.Up, mount.Horizontal, mount.Down];
        if (sheets.Select(sheet => sheet.Bucket.Scale).Distinct().Count() != 1 ||
            sheets.Any(sheet => !PrivateLocalPresentationAssetCatalog
                .IsExactMap3PlayerLocomotionBinding(sheet.Definition, sheet.Bucket)))
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The private Map 3 player locomotion mount is incompatible with the viewport.");
            return false;
        }

        Dictionary<(
            PrivateOriginalMapPlayerLocomotionSheet Sheet,
            int Half,
            bool HorizontalMirror), ImageTexture> textures = [];
        if (!TryDecodeLocomotionSheet(
                mount.Up,
                PrivateOriginalMapPlayerLocomotionSheet.Up,
                includeMirroredFrames: false,
                textures) ||
            !TryDecodeLocomotionSheet(
                mount.Horizontal,
                PrivateOriginalMapPlayerLocomotionSheet.Horizontal,
                includeMirroredFrames: true,
                textures) ||
            !TryDecodeLocomotionSheet(
                mount.Down,
                PrivateOriginalMapPlayerLocomotionSheet.Down,
                includeMirroredFrames: false,
                textures))
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.TextureRejected,
                "Godot rejected an admitted private Map 3 player locomotion sheet.");
            return false;
        }

        _playerLocomotionTextures = textures;
        _playerLocomotionScale = sheets[0].Bucket.Scale;
        return true;
    }

    public void Project(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualPayloadDefinition visualDefinition,
        bool staticOverlayDiagnostic = false,
        PrivateOriginalMapPlayerLocomotionSnapshot? playerLocomotion = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (playerLocomotion is not null &&
            playerLocomotion.DestinationPosition != snapshot.PlayerPosition)
        {
            throw new ArgumentException(
                "The player locomotion destination must match the authoritative session position.",
                nameof(playerLocomotion));
        }

        if (playerLocomotion is not null)
        {
            _playerLocomotion = playerLocomotion;
        }

        _projection = _atlasRgbaBytes is null
            ? PrivateOriginalMapBaseViewProjection.Create(
                snapshot,
                visualDefinition,
                staticOverlayDiagnostic)
            : _worldTreatment switch
            {
                PrivateMap3WorldTreatment.ExactNearest =>
                    PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                        snapshot,
                        visualDefinition.Selection,
                        _atlasRgbaBytes,
                        _atlasScale,
                        staticOverlayDiagnostic),
                PrivateMap3WorldTreatment.EdgeScale2x =>
                    PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(
                        PrivateOriginalMapBaseViewProjection.Create(
                            snapshot,
                            visualDefinition,
                            staticOverlayDiagnostic),
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
        if (_projection.ShowsPlayerMarker &&
            _playerLocomotionTextures is not null &&
            _playerLocomotion is not null)
        {
            ImageTexture texture = _playerLocomotionTextures[(
                _playerLocomotion.Sheet,
                _playerLocomotion.SelectedHalf,
                _playerLocomotion.HorizontalMirror)];
            DrawTextureRect(
                texture,
                PlayerLocomotionRect(_projection, _playerLocomotion),
                tile: false);
        }
        else if (_projection.ShowsPlayerMarker && _playerReferenceTexture is not null)
        {
            DrawTextureRect(
                _playerReferenceTexture,
                PlayerReferenceRect(_projection),
                tile: false);
        }
        else if (_projection.ShowsPlayerMarker)
        {
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

    private static bool TryDecodeLocomotionSheet(
        PrivateLocalPresentationRasterMount mount,
        PrivateOriginalMapPlayerLocomotionSheet sheet,
        bool includeMirroredFrames,
        IDictionary<(
            PrivateOriginalMapPlayerLocomotionSheet Sheet,
            int Half,
            bool HorizontalMirror), ImageTexture> textures)
    {
        Image image = new();
        Error error = image.LoadPngFromBuffer(mount.CopyPngBytes());
        int scale = mount.Bucket.Scale;
        int frameWidth = checked(
            PrivateLocalPresentationAssetCatalog.Map3PlayerReferenceLogicalWidth * scale);
        int frameHeight = checked(
            PrivateLocalPresentationAssetCatalog.Map3PlayerReferenceLogicalHeight * scale);
        if (error != Error.Ok ||
            image.GetWidth() != checked(frameWidth * 2) ||
            image.GetHeight() != frameHeight ||
            image.GetFormat() != Image.Format.Rgba8)
        {
            image.Dispose();
            return false;
        }

        for (int half = 0; half < 2; half++)
        {
            Image frame = image.GetRegion(new Rect2I(half * frameWidth, 0, frameWidth, frameHeight));
            textures.Add((sheet, half, false), ImageTexture.CreateFromImage(frame));
            frame.Dispose();
            if (includeMirroredFrames)
            {
                Image mirrored = image.GetRegion(
                    new Rect2I(half * frameWidth, 0, frameWidth, frameHeight));
                mirrored.FlipX();
                textures.Add((sheet, half, true), ImageTexture.CreateFromImage(mirrored));
                mirrored.Dispose();
            }
        }

        image.Dispose();
        return true;
    }
}

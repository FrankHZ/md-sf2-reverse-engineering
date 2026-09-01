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
                    visualDefinition);
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
        OriginalMapVisualPayloadDefinition visualDefinition)
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
                if (slot >= visualDefinition.Tilesets.Count)
                {
                    continue;
                }

                IReadOnlyList<byte> decoded = visualDefinition.Tilesets[slot].DecodedBytes;
                int tileByteOffset = localTile * TileBytes;
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
                        byte packed = decoded[
                            tileByteOffset + (sourceRow * 4) + (sourceColumn / 2)];
                        int paletteIndex = sourceColumn % 2 == 0
                            ? (packed >> 4) & 0x0F
                            : packed & 0x0F;
                        if (paletteIndex == 0)
                        {
                            continue;
                        }

                        ushort colorWord = visualDefinition.Palette.EffectiveWords[paletteIndex];
                        int pixelX = (blockColumn * BlockPixelSize) +
                            (tileColumn * TilePixelSize) + pixelColumn;
                        int pixelY = (blockRow * BlockPixelSize) +
                            (tileRow * TilePixelSize) + pixelRow;
                        int destination = ((pixelY * PixelWidth) + pixelX) * 4;
                        pixels[destination] = ExpandChannel((colorWord & 0x000E) >> 1);
                        pixels[destination + 1] = ExpandChannel((colorWord & 0x00E0) >> 5);
                        pixels[destination + 2] = ExpandChannel((colorWord & 0x0E00) >> 9);
                        pixels[destination + 3] = byte.MaxValue;
                    }
                }
            }
        }
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

    private ImageTexture? _texture;
    private PrivateOriginalMapBaseViewProjection? _projection;

    internal PrivateOriginalMapBaseViewProjection? Projection => _projection;

    public void Project(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapVisualPayloadDefinition visualDefinition)
    {
        _projection = PrivateOriginalMapBaseViewProjection.Create(snapshot, visualDefinition);
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

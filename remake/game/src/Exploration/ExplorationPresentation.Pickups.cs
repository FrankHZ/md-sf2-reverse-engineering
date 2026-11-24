using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal sealed partial class ExplorationPresentation
{
    private static readonly (int Outline, int Fill)[] PickupStarRows =
    [
        (1, 0), (3, 1), (3, 1), (7, 3), (9, 7), (7, 3), (3, 1), (3, 1), (1, 0),
    ];

    internal object? PickupGlintProjection { get; private set; }

    internal void DrawPickupGlints(IReadOnlyList<MapPosition> pickups, Vector2 origin, Rect2 clip,
        Func<Rect2, bool>? coveredByTerrain = null)
    {
        const int cellSize = WorldViewportLayout.SourceCell;
        const int pixelSize = WorldViewportLayout.PixelScale;
        int size = PickupStarRows.Length;
        var inset = new Vector2(cellSize / 2 - size / 2, cellSize * 5 / 8 - size / 2);
        float opacity = _reducedFlash ? 1f :
            0.84f + 0.16f * Mathf.Sin((float)(Time.GetTicksMsec() % 2400) * Mathf.Tau / 2400);
        var outline = new Color(0.15f, 0.09f, 0.14f);
        var fill = new Color(1f, 0.87f, 0.49f, opacity);
        var highlight = new Color(1f, 0.97f, 0.84f, opacity);
        List<object>? marks = _observeCamera ? [] : null;
        foreach (var cell in pickups)
        {
            var topLeft = (new Vector2(cell.X, cell.Y) * cellSize + inset - origin) * pixelSize;
            var bounds = new Rect2(topLeft, Vector2.One * (size * pixelSize));
            if (!clip.Encloses(bounds) || coveredByTerrain?.Invoke(bounds) == true) continue;

            Vector2 center = bounds.GetCenter();
            for (int row = 0; row < size; row++)
            {
                var (outlineWidth, fillWidth) = PickupStarRows[row];
                float y = topLeft.Y + row * pixelSize;
                _owner.DrawRect(PickupStarSpan(center.X, y, outlineWidth), outline);
                if (fillWidth > 0) _owner.DrawRect(PickupStarSpan(center.X, y, fillWidth), fill);
            }
            _owner.DrawRect(PickupStarSpan(center.X, topLeft.Y + size / 2 * pixelSize, 1), highlight);
            marks?.Add(new
            {
                x = cell.X, y = cell.Y, screenX = center.X, screenY = center.Y, opacity, pixelSize,
                left = bounds.Position.X, top = bounds.Position.Y, width = bounds.Size.X, height = bounds.Size.Y,
            });
        }
        PickupGlintProjection = marks;
    }

    private bool LayerCoversPickup(Rect2 star, ExplorationState world, ExplorationMapVisual visual,
        Rect2I area, Vector2 origin, MapOverlayOffset offset, bool overlay, bool highOnly)
    {
        const int cell = WorldViewportLayout.SourceCell;
        const int pixel = WorldViewportLayout.PixelScale;
        const int tileSize = cell / 3;
        // Inspect only blocks underneath this star in the later plane's coordinates.
        var start = ((star.Position / pixel + origin) / cell).Floor();
        var end = ((star.End / pixel + origin) / cell).Ceil();
        for (int y = (int)start.Y; y < end.Y; y++)
            for (int x = (int)start.X; x < end.X; x++)
            {
                int sourceX = x + offset.X, sourceY = y + offset.Y;
                if (LayerBlock(world.Layout, sourceX, sourceY, overlay) is not { } block) continue;
                var blockTop = (new Vector2(x, y) * cell - origin) * pixel;
                Bitmap? ink = null;
                for (int tile = 0; tile < 9; tile++)
                {
                    if (highOnly && (visual.Blocks[block][tile] & 0x8000) == 0) continue;
                    var tileTop = blockTop + new Vector2(tile % 3, tile / 3) * (tileSize * pixel);
                    var tileRect = new Rect2(tileTop, Vector2.One * (tileSize * pixel));
                    if (!star.Intersects(tileRect)) continue;
                    for (int row = 0; row < PickupStarRows.Length; row++)
                    {
                        var span = PickupStarSpan(star.GetCenter().X, star.Position.Y + row * pixel, PickupStarRows[row].Outline);
                        if (!span.Intersects(tileRect)) continue;
                        ink ??= _mapTextures.BlockInk(visual, block, BlockVariant(visual, world.Layout, sourceX, sourceY, area, out _));
                        var hit = span.Intersection(tileRect);
                        float texelsPerPixel = ink.GetSize().X / (float)(cell * pixel);
                        var first = ((hit.Position - blockTop) * texelsPerPixel).Floor();
                        var after = ((hit.End - blockTop) * texelsPerPixel).Ceil();
                        for (int ty = (int)first.Y; ty < after.Y; ty++)
                            for (int tx = (int)first.X; tx < after.X; tx++)
                                if (ink.GetBit(tx, ty)) return true;
                    }
                }
            }
        return false;
    }

    private static Rect2 PickupStarSpan(float centerX, float y, int sourcePixels)
    {
        const int pixelSize = WorldViewportLayout.PixelScale;
        int width = sourcePixels * pixelSize;
        return new(new Vector2(centerX - width / 2f, y), new Vector2(width, pixelSize));
    }
}

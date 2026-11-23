using Godot;
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

    internal void DrawPickupGlints(IReadOnlyList<MapPosition> pickups, Vector2 origin, Rect2 clip)
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
            if (!clip.Encloses(bounds)) continue;

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

    private static Rect2 PickupStarSpan(float centerX, float y, int sourcePixels)
    {
        const int pixelSize = WorldViewportLayout.PixelScale;
        int width = sourcePixels * pixelSize;
        return new(new Vector2(centerX - width / 2f, y), new Vector2(width, pixelSize));
    }
}

using Godot;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal sealed partial class ExplorationPresentation
{
    internal object? PickupGlintProjection { get; private set; }

    internal void DrawPickupGlints(IReadOnlyList<MapPosition> pickups, Vector2 origin, Rect2 clip)
    {
        const float radius = 10;
        float opacity = _reducedFlash ? 0.78f :
            0.75f + 0.12f * Mathf.Sin((float)(Time.GetTicksMsec() % 2400) * Mathf.Tau / 2400);
        List<object>? marks = _observeCamera ? [] : null;
        foreach (var cell in pickups)
        {
            var center = (new Vector2(cell.X + 0.5f, cell.Y + 0.625f) * WorldViewportLayout.SourceCell - origin) *
                WorldViewportLayout.PixelScale;
            if (!clip.Encloses(new(center - Vector2.One * radius, Vector2.One * (radius * 2)))) continue;
            var edge = new Color(0.23f, 0.15f, 0.08f, opacity);
            var light = new Color(1f, 0.88f, 0.61f, opacity);
            _owner.DrawLine(center - Vector2.Up * (radius - 1), center + Vector2.Up * (radius - 1), edge, 5);
            _owner.DrawLine(center - Vector2.Right * (radius - 2), center + Vector2.Right * (radius - 2), edge, 5);
            _owner.DrawLine(center - Vector2.Up * (radius - 2), center + Vector2.Up * (radius - 2), light, 2);
            _owner.DrawLine(center - Vector2.Right * (radius - 3), center + Vector2.Right * (radius - 3), light, 2);
            marks?.Add(new { x = cell.X, y = cell.Y, screenX = center.X, screenY = center.Y, opacity });
        }
        PickupGlintProjection = marks;
    }
}

using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Maps;

// Disposable presentation geometry, shared across view lifetimes; no session mutation.
internal sealed class WorldViewportLayout
{
    internal const int SourceCell = 24, PixelScale = 4, CellSize = SourceCell * PixelScale;
    internal static readonly Vector2 LogicalHalfFrame = new(120, 108);
    private MapId? _map;
    private Vector2 _target;
    private Rect2 _bounds;
    private Vector2 _viewport;
    internal Vector2 Origin { get; private set; }
    internal bool Settled => Origin.DistanceTo(_target) < 0.26f;
    internal Rect2 Bounds => _bounds;
    internal Rect2 Screen => new Rect2((_bounds.Position - Origin) * PixelScale,
        _bounds.Size * PixelScale).Intersection(new(Vector2.Zero, _viewport));

    internal static Rect2 FieldBounds(ExplorationState world, LogicalView? view = null)
    {
        // LoadScene selects its area from an explicit origin independently of the
        // retained player. Never clamp that story camera to the player's old room.
        if (view is not null)
            return new(view.Area.MinX * SourceCell, view.Area.MinY * SourceCell,
                (view.Area.MaxX - view.Area.MinX + 1) * SourceCell,
                (view.Area.MaxY - view.Area.MinY + 1) * SourceCell);
        var area = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position)!.Area;
        return new(area.MinimumX * SourceCell, area.MinimumY * SourceCell,
            (area.MaximumX - area.MinimumX + 1) * SourceCell,
            (area.MaximumY - area.MinimumY + 1) * SourceCell);
    }

    internal void Focus(MapId map, Rect2 bounds, Vector2 point, Vector2 viewport, bool story = false)
    {
        // Packed rooms on one map are separate display spaces. Their ordinary warp
        // transition must not pan through the intervening unopened storage.
        bool changed = _map != map || _bounds.HasArea() && !_bounds.Intersects(bounds);
        _map = map; _bounds = bounds; _viewport = viewport;
        Vector2 extent = viewport / PixelScale;
        // Keep the subject clear of bottom overlays without reserving world space for UI.
        Vector2 center = extent / 2 - new Vector2(0, 24);
        Vector2 desired = point - center;
        if (!story && !changed)
        {
            var displacement = point - (Origin + center);
            Vector2 dead = new(24, 12);
            desired = Origin + new Vector2(
                displacement.X - Mathf.Clamp(displacement.X, -dead.X, dead.X),
                displacement.Y - Mathf.Clamp(displacement.Y, -dead.Y, dead.Y));
        }
        _target = Clamp(desired, extent);
        Origin = changed ? _target : Clamp(Origin, extent);
    }

    private Vector2 Clamp(Vector2 desired, Vector2 extent) => new(
        Axis(desired.X, _bounds.Position.X, _bounds.Size.X, extent.X),
        Axis(desired.Y, _bounds.Position.Y, _bounds.Size.Y, extent.Y));
    private static float Axis(float desired, float low, float size, float extent) =>
        size <= extent ? low + (size - extent) / 2 : Mathf.Clamp(desired, low, low + size - extent);

    internal void Advance(double delta, bool immediate = false)
    {
        Origin = immediate ? _target : Origin.MoveToward(_target, (float)(Math.Min(delta, 0.05) * 180));
        // Integral output pixels retain nearest sampling while panning.
        Origin = (Origin * PixelScale).Round() / PixelScale;
    }

    internal static Vector2 Plane(Vector2 origin, ExplorationViewArea area, bool foreground) => foreground
        ? origin * new Vector2(area.ParallaxAX, area.ParallaxAY) / 256 + new Vector2(area.ForegroundX, area.ForegroundY) * SourceCell
        : origin * new Vector2(area.ParallaxBX, area.ParallaxBY) / 256 + new Vector2(area.BackgroundX, area.BackgroundY) * SourceCell;
}

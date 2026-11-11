using Godot;

namespace Sf2.Remake.GodotAdapter.Previews;

// All positions here are logical preview pixels; cell coordinates are battle-local.
internal sealed class PreviewInspectionCamera
{
    internal const int Tile = 96;
    internal static readonly Rect2 View = new(0, 72, 1920, 925);
    internal static readonly Vector2 Opening = new(192, -384);
    internal Vector2I MapSize { get; private set; } = new(16, 20);
    private Vector2 _origin = Opening;
    internal Vector2 Origin => _origin.Round();

    internal void Reset(Vector2I size)
    {
        MapSize = size;
        _origin = Clamp(Opening);
    }

    private Vector2 Clamp(Vector2 origin)
    {
        static float Axis(float origin, float length, float start, float view) =>
            length <= view ? start + (view - length) / 2 : Mathf.Clamp(origin, start + view - length, start);
        return new(Axis(origin.X, MapSize.X * Tile, View.Position.X, View.Size.X),
            Axis(origin.Y, MapSize.Y * Tile, View.Position.Y, View.Size.Y));
    }

    internal bool Pan(Vector2 velocity, double delta)
    {
        var before = Origin;
        _origin = Clamp(_origin - velocity * (float)delta);
        return Origin != before;
    }

    internal void Reveal(Vector2I cell)
    {
        var topLeft = (Vector2)cell * Tile + _origin;
        var last = View.End - Vector2.One * Tile;
        _origin = Clamp(_origin + topLeft.Clamp(View.Position, last) - topLeft);
    }

    internal Rect2 OverviewCell(Vector2I cell)
    {
        var area = new Rect2(110, 124, 1120, 800);
        float side = Mathf.Floor(Mathf.Min(area.Size.X / MapSize.X, area.Size.Y / MapSize.Y));
        var corner = area.GetCenter() - (Vector2)MapSize * side / 2;
        return new(corner + (Vector2)cell * side, Vector2.One * side);
    }
}

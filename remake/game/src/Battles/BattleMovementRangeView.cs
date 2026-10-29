using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

// Board-local ground overlay: after ground (Z0), before actors (Z1), source foreground (Z2)
// and preview/target marks (Z3). Parent board and viewport retain their existing clipping.
internal sealed partial class BattleMovementRangeView : Node2D
{
    private BattleMovementRange? _range;
    private BattleMovementRange? _drawn;
    private int _draws;
    private static readonly Color Fill = new(0.12f, 0.65f, 0.95f, 0.20f);
    private static readonly Color Edge = new(0.22f, 0.78f, 1f, 0.55f);
    internal void Present(BattleMovementRange? range)
    {
        _range = range;
        Visible = range is not null;
        QueueRedraw();
    }
    public override void _Draw()
    {
        if (_range is null) return;
        _draws++;
        _drawn = _range;
        foreach (var tile in _range.Destinations)
        {
            var rect = new Rect2(new Vector2(tile.X, tile.Y) * WorldViewportLayout.CellSize + Vector2.One,
                Vector2.One * (WorldViewportLayout.CellSize - 2));
            DrawRect(rect, Fill);
            DrawRect(rect, Edge, false, 2);
        }
    }
    internal object Observe() => new
    {
        visible = IsVisibleInTree(), actor = _range?.Actor.Value, origin = _range?.Origin,
        destinations = _range?.Destinations, drawnDestinations = _drawn?.Destinations, z = ZIndex, draws = _draws,
        cellSize = WorldViewportLayout.CellSize, fillAlpha = Fill.A, edgeAlpha = Edge.A,
        globalOrigin = BattleMapViewport.Rectangle(new Rect2(ToGlobal(Vector2.Zero), Vector2.Zero)),
    };
}

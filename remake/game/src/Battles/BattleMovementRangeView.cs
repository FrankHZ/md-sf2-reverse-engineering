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
    internal bool Steady { get; set; }
    private double _pulse;
    private float Alpha => Steady ? 0.29f : 0.29f + 0.045f * MathF.Sin((float)(_pulse * Math.Tau / 1.2));
    public override void _Process(double delta)
    { if (IsVisibleInTree() && !Steady) { _pulse += delta; QueueRedraw(); } }
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
        var stops = _range.Destinations.ToHashSet();
        foreach (var tile in _range.ReachableCells)
            BattleActionRangeView.Tile(this, tile, new("78e9f3"), stops.Contains(tile) ? Alpha : 0);
    }
    internal object Observe() => new
    {
        visible = IsVisibleInTree(), actor = _range?.Actor.Value, origin = _range?.Origin,
        reachableCells = _range?.ReachableCells, drawnReachableCells = _drawn?.ReachableCells,
        destinations = _range?.Destinations, drawnDestinations = _drawn?.Destinations, z = ZIndex, draws = _draws,
        cellSize = WorldViewportLayout.CellSize, fillAlpha = Alpha, edgeAlpha = 0.9f, steady = Steady,
        globalOrigin = BattleMapViewport.Rectangle(new Rect2(ToGlobal(Vector2.Zero), Vector2.Zero)),
    };
}

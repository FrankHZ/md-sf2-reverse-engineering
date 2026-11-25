using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

// Board marks from the current admitted action footprint.
internal sealed partial class BattleActionRangeView : Node2D
{
    private BattleActionFootprint? _footprint;
    private bool _steady;
    private double _pulse;
    private int _draws;
    internal void Clear() { _footprint = null; Hide(); QueueRedraw(); }

    internal void Present(BattleActionFootprint? footprint, bool steady)
    {
        _steady = steady;
        _footprint = footprint;
        Visible = footprint is not null && (footprint.Reach.Count > 0 || footprint.Area.Count > 0);
        QueueRedraw();
    }
    public override void _Process(double delta)
    { if (IsVisibleInTree() && !_steady) { _pulse += delta; QueueRedraw(); } }
    internal static void Tile(CanvasItem view, MapPosition cell, Color color, float alpha)
    {
        var rect = new Rect2(new Vector2(cell.X, cell.Y) * WorldViewportLayout.CellSize + new Vector2(2, 2), new Vector2(92, 92));
        if (alpha > 0) view.DrawRect(rect, new Color(color, alpha));
        view.DrawRect(rect, new Color("18202b"), false, 6);
        view.DrawRect(rect.Grow(-2), new Color(color, 0.9f), false, 2);
    }
    private float Alpha => _steady ? 0.29f : 0.29f + 0.045f * MathF.Sin((float)(_pulse * Math.Tau / 1.2));
    public override void _Draw()
    {
        _draws++;
        if (_footprint is not { } footprint) return;
        var reach = footprint.Reach.ToHashSet();
        foreach (var p in footprint.Reach) Tile(this, p, new("ffda7a"), Alpha);
        foreach (var p in footprint.Area.Distinct())
        {
            if (!reach.Contains(p)) Tile(this, p, new("ffda7a"), 0);
            var cell = new Vector2(p.X, p.Y) * WorldViewportLayout.CellSize;
            DrawRect(new Rect2(cell + new Vector2(12, 12), new Vector2(72, 72)), new Color("fff3c88c"));
        }
    }
    internal object Observe() => new { visible = IsVisibleInTree(), reach = _footprint?.Reach ?? [], effect = _footprint?.Area ?? [],
        centers = _footprint?.Centers.Select(unit => unit.Actor.Value).ToArray() ?? [],
        recipients = _footprint?.Recipients.Select(unit => unit.Actor.Value).ToArray() ?? [],
        fillAlpha = Alpha, steady = _steady, draws = _draws, z = ZIndex };
}

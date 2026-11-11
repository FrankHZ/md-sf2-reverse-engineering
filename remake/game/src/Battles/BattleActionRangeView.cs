using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

// Disposable geometry from the live admitted offer and its actual content. Never selects targets.
internal sealed partial class BattleActionRangeView : Node2D
{
    private MapPosition[] _reach = [], _effect = [];
    private bool _steady;
    private double _pulse;
    private int _draws;
    internal void Present(BattleActionChoice? offer, SessionSnapshot current, ActorRef? candidate, bool steady)
    {
        _reach = []; _effect = []; _steady = steady;
        if (offer is { MinimumRange: { } min, MaximumRange: { } max } && current.Selection is { } selection)
        {
            var definition = current.Battle.Definition;
            var origin = selection.Preview.Destination;
            var cells = Enumerable.Range(0, definition.Height).SelectMany(y =>
                Enumerable.Range(0, definition.Width).Select(x => new MapPosition(x, y))).ToArray();
            _reach = cells.Where(p => Distance(origin, p) >= min && Distance(origin, p) <= max &&
                definition.Terrain[p.Y * 48 + p.X].Surface != TerrainSurface.Barrier).ToArray();
            if (candidate is { } id && offer.Targets.Any(t => t.Enabled && t.Actor == id))
            {
                var center = id == selection.Actor ? origin : current.Battle.GetActor(id).Position!;
                int radius = 0; bool all = false;
                if (offer.Action.Spell is { } key)
                {
                    if (definition.Spells.TryGetValue(key, out var heal))
                    { radius = heal.Radius; all = heal.Targets == HealingTargets.AuraAllAllies; }
                    else if (definition.AttackSpells.TryGetValue(key, out var attack)) radius = attack.Radius;
                    else if (definition.SupportSpells.TryGetValue(key, out var support)) radius = support.Radius;
                }
                _effect = all ? current.Battle.Actors.Where(a => a.IsAlly && a.Hp > 0 && a.Position is not null)
                    .Select(a => a.Actor == selection.Actor ? origin : a.Position!).ToArray() :
                    cells.Where(p => Distance(center, p) <= radius).ToArray();
            }
        }
        Visible = _reach.Length > 0 || _effect.Length > 0; QueueRedraw();
    }
    private static int Distance(MapPosition a, MapPosition b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    public override void _Process(double delta)
    { if (IsVisibleInTree() && !_steady) { _pulse += delta; QueueRedraw(); } }
    internal static void Tile(CanvasItem view, MapPosition cell, Color color, float alpha)
    {
        var rect = new Rect2(new Vector2(cell.X, cell.Y) * WorldViewportLayout.CellSize + new Vector2(2, 2), new Vector2(92, 92));
        view.DrawRect(rect, new Color(color, alpha));
        view.DrawRect(rect, new Color("18202b"), false, 6);
        view.DrawRect(rect.Grow(-2), new Color(color, 0.9f), false, 2);
    }
    private float Alpha => _steady ? 0.605f : 0.605f + 0.075f * MathF.Sin((float)(_pulse * Math.Tau / 1.2));
    public override void _Draw()
    {
        _draws++;
        foreach (var p in _reach) Tile(this, p, new("ffda7a"), Alpha);
        foreach (var p in _effect) Tile(this, p, Colors.White, 0.28f);
    }
    internal object Observe() => new { visible = IsVisibleInTree(), reach = _reach, effect = _effect,
        fillAlpha = Alpha, steady = _steady, draws = _draws, z = ZIndex };
}

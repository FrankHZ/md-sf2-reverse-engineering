using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.GodotAdapter.Battles;

// Functional modern attack-spell delivery. Authoritative cards and messages are shared;
// this view only draws the selected effect and completes its own presentation wait.
internal sealed partial class AttackSpellView : Node2D
{
    private BattleSceneState? _scene;
    private double _elapsed;
    private int _drawCount;
    internal void Present(BattleSceneState scene)
    { _scene = scene; _elapsed = 0; Show(); QueueRedraw(); }
    internal bool Advance(double elapsed)
    {
        _elapsed = elapsed; QueueRedraw();
        return elapsed >= (_scene!.Phase is BattleScenePhase.ActionAnimation or BattleScenePhase.Reaction ? 0.6 : 0.2);
    }
    public override void _Draw()
    {
        if (_scene?.AttackSpell is not { } spell) return;
        _drawCount++;
        DrawRect(new Rect2(0, 44, 256, 122), new Color("18243a"));
        var fire = spell.Element == AttackSpellElement.Fire;
        var color = new Color(fire ? "ff984d" : "9ce6e7");
        for (int i = 0; i < _scene.AttackTargets.Count; i++)
        {
            Vector2 position = new(128 + (i - (_scene.AttackTargets.Count - 1) / 2f) * 30, 112);
            float radius = 9 + 7 * (float)Math.Sin(Math.Min(1, _elapsed / 0.6) * Math.PI);
            if (fire)
                DrawColoredPolygon([position + new Vector2(0, -radius), position + new Vector2(radius / 2, radius / 2),
                    position + new Vector2(-radius / 2, radius / 2)], color);
            else if (spell.Element == AttackSpellElement.Lightning)
                DrawPolyline([position + new Vector2(4, -2 * radius), position + new Vector2(-5, -2),
                    position + new Vector2(4, -2), position + new Vector2(-4, radius)], new Color("ffe484"), 2);
            else if (spell.Spell.Value == "aqua")
                for (int bubble = 0; bubble < 5; bubble++)
                {
                    var center = position + Vector2.FromAngle(bubble * Mathf.Tau / 5) * radius;
                    DrawArc(center, radius / 3, 0, Mathf.Tau, 16, color, 1.5f);
                }
            else if (spell.Element == AttackSpellElement.Ice)
                DrawColoredPolygon([position + new Vector2(0, -radius), position + new Vector2(radius / 2, 0),
                    position + new Vector2(0, radius), position + new Vector2(-radius / 2, 0)], new Color("b6dfff"));
            else
                for (int ring = 0; ring < 3; ring++)
                    DrawArc(position, radius + ring * 3, -2.8f, 0.5f, 24, color, 2);
        }
    }
    internal object Observe() => new
    {
        visible = IsVisibleInTree(), drawCount = _drawCount, elapsed = _elapsed,
        element = _scene?.AttackSpell?.Element.ToString(), targets = _scene?.AttackTargets.Select(a => a.Value),
        target = _scene?.Target?.Value, amount = _scene?.Amount, modern = true
    };
}

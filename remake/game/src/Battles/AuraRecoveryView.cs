using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// Finite authored recovery feedback; no original fairy/timing claim or gameplay mutation.
internal sealed partial class AuraRecoveryView : Node2D
{
    private BattleSceneState? _scene;
    private EngineBattleState? _battle;
    private double _elapsed;
    private int _drawCount;
    internal void Present(BattleSceneState scene, EngineBattleState battle)
    { _scene = scene; _battle = battle; _elapsed = 0; Show(); QueueRedraw(); }
    internal bool Advance(double elapsed)
    {
        _elapsed = elapsed; QueueRedraw();
        return elapsed >= (_scene!.Phase is BattleScenePhase.ActionAnimation or BattleScenePhase.Reaction ? 0.6 : 0.2);
    }
    public override void _Draw()
    {
        if (_scene is null || _battle is null) return;
        _drawCount++;
        DrawRect(new Rect2(0, 44, 256, 122), new Color("102e2b"));
        DrawString(ThemeDB.FallbackFont, new(10, 60), $"AURA {_scene.Spell!.Spell.Level}  |  {_scene.Phase}", fontSize: 10);
        Vector2 center = new(128, 110);
        float radius = 12 + 10 * (float)Math.Sin(Math.Min(1, _elapsed / 0.6) * Math.PI);
        DrawCircle(center, 10, new Color("7ce2af"));
        if (_scene.Phase is BattleScenePhase.ActionAnimation or BattleScenePhase.Reaction)
            DrawArc(center, radius, 0, Mathf.Tau, 32, new Color("b5ffde"), 2);
        DrawString(ThemeDB.FallbackFont, new(10, 160), $"{_scene.HealingTargets.Count} ordered recipients", fontSize: 9);
    }
    internal object Observe() => new
    {
        visible = IsVisibleInTree(), elapsed = _elapsed, drawCount = _drawCount,
        targets = _scene?.HealingTargets.Select(a => a.Value), target = _scene?.Target?.Value,
        displayed = _scene?.DisplayedAlly?.Value,
        hp = _scene?.Target is { } id ? _battle?.GetActor(id).Hp : null,
        amount = _scene?.Amount, authored = true
    };
}

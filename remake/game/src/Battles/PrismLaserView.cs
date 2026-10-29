using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// Small project-authored diagram, not original sprites, timing or audio.
// It renders the immutable shot and live HP; only its actual elapsed delivery
// releases the existing scene token. No gameplay state or completion injection.
internal sealed partial class PrismLaserView : Node2D
{
    private BattleSceneState? _scene;
    private EngineBattleState? _battle;
    private double _elapsed;
    internal string Stage => _scene?.Phase != BattleScenePhase.ActionAnimation ? "idle" :
        _elapsed < 0.35 ? "charge" : _elapsed < 0.65 ? "fire" : _elapsed < 0.9 ? "settle" : "complete";
    internal void Present(BattleSceneState scene, EngineBattleState battle)
    { _scene = scene; _battle = battle; _elapsed = 0; Show(); QueueRedraw(); }
    internal bool Advance(double elapsed)
    {
        _elapsed = elapsed; QueueRedraw();
        return elapsed >= (_scene!.Phase == BattleScenePhase.ActionAnimation ? 0.9 :
            _scene.Phase is BattleScenePhase.Initialize or BattleScenePhase.End ? 0.25 : 0.2);
    }
    public override void _Draw()
    {
        if (_scene?.Laser is not { } shot || _battle is null) return;
        DrawRect(new Rect2(0, 44, 256, 122), new Color("142036"));
        Vector2 direction = shot.Direction switch { 0 => Vector2.Right, 1 => Vector2.Up, 2 => Vector2.Left, _ => Vector2.Down };
        Vector2 center = new(128, 105), extent = direction * (shot.Direction % 2 == 0 ? 92 : 39);
        Vector2 origin = center - extent, end = center + extent;
        DrawLine(origin, end, new Color("40516a"), 1);
        if (Stage == "fire")
        { DrawLine(origin, end, new Color("77dafa"), 8); DrawLine(origin, end, Colors.White, 2); }
        float radius = Stage == "charge" ? 5 + (float)(_elapsed / 0.35) * 5 : 5;
        DrawCircle(origin, radius, new Color("cf8fff"));
        int remaining = shot.Direction switch { 0 => 47 - shot.Origin.X, 1 => shot.Origin.Y, 2 => shot.Origin.X, _ => 47 - shot.Origin.Y };
        foreach (var id in shot.Targets)
        {
            var actor = _battle.GetActor(id);
            if (actor.Position is not { } position) continue;
            int distance = Math.Abs(position.X - shot.Origin.X) + Math.Abs(position.Y - shot.Origin.Y);
            var point = origin.Lerp(end, (float)distance / remaining);
            var color = actor.IsAlly ? new Color("74dba4") : new Color("f4ac75");
            DrawCircle(point, 4, color);
            if (_scene.ReactionTarget == id && _scene.Phase is BattleScenePhase.Reaction or BattleScenePhase.ResultMessage or BattleScenePhase.DeathMessage)
                DrawArc(point, 7, 0, Mathf.Tau, 24, Colors.White, 1);
        }
        DrawString(ThemeDB.FallbackFont, new(8, 55), $"PRISM LASER  {Stage.ToUpperInvariant()}", fontSize: 9, modulate: new Color("cf8fff"));
    }
    internal object Observe() => new
    {
        visible = Visible,
        stage = Stage,
        elapsed = _elapsed,
        origin = _scene?.Laser?.Origin,
        direction = _scene?.Laser?.Direction,
        focus = _scene?.Laser?.Focus?.Value,
        targets = _scene?.Laser?.Targets.Select(a => a.Value),
        target = _scene?.ReactionTarget?.Value,
        targetSide = _scene?.ReactionTarget is { } id ? _battle?.GetActor(id).Faction.ToString() : null,
        beamVisible = Visible && Stage == "fire",
        authored = true
    };
}

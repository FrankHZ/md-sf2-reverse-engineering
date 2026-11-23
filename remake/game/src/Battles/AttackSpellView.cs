using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.GodotAdapter.Battles;

// Local effect playback. The scene owns actors, reading and continuation tokens.
internal sealed partial class AttackSpellView : Node2D
{
    internal const double BlazeDuration = 48.0 / 60;
    internal static readonly string[] BlazeResources = ["blaze-1/frame-0", "blaze-1/frame-1"];
    private BattleSceneState? _scene;
    private double _elapsed;
    private int _drawCount;
    private IReadOnlyList<Texture2D>? _breath;
    private Texture2D[]? _blaze;
    private Vector2 _blazePosition;
    private bool _reducedFlash;

    internal void Reset()
    {
        Hide();
        _scene = null;
        _breath = null;
        _blaze = null;
    }

    internal void Present(BattleSceneState scene, IReadOnlyList<Texture2D>? breath = null)
    { _scene = scene; _breath = breath; _elapsed = 0; Show(); QueueRedraw(); }

    internal void PrepareBlaze(BattleSceneState scene, Texture2D[] frames, bool targetIsAlly, bool reducedFlash)
    {
        _scene = scene;
        _blaze = frames;
        _blazePosition = targetIsAlly ? new(176, 128) : new(72, 112);
        _reducedFlash = reducedFlash;
        AdvanceBlaze(-1);
        Show();
    }

    internal void AdvanceBlaze(double age)
    {
        _elapsed = age;
        QueueRedraw();
    }

    private int? BlazeFrame => _blaze is null || _elapsed < 0 || _elapsed >= BlazeDuration ? null :
        _reducedFlash ? 0 : (int)(_elapsed * 10) % 2;

    private float BlazeOpacity => (float)Math.Clamp(Math.Min(_elapsed * 15, (BlazeDuration - _elapsed) * 10), 0, 1);

    internal bool Advance(double elapsed)
    {
        _elapsed = elapsed; QueueRedraw();
        return elapsed >= (_scene!.Phase is BattleScenePhase.ActionAnimation or BattleScenePhase.Reaction ? 0.6 : 0.2);
    }
    private byte? Selector => _scene?.AttackSpell?.SourceAnimationSelector;
    private int Variation => (Selector.GetValueOrDefault() >> 5) + 1;

    public override void _Draw()
    {
        if (_scene?.AttackSpell is not { } spell) return;
        if (_blaze is not null)
        {
            if (BlazeFrame is not { } frame) return;
            DrawTexture(_blaze[frame], _blazePosition, new Color(1, 1, 1, BlazeOpacity));
            _drawCount++;
            return;
        }
        _drawCount++;
        if (_breath is { } particles)
        {
            for (int particle = 0; particle < 8; particle++)
            {
                float travel = (float)(_elapsed - particle * 0.04) / 0.32f;
                if (travel is < 0 or > 1) continue;
                var position = new Vector2(156 - 112 * travel, 94 + (particle % 3 - 1) * travel * 10);
                DrawTexture(particles[(int)(travel * 8) % particles.Count], position);
            }
            return;
        }
        DrawRect(new Rect2(0, 44, 256, 122), new Color("18243a"));
        var fire = spell.Element == AttackSpellElement.Fire || (Selector & 31) == 1;
        var lightning = spell.Element == AttackSpellElement.Lightning || (Selector & 31) == 7;
        var color = new Color(fire ? "ff984d" : "9ce6e7");
        for (int i = 0; i < _scene.AttackTargets.Count; i++)
        {
            Vector2 position = new(128 + (i - (_scene.AttackTargets.Count - 1) / 2f) * 30, 112);
            float radius = 9 + 7 * (float)Math.Sin(Math.Min(1, _elapsed / 0.6) * Math.PI);
            if (fire || lightning)
            {
                for (int pulse = 0; pulse < Variation; pulse++)
                {
                    var center = position + new Vector2((pulse - (Variation - 1) / 2f) * 9, pulse % 2 * 5);
                    if (fire)
                        DrawColoredPolygon([center + new Vector2(0, -radius), center + new Vector2(radius / 2, radius / 2),
                            center + new Vector2(-radius / 2, radius / 2)], color);
                    else
                        DrawPolyline([center + new Vector2(4, -2 * radius), center + new Vector2(-5, -2),
                            center + new Vector2(4, -2), center + new Vector2(-4, radius)], new Color("ffe484"), 2);
                }
            }
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
        element = _scene?.AttackSpell?.Element.ToString(), selector = Selector,
        variation = Selector.HasValue ? (int?)Variation : null, targets = _scene?.AttackTargets.Select(a => a.Value),
        target = _scene?.Target?.Value, amount = _scene?.Amount, modern = true, preparedBreath = _breath is not null,
        preparedBlaze = _blaze is null ? null : new
        {
            frame = BlazeFrame is { } frame ? BlazeResources[frame] : null,
            x = _blazePosition.X, y = _blazePosition.Y, reducedFlash = _reducedFlash, opacity = BlazeOpacity,
        }
    };
}

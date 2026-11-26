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
    private Func<int, BattleSceneRaster>? _breath;
    private Func<int, BattleSceneRaster>? _blaze;
    private BlazeMeteorEffect? _blazeMeteors;
    private BlastEffect? _blast;
    private BoltEffect? _bolt;
    private int _boltCue = -1;
    private FreezeEffect? _freeze;
    private int _freezeCue = -1;
    private KiwiBreathEffect? _kiwi;
    private int _kiwiCue = -1;
    private AquaEffect? _aqua;
    private int _aquaCue = -1;
    private Vector2 _blazePosition;
    private bool _reducedFlash;

    internal void Reset()
    {
        Hide();
        _scene = null;
        _breath = null;
        _blaze = null;
        _blazeMeteors = null;
        _blast = null;
        _bolt = null;
        _boltCue = -1;
        _freeze = null;
        _kiwi = null;
        _kiwiCue = -1;
        _aqua = null;
        _aquaCue = -1;
        _freezeCue = -1;
    }

    internal void Present(BattleSceneState scene, Func<int, BattleSceneRaster>? breath = null)
    { _scene = scene; _breath = breath; _elapsed = 0; Show(); QueueRedraw(); }

    internal void PrepareAttackSpell(BattleSceneState scene, Func<int, BattleSceneRaster> frames, bool targetIsAlly, bool reducedFlash)
    {
        _scene = scene;
        if (scene.IsMonsterBreath && scene.AttackAnimationSelector == 76)
            _kiwi = new(frames, scene.DisplayedAlly == scene.Actor, targetIsAlly, reducedFlash);
        else if (scene.AttackAnimationSelector is 22 or 54)
            _aqua = new(frames, targetIsAlly, reducedFlash, scene.AttackSpell!.Spell.Level);
        else if (scene.AttackAnimationSelector is 7 or 39 or 71 or 103)
            _bolt = new(frames, targetIsAlly, reducedFlash, (scene.AttackAnimationSelector.Value >> 5) + 1);
        else if (scene.AttackAnimationSelector is 2 or 34 or 66 or 98)
            _freeze = new(frames, targetIsAlly, reducedFlash, (scene.AttackAnimationSelector.Value >> 5) + 1);
        else if (scene.AttackSpell!.Spell.Value == "blast")
            _blast = new(frames, targetIsAlly, reducedFlash, (scene.AttackAnimationSelector!.Value >> 5) + 1);
        else if (scene.AttackAnimationSelector is 33 or 65 or 97)
            _blazeMeteors = new(frames, targetIsAlly, reducedFlash, (scene.AttackAnimationSelector.Value >> 5) + 1);
        else
        {
            _blaze = frames;
            _blazePosition = targetIsAlly ? new(176, 128) : new(72, 112);
        }
        _reducedFlash = reducedFlash;
        AdvancePrepared(-1);
        Show();
    }

    internal double PreparedDuration => _kiwi is not null ? KiwiBreathEffect.Duration : _aqua is not null ? AquaEffect.Duration : _bolt is not null ? BoltEffect.Duration : _freeze is not null ? FreezeEffect.Duration : _blast is not null ? BlastEffect.Duration : _blazeMeteors is null ? BlazeDuration : _blazeMeteors.Duration;

    internal void AdvancePrepared(double age)
    {
        _elapsed = age;
        QueueRedraw();
    }

    internal int? PreparedSound()
    {
        if (_kiwi is not null)
        {
            if (_elapsed < 0 || _elapsed >= 281.0 / 60) return null;
            int spawn = (int)(_elapsed * 60) / 4;
            if (spawn <= _kiwiCue) return null;
            _kiwiCue = spawn;
            return 92;
        }
        if (_aqua is not null)
        {
            if (_elapsed < 0 || _elapsed >= 228.0 / 60) return null;
            int spawn = (int)(_elapsed * 60) / 9;
            if (spawn <= _aquaCue) return null;
            _aquaCue = spawn;
            return 104;
        }
        if (_bolt is not null)
        {
            if (_elapsed < 4.0 / 60 || _elapsed >= 98.0 / 60) return null;
            int tick = (int)(_elapsed * 60), cycle = tick / 36;
            if (tick % 36 < 4) return null;
            int pulse = cycle * 2 + (tick % 36 >= 16 ? 1 : 0);
            if (pulse <= _boltCue) return null;
            _boltCue = pulse;
            return 119; // Two audible strike starts per modern cycle; shared YM5 replacement.
        }
        if (_freeze is null || _elapsed < 0.1 || _elapsed >= FreezeEffect.Duration) return null;
        int cue = (int)((_elapsed - 0.1) / 0.3);
        if (cue <= _freezeCue) return null;
        _freezeCue = cue;
        return 120; // Modern cadence; source repeats at random 10..23-tick intervals.
    }

    private int? BlazeFrame => _blaze is null || _elapsed < 0 || _elapsed >= BlazeDuration ? null :
        _reducedFlash ? 0 : (int)(_elapsed * 10) % 2;

    private float BlazeOpacity => (float)Math.Clamp(Math.Min(_elapsed * 15, (BlazeDuration - _elapsed) * 10), 0, 1);

    internal bool Advance(double elapsed)
    {
        _elapsed = elapsed; QueueRedraw();
        return elapsed >= (_scene!.Phase is BattleScenePhase.ActionAnimation or BattleScenePhase.Reaction ? 0.6 : 0.2);
    }
    private byte? Selector => _scene?.AttackAnimationSelector;
    private int Variation => (Selector.GetValueOrDefault() >> 5) + 1;

    public override void _Draw()
    {
        if (_scene?.AttackSpell is not { } spell) return;
        if (_kiwi is { } kiwi)
        {
            if (kiwi.Draw(this, _elapsed)) _drawCount++;
            return;
        }
        if (_aqua is { } aqua)
        {
            if (aqua.Draw(this, _elapsed)) _drawCount++;
            return;
        }
        if (_bolt is { } bolt)
        {
            if (bolt.Draw(this, _elapsed)) _drawCount++;
            return;
        }
        if (_freeze is { } freeze)
        {
            if (freeze.Draw(this, _elapsed)) _drawCount++;
            return;
        }
        if (_blast is { } blast)
        {
            if (blast.Draw(this, _elapsed)) _drawCount++;
            return;
        }
        if (_blazeMeteors is { } blazeTwo)
        {
            if (blazeTwo.Draw(this, _elapsed)) _drawCount++;
            return;
        }
        if (_blaze is not null)
        {
            if (BlazeFrame is not { } frame) return;
            _blaze(frame).Draw(this, _blazePosition, new Color(1, 1, 1, BlazeOpacity));
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
                particles((int)(travel * 8) % 2).Draw(this, position);
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
        target = _scene?.Target?.Value, amount = _scene?.Amount, modern = true, preparedBreath = _kiwi is not null || _breath is not null,
        preparedKiwi = _kiwi?.Observe(_elapsed),
        preparedAqua = _aqua?.Observe(_elapsed),
        preparedBlast = _blast?.Observe(_elapsed),
        preparedBolt = _bolt?.Observe(_elapsed),
        preparedFreeze = _freeze?.Observe(_elapsed),
        preparedBlazeTwo = _blazeMeteors?.Variation == 2 ? _blazeMeteors.Observe(_elapsed) : null,
        preparedBlazeFamily = _blazeMeteors?.Observe(_elapsed),
        preparedBlaze = _blaze is null ? null : ObserveBlaze(),
    };

    private object ObserveBlaze()
    {
        int? frame = BlazeFrame;
        BattleSceneRaster? raster = frame is { } index ? _blaze!(index) : null;
        return new
        {
            frame = frame is { } slot ? BlazeResources[slot] : null,
            x = _blazePosition.X, y = _blazePosition.Y, reducedFlash = _reducedFlash, opacity = BlazeOpacity,
            width = raster?.Size.X, height = raster?.Size.Y, hd = raster?.Hd,
            textureWidth = raster?.Texture.GetWidth(), textureHeight = raster?.Texture.GetHeight(),
        };
    }
}

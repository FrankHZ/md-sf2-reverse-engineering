using Godot;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// Source composition on the existing finite clock. Results only select the visual;
// this presenter never writes HP, removal, custody or rewards.
internal sealed partial class DesoulEffectView : Node2D
{
    internal static readonly string[] Resources = ["desoul/hover", "desoul/strike", "desoul/soul-0", "desoul/soul-1"];
    internal const double StrikeDuration = 52.0 / 60;
    private Func<int, BattleSceneRaster>? _frames;
    private double _age, _trigger, _reactionAge = -1;
    private int _variation, _lastHover = -1, _draws;
    private bool _targetAlly, _reduced, _castCue, _strikeCue;
    private BattleScenePhase _phase;
    private Sprite2D[] _backdrops = [];
    private readonly ShaderMaterial _ripple = new()
    {
        Shader = new Shader { Code = """
            shader_type canvas_item;
            uniform vec2 source_size;
            uniform int phase;
            const float offsets[16] = float[16](5.0,0.0,3.0,0.0,-3.0,0.0,-5.0,0.0,-5.0,0.0,-3.0,0.0,3.0,0.0,5.0,0.0);
            void fragment() {
                vec2 uv = UV;
                int row = int(floor(UV.y * source_size.y));
                if (row < 96) uv.x = fract(uv.x - offsets[(row + phase) % 16] / source_size.x);
                COLOR = texture(TEXTURE, uv);
            }
            """ }
    };
    internal void ConfigureBackdrop(params Sprite2D[] sprites) => _backdrops = sprites;
    private void RefreshBackdrop()
    {
        bool ripple = Active && !_reduced && _age >= _trigger + 28.0 / 60;
        foreach (var sprite in _backdrops)
        {
            sprite.Material = ripple ? _ripple : null;
            if (ripple) _ripple.SetShaderParameter("source_size", sprite.GetRect().Size * sprite.Scale);
        }
        if (ripple) _ripple.SetShaderParameter("phase", (int)((_age - _trigger) * 60) & 15);
    }
    internal bool Active => _frames is not null;
    internal double CastDuration => _trigger + 1.0;
    internal bool Flash => _age >= _trigger && _age < _trigger + 28.0 / 60 && (int)((_age - _trigger) * 60) % 7 < 4;

    internal void Begin(Func<int, BattleSceneRaster> frames, double trigger, int variation, bool targetAlly, bool reduced)
    {
        Reset(); _frames = frames; _trigger = trigger; _variation = variation;
        _targetAlly = targetAlly; _reduced = reduced; Show();
    }

    internal void Present(BattleSceneState state)
    {
        _phase = state.Phase;
        if (_phase is BattleScenePhase.Reward or BattleScenePhase.End || state.IsFieldDeath) { Reset(); return; }
        if (_phase == BattleScenePhase.Reaction)
        { _reactionAge = state.IsInstantDeath ? 0 : -1; _strikeCue = false; }
        if (_phase == BattleScenePhase.TargetEnter) _reactionAge = -1;
        QueueRedraw();
    }

    internal void Reset()
    {
        _frames = null; _age = 0; _reactionAge = -1; _lastHover = -1;
        _castCue = _strikeCue = false; RefreshBackdrop(); Hide(); QueueRedraw();
    }

    internal void Advance(double delta, Action<int> sound)
    {
        if (!Active) return;
        _age += delta;
        if (!_castCue && _age >= _trigger) { _castCue = true; sound(77); }
        if (_reactionAge >= 0)
        {
            _reactionAge += delta;
            if (!_strikeCue && _reactionAge >= 20.0 / 60) { _strikeCue = true; sound(106); }
            if (_reactionAge >= StrikeDuration) _reactionAge = -1;
        }
        else if (_age >= _trigger + 28.0 / 60)
        {
            int cycle = (int)((_age - _trigger) / 0.7);
            if (cycle != _lastHover) { _lastHover = cycle; sound(110); }
        }
        RefreshBackdrop(); QueueRedraw();
    }

    private readonly record struct Part(int Frame, Vector2 Position, bool Flip);
    private IEnumerable<Part> Parts()
    {
        if (!Active || _age < _trigger + 28.0 / 60) yield break;
        double tick = (_age - _trigger - 28.0 / 60) * 60;
        Vector2 origin = _targetAlly ? new(144, 40) : new(64, 24);
        bool strike = _reactionAge >= 20.0 / 60 && _reactionAge < StrikeDuration;
        bool flip = _targetAlly ^ ((int)(tick / 42) % 2 != 0);
        Vector2 position = _reactionAge >= 0
            ? origin + new Vector2(-32, 16)
            : origin + new Vector2((float)Math.Sin(tick * Math.PI / 84) * 24 - 32,
                _reduced ? 0 : (float)Math.Sin(tick * Math.PI / 42) * 8);
        // Souls follow the reaper; source slots42/43 are behind slots38..41.
        if (_variation == 2)
            for (int i = 0; i < 2; i++)
            {
                int delay = i == 0 ? 30 : 20;
                int steps = (int)tick + 1;
                int direction = i == 0 ? -1 : 1;
                int distance = Math.Min(steps, delay);
                int remaining = Math.Max(0, steps - delay);
                int cycles = remaining / 60, tail = remaining % 60;
                int travel = distance - (cycles % 2 == 1 ? 60 : 0) + (cycles % 2 == 0 ? -tail : tail);
                float x = (32 + direction * travel) >> 1;
                bool right = direction * (steps < delay ? 1 : cycles % 2 == 0 ? -1 : 1) > 0;
                yield return new(2 + (_reduced ? 0 : (int)(tick / 6) % 2),
                    position + new Vector2(x, i == 0 ? -16 : 32), !right);
            }
        yield return new(strike ? 1 : 0, position, flip);
    }

    public override void _Draw()
    {
        foreach (var part in Parts())
        {
            var raster = _frames!(part.Frame);
            var position = part.Position.Round();
            if (part.Flip) position.X += raster.Size.X;
            DrawSetTransform(position, scale: new(part.Flip ? -1 : 1, 1));
            raster.Draw(this, Vector2.Zero); _draws++;
        }
        DrawSetTransform(Vector2.Zero);
    }

    internal object? Observe() => !Active ? null : new
    {
        variation = _variation, targetIsAlly = _targetAlly, reducedFlash = _reduced,
        ripple = _backdrops.Any(sprite => sprite.Material is not null),
        phase = _phase.ToString(), age = _age, reactionAge = _reactionAge, drawCount = _draws,
        sprites = Parts().Select(part =>
        {
            var raster = _frames!(part.Frame);
            return new { resource = Resources[part.Frame], x = part.Position.X, y = part.Position.Y,
                flipX = part.Flip, hd = raster.Hd, width = raster.Size.X, height = raster.Size.Y,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight() };
        }).ToArray(),
    };
}

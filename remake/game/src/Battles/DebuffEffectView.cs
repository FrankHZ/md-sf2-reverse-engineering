using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

// DEBUFF1's four foreground clouds. This clock never consumes gameplay RNG.
internal sealed partial class DebuffEffectView : Node2D
{
    internal static readonly string[] Resources = ["debuff-1/frame-0", "debuff-1/frame-1", "debuff-1/frame-2"];
    private static readonly int[] Frames = [0, 1, 2, 1, 0];
    private readonly Sprite2D[] _clouds = new Sprite2D[4];
    private readonly int[] _delays = new int[4], _steps = new int[4];
    private Func<int, BattleSceneRaster>? _rasters;
    private Random _spacing = new(9);
    private Vector2 _origin;
    private double _fraction;
    private int _tick, _trigger;
    internal double MinimumCastDuration => (_trigger + 30) / 60.0;
    internal bool Active => _rasters is not null;
    internal bool Flash => _tick >= _trigger && _tick < _trigger + 28 && (_tick - _trigger) % 7 < 4;
    internal double CastElapsed(double elapsed) => elapsed - Math.Clamp(elapsed - _trigger / 60.0, 0, 28.0 / 60);

    public override void _Ready()
    {
        for (int i = 0; i < _clouds.Length; i++)
        {
            _clouds[i] = new Sprite2D { Name = $"Cloud{i}", Centered = false, ZIndex = 4, Visible = false };
            AddChild(_clouds[i]);
        }
    }

    internal void Begin(Func<int, BattleSceneRaster> rasters, int trigger, bool targetAlly)
    {
        Reset();
        _rasters = rasters;
        _trigger = trigger;
        _origin = targetAlly ? new(168, 48) : new(64, 32);
        _spacing = new Random(9);
        for (int i = 0; i < 4; i++)
            _delays[i] = i == 0 ? 1 : i * 6 + _spacing.Next(4);
    }

    internal void Reset()
    {
        _rasters = null;
        _fraction = 0;
        _tick = 0;
        Array.Clear(_steps);
        foreach (var cloud in _clouds) { cloud.Hide(); cloud.Texture = null; }
    }

    internal bool Advance(double delta)
    {
        _fraction += delta * 60;
        int ticks = (int)_fraction;
        _fraction -= ticks;
        bool cue = false;
        for (int tick = 0; tick < ticks; tick++, _tick++)
        {
            cue |= _tick == _trigger;
            if (_tick < _trigger + 28) continue;
            for (int i = 0; i < 4; i++)
            {
                if (--_delays[i] != 0) continue;
                _steps[i] = (_steps[i] + 1) % 6;
                _delays[i] = _steps[i] == 0 ? 5 + _spacing.Next(8) : 5;
                if (_steps[i] == 1) _clouds[i].Position = _origin + new Vector2(_spacing.Next(32), _spacing.Next(32));
            }
        }
        RefreshTextures();
        return cue;
    }

    internal void RefreshTextures()
    {
        if (_rasters is null) return;
        for (int i = 0; i < 4; i++)
        {
            _clouds[i].Visible = _steps[i] != 0;
            if (_steps[i] != 0) _rasters(Frames[_steps[i] - 1]).Bind(_clouds[i]);
        }
    }

    internal object? Observe() => !Active ? null : new
    {
        flash = Flash,
        sprites = _clouds.Select((cloud, i) => new { cloud, step = _steps[i] })
            .Where(row => row.step != 0).Select(row => new
            {
                name = row.cloud.Name.ToString(), resource = Resources[Frames[row.step - 1]],
                hd = _rasters!(Frames[row.step - 1]).Hd,
                x = row.cloud.Position.X, y = row.cloud.Position.Y, z = row.cloud.ZIndex,
            }).ToArray(),
    };
}

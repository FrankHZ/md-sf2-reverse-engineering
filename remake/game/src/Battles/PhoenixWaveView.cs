using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

internal partial class PhoenixWaveView : Node2D
{
    internal const int Selector = 61;
    private const int WaveTicks = 24;
    private const int IntervalTicks = 10;
    private const int WaveCount = 3;
    internal const double Duration = (WaveTicks + (WaveCount - 1) * IntervalTicks) / 60.0;
    internal static readonly string[] Resources =
        ["phoenix-wave/frame-0", "phoenix-wave/frame-1", "phoenix-wave/frame-2", "phoenix-wave/frame-3"];

    private readonly Sprite2D[] _waves = new Sprite2D[WaveCount];
    private Func<int, BattleSceneRaster>? _frames;
    private bool _fromAlly;
    private double _elapsed;

    internal bool Active => _frames is not null;

    public override void _Ready()
    {
        for (int i = 0; i < _waves.Length; i++)
        {
            _waves[i] = new Sprite2D { Centered = false, Visible = false };
            AddChild(_waves[i]);
        }
    }

    internal void Begin(Func<int, BattleSceneRaster> frames, bool fromAlly)
    {
        _frames = frames;
        _fromAlly = fromAlly;
        _elapsed = -1;
    }

    internal void Reset()
    {
        _frames = null;
        foreach (var wave in _waves)
        {
            wave.Hide();
            wave.Texture = null;
        }
    }

    internal void Advance(double elapsed)
    {
        _elapsed = elapsed;
        RefreshTextures();
    }

    internal void RefreshTextures()
    {
        if (_frames is null) return;
        for (int i = 0; i < _waves.Length; i++)
        {
            var wave = _waves[i];
            double age = Age(i);
            wave.Visible = age >= 0 && age < WaveTicks;
            if (!wave.Visible) continue;

            int tick = (int)age;
            int x = 156 - 3 * tick;
            _frames(tick / 6).Bind(wave);
            wave.FlipH = !_fromAlly;
            wave.Position = new(_fromAlly ? x : 256 - 32 - x, 84);
        }
    }

    private double Age(int wave) => _elapsed * 60 - wave * IntervalTicks;

    internal object? Observe() => Active ? new
    {
        selector = Selector,
        waves = _waves.Select((wave, index) => (wave, index)).Where(entry => entry.wave.Visible).Select(entry => new
        {
            x = entry.wave.Position.X,
            y = entry.wave.Position.Y,
            frame = (int)Age(entry.index) / 6,
        }).ToArray(),
    } : null;
}

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
    private Texture2D[] _frames = [];
    private bool _fromAlly;

    internal bool Active => _frames.Length != 0;

    public override void _Ready()
    {
        for (int i = 0; i < _waves.Length; i++)
        {
            _waves[i] = new Sprite2D { Centered = false, Visible = false };
            AddChild(_waves[i]);
        }
    }

    internal void Begin(Texture2D[] frames, bool fromAlly)
    {
        _frames = frames;
        _fromAlly = fromAlly;
    }

    internal void Reset()
    {
        _frames = [];
        foreach (var wave in _waves) wave.Hide();
    }

    internal void Advance(double elapsed)
    {
        for (int i = 0; i < _waves.Length; i++)
        {
            var wave = _waves[i];
            double age = elapsed * 60 - i * IntervalTicks;
            wave.Visible = age >= 0 && age < WaveTicks;
            if (!wave.Visible) continue;

            int tick = (int)age;
            int x = 156 - 3 * tick;
            wave.Texture = _frames[tick / 6];
            wave.FlipH = !_fromAlly;
            wave.Position = new(_fromAlly ? x : 256 - 32 - x, 84);
        }
    }

    internal object? Observe() => Active ? new
    {
        selector = Selector,
        waves = _waves.Where(wave => wave.Visible).Select(wave => new
        {
            x = wave.Position.X,
            y = wave.Position.Y,
            frame = Array.IndexOf(_frames, wave.Texture),
        }).ToArray(),
    } : null;
}

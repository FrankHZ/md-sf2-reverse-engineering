using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

// The DETOX tileset-7 mote and trail, on the existing presentation clock.
internal sealed partial class DetoxEffectView : Node2D
{
    private const int FlashTicks = 28;
    private const int MoteTicks = 38;
    private const int TrailFrameTicks = 6;
    private const int TrailFrames = 5;
    internal static readonly string[] Resources = Enumerable.Range(0, 11).Select(i => $"detox/frame-{i}").ToArray();

    // The 8-angle samples used by sub_1812 and table_1840.
    private static readonly int[] ShrinkFrames = [4, 3, 2, 1, 0];
    private static readonly int[] QuarterSine = [0, 49, 97, 142, 181, 212, 236, 251, 256];
    private readonly record struct Trail(int Age, Vector2 Position, bool Front);
    private readonly Sprite2D _mote = new() { Name = "Mote", Centered = false, Visible = false, ZIndex = 4 };
    private readonly Sprite2D[] _sprites = new Sprite2D[24];
    private readonly Trail[] _trails = new Trail[24];

    private Func<int, BattleSceneRaster>? _frames;
    private Random _spacing = null!;
    private double _fraction;
    private int _tick, _trigger, _moteAge, _moteFrame, _angle, _nextTrail;
    private bool _secondCenter, _closing;

    internal bool Active => _frames is not null;
    internal bool Flash => _tick >= _trigger && _tick < _trigger + FlashTicks &&
        (_tick - _trigger) % 7 < 4;
    internal bool Finishing => _moteAge != 0 || _trails.Any(trail => trail.Age != 0);

    public override void _Ready()
    {
        AddChild(_mote);
        for (int i = 0; i < _sprites.Length; i++)
        {
            _sprites[i] = new Sprite2D { Name = $"Trail{i}", Centered = false, Visible = false };
            AddChild(_sprites[i]);
        }
    }

    internal void Begin(Func<int, BattleSceneRaster> frames, int trigger)
    {
        Reset();
        _frames = frames;
        _trigger = trigger;
        _spacing = new Random(7);
    }

    internal double CastElapsed(double elapsed) =>
        elapsed - Math.Clamp(elapsed - _trigger / 60.0, 0, FlashTicks / 60.0);

    internal void Stop()
    {
        _closing = true;
        if (_moteAge != 0) _moteAge = 1;
    }

    internal void Reset()
    {
        _frames = null;
        _fraction = 0;
        _tick = _moteAge = _moteFrame = _angle = 0;
        _nextTrail = 1;
        _secondCenter = _closing = false;
        Array.Clear(_trails);
        foreach (var sprite in _sprites.Prepend(_mote))
        {
            sprite.Hide();
            sprite.Texture = null;
        }
    }

    internal bool Advance(double delta)
    {
        _fraction += delta * 60;
        int ticks = (int)_fraction;
        _fraction -= ticks;
        bool cue = false;
        for (int i = 0; i < ticks; i++)
        {
            cue |= _tick == _trigger;
            Step();
            _tick++;
        }
        if (ticks != 0) RefreshTextures();
        return cue;
    }

    private void Step()
    {
        for (int i = 0; i < _trails.Length; i++)
            if (_trails[i] is { Age: > 0 } trail)
                _trails[i] = trail with { Age = trail.Age == TrailFrames * TrailFrameTicks ? 0 : trail.Age + 1 };
        if (_tick < _trigger + FlashTicks) return;
        if (_moteAge == 0)
        {
            if (_closing) return;
            _moteAge = 1;
        }

        _moteAge = Math.Min(_moteAge + 1, MoteTicks);
        int stage = Math.Min((_moteAge - 2) / 6, 5);
        if (stage > 0)
            _moteFrame = _closing ? ShrinkFrames[stage - 1] : stage;

        if (_closing && _moteAge == MoteTicks)
        {
            _moteAge = 0;
            return;
        }

        // Both source centers are visited by the same effect, even when the engine
        // sets a caster mirror bit. The source routine never reads that bit.
        int angle = (_angle + (_secondCenter ? 8 : -8)) & 255;
        _mote.Position = new((_secondCenter ? 80 : 176) + ((48 * Sine(angle)) >> 8),
            64 + ((16 * Sine(angle + 64)) >> 8));
        if (_secondCenter && angle == 8)
        {
            _angle = 120;
            _secondCenter = false;
        }
        else if (!_secondCenter && angle == 136)
        {
            _angle = 8;
            _secondCenter = true;
        }
        else _angle = angle;

        if (--_nextTrail != 0) return;
        // Retail draws range 8 and adds 2. This stream is local to delivery.
        _nextTrail = _spacing.Next(8) + 2;
        int slot = Array.FindIndex(_trails, trail => trail.Age == 0);
        if (slot >= 0) _trails[slot] = new(1, _mote.Position + new Vector2(8, 8), angle >= 128);
    }

    private static int Sine(int angle)
    {
        int phase = (angle & 127) / 8;
        int magnitude = QuarterSine[Math.Min(phase, 16 - phase)];
        return (angle & 128) == 0 ? magnitude : -magnitude;
    }

    internal void RefreshTextures()
    {
        if (_frames is not { } frames) return;
        _mote.Visible = _moteAge != 0;
        if (_mote.Visible) frames(_moteFrame).Bind(_mote);
        for (int i = 0; i < _sprites.Length; i++)
        {
            var sprite = _sprites[i];
            var trail = _trails[i];
            sprite.Visible = trail.Age != 0;
            if (!sprite.Visible) continue;
            frames(TrailFrame(trail)).Bind(sprite);
            sprite.Position = trail.Position + Vector2.Down * (trail.Age - 1);
            sprite.ZIndex = trail.Front ? 4 : 0;
        }
    }

    private static int TrailFrame(Trail trail) => 6 + (trail.Age - 1) / TrailFrameTicks;

    internal object? Observe() => Active ? new
    {
        flash = Flash,
        closing = _closing,
        sprites = _sprites.Select((sprite, i) => (sprite, frame: TrailFrame(_trails[i])))
            .Prepend((sprite: _mote, frame: _moteFrame))
            .Where(entry => entry.sprite.Visible)
            .Select(entry => new
            {
                name = entry.sprite.Name.ToString(),
                resource = Resources[entry.frame],
                hd = _frames!(entry.frame).Hd,
                x = entry.sprite.Position.X,
                y = entry.sprite.Position.Y,
                z = entry.sprite.ZIndex,
            }).ToArray(),
    } : null;
}

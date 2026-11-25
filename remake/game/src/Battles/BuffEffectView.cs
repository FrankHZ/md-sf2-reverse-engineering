using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// BUFF1 has no spell tiles: a cast flash and accelerating PSH cues accompany
// the existing actor raster. Presentation time never consumes gameplay RNG.
internal sealed class BuffEffectView
{
    private int _trigger, _tick, _delay, _interval;
    private double _fraction;
    internal bool Active { get; private set; }
    internal bool Flash => Active && _tick >= _trigger && _tick < _trigger + 28 && (_tick - _trigger) % 7 < 4;
    internal double Duration => (_trigger + 28 + 212) / 60.0;
    internal double CastElapsed(double elapsed) => elapsed - Math.Clamp(elapsed - _trigger / 60.0, 0, 28.0 / 60);
    internal int Pulses { get; private set; }
    internal static int? Selector(BattleSceneState? state) => state is { IsSilencedCast: false, SupportSpell.Spell: var spell }
        ? (spell.Value, spell.Level) switch
        {
            ("boost", 1) or ("attack", 1) => 8,
            ("boost", 2) => 40,
            _ => null,
        } : null;

    internal void Begin(int trigger)
    {
        Reset();
        Active = true;
        _trigger = trigger;
        _delay = 1;
        _interval = 30;
    }

    internal void Reset()
    {
        Active = false;
        _fraction = 0;
        _tick = Pulses = 0;
    }

    internal void Advance(double delta, Action<int> sound)
    {
        if (!Active) return;
        _fraction += delta * 60;
        int ticks = (int)_fraction;
        _fraction -= ticks;
        for (int i = 0; i < ticks; i++, _tick++)
        {
            if (_tick == _trigger) sound(77);
            if (_tick < _trigger + 28 || _interval <= 0 || --_delay != 0) continue;
            _interval -= 2;
            if (_interval <= 0) continue;
            _delay = _interval;
            Pulses++;
            sound(97);
        }
    }

    internal object? Observe() => !Active ? null : new { flash = Flash, pulses = Pulses, interval = _interval };
}

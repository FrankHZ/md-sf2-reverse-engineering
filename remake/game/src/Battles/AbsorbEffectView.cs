using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// ABSORB loads no tiles and its update is RTS. Only the cast flash and warp cue
// belong here; the existing support continuation owns both MP reactions.
internal sealed class AbsorbEffectView
{
    private double _elapsed;
    private int _trigger;
    private bool _soundPlayed;
    internal bool Active { get; private set; }
    internal bool Flash => Active && Tick >= _trigger && Tick < _trigger + 28 && (Tick - _trigger) % 7 < 4;
    private int Tick => (int)(_elapsed * 60);
    internal static int? Selector(BattleSceneState? state) =>
        state is { IsSilencedCast: false, EquipmentItem: not null, SupportSpell.Spell.Value: "spoit" } ? 10 : null;
    internal double CastElapsed(double elapsed) => elapsed - Math.Clamp(elapsed - _trigger / 60.0, 0, 28.0 / 60);

    internal void Begin(int trigger)
    { Reset(); Active = true; _trigger = trigger; }

    internal void Reset()
    { Active = false; _elapsed = 0; _soundPlayed = false; }

    internal void Advance(double delta, Action<int> sound)
    {
        if (!Active) return;
        _elapsed += delta;
        if (!_soundPlayed && Tick >= _trigger) { _soundPlayed = true; sound(89); }
    }

    internal object? Observe() => !Active ? null : new
    { flash = Flash, tick = Tick, trigger = _trigger, soundPlayed = _soundPlayed };
}

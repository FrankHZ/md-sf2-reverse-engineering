using Godot;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// Invocation1: two opposing fists, two poses. Cosmetic choices never draw gameplay RNG.
internal sealed partial class AtlasEffectView : Node2D
{
    internal static readonly string[] Resources = ["atlas/summon-0", "atlas/summon-1"];
    private Func<int, BattleSceneRaster>? _frames;
    private double _age, _trigger;
    private bool _reduced, _castCue;
    private int _variation, _draws, _lastTick = -1, _shakeAge = 4;
    internal bool Active => _frames is not null;
    private double Tick => (_age - _trigger) * 60;
    internal bool ReplacesCaster => Active && Tick >= 28;
    internal double CastDuration => _trigger + 116.0 / 60;
    internal bool Flash => Active && Tick >= 0 && Tick < 28 && (int)Tick % 7 < 4;
    internal Vector2 Shake => !Active || _reduced || _shakeAge >= 4 ? Vector2.Zero :
        new Vector2(0, new[] { -3, 3, 2, -2 }[_shakeAge]);

    internal void Begin(Func<int, BattleSceneRaster> frames, double trigger, int variation, bool reduced)
    {
        Reset(); _frames = frames; _trigger = trigger; _variation = variation; _reduced = reduced; Show();
    }
    internal void Present(BattleSceneState state)
    {
        if (state.Phase is BattleScenePhase.Reward or BattleScenePhase.End || state.IsFieldDeath) Reset();
    }
    internal void Reset()
    {
        _frames = null; _age = 0; _castCue = false; _draws = 0; _lastTick = -1; _shakeAge = 4;
        Hide(); QueueRedraw();
    }
    internal void Advance(double delta, Action<int> sound)
    {
        if (!Active) return;
        _age += delta;
        if (!_castCue && Tick >= 0) { _castCue = true; sound(77); }
        // Walk crossed logical ticks so held acceleration cannot skip impact cues.
        int tick = (int)Math.Floor(Tick - 36);
        for (int step = _lastTick + 1; step <= tick; step++)
        {
            _shakeAge = Math.Min(4, _shakeAge + 1);
            for (int slot = 0; slot < 2; slot++)
            {
                int elapsed = step - (slot == 0 ? 6 : 1);
                if (elapsed >= 0 && elapsed % 14 == 12)
                {
                    sound((elapsed / 14 + slot) % 2 == 0 ? 83 : 84);
                    if (_shakeAge == 4) _shakeAge = 0;
                }
            }
        }
        _lastTick = Math.Max(_lastTick, tick);
        QueueRedraw();
    }
    private readonly record struct Part(int Slot, int Frame, Vector2 Position, bool FlipX);
    private IEnumerable<Part> Parts()
    {
        if (!Active || Tick < 36) yield break;
        for (int slot = 0; slot < 2; slot++)
        {
            int elapsed = (int)(Tick - 36) - (slot == 0 ? 6 : 1);
            if (elapsed < 0) continue;
            int cycle = elapsed / 14, age = elapsed % 14;
            bool mirror = (cycle + slot) % 2 == 0;
            // Source random x0..23/y0..47 and later random side become fixed phases.
            // Each pass moves +/-9 and +10 for11 ticks; pose0 holds through the
            // impact and two-tick reset. Source positions include the VDP128 bias.
            // LoadInvocationSpell puts frame0 in the alternate bank, then setup
            // loads frame1 into the base bank. layout+36 selects frame0 at impact.
            int movement = Math.Min(age, 11);
            int x = (mirror ? 16 : 304) + (cycle * 13 + slot * 7) % 24 - 128;
            int y = 16 + (cycle * 19 + slot * 23) % 48 - 128;
            yield return new(slot, age >= 11 ? 0 : 1,
                new(x + (mirror ? 9 : -9) * movement, y + 10 * movement), mirror);
        }
    }
    public override void _Draw()
    {
        foreach (var part in Parts())
        {
            var raster = _frames!(part.Frame);
            var position = part.Position.Round();
            if (part.FlipX) position.X += raster.Size.X;
            DrawSetTransform(position, scale: new(part.FlipX ? -1 : 1, 1));
            raster.Draw(this, Vector2.Zero); _draws++;
        }
        DrawSetTransform(Vector2.Zero);
    }
    internal object? Observe() => !Active ? null : new
    {
        variation = _variation, reducedFlash = _reduced, age = _age, replacesCaster = ReplacesCaster,
        flash = Flash, shakeY = Shake.Y, drawCount = _draws,
        sprites = Parts().Select(part =>
        {
            var raster = _frames!(part.Frame);
            return new { slot = part.Slot, resource = Resources[part.Frame], x = part.Position.X, y = part.Position.Y,
                flipX = part.FlipX, hd = raster.Hd, width = raster.Size.X, height = raster.Size.Y,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight() };
        }).ToArray(),
    };
}

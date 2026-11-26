using Godot;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// Invocation2 and tileset14. Presentation owns its clock; no gameplay RNG is consumed.
internal sealed partial class NeptunEffectView : Node2D
{
    internal static readonly string[] Resources = ["neptun/summon-0", "neptun/summon-1",
        "neptun/rise-0", "neptun/rise-1", "neptun/rise-2", "neptun/rain-top", "neptun/rain-bottom",
        "neptun/sizzle-0", "neptun/sizzle-1", "neptun/sizzle-2"];
    private static readonly int[] RainX = [8,21,34,46,61,73,86,99,113,125,139,151,165,178,191,203,218,230,248];
    private static readonly int[] BlinkDurations = [1,8,1,7,1,6,1,5,1,4,1,3,1,2,1,1,1,1,1,1,1];
    private Func<int, BattleSceneRaster>? _frames;
    private double _age, _trigger;
    private bool _reduced, _castCue, _manifestCue, _riseCue;
    private int _variation, _draws;
    private readonly int[] _sizzleCycles = [-1, -1, -1];
    internal bool Active => _frames is not null;
    private double Tick => (_age - _trigger) * 60;
    internal bool ReplacesCaster => Active && Tick >= 28;
    internal double CastDuration => _trigger + 187.0 / 60;
    internal bool Flash => Active && Tick >= 0 && Tick < 28 && (int)Tick % 7 < 4;
    internal Vector2 Shake => !Active || _reduced || Tick < 147 ? Vector2.Zero :
        new Vector2(new[] { 1, 0, -1, 0 }[(int)(Tick - 147) & 3], 0);

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
        _frames = null; _age = 0; _castCue = _manifestCue = _riseCue = false; _draws = 0;
        Array.Fill(_sizzleCycles, -1); Hide(); QueueRedraw();
    }
    internal void Advance(double delta, Action<int> sound)
    {
        if (!Active) return;
        _age += delta;
        if (!_castCue && Tick >= 0) { _castCue = true; sound(77); }
        if (!_manifestCue && Tick >= 36) { _manifestCue = true; sound(108); }
        if (!_riseCue && Tick >= 110) { _riseCue = true; sound(110); }
        for (int slot = 0; slot < 3; slot++)
        {
            int elapsed = (int)(Tick - 147) - (2 + slot * 3);
            if (Tick < 147 || elapsed < 0) continue;
            int cycle = elapsed / (26 + slot * 2);
            if (cycle != _sizzleCycles[slot]) { _sizzleCycles[slot] = cycle; sound(115); }
        }
        QueueRedraw();
    }
    private readonly record struct Part(int Frame, Vector2 Position);
    private IEnumerable<Part> Parts()
    {
        if (!Active || Tick < 36) yield break;
        // Setup blinks frame1 between source (256,176) and offscreen (1,1).
        // At110 switch back to frame0; at127 sprite toggles hide the body as
        // LoadSpellTileset overwrites its VRAM. Rain starts after20 more ticks.
        if (Tick < 127)
        {
            bool visible = true;
            if (Tick < 85 && !_reduced)
            {
                int remaining = (int)(Tick - 36), phase = 0;
                while (phase < BlinkDurations.Length - 1 && remaining >= BlinkDurations[phase])
                    remaining -= BlinkDurations[phase++];
                visible = phase % 2 == 0;
            }
            if (visible) yield return new(Tick < 110 ? 1 : 0, new(128, 48));
        }
        if (Tick >= 85 && Tick < 127)
        {
            int age = Math.Max(0, (int)(Tick - 110));
            int frame = Tick < 111 ? 0 : ((age + 1) / 2) % 2 == 1 ? 2 : 1;
            yield return new(2 + frame, new(168, 64 - (Tick < 110 ? 0 : 5 * (age + 1))));
        }
        if (Tick < 147) yield break;
        int tick = (int)(Tick - 147);
        // Source regenerates19 two-part streaks every3 updates with random
        // y24..135. Fixed phase offsets replace cosmetic RNG, preserving layers.
        for (int i = 0; i < RainX.Length; i++)
        {
            int y = 24 + (i * 37 + tick / 3 * 19) % 112;
            yield return new(5, new(RainX[i], y));
            yield return new(6, new(RainX[i], y + 32));
        }
        for (int slot = 0; slot < 3; slot++)
        {
            int elapsed = tick - (2 + slot * 3);
            if (elapsed < 0) continue;
            int period = 26 + slot * 2, age = elapsed % period;
            if (age >= 22 || (age >= 10 && !_reduced && age % 2 == 0)) continue;
            int frame = age < 5 ? 0 : age < 10 ? 1 : 2;
            yield return new(7 + frame, new((slot * 83 + elapsed / period * 47) % 256, 128));
        }
    }
    public override void _Draw()
    {
        foreach (var part in Parts()) { _frames!(part.Frame).Draw(this, part.Position.Round()); _draws++; }
    }
    internal object? Observe() => !Active ? null : new
    {
        variation = _variation, reducedFlash = _reduced, age = _age, replacesCaster = ReplacesCaster,
        flash = Flash, drawCount = _draws,
        sprites = Parts().Select(part =>
        {
            var raster = _frames!(part.Frame);
            return new { resource = Resources[part.Frame], x = part.Position.X, y = part.Position.Y,
                hd = raster.Hd, width = raster.Size.X, height = raster.Size.Y,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight() };
        }).ToArray(),
    };
}

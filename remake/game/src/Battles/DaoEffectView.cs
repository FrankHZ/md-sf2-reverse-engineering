using Godot;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// Pinned invocation0 / DAO tileset3. Visual motion uses a fixed schedule;
// application owns recipients, division, payment, damage and return.
internal sealed partial class DaoEffectView : Node2D
{
    internal static readonly string[] Resources = Enumerable.Range(0, 8).Select(i => $"dao/summon-{i}")
        .Concat(new[] { "dao/rock-small", "dao/rock-medium", "dao/rock-large" }).ToArray();
    private Func<int, BattleSceneRaster>? _frames;
    private double _age, _trigger;
    private bool _reduced, _castCue, _manifestCue;
    private int _variation, _rumble = -1, _draws;
    private BattleScenePhase _phase;
    internal bool Active => _frames is not null;
    private double Tick => (_age - _trigger) * 60;
    internal bool ReplacesCaster => Active && Tick >= 28;
    internal float DistortionTick => Active && !_reduced && Tick >= 62 ? (float)(Tick - 62) : -1;
    internal double CastDuration => _trigger + 152.0 / 60;
    internal bool Flash => Active && Tick >= 0 && Tick < 28 && (int)Tick % 7 < 4;
    internal bool RockFlash => Active && Tick >= 62 && (int)(Tick - 62) % 45 < 2;
    internal Vector2 Shake => !Active || _reduced || Tick < 62 ? Vector2.Zero :
        new Vector2(new[] { -1, 1, -2, -2, -1, -1, 1, 1, 2, 0, 3, 3, 2, 2, 0, 0 }[(int)Tick & 15],
                    new[] { 0, 2, 1, 1, 1, -1, 0, 0 }[(int)Tick & 7]);

    internal void Begin(Func<int, BattleSceneRaster> frames, double trigger, int variation, bool reduced)
    {
        Reset(); _frames = frames; _trigger = trigger; _variation = variation; _reduced = reduced; Show();
    }
    internal void Present(BattleSceneState state)
    {
        _phase = state.Phase;
        if (_phase is BattleScenePhase.Reward or BattleScenePhase.End || state.IsFieldDeath) Reset();
    }
    internal void Reset()
    {
        _frames = null; _age = 0; _rumble = -1; _castCue = _manifestCue = false; _draws = 0;
        Hide(); QueueRedraw();
    }
    internal void Advance(double delta, Action<int> sound)
    {
        if (!Active) return;
        _age += delta;
        if (!_castCue && Tick >= 0) { _castCue = true; sound(77); }
        if (!_manifestCue && Tick >= 41) { _manifestCue = true; sound(118); }
        if (Tick >= 62)
        {
            int rumble = (int)(Tick - 62) / 8;
            if (rumble != _rumble) { _rumble = rumble; sound(93); }
        }
        QueueRedraw();
    }
    private readonly record struct Part(int Frame, Vector2 Position, bool FlipX = false, bool FlipY = false);
    private IEnumerable<Part> Parts()
    {
        if (!Active || Tick < 36) yield break;
        // Setup displays0, then1..5 on consecutive updates, holds5 for20,
        // then6 and7. Header (256,176) is biased by the VDP's128 origin.
        int frame = Tick < 41 ? Math.Clamp((int)Tick - 36, 0, 4) : Tick < 61 ? 5 : Tick < 62 ? 6 : 7;
        yield return new(frame, new(128, 48));
        if (Tick < 62) yield break;
        // The source emits at most16 rising rocks, starting at y128..191,
        // with -1792..-1025 /256 velocity and +6/256 acceleration.
        // A repeatable visual schedule replaces source cosmetic RNG draws.
        int tick = (int)(Tick - 62);
        for (int slot = 0; slot < 16; slot++)
        {
            int start = slot * 8;
            if (tick < start) continue;
            int age = (tick - start) % 128;
            float y = 128 + slot * 17 % 64 + age * (-1792 + slot * 47) / 256f + 3 * age * (age - 1) / 256f;
            if (y <= -32 || age > 52) continue;
            yield return new(8 + slot % 3, new(slot * 71 % 256, y), slot % 2 != 0, slot % 4 >= 2);
        }
    }
    public override void _Draw()
    {
        foreach (var part in Parts())
        {
            var raster = _frames!(part.Frame);
            var position = part.Position.Round();
            if (part.FlipX) position.X += raster.Size.X;
            if (part.FlipY) position.Y += raster.Size.Y;
            DrawSetTransform(position, scale: new(part.FlipX ? -1 : 1, part.FlipY ? -1 : 1));
            raster.Draw(this, Vector2.Zero); _draws++;
        }
        DrawSetTransform(Vector2.Zero);
    }
    internal object? Observe() => !Active ? null : new
    {
        variation = _variation, reducedFlash = _reduced, phase = _phase.ToString(), age = _age,
        replacesCaster = ReplacesCaster, flash = Flash, rockFlash = RockFlash, drawCount = _draws,
        sprites = Parts().Select(part =>
        {
            var raster = _frames!(part.Frame);
            return new { resource = Resources[part.Frame], x = part.Position.X, y = part.Position.Y,
                flipX = part.FlipX, flipY = part.FlipY, hd = raster.Hd, width = raster.Size.X, height = raster.Size.Y,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight() };
        }).ToArray(),
    };
}

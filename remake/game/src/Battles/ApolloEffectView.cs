using Godot;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// Invocation3 and tileset19; deterministic cosmetic scheduling never draws gameplay RNG.
internal sealed partial class ApolloEffectView : Node2D
{
    internal static readonly string[] Resources = Enumerable.Range(0, 3).Select(i => $"apollo/summon-{i}")
        .Concat(Enumerable.Range(0, 2).Select(i => $"apollo/mouth-{i}"))
        .Concat(Enumerable.Range(0, 4).Select(i => $"apollo/small-{i}"))
        .Concat(Enumerable.Range(0, 4).Select(i => $"apollo/large-{i}")).ToArray();
    private Func<int, BattleSceneRaster>? _frames;
    private double _age, _trigger;
    private bool _reduced, _castCue, _manifestCue;
    private int _variation, _flameCue = -1, _draws;
    internal bool Active => _frames is not null;
    private double Tick => (_age - _trigger) * 60;
    internal bool ReplacesCaster => Active && Tick >= 28;
    internal double CastDuration => _trigger + 148.0 / 60;
    internal bool Flash => Active && Tick >= 0 && Tick < 28 && (int)Tick % 7 < 4;
    internal bool FlameFlash => Active && Tick >= 108 && (int)(Tick - 108) % 9 < 1;
    internal Vector2 Shake => !Active || _reduced || Tick < 108 ? Vector2.Zero :
        new Vector2(new[] { 1, 2, 1, 0, -1, -2, -1, 0 }[(int)Tick & 7], 0);

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
        _frames = null; _age = 0; _flameCue = -1; _castCue = _manifestCue = false; _draws = 0;
        Hide(); QueueRedraw();
    }
    internal void Advance(double delta, Action<int> sound)
    {
        if (!Active) return;
        _age += delta;
        if (!_castCue && Tick >= 0) { _castCue = true; sound(77); }
        if (!_manifestCue && Tick >= 36) { _manifestCue = true; sound(118); }
        if (Tick >= 88)
        {
            int cue = Tick < 108 ? 0 : 1 + (int)(Tick - 108) / 6;
            if (cue != _flameCue) { _flameCue = cue; sound(116); }
        }
        QueueRedraw();
    }
    private readonly record struct Part(int Frame, Vector2 Position, bool FlipX = false, bool FlipY = false);
    private IEnumerable<Part> Parts()
    {
        if (!Active || Tick < 36) yield break;
        // Header272,1;22 lowering updates of8 pixels. Frame1 is preloaded
        // into the hidden bank; frame0 stays visible for30 more ticks.
        int frame = Tick < 88 ? 0 : Tick < 110 ? 1 : ((int)(Tick - 110) / 6) % 2 == 0 ? 2 : 1;
        yield return new(frame, new(144, Tick < 58 ? -127 + 8 * ((int)Tick - 35) : 49));
        if (Tick < 88) yield break;
        if (Tick < 108)
        {
            if ((int)Tick % 2 == 0) yield return new(3, new(146, 75));
            yield break;
        }
        int tick = (int)(Tick - 108);
        if ((tick + 4) % 6 >= 4)
        {
            int mouth = (tick + 4) / 6 % 2;
            yield return new(3 + mouth, mouth == 0 ? new(146, 75) : new(158, 76));
        }
        // Source chains up to20 particles,3-tick frames, two sizes, random
        // x0..127/y16..175 and flips; large particles drift left1/update.
        // Fixed positions and an alternating size retain composition without RNG.
        for (int slot = 0; slot < 20; slot++)
        {
            int start = slot * 2;
            if (tick < start) continue;
            int age = (tick - start) % 40;
            if (age >= 12) continue;
            bool large = slot % 2 != 0;
            yield return new((large ? 9 : 5) + age / 3,
                new(slot * 47 % 128 - (large ? age : 0), 16 + slot * 31 % 160), slot % 4 >= 2, slot % 3 == 0);
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
        variation = _variation, reducedFlash = _reduced, age = _age, replacesCaster = ReplacesCaster,
        flash = Flash, flameFlash = FlameFlash, drawCount = _draws,
        sprites = Parts().Select(part =>
        {
            var raster = _frames!(part.Frame);
            return new { resource = Resources[part.Frame], x = part.Position.X, y = part.Position.Y,
                flipX = part.FlipX, flipY = part.FlipY, hd = raster.Hd, width = raster.Size.X, height = raster.Size.Y,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight() };
        }).ToArray(),
    };
}

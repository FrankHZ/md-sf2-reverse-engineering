using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

// Original prepared composition, finite local clock and deterministic spreads.
// Source random waits/global palette flashes and exact hardware cadence are not replayed.
internal sealed class BoltEffect(Func<int, BattleSceneRaster> frames, bool targetIsAlly, bool reducedFlash, int variation)
{
    internal static readonly string[] Resources = Enumerable.Range(0, 30).Select(i => "bolt/frame-" + i).ToArray();
    internal const double Duration = 120.0 / 60;
    private int OrbCount => variation == 4 ? 4 : variation - 1;
    private static readonly Vector2[] OrbPositions = [new(72, 64), new(24, 88), new(120, 72), new(176, 56)];
    private readonly record struct Sprite(int Slot, int Frame, Vector2 Position, float Opacity);

    private IEnumerable<Sprite> Sprites(double age)
    {
        if (age < 0 || age >= Duration) yield break;
        int tick = (int)(age * 60);
        for (int slot = 0; slot < variation; slot++)
        {
            int local = tick - slot * 4;
            if (local < 0 || local >= 108) continue;
            int cycle = local / 36, phase = local % 36;
            int frame = phase < 12 ? phase / 2 : phase is >= 16 and < 26 ? 6 + (phase - 16) / 2 : -1;
            if (frame < 0) continue;
            // Source chooses one of two eleven-frame tables and X in [16,111]
            // or [144,239]. Keep its side offset without consuming session RNG.
            int bank = (slot + cycle) % 2;
            int x = (targetIsAlly ? 144 : 16) + (slot * 29 + cycle * 37 + (phase >= 16 ? 17 : 0)) % 96;
            float opacity = reducedFlash ? 0.6f : 1;
            yield return new(slot, bank * 11 + frame, new(x - 24, targetIsAlly ? 56 : 48), opacity);
        }
        float fade = (float)Math.Clamp(Math.Min(age * 10, (Duration - age) * 10), 0, 1);
        for (int slot = 0; slot < OrbCount; slot++)
        {
            int phase = reducedFlash ? 0 : (tick / 3 + slot * 2) % 8;
            var position = OrbPositions[(slot + (targetIsAlly ? 2 : 0)) % 4];
            yield return new(4 + slot, 22 + phase, position, fade * (reducedFlash ? 0.6f : 1));
        }
    }

    internal bool Draw(Node2D node, double age)
    {
        bool drawn = false;
        foreach (var sprite in Sprites(age))
        {
            frames(sprite.Frame).Draw(node, sprite.Position, new Color(1, 1, 1, sprite.Opacity));
            drawn = true;
        }
        return drawn;
    }

    internal object Observe(double age) => new
    {
        variation, targetIsAlly, reducedFlash, duration = Duration, strikes = variation, orbs = OrbCount,
        sprites = Sprites(age).Select(sprite =>
        {
            var raster = frames(sprite.Frame);
            return new
            {
                slot = sprite.Slot, resource = Resources[sprite.Frame],
                x = sprite.Position.X, y = sprite.Position.Y,
                width = raster.Size.X, height = raster.Size.Y, hd = raster.Hd,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight(),
                opacity = sprite.Opacity,
            };
        }).ToArray(),
    };
}

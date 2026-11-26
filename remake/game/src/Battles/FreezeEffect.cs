using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

// Retail FREEZE composition on the adapter's finite, deterministic clock.
// No session RNG, fixed-point/hardware timing or source palette-flicker claim.
internal sealed class FreezeEffect(Func<int, BattleSceneRaster> frames, bool targetIsAlly, bool reducedFlash, int variation)
{
    internal static readonly string[] Resources = Enumerable.Range(0, 14).Select(i => "freeze/frame-" + i).ToArray();
    internal const double Duration = 96.0 / 60;
    private static readonly int[] Thresholds = [8, 13, 18, 23, 28, 34, 40];
    // table_1C18A initial frame, then table_1C19A's seven growth transitions.
    private static readonly int[][] Growth =
    [
        [0, 6, 1, 7, 2, 8, 3, 9],
        [1, 7, 2, 8, 3, 9, 4, 10],
        [2, 8, 3, 9, 4, 10, 5, 11],
        [3, 9, 4, 10, 5, 11, 5, 11],
    ];
    private readonly record struct Sprite(int Slot, int Frame, Vector2 Position, float Opacity);

    private Vector2 Entry(int slot, int ticks)
    {
        // table_1A6DA, minus the 128-pixel VDP coordinate bias. A local fixed
        // spread substitutes for GenerateRandomNumber, without touching gameplay.
        Vector2 position = new(targetIsAlly ? 0 : 240, (targetIsAlly ? 28 : 12) + slot * 17 % 40);
        position.X += slot * 5 % 8;
        int angle = targetIsAlly ? 226 : 158, direction = targetIsAlly ? 1 : -1;
        for (int tick = 0; tick < ticks; tick++)
        {
            angle += direction;
            float radians = angle * Mathf.Tau / 256;
            position += new Vector2(MathF.Cos(radians), -MathF.Sin(radians)) * 8;
        }
        return position;
    }

    private IEnumerable<Sprite> Sprites(double age)
    {
        if (age < 0 || age >= Duration) yield break;
        float tick = (float)(age * 60);
        // Source chains new particles at age4 and retires at46. Stop after a
        // bounded sixteen-particle stream; the original runs until script stop.
        for (int slot = 0; slot < 16; slot++)
        {
            float life = tick - slot * 3;
            if (life < 0 || life >= 44) continue;
            int sourceAge = (int)life + 2;
            int stage = Thresholds.Count(threshold => sourceAge >= threshold);
            // Reduced Flash retains growth, selecting one brightness per size.
            int frame = Growth[variation - 1][stage];
            if (reducedFlash && frame is >= 6 and < 12) frame -= 6;
            Vector2 position;
            if (sourceAge < 28) position = Entry(slot, (int)life);
            else
            {
                float orbitAge = sourceAge - 28;
                float radius = (0x1000 + (variation - 1) * 0x600 + orbitAge * (0xA0 + (variation - 1) * 0x20)) / 256f;
                float angle = (slot * 47 + orbitAge * (targetIsAlly ? 6 : -6)) * Mathf.Tau / 256;
                var entry = Entry(slot, 25);
                position = new((targetIsAlly ? 184 : 72) + MathF.Cos(angle) * radius,
                    entry.Y + 8 - orbitAge * 8 - MathF.Sin(angle) * radius / 4);
            }
            float opacity = Math.Clamp(Math.Min((life + 1) / 3, (44 - life) / 6), 0, 1);
            yield return new(slot, frame, position, opacity);
        }
        // v4 reserves a separate 1x3 glint, source tiles81/84 (one based).
        if (variation == 4 && tick >= 20 && tick < 86)
        {
            int pulse = (int)(tick - 20) / 8;
            int phase = (int)(tick - 20) % 8;
            if (phase < 6)
                yield return new(16, reducedFlash ? 12 : 12 + phase / 2 % 2,
                    new((targetIsAlly ? 160 : 56) + pulse * 19 % 48,
                        (targetIsAlly ? 88 : 72) + pulse * 29 % 48), reducedFlash ? 0.65f : 1);
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
        variation, targetIsAlly, reducedFlash, duration = Duration,
        sprites = Sprites(age).Select(sprite =>
        {
            var raster = frames(sprite.Frame);
            return new
            {
                slot = sprite.Slot, resource = Resources[sprite.Frame],
                x = MathF.Round(sprite.Position.X), y = MathF.Round(sprite.Position.Y),
                width = raster.Size.X, height = raster.Size.Y, hd = raster.Hd,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight(),
                opacity = sprite.Opacity,
            };
        }).ToArray(),
    };
}

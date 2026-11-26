using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

// FLAME_BREATH v3 on the finite scene clock. No gameplay RNG or result authority.
internal sealed class KiwiBreathEffect(Func<int, BattleSceneRaster> frames, bool casterOnRight, bool targetIsAlly, bool reducedFlash)
{
    internal static readonly string[] Resources = Enumerable.Range(0, 6).Select(i => $"kiwi-flame/frame-{i}").ToArray();
    internal const double Duration = 329.0 / 60;
    private readonly record struct Particle(int Frame, Vector2 Position, bool FlipX);

    private IEnumerable<Particle> Sprites(double age)
    {
        if (age < 0 || age >= Duration) yield break;
        int clock = (int)(age * 60);
        // A state6 successor walks angle64..128..64 by4. Modern four-tick
        // emission stops before the285-update hold expires; every particle drains.
        for (int spawn = 280; spawn >= 0; spawn -= 4)
        {
            int tick = clock - spawn;
            if (tick < 0 || tick >= 48) continue;
            int state = tick + 2, wave = spawn / 4 % 32;
            int angle = 64 + 4 * (wave <= 16 ? wave : 32 - wave);
            double radians = angle * Math.PI / 128;
            // sub_179C has -sin in its high word (Y), cos in its low word (X).
            // The modern trigonometric approximation retains the3px speed and fan.
            var position = new Vector2(156 + (float)(3 * Math.Cos(radians) * tick),
                94 - (float)(3 * Math.Sin(radians) * tick));
            int frame = state < 5 ? 0 : Math.Min(5, state / 5);
            if (!casterOnRight) position.X = 256 - position.X - frames(frame).Size.X;
            yield return new(frame, position, !reducedFlash && (state & 4) != 0);
        }
    }

    internal bool Draw(Node2D node, double age)
    {
        bool drawn = false;
        foreach (var particle in Sprites(age))
        {
            var raster = frames(particle.Frame);
            var position = particle.Position;
            if (particle.FlipX) position.X += raster.Size.X;
            node.DrawSetTransform(position, scale: new(particle.FlipX ? -1 : 1, 1));
            raster.Draw(node, Vector2.Zero); drawn = true;
        }
        if (drawn) node.DrawSetTransform(Vector2.Zero);
        return drawn;
    }

    internal object Observe(double age) => new
    {
        duration = Duration, variation = 3, reducedFlash, casterOnRight, targetIsAlly,
        sprites = Sprites(age).Select(particle =>
        {
            var raster = frames(particle.Frame);
            return new { resource = Resources[particle.Frame], x = particle.Position.X, y = particle.Position.Y,
                width = raster.Size.X, height = raster.Size.Y, hd = raster.Hd,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight(), flipX = particle.FlipX };
        }).ToArray(),
    };
}

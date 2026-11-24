using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

// Rank 2 keeps the retail composition and uses the adapter's presentation clock.
internal sealed class BlazeTwoEffect(Func<int, BattleSceneRaster> frames, bool targetIsAlly, bool reducedFlash)
{
    internal static readonly string[] Resources =
    [
        "blaze-2/flame-0", "blaze-2/flame-1", "blaze-2/meteor-0", "blaze-2/meteor-1",
        "blaze-2/impact-0", "blaze-2/impact-1",
    ];

    private const int MeteorCount = 10;
    private const double SpawnInterval = 0.08;
    private const double Flight = 0.44;
    private const double Impact = 0.19;
    internal const double Duration = (MeteorCount - 1) * SpawnInterval + Flight + Impact;

    private readonly record struct Sprite(int Texture, Vector2 Position, float Opacity, bool Flip = false, int? Meteor = null);

    private IEnumerable<Sprite> Sprites(double age)
    {
        if (age < 0 || age >= Duration) yield break;

        // table_1A594 / table_1A5BC. The leftward branch flips the meteor artwork.
        int startX = targetIsAlly ? 48 : 80;
        int direction = targetIsAlly ? 1 : -1;
        for (int slot = 0; slot < MeteorCount; slot++)
        {
            double elapsed = age - slot * SpawnInterval;
            if (elapsed < 0 || elapsed >= Flight + Impact) continue;
            int x = startX + 6 + slot * 37 % 82;
            int drift = direction * (61 + slot * 11 % 18);
            if (elapsed < Flight)
            {
                float travel = (float)(elapsed / Flight);
                int frame = reducedFlash ? 2 : 2 + (int)(elapsed * 10) % 2;
                yield return new(frame, new(x + drift * travel, -32 + 152 * travel), 1, !targetIsAlly, slot);
            }
            else
            {
                double impactAge = elapsed - Flight;
                int frame = reducedFlash || impactAge < Impact / 2 ? 4 : 5;
                float opacity = (float)Math.Min(1, (Impact - impactAge) * 14);
                yield return new(frame, new(x + drift, frame == 4 ? 120 : 128), opacity, Meteor: slot);
            }
        }

        var flame = targetIsAlly ? new Vector2(172, 112) : new Vector2(68, 96);
        int flameFrame = reducedFlash ? 0 : (int)(age * 10) % 2;
        float flameOpacity = (float)Math.Clamp(Math.Min(age * 15, (Duration - age) * 10), 0, 1);
        yield return new(flameFrame, flame, flameOpacity);
    }

    internal bool Draw(Node2D node, double age)
    {
        bool drawn = false;
        foreach (var sprite in Sprites(age))
        {
            var raster = frames(sprite.Texture);
            var position = sprite.Position.Round();
            if (sprite.Flip) position.X += raster.Size.X;
            node.DrawSetTransform(position, scale: new(sprite.Flip ? -1 : 1, 1));
            raster.Draw(node, Vector2.Zero, new Color(1, 1, 1, sprite.Opacity));
            drawn = true;
        }
        if (drawn) node.DrawSetTransform(Vector2.Zero);
        return drawn;
    }

    internal object Observe(double age) => new
    {
        duration = Duration, targetIsAlly, reducedFlash,
        sprites = Sprites(age).Select(sprite =>
        {
            var raster = frames(sprite.Texture);
            return new
            {
                resource = Resources[sprite.Texture], x = MathF.Round(sprite.Position.X), y = MathF.Round(sprite.Position.Y),
                width = raster.Size.X, height = raster.Size.Y, hd = raster.Hd,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight(),
                opacity = sprite.Opacity, flip = sprite.Flip, meteor = sprite.Meteor,
            };
        }).ToArray(),
    };
}

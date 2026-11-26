using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

// Flame variations keep the retail composition and use the adapter's presentation clock.
internal sealed class BlazeMeteorEffect(Func<int, BattleSceneRaster> frames, bool targetIsAlly, bool reducedFlash, int variation)
{
    internal static readonly string[] SmallResources =
    [
        "blaze-2/flame-0", "blaze-2/flame-1", "blaze-2/meteor-0", "blaze-2/meteor-1",
        "blaze-2/impact-0", "blaze-2/impact-1",
    ];

    internal static readonly string[] LargeResources =
    ["blaze-large/flame-0", "blaze-large/flame-1", "blaze-large/meteor-0", "blaze-large/meteor-1",
     "blaze-large/impact-0", "blaze-large/impact-1"];
    internal static readonly string[] DragonResources = [.. LargeResources,
        "blaze-dragon/head", "blaze-dragon/body", "blaze-dragon/tail-0", "blaze-dragon/tail-1"];
    internal int Variation => variation;
    private string[] Resources => variation == 2 ? SmallResources : variation == 3 ? LargeResources : DragonResources;
    private int MeteorCount => variation == 2 ? 10 : variation == 3 ? 14 : 4;
    private const double SpawnInterval = 0.08;
    private const double Flight = 0.44;
    private const double Impact = 0.19;
    internal double Duration => variation == 4 ? 1.8 : (MeteorCount - 1) * SpawnInterval + Flight + Impact;

    private readonly (Vector2 Position, int Heading)[] _dragon = variation == 4 ? DragonPath(targetIsAlly) : [];

    private static (Vector2, int)[] DragonPath(bool targetIsAlly)
    {
        // Setup and update/blaze.asm: speed ramp, 8/256 turns, six-tick straight
        // interval and source wall steering table. Local visual RNG cannot alter play.
        var points = new (Vector2, int)[109];
        var position = targetIsAlly ? new Vector2(168, 96) : new Vector2(64, 80);
        int heading = 64, destination = 64, turn = 0, straight = 0, speed = 128;
        bool slowing = false;
        int[] wallHeading = [0, 224, 160, 208, 96, 0, 144, 0, 32, 16, 0, 0, 80, 255];
        var random = new Random(9);
        for (int tick = 0; tick < points.Length; tick++)
        {
            points[tick] = (position, heading);
            speed += slowing ? -16 : 16;
            if (speed >= 768) slowing = true;
            if (speed <= 256 && slowing) slowing = false;
            if (turn != 0)
            {
                heading = (heading + turn * 8) & 255;
                if (heading == destination) { turn = 0; straight = 6; }
            }
            else if (straight > 0) straight--;
            else
            {
                int left = targetIsAlly ? 128 : 32;
                int wall = (position.X < left ? 1 : 0) | (position.Y < 32 ? 2 : 0) |
                    (position.X > left + 128 ? 4 : 0) | (position.Y > 112 ? 8 : 0);
                if (wall != 0)
                {
                    bool corner = (wall & (wall - 1)) != 0;
                    destination = (wallHeading[wall] + random.Next(corner ? 33 : 65)) & 240;
                    int low = Math.Min(heading, destination), high = Math.Max(heading, destination);
                    turn = high - low >= 128 ? 1 : -1;
                }
            }
            float angle = heading * Mathf.Tau / 256;
            position += new Vector2(MathF.Cos(angle), -MathF.Sin(angle)) * (speed / 256f);
        }
        return points;
    }

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
                yield return new(frame, new(x + drift, frame == 4 || variation != 2 ? 120 : 128), opacity, Meteor: slot);
            }
        }

        if (variation == 4)
        {
            // table_1BE82 retains five three-tick body gaps and two two-tick tails.
            for (int part = 7; part >= 0; part--)
            {
                int delay = part <= 5 ? part * 3 : 15 + (part - 5) * 2;
                int tick = Math.Clamp((int)(age * 60) - delay, 0, _dragon.Length - 1);
                var (point, heading) = _dragon[tick];
                int texture = part == 0 ? 6 : part <= 5 ? 7 : part == 6 ? 8 : 9;
                float offset = part == 0 ? 0 : part <= 5 ? 4 : 8;
                float opacity = (float)Math.Clamp(Math.Min(age * 15, (Duration - age) * 10), 0, 1);
                yield return new(texture, point + new Vector2(offset, offset), opacity,
                    part == 0 && heading is >= 64 and < 192);
            }
        }
        var flame = variation == 2
            ? targetIsAlly ? new Vector2(172, 112) : new Vector2(68, 96)
            : targetIsAlly ? new Vector2(168, 96) : new Vector2(64, 80);
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
        duration = Duration, variation, meteorCount = MeteorCount, targetIsAlly, reducedFlash,
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

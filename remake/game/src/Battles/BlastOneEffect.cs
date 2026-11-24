using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

// Retail rank-1 composition with deterministic scheduling on the adapter clock.
internal sealed class BlastOneEffect(Func<int, BattleSceneRaster> frames, bool targetIsAlly, bool reducedFlash)
{
    internal static readonly string[] Resources = Enumerable.Range(0, 15).Select(i => "blast-1/frame-" + i).ToArray();
    internal const double Duration = 1.4;
    private readonly record struct Sprite(int Frame, Vector2 Position, bool FlipX, bool FlipY, float Opacity);
    private static readonly Vector2[] ParticleOffsets = [new(-48,-48), new(48,-48), new(-48,48), new(48,48), new(-52,0), new(52,0)];
    private static readonly (int X, int Y, int Kind, bool FlipX, bool FlipY, int EndX, int EndY)[] Slashes =
    [
        (24,24,1,false,true,8,0), (0,24,0,false,false,0,8),
        (-24,24,1,true,true,0,0), (24,0,2,false,false,8,8),
        (-24,0,2,true,false,8,8), (24,-24,1,false,false,8,0),
        (0,-24,0,false,true,0,0), (-24,-24,1,true,false,0,0),
    ];

    private IEnumerable<Sprite> Sprites(double age)
    {
        if (age < 0 || age >= Duration) yield break;
        // Setup's VDP origins (208,224)/(312,240), minus the 128-pixel VDP bias.
        Vector2 origin = targetIsAlly ? new(184,112) : new(80,96);
        for (int slot = 0; slot < 8; slot++)
        {
            double elapsed = age - slot * 0.08;
            if (elapsed < 0 || elapsed >= 0.65) continue;
            int direction = slot % ParticleOffsets.Length;
            Vector2 offset = ParticleOffsets[direction];
            int phase = reducedFlash ? 1 : Math.Min(2, (int)(elapsed / 0.12));
            int frame = (direction >= 4 ? 3 : 0) + phase;
            float travel = (float)Math.Clamp((elapsed - 0.2) / 0.45, 0, 1);
            float fade = (float)Math.Min(1, (0.65 - elapsed) * 10);
            yield return new(frame, origin + offset * (1 - travel), offset.X > 0, offset.Y > 0, fade);
        }
        // One short slash at a time. The table's eight direction/flip operands remain local
        // to the target. Mirroring changes the origin, not the individual table modifiers.
        int slash = Math.Min(7, (int)(age / 0.175));
        double slashAge = age - slash * 0.175;
        int stage = reducedFlash ? 1 : Math.Min(2, (int)(slashAge / (0.175 / 3)));
        var entry = Slashes[slash];
        Vector2 position = origin + new Vector2(entry.X - 16, entry.Y - 8);
        if (stage == 2) position += new Vector2(entry.EndX, entry.EndY);
        yield return new(6 + entry.Kind * 3 + stage, position, entry.FlipX, entry.FlipY, 1);
    }

    internal bool Draw(Node2D node, double age)
    {
        bool drawn = false;
        foreach (var sprite in Sprites(age))
        {
            var raster = frames(sprite.Frame);
            Vector2 position = sprite.Position.Round();
            if (sprite.FlipX) position.X += raster.Size.X;
            if (sprite.FlipY) position.Y += raster.Size.Y;
            node.DrawSetTransform(position, scale: new(sprite.FlipX ? -1 : 1, sprite.FlipY ? -1 : 1));
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
            var raster = frames(sprite.Frame);
            return new
            {
                resource = Resources[sprite.Frame], x = MathF.Round(sprite.Position.X), y = MathF.Round(sprite.Position.Y),
                width = raster.Size.X, height = raster.Size.Y, hd = raster.Hd,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight(),
                flipX = sprite.FlipX, flipY = sprite.FlipY, opacity = sprite.Opacity,
            };
        }).ToArray(),
    };
}

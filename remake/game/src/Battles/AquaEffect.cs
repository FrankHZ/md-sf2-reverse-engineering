using Godot;

namespace Sf2.Remake.GodotAdapter.Battles;

// Source bubble geometry on the shared finite presentation clock; no gameplay RNG.
internal sealed class AquaEffect(Func<int, BattleSceneRaster> frames, bool targetIsAlly, bool reducedFlash, int rank)
{
    internal static readonly string[] RankOneResources = Enumerable.Range(0, 7).Select(i => $"aqua-1/frame-{i}").ToArray();
    internal static readonly string[] RankTwoResources = Enumerable.Range(0, 7).Select(i => $"aqua-2/frame-{i}").ToArray();
    internal static string[] ResourcesFor(int rank) => rank == 1 ? RankOneResources : RankTwoResources;
    internal const double Duration = 307.0 / 60;
    private readonly record struct Bubble(int Frame, Vector2 Position, bool FlipX);

    private IEnumerable<Bubble> Sprites(double age)
    {
        if (age < 0 || age >= Duration) yield break;
        int clock = (int)(age * 60);
        // Modern deterministic samples within source random ranges. Emit during the
        // source228-tick hold, then let every last bubble complete its80-tick lifetime.
        for (int spawn = 225; spawn >= 0; spawn -= 9)
        {
            int tick = clock - spawn;
            if (tick < 0 || tick >= 80) continue;
            int slot = spawn / 9;
            int vx = 320 + slot * 137 % 512, vy = -512 + slot * 73 % 256;
            int x = 84, y = 120, rx = 0, ry = 0;
            for (int step = 2; step <= tick + 1; step++)
            {
                if (step < 40)
                {
                    rx += vx; x += rx / 256; rx %= 256;
                    vy += 28; ry += vy; y += ry / 256; ry %= 256;
                }
                else if (step == 40) { vy = -64 + slot * 11 % 32; rx = ry = 0; }
                else if (step < 80)
                {
                    int counter = step - 40, sway = Math.Min(2, (counter >> 1) & 3);
                    x += (counter & 8) == 0 ? sway : -sway;
                    vy -= 32; ry += vy; y += ry / 256; ry %= 256;
                }
            }
            int frame = Math.Min(6, (tick + 2) / 10);
            int size = frame < 4 ? 8 : (frame - 2) * 8;
            if (!targetIsAlly) x = 256 - x - size;
            yield return new(frame, new(x, y), !reducedFlash && ((tick + 2) & 4) != 0);
        }
    }

    internal bool Draw(Node2D node, double age)
    {
        bool drawn = false;
        foreach (var bubble in Sprites(age))
        {
            var raster = frames(bubble.Frame);
            var position = bubble.Position;
            if (bubble.FlipX) position.X += raster.Size.X;
            node.DrawSetTransform(position, scale: new(bubble.FlipX ? -1 : 1, 1));
            raster.Draw(node, Vector2.Zero); drawn = true;
        }
        if (drawn) node.DrawSetTransform(Vector2.Zero);
        return drawn;
    }

    internal object Observe(double age) => new
    {
        duration = Duration, rank, targetIsAlly, reducedFlash,
        sprites = Sprites(age).Select(bubble =>
        {
            var raster = frames(bubble.Frame);
            return new { resource = ResourcesFor(rank)[bubble.Frame], x = bubble.Position.X, y = bubble.Position.Y,
                width = raster.Size.X, height = raster.Size.Y, hd = raster.Hd,
                textureWidth = raster.Texture.GetWidth(), textureHeight = raster.Texture.GetHeight(), flipX = bubble.FlipX };
        }).ToArray(),
    };
}

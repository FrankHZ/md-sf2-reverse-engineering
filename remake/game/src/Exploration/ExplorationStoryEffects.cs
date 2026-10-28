using Godot;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Finite csc27/csc18/csc22 projections on the existing modern presentation clock.
internal static class ExplorationStoryEffects
{
    internal static int HeadShift(double age) => ((int)(age * 60) % 6) switch
        { 0 or 1 => -1, 3 or 4 => 1, _ => 0 };

    internal static void ShiftHead(Image image, int shift)
    {
        for (int y = 0; y < 12; y++)
            if (shift < 0)
                for (int x = 8; x <= 15; x++) image.SetPixel(x - 1, y, image.GetPixel(x, y));
            else if (shift > 0)
                for (int x = 15; x >= 8; x--) image.SetPixel(x + 1, y, image.GetPixel(x, y));
    }

    internal static IReadOnlyList<Rect2> Draw(Control owner, ImageTexture texture, Rect2 bounds,
        IReadOnlyList<Rect2> ink, bool mirror, float scale, string effect, double age, bool reducedFlash)
    {
        if (effect == "flash-white")
        {
            bool white = !reducedFlash && (int)(age * 60) % 4 < 2;
            if (white)
                foreach (var run in ink)
                    owner.DrawRect(new(bounds.Position + new Vector2(mirror ? 24 - run.End.X : run.Position.X, run.Position.Y) * scale,
                        run.Size * scale), Colors.White);
            else
            {
                if (mirror) bounds.Size = new(-bounds.Size.X, bounds.Size.Y);
                owner.DrawTextureRect(texture, bounds, false);
            }
            return ink;
        }
        if (effect != "slide-in") throw new InvalidOperationException("story-effect-binding");
        int offset = 22 - Math.Clamp((int)(age * 30), 0, 22);
        var shown = ink.Select(run => run.Intersection(new(0, 0, 24, 24 - offset)))
            .Where(run => run.HasArea()).Select(run => new Rect2(run.Position + new Vector2(0, offset), run.Size)).ToArray();
        var destination = new Rect2(bounds.Position + new Vector2(0, offset) * scale, new Vector2(mirror ? -24 : 24, 24 - offset) * scale);
        owner.DrawTextureRectRegion(texture, destination, new(0, 0, 24, 24 - offset));
        return shown;
    }
}

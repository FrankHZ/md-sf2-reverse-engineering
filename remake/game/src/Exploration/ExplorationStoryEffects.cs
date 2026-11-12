using Godot;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Finite csc27/csc18/csc22 projections on the existing modern presentation clock.
internal static class ExplorationStoryEffects
{
    // Modern raster projection of the source orientation and centered size operands.
    internal static void Transform(Image image, int orientation, int size)
    {
        if (size is < 0 or > 24) throw new InvalidOperationException("entity-sprite-size");
        if (size < 24)
        {
            int density = image.GetWidth() / 24;
            using var small = image.GetRegion(new(0, 0, image.GetWidth(), image.GetHeight()));
            int extent = Math.Max(1, size);
            small.Resize(extent * density, extent * density, Image.Interpolation.Nearest);
            image.Fill(Colors.Transparent);
            image.BlitRect(small, new(0, 0, extent * density, extent * density),
                new((24 - extent) / 2 * density, (24 - extent) / 2 * density));
        }
        if (orientation == 1) image.Rotate90(ClockDirection.Counterclockwise);
        else if (orientation == 3) image.Rotate90(ClockDirection.Clockwise);
        else if (orientation == 2) image.FlipY();
    }

    internal static int HeadShift(double age) => ((int)(age * 60) % 6) switch
        { 0 or 1 => -1, 3 or 4 => 1, _ => 0 };

    internal static void ShiftHead(Image image, int shift)
    {
        if (shift == 0) return;
        int density = image.GetWidth() / 24;
        using var band = image.GetRegion(new(8 * density, 0, 8 * density, 12 * density));
        image.BlitRect(band, new(0, 0, band.GetWidth(), band.GetHeight()), new((8 + shift) * density, 0));
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
        if (effect is not ("slide-in" or "slide-out")) throw new InvalidOperationException("story-effect-binding");
        int progress = Math.Clamp((int)(age * 30), 0, 22);
        int offset = effect == "slide-in" ? 22 - progress : progress;
        if (effect == "slide-out" && age >= 46.0 / 60) return [];
        var shown = ink.Select(run => run.Intersection(new(0, 0, 24, 24 - offset)))
            .Where(run => run.HasArea()).Select(run => new Rect2(run.Position + new Vector2(0, offset), run.Size)).ToArray();
        var destination = new Rect2(bounds.Position + new Vector2(0, offset) * scale, new Vector2(mirror ? -24 : 24, 24 - offset) * scale);
        owner.DrawTextureRectRegion(texture, destination, ExplorationRasterImages.TextureRegion(new(0, 0, 24, 24 - offset), texture));
        return shown;
    }
}

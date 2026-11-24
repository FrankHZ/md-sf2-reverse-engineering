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

    // sub_45A8C clears color 2 in the outer bottom tiles and complete $2222
    // words in the middle bottom tile. Opaque black is source palette index 2.
    internal static void RemoveShadow(Image image, Image? original = null)
    {
        var source = original ?? image;
        int density = image.GetWidth() / source.GetWidth();
        for (int y = 16; y < 24; y++)
            for (int x = 0; x < 24; x += 4)
            {
                if (x is >= 8 and < 16 && !Enumerable.Range(x, 4).All(i => source.GetPixel(i, y) == Colors.Black))
                    continue;
                for (int i = x; i < x + 4; i++)
                    if (source.GetPixel(i, y) == Colors.Black)
                        image.FillRect(new(i * density, y * density, density, density), Colors.Transparent);
            }
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
        if (effect is "wipe-in" or "wipe-out")
        {
            // csc22 table: d0=1..23 (out), 22..0 (in), d1=0.
            // ApplySpriteCropEffect copies row zero to d0, clears rows
            // 0..d0-1, and leaves rows d0+1..23 unchanged.
            int step = Math.Clamp((int)(age * 30), 0, 22);
            int row = effect == "wipe-out" ? step + 1 : 22 - step;
            List<Rect2> croppedInk = [];
            var bands = new[] { (new Rect2(0, 0, 24, 1), row), (new Rect2(0, row + 1, 24, 23 - row), 0) };
            foreach (var (source, shift) in bands)
            {
                if (!source.HasArea()) continue;
                croppedInk.AddRange(ink.Select(run => run.Intersection(source)).Where(run => run.HasArea())
                    .Select(run => new Rect2(run.Position + new Vector2(0, shift), run.Size)));
                var target = new Rect2(bounds.Position + new Vector2(0, source.Position.Y + shift) * scale,
                    new Vector2(mirror ? -24 : 24, source.Size.Y) * scale);
                owner.DrawTextureRectRegion(texture, target, ExplorationRasterImages.TextureRegion(source, texture));
            }
            return croppedInk;
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

using Godot;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal static class ExplorationRasterImages
{
    internal static Image Open(ExplorationRaster raster)
    {
        if (raster.Format == "rgba8") return Image.CreateFromData(raster.Width, raster.Height, false, Image.Format.Rgba8, raster.CopyBytes());
        var image = new Image();
        if (image.LoadPngFromBuffer(raster.CopyBytes()) != Error.Ok || image.GetWidth() != raster.Width || image.GetHeight() != raster.Height)
        { image.Dispose(); throw new InvalidOperationException("raster-decode"); }
        return image;
    }

    internal static ImageTexture Portrait(ExplorationPortraitVisual visual, IReadOnlyList<int>? tiles = null, bool mirror = false)
    {
        using var raster = Open(visual.Raster);
        using var composed = raster.GetRegion(new(0, 0, 64, 64));
        if (tiles is not null)
            for (int tile = 0; tile < tiles.Count; tile++)
                if (tiles[tile] != tile)
                    composed.BlitRect(raster, new(tiles[tile] % 8 * 8, tiles[tile] / 8 * 8, 8, 8), new(tile % 8 * 8, tile / 8 * 8));
        // Six visible columns/eight rows; atlas columns6/7 contain alternates.
        using var crop = composed.GetRegion(new(0, 0, 48, 64));
        if (mirror) crop.FlipX();
        return ImageTexture.CreateFromImage(crop);
    }
}

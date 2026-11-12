using Godot;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal static class ExplorationRasterImages
{
    internal static Rect2 TextureRegion(Rect2 logicalRegion, Texture2D texture)
    {
        float density = texture.GetWidth() / 24f;
        return new(logicalRegion.Position * density, logicalRegion.Size * density);
    }

    internal static Image Open(ExplorationRaster raster)
    {
        if (raster.Format == "rgba8") return Image.CreateFromData(raster.Width, raster.Height, false, Image.Format.Rgba8, raster.CopyBytes());
        var image = new Image();
        if (image.LoadPngFromBuffer(raster.CopyBytes()) != Error.Ok || image.GetWidth() != raster.Width || image.GetHeight() != raster.Height)
        { image.Dispose(); throw new InvalidOperationException("raster-decode"); }
        return image;
    }

    internal static ImageTexture Portrait(ExplorationPortraitVisual visual, IReadOnlyList<int>? tiles = null, bool mirror = false, bool hd = false)
    {
        using var raster = Open(hd && visual.HdRaster is not null ? visual.HdRaster : visual.Raster);
        int tileSize = raster.GetWidth() / 8;
        using var composed = raster.GetRegion(new(0, 0, raster.GetWidth(), raster.GetHeight()));
        if (tiles is not null)
            for (int tile = 0; tile < tiles.Count; tile++)
                if (tiles[tile] != tile)
                    composed.BlitRect(raster, new(tiles[tile] % 8 * tileSize, tiles[tile] / 8 * tileSize, tileSize, tileSize),
                        new(tile % 8 * tileSize, tile / 8 * tileSize));
        // Six visible columns/eight rows; atlas columns6/7 contain alternates.
        using var crop = composed.GetRegion(new(0, 0, 6 * tileSize, 8 * tileSize));
        if (mirror) crop.FlipX();
        return ImageTexture.CreateFromImage(crop);
    }
}

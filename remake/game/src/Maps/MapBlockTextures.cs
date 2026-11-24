using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Maps;

// Disposable Godot resources derived from already admitted atlas/block definitions.
internal sealed class MapBlockTextures : IDisposable
{
    private readonly Dictionary<MapId, Image> _atlases = [];
    private readonly Dictionary<(MapId Map, int Block), ImageTexture> _blocks = [];
    private readonly Dictionary<ExplorationRaster, ImageTexture> _hd = [];
    private readonly Dictionary<(MapId Map, int Block, ExplorationRaster? Variant), Bitmap> _blockInk = [];

    private ExplorationMapVisual? _checkedBank;
    internal IEnumerable<ImageTexture> Textures => _blocks.Values.Concat(_hd.Values);

    internal void PrepareHd(ExplorationMapVisual visual)
    {
        if (_checkedBank == visual || visual.HdBlocks is null) return;
        foreach (var raster in visual.HdBlocks.Concat(visual.HdBlockContexts?.Values ?? []).Distinct())
        {
            using var image = Exploration.ExplorationRasterImages.Open(raster);
        }
        _checkedBank = visual;
    }

    internal static ExplorationRaster? Variant(ExplorationMapVisual visual, bool hd, WorkingMapLayout layout,
        int x, int y, Rect2I bounds, out bool contextual)
    {
        contextual = false;
        if (!hd || visual.HdBlocks is null) return null;
        int block = layout[x, y] & 1023;
        // A detached secondary plane may scroll off the normal packed-room bounds.
        // Its source remains the same 64x64 working layout.
        if (!bounds.HasPoint(new(x, y))) bounds = new(0, 0, 64, 64);
        int At(int dx, int dy) => bounds.HasPoint(new(x + dx, y + dy)) &&
            x + dx is >= 0 and < 64 && y + dy is >= 0 and < 64 ? layout[x + dx, y + dy] & 1023 : -1;
        if (visual.HdBlockContexts is { } variants && variants.TryGetValue(new(At(-1,-1), At(0,-1), At(1,-1),
            At(-1,0), block, At(1,0), At(-1,1), At(0,1), At(1,1)), out var variant))
        {
            contextual = true;
            return variant;
        }
        return visual.HdBlocks[block];
    }

    internal int Count => _blocks.Count + _hd.Count;

    internal ImageTexture Block(ExplorationMapVisual visual, int block, ExplorationRaster? variant = null)
    {
        var key = (visual.Map, block);
        if (block < 0 || block >= visual.Blocks.Count) throw new InvalidOperationException("map-block-binding");
        if (variant is not null)
        {
            if (_hd.TryGetValue(variant, out var prepared)) return prepared;
            using var pixels = Exploration.ExplorationRasterImages.Open(variant);
            _hd.Add(variant, prepared = ImageTexture.CreateFromImage(pixels));
            return prepared;
        }
        if (_blocks.TryGetValue(key, out var texture)) return texture;
        using var image = OpenOriginalBlock(visual, block);
        _blocks.Add(key, texture = ImageTexture.CreateFromImage(image));
        return texture;
    }

    internal Bitmap BlockInk(ExplorationMapVisual visual, int block, ExplorationRaster? variant)
    {
        var key = (visual.Map, block, variant);
        if (_blockInk.TryGetValue(key, out var ink)) return ink;
        // Read the same source pixels as the texture upload, including in headless mode.
        using var image = variant is null ? OpenOriginalBlock(visual, block) : Exploration.ExplorationRasterImages.Open(variant);
        ink = new Bitmap();
        ink.CreateFromImageAlpha(image, 0);
        _blockInk.Add(key, ink);
        return ink;
    }

    private Image OpenOriginalBlock(ExplorationMapVisual visual, int block)
    {
        if (!_atlases.TryGetValue(visual.Map, out var atlas))
        {
            atlas = visual.Atlas.Format == "rgba8"
                ? Image.CreateFromData(visual.Atlas.Width, visual.Atlas.Height, false, Image.Format.Rgba8, visual.Atlas.CopyBytes())
                : new Image();
            if (visual.Atlas.Format != "rgba8" && (atlas.LoadPngFromBuffer(visual.Atlas.CopyBytes()) != Error.Ok ||
                atlas.GetWidth() != visual.Atlas.Width || atlas.GetHeight() != visual.Atlas.Height))
            { atlas.Dispose(); throw new InvalidOperationException("raster-decode"); }
            _atlases.Add(visual.Map, atlas);
        }
        var image = Image.CreateEmpty(24, 24, false, Image.Format.Rgba8);
        for (int tile = 0; tile < 9; tile++)
        {
            int word = visual.Blocks[block][tile], index = (word & 0x3FF) - 0x100;
            if (index < 0) continue;
            if (index >= 640) throw new InvalidOperationException("map-tile-binding");
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    int sx = (word & 0x800) != 0 ? 7 - x : x, sy = (word & 0x1000) != 0 ? 7 - y : y;
                    image.SetPixel(tile % 3 * 8 + x, tile / 3 * 8 + y,
                        atlas.GetPixel((index % 16 * 8 + sx) * visual.Scale, (index / 16 * 8 + sy) * visual.Scale));
                }
        }
        return image;
    }

    public void Dispose()
    {
        _checkedBank = null;
        foreach (var texture in _blocks.Values) texture.Dispose();
        foreach (var atlas in _atlases.Values) atlas.Dispose();
        foreach (var texture in _hd.Values) texture.Dispose();
        foreach (var ink in _blockInk.Values) ink.Dispose();
        _hd.Clear(); _blocks.Clear(); _atlases.Clear(); _blockInk.Clear();
    }
}

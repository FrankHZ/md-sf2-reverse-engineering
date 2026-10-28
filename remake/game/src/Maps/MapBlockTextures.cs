using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Maps;

// Disposable Godot resources derived from already admitted atlas/block definitions.
internal sealed class MapBlockTextures : IDisposable
{
    private readonly Dictionary<MapId, Image> _atlases = [];
    private readonly Dictionary<(MapId Map, int Block), ImageTexture> _blocks = [];

    internal int Count => _blocks.Count;

    internal ImageTexture Block(ExplorationMapVisual visual, int block)
    {
        if (_blocks.TryGetValue((visual.Map, block), out var texture)) return texture;
        if (block < 0 || block >= visual.Blocks.Count) throw new InvalidOperationException("map-block-binding");
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
        using var image = Image.CreateEmpty(24, 24, false, Image.Format.Rgba8);
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
        _blocks.Add((visual.Map, block), texture = ImageTexture.CreateFromImage(image));
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in _blocks.Values) texture.Dispose();
        foreach (var atlas in _atlases.Values) atlas.Dispose();
        _blocks.Clear(); _atlases.Clear();
    }
}

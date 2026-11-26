using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Maps;

// Disposable Godot resources derived from already admitted atlas/block definitions.
internal sealed class MapBlockTextures : IDisposable
{
    private readonly Dictionary<MapId, Image> _atlases = [];
    private readonly Dictionary<(MapId Map, int Block), ImageTexture> _blocks = [];
    private readonly Dictionary<ExplorationRaster, ImageTexture> _variants = [];
    private readonly Dictionary<(MapId Map, int Block, ExplorationRaster? Variant), Bitmap> _blockInk = [];

    private ScenePaletteDefinition? _scenePalette;
    private IReadOnlyList<ushort>? _sceneColors;
    private readonly Dictionary<int, object> _paletteSamples = [];
    private readonly List<object> _hdSamples = [];
    private ScenePaletteHdBank? _hdPaletteBank;
    internal ScenePaletteHdBank? HdPaletteBank => _hdPaletteBank;
    internal bool SupportsHdPalette => HdPaletteOffset is not null || _hdPaletteBank is not null;
    internal object PaletteProjection => new { active = _scenePalette is not null,
        colors = _sceneColors, hdOffset = HdPaletteOffset, hdBank = _hdPaletteBank is not null,
        blocks = _blocks.Count, variants = _variants.Count, slots = _paletteSamples.Values.ToArray(),
        hdSamples = _hdSamples.ToArray() };
    // Uniform channel fades use the ordinary bank. Authored slot replacement uses
    // an exact prepared palette bank, with indexed Original fallback if absent.
    internal int? HdPaletteOffset { get; private set; } = 0;
    internal bool PaletteChanged(ScenePaletteDefinition? resource, IReadOnlyList<ushort>? colors) =>
        !ReferenceEquals(_scenePalette, resource) || !ReferenceEquals(_sceneColors, colors);
    internal void SetPalette(ScenePaletteDefinition? resource, IReadOnlyList<ushort>? colors)
    {
        // Color changes invalidate uploaded pixels, not immutable PNG admission.
        // A map change still calls Dispose directly and releases this validation.
        var checkedBank = _checkedBank;
        Dispose();
        _checkedBank = checkedBank;
        _scenePalette = resource; _sceneColors = colors;
        HdPaletteOffset = resource is null || colors is null ? 0 : null;
        if (resource is not null && colors is not null)
        {
            _hdPaletteBank = resource.HdBanks.FirstOrDefault(bank => bank.Colors.SequenceEqual(colors));
            for (int offset = -7; offset <= 7; offset++)
                if (resource.Base.Select(color => Sf2.Remake.Application.Runtime.Exploration.ScenePaletteState.Offset(color, offset)).SequenceEqual(colors))
                { HdPaletteOffset = offset; break; }
        }
    }

    private ExplorationMapVisual? _checkedBank;
    private MapTileAnimation? _checkedAnimation;

    internal void PrepareAnimation(MapTileAnimation? animation)
    {
        if (animation is null || animation == _checkedAnimation) return;
        // A later transfer must not discover a corrupt PNG in a retained draw callback.
        foreach (var phase in animation.Phases)
        {
            foreach (var pair in phase.Blocks.Values)
            {
                using var original = Exploration.ExplorationRasterImages.Open(pair.Original);
                using var hd = Exploration.ExplorationRasterImages.Open(pair.Hd);
            }
            foreach (var edge in phase.Contexts.Values.Distinct())
            {
                using var image = Exploration.ExplorationRasterImages.Open(edge);
            }
        }
        _checkedAnimation = animation;
    }

    internal IEnumerable<ImageTexture> Textures => _blocks.Values.Concat(_variants.Values);

    internal void PrepareHd(ExplorationMapVisual visual)
    {
        if (_checkedBank == visual || visual.HdBlocks is null) return;
        foreach (var raster in visual.HdBlocks.Concat(visual.HdBlockContexts?.Values ?? []).Distinct())
        {
            using var image = Exploration.ExplorationRasterImages.Open(raster);
        }
        foreach (var bank in visual.ScenePalette?.HdBanks ?? [])
            foreach (var raster in bank.Blocks.Concat(bank.Contexts?.Values ?? []).Distinct())
            { using var image = Exploration.ExplorationRasterImages.Open(raster); }
        _checkedBank = visual;
    }

    internal static BlockNeighborhood Neighborhood(WorkingMapLayout layout, int x, int y, Rect2I bounds)
    {
        // A detached secondary plane may scroll off the normal packed-room bounds.
        // Its source remains the same 64x64 working layout.
        if (!bounds.HasPoint(new(x, y))) bounds = new(0, 0, 64, 64);
        int At(int dx, int dy) => bounds.HasPoint(new(x + dx, y + dy)) &&
            x + dx is >= 0 and < 64 && y + dy is >= 0 and < 64 ? layout[x + dx, y + dy] & 1023 : -1;
        return new(At(-1,-1), At(0,-1), At(1,-1), At(-1,0), At(0,0), At(1,0), At(-1,1), At(0,1), At(1,1));
    }

    internal static ExplorationRaster? Variant(ExplorationMapVisual visual, bool hd, WorkingMapLayout layout,
        int x, int y, Rect2I bounds, int phase, out bool contextual, ScenePaletteHdBank? paletteBank = null)
    {
        contextual = false;
        var transfer = phase < 0 ? null : visual.TileAnimation!.Phases[phase];
        int block = layout[x, y] & 1023;
        var center = transfer?.Blocks.GetValueOrDefault(block);
        if (!hd || visual.HdBlocks is null) return center?.Original;
        var key = Neighborhood(layout, x, y, bounds);
        if (paletteBank is { } bank)
        {
            if (bank.Contexts?.TryGetValue(key, out var tinted) == true)
            { contextual = true; return tinted; }
            return bank.Blocks[block];
        }
        if (transfer?.Contexts.TryGetValue(key, out var edge) == true)
        {
            contextual = true;
            return edge;
        }
        if (center is not null) return center.Hd;
        if (visual.HdBlockContexts is { } variants && variants.TryGetValue(key, out var variant))
        {
            contextual = true;
            return variant;
        }
        return visual.HdBlocks[key.Center];
    }

    internal int Count => _blocks.Count + _variants.Count;

    internal ImageTexture Block(ExplorationMapVisual visual, int block, ExplorationRaster? variant = null)
    {
        var key = (visual.Map, block);
        if (block < 0 || block >= visual.Blocks.Count) throw new InvalidOperationException("map-block-binding");
        if (variant is not null)
        {
            if (_variants.TryGetValue(variant, out var prepared)) return prepared;
            using var pixels = Exploration.ExplorationRasterImages.Open(variant);
            Exploration.ExplorationPresentation.OffsetSprite(pixels, _hdPaletteBank is null ? HdPaletteOffset ?? 0 : 0);
            if (_hdPaletteBank is not null && _hdSamples.Count < 8)
            {
                using var normalized = pixels.GetRegion(new(0, 0, pixels.GetWidth(), pixels.GetHeight()));
                normalized.Convert(Image.Format.Rgba8);
                _hdSamples.Add(new { block, width = pixels.GetWidth(), height = pixels.GetHeight(),
                    sourceFormat = pixels.GetFormat().ToString(),
                    rgbaSha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(normalized.GetData())) });
            }
            _variants.Add(variant, prepared = ImageTexture.CreateFromImage(pixels));
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
        if (_scenePalette is not null && !ReferenceEquals(_scenePalette, visual.ScenePalette))
            throw new InvalidOperationException("scene-palette-map-binding");
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
                    int ax = index % 16 * 8 + sx, ay = index / 16 * 8 + sy;
                    Color pixel;
                    if (_scenePalette is not null)
                    {
                        int slot = _scenePalette.Slot(ax, ay), color = _sceneColors![slot];
                        pixel = slot == 0 ? Colors.Transparent : new((color & 14) / 14f,
                            ((color >> 4) & 14) / 14f, ((color >> 8) & 14) / 14f);
                    }
                    else pixel = atlas.GetPixel(ax * visual.Scale, ay * visual.Scale);
                    image.SetPixel(tile % 3 * 8 + x, tile / 3 * 8 + y, pixel);
                    if (_scenePalette is not null)
                    {
                        int slot = _scenePalette.Slot(ax, ay);
                        if (!_paletteSamples.ContainsKey(slot))
                        {
                            Color actual = image.GetPixel(tile % 3 * 8 + x, tile / 3 * 8 + y);
                            _paletteSamples.Add(slot, new { slot, word = _sceneColors![slot],
                                r = actual.R, g = actual.G, b = actual.B, a = actual.A });
                        }
                    }
                }
        }
        return image;
    }

    public void Dispose()
    {
        HdPaletteOffset = 0;
        _hdPaletteBank = null;
        _checkedBank = null; _checkedAnimation = null; _scenePalette = null; _sceneColors = null;
        _paletteSamples.Clear();
        _hdSamples.Clear();
        foreach (var texture in _blocks.Values) texture.Dispose();
        foreach (var atlas in _atlases.Values) atlas.Dispose();
        foreach (var texture in _variants.Values) texture.Dispose();
        foreach (var ink in _blockInk.Values) ink.Dispose();
        _variants.Clear(); _blocks.Clear(); _atlases.Clear(); _blockInk.Clear();
    }
}

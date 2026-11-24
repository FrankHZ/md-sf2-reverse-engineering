using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Exploration;

namespace Sf2.Remake.GodotAdapter.Battles;

internal readonly record struct BattleSceneRaster(Texture2D Texture, Vector2 Size, bool Hd)
{
    internal static bool HasHdGroup(BattleSceneDefinition scene, IEnumerable<string> keys) =>
        scene.HdRasters is { } variants && keys.All(variants.ContainsKey);

    internal void Bind(Sprite2D sprite)
    {
        sprite.Texture = Texture;
        sprite.Scale = Size / Texture.GetSize();
    }

    internal void Draw(Node2D node, Vector2 position, Color? color = null) =>
        node.DrawTextureRect(Texture, new(position, Size), false, color ?? Colors.White);
}

internal sealed partial class BattleSceneView
{
    private readonly record struct RasterBinding(string Key, ExplorationRaster Original,
        ExplorationRaster? Hd, Vector2 Size);

    private readonly Dictionary<ExplorationRaster, ImageTexture> _textures = [];
    private readonly Dictionary<Sprite2D, RasterBinding> _spriteBindings = [];
    private readonly List<RasterBinding[]> _effectBindings = [];

    internal void PrepareGraphics(string mode, BattleSceneDefinition? content)
    {
        bool hd = mode == "hd";
        if (hd && content?.HdRasters is { } bank)
            foreach (var raster in bank.Values.Where(raster => !_textures.ContainsKey(raster)))
            {
                // Decode each declaration, even in an incomplete group.
                using var image = ExplorationRasterImages.Open(raster);
            }
        foreach (var binding in _spriteBindings.Values.Concat(_effectBindings.SelectMany(group => group)))
            Resolve(binding, hd);
    }

    internal void PublishGraphics()
    {
        foreach (var (sprite, binding) in _spriteBindings)
            if (sprite.Visible) Resolve(binding, _settings.GraphicsMode == "hd").Bind(sprite);
        _attackSpell.QueueRedraw();
        _phoenixWave.RefreshTextures();
    }

    private RasterBinding Raster(string key, bool groupHasHd, float originalScale)
    {
        var content = Content!;
        if (!content.Rasters.TryGetValue(key, out var original))
            throw new InvalidOperationException("battle-scene-texture-unavailable");
        return new(key, original, groupHasHd ? content.HdRasters?.GetValueOrDefault(key) : null,
            new Vector2(original.Width, original.Height) * originalScale);
    }

    private BattleSceneRaster Resolve(RasterBinding binding, bool useHd)
    {
        var hd = useHd ? binding.Hd : null;
        var raster = hd ?? binding.Original;
        if (!_textures.TryGetValue(raster, out var texture))
        {
            using var image = ExplorationRasterImages.Open(raster);
            texture = ImageTexture.CreateFromImage(image);
            _textures.Add(raster, texture);
        }
        return new(texture, binding.Size, hd is not null);
    }

    private Func<int, BattleSceneRaster> Frames(IReadOnlyList<string> keys)
    {
        bool groupHasHd = BattleSceneRaster.HasHdGroup(Content!, keys);
        var bindings = keys.Select(key => Raster(key, groupHasHd, 1)).ToArray();
        foreach (var binding in bindings) Resolve(binding, _settings.GraphicsMode == "hd");
        _effectBindings.Add(bindings);
        return index => Resolve(bindings[index], _settings.GraphicsMode == "hd");
    }

    private void Bind(Sprite2D node, string key, bool groupHasHd = true, float originalScale = 0.5f)
    {
        var binding = Raster(key, groupHasHd, originalScale);
        Resolve(binding, _settings.GraphicsMode == "hd").Bind(node);
        _spriteBindings[node] = binding;
    }

    private object BoundResource(Sprite2D node)
    {
        string? key = _spriteBindings.TryGetValue(node, out var binding) ? binding.Key : null;
        return new
        {
            resource = key, texturePresent = node.Texture is not null, visible = node.IsVisibleInTree(),
            hd = node.Texture is not null && binding.Hd is not null &&
                _textures.GetValueOrDefault(binding.Hd) == node.Texture,
            textureWidth = node.Texture?.GetWidth(), textureHeight = node.Texture?.GetHeight(),
            width = key is null ? (float?)null : binding.Size.X, height = key is null ? (float?)null : binding.Size.Y,
        };
    }

    private void RetireRasters()
    {
        // Effects drop their borrowers before this is called. Reparenting keeps the
        // presenters and bindings alive, so retirement follows playback, not ExitTree.
        foreach (var sprite in _spriteBindings.Keys) sprite.Texture = null;
        _spriteBindings.Clear();
        DisposeTextures();
    }

    private void DisposeTextures()
    {
        foreach (var texture in _textures.Values) texture.Dispose();
        _textures.Clear();
    }
}

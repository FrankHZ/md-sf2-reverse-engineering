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
        if (content is not null)
            foreach (var raster in SelectedRasters(content, hd).Distinct().Where(raster => !_textures.ContainsKey(raster)))
            {
                using var image = ExplorationRasterImages.Open(raster);
            }
        foreach (var binding in _spriteBindings.Values.Concat(_effectBindings.SelectMany(group => group)))
            Resolve(binding, hd);
    }

    private static IEnumerable<ExplorationRaster> SelectedRasters(BattleSceneDefinition content, bool hd)
    {
        // Validate declarations even if their group is incomplete. Also prepare the
        // actual Original fallback before an empty movement view can publish HD.
        var selected = new List<ExplorationRaster>();
        if (hd && content.HdRasters is { } variants) selected.AddRange(variants.Values);
        void Group(IEnumerable<string> resources)
        {
            var keys = resources.ToArray();
            bool useHd = hd && BattleSceneRaster.HasHdGroup(content, keys);
            foreach (string key in keys)
                selected.Add((useHd ? content.HdRasters! : content.Rasters)[key]);
        }
        var actors = content.Allies.Values.Concat(content.Enemies.Values)
            .Concat(content.Equipment?.Values ?? [])
            .Concat(content.AllyAppearances?.Values ?? [])
            .Concat(content.Species?.Values.Select(value => value.Body) ?? []);
        foreach (var actor in actors.Distinct())
        {
            Group(actor.Frames);
            Group(actor.WeaponFrames);
        }
        Group([content.Background]); Group([content.Ground]);
        foreach (var terrain in (content.TerrainScenes ?? []).Distinct())
        {
            Group([terrain.Background]); Group([terrain.Ground]);
        }
        if (content.Healing is { } heal)
        {
            Group(heal.Bodies.Concat(heal.Wings).Concat(heal.Dust));
            if (heal.Enemy is { } enemy) Group(enemy.Bodies.Concat(enemy.Wings).Concat(enemy.Dust));
        }
        if (content.FieldDeath is { } death) Group(death.ExitFrames);
        foreach (var keys in new[] { AttackSpellView.BlazeResources, BlazeMeteorEffect.SmallResources,
            BlazeMeteorEffect.LargeResources, BlazeMeteorEffect.DragonResources,
            FreezeEffect.Resources, BoltEffect.Resources,
            BlastEffect.RankOneResources, BlastEffect.RankTwoResources, BlastEffect.Resources, BreathResources, PhoenixWaveView.Resources, DetoxEffectView.Resources, DebuffEffectView.Resources })
            if (keys.All(content.Rasters.ContainsKey)) Group(keys);
        return selected;
    }

    internal void PublishGraphics()
    {
        foreach (var (sprite, binding) in _spriteBindings)
            if (sprite.Visible) Resolve(binding, _settings.GraphicsMode == "hd").Bind(sprite);
        _attackSpell.QueueRedraw();
        _phoenixWave.RefreshTextures();
        _detox.RefreshTextures();
        _debuff.RefreshTextures();
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

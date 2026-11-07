using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Entry;
using Sf2.Remake.GodotAdapter.Exploration;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Previews;

// Static review compositions. No session, save store, emulator or clocks are mounted.
internal sealed partial class PreviewStage : Control
{
    private readonly MapBlockTextures _blocks = new();
    private readonly Dictionary<string, ImageTexture> _textures = new();
    private EntryPresentationAssets? _entry;
    private ExplorationDefinition? _world;
    private IReadOnlyDictionary<string, ExplorationRaster>? _battle;
    private int _scene, _target = -1;
    private int _allySprite = 2, _opponentSprite = 103;
    internal bool HasWorld => _world?.Visuals is not null;
    internal bool HasWitch => _entry is not null;
    internal string? AssetFailure { get; private set; }

    internal void LoadSelection()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
        static string Env(string key) => System.Environment.GetEnvironmentVariable(key) ?? "";
        if (Env("SF2_PRIVATE_ENTRY_CONTENT").Length > 0)
        {
            try { _entry = new EntryPresentationAssets(); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException
                or InvalidOperationException or KeyNotFoundException or FormatException or System.Text.Json.JsonException)
            { AssetFailure = "entry"; }
        }
        if (Env("SF2_PRIVATE_EXPLORATION_CONTENT").Length == 0) return;
        var result = new OpeningNewGameReader(Env("SF2_PRIVATE_EXPLORATION_CONTENT"),
            Env("SF2_PRIVATE_BATTLE01_DATA"), Env("SF2_PRIVATE_BATTLE01_SCENE"), Env("SF2_PRIVATE_BATTLE01_TERRAIN"),
            Env("SF2_PRIVATE_STATIC_DATA"), Env("SF2_PRIVATE_ENEMY_DATA"), Env("SF2_PRIVATE_ENEMY_GOLD"),
            1, 1, Env("SF2_PRIVATE_BATTLE_SCENE_CONTENT")).Read();
        if (result is ExplorationReadAccepted accepted)
        {
            _world = accepted.Definition.Exploration;
            var battle = accepted.Definition.ScenesFor("battle-1");
            _battle = battle?.Rasters;
            if (battle?.FieldDeath is { } art)
            {
                _allySprite = art.AllySprites.GetValueOrDefault(1, _allySprite);
                _opponentSprite = art.EnemySprites.Values.FirstOrDefault(_opponentSprite);
            }
            if (_world?.Visuals?.ChurchMenu is null || _battle?.ContainsKey("command-menu.attack.normal") != true)
                AssetFailure = "command art";
        }
        else AssetFailure = result is ScenarioReadRejected rejected ? rejected.Failure.Code : "world";
    }

    internal void ShowScene(int scene, int target = -1)
    {
        _scene = scene;
        _target = target;
        QueueRedraw();
    }

    internal Texture2D? Icon(string key, bool selected)
    {
        string state = key + (selected ? ".selected" : ".normal");
        if (_textures.TryGetValue(state, out var ready)) return ready;
        var visuals = _world?.Visuals;
        var raster = key.StartsWith("command-menu.", StringComparison.Ordinal) ? _battle?.GetValueOrDefault(state)
            : key.StartsWith("identity-icon.", StringComparison.Ordinal) ? visuals?.IdentityIcons?.GetValueOrDefault(state)
            : visuals?.ChurchMenu?.GetValueOrDefault(state);
        if (raster is null) return null;
        using var image = ExplorationRasterImages.Open(raster);
        return _textures[state] = ImageTexture.CreateFromImage(image);
    }

    internal Texture2D? Portrait()
    {
        if (_textures.TryGetValue("portrait", out var ready)) return ready;
        if (_world?.Visuals?.Sprites.GetValueOrDefault(1)?.Portrait is not int id ||
            !_world.Visuals.Portraits.TryGetValue(id, out var face)) return null;
        return _textures["portrait"] = ExplorationRasterImages.Portrait(face);
    }

    private void Character(int sprite, Vector2 at, int face)
    {
        if (_world?.Visuals?.Sprites.GetValueOrDefault(sprite) is { } art)
        {
            string key = $"sprite-{sprite}-{face}";
            if (!_textures.TryGetValue(key, out var texture))
            {
                using var image = ExplorationRasterImages.Open(art.Directions[face]);
                using var frame = image.GetRegion(new(0, 0, 24, 24));
                _textures[key] = texture = ImageTexture.CreateFromImage(frame);
            }
            DrawTextureRect(texture, new(at, new(96, 96)), false);
        }
        else
        {
            DrawCircle(at + new Vector2(48, 35), 17, new Color("ebd6ae"));
            DrawRect(new(at + new Vector2(24, 53), new(48, 35)), new Color("738eaa"));
        }
    }

    public override void _Draw()
    {
        if (_scene == 0)
        {
            DrawRect(new(Vector2.Zero, Size), new Color("090f1b"));
            if (_entry is not null) DrawTextureRect(_entry.Textures["witch"], new(160, 92, 1024, 896), false);
            else DrawRect(new(160, 92, 1024, 896), new Color("172638"));
            return;
        }
        bool battle = _scene == 3;
        MapId id = new(battle ? "map-57" : "map-3");
        Vector2I origin = _scene switch { 1 => new(33, 3), 2 => new(25, 6), _ => new(-2, 4) };
        var visual = _world?.Visuals?.Maps.GetValueOrDefault(id);
        var layout = _world?.Maps.GetValueOrDefault(id)?.Layout;
        for (int y = 0; y < 12; y++)
            for (int x = 0; x < 20; x++)
            {
                Rect2 cell = new(x * 96, y * 96, 96, 96);
                if (battle && (x < 2 || x >= 18)) DrawRect(cell, new Color("111827"));
                else if (layout is not null && visual is not null)
                    DrawTextureRect(_blocks.Block(visual, layout[x + origin.X, y + origin.Y] & 1023), cell, false);
                else DrawRect(cell, (x + y) % 2 == 0 ? new Color("293e42") : new Color("30464a"));
            }
        if (battle)
        {
            Character(0, new(864, 480), 1);
            Character(_allySprite, new(1056, 480), 1);
            Character(_opponentSprite, new(960, 288), 0);
            if (_target >= 0)
                DrawRect(new(_target switch { 0 => new Vector2(864, 480), 1 => new(1056, 480), _ => new(960, 288) }, new(96, 96)),
                    new Color("ffe19a"), false, 4);
        }
        else
        {
            Vector2 speaker = _scene == 2 ? new(672, 480) : new(864, 480);
            Character(_scene == 2 ? 192 : 1, speaker, 0);
            Character(0, speaker + new Vector2(0, 96), 2);
        }
    }

    public override void _ExitTree()
    {
        _entry?.Dispose();
        _blocks.Dispose();
        foreach (var texture in _textures.Values) texture.Dispose();
    }
}

using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;
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
    internal BattleSceneDefinition? SceneArt { get; private set; }
    private BattleDefinition? _battle;
    internal PreviewInspectionCamera Camera { get; } = new();
    private int _scene, _target = -1;
    private Vector2I? _cursor;
    private readonly Sprite2D _cellCursor = new() { Name = "OriginalCellCursor", Centered = false, Visible = false };
    private bool _overview, _facingSign;
    private int _allySprite = 2, _chesterSprite = 5, _opponentSprite = 103;
    internal bool HasWorld => _world?.Visuals is not null;
    internal bool HasWitch => _entry is not null;
    internal string? AssetFailure { get; private set; }

    internal void LoadSelection()
    {
        AddChild(_cellCursor);
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
            _battle = accepted.Definition.Encounters.GetValueOrDefault("battle-1");
            if (_battle != null) Camera.Reset(new(_battle.Width, _battle.Height));
            var battle = accepted.Definition.ScenesFor("battle-1");
            SceneArt = battle;
            string cursorFile = ProjectSettings.GlobalizePath("res://../../local/design-preview/cell-cursor.png");
            if (battle is not null && File.Exists(cursorFile))
            {
                using var image = Image.LoadFromFile(cursorFile);
                if (image?.GetSize() == new Vector2I(32, 32))
                    _cellCursor.Texture = _textures["cell-cursor"] = ImageTexture.CreateFromImage(image);
            }
            if (battle?.FieldDeath is { } art)
            {
                _allySprite = art.AllySprites.GetValueOrDefault(1, _allySprite);
                _chesterSprite = art.AllySprites.GetValueOrDefault(2, _chesterSprite);
                _opponentSprite = art.EnemySprites.Values.FirstOrDefault(_opponentSprite);
            }
            if (_world?.Visuals?.ChurchMenu is null || _world.Visuals.ShopMenu is null || SceneArt?.Rasters.ContainsKey("command-menu.attack.normal") != true)
                AssetFailure = "command art";
        }
        else AssetFailure = result is ScenarioReadRejected rejected ? rejected.Failure.Code : "world";
    }

    internal void ShowScene(int scene, int target, Vector2I? cursor, bool overview, bool facingSign)
    {
        _scene = scene;
        _target = target;
        _cursor = cursor; _overview = overview; _facingSign = facingSign;
        _cellCursor.Visible = false;
        QueueRedraw();
    }

    internal Texture2D? Icon(string key, bool selected)
    {
        string state = key + (selected ? ".selected" : ".normal");
        if (_textures.TryGetValue(state, out var ready)) return ready;
        var visuals = _world?.Visuals;
        var raster = key.StartsWith("command-menu.", StringComparison.Ordinal) || key.StartsWith("battlefield-menu.", StringComparison.Ordinal)
            ? SceneArt?.Rasters.GetValueOrDefault(state)
            : key.StartsWith("field.", StringComparison.Ordinal) ? visuals?.FieldMenu?.GetValueOrDefault(state[6..])
            : key.StartsWith("op.", StringComparison.Ordinal) ? visuals?.ItemMenu?.GetValueOrDefault(state[3..])
            : key.StartsWith("identity-icon.", StringComparison.Ordinal) ? visuals?.IdentityIcons?.GetValueOrDefault(state)
            : key.StartsWith("shop.", StringComparison.Ordinal) ? visuals?.ShopMenu?.GetValueOrDefault(state[5..])
            : visuals?.ChurchMenu?.GetValueOrDefault(state);
        if (raster is null) return null;
        using var image = ExplorationRasterImages.Open(raster);
        return _textures[state] = ImageTexture.CreateFromImage(image);
    }

    internal int? LandEffect(int unit, Vector2I cell)
    {
        var deployment = _battle?.Deployments.FirstOrDefault(row => row.ProcessingOrder == (unit == 2 ? 128 : unit));
        if (_battle is null || deployment is null) return null;
        return BattleTerrainRules.LandEffect(_battle.Terrain[cell.Y * 48 + cell.X], deployment.Definition.Mover);
    }

    internal Rect2 BoardCell(Vector2I cell, bool overview)
    {
        if (overview) return Camera.OverviewCell(cell);
        var origin = _scene == DesignPreview.InspectScene ? Camera.Origin : PreviewInspectionCamera.Opening;
        return new(origin + (Vector2)cell * PreviewInspectionCamera.Tile, new(96, 96));
    }

    private void DrawCellCursor(Vector2I cell)
    {
        var rect = BoardCell(cell, _overview);
        if (_cellCursor.Texture is not null)
        {
            // 24 source pixels per board cell. Center the private sprite on the review grid.
            float scale = _overview ? Mathf.Max(1, Mathf.Floor(rect.Size.X / 24)) : 4;
            _cellCursor.Scale = Vector2.One * scale;
            _cellCursor.Position = rect.GetCenter() - Vector2.One * (16 * scale);
            _cellCursor.Show();
            return;
        }
        foreach (var dir in new[] { new Vector2(1, 1), new(-1, 1), new(1, -1), new(-1, -1) })
        {
            var corner = rect.GetCenter() - rect.Size * dir / 2;
            float arm = rect.Size.X / 4;
            DrawPolyline([corner + new Vector2(dir.X * arm, 0), corner, corner + new Vector2(0, dir.Y * arm)],
                Colors.White, _overview ? 2 : 4);
        }
    }

    internal Texture2D BattleTexture(string resource)
    {
        if (_textures.TryGetValue(resource, out var texture)) return texture;
        using var image = ExplorationRasterImages.Open(SceneArt!.Rasters[resource]);
        var result = ImageTexture.CreateFromImage(image);
        result.ResourceName = resource;
        return _textures[resource] = result;
    }

    internal Texture2D? MemberPortrait(int member) => Portrait(member switch { 1 => _allySprite, 2 => _chesterSprite, _ => 0 });

    internal Texture2D? Portrait(int sprite = 1)
    {
        if (_world?.Visuals?.Sprites.GetValueOrDefault(sprite)?.Portrait is not int id ||
            !_world.Visuals.Portraits.TryGetValue(id, out var face)) return null;
        string key = $"portrait-{id}";
        if (_textures.TryGetValue(key, out var ready)) return ready;
        return _textures[key] = ExplorationRasterImages.Portrait(face);
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
        bool battle = _scene is 3 or DesignPreview.InspectScene or DesignPreview.OutcomeScene;
        bool shop = _scene == DesignPreview.ShopScene;
        MapId id = new(battle ? "map-57" : shop ? "map-16" : "map-3");
        Vector2I origin = _scene switch { 1 or DesignPreview.FieldScene => new(33, 3), 2 => new(25, 6), DesignPreview.ShopScene => new(30, 19), _ => new(-2, 4) };
        var visual = _world?.Visuals?.Maps.GetValueOrDefault(id);
        var layout = _world?.Maps.GetValueOrDefault(id)?.Layout;
        if (battle) DrawRect(new(Vector2.Zero, Size), new Color("111827"));
        var dimensions = battle ? Camera.MapSize : new Vector2I(20, 12);
        var source = _battle?.SourceMapRegion?.Origin;
        if (battle) origin = source is null ? Vector2I.Zero : new(source.X, source.Y);
        for (int y = 0; y < dimensions.Y; y++)
            for (int x = 0; x < dimensions.X; x++)
            {
                Rect2 cell = battle ? BoardCell(new(x, y), _overview) : new(x * 96, y * 96, 96, 96);
                if (!cell.Intersects(new Rect2(Vector2.Zero, Size))) continue;
                if (layout is not null && visual is not null)
                    DrawTextureRect(_blocks.Block(visual, layout[x + origin.X, y + origin.Y] & 1023), cell, false);
                else DrawRect(cell, (x + y) % 2 == 0 ? new Color("293e42") : new Color("30464a"));
            }
        if (_overview)
        {
            for (int i = 0; i < DesignPreview.UnitCells.Length; i++)
            {
                Vector2 point = BoardCell(DesignPreview.UnitCells[i], true).GetCenter();
                DrawCircle(point, 16, Colors.Black); DrawCircle(point, 12, new Color(i == 2 ? "ed987f" : "91d1f3"));
            }
            if (_cursor is { } located) DrawCellCursor(located);
        }
        else if (battle)
        {
            Character(0, BoardCell(DesignPreview.UnitCells[0], false).Position, 1);
            Character(_allySprite, BoardCell(DesignPreview.UnitCells[1], false).Position, 1);
            Character(_opponentSprite, BoardCell(DesignPreview.UnitCells[2], false).Position, 0);
            if (_cursor is { } cursor) DrawCellCursor(cursor);
            else if (_target >= 0) DrawCellCursor(DesignPreview.UnitCells[_target]);
        }
        else if (_scene == DesignPreview.FieldScene)
        {
            DrawRect(new(904, 524, 12, 48), new Color("60432f"));
            DrawRect(new(874, 503, 72, 40), new Color("543c2e"));
            DrawRect(new(878, 507, 64, 32), new Color("c5a778"));
            DrawLine(new(887, 516), new(930, 516), new Color("675644"), 3);
            DrawLine(new(887, 525), new(919, 525), new Color("675644"), 3);
            Character(0, new(864, 576), _facingSign ? 2 : 1);
        }
        else
        {
            Vector2 speaker = shop ? new(480, 480) : _scene == 2 ? new(672, 480) : new(864, 480);
            Character(shop ? 193 : _scene == 2 ? 192 : 1, speaker, 0);
            Character(0, speaker + new Vector2(0, shop ? 192 : 96), 2);
        }
    }

    public override void _ExitTree()
    {
        _entry?.Dispose();
        _blocks.Dispose();
        foreach (var texture in _textures.Values) texture.Dispose();
    }
}

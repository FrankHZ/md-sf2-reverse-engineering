using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal sealed record PrivateBattle01Tile(MapPosition Position, byte Terrain, bool Reachable, bool CanStop);
internal sealed record PrivateBattle01Unit(int Index, MapPosition Position, ushort Hp);
internal sealed record PrivateBattle01Projection(
    Battle01Phase Phase, int Width, int Height, int? ActorIndex,
    IReadOnlyList<PrivateBattle01Tile> Tiles, IReadOnlyList<PrivateBattle01Unit> Units,
    MapPosition? Cursor, IReadOnlyList<MapPosition> Path, int? GridCost, int? PathCost, int? Budget,
    bool CanConfirm, string Controls, string Status);

// Reviewed fixed Map57 base art is optional; live units always remain diagnostic markers.
public sealed partial class PrivateBattle01Presenter : Node2D
{
    internal const string Heading = "DIAGNOSTIC BATTLEFIELD  |  Map 57 / Battle 01";
    internal const string Boundary = "Controlled inputs. Original Map 57 graphics unavailable.";
    internal const string BaseArtHeading = "MAP 57 BASE ART + DIAGNOSTIC UNITS";
    internal const string BaseArtBoundary = "Reviewed fixed base art. Controlled inputs.\nOriginal scene, layers and animation are not reproduced.";
    internal const int TileSize = PrivateOriginalMapBaseViewProjection.BlockPixelSize;
    internal static readonly Vector2 GridOrigin = new(40, 32);
    private PrivateBattle01Projection? _projection;
    private PrivateBattle01BaseViewProjection? _baseView;
    private ImageTexture? _baseTexture;
    private Label? _details;
    private Label? _allies;
    private Label? _enemies;
    private Label? _remainingEnemies;
    private Label? _status;
    private Label? _controls;

    internal PrivateBattle01Projection? Projection => _projection;
    internal PrivateBattle01BaseViewProjection? BaseView => _baseView;
    internal bool BaseArtUnavailable { get; private set; }

    public PrivateBattle01Presenter()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        TextureRepeat = TextureRepeatEnum.Disabled;
    }

    internal bool TryBindBaseAtlas(PrivateLocalPresentationRasterMount mount,
        OriginalBattle01AdmissionDefinition definition)
    {
        if (!PrivateLocalPresentationAssetCatalog.IsExactMap57BaseAtlasBinding(mount.Definition, mount.Bucket))
            return false;
        using Image image = new();
        if (image.LoadPngFromBuffer(mount.CopyPngBytes()) != Error.Ok ||
            image.GetWidth() != mount.Bucket.Width || image.GetHeight() != mount.Bucket.Height ||
            image.GetFormat() != Image.Format.Rgba8)
            return false;
        try
        {
            var view = PrivateBattle01BaseViewProjection.Create(definition, image.GetData(), mount.Bucket.Scale);
            using Image rendered = Image.CreateFromData(view.PixelWidth * view.RasterScale,
                view.PixelHeight * view.RasterScale, false, Image.Format.Rgba8, view.RgbaBytes.ToArray());
            _baseTexture = ImageTexture.CreateFromImage(rendered);
            _baseView = view;
            return true;
        }
        catch (ArgumentException) { return false; }
    }

    internal void ProjectBaseArtUnavailable()
    {
        BaseArtUnavailable = true;
        AddLabel("Map 57 base art unavailable.\nRelaunch with the reviewed asset pack and fixed layout.",
            new(40, 40), new(880, 140), 24);
        QueueRedraw();
    }

    internal static PrivateBattle01Projection BuildProjection(Battle01InitializedState battle, string status)
    {
        var control = battle.FirstControl;
        var movement = control?.Movement;
        var tiles = new List<PrivateBattle01Tile>();
        for (int y = 0; y < battle.AreaHeight; y++)
        for (int x = 0; x < battle.AreaWidth; x++)
        {
            var position = new MapPosition(x + battle.AreaX, y + battle.AreaY);
            tiles.Add(new(position, battle.TerrainAt(position),
                movement?.Range.Grid.CostAt(position) is not null,
                movement?.Range.CanStopAt(position) == true));
        }
        int? actor = control?.ActorIndex ?? battle.FirstRound?.FirstCandidate?.CombatantIndex;
        string controls = battle.Phase switch
        {
            Battle01Phase.PlayerMovementSelection => "I / J / K / L: cursor   Space: confirm   Backspace: cancel",
            Battle01Phase.PlayerActionChoice => "Backspace: cancel relocation. Actions and turn completion are not connected.",
            _ => "Current battle retained. Relaunch starts Map 3.",
        };
        return new(battle.Phase, battle.AreaWidth, battle.AreaHeight, actor, tiles.AsReadOnly(),
            Array.AsReadOnly(battle.Roster.Select(unit => new PrivateBattle01Unit(unit.Index, unit.Position, unit.Stats.HpCurrent)).ToArray()),
            movement?.Cursor, movement?.Preview.Positions ?? Array.Empty<MapPosition>(),
            movement?.GridCost, movement?.Preview.Cost, movement?.Range.Budget,
            movement?.Stage == Battle01PlayerMovementStage.Selection && movement.CanConfirm, controls, status);
    }

    internal void Project(Battle01InitializedState battle, string status)
    {
        _projection = BuildProjection(battle, status);
        if (_details is null)
        {
            AddLabel(_baseView is null ? Heading : BaseArtHeading, new(456, 18), new(480, 48), 22);
            AddLabel(_baseView is null ? Boundary : BaseArtBoundary, new(456, 72), new(480, 50), 16);
            _details = AddLabel("", new(456, 130), new(480, 108), 16);
            AddLabel("Live units  |  (x,y)  |  HP", new(456, 244), new(480, 26), 16);
            _allies = AddLabel("", new(456, 274), new(144, 90), 15);
            _enemies = AddLabel("", new(624, 274), new(144, 90), 15);
            _remainingEnemies = AddLabel("", new(792, 274), new(144, 90), 15);
            _status = AddLabel("", new(456, 382), new(480, 82), 13);
            _controls = AddLabel("", new(456, 474), new(480, 56), 15);
        }
        var view = _projection;
        string cursor = view.Cursor is null ? "unavailable" : $"({view.Cursor.X},{view.Cursor.Y})";
        string terrain = view.Cursor is null ? "-" : battle.TerrainAt(view.Cursor).ToString("X2");
        _details.Text = $"Stage: {view.Phase}\n" +
            $"Current actor: {view.ActorIndex?.ToString() ?? "-"}    Cursor: {cursor}    Terrain: {terrain}\n" +
            $"Grid cost: {view.GridCost?.ToString() ?? "-"}   Path cost: {view.PathCost?.ToString() ?? "-"}   Budget: {view.Budget?.ToString() ?? "-"}\n" +
            $"Confirm: {(view.CanConfirm ? "available" : "unavailable")}";
        static string UnitLine(PrivateBattle01Unit unit) =>
            $"{UnitTag(unit.Index)} ({unit.Position.X},{unit.Position.Y}) HP {unit.Hp}";
        _allies!.Text = string.Join("\n", view.Units.Where(unit => unit.Index < 128).Select(UnitLine));
        _enemies!.Text = string.Join("\n", view.Units.Where(unit => unit.Index >= 128).Take(3).Select(UnitLine));
        _remainingEnemies!.Text = string.Join("\n", view.Units.Where(unit => unit.Index >= 128).Skip(3).Select(UnitLine));
        _status!.Text = "Teal: reachable  |  gold: actor / path\n" +
            (_baseView is null ? "Tile number: terrain ID; dots: legal stops\n" :
                "Terrain: current cursor; dots: legal stops\n") + view.Status;
        _controls!.Text = view.Controls;
        QueueRedraw();
    }

    private Label AddLabel(string text, Vector2 position, Vector2 size, int fontSize)
    {
        var label = new Label { Text = text, Position = position, Size = size,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        AddChild(label);
        return label;
    }

    internal static string UnitTag(int index) => index < 128 ? $"A{index}" : $"E{index - 128}";

    internal static Vector2 Cell(MapPosition position) => GridOrigin + new Vector2(position.X, position.Y) * TileSize;

    public override void _Draw()
    {
        DrawRect(new Rect2(0, 0, 960, 540), new Color("#101821"));
        if (_projection is not { } view) return;
        if (_baseTexture is not null)
            DrawTextureRect(_baseTexture, new Rect2(GridOrigin, new Vector2(view.Width, view.Height) * TileSize), false);
        var font = ThemeDB.FallbackFont;
        for (int x = 0; x < view.Width; x++)
            DrawString(font, GridOrigin + new Vector2(x * TileSize + 2, -6), x.ToString(), fontSize: 10);
        for (int y = 0; y < view.Height; y++)
            DrawString(font, GridOrigin + new Vector2(-22, y * TileSize + 14), y.ToString(), fontSize: 10);
        foreach (var tile in view.Tiles)
        {
            Vector2 at = Cell(tile.Position);
            var fill = tile.Reachable ? new Color("#245c62") :
                tile.Terrain == 255 ? new Color("#111720") : new Color("#303b49");
            if (_baseView is null)
                DrawRect(new Rect2(at, new Vector2(TileSize - 1, TileSize - 1)), fill);
            else if (tile.Reachable)
                DrawRect(new Rect2(at, new Vector2(TileSize, TileSize)), new Color(0.15f, 0.65f, 0.65f, 0.20f));
            DrawRect(new Rect2(at, new Vector2(TileSize, TileSize)), new Color(0, 0, 0, 0.20f), false);
            if (tile.Terrain != 255 && _baseView is null)
                DrawString(font, at + new Vector2(2, 11), tile.Terrain.ToString("X"), fontSize: 9,
                    modulate: new Color("#9eafbd"));
            if (tile.CanStop) DrawCircle(at + new Vector2(TileSize - 4, TileSize - 4), 1.5f, new Color("#75c4ba"));
        }
        for (int i = 1; i < view.Path.Count; i++)
            DrawLine(Cell(view.Path[i - 1]) + Vector2.One * (TileSize / 2),
                Cell(view.Path[i]) + Vector2.One * (TileSize / 2), new Color("#f2cc75"), 2);
        foreach (var unit in view.Units)
        {
            Vector2 at = Cell(unit.Position);
            var color = unit.Index < 128 ? new Color("#377abc") : new Color("#b45263");
            DrawRect(new Rect2(at + Vector2.One * 4, new Vector2(16, 16)), color);
            if (unit.Index == view.ActorIndex)
                DrawRect(new Rect2(at + Vector2.One * 3, new Vector2(18, 18)), new Color("#f2cc75"), false, 2);
            DrawString(font, at + new Vector2(4, 15), UnitTag(unit.Index), fontSize: 10);
        }
        if (view.Cursor is { } cursor)
            DrawRect(new Rect2(Cell(cursor), new Vector2(TileSize, TileSize)), Colors.White, false, 2);
    }
}

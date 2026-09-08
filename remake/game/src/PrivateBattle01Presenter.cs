using Godot;
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

// Project-authored geometry only. No Map 40 or original Map 57 raster is used here.
public sealed partial class PrivateBattle01Presenter : Node2D
{
    internal const string Heading = "DIAGNOSTIC BATTLEFIELD  |  Map 57 / Battle 01";
    internal const string Boundary = "Controlled inputs. Original Map 57 graphics unavailable.";
    internal const int TileSize = 20;
    internal static readonly Vector2 GridOrigin = new(48, 100);
    private PrivateBattle01Projection? _projection;
    private Label? _details;
    private Label? _allies;
    private Label? _enemies;
    private Label? _status;
    private Label? _controls;

    internal PrivateBattle01Projection? Projection => _projection;

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
            AddLabel(Heading, new(24, 18), new(912, 32), 25);
            AddLabel(Boundary, new(24, 54), new(912, 28), 18);
            _details = AddLabel("", new(408, 98), new(528, 110), 16);
            AddLabel("Live units  |  (x,y)  |  HP", new(408, 208), new(528, 26), 16);
            _allies = AddLabel("", new(408, 238), new(252, 156), 16);
            _enemies = AddLabel("", new(676, 238), new(260, 156), 16);
            _status = AddLabel("", new(408, 404), new(528, 96), 16);
            _controls = AddLabel("", new(24, 508), new(912, 28), 17);
        }
        var view = _projection;
        string cursor = view.Cursor is null ? "unavailable" : $"({view.Cursor.X},{view.Cursor.Y})";
        string terrain = view.Cursor is null ? "-" : battle.TerrainAt(view.Cursor).ToString("X2");
        _details.Text = $"Stage: {view.Phase}\n" +
            $"Current actor: {view.ActorIndex?.ToString() ?? "-"}    Cursor: {cursor}    Terrain: {terrain}\n" +
            $"Grid cost: {view.GridCost?.ToString() ?? "-"}   Path cost: {view.PathCost?.ToString() ?? "-"}   Budget: {view.Budget?.ToString() ?? "-"}\n" +
            $"Confirm: {(view.CanConfirm ? "available" : "unavailable")}";
        static string UnitLine(PrivateBattle01Unit unit) =>
            $"{UnitTag(unit.Index)} [{unit.Index}]  ({unit.Position.X},{unit.Position.Y})  HP {unit.Hp}";
        _allies!.Text = string.Join("\n", view.Units.Where(unit => unit.Index < 128).Select(UnitLine));
        _enemies!.Text = string.Join("\n", view.Units.Where(unit => unit.Index >= 128).Select(UnitLine));
        _status!.Text = "Teal: reachable  |  gold: actor / path\n" +
            "Tile number: terrain ID; dots: legal stops\n" + view.Status;
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

    private static Vector2 Cell(MapPosition position) => GridOrigin + new Vector2(position.X, position.Y) * TileSize;

    public override void _Draw()
    {
        if (_projection is not { } view) return;
        DrawRect(new Rect2(0, 0, 960, 540), new Color("#101821"));
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
            DrawRect(new Rect2(at, new Vector2(TileSize - 1, TileSize - 1)), fill);
            if (tile.Terrain != 255)
                DrawString(font, at + new Vector2(2, 11), tile.Terrain.ToString("X"), fontSize: 9,
                    modulate: new Color("#9eafbd"));
            if (tile.CanStop) DrawCircle(at + new Vector2(16, 16), 1, new Color("#75c4ba"));
        }
        for (int i = 1; i < view.Path.Count; i++)
            DrawLine(Cell(view.Path[i - 1]) + Vector2.One * 10,
                Cell(view.Path[i]) + Vector2.One * 10, new Color("#f2cc75"), 2);
        foreach (var unit in view.Units)
        {
            Vector2 at = Cell(unit.Position);
            var color = unit.Index < 128 ? new Color("#377abc") : new Color("#b45263");
            DrawRect(new Rect2(at + Vector2.One * 2, new Vector2(16, 16)), color);
            if (unit.Index == view.ActorIndex)
                DrawRect(new Rect2(at + Vector2.One, new Vector2(18, 18)), new Color("#f2cc75"), false, 2);
            DrawString(font, at + new Vector2(2, 13), UnitTag(unit.Index), fontSize: 10);
        }
        if (view.Cursor is { } cursor)
            DrawRect(new Rect2(Cell(cursor), new Vector2(TileSize, TileSize)), Colors.White, false, 2);
    }
}

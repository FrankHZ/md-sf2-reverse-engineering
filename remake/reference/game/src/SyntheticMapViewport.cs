using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class SyntheticMapViewport : Node2D
{
    internal const int TileSize = 48;
    internal const int VisibleColumns = 12;
    internal const int VisibleRows = 7;
    internal static readonly Vector2 CanvasSize = new(
        VisibleColumns * TileSize,
        VisibleRows * TileSize);

    private GameSessionSnapshot? _snapshot;

    public void Project(GameSessionSnapshot snapshot)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_snapshot is null)
        {
            return;
        }

        int playerX = _snapshot.Exploration.PlayerPosition.X;
        int playerY = _snapshot.Exploration.PlayerPosition.Y;
        int startX = Math.Clamp(
            playerX - (VisibleColumns / 2),
            0,
            _snapshot.Exploration.Walkability.Width - VisibleColumns);
        int startY = Math.Clamp(
            playerY - (VisibleRows / 2),
            0,
            _snapshot.Exploration.Walkability.Height - VisibleRows);

        for (int row = 0; row < VisibleRows; row++)
        {
            for (int column = 0; column < VisibleColumns; column++)
            {
                int mapX = startX + column;
                int mapY = startY + row;
                bool passable = _snapshot.Exploration.Walkability.IsPassable(
                    new MapPosition(mapX, mapY));
                ushort word = _snapshot.Exploration.Layout[mapX, mapY];
                Color fill = passable
                    ? word % 2 == 0
                        ? new Color("3d6f64")
                        : new Color("4d8068")
                    : new Color("263245");
                Rect2 tile = new(
                    new Vector2(column * TileSize, row * TileSize),
                    new Vector2(TileSize - 2, TileSize - 2));
                DrawRect(tile, fill);

                if (!passable)
                {
                    DrawLine(
                        tile.Position + new Vector2(10, 10),
                        tile.End - new Vector2(10, 10),
                        new Color("9aa8bd"),
                        2);
                    DrawLine(
                        new Vector2(tile.End.X - 10, tile.Position.Y + 10),
                        new Vector2(tile.Position.X + 10, tile.End.Y - 10),
                        new Color("9aa8bd"),
                        2);
                }

                MapEntityDefinition? entity = _snapshot.Entities.SingleOrDefault(
                    candidate => candidate.Position.X == mapX && candidate.Position.Y == mapY);
                if (entity is not null)
                {
                    DrawEntityGlyph(tile.GetCenter());
                }

                if (mapX == playerX && mapY == playerY)
                {
                    DrawPlayerGlyph(tile.GetCenter(), _snapshot.Facing);
                }
            }
        }

        DrawRect(new Rect2(Vector2.Zero, CanvasSize), new Color("9aa8bd"), filled: false, width: 2);
    }

    internal static Vector2 FacingVector(SemanticFacing facing) => facing switch
    {
        SemanticFacing.North => Vector2.Up,
        SemanticFacing.East => Vector2.Right,
        SemanticFacing.South => Vector2.Down,
        SemanticFacing.West => Vector2.Left,
        _ => Vector2.Zero,
    };

    private void DrawPlayerGlyph(Vector2 center, SemanticFacing facing)
    {
        Vector2 direction = FacingVector(facing);
        Vector2 side = new(-direction.Y, direction.X);
        Vector2 tip = center + (direction * 16);
        Vector2 tail = center - (direction * 11);
        Vector2[] arrow =
        [
            tip,
            tail + (side * 10),
            tail - (side * 10),
        ];
        DrawColoredPolygon(arrow, new Color("ffd166"));
        DrawPolyline([tip, tail + (side * 10), tail - (side * 10), tip], new Color("4a3000"), 3);
        DrawCircle(center, 4, new Color("4a3000"));
    }

    private void DrawEntityGlyph(Vector2 center)
    {
        Vector2[] diamond =
        [
            center + new Vector2(0, -14),
            center + new Vector2(14, 0),
            center + new Vector2(0, 14),
            center + new Vector2(-14, 0),
            center + new Vector2(0, -14),
        ];
        DrawPolyline(diamond, new Color("f2f5ff"), 3);
        DrawCircle(center, 5, new Color("f2f5ff"));
    }
}

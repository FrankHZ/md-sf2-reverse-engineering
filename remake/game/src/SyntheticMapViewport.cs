using Godot;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class SyntheticMapViewport : Node2D
{
    private const int TileSize = 48;
    private const int VisibleColumns = 12;
    private const int VisibleRows = 7;

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

                if (mapX == playerX && mapY == playerY)
                {
                    Rect2 player = tile.Grow(-10);
                    DrawRect(player, new Color("ffd166"));
                    DrawCircle(player.GetCenter(), 7, new Color("7a4f00"));
                }
            }
        }
    }
}

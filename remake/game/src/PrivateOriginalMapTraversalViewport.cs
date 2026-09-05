using System.Collections.ObjectModel;
using Godot;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal enum PrivateOriginalMapTraversalCellCategory
{
    OutsideAcceptedActiveArea,
    ActiveNonBlocked,
    BlockedByAcceptedCollisionClass,
}

internal sealed record PrivateOriginalMapTraversalViewCell(
    int MapX,
    int MapY,
    int Column,
    int Row,
    PrivateOriginalMapTraversalCellCategory Category,
    bool IsPlayer);

internal sealed class PrivateOriginalMapTraversalViewProjection
{
    public const int ColumnCount = 12;
    public const int RowCount = 7;

    private readonly ReadOnlyCollection<PrivateOriginalMapTraversalViewCell> _cells;

    private PrivateOriginalMapTraversalViewProjection(
        MapId map,
        int originX,
        int originY,
        int playerColumn,
        int playerRow,
        IEnumerable<PrivateOriginalMapTraversalViewCell> cells)
    {
        Map = map;
        OriginX = originX;
        OriginY = originY;
        PlayerColumn = playerColumn;
        PlayerRow = playerRow;
        _cells = Array.AsReadOnly(cells.ToArray());
    }

    public MapId Map { get; }

    public int OriginX { get; }

    public int OriginY { get; }

    public int PlayerColumn { get; }

    public int PlayerRow { get; }

    public IReadOnlyList<PrivateOriginalMapTraversalViewCell> Cells => _cells;

    public static PrivateOriginalMapTraversalViewProjection Create(
        PrivateOriginalMapSessionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        int originX = Math.Clamp(
            snapshot.PlayerPosition.X - (ColumnCount / 2),
            0,
            WorkingMapLayout.ColumnCount - ColumnCount);
        int originY = Math.Clamp(
            snapshot.PlayerPosition.Y - (RowCount / 2),
            0,
            WorkingMapLayout.RowCount - RowCount);
        List<PrivateOriginalMapTraversalViewCell> cells = [];

        for (int row = 0; row < RowCount; row++)
        {
            for (int column = 0; column < ColumnCount; column++)
            {
                int mapX = originX + column;
                int mapY = originY + row;
                MapPosition position = new(mapX, mapY);
                PrivateOriginalMapTraversalCellCategory category =
                    Classify(snapshot, position);
                cells.Add(new PrivateOriginalMapTraversalViewCell(
                    mapX,
                    mapY,
                    column,
                    row,
                    category,
                    position == snapshot.PlayerPosition));
            }
        }

        return new PrivateOriginalMapTraversalViewProjection(
            snapshot.Map,
            originX,
            originY,
            snapshot.PlayerPosition.X - originX,
            snapshot.PlayerPosition.Y - originY,
            cells);
    }

    private static PrivateOriginalMapTraversalCellCategory Classify(
        PrivateOriginalMapSessionSnapshot snapshot,
        MapPosition position)
    {
        if (!snapshot.CurrentRuntime.Traversal.IsWithinActiveArea(position))
        {
            return PrivateOriginalMapTraversalCellCategory.OutsideAcceptedActiveArea;
        }

        return OriginalMapTraversal.IsBlocked(snapshot.WorkingLayout, position)
            ? PrivateOriginalMapTraversalCellCategory.BlockedByAcceptedCollisionClass
            : PrivateOriginalMapTraversalCellCategory.ActiveNonBlocked;
    }
}

public sealed partial class PrivateOriginalMapTraversalViewport : Node2D
{
    private const int TileSize = 48;

    private static readonly Color OutsideActiveAreaColor = new("242a33");
    private static readonly Color ActiveNonBlockedColor = new("35665a");
    private static readonly Color BlockedByCollisionColor = new("6f3546");
    private static readonly Color PlayerColor = new("ffd166");

    private PrivateOriginalMapTraversalViewProjection? _projection;

    internal PrivateOriginalMapTraversalViewProjection? Projection => _projection;

    public void Project(PrivateOriginalMapSessionSnapshot snapshot)
    {
        _projection = PrivateOriginalMapTraversalViewProjection.Create(snapshot);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_projection is null)
        {
            return;
        }

        foreach (PrivateOriginalMapTraversalViewCell cell in _projection.Cells)
        {
            Color fill = cell.Category switch
            {
                PrivateOriginalMapTraversalCellCategory.OutsideAcceptedActiveArea =>
                    OutsideActiveAreaColor,
                PrivateOriginalMapTraversalCellCategory.ActiveNonBlocked =>
                    ActiveNonBlockedColor,
                PrivateOriginalMapTraversalCellCategory.BlockedByAcceptedCollisionClass =>
                    BlockedByCollisionColor,
                _ => throw new InvalidOperationException(
                    "Unknown private traversal diagnostic category."),
            };
            Rect2 tile = new(
                new Vector2(cell.Column * TileSize, cell.Row * TileSize),
                new Vector2(TileSize - 2, TileSize - 2));
            DrawRect(tile, fill);

            if (cell.IsPlayer)
            {
                DrawCircle(tile.GetCenter(), 12, PlayerColor);
            }
        }
    }
}

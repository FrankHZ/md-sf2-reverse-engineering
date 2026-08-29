using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public enum ExplorationDirection
{
    North,
    East,
    South,
    West,
}

public sealed record MapPosition
{
    public MapPosition(int x, int y)
    {
        ValidateCoordinate(x, nameof(x));
        ValidateCoordinate(y, nameof(y));
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }

    private static void ValidateCoordinate(int value, string parameterName)
    {
        if (value < 0 || value >= WorkingMapLayout.ColumnCount)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}

public sealed class SyntheticWalkabilityGrid
{
    private readonly bool[] _passableCells;
    private readonly ReadOnlyCollection<bool> _readOnlyPassableCells;

    public SyntheticWalkabilityGrid(int width, int height, IEnumerable<bool> passableCells)
    {
        if (width < 1 || width > WorkingMapLayout.ColumnCount)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height < 1 || height > WorkingMapLayout.RowCount)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        ArgumentNullException.ThrowIfNull(passableCells);
        int expectedCount = checked(width * height);
        bool[] copied = [.. passableCells.Take(expectedCount + 1)];
        if (copied.Length != expectedCount)
        {
            throw new ArgumentException(
                $"A traversal grid must contain exactly {expectedCount} cells.",
                nameof(passableCells));
        }

        Width = width;
        Height = height;
        _passableCells = copied;
        _readOnlyPassableCells = Array.AsReadOnly(_passableCells);
    }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<bool> PassableCells => _readOnlyPassableCells;

    public bool Contains(MapPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        return position.X < Width && position.Y < Height;
    }

    public bool IsPassable(MapPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        return Contains(position) && _passableCells[(position.Y * Width) + position.X];
    }
}

public sealed record ExplorationMovementState
{
    public ExplorationMovementState(
        MapId map,
        WorkingMapLayout layout,
        SyntheticWalkabilityGrid walkability,
        MapPosition playerPosition)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        Walkability = walkability ?? throw new ArgumentNullException(nameof(walkability));
        PlayerPosition = playerPosition ?? throw new ArgumentNullException(nameof(playerPosition));
        if (!walkability.IsPassable(playerPosition))
        {
            throw new ArgumentException(
                "The admitted player position must be a passable synthetic cell.",
                nameof(playerPosition));
        }
    }

    public MapId Map { get; }

    public WorkingMapLayout Layout { get; }

    public SyntheticWalkabilityGrid Walkability { get; }

    public MapPosition PlayerPosition { get; }
}

public sealed record ExplorationMovementCommand
{
    public ExplorationMovementCommand(ExplorationDirection direction)
    {
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        Direction = direction;
    }

    public ExplorationDirection Direction { get; }
}

public enum ExplorationMovementOutcome
{
    Moved,
    BlockedByBoundary,
    BlockedByTerrain,
}

public sealed record ExplorationMovementResult(
    ExplorationMovementState State,
    ExplorationMovementOutcome Outcome)
{
    public ExplorationMovementState State { get; } =
        State ?? throw new ArgumentNullException(nameof(State));
}

public static class ExplorationMovementReducer
{
    public static ExplorationMovementResult TryMove(
        ExplorationMovementState state,
        ExplorationMovementCommand command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);

        (int deltaX, int deltaY) = command.Direction switch
        {
            ExplorationDirection.North => (0, -1),
            ExplorationDirection.East => (1, 0),
            ExplorationDirection.South => (0, 1),
            ExplorationDirection.West => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(command)),
        };

        int destinationX = state.PlayerPosition.X + deltaX;
        int destinationY = state.PlayerPosition.Y + deltaY;
        if (destinationX < 0 ||
            destinationX >= state.Walkability.Width ||
            destinationY < 0 ||
            destinationY >= state.Walkability.Height)
        {
            return new ExplorationMovementResult(
                state,
                ExplorationMovementOutcome.BlockedByBoundary);
        }

        MapPosition destination = new(destinationX, destinationY);
        if (!state.Walkability.IsPassable(destination))
        {
            return new ExplorationMovementResult(
                state,
                ExplorationMovementOutcome.BlockedByTerrain);
        }

        return new ExplorationMovementResult(
            new ExplorationMovementState(
                state.Map,
                state.Layout,
                state.Walkability,
                destination),
            ExplorationMovementOutcome.Moved);
    }
}

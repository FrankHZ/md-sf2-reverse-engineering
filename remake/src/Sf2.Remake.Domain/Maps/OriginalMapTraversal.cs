using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public sealed record OriginalMapTraversalArea
{
    public OriginalMapTraversalArea(int minimumX, int minimumY, int maximumX, int maximumY)
    {
        ValidateCoordinate(minimumX, nameof(minimumX));
        ValidateCoordinate(minimumY, nameof(minimumY));
        ValidateCoordinate(maximumX, nameof(maximumX));
        ValidateCoordinate(maximumY, nameof(maximumY));
        if (minimumX > maximumX || minimumY > maximumY)
        {
            throw new ArgumentException("An active map area must have ordered inclusive bounds.");
        }

        MinimumX = minimumX;
        MinimumY = minimumY;
        MaximumX = maximumX;
        MaximumY = maximumY;
    }

    public int MinimumX { get; }

    public int MinimumY { get; }

    public int MaximumX { get; }

    public int MaximumY { get; }

    public bool Contains(MapPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        return position.X >= MinimumX &&
            position.X <= MaximumX &&
            position.Y >= MinimumY &&
            position.Y <= MaximumY;
    }

    private static void ValidateCoordinate(int value, string parameterName)
    {
        if (value < 0 || value >= WorkingMapLayout.ColumnCount)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}

public sealed record OriginalMapTraversalAreaSelection
{
    public OriginalMapTraversalAreaSelection(
        int oneBasedRecordOrdinal,
        OriginalMapTraversalArea area)
    {
        if (oneBasedRecordOrdinal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(oneBasedRecordOrdinal));
        }

        OneBasedRecordOrdinal = oneBasedRecordOrdinal;
        Area = area ?? throw new ArgumentNullException(nameof(area));
    }

    public int OneBasedRecordOrdinal { get; }

    public OriginalMapTraversalArea Area { get; }
}

public enum OriginalMapTraversalOutcome
{
    Moved,
    BlockedByBoundary,
    BlockedOutsideActiveArea,
    BlockedByCollision,
}

public sealed record OriginalMapTraversalResult
{
    public OriginalMapTraversalResult(
        MapPosition source,
        MapPosition position,
        ExplorationDirection direction,
        OriginalMapTraversalOutcome outcome,
        ushort sourceWord,
        ushort? destinationWord)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        if (outcome == OriginalMapTraversalOutcome.Moved && position == source)
        {
            throw new ArgumentException("A moved traversal result requires a new position.");
        }

        if (outcome != OriginalMapTraversalOutcome.Moved && position != source)
        {
            throw new ArgumentException("A blocked traversal result must retain its source position.");
        }

        Direction = direction;
        Outcome = outcome;
        SourceWord = sourceWord;
        DestinationWord = destinationWord;
    }

    public MapPosition Source { get; }

    public MapPosition Position { get; }

    public ExplorationDirection Direction { get; }

    public OriginalMapTraversalOutcome Outcome { get; }

    public ushort SourceWord { get; }

    public ushort? DestinationWord { get; }
}

public sealed class OriginalMapTraversal
{
    public const ushort LayoutBlockIndexMask = 0x03FF;
    public const ushort LayoutFlagsMask = 0xFC00;
    public const ushort CollisionMask = 0xC000;
    public const ushort RightStairMask = 0x8000;
    public const ushort LeftStairMask = 0x4000;

    private static readonly ReadOnlyCollection<int> ReadOnlyStairWordDeltas =
        Array.AsReadOnly(new[] { -63, 63, 65, -65 });

    private readonly ReadOnlyCollection<OriginalMapTraversalArea> _activeAreas;

    public OriginalMapTraversal(IEnumerable<OriginalMapTraversalArea> activeAreas)
    {
        ArgumentNullException.ThrowIfNull(activeAreas);
        List<OriginalMapTraversalArea> copiedAreas = [];
        HashSet<OriginalMapTraversalArea> uniqueAreas = [];
        foreach (OriginalMapTraversalArea area in activeAreas)
        {
            OriginalMapTraversalArea admitted = area ?? throw new ArgumentException(
                "Original traversal areas cannot contain null values.",
                nameof(activeAreas));
            if (!uniqueAreas.Add(admitted))
            {
                throw new ArgumentException(
                    "Original traversal areas cannot contain duplicate bounds.",
                    nameof(activeAreas));
            }

            copiedAreas.Add(admitted);
        }

        if (copiedAreas.Count == 0)
        {
            throw new ArgumentException(
                "Original traversal requires at least one exact active area.",
                nameof(activeAreas));
        }

        _activeAreas = copiedAreas.AsReadOnly();
    }

    public IReadOnlyList<OriginalMapTraversalArea> ActiveAreas => _activeAreas;

    public static IReadOnlyList<int> StairWordDeltas => ReadOnlyStairWordDeltas;

    public bool IsWithinActiveArea(MapPosition position)
    {
        return SelectActiveArea(position) is not null;
    }

    public OriginalMapTraversalAreaSelection? SelectActiveArea(MapPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        for (int index = 0; index < _activeAreas.Count; index++)
        {
            OriginalMapTraversalArea area = _activeAreas[index];
            if (area.Contains(position))
            {
                return new OriginalMapTraversalAreaSelection(index + 1, area);
            }
        }

        return null;
    }

    public static bool IsBlocked(WorkingMapLayout layout, MapPosition position)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(position);
        return (layout[position.X, position.Y] & CollisionMask) == CollisionMask;
    }

    public OriginalMapTraversalResult TryMove(
        WorkingMapLayout layout,
        MapPosition position,
        ExplorationDirection direction)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(position);
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        ushort sourceWord = layout[position.X, position.Y];
        MapPosition? destination = ResolveCandidateTarget(layout, position, direction);
        if (destination is null)
        {
            return Blocked(
                position,
                direction,
                OriginalMapTraversalOutcome.BlockedByBoundary,
                sourceWord,
                destinationWord: null);
        }

        ushort destinationWord = layout[destination.X, destination.Y];
        if (!IsWithinActiveArea(destination))
        {
            return Blocked(
                position,
                direction,
                OriginalMapTraversalOutcome.BlockedOutsideActiveArea,
                sourceWord,
                destinationWord);
        }

        if ((destinationWord & CollisionMask) == CollisionMask)
        {
            return Blocked(
                position,
                direction,
                OriginalMapTraversalOutcome.BlockedByCollision,
                sourceWord,
                destinationWord);
        }

        return new OriginalMapTraversalResult(
            position,
            destination,
            direction,
            OriginalMapTraversalOutcome.Moved,
            sourceWord,
            destinationWord);
    }

    public MapPosition? ResolveCandidateTarget(
        WorkingMapLayout layout,
        MapPosition position,
        ExplorationDirection direction)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(position);
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        ushort sourceWord = layout[position.X, position.Y];
        (int deltaX, int deltaY) = DirectionDelta(direction);
        ushort sourceCollision = (ushort)(sourceWord & CollisionMask);
        if (direction is ExplorationDirection.East or ExplorationDirection.West &&
            sourceCollision is RightStairMask or LeftStairMask)
        {
            (int stairX, int stairY) = ResolveStairCandidate(
                position,
                direction,
                sourceCollision);
            if (IsWithinLayout(stairX, stairY) &&
                (layout[stairX, stairY] & CollisionMask) == sourceCollision)
            {
                deltaY = stairY - position.Y;
            }
        }

        int destinationX = position.X + deltaX;
        int destinationY = position.Y + deltaY;
        if (!IsWithinLayout(destinationX, destinationY))
        {
            return null;
        }

        return new MapPosition(destinationX, destinationY);
    }

    private static OriginalMapTraversalResult Blocked(
        MapPosition position,
        ExplorationDirection direction,
        OriginalMapTraversalOutcome outcome,
        ushort sourceWord,
        ushort? destinationWord) =>
        new(position, position, direction, outcome, sourceWord, destinationWord);

    private static (int X, int Y) ResolveStairCandidate(
        MapPosition position,
        ExplorationDirection direction,
        ushort sourceCollision)
    {
        int offsetIndex = (sourceCollision, direction) switch
        {
            (RightStairMask, ExplorationDirection.East) => 0,
            (RightStairMask, ExplorationDirection.West) => 1,
            (LeftStairMask, ExplorationDirection.East) => 2,
            (LeftStairMask, ExplorationDirection.West) => 3,
            _ => throw new InvalidOperationException("Unsupported original stair transition."),
        };
        int candidateX = position.X + (direction == ExplorationDirection.East ? 1 : -1);
        int candidateLinear =
            (position.Y * WorkingMapLayout.ColumnCount) + position.X +
            ReadOnlyStairWordDeltas[offsetIndex];
        int candidateY = candidateX is >= 0 and < WorkingMapLayout.ColumnCount
            ? (candidateLinear - candidateX) / WorkingMapLayout.ColumnCount
            : position.Y;
        return (candidateX, candidateY);
    }

    private static (int X, int Y) DirectionDelta(ExplorationDirection direction) =>
        direction switch
        {
            ExplorationDirection.North => (0, -1),
            ExplorationDirection.East => (1, 0),
            ExplorationDirection.South => (0, 1),
            ExplorationDirection.West => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

    private static bool IsWithinLayout(int x, int y) =>
        x >= 0 &&
        x < WorkingMapLayout.ColumnCount &&
        y >= 0 &&
        y < WorkingMapLayout.RowCount;
}

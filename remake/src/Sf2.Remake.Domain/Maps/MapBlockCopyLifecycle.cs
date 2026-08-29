using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public abstract class MapBlockRegionMutation
{
    private protected MapBlockRegionMutation()
    {
    }

    internal abstract int DestinationX { get; }

    internal abstract int DestinationY { get; }

    internal abstract int Width { get; }

    internal abstract int Height { get; }

    internal abstract WorkingMapLayout Apply(WorkingMapLayout layout);

    public static MapBlockRegionMutation CopyFrom(WorkingMapBlockCopy copy) =>
        new CopyRegionMutation(copy);

    public static MapBlockRegionMutation Clear(
        int destinationX,
        int destinationY,
        int width,
        int height) =>
        new ClearRegionMutation(destinationX, destinationY, width, height);

    private sealed class CopyRegionMutation : MapBlockRegionMutation
    {
        public CopyRegionMutation(WorkingMapBlockCopy copy)
        {
            Copy = copy ?? throw new ArgumentNullException(nameof(copy));
        }

        private WorkingMapBlockCopy Copy { get; }

        internal override int DestinationX => Copy.DestinationX;

        internal override int DestinationY => Copy.DestinationY;

        internal override int Width => Copy.Width;

        internal override int Height => Copy.Height;

        internal override WorkingMapLayout Apply(WorkingMapLayout layout) =>
            layout.ApplyBlockCopy(Copy);
    }

    private sealed class ClearRegionMutation : MapBlockRegionMutation
    {
        public ClearRegionMutation(
            int destinationX,
            int destinationY,
            int width,
            int height)
        {
            MapBlockRegionBounds.Validate(destinationX, destinationY, width, height);
            DestinationX = destinationX;
            DestinationY = destinationY;
            Width = width;
            Height = height;
        }

        internal override int DestinationX { get; }

        internal override int DestinationY { get; }

        internal override int Width { get; }

        internal override int Height { get; }

        internal override WorkingMapLayout Apply(WorkingMapLayout layout)
        {
            ushort[] words = [.. layout.Words];
            MapBlockRegionBounds.Write(words, DestinationX, DestinationY, Width, Height, []);
            return new WorkingMapLayout(words);
        }
    }
}

public abstract class MapBlockCopyLifecycleState
{
    private protected MapBlockCopyLifecycleState()
    {
    }

    public static MapBlockCopyLifecycleInactiveState Inactive { get; } = new();
}

public sealed class MapBlockCopyLifecycleInactiveState : MapBlockCopyLifecycleState
{
    internal MapBlockCopyLifecycleInactiveState()
    {
    }
}

public sealed class MapBlockCopyLifecycleActiveState : MapBlockCopyLifecycleState
{
    private readonly ReadOnlyCollection<ushort> _savedWords;

    public MapBlockCopyLifecycleActiveState(
        int recordOrdinal,
        int destinationX,
        int destinationY,
        int width,
        int height,
        IEnumerable<ushort> savedWords)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(recordOrdinal, 1);
        int expectedWordCount =
            MapBlockRegionBounds.Validate(destinationX, destinationY, width, height);
        ArgumentNullException.ThrowIfNull(savedWords);
        ushort[] copied = [.. savedWords.Take(expectedWordCount + 1)];
        if (copied.Length != expectedWordCount)
        {
            throw new ArgumentException(
                $"An active block-copy snapshot must contain exactly {expectedWordCount} words.",
                nameof(savedWords));
        }

        RecordOrdinal = recordOrdinal;
        DestinationX = destinationX;
        DestinationY = destinationY;
        Width = width;
        Height = height;
        _savedWords = Array.AsReadOnly(copied);
    }

    public int RecordOrdinal { get; }

    public int DestinationX { get; }

    public int DestinationY { get; }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<ushort> SavedWords => _savedWords;
}

public sealed class MapBlockCopyLifecycleResult
{
    private readonly ReadOnlyCollection<MapViewUpdateChannel> _updateMarks;

    internal MapBlockCopyLifecycleResult(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState,
        IEnumerable<MapViewUpdateChannel> updateMarks)
    {
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        LifecycleState =
            lifecycleState ?? throw new ArgumentNullException(nameof(lifecycleState));
        UpdateState = updateState ?? throw new ArgumentNullException(nameof(updateState));
        ArgumentNullException.ThrowIfNull(updateMarks);
        _updateMarks = updateMarks.ToList().AsReadOnly();
    }

    public WorkingMapLayout Layout { get; }

    public MapBlockCopyLifecycleState LifecycleState { get; }

    public MapViewUpdateState UpdateState { get; }

    public IReadOnlyList<MapViewUpdateChannel> UpdateMarks => _updateMarks;
}

public static class MapBlockCopyLifecycleReducer
{
    private static readonly MapViewUpdateChannel[] Channel0Mark =
    [
        MapViewUpdateChannel.Channel0,
    ];

    public static MapBlockCopyLifecycleResult Activate(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState,
        int recordOrdinal,
        MapBlockRegionMutation mutation)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(lifecycleState);
        ArgumentNullException.ThrowIfNull(updateState);
        ArgumentOutOfRangeException.ThrowIfLessThan(recordOrdinal, 1);
        ArgumentNullException.ThrowIfNull(mutation);

        if (lifecycleState is MapBlockCopyLifecycleActiveState)
        {
            return Unchanged(layout, lifecycleState, updateState);
        }

        EnsureInactive(lifecycleState);
        ushort[] savedWords = MapBlockRegionBounds.Read(
            layout,
            mutation.DestinationX,
            mutation.DestinationY,
            mutation.Width,
            mutation.Height);
        WorkingMapLayout mutatedLayout = mutation.Apply(layout);
        MapBlockCopyLifecycleActiveState activeState = new(
            recordOrdinal,
            mutation.DestinationX,
            mutation.DestinationY,
            mutation.Width,
            mutation.Height,
            savedWords);

        return Changed(mutatedLayout, activeState, updateState);
    }

    public static MapBlockCopyLifecycleResult Restore(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(lifecycleState);
        ArgumentNullException.ThrowIfNull(updateState);

        if (lifecycleState is MapBlockCopyLifecycleInactiveState)
        {
            return Unchanged(layout, lifecycleState, updateState);
        }

        if (lifecycleState is not MapBlockCopyLifecycleActiveState activeState)
        {
            throw new ArgumentOutOfRangeException(nameof(lifecycleState));
        }

        ushort[] words = [.. layout.Words];
        MapBlockRegionBounds.Write(
            words,
            activeState.DestinationX,
            activeState.DestinationY,
            activeState.Width,
            activeState.Height,
            activeState.SavedWords);
        return Changed(
            new WorkingMapLayout(words),
            MapBlockCopyLifecycleState.Inactive,
            updateState);
    }

    private static void EnsureInactive(MapBlockCopyLifecycleState lifecycleState)
    {
        if (lifecycleState is not MapBlockCopyLifecycleInactiveState)
        {
            throw new ArgumentOutOfRangeException(nameof(lifecycleState));
        }
    }

    private static MapBlockCopyLifecycleResult Changed(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState) =>
        new(
            layout,
            lifecycleState,
            new MapViewUpdateState(
                Channel0Requested: true,
                Channel1Requested: updateState.Channel1Requested),
            Channel0Mark);

    private static MapBlockCopyLifecycleResult Unchanged(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState) =>
        new(layout, lifecycleState, updateState, []);
}

internal static class MapBlockRegionBounds
{
    public static int Validate(int x, int y, int width, int height)
    {
        ValidateCoordinate(x, nameof(x));
        ValidateCoordinate(y, nameof(y));
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        long firstIndex = ((long)y * WorkingMapLayout.ColumnCount) + x;
        long finalRowStart =
            firstIndex + (((long)height - 1) * WorkingMapLayout.ColumnCount);
        long finalIndex = finalRowStart + width - 1;
        if (finalIndex >= WorkingMapLayout.WordCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                "The block region exceeds the working layout.");
        }

        long wordCount = (long)width * height;
        if (wordCount > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        return (int)wordCount;
    }

    public static ushort[] Read(
        WorkingMapLayout layout,
        int x,
        int y,
        int width,
        int height)
    {
        int wordCount = Validate(x, y, width, height);
        ushort[] words = new ushort[wordCount];
        int index = 0;
        int rowStart = (y * WorkingMapLayout.ColumnCount) + x;
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                words[index++] = layout.GetWord(rowStart + column);
            }

            rowStart += WorkingMapLayout.ColumnCount;
        }

        return words;
    }

    public static void Write(
        ushort[] layoutWords,
        int x,
        int y,
        int width,
        int height,
        IReadOnlyList<ushort> sourceWords)
    {
        int wordCount = Validate(x, y, width, height);
        if (sourceWords.Count != 0 && sourceWords.Count != wordCount)
        {
            throw new ArgumentException(
                $"The block-region source must contain exactly {wordCount} words.",
                nameof(sourceWords));
        }

        int sourceIndex = 0;
        int rowStart = (y * WorkingMapLayout.ColumnCount) + x;
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                layoutWords[rowStart + column] =
                    sourceWords.Count == 0 ? (ushort)0 : sourceWords[sourceIndex++];
            }

            rowStart += WorkingMapLayout.ColumnCount;
        }
    }

    private static void ValidateCoordinate(int value, string parameterName)
    {
        if (value < 0 || value >= WorkingMapLayout.ColumnCount)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}

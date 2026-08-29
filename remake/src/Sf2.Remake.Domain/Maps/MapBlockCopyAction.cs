using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public sealed record MapCellCoordinate
{
    public MapCellCoordinate(int x, int y)
    {
        Validate(x, nameof(x));
        Validate(y, nameof(y));
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }

    private static void Validate(int value, string parameterName)
    {
        if (value < 0 || value >= WorkingMapLayout.ColumnCount)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}

public sealed record MapBlockCopyActionRecord
{
    public MapBlockCopyActionRecord(
        MapCellCoordinate trigger,
        MapBlockRegionMutation mutation)
    {
        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        Mutation = mutation ?? throw new ArgumentNullException(nameof(mutation));
    }

    public MapCellCoordinate Trigger { get; }

    public MapBlockRegionMutation Mutation { get; }
}

public sealed class MapBlockCopyActionTable
{
    private readonly ReadOnlyCollection<MapBlockCopyActionRecord> _records;

    public MapBlockCopyActionTable(IEnumerable<MapBlockCopyActionRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        List<MapBlockCopyActionRecord> copied = records.ToList();
        if (copied.Any(record => record is null))
        {
            throw new ArgumentException("Action records cannot contain null entries.", nameof(records));
        }

        _records = copied.AsReadOnly();
    }

    public IReadOnlyList<MapBlockCopyActionRecord> Records => _records;
}

public enum MapBlockCopyActionOutcome
{
    FadingSkipped,
    Neutral,
    ShowBusy,
    ShowNoMatch,
    Activated,
    RestoreInactive,
    Restored,
}

public sealed class MapBlockCopyActionResult
{
    private readonly ReadOnlyCollection<MapViewUpdateChannel> _updateMarks;

    internal MapBlockCopyActionResult(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState,
        IEnumerable<MapViewUpdateChannel> updateMarks,
        MapBlockCopyActionOutcome outcome)
    {
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        LifecycleState =
            lifecycleState ?? throw new ArgumentNullException(nameof(lifecycleState));
        UpdateState = updateState ?? throw new ArgumentNullException(nameof(updateState));
        ArgumentNullException.ThrowIfNull(updateMarks);
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        _updateMarks = updateMarks.ToList().AsReadOnly();
        Outcome = outcome;
    }

    public WorkingMapLayout Layout { get; }

    public MapBlockCopyLifecycleState LifecycleState { get; }

    public MapViewUpdateState UpdateState { get; }

    public IReadOnlyList<MapViewUpdateChannel> UpdateMarks => _updateMarks;

    public MapBlockCopyActionOutcome Outcome { get; }
}

public static class MapBlockCopyActionReducer
{
    private const ushort BlockFlagMask = 0x3C00;
    private const ushort ShowFlag = 0x0800;
    private const ushort HideFlag = 0x0C00;

    public static MapBlockCopyActionResult Apply(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState,
        MapCellCoordinate coordinate,
        bool isFading,
        MapBlockCopyActionTable actionTable)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(lifecycleState);
        ArgumentNullException.ThrowIfNull(updateState);
        ArgumentNullException.ThrowIfNull(coordinate);
        ArgumentNullException.ThrowIfNull(actionTable);

        if (isFading)
        {
            return Unchanged(
                layout,
                lifecycleState,
                updateState,
                MapBlockCopyActionOutcome.FadingSkipped);
        }

        ushort blockFlag = (ushort)(layout[coordinate.X, coordinate.Y] & BlockFlagMask);
        return blockFlag switch
        {
            ShowFlag => ApplyShow(
                layout,
                lifecycleState,
                updateState,
                coordinate,
                actionTable),
            HideFlag => ApplyHide(layout, lifecycleState, updateState),
            _ => Unchanged(
                layout,
                lifecycleState,
                updateState,
                MapBlockCopyActionOutcome.Neutral),
        };
    }

    private static MapBlockCopyActionResult ApplyShow(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState,
        MapCellCoordinate coordinate,
        MapBlockCopyActionTable actionTable)
    {
        if (lifecycleState is MapBlockCopyLifecycleActiveState)
        {
            return Unchanged(
                layout,
                lifecycleState,
                updateState,
                MapBlockCopyActionOutcome.ShowBusy);
        }

        for (int index = 0; index < actionTable.Records.Count; index++)
        {
            MapBlockCopyActionRecord record = actionTable.Records[index];
            if (record.Trigger != coordinate)
            {
                continue;
            }

            MapBlockCopyLifecycleResult lifecycleResult =
                MapBlockCopyLifecycleReducer.Activate(
                    layout,
                    lifecycleState,
                    updateState,
                    recordOrdinal: index + 1,
                    record.Mutation);
            return FromLifecycle(lifecycleResult, MapBlockCopyActionOutcome.Activated);
        }

        return Unchanged(
            layout,
            lifecycleState,
            updateState,
            MapBlockCopyActionOutcome.ShowNoMatch);
    }

    private static MapBlockCopyActionResult ApplyHide(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState)
    {
        bool wasActive = lifecycleState is MapBlockCopyLifecycleActiveState;
        MapBlockCopyLifecycleResult lifecycleResult = MapBlockCopyLifecycleReducer.Restore(
            layout,
            lifecycleState,
            updateState);
        return FromLifecycle(
            lifecycleResult,
            wasActive
                ? MapBlockCopyActionOutcome.Restored
                : MapBlockCopyActionOutcome.RestoreInactive);
    }

    private static MapBlockCopyActionResult FromLifecycle(
        MapBlockCopyLifecycleResult lifecycleResult,
        MapBlockCopyActionOutcome outcome) =>
        new(
            lifecycleResult.Layout,
            lifecycleResult.LifecycleState,
            lifecycleResult.UpdateState,
            lifecycleResult.UpdateMarks,
            outcome);

    private static MapBlockCopyActionResult Unchanged(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState,
        MapBlockCopyActionOutcome outcome) =>
        new(layout, lifecycleState, updateState, [], outcome);
}

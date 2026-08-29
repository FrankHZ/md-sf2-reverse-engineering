using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class MapBlockCopyActionTests
{
    [Fact]
    public void FadingSkipsShowActionWithoutChangingAnyState()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(2, 3), 0x0800), (0, 77));
        MapViewUpdateState updates = State(false, true);
        MapBlockCopyActionTable table = Table(Record(2, 3, CopyMutation(0, 0, 10, 0)));

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            updates,
            Cell(2, 3),
            isFading: true,
            table);

        AssertUnchanged(result, layout, Inactive, updates);
        Assert.Equal(MapBlockCopyActionOutcome.FadingSkipped, result.Outcome);
    }

    [Fact]
    public void NeutralBlockFlagLeavesEveryInputUnchanged()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(2, 3), 0x0400), (0, 77));
        MapViewUpdateState updates = State(true, false);

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            updates,
            Cell(2, 3),
            isFading: false,
            Table(Record(2, 3, CopyMutation(0, 0, 10, 0))));

        AssertUnchanged(result, layout, Inactive, updates);
        Assert.Equal(MapBlockCopyActionOutcome.Neutral, result.Outcome);
    }

    [Fact]
    public void ShowWithPositiveRecordCopiesAndActivatesExactOrdinal()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(2, 3), 0x0800), (0, 77), (10, 11));

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            State(false, false),
            Cell(2, 3),
            isFading: false,
            Table(Record(2, 3, CopyMutation(0, 0, 10, 0))));

        MapBlockCopyLifecycleActiveState active = Active(result);
        Assert.Equal(MapBlockCopyActionOutcome.Activated, result.Outcome);
        Assert.Equal(1, active.RecordOrdinal);
        Assert.Equal([11], active.SavedWords);
        Assert.Equal(77, result.Layout.GetWord(10));
        AssertChannel0Only(result);
    }

    [Fact]
    public void ShowWithClearRecordSnapshotsAndClearsDestination()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(4, 5), 0x0800), (20, 99));

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            State(false, true),
            Cell(4, 5),
            isFading: false,
            Table(Record(4, 5, MapBlockRegionMutation.Clear(20, 0, 1, 1))));

        Assert.Equal(MapBlockCopyActionOutcome.Activated, result.Outcome);
        Assert.Equal([99], Active(result).SavedWords);
        Assert.Equal(0, result.Layout.GetWord(20));
        Assert.True(result.UpdateState.Channel1Requested);
        AssertChannel0Only(result);
    }

    [Fact]
    public void HideWithActiveSnapshotRestoresAndBecomesInactive()
    {
        WorkingMapLayout original = LayoutWith((CellIndex(1, 1), 0x0800), (0, 55), (10, 22));
        MapBlockCopyActionResult shown = Apply(
            original,
            Inactive,
            State(false, true),
            Cell(1, 1),
            isFading: false,
            Table(Record(1, 1, CopyMutation(0, 0, 10, 0))));
        WorkingMapLayout hideLayout = WithWord(shown.Layout, CellIndex(1, 1), 0x0C00);

        MapBlockCopyActionResult hidden = Apply(
            hideLayout,
            shown.LifecycleState,
            shown.UpdateState,
            Cell(1, 1),
            isFading: false,
            Table());

        Assert.Equal(MapBlockCopyActionOutcome.Restored, hidden.Outcome);
        Assert.Same(Inactive, hidden.LifecycleState);
        Assert.Equal(22, hidden.Layout.GetWord(10));
        Assert.True(hidden.UpdateState.Channel1Requested);
        AssertChannel0Only(hidden);
    }

    [Fact]
    public void HideWhileInactiveIsNoOp()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(1, 1), 0x0C00));
        MapViewUpdateState updates = State(false, true);

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            updates,
            Cell(1, 1),
            isFading: false,
            Table());

        AssertUnchanged(result, layout, Inactive, updates);
        Assert.Equal(MapBlockCopyActionOutcome.RestoreInactive, result.Outcome);
    }

    [Fact]
    public void ShowWhileActiveIsBusyAndDoesNotApplyMatchingRecord()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(1, 1), 0x0800), (20, 44));
        MapBlockCopyLifecycleActiveState active = ActiveState(3, 10, 0, [5]);
        MapViewUpdateState updates = State(false, true);

        MapBlockCopyActionResult result = Apply(
            layout,
            active,
            updates,
            Cell(1, 1),
            isFading: false,
            Table(Record(1, 1, MapBlockRegionMutation.Clear(20, 0, 1, 1))));

        AssertUnchanged(result, layout, active, updates);
        Assert.Equal(MapBlockCopyActionOutcome.ShowBusy, result.Outcome);
        Assert.Equal(44, result.Layout.GetWord(20));
    }

    [Fact]
    public void ShowWithoutMatchingRecordIsNoOp()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(1, 1), 0x0800));
        MapViewUpdateState updates = State(false, false);

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            updates,
            Cell(1, 1),
            isFading: false,
            Table(Record(2, 2, MapBlockRegionMutation.Clear(20, 0, 1, 1))));

        AssertUnchanged(result, layout, Inactive, updates);
        Assert.Equal(MapBlockCopyActionOutcome.ShowNoMatch, result.Outcome);
    }

    [Fact]
    public void FirstMatchingRecordWins()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(1, 1), 0x0800), (20, 8), (21, 9));

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            State(false, false),
            Cell(1, 1),
            isFading: false,
            Table(
                Record(1, 1, MapBlockRegionMutation.Clear(20, 0, 1, 1)),
                Record(1, 1, MapBlockRegionMutation.Clear(21, 0, 1, 1))));

        Assert.Equal(0, result.Layout.GetWord(20));
        Assert.Equal(9, result.Layout.GetWord(21));
        Assert.Equal(1, Active(result).RecordOrdinal);
    }

    [Fact]
    public void PrecedingNonmatchesContributeToOneBasedOrdinal()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(3, 4), 0x0800), (20, 8));

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            State(false, false),
            Cell(3, 4),
            isFading: false,
            Table(
                Record(1, 4, MapBlockRegionMutation.Clear(21, 0, 1, 1)),
                Record(3, 2, MapBlockRegionMutation.Clear(22, 0, 1, 1)),
                Record(3, 4, MapBlockRegionMutation.Clear(20, 0, 1, 1))));

        Assert.Equal(3, Active(result).RecordOrdinal);
        Assert.Equal(0, result.Layout.GetWord(20));
    }

    [Theory]
    [InlineData(3, 8)]
    [InlineData(9, 4)]
    public void PartialCoordinateMatchDoesNotSelectRecord(int triggerX, int triggerY)
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(3, 4), 0x0800));

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            State(false, false),
            Cell(3, 4),
            isFading: false,
            Table(Record(triggerX, triggerY, MapBlockRegionMutation.Clear(20, 0, 1, 1))));

        Assert.Equal(MapBlockCopyActionOutcome.ShowNoMatch, result.Outcome);
    }

    [Fact]
    public void BitsOutsideMaskDoNotChangeShowClassification()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(1, 2), 0xC803), (20, 7));

        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            State(false, false),
            Cell(1, 2),
            isFading: false,
            Table(Record(1, 2, MapBlockRegionMutation.Clear(20, 0, 1, 1))));

        Assert.Equal(MapBlockCopyActionOutcome.Activated, result.Outcome);
        Assert.Equal(0, result.Layout.GetWord(20));
    }

    [Fact]
    public void ActionTableDefensivelyCopiesAndExposesReadOnlyRecords()
    {
        MapBlockCopyActionRecord first = Record(
            1,
            1,
            MapBlockRegionMutation.Clear(20, 0, 1, 1));
        List<MapBlockCopyActionRecord> callerRecords = [first];
        MapBlockCopyActionTable table = new(callerRecords);

        callerRecords.Clear();
        IList<MapBlockCopyActionRecord> exposed =
            Assert.IsAssignableFrom<IList<MapBlockCopyActionRecord>>(table.Records);

        Assert.Equal([first], table.Records);
        Assert.Throws<NotSupportedException>(() => exposed.Add(first));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(64, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 64)]
    public void CoordinateOutsideWorkingLayoutFailsClosed(int x, int y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Cell(x, y));
    }

    [Fact]
    public void NullRecordAndNullMembersFailClosed()
    {
        Assert.Throws<ArgumentNullException>(
            () => new MapBlockCopyActionRecord(null!, MapBlockRegionMutation.Clear(0, 0, 1, 1)));
        Assert.Throws<ArgumentNullException>(
            () => new MapBlockCopyActionRecord(Cell(0, 0), null!));
        Assert.Throws<ArgumentException>(
            () => new MapBlockCopyActionTable([null!]));
    }

    [Fact]
    public void InvalidSelectedMutationLeavesAllInputsUnchanged()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(1, 1), 0x0800));
        ushort[] before = [.. layout.Words];
        MapViewUpdateState updates = State(false, true);
        MapBlockRegionMutation invalid = MapBlockRegionMutation.CopyFrom(
            Copy(63, 63, 0, 0, width: 2));

        Assert.Throws<ArgumentOutOfRangeException>(() => Apply(
            layout,
            Inactive,
            updates,
            Cell(1, 1),
            isFading: false,
            Table(Record(1, 1, invalid))));
        Assert.Equal(before, layout.Words.ToArray());
        Assert.False(updates.Channel0Requested);
        Assert.True(updates.Channel1Requested);
    }

    [Fact]
    public void ResultMarksAreReadOnly()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(1, 1), 0x0800));
        MapBlockCopyActionResult result = Apply(
            layout,
            Inactive,
            State(false, false),
            Cell(1, 1),
            isFading: false,
            Table(Record(1, 1, MapBlockRegionMutation.Clear(20, 0, 1, 1))));
        IList<MapViewUpdateChannel> marks = Assert.IsAssignableFrom<IList<MapViewUpdateChannel>>(
            result.UpdateMarks);

        Assert.Throws<NotSupportedException>(() => marks.Add(MapViewUpdateChannel.Channel1));
        AssertChannel0Only(result);
    }

    [Fact]
    public void IdenticalInputsProduceIdenticalResults()
    {
        WorkingMapLayout layout = LayoutWith((CellIndex(5, 6), 0x0800), (20, 8));
        MapViewUpdateState updates = State(true, false);
        MapCellCoordinate cell = Cell(5, 6);
        MapBlockCopyActionTable table =
            Table(Record(5, 6, MapBlockRegionMutation.Clear(20, 0, 1, 1)));

        MapBlockCopyActionResult first = Apply(
            layout,
            Inactive,
            updates,
            cell,
            isFading: false,
            table);
        MapBlockCopyActionResult second = Apply(
            layout,
            Inactive,
            updates,
            cell,
            isFading: false,
            table);

        Assert.Equal(first.Outcome, second.Outcome);
        Assert.Equal(first.Layout.Words.ToArray(), second.Layout.Words.ToArray());
        Assert.Equal(Active(first).SavedWords, Active(second).SavedWords);
        Assert.Equal(first.UpdateState, second.UpdateState);
        Assert.Equal(first.UpdateMarks, second.UpdateMarks);
    }

    private static MapBlockCopyActionResult Apply(
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState,
        MapCellCoordinate coordinate,
        bool isFading,
        MapBlockCopyActionTable table) =>
        MapBlockCopyActionReducer.Apply(
            layout,
            lifecycleState,
            updateState,
            coordinate,
            isFading,
            table);

    private static void AssertUnchanged(
        MapBlockCopyActionResult result,
        WorkingMapLayout layout,
        MapBlockCopyLifecycleState lifecycleState,
        MapViewUpdateState updateState)
    {
        Assert.Same(layout, result.Layout);
        Assert.Same(lifecycleState, result.LifecycleState);
        Assert.Same(updateState, result.UpdateState);
        Assert.Empty(result.UpdateMarks);
    }

    private static void AssertChannel0Only(MapBlockCopyActionResult result)
    {
        Assert.True(result.UpdateState.Channel0Requested);
        Assert.Equal([MapViewUpdateChannel.Channel0], result.UpdateMarks);
    }

    private static MapBlockCopyLifecycleActiveState Active(MapBlockCopyActionResult result) =>
        Assert.IsType<MapBlockCopyLifecycleActiveState>(result.LifecycleState);

    private static MapBlockCopyLifecycleActiveState ActiveState(
        int ordinal,
        int destinationX,
        int destinationY,
        IEnumerable<ushort> savedWords) =>
        new(ordinal, destinationX, destinationY, 1, 1, savedWords);

    private static MapBlockCopyActionRecord Record(
        int x,
        int y,
        MapBlockRegionMutation mutation) =>
        new(Cell(x, y), mutation);

    private static MapBlockCopyActionTable Table(params MapBlockCopyActionRecord[] records) =>
        new(records);

    private static MapCellCoordinate Cell(int x, int y) => new(x, y);

    private static int CellIndex(int x, int y) => (y * WorkingMapLayout.ColumnCount) + x;

    private static MapBlockRegionMutation CopyMutation(
        int sourceX,
        int sourceY,
        int destinationX,
        int destinationY) =>
        MapBlockRegionMutation.CopyFrom(
            Copy(sourceX, sourceY, destinationX, destinationY));

    private static WorkingMapBlockCopy Copy(
        int sourceX,
        int sourceY,
        int destinationX,
        int destinationY,
        int width = 1,
        int height = 1) =>
        new(sourceX, sourceY, destinationX, destinationY, width, height);

    private static MapViewUpdateState State(bool channel0, bool channel1) =>
        new(channel0, channel1);

    private static WorkingMapLayout LayoutWith(params (int Index, ushort Value)[] values)
    {
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        foreach ((int index, ushort value) in values)
        {
            words[index] = value;
        }

        return new WorkingMapLayout(words);
    }

    private static WorkingMapLayout WithWord(WorkingMapLayout layout, int index, ushort value)
    {
        ushort[] words = [.. layout.Words];
        words[index] = value;
        return new WorkingMapLayout(words);
    }

    private static MapBlockCopyLifecycleState Inactive =>
        MapBlockCopyLifecycleState.Inactive;
}

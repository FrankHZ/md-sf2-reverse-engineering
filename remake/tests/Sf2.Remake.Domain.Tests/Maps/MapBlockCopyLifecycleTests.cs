using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class MapBlockCopyLifecycleTests
{
    [Fact]
    public void PositiveCopySnapshotsDestinationBeforeApplyingCopy()
    {
        WorkingMapLayout input = LayoutWith((0, 77), (10, 11));

        MapBlockCopyLifecycleResult result = Activate(
            input,
            State(false, false),
            ordinal: 3,
            MapBlockRegionMutation.CopyFrom(Copy(0, 0, 10, 0)));

        MapBlockCopyLifecycleActiveState active = Active(result);
        Assert.Equal(77, result.Layout.GetWord(10));
        Assert.Equal([11], active.SavedWords);
        Assert.Equal(3, active.RecordOrdinal);
        Assert.Equal(
            (10, 0, 1, 1),
            (active.DestinationX, active.DestinationY, active.Width, active.Height));
        Assert.Equal(11, input.GetWord(10));
    }

    [Fact]
    public void ClearSnapshotsDestinationThenWritesOpaqueZeroWords()
    {
        WorkingMapLayout input = LayoutWith((65, 10), (66, 20), (129, 30), (130, 40));

        MapBlockCopyLifecycleResult result = Activate(
            input,
            State(false, true),
            ordinal: 1,
            MapBlockRegionMutation.Clear(1, 1, 2, 2));

        Assert.Equal([10, 20, 30, 40], Active(result).SavedWords);
        Assert.Equal([0, 0], Words(result.Layout, 65, 2));
        Assert.Equal([0, 0], Words(result.Layout, 129, 2));
        Assert.Equal([10, 20], Words(input, 65, 2));
        Assert.Equal([30, 40], Words(input, 129, 2));
    }

    [Fact]
    public void ActiveActivationIsBusySkip()
    {
        WorkingMapLayout layout = LayoutWith((0, 15), (1, 25));
        MapBlockCopyLifecycleActiveState active = ActiveState(4, 1, 0, 1, 1, [25]);
        MapViewUpdateState updateState = State(false, true);

        MapBlockCopyLifecycleResult result = MapBlockCopyLifecycleReducer.Activate(
            layout,
            active,
            updateState,
            recordOrdinal: 7,
            MapBlockRegionMutation.Clear(1, 0, 1, 1));

        Assert.Same(layout, result.Layout);
        Assert.Same(active, result.LifecycleState);
        Assert.Same(updateState, result.UpdateState);
        Assert.Empty(result.UpdateMarks);
        Assert.Equal(25, result.Layout.GetWord(1));
    }

    [Fact]
    public void InactiveRestoreIsNoOp()
    {
        WorkingMapLayout layout = SequentialLayout();
        MapViewUpdateState updateState = State(true, false);

        MapBlockCopyLifecycleResult result = MapBlockCopyLifecycleReducer.Restore(
            layout,
            MapBlockCopyLifecycleState.Inactive,
            updateState);

        Assert.Same(layout, result.Layout);
        Assert.Same(MapBlockCopyLifecycleState.Inactive, result.LifecycleState);
        Assert.Same(updateState, result.UpdateState);
        Assert.Empty(result.UpdateMarks);
    }

    [Fact]
    public void ActiveRestoreReplacesOnlySavedRectangleAndBecomesInactive()
    {
        WorkingMapLayout initial = LayoutWith((0, 90), (10, 11), (11, 12));
        MapBlockCopyLifecycleResult activated = Activate(
            initial,
            State(false, false),
            ordinal: 2,
            MapBlockRegionMutation.CopyFrom(Copy(0, 0, 10, 0, width: 2)));
        WorkingMapLayout externallyChanged = activated.Layout.ApplyBlockCopy(
            Copy(0, 0, 20, 0));

        MapBlockCopyLifecycleResult restored = MapBlockCopyLifecycleReducer.Restore(
            externallyChanged,
            activated.LifecycleState,
            activated.UpdateState);

        Assert.Same(MapBlockCopyLifecycleState.Inactive, restored.LifecycleState);
        Assert.Equal([11, 12], Words(restored.Layout, 10, 2));
        Assert.Equal(90, restored.Layout.GetWord(20));
        Assert.Equal(90, externallyChanged.GetWord(10));
    }

    [Fact]
    public void PositiveCopyPreservesForwardOverlapCascade()
    {
        WorkingMapLayout layout = LayoutWith(
            (0, 10),
            (1, 20),
            (2, 30),
            (3, 40),
            (4, 50));

        MapBlockCopyLifecycleResult result = Activate(
            layout,
            State(false, false),
            ordinal: 1,
            MapBlockRegionMutation.CopyFrom(Copy(0, 0, 1, 0, width: 4)));

        Assert.Equal([10, 10, 10, 10, 10], Words(result.Layout, 0, 5));
        Assert.Equal([20, 30, 40, 50], Active(result).SavedWords);
    }

    [Fact]
    public void MultirowSnapshotAndRestoreUseRowMajorOrder()
    {
        WorkingMapLayout layout = LayoutWith((65, 1), (66, 2), (129, 3), (130, 4));
        MapBlockCopyLifecycleResult activated = Activate(
            layout,
            State(false, false),
            ordinal: 9,
            MapBlockRegionMutation.Clear(1, 1, 2, 2));

        MapBlockCopyLifecycleResult restored = MapBlockCopyLifecycleReducer.Restore(
            activated.Layout,
            activated.LifecycleState,
            activated.UpdateState);

        Assert.Equal([1, 2, 3, 4], Active(activated).SavedWords);
        Assert.Equal([1, 2], Words(restored.Layout, 65, 2));
        Assert.Equal([3, 4], Words(restored.Layout, 129, 2));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(27)]
    public void ActiveStatePreservesExactOneBasedRecordOrdinal(int ordinal)
    {
        MapBlockCopyLifecycleResult result = Activate(
            SequentialLayout(),
            State(false, false),
            ordinal,
            MapBlockRegionMutation.Clear(0, 0, 1, 1));

        Assert.Equal(ordinal, Active(result).RecordOrdinal);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SuccessfulActivationSetsOnlyChannel0WithoutClearingChannel1(
        bool channel0,
        bool channel1)
    {
        MapViewUpdateState input = State(channel0, channel1);

        MapBlockCopyLifecycleResult result = Activate(
            SequentialLayout(),
            input,
            ordinal: 1,
            MapBlockRegionMutation.Clear(0, 0, 1, 1));

        Assert.True(result.UpdateState.Channel0Requested);
        Assert.Equal(channel1, result.UpdateState.Channel1Requested);
        Assert.Equal([MapViewUpdateChannel.Channel0], result.UpdateMarks);
        Assert.Equal(channel0, input.Channel0Requested);
        Assert.Equal(channel1, input.Channel1Requested);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SuccessfulRestoreSetsOnlyChannel0WithoutClearingChannel1(
        bool channel0,
        bool channel1)
    {
        MapViewUpdateState input = State(channel0, channel1);
        MapBlockCopyLifecycleActiveState active = ActiveState(1, 0, 0, 1, 1, [7]);

        MapBlockCopyLifecycleResult result = MapBlockCopyLifecycleReducer.Restore(
            SequentialLayout(),
            active,
            input);

        Assert.True(result.UpdateState.Channel0Requested);
        Assert.Equal(channel1, result.UpdateState.Channel1Requested);
        Assert.Equal([MapViewUpdateChannel.Channel0], result.UpdateMarks);
    }

    [Fact]
    public void ActiveStateDefensivelyCopiesSavedWords()
    {
        ushort[] savedWords = [5, 6];
        MapBlockCopyLifecycleActiveState state = ActiveState(1, 0, 0, 2, 1, savedWords);

        savedWords[0] = 99;
        IList<ushort> exposed = Assert.IsAssignableFrom<IList<ushort>>(state.SavedWords);

        Assert.Equal([5, 6], state.SavedWords);
        Assert.Throws<NotSupportedException>(() => exposed.Add(7));
    }

    [Fact]
    public void ResultUpdateMarksCannotBeModifiedByCaller()
    {
        MapBlockCopyLifecycleResult result = Activate(
            SequentialLayout(),
            State(false, false),
            ordinal: 1,
            MapBlockRegionMutation.Clear(0, 0, 1, 1));
        IList<MapViewUpdateChannel> marks = Assert.IsAssignableFrom<IList<MapViewUpdateChannel>>(
            result.UpdateMarks);

        Assert.Throws<NotSupportedException>(() => marks.Add(MapViewUpdateChannel.Channel1));
        Assert.Equal([MapViewUpdateChannel.Channel0], result.UpdateMarks);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveRecordOrdinalFailsBeforeChangingInputs(int ordinal)
    {
        WorkingMapLayout layout = SequentialLayout();
        MapViewUpdateState updateState = State(false, true);

        Assert.Throws<ArgumentOutOfRangeException>(() => Activate(
            layout,
            updateState,
            ordinal,
            MapBlockRegionMutation.Clear(0, 0, 1, 1)));
        Assert.Equal(0, layout.GetWord(0));
        Assert.False(updateState.Channel0Requested);
        Assert.True(updateState.Channel1Requested);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void ClearRejectsNonPositiveDimensions(int width, int height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MapBlockRegionMutation.Clear(0, 0, width, height));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(64, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 64)]
    public void ClearRejectsCoordinatesOutsideLayout(int x, int y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MapBlockRegionMutation.Clear(x, y, 1, 1));
    }

    [Fact]
    public void ClearRejectsRegionOutsideLayout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MapBlockRegionMutation.Clear(63, 63, 2, 1));
    }

    [Fact]
    public void InvalidCopySourceFailsWithoutChangingInputsOrAddingMarks()
    {
        WorkingMapLayout layout = SequentialLayout();
        ushort[] before = [.. layout.Words];
        MapViewUpdateState updateState = State(false, true);
        MapBlockRegionMutation mutation = MapBlockRegionMutation.CopyFrom(
            Copy(63, 63, 0, 0, width: 2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => Activate(layout, updateState, 1, mutation));
        Assert.Equal(before, layout.Words.ToArray());
        Assert.False(updateState.Channel0Requested);
        Assert.True(updateState.Channel1Requested);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void ActiveStateRejectsWrongSnapshotLength(int length)
    {
        Assert.Throws<ArgumentException>(
            () => ActiveState(1, 0, 0, 1, 1, new ushort[length]));
    }

    [Fact]
    public void IdenticalInputsProduceIdenticalActivateAndRestoreResults()
    {
        WorkingMapLayout layout = SequentialLayout();
        MapViewUpdateState updateState = State(true, false);
        MapBlockRegionMutation mutation = MapBlockRegionMutation.CopyFrom(
            Copy(4, 5, 6, 7, width: 3, height: 2));

        MapBlockCopyLifecycleResult first = Activate(layout, updateState, 4, mutation);
        MapBlockCopyLifecycleResult second = Activate(layout, updateState, 4, mutation);
        MapBlockCopyLifecycleResult firstRestore = MapBlockCopyLifecycleReducer.Restore(
            first.Layout,
            first.LifecycleState,
            first.UpdateState);
        MapBlockCopyLifecycleResult secondRestore = MapBlockCopyLifecycleReducer.Restore(
            second.Layout,
            second.LifecycleState,
            second.UpdateState);

        Assert.Equal(first.Layout.Words.ToArray(), second.Layout.Words.ToArray());
        Assert.Equal(Active(first).SavedWords, Active(second).SavedWords);
        Assert.Equal(first.UpdateState, second.UpdateState);
        Assert.Equal(firstRestore.Layout.Words.ToArray(), secondRestore.Layout.Words.ToArray());
        Assert.Equal(firstRestore.UpdateState, secondRestore.UpdateState);
    }

    private static MapBlockCopyLifecycleResult Activate(
        WorkingMapLayout layout,
        MapViewUpdateState updateState,
        int ordinal,
        MapBlockRegionMutation mutation) =>
        MapBlockCopyLifecycleReducer.Activate(
            layout,
            MapBlockCopyLifecycleState.Inactive,
            updateState,
            ordinal,
            mutation);

    private static MapBlockCopyLifecycleActiveState Active(
        MapBlockCopyLifecycleResult result) =>
        Assert.IsType<MapBlockCopyLifecycleActiveState>(result.LifecycleState);

    private static MapBlockCopyLifecycleActiveState ActiveState(
        int ordinal,
        int x,
        int y,
        int width,
        int height,
        IEnumerable<ushort> savedWords) =>
        new(ordinal, x, y, width, height, savedWords);

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

    private static WorkingMapLayout SequentialLayout() =>
        new(Enumerable.Range(0, WorkingMapLayout.WordCount).Select(index => (ushort)index));

    private static WorkingMapLayout LayoutWith(params (int Index, ushort Value)[] values)
    {
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        foreach ((int index, ushort value) in values)
        {
            words[index] = value;
        }

        return new WorkingMapLayout(words);
    }

    private static ushort[] Words(WorkingMapLayout layout, int start, int count) =>
        Enumerable.Range(start, count).Select(layout.GetWord).ToArray();
}

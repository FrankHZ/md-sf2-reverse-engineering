using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class MapBlockMutationTests
{
    [Fact]
    public void SetBlocksCopiesOnceAndRequestsChannelsInOrder()
    {
        WorkingMapLayout layout = LayoutWith((0, 77));
        MapBlockMutationCommand command = Command(MapBlockMutationKind.SetBlocks, Copy(0, 0, 1, 0));

        MapBlockMutationResult result = MapBlockMutationReducer.Apply(layout, State(false, false), command);

        Assert.Equal(77, result.Layout.GetWord(1));
        Assert.Equal(
            [MapViewUpdateChannel.Channel0, MapViewUpdateChannel.Channel1],
            result.UpdateMarks);
        Assert.True(result.UpdateState.Channel0Requested);
        Assert.True(result.UpdateState.Channel1Requested);
    }

    [Fact]
    public void SetBlocksVarCopiesOnceWithoutRequestingChannels()
    {
        WorkingMapLayout layout = LayoutWith((0, 88));
        MapViewUpdateState state = State(false, true);
        MapBlockMutationCommand command =
            Command(MapBlockMutationKind.SetBlocksVar, Copy(0, 0, 1, 0));

        MapBlockMutationResult result = MapBlockMutationReducer.Apply(layout, state, command);

        Assert.Equal(88, result.Layout.GetWord(1));
        Assert.Empty(result.UpdateMarks);
        Assert.Same(state, result.UpdateState);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SetBlocksSetsWithoutClearingAndAlwaysRecordsBothMarks(
        bool channel0,
        bool channel1)
    {
        MapViewUpdateState initial = State(channel0, channel1);

        MapBlockMutationResult result = MapBlockMutationReducer.Apply(
            SequentialLayout(),
            initial,
            Command(MapBlockMutationKind.SetBlocks, Copy(0, 0, 1, 0)));

        Assert.True(result.UpdateState.Channel0Requested);
        Assert.True(result.UpdateState.Channel1Requested);
        Assert.Equal(
            [MapViewUpdateChannel.Channel0, MapViewUpdateChannel.Channel1],
            result.UpdateMarks);
        Assert.Equal(channel0, initial.Channel0Requested);
        Assert.Equal(channel1, initial.Channel1Requested);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SetBlocksVarPreservesEveryInitialState(bool channel0, bool channel1)
    {
        MapViewUpdateState initial = State(channel0, channel1);

        MapBlockMutationResult result = MapBlockMutationReducer.Apply(
            SequentialLayout(),
            initial,
            Command(MapBlockMutationKind.SetBlocksVar, Copy(0, 0, 1, 0)));

        Assert.Same(initial, result.UpdateState);
        Assert.Empty(result.UpdateMarks);
    }

    [Fact]
    public void CommandDelegatesLegalCrossRowCopy()
    {
        MapBlockMutationResult result = MapBlockMutationReducer.Apply(
            SequentialLayout(),
            State(false, false),
            Command(MapBlockMutationKind.SetBlocks, Copy(62, 0, 62, 1, width: 4)));

        Assert.Equal([62, 63, 64, 65], Words(result.Layout, 126, 4));
    }

    [Fact]
    public void CommandPreservesHorizontalOverlapCascade()
    {
        WorkingMapLayout layout = LayoutWith(
            (0, 10),
            (1, 20),
            (2, 30),
            (3, 40),
            (4, 50));

        MapBlockMutationResult result = MapBlockMutationReducer.Apply(
            layout,
            State(false, false),
            Command(MapBlockMutationKind.SetBlocksVar, Copy(0, 0, 1, 0, width: 4)));

        Assert.Equal([10, 10, 10, 10, 10], Words(result.Layout, 0, 5));
    }

    [Fact]
    public void CommandPreservesVerticalOverlapCascade()
    {
        WorkingMapLayout layout = LayoutWith(
            (0, 10),
            (64, 20),
            (128, 30),
            (192, 40));

        MapBlockMutationResult result = MapBlockMutationReducer.Apply(
            layout,
            State(false, false),
            Command(MapBlockMutationKind.SetBlocks, Copy(0, 0, 0, 1, height: 3)));

        Assert.Equal(10, result.Layout[0, 1]);
        Assert.Equal(10, result.Layout[0, 2]);
        Assert.Equal(10, result.Layout[0, 3]);
    }

    [Fact]
    public void InputsRemainUnchangedAfterSuccessfulCommand()
    {
        WorkingMapLayout layout = LayoutWith((0, 42));
        MapViewUpdateState state = State(false, false);

        MapBlockMutationResult result = MapBlockMutationReducer.Apply(
            layout,
            state,
            Command(MapBlockMutationKind.SetBlocks, Copy(0, 0, 1, 0)));

        Assert.Equal(0, layout.GetWord(1));
        Assert.False(state.Channel0Requested);
        Assert.False(state.Channel1Requested);
        Assert.Equal(42, result.Layout.GetWord(1));
    }

    [Fact]
    public void ConsecutiveCommandsUsePriorResultState()
    {
        WorkingMapLayout layout = LayoutWith((0, 99));
        MapBlockMutationResult first = MapBlockMutationReducer.Apply(
            layout,
            State(false, false),
            Command(MapBlockMutationKind.SetBlocks, Copy(0, 0, 1, 0)));

        MapBlockMutationResult second = MapBlockMutationReducer.Apply(
            first.Layout,
            first.UpdateState,
            Command(MapBlockMutationKind.SetBlocksVar, Copy(1, 0, 2, 0)));

        Assert.Equal(99, second.Layout.GetWord(2));
        Assert.True(second.UpdateState.Channel0Requested);
        Assert.True(second.UpdateState.Channel1Requested);
        Assert.Empty(second.UpdateMarks);
    }

    [Fact]
    public void InvalidCopyFailsBeforeProducingAnyResultOrChangingInputs()
    {
        WorkingMapLayout layout = SequentialLayout();
        ushort[] before = [.. layout.Words];
        MapViewUpdateState state = State(false, true);
        MapBlockMutationCommand command =
            Command(MapBlockMutationKind.SetBlocks, Copy(63, 63, 0, 0, width: 2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => MapBlockMutationReducer.Apply(layout, state, command));
        Assert.Equal(before, layout.Words.ToArray());
        Assert.False(state.Channel0Requested);
        Assert.True(state.Channel1Requested);
    }

    [Fact]
    public void UnknownCommandKindFailsClosed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Command((MapBlockMutationKind)99, Copy(0, 0, 1, 0)));
    }

    [Fact]
    public void UpdateMarksCannotBeModifiedByCaller()
    {
        MapBlockMutationResult result = MapBlockMutationReducer.Apply(
            SequentialLayout(),
            State(false, false),
            Command(MapBlockMutationKind.SetBlocks, Copy(0, 0, 1, 0)));
        IList<MapViewUpdateChannel> marks = Assert.IsAssignableFrom<IList<MapViewUpdateChannel>>(
            result.UpdateMarks);

        Assert.Throws<NotSupportedException>(() => marks.Add(MapViewUpdateChannel.Channel0));
        Assert.Equal(
            [MapViewUpdateChannel.Channel0, MapViewUpdateChannel.Channel1],
            result.UpdateMarks);
    }

    [Fact]
    public void ViewUpdateStateRejectsUnknownChannel()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => State(false, false).IsRequested((MapViewUpdateChannel)99));
    }

    [Fact]
    public void IdenticalInputsProduceIdenticalResults()
    {
        WorkingMapLayout layout = SequentialLayout();
        MapViewUpdateState state = State(true, false);
        MapBlockMutationCommand command =
            Command(MapBlockMutationKind.SetBlocks, Copy(4, 5, 6, 7, width: 3, height: 2));

        MapBlockMutationResult first = MapBlockMutationReducer.Apply(layout, state, command);
        MapBlockMutationResult second = MapBlockMutationReducer.Apply(layout, state, command);

        Assert.Equal(first.Layout.Words.ToArray(), second.Layout.Words.ToArray());
        Assert.Equal(first.UpdateState, second.UpdateState);
        Assert.Equal(first.UpdateMarks, second.UpdateMarks);
    }

    private static MapBlockMutationCommand Command(
        MapBlockMutationKind kind,
        WorkingMapBlockCopy copy) => new(kind, copy);

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

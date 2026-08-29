using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class WorkingMapLayoutTests
{
    [Fact]
    public void LayoutRequiresExactly4096Words()
    {
        Assert.Throws<ArgumentException>(() => new WorkingMapLayout(new ushort[4095]));
        Assert.Throws<ArgumentException>(() => new WorkingMapLayout(new ushort[4097]));
    }

    [Fact]
    public void SingleWordCopyReturnsNewLayoutAndLeavesInputUnchanged()
    {
        WorkingMapLayout input = SequentialLayout();

        WorkingMapLayout output = input.ApplyBlockCopy(Copy(1, 1, 5, 5, 1, 1));

        Assert.Equal(65, input[1, 1]);
        Assert.Equal(325, input[5, 5]);
        Assert.Equal(65, output[5, 5]);
    }

    [Fact]
    public void SameRowCopiesMultipleWordsInIncreasingColumnOrder()
    {
        WorkingMapLayout output = SequentialLayout().ApplyBlockCopy(Copy(3, 4, 20, 4, 4, 1));

        Assert.Equal([259, 260, 261, 262], Words(output, 276, 4));
    }

    [Fact]
    public void MultiRowRectangleAdvancesEachRowBy64Words()
    {
        WorkingMapLayout output = SequentialLayout().ApplyBlockCopy(Copy(1, 1, 10, 10, 3, 2));

        Assert.Equal([65, 66, 67], Words(output, 650, 3));
        Assert.Equal([129, 130, 131], Words(output, 714, 3));
    }

    [Fact]
    public void LinearRowCopyMayCrossColumn63WhenIndexesRemainInBounds()
    {
        WorkingMapLayout output = SequentialLayout().ApplyBlockCopy(Copy(62, 0, 62, 1, 4, 1));

        Assert.Equal([62, 63, 64, 65], Words(output, 126, 4));
    }

    [Fact]
    public void ForwardHorizontalOverlapCascadesWhenDestinationFollowsSource()
    {
        WorkingMapLayout input = LayoutWith(
            (0, 10),
            (1, 20),
            (2, 30),
            (3, 40),
            (4, 50));

        WorkingMapLayout output = input.ApplyBlockCopy(Copy(0, 0, 1, 0, 4, 1));

        Assert.Equal([10, 10, 10, 10, 10], Words(output, 0, 5));
    }

    [Fact]
    public void ReverseHorizontalOverlapReadsUnchangedLaterSources()
    {
        WorkingMapLayout input = LayoutWith(
            (0, 10),
            (1, 20),
            (2, 30),
            (3, 40),
            (4, 50));

        WorkingMapLayout output = input.ApplyBlockCopy(Copy(1, 0, 0, 0, 4, 1));

        Assert.Equal([20, 30, 40, 50], Words(output, 0, 4));
    }

    [Fact]
    public void ForwardVerticalOverlapCascadesWhenDestinationIsLower()
    {
        WorkingMapLayout input = LayoutWith(
            (0, 10),
            (64, 20),
            (128, 30),
            (192, 40));

        WorkingMapLayout output = input.ApplyBlockCopy(Copy(0, 0, 0, 1, 1, 3));

        Assert.Equal(10, output[0, 1]);
        Assert.Equal(10, output[0, 2]);
        Assert.Equal(10, output[0, 3]);
    }

    [Fact]
    public void ReverseVerticalOverlapReadsUnchangedLowerSources()
    {
        WorkingMapLayout input = LayoutWith(
            (0, 10),
            (64, 20),
            (128, 30),
            (192, 40));

        WorkingMapLayout output = input.ApplyBlockCopy(Copy(0, 1, 0, 0, 1, 3));

        Assert.Equal(20, output[0, 0]);
        Assert.Equal(30, output[0, 1]);
        Assert.Equal(40, output[0, 2]);
    }

    [Fact]
    public void SourceEqualToDestinationPreservesEveryWord()
    {
        WorkingMapLayout input = SequentialLayout();

        WorkingMapLayout output = input.ApplyBlockCopy(Copy(4, 5, 4, 5, 7, 3));

        Assert.Equal(input.Words.ToArray(), output.Words.ToArray());
    }

    [Fact]
    public void WordsOutsideDestinationRemainUnchanged()
    {
        WorkingMapLayout input = SequentialLayout();
        WorkingMapLayout output = input.ApplyBlockCopy(Copy(0, 0, 10, 10, 2, 2));
        HashSet<int> changed = [650, 651, 714, 715];

        for (int index = 0; index < WorkingMapLayout.WordCount; index++)
        {
            if (!changed.Contains(index))
            {
                Assert.Equal(input.GetWord(index), output.GetWord(index));
            }
        }
    }

    [Fact]
    public void ConstructorDefensivelyCopiesCallerWords()
    {
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        words[123] = 456;
        WorkingMapLayout layout = new(words);

        words[123] = 999;

        Assert.Equal(456, layout.GetWord(123));
    }

    [Fact]
    public void ConsecutiveReducersUseThePriorOutputAsState()
    {
        WorkingMapLayout initial = LayoutWith((0, 77));

        WorkingMapLayout first = initial.ApplyBlockCopy(Copy(0, 0, 1, 0, 1, 1));
        WorkingMapLayout second = first.ApplyBlockCopy(Copy(1, 0, 2, 0, 1, 1));

        Assert.Equal(0, initial.GetWord(1));
        Assert.Equal(77, first.GetWord(1));
        Assert.Equal(77, second.GetWord(2));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void NonPositiveDimensionsAreRejected(int width, int height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Copy(0, 0, 0, 0, width, height));
    }

    [Theory]
    [InlineData(-1, 0, 0, 0)]
    [InlineData(64, 0, 0, 0)]
    [InlineData(0, -1, 0, 0)]
    [InlineData(0, 64, 0, 0)]
    [InlineData(0, 0, -1, 0)]
    [InlineData(0, 0, 64, 0)]
    [InlineData(0, 0, 0, -1)]
    [InlineData(0, 0, 0, 64)]
    public void CoordinatesOutsideTheLayoutAreRejected(
        int sourceX,
        int sourceY,
        int destinationX,
        int destinationY)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Copy(sourceX, sourceY, destinationX, destinationY, 1, 1));
    }

    [Theory]
    [InlineData(63, 63, 0, 0, 2, 1)]
    [InlineData(0, 0, 63, 63, 2, 1)]
    [InlineData(0, 63, 0, 0, 1, 2)]
    [InlineData(0, 0, 0, 63, 1, 2)]
    [InlineData(0, 0, 0, 0, 4097, 1)]
    public void AnySourceOrDestinationSpanOutsideTheLayoutIsRejected(
        int sourceX,
        int sourceY,
        int destinationX,
        int destinationY,
        int width,
        int height)
    {
        WorkingMapLayout layout = SequentialLayout();
        WorkingMapBlockCopy operation =
            Copy(sourceX, sourceY, destinationX, destinationY, width, height);

        Assert.Throws<ArgumentOutOfRangeException>(() => layout.ApplyBlockCopy(operation));
    }

    [Fact]
    public void IdenticalInputsProduceIdenticalResults()
    {
        WorkingMapLayout input = SequentialLayout();
        WorkingMapBlockCopy operation = Copy(7, 8, 9, 10, 5, 6);

        WorkingMapLayout first = input.ApplyBlockCopy(operation);
        WorkingMapLayout second = input.ApplyBlockCopy(operation);

        Assert.Equal(first.Words.ToArray(), second.Words.ToArray());
    }

    private static WorkingMapBlockCopy Copy(
        int sourceX,
        int sourceY,
        int destinationX,
        int destinationY,
        int width,
        int height) =>
        new(sourceX, sourceY, destinationX, destinationY, width, height);

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

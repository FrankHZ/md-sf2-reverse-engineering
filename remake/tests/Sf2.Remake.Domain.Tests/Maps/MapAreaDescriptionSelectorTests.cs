using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class MapAreaDescriptionSelectorTests
{
    [Fact]
    public void DirectReturnSourceHasNoMatch()
    {
        AreaDescriptionSelection selected = MapAreaDescriptionSelector.Select(
            MapAreaDescriptionSource.DirectReturn(),
            Query(1, 2));

        Assert.Equal(AreaDescriptionSelection.NoMatch, selected);
    }

    [Fact]
    public void EmptyTableHasNoMatch()
    {
        AreaDescriptionSelection selected = MapAreaDescriptionSelector.Select(
            MapAreaDescriptionSource.Table(1000, []),
            Query(1, 2));

        Assert.Equal(AreaDescriptionSelection.NoMatch, selected);
    }

    [Fact]
    public void ExactCoordinatesSelectTextAndComputeLogicalIndexes()
    {
        MapAreaDescriptionSource source = Table(
            1000,
            Text(3, 4, investigationOffset: 7, descriptionOffset: 11));

        AreaDescriptionSelection selected = MapAreaDescriptionSelector.Select(source, Query(3, 4));

        Assert.Equal(AreaDescriptionSelectionKind.Text, selected.Kind);
        Assert.Equal(430, selected.InvestigationTextIndex);
        Assert.Equal(1011, selected.DescriptionTextIndex);
        Assert.Null(selected.Function);
    }

    [Fact]
    public void YMismatchContinuesScanningAfterAnXMatch()
    {
        MapAreaDescriptionSource source = Table(
            0,
            Function(8, 1, "wrong-y"),
            Function(8, 2, "match"));

        AreaDescriptionSelection selected = MapAreaDescriptionSelector.Select(source, Query(8, 2));

        Assert.Equal(FunctionId("match"), selected.Function);
    }

    [Fact]
    public void DuplicateCoordinatesPreserveFirstMatchingEntry()
    {
        MapAreaDescriptionSource source = Table(
            0,
            Function(6, 7, "first"),
            Function(6, 7, "second"));

        AreaDescriptionSelection selected = MapAreaDescriptionSelector.Select(source, Query(6, 7));

        Assert.Equal(FunctionId("first"), selected.Function);
    }

    [Fact]
    public void OrdinaryAdmissionSkipsConditionedEntryAndContinuesScanning()
    {
        MapAreaDescriptionSource source = Table(
            0,
            ConditionedFunction(6, 7, "conditioned"),
            Function(6, 7, "ordinary"));

        AreaDescriptionSelection selected = MapAreaDescriptionSelector.Select(
            source,
            Query(6, 7, AreaDescriptionAdmission.Ordinary));

        Assert.Equal(FunctionId("ordinary"), selected.Function);
    }

    [Fact]
    public void ConditionedAdmissionSelectsFirstConditionedEntry()
    {
        MapAreaDescriptionSource source = Table(
            0,
            ConditionedFunction(6, 7, "conditioned"),
            Function(6, 7, "ordinary"));

        AreaDescriptionSelection selected = MapAreaDescriptionSelector.Select(
            source,
            Query(6, 7, AreaDescriptionAdmission.AllowConditioned));

        Assert.Equal(FunctionId("conditioned"), selected.Function);
    }

    [Fact]
    public void OrdinaryAdmissionWithOnlyConditionedCoordinateMatchHasNoMatch()
    {
        MapAreaDescriptionSource source = Table(0, ConditionedFunction(6, 7, "conditioned"));

        AreaDescriptionSelection selected = MapAreaDescriptionSelector.Select(source, Query(6, 7));

        Assert.Equal(AreaDescriptionSelection.NoMatch, selected);
    }

    [Fact]
    public void FunctionSelectionReturnsOnlyOpaqueTarget()
    {
        MapAreaDescriptionSource source = Table(999, Function(1, 1, "opaque-function"));

        AreaDescriptionSelection selected = MapAreaDescriptionSelector.Select(source, Query(1, 1));

        Assert.Equal(AreaDescriptionSelectionKind.Function, selected.Kind);
        Assert.Equal(FunctionId("opaque-function"), selected.Function);
        Assert.Null(selected.InvestigationTextIndex);
        Assert.Null(selected.DescriptionTextIndex);
    }

    [Fact]
    public void SourceCopiesCallerEntriesAndPreservesOrder()
    {
        List<MapAreaDescriptionEntry> entries =
        [
            Function(2, 2, "first"),
            Function(2, 2, "second"),
        ];
        MapAreaDescriptionSource source = MapAreaDescriptionSource.Table(0, entries);

        entries.Reverse();
        entries.RemoveAt(0);

        Assert.Equal(["first", "second"], source.Entries.Select(GetFunctionValue));
        Assert.Equal(
            FunctionId("first"),
            MapAreaDescriptionSelector.Select(source, Query(2, 2)).Function);
    }

    [Fact]
    public void RepeatedSelectionIsDeterministicAndDoesNotMutateTable()
    {
        MapAreaDescriptionSource source = Table(
            200,
            Text(5, 6, investigationOffset: 2, descriptionOffset: 3));
        MapAreaDescriptionEntry[] before = [.. source.Entries];

        AreaDescriptionSelection first = MapAreaDescriptionSelector.Select(source, Query(5, 6));
        AreaDescriptionSelection second = MapAreaDescriptionSelector.Select(source, Query(5, 6));

        Assert.Equal(first, second);
        Assert.Equal(before, source.Entries);
    }

    [Fact]
    public void ConditionedTextIsRejectedByModernAdmissionPolicy()
    {
        AreaDescriptionPayload text = AreaDescriptionPayload.Text(1, 2);

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => new MapAreaDescriptionEntry(
                1,
                2,
                AreaDescriptionCondition.RequiresConditionedAdmission,
                text));

        Assert.Contains("must use a function target", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownConditionIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MapAreaDescriptionEntry(
                1,
                2,
                (AreaDescriptionCondition)99,
                AreaDescriptionPayload.FunctionTarget(FunctionId("target"))));
    }

    [Fact]
    public void UnknownAdmissionIsRejected()
    {
        MapAreaDescriptionSource source = Table(0, Function(1, 2, "target"));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => MapAreaDescriptionSelector.Select(
                source,
                Query(1, 2, (AreaDescriptionAdmission)99)));
    }

    [Fact]
    public void NullTableEntryIsRejected()
    {
        MapAreaDescriptionEntry[] entries = [null!];

        Assert.Throws<ArgumentException>(
            () => MapAreaDescriptionSource.Table(0, entries));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    public void NegativeTextOffsetsAreRejected(int investigationOffset, int descriptionOffset)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => AreaDescriptionPayload.Text(investigationOffset, descriptionOffset));
    }

    [Fact]
    public void NegativeDescriptionTextBaseIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MapAreaDescriptionSource.Table(-1, []));
    }

    [Fact]
    public void InvestigationTextIndexOverflowFailsClosed()
    {
        MapAreaDescriptionSource source = Table(
            0,
            Text(1, 1, int.MaxValue, descriptionOffset: 0));

        Assert.Throws<OverflowException>(
            () => MapAreaDescriptionSelector.Select(source, Query(1, 1)));
    }

    [Fact]
    public void DescriptionTextIndexOverflowFailsClosed()
    {
        MapAreaDescriptionSource source = Table(
            int.MaxValue,
            Text(1, 1, investigationOffset: 0, descriptionOffset: 1));

        Assert.Throws<OverflowException>(
            () => MapAreaDescriptionSelector.Select(source, Query(1, 1)));
    }

    private static MapAreaDescriptionSource Table(
        int descriptionTextBase,
        params MapAreaDescriptionEntry[] entries) =>
        MapAreaDescriptionSource.Table(descriptionTextBase, entries);

    private static MapAreaDescriptionQuery Query(
        byte x,
        byte y,
        AreaDescriptionAdmission admission = AreaDescriptionAdmission.Ordinary) =>
        new(x, y, admission);

    private static MapAreaDescriptionEntry Text(
        byte x,
        byte y,
        int investigationOffset,
        int descriptionOffset) =>
        new(
            x,
            y,
            AreaDescriptionCondition.Always,
            AreaDescriptionPayload.Text(investigationOffset, descriptionOffset));

    private static MapAreaDescriptionEntry Function(byte x, byte y, string target) =>
        new(
            x,
            y,
            AreaDescriptionCondition.Always,
            AreaDescriptionPayload.FunctionTarget(FunctionId(target)));

    private static MapAreaDescriptionEntry ConditionedFunction(byte x, byte y, string target) =>
        new(
            x,
            y,
            AreaDescriptionCondition.RequiresConditionedAdmission,
            AreaDescriptionPayload.FunctionTarget(FunctionId(target)));

    private static AreaDescriptionFunctionId FunctionId(string value) => new(value);

    private static string GetFunctionValue(MapAreaDescriptionEntry entry) =>
        entry.Payload.Function?.Value ?? throw new InvalidOperationException();
}

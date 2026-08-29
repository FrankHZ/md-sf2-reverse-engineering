using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class MapSetupEventSelectorTests
{
    [Fact]
    public void EntitySpecificAfterEarlierNonMatchCarriesFlagsUnchanged()
    {
        MapSetupEventTable<EntityEventRecord> table = EntityTable(
            EntityEventRecord.Specific(7, 0x02, Target("wrong")),
            EntityEventRecord.Specific(128, 0xA5, Target("specific")),
            EntityEventRecord.Default(0x00, Target("default")));

        EntityEventSelection selected = MapSetupEventSelector.Select(table, new(128));

        Assert.Equal(Target("specific"), selected.Target);
        Assert.Equal(0xA5, selected.EventFlags);
    }

    [Fact]
    public void EntityNonMatchUsesDefaultAndItsFlags()
    {
        MapSetupEventTable<EntityEventRecord> table = EntityTable(
            EntityEventRecord.Specific(128, 0x01, Target("specific")),
            EntityEventRecord.Default(0x7E, Target("default")));

        EntityEventSelection selected = MapSetupEventSelector.Select(table, new(135));

        Assert.Equal(Target("default"), selected.Target);
        Assert.Equal(0x7E, selected.EventFlags);
    }

    [Fact]
    public void EntityDuplicateMatchesPreserveFirstEntry()
    {
        MapSetupEventTable<EntityEventRecord> table = EntityTable(
            EntityEventRecord.Specific(9, 0x11, Target("first")),
            EntityEventRecord.Specific(9, 0x22, Target("second")),
            EntityEventRecord.Default(0, Target("default")));

        EntityEventSelection selected = MapSetupEventSelector.Select(table, new(9));

        Assert.Equal(Target("first"), selected.Target);
        Assert.Equal(0x11, selected.EventFlags);
    }

    [Fact]
    public void ZoneExactMatchWins()
    {
        MapSetupEventTable<ZoneEventRecord> table = ZoneTable(
            Zone(27, 5, "exact"),
            ZoneEventRecord.Default(Target("default")));

        ZoneEventSelection selected = MapSetupEventSelector.Select(table, new(27, 5));

        Assert.Equal(Target("exact"), selected.Target);
    }

    [Fact]
    public void ZoneWildcardYMatchesAnyQueryY()
    {
        MapSetupEventTable<ZoneEventRecord> table = ZoneTable(
            ZoneEventRecord.Specific(Exact(2), EventFieldMatch.Any, Target("wildcard-y")),
            ZoneEventRecord.Default(Target("default")));

        ZoneEventSelection selected = MapSetupEventSelector.Select(table, new(2, 42));

        Assert.Equal(Target("wildcard-y"), selected.Target);
    }

    [Fact]
    public void ZoneWildcardXMatchesAnyQueryX()
    {
        MapSetupEventTable<ZoneEventRecord> table = ZoneTable(
            ZoneEventRecord.Specific(EventFieldMatch.Any, Exact(42), Target("wildcard-x")),
            ZoneEventRecord.Default(Target("default")));

        ZoneEventSelection selected = MapSetupEventSelector.Select(table, new(99, 42));

        Assert.Equal(Target("wildcard-x"), selected.Target);
    }

    [Fact]
    public void ZoneOverlapsPreserveFirstMatchingEntry()
    {
        MapSetupEventTable<ZoneEventRecord> table = ZoneTable(
            ZoneEventRecord.Specific(Exact(2), EventFieldMatch.Any, Target("first")),
            Zone(2, 23, "later-more-specific"),
            ZoneEventRecord.Default(Target("default")));

        ZoneEventSelection selected = MapSetupEventSelector.Select(table, new(2, 23));

        Assert.Equal(Target("first"), selected.Target);
    }

    [Fact]
    public void ZoneNonMatchUsesDefault()
    {
        MapSetupEventTable<ZoneEventRecord> table = ZoneTable(
            Zone(27, 5, "specific"),
            ZoneEventRecord.Default(Target("default")));

        ZoneEventSelection selected = MapSetupEventSelector.Select(table, new(10, 10));

        Assert.Equal(Target("default"), selected.Target);
    }

    [Fact]
    public void ItemIndexIsMaskedBeforeMatchingAndReturnedNormalized()
    {
        MapSetupEventTable<ItemEventRecord> table = ItemTable(
            Item(15, 19, 1, 112, "specific"),
            ItemEventRecord.Default(Target("default")));

        ItemEventSelection selected = MapSetupEventSelector.Select(table, new(15, 19, 1, 240));

        Assert.Equal(Target("specific"), selected.Target);
        Assert.Equal(112, selected.ItemIndex);
    }

    [Fact]
    public void ItemFacingMismatchUsesDefault()
    {
        MapSetupEventTable<ItemEventRecord> table = ItemTable(
            Item(15, 19, 1, 112, "specific"),
            ItemEventRecord.Default(Target("default")));

        ItemEventSelection selected = MapSetupEventSelector.Select(table, new(15, 19, 2, 112));

        Assert.Equal(Target("default"), selected.Target);
        Assert.Equal(112, selected.ItemIndex);
    }

    [Fact]
    public void ItemWildcardFacingMatchesAnyFacing()
    {
        MapSetupEventTable<ItemEventRecord> table = ItemTable(
            ItemEventRecord.Specific(
                Exact(35),
                Exact(24),
                EventFieldMatch.Any,
                125,
                Target("wildcard-facing")),
            ItemEventRecord.Default(Target("default")));

        ItemEventSelection selected = MapSetupEventSelector.Select(table, new(35, 24, 3, 125));

        Assert.Equal(Target("wildcard-facing"), selected.Target);
        Assert.Equal(125, selected.ItemIndex);
    }

    [Fact]
    public void ItemWildcardCoordinatesMatchAnyPosition()
    {
        MapSetupEventTable<ItemEventRecord> table = ItemTable(
            ItemEventRecord.Specific(
                EventFieldMatch.Any,
                EventFieldMatch.Any,
                Exact(1),
                12,
                Target("wildcard-position")),
            ItemEventRecord.Default(Target("default")));

        ItemEventSelection selected = MapSetupEventSelector.Select(table, new(77, 88, 1, 12));

        Assert.Equal(Target("wildcard-position"), selected.Target);
    }

    [Fact]
    public void ItemOverlapsPreserveFirstMatchingEntry()
    {
        MapSetupEventTable<ItemEventRecord> table = ItemTable(
            ItemEventRecord.Specific(
                Exact(3),
                Exact(4),
                EventFieldMatch.Any,
                5,
                Target("first")),
            Item(3, 4, 2, 5, "second"),
            ItemEventRecord.Default(Target("default")));

        ItemEventSelection selected = MapSetupEventSelector.Select(table, new(3, 4, 2, 5));

        Assert.Equal(Target("first"), selected.Target);
    }

    [Fact]
    public void TableCopiesCallerRecordsAndPreservesOrder()
    {
        List<ZoneEventRecord> records =
        [
            Zone(1, 1, "first"),
            Zone(1, 1, "second"),
            ZoneEventRecord.Default(Target("default")),
        ];
        MapSetupEventTable<ZoneEventRecord> table = ZoneTable(records.ToArray());

        records.Reverse();
        records.RemoveAt(0);

        Assert.Equal(["first", "second", "default"], table.Records.Select(x => x.Target.Value));
        Assert.Equal(Target("first"), MapSetupEventSelector.Select(table, new ZoneEventQuery(1, 1)).Target);
    }

    [Fact]
    public void TableWithoutDefaultFailsClosed()
    {
        EntityEventRecord[] records = [EntityEventRecord.Specific(1, 0, Target("specific"))];

        ArgumentException error = Assert.Throws<ArgumentException>(() => EntityTable(records));

        Assert.Contains("exactly one default", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TableWithMultipleDefaultsFailsClosed()
    {
        ItemEventRecord[] records =
        [
            ItemEventRecord.Default(Target("default-a")),
            ItemEventRecord.Default(Target("default-b")),
        ];

        ArgumentException error = Assert.Throws<ArgumentException>(() => ItemTable(records));

        Assert.Contains("exactly one default", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SpecificItemIndexesMustAlreadyBeNormalized()
    {
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(
            () => Item(1, 1, 1, 128, "invalid"));

        Assert.Contains("already be normalized", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RepeatedSelectionIsDeterministicWithoutMutatingTheTable()
    {
        MapSetupEventTable<ZoneEventRecord> table = ZoneTable(
            ZoneEventRecord.Specific(EventFieldMatch.Any, Exact(9), Target("specific")),
            ZoneEventRecord.Default(Target("default")));
        ZoneEventRecord[] before = [.. table.Records];

        ZoneEventSelection first = MapSetupEventSelector.Select(table, new(88, 9));
        ZoneEventSelection second = MapSetupEventSelector.Select(table, new(88, 9));

        Assert.Equal(first, second);
        Assert.Equal(before, table.Records);
    }

    private static MapSetupEventTable<EntityEventRecord> EntityTable(
        params EntityEventRecord[] records) => new(records);

    private static MapSetupEventTable<ZoneEventRecord> ZoneTable(
        params ZoneEventRecord[] records) => new(records);

    private static MapSetupEventTable<ItemEventRecord> ItemTable(
        params ItemEventRecord[] records) => new(records);

    private static EventTargetId Target(string value) => new(value);

    private static EventFieldMatch Exact(byte value) => EventFieldMatch.Exact(value);

    private static ZoneEventRecord Zone(byte x, byte y, string target) =>
        ZoneEventRecord.Specific(Exact(x), Exact(y), Target(target));

    private static ItemEventRecord Item(
        byte x,
        byte y,
        byte facing,
        byte item,
        string target) =>
        ItemEventRecord.Specific(Exact(x), Exact(y), Exact(facing), item, Target(target));
}

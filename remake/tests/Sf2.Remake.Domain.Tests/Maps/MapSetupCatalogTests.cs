using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class MapSetupCatalogTests
{
    private static readonly MapSetupId VoidSetup = new("void");

    [Fact]
    public void ConstructorCopiesEntriesAndPreservesInputOrder()
    {
        List<MapSetupCatalogEntry> source =
        [
            Entry("map-a", Route("default-a")),
            Entry("map-b", Route("default-b")),
            Entry("map-c", Route("default-c")),
        ];
        MapSetupCatalog catalog = new(source);

        source.Reverse();
        source.RemoveAt(0);

        Assert.Equal(["map-a", "map-b", "map-c"], catalog.Entries.Select(entry => entry.Map.Value));
    }

    [Fact]
    public void DuplicateMapIdFailsClosed()
    {
        MapSetupCatalogEntry[] entries =
        [
            Entry("map-a", Route("default-a")),
            Entry("map-a", Route("different-default")),
        ];

        ArgumentException error = Assert.Throws<ArgumentException>(() => new MapSetupCatalog(entries));

        Assert.Contains("Duplicate map ID 'map-a'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void KnownMapWithNoSetFlagsReturnsDefaultAfterFullRead()
    {
        MapSetupCatalog catalog = Catalog(
            Entry(
                "map-a",
                Route(
                    "default-a",
                    ("flag-a", "setup-a"),
                    ("flag-b", "setup-b"))));
        List<string> reads = [];

        MapSetupId selected = catalog.Select(
            new MapId("map-a"),
            VoidSetup,
            flag =>
            {
                reads.Add(flag.Value);
                return false;
            });

        Assert.Equal(new MapSetupId("default-a"), selected);
        Assert.Equal(["flag-a", "flag-b"], reads);
    }

    [Fact]
    public void KnownMapWithMultipleSetFlagsUsesLastSetVariantAfterFullRead()
    {
        MapSetupCatalog catalog = Catalog(
            Entry(
                "map-a",
                Route(
                    "default-a",
                    ("flag-a", "setup-a"),
                    ("flag-b", "setup-b"),
                    ("flag-c", "setup-c"))));
        List<string> reads = [];

        MapSetupId selected = catalog.Select(
            new MapId("map-a"),
            VoidSetup,
            flag =>
            {
                reads.Add(flag.Value);
                return flag.Value is "flag-a" or "flag-c";
            });

        Assert.Equal(new MapSetupId("setup-c"), selected);
        Assert.Equal(["flag-a", "flag-b", "flag-c"], reads);
    }

    [Fact]
    public void KnownMapPreservesAliasBackToDefault()
    {
        MapSetupId defaultSetup = new("default-a");
        MapSetupRoute route = new(
            defaultSetup,
            [
                new(new FlagId("flag-a"), new MapSetupId("setup-a")),
                new(new FlagId("restore-default"), defaultSetup),
            ]);
        MapSetupCatalog catalog = Catalog(Entry("map-a", route));

        MapSetupId selected = catalog.Select(new MapId("map-a"), VoidSetup, _ => true);

        Assert.Equal(defaultSetup, selected);
    }

    [Fact]
    public void MissingMapReturnsVoidWithoutReadingFlags()
    {
        MapSetupCatalog catalog = Catalog(Entry("map-a", Route("default-a")));
        int reads = 0;

        MapSetupId selected = catalog.Select(
            new MapId("missing"),
            VoidSetup,
            _ =>
            {
                reads++;
                return true;
            });

        Assert.Equal(VoidSetup, selected);
        Assert.Equal(0, reads);
    }

    [Fact]
    public void SyntheticMapIdsOutsideTheOriginalRangeAreAdmitted()
    {
        MapSetupCatalog catalog = Catalog(Entry("map-9001", Route("future-default")));

        MapSetupId selected = catalog.Select(new MapId("map-9001"), VoidSetup, _ => false);

        Assert.Equal(new MapSetupId("future-default"), selected);
    }

    [Fact]
    public void RepeatedSelectionHasTheSameResultAndReadOrder()
    {
        MapSetupCatalog catalog = Catalog(
            Entry(
                "map-a",
                Route(
                    "default-a",
                    ("flag-a", "setup-a"),
                    ("flag-b", "setup-b"))));
        List<string> firstReads = [];
        List<string> secondReads = [];

        MapSetupId first = catalog.Select(
            new MapId("map-a"),
            VoidSetup,
            flag =>
            {
                firstReads.Add(flag.Value);
                return true;
            });
        MapSetupId second = catalog.Select(
            new MapId("map-a"),
            VoidSetup,
            flag =>
            {
                secondReads.Add(flag.Value);
                return true;
            });

        Assert.Equal(first, second);
        Assert.Equal(["flag-a", "flag-b"], firstReads);
        Assert.Equal(firstReads, secondReads);
    }

    private static MapSetupCatalog Catalog(params MapSetupCatalogEntry[] entries) => new(entries);

    private static MapSetupCatalogEntry Entry(string map, MapSetupRoute route) =>
        new(new MapId(map), route);

    private static MapSetupRoute Route(
        string defaultSetup,
        params (string Flag, string Setup)[] alternatives) =>
        new(
            new MapSetupId(defaultSetup),
            alternatives.Select(
                alternative => new MapSetupFlagVariant(
                    new FlagId(alternative.Flag),
                    new MapSetupId(alternative.Setup))));
}

using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class MapSetupSelectorTests
{
    private static readonly MapSetupId VoidSetup = new("void");
    private static readonly MapSetupId DefaultSetup = new("default");

    [Fact]
    public void MissingRouteReturnsVoidWithoutReadingFlags()
    {
        int reads = 0;

        MapSetupId selected = MapSetupSelector.Select(
            route: null,
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
    public void NoSetFlagsReturnsDefaultAfterReadingEveryAlternative()
    {
        MapSetupRoute route = Route(
            ("flag-a", "setup-a"),
            ("flag-b", "setup-b"));
        List<string> reads = [];

        MapSetupId selected = MapSetupSelector.Select(
            route,
            VoidSetup,
            flag =>
            {
                reads.Add(flag.Value);
                return false;
            });

        Assert.Equal(DefaultSetup, selected);
        Assert.Equal(["flag-a", "flag-b"], reads);
    }

    [Fact]
    public void SingleSetFlagWins()
    {
        MapSetupRoute route = Route(
            ("flag-a", "setup-a"),
            ("flag-b", "setup-b"));

        MapSetupId selected = MapSetupSelector.Select(
            route,
            VoidSetup,
            flag => flag == new FlagId("flag-a"));

        Assert.Equal(new MapSetupId("setup-a"), selected);
    }

    [Fact]
    public void MultipleSetFlagsAreFullyReadAndLastSetWins()
    {
        MapSetupRoute route = Route(
            ("flag-a", "setup-a"),
            ("flag-b", "setup-b"),
            ("flag-c", "setup-c"));
        List<string> reads = [];

        MapSetupId selected = MapSetupSelector.Select(
            route,
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
    public void AliasCanRestoreDefault()
    {
        MapSetupRoute route = new(
            DefaultSetup,
            [
                new(new FlagId("flag-a"), new MapSetupId("setup-a")),
                new(new FlagId("restore-default"), DefaultSetup),
            ]);

        MapSetupId selected = MapSetupSelector.Select(route, VoidSetup, _ => true);

        Assert.Equal(DefaultSetup, selected);
    }

    [Fact]
    public void LaterVariantCanOverrideAnAliasToDefault()
    {
        MapSetupRoute route = new(
            DefaultSetup,
            [
                new(new FlagId("flag-a"), new MapSetupId("setup-a")),
                new(new FlagId("restore-default"), DefaultSetup),
                new(new FlagId("flag-c"), new MapSetupId("setup-c")),
            ]);

        MapSetupId selected = MapSetupSelector.Select(route, VoidSetup, _ => true);

        Assert.Equal(new MapSetupId("setup-c"), selected);
    }

    [Fact]
    public void RouteCopiesCallerAlternativesAndRepeatedSelectionIsDeterministic()
    {
        List<MapSetupFlagVariant> alternatives =
        [
            new(new FlagId("flag-a"), new MapSetupId("setup-a")),
            new(new FlagId("flag-b"), new MapSetupId("setup-b")),
        ];
        MapSetupFlagVariant[] original = [.. alternatives];
        MapSetupRoute route = new(DefaultSetup, alternatives);
        List<string> firstReads = [];
        List<string> secondReads = [];

        MapSetupId first = MapSetupSelector.Select(
            route,
            VoidSetup,
            flag =>
            {
                firstReads.Add(flag.Value);
                return true;
            });

        Assert.Equal(original, alternatives);
        alternatives.Reverse();

        MapSetupId second = MapSetupSelector.Select(
            route,
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

    private static MapSetupRoute Route(params (string Flag, string Setup)[] alternatives) =>
        new(
            DefaultSetup,
            alternatives.Select(
                alternative => new MapSetupFlagVariant(
                    new FlagId(alternative.Flag),
                    new MapSetupId(alternative.Setup))));
}

using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class ExplorationStartSetupTests
{
    [Theory]
    [InlineData(0, 0, 2, "base-entry", 100)]
    [InlineData(79, 0, 2, "base-entry", 100)]
    [InlineData(10, 0, 3, "first-entry", 101)]
    [InlineData(20, 0, 4, "later-entry", 102)]
    [InlineData(10, 20, 4, "later-entry", 102)]
    [InlineData(20, 10, 4, "later-entry", 102)]
    [InlineData(10, 30, 2, "base-entry", 100)]
    public void DefaultEntryUsesTheSameOrderedSetupAsPopulation(int first, int second,
        int x, string program, int writtenFlag)
    {
        var (definition, party) = Content();
        var start = Start(party, flags: new[] { first, second }.Where(flag => flag != 0).ToArray());
        var initial = ExplorationDispatcher.Initialize(definition, start);
        Assert.Equal(new ProgramLocation(program, 0), initial.Story.Cursor);
        Assert.Equal(ProgramContinuation.MapLoaded, initial.Story.Continuation);
        Assert.DoesNotContain(writtenFlag, initial.Story.Flags); // Initial-save capture runs no program.
        AssertPopulation(initial, x);

        var started = Assert.IsType<SessionStarted>(GameSession.Start(definition, start));
        Assert.Null(started.Result.Failure);
        AssertPopulation(started.Session.Current, x);
        AssertEntry(started.Session.Current, writtenFlag);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(20, 4)]
    public void ExplicitWholeProgramOverridesSelectedOnLoadWithoutReplacingPopulation(int flag, int x)
    {
        var (definition, party) = Content();
        var start = Start(party, flags: flag == 0 ? [] : [flag], entry: new("explicit-entry", 0));
        var initial = ExplorationDispatcher.Initialize(definition, start);
        Assert.Equal(new ProgramLocation("explicit-entry", 0), initial.Story.Cursor);
        Assert.Equal(ProgramContinuation.FieldInput, initial.Story.Continuation);
        var started = Assert.IsType<SessionStarted>(GameSession.Start(definition, start));
        Assert.Null(started.Result.Failure);
        AssertPopulation(started.Session.Current, x);
        AssertEntry(started.Session.Current, 103);
    }

    [Fact]
    public void SelectedSetupWithNoOnLoadDoesNotFallBackToBaseProgram()
    {
        var (definition, party) = Content();
        var started = Assert.IsType<SessionStarted>(GameSession.Start(definition, Start(party, flags: [40])));
        Assert.Null(started.Result.Failure);
        AssertPopulation(started.Session.Current, 5);
        AssertEntry(started.Session.Current, null);
    }

    [Fact]
    public void DifferentMapUsesItsOwnPopulationAndDefaultEntry()
    {
        var (definition, party) = Content();
        var started = Assert.IsType<SessionStarted>(GameSession.Start(definition,
            Start(party, map: "courtyard", flags: [10, 20])));
        Assert.Null(started.Result.Failure);
        Assert.Equal(new MapId("courtyard"), started.Session.Current.Exploration!.Map);
        AssertPopulation(started.Session.Current, 6);
        AssertEntry(started.Session.Current, 104);
    }

    [Theory]
    [InlineData("missing-entry", 0)]
    [InlineData("explicit-entry", 1)]
    [InlineData("explicit-entry", -1)]
    public void InvalidExplicitEntryRejectsInsteadOfUsingSelectedDefault(string program, int instruction)
    {
        var (definition, party) = Content();
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(definition,
            Start(party, flags: [20], entry: new(program, instruction))));
        Assert.Equal(SessionFailureKind.ContentError, failed.Failure.Kind);
        Assert.Equal("program-entry", failed.Failure.Code);
        Assert.Equal("start.program", failed.Failure.Field);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingMapOrExplorationRejects(bool missingExploration)
    {
        var (definition, party) = Content();
        if (missingExploration) definition = new("battle-only", definition.Encounters.Values);
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(definition, Start(party, map: "missing")));
        Assert.Equal(SessionFailureKind.ContentError, failed.Failure.Kind);
        Assert.Equal("missing-map", failed.Failure.Code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnavailableSelectedSetupRejectsEvenWithAnExplicitEntry(bool explicitEntry)
    {
        var (definition, party) = Content();
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(definition,
            Start(party, flags: [90], entry: explicitEntry ? new("explicit-entry", 0) : null)));
        Assert.Equal(SessionFailureKind.UnsupportedCapability, failed.Failure.Kind);
        Assert.Equal("map-setup", failed.Failure.Code);
        Assert.Equal("unavailable", failed.Failure.Field);
    }

    [Theory]
    [InlineData(4, 32, 20, "exploration-start")]
    [InlineData(0, 0, 20, "exploration-start")]
    [InlineData(0, 385, 20, "exploration-start")]
    [InlineData(0, 32, -1, "exploration-start")]
    [InlineData(0, 32, 65536, "exploration-start")]
    public void InvalidStartStillRejectsBeforeSetupSelection(byte facing, ushort speed, int flag, string code)
    {
        var (definition, party) = Content();
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(definition,
            new ExplorationStartInput(new("meadow"), new("entity-0"), new(1, 1), facing, speed, [flag], party)));
        Assert.Equal(SessionFailureKind.ContentError, failed.Failure.Kind);
        Assert.Equal(code, failed.Failure.Code);
    }

    private static void AssertPopulation(SessionSnapshot snapshot, int x)
    {
        var world = snapshot.Exploration!;
        Assert.Equal(2, world.AllEntities.Count());
        Assert.Equal(new MapPosition(x, 2), world.Entities[new("entity-128")].Position);
        Assert.Equal(200 + x, world.Entities[new("entity-128")].Sprite);
        Assert.Equal(300 + x, world.PlayerEntity.Sprite);
    }

    private static void AssertEntry(SessionSnapshot snapshot, int? writtenFlag)
    {
        Assert.True(snapshot.CanWaitAtInput);
        Assert.Null(snapshot.Story.Cursor);
        Assert.Equal(writtenFlag is { } flag ? new[] { flag } : [],
            snapshot.Story.Flags.Where(value => value is >= 100 and <= 104).ToArray());
    }

    private static ExplorationStartInput Start(BattleStartInput party, string map = "meadow",
        int[]? flags = null, ProgramLocation? entry = null) =>
        new(new(map), new("entity-0"), new(1, 1), 0, 32, flags ?? [], party, entry);

    private static (ScenarioDefinition Definition, BattleStartInput Party) Content()
    {
        var battle = EngineTestContent.Admitted();
        var layout = new WorkingMapLayout(new ushort[4096]);
        var traversal = new OriginalMapTraversal([new(0, 0, 63, 63)]);
        ExplorationEntityDefinition[] Entities(int x) => [new(new("entity-128"), new(x, 2), 0, 32, Sprite: 200 + x)];
        ExplorationPopulation Population(int x) => new(30, 128, 300 + x, []);
        ExplorationSetupDefinition Setup(string id, int x, string? program) =>
            new(new(id), Entities(x), [], program is null ? null : new(program, 0), null, Population(x));
        var map = new ExplorationMapDefinition(new("meadow"), layout, traversal, Entities(2), [],
            onLoad: new("base-entry", 0), population: Population(2),
            setup: new(new("base"), [new(new("10"), new("first")), new(new("20"), new("later")),
                new(new("30"), new("base")), new(new("40"), new("quiet")), new(new("90"), new("unavailable"))]),
            setups: [Setup("first", 3, "first-entry"), Setup("later", 4, "later-entry"), Setup("quiet", 5, null)]);
        var other = new ExplorationMapDefinition(new("courtyard"), layout, traversal, Entities(6), [],
            onLoad: new("other-entry", 0), population: Population(6));
        var programs = new[] { "base-entry", "first-entry", "later-entry", "explicit-entry", "other-entry" }
            .Select((id, index) => new StoryProgram(id, [new WriteFlag(100 + index, true), new ReturnProgram()]));
        return (new("setup-start", battle.Definition.Encounters.Values, exploration: new([map, other], programs)), battle.Start);
    }
}

using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PreVoyageGuidanceTests
{
    private static readonly int[] TravelMaps = [3, 19, 20, 21, 40, 57, 66, 5, 43, 17, 16, 18, 60, 58];

    public static IEnumerable<object[]> StoryBoundaries()
    {
        yield return Case(3, [], "start-day", "leave-home", 56, 3);
        yield return Case(3, [], "start-day", "mother", 3, 3);
        yield return Case(3, [601], "attend-school", "school-courtyard", 42, 9);
        yield return Case(3, [601], "attend-school", "teacher", 59, 13);
        yield return Case(3, [601, 602], "join-lesson", "return-class", 59, 13);
        yield return Case(3, [603], "visit-castle", "castle-north");
        yield return Case(19, [603], "visit-castle", "royal-quarters");
        yield return Case(20, [603, 605], "join-astral", "return-hall");
        yield return Case(19, [603, 605, 608], "follow-astral", "west-stairs");
        yield return Case(21, [603, 605, 608], "follow-astral", "tower-guard");
        yield return Case(40, [603, 605, 608, 401], "enter-tower", "tower-passage");
        yield return Case(57, [501], "return-king", "reverse-tower");
        yield return Case(20, [501], "return-king", "king-room");
        yield return Case(19, [501, 609], "hear-commission", "royal-party");
        yield return Case(3, [501, 609, 610], "fetch-hawel", "west-town-exit");
        yield return Case(66, [610, 402], "fetch-hawel", "toward-yeel");
        yield return Case(5, [610, 502], "find-hawel", "ask-hawel");
        yield return Case(66, [610, 502], "find-hawel", "visit-yeel");
        yield return Case(5, [72, 502, 403], "visit-hawel", "next-north");
        yield return Case(66, [72, 502, 403], "visit-hawel", "route-unavailable");
        yield return Case(66, [72, 502, 503], "visit-hawel", "known-house");
        yield return Case(43, [4, 612, 404], "ancient-shrine-goal", "leave-house");
        yield return Case(66, [4, 612, 404], "ancient-shrine-goal", "south-house");
        yield return Case(17, [4, 612, 504, 660], "leave-cells", "other-prisoners", 59, 5);
        yield return Case(17, [504, 660, 661], "escape-galam", "approach-stairs", 53, 5);
        yield return Case(17, [504, 660, 661, 662], "escape-galam", "use-shortcut", 56, 5);
        yield return Case(17, [504, 660, 661, 662, 73], "escape-galam", "tunnel-kitchen", 29, 18);
        yield return Case(17, [504, 660, 661, 662, 73], "escape-galam", "leave-kitchen", 32, 9);
        yield return Case(17, [504, 660, 661, 662, 73], "escape-galam", "use-shortcut", 53, 5);
        yield return Case(16, [504, 663, 664, 405], "escape-galam", "west-castle-exit");
        yield return Case(16, [504, 505, 406], "return-granseal", "south-galam-gate");
        yield return Case(17, [504, 505, 406], "return-granseal", "castle-interior-exit");
        yield return Case(18, [505, 406], "return-granseal", "south-drawbridge");
        yield return Case(16, [505, 406, 407], "return-granseal", "south-galam-gate");
        yield return Case(66, [505, 506, 407], "check-castle", "toward-granseal");
        yield return Case(3, [506, 407], "check-castle", "castle-north");
        yield return Case(3, [506, 407, 6, 38], "check-castle", "castle-north");
        yield return Case(19, [506, 620], "rescue-at-tower", "royal-party");
        yield return Case(19, [506, 620, 621, 83], "rescue-at-tower", "west-stairs");
        yield return Case(60, [506, 621, 407, 457], "rescue-at-tower", "tower-portals");
        yield return Case(16, [506, 621, 407, 457], "rescue-at-tower", "south-galam-gate");
        yield return Case(16, [505, 407, 457], "rescue-at-tower", "south-galam-gate");
        yield return Case(60, [506, 620], "rescue-at-tower", "tower-portals");
        yield return Case(58, [506, 621, 507, 74, 640], "return-from-tower", "reverse-tower");
        yield return Case(60, [507, 74, 640], "return-from-tower", "reverse-tower");
        yield return Case(19, [507, 74, 640], "return-from-tower", "cross-castle");
        yield return Case(19, [507, 74, 640, 622], "escape-earthquake", "castle-south");
        yield return Case(4, [507, 74, 640, 624], "reach-docks", "east-safe-exit", 49, 24);
        yield return Case(44, [507, 74, 640, 624, 623], "board-ship", "dock-man", 9, 14);
    }

    private static object[] Case(int map, int[] flags, string goal, string next, int x = 7, int y = 7) => [map, flags, goal, next, x, y];

    [Theory]
    [MemberData(nameof(StoryBoundaries))]
    public void ActualContentRulesCoverTheReachedFreeControlAndReturn(int map, int[] flags, string goal, string next, int x, int y)
    {
        var (definition, snapshot) = Sample(map, flags, new(x, y));
        var result = Assert.IsType<FieldGuidance>(FieldGuidanceQuery.Query(definition, snapshot));
        Assert.Equal("guidance." + goal, result.ObjectiveKey);
        Assert.Equal("guidance." + next, result.RouteKey);
        Assert.Equal(snapshot.Revision, result.Revision);
        Assert.Equal(snapshot.Exploration!.Map, result.Map);
    }

    [Theory]
    [InlineData(66, new[] { 507, 640 })]
    [InlineData(44, new[] { 507, 640 })]
    [InlineData(44, new[] { 507, 74, 640 })]
    [InlineData(14, new[] { 507, 700 })]
    [InlineData(3, new[] { 603, 605, 608, 401, 700 })]
    [InlineData(5, new[] { 502 })]
    public void BoardingOrUndisclosedProgressCannotReviveAnEarlierTask(int map, int[] flags)
    {
        var (definition, snapshot) = Sample(map, flags, new(7, 7));
        Assert.Null(FieldGuidanceQuery.Query(definition, snapshot));
    }

    [Fact]
    public void DetoursAndDefeatOrEgressKeepTheDisclosedGoalWithoutOptionalRecruitmentOrFollowers()
    {
        foreach (int map in TravelMaps)
            foreach (var row in new[] { (new[] { 505, 406, 407 }, "return-granseal"),
                (new[] { 506, 621, 407, 457 }, "rescue-at-tower"), (new[] { 4, 612, 404 }, "ancient-shrine-goal") })
            {
                var (definition, snapshot) = Sample(map, row.Item1, new(7, 7));
                var result = Assert.IsType<FieldGuidance>(FieldGuidanceQuery.Query(definition, snapshot));
                Assert.Equal("guidance." + row.Item2, result.ObjectiveKey);
                Assert.Null(result.Companion);
            }
        var (guide, field) = Sample(5, [72, 502, 403], new(7, 7));
        Assert.Equal(4, FieldGuidanceQuery.Query(guide, field)!.Companion);
        Assert.All(field.Exploration!.AllEntities, e => Assert.Null(e.Follower));
    }

    [Fact]
    public void RequeryAfterAStoryCommitClearsThePriorDestinationAndDoesNotMutateTheSnapshot()
    {
        var (definition, snapshot) = Sample(66, [72, 502, 503], new(6, 6));
        Assert.Equal("guidance.hawel-outside-yeel", FieldGuidanceQuery.Query(definition, snapshot)!.DestinationKey);
        var changed = snapshot.WithStory(new([4, 612, 504], portraitWindow: new ClosedPortraitWindow()));
        var story = changed.Story; var party = changed.Exploration!.Party;
        Assert.Equal("guidance.galam-exit", FieldGuidanceQuery.Query(definition, changed)!.DestinationKey);
        Assert.Same(story, changed.Story); Assert.Same(party, changed.Exploration.Party);
    }

    private static (ScenarioDefinition, SessionSnapshot) Sample(int mapId, int[] flags, MapPosition position)
    {
        var original = Start("harbor-arrival");
        var maps = TravelMaps.Concat([4, 44, 14]).Select(Map).ToArray();
        var world = new ExplorationDefinition(maps, [], provenance: new("synthetic-guidance", "content-rules", "no-private-input", [], "Query behavior only."));
        world.Guidance = PrivateFieldGuidance.Bind(world);
        Assert.InRange(world.Guidance.Count, 100, 1000);
        var definition = new ScenarioDefinition("changed-guidance-content", original.Definition.Encounters.Values, exploration: world);
        var selected = world.Maps[new($"map-{mapId}")].SelectSetup(flags);
        var player = original.Current.Exploration!.PlayerEntity with { Motion = EntityMotionState.At(position, 0, 32) };
        var field = new ExplorationState(selected, selected.Layout, player.Entity, [player], original.Current.Exploration.Party);
        return (definition, new(original.Current.SessionId, 17, 12, new ActiveExploration(field),
            new(flags, portraitWindow: new ClosedPortraitWindow()), SessionStopReason.PlayerInput));
    }

    private static ExplorationMapDefinition Map(int id)
    {
        var layout = new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]);
        var events = id switch
        {
            5 => new[] { new ExplorationEvent(ExplorationEventKind.Warp, null, 0, null, null, new("map-66"), new(18, 21)) },
            66 => [new(ExplorationEventKind.Warp, 6, 5, null, null, new("map-43"), new(6, 11))],
            4 => [new(ExplorationEventKind.Warp, 50, 24, null, null, new("map-44"), new(1, 25))],
            _ => [],
        };
        int[] flags = id switch { 3 => [609, 506], 19 => [501, 609, 506, 507], 20 or 21 => [501, 609, 506],
            16 => [663], 17 => [505], 44 => [507], _ => [] };
        var setup = id is 57 or 58 or 60 ? null : new MapSetupRoute(new($"ms-map{id}"),
            flags.Select(f => new MapSetupFlagVariant(new(f.ToString()), new($"ms-map{id}-flag{f}"))));
        var population = new ExplorationPopulation(1, 128, 0, []);
        return new(new($"map-{id}"), layout, new([new(0, 0, 63, 63)]), [], events, setup: setup,
            setups: flags.Select(f => new ExplorationSetupDefinition(new($"ms-map{id}-flag{f}"), [], events, null, null, population)));
    }
}

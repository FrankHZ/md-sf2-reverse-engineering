using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldGuidanceTests
{
    // Different map, progress bits, companion and text prove the query consumes content.
    private static readonly FieldGuidanceAnchor Exit = new(new(5, 1), new(4, 1), new("outside"), new(2, 3), "door");
    private static FieldGuidanceRule Rule(int[]? required = null, int[]? forbidden = null) =>
        new(new("harbor"), new("day"), required ?? [91], forbidden ?? [92, 93], new("ferryman"), "objective", 7,
            "destination", "next-exit", Exit);

    private static (ScenarioDefinition Definition, SessionSnapshot Snapshot) Sample(int[] flags, bool follower = true,
        bool blocked = false, bool bound = true, string setup = "day")
    {
        var session = Start("harbor-arrival");
        var before = session.Current;
        var field = before.Exploration!;
        var old = field.Definition;
        var words = Enumerable.Repeat((ushort)0, WorkingMapLayout.WordCount).ToArray();
        if (blocked) words[1 * 64 + 4] = 0xC000;
        var layout = new WorkingMapLayout(words);
        var map = new ExplorationMapDefinition(new("harbor"), layout, new([new(0, 0, 7, 7)]), [],
            bound ? [new(ExplorationEventKind.Warp, 5, 1, null, null, new("outside"), new(2, 3))] : [],
            setup: new(new(setup), []));
        var entities = field.AllEntities.Select(e => e.Entity == new EntityRef("ferryman")
            ? e with { Follower = follower ? new(field.PlayerEntity.Slot, 0, 0) : null } : e);
        var changed = new ExplorationState(map, layout, field.Player, entities, field.Party);
        session.Definition.Exploration!.Guidance = [Rule()];
        return (session.Definition, new(before.SessionId, before.Revision, before.ObservationSequence,
            new ActiveExploration(changed), new(flags, portraitWindow: new ClosedPortraitWindow()), SessionStopReason.PlayerInput));
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, false, false)]
    [InlineData(true, false, true, false)]
    [InlineData(false, true, true, false)]
    public void DisclosesOnlyTheLiveUncompletedOffer(bool offer, bool joined, bool completed, bool expected)
    {
        var (definition, current) = Sample((offer ? new[] { 91 } : []).Concat(joined ? [92] : []).Concat(completed ? [93] : []).ToArray());
        var guidance = FieldGuidanceQuery.Query(definition, current);
        Assert.Equal(expected, guidance is not null);
        if (guidance is null) return;
        Assert.Equal(current.SessionId, guidance.SessionId); Assert.Equal(current.Revision, guidance.Revision);
        Assert.Equal("objective", guidance.ObjectiveKey); Assert.Equal(7, guidance.Companion); Assert.Equal(Exit, guidance.Anchor);
        Assert.Equal("destination", guidance.DestinationKey);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, true)]
    public void MissingOrBlockedLocalEntranceRetainsKnownDestinationWithoutMarker(bool blocked, bool bound)
    {
        var (definition, current) = Sample([91], blocked: blocked, bound: bound);
        var guidance = Assert.IsType<FieldGuidance>(FieldGuidanceQuery.Query(definition, current));
        Assert.Null(guidance.Anchor); Assert.Equal("destination", guidance.DestinationKey);
        Assert.Equal("guidance.route-unavailable", guidance.RouteKey);
    }

    [Fact]
    public void IncompleteLegCannotBorrowTheCompletedLegAndAmbiguityCannotChooseFirst()
    {
        var (definition, current) = Sample([91]);
        definition.Exploration!.Guidance = [Rule(forbidden: [92, 93, 94]) with { Anchor = null, RouteKey = "unavailable" }, Rule([91, 94])];
        var first = FieldGuidanceQuery.Query(definition, current)!;
        Assert.Null(first.Anchor); Assert.Equal("unavailable", first.RouteKey);
        var completed = current.WithStory(new([91, 94], portraitWindow: new ClosedPortraitWindow()));
        Assert.Equal(Exit, FieldGuidanceQuery.Query(definition, completed)!.Anchor);
        definition.Exploration.Guidance = [Rule(), Rule()];
        Assert.Null(FieldGuidanceQuery.Query(definition, current));
    }

    [Fact]
    public void AbsentFollowerChangedSetupAndSceneOwnTheField()
    {
        foreach (var sample in new[] { Sample([91], follower: false), Sample([91], setup: "night") })
            Assert.Null(FieldGuidanceQuery.Query(sample.Definition, sample.Snapshot));
        var (definition, current) = Sample([91]);
        foreach (var story in new[] {
            new StoryState([91], cursor: new("caller", 0)),
            new StoryState([91], wait: new DialogueWait(new(1), 1, TextDisplayMode.Single, null)),
            new StoryState([91], warp: new(new("outside"), new(2, 3), 0, MapLoadMode.Rebuild)),
        }) Assert.Null(FieldGuidanceQuery.Query(definition, current.WithStory(story)));
    }

    [Fact]
    public void OrdinaryCommittedMovementKeepsGuidanceButAnAutomaticSceneDoesNot()
    {
        var (definition, current) = Sample([91]);
        var moving = new SessionSnapshot(current.SessionId, current.Revision + 1, current.ObservationSequence,
            current.Active, new([91], wait: new EntityWait(new(1), current.Exploration!.Player),
                portraitWindow: new ClosedPortraitWindow()), SessionStopReason.SimulationWait);
        Assert.False(moving.CanWaitAtInput);
        Assert.True(moving.HasOrdinaryFieldTime);
        Assert.Equal(moving.Revision, FieldGuidanceQuery.Query(definition, moving)!.Revision);
        Assert.Null(FieldGuidanceQuery.Query(definition, moving.WithStory(new([91], wait: new TickWait(new(2), 10)))));
    }

    [Fact]
    public void FacadeQueryHasNoCommandsClockRngOrProgressWritesAndUncoveredContentIsEmpty()
    {
        var session = Start("harbor-arrival");
        var before = session.Current;
        for (int i = 0; i < 4; i++) Assert.Null(session.QueryFieldGuidance());
        Assert.Same(before, session.Current);
        var (definition, current) = Sample([91]);
        var story = current.Story; var field = current.Exploration; var party = field!.Party;
        for (int i = 0; i < 4; i++) Assert.NotNull(FieldGuidanceQuery.Query(definition, current));
        Assert.Same(story, current.Story); Assert.Same(field, current.Exploration); Assert.Same(party, current.Exploration!.Party);
    }
}

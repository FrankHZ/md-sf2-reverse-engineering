using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class RaftTravelTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void OccupancyPrecedesTheFirstMatchingWarpAndEmptyMarkersStillMove(bool occupied, bool rowEnabled)
    {
        var battle = EngineTestContent.Admitted();
        var words = new ushort[4096];
        words[7 * 64 + 19] = 0xD000;
        ExplorationEvent warp = new(ExplorationEventKind.Warp, 19, null, null, null, new("river"), new(2, 3),
            RequiredMarker: 0x1000, RequiredFlag: 71);
        var map = new ExplorationMapDefinition(new("reach"), new(words), new([new(0, 0, 63, 63)]), [],
            [warp, warp with { DestinationMap = new("later") }]);
        var player = new EntityRef("leader");
        var actor = EntityMotionState.At(new(19, occupied ? 7 : 8), 3, 32) with { FlagsA = 0x80, YDestination = 7 * 384 };
        var world = new ExplorationState(map, map.Layout, player,
            [new(player, EntityMotionState.At(new(18, 7), 0, 32) with { FlagsA = 0x20 }, true),
             new(new("blocker"), actor, true)], battle.Start, travelMode: FieldTravelMode.Raft);
        var (next, outcome) = MapEventDispatcher.Move(world, ExplorationDirection.East, rowEnabled ? [71] : []);
        Assert.Equal(!occupied, outcome.Moved);
        Assert.Equal(occupied || !rowEnabled ? null : warp, outcome.Event);
        Assert.Equal((occupied ? 18 : 19) * 384, next.PlayerEntity.Motion.XDestination);
        Assert.Null(outcome.VehicleEvent);
    }

    [Fact]
    public void EnteringAnAvailableEncounterClearsRidingModeBeforeTheFirstSceneCommand()
    {
        var session = EngineTestContent.Start("harbor-arrival");
        var field = session.Current.Exploration!;
        var berth = new RaftBerth(field.Map, field.PlayerEntity.Position);
        var story = session.Current.Story.Copy(null, flags: [71, 72], raft: berth);
        var current = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveExploration(field.WithTravelMode(FieldTravelMode.Raft)),
            story, SessionStopReason.PlayerInput);
        var route = new ExplorationBattleRoute(session.Definition.Encounters.Keys.First(), 71, 72, 73, new("before-entry", 0), null);
        var selected = BattleEntry.Select(current, story, route, []);
        Assert.Equal(FieldTravelMode.Walking, selected.Exploration!.TravelMode);
        Assert.Equal(route, selected.Story.EnteringBattle);
        Assert.Equal(route.BeforeProgram, selected.Story.Cursor);
        Assert.DoesNotContain(71, selected.Story.Flags);
        Assert.Equal(berth, selected.Story.Raft);
        Assert.Same(field.Party, selected.Exploration.Party);
    }

    [Fact]
    public void ScrollKeepsFacingHiddenActorsAndStepCountAndBypassesTheDestinationEncounter()
    {
        var battle = EngineTestContent.Admitted();
        var layout = new WorkingMapLayout(new ushort[4096]);
        var area = new OriginalMapTraversal([new(0, 0, 63, 63)]);
        var view = new ExplorationViewArea(0, 0, 63, 63, 0, 0, 0, 0, 256, 256, 256, 256, 0, 0, 0, 0, 0);
        var old = new ExplorationMapDefinition(new("reach"), layout, area, [], []);
        var target = new ExplorationMapDefinition(new("river"), layout, area, [], [], viewAreas: [view],
            entryFlags: [new(71, false)], battle: new("waiting", 71, 72, 73, null, null, Unavailable: true));
        var definition = new ScenarioDefinition("river-trip", battle.Definition.Encounters.Values,
            exploration: new([old, target], []));
        var player = new EntityRef("leader");
        var hidden = new ExplorationEntity(new("boat"), EntityMotionState.At(new(0, 0), 2, 32) with
            { X = 24576, Y = 24576, XDestination = 24576, YDestination = 24576 }, true, Slot: 31, Sprite: 61);
        var riding = new ExplorationState(old, layout, player, [new(player, EntityMotionState.At(new(17, 62), 2, 32),
            true, Slot: 0, Sprite: 61), hidden], battle.Start, travelMode: FieldTravelMode.Raft, currentEntities: [0]);
        var story = new StoryState([71], stepCounter: 23000, raft: new(old.Map, new(17, 3)));
        var current = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveExploration(riding), story, SessionStopReason.PlayerInput);
        var scroll = new ExplorationEvent(ExplorationEventKind.Warp, null, 63, null, null, target.Map,
            new(0, 1), Facing: 3, LoadMode: MapLoadMode.ScrollVertical);
        var result = MapTransfer.BeginWarp(definition, current, scroll, [], RuleCompositions.Sf2());
        var arrived = result.Exploration!;
        Assert.Equal(target.Map, arrived.Map);
        Assert.Equal(new MapPosition(17, 1), arrived.PlayerEntity.Position);
        Assert.Equal(2, arrived.PlayerEntity.Motion.Facing);
        Assert.Equal(FieldTravelMode.Raft, arrived.TravelMode);
        Assert.Same(hidden, arrived.AllEntities.Single(actor => actor.Slot == 31));
        Assert.Same(riding.Party, arrived.Party);
        Assert.Equal(story.Raft, result.Story.Raft);
        Assert.Equal(story.StepCounter, result.Story.StepCounter);
        Assert.Equal(story.Flags, result.Story.Flags);
        Assert.Null(result.Story.EnteringBattle);
    }
}

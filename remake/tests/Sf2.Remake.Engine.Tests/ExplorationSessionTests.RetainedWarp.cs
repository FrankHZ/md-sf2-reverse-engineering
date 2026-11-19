using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed partial class ExplorationSessionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OrdinaryWarpKeepsTheRequestedCoordinate(bool retainX)
    {
        var source = Start("harbor-arrival");
        var player = source.Current.Exploration!.PlayerEntity with { Motion = EntityMotionState.At(new(4, 6), 0, 32) };
        var trigger = retainX ? new MapPosition(4, 5) : new MapPosition(5, 6);
        var words = new ushort[WorkingMapLayout.WordCount];
        words[trigger.Y * 64 + trigger.X] = 0x1000;
        var layout = new WorkingMapLayout(words);
        var traversal = new OriginalMapTraversal([new(0, 0, 12, 12)]);
        var palette = new PalettePair(0xEEE, 0x888);
        var destination = new ExplorationMapDefinition(new("destination"), layout, traversal, [], [], basePalette: palette);
        var origin = new ExplorationMapDefinition(new("origin"), layout, traversal, [],
            [new(ExplorationEventKind.Warp, trigger.X, trigger.Y, null, null, destination.Map, new(8, 9),
                RequiredMarker: 0x1000, RetainDestinationX: retainX, RetainDestinationY: !retainX)]);
        var definition = new ScenarioDefinition("retained-warp", source.Definition.Encounters.Values,
            exploration: new([origin, destination], []));
        var world = new ExplorationState(origin, layout, player.Entity, [player], source.Current.Exploration.Party);
        var before = new SessionSnapshot(Guid.NewGuid(), 1, 1, new ActiveExploration(world),
            new StoryState([], null, display: new(3, palette, palette, FullFadeVisibility.BaseRestored)), SessionStopReason.PlayerInput);

        var requested = Step(definition, before, new Move(retainX ? ExplorationDirection.North : ExplorationDirection.East)).Snapshot;
        var current = Step(definition, requested, new AdvanceSimulation(requested.Story.Wait!.Token)).Snapshot;
        current = DrainFade(definition, current, early: true);
        current = Step(definition, current, new AdvanceSimulation(current.Story.Wait!.Token, 2)).Snapshot;
        current = DrainFade(definition, current, early: true);

        Assert.True(current.CanWaitAtInput);
        Assert.Equal(destination.Map, current.Exploration!.Map);
        Assert.Equal(retainX ? new MapPosition(4, 9) : new MapPosition(8, 6), current.Exploration.PlayerEntity.Position);
    }
}

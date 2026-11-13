using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed partial class ExplorationSessionTests
{
    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    public void ZoneArrivalCarriesTheMoveOnlyForADirectReturn(bool skipBody, bool waits, bool blocked)
    {
        var (original, before) = ControlWorld();
        var world = before.Exploration!;
        var words = new ushort[WorkingMapLayout.WordCount];
        words[4 * 64 + 5] = (ushort)(blocked ? 0xD400 : 0x1400);
        var layout = new WorkingMapLayout(words);
        var map = new ExplorationMapDefinition(world.Map, layout, world.Definition.Traversal, [],
            [new(ExplorationEventKind.SourceZone, 5, 4, null, new("zone", 0), RequiredMarker: 0x1400,
                SourceInit: new([new SetEntitySpeed(64, 64), new IdleEntityAction()]))], population: world.Population);
        StoryInstruction body = waits ? new WaitProgramTicks(1) : new WriteFlag(14, true);
        var definition = new ScenarioDefinition("zone-movement", original.Encounters.Values, exploration: new([map],
            [new("zone", [new BranchFlag(9, true, new("zone", 2)), body, new EndProgram()])]));
        world = new(map, layout, world.Player, world.AllEntities, world.Party);
        before = new(before.SessionId, before.Revision, before.ObservationSequence, new ActiveExploration(world),
            before.Story.Copy(null, flags: skipBody ? [9] : []), before.StopReason);

        var requested = Step(definition, before, new Move(ExplorationDirection.East)).Snapshot;
        var move = Assert.IsType<EntityWait>(requested.Story.Wait);
        var current = Step(definition, requested, new AdvanceSimulation(move.Token)).Snapshot;
        if (waits)
        {
            var timer = Assert.IsType<TickWait>(current.Story.Wait);
            current = Step(definition, current, new AdvanceSimulation(timer.Token)).Snapshot;
        }
        var arrival = Assert.IsType<ZoneArrivalWait>(current.Story.Wait);
        Assert.Equal(skipBody && !blocked ? move.Token : (WaitToken?)null, arrival.FieldMoveToken);
        Assert.NotEqual(move.Token, arrival.Token);
        Assert.False(current.CanWaitAtInput);
        Assert.Equal(!waits && !skipBody, current.Story.Flags.Contains(14));
        for (int tick = 0; tick < 60 && !current.CanWaitAtInput; tick++)
            current = Step(definition, current, new AdvanceSimulation(arrival.Token)).Snapshot;
        Assert.True(current.CanWaitAtInput);
        Assert.Equal(new MapPosition(blocked ? 4 : 5, 4), current.Exploration!.PlayerEntity.Position);
    }
}

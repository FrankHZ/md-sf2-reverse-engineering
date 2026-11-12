using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed partial class ExplorationSessionTests
{
    [Theory]
    [InlineData(1200, 0, 0)]
    [InlineData(600, 0, 88)]
    [InlineData(600, 5, 83)]
    [InlineData(255, 0, 0)]
    [InlineData(256, 0, 0)]
    [InlineData(40, 5, 35)]
    [InlineData(255, 254, 1)]
    [InlineData(256, 255, 1)]
    [InlineData(128, 128, 0)]
    [InlineData(127, 128, 255)]
    public void SourceEntityWaitUsesTheSignedTimerAndContinuesInTheClearingService(
        int operand, int initialTimer, int expectedIncrements)
    {
        var session = StartProgram($$"""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
                {"op":"wait-timer","operand":{{operand}}},{"op":"refresh-sprite","source":"waiting"}]},
             {"op":"end"}]
            """);
        var world = session.Current.Exploration!;
        var npc = world.Entities[new("ferryman")];
        world = world.WithEntity(npc with { Motion = npc.Motion with { WaitTimer = (byte)initialTimer } });
        for (int elapsed = 1; elapsed <= expectedIncrements; elapsed++)
        {
            var tick = EntityActionRunner.Tick(world);
            Assert.Null(tick.Failure);
            world = tick.World;
            var pending = world.Entities[npc.Entity];
            Assert.Equal(0, pending.ActionCursor);
            Assert.Equal(unchecked((byte)(initialTimer + elapsed)), pending.Motion.WaitTimer);
        }
        var cleared = EntityActionRunner.Tick(world);
        Assert.Null(cleared.Failure);
        var next = cleared.World.Entities[npc.Entity];
        Assert.Equal(0, next.Motion.WaitTimer);
        Assert.Equal(1, next.ActionCursor);
        Assert.True(next.WaitingForSprite);
    }

    [Theory]
    [InlineData("""{"op":"wait-timer","operand":1200}""", true)]
    [InlineData("""{"op":"wait","ticks":176}""", false)]
    public void AsynchronousEntityWaitAllowsTheCallerToProceedBeforeTheActorMoves(string action, bool moves)
    {
        var session = StartProgram($$"""
            [{"op":"motion","entity":"ferryman","installation":"SlotTimer","wait":false,
              "actions":[{{action}},{"op":"move","x":1,"y":0}]},
             {"op":"set-flag","flag":14,"value":true},{"op":"end"}]
            """);
        var installed = session.Current.Exploration!.Entities[new("ferryman")];
        Assert.Contains(14, session.Current.Story.Flags);
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Equal(installed.Slot, installed.Motion.WaitTimer);
        Assert.False(installed.Motion.IsMoving);
        Accept(session, new WaitAtInput());
        var after = session.Current.Exploration!.Entities[installed.Entity];
        Assert.Equal(moves ? 2 : 0, after.ActionCursor);
        Assert.Equal(moves ? 0 : installed.Slot + 1, after.Motion.WaitTimer);
        Assert.Equal(installed.Motion.X + (moves ? 384 : 0), after.Motion.XDestination);
    }
}

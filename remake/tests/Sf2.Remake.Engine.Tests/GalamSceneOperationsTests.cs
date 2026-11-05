using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class GalamSceneOperationsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DirectDestinationUsesItsFixedVelocityAndOnlyRequestedWait(bool wait)
    {
        var session = Program($$"""
            [{"op":"entity-destination","entity":"traveler","position":{"x":3,"y":2},"wait":{{wait.ToString().ToLowerInvariant()}}},
             {"op":"set-flag","flag":701,"value":true},{"op":"wait-ticks","ticks":1},{"op":"end"}]
            """);
        var actor = session.Current.Exploration!.PlayerEntity;
        Assert.Equal(new MapPosition(1, 1), actor.Position);
        Assert.Equal(32, actor.Motion.XVelocity);
        Assert.Equal(32, actor.Motion.YVelocity);
        Assert.Equal(96, actor.Motion.XSpeed);
        Assert.Equal(768, actor.Motion.XTravel);
        Assert.Equal(!wait, session.Current.Story.Flags.Contains(701));
        if (wait)
        {
            var pending = Assert.IsType<EntityWait>(session.Current.Story.Wait);
            Assert.Equal(EntityWaitCompletion.MotionStopped, pending.Completion);
            Accept(session, new AdvanceSimulation(pending.Token, 24));
            Assert.Equal(new MapPosition(3, 2), session.Current.Exploration!.PlayerEntity.Position);
            Assert.Contains(701, session.Current.Story.Flags);
        }
    }

    [Fact]
    public void LayoutCopyChangesCollisionAndRetainsTheSourceAndParty()
    {
        var session = Program("""
            [{"op":"copy-layout","copy":{"source":{"x":0,"y":0},"destination":{"x":3,"y":1},"width":1,"height":1}},
             {"op":"end"}]
            """, document => document["world"]!["maps"]![0]!["layout"]![0]![0] = 0xC123);
        var world = session.Current.Exploration!;
        Assert.Equal(0xC123, world.Layout[0, 0]);
        Assert.Equal(0xC123, world.Layout[3, 1]);
        Assert.True(OriginalMapTraversal.IsBlocked(world.Layout, new(3, 1)));
        Assert.Equal(0u, world.Party.Gold);
        Assert.Equal(new MapPosition(1, 1), world.PlayerEntity.Position);
    }

    [Fact]
    public void PortraitRestoresTheEventEntityAfterNarrationWithoutChangingTheCaller()
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", interaction: true, configure: document =>
        {
            document["world"]!["presentation"]!["sprites"]![0]!["portrait"] = 7;
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"close-portrait"},{"op":"speaker","entity":null},
                 {"op":"open-portrait","entity":null,"flags":0,"useEventSpeaker":true},
                 {"op":"wait-ticks","ticks":1},{"op":"end"}]
                """);
        });
        var caller = session.Current.Story.EventCaller;
        for (int i = 0; i < 30 && session.Current.Story.Wait is not TickWait; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.IsType<TickWait>(session.Current.Story.Wait);
        Assert.Equal(7, Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Portrait);
        Assert.Same(caller, session.Current.Story.EventCaller);
        Assert.Null(session.Current.Story.Speaker);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void WarpSkipsAnAlreadyCompletedInitSceneButRejectsTheReachableUnsupportedLoad(bool completed, bool backward)
    {
        var basis = Start("harbor-arrival");
        var words = new ushort[WorkingMapLayout.WordCount]; words[66] = 0x1000;
        var layout = new WorkingMapLayout(words);
        var map = new ExplorationMapDefinition(new("origin"), layout, new OriginalMapTraversal([new(0, 0, 12, 12)]), [],
            [new(ExplorationEventKind.Warp, 2, 1, null, null, new("origin"), new(9, 9),
                RequiredMarker: 0x1000, LoadMode: MapLoadMode.Preserve)], onLoad: new("entry", 0));
        var player = basis.Current.Exploration!.PlayerEntity with
        { Motion = EntityMotionState.At(new(1, 1), 0, 32) with { FlagsA = 0xA0 } };
        var world = new ExplorationState(map, layout, player.Entity, [player], basis.Current.Exploration.Party);
        var definition = new ScenarioDefinition("warp", basis.Definition.Encounters.Values, exploration: new([map],
            [new StoryProgram("entry", backward
                ? [new BranchFlag(660, true, new("entry", 2)), new LoadSceneMap(map.Map, new(0, 0)),
                    new WriteFlag(660, false), new JumpProgram(new("entry", 1))]
                : [new BranchFlag(660, true, new("ready", 0)), new LoadSceneMap(map.Map, new(0, 0)), new EndProgram()]),
             new StoryProgram("ready", [new WriteFlag(611, true), new EndProgram()])]));
        var palette = new PalettePair(0xEEE, 0x888);
        var current = new SessionSnapshot(Guid.NewGuid(), 1, 1, new ActiveExploration(world),
            new StoryState(completed ? [660] : [], null, display: new(3, palette, palette, FullFadeVisibility.BaseRestored)), SessionStopReason.PlayerInput);
        var moved = ExplorationDispatcher.Submit(definition, current, new Move(ExplorationDirection.East));
        if (!completed || backward)
        {
            Assert.Equal("ordinary-warp-scene-load", moved.Failure!.Code);
            Assert.Same(current.Active, moved.Snapshot.Active);
            Assert.DoesNotContain(611, moved.Snapshot.Story.Flags);
            return;
        }
        Assert.Null(moved.Failure);
        var reached = ExplorationDispatcher.Submit(definition, moved.Snapshot, new AdvanceSimulation(moved.Snapshot.Story.Wait!.Token, 600));
        Assert.Null(reached.Failure); current = reached.Snapshot;
        for (int i = 0; i < 12 && !current.CanWaitAtInput; i++)
        {
            var wait = current.Story.Wait!;
            SessionCommand command = wait is FullFadeWait { LogicalDone: true } fade
                ? new CompletePresentation(fade.Token, fade.Kind) : new AdvanceSimulation(wait.Token, 600);
            var step = ExplorationDispatcher.Submit(definition, current, command);
            Assert.Null(step.Failure); current = step.Snapshot;
        }
        Assert.True(current.CanWaitAtInput);
        Assert.Equal(new MapPosition(9, 9), current.Exploration!.PlayerEntity.Position);
        Assert.Contains(611, current.Story.Flags);
        Assert.Same(layout, current.Exploration.Layout);
    }

    private static GameSession Program(string instructions, Action<JsonNode>? change = null) => Start("harbor-arrival", document =>
    {
        document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse(instructions);
        document["start"]!["program"] = JsonNode.Parse("""{"program":"invitation","instruction":0}""");
        change?.Invoke(document);
    });
}

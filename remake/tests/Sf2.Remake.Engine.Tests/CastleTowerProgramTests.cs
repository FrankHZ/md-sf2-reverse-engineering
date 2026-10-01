using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class CastleTowerProgramTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(255)]
    public void UnequalPlaneInitializationQuantizesThenScalesBeforeOffsets(int layer)
    {
        var world = ParallaxWorld(layer);
        world = world.WithEntity(world.PlayerEntity with { Motion = world.PlayerEntity.Motion with { X = 4020, Y = 4310 } });
        var view = ExplorationViewRunner.Initialize(world);
        Assert.Equal(layer == 0 ? 1024 + 768 : 2048, view.AX.Position);
        Assert.Equal(layer == 0 ? 1152 + 1152 : 2304, view.AY.Position);
        Assert.Equal(layer == 0 ? 2048 : 1024 + 768, view.BX.Position);
        Assert.Equal(layer == 0 ? 2304 : 1152 + 1152, view.BY.Position);
        Assert.False(view.Scrolling);
        var destination = ExplorationViewRunner.SetDestination(view, new(9, 11));
        Assert.Equal(layer == 0 ? 1728 + 768 : 3456, destination.AX.Destination);
        Assert.Equal(layer == 0 ? 2112 + 1152 : 4224, destination.AY.Destination);
        Assert.Equal(layer == 0 ? 3456 : 1728 + 768, destination.BX.Destination);
        Assert.Equal(layer == 0 ? 4224 : 2112 + 1152, destination.BY.Destination);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(255)]
    public void UnequalPlaneFollowUsesMainPlaneAndScalesEachAxisSpeed(int layer)
    {
        var world = ParallaxWorld(layer);
        var view = ExplorationViewRunner.Initialize(world);
        view = view with { AX = new(384), AY = new(384), BX = new(384), BY = new(384) };
        var next = ExplorationViewRunner.Tick(world, view, new(2, 0, 0));
        Assert.Equal(layer == 0 ? 396 : 360, next.AX.Position);
        Assert.Equal(layer == 0 ? 396 : 360, next.AY.Position);
        Assert.Equal(layer == 0 ? 360 : 396, next.BX.Position);
        Assert.Equal(layer == 0 ? 360 : 396, next.BY.Position);
        Assert.Equal(layer == 0 ? 12 : 24, next.AX.Speed);
        Assert.Equal(layer == 0 ? 24 : 12, next.BX.Speed);
        Assert.Equal(1, next.FollowCounter);
        Assert.True(next.HideWindows);
        var clamped = ExplorationViewRunner.Initialize(world);
        var stopped = ExplorationViewRunner.Tick(world, clamped, new(2, 0, 0));
        Assert.False(stopped.Scrolling);
        Assert.Equal(0, stopped.FollowCounter);
    }

    [Fact]
    public void ParallaxAxisCompletionKeepsWindowHideUntilItsOwnFinalPass()
    {
        var world = ParallaxWorld(255);
        var view = ExplorationViewRunner.Initialize(world) with
        { TargetSlot = null, AX = new(0, 24), AY = new(0), BX = new(0, 36), BY = new(0) };
        var first = ExplorationViewRunner.Tick(world, view, new(2, 0, 0));
        Assert.Null(first.AX.Destination);
        Assert.Equal(12, first.BX.Position);
        Assert.True(first.HideWindows);
        var second = ExplorationViewRunner.Tick(world, first, new(2, 0, 0));
        Assert.False(second.HideWindows);
        Assert.True(second.BX.Active);
        var third = ExplorationViewRunner.Tick(world, second, new(2, 0, 0));
        Assert.False(third.Scrolling);
        Assert.Equal(36, third.BX.Position);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(255)]
    public void CoincidentOriginsWithDifferentPerAxisParallaxStillSeparateDuringScroll(int layer)
    {
        var world = ParallaxWorld(layer);
        var view = ExplorationViewRunner.Initialize(world);
        view = view with { Area = view.Area with
        { ForegroundX = 0, ForegroundY = 0, BackgroundX = 0, BackgroundY = 0,
            ParallaxAY = 256, ParallaxBY = 256 }, AX = new(0), AY = new(0), BX = new(0), BY = new(0) };
        var destination = ExplorationViewRunner.SetDestination(view, new(2, 3));
        Assert.Equal(layer == 0 ? 384 : 768, destination.AX.Destination);
        Assert.Equal(layer == 0 ? 768 : 384, destination.BX.Destination);
        Assert.Equal(1152, destination.AY.Destination);
        Assert.Equal(1152, destination.BY.Destination);
        var next = ExplorationViewRunner.Tick(world, destination, new(2, 0, 0));
        Assert.Equal(layer == 0 ? 12 : 24, next.AX.Position);
        Assert.Equal(layer == 0 ? 24 : 12, next.BX.Position);
        Assert.Equal(24, next.AY.Position);
        Assert.Equal(24, next.BY.Position);
    }

    private static ExplorationState ParallaxWorld(int layer) => Start("harbor-arrival", document =>
    {
        document["world"]!["maps"]![0]!["layout"] = new JsonArray(Enumerable.Range(0, 32)
            .Select(_ => (JsonNode)new JsonArray(Enumerable.Range(0, 32).Select(_ => (JsonNode)JsonValue.Create(0)!).ToArray())).ToArray());
        var area = document["world"]!["maps"]![0]!["areas"]![0]!;
        area["maxX"] = 31;
        area["maxY"] = 31;
        area["view"] = JsonNode.Parse($$"""
            {"foregroundX":{{(layer == 0 ? 2 : 0)}},"foregroundY":{{(layer == 0 ? 3 : 0)}},
             "backgroundX":{{(layer == 0 ? 0 : 2)}},"backgroundY":{{(layer == 0 ? 0 : 3)}},
             "parallaxAX":{{(layer == 0 ? 128 : 256)}},"parallaxAY":{{(layer == 0 ? 128 : 256)}},
             "parallaxBX":{{(layer == 0 ? 256 : 128)}},"parallaxBY":{{(layer == 0 ? 256 : 128)}},
             "autoscrollAX":0,"autoscrollAY":0,"autoscrollBX":0,"autoscrollBY":0,"layer":{{layer}}}
            """);
    }).Current.Exploration!;

    [Fact]
    public void RelativeDestinationRedispatchAllowsTimedReversalBeforeTheFirstDestination()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":true,"actions":[
              {"op":"flags","field":"a","mask":32,"value":0},
              {"op":"speed","x":32,"y":32},
              {"op":"move","x":1,"y":0,"wait":false},
              {"op":"wait","ticks":2},
              {"op":"move","x":-1,"y":0,"wait":false},
              {"op":"wait","ticks":2}]},{"op":"end"}]
            """);
        var origin = session.Current.Exploration!.Entities[new("ferryman")].Motion.X;
        foreach (int expected in new[] { 0, 32, 64, 32, 0 })
        {
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait?.Token));
            Assert.Equal(origin + expected, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        }
        Assert.Equal(origin - 320, session.Current.Exploration.Entities[new("ferryman")].Motion.XDestination);
        Assert.Null(session.Current.Exploration.Entities[new("ferryman")].Actions);
        Assert.Equal(SessionStopReason.SimulationWait, session.Current.StopReason);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 10));
        Assert.Equal(origin - 320, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void CoordinateBranchUsesFixedPointCoordinatesDuringMotion(bool whenEqual, bool taken)
    {
        var session = StartProgram($$$"""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
              {"op":"speed","x":32,"y":32},{"op":"move","x":1,"y":0}]},
             {"op":"wait-ticks","ticks":2},
             {"op":"branch-coordinates","entity":"ferryman","x":800,"y":384,
              "whenEqual":{{{whenEqual.ToString().ToLowerInvariant()}}},"target":{"program":"ledger","instruction":0}},
             {"op":"end"}]
            """);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 2));
        Assert.Equal(taken, session.Current.Story.Flags.Contains(12));
        Assert.Equal(800, session.Current.Exploration!.Entities[new("ferryman")].Motion.X);
    }

    [Fact]
    public void PriorityBelongsToTheEntityAndDoesNotRelocateIt()
    {
        var session = StartProgram("""
            [{"op":"priority","entity":"ferryman","value":true},
             {"op":"wait-ticks","ticks":1},
             {"op":"priority","entity":"ferryman","value":false},{"op":"end"}]
            """);
        var before = session.Current.Exploration!.Entities[new("ferryman")];
        Assert.True(before.Priority);
        Assert.False(session.Current.Exploration.PlayerEntity.Priority);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(before with { Priority = false }, session.Current.Exploration.Entities[new("ferryman")]);
    }

    [Theory]
    [InlineData("rebuild", false, true)]
    [InlineData("preserve", true, false)]
    public void OnlyARebuiltMapAppliesItsEntryFlagWritesBeforeItsInit(string mode, bool temporary, bool entered)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["flags"] = JsonNode.Parse("[256,383,384,604]");
            document["world"]!["maps"]![0]!["entryFlags"] = JsonNode.Parse("""
                [{"flag":256,"value":false},{"flag":383,"value":false},{"flag":80,"value":true}]
                """);
            document["world"]!["maps"]![0]!["onLoad"] = JsonNode.Parse("""
                {"program":"arrival","instruction":0}
                """);
            document["world"]!["programs"]![3]!["instructions"] = JsonNode.Parse("""
                [{"op":"branch-flag","flag":256,"whenSet":true,"target":{"program":"ledger","instruction":0}},
                 {"op":"set-flag","flag":14,"value":true},{"op":"end"}]
                """);
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse($$$"""
                [{"op":"transfer","map":"quay","position":{"x":1,"y":1},"facing":0,"loadMode":"{{{mode}}}"},
                 {"op":"end"}]
                """);
        });
        Accept(session, new Interact(new("ferryman")));
        Assert.Equal(temporary, session.Current.Story.Flags.Contains(256));
        Assert.Equal(temporary, session.Current.Story.Flags.Contains(383));
        Assert.Equal(entered, session.Current.Story.Flags.Contains(80));
        Assert.Equal(entered, session.Current.Story.Flags.Contains(14));
        Assert.Contains(384, session.Current.Story.Flags);
        Assert.Contains(604, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RebuildSelectsSetupBeforeTemporaryClearButCopiesLayoutAfterIt(bool alternateSetup)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["flags"] = JsonNode.Parse("[256]");
            var target = document["world"]!["maps"]![1]!;
            target["entryFlags"] = JsonNode.Parse("""[{"flag":256,"value":false}]""");
            target["layoutEvents"] = JsonNode.Parse("""
                {"doors":[],"roofs":[],"flags":[{"flag":256,"copy":{
                 "source":{"x":0,"y":0},"destination":{"x":1,"y":1},"width":1,"height":1}}]}
                """);
            if (alternateSetup) target["setup"] = JsonNode.Parse("""
                {"default":"base","variants":[{"flag":256,"setup":"alternate"}]}
                """);
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"transfer","map":"yard-map","position":{"x":1,"y":1},"facing":0,"loadMode":"rebuild"},
                 {"op":"end"}]
                """);
        });
        var result = Send(session, new Interact(new("ferryman")));
        if (alternateSetup)
        {
            Assert.Equal("map-setup", result.Failure!.Code);
            Assert.Equal("quay", session.Current.Exploration!.Map.Value);
            Assert.Contains(256, session.Current.Story.Flags);
        }
        else
        {
            Assert.Null(result.Failure);
            Assert.Equal("yard-map", session.Current.Exploration!.Map.Value);
            Assert.DoesNotContain(256, session.Current.Story.Flags);
            Assert.Contains(14, session.Current.Story.Flags);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BoundMapInitializationUsesLiveFlagsToSelectFirstOrRepeatVisit(bool seen)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", configure: document =>
        {
            document["start"]!["flags"] = seen ? new JsonArray(605) : new JsonArray();
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"transfer","map":"quay","position":{"x":1,"y":1},"facing":0,"loadMode":"rebuild"},{"op":"end"}]
                """);
            document["world"]!["maps"]![0]!["onLoad"] = JsonNode.Parse("""{"program":"initialize","instruction":0}""");
            var programs = document["world"]!["programs"]!.AsArray();
            programs.Add(JsonNode.Parse("""
                {"id":"initialize","instructions":[{"op":"branch-flag","flag":605,"whenSet":true,"target":{"program":"repeat","instruction":0}},
                {"op":"call","target":{"program":"first","instruction":0},"activateEntities":true},
                {"op":"set-flag","flag":605,"value":true},{"op":"end"}]}
                """));
            programs.Add(JsonNode.Parse("""
                {"id":"first","instructions":[{"op":"camera-target","position":{"x":3,"y":3}},
                {"op":"wait-view"},{"op":"set-flag","flag":604,"value":true},{"op":"end-map-script"}]}
                """));
            programs.Add(JsonNode.Parse("""{"id":"repeat","instructions":[{"op":"end"}]}"""));
        });
        Assert.Equal(seen ? SessionStopReason.PlayerInput : SessionStopReason.SimulationWait, session.Current.StopReason);
        for (int i = 0; session.Current.Story.Wait is ViewWait && i < 100; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(!seen, session.Current.Story.Flags.Contains(604));
        Assert.Contains(605, session.Current.Story.Flags);
        Assert.Empty(session.Current.Story.Callers);
        Assert.Null(session.Current.Story.Cursor);
        Assert.Null(session.Current.Story.EventCaller);
        Assert.True(session.Current.CanWaitAtInput);
    }

    private static GameSession StartProgram(string instructions) => Start("harbor-arrival", document =>
    {
        document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse(instructions);
        document["start"]!["program"] = JsonNode.Parse("""{"program":"invitation","instruction":0}""");
    });
}

using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class CastleTowerProgramTests
{
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

    private static GameSession StartProgram(string instructions) => Start("harbor-arrival", document =>
    {
        document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse(instructions);
        document["start"]!["program"] = JsonNode.Parse("""{"program":"invitation","instruction":0}""");
    });
}

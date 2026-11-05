using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class DepartureProgramTests
{
    [Fact]
    public void WarpInitCanReplaceTheSceneAndResumeItsCallerWithoutRunningTheSceneMapInit()
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", configure: document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""[{"op":"wait-ticks","ticks":1},{"op":"end"}]""");
            var target = document["world"]!["maps"]![1]!;
            target["basePalette"] = JsonNode.Parse("""{"color2":546,"color3":1092}""");
            target["onLoad"] = JsonNode.Parse("""{"program":"forbidden-init","instruction":0}""");
            document["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""{"id":"forbidden-init","instructions":[{"op":"set-flag","flag":999,"value":true},{"op":"end"}]}"""));
            document["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""{"id":"scene-test","instructions":[{"op":"scene-map","map":"yard-map","camera":{"x":3,"y":4}},{"op":"set-flag","flag":700,"value":true},{"op":"wait-ticks","ticks":1},{"op":"end"}]}"""));
        });
        var entry = session.Current;
        var warp = new OrdinaryWarp(new("yard-map"), new(1, 1), 1, MapLoadMode.Rebuild, true);
        var story = entry.Story.Copy(new("scene-test", 0), warp: warp,
            display: new(1, new(0xEEE, 0xAAA), new(0, 0), FullFadeVisibility.Black));
        var result = ProgramRunner.Run(session.Definition, entry.WithStory(story), []);
        Assert.Null(result.Failure);
        Assert.Equal(new MapId("yard-map"), result.Snapshot.Exploration!.Map);
        Assert.Contains(700, result.Snapshot.Story.Flags);
        Assert.DoesNotContain(999, result.Snapshot.Story.Flags);
        Assert.Same(warp, result.Snapshot.Story.Warp);
        Assert.IsType<TickWait>(result.Snapshot.Story.Wait);
    }

    [Fact]
    public void ScriptedPlacementCanLandOnBlockedLayoutButOrdinaryEntryAndAreaBoundsStayStrict()
    {
        var party = ExplorationTextWaitTests.StartFieldText("A{W1}").Current.Exploration!.Party;
        var words = new ushort[4096];
        words[2 * 64 + 2] = 0xC003;
        var map = new ExplorationMapDefinition(new("ship"), new WorkingMapLayout(words),
            new([new(0, 0, 4, 4)]), [], []);
        var ordinary = Assert.Throws<BattleRuleException>(() =>
            MapTransfer.Build(map, new("entity-0"), new(2, 2), 0, 32, party, []));
        Assert.Equal("map-entry-position", ordinary.Code);
        var scripted = MapTransfer.Build(map, new("entity-0"), new(2, 2), 0, 32, party, [], sourcePlacement: true);
        Assert.Equal(new MapPosition(2, 2), scripted.PlayerEntity.Position);
        Assert.Equal((ushort)0xC003, scripted.Layout[2, 2]);
        Assert.Throws<BattleRuleException>(() =>
            MapTransfer.Build(map, new("entity-0"), new(5, 2), 0, 32, party, [], sourcePlacement: true));
    }

    [Fact]
    public void AutomaticPlanesKeepSignedStepsIndependentOfCameraDestinationAndDoNotBlockWaits()
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}");
        var world = session.Current.Exploration!;
        var initial = session.Current.Story.LogicalView! with { TargetSlot = null };
        var view = ExplorationViewRunner.Autoscroll(initial, new(2, -2));
        view = ExplorationViewRunner.Position(view, new(2, 0));
        view = ExplorationViewRunner.SetDestination(view, new(5, 5));
        Assert.Null(view.BX.Destination);
        var first = ExplorationViewRunner.Tick(world, view, new(2, 0, 0));
        Assert.Equal(0, first.BX.Position);
        Assert.Equal(-2, first.BX.Destination);
        var second = ExplorationViewRunner.Tick(world, first, new(2, 0, 0));
        Assert.Equal(24574, second.BX.Position);
        Assert.Equal(2, second.BX.Speed);
        Assert.True(second.Scrolling); // the independent ordinary A camera still moves
        var onlyAuto = second with { AX = new(0), AY = new(0), BY = new(0) };
        Assert.False(onlyAuto.Scrolling);
        var stopped = ExplorationViewRunner.Autoscroll(onlyAuto, new(2, 2, true));
        Assert.Equal(0, stopped.Area.AutoscrollBX);
        Assert.True(stopped.Scrolling); // drain the already requested final destination
        var drained = ExplorationViewRunner.Tick(world, stopped, new(2, 0, 0));
        Assert.False(drained.Scrolling);
        Assert.Equal(24572, drained.BX.Position);
        Assert.Equal(world.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
    }

    [Fact]
    public void DirectPlanePositionDoesNotRetargetOtherAxesOrChangeTheSourceArea()
    {
        var view = ExplorationTextWaitTests.StartFieldText("A{W1}").Current.Story.LogicalView!;
        var moved = ExplorationViewRunner.Position(view, new(3, 0x3F60));
        Assert.Equal(view.AX, moved.AX); Assert.Equal(view.AY, moved.AY); Assert.Equal(view.BX, moved.BX);
        Assert.Equal(0x3F60, moved.BY.Position); Assert.Equal(view.Area, moved.Area);
    }
}

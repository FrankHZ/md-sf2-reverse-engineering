using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CastleSceneLifecycleTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CurrentMapReloadUsesTheWorkingLayoutIncludingAScriptedReset(bool reset)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", configure: document =>
        {
            var map = document["world"]!["maps"]![0]!;
            map["layout"]![0]![0] = 12;
            map["basePalette"] = JsonNode.Parse("""{"color2":2,"color3":4}""");
            document["start"]!["display"] = JsonNode.Parse(
                """{"period":3,"base":{"color2":2,"color3":4},"current":{"color2":2,"color3":4},"visibility":"base-restored"}""");
            var operations = JsonNode.Parse("""
                [{"op":"copy-layout","copy":{"source":{"x":0,"y":0},"destination":{"x":4,"y":2},"width":1,"height":1}},
                 {"op":"wait-ticks","ticks":1},{"op":"reload-scene-map","camera":{"x":2,"y":1}},{"op":"wait-ticks","ticks":10},{"op":"end"}]
                """)!.AsArray();
            if (reset) operations.Insert(1, JsonNode.Parse("""{"op":"reset-scene-map"}"""));
            document["world"]!["programs"]![0]!["instructions"] = operations;
        });
        var beforeReload = session.Current.Exploration!;
        Assert.Equal(beforeReload.PlayerEntity.Slot, session.Current.Story.LogicalView!.TargetSlot);
        Assert.Equal(reset ? 0 : 12, beforeReload.Layout[4, 2]);
        EngineTestContent.Accept(session, new AdvanceSimulation(Assert.IsType<TickWait>(session.Current.Story.Wait).Token));
        var world = session.Current.Exploration!;
        Assert.IsType<TickWait>(session.Current.Story.Wait);
        Assert.Equal(12, world.Definition.Layout[0, 0]);
        Assert.Equal(reset ? 0 : 12, world.Layout[4, 2]);
        Assert.Equal(reset, world.Layout.BlankBlockZero);
        Assert.Null(session.Current.Story.LogicalView!.TargetSlot);
        Assert.Equal(2 * 384, session.Current.Story.LogicalView.BX.Position);
    }

    [Theory]
    [InlineData(6, 37, false)]
    [InlineData(23, 37, true)]
    public void UncompletedBedroomRetainsItsCastUntilTheActualFirstSceneRuns(int x, int y, bool firstScene)
    {
        var (definition, party) = Content();
        var session = Start(definition, party, new(x, y), []);
        Assert.Equal(firstScene, session.Current.Story.Flags.Contains(605));
        Assert.Equal(firstScene, session.Current.Story.Flags.Contains(900));
        Assert.Equal(!firstScene, session.Current.Exploration!.TryResolveEntity(new("entity-130"), out _));
        Assert.True(session.Current.Exploration.Entities[new("entity-128")].Visible);
    }

    [Theory]
    [InlineData(6, 37, false, false)]
    [InlineData(36, 40, false, true)]
    [InlineData(23, 37, true, false)]
    [InlineData(25, 50, true, true)]
    public void CompletedBedroomRetiresOnlyItsSceneActorAcrossEntrancesAndLiveFollowerCounts(
        int x, int y, bool departed, bool followers)
    {
        var (definition, party) = Content();
        int[] flags = [605, 607, .. departed ? new[] { 608 } : [], .. followers ? new[] { 66 } : []];
        var session = Start(definition, party, new(x, y), flags);
        var world = session.Current.Exploration!;
        var retired = world.AllEntities.Single(entity => entity.Entity == new EntityRef("entity-130"));
        Assert.Equal(followers ? 5 : 3, retired.Slot);
        Assert.False(retired.Visible);
        Assert.False(world.TryResolveEntity(new("entity-130"), out _));
        Assert.True(world.Entities[new("entity-128")].Visible);
        Assert.True(world.Entities[new("entity-129")].Visible);
        Assert.DoesNotContain(900, session.Current.Story.Flags);
        Assert.Equal(flags.Order(), session.Current.Story.Flags);
        Assert.Equal(followers ? 2 : 0, world.AllEntities.Count(entity => entity.Follower is not null));
        Assert.Same(party, world.Party);
    }

    [Fact]
    public void RetiredBedroomActorCannotBeTalkedToOrBlockItsFormerTile()
    {
        var (definition, party) = Content();
        var session = Start(definition, party, new(18, 39), [605, 608]);
        Assert.Null(session.QueryExplorationInteraction());
        EngineTestContent.Accept(session, new Move(ExplorationDirection.East));
        while (session.Current.Story.Wait is EntityWait wait)
            EngineTestContent.Accept(session, new AdvanceSimulation(wait.Token, 60));
        Assert.Equal(new MapPosition(19, 39), session.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void PreservedStairsAndRebuiltReentryCannotReviveTheRetiredBedroomActor()
    {
        var (definition, party) = Content();
        var session = Start(definition, party, new(6, 37), [605, 608, 66]);
        var current = session.Current;
        var guard = current.Exploration!.Entities[new("entity-128")];
        foreach (var mode in new[] { MapLoadMode.Preserve, MapLoadMode.Preserve, MapLoadMode.Rebuild })
        {
            current = Enter(definition, current, new("map-20"), new(25, 50), mode);
            Assert.False(current.Exploration!.TryResolveEntity(new("entity-130"), out _));
            Assert.Equal(guard.Position, current.Exploration.Entities[guard.Entity].Position);
            Assert.True(current.Exploration.Entities[guard.Entity].Visible);
            Assert.Equal(SessionStopReason.PlayerInput, current.StopReason);
        }
        current = Enter(definition, current, new("map-21"), new(3, 16), MapLoadMode.Rebuild);
        Assert.True(current.Exploration!.Entities[new("entity-130")].Visible);
        Assert.Equal(new MapPosition(7, 4), current.Exploration.Entities[new("entity-130")].Position);
        current = Enter(definition, current, new("map-20"), new(6, 37), MapLoadMode.Rebuild);
        Assert.False(current.Exploration!.TryResolveEntity(new("entity-130"), out _));
    }

    [Fact]
    public void LaterSetupKeepsItsOwnAstralEvenWhenTheEarlierSceneFlagIsStillSet()
    {
        var (definition, party) = Content();
        var session = Start(definition, party, new(6, 37), [605, 609]);
        Assert.True(session.Current.Exploration!.Entities[new("entity-130")].Visible);
        Assert.Equal(new MapPosition(18, 39), session.Current.Exploration.Entities[new("entity-130")].Position);
        var entered = Enter(definition, session.Current, new("map-20"), new(23, 37), MapLoadMode.Rebuild);
        Assert.True(entered.Exploration!.Entities[new("entity-130")].Visible);
        Assert.Equal(new MapPosition(18, 39), entered.Exploration.Entities[new("entity-130")].Position);
    }

    private static SessionSnapshot Enter(ScenarioDefinition definition, SessionSnapshot current, MapId map,
        MapPosition position, MapLoadMode mode)
    {
        var next = MapTransfer.Apply(definition, current, map, position, 0, mode, current.Story, [], RuleCompositions.Sf2());
        var result = ProgramRunner.Run(definition, next, [], RuleCompositions.Sf2());
        Assert.Null(result.Failure);
        return result.Snapshot;
    }

    private static GameSession Start(ScenarioDefinition definition, Sf2.Remake.Domain.Battles.BattleStartInput party,
        MapPosition position, int[] flags) => Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(new("map-20"), new("entity-0"), position, 0, 32, flags, party))).Session;

    private static (ScenarioDefinition Definition, Sf2.Remake.Domain.Battles.BattleStartInput Party) Content()
    {
        var battle = EngineTestContent.Admitted();
        var layout = new WorkingMapLayout(new ushort[4096]);
        var traversal = new OriginalMapTraversal([new(0, 0, 63, 63)]);
        ExplorationEntityDefinition Actor(int id, int sprite, int x, int y) => new(new("entity-" + id), new(x, y), 0, 32, true, false, sprite);
        var population = new ExplorationPopulation(30, 128, 0, [new(66, 1, 1), new(66, 2, 2)]);
        var map = new ExplorationMapDefinition(new("map-20"), layout, traversal,
            [Actor(128, 206, 10, 10), Actor(129, 208, 20, 39), Actor(130, 209, 19, 39)], [],
            onLoad: new("ms-map20-initfunction", 0), population: population,
            setup: new(new("default"), [new(new("609"), new("later"))]),
            setups: [new(new("later"), [Actor(128, 206, 10, 10), Actor(129, 208, 20, 39), Actor(130, 209, 18, 39)], [],
                new("return", 0), null, population)]);
        var next = new ExplorationMapDefinition(new("map-21"), layout, traversal,
            [Actor(128, 206, 5, 16), Actor(129, 208, 9, 4), Actor(130, 209, 7, 4)], [], population: population);
        List<StoryProgram> programs = [
            new("ms-map20-initfunction", [
                new BranchEntityCoordinates(new("entity-0"), 23 * 384, 37 * 384, false, new("return", 0)),
                new BranchFlag(605, true, new("hide", 0)), new CallProgram(new("first-scene", 0)),
                new WriteFlag(605, true), new JumpProgram(new("return", 0))],
                "disasm/data/maps/entries/map20/mapsetups/s6_initfunction.asm:ms_map20_InitFunction", false),
            new("first-scene", [new WriteFlag(900, true), new HideMapEntity(new("entity-130"), true), new ReturnProgram()]),
            new("hide", [new HideMapEntity(new("entity-130"), true), new ReturnProgram()]),
            new("return", [new ReturnProgram()]),
        ];
        PrivateCastleSceneContent.Bind([map, next], programs);
        return (new("castle-scene-content", battle.Definition.Encounters.Values,
            exploration: new([map, next], programs)), battle.Start);
    }
}

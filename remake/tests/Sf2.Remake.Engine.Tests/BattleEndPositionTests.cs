using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleEndPositionTests
{
    [Fact]
    public void SceneReturnHidesAlliesClearsThirtyEnemyHpSlotsAndOnlyMovesTheTailX()
    {
        var before = Battle(gateAlive: false);
        var after = BattleEndPositions.Apply(before);
        Assert.Equal(new MapPosition(4, 2), after.GetActor(new("medic-a")).Position);
        Assert.Equal(before.GetActor(new("guard-a")).Hp, after.GetActor(new("guard-a")).Hp);
        Assert.Null(after.GetActor(new("guard-a")).Position);
        Assert.Equal(new MapPosition(5, 2), after.GetActor(new("dummy-a")).Position);
        Assert.Equal(0, after.GetActor(new("other")).Hp);
        Assert.Null(after.GetActor(new("other")).Position);
        Assert.Equal(before.GetActor(new("tail")).Hp, after.GetActor(new("tail")).Hp);
        Assert.Equal(new MapPosition(0, 5), after.GetActor(new("tail")).Position);
        Assert.Equal(before.MainSeed, after.MainSeed);
        Assert.Equal(before.Gold, after.Gold);
        Assert.All(after.Actors, a => Assert.Equal(before.GetActor(a.Actor).Kills, a.Kills));
        // A surviving slot158 prevents ordinary faction victory after the helper.
        Assert.Null(BattleOutcomeRules.Check(after));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AVisibleLeaderAndDeadGateAreBothRequired(bool gateAlive, bool leaderDead)
    {
        var before = Battle(gateAlive);
        if (leaderDead) before = before.With(actors: before.Actors.Select(a => a.Actor == new ActorRef("medic-a") ? a.With(hp: 0) : a));
        Assert.Same(before, BattleEndPositions.Apply(before));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DefeatedSceneSuspendsDeathRemovalButTheCompletedFlagSkipsOnlyTheProgram(bool completed)
    {
        var battle = BattleEndPositions.Apply(Battle(false)).With(firstAlly: new ActorRef("medic-a"));
        var route = new ExplorationBattleRoute(battle.Definition.Encounter, 405, 505, 455, null, null,
            Outcome: new(new("after", 0), 0, new("defeated", 0), new("defeat", 0), new("return", 0),
                new("castle"), 3, new("castle"), new(2, 3), 1, DefeatedScene: true));
        var story = new StoryState(completed ? [405, 505] : [405], enteringBattle: route);
        var before = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null), story, SessionStopReason.SimulationWait);
        List<SessionObservation> events = [];
        var after = BattleActionCommitter.Publish(before, battle, new("medic-a"), new(4, 2), [], events,
            RuleCompositions.Sf2(), sceneEnded: true, deaths: new([new("dummy-a")]));
        Assert.Equal(new MapPosition(5, 2), after.Battle.GetActor(new("dummy-a")).Position);
        Assert.Equal(before.Battle.GetActor(new("medic-a")).Kills, after.Battle.GetActor(new("medic-a")).Kills);
        Assert.Null(after.Battle.GetActor(new("other")).Position);
        Assert.Equal(0, after.Battle.GetActor(new("other")).Hp);
        Assert.False(after.HasBattleControl);
        if (completed)
        {
            Assert.Null(after.Story.DefeatedScene);
            Assert.NotNull(after.BattleScene);
            Assert.Contains(events, e => e.Kind == "enemy-defeated-program-skipped");
        }
        else
        {
            Assert.NotNull(after.Story.DefeatedScene);
            Assert.Null(after.BattleScene);
            Assert.Single(after.Story.DefeatedScene.Deaths.Actors);
            Assert.Contains(events, e => e.Kind == "enemy-defeated-program-pending");
            Assert.DoesNotContain(events, e => e.Kind == "field-death-started");
        }
    }

    [Fact]
    public void DefeatedSceneProjectsSourcePositionsThenResumesTheOriginalDeathWork()
    {
        var battle = BattleEndPositions.Apply(Battle(false)).With(firstAlly: new ActorRef("medic-a"));
        var map = new ExplorationMapDefinition(new("castle"), new(new ushort[WorkingMapLayout.WordCount]),
            new OriginalMapTraversal([new(0, 0, 63, 63)]), [], [], population: new(30, 128, 0, []));
        var world = new ExplorationDefinition([map],
            [new("defeated", [new SetEntityPosition(new("entity-7"), new(9, 11), 3), new WaitProgramTicks(1), new EndProgram(true)])]);
        var scene = new BattleSceneDefinition(battle.Definition.Encounter, "background", "ground",
            new Dictionary<string, ExplorationRaster>(), new Dictionary<BattleClassRule, BattleSceneActorVisual>(),
            new Dictionary<ActorRef, BattleSceneActorVisual>(), new Dictionary<int, string>(), [],
            FieldDeath: new(new Dictionary<int, int>(), new Dictionary<ActorRef, int> { [new("dummy-a")] = 30, [new("tail")] = 31 }, []));
        var definition = new ScenarioDefinition("projection", [battle.Definition], exploration: world, battleScenes: scene);
        var route = new ExplorationBattleRoute(battle.Definition.Encounter, 405, 505, 455, null, null,
            Outcome: new(new("after", 0), 0, new("defeated", 0), new("defeat", 0), new("return", 0),
                map.Map, 3, map.Map, new(2, 3), 1, BattleOrigin: new(10, 12), DefeatedScene: true));
        var story = new StoryState([], enteringBattle: route, fieldPlayer: new(new("entity-0"), 32),
            defeatedScene: new(battle, new("medic-a"), new(4, 2), new([new("dummy-a")])));
        var before = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null), story, SessionStopReason.SimulationWait);
        var result = BattleDefeatedProgram.Begin(definition, new(before, [], before.StopReason), RuleCompositions.Sf2());
        Assert.Null(result.Failure);
        var field = result.Snapshot;
        Assert.Equal(new MapPosition(9, 11), field.Exploration!.PlayerEntity.Position);
        Assert.Equal(new MapPosition(15, 14), field.Exploration.Entities[new("entity-128")].Position);
        Assert.False(field.CanWaitAtInput);
        Assert.Null(field.Story.DefeatedScene!.CompletedWorld);
        result = ExplorationDispatcher.Submit(definition, field, new AdvanceSimulation(field.Story.Wait!.Token));
        Assert.Null(result.Failure);
        var resumed = result.Snapshot;
        Assert.Equal(SessionMode.Battle, resumed.Mode);
        Assert.NotNull(resumed.BattleScene);
        Assert.Equal(new MapPosition(9, 11), resumed.Story.DefeatedScene!.CompletedWorld!.PlayerEntity.Position);
        Assert.Equal(new MapPosition(4, 2), resumed.Battle.GetActor(new("medic-a")).Position);
        Assert.Equal(new MapPosition(5, 2), resumed.Battle.GetActor(new("dummy-a")).Position);
        Assert.Equal(battle.Gold, resumed.Battle.Gold);
        Assert.Equal(battle.GetActor(new("medic-a")).Kills, resumed.Battle.GetActor(new("medic-a")).Kills);
        Assert.Contains(result.Observations, e => e.Kind == "enemy-defeated-program-finished");
        Assert.Contains(result.Observations, e => e.Kind == "field-death-started");
    }

    private static EngineBattleState Battle(bool gateAlive)
    {
        var session = Start(change: d =>
        {
            d["encounters"]![0]!["placements"]![2]!["processingOrder"] = 128;
            foreach (var row in new[] { ("other", 129, 9), ("tail", 158, 10) })
            {
                var actor = d["actors"]![2]!.DeepClone(); actor["id"] = row.Item1; d["actors"]!.AsArray().Add(actor);
                var placement = d["encounters"]![0]!["placements"]![2]!.DeepClone();
                placement["actor"] = row.Item1; placement["processingOrder"] = row.Item2; placement["x"] = row.Item3;
                d["encounters"]![0]!["placements"]!.AsArray().Add(placement);
                var input = d["start"]!["actors"]![2]!.DeepClone(); input["actor"] = row.Item1; d["start"]!["actors"]!.AsArray().Add(input);
            }
        });
        var b = session.Current.Battle;
        var definition = b.Definition.WithOutcome(new(new("medic-a"), new("dummy-a"), new Dictionary<int, MapPosition>
            { [7] = new(4, 2), [128] = new(5, 2) }));
        return new(definition, b.Actors.Select(a => a.Actor == new ActorRef("dummy-a") ? a.With(hp: (ushort)(gateAlive ? 5 : 0)) : a),
            b.MainSeed, b.ThinkingSeed, b.Round, b.Queue, b.Cursor, b.Gold);
    }
}

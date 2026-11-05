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

    [Theory]
    [InlineData(10, 12)]
    [InlineData(0, 14)]
    public void DefeatedScenePosesSurviveDeathDeliveryWithoutChangingCombatAuthority(int originX, int originY)
    {
        var (definition, result) = DefeatedSceneBoundary(originX, originY);
        var field = result.Snapshot;
        Assert.False(field.CanWaitAtInput);
        Assert.Null(field.BattleFieldScene);
        Assert.Null(field.Story.DefeatedScene!.CompletedWorld);
        var before = field.Story.DefeatedScene.Battle;
        result = ExplorationDispatcher.Submit(definition, field, new AdvanceSimulation(field.Story.Wait!.Token));
        Assert.Null(result.Failure);
        Assert.Contains(result.Observations, e => e.Kind == "enemy-defeated-program-finished");
        Assert.Contains(result.Observations, e => e.Kind == "field-death-started");
        var phases = new HashSet<BattleScenePhase>();
        while (true)
        {
            var current = result.Snapshot;
            var death = current.BattleScene!;
            phases.Add(death.Phase);
            var projection = Assert.IsType<ExplorationState>(current.BattleFieldScene);
            Assert.Equal(new MapPosition(originX + 6, originY + 6), projection.Entities[new("entity-7")].Position);
            Assert.Equal(1, projection.Entities[new("entity-7")].Motion.Facing);
            Assert.Equal(new MapPosition(originX + 8, originY + 4), projection.Entities[new("entity-9")].Position);
            Assert.Equal(2, projection.Entities[new("entity-9")].Motion.Facing);
            Assert.Equal(new MapPosition(originX + 7, originY + 4), projection.Entities[new("entity-128")].Position);
            Assert.Equal(new MapPosition(4, 2), current.Battle.GetActor(new("medic-a")).Position);
            Assert.Equal(new MapPosition(6, 2), current.Battle.GetActor(new("guard-a")).Position);
            Assert.Equal(before.Queue, current.Battle.Queue);
            Assert.Equal(before.Cursor, current.Battle.Cursor);
            Assert.Equal(before.Gold, current.Battle.Gold);
            foreach (var actor in before.Actors)
            {
                var actual = current.Battle.GetActor(actor.Actor);
                Assert.Equal(actor.Hp, actual.Hp);
                Assert.Equal(actor.Mp, actual.Mp);
                Assert.Equal(actor.Exp, actual.Exp);
            }
            if (death.Phase == BattleScenePhase.FieldSettle)
            {
                Assert.Null(current.Battle.GetActor(new("dummy-a")).Position);
                Assert.Equal(before.GetActor(new("medic-a")).Kills + 1, current.Battle.GetActor(new("medic-a")).Kills);
                var finished = BattleFieldDeathContinuation.Submit(current,
                    new CompletePresentation(death.Token, death.CompletionKind), RuleCompositions.Sf2());
                var after = BattleOutcome.Begin(definition, finished, RuleCompositions.Sf2());
                Assert.Null(after.Failure);
                Assert.Equal("after", after.Snapshot.Story.Cursor!.Value.Program);
                Assert.Null(after.Snapshot.BattleFieldScene);
                foreach (int slot in new[] { 7, 9 })
                {
                    var entity = new EntityRef("entity-" + slot);
                    Assert.Equal(projection.Entities[entity].Motion, after.Snapshot.Exploration!.Entities[entity].Motion);
                }
                Assert.False(after.Snapshot.Exploration!.Entities[new("entity-128")].Visible);
                break;
            }
            Assert.Equal(before.GetActor(new("medic-a")).Kills, current.Battle.GetActor(new("medic-a")).Kills);
            Assert.Equal(new MapPosition(5, 2), current.Battle.GetActor(new("dummy-a")).Position);
            result = BattleFieldDeathContinuation.Submit(current, new CompletePresentation(death.Token, death.CompletionKind), RuleCompositions.Sf2());
            Assert.Null(result.Failure);
        }
        Assert.Contains(BattleScenePhase.FieldSpin, phases);
        Assert.Contains(BattleScenePhase.FieldExit, phases);
    }

    private static (ScenarioDefinition Definition, SessionResult Started) DefeatedSceneBoundary(int originX, int originY)
    {
        var battle = BattleEndPositions.Apply(Battle(false, stageCompanion: true)).With(firstAlly: new ActorRef("medic-a"));
        battle = battle.With(actors: battle.Actors.Select(actor => actor.Actor == new ActorRef("tail")
            ? actor.With(hp: 0, clearPosition: true) : actor));
        battle = battle.With(visuals: new BattleVisualState(
            battle.Actors.Select((actor, slot) => (actor.Actor, slot)).ToDictionary(row => row.Actor, row => row.slot),
            battle.Actors.Select((actor, slot) => (actor, slot)).Where(row => row.actor.Position is not null)
                .Select(row => new BattleVisual(row.slot, row.actor.Actor, row.actor.Position!, 3))));
        var map = new ExplorationMapDefinition(new("castle"), new(new ushort[WorkingMapLayout.WordCount]),
            new OriginalMapTraversal([new(0, 0, 63, 63)]), [], [], population: new(30, 128, 0, []));
        var world = new ExplorationDefinition([map],
            [new("defeated", [new SetEntityPosition(new("entity-7"), new(originX + 6, originY + 6), 1),
                new SetEntityPosition(new("entity-9"), new(originX + 8, originY + 4), 2),
                new SetEntityPosition(new("entity-128"), new(originX + 7, originY + 4), 0),
                new WaitProgramTicks(1), new EndProgram(true)]),
             new("after", [new WaitProgramTicks(1), new EndProgram()])]);
        var scene = new BattleSceneDefinition(battle.Definition.Encounter, "background", "ground",
            new Dictionary<string, ExplorationRaster>(), new Dictionary<BattleClassRule, BattleSceneActorVisual>(),
            new Dictionary<ActorRef, BattleSceneActorVisual>(), new Dictionary<int, string>(), [],
            FieldDeath: new(new Dictionary<int, int>(), new Dictionary<ActorRef, int> { [new("dummy-a")] = 30, [new("tail")] = 31 }, []));
        var definition = new ScenarioDefinition("projection", [battle.Definition], exploration: world, battleScenes: scene);
        var route = new ExplorationBattleRoute(battle.Definition.Encounter, 405, 505, 455, null, null,
            Outcome: new(new("after", 0), 0, new("defeated", 0), new("defeat", 0), new("return", 0),
                map.Map, 3, map.Map, new(2, 3), 1, BattleOrigin: new(originX, originY), DefeatedScene: true));
        var story = new StoryState([], enteringBattle: route, fieldPlayer: new(new("entity-0"), 32),
            defeatedScene: new(battle, new("medic-a"), new(4, 2), new([new("dummy-a")])));
        var before = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null), story, SessionStopReason.SimulationWait);
        var result = BattleDefeatedProgram.Begin(definition, new(before, [], before.StopReason), RuleCompositions.Sf2());
        Assert.Null(result.Failure);
        return (definition, result);
    }

    private static EngineBattleState Battle(bool gateAlive, bool stageCompanion = false)
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
        var positions = new Dictionary<int, MapPosition> { [7] = new(4, 2), [128] = new(5, 2) };
        if (stageCompanion) positions[9] = new(6, 2);
        var definition = b.Definition.WithOutcome(new(new("medic-a"), new("dummy-a"), positions));
        return new(definition, b.Actors.Select(a => a.Actor == new ActorRef("dummy-a") ? a.With(hp: (ushort)(gateAlive ? 5 : 0)) : a),
            b.MainSeed, b.ThinkingSeed, b.Round, b.Queue, b.Cursor, b.Gold);
    }
}

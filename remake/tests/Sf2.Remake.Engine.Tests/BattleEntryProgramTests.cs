using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleEntryProgramTests
{
    [Fact]
    public void SceneReplacementWaitsForEveryPhysicalSpriteAndCameraAndGestureUseTheNewAlias()
    {
        var accepted = Assert.IsType<ExplorationReadAccepted>(new AuthoredScenarioPackageReader(PathFor("harbor-arrival")).Read());
        var basis = accepted.Definition.Exploration!.Maps[new("quay")];
        var population = new ExplorationPopulation(30, 128, 0, [new(41, 2, 5)]);
        var source = new ExplorationMapDefinition(new("approach"), basis.Layout, basis.Traversal, [], [],
            onLoad: new("scene", 0), population: population);
        var target = new ExplorationMapDefinition(new("stage"), basis.Layout, basis.Traversal, [], [],
            onLoad: new("forbidden-init", 0), population: population, entryFlags: [new(91, true)]);
        var records = Enumerable.Range(0, 8).Select(index => new ExplorationEntityDefinition(new("entity-" + (128 + index)),
            new(2, 1), 3, 32, Sprite: 80 + index)).ToArray();
        StoryInstruction[] instructions = [new LoadSceneMap(target.Map, new(0, 0)),
            new LoadSceneEntities(population, new(1, 1), 1, records), new SetCameraEntity(new("entity-135")),
            new WaitProgramTicks(1), new SetCameraTarget(new(0, 0)),
            new PresentCue(PresentationCueKind.Gesture, "shiver", new("entity-135"), null), new WaitProgramTicks(1), new EndProgram()];
        var world = new ExplorationDefinition([source, target], [new("scene", instructions), new("forbidden-init", [new WriteFlag(92, true), new EndProgram()])]);
        var definition = new ScenarioDefinition("scene-replacement", accepted.Definition.Encounters.Values, exploration: world);
        var started = Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(source.Map, new("entity-0"), new(1, 1), 1, 32, [41, 90], accepted.Start.Party)));
        var session = started.Session;
        Assert.IsType<EntitySetSpriteWait>(session.Current.Story.Wait);
        var entities = session.Current.Exploration!;
        Assert.Equal(target.Map, entities.Map);
        Assert.Equal(new[] { 41, 90 }, session.Current.Story.Flags);
        Assert.Equal(10, entities.AllEntities.Count);
        Assert.Equal(9, entities.Entities[new("entity-135")].Slot);
        Assert.NotSame(entities.PlayerEntity, entities.Entities[new("entity-135")]);
        Assert.Single(entities.AllEntities, entity => entity.Follower is not null);
        foreach (var entity in entities.AllEntities.Reverse())
        {
            var before = session.Current;
            Assert.NotNull(Send(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest + 1)).Failure);
            Assert.Same(before, session.Current);
            Accept(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest));
            if (entity != entities.AllEntities[0]) Assert.IsType<EntitySetSpriteWait>(session.Current.Story.Wait);
        }
        Assert.Equal(9, session.Current.Story.CameraEntitySlot);
        var counter = session.Current.Exploration!.Entities[new("entity-135")].Motion.AnimationCounter;
        var size = session.Current.Exploration.SpriteSize;
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Null(session.Current.Story.CameraEntitySlot);
        var wait = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(255, session.Current.Exploration.Entities[new("entity-135")].Motion.AnimationCounter);
        Assert.Equal(21, session.Current.Exploration.SpriteSize);
        var held = session.Current;
        Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure);
        Assert.Same(held, session.Current);
        Accept(session, new CompletePresentation(wait.Token, wait.Cue.Kind));
        Assert.Equal(counter, session.Current.Exploration.Entities[new("entity-135")].Motion.AnimationCounter);
        Assert.Equal(size, session.Current.Exploration.SpriteSize);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InitLoadAndStartKeepInputAndIntroFlagBehindTheTypedLoadService(bool seen)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["map"] = "yard-map";
            document["start"]!["flags"] = JsonNode.Parse(seen ? "[13,20]" : "[13]");
            document["world"]!["maps"]![1]!["battle"]!["load"] = JsonNode.Parse("""{"program":"load-board","instruction":0}""");
            document["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""
                {"id":"load-board","instructions":[{"op":"present","kind":"BattleLoad","resource":null,"entity":null,"position":null},{"op":"end"}]}
                """));
        });
        if (!seen) Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        var wait = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(PresentationCueKind.BattleLoad, wait.Cue.Kind);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.False(session.Current.HasBattleControl);
        Assert.Null(session.Current.Selection);
        Assert.Equal(0, session.Current.Battle.Round);
        Assert.Equal(seen, session.Current.Story.Flags.Contains(20));
        var held = session.Current;
        Assert.NotNull(Send(session, new Confirm()).Failure);
        Assert.Same(held, session.Current);
        Assert.NotNull(Send(session, new CompletePresentation(wait.Token, PresentationCueKind.FadeIn)).Failure);
        Assert.Same(held, session.Current);
        Accept(session, new CompletePresentation(wait.Token, wait.Cue.Kind));
        Assert.True(session.Current.HasBattleControl);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.Contains(20, session.Current.Story.Flags);
        Assert.Equal(!seen, session.Current.Story.Flags.Contains(16));
        Assert.NotNull(session.Current.Selection);
    }

    [Fact]
    public void CompletedBattleClearsItsUnlockAndReturnsToFieldWithoutRunningTheBeforeBody()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["map"] = "yard-map";
            document["start"]!["flags"] = JsonNode.Parse("[13,21]");
        });
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.DoesNotContain(13, session.Current.Story.Flags);
        Assert.DoesNotContain(15, session.Current.Story.Flags);
        Assert.DoesNotContain(20, session.Current.Story.Flags);
    }

    [Fact]
    public void RejectedInitializationRetainsTheCompletedBeforeProgramWithoutPublishingBattleOrIntroState()
    {
        var accepted = Assert.IsType<ExplorationReadAccepted>(new AuthoredScenarioPackageReader(PathFor("harbor-arrival")).Read());
        var party = accepted.Start.Party;
        var invalid = new BattleStartInput(party.Encounter, party.Actors.Select((actor, index) => index == 0 ? actor with { Hp = ushort.MaxValue } : actor),
            party.MainSeed, party.ThinkingSeed, party.Gold);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(accepted.Definition,
            new ExplorationStartInput(new("yard-map"), accepted.Start.Player, new(1, 1), 0, 32, [13, 90], invalid))).Session;
        var active = session.Current.Active;
        Assert.Contains(15, session.Current.Story.Flags);
        var result = Send(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Equal("numeric-range", result.Failure!.Code);
        Assert.Same(active, session.Current.Active);
        Assert.Contains(90, session.Current.Story.Flags);
        Assert.DoesNotContain(20, session.Current.Story.Flags);
        Assert.DoesNotContain(result.Observations, row => row.Kind is "battle-initialized" or "battle-loaded");
        Assert.Equal(party.MainSeed, session.Current.Exploration!.Party.MainSeed);
    }
}

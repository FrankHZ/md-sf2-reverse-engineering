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
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BoundSceneFadeLoadFadeUsesNewBaseAndKeepsCameraEntityUnsupported(bool overridePeriod)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", configure: document =>
        {
            document["world"]!["maps"]![1]!["basePalette"] = JsonNode.Parse("""{"color2":546,"color3":1092}""");
            document["start"]!["display"] = JsonNode.Parse("""{"period":3,"base":{"color2":3822,"color3":2730},"current":{"color2":3822,"color3":2730},"visibility":"base-restored"}""");
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"present","kind":"FadeOut","resource":"black","entity":null,"position":null,"fullBlack":{"period":1}},
                {"op":"scene-map","map":"yard-map","camera":{"x":3,"y":7}},
                {"op":"present","kind":"FadeIn","resource":"black","entity":null,"position":null,"fullBlack":{"period":1}},
                {"op":"wait-ticks","ticks":1},{"op":"camera-entity","entity":"traveler"},{"op":"end"}]
                """);
            if (!overridePeriod)
                foreach (var instruction in document["world"]!["programs"]![0]!["instructions"]!.AsArray())
                    instruction!.AsObject().Remove("fullBlack");
        });
        var fade = Assert.IsType<FullFadeWait>(session.Current.Story.Wait);
        Assert.Equal(overridePeriod ? 1 : 3, fade.Period);
        Accept(session, new AdvanceSimulation(fade.Token, 600));
        Assert.Equal("quay", session.Current.Exploration!.Map.Value);
        Assert.Equal(new PalettePair(0, 0), session.Current.Story.Display!.Current);
        Accept(session, new CompletePresentation(fade.Token, fade.Kind));
        Assert.Equal("yard-map", session.Current.Exploration!.Map.Value);
        var second = Assert.IsType<FullFadeWait>(session.Current.Story.Wait);
        Assert.Equal(new PalettePair(0x222, 0x444), session.Current.Story.Display!.Base);
        Assert.Equal(new PalettePair(0, 0), session.Current.Story.Display.Current);
        Assert.Null(session.Current.Story.EntityServices);
        Assert.Equal(3 * 384, session.Current.Story.LogicalView!.BX.Position);
        Accept(session, new AdvanceSimulation(second.Token, 600));
        Assert.Equal(session.Current.Story.Display.Base, session.Current.Story.Display.Current);
        Assert.Equal(FullFadeVisibility.BaseRestored, session.Current.Story.Display.Visibility);
        Accept(session, new CompletePresentation(second.Token, second.Kind));
        Assert.Equal(3, session.Current.Story.Display.Period);
        Assert.False(session.Current.CanWaitAtInput);
        var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal("field-view-camera-command", result.Failure!.Code);
        Assert.Null(session.Current.Story.LogicalView.TargetSlot);
    }

    [Theory]
    [InlineData(0, 3, 7)]
    [InlineData(255, 11, 4)]
    public void BoundBlackSceneUsesExplicitAreaOriginAndPreservesCallerPartyAndCounter(int layer, int x, int y)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", configure: document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""[{"op":"wait-ticks","ticks":1},{"op":"end"}]""");
            document["start"]!["player"] = "entity-0";
            var stage = document["world"]!["maps"]![1]!;
            stage["basePalette"] = JsonNode.Parse("""{"color2":546,"color3":1092}""");
            stage["onLoad"] = JsonNode.Parse("""{"program":"forbidden-init","instruction":0}""");
            stage["areas"]![0]!["view"] = JsonNode.Parse(layer == 0
                ? """{"foregroundX":2,"foregroundY":3,"backgroundX":0,"backgroundY":0,"parallaxAX":128,"parallaxAY":256,"parallaxBX":256,"parallaxBY":256,"autoscrollAX":0,"autoscrollAY":0,"autoscrollBX":0,"autoscrollBY":0,"layer":0}"""
                : """{"foregroundX":0,"foregroundY":0,"backgroundX":4,"backgroundY":5,"parallaxAX":256,"parallaxAY":256,"parallaxBX":128,"parallaxBY":128,"autoscrollAX":0,"autoscrollAY":0,"autoscrollBX":0,"autoscrollBY":0,"layer":255}""");
            var retainedPlayerArea = stage["areas"]![0]!.DeepClone();
            retainedPlayerArea["maxX"] = 2;
            retainedPlayerArea["maxY"] = 2;
            stage["areas"]![0]!["minX"] = 3;
            stage["areas"]![0]!["minY"] = 3;
            stage["areas"]!.AsArray().Insert(0, retainedPlayerArea);
            document["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""{"id":"forbidden-init","instructions":[{"op":"set-flag","flag":909,"value":true},{"op":"end"}]}"""));
            document["world"]!["programs"]!.AsArray().Add(JsonNode.Parse($$$"""{"id":"scene-test","instructions":[{"op":"scene-map","map":"yard-map","camera":{"x":{{{x}}},"y":{{{y}}}}},{"op":"wait-ticks","ticks":1},{"op":"end"}]}"""));
        });
        var entry = session.Current;
        var route = session.Definition.Exploration!.Maps[new("yard-map")].Battle!;
        var old = entry.Story.LogicalView!;
        var story = entry.Story.Copy(new("scene-test", 0), continuation: ProgramContinuation.BeforeBattleFinished,
            enteringBattle: route, callers: [new("invitation", 1)], entityServices: true, windowFixPending: true,
            display: new(1, new(0xEEE, 0xAAA), new(0, 0), FullFadeVisibility.Black),
            logicalView: old with { FollowCounter = 9, AX = old.AX with { Destination = 3000, Speed = 32 } });
        var result = ProgramRunner.Run(session.Definition, entry.WithStory(story), []);
        Assert.Null(result.Failure);
        var current = result.Snapshot;
        Assert.Equal("yard-map", current.Exploration!.Map.Value);
        Assert.Same(entry.Exploration!.Party, current.Exploration.Party);
        Assert.Equal(entry.Exploration.AllEntities, current.Exploration.AllEntities);
        Assert.Equal(entry.Story.Flags, current.Story.Flags);
        Assert.DoesNotContain(909, current.Story.Flags);
        Assert.Equal(story.Callers, current.Story.Callers);
        Assert.Same(route, current.Story.EnteringBattle);
        Assert.True(current.Story.EntityServices);
        Assert.True(current.Story.WindowFixPending);
        Assert.IsType<TickWait>(current.Story.Wait);
        Assert.Equal(new PalettePair(0x222, 0x444), current.Story.Display!.Base);
        Assert.Equal(new PalettePair(0, 0), current.Story.Display.Current);
        var view = current.Story.LogicalView!;
        Assert.Equal(3, view.Area.MinX);
        Assert.Null(view.TargetSlot);
        Assert.False(view.Scrolling);
        Assert.Equal(9, view.FollowCounter);
        Assert.Equal(0, view.AX.Speed);
        Assert.Equal(layer == 0 ? x * 192 + 768 : x * 384, view.AX.Position);
        Assert.Equal(layer == 0 ? y * 384 + 1152 : y * 384, view.AY.Position);
        Assert.Equal(layer == 0 ? x * 384 : x * 192 + 1536, view.BX.Position);
        Assert.Equal(layer == 0 ? y * 384 : y * 192 + 1920, view.BY.Position);
        var replacement = SceneEntities.Reload(current.Exploration, new(new(30, 128, 0, []), new(20, 20), 1,
            Enumerable.Range(0, 8).Select(index => new ExplorationEntityDefinition(new("entity-" + (128 + index)), new(7, 9), 3, 32, Sprite: 30)).ToArray()), current.Story.Flags, 999);
        Assert.Equal(view, current.Story.LogicalView);
        Assert.Equal(20, replacement.PlayerEntity.Position.X);
        Assert.NotSame(replacement.PlayerEntity, replacement.Entities[new("entity-135")]);
        // Entity replacement is a separate instruction; it cannot implicitly re-center the scene.
        var tick = ExplorationTextRunner.AfterEntities(replacement, current.Story);
        Assert.Equal(view.AX.Position, tick.LogicalView!.AX.Position);
        Assert.Null(tick.LogicalView.TargetSlot);
    }

    [Theory]
    [InlineData("visible", "scene-load-black")]
    [InlineData("transitioning", "full-fade-state")]
    [InlineData("palette", "scene-load-palette-binding")]
    [InlineData("window", "scene-load-windows")]
    [InlineData("warp", "ordinary-warp-scene-load")]
    public void BoundSceneRejectsUnsupportedStatesBeforePublishingReplacement(string condition, string failure)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", configure: document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""[{"op":"wait-ticks","ticks":1},{"op":"end"}]""");
            if (condition != "palette") document["world"]!["maps"]![1]!["basePalette"] = JsonNode.Parse("""{"color2":546,"color3":1092}""");
            document["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""{"id":"scene-test","instructions":[{"op":"scene-map","map":"yard-map","camera":{"x":3,"y":4}},{"op":"end"}]}"""));
        });
        var entry = session.Current;
        var display = condition == "visible" ? new ExplorationDisplay(1, new(0xEEE, 0xAAA), new(0xEEE, 0xAAA), FullFadeVisibility.BaseRestored) :
            new ExplorationDisplay(1, new(0xEEE, 0xAAA), new(0, 0), condition == "transitioning" ? FullFadeVisibility.Transitioning : FullFadeVisibility.Black);
        var story = entry.Story.Copy(new("scene-test", 0), display: display,
            textWindow: condition == "window" ? new OpenTextWindow(100, TextDisplayMode.Single, null, 0) : null,
            warp: condition == "warp" ? new(new("yard-map"), new(1, 1), 1, MapLoadMode.Rebuild) : null);
        var result = ProgramRunner.Run(session.Definition, entry.WithStory(story), []);
        Assert.Equal(failure, result.Failure!.Code);
        Assert.Same(entry.Active, result.Snapshot.Active);
        Assert.Same(display, result.Snapshot.Story.Display);
        Assert.Equal(story.Cursor, result.Snapshot.Story.Cursor);
    }

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

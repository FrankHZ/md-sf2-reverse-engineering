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
    [InlineData(ProgramContinuation.FieldInput, false)]
    [InlineData(ProgramContinuation.MapLoaded, true)]
    [InlineData(ProgramContinuation.BeforeBattleFinished, false)]
    [InlineData(ProgramContinuation.BeforeBattleFinished, true)]
    public void BoundCameraTrackingResolvesPhysicalAliasesWithoutAnInstallService(ProgramContinuation continuation, bool follower)
    {
        var session = CameraSession("entity-135");
        var entry = session.Current;
        var world = CameraPopulation(entry.Exploration!, follower);
        var target = world.Entities[new("entity-135")];
        world = world.WithEntity(target with { Visible = false }); // Invisible is still allocated.
        var oldView = entry.Story.LogicalView! with { FollowCounter = 7, AX = new(0, 384, 12), BX = new(0, 384, 24) };
        var route = session.Definition.Exploration!.Maps[new("yard-map")].Battle!;
        var story = entry.Story.Copy(new("tracking", 0), continuation: continuation,
            enteringBattle: continuation == ProgramContinuation.BeforeBattleFinished ? route : null,
            logicalView: oldView, callers: [new("invitation", 1)], entityServices: false,
            cameraEntitySlot: 1, cameraTarget: new(4, 5), randomSeedCopy: 0xA9);
        var result = ProgramRunner.Run(session.Definition, CameraSnapshot(entry, world, story), []);
        Assert.Null(result.Failure);
        Assert.Equal(8 + (follower ? 1 : 0), result.Snapshot.Story.LogicalView!.TargetSlot);
        Assert.Equal(oldView with { TargetSlot = target.Slot }, result.Snapshot.Story.LogicalView);
        Assert.Null(result.Snapshot.Story.CameraEntitySlot);
        Assert.Null(result.Snapshot.Story.CameraTarget);
        Assert.Same(world, result.Snapshot.Exploration);
        Assert.Same(world.Party, result.Snapshot.Exploration!.Party);
        Assert.Equal(story.SimulationTick, result.Snapshot.Story.SimulationTick);
        Assert.Equal(story.RandomSeedCopy, result.Snapshot.Story.RandomSeedCopy);
        Assert.Equal(story.Callers, result.Snapshot.Story.Callers);
        Assert.Equal(story.Continuation, result.Snapshot.Story.Continuation);
        Assert.Equal(story.EnteringBattle, result.Snapshot.Story.EnteringBattle);
        Assert.Equal(story.LogicalText, result.Snapshot.Story.LogicalText);
        Assert.Equal(story.PortraitWindow, result.Snapshot.Story.PortraitWindow);
        Assert.False(result.Snapshot.Story.EntityServices);
    }

    [Theory]
    [InlineData("entity-159", false)]
    [InlineData("entity-135", true)]
    public void BoundCameraTrackingRetainsSlotZeroAliasesAndNullDetach(string selector, bool detach)
    {
        var session = CameraSession(detach ? null : selector);
        var entry = session.Current;
        var world = CameraPopulation(entry.Exploration!, false);
        var view = entry.Story.LogicalView! with { TargetSlot = 8, FollowCounter = 9, BY = new(0, 384, 32) };
        var result = ProgramRunner.Run(session.Definition, CameraSnapshot(entry, world,
            entry.Story.Copy(new("tracking", 0), logicalView: view)), []);
        Assert.Null(result.Failure);
        Assert.Equal(view with { TargetSlot = detach ? null : 0 }, result.Snapshot.Story.LogicalView);
        Assert.Equal(entry.Story.SimulationTick, result.Snapshot.Story.SimulationTick);
    }

    [Theory]
    [InlineData("removed", "program-entity")]
    [InlineData("out-of-table", "program-entity")]
    [InlineData("missing", "program-entity")]
    [InlineData("cursor", "field-view-target")]
    [InlineData("incomplete", "field-text-context")]
    [InlineData("outcome", "field-text-context")]
    public void BoundCameraTrackingRejectsInvalidTargetsAndContextsBeforePublication(string condition, string code)
    {
        var session = CameraSession(condition == "out-of-table" ? "entity-255" : condition == "missing" ? "missing" : "entity-135");
        var entry = session.Current;
        var world = CameraPopulation(entry.Exploration!, false);
        if (condition == "removed") world = world.Hide(world.Entities[new("entity-135")], removeAliases: true);
        if (condition == "cursor") world = new(entry.Exploration!.Definition, entry.Exploration.Layout, entry.Exploration.Player,
            entry.Exploration.AllEntities.Append(new(new("entity-135"), world.PlayerEntity.Motion, true, Slot: 63)), entry.Exploration.Party);
        var story = entry.Story.Copy(new("tracking", 0),
            continuation: condition == "incomplete" ? ProgramContinuation.BeforeBattleFinished :
                condition == "outcome" ? ProgramContinuation.VictoryProgramFinished : ProgramContinuation.FieldInput);
        var input = CameraSnapshot(entry, world, story);
        var result = ProgramRunner.Run(session.Definition, input, []);
        Assert.Equal(code, result.Failure!.Code);
        Assert.Same(world, result.Snapshot.Exploration);
        Assert.Equal(story.LogicalView, result.Snapshot.Story.LogicalView);
        Assert.Equal(story.SimulationTick, result.Snapshot.Story.SimulationTick);
        Assert.Equal(story.Cursor, result.Snapshot.Story.Cursor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(255)]
    public void BoundCameraTrackingPreservesActiveScrollThenFollowsTheLiveMainPlane(int layer)
    {
        var session = CameraSession("ferryman");
        var entry = session.Current;
        var world = entry.Exploration!;
        var entity = world.Entities[new("ferryman")];
        var area = entry.Story.LogicalView!.Area with { ForegroundY = 0, Layer = layer,
            ParallaxAX = layer == 0 ? 128 : 256, ParallaxBX = layer == 0 ? 256 : 128 };
        var view = entry.Story.LogicalView with { Area = area, FollowCounter = 6,
            AX = new(380, 384, layer == 0 ? 12 : 24), BX = new(layer == 0 ? 360 : 380, 384, layer == 0 ? 24 : 12), AY = new(0), BY = new(0) };
        var bound = ProgramRunner.Run(session.Definition, entry.WithStory(entry.Story.Copy(new("tracking", 0), logicalView: view)), []);
        Assert.Null(bound.Failure);
        Assert.Equal(view with { TargetSlot = entity.Slot }, bound.Snapshot.Story.LogicalView);
        var live = world.WithEntity(entity with { Motion = entity.Motion with { X = 384 + 2305, Y = 1536 } });
        var finishing = ExplorationViewRunner.Tick(live, bound.Snapshot.Story.LogicalView!, new(2, 0, 0));
        Assert.Equal(384, finishing.AX.Position);
        Assert.Equal(384, finishing.BX.Position);
        Assert.Equal(layer == 0 ? 12 : 24, finishing.AX.Speed);
        Assert.Equal(6, finishing.FollowCounter);
        var following = ExplorationViewRunner.Tick(live, finishing, new(2, 0, 0));
        Assert.Equal(7, following.FollowCounter);
        Assert.Equal(layer == 0 ? 16 : 32, following.AX.Speed);
        Assert.Equal(layer == 0 ? 32 : 16, following.BX.Speed);
        Assert.Equal(layer == 0 ? 384 : 768, following.AX.Destination ?? following.AX.Position);
        Assert.Equal(layer == 0 ? 768 : 384, following.BX.Destination ?? following.BX.Position);
        var clamped = following with { AX = new(0), BX = new(0), AY = new(0), BY = new(0) };
        var inside = live.WithEntity(entity with { Motion = entity.Motion with { X = 1536, Y = 2304 } });
        var stable = ExplorationViewRunner.Tick(inside, clamped, new(2, 0, 0));
        Assert.Equal(0, stable.FollowCounter);
        Assert.False(stable.Scrolling);
        var upper = clamped with { AX = new(30 * 384 - 3840), BX = new(30 * 384 - 3840),
            AY = new(30 * 384 - 3456), BY = new(30 * 384 - 3456) };
        var beyond = live.WithEntity(entity with { Motion = entity.Motion with { X = 30 * 384, Y = 30 * 384 } });
        Assert.False(ExplorationViewRunner.Tick(beyond, upper, new(2, 0, 0)).Scrolling);
        var destination = ExplorationViewRunner.SetDestination(following, new(2, 3));
        Assert.Null(destination.TargetSlot);
        Assert.Equal(following.FollowCounter, destination.FollowCounter);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BoundCameraTrackingClosesRealW1WindowsBeforeMotionAndNextInput(bool enabled)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", second: "B{W1}", speed: 3, enabled: enabled, configure: document =>
        {
            document["world"]!["presentation"]!["sprites"]![0]!["portrait"] = 7;
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"wait-ticks","ticks":1},{"op":"text-cursor","text":100},{"op":"open-portrait","entity":"ferryman","flags":0},
                {"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},
                {"op":"close-portrait"},{"op":"close-text"},{"op":"wait-ticks","ticks":10},
                {"op":"camera-entity","entity":"ferryman"},
                {"op":"motion","entity":"ferryman","wait":false,"actions":[{"op":"move","x":0,"y":7,"wait":true},{"op":"idle"}]},
                {"op":"wait-ticks","ticks":20},{"op":"wait-view"},
                {"op":"open-portrait","entity":"ferryman","flags":0},
                {"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},{"op":"end"}]
                """);
        });
        var route = session.Definition.Exploration!.Maps[new("yard-map")].Battle!;
        var start = session.Current;
        var current = start.WithStory(start.Story.Copy(start.Story.Cursor, start.Story.Wait,
            continuation: ProgramContinuation.BeforeBattleFinished, enteringBattle: route, callers: [new("invitation", 3)], entityServices: enabled));
        SessionResult Submit(SessionCommand command)
        {
            var result = ExplorationDispatcher.Submit(session.Definition, current, command);
            Assert.Null(result.Failure); current = result.Snapshot; return result;
        }
        for (int i = 0; !current.CanWaitForText && i < 200; i++)
        {
            if (current.Story.Wait is FieldTextWait { LogicalDone: true, Revealed: false }) Submit(new CompleteTextReveal(current.Story.Wait.Token));
            else Submit(new AdvanceSimulation(current.Story.Wait!.Token));
        }
        Assert.True(current.CanWaitForText);
        Submit(new Acknowledge(current.Story.Wait!.Token));
        Assert.IsType<PortraitMovementWait>(current.Story.Wait);
        Assert.False(Assert.IsType<OpenPortraitWindow>(current.Story.PortraitWindow).Work!.Registered);
        bool closed = false, tracked = false;
        for (int i = 0; !current.CanWaitForText && i < 300; i++)
        {
            if (current.Story.Wait is TickWait { Remaining: 10 })
            { closed = true; Assert.IsType<ClosedPortraitWindow>(current.Story.PortraitWindow); Assert.IsType<ClosedTextWindow>(current.Story.TextWindow); }
            tracked |= current.Story.LogicalView!.TargetSlot == current.Exploration!.Entities[new("ferryman")].Slot;
            if (current.Story.Wait is FieldTextWait { LogicalDone: true, Revealed: false }) Submit(new CompleteTextReveal(current.Story.Wait.Token));
            else Submit(new AdvanceSimulation(current.Story.Wait!.Token));
        }
        Assert.True(closed && tracked && current.CanWaitForText);
        Assert.Equal(101, current.Story.TextCursor - 1);
        Assert.Same(route, current.Story.EnteringBattle);
        Assert.Equal(start.Story.Callers.Count + 1, current.Story.Callers.Count);
        Assert.False(current.CanWaitAtInput);
        Assert.Equal(enabled, current.Exploration!.Entities[new("ferryman")].Motion.Y != start.Exploration!.Entities[new("ferryman")].Motion.Y);
        Assert.Equal(current.Exploration.Entities[new("ferryman")].Slot, current.Story.LogicalView!.TargetSlot);
        Assert.Equal(enabled, current.Story.LogicalView.AY.Position != start.Story.LogicalView!.AY.Position);
    }

    [Fact]
    public void BoundCameraTrackingRejectsUnadmittedPaletteAndConflictingWhiteProfileBeforePublication()
    {
        var session = CameraSession("ferryman", unsupportedPalette: true);
        var entry = session.Current;
        var display = new ExplorationDisplay(1, new(0xEEE, 0xAAA), new(0xEEE, 0xAAA), FullFadeVisibility.BaseRestored);
        var bound = ProgramRunner.Run(session.Definition, entry.WithStory(entry.Story.Copy(new("tracking", 0), display: display)), []);
        Assert.Null(bound.Failure);
        var before = bound.Snapshot;
        // Validate the conflicting command directly: published camera/timer state is retained.
        Assert.Throws<BattleRuleException>(() => MapTransfer.ValidateCue(new(PresentationCueKind.FadeOut, "white", FullBlack: new(1))));
        var result = ExplorationDispatcher.Submit(session.Definition, before, new AdvanceSimulation(before.Story.Wait!.Token));
        Assert.Equal("full-black-fade-binding", result.Failure!.Code);
        Assert.IsNotType<FullFadeWait>(result.Snapshot.Story.Wait);
        Assert.Same(display, result.Snapshot.Story.Display);
        Assert.Equal(entry.Exploration!.Entities[new("ferryman")].Slot, result.Snapshot.Story.LogicalView!.TargetSlot);
    }


    private static (GameSession Session, SessionSnapshot Started) WhiteStart(PresentationCueKind kind,
        PalettePair basis, FullFadeVisibility visibility = FullFadeVisibility.BaseRestored, byte period = 3,
        bool programEnabled = false, bool? entityOverride = null)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", enabled: programEnabled, npcRandom: true,
            configure: document => document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse(
                """[{"op":"present","kind":"FadeOut","resource":"white","entity":null,"position":null},{"op":"wait-ticks","ticks":1},{"op":"end"}]"""));
        var current = visibility == FullFadeVisibility.Black ? new PalettePair(0, 0) :
            visibility == FullFadeVisibility.White ? new PalettePair(0xEEE, 0xEEE) : basis;
        var entry = session.Current;
        var story = entry.Story.Copy(new("invitation", 0), display: new(period, basis, current, visibility),
            entityServices: entityOverride);
        var started = MapTransfer.BeginFade(entry, story, kind, FullFadePurpose.Script, null, [], FullFadeColor.White);
        return (session, started);
    }

    [Theory]
    [InlineData(PresentationCueKind.FadeOut, FullFadeVisibility.Black, 0, 14)]
    [InlineData(PresentationCueKind.FadeOut, FullFadeVisibility.BaseRestored, 0xE20, 0x24E)]
    [InlineData(PresentationCueKind.FadeOut, FullFadeVisibility.BaseRestored, 0xEEE, 0xEEE)]
    [InlineData(PresentationCueKind.FadeIn, FullFadeVisibility.White, 0xE20, 0x24E)]
    [InlineData(PresentationCueKind.FadeIn, FullFadeVisibility.Black, 0, 0)]
    [InlineData(PresentationCueKind.FadeOut, FullFadeVisibility.White, 0, 0)]
    public void BoundWhiteFadeUsesBaseOffsetsTerminatorExtraServiceAndPeriodRestoration(
        PresentationCueKind kind, FullFadeVisibility visibility, int a, int b)
    {
        var basis = new PalettePair((ushort)a, (ushort)b);
        var (session, current) = WhiteStart(kind, basis, visibility, 17);
        var wait = Assert.IsType<FullFadeWait>(current.Story.Wait);
        Assert.Equal(FullFadeColor.White, wait.Color);
        Assert.Equal(1, wait.Period);
        Assert.Equal(FullFadeVisibility.Transitioning, current.Story.Display!.Visibility);
        ushort Color(ushort word, int offset) => (ushort)(Math.Clamp((word & 14) + offset * 2, 0, 14) |
            Math.Clamp(((word >> 4) & 14) + offset * 2, 0, 14) << 4 |
            Math.Clamp(((word >> 8) & 14) + offset * 2, 0, 14) << 8);
        long tick = current.Story.SimulationTick;
        for (int entry = 0; entry < 9; entry++)
        {
            var result = ExplorationDispatcher.Submit(session.Definition, current, new AdvanceSimulation(wait.Token));
            Assert.Null(result.Failure); current = result.Snapshot;
            wait = Assert.IsType<FullFadeWait>(current.Story.Wait);
            Assert.Same(basis, current.Story.Display!.Base);
            if (entry < 7)
            {
                int offset = kind == PresentationCueKind.FadeIn ? 6 - entry : entry + 1;
                Assert.Equal(new PalettePair(Color(basis.Color2, offset), Color(basis.Color3, offset)), current.Story.Display.Current);
            }
            Assert.Equal(entry == 8, wait.LogicalDone);
            Assert.Equal(entry == 8 ? 17 : 1, current.Story.Display.Period);
            Assert.Equal(tick + entry + 1, current.Story.SimulationTick);
        }
        Assert.Equal(kind == PresentationCueKind.FadeOut ? FullFadeVisibility.White : FullFadeVisibility.BaseRestored,
            current.Story.Display!.Visibility);
        var held = current;
        var rejected = ExplorationDispatcher.Submit(session.Definition, current, new AdvanceSimulation(wait.Token));
        Assert.Equal("fade-awaiting-presentation", rejected.Failure!.Code);
        Assert.Equal(held.Story, rejected.Snapshot.Story);
        var completed = ExplorationDispatcher.Submit(session.Definition, current, new CompletePresentation(wait.Token, kind));
        Assert.Null(completed.Failure);
        Assert.Equal(held.Story.SimulationTick, completed.Snapshot.Story.SimulationTick);
        Assert.Equal(held.Exploration!.Party, completed.Snapshot.Exploration!.Party);
        Assert.IsType<TickWait>(completed.Snapshot.Story.Wait);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BoundWhiteFadeEarlyReceiptAndBatchHaveIdenticalServices(bool early)
    {
        var (session, start) = WhiteStart(PresentationCueKind.FadeOut, new(0xE20, 0x24E));
        var wait = (FullFadeWait)start.Story.Wait!;
        if (early) start = ExplorationDispatcher.Submit(session.Definition, start,
            new CompletePresentation(wait.Token, wait.Kind)).Snapshot;
        var batched = ExplorationDispatcher.Submit(session.Definition, start, new AdvanceSimulation(wait.Token, 600));
        Assert.Null(batched.Failure);
        var single = start;
        for (int i = 0; i < 9; i++) single = ExplorationDispatcher.Submit(session.Definition, single,
            new AdvanceSimulation(wait.Token)).Snapshot;
        Assert.Equal(single.Story.SimulationTick, batched.Snapshot.Story.SimulationTick);
        Assert.Equal(single.Story.Display, batched.Snapshot.Story.Display);
        Assert.Equal(single.Exploration!.Party, batched.Snapshot.Exploration!.Party);
        Assert.Equal(single.Story.Wait, batched.Snapshot.Story.Wait);
        var stale = ExplorationDispatcher.Submit(session.Definition, start,
            new CompletePresentation(new(wait.Token.Value + 100), wait.Kind));
        Assert.Equal("stale-or-wrong-presentation", stale.Failure!.Code);
        Assert.Equal(start.Story, stale.Snapshot.Story);
        var wrong = ExplorationDispatcher.Submit(session.Definition, start,
            new CompletePresentation(wait.Token, PresentationCueKind.FadeIn));
        Assert.Equal("stale-or-wrong-presentation", wrong.Failure!.Code);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void BoundWhiteFadeHonorsEntityOverrideAndRegisteredPortrait(bool programEnabled, bool enabled)
    {
        var (session, current) = WhiteStart(PresentationCueKind.FadeOut, new(0, 14),
            programEnabled: programEnabled, entityOverride: enabled);
        var work = new PortraitWork(Blink: 4, Registered: true, Moving: true, Movement: 0);
        current = current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait,
            portraitWindow: new OpenPortraitWindow(1, 0, work),
            logicalText: current.Story.LogicalText! with { Open = true, Moving = true, AnimationLength = 4, AnimationCounter = 0 }));
        var before = current;
        var result = ExplorationDispatcher.Submit(session.Definition, current, new AdvanceSimulation(current.Story.Wait!.Token));
        Assert.Null(result.Failure); current = result.Snapshot;
        var serviced = Assert.IsType<OpenPortraitWindow>(current.Story.PortraitWindow).Work!;
        Assert.Equal(3, serviced.Blink);
        Assert.True(serviced.EyesClosed);
        Assert.True(serviced.Movement > work.Movement);
        Assert.True(current.Story.LogicalText!.AnimationCounter > 0);
        Assert.Equal(enabled, current.Exploration!.Party.MainSeed != before.Exploration!.Party.MainSeed);
        Assert.Equal(before.Story.SimulationTick + 1, current.Story.SimulationTick);
    }

    [Fact]
    public void BoundWhiteFadeTracksLiveMotionThroughTheCommonViewService()
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", enabled: true, configure: document =>
        {
            document["world"]!["maps"]![0]!["entities"]![0]!["actions"] = JsonNode.Parse(
                """[{"op":"move","x":0,"y":4,"wait":true},{"op":"idle"}]""");
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse(
                """[{"op":"camera-entity","entity":"ferryman"},{"op":"present","kind":"FadeOut","resource":"white","entity":null,"position":null},{"op":"wait-ticks","ticks":1},{"op":"end"}]""");
        });
        var entry = session.Current;
        var start = ProgramRunner.Run(session.Definition, entry.WithStory(entry.Story.Copy(new("invitation", 0),
            display: new(3, new(0, 14), new(0, 14), FullFadeVisibility.BaseRestored),
            logicalView: entry.Story.LogicalView! with { AY = new(16128, null, 24), BY = new(3840, null, 24) })), []);
        Assert.Null(start.Failure);
        var current = start.Snapshot;
        var actor = current.Exploration!.Entities[new("ferryman")];
        Assert.Equal(actor.Slot, current.Story.LogicalView!.TargetSlot);
        var view = current.Story.LogicalView;
        var wait = Assert.IsType<FullFadeWait>(current.Story.Wait);
        var result = ExplorationDispatcher.Submit(session.Definition, current, new AdvanceSimulation(wait.Token, 9));
        Assert.Null(result.Failure);
        current = result.Snapshot;
        Assert.NotEqual(actor.Motion.Y, current.Exploration!.Entities[new("ferryman")].Motion.Y);
        Assert.NotEqual(view.AY.Position, current.Story.LogicalView!.AY.Position);
        Assert.Equal(actor.Slot, current.Story.LogicalView.TargetSlot);
        Assert.Equal(9, current.Story.SimulationTick - start.Snapshot.Story.SimulationTick);
    }

    [Fact]
    public void BoundWhiteFadeRejectsInvalidStateWithoutPublication()
    {
        var (session, start) = WhiteStart(PresentationCueKind.FadeOut, new(0, 14));
        Assert.Throws<BattleRuleException>(() => MapTransfer.BeginFade(start, start.Story,
            PresentationCueKind.FadeIn, FullFadePurpose.Script, null, [], FullFadeColor.White));
        foreach (var cue in new[] { new PresentCue(PresentationCueKind.FadeIn, "white", FullBlack: new(1)),
            new PresentCue(PresentationCueKind.FlashWhite, "white"), new PresentCue(PresentationCueKind.RestorePalette, "white") })
            Assert.Throws<BattleRuleException>(() => MapTransfer.ValidateCue(cue));
    }

    private static GameSession CameraSession(string? target, bool unsupportedPalette = false) => ExplorationTextWaitTests.StartFieldText("A{W1}", configure: document =>
    {
        document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""[{"op":"wait-ticks","ticks":1},{"op":"end"}]""");
        var program = JsonNode.Parse("""{"id":"tracking","instructions":[{"op":"camera-entity","entity":null},{"op":"wait-ticks","ticks":1},{"op":"end"}]}""")!;
        program["instructions"]![0]!["entity"] = target;
        if (unsupportedPalette) program["instructions"]!.AsArray().Insert(2, JsonNode.Parse("""{"op":"present","kind":"FadeOut","resource":"blue","entity":null,"position":null}"""));
        document["world"]!["programs"]!.AsArray().Add(program);
    });

    private static ExplorationState CameraPopulation(ExplorationState world, bool follower) => SceneEntities.Build(
        world.Definition, world.Layout, new("entity-0"), new(1, 1), 1, 32, world.Party, follower ? [41] : [],
        new(30, 128, 30, [new(41, 2, 30)]),
        Enumerable.Range(0, 8).Select(index => new ExplorationEntityDefinition(new("entity-" + (128 + index)), new(7, 9), 3, 32, Sprite: 30)).ToArray());

    private static SessionSnapshot CameraSnapshot(SessionSnapshot basis, ExplorationState world, StoryState story) =>
        new(basis.SessionId, basis.Revision, basis.ObservationSequence, new ActiveExploration(world), story, basis.StopReason);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PostLoadWaitServicesRetainedEntitiesBeforeSeparateReplacement(bool enabled)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", enabled: enabled, npcRandom: true, configure: document =>
        {
            document["start"]!["player"] = "entity-0";
            document["world"]!["maps"]![1]!["basePalette"] = JsonNode.Parse("""{"color2":546,"color3":1092}""");
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""[{"op":"wait-ticks","ticks":1},{"op":"end"}]""");
            document["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""
                {"id":"post-load","instructions":[{"op":"scene-map","map":"yard-map","camera":{"x":3,"y":7}},
                {"op":"wait-ticks","ticks":1},{"op":"scene-entities","population":{"allyCount":1,"nonAllyStart":128,"playerSprite":30,"followers":[]},
                "position":{"x":8,"y":9},"facing":1,"entities":[]},{"op":"end"}]}
                """));
        });
        var entry = session.Current;
        var story = entry.Story.Copy(new("post-load", 0), entityServices: enabled,
            display: new(1, new(0xEEE, 0xAAA), new(0, 0), FullFadeVisibility.Black));
        var loaded = ProgramRunner.Run(session.Definition, entry.WithStory(story), []);
        Assert.Null(loaded.Failure);
        var wait = Assert.IsType<TickWait>(loaded.Snapshot.Story.Wait);
        Assert.Equal(1, wait.Remaining);
        Assert.Equal(entry.Exploration!.AllEntities, loaded.Snapshot.Exploration!.AllEntities);
        Assert.Equal(entry.Story.SimulationTick, loaded.Snapshot.Story.SimulationTick);
        Assert.Equal(entry.Exploration.Party.MainSeed, loaded.Snapshot.Exploration.Party.MainSeed);
        var serviced = ExplorationDispatcher.Submit(session.Definition, loaded.Snapshot, new AdvanceSimulation(wait.Token));
        Assert.Null(serviced.Failure);
        Assert.IsType<EntitySetSpriteWait>(serviced.Snapshot.Story.Wait);
        Assert.Equal(entry.Story.SimulationTick + 1, serviced.Snapshot.Story.SimulationTick);
        Assert.Equal(new MapPosition(8, 9), serviced.Snapshot.Exploration!.PlayerEntity.Position);
        Assert.Equal(enabled, entry.Exploration.Party.MainSeed != serviced.Snapshot.Exploration.Party.MainSeed);
        Assert.Equal(enabled, serviced.Snapshot.Story.EntityServices);
        Assert.Equal(loaded.Snapshot.Story.LogicalView!.Area, serviced.Snapshot.Story.LogicalView!.Area);
        Assert.Equal(loaded.Snapshot.Story.LogicalView.BX.Position, serviced.Snapshot.Story.LogicalView.BX.Position);
        Assert.Equal(loaded.Snapshot.Story.LogicalView.BY.Position, serviced.Snapshot.Story.LogicalView.BY.Position);
        Assert.Equal(24, serviced.Snapshot.Story.LogicalView.BX.Speed);
        Assert.False(serviced.Snapshot.Story.LogicalView.Scrolling);
        Assert.Null(serviced.Snapshot.Story.LogicalView!.TargetSlot);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitDetachPrecedesFadeServicesWithoutChangingAxesCounterOrRng(bool detach)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", configure: document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""[{"op":"wait-ticks","ticks":1},{"op":"end"}]""");
            var program = JsonNode.Parse("""
                {"id":"fade-test","instructions":[{"op":"present","kind":"FadeOut","resource":"black","entity":null,"position":null},
                {"op":"scene-map","map":"yard-map","camera":{"x":3,"y":7}},{"op":"end"}]}
                """)!;
            if (detach) program["instructions"]!.AsArray().Insert(0, JsonNode.Parse("""{"op":"camera-entity","entity":null}"""));
            document["world"]!["programs"]!.AsArray().Add(program);
        });
        var entry = session.Current;
        var oldView = entry.Story.LogicalView! with { FollowCounter = 9 };
        var moving = entry.Exploration!.WithEntity(entry.Exploration.PlayerEntity with
            { Motion = entry.Exploration.PlayerEntity.Motion with { X = 10 * 384 } });
        var story = entry.Story.Copy(new("fade-test", 0), logicalView: oldView,
            display: new(3, new(0xEEE, 0xAAA), new(0xEEE, 0xAAA), FullFadeVisibility.BaseRestored));
        SessionSnapshot Snapshot(StoryState state) => new(entry.SessionId, entry.Revision, entry.ObservationSequence,
            new ActiveExploration(moving), state, entry.StopReason);
        var activeAxes = oldView with { AX = oldView.AX with { Destination = 4000, Speed = 32 },
            BY = oldView.BY with { Destination = 3000, Speed = 24 } };
        var immediate = ProgramRunner.Run(session.Definition, Snapshot(story.Copy(story.Cursor, logicalView: activeAxes)), []);
        Assert.Null(immediate.Failure);
        Assert.Equal(activeAxes with { TargetSlot = detach ? null : activeAxes.TargetSlot }, immediate.Snapshot.Story.LogicalView);
        Assert.Equal(entry.Story.SimulationTick, immediate.Snapshot.Story.SimulationTick);
        Assert.Same(entry.Exploration.Party, immediate.Snapshot.Exploration!.Party);
        var started = ProgramRunner.Run(session.Definition, Snapshot(story), []);
        Assert.Null(started.Failure);
        var fade = Assert.IsType<FullFadeWait>(started.Snapshot.Story.Wait);
        var first = ExplorationDispatcher.Submit(session.Definition, started.Snapshot, new AdvanceSimulation(fade.Token));
        Assert.Null(first.Failure);
        Assert.Equal(detach ? 9 : 10, first.Snapshot.Story.LogicalView!.FollowCounter);
        Assert.Equal(detach ? null : oldView.TargetSlot, first.Snapshot.Story.LogicalView.TargetSlot);
        Assert.Equal(entry.Story.SimulationTick + 1, first.Snapshot.Story.SimulationTick);
        Assert.Equal(entry.Exploration.Party.MainSeed, first.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal(entry.Exploration.Party.ThinkingSeed, first.Snapshot.Exploration.Party.ThinkingSeed);
        Assert.Equal(entry.Exploration.Party.Actors, first.Snapshot.Exploration.Party.Actors);
        Assert.Equal(entry.Exploration.Party.Gold, first.Snapshot.Exploration.Party.Gold);
        Assert.Equal(entry.Story.EntityServices, first.Snapshot.Story.EntityServices);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BoundSceneFadeLoadFadeUsesNewBaseAndCanTrackTheRetainedPlayer(bool overridePeriod)
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
        Assert.Null(result.Failure);
        Assert.Equal(session.Current.Exploration!.PlayerEntity.Slot, session.Current.Story.LogicalView.TargetSlot);
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
            stage["areas"]![0]!["minX"] = 3;
            stage["areas"]![0]!["minY"] = 3;
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
        Assert.Null(current.Exploration.Definition.Traversal.SelectActiveArea(current.Exploration.PlayerEntity.Position));
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

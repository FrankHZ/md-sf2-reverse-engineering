using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ExplorationSessionTests
{
    private static SessionResult Step(ScenarioDefinition definition, SessionSnapshot snapshot, SessionCommand command)
    {
        var result = ExplorationDispatcher.Submit(definition, snapshot, command);
        Assert.Null(result.Failure);
        return result;
    }

    private static SessionSnapshot ReachFade(ScenarioDefinition definition, SessionSnapshot snapshot)
    {
        snapshot = Step(definition, snapshot, new Move(ExplorationDirection.East)).Snapshot;
        return Step(definition, snapshot, new AdvanceSimulation(snapshot.Story.Wait!.Token, 600)).Snapshot;
    }

    private static SessionSnapshot DrainFade(ScenarioDefinition definition, SessionSnapshot snapshot, bool early)
    {
        var fade = Assert.IsType<FullFadeWait>(snapshot.Story.Wait);
        if (early) snapshot = Step(definition, snapshot, new CompletePresentation(fade.Token, fade.Kind)).Snapshot;
        snapshot = Step(definition, snapshot, new AdvanceSimulation(fade.Token, 600)).Snapshot;
        if (!early) snapshot = Step(definition, snapshot, new CompletePresentation(fade.Token, fade.Kind)).Snapshot;
        return snapshot;
    }

    [Theory]
    [InlineData(1, true, true)]
    [InlineData(3, true, false)]
    [InlineData(6, true, true)]
    [InlineData(1, false, false)]
    [InlineData(3, false, true)]
    [InlineData(6, false, false)]
    public void OrdinaryWarpJoinsFiniteServicesAndActualVisibility(byte period, bool preserve, bool early)
    {
        var npc = new ExplorationEntity(new("timer"), EntityMotionState.At(new(5, 5), 0, 32), true,
            new([new WaitEntityTicks(200)]), Slot: 1);
        var (definition, before) = WarpWorld([npc], preserve, 123, period: period);
        var current = ReachFade(definition, before);
        var fade = Assert.IsType<FullFadeWait>(current.Story.Wait);
        Assert.Equal(FullFadePurpose.WarpOut, fade.Purpose);
        Assert.Equal(1, current.Story.SimulationTick);
        Assert.Null(current.Exploration!.PlayerEntity.Actions);
        Assert.Equal(384, current.Exploration.PlayerEntity.Motion.XTravel);
        current = Step(definition, current, new AdvanceSimulation(fade.Token, 8 * period - 1)).Snapshot;
        Assert.False(Assert.IsType<FullFadeWait>(current.Story.Wait).LogicalDone);
        Assert.Equal(8 * period, current.Exploration!.Entities[npc.Entity].Motion.WaitTimer);
        if (early) current = Step(definition, current, new CompletePresentation(fade.Token, fade.Kind)).Snapshot;
        current = Step(definition, current, new AdvanceSimulation(fade.Token, 600)).Snapshot;
        Assert.Equal(1 + 8 * period, current.Story.SimulationTick);
        Assert.Equal(1 + 8 * period, current.Exploration!.Entities[npc.Entity].Motion.WaitTimer);
        Assert.Equal("origin", current.Exploration.Map.Value);
        if (!early)
        {
            Assert.True(Assert.IsType<FullFadeWait>(current.Story.Wait).LogicalDone);
            var stopped = ExplorationDispatcher.Submit(definition, current, new AdvanceSimulation(fade.Token, 600));
            Assert.Equal("fade-awaiting-presentation", stopped.Failure!.Code);
            Assert.Same(current, stopped.Snapshot);
            current = Step(definition, current, new CompletePresentation(fade.Token, fade.Kind)).Snapshot;
        }
        Assert.IsType<WarpLoadWait>(current.Story.Wait);
        Assert.Equal(new PalettePair(0, 0), current.Story.Display!.Current);
        var oldActors = current.Exploration!.AllEntities;
        current = Step(definition, current, new AdvanceSimulation(current.Story.Wait!.Token)).Snapshot;
        Assert.Equal(oldActors, current.Exploration!.AllEntities);
        current = Step(definition, current, new AdvanceSimulation(current.Story.Wait!.Token, 600)).Snapshot;
        var fadeIn = Assert.IsType<FullFadeWait>(current.Story.Wait);
        Assert.Equal(FullFadePurpose.WarpIn, fadeIn.Purpose);
        Assert.Equal(preserve ? "origin" : "destination", current.Exploration!.Map.Value);
        Assert.Equal(new MapPosition(9, 9), current.Exploration.PlayerEntity.Position);
        Assert.Equal(3 + 8 * period, current.Story.SimulationTick);
        if (!preserve)
        {
            Assert.Equal(0, current.Exploration.Entities[new("destination")].Motion.WaitTimer);
            Assert.Contains(80, current.Story.Flags); Assert.Contains(99, current.Story.Flags);
        }
        current = DrainFade(definition, current, !early);
        Assert.Equal(4 + 16 * period, current.Story.SimulationTick); // Sum of the explicit helper services in this fixture only.
        Assert.Equal(SessionStopReason.PlayerInput, current.StopReason);
        Assert.Null(current.Story.Warp);
        Assert.Equal(FullFadeVisibility.BaseRestored, current.Story.Display!.Visibility);
        Assert.Equal(current.Story.Display.Base, current.Story.Display.Current);
        Assert.Equal(period, current.Story.Display.Period);
        Assert.Equal(123u, current.Exploration!.Party.MainSeed);
        var stale = ExplorationDispatcher.Submit(definition, current, new CompletePresentation(fadeIn.Token, fadeIn.Kind));
        Assert.Same(current, stale.Snapshot); Assert.NotNull(stale.Failure);
    }

    [Fact]
    public void FadeDeliveryCannotReplaceLogicalWorkOrRepeatOrUseOldToken()
    {
        var (definition, before) = WarpWorld([], true, 123);
        var current = ReachFade(definition, before);
        var fade = Assert.IsType<FullFadeWait>(current.Story.Wait);
        current = Step(definition, current, new CompletePresentation(fade.Token, fade.Kind)).Snapshot;
        Assert.False(Assert.IsType<FullFadeWait>(current.Story.Wait).LogicalDone);
        foreach (var command in new SessionCommand[] { new Move(ExplorationDirection.East), new Cancel(), new WaitAtInput(),
            new Acknowledge(fade.Token), new CompletePresentation(fade.Token, fade.Kind),
            new CompletePresentation(fade.Token, PresentationCueKind.FadeIn), new AdvanceSimulation(new(fade.Token.Value - 1)) })
        {
            var reject = ExplorationDispatcher.Submit(definition, current, command);
            Assert.NotNull(reject.Failure); Assert.Same(current, reject.Snapshot);
        }
        var single = current;
        for (int index = 0; index < 24; index++) single = Step(definition, single, new AdvanceSimulation(fade.Token)).Snapshot;
        var batch = Step(definition, current, new AdvanceSimulation(fade.Token, 600)).Snapshot;
        Assert.Equal(single.Story.SimulationTick, batch.Story.SimulationTick);
        Assert.Equal(single.Story.Wait, batch.Story.Wait);
        Assert.Equal(single.Exploration!.AllEntities, batch.Exploration!.AllEntities);
        Assert.IsType<WarpLoadWait>(batch.Story.Wait);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothPaletteWordsSelectReturnAndEqualBlackIsUnsupported(bool color3Nonzero)
    {
        var (definition, before) = WarpWorld([], false, 123, palette: new(0, color3Nonzero ? (ushort)0xE : (ushort)0));
        var current = DrainFade(definition, ReachFade(definition, before), false);
        var result = ExplorationDispatcher.Submit(definition, current, new AdvanceSimulation(current.Story.Wait!.Token, 600));
        if (color3Nonzero) Assert.Equal(PresentationCueKind.FadeIn, Assert.IsType<FullFadeWait>(result.Snapshot.Story.Wait).Kind);
        else
        {
            Assert.Equal("equal-palette-still-black", result.Failure!.Code);
            Assert.Equal("destination", result.Snapshot.Exploration!.Map.Value);
            Assert.Equal(27, result.Snapshot.Story.SimulationTick);
            Assert.NotNull(result.Snapshot.Story.Warp);
            Assert.Equal(FullFadeVisibility.Black, result.Snapshot.Story.Display!.Visibility);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnLoadWaitAndFullFadeRestoreTemporaryPeriodAndSkipDuplicateReturn(bool entitiesRunning)
    {
        var (definition, before) = WarpWorld([], false, 123, onLoad:
            [new WaitProgramTicks(2), new PresentCue(PresentationCueKind.FadeIn, "black", FullBlack: new(6)), new WriteFlag(602, true), new EndProgram()]);
        var programs = definition.Exploration!.Programs.Values.Select(program => program.Id == "arrive"
            ? new StoryProgram(program.Id, program.Instructions, entitiesRunning: entitiesRunning) : program);
        definition = new("warp", definition.Encounters.Values, exploration: new(definition.Exploration.Maps.Values, programs));
        var current = DrainFade(definition, ReachFade(definition, before), true);
        current = Step(definition, current, new AdvanceSimulation(current.Story.Wait!.Token, 600)).Snapshot;
        Assert.IsType<TickWait>(current.Story.Wait);
        current = Step(definition, current, new AdvanceSimulation(current.Story.Wait!.Token, 600)).Snapshot;
        var fade = Assert.IsType<FullFadeWait>(current.Story.Wait);
        Assert.Equal(FullFadePurpose.Script, fade.Purpose);
        Assert.Equal(6, current.Story.Display!.Period);
        current = Step(definition, current, new CompletePresentation(fade.Token, fade.Kind)).Snapshot;
        current = Step(definition, current, new AdvanceSimulation(fade.Token, 48)).Snapshot;
        Assert.Equal(6, current.Story.Display!.Period);
        Assert.False(Assert.IsType<FullFadeWait>(current.Story.Wait).LogicalDone);
        var result = Step(definition, current, new AdvanceSimulation(fade.Token, 600));
        Assert.Equal(3, result.Snapshot.Story.Display!.Period);
        Assert.Equal(SessionStopReason.PlayerInput, result.StopReason);
        Assert.Contains(602, result.Snapshot.Story.Flags);
        Assert.DoesNotContain(result.Observations, row => row.Kind == "full-fade-started");
        Assert.Contains(result.Observations, row => row.Kind == "warp-visible");
    }

    [Theory]
    [InlineData("period")]
    [InlineData("palette")]
    [InlineData("white")]
    [InlineData("unbound")]
    public void UnsupportedStaticBindingsRejectBeforeMovement(string shape)
    {
        var (definition, before) = WarpWorld([], false, 123, period: shape == "period" ? (byte)0 : (byte)3,
            onLoad: shape is "white" or "unbound" ? [new PresentCue(PresentationCueKind.FadeIn, shape == "white" ? "white" : "black"), new EndProgram()] : null);
        if (shape == "palette")
        {
            var target = definition.Exploration!.Maps[new("destination")];
            var missing = new ExplorationMapDefinition(target.Map, target.Layout, target.Traversal, target.Entities, target.Events);
            definition = new("warp", definition.Encounters.Values,
                exploration: new([definition.Exploration.Maps[new("origin")], missing], definition.Exploration.Programs.Values));
        }
        var result = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Same(before.Active, result.Snapshot.Active); Assert.Same(before.Story, result.Snapshot.Story);
        Assert.Empty(result.Observations);
    }

    [Fact]
    public void DynamicOnLoadFailureKeepsMapConsumedWorkAndCursor()
    {
        var (definition, before) = WarpWorld([], false, 123, onLoad:
            [new WriteFlag(603, true), new UnsupportedInstruction("unbound-effect", "authored"), new EndProgram()]);
        var current = DrainFade(definition, ReachFade(definition, before), true);
        var result = ExplorationDispatcher.Submit(definition, current, new AdvanceSimulation(current.Story.Wait!.Token, 600));
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal("destination", result.Snapshot.Exploration!.Map.Value);
        Assert.Equal(new ProgramLocation("arrive", 1), result.Snapshot.Story.Cursor);
        Assert.Contains(603, result.Snapshot.Story.Flags);
        Assert.Equal(27, result.Snapshot.Story.SimulationTick);
        Assert.NotNull(result.Snapshot.Story.Warp);
        Assert.Equal(FullFadeVisibility.Black, result.Snapshot.Story.Display!.Visibility);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TerminalFadeServiceConsumesOldSlotRandomActionBeforeRebuild(bool fail)
    {
        var npc = new ExplorationEntity(new("walker"), EntityMotionState.At(new(5, 5), 0, 32), true,
            new([new WaitEntityTicks(8), fail ? new UnsupportedEntityAction("unbound", "authored") :
                new RandomWalkEntity(new(5, 5), 1), new StopEntityActions()]), Slot: 1);
        var (definition, before) = WarpWorld([npc], false, 0x12341234u, period: 1);
        var current = ReachFade(definition, before);
        var fade = Assert.IsType<FullFadeWait>(current.Story.Wait);
        current = Step(definition, current, new AdvanceSimulation(fade.Token, 7)).Snapshot;
        Assert.Equal(0x12341234u, current.Exploration!.Party.MainSeed);
        var terminal = ExplorationDispatcher.Submit(definition, current, new AdvanceSimulation(fade.Token, 600));
        Assert.Equal(9, terminal.Snapshot.Story.SimulationTick);
        Assert.True(Assert.IsType<FullFadeWait>(terminal.Snapshot.Story.Wait).LogicalDone);
        Assert.Equal(FullFadeVisibility.Black, terminal.Snapshot.Story.Display!.Visibility);
        Assert.Equal("origin", terminal.Snapshot.Exploration!.Map.Value);
        if (fail)
        {
            Assert.Equal(SessionStopReason.Unsupported, terminal.StopReason);
            Assert.NotNull(terminal.Snapshot.Story.Warp);
            Assert.Equal(1, terminal.Snapshot.Exploration.Entities[npc.Entity].ActionCursor);
        }
        else
        {
            Assert.Null(terminal.Failure);
            Assert.Equal(0xECAB1234u, terminal.Snapshot.Exploration.Party.MainSeed);
            current = Step(definition, terminal.Snapshot, new CompletePresentation(fade.Token, fade.Kind)).Snapshot;
            current = Step(definition, current, new AdvanceSimulation(current.Story.Wait!.Token, 600)).Snapshot;
            Assert.Equal(0xECAB1234u, current.Exploration!.Party.MainSeed);
            Assert.DoesNotContain(npc.Entity, current.Exploration.Entities.Keys);
        }
    }

    [Fact]
    public void OnLoadReplacementRetiresOrdinaryContinuationAndRestoresThroughItsOwnHelper()
    {
        var (definition, before) = WarpWorld([], false, 123, onLoad:
            [new TransferToMap(new("origin"), new(4, 4), 1, MapLoadMode.Preserve),
                new PresentCue(PresentationCueKind.FadeIn, "black", FullBlack: new()), new EndProgram()]);
        // Replace into the current destination without recursively entering its onLoad.
        var replacement = new ExplorationMapDefinition(new("replacement"), before.Exploration!.Layout,
            before.Exploration.Definition.Traversal, [], [], basePalette: new(0xEEE, 0x888));
        var instructions = new StoryInstruction[] { new TransferToMap(replacement.Map, new(4, 4), 1, MapLoadMode.Rebuild),
            new PresentCue(PresentationCueKind.FadeIn, "black", FullBlack: new()), new EndProgram() };
        definition = new("warp", definition.Encounters.Values, exploration: new(definition.Exploration!.Maps.Values.Append(replacement),
            [new StoryProgram("arrive", instructions), definition.Exploration.Programs["script"]]));
        var current = DrainFade(definition, ReachFade(definition, before), false);
        var abandoned = current.Story.Wait!.Token;
        current = Step(definition, current, new AdvanceSimulation(abandoned, 600)).Snapshot;
        Assert.Null(current.Story.Warp);
        Assert.Equal("replacement", current.Exploration!.Map.Value);
        Assert.Equal(FullFadePurpose.Script, Assert.IsType<FullFadeWait>(current.Story.Wait).Purpose);
        Assert.NotNull(ExplorationDispatcher.Submit(definition, current, new AdvanceSimulation(abandoned)).Failure);
        current = DrainFade(definition, current, true);
        Assert.Equal(SessionStopReason.PlayerInput, current.StopReason);
        Assert.Equal(new MapPosition(4, 4), current.Exploration!.PlayerEntity.Position);
    }

    [Theory]
    [InlineData("duplicate")]
    [InlineData("unknown")]
    [InlineData("embedded")]
    [InlineData("zero-period")]
    [InlineData("invalid-word")]
    public void DisplayContentRejectsAmbiguousOrInvalidBindings(string shape)
    {
        var document = Document("harbor-arrival");
        document["start"]!["display"] = System.Text.Json.Nodes.JsonNode.Parse("""{"period":3,"base":{"color2":14,"color3":0},"current":{"color2":14,"color3":0},"visibility":"base-restored"}""");
        document["start"]!["mapPalettes"] = System.Text.Json.Nodes.JsonNode.Parse("""[{"map":"quay","base":{"color2":14,"color3":0}}]""");
        var bindings = document["start"]!["mapPalettes"]!.AsArray();
        if (shape == "duplicate") bindings.Add(bindings[0]!.DeepClone());
        if (shape == "unknown") bindings[0]!["map"] = "absent";
        if (shape == "embedded") document["world"]!["maps"]![0]!["basePalette"] = bindings[0]!["base"]!.DeepClone();
        if (shape == "zero-period") document["start"]!["display"]!["period"] = 0;
        if (shape == "invalid-word") bindings[0]!["base"]!["color3"] = 1;
        Assert.IsNotType<SessionStarted>(GameSession.Start(Reader(document)));
    }

    [Theory]
    [InlineData(1, 4, true)]
    [InlineData(602, 3, true)]
    [InlineData(603, 5, false)]
    public void OnLoadUsesActualFlagsAndEntityEffectsBeforeReturn(int flag, int x, bool visible)
    {
        var (definition, before) = WarpWorld([], false, 123, onLoad:
            [new BranchFlag(1, true, new("arrive", 4)), new BranchFlag(602, true, new("arrive", 6)),
             new BranchFlag(603, true, new("arrive", 8)), new EndProgram(),
             new SetEntityPosition(new("destination"), new(4, 4), 1), new EndProgram(),
             new SetEntityPosition(new("destination"), new(3, 3), 1), new EndProgram(),
             new SetEntityVisibility(new("destination"), false), new EndProgram()]);
        before = before.WithStory(before.Story.Copy(null, flags: [flag]));
        var current = DrainFade(definition, ReachFade(definition, before), true);
        current = Step(definition, current, new AdvanceSimulation(current.Story.Wait!.Token, 600)).Snapshot;
        Assert.Equal(FullFadePurpose.WarpIn, Assert.IsType<FullFadeWait>(current.Story.Wait).Purpose);
        Assert.Equal(x, current.Exploration!.Entities[new("destination")].Position.X);
        Assert.Equal(visible, current.Exploration.Entities[new("destination")].Visible);
    }

    [Fact]
    public void MissingDisplayBindingCannotDefaultTheWarpPeriod()
    {
        var (definition, before) = WarpWorld([], true, 123);
        before = before.WithStory(new StoryState());
        var result = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        Assert.Equal("full-fade-state", result.Failure!.Code);
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Same(before.Active, result.Snapshot.Active); Assert.Empty(result.Observations);
    }

    [Fact]
    public void BattleSelectedWarpKeepsItsExistingCallerWithoutOrdinaryFadeBinding()
    {
        var (definition, before) = WarpWorld([], false, 123);
        var target = definition.Exploration!.Maps[new("destination")];
        var battleMap = new ExplorationMapDefinition(target.Map, target.Layout, target.Traversal, target.Entities, [],
            battle: new(before.Exploration!.Party.Encounter, null, null, null, new("battle-selected", 0), null));
        definition = new("warp", definition.Encounters.Values, exploration: new(
            [definition.Exploration.Maps[new("origin")], battleMap],
            [new StoryProgram("battle-selected", [new WaitProgramTicks(2), new EndProgram()])]));
        before = before.WithStory(new StoryState());
        var queued = Step(definition, before, new Move(ExplorationDirection.East));
        var result = Step(definition, queued.Snapshot, new AdvanceSimulation(queued.Snapshot.Story.Wait!.Token, 600));
        Assert.IsType<TickWait>(result.Snapshot.Story.Wait);
        Assert.Equal(1, result.Snapshot.Story.SimulationTick);
        Assert.Contains(result.Observations, row => row.Kind == "battle-selected");
        Assert.DoesNotContain(result.Observations, row => row.Kind == "full-fade-started");
        Assert.Null(result.Snapshot.Story.Warp);
    }

    [Theory]
    [InlineData(true, 0, 0x12341234u, 0xECAB1234u, 5, 6)]
    [InlineData(true, 2, 0x12341234u, 0x12341234u, 5, 5)]
    [InlineData(true, 0, 0xC632A55Au, 0x1091A55Au, 6, 5)]
    [InlineData(false, 0, 0x12341234u, 0xECAB1234u, 5, 6)]
    public void WarpProducingPassRetainsOldPopulationBeforeFade(bool preserve, byte delay, uint seed,
        uint expectedSeed, int npcX, int npcY)
    {
        var npc = new ExplorationEntity(new("walker"), EntityMotionState.At(new(5, 5), 0, 32), true,
            new([new WaitEntityTicks(delay), new RandomWalkEntity(new(5, 5), 1), new StopEntityActions()]), Slot: 1);
        var idle = new ExplorationEntity(new("idle"), EntityMotionState.At(new(8, 8), 0, 32), true, Slot: 2);
        var (definition, before) = WarpWorld([npc, idle], preserve, seed);
        var pending = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        Assert.Null(pending.Failure);
        Assert.Equal(seed, pending.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal(before.Exploration!.PlayerEntity, pending.Snapshot.Exploration.PlayerEntity);
        var token = Assert.IsType<EntityWait>(pending.Snapshot.Story.Wait).Token;
        var result = ExplorationDispatcher.Submit(definition, pending.Snapshot, new AdvanceSimulation(token, 600));
        Assert.Null(result.Failure);
        var world = result.Snapshot.Exploration!;
        Assert.Equal(expectedSeed, world.Party.MainSeed);
        Assert.Equal(1, result.Snapshot.Story.SimulationTick);
        Assert.IsType<FullFadeWait>(result.Snapshot.Story.Wait);
        Assert.Equal(new MapPosition(1, 1), world.PlayerEntity.Position);
        Assert.Equal((npcX * 384, npcY * 384), ((int)world.Entities[npc.Entity].Motion.XDestination, (int)world.Entities[npc.Entity].Motion.YDestination));
        Assert.Equal(delay == 0 ? 0 : 1, world.Entities[npc.Entity].Motion.WaitTimer);
        Assert.Equal(idle, world.Entities[idle.Entity]);
        var stale = ExplorationDispatcher.Submit(definition, result.Snapshot, new AdvanceSimulation(token));
        Assert.Equal("stale-or-wrong-wait", stale.Failure!.Code);
        Assert.Same(result.Snapshot, stale.Snapshot);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LaterSlotSeesPlayerTravelReservationBeforeWarpRelocation(bool blockedMarker)
    {
        var npc = new ExplorationEntity(new("walker"),
            EntityMotionState.At(new(2, 2), 0, 32) with { FlagsA = 0x20 }, true,
            new([new MoveEntityAbsolute(new(2, 1)), new StopEntityActions()]), Slot: 1);
        var (definition, before) = WarpWorld([npc], true, 123, blockedMarker);
        var queued = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        var result = ExplorationDispatcher.Submit(definition, queued.Snapshot, new AdvanceSimulation(queued.Snapshot.Story.Wait!.Token));
        Assert.Null(result.Failure);
        Assert.Equal(new MapPosition(1, 1), result.Snapshot.Exploration!.PlayerEntity.Position);
        var after = result.Snapshot.Exploration.Entities[npc.Entity];
        Assert.Equal((blockedMarker ? 1 : 2) * 384, after.Motion.YDestination);
        Assert.Equal(blockedMarker ? 1 : 0, after.ActionCursor);
    }

    [Fact]
    public void EarlierSlotCanObstructPendingWarpAndRemainingSlotsStillRun()
    {
        var npc = new ExplorationEntity(new("walker"),
            EntityMotionState.At(new(2, 2), 0, 32) with { FlagsA = 0x80 }, true,
            new([new MoveEntityAbsolute(new(2, 1)), new StopEntityActions()]), Slot: 1);
        var timer = new ExplorationEntity(new("timer"), EntityMotionState.At(new(5, 5), 0, 32), true,
            new([new WaitEntityTicks(10)]), Slot: 3);
        var (definition, before) = WarpWorld([npc, timer], true, 123, playerSlot: 2);
        var queued = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        Assert.Equal(ExplorationDirection.East, Assert.IsType<EntityWait>(queued.Snapshot.Story.Wait).PendingMove);
        var result = ExplorationDispatcher.Submit(definition, queued.Snapshot, new AdvanceSimulation(queued.Snapshot.Story.Wait!.Token, 600));
        Assert.Null(result.Failure);
        Assert.Null(result.Snapshot.Story.Wait);
        Assert.Equal(1, result.Snapshot.Story.SimulationTick);
        Assert.Equal(new MapPosition(1, 1), result.Snapshot.Exploration!.PlayerEntity.Position);
        Assert.Equal(1, result.Snapshot.Exploration.Entities[timer.Entity].Motion.WaitTimer);
        Assert.Equal(new[] { "simulation-tick", "movement-blocked" }, result.Observations.Select(row => row.Kind));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidWarpDestinationDoesNotConsumeExpiringNpcAction(bool preserve)
    {
        var npc = new ExplorationEntity(new("walker"), EntityMotionState.At(new(5, 5), 0, 32), true,
            new([new WaitEntityTicks(0), new RandomWalkEntity(new(5, 5), 1)]), Slot: 1);
        var (definition, before) = WarpWorld([npc], preserve, 123, destinationX: 63);
        var result = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        Assert.Equal("map-entry-position", result.Failure!.Code);
        Assert.Equal(SessionStopReason.Faulted, result.StopReason);
        Assert.Same(before.Active, result.Snapshot.Active);
        Assert.Same(before.Story, result.Snapshot.Story);
        Assert.Equal(before.Revision, result.Snapshot.Revision);
        Assert.Equal(before.ObservationSequence, result.Snapshot.ObservationSequence);
        Assert.Empty(result.Observations);
    }

    [Fact]
    public void WarpBatchStopsAtSameTransferBoundaryAsOneOpportunity()
    {
        var npc = new ExplorationEntity(new("timer"), EntityMotionState.At(new(5, 5), 0, 32), true,
            new([new WaitEntityTicks(10)]), Slot: 1);
        var (definition, before) = WarpWorld([npc], true, 123);
        var queued = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        var token = queued.Snapshot.Story.Wait!.Token;
        var single = ExplorationDispatcher.Submit(definition, queued.Snapshot, new AdvanceSimulation(token));
        var batch = ExplorationDispatcher.Submit(definition, queued.Snapshot, new AdvanceSimulation(token, 600));
        Assert.Null(single.Failure);
        Assert.Null(batch.Failure);
        Assert.Equal(single.Observations, batch.Observations);
        Assert.Equal(single.Snapshot.Story.SimulationTick, batch.Snapshot.Story.SimulationTick);
        Assert.Equal(single.Snapshot.Exploration!.AllEntities, batch.Snapshot.Exploration!.AllEntities);
        Assert.Equal(single.Snapshot.Exploration.Party.MainSeed, batch.Snapshot.Exploration.Party.MainSeed);
        Assert.IsType<FullFadeWait>(batch.Snapshot.Story.Wait);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScriptedTransferAndOnLoadOnlyConsumeFieldPassWhenReachedByFieldAction(bool fieldAction)
    {
        var npc = new ExplorationEntity(new("walker"), EntityMotionState.At(new(5, 5), 0, 32), true,
            new([new RandomWalkEntity(new(5, 5), 1)]), Slot: 1);
        var (definition, before) = WarpWorld([npc], false, 0x12341234u, warpProgram: true);
        var running = new SessionSnapshot(before.SessionId, before.Revision, before.ObservationSequence,
            before.Active, before.Story.Copy(new("script", 0)), SessionStopReason.SimulationWait);
        SessionResult result;
        if (fieldAction)
        {
            var queued = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
            result = ExplorationDispatcher.Submit(definition, queued.Snapshot, new AdvanceSimulation(queued.Snapshot.Story.Wait!.Token, 600));
        }
        else result = ProgramRunner.Run(definition, running, []);
        Assert.Null(result.Failure);
        Assert.Equal(SessionStopReason.PlayerInput, result.StopReason);
        Assert.Equal(fieldAction ? 1 : 0, result.Snapshot.Story.SimulationTick);
        Assert.Equal(fieldAction ? 0xECAB1234u : 0x12341234u, result.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal(0, result.Snapshot.Exploration.Entities[new("destination")].Motion.WaitTimer);
        Assert.Equal(new[] { 80, 98, 99 }, result.Snapshot.Story.Flags);
        Assert.Equal(fieldAction ? 1 : 0, result.Observations.Count(row => row.Kind == "simulation-tick"));
        Assert.DoesNotContain(result.Observations, row => row.Kind == "warp-started");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WarpKeepsOldWorldCollisionAndPhysicalSlotOrder(bool reverse)
    {
        ExplorationEntity Mover(string id, int x, int slot) => new(new(id),
            EntityMotionState.At(new(x, 5), 0, 32) with { FlagsA = 0x20 }, true,
            new([new MoveEntityAbsolute(new(5, 5)), new StopEntityActions()]), Slot: slot);
        var west = Mover("west", 4, reverse ? 2 : 1);
        var east = Mover("east", 6, reverse ? 1 : 2);
        var (definition, before) = WarpWorld([east, west], true, 123);
        var queued = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        var result = ExplorationDispatcher.Submit(definition, queued.Snapshot, new AdvanceSimulation(queued.Snapshot.Story.Wait!.Token));
        Assert.Null(result.Failure);
        var winner = result.Snapshot.Exploration!.Entities[new(reverse ? "east" : "west")];
        var loser = result.Snapshot.Exploration.Entities[new(reverse ? "west" : "east")];
        Assert.Equal(5 * 384, winner.Motion.XDestination);
        Assert.Equal((reverse ? 4 : 6) * 384, loser.Motion.XDestination);
        Assert.Equal(0, loser.ActionCursor);
        Assert.Equal(123u, result.Snapshot.Exploration.Party.MainSeed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WarpMarkerPrecedesTravelButEntityObstructionPrecedesMarker(bool obstructed)
    {
        var blocker = new ExplorationEntity(new("blocker"),
            EntityMotionState.At(new(2, 1), 0, 32) with { FlagsA = 0x80 }, true, Slot: 1);
        var npc = new ExplorationEntity(new("timer"), EntityMotionState.At(new(5, 5), 0, 32), true,
            new([new WaitEntityTicks(10)]), Slot: 2);
        var (definition, before) = WarpWorld(obstructed ? [blocker, npc] : [npc], true, 123, blockedMarker: true);
        var queued = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        Assert.Null(queued.Failure);
        if (obstructed)
        {
            Assert.Null(queued.Snapshot.Story.Wait);
            Assert.Equal("movement-blocked", Assert.Single(queued.Observations).Kind);
            Assert.Equal(0, queued.Snapshot.Story.SimulationTick);
            Assert.Equal(0, queued.Snapshot.Exploration!.Entities[npc.Entity].Motion.WaitTimer);
            return;
        }
        var result = ExplorationDispatcher.Submit(definition, queued.Snapshot, new AdvanceSimulation(queued.Snapshot.Story.Wait!.Token));
        Assert.Null(result.Failure);
        Assert.Equal(new MapPosition(1, 1), result.Snapshot.Exploration!.PlayerEntity.Position);
        Assert.Equal(1, result.Snapshot.Exploration.Entities[npc.Entity].Motion.WaitTimer);
        Assert.Equal(0, result.Snapshot.Exploration.PlayerEntity.Motion.XTravel);
    }

    [Fact]
    public void PendingFieldMoveRejectsCancellationReplacementAndStaleDelivery()
    {
        var session = Start("harbor-arrival");
        var pending = Accept(session, new Move(ExplorationDirection.South)).Snapshot;
        var token = Assert.IsType<EntityWait>(pending.Story.Wait).Token;
        Assert.Equal(ExplorationDirection.South, Assert.IsType<EntityWait>(pending.Story.Wait).PendingMove);
        foreach (var command in new SessionCommand[] { new Cancel(), new Move(ExplorationDirection.West),
            new Acknowledge(token), new CompletePresentation(token, PresentationCueKind.FadeOut),
            new AdvanceSimulation(new(token.Value + 1)), new WaitAtInput() })
        {
            Assert.NotNull(Send(session, command).Failure);
            Assert.Same(pending, session.Current);
        }
        Assert.Equal("stale-input", session.Submit(new(pending.SessionId, pending.Revision - 1, null,
            new AdvanceSimulation(token))).Failure!.Code);
        var first = Accept(session, new AdvanceSimulation(token));
        Assert.Equal(1, session.Current.Story.SimulationTick);
        Assert.Single(first.Observations, row => row.Kind == "movement-started");
        Assert.Null(Assert.IsType<EntityWait>(session.Current.Story.Wait).PendingMove);
        var arrived = Accept(session, new AdvanceSimulation(token, 600));
        Assert.DoesNotContain(arrived.Observations, row => row.Kind == "movement-started");
        Assert.Equal(new MapPosition(1, 2), session.Current.Exploration!.PlayerEntity.Position);
        var stopped = session.Current;
        Assert.NotNull(Send(session, new AdvanceSimulation(token)).Failure);
        Assert.Same(stopped, session.Current);
    }

    private static (ScenarioDefinition Definition, SessionSnapshot Snapshot) WarpWorld(
        ExplorationEntity[] npcs, bool preserve, uint seed, bool blockedMarker = false, int playerSlot = 0,
        int destinationX = 9, bool warpProgram = false, byte period = 3,
        PalettePair? palette = null, StoryInstruction[]? onLoad = null)
    {
        var source = Start("harbor-arrival");
        var words = new ushort[WorkingMapLayout.WordCount];
        words[66] = blockedMarker ? (ushort)0xD000 : (ushort)0x1000;
        var layout = new WorkingMapLayout(words);
        var map = new ExplorationMapDefinition(new("origin"), layout, new OriginalMapTraversal([new(0, 0, 12, 12)]), [],
            [new(ExplorationEventKind.Warp, 2, 1, null, warpProgram ? new("script", 0) : null, new(preserve ? "origin" : "destination"), new(destinationX, 9),
                RequiredMarker: 0x1000, LoadMode: preserve ? MapLoadMode.Preserve : MapLoadMode.Rebuild)],
            population: new(30, 128, 0, []));
        var destination = new ExplorationMapDefinition(new("destination"), layout, map.Traversal,
            [new(new("destination"), new(5, 5), 0, 32, Actions: new([new WaitEntityTicks(10), new StopEntityActions()]))], [],
            onLoad: new("arrive", 0), basePalette: palette ?? new(0xEEE, 0x888), entryFlags: [new(80, true)]);
        var player = source.Current.Exploration!.PlayerEntity with
        { Motion = EntityMotionState.At(new(1, 1), 0, 32) with { FlagsA = 0xA0 }, Slot = playerSlot };
        var party = source.Current.Exploration.Party;
        var world = new ExplorationState(map, layout, player.Entity, new[] { player }.Concat(npcs),
            new(party.Encounter, party.Actors, seed, party.ThinkingSeed, party.Gold, party.NewBattle));
        var definition = new ScenarioDefinition("warp", source.Definition.Encounters.Values,
            exploration: new([map, destination], [new StoryProgram("arrive", onLoad ?? [new WriteFlag(99, true), new EndProgram()]),
                new StoryProgram("script", [new TransferToMap(new("destination"), new(9, 9), 0, MapLoadMode.Rebuild),
                    new WriteFlag(98, true), new EndProgram()])]));
        return (definition, new(Guid.NewGuid(), 1, 1, new ActiveExploration(world), new StoryState([], null, display: new(period, palette ?? new(0xEEE, 0x888), palette ?? new(0xEEE, 0x888), FullFadeVisibility.BaseRestored)), SessionStopReason.PlayerInput));
    }

    [Theory]
    [InlineData("clear", 2, 2, false)]
    [InlineData("current", 2, 2, true)]
    [InlineData("reserved", 4, 3, true)]
    [InlineData("near-current", 4, 3, true)]
    [InlineData("axis-boundary", 2, 2, false)]
    [InlineData("mover-ignores", 4, 3, false)]
    [InlineData("nonblocking", 2, 2, false)]
    [InlineData("hidden", 4, 3, false)]
    [InlineData("retired", 2, 2, false)]
    [InlineData("non-door", 4, 3, false)]
    [InlineData("non-door-current", 2, 2, true)]
    public void SourceDoorChecksEntityObstructionBeforeCopyAndTraversal(string shape, int x, int y, bool blocked)
    {
        var source = Start("harbor-arrival");
        var original = source.Current.Exploration!;
        var words = new ushort[WorkingMapLayout.WordCount];
        bool door = !shape.StartsWith("non-door", StringComparison.Ordinal);
        words[y * 64 + x] = door ? (ushort)0xC400 : (ushort)0; // Closed traversal would return the origin.
        var layout = new WorkingMapLayout(words);
        var afterStep = new ProgramLocation("after-step", 0);
        var map = new ExplorationMapDefinition(new("door-yard"), layout,
            new OriginalMapTraversal([new(0, 0, 7, 7)]), [],
            [new(ExplorationEventKind.Step, x, y, null, afterStep, RequiredMarker: 0)],
            population: new(30, 128, 0, []),
            layoutEvents: new([new(new(x, y), new(0, 0, x, y, 1, 1))], [], new([])));
        var player = original.PlayerEntity with
        {
            Motion = EntityMotionState.At(new(x - 1, y), 3, 32) with
                { FlagsA = shape == "mover-ignores" ? (byte)0x80 : (byte)0xA0 },
        };
        var motion = EntityMotionState.At(new(x, y), 0, 32) with { FlagsA = 0x80 };
        motion = shape switch
        {
            "clear" or "retired" or "non-door" => motion with { X = 0x7000, Y = 0x7000, XDestination = 0x7000, YDestination = 0x7000 },
            "current" => motion with { XDestination = 0, YDestination = 0 },
            "reserved" => motion with { X = 0, Y = 0 },
            "near-current" => motion with { X = (short)(x * 384 + 255), Y = (short)(y * 384 - 255), XDestination = 0, YDestination = 0 },
            "axis-boundary" => motion with { X = (short)(x * 384 + 256), XDestination = 0, YDestination = 0 },
            "nonblocking" => motion with { FlagsA = 0x20 },
            _ => motion,
        };
        var other = new ExplorationEntity(new("blocker"), motion, shape != "hidden", Slot: player.Slot + 1);
        var world = new ExplorationState(map, layout, player.Entity, shape == "clear" ? [player] : [player, other], original.Party);
        var definition = new ScenarioDefinition("door-yard", source.Definition.Encounters.Values,
            exploration: new([map], [new StoryProgram("after-step", [new EndProgram()])]));
        var before = new SessionSnapshot(Guid.NewGuid(), 1, 1, new ActiveExploration(world), new StoryState([], null), SessionStopReason.PlayerInput);
        var result = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        Assert.Null(result.Failure);
        if (!blocked)
        {
            var pending = Assert.IsType<EntityWait>(result.Snapshot.Story.Wait);
            Assert.Same(layout, result.Snapshot.Exploration!.Layout);
            result = ExplorationDispatcher.Submit(definition, result.Snapshot, new AdvanceSimulation(pending.Token));
            Assert.Null(result.Failure);
        }
        Assert.Equal((byte)0, result.Snapshot.Exploration!.PlayerEntity.Motion.Facing);
        Assert.Equal(original.Party.MainSeed, result.Snapshot.Exploration.Party.MainSeed);
        Assert.Equal(before.Story.SimulationTick + (blocked ? 0 : 1), result.Snapshot.Story.SimulationTick);
        if (blocked)
        {
            Assert.Equal("movement-blocked", Assert.Single(result.Observations).Kind);
            Assert.Same(layout, result.Snapshot.Exploration.Layout);
            Assert.Equal(player.Position, result.Snapshot.Exploration.PlayerEntity.Position);
            Assert.False(result.Snapshot.Exploration.PlayerEntity.Busy);
            Assert.Null(result.Snapshot.Story.Wait);
        }
        else
        {
            Assert.Equal(door ? new[] { "simulation-tick", "door-opened", "movement-started" } : new[] { "simulation-tick", "movement-started" }, result.Observations.Select(row => row.Kind));
            Assert.Equal(0, result.Snapshot.Exploration.Layout[x, y]);
            Assert.Equal(afterStep, Assert.IsType<EntityWait>(result.Snapshot.Story.Wait).AfterMotion);
            Assert.Equal((x * 384, y * 384), ((int)result.Snapshot.Exploration.PlayerEntity.Motion.XDestination,
                (int)result.Snapshot.Exploration.PlayerEntity.Motion.YDestination));
            // Return to the same approach in the now-open layout: no second door event.
            var repeat = new SessionSnapshot(before.SessionId, before.Revision, before.ObservationSequence,
                new ActiveExploration(result.Snapshot.Exploration.WithEntity(player)), before.Story, before.StopReason);
            var again = ExplorationDispatcher.Submit(definition, repeat, new Move(ExplorationDirection.East));
            Assert.Null(again.Failure);
            Assert.Equal("movement-requested", Assert.Single(again.Observations).Kind);
        }
    }

    [Fact]
    public void ImmediatePlainAcknowledgementPreservesNpcPhaseAndOpenTextUntilExplicitClose()
    {
        var session = StartProgram("""
            [{"op":"close-portrait"},
             {"op":"motion","entity":"ferryman","wait":false,"actions":[{"op":"random-walk","x":2,"y":1,"radius":1}]},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"end"}]
            """);
        var before = session.Current;
        Assert.True(before.CanWaitForText);
        var result = Accept(session, new Acknowledge(before.Story.Wait!.Token));
        Assert.Same(before.Exploration, session.Current.Exploration);
        Assert.Same(before.Story.TextWindow, session.Current.Story.TextWindow);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        Assert.DoesNotContain(result.Observations, row => row.Kind is "simulation-tick" or "gameplay-wait");
    }

    [Theory]
    [InlineData(true, 0x12341234u, 0, 0xECAB1234u, 2, 2)]
    [InlineData(true, 0xC632A55Au, 2, 0x1091A55Au, 3, 1)]
    [InlineData(false, 0xC632A55Au, 2, 0xC632A55Au, 2, 1)]
    public void PlainInputWaitRunsOnlyEnabledEntityServiceAndAcceptingInputAddsNoPoll(
        bool enabled, uint seed, int delay, uint expectedSeed, int targetX, int targetY)
    {
        var session = StartProgram("""
            [{"op":"close-portrait"},{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"close-text"},{"op":"wait-ticks","ticks":10},{"op":"end"}]
            """, document =>
        {
            document["world"]!["programs"]![0]!["entitiesRunning"] = enabled;
            document["battle"]!["start"]!["mainSeed"] = seed;
            document["world"]!["maps"]![0]!["entities"]![0]!["actions"] = System.Text.Json.Nodes.JsonNode.Parse($$"""
                [{"op":"wait","ticks":{{delay}}},{"op":"random-walk","x":2,"y":1,"radius":1}]
                """);
        });
        var initial = session.Current;
        var token = initial.Story.Wait!.Token;
        Assert.True(initial.CanWaitForText);
        Assert.Equal("explicit-text-wait-required", Send(session, new AdvanceSimulation(token, 60)).Failure!.Code);
        Assert.Equal("stale-or-wrong-wait", Send(session, new WaitForText(new(token.Value + 1))).Failure!.Code);
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(initial, session.Current);
        for (int tick = 0; tick <= delay; tick++)
        {
            var waited = Accept(session, new WaitForText(token));
            Assert.Equal("gameplay-wait", Assert.Single(waited.Observations).Kind);
            Assert.Equal(initial.Revision + tick + 1, session.Current.Revision);
            Assert.Equal(token, session.Current.Story.Wait!.Token);
        }
        var beforeAck = session.Current;
        Assert.Equal(expectedSeed, beforeAck.Exploration!.Party.MainSeed);
        var npc = beforeAck.Exploration.Entities[new("ferryman")];
        Assert.Equal((targetX * 384, targetY * 384), ((int)npc.Motion.XDestination, (int)npc.Motion.YDestination));
        var ack = Accept(session, new Acknowledge(token));
        Assert.Equal(beforeAck.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.Equal(beforeAck.Exploration, session.Current.Exploration);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(10, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        Assert.DoesNotContain(ack.Observations, row => row.Kind is "simulation-tick" or "gameplay-wait");
        Assert.False(session.Current.CanWaitForText);
        // Subsequent sleep remains mandatory and separate from the input-first accepting poll.
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 10));
        Assert.Equal(beforeAck.Story.SimulationTick + 10, session.Current.Story.SimulationTick);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BranchAndCallCarryRealPortraitCloseRatherThanInferringItFromText(bool executeClose)
    {
        var session = StartProgram("""
            [{"op":"call","target":{"program":"portrait-tail","instruction":0}},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"close-text"},{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"end"}]
            """, document =>
        {
            if (executeClose) document["start"]!["flags"]!.AsArray().Add(9);
            document["world"]!["programs"]!.AsArray().Add(System.Text.Json.Nodes.JsonNode.Parse("""
                {"id":"portrait-tail","instructions":[
                 {"op":"branch-flag","flag":9,"whenSet":false,"target":{"program":"portrait-tail","instruction":2}},
                 {"op":"close-portrait"},{"op":"end"}]}
                """));
        });
        Assert.Equal(executeClose, session.Current.CanWaitForText);
        Assert.Empty(session.Current.Story.Callers);
        if (executeClose) Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        else
        {
            Assert.IsType<UnknownPortraitWindow>(session.Current.Story.PortraitWindow);
            var before = session.Current;
            Assert.Equal("text-input-unavailable", Send(session, new WaitForText(before.Story.Wait!.Token)).Failure!.Code);
            Assert.Same(before, session.Current);
        }
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("skip")]
    [InlineData("missing")]
    [InlineData("absent")]
    [InlineData("open")]
    public void PortraitLookupSkipAndTextOnlyClosePreserveTheirActualGate(string mode)
    {
        string prefix = mode == "unknown" ? "" : """{"op":"close-portrait"},""";
        string entity = mode == "skip" ? "null" : "\"ferryman\"";
        string secondEntity = mode == "open" ? "\"ferryman\"" : "null";
        var session = StartProgram("[" + prefix + $$"""
             {"op":"open-portrait","entity":{{entity}},"flags":192},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"close-text"},
             {"op":"open-portrait","entity":{{secondEntity}},"flags":0},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"end"}]
            """, document => { if (mode != "missing") AddPortraitVisuals(document, mode == "absent" ? null : 7); });
        Assert.Equal(mode is "skip" or "absent", session.Current.CanWaitForText);
        if (mode == "open") Assert.Equal(new OpenPortraitWindow(7, 192), session.Current.Story.PortraitWindow);
        if (mode is "unknown" or "missing") Assert.IsType<UnknownPortraitWindow>(session.Current.Story.PortraitWindow);
    }

    [Fact]
    public void SingleTextAcknowledgementRunsItsExplicitCloseTailBeforeMandatorySleep()
    {
        var session = StartProgram("""
            [{"op":"close-portrait"},{"op":"open-portrait","entity":"ferryman","flags":128},
             {"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},
             {"op":"close-portrait"},{"op":"close-text"},{"op":"wait-ticks","ticks":10},{"op":"end"}]
            """, document => AddPortraitVisuals(document, 7));
        Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.False(session.Current.CanWaitForText);
        var result = Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Equal(new[] { "ClosePortrait", "CloseText", "WaitProgramTicks" },
            result.Observations.Where(row => row.Kind == "program-instruction").Select(row => row.Detail));
        Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        Assert.Equal(10, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
    }

    [Fact]
    public void PlainWaitKeepsEntityFailureAndRejectsStaleEnvelopes()
    {
        var session = StartProgram("""
            [{"op":"close-portrait"},
             {"op":"motion","entity":"ferryman","wait":false,"actions":[{"op":"native-call","symbol":"missing","source":"authored"}]},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"end"}]
            """);
        var before = session.Current;
        var envelope = new CommandEnvelope(before.SessionId, before.Revision, null, new WaitForText(before.Story.Wait!.Token));
        Assert.Equal("stale-input", session.Submit(envelope with { ExpectedRevision = before.Revision - 1 }).Failure!.Code);
        Assert.Equal("wrong-actor", session.Submit(envelope with { Actor = new("outsider") }).Failure!.Code);
        Assert.Same(before, session.Current);
        var failed = session.Submit(envelope);
        Assert.Equal(SessionStopReason.Unsupported, failed.StopReason);
        Assert.Equal("entity-action-stopped", Assert.Single(failed.Observations).Kind);
        Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal(before.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal("session-stopped", Send(session, envelope.Command).Failure!.Code);
    }

    private static void AddPortraitVisuals(System.Text.Json.Nodes.JsonNode document, int? portrait)
    {
        object Raster(int width, int height)
        {
            byte[] data = new byte[width * height * 4];
            return new { width, height, format = "rgba8", data = Convert.ToBase64String(data),
                sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)) };
        }
        document["world"]!["maps"]![0]!["entities"]![0]!["sprite"] = 30;
        document["world"]!["presentation"] = System.Text.Json.JsonSerializer.SerializeToNode(new
        {
            maps = document["world"]!["maps"]!.AsArray().Select(map => new
            { map = map!["id"]!.GetValue<string>(), atlas = Raster(128, 320), scale = 1,
                blocks = Enumerable.Range(0, 1024).Select(_ => new int[9]).ToArray() }).ToArray(),
            sprites = new[] { new { sprite = 30, directions = Enumerable.Range(0, 3).Select(_ => Raster(48, 24)).ToArray(), portrait, speech = 0 } },
            portraits = new[] { new { portrait = 7, raster = Raster(64, 64) } },
        });
    }

    [Theory]
    [InlineData(0x12341234u, 0, 0xECAB1234u, 2, 2)]
    [InlineData(0xC632A55Au, 2, 0x1091A55Au, 3, 1)]
    public void PlayerWaitAdvancesOneEligibleOpportunityWithSourceWaitAndRandomWalkRules(
        uint seed, int waitTicks, uint afterSeed, int targetX, int targetY)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["battle"]!["start"]!["mainSeed"] = seed;
            document["world"]!["maps"]![0]!["entities"]![0]!["actions"] = System.Text.Json.Nodes.JsonNode.Parse($$"""
                [{"op":"wait","ticks":{{waitTicks}}},{"op":"random-walk","x":2,"y":1,"radius":1}]
                """);
        });
        var initial = session.Current;
        for (int tick = 1; tick <= waitTicks; tick++)
        {
            Accept(session, new WaitAtInput());
            Assert.Equal(tick, session.Current.Exploration!.Entities[new("ferryman")].Motion.WaitTimer);
            Assert.Equal(seed, session.Current.Exploration.Party.MainSeed);
        }
        var before = session.Current;
        var result = Accept(session, new WaitAtInput());
        var npc = session.Current.Exploration!.Entities[new("ferryman")];
        // Source LCG advances the high word; scaled range 4 selects South or East above.
        Assert.Equal(afterSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal((targetX * 384, targetY * 384), ((int)npc.Motion.XDestination, (int)npc.Motion.YDestination));
        Assert.Equal((768, 384), ((int)npc.Motion.X, (int)npc.Motion.Y));
        Assert.Equal(0, npc.Motion.WaitTimer);
        Assert.True(npc.Busy);
        Assert.Equal(SessionStopReason.PlayerInput, result.StopReason);
        Assert.Equal(before.Revision + 1, session.Current.Revision);
        Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal("gameplay-wait", Assert.Single(result.Observations).Kind);
        Assert.Equal(initial.SessionId, session.Current.SessionId);
        Assert.Equal(initial.Exploration!.PlayerEntity, session.Current.Exploration.PlayerEntity);
        Assert.Equal(initial.Exploration.Party.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Accept(session, new WaitAtInput());
        npc = session.Current.Exploration.Entities[new("ferryman")];
        Assert.Equal((768 + (targetX - 2) * 96, 384 + (targetY - 1) * 96), ((int)npc.Motion.X, (int)npc.Motion.Y));
        Assert.Equal(afterSeed, session.Current.Exploration.Party.MainSeed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerWaitResolvesCompetingDestinationsInPhysicalSlotOrder(bool reverse)
    {
        var session = Start("harbor-arrival", document =>
        {
            var entities = document["world"]!["maps"]![0]!["entities"]!.AsArray();
            entities[0]!["actions"] = System.Text.Json.Nodes.JsonNode.Parse("""[{"op":"move","x":1,"y":0}]""");
            entities.Add(System.Text.Json.Nodes.JsonNode.Parse("""
                {"id":"other","position":{"x":4,"y":1},"facing":2,"speed":96,"visible":true,
                 "obstruction":true,"actions":[{"op":"move","x":-1,"y":0}]}
                """));
            if (reverse)
            {
                var first = entities[0]; entities.RemoveAt(0); entities.Add(first);
            }
        });
        var seed = session.Current.Exploration!.Party.MainSeed;
        Accept(session, new WaitAtInput());
        var firstNpc = session.Current.Exploration!.AllEntities[1];
        var secondNpc = session.Current.Exploration.AllEntities[2];
        Assert.Equal(reverse ? "other" : "ferryman", firstNpc.Entity.Value);
        Assert.Equal((1, 2), (firstNpc.Slot, secondNpc.Slot));
        Assert.Equal(1152, firstNpc.Motion.XDestination);
        Assert.Equal(1, firstNpc.ActionCursor);
        Assert.Equal(secondNpc.Motion.X, secondNpc.Motion.XDestination);
        Assert.Equal(0, secondNpc.ActionCursor);
        Accept(session, new WaitAtInput());
        var moved = session.Current.Exploration.AllEntities[1];
        Assert.Equal(firstNpc.Motion.X + (reverse ? -96 : 96), moved.Motion.X);
        Assert.Equal(0, session.Current.Exploration.AllEntities[2].ActionCursor);
        Assert.Equal(seed, session.Current.Exploration.Party.MainSeed);
    }

    [Fact]
    public void PlayerWaitThenMoveUsesChangedOccupancyAndRejectsAnotherWaitDuringPlayerMotion()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[{"op":"move","x":0,"y":-1}]},{"op":"end"}]
            """);
        var blocked = Accept(session, new Move(ExplorationDirection.East));
        Assert.Contains(blocked.Observations, row => row.Kind == "movement-blocked");
        Assert.Equal(new MapPosition(1, 1), session.Current.Exploration!.PlayerEntity.Position);
        var before = session.Current;
        foreach (SessionCommand wrong in new SessionCommand[]
        {
            new Acknowledge(new(1)), new CompletePresentation(new(1), PresentationCueKind.SoundWait),
        })
        {
            Assert.NotNull(Send(session, wrong).Failure);
            Assert.Same(before, session.Current);
        }
        Accept(session, new WaitAtInput());
        var moving = Accept(session, new Move(ExplorationDirection.East));
        Assert.Contains(moving.Observations, row => row.Kind == "movement-requested");
        var pending = session.Current;
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(pending, session.Current);
        // Mandatory motion still requires the existing token-bound simulation command.
        Accept(session, new AdvanceSimulation(pending.Story.Wait!.Token, 600));
        Assert.Equal(new MapPosition(2, 1), session.Current.Exploration!.PlayerEntity.Position);
        Accept(session, new WaitAtInput());
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Theory]
    [InlineData("dialogue")]
    [InlineData("choice")]
    [InlineData("ticks")]
    [InlineData("audio")]
    [InlineData("open-text")]
    [InlineData("player-actions")]
    public void PlayerWaitRejectsOtherExplorationConsumersWithoutAdvancing(string consumer)
    {
        string instructions = consumer switch
        {
            "dialogue" => """{"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":null},""",
            "choice" => """{"op":"yes-no","flag":10},""",
            "ticks" => """{"op":"wait-ticks","ticks":2},""",
            "audio" => """{"op":"present","kind":"SoundWait","resource":null,"entity":null,"position":null},""",
            "open-text" => """{"op":"text-cursor","text":100},{"op":"show-text","mode":"continued","speaker":null,"waitForAcknowledgement":false},""",
            _ => """{"op":"motion","entity":"traveler","wait":false,"actions":[{"op":"move","x":0,"y":1}]},""",
        };
        var session = StartProgram("[" + instructions + """{"op":"end"}]""");
        var before = session.Current;
        var result = Send(session, new WaitAtInput());
        Assert.Equal("field-input-unavailable", result.Failure!.Code);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(before.Story.Wait?.Token ?? new(1))).Failure!.Code);
        Assert.Empty(result.Observations);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void PlayerWaitRejectsBattleControlAndSceneWithoutGrantingBattleTicks()
    {
        var session = Start("stone-court");
        var battle = session.Current;
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(battle, session.Current);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(new(1))).Failure!.Code);
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new("raider")));
        Accept(session, new Confirm());
        var scene = session.Current;
        Assert.NotNull(scene.BattleScene);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(scene.BattleScene!.Token)).Failure!.Code);
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Equal("invalid-battle-tick", Send(session, new AdvanceSimulation(scene.BattleScene!.Token)).Failure!.Code);
        Assert.Same(scene, session.Current);
    }

    [Fact]
    public void PlayerWaitUsesSessionRevisionAndDoesNotAdvanceForStaleOrWrongActorInput()
    {
        var session = Start("harbor-arrival");
        var before = session.Current;
        var envelope = new CommandEnvelope(before.SessionId, before.Revision, null, new WaitAtInput());
        Assert.Equal("stale-input", session.Submit(envelope with { SessionId = Guid.NewGuid() }).Failure!.Code);
        Assert.Equal("wrong-actor", session.Submit(envelope with { Actor = new("outsider") }).Failure!.Code);
        Assert.Same(before, session.Current);
        Assert.Null(session.Submit(envelope).Failure);
        var after = session.Current;
        Assert.Equal(before.Story.SimulationTick + 1, after.Story.SimulationTick);
        Assert.Equal(before.Exploration!.Party.MainSeed, after.Exploration!.Party.MainSeed);
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        Assert.Same(after, session.Current);
    }

    [Fact]
    public void PlayerWaitPreservesEntityFailureAndCannotContinueTheStoppedConsumer()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
              {"op":"native-call","symbol":"unimplemented-service","source":"authored"}]},{"op":"end"}]
            """);
        var before = session.Current;
        var result = Send(session, new WaitAtInput());
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure!.Kind);
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal(before.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal("entity-action-stopped", Assert.Single(result.Observations).Kind);
        var stopped = session.Current;
        Assert.Equal("session-stopped", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(stopped, session.Current);
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(63, false)]
    public void OrdinaryWarpPublishesOriginOnlyAfterItsDestinationIsAdmitted(int destinationX, bool accepted)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["display"] = System.Text.Json.Nodes.JsonNode.Parse("""{"period":3,"base":{"color2":3822,"color3":2184},"current":{"color2":3822,"color3":2184},"visibility":"base-restored"}""");
            foreach (var map in document["world"]!["maps"]!.AsArray())
                map!["basePalette"] = System.Text.Json.Nodes.JsonNode.Parse("""{"color2":3822,"color3":2184}""");
            document["world"]!["maps"]![0]!["events"]!.AsArray().Add(System.Text.Json.Nodes.JsonNode.Parse($$"""
                {"kind":"warp","x":2,"y":1,"map":"quay","position":{"x":{{destinationX}},"y":2},
                 "facing":3,"marker":null,"requiredFlag":null,"requiredValue":true}
                """));
        });
        var before = session.Current;
        var result = Send(session, new Move(ExplorationDirection.East));
        if (accepted)
        {
            Assert.Null(result.Failure);
            Assert.Equal(before.Exploration!.PlayerEntity.Position, session.Current.Exploration!.PlayerEntity.Position);
            result = Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 600));
            Assert.Equal(before.Exploration!.PlayerEntity.Position, session.Current.Exploration!.PlayerEntity.Position);
            Assert.IsType<FullFadeWait>(session.Current.Story.Wait);
            Assert.Equal(1, session.Current.Story.SimulationTick);
            Assert.Equal(new[] { "simulation-tick", "warp-started", "full-fade-started" }, result.Observations.Take(3).Select(row => row.Kind));
        }
        else
        {
            Assert.NotNull(result.Failure);
            Assert.Equal(before.Exploration!.PlayerEntity.Position, session.Current.Exploration!.PlayerEntity.Position);
            Assert.DoesNotContain(result.Observations, row => row.Kind == "warp-started");
        }
    }

    [Fact]
    public void OpenTextCanWaitForFiniteSoundAndPreviousMusicBeforeAcceptingInput()
    {
        var session = StartProgram("""
            [{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"waitForAcknowledgement":false},
             {"op":"present","kind":"SoundWait","resource":null,"entity":null,"position":null},
             {"op":"present","kind":"PreviousMusic","resource":null,"entity":null,"position":null},
             {"op":"wait-text-input"},{"op":"close-text"},
             {"op":"set-flag","flag":7,"value":true},{"op":"end"}]
            """);
        var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(PresentationCueKind.SoundWait, sound.Cue.Kind);
        Assert.Equal(100, Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow).Text);
        var waiting = session.Current;
        Assert.NotNull(Send(session, new Acknowledge(sound.Token)).Failure);
        Assert.Same(waiting, session.Current);
        Accept(session, new CompletePresentation(sound.Token, PresentationCueKind.SoundWait));
        var previous = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(PresentationCueKind.PreviousMusic, previous.Cue.Kind);
        Assert.NotNull(Send(session, new Acknowledge(previous.Token)).Failure);
        Accept(session, new CompletePresentation(previous.Token, PresentationCueKind.PreviousMusic));
        var input = Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Assert.Equal(100, input.Text);
        Assert.Equal(101, session.Current.Story.TextCursor);
        Assert.DoesNotContain(7, session.Current.Story.Flags);
        Accept(session, new Acknowledge(input.Token));
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Contains(7, session.Current.Story.Flags);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void WaitingForTextInputWithoutAnOpenWindowFailsAtThatInstruction()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = System.Text.Json.Nodes.JsonNode.Parse("""
                [{"op":"wait-text-input"},{"op":"end"}]
                """);
        });
        var result = Send(session, new Interact(new("ferryman")));
        Assert.Equal("text-input-without-window", result.Failure!.Code);
        Assert.Equal(new ProgramLocation("invitation", 0), session.Current.Story.Cursor);
    }

    [Theory]
    [InlineData("harbor-arrival", "ferryman", 2, 1, 3, 1)]
    [InlineData("hill-passage", "watcher", 3, 2, 3, 1)]
    public void DialogueMotionCallTransferAndBothIntroHooksReachExistingBattle(string package, string npc,
        int fromX, int fromY, int toX, int toY)
    {
        var session = Start(package);
        var identity = session.Current.SessionId;
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        var initialSeed = session.Current.Exploration!.Party.MainSeed;
        var dialogue = Accept(session, new Interact(new(npc)));
        var wait = Assert.IsType<DialogueWait>(dialogue.Snapshot.Story.Wait);
        Assert.Equal(1, session.Current.Story.Cursor!.Value.Instruction);
        Assert.DoesNotContain(10, session.Current.Story.Flags);
        Assert.Same(session.Current, Send(session, new Acknowledge(new(wait.Token.Value + 1))).Snapshot);
        Accept(session, new Acknowledge(wait.Token));
        var choice = Assert.IsType<ChoiceWait>(session.Current.Story.Wait);
        Accept(session, new ChooseDialogue(choice.Token, true));
        var motionWait = Assert.IsType<EntityWait>(session.Current.Story.Wait);
        Assert.Equal(new MapPosition(fromX, fromY), session.Current.Exploration!.Entities[new(npc)].Position);
        Accept(session, new AdvanceSimulation(motionWait.Token));
        var first = session.Current.Exploration.Entities[new(npc)].Motion;
        Assert.Equal((fromX * 384, fromY * 384), ((int)first.X, (int)first.Y));
        Assert.True(first.IsMoving);
        Assert.Equal(initialSeed, session.Current.Exploration.Party.MainSeed);
        for (int i = 0; session.Current.Story.Wait is EntityWait && i < 10; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
        Assert.Equal(new MapPosition(toX, toY), session.Current.Exploration.Entities[new(npc)].Position);
        Assert.Contains(12, session.Current.Story.Flags);
        var timer = Assert.IsType<TickWait>(session.Current.Story.Wait);
        Assert.Equal(2, timer.Remaining);
        Accept(session, new AdvanceSimulation(timer.Token));
        Assert.Equal(1, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        var transferred = Accept(session, new AdvanceSimulation(timer.Token));
        Assert.Contains(transferred.Observations, observation => observation.Kind == "map-transferred");
        Assert.DoesNotContain(transferred.Observations, observation => observation.Kind == "warp-started");
        Assert.Contains(14, session.Current.Story.Flags);
        Assert.Contains(15, session.Current.Story.Flags);
        Assert.DoesNotContain(20, session.Current.Story.Flags);
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        var entered = Accept(session, new Acknowledge(session.Current.Story.Wait.Token));
        Assert.Equal(identity, session.Current.SessionId);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Null(session.Current.Exploration);
        Assert.NotNull(session.Current.Selection);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.Equal(new[] { 10, 12, 13, 14, 15, 16, 20 }, session.Current.Story.Flags);
        string[] kinds = entered.Observations.Select(observation => observation.Kind).ToArray();
        Assert.True(Array.IndexOf(kinds, "battle-initialized") < Array.IndexOf(kinds, "battle-loaded"));
        Assert.True(Array.IndexOf(kinds, "battle-loaded") < Array.IndexOf(kinds, "round-started"));
        var flags = session.Current.Story.Flags;
        Stay(session);
        Assert.Equal(flags, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData("harbor-arrival", "ferryman")]
    [InlineData("hill-passage", "watcher")]
    public void DecliningReturnsControlAndAReplayedChoiceCannotMutateFlags(string package, string npc)
    {
        var session = Start(package);
        var position = session.Current.Exploration!.PlayerEntity.Position;
        Accept(session, new Interact(new(npc)));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new ChooseDialogue(token, false));
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(position, session.Current.Exploration!.PlayerEntity.Position);
        Assert.Empty(session.Current.Story.Flags);
        var before = session.Current;
        Assert.NotNull(Send(session, new ChooseDialogue(token, true)).Failure);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void UnsupportedInstructionKeepsItsCursorAndEarlierCommittedFlag()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = System.Text.Json.Nodes.JsonNode.Parse("""
                [{"op":"set-flag","flag":7,"value":true},
                 {"op":"native-call","symbol":"unimplemented-service","source":"authored"},
                 {"op":"set-flag","flag":8,"value":true},{"op":"end"}]
                """);
        });
        var result = Send(session, new Interact(new("ferryman")));
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure!.Kind);
        Assert.Equal(new ProgramLocation("invitation", 1), session.Current.Story.Cursor);
        Assert.Equal(new[] { 7 }, session.Current.Story.Flags);
        Assert.Equal(SessionStopReason.Unsupported, session.Current.StopReason);
    }

    [Fact]
    public void BlockedMovementStillFacesTheObstacleWithoutRelocatingOrAdvancingRng()
    {
        var session = Start("harbor-arrival");
        var before = session.Current;
        var result = Accept(session, new Move(ExplorationDirection.East));
        Assert.Equal(before.Exploration!.PlayerEntity.Position, session.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(before.Exploration.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Contains(result.Observations, observation => observation.Kind == "movement-blocked");
        Accept(session, new Interact(new("ferryman")));
    }

    [Fact]
    public void ContinuedTextSurvivesItsAcknowledgementAndTickWaitUntilExplicitClose()
    {
        var session = StartProgram("""
            [{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"continued","speaker":"ferryman","speakerFlags":192},
             {"op":"wait-ticks","ticks":2},{"op":"close-text"},
             {"op":"wait-ticks","ticks":1},{"op":"end"}]
            """);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        var text = Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(100, text.Text);
        Assert.Equal(192, text.SpeakerFlags);
        Assert.Equal(192, Assert.IsType<DialogueWait>(session.Current.Story.Wait).SpeakerFlags);
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Equal(text, session.Current.Story.TextWindow);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        var wait = session.Current.Story.Wait!.Token;
        Accept(session, new AdvanceSimulation(wait, 600));
        Assert.Equal(2, session.Current.Story.SimulationTick);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(1, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        var stopped = session.Current;
        Assert.NotNull(Send(session, new AdvanceSimulation(wait)).Failure);
        Assert.Same(stopped, session.Current);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(3, session.Current.Story.SimulationTick);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void SpriteRefreshRetainsItsCursorUntilTheMatchingPresentationCompletes()
    {
        var session = StartProgram("""
            [{"op":"set-flag","flag":7,"value":true},
             {"op":"motion","entity":"ferryman","wait":true,"actions":[
               {"op":"speed","x":32,"y":48},
               {"op":"acceleration","x":2,"y":3},
               {"op":"flags","field":"a","mask":128,"value":128},
               {"op":"sprite-size","size":24},
               {"op":"refresh-sprite","source":"authored/native-refresh"},
               {"op":"face","facing":2}]},
             {"op":"set-flag","flag":8,"value":true},{"op":"end"}]
            """);
        var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Null(result.Failure);
        Assert.Equal(new ProgramLocation("invitation", 1), session.Current.Story.Cursor);
        Assert.Equal(new[] { 7 }, session.Current.Story.Flags);
        Assert.Equal(1, session.Current.Story.SimulationTick);
        Assert.Equal(24, session.Current.Exploration!.SpriteSize);
        var entity = session.Current.Exploration.Entities[new("ferryman")];
        Assert.Equal(4, entity.ActionCursor);
        Assert.Equal((32, 48, 2, 3, 160), ((int)entity.Motion.XSpeed, (int)entity.Motion.YSpeed,
            (int)entity.Motion.XAcceleration, (int)entity.Motion.YAcceleration, (int)entity.Motion.FlagsA));
        Assert.True(entity.WaitingForSprite);
        var pending = session.Current;
        Assert.NotNull(Send(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest + 1)).Failure);
        Assert.Same(pending, session.Current);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 3));
        Assert.Equal(4, session.Current.Exploration!.Entities[new("ferryman")].ActionCursor);
        Assert.DoesNotContain(8, session.Current.Story.Flags);
        Accept(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest));
        Assert.NotNull(Send(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest)).Failure);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Contains(8, session.Current.Story.Flags);
        Assert.Equal(2, session.Current.Exploration.Entities[new("ferryman")].Motion.Facing);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void NonblockingActionOperationsRunInOrderBeforeTheFirstMovementTick()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":true,"actions":[
               {"op":"speed","x":192,"y":192},
               {"op":"face","facing":3},{"op":"move","x":1,"y":0}]},
             {"op":"set-flag","flag":11,"value":true},{"op":"end"}]
            """);
        var initial = session.Current.Exploration!.Entities[new("ferryman")].Motion;
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var scheduled = session.Current.Exploration.Entities[new("ferryman")];
        Assert.Equal(initial.X, scheduled.Motion.X);
        Assert.Equal(initial.X + 384, scheduled.Motion.XDestination);
        Assert.Equal(3, scheduled.Motion.Facing);
        Assert.Equal(3, scheduled.ActionCursor);
        Assert.DoesNotContain(11, session.Current.Story.Flags);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(initial.X + 192, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(initial.X + 384, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Contains(11, session.Current.Story.Flags);
        Assert.Equal(3, session.Current.Story.SimulationTick);
    }

    [Fact]
    public void ReplacingAnActionStreamChangesTheRemainingPathWithoutSnappingCurrentMotion()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
               {"op":"move","x":1,"y":0},{"op":"move","x":1,"y":0}]},
             {"op":"wait-ticks","ticks":1},
             {"op":"motion","entity":"ferryman","wait":true,"actions":[
               {"op":"face","facing":3}]},{"op":"end"}]
            """);
        var initial = session.Current.Exploration!.Entities[new("ferryman")].Motion;
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(initial.X, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 600));
        Assert.Equal(initial.X + 384, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void SeenIntroFlagSkipsBothHooksWhileStillInitializingAndLoadingTheEncounter()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["flags"] = System.Text.Json.Nodes.JsonNode.Parse("[20]");
        });
        Accept(session, new Interact(new("ferryman")));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Accept(session, new ChooseDialogue(session.Current.Story.Wait!.Token, true));
        for (int ticks = 0; session.Current.Mode == SessionMode.Exploration && ticks < 30; ticks++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.DoesNotContain(15, session.Current.Story.Flags);
        Assert.DoesNotContain(16, session.Current.Story.Flags);
        Assert.Contains(20, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData("dialogue")]
    [InlineData("choice")]
    [InlineData("ticks")]
    public void FreshBattleStartRetainsProgramControlUntilItsWaitCompletes(string waiting)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["map"] = "yard-map";
            document["start"]!["flags"] = System.Text.Json.Nodes.JsonNode.Parse("[13]");
            document["world"]!["maps"]![1]!["battle"]!["before"] = null;
            string instructions = waiting switch
            {
                "dialogue" => """{"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":null}""",
                "choice" => """{"op":"yes-no","flag":10}""",
                _ => """{"op":"wait-ticks","ticks":2}""",
            };
            document["world"]!["programs"]!.AsArray().Single(program => program!["id"]!.GetValue<string>() == "battle-start")!["instructions"] =
                System.Text.Json.Nodes.JsonNode.Parse("[" + instructions + """,{"op":"set-flag","flag":16,"value":true},{"op":"end"}]""");
        });
        var pending = session.Current;
        Assert.Equal(SessionMode.Battle, pending.Mode);
        Assert.False(pending.HasBattleControl);
        Assert.Equal(0, pending.Battle.Round);
        Assert.Null(pending.Selection);
        Assert.DoesNotContain(16, pending.Story.Flags);
        Assert.Equal("program-owns-control", Send(session, new Confirm()).Failure!.Code);
        Assert.Same(pending, session.Current);
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(pending, session.Current);
        var token = pending.Story.Wait!.Token;
        SessionCommand command = waiting switch
        {
            "dialogue" => new Acknowledge(token),
            "choice" => new ChooseDialogue(token, true),
            _ => new AdvanceSimulation(token, 2),
        };
        Accept(session, command);
        Assert.Equal(pending.SessionId, session.Current.SessionId);
        Assert.True(session.Current.HasBattleControl);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.NotNull(session.Current.Selection);
        Assert.Contains(16, session.Current.Story.Flags);
        if (waiting == "choice") Assert.Contains(10, session.Current.Story.Flags);
        Accept(session, new Confirm());
        Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
    }

    [Fact]
    public void SimulationBatchesConsumeElapsedTicksAndStopBeforeAcknowledgingTheNextDialogue()
    {
        var session = StartProgram("""
            [{"op":"wait-ticks","ticks":120},{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null},
             {"op":"set-flag","flag":7,"value":true},{"op":"end"}]
            """);
        var timer = session.Current.Story.Wait!.Token;
        Accept(session, new AdvanceSimulation(timer, 30));
        Assert.Equal(30, session.Current.Story.SimulationTick);
        Assert.Equal(90, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        Accept(session, new AdvanceSimulation(timer, 60));
        Assert.Equal(90, session.Current.Story.SimulationTick);
        Assert.Equal(30, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        Accept(session, new AdvanceSimulation(timer, 60));
        Assert.Equal(120, session.Current.Story.SimulationTick);
        var dialogue = Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Assert.DoesNotContain(7, session.Current.Story.Flags);
        var pending = session.Current;
        Assert.Equal("stale-or-wrong-wait", Send(session, new AdvanceSimulation(timer, 30)).Failure!.Code);
        Assert.Same(pending, session.Current);
        Accept(session, new Acknowledge(dialogue.Token));
        Assert.Contains(7, session.Current.Story.Flags);
        Assert.Equal(120, session.Current.Story.SimulationTick);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BackgroundMotionConsumesTheBatchWithoutReleasingExistingInputOrDialogue(bool dialogue)
    {
        var session = StartProgram("""
            [{"op":"set-flag","flag":7,"value":true},
             {"op":"motion","entity":"ferryman","wait":false,"actions":[
               {"op":"speed","x":1,"y":1},{"op":"move","x":1,"y":0}]},
            """ + (dialogue ? """
             {"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":null},
            """ : "") + """{"op":"end"}]""");
        var wait = session.Current.Story.Wait;
        var expectedStop = dialogue ? SessionStopReason.PresentationWait : SessionStopReason.PlayerInput;
        Assert.Equal(expectedStop, session.Current.StopReason);
        Accept(session, new AdvanceSimulation(wait?.Token));
        var before = session.Current;
        var npc = before.Exploration!.Entities[new("ferryman")];
        var result = Accept(session, new AdvanceSimulation(wait?.Token, 60));
        Assert.Equal(before.Story.SimulationTick + 60, session.Current.Story.SimulationTick);
        Assert.Equal(npc.Motion.X + 60, session.Current.Exploration!.Entities[new("ferryman")].Motion.X);
        Assert.Equal(expectedStop, result.StopReason);
        Assert.Same(wait, session.Current.Story.Wait);
        Assert.Equal(before.SessionId, session.Current.SessionId);
        Assert.Equal(before.Story.Flags, session.Current.Story.Flags);
        Assert.Equal(before.Exploration.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(60, result.Observations.Count(observation => observation.Kind == "simulation-tick"));
        Accept(session, new AdvanceSimulation(wait?.Token, 30));
        Assert.Equal(before.Story.SimulationTick + 90, session.Current.Story.SimulationTick);
        Assert.Equal(npc.Motion.X + 90, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(expectedStop, session.Current.StopReason);
    }

    [Fact]
    public void CompletingForegroundMotionYieldsAtNewFieldInputBeforeContinuingBackgroundTicks()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
               {"op":"speed","x":1,"y":1},{"op":"move","x":1,"y":0}]},
             {"op":"motion","entity":"traveler","wait":true,"actions":[
               {"op":"move","x":1,"y":0}]},{"op":"end"}]
            """);
        var startX = session.Current.Exploration!.Entities[new("ferryman")].Motion.X;
        var wait = Assert.IsType<EntityWait>(session.Current.Story.Wait).Token;
        Accept(session, new AdvanceSimulation(wait, 60));
        Assert.Equal(5, session.Current.Story.SimulationTick);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Null(session.Current.Story.Wait);
        Assert.Equal(startX + 4, session.Current.Exploration!.Entities[new("ferryman")].Motion.X);
        var boundary = session.Current;
        Assert.Equal("stale-or-wrong-wait", Send(session, new AdvanceSimulation(wait, 55)).Failure!.Code);
        Assert.Same(boundary, session.Current);
        Accept(session, new AdvanceSimulation(null, 2));
        Assert.Equal(7, session.Current.Story.SimulationTick);
        Assert.Equal(startX + 6, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    private static GameSession StartProgram(string instructions, Action<System.Text.Json.Nodes.JsonNode>? change = null) => Start("harbor-arrival", document =>
    {
        document["world"]!["programs"]![0]!["instructions"] = System.Text.Json.Nodes.JsonNode.Parse(instructions);
        document["start"]!["program"] = System.Text.Json.Nodes.JsonNode.Parse("""
            {"program":"invitation","instruction":0}
            """);
        change?.Invoke(document);
    });

    [Fact]
    public void UnsupportedTargetSetupKeepsTheSourceMapAndTheCompletedProgramPrefix()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["maps"]![1]!["setup"] = System.Text.Json.Nodes.JsonNode.Parse("""
                {"default":"base","variants":[{"flag":10,"setup":"closed"}]}
                """);
        });
        var originalMap = session.Current.Exploration!.Map;
        Accept(session, new Interact(new("ferryman")));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Accept(session, new ChooseDialogue(session.Current.Story.Wait!.Token, true));
        SessionResult? result = null;
        for (int tick = 0; session.Current.StopReason == SessionStopReason.SimulationWait && tick < 100; tick++)
            result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal("map-setup", result!.Failure!.Code);
        Assert.Equal(originalMap, session.Current.Exploration!.Map);
        Assert.Contains(13, session.Current.Story.Flags);
        Assert.DoesNotContain(14, session.Current.Story.Flags);
    }
}

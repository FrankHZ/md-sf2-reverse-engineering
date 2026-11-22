using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class SettledFieldSnapshotTests
{
    private static readonly SessionRules Rules = RuleCompositions.Sf2();
    private static readonly MapId Visit = new("unlisted-field");
    private static readonly MapId Refuge = new("refuge");
    private static readonly EntityRef Player = new("walker");
    private static readonly EntityRef Wanderer = new("wanderer");
    private static readonly EntityRef Patrol = new("patrol");
    private static readonly EntityRef Merchant = new("merchant");
    private static readonly EntityRef Companion = new("companion");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheNextOrdinaryServicesContinueAHiddenWanderersPhaseAndClosedIdle(bool inMotion)
    {
        var loop = new EntityActionProgram([new FaceEntity(2), new RandomWalkEntity(new(20, 20), 2),
            new WaitEntityTicks(3), new JumpEntityAction(1), new StopEntityActions()]);
        var patrol = new EntityActionProgram([new MoveEntityRelative(0, 1), new WaitEntityTicks(3),
            new MoveEntityRelative(0, -1), new WaitEntityTicks(3), new JumpEntityAction(0), new StopEntityActions()]);
        List<StoryInstruction> enter = [
            new StartEntityMotion(Merchant, new([new IdleEntityAction()]), false),
            new FollowEntity(Companion, Player, -24, 0), new ParkRaft(new(Visit, new(41, 41))),
        ];
        if (inMotion) enter.Add(new StartEntityMotion(Wanderer, loop, false));
        enter.Add(new EndProgram());
        var session = Start(
            [new(Wanderer, new(20, 20), 0, 96, Actions: inMotion ? null : loop, Visible: false),
                new(Merchant, new(30, 30), 0, 32, Actions: loop), new(Companion, new(4, 4), 0, 32),
                new(Patrol, new(40, 40), 0, 32, Actions: patrol)],
            [new("enter", enter)]);
        var before = session.Current;
        for (int i = 0; i < 12; i++)
        {
            var resident = before.Exploration!.Entities[Wanderer];
            if (resident.ActionCursor == 2 && (inMotion
                ? resident.WaitingForMotion && resident.Motion.IsMoving
                : !resident.Motion.IsMoving && resident.Motion.WaitTimer > 0))
                break;
            before = Submit(session, before, new WaitAtInput());
        }
        var actor = before.Exploration!.Entities[Wanderer];
        Assert.False(actor.Visible);
        Assert.Equal(2, actor.ActionCursor);
        Assert.Equal(inMotion, actor.Motion.IsMoving);
        if (!inMotion) Assert.InRange(actor.Motion.WaitTimer, 1, 3);
        Assert.True(before.Exploration.Entities[Merchant].IsScriptIdle);
        Assert.Null(before.Exploration.Entities[Companion].Actions);
        Assert.NotNull(before.Exploration.Entities[Companion].Follower);

        var record = new FieldSnapshotRecord(1, 1, DateTimeOffset.UnixEpoch, new(Capture(session.Definition, before)));
        var bytes = OpeningSaveJson.WriteRecord(record);
        var saved = Assert.IsType<FieldSnapshotRecord>(OpeningSaveJson.ReadRecord(bytes, 1));
        var detached = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(session.Definition, saved, Rules)).Result.Snapshot;
        Assert.Equal(before.Story.Raft, detached.Story.Raft);
        Assert.Equal(before.Story.SimulationTick, detached.Story.SimulationTick);
        Assert.Equal(Refuge, detached.Story.EgressMap);
        Assert.Equal(Visit, detached.Exploration!.Map);
        var control = before;
        uint initialSeed = control.Exploration!.Party.MainSeed;
        var patrolMotion = control.Exploration.Entities[Patrol].Motion;
        for (int i = 0; i < 10; i++)
        {
            control = Submit(session, control, new WaitAtInput());
            detached = Submit(session, detached, new WaitAtInput());
            EqualNext(control, detached);
        }
        Assert.NotEqual(initialSeed, control.Exploration!.Party.MainSeed);
        Assert.NotEqual(patrolMotion, control.Exploration.Entities[Patrol].Motion);
        foreach (var direction in new[] { ExplorationDirection.East, ExplorationDirection.East, ExplorationDirection.South })
        {
            control = FieldVisitSnapshotTests.Perform(session, control, new Move(direction));
            detached = FieldVisitSnapshotTests.Perform(session, detached, new Move(direction));
            EqualNext(control, detached);
        }
        Assert.NotEqual(new MapPosition(4, 4), detached.Exploration!.Entities[Companion].Position);
        Assert.True(detached.Exploration.Entities[Merchant].IsScriptIdle);
    }

    [Fact]
    public void CompletedGrantAndJoinKeepTheirReceiptAndDoNotReplayOnTheNextVisitInput()
    {
        var session = Start([], [
            new("grant", [new BranchFlag(900, true, new("grant", 7)), new TryAddPartyItem(5, new("grant", 7)),
                new NameDialogueItem(6), new WaitProgramTicks(1), new JoinPartyMember(1), new WriteFlag(900, true),
                new SetTextCursor(71), new EndProgram()]),
            new("service", [new OpenChurch(new(1, 1, 10, 1, 1)), new EndProgram()]),
            new("read-receipt", [new ShowText(TextDisplayMode.Single, null, ExplicitWindows: true,
                Narration: true, UseItemReceipt: true), new CloseText(), new EndProgram()]),
        ], [Step(5, 4, "grant"), Step(5, 5, "service"), Step(6, 4, "read-receipt")]);

        var pending = Submit(session, session.Current, new Move(ExplorationDirection.East));
        for (int i = 0; i < 100 && pending.Story.Wait is not TickWait; i++)
            pending = Submit(session, pending, new AdvanceSimulation(pending.Story.Wait?.Token));
        Assert.IsType<TickWait>(pending.Story.Wait);
        Assert.DoesNotContain(900, pending.Story.Flags);
        Assert.Equal(new StoryItemReceipt(0, 5), pending.Story.ItemReceipt);
        Refuse(session, pending, "field-snapshot-busy");
        Assert.Single(pending.Exploration!.Party.Actors[0].SourceLoadout!.Items, word => word == 5);
        var completed = FieldVisitSnapshotTests.Settle(session, pending);
        Assert.Contains(900, completed.Story.Flags);
        Assert.Equal(new[] { "ally-0", "ally-1" }, completed.Exploration!.Party.ActiveAllies!.Select(actor => actor.Value));
        Assert.Equal(new[] { 1 }, completed.Story.PartyLists!.Reserve); // JoinForce's counted list precedes the active bit.

        var captured = Capture(session.Definition, completed);
        var restored = captured.Build(session.Definition, Rules).Snapshot;
        Assert.Equal(completed.Story.ItemReceipt, restored.Story.ItemReceipt);
        Assert.Equal(completed.Story.DialogueItem, restored.Story.DialogueItem);
        Assert.Equal(completed.Story.PartyLists.Reserve, restored.Story.PartyLists!.Reserve);
        foreach (var start in new[] { completed, restored })
        {
            var away = FieldVisitSnapshotTests.Perform(session, start, new Move(ExplorationDirection.West));
            var returned = FieldVisitSnapshotTests.Perform(session, away, new Move(ExplorationDirection.East));
            Assert.Single(returned.Exploration!.Party.Actors[0].SourceLoadout!.Items, word => word == 5);
            Assert.Equal(new[] { "ally-0", "ally-1" }, returned.Exploration.Party.ActiveAllies!.Select(actor => actor.Value));

            var reading = AdvanceUntil<FieldTextWait>(session, returned, new Move(ExplorationDirection.East));
            var text = Assert.IsType<FieldTextWait>(reading.Story.Wait);
            Assert.Equal((byte)5, Assert.Single(text.DisplayNames!).ItemId);
            Assert.Equal(72, reading.Story.TextCursor);
            var service = AdvanceUntil<ChurchWait>(session, returned, new Move(ExplorationDirection.South));
            Refuse(session, service, "field-snapshot-busy");
            Assert.Equal(captured.Party.Gold, service.Exploration!.Party.Gold);
        }
    }

    [Fact]
    public void TheNextPaletteCommandUsesTheSavedCopyAndSimulationClock()
    {
        ushort[] lookup = Enumerable.Range(0, 512).Select(index => PaletteColorTransition.Word(511 - index)).ToArray();
        var session = Start([], [
            new("enter", [new CallProgram(new("colors", 0), ActivateEntities: true), new EndProgram()]),
            new("colors", [new RemapPaletteColors(lookup), new ReturnProgram()]),
            new("restore-colors", [new RemapPaletteColors(null), new EndProgram()]),
        ], [Step(5, 4, "restore-colors")]);
        var live = session.Current;
        Assert.True(live.CanWaitAtInput);
        Refuse(session, live, "field-snapshot-busy");
        for (int tick = 0; tick < 32; tick++) live = Submit(session, live, new WaitAtInput());
        Assert.True(live.Story.EntityServices);
        Assert.Equal(0xEEE, live.Story.Display!.Colors!.ColorsAt(live.Story.SimulationTick)[0]);
        var detached = Capture(session.Definition, live).Build(session.Definition, Rules).Snapshot;
        Assert.Equal(live.Story.EntityServices, detached.Story.EntityServices);
        var controls = new[] { live, detached }.Select(start =>
            FieldVisitSnapshotTests.Perform(session, start, new Move(ExplorationDirection.East))).ToArray();
        Assert.Equal(controls[0].Story.SimulationTick, controls[1].Story.SimulationTick);
        foreach (var end in controls)
        {
            var colors = end.Story.Display!.Colors!;
            Assert.Equal(end.Story.SimulationTick, colors.StartedAt);
            Assert.Equal(0xEEE, colors.ColorsAt(colors.StartedAt)[0]);
            Assert.Equal(0, colors.ColorsAt(colors.StartedAt + 32)[0]);
        }
    }

    [Fact]
    public void OrdinaryWarpAndBattleOwnersAndStaleConfirmationCannotCaptureSettledWalking()
    {
        var warp = new ExplorationEvent(ExplorationEventKind.Warp, 5, 4, null, null, Visit, new(8, 8),
            LoadMode: MapLoadMode.Preserve, RequiredMarker: 0x1000);
        var session = Start([], [], [warp]);
        var before = session.Current;
        var availability = SettledFieldSnapshot.Query(session.Definition, before, Rules);
        Assert.True(availability.Allowed, availability.Error);
        var moving = AdvanceUntil<FullFadeWait>(session, before, new Move(ExplorationDirection.East));
        Assert.NotNull(moving.Story.Warp);
        Refuse(session, moving, "field-snapshot-busy");
        Assert.Equal("save-stale-revision", SettledFieldSnapshot.Capture(session.Definition, moving, Rules,
            availability.SessionId, availability.Revision).Error);
        Assert.Equal("save-stale-revision", SettledFieldSnapshot.Capture(session.Definition, before, Rules,
            Guid.NewGuid(), before.Revision).Error);

        // These ownership registers cannot pass merely because the input shell reports PlayerInput.
        Refuse(session, before.WithStory(before.Story.Copy(null, battleRequest: true)), "field-snapshot-busy");
        Refuse(session, before.WithStory(before.Story.Copy(null, scriptWarp: new(Visit, new(8, 8), 0, MapLoadMode.Preserve))),
            "field-snapshot-busy");
        Refuse(session, before.WithStory(before.Story.Copy(null, continuation: ProgramContinuation.OutcomeMapLoaded)),
            "field-snapshot-busy");
        var riding = new SessionSnapshot(before.SessionId, before.Revision, before.ObservationSequence,
            new ActiveExploration(before.Exploration!.WithTravelMode(FieldTravelMode.Raft)), before.Story, before.StopReason);
        Refuse(session, riding, "field-visit-mode");
    }

    private static SessionSnapshot AdvanceUntil<TWait>(GameSession session, SessionSnapshot before, SessionCommand command)
        where TWait : ProgramWait
    {
        var current = Submit(session, before, command);
        for (int i = 0; i < 100 && current.Story.Wait is not TWait; i++)
            current = Submit(session, current, new AdvanceSimulation(current.Story.Wait?.Token));
        Assert.IsType<TWait>(current.Story.Wait);
        return current;
    }

    private static void Refuse(GameSession session, SessionSnapshot before, string code)
    {
        Assert.Equal(code, SettledFieldSnapshot.Query(session.Definition, before, Rules).Error);
        var rejected = SettledFieldSnapshot.Capture(session.Definition, before, Rules, before.SessionId, before.Revision);
        Assert.Equal(code, rejected.Error);
        Assert.Null(rejected.Data);
    }

    internal static SettledFieldSnapshot Capture(ScenarioDefinition definition, SessionSnapshot before)
    {
        var offered = SettledFieldSnapshot.Query(definition, before, Rules);
        Assert.True(offered.Allowed, offered.Error);
        Assert.Equal(before.SessionId, offered.SessionId);
        Assert.Equal(before.Revision, offered.Revision);
        var result = SettledFieldSnapshot.Capture(definition, before, Rules, offered.SessionId, offered.Revision);
        Assert.Null(result.Error);
        return Assert.IsType<SettledFieldSnapshot>(result.Data);
    }

    private static SessionSnapshot Submit(GameSession session, SessionSnapshot current, SessionCommand command)
    {
        var result = ExplorationDispatcher.Submit(session.Definition, current, command, Rules);
        Assert.Null(result.Failure);
        return result.Snapshot;
    }

    private static void EqualNext(SessionSnapshot expected, SessionSnapshot actual)
    {
        Assert.Equal(expected.Story.SimulationTick, actual.Story.SimulationTick);
        Assert.Equal(expected.Story.StepCounter, actual.Story.StepCounter);
        Assert.Equal(expected.Story.LogicalView, actual.Story.LogicalView);
        var control = expected.Exploration!;
        var restored = actual.Exploration!;
        Assert.Equal(control.Party.MainSeed, restored.Party.MainSeed);
        Assert.Equal(control.Party.ThinkingSeed, restored.Party.ThinkingSeed);
        foreach (var actor in control.AllEntities)
        {
            var other = restored.AllEntities.Single(row => row.Slot == actor.Slot);
            Assert.Equal(actor.Motion, other.Motion);
            Assert.Equal(actor.ActionCursor, other.ActionCursor);
            Assert.Equal(actor.WaitingForMotion, other.WaitingForMotion);
            Assert.Equal(actor.Follower, other.Follower);
        }
    }

    private static ExplorationEvent Step(int x, int y, string program) =>
        new(ExplorationEventKind.Step, x, y, null, new(program, 0));

    private static GameSession Start(ExplorationEntityDefinition[] entities, StoryProgram[] programs,
        ExplorationEvent[]? events = null)
    {
        var (basis, data) = OpeningRestoreTests.Fixture();
        var party = Assert.IsType<SessionStarted>(GameSession.Restore(basis, data, Rules)).Session.Current.Exploration!.Party;
        var source = basis.Exploration!;
        var template = source.Maps[new("map-3")];
        var palette = template.BasePalette!;
        var words = new ushort[WorkingMapLayout.WordCount];
        words[4 * 64 + 5] = 0x1000;
        var map = new ExplorationMapDefinition(Visit, new(words), template.Traversal, entities, events ?? [],
            onLoad: programs.Any(program => program.Id == "enter") ? new("enter", 0) : null,
            basePalette: palette, viewAreas: template.ViewAreas);
        var refuge = new ExplorationMapDefinition(Refuge, template.Layout, template.Traversal, [], [],
            basePalette: palette, viewAreas: template.ViewAreas)
            { ChurchSavepoint = new(Refuge, new(12, 12), 1) };
        var visuals = new ExplorationVisuals(new[] { Visit, Refuge }.ToDictionary(id => id,
            id => source.Visuals!.Maps[template.Map] with { Map = id }), source.Visuals!.Sprites,
            source.Visuals.Portraits, source.Visuals.Audio);
        var text = new KeyValuePair<int, IReadOnlyList<ExplorationTextToken>>(71,
            [new(ExplorationTextTokenKind.ItemName, "{ITEM}")]);
        var definition = new ScenarioDefinition("settled-field", basis.Encounters.Values, exploration: new(
            [map, refuge], programs, [new(71, "{ITEM}")], source.Provenance, source.PartyFlags, visuals,
            textTokens: [text], textFont: new(Enumerable.Repeat((byte)1, 256).ToArray(),
                Enumerable.Repeat((byte)8, 128).ToArray())), encounterStarts: [party])
        { Persistence = new Sf2OpeningRestore(), FieldPreparation = new([new(5, "Herb", false, false), new(6, "Seed", false, false)]) };
        var start = new ExplorationStartInput(Visit, Player, new(4, 4), 0, 32, [0, 32], party,
            display: new(3, palette, palette, FullFadeVisibility.BaseRestored), textSettings: new(3, 0, 0),
            stepCounter: 27, egressMap: Refuge);
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, start, Rules)).Session;
    }
}

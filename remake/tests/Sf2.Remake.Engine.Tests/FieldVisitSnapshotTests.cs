using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldVisitSnapshotTests
{
    private static readonly PalettePair Palette = new(0xEEE, 0x888);
    private static readonly OriginalMapTraversal Traversal = new([new(0, 0, 63, 63)]);
    private static readonly ExplorationViewArea View = new(0, 0, 63, 63, 0, 0, 0, 0, 256, 256, 256, 256, 0, 0, 0, 0, 0);
    private static readonly ExplorationPopulation Population = new(30, 128, 0, []);
    private static readonly EntityRef Player = new("entity-0");

    [Fact]
    public void WalkingOffTheRoofRestoresTheWordsThatWereThereBeforeActivation()
    {
        var words = new ushort[WorkingMapLayout.WordCount];
        words[4 * 64 + 5] = 0x0800;
        words[4 * 64 + 6] = 0x0C00;
        words[8 * 64 + 8] = 0x007;
        words[0] = 0x123;
        words[1] = 0x456;
        var roof = MapBlockRegionMutation.CopyFrom(new(1, 0, 8, 8, 1, 1));
        var map = new ExplorationMapDefinition(new("roof"), new(words), Traversal, [], [],
            onLoad: new("change-ground", 0), basePalette: Palette,
            layoutEvents: new([], [], new([new(new(5, 4), roof)])));
        var session = Start(map, [map],
            [new("change-ground", [new CopyMapLayout(new(0, 0, 8, 8, 1, 1)), new EndProgram()])]);
        var underRoof = Perform(session, session.Current, new Move(ExplorationDirection.East));
        Assert.Equal(new MapPosition(5, 4), underRoof.Exploration!.PlayerEntity.Position);
        Assert.Equal(0x456, underRoof.Exploration.Layout[8, 8]);

        var visit = FieldVisitSnapshot.Capture(underRoof);
        Assert.Throws<BattleRuleException>(() => (visit with { Roof = visit.Roof! with { RecordOrdinal = 2 } })
            .Rebuild(session.Definition.Exploration!, underRoof.Exploration.Party));
        var rebuilt = Rebuild(session, underRoof);
        var exit = new Move(ExplorationDirection.East);
        foreach (var before in new[] { underRoof, rebuilt })
        {
            var after = Perform(session, before, exit);
            Assert.Equal(new MapPosition(6, 4), after.Exploration!.PlayerEntity.Position);
            Assert.Same(MapBlockCopyLifecycleState.Inactive, after.Exploration.RoofState);
            Assert.Equal(0x123, after.Exploration.Layout[8, 8]);
        }
    }

    [Fact]
    public void CurrentKeepsTheReloadedCastAndBindingsWhileFreshTransferAndScrollChangeTheirOwnState()
    {
        var a = new MapId("a");
        var b = new MapId("b");
        var c = new MapId("c");
        var departure = new RaftBerth(b, new(12, 12));
        var changedBerth = new RaftBerth(b, new(11, 11));
        var aWords = new ushort[4096];
        aWords[4 * 64 + 5] = aWords[8 * 64 + 7] = 0x1000;
        var bWords = new ushort[4096];
        bWords[3 * 64 + 2] = 0x1000;
        ExplorationEntityDefinition Loner(int sprite) => new(new("entity-128"), new(10, 10), 0, 32, Sprite: sprite);
        var first = new ExplorationMapDefinition(a, new(aWords), Traversal, [Loner(191)],
            [Warp(5, 4, a, 6, 8, MapLoadMode.Preserve), Warp(7, 8, b, 2, 4, MapLoadMode.Rebuild)],
            onLoad: new("change-cast", 0), population: Population, basePalette: Palette, viewAreas: [View],
            setup: new(new("usual"), [new(new("15"), new("later"))]),
            setups: [new(new("later"), [], [], null, null, Population)]);
        var second = new ExplorationMapDefinition(b, new(bWords), Traversal, [], [],
            setup: new(new("usual"), [new(new("15"), new("later"))]),
            setups: [new(new("later"), [Loner(193)], [Warp(2, 3, c, 2, 5, MapLoadMode.ScrollVertical)],
                new("change-berth", 0), null, Population)],
            population: Population, entryFlags: [new(64, true)], basePalette: Palette, viewAreas: [View]);
        var third = new ExplorationMapDefinition(c, new(new ushort[4096]), Traversal, [], [], basePalette: Palette, viewAreas: [View]);
        StoryProgram[] programs =
        [
            new("change-cast", [new LoadSceneEntities(Population, new(4, 4), 0,
                [Loner(192) with { Actions = new([new IdleEntityAction()]) }]),
                new ParkRaft(departure), new WriteFlag(15, true), new EndProgram()]),
            new("change-berth", [new ParkRaft(changedBerth), new EndProgram()]),
        ];
        var session = Start(first, [first, second, third], programs);
        var loaded = Settle(session, session.Current);
        var current = Perform(session, loaded, new Move(ExplorationDirection.East));
        Assert.Equal(new MapPosition(6, 8), current.Exploration!.PlayerEntity.Position);
        var rebuilt = Rebuild(session, current);
        Assert.Equal(new FieldMapBody(a, new("usual")), rebuilt.Exploration!.Definition.Body);
        var idle = rebuilt.Exploration.Entities[new("entity-128")];
        Assert.Equal(192, idle.Sprite);
        Assert.True(idle.IsScriptIdle);
        var tick = Perform(session, rebuilt, new WaitAtInput());
        Assert.True(tick.Exploration!.Entities[idle.Entity].IsScriptIdle);

        var enter = new Move(ExplorationDirection.East);
        var fresh = Perform(session, current, enter);
        var freshFromRebuild = Perform(session, rebuilt, enter);
        Assert.Equal(b, fresh.Exploration!.Map);
        Assert.Equal(new MapPosition(2, 4), freshFromRebuild.Exploration!.PlayerEntity.Position);
        Assert.Equal(new FieldMapBody(b, new("later")), freshFromRebuild.Exploration.Definition.Body);
        Assert.Equal(193, freshFromRebuild.Exploration.Entities[new("entity-128")].Sprite);

        var visit = FieldVisitSnapshot.Capture(freshFromRebuild);
        Assert.Equal(changedBerth, visit.Berth);
        var restoredFresh = Rebuild(session, freshFromRebuild);
        var raft = restoredFresh.Exploration!.Entities[new("entity-159")];
        Assert.Equal(31, raft.Slot);
        Assert.Equal(departure.Position, raft.Position);

        var north = new Move(ExplorationDirection.North);
        foreach (var before in new[] { fresh, restoredFresh })
        {
            var scrolled = Perform(session, before, north);
            Assert.Equal(c, scrolled.Exploration!.Map);
            Assert.Equal(new MapPosition(2, 5), scrolled.Exploration.PlayerEntity.Position);
            var restoredScroll = Rebuild(session, scrolled);
            var world = restoredScroll.Exploration!;
            Assert.Equal(new MapPopulationSource(new(b, new("later"))), world.PopulationSource);
            Assert.Equal(193, world.Entities[new("entity-128")].Sprite);
            Assert.Equal(0, world.Aliases[new("entity-159")]);
            Assert.DoesNotContain(world.AllEntities, actor => actor.Slot == 31);
            var east = new Move(ExplorationDirection.East);
            var control = Perform(session, scrolled, east);
            var replay = Perform(session, restoredScroll, east);
            Assert.Equal(new MapPosition(3, 5), replay.Exploration!.PlayerEntity.Position);
            Assert.Equal(control.Exploration!.PlayerEntity.Motion, replay.Exploration.PlayerEntity.Motion);
        }
    }

    private static ExplorationEvent Warp(int x, int y, MapId map, int dx, int dy, MapLoadMode mode) =>
        new(ExplorationEventKind.Warp, x, y, null, null, map, new(dx, dy), LoadMode: mode, RequiredMarker: 0x1000);

    private static GameSession Start(ExplorationMapDefinition map, ExplorationMapDefinition[] maps, StoryProgram[] programs)
    {
        var basis = EngineTestContent.Start("harbor-arrival");
        var definition = new ScenarioDefinition("field-visit", basis.Definition.Encounters.Values, exploration: new(maps, programs));
        var start = new ExplorationStartInput(map.Map, Player, new(4, 4), 0, 32, [], basis.Current.Exploration!.Party,
            display: new(1, Palette, Palette, FullFadeVisibility.BaseRestored));
        var started = Assert.IsType<SessionStarted>(GameSession.Start(definition, start));
        Assert.Null(started.Result.Failure);
        return started.Session;
    }

    // Supply equivalent story controls to exercise this slice's physical reconstruction.
    internal static SessionSnapshot Rebuild(GameSession session, SessionSnapshot before)
    {
        var visit = FieldVisitSnapshot.Capture(before);
        var field = visit.Rebuild(session.Definition.Exploration!, before.Exploration!.Party);
        return new(Guid.NewGuid(), 0, before.ObservationSequence, new ActiveExploration(field),
            before.Story, SessionStopReason.PlayerInput);
    }

    internal static SessionSnapshot Perform(GameSession session, SessionSnapshot before, SessionCommand command) =>
        Settle(session, Submit(session, before, command));

    internal static SessionSnapshot Settle(GameSession session, SessionSnapshot current)
    {
        for (int i = 0; i < 400 && !current.CanWaitAtInput; i++)
        {
            SessionCommand command = current.Story.Wait switch
            {
                EntitySetSpriteWait => Ready(current.Exploration!.AllEntities.First(actor => actor.WaitingForSprite)),
                EntitySpriteWait sprite => new EntitySpriteReady(sprite.Slot, sprite.Request),
                FullFadeWait { LogicalDone: true } fade => new CompletePresentation(fade.Token, fade.Kind),
                PresentationWait wait => new CompletePresentation(wait.Token, wait.Cue.Kind),
                var wait => new AdvanceSimulation(wait?.Token),
            };
            current = Submit(session, current, command);
        }
        Assert.True(current.CanWaitAtInput, current.Story.Wait?.ToString());
        return current;
    }

    private static EntitySpriteReady Ready(ExplorationEntity actor) => new(actor.Slot, actor.SpriteRequest);

    private static SessionSnapshot Submit(GameSession session, SessionSnapshot current, SessionCommand command)
    {
        var result = ExplorationDispatcher.Submit(session.Definition, current, command, session.Rules);
        Assert.Null(result.Failure);
        return result.Snapshot;
    }
}

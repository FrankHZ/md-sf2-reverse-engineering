using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class OpeningRestoreTests
{
    [Theory]
    [InlineData(0, -1, true)]
    [InlineData(0, 519, true)]
    [InlineData(0, 420, false)]
    [InlineData(0, 470, false)]
    [InlineData(0, 520, false)]
    [InlineData(0, 470, true, 520)]
    [InlineData(0x1400, 470, false, 520)]
    [InlineData(0x1400, -1, false)]
    [InlineData(0xC000, -1, false)]
    public void PondSaveKeepsSettledGroundBeforeConfrontationOrAfterRescue(ushort tile, int flag, bool allowed, int second = -1)
    {
        var (basis, data) = Fixture();
        var words = new ushort[4096];
        words[4 * 64 + 3] = tile;
        var map = new ExplorationMapDefinition(new("map-51"), new(words), new([new(0, 0, 12, 12)]), [], []);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values, exploration: new([map], []));
        var player = new FieldReturnAnchor(map.Map, new(3, 4), 2);
        int[] flags = new[] { flag, second }.Where(value => value >= 0).ToArray();
        var policy = basis.Persistence!;
        if (!allowed)
        {
            Assert.Equal("save-location-unavailable",
                Assert.Throws<BattleRuleException>(() => policy.Select(definition, player, flags, null)).Code);
            return;
        }
        var location = policy.Select(definition, player, flags, null);
        Assert.Equal("harpy-pond", location.Recipe);
        Assert.Equal(player, location.Anchor);
        policy.ValidateRegisters(data with { Recipe = location.Recipe, EgressMap = "map-9" });
    }

    [Theory]
    [InlineData(26, 23, 3)]
    [InlineData(25, 23, 2)]
    public void CompletedDevilsTailRestoresThePlayersCurrentFieldPosition(int x, int y, byte facing)
    {
        var (definition, data) = Fixture();
        var field = data with { Map = "map-67", Recipe = "devils-tail", X = x, Y = y, Facing = facing,
            Flags = data.Flags.Append(521).ToArray(), EgressMap = "map-6" };
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(definition, field, RuleCompositions.Sf2()));
        var player = restored.Session.Current.Exploration!.PlayerEntity;
        Assert.Equal(new MapPosition(x, y), player.Position);
        Assert.Equal(facing, player.Motion.Facing);
        Assert.IsType<SessionStartFailed>(GameSession.Restore(definition, field with { Flags = data.Flags }, RuleCompositions.Sf2()));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void CaveSaveRebuildsTheExactFieldFacing(byte facing)
    {
        var (definition, data) = Fixture();
        var slot = data with { Map = "map-32", Recipe = "fairy-cave", X = 28, Y = 4, Facing = facing };
        var result = GameSession.Restore(definition, slot, RuleCompositions.Sf2());
        if (facing == 8)
            Assert.Equal("save-range", Assert.IsType<SessionStartFailed>(result).Failure.Code);
        else
        {
            var world = Assert.IsType<SessionStarted>(result).Session.Current.Exploration!;
            Assert.Equal(new MapPosition(28, 4), world.PlayerEntity.Position);
            Assert.Equal(facing, world.PlayerEntity.Motion.Facing);
        }
    }

    [Theory]
    [InlineData(false, 15, 30, 1)]
    [InlineData(true, 17, 15, 1)]
    [InlineData(true, 12, 15, 2)]
    public void MansionSaveRebuildsFirstVisitWithoutReplayingStory(bool metGoliath, int x, int y, byte facing)
    {
        var (definition, data) = Fixture();
        var flags = metGoliath ? data.Flags.Append(774).ToArray() : data.Flags;
        var slot = data with { Map = "map-33", Recipe = "creed-mansion", X = x, Y = y, Facing = facing,
            Flags = flags, EgressMap = "map-6" };
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(definition, slot, RuleCompositions.Sf2()));
        var world = restored.Session.Current.Exploration!;
        Assert.Equal(new MapPosition(x, y), world.PlayerEntity.Position);
        Assert.Equal(facing, world.PlayerEntity.Motion.Facing);
        Assert.Equal(metGoliath ? new MapPosition(17, 12) : new MapPosition(63, 63), world.Entities[new("entity-128")].Position);
        Assert.Equal(metGoliath ? 3 : 1, world.Entities[new("entity-128")].Motion.Facing);
        Assert.Equal((ushort)0xC400, world.Layout[15, 20]);
        Assert.Equal((ushort)0xC400, world.Layout[17, 11]);
        Assert.DoesNotContain(999, restored.Session.Current.Story.Flags); // on-load must not run
        Assert.DoesNotContain(521, restored.Session.Current.Story.Flags); // no invented prerequisite
        foreach (int later in new[] { 22, 523, 784, 786 })
            Assert.Equal("save-location-unavailable", Assert.Throws<BattleRuleException>(() => definition.Persistence!.Select(
                definition, new(new("map-33"), new(x, y), facing), flags.Append(later).ToArray(), null)).Code);
    }

    [Theory]
    [InlineData(4, 24, 0, false)]
    [InlineData(15, 3, 1, true)]
    public void DesktopSaveRestoresCompletedArrivalWithoutReplayingIt(int x, int y, byte facing, bool permission)
    {
        var (definition, data) = Fixture();
        var flags = data.Flags.Concat(permission ? new[] { 776, 777 } : new[] { 776 }).ToArray();
        var slot = data with { Map = "map-22", Recipe = "desktop-kingdom", X = x, Y = y, Facing = facing,
            Flags = flags, EgressMap = "map-9" };
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(definition, slot, RuleCompositions.Sf2()));
        Assert.Equal(new MapPosition(x, y), restored.Session.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(facing, restored.Session.Current.Exploration.PlayerEntity.Motion.Facing);
        Assert.Contains(776, restored.Session.Current.Story.Flags);
        Assert.Equal(permission, restored.Session.Current.Story.Flags.Contains(777));
        Assert.DoesNotContain(999, restored.Session.Current.Story.Flags);
        Assert.DoesNotContain(774, restored.Session.Current.Story.Flags); // mansion scene is not the arrival selector
        var policy = definition.Persistence!;
        var anchor = new FieldReturnAnchor(new("map-22"), new(x, y), facing);
        foreach (var unsupported in new[] { data.Flags, flags.Append(522).ToArray() })
            Assert.Equal("save-location-unavailable", Assert.Throws<BattleRuleException>(() =>
                policy.Select(definition, anchor, unsupported, null)).Code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DesktopOutcomeSaveRetainsUnlockOrCompletedAftermathWithoutReplayingInit(bool victory)
    {
        var (definition, data) = Fixture();
        var flags = data.Flags.Concat(victory ? new[] { 776, 777, 522, 778 } : new[] { 776, 777, 422 }).ToArray();
        var slot = data with { Map = "map-22", Recipe = "desktop-kingdom", X = 5, Y = 8, Facing = 1,
            Flags = flags, EgressMap = "map-22" };
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(definition, slot, RuleCompositions.Sf2()));
        Assert.Equal(new MapPosition(5, 8), restored.Session.Current.Exploration!.PlayerEntity.Position);
        // This authored fixture clears its local F256 entry flag on population rebuild.
        Assert.Equal(flags.Where(flag => flag != 256).Order(), restored.Session.Current.Story.Flags.Order());
        Assert.DoesNotContain(999, restored.Session.Current.Story.Flags);
        Assert.Equal("desktop-kingdom", definition.Persistence!.Select(definition,
            new(new("map-22"), new(5, 8), 1), flags, null).Recipe);
    }

    private static ushort[] MansionLayout()
    {
        var words = new ushort[WorkingMapLayout.WordCount];
        words[20 * 64 + 15] = words[11 * 64 + 17] = 0xC400;
        return words;
    }

    internal static (ScenarioDefinition Definition, OpeningSaveData Data) Fixture(bool joined = false, bool victory = false)
    {
        var growth = new BattleGrowthDefinition(0, 0, [], [], new Dictionary<byte, SpellRef>());
        var deployments = Enumerable.Range(0, 2).Select(id => new BattleDeploymentDefinition(
            new(new("ally-" + id), BattleClassRule.UnpromotedSwordsman, 1, 20, 5, 8, 4, 6, false, 6, [],
                sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]), growth: growth), BattleFaction.Ally,
            id, BattleControl.Player, null, new(1 + id, 1), new(null, 0, 0, 0, 0, AllyPartyMember: id))).ToArray();
        var battle = new BattleDefinition("opening", new("battle"), 1, 1, [], deployments, []);
        var palette = new PalettePair(0x222, 0x444);
        var maps = new[] { "map-3", "map-57", "map-32", "map-67", "map-33", "map-22", "map-26" }.Select(id => new ExplorationMapDefinition(new(id),
            new WorkingMapLayout(id == "map-33" ? MansionLayout() : new ushort[WorkingMapLayout.WordCount]), new([new(0, 0, 63, 63)]),
            id == "map-3" ? [new(new("entity-1"), new(42, 8), 3, 32, Sprite: 1,
                Actions: new([new WaitEntityTicks(5), new JumpEntityAction(0)])), new(new("entity-142"), new(54, 17), 1, 32, Sprite: 2)] : id == "map-33" ? [new(new("entity-128"), new(63, 63), 1, 32, Sprite: 202)] : id == "map-26" ? [new(new("entity-128"), new(10, 10), 1, 32, Sprite: 135), new(new("entity-129"), new(5, 24), 3, 32, Sprite: 131)] : [], [],
            // This must never execute during restore.
            onLoad: new("dangerous-onload", 0), population: new(30, 128, 0, [new(66, 1, 1)]),
            entryFlags: [new(256, false)], basePalette: palette,
            viewAreas: [new(0, 0, 63, 63, 0, 0, 0, 0, 256, 256, 256, 256, 0, 0, 0, 0, 0)])).ToArray();
        var audio = new ExplorationAudio(1, 0, 44100, 1, [], "", 0, 1);
        var visuals = new ExplorationVisuals(maps.ToDictionary(map => map.Map, map => new ExplorationMapVisual(map.Map,
            new(1, 1, "rgba8", []), 1, [], [new(1, 1)])), new Dictionary<int, ExplorationSpriteVisual>(),
            new Dictionary<int, ExplorationPortraitVisual>(), new Dictionary<string, ExplorationAudio> { ["field"] = audio,
                ["previous"] = new(2, 0, 44100, 1, [], "", 0, 1) });
        var world = new ExplorationDefinition(maps, [new("dangerous-onload", [new WriteFlag(999, true), new EndProgram()])],
            provenance: new("project-test", "accepted", "test-rom", [], "engine behavior"), partyFlags: new(30, 0, 32, 12), visuals: visuals);
        var definition = new ScenarioDefinition("opening-test", [battle], exploration: world) { Persistence = new Sf2OpeningRestore() };
        var flags = new List<int> { 0, 32, 80, 256 };
        if (joined || victory) flags.AddRange([1, 33, 66, 600, 602, 603]);
        if (victory) flags.AddRange([401, 501]);
        var allies = deployments.Select(row => new SavedAlly(row.Actor.Value, 0, 7, 2, 38, 4, 2, 2, 0, true, false,
            3, 25, 8, 14, 9, 10, [127, 127, 127, 127], [63, 63, 63, 63])).ToArray();
        var data = new OpeningSaveData(OpeningSave.Compatibility(definition, RuleCompositions.Sf2()),
            victory ? "battle01-return" : joined ? "map3-joined" : "map3-opening", victory ? "map-57" : "map-3",
            victory ? 4 : joined ? 32 : 56, victory ? 5 : joined ? 13 : 3, joined && !victory ? (byte)1 : (byte)3,
            "opening", allies, joined || victory ? ["ally-0", "ally-1"] : ["ally-0"], 0x1234abcdu, uint.MaxValue,
            4321, [2], [new(1, 3)], flags, 27, "map-3", 535, new(2, 0, 0), 3, new("field", []), 0, false, false);
        return (definition, data);
    }

    [Theory]
    [InlineData(true, "map-72", 48, true)]
    [InlineData(false, "map-72", 48, false)]
    [InlineData(true, "map-72", 49, false)]
    [InlineData(true, "map-3", 48, false)]
    [InlineData(true, null, 0, false)]
    public void PolcaLandingRequiresBoardableWaterAndThePhysicalBerth(bool water, string? parkedMap, int y, bool boardable)
    {
        var (basis, data) = Fixture();
        var rules = RuleCompositions.Sf2();
        var party = Assert.IsType<SessionStarted>(GameSession.Restore(basis, data, rules)).Session.Current.Exploration!.Party;
        var template = basis.Exploration!.Maps[new("map-57")];
        var words = new ushort[WorkingMapLayout.WordCount];
        if (water) words[48 * WorkingMapLayout.ColumnCount + 43] = 0xFC01;
        var map = new ExplorationMapDefinition(new("map-72"), new(words), template.Traversal, [], [],
            population: template.Population, basePalette: template.BasePalette, viewAreas: template.ViewAreas);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values,
            exploration: new(basis.Exploration.Maps.Values.Append(map), []));
        var policy = basis.Persistence!;
        var anchor = new FieldReturnAnchor(map.Map, new(43, 47), 1);
        int[] flags = [0, 32, 64, 10];
        var location = policy.Select(definition, anchor, flags, null);
        RaftBerth? berth = parkedMap is null ? null : new(new(parkedMap), new(43, y));
        ExplorationState Restore() => policy.Rebuild(definition, location, party, flags, berth, out _);

        if (boardable)
        {
            var world = Restore();
            Assert.Equal(anchor.Facing, world.PlayerEntity.Motion.Facing);
            Assert.Equal(berth!.Position, Assert.Single(world.AllEntities, e => e.Slot == 31).Position);
        }
        else Assert.Equal("save-raft-berth", Assert.Throws<BattleRuleException>(Restore).Code);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, false, true)]
    public void HassanSavepointRebuildsTheSelectedCastWithoutJoiningOrHealing(bool tarosComplete, bool caravan, bool rickJoined)
    {
        var (basis, data) = Fixture();
        var rules = RuleCompositions.Sf2();
        var party = Assert.IsType<SessionStarted>(GameSession.Restore(basis, data, rules)).Session.Current.Exploration!.Party;
        var source = basis.Exploration!.Maps[new("map-57")];
        var entities = new List<ExplorationEntityDefinition> { new(new("entity-11"), new(16, 59), 3, 32, Sprite: 11) };
        if (tarosComplete) entities.Add(new(new("entity-12"), new(12, 3), 0, 32, Sprite: 12));
        entities.AddRange(Enumerable.Range(128, 10).Select(id =>
            new ExplorationEntityDefinition(new($"entity-{id}"), new(16, 3), 3, 32, Sprite: 80)));
        var town = new ExplorationMapDefinition(new("map-9"), source.Layout, source.Traversal, entities, [],
            population: source.Population, basePalette: source.BasePalette, viewAreas: source.ViewAreas);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values,
            exploration: new(basis.Exploration.Maps.Values.Append(town), []));
        var flags = new List<int> { 0, 32, 64 };
        if (tarosComplete) flags.Add(518);
        if (caravan) flags.Add(65);
        if (rickJoined) flags.Add(12);
        var policy = basis.Persistence!;
        var location = policy.Select(definition, new(town.Map, new(14, 2), 3), flags, null);
        Assert.Equal(new FieldReturnAnchor(town.Map, new(8, 9), 1), location.Anchor);
        ExplorationState Resume() => policy.Rebuild(definition, location, party, flags, new(new("map-69"), new(28, 55)), out _);
        if (rickJoined && !tarosComplete)
        {
            Assert.Equal("save-anchor-unusable", Assert.Throws<BattleRuleException>(Resume).Code);
            return;
        }
        var world = Resume();
        Assert.Equal(location.Anchor.Position, world.PlayerEntity.Position);
        Assert.Equal(location.Anchor.Facing, world.PlayerEntity.Motion.Facing);
        Assert.Equal(!tarosComplete, world.Entities[new("entity-137")].Motion.X == 0x7000);
        Assert.Equal(caravan, world.Entities[new("entity-11")].Motion.X == 0x7000);
        if (tarosComplete) Assert.Equal(rickJoined, world.Entities[new("entity-12")].Motion.X == 0x7000);
        Assert.DoesNotContain(world.AllEntities, e => e.Slot == 31);
        Assert.Same(party, world.Party);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, false)] [InlineData(true, true)]
    public void RestoreRebuildsEachRecipeWithoutOnloadGrowthHealingOrReward(bool joined, bool victory)
    {
        var (definition, data) = Fixture(joined, victory);
        data = data with { Allies = data.Allies.Select(a => a.Actor == "ally-1" ? a with { Hp = 0 } : a).ToArray() };
        var first = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var second = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        Assert.NotEqual(first.Current.SessionId, second.Current.SessionId);
        Assert.True(first.Current.CanWaitAtInput);
        Assert.Equal(new MapPosition(data.X, data.Y), first.Current.Exploration!.PlayerEntity.Position);
        Assert.DoesNotContain(999, first.Current.Story.Flags); Assert.DoesNotContain(256, first.Current.Story.Flags);
        Assert.Equal(data.MainSeed, first.Current.Exploration.Party.MainSeed);
        Assert.Equal(data.ThinkingSeed, first.Current.Exploration.Party.ThinkingSeed);
        Assert.Equal(data.Gold, first.Current.Exploration.Party.Gold);
        Assert.Equal(7, first.Current.Exploration.Party.Actors[0].Hp);
        Assert.Equal(0, first.Current.Exploration.Party.Actors[1].Hp);
        Assert.Equal((byte)38, first.Current.Exploration.Party.Actors[0].Exp);
        Assert.Equal(3, first.Current.Exploration.Party.Actors[0].Progress!.Level);
        Assert.Equal(535, first.Current.Story.TextCursor);
        Assert.Equal(0, first.Current.Story.Music!.Step);
        var capture = first.CaptureSave(first.Current.SessionId, first.Current.Revision);
        Assert.Null(capture.Error); Assert.Equal(data.Allies[0], capture.Data!.Allies[0] with { Items = data.Allies[0].Items, Spells = data.Allies[0].Spells });
        Assert.NotSame(first.Current.Exploration.Party, second.Current.Exploration!.Party);
        foreach (var session in new[] { first, second })
            Assert.Null(EngineTestContent.Send(session, new WaitAtInput()).Failure);
        Assert.Equal(first.Current.Exploration.Party.MainSeed, second.Current.Exploration.Party.MainSeed);
        Assert.Equal(first.Current.Exploration.AllEntities.Select(e => e.Motion), second.Current.Exploration.AllEntities.Select(e => e.Motion));
    }

    [Theory]
    [InlineData(4, 5)] [InlineData(12, 8)]
    public void VictoryRestoreUsesTheCompletedOutcomeLanding(int x, int y)
    {
        var (definition, data) = Fixture(true, true);
        data = data with { X = x, Y = y, Gold = 942, MainSeed = 0, ThinkingSeed = uint.MaxValue };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        Assert.Equal(new MapPosition(x, y), session.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(new FieldReturnAnchor(new("map-57"), new(x, y), 3), session.Current.Story.SaveAnchor);
        var captured = session.CaptureSave(session.Current.SessionId, session.Current.Revision).Data!;
        Assert.Equal(data.Depot, captured.Depot);
        Assert.Equal(data, captured with
        {
            Allies = data.Allies, ActiveAllies = data.ActiveAllies, OnceFlags = data.OnceFlags, Deals = data.Deals,
            Flags = data.Flags, Music = data.Music, Names = data.Names, Depot = data.Depot,
        });
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void InvalidProgressCannotPublishAReplacement(int variation)
    {
        var (definition, data) = Fixture();
        var original = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var before = original.Current;
        var invalid = variation switch
        {
            0 => data with { Compatibility = data.Compatibility with { GameplayRevision = 2 } },
            1 => data with { ActiveAllies = ["ally-0", "ally-1"] },
            2 => data with { Allies = [data.Allies[0], data.Allies[0]] },
            3 => data with { Allies = data.Allies.Select(a => a with { Hp = 201 }).ToArray() },
            4 => data with { Recipe = "battle01-return" },
            _ => data with { Music = new("missing", []) },
        };
        Assert.IsType<SessionStartFailed>(GameSession.Restore(definition, invalid, RuleCompositions.Sf2()));
        Assert.Same(before, original.Current);
    }

    [Theory]
    [InlineData(601)] [InlineData(602)] [InlineData(604)]
    public void IntermediateConversationFlagsSurviveAndF602RepositionsSarah(int flag)
    {
        var (definition, data) = Fixture();
        data = data with { Flags = data.Flags.Append(flag).ToArray() };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        Assert.Contains(flag, session.Current.Story.Flags);
        Assert.Equal(flag == 602 ? new MapPosition(41, 10) : new(42, 8), session.Current.Exploration!.Entities[new("entity-1")].Position);
        Assert.Equal((byte)0xE0, session.Current.Exploration.Entities[new("entity-1")].Motion.FlagsA);
    }
}

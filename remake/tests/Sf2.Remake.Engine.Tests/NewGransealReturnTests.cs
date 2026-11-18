using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.TestSupport;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class NewGransealReturnTests
{
    [PrivateInputFact("SF2_NEW_GRANSEAL_SAVE")]
    public void SourceInventorCallerKeepsFlagPriorityAndActualCannonLookup()
    {
        string Input(string key) => PrivateInputFactAttribute.RequireInput(key);
        var read = new OpeningRestoreReader(Input("SF2_PRIVATE_EXPLORATION_CONTENT"), Input("SF2_PRIVATE_BATTLE01_DATA"),
            Input("SF2_PRIVATE_BATTLE01_SCENE"), Input("SF2_PRIVATE_BATTLE01_TERRAIN"), Input("SF2_PRIVATE_STATIC_DATA"),
            Input("SF2_PRIVATE_ENEMY_DATA"), Input("SF2_PRIVATE_ENEMY_GOLD"), Input("SF2_PRIVATE_BATTLE_SCENE_CONTENT")).Read();
        var definition = Assert.IsType<ExplorationReadAccepted>(read).Definition;
        var save = OpeningSaveJson.ReadRecord(File.ReadAllBytes(Input("SF2_NEW_GRANSEAL_SAVE")), 1);
        var original = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, save, RuleCompositions.Sf2())).Session.Current;
        // Controlled valid flag/pack variants of an ordinary save, not natural-route evidence.
        (int[] Story, bool Cannon, int Text)[] cases = [
            ([804, 805], true, 1055), ([702, 804, 805], true, 1063),
            ([802, 804], false, 1122), ([802], true, 1114),
            ([802, 805], false, 1116), ([802, 805], true, 1119)];
        foreach (var sample in cases)
        {
            var current = original;
            if (!sample.Cannon)
                foreach (var member in current.Exploration!.Party.Actors)
                    current = Pack(current, int.Parse(member.Actor.Value.AsSpan(5)),
                        member.SourceLoadout!.Items.Where(item => (item & 127) != 114).Concat(Enumerable.Repeat((ushort)127, 4)).Take(4).ToArray());
            var flags = original.Story.Flags.Except([70, 702, 761, 802, 804, 805]).Concat(sample.Story).ToArray();
            current = current.WithStory(current.Story.Copy(new("map6-entityevent13", 0), flags: flags));
            var result = ProgramRunner.Run(definition, current, []);
            Assert.Null(result.Failure);
            Assert.Equal(sample.Text, Assert.IsType<FieldTextWait>(result.Snapshot.Story.Wait).Text);
        }
    }

    [Theory]
    [InlineData(true, false, false, true)]
    [InlineData(true, false, true, false)]
    [InlineData(false, false, false, false)]
    [InlineData(true, true, false, true)]
    public void JoinedLookupUsesCountLimitedSlotsAndIgnoresEquipmentBits(bool joined, bool broken, bool gap, bool found)
    {
        var (definition, current) = Inventory();
        ushort cannon = (ushort)(114 | 128 | (broken ? 32768 : 0));
        current = Pack(current, 1, gap ? [1, 127, 127, cannon] : [1, cannon, 127, 127]);
        current = current.WithStory(current.Story.Copy(null, flags: joined ? [0, 1, 32] : [0, 32]));
        var result = FieldItemEvents.Find(definition, current, 114);
        Assert.Equal(found, result is not null);
        if (found) Assert.Equal(new ActorRef("ally-1"), result!.Value.Member.Carried.Actor);
    }

    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(true, false, 1)]
    [InlineData(true, true, -1)]
    public void GiftUsesFirstJoinedCapacityIncludingDeadReserveAndFailureCanRetry(bool firstFull, bool secondFull, int recipient)
    {
        var (definition, current) = Inventory();
        current = Pack(current, 0, firstFull ? [114, 1, 2, 3] : [114, 1, 127, 127]);
        current = Pack(current, 1, secondFull ? [1, 2, 3, 4] : [1, 127, 127, 2], dead: true);
        current = current.WithStory(current.Story.Copy(new("gift", 0), flags: [0, 1, 32]));
        var result = ProgramRunner.Run(definition, current, []);
        Assert.Null(result.Failure);
        Assert.Equal(recipient >= 0, result.Snapshot.Story.Flags.Contains(804));
        Assert.Equal(recipient >= 0 ? recipient : null, result.Snapshot.Story.ItemReceipt?.Member);
        Assert.Equal(1, result.Snapshot.Exploration!.Party.Actors.Sum(a => a.SourceLoadout!.Items.Count(i => (i & 127) == 114)));
        Assert.Equal(recipient >= 0 ? 1 : 0, result.Snapshot.Exploration.Party.Actors.Sum(a => a.SourceLoadout!.Items.Count(i => i == 116)));
        Assert.Equal((ushort)0, result.Snapshot.Exploration.Party.Actors.Single(a => a.Actor.Value == "ally-1").Hp);
        Assert.DoesNotContain(33, result.Snapshot.Story.Flags);
        if (recipient < 0)
        {
            // A different legal carried loadout can retry the unchanged caller; no completion flag was set.
            var freed = Pack(result.Snapshot, 1, [1, 2, 3, 127]);
            var retried = ProgramRunner.Run(definition, freed.WithStory(freed.Story.Copy(new("gift", 0))), []);
            Assert.Null(retried.Failure);
            Assert.Contains(804, retried.Snapshot.Story.Flags);
            Assert.Equal(1, retried.Snapshot.Story.ItemReceipt!.Member);
        }
    }

    [Fact]
    public void FullActiveForceRecruitsJanetIntoReserveWithoutHpItemOrElricGuard()
    {
        var flags = Enumerable.Range(0, 12).Concat(Enumerable.Range(32, 12)).ToArray();
        var result = MapPartyMembership.Join(flags, new(30, 0, 32, 12), 18);
        Assert.Contains(18, result.Flags);
        Assert.DoesNotContain(50, result.Flags);
        Assert.Equal(Enumerable.Range(32, 12), result.Flags.Where(f => f >= 32));
    }

    [Theory]
    [InlineData(6)] [InlineData(7)]
    public void LocalSceneRestoresItsActorsAndFollowingWhileFreshEntryRebuilds(int mapId)
    {
        var (basis, current) = Inventory();
        var population = new ExplorationPopulation(30, 128, 0, [new(67, 7, 18), new(70, 31, 209)],
            AllySprites: [new(18, 45, null, null)]);
        var resident = new ExplorationEntityDefinition(new("entity-128"), new(10, 5), 3, 32, Sprite: 209);
        var defaultId = new MapSetupId($"ms-map{mapId}");
        var freshId = new MapSetupId($"ms-map{mapId}-flag805");
        var map = new ExplorationMapDefinition(new($"map-{mapId}"), new(new ushort[4096]), new([new(0, 0, 31, 31)]),
            mapId == 7 ? [resident] : [], [], population: population,
            setup: new(defaultId, mapId == 7 ? [new(new("805"), freshId)] : []),
            setups: mapId == 7 ? [new(freshId, [], [], null, null, population)] : []);
        var allocation = new AddMapEntity(new("entity-18"), new(16, 22), 3, 18, new([new IdleEntityAction()]));
        var visuals = basis.Exploration!.Visuals! with { Sprites = Enumerable.Range(0, 256).ToDictionary(i => i, i => new ExplorationSpriteVisual(i, [], null, 0)) };
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values,
            exploration: new([map], [new("cs-55288", [allocation, new EndProgram()])], visuals: visuals));
        int[] before = mapId == 7 ? [67, 802] : [67, 70, 802, 805];
        var world = MapTransfer.Build(map, new("entity-0"), new(11, 7), 1, 32, current.Exploration!.Party, before);
        if (mapId == 6)
        {
            world = SceneEntities.Add(world, allocation, 1, before);
            var actor = world.Entities[new("entity-18")];
            Assert.Equal(45, actor.Sprite);
            world = world.WithEntity(actor with { Actions = null, WaitingForSprite = false, SpriteReady = 1 });
            world = world.WithFollower(FollowerMotion.Install(world.Entities[new("entity-18")], world.Aliases[new("entity-31")], -24, 0));
        }
        else world = world.WithFollower(FollowerMotion.Install(world.Entities[new("entity-128")], world.Aliases[new("entity-7")], -24, 0));
        int[] after = [67, 70, 802, 805, 806];
        var scene = Sf2NewGransealRestore.Capture(definition, world);
        var location = new SaveLocation("new-granseal-return", new(map.Map, new(11, 7), 1), "town");
        var restored = Sf2NewGransealRestore.Rebuild(definition, location, world.Party, after, scene, out _, 0);
        Assert.Equal(scene, Sf2NewGransealRestore.Capture(definition, restored), SceneComparer.Instance);
        var fresh = MapTransfer.Build(map, new("entity-0"), new(11, 7), 1, 32, world.Party, after);
        Assert.DoesNotContain(fresh.AllEntities, e => e.Entity.Value == (mapId == 6 ? "entity-18" : "entity-128"));
        Assert.Contains(fresh.AllEntities, e => e.Entity.Value == "entity-31");
        if (mapId == 6)
            Assert.Throws<BattleRuleException>(() => Sf2NewGransealRestore.Rebuild(definition, location, world.Party,
                after.Where(f => f != 806).ToArray(), scene, out _, 0));
    }

    private sealed class SceneComparer : IEqualityComparer<SavedFieldScene>
    {
        internal static readonly SceneComparer Instance = new();
        public bool Equals(SavedFieldScene? a, SavedFieldScene? b) => a!.Setup == b!.Setup && a.Actors.SequenceEqual(b.Actors) &&
            a.Aliases.OrderBy(p => p.Key).SequenceEqual(b.Aliases.OrderBy(p => p.Key)) && a.CurrentEntities.SequenceEqual(b.CurrentEntities) && a.Layout.SequenceEqual(b.Layout);
        public int GetHashCode(SavedFieldScene obj) => 0;
    }

    private static SessionSnapshot Pack(SessionSnapshot current, int id, ushort[] items, bool dead = false)
    {
        var field = current.Exploration!;
        var actor = field.Party.Actors.Single(a => a.Actor.Value == "ally-" + id);
        var loadout = new BattleSourceLoadout(items, actor.SourceLoadout!.Spells);
        actor = actor with { Hp = dead ? (ushort)0 : actor.Hp, SourceLoadout = loadout,
            Progress = actor.Progress is { } p ? p with { SourceLoadout = loadout } : null };
        return new(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(field.WithParty(FieldInventory.Replace(field.Party, [actor]))), current.Story, current.StopReason);
    }

    private static (ScenarioDefinition, SessionSnapshot) Inventory()
    {
        var (basis, data) = OpeningRestoreTests.Fixture(joined: true);
        var current = Assert.IsType<SessionStarted>(GameSession.Restore(basis, data, RuleCompositions.Sf2())).Session.Current;
        var programs = new[] { new StoryProgram("gift", [new TryAddPartyItem(116, new("full", 0)), new WriteFlag(804, true), new WaitProgramTicks(1), new EndProgram()]),
            new StoryProgram("full", [new WaitProgramTicks(1), new EndProgram()]) };
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values,
            exploration: new(basis.Exploration!.Maps.Values, programs, partyFlags: basis.Exploration.PartyFlags, visuals: basis.Exploration.Visuals));
        return (definition, current);
    }
}

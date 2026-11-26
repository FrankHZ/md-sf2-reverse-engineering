using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class HistoricalSaveRosterTests
{
    [Fact]
    public void ExpandedContentRestoresSavedBattleSlotsAndKeepsNewMembersRetained()
    {
        var (basis, snapshot) = BattleSaveFixture.Create(encounterId: "battle-1");
        var latest = Package(4, 11, basis);
        var seed = BattleSaveFixture.Capture(basis, snapshot);
        var old = basis.Encounters["battle-1"];
        var init = old.Initialization!;
        var extra = latest.Encounters["opening"].Deployments.Where(d => d.Actor.Value != "ally-0").ToArray();
        var slots = init.SourceCombatants.Concat(extra.Select(d => KeyValuePair.Create(d.ProcessingOrder, d.Actor)))
            .ToDictionary(p => p.Key, p => p.Value);
        var expanded = new BattleDefinition(old.Encounter, old.Map, old.Width, old.Height, old.Terrain,
            old.Deployments.Concat(extra), old.Spells.Values, old.Rewards,
            new(init.Regions, init.RegionProgram, init.AllyFormation, init.AiPoints, slots, init.EnemyCount, init.RegionPrograms), old.Outcome);
        latest = new ScenarioDefinition(latest.Package, latest.Encounters.Values.Select(e => e.Encounter == old.Encounter ? expanded : e),
            latest.PrivateDefinitions, latest.Exploration) { Persistence = latest.Persistence };
        var retained = Enumerable.Range(1, 6).Select(id => seed.Story.Player.RetainedAllies[0] with { Actor = new($"ally-{id}") }).ToArray();
        var data = seed with
        {
            Compatibility = OpeningSave.FieldCompatibility(latest, RuleCompositions.Sf2()) with { RecipeRevision = 1 },
            Story = seed.Story with { Player = seed.Story.Player with { RetainedAllies = retained } },
        };
        var restored = GameSession.RestoreBattle(latest, data, RuleCompositions.Sf2());
        Assert.True(restored is SessionStarted, (restored as SessionStartFailed)?.Failure.Code);
        var session = ((SessionStarted)restored).Session;
        Assert.Equal(init.SourceCombatants, session.Current.Battle.Definition.Initialization!.SourceCombatants);
        var captured = session.CaptureBattleSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(captured.Error);
        Assert.True(BattleContinuationValues.Same(data.Battle, captured.Data!.Battle));
        Assert.Equal(10, captured.Data.Story.Player.RetainedAllies.Count);
        foreach (var member in retained)
            Assert.True(BattleContinuationValues.SameCarried(member, captured.Data.Story.Player.RetainedAllies.Single(a => a.Actor == member.Actor)));
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1507850002u)]
    public void DirectAndStagedFieldUpgradesCarryTheSamePartyAndSeeds(uint main)
    {
        var (_, original) = OpeningRestoreTests.Fixture(joined: true);
        var first = Package(1, 7);
        var saved = original with
        {
            Compatibility = OpeningSave.FieldCompatibility(first, RuleCompositions.Sf2()), MainSeed = main,
            Allies = Enumerable.Range(0, 7).Select(id => original.Allies[Math.Min(id, 1)] with { Actor = $"ally-{id}" }).ToArray(),
        };
        var latest = Package(4, 11);
        var direct = Upgrade(latest, saved);
        var staged = Upgrade(latest, Upgrade(Package(3, 10), Upgrade(Package(2, 9), saved)));
        Assert.Equal(11, direct.Allies.Count);
        Assert.Equal(main, direct.MainSeed);
        Assert.Equal(saved.ThinkingSeed, direct.ThinkingSeed);
        Assert.Equal(main, staged.MainSeed);
        Assert.Equal(saved.ThinkingSeed, staged.ThinkingSeed);
        for (int id = 0; id < direct.Allies.Count; id++)
        {
            EqualAlly(direct.Allies[id], staged.Allies[id]);
            if (id < saved.Allies.Count) EqualAlly(saved.Allies[id], direct.Allies[id]);
            else Assert.Equal(5, direct.Allies[id].Level);
        }
    }

    [Theory]
    [InlineData(7, 1)]
    [InlineData(9, 1)]
    [InlineData(10, 1)]
    [InlineData(11, 1)]
    [InlineData(9, 2)]
    [InlineData(10, 3)]
    public void BattleUpgradeExtendsOnlyTheRetainedList(int count, int stamp)
    {
        var (basis, snapshot) = BattleSaveFixture.Create(encounterId: "battle-1");
        var latest = Package(4, 11, basis);
        var seed = BattleSaveFixture.Capture(basis, snapshot);
        var player = seed.Story.Player;
        var retained = Enumerable.Range(1, count - 1)
            .Select(id => player.RetainedAllies[0] with { Actor = new($"ally-{id}") }).ToArray();
        var compatibility = OpeningSave.FieldCompatibility(latest, RuleCompositions.Sf2());
        var data = seed with
        {
            Compatibility = compatibility with { RecipeRevision = stamp },
            Story = seed.Story with { Player = player with { RetainedAllies = retained } },
        };
        var result = GameSession.RestoreBattle(latest, data, RuleCompositions.Sf2());
        Assert.True(result is SessionStarted, (result as SessionStartFailed)?.Failure.Code);
        var session = ((SessionStarted)result).Session;
        var captured = session.CaptureBattleSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(captured.Error);
        var upgraded = Assert.IsType<BattleSaveData>(captured.Data);
        Assert.Equal(compatibility, upgraded.Compatibility);
        Assert.True(BattleContinuationValues.Same(data.Battle, upgraded.Battle));
        var story = upgraded.Story;
        Assert.Equal(10, story.Player.RetainedAllies.Count);
        for (int id = 1; id <= 10; id++)
        {
            var member = story.Player.RetainedAllies[id - 1];
            Assert.Equal($"ally-{id}", member.Actor.Value);
            if (id < count) Assert.True(BattleContinuationValues.SameCarried(retained[id - 1], member));
            else Assert.Equal(5, member.Progress!.Level);
        }
    }

    [Fact]
    public void LegacyTwelveMemberBattle23ContinuesIntoTheCreedRosterWithoutChangingItsBattle()
    {
        var (basis, snapshot) = BattleSaveFixture.Create(encounterId: "battle-23");
        var latest = Package(6, 16, basis);
        var seed = BattleSaveFixture.Capture(basis, snapshot);
        int[] members = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13];
        var data = seed with
        {
            Compatibility = OpeningSave.FieldCompatibility(latest, RuleCompositions.Sf2()) with { RecipeRevision = 1 },
            Story = seed.Story with { Player = seed.Story.Player with
                { RetainedAllies = members.Select(id => seed.Story.Player.RetainedAllies[0] with { Actor = new("ally-" + id) }).ToArray() } },
        };
        var result = GameSession.RestoreBattle(latest, data, RuleCompositions.Sf2());
        Assert.True(result is SessionStarted, (result as SessionStartFailed)?.Failure.Code);
        var session = ((SessionStarted)result).Session;
        var captured = session.CaptureBattleSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(captured.Error);
        var upgraded = Assert.IsType<BattleSaveData>(captured.Data);
        Assert.True(BattleContinuationValues.Same(data.Battle, upgraded.Battle));
        Assert.Equal(15, upgraded.Story.Player.RetainedAllies.Count);
        foreach (var member in data.Story.Player.RetainedAllies)
            Assert.True(BattleContinuationValues.SameCarried(member, upgraded.Story.Player.RetainedAllies.Single(a => a.Actor == member.Actor)));
        Assert.Equal(data.Battle.MainSeed, upgraded.Battle.MainSeed);
    }

    [Fact]
    public void NorthCaveBattleAddsHiginsToRetainedPartyWithoutJoiningOrDeployingHim()
    {
        var (basis, snapshot) = BattleSaveFixture.Create(encounterId: "battle-25");
        var latest = Package(9, 19, basis);
        var seed = BattleSaveFixture.Capture(basis, snapshot);
        int[] members = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 14, 15, 16, 17, 18];
        var data = seed with
        {
            Compatibility = OpeningSave.FieldCompatibility(latest, RuleCompositions.Sf2()) with { RecipeRevision = 8 },
            Story = seed.Story with { Player = seed.Story.Player with
                { RetainedAllies = members.Select(id => seed.Story.Player.RetainedAllies[0] with { Actor = new("ally-" + id) }).ToArray() } },
        };
        var session = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(latest, data, RuleCompositions.Sf2())).Session;
        var upgraded = Assert.IsType<BattleSaveData>(session.CaptureBattleSave(session.Current.SessionId, session.Current.Revision).Data);
        Assert.True(BattleContinuationValues.Same(data.Battle, upgraded.Battle));
        Assert.Equal(data.Story.Flags, upgraded.Story.Flags);
        Assert.Single(upgraded.Story.Player.RetainedAllies, row => row.Actor == new ActorRef("ally-19"));
    }

    [Fact]
    public void FieldUpgradeInitializesRickWithoutChangingSavedPartyOrRandomState()
    {
        var (_, original) = OpeningRestoreTests.Fixture(joined: true);
        var prior = Package(9, 19);
        var latest = Package(10, 20);
        var rules = RuleCompositions.Sf2();
        var previousRows = prior.Encounters["opening"].Deployments;
        var saved = original with
        {
            Compatibility = OpeningSave.FieldCompatibility(prior, rules),
            Flags = original.Flags.Where(flag => flag != 256).Order().ToArray(),
            Allies = previousRows.Select(row => original.Allies[row.Actor.Value == "ally-0" ? 0 : 1] with
                { Actor = row.Actor.Value }).ToArray(),
        };

        var upgraded = Upgrade(latest, saved);
        var rick = Assert.Single(upgraded.Allies, row => row.Actor == "ally-12");
        Assert.Equal(5, rick.Level);
        Assert.Equal(rick.MaxHp, rick.Hp);
        Assert.Equal(0, rick.Exp);
        Assert.Equal(OpeningSave.FieldCompatibility(latest, rules), upgraded.Compatibility);
        Assert.Equal(saved.Flags, upgraded.Flags);
        Assert.Equal(saved.ActiveAllies, upgraded.ActiveAllies);
        Assert.Equal(saved.MainSeed, upgraded.MainSeed);
        Assert.Equal(saved.ThinkingSeed, upgraded.ThinkingSeed);
        Assert.Equal(saved.Gold, upgraded.Gold);
        Assert.Equal(saved.Allies.Count + 1, upgraded.Allies.Count);
        foreach (var member in saved.Allies)
            EqualAlly(member, upgraded.Allies.Single(row => row.Actor == member.Actor));

        var resaved = Upgrade(latest, upgraded);
        EqualAlly(rick, Assert.Single(resaved.Allies, row => row.Actor == "ally-12"));
    }

    [Fact]
    public void Battle26UpgradeAppendsRickToRetainedPartyAndResumesTheSameFight()
    {
        var (basis, snapshot) = BattleSaveFixture.Create(encounterId: "battle-26");
        var prior = Package(9, 19, basis);
        var latest = Package(10, 20, basis);
        var rules = RuleCompositions.Sf2();
        var seed = BattleSaveFixture.Capture(basis, snapshot);
        var player = seed.Story.Player;
        var retained = prior.Encounters["opening"].Deployments.Where(row => row.Actor.Value != "ally-0")
            .Select(row => player.RetainedAllies[0] with { Actor = row.Actor }).ToArray();
        var data = seed with
        {
            Compatibility = OpeningSave.FieldCompatibility(prior, rules),
            Story = seed.Story with { Player = player with { RetainedAllies = retained } },
        };

        var session = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(latest, data, rules)).Session;
        var upgraded = Assert.IsType<BattleSaveData>(session.CaptureBattleSave(session.Current.SessionId, session.Current.Revision).Data);
        Assert.Equal(OpeningSave.FieldCompatibility(latest, rules), upgraded.Compatibility);
        Assert.True(BattleContinuationValues.Same(data.Battle, upgraded.Battle));
        Assert.Equal(data.Story.Flags, upgraded.Story.Flags);
        Assert.Equal(retained.Length + 1, upgraded.Story.Player.RetainedAllies.Count);
        foreach (var member in retained)
            Assert.True(BattleContinuationValues.SameCarried(member,
                upgraded.Story.Player.RetainedAllies.Single(row => row.Actor == member.Actor)));
        var rick = Assert.Single(upgraded.Story.Player.RetainedAllies, row => row.Actor.Value == "ally-12");
        Assert.Equal(5, rick.Progress!.Level);
        Assert.IsType<SessionStarted>(GameSession.RestoreBattle(latest, upgraded, rules));
    }

    [Fact]
    public void EquipmentRosterUpgradePreservesCarriedPartyAndRandomState()
    {
        var (_, original) = OpeningRestoreTests.Fixture(joined: true);
        var prior = Package(10, 20);
        var latest = Package(11, 30);
        var rules = RuleCompositions.Sf2();
        var saved = original with
        {
            Compatibility = OpeningSave.FieldCompatibility(prior, rules),
            Flags = original.Flags.Where(flag => flag != 256).Order().ToArray(),
            Allies = prior.Encounters["opening"].Deployments.Select(row =>
                original.Allies[row.Actor.Value == "ally-0" ? 0 : 1] with { Actor = row.Actor.Value }).ToArray(),
        };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(prior, saved, rules)).Session;
        var exact = FieldPartySnapshot.Capture(prior, session.Current.Exploration!.Party);
        var (restored, _) = exact.Rebuild(latest, saved.Flags, saved.Names,
            expected: OpeningSave.FieldCompatibility(latest, rules), priorRecipe: 10);
        var after = FieldPartySnapshot.Capture(latest, restored);
        Assert.Equal(30, after.Allies.Count);
        Assert.Equal(exact.MainSeed, after.MainSeed);
        Assert.Equal(exact.ThinkingSeed, after.ThinkingSeed);
        foreach (var ally in exact.Allies)
            EqualAlly(ally, after.Allies.Single(row => row.Actor == ally.Actor));
        Assert.Equal(exact.ActiveAllies, after.ActiveAllies);
        Assert.Equal(saved.Flags, session.Current.Story.Flags.Order());
    }

    private static void EqualAlly(SavedAlly expected, SavedAlly actual)
    {
        Assert.Equal(expected.Items, actual.Items);
        Assert.Equal(expected.Spells, actual.Spells);
        Assert.Equal(expected, actual with { Items = expected.Items, Spells = expected.Spells });
    }

    private static OpeningSaveData Upgrade(ScenarioDefinition definition, OpeningSaveData saved)
    {
        var outcome = GameSession.Restore(definition, saved, RuleCompositions.Sf2());
        Assert.True(outcome is SessionStarted, (outcome as SessionStartFailed)?.Failure.Code);
        var session = ((SessionStarted)outcome).Session;
        var capture = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(capture.Error);
        return Assert.IsType<OpeningSaveData>(capture.Data);
    }

    private static ScenarioDefinition Package(int revision, int count, ScenarioDefinition? basis = null)
    {
        basis ??= OpeningRestoreTests.Fixture().Definition;
        var opening = basis.Encounters["opening"];
        var original = opening.Deployments[0];
        var curve = Enumerable.Repeat(new StatGrowthFraction(32, 32), 29).ToArray();
        var growth = new BattleGrowthDefinition(0, 0, Enumerable.Repeat(new StatGrowth(7, 35, curve), 5).ToArray(),
            [], new Dictionary<byte, SpellRef>());
        var ids = count < 12 ? Enumerable.Range(0, count).ToArray() : new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13, 14, 15, 16, 17, 18, 11, 19, 12, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 }.Take(count).ToArray();
        var rows = ids.Select(id => original with
        {
            Definition = new(new($"ally-{id}"), original.Definition.ClassRule, 1, 7, 7, 7, 7, 7, false, 6, [],
                growth: growth, sourceLoadout: original.Definition.SourceLoadout),
            ProcessingOrder = id, Initialization = original.Initialization! with { AllyPartyMember = id },
        });
        SourceItemDefinition[] items =
        [
            new(127, "EMPTY", "", "", "", [], 0, 0, 0, "", "", "", 0, []),
            new(1, "ITEM", "", "", "", [], 0, 0, 0, "", "", "", 0, []),
        ];
        var definitions = new PrivateBattleDefinitions(null!, [], [], items, [], [], [],
            ids.Select(id => new SourceAllyDefinition((byte)id, $"ally-{id}", "", 0, 5, [])));
        var map = basis.Exploration!;
        var world = new ExplorationDefinition(map.Maps.Values, map.Programs.Values, provenance: map.Provenance,
            partyFlags: map.PartyFlags, visuals: map.Visuals) { GameplayRevision = 4 };
        var encounters = basis.Encounters.Values.Select(e => e == opening ? opening.WithDeployments(rows) : e);
        return new("private-battle01-to-battle02", encounters, definitions, world)
        {
            Persistence = new Sf2OpeningRestore(revision),
        };
    }
}

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
        var rows = Enumerable.Range(0, count).Select(id => original with
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
            Enumerable.Range(0, count).Select(id => new SourceAllyDefinition((byte)id, $"ally-{id}", "", 0, 5, [])));
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

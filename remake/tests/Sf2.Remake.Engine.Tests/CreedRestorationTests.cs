using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CreedRestorationTests
{
    [Theory]
    [InlineData(false, false, 15, 9)]
    [InlineData(true, false, 16, 10)]
    [InlineData(true, true, 9, 6)]
    public void CompletedRestorationRebuildsGoliathWithoutReplayingOrChangingParty(
        bool victory, bool independentBattle19, int x, int y)
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var flags = data.Flags.Concat(new[] { 774, 783 })
            .Concat(victory ? new[] { 523 } : []).Concat(independentBattle19 ? new[] { 519 } : []).ToArray();
        var slot = data with { Map = "map-33", Recipe = "creed-mansion", X = x, Y = y,
            Flags = flags, EgressMap = "map-9" };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, slot, RuleCompositions.Sf2())).Session;
        var field = session.Current.Exploration!;
        Assert.Equal(new MapPosition(x, y), field.PlayerEntity.Position);
        Assert.Equal(new MapPosition(17, 10), field.Entities[new("entity-128")].Position);
        Assert.Equal(3, field.Entities[new("entity-128")].Motion.Facing);
        Assert.DoesNotContain(999, session.Current.Story.Flags);
        var saved = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(saved.Error);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(slot.Allies),
            System.Text.Json.JsonSerializer.Serialize(saved.Data!.Allies));
        Assert.Equal(slot.MainSeed, saved.Data.MainSeed);
        Assert.Equal(slot.ThinkingSeed, saved.Data.ThinkingSeed);
        Assert.Equal(slot.Flags.Where(flag => flag is < 256 or > 383).Order(), saved.Data.Flags.Order());
        Assert.Equal(slot.EgressMap, saved.Data.EgressMap);
        Assert.Equal(slot.Gold, saved.Data.Gold);
        Assert.Equal(slot.Depot, saved.Data.Depot);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void BasementSaveAdmissionDoesNotInventAConsultationOrRestorationPrerequisite(bool restored, bool complete)
    {
        var (basis, data) = OpeningRestoreTests.Fixture();
        var map = new ExplorationMapDefinition(new("map-34"), new(new ushort[4096]),
            new([new(0, 0, 29, 34)]), [], []);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values, exploration: new([map], []));
        var player = new FieldReturnAnchor(map.Map, new(26, 28), 2);
        int[] flags = restored ? [523, 783] : complete ? [784] : [];
        if (complete)
            Assert.Throws<BattleRuleException>(() => basis.Persistence!.Select(definition, player, flags, null));
        else
        {
            var location = basis.Persistence!.Select(definition, player, flags, null);
            Assert.Equal("evil-spirit-shrine", location.Recipe);
            Assert.Equal(player, location.Anchor);
            basis.Persistence!.ValidateRegisters(data with { Recipe = location.Recipe, EgressMap = "map-9" });
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public void SpecialResidentDoesNotConsumeTheNextOrdinarySlotOrChangeFollowerIdentities(int followerCount)
    {
        var followers = Enumerable.Range(0, followerCount).Select(i => new MapFollowerSpawn(i + 7, i + 7)).ToArray();
        var allocation = MapEntityAllocator.Allocate([212, 219, 252, 222, 200, 63, 63], followers, 30, 128, 0);
        Assert.Equal(47, allocation.Aliases[130]);
        Assert.Equal(followerCount + 3, allocation.Aliases[131]);
        Assert.Equal(followerCount + 6, allocation.Aliases[134]);
        Assert.Equal(252, allocation.Slots.Single(row => row.Slot == 47).Sprite);
        for (int i = 0; i < followerCount; i++) Assert.Equal(i + 1, allocation.Aliases[i + 7]);
        Assert.Equal(0, allocation.Aliases[0]);
    }
}

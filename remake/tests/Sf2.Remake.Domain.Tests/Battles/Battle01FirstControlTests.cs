using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01FirstControlTests
{
    [Fact]
    public void FirstEntryCannotRestartTheCompletedEnemyPrefixOrSupplyBowiesWordEarly()
    {
        var before = Battle01EnemyStandbyTests.AllEnemiesCompleted();
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01FirstControl.Enter(before, 0)).ParamName);
        Assert.Null(before.Roster[0].AiBitfield); Assert.Equal(16, before.FirstRound!.CurrentTurnOffset);
        Assert.Equal((ushort?)0x0134, before.RandomSeedCopy);
    }

    [Fact]
    public void ComputedCandidateEntersPlayerSelectionAndOnlyItsMissingActivationWordIsSupplied()
    {
        var before = Round(); int actor = before.FirstRound!.FirstCandidate!.Value.CombatantIndex;
        var transition = Battle01FirstControl.Enter(before, actor); var after = transition.State!;
        Assert.Equal(Battle01FirstControlAvailability.Player, transition.Decision.Availability);
        Assert.Equal(actor, after.FirstControl!.ActorIndex); Assert.Equal(Battle01Phase.PlayerMovementSelection, after.Phase);
        Assert.True(after.FirstControl.CandidateWordSupplied); Assert.Equal((ushort?)0, after.Roster[1].AiBitfield);
        Assert.Null(before.Roster[1].AiBitfield); Assert.Null(after.Roster[0].AiBitfield); Assert.Null(after.Roster[2].AiBitfield);
        Assert.False(after.FirstControl.Preset.AllyAutoBattle); Assert.False(after.FirstControl.Preset.OpponentControl);
        Assert.Equal(10, after.FirstControl.Movement.Range.Budget);
        Assert.Equal((byte?)4, after.Roster[1].ClassId); Assert.Equal(12, after.FirstControl.Movement.Range.Profile.MovementType);
        Assert.Equal(before.Roster[1].Position, after.FirstControl.Movement.Cursor);
        Assert.Equal(new byte[] { 255 }, after.FirstControl.Movement.Preview.Directions);
        Assert.Same(before.FirstRound, after.FirstRound); Assert.Equal(0, after.FirstRound!.CurrentTurnOffset);
        Assert.Equal(0xA4991234u, after.RandomSeedImage); Assert.Equal(before.RandomSeedImage, after.RandomSeedImage);
        Assert.Same(before.Occupancy, after.Occupancy); Assert.Same(before.RegionFlags90Through105, after.RegionFlags90Through105);
        Assert.Same(before.Roster[1].Stats, after.Roster[1].Stats); Assert.Same(before.Roster[1].Deployment, after.Roster[1].Deployment);
        Assert.Throws<ArgumentException>(() => Battle01FirstControl.Enter(after, actor));
        Assert.Equal(JsonSerializer.Serialize(after), JsonSerializer.Serialize(Battle01FirstControl.Enter(before, actor).State));
    }

    [Theory]
    [InlineData(1, 0, 0, false, false, Battle01FirstControlAvailability.Player)]
    [InlineData(1, 16, 0, false, false, Battle01FirstControlAvailability.MuddledAi)]
    [InlineData(1, 0, 4, false, false, Battle01FirstControlAvailability.AiControlled)]
    [InlineData(1, 0, 0, true, false, Battle01FirstControlAvailability.AllyAutoBattle)]
    [InlineData(128, 0, 0, false, false, Battle01FirstControlAvailability.OpponentAi)]
    [InlineData(128, 0, 0, false, true, Battle01FirstControlAvailability.Player)]
    [InlineData(128, 0, 4, false, true, Battle01FirstControlAvailability.AiControlled)]
    [InlineData(1, 8, 0, false, false, Battle01FirstControlAvailability.Player)]
    [InlineData(1, 64, 0, false, false, Battle01FirstControlAvailability.Sleeping)]
    [InlineData(1, 1, 0, false, false, Battle01FirstControlAvailability.Stunned)]
    public void ActualStatusAndActivationFieldsChooseTheAcceptedBranch(int actor, int status, int word,
        bool autoBattle, bool opponentControl, Battle01FirstControlAvailability expected)
    {
        using var fixture = Battle01PlayerMovementTests.Fixture("h2", "map3-battle01-turn-control-static-v1");
        var pass = fixture.RootElement.GetProperty("controlDispatch").GetProperty("passes").GetProperty("execution");
        Assert.Equal("AI", pass.GetProperty("AI_CONTROLLED").GetString()); Assert.Equal("AI", pass.GetProperty("MUDDLE").GetString());
        Assert.Equal("player", pass.GetProperty("enemyOpponentControl").GetProperty("true").GetString());
        Assert.Equal(expected, Battle01FirstControl.Classify(actor, 12, 9, 18, (ushort)status, (ushort)word, autoBattle, opponentControl));
    }

    [Fact]
    public void DeadUnplacedOrMissingActivationInputsAreExplicitlyUnavailable()
    {
        Assert.Equal(Battle01FirstControlAvailability.Dead, Battle01FirstControl.Classify(1, 0, 9, 18, 0, 0, false, false));
        Assert.Equal(Battle01FirstControlAvailability.Unplaced, Battle01FirstControl.Classify(1, 12, 255, 18, 0, 0, false, false));
        Assert.Equal(Battle01FirstControlAvailability.MissingActivationWord,
            Battle01FirstControl.Classify(128, 12, 9, 18, 0, null, false, true));
    }

    [Fact]
    public void ExistingActivationWordIsPreservedAndAiIsNeitherExecutedNorSkipped()
    {
        var ai = Round(word: 4); var unavailable = Battle01FirstControl.Enter(ai, 1);
        Assert.Null(unavailable.State); Assert.Equal(1, unavailable.Decision.ActorIndex);
        Assert.Equal(Battle01FirstControlAvailability.AiControlled, unavailable.Decision.Availability);
        Assert.Equal((ushort?)4, ai.Roster[1].AiBitfield); Assert.Null(ai.FirstControl);
        var existing = Round(word: 0x8000); var entered = Battle01FirstControl.Enter(existing, 1).State!;
        Assert.False(entered.FirstControl!.CandidateWordSupplied); Assert.Equal((ushort?)0x8000, entered.Roster[1].AiBitfield);
        Assert.Same(existing.Roster[1], entered.Roster[1]);
        var muddled = Round(status: 0x20);
        Assert.Equal(Battle01FirstControlAvailability.MuddledAi, Battle01FirstControl.Enter(muddled, 1).Decision.Availability);
        Assert.Null(muddled.Roster[1].AiBitfield);
    }

    [Fact]
    public void FirstCandidateIsReadWithoutAssumingActorOneOrAdvancingTheOffset()
    {
        var round = Round(); var slots = round.FirstRound!.Slots.ToArray(); slots[0] = new(128, 6);
        var enemyFirst = WithOrder(round, slots);
        var unavailable = Battle01FirstControl.Enter(enemyFirst, 128);
        Assert.Null(unavailable.State); Assert.Equal(128, unavailable.Decision.ActorIndex);
        Assert.Equal(Battle01FirstControlAvailability.OpponentAi, unavailable.Decision.Availability);
        Assert.Equal(0, enemyFirst.FirstRound!.CurrentTurnOffset); Assert.Equal(slots, enemyFirst.FirstRound.Slots);
        Assert.Throws<ArgumentException>(() => Battle01FirstControl.Enter(enemyFirst, 1));
        slots[0] = new(255, 255); var sentinel = WithOrder(round, slots);
        Assert.Equal(Battle01FirstControlAvailability.Sentinel, Battle01FirstControl.Enter(sentinel, 255).Decision.Availability);
        Assert.Null(sentinel.FirstControl); Assert.Equal(round.RandomSeedImage, sentinel.RandomSeedImage);
    }

    [Fact]
    public void MovementProjectionFailureCannotInstallTheCandidateSupplement()
    {
        var terrain = Enumerable.Repeat((byte)1, 2304).ToArray(); terrain[18 * 48 + 10] = 16;
        var round = Round(terrain: terrain);
        Assert.Equal("terrain", Assert.Throws<ArgumentException>(() => Battle01FirstControl.Enter(round, 1)).ParamName);
        Assert.Null(round.FirstControl); Assert.Null(round.Roster[1].AiBitfield);
        Assert.Equal(Battle01Phase.FirstRoundGenerated, round.Phase); Assert.Equal(0xA4991234u, round.RandomSeedImage);
    }

    internal static Battle01InitializedState Round(ushort? word = null, ushort status = 0, byte[]? terrain = null)
    {
        var initial = Battle01FirstRoundTests.Initial(); var roster = initial.Roster.ToArray(); var actor = roster[1];
        roster[1] = new(actor.Deployment, Stats(actor.Stats, status, 5), 4, null, word);
        var before = new Battle01InitializedState(roster, initial.Regions.ToArray(),
            terrain ?? Enumerable.Repeat((byte)1, 2304).ToArray(), initial.Occupancy.ToArray(), initial.RandomSeedImage);
        return Battle01FirstRound.Enter(before);
    }
    internal static Battle01Stats Stats(Battle01Stats source, ushort status, byte move) => new(source.Level,
        source.HpMax, source.HpCurrent, source.MpMax, source.MpCurrent, source.Attack, source.Defense, source.Agility,
        move, status, source.Items, source.Spells);
    private static Battle01InitializedState WithOrder(Battle01InitializedState source, Battle01TurnEntry[] slots) =>
        new(source, source.Roster.ToArray(), source.RegionFlags90Through105.ToArray(), source.NewlyTestedRegionMask,
            source.RandomSeedImage, new(slots, [], []));
}

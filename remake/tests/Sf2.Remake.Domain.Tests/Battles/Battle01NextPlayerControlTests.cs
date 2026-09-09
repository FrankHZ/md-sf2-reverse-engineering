using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01NextPlayerControlTests
{
    [Fact]
    public void CompletedEnemyPrefixHandsOnlyActualBowieHisOwnRegularRangeAndCancelOrigin()
    {
        var before = Battle01EnemyStandbyTests.AllEnemiesCompleted(); string frozen = JsonSerializer.Serialize(before);
        Assert.Equal("actor", Assert.Throws<ArgumentException>(() => Battle01NextPlayerControl.Enter(before, 1)).ParamName);
        var ready = Battle01NextPlayerControl.Enter(before, 0).State!; var control = ready.FirstControl!;
        Assert.Equal(0, control.ActorIndex); Assert.Same(Battle01MovementProfile.Regular, control.Movement.Range.Profile);
        Assert.Equal(12, control.Movement.Range.Budget); Assert.Equal(ready.Roster[0].Stats.Move * 2, control.Movement.Range.Budget);
        Assert.True(control.CandidateWordSupplied); Assert.Null(before.Roster[0].AiBitfield); Assert.Equal((ushort?)0, ready.Roster[0].AiBitfield);
        Assert.Same(before.TurnCompletion, ready.TurnCompletion); Assert.Same(before.Occupancy, control.Movement.Range.OriginOccupancy);
        Assert.Same(before.FirstRound, ready.FirstRound); Assert.Equal((ushort?)0x0134, ready.RandomSeedCopy);
        var selected = Battle01PlayerMovement.SelectDestination(ready, 0, new(8, 17));
        var moved = Battle01PlayerMovement.Confirm(selected, 0); var cancelled = Battle01PlayerMovement.Cancel(moved, 0);
        Assert.Equal(new MapPosition(8, 17), moved.Roster[0].Position); Assert.Equal(new MapPosition(8, 18), cancelled.Roster[0].Position);
        Assert.Equal(before.Occupancy, cancelled.Occupancy); Assert.Same(before.TurnCompletion, cancelled.TurnCompletion);
        Assert.Equal(16, cancelled.FirstRound!.CurrentTurnOffset); Assert.Equal(before.AiMemory, cancelled.AiMemory);
        for (int i = 1; i < 9; i++) Assert.Same(before.Roster[i], cancelled.Roster[i]);
        Assert.Equal(frozen, JsonSerializer.Serialize(before));
        var firstEnemy = Battle01EnemyStandby.CompleteFirst(Battle01EnemyStandbyTests.SecondCompleted(), 128, Battle01EnemyStandbyTests.Policy);
        Assert.Equal("turnOrder", Assert.Throws<ArgumentException>(() => Battle01NextPlayerControl.Enter(firstEnemy, 0)).ParamName);
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01NextPlayerControl.Enter(ready, 0)).ParamName);
    }

    [Fact]
    public void NextPlayerUsesLiveOccupancyAndItsOwnCancelThenStopsAtTheActualEnemyCandidate()
    {
        var first = FirstCompleted(); var firstReceipt = first.TurnCompletion;
        var next = Battle01NextPlayerControl.Enter(first, first.FirstRound!.CurrentCandidate!.Value.CombatantIndex).State!;
        var control = next.FirstControl!; var range = control.Movement.Range;
        Assert.Equal(2, control.ActorIndex); Assert.Equal(Battle01Phase.PlayerMovementSelection, next.Phase);
        Assert.Same(Battle01MovementProfile.Centaur, range.Profile); Assert.Equal(14, range.Budget);
        Assert.Equal(next.Roster[2].Stats.Move * 2, range.Budget); Assert.Equal(new MapPosition(7, 18), range.Origin);
        Assert.Same(first.Occupancy, range.OriginOccupancy); Assert.Same(firstReceipt, next.TurnCompletion);
        Assert.Same(first.FirstRound, next.FirstRound); Assert.Equal(0xA4991234u, next.RandomSeedImage);
        Assert.True(control.CandidateWordSupplied); Assert.Equal((ushort?)0, next.Roster[2].AiBitfield);
        Assert.Null(first.Roster[2].AiBitfield); Assert.Null(next.Roster[0].AiBitfield);
        Assert.Same(first.Roster[1], next.Roster[1]);
        var occupied = new MapPosition(9, 17);
        Assert.Equal(1, range.OriginOccupancy[17 * 48 + 9]); Assert.NotNull(range.Grid.CostAt(occupied));
        var preview = Battle01PlayerMovement.SelectDestination(next, 2, occupied);
        Assert.False(preview.FirstControl!.Movement.CanConfirm);
        Assert.Equal("destination", Assert.Throws<ArgumentException>(() => Battle01PlayerMovement.Confirm(preview, 2)).ParamName);
        var selected = Battle01PlayerMovement.SelectDestination(next, 2, new(7, 17));
        var moved = Battle01PlayerMovement.Confirm(selected, 2);
        var cancelled = Battle01PlayerMovement.Cancel(moved, 2);
        Assert.Equal(new MapPosition(7, 18), cancelled.Roster[2].Position);
        Assert.Equal(new MapPosition(9, 17), cancelled.Roster[1].Position);
        Assert.Equal(first.Occupancy, cancelled.Occupancy); Assert.Same(firstReceipt, cancelled.TurnCompletion);
        Assert.Same(firstReceipt, moved.TurnCompletion); Assert.Equal(2, cancelled.FirstRound!.CurrentTurnOffset);
        var completed = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(
            Battle01PlayerMovement.SelectDestination(cancelled, 2, new(7, 17)), 2), 2,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        Assert.Equal(Battle01Phase.PlayerTurnCompleted, completed.Phase); Assert.Null(completed.FirstControl);
        Assert.Equal(2, completed.TurnCompletion!.CompletedActorIndex); Assert.Same(firstReceipt, completed.TurnCompletion.Previous);
        Assert.Equal(new Battle01FactionCounts(3, 6), completed.TurnCompletion.BeforeAfterTurn);
        Assert.Equal(completed.TurnCompletion.BeforeAfterTurn, completed.TurnCompletion.AfterAfterTurn);
        Assert.Equal(4, completed.FirstRound!.CurrentTurnOffset); Assert.Equal(first.FirstRound.Slots[2], completed.FirstRound.CurrentCandidate);
        Assert.Same(first.FirstRound.Slots, completed.FirstRound.Slots); Assert.Equal(64, completed.FirstRound.Slots.Count);
        Assert.Equal((byte)128, completed.FirstRound.CurrentCandidate!.Value.CombatantIndex);
        var unavailable = Battle01NextPlayerControl.Enter(completed, 128);
        Assert.Null(unavailable.State); Assert.Equal(Battle01FirstControlAvailability.OpponentAi, unavailable.Decision.Availability);
        Assert.Equal(128, unavailable.Decision.ActorIndex); Assert.Equal(first.RandomSeedImage, completed.RandomSeedImage);
        Assert.Equal(new MapPosition(9, 17), completed.Roster[1].Position); Assert.Equal(new MapPosition(7, 17), completed.Roster[2].Position);
        Assert.Equal(first.FirstRound.FirstCandidate, completed.FirstRound.FirstCandidate);
    }

    [Fact]
    public void WrongPhaseWrongActorAndSentinelNeverReenterFirstControlOrSkipAnEntry()
    {
        var first = FirstCompleted(); string frozen = JsonSerializer.Serialize(first);
        Assert.Equal("actor", Assert.Throws<ArgumentException>(() => Battle01NextPlayerControl.Enter(first, 1)).ParamName);
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01FirstControl.Enter(first, 2)).ParamName);
        var ready = Battle01NextPlayerControl.Enter(first, 2).State!;
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01NextPlayerControl.Enter(ready, 2)).ParamName);
        var slots = first.FirstRound!.Slots.ToArray(); slots[1] = new(255, 255);
        var order = new Battle01FirstRoundOrder(slots, [], []).AdvanceCompletedPlayerTurn();
        var sentinel = Replace(first, order: order);
        var unavailable = Battle01NextPlayerControl.Enter(sentinel, 255);
        Assert.Null(unavailable.State); Assert.Null(unavailable.Decision.ActorIndex);
        Assert.Equal(Battle01FirstControlAvailability.Sentinel, unavailable.Decision.Availability);
        Assert.Equal(2, sentinel.FirstRound!.CurrentTurnOffset); Assert.Equal(slots[2], sentinel.FirstRound.Slots[2]);
        Assert.Same(first.TurnCompletion, sentinel.TurnCompletion); Assert.Equal(frozen, JsonSerializer.Serialize(first));
        Assert.Equal(JsonSerializer.Serialize(ready), JsonSerializer.Serialize(Battle01NextPlayerControl.Enter(first, 2).State));
    }

    [Theory]
    [InlineData(0, 4, 1, Battle01FirstControlAvailability.AiControlled)]
    [InlineData(0x20, 0, 1, Battle01FirstControlAvailability.MuddledAi)]
    [InlineData(0x40, 0, 1, Battle01FirstControlAvailability.Sleeping)]
    [InlineData(1, 0, 1, Battle01FirstControlAvailability.Stunned)]
    [InlineData(0, 0, 3, Battle01FirstControlAvailability.UnsupportedMovementProfile)]
    public void CurrentCandidateClassificationStillRefusesUnconsumedBranches(ushort status, ushort word, byte classId,
        Battle01FirstControlAvailability expected)
    {
        var first = FirstCompleted(); var roster = first.Roster.ToArray(); var actor = roster[2];
        roster[2] = new(actor.Deployment, Battle01FirstControlTests.Stats(actor.Stats, status, 7),
            classId, null, word, actor.Position);
        var altered = Replace(first, roster); string frozen = JsonSerializer.Serialize(altered);
        var result = Battle01NextPlayerControl.Enter(altered, 2);
        Assert.Null(result.State); Assert.Equal(expected, result.Decision.Availability); Assert.Equal(2, result.Decision.ActorIndex);
        Assert.Same(first.TurnCompletion, altered.TurnCompletion); Assert.Equal(frozen, JsonSerializer.Serialize(altered));
    }

    [Fact]
    public void LateRangeFailureCannotSupplyTheMissingWordOrUndoThePreviousStay()
    {
        var first = FirstCompleted(); var terrain = first.Terrain.ToArray(); terrain[18 * 48 + 6] = 16;
        var invalid = Replace(first, terrain: terrain); string frozen = JsonSerializer.Serialize(invalid);
        Assert.Equal("terrain", Assert.Throws<ArgumentException>(() => Battle01NextPlayerControl.Enter(invalid, 2)).ParamName);
        Assert.Null(invalid.Roster[2].AiBitfield); Assert.Null(invalid.FirstControl);
        Assert.Same(first.TurnCompletion, invalid.TurnCompletion); Assert.Equal(frozen, JsonSerializer.Serialize(invalid));
    }

    internal static Battle01InitializedState FirstCompleted()
    {
        var round = Battle01FirstControlTests.Round(); var roster = round.Roster.ToArray(); var actor = roster[2];
        roster[2] = new(actor.Deployment, Battle01FirstControlTests.Stats(actor.Stats, 0, 7), 1, null);
        round = new(round, roster, round.RegionFlags90Through105.ToArray(), round.NewlyTestedRegionMask,
            round.RandomSeedImage, round.FirstRound!);
        var ready = Battle01FirstControl.Enter(round, 1).State!;
        return Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(
            Battle01PlayerMovement.SelectDestination(ready, 1, new(9, 17)), 1), 1,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
    }

    private static Battle01InitializedState Replace(Battle01InitializedState source, Battle01Combatant[]? roster = null,
        byte[]? terrain = null, Battle01FirstRoundOrder? order = null)
    {
        var initial = new Battle01InitializedState(roster ?? source.Roster.ToArray(), source.Regions.ToArray(),
            terrain ?? source.Terrain.ToArray(), source.Occupancy.ToArray(), source.RandomSeedImage);
        var retained = new Battle01InitializedState(initial, initial.Roster.ToArray(), source.RegionFlags90Through105.ToArray(),
            source.NewlyTestedRegionMask, source.RandomSeedImage, order ?? source.FirstRound!);
        return new(retained, retained.FirstRound!, source.TurnCompletion!);
    }
}

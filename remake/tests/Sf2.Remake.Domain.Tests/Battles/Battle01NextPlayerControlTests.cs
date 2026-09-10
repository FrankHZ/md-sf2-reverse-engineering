using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01NextPlayerControlTests
{
    [Fact]
    public void SarahCanMoveAndCancelAfterChesterDefeatWithoutRestoringADeadOccupant()
    {
        var defeated=Battle01EnemyPhysicalAttackTests.FirstAllyDefeatCompleted();
        var end=Battle01EnemyPursuit.CompleteNext(defeated,130,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var round=Battle01FirstRound.EnterNext(end);
        Assert.Throws<ArgumentException>(()=>Battle01NextPlayerControl.Enter(round,2));
        var ready=Battle01NextPlayerControl.Enter(round,1).State!;
        Assert.Equal(10,ready.FirstControl!.Movement.Range.Budget);
        var moved=Battle01PlayerMovement.SelectDestination(ready,1,new(10,17));
        Assert.Equal(2,moved.FirstControl!.Movement.GridCost);
        moved=Battle01PlayerMovement.Confirm(moved,1);
        var cancelled=Battle01PlayerMovement.Cancel(moved,1);
        Assert.Equal(new MapPosition(9,17),cancelled.Roster[1].Position);
        Assert.Null(cancelled.Roster[2].Position);Assert.Equal(0,cancelled.Roster[2].Stats.HpCurrent);
        Assert.Equal((ushort?)1,cancelled.Roster[2].Stats.CurrentDefeats);
        Assert.Same(end.TurnCompletion,cancelled.TurnCompletion);
        Assert.Equal(107,Battle01EnemyPursuitTests.Receipts(cancelled).Count());
        Assert.Equal(ready.Occupancy,cancelled.Occupancy);Assert.Equal(-1,cancelled.OccupantAt(new(9,9)));
        Assert.Equal((0x02A11234u,(ushort?)0x0234),(cancelled.RandomSeedImage,cancelled.RandomSeedCopy));
    }

    [Fact]
    public void RoundTenChesterMovementRetainsBothCorpsesAndSecondDefeatAwards()
    {
        var completed = Battle01PlayerPhysicalAttackTests.SecondDefeatCompleted();
        var generated = Battle01FirstRound.EnterNext(completed);
        var ready = Battle01NextPlayerControl.Enter(generated, 2).State!;
        Assert.Equal((10, (byte)0, 2, 14), (ready.FirstRound!.RoundNumber, ready.FirstRound.CurrentTurnOffset,
            ready.FirstControl!.ActorIndex, ready.FirstControl.Movement.Range.Budget));
        // The source terrain0 restriction is asserted by the required real Content route; this authored grid uses plains.
        var preview = Battle01PlayerMovement.SelectDestination(ready, 2, new(12, 14));
        Assert.Equal(2, preview.FirstControl!.Movement.GridCost);
        var moved = Battle01PlayerMovement.Confirm(preview, 2); Assert.Equal(new MapPosition(12, 14), moved.Roster[2].Position);
        var cancelled = Battle01PlayerMovement.Cancel(moved, 2); Assert.Equal(new MapPosition(11, 14), cancelled.Roster[2].Position);
        Assert.Same(completed.TurnCompletion, cancelled.TurnCompletion); Assert.Same(generated.FirstRound, cancelled.FirstRound);
        Assert.Same(ready.Roster[2].Stats, cancelled.Roster[2].Stats); Assert.Equal(ready.Occupancy, cancelled.Occupancy);
        Assert.Equal((0x9F861234u, (ushort?)0x0034, (ushort)7), (cancelled.RandomSeedImage, cancelled.RandomSeedCopy, cancelled.NewlyTestedRegionMask));
        Assert.Equal(new ushort[] { 3, 11, 9, 5, 5, 5, 0, 0, 5 }, cancelled.Roster.Select(u => u.Stats.HpCurrent));
        Assert.Equal(((uint?)120, (byte?)63, (ushort?)2, (byte?)10), (cancelled.CurrentGold,
            cancelled.Roster[0].Stats.CurrentExp, cancelled.Roster[0].Stats.CurrentKills, cancelled.Roster[2].Stats.CurrentExp));
        Assert.Equal(7, cancelled.Occupancy.Count(id => id >= 0)); Assert.Null(cancelled.Roster[6].Position); Assert.Null(cancelled.Roster[7].Position);
        Assert.Equal(generated.AiMemory, cancelled.AiMemory); Assert.Equal(generated.AiLastTargets, cancelled.AiLastTargets);
    }

    [Fact]
    public void ActualSarahControlAfterChesterAttackMovesAndCancelsWithoutTakingHerTurn()
    {
        var after = Battle01EnemyPhysicalAttackTests.AfterChesterPlayerRelay();
        var ready = Battle01NextPlayerControl.Enter(after, 1).State!;
        Assert.Equal((9, (byte)10, 1, 10, 76), (ready.FirstRound!.RoundNumber, ready.FirstRound.CurrentTurnOffset,
            ready.FirstControl!.ActorIndex, ready.FirstControl.Movement.Range.Budget, Battle01EnemyPursuitTests.Receipts(ready).Count()));
        Assert.Equal(new[] { new Battle01TurnEntry(2, 8), new(128, 6), new(129, 6), new(131, 6),
            new(133, 5), new(1, 4), new(130, 4), new(0, 3) }.Concat(Enumerable.Repeat(new Battle01TurnEntry(255, 255), 56)), ready.FirstRound.Slots);
        var preview = Battle01PlayerMovement.SelectDestination(ready, 1, new(10, 17));
        Assert.Equal(2, preview.FirstControl!.Movement.GridCost);
        var moved = Battle01PlayerMovement.Confirm(preview, 1);
        Assert.Equal(new MapPosition(10, 17), moved.Roster[1].Position);
        var cancelled = Battle01PlayerMovement.Cancel(moved, 1);
        Assert.Equal(new MapPosition?[] { new(11, 15), new(9, 17), new(11, 14), new(8, 3), new(9, 5),
            new(6, 3), new(10, 15), null, new(7, 5) }, cancelled.Roster.Select(u => u.Position));
        Assert.Equal(new ushort[] { 3, 11, 9, 5, 5, 5, 3, 0, 5 }, cancelled.Roster.Select(u => u.Stats.HpCurrent));
        Assert.Same(after.TurnCompletion, cancelled.TurnCompletion); Assert.Same(after.FirstRound, cancelled.FirstRound);
        Assert.Equal((0x25991234u, (ushort?)0x0634), (cancelled.RandomSeedImage, cancelled.RandomSeedCopy));
        Assert.Equal(((uint?)60, (byte?)39, (ushort?)1, (byte?)10), (cancelled.CurrentGold,
            cancelled.Roster[0].Stats.CurrentExp, cancelled.Roster[0].Stats.CurrentKills, cancelled.Roster[2].Stats.CurrentExp));
        Assert.Null(cancelled.Roster[2].Stats.CurrentKills); Assert.Equal(8, cancelled.Occupancy.Count(id => id >= 0));
        Assert.Equal(new[] { false, true, false }.Concat(Enumerable.Repeat(false, 13)), cancelled.RegionFlags90Through105);
        Assert.Equal(0, cancelled.NewlyTestedRegionMask);
    }

    [Fact]
    public void RoundNineChesterMovesAndCancelsWithRecordedDamageAndTheEarlierDefeatPreserved()
    {
        var after = Battle01EnemyPhysicalAttackTests.ChesterHitCompleted();
        var generated = Battle01FirstRound.EnterNext(after);
        Assert.Equal(9, generated.FirstRound!.RoundNumber);
        Assert.Equal(new[] { new Battle01TurnEntry(2, 8), new(128, 6), new(129, 6), new(131, 6),
            new(133, 5), new(1, 4), new(130, 4), new(0, 3) }.Concat(Enumerable.Repeat(new Battle01TurnEntry(255, 255), 56)), generated.FirstRound.Slots);
        Assert.Equal((0x71D31234u, (ushort?)0x0134), (generated.RandomSeedImage, generated.RandomSeedCopy));
        Assert.Equal(7, generated.NewlyTestedRegionMask);
        Assert.Equal(new[] { false, true, false }, generated.RegionFlags90Through105.Take(3));
        Assert.Equal(new ushort?[] { 0x2060, 0x2060, 0x2060, 0x2061, 0x2071, 0x2070 }, generated.Roster.Skip(3).Select(u => u.AiBitfield));
        var ready = Battle01NextPlayerControl.Enter(generated, 2).State!;
        Assert.Equal(0, ready.FirstRound!.CurrentTurnOffset); Assert.Equal(2, ready.FirstControl!.ActorIndex);
        Assert.Equal(14, ready.FirstControl.Movement.Range.Budget); Assert.Equal(9, ready.Roster[2].Stats.HpCurrent);
        var moved = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 2, new(12, 14)), 2);
        Assert.Equal(new MapPosition(12, 14), moved.Roster[2].Position);
        var cancelled = Battle01PlayerMovement.Cancel(moved, 2);
        Assert.Equal(new MapPosition(11, 14), cancelled.Roster[2].Position);
        Assert.Same(after.Roster[2].Stats, cancelled.Roster[2].Stats);
        Assert.Same(after.TurnCompletion, cancelled.TurnCompletion); Assert.Same(generated.FirstRound, cancelled.FirstRound);
        Assert.Equal(ready.Occupancy, cancelled.Occupancy);
        Assert.Equal(generated.RandomSeedImage, cancelled.RandomSeedImage); Assert.Equal(generated.RandomSeedCopy, cancelled.RandomSeedCopy);
        Assert.Equal(generated.AiLastTargets, cancelled.AiLastTargets); Assert.Equal(generated.AiMemory, cancelled.AiMemory);
        Assert.Equal((6, (byte?)39, (ushort?)1, (uint?)60), ((int)cancelled.Roster[0].Stats.HpCurrent,
            cancelled.Roster[0].Stats.CurrentExp, cancelled.Roster[0].Stats.CurrentKills, cancelled.CurrentGold));
        Assert.Null(cancelled.Roster[7].Position); Assert.Equal(0, cancelled.Roster[7].Stats.HpCurrent);
        Assert.Null(cancelled.Roster[2].Stats.CurrentExp); Assert.Null(cancelled.Roster[2].Stats.CurrentKills);
    }

    [Fact]
    public void BowieMovesAndCancelsAfterPhysicalReplayWithoutHealingOrLosingAttackHistory()
    {
        var after = Battle01EnemyPhysicalAttackTests.CompletedAttack();
        var ready = Battle01NextPlayerControl.Enter(after, 0).State!;
        Assert.Equal(12, ready.FirstControl!.Movement.Range.Budget);
        var selected = Battle01PlayerMovement.SelectDestination(ready, 0, new(10, 15));
        var confirmed = Battle01PlayerMovement.Confirm(selected, 0);
        var cancelled = Battle01PlayerMovement.Cancel(confirmed, 0);
        Assert.Equal(new MapPosition(11, 15), cancelled.Roster[0].Position);
        Assert.Same(after.Roster[0].Stats, cancelled.Roster[0].Stats);
        Assert.Equal(9, cancelled.Roster[0].Stats.HpCurrent);
        Assert.Same(after.TurnCompletion, cancelled.TurnCompletion);
        Assert.Equal(after.Occupancy, cancelled.Occupancy);
    }


    [Fact]
    public void NewRoundPlayerUsesCurrentOriginAndCancelKeepsThePreviousRound()
    {
        var generated = Battle01FirstRound.EnterNext(Battle01FirstRoundTests.CompletedFirstRound());
        var ready = Battle01NextPlayerControl.Enter(generated, 2).State!;
        Assert.Equal(new MapPosition(7,17), ready.FirstControl!.Movement.Range.Origin);
        var moved = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready,2,new(7,16)),2);
        var cancelled = Battle01PlayerMovement.Cancel(moved,2);
        Assert.Equal(ready.Occupancy, cancelled.Occupancy);
        Assert.Same(generated.TurnCompletion, cancelled.TurnCompletion);
        Assert.Same(generated.FirstRound, cancelled.FirstRound);
        Assert.Equal(generated.RandomSeedCopy, cancelled.RandomSeedCopy);
        Assert.Equal(new MapPosition(7,17), cancelled.Roster[2].Position);
    }
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

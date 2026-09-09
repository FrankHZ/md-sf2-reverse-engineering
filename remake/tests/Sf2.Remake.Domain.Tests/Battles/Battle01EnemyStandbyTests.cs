using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01EnemyStandbyTests
{
    [Fact]
    public void FirstInactiveEnemyMovesWestAndCompletesExactlyOneTurnWithIndependentThinkingRng()
    {
        var before = SecondCompleted(); string frozen = JsonSerializer.Serialize(before);
        var after = Battle01EnemyStandby.CompleteFirst(before, 128, Policy);
        var receipt = after.TurnCompletion!; var decision = receipt.EnemyStandby!;
        Assert.Equal(Battle01Phase.EnemyTurnCompleted, after.Phase); Assert.Null(after.FirstControl);
        Assert.Equal(6, after.FirstRound!.CurrentTurnOffset); Assert.Equal((byte)131, after.FirstRound.CurrentCandidate!.Value.CombatantIndex);
        Assert.Same(before.FirstRound!.Slots, after.FirstRound.Slots); Assert.Equal(64, after.FirstRound.Slots.Count);
        Assert.Same(before.TurnCompletion, receipt.Previous); Assert.Equal(128, receipt.CompletedActorIndex);
        Assert.Equal(2, receipt.Previous!.CompletedActorIndex); Assert.Equal(1, receipt.Previous.Previous!.CompletedActorIndex);
        Assert.Null(receipt.Previous.Previous.Previous); Assert.Equal(new Battle01FactionCounts(3, 6), receipt.BeforeAfterTurn);
        Assert.Equal(receipt.BeforeAfterTurn, receipt.AfterAfterTurn); Assert.Same(Policy, receipt.Policy);
        Assert.Equal((ushort?)0x3934, after.RandomSeedCopy); Assert.Equal(0xA4991234u, after.RandomSeedImage);
        Assert.Equal((ushort?)0x1234, before.RandomSeedCopy); Assert.Equal(7, before.NewlyTestedRegionMask);
        Assert.Equal(0, after.NewlyTestedRegionMask); Assert.Same(before.RegionFlags90Through105, after.RegionFlags90Through105);
        Assert.All(after.RegionFlags90Through105, flag => Assert.False(flag)); Assert.Same(before.AiLastTargets, after.AiLastTargets);
        Assert.Equal(0x14, after.AiMemory[0]); Assert.All(after.AiMemory.Skip(1), value => Assert.Equal(0, value));
        Assert.Equal(new MapPosition(7, 3), decision.Origin); Assert.Equal(new MapPosition(6, 3), decision.Destination);
        Assert.Equal(new byte[] { 2, 255 }, decision.MoveString); Assert.Equal(3, decision.Action); Assert.Equal(6, decision.MovementType);
        Assert.Equal(new byte[] { 8, 2, 1 }, decision.Rolls.Select(roll => roll.Range));
        Assert.Equal(new byte[] { 7, 0, 0 }, decision.Rolls.Select(roll => roll.Result));
        Assert.Equal(new ushort[] { 0x0734, 0x0034, 0x3934 }, decision.Rolls.Select(roll => roll.AfterSeedCopy));
        Assert.Equal(new[] { 61, 85, 1 }, decision.Rolls.Select(roll => roll.GeneratorSteps));
        Assert.All(decision.Rolls, roll => Assert.All(roll.GeneratedBytes.Take(roll.GeneratorSteps - 1), value => Assert.True(value >= roll.Range)));
        Assert.Equal(new int?[] { 2, 2, null, 2 }, decision.Candidates.Select(candidate => candidate.GridCost));
        Assert.Equal(new int?[] { null, null, null, 131 }, decision.Candidates.Select(candidate => candidate.Occupant));
        Assert.Equal(new[] { true, true, false, false }, decision.Candidates.Select(candidate => candidate.Eligible));
        Assert.Equal(-1, after.OccupantAt(new(7, 3))); Assert.Equal(128, after.OccupantAt(new(6, 3)));
        Assert.Equal(9, after.Occupancy.Count(value => value >= 0)); Assert.Null(after.Roster[0].AiBitfield);
        for (int i = 0; i < 9; i++)
        {
            Assert.Same(before.Roster[i].Stats, after.Roster[i].Stats); Assert.Same(before.Roster[i].Deployment, after.Roster[i].Deployment);
            Assert.Equal(before.Roster[i].AiBitfield, after.Roster[i].AiBitfield);
            if (i != 3) Assert.Same(before.Roster[i], after.Roster[i]);
        }
        Assert.Equal(frozen, JsonSerializer.Serialize(before));
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.CompleteFirst(after, 131, Policy)).ParamName);
    }

    [Theory]
    [InlineData("seed-missing", "randomSeedCopy")]
    [InlineData("seed-drift", "randomSeedCopy")]
    [InlineData("actor", "actor")]
    [InlineData("policy", "policy")]
    [InlineData("terrain", "terrain")]
    [InlineData("occupancy", "occupancy")]
    [InlineData("status", "status")]
    [InlineData("activation", "activation")]
    [InlineData("position", "position")]
    public void RejectionIncludingLateCompletionFailureLeavesBothPlayerReceiptsAndEveryByteUnchanged(string mutation, string field)
    {
        var source = SecondCompleted(); var roster = source.Roster.ToArray(); var terrain = source.Terrain.ToArray();
        var occupancy = source.Occupancy.ToArray();
        if (mutation == "terrain") terrain[3 * 48 + 6] = 16;
        if (mutation == "occupancy") occupancy[3 * 48 + 6] = 129;
        if (mutation == "status") roster[1] = new(roster[1].Deployment,
            Battle01FirstControlTests.Stats(roster[1].Stats, 1, 5), roster[1].ClassId, null, 0, roster[1].Position);
        if (mutation == "activation") roster[3] = new(roster[3].Deployment, roster[3].Stats, null, roster[3].EnemySource, 0x2061);
        if (mutation == "position") roster[3] = roster[3].WithPosition(new(7, 2));
        var input = CopyCompleted(source, roster, terrain, occupancy,
            mutation == "seed-missing" ? null : mutation == "seed-drift" ? (ushort)0x3412 : (ushort)0x1234);
        string frozen = JsonSerializer.Serialize(input);
        Assert.Equal(field, Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.CompleteFirst(input,
            mutation == "actor" ? 131 : 128, mutation == "policy" ? null : Policy)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(input)); Assert.Same(source.TurnCompletion, input.TurnCompletion);
        Assert.Equal(4, input.FirstRound!.CurrentTurnOffset);
    }

    [Theory]
    [InlineData(0x0034, 0, 0x3934, 0)]
    [InlineData(0x0034, 1, 0x3934, 0)]
    [InlineData(0x8034, 128, 0xB934, 0)]
    [InlineData(0xFF34, 255, 0x1C34, 0)]
    public void SignedRangeBoundariesStillAdvanceTheHighByteOnce(int seed, int range, int expectedSeed, int expectedResult)
    {
        var roll = Battle01EnemyStandby.ThinkingRoll((ushort)seed, (byte)range);
        Assert.Equal((ushort)expectedSeed, roll.AfterSeedCopy); Assert.Equal(expectedResult, roll.Result); Assert.Equal(1, roll.GeneratorSteps);
    }

    [Theory]
    [InlineData(3, 2)]
    [InlineData(7, 4)]
    [InlineData(2, 6)]
    public void ImmediateIdleRollsRetainMemoryAndAdvanceOnlyTheRealThinkingCall(byte seed, byte result)
    {
        var battle = SecondCompleted();
        var decision = Battle01EnemyStandby.Decide(battle, battle.Roster[3], (ushort)((seed << 8) | 0x34), 0x24);
        Assert.Equal(result, Assert.Single(decision.Rolls).Result); Assert.Equal((ushort)((result << 8) | 0x34), decision.SeedCopyAfter);
        Assert.Equal(0x24, decision.MemoryAfter); Assert.Empty(decision.Candidates); Assert.Equal(decision.Origin, decision.Destination);
        Assert.Equal(new byte[] { 255 }, decision.MoveString);
    }

    [Theory]
    [InlineData(0x8034, 7)]
    [InlineData(0xFF34, 7)]
    [InlineData(0x7F34, 0)]
    public void HighByteSignedEdgesMatchTheExistingThinkingHelper(int seed, byte result)
    {
        var roll = Battle01EnemyStandby.ThinkingRoll((ushort)seed, 8);
        Assert.Equal(result, roll.Result); Assert.Equal((ushort)((result << 8) | 0x34), roll.AfterSeedCopy);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public void BothTablesExcludeThePreviousIndexAndNoAlternativeClearsMemoryWithoutInventingASelectionRoll(byte count)
    {
        var battle = SecondCompleted(); var actor = battle.Roster[3];
        var result = Battle01EnemyStandby.Decide(battle, actor, 0x1234, count);
        Assert.Equal(count, result.Candidates.Count);
        Assert.Equal(count == 3 ? new MapPosition[] { new(7, 2), new(6, 4), new(8, 4) } :
            new MapPosition[] { new(7, 2), new(6, 3), new(7, 4), new(8, 3) }, result.Candidates.Select(candidate => candidate.Position));
        Assert.Equal(2, result.Rolls.Count); Assert.NotEqual(0, result.MemoryAfter >> 4);
        var terrain = Enumerable.Repeat((byte)255, 2304).ToArray(); terrain[3 * 48 + 7] = 1;
        var isolated = CopyCompleted(battle, terrain: terrain);
        var idle = Battle01EnemyStandby.Decide(isolated, isolated.Roster[3], 0x1234, count);
        Assert.Equal((byte)0, idle.MemoryAfter); Assert.Single(idle.Rolls); Assert.Equal((ushort)0x0734, idle.SeedCopyAfter);
        Assert.Equal(actor.Position, idle.Destination); Assert.Equal(new byte[] { 255 }, idle.MoveString);
    }

    [Fact]
    public void SourceDirectionMaskRetainsAnEarlierHigherCostBranchAndRejectsAnIncompletePath()
    {
        // Source DBE0/DC16 accumulates right=8 before lowering threshold for left=6.
        // A lowest-cost preview would take left; the source picks right and then north.
        var low = new byte[2304]; var high = Enumerable.Repeat((byte)255, 2304).ToArray();
        foreach ((int x, int y, byte cost) in new[] { (7, 3, (byte)10), (8, 3, (byte)8), (6, 3, (byte)6),
            (8, 2, (byte)4), (7, 2, (byte)0) }) { low[y * 48 + x] = cost; high[y * 48 + x] = 0; }
        var grid = new Battle01MovementGrid(low, high, []);
        Assert.Equal(new byte[] { 0, 3, 2, 255 }, Battle01EnemyStandby.SourceMoveString(grid, new(7, 2), new(7, 3)));
        high[2 * 48 + 7] = 255;
        Assert.Equal("path", Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.SourceMoveString(
            new(low, high, []), new(7, 2), new(7, 3))).ParamName);
    }

    [Fact]
    public void EligibilityFollowsTheAcceptedCallerMatrixAndKeepsMoveOrderOutsideThePrimitive()
    {
        using var fixture = Battle01PlayerMovementTests.Fixture("h2", "battle-ai-remaining-static-v1");
        var battle = SecondCompleted(); var source = battle.Roster[3];
        foreach (var row in fixture.RootElement.GetProperty("expected").GetProperty("standby").GetProperty("eligibility").GetProperty("decisionMatrix").EnumerateArray())
        {
            byte Field(string name, byte absent) => row.GetProperty(name).GetBoolean() ? (byte)0 : absent;
            var d = source.Deployment with { PrimaryOrder = Field("primaryOrder", 255), SecondaryOrder = Field("secondaryOrder", 255),
                PrimaryRegion = Field("primaryTriggerConfigured", 15), SecondaryRegion = Field("secondaryTriggerConfigured", 15) };
            var actor = new Battle01Combatant(d, source.Stats, null, source.EnemySource, source.AiBitfield);
            if (row.GetProperty("callerOutcome").GetString() == "move-order")
                Assert.Equal("moveOrder", Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.Decide(battle, actor, 0x1234, 4)).ParamName);
            else
            {
                var decision = Battle01EnemyStandby.Decide(battle, actor, 0x1234, 4);
                bool regular = row.GetProperty("callerOutcome").GetString() == "regular-move";
                Assert.Equal(regular ? new MapPosition(6, 3) : source.Position, decision.Destination);
                Assert.Equal(regular ? 2 : 1, decision.Rolls.Count);
                if (!regular) Assert.Equal((byte)4, decision.MemoryAfter);
            }
        }
    }

    [Fact]
    public void CostZeroCanSelectTheActorsOwnCellButRelevantUnknownOccupancyRejects()
    {
        var source = SecondCompleted(); var roster = source.Roster.ToArray(); roster[3] = roster[3].WithPosition(new(6, 3));
        var battle = CopyCompleted(source, roster);
        var decision = Battle01EnemyStandby.Decide(battle, roster[3], 0x1234, 4);
        Assert.Equal(new MapPosition(6, 3), decision.Destination); Assert.Equal(new byte[] { 255 }, decision.MoveString);
        var own = decision.Candidates.Single(candidate => candidate.Index == 1);
        Assert.Equal(0, own.GridCost); Assert.Equal(128, own.Occupant); Assert.True(own.Eligible);
        roster[0] = roster[0].WithPosition(new(6, 2)); battle = CopyCompleted(source, roster);
        Assert.Equal("activation", Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.Decide(battle, roster[3], 0x1234, 4)).ParamName);
        Assert.Null(battle.Roster[0].AiBitfield);
    }

    internal static Battle01StayCompletionPolicy Policy => Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;
    internal static Battle01InitializedState SecondCompleted()
    {
        var original = Battle01FirstRoundTests.Initial(); var roster = original.Roster.ToArray();
        MapPosition[] enemies = [new(7, 3), new(9, 4), new(6, 4), new(8, 3), new(9, 5), new(6, 5)];
        for (int i = 0; i < 9; i++)
        {
            var unit = roster[i]; var deployment = i >= 3 ? unit.Deployment with { Position = enemies[i - 3] } : unit.Deployment;
            roster[i] = new(deployment, i < 3 ? Battle01FirstControlTests.Stats(unit.Stats, 0, (byte)(i == 2 ? 7 : 5)) : unit.Stats,
                i < 3 ? (byte)(i == 1 ? 4 : 1) : null, unit.EnemySource, unit.AiBitfield);
        }
        var terrain = Enumerable.Repeat((byte)255, 2304).ToArray();
        for (int y = 0; y < 20; y++) for (int x = 0; x < 16; x++) terrain[y * 48 + x] = 1;
        terrain[4 * 48 + 7] = 255; terrain[2 * 48 + 7] = 0;
        var occupancy = Enumerable.Repeat(-1, 2304).ToArray(); foreach (var unit in roster) occupancy[unit.Position.Y * 48 + unit.Position.X] = unit.Index;
        var initial = new Battle01InitializedState(roster, original.Regions.ToArray(), terrain, occupancy, original.RandomSeedImage, 0x1234);
        var ready = Battle01FirstControl.Enter(Battle01FirstRound.Enter(initial), 1).State!;
        var first = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 1, new(9, 17)), 1), 1, Policy);
        var next = Battle01NextPlayerControl.Enter(first, 2).State!;
        return Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(next, 2, new(7, 17)), 2), 2, Policy);
    }
    internal static Battle01InitializedState CopyCompleted(Battle01InitializedState source, Battle01Combatant[]? roster = null,
        byte[]? terrain = null, int[]? occupancy = null, ushort? seedCopy = 0x1234)
    {
        var initial = new Battle01InitializedState(roster ?? source.Roster.ToArray(), source.Regions.ToArray(),
            terrain ?? source.Terrain.ToArray(), occupancy ?? source.Occupancy.ToArray(), source.RandomSeedImage, seedCopy);
        var round = new Battle01InitializedState(initial, initial.Roster.ToArray(), source.RegionFlags90Through105.ToArray(),
            source.NewlyTestedRegionMask, source.RandomSeedImage, source.FirstRound!);
        return new(round, source.FirstRound!, source.TurnCompletion!);
    }
}

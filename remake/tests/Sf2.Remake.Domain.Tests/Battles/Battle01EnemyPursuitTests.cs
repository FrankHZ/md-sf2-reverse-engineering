using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01EnemyPursuitTests
{
    private static Battle01StayCompletionPolicy Policy => Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;

    [Fact]
    public void ActualOrderIncludesTheInactivePrefixOccupiedFallbackAndBothCommandsets()
    {
        var generated = RoundThree();
        AssertOrder(generated, [2, 129, 130, 131, 128, 133, 0, 1, 132], [8, 6, 6, 6, 5, 5, 4, 4, 4], 0x9BD71234);
        Assert.Equal(new[] { false, true, false }, generated.RegionFlags90Through105.Take(3));
        Assert.Equal(new ushort?[] { 0x2060, 0x2060, 0x2060, 0x2061, 0x2071, 0x2070 }, generated.Roster.Skip(3).Select(unit => unit.AiBitfield));
        var current = generated;
        for (int i = 0; i < 3; i++) current = CompleteTurn(current);
        Assert.Equal(new MapPosition(9, 5), current.Roster.Single(unit => unit.Index == 129).Position);
        Assert.Equal(new MapPosition(5, 4), current.Roster.Single(unit => unit.Index == 130).Position);
        var first = current; string frozen = JsonSerializer.Serialize(first);
        current = Battle01EnemyPursuit.CompleteNext(first, 131, Policy);
        var decision = current.TurnCompletion!.EnemyPursuit!;
        Assert.Equal((byte)6, decision.CommandSet); Assert.Equal(new[] { 28, 28, 28 }, decision.TargetCosts.Select(item => item.Cost));
        Assert.Equal(new[] { 0, 1, 2 }, decision.TargetCosts.Select(item => item.ActorIndex)); Assert.Equal(0, decision.TargetIndex);
        Assert.Equal(new MapPosition(9, 5), decision.PreliminaryDestination); Assert.Equal(new byte[] { 0, 3, 255 }, decision.PreliminaryMoveString);
        Assert.Equal(new MapPosition(9, 4), decision.Destination); Assert.Equal(new byte[] { 0, 255 }, decision.MoveString); Assert.Equal(2, decision.GridCost);
        Assert.Equal(0x24, decision.MemoryBefore); Assert.Equal((ushort)0x0034, decision.SeedCopyBefore);
        AssertPursuitRetained(first, current); Assert.Equal(frozen, JsonSerializer.Serialize(first));
        Assert.Equal(8, current.FirstRound!.CurrentTurnOffset);
        Assert.Equal("actor", Assert.Throws<ArgumentException>(() => Battle01EnemyPursuit.CompleteNext(current, 131, Policy)).ParamName);
        while (current.FirstRound!.CurrentCandidate!.Value.CombatantIndex != 132) current = CompleteTurn(current);
        var beforeSecond = current; current = CompleteTurn(current); decision = current.TurnCompletion!.EnemyPursuit!;
        Assert.Equal((byte)7, decision.CommandSet); Assert.Equal(new[] { 22, 26, 30 }, decision.TargetCosts.Select(item => item.Cost));
        Assert.Equal(0, decision.TargetIndex); Assert.Equal(new MapPosition(11, 6), decision.Destination);
        Assert.Equal(new byte[] { 0, 3, 255 }, decision.PreliminaryMoveString); Assert.Equal(new byte[] { 0, 3, 255 }, decision.MoveString);
        Assert.Equal(4, decision.GridCost); AssertPursuitRetained(beforeSecond, current);
        Assert.Equal(18, current.FirstRound!.CurrentTurnOffset); Assert.Equal(27, Receipts(current).Count());
        Assert.Equal((ushort?)0x0234, current.RandomSeedCopy); Assert.Equal(new byte[] { 0x14, 0x24, 0x14, 0x24, 0x34, 0x34 }, current.AiMemory.Take(6));
        Assert.Equal(958, Receipts(current).Take(9).Sum(r => r.EnemyStandby?.Rolls.Sum(roll => roll.GeneratorSteps) ?? 0));
        var fourth = Battle01FirstRound.EnterNext(current);
        AssertOrder(fourth, [1, 2, 130, 132, 129, 131, 0, 128, 133], [6, 6, 6, 6, 5, 5, 4, 4, 4], 0x51DC1234);
        Assert.Equal(Battle01FirstControlAvailability.Player, Battle01NextPlayerControl.Enter(fourth, 1).Decision.Availability);
    }

    [Fact]
    public void RepeatedPursuitStopsAtTheActualRoundSixAttackCohortBeforeAnyMutation()
    {
        var current = RoundThree();
        foreach (var expected in new[] { (Round: 3, Copy: 0x0234, Steps: 958), (Round: 4, Copy: 0x0234, Steps: 512), (Round: 5, Copy: 0x5634, Steps: 380) })
        {
            while (current.FirstRound!.CurrentCandidate is not null) current = CompleteTurn(current);
            Assert.Equal(expected.Round, current.FirstRound.RoundNumber); Assert.Equal((ushort?)expected.Copy, current.RandomSeedCopy);
            Assert.Equal(expected.Steps, Receipts(current).Take(9).Sum(r => r.EnemyStandby?.Rolls.Sum(roll => roll.GeneratorSteps) ?? 0));
            current = Battle01FirstRound.EnterNext(current);
        }
        AssertOrder(current, [2, 129, 130, 1, 128, 132, 0, 131, 133], [7, 6, 6, 5, 5, 5, 4, 4, 4], 0x07821234);
        for (int i = 0; i < 5; i++) current = CompleteTurn(current);
        string frozen = JsonSerializer.Serialize(current);
        var error = Assert.Throws<Battle01AttackSelectionRequiredException>(() => Battle01EnemyPursuit.CompleteNext(current, 132, Policy));
        Assert.Equal(132, error.ActorIndex); Assert.Equal("attack.targets", error.ParamName);
        Assert.Equal(new[] { new Battle01AttackCandidate(0, new(11, 14), 8) }, error.Targets);
        Assert.Equal(50, Receipts(current).Count()); Assert.Equal(128, current.TurnCompletion!.CompletedActorIndex);
        Assert.Equal(10, current.FirstRound!.CurrentTurnOffset); Assert.Equal(new MapPosition(11, 10), current.Roster[7].Position);
        Assert.Equal(0x07821234u, current.RandomSeedImage); Assert.Equal((ushort?)0x0034, current.RandomSeedCopy);
        Assert.Equal(new byte[] { 4, 0x34, 0x24, 0x24, 0x34, 0x24 }, current.AiMemory.Take(6)); Assert.Equal(0, current.NewlyTestedRegionMask);
        Assert.Equal(frozen, JsonSerializer.Serialize(current));
    }

    [Fact]
    public void EnemyFirstPursuitClearsOnlyTestedBitsAndRetainedFlagsSurviveLeavingTheRegion()
    {
        var original = RoundThree(); var slots = original.FirstRound!.Slots.ToArray(); (slots[0], slots[8]) = (slots[8], slots[0]);
        var enemyFirst = Copy(original, original.Roster.ToArray(), order: new(slots, [], [], 3));
        var completed = Battle01EnemyPursuit.CompleteNext(enemyFirst, 132, Policy);
        Assert.Equal(7, enemyFirst.NewlyTestedRegionMask); Assert.Equal(0, completed.NewlyTestedRegionMask);
        Assert.Equal(2, completed.FirstRound!.CurrentTurnOffset); Assert.Equal(19, Receipts(completed).Count());
        AssertPursuitRetained(enemyFirst, completed);
        var current = original; while (current.FirstRound!.CurrentCandidate is not null) current = CompleteTurn(current);
        // Authored position-only seam: the already-triggered flag must not clear when all allies leave.
        var roster = current.Roster.ToArray(); roster[0] = roster[0].WithPosition(new(8, 17)); var departed = Copy(current, roster);
        Assert.All(departed.Regions, region => Assert.DoesNotContain(departed.Roster.Take(3), ally => Battle01FirstRound.IsInside(region, ally.Position)));
        var next = Battle01FirstRound.EnterNext(departed);
        Assert.Equal(new[] { false, true, false }, next.RegionFlags90Through105.Take(3));
        Assert.Equal(departed.Roster.Select(unit => unit.AiBitfield), next.Roster.Select(unit => unit.AiBitfield));
    }

    [Fact]
    public void AnEnemyFirstAttackRejectionRetainsIncomingTestedSevenAndTheGeneratedOrder()
    {
        var original = RoundThree(); var slots = original.FirstRound!.Slots.ToArray(); (slots[0], slots[8]) = (slots[8], slots[0]);
        var roster = original.Roster.ToArray(); roster[7] = roster[7].WithPosition(new(11, 10));
        var current = Copy(original, roster, order: new(slots, [], [], 3)); string frozen = JsonSerializer.Serialize(current);
        var error = Assert.Throws<Battle01AttackSelectionRequiredException>(() => Battle01EnemyPursuit.CompleteNext(current, 132, Policy));
        Assert.Equal(0, Assert.Single(error.Targets).ActorIndex); Assert.Equal(7, current.NewlyTestedRegionMask);
        Assert.Equal(0, current.FirstRound!.CurrentTurnOffset); Assert.Equal(18, Receipts(current).Count());
        Assert.Equal(frozen, JsonSerializer.Serialize(current));
    }

    [Fact]
    public void SourceDestinationFailureCompletesOriginStayAndKeepsTheThinkingHistory()
    {
        var original = FirstPursuit(); var roster = original.Roster.ToArray();
        int[] indexes = [3, 4, 5, 7, 8]; MapPosition[] occupied = [new(9, 5), new(9, 4), new(8, 5), new(10, 5), new(9, 6)];
        for (int i = 0; i < indexes.Length; i++) roster[indexes[i]] = roster[indexes[i]].WithPosition(occupied[i]);
        var before = Copy(original, roster); var after = Battle01EnemyPursuit.CompleteNext(before, 131, Policy);
        Assert.Equal(before.Roster[6].Position, after.TurnCompletion!.EnemyPursuit!.Destination);
        Assert.Equal(new byte[] { 255 }, after.TurnCompletion.EnemyPursuit.MoveString); Assert.Equal(0, after.TurnCompletion.EnemyPursuit.GridCost);
        AssertPursuitRetained(before, after); Battle01EnemyStandby.RequireThinkingHistory(after);
    }

    [Fact]
    public void RadiusSearchUsesStrictLowerCostFirstTieAndOwnCellZeroBeforeOccupancy()
    {
        var low = Enumerable.Repeat((byte)255, 2304).ToArray(); var high = low.ToArray(); var occupancy = Enumerable.Repeat(-1, 2304).ToArray();
        void Set(int x, int y, byte cost) { low[y * 48 + x] = cost; high[y * 48 + x] = 0; }
        Set(9, 4, 2); Set(8, 5, 2); Set(10, 5, 4); Set(9, 6, 6); var grid = new Battle01MovementGrid(low, high, []);
        Assert.Equal(new MapPosition(9, 4), Battle01EnemyPursuit.DetermineAttackPosition(grid, new(9, 5), 1, occupancy));
        occupancy[4 * 48 + 9] = 129;
        Assert.Equal(new MapPosition(8, 5), Battle01EnemyPursuit.DetermineAttackPosition(grid, new(9, 5), 1, occupancy));
        Set(9, 6, 1);
        Assert.Equal(new MapPosition(9, 6), Battle01EnemyPursuit.DetermineAttackPosition(grid, new(9, 5), 1, occupancy));
        Set(9, 5, 0); occupancy[5 * 48 + 9] = 131;
        Assert.Equal(new MapPosition(9, 5), Battle01EnemyPursuit.DetermineAttackPosition(grid, new(9, 5), 0, occupancy));
    }

    [Fact]
    public void PreliminaryWalkRetainsAccumulatedBitsAndSupportsAValidEmptyPath()
    {
        var low = Enumerable.Repeat((byte)255, 2304).ToArray(); var high = low.ToArray();
        void Set(int x, int y, byte cost) { low[y * 48 + x] = cost; high[y * 48 + x] = 0; }
        Set(7, 3, 6); Set(8, 3, 4); Set(7, 2, 2); Set(8, 2, 0); var grid = new Battle01MovementGrid(low, high, []);
        var path = Battle01EnemyStandby.SourceWalk(grid, new(7, 3), 2);
        Assert.Equal(new MapPosition(8, 2), path.Destination); Assert.Equal(new byte[] { 0, 1, 255 }, path.MoveString);
        Assert.Equal(new byte[] { 255 }, Battle01EnemyStandby.SourceWalk(grid, new(8, 2), 0).MoveString);
        Set(15, 4, 2); Set(16, 4, 0);
        Assert.Equal("path", Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.SourceWalk(grid, new(15, 4), 0)).ParamName);
        high[2 * 48 + 8] = 255;
        Assert.Equal("path", Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.SourceWalk(grid, new(7, 3), 0)).ParamName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IncompleteOrHighTargetCostsRejectBeforeTheDisputedClassBranch(bool highCost)
    {
        var original = FirstPursuit(); var roster = original.Roster.ToArray(); var terrain = Enumerable.Repeat((byte)255, 2304).ToArray();
        roster[6] = roster[6].WithPosition(new(1, 1));
        for (int i = 0; i < 3; i++) roster[i] = roster[i].WithPosition(new(5 - i, 9));
        if (highCost)
        {
            for (int y = 1; y <= 9; y += 2) for (int x = 1; x <= 14; x++) terrain[y * 48 + x] = 1;
            for (int y = 2; y <= 8; y += 2) terrain[y * 48 + (y % 4 == 2 ? 14 : 1)] = 1;
            var grid = Battle01PlayerMovement.BuildWeightedGrid(terrain, Battle01PlayerMovement.HoveringCosts, 49, 128);
            Assert.Equal(128, grid.CostAt(new(5, 9)));
        }
        var current = Copy(original, roster, terrain); string frozen = JsonSerializer.Serialize(current);
        Assert.Equal("pursuit.targets", Assert.Throws<ArgumentException>(() => Battle01EnemyPursuit.CompleteNext(current, 131, Policy)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(current));
    }

    [Theory]
    [InlineData("actor", "actor")]
    [InlineData("inactive", "activation")]
    [InlineData("secondary", "activation")]
    [InlineData("status", "status")]
    [InlineData("terrain", "terrain")]
    [InlineData("occupancy", "occupancy")]
    [InlineData("policy", "policy")]
    public void InvalidAdmissionAndLateInputsRetainTheExactState(string mutation, string field)
    {
        var original = FirstPursuit(); var roster = original.Roster.ToArray(); var terrain = original.Terrain.ToArray();
        if (mutation == "secondary") { var unit = roster[6]; roster[6] = new(unit.Deployment with { SecondaryOrder = 0 }, unit.Stats, unit.ClassId, unit.EnemySource, unit.AiBitfield, unit.Position); }
        if (mutation == "status") { var unit = roster[0]; roster[0] = new(unit.Deployment, Battle01FirstControlTests.Stats(unit.Stats, 1, unit.Stats.Move), unit.ClassId, unit.EnemySource, unit.AiBitfield, unit.Position); }
        if (mutation == "terrain") terrain[0] = 16;
        var occupancy = Occupancy(roster); if (mutation == "occupancy") occupancy[17 * 48 + 9] = -1;
        var current = Battle01FirstRoundTests.CopyCurrent(original, roster: roster, terrain: terrain, occupancy: occupancy);
        string frozen = JsonSerializer.Serialize(current);
        if (mutation == "inactive")
            Assert.Equal(field, Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.CompleteNext(current, 131, Policy)).ParamName);
        else
            Assert.Equal(field, Assert.Throws<ArgumentException>(() => Battle01EnemyPursuit.CompleteNext(current, mutation == "actor" ? 132 : 131, mutation == "policy" ? null : Policy)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(current));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("both")]
    [InlineData("seed")]
    [InlineData("memory")]
    [InlineData("actor")]
    [InlineData("player")]
    [InlineData("main")]
    public void MixedHistoryRejectsMissingConflictingOrChangedPursuitEvidence(string mutation)
    {
        var before = FirstPursuit(); var current = Battle01EnemyPursuit.CompleteNext(before, 131, Policy);
        var receipt = current.TurnCompletion!; var decision = receipt.EnemyPursuit!;
        receipt = mutation switch
        {
            "missing" => receipt with { EnemyPursuit = null },
            "both" => receipt with { EnemyStandby = before.TurnCompletion!.EnemyStandby },
            "seed" => receipt with { EnemyPursuit = decision with { SeedCopyAfter = 0x1234 } },
            "memory" => receipt with { EnemyPursuit = decision with { MemoryAfter = 0 } },
            "actor" => receipt with { EnemyPursuit = decision with { ActorIndex = 132 } },
            "player" => receipt with { Previous = receipt.Previous! with { CompletedActorIndex = 2, EnemyStandby = null, EnemyPursuit = decision } },
            "main" => receipt with { EnemyPursuit = decision with { MainSeedImage = 0 } },
            _ => throw new InvalidOperationException()
        };
        var bad = Battle01FirstRoundTests.CopyCurrent(current, receipt: receipt); string frozen = JsonSerializer.Serialize(bad);
        Assert.Equal("completion", Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.RequireThinkingHistory(bad)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(bad));
    }

    internal static Battle01InitializedState RoundThree()
    {
        var current = Battle01FirstRound.EnterNext(Battle01FirstRoundTests.CompletedFirstRound());
        while (current.FirstRound!.CurrentCandidate is { } candidate)
        {
            if (candidate.CombatantIndex != 0) { current = CompleteTurn(current); continue; }
            var ready = Battle01NextPlayerControl.Enter(current, 0).State!;
            var selected = Battle01PlayerMovement.SelectDestination(ready, 0, new(11, 15));
            current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(selected, 0), 0, Policy);
        }
        return Battle01FirstRound.EnterNext(current);
    }
    internal static Battle01InitializedState FirstPursuit()
    {
        var current = RoundThree(); for (int i = 0; i < 3; i++) current = CompleteTurn(current); return current;
    }
    internal static Battle01InitializedState CompleteTurn(Battle01InitializedState current)
    {
        int actor = current.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
        if (actor >= 128 && (current.Roster.Single(unit => unit.Index == actor).AiBitfield!.Value & 1) != 0)
            return Battle01EnemyPursuit.CompleteNext(current, actor, Policy);
        return Battle01FirstRoundTests.CompleteOriginTurn(current);
    }
    internal static IEnumerable<Battle01TurnCompletionReceipt> Receipts(Battle01InitializedState current)
    {
        for (var receipt = current.TurnCompletion; receipt is not null; receipt = receipt.Previous) yield return receipt;
    }
    private static int[] Occupancy(Battle01Combatant[] roster)
    {
        var result = Enumerable.Repeat(-1, 2304).ToArray(); foreach (var unit in roster) result[unit.Position.Y * 48 + unit.Position.X] = unit.Index; return result;
    }
    private static Battle01InitializedState Copy(Battle01InitializedState source, Battle01Combatant[] roster, byte[]? terrain = null, Battle01FirstRoundOrder? order = null) =>
        Battle01FirstRoundTests.CopyCurrent(source, roster: roster, occupancy: Occupancy(roster), terrain: terrain, order: order);
    private static void AssertOrder(Battle01InitializedState current, byte[] actors, byte[] scores, uint main)
    {
        Assert.Equal(actors, current.FirstRound!.Slots.Take(9).Select(slot => slot.CombatantIndex));
        Assert.Equal(scores, current.FirstRound.Slots.Take(9).Select(slot => slot.AlteredAgility));
        Assert.Equal(64, current.FirstRound.Slots.Count); Assert.All(current.FirstRound.Slots.Skip(9), slot => Assert.Equal(new Battle01TurnEntry(255, 255), slot));
        Assert.Equal(main, current.RandomSeedImage);
    }
    private static void AssertPursuitRetained(Battle01InitializedState before, Battle01InitializedState after)
    {
        var receipt = after.TurnCompletion!; var decision = receipt.EnemyPursuit!;
        Assert.Null(receipt.EnemyStandby); Assert.Same(before.TurnCompletion, receipt.Previous);
        Assert.Equal(before.RandomSeedImage, after.RandomSeedImage); Assert.Equal(before.RandomSeedImage, decision.MainSeedImage);
        Assert.Equal(before.RandomSeedCopy, after.RandomSeedCopy); Assert.Equal(decision.SeedCopyBefore, decision.SeedCopyAfter);
        Assert.Equal(decision.MemoryBefore, decision.MemoryAfter); Assert.Equal(before.AiMemory, after.AiMemory);
        Assert.All(after.AiMemory.Skip(6), value => Assert.Equal(0, value)); Assert.Same(before.AiLastTargets, after.AiLastTargets);
        Assert.Same(before.RegionFlags90Through105, after.RegionFlags90Through105); Assert.Same(before.Terrain, after.Terrain);
        Assert.Equal(before.Roster.Select(unit => unit.AiBitfield), after.Roster.Select(unit => unit.AiBitfield));
        Assert.Equal(0, after.NewlyTestedRegionMask); Assert.Equal((byte)3, decision.Action);
        for (int i = 0; i < 9; i++) { Assert.Same(before.Roster[i].Stats, after.Roster[i].Stats); Assert.Same(before.Roster[i].Deployment, after.Roster[i].Deployment); }
        Assert.All(after.Roster, unit => Assert.Equal(unit.Index, after.OccupantAt(unit.Position)));
        Battle01EnemyStandby.RequireThinkingHistory(after);
    }
}

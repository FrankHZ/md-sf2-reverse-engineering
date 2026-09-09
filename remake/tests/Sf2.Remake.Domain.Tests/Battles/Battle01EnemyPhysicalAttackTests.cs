using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01EnemyPhysicalAttackTests
{
    internal static Battle01PhysicalCompletionPolicy Policy => Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike;

    [Fact]
    public void ActualRoundSixAttackReplaysHpOnceAndAdvancesToBowieWithAllRandomChannels()
    {
        var before = AttackBoundary(); string frozen = JsonSerializer.Serialize(before);
        var after = Battle01EnemyPhysicalAttack.CompleteNext(before, 132, Policy);
        var receipt = after.TurnCompletion!; var d = receipt.EnemyPhysicalAttack!;
        Assert.Equal(0, d.TargetIndex); Assert.Equal(new MapPosition(11, 14), d.Destination);
        Assert.Equal(new byte[] { 3, 3, 3, 3, 255 }, d.MoveString); Assert.Equal(8, d.GridCost);
        var p = Assert.Single(d.Priorities);
        Assert.Equal(3, p.Priority); Assert.Equal(57, p.Roll.GeneratorSteps); Assert.Equal(230, p.LandMultiplier);
        Assert.Equal(new ushort[] { 32, 32, 1, 1, 32, 32 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 12, 30, 0, 0, 11, 21 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new[] { "dodge", "critical", "spread-1", "spread-2", "double", "counter" }, d.Effect.Rolls.Select(r => r.Purpose));
        Assert.Equal(0xAF881234u, after.RandomSeedImage); Assert.Equal((ushort?)0x0134, after.RandomSeedCopy);
        Assert.False(d.Effect.Dodged); Assert.False(d.Effect.Critical);
        Assert.Equal((9, 12, 9), ((int)d.Effect.TemporaryHp, (int)d.Effect.RestoredHp, (int)after.Roster[0].Stats.HpCurrent));
        Assert.Equal(new Battle01PhysicalReaction(0, -3, 0, 0, 1), d.Effect.Reaction);
        Assert.Same(before.Roster[0].Stats, d.Effect.BeforeStats);
        Assert.Equal(0, after.AiLastTargets[4]); Assert.All(after.AiLastTargets.Where((_, i) => i != 4), value => Assert.Equal(255, value));
        Assert.Equal(before.AiMemory, after.AiMemory); Assert.Equal(before.RegionFlags90Through105, after.RegionFlags90Through105);
        Assert.Equal(before.Roster.Select(u => u.AiBitfield), after.Roster.Select(u => u.AiBitfield));
        Assert.Equal(-1, after.OccupantAt(new(11, 10))); Assert.Equal(132, after.OccupantAt(new(11, 14)));
        Assert.Equal(51, Battle01EnemyPursuitTests.Receipts(after).Count());
        Assert.Equal(12, after.FirstRound!.CurrentTurnOffset); Assert.Equal(0, after.FirstRound.CurrentCandidate!.Value.CombatantIndex);
        Assert.Same(before.FirstRound!.Slots, after.FirstRound.Slots);
        Assert.Same(Policy, receipt.Policy); Assert.Null(receipt.EnemyStandby); Assert.Null(receipt.EnemyPursuit);
        Assert.Equal(new Battle01FactionCounts(3, 6), receipt.BeforeAfterTurn); Assert.Equal(receipt.BeforeAfterTurn, receipt.AfterAfterTurn);
        Assert.Equal(frozen, JsonSerializer.Serialize(before));
        Assert.Equal(9, Battle01NextPlayerControl.Enter(after, 0).State!.Roster[0].Stats.HpCurrent);
    }

    [Theory]
    [InlineData("main")]
    [InlineData("thinking")]
    [InlineData("lastTarget")]
    [InlineData("memory")]
    [InlineData("hp")]
    [InlineData("reaction")]
    [InlineData("roll")]
    [InlineData("priority")]
    [InlineData("path")]
    [InlineData("policy")]
    [InlineData("mixed")]
    public void ForgedPhysicalHistoryCannotEnterTheNextPlayerTurn(string mutation)
    {
        var after = CompletedAttack(); var d = after.TurnCompletion!.EnemyPhysicalAttack!;
        var receipt = after.TurnCompletion; var roster = after.Roster.ToArray();
        var targets = after.AiLastTargets.ToArray(); var memory = after.AiMemory.ToArray();
        uint main = after.RandomSeedImage; ushort copy = after.RandomSeedCopy!.Value;
        switch (mutation)
        {
            case "main": main ^= 0x10000; break;
            case "thinking": copy ^= 0x100; break;
            case "lastTarget": targets[4] = 255; break;
            case "memory": memory[4] ^= 1; break;
            case "hp": roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentHp(10)); break;
            case "reaction": d = d with { Effect = d.Effect with { Reaction = d.Effect.Reaction! with { HpDelta = -2 } } }; break;
            case "roll":
                var rolls = d.Effect.Rolls.ToArray(); rolls[2] = rolls[2] with { Range = 2 };
                d = d with { Effect = d.Effect with { Rolls = rolls } }; break;
            case "priority": d = d with { Priorities = [d.Priorities[0] with { Priority = 4 }] }; break;
            case "path": d = d with { MoveString = [3, 3, 255] }; break;
            case "policy": receipt = receipt with { Policy = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats }; break;
            case "mixed": receipt = receipt with { EnemyStandby = receipt.Previous!.EnemyStandby }; break;
        }
        receipt = receipt with { EnemyPhysicalAttack = d };
        var forged = Battle01FirstRoundTests.CopyCurrent(after, roster: roster, memory: memory, copy: copy,
            receipt: receipt, mainImage: main, lastTargets: targets);
        string frozen = JsonSerializer.Serialize(forged);
        Assert.ThrowsAny<ArgumentException>(() => Battle01NextPlayerControl.Enter(forged, 0));
        Assert.Equal(frozen, JsonSerializer.Serialize(forged));
    }

    [Fact]
    public void FinalizationRejectsInconsistentReplayedHpAfterLocalEffectConstruction()
    {
        var before = AttackBoundary(); string frozen = JsonSerializer.Serialize(before);
        var d = Battle01EnemyPhysicalAttack.Decide(before, before.Roster[7]);
        var roster = before.Roster.ToArray(); var occupancy = before.Occupancy.ToArray();
        roster[7] = roster[7].WithPosition(d.Destination);
        roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentHp(10)); // replay should have produced9
        occupancy[11 + 10 * 48] = -1; occupancy[11 + 14 * 48] = 132;
        var targets = before.AiLastTargets.ToArray(); targets[4] = 0;
        var local = new Battle01InitializedState(before, roster, occupancy, before.AiMemory.ToArray(),
            d.SeedCopyAfter, d.Effect.MainSeedAfter, targets);
        Assert.Equal("attack.history", Assert.Throws<ArgumentException>(() =>
            Battle01TurnCompletion.CompletePhysical(local, d, Policy)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(before));
        Assert.Equal(50, Battle01EnemyPursuitTests.Receipts(before).Count());
    }

    [Fact]
    public void SourceArithmeticKeepsZeroIntermediateAndBothDownwardDrawsAtTheOriginalRange()
    {
        Assert.Equal(0, Battle01EnemyPhysicalAttack.LandDamage(4, 4, 230));
        var target = AttackBoundary().Roster[0].Stats;
        var effect = FindEffect(target, 12, 256, e => !e.Dodged && !e.Critical &&
            e.Rolls[2].Result == 1 && e.Rolls[3].Result == 1);
        Assert.Equal(6, effect.Damage);
        Assert.Equal(new ushort[] { 2, 2 }, effect.Rolls.Skip(2).Take(2).Select(r => r.Range));
        var zero = FindEffect(target, 4, 230, e => !e.Dodged && !e.Critical);
        Assert.Equal(1, zero.Damage); Assert.Equal(new ushort[] { 1, 1 }, zero.Rolls.Skip(2).Take(2).Select(r => r.Range));
    }

    [Fact]
    public void MissAndCriticalUseRealSeedsAndPreserveSourceCallOrder()
    {
        var target = AttackBoundary().Roster[0].Stats;
        var miss = FindEffect(target, 8, 230, e => e.Dodged);
        Assert.Equal(new[] { "dodge", "double", "counter" }, miss.Rolls.Select(r => r.Purpose));
        Assert.Null(miss.Reaction); Assert.Same(target, miss.AfterStats);
        var critical = FindEffect(target, 8, 230, e => e.Critical);
        Assert.Equal(4, critical.Damage); Assert.Equal(8, critical.AfterStats.HpCurrent);
    }

    [Theory]
    [InlineData("double")]
    [InlineData("counter")]
    [InlineData("lethal")]
    public void UnsupportedResolutionRejectsBeforeAnyExternalStateChanges(string boundary)
    {
        var state = AttackBoundary(); string frozen = JsonSerializer.Serialize(state); var target = state.Roster[0].Stats;
        bool found = false;
        for (uint seed = 0; seed <= ushort.MaxValue && !found; seed++)
        {
            try { Battle01EnemyPhysicalAttack.Resolve(boundary == "lethal" ? 100 : 8, target, 230, seed << 16 | 0x1234, 0, new(11, 14), new(11, 15)); }
            catch (Battle01PhysicalAttackUnsupportedException error) { found = error.ParamName == "attack." + boundary; }
        }
        Assert.True(found); Assert.Equal(frozen, JsonSerializer.Serialize(state));
    }

    [Fact]
    public void ScriptThreeAndSelectionRetainLethalityBranchClassCohortAndMovementTieOrder()
    {
        Assert.Equal(16, Battle01EnemyPhysicalAttack.Priority(8, 0, 0));
        Assert.Equal(1, Battle01EnemyPhysicalAttack.Priority(8, 9, 0));
        Assert.Equal(3, Battle01EnemyPhysicalAttack.Priority(8, 9, 1));
        var actor = AttackBoundary().Roster[0]; var roll = Battle01EnemyStandby.ThinkingRoll(0x0034, 3);
        Battle01PhysicalTargetPriority Entry(int index, byte classId, int cost, int priority) =>
            new(new(actor.Deployment with { CombatantIndex = index }, actor.Stats, classId, null), new(index, new(11, 14), cost),
                230, 3, 9, priority, roll);
        Assert.Equal(0, Battle01EnemyPhysicalAttack.SelectTarget([Entry(2, 0, 8, 3), Entry(0, 0, 8, 3)]).Target.Index);
        Assert.Equal(2, Battle01EnemyPhysicalAttack.SelectTarget([Entry(2, 0, 10, 3), Entry(0, 0, 8, 3)]).Target.Index);
        Assert.Equal(0, Battle01EnemyPhysicalAttack.SelectTarget([Entry(2, 1, 8, 16), Entry(1, 4, 8, 16), Entry(0, 0, 0, 16)]).Target.Index);
    }

    internal static Battle01InitializedState AttackBoundary()
    {
        var current = Battle01EnemyPursuitTests.RoundThree();
        // The earlier movement-only authored helper has no equipment. Supply the accepted combat
        // comparison profile explicitly; HP, effective modifiers, order and RNG remain identical.
        var roster = current.Roster.ToArray();
        roster[0] = roster[0].WithStats(new(1, 12, 12, 8, 8, 9, 4, 4, 6, 0,
            [199, 0, 127, 127], [10, 63, 63, 63]));
        current = Battle01FirstRoundTests.CopyCurrent(current, roster: roster);
        for (int round = 3; round <= 5; round++)
        {
            while (current.FirstRound!.CurrentCandidate is not null) current = Battle01EnemyPursuitTests.CompleteTurn(current);
            current = Battle01FirstRound.EnterNext(current);
        }
        for (int i = 0; i < 5; i++) current = Battle01EnemyPursuitTests.CompleteTurn(current);
        return current;
    }
    internal static Battle01InitializedState CompletedAttack() => Battle01EnemyPhysicalAttack.CompleteNext(AttackBoundary(), 132, Policy);
    internal static Battle01InitializedState CompletedBowie()
    {
        var ready = Battle01NextPlayerControl.Enter(CompletedAttack(), 0).State!;
        return Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(ready, 0), 0,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
    }
    private static Battle01PhysicalEffect FindEffect(Battle01Stats target, int attack, int multiplier, Func<Battle01PhysicalEffect, bool> match)
    {
        for (uint seed = 0; seed <= ushort.MaxValue; seed++)
        {
            try
            {
                var effect = Battle01EnemyPhysicalAttack.Resolve(attack, target, multiplier, seed << 16 | 0x1234, 0, new(11, 14), new(11, 15));
                if (match(effect)) return effect;
            }
            catch (Battle01PhysicalAttackUnsupportedException) { }
        }
        throw new InvalidOperationException("No source RNG seed matched the authored branch.");
    }
}

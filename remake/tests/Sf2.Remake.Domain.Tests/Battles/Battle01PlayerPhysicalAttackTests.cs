using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01PlayerPhysicalAttackTests
{
    internal static Battle01PlayerPhysicalCompletionPolicy Policy => Battle01PlayerPhysicalCompletionPolicy.ControlledNonlethalStrikeAndExp;
    internal static Battle01InitializedState Ready() => Battle01NextPlayerControl.Enter(
        Battle01EnemyPhysicalAttack.CompleteNext(Battle01EnemyPhysicalAttackTests.AttackBoundary(0), 132,
            Battle01EnemyPhysicalAttackTests.Policy), 0).State!;
    internal static Battle01InitializedState Selected() => Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(Ready(), 0), 0);
    internal static Battle01InitializedState Completed() => Battle01PlayerPhysicalAttack.Confirm(Selected(), 0, Policy);

    [Fact]
    public void ManualAttackCommitsReceipt52AndActualDispatchReachesRoundSevenPlayerTwo()
    {
        var selected = Selected(); string frozen = JsonSerializer.Serialize(selected);
        Assert.Equal(new[] { 132 }, selected.FirstControl!.Movement.Attack!.Targets);
        var after = Battle01PlayerPhysicalAttack.Confirm(selected, 0, Policy);
        var d = after.TurnCompletion!.PlayerPhysicalAttack!;
        Assert.Equal(new MapPosition(11, 15), d.RangeOrigin); Assert.Equal(new byte[] { 255 }, d.MoveString);
        Assert.Equal((0, 132, 0, 132, 1, 230), (d.ActorIndex, d.TargetIndex, (int)d.Action, (int)d.ItemOrSpellWord, (int)d.TargetTerrain, d.LandMultiplier));
        Assert.Equal(new ushort[] { 8, 16, 1, 1, 32, 32, 16, 16 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 7, 14, 0, 0, 12, 31, 15, 14 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0xE9EF1234, 0xE12A1234, 0x6F291234, 0xA51C1234, 0x62731234, 0xFFDE1234, 0xFE4D1234, 0xE9F01234 }, d.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((3, 2, 5, 2), (d.Effect.Damage, (int)d.Effect.TemporaryHp, (int)d.Effect.RestoredHp, (int)d.Effect.AfterStats.HpCurrent));
        Assert.Equal(new Battle01PhysicalReaction(132, -3, 0, 0, 1), d.Effect.Reaction);
        Assert.Equal((30, 15, 15, (byte?)15), (d.AccumulatedExp, d.HalvedExp, d.AwardedExp, after.Roster[0].Stats.CurrentExp));
        Assert.Equal(9, after.Roster[0].Stats.HpCurrent); Assert.Equal(0xE9F01234u, after.RandomSeedImage);
        Assert.Equal(selected.RandomSeedCopy, after.RandomSeedCopy); Assert.Same(selected.AiMemory, after.AiMemory);
        Assert.Same(selected.AiLastTargets, after.AiLastTargets); Assert.Equal(selected.NewlyTestedRegionMask, after.NewlyTestedRegionMask);
        Assert.Equal(52, Battle01EnemyPursuitTests.Receipts(after).Count()); Assert.Equal(14, after.FirstRound!.CurrentTurnOffset);
        Assert.Same(Policy, after.TurnCompletion.Policy); Assert.Null(after.FirstControl);
        after = Battle01EnemyPursuit.CompleteNext(after, 131, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        Assert.Equal(new MapPosition(11, 8), after.Roster[6].Position);
        Assert.Equal(new byte[] { 3, 3, 255 }, after.TurnCompletion!.EnemyPursuit!.MoveString);
        after = Battle01EnemyStandby.CompleteNext(after, 133, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        Assert.Equal(new MapPosition(7, 5), after.Roster[8].Position);
        Assert.Equal(new[] { 114, 19 }, after.TurnCompletion!.EnemyStandby!.Rolls.Select(r => r.GeneratorSteps));
        Assert.Equal((ushort?)0x0234, after.RandomSeedCopy);
        after = Battle01FirstRound.EnterNext(after);
        Assert.Equal(0xCF491234u, after.RandomSeedImage); Assert.Equal(7, after.NewlyTestedRegionMask);
        Assert.Equal(new byte[] { 2, 131, 132, 133, 0, 1, 129, 128, 130 }, after.FirstRound!.Slots.Take(9).Select(s => s.CombatantIndex));
        Assert.Equal(new byte[] { 6, 6, 6, 6, 5, 5, 5, 4, 4 }, after.FirstRound.Slots.Take(9).Select(s => s.AlteredAgility));
        Assert.Equal(64, after.FirstRound.Slots.Count); Assert.All(after.FirstRound.Slots.Skip(9), s => Assert.True(s.IsSentinel));
        var ready = Battle01NextPlayerControl.Enter(after, 2).State!;
        Assert.Equal((9, 2, (byte?)15), ((int)ready.Roster[0].Stats.HpCurrent, (int)ready.Roster[7].Stats.HpCurrent, ready.Roster[0].Stats.CurrentExp));
        Assert.Equal(14, ready.FirstControl!.Movement.Range.Budget); Assert.Equal(new MapPosition(7, 17), ready.Roster[2].Position);
        Assert.Equal(frozen, JsonSerializer.Serialize(selected));
    }

    [Fact]
    public void NestedCancelRetainsProvisionalTileThenRestoresOriginWithoutConsumingEffects()
    {
        var ready = Ready();
        var moved = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 0, new(12, 14)), 0);
        var selected = Battle01PlayerPhysicalAttack.Begin(moved, 0);
        var cycled = Battle01PlayerPhysicalAttack.Cycle(Battle01PlayerPhysicalAttack.Cycle(selected, 0, 1), 0, -1);
        var cancelled = Battle01PlayerPhysicalAttack.Cancel(cycled, 0);
        Assert.Equal(Battle01Phase.PlayerActionChoice, cancelled.Phase); Assert.Equal(new MapPosition(12, 14), cancelled.Roster[0].Position);
        var restored = Battle01PlayerMovement.Cancel(cancelled, 0);
        Assert.Equal(new MapPosition(11, 15), restored.Roster[0].Position);
        foreach (var state in new[] { moved, selected, cycled, cancelled, restored })
        {
            Assert.Equal(ready.RandomSeedImage, state.RandomSeedImage); Assert.Equal(ready.RandomSeedCopy, state.RandomSeedCopy);
            Assert.Same(ready.TurnCompletion, state.TurnCompletion); Assert.Same(ready.Roster[0].Stats, state.Roster[0].Stats);
            Assert.Same(ready.Roster[7].Stats, state.Roster[7].Stats);
        }
        Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(selected, 0, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats));
        Assert.Throws<ArgumentException>(() => Battle01PlayerMovement.Cancel(selected, 0));
    }

    [Theory]
    [InlineData("main")]
    [InlineData("copy")]
    [InlineData("actorHp")]
    [InlineData("targetHp")]
    [InlineData("otherEnemyHp")]
    [InlineData("exp")]
    [InlineData("expBefore")]
    [InlineData("award")]
    [InlineData("reaction")]
    [InlineData("policy")]
    [InlineData("mixed")]
    [InlineData("actor")]
    public void MixedHistoryRejectsForgedHpExpRngAndReceiptRoles(string field)
    {
        var after = Completed(); var receipt = after.TurnCompletion!; var d = receipt.PlayerPhysicalAttack!;
        var roster = after.Roster.ToArray(); uint main = after.RandomSeedImage; ushort copy = after.RandomSeedCopy!.Value;
        switch (field)
        {
            case "main": main ^= 0x10000; break;
            case "copy": copy ^= 0x100; break;
            case "actorHp": roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentHp(10)); break;
            case "targetHp": roster[7] = roster[7].WithStats(roster[7].Stats.WithCurrentHp(3)); break;
            case "otherEnemyHp": roster[3] = roster[3].WithStats(roster[3].Stats.WithCurrentHp(4)); break;
            case "exp": roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentExp(16)); break;
            case "expBefore": d = d with { Actor = d.Actor.WithStats(d.Actor.Stats.WithCurrentExp(1)) }; break;
            case "award": d = d with { AwardedExp = 14 }; break;
            case "reaction": d = d with { Effect = d.Effect with { Reaction = d.Effect.Reaction! with { TargetIndex = 131 } } }; break;
            case "policy": receipt = receipt with { Policy = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats }; break;
            case "mixed": receipt = receipt with { EnemyPhysicalAttack = receipt.Previous!.EnemyPhysicalAttack }; break;
            case "actor": receipt = receipt with { CompletedActorIndex = 1 }; break;
        }
        var forged = Battle01FirstRoundTests.CopyCurrent(after, roster: roster, mainImage: main, copy: copy,
            receipt: receipt with { PlayerPhysicalAttack = d });
        string frozen = JsonSerializer.Serialize(forged);
        Assert.ThrowsAny<ArgumentException>(() => Battle01EnemyPursuit.CompleteNext(forged, 131, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats));
        Assert.Equal(frozen, JsonSerializer.Serialize(forged));
    }

    [Fact]
    public void FailedFinalizationAfterLocalHpAndExpReplayPublishesNeitherChannel()
    {
        var selected = Selected(); string frozen = JsonSerializer.Serialize(selected);
        var d = Battle01PlayerPhysicalAttack.Decide(selected, 0); var roster = selected.Roster.ToArray();
        roster[0] = roster[0].WithStats(d.ActorAfterStats); roster[7] = roster[7].WithStats(d.Effect.AfterStats);
        var replayed = new Battle01InitializedState(selected, roster, d.Effect.MainSeedAfter ^ 0x10000);
        Assert.Equal("attack.history", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CompletePlayerPhysical(replayed, d, Policy)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(selected)); Assert.Equal((byte?)0, selected.Roster[0].Stats.CurrentExp);
    }

    [Fact]
    public void RealSeedsExerciseMissCriticalAndIndependentExpVariance()
    {
        Assert.Equal(0, Battle01PlayerPhysicalAttack.DamageExperience(0, 5));
        Assert.Equal(16, Battle01PlayerPhysicalAttack.DamageExperience(1, 3));
        Assert.Equal(49, Battle01PlayerPhysicalAttack.DamageExperience(5, 5));
        var state = Ready(); var actor = state.Roster[0].Stats; var target = state.Roster[7].Stats;
        var miss = Find(actor, target, r => r.Effect.Dodged);
        Assert.Equal(new ushort[] { 8, 32, 32, 16, 16 }, miss.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(1, miss.Award); Assert.Equal(0, miss.Accumulated); Assert.Null(miss.Effect.Reaction);
        var critical = Find(actor, target, r => r.Effect.Critical);
        Assert.Equal(3, critical.Effect.Damage); // prowess3 adds floor(3/4), which is zero.
        var plus = Find(actor, target, r => !r.Effect.Dodged && r.Award == 16);
        var minus = Find(actor, target, r => !r.Effect.Dodged && r.Award == 14);
        Assert.Equal((30, 15), (plus.Accumulated, plus.Halved)); Assert.Equal((30, 15), (minus.Accumulated, minus.Halved));
        Assert.Equal(0, plus.Effect.Rolls[^2].Result); Assert.Equal(0, minus.Effect.Rolls[^1].Result);
    }

    [Fact]
    public void LegalRingUsesLiveOpposingNonneutralPlacedUnitsAndSelectionWraps()
    {
        var ready = Ready(); var roster = ready.Roster.ToArray();
        roster[3] = roster[3].WithPosition(new(11, 16)); roster[4] = roster[4].WithPosition(new(12, 15));
        roster[5] = roster[5].WithPosition(new(10, 15));
        int[] Occupancy() { var cells = Enumerable.Repeat(-1, 2304).ToArray(); foreach (var u in roster) cells[u.Position.Y * 48 + u.Position.X] = u.Index; return cells; }
        // Independent authored occupancy arrangement for range/order only; no action is dispatched from it.
        var arranged = Battle01FirstRoundTests.CopyCurrent(ready, roster: roster, occupancy: Occupancy());
        Assert.Equal(new[] { 128, 129, 132, 130 }, Battle01PlayerPhysicalAttack.Targets(arranged, 0));
        roster[3] = roster[3].WithAiBitfield((ushort)(roster[3].AiBitfield!.Value | 8));
        var neutral = Battle01FirstRoundTests.CopyCurrent(ready, roster: roster, occupancy: Occupancy());
        Assert.Equal(new[] { 129, 132, 130 }, Battle01PlayerPhysicalAttack.Targets(neutral, 0));
        var action = Battle01PlayerMovement.Confirm(ready, 0);
        // Three legal targets retain strict source profiles and current HP; source ring ordering is explicit.
        roster[3] = roster[3].WithAiBitfield((ushort)(roster[3].AiBitfield!.Value & ~8));
        arranged = new Battle01InitializedState(action, roster, Array.AsReadOnly(Occupancy()), action.FirstControl!);
        var selected = Battle01PlayerPhysicalAttack.Begin(arranged, 0);
        Assert.Equal(128, selected.FirstControl!.Movement.Attack!.TargetIndex);
        selected = Battle01PlayerPhysicalAttack.Cycle(selected, 0, -1);
        Assert.Equal(130, selected.FirstControl!.Movement.Attack!.TargetIndex);
        selected = Battle01PlayerPhysicalAttack.Cycle(selected, 0, 1);
        Assert.Equal(128, selected.FirstControl!.Movement.Attack!.TargetIndex);
        var away = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 0, new(12, 15)), 0);
        Assert.Equal("attack.emptyTargets", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() => Battle01PlayerPhysicalAttack.Begin(away, 0)).ParamName);
    }

    [Theory]
    [InlineData("double")]
    [InlineData("counter")]
    [InlineData("lethal")]
    [InlineData("levelUp")]
    public void ActualUnsupportedBranchesRejectTheWholeResolution(string boundary)
    {
        var state = Ready(); string frozen = JsonSerializer.Serialize(state); var actor = state.Roster[0].Stats;
        var target = state.Roster[7].Stats;
        if (boundary == "levelUp") actor = actor.WithCurrentExp(99);
        if (boundary == "lethal") target = target.WithCurrentHp(1);
        bool found = false;
        for (uint seed = 0; seed <= ushort.MaxValue && !found; seed++)
            try { Battle01PlayerPhysicalAttack.Resolve(actor, target, 230, seed << 16 | 0x1234, 132, new(11, 15), new(11, 14)); }
            catch (Battle01PhysicalAttackUnsupportedException error) { found = error.ParamName == "attack." + boundary; }
        Assert.True(found); Assert.Equal(frozen, JsonSerializer.Serialize(state));
        Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() => Battle01PlayerPhysicalAttack.Resolve(
            Battle01EnemyPhysicalAttackTests.CompletedAttack().Roster[0].Stats, target, 230, 0xAF881234, 132, new(11, 15), new(11, 14)));
    }

    private static (Battle01PhysicalEffect Effect, int Accumulated, int Halved, int Award, Battle01Stats ActorAfter)
        Find(Battle01Stats actor, Battle01Stats target,
            Func<(Battle01PhysicalEffect Effect, int Accumulated, int Halved, int Award, Battle01Stats ActorAfter), bool> predicate)
    {
        for (uint seed = 0; seed <= ushort.MaxValue; seed++)
            try
            {
                var result = Battle01PlayerPhysicalAttack.Resolve(actor, target, 230, seed << 16 | 0x1234, 132, new(11, 15), new(11, 14));
                if (predicate(result)) return result;
            }
            catch (Battle01PhysicalAttackUnsupportedException) { }
        throw new InvalidOperationException("No real main seed exercised the branch.");
    }
}

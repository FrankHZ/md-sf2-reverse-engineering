using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01PlayerPhysicalAttackTests
{
    internal static Battle01PlayerPhysicalCompletionPolicy SecondPolicy => Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat;
    internal static Battle01InitializedState SecondDefeatSelected()
    {
        var current = Battle01NextPlayerControl.Enter(Battle01EnemyPhysicalAttackTests.AfterChesterPlayerRelay(), 1).State!;
        var stay = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, 1), 1, stay);
        current = Battle01EnemyStandby.CompleteNext(current, 130, stay);
        current = Battle01NextPlayerControl.Enter(current, 0).State!;
        return Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(current, 0), 0);
    }
    internal static Battle01InitializedState SecondDefeatCompleted() => Battle01PlayerPhysicalAttack.Confirm(SecondDefeatSelected(), 0, SecondPolicy);

    [Fact]
    public void SecondDefeatPreservesTheFirstCorpseAndCreditsOnlyTheNewTarget()
    {
        var selected = SecondDefeatSelected(); var oldCorpse = selected.Roster[7];
        var frozen = JsonSerializer.Serialize(selected, new JsonSerializerOptions { MaxDepth = 256 });
        var after = Battle01PlayerPhysicalAttack.Confirm(selected, 0, SecondPolicy);
        var receipt = after.TurnCompletion!; var d = receipt.PlayerPhysicalAttack!;
        Assert.Same(selected.TurnCompletion, receipt.Previous); Assert.Same(SecondPolicy, receipt.Policy);
        Assert.Equal(79, Battle01EnemyPursuitTests.Receipts(after).Count());
        // This existing authored grid uses plains here; the required real Content route uses Low Sky.
        Assert.Equal((0, 131, 1, 230), (d.ActorIndex, d.TargetIndex, (int)d.TargetTerrain, d.LandMultiplier));
        Assert.Equal(new[] { 131 }, d.LegalTargets); Assert.Equal(new byte[] { 255 }, d.MoveString);
        Assert.Equal(new MapPosition(11, 15), d.RangeOrigin); Assert.Equal(new MapPosition(10, 15), d.Target.Position);
        Assert.Equal(new ushort[] { 8, 16, 1, 1, 16, 16 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 7, 13, 0, 0, 9, 5 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0xE8CC1234, 0xD2631234, 0xAF0E1234, 0xE3BD1234, 0x90A01234, 0x58271234 },
            d.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((3, 0, 3, 0, 49, 24, 24), (d.Effect.Damage, (int)d.Effect.TemporaryHp, (int)d.Effect.RestoredHp,
            (int)d.Effect.AfterStats.HpCurrent, d.AccumulatedExp, d.HalvedExp, d.AwardedExp));
        Assert.Equal(0, d.Target.Stats.HpCurrent - d.Effect.Damage);
        Assert.Equal(new Battle01PhysicalReaction(131, -3, 0, 0, 1), d.Effect.Reaction);
        Assert.False(d.Effect.Dodged); Assert.False(d.Effect.Critical); Assert.True(d.DefeatedTarget);
        Assert.Equal(((uint?)120, (byte?)63, (ushort?)2, (byte?)10),
            (after.CurrentGold, after.Roster[0].Stats.CurrentExp, after.Roster[0].Stats.CurrentKills, after.Roster[2].Stats.CurrentExp));
        Assert.Null(after.Roster[2].Stats.CurrentKills); Assert.Equal(3, after.Roster[0].Stats.HpCurrent);
        Assert.Same(oldCorpse, after.Roster[7]); Assert.Null(after.Roster[6].Position); Assert.Null(after.Roster[7].Position);
        Assert.Equal(-1, after.OccupantAt(new(10, 15))); Assert.Equal(7, after.Occupancy.Count(id => id >= 0));
        Assert.Equal(new[] { 131 }, receipt.EnemyDefeat!.FirstWorklist); Assert.Empty(receipt.EnemyDefeat.AfterTurnWorklist);
        Assert.Equal((0, (ushort)1, (ushort)2), (receipt.EnemyDefeat.CreditedAlly, receipt.EnemyDefeat.KillsBefore, receipt.EnemyDefeat.KillsAfter));
        Assert.Equal(new Battle01FactionCounts(3, 4), receipt.BeforeAfterTurn); Assert.Equal(receipt.BeforeAfterTurn, receipt.AfterAfterTurn);
        Assert.Equal((0x58271234u, (ushort?)0x0034, (byte)16), (after.RandomSeedImage, after.RandomSeedCopy, after.FirstRound!.CurrentTurnOffset));
        Assert.Null(after.FirstRound.CurrentCandidate); Assert.Same(selected.FirstRound!.Slots, after.FirstRound.Slots);
        Assert.Same(selected.AiMemory, after.AiMemory); Assert.Same(selected.AiLastTargets, after.AiLastTargets);
        Battle01PlayerPhysicalAttack.RequireAccountingInputs(after, 0, 0, 0);
        Assert.Equal(frozen, JsonSerializer.Serialize(selected, new JsonSerializerOptions { MaxDepth = 256 }));
    }

    [Fact]
    public void HoveringTerrainZeroUsesUnreducedDamageAndTheSameLethalEarlyReturn()
    {
        var selected = SecondDefeatSelected(); var actor = selected.Roster[0]; var target = selected.Roster[6];
        var (effect, accumulated, halved, award, after) = Battle01PlayerPhysicalAttack.Resolve(actor.Stats, target.Stats,
            Battle01PlayerPhysicalAttack.TargetLandMultiplier(0), selected.RandomSeedImage, 131,
            actor.RequirePosition(), target.RequirePosition(), allowDefeat: true);
        Assert.Equal((4, -1, 0, 3, 0, 49, 24, 24, (byte?)63), (effect.Damage,
            target.Stats.HpCurrent - effect.Damage, (int)effect.TemporaryHp, (int)effect.RestoredHp,
            (int)effect.AfterStats.HpCurrent, accumulated, halved, award, after.CurrentExp));
        Assert.Equal(new Battle01PhysicalReaction(131, -4, 0, 0, 1), effect.Reaction);
        Assert.Equal(new ushort[] { 8, 16, 1, 1, 16, 16 }, effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 7, 13, 0, 0, 9, 5 }, effect.Rolls.Select(r => r.Result));
        Assert.Equal(0x58271234u, effect.MainSeedAfter); Assert.Equal(3, target.Stats.HpCurrent);
    }

    [Fact]
    public void EarlierPoliciesKeepTheirLimitsAndTheSecondPolicyDoesNotAdmitChester()
    {
        var selected = SecondDefeatSelected();
        Assert.Equal("attack.additionalDefeat", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01PlayerPhysicalAttack.Confirm(selected, 0, Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat)).ParamName);
        Assert.Equal("attack.lethal", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01PlayerPhysicalAttack.Confirm(selected, 0, Policy)).ParamName);
        Assert.Equal("policy", Assert.Throws<ArgumentException>(() => Battle01PlayerPhysicalAttack.Confirm(ChesterSelected(), 2, SecondPolicy)).ParamName);
        Assert.Equal(256, Battle01PlayerPhysicalAttack.TargetLandMultiplier(0));
        Assert.Equal(230, Battle01PlayerPhysicalAttack.TargetLandMultiplier(1));
        foreach (byte terrain in new byte[] { 2, 3, 6, 7, 8, 255 })
            Assert.Equal("attack.targetTerrain", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
                Battle01PlayerPhysicalAttack.TargetLandMultiplier(terrain)).ParamName);
        Assert.Equal("battle01-controlled-player-first-defeat-exp-gold-kills-v1", Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat.Id);
    }

    [Theory]
    [InlineData("oldCorpsePosition")]
    [InlineData("oldCorpseHp")]
    [InlineData("newCellOccupied")]
    [InlineData("repeatAward")]
    [InlineData("firstWorklist")]
    [InlineData("bothCorpsesWorklist")]
    [InlineData("afterWorklist")]
    [InlineData("oldPolicy")]
    [InlineData("priorCleanup")]
    [InlineData("priorPolicy")]
    [InlineData("kills")]
    [InlineData("exp")]
    [InlineData("main")]
    [InlineData("copy")]
    [InlineData("firstCount")]
    [InlineData("afterCount")]
    public void SecondDefeatHistoryRejectsIndependentCorpseAwardAndPolicyForgery(string field)
    {
        var after = SecondDefeatCompleted(); var r = after.TurnCompletion!; var d = r.PlayerPhysicalAttack!;
        var roster = after.Roster.ToArray(); var occupancy = after.Occupancy.ToArray(); uint main = after.RandomSeedImage;
        ushort copy = after.RandomSeedCopy!.Value; uint gold = after.CurrentGold!.Value;
        switch (field)
        {
            case "oldCorpsePosition": roster[7] = roster[7].WithPosition(new(12, 14)); break;
            case "oldCorpseHp": roster[7] = roster[7].WithStats(roster[7].Stats.WithCurrentHp(1)); break;
            case "newCellOccupied": occupancy[15 * 48 + 10] = 131; break;
            case "repeatAward": gold += 60; break;
            case "firstWorklist": r = r with { EnemyDefeat = r.EnemyDefeat! with { FirstWorklist = new[] { 132 } } }; break;
            case "bothCorpsesWorklist": r = r with { EnemyDefeat = r.EnemyDefeat! with { FirstWorklist = new[] { 131, 132 } } }; break;
            case "afterWorklist": r = r with { EnemyDefeat = r.EnemyDefeat! with { AfterTurnWorklist = new[] { 131 } } }; break;
            case "oldPolicy": r = r with { Policy = Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat }; break;
            case "priorCleanup": r = r with { Previous = ChangePriorDefeat(r.Previous!, old => old with { EnemyDefeat = null }) }; break;
            case "priorPolicy": r = r with { Previous = ChangePriorDefeat(r.Previous!, old => old with { Policy = SecondPolicy }) }; break;
            case "kills": roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentKills(3)); break;
            case "exp": roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentExp(87)); break;
            case "main": main ^= 0x10000; break;
            case "copy": copy ^= 0x100; break;
            case "firstCount": r = r with { BeforeAfterTurn = new(3, 5) }; break;
            case "afterCount": r = r with { AfterAfterTurn = new(3, 5) }; break;
        }
        var forged = Battle01FirstRoundTests.CopyCurrent(after, roster: roster, occupancy: occupancy, mainImage: main, copy: copy, gold: gold, receipt: r);
        Assert.ThrowsAny<ArgumentException>(() => Battle01FirstRound.EnterNext(forged));
        Assert.Equal((uint?)120, after.CurrentGold); Assert.Equal((ushort?)2, after.Roster[0].Stats.CurrentKills);
        Assert.Null(after.Roster[7].Position); Assert.Null(after.Roster[6].Position);
    }

    private static Battle01TurnCompletionReceipt ChangePriorDefeat(Battle01TurnCompletionReceipt receipt,
        Func<Battle01TurnCompletionReceipt, Battle01TurnCompletionReceipt> change) => receipt.EnemyDefeat is not null
            ? change(receipt) : receipt with { Previous = ChangePriorDefeat(receipt.Previous!, change) };

    internal static Battle01PlayerPhysicalCompletionPolicy Policy => Battle01PlayerPhysicalCompletionPolicy.ControlledNonlethalStrikeAndExp;

    [Fact]
    public void ChesterRequiresKnownExpAndHistoryRejectsItsLateInjection()
    {
        var target = Battle01EnemyPhysicalAttackTests.ChesterAttackBoundary().Roster[2];
        Assert.Equal("battle01-class1-wooden-stick-effective-prowess3-v1", Battle01EnemyPhysicalAttack.RequireTargetProfile(target));
        Assert.Equal("attack.targetProfile", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01PlayerPhysicalAttack.RequireActor(target)).ParamName);
        Battle01PlayerPhysicalAttack.RequireActor(target.WithStats(target.Stats.WithCurrentExp(0)));
        var after = Battle01FirstRound.EnterNext(Battle01EnemyPhysicalAttackTests.ChesterHitCompleted());
        var ready = Battle01NextPlayerControl.Enter(after, 2).State!;
        var provisional = Battle01PlayerMovement.Confirm(ready, 2);
        Assert.Equal("attack.targetProfile", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01PlayerPhysicalAttack.Begin(provisional, 2)).ParamName);
        Assert.Same(after.TurnCompletion, provisional.TurnCompletion);
        var roster = provisional.Roster.ToArray();
        roster[2] = roster[2].WithStats(roster[2].Stats.WithCurrentExp(0));
        var injected = Battle01FirstRoundTests.CopyCurrent(provisional, roster: roster);
        Assert.ThrowsAny<ArgumentException>(() => Battle01PlayerPhysicalAttack.Begin(injected, 2));
    }
    internal static Battle01InitializedState Ready() => Battle01NextPlayerControl.Enter(
        Battle01EnemyPhysicalAttack.CompleteNext(Battle01EnemyPhysicalAttackTests.AttackBoundary(0), 132,
            Battle01EnemyPhysicalAttackTests.Policy), 0).State!;
    internal static Battle01InitializedState Selected() => Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(Ready(), 0), 0);
    internal static Battle01InitializedState Completed() => Battle01PlayerPhysicalAttack.Confirm(Selected(), 0, Policy);

    internal static Battle01InitializedState ChesterReady() => Battle01NextPlayerControl.Enter(
        Battle01FirstRound.EnterNext(Battle01EnemyPhysicalAttackTests.ChesterHitCompleted(0)), 2).State!;
    internal static Battle01InitializedState ChesterSelected() =>
        Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(ChesterReady(), 2), 2);
    internal static Battle01InitializedState ChesterCompleted() =>
        Battle01PlayerPhysicalAttack.Confirm(ChesterSelected(), 2, Policy);

    [Theory]
    [InlineData("level")]
    [InlineData("maxHp")]
    [InlineData("mp")]
    [InlineData("attack")]
    [InlineData("defense")]
    [InlineData("agility")]
    [InlineData("move")]
    [InlineData("status")]
    [InlineData("item")]
    [InlineData("spell")]
    [InlineData("exp")]
    [InlineData("kills")]
    public void KnownChesterExpDoesNotWidenHisOtherCombatProfileInputs(string field)
    {
        var actor = ChesterReady().Roster[2]; var s = actor.Stats;
        var stats = new Battle01Stats(field == "level" ? (byte)2 : s.Level, field == "maxHp" ? (ushort)12 : s.HpMax,
            s.HpCurrent, field == "mp" ? (byte)1 : s.MpMax, s.MpCurrent, field == "attack" ? (byte)9 : s.Attack,
            field == "defense" ? (byte)4 : s.Defense, field == "agility" ? (byte)6 : s.Agility,
            field == "move" ? (byte)6 : s.Move, field == "status" ? (ushort)1 : s.Status,
            field == "item" ? new ushort[] {199,0,127,127} : s.Items,
            field == "spell" ? new byte[] {10,63,63,63} : s.Spells,
            field == "exp" ? (byte)100 : (byte)0, field == "kills" ? (ushort)0 : null);
        Assert.Equal("attack.targetProfile", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01PlayerPhysicalAttack.RequireActor(actor.WithStats(stats))).ParamName);
    }

    [Fact]
    public void ChesterDoesNotAdmitLevelTransitionOrAnotherLethalStrike()
    {
        var ready = ChesterReady(); var actor = ready.Roster[2]; var target = ready.Roster[6];
        var highExp = actor.Stats.WithCurrentExp(99);
        Battle01PlayerPhysicalAttack.RequireActor(actor.WithStats(highExp));
        Assert.Equal("attack.levelUp", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01PlayerPhysicalAttack.Resolve(highExp, target.Stats, 230, ready.RandomSeedImage, target.Index,
                actor.RequirePosition(), target.RequirePosition())).ParamName);
        Assert.Equal("attack.lethal", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01PlayerPhysicalAttack.Resolve(actor.Stats, target.Stats.WithCurrentHp(1), 230, ready.RandomSeedImage,
                target.Index, actor.RequirePosition(), target.RequirePosition())).ParamName);
        Assert.Null(actor.Stats.CurrentKills); Assert.Equal((byte?)0, actor.Stats.CurrentExp);
    }

    [Fact]
    public void ChesterAttackUsesHisActualProfileAndEarnsExpThroughReceipt72()
    {
        var ready = ChesterReady(); var frozen = JsonSerializer.Serialize(ready, new JsonSerializerOptions { MaxDepth = 256 });
        var choice = Battle01PlayerMovement.Confirm(ready, 2);
        var selected = Battle01PlayerPhysicalAttack.Begin(choice, 2);
        var cycled = Battle01PlayerPhysicalAttack.Cycle(Battle01PlayerPhysicalAttack.Cycle(selected, 2, 1), 2, -1);
        var cancelled = Battle01PlayerPhysicalAttack.Cancel(cycled, 2);
        Assert.Equal(new[] { 131 }, cycled.FirstControl!.Movement.Attack!.Targets);
        Assert.Equal(Battle01Phase.PlayerActionChoice, cancelled.Phase);
        foreach (var state in new[] { choice, selected, cycled, cancelled })
        {
            Assert.Same(ready.TurnCompletion, state.TurnCompletion);
            Assert.Same(ready.Roster[2].Stats, state.Roster[2].Stats);
            Assert.Same(ready.Roster[6].Stats, state.Roster[6].Stats);
            Assert.Equal((0x71D31234u, (ushort?)0x0134), (state.RandomSeedImage, state.RandomSeedCopy));
        }
        var after = Battle01PlayerPhysicalAttack.Confirm(Battle01PlayerPhysicalAttack.Begin(cancelled, 2), 2, Policy);
        var receipt = after.TurnCompletion!; var d = receipt.PlayerPhysicalAttack!;
        Assert.Equal("battle01-class1-wooden-stick-effective-prowess3-v1", d.CombatProfile);
        Assert.Equal((2, 131, 0, 131, 1, 230), (d.ActorIndex, d.TargetIndex, (int)d.Action,
            (int)d.ItemOrSpellWord, (int)d.TargetTerrain, d.LandMultiplier));
        Assert.Equal(new MapPosition(11, 14), d.RangeOrigin); Assert.Equal(d.RangeOrigin, d.MovementOrigin);
        Assert.Equal(new byte[] { 255 }, d.MoveString);
        Assert.Equal(new ushort[] { 8, 16, 1, 1, 32, 32, 16, 16 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 6, 2, 0, 0, 24, 1, 8, 10 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0xC7BE1234, 0x24AD1234, 0xDCD01234, 0x36971234, 0xC5B21234,
            0x0A111234, 0x82E41234, 0xA59B1234 }, d.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((2, 3, 5, 3), (d.Effect.Damage, (int)d.Effect.TemporaryHp, (int)d.Effect.RestoredHp,
            (int)after.Roster[6].Stats.HpCurrent));
        Assert.Equal(new Battle01PhysicalReaction(131, -2, 0, 0, 1), d.Effect.Reaction);
        Assert.False(d.Effect.Dodged); Assert.False(d.Effect.Critical); Assert.False(d.DefeatedTarget);
        Assert.Equal((20, 10, 10, (byte?)0, (byte?)10), (d.AccumulatedExp, d.HalvedExp, d.AwardedExp,
            d.Actor.Stats.CurrentExp, after.Roster[2].Stats.CurrentExp));
        Assert.Equal(9, after.Roster[2].Stats.HpCurrent); Assert.Null(after.Roster[2].Stats.CurrentKills);
        Assert.Equal((72, (byte)2, (byte?)128), (Battle01EnemyPursuitTests.Receipts(after).Count(),
            after.FirstRound!.CurrentTurnOffset, after.FirstRound.CurrentCandidate?.CombatantIndex));
        Assert.Same(ready.FirstRound!.Slots, after.FirstRound.Slots); Assert.Same(ready.TurnCompletion, receipt.Previous);
        Assert.Equal(new Battle01FactionCounts(3, 5), receipt.BeforeAfterTurn); Assert.Equal(receipt.BeforeAfterTurn, receipt.AfterAfterTurn);
        Assert.Null(receipt.EnemyDefeat); Assert.Same(Policy, receipt.Policy);
        Assert.Same(ready.AiMemory, after.AiMemory); Assert.Same(ready.AiLastTargets, after.AiLastTargets);
        Assert.Equal(((uint?)60, (byte?)39, (ushort?)1), (after.CurrentGold, after.Roster[0].Stats.CurrentExp, after.Roster[0].Stats.CurrentKills));
        Assert.Null(after.Roster[7].Position); Assert.Equal(8, after.Occupancy.Count(id => id >= 0));
        Battle01PlayerPhysicalAttack.RequireAccountingInputs(after, 0, 0, 0);
        Assert.Equal("accounting.input", Assert.Throws<ArgumentException>(() =>
            Battle01PlayerPhysicalAttack.RequireAccountingInputs(after, 0, 0, null)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(ready, new JsonSerializerOptions { MaxDepth = 256 }));
    }

    [Theory]
    [InlineData("exp")]
    [InlineData("hp")]
    [InlineData("main")]
    public void ChesterLateFinalizationRejectsLocallyConstructedFalseEffects(string field)
    {
        var selected = ChesterSelected(); var d = Battle01PlayerPhysicalAttack.Decide(selected, 2);
        var roster = selected.Roster.ToArray();
        roster[2] = roster[2].WithStats(field == "exp" ? d.ActorAfterStats.WithCurrentExp(11) : d.ActorAfterStats);
        roster[6] = roster[6].WithStats(field == "hp" ? d.Effect.AfterStats.WithCurrentHp(4) : d.Effect.AfterStats);
        var local = new Battle01InitializedState(selected, roster,
            d.Effect.MainSeedAfter ^ (field == "main" ? 0x10000u : 0u), d.GoldAfter);
        // Actor EXP is rejected by the finalizer's stat refresh before receipt history is checked.
        Assert.Equal(field == "exp" ? "stats" : "attack.history", Assert.Throws<ArgumentException>(() =>
            Battle01TurnCompletion.CompletePlayerPhysical(local, d, Policy)).ParamName);
        Assert.Equal((0x71D31234u, (byte?)0, 5, 71), (selected.RandomSeedImage, selected.Roster[2].Stats.CurrentExp,
            (int)selected.Roster[6].Stats.HpCurrent, Battle01EnemyPursuitTests.Receipts(selected).Count()));
    }

    [Theory]
    [InlineData("exp")]
    [InlineData("hp")]
    [InlineData("main")]
    [InlineData("copy")]
    [InlineData("gold")]
    [InlineData("kills")]
    [InlineData("actor")]
    [InlineData("award")]
    [InlineData("reaction")]
    [InlineData("roll")]
    public void ChesterReceiptHistoryRejectsEachForgedEffectChannel(string field)
    {
        var after = ChesterCompleted(); var roster = after.Roster.ToArray(); var receipt = after.TurnCompletion!;
        var d = receipt.PlayerPhysicalAttack!; uint main = after.RandomSeedImage, gold = 60;
        ushort copy = after.RandomSeedCopy!.Value;
        switch (field)
        {
            case "exp": roster[2] = roster[2].WithStats(roster[2].Stats.WithCurrentExp(11)); break;
            case "hp": roster[6] = roster[6].WithStats(roster[6].Stats.WithCurrentHp(4)); break;
            case "main": main ^= 0x10000; break;
            case "copy": copy ^= 0x100; break;
            case "gold": gold++; break;
            case "kills": roster[2] = roster[2].WithStats(roster[2].Stats.WithCurrentKills(0)); break;
            case "actor": d = d with { Actor = d.Actor.WithStats(d.Actor.Stats.WithCurrentExp(1)) }; break;
            case "award": d = d with { AwardedExp = 11 }; break;
            case "reaction": d = d with { Effect = d.Effect with { Reaction = d.Effect.Reaction! with { HpDelta = -3 } } }; break;
            case "roll":
                var rolls = d.Effect.Rolls.ToArray(); rolls[0] = rolls[0] with { Result = 7 };
                d = d with { Effect = d.Effect with { Rolls = rolls } }; break;
        }
        var forged = Battle01FirstRoundTests.CopyCurrent(after, roster: roster, mainImage: main, copy: copy,
            gold: gold, receipt: receipt with { PlayerPhysicalAttack = d });
        Assert.ThrowsAny<ArgumentException>(() => Battle01EnemyStandby.CompleteNext(forged, 128,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats));
        Assert.Equal((byte?)10, after.Roster[2].Stats.CurrentExp); Assert.Equal(3, after.Roster[6].Stats.HpCurrent);
    }

    internal static Battle01InitializedState FirstDefeatSelected(bool accounting = true, byte? chesterExp = null)
    {
        var stay = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;
        var current = Battle01EnemyPhysicalAttack.CompleteNext(
            Battle01EnemyPhysicalAttackTests.AttackBoundary(0, firstDefeatAccounting: accounting, chesterExp), 132, Battle01EnemyPhysicalAttackTests.Policy);
        current = Battle01NextPlayerControl.Enter(current, 0).State!;
        current = Battle01PlayerPhysicalAttack.Confirm(Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(current, 0), 0), 0,
            Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat);
        current = Battle01EnemyPursuit.CompleteNext(current, 131, stay);
        current = Battle01EnemyStandby.CompleteNext(current, 133, stay);
        current = Battle01NextPlayerControl.Enter(Battle01FirstRound.EnterNext(current), 2).State!;
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, 2), 2, stay);
        current = Battle01EnemyPursuit.CompleteNext(current, 131, stay);
        current = Battle01EnemyPhysicalAttack.CompleteNext(current, 132, Battle01EnemyPhysicalAttackTests.Policy);
        Assert.Equal(new[] { accounting ? 2 : 3, 3 }, current.TurnCompletion!.EnemyPhysicalAttack!.Priorities.Select(p => p.PotentialDamage));
        current = Battle01EnemyStandby.CompleteNext(current, 133, stay);
        current = Battle01NextPlayerControl.Enter(current, 0).State!;
        return Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(current, 0), 0);
    }

    internal static Battle01InitializedState FirstDefeatCompleted(byte? chesterExp = null) => Battle01PlayerPhysicalAttack.Confirm(FirstDefeatSelected(chesterExp: chesterExp), 0,
        Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat);

    [Fact]
    public void FirstEnemyDefeatCommitsOrderedAwardsCleanupAndActualPlayerOneControl()
    {
        var selected = FirstDefeatSelected(); string frozen = JsonSerializer.Serialize(selected);
        Assert.Equal((0x18571234u, (ushort?)0x0234, 58, (byte)8), (selected.RandomSeedImage, selected.RandomSeedCopy,
            Battle01EnemyPursuitTests.Receipts(selected).Count(), selected.FirstRound!.CurrentTurnOffset));
        Assert.Equal((6, 2, (byte?)15, (uint?)0, (ushort?)0), ((int)selected.Roster[0].Stats.HpCurrent,
            (int)selected.Roster[7].Stats.HpCurrent, selected.Roster[0].Stats.CurrentExp, selected.CurrentGold, selected.Roster[0].Stats.CurrentKills));
        Assert.Equal("attack.lethal", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01PlayerPhysicalAttack.Confirm(selected, 0, Policy)).ParamName);
        var after = Battle01PlayerPhysicalAttack.Confirm(selected, 0, Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat);
        var d = after.TurnCompletion!.PlayerPhysicalAttack!; var cleanup = after.TurnCompletion.EnemyDefeat!;
        Assert.Equal(new ushort[] { 8, 16, 1, 1, 16, 16 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 1, 1, 0, 0, 14, 15 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0x3C721234, 0x11D11234, 0xE7A41234, 0xC35B1234, 0xEBA61234, 0xF7751234 },
            d.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((3, 0, 2, 0), (d.Effect.Damage, (int)d.Effect.TemporaryHp, (int)d.Effect.RestoredHp, (int)d.Effect.AfterStats.HpCurrent));
        Assert.Equal(new Battle01PhysicalReaction(132, -3, 0, 0, 1), d.Effect.Reaction);
        Assert.Equal((49, 24, 24, (byte?)39), (d.AccumulatedExp, d.HalvedExp, d.AwardedExp, after.Roster[0].Stats.CurrentExp));
        Assert.Equal(((uint?)0, (uint?)60, (uint?)60), (d.GoldBefore, d.GoldAfter, after.CurrentGold));
        Assert.Equal((0, 0, 1), (cleanup.CreditedAlly, (int)cleanup.KillsBefore, (int)cleanup.KillsAfter));
        Assert.Equal((ushort?)1, after.Roster[0].Stats.CurrentKills);
        Assert.Equal(new[] { 132 }, cleanup.FirstWorklist); Assert.Empty(cleanup.AfterTurnWorklist);
        Assert.Equal(new Battle01FactionCounts(3, 5), after.TurnCompletion.BeforeAfterTurn);
        Assert.Equal(after.TurnCompletion.BeforeAfterTurn, after.TurnCompletion.AfterAfterTurn);
        Assert.Equal(9, after.Roster.Count); Assert.Null(after.Roster[7].Position); Assert.Equal(-1, after.OccupantAt(new(11, 14)));
        Assert.Same(selected.Roster[7].Deployment, after.Roster[7].Deployment); Assert.Equal(5, after.Roster[7].EnemySource!.SourceStats.HpCurrent);
        Assert.Null(after.Roster[7].WithAiBitfield(after.Roster[7].AiBitfield!.Value).WithStats(after.Roster[7].Stats.WithCurrentHp(0)).Position);
        Assert.Same(selected.FirstRound.Slots, after.FirstRound!.Slots);
        Assert.Equal((59, (byte)10, 1), (Battle01EnemyPursuitTests.Receipts(after).Count(), after.FirstRound.CurrentTurnOffset,
            (int)after.FirstRound.CurrentCandidate!.Value.CombatantIndex));
        var ready = Battle01NextPlayerControl.Enter(after, 1).State!;
        Assert.Equal((1, 10, 11), (ready.FirstControl!.ActorIndex, ready.FirstControl.Movement.Range.Budget, (int)ready.Roster[1].Stats.HpCurrent));
        Assert.Equal(new MapPosition(9, 17), ready.Roster[1].Position);
        var moved = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 1, new(10, 17)), 1);
        var cancelled = Battle01PlayerMovement.Cancel(moved, 1);
        Assert.Equal(ready.Roster[1].Position, cancelled.Roster[1].Position);
        Assert.Same(after.TurnCompletion, cancelled.TurnCompletion);
        var stayed = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(cancelled, 1), 1,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        Assert.Equal(after.CurrentGold, stayed.CurrentGold); Assert.Equal((ushort?)1, stayed.Roster[0].Stats.CurrentKills);
        Assert.Equal(0xF7751234u, stayed.RandomSeedImage); Assert.Equal((ushort?)0x0234, stayed.RandomSeedCopy);
        Assert.Null(stayed.Roster[7].Position); Assert.Equal(frozen, JsonSerializer.Serialize(selected));
    }

    [Fact]
    public void FirstDefeatLateFinalizationFailureRetainsAllSelectedSnapshotChannels()
    {
        var selected = FirstDefeatSelected(); string frozen = JsonSerializer.Serialize(selected);
        var decision = Battle01PlayerPhysicalAttack.Decide(selected, 0, allowDefeat: true);
        var roster = selected.Roster.ToArray(); roster[0] = roster[0].WithStats(decision.ActorAfterStats);
        roster[7] = roster[7].WithStats(decision.Effect.AfterStats);
        var replayed = new Battle01InitializedState(selected, roster, decision.Effect.MainSeedAfter ^ 0x10000, decision.GoldAfter);
        Assert.Equal("attack.history", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CompletePlayerPhysical(
            replayed, decision, Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(selected));
    }

    [Theory]
    [InlineData("gold")]
    [InlineData("goldBefore")]
    [InlineData("kills")]
    [InlineData("killsBefore")]
    [InlineData("killsAfter")]
    [InlineData("credit")]
    [InlineData("firstList")]
    [InlineData("duplicateList")]
    [InlineData("secondList")]
    [InlineData("missingCleanup")]
    [InlineData("oldPolicy")]
    [InlineData("position")]
    [InlineData("occupancy")]
    [InlineData("hp")]
    [InlineData("exp")]
    [InlineData("main")]
    [InlineData("copy")]
    [InlineData("firstCount")]
    [InlineData("secondCount")]
    public void FirstDefeatHistoryRejectsIndependentAccountingCleanupAndPlacementForgery(string field)
    {
        var after = FirstDefeatCompleted(); var receipt = after.TurnCompletion!; var d = receipt.PlayerPhysicalAttack!;
        var cleanup = receipt.EnemyDefeat!; var roster = after.Roster.ToArray(); var occupancy = after.Occupancy.ToArray();
        uint gold = after.CurrentGold!.Value, main = after.RandomSeedImage; ushort copy = after.RandomSeedCopy!.Value;
        switch (field)
        {
            case "gold": gold++; break;
            case "goldBefore": d = d with { GoldBefore = 1 }; break;
            case "kills": roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentKills(2)); break;
            case "killsBefore": cleanup = cleanup with { KillsBefore = 1 }; break;
            case "killsAfter": cleanup = cleanup with { KillsAfter = 2 }; break;
            case "credit": cleanup = cleanup with { CreditedAlly = 1 }; break;
            case "firstList": cleanup = cleanup with { FirstWorklist = new[] { 131 } }; break;
            case "duplicateList": cleanup = cleanup with { FirstWorklist = new[] { 132, 132 } }; break;
            case "secondList": cleanup = cleanup with { AfterTurnWorklist = new[] { 132 } }; break;
            case "missingCleanup": cleanup = null; break;
            case "oldPolicy": receipt = receipt with { Policy = Policy }; break;
            case "position": roster[7] = roster[7].WithPosition(d.Target.Position); break;
            case "occupancy": occupancy[11 + 14 * 48] = 132; break;
            case "hp": roster[7] = roster[7].WithStats(roster[7].Stats.WithCurrentHp(1)); break;
            case "exp": roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentExp(40)); break;
            case "main": main ^= 0x10000; break;
            case "copy": copy ^= 0x100; break;
            case "firstCount": receipt = receipt with { BeforeAfterTurn = new(3, 6) }; break;
            case "secondCount": receipt = receipt with { AfterAfterTurn = new(3, 6) }; break;
        }
        var forged = Battle01FirstRoundTests.CopyCurrent(after, roster: roster, occupancy: occupancy, mainImage: main,
            copy: copy, gold: gold, receipt: receipt with { PlayerPhysicalAttack = d, EnemyDefeat = cleanup });
        string frozen = JsonSerializer.Serialize(forged);
        Assert.ThrowsAny<ArgumentException>(() => Battle01NextPlayerControl.Enter(forged, 1));
        Assert.Equal(frozen, JsonSerializer.Serialize(forged));
    }

    [Fact]
    public void FollowingGenerationExcludesTheCleanedEnemyWithoutRepeatingItsReward()
    {
        var after = FirstDefeatCompleted();
        var current = Battle01NextPlayerControl.Enter(after, 1).State!;
        var stay = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, 1), 1, stay);
        foreach (int actor in new[] { 129, 128, 130 })
        {
            Assert.Equal(actor, current.FirstRound!.CurrentCandidate!.Value.CombatantIndex);
            current = Battle01EnemyStandby.CompleteNext(current, actor, stay);
        }
        Assert.Null(current.FirstRound!.CurrentCandidate);
        var generated = Battle01FirstRound.EnterNext(current);
        Assert.Equal(8, generated.FirstRound!.RoundNumber);
        Assert.Equal(new byte[] { 0, 1, 2, 128, 129, 130, 131, 133 },
            generated.FirstRound.Slots.Where(slot => !slot.IsSentinel).Select(slot => slot.CombatantIndex).Order());
        Assert.Equal(56, generated.FirstRound.Slots.Count(slot => slot.IsSentinel));
        Assert.Null(generated.Roster[7].Position); Assert.Equal(0, generated.Roster[7].Stats.HpCurrent);
        Assert.Equal(8, generated.Occupancy.Count(index => index >= 0)); Assert.Equal((uint?)60, generated.CurrentGold);
        Assert.Equal((ushort?)1, generated.Roster[0].Stats.CurrentKills); Assert.Equal((byte?)39, generated.Roster[0].Stats.CurrentExp);
        Battle01PlayerPhysicalAttack.RequireAccountingInputs(generated, 0, 0, null);
    }

    [Fact]
    public void KillAccountingUsesSourceCapsAndKeepsUnknownInputsUnknown()
    {
        Assert.Equal(60u, Battle01PlayerPhysicalAttack.GoldAfterKill(0));
        Assert.Equal(9999998u, Battle01PlayerPhysicalAttack.GoldAfterKill(9999938));
        foreach (uint before in new uint[] { 9999939, 9999940, 9999999, uint.MaxValue })
            Assert.Equal(9999999u, Battle01PlayerPhysicalAttack.GoldAfterKill(before));
        Assert.Equal((ushort)9999, Battle01PlayerPhysicalAttack.KillsAfterKill(9998));
        Assert.Equal((ushort)9999, Battle01PlayerPhysicalAttack.KillsAfterKill(9999));
        var after = FirstDefeatCompleted();
        Assert.Throws<ArgumentException>(() => Battle01PlayerPhysicalAttack.RequireAccountingInputs(after, null, 0, null));
        Assert.Throws<ArgumentException>(() => Battle01PlayerPhysicalAttack.RequireAccountingInputs(after, 0, null, null));
        Assert.Null(Completed().CurrentGold); Assert.Null(Completed().Roster[0].Stats.CurrentKills);
        var unspecified = FirstDefeatSelected(accounting: false); string frozen = JsonSerializer.Serialize(unspecified);
        Assert.Equal("attack.killAccountingInput", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01PlayerPhysicalAttack.Confirm(unspecified, 0, Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(unspecified));
    }

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
    public void GeneratedRoundAndDamagedEnemyControlRetainThePlayerReactionProvenance()
    {
        var current=Completed();
        current=Battle01EnemyPursuit.CompleteNext(current,131,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        current=Battle01EnemyStandby.CompleteNext(current,133,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var generated=Battle01FirstRound.EnterNext(current);
        var forged=Battle01FirstRoundTests.CopyCurrent(generated,mainImage:generated.RandomSeedImage ^ 0x10000);
        Assert.ThrowsAny<ArgumentException>(()=>Battle01NextPlayerControl.Enter(forged,2));
        current=Battle01NextPlayerControl.Enter(generated,2).State!;
        current=Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current,2),2,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        current=Battle01EnemyPursuit.CompleteNext(current,131,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        Assert.Equal(132,current.FirstRound!.CurrentCandidate!.Value.CombatantIndex);
        Assert.Equal(2,current.Roster[7].Stats.HpCurrent);
        // A valid damaged actor can still classify its actual physical cohort; it is never healed to source HP5.
        Assert.Throws<Battle01AttackSelectionRequiredException>(()=>Battle01EnemyPursuit.CompleteNext(current,132,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats));
        Assert.Equal(5,current.Roster[7].EnemySource!.SourceStats.HpCurrent); Assert.Equal(2,current.Roster[7].Stats.HpCurrent);
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
        int[] Occupancy() { var cells = Enumerable.Repeat(-1, 2304).ToArray(); foreach (var u in roster) cells[u.RequirePosition().Y * 48 + u.RequirePosition().X] = u.Index; return cells; }
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

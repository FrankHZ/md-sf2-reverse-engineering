using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01EnemyPhysicalAttackTests
{
    internal static Battle01InitializedState LeaderDefeatBoundary(ushort? defeats = 0)
    {
        var current = FirstAllyDefeatBoundary(bowieDefeats: defeats);
        var stay = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;
        for (int step = 0; step < 32; step++)
        {
            var order = current.FirstRound!;
            if (order.RoundNumber == 16 && order.CurrentTurnOffset == 0) return current;
            if (order.CurrentCandidate is not { } candidate) { current = Battle01FirstRound.EnterNext(current); continue; }
            int actor = candidate.CombatantIndex;
            if (actor < 128)
                current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(
                    Battle01NextPlayerControl.Enter(current, actor).State!, actor), actor, stay);
            else
            {
                try { current = Battle01EnemyPursuit.CompleteNext(current, actor, stay); }
                catch (Battle01AttackSelectionRequiredException)
                { current = Battle01EnemyPhysicalAttack.CompleteNext(current, actor, Battle01PhysicalCompletionPolicy.ControlledLeaderDefeatPending); }
            }
        }
        throw new InvalidOperationException("The four origin STAY choices must reach R16 enemy129.");
    }

    internal static Battle01InitializedState LeaderDefeatCompleted() => Battle01EnemyPhysicalAttack.CompleteNext(
        LeaderDefeatBoundary(), 129, Battle01PhysicalCompletionPolicy.ControlledLeaderDefeatPending);

    [Fact]
    public void LeaderDefeatStopsAfterFirstCleanupAndCountWithoutAdvancingThe119ReceiptPrefix()
    {
        var before = LeaderDefeatBoundary(); var json = new JsonSerializerOptions { MaxDepth = 256 };
        string frozen = JsonSerializer.Serialize(before, json);
        var after = Battle01EnemyPhysicalAttack.CompleteNext(before, 129, Battle01PhysicalCompletionPolicy.ControlledLeaderDefeatPending);
        var terminal = Assert.IsType<Battle01DefeatPendingReceipt>(after.DefeatPending); var d = terminal.Attack;
        Assert.Equal(Battle01Phase.DefeatPending, after.Phase); Assert.Null(after.FirstControl);
        Assert.Same(before.TurnCompletion, after.TurnCompletion); Assert.Same(before.TurnCompletion, terminal.Previous);
        Assert.Equal(119, Battle01EnemyPursuitTests.Receipts(after).Count()); Assert.Same(before.FirstRound, after.FirstRound);
        Assert.Equal((16, (byte)0, 129), (after.FirstRound!.RoundNumber, after.FirstRound.CurrentTurnOffset, after.FirstRound.CurrentCandidate!.Value.CombatantIndex));
        Assert.False(terminal.AfterTurnExecuted); Assert.False(terminal.TurnAdvanced);
        Assert.Equal(new Battle01FactionCounts(0, 4), terminal.FirstOutcome);
        Assert.Equal(new[] { 0 }, terminal.Cleanup.FirstWorklist); Assert.Empty(terminal.Cleanup.AfterTurnWorklist);
        Assert.Equal((0, (ushort)0, (ushort)1), (terminal.Cleanup.DefeatedAlly, terminal.Cleanup.DefeatsBefore, terminal.Cleanup.DefeatsAfter));
        Assert.Equal((129, 0, 8), (d.ActorIndex, d.TargetIndex, d.GridCost));
        Assert.Equal(new MapPosition(11, 10), d.Origin); Assert.Equal(new MapPosition(11, 14), d.Destination);
        Assert.Equal(new byte[] { 3, 3, 3, 3, 255 }, d.MoveString);
        var p = Assert.Single(d.Priorities);
        Assert.Equal((230, 3, 0, 16, 66, (byte)0), (p.LandMultiplier, p.PotentialDamage, p.RemainingHp, p.Priority, p.Roll.GeneratorSteps, p.Roll.Result));
        Assert.Equal(new ushort[] { 32, 32, 1, 1 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 28, 18, 0, 0 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0xE4281234, 0x960F1234, 0x9ECA1234, 0x10491234 }, d.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((3, 0, 3), (d.Effect.Damage, (int)d.Effect.TemporaryHp, (int)d.Effect.RestoredHp));
        Assert.Equal(new Battle01PhysicalReaction(0, -3, 0, 0, 1), d.Effect.Reaction);
        Assert.False(d.Effect.Dodged); Assert.False(d.Effect.Critical);
        Assert.Equal((0x10491234u, (ushort?)0x0034, (ushort)0), (after.RandomSeedImage, after.RandomSeedCopy, after.NewlyTestedRegionMask));
        Assert.Equal(0, after.Roster[0].Stats.HpCurrent); Assert.Equal((ushort?)1, after.Roster[0].Stats.CurrentDefeats);
        Assert.Null(after.Roster[0].Position); Assert.Equal(-1, after.OccupantAt(new(11, 15)));
        Assert.Equal(129, after.OccupantAt(new(11, 14))); Assert.Equal(-1, after.OccupantAt(new(11, 10)));
        Assert.Equal(5, after.Occupancy.Count(index => index >= 0));
        foreach (int i in new[] { 1, 2, 3, 5, 6, 7, 8 }) Assert.Same(before.Roster[i], after.Roster[i]);
        Assert.Equal(((uint?)120, (byte?)63, (ushort?)2), (after.CurrentGold, after.Roster[0].Stats.CurrentExp, after.Roster[0].Stats.CurrentKills));
        Assert.Equal(frozen, JsonSerializer.Serialize(before, json));
        Battle01PlayerPhysicalAttack.RequireAccountingInputs(after, 0, 0, 0, 0, 0);
        Assert.Equal("accounting.input", Assert.Throws<ArgumentException>(() => Battle01PlayerPhysicalAttack.RequireAccountingInputs(after, 0, 0, 0, 0)).ParamName);
    }

    [Fact]
    public void LeaderDefeatRequiresItsNewPolicyAndSuppliedZeroBeforeAnyPublication()
    {
        var known = LeaderDefeatBoundary(); var unknown = LeaderDefeatBoundary(null);
        foreach (var policy in new[] { Policy, Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat })
            Assert.Equal("attack.lethal", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
                Battle01EnemyPhysicalAttack.CompleteNext(known, 129, policy)).ParamName);
        Assert.Equal("cleanup.before", Assert.Throws<ArgumentException>(() => Battle01EnemyPhysicalAttack.CompleteNext(
            unknown, 129, Battle01PhysicalCompletionPolicy.ControlledLeaderDefeatPending)).ParamName);
        Assert.Equal(3, known.Roster[0].Stats.HpCurrent); Assert.Null(unknown.Roster[0].Stats.CurrentDefeats);
    }

    internal static Battle01InitializedState FirstAllyDefeatBoundary(ushort? defeats = 0, ushort? bowieDefeats = null)
    {
        // Existing authored combat fixture supplies the comparison before any physical receipt.
        // The separate required Content test owns real transport, initialization and terrain.
        var current = AttackBoundary(0, firstDefeatAccounting: true, chesterExp: 0, chesterDefeats: defeats, bowieDefeats: bowieDefeats);
        var stay = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;
        for (int step=0;step<80;step++)
        {
            var order=current.FirstRound!;
            if(order.RoundNumber==13 && order.CurrentTurnOffset==10 && order.CurrentCandidate?.CombatantIndex==133)return current;
            if(order.CurrentCandidate is not {} candidate) { current=Battle01FirstRound.EnterNext(current);continue; }
            int actor=candidate.CombatantIndex;
            if(actor>=128)
            {
                if((current.Roster.Single(unit=>unit.Index==actor).AiBitfield & 1)==0)
                    current=Battle01EnemyStandby.CompleteNext(current,actor,stay);
                else
                {
                    try { current=Battle01EnemyPursuit.CompleteNext(current,actor,stay); }
                    catch(Battle01AttackSelectionRequiredException) { current=Battle01EnemyPhysicalAttack.CompleteNext(current,actor,Policy); }
                }
                continue;
            }
            current=Battle01NextPlayerControl.Enter(current,actor).State!;
            if(actor==2 && order.RoundNumber is 8 or 10)
                current=Battle01PlayerMovement.SelectDestination(current,actor,order.RoundNumber==8 ? new(11,14) : new(9,9));
            current=Battle01PlayerMovement.Confirm(current,actor);
            if(actor==0 && order.RoundNumber is 6 or 7 or 9 || actor==2 && order.RoundNumber==9)
            {
                current=Battle01PlayerPhysicalAttack.Begin(current,actor);
                var policy=actor==2 ? Battle01PlayerPhysicalCompletionPolicy.ControlledNonlethalStrikeAndExp
                    : order.RoundNumber==9 ? Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat
                    : Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat;
                current=Battle01PlayerPhysicalAttack.Confirm(current,actor,policy);
            }
            else current=Battle01TurnCompletion.CommitStay(current,actor,stay);
        }
        throw new InvalidOperationException("Authored continuation must reach the first ally defeat boundary.");
    }

    internal static Battle01InitializedState FirstAllyDefeatCompleted() => Battle01EnemyPhysicalAttack.CompleteNext(
        FirstAllyDefeatBoundary(),133,Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat);

    [Fact]
    public void FirstAllyDefeatRetainsTheSelectedStrikeAndEveryEarlierReceipt()
    {
        var before=FirstAllyDefeatBoundary(); var json=new JsonSerializerOptions{MaxDepth=256};
        string frozen=JsonSerializer.Serialize(before,json);
        var after=Battle01EnemyPhysicalAttack.CompleteNext(before,133,Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat);
        var r=after.TurnCompletion!;var d=r.EnemyPhysicalAttack!;var cleanup=r.AllyDefeat!;
        Assert.Equal(105,Battle01EnemyPursuitTests.Receipts(before).Count());
        Assert.Equal(106,Battle01EnemyPursuitTests.Receipts(after).Count());
        Assert.Same(before.TurnCompletion,r.Previous);Assert.Equal(frozen,JsonSerializer.Serialize(before,json));
        Assert.Equal((133,2,2,0,1),(d.ActorIndex,d.TargetIndex,d.Effect.Damage,(int)d.Effect.TemporaryHp,(int)d.Effect.RestoredHp));
        Assert.Equal(new ushort[]{6,17,0,0},d.Effect.Rolls.Select(roll=>roll.Result));
        Assert.Equal(new ushort[]{32,32,1,1},d.Effect.Rolls.Select(roll=>roll.Range));
        Assert.Equal(new Battle01PhysicalReaction(2,-2,0,0,1),d.Effect.Reaction);
        Assert.Equal((0x33171234u,(ushort?)0x0234),(after.RandomSeedImage,after.RandomSeedCopy));
        Assert.Equal(((ushort)0,(ushort)1),(cleanup.DefeatsBefore,cleanup.DefeatsAfter));
        Assert.Equal(new[]{2},cleanup.FirstWorklist);Assert.Empty(cleanup.AfterTurnWorklist);
        Assert.Equal(new Battle01FactionCounts(2,4),r.BeforeAfterTurn);Assert.Equal(r.BeforeAfterTurn,r.AfterAfterTurn);
        Assert.Null(after.Roster[2].Position);Assert.Equal(-1,after.OccupantAt(new(9,9)));
        Assert.Equal(0,after.Roster[2].Stats.HpCurrent);Assert.Equal((ushort?)1,after.Roster[2].Stats.CurrentDefeats);
        Assert.Same(before.Roster[6],after.Roster[6]);Assert.Same(before.Roster[7],after.Roster[7]);
        Assert.Same(before.FirstRound!.Slots,after.FirstRound!.Slots);
        Assert.Equal(((uint?)120,(byte?)63,(ushort?)2,(byte?)10),(after.CurrentGold,after.Roster[0].Stats.CurrentExp,
            after.Roster[0].Stats.CurrentKills,after.Roster[2].Stats.CurrentExp));
        Assert.Null(after.Roster[2].Stats.CurrentKills);
        Battle01PlayerPhysicalAttack.RequireAccountingInputs(after,0,0,0,0);
        Assert.Equal("accounting.input",Assert.Throws<ArgumentException>(() =>
            Battle01PlayerPhysicalAttack.RequireAccountingInputs(after,0,0,0)).ParamName);
    }

    [Fact]
    public void FirstAllyDefeatStillRejectsOldPolicyAndUnspecifiedCounterWithoutPublishingDamage()
    {
        var known=FirstAllyDefeatBoundary();
        Assert.Equal("attack.lethal",Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01EnemyPhysicalAttack.CompleteNext(known,133,Policy)).ParamName);
        var unknown=FirstAllyDefeatBoundary(null);var json=new JsonSerializerOptions{MaxDepth=256};
        string before=JsonSerializer.Serialize(unknown,json);
        Assert.Equal("cleanup.before",Assert.Throws<ArgumentException>(() =>
            Battle01EnemyPhysicalAttack.CompleteNext(unknown,133,Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat)).ParamName);
        Assert.Equal(before,JsonSerializer.Serialize(unknown,json));
    }

    internal static Battle01InitializedState AfterChesterPlayerRelay()
    {
        var current = Battle01PlayerPhysicalAttackTests.ChesterCompleted();
        var stay = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;
        current = Battle01EnemyStandby.CompleteNext(current, 128, stay);
        current = Battle01EnemyStandby.CompleteNext(current, 129, stay);
        current = Battle01EnemyPhysicalAttack.CompleteNext(current, 131, Policy);
        return Battle01EnemyStandby.CompleteNext(current, 133, stay);
    }

    [Fact]
    public void DamagedEnemyAfterChesterAttackSelectsBowieAndPreservesEarnedExp()
    {
        var after = AfterChesterPlayerRelay(); var receipt = after.TurnCompletion!.Previous!;
        var d = receipt.EnemyPhysicalAttack!;
        Assert.Equal((131, 0, 3), (d.ActorIndex, d.TargetIndex, (int)after.Roster[6].Stats.HpCurrent));
        Assert.Equal("battle01-class0-wooden-sword-effective-prowess3-v1", d.CombatProfile);
        Assert.Equal(new[] { 2, 1, 0 }, d.Priorities.Select(p => p.Target.Index));
        Assert.Equal(new[] { 230, 230, 230 }, d.Priorities.Select(p => p.LandMultiplier));
        Assert.Equal(new[] { 2, 2, 3 }, d.Priorities.Select(p => p.PotentialDamage));
        Assert.Equal(new[] { 7, 9, 3 }, d.Priorities.Select(p => p.RemainingHp));
        Assert.Equal(new[] { 1, 1, 7 }, d.Priorities.Select(p => p.Priority));
        Assert.Equal(new[] { 66, 57, 133 }, d.Priorities.Select(p => p.Roll.GeneratorSteps));
        Assert.Equal(new byte[] { 0, 1, 2 }, d.Priorities.Select(p => p.Roll.Result));
        Assert.Equal(new MapPosition(10, 15), d.Destination);
        Assert.Equal(new byte[] { 2, 3, 3, 255 }, d.MoveString);
        Assert.Equal(new ushort[] { 32, 32, 1, 1, 32, 32 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 13, 10, 0, 0, 12, 4 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0x68E61234, 0x53B51234, 0x40381234, 0x42DF1234, 0x655A1234, 0x25991234 },
            d.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((3, 3, 6, 3), (d.Effect.Damage, (int)d.Effect.TemporaryHp, (int)d.Effect.RestoredHp,
            (int)after.Roster[0].Stats.HpCurrent));
        Assert.False(d.Effect.Dodged); Assert.False(d.Effect.Critical);
        Assert.Equal(new Battle01PhysicalReaction(0, -3, 0, 0, 1), d.Effect.Reaction);
        Assert.Equal((byte?)10, after.Roster[2].Stats.CurrentExp); Assert.Null(after.Roster[2].Stats.CurrentKills);
        Assert.Equal(new byte[] { 255, 255, 255, 0, 0, 255 }, after.AiLastTargets.Take(6));
        Battle01PlayerPhysicalAttack.RequireAccountingInputs(after, 0, 0, 0);
    }

    internal static Battle01PhysicalCompletionPolicy Policy => Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike;

    internal static Battle01InitializedState ChesterAttackBoundary(byte? chesterExp = null)
    {
        var stay = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;
        var current = Battle01NextPlayerControl.Enter(Battle01PlayerPhysicalAttackTests.FirstDefeatCompleted(chesterExp), 1).State!;
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, 1), 1, stay);
        foreach (int actor in new[] { 129, 128, 130 }) current = Battle01EnemyStandby.CompleteNext(current, actor, stay);
        current = Battle01NextPlayerControl.Enter(Battle01FirstRound.EnterNext(current), 2).State!;
        current = Battle01PlayerMovement.SelectDestination(current, 2, new(11, 14));
        Assert.Equal(14, current.FirstControl!.Movement.GridCost);
        // This owner uses authored terrain; the real Content test owns the original exact path.
        Assert.Equal(8, current.FirstControl.Movement.Preview.Directions.Count);
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, 2), 2, stay);
        current = Battle01EnemyStandby.CompleteNext(current, 133, stay);
        foreach (int actor in new[] { 0, 1 })
        {
            current = Battle01NextPlayerControl.Enter(current, actor).State!;
            current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, actor), actor, stay);
        }
        foreach (int actor in new[] { 129, 128, 130 }) current = Battle01EnemyStandby.CompleteNext(current, actor, stay);
        Assert.Equal((8, (byte)14, 70, 0xB28D1234u, (ushort?)0x0034),
            (current.FirstRound!.RoundNumber, current.FirstRound.CurrentTurnOffset,
                Battle01EnemyPursuitTests.Receipts(current).Count(), current.RandomSeedImage, current.RandomSeedCopy));
        Assert.Equal(new MapPosition(11, 15), current.Roster[0].Position);
        Assert.Equal(new MapPosition(11, 14), current.Roster[2].Position);
        return current;
    }

    internal static Battle01InitializedState ChesterHitCompleted(byte? chesterExp = null) => Battle01EnemyPhysicalAttack.CompleteNext(ChesterAttackBoundary(chesterExp), 131, Policy);

    [Fact]
    public void ChesterFinalizationRejectsAFalseHpReplayAfterConstructingTheLocalPhysicalEffect()
    {
        var before = ChesterAttackBoundary(); var d = Battle01EnemyPhysicalAttack.Decide(before, before.Roster[6]);
        Assert.Equal(9, d.Effect.AfterStats.HpCurrent);
        var roster = before.Roster.ToArray(); var occupancy = before.Occupancy.ToArray(); var targets = before.AiLastTargets.ToArray();
        roster[6] = roster[6].WithPosition(d.Destination); roster[2] = roster[2].WithStats(roster[2].Stats.WithCurrentHp(10));
        occupancy[11 + 10 * 48] = -1; occupancy[11 + 13 * 48] = 131; targets[3] = 2;
        var local = new Battle01InitializedState(before, roster, occupancy, before.AiMemory.ToArray(),
            d.SeedCopyAfter, d.Effect.MainSeedAfter, targets);
        Assert.Equal("attack.history", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CompletePhysical(local, d, Policy)).ParamName);
        Assert.Equal(11, before.Roster[2].Stats.HpCurrent); Assert.Equal(0xB28D1234u, before.RandomSeedImage);
        Assert.Equal((ushort?)0x0034, before.RandomSeedCopy); Assert.Equal(255, before.AiLastTargets[3]);
        Assert.Equal(70, Battle01EnemyPursuitTests.Receipts(before).Count());
    }

    [Fact]
    public void ActualRoundEightChesterHitReplaysTheSelectedProfileAndPreservesFirstDefeatAccounting()
    {
        var before = ChesterAttackBoundary();
        var after = Battle01EnemyPhysicalAttack.CompleteNext(before, 131, Policy);
        var receipt = after.TurnCompletion!; var d = receipt.EnemyPhysicalAttack!; var p = Assert.Single(d.Priorities);
        Assert.Equal("battle01-class1-wooden-stick-effective-prowess3-v1", d.CombatProfile);
        Assert.Equal((131, 2, 6, 230, 2, 9, 7),
            (d.ActorIndex, d.TargetIndex, d.GridCost, p.LandMultiplier, p.PotentialDamage, p.RemainingHp, p.Priority));
        Assert.Equal(new MapPosition(11, 13), d.Destination);
        Assert.Equal(new byte[] { 3, 3, 3, 255 }, d.MoveString);
        Assert.Equal((57, (ushort)0x0034, (ushort)0x0134, (byte)1),
            (p.Roll.GeneratorSteps, p.Roll.BeforeSeedCopy, p.Roll.AfterSeedCopy, p.Roll.Result));
        Assert.Equal(new ushort[] { 32, 32, 1, 1, 32, 32 }, d.Effect.Rolls.Select(r => r.Range));
        Assert.Equal(new ushort[] { 2, 27, 0, 0, 25, 13 }, d.Effect.Rolls.Select(r => r.Result));
        Assert.Equal(new uint[] { 0xB28D1234, 0x11301234, 0xDF771234, 0x59121234, 0x85F11234, 0xCD441234 }, d.Effect.Rolls.Select(r => r.BeforeImage));
        Assert.Equal(new uint[] { 0x11301234, 0xDF771234, 0x59121234, 0x85F11234, 0xCD441234, 0x6C7B1234 }, d.Effect.Rolls.Select(r => r.AfterImage));
        Assert.Equal((2, 9, 11, 9), (d.Effect.Damage, (int)d.Effect.TemporaryHp, (int)d.Effect.RestoredHp, (int)after.Roster[2].Stats.HpCurrent));
        Assert.False(d.Effect.Dodged); Assert.False(d.Effect.Critical);
        Assert.Same(before.Roster[2].Stats, d.Effect.BeforeStats);
        Assert.Equal(new Battle01PhysicalReaction(2, -2, 0, 0, 1), d.Effect.Reaction);
        Assert.Equal((0x6C7B1234u, (ushort?)0x0134), (after.RandomSeedImage, after.RandomSeedCopy));
        Assert.Equal(new byte[] { 4, 52, 4, 36, 52, 52 }, after.AiMemory.Take(6));
        Assert.Equal(new byte[] { 255, 255, 255, 2, 0, 255 }, after.AiLastTargets.Take(6));
        Assert.Equal(0, after.NewlyTestedRegionMask); Assert.Equal(before.RegionFlags90Through105, after.RegionFlags90Through105);
        Assert.Equal(before.Roster.Select(u => u.AiBitfield), after.Roster.Select(u => u.AiBitfield));
        Assert.Equal(new Battle01FactionCounts(3, 5), receipt.BeforeAfterTurn); Assert.Equal(receipt.BeforeAfterTurn, receipt.AfterAfterTurn);
        Assert.Same(before.TurnCompletion, receipt.Previous); Assert.Null(receipt.EnemyDefeat); Assert.Null(receipt.PlayerPhysicalAttack);
        Assert.Equal(71, Battle01EnemyPursuitTests.Receipts(after).Count()); Assert.Equal(16, after.FirstRound!.CurrentTurnOffset);
        Assert.Same(before.FirstRound!.Slots, after.FirstRound.Slots);
        Assert.Equal((6, (byte?)39, (ushort?)1, (uint?)60),
            ((int)after.Roster[0].Stats.HpCurrent, after.Roster[0].Stats.CurrentExp, after.Roster[0].Stats.CurrentKills, after.CurrentGold));
        Assert.Null(after.Roster[2].Stats.CurrentExp); Assert.Null(after.Roster[2].Stats.CurrentKills);
        Assert.Null(after.Roster[7].Position); Assert.Equal(0, after.Roster[7].Stats.HpCurrent);
        Assert.Equal(8, after.Roster.Count(u => u.Position is not null));
        Assert.Equal(-1, after.OccupantAt(new(11, 10))); Assert.Equal(131, after.OccupantAt(new(11, 13)));
        for (int i = 0; i < before.Roster.Count; i++)
            if (i != 2) Assert.Same(before.Roster[i].Stats, after.Roster[i].Stats);
        Assert.Equal(11, before.Roster[2].Stats.HpCurrent);
    }

    [Theory]
    [InlineData("equipment")]
    [InlineData("status")]
    [InlineData("attack")]
    [InlineData("defense")]
    [InlineData("class")]
    [InlineData("exp")]
    [InlineData("kills")]
    public void ChesterTargetAdmissionDoesNotAcceptDriftedOrFabricatedInputs(string mutation)
    {
        var before = ChesterAttackBoundary(); var target = before.Roster[2]; var s = target.Stats;
        var stats = new Battle01Stats(s.Level, s.HpMax, s.HpCurrent, s.MpMax, s.MpCurrent,
            mutation == "attack" ? (byte)9 : s.Attack, mutation == "defense" ? (byte)4 : s.Defense,
            s.Agility, s.Move, mutation == "status" ? (ushort)1 : s.Status,
            mutation == "equipment" ? new ushort[] { 199, 0, 127, 127 } : s.Items, s.Spells,
            mutation == "exp" ? (byte)100 : null, mutation == "kills" ? (ushort)0 : null);
        var forgedTarget = new Battle01Combatant(target.Deployment, stats, mutation == "class" ? (byte)4 : target.ClassId, null)
            .WithPosition(target.Position).WithAiBitfield(0);
        var roster = before.Roster.ToArray(); roster[2] = forgedTarget;
        var forged = Battle01FirstRoundTests.CopyCurrent(before, roster: roster);
        Assert.Equal("attack.targetProfile", Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
            Battle01EnemyPhysicalAttack.RequireTargetProfile(forgedTarget)).ParamName);
        Assert.ThrowsAny<ArgumentException>(() => Battle01EnemyPhysicalAttack.CompleteNext(forged, 131, Policy));
        Assert.Same(before.TurnCompletion, forged.TurnCompletion); Assert.Equal(11, before.Roster[2].Stats.HpCurrent);
    }

    [Theory]
    [InlineData("hp")]
    [InlineData("main")]
    [InlineData("copy")]
    [InlineData("lastTarget")]
    [InlineData("exp")]
    [InlineData("gold")]
    [InlineData("kills")]
    [InlineData("profile")]
    [InlineData("reaction")]
    [InlineData("roll")]
    public void ChesterPhysicalHistoryRejectsForgedChannelsBeforeRoundNine(string mutation)
    {
        var after = ChesterHitCompleted(); var roster = after.Roster.ToArray(); var targets = after.AiLastTargets.ToArray();
        var receipt = after.TurnCompletion!; var d = receipt.EnemyPhysicalAttack!;
        uint main = after.RandomSeedImage, gold = after.CurrentGold!.Value; ushort copy = after.RandomSeedCopy!.Value;
        switch (mutation)
        {
            case "hp": roster[2] = roster[2].WithStats(roster[2].Stats.WithCurrentHp(10)); break;
            case "main": main ^= 0x10000; break;
            case "copy": copy ^= 0x100; break;
            case "lastTarget": targets[3] = 0; break;
            case "exp": roster[2] = roster[2].WithStats(roster[2].Stats.WithCurrentExp(0)); break;
            case "gold": gold++; break;
            case "kills": roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentKills(2)); break;
            case "profile": d = d with { Priorities = [d.Priorities[0] with { Target = after.Roster[0] }] }; break;
            case "reaction": d = d with { Effect = d.Effect with { Reaction = d.Effect.Reaction! with { HpDelta = -3 } } }; break;
            case "roll":
                var rolls = d.Effect.Rolls.ToArray(); rolls[0] = rolls[0] with { Result = 3 };
                d = d with { Effect = d.Effect with { Rolls = rolls } }; break;
        }
        var forged = Battle01FirstRoundTests.CopyCurrent(after, roster: roster, mainImage: main, copy: copy,
            lastTargets: targets, gold: gold, receipt: receipt with { EnemyPhysicalAttack = d });
        Assert.ThrowsAny<ArgumentException>(() => Battle01FirstRound.EnterNext(forged));
        Assert.Same(after.FirstRound, forged.FirstRound); Assert.Equal(9, after.Roster[2].Stats.HpCurrent);
    }

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

    internal static Battle01InitializedState AttackBoundary(byte? currentExp = null, bool firstDefeatAccounting = false,
        byte? chesterExp = null, ushort? chesterDefeats = null, ushort? bowieDefeats = null)
    {
        var current = Battle01EnemyPursuitTests.RoundThree();
        // The earlier movement-only authored helper has no equipment. Supply the accepted combat
        // comparison profile explicitly; HP, effective modifiers, order and RNG remain identical.
        var roster = current.Roster.ToArray();
        roster[0] = roster[0].WithStats(new(1, 12, 12, 8, 8, 9, 4, 4, 6, 0,
            [199, 0, 127, 127], [10, 63, 63, 63], currentExp, firstDefeatAccounting ? (ushort)0 : null, bowieDefeats));
        if (firstDefeatAccounting)
        {
            // Explicit authored comparison fixture matching the selected-input allied stat roles.
            roster[1] = roster[1].WithStats(new(1, 11, 11, 10, 10, 9, 5, 5, 5, 0,
                [213, 0, 0, 127], [0, 63, 63, 63]));
            roster[2] = roster[2].WithStats(new(1, 11, 11, 0, 0, 8, 5, 7, 7, 0,
                [184, 0, 127, 127], [63, 63, 63, 63], chesterExp, currentDefeats: chesterDefeats));
        }
        current = Battle01FirstRoundTests.CopyCurrent(current, roster: roster, gold: firstDefeatAccounting ? 0u : null);
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

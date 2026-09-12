using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01TurnCompletionTests
{
    [Theory]
    [InlineData("exp")]
    [InlineData("gold")]
    [InlineData("occupancy")]
    [InlineData("oldCorpse")]
    [InlineData("main")]
    [InlineData("policy")]
    public void ChesterFirstKillLateFinalizationFailuresRetainTheEntireTargetSelection(string field)
    {
        var selected = Battle01PlayerPhysicalAttackTests.ChesterFirstKillSelected();
        var json = new JsonSerializerOptions { MaxDepth = 256 }; string frozen = JsonSerializer.Serialize(selected,json);
        var d = Battle01PlayerPhysicalAttack.Decide(selected,2,allowDefeat:true,maximumDefeats:3);
        var roster = selected.Roster.ToArray(); var occupancy = selected.Occupancy.ToArray();
        roster[2] = roster[2].WithStats(field == "exp" ? d.ActorAfterStats.WithCurrentExp(55) : d.ActorAfterStats);
        roster[4] = roster[4].WithStats(d.Effect.AfterStats);
        if (field == "occupancy") occupancy[4 * 48 + 10] = -1;
        if (field == "oldCorpse") roster[6] = roster[6].WithPosition(new(10,15));
        var local = new Battle01InitializedState(selected,roster,d.Effect.MainSeedAfter ^ (field == "main" ? 0x10000u : 0u),
            d.GoldAfter + (field == "gold" ? 1u : 0u));
        local = new Battle01InitializedState(local,roster,Array.AsReadOnly(occupancy),local.FirstControl!);
        Assert.ThrowsAny<ArgumentException>(() => Battle01TurnCompletion.CompletePlayerPhysical(local,d,
            field == "policy" ? Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat : Battle01PlayerPhysicalCompletionPolicy.ControlledChesterFirstKill));
        Assert.Equal(frozen,JsonSerializer.Serialize(selected,json));
        Assert.Equal((93,(uint?)120,(ushort?)0),(Battle01EnemyPursuitTests.Receipts(selected).Count(),selected.CurrentGold,selected.Roster[2].Stats.CurrentKills));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CounterLateReplayAndFinalizationFailurePreserveThePrePrimaryState(bool omitExp)
    {
        var before=Battle01EnemyPhysicalAttackTests.CounterattackBoundary();
        var json=new JsonSerializerOptions{MaxDepth=256};string frozen=JsonSerializer.Serialize(before,json);
        var d=Battle01EnemyPhysicalAttack.Decide(before,before.Roster[5],allowChesterCounter:true);
        var roster=before.Roster.ToArray();var occupancy=before.Occupancy.ToArray();
        roster[5]=roster[5].WithPosition(d.Destination).WithStats(omitExp ? d.Counterattack!.Effect.AfterStats : d.Actor.Stats);
        roster[2]=roster[2].WithStats(omitExp ? d.Effect.AfterStats : d.Counterattack!.ActorAfterStats);
        occupancy[Battle01PlayerMovement.Offset(d.Origin)]=-1;occupancy[Battle01PlayerMovement.Offset(d.Destination)]=130;
        var targets=before.AiLastTargets.ToArray();targets[2]=2;
        var local=new Battle01InitializedState(before,roster,occupancy,before.AiMemory.ToArray(),d.SeedCopyAfter,d.MainSeedAfter,targets);
        Assert.ThrowsAny<ArgumentException>(()=>Battle01TurnCompletion.CompletePhysical(local,d,Battle01PhysicalCompletionPolicy.ControlledNonlethalChesterCounterAndExp));
        Assert.Equal(frozen,JsonSerializer.Serialize(before,json));Assert.Equal(88,Battle01EnemyPursuitTests.Receipts(before).Count());
    }

    [Fact]
    public void TerminalBattleRejectsEveryContinuingGameplayEntryWithoutMutation()
    {
        var current = Battle01EnemyPhysicalAttackTests.LeaderDefeatCompleted();
        var policy = Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;
        var json = new System.Text.Json.JsonSerializerOptions { MaxDepth = 256 };
        string before = System.Text.Json.JsonSerializer.Serialize(current, json);
        Action[] operations = [
            () => Battle01FirstRound.EnterNext(current),
            () => Battle01NextPlayerControl.Enter(current, 1),
            () => Battle01EnemyStandby.CompleteNext(current, 129, policy),
            () => Battle01EnemyPursuit.CompleteNext(current, 129, policy),
            () => Battle01EnemyPhysicalAttack.CompleteNext(current, 129, Battle01PhysicalCompletionPolicy.ControlledLeaderDefeatPending),
            () => Battle01PlayerMovement.Confirm(current, 1),
            () => Battle01PlayerMovement.Cancel(current, 1),
            () => Battle01PlayerPhysicalAttack.Begin(current, 1),
            () => Battle01TurnCompletion.CommitStay(current, 1, policy) ];
        foreach (var operation in operations) Assert.ThrowsAny<ArgumentException>(operation);
        Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(current, json));
    }

    [Theory]
    [InlineData("count")]
    [InlineData("cleanup")]
    [InlineData("previous")]
    [InlineData("round")]
    [InlineData("seed")]
    [InlineData("mask")]
    [InlineData("counter")]
    [InlineData("occupancy")]
    [InlineData("gold")]
    public void TerminalHistoryCannotForgeOutcomeCleanupOrContinuity(string drift)
    {
        var current = Battle01EnemyPhysicalAttackTests.LeaderDefeatCompleted(); var receipt = current.DefeatPending!;
        receipt = drift switch {
            "count" => receipt with { FirstOutcome = new(1, 4) },
            "cleanup" => receipt with { Cleanup = receipt.Cleanup with { FirstWorklist = new[] { 0, 2 } } },
            "previous" => receipt with { Previous = receipt.Previous.Previous! },
            "round" => receipt with { RoundNumber = 17 }, _ => receipt };
        var roster = current.Roster.ToArray(); var occupancy = current.Occupancy.ToArray();
        if (drift == "counter") roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentDefeats(2));
        if (drift == "occupancy") occupancy[15 * 48 + 11] = 0;
        var forged = Battle01FirstRoundTests.CopyCurrent(current, roster: roster, occupancy: occupancy,
            copy: drift == "seed" ? (ushort)0x0234 : null, tested: drift == "mask" ? (ushort)7 : null,
            gold: drift == "gold" ? 60u : null);
        forged = new Battle01InitializedState(forged, receipt);
        Assert.ThrowsAny<ArgumentException>(() => Battle01TurnCompletion.RequireDefeatPending(forged));
    }

    [Theory]
    [InlineData("counter")][InlineData("beforeCounter")][InlineData("afterCounter")][InlineData("ally")]
    [InlineData("worklist")][InlineData("afterWorklist")][InlineData("missing")][InlineData("policy")]
    [InlineData("counts")][InlineData("afterCounts")][InlineData("hp")][InlineData("placement")]
    [InlineData("occupancy")][InlineData("main")][InlineData("copy")][InlineData("gold")]
    [InlineData("kills")][InlineData("exp")][InlineData("duplicate")][InlineData("oldEnemy")]
    public void FirstAllyDefeatHistoryRejectsIndependentCleanupAccountingAndPlacementForgery(string field)
    {
        var accepted=Battle01EnemyPhysicalAttackTests.FirstAllyDefeatCompleted();
        var r=accepted.TurnCompletion!;var cleanup=r.AllyDefeat!;
        var roster=accepted.Roster.ToArray();var occupancy=accepted.Occupancy.ToArray();
        uint main=accepted.RandomSeedImage;ushort copy=accepted.RandomSeedCopy!.Value;uint? gold=accepted.CurrentGold;
        switch(field)
        {
            case "counter": roster[2]=roster[2].WithStats(roster[2].Stats.WithCurrentDefeats(2));break;
            case "beforeCounter": r=r with {AllyDefeat=cleanup with {DefeatsBefore=1}};break;
            case "afterCounter": r=r with {AllyDefeat=cleanup with {DefeatsAfter=2}};break;
            case "ally": r=r with {AllyDefeat=cleanup with {DefeatedAlly=1}};break;
            case "worklist": r=r with {AllyDefeat=cleanup with {FirstWorklist=new[]{2,131}}};break;
            case "afterWorklist": r=r with {AllyDefeat=cleanup with {AfterTurnWorklist=new[]{2}}};break;
            case "missing": r=r with {AllyDefeat=null};break;
            case "policy": r=r with {Policy=Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike};break;
            case "counts": r=r with {BeforeAfterTurn=new(3,4)};break;
            case "afterCounts": r=r with {AfterAfterTurn=new(2,3)};break;
            case "hp": roster[2]=roster[2].WithStats(roster[2].Stats.WithCurrentHp(1));break;
            case "placement": roster[2]=roster[2].WithPosition(new(9,9));break;
            case "occupancy": occupancy[9+9*48]=2;break;
            case "main": main ^= 0x10000;break;
            case "copy": copy ^= 0x100;break;
            case "gold": gold=121;break;
            case "kills": roster[0]=roster[0].WithStats(roster[0].Stats.WithCurrentKills(3));break;
            case "exp": roster[2]=roster[2].WithStats(roster[2].Stats.WithCurrentExp(11));break;
            case "duplicate": r=r with {Previous=r.Previous! with {AllyDefeat=cleanup}};break;
            case "oldEnemy": roster[6]=roster[6].WithStats(roster[6].Stats.WithCurrentHp(1));break;
        }
        var forged=Battle01FirstRoundTests.CopyCurrent(accepted,roster:roster,occupancy:occupancy,receipt:r,
            mainImage:main,copy:copy,gold:gold);
        var options=new System.Text.Json.JsonSerializerOptions {MaxDepth=256};
        string frozen=System.Text.Json.JsonSerializer.Serialize(forged,options);
        Assert.ThrowsAny<ArgumentException>(()=>Battle01EnemyPursuit.CompleteNext(forged,130,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats));
        Assert.Equal(frozen,System.Text.Json.JsonSerializer.Serialize(forged,options));
    }

    [Fact]
    public void FirstAllyDefeatLocalFinalizationRejectsAnIncorrectReactionReplay()
    {
        var before=Battle01EnemyPhysicalAttackTests.FirstAllyDefeatBoundary();
        var d=Battle01EnemyPhysicalAttack.Decide(before,before.Roster[8],allowAllyDefeat:true);
        var roster=before.Roster.ToArray();var targets=before.AiLastTargets.ToArray();targets[5]=2;
        // Intentionally omit the computed HP0 replay after the source-exact damage has been constructed.
        var local=new Battle01InitializedState(before,roster,before.Occupancy.ToArray(),before.AiMemory.ToArray(),
            d.SeedCopyAfter,d.Effect.MainSeedAfter,targets);
        Assert.Equal("cleanup.before",Assert.Throws<ArgumentException>(()=>Battle01TurnCompletion.CompletePhysical(
            local,d,Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat)).ParamName);
        Assert.Equal(1,before.Roster[2].Stats.HpCurrent);Assert.Equal((ushort?)0,before.Roster[2].Stats.CurrentDefeats);
        Assert.Equal(0x66531234u,before.RandomSeedImage);Assert.Equal((ushort?)0x0134,before.RandomSeedCopy);
    }

    [Theory]
    [InlineData(0,1)][InlineData(9998,9999)][InlineData(9999,9999)]
    public void AllyDefeatsUseTheSourceWordSaturation(int before,int after) =>
        Assert.Equal((ushort)after,Battle01TurnCompletion.DefeatsAfterDeath((ushort)before));

    [Theory]
    [InlineData("oldCorpsePlaced")]
    [InlineData("oldCorpseStats")]
    [InlineData("oldCorpseCell")]
    [InlineData("thirdCorpse")]
    [InlineData("newCell")]
    [InlineData("gold")]
    [InlineData("main")]
    [InlineData("exp")]
    public void LateSecondDefeatFinalizationRejectsFalseCleanupAndAwardsWithoutChangingSelection(string field)
    {
        var selected = Battle01PlayerPhysicalAttackTests.SecondDefeatSelected();
        var frozen = JsonSerializer.Serialize(selected, new JsonSerializerOptions { MaxDepth = 256 });
        var d = Battle01PlayerPhysicalAttack.Decide(selected, 0, allowDefeat: true, maximumDefeats: 2);
        var roster = selected.Roster.ToArray(); roster[0] = roster[0].WithStats(d.ActorAfterStats); roster[6] = roster[6].WithStats(d.Effect.AfterStats);
        var occupancy = selected.Occupancy.ToArray(); uint main = d.Effect.MainSeedAfter; uint gold = d.GoldAfter!.Value;
        switch (field)
        {
            case "oldCorpsePlaced": roster[7] = roster[7].WithPosition(new(12, 14)); break;
            case "oldCorpseStats": roster[7] = roster[7].WithStats(roster[7].Stats.WithCurrentExp(1)); break;
            case "oldCorpseCell": occupancy[14 * 48 + 12] = 132; break;
            case "thirdCorpse": roster[5] = roster[5].WithStats(roster[5].Stats.WithCurrentHp(0)); break;
            case "newCell": occupancy[15 * 48 + 10] = -1; break;
            case "gold": gold += 60; break;
            case "main": main ^= 0x10000; break;
            case "exp": roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentExp(87)); break;
        }
        var placed = new Battle01InitializedState(selected, roster, Array.AsReadOnly(occupancy), selected.FirstControl!);
        var replayed = new Battle01InitializedState(placed, roster, main, gold);
        var error = Assert.ThrowsAny<ArgumentException>(() => Battle01TurnCompletion.CompletePlayerPhysical(
            replayed, d, Battle01PlayerPhysicalAttackTests.SecondPolicy));
        if (field == "thirdCorpse") Assert.Equal("cleanup.before", error.ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(selected, new JsonSerializerOptions { MaxDepth = 256 }));
    }

    [Fact]
    public void PlayerStayRetainsPriorPhysicalDamageUnderItsOwnStrictPolicy()
    {
        var after = Battle01EnemyPhysicalAttackTests.CompletedAttack();
        var ready = Battle01NextPlayerControl.Enter(after, 0).State!;
        var completed = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(ready, 0), 0,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        Assert.Same(after.Roster[0].Stats, completed.Roster[0].Stats);
        Assert.Equal(9, completed.Roster[0].Stats.HpCurrent);
        Assert.Null(completed.TurnCompletion!.EnemyPhysicalAttack);
        Assert.Same(after.TurnCompletion, completed.TurnCompletion.Previous);
        Assert.Equal(after.RandomSeedImage, completed.RandomSeedImage);
        Assert.Equal("policy", Assert.Throws<ArgumentException>(() =>
            Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(ready, 0), 0, null)).ParamName);
    }


    [Fact]
    public void LaterPlayerCompletionAppendsItsOwnRoundAndCannotCompleteTwice()
    {
        var generated = Battle01FirstRound.EnterNext(Battle01FirstRoundTests.CompletedFirstRound());
        var completed = Battle01FirstRoundTests.CompleteOriginTurn(generated);
        Assert.Equal(2, completed.TurnCompletion!.RoundNumber);
        Assert.Equal(2, completed.TurnCompletion.CompletedActorIndex);
        Assert.Same(generated.TurnCompletion, completed.TurnCompletion.Previous);
        Assert.Equal(1, completed.TurnCompletion.Previous!.RoundNumber);
        Assert.Equal(2, completed.FirstRound!.CurrentTurnOffset);
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(
            completed,2,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats)).ParamName);
        Assert.All(Enumerable.Range(0,9), i => Assert.Same(generated.Roster[i].Stats,completed.Roster[i].Stats));
    }
    private static Battle01StayCompletionPolicy Policy => Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;

    [Fact]
    public void BowiesSeparateStayRetainsAllNineReceiptsAndStopsAtTheActualSentinelWithoutRegeneration()
    {
        var enemies = Battle01EnemyStandbyTests.AllEnemiesCompleted(); var ready = Battle01NextPlayerControl.Enter(enemies, 0).State!;
        var action = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 0, new(8, 17)), 0);
        var completed = Battle01TurnCompletion.CommitStay(action, 0, Policy);
        Assert.Equal(18, completed.FirstRound!.CurrentTurnOffset); Assert.Null(completed.FirstRound.CurrentCandidate);
        Assert.Equal(new Battle01TurnEntry(255, 255), completed.FirstRound.Slots[9]); Assert.Same(enemies.FirstRound!.Slots, completed.FirstRound.Slots);
        Assert.Equal(Battle01Phase.PlayerTurnCompleted, completed.Phase); Assert.Null(completed.FirstControl);
        var actors = new List<int>(); for (var receipt = completed.TurnCompletion; receipt is not null; receipt = receipt.Previous) actors.Add(receipt.CompletedActorIndex);
        Assert.Equal(new[] { 0, 132, 130, 129, 133, 131, 128, 2, 1 }, actors);
        Assert.Same(enemies.TurnCompletion, completed.TurnCompletion!.Previous); Assert.Equal(new MapPosition(8, 17), completed.Roster[0].Position);
        Assert.Same(enemies.AiMemory, completed.AiMemory); Assert.Same(enemies.AiLastTargets, completed.AiLastTargets);
        Assert.Same(enemies.RegionFlags90Through105, completed.RegionFlags90Through105); Assert.Equal(enemies.RandomSeedImage, completed.RandomSeedImage);
        Assert.Equal(enemies.RandomSeedCopy, completed.RandomSeedCopy); Assert.All(Enumerable.Range(0, 9), i => Assert.Same(enemies.Roster[i].Stats, completed.Roster[i].Stats));
        string frozen = JsonSerializer.Serialize(completed);
        for (int i = 0; i < 2; i++) Assert.Equal(Battle01FirstControlAvailability.Sentinel, Battle01NextPlayerControl.Enter(completed, 255).Decision.Availability);
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(completed, 0, Policy)).ParamName);
        Assert.Throws<ArgumentException>(() => Battle01FirstRound.Enter(completed));
        Assert.Equal(frozen, JsonSerializer.Serialize(completed));
    }

    [Fact]
    public void EnemyCompletionUsesTheSamePassiveRecoveryBoundaryBeforeAnyStateCanCommit()
    {
        var before = Battle01EnemyStandbyTests.SecondCompleted(); var roster = before.Roster.ToArray(); var unit = roster[1]; var stats = unit.Stats;
        roster[1] = new(unit.Deployment, new(stats.Level, stats.HpMax, stats.HpCurrent, stats.MpMax, stats.MpCurrent,
            stats.Attack, stats.Defense, stats.Agility, stats.Move, stats.Status, [0xE1, 127, 127, 127], stats.Spells),
            unit.ClassId, null, unit.AiBitfield, unit.Position);
        var input = Battle01EnemyStandbyTests.CopyCompleted(before, roster); string frozen = JsonSerializer.Serialize(input);
        Assert.Equal("equipment", Assert.Throws<ArgumentException>(() => Battle01EnemyStandby.CompleteFirst(input, 128, Policy)).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(input)); Assert.Same(before.TurnCompletion, input.TurnCompletion);
    }

    [Fact]
    public void StayRetainsProvisionalPositionAndStatsChecksBothFactionsThenAdvancesOneByteEntry()
    {
        var ready = Battle01FirstControl.Enter(Battle01FirstControlTests.Round(), 1).State!;
        var origin = ready.Roster[1].RequirePosition(); var destination = new MapPosition(origin.X, origin.Y - 1);
        var action = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 1, destination), 1);
        string before = JsonSerializer.Serialize(action);
        var completed = Battle01TurnCompletion.CommitStay(action, 1, Policy);
        Assert.Equal(Battle01Phase.PlayerTurnCompleted, completed.Phase); Assert.Null(completed.FirstControl);
        Assert.Equal(1, completed.TurnCompletion!.CompletedActorIndex); Assert.Same(Policy, completed.TurnCompletion.Policy);
        Assert.Equal(new Battle01FactionCounts(3, 6), completed.TurnCompletion.BeforeAfterTurn);
        Assert.Equal(completed.TurnCompletion.BeforeAfterTurn, completed.TurnCompletion.AfterAfterTurn);
        Assert.Equal(destination, completed.Roster[1].Position); Assert.Equal(origin, completed.Roster[1].Deployment.Position);
        Assert.Equal(-1, completed.OccupantAt(origin)); Assert.Equal(1, completed.OccupantAt(destination));
        Assert.Same(action.Roster, completed.Roster); Assert.Same(action.Occupancy, completed.Occupancy);
        Assert.Same(ready.Roster[1].Stats, completed.Roster[1].Stats);
        Assert.Same(action.FirstRound!.Slots, completed.FirstRound!.Slots); Assert.Equal(64, completed.FirstRound.Slots.Count);
        Assert.Equal(0, action.FirstRound.CurrentTurnOffset); Assert.Equal(2, completed.FirstRound.CurrentTurnOffset);
        Assert.Equal(action.FirstRound.Slots[1], completed.FirstRound.CurrentCandidate);
        Assert.Equal(action.FirstRound.FirstCandidate, completed.FirstRound.FirstCandidate);
        Assert.NotEqual(action.FirstRound.Slots[2], completed.FirstRound.CurrentCandidate);
        Assert.Equal(action.RandomSeedImage, completed.RandomSeedImage); Assert.Equal(0xA4991234u, completed.RandomSeedImage);
        Assert.Same(action.RegionFlags90Through105, completed.RegionFlags90Through105);
        Assert.Same(action.AiMemory, completed.AiMemory); Assert.Same(action.AiLastTargets, completed.AiLastTargets);
        Assert.Equal(before, JsonSerializer.Serialize(action));
        Assert.Equal(JsonSerializer.Serialize(completed), JsonSerializer.Serialize(Battle01TurnCompletion.CommitStay(action, 1, Policy)));
        foreach (Action request in new Action[] {
            () => Battle01TurnCompletion.CommitStay(completed, 1, Policy),
            () => Battle01PlayerMovement.Cancel(completed, 1),
            () => Battle01PlayerMovement.Confirm(completed, 1),
            () => Battle01PlayerMovement.SelectDestination(completed, 1, origin),
            () => Battle01FirstControl.Enter(completed, completed.FirstRound.CurrentCandidate!.Value.CombatantIndex) })
            Assert.Equal("phase", Assert.Throws<ArgumentException>(request).ParamName);
        Assert.Equal("current", Assert.Throws<ArgumentException>(() => Battle01FirstRound.Enter(completed)).ParamName);
    }

    [Fact]
    public void StayingAtOriginIsValidAndSentinelAtTheNextByteEntryIsNeverSkipped()
    {
        var round = Battle01FirstControlTests.Round(); var slots = round.FirstRound!.Slots.ToArray();
        slots[1] = new(255, 255); slots[2] = new(128, 90);
        round = new(round, round.Roster.ToArray(), round.RegionFlags90Through105.ToArray(), round.NewlyTestedRegionMask,
            round.RandomSeedImage, new(slots, [], []));
        var ready = Battle01FirstControl.Enter(round, 1).State!;
        var action = Battle01PlayerMovement.Confirm(ready, 1);
        var completed = Battle01TurnCompletion.CommitStay(action, 1, Policy);
        Assert.Equal(ready.Roster.Select(unit => unit.Position), completed.Roster.Select(unit => unit.Position));
        Assert.Equal(ready.Occupancy, completed.Occupancy); Assert.Null(completed.FirstRound!.CurrentCandidate);
        Assert.Equal(2, completed.FirstRound.CurrentTurnOffset); Assert.Equal(slots, completed.FirstRound.Slots);
        Assert.Equal(round.FirstRound!.FirstCandidate, completed.FirstRound.FirstCandidate);
    }

    [Theory]
    [InlineData(2, 127, "status")]
    [InlineData(0, 0xE1, "equipment")]
    [InlineData(0, 0xE4, "equipment")]
    [InlineData(0, 0xFC, "equipment")]
    [InlineData(0, 127, "stats")]
    public void UnconsumedStatusAndEquippedRecoveryRejectWithoutNormalizingAnything(ushort status, ushort item, string field)
    {
        var action = ActionChoice(); var source = action.Roster[1].Stats;
        var stats = new Battle01Stats(source.Level, source.HpMax, source.HpCurrent, source.MpMax, source.MpCurrent,
            source.Attack, source.Defense, source.Agility, source.Move, status, [item, 127, 127, 127], source.Spells);
        var roster = action.Roster.ToArray(); var actor = roster[1];
        roster[1] = new(actor.Deployment, stats, actor.ClassId, actor.EnemySource, actor.AiBitfield, actor.Position);
        var altered = new Battle01InitializedState(action, roster, action.Occupancy, action.FirstControl!);
        string before = JsonSerializer.Serialize(altered);
        Assert.Equal(field, Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(altered, 1, Policy)).ParamName);
        Assert.Equal(before, JsonSerializer.Serialize(altered)); Assert.Null(altered.TurnCompletion);
    }

    [Fact]
    public void MissingPolicyWrongActorWrongPhaseAndInconsistentPlacementCannotCommit()
    {
        var action = ActionChoice();
        Assert.Equal("policy", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(action, 1, null)).ParamName);
        Assert.Equal("actor", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(action, 2, Policy)).ParamName);
        var cancelled = Battle01PlayerMovement.Cancel(Battle01PlayerMovement.SelectDestination(
            Battle01FirstControl.Enter(Battle01FirstControlTests.Round(), 1).State!, 1, new(9, 17)), 1);
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(cancelled, 1, Policy)).ParamName);
        var inconsistent = new Battle01InitializedState(action, action.Roster.ToArray(),
            Array.AsReadOnly(Enumerable.Repeat(-1, 2304).ToArray()), action.FirstControl!);
        Assert.Equal("occupancy", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(inconsistent, 1, Policy)).ParamName);
        Assert.Equal(0, action.FirstRound!.CurrentTurnOffset); Assert.Null(action.TurnCompletion);
    }

    [Fact]
    public void DefeatedWrapperPrecedesPolicyAndIncompleteRosterCannotNormalize()
    {
        var action = ActionChoice();
        var missing = new Battle01InitializedState(action, action.Roster.Where(unit => unit.Index != 128).ToArray(),
            action.Occupancy, action.FirstControl!);
        Assert.Equal("defeated", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(missing, 1, null)).ParamName);
        var incomplete = new Battle01InitializedState(action, action.Roster.Where(unit => unit.Index != 2).ToArray(),
            action.Occupancy, action.FirstControl!);
        Assert.Equal("roster", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(incomplete, 1, Policy)).ParamName);
        var unplaced = action.Roster.Select(unit => unit.Index == 2 ? unit.WithPosition(new(47, 47)) : unit).ToArray();
        var outside = new Battle01InitializedState(action, unplaced, action.Occupancy, action.FirstControl!);
        Assert.Equal("position", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(outside, 1, Policy)).ParamName);
        Assert.Same(action.FirstRound, outside.FirstRound); Assert.Null(outside.TurnCompletion);
    }

    private static Battle01InitializedState ActionChoice() =>
        Battle01PlayerMovement.Confirm(Battle01FirstControl.Enter(Battle01FirstControlTests.Round(), 1).State!, 1);
}

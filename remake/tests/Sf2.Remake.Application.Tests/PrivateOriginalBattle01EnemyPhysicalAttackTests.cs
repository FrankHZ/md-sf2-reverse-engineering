using System.Text.Json;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Application.Tests.PrivateOriginalBattle01FirstControlTests;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01EnemyPhysicalAttackTests
{
    internal static GameSession ChesterAttackSession()
    {
        var session = PrivateOriginalBattle01PlayerPhysicalAttackTests.ReadySession(firstDefeat: true);
        for (int step = 0; step < 24; step++)
        {
            var current = session.PrivateOriginalBattle01!;
            if (current.Battle.FirstRound!.RoundNumber == 8 && current.Battle.FirstRound.CurrentTurnOffset == 14) return session;
            if (current.Battle.FirstRound.CurrentCandidate is null)
            {
                Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current));
                continue;
            }
            int actor = current.Battle.FirstRound.CurrentCandidate.Value.CombatantIndex;
            if (actor >= 128)
            {
                if ((current.Battle.Roster.Single(u => u.Index == actor).AiBitfield!.Value & 1) == 0)
                    Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(session.CompletePrivateOriginalBattle01EnemyStandby(current, actor));
                else
                {
                    var pursuit = session.CompletePrivateOriginalBattle01EnemyPursuit(current, actor);
                    if (pursuit is not PrivateOriginalBattle01EnemyPursuitCompleted)
                    {
                        Assert.IsType<PrivateOriginalBattle01AttackSelectionRequired>(pursuit);
                        Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current, actor));
                    }
                }
                continue;
            }
            if (current.Battle.FirstControl is null)
                current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current, actor)).Snapshot;
            if (current.Battle.FirstRound!.RoundNumber == 8 && actor == 2)
                current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(current, actor, new(11, 14))).Snapshot;
            current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current, actor)).Snapshot;
            if (actor == 0 && current.Battle.FirstRound!.RoundNumber < 8)
            {
                current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(current, actor)).Snapshot;
                Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.ConfirmPrivateOriginalBattle01PlayerAttack(current, actor));
            }
            else Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(current, actor));
        }
        throw new InvalidOperationException("The bounded authored route must reach actual R8 enemy131.");
    }

    [Fact]
    public void ChesterHitPublishesOnceAndHisNextControlRetainsPreparationAndEveryPriorAward()
    {
        var session = ChesterAttackSession(); var before = session.PrivateOriginalBattle01!;
        var completed = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before, 131)).Snapshot;
        Assert.Same(completed, session.PrivateOriginalBattle01); Assert.NotSame(before, completed);
        Assert.Same(before.Preparation, completed.Preparation); Assert.Same(before.SourceSnapshot, completed.SourceSnapshot);
        Assert.Same(before.SourceBridge, completed.SourceBridge); Assert.Same(before.SourceLocomotion, completed.SourceLocomotion);
        Assert.Equal(11, before.Battle.Roster[2].Stats.HpCurrent); Assert.Equal(9, completed.Battle.Roster[2].Stats.HpCurrent);
        Assert.Equal(11, completed.Preparation.Party.Allies[2].HpCurrent);
        Assert.Null(completed.Preparation.Party.Allies[2].CurrentExp); Assert.Null(completed.Battle.Roster[2].Stats.CurrentExp);
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before, 131)).Diagnostic.Field);
        Assert.Equal("actor", Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(completed, 131)).Diagnostic.Field);
        Assert.Same(completed, session.PrivateOriginalBattle01);
        var next = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(completed)).Snapshot;
        var ready = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(next, 2)).Snapshot;
        Assert.Equal((9, 2, 14), (ready.Battle.FirstRound!.RoundNumber, ready.Battle.FirstControl!.ActorIndex, ready.Battle.FirstControl.Movement.Range.Budget));
        Assert.Equal((0x71D31234u, (ushort?)0x0134), (ready.Battle.RandomSeedImage, ready.Battle.RandomSeedCopy));
        Assert.Equal(((uint?)60, (byte?)39, (ushort?)1), (ready.Battle.CurrentGold, ready.Battle.Roster[0].Stats.CurrentExp, ready.Battle.Roster[0].Stats.CurrentKills));
        Assert.Null(ready.Battle.Roster[7].Position); Assert.Equal(0, ready.Battle.Roster[7].Stats.HpCurrent);
        var moved = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(ready, 2, new(12, 14))).Snapshot;
        moved = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(moved, 2)).Snapshot;
        var cancelled = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.CancelPrivateOriginalBattle01PlayerMovement(moved, 2)).Snapshot;
        Assert.Same(ready.Battle.TurnCompletion, cancelled.Battle.TurnCompletion); Assert.Equal(ready.Battle.Occupancy, cancelled.Battle.Occupancy);
        Assert.Same(ready.Battle.Roster[2].Stats, cancelled.Battle.Roster[2].Stats);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("equivalent")]
    [InlineData("foreign")]
    public void ChesterHitRequiresTheExactCurrentSnapshot(string kind)
    {
        var session = ChesterAttackSession(); var current = session.PrivateOriginalBattle01!;
        var request = kind == "null" ? null : kind == "foreign" ? ChesterAttackSession().PrivateOriginalBattle01 :
            new PrivateOriginalBattle01SessionSnapshot(current.Preparation, current.Battle, current.SourceLocomotion, current.SourceBridge);
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(request, 131)).Diagnostic.Field);
        Assert.Same(current, session.PrivateOriginalBattle01);
    }

    [Fact]
    public void ChesterHistoryRejectionRetainsMovementHpRandomnessAndAccountingInTheExactSnapshot()
    {
        var session = ChesterAttackSession(); var source = session.PrivateOriginalBattle01!; var b = source.Battle;
        // The current balance must rewind through the existing first-defeat receipt.
        var forged = Internal<Battle01InitializedState>(b, b.Roster.ToArray(), b.RandomSeedImage, (uint?)61);
        var current = new PrivateOriginalBattle01SessionSnapshot(source.Preparation, forged, source.SourceLocomotion, source.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, current);
        Assert.Equal("attack.history", Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current, 131)).Diagnostic.Field);
        Assert.Same(current, session.PrivateOriginalBattle01); Assert.Same(b.TurnCompletion, current.Battle.TurnCompletion);
        Assert.Equal(b.RandomSeedImage, current.Battle.RandomSeedImage); Assert.Equal(b.RandomSeedCopy, current.Battle.RandomSeedCopy);
        Assert.Equal(b.Occupancy, current.Battle.Occupancy); Assert.Equal(b.AiLastTargets, current.Battle.AiLastTargets);
        Assert.Equal(11, current.Battle.Roster[2].Stats.HpCurrent); Assert.Equal(new MapPosition(11, 10), current.Battle.Roster[6].Position);
    }

    [Fact]
    public void AttackCommitsExactlyOnceAndLeavesPreparedPartyAndSourceProvenanceIntact()
    {
        var session = AttackSession(); var before = session.PrivateOriginalBattle01!;
        string frozen = JsonSerializer.Serialize(before.Battle);
        var after = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before, 132)).Snapshot;
        Assert.Same(after, session.PrivateOriginalBattle01); Assert.NotSame(before, after);
        Assert.Same(before.Preparation, after.Preparation); Assert.Same(before.SourceBridge, after.SourceBridge);
        Assert.Same(before.SourceSnapshot, after.SourceSnapshot); Assert.Same(before.SourceLocomotion, after.SourceLocomotion);
        Assert.Equal(12, after.Preparation.Party.Allies[0].HpCurrent); Assert.Equal(9, after.Battle.Roster[0].Stats.HpCurrent);
        Assert.Equal(0xAF881234u, after.Battle.RandomSeedImage); Assert.Equal((ushort?)0x0134, after.Battle.RandomSeedCopy);
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before, 132)).Diagnostic.Field);
        Assert.Equal("actor", Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(after, 132)).Diagnostic.Field);
        Assert.Same(after, session.PrivateOriginalBattle01); Assert.Equal(frozen, JsonSerializer.Serialize(before.Battle));
        var ready = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(after, 0)).Snapshot;
        Assert.Equal(0, ready.Battle.FirstControl!.ActorIndex); Assert.Equal(9, ready.Battle.Roster[0].Stats.HpCurrent);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("equivalent")]
    [InlineData("foreign")]
    public void OnlyTheExactCurrentSnapshotCanRequestAnAttack(string kind)
    {
        var session = AttackSession(); var current = session.PrivateOriginalBattle01!;
        var request = kind == "null" ? null : kind == "foreign" ? AttackSession().PrivateOriginalBattle01 :
            new PrivateOriginalBattle01SessionSnapshot(current.Preparation, current.Battle, current.SourceLocomotion, current.SourceBridge);
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(request, 132)).Diagnostic.Field);
        Assert.Same(current, session.PrivateOriginalBattle01);
    }

    [Theory]
    [InlineData("terrain", "terrain")]
    [InlineData("occupancy", "occupancy")]
    [InlineData("status", "status")]
    [InlineData("equipment", "attack.targetProfile")]
    [InlineData("profile", "attack.targetProfile")]
    [InlineData("unreceiptedHp", "attack.history")]
    [InlineData("unreceiptedDoubleSeed", "attack.history")]
    [InlineData("unreceiptedCounterSeed", "attack.history")]
    public void InvalidInputsAndComputedFollowupsRetainEveryCurrentChannel(string mutation, string field)
    {
        var session = AttackSession(); var source = session.PrivateOriginalBattle01!;
        var b = source.Battle; var roster = b.Roster.ToArray(); var terrain = b.Terrain.ToArray(); var occupancy = b.Occupancy.ToArray();
        uint main = b.RandomSeedImage; var target = roster[0]; var s = target.Stats;
        switch (mutation)
        {
            case "terrain": terrain[0] = 16; break;
            case "occupancy": occupancy[11 + 14 * 48] = 128; break;
            case "status":
            case "equipment":
            // Fabricated HP1 has no physical receipt; history admission rejects before resolution.
            case "unreceiptedHp":
                var changed = new Battle01Stats(s.Level, s.HpMax, mutation == "unreceiptedHp" ? (ushort)1 : s.HpCurrent,
                    s.MpMax, s.MpCurrent, s.Attack, s.Defense, s.Agility, s.Move, mutation == "status" ? (ushort)1 : s.Status,
                    mutation == "equipment" ? new ushort[] { 127, 0, 127, 127 } : s.Items, s.Spells);
                roster[0] = Internal<Battle01Combatant>(target.Deployment, changed, target.ClassId, target.EnemySource, target.AiBitfield, target.Position);
                break;
            case "profile":
                roster[0] = Internal<Battle01Combatant>(target.Deployment, s, (byte?)4, target.EnemySource, target.AiBitfield, target.Position); break;
            // These arbitrary images have no matching round/action history, so admission rejects before resolution.
            // Real source-seed double/counter branches remain covered by the Domain resolver tests.
            case "unreceiptedDoubleSeed": main = 0x00E71234; break;
            case "unreceiptedCounterSeed": main = 0x00A91234; break;
        }
        var initial = Internal<Battle01InitializedState>(roster, b.Regions.ToArray(), terrain, occupancy, main, b.RandomSeedCopy);
        var memory = Internal<Battle01InitializedState>(initial, roster, occupancy, b.AiMemory.ToArray(), b.RandomSeedCopy!.Value, main, b.AiLastTargets.ToArray());
        var round = Internal<Battle01InitializedState>(memory, roster, b.RegionFlags90Through105.ToArray(), b.NewlyTestedRegionMask, main, b.FirstRound);
        var battle = Internal<Battle01InitializedState>(round, round.FirstRound, b.TurnCompletion);
        var current = new PrivateOriginalBattle01SessionSnapshot(source.Preparation, battle, source.SourceLocomotion, source.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, current);
        string frozen = JsonSerializer.Serialize(current.Battle);
        Assert.Equal(field, Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackRejected>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current, 132)).Diagnostic.Field);
        Assert.Same(current, session.PrivateOriginalBattle01); Assert.Equal(frozen, JsonSerializer.Serialize(current.Battle));
    }

    internal static GameSession AttackSession()
    {
        var session = PrivateOriginalBattle01EnemyPursuitTests.RoundThreeSession();
        for (int round = 3; round <= 5; round++)
        {
            while (session.PrivateOriginalBattle01!.Battle.FirstRound!.CurrentCandidate is not null) CompleteCurrent(session);
            Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(session.PrivateOriginalBattle01));
        }
        for (int i = 0; i < 5; i++) CompleteCurrent(session);
        return session;
    }
    private static void CompleteCurrent(GameSession session)
    {
        var input = session.PrivateOriginalBattle01!; int actor = input.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
        if (actor >= 128 && (input.Battle.Roster.Single(unit => unit.Index == actor).AiBitfield!.Value & 1) != 0)
            Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(session.CompletePrivateOriginalBattle01EnemyPursuit(input, actor));
        else PrivateOriginalBattle01FirstRoundTests.CompleteOriginTurn(session);
    }
}

using System.Text.Json;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Application.Tests.PrivateOriginalBattle01FirstControlTests;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01EnemyPhysicalAttackTests
{
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
    [InlineData("lethal", "attack.lethal")]
    [InlineData("double", "attack.double")]
    [InlineData("counter", "attack.counter")]
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
            case "lethal":
                var changed = new Battle01Stats(s.Level, s.HpMax, mutation == "lethal" ? (ushort)1 : s.HpCurrent,
                    s.MpMax, s.MpCurrent, s.Attack, s.Defense, s.Agility, s.Move, mutation == "status" ? (ushort)1 : s.Status,
                    mutation == "equipment" ? new ushort[] { 127, 0, 127, 127 } : s.Items, s.Spells);
                roster[0] = Internal<Battle01Combatant>(target.Deployment, changed, target.ClassId, target.EnemySource, target.AiBitfield, target.Position);
                break;
            case "profile":
                roster[0] = Internal<Battle01Combatant>(target.Deployment, s, (byte?)4, target.EnemySource, target.AiBitfield, target.Position); break;
            // Authored main seeds: six source draws [1,19,0,0,0,12] / [1,13,0,0,24,0].
            case "double": main = 0x00E71234; break;
            case "counter": main = 0x00A91234; break;
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

using System.Text.Json;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Application.Tests.PrivateOriginalBattle01FirstControlTests;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01EnemyStandbyTests
{
    [Fact]
    public void CompletionReplacesOnlyTheExactSnapshotAndRetainsTheTwoPlayerTurnsAndSourceIdentities()
    {
        var session = SecondCompleted(); var before = session.PrivateOriginalBattle01!;
        var classified = Assert.IsType<PrivateOriginalBattle01NextPlayerControlUnavailable>(session.EnterPrivateOriginalBattle01NextPlayerControl(before, 128));
        Assert.Equal(Battle01FirstControlAvailability.OpponentAi, classified.Decision.Availability); Assert.Same(before, session.PrivateOriginalBattle01);
        var after = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(session.CompletePrivateOriginalBattle01EnemyStandby(before, 128)).Snapshot;
        Assert.Same(after, session.PrivateOriginalBattle01); Assert.NotSame(before, after);
        Assert.Same(before.Preparation, after.Preparation); Assert.Same(before.SourceSnapshot, after.SourceSnapshot);
        Assert.Same(before.SourceLocomotion, after.SourceLocomotion); Assert.Same(before.SourceBridge, after.SourceBridge);
        Assert.Same(before.Battle.TurnCompletion, after.Battle.TurnCompletion!.Previous);
        Assert.Equal(Battle01Phase.EnemyTurnCompleted, after.Battle.Phase); Assert.Equal(new MapPosition(6, 3), after.Battle.Roster[3].Position);
        Assert.Equal(6, after.Battle.FirstRound!.CurrentTurnOffset); Assert.Equal((byte)131, after.Battle.FirstRound.CurrentCandidate!.Value.CombatantIndex);
        Assert.Equal((ushort?)0x1234, before.Battle.RandomSeedCopy); Assert.Equal((ushort?)0x3934, after.Battle.RandomSeedCopy);
        Assert.Equal("snapshot", Reject(session.CompletePrivateOriginalBattle01EnemyStandby(before, 128)));
        Assert.Equal("actor", Reject(session.CompletePrivateOriginalBattle01EnemyStandby(after, 128))); Assert.Same(after, session.PrivateOriginalBattle01);
    }

    [Fact]
    public void MissingForeignCopiedWrongActorAndWrongPhaseRequestsAreAtomic()
    {
        var session = SecondCompleted(); var before = session.PrivateOriginalBattle01!;
        foreach (var expected in new PrivateOriginalBattle01SessionSnapshot?[] { null, SecondCompleted().PrivateOriginalBattle01,
            new(before.Preparation, before.Battle, before.SourceLocomotion, before.SourceBridge) })
            Assert.Equal("snapshot", Reject(session.CompletePrivateOriginalBattle01EnemyStandby(expected, 128)));
        Assert.Equal("actor", Reject(session.CompletePrivateOriginalBattle01EnemyStandby(before, 131))); Assert.Same(before, session.PrivateOriginalBattle01);
        var round = RoundSession(); Assert.Equal("phase", Reject(round.CompletePrivateOriginalBattle01EnemyStandby(round.PrivateOriginalBattle01, 128)));
        var pending = PrivateOriginalBattle01StartupTests.PendingSession(); Assert.Equal("battle", Reject(pending.CompletePrivateOriginalBattle01EnemyStandby(null, 128)));
    }

    [Theory]
    [InlineData("seed", "randomSeedCopy")]
    [InlineData("terrain", "terrain")]
    [InlineData("occupancy", "occupancy")]
    public void LateFailuresDoNotLeakThinkingMemoryOrMovementIntoTheCommittedSecondStay(string mutation, string field)
    {
        var session = SecondCompleted(); var source = session.PrivateOriginalBattle01!; var battle = source.Battle;
        var terrain = battle.Terrain.ToArray(); var occupancy = battle.Occupancy.ToArray();
        if (mutation == "terrain") terrain[3 * 48 + 6] = 16;
        if (mutation == "occupancy") occupancy[17 * 48 + 9] = -1; // Failure after local RNG, relocation and receipt preparation.
        var initial = Internal<Battle01InitializedState>(battle.Roster.ToArray(), battle.Regions.ToArray(), terrain, occupancy,
            battle.RandomSeedImage, mutation == "seed" ? null : (ushort?)0x1234);
        var round = Internal<Battle01InitializedState>(initial, initial.Roster.ToArray(), battle.RegionFlags90Through105.ToArray(),
            battle.NewlyTestedRegionMask, battle.RandomSeedImage, battle.FirstRound);
        var invalid = Internal<Battle01InitializedState>(round, battle.FirstRound, battle.TurnCompletion);
        var input = Replace(session, invalid); string frozen = JsonSerializer.Serialize(input.Battle);
        Assert.Equal(field, Reject(session.CompletePrivateOriginalBattle01EnemyStandby(input, 128)));
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Equal(frozen, JsonSerializer.Serialize(input.Battle));
        Assert.Same(battle.TurnCompletion, input.Battle.TurnCompletion); Assert.Equal(4, input.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Equal(7, input.Battle.NewlyTestedRegionMask); Assert.All(input.Battle.AiMemory, value => Assert.Equal(0, value));
    }

    [Fact]
    public void EachRemainingEnemyCommitsIndependentlyAndTheLastHandsOffToBowieWithoutAutomatingHisTurn()
    {
        var session = SecondCompleted(); var players = session.PrivateOriginalBattle01!;
        var current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(session.CompletePrivateOriginalBattle01EnemyStandby(players, 128)).Snapshot;
        for (int i = 0; i < 5; i++)
        {
            var previous = current; int actor = previous.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
            current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(session.CompletePrivateOriginalBattle01EnemyStandby(previous, actor)).Snapshot;
            Assert.Same(current, session.PrivateOriginalBattle01); Assert.Same(previous.Battle.TurnCompletion, current.Battle.TurnCompletion!.Previous);
            Assert.Same(players.Preparation, current.Preparation); Assert.Same(players.SourceLocomotion, current.SourceLocomotion);
            Assert.Same(players.SourceSnapshot, current.SourceSnapshot); Assert.Same(players.SourceBridge, current.SourceBridge);
            Assert.Equal("snapshot", Reject(session.CompletePrivateOriginalBattle01EnemyStandby(previous, actor))); Assert.Same(current, session.PrivateOriginalBattle01);
            Assert.Null(current.Battle.Roster[0].AiBitfield);
        }
        Assert.Equal(16, current.Battle.FirstRound!.CurrentTurnOffset); Assert.Equal((ushort?)0x0134, current.Battle.RandomSeedCopy);
        var ready = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current, 0)).Snapshot;
        Assert.Equal(0, ready.Battle.FirstControl!.ActorIndex); Assert.Equal(12, ready.Battle.FirstControl.Movement.Range.Budget);
        Assert.Same(current.Battle.TurnCompletion, ready.Battle.TurnCompletion); Assert.Equal((ushort?)0, ready.Battle.Roster[0].AiBitfield);
    }

    [Fact]
    public void MidRelayLateOccupancyFailureKeepsTheLastEnemyCommitAndItsExactThinkingState()
    {
        var session = SecondCompleted(); var current = session.PrivateOriginalBattle01!;
        foreach (int actor in new[] { 128, 131 }) current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(
            session.CompletePrivateOriginalBattle01EnemyStandby(current, actor)).Snapshot;
        var battle = current.Battle; var occupancy = battle.Occupancy.ToArray(); occupancy[17 * 48 + 9] = -1;
        var invalid = Internal<Battle01InitializedState>(battle, battle.Roster.ToArray(), occupancy, battle.AiMemory.ToArray(), battle.RandomSeedCopy!.Value);
        var input = Replace(session, invalid); string frozen = JsonSerializer.Serialize(input.Battle);
        Assert.Equal("occupancy", Reject(session.CompletePrivateOriginalBattle01EnemyStandby(input, 133)));
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Equal(frozen, JsonSerializer.Serialize(input.Battle));
        Assert.Same(battle.TurnCompletion, input.Battle.TurnCompletion); Assert.Equal(131, input.Battle.TurnCompletion!.CompletedActorIndex);
        Assert.Equal(8, input.Battle.FirstRound!.CurrentTurnOffset); Assert.Equal((ushort?)0x0134, input.Battle.RandomSeedCopy);
        Assert.Equal(0x24, input.Battle.AiMemory[3]); Assert.Equal(0, input.Battle.AiMemory[5]);
    }

    internal static GameSession AllEnemiesCompleted()
    {
        var session = SecondCompleted();
        for (int i = 0; i < 6; i++)
        {
            var current = session.PrivateOriginalBattle01!;
            Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(session.CompletePrivateOriginalBattle01EnemyStandby(
                current, current.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex));
        }
        return session;
    }

    internal static GameSession SecondCompleted()
    {
        var session = RoundSession(); var source = session.PrivateOriginalBattle01!.Battle; var roster = source.Roster.ToArray();
        MapPosition[] positions = [new(8, 18), new(9, 18), new(7, 18), new(7, 3), new(9, 4), new(6, 4), new(8, 3), new(9, 5), new(6, 5)];
        for (int i = 0; i < 9; i++)
        {
            var unit = roster[i]; var deployment = unit.Deployment with { Position = positions[i],
                AiCommandSet = (byte)(i >= 7 ? 7 : i >= 3 ? 6 : 0), PrimaryOrder = 255, SecondaryOrder = 255,
                PrimaryRegion = (byte)(i < 6 ? 2 : i < 8 ? 1 : 0), SecondaryRegion = 15,
                SourceFiller = (byte)(i >= 7 ? 112 : i >= 3 ? 96 : 0) };
            roster[i] = Internal<Battle01Combatant>(deployment, unit.Stats, unit.ClassId, unit.EnemySource,
                i < 3 ? null : (ushort?)(i >= 7 ? 0x2070 : 0x2060), positions[i]);
        }
        var terrain = source.Terrain.ToArray(); terrain[4 * 48 + 7] = 255; terrain[2 * 48 + 7] = 0;
        var occupancy = Enumerable.Repeat(-1, 2304).ToArray(); foreach (var unit in roster) occupancy[unit.Position.Y * 48 + unit.Position.X] = unit.Index;
        var initial = Internal<Battle01InitializedState>(roster, source.Regions.ToArray(), terrain, occupancy, 0x1234u, (ushort?)0x1234);
        var snapshot = Replace(session, Battle01FirstRound.Enter(initial));
        snapshot = Assert.IsType<PrivateOriginalBattle01FirstControlEntered>(session.EnterPrivateOriginalBattle01FirstControl(snapshot, 1)).Snapshot;
        snapshot = MoveStay(session, snapshot, 1, new(9, 17));
        snapshot = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(snapshot, 2)).Snapshot;
        MoveStay(session, snapshot, 2, new(7, 17)); return session;
    }
    private static PrivateOriginalBattle01SessionSnapshot MoveStay(GameSession session, PrivateOriginalBattle01SessionSnapshot snapshot, int actor, MapPosition target)
    {
        var selected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(snapshot, actor, target)).Snapshot;
        var moved = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(selected, actor)).Snapshot;
        return Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(moved, actor)).Snapshot;
    }
    private static PrivateOriginalBattle01SessionSnapshot Replace(GameSession session, Battle01InitializedState battle)
    {
        var source = session.PrivateOriginalBattle01!; var snapshot = new PrivateOriginalBattle01SessionSnapshot(source.Preparation, battle, source.SourceLocomotion, source.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, snapshot); return snapshot;
    }
    private static string Reject(PrivateOriginalBattle01EnemyStandbyResult result) => Assert.IsType<PrivateOriginalBattle01EnemyStandbyRejected>(result).Diagnostic.Field;
}

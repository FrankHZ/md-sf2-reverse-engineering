using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01NextPlayerControlTests
{
    [Fact]
    public void CurrentSnapshotEntersOnceAndSecondStayRetainsBothMovesAtTheEnemyBoundary()
    {
        var session = FirstCompleted(); var first = session.PrivateOriginalBattle01!;
        var next = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(first, 2)).Snapshot;
        Assert.Same(next, session.PrivateOriginalBattle01); Assert.Same(first.Preparation, next.Preparation);
        Assert.Same(first.SourceSnapshot, next.SourceSnapshot); Assert.Same(first.SourceLocomotion, next.SourceLocomotion);
        Assert.Same(first.SourceBridge, next.SourceBridge); Assert.Same(first.Battle.TurnCompletion, next.Battle.TurnCompletion);
        Assert.Same(first.Battle.Occupancy, next.Battle.FirstControl!.Movement.Range.OriginOccupancy);
        Assert.Equal(new MapPosition(2, 1), next.Battle.FirstControl.Movement.Range.Origin);
        Assert.Equal("snapshot", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(first, 2)));
        Assert.Equal("phase", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(next, 2)));
        Assert.Same(next, session.PrivateOriginalBattle01);
        var moved = Move(session, next, 2, new(2, 2));
        var cancelled = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.CancelPrivateOriginalBattle01PlayerMovement(moved, 2)).Snapshot;
        Assert.Equal(new MapPosition(1, 2), cancelled.Battle.Roster[1].Position);
        Assert.Equal(new MapPosition(2, 1), cancelled.Battle.Roster[2].Position);
        Assert.Equal(first.Battle.Occupancy, cancelled.Battle.Occupancy);
        moved = Move(session, cancelled, 2, new(2, 2));
        var second = Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(moved, 2)).Snapshot;
        Assert.Same(first.Battle.TurnCompletion, second.Battle.TurnCompletion!.Previous);
        Assert.Equal(new MapPosition(1, 2), second.Battle.Roster[1].Position);
        Assert.Equal(new MapPosition(2, 2), second.Battle.Roster[2].Position);
        Assert.Equal(4, second.Battle.FirstRound!.CurrentTurnOffset);
        int candidate = second.Battle.FirstRound.CurrentCandidate!.Value.CombatantIndex;
        Assert.Equal(128, candidate);
        var unavailable = Assert.IsType<PrivateOriginalBattle01NextPlayerControlUnavailable>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(second, candidate));
        Assert.Equal(Battle01FirstControlAvailability.OpponentAi, unavailable.Decision.Availability);
        Assert.Equal(candidate, unavailable.Decision.ActorIndex); Assert.Same(second, session.PrivateOriginalBattle01);
        Assert.Same(first.Battle.FirstRound!.Slots, second.Battle.FirstRound.Slots);
        Assert.Equal(first.Battle.RandomSeedImage, second.Battle.RandomSeedImage);
        Assert.Equal("snapshot", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(next, 2)));
        Assert.Same(second, session.PrivateOriginalBattle01);
    }

    [Fact]
    public void MissingForeignCopiedWrongActorAndWrongPhaseRequestsCannotChangeTheSession()
    {
        var session = FirstCompleted(); var current = session.PrivateOriginalBattle01!;
        foreach (var expected in new PrivateOriginalBattle01SessionSnapshot?[] { null, FirstCompleted().PrivateOriginalBattle01,
            new(current.Preparation, current.Battle, current.SourceLocomotion, current.SourceBridge) })
            Assert.Equal("snapshot", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(expected, 2)));
        Assert.Equal("actor", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(current, 1)));
        Assert.Same(current, session.PrivateOriginalBattle01);
        var round = PrivateOriginalBattle01FirstControlTests.RoundSession();
        Assert.Equal("phase", Reject(round.EnterPrivateOriginalBattle01NextPlayerControl(round.PrivateOriginalBattle01, 1)));
        Assert.Null(round.PrivateOriginalBattle01!.Battle.FirstControl);
    }

    [Fact]
    public void LateRangeFailureRetainsTheExactCommittedStayAndUnsuppliedCandidateWord()
    {
        var session = FirstCompleted(); var source = session.PrivateOriginalBattle01!; var battle = source.Battle;
        var terrain = battle.Terrain.ToArray(); terrain[2 * 48 + 2] = 16;
        var initial = PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(
            battle.Roster.ToArray(), battle.Regions.ToArray(), terrain, battle.Occupancy.ToArray(), battle.RandomSeedImage);
        var invalid = PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(initial, battle.FirstRound, battle.TurnCompletion);
        var input = new PrivateOriginalBattle01SessionSnapshot(source.Preparation, invalid, source.SourceLocomotion, source.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, input);
        Assert.Equal("terrain", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(input, 2)));
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Null(input.Battle.Roster[2].AiBitfield);
        Assert.Same(battle.TurnCompletion, input.Battle.TurnCompletion); Assert.Null(input.Battle.FirstControl);
        Assert.Equal(2, input.Battle.FirstRound!.CurrentTurnOffset);
    }

    private static GameSession FirstCompleted()
    {
        var session = PrivateOriginalBattle01FirstControlTests.RoundSession();
        var ready = Assert.IsType<PrivateOriginalBattle01FirstControlEntered>(
            session.EnterPrivateOriginalBattle01FirstControl(session.PrivateOriginalBattle01, 1)).Snapshot;
        var moved = Move(session, ready, 1, new(1, 2));
        Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(moved, 1));
        return session;
    }
    private static PrivateOriginalBattle01SessionSnapshot Move(GameSession session,
        PrivateOriginalBattle01SessionSnapshot snapshot, int actor, MapPosition destination)
    {
        var selected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(snapshot, actor, destination)).Snapshot;
        return Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(selected, actor)).Snapshot;
    }
    private static string Reject(PrivateOriginalBattle01NextPlayerControlResult result) =>
        Assert.IsType<PrivateOriginalBattle01NextPlayerControlRejected>(result).Diagnostic.Field;
}

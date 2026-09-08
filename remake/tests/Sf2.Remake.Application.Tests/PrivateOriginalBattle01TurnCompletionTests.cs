using System.Text.Json;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01TurnCompletionTests
{
    [Fact]
    public void ExactCurrentStayCommitsOnceAndRetainsInitializationProvenanceAndTheMovedUnit()
    {
        var session = ActionChoice(); var before = session.PrivateOriginalBattle01!;
        var committed = Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(before, 1)).Snapshot;
        Assert.Same(committed, session.PrivateOriginalBattle01); Assert.NotSame(before, committed);
        Assert.Equal(Battle01Phase.PlayerTurnCompleted, committed.Battle.Phase); Assert.Null(committed.Battle.FirstControl);
        Assert.Equal(new MapPosition(1, 2), committed.Battle.Roster[1].Position);
        Assert.Same(before.Battle.Occupancy, committed.Battle.Occupancy); Assert.Same(before.Battle.Roster, committed.Battle.Roster);
        Assert.Same(before.Preparation, committed.Preparation); Assert.Same(before.SourceSnapshot, committed.SourceSnapshot);
        Assert.Same(before.SourceLocomotion, committed.SourceLocomotion); Assert.Same(before.SourceBridge, committed.SourceBridge);
        Assert.Equal(2, committed.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Equal(before.Battle.FirstRound!.Slots[1], committed.Battle.FirstRound.CurrentCandidate);
        Assert.Same(before.Battle.FirstRound.Slots, committed.Battle.FirstRound.Slots);
        Assert.Equal(before.Battle.RandomSeedImage, committed.Battle.RandomSeedImage);
        string frozen = JsonSerializer.Serialize(committed.Battle);
        Assert.Equal("snapshot", Reject(session.CommitPrivateOriginalBattle01Stay(before, 1)));
        Assert.Equal("phase", Reject(session.CommitPrivateOriginalBattle01Stay(committed, 1)));
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01PlayerMovementRejected>(
            session.CancelPrivateOriginalBattle01PlayerMovement(committed, 1)).Diagnostic.Field);
        Assert.IsType<PrivateOriginalBattle01PlayerMovementRejected>(session.SelectPrivateOriginalBattle01PlayerDestination(committed, 1, new(1, 1)));
        Assert.IsType<PrivateOriginalBattle01PlayerMovementRejected>(session.ConfirmPrivateOriginalBattle01PlayerMovement(committed, 1));
        Assert.IsType<PrivateOriginalBattle01FirstControlRejected>(session.EnterPrivateOriginalBattle01FirstControl(committed,
            committed.Battle.FirstRound.CurrentCandidate!.Value.CombatantIndex));
        Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(session.EnterPrivateOriginalBattle01FirstRound(committed));
        Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMap(new(ExplorationDirection.North)));
        Assert.Throws<InvalidOperationException>(() => session.BeginPrivateOriginalMapPlayerLocomotion(new(ExplorationDirection.North)));
        Assert.Equal(frozen, JsonSerializer.Serialize(session.PrivateOriginalBattle01!.Battle)); Assert.Same(committed, session.PrivateOriginalBattle01);
    }

    [Fact]
    public void MissingForeignStaleWrongActorAndBeforeActionRequestsPreserveTheirSession()
    {
        var session = ActionChoice(); var current = session.PrivateOriginalBattle01!;
        Assert.Equal("snapshot", Reject(session.CommitPrivateOriginalBattle01Stay(null, 1)));
        Assert.Equal("snapshot", Reject(session.CommitPrivateOriginalBattle01Stay(ActionChoice().PrivateOriginalBattle01, 1)));
        Assert.Equal("actor", Reject(session.CommitPrivateOriginalBattle01Stay(current, 2)));
        Assert.Same(current, session.PrivateOriginalBattle01);
        var cancelled = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.CancelPrivateOriginalBattle01PlayerMovement(current, 1)).Snapshot;
        Assert.Equal("snapshot", Reject(session.CommitPrivateOriginalBattle01Stay(current, 1)));
        Assert.Equal("phase", Reject(session.CommitPrivateOriginalBattle01Stay(cancelled, 1)));
        Assert.Same(cancelled, session.PrivateOriginalBattle01); Assert.Equal(0, cancelled.Battle.FirstRound!.CurrentTurnOffset);
        var second = ActionChoice();
        var selected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(cancelled, 1, new(1, 2))).Snapshot;
        var confirmed = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(selected, 1)).Snapshot;
        var a = Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(confirmed, 1)).Snapshot;
        var b = Assert.IsType<PrivateOriginalBattle01StayCommitted>(second.CommitPrivateOriginalBattle01Stay(second.PrivateOriginalBattle01, 1)).Snapshot;
        Assert.Equal(JsonSerializer.Serialize(a.Battle), JsonSerializer.Serialize(b.Battle));
    }

    private static GameSession ActionChoice()
    {
        var session = PrivateOriginalBattle01FirstControlTests.RoundSession();
        var ready = Assert.IsType<PrivateOriginalBattle01FirstControlEntered>(
            session.EnterPrivateOriginalBattle01FirstControl(session.PrivateOriginalBattle01, 1)).Snapshot;
        var selected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(ready, 1, new(1, 2))).Snapshot;
        Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(selected, 1));
        return session;
    }
    private static string Reject(PrivateOriginalBattle01TurnCompletionResult result) =>
        Assert.IsType<PrivateOriginalBattle01TurnCompletionRejected>(result).Diagnostic.Field;
}

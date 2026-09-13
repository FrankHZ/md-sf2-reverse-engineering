using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01NextPlayerControlTests
{
    [Fact]
    public void Dead129TurnRelaysOnlyActualSarahAndCancelKeepsAllNinetyEightReceipts()
    {
        var session=PrivateOriginalBattle01TurnCompletionTests.Dead129Session(complete:true);var dead=session.PrivateOriginalBattle01!;
        Assert.IsType<PrivateOriginalBattle01NextPlayerControlRejected>(session.EnterPrivateOriginalBattle01NextPlayerControl(dead,1));
        var chased=Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(session.CompletePrivateOriginalBattle01EnemyPursuit(dead,130)).Snapshot;
        Assert.IsType<PrivateOriginalBattle01NextPlayerControlRejected>(session.EnterPrivateOriginalBattle01NextPlayerControl(chased,2));
        var ready=Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(chased,1)).Snapshot;
        var json=new System.Text.Json.JsonSerializerOptions{MaxDepth=256};string frozen=System.Text.Json.JsonSerializer.Serialize(ready.Battle,json);
        Assert.Equal((1,10),(ready.Battle.FirstControl!.ActorIndex,ready.Battle.FirstControl.Movement.Range.Budget));
        var selected=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(ready,1,new(10,17))).Snapshot;
        Assert.Equal(2,selected.Battle.FirstControl!.Movement.GridCost);
        var moved=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(selected,1)).Snapshot;
        Assert.Equal(new MapPosition(10,17),moved.Battle.Roster[1].Position);
        var cancelled=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.CancelPrivateOriginalBattle01PlayerMovement(moved,1)).Snapshot;
        Assert.Equal(frozen,System.Text.Json.JsonSerializer.Serialize(cancelled.Battle,json));Assert.Same(chased.Battle.TurnCompletion,cancelled.Battle.TurnCompletion);
        int count=0;for(var r=cancelled.Battle.TurnCompletion;r is not null;r=r.Previous)count++;
        Assert.Equal(98,count);Battle01PlayerPhysicalAttack.RequireAccountingInputs(cancelled.Battle,0,0,0,0,0,0);
    }

    [Fact]
    public void NewRoundEntryRejectsWrongActorAndStaleRequestsAndCancelRetainsPriorRound()
    {
        var session = PrivateOriginalBattle01FirstRoundTests.CompletedFirstRound();
        var generated = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(
            session.EnterPrivateOriginalBattle01NextRound(session.PrivateOriginalBattle01)).Snapshot;
        Assert.Equal("actor",Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(generated,1)));
        Assert.Same(generated,session.PrivateOriginalBattle01);
        var ready = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(generated,2)).Snapshot;
        Assert.Equal("snapshot",Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(generated,2)));
        Assert.Equal("phase",Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(ready,2)));
        var moved = Move(session,ready,2,new(7,16));
        var cancelled = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.CancelPrivateOriginalBattle01PlayerMovement(moved,2)).Snapshot;
        Assert.Equal(new MapPosition(7,17),cancelled.Battle.Roster[2].Position);
        Assert.Equal(generated.Battle.Occupancy,cancelled.Battle.Occupancy);
        Assert.Same(generated.Battle.TurnCompletion,cancelled.Battle.TurnCompletion);
        Assert.Same(generated.Battle.FirstRound,cancelled.Battle.FirstRound);
    }

    [Fact]
    public void LateControlFailureKeepsTheSuccessfullyGeneratedRoundWithoutReroll()
    {
        var session = PrivateOriginalBattle01FirstRoundTests.CompletedFirstRound();
        var generated = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(
            session.EnterPrivateOriginalBattle01NextRound(session.PrivateOriginalBattle01)).Snapshot;
        var terrain = generated.Battle.Terrain.ToArray(); terrain[16*48+7]=16;
        var input = PrivateOriginalBattle01FirstRoundTests.CopyCurrent(session,terrain:terrain);
        Assert.Equal("terrain",Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(input,2)));
        Assert.Same(input,session.PrivateOriginalBattle01); Assert.Equal(Battle01Phase.RoundGenerated,input.Battle.Phase);
        Assert.Equal(2,input.Battle.FirstRound!.RoundNumber); Assert.Equal(0xAA861234u,input.Battle.RandomSeedImage);
        Assert.Same(generated.Battle.TurnCompletion,input.Battle.TurnCompletion);
        Assert.Equal("round.phase",Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(
            session.EnterPrivateOriginalBattle01NextRound(input)).Diagnostic.Field);
        Assert.Same(input,session.PrivateOriginalBattle01);
    }
    [Fact]
    public void ExactBowieEntryAndSeparateMoveCancelStayEndAtTheSentinelWithAllReceipts()
    {
        var session = PrivateOriginalBattle01EnemyStandbyTests.AllEnemiesCompleted(); var enemies = session.PrivateOriginalBattle01!;
        Assert.Equal("snapshot", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(null, 0)));
        Assert.Equal("snapshot", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(
            new(enemies.Preparation, enemies.Battle, enemies.SourceLocomotion, enemies.SourceBridge), 0)));
        Assert.Equal("actor", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(enemies, 1)));
        var ready = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(enemies, 0)).Snapshot;
        Assert.Equal("snapshot", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(enemies, 0)));
        var moved = Move(session, ready, 0, new(8, 17)); var cancelled = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.CancelPrivateOriginalBattle01PlayerMovement(moved, 0)).Snapshot;
        Assert.Equal(new MapPosition(8, 18), cancelled.Battle.Roster[0].Position); Assert.Equal(enemies.Battle.Occupancy, cancelled.Battle.Occupancy);
        Assert.Same(enemies.Battle.TurnCompletion, cancelled.Battle.TurnCompletion);
        moved = Move(session, cancelled, 0, new(8, 17)); var completed = Assert.IsType<PrivateOriginalBattle01StayCommitted>(
            session.CommitPrivateOriginalBattle01Stay(moved, 0)).Snapshot;
        Assert.Same(completed, session.PrivateOriginalBattle01); Assert.Equal(18, completed.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Null(completed.Battle.FirstRound.CurrentCandidate); Assert.Equal((ushort?)0x0134, completed.Battle.RandomSeedCopy);
        var receipts = new List<int>(); for (var receipt = completed.Battle.TurnCompletion; receipt is not null; receipt = receipt.Previous) receipts.Add(receipt.CompletedActorIndex);
        Assert.Equal(new[] { 0, 132, 130, 129, 133, 131, 128, 2, 1 }, receipts);
        Assert.Equal(Battle01FirstControlAvailability.Sentinel, Assert.IsType<PrivateOriginalBattle01NextPlayerControlUnavailable>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(completed, 255)).Decision.Availability);
        Assert.Same(completed, session.PrivateOriginalBattle01); Assert.Same(enemies.Battle.FirstRound!.Slots, completed.Battle.FirstRound.Slots);
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01TurnCompletionRejected>(session.CommitPrivateOriginalBattle01Stay(completed, 0)).Diagnostic.Field);
        Assert.Same(completed, session.PrivateOriginalBattle01);
    }

    [Fact]
    public void LateBowieRangeFailureRetainsAllEightReceiptsAndTheMissingWord()
    {
        var session = PrivateOriginalBattle01EnemyStandbyTests.AllEnemiesCompleted();
        var source = session.PrivateOriginalBattle01!; var battle = source.Battle;
        var terrain = battle.Terrain.ToArray(); terrain[17 * 48 + 8] = 16;
        var initial = PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(
            battle.Roster.ToArray(), battle.Regions.ToArray(), terrain, battle.Occupancy.ToArray(), battle.RandomSeedImage, battle.RandomSeedCopy);
        var thinking = PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(initial,
            battle.Roster.ToArray(), battle.Occupancy.ToArray(), battle.AiMemory.ToArray(), battle.RandomSeedCopy!.Value);
        var invalid = PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(thinking, battle.FirstRound, battle.TurnCompletion);
        var input = new PrivateOriginalBattle01SessionSnapshot(source.Preparation, invalid, source.SourceLocomotion, source.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, input);
        Assert.Equal("terrain", Reject(session.EnterPrivateOriginalBattle01NextPlayerControl(input, 0)));
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Null(input.Battle.Roster[0].AiBitfield);
        Assert.Same(battle.TurnCompletion, input.Battle.TurnCompletion); Assert.Null(input.Battle.FirstControl);
        Assert.Equal(16, input.Battle.FirstRound!.CurrentTurnOffset); Assert.Equal((ushort?)0x0134, input.Battle.RandomSeedCopy);
        Assert.Equal(battle.AiMemory, input.Battle.AiMemory); Assert.Equal(battle.RandomSeedImage, input.Battle.RandomSeedImage);
    }

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

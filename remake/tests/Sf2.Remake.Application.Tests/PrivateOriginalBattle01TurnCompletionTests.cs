using System.Text.Json;
using System.Reflection;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Application.Tests.PrivateOriginalBattle01FirstControlTests;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01TurnCompletionTests
{
    internal static GameSession Dead129Session(bool complete=false)
    {
        var session=PrivateOriginalBattle01EnemyPhysicalAttackTests.Enemy128DefeatSession();
        Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(session.PrivateOriginalBattle01,128));
        Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(session.PrivateOriginalBattle01,0));
        Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(session.PrivateOriginalBattle01,0,new(9,11)));
        Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(session.PrivateOriginalBattle01,0));
        Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(session.PrivateOriginalBattle01,0));
        if(complete)Assert.IsType<PrivateOriginalBattle01DefeatedTurnCompleted>(session.CompletePrivateOriginalBattle01DefeatedTurn(session.PrivateOriginalBattle01,129));
        return session;
    }

    [Fact]
    public void Dead129TurnPublishesTheCompleteDomainResultOnceWithSourceAndPreparationRetained()
    {
        var session=Dead129Session();var before=session.PrivateOriginalBattle01!;
        var expected=Battle01TurnCompletion.CompleteDefeatedTurn(before.Battle,129,
            Battle01DefeatedTurnCompletionPolicy.ControlledEnemy129AfterChesterDefeat);
        var json=new JsonSerializerOptions{MaxDepth=256};string frozen=JsonSerializer.Serialize(before.Battle,json);
        var after=Assert.IsType<PrivateOriginalBattle01DefeatedTurnCompleted>(session.CompletePrivateOriginalBattle01DefeatedTurn(before,129)).Snapshot;
        Assert.Equal(JsonSerializer.Serialize(expected,json),JsonSerializer.Serialize(after.Battle,json));
        Assert.Same(after,session.PrivateOriginalBattle01);Assert.Same(before.Preparation,after.Preparation);
        Assert.Same(before.SourceSnapshot,after.SourceSnapshot);Assert.Same(before.SourceLocomotion,after.SourceLocomotion);Assert.Same(before.SourceBridge,after.SourceBridge);
        Assert.Equal("snapshot",Reject(session.CompletePrivateOriginalBattle01DefeatedTurn(before,129)));
        Assert.Equal("actor",Reject(session.CompletePrivateOriginalBattle01DefeatedTurn(after,129)));
        Assert.Equal(frozen,JsonSerializer.Serialize(before.Battle,json));Assert.Same(after,session.PrivateOriginalBattle01);
    }

    [Theory]
    [InlineData("null")][InlineData("foreign")][InlineData("stale")][InlineData("actor")]
    [InlineData("latePreparation")][InlineData("gold")][InlineData("occupancy")]
    public void Dead129TurnRejectsRequestsWithoutPublishingPartOfNinetySeven(string field)
    {
        var session=Dead129Session();var source=session.PrivateOriginalBattle01!;var battle=source.Battle;var preparation=source.Preparation;
        if(field=="latePreparation")preparation=new(source.Preparation.Pending,source.Preparation.Inputs,OriginalBattle01ControlledPartyPreset.LeaderDefeatComparison);
        if(field=="gold")battle=Internal<Battle01InitializedState>(battle,battle.Roster.ToArray(),battle.RandomSeedImage,181u);
        if(field=="occupancy")
        {
            var occupied=battle.Occupancy.ToArray();occupied[4*48+10]=129;
            battle=Internal<Battle01InitializedState>(battle,battle.Roster.ToArray(),Array.AsReadOnly(occupied),battle.FirstControl!);
        }
        var current=new PrivateOriginalBattle01SessionSnapshot(preparation,battle,source.SourceLocomotion,source.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session,current);
        var request=field=="null"?null:field=="foreign"?Dead129Session().PrivateOriginalBattle01:field=="stale"?source:current;
        var json=new JsonSerializerOptions{MaxDepth=256};string frozen=JsonSerializer.Serialize(current.Battle,json);
        Assert.IsType<PrivateOriginalBattle01TurnCompletionRejected>(session.CompletePrivateOriginalBattle01DefeatedTurn(request,field=="actor"?130:129));
        Assert.Same(current,session.PrivateOriginalBattle01);Assert.Equal(frozen,JsonSerializer.Serialize(current.Battle,json));
    }

    [Fact]
    public void Dead129TurnLateAccountingRejectionDoesNotPublishTheLocallyCompletedReceipt()
    {
        var session=Dead129Session();var current=session.PrivateOriginalBattle01!;
        // Authored negative fixture only: make Bowie's recorded defeat input consistently Unknown.
        // Every stats object is local to this session; the required-private positive route is untouched.
        var stats=new HashSet<Battle01Stats>{current.Battle.Roster[0].Stats};
        for(var r=current.Battle.TurnCompletion;r is not null;r=r.Previous)
        {
            if(r.PlayerPhysicalAttack is {ActorIndex:0} player){stats.Add(player.Actor.Stats);stats.Add(player.ActorAfterStats);}
            if(r.EnemyPhysicalAttack is {TargetIndex:0} enemy)
            {stats.Add(enemy.Priorities.Single(p=>p.Target.Index==0).Target.Stats);stats.Add(enemy.Effect.BeforeStats);stats.Add(enemy.Effect.AfterStats);}
        }
        var defeats=typeof(Battle01Stats).GetField("<CurrentDefeats>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic)!;
        foreach(var s in stats)defeats.SetValue(s,null);
        var json=new JsonSerializerOptions{MaxDepth=256};string frozen=JsonSerializer.Serialize(current.Battle,json);
        var local=Battle01TurnCompletion.CompleteDefeatedTurn(current.Battle,129,
            Battle01DefeatedTurnCompletionPolicy.ControlledEnemy129AfterChesterDefeat);
        Assert.True(local.TurnCompletion!.DefeatedTurnCompleted);
        Battle01PlayerPhysicalAttack.RequireAccountingInputs(local,0,0,0,0,null,0);
        Assert.Equal("accounting.input",Reject(session.CompletePrivateOriginalBattle01DefeatedTurn(current,129)));
        Assert.Same(current,session.PrivateOriginalBattle01);Assert.Equal(frozen,JsonSerializer.Serialize(current.Battle,json));
        Assert.Equal(6,current.Battle.FirstRound!.CurrentTurnOffset);
    }


    [Fact]
    public void SecondRoundStayCommitsOnceWithThePriorRoundHistoryAndEffectiveStats()
    {
        var session = PrivateOriginalBattle01FirstRoundTests.CompletedFirstRound();
        var generated = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(
            session.EnterPrivateOriginalBattle01NextRound(session.PrivateOriginalBattle01)).Snapshot;
        PrivateOriginalBattle01FirstRoundTests.CompleteOriginTurn(session);
        var completed = session.PrivateOriginalBattle01!;
        Assert.Equal(2,completed.Battle.TurnCompletion!.RoundNumber);
        Assert.Same(generated.Battle.TurnCompletion,completed.Battle.TurnCompletion.Previous);
        Assert.All(Enumerable.Range(0,9), i => Assert.Same(generated.Battle.Roster[i].Stats,completed.Battle.Roster[i].Stats));
        Assert.IsType<PrivateOriginalBattle01TurnCompletionRejected>(session.CommitPrivateOriginalBattle01Stay(completed,2));
        Assert.Same(completed,session.PrivateOriginalBattle01);
    }
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

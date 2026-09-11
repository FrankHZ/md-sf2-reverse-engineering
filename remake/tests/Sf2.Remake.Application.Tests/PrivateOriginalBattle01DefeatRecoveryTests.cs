using System.Text.Json;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01DefeatRecoveryTests
{
    private static GameSession TerminalSession()
    {
        var session = PrivateOriginalBattle01EnemyPhysicalAttackTests.FirstAllyDefeatSession(leader: true);
        Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(session.PrivateOriginalBattle01, 129));
        return session;
    }

    [Fact]
    public void RecoveryPublishesOnceAndClosesEveryContinuingEntryPoint()
    {
        var session = TerminalSession(); var before = session.PrivateOriginalBattle01!;
        var json = new JsonSerializerOptions { MaxDepth = 256 }; string frozen = JsonSerializer.Serialize(before.Battle, json);
        Assert.IsType<PrivateOriginalBattle01DefeatRecoveryRejected>(session.RecoverPrivateOriginalBattle01Defeat(null));
        var after = Assert.IsType<PrivateOriginalBattle01DefeatRecovered>(session.RecoverPrivateOriginalBattle01Defeat(before)).Snapshot;
        Assert.Same(after, session.PrivateOriginalBattle01); Assert.Same(before.Battle, after.Battle.DefeatRecovery!.Before);
        Assert.Same(before.Preparation, after.Preparation); Assert.Same(before.SourceSnapshot, after.SourceSnapshot);
        Assert.Same(before.SourceLocomotion, after.SourceLocomotion); Assert.Same(before.SourceBridge, after.SourceBridge);
        Assert.Equal(frozen, JsonSerializer.Serialize(before.Battle, json));
        Assert.Equal(GameFlowStage.Battle, session.PrivateOriginalFlowStage); Assert.Equal(new MapId("map57"), session.PrivateOriginalCurrentMap);
        string recovered = JsonSerializer.Serialize(after.Battle, json);
        object[] results = [session.RecoverPrivateOriginalBattle01Defeat(before), session.RecoverPrivateOriginalBattle01Defeat(after),
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(after,129), session.CompletePrivateOriginalBattle01EnemyPursuit(after,129),
            session.CompletePrivateOriginalBattle01EnemyStandby(after,129), session.EnterPrivateOriginalBattle01NextRound(after),
            session.EnterPrivateOriginalBattle01NextPlayerControl(after,1), session.EnterPrivateOriginalBattle01FirstRound(after),
            session.EnterPrivateOriginalBattle01FirstControl(after,0), session.SelectPrivateOriginalBattle01PlayerDestination(after,1,new(10,17)),
            session.ConfirmPrivateOriginalBattle01PlayerMovement(after,1), session.CancelPrivateOriginalBattle01PlayerMovement(after,1),
            session.BeginPrivateOriginalBattle01PlayerAttack(after,1), session.CyclePrivateOriginalBattle01PlayerAttackTarget(after,1,1),
            session.CancelPrivateOriginalBattle01PlayerAttackTarget(after,1), session.ConfirmPrivateOriginalBattle01PlayerAttack(after,1),
            session.CommitPrivateOriginalBattle01Stay(after,1), session.InitializePrivateOriginalBattle01(after.Preparation)];
        Assert.All(results, result => Assert.EndsWith("Rejected", result.GetType().Name));
        Assert.Same(after, session.PrivateOriginalBattle01); Assert.Equal(recovered, JsonSerializer.Serialize(after.Battle,json));
    }

    [Fact]
    public void ForeignSnapshotAndWrongPhaseCannotPublishRecovery()
    {
        var session = TerminalSession(); var current = session.PrivateOriginalBattle01!;
        var foreign = new PrivateOriginalBattle01SessionSnapshot(current.Preparation,current.Battle,current.SourceLocomotion,current.SourceBridge);
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01DefeatRecoveryRejected>(session.RecoverPrivateOriginalBattle01Defeat(foreign)).Diagnostic.Field);
        Assert.Same(current,session.PrivateOriginalBattle01);
        var early = PrivateOriginalBattle01EnemyPhysicalAttackTests.AttackSession();
        var before = early.PrivateOriginalBattle01;
        Assert.IsType<PrivateOriginalBattle01DefeatRecoveryRejected>(early.RecoverPrivateOriginalBattle01Defeat(before));
        Assert.Same(before,early.PrivateOriginalBattle01);
    }

    [Theory]
    [InlineData("gold")] [InlineData("party")]
    public void ForgedCurrentStateRetainsTheExactSessionOnFailure(string change)
    {
        var session = TerminalSession(); var before = session.PrivateOriginalBattle01!;
        var prepared = change == "party" ? new PrivateOriginalBattle01StartupPrepared(before.Preparation.Pending,
            before.Preparation.Inputs, OriginalBattle01ControlledPartyPreset.ChesterDefeatComparison) : before.Preparation;
        var battle = change == "gold" ? PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(
            before.Battle,before.Battle.Roster.ToArray(),before.Battle.RandomSeedImage,(uint?)121) : before.Battle;
        var forged = new PrivateOriginalBattle01SessionSnapshot(prepared,battle,before.SourceLocomotion,before.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session,forged);
        string frozen=JsonSerializer.Serialize(battle,new JsonSerializerOptions {MaxDepth=256});
        Assert.IsType<PrivateOriginalBattle01DefeatRecoveryRejected>(session.RecoverPrivateOriginalBattle01Defeat(forged));
        Assert.Same(forged,session.PrivateOriginalBattle01);
        Assert.Equal(frozen,JsonSerializer.Serialize(battle,new JsonSerializerOptions {MaxDepth=256}));
    }
}

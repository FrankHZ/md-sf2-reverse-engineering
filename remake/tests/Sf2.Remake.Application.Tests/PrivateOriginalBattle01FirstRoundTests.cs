using System.Reflection;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01FirstRoundTests
{
    [Fact]
    public void ExactSentinelCommitsOneNewGenerationWithTheEntirePreviousSnapshotProvenance()
    {
        var session=CompletedFirstRound(); var before=session.PrivateOriginalBattle01!;
        foreach (var invalid in new PrivateOriginalBattle01SessionSnapshot?[] {null,CompletedFirstRound().PrivateOriginalBattle01,
            new(before.Preparation,before.Battle,before.SourceLocomotion,before.SourceBridge)})
            Assert.Equal("snapshot",Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(session.EnterPrivateOriginalBattle01NextRound(invalid)).Diagnostic.Field);
        var after=Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(before)).Snapshot;
        Assert.Same(after,session.PrivateOriginalBattle01); Assert.Same(before.Preparation,after.Preparation);
        Assert.Same(before.SourceLocomotion,after.SourceLocomotion); Assert.Same(before.SourceSnapshot,after.SourceSnapshot); Assert.Same(before.SourceBridge,after.SourceBridge);
        Assert.Same(before.Battle.TurnCompletion,after.Battle.TurnCompletion); Assert.Same(before.Battle.AiMemory,after.Battle.AiMemory);
        Assert.Equal(Battle01Phase.RoundGenerated,after.Battle.Phase); Assert.Equal(2,after.Battle.FirstRound!.RoundNumber);
        Assert.Equal(0xAA861234u,after.Battle.RandomSeedImage); Assert.Equal((ushort?)0x0134,after.Battle.RandomSeedCopy);
        Assert.Equal("snapshot",Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(session.EnterPrivateOriginalBattle01NextRound(before)).Diagnostic.Field);
        Assert.Equal("round.phase",Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(session.EnterPrivateOriginalBattle01NextRound(after)).Diagnostic.Field);
        Assert.Same(after,session.PrivateOriginalBattle01);
    }

    [Fact]
    public void RepeatedRoundAndActorCommitsReachRealRoundThreeControlWithoutDiscardingReceipts()
    {
        var session=CompletedFirstRound(); var first=session.PrivateOriginalBattle01!;
        var second=Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(first)).Snapshot;
        for (int i=0;i<9;i++) CompleteOriginTurn(session);
        var completed=session.PrivateOriginalBattle01!;
        Assert.Equal(18,completed.Battle.FirstRound!.CurrentTurnOffset); Assert.Equal((ushort?)0x0034,completed.Battle.RandomSeedCopy);
        var third=Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(completed)).Snapshot;
        var ready=Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(third,2)).Snapshot;
        Assert.Equal(3,ready.Battle.FirstRound!.RoundNumber); Assert.Equal(0x9BD71234u,ready.Battle.RandomSeedImage);
        Assert.Equal(2,ready.Battle.FirstControl!.ActorIndex); Assert.False(ready.Battle.FirstControl.CandidateWordSupplied);
        Assert.Same(completed.Battle.TurnCompletion,ready.Battle.TurnCompletion);
        var receipts=new List<Battle01TurnCompletionReceipt>(); for (var r=ready.Battle.TurnCompletion;r is not null;r=r.Previous) receipts.Add(r);
        Assert.Equal(18,receipts.Count); Assert.All(receipts.Take(9),r=>Assert.Equal(2,r.RoundNumber)); Assert.All(receipts.Skip(9),r=>Assert.Equal(1,r.RoundNumber));
        for (int i=0;i<9;i++) Assert.Same(first.Battle.Roster[i].Stats,ready.Battle.Roster[i].Stats);
        Assert.Same(first.Battle.AiLastTargets,ready.Battle.AiLastTargets); Assert.Same(first.Preparation,ready.Preparation);
    }

    [Fact]
    public void LateSpawnRejectionDoesNotInstallTheNewRoundOrLoseTheLastCompletion()
    {
        var session=CompletedFirstRound(); var before=session.PrivateOriginalBattle01!; var roster=before.Battle.Roster.ToArray(); var e=roster[3];
        roster[3]=PrivateOriginalBattle01FirstControlTests.Internal<Battle01Combatant>(e.Deployment with {Spawn=1},e.Stats,e.ClassId,e.EnemySource,e.AiBitfield,e.Position);
        var input=CopyCurrent(session,roster:roster);
        Assert.Equal("round.spawn",Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(session.EnterPrivateOriginalBattle01NextRound(input)).Diagnostic.Field);
        Assert.Same(input,session.PrivateOriginalBattle01); Assert.Same(before.Battle.TurnCompletion,input.Battle.TurnCompletion);
        Assert.Equal(1,input.Battle.FirstRound!.RoundNumber); Assert.Equal(18,input.Battle.FirstRound.CurrentTurnOffset);
        Assert.Equal(before.Battle.RandomSeedImage,input.Battle.RandomSeedImage); Assert.Equal(before.Battle.AiMemory,input.Battle.AiMemory);
        Assert.All(input.Battle.RegionFlags90Through105,Assert.False);
    }

    internal static GameSession CompletedFirstRound()
    {
        var session=PrivateOriginalBattle01EnemyStandbyTests.AllEnemiesCompleted(); var current=session.PrivateOriginalBattle01!;
        var ready=Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current,0)).Snapshot;
        var selected=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(ready,0,new(8,17))).Snapshot;
        var moved=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(selected,0)).Snapshot;
        Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(moved,0)); return session;
    }
    internal static void CompleteOriginTurn(GameSession session)
    {
        var current=session.PrivateOriginalBattle01!; int actor=current.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
        if (actor>=128) Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(session.CompletePrivateOriginalBattle01EnemyStandby(current,actor));
        else
        {
            var ready=Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current,actor)).Snapshot;
            var moved=Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(ready,actor)).Snapshot;
            Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(moved,actor));
        }
    }
    internal static PrivateOriginalBattle01SessionSnapshot CopyCurrent(GameSession session,Battle01Combatant[]? roster=null,
        byte[]? terrain=null,int[]? occupancy=null,Battle01Region[]? regions=null,Battle01FirstRoundOrder? order=null)
    {
        var source=session.PrivateOriginalBattle01!; var b=source.Battle;
        var initial=PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(roster ?? b.Roster.ToArray(),regions ?? b.Regions.ToArray(),
            terrain ?? b.Terrain.ToArray(),occupancy ?? b.Occupancy.ToArray(),b.RandomSeedImage,b.RandomSeedCopy);
        var thinking=PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(initial,initial.Roster.ToArray(),initial.Occupancy.ToArray(),b.AiMemory.ToArray(),b.RandomSeedCopy!.Value);
        var round=PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(thinking,thinking.Roster.ToArray(),b.RegionFlags90Through105.ToArray(),b.NewlyTestedRegionMask,b.RandomSeedImage,order ?? b.FirstRound);
        var battle=PrivateOriginalBattle01FirstControlTests.Internal<Battle01InitializedState>(round,round.FirstRound,b.TurnCompletion);
        var input=new PrivateOriginalBattle01SessionSnapshot(source.Preparation,battle,source.SourceLocomotion,source.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session,input); return input;
    }

    [Fact]
    public void FirstRoundReplacesTheOnlyCurrentBattleAndRetainsInitializationAndSourceProvenance()
    {
        var session = InitializedSession(); var before = session.PrivateOriginalBattle01!;
        var applied = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01FirstRound(before));
        var after = applied.Snapshot;
        Assert.Same(after, session.PrivateOriginalBattle01); Assert.NotSame(before, after);
        Assert.Equal(Battle01Phase.FirstRoundGenerated, after.Battle.Phase);
        Assert.Equal(Battle01Phase.BeforeFirstRound, before.Battle.Phase); Assert.Null(before.Battle.FirstRound);
        Assert.Same(before.Preparation, after.Preparation); Assert.Same(before.SourceSnapshot, after.SourceSnapshot);
        Assert.Same(before.SourceLocomotion, after.SourceLocomotion); Assert.Same(before.SourceBridge, after.SourceBridge);
        Assert.Same(before.Battle.Terrain, after.Battle.Terrain); Assert.Same(before.Battle.Occupancy, after.Battle.Occupancy);
        Assert.Equal(0x00001234u, before.Battle.RandomSeedImage); Assert.Equal(0xA4991234u, after.Battle.RandomSeedImage);
        Assert.Equal(7, after.Battle.NewlyTestedRegionMask); Assert.Equal(0, before.Battle.NewlyTestedRegionMask);
        // This existing authored pending seed puts allies on its authored small region edges.
        // Real baseline no-activation behavior belongs to the required Content path.
        Assert.All(after.Battle.RegionFlags90Through105.Take(3), flag => Assert.True(flag));
        Assert.All(before.Battle.RegionFlags90Through105, flag => Assert.False(flag));
        for (int index = 0; index < 9; index++)
        {
            Assert.Same(before.Battle.Roster[index].Stats, after.Battle.Roster[index].Stats);
            Assert.Same(before.Battle.Roster[index].Deployment, after.Battle.Roster[index].Deployment);
        }
        Assert.Equal(GameFlowStage.Battle, session.PrivateOriginalFlowStage);
        Assert.Equal(new MapId("map57"), session.PrivateOriginalCurrentMap);
        Assert.Null(session.PrivateOriginalBattle01Admission);
        Assert.Equal(0, after.Battle.FirstRound!.CurrentTurnOffset);
        Assert.Equal((byte)1, after.Battle.FirstRound.FirstCandidate!.Value.CombatantIndex);
        Assert.Empty(after.Battle.FirstRound.RegionCutsceneRows); Assert.Empty(after.Battle.FirstRound.SpawnedCombatants);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("stale-copy")]
    public void NonCurrentRequestsRejectWithoutChangingTheBeforeRoundSnapshot(string scenario)
    {
        var session = InitializedSession(); var current = session.PrivateOriginalBattle01!;
        PrivateOriginalBattle01SessionSnapshot? request = scenario switch
        {
            "missing" => null,
            "foreign" => InitializedSession().PrivateOriginalBattle01!,
            _ => new(current.Preparation, current.Battle, current.SourceLocomotion, current.SourceBridge),
        };
        var rejected = Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(session.EnterPrivateOriginalBattle01FirstRound(request));
        Assert.Equal("snapshot", rejected.Diagnostic.Field); Assert.Same(current, session.PrivateOriginalBattle01);
        Assert.Equal(0x1234u, current.Battle.RandomSeedImage); Assert.Null(current.Battle.FirstRound);
        Assert.All(current.Battle.RegionFlags90Through105, flag => Assert.False(flag));
    }

    [Fact]
    public void MissingBattleAndCompletedFirstRoundCannotConsumeAnotherAdmissionOrAdvanceRng()
    {
        var pendingSession = PrivateOriginalBattle01StartupTests.PendingSession();
        var pending = pendingSession.PrivateOriginalBattle01Admission;
        Assert.Equal("battle", Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(
            pendingSession.EnterPrivateOriginalBattle01FirstRound(null)).Diagnostic.Field);
        Assert.Same(pending, pendingSession.PrivateOriginalBattle01Admission);
        var session = InitializedSession(); var before = session.PrivateOriginalBattle01!;
        var after = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01FirstRound(before)).Snapshot;
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(
            session.EnterPrivateOriginalBattle01FirstRound(before)).Diagnostic.Field);
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(
            session.EnterPrivateOriginalBattle01FirstRound(after)).Diagnostic.Field);
        Assert.Equal("battle", Assert.IsType<PrivateOriginalBattle01InitializationRejected>(
            session.InitializePrivateOriginalBattle01(before.Preparation)).Diagnostic.Field);
        Assert.Same(after, session.PrivateOriginalBattle01); Assert.Equal(0xA4991234u, after.Battle.RandomSeedImage);
        Assert.All(after.Battle.Roster.Skip(3), unit => Assert.Equal(8, unit.Stats.Attack));
        Assert.Null(session.PrivateOriginalBattle01Admission);
    }

    [Fact]
    public void FailedProjectionAfterTestingEarlierRegionsDoesNotInstallPartialFlagsOrRng()
    {
        var session = InitializedSession(); var before = session.PrivateOriginalBattle01!;
        var regions = before.Battle.Regions.ToArray();
        regions[2] = new(2, 0, [new(0, 0), new(1, 0), new(2, 0), new(3, 0)], 0, 0);
        // Construct one invalid test-owned immutable state using the existing internal constructor.
        // The real admission path cannot supply this polygon; no production injection API is added.
        var invalidState = (Battle01InitializedState)Activator.CreateInstance(typeof(Battle01InitializedState),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            [before.Battle.Roster.ToArray(), regions, before.Battle.Terrain.ToArray(),
                before.Battle.Occupancy.ToArray(), before.Battle.RandomSeedImage], null)!;
        var invalid = new PrivateOriginalBattle01SessionSnapshot(before.Preparation, invalidState, before.SourceLocomotion, before.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, invalid);
        var rejected = Assert.IsType<PrivateOriginalBattle01FirstRoundRejected>(session.EnterPrivateOriginalBattle01FirstRound(invalid));
        Assert.Equal("round.regions", rejected.Diagnostic.Field); Assert.Same(invalid, session.PrivateOriginalBattle01);
        Assert.All(invalidState.RegionFlags90Through105, flag => Assert.False(flag));
        Assert.Equal(0, invalidState.NewlyTestedRegionMask); Assert.Equal(0x1234u, invalidState.RandomSeedImage);
        Assert.Null(invalidState.FirstRound); Assert.Same(before.Preparation, invalid.Preparation);
        Assert.Same(before.SourceBridge, session.PrivateOriginalMapBattleBridge);
    }

    [Fact]
    public void OldExplorationStaysClosedAndFreshSessionResetsToMap3AfterRoundGeneration()
    {
        var session = InitializedSession();
        var after = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(
            session.EnterPrivateOriginalBattle01FirstRound(session.PrivateOriginalBattle01)).Snapshot;
        Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMap(new(ExplorationDirection.North)));
        Assert.Throws<InvalidOperationException>(() => session.BeginPrivateOriginalMapPlayerLocomotion(new(ExplorationDirection.North)));
        Assert.Throws<InvalidOperationException>(() => session.RequestPrivateOriginalMapInteraction(after.SourceSnapshot.SimulationStep));
        Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMapBattleBridge(new MoveExplorationCommand(ExplorationDirection.North)));
        Assert.Same(after, session.PrivateOriginalBattle01);
        var source = after.SourceSnapshot;
        var fresh = Assert.IsType<PrivateOriginalMapGameSessionStarted>(GameSession.StartPrivateOriginalMap(
            new PrivateOriginalBattle01InitializationTests.MapSource(source.Definition, source.Receipt),
            new(source.Receipt.PackageId, ContentProfile.PrivateLocal, source.Receipt.ContentDigest))).Session;
        Assert.Equal(new MapId("map3"), fresh.PrivateOriginalCurrentMap); Assert.Equal(GameFlowStage.Exploration, fresh.PrivateOriginalFlowStage);
        Assert.Null(fresh.PrivateOriginalBattle01); Assert.Null(fresh.PrivateOriginalBattle01Admission);
    }

    private static GameSession InitializedSession()
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession();
        Assert.IsType<PrivateOriginalBattle01Initialized>(
            session.InitializePrivateOriginalBattle01(PrivateOriginalBattle01InitializationTests.Prepare(session)));
        return session;
    }
}

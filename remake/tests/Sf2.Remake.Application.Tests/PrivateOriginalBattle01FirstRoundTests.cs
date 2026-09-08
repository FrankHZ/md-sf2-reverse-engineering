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

using System.Text.Json;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01PlayerMovementTests
{
    [Fact]
    public void PublicSelectionConfirmAndCancelKeepOneLivePositionAndPreserveSourceAndRoundState()
    {
        var session = ControlledSession(); var before = session.PrivateOriginalBattle01!; var origin = before.Battle.Roster[1].Position;
        var selected = Applied(session.SelectPrivateOriginalBattle01PlayerDestination(before, 1, new(1, 2)));
        Assert.Same(selected, session.PrivateOriginalBattle01); Assert.Equal(origin, selected.Battle.Roster[1].Position);
        Assert.Same(before.Battle.Occupancy, selected.Battle.Occupancy); Assert.Equal(new MapPosition(1, 2), selected.Battle.FirstControl!.Movement.Cursor);
        var confirmed = Applied(session.ConfirmPrivateOriginalBattle01PlayerMovement(selected, 1));
        Assert.Equal(Battle01Phase.PlayerActionChoice, confirmed.Battle.Phase); Assert.Equal(new MapPosition(1, 2), confirmed.Battle.Roster[1].Position);
        Assert.Equal(origin, confirmed.Battle.Roster[1].Deployment.Position);
        Assert.Equal(-1, confirmed.Battle.OccupantAt(origin)); Assert.Equal(1, confirmed.Battle.OccupantAt(new(1, 2)));
        Assert.Equal("phase", Rejected(session.ConfirmPrivateOriginalBattle01PlayerMovement(confirmed, 1)));
        Assert.Equal("phase", Rejected(session.SelectPrivateOriginalBattle01PlayerDestination(confirmed, 1, new(1, 3))));
        Assert.Same(confirmed, session.PrivateOriginalBattle01);
        var cancelled = Applied(session.CancelPrivateOriginalBattle01PlayerMovement(confirmed, 1));
        Assert.Equal(Battle01Phase.PlayerMovementSelection, cancelled.Battle.Phase); Assert.Equal(origin, cancelled.Battle.Roster[1].Position);
        Assert.Equal(origin, cancelled.Battle.FirstControl!.Movement.Cursor); Assert.Equal(before.Battle.Occupancy, cancelled.Battle.Occupancy);
        Assert.Equal("cancel", Rejected(session.CancelPrivateOriginalBattle01PlayerMovement(cancelled, 1)));
        Assert.Same(cancelled, session.PrivateOriginalBattle01);
        Assert.Same(before.Battle.FirstRound, cancelled.Battle.FirstRound); Assert.Equal(before.Battle.RandomSeedImage, cancelled.Battle.RandomSeedImage);
        Assert.Same(before.Battle.RegionFlags90Through105, cancelled.Battle.RegionFlags90Through105);
        Assert.Same(before.Battle.Roster[1].Stats, cancelled.Battle.Roster[1].Stats); Assert.Same(before.Battle.Roster[1].Deployment, cancelled.Battle.Roster[1].Deployment);
        Assert.Same(before.Preparation, cancelled.Preparation); Assert.Same(before.SourceBridge, cancelled.SourceBridge);
        Assert.Same(before.SourceSnapshot, cancelled.SourceSnapshot); Assert.Same(before.SourceLocomotion, cancelled.SourceLocomotion);
        Assert.Equal(0, cancelled.Battle.FirstRound!.CurrentTurnOffset); Assert.Null(session.PrivateOriginalBattle01Admission);
        Assert.Equal(origin, before.Battle.Roster[1].Position); Assert.Equal(new MapPosition(1, 2), confirmed.Battle.Roster[1].Position);
    }

    [Fact]
    public void InvalidDestinationsWrongActorsStaleRequestsAndOccupiedConfirmsCommitNothing()
    {
        var session = ControlledSession(); var before = session.PrivateOriginalBattle01!;
        Assert.Equal("destination", Rejected(session.SelectPrivateOriginalBattle01PlayerDestination(before, 1, new(48, 1))));
        Assert.Equal("destination", Rejected(session.SelectPrivateOriginalBattle01PlayerDestination(before, 1, new(15, 19))));
        Assert.Equal("actor", Rejected(session.SelectPrivateOriginalBattle01PlayerDestination(before, 2, new(1, 2))));
        Assert.Equal("snapshot", Rejected(session.ConfirmPrivateOriginalBattle01PlayerMovement(null, 1)));
        Assert.Equal("snapshot", Rejected(session.CancelPrivateOriginalBattle01PlayerMovement(ControlledSession().PrivateOriginalBattle01, 1)));
        Assert.Same(before, session.PrivateOriginalBattle01);
        var occupied = Applied(session.SelectPrivateOriginalBattle01PlayerDestination(before, 1, new(0, 1)));
        Assert.False(occupied.Battle.FirstControl!.Movement.CanConfirm);
        Assert.Equal("destination", Rejected(session.ConfirmPrivateOriginalBattle01PlayerMovement(occupied, 1)));
        Assert.Equal("snapshot", Rejected(session.ConfirmPrivateOriginalBattle01PlayerMovement(before, 1)));
        Assert.Same(occupied, session.PrivateOriginalBattle01);
        var cancelled = Applied(session.CancelPrivateOriginalBattle01PlayerMovement(occupied, 1));
        Assert.Equal(before.Battle.Occupancy, cancelled.Battle.Occupancy);
        Assert.Equal("snapshot", Rejected(session.CancelPrivateOriginalBattle01PlayerMovement(occupied, 1)));
        Assert.Same(cancelled, session.PrivateOriginalBattle01);
    }

    [Fact]
    public void EquivalentSessionsProduceTheSamePreviewAndExplorationStaysClosedUntilFreshStart()
    {
        var first = ControlledSession(); var second = ControlledSession();
        var a = Applied(first.SelectPrivateOriginalBattle01PlayerDestination(first.PrivateOriginalBattle01, 1, new(1, 3)));
        var b = Applied(second.SelectPrivateOriginalBattle01PlayerDestination(second.PrivateOriginalBattle01, 1, new(1, 3)));
        Assert.Equal(JsonSerializer.Serialize(a.Battle), JsonSerializer.Serialize(b.Battle));
        Assert.Throws<InvalidOperationException>(() => first.ApplyPrivateOriginalMap(new(ExplorationDirection.North)));
        Assert.Throws<InvalidOperationException>(() => first.BeginPrivateOriginalMapPlayerLocomotion(new(ExplorationDirection.North)));
        Assert.Throws<InvalidOperationException>(() => first.RequestPrivateOriginalMapInteraction(a.SourceSnapshot.SimulationStep));
        Assert.Throws<InvalidOperationException>(() => first.ApplyPrivateOriginalMapBattleBridge(new MoveExplorationCommand(ExplorationDirection.North)));
        var source = a.SourceSnapshot;
        var fresh = Assert.IsType<PrivateOriginalMapGameSessionStarted>(GameSession.StartPrivateOriginalMap(
            new PrivateOriginalBattle01InitializationTests.MapSource(source.Definition, source.Receipt),
            new(source.Receipt.PackageId, ContentProfile.PrivateLocal, source.Receipt.ContentDigest))).Session;
        Assert.Null(fresh.PrivateOriginalBattle01); Assert.Equal(new MapId("map3"), fresh.PrivateOriginalCurrentMap);
        Assert.Equal(GameFlowStage.Exploration, fresh.PrivateOriginalFlowStage);
    }

    private static GameSession ControlledSession()
    {
        var session = PrivateOriginalBattle01FirstControlTests.RoundSession();
        Assert.IsType<PrivateOriginalBattle01FirstControlEntered>(session.EnterPrivateOriginalBattle01FirstControl(session.PrivateOriginalBattle01, 1));
        return session;
    }
    private static PrivateOriginalBattle01SessionSnapshot Applied(PrivateOriginalBattle01PlayerMovementResult result) =>
        Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(result).Snapshot;
    private static string Rejected(PrivateOriginalBattle01PlayerMovementResult result) =>
        Assert.IsType<PrivateOriginalBattle01PlayerMovementRejected>(result).Diagnostic.Field;
}

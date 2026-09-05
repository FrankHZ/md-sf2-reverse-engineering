using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class OriginalMapGameSessionTests
{
    [Fact]
    public void ExactAcceptedImportStartsASeparatedPrivateMap3Session()
    {
        AcceptedSource source = new(Accepted());

        PrivateOriginalMapGameSessionStarted started =
            Assert.IsType<PrivateOriginalMapGameSessionStarted>(
                GameSession.StartPrivateOriginalMap(source, Request()));

        Assert.Equal(1, source.AdmitCalls);
        Assert.Equal(ContentProfile.PrivateLocal, started.Session.PrivateOriginalMapSnapshot.Profile);
        Assert.Equal(GameFlowStage.Exploration, started.Session.PrivateOriginalMapSnapshot.FlowStage);
        Assert.Equal(new MapId("map3"), started.Session.PrivateOriginalMapSnapshot.Map);
        Assert.Equal(new MapPosition(56, 3),
            started.Session.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(
            OriginalMapRuntimeAdmission.ControlledStartAreaRecordOrdinal,
            started.Session.PrivateOriginalMapSnapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Equal(
            OriginalMapRuntimeAdmission.ControlledStartAreaRecordOrdinal,
            started.Session.PrivateOriginalMapSnapshot.CurrentAreaDefinition
                .Identity.OneBasedRecordOrdinal);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedBlocksetResourceId,
            started.Session.PrivateOriginalMapSnapshot.CurrentBlockDefinition
                .Identity.ResourceId);
        Assert.Equal(
            0,
            started.Session.PrivateOriginalMapSnapshot.CurrentBlockDefinition
                .Identity.ZeroBasedBlockIndex);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedVisualReferenceProjectionDigest,
            started.Session.PrivateOriginalMapSnapshot.Definition.VisualResourceSelection
                .ProjectionDigest);
        Assert.Same(
            started.Session.PrivateOriginalMapSnapshot.Definition.EntityPopulation,
            started.Session.PrivateOriginalMapSnapshot.EntityPopulation);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedEntityRecordCount,
            started.Session.PrivateOriginalMapSnapshot.EntityPopulation.Records.Count);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedEntityProjectionDigest,
            started.Session.PrivateOriginalMapSnapshot.EntityPopulation.ProjectionDigest);
        Assert.Equal(0, started.Session.PrivateOriginalMapSnapshot.SimulationStep);
        Assert.Null(started.Session.PrivateOriginalMapSnapshot.LastTraversal);
        Assert.Null(started.Session.PrivateOriginalMapSnapshot.LastLayoutMutation);
        Assert.False(started.Session.PrivateOriginalMapSnapshot.ControlledStepCopyApplied);
        Assert.False(started.Session.PrivateOriginalMapSnapshot.BowieDoorStepCopyApplied);
        Assert.Null(started.Session.PrivateOriginalMapSnapshot.LastNaturalStepCopy);
        Assert.False(started.Session.PrivateOriginalMapSnapshot.SchoolDoorStepCopyApplied);
        Assert.Equal(
            PrivateOriginalMapZone601LifecyclePhase.Ready,
            started.Session.PrivateOriginalMapSnapshot.Zone601!.Phase);
        Assert.False(started.Session.PrivateOriginalMapSnapshot.Zone601.Flag601Set);
        Assert.Null(started.Session.PrivateOriginalMapSnapshot.LastZone601);
        Assert.Same(
            started.Session.PrivateOriginalMapSnapshot.Definition.WorkingLayout,
            started.Session.PrivateOriginalMapSnapshot.WorkingLayout);
        Assert.Equal(OriginalMapRuntimeAdmission.AcceptedContentDigest,
            started.Receipt.ContentDigest);
        Assert.Throws<InvalidOperationException>(() => _ = started.Session.Snapshot);
    }

    [Fact]
    public void ControlledAdmissionOwnsTheObservedDownHalfAndCounterState()
    {
        GameSession session = Start(Definition(EmptyWords()));

        PrivateOriginalMapPlayerLocomotionSnapshot animation =
            session.PrivateOriginalMapPlayerLocomotion;

        Assert.Equal(PrivateOriginalMapPlayerLocomotionPhase.Admission, animation.Phase);
        Assert.Equal(ExplorationDirection.South, animation.Direction);
        Assert.Equal((byte)3, animation.OpaqueFacing);
        Assert.Equal(PrivateOriginalMapPlayerLocomotionSheet.Down, animation.Sheet);
        Assert.Equal(2, animation.SourceSlot);
        Assert.False(animation.HorizontalMirror);
        Assert.Equal(0, animation.Tick);
        Assert.Equal(25, animation.CounterAtSelection);
        Assert.Equal(26, animation.StoredCounter);
        Assert.Equal(1, animation.SelectedHalf);
        Assert.Equal(new MapPosition(56, 3), animation.SourcePosition);
        Assert.Equal(animation.SourcePosition, animation.DestinationPosition);
        Assert.False(animation.IsMoving);
    }

    [Theory]
    [InlineData(
        ExplorationDirection.North,
        1,
        PrivateOriginalMapPlayerLocomotionSheet.Up,
        0,
        false)]
    [InlineData(
        ExplorationDirection.East,
        0,
        PrivateOriginalMapPlayerLocomotionSheet.Horizontal,
        1,
        true)]
    public void BlockedAttemptChangesFacingBeforeTheSingleSpriteTick(
        ExplorationDirection direction,
        byte facing,
        PrivateOriginalMapPlayerLocomotionSheet sheet,
        int sourceSlot,
        bool horizontalMirror)
    {
        ushort[] words = EmptyWords();
        MapPosition blocked = direction == ExplorationDirection.North
            ? new MapPosition(56, 2)
            : new MapPosition(57, 3);
        words[Index(blocked.X, blocked.Y)] = OriginalMapTraversal.CollisionMask;
        GameSession session = Start(Definition(words));

        PrivateOriginalMapPlayerLocomotionStarted started =
            session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(direction));

        Assert.Equal(OriginalMapTraversalOutcome.BlockedByCollision,
            started.Move.Traversal.Outcome);
        Assert.Equal(new MapPosition(56, 3), started.Move.Snapshot.PlayerPosition);
        Assert.Equal(PrivateOriginalMapPlayerLocomotionPhase.Blocked,
            started.Animation.Phase);
        Assert.Equal(facing, started.Animation.OpaqueFacing);
        Assert.Equal(sheet, started.Animation.Sheet);
        Assert.Equal(sourceSlot, started.Animation.SourceSlot);
        Assert.Equal(horizontalMirror, started.Animation.HorizontalMirror);
        Assert.Equal(1, started.Animation.Tick);
        Assert.Equal(26, started.Animation.CounterAtSelection);
        Assert.Equal(27, started.Animation.StoredCounter);
        Assert.Equal(1, started.Animation.SelectedHalf);
        Assert.Equal(started.Animation.SourcePosition,
            started.Animation.DestinationPosition);
        Assert.False(started.Animation.IsMoving);
    }

    [Fact]
    public void SuccessfulControlledMoveOwnsTheExactThirteenTickCadenceAndSettlement()
    {
        GameSession session = Start(Definition(EmptyWords()));

        PrivateOriginalMapPlayerLocomotionStarted started =
            session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(ExplorationDirection.West));
        List<PrivateOriginalMapPlayerLocomotionSnapshot> ticks = [started.Animation];
        while (session.PrivateOriginalMapPlayerLocomotion.IsMoving)
        {
            ticks.Add(session.AdvancePrivateOriginalMapPlayerLocomotion());
        }

        Assert.Equal(OriginalMapTraversalOutcome.Moved, started.Move.Traversal.Outcome);
        Assert.Equal(new MapPosition(55, 3), started.Move.Snapshot.PlayerPosition);
        Assert.Equal(Enumerable.Range(1, 13), ticks.Select(tick => tick.Tick));
        Assert.Equal(
            [26, 28, 30, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19],
            ticks.Select(tick => tick.CounterAtSelection));
        Assert.Equal(
            [27, 29, 0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20],
            ticks.Select(tick => tick.StoredCounter));
        Assert.Equal(
            [1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1],
            ticks.Select(tick => tick.SelectedHalf));
        Assert.Equal(
            Enumerable.Range(0, 13).Select(index => -index * 32),
            ticks.Select(tick => tick.OffsetXUnits));
        Assert.All(ticks, tick => Assert.Equal(0, tick.OffsetYUnits));
        Assert.All(ticks, tick => Assert.Equal((byte)2, tick.OpaqueFacing));
        Assert.All(ticks, tick => Assert.Equal(
            PrivateOriginalMapPlayerLocomotionSheet.Horizontal,
            tick.Sheet));
        Assert.All(ticks, tick => Assert.False(tick.HorizontalMirror));
        Assert.Equal(PrivateOriginalMapPlayerLocomotionPhase.Settled,
            ticks[^1].Phase);
        Assert.Equal(new MapPosition(55, 3), ticks[^1].DestinationPosition);
        Assert.False(ticks[^1].IsMoving);
        Assert.Equal(1, session.PrivateOriginalMapSnapshot.SimulationStep);
        Assert.Throws<InvalidOperationException>(() =>
            session.AdvancePrivateOriginalMapPlayerLocomotion());
    }

    [Fact]
    public void CandidateTargetWarpPrecedesCollisionAndRelocatesAtomically()
    {
        ushort[] words = EmptyWords();
        words[Index(
            OriginalMapRuntimeAdmission.HouseWarpTriggerX,
            OriginalMapRuntimeAdmission.HouseWarpTriggerY)] =
            OriginalMapTraversal.CollisionMask;
        FillRoofRegion(words, value: 1);
        GameSession session = Start(Definition(words));
        WorkingMapLayout admittedLayout = session.PrivateOriginalMapSnapshot.WorkingLayout;

        PrivateOriginalMapMoveApplied ordinary = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, ordinary.Traversal.Outcome);
        Assert.Null(ordinary.SameMapWarp);
        Assert.Equal(new MapPosition(55, 3), ordinary.Snapshot.PlayerPosition);

        PrivateOriginalMapPlayerLocomotionStarted relocated =
            session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(ExplorationDirection.West));

        PrivateOriginalMapSameMapWarpReceipt receipt =
            Assert.IsType<PrivateOriginalMapSameMapWarpReceipt>(
                relocated.Move.SameMapWarp);
        Assert.Equal(
            OriginalMapRuntimeAdmission.HouseWarpRecordOrdinal,
            receipt.RecordIdentity.OneBasedRecordOrdinal);
        Assert.Equal(new MapPosition(55, 3), receipt.Source);
        Assert.Equal(
            new MapPosition(
                OriginalMapRuntimeAdmission.HouseWarpTriggerX,
                OriginalMapRuntimeAdmission.HouseWarpTriggerY),
            receipt.Trigger);
        Assert.Equal(
            new MapPosition(
                OriginalMapRuntimeAdmission.HouseWarpDestinationX,
                OriginalMapRuntimeAdmission.HouseWarpDestinationY),
            receipt.Destination);
        Assert.Equal((byte)0, receipt.OpaqueFacing);
        Assert.Equal(2, receipt.SourceAreaOrdinal);
        Assert.Equal(1, receipt.DestinationAreaOrdinal);
        Assert.Equal(2, receipt.SimulationStep);
        Assert.Equal(receipt.Destination, relocated.Move.Snapshot.PlayerPosition);
        Assert.NotSame(admittedLayout, relocated.Move.Snapshot.WorkingLayout);
        AssertRoofRegion(relocated.Move.Snapshot.WorkingLayout, expected: 0);
        Assert.Null(relocated.Move.Snapshot.LastTraversal);
        Assert.Null(relocated.Move.Snapshot.LastLayoutMutation);
        Assert.Same(receipt, relocated.Move.Snapshot.LastSameMapWarp);
        PrivateOriginalMapRoofOnLoadReceipt roof =
            Assert.IsType<PrivateOriginalMapRoofOnLoadReceipt>(relocated.Move.RoofOnLoad);
        Assert.Same(roof, relocated.Move.Snapshot.LastRoofOnLoad);
        Assert.Equal(OriginalMapRuntimeAdmission.HouseRoofOnLoadRecordOrdinal,
            roof.RecordIdentity.OneBasedRecordOrdinal);
        Assert.Equal(receipt.RecordIdentity, roof.AppliedAfterWarp);
        Assert.Equal(OriginalMapRuntimeAdmission.HouseRoofDestinationAreaOrdinal,
            roof.DestinationArea.OneBasedRecordOrdinal);
        Assert.Equal(56, roof.SavedCellCount);
        Assert.True(roof.ViewUpdateRequested);
        Assert.Equal(receipt.SimulationStep, roof.SimulationStep);
        MapBlockCopyLifecycleActiveState roofState =
            Assert.IsType<MapBlockCopyLifecycleActiveState>(
                relocated.Move.Snapshot.RoofOnLoadLifecycle);
        Assert.Equal(Enumerable.Repeat((ushort)1, 56), roofState.SavedWords);
        Assert.Equal(PrivateOriginalMapPlayerLocomotionPhase.Relocated,
            relocated.Animation.Phase);
        Assert.Equal(ExplorationDirection.East, relocated.Animation.Direction);
        Assert.Equal((byte)0, relocated.Animation.OpaqueFacing);
        Assert.False(relocated.Animation.IsMoving);
        Assert.Equal(receipt.Source, relocated.Animation.SourcePosition);
        Assert.Equal(receipt.Destination, relocated.Animation.DestinationPosition);
        Assert.DoesNotContain(
            typeof(PrivateOriginalMapSameMapWarpReceipt).GetProperties(),
            property => new[] { "Path", "Payload", "Address", "Word" }
                .Any(fragment => property.Name.Contains(fragment, StringComparison.Ordinal)));
        Assert.DoesNotContain(
            typeof(PrivateOriginalMapRoofOnLoadReceipt).GetProperties(),
            property => new[] { "Path", "Payload", "Address", "Word" }
                .Any(fragment => property.Name.Contains(fragment, StringComparison.Ordinal)));
        Assert.Throws<ArgumentException>(() => new PrivateOriginalMapSameMapWarpReceipt(
            receipt.RecordIdentity,
            new MapPosition(56, 3),
            receipt.Trigger,
            receipt.Destination,
            receipt.OpaqueFacing,
            receipt.SourceAreaOrdinal,
            receipt.DestinationAreaOrdinal,
            receipt.SimulationStep));

        PrivateOriginalMapMoveApplied afterReload = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Null(afterReload.SameMapWarp);
        Assert.Null(afterReload.RoofOnLoad);
        Assert.Same(roofState, afterReload.Snapshot.RoofOnLoadLifecycle);
        AssertRoofRegion(afterReload.Snapshot.WorkingLayout, expected: 0);
    }

    [Fact]
    public void RestartClearsSameMapWarpAndNonmatchingTargetsRemainOrdinaryTraversal()
    {
        AcceptedSource source = new(Accepted());
        GameSession first = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        PrivateOriginalMapMoveApplied warped = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        Assert.NotNull(warped.SameMapWarp);

        GameSession restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        Assert.Equal(new MapPosition(56, 3),
            restarted.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(0, restarted.PrivateOriginalMapSnapshot.SimulationStep);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastSameMapWarp);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastRoofOnLoad);
        Assert.IsType<MapBlockCopyLifecycleInactiveState>(
            restarted.PrivateOriginalMapSnapshot.RoofOnLoadLifecycle);

        PrivateOriginalMapMoveApplied east = restarted.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, east.Traversal.Outcome);
        Assert.Null(east.SameMapWarp);
        Assert.Null(east.RoofOnLoad);
    }

    [Fact]
    public void BowieDoorStepCopyPrecedesCollisionAndMovesAtomicallyOnce()
    {
        ushort[] words = EmptyWords();
        words[Index(3, 3)] = OriginalMapTraversal.LeftStairMask;
        words[Index(4, 4)] = OriginalMapTraversal.LeftStairMask;
        GameSession session = Start(Definition(words));

        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        for (int count = 0; count < 3; count++)
        {
            _ = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.South));
        }

        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(4, 7), before.PlayerPosition);
        Assert.True(OriginalMapTraversal.IsBlocked(
            before.WorkingLayout,
            new MapPosition(4, 8)));
        PrivateOriginalMapMoveApplied opened = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.South));

        PrivateOriginalMapNaturalStepCopyReceipt receipt =
            Assert.IsType<PrivateOriginalMapNaturalStepCopyReceipt>(
                opened.Snapshot.LastNaturalStepCopy);
        Assert.Equal(OriginalMapTraversalOutcome.Moved, opened.Traversal.Outcome);
        Assert.Equal(new MapPosition(4, 8), opened.Snapshot.PlayerPosition);
        Assert.True(opened.Snapshot.BowieDoorStepCopyApplied);
        Assert.Equal(OriginalMapRuntimeAdmission.BowieDoorStepCopyRecordOrdinal,
            receipt.RecordIdentity.OneBasedRecordOrdinal);
        Assert.Equal(new MapPosition(4, 7), receipt.Source);
        Assert.Equal(new MapPosition(4, 8), receipt.Trigger);
        Assert.Equal((62, 0, 4, 8, 1, 1), Geometry(receipt.Copy));
        Assert.Equal(
            PrivateOriginalMapCollisionCategory.BlockedByAcceptedCollisionClass,
            receipt.BeforeCollision);
        Assert.Equal(
            PrivateOriginalMapCollisionCategory.ActiveNonBlocked,
            receipt.AfterCollision);
        Assert.Equal(opened.Snapshot.SimulationStep, receipt.SimulationStep);
        Assert.NotSame(before.WorkingLayout, opened.Snapshot.WorkingLayout);
        Assert.Equal(
            opened.Snapshot.WorkingLayout[62, 0],
            opened.Snapshot.WorkingLayout[4, 8]);

        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        WorkingMapLayout openedLayout = session.PrivateOriginalMapSnapshot.WorkingLayout;
        PrivateOriginalMapMoveApplied returned = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.South));
        Assert.True(returned.Snapshot.BowieDoorStepCopyApplied);
        Assert.Null(returned.Snapshot.LastNaturalStepCopy);
        Assert.Same(openedLayout, returned.Snapshot.WorkingLayout);
        Assert.DoesNotContain(
            typeof(PrivateOriginalMapNaturalStepCopyReceipt).GetProperties(),
            property => new[] { "Path", "Payload", "Address", "Word" }
                .Any(fragment => property.Name.Contains(fragment, StringComparison.Ordinal)));
    }

    [Fact]
    public void RestartRestoresTheClosedBowieDoorAndClearsItsNaturalReceipt()
    {
        AcceptedSource source = new(Accepted());
        GameSession first = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        _ = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        _ = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        _ = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        for (int count = 0; count < 4; count++)
        {
            _ = first.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.South));
        }

        Assert.True(first.PrivateOriginalMapSnapshot.BowieDoorStepCopyApplied);
        Assert.NotNull(first.PrivateOriginalMapSnapshot.LastNaturalStepCopy);

        GameSession restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        Assert.False(restarted.PrivateOriginalMapSnapshot.BowieDoorStepCopyApplied);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastNaturalStepCopy);
        Assert.True(OriginalMapTraversal.IsBlocked(
            restarted.PrivateOriginalMapSnapshot.WorkingLayout,
            new MapPosition(4, 8)));
    }

    [Fact]
    public void BowieDoorDoesNotGeneralizeBeyondTheAcceptedSouthApproach()
    {
        GameSession session = Start(Definition(EmptyWords()));
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        for (int count = 0; count < 4; count++)
        {
            _ = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.South));
        }

        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(5, 8), before.PlayerPosition);
        PrivateOriginalMapMoveApplied blocked = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));

        Assert.Equal(OriginalMapTraversalOutcome.BlockedByCollision, blocked.Traversal.Outcome);
        Assert.False(blocked.Snapshot.BowieDoorStepCopyApplied);
        Assert.Null(blocked.Snapshot.LastNaturalStepCopy);
        Assert.Same(before.WorkingLayout, blocked.Snapshot.WorkingLayout);
    }

    [Fact]
    public void SchoolDoorNaturalStepCopyUsesItsExactNorthApproachAndAppliesOnce()
    {
        GameSession session = Start(Definition(EmptyWords()));
        MoveToSchoolDoorApproach(session);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(41, 14), before.PlayerPosition);
        Assert.True(OriginalMapTraversal.IsBlocked(
            before.WorkingLayout,
            new MapPosition(41, 13)));

        PrivateOriginalMapMoveApplied opened = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        PrivateOriginalMapNaturalStepCopyReceipt receipt =
            Assert.IsType<PrivateOriginalMapNaturalStepCopyReceipt>(
                opened.Snapshot.LastNaturalStepCopy);

        Assert.Equal(OriginalMapTraversalOutcome.Moved, opened.Traversal.Outcome);
        Assert.Equal(new MapPosition(41, 13), opened.Snapshot.PlayerPosition);
        Assert.True(opened.Snapshot.SchoolDoorStepCopyApplied);
        Assert.False(opened.Snapshot.ControlledStepCopyApplied);
        Assert.Equal(opened.Snapshot.Definition.ControlledStepCopy!.Identity,
            receipt.RecordIdentity);
        Assert.Equal(new MapPosition(41, 14), receipt.Source);
        Assert.Equal(new MapPosition(41, 13), receipt.Trigger);
        Assert.Equal((62, 0, 41, 13, 1, 1), Geometry(receipt.Copy));
        Assert.Equal(
            PrivateOriginalMapCollisionCategory.BlockedByAcceptedCollisionClass,
            receipt.BeforeCollision);
        Assert.Equal(
            PrivateOriginalMapCollisionCategory.ActiveNonBlocked,
            receipt.AfterCollision);
        Assert.NotSame(before.WorkingLayout, opened.Snapshot.WorkingLayout);

        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.South));
        WorkingMapLayout openedLayout = session.PrivateOriginalMapSnapshot.WorkingLayout;
        PrivateOriginalMapMoveApplied revisited = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, revisited.Traversal.Outcome);
        Assert.True(revisited.Snapshot.SchoolDoorStepCopyApplied);
        Assert.Null(revisited.Snapshot.LastNaturalStepCopy);
        Assert.Same(openedLayout, revisited.Snapshot.WorkingLayout);

        foreach (ExplorationDirection direction in new[]
        {
            ExplorationDirection.North,
            ExplorationDirection.North,
            ExplorationDirection.North,
            ExplorationDirection.North,
            ExplorationDirection.North,
            ExplorationDirection.North,
            ExplorationDirection.East,
            ExplorationDirection.East,
            ExplorationDirection.East,
            ExplorationDirection.East,
            ExplorationDirection.East,
        })
        {
            _ = session.ApplyPrivateOriginalMap(new MoveExplorationCommand(direction));
        }

        Assert.Equal(new MapPosition(59, 12), session.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.True(session.PrivateOriginalMapSnapshot.SchoolDoorStepCopyApplied);
        Assert.Same(openedLayout, session.PrivateOriginalMapSnapshot.WorkingLayout);
    }

    [Fact]
    public void SchoolDoorDoesNotGeneralizeToTheNorthSideOrConflateTheControlledDiagnostic()
    {
        GameSession session = Start(Definition(EmptyWords()));
        for (int count = 0; count < 9; count++)
        {
            _ = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.South));
        }

        for (int count = 0; count < 15; count++)
        {
            _ = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.West));
        }

        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(41, 12), before.PlayerPosition);
        PrivateOriginalMapMoveApplied blocked = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.South));
        Assert.Equal(OriginalMapTraversalOutcome.BlockedByCollision, blocked.Traversal.Outcome);
        Assert.False(blocked.Snapshot.SchoolDoorStepCopyApplied);
        Assert.False(blocked.Snapshot.ControlledStepCopyApplied);
        Assert.Null(blocked.Snapshot.LastNaturalStepCopy);
        Assert.Same(before.WorkingLayout, blocked.Snapshot.WorkingLayout);

        PrivateOriginalMapLayoutMutationApplied diagnostic =
            Assert.IsType<PrivateOriginalMapLayoutMutationApplied>(
                session.ApplyPrivateOriginalMapLayoutMutation(
                    MutationCommand(blocked.Snapshot)));
        Assert.True(diagnostic.Snapshot.ControlledStepCopyApplied);
        Assert.False(diagnostic.Snapshot.SchoolDoorStepCopyApplied);
    }

    [Fact]
    public void FirstHouseWarpThenSlopeCandidateRunsZone601ExactlyOnceAndPersistsAcrossWarp()
    {
        GameSession session = Start(Definition(EmptyWords()));
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        PrivateOriginalMapMoveApplied house = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        Assert.NotNull(house.SameMapWarp);
        Assert.Equal(new MapPosition(3, 3), house.Snapshot.PlayerPosition);

        PrivateOriginalMapMoveApplied intercepted = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));

        Assert.Equal(OriginalMapTraversalOutcome.Moved, intercepted.Traversal.Outcome);
        Assert.Equal(new MapPosition(3, 3), intercepted.Traversal.Source);
        Assert.Equal(new MapPosition(4, 4), intercepted.Snapshot.PlayerPosition);
        PrivateOriginalMapZone601Receipt receipt =
            Assert.IsType<PrivateOriginalMapZone601Receipt>(intercepted.Zone601);
        Assert.Same(receipt, intercepted.Snapshot.LastZone601);
        Assert.Equal(OriginalMapRuntimeAdmission.Zone601RecordOrdinal,
            receipt.EventIdentity.OneBasedRecordOrdinal);
        Assert.Equal(OriginalMapRuntimeAdmission.Zone601TargetIdentity,
            receipt.EventIdentity.TargetIdentity);
        Assert.Equal(OriginalMapRuntimeAdmission.Zone601GateFlag, receipt.GateFlag);
        Assert.Equal(OriginalMapRuntimeAdmission.Zone601BlockingSequenceIdentity,
            receipt.BlockingSequenceIdentity);
        Assert.Equal(OriginalMapRuntimeAdmission.Zone601TextIds, receipt.TextIds);
        Assert.Equal(
            OriginalMapRuntimeAdmission.Zone601BlockingStages,
            receipt.BlockingStages);
        Assert.Equal(receipt.SimulationStep, intercepted.Snapshot.SimulationStep);

        PrivateOriginalMapZone601State completed = intercepted.Snapshot.Zone601!;
        Assert.Equal(
            PrivateOriginalMapZone601LifecyclePhase.AmbientWalkingHandoff,
            completed.Phase);
        Assert.True(completed.Flag601Set);
        Assert.Equal(OriginalMapRuntimeAdmission.Zone601LogicalActorId,
            completed.LogicalActorId);
        Assert.Equal(new MapPosition(5, 4), completed.ActorPosition);
        Assert.Equal((byte)2, completed.ActorOpaqueFacing);
        Assert.Equal(OriginalMapRuntimeAdmission.Zone601AmbientBehaviorIdentity,
            completed.ActorBehaviorIdentity);
        Assert.Equal(new MapPosition(5, 6), completed.AmbientCenter);
        Assert.Equal(1, completed.AmbientRange);

        PrivateOriginalMapMoveApplied back = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        Assert.Null(back.Zone601);
        PrivateOriginalMapMoveApplied reentered = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Null(reentered.Zone601);
        Assert.True(reentered.Snapshot.Zone601!.Flag601Set);
        Assert.Null(reentered.Snapshot.LastZone601);

        for (int count = 0; count < 51; count++)
        {
            _ = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.East));
        }

        PrivateOriginalMapMoveApplied returned = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        Assert.Null(returned.SameMapWarp);
        returned = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        Assert.NotNull(returned.SameMapWarp);
        Assert.True(returned.Snapshot.Zone601!.Flag601Set);
    }

    [Fact]
    public void RestartRestoresTheAcceptedZone601ActorAndClearsItsFlagAndReceipt()
    {
        AcceptedSource source = new(Accepted());
        GameSession first = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        _ = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        _ = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        _ = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.True(first.PrivateOriginalMapSnapshot.Zone601!.Flag601Set);

        GameSession restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        PrivateOriginalMapZone601State ready = restarted.PrivateOriginalMapSnapshot.Zone601!;
        Assert.Equal(PrivateOriginalMapZone601LifecyclePhase.Ready, ready.Phase);
        Assert.False(ready.Flag601Set);
        Assert.Equal(new MapPosition(5, 6), ready.ActorPosition);
        Assert.Equal((byte)0, ready.ActorOpaqueFacing);
        Assert.Equal(OriginalMapRuntimeAdmission.Zone601ActorInitialBehaviorIdentity,
            ready.ActorBehaviorIdentity);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastZone601);
    }

    [Fact]
    public void SarahOccupiesHerLiveTileAndFirstInteractionAppliesTheAcceptedRouteAtomically()
    {
        GameSession session = Start(Definition(EmptyWords()));
        MoveToSarahInteraction(session);

        PrivateOriginalMapPlayerLocomotionStarted blocked =
            session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(ExplorationDirection.North));
        Assert.Equal(
            OriginalMapTraversalOutcome.BlockedByOccupiedEntity,
            blocked.Move.Traversal.Outcome);
        Assert.Equal(new MapPosition(42, 9), blocked.Move.Snapshot.PlayerPosition);
        Assert.Equal((byte)1, blocked.Animation.OpaqueFacing);

        long step = session.PrivateOriginalMapSnapshot.SimulationStep;
        PrivateOriginalMapSarahInteractionApplied applied =
            Assert.IsType<PrivateOriginalMapSarahInteractionApplied>(
                session.InteractPrivateOriginalMapSarah(
                    new InteractPrivateOriginalMapSarahCommand(step)));

        Assert.Equal(new[] { 512, 480, 481 }, applied.Receipt.TextIds);
        Assert.Equal(OriginalMapRuntimeAdmission.SarahFirstStages, applied.Receipt.Stages);
        Assert.False(applied.Receipt.Repeated);
        Assert.False(applied.Receipt.LaterBranchFlag603Set);
        Assert.False(applied.Receipt.LaterBranchFlag602Set);
        Assert.Equal("cs_513D6", applied.Receipt.BlockingSequenceIdentity);
        Assert.Equal(new MapPosition(41, 7), applied.Snapshot.Sarah!.ActorPosition);
        Assert.Equal((byte)3, applied.Snapshot.Sarah.ActorOpaqueFacing);
        Assert.True(applied.Snapshot.Sarah.TemporaryRouteFlag256Set);
        Assert.Same(applied.Receipt, applied.Snapshot.LastSarah);
    }

    [Fact]
    public void Entity142RequestAndAcknowledgementApplyTheExactFirstAndRepeatEffects()
    {
        GameSession session = Start(Definition(EmptyWords()));
        PrivateOriginalMapSessionSnapshot initial = session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapEntity142InteractionSelectionRejected wrongTarget =
            Assert.IsType<PrivateOriginalMapEntity142InteractionSelectionRejected>(
                session.RequestPrivateOriginalMapInteraction(initial.SimulationStep));
        Assert.Equal(
            PrivateOriginalMapEntity142RequestFailureCode.InteractionTargetMismatch,
            wrongTarget.Rejected.Diagnostic.Code);
        Assert.Same(initial, wrongTarget.Rejected.Snapshot);
        Assert.Same(initial, session.PrivateOriginalMapSnapshot);

        MoveToEntity142Interaction(session);
        PrivateOriginalMapSessionSnapshot beforeRequest = session.PrivateOriginalMapSnapshot;
        Assert.NotNull(beforeRequest.Entity142);
        Assert.False(beforeRequest.Entity142.Flag261Set);
        Assert.False(beforeRequest.Entity142.Flag602Set);

        PrivateOriginalMapEntity142InteractionRequested selected =
            Assert.IsType<PrivateOriginalMapEntity142InteractionRequested>(
                session.RequestPrivateOriginalMapInteraction(beforeRequest.SimulationStep));
        PrivateOriginalMapEntity142Request request = selected.Applied.Request;
        Assert.Same(request, selected.Applied.Snapshot.PendingEntity142);
        Assert.Same(request, selected.Applied.Snapshot.LastEntity142Request);
        Assert.Equal(1, request.RequestSequence);
        Assert.False(request.Repeated);
        Assert.Equal(OriginalMapRuntimeAdmission.Entity142FirstTextIds, request.TextIds);
        Assert.Equal(OriginalMapRuntimeAdmission.Entity142FirstStages, request.Stages);
        Assert.Equal(OriginalMapRuntimeAdmission.Entity142EventTargetIdentity,
            request.EventIdentity.TargetIdentity);
        Assert.False(selected.Applied.Snapshot.Entity142!.Flag261Set);

        PrivateOriginalMapSessionSnapshot pending = session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapEntity142InteractionSelectionRejected duplicateRequest =
            Assert.IsType<PrivateOriginalMapEntity142InteractionSelectionRejected>(
                session.RequestPrivateOriginalMapInteraction(pending.SimulationStep));
        Assert.Equal(
            PrivateOriginalMapEntity142RequestFailureCode.PendingRequestExists,
            duplicateRequest.Rejected.Diagnostic.Code);
        Assert.Same(pending, duplicateRequest.Rejected.Snapshot);

        PrivateOriginalMapEntity142AcknowledgementRejected wrong =
            Assert.IsType<PrivateOriginalMapEntity142AcknowledgementRejected>(
                session.AcknowledgePrivateOriginalMapEntity142(
                    new AcknowledgePrivateOriginalMapEntity142Command(
                        pending.SimulationStep,
                        request.RequestSequence + 1,
                        request.EventIdentity)));
        Assert.Equal(
            PrivateOriginalMapEntity142AcknowledgementFailureCode.ReferenceMismatch,
            wrong.Diagnostic.Code);
        Assert.Same(pending, wrong.Snapshot);
        Assert.Same(pending, session.PrivateOriginalMapSnapshot);

        OriginalMapEntity142EventIdentity wrongIdentity = new(
            request.EventIdentity.Profile,
            request.EventIdentity.Map,
            request.EventIdentity.Setup,
            request.EventIdentity.ResourceId,
            request.EventIdentity.OneBasedRecordOrdinal,
            "project-authored-wrong-target",
            request.EventIdentity.OpaqueEventFacing);
        PrivateOriginalMapEntity142AcknowledgementRejected wrongEvent =
            Assert.IsType<PrivateOriginalMapEntity142AcknowledgementRejected>(
                session.AcknowledgePrivateOriginalMapEntity142(
                    new AcknowledgePrivateOriginalMapEntity142Command(
                        pending.SimulationStep,
                        request.RequestSequence,
                        wrongIdentity)));
        Assert.Equal(
            PrivateOriginalMapEntity142AcknowledgementFailureCode.ReferenceMismatch,
            wrongEvent.Diagnostic.Code);
        Assert.Same(pending, wrongEvent.Snapshot);

        PrivateOriginalMapEntity142AcknowledgementApplied acknowledged =
            Assert.IsType<PrivateOriginalMapEntity142AcknowledgementApplied>(
                session.AcknowledgePrivateOriginalMapEntity142(
                    new AcknowledgePrivateOriginalMapEntity142Command(
                        pending.SimulationStep,
                        request.RequestSequence,
                        request.EventIdentity)));
        Assert.True(acknowledged.Snapshot.Entity142!.Flag261Set);
        Assert.True(acknowledged.Snapshot.Entity142.Flag602Set);
        Assert.Null(acknowledged.Snapshot.PendingEntity142);
        Assert.Same(acknowledged.Receipt, acknowledged.Snapshot.LastEntity142Acknowledgement);
        Assert.Same(request, acknowledged.Receipt.Request);
        Assert.Equal(OriginalMapRuntimeAdmission.Entity142FirstInteractionFlag261,
            acknowledged.Receipt.FirstInteractionFlag261);
        Assert.Equal(OriginalMapRuntimeAdmission.Entity142CompletionFlag602,
            acknowledged.Receipt.CompletionFlag602);

        PrivateOriginalMapEntity142AcknowledgementRejected duplicate =
            Assert.IsType<PrivateOriginalMapEntity142AcknowledgementRejected>(
                session.AcknowledgePrivateOriginalMapEntity142(
                    new AcknowledgePrivateOriginalMapEntity142Command(
                        acknowledged.Snapshot.SimulationStep,
                        request.RequestSequence,
                        request.EventIdentity)));
        Assert.Equal(
            PrivateOriginalMapEntity142AcknowledgementFailureCode.NoPendingRequest,
            duplicate.Diagnostic.Code);
        Assert.Same(acknowledged.Snapshot, duplicate.Snapshot);

        PrivateOriginalMapEntity142InteractionRequested repeat =
            Assert.IsType<PrivateOriginalMapEntity142InteractionRequested>(
                session.RequestPrivateOriginalMapInteraction(
                    session.PrivateOriginalMapSnapshot.SimulationStep));
        Assert.True(repeat.Applied.Request.Repeated);
        Assert.Equal(2, repeat.Applied.Request.RequestSequence);
        Assert.Equal(OriginalMapRuntimeAdmission.Entity142RepeatTextIds,
            repeat.Applied.Request.TextIds);
        Assert.Equal(OriginalMapRuntimeAdmission.Entity142RepeatStages,
            repeat.Applied.Request.Stages);
        PrivateOriginalMapEntity142AcknowledgementApplied repeated =
            Assert.IsType<PrivateOriginalMapEntity142AcknowledgementApplied>(
                session.AcknowledgePrivateOriginalMapEntity142(
                    new AcknowledgePrivateOriginalMapEntity142Command(
                        repeat.Applied.Snapshot.SimulationStep,
                        repeat.Applied.Request.RequestSequence,
                        repeat.Applied.Request.EventIdentity)));
        Assert.True(repeated.Snapshot.Entity142!.Flag261Set);
        Assert.True(repeated.Snapshot.Entity142.Flag602Set);
        Assert.Equal(2, repeated.Snapshot.Entity142.LastAcknowledgedRequestSequence);
    }

    [Fact]
    public void Entity142OccupiesItsTileAndOtherMutationClearsPendingWithoutLosingFlags()
    {
        GameSession session = Start(Definition(EmptyWords()));
        MoveToEntity142Interaction(session);
        PrivateOriginalMapSessionSnapshot beforeBlock = session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapMoveApplied blocked = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        Assert.Equal(OriginalMapTraversalOutcome.BlockedByOccupiedEntity,
            blocked.Traversal.Outcome);
        Assert.Equal(beforeBlock.PlayerPosition, blocked.Snapshot.PlayerPosition);

        PrivateOriginalMapEntity142RequestApplied requested =
            Assert.IsType<PrivateOriginalMapEntity142RequestApplied>(
                session.RequestPrivateOriginalMapEntity142(
                    new RequestPrivateOriginalMapEntity142Command(
                        session.PrivateOriginalMapSnapshot.SimulationStep)));
        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Null(moved.Snapshot.PendingEntity142);
        Assert.Null(moved.Snapshot.LastEntity142Request);
        Assert.False(moved.Snapshot.Entity142!.Flag261Set);

        PrivateOriginalMapEntity142AcknowledgementRejected stale =
            Assert.IsType<PrivateOriginalMapEntity142AcknowledgementRejected>(
                session.AcknowledgePrivateOriginalMapEntity142(
                    new AcknowledgePrivateOriginalMapEntity142Command(
                        requested.Snapshot.SimulationStep,
                        requested.Request.RequestSequence,
                        requested.Request.EventIdentity)));
        Assert.Equal(
            PrivateOriginalMapEntity142AcknowledgementFailureCode.StaleSimulationStep,
            stale.Diagnostic.Code);
        Assert.Same(moved.Snapshot, stale.Snapshot);
    }

    [Fact]
    public void Entity142CompletionPersistsButUnsupportedFlag602SarahBranchDoesNotRun()
    {
        GameSession session = Start(Definition(EmptyWords()));
        MoveToEntity142Interaction(session);
        PrivateOriginalMapEntity142RequestApplied requested =
            Assert.IsType<PrivateOriginalMapEntity142RequestApplied>(
                session.RequestPrivateOriginalMapEntity142(
                    new RequestPrivateOriginalMapEntity142Command(
                        session.PrivateOriginalMapSnapshot.SimulationStep)));
        _ = Assert.IsType<PrivateOriginalMapEntity142AcknowledgementApplied>(
            session.AcknowledgePrivateOriginalMapEntity142(
                new AcknowledgePrivateOriginalMapEntity142Command(
                    requested.Snapshot.SimulationStep,
                    requested.Request.RequestSequence,
                    requested.Request.EventIdentity)));

        Move(session, ExplorationDirection.North, 1);
        Move(session, ExplorationDirection.West, 13);
        Move(session, ExplorationDirection.North, 7);
        PrivateOriginalMapPlayerLocomotionStarted sarahFacing =
            session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(ExplorationDirection.North));
        Assert.Equal(OriginalMapTraversalOutcome.BlockedByOccupiedEntity,
            sarahFacing.Move.Traversal.Outcome);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(42, 9), before.PlayerPosition);
        Assert.True(before.Entity142!.Flag602Set);
        PrivateOriginalMapSarahInteractionSelectionRejected rejected =
            Assert.IsType<PrivateOriginalMapSarahInteractionSelectionRejected>(
                session.RequestPrivateOriginalMapInteraction(before.SimulationStep));
        Assert.Equal(
            PrivateOriginalMapSarahInteractionFailureCode.UnsupportedLaterBranchState,
            rejected.Rejected.Diagnostic.Code);
        Assert.Same(before, rejected.Rejected.Snapshot);

        GameSession restarted = Start(Definition(EmptyWords()));
        Assert.False(restarted.PrivateOriginalMapSnapshot.Entity142!.Flag261Set);
        Assert.False(restarted.PrivateOriginalMapSnapshot.Entity142.Flag602Set);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.PendingEntity142);
    }

    [Fact]
    public void AcceptedRouteEntersAstralZoneOnceAndAtomicallyRepositionsLiveActors()
    {
        GameSession session = Start(Definition(EmptyWords()));
        CompleteRouteThroughEntity142(session);

        Move(session, ExplorationDirection.North, 4);
        Move(session, ExplorationDirection.East, 2);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(57, 13), before.PlayerPosition);
        Assert.False(before.AstralZoneFlag260Set);

        PrivateOriginalMapMoveApplied applied = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));

        PrivateOriginalMapAstralZoneReceipt receipt =
            Assert.IsType<PrivateOriginalMapAstralZoneReceipt>(applied.AstralZone);
        Assert.Same(receipt, applied.Snapshot.LastAstralZone);
        Assert.Equal(new MapPosition(58, 13), applied.Snapshot.PlayerPosition);
        Assert.Equal("Map3_ZoneEvent7", receipt.EventIdentity.TargetIdentity);
        Assert.Equal("cs_5148C", receipt.PositionProgramIdentity);
        Assert.Equal(new[] { 514, 515, 516 }, receipt.TextIds);
        Assert.Equal(OriginalMapRuntimeAdmission.AstralZoneStages, receipt.Stages);
        Assert.False(receipt.MessengerCompletionFlag603Set);
        Assert.True(receipt.RequiredEntity142Flag602Set);
        Assert.True(receipt.CompletionFlag260Set);
        Assert.True(applied.Snapshot.AstralZoneFlag260Set);
        Assert.Equal(
            PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned,
            applied.Snapshot.Sarah!.Phase);
        Assert.Equal(new MapPosition(41, 10), applied.Snapshot.Sarah.ActorPosition);
        Assert.Equal((byte)1, applied.Snapshot.Sarah.ActorOpaqueFacing);
        Assert.Equal(
            PrivateOriginalMapZone601LifecyclePhase.AstralZoneRepositioned,
            applied.Snapshot.Zone601!.Phase);
        Assert.Equal(new MapPosition(6, 4), applied.Snapshot.Zone601.ActorPosition);
        Assert.Equal((byte)1, applied.Snapshot.Zone601.ActorOpaqueFacing);

        PrivateOriginalMapMoveApplied left = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        PrivateOriginalMapMoveApplied revisited = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Null(left.AstralZone);
        Assert.Null(revisited.AstralZone);
        Assert.True(revisited.Snapshot.AstralZoneFlag260Set);
        Assert.Equal(new MapPosition(41, 10), revisited.Snapshot.Sarah!.ActorPosition);
        Assert.Equal(new MapPosition(6, 4), revisited.Snapshot.Zone601!.ActorPosition);
    }

    [Fact]
    public void AstralZoneRequiresTheCompletedAcceptedRouteAndRestartClearsIt()
    {
        GameSession incomplete = Start(Definition(EmptyWords()));
        Move(incomplete, ExplorationDirection.South, 10);
        Move(incomplete, ExplorationDirection.East, 1);
        PrivateOriginalMapMoveApplied ordinary = incomplete.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Null(ordinary.AstralZone);
        Assert.False(ordinary.Snapshot.AstralZoneFlag260Set);

        GameSession completed = Start(Definition(EmptyWords()));
        CompleteRouteThroughEntity142(completed);
        Move(completed, ExplorationDirection.North, 4);
        Move(completed, ExplorationDirection.East, 3);
        Assert.True(completed.PrivateOriginalMapSnapshot.AstralZoneFlag260Set);

        GameSession restarted = Start(Definition(EmptyWords()));
        Assert.False(restarted.PrivateOriginalMapSnapshot.AstralZoneFlag260Set);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastAstralZone);
        Assert.Equal(new MapPosition(42, 8), restarted.PrivateOriginalMapSnapshot.Sarah!.ActorPosition);
        Assert.Equal(new MapPosition(5, 6), restarted.PrivateOriginalMapSnapshot.Zone601!.ActorPosition);
    }

    [Fact]
    public void AcceptedMessengerRouteAtomicallyJoinsForceAndReleasesLiveRouteActors()
    {
        GameSession session = Start(Definition(EmptyWords()));
        Assert.False(session.PrivateOriginalMapSnapshot.MessengerAcceptance!.Accepted);

        CompleteRouteThroughEntity142(session);
        Move(session, ExplorationDirection.North, 4);
        Move(session, ExplorationDirection.East, 3);
        Assert.True(session.PrivateOriginalMapSnapshot.AstralZoneFlag260Set);
        Move(session, ExplorationDirection.West, 16);
        Move(session, ExplorationDirection.North, 3);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(42, 10), before.PlayerPosition);

        PrivateOriginalMapPlayerLocomotionStarted started =
            session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(ExplorationDirection.East));
        PrivateOriginalMapMessengerAcceptanceReceipt receipt =
            Assert.IsType<PrivateOriginalMapMessengerAcceptanceReceipt>(
                started.Move.MessengerAcceptance);

        Assert.Same(receipt, started.Move.Snapshot.LastMessengerAcceptance);
        Assert.Equal(new MapPosition(43, 10), started.Move.Snapshot.PlayerPosition);
        Assert.Equal("Map3_ZoneEvent8", receipt.EventIdentity.TargetIdentity);
        Assert.Equal("cs_5149A", receipt.MessengerProgramIdentity);
        Assert.Equal("cs_51614", receipt.AcceptedBranchProgramIdentity);
        Assert.Equal(OriginalMapRuntimeAdmission.MessengerControlShapeSha256,
            receipt.ControlShapeSha256);
        Assert.Equal(0, receipt.PromptReturn);
        Assert.Equal(89, receipt.PromptFlag89);
        Assert.True(receipt.PromptFlag89Set);
        Assert.Equal(new[] { 517, 518, 519, 520, 521, 522, 523, 524, 525, 526, 527,
            528, 529, 530, 531, 535, 536, 447 }, receipt.TextIds);
        Assert.Equal(OriginalMapRuntimeAdmission.MessengerSpeakerOperands,
            receipt.SpeakerOperands);
        Assert.Equal(OriginalMapRuntimeAdmission.MessengerStages, receipt.Stages);
        Assert.True(receipt.Flag600Set);
        Assert.True(receipt.Flag66Set);
        Assert.True(receipt.CompletionFlag603Set);
        Assert.Equal("WaitForEvent", receipt.TerminalIdentity);

        PrivateOriginalMapMessengerAcceptanceState completed =
            started.Move.Snapshot.MessengerAcceptance!;
        Assert.True(completed.Accepted);
        Assert.Equal(new[] { 1, 2 }, completed.JoinedCharacterIds);
        Assert.Equal(new[] { (1, 0, 2), (2, 1, 2) },
            completed.Followers.Select(link =>
                (link.FollowerId, link.LeaderId, link.Distance)));
        Assert.Equal(new[] { (138, 27, 3, (byte)3), (139, 31, 3, (byte)3) },
            completed.Guards.Select(guard =>
                (guard.LogicalActorId, guard.Position.X, guard.Position.Y,
                    guard.OpaqueFacing)));
        Assert.True(started.Move.Snapshot.Sarah!.IsMessengerFollowerReady);
        Assert.False(started.Move.Snapshot.Sarah.OccupiesRouteTile);
        Assert.True(started.Move.Snapshot.Entity142!.RouteOccupancyReleased);
        Assert.False(started.Move.Snapshot.Entity142.OccupiesRouteTile);
        Assert.Equal(PrivateOriginalMapPlayerLocomotionPhase.ScriptedEndpoint,
            started.Animation.Phase);
        Assert.Equal(ExplorationDirection.South, started.Animation.Direction);
        Assert.Equal((byte)3, started.Animation.OpaqueFacing);
        Assert.Equal(PrivateOriginalMapPlayerLocomotionSheet.Down,
            started.Animation.Sheet);

        PrivateOriginalMapMoveApplied left = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        PrivateOriginalMapMoveApplied revisited = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Null(left.MessengerAcceptance);
        Assert.Null(revisited.MessengerAcceptance);
        Assert.True(revisited.Snapshot.MessengerAcceptance!.Accepted);

        GameSession restarted = Start(Definition(EmptyWords()));
        Assert.False(restarted.PrivateOriginalMapSnapshot.MessengerAcceptance!.Accepted);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastMessengerAcceptance);
        Assert.True(restarted.PrivateOriginalMapSnapshot.Sarah!.OccupiesRouteTile);
        Assert.True(restarted.PrivateOriginalMapSnapshot.Entity142!.OccupiesRouteTile);
    }

    [Fact]
    public void MessengerTriggerWithoutTheAcceptedPreconditionsRemainsOrdinaryTraversal()
    {
        GameSession session = Start(Definition(EmptyWords()));
        Move(session, ExplorationDirection.South, 7);
        Move(session, ExplorationDirection.West, 14);
        Assert.Equal(new MapPosition(42, 10), session.PrivateOriginalMapSnapshot.PlayerPosition);

        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));

        Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
        Assert.Null(moved.MessengerAcceptance);
        Assert.False(moved.Snapshot.MessengerAcceptance!.Accepted);
        Assert.Same(before.Sarah, moved.Snapshot.Sarah);
        Assert.Same(before.Entity142, moved.Snapshot.Entity142);
    }

    [Fact]
    public void AcceptedCastleGateRouteOpensOnceAndRetainsItsTypedAtomicReceipt()
    {
        GameSession session = Start(Definition(EmptyWords()));
        CompleteRouteThroughMessenger(session);
        MoveToCastleGateApproach(session);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        Assert.False(before.CastleGate!.Opened);
        Assert.Equal(new MapPosition(31, 6), before.PlayerPosition);

        PrivateOriginalMapMoveApplied opened = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        PrivateOriginalMapCastleGateReceipt receipt =
            Assert.IsType<PrivateOriginalMapCastleGateReceipt>(opened.CastleGate);

        Assert.Same(receipt, opened.Snapshot.LastCastleGate);
        Assert.Same(receipt.Traversal, opened.Traversal);
        Assert.Equal(new MapPosition(31, 5), opened.Snapshot.PlayerPosition);
        Assert.True(opened.Snapshot.CastleGate!.Opened);
        Assert.True(opened.Snapshot.CastleGate.Flag604Set);
        Assert.Equal("Map3_ZoneEvent4", receipt.EventIdentity.TargetIdentity);
        Assert.Equal("cs_51652", receipt.ProgramIdentity);
        Assert.Equal(OriginalMapRuntimeAdmission.CastleGateControlShapeSha256,
            receipt.ControlShapeSha256);
        Assert.Equal(537, receipt.TextCursorId);
        Assert.Equal(604, receipt.CompletionFlag);
        Assert.Equal(26, receipt.SourceOperationCount);
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 25 },
            receipt.ProjectionSourceOperationIndices);
        Assert.Equal(
            new[] { (138, 28, 3), (139, 30, 3) },
            receipt.GuardMoves.Select(move =>
                (move.LogicalActorId, move.Destination.X, move.Destination.Y)));
        Assert.Equal(OriginalMapRuntimeAdmission.CastleGateStages, receipt.Stages);
        Assert.Same(before.CastleGate, receipt.Before);
        Assert.Same(opened.Snapshot.CastleGate, receipt.After);

        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.South));
        PrivateOriginalMapMoveApplied revisited = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        Assert.Null(revisited.CastleGate);
        Assert.True(revisited.Snapshot.CastleGate!.Opened);
        Assert.Null(revisited.Snapshot.LastCastleGate);

        GameSession restarted = Start(Definition(EmptyWords()));
        Assert.False(restarted.PrivateOriginalMapSnapshot.CastleGate!.Opened);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastCastleGate);
    }

    [Fact]
    public void OpenCastleGateAdmitsExactNorthMap19RuntimeAndRestartReturnsToMap3()
    {
        GameSession session = Start(Definition(EmptyWords()));
        CompleteRouteThroughMessenger(session);
        MoveToCastleGateApproach(session);
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        Move(session, ExplorationDirection.West, 3);
        Move(session, ExplorationDirection.North, 3);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        Assert.Equal(
            new MapPosition(
                OriginalMapRuntimeAdmission.NorthMap19WarpApproachX,
                OriginalMapRuntimeAdmission.NorthMap19WarpApproachY),
            before.PlayerPosition);
        Assert.True(before.CastleGate!.Opened);

        PrivateOriginalMapPlayerLocomotionStarted relocated =
            session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(ExplorationDirection.North));
        PrivateOriginalMapMoveApplied applied = relocated.Move;
        PrivateOriginalMapCrossMapTransitionReceipt transition =
            Assert.IsType<PrivateOriginalMapCrossMapTransitionReceipt>(
                applied.CrossMapTransition);

        Assert.Same(transition, applied.Snapshot.LastCrossMapTransition);
        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.Map19Id), applied.Snapshot.Map);
        Assert.Same(
            applied.Snapshot.Definition.RuntimeCatalog.Resolve(
                new MapId(OriginalMapRuntimeAdmission.Map19Id)),
            applied.Snapshot.CurrentRuntime);
        Assert.Same(
            applied.Snapshot.CurrentRuntime.WorkingLayout,
            applied.Snapshot.WorkingLayout);
        Assert.Equal(
            new MapPosition(
                OriginalMapRuntimeAdmission.NorthMap19WarpDestinationX,
                OriginalMapRuntimeAdmission.NorthMap19WarpDestinationY),
            applied.Snapshot.PlayerPosition);
        Assert.Equal(1, applied.Snapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Equal(
            OriginalMapRuntimeAdmission.Map19BlocksetResourceId,
            applied.Snapshot.CurrentBlockDefinition.Identity.ResourceId);
        Assert.Equal(
            OriginalMapRuntimeAdmission.Map19EntityListResourceId,
            applied.Snapshot.EntityPopulation.ResourceId);
        Assert.Equal(
            OriginalMapRuntimeAdmission.Map19EntityRecordCount,
            applied.Snapshot.EntityPopulation.Records.Count);
        Assert.True(applied.Snapshot.MessengerAcceptance!.Accepted);
        Assert.True(applied.Snapshot.CastleGate!.Opened);
        Assert.Equal(
            OriginalMapRuntimeAdmission.NorthMap19WarpDestinationOpaqueFacing,
            relocated.Animation.OpaqueFacing);

        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
        Assert.Equal(new MapPosition(27, 30), moved.Snapshot.PlayerPosition);
        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.Map19Id), moved.Snapshot.Map);
        Assert.Null(moved.Snapshot.LastCrossMapTransition);

        GameSession restarted = Start(Definition(EmptyWords()));
        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.MapId), restarted.PrivateOriginalMapSnapshot.Map);
        Assert.Same(
            restarted.PrivateOriginalMapSnapshot.Definition.InitialRuntime,
            restarted.PrivateOriginalMapSnapshot.CurrentRuntime);
        Assert.False(restarted.PrivateOriginalMapSnapshot.CastleGate!.Opened);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastCrossMapTransition);
    }

    [Fact]
    public void NorthMap19TransitionRequiresTheOpenedGateExactApproachAndDirection()
    {
        GameSession session = Start(Definition(EmptyWords()));
        Move(session, ExplorationDirection.South, 1);
        Move(session, ExplorationDirection.West, 28);
        Move(session, ExplorationDirection.North, 2);
        Assert.Equal(new MapPosition(28, 2), session.PrivateOriginalMapSnapshot.PlayerPosition);

        PrivateOriginalMapMoveApplied unopened = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        Assert.Null(unopened.CrossMapTransition);
        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.MapId), unopened.Snapshot.Map);
        Assert.Equal(new MapPosition(28, 1), unopened.Snapshot.PlayerPosition);
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactRuntimeCatalogAndNorthTransition()
    {
        OriginalMapImportDefinition accepted = Definition(EmptyWords());
        OriginalMapImportDefinition missingRuntime = Rebind(
            accepted,
            new OriginalMapExplorationRuntimeCatalog([accepted.InitialRuntime]),
            northTransition: null);
        AssertRejectedDefinition(
            missingRuntime,
            OriginalMapImportFailureCode.InvalidMapProjection);

        OriginalMapCrossMapTransitionDefinition admitted =
            accepted.NorthMap19Transition!;
        OriginalMapCrossMapTransitionDefinition wrongTransition = new(
            admitted.Identity,
            admitted.SourceTriggerX,
            admitted.SourceTriggerY,
            admitted.AdmittedApproach,
            admitted.AdmittedDirection,
            admitted.AdmittedTrigger,
            admitted.DestinationMap,
            admitted.Destination,
            destinationOpaqueFacing: 2);
        AssertRejectedDefinition(
            Rebind(accepted, accepted.RuntimeCatalog, wrongTransition),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void Map19RuntimeRejectsMutatedLayoutWithPreviouslyAcceptedDigestStrings()
    {
        OriginalMapImportDefinition accepted = Definition(EmptyWords());
        OriginalMapExplorationRuntimeDefinition admittedMap19 =
            accepted.RuntimeCatalog.Resolve(new MapId(OriginalMapRuntimeAdmission.Map19Id));
        ushort[] mutatedWords = [.. admittedMap19.WorkingLayout.Words];
        mutatedWords[0] ^= 1;

        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            new OriginalMapExplorationRuntimeDefinition(
                admittedMap19.Map,
                new WorkingMapLayout(mutatedWords),
                admittedMap19.BlockCatalog,
                admittedMap19.AreaCatalog,
                admittedMap19.EntityPopulation,
                admittedMap19.SelectedSetup,
                admittedMap19.SelectedInitIdentity,
                OriginalMapRuntimeAdmission.Map19DecodedLayoutDigest,
                OriginalMapRuntimeAdmission.Map19CollisionProjectionDigest));

        Assert.Equal("decodedLayoutDigest", error.ParamName);
    }

    [Fact]
    public void CastleGateTargetWithoutMessengerAcceptanceRemainsOrdinaryTraversal()
    {
        GameSession session = Start(Definition(EmptyWords()));
        MoveToSchoolDoorApproach(session);
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        MoveFromOpenSchoolDoorToCastleGateApproach(session);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;

        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));

        Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
        Assert.Null(moved.CastleGate);
        Assert.False(moved.Snapshot.CastleGate!.Opened);
        Assert.Same(before.CastleGate, moved.Snapshot.CastleGate);
        Assert.False(moved.Snapshot.MessengerAcceptance!.Accepted);
    }

    [Fact]
    public void SarahReinteractionIsTextOnlyWhileWarpPreservesAndRestartClearsTheRoute()
    {
        GameSession session = Start(Definition(EmptyWords()));
        MoveToSarahInteraction(session);
        _ = session.BeginPrivateOriginalMapPlayerLocomotion(
            new MoveExplorationCommand(ExplorationDirection.North));
        _ = Assert.IsType<PrivateOriginalMapSarahInteractionApplied>(
            session.InteractPrivateOriginalMapSarah(
                new InteractPrivateOriginalMapSarahCommand(
                    session.PrivateOriginalMapSnapshot.SimulationStep)));

        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        _ = session.BeginPrivateOriginalMapPlayerLocomotion(
            new MoveExplorationCommand(ExplorationDirection.North));
        PrivateOriginalMapSarahState before = session.PrivateOriginalMapSnapshot.Sarah!;
        PrivateOriginalMapSarahInteractionApplied repeated =
            Assert.IsType<PrivateOriginalMapSarahInteractionApplied>(
                session.InteractPrivateOriginalMapSarah(
                    new InteractPrivateOriginalMapSarahCommand(
                        session.PrivateOriginalMapSnapshot.SimulationStep)));
        Assert.True(repeated.Receipt.Repeated);
        Assert.Equal(new[] { 480, 481 }, repeated.Receipt.TextIds);
        Assert.Equal(OriginalMapRuntimeAdmission.SarahRepeatStages, repeated.Receipt.Stages);
        Assert.Same(before, repeated.Snapshot.Sarah);

        Move(session, ExplorationDirection.South, 2);
        Move(session, ExplorationDirection.East, 13);
        Move(session, ExplorationDirection.North, 7);
        Assert.NotNull(session.PrivateOriginalMapSnapshot.LastSameMapWarp);
        Assert.Same(before, session.PrivateOriginalMapSnapshot.Sarah);

        GameSession restarted = Start(Definition(EmptyWords()));
        Assert.Equal(new MapPosition(42, 8), restarted.PrivateOriginalMapSnapshot.Sarah!.ActorPosition);
        Assert.Equal((byte)3, restarted.PrivateOriginalMapSnapshot.Sarah.ActorOpaqueFacing);
        Assert.False(restarted.PrivateOriginalMapSnapshot.Sarah.TemporaryRouteFlag256Set);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastSarah);
    }

    [Fact]
    public void WrongAndStaleSarahInteractionsAreReferenceEqualZeroMutation()
    {
        GameSession session = Start(Definition(EmptyWords()));
        PrivateOriginalMapSessionSnapshot initial = session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapSarahInteractionRejected wrong =
            Assert.IsType<PrivateOriginalMapSarahInteractionRejected>(
                session.InteractPrivateOriginalMapSarah(
                    new InteractPrivateOriginalMapSarahCommand(initial.SimulationStep)));
        Assert.Equal(
            PrivateOriginalMapSarahInteractionFailureCode.InteractionTargetMismatch,
            wrong.Diagnostic.Code);
        Assert.Same(initial, wrong.Snapshot);
        Assert.Same(initial, session.PrivateOriginalMapSnapshot);

        MoveToSarahInteraction(session);
        PrivateOriginalMapSessionSnapshot current = session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapSarahInteractionRejected stale =
            Assert.IsType<PrivateOriginalMapSarahInteractionRejected>(
                session.InteractPrivateOriginalMapSarah(
                    new InteractPrivateOriginalMapSarahCommand(current.SimulationStep - 1)));
        Assert.Equal(
            PrivateOriginalMapSarahInteractionFailureCode.StaleSimulationStep,
            stale.Diagnostic.Code);
        Assert.Same(current, stale.Snapshot);
        Assert.Same(current, session.PrivateOriginalMapSnapshot);
    }

    [Fact]
    public void RestartRestoresTheClosedSchoolDoorAndClearsItsNaturalState()
    {
        AcceptedSource source = new(Accepted());
        GameSession first = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        MoveToSchoolDoorApproach(first);
        _ = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));
        Assert.True(first.PrivateOriginalMapSnapshot.SchoolDoorStepCopyApplied);
        Assert.NotNull(first.PrivateOriginalMapSnapshot.LastNaturalStepCopy);

        GameSession restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        Assert.False(restarted.PrivateOriginalMapSnapshot.SchoolDoorStepCopyApplied);
        Assert.Null(restarted.PrivateOriginalMapSnapshot.LastNaturalStepCopy);
        Assert.True(OriginalMapTraversal.IsBlocked(
            restarted.PrivateOriginalMapSnapshot.WorkingLayout,
            new MapPosition(41, 13)));
    }

    [Fact]
    public void OtherAdmittedWarpDoesNotInventASecondRoofEffect()
    {
        GameSession session = Start(Definition(EmptyWords()));
        WorkingMapLayout admittedLayout = session.PrivateOriginalMapSnapshot.WorkingLayout;
        for (int count = 0; count < 4; count++)
        {
            _ = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.South));
        }

        for (int count = 0; count < 9; count++)
        {
            _ = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.West));
        }

        PrivateOriginalMapMoveApplied relocated = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));

        Assert.Equal(
            OriginalMapRuntimeAdmission.SchoolWarpRecordOrdinal,
            Assert.IsType<PrivateOriginalMapSameMapWarpReceipt>(relocated.SameMapWarp)
                .RecordIdentity.OneBasedRecordOrdinal);
        Assert.Null(relocated.RoofOnLoad);
        Assert.Null(relocated.Snapshot.LastRoofOnLoad);
        Assert.IsType<MapBlockCopyLifecycleInactiveState>(
            relocated.Snapshot.RoofOnLoadLifecycle);
        Assert.Same(admittedLayout, relocated.Snapshot.WorkingLayout);
    }

    [Fact]
    public void ActiveCycleRejectsReplacementAndRestartRestoresAdmissionAnimation()
    {
        AcceptedSource source = new(Accepted());
        GameSession first = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        first.BeginPrivateOriginalMapPlayerLocomotion(
            new MoveExplorationCommand(ExplorationDirection.West));

        Assert.Throws<InvalidOperationException>(() =>
            first.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(ExplorationDirection.South)));

        GameSession restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        Assert.Equal(PrivateOriginalMapPlayerLocomotionPhase.Admission,
            restarted.PrivateOriginalMapPlayerLocomotion.Phase);
        Assert.Equal(26, restarted.PrivateOriginalMapPlayerLocomotion.StoredCounter);
        Assert.Equal(new MapPosition(56, 3),
            restarted.PrivateOriginalMapPlayerLocomotion.SourcePosition);
    }

    [Fact]
    public void GameSessionOwnsMovedCollisionAndActiveAreaOutcomes()
    {
        ushort[] words = EmptyWords();
        words[Index(56, 4)] = OriginalMapTraversal.CollisionMask;
        GameSession session = Start(Definition(words));

        PrivateOriginalMapMoveApplied moved = null!;
        for (int expectedX = 57; expectedX <= 61; expectedX++)
        {
            moved = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.East));
            Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
            Assert.Equal(new MapPosition(expectedX, 3), moved.Snapshot.PlayerPosition);
            Assert.Equal(2, moved.Snapshot.CurrentArea.OneBasedRecordOrdinal);
        }

        PrivateOriginalMapMoveApplied blockedArea = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(
            OriginalMapTraversalOutcome.BlockedOutsideActiveArea,
            blockedArea.Traversal.Outcome);
        Assert.Equal(new MapPosition(61, 3), blockedArea.Snapshot.PlayerPosition);
        Assert.Equal(6, blockedArea.Snapshot.SimulationStep);

        GameSession collisionSession = Start(Definition(words));
        WorkingMapLayout collisionLayout =
            collisionSession.PrivateOriginalMapSnapshot.WorkingLayout;
        PrivateOriginalMapMoveApplied blockedCollision =
            collisionSession.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.South));
        Assert.Equal(
            OriginalMapTraversalOutcome.BlockedByCollision,
            blockedCollision.Traversal.Outcome);
        Assert.Equal(new MapPosition(56, 3), blockedCollision.Snapshot.PlayerPosition);
        Assert.Same(collisionLayout, blockedCollision.Snapshot.WorkingLayout);
    }

    [Fact]
    public void LayoutBoundaryBlocksWithoutChangingTheAuthoritativePosition()
    {
        GameSession session = Start(Definition(EmptyWords()));
        for (int index = 0; index < 3; index++)
        {
            Assert.Equal(
                OriginalMapTraversalOutcome.Moved,
                session.ApplyPrivateOriginalMap(
                    new MoveExplorationCommand(ExplorationDirection.North)).Traversal.Outcome);
        }

        PrivateOriginalMapMoveApplied blocked = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));

        Assert.Equal(OriginalMapTraversalOutcome.BlockedByBoundary, blocked.Traversal.Outcome);
        Assert.Equal(new MapPosition(56, 0), blocked.Snapshot.PlayerPosition);
        Assert.Equal(4, blocked.Snapshot.SimulationStep);
    }

    [Fact]
    public void DirectionalStairMovementUsesTheAcceptedOriginalPolicy()
    {
        ushort[] words = EmptyWords();
        words[Index(56, 3)] = OriginalMapTraversal.RightStairMask;
        words[Index(57, 2)] = OriginalMapTraversal.RightStairMask;
        GameSession session = Start(Definition(words));

        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));

        Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
        Assert.Equal(new MapPosition(57, 2), moved.Snapshot.PlayerPosition);
        Assert.Equal(OriginalMapTraversal.RightStairMask, moved.Traversal.SourceWord);
        Assert.Equal(OriginalMapTraversal.RightStairMask, moved.Traversal.DestinationWord);
    }

    [Fact]
    public void RestartReconstructsTheControlledStartWithoutPersistence()
    {
        AcceptedSource source = new(Accepted());
        PrivateOriginalMapGameSessionStarted first =
            Assert.IsType<PrivateOriginalMapGameSessionStarted>(
                GameSession.StartPrivateOriginalMap(source, Request()));
        Assert.IsType<PrivateOriginalMapMoveApplied>(
            first.Session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.East)));

        PrivateOriginalMapGameSessionStarted restarted =
            Assert.IsType<PrivateOriginalMapGameSessionStarted>(
                GameSession.StartPrivateOriginalMap(source, Request()));

        Assert.Equal(2, source.AdmitCalls);
        Assert.Equal(new MapPosition(56, 3),
            restarted.Session.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(0, restarted.Session.PrivateOriginalMapSnapshot.SimulationStep);
        Assert.Null(restarted.Session.PrivateOriginalMapSnapshot.LastTraversal);
    }

    [Fact]
    public void CurrentAreaRecomputesFromPositionAndSurvivesBlockedMutationAndRestartBoundaries()
    {
        ushort[] words = EmptyWords();
        words[Index(49, 4)] = OriginalMapTraversal.CollisionMask;
        OriginalMapImportAccepted accepted = Accepted(Definition(words));
        AcceptedSource source = new(accepted);
        GameSession session = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;

        Assert.Equal(2, session.PrivateOriginalMapSnapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(
            session.PrivateOriginalMapSnapshot.Definition.AreaCatalog.Records[1],
            session.PrivateOriginalMapSnapshot.CurrentAreaDefinition);
        Assert.Equal(
            OriginalMapTraversalOutcome.Moved,
            session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.South)).Traversal.Outcome);
        for (int index = 0; index < 6; index++)
        {
            Assert.Equal(
                OriginalMapTraversalOutcome.Moved,
                session.ApplyPrivateOriginalMap(
                    new MoveExplorationCommand(ExplorationDirection.West)).Traversal.Outcome);
        }

        PrivateOriginalMapSessionSnapshot crossed = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(50, 4), crossed.PlayerPosition);
        Assert.Equal(1, crossed.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(crossed.Definition.AreaCatalog.Records[0], crossed.CurrentAreaDefinition);

        PrivateOriginalMapMoveApplied blocked = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        Assert.Equal(OriginalMapTraversalOutcome.BlockedByCollision, blocked.Traversal.Outcome);
        Assert.Equal(1, blocked.Snapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(crossed.CurrentAreaDefinition, blocked.Snapshot.CurrentAreaDefinition);

        PrivateOriginalMapLayoutMutationApplied mutated =
            Assert.IsType<PrivateOriginalMapLayoutMutationApplied>(
                session.ApplyPrivateOriginalMapLayoutMutation(MutationCommand(blocked.Snapshot)));
        Assert.Equal(1, mutated.Snapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(crossed.CurrentAreaDefinition, mutated.Snapshot.CurrentAreaDefinition);
        Assert.Equal(blocked.Snapshot.PlayerPosition, mutated.Snapshot.PlayerPosition);

        GameSession restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        Assert.Equal(new MapPosition(56, 3), restarted.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(2, restarted.PrivateOriginalMapSnapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(
            restarted.PrivateOriginalMapSnapshot.Definition.AreaCatalog.Records[1],
            restarted.PrivateOriginalMapSnapshot.CurrentAreaDefinition);
    }

    [Fact]
    public void CurrentBlockRecomputesFromAuthoritativeLayoutAndRejectsDanglingCurrentState()
    {
        ushort[] words = EmptyWords();
        words[Index(56, 3)] = 1;
        words[Index(57, 3)] = 2;
        GameSession session = Start(Definition(words));

        Assert.Equal(
            1,
            session.PrivateOriginalMapSnapshot.CurrentBlockDefinition
                .Identity.ZeroBasedBlockIndex);

        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(
            2,
            moved.Snapshot.CurrentBlockDefinition.Identity.ZeroBasedBlockIndex);

        ushort[] invalidWords = [.. moved.Snapshot.WorkingLayout.Words];
        invalidWords[0] = OriginalMapRuntimeAdmission.AcceptedBlockCount;
        Assert.Throws<ArgumentException>(() => new PrivateOriginalMapSessionSnapshot(
            moved.Snapshot.Definition,
            moved.Snapshot.Receipt,
            new WorkingMapLayout(invalidWords),
            moved.Snapshot.SimulationStep,
            moved.Snapshot.PlayerPosition,
            moved.Snapshot.LastTraversal,
            moved.Snapshot.ControlledStepCopyApplied,
            moved.Snapshot.LastLayoutMutation,
            currentRuntime: moved.Snapshot.CurrentRuntime));
    }

    [Fact]
    public void ExactControlledStepCopyMutatesOnlyTheAuthoritativeSessionLayout()
    {
        ushort[] words = EmptyWords();
        words[Index(41, 13)] = OriginalMapTraversal.CollisionMask;
        OriginalMapImportDefinition definition = Definition(words);
        GameSession session = Start(definition);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;

        PrivateOriginalMapLayoutMutationApplied applied =
            Assert.IsType<PrivateOriginalMapLayoutMutationApplied>(
                session.ApplyPrivateOriginalMapLayoutMutation(
                    MutationCommand(before)));

        Assert.Same(applied.Snapshot, session.PrivateOriginalMapSnapshot);
        Assert.NotSame(definition.WorkingLayout, applied.Snapshot.WorkingLayout);
        Assert.True(OriginalMapTraversal.IsBlocked(
            definition.WorkingLayout,
            new MapPosition(41, 13)));
        Assert.False(OriginalMapTraversal.IsBlocked(
            applied.Snapshot.WorkingLayout,
            new MapPosition(41, 13)));
        Assert.Equal(1, applied.Snapshot.SimulationStep);
        Assert.True(applied.Snapshot.ControlledStepCopyApplied);
        Assert.Null(applied.Snapshot.LastTraversal);
        Assert.Same(applied.Receipt, applied.Snapshot.LastLayoutMutation);
        Assert.Equal(
            PrivateOriginalMapCollisionCategory.BlockedByAcceptedCollisionClass,
            applied.Receipt.BeforeCollision);
        Assert.Equal(
            PrivateOriginalMapCollisionCategory.ActiveNonBlocked,
            applied.Receipt.AfterCollision);
        Assert.Equal((62, 0, 41, 13, 1, 1), Geometry(applied.Receipt.Copy));
        Assert.DoesNotContain(
            applied.Receipt.GetType().GetProperties(),
            property => property.Name.Contains("Word", StringComparison.Ordinal) ||
                property.Name.Contains("Path", StringComparison.Ordinal) ||
                property.Name.Contains("Payload", StringComparison.Ordinal));
    }

    [Fact]
    public void WrongStaleAndDuplicateMutationCommandsAreZeroMutation()
    {
        ushort[] words = EmptyWords();
        words[Index(41, 13)] = OriginalMapTraversal.CollisionMask;
        GameSession session = Start(Definition(words));
        PrivateOriginalMapSessionSnapshot initial = session.PrivateOriginalMapSnapshot;
        OriginalMapStepCopyIdentity exact = initial.Definition.ControlledStepCopy!.Identity;

        foreach (OriginalMapStepCopyIdentity wrong in new[]
        {
            new OriginalMapStepCopyIdentity(
                ContentProfile.PublicSynthetic,
                exact.Map,
                exact.SourceResourceId,
                exact.OneBasedRecordOrdinal),
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                new MapId("other-map"),
                exact.SourceResourceId,
                exact.OneBasedRecordOrdinal),
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                exact.Map,
                "OtherStepEvents",
                exact.OneBasedRecordOrdinal),
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                exact.Map,
                exact.SourceResourceId,
                exact.OneBasedRecordOrdinal - 1),
        })
        {
            AssertRejectedMutation(
                session,
                new ApplyPrivateOriginalMapLayoutMutationCommand(wrong, 0),
                initial,
                PrivateOriginalMapLayoutMutationFailureCode.ReferenceMismatch);
        }

        AssertRejectedMutation(
            session,
            new ApplyPrivateOriginalMapLayoutMutationCommand(exact, 1),
            initial,
            PrivateOriginalMapLayoutMutationFailureCode.StaleSimulationStep);

        PrivateOriginalMapLayoutMutationApplied applied =
            Assert.IsType<PrivateOriginalMapLayoutMutationApplied>(
                session.ApplyPrivateOriginalMapLayoutMutation(
                    new ApplyPrivateOriginalMapLayoutMutationCommand(exact, 0)));
        AssertRejectedMutation(
            session,
            new ApplyPrivateOriginalMapLayoutMutationCommand(exact, 1),
            applied.Snapshot,
            PrivateOriginalMapLayoutMutationFailureCode.AlreadyApplied);
    }

    [Fact]
    public void MovementReadsMutatedLayoutAndRestartRestoresTheAdmittedLayout()
    {
        ushort[] words = EmptyWords();
        words[Index(41, 13)] = OriginalMapTraversal.CollisionMask;
        OriginalMapImportAccepted accepted = Accepted(Definition(words));
        AcceptedSource source = new(accepted);
        GameSession first = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;

        for (int index = 0; index < 9; index++)
        {
            Assert.Equal(
                OriginalMapTraversalOutcome.Moved,
                first.ApplyPrivateOriginalMap(
                    new MoveExplorationCommand(ExplorationDirection.South)).Traversal.Outcome);
        }

        for (int index = 0; index < 15; index++)
        {
            Assert.Equal(
                OriginalMapTraversalOutcome.Moved,
                first.ApplyPrivateOriginalMap(
                    new MoveExplorationCommand(ExplorationDirection.West)).Traversal.Outcome);
        }

        Assert.Equal(new MapPosition(41, 12), first.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.IsType<PrivateOriginalMapLayoutMutationApplied>(
            first.ApplyPrivateOriginalMapLayoutMutation(
                MutationCommand(first.PrivateOriginalMapSnapshot)));
        PrivateOriginalMapMoveApplied entered = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.South));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, entered.Traversal.Outcome);
        Assert.Equal(new MapPosition(41, 13), entered.Snapshot.PlayerPosition);
        Assert.True(entered.Snapshot.ControlledStepCopyApplied);
        Assert.Null(entered.Snapshot.LastLayoutMutation);

        GameSession restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        Assert.True(OriginalMapTraversal.IsBlocked(
            restarted.PrivateOriginalMapSnapshot.WorkingLayout,
            new MapPosition(41, 13)));
        Assert.False(restarted.PrivateOriginalMapSnapshot.ControlledStepCopyApplied);
        Assert.Equal(0, restarted.PrivateOriginalMapSnapshot.SimulationStep);
    }

    [Fact]
    public void RequestTrustRootRejectsBeforeCallingTheSource()
    {
        AcceptedSource source = new(Accepted());

        PrivateOriginalMapGameSessionStartRejected wrongPackage =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    source,
                    new OriginalMapImportRequest(
                        "other-package",
                        ContentProfile.PrivateLocal,
                        OriginalMapRuntimeAdmission.AcceptedContentDigest)));
        Assert.Equal(
            OriginalMapImportFailureCode.PackageIdentityMismatch,
            wrongPackage.Diagnostic.Code);

        PrivateOriginalMapGameSessionStartRejected wrongProfile =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    source,
                    new OriginalMapImportRequest(
                        OriginalMapRuntimeAdmission.PackageId,
                        ContentProfile.PublicSynthetic,
                        OriginalMapRuntimeAdmission.AcceptedContentDigest)));
        Assert.Equal(OriginalMapImportFailureCode.ProfileMismatch, wrongProfile.Diagnostic.Code);

        PrivateOriginalMapGameSessionStartRejected wrongDigest =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    source,
                    new OriginalMapImportRequest(
                        OriginalMapRuntimeAdmission.PackageId,
                        ContentProfile.PrivateLocal,
                        new string('0', 64))));
        Assert.Equal(
            OriginalMapImportFailureCode.ContentDigestMismatch,
            wrongDigest.Diagnostic.Code);
        Assert.Equal(0, source.AdmitCalls);
    }

    [Fact]
    public void AcceptedSourceReceiptMustRetainExactDigestAndCapabilities()
    {
        OriginalMapImportDefinition definition = Definition(EmptyWords());
        PrivateOriginalMapGameSessionStartRejected wrongDigest =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    new AcceptedSource(Accepted(
                        definition,
                        Receipt(contentDigest: new string('1', 64)))),
                    Request()));
        Assert.Equal(
            OriginalMapImportFailureCode.ContentDigestMismatch,
            wrongDigest.Diagnostic.Code);

        PrivateOriginalMapGameSessionStartRejected missingCapability =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    new AcceptedSource(Accepted(
                        definition,
                        Receipt(capabilities:
                            [OriginalMapRuntimeAdmission.ImportCapability]))),
                    Request()));
        Assert.Equal(
            OriginalMapImportFailureCode.MissingReference,
            missingCapability.Diagnostic.Code);
    }

    [Fact]
    public void AcceptedSourceReceiptMustRetainExactCanonicalProvenanceAndEvidenceOwners()
    {
        OriginalMapImportDefinition definition = Definition(EmptyWords());
        AssertRejectedReceipt(
            definition,
            Receipt(romSha256: new string('A', 64)),
            OriginalMapImportFailureCode.ProvenanceMismatch);
        AssertRejectedReceipt(
            definition,
            Receipt(upstreamRepository: "https://example.invalid/incompatible.git"),
            OriginalMapImportFailureCode.ProvenanceMismatch);
        AssertRejectedReceipt(
            definition,
            Receipt(upstreamCommit: new string('b', 40)),
            OriginalMapImportFailureCode.ProvenanceMismatch);
        AssertRejectedReceipt(
            definition,
            Receipt(evidenceOwnerIds:
                OriginalMapRuntimeAdmission.RequiredEvidenceOwners.Skip(1)),
            OriginalMapImportFailureCode.ProvenanceMismatch);
        AssertRejectedReceipt(
            definition,
            Receipt(evidenceOwnerIds:
                OriginalMapRuntimeAdmission.RequiredEvidenceOwners.Append(
                    "incompatible-extra-owner")),
            OriginalMapImportFailureCode.ProvenanceMismatch);

        PrivateOriginalMapGameSessionStarted reordered =
            Assert.IsType<PrivateOriginalMapGameSessionStarted>(
                GameSession.StartPrivateOriginalMap(
                    new AcceptedSource(Accepted(
                        definition,
                        Receipt(evidenceOwnerIds:
                            OriginalMapRuntimeAdmission.RequiredEvidenceOwners.Reverse()))),
                    Request()));
        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.MapId),
            reordered.Session.PrivateOriginalMapSnapshot.Map);
    }

    [Fact]
    public void AcceptedSourceReceiptMustRetainExactMapProjectionDigests()
    {
        OriginalMapImportDefinition definition = Definition(EmptyWords());
        AssertRejectedReceipt(
            definition,
            Receipt(decodedLayoutDigest: new string('2', 64)),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            definition,
            Receipt(collisionProjectionDigest: new string('3', 64)),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainTheExactControlledStepCopy()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        WorkingMapLayout layout = new(EmptyWords());
        OriginalMapAreaCatalog areaCatalog = AcceptedAreaCatalog();
        OriginalMapControlledAdmission controlled = new(
            map,
            new MapPosition(
                OriginalMapRuntimeAdmission.StartX,
                OriginalMapRuntimeAdmission.StartY),
            OriginalMapRuntimeAdmission.OpaqueStartFacing,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            OriginalMapRuntimeAdmission.SelectedInitIdentity,
            noProgramRequest: true);
        OriginalMapImportDefinition missing = new(
            map,
            layout,
            AcceptedBlockCatalog(),
            areaCatalog,
            AcceptedEntityPopulation(map),
            AcceptedVisualResourceSelection(map),
            controlled,
            ["natural-route-and-effects-unknown"]);
        OriginalMapImportDefinition wrongIdentity = new(
            map,
            layout,
            AcceptedBlockCatalog(),
            areaCatalog,
            AcceptedEntityPopulation(map),
            AcceptedVisualResourceSelection(map),
            controlled,
            new OriginalMapStepCopyDefinition(
                new OriginalMapStepCopyIdentity(
                    ContentProfile.PrivateLocal,
                    map,
                    "OtherStepEvents",
                    OriginalMapRuntimeAdmission.ControlledStepCopyRecordOrdinal),
                new MapPosition(41, 13),
                new WorkingMapBlockCopy(62, 0, 41, 13, 1, 1)),
            ["natural-route-and-effects-unknown"]);

        AssertRejectedReceipt(
            missing,
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            wrongIdentity,
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactOrderedAreaProjection()
    {
        OriginalMapTraversalArea[] changedBounds = AcceptedAreas();
        changedBounds[0] = new OriginalMapTraversalArea(0, 0, 49, 31);
        OriginalMapTraversalArea[] reordered = AcceptedAreas();
        (reordered[0], reordered[1]) = (reordered[1], reordered[0]);
        OriginalMapAreaDefinition[] changedSourceRecords = AcceptedAreaDefinitions();
        changedSourceRecords[1] = AcceptedAreaDefinition(
            oneBasedRecordOrdinal: 2,
            area: changedSourceRecords[1].MainLayerBounds,
            defaultMusic: 9);

        AssertRejectedReceipt(
            Definition(EmptyWords(), AcceptedAreaCatalog(changedBounds)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(EmptyWords(), AcceptedAreaCatalog(reordered)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(EmptyWords(), new OriginalMapAreaCatalog(changedSourceRecords)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactOrderedBlocksetProjection()
    {
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                blockCatalog: ProjectAuthoredBlockCatalog(
                    "OtherBlocks",
                    OriginalMapRuntimeAdmission.AcceptedBlockCount)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                blockCatalog: ProjectAuthoredBlockCatalog(
                    OriginalMapRuntimeAdmission.AcceptedBlocksetResourceId,
                    OriginalMapRuntimeAdmission.AcceptedBlockCount - 1)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                blockCatalog: ProjectAuthoredBlockCatalog(
                    OriginalMapRuntimeAdmission.AcceptedBlocksetResourceId,
                    OriginalMapRuntimeAdmission.AcceptedBlockCount + 1)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                blockCatalog: ProjectAuthoredBlockCatalog(
                    OriginalMapRuntimeAdmission.AcceptedBlocksetResourceId,
                    OriginalMapRuntimeAdmission.AcceptedBlockCount,
                    mutateFirstWord: true)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);

        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedBlocksetProjection(
            AcceptedBlockCatalog()));
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactVisualResourceSelectionProjection()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                visualResourceSelection: new OriginalMapVisualResourceSelection(
                    map,
                    paletteIndex: 1,
                    [0, 37, 43, 53, 66])),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                visualResourceSelection: new OriginalMapVisualResourceSelection(
                    map,
                    paletteIndex: 0,
                    [0, 37, 43, 53, 67])),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                visualResourceSelection: new OriginalMapVisualResourceSelection(
                    map,
                    paletteIndex: 0,
                    [37, 0, 43, 53, 66])),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);

        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedVisualResourceSelection(
            AcceptedVisualResourceSelection(map)));
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactSameMapWarps()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        OriginalMapSameMapWarpDefinition[] accepted = AcceptedSameMapWarps(map).Records.ToArray();
        OriginalMapSameMapWarpDefinition changedDestination = new(
            accepted[1].Identity,
            accepted[1].Trigger,
            new MapPosition(4, 3),
            accepted[1].OpaqueFacing);

        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                sameMapWarps: new OriginalMapSameMapWarpCatalog(
                    [accepted[0], changedDestination])),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                sameMapWarps: new OriginalMapSameMapWarpCatalog(
                    accepted.Reverse())),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedSameMapWarps(
            AcceptedSameMapWarps(map)));
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactRoofOnLoadClear()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        OriginalMapSameMapWarpCatalog warps = AcceptedSameMapWarps(map);
        OriginalMapAreaCatalog areas = AcceptedAreaCatalog();
        OriginalMapRoofOnLoadDefinition accepted = RoofOnLoadClear(map, warps, areas);
        OriginalMapRoofOnLoadDefinition changed = new(
            accepted.Identity,
            new MapPosition(
                OriginalMapRuntimeAdmission.HouseRoofSourceTriggerX + 1,
                OriginalMapRuntimeAdmission.HouseRoofSourceTriggerY),
            accepted.ClearDestination,
            accepted.Width,
            accepted.Height,
            accepted.AppliedAfterWarp,
            accepted.DestinationArea);

        AssertRejectedReceipt(
            Definition(EmptyWords(), roofOnLoadClear: changed),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(EmptyWords(), omitRoofOnLoadClear: true),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedRoofOnLoadClear(accepted));
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactSelectedSetupEntityPopulation()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                entityPopulation: ProjectAuthoredEntityPopulation(
                    map,
                    "other-entities",
                    OriginalMapRuntimeAdmission.AcceptedEntityRecordCount,
                    acceptedDigestOverride: true)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                entityPopulation: ProjectAuthoredEntityPopulation(
                    map,
                    OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                    OriginalMapRuntimeAdmission.AcceptedEntityRecordCount,
                    acceptedDigestOverride: true,
                    allFixed: true)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                entityPopulation: ProjectAuthoredEntityPopulation(
                    map,
                    OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                    OriginalMapRuntimeAdmission.AcceptedEntityRecordCount - 1,
                    acceptedDigestOverride: true)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                entityPopulation: ProjectAuthoredEntityPopulation(
                    map,
                    OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                    OriginalMapRuntimeAdmission.AcceptedEntityRecordCount,
                    mutateFirstTail: true)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);

        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedEntityPopulation(
            AcceptedEntityPopulation(map)));
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactBowieDoorStepCopy()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        AssertRejectedReceipt(
            Definition(EmptyWords(), omitBowieDoorStepCopy: true),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                bowieDoorStepCopy: new OriginalMapStepCopyDefinition(
                    new OriginalMapStepCopyIdentity(
                        ContentProfile.PrivateLocal,
                        map,
                        OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                        OriginalMapRuntimeAdmission.BowieDoorStepCopyRecordOrdinal + 1),
                    new MapPosition(4, 8),
                    new WorkingMapBlockCopy(62, 0, 4, 8, 1, 1))),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(
                EmptyWords(),
                bowieDoorStepCopy: new OriginalMapStepCopyDefinition(
                    new OriginalMapStepCopyIdentity(
                        ContentProfile.PrivateLocal,
                        map,
                        OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                        OriginalMapRuntimeAdmission.BowieDoorStepCopyRecordOrdinal),
                    new MapPosition(5, 8),
                    new WorkingMapBlockCopy(62, 0, 5, 8, 1, 1))),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedBowieDoorStepCopy(
            BowieDoorStepCopy(map)));
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactZone601Projection()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        AssertRejectedReceipt(
            Definition(EmptyWords(), omitZone601: true),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        OriginalMapZone601Definition accepted = AcceptedZone601(map);
        OriginalMapZone601Definition wrongFlag = new(
            accepted.Identity,
            accepted.Trigger,
            accepted.GateFlag + 1,
            accepted.BlockingSequenceIdentity,
            accepted.ActorSourceRecord,
            accepted.LogicalActorId,
            accepted.ActorInitialPosition,
            accepted.ActorInitialOpaqueFacing,
            accepted.ActorInitialBehaviorIdentity,
            accepted.ActorBlockingEndPosition,
            accepted.ActorBlockingEndOpaqueFacing,
            accepted.OpaqueFaceWaitOperand,
            accepted.TextIds,
            accepted.AmbientBehaviorIdentity,
            accepted.AmbientCenter,
            accepted.AmbientRange,
            accepted.BlockingStages);
        AssertRejectedReceipt(
            Definition(EmptyWords(), zone601: wrongFlag),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedZone601(
            accepted,
            AcceptedEntityPopulation(map),
            AcceptedAreaCatalog().Traversal,
            Definition(EmptyWords()).WorkingLayout));
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactSarahProjection()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        AssertRejectedReceipt(
            Definition(EmptyWords(), omitSarah: true),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        OriginalMapSarahDefinition accepted = AcceptedSarah(map);
        OriginalMapSarahDefinition wrongFlag = new(
            accepted.Identity,
            accepted.ActorSourceRecord,
            accepted.LogicalActorId,
            accepted.ActorInitialPosition,
            accepted.ActorInitialOpaqueFacing,
            accepted.PlayerInteractionPosition,
            accepted.PlayerInteractionOpaqueFacing,
            accepted.LaterBranchFlag603,
            accepted.LaterBranchFlag602,
            accepted.TemporaryRouteFlag256 + 1,
            accepted.BlockingSequenceIdentity,
            accepted.FirstInteractionWaypoint,
            accepted.RestoredOpaqueFacing,
            accepted.FirstInteractionTextIds,
            accepted.RepeatInteractionTextIds,
            accepted.FirstInteractionStages,
            accepted.RepeatInteractionStages);
        AssertRejectedReceipt(
            Definition(EmptyWords(), sarah: wrongFlag),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedSarah(
            accepted,
            AcceptedEntityPopulation(map),
            AcceptedAreaCatalog().Traversal,
            Definition(EmptyWords()).WorkingLayout));
    }

    [Fact]
    public void TypedSourceRejectionPassesThroughWithoutCreatingASession()
    {
        OriginalMapImportDiagnostic diagnostic = new(
            OriginalMapImportFailureCode.PackageUnavailable,
            "package",
            "The ignored package is unavailable.");

        PrivateOriginalMapGameSessionStartRejected rejected =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(new RejectedSource(diagnostic), Request()));

        Assert.Same(diagnostic, rejected.Diagnostic);
    }

    [Fact]
    public void PrivateOriginalSessionRejectsThePublicSyntheticApplySurface()
    {
        GameSession session = Start(Definition(EmptyWords()));

        Assert.Throws<InvalidOperationException>(() =>
            session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));
    }

    private static void MoveToSchoolDoorApproach(GameSession session)
    {
        for (int count = 0; count < 11; count++)
        {
            _ = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.South));
        }

        for (int count = 0; count < 15; count++)
        {
            _ = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.West));
        }
    }

    private static void MoveToSarahInteraction(GameSession session)
    {
        Move(session, ExplorationDirection.South, 6);
        Move(session, ExplorationDirection.West, 14);
        Assert.Equal(new MapPosition(42, 9), session.PrivateOriginalMapSnapshot.PlayerPosition);
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactEntity142Projection()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        AssertRejectedReceipt(
            Definition(EmptyWords(), omitEntity142: true),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        OriginalMapEntity142Definition accepted = AcceptedEntity142(map);
        OriginalMapEntity142Definition wrongFlag = new(
            accepted.Identity,
            accepted.ActorSourceRecord,
            accepted.LogicalActorId,
            accepted.PhysicalActorSlot,
            accepted.ActorPosition,
            accepted.ActorOpaqueFacing,
            accepted.PlayerInteractionPosition,
            accepted.PlayerInteractionOpaqueFacing,
            accepted.FirstInteractionFlag261 + 1,
            accepted.CompletionFlag602,
            accepted.FirstInteractionTextIds,
            accepted.RepeatInteractionTextIds,
            accepted.FirstInteractionStages,
            accepted.RepeatInteractionStages);
        AssertRejectedReceipt(
            Definition(EmptyWords(), entity142: wrongFlag),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedEntity142(
            accepted,
            AcceptedEntityPopulation(map),
            AcceptedAreaCatalog().Traversal,
            Definition(EmptyWords()).WorkingLayout));
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactAstralZoneProjection()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        AssertRejectedReceipt(
            Definition(EmptyWords(), omitAstralZone: true),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        OriginalMapAstralZoneDefinition accepted = AcceptedAstralZone(map);
        OriginalMapAstralZoneDefinition wrongProgram = new(
            accepted.Identity,
            accepted.Trigger,
            "project-authored-wrong-program",
            accepted.MessengerCompletionFlag603,
            accepted.RequiredEntity142Flag602,
            accepted.CompletionFlag260,
            accepted.SarahSourceRecord,
            accepted.SarahLogicalActorId,
            accepted.SarahDestination,
            accepted.SarahOpaqueFacing,
            accepted.Zone601ActorSourceRecord,
            accepted.Zone601LogicalActorId,
            accepted.Zone601ActorDestination,
            accepted.Zone601ActorOpaqueFacing,
            accepted.TextIds,
            accepted.Stages);
        AssertRejectedReceipt(
            Definition(EmptyWords(), astralZone: wrongProgram),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedAstralZone(
            accepted,
            AcceptedSarah(map),
            AcceptedZone601(map),
            AcceptedAreaCatalog().Traversal,
            Definition(EmptyWords()).WorkingLayout));
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactMessengerProjection()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        AssertRejectedReceipt(
            Definition(EmptyWords(), omitMessengerAcceptance: true),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        OriginalMapMessengerAcceptanceDefinition accepted =
            AcceptedOriginalMapMessenger.Create(map);
        OriginalMapMessengerAcceptanceDefinition wrongBranch = new(
            accepted.Identity,
            accepted.Approach,
            accepted.EntryDirection,
            accepted.Trigger,
            accepted.MessengerProgramIdentity,
            "project-authored-wrong-accepted-branch",
            accepted.ControlShapeSha256,
            accepted.PromptReturn,
            accepted.PromptFlag89,
            accepted.JoinSelector,
            accepted.Flag600,
            accepted.Flag66,
            accepted.CompletionFlag603,
            accepted.SarahSourceRecord,
            accepted.SarahCharacterId,
            accepted.Entity142SourceRecord,
            accepted.Entity142LogicalActorId,
            accepted.MessengerActorSourceRecord,
            accepted.MessengerLogicalActorId,
            accepted.MessengerActorInitialPosition,
            accepted.MessengerActorInitialOpaqueFacing,
            accepted.TextIds,
            accepted.SpeakerOperands,
            accepted.JoinedCharacterIds,
            accepted.Followers,
            accepted.Guards,
            accepted.Endpoint,
            accepted.EndpointOpaqueFacing,
            accepted.TerminalIdentity,
            accepted.Stages);
        AssertRejectedReceipt(
            Definition(EmptyWords(), messengerAcceptance: wrongBranch),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactCastleGateProjection()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        AssertRejectedReceipt(
            Definition(EmptyWords(), omitCastleGate: true),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        OriginalMapCastleGateDefinition accepted = AcceptedOriginalMapCastleGate.Create(map);
        OriginalMapCastleGateDefinition wrongProgram = new(
            accepted.Identity,
            accepted.Approach,
            accepted.EntryDirection,
            accepted.Trigger,
            "project-authored-wrong-program",
            accepted.ControlShapeSha256,
            accepted.TextCursorId,
            accepted.CompletionFlag,
            accepted.SourceOperationCount,
            accepted.ProjectionSourceOperationIndices,
            accepted.GuardMoves,
            accepted.Stages);
        AssertRejectedReceipt(
            Definition(EmptyWords(), castleGate: wrongProgram),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    private static void CompleteRouteThroughEntity142(GameSession session)
    {
        Move(session, ExplorationDirection.West, 2);
        _ = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.True(session.PrivateOriginalMapSnapshot.Zone601!.Flag601Set);

        Move(session, ExplorationDirection.West, 1);
        Move(session, ExplorationDirection.South, 1);
        Move(session, ExplorationDirection.East, 39);
        Move(session, ExplorationDirection.South, 3);
        Move(session, ExplorationDirection.East, 1);
        Move(session, ExplorationDirection.South, 2);
        Move(session, ExplorationDirection.West, 1);
        Assert.Equal(new MapPosition(42, 9), session.PrivateOriginalMapSnapshot.PlayerPosition);
        _ = session.BeginPrivateOriginalMapPlayerLocomotion(
            new MoveExplorationCommand(ExplorationDirection.North));
        _ = Assert.IsType<PrivateOriginalMapSarahInteractionApplied>(
            session.InteractPrivateOriginalMapSarah(
                new InteractPrivateOriginalMapSarahCommand(
                    session.PrivateOriginalMapSnapshot.SimulationStep)));

        Move(session, ExplorationDirection.South, 8);
        Move(session, ExplorationDirection.North, 1);
        Move(session, ExplorationDirection.East, 13);
        Move(session, ExplorationDirection.South, 1);
        Assert.Equal(new MapPosition(55, 17), session.PrivateOriginalMapSnapshot.PlayerPosition);
        _ = session.BeginPrivateOriginalMapPlayerLocomotion(
            new MoveExplorationCommand(ExplorationDirection.West));
        PrivateOriginalMapEntity142RequestApplied requested =
            Assert.IsType<PrivateOriginalMapEntity142RequestApplied>(
                session.RequestPrivateOriginalMapEntity142(
                    new RequestPrivateOriginalMapEntity142Command(
                        session.PrivateOriginalMapSnapshot.SimulationStep)));
        _ = Assert.IsType<PrivateOriginalMapEntity142AcknowledgementApplied>(
            session.AcknowledgePrivateOriginalMapEntity142(
                new AcknowledgePrivateOriginalMapEntity142Command(
                    requested.Snapshot.SimulationStep,
                    requested.Request.RequestSequence,
                    requested.Request.EventIdentity)));
        Assert.True(session.PrivateOriginalMapSnapshot.Entity142!.Flag602Set);
    }

    private static void CompleteRouteThroughMessenger(GameSession session)
    {
        CompleteRouteThroughEntity142(session);
        Move(session, ExplorationDirection.North, 4);
        Move(session, ExplorationDirection.East, 3);
        Move(session, ExplorationDirection.West, 16);
        Move(session, ExplorationDirection.North, 3);
        PrivateOriginalMapMoveApplied accepted = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.NotNull(accepted.MessengerAcceptance);
        Assert.True(accepted.Snapshot.MessengerAcceptance!.Accepted);
    }

    private static void MoveToCastleGateApproach(GameSession session)
    {
        Move(session, ExplorationDirection.South, 4);
        Move(session, ExplorationDirection.West, 2);
        Move(session, ExplorationDirection.North, 1);
        Assert.True(session.PrivateOriginalMapSnapshot.SchoolDoorStepCopyApplied);
        MoveFromOpenSchoolDoorToCastleGateApproach(session);
    }

    private static void MoveFromOpenSchoolDoorToCastleGateApproach(GameSession session)
    {
        Move(session, ExplorationDirection.South, 4);
        Move(session, ExplorationDirection.West, 6);
        Move(session, ExplorationDirection.North, 8);
        Move(session, ExplorationDirection.West, 2);
        Move(session, ExplorationDirection.North, 3);
        Move(session, ExplorationDirection.West, 2);
    }

    private static void MoveToEntity142Interaction(GameSession session)
    {
        Move(session, ExplorationDirection.South, 14);
        Move(session, ExplorationDirection.West, 1);
        Assert.Equal(new MapPosition(55, 17),
            session.PrivateOriginalMapSnapshot.PlayerPosition);
        PrivateOriginalMapPlayerLocomotionStarted facing =
            session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(ExplorationDirection.West));
        Assert.Equal(OriginalMapTraversalOutcome.BlockedByOccupiedEntity,
            facing.Move.Traversal.Outcome);
        Assert.Equal(OriginalMapRuntimeAdmission.Entity142PlayerInteractionOpaqueFacing,
            facing.Animation.OpaqueFacing);
    }

    private static void Move(
        GameSession session,
        ExplorationDirection direction,
        int count)
    {
        for (int index = 0; index < count; index++)
        {
            _ = session.ApplyPrivateOriginalMap(new MoveExplorationCommand(direction));
        }
    }

    private static GameSession Start(OriginalMapImportDefinition definition) =>
        Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(
                new AcceptedSource(Accepted(definition)),
                Request())).Session;

    private static void AssertRejectedDefinition(
        OriginalMapImportDefinition definition,
        OriginalMapImportFailureCode expectedCode)
    {
        PrivateOriginalMapGameSessionStartRejected rejected =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    new AcceptedSource(Accepted(definition)),
                    Request()));
        Assert.Equal(expectedCode, rejected.Diagnostic.Code);
    }

    private static OriginalMapImportDefinition Rebind(
        OriginalMapImportDefinition definition,
        OriginalMapExplorationRuntimeCatalog runtimeCatalog,
        OriginalMapCrossMapTransitionDefinition? northTransition) =>
        new(
            definition.Map,
            definition.WorkingLayout,
            definition.BlockCatalog,
            definition.AreaCatalog,
            definition.EntityPopulation,
            definition.VisualResourceSelection,
            definition.ControlledAdmission,
            definition.ControlledStepCopy,
            definition.SameMapWarps,
            definition.UnsupportedCapabilities,
            definition.RoofOnLoadClear,
            definition.BowieDoorStepCopy,
            definition.Zone601,
            definition.Sarah,
            definition.Entity142,
            definition.AstralZone,
            definition.MessengerAcceptance,
            definition.CastleGate,
            runtimeCatalog,
            northTransition);

    private static OriginalMapImportAccepted Accepted(
        OriginalMapImportDefinition? definition = null,
        OriginalMapImportReceipt? receipt = null) =>
        new(definition ?? Definition(EmptyWords()), receipt ?? Receipt());

    private static OriginalMapImportDefinition Definition(
        ushort[] words,
        OriginalMapAreaCatalog? areaCatalog = null,
        OriginalMapBlockCatalog? blockCatalog = null,
        OriginalMapEntityPopulation? entityPopulation = null,
        OriginalMapVisualResourceSelection? visualResourceSelection = null,
        OriginalMapSameMapWarpCatalog? sameMapWarps = null,
        OriginalMapRoofOnLoadDefinition? roofOnLoadClear = null,
        bool omitRoofOnLoadClear = false,
        OriginalMapStepCopyDefinition? bowieDoorStepCopy = null,
        bool omitBowieDoorStepCopy = false,
        OriginalMapZone601Definition? zone601 = null,
        bool omitZone601 = false,
        OriginalMapSarahDefinition? sarah = null,
        bool omitSarah = false,
        OriginalMapEntity142Definition? entity142 = null,
        bool omitEntity142 = false,
        OriginalMapAstralZoneDefinition? astralZone = null,
        bool omitAstralZone = false,
        OriginalMapMessengerAcceptanceDefinition? messengerAcceptance = null,
        bool omitMessengerAcceptance = false,
        OriginalMapCastleGateDefinition? castleGate = null,
        bool omitCastleGate = false)
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        ushort[] admittedWords = [.. words];
        admittedWords[Index(
            OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
            OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY)] |=
            OriginalMapTraversal.CollisionMask;
        admittedWords[Index(
            OriginalMapRuntimeAdmission.ControlledStepCopySourceX,
            OriginalMapRuntimeAdmission.ControlledStepCopySourceY)] &=
            unchecked((ushort)~OriginalMapTraversal.CollisionMask);
        admittedWords[Index(
            OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationX,
            OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationY)] |=
            OriginalMapTraversal.CollisionMask;
        admittedWords[Index(3, 3)] = (ushort)(
            (admittedWords[Index(3, 3)] & ~OriginalMapTraversal.CollisionMask) |
            OriginalMapTraversal.LeftStairMask);
        admittedWords[Index(4, 4)] = (ushort)(
            (admittedWords[Index(4, 4)] & ~OriginalMapTraversal.CollisionMask) |
            OriginalMapTraversal.LeftStairMask);
        OriginalMapAreaCatalog admittedAreas = areaCatalog ?? AcceptedAreaCatalog();
        OriginalMapSameMapWarpCatalog admittedWarps =
            sameMapWarps ?? AcceptedSameMapWarps(map);
        WorkingMapLayout workingLayout = new(admittedWords);
        OriginalMapBlockCatalog admittedBlocks = blockCatalog ?? AcceptedBlockCatalog();
        OriginalMapEntityPopulation admittedPopulation =
            entityPopulation ?? AcceptedEntityPopulation(map);
        OriginalMapExplorationRuntimeDefinition initialRuntime = new(
            map,
            workingLayout,
            admittedBlocks,
            admittedAreas,
            admittedPopulation,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            OriginalMapRuntimeAdmission.SelectedInitIdentity,
            OriginalMapRuntimeAdmission.AcceptedDecodedLayoutDigest,
            OriginalMapRuntimeAdmission.AcceptedCollisionProjectionDigest,
            useProjectionDigestOverride: true);
        OriginalMapExplorationRuntimeCatalog runtimeCatalog =
            AcceptedOriginalMapRuntimeCatalog.Create(initialRuntime);
        return new OriginalMapImportDefinition(
            map,
            workingLayout,
            admittedBlocks,
            admittedAreas,
            admittedPopulation,
            visualResourceSelection ?? AcceptedVisualResourceSelection(map),
            new OriginalMapControlledAdmission(
                map,
                new MapPosition(
                    OriginalMapRuntimeAdmission.StartX,
                    OriginalMapRuntimeAdmission.StartY),
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            ControlledStepCopy(map),
            admittedWarps,
            ["natural-route-and-effects-unknown"],
            omitRoofOnLoadClear
                ? null
                : roofOnLoadClear ?? RoofOnLoadClear(map, admittedWarps, admittedAreas),
            omitBowieDoorStepCopy ? null : bowieDoorStepCopy ?? BowieDoorStepCopy(map),
            omitZone601 || entityPopulation is not null
                ? null
                : zone601 ?? AcceptedZone601(map),
            omitSarah || entityPopulation is not null
                ? null
                : sarah ?? AcceptedSarah(map),
            omitEntity142 || entityPopulation is not null
                ? null
                : entity142 ?? AcceptedEntity142(map),
            omitAstralZone || entityPopulation is not null || omitZone601 || omitSarah ||
                omitEntity142
                ? null
                : astralZone ?? AcceptedAstralZone(map),
            omitMessengerAcceptance || entityPopulation is not null || omitZone601 ||
                omitSarah || omitEntity142 || omitAstralZone
                ? null
                : messengerAcceptance ?? AcceptedOriginalMapMessenger.Create(map),
            omitCastleGate || omitMessengerAcceptance || entityPopulation is not null ||
                omitZone601 || omitSarah || omitEntity142 || omitAstralZone
                ? null
                : castleGate ?? AcceptedOriginalMapCastleGate.Create(map),
            runtimeCatalog,
            AcceptedOriginalMapRuntimeCatalog.NorthTransition());
    }

    private static OriginalMapStepCopyDefinition BowieDoorStepCopy(MapId map) =>
        new(
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyRecordOrdinal),
            new MapPosition(
                OriginalMapRuntimeAdmission.BowieDoorStepCopyTriggerX,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyTriggerY),
            new WorkingMapBlockCopy(
                OriginalMapRuntimeAdmission.BowieDoorStepCopySourceX,
                OriginalMapRuntimeAdmission.BowieDoorStepCopySourceY,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationX,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationY,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyWidth,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyHeight));

    private static OriginalMapSameMapWarpCatalog AcceptedSameMapWarps(MapId map) =>
        new(
        [
            SameMapWarp(
                map,
                OriginalMapRuntimeAdmission.SchoolWarpRecordOrdinal,
                OriginalMapRuntimeAdmission.SchoolWarpTriggerX,
                OriginalMapRuntimeAdmission.SchoolWarpTriggerY,
                OriginalMapRuntimeAdmission.SchoolWarpDestinationX,
                OriginalMapRuntimeAdmission.SchoolWarpDestinationY,
                OriginalMapRuntimeAdmission.SchoolWarpOpaqueFacing),
            SameMapWarp(
                map,
                OriginalMapRuntimeAdmission.HouseWarpRecordOrdinal,
                OriginalMapRuntimeAdmission.HouseWarpTriggerX,
                OriginalMapRuntimeAdmission.HouseWarpTriggerY,
                OriginalMapRuntimeAdmission.HouseWarpDestinationX,
                OriginalMapRuntimeAdmission.HouseWarpDestinationY,
                OriginalMapRuntimeAdmission.HouseWarpOpaqueFacing),
        ]);

    private static OriginalMapSameMapWarpDefinition SameMapWarp(
        MapId map,
        int ordinal,
        int triggerX,
        int triggerY,
        int destinationX,
        int destinationY,
        byte opaqueFacing) =>
        new(
            new OriginalMapSameMapWarpIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.SameMapWarpResourceId,
                ordinal),
            new MapPosition(triggerX, triggerY),
            new MapPosition(destinationX, destinationY),
            opaqueFacing);

    private static OriginalMapRoofOnLoadDefinition RoofOnLoadClear(
        MapId map,
        OriginalMapSameMapWarpCatalog warps,
        OriginalMapAreaCatalog areas) =>
        new(
            new OriginalMapRoofOnLoadIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.RoofOnLoadResourceId,
                OriginalMapRuntimeAdmission.HouseRoofOnLoadRecordOrdinal),
            new MapPosition(
                OriginalMapRuntimeAdmission.HouseRoofSourceTriggerX,
                OriginalMapRuntimeAdmission.HouseRoofSourceTriggerY),
            new MapPosition(
                OriginalMapRuntimeAdmission.HouseRoofClearDestinationX,
                OriginalMapRuntimeAdmission.HouseRoofClearDestinationY),
            OriginalMapRuntimeAdmission.HouseRoofClearWidth,
            OriginalMapRuntimeAdmission.HouseRoofClearHeight,
            warps.Records.Single(record => record.Identity.OneBasedRecordOrdinal ==
                OriginalMapRuntimeAdmission.HouseWarpRecordOrdinal).Identity,
            areas.Records[
                areas.Traversal.SelectActiveArea(new MapPosition(
                    OriginalMapRuntimeAdmission.HouseWarpDestinationX,
                    OriginalMapRuntimeAdmission.HouseWarpDestinationY))!.OneBasedRecordOrdinal - 1]
                .Identity);

    private static OriginalMapVisualResourceSelection AcceptedVisualResourceSelection(MapId map) =>
        new(map, paletteIndex: 0, [0, 37, 43, 53, 66]);

    private static OriginalMapZone601Definition AcceptedZone601(MapId map) =>
        new(
            new OriginalMapZoneEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.Zone601ResourceId,
                OriginalMapRuntimeAdmission.Zone601RecordOrdinal,
                OriginalMapRuntimeAdmission.Zone601TargetIdentity),
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601TriggerX,
                OriginalMapRuntimeAdmission.Zone601TriggerY),
            OriginalMapRuntimeAdmission.Zone601GateFlag,
            OriginalMapRuntimeAdmission.Zone601BlockingSequenceIdentity,
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.Zone601ActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.Zone601LogicalActorId,
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601ActorInitialX,
                OriginalMapRuntimeAdmission.Zone601ActorInitialY),
            OriginalMapRuntimeAdmission.Zone601ActorInitialOpaqueFacing,
            OriginalMapRuntimeAdmission.Zone601ActorInitialBehaviorIdentity,
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601ActorBlockingEndX,
                OriginalMapRuntimeAdmission.Zone601ActorBlockingEndY),
            OriginalMapRuntimeAdmission.Zone601ActorBlockingEndOpaqueFacing,
            OriginalMapRuntimeAdmission.Zone601OpaqueFaceWaitOperand,
            OriginalMapRuntimeAdmission.Zone601TextIds,
            OriginalMapRuntimeAdmission.Zone601AmbientBehaviorIdentity,
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601AmbientCenterX,
                OriginalMapRuntimeAdmission.Zone601AmbientCenterY),
            OriginalMapRuntimeAdmission.Zone601AmbientRange,
            OriginalMapRuntimeAdmission.Zone601BlockingStages);

    private static OriginalMapSarahDefinition AcceptedSarah(MapId map) =>
        new(
            new OriginalMapSarahEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SarahEntityEventResourceId,
                OriginalMapRuntimeAdmission.SarahEntityEventRecordOrdinal,
                OriginalMapRuntimeAdmission.SarahEntityEventTargetIdentity,
                OriginalMapRuntimeAdmission.SarahEntityEventOpaqueFacing),
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.SarahActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.SarahLogicalActorId,
            new MapPosition(
                OriginalMapRuntimeAdmission.SarahActorInitialX,
                OriginalMapRuntimeAdmission.SarahActorInitialY),
            OriginalMapRuntimeAdmission.SarahActorInitialOpaqueFacing,
            new MapPosition(
                OriginalMapRuntimeAdmission.SarahPlayerInteractionX,
                OriginalMapRuntimeAdmission.SarahPlayerInteractionY),
            OriginalMapRuntimeAdmission.SarahPlayerInteractionOpaqueFacing,
            OriginalMapRuntimeAdmission.SarahLaterBranchFlag603,
            OriginalMapRuntimeAdmission.SarahLaterBranchFlag602,
            OriginalMapRuntimeAdmission.SarahTemporaryRouteFlag256,
            OriginalMapRuntimeAdmission.SarahBlockingSequenceIdentity,
            new MapPosition(
                OriginalMapRuntimeAdmission.SarahFirstWaypointX,
                OriginalMapRuntimeAdmission.SarahFirstWaypointY),
            OriginalMapRuntimeAdmission.SarahRestoredOpaqueFacing,
            OriginalMapRuntimeAdmission.SarahFirstTextIds,
            OriginalMapRuntimeAdmission.SarahRepeatTextIds,
            OriginalMapRuntimeAdmission.SarahFirstStages,
            OriginalMapRuntimeAdmission.SarahRepeatStages);

    private static OriginalMapEntity142Definition AcceptedEntity142(MapId map) =>
        new(
            new OriginalMapEntity142EventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.Entity142EventResourceId,
                OriginalMapRuntimeAdmission.Entity142EventRecordOrdinal,
                OriginalMapRuntimeAdmission.Entity142EventTargetIdentity,
                OriginalMapRuntimeAdmission.Entity142EventOpaqueFacing),
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.Entity142LogicalActorId,
            OriginalMapRuntimeAdmission.Entity142PhysicalActorSlot,
            new MapPosition(
                OriginalMapRuntimeAdmission.Entity142ActorX,
                OriginalMapRuntimeAdmission.Entity142ActorY),
            OriginalMapRuntimeAdmission.Entity142ActorOpaqueFacing,
            new MapPosition(
                OriginalMapRuntimeAdmission.Entity142PlayerInteractionX,
                OriginalMapRuntimeAdmission.Entity142PlayerInteractionY),
            OriginalMapRuntimeAdmission.Entity142PlayerInteractionOpaqueFacing,
            OriginalMapRuntimeAdmission.Entity142FirstInteractionFlag261,
            OriginalMapRuntimeAdmission.Entity142CompletionFlag602,
            OriginalMapRuntimeAdmission.Entity142FirstTextIds,
            OriginalMapRuntimeAdmission.Entity142RepeatTextIds,
            OriginalMapRuntimeAdmission.Entity142FirstStages,
            OriginalMapRuntimeAdmission.Entity142RepeatStages);

    private static OriginalMapAstralZoneDefinition AcceptedAstralZone(MapId map) =>
        new(
            new OriginalMapAstralZoneEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.AstralZoneEventResourceId,
                OriginalMapRuntimeAdmission.AstralZoneEventRecordOrdinal,
                OriginalMapRuntimeAdmission.AstralZoneEventTargetIdentity),
            new MapPosition(
                OriginalMapRuntimeAdmission.AstralZoneTriggerX,
                OriginalMapRuntimeAdmission.AstralZoneTriggerY),
            OriginalMapRuntimeAdmission.AstralZonePositionProgramIdentity,
            OriginalMapRuntimeAdmission.AstralZoneMessengerCompletionFlag603,
            OriginalMapRuntimeAdmission.AstralZoneRequiredEntity142Flag602,
            OriginalMapRuntimeAdmission.AstralZoneCompletionFlag260,
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.SarahActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.SarahLogicalActorId,
            new MapPosition(
                OriginalMapRuntimeAdmission.AstralZoneSarahDestinationX,
                OriginalMapRuntimeAdmission.AstralZoneSarahDestinationY),
            OriginalMapRuntimeAdmission.AstralZoneSarahOpaqueFacing,
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.Zone601ActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.Zone601LogicalActorId,
            new MapPosition(
                OriginalMapRuntimeAdmission.AstralZoneActor128DestinationX,
                OriginalMapRuntimeAdmission.AstralZoneActor128DestinationY),
            OriginalMapRuntimeAdmission.AstralZoneActor128OpaqueFacing,
            OriginalMapRuntimeAdmission.AstralZoneTextIds,
            OriginalMapRuntimeAdmission.AstralZoneStages);

    private static OriginalMapEntityPopulation AcceptedEntityPopulation(MapId map) =>
        AcceptedOriginalMapMessenger.CreateEntityPopulation(map);

    private static OriginalMapEntityPopulation ProjectAuthoredEntityPopulation(
        MapId map,
        string resourceId,
        int count,
        bool acceptedDigestOverride = false,
        bool mutateFirstTail = false,
        bool allFixed = false)
    {
        OriginalMapEntityDefinition[] records = Enumerable.Range(0, count)
            .Select(index => new OriginalMapEntityDefinition(
                new OriginalMapEntityRecordIdentity(resourceId, index + 1),
                rawX: index == 0
                    ? (byte)OriginalMapRuntimeAdmission.SarahActorInitialX
                    : index == 2
                    ? (byte)OriginalMapRuntimeAdmission.Zone601ActorInitialX
                    : index == OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal - 1
                    ? (byte)OriginalMapRuntimeAdmission.Entity142ActorX
                    : checked((byte)index),
                rawY: index == 0
                    ? (byte)OriginalMapRuntimeAdmission.SarahActorInitialY
                    : index == 2
                    ? (byte)OriginalMapRuntimeAdmission.Zone601ActorInitialY
                    : index == OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal - 1
                    ? (byte)OriginalMapRuntimeAdmission.Entity142ActorY
                    : (byte)0,
                opaqueFacing: index == 0
                    ? OriginalMapRuntimeAdmission.SarahActorInitialOpaqueFacing
                    : index == 2
                    ? OriginalMapRuntimeAdmission.Zone601ActorInitialOpaqueFacing
                    : index == OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal - 1
                    ? OriginalMapRuntimeAdmission.Entity142ActorOpaqueFacing
                    : (byte)3,
                mapSprite: index == 2
                    ? (byte)195
                    : index == OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal - 1
                    ? OriginalMapRuntimeAdmission.Entity142ActorMapSprite
                    : checked((byte)(index + 1)),
                index == 0 && mutateFirstTail
                    ? [1, 0, 0, 0]
                    : index == 0
                        ? [0, 4, 0x60, 0xCE]
                    : index == 2
                        ? [0, 4, 97, 2]
                    : index == OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal - 1
                        ? [0, 4, 0x60, 0xCE]
                    : !allFixed && (index ==
                            OriginalMapRuntimeAdmission.AcceptedFixedEntityRecordCount - 1 ||
                        index > OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal - 1)
                        ? [0xFF, checked((byte)index), 0, 1]
                        : [0, 0, 0, 0]))
            .ToArray();
        MapSetupId setup = new(OriginalMapRuntimeAdmission.SelectedSetupId);
        return acceptedDigestOverride
            ? new OriginalMapEntityPopulation(
                map,
                setup,
                records,
                OriginalMapRuntimeAdmission.AcceptedEntityProjectionDigest)
            : new OriginalMapEntityPopulation(map, setup, records);
    }

    private static OriginalMapBlockCatalog AcceptedBlockCatalog() =>
        new(
            ProjectAuthoredBlockDefinitions(
                OriginalMapRuntimeAdmission.AcceptedBlocksetResourceId,
                OriginalMapRuntimeAdmission.AcceptedBlockCount),
            OriginalMapRuntimeAdmission.AcceptedBlocksetProjectionDigest);

    private static OriginalMapBlockCatalog ProjectAuthoredBlockCatalog(
        string resourceId,
        int count,
        bool mutateFirstWord = false) =>
        new(ProjectAuthoredBlockDefinitions(resourceId, count, mutateFirstWord));

    private static IEnumerable<OriginalMapBlockDefinition> ProjectAuthoredBlockDefinitions(
        string resourceId,
        int count,
        bool mutateFirstWord = false) =>
        Enumerable.Range(0, count).Select(index =>
        {
            ushort[] words = new ushort[OriginalMapBlockDefinition.OpaqueWordCount];
            if (index == 0 && mutateFirstWord)
            {
                words[0] = 1;
            }

            return new OriginalMapBlockDefinition(
                new OriginalMapBlockRecordIdentity(resourceId, index),
                words);
        });

    private static OriginalMapTraversalArea[] AcceptedAreas() =>
    [
        new OriginalMapTraversalArea(0, 0, 50, 31),
        new OriginalMapTraversalArea(51, 0, 61, 9),
        new OriginalMapTraversalArea(51, 10, 61, 19),
    ];

    private static OriginalMapAreaCatalog AcceptedAreaCatalog(
        IEnumerable<OriginalMapTraversalArea>? activeAreas = null) =>
        new((activeAreas ?? AcceptedAreas()).Select(
            (area, index) => AcceptedAreaDefinition(index + 1, area)));

    private static OriginalMapAreaDefinition[] AcceptedAreaDefinitions() =>
        AcceptedAreas()
            .Select((area, index) => AcceptedAreaDefinition(index + 1, area))
            .ToArray();

    private static OriginalMapAreaDefinition AcceptedAreaDefinition(
        int oneBasedRecordOrdinal,
        OriginalMapTraversalArea area,
        byte defaultMusic = 8) =>
        new(
            new OriginalMapAreaRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedAreaResourceId,
                oneBasedRecordOrdinal),
            area,
            new OriginalMapAreaWordPair(0, oneBasedRecordOrdinal == 1 ? (ushort)32 : (ushort)0),
            new OriginalMapAreaWordPair(0, 0),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaBytePair(0, 0),
            new OriginalMapAreaBytePair(0, 0),
            mainLayerType: 0,
            defaultMusic);

    private static OriginalMapStepCopyDefinition ControlledStepCopy(MapId map) =>
        new(
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                OriginalMapRuntimeAdmission.ControlledStepCopyRecordOrdinal),
            new MapPosition(
                OriginalMapRuntimeAdmission.ControlledStepCopyTriggerX,
                OriginalMapRuntimeAdmission.ControlledStepCopyTriggerY),
            new WorkingMapBlockCopy(
                OriginalMapRuntimeAdmission.ControlledStepCopySourceX,
                OriginalMapRuntimeAdmission.ControlledStepCopySourceY,
                OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
                OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY,
                OriginalMapRuntimeAdmission.ControlledStepCopyWidth,
                OriginalMapRuntimeAdmission.ControlledStepCopyHeight));

    private static ApplyPrivateOriginalMapLayoutMutationCommand MutationCommand(
        PrivateOriginalMapSessionSnapshot snapshot) =>
        new(
            snapshot.Definition.ControlledStepCopy!.Identity,
            snapshot.SimulationStep);

    private static void AssertRejectedMutation(
        GameSession session,
        ApplyPrivateOriginalMapLayoutMutationCommand command,
        PrivateOriginalMapSessionSnapshot expectedSnapshot,
        PrivateOriginalMapLayoutMutationFailureCode expectedCode)
    {
        PrivateOriginalMapLayoutMutationRejected rejected =
            Assert.IsType<PrivateOriginalMapLayoutMutationRejected>(
                session.ApplyPrivateOriginalMapLayoutMutation(command));
        Assert.Same(expectedSnapshot, rejected.Snapshot);
        Assert.Same(expectedSnapshot, session.PrivateOriginalMapSnapshot);
        Assert.Equal(expectedCode, rejected.Diagnostic.Code);
    }

    private static (int, int, int, int, int, int) Geometry(WorkingMapBlockCopy copy) =>
        (copy.SourceX, copy.SourceY, copy.DestinationX, copy.DestinationY,
            copy.Width, copy.Height);

    private static OriginalMapImportReceipt Receipt(
        string? contentDigest = null,
        IEnumerable<string>? capabilities = null,
        string? romSha256 = null,
        string? upstreamRepository = null,
        string? upstreamCommit = null,
        string? decodedLayoutDigest = null,
        string? collisionProjectionDigest = null,
        IEnumerable<string>? evidenceOwnerIds = null) =>
        new(
            OriginalMapRuntimeAdmission.PackageId,
            OriginalMapRuntimeAdmission.SchemaVersion,
            contentDigest ?? OriginalMapRuntimeAdmission.AcceptedContentDigest,
            decodedLayoutDigest ?? OriginalMapRuntimeAdmission.AcceptedDecodedLayoutDigest,
            collisionProjectionDigest ??
                OriginalMapRuntimeAdmission.AcceptedCollisionProjectionDigest,
            ContentProfile.PrivateLocal,
            new OriginalMapImportProvenance(
                OriginalMapRuntimeAdmission.PackageId,
                romSha256 ?? OriginalMapRuntimeAdmission.AcceptedRomSha256,
                upstreamRepository ?? OriginalMapRuntimeAdmission.AcceptedUpstreamRepository,
                upstreamCommit ?? OriginalMapRuntimeAdmission.AcceptedUpstreamCommit),
            evidenceOwnerIds ?? OriginalMapRuntimeAdmission.RequiredEvidenceOwners,
            capabilities ?? OriginalMapRuntimeAdmission.RequiredCapabilities);

    private static void AssertRejectedReceipt(
        OriginalMapImportDefinition definition,
        OriginalMapImportReceipt receipt,
        OriginalMapImportFailureCode expectedCode)
    {
        PrivateOriginalMapGameSessionStartRejected rejected =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    new AcceptedSource(Accepted(definition, receipt)),
                    Request()));
        Assert.Equal(expectedCode, rejected.Diagnostic.Code);
    }

    private static OriginalMapImportRequest Request() =>
        new(
            OriginalMapRuntimeAdmission.PackageId,
            ContentProfile.PrivateLocal,
            OriginalMapRuntimeAdmission.AcceptedContentDigest);

    private static ushort[] EmptyWords() => new ushort[WorkingMapLayout.WordCount];

    private static void FillRoofRegion(ushort[] words, ushort value)
    {
        for (int y = 0; y < OriginalMapRuntimeAdmission.HouseRoofClearHeight; y++)
        {
            for (int x = 0; x < OriginalMapRuntimeAdmission.HouseRoofClearWidth; x++)
            {
                words[Index(
                    OriginalMapRuntimeAdmission.HouseRoofClearDestinationX + x,
                    OriginalMapRuntimeAdmission.HouseRoofClearDestinationY + y)] = value;
            }
        }
    }

    private static void AssertRoofRegion(WorkingMapLayout layout, ushort expected)
    {
        for (int y = 0; y < OriginalMapRuntimeAdmission.HouseRoofClearHeight; y++)
        {
            for (int x = 0; x < OriginalMapRuntimeAdmission.HouseRoofClearWidth; x++)
            {
                Assert.Equal(
                    expected,
                    layout[
                        OriginalMapRuntimeAdmission.HouseRoofClearDestinationX + x,
                        OriginalMapRuntimeAdmission.HouseRoofClearDestinationY + y]);
            }
        }
    }

    private static int Index(int x, int y) =>
        (y * WorkingMapLayout.ColumnCount) + x;

    private sealed class AcceptedSource(OriginalMapImportAccepted accepted) : IOriginalMapImportSource
    {
        public int AdmitCalls { get; private set; }

        public OriginalMapImportResult Admit(OriginalMapImportRequest request)
        {
            AdmitCalls++;
            return accepted;
        }
    }

    private sealed class RejectedSource(OriginalMapImportDiagnostic diagnostic) :
        IOriginalMapImportSource
    {
        public OriginalMapImportResult Admit(OriginalMapImportRequest request) =>
            new OriginalMapImportRejected(diagnostic);
    }
}

internal static class AcceptedOriginalMapRuntimeCatalog
{
    internal static OriginalMapExplorationRuntimeCatalog Create(
        OriginalMapExplorationRuntimeDefinition initialRuntime) =>
        new([initialRuntime, Map19Runtime()]);

    internal static OriginalMapCrossMapTransitionDefinition NorthTransition() =>
        new(
            new OriginalMapCrossMapTransitionIdentity(
                ContentProfile.PrivateLocal,
                new MapId(OriginalMapRuntimeAdmission.MapId),
                OriginalMapRuntimeAdmission.SameMapWarpResourceId,
                OriginalMapRuntimeAdmission.NorthMap19WarpRecordOrdinal),
            OriginalMapRuntimeAdmission.NorthMap19WarpSourceTriggerX,
            OriginalMapRuntimeAdmission.NorthMap19WarpSourceTriggerY,
            new MapPosition(
                OriginalMapRuntimeAdmission.NorthMap19WarpApproachX,
                OriginalMapRuntimeAdmission.NorthMap19WarpApproachY),
            OriginalMapRuntimeAdmission.NorthMap19WarpDirection,
            new MapPosition(
                OriginalMapRuntimeAdmission.NorthMap19WarpTriggerX,
                OriginalMapRuntimeAdmission.NorthMap19WarpTriggerY),
            new MapId(OriginalMapRuntimeAdmission.Map19Id),
            new MapPosition(
                OriginalMapRuntimeAdmission.NorthMap19WarpDestinationX,
                OriginalMapRuntimeAdmission.NorthMap19WarpDestinationY),
            OriginalMapRuntimeAdmission.NorthMap19WarpDestinationOpaqueFacing);

    private static OriginalMapExplorationRuntimeDefinition Map19Runtime()
    {
        MapId map = new(OriginalMapRuntimeAdmission.Map19Id);
        WorkingMapLayout layout = new(new ushort[WorkingMapLayout.WordCount]);
        OriginalMapBlockCatalog blocks = new(
            Enumerable.Range(0, OriginalMapRuntimeAdmission.Map19BlockCount)
                .Select(index => new OriginalMapBlockDefinition(
                    new OriginalMapBlockRecordIdentity(
                        OriginalMapRuntimeAdmission.Map19BlocksetResourceId,
                        index),
                    new ushort[OriginalMapBlockDefinition.OpaqueWordCount])),
            OriginalMapRuntimeAdmission.Map19BlocksetProjectionDigest);
        OriginalMapAreaCatalog areas = new(
        [
            new OriginalMapAreaDefinition(
                new OriginalMapAreaRecordIdentity(
                    OriginalMapRuntimeAdmission.Map19AreaResourceId,
                    1),
                new OriginalMapTraversalArea(0, 0, 40, 31),
                new OriginalMapAreaWordPair(0, 32),
                new OriginalMapAreaWordPair(0, 0),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaBytePair(0, 0),
                new OriginalMapAreaBytePair(0, 0),
                mainLayerType: 0,
                defaultMusic: 38),
        ]);
        OriginalMapEntityDefinition[] entities = Enumerable.Range(
                0,
                OriginalMapRuntimeAdmission.Map19EntityRecordCount)
            .Select(index => new OriginalMapEntityDefinition(
                new OriginalMapEntityRecordIdentity(
                    OriginalMapRuntimeAdmission.Map19EntityListResourceId,
                    index + 1),
                rawX: checked((byte)(index + 1)),
                rawY: index < OriginalMapRuntimeAdmission.Map19FixedEntityRecordCount
                    ? (byte)1
                    : (byte)2,
                opaqueFacing: 0,
                mapSprite: checked((byte)(index + 1)),
                index < OriginalMapRuntimeAdmission.Map19FixedEntityRecordCount
                    ? [0, 0, 0, 0]
                    : [0xFF, checked((byte)(index + 1)), 2, 1]))
            .ToArray();
        OriginalMapEntityPopulation population = new(
            map,
            new MapSetupId(OriginalMapRuntimeAdmission.Map19SelectedSetupId),
            entities,
            OriginalMapRuntimeAdmission.Map19EntityProjectionDigest);
        return new OriginalMapExplorationRuntimeDefinition(
            map,
            layout,
            blocks,
            areas,
            population,
            new MapSetupId(OriginalMapRuntimeAdmission.Map19SelectedSetupId),
            OriginalMapRuntimeAdmission.Map19SelectedInitIdentity,
            OriginalMapRuntimeAdmission.Map19DecodedLayoutDigest,
            OriginalMapRuntimeAdmission.Map19CollisionProjectionDigest,
            useProjectionDigestOverride: true);
    }
}

internal static class AcceptedOriginalMapMessenger
{
    internal static OriginalMapEntityPopulation CreateEntityPopulation(MapId map)
    {
        OriginalMapEntityDefinition[] records = Enumerable.Range(
            0,
            OriginalMapRuntimeAdmission.AcceptedEntityRecordCount)
            .Select(index => CreateEntityRecord(index))
            .ToArray();
        return new OriginalMapEntityPopulation(
            map,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            records,
            OriginalMapRuntimeAdmission.AcceptedEntityProjectionDigest);
    }

    private static OriginalMapEntityDefinition CreateEntityRecord(int index)
    {
        int ordinal = index + 1;
        (byte x, byte y, byte facing, byte sprite, byte[] tail) = ordinal switch
        {
            OriginalMapRuntimeAdmission.SarahActorSourceRecordOrdinal =>
                ((byte)OriginalMapRuntimeAdmission.SarahActorInitialX,
                    (byte)OriginalMapRuntimeAdmission.SarahActorInitialY,
                    OriginalMapRuntimeAdmission.SarahActorInitialOpaqueFacing,
                    (byte)1,
                    new byte[] { 0, 4, 0x60, 0xCE }),
            2 => ((byte)0xEA, (byte)8, (byte)1, (byte)5,
                new byte[] { 0xFF, 42, 8, 3 }),
            OriginalMapRuntimeAdmission.Zone601ActorSourceRecordOrdinal =>
                ((byte)OriginalMapRuntimeAdmission.Zone601ActorInitialX,
                    (byte)OriginalMapRuntimeAdmission.Zone601ActorInitialY,
                    OriginalMapRuntimeAdmission.Zone601ActorInitialOpaqueFacing,
                    (byte)195,
                    new byte[] { 0, 4, 97, 2 }),
            OriginalMapRuntimeAdmission.MessengerGuard138SourceRecordOrdinal =>
                ((byte)OriginalMapRuntimeAdmission.MessengerGuard138X,
                    (byte)OriginalMapRuntimeAdmission.MessengerGuard138Y,
                    OriginalMapRuntimeAdmission.MessengerGuard138OpaqueFacing,
                    OriginalMapRuntimeAdmission.MessengerGuardMapSprite,
                    new byte[] { 0, 4, 0x60, 0xCE }),
            OriginalMapRuntimeAdmission.MessengerGuard139SourceRecordOrdinal =>
                ((byte)OriginalMapRuntimeAdmission.MessengerGuard139X,
                    (byte)OriginalMapRuntimeAdmission.MessengerGuard139Y,
                    OriginalMapRuntimeAdmission.MessengerGuard139OpaqueFacing,
                    OriginalMapRuntimeAdmission.MessengerGuardMapSprite,
                    new byte[] { 0, 4, 0x60, 0xCE }),
            OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal =>
                ((byte)OriginalMapRuntimeAdmission.Entity142ActorX,
                    (byte)OriginalMapRuntimeAdmission.Entity142ActorY,
                    OriginalMapRuntimeAdmission.Entity142ActorOpaqueFacing,
                    OriginalMapRuntimeAdmission.Entity142ActorMapSprite,
                    new byte[] { 0, 4, 0x60, 0xCE }),
            OriginalMapRuntimeAdmission.MessengerActor143SourceRecordOrdinal =>
                ((byte)OriginalMapRuntimeAdmission.MessengerActor143InitialX,
                    (byte)OriginalMapRuntimeAdmission.MessengerActor143InitialY,
                    OriginalMapRuntimeAdmission.MessengerActor143InitialOpaqueFacing,
                    OriginalMapRuntimeAdmission.MessengerActor143MapSprite,
                    new byte[] { 0, 4, 0x60, 0xCE }),
            _ when ordinal == OriginalMapRuntimeAdmission.AcceptedFixedEntityRecordCount ||
                    ordinal > OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal =>
                (checked((byte)index), (byte)0, (byte)3, checked((byte)(index + 1)),
                    new byte[] { 0xFF, checked((byte)index), 0, 1 }),
            _ => (checked((byte)index), (byte)0, (byte)3,
                checked((byte)(index + 1)), new byte[] { 0, 0, 0, 0 }),
        };
        return new OriginalMapEntityDefinition(
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                ordinal),
            x,
            y,
            facing,
            sprite,
            tail);
    }

    internal static OriginalMapMessengerAcceptanceDefinition Create(MapId map) =>
        new(
            new OriginalMapMessengerZoneEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.MessengerZoneEventResourceId,
                OriginalMapRuntimeAdmission.MessengerZoneEventRecordOrdinal,
                OriginalMapRuntimeAdmission.MessengerZoneEventTargetIdentity),
            new MapPosition(
                OriginalMapRuntimeAdmission.MessengerApproachX,
                OriginalMapRuntimeAdmission.MessengerApproachY),
            OriginalMapRuntimeAdmission.MessengerEntryDirection,
            new MapPosition(
                OriginalMapRuntimeAdmission.MessengerTriggerX,
                OriginalMapRuntimeAdmission.MessengerTriggerY),
            OriginalMapRuntimeAdmission.MessengerProgramIdentity,
            OriginalMapRuntimeAdmission.MessengerAcceptedBranchProgramIdentity,
            OriginalMapRuntimeAdmission.MessengerControlShapeSha256,
            OriginalMapRuntimeAdmission.MessengerPromptReturn,
            OriginalMapRuntimeAdmission.MessengerPromptFlag89,
            OriginalMapRuntimeAdmission.MessengerJoinSelector,
            OriginalMapRuntimeAdmission.MessengerFlag600,
            OriginalMapRuntimeAdmission.MessengerFlag66,
            OriginalMapRuntimeAdmission.MessengerCompletionFlag603,
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.SarahActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.MessengerSarahCharacterId,
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.Entity142LogicalActorId,
            new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.MessengerActor143SourceRecordOrdinal),
            OriginalMapRuntimeAdmission.MessengerActor143LogicalId,
            new MapPosition(
                OriginalMapRuntimeAdmission.MessengerActor143InitialX,
                OriginalMapRuntimeAdmission.MessengerActor143InitialY),
            OriginalMapRuntimeAdmission.MessengerActor143InitialOpaqueFacing,
            OriginalMapRuntimeAdmission.MessengerTextIds,
            OriginalMapRuntimeAdmission.MessengerSpeakerOperands,
            OriginalMapRuntimeAdmission.MessengerJoinedCharacterIds,
            OriginalMapRuntimeAdmission.MessengerFollowers,
            OriginalMapRuntimeAdmission.MessengerGuards,
            new MapPosition(
                OriginalMapRuntimeAdmission.MessengerTriggerX,
                OriginalMapRuntimeAdmission.MessengerTriggerY),
            OriginalMapRuntimeAdmission.MessengerEndpointOpaqueFacing,
            OriginalMapRuntimeAdmission.MessengerTerminalIdentity,
            OriginalMapRuntimeAdmission.MessengerStages);
}

internal static class AcceptedOriginalMapCastleGate
{
    internal static OriginalMapCastleGateDefinition Create(MapId map) =>
        new(
            new OriginalMapZoneEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.CastleGateZoneEventResourceId,
                OriginalMapRuntimeAdmission.CastleGateZoneEventRecordOrdinal,
                OriginalMapRuntimeAdmission.CastleGateZoneEventTargetIdentity),
            new MapPosition(
                OriginalMapRuntimeAdmission.CastleGateApproachX,
                OriginalMapRuntimeAdmission.CastleGateApproachY),
            OriginalMapRuntimeAdmission.CastleGateEntryDirection,
            new MapPosition(
                OriginalMapRuntimeAdmission.CastleGateTriggerX,
                OriginalMapRuntimeAdmission.CastleGateTriggerY),
            OriginalMapRuntimeAdmission.CastleGateProgramIdentity,
            OriginalMapRuntimeAdmission.CastleGateControlShapeSha256,
            OriginalMapRuntimeAdmission.CastleGateTextCursorId,
            OriginalMapRuntimeAdmission.CastleGateCompletionFlag604,
            OriginalMapRuntimeAdmission.CastleGateSourceProgramOperationCount,
            OriginalMapRuntimeAdmission.CastleGateProjectionSourceOperationIndices,
            OriginalMapRuntimeAdmission.CastleGateGuardMoves,
            OriginalMapRuntimeAdmission.CastleGateStages);
}

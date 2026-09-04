using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalMapBattleBridgeTests
{
    [Fact]
    public void ExactPrivateSessionBindsOneControlledStartBridgeWithoutPresentationDependency()
    {
        PrivateOriginalMapGameSessionStarted started = Start();

        PrivateOriginalMapBattleBridgeBound bound = Assert.IsType<
            PrivateOriginalMapBattleBridgeBound>(
            started.Session.BindPrivateOriginalMapBattleBridge(Battle()));

        Assert.Equal(PrivateOriginalMapBattleBridgeStatus.Ready, bound.Bridge.Status);
        Assert.Equal(
            PrivateOriginalMapBattleBridgeAdmission.Capability,
            bound.Bridge.Definition.Capability);
        Assert.Equal(new MapId("map3"), bound.Bridge.Definition.TriggerMap);
        Assert.Equal(new MapPosition(56, 3), bound.Bridge.Definition.TriggerPosition);
        Assert.Equal("project-authored-tactical-battle", bound.Bridge.Definition.Rules.Battle.Value);
        Assert.Same(bound.Bridge, started.Session.PrivateOriginalMapBattleBridge);
        Assert.DoesNotContain(
            typeof(PrivateOriginalMapBattleBridgeDefinition).GetProperties(),
            property => property.Name.Contains("Flag", StringComparison.Ordinal) ||
                property.Name.Contains("Effect", StringComparison.Ordinal) ||
                property.Name.StartsWith("Return", StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(PrivateOriginalMapBattleBridgeSnapshot).GetProperties(),
            property => property.PropertyType == typeof(PublicSyntheticFlagStateSnapshot));

        PrivateOriginalMapBattleBridgeBindingRejected duplicate = Assert.IsType<
            PrivateOriginalMapBattleBridgeBindingRejected>(
            started.Session.BindPrivateOriginalMapBattleBridge(Battle()));
        Assert.Equal(
            PrivateOriginalMapBattleBridgeFailureCode.AlreadyBound,
            duplicate.Diagnostic.Code);
        Assert.Same(bound.Bridge, started.Session.PrivateOriginalMapBattleBridge);
    }

    [Fact]
    public void TriggerAndAcknowledgementsFailClosedWithoutChangingEitherState()
    {
        PrivateOriginalMapGameSessionStarted started = Start();
        GameSession session = started.Session;
        PrivateOriginalMapBattleBridgeSnapshot ready = Bind(started);
        PrivateOriginalMapSessionSnapshot traversal = session.PrivateOriginalMapSnapshot;

        AssertRejected(
            session,
            new RequestPrivateOriginalMapBattleBridgeCommand(
                new PrivateOriginalMapBattleBridgeId("wrong-bridge"),
                traversal.SimulationStep),
            PrivateOriginalMapBattleBridgeFailureCode.WrongTrigger,
            traversal,
            ready);
        AssertRejected(
            session,
            new RequestPrivateOriginalMapBattleBridgeCommand(
                ready.Definition.Bridge,
                traversal.SimulationStep + 1),
            PrivateOriginalMapBattleBridgeFailureCode.StaleTraversalStep,
            traversal,
            ready);

        PrivateOriginalMapBattleBridgeRequested requested = Assert.IsType<
            PrivateOriginalMapBattleBridgeRequested>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new RequestPrivateOriginalMapBattleBridgeCommand(
                    ready.Definition.Bridge,
                    traversal.SimulationStep)));
        Assert.Same(traversal, requested.Snapshot);
        Assert.Equal(PrivateOriginalMapBattleBridgeStatus.Pending, requested.Bridge.Status);

        AssertRejected(
            session,
            new AcknowledgePublicSyntheticBattleEntryCommand(
                requested.Bridge.Definition.Request,
                requested.Bridge.Definition.Rules.Battle,
                requested.Cue.Sequence + 1),
            PrivateOriginalMapBattleBridgeFailureCode.AcknowledgementMismatch,
            traversal,
            requested.Bridge);
        Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.Throws<InvalidOperationException>(() =>
            session.ApplyPrivateOriginalMapLayoutMutation(
                MutationCommand(traversal)));
        Assert.Same(traversal, session.PrivateOriginalMapSnapshot);
        Assert.Same(requested.Bridge, session.PrivateOriginalMapBattleBridge);
    }

    [Fact]
    public void PendingEntryCanBeDeclinedExactlyOnceBeforeMovementResumes()
    {
        PrivateOriginalMapGameSessionStarted started = Start();
        GameSession session = started.Session;
        PrivateOriginalMapBattleBridgeSnapshot ready = Bind(started);
        PrivateOriginalMapSessionSnapshot traversal = session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapBattleBridgeRequested requested = Assert.IsType<
            PrivateOriginalMapBattleBridgeRequested>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new RequestPrivateOriginalMapBattleBridgeCommand(
                    ready.Definition.Bridge,
                    traversal.SimulationStep)));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DeclinePrivateOriginalMapBattleBridgeEntryCommand(
                requested.Bridge.Definition.Bridge,
                requested.Bridge.Definition.Request,
                0));
        AssertRejected(
            session,
            new DeclinePrivateOriginalMapBattleBridgeEntryCommand(
                new PrivateOriginalMapBattleBridgeId("wrong-bridge"),
                requested.Bridge.Definition.Request,
                requested.Cue.Sequence),
            PrivateOriginalMapBattleBridgeFailureCode.AcknowledgementMismatch,
            traversal,
            requested.Bridge);
        AssertRejected(
            session,
            new DeclinePrivateOriginalMapBattleBridgeEntryCommand(
                requested.Bridge.Definition.Bridge,
                new PublicSyntheticBattleRequestId("wrong-request"),
                requested.Cue.Sequence),
            PrivateOriginalMapBattleBridgeFailureCode.AcknowledgementMismatch,
            traversal,
            requested.Bridge);
        AssertRejected(
            session,
            new DeclinePrivateOriginalMapBattleBridgeEntryCommand(
                requested.Bridge.Definition.Bridge,
                requested.Bridge.Definition.Request,
                requested.Cue.Sequence + 1),
            PrivateOriginalMapBattleBridgeFailureCode.AcknowledgementMismatch,
            traversal,
            requested.Bridge);

        PrivateOriginalMapBattleBridgeDeclined declined = Assert.IsType<
            PrivateOriginalMapBattleBridgeDeclined>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new DeclinePrivateOriginalMapBattleBridgeEntryCommand(
                    requested.Bridge.Definition.Bridge,
                    requested.Bridge.Definition.Request,
                    requested.Cue.Sequence)));

        Assert.Same(traversal, declined.Snapshot);
        Assert.Same(traversal, session.PrivateOriginalMapSnapshot);
        Assert.Equal(PrivateOriginalMapBattleBridgeStatus.Declined, declined.Bridge.Status);
        Assert.Equal(2, declined.Bridge.OperationSequence);
        Assert.Equal(requested.Cue.Sequence, declined.Bridge.LastCueSequence);
        Assert.False(declined.Bridge.IsBusy);
        Assert.Null(declined.Bridge.BattleState);
        Assert.Null(declined.Bridge.Completion);
        Assert.Null(declined.Bridge.Lifecycle);
        Assert.Null(declined.Bridge.ReturnSnapshot);
        Assert.Throws<InvalidOperationException>(() => declined.Bridge.Decline());

        AssertRejected(
            session,
            new DeclinePrivateOriginalMapBattleBridgeEntryCommand(
                declined.Bridge.Definition.Bridge,
                declined.Bridge.Definition.Request,
                declined.Bridge.LastCueSequence),
            PrivateOriginalMapBattleBridgeFailureCode.WrongState,
            traversal,
            declined.Bridge);
        AssertRejected(
            session,
            new AcknowledgePublicSyntheticBattleEntryCommand(
                declined.Bridge.Definition.Request,
                declined.Bridge.Definition.Rules.Battle,
                declined.Bridge.LastCueSequence),
            PrivateOriginalMapBattleBridgeFailureCode.WrongState,
            traversal,
            declined.Bridge);
        AssertRejected(
            session,
            new RequestPrivateOriginalMapBattleBridgeCommand(
                declined.Bridge.Definition.Bridge,
                traversal.SimulationStep),
            PrivateOriginalMapBattleBridgeFailureCode.WrongState,
            traversal,
            declined.Bridge);

        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
        Assert.Equal(traversal.SimulationStep + 1, moved.Snapshot.SimulationStep);
        Assert.Same(declined.Bridge, session.PrivateOriginalMapBattleBridge);

        PrivateOriginalMapGameSessionStarted restarted = Start();
        Assert.Null(restarted.Session.PrivateOriginalMapBattleBridge);
    }

    [Fact]
    public void DefeatRetriesAndVictoryReturnsTheSameTraversalSnapshotBeforeMovementResumes()
    {
        PrivateOriginalMapGameSessionStarted started = Start();
        GameSession session = started.Session;
        PrivateOriginalMapBattleBridgeSnapshot ready = Bind(started);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;

        PrivateOriginalMapBattleBridgeRequested requested = Assert.IsType<
            PrivateOriginalMapBattleBridgeRequested>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new RequestPrivateOriginalMapBattleBridgeCommand(
                    ready.Definition.Bridge,
                    before.SimulationStep)));
        PrivateOriginalMapBattleBridgeAdmitted admitted = Assert.IsType<
            PrivateOriginalMapBattleBridgeAdmitted>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new AcknowledgePublicSyntheticBattleEntryCommand(
                    requested.Bridge.Definition.Request,
                    requested.Bridge.Definition.Rules.Battle,
                    requested.Cue.Sequence)));
        Assert.Equal(TacticalBattlePhase.MoveSelection, admitted.Bridge.BattleState?.Phase);

        PrivateOriginalMapBattleBridgeSelectionConfirmed firstExchange = ApplyAttack(
            session,
            move: [TacticalDirection.East],
            target: [TacticalDirection.East]);
        Assert.Equal(TacticalSelectionOutcome.AttackConfirmed, firstExchange.Outcome);
        Assert.Equal(TacticalEnemyResponseKind.Attacked, firstExchange.EnemyResponse!.Kind);
        Assert.Same(before, firstExchange.Snapshot);
        Assert.Same(before.WorkingLayout, firstExchange.Snapshot.WorkingLayout);

        PrivateOriginalMapBattleBridgeSelectionConfirmed defeated = ApplyAttack(
            session,
            move: [],
            target: [TacticalDirection.East]);
        Assert.Equal(TacticalSelectionOutcome.BattleDefeated, defeated.Outcome);
        Assert.Equal(TacticalEnemyResponseKind.ActorDefeated, defeated.EnemyResponse!.Kind);
        Assert.Null(defeated.Completion);
        Assert.Equal(TacticalBattleOutcome.Defeat, defeated.Bridge.BattleState!.Outcome);
        Assert.Equal(PrivateOriginalMapBattleBridgeStatus.Completed, defeated.Bridge.Status);
        Assert.Same(before, session.PrivateOriginalMapSnapshot);
        Assert.Same(before.WorkingLayout, session.PrivateOriginalMapSnapshot.WorkingLayout);
        Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.Throws<InvalidOperationException>(() =>
            session.ApplyPrivateOriginalMapLayoutMutation(MutationCommand(before)));

        AssertRejected(
            session,
            new AcknowledgePublicSyntheticBattleCompletionCommand(
                defeated.Bridge.Definition.Rules.Battle,
                defeated.Bridge.LastCueSequence + 1),
            PrivateOriginalMapBattleBridgeFailureCode.AcknowledgementMismatch,
            before,
            defeated.Bridge);
        PrivateOriginalMapBattleBridgeRestarted retry = Assert.IsType<
            PrivateOriginalMapBattleBridgeRestarted>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new AcknowledgePublicSyntheticBattleCompletionCommand(
                    defeated.Bridge.Definition.Rules.Battle,
                    defeated.Bridge.LastCueSequence)));
        Assert.Equal(PrivateOriginalMapBattleBridgeStatus.Active, retry.Bridge.Status);
        Assert.Equal(TacticalBattleOutcome.InProgress, retry.Bridge.BattleState!.Outcome);
        Assert.Equal(2, retry.Bridge.BattleState.ActorHitPoints);
        Assert.Equal(3, retry.Bridge.BattleState.EnemyHitPoints);
        Assert.Equal("battle-restarted", retry.Cue.Cue.Value);
        Assert.Same(before, retry.Snapshot);
        Assert.Same(before, session.PrivateOriginalMapSnapshot);
        Assert.Same(before.WorkingLayout, retry.Snapshot.WorkingLayout);

        AssertRejected(
            session,
            new AcknowledgePublicSyntheticBattleCompletionCommand(
                defeated.Bridge.Definition.Rules.Battle,
                defeated.Bridge.LastCueSequence),
            PrivateOriginalMapBattleBridgeFailureCode.BattleNotCompleted,
            before,
            retry.Bridge);

        PrivateOriginalMapBattleBridgeSelectionConfirmed firstRangedAttack = ApplyAttack(
            session,
            move: [],
            target: [TacticalDirection.East, TacticalDirection.East]);
        Assert.Equal(TacticalEnemyResponseKind.Moved, firstRangedAttack.EnemyResponse!.Kind);
        Assert.Equal(new TacticalPosition(1, 1),
            firstRangedAttack.Bridge.BattleState!.EnemyPosition);
        PrivateOriginalMapBattleBridgeSelectionConfirmed secondRangedAttack = ApplyAttack(
            session,
            move: [TacticalDirection.North],
            target: [TacticalDirection.East, TacticalDirection.South]);
        Assert.Equal(TacticalEnemyResponseKind.Moved, secondRangedAttack.EnemyResponse!.Kind);
        Assert.Equal(new TacticalPosition(1, 0),
            secondRangedAttack.Bridge.BattleState!.EnemyPosition);
        PrivateOriginalMapBattleBridgeSelectionConfirmed completed = ApplyAttack(
            session,
            move: [TacticalDirection.South],
            target: [TacticalDirection.East, TacticalDirection.North]);
        Assert.Equal(TacticalSelectionOutcome.BattleCompleted, completed.Outcome);
        Assert.NotNull(completed.Completion);
        Assert.Null(completed.EnemyResponse);
        Assert.Equal(TacticalBattleOutcome.Victory, completed.Bridge.BattleState!.Outcome);
        Assert.Equal(PrivateOriginalMapBattleBridgeStatus.Completed, completed.Bridge.Status);
        Assert.Same(before, session.PrivateOriginalMapSnapshot);

        AssertRejected(
            session,
            new AcknowledgePublicSyntheticBattleCompletionCommand(
                completed.Completion!.Battle,
                completed.Completion.CueSequence + 1),
            PrivateOriginalMapBattleBridgeFailureCode.AcknowledgementMismatch,
            before,
            completed.Bridge);
        PrivateOriginalMapBattleBridgeReturned returned = Assert.IsType<
            PrivateOriginalMapBattleBridgeReturned>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new AcknowledgePublicSyntheticBattleCompletionCommand(
                    completed.Completion.Battle,
                    completed.Completion.CueSequence)));

        Assert.Equal(PrivateOriginalMapBattleBridgeStatus.Returned, returned.Bridge.Status);
        Assert.Same(before, returned.Snapshot);
        Assert.Same(before, session.PrivateOriginalMapSnapshot);
        Assert.Same(before.WorkingLayout, returned.Snapshot.WorkingLayout);
        Assert.Equal(before.PlayerPosition, returned.Snapshot.PlayerPosition);
        Assert.Equal(before.SimulationStep, returned.Snapshot.SimulationStep);
        Assert.Null(returned.Bridge.BattleState);

        AssertRejected(
            session,
            new RequestPrivateOriginalMapBattleBridgeCommand(
                returned.Bridge.Definition.Bridge,
                before.SimulationStep),
            PrivateOriginalMapBattleBridgeFailureCode.WrongState,
            before,
            returned.Bridge);
        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
        Assert.Equal(before.SimulationStep + 1, moved.Snapshot.SimulationStep);

        PrivateOriginalMapGameSessionStarted restarted = Start();
        Assert.Null(restarted.Session.PrivateOriginalMapBattleBridge);
        Assert.Equal(0, restarted.Session.PrivateOriginalMapSnapshot.SimulationStep);
    }

    [Fact]
    public void UnknownCommandsAreTypedAndPathFreeAfterSessionOwnedBinding()
    {
        PrivateOriginalMapGameSessionStarted started = Start();
        PrivateOriginalMapBattleBridgeSnapshot ready = Bind(started);
        PrivateOriginalMapBattleBridgeRejected unsupported = Assert.IsType<
            PrivateOriginalMapBattleBridgeRejected>(
            started.Session.ApplyPrivateOriginalMapBattleBridge(new UnknownCommand()));
        Assert.Equal(
            PrivateOriginalMapBattleBridgeFailureCode.UnsupportedCommand,
            unsupported.Diagnostic.Code);
        Assert.DoesNotContain("\\", unsupported.Diagnostic.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(":/", unsupported.Diagnostic.Message, StringComparison.Ordinal);
        Assert.Same(ready, started.Session.PrivateOriginalMapBattleBridge);
    }

    private static PrivateOriginalMapBattleBridgeSnapshot Bind(
        PrivateOriginalMapGameSessionStarted started) =>
        Assert.IsType<PrivateOriginalMapBattleBridgeBound>(
            started.Session.BindPrivateOriginalMapBattleBridge(Battle())).Bridge;

    private static void AssertMove(GameSession session, TacticalDirection direction)
    {
        PrivateOriginalMapBattleBridgeCursorMoved moved = Assert.IsType<
            PrivateOriginalMapBattleBridgeCursorMoved>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new MovePublicSyntheticBattleCursorCommand(direction)));
        Assert.Equal(TacticalCursorMoveOutcome.Moved, moved.Outcome);
    }

    private static PrivateOriginalMapBattleBridgeSelectionConfirmed AssertSelection(
        GameSession session,
        TacticalSelectionOutcome outcome)
    {
        PrivateOriginalMapBattleBridgeSelectionConfirmed selected = Assert.IsType<
            PrivateOriginalMapBattleBridgeSelectionConfirmed>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new ConfirmPublicSyntheticBattleSelectionCommand()));
        Assert.Equal(outcome, selected.Outcome);
        return selected;
    }

    private static PrivateOriginalMapBattleBridgeSelectionConfirmed ApplyAttack(
        GameSession session,
        IEnumerable<TacticalDirection> move,
        IEnumerable<TacticalDirection> target)
    {
        foreach (TacticalDirection direction in move)
        {
            AssertMove(session, direction);
        }

        AssertSelection(session, TacticalSelectionOutcome.MoveConfirmed);
        foreach (TacticalDirection direction in target)
        {
            AssertMove(session, direction);
        }

        return Assert.IsType<PrivateOriginalMapBattleBridgeSelectionConfirmed>(
            session.ApplyPrivateOriginalMapBattleBridge(
                new ConfirmPublicSyntheticBattleSelectionCommand()));
    }

    private static void AssertRejected(
        GameSession session,
        IGameSessionCommand command,
        PrivateOriginalMapBattleBridgeFailureCode code,
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapBattleBridgeSnapshot bridge)
    {
        PrivateOriginalMapBattleBridgeRejected rejected = Assert.IsType<
            PrivateOriginalMapBattleBridgeRejected>(
            session.ApplyPrivateOriginalMapBattleBridge(command));
        Assert.Equal(code, rejected.Diagnostic.Code);
        Assert.Same(snapshot, rejected.Snapshot);
        Assert.Same(bridge, rejected.Bridge);
        Assert.Same(snapshot, session.PrivateOriginalMapSnapshot);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
    }

    private static PrivateOriginalMapGameSessionStarted Start() =>
        Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(
                new ImportSource(new OriginalMapImportAccepted(
                    ImportDefinition(),
                    ImportReceipt())),
                new OriginalMapImportRequest(
                    OriginalMapRuntimeAdmission.PackageId,
                    ContentProfile.PrivateLocal,
                    OriginalMapRuntimeAdmission.AcceptedContentDigest)));

    private static PublicSyntheticBattleDefinition Battle() =>
        new(
            new PublicSyntheticBattleRequestId("project-authored-battle-request"),
            new TacticalBattleRules(
                new TacticalBattleId("project-authored-tactical-battle"),
                new TacticalBattleGrid(3, 2, Enumerable.Repeat(true, 6)),
                new TacticalCombatantId("project-authored-actor"),
                new TacticalPosition(0, 1),
                new TacticalCombatantId("project-authored-enemy"),
                new TacticalPosition(2, 1),
                actorMoveRange: 1,
                actorAttackRange: 2,
                actorMaxHitPoints: 2,
                actorDamage: 1,
                enemyMoveRange: 1,
                enemyAttackRange: 1,
                enemyMaxHitPoints: 3,
                enemyDamage: 1),
            new MapId("public-source-must-not-leak"),
            new MapPosition(2, 1),
            new MapSetupId("public-source-setup-must-not-leak"),
            new EventTargetId("public-source-zone-must-not-leak"),
            new MapId("public-return-must-not-leak"),
            new MapPosition(1, 1),
            new MapSetupId("public-return-setup-must-not-leak"),
            SemanticFacing.East,
            new MapEventEffectId("public-effect-must-not-leak"),
            new FlagId("public-flag-must-not-leak"),
            new PresentationCueId("battle-requested"),
            new PresentationCueId("battle-admitted"),
            new PresentationCueId("battle-moved"),
            new PresentationCueId("battle-attacked"),
            new PresentationCueId("battle-enemy-response"),
            new PresentationCueId("battle-completed"),
            new PresentationCueId("battle-defeated"),
            new PresentationCueId("battle-restarted"),
            new PresentationCueId("battle-returned"));

    private static OriginalMapImportDefinition ImportDefinition()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        words[Index(
            OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
            OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY)] |=
            OriginalMapTraversal.CollisionMask;
        return new OriginalMapImportDefinition(
            map,
            new WorkingMapLayout(words),
            BlockCatalog(),
            AreaCatalog(),
            EntityPopulation(map),
            Selection(),
            new OriginalMapControlledAdmission(
                map,
                new MapPosition(56, 3),
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            StepCopy(map),
            ["original-battle-route-unknown"]);
    }

    private static OriginalMapImportReceipt ImportReceipt() =>
        new(
            OriginalMapRuntimeAdmission.PackageId,
            OriginalMapRuntimeAdmission.SchemaVersion,
            OriginalMapRuntimeAdmission.AcceptedContentDigest,
            OriginalMapRuntimeAdmission.AcceptedDecodedLayoutDigest,
            OriginalMapRuntimeAdmission.AcceptedCollisionProjectionDigest,
            ContentProfile.PrivateLocal,
            new OriginalMapImportProvenance(
                OriginalMapRuntimeAdmission.PackageId,
                OriginalMapRuntimeAdmission.AcceptedRomSha256,
                OriginalMapRuntimeAdmission.AcceptedUpstreamRepository,
                OriginalMapRuntimeAdmission.AcceptedUpstreamCommit),
            OriginalMapRuntimeAdmission.RequiredEvidenceOwners,
            OriginalMapRuntimeAdmission.RequiredCapabilities);

    private static OriginalMapVisualResourceSelection Selection() =>
        new(new MapId("map3"), paletteIndex: 0, [0, 37, 43, 53, 66]);

    private static OriginalMapEntityPopulation EntityPopulation(MapId map) =>
        new(
            map,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            Enumerable.Range(0, OriginalMapRuntimeAdmission.AcceptedEntityRecordCount)
                .Select(index => new OriginalMapEntityDefinition(
                    new OriginalMapEntityRecordIdentity(
                        OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                        index + 1),
                    rawX: checked((byte)index),
                    rawY: 0,
                    opaqueFacing: 3,
                    mapSprite: checked((byte)(index + 1)),
                    index >= OriginalMapRuntimeAdmission.AcceptedFixedEntityRecordCount
                        ? [0xFF, checked((byte)index), 0, 1]
                        : [0, 0, 0, 0])),
            OriginalMapRuntimeAdmission.AcceptedEntityProjectionDigest);

    private static OriginalMapBlockCatalog BlockCatalog() =>
        new(
            Enumerable.Range(0, OriginalMapRuntimeAdmission.AcceptedBlockCount)
                .Select(index => new OriginalMapBlockDefinition(
                    new OriginalMapBlockRecordIdentity(
                        OriginalMapRuntimeAdmission.AcceptedBlocksetResourceId,
                        index),
                    new ushort[OriginalMapBlockDefinition.OpaqueWordCount])),
            OriginalMapRuntimeAdmission.AcceptedBlocksetProjectionDigest);

    private static OriginalMapAreaCatalog AreaCatalog() =>
        new(new[]
        {
            Area(1, new OriginalMapTraversalArea(0, 0, 50, 31), 32),
            Area(2, new OriginalMapTraversalArea(51, 0, 61, 9), 0),
            Area(3, new OriginalMapTraversalArea(51, 10, 61, 19), 0),
        });

    private static OriginalMapAreaDefinition Area(
        int ordinal,
        OriginalMapTraversalArea area,
        ushort layerY) =>
        new(
            new OriginalMapAreaRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedAreaResourceId,
                ordinal),
            area,
            new OriginalMapAreaWordPair(0, layerY),
            new OriginalMapAreaWordPair(0, 0),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaBytePair(0, 0),
            new OriginalMapAreaBytePair(0, 0),
            mainLayerType: 0,
            defaultMusic: 8);

    private static OriginalMapStepCopyDefinition StepCopy(MapId map) =>
        new(
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                OriginalMapRuntimeAdmission.ControlledStepCopyRecordOrdinal),
            new MapPosition(41, 13),
            new WorkingMapBlockCopy(62, 0, 41, 13, 1, 1));

    private static ApplyPrivateOriginalMapLayoutMutationCommand MutationCommand(
        PrivateOriginalMapSessionSnapshot snapshot) =>
        new(snapshot.Definition.ControlledStepCopy!.Identity, snapshot.SimulationStep);

    private static int Index(int x, int y) =>
        (y * WorkingMapLayout.ColumnCount) + x;

    private sealed record ImportSource(OriginalMapImportResult Result) : IOriginalMapImportSource
    {
        public OriginalMapImportResult Admit(OriginalMapImportRequest request) => Result;
    }

    private sealed record UnknownCommand : IGameSessionCommand;
}

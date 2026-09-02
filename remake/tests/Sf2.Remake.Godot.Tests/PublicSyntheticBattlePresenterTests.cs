using System.Reflection;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PublicSyntheticBattlePresenterTests
{
    [Fact]
    public void LegacyMap3SmokeReceiptRemainsByteStableBeforeBattleCommands()
    {
        (GameSession session, ScenarioAdmissionReceipt receipt) = StartSession();
        GameSessionSnapshot before = session.Snapshot;
        GameSessionCommandApplied applied = Assert.IsType<GameSessionCommandApplied>(
            session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));

        string json = PublicSyntheticMap3SmokeDriver.CreateLegacyReceipt(
            before,
            applied,
            receipt);

        Assert.Equal(
            "{\"status\":\"Pass\",\"profile\":\"public-synthetic\"," +
            "\"scenarioId\":\"map3-public-synthetic-smoke\"," +
            "\"exactControlledAdmission\":false," +
            "\"capability\":\"map3-synthetic-exploration-smoke\"," +
            "\"evidenceOwner\":\"sf2-map3-admitted-start-runtime-v1\"," +
            "\"mapId\":\"map3\",\"opaqueStartFacing\":3," +
            "\"before\":{\"x\":56,\"y\":3}," +
            "\"after\":{\"x\":57,\"y\":3},\"outcome\":\"Moved\"," +
            "\"simulationStep\":1," +
            "\"banner\":\"PUBLIC SYNTHETIC \\u2014 NOT ORIGINAL FIDELITY\"}",
            json);
    }

    [Fact]
    public void TypedBattleSnapshotProjectsTheExactProjectAuthoredGrid()
    {
        GameSession session = StartActiveBattle();

        PublicSyntheticBattlePresentationProjection projection =
            PublicSyntheticBattlePresentationProjection.Create(
                session.Snapshot,
                "Project-authored tactical battle admitted");

        Assert.True(projection.Visible);
        Assert.Contains("PROJECT-AUTHORED PUBLIC-SYNTHETIC", projection.Title);
        Assert.Contains("public-synthetic-map3-tactical-battle", projection.Status);
        Assert.Contains("MoveSelection", projection.Status);
        Assert.Equal(
            "MOVE · I/J/K/L cursor · Space confirm destination",
            projection.Instruction);
        Assert.Equal("A actor · E enemy · ▣ cursor · · open cell", projection.Legend);
        Assert.Equal(2, projection.ActorHitPoints);
        Assert.Equal(2, projection.ActorMaxHitPoints);
        Assert.Equal(3, projection.EnemyHitPoints);
        Assert.Equal(3, projection.EnemyMaxHitPoints);
        Assert.Contains("Actor HP 2", projection.Status);
        Assert.Contains("Enemy HP 3", projection.Status);
        Assert.Contains("Outcome InProgress", projection.Status);
        Assert.Equal(6, projection.Cells.Count);
        PublicSyntheticBattleCellProjection actor = Assert.Single(
            projection.Cells,
            cell => cell.HasActor);
        PublicSyntheticBattleCellProjection enemy = Assert.Single(
            projection.Cells,
            cell => cell.HasEnemy);
        PublicSyntheticBattleCellProjection cursor = Assert.Single(
            projection.Cells,
            cell => cell.HasCursor);
        Assert.Equal(new TacticalPosition(0, 1), actor.Position);
        Assert.Equal(new TacticalPosition(2, 1), enemy.Position);
        Assert.Same(actor, cursor);
    }

    [Fact]
    public void BattlePanelFitsTheFixedCanvasAndNamesEveryPhaseAction()
    {
        Assert.Equal(
            new global::Godot.Vector2(
                PrivateLocalPresentationAssetCatalog.TacticalCursorLogicalWidth,
                PrivateLocalPresentationAssetCatalog.TacticalCursorLogicalHeight),
            PublicSyntheticBattlePresenter.TacticalCellSize);
        Assert.True(
            PublicSyntheticBattlePresenter.PanelBounds.End.X <=
            PublicSyntheticBattlePresenter.CanvasSize.X);
        Assert.True(
            PublicSyntheticBattlePresenter.PanelBounds.End.Y <=
            PublicSyntheticBattlePresenter.CanvasSize.Y);
        Assert.Equal(
            new global::Godot.Vector2(8, 12),
            PublicSyntheticBattlePresenter.TacticalCellLabelOffset);
        Assert.Equal(
            new global::Godot.Vector2(42, 32),
            PublicSyntheticBattlePresenter.TacticalCellLabelSize);
        Assert.True(
            PublicSyntheticBattlePresenter.TacticalCellLabelOffset.X >= 0 &&
            PublicSyntheticBattlePresenter.TacticalCellLabelOffset.Y >= 0 &&
            PublicSyntheticBattlePresenter.TacticalCellLabelOffset.X +
                PublicSyntheticBattlePresenter.TacticalCellLabelSize.X <=
                PublicSyntheticBattlePresenter.TacticalCellSize.X &&
            PublicSyntheticBattlePresenter.TacticalCellLabelOffset.Y +
                PublicSyntheticBattlePresenter.TacticalCellLabelSize.Y <=
                PublicSyntheticBattlePresenter.TacticalCellSize.Y);

        GameSession session = StartActiveBattle();
        PublicSyntheticBattlePresentationProjection move =
            PublicSyntheticBattlePresentationProjection.Create(session.Snapshot, "ready");
        Assert.StartsWith("MOVE", move.Instruction);

        Assert.IsType<GameSessionPublicSyntheticBattleSelectionConfirmed>(session.Apply(
            new ConfirmPublicSyntheticBattleSelectionCommand()));
        PublicSyntheticBattlePresentationProjection target =
            PublicSyntheticBattlePresentationProjection.Create(session.Snapshot, "targeting");
        Assert.StartsWith("TARGET", target.Instruction);
        Assert.Contains("Backspace cancel", target.Instruction);
    }

    [Fact]
    public void EnemyResponseDefeatAndRetryProjectOnlyTypedState()
    {
        GameSession session = StartActiveBattle();
        GameSessionPublicSyntheticBattleSelectionConfirmed exchange = ApplyBattleAttack(
            session,
            move: [TacticalDirection.East],
            target: [TacticalDirection.East]);
        PublicSyntheticBattlePresentationProjection exchangeProjection =
            PublicSyntheticBattlePresentationProjection.Create(
                exchange.Snapshot,
                "exchange",
                exchange);
        Assert.Contains("Actor HP 1", exchangeProjection.Status);
        Assert.Contains("Enemy HP 2", exchangeProjection.Status);
        Assert.Contains("Enemy Attacked", exchangeProjection.CueStatus);

        GameSessionPublicSyntheticBattleSelectionConfirmed defeated = ApplyBattleAttack(
            session,
            move: [],
            target: [TacticalDirection.East]);
        PublicSyntheticBattlePresentationProjection defeatedProjection =
            PublicSyntheticBattlePresentationProjection.Create(
                defeated.Snapshot,
                "defeated",
                defeated);
        Assert.Contains("Outcome Defeat", defeatedProjection.Status);
        Assert.Equal("DEFEAT · M retry battle", defeatedProjection.Instruction);
        Assert.Contains("Actor HP 0", defeatedProjection.Status);
        Assert.Contains("public-synthetic-map3-battle-defeated", defeatedProjection.CueStatus);
        Assert.Contains("Enemy ActorDefeated", defeatedProjection.CueStatus);

        GameSessionPublicSyntheticBattleRestarted restarted = Assert.IsType<
            GameSessionPublicSyntheticBattleRestarted>(session.Apply(
            new AcknowledgePublicSyntheticBattleCompletionCommand(
                defeated.Snapshot.PublicSyntheticBattle!.Definition.Rules.Battle,
                defeated.Snapshot.LastCueSequence)));
        PublicSyntheticBattlePresentationProjection restartedProjection =
            PublicSyntheticBattlePresentationProjection.Create(
                restarted.Snapshot,
                "restarted",
                restarted);
        Assert.Contains("Actor HP 2", restartedProjection.Status);
        Assert.Contains("Enemy HP 3", restartedProjection.Status);
        Assert.Contains("Outcome InProgress", restartedProjection.Status);
        Assert.StartsWith("MOVE", restartedProjection.Instruction);
        Assert.Contains("public-synthetic-map3-battle-restarted", restartedProjection.CueStatus);
    }

    [Fact]
    public void StrategicVictoryAndReturnProjectOnlyTypedResultWithoutCachedBattleState()
    {
        GameSession session = StartActiveBattle();
        ApplyBattleAttack(
            session,
            move: [],
            target: [TacticalDirection.East, TacticalDirection.East]);
        ApplyBattleAttack(
            session,
            move: [TacticalDirection.North],
            target: [TacticalDirection.East, TacticalDirection.South]);
        GameSessionPublicSyntheticBattleSelectionConfirmed completed = ApplyBattleAttack(
            session,
            move: [TacticalDirection.South],
            target: [TacticalDirection.East, TacticalDirection.North]);

        PublicSyntheticBattlePresentationProjection completedProjection =
            PublicSyntheticBattlePresentationProjection.Create(
                completed.Snapshot,
                "completed",
                completed);
        Assert.Contains("Completed", completedProjection.Status);
        Assert.Contains("Outcome Victory", completedProjection.Status);
        Assert.Equal("VICTORY · M return to exploration", completedProjection.Instruction);
        Assert.Contains("public-synthetic-map3-battle-attack-completed", completedProjection.CueStatus);
        Assert.Contains("public-synthetic-map3-battle-completed", completedProjection.CueStatus);

        GameSessionPublicSyntheticBattleReturned returned =
            Assert.IsType<GameSessionPublicSyntheticBattleReturned>(session.Apply(
                new AcknowledgePublicSyntheticBattleCompletionCommand(
                    completed.Completion!.Battle,
                    completed.Completion.CueSequence)));
        PublicSyntheticBattlePresentationProjection returnedProjection =
            PublicSyntheticBattlePresentationProjection.Create(
                returned.Snapshot,
                "returned",
                returned);

        Assert.Null(returned.Snapshot.PublicSyntheticBattle);
        Assert.True(returnedProjection.Visible);
        Assert.Empty(returnedProjection.Cells);
        Assert.Equal("RETURNED · exploration resumed", returnedProjection.Instruction);
        Assert.Contains("completed; returned", returnedProjection.Status);
        Assert.Contains(
            "public-synthetic-map3-battle-completion-world-effect",
            returnedProjection.Status);
        Assert.Contains(
            "public-synthetic-map3-battle-completed",
            returnedProjection.Status);
        Assert.Contains("public-synthetic-map3-battle-returned", returnedProjection.CueStatus);

        PublicSyntheticBattlePresentationProjection laterProjection =
            PublicSyntheticBattlePresentationProjection.Create(
                returned.Snapshot,
                "later projection");
        Assert.False(laterProjection.Visible);
        Assert.Empty(laterProjection.Cells);
    }

    [Fact]
    public void PrivateBridgeProjectsTheSharedBattleWithoutPublicWorldState()
    {
        PrivateOriginalMapBattleBridgeSnapshot bridge = ActivePrivateBridge();

        PublicSyntheticBattlePresentationProjection projection =
            PublicSyntheticBattlePresentationProjection.Create(
                bridge,
                "Explicit private-local bridge admitted");

        Assert.True(projection.Visible);
        Assert.Contains("PROJECT-AUTHORED PUBLIC-SYNTHETIC", projection.Title);
        Assert.Contains("project-authored-tactical-battle", projection.Status);
        Assert.Contains("MoveSelection", projection.Status);
        Assert.Equal(6, projection.Cells.Count);
        Assert.Single(projection.Cells, cell => cell.HasActor);
        Assert.Single(projection.Cells, cell => cell.HasEnemy);
        Assert.Single(projection.Cells, cell => cell.HasCursor);
        Assert.DoesNotContain("effect", projection.Status, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("flag", projection.Status, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("return", projection.Status, StringComparison.OrdinalIgnoreCase);
    }

    private static GameSession StartActiveBattle()
    {
        (GameSession session, _) = StartSession();

        Assert.IsType<GameSessionCommandApplied>(session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionEventRequested eventRequested = Assert.IsType<GameSessionEventRequested>(
            session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.IsType<GameSessionEventEffectApplied>(session.Apply(
            new AcknowledgeMapEventRequestCommand(
                eventRequested.Request.Request,
                eventRequested.Cue.Sequence,
                eventRequested.Request.ExpectedEffect)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        Assert.IsType<GameSessionCommandApplied>(session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionLocalTransitionRequested local =
            Assert.IsType<GameSessionLocalTransitionRequested>(session.Apply(
                new RequestSelectedLocalTransitionCommand()));
        Assert.IsType<GameSessionLocalTransitionApplied>(session.Apply(
            new AcknowledgeMapLocalTransitionCommand(
                local.Transition.Request,
                local.Transition.CueSequence,
                local.Transition.Transition)));
        Assert.IsType<GameSessionCommandApplied>(session.Apply(
            new MoveExplorationCommand(ExplorationDirection.West)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionOutboundTransitionRequested outbound =
            Assert.IsType<GameSessionOutboundTransitionRequested>(session.Apply(
                new RequestSelectedOutboundTransitionCommand()));
        Assert.IsType<GameSessionOutboundTransitionApplied>(session.Apply(
            new AcknowledgeMapOutboundTransitionCommand(
                outbound.Transition.Request,
                outbound.Cue.Sequence,
                outbound.Transition.Transition)));
        Assert.IsType<GameSessionCommandApplied>(session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.IsType<GameSessionContextSelected>(session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionPublicSyntheticBattleRequested requested =
            Assert.IsType<GameSessionPublicSyntheticBattleRequested>(session.Apply(
                new RequestSelectedPublicSyntheticBattleCommand()));
        Assert.IsType<GameSessionPublicSyntheticBattleAdmitted>(session.Apply(
            new AcknowledgePublicSyntheticBattleEntryCommand(
                requested.Battle.Definition.Request,
                requested.Battle.Definition.Rules.Battle,
                requested.Cue.Sequence)));
        return session;
    }

    private static GameSessionPublicSyntheticBattleSelectionConfirmed ApplyBattleAttack(
        GameSession session,
        IEnumerable<TacticalDirection> move,
        IEnumerable<TacticalDirection> target)
    {
        foreach (TacticalDirection direction in move)
        {
            GameSessionPublicSyntheticBattleCursorMoved moved = Assert.IsType<
                GameSessionPublicSyntheticBattleCursorMoved>(session.Apply(
                new MovePublicSyntheticBattleCursorCommand(direction)));
            Assert.Equal(TacticalCursorMoveOutcome.Moved, moved.Outcome);
        }

        Assert.Equal(
            TacticalSelectionOutcome.MoveConfirmed,
            Assert.IsType<GameSessionPublicSyntheticBattleSelectionConfirmed>(session.Apply(
                new ConfirmPublicSyntheticBattleSelectionCommand())).Outcome);
        foreach (TacticalDirection direction in target)
        {
            GameSessionPublicSyntheticBattleCursorMoved moved = Assert.IsType<
                GameSessionPublicSyntheticBattleCursorMoved>(session.Apply(
                new MovePublicSyntheticBattleCursorCommand(direction)));
            Assert.Equal(TacticalCursorMoveOutcome.Moved, moved.Outcome);
        }

        return Assert.IsType<GameSessionPublicSyntheticBattleSelectionConfirmed>(session.Apply(
            new ConfirmPublicSyntheticBattleSelectionCommand()));
    }

    private static PrivateOriginalMapBattleBridgeSnapshot ActivePrivateBridge()
    {
        PublicSyntheticBattleDefinition battle = new(
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
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        MapPosition position = new(56, 3);
        PrivateOriginalMapBattleBridgeDefinition definition =
            Assert.IsType<PrivateOriginalMapBattleBridgeDefinition>(Activator.CreateInstance(
                typeof(PrivateOriginalMapBattleBridgeDefinition),
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                [map, position, battle],
                culture: null));
        PrivateOriginalMapBattleBridgeSnapshot pending =
            Assert.IsType<PrivateOriginalMapBattleBridgeSnapshot>(
                typeof(PrivateOriginalMapBattleBridgeSnapshot)
                    .GetMethod("Pending", BindingFlags.Static | BindingFlags.NonPublic)!
                    .Invoke(null, [definition, PrivateSnapshot(map, position)]));
        object pendingLifecycle = typeof(PrivateOriginalMapBattleBridgeSnapshot)
            .GetProperty("Lifecycle", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(pending)!;
        object activeLifecycle = pendingLifecycle.GetType()
            .GetMethod("Admit", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(pendingLifecycle, [2L, 2L])!;
        return Assert.IsType<PrivateOriginalMapBattleBridgeSnapshot>(
            typeof(PrivateOriginalMapBattleBridgeSnapshot)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(pending, [activeLifecycle, 2L]));
    }

    private static PrivateOriginalMapSessionSnapshot PrivateSnapshot(
        MapId map,
        MapPosition position)
    {
        WorkingMapLayout layout = new(new ushort[WorkingMapLayout.WordCount]);
        OriginalMapImportDefinition definition = new(
            map,
            layout,
            new OriginalMapBlockCatalog(
            [
                new OriginalMapBlockDefinition(
                    new OriginalMapBlockRecordIdentity("project-authored-blocks", 0),
                    new ushort[OriginalMapBlockDefinition.OpaqueWordCount]),
            ]),
            new OriginalMapAreaCatalog(
            [
                new OriginalMapAreaDefinition(
                    new OriginalMapAreaRecordIdentity("project-authored-areas", 1),
                    new OriginalMapTraversalArea(0, 0, 63, 63),
                    new OriginalMapAreaWordPair(0, 0),
                    new OriginalMapAreaWordPair(0, 0),
                    new OriginalMapAreaWordPair(256, 256),
                    new OriginalMapAreaWordPair(256, 256),
                    new OriginalMapAreaBytePair(0, 0),
                    new OriginalMapAreaBytePair(0, 0),
                    mainLayerType: 0,
                    defaultMusic: 0),
            ]),
            new OriginalMapVisualResourceSelection(map, 0, [0, 37, 43, 53, 66]),
            new OriginalMapControlledAdmission(
                map,
                position,
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            ["original-battle-route-unknown"]);
        OriginalMapImportReceipt receipt = new(
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
        return new PrivateOriginalMapSessionSnapshot(
            definition,
            receipt,
            layout,
            simulationStep: 0,
            position,
            lastTraversal: null,
            controlledStepCopyApplied: false,
            lastLayoutMutation: null);
    }

    private static (GameSession Session, ScenarioAdmissionReceipt Receipt) StartSession()
    {
        string contentPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "game",
            "content",
            "public-synthetic-map3-smoke-v1.json"));
        PublicSyntheticMap3PackageReader source =
            PublicSyntheticMap3PackageReader.FromDocumentBytes(File.ReadAllBytes(contentPath));
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(GameSession.Start(
            source,
            new MapScenarioRequest(
                PublicSyntheticMap3PackageReader.PackageId,
                ContentProfile.PublicSynthetic)));
        return (started.Session, started.Receipt);
    }
}

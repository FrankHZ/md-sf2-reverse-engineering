using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal static class PrivateMap3SmokeDriver
{
    internal static void Run(
        SceneTree sceneTree,
        GameSession session,
        PrivateMap3Presenter presenter,
        long smokeStarted) =>
        Run(sceneTree, session, presenter, battlePresenter: null, smokeStarted);

    internal static void Run(
        SceneTree sceneTree,
        GameSession session,
        PrivateMap3Presenter presenter,
        PublicSyntheticBattlePresenter? battlePresenter,
        long smokeStarted)
    {
        if (session.PrivateOriginalMapBattleBridge is not null &&
            !RunBattleBridge(sceneTree, session, presenter, battlePresenter))
        {
            return;
        }

        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapMoveApplied? moved = null;
        ExplorationDirection movedDirection = ExplorationDirection.East;
        PrivateMap3CameraProjection? cameraAtMovementStart = null;
        PrivateMap3CameraProjection? cameraAtMovementEnd = null;
        bool observedSubTileCameraOrigin = false;
        foreach (ExplorationDirection direction in new[]
        {
            ExplorationDirection.East,
            ExplorationDirection.South,
            ExplorationDirection.West,
            ExplorationDirection.North,
        })
        {
            PrivateOriginalMapPlayerLocomotionStarted started =
                session.BeginPrivateOriginalMapPlayerLocomotion(
                    new MoveExplorationCommand(direction));
            PrivateOriginalMapMoveApplied applied = started.Move;
            presenter.Project(
                applied.Snapshot,
                applied.Traversal.Outcome.ToString(),
                started.Animation);
            if (applied.Traversal.Outcome == OriginalMapTraversalOutcome.Moved)
            {
                if (presenter.ExpectsBaseProjection)
                {
                    if (!HasCoherentCameraProjection(
                            presenter.BaseProjection,
                            applied.Snapshot,
                            started.Animation))
                    {
                        Fail(
                            sceneTree,
                            presenter,
                            "PrivateLocal camera did not retain the source focus when movement began.");
                        return;
                    }

                    cameraAtMovementStart = presenter.BaseProjection!.Camera;
                }

                int advances = 0;
                PrivateOriginalMapPlayerLocomotionSnapshot animation = started.Animation;
                while (animation.IsMoving)
                {
                    animation = session.AdvancePrivateOriginalMapPlayerLocomotion();
                    presenter.Project(applied.Snapshot, "Movement smoke tick", animation);
                    if (presenter.ExpectsBaseProjection)
                    {
                        if (!HasCoherentCameraProjection(
                                presenter.BaseProjection,
                                applied.Snapshot,
                                animation))
                        {
                            Fail(
                                sceneTree,
                                presenter,
                                "PrivateLocal camera drifted from the Application-owned locomotion focus.");
                            return;
                        }

                        cameraAtMovementEnd = presenter.BaseProjection!.Camera;
                        observedSubTileCameraOrigin |=
                            cameraAtMovementEnd!.OriginPixelOffsetX != 0 ||
                            cameraAtMovementEnd.OriginPixelOffsetY != 0;
                    }

                    advances++;
                }

                if (advances !=
                        PrivateOriginalMapPlayerLocomotionSnapshot.SuccessfulMovementTickCount - 1 ||
                    animation.Tick !=
                        PrivateOriginalMapPlayerLocomotionSnapshot.SuccessfulMovementTickCount ||
                    animation.DestinationPosition != applied.Snapshot.PlayerPosition ||
                    animation.StoredCounter is < 0 or > 30)
                {
                    Fail(
                        sceneTree,
                        presenter,
                        "PrivateLocal player locomotion did not settle through the Application-owned tick sequence.");
                    return;
                }

                moved = applied;
                movedDirection = direction;
                break;
            }
        }

        if (moved is null)
        {
            Fail(
                sceneTree,
                presenter,
                "No bounded semantic movement was admitted from the controlled start.");
            return;
        }

        PrivateOriginalMapTraversalViewProjection? projection = presenter.Projection;
        if (projection is null)
        {
            Fail(
                sceneTree,
                presenter,
                "PrivateLocal traversal diagnostic view was not projected.");
            return;
        }

        if (presenter.ExpectsBaseProjection)
        {
            PrivateOriginalMapBaseViewProjection? baseProjection =
                presenter.BaseProjection;
            if (baseProjection is null ||
                baseProjection.Map != moved.Snapshot.Map ||
                baseProjection.PlayerColumn !=
                    moved.Snapshot.PlayerPosition.X - baseProjection.OriginX ||
                baseProjection.PlayerRow !=
                    moved.Snapshot.PlayerPosition.Y - baseProjection.OriginY)
            {
                Fail(
                    sceneTree,
                    presenter,
                    "PrivateLocal project-authored base view was not projected from the current snapshot.");
                return;
            }

            if (cameraAtMovementStart is null || cameraAtMovementEnd is null ||
                (cameraAtMovementStart.TopLeftPixelX != cameraAtMovementEnd.TopLeftPixelX ||
                    cameraAtMovementStart.TopLeftPixelY != cameraAtMovementEnd.TopLeftPixelY) &&
                !observedSubTileCameraOrigin)
            {
                Fail(
                    sceneTree,
                    presenter,
                    "PrivateLocal camera did not exercise its bounded sub-tile follow projection.");
                return;
            }
        }

        object receipt = new
        {
            status = "Pass",
            profile = "private-local",
            packageId = OriginalMapRuntimeAdmission.PackageId,
            capability = OriginalMapRuntimeAdmission.TraversalCapability,
            mapId = moved.Snapshot.Map.Value,
            before = new
            {
                x = before.PlayerPosition.X,
                y = before.PlayerPosition.Y,
            },
            after = new
            {
                x = moved.Snapshot.PlayerPosition.X,
                y = moved.Snapshot.PlayerPosition.Y,
            },
            direction = movedDirection.ToString(),
            outcome = moved.Traversal.Outcome.ToString(),
            simulationStep = moved.Snapshot.SimulationStep,
            banner = Map3Root.PrivateBannerText,
        };
        GD.Print(Map3Root.PrivateSmokeMarker + JsonSerializer.Serialize(receipt));
        object viewReceipt = new
        {
            status = "Pass",
            profile = "private-local",
            capability = Map3Root.PrivateViewCapability,
            mapId = projection.Map.Value,
            crop = new
            {
                x = projection.OriginX,
                y = projection.OriginY,
                columns = PrivateOriginalMapTraversalViewProjection.ColumnCount,
                rows = PrivateOriginalMapTraversalViewProjection.RowCount,
            },
            player = new
            {
                column = projection.PlayerColumn,
                row = projection.PlayerRow,
            },
            categories = new
            {
                outsideAcceptedActiveArea = projection.Cells.Count(cell =>
                    cell.Category ==
                        PrivateOriginalMapTraversalCellCategory.OutsideAcceptedActiveArea),
                activeNonBlocked = projection.Cells.Count(cell =>
                    cell.Category ==
                        PrivateOriginalMapTraversalCellCategory.ActiveNonBlocked),
                blockedByAcceptedCollisionClass = projection.Cells.Count(cell =>
                    cell.Category ==
                        PrivateOriginalMapTraversalCellCategory.BlockedByAcceptedCollisionClass),
            },
        };
        GD.Print(Map3Root.PrivateViewSmokeMarker + JsonSerializer.Serialize(viewReceipt));
        if (!RunStepCopyDiagnostic(sceneTree, session, presenter))
        {
            return;
        }

        RunAreaDiagnostic(session);

        if (presenter.UsesLocalBaseAtlas &&
            !RunBaseAtlasDiagnostic(sceneTree, presenter))
        {
            return;
        }

        if (presenter.WorldTreatment == PrivateMap3WorldTreatment.EdgeScale2x &&
            !RunWorldTreatmentDiagnostic(sceneTree, presenter))
        {
            return;
        }

        Map3Root.TracePrivateStage(enabled: true, "quit-scheduled", smokeStarted);
        sceneTree.Quit(0);
    }

    private static bool HasCoherentCameraProjection(
        PrivateOriginalMapBaseViewProjection? projection,
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapPlayerLocomotionSnapshot animation)
    {
        if (projection is null)
        {
            return false;
        }

        PrivateMap3CameraProjection expected =
            PrivateMap3CameraProjection.Create(snapshot, animation);
        Rect2 player = PrivateOriginalMapBaseViewport.PlayerLocomotionRect(
            projection,
            animation);
        return projection.Camera == expected &&
            projection.OriginX == expected.OriginX &&
            projection.OriginY == expected.OriginY &&
            player.Position == new Vector2(expected.PlayerPixelX, expected.PlayerPixelY);
    }

    private static bool RunBaseAtlasDiagnostic(
        SceneTree sceneTree,
        PrivateMap3Presenter presenter)
    {
        PrivateOriginalMapBaseViewProjection? projection = presenter.BaseProjection;
        int? mountedScale = presenter.BaseAtlasScale;
        string? expectedBucketDigest = mountedScale switch
        {
            2 => PrivateLocalPresentationAssetCatalog.Map3BaseAtlas2xDigest,
            4 => PrivateLocalPresentationAssetCatalog.Map3BaseAtlas4xDigest,
            _ => null,
        };
        string? expectedPlayerDigest = mountedScale switch
        {
            2 => PrivateLocalPresentationAssetCatalog.Map3PlayerReference2xDigest,
            4 => PrivateLocalPresentationAssetCatalog.Map3PlayerReference4xDigest,
            _ => null,
        };
        if (projection is null ||
            mountedScale is not int scale ||
            projection.RasterScale != scale ||
            projection.RasterPixelWidth != checked(
                PrivateOriginalMapBaseViewProjection.PixelWidth * scale) ||
            projection.RasterPixelHeight != checked(
                PrivateOriginalMapBaseViewProjection.PixelHeight * scale) ||
            PrivateOriginalMapBaseViewport.LogicalTextureRect != new Rect2(
                Vector2.Zero,
                new Vector2(
                    PrivateOriginalMapBaseViewProjection.PixelWidth,
                    PrivateOriginalMapBaseViewProjection.PixelHeight)) ||
            !presenter.UsesRequiredBaseAtlasSampling ||
            !string.Equals(
                presenter.BaseAtlasAssetId,
                PrivateLocalPresentationAssetCatalog.Map3BaseAtlasAssetId,
                StringComparison.Ordinal) ||
            expectedBucketDigest is null ||
            !string.Equals(
                presenter.BaseAtlasBucketDigest,
                expectedBucketDigest,
                StringComparison.Ordinal) ||
            !presenter.UsesLocalPlayerReference ||
            !string.Equals(
                presenter.PlayerReferenceAssetId,
                PrivateLocalPresentationAssetCatalog.Map3PlayerReferenceAssetId,
                StringComparison.Ordinal) ||
            presenter.PlayerReferenceScale != scale ||
            expectedPlayerDigest is null ||
            !string.Equals(
                presenter.PlayerReferenceBucketDigest,
                expectedPlayerDigest,
                StringComparison.Ordinal))
        {
            Fail(
                sceneTree,
                presenter,
                "PrivateLocal Map 3 base-atlas and player-reference projection was not bound exactly.");
            return false;
        }

        object receipt = new
        {
            status = "Pass",
            profile = "private-local",
            capability = Map3Root.PrivateBaseAtlasCapability,
            assetId = presenter.BaseAtlasAssetId,
            scale = presenter.BaseAtlasScale,
            bucketSha256 = presenter.BaseAtlasBucketDigest,
            mapId = projection.Map.Value,
            crop = new
            {
                x = projection.OriginX,
                y = projection.OriginY,
                columns = PrivateOriginalMapBaseViewProjection.ColumnCount,
                rows = PrivateOriginalMapBaseViewProjection.RowCount,
            },
            banner = Map3Root.PrivateBannerText,
        };
        GD.Print(Map3Root.PrivateBaseAtlasSmokeMarker + JsonSerializer.Serialize(receipt));
        return true;
    }

    private static bool RunWorldTreatmentDiagnostic(
        SceneTree sceneTree,
        PrivateMap3Presenter presenter)
    {
        PrivateOriginalMapBaseViewProjection? projection = presenter.BaseProjection;
        if (projection is null ||
            presenter.WorldTreatment != PrivateMap3WorldTreatment.EdgeScale2x ||
            presenter.BaseAtlasScale is not int scale ||
            projection.RasterScale != scale ||
            projection.RasterPixelWidth != checked(
                PrivateOriginalMapBaseViewProjection.PixelWidth * scale) ||
            projection.RasterPixelHeight != checked(
                PrivateOriginalMapBaseViewProjection.PixelHeight * scale))
        {
            Fail(
                sceneTree,
                presenter,
                "PrivateLocal Map 3 edge-scale2x world treatment was not projected exactly.");
            return false;
        }

        object receipt = new
        {
            status = "Pass",
            profile = "private-local",
            capability = Map3Root.PrivateWorldTreatmentCapability,
            treatment = "edge-scale2x",
            scale,
            mapId = projection.Map.Value,
            crop = new
            {
                x = projection.OriginX,
                y = projection.OriginY,
                columns = PrivateOriginalMapBaseViewProjection.ColumnCount,
                rows = PrivateOriginalMapBaseViewProjection.RowCount,
            },
            banner = Map3Root.PrivateBannerText,
        };
        GD.Print(
            Map3Root.PrivateWorldTreatmentSmokeMarker +
            JsonSerializer.Serialize(receipt));
        return true;
    }

    private static bool RunBattleBridge(
        SceneTree sceneTree,
        GameSession session,
        PrivateMap3Presenter presenter,
        PublicSyntheticBattlePresenter? battlePresenter)
    {
        PrivateOriginalMapBattleBridgeSnapshot ready =
            session.PrivateOriginalMapBattleBridge!;
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        if (battlePresenter is null || !presenter.ExpectsBaseProjection ||
            ready.Status != PrivateOriginalMapBattleBridgeStatus.Ready)
        {
            Fail(
                sceneTree,
                presenter,
                "PrivateLocal battle bridge presentation was not ready.");
            return false;
        }

        PrivateOriginalMapBattleBridgeRequested? requested =
            session.ApplyPrivateOriginalMapBattleBridge(
                new RequestPrivateOriginalMapBattleBridgeCommand(
                    ready.Definition.Bridge,
                    before.SimulationStep)) as PrivateOriginalMapBattleBridgeRequested;
        PrivateOriginalMapBattleBridgeAdmitted? admitted = requested is null
            ? null
            : session.ApplyPrivateOriginalMapBattleBridge(
                new AcknowledgePublicSyntheticBattleEntryCommand(
                    requested.Bridge.Definition.Request,
                    requested.Bridge.Definition.Rules.Battle,
                    requested.Cue.Sequence)) as PrivateOriginalMapBattleBridgeAdmitted;
        if (admitted?.Bridge.BattleState?.Phase != TacticalBattlePhase.MoveSelection)
        {
            Fail(sceneTree, presenter, "PrivateLocal battle bridge was not admitted.");
            return false;
        }

        battlePresenter.Project(
            admitted.Bridge,
            "Project-authored tactical battle admitted",
            admitted);
        if (!MoveBattleCursor(session) ||
            session.ApplyPrivateOriginalMapBattleBridge(
                new ConfirmPublicSyntheticBattleSelectionCommand()) is not
                    PrivateOriginalMapBattleBridgeSelectionConfirmed
                { Outcome: TacticalSelectionOutcome.MoveConfirmed } ||
            !MoveBattleCursor(session) ||
            session.ApplyPrivateOriginalMapBattleBridge(
                new CancelPublicSyntheticBattleSelectionCommand()) is not
                    PrivateOriginalMapBattleBridgeSelectionCancelled
                { Outcome: TacticalCancelOutcome.ReturnedToMoveSelection })
        {
            Fail(sceneTree, presenter, "PrivateLocal tactical bridge cancel path failed.");
            return false;
        }

        PrivateOriginalMapBattleBridgeSelectionConfirmed? exchange = ApplyBattleAttack(
            session,
            [TacticalDirection.East],
            [TacticalDirection.East]);
        PrivateOriginalMapBattleBridgeSelectionConfirmed? defeated = exchange is null
            ? null
            : ApplyBattleAttack(session, [], [TacticalDirection.East]);
        PrivateOriginalMapBattleBridgeRestarted? restarted = defeated is null
            ? null
            : session.ApplyPrivateOriginalMapBattleBridge(
                new AcknowledgePublicSyntheticBattleCompletionCommand(
                    defeated.Bridge.Definition.Rules.Battle,
                    defeated.Bridge.LastCueSequence)) as
                PrivateOriginalMapBattleBridgeRestarted;
        if (exchange?.EnemyResponse?.Kind != TacticalEnemyResponseKind.Attacked ||
            defeated?.Outcome != TacticalSelectionOutcome.BattleDefeated ||
            defeated.EnemyResponse?.Kind != TacticalEnemyResponseKind.ActorDefeated ||
            defeated.Completion is not null ||
            restarted?.Bridge.BattleState?.Outcome != TacticalBattleOutcome.InProgress ||
            !ReferenceEquals(before, restarted.Snapshot) ||
            !ReferenceEquals(before, session.PrivateOriginalMapSnapshot))
        {
            Fail(sceneTree, presenter, "PrivateLocal tactical bridge defeat/retry failed.");
            return false;
        }

        PrivateOriginalMapBattleBridgeSelectionConfirmed? firstRanged = ApplyBattleAttack(
            session,
            [],
            [TacticalDirection.East, TacticalDirection.East]);
        PrivateOriginalMapBattleBridgeSelectionConfirmed? secondRanged = firstRanged is null
            ? null
            : ApplyBattleAttack(
                session,
                [TacticalDirection.North],
                [TacticalDirection.East, TacticalDirection.South]);
        PrivateOriginalMapBattleBridgeSelectionConfirmed? completed = secondRanged is null
            ? null
            : ApplyBattleAttack(
                session,
                [TacticalDirection.South],
                [TacticalDirection.East, TacticalDirection.North]);
        if (firstRanged?.EnemyResponse?.Kind != TacticalEnemyResponseKind.Moved ||
            secondRanged?.EnemyResponse?.Kind != TacticalEnemyResponseKind.Moved ||
            completed?.Outcome != TacticalSelectionOutcome.BattleCompleted ||
            completed.Completion is null ||
            completed.Bridge.BattleState?.Outcome != TacticalBattleOutcome.Victory)
        {
            Fail(sceneTree, presenter, "PrivateLocal tactical bridge did not complete.");
            return false;
        }

        PrivateOriginalMapBattleBridgeReturned? returned =
            session.ApplyPrivateOriginalMapBattleBridge(
                new AcknowledgePublicSyntheticBattleCompletionCommand(
                    completed.Completion.Battle,
                    completed.Completion.CueSequence)) as
                PrivateOriginalMapBattleBridgeReturned;
        if (returned is null ||
            returned.Bridge.Status != PrivateOriginalMapBattleBridgeStatus.Returned ||
            !ReferenceEquals(before, returned.Snapshot) ||
            !ReferenceEquals(before, session.PrivateOriginalMapSnapshot) ||
            returned.Snapshot.SimulationStep != before.SimulationStep ||
            !ReferenceEquals(returned.Snapshot.WorkingLayout, before.WorkingLayout))
        {
            Fail(
                sceneTree,
                presenter,
                "PrivateLocal battle bridge did not restore the same traversal state.");
            return false;
        }

        battlePresenter.Project(
            returned.Bridge,
            "Project-authored battle complete; private Map 3 restored",
            returned);
        presenter.Project(returned.Snapshot, "Battle bridge returned");
        return true;
    }

    private static bool MoveBattleCursor(GameSession session) =>
        session.ApplyPrivateOriginalMapBattleBridge(
            new MovePublicSyntheticBattleCursorCommand(TacticalDirection.East)) is
                PrivateOriginalMapBattleBridgeCursorMoved
        { Outcome: TacticalCursorMoveOutcome.Moved };

    private static PrivateOriginalMapBattleBridgeSelectionConfirmed? ApplyBattleAttack(
        GameSession session,
        IEnumerable<TacticalDirection> move,
        IEnumerable<TacticalDirection> target)
    {
        foreach (TacticalDirection direction in move)
        {
            if (session.ApplyPrivateOriginalMapBattleBridge(
                    new MovePublicSyntheticBattleCursorCommand(direction)) is not
                PrivateOriginalMapBattleBridgeCursorMoved
                    { Outcome: TacticalCursorMoveOutcome.Moved })
            {
                return null;
            }
        }

        if (session.ApplyPrivateOriginalMapBattleBridge(
                new ConfirmPublicSyntheticBattleSelectionCommand()) is not
            PrivateOriginalMapBattleBridgeSelectionConfirmed
                { Outcome: TacticalSelectionOutcome.MoveConfirmed })
        {
            return null;
        }

        foreach (TacticalDirection direction in target)
        {
            if (session.ApplyPrivateOriginalMapBattleBridge(
                    new MovePublicSyntheticBattleCursorCommand(direction)) is not
                PrivateOriginalMapBattleBridgeCursorMoved
                    { Outcome: TacticalCursorMoveOutcome.Moved })
            {
                return null;
            }
        }

        return session.ApplyPrivateOriginalMapBattleBridge(
            new ConfirmPublicSyntheticBattleSelectionCommand()) as
            PrivateOriginalMapBattleBridgeSelectionConfirmed;
    }

    private static bool RunStepCopyDiagnostic(
        SceneTree sceneTree,
        GameSession session,
        PrivateMap3Presenter presenter)
    {
        PrivateOriginalMapSessionSnapshot current = session.PrivateOriginalMapSnapshot;
        OriginalMapStepCopyDefinition? admitted = current.Definition.ControlledStepCopy;
        if (admitted is null)
        {
            Fail(
                sceneTree,
                presenter,
                "The admitted private definition has no controlled step-copy record.");
            return false;
        }

        PrivateOriginalMapLayoutMutationResult result =
            session.ApplyPrivateOriginalMapLayoutMutation(
                new ApplyPrivateOriginalMapLayoutMutationCommand(
                    admitted.Identity,
                    current.SimulationStep));
        if (result is not PrivateOriginalMapLayoutMutationApplied applied)
        {
            PrivateOriginalMapLayoutMutationRejected rejected =
                (PrivateOriginalMapLayoutMutationRejected)result;
            Fail(
                sceneTree,
                presenter,
                $"Controlled step-copy diagnostic rejected ({rejected.Diagnostic.Code}).");
            return false;
        }

        presenter.Project(
            applied.Snapshot,
            "Controlled step-copy diagnostic applied");
        WorkingMapBlockCopy copy = applied.Receipt.Copy;
        object receipt = new
        {
            status = "Pass",
            profile = "private-local",
            capability = OriginalMapRuntimeAdmission.ControlledStepCopyCapability,
            mapId = applied.Receipt.RecordIdentity.Map.Value,
            sourceResourceId = applied.Receipt.RecordIdentity.SourceResourceId,
            recordOrdinal = applied.Receipt.RecordIdentity.OneBasedRecordOrdinal,
            trigger = new
            {
                x = applied.Receipt.Trigger.X,
                y = applied.Receipt.Trigger.Y,
            },
            copy = new
            {
                sourceX = copy.SourceX,
                sourceY = copy.SourceY,
                destinationX = copy.DestinationX,
                destinationY = copy.DestinationY,
                width = copy.Width,
                height = copy.Height,
            },
            beforeCollision = applied.Receipt.BeforeCollision.ToString(),
            afterCollision = applied.Receipt.AfterCollision.ToString(),
            simulationStep = applied.Receipt.SimulationStep,
            disclosure = Map3Root.PrivateBannerText,
        };
        GD.Print(Map3Root.PrivateStepCopySmokeMarker + JsonSerializer.Serialize(receipt));
        return true;
    }

    private static void RunAreaDiagnostic(GameSession session)
    {
        PrivateOriginalMapSessionSnapshot snapshot = session.PrivateOriginalMapSnapshot;
        OriginalMapTraversalAreaSelection selection = snapshot.CurrentArea;
        object receipt = new
        {
            status = "Pass",
            profile = "private-local",
            capability = OriginalMapRuntimeAdmission.CurrentAreaDiagnosticCapability,
            mapId = snapshot.Map.Value,
            oneBasedRecordOrdinal = selection.OneBasedRecordOrdinal,
            simulationStep = snapshot.SimulationStep,
            disclosure = Map3Root.PrivateBannerText,
        };
        GD.Print(Map3Root.PrivateAreaSmokeMarker + JsonSerializer.Serialize(receipt));
    }

    private static void Fail(
        SceneTree sceneTree,
        PrivateMap3Presenter presenter,
        string message)
    {
        GD.PrintErr(message);
        presenter.ProjectStatus(message);
        GD.Print(Map3Root.PrivateSmokeMarker + JsonSerializer.Serialize(
            new { status = "Fail", profile = "private-local", message }));
        Map3Root.TracePrivateStage(
            enabled: true,
            "failure-quit-scheduled",
            System.Diagnostics.Stopwatch.GetTimestamp());
        sceneTree.Quit(1);
    }
}

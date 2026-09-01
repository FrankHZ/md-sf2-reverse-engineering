using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal static class PrivateMap3SmokeDriver
{
    internal static void Run(
        SceneTree sceneTree,
        GameSession session,
        PrivateMap3Presenter presenter,
        long smokeStarted)
    {
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapMoveApplied? moved = null;
        ExplorationDirection movedDirection = ExplorationDirection.East;
        foreach (ExplorationDirection direction in new[]
        {
            ExplorationDirection.East,
            ExplorationDirection.South,
            ExplorationDirection.West,
            ExplorationDirection.North,
        })
        {
            PrivateOriginalMapMoveApplied applied = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(direction));
            presenter.Project(
                applied.Snapshot,
                applied.Traversal.Outcome.ToString());
            if (applied.Traversal.Outcome == OriginalMapTraversalOutcome.Moved)
            {
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

        Map3Root.TracePrivateStage(enabled: true, "quit-scheduled", smokeStarted);
        sceneTree.Quit(0);
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

using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapNaturalStepCopyReceipt
{
    public PrivateOriginalMapNaturalStepCopyReceipt(
        OriginalMapStepCopyIdentity recordIdentity,
        MapPosition source,
        MapPosition trigger,
        WorkingMapBlockCopy copy,
        PrivateOriginalMapCollisionCategory beforeCollision,
        PrivateOriginalMapCollisionCategory afterCollision,
        long simulationStep)
    {
        RecordIdentity = recordIdentity ?? throw new ArgumentNullException(nameof(recordIdentity));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        Copy = copy ?? throw new ArgumentNullException(nameof(copy));
        if (!Enum.IsDefined(beforeCollision))
        {
            throw new ArgumentOutOfRangeException(nameof(beforeCollision));
        }

        if (!Enum.IsDefined(afterCollision))
        {
            throw new ArgumentOutOfRangeException(nameof(afterCollision));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        BeforeCollision = beforeCollision;
        AfterCollision = afterCollision;
        SimulationStep = simulationStep;
    }

    public OriginalMapStepCopyIdentity RecordIdentity { get; }

    public MapPosition Source { get; }

    public MapPosition Trigger { get; }

    public WorkingMapBlockCopy Copy { get; }

    public PrivateOriginalMapCollisionCategory BeforeCollision { get; }

    public PrivateOriginalMapCollisionCategory AfterCollision { get; }

    public long SimulationStep { get; }
}

public sealed partial class GameSession
{
    private bool TryApplyPrivateOriginalMapBowieDoorStepCopy(
        PrivateOriginalMapSessionSnapshot current,
        MoveExplorationCommand command,
        out PrivateOriginalMapMoveApplied? applied)
    {
        applied = null;
        OriginalMapStepCopyDefinition? admitted = current.Definition.BowieDoorStepCopy;
        MapPosition? candidate = current.Definition.Traversal.ResolveCandidateTarget(
            current.WorkingLayout,
            current.PlayerPosition,
            command.Direction);
        if (admitted is null ||
            current.BowieDoorStepCopyApplied ||
            current.PlayerPosition != new MapPosition(
                OriginalMapRuntimeAdmission.BowieDoorStepCopyApproachX,
                OriginalMapRuntimeAdmission.BowieDoorStepCopyApproachY) ||
            command.Direction != OriginalMapRuntimeAdmission.BowieDoorStepCopyDirection ||
            candidate != admitted.Trigger)
        {
            return false;
        }

        PrivateOriginalMapCollisionCategory before = ClassifyCollision(
            current,
            admitted.Trigger);
        WorkingMapLayout nextLayout = current.WorkingLayout.ApplyBlockCopy(admitted.Copy);
        PrivateOriginalMapCollisionCategory after = ClassifyCollision(
            current.Definition.Traversal,
            nextLayout,
            admitted.Trigger);
        OriginalMapTraversalResult traversal = current.Definition.Traversal.TryMove(
            nextLayout,
            current.PlayerPosition,
            command.Direction);
        if (before != PrivateOriginalMapCollisionCategory.BlockedByAcceptedCollisionClass ||
            after != PrivateOriginalMapCollisionCategory.ActiveNonBlocked ||
            traversal.Outcome != OriginalMapTraversalOutcome.Moved ||
            traversal.Position != admitted.Trigger)
        {
            throw new InvalidOperationException(
                "The admitted Bowie-door step-copy did not produce its exact atomic traversal result.");
        }

        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapNaturalStepCopyReceipt receipt = new(
            admitted.Identity,
            current.PlayerPosition,
            admitted.Trigger,
            admitted.Copy,
            before,
            after,
            nextStep);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            nextLayout,
            nextStep,
            traversal.Position,
            traversal,
            current.ControlledStepCopyApplied,
            lastLayoutMutation: null,
            lastSameMapWarp: null,
            roofOnLoadLifecycle: current.RoofOnLoadLifecycle,
            lastRoofOnLoad: null,
            bowieDoorStepCopyApplied: true,
            receipt);
        _privateOriginalMapSnapshot = next;
        applied = new PrivateOriginalMapMoveApplied(next, traversal);
        return true;
    }
}

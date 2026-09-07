using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapCrossMapTransitionReceipt
{
    internal PrivateOriginalMapCrossMapTransitionReceipt(
        OriginalMapCrossMapTransitionDefinition definition,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        RecordIdentity = definition.Identity;
        Source = definition.AdmittedApproach;
        Trigger = definition.AdmittedTrigger;
        DestinationMap = definition.DestinationMap;
        Destination = definition.Destination;
        DestinationOpaqueFacing = definition.DestinationOpaqueFacing;
        SimulationStep = simulationStep;
    }

    public string Capability => DestinationMap == new MapId(OriginalMapRuntimeAdmission.Map20Id)
        ? OriginalMapRuntimeAdmission.RoyalMap20TransitionCapability
        : RecordIdentity.SourceMap == new MapId(OriginalMapRuntimeAdmission.Map20Id)
            ? OriginalMapRuntimeAdmission.RoyalReturnMap19TransitionCapability
            : OriginalMapRuntimeAdmission.NorthMap19TransitionCapability;

    public OriginalMapCrossMapTransitionIdentity RecordIdentity { get; }

    public MapPosition Source { get; }

    public MapPosition Trigger { get; }

    public MapId DestinationMap { get; }

    public MapPosition Destination { get; }

    public byte DestinationOpaqueFacing { get; }

    public long SimulationStep { get; }
}

public sealed partial class GameSession
{
    private bool TryApplyPrivateOriginalMapCrossMapTransition(
        PrivateOriginalMapSessionSnapshot current,
        MoveExplorationCommand command,
        out PrivateOriginalMapMoveApplied? applied)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(command);
        applied = null;
        OriginalMapCrossMapTransitionDefinition? transition =
            current.Map == current.Definition.Map
                ? current.Definition.NorthMap19Transition
                : current.Map == new MapId(OriginalMapRuntimeAdmission.Map19Id)
                    ? current.Definition.RoyalMap20Transition
                    : current.Map == new MapId(OriginalMapRuntimeAdmission.Map20Id)
                        ? current.Definition.RoyalReturnMap19Transition
                        : null;
        if (transition is null ||
            current.Map != transition.Identity.SourceMap ||
            current.PlayerPosition != transition.AdmittedApproach ||
            command.Direction != transition.AdmittedDirection ||
            current.CastleGate?.Opened != true ||
            (current.Map == new MapId(OriginalMapRuntimeAdmission.Map20Id) && current.PalaceFirstVisit is null))
        {
            return false;
        }

        if (PrivateOriginalMapPlayerLocomotion.IsMoving)
        {
            throw new InvalidOperationException(
                "A cross-map relocation is unavailable while player locomotion is active.");
        }

        OriginalMapExplorationRuntimeDefinition destinationRuntime =
            current.Definition.RuntimeCatalog.Resolve(transition.DestinationMap);
        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapCrossMapTransitionReceipt receipt = new(transition, nextStep);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            destinationRuntime.WorkingLayout,
            nextStep,
            transition.Destination,
            lastTraversal: null,
            controlledStepCopyApplied: false,
            lastLayoutMutation: null,
            lastSameMapWarp: null,
            roofOnLoadLifecycle: MapBlockCopyLifecycleState.Inactive,
            lastRoofOnLoad: null,
            bowieDoorStepCopyApplied: false,
            lastNaturalStepCopy: null,
            schoolDoorStepCopyApplied: false,
            current.Zone601,
            lastZone601: null,
            current.Sarah,
            lastSarah: null,
            current.Entity142,
            pendingEntity142: null,
            lastEntity142Request: null,
            lastEntity142Acknowledgement: null,
            lastAstralZone: null,
            current.MessengerAcceptance,
            lastMessengerAcceptance: null,
            current.CastleGate,
            lastCastleGate: null,
            destinationRuntime,
            receipt,
            current.PalaceFirstVisit,
            current.AstralAcceptance);
        PrivateOriginalMapPlayerLocomotionSnapshot animation =
            PrivateOriginalMapPlayerLocomotionSnapshot.Relocate(PrivateOriginalMapPlayerLocomotion, receipt);
        _privateOriginalMapSnapshot = next;
        _privateOriginalMapPlayerLocomotion = animation;
        applied = new PrivateOriginalMapMoveApplied(next, receipt);
        return true;
    }
}

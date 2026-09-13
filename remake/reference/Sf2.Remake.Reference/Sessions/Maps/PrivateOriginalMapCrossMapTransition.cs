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

    public string Capability =>
        (RecordIdentity.Profile, RecordIdentity.SourceMap.Value,
            RecordIdentity.SourceResourceId, RecordIdentity.OneBasedRecordOrdinal) switch
        {
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.MapId,
                OriginalMapRuntimeAdmission.SameMapWarpResourceId, OriginalMapRuntimeAdmission.NorthMap19WarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.NorthMap19TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map19Id,
                OriginalMapRuntimeAdmission.RoyalMap20WarpResourceId, OriginalMapRuntimeAdmission.RoyalMap20WarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.RoyalMap20TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map20Id,
                OriginalMapRuntimeAdmission.RoyalReturnWarpResourceId, OriginalMapRuntimeAdmission.RoyalReturnWarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.RoyalReturnMap19TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map19Id,
                OriginalMapRuntimeAdmission.RoyalMap20WarpResourceId, OriginalMapRuntimeAdmission.WestTowerWarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.WestTowerMap20TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map20Id,
                OriginalMapRuntimeAdmission.RoyalReturnWarpResourceId, OriginalMapRuntimeAdmission.MiddleTowerWarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.MiddleTowerMap21TransitionCapability,
            (ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.Map21Id,
                OriginalMapRuntimeAdmission.NorthMap40WarpResourceId, OriginalMapRuntimeAdmission.NorthMap40WarpRecordOrdinal) =>
                OriginalMapRuntimeAdmission.NorthMap40TransitionCapability,
            _ => throw new InvalidOperationException("The cross-map receipt has no admitted source identity."),
        };

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
                    ? current.PlayerPosition == current.Definition.WestTowerMap20Transition?.AdmittedApproach
                        ? current.Definition.WestTowerMap20Transition
                        : current.Definition.RoyalMap20Transition
                    : current.Map == new MapId(OriginalMapRuntimeAdmission.Map20Id)
                        ? current.PlayerPosition == current.Definition.MiddleTowerMap21Transition?.AdmittedApproach
                            ? current.Definition.MiddleTowerMap21Transition
                            : current.Definition.RoyalReturnMap19Transition
                        : current.Map == new MapId(OriginalMapRuntimeAdmission.Map21Id)
                            ? current.Definition.NorthMap40Transition
                            : null;
        if (transition is null ||
            current.Map != transition.Identity.SourceMap ||
            current.PlayerPosition != transition.AdmittedApproach ||
            command.Direction != transition.AdmittedDirection ||
            current.CastleGate?.Opened != true ||
            (current.Map == new MapId(OriginalMapRuntimeAdmission.Map20Id) && current.PalaceFirstVisit is null) ||
            ((ReferenceEquals(transition, current.Definition.WestTowerMap20Transition) ||
                ReferenceEquals(transition, current.Definition.MiddleTowerMap21Transition)) &&
                (current.PalaceFirstVisit is null || current.AstralAcceptance is null)) ||
            (ReferenceEquals(transition, current.Definition.NorthMap40Transition) &&
                (current.PalaceFirstVisit is null || current.AstralAcceptance is null || current.MiddleTowerGuard is null)))
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
            current.AstralAcceptance, current.MiddleTowerGuard);
        PrivateOriginalMapPlayerLocomotionSnapshot animation =
            PrivateOriginalMapPlayerLocomotionSnapshot.Relocate(PrivateOriginalMapPlayerLocomotion, receipt);
        _privateOriginalMapSnapshot = next;
        _privateOriginalMapPlayerLocomotion = animation;
        applied = new PrivateOriginalMapMoveApplied(next, receipt);
        return true;
    }
}

using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapSameMapWarpReceipt
{
    public PrivateOriginalMapSameMapWarpReceipt(
        OriginalMapSameMapWarpIdentity recordIdentity,
        MapPosition source,
        MapPosition trigger,
        MapPosition destination,
        byte opaqueFacing,
        int sourceAreaOrdinal,
        int destinationAreaOrdinal,
        long simulationStep)
    {
        RecordIdentity = recordIdentity ??
            throw new ArgumentNullException(nameof(recordIdentity));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        if (source == destination || trigger == destination)
        {
            throw new ArgumentException(
                "A same-map warp receipt must describe an actual relocation.",
                nameof(destination));
        }

        int triggerDistance =
            Math.Abs(source.X - trigger.X) + Math.Abs(source.Y - trigger.Y);
        if (triggerDistance != 1)
        {
            throw new ArgumentException(
                "A same-map warp trigger must be the movement candidate adjacent to its source.",
                nameof(trigger));
        }

        if (opaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(opaqueFacing));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(sourceAreaOrdinal, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(destinationAreaOrdinal, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        OpaqueFacing = opaqueFacing;
        SourceAreaOrdinal = sourceAreaOrdinal;
        DestinationAreaOrdinal = destinationAreaOrdinal;
        SimulationStep = simulationStep;
    }

    public OriginalMapSameMapWarpIdentity RecordIdentity { get; }

    public MapPosition Source { get; }

    public MapPosition Trigger { get; }

    public MapPosition Destination { get; }

    public byte OpaqueFacing { get; }

    public int SourceAreaOrdinal { get; }

    public int DestinationAreaOrdinal { get; }

    public long SimulationStep { get; }
}

public sealed partial class GameSession
{
    private bool TryApplyPrivateOriginalMapSameMapWarp(
        PrivateOriginalMapSessionSnapshot current,
        MoveExplorationCommand command,
        out PrivateOriginalMapMoveApplied? applied)
    {
        applied = null;
        OriginalMapSameMapWarpCatalog? catalog = current.Definition.SameMapWarps;
        MapPosition? trigger = current.Definition.Traversal.ResolveCandidateTarget(
            current.WorkingLayout,
            current.PlayerPosition,
            command.Direction);
        if (catalog is null || trigger is null)
        {
            return false;
        }

        OriginalMapSameMapWarpDefinition? warp = catalog.Select(current.Map, trigger!);
        if (warp is null)
        {
            return false;
        }

        OriginalMapTraversalAreaSelection sourceArea = current.CurrentArea;
        OriginalMapTraversalAreaSelection destinationArea =
            current.Definition.Traversal.SelectActiveArea(warp.Destination) ??
            throw new InvalidOperationException(
                "The admitted same-map warp destination has no active area.");
        if (OriginalMapTraversal.IsBlocked(current.WorkingLayout, warp.Destination))
        {
            throw new InvalidOperationException(
                "The admitted same-map warp destination is blocked in the authoritative layout.");
        }

        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapSameMapWarpReceipt receipt = new(
            warp.Identity,
            current.PlayerPosition,
            warp.Trigger,
            warp.Destination,
            warp.OpaqueFacing,
            sourceArea.OneBasedRecordOrdinal,
            destinationArea.OneBasedRecordOrdinal,
            nextStep);
        WorkingMapLayout nextLayout = current.WorkingLayout;
        MapBlockCopyLifecycleState nextRoofLifecycle = current.RoofOnLoadLifecycle;
        PrivateOriginalMapRoofOnLoadReceipt? roofReceipt = null;
        OriginalMapRoofOnLoadDefinition? roof = current.Definition.RoofOnLoadClear;
        OriginalMapAreaDefinition destinationAreaDefinition =
            current.Definition.AreaCatalog.Resolve(destinationArea);
        if (roof is not null &&
            roof.AppliesTo(warp.Identity, destinationAreaDefinition.Identity))
        {
            MapBlockCopyLifecycleResult roofResult = MapBlockCopyLifecycleReducer.Activate(
                current.WorkingLayout,
                current.RoofOnLoadLifecycle,
                new MapViewUpdateState(
                    Channel0Requested: false,
                    Channel1Requested: false),
                roof.Identity.OneBasedRecordOrdinal,
                roof.CreateClearMutation());
            nextLayout = roofResult.Layout;
            nextRoofLifecycle = roofResult.LifecycleState;
            if (roofResult.UpdateMarks.Count != 0)
            {
                MapBlockCopyLifecycleActiveState active =
                    (MapBlockCopyLifecycleActiveState)roofResult.LifecycleState;
                roofReceipt = new PrivateOriginalMapRoofOnLoadReceipt(
                    roof.Identity,
                    warp.Identity,
                    destinationAreaDefinition.Identity,
                    roof.SourceTrigger,
                    roof.ClearDestination,
                    roof.Width,
                    roof.Height,
                    active.SavedWords.Count,
                    roofResult.UpdateState.Channel0Requested,
                    nextStep);
            }
        }

        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            nextLayout,
            nextStep,
            warp.Destination,
            lastTraversal: null,
            current.ControlledStepCopyApplied,
            lastLayoutMutation: null,
            receipt,
            nextRoofLifecycle,
            roofReceipt,
            current.BowieDoorStepCopyApplied,
            lastNaturalStepCopy: null,
            current.SchoolDoorStepCopyApplied,
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
            lastCastleGate: null);
        _privateOriginalMapSnapshot = next;
        applied = new PrivateOriginalMapMoveApplied(next, receipt, roofReceipt);
        return true;
    }

}

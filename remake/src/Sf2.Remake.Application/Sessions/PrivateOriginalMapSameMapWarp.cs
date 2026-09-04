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
        if (catalog is null ||
            !TryCandidateTarget(current.PlayerPosition, command.Direction, out MapPosition? trigger))
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
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            current.WorkingLayout,
            nextStep,
            warp.Destination,
            lastTraversal: null,
            current.ControlledStepCopyApplied,
            lastLayoutMutation: null,
            receipt);
        _privateOriginalMapSnapshot = next;
        applied = new PrivateOriginalMapMoveApplied(next, receipt);
        return true;
    }

    private static bool TryCandidateTarget(
        MapPosition source,
        ExplorationDirection direction,
        out MapPosition? candidate)
    {
        (int deltaX, int deltaY) = direction switch
        {
            ExplorationDirection.North => (0, -1),
            ExplorationDirection.East => (1, 0),
            ExplorationDirection.South => (0, 1),
            ExplorationDirection.West => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
        int x = source.X + deltaX;
        int y = source.Y + deltaY;
        if (x is < 0 or >= WorkingMapLayout.ColumnCount ||
            y is < 0 or >= WorkingMapLayout.RowCount)
        {
            candidate = null;
            return false;
        }

        candidate = new MapPosition(x, y);
        return true;
    }
}

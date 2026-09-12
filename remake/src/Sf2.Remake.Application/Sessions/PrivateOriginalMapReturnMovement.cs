using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapReturnInputReceipt(long Ordinal, OriginalMapTraversalResult Traversal);
public sealed record PrivateOriginalMapReturnDoorCopyReceipt(PrivateOriginalMapReturnArrivalSnapshot Entry,
    OriginalMapStepCopyDefinition Definition, PrivateOriginalMapReturnInputReceipt Input,
    ushort BeforeWord, ushort AfterWord);
public sealed record PrivateOriginalMapReturnRoofActionReceipt(long InputOrdinal, MapBlockCopyActionResult Action);

public abstract record PrivateOriginalMapReturnMovementResult;
public sealed record PrivateOriginalMapReturnMovementApplied(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalMapReturnMovementResult;
public sealed record PrivateOriginalMapReturnMovementUnsupported(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalMapReturnMovementResult;
public sealed record PrivateOriginalMapReturnMovementRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalMapReturnMovementResult;

public sealed partial class GameSession
{
    public PrivateOriginalMapReturnMovementResult BeginPrivateOriginalMapReturnMovement(
        PrivateOriginalBattle01SessionSnapshot? expected, MoveExplorationCommand? command)
    {
        if (ReturnMovementDiagnostic(expected) is { } failure) return new PrivateOriginalMapReturnMovementRejected(failure);
        if (command is null) return ReturnRejected("return.input");
        var current = PrivateOriginalBattle01!;
        var arrival = current.Arrival!;
        if (arrival.Locomotion.IsMoving) return ReturnRejected("return.busy");
        try
        {
            var (traversal, unsupported, layout) = EvaluateReturnMovement(arrival.CurrentRuntime.Traversal,
                arrival.WorkingLayout, arrival.PlayerPosition, command.Direction,
                arrival.LoadDefinition.ChurchDoor, arrival.Party.CurrentBattle);
            if (unsupported is not null) return new PrivateOriginalMapReturnMovementUnsupported(unsupported);
            var input = new PrivateOriginalMapReturnInputReceipt(checked(arrival.InputOrdinal + 1), traversal!);
            var door = ReferenceEquals(layout, arrival.WorkingLayout) ? arrival.DoorCopy :
                new PrivateOriginalMapReturnDoorCopyReceipt(arrival.EntryBeforeMovement ?? arrival,
                    arrival.LoadDefinition.ChurchDoor, input, arrival.WorkingLayout[32, 15], layout[32, 15]);
            return PublishReturnMovement(current, new(arrival, input, advance: false, layout, door));
        }
        catch (ArgumentException error) { return ReturnRejected(error.ParamName ?? "return.state"); }
        catch (OverflowException) { return ReturnRejected("return.ordinal"); }
    }

    public PrivateOriginalMapReturnMovementResult AdvancePrivateOriginalMapReturnMovement(
        PrivateOriginalBattle01SessionSnapshot? expected)
    {
        if (ReturnMovementDiagnostic(expected) is { } failure) return new PrivateOriginalMapReturnMovementRejected(failure);
        var current = PrivateOriginalBattle01!;
        var arrival = current.Arrival!;
        if (!arrival.Locomotion.IsMoving || arrival.LastInput is null) return ReturnRejected("return.idle");
        try { return PublishReturnMovement(current, new(arrival, arrival.LastInput, advance: true,
            arrival.WorkingLayout, arrival.DoorCopy)); }
        catch (ArgumentException error) { return ReturnRejected(error.ParamName ?? "return.state"); }
        catch (OverflowException) { return ReturnRejected("return.ordinal"); }
    }

    public PrivateOriginalMapReturnMovementResult RefusePrivateOriginalMapReturnInteraction(
        PrivateOriginalBattle01SessionSnapshot? expected)
    {
        if (ReturnMovementDiagnostic(expected) is { } failure) return new PrivateOriginalMapReturnMovementRejected(failure);
        if (PrivateOriginalBattle01!.Arrival!.Locomotion.IsMoving) return ReturnRejected("return.busy");
        return new PrivateOriginalMapReturnMovementUnsupported(new("return.interaction",
            "Unsupported: interactions, services and other gameplay actions remain unavailable."));
    }

    internal static (OriginalMapTraversalResult? Traversal, OriginalBattle01StartupDiagnostic? Unsupported, WorkingMapLayout Layout)
        EvaluateReturnMovement(OriginalMapTraversal traversal, WorkingMapLayout layout, MapPosition source,
            ExplorationDirection direction, OriginalMapStepCopyDefinition? churchDoor, byte currentBattle)
    {
        var candidate = traversal.ResolveCandidateTarget(layout, source, direction);
        if (candidate is null || !(candidate.X is >= 31 and <= 33 && candidate.Y is >= 12 and <= 14 ||
            candidate.X == 32 && candidate.Y is 15 or 16))
            return (null, new("return.region", "Unsupported: outside the church pocket and two-cell doorway route."), layout);
        if (candidate == new MapPosition(32, 13))
            return (null, new("return.followers", "Unsupported: the frozen follower cell is unavailable; original collision is Unknown."), layout);
        // esc02 calls OpenDoor before re-reading the target marker and checking passability.
        if ((layout[candidate.X, candidate.Y] & 0x3C00) == 0x0400)
        {
            if (currentBattle != 255 || !OriginalMapRuntimeAdmission.HasExactAcceptedChurchDoorStepCopy(churchDoor) ||
                candidate != churchDoor!.Trigger || source != new MapPosition(32, 14) ||
                direction != ExplorationDirection.South || layout[32, 15] != 0xC48F)
                throw new ArgumentException("Retain the admitted exploration doorway binding.", "return.door");
            layout = layout.ApplyBlockCopy(churchDoor.Copy);
            if (layout[32, 15] != 0x080E)
                throw new ArgumentException("The door copy must re-read as the admitted passable show marker.", "return.door");
        }
        int marker = layout[candidate.X, candidate.Y] & 0x3C00;
        if (marker is 0x0400 or 0x1000 or 0x1400 or 0x3800 or 0x3C00)
            return (null, new("return.event", "Unsupported: an unadmitted door, warp, zone or transport marker."), layout);
        return (traversal.TryMove(layout, source, direction), null, layout);
    }

    private OriginalBattle01StartupDiagnostic? ReturnMovementDiagnostic(PrivateOriginalBattle01SessionSnapshot? expected)
    {
        var current = PrivateOriginalBattle01;
        if (current is null || expected is null || !ReferenceEquals(current, expected))
            return ReturnDiagnostic("snapshot");
        if (current.Arrival is not { } arrival) return ReturnDiagnostic("return.phase");
        var entry = arrival.EntryBeforeMovement ?? arrival;
        if (!ReferenceEquals(current.Preparation, entry.Before.Preparation) ||
            !ReferenceEquals(current.Battle, entry.Before.Battle) ||
            !ReferenceEquals(current.DefeatReturn, entry.Before.DefeatReturn) || !entry.Before.CanEnterExploration ||
            !ReferenceEquals(current.SourceLocomotion, entry.Before.SourceLocomotion) ||
            !ReferenceEquals(current.SourceBridge, entry.Before.SourceBridge) ||
            !ReferenceEquals(arrival.LoadDefinition, current.Preparation.ArrivalLoad) ||
            GetArrivalSourceDiagnostic(current.SourceSnapshot, arrival.LoadDefinition) is not null)
            return ReturnDiagnostic("return.binding");
        try { arrival.ValidateCurrentState(); }
        catch (ArgumentException error) { return ReturnDiagnostic(error.ParamName ?? "return.state"); }
        // Exact admitted pocket, not a generic event-free map or an alternate import.
        ushort[] words = [0x00DB, 0xE8DC, 0x00DD, 0x0061, 0x0062, 0x0063, 0x0076, 0x0077, 0x0078];
        for (int y = 12, index = 0; y <= 14; y++) for (int x = 31; x <= 33; x++, index++)
            if (arrival.WorkingLayout[x, y] != words[index] ||
                arrival.CurrentRuntime.Traversal.SelectActiveArea(new(x, y))?.OneBasedRecordOrdinal != 1)
                return ReturnDiagnostic("return.layout");
        return null;
    }

    private PrivateOriginalMapReturnMovementApplied PublishReturnMovement(PrivateOriginalBattle01SessionSnapshot current,
        PrivateOriginalMapReturnArrivalSnapshot arrival)
    {
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, current.Battle,
            current.SourceLocomotion, current.SourceBridge, current.DefeatReturn, arrival);
        var result = new PrivateOriginalMapReturnMovementApplied(next);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static OriginalBattle01StartupDiagnostic ReturnDiagnostic(string field) =>
        new(field, "Return movement rejected (" + field + "); current state retained.");
    private static PrivateOriginalMapReturnMovementRejected ReturnRejected(string field) => new(ReturnDiagnostic(field));
}

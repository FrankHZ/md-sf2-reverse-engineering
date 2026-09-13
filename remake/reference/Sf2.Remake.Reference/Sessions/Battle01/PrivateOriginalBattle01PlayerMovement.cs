using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum PrivateOriginalBattle01PlayerMovementOperation { SelectDestination, Confirm, Cancel }
public abstract record PrivateOriginalBattle01PlayerMovementResult;
public sealed record PrivateOriginalBattle01PlayerMovementApplied(PrivateOriginalBattle01SessionSnapshot Snapshot,
    PrivateOriginalBattle01PlayerMovementOperation Operation) : PrivateOriginalBattle01PlayerMovementResult;
public sealed record PrivateOriginalBattle01PlayerMovementRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01PlayerMovementResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01PlayerMovementResult SelectPrivateOriginalBattle01PlayerDestination(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex, MapPosition destination) =>
        ApplyPrivateOriginalBattle01PlayerMovement(expected, PrivateOriginalBattle01PlayerMovementOperation.SelectDestination,
            battle => Battle01PlayerMovement.SelectDestination(battle, actorIndex, destination));

    public PrivateOriginalBattle01PlayerMovementResult ConfirmPrivateOriginalBattle01PlayerMovement(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex) =>
        ApplyPrivateOriginalBattle01PlayerMovement(expected, PrivateOriginalBattle01PlayerMovementOperation.Confirm,
            battle => Battle01PlayerMovement.Confirm(battle, actorIndex));

    public PrivateOriginalBattle01PlayerMovementResult CancelPrivateOriginalBattle01PlayerMovement(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex) =>
        ApplyPrivateOriginalBattle01PlayerMovement(expected, PrivateOriginalBattle01PlayerMovementOperation.Cancel,
            battle => Battle01PlayerMovement.Cancel(battle, actorIndex));

    private PrivateOriginalBattle01PlayerMovementResult ApplyPrivateOriginalBattle01PlayerMovement(
        PrivateOriginalBattle01SessionSnapshot? expected, PrivateOriginalBattle01PlayerMovementOperation operation,
        Func<Battle01InitializedState, Battle01InitializedState> transition)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return MovementRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return MovementRejected("snapshot");
        Battle01InitializedState battle;
        try { battle = transition(current.Battle); }
        catch (ArgumentException error) { return MovementRejected(error.ParamName ?? "movement"); }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle, current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01PlayerMovementApplied(next, operation);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01PlayerMovementRejected MovementRejected(string field) =>
        new(new(field, "The request cannot change this session's current player movement state."));
}

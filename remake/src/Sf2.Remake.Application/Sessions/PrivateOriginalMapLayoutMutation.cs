using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum PrivateOriginalMapCollisionCategory
{
    OutsideAcceptedActiveArea,
    ActiveNonBlocked,
    BlockedByAcceptedCollisionClass,
}

public sealed record ApplyPrivateOriginalMapLayoutMutationCommand
{
    public ApplyPrivateOriginalMapLayoutMutationCommand(
        OriginalMapStepCopyIdentity recordIdentity,
        long expectedSimulationStep)
    {
        RecordIdentity = recordIdentity ?? throw new ArgumentNullException(nameof(recordIdentity));
        ArgumentOutOfRangeException.ThrowIfNegative(expectedSimulationStep);
        ExpectedSimulationStep = expectedSimulationStep;
    }

    public OriginalMapStepCopyIdentity RecordIdentity { get; }

    public long ExpectedSimulationStep { get; }
}

public sealed record PrivateOriginalMapLayoutMutationReceipt
{
    public PrivateOriginalMapLayoutMutationReceipt(
        OriginalMapStepCopyIdentity recordIdentity,
        MapPosition trigger,
        WorkingMapBlockCopy copy,
        PrivateOriginalMapCollisionCategory beforeCollision,
        PrivateOriginalMapCollisionCategory afterCollision,
        long simulationStep)
    {
        RecordIdentity = recordIdentity ?? throw new ArgumentNullException(nameof(recordIdentity));
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

    public MapPosition Trigger { get; }

    public WorkingMapBlockCopy Copy { get; }

    public PrivateOriginalMapCollisionCategory BeforeCollision { get; }

    public PrivateOriginalMapCollisionCategory AfterCollision { get; }

    public long SimulationStep { get; }
}

public enum PrivateOriginalMapLayoutMutationFailureCode
{
    ReferenceMismatch,
    StaleSimulationStep,
    AlreadyApplied,
}

public sealed record PrivateOriginalMapLayoutMutationDiagnostic(
    PrivateOriginalMapLayoutMutationFailureCode Code,
    string Message)
{
    public PrivateOriginalMapLayoutMutationFailureCode Code { get; } =
        Enum.IsDefined(Code) ? Code : throw new ArgumentOutOfRangeException(nameof(Code));

    public string Message { get; } = !string.IsNullOrWhiteSpace(Message)
        ? Message
        : throw new ArgumentException("A mutation diagnostic requires a message.", nameof(Message));
}

public abstract record PrivateOriginalMapLayoutMutationResult;

public sealed record PrivateOriginalMapLayoutMutationApplied(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapLayoutMutationReceipt Receipt) :
    PrivateOriginalMapLayoutMutationResult
{
    public PrivateOriginalMapSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public PrivateOriginalMapLayoutMutationReceipt Receipt { get; } =
        Receipt ?? throw new ArgumentNullException(nameof(Receipt));
}

public sealed record PrivateOriginalMapLayoutMutationRejected(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapLayoutMutationDiagnostic Diagnostic) :
    PrivateOriginalMapLayoutMutationResult
{
    public PrivateOriginalMapSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public PrivateOriginalMapLayoutMutationDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

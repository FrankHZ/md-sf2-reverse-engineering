using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record CompletePrivateOriginalMapMiddleTowerGuardCommand(
    long ExpectedSimulationStep, OriginalMapMiddleTowerGuardPreset Preset) : IGameSessionCommand;

/// <summary>One controlled Map 21 completion; not a global flag store or natural map lifecycle.</summary>
public sealed record PrivateOriginalMapMiddleTowerGuardReceipt
{
    internal PrivateOriginalMapMiddleTowerGuardReceipt(OriginalMapMiddleTowerGuardDefinition definition, long simulationStep)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        SimulationStep = simulationStep;
    }

    public OriginalMapMiddleTowerGuardDefinition Definition { get; }
    public OriginalMapMiddleTowerGuardPreset Preset => Definition.Preset;
    public long SimulationStep { get; }
    public MapPosition ActorPosition => Definition.AcceptedActorEndpoint;
    public bool HandlerFlag256Set => true;
    public bool ProgramStoryFlag1Set => true;
    public bool ProgramFlag401Set => true;
}

public enum PrivateOriginalMapMiddleTowerGuardFailureCode
{
    StaleSimulationStep,
    LocomotionBusy,
    BattleBridgeBusy,
    InvalidPreset,
    InteractionTargetMismatch,
    AlreadyCompleted,
}

public sealed record PrivateOriginalMapMiddleTowerGuardApplied(
    PrivateOriginalMapSessionSnapshot Snapshot, PrivateOriginalMapMiddleTowerGuardReceipt Receipt) : PrivateOriginalMapInteractionResult;

public sealed record PrivateOriginalMapMiddleTowerGuardRejected(
    PrivateOriginalMapSessionSnapshot Snapshot, PrivateOriginalMapMiddleTowerGuardFailureCode Code) : PrivateOriginalMapInteractionResult;

public sealed partial class GameSession
{
    public PrivateOriginalMapInteractionResult CompletePrivateOriginalMapMiddleTowerGuard(
        CompletePrivateOriginalMapMiddleTowerGuardCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var current = PrivateOriginalMapSnapshot;
        PrivateOriginalMapMiddleTowerGuardRejected Reject(PrivateOriginalMapMiddleTowerGuardFailureCode code) => new(current, code);
        if (command.ExpectedSimulationStep != current.SimulationStep)
            return Reject(PrivateOriginalMapMiddleTowerGuardFailureCode.StaleSimulationStep);
        if (PrivateOriginalMapPlayerLocomotion.IsMoving)
            return Reject(PrivateOriginalMapMiddleTowerGuardFailureCode.LocomotionBusy);
        if (IsPrivateOriginalMapBattleBridgeBusy)
            return Reject(PrivateOriginalMapMiddleTowerGuardFailureCode.BattleBridgeBusy);
        if (command.Preset != OriginalMapMiddleTowerGuardPreset.ControlledPostAstralAndLocal256Clear)
            return Reject(PrivateOriginalMapMiddleTowerGuardFailureCode.InvalidPreset);
        if (current.MiddleTowerGuard is not null)
            return Reject(PrivateOriginalMapMiddleTowerGuardFailureCode.AlreadyCompleted);
        if (!current.CanAcceptMiddleTowerGuard(PrivateOriginalMapPlayerLocomotion.OpaqueFacing))
            return Reject(PrivateOriginalMapMiddleTowerGuardFailureCode.InteractionTargetMismatch);

        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapMiddleTowerGuardReceipt completed = new(current.Definition.MiddleTowerGuard!, nextStep);
        // Explicit remake policy: preserve the player's position/facing and locomotion on F.
        // The source only moves entity 128; entity 135's identity/facing effect remains Unknown.
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition, current.Receipt, current.WorkingLayout, nextStep, current.PlayerPosition,
            lastTraversal: null, current.ControlledStepCopyApplied, lastLayoutMutation: null,
            roofOnLoadLifecycle: current.RoofOnLoadLifecycle,
            bowieDoorStepCopyApplied: current.BowieDoorStepCopyApplied,
            schoolDoorStepCopyApplied: current.SchoolDoorStepCopyApplied,
            zone601: current.Zone601, sarah: current.Sarah, entity142: current.Entity142,
            messengerAcceptance: current.MessengerAcceptance, castleGate: current.CastleGate,
            currentRuntime: current.CurrentRuntime, palaceFirstVisit: current.PalaceFirstVisit,
            astralAcceptance: current.AstralAcceptance, middleTowerGuard: completed);
        _privateOriginalMapSnapshot = next;
        return new PrivateOriginalMapMiddleTowerGuardApplied(next, completed);
    }
}

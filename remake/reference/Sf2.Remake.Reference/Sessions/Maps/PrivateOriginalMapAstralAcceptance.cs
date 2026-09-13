using Sf2.Remake.Application.Content;

namespace Sf2.Remake.Application.Sessions;

/// <summary>Once-only controlled completion; no natural caller or script execution claim.</summary>
public sealed record PrivateOriginalMapAstralAcceptanceState
{
    internal PrivateOriginalMapAstralAcceptanceState(
        OriginalMapAstralAcceptanceDefinition definition, long simulationStep)
    {
        Definition = definition;
        SimulationStep = simulationStep;
    }

    public OriginalMapAstralAcceptanceDefinition Definition { get; }
    public long SimulationStep { get; }
    public bool HandlerFlag607Set => true;
    public bool ProgramFlag608Set => true;
}

public enum PrivateOriginalMapAstralAcceptanceFailureCode
{
    StaleSimulationStep,
    LocomotionBusy,
    BattleBridgeBusy,
    InteractionTargetMismatch,
    AlreadyCompleted,
}

public sealed record PrivateOriginalMapAstralAcceptanceApplied(
    PrivateOriginalMapSessionSnapshot Snapshot) : PrivateOriginalMapInteractionResult;

public sealed record PrivateOriginalMapAstralAcceptanceRejected(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapAstralAcceptanceFailureCode Code) : PrivateOriginalMapInteractionResult;

public sealed partial class GameSession
{
    private PrivateOriginalMapInteractionResult AcceptPrivateOriginalMapAstral(long expectedSimulationStep)
    {
        PrivateOriginalMapSessionSnapshot current = PrivateOriginalMapSnapshot;
        PrivateOriginalMapAstralAcceptanceRejected Reject(PrivateOriginalMapAstralAcceptanceFailureCode code) =>
            new(current, code);
        if (expectedSimulationStep != current.SimulationStep)
        {
            return Reject(PrivateOriginalMapAstralAcceptanceFailureCode.StaleSimulationStep);
        }

        if (PrivateOriginalMapPlayerLocomotion.IsMoving)
        {
            return Reject(PrivateOriginalMapAstralAcceptanceFailureCode.LocomotionBusy);
        }

        if (IsPrivateOriginalMapBattleBridgeBusy)
        {
            return Reject(PrivateOriginalMapAstralAcceptanceFailureCode.BattleBridgeBusy);
        }

        if (current.AstralAcceptance is not null)
        {
            return Reject(PrivateOriginalMapAstralAcceptanceFailureCode.AlreadyCompleted);
        }

        if (!current.CanAcceptAstral(PrivateOriginalMapPlayerLocomotion.OpaqueFacing))
        {
            return Reject(PrivateOriginalMapAstralAcceptanceFailureCode.InteractionTargetMismatch);
        }

        // F explicitly chooses this acceptance result. The messenger's earlier flag 89 is irrelevant.
        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapAstralAcceptanceState completed = new(current.Definition.AstralAcceptance!, nextStep);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition, current.Receipt, current.WorkingLayout, nextStep, current.PlayerPosition,
            lastTraversal: null, current.ControlledStepCopyApplied, lastLayoutMutation: null,
            roofOnLoadLifecycle: current.RoofOnLoadLifecycle,
            bowieDoorStepCopyApplied: current.BowieDoorStepCopyApplied,
            schoolDoorStepCopyApplied: current.SchoolDoorStepCopyApplied,
            zone601: current.Zone601, sarah: current.Sarah, entity142: current.Entity142,
            messengerAcceptance: current.MessengerAcceptance, castleGate: current.CastleGate,
            currentRuntime: current.CurrentRuntime, palaceFirstVisit: current.PalaceFirstVisit,
            astralAcceptance: completed);
        _privateOriginalMapSnapshot = next;
        return new PrivateOriginalMapAstralAcceptanceApplied(next);
    }
}

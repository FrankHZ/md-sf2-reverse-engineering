using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record CompletePrivateOriginalMapPalaceFirstVisitCommand(
    long ExpectedSimulationStep,
    OriginalMapPalaceFirstVisitPreset Preset) : IGameSessionCommand;

/// <summary>Retained controlled completion, separate from immutable source entity-population data.</summary>
public sealed record PrivateOriginalMapPalaceFirstVisitReceipt
{
    internal PrivateOriginalMapPalaceFirstVisitReceipt(
        OriginalMapPalaceFirstVisitDefinition definition, long simulationStep)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        SimulationStep = simulationStep;
    }

    public OriginalMapPalaceFirstVisitDefinition Definition { get; }
    public OriginalMapPalaceFirstVisitPreset Preset => Definition.Preset;
    public long SimulationStep { get; }
    public bool CompletionFlag605Set => true;
    public MapPosition PlayerSource => Definition.Entry;
    public MapPosition PlayerEndpoint => Definition.PlayerEndpoint;
    public byte PlayerOpaqueFacing => Definition.PlayerOpaqueFacing;
    public MapPosition Entity131Endpoint => Definition.Entity131Endpoint;
    public bool Entity130Hidden => true;
}

public enum PrivateOriginalMapPalaceFirstVisitFailureCode
{
    StaleSimulationStep,
    LocomotionBusy,
    BattleBridgeBusy,
    InvalidPreset,
    EntryMismatch,
    AlreadyCompleted,
}

public sealed record PrivateOriginalMapPalaceFirstVisitApplied(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapPalaceFirstVisitReceipt Receipt) : PrivateOriginalMapInteractionResult;

public sealed record PrivateOriginalMapPalaceFirstVisitRejected(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapPalaceFirstVisitFailureCode Code) : PrivateOriginalMapInteractionResult;

public sealed partial class GameSession
{
    public PrivateOriginalMapInteractionResult CompletePrivateOriginalMapPalaceFirstVisit(
        CompletePrivateOriginalMapPalaceFirstVisitCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        PrivateOriginalMapSessionSnapshot current = PrivateOriginalMapSnapshot;
        PrivateOriginalMapPalaceFirstVisitRejected Reject(PrivateOriginalMapPalaceFirstVisitFailureCode code) =>
            new(current, code);
        if (command.ExpectedSimulationStep != current.SimulationStep)
        {
            return Reject(PrivateOriginalMapPalaceFirstVisitFailureCode.StaleSimulationStep);
        }

        if (PrivateOriginalMapPlayerLocomotion.IsMoving)
        {
            return Reject(PrivateOriginalMapPalaceFirstVisitFailureCode.LocomotionBusy);
        }

        if (IsPrivateOriginalMapBattleBridgeBusy)
        {
            return Reject(PrivateOriginalMapPalaceFirstVisitFailureCode.BattleBridgeBusy);
        }

        OriginalMapPalaceFirstVisitDefinition? definition = current.Definition.PalaceFirstVisit;
        if (definition is null || current.Map != definition.Map)
        {
            return Reject(PrivateOriginalMapPalaceFirstVisitFailureCode.EntryMismatch);
        }

        if (command.Preset != definition.Preset)
        {
            return Reject(PrivateOriginalMapPalaceFirstVisitFailureCode.InvalidPreset);
        }

        if (current.PalaceFirstVisit is not null)
        {
            return Reject(PrivateOriginalMapPalaceFirstVisitFailureCode.AlreadyCompleted);
        }

        if (current.PlayerPosition != definition.Entry || current.CastleGate?.Opened != true ||
            current.MessengerAcceptance?.Accepted != true || current.PendingEntity142 is not null)
        {
            return Reject(PrivateOriginalMapPalaceFirstVisitFailureCode.EntryMismatch);
        }

        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapPalaceFirstVisitReceipt receipt = new(definition, nextStep);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition, current.Receipt, current.WorkingLayout, nextStep, definition.PlayerEndpoint,
            lastTraversal: null, current.ControlledStepCopyApplied, lastLayoutMutation: null,
            roofOnLoadLifecycle: current.RoofOnLoadLifecycle,
            bowieDoorStepCopyApplied: current.BowieDoorStepCopyApplied,
            schoolDoorStepCopyApplied: current.SchoolDoorStepCopyApplied,
            zone601: current.Zone601, sarah: current.Sarah, entity142: current.Entity142,
            messengerAcceptance: current.MessengerAcceptance, castleGate: current.CastleGate,
            currentRuntime: current.CurrentRuntime, palaceFirstVisit: receipt);
        PrivateOriginalMapPlayerLocomotionSnapshot animation =
            PrivateOriginalMapPlayerLocomotionSnapshot.CompletePalaceFirstVisit(
                PrivateOriginalMapPlayerLocomotion, receipt);
        _privateOriginalMapSnapshot = next;
        _privateOriginalMapPlayerLocomotion = animation;
        return new PrivateOriginalMapPalaceFirstVisitApplied(next, receipt);
    }
}

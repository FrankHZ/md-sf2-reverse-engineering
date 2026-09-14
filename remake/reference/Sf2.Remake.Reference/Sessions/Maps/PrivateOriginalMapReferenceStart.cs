using System.Text.Json;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

// Temporary input for the remaining castle/tower/return comparisons. It is never an opening execution result.
public sealed record PrivateOriginalMapReferenceStart(int FormatVersion, string Boundary, int X, int Y,
    bool BowieDoorOpen, bool SchoolDoorOpen, bool AfterMessenger)
{
    public static PrivateOriginalMapReferenceStart Read(string path) =>
        JsonSerializer.Deserialize<PrivateOriginalMapReferenceStart>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new ArgumentException("reference-start");
}

public sealed partial class GameSession
{
    public PrivateOriginalMapReferenceStart? PrivateOriginalMapReferenceInput { get; private set; }

    public void ApplyPrivateOriginalMapReferenceStart(PrivateOriginalMapReferenceStart input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var current = PrivateOriginalMapSnapshot;
        if (current.SimulationStep != 0 || PrivateOriginalMapReferenceInput is not null || input.FormatVersion != 1 || string.IsNullOrWhiteSpace(input.Boundary) ||
            !input.AfterMessenger || !input.BowieDoorOpen || !input.SchoolDoorOpen)
            throw new ArgumentException("An explicit fresh post-opening reference input is required.", nameof(input));
        var definition = current.Definition;
        var zone = definition.Zone601 ?? throw new ArgumentException("reference-start.zone601");
        var sarah = definition.Sarah ?? throw new ArgumentException("reference-start.sarah");
        var astral = definition.AstralZone ?? throw new ArgumentException("reference-start.astral");
        var entity = definition.Entity142 ?? throw new ArgumentException("reference-start.entity142");
        var messenger = definition.MessengerAcceptance ?? throw new ArgumentException("reference-start.messenger");
        var layout = current.WorkingLayout.ApplyBlockCopy(definition.BowieDoorStepCopy!.Copy).ApplyBlockCopy(definition.ControlledStepCopy!.Copy);
        _privateOriginalMapSnapshot = new(definition, current.Receipt, layout, simulationStep: 0,
            new(input.X, input.Y), lastTraversal: null, controlledStepCopyApplied: false, lastLayoutMutation: null,
            bowieDoorStepCopyApplied: input.BowieDoorOpen, schoolDoorStepCopyApplied: input.SchoolDoorOpen,
            zone601: PrivateOriginalMapZone601State.AstralZoneRepositioned(zone, astral),
            sarah: PrivateOriginalMapSarahState.MessengerFollowerReady(sarah, astral, messenger),
            entity142: PrivateOriginalMapEntity142State.ReleaseRouteOccupancy(entity,
                PrivateOriginalMapEntity142State.Acknowledged(entity, 1), messenger),
            messengerAcceptance: PrivateOriginalMapMessengerAcceptanceState.Completed(messenger), currentRuntime: definition.InitialRuntime,
            referenceStart: input);
        PrivateOriginalMapReferenceInput = input;
        InitializePrivateOriginalMapPlayerLocomotion();
    }
}

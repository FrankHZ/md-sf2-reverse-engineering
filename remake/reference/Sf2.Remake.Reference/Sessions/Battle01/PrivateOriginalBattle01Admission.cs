using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed class PrivateOriginalBattle01PendingAdmission
{
    internal PrivateOriginalBattle01PendingAdmission(OriginalBattle01AdmissionDefinition definition,
        PrivateOriginalMapSessionSnapshot sourceSnapshot, MapPosition trigger)
    {
        Definition = definition;
        SourceSnapshot = sourceSnapshot;
        Trigger = trigger;
    }

    public OriginalBattle01AdmissionDefinition Definition { get; }
    public PrivateOriginalMapSessionSnapshot SourceSnapshot { get; }
    public MapPosition Trigger { get; }
    public MapPosition Source => SourceSnapshot.PlayerPosition;
    public long SourceSimulationStep => SourceSnapshot.SimulationStep;
}

public sealed partial class GameSession
{
    // Retained only as an explicit reference fixture binding for independent M4 comparisons.
    // Field movement no longer manufactures this endpoint; common ProgramRunner owns admission.
    public PrivateOriginalBattle01PendingAdmission? PrivateOriginalBattle01Admission { get; private set; }
}

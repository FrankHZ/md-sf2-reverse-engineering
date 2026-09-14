using Sf2.Remake.Application.Content;

namespace Sf2.Remake.Application.Sessions;

/// <summary>Immutable comparison metadata for later battle/return fixtures; no execution writer remains.</summary>
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

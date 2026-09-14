using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

/// <summary>Immutable comparison metadata for later battle/return fixtures; no execution writer remains.</summary>
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

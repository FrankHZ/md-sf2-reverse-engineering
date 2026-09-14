using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

/// <summary>Immutable comparison metadata for later battle/return fixtures; no execution writer remains.</summary>
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

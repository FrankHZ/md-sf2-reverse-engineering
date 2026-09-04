using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapRoofOnLoadReceipt
{
    public PrivateOriginalMapRoofOnLoadReceipt(
        OriginalMapRoofOnLoadIdentity recordIdentity,
        OriginalMapSameMapWarpIdentity appliedAfterWarp,
        OriginalMapAreaRecordIdentity destinationArea,
        MapPosition sourceTrigger,
        MapPosition clearDestination,
        int width,
        int height,
        int savedCellCount,
        bool viewUpdateRequested,
        long simulationStep)
    {
        RecordIdentity = recordIdentity ??
            throw new ArgumentNullException(nameof(recordIdentity));
        AppliedAfterWarp = appliedAfterWarp ??
            throw new ArgumentNullException(nameof(appliedAfterWarp));
        DestinationArea = destinationArea ??
            throw new ArgumentNullException(nameof(destinationArea));
        SourceTrigger = sourceTrigger ?? throw new ArgumentNullException(nameof(sourceTrigger));
        ClearDestination = clearDestination ??
            throw new ArgumentNullException(nameof(clearDestination));
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        if (savedCellCount != checked(width * height))
        {
            throw new ArgumentException(
                "A roof-on-load receipt must retain the exact saved region size.",
                nameof(savedCellCount));
        }

        if (!viewUpdateRequested)
        {
            throw new ArgumentException(
                "An applied roof-on-load clear must request the typed view update.",
                nameof(viewUpdateRequested));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        Width = width;
        Height = height;
        SavedCellCount = savedCellCount;
        ViewUpdateRequested = viewUpdateRequested;
        SimulationStep = simulationStep;
    }

    public OriginalMapRoofOnLoadIdentity RecordIdentity { get; }

    public OriginalMapSameMapWarpIdentity AppliedAfterWarp { get; }

    public OriginalMapAreaRecordIdentity DestinationArea { get; }

    public MapPosition SourceTrigger { get; }

    public MapPosition ClearDestination { get; }

    public int Width { get; }

    public int Height { get; }

    public int SavedCellCount { get; }

    public bool ViewUpdateRequested { get; }

    public long SimulationStep { get; }
}

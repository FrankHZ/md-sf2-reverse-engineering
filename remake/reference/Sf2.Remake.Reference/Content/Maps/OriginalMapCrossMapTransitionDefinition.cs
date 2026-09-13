using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapCrossMapTransitionIdentity
{
    public OriginalMapCrossMapTransitionIdentity(
        ContentProfile profile,
        MapId sourceMap,
        string sourceResourceId,
        int oneBasedRecordOrdinal)
    {
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        SourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceResourceId);
        ArgumentOutOfRangeException.ThrowIfLessThan(oneBasedRecordOrdinal, 1);
        Profile = profile;
        SourceResourceId = sourceResourceId;
        OneBasedRecordOrdinal = oneBasedRecordOrdinal;
    }

    public ContentProfile Profile { get; }

    public MapId SourceMap { get; }

    public string SourceResourceId { get; }

    public int OneBasedRecordOrdinal { get; }
}

public sealed record OriginalMapCrossMapTransitionDefinition
{
    public OriginalMapCrossMapTransitionDefinition(
        OriginalMapCrossMapTransitionIdentity identity,
        byte sourceTriggerX,
        byte sourceTriggerY,
        MapPosition admittedApproach,
        ExplorationDirection admittedDirection,
        MapPosition admittedTrigger,
        MapId destinationMap,
        MapPosition destination,
        byte destinationOpaqueFacing)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        AdmittedApproach = admittedApproach ??
            throw new ArgumentNullException(nameof(admittedApproach));
        if (!Enum.IsDefined(admittedDirection))
        {
            throw new ArgumentOutOfRangeException(nameof(admittedDirection));
        }

        AdmittedTrigger = admittedTrigger ?? throw new ArgumentNullException(nameof(admittedTrigger));
        DestinationMap = destinationMap ?? throw new ArgumentNullException(nameof(destinationMap));
        Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        if (identity.SourceMap == destinationMap)
        {
            throw new ArgumentException(
                "A cross-map transition must change maps.",
                nameof(destinationMap));
        }

        SourceTriggerX = sourceTriggerX;
        SourceTriggerY = sourceTriggerY;
        AdmittedDirection = admittedDirection;
        DestinationOpaqueFacing = destinationOpaqueFacing;
    }

    public OriginalMapCrossMapTransitionIdentity Identity { get; }

    public byte SourceTriggerX { get; }

    public byte SourceTriggerY { get; }

    public MapPosition AdmittedApproach { get; }

    public ExplorationDirection AdmittedDirection { get; }

    public MapPosition AdmittedTrigger { get; }

    public MapId DestinationMap { get; }

    public MapPosition Destination { get; }

    public byte DestinationOpaqueFacing { get; }
}

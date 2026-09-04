using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapRoofOnLoadIdentity
{
    public OriginalMapRoofOnLoadIdentity(
        ContentProfile profile,
        MapId map,
        string resourceId,
        int oneBasedRecordOrdinal)
    {
        if (profile != ContentProfile.PrivateLocal)
        {
            throw new ArgumentException(
                "An original roof-on-load identity must remain PrivateLocal.",
                nameof(profile));
        }

        Map = map ?? throw new ArgumentNullException(nameof(map));
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentOutOfRangeException.ThrowIfLessThan(oneBasedRecordOrdinal, 1);
        Profile = profile;
        ResourceId = resourceId;
        OneBasedRecordOrdinal = oneBasedRecordOrdinal;
    }

    public ContentProfile Profile { get; }

    public MapId Map { get; }

    public string ResourceId { get; }

    public int OneBasedRecordOrdinal { get; }
}

public sealed record OriginalMapRoofOnLoadDefinition
{
    public OriginalMapRoofOnLoadDefinition(
        OriginalMapRoofOnLoadIdentity identity,
        MapPosition sourceTrigger,
        MapPosition clearDestination,
        int width,
        int height,
        OriginalMapSameMapWarpIdentity appliedAfterWarp,
        OriginalMapAreaRecordIdentity destinationArea)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        SourceTrigger = sourceTrigger ?? throw new ArgumentNullException(nameof(sourceTrigger));
        ClearDestination = clearDestination ??
            throw new ArgumentNullException(nameof(clearDestination));
        AppliedAfterWarp = appliedAfterWarp ??
            throw new ArgumentNullException(nameof(appliedAfterWarp));
        DestinationArea = destinationArea ??
            throw new ArgumentNullException(nameof(destinationArea));
        if (appliedAfterWarp.Profile != ContentProfile.PrivateLocal ||
            appliedAfterWarp.Map != identity.Map)
        {
            throw new ArgumentException(
                "A roof-on-load definition must bind one private same-map warp.",
                nameof(appliedAfterWarp));
        }

        _ = MapBlockRegionMutation.Clear(
            clearDestination.X,
            clearDestination.Y,
            width,
            height);
        Width = width;
        Height = height;
    }

    public OriginalMapRoofOnLoadIdentity Identity { get; }

    public MapPosition SourceTrigger { get; }

    public MapPosition ClearDestination { get; }

    public int Width { get; }

    public int Height { get; }

    public OriginalMapSameMapWarpIdentity AppliedAfterWarp { get; }

    public OriginalMapAreaRecordIdentity DestinationArea { get; }

    internal MapBlockRegionMutation CreateClearMutation() =>
        MapBlockRegionMutation.Clear(
            ClearDestination.X,
            ClearDestination.Y,
            Width,
            Height);

    internal bool AppliesTo(
        OriginalMapSameMapWarpIdentity warp,
        OriginalMapAreaRecordIdentity area) =>
        warp == AppliedAfterWarp && area == DestinationArea;
}

using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapSameMapWarpIdentity
{
    public OriginalMapSameMapWarpIdentity(
        ContentProfile profile,
        MapId map,
        string resourceId,
        int oneBasedRecordOrdinal)
    {
        if (profile != ContentProfile.PrivateLocal)
        {
            throw new ArgumentException(
                "An original same-map warp identity must remain PrivateLocal.",
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

public sealed record OriginalMapSameMapWarpDefinition
{
    public OriginalMapSameMapWarpDefinition(
        OriginalMapSameMapWarpIdentity identity,
        MapPosition trigger,
        MapPosition destination,
        byte opaqueFacing)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        if (trigger == destination)
        {
            throw new ArgumentException(
                "A same-map warp must relocate to a different position.",
                nameof(destination));
        }

        if (opaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(opaqueFacing));
        }

        OpaqueFacing = opaqueFacing;
    }

    public OriginalMapSameMapWarpIdentity Identity { get; }

    public MapPosition Trigger { get; }

    public MapPosition Destination { get; }

    public byte OpaqueFacing { get; }
}

public sealed class OriginalMapSameMapWarpCatalog
{
    private readonly ReadOnlyCollection<OriginalMapSameMapWarpDefinition> _records;

    public OriginalMapSameMapWarpCatalog(
        IEnumerable<OriginalMapSameMapWarpDefinition> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        List<OriginalMapSameMapWarpDefinition> copied = [];
        HashSet<int> ordinals = [];
        HashSet<MapPosition> triggers = [];
        MapId? map = null;
        string? resourceId = null;
        foreach (OriginalMapSameMapWarpDefinition record in records)
        {
            OriginalMapSameMapWarpDefinition admitted = record ?? throw new ArgumentException(
                "A same-map warp catalog cannot contain null records.",
                nameof(records));
            map ??= admitted.Identity.Map;
            resourceId ??= admitted.Identity.ResourceId;
            if (admitted.Identity.Profile != ContentProfile.PrivateLocal ||
                admitted.Identity.Map != map ||
                !string.Equals(
                    admitted.Identity.ResourceId,
                    resourceId,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Every same-map warp record must retain one private map and resource identity.",
                    nameof(records));
            }

            if (!ordinals.Add(admitted.Identity.OneBasedRecordOrdinal) ||
                !triggers.Add(admitted.Trigger))
            {
                throw new ArgumentException(
                    "Same-map warp record ordinals and trigger positions must be unique.",
                    nameof(records));
            }

            copied.Add(admitted);
        }

        if (copied.Count == 0)
        {
            throw new ArgumentException(
                "A same-map warp catalog requires at least one exact record.",
                nameof(records));
        }

        Map = map!;
        ResourceId = resourceId!;
        _records = copied.AsReadOnly();
    }

    public MapId Map { get; }

    public string ResourceId { get; }

    public IReadOnlyList<OriginalMapSameMapWarpDefinition> Records => _records;

    public OriginalMapSameMapWarpDefinition? Select(MapId map, MapPosition candidateTarget)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(candidateTarget);
        if (map != Map)
        {
            return null;
        }

        return _records.FirstOrDefault(record => record.Trigger == candidateTarget);
    }
}

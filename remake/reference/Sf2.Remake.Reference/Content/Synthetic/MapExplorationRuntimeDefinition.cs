using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record MapExplorationRuntimeDefinition
{
    public MapExplorationRuntimeDefinition(
        MapId map,
        WorkingMapLayout layout,
        SyntheticWalkabilityGrid walkability,
        MapAreaDescriptionSource areaDescriptions,
        MapSetupEventTable<ZoneEventRecord> zoneEvents)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        Walkability = walkability ?? throw new ArgumentNullException(nameof(walkability));
        AreaDescriptions = areaDescriptions ??
            throw new ArgumentNullException(nameof(areaDescriptions));
        ZoneEvents = zoneEvents ?? throw new ArgumentNullException(nameof(zoneEvents));
    }

    public MapId Map { get; }

    public WorkingMapLayout Layout { get; }

    public SyntheticWalkabilityGrid Walkability { get; }

    public MapAreaDescriptionSource AreaDescriptions { get; }

    public MapSetupEventTable<ZoneEventRecord> ZoneEvents { get; }

    public ExplorationMovementState CreateExplorationState(MapPosition position) =>
        new(Map, Layout, Walkability, position);
}

public sealed class MapExplorationRuntimeCatalog
{
    private readonly ReadOnlyCollection<MapExplorationRuntimeDefinition> _definitions;
    private readonly Dictionary<MapId, MapExplorationRuntimeDefinition> _byMap;

    public MapExplorationRuntimeCatalog(
        IEnumerable<MapExplorationRuntimeDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        List<MapExplorationRuntimeDefinition> copiedDefinitions = [];
        _byMap = [];
        foreach (MapExplorationRuntimeDefinition definition in definitions)
        {
            MapExplorationRuntimeDefinition admitted = definition ??
                throw new ArgumentException(
                    "Map exploration runtime definitions cannot contain null values.",
                    nameof(definitions));
            if (!_byMap.TryAdd(admitted.Map, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate map exploration runtime ID '{admitted.Map}'.",
                    nameof(definitions));
            }

            copiedDefinitions.Add(admitted);
        }

        if (copiedDefinitions.Count == 0)
        {
            throw new ArgumentException(
                "A map exploration runtime catalog requires at least one exact map definition.",
                nameof(definitions));
        }

        _definitions = copiedDefinitions.AsReadOnly();
    }

    public IReadOnlyList<MapExplorationRuntimeDefinition> Definitions => _definitions;

    public MapExplorationRuntimeDefinition GetRequired(MapId map)
    {
        ArgumentNullException.ThrowIfNull(map);
        if (!_byMap.TryGetValue(map, out MapExplorationRuntimeDefinition? definition))
        {
            throw new ArgumentException(
                $"Map '{map}' has no admitted exploration runtime.",
                nameof(map));
        }

        return definition;
    }
}

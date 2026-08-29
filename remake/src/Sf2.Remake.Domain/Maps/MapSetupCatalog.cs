using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public sealed record MapId
{
    public MapId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapSetupCatalogEntry
{
    public MapSetupCatalogEntry(MapId map, MapSetupRoute route)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Route = route ?? throw new ArgumentNullException(nameof(route));
    }

    public MapId Map { get; }

    public MapSetupRoute Route { get; }
}

public sealed class MapSetupCatalog
{
    private readonly ReadOnlyCollection<MapSetupCatalogEntry> _entries;
    private readonly Dictionary<MapId, MapSetupRoute> _routes;

    public MapSetupCatalog(IEnumerable<MapSetupCatalogEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<MapSetupCatalogEntry> copiedEntries = [];
        _routes = [];
        foreach (MapSetupCatalogEntry entry in entries)
        {
            MapSetupCatalogEntry admittedEntry = entry ?? throw new ArgumentException(
                "Catalog entries cannot contain null values.",
                nameof(entries));
            if (!_routes.TryAdd(admittedEntry.Map, admittedEntry.Route))
            {
                throw new ArgumentException(
                    $"Duplicate map ID '{admittedEntry.Map}'.",
                    nameof(entries));
            }

            copiedEntries.Add(admittedEntry);
        }

        _entries = copiedEntries.AsReadOnly();
    }

    public IReadOnlyList<MapSetupCatalogEntry> Entries => _entries;

    public MapSetupId Select(
        MapId map,
        MapSetupId voidSetup,
        Func<FlagId, bool> isFlagSet)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(voidSetup);
        ArgumentNullException.ThrowIfNull(isFlagSet);

        _routes.TryGetValue(map, out MapSetupRoute? route);
        return MapSetupSelector.Select(route, voidSetup, isFlagSet);
    }
}

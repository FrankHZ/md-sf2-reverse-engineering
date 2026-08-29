using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record MapFieldSearchContextId
{
    public MapFieldSearchContextId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapFieldSearchRequestId
{
    public MapFieldSearchRequestId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapFieldSearchResultId
{
    public MapFieldSearchResultId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapDiscoveryId
{
    public MapDiscoveryId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapFieldSearchDefinition
{
    public MapFieldSearchDefinition(
        MapFieldSearchContextId context,
        MapFieldSearchRequestId request,
        MapFieldSearchResultId result,
        MapDiscoveryId discovery,
        MapId map,
        MapPosition position,
        MapSetupId setup,
        EventTargetId zoneTarget,
        PresentationCueId requestCue,
        PresentationCueId discoveryCue)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        Setup = setup ?? throw new ArgumentNullException(nameof(setup));
        ZoneTarget = zoneTarget ?? throw new ArgumentNullException(nameof(zoneTarget));
        RequestCue = requestCue ?? throw new ArgumentNullException(nameof(requestCue));
        DiscoveryCue = discoveryCue ?? throw new ArgumentNullException(nameof(discoveryCue));
        if (RequestCue == DiscoveryCue)
        {
            throw new ArgumentException(
                "The field-search request and discovery cues must be distinct.",
                nameof(discoveryCue));
        }
    }

    public MapFieldSearchContextId Context { get; }

    public MapFieldSearchRequestId Request { get; }

    public MapFieldSearchResultId Result { get; }

    public MapDiscoveryId Discovery { get; }

    public MapId Map { get; }

    public MapPosition Position { get; }

    public MapSetupId Setup { get; }

    public EventTargetId ZoneTarget { get; }

    public PresentationCueId RequestCue { get; }

    public PresentationCueId DiscoveryCue { get; }
}

public sealed class MapFieldSearchCatalog
{
    private readonly ReadOnlyCollection<MapFieldSearchDefinition> _definitions;
    private readonly Dictionary<MapFieldSearchRequestId, MapFieldSearchDefinition> _byRequest;
    private readonly Dictionary<
        (MapId Map, MapPosition Position, MapSetupId Setup, EventTargetId ZoneTarget),
        MapFieldSearchDefinition> _bySelection;

    public MapFieldSearchCatalog(IEnumerable<MapFieldSearchDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        List<MapFieldSearchDefinition> copiedDefinitions = [];
        HashSet<MapFieldSearchContextId> contextIds = [];
        HashSet<MapFieldSearchResultId> resultIds = [];
        HashSet<MapDiscoveryId> discoveryIds = [];
        HashSet<PresentationCueId> cueIds = [];
        _byRequest = [];
        _bySelection = [];
        foreach (MapFieldSearchDefinition definition in definitions)
        {
            MapFieldSearchDefinition admitted = definition ?? throw new ArgumentException(
                "Field-search definitions cannot contain null values.",
                nameof(definitions));
            if (!contextIds.Add(admitted.Context) ||
                !_byRequest.TryAdd(admitted.Request, admitted) ||
                !resultIds.Add(admitted.Result) ||
                !discoveryIds.Add(admitted.Discovery) ||
                !cueIds.Add(admitted.RequestCue) ||
                !cueIds.Add(admitted.DiscoveryCue) ||
                !_bySelection.TryAdd(
                    (admitted.Map, admitted.Position, admitted.Setup, admitted.ZoneTarget),
                    admitted))
            {
                throw new ArgumentException(
                    "Field-search context, request, result, discovery, cue, and selected location identities must be unique.",
                    nameof(definitions));
            }

            copiedDefinitions.Add(admitted);
        }

        _definitions = copiedDefinitions.AsReadOnly();
    }

    public IReadOnlyList<MapFieldSearchDefinition> Definitions => _definitions;

    public MapFieldSearchDefinition? FindByRequest(MapFieldSearchRequestId request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _byRequest.TryGetValue(request, out MapFieldSearchDefinition? definition);
        return definition;
    }

    public MapFieldSearchDefinition? FindForSelection(
        MapId map,
        MapPosition position,
        MapSetupId setup,
        EventTargetId zoneTarget)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(setup);
        ArgumentNullException.ThrowIfNull(zoneTarget);
        _bySelection.TryGetValue(
            (map, position, setup, zoneTarget),
            out MapFieldSearchDefinition? definition);
        return definition;
    }
}

using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Items;

namespace Sf2.Remake.Application.Content;

public sealed record MapItemAcquisitionRequestId
{
    public MapItemAcquisitionRequestId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapItemAcquisitionResultId
{
    public MapItemAcquisitionResultId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapItemAcquisitionDefinition
{
    public MapItemAcquisitionDefinition(
        MapDiscoveryId discovery,
        MapItemAcquisitionRequestId request,
        MapItemAcquisitionResultId result,
        ItemId item,
        PresentationCueId requestCue,
        PresentationCueId acquiredCue)
    {
        Discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Item = item ?? throw new ArgumentNullException(nameof(item));
        RequestCue = requestCue ?? throw new ArgumentNullException(nameof(requestCue));
        AcquiredCue = acquiredCue ?? throw new ArgumentNullException(nameof(acquiredCue));
        if (RequestCue == AcquiredCue)
        {
            throw new ArgumentException(
                "The item-acquisition request and acquired cues must be distinct.",
                nameof(acquiredCue));
        }
    }

    public MapDiscoveryId Discovery { get; }

    public MapItemAcquisitionRequestId Request { get; }

    public MapItemAcquisitionResultId Result { get; }

    public ItemId Item { get; }

    public PresentationCueId RequestCue { get; }

    public PresentationCueId AcquiredCue { get; }
}

public sealed class MapItemAcquisitionCatalog
{
    private readonly ReadOnlyCollection<MapItemAcquisitionDefinition> _definitions;
    private readonly Dictionary<MapDiscoveryId, MapItemAcquisitionDefinition> _byDiscovery;
    private readonly Dictionary<MapItemAcquisitionRequestId, MapItemAcquisitionDefinition> _byRequest;

    public MapItemAcquisitionCatalog(IEnumerable<MapItemAcquisitionDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        List<MapItemAcquisitionDefinition> copiedDefinitions = [];
        HashSet<MapItemAcquisitionResultId> resultIds = [];
        HashSet<ItemId> itemIds = [];
        HashSet<PresentationCueId> cueIds = [];
        _byDiscovery = [];
        _byRequest = [];
        foreach (MapItemAcquisitionDefinition definition in definitions)
        {
            MapItemAcquisitionDefinition admitted = definition ?? throw new ArgumentException(
                "Item-acquisition definitions cannot contain null values.",
                nameof(definitions));
            if (!_byDiscovery.TryAdd(admitted.Discovery, admitted) ||
                !_byRequest.TryAdd(admitted.Request, admitted) ||
                !resultIds.Add(admitted.Result) ||
                !itemIds.Add(admitted.Item) ||
                !cueIds.Add(admitted.RequestCue) ||
                !cueIds.Add(admitted.AcquiredCue))
            {
                throw new ArgumentException(
                    "Item-acquisition discovery, request, result, item, and cue identities must be unique.",
                    nameof(definitions));
            }

            copiedDefinitions.Add(admitted);
        }

        _definitions = copiedDefinitions.AsReadOnly();
    }

    public IReadOnlyList<MapItemAcquisitionDefinition> Definitions => _definitions;

    public MapItemAcquisitionDefinition? FindByDiscovery(MapDiscoveryId discovery)
    {
        ArgumentNullException.ThrowIfNull(discovery);
        _byDiscovery.TryGetValue(discovery, out MapItemAcquisitionDefinition? definition);
        return definition;
    }

    public MapItemAcquisitionDefinition? FindByRequest(MapItemAcquisitionRequestId request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _byRequest.TryGetValue(request, out MapItemAcquisitionDefinition? definition);
        return definition;
    }
}

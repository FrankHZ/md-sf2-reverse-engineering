using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record MapEventRequestId
{
    public MapEventRequestId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record PresentationCueId
{
    public PresentationCueId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapEventRequestDefinition
{
    public MapEventRequestDefinition(
        MapEventRequestId request,
        EventTargetId zoneTarget,
        PresentationCueId cue)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ZoneTarget = zoneTarget ?? throw new ArgumentNullException(nameof(zoneTarget));
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
    }

    public MapEventRequestId Request { get; }

    public EventTargetId ZoneTarget { get; }

    public PresentationCueId Cue { get; }
}

public sealed class MapEventRequestCatalog
{
    private readonly ReadOnlyCollection<MapEventRequestDefinition> _definitions;
    private readonly Dictionary<EventTargetId, MapEventRequestDefinition> _byTarget;

    public MapEventRequestCatalog(IEnumerable<MapEventRequestDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        List<MapEventRequestDefinition> copiedDefinitions = [];
        HashSet<MapEventRequestId> requestIds = [];
        HashSet<PresentationCueId> cueIds = [];
        _byTarget = [];
        foreach (MapEventRequestDefinition definition in definitions)
        {
            MapEventRequestDefinition admitted = definition ?? throw new ArgumentException(
                "Event-request definitions cannot contain null values.",
                nameof(definitions));
            if (!requestIds.Add(admitted.Request))
            {
                throw new ArgumentException(
                    $"Duplicate event-request ID '{admitted.Request}'.",
                    nameof(definitions));
            }

            if (!cueIds.Add(admitted.Cue))
            {
                throw new ArgumentException(
                    $"Duplicate presentation-cue ID '{admitted.Cue}'.",
                    nameof(definitions));
            }

            if (!_byTarget.TryAdd(admitted.ZoneTarget, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate event-request target '{admitted.ZoneTarget}'.",
                    nameof(definitions));
            }

            copiedDefinitions.Add(admitted);
        }

        _definitions = copiedDefinitions.AsReadOnly();
    }

    public IReadOnlyList<MapEventRequestDefinition> Definitions => _definitions;

    public MapEventRequestDefinition? FindByTarget(EventTargetId target)
    {
        ArgumentNullException.ThrowIfNull(target);
        _byTarget.TryGetValue(target, out MapEventRequestDefinition? definition);
        return definition;
    }
}

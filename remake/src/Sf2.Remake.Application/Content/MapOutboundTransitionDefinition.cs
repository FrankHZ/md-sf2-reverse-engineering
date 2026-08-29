using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record MapOutboundTransitionRequestId
{
    public MapOutboundTransitionRequestId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapOutboundTransitionId
{
    public MapOutboundTransitionId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapOutboundTransitionDefinition
{
    public MapOutboundTransitionDefinition(
        MapOutboundTransitionRequestId request,
        MapOutboundTransitionId transition,
        EventTargetId zoneTarget,
        MapId sourceMap,
        MapPosition sourcePosition,
        MapSetupId sourceSetup,
        MapId destinationMap,
        MapPosition destinationPosition,
        MapSetupId destinationSetup,
        SemanticFacing destinationFacing,
        PresentationCueId cue)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Transition = transition ?? throw new ArgumentNullException(nameof(transition));
        ZoneTarget = zoneTarget ?? throw new ArgumentNullException(nameof(zoneTarget));
        SourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));
        SourcePosition = sourcePosition ?? throw new ArgumentNullException(nameof(sourcePosition));
        SourceSetup = sourceSetup ?? throw new ArgumentNullException(nameof(sourceSetup));
        DestinationMap = destinationMap ?? throw new ArgumentNullException(nameof(destinationMap));
        DestinationPosition = destinationPosition ??
            throw new ArgumentNullException(nameof(destinationPosition));
        DestinationSetup = destinationSetup ?? throw new ArgumentNullException(nameof(destinationSetup));
        if (!Enum.IsDefined(destinationFacing))
        {
            throw new ArgumentOutOfRangeException(nameof(destinationFacing));
        }

        if (SourceMap == DestinationMap)
        {
            throw new ArgumentException(
                "An outbound transition must change the live map.",
                nameof(destinationMap));
        }

        DestinationFacing = destinationFacing;
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
    }

    public MapOutboundTransitionRequestId Request { get; }

    public MapOutboundTransitionId Transition { get; }

    public EventTargetId ZoneTarget { get; }

    public MapId SourceMap { get; }

    public MapPosition SourcePosition { get; }

    public MapSetupId SourceSetup { get; }

    public MapId DestinationMap { get; }

    public MapPosition DestinationPosition { get; }

    public MapSetupId DestinationSetup { get; }

    public SemanticFacing DestinationFacing { get; }

    public PresentationCueId Cue { get; }
}

public sealed class MapOutboundTransitionCatalog
{
    private readonly ReadOnlyCollection<MapOutboundTransitionDefinition> _definitions;
    private readonly Dictionary<EventTargetId, MapOutboundTransitionDefinition> _byTarget;
    private readonly Dictionary<MapOutboundTransitionRequestId, MapOutboundTransitionDefinition>
        _byRequest;

    public MapOutboundTransitionCatalog(IEnumerable<MapOutboundTransitionDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        List<MapOutboundTransitionDefinition> copiedDefinitions = [];
        HashSet<MapOutboundTransitionId> transitionIds = [];
        HashSet<PresentationCueId> cueIds = [];
        _byTarget = [];
        _byRequest = [];
        foreach (MapOutboundTransitionDefinition definition in definitions)
        {
            MapOutboundTransitionDefinition admitted = definition ?? throw new ArgumentException(
                "Outbound-transition definitions cannot contain null values.",
                nameof(definitions));
            if (!_byRequest.TryAdd(admitted.Request, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate outbound-transition request ID '{admitted.Request}'.",
                    nameof(definitions));
            }

            if (!transitionIds.Add(admitted.Transition))
            {
                throw new ArgumentException(
                    $"Duplicate outbound-transition ID '{admitted.Transition}'.",
                    nameof(definitions));
            }

            if (!_byTarget.TryAdd(admitted.ZoneTarget, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate outbound-transition target '{admitted.ZoneTarget}'.",
                    nameof(definitions));
            }

            if (!cueIds.Add(admitted.Cue))
            {
                throw new ArgumentException(
                    $"Duplicate outbound-transition cue '{admitted.Cue}'.",
                    nameof(definitions));
            }

            copiedDefinitions.Add(admitted);
        }

        _definitions = copiedDefinitions.AsReadOnly();
    }

    public IReadOnlyList<MapOutboundTransitionDefinition> Definitions => _definitions;

    public MapOutboundTransitionDefinition? FindByTarget(EventTargetId target)
    {
        ArgumentNullException.ThrowIfNull(target);
        _byTarget.TryGetValue(target, out MapOutboundTransitionDefinition? definition);
        return definition;
    }

    public MapOutboundTransitionDefinition? FindByRequest(MapOutboundTransitionRequestId request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _byRequest.TryGetValue(request, out MapOutboundTransitionDefinition? definition);
        return definition;
    }
}

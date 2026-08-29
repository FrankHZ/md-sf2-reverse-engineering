using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record MapLocalTransitionRequestId
{
    public MapLocalTransitionRequestId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapLocalTransitionId
{
    public MapLocalTransitionId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record OpaqueMapOrientationId
{
    public OpaqueMapOrientationId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapLocalTransitionDefinition
{
    public MapLocalTransitionDefinition(
        MapLocalTransitionRequestId request,
        MapLocalTransitionId transition,
        EventTargetId zoneTarget,
        MapId sourceMap,
        MapPosition sourcePosition,
        MapSetupId sourceSetup,
        MapId destinationMap,
        MapPosition destinationPosition,
        OpaqueMapOrientationId destinationOrientation,
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
        DestinationOrientation = destinationOrientation ??
            throw new ArgumentNullException(nameof(destinationOrientation));
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
    }

    public MapLocalTransitionRequestId Request { get; }

    public MapLocalTransitionId Transition { get; }

    public EventTargetId ZoneTarget { get; }

    public MapId SourceMap { get; }

    public MapPosition SourcePosition { get; }

    public MapSetupId SourceSetup { get; }

    public MapId DestinationMap { get; }

    public MapPosition DestinationPosition { get; }

    public OpaqueMapOrientationId DestinationOrientation { get; }

    public PresentationCueId Cue { get; }
}

public sealed class MapLocalTransitionCatalog
{
    private readonly ReadOnlyCollection<MapLocalTransitionDefinition> _definitions;
    private readonly Dictionary<EventTargetId, MapLocalTransitionDefinition> _byTarget;
    private readonly Dictionary<MapLocalTransitionRequestId, MapLocalTransitionDefinition> _byRequest;

    public MapLocalTransitionCatalog(IEnumerable<MapLocalTransitionDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        List<MapLocalTransitionDefinition> copiedDefinitions = [];
        HashSet<MapLocalTransitionId> transitionIds = [];
        HashSet<PresentationCueId> cueIds = [];
        _byTarget = [];
        _byRequest = [];
        foreach (MapLocalTransitionDefinition definition in definitions)
        {
            MapLocalTransitionDefinition admitted = definition ?? throw new ArgumentException(
                "Local-transition definitions cannot contain null values.",
                nameof(definitions));
            if (!_byRequest.TryAdd(admitted.Request, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate local-transition request ID '{admitted.Request}'.",
                    nameof(definitions));
            }

            if (!transitionIds.Add(admitted.Transition))
            {
                throw new ArgumentException(
                    $"Duplicate local-transition ID '{admitted.Transition}'.",
                    nameof(definitions));
            }

            if (!_byTarget.TryAdd(admitted.ZoneTarget, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate local-transition target '{admitted.ZoneTarget}'.",
                    nameof(definitions));
            }

            if (!cueIds.Add(admitted.Cue))
            {
                throw new ArgumentException(
                    $"Duplicate local-transition cue '{admitted.Cue}'.",
                    nameof(definitions));
            }

            copiedDefinitions.Add(admitted);
        }

        _definitions = copiedDefinitions.AsReadOnly();
    }

    public IReadOnlyList<MapLocalTransitionDefinition> Definitions => _definitions;

    public MapLocalTransitionDefinition? FindByTarget(EventTargetId target)
    {
        ArgumentNullException.ThrowIfNull(target);
        _byTarget.TryGetValue(target, out MapLocalTransitionDefinition? definition);
        return definition;
    }

    public MapLocalTransitionDefinition? FindByRequest(MapLocalTransitionRequestId request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _byRequest.TryGetValue(request, out MapLocalTransitionDefinition? definition);
        return definition;
    }
}

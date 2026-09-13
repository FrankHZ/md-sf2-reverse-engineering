using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public enum SemanticFacing
{
    North,
    East,
    South,
    West,
}

public sealed record MapEntityId
{
    public MapEntityId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapEntityInteractionTargetId
{
    public MapEntityInteractionTargetId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapEntityInteractionRequestId
{
    public MapEntityInteractionRequestId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapEntityDefinition
{
    public MapEntityDefinition(
        MapEntityId entity,
        MapId map,
        MapPosition position,
        MapEntityInteractionTargetId interactionTarget)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        InteractionTarget = interactionTarget ??
            throw new ArgumentNullException(nameof(interactionTarget));
    }

    public MapEntityId Entity { get; }

    public MapId Map { get; }

    public MapPosition Position { get; }

    public MapEntityInteractionTargetId InteractionTarget { get; }
}

public sealed record MapEntityInteractionDefinition
{
    public MapEntityInteractionDefinition(
        MapEntityInteractionRequestId request,
        MapEntityInteractionTargetId target,
        PresentationCueId cue)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
    }

    public MapEntityInteractionRequestId Request { get; }

    public MapEntityInteractionTargetId Target { get; }

    public PresentationCueId Cue { get; }
}

public sealed class MapEntityInteractionCatalog
{
    private readonly ReadOnlyCollection<MapEntityDefinition> _entities;
    private readonly ReadOnlyCollection<MapEntityInteractionDefinition> _interactions;
    private readonly Dictionary<MapEntityId, MapEntityDefinition> _byEntity;
    private readonly Dictionary<(MapId Map, MapPosition Position), MapEntityDefinition> _byPosition;
    private readonly Dictionary<MapEntityInteractionTargetId, MapEntityInteractionDefinition>
        _byTarget;
    private readonly Dictionary<MapEntityInteractionRequestId, MapEntityInteractionDefinition>
        _byRequest;

    public MapEntityInteractionCatalog(
        IEnumerable<MapEntityDefinition> entities,
        IEnumerable<MapEntityInteractionDefinition> interactions)
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(interactions);

        List<MapEntityDefinition> copiedEntities = [];
        _byEntity = [];
        _byPosition = [];
        foreach (MapEntityDefinition definition in entities)
        {
            MapEntityDefinition admitted = definition ?? throw new ArgumentException(
                "Entity definitions cannot contain null values.",
                nameof(entities));
            if (!_byEntity.TryAdd(admitted.Entity, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate entity ID '{admitted.Entity}'.",
                    nameof(entities));
            }

            if (!_byPosition.TryAdd((admitted.Map, admitted.Position), admitted))
            {
                throw new ArgumentException(
                    $"Multiple entities cannot occupy '{admitted.Map}' at " +
                    $"({admitted.Position.X}, {admitted.Position.Y}).",
                    nameof(entities));
            }

            copiedEntities.Add(admitted);
        }

        List<MapEntityInteractionDefinition> copiedInteractions = [];
        HashSet<PresentationCueId> cueIds = [];
        _byTarget = [];
        _byRequest = [];
        foreach (MapEntityInteractionDefinition definition in interactions)
        {
            MapEntityInteractionDefinition admitted = definition ?? throw new ArgumentException(
                "Entity-interaction definitions cannot contain null values.",
                nameof(interactions));
            if (!_byRequest.TryAdd(admitted.Request, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate entity-interaction request ID '{admitted.Request}'.",
                    nameof(interactions));
            }

            if (!_byTarget.TryAdd(admitted.Target, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate entity-interaction target '{admitted.Target}'.",
                    nameof(interactions));
            }

            if (!cueIds.Add(admitted.Cue))
            {
                throw new ArgumentException(
                    $"Duplicate entity-interaction cue '{admitted.Cue}'.",
                    nameof(interactions));
            }

            copiedInteractions.Add(admitted);
        }

        HashSet<MapEntityInteractionTargetId> referencedTargets = copiedEntities
            .Select(entity => entity.InteractionTarget)
            .ToHashSet();
        if (referencedTargets.Count != copiedEntities.Count ||
            !referencedTargets.SetEquals(_byTarget.Keys))
        {
            throw new ArgumentException(
                "Every entity must reference one unique admitted interaction target, with no dangling definitions.",
                nameof(interactions));
        }

        _entities = copiedEntities.AsReadOnly();
        _interactions = copiedInteractions.AsReadOnly();
    }

    public IReadOnlyList<MapEntityDefinition> Entities => _entities;

    public IReadOnlyList<MapEntityInteractionDefinition> Interactions => _interactions;

    public MapEntityDefinition? FindEntityAt(MapId map, MapPosition position)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(position);
        _byPosition.TryGetValue((map, position), out MapEntityDefinition? definition);
        return definition;
    }

    public MapEntityDefinition? FindEntity(MapEntityId entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _byEntity.TryGetValue(entity, out MapEntityDefinition? definition);
        return definition;
    }

    public MapEntityInteractionDefinition? FindByTarget(MapEntityInteractionTargetId target)
    {
        ArgumentNullException.ThrowIfNull(target);
        _byTarget.TryGetValue(target, out MapEntityInteractionDefinition? definition);
        return definition;
    }

    public MapEntityInteractionDefinition? FindByRequest(MapEntityInteractionRequestId request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _byRequest.TryGetValue(request, out MapEntityInteractionDefinition? definition);
        return definition;
    }
}

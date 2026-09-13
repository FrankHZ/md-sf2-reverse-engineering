using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record MapEventEffectId
{
    public MapEventEffectId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record MapEventEffectDefinition
{
    public MapEventEffectDefinition(
        MapEventEffectId effect,
        MapEventRequestId request,
        FlagId flag,
        PresentationCueId cue)
    {
        Effect = effect ?? throw new ArgumentNullException(nameof(effect));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Flag = flag ?? throw new ArgumentNullException(nameof(flag));
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
    }

    public MapEventEffectId Effect { get; }

    public MapEventRequestId Request { get; }

    public FlagId Flag { get; }

    public PresentationCueId Cue { get; }
}

public sealed class MapEventEffectCatalog
{
    private readonly ReadOnlyCollection<MapEventEffectDefinition> _definitions;
    private readonly Dictionary<MapEventRequestId, MapEventEffectDefinition> _byRequest;

    public MapEventEffectCatalog(IEnumerable<MapEventEffectDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        List<MapEventEffectDefinition> copiedDefinitions = [];
        HashSet<MapEventEffectId> effectIds = [];
        HashSet<FlagId> flags = [];
        HashSet<PresentationCueId> cueIds = [];
        _byRequest = [];
        foreach (MapEventEffectDefinition definition in definitions)
        {
            MapEventEffectDefinition admitted = definition ?? throw new ArgumentException(
                "Event-effect definitions cannot contain null values.",
                nameof(definitions));
            if (!effectIds.Add(admitted.Effect))
            {
                throw new ArgumentException(
                    $"Duplicate event-effect ID '{admitted.Effect}'.",
                    nameof(definitions));
            }

            if (!_byRequest.TryAdd(admitted.Request, admitted))
            {
                throw new ArgumentException(
                    $"Duplicate event-effect request '{admitted.Request}'.",
                    nameof(definitions));
            }

            if (!flags.Add(admitted.Flag))
            {
                throw new ArgumentException(
                    $"Duplicate event-effect flag '{admitted.Flag}'.",
                    nameof(definitions));
            }

            if (!cueIds.Add(admitted.Cue))
            {
                throw new ArgumentException(
                    $"Duplicate event-effect cue '{admitted.Cue}'.",
                    nameof(definitions));
            }

            copiedDefinitions.Add(admitted);
        }

        _definitions = copiedDefinitions.AsReadOnly();
    }

    public IReadOnlyList<MapEventEffectDefinition> Definitions => _definitions;

    public MapEventEffectDefinition? FindByRequest(MapEventRequestId request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _byRequest.TryGetValue(request, out MapEventEffectDefinition? definition);
        return definition;
    }
}

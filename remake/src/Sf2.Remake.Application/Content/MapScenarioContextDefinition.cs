using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed class MapScenarioContextDefinition
{
    private readonly ReadOnlyCollection<FlagId> _initialSetFlags;
    private readonly HashSet<FlagId> _initialSetFlagLookup;

    public MapScenarioContextDefinition(
        MapSetupCatalog setupCatalog,
        MapSetupId voidSetup,
        IEnumerable<FlagId> initialSetFlags,
        MapAreaDescriptionSource areaDescriptions,
        MapSetupEventTable<ZoneEventRecord> zoneEvents,
        MapEventRequestCatalog eventRequests,
        MapEventEffectCatalog eventEffects)
    {
        SetupCatalog = setupCatalog ?? throw new ArgumentNullException(nameof(setupCatalog));
        VoidSetup = voidSetup ?? throw new ArgumentNullException(nameof(voidSetup));
        ArgumentNullException.ThrowIfNull(initialSetFlags);
        AreaDescriptions = areaDescriptions ??
            throw new ArgumentNullException(nameof(areaDescriptions));
        ZoneEvents = zoneEvents ?? throw new ArgumentNullException(nameof(zoneEvents));
        EventRequests = eventRequests ?? throw new ArgumentNullException(nameof(eventRequests));
        EventEffects = eventEffects ?? throw new ArgumentNullException(nameof(eventEffects));

        List<FlagId> copiedFlags = [];
        _initialSetFlagLookup = [];
        foreach (FlagId flag in initialSetFlags)
        {
            FlagId admittedFlag = flag ?? throw new ArgumentException(
                "Initial set flags cannot contain null values.",
                nameof(initialSetFlags));
            if (!_initialSetFlagLookup.Add(admittedFlag))
            {
                throw new ArgumentException(
                    $"Duplicate initial set flag '{admittedFlag}'.",
                    nameof(initialSetFlags));
            }

            copiedFlags.Add(admittedFlag);
        }

        _initialSetFlags = copiedFlags.AsReadOnly();

        HashSet<EventTargetId> specificTargets = ZoneEvents.Records
            .Where(record => !record.IsDefault)
            .Select(record => record.Target)
            .ToHashSet();
        HashSet<EventTargetId> defaultTargets = ZoneEvents.Records
            .Where(record => record.IsDefault)
            .Select(record => record.Target)
            .ToHashSet();
        foreach (MapEventRequestDefinition definition in EventRequests.Definitions)
        {
            if (!specificTargets.Contains(definition.ZoneTarget) ||
                defaultTargets.Contains(definition.ZoneTarget))
            {
                throw new ArgumentException(
                    $"Event request '{definition.Request}' must reference one non-default zone target.",
                    nameof(eventRequests));
            }
        }

        HashSet<MapEventRequestId> requestIds = EventRequests.Definitions
            .Select(definition => definition.Request)
            .ToHashSet();
        HashSet<PresentationCueId> requestCueIds = EventRequests.Definitions
            .Select(definition => definition.Cue)
            .ToHashSet();
        HashSet<FlagId> setupVariantFlags = SetupCatalog.Entries
            .SelectMany(entry => entry.Route.FlagAlternatives)
            .Select(alternative => alternative.Flag)
            .ToHashSet();
        foreach (MapEventEffectDefinition definition in EventEffects.Definitions)
        {
            if (!requestIds.Contains(definition.Request))
            {
                throw new ArgumentException(
                    $"Event effect '{definition.Effect}' references an unknown request.",
                    nameof(eventEffects));
            }

            if (!setupVariantFlags.Contains(definition.Flag) ||
                _initialSetFlagLookup.Contains(definition.Flag))
            {
                throw new ArgumentException(
                    $"Event effect '{definition.Effect}' must set one initially-clear setup-variant flag.",
                    nameof(eventEffects));
            }

            if (requestCueIds.Contains(definition.Cue))
            {
                throw new ArgumentException(
                    $"Event effect '{definition.Effect}' cannot reuse a request cue ID.",
                    nameof(eventEffects));
            }
        }

        foreach (MapEventRequestId request in requestIds)
        {
            if (EventEffects.FindByRequest(request) is null)
            {
                throw new ArgumentException(
                    $"Event request '{request}' requires one admitted synthetic effect.",
                    nameof(eventEffects));
            }
        }
    }

    public MapSetupCatalog SetupCatalog { get; }

    public MapSetupId VoidSetup { get; }

    public IReadOnlyList<FlagId> InitialSetFlags => _initialSetFlags;

    public MapAreaDescriptionSource AreaDescriptions { get; }

    public MapSetupEventTable<ZoneEventRecord> ZoneEvents { get; }

    public MapEventRequestCatalog EventRequests { get; }

    public MapEventEffectCatalog EventEffects { get; }

    public bool IsInitiallySet(FlagId flag)
    {
        ArgumentNullException.ThrowIfNull(flag);
        return _initialSetFlagLookup.Contains(flag);
    }
}

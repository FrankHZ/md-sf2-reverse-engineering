using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed class MapScenarioContextDefinition
{
    private readonly ReadOnlyCollection<FlagId> _setFlags;
    private readonly HashSet<FlagId> _setFlagLookup;

    public MapScenarioContextDefinition(
        MapSetupCatalog setupCatalog,
        MapSetupId voidSetup,
        IEnumerable<FlagId> setFlags,
        MapAreaDescriptionSource areaDescriptions,
        MapSetupEventTable<ZoneEventRecord> zoneEvents,
        MapEventRequestCatalog eventRequests)
    {
        SetupCatalog = setupCatalog ?? throw new ArgumentNullException(nameof(setupCatalog));
        VoidSetup = voidSetup ?? throw new ArgumentNullException(nameof(voidSetup));
        ArgumentNullException.ThrowIfNull(setFlags);
        AreaDescriptions = areaDescriptions ??
            throw new ArgumentNullException(nameof(areaDescriptions));
        ZoneEvents = zoneEvents ?? throw new ArgumentNullException(nameof(zoneEvents));
        EventRequests = eventRequests ?? throw new ArgumentNullException(nameof(eventRequests));

        List<FlagId> copiedFlags = [];
        _setFlagLookup = [];
        foreach (FlagId flag in setFlags)
        {
            FlagId admittedFlag = flag ?? throw new ArgumentException(
                "Set flags cannot contain null values.",
                nameof(setFlags));
            if (!_setFlagLookup.Add(admittedFlag))
            {
                throw new ArgumentException(
                    $"Duplicate set flag '{admittedFlag}'.",
                    nameof(setFlags));
            }

            copiedFlags.Add(admittedFlag);
        }

        _setFlags = copiedFlags.AsReadOnly();

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
    }

    public MapSetupCatalog SetupCatalog { get; }

    public MapSetupId VoidSetup { get; }

    public IReadOnlyList<FlagId> SetFlags => _setFlags;

    public MapAreaDescriptionSource AreaDescriptions { get; }

    public MapSetupEventTable<ZoneEventRecord> ZoneEvents { get; }

    public MapEventRequestCatalog EventRequests { get; }

    public bool IsFlagSet(FlagId flag)
    {
        ArgumentNullException.ThrowIfNull(flag);
        return _setFlagLookup.Contains(flag);
    }
}

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
        MapEventEffectCatalog eventEffects,
        MapLocalTransitionCatalog localTransitions,
        SemanticFacing initialFacing,
        MapEntityInteractionCatalog entityInteractions,
        MapDialogueCatalog dialogues)
    {
        SetupCatalog = setupCatalog ?? throw new ArgumentNullException(nameof(setupCatalog));
        VoidSetup = voidSetup ?? throw new ArgumentNullException(nameof(voidSetup));
        ArgumentNullException.ThrowIfNull(initialSetFlags);
        AreaDescriptions = areaDescriptions ??
            throw new ArgumentNullException(nameof(areaDescriptions));
        ZoneEvents = zoneEvents ?? throw new ArgumentNullException(nameof(zoneEvents));
        EventRequests = eventRequests ?? throw new ArgumentNullException(nameof(eventRequests));
        EventEffects = eventEffects ?? throw new ArgumentNullException(nameof(eventEffects));
        LocalTransitions = localTransitions ??
            throw new ArgumentNullException(nameof(localTransitions));
        if (!Enum.IsDefined(initialFacing))
        {
            throw new ArgumentOutOfRangeException(nameof(initialFacing));
        }

        InitialFacing = initialFacing;
        EntityInteractions = entityInteractions ??
            throw new ArgumentNullException(nameof(entityInteractions));
        Dialogues = dialogues ?? throw new ArgumentNullException(nameof(dialogues));

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

        HashSet<PresentationCueId> effectCueIds = EventEffects.Definitions
            .Select(definition => definition.Cue)
            .ToHashSet();
        HashSet<MapId> setupMaps = SetupCatalog.Entries
            .Select(entry => entry.Map)
            .ToHashSet();
        foreach (MapLocalTransitionDefinition definition in LocalTransitions.Definitions)
        {
            List<ZoneEventRecord> sourceRecords = ZoneEvents.Records
                .Where(record => !record.IsDefault && record.Target == definition.ZoneTarget)
                .ToList();
            if (sourceRecords.Count != 1 ||
                sourceRecords[0].X.ExactValue is not byte sourceX ||
                sourceRecords[0].Y.ExactValue is not byte sourceY ||
                sourceX != definition.SourcePosition.X ||
                sourceY != definition.SourcePosition.Y ||
                defaultTargets.Contains(definition.ZoneTarget))
            {
                throw new ArgumentException(
                    $"Local transition '{definition.Transition}' must reference one exact non-default source zone.",
                    nameof(localTransitions));
            }

            if (EventRequests.FindByTarget(definition.ZoneTarget) is not null)
            {
                throw new ArgumentException(
                    $"Local transition '{definition.Transition}' cannot reuse an event-effect target.",
                    nameof(localTransitions));
            }

            if (!setupMaps.Contains(definition.SourceMap) ||
                !setupMaps.Contains(definition.DestinationMap))
            {
                throw new ArgumentException(
                    $"Local transition '{definition.Transition}' references an unadmitted map.",
                    nameof(localTransitions));
            }

            MapSetupCatalogEntry sourceSetupEntry = SetupCatalog.Entries.Single(
                entry => entry.Map == definition.SourceMap);
            bool ownsSourceSetup = sourceSetupEntry.Route.DefaultSetup == definition.SourceSetup ||
                sourceSetupEntry.Route.FlagAlternatives.Any(
                    alternative => alternative.Setup == definition.SourceSetup);
            if (!ownsSourceSetup)
            {
                throw new ArgumentException(
                    $"Local transition '{definition.Transition}' references an unadmitted source setup.",
                    nameof(localTransitions));
            }

            if (requestCueIds.Contains(definition.Cue) || effectCueIds.Contains(definition.Cue))
            {
                throw new ArgumentException(
                    $"Local transition '{definition.Transition}' cannot reuse an event cue ID.",
                    nameof(localTransitions));
            }
        }

        HashSet<PresentationCueId> transitionCueIds = LocalTransitions.Definitions
            .Select(definition => definition.Cue)
            .ToHashSet();
        foreach (MapEntityInteractionDefinition definition in EntityInteractions.Interactions)
        {
            if (requestCueIds.Contains(definition.Cue) ||
                effectCueIds.Contains(definition.Cue) ||
                transitionCueIds.Contains(definition.Cue))
            {
                throw new ArgumentException(
                    $"Entity interaction '{definition.Request}' cannot reuse another cue ID.",
                    nameof(entityInteractions));
            }
        }

        foreach (MapEntityDefinition entity in EntityInteractions.Entities)
        {
            if (!setupMaps.Contains(entity.Map))
            {
                throw new ArgumentException(
                    $"Entity '{entity.Entity}' references an unadmitted map.",
                    nameof(entityInteractions));
            }
        }

        HashSet<MapEntityInteractionTargetId> interactionTargets = EntityInteractions.Interactions
            .Select(definition => definition.Target)
            .ToHashSet();
        HashSet<MapEntityInteractionTargetId> dialogueTargets = Dialogues.Definitions
            .Select(definition => definition.InteractionTarget)
            .ToHashSet();
        if (!interactionTargets.SetEquals(dialogueTargets))
        {
            throw new ArgumentException(
                "Every admitted entity-interaction target requires one exact dialogue definition, with no dangling dialogues.",
                nameof(dialogues));
        }

        HashSet<PresentationCueId> occupiedCueIds = requestCueIds
            .Concat(effectCueIds)
            .Concat(transitionCueIds)
            .Concat(EntityInteractions.Interactions.Select(definition => definition.Cue))
            .ToHashSet();
        foreach (MapDialogueDefinition dialogue in Dialogues.Definitions)
        {
            foreach (PresentationCueId cue in dialogue.Lines
                         .Select(line => line.Cue)
                         .Append(dialogue.CloseCue))
            {
                if (!occupiedCueIds.Add(cue))
                {
                    throw new ArgumentException(
                        $"Dialogue '{dialogue.Dialogue}' cannot reuse another presentation cue ID.",
                        nameof(dialogues));
                }
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

    public MapLocalTransitionCatalog LocalTransitions { get; }

    public SemanticFacing InitialFacing { get; }

    public MapEntityInteractionCatalog EntityInteractions { get; }

    public MapDialogueCatalog Dialogues { get; }

    public bool IsInitiallySet(FlagId flag)
    {
        ArgumentNullException.ThrowIfNull(flag);
        return _initialSetFlagLookup.Contains(flag);
    }
}

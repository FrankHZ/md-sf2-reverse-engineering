using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed class MapScenarioContextDefinition
{
    private readonly ReadOnlyCollection<FlagId> _initialSetFlags;
    private readonly HashSet<FlagId> _initialSetFlagLookup;

    public MapScenarioContextDefinition(
        MapExplorationRuntimeCatalog mapRuntimes,
        MapSetupCatalog setupCatalog,
        MapSetupId voidSetup,
        IEnumerable<FlagId> initialSetFlags,
        MapEventRequestCatalog eventRequests,
        MapEventEffectCatalog eventEffects,
        MapLocalTransitionCatalog localTransitions,
        SemanticFacing initialFacing,
        MapEntityInteractionCatalog entityInteractions,
        MapDialogueCatalog dialogues,
        MapFieldSearchCatalog fieldSearches,
        MapItemAcquisitionCatalog itemAcquisitions,
        MapOutboundTransitionCatalog? outboundTransitions = null,
        PublicSyntheticBattleCatalog? publicSyntheticBattles = null)
    {
        MapRuntimes = mapRuntimes ?? throw new ArgumentNullException(nameof(mapRuntimes));
        SetupCatalog = setupCatalog ?? throw new ArgumentNullException(nameof(setupCatalog));
        VoidSetup = voidSetup ?? throw new ArgumentNullException(nameof(voidSetup));
        ArgumentNullException.ThrowIfNull(initialSetFlags);
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
        FieldSearches = fieldSearches ?? throw new ArgumentNullException(nameof(fieldSearches));
        ItemAcquisitions = itemAcquisitions ??
            throw new ArgumentNullException(nameof(itemAcquisitions));
        OutboundTransitions = outboundTransitions ?? new MapOutboundTransitionCatalog([]);
        PublicSyntheticBattles = publicSyntheticBattles ??
            new PublicSyntheticBattleCatalog([]);

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

        HashSet<MapId> runtimeMaps = MapRuntimes.Definitions
            .Select(definition => definition.Map)
            .ToHashSet();
        HashSet<MapId> setupMaps = SetupCatalog.Entries
            .Select(entry => entry.Map)
            .ToHashSet();
        if (!runtimeMaps.SetEquals(setupMaps))
        {
            throw new ArgumentException(
                "Map exploration runtime and setup catalogs must own the same exact map IDs.",
                nameof(setupCatalog));
        }

        HashSet<EventTargetId> specificTargets = MapRuntimes.Definitions
            .SelectMany(definition => definition.ZoneEvents.Records)
            .Where(record => !record.IsDefault)
            .Select(record => record.Target)
            .ToHashSet();
        HashSet<EventTargetId> defaultTargets = MapRuntimes.Definitions
            .SelectMany(definition => definition.ZoneEvents.Records)
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
        foreach (MapLocalTransitionDefinition definition in LocalTransitions.Definitions)
        {
            if (definition.SourceMap != definition.DestinationMap)
            {
                throw new ArgumentException(
                    $"Local transition '{definition.Transition}' must remain within one map runtime.",
                    nameof(localTransitions));
            }

            MapExplorationRuntimeDefinition sourceRuntime =
                MapRuntimes.GetRequired(definition.SourceMap);
            List<ZoneEventRecord> sourceRecords = sourceRuntime.ZoneEvents.Records
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
        HashSet<EventTargetId> localTransitionTargets = LocalTransitions.Definitions
            .Select(definition => definition.ZoneTarget)
            .ToHashSet();
        foreach (MapOutboundTransitionDefinition definition in OutboundTransitions.Definitions)
        {
            MapExplorationRuntimeDefinition sourceRuntime =
                MapRuntimes.GetRequired(definition.SourceMap);
            MapExplorationRuntimeDefinition destinationRuntime =
                MapRuntimes.GetRequired(definition.DestinationMap);
            List<ZoneEventRecord> sourceRecords = sourceRuntime.ZoneEvents.Records
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
                    $"Outbound transition '{definition.Transition}' must reference one exact non-default source zone.",
                    nameof(outboundTransitions));
            }

            if (EventRequests.FindByTarget(definition.ZoneTarget) is not null ||
                localTransitionTargets.Contains(definition.ZoneTarget))
            {
                throw new ArgumentException(
                    $"Outbound transition '{definition.Transition}' cannot reuse another admitted target.",
                    nameof(outboundTransitions));
            }

            MapSetupCatalogEntry sourceSetupEntry = SetupCatalog.Entries.Single(
                entry => entry.Map == definition.SourceMap);
            MapSetupCatalogEntry destinationSetupEntry = SetupCatalog.Entries.Single(
                entry => entry.Map == definition.DestinationMap);
            bool ownsSourceSetup = OwnsSetup(sourceSetupEntry, definition.SourceSetup);
            bool ownsDestinationSetup =
                OwnsSetup(destinationSetupEntry, definition.DestinationSetup);
            MapSetupId selectedDestinationSetup = SetupCatalog.Select(
                definition.DestinationMap,
                VoidSetup,
                IsInitiallySet);
            if (!ownsSourceSetup ||
                !ownsDestinationSetup ||
                selectedDestinationSetup != definition.DestinationSetup ||
                !sourceRuntime.Walkability.IsPassable(definition.SourcePosition) ||
                !destinationRuntime.Walkability.IsPassable(definition.DestinationPosition))
            {
                throw new ArgumentException(
                    $"Outbound transition '{definition.Transition}' must reference exact admitted source and destination runtime state.",
                    nameof(outboundTransitions));
            }

            if (requestCueIds.Contains(definition.Cue) ||
                effectCueIds.Contains(definition.Cue) ||
                transitionCueIds.Contains(definition.Cue))
            {
                throw new ArgumentException(
                    $"Outbound transition '{definition.Transition}' cannot reuse another cue ID.",
                    nameof(outboundTransitions));
            }
        }

        HashSet<PresentationCueId> outboundTransitionCueIds = OutboundTransitions.Definitions
            .Select(definition => definition.Cue)
            .ToHashSet();
        foreach (MapEntityInteractionDefinition definition in EntityInteractions.Interactions)
        {
            if (requestCueIds.Contains(definition.Cue) ||
                effectCueIds.Contains(definition.Cue) ||
                transitionCueIds.Contains(definition.Cue) ||
                outboundTransitionCueIds.Contains(definition.Cue))
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
            .Concat(outboundTransitionCueIds)
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

        foreach (MapFieldSearchDefinition search in FieldSearches.Definitions)
        {
            if (!setupMaps.Contains(search.Map))
            {
                throw new ArgumentException(
                    $"Field-search context '{search.Context}' references an unadmitted map.",
                    nameof(fieldSearches));
            }

            MapSetupCatalogEntry setupEntry = SetupCatalog.Entries.Single(
                entry => entry.Map == search.Map);
            bool ownsSetup = setupEntry.Route.DefaultSetup == search.Setup ||
                setupEntry.Route.FlagAlternatives.Any(
                    alternative => alternative.Setup == search.Setup);
            MapExplorationRuntimeDefinition searchRuntime =
                MapRuntimes.GetRequired(search.Map);
            ZoneEventSelection selectedZone = MapSetupEventSelector.Select(
                searchRuntime.ZoneEvents,
                new ZoneEventQuery(
                    checked((byte)search.Position.X),
                    checked((byte)search.Position.Y)));
            if (!ownsSetup || selectedZone.Target != search.ZoneTarget)
            {
                throw new ArgumentException(
                    $"Field-search context '{search.Context}' must reference one exact admitted setup and selected zone.",
                    nameof(fieldSearches));
            }

            if (!occupiedCueIds.Add(search.RequestCue) ||
                !occupiedCueIds.Add(search.DiscoveryCue))
            {
                throw new ArgumentException(
                    $"Field-search context '{search.Context}' cannot reuse another presentation cue ID.",
                    nameof(fieldSearches));
            }
        }

        foreach (MapItemAcquisitionDefinition acquisition in ItemAcquisitions.Definitions)
        {
            MapFieldSearchDefinition? sourceSearch = FieldSearches.Definitions.SingleOrDefault(
                search => search.Discovery == acquisition.Discovery);
            if (sourceSearch is null)
            {
                throw new ArgumentException(
                    $"Item acquisition '{acquisition.Request}' references an unknown field-search discovery.",
                    nameof(itemAcquisitions));
            }

            if (!occupiedCueIds.Add(acquisition.RequestCue) ||
                !occupiedCueIds.Add(acquisition.AcquiredCue))
            {
                throw new ArgumentException(
                    $"Item acquisition '{acquisition.Request}' cannot reuse another presentation cue ID.",
                    nameof(itemAcquisitions));
            }
        }

        HashSet<EventTargetId> occupiedTargets = EventRequests.Definitions
            .Select(definition => definition.ZoneTarget)
            .Concat(localTransitionTargets)
            .Concat(OutboundTransitions.Definitions.Select(definition => definition.ZoneTarget))
            .ToHashSet();
        foreach (PublicSyntheticBattleDefinition battle in PublicSyntheticBattles.Definitions)
        {
            MapExplorationRuntimeDefinition sourceRuntime =
                MapRuntimes.GetRequired(battle.SourceMap);
            MapExplorationRuntimeDefinition returnRuntime =
                MapRuntimes.GetRequired(battle.ReturnMap);
            List<ZoneEventRecord> sourceRecords = sourceRuntime.ZoneEvents.Records
                .Where(record => !record.IsDefault && record.Target == battle.SourceZoneTarget)
                .ToList();
            MapSetupCatalogEntry sourceSetupEntry = SetupCatalog.Entries.Single(
                entry => entry.Map == battle.SourceMap);
            MapSetupCatalogEntry returnSetupEntry = SetupCatalog.Entries.Single(
                entry => entry.Map == battle.ReturnMap);
            if (sourceRecords.Count != 1 ||
                sourceRecords[0].X.ExactValue is not byte sourceX ||
                sourceRecords[0].Y.ExactValue is not byte sourceY ||
                sourceX != battle.SourcePosition.X ||
                sourceY != battle.SourcePosition.Y ||
                defaultTargets.Contains(battle.SourceZoneTarget) ||
                !occupiedTargets.Add(battle.SourceZoneTarget) ||
                !OwnsSetup(sourceSetupEntry, battle.SourceSetup) ||
                !OwnsSetup(returnSetupEntry, battle.ReturnSetup) ||
                !sourceRuntime.Walkability.IsPassable(battle.SourcePosition) ||
                !returnRuntime.Walkability.IsPassable(battle.ReturnPosition))
            {
                throw new ArgumentException(
                    $"Public-synthetic battle '{battle.Rules.Battle}' requires exact admitted source and return state.",
                    nameof(publicSyntheticBattles));
            }

            foreach (PresentationCueId cue in battle.Cues)
            {
                if (!occupiedCueIds.Add(cue))
                {
                    throw new ArgumentException(
                        $"Public-synthetic battle '{battle.Rules.Battle}' cannot reuse a presentation cue ID.",
                        nameof(publicSyntheticBattles));
                }
            }
        }
    }

    public MapExplorationRuntimeCatalog MapRuntimes { get; }

    public MapSetupCatalog SetupCatalog { get; }

    public MapSetupId VoidSetup { get; }

    public IReadOnlyList<FlagId> InitialSetFlags => _initialSetFlags;

    public MapEventRequestCatalog EventRequests { get; }

    public MapEventEffectCatalog EventEffects { get; }

    public MapLocalTransitionCatalog LocalTransitions { get; }

    public SemanticFacing InitialFacing { get; }

    public MapEntityInteractionCatalog EntityInteractions { get; }

    public MapDialogueCatalog Dialogues { get; }

    public MapFieldSearchCatalog FieldSearches { get; }

    public MapItemAcquisitionCatalog ItemAcquisitions { get; }

    public MapOutboundTransitionCatalog OutboundTransitions { get; }

    public PublicSyntheticBattleCatalog PublicSyntheticBattles { get; }

    public bool IsInitiallySet(FlagId flag)
    {
        ArgumentNullException.ThrowIfNull(flag);
        return _initialSetFlagLookup.Contains(flag);
    }

    private static bool OwnsSetup(MapSetupCatalogEntry entry, MapSetupId setup) =>
        entry.Route.DefaultSetup == setup ||
        entry.Route.FlagAlternatives.Any(alternative => alternative.Setup == setup);
}

using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapReturnEntity(int Id, int PhysicalSlot, bool IsFollower,
    OriginalMapEntityDefinition? SourceRecord, string ActscriptIdentity, MapPosition DeclarationPosition,
    MapPosition? Position, MapPosition? TargetPosition, byte Facing, byte MapSprite,
    bool Dead, bool Hidden = false, bool MovedOutOfMap = false)
{
    // No follower/NPC actscript is run. NPC generated-script internals are not projected as facts.
    public int? VelocityXUnits => IsFollower || Id == 0 ? 0 : null;
    public int? VelocityYUnits => VelocityXUnits;
    public int? TravelXUnits => VelocityXUnits;
    public int? TravelYUnits => VelocityXUnits;
}

public sealed class PrivateOriginalMapReturnArrivalSnapshot
{
    internal PrivateOriginalMapReturnArrivalSnapshot(PrivateOriginalBattle01SessionSnapshot before,
        Battle01EntryParty party, OriginalMapReturnEntryLoadDefinition load, WorkingMapLayout layout,
        MapBlockCopyLifecycleResult roof, IEnumerable<PrivateOriginalMapReturnEntity> entities,
        IReadOnlyDictionary<int, bool> flags)
    {
        Before = before; Party = party; LoadDefinition = load; WorkingLayout = layout; RoofClear = roof;
        Entities = Array.AsReadOnly(entities.ToArray());
        Flags = new ReadOnlyDictionary<int, bool>(flags.ToDictionary(pair => pair.Key, pair => pair.Value));
        ValidateEntities(Entities, load.Runtime.EntityPopulation, before.DefeatReturn!.DestinationPosition);
        Locomotion = PrivateOriginalMapPlayerLocomotionSnapshot.EnterGranseal(before.DefeatReturn);
    }
    public PrivateOriginalBattle01SessionSnapshot Before { get; }
    public Battle01EntryParty Party { get; }
    public OriginalMapReturnEntryLoadDefinition LoadDefinition { get; }
    public OriginalMapExplorationRuntimeDefinition CurrentRuntime => LoadDefinition.Runtime;
    public OriginalMapImportDefinition Definition => Before.SourceSnapshot.Definition;
    public MapId Map => CurrentRuntime.Map;
    public WorkingMapLayout WorkingLayout { get; }
    public MapPosition PlayerPosition => Before.DefeatReturn!.DestinationPosition;
    public byte PlayerOpaqueFacing => Before.DefeatReturn!.DestinationOpaqueFacing;
    public OriginalMapAreaDefinition CurrentAreaDefinition => LoadDefinition.Area;
    public MapBlockCopyLifecycleResult RoofClear { get; }
    public IReadOnlyList<PrivateOriginalMapReturnEntity> Entities { get; }
    public IReadOnlyDictionary<int, bool> Flags { get; }
    public PrivateOriginalMapPlayerLocomotionSnapshot Locomotion { get; }
    public bool ExplorationInputAvailable => false;
    public string EntityPolicy => "Declared positions frozen; follower/NPC scripts are not running.";
    public bool Entity142Hidden => Entities.Single(entity => entity.Id == 142).Hidden;
    public bool Entity142MovedOut => Entities.Single(entity => entity.Id == 142).MovedOutOfMap;

    internal static PrivateOriginalMapReturnEntity[] DeclareEntities(OriginalMapEntityPopulation population,
        Battle01EntryParty party, MapPosition destination, byte facing)
    {
        if (!OriginalMapRuntimeAdmission.HasExactAcceptedEntityPopulation(population))
            throw new ArgumentException("Retain the exact admitted default Map3 population.", "arrival.population");
        var entities = new List<PrivateOriginalMapReturnEntity>();
        for (int id = 1; id <= 2; id++)
        {
            var ally = party.Slots[id];
            entities.Add(new(id, id, true, population.Records[id - 1], "eas_Follower" + id,
                destination, destination, destination, facing, ally.HpCurrent == 0 ? (byte)190 : (byte)id, ally.HpCurrent == 0));
        }
        foreach (var row in population.Records)
        {
            if (row.MapSprite is 1 or 2) continue; // Already declared by F66, preserving each source row as provenance.
            int ordinal = row.Identity.OneBasedRecordOrdinal;
            entities.Add(new(128 + ordinal - 3, ordinal, false, row,
                row.Identity.ResourceId + "[" + ordinal + "].tail", row.Position, row.Position,
                row.Position, row.OpaqueFacing, row.MapSprite, false));
        }
        entities.Add(new(0, 0, false, null, "eas_Idle", destination, destination, destination, facing, 0, false));
        return entities.ToArray();
    }

    internal static void ValidateEntities(IReadOnlyList<PrivateOriginalMapReturnEntity> entities,
        OriginalMapEntityPopulation population, MapPosition destination)
    {
        if (entities.Count != population.Records.Count + 1 || entities.Select(e => e.Id).Distinct().Count() != entities.Count ||
            entities.Select(e => e.PhysicalSlot).Distinct().Count() != entities.Count)
            throw new ArgumentException("Retain distinct admitted declarations, without duplicate setup rows.", "arrival.entities");
        foreach (var entity in entities)
        {
            if (entity.Id is >= 0 and <= 2)
            {
                if (entity.PhysicalSlot != entity.Id || entity.IsFollower != (entity.Id != 0) ||
                    entity.Position != destination || entity.TargetPosition != destination || entity.DeclarationPosition != destination ||
                    entity.Facing != 1 || entity.Hidden || entity.MovedOutOfMap ||
                    entity.Dead != (entity.Id == 2) || entity.MapSprite != (entity.Id == 2 ? 190 : entity.Id) ||
                    entity.ActscriptIdentity != (entity.Id == 0 ? "eas_Idle" : "eas_Follower" + entity.Id) ||
                    (entity.Id == 0 ? entity.SourceRecord is not null :
                        !ReferenceEquals(entity.SourceRecord, population.Records[entity.Id - 1])))
                    throw new ArgumentException("Only the authenticated player/F66 follower group may share the entry cell.", "arrival.followers");
            }
            else
            {
                var row = entity.SourceRecord;
                int ordinal = row?.Identity.OneBasedRecordOrdinal ?? 0;
                bool moved = entity.Id == 142;
                if (entity.IsFollower || row is null || ordinal < 3 || ordinal > population.Records.Count ||
                    !ReferenceEquals(row, population.Records[ordinal - 1]) || entity.Id != 128 + ordinal - 3 ||
                    entity.PhysicalSlot != ordinal || entity.DeclarationPosition != row.Position ||
                    entity.Position != (moved ? null : row.Position) || entity.TargetPosition != entity.Position ||
                    entity.Facing != row.OpaqueFacing || entity.MapSprite != row.MapSprite || entity.Dead ||
                    entity.Hidden != moved || entity.MovedOutOfMap != moved ||
                    entity.ActscriptIdentity != row.Identity.ResourceId + "[" + ordinal + "].tail" ||
                    entity.Position == destination)
                    throw new ArgumentException("Retain each admitted NPC declaration and the explicit entity142 init effects.", "arrival.occupancy");
            }
        }
        if (!entities.Where(e => e.IsFollower).Select(e => e.Id).Order().SequenceEqual(new[] { 1, 2 }) ||
            entities.Count(e => e.Id == 0) != 1 ||
            !entities.Single(e => e.Id == 2).Dead || entities.Single(e => e.Id == 2).MapSprite != 190)
            throw new ArgumentException("The declared dead Chester follower must retain BLUE_FLAME identity.", "arrival.followers");
    }
}

public abstract record PrivateOriginalBattle01ExplorationEntryResult;
public sealed record PrivateOriginalBattle01ExplorationEntered(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01ExplorationEntryResult;
public sealed record PrivateOriginalBattle01ExplorationEntryRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01ExplorationEntryResult;

public sealed partial class GameSession
{
    internal static OriginalBattle01StartupDiagnostic? GetArrivalSourceDiagnostic(PrivateOriginalMapSessionSnapshot source,
        OriginalMapReturnEntryLoadDefinition? load)
    {
        if (load is null || !ReferenceEquals(load, source.Definition.ReturnEntryLoad) ||
            !OriginalMapRuntimeAdmission.HasExactAcceptedReturnEntryLoad(load, source.Definition.RuntimeCatalog))
            return new("arrival.load", "The exact catalog-owned Granseal return-load definition is required.");
        if (source.Map != new MapId("map40") || source.MiddleTowerGuard?.ProgramStoryFlag1Set != true ||
            source.Sarah?.IsMessengerFollowerReady != true || source.Entity142?.Flag602Set != true ||
            source.MessengerAcceptance is not { Accepted: true, Flag66Set: true, Flag603Set: true } ||
            source.Zone601?.Flag601Set != true || source.PalaceFirstVisit is null ||
            source.AstralAcceptance?.ProgramFlag608Set != true || source.CastleGate?.Opened != true)
            return new("arrival.story", "Retain the exact accepted palace/guard/messenger story chain.");
        return null;
    }

    public PrivateOriginalBattle01ExplorationEntryResult EnterPrivateOriginalBattle01Exploration(
        PrivateOriginalBattle01SessionSnapshot? expected)
    {
        var current = PrivateOriginalBattle01;
        if (current is null || expected is null || !ReferenceEquals(expected, current)) return EntryRejected("snapshot");
        if (current.Arrival is not null || current.DefeatReturn is null) return EntryRejected("arrival.phase");
        var prepared = current.Preparation;
        if (!current.CanEnterExploration || prepared.ReturnInputs?.GetAdmissionDiagnostic() is not null ||
            prepared.Party.Id != OriginalBattle01ControlledPartyPreset.LeaderDefeatComparisonId ||
            prepared.Party.GetAdmissionDiagnostic() is not null) return EntryRejected("arrival.binding");
        if (GetArrivalSourceDiagnostic(current.SourceSnapshot, prepared.ArrivalLoad) is { } sourceFailure)
            return new PrivateOriginalBattle01ExplorationEntryRejected(sourceFailure);
        PrivateOriginalBattle01SessionSnapshot next;
        PrivateOriginalBattle01ExplorationEntered result;
        try
        {
            var inputs = prepared.ArrivalInputs!; var load = prepared.ArrivalLoad!;
            var request = current.DefeatReturn;
            var profiles = prepared.Party.Allies.Select(ally => new Battle01AllyInput(ally.Id, ally.ClassId,
                new(ally.Level, ally.HpMax, ally.HpCurrent, ally.MpMax, ally.MpCurrent, ally.EffectiveAttack,
                    ally.EffectiveDefense, ally.EffectiveAgility, ally.EffectiveMove, ally.StatusEffects,
                    ally.Items, ally.Spells, ally.CurrentExp, ally.CurrentKills, ally.CurrentDefeats))).ToArray();
            var party = Battle01ExplorationEntry.Complete(current.Battle, request, inputs.DormantSlots, profiles, inputs.StepCounter);
            var declarations = PrivateOriginalMapReturnArrivalSnapshot.DeclareEntities(load.Runtime.EntityPopulation,
                party, request.DestinationPosition, request.DestinationOpaqueFacing);
            var flags = inputs.Flags.ToDictionary(pair => pair.Key, pair => pair.Value);
            foreach (int flag in Enumerable.Range(256, 128)) flags[flag] = false;
            foreach (int flag in new[] { 1, 66, 80, 399, 401, 600, 601, 602, 603, 608 }) flags[flag] = true;
            flags[64] = prepared.ReturnInputs!.Flag64; flags[640] = prepared.ReturnInputs.Flag640; flags[501] = false;
            var layout = new WorkingMapLayout(load.Runtime.WorkingLayout.Words);
            if (load.FlagCopies.Any(row => flags[row.Flag]) || load.Chests.Any(row => flags[row.Flag]))
                return EntryRejected("arrival.load.flags");
            if (load.Runtime.Traversal.SelectActiveArea(request.DestinationPosition)?.OneBasedRecordOrdinal != 1 ||
                OriginalMapTraversal.IsBlocked(layout, request.DestinationPosition)) return EntryRejected("arrival.destination");
            var roofRow = load.SelectRoof(request.DestinationPosition);
            var roof = MapBlockCopyLifecycleReducer.Activate(layout, MapBlockCopyLifecycleState.Inactive,
                new MapViewUpdateState(false, false), roofRow.Ordinal,
                MapBlockRegionMutation.Clear(roofRow.DestinationX, roofRow.DestinationY, roofRow.Width, roofRow.Height));
            load.Runtime.BlockCatalog.ValidateLayoutReferences(roof.Layout, "arrival.layout");
            // F1 skips Sarah's F602 reposition; F603 moves the same hidden142 out of the map.
            var entities = declarations.Select(entity => entity.Id == 142
                ? entity with { Hidden = true, MovedOutOfMap = true, Position = null, TargetPosition = null } : entity).ToArray();
            var arrival = new PrivateOriginalMapReturnArrivalSnapshot(current, party, load, roof.Layout, roof, entities, flags);
            next = new(prepared, current.Battle, current.SourceLocomotion, current.SourceBridge, request, arrival);
            result = new(next);
        }
        catch (ArgumentException error) { return EntryRejected(error.ParamName ?? "arrival"); }
        // Nothing that can validate, allocate, invoke a source, or project remains after this assignment.
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01ExplorationEntryRejected EntryRejected(string field) =>
        new(new(field, "Granseal entry rejected (" + field + "); the complete current state is retained."));
}

using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapReturnEntity(int Id, int PhysicalSlot, bool IsFollower,
    OriginalMapEntityDefinition? SourceRecord, string ActscriptIdentity, MapPosition DeclarationPosition,
    MapPosition? Position, MapPosition? TargetPosition, byte Facing, byte MapSprite,
    bool Dead, bool Hidden = false, bool MovedOutOfMap = false, bool IsLivePlayerPose = false)
{
    // No follower/NPC actscript is run. NPC generated-script internals are not projected as facts.
    public int? VelocityXUnits => IsFollower || (Id == 0 && !IsLivePlayerPose) ? 0 : null;
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
        RoofLifecycle = roof.LifecycleState; ViewUpdateState = roof.UpdateState;
        Entities = Array.AsReadOnly(entities.ToArray());
        Flags = new ReadOnlyDictionary<int, bool>(flags.ToDictionary(pair => pair.Key, pair => pair.Value));
        ValidateEntities(Entities, load.Runtime.EntityPopulation, before.DefeatReturn!.DestinationPosition);
        Locomotion = PrivateOriginalMapPlayerLocomotionSnapshot.EnterGranseal(before.DefeatReturn);
        ValidateCurrentState();
    }

    internal PrivateOriginalMapReturnArrivalSnapshot(PrivateOriginalMapReturnArrivalSnapshot current,
        PrivateOriginalMapReturnInputReceipt input, bool advance, WorkingMapLayout layout,
        PrivateOriginalMapReturnDoorCopyReceipt? doorCopy)
    {
        current.ValidateCurrentState();
        var entry = current.EntryBeforeMovement ?? current;
        ValidateEntities(entry.Entities, entry.CurrentRuntime.EntityPopulation, entry.PlayerPosition);
        if (entry.EntryBeforeMovement is not null || entry.LastInput is not null ||
            !ReferenceEquals(current.Before, entry.Before) || !ReferenceEquals(current.Party, entry.Party) ||
            !ReferenceEquals(current.Flags, entry.Flags) || !ReferenceEquals(current.RoofClear, entry.RoofClear))
            throw new ArgumentException("Return movement must retain its authenticated entry.", "return.entry");
        if (advance ? !ReferenceEquals(input, current.LastInput) :
            input.Ordinal != checked(current.InputOrdinal + 1) || input.Traversal.Source != current.PlayerPosition)
            throw new ArgumentException("Retain the exact current return input and ordinal.", "return.input");

        Locomotion = advance ? current.Locomotion.Advance() :
            PrivateOriginalMapPlayerLocomotionSnapshot.Begin(current.Locomotion, input.Traversal.Direction,
                current.PlayerPosition, input.Traversal);
        Before = entry.Before; Party = entry.Party; LoadDefinition = entry.LoadDefinition;
        WorkingLayout = layout; RoofClear = entry.RoofClear; Flags = entry.Flags;
        DoorCopy = doorCopy; RoofLifecycle = current.RoofLifecycle; LastRoofAction = current.LastRoofAction;
        ViewUpdateState = ReferenceEquals(doorCopy, current.DoorCopy) ? current.ViewUpdateState :
            new(true, current.ViewUpdateState.Channel1Requested);
        EntryBeforeMovement = entry; LastInput = input;
        if (advance && !Locomotion.IsMoving)
        {
            // Controlled no-fade settlement policy. This is not original VInt/actscript timing.
            var action = MapBlockCopyActionReducer.Apply(WorkingLayout, RoofLifecycle, ViewUpdateState,
                new(PlayerPosition.X, PlayerPosition.Y), isFading: false, LoadDefinition.RoofActions);
            WorkingLayout = action.Layout; RoofLifecycle = action.LifecycleState; ViewUpdateState = action.UpdateState;
            LastRoofAction = new(input.Ordinal, action);
        }
        CurrentRuntime.BlockCatalog.ValidateLayoutReferences(WorkingLayout, "return.layout");
        Entities = Array.AsReadOnly(entry.Entities.Select(entity => entity.Id == 0
            ? entity with { Position = PlayerPosition, TargetPosition = PlayerPosition,
                Facing = PlayerOpaqueFacing, IsLivePlayerPose = true }
            : entity).ToArray());
        ValidateCurrentState();
    }
    public PrivateOriginalBattle01SessionSnapshot Before { get; }
    public Battle01EntryParty Party { get; }
    public OriginalMapReturnEntryLoadDefinition LoadDefinition { get; }
    public OriginalMapExplorationRuntimeDefinition CurrentRuntime => LoadDefinition.Runtime;
    public OriginalMapImportDefinition Definition => Before.SourceSnapshot.Definition;
    public MapId Map => CurrentRuntime.Map;
    public WorkingMapLayout WorkingLayout { get; }
    public MapPosition PlayerPosition => LastInput?.Traversal.Position ?? Before.DefeatReturn!.DestinationPosition;
    public byte PlayerOpaqueFacing => Locomotion.OpaqueFacing;
    public OriginalMapAreaDefinition CurrentAreaDefinition => LoadDefinition.Area;
    public MapBlockCopyLifecycleResult RoofClear { get; }
    public MapBlockCopyLifecycleState RoofLifecycle { get; }
    public MapViewUpdateState ViewUpdateState { get; }
    public PrivateOriginalMapReturnDoorCopyReceipt? DoorCopy { get; }
    public PrivateOriginalMapReturnRoofActionReceipt? LastRoofAction { get; }
    public IReadOnlyList<PrivateOriginalMapReturnEntity> Entities { get; }
    public IReadOnlyDictionary<int, bool> Flags { get; }
    public PrivateOriginalMapPlayerLocomotionSnapshot Locomotion { get; }
    public PrivateOriginalMapReturnArrivalSnapshot? EntryBeforeMovement { get; }
    public PrivateOriginalMapReturnInputReceipt? LastInput { get; }
    public long InputOrdinal => LastInput?.Ordinal ?? 0;
    public bool ExplorationInputAvailable => !Locomotion.IsMoving;
    public string EntityPolicy => "Player-only movement; follower/NPC declarations frozen. Scripts are not running.";
    public bool Entity142Hidden => Entities.Single(entity => entity.Id == 142).Hidden;
    public bool Entity142MovedOut => Entities.Single(entity => entity.Id == 142).MovedOutOfMap;

    internal void ValidateCurrentState()
    {
        var entry = EntryBeforeMovement ?? this;
        ValidateEntities(entry.Entities, entry.CurrentRuntime.EntityPopulation, entry.Before.DefeatReturn!.DestinationPosition);
        if (entry.EntryBeforeMovement is not null || entry.LastInput is not null || entry.DoorCopy is not null ||
            entry.LastRoofAction is not null || !ReferenceEquals(entry.WorkingLayout, entry.RoofClear.Layout) ||
            !ReferenceEquals(entry.RoofLifecycle, entry.RoofClear.LifecycleState) ||
            !ReferenceEquals(Before, entry.Before) || !ReferenceEquals(Party, entry.Party) || Party.CurrentBattle != 255 ||
            !ReferenceEquals(LoadDefinition, entry.LoadDefinition) || !ReferenceEquals(Flags, entry.Flags) ||
            !ReferenceEquals(RoofClear, entry.RoofClear))
            throw new ArgumentException("Retain the strict entry before-image and current exploration owners.", "return.entry");
        if (entry.RoofLifecycle is not MapBlockCopyLifecycleActiveState saved || !IsChurchRoof(saved) ||
            !saved.SavedWords.SequenceEqual(Enumerable.Range(41, 6).SelectMany(y =>
                Enumerable.Range(30, 5).Select(x => CurrentRuntime.WorkingLayout[x, y]))))
            throw new ArgumentException("Retain the original church saved30 and ordinal8.", "return.roof");
        if (RoofLifecycle is MapBlockCopyLifecycleActiveState active)
        {
            if (!IsChurchRoof(active) || !active.SavedWords.SequenceEqual(saved.SavedWords))
                throw new ArgumentException("The live roof must retain the authenticated saved rectangle.", "return.roof");
        }
        else if (RoofLifecycle is not MapBlockCopyLifecycleInactiveState || DoorCopy is null)
            throw new ArgumentException("The live roof must be active8 or a doorway restoration.", "return.roof");
        if (DoorCopy is { } door && (!ReferenceEquals(door.Entry, entry) ||
            !ReferenceEquals(door.Definition, LoadDefinition.ChurchDoor) || door.Input.Ordinal < 1 ||
            door.Input.Ordinal > InputOrdinal || door.BeforeWord != 0xC48F || door.AfterWord != 0x080E ||
            door.Input.Traversal.Source != new MapPosition(32, 14) ||
            door.Input.Traversal.Position != new MapPosition(32, 15) ||
            door.Input.Traversal.Direction != ExplorationDirection.South ||
            door.Input.Traversal.Outcome != OriginalMapTraversalOutcome.Moved ||
            door.Input.Traversal.DestinationWord != 0x080E))
            throw new ArgumentException("Retain the single entry-bound door-copy input receipt.", "return.door");
        for (int i = 0; i < WorkingMapLayout.WordCount; i++)
        {
            int x = i % 64, y = i / 64;
            ushort entryWord = x is >= 30 and <= 34 && y is >= 41 and <= 46 ? (ushort)0 :
                CurrentRuntime.WorkingLayout.Words[i];
            if (entry.WorkingLayout.Words[i] != entryWord)
                throw new ArgumentException("The entry layout must retain the original once-only roof clear.", "return.entry");
            ushort expected = i == 15 * 64 + 32 && DoorCopy is not null ? (ushort)0x080E :
                x is >= 30 and <= 34 && y is >= 41 and <= 46 && RoofLifecycle is MapBlockCopyLifecycleInactiveState
                    ? saved.SavedWords[(y - 41) * 5 + x - 30] : entry.WorkingLayout.Words[i];
            if (WorkingLayout.Words[i] != expected)
                throw new ArgumentException("Only the door word and saved church roof may differ from entry.", "return.layout");
        }
        if (LastRoofAction is { } roof && (roof.InputOrdinal < 1 || roof.InputOrdinal > InputOrdinal ||
            !ReferenceEquals(roof.Action.LifecycleState, RoofLifecycle) ||
            roof.Action.UpdateState != ViewUpdateState ||
            roof.InputOrdinal == InputOrdinal && Locomotion.IsMoving))
            throw new ArgumentException("Roof actions belong only to a settled input.", "return.roof");
    }

    private static bool IsChurchRoof(MapBlockCopyLifecycleActiveState state) => state.RecordOrdinal == 8 &&
        state.DestinationX == 30 && state.DestinationY == 41 && state.Width == 5 && state.Height == 6;

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
            if (entity.IsLivePlayerPose)
                throw new ArgumentException("A live player pose is not an initial declaration.", "arrival.entities");
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
            // Preserve only known permanent values from the authenticated current owners.
            // Earlier interaction receipts and unmodeled flags do not supply current defaults.
            var source = current.SourceSnapshot; var battle = current.Battle;
            var flags = new Dictionary<int, bool>
            {
                [1] = source.MiddleTowerGuard!.ProgramStoryFlag1Set,
                [64] = request.Admission.Flag64,
                [66] = source.MessengerAcceptance!.Flag66Set,
                [88] = battle.SuspendedFlag88,
                [399] = battle.BattleEntryFlag399,
                [401] = battle.UnlockFlag401,
                [451] = battle.IntroFlag451,
                [501] = battle.CompletedFlag501,
                [600] = source.MessengerAcceptance.Flag600Set,
                [601] = source.Zone601!.Flag601Set,
                [602] = source.Entity142!.Flag602Set,
                [603] = source.MessengerAcceptance.Flag603Set,
                [604] = source.CastleGate!.Flag604Set,
                [605] = source.PalaceFirstVisit!.CompletionFlag605Set,
                [607] = source.AstralAcceptance!.HandlerFlag607Set,
                [608] = source.AstralAcceptance.ProgramFlag608Set,
                [640] = request.Admission.Flag640,
            };
            for (int index = 0; index < battle.RegionFlags90Through105.Count; index++)
                flags.Add(90 + index, battle.RegionFlags90Through105[index]);
            foreach (var pair in inputs.Flags) flags.Add(pair.Key, pair.Value);
            foreach (int flag in Enumerable.Range(256, 128)) flags[flag] = false;
            flags[80] = true;
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

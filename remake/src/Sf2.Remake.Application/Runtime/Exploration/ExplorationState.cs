using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

public readonly record struct WaitToken(long Value);
public abstract record ProgramWait(WaitToken Token);
public sealed record DialogueWait(WaitToken Token, int Text, TextDisplayMode Mode, EntityRef? Speaker, byte SpeakerFlags = 0,
    bool CloseOnAcknowledgement = true, bool? InputFirstEntityService = null) : ProgramWait(Token);
public sealed record ChoiceWait(WaitToken Token, int ResultFlag) : ProgramWait(Token);
public sealed record GestureRestore(byte AnimationCounter, ushort SpriteSize);
public sealed record PresentationWait(WaitToken Token, PresentCue Cue, GestureRestore? Restore = null) : ProgramWait(Token);
public enum FullFadePurpose { WarpOut, WarpIn, Script }
public sealed record FullFadeWait(WaitToken Token, PresentationCueKind Kind, FullFadePurpose Purpose,
    byte Period, byte Countdown, int Entry = 0, int ExtraServices = 0, bool LogicalDone = false,
    bool ActualDone = false, byte? RestorePeriod = null) : ProgramWait(Token);
public sealed record WarpLoadWait(WaitToken Token, int Remaining = 2) : ProgramWait(Token);
public sealed record OrdinaryWarp(MapId Map, MapPosition Position, byte Facing, MapLoadMode Mode);
public sealed record EntityWait(WaitToken Token, EntityRef Entity, ProgramLocation? AfterMotion = null,
    ExplorationDirection? PendingMove = null) : ProgramWait(Token);
public sealed record EntitySpriteWait(WaitToken Token, int Slot, long Request) : ProgramWait(Token);
public sealed record EntitySetSpriteWait(WaitToken Token) : ProgramWait(Token);
public sealed record TickWait(WaitToken Token, int Remaining) : ProgramWait(Token);
public sealed record EntityEventFacingWait(WaitToken Token) : ProgramWait(Token);
public sealed record EntityEventContext(EntityRef Entity, byte OriginalFacing, byte Flags);
public abstract record TextWindow;
public sealed record ClosedTextWindow : TextWindow;
public sealed record OpenTextWindow(int Text, TextDisplayMode Mode, EntityRef? Speaker, byte SpeakerFlags = 0) : TextWindow;
public abstract record PortraitWindow;
// Old content supplies a display hint, never evidence of the source service gate.
public sealed record UnknownPortraitWindow(EntityRef? LegacySpeaker = null, byte LegacyFlags = 0) : PortraitWindow;
public sealed record ClosedPortraitWindow : PortraitWindow;
public sealed record OpenPortraitWindow(int Portrait, byte Flags) : PortraitWindow;
public sealed record FieldReturnAnchor(MapId Map, MapPosition Position, byte Facing);
public enum ProgramContinuation { FieldInput, MapLoaded, BeforeBattleFinished, BattleLoadFinished, BattleStartFinished, VictoryProgramFinished, DefeatProgramFinished, OutcomeMapLoaded }

public sealed class StoryState
{
    internal StoryState(IEnumerable<int>? flags = null, ProgramLocation? cursor = null,
        IEnumerable<ProgramLocation>? callers = null, ProgramWait? wait = null, int textCursor = 0,
        ProgramContinuation continuation = ProgramContinuation.FieldInput,
        ExplorationBattleRoute? enteringBattle = null, FieldReturnAnchor? returnAnchor = null, TextWindow? textWindow = null, long simulationTick = 0,
        MapPartyLists? partyLists = null, EntityEventContext? entityEvent = null, EntityRef? speaker = null, MapPosition? cameraTarget = null,
        int? cameraEntitySlot = null, FieldReturnAnchor? outcomeReturn = null, PortraitWindow? portraitWindow = null,
        ExplorationDisplay? display = null, OrdinaryWarp? warp = null)
    {
        Flags = Array.AsReadOnly((flags ?? []).Distinct().Order().ToArray()); Cursor = cursor;
        Callers = Array.AsReadOnly((callers ?? []).ToArray()); Wait = wait; TextCursor = textCursor;
        Continuation = continuation; EnteringBattle = enteringBattle; ReturnAnchor = returnAnchor;
        TextWindow = textWindow ?? new ClosedTextWindow(); SimulationTick = simulationTick; PartyLists = partyLists; EntityEvent = entityEvent;
        Speaker = speaker; CameraTarget = cameraTarget; CameraEntitySlot = cameraEntitySlot; OutcomeReturn = outcomeReturn;
        PortraitWindow = portraitWindow ?? new UnknownPortraitWindow();
        Display = display; Warp = warp;
    }
    public IReadOnlyList<int> Flags { get; }
    public ProgramLocation? Cursor { get; }
    public IReadOnlyList<ProgramLocation> Callers { get; }
    public ProgramWait? Wait { get; }
    public int TextCursor { get; }
    public ProgramContinuation Continuation { get; }
    public ExplorationBattleRoute? EnteringBattle { get; }
    public FieldReturnAnchor? ReturnAnchor { get; }
    public FieldReturnAnchor? OutcomeReturn { get; }
    public TextWindow TextWindow { get; }
    public PortraitWindow PortraitWindow { get; }
    public long SimulationTick { get; }
    public MapPartyLists? PartyLists { get; }
    public EntityEventContext? EntityEvent { get; }
    public EntityRef? Speaker { get; }
    public MapPosition? CameraTarget { get; }
    public int? CameraEntitySlot { get; }
    public ExplorationDisplay? Display { get; }
    public OrdinaryWarp? Warp { get; }
    internal StoryState Copy(ProgramLocation? cursor, ProgramWait? wait = null,
        IEnumerable<int>? flags = null, IEnumerable<ProgramLocation>? callers = null, int? textCursor = null,
        ProgramContinuation? continuation = null, ExplorationBattleRoute? enteringBattle = null,
        FieldReturnAnchor? returnAnchor = null, TextWindow? textWindow = null, long? simulationTick = null, MapPartyLists? partyLists = null,
        EntityEventContext? entityEvent = null, bool clearEntityEvent = false, EntityRef? speaker = null, MapPosition? cameraTarget = null,
        bool clearSpeaker = false, bool clearCameraTarget = false, int? cameraEntitySlot = null, bool clearCameraEntity = false,
        FieldReturnAnchor? outcomeReturn = null, bool clearEnteringBattle = false, PortraitWindow? portraitWindow = null,
        ExplorationDisplay? display = null, OrdinaryWarp? warp = null, bool clearWarp = false) =>
        new(flags ?? Flags, cursor, callers ?? Callers, wait, textCursor ?? TextCursor,
            continuation ?? Continuation, clearEnteringBattle ? null : enteringBattle ?? EnteringBattle, returnAnchor ?? ReturnAnchor,
            textWindow ?? TextWindow, simulationTick ?? SimulationTick, partyLists ?? PartyLists, clearEntityEvent ? null : entityEvent ?? EntityEvent,
            clearSpeaker ? null : speaker ?? Speaker, clearCameraTarget ? null : cameraTarget ?? CameraTarget,
            clearCameraEntity ? null : cameraEntitySlot ?? CameraEntitySlot, outcomeReturn ?? OutcomeReturn, portraitWindow ?? PortraitWindow,
            display ?? Display, clearWarp ? null : warp ?? Warp);
}

public sealed record ExplorationEntity(EntityRef Entity, EntityMotionState Motion, bool Visible,
    EntityActionProgram? Actions = null, int ActionCursor = 0, bool WaitingForMotion = false,
    int Slot = -1, int? Sprite = null, EntityFollower? Follower = null,
    long SpriteRequest = 0, long SpriteReady = 0, bool WaitingForSprite = false, bool Priority = false)
{
    public MapPosition Position => new(Motion.X / 384, Motion.Y / 384);
    public bool Busy => Motion.IsMoving || Actions is not null;
}
public sealed record EntityFollower(int LeaderSlot, int OffsetX, int OffsetY);

public sealed class ExplorationState
{
    internal ExplorationState(ExplorationMapDefinition definition, WorkingMapLayout layout, EntityRef player,
        IEnumerable<ExplorationEntity> entities, BattleStartInput party, ushort spriteSize = 16,
        IReadOnlyDictionary<EntityRef, int>? aliases = null, MapBlockCopyLifecycleState? roofState = null,
        ExplorationPopulation? population = null)
    {
        Definition = definition; Layout = layout; Player = player; Party = party; SpriteSize = spriteSize; RoofState = roofState ?? MapBlockCopyLifecycleState.Inactive;
        Population = population ?? definition.Population;
        AllEntities = Array.AsReadOnly(entities.Select((entity, index) => entity.Slot < 0 ? entity with { Slot = index } : entity)
            .OrderBy(entity => entity.Slot).ToArray());
        Aliases = new ReadOnlyDictionary<EntityRef, int>((aliases ?? AllEntities.ToDictionary(entity => entity.Entity, entity => entity.Slot))
            .ToDictionary(pair => pair.Key, pair => pair.Value));
        var slots = AllEntities.ToDictionary(entity => entity.Slot);
        Entities = new ReadOnlyDictionary<EntityRef, ExplorationEntity>(Aliases.ToDictionary(pair => pair.Key, pair => slots[pair.Value]));
    }
    public ExplorationMapDefinition Definition { get; }
    public ExplorationPopulation? Population { get; }
    public MapId Map => Definition.Map;
    public WorkingMapLayout Layout { get; }
    public MapBlockCopyLifecycleState RoofState { get; }
    public EntityRef Player { get; }
    public IReadOnlyDictionary<EntityRef, ExplorationEntity> Entities { get; }
    public IReadOnlyList<ExplorationEntity> AllEntities { get; }
    public IReadOnlyDictionary<EntityRef, int> Aliases { get; }
    // Before encounter entry these are the only live vitals/RNG/accounting. Entry consumes this mode.
    public BattleStartInput Party { get; }
    public ushort SpriteSize { get; }
    public ExplorationEntity PlayerEntity => Entities[Player];
    public bool TryResolveEntity(EntityRef reference, out ExplorationEntity entity)
    {
        entity = null!;
        if (Population is not null)
        {
            if (!reference.Value.StartsWith("entity-", StringComparison.Ordinal) ||
                !int.TryParse(reference.Value.AsSpan(7), System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out int selector)) return false;
            int encoded = selector & 255;
            int index = encoded < 128 ? encoded : encoded - 96;
            // The accepted source contract covers the 64 identity entries and 49 normal records.
            // Out-of-table selectors and removed (FF) identities cannot become player fallbacks.
            if (index >= 64) return false;
            reference = new("entity-" + (index < 32 ? index : index + 96).ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (!Aliases.TryGetValue(reference, out int slot) || slot >= 49) return false;
        }
        return Entities.TryGetValue(reference, out entity!);
    }
    internal ExplorationState WithEntities(IEnumerable<ExplorationEntity> entities, ushort? spriteSize = null) => new(Definition, Layout, Player, entities, Party, spriteSize ?? SpriteSize, Aliases, RoofState, Population);
    internal ExplorationState WithLayout(WorkingMapLayout layout, MapBlockCopyLifecycleState? roofState = null) =>
        new(Definition, layout, Player, AllEntities, Party, SpriteSize, Aliases, roofState ?? RoofState, Population);
    internal ExplorationState WithParty(BattleStartInput party) => new(Definition, Layout, Player, AllEntities, party, SpriteSize, Aliases, RoofState, Population);
    internal ExplorationState Hide(ExplorationEntity entity, bool removeAliases)
    {
        var hidden = entity with { Visible = false, Actions = null, Follower = null, WaitingForSprite = false,
            Motion = entity.Motion with { X = 0x7000, Y = 0x7000, XDestination = 0x7000, YDestination = 0x7000 } };
        // For source populations an absent reference represents FF, distinct from a fresh zero mapping.
        return new(Definition, Layout, Player, AllEntities.Select(row => row.Slot == hidden.Slot ? hidden : row), Party, SpriteSize,
            Aliases.Where(pair => !removeAliases || pair.Value != hidden.Slot).ToDictionary(pair => pair.Key, pair => pair.Value), RoofState, Population);
    }
    internal ExplorationState WithEntity(ExplorationEntity entity) =>
        WithEntities(AllEntities.Select(current => current.Slot == entity.Slot ? entity : current));
}

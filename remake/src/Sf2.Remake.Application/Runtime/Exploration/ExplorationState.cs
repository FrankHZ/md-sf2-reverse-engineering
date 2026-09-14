using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

public readonly record struct WaitToken(long Value);
public abstract record ProgramWait(WaitToken Token);
public sealed record DialogueWait(WaitToken Token, int Text, TextDisplayMode Mode, EntityRef? Speaker, byte SpeakerFlags = 0) : ProgramWait(Token);
public sealed record ChoiceWait(WaitToken Token, int ResultFlag) : ProgramWait(Token);
public sealed record PresentationWait(WaitToken Token, PresentCue Cue) : ProgramWait(Token);
public sealed record EntityWait(WaitToken Token, EntityRef Entity, ProgramLocation? AfterMotion = null) : ProgramWait(Token);
public sealed record TickWait(WaitToken Token, int Remaining) : ProgramWait(Token);
public sealed record EntityEventFacingWait(WaitToken Token) : ProgramWait(Token);
public sealed record EntityEventContext(EntityRef Entity, byte OriginalFacing, byte Flags);
public abstract record TextWindow;
public sealed record ClosedTextWindow : TextWindow;
public sealed record OpenTextWindow(int Text, TextDisplayMode Mode, EntityRef? Speaker, byte SpeakerFlags = 0) : TextWindow;
public sealed record FieldReturnAnchor(MapId Map, MapPosition Position, byte Facing);
public enum ProgramContinuation { FieldInput, MapLoaded, BeforeBattleFinished, BattleStartFinished }

public sealed class StoryState
{
    internal StoryState(IEnumerable<int>? flags = null, ProgramLocation? cursor = null,
        IEnumerable<ProgramLocation>? callers = null, ProgramWait? wait = null, int textCursor = 0,
        ProgramContinuation continuation = ProgramContinuation.FieldInput,
        ExplorationBattleRoute? enteringBattle = null, FieldReturnAnchor? returnAnchor = null, TextWindow? textWindow = null, long simulationTick = 0,
        MapPartyLists? partyLists = null, EntityEventContext? entityEvent = null, EntityRef? speaker = null, MapPosition? cameraTarget = null)
    {
        Flags = Array.AsReadOnly((flags ?? []).Distinct().Order().ToArray()); Cursor = cursor;
        Callers = Array.AsReadOnly((callers ?? []).ToArray()); Wait = wait; TextCursor = textCursor;
        Continuation = continuation; EnteringBattle = enteringBattle; ReturnAnchor = returnAnchor;
        TextWindow = textWindow ?? new ClosedTextWindow(); SimulationTick = simulationTick; PartyLists = partyLists; EntityEvent = entityEvent;
        Speaker = speaker; CameraTarget = cameraTarget;
    }
    public IReadOnlyList<int> Flags { get; }
    public ProgramLocation? Cursor { get; }
    public IReadOnlyList<ProgramLocation> Callers { get; }
    public ProgramWait? Wait { get; }
    public int TextCursor { get; }
    public ProgramContinuation Continuation { get; }
    public ExplorationBattleRoute? EnteringBattle { get; }
    public FieldReturnAnchor? ReturnAnchor { get; }
    public TextWindow TextWindow { get; }
    public long SimulationTick { get; }
    public MapPartyLists? PartyLists { get; }
    public EntityEventContext? EntityEvent { get; }
    public EntityRef? Speaker { get; }
    public MapPosition? CameraTarget { get; }
    internal StoryState Copy(ProgramLocation? cursor, ProgramWait? wait = null,
        IEnumerable<int>? flags = null, IEnumerable<ProgramLocation>? callers = null, int? textCursor = null,
        ProgramContinuation? continuation = null, ExplorationBattleRoute? enteringBattle = null,
        FieldReturnAnchor? returnAnchor = null, TextWindow? textWindow = null, long? simulationTick = null, MapPartyLists? partyLists = null,
        EntityEventContext? entityEvent = null, bool clearEntityEvent = false, EntityRef? speaker = null, MapPosition? cameraTarget = null,
        bool clearSpeaker = false, bool clearCameraTarget = false) =>
        new(flags ?? Flags, cursor, callers ?? Callers, wait, textCursor ?? TextCursor,
            continuation ?? Continuation, enteringBattle ?? EnteringBattle, returnAnchor ?? ReturnAnchor,
            textWindow ?? TextWindow, simulationTick ?? SimulationTick, partyLists ?? PartyLists, clearEntityEvent ? null : entityEvent ?? EntityEvent,
            clearSpeaker ? null : speaker ?? Speaker, clearCameraTarget ? null : cameraTarget ?? CameraTarget);
}

public sealed record ExplorationEntity(EntityRef Entity, EntityMotionState Motion, bool Visible,
    EntityActionProgram? Actions = null, int ActionCursor = 0, bool WaitingForMotion = false,
    int Slot = -1, int? Sprite = null, EntityFollower? Follower = null,
    long SpriteRequest = 0, long SpriteReady = 0, bool WaitingForSprite = false)
{
    public MapPosition Position => new(Motion.X / 384, Motion.Y / 384);
    public bool Busy => Motion.IsMoving || Actions is not null;
}
public sealed record EntityFollower(int LeaderSlot, int OffsetX, int OffsetY);

public sealed class ExplorationState
{
    internal ExplorationState(ExplorationMapDefinition definition, WorkingMapLayout layout, EntityRef player,
        IEnumerable<ExplorationEntity> entities, BattleStartInput party, ushort spriteSize = 16,
        IReadOnlyDictionary<EntityRef, int>? aliases = null, MapBlockCopyLifecycleState? roofState = null)
    {
        Definition = definition; Layout = layout; Player = player; Party = party; SpriteSize = spriteSize; RoofState = roofState ?? MapBlockCopyLifecycleState.Inactive;
        AllEntities = Array.AsReadOnly(entities.Select((entity, index) => entity.Slot < 0 ? entity with { Slot = index } : entity)
            .OrderBy(entity => entity.Slot).ToArray());
        Aliases = new ReadOnlyDictionary<EntityRef, int>((aliases ?? AllEntities.ToDictionary(entity => entity.Entity, entity => entity.Slot))
            .ToDictionary(pair => pair.Key, pair => pair.Value));
        var slots = AllEntities.ToDictionary(entity => entity.Slot);
        Entities = new ReadOnlyDictionary<EntityRef, ExplorationEntity>(Aliases.ToDictionary(pair => pair.Key, pair => slots[pair.Value]));
    }
    public ExplorationMapDefinition Definition { get; }
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
    internal ExplorationState WithEntities(IEnumerable<ExplorationEntity> entities, ushort? spriteSize = null) => new(Definition, Layout, Player, entities, Party, spriteSize ?? SpriteSize, Aliases, RoofState);
    internal ExplorationState WithLayout(WorkingMapLayout layout, MapBlockCopyLifecycleState? roofState = null) =>
        new(Definition, layout, Player, AllEntities, Party, SpriteSize, Aliases, roofState ?? RoofState);
    internal ExplorationState WithParty(BattleStartInput party) => new(Definition, Layout, Player, AllEntities, party, SpriteSize, Aliases, RoofState);
    internal ExplorationState Hide(ExplorationEntity entity, bool removeAliases)
    {
        var hidden = entity with { Visible = false, Actions = null, Follower = null, WaitingForSprite = false,
            Motion = entity.Motion with { X = 0x7000, Y = 0x7000, XDestination = 0x7000, YDestination = 0x7000 } };
        return new(Definition, Layout, Player, AllEntities.Select(row => row.Slot == hidden.Slot ? hidden : row), Party, SpriteSize,
            Aliases.Where(pair => !removeAliases || pair.Value != hidden.Slot).ToDictionary(pair => pair.Key, pair => pair.Value), RoofState);
    }
    internal ExplorationState WithEntity(ExplorationEntity entity) =>
        WithEntities(AllEntities.Select(current => current.Slot == entity.Slot ? entity : current));
}

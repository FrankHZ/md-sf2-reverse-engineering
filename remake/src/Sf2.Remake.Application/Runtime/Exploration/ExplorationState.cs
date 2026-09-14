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
        ExplorationBattleRoute? enteringBattle = null, FieldReturnAnchor? returnAnchor = null, TextWindow? textWindow = null, long simulationTick = 0)
    {
        Flags = Array.AsReadOnly((flags ?? []).Distinct().Order().ToArray()); Cursor = cursor;
        Callers = Array.AsReadOnly((callers ?? []).ToArray()); Wait = wait; TextCursor = textCursor;
        Continuation = continuation; EnteringBattle = enteringBattle; ReturnAnchor = returnAnchor;
        TextWindow = textWindow ?? new ClosedTextWindow(); SimulationTick = simulationTick;
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
    internal StoryState Copy(ProgramLocation? cursor, ProgramWait? wait = null,
        IEnumerable<int>? flags = null, IEnumerable<ProgramLocation>? callers = null, int? textCursor = null,
        ProgramContinuation? continuation = null, ExplorationBattleRoute? enteringBattle = null,
        FieldReturnAnchor? returnAnchor = null, TextWindow? textWindow = null, long? simulationTick = null) =>
        new(flags ?? Flags, cursor, callers ?? Callers, wait, textCursor ?? TextCursor,
            continuation ?? Continuation, enteringBattle ?? EnteringBattle, returnAnchor ?? ReturnAnchor,
            textWindow ?? TextWindow, simulationTick ?? SimulationTick);
}

public sealed record ExplorationEntity(EntityRef Entity, EntityMotionState Motion, bool Visible,
    EntityActionProgram? Actions = null, int ActionCursor = 0, bool WaitingForMotion = false)
{
    public MapPosition Position => new(Motion.X / 384, Motion.Y / 384);
    public bool Busy => Motion.IsMoving || Actions is not null;
}

public sealed class ExplorationState
{
    internal ExplorationState(ExplorationMapDefinition definition, WorkingMapLayout layout, EntityRef player,
        IEnumerable<ExplorationEntity> entities, BattleStartInput party, ushort spriteSize = 16)
    {
        Definition = definition; Layout = layout; Player = player; Party = party; SpriteSize = spriteSize;
        Entities = new ReadOnlyDictionary<EntityRef, ExplorationEntity>(entities.ToDictionary(entity => entity.Entity));
    }
    public ExplorationMapDefinition Definition { get; }
    public MapId Map => Definition.Map;
    public WorkingMapLayout Layout { get; }
    public EntityRef Player { get; }
    public IReadOnlyDictionary<EntityRef, ExplorationEntity> Entities { get; }
    // Before encounter entry these are the only live vitals/RNG/accounting. Entry consumes this mode.
    public BattleStartInput Party { get; }
    public ushort SpriteSize { get; }
    public ExplorationEntity PlayerEntity => Entities[Player];
    internal ExplorationState WithEntities(IEnumerable<ExplorationEntity> entities, ushort? spriteSize = null) => new(Definition, Layout, Player, entities, Party, spriteSize ?? SpriteSize);
    internal ExplorationState WithEntity(ExplorationEntity entity) =>
        WithEntities(Entities.Values.Select(current => current.Entity == entity.Entity ? entity : current));
}

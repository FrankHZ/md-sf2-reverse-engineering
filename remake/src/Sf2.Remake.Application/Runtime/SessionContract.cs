using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime;

public enum SessionFailureKind { IllegalCommand, UnsupportedCapability, ContentError, InvariantFailure, AdapterError }
public sealed record SessionFailure(SessionFailureKind Kind, string Code, string Field, string Message);
public enum SessionStopReason { PlayerInput, PresentationWait, SimulationWait, Rejected, Unsupported, Faulted }
public enum SessionMode { Exploration, Battle }
public enum BattleSelectionStage { Movement, ActionChoice, TargetChoice, CommitReady }
public enum SessionAction { Stay, Heal, PhysicalAttack, Item }

public abstract record SessionCommand;
public sealed record Move(ExplorationDirection Direction) : SessionCommand;
public sealed record ChooseAction(SessionAction Action) : SessionCommand;
public sealed record SelectSpell(SpellRef Spell) : SessionCommand;
public sealed record SelectItem(int Slot) : SessionCommand;
public sealed record SelectTarget(ActorRef Target) : SessionCommand;
public sealed record Confirm : SessionCommand;
public sealed record Cancel : SessionCommand;
public sealed record AdvanceSimulation(WaitToken? Wait = null, int Ticks = 1) : SessionCommand;
public sealed record Interact(EntityRef Entity) : SessionCommand;
public sealed record Acknowledge(WaitToken Wait) : SessionCommand;
public sealed record CompletePresentation(WaitToken Wait, PresentationCueKind Kind) : SessionCommand;
public sealed record EntitySpriteReady(int Slot, long Request) : SessionCommand;
public sealed record ChooseDialogue(WaitToken Wait, bool Yes) : SessionCommand;
public sealed record CommandEnvelope(Guid SessionId, long ExpectedRevision, ActorRef? Actor, SessionCommand Command);

public sealed class BattleSelection
{
    internal BattleSelection(ActorRef actor, BattleMovementPreview preview, BattleSelectionStage stage,
        SessionAction? action = null, SpellRef? spell = null, ActorRef? target = null, int? itemSlot = null)
    { Actor = actor; Preview = preview; Stage = stage; Action = action; Spell = spell; Target = target; ItemSlot = itemSlot; }
    public ActorRef Actor { get; }
    public BattleMovementPreview Preview { get; }
    public BattleSelectionStage Stage { get; }
    public SessionAction? Action { get; }
    public int? ItemSlot { get; }
    public SpellRef? Spell { get; }
    public ActorRef? Target { get; }
}

public abstract record ActiveSessionState;
public sealed record ActiveBattle(EngineBattleState Battle, BattleSelection? Selection, BattleSceneState? Scene = null) : ActiveSessionState;
public sealed record ActiveExploration(ExplorationState World) : ActiveSessionState;

public sealed class SessionSnapshot
{
    internal SessionSnapshot(Guid sessionId, long revision, long observationSequence,
        EngineBattleState battle, BattleSelection? selection, SessionStopReason stopReason)
        : this(sessionId, revision, observationSequence, new ActiveBattle(battle, selection), new StoryState(), stopReason) { }
    internal SessionSnapshot(Guid sessionId, long revision, long observationSequence,
        ActiveSessionState active, StoryState story, SessionStopReason stopReason)
    {
        SessionId = sessionId; Revision = revision; ObservationSequence = observationSequence;
        Active = active; Story = story; StopReason = stopReason;
    }
    public Guid SessionId { get; }
    public long Revision { get; }
    public long ObservationSequence { get; }
    public ActiveSessionState Active { get; }
    public StoryState Story { get; }
    public SessionMode Mode => Active is ActiveBattle ? SessionMode.Battle : SessionMode.Exploration;
    public bool HasBattleControl => Mode == SessionMode.Battle && Story.Cursor is null && Story.Wait is null && BattleScene is null;
    public BattleSceneState? BattleScene => (Active as ActiveBattle)?.Scene;
    public EngineBattleState Battle => Active is ActiveBattle battle ? battle.Battle :
        throw new InvalidOperationException("The active mode is exploration.");
    public ExplorationState? Exploration => (Active as ActiveExploration)?.World;
    public BattleSelection? Selection => (Active as ActiveBattle)?.Selection;
    public SessionStopReason StopReason { get; }
    internal SessionSnapshot WithStory(StoryState story) => new(SessionId, Revision, ObservationSequence, Active, story, StopReason);
}

public sealed record SessionObservation(long Sequence, long Revision, string Kind,
    ActorRef? Actor = null, long? Before = null, long? After = null,
    MapPosition? From = null, MapPosition? To = null, ushort? RandomRange = null, ushort? RandomValue = null,
    ActorRef? Target = null, string? Detail = null, EntityRef? Entity = null, ProgramLocation? Program = null);
public sealed record SessionResult(SessionSnapshot Snapshot, IReadOnlyList<SessionObservation> Observations,
    SessionStopReason StopReason, SessionFailure? Failure = null);
public abstract record SessionStartOutcome;
public sealed record SessionStarted(GameSession Session, SessionResult Result) : SessionStartOutcome;
public sealed record SessionStartFailed(SessionFailure Failure) : SessionStartOutcome;

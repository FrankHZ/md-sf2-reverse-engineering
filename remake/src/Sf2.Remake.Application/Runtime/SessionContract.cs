using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime;

public enum SessionFailureKind { IllegalCommand, UnsupportedCapability, ContentError, InvariantFailure, AdapterError }
public sealed record SessionFailure(SessionFailureKind Kind, string Code, string Field, string Message);
public enum SessionStopReason { PlayerInput, SimulationWait, Rejected, Unsupported, Faulted }
public enum SessionMode { Battle }
public enum BattleSelectionStage { Movement, ActionChoice, TargetChoice, CommitReady }
public enum SessionAction { Stay, Heal, PhysicalAttack }

public abstract record SessionCommand;
public sealed record Move(ExplorationDirection Direction) : SessionCommand;
public sealed record ChooseAction(SessionAction Action) : SessionCommand;
public sealed record SelectSpell(SpellRef Spell) : SessionCommand;
public sealed record SelectTarget(ActorRef Target) : SessionCommand;
public sealed record Confirm : SessionCommand;
public sealed record Cancel : SessionCommand;
public sealed record AdvanceSimulation : SessionCommand;
public sealed record CommandEnvelope(Guid SessionId, long ExpectedRevision, ActorRef? Actor, SessionCommand Command);

public sealed class BattleSelection
{
    internal BattleSelection(ActorRef actor, BattleMovementPreview preview, BattleSelectionStage stage,
        SessionAction? action = null, SpellRef? spell = null, ActorRef? target = null)
    { Actor = actor; Preview = preview; Stage = stage; Action = action; Spell = spell; Target = target; }
    public ActorRef Actor { get; }
    public BattleMovementPreview Preview { get; }
    public BattleSelectionStage Stage { get; }
    public SessionAction? Action { get; }
    public SpellRef? Spell { get; }
    public ActorRef? Target { get; }
}

public sealed class SessionSnapshot
{
    internal SessionSnapshot(Guid sessionId, long revision, long observationSequence,
        EngineBattleState battle, BattleSelection? selection, SessionStopReason stopReason)
    {
        SessionId = sessionId; Revision = revision; ObservationSequence = observationSequence;
        Battle = battle; Selection = selection; StopReason = stopReason;
    }
    public Guid SessionId { get; }
    public long Revision { get; }
    public long ObservationSequence { get; }
    public SessionMode Mode => SessionMode.Battle;
    public EngineBattleState Battle { get; }
    public BattleSelection? Selection { get; }
    public SessionStopReason StopReason { get; }
}

public sealed record SessionObservation(long Sequence, long Revision, string Kind,
    ActorRef? Actor = null, long? Before = null, long? After = null,
    MapPosition? From = null, MapPosition? To = null);
public sealed record SessionResult(SessionSnapshot Snapshot, IReadOnlyList<SessionObservation> Observations,
    SessionStopReason StopReason, SessionFailure? Failure = null);
public abstract record SessionStartOutcome;
public sealed record SessionStarted(GameSession Session, SessionResult Result) : SessionStartOutcome;
public sealed record SessionStartFailed(SessionFailure Failure) : SessionStartOutcome;

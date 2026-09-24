using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public enum BattleMovementPurpose { Player, Return, Automatic }

// A finite route delivery, distinct from committed combatant placement. The
// current battle is the only live gameplay state; no future battle is retained.
public sealed class BattleMovementState
{
    internal BattleMovementState(ActorRef actor, IReadOnlyList<MapPosition> path, int segment,
        WaitToken token, BattleMovementPurpose purpose, ActorRef? target = null)
    { Actor = actor; Path = path; Segment = segment; Token = token; Purpose = purpose; Target = target; }
    public ActorRef Actor { get; }
    public IReadOnlyList<MapPosition> Path { get; }
    public int Segment { get; }
    public WaitToken Token { get; }
    public BattleMovementPurpose Purpose { get; }
    public MapPosition From => Path[Segment];
    public MapPosition To => Path[Segment + 1];
    public int Facing => To.X > From.X ? 0 : To.Y < From.Y ? 1 : To.X < From.X ? 2 : 3;
    public PresentationCueKind CompletionKind => PresentationCueKind.Gesture;
    internal ActorRef? Target { get; }
}

internal static class BattleMovementContinuation
{
    internal static SessionResult BeginPlayer(SessionSnapshot current, BattleSelection next,
        IReadOnlyList<MapPosition> path, BattleMovementPurpose purpose, string selectionEvent)
    {
        long revision = checked(current.Revision + 1), sequence = current.ObservationSequence;
        List<SessionObservation> observations = [new(++sequence, revision, selectionEvent, next.Actor)];
        BattleMovementState? movement = null;
        if (path.Count > 1)
        {
            observations.Add(new(++sequence, revision, "battle-movement-segment-started", next.Actor,
                From: path[0], To: path[1], Detail: purpose.ToString()));
            movement = new(next.Actor, path, 0, new(sequence), purpose);
        }
        return Result(current, current.Battle, next, movement, revision, sequence, observations);
    }

    internal static SessionResult BeginAutomatic(SessionSnapshot current, ActorRef actor,
        BattleAutomaticAction action, List<SessionObservation> observations)
    {
        long revision = checked(current.Revision + 1), sequence = current.ObservationSequence;
        foreach (var effect in action.Effects)
            observations.Add(new(++sequence, revision, effect.Kind, effect.Actor, effect.Before, effect.After,
                RandomRange: effect.RandomRange, RandomValue: effect.RandomValue, Target: effect.Target));
        var decided = new SessionSnapshot(current.SessionId, revision, sequence,
            new ActiveBattle(action.Battle, null), current.Story, SessionStopReason.SimulationWait);
        if (action.Path.Count <= 1)
            return FinishAutomatic(decided, actor, action.Destination, action.Target, observations);
        observations.Add(new(++sequence, revision, "battle-movement-segment-started", actor,
            From: action.Path[0], To: action.Path[1], Detail: BattleMovementPurpose.Automatic.ToString()));
        return Result(current, action.Battle, null, new(actor, action.Path, 0, new(sequence),
            BattleMovementPurpose.Automatic, action.Target), revision, sequence, observations);
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command)
    {
        var movement = current.BattleMovement!;
        if (command is not CompletePresentation completion || completion.Wait != movement.Token ||
            completion.Kind != movement.CompletionKind)
            return BattleCommandDispatcher.Reject(current, "movement-pending", "command");
        long revision = checked(current.Revision + 1), sequence = current.ObservationSequence;
        List<SessionObservation> observations = [new(++sequence, revision, "battle-movement-segment-arrived",
            movement.Actor, From: movement.From, To: movement.To, Detail: movement.Purpose.ToString())];
        int next = movement.Segment + 1;
        if (next < movement.Path.Count - 1)
        {
            observations.Add(new(++sequence, revision, "battle-movement-segment-started", movement.Actor,
                From: movement.Path[next], To: movement.Path[next + 1], Detail: movement.Purpose.ToString()));
            return Result(current, current.Battle, current.Selection, new(movement.Actor, movement.Path,
                next, new(sequence), movement.Purpose, movement.Target), revision, sequence, observations);
        }
        observations.Add(new(++sequence, revision, "battle-movement-finished", movement.Actor,
            From: movement.Path[0], To: movement.To, Detail: movement.Purpose.ToString()));
        var finished = Result(current, current.Battle, current.Selection, null, revision, sequence, observations);
        if (movement.Purpose != BattleMovementPurpose.Automatic) return finished;
        var automatic = FinishAutomatic(finished.Snapshot, movement.Actor, movement.To, movement.Target, observations);
        return automatic.Snapshot.BattleScene is not null ? automatic : BattleAdvancer.Advance(automatic.Snapshot, observations);
    }

    private static SessionResult FinishAutomatic(SessionSnapshot current, ActorRef actor, MapPosition destination,
        ActorRef? target, List<SessionObservation> observations)
    {
        // The decision and its preflight ran once before movement. Only now does
        // actual action construction publish its main RNG and scene/reward work.
        if (target is { } victim)
            return BattleSceneContinuation.Begin(current, PhysicalBattleAction.Prepare(current.Battle, actor, destination, victim), observations);
        // The exact source path/stop was admitted before delivery. No other
        // gameplay command could change occupancy while its continuation lived.
        var moved = current.Battle.With(actors: current.Battle.Actors.Select(a =>
            a.Actor == actor ? a.With(position: destination) : a));
        var committed = BattleActionCommitter.Publish(current, moved, actor, destination, [], observations).WithStory(current.Story);
        return new(committed, observations.AsReadOnly(), committed.StopReason);
    }

    private static SessionResult Result(SessionSnapshot current, EngineBattleState battle, BattleSelection? selection,
        BattleMovementState? movement, long revision, long sequence, List<SessionObservation> observations)
    {
        var reason = movement is not null ? SessionStopReason.PresentationWait :
            selection is null ? SessionStopReason.SimulationWait : SessionStopReason.PlayerInput;
        var snapshot = new SessionSnapshot(current.SessionId, revision, sequence,
            new ActiveBattle(battle, selection, Movement: movement), current.Story, reason);
        return new(snapshot, observations.AsReadOnly(), reason);
    }
}

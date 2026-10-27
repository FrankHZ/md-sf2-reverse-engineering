using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Battles;

// The reset list is already committed. Cursor counts actual appearances delivered;
// a completed cursor retains that prefix if subsequent queue generation fails.
public sealed class BattleRoundState
{
    internal BattleRoundState(IReadOnlyList<ActorRef> appearances, int cursor, WaitToken token)
    { Appearances = appearances; Cursor = cursor; Token = token; }
    public IReadOnlyList<ActorRef> Appearances { get; }
    public int Cursor { get; }
    public WaitToken Token { get; }
    public ActorRef? Actor => Cursor < Appearances.Count ? Appearances[Cursor] : null;
    public PresentationCueKind CompletionKind => PresentationCueKind.EntityEffect;
}

internal static class BattleRoundContinuation
{
    internal static SessionResult Begin(SessionSnapshot current, List<SessionObservation> observations)
    {
        var battle = current.Battle;
        long revision = current.Revision + 1, sequence = current.ObservationSequence;
        IReadOnlyList<ActorRef> appearances = [];
        if (battle.Definition.Initialization is not null)
        {
            battle = BattleActivationRules.BeforeRound(battle);
            observations.Add(new(++sequence, revision, "regions-tested", After: battle.Regions!.Tested));
            observations.Add(new(++sequence, revision, "region-program-none"));
            var batch = BattleSpawnRules.Populate(battle);
            battle = batch.Battle; appearances = batch.Appearances;
            foreach (var effect in batch.Effects)
                observations.Add(new(++sequence, revision, effect.Kind, effect.Actor, effect.Before, effect.After));
            observations.Add(new(++sequence, revision, "spawn-list-ready", After: appearances.Count));
        }
        if (appearances.Count > 0)
            observations.Add(new(++sequence, revision, "appearance-started", appearances[0]));
        var pending = Snapshot(current, battle, new(appearances, 0, new(sequence)), revision, sequence);
        return appearances.Count > 0 ? new(pending, observations.AsReadOnly(), pending.StopReason) : Generate(pending, observations);
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command, SessionRules rules)
    {
        var round = current.BattleRound!;
        if (round.Actor is null || command is not CompletePresentation completion ||
            completion.Wait != round.Token || completion.Kind != round.CompletionKind)
            return BattleCommandDispatcher.Reject(current, "appearance-pending", "command");
        long revision = current.Revision + 1, sequence = current.ObservationSequence;
        List<SessionObservation> observations = [new(++sequence, revision, "appearance-completed", round.Actor)];
        int next = round.Cursor + 1;
        if (next < round.Appearances.Count)
            observations.Add(new(++sequence, revision, "appearance-started", round.Appearances[next]));
        var pending = Snapshot(current, current.Battle, new(round.Appearances, next, new(sequence)), revision, sequence);
        if (next < round.Appearances.Count) return new(pending, observations.AsReadOnly(), pending.StopReason);
        var generated = Generate(pending, observations);
        return generated.Failure is not null ? generated : BattleAdvancer.Advance(generated.Snapshot, observations, rules);
    }

    private static SessionSnapshot Snapshot(SessionSnapshot current, EngineBattleState battle, BattleRoundState round,
        long revision, long sequence) => new(current.SessionId, revision, sequence,
            new ActiveBattle(battle, null, Round: round), current.Story, SessionStopReason.PresentationWait);

    private static SessionResult Generate(SessionSnapshot current, List<SessionObservation> observations)
    {
        try
        {
            var previous = current.Battle;
            var battle = BattleTurnFlow.GenerateQueue(previous, out var generated);
            long revision = current.Revision + 1, sequence = current.ObservationSequence;
            observations.Add(new(++sequence, revision, "round-started", Before: previous.Round, After: battle.Round));
            observations.Add(new(++sequence, revision, "round-rng", Before: previous.MainSeed, After: battle.MainSeed, TurnGeneration: new(
                current.SessionId, battle.Round, previous.MainSeed, battle.MainSeed,
                Array.AsReadOnly(generated.Candidates.Select(c => new BattleTurnCandidate(c.Actor,
                    c.ProcessingOrder, c.Placed, c.Hp, c.Agility, c.ExtraRoundAction)).ToArray()),
                Array.AsReadOnly(generated.Draws.Select(d => new BattleTurnDraw(d.Actor, d.Turn,
                    d.Index, d.Range, d.Draw.Before, d.Draw.After, d.Draw.Value)).ToArray()),
                Array.AsReadOnly(generated.Unsorted.Select(s => new BattleTurnSlot(s.Actor, s.AlteredAgility)).ToArray()),
                Array.AsReadOnly(generated.Slots.Select(s => new BattleTurnSlot(s.Actor, s.AlteredAgility)).ToArray()))));
            var snapshot = new SessionSnapshot(current.SessionId, revision, sequence,
                new ActiveBattle(battle, null), current.Story, SessionStopReason.SimulationWait);
            return new(snapshot, observations.AsReadOnly(), snapshot.StopReason);
        }
        catch (Exception error) when (error is ArgumentException or OverflowException or BattleRuleException)
        {
            var failure = error is BattleRuleException rule ? BattleChoices.Failure(rule) :
                new SessionFailure(SessionFailureKind.InvariantFailure, "round-generation", "battle.queue", "Turn generation failed.");
            var failed = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
                current.Active, current.Story, SessionStopReason.Faulted);
            return new(failed, observations.AsReadOnly(), failed.StopReason, failure);
        }
    }
}

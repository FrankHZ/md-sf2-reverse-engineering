using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Battles;

public enum BattleRetreatPhase { Text, Effect, Close, Return }
public sealed record BattleRetreatState(ActorRef Actor, RetreatPayment Payment, WaitToken Token,
    BattleRetreatPhase Phase = BattleRetreatPhase.Text, bool Revealed = false)
{
    public byte? PaidMp => (Payment as SpellRetreatPayment)?.MpCost;
    public string Text => TextFor(Actor.Value);
    public string TextFor(string actorName) => Payment switch
    {
        SpellRetreatPayment spell => $"{actorName} cast {spell.Spell.Value.ToUpperInvariant()} level {spell.Spell.Level}!",
        ItemRetreatPayment item => $"{actorName} used {item.Name}!",
        _ => throw new InvalidOperationException("Unknown retreat payment."),
    };
    public PresentationCueKind CompletionKind => Phase == BattleRetreatPhase.Effect
        ? PresentationCueKind.EntityEffect : PresentationCueKind.FadeOut;
}

internal static class BattleRetreatContinuation
{
    internal static SessionResult Begin(SessionSnapshot current, BattleActionResolution action)
    {
        var actor = action.Prepared.GetActor(action.Actor);
        var payment = action.Retreat!;
        var battle = payment is SpellRetreatPayment spell
            ? action.Prepared.With(actors: action.Prepared.Actors.Select(value => value.Actor == action.Actor
                ? value.With(mp: checked((byte)(actor.Mp - spell.MpCost))) : value))
            : action.Prepared;
        long revision = checked(current.Revision + 1), sequence = current.ObservationSequence;
        List<SessionObservation> observations = [];
        if (current.Battle.GetActor(action.Actor).Position != action.Destination)
            observations.Add(new(++sequence, revision, "movement", action.Actor,
                From: current.Battle.GetActor(action.Actor).Position, To: action.Destination));
        if (payment is SpellRetreatPayment paidSpell)
            observations.Add(new(++sequence, revision, "retreat-mp-paid", action.Actor, actor.Mp, actor.Mp - paidSpell.MpCost));
        else if (payment is ItemRetreatPayment item)
            observations.Add(new(++sequence, revision, "retreat-item-paid", action.Actor, item.Word, item.Slot));
        else throw new InvalidOperationException("Unknown retreat payment.");
        observations.Add(new(++sequence, revision, "retreat-text-requested", action.Actor));
        var retreat = new BattleRetreatState(action.Actor, payment, new(sequence));
        var snapshot = new SessionSnapshot(current.SessionId, revision, sequence,
            new ActiveBattle(battle, null, Retreat: retreat), current.Story, SessionStopReason.PresentationWait);
        return new(snapshot, observations.AsReadOnly(), snapshot.StopReason);
    }

    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current,
        SessionCommand command, SessionRules rules)
    {
        var retreat = current.BattleRetreat!;
        if (current.StopReason is SessionStopReason.Faulted or SessionStopReason.Unsupported)
            return BattleCommandDispatcher.Reject(current, "session-stopped", "command");
        List<SessionObservation> observations = [];
        BattleRetreatState next;
        string kind;
        switch (command)
        {
            case CompleteTextReveal reveal when retreat.Phase == BattleRetreatPhase.Text &&
                reveal.Wait == retreat.Token && !retreat.Revealed:
                next = retreat with { Revealed = true }; kind = "retreat-text-revealed"; break;
            case Acknowledge ack when retreat.Phase == BattleRetreatPhase.Text &&
                ack.Wait == retreat.Token && retreat.Revealed:
                next = retreat with { Phase = BattleRetreatPhase.Effect }; kind = "retreat-effect-requested"; break;
            case CompletePresentation completed when retreat.Phase is BattleRetreatPhase.Effect or BattleRetreatPhase.Close &&
                completed.Wait == retreat.Token && completed.Kind == retreat.CompletionKind:
                next = retreat with { Phase = retreat.Phase == BattleRetreatPhase.Effect ? BattleRetreatPhase.Close : BattleRetreatPhase.Return };
                kind = next.Phase == BattleRetreatPhase.Close ? "retreat-close-requested" : "retreat-closed"; break;
            default: return BattleCommandDispatcher.Reject(current, "stale-or-wrong-retreat-wait", "wait");
        }
        long revision = checked(current.Revision + 1), sequence = checked(current.ObservationSequence + 1);
        // Reveal retains its token; each subsequent service owns a distinct one.
        if (next.Phase != retreat.Phase) next = next with { Token = new(sequence) };
        observations.Add(new(sequence, revision, kind, retreat.Actor));
        current = new(current.SessionId, revision, sequence,
            new ActiveBattle(current.Battle, null, Retreat: next), current.Story, SessionStopReason.PresentationWait);
        return next.Phase == BattleRetreatPhase.Return
            ? BattleOutcome.ReturnFromRetreat(definition, current, observations, rules)
            : new(current, observations.AsReadOnly(), current.StopReason);
    }
}

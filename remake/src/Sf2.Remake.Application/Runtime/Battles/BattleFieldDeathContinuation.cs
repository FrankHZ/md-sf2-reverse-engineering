using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

// Retained source caller/accounting operands, not a synthetic physical action.
internal sealed record FieldDeathTurn(ActorRef Actor, MapPosition? Destination, BattleDeathCredit Credit);

internal static class BattleFieldDeathContinuation
{
    internal static SessionSnapshot Begin(SessionSnapshot current, FieldDeathTurn turn, BattleDeathBatch deaths,
        List<SessionObservation> observations)
    {
        long sequence = current.ObservationSequence;
        observations.Add(new(++sequence, current.Revision, "field-death-started", turn.Actor));
        observations.Add(new(++sequence, current.Revision, "scene-step-started", turn.Actor, Detail: BattleScenePhase.FieldSpin.ToString()));
        return Snapshot(current, current.Battle, new(null, BattleScenePhase.FieldSpin, 0, new(sequence),
            deaths: deaths, fieldDeath: turn), current.Revision, sequence);
    }

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command, SessionRules rules)
    {
        var scene = current.BattleScene!;
        if (command is not CompletePresentation completion || completion.Wait != scene.Token || completion.Kind != scene.CompletionKind)
            return BattleCommandDispatcher.Reject(current, "battle-scene-wait", "command");
        try
        {
            var turn = scene.FieldDeath!;
            var battle = current.Battle;
            long revision = current.Revision + 1, sequence = current.ObservationSequence;
            List<SessionObservation> observations = [new(++sequence, revision, "scene-step-completed", turn.Actor, Detail: scene.Phase.ToString())];
            int step = scene.FieldStep;
            BattleScenePhase next;
            switch (scene.Phase)
            {
                case BattleScenePhase.FieldSpin:
                    if (step < 11) { step++; next = BattleScenePhase.FieldSpin; }
                    else
                    {
                        next = BattleScenePhase.FieldExit; step = 0;
                        observations.Add(new(++sequence, revision, "field-death-sound", turn.Actor, After: 116));
                    }
                    break;
                case BattleScenePhase.FieldExit:
                    if (step < 2) { step++; next = BattleScenePhase.FieldExit; }
                    else
                    {
                        var cleanup = scene.Deaths.Clean(battle, turn.Credit.Ally, rules.Progression);
                        battle = cleanup.Battle;
                        foreach (var effect in cleanup.Effects)
                            observations.Add(new(++sequence, revision, effect.Kind, effect.Actor, effect.Before, effect.After));
                        next = BattleScenePhase.FieldSettle; step = 0;
                    }
                    break;
                case BattleScenePhase.FieldSettle:
                    observations.Add(new(++sequence, revision, "field-death-ended", turn.Actor));
                    var ended = new SessionSnapshot(current.SessionId, revision, sequence,
                        new ActiveBattle(battle, null), current.Story, SessionStopReason.SimulationWait);
                    var committed = BattleActionCommitter.Publish(ended, battle, turn.Actor, turn.Destination,
                        [], observations, rules, defeatedHookHandled: true);
                    return BattleAdvancer.Advance(committed, observations, rules);
                default: throw new InvalidOperationException("Unknown field death phase.");
            }
            observations.Add(new(++sequence, revision, "scene-step-started", turn.Actor, Detail: next.ToString()));
            var state = new BattleSceneState(null, next, 0, new(sequence), deaths: scene.Deaths, fieldStep: step, fieldDeath: turn);
            return new(Snapshot(current, battle, state, revision, sequence), observations.AsReadOnly(), SessionStopReason.PresentationWait);
        }
        // A failed later step keeps the accepted presentation/accounting prefix and live token.
        catch (BattlePolicyFault error) { return new(current, [], current.StopReason, BattleChoices.Failure(error)); }
        catch (BattleRuleException error) { return BattleCommandDispatcher.Reject(current, error.Code, error.Field, error.Unsupported); }
    }

    private static SessionSnapshot Snapshot(SessionSnapshot current, EngineBattleState battle, BattleSceneState scene,
        long revision, long sequence) => new(current.SessionId, revision, sequence,
            new ActiveBattle(battle, null, scene), current.Story, SessionStopReason.PresentationWait);
}

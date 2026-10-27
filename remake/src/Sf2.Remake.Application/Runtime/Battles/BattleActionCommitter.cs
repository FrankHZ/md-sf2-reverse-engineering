using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

internal static class BattleActionCommitter
{
    // Called only after an entire player or enemy ACTION has resolved on temporary state.
    internal static SessionSnapshot Publish(SessionSnapshot current, EngineBattleState battle,
        ActorRef actor, MapPosition? destination, IReadOnlyList<BattleEffect> effects, List<SessionObservation> observations, SessionRules rules, bool defeatedHookHandled = false, bool sceneEnded = false, BattleDeathBatch? deaths = null)
    {
        // The no-scene path is ExecuteIndividualTurn @NoAction. Scene completion
        // already performed LoadBattle's whole-field reset before death processing.
        if (!defeatedHookHandled && !sceneEnded)
            battle = battle.With(actors: battle.Actors.Select(value =>
                value.Actor == actor && value.Position is not null ? value.With(facing: 3) : value));
        bool hook = !defeatedHookHandled && BattleOutcomeSelection.DefeatedHook(rules.Outcome, battle);
        deaths ??= BattleDeathBatch.Empty;
        if (hook) deaths = deaths.AppendSurvivors(battle);
        if (deaths.Actors.Count > 0 && (battle.FirstAlly is not { } first ||
            !battle.GetActor(first).IsAlly))
            throw new BattleRuleException("death-first-ally-context", "battle.firstAlly", true);
        // Outcome must not run before the ordered removal is committed.
        var outcome = deaths.Actors.Count == 0 ? BattleOutcomeSelection.Check(rules.Outcome, battle) : null;
        long revision = checked(current.Revision + 1), sequence = current.ObservationSequence;
        var origin = current.Battle.GetActor(actor).Position;
        if (!defeatedHookHandled && !sceneEnded && origin != destination)
            observations.Add(new(++sequence, revision, "movement", actor, From: origin, To: destination));
        foreach (var effect in effects.Where(effect => effect.Kind is not ("death-cleanup" or "kills" or "defeats")))
            observations.Add(new(++sequence, revision, effect.Kind, effect.Actor, effect.Before, effect.After,
                RandomRange: effect.RandomRange, RandomValue: effect.RandomValue, Target: effect.Target));
        if (hook)
            observations.Add(new(++sequence, revision, current.Story.EnteringBattle?.CompletedFlag is { } completed &&
                current.Story.Flags.Contains(completed) ? "enemy-defeated-program-skipped" : "enemy-defeated-program-none"));
        foreach (var effect in effects.Where(effect => effect.Kind is "death-cleanup" or "kills" or "defeats"))
            observations.Add(new(++sequence, revision, effect.Kind, effect.Actor, effect.Before, effect.After));
        if (deaths.Actors.Count > 0)
        {
            var pending = new SessionSnapshot(current.SessionId, revision, sequence,
                new ActiveBattle(battle, null), current.Story, SessionStopReason.SimulationWait);
            return BattleFieldDeathContinuation.Begin(pending, new(actor, destination, battle.FirstAlly!.Value), deaths, observations);
        }
        bool sleep = outcome is null && battle.GetActor(actor).Hp > 0 && (battle.GetActor(actor).Status & 0xC0) != 0;
        if (outcome is null && !sleep) observations.Add(new(++sequence, revision, "after-turn", actor));
        else if (outcome is not null) observations.Add(new(++sequence, revision, "battle-outcome", Detail: outcome.Value.ToString()));
        if (battle.MainSeed != current.Battle.MainSeed)
            observations.Add(new(++sequence, revision, "action-rng", actor, current.Battle.MainSeed, battle.MainSeed));
        observations.Add(new(++sequence, revision, "action-committed", actor));
        var published = new SessionSnapshot(current.SessionId, revision, sequence,
            new ActiveBattle(battle, null), current.Story, SessionStopReason.SimulationWait);
        if (sleep) return BattleAfterTurnContinuation.Begin(published, actor, observations);
        return new(current.SessionId, revision, sequence,
            new ActiveBattle(outcome is null ? BattleTurnFlow.ConsumeEntry(battle) : battle, null), current.Story, SessionStopReason.SimulationWait);
    }
}

using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

internal static class BattleAdvancer
{
    internal static SessionResult Start(BattleDefinition definition, BattleStartInput start) =>
        Start(definition, start, Gameplay.RuleCompositions.Sf2().Bind([definition], start.Actors));

    internal static SessionResult Start(BattleDefinition definition, BattleStartInput start, SessionRules rules)
    {
        foreach (var deployment in definition.Deployments)
            if (start.Actors.FirstOrDefault(input => input.Actor == deployment.Actor) is { } input)
                rules.RequireStart(deployment, input);
        var battle = BattleTurnFlow.Start(definition, start);
        List<SessionObservation> observations = [];
        if (start.NewBattle is not null) observations.Add(new(1, 0, "new-battle-initialized"));
        var result = Advance(new(Guid.NewGuid(), 0, observations.Count, battle,
            null, SessionStopReason.SimulationWait), observations, rules);
        // A private new-battle entry is one transaction through supported initial control.
        if (start.NewBattle is not null && result.Failure is { } failure)
            throw new BattleRuleException(failure.Code, failure.Field,
                failure.Kind == SessionFailureKind.UnsupportedCapability);
        return result;
    }

    internal static SessionResult Advance(SessionSnapshot current, List<SessionObservation> observations) =>
        Advance(current, observations, Gameplay.RuleCompositions.Sf2().Bind([current.Battle.Definition]));

    internal static SessionResult Advance(SessionSnapshot current, List<SessionObservation> observations, SessionRules rules)
    {
        if (current.BattleScene is not null || current.BattleMovement is not null)
            return new(current, observations.AsReadOnly(), SessionStopReason.PresentationWait);
        var battle = current.Battle;
        long revision = current.Revision, sequence = current.ObservationSequence;
        // A bounded host tick yields real automatic work; the next frame resumes through the same facade.
        for (int steps = 0; steps < 256; steps++)
        {
            try
            {
                if (BattleOutcomeRules.Check(battle) is not null)
                    return new(new(current.SessionId, revision, sequence, battle, null, SessionStopReason.SimulationWait),
                        observations.AsReadOnly(), SessionStopReason.SimulationWait);
                if (BattleTurnFlow.AtRoundEnd(battle))
                {
                    var previous = battle;
                    battle = BattleTurnFlow.GenerateRound(battle, out var generated);
                    revision++;
                    if (battle.Regions is { } regions)
                    {
                        observations.Add(new(++sequence, revision, "regions-tested", After: regions.Tested));
                        observations.Add(new(++sequence, revision, "region-program-none"));
                        observations.Add(new(++sequence, revision, "spawn-modes-admitted"));
                    }
                    observations.Add(new(++sequence, revision, "round-started", Before: previous.Round, After: battle.Round));
                    observations.Add(new(++sequence, revision, "round-rng", Before: previous.MainSeed, After: battle.MainSeed, TurnGeneration: new(
                        current.SessionId, battle.Round, previous.MainSeed, battle.MainSeed,
                        Array.AsReadOnly(generated.Candidates.Select(c => new BattleTurnCandidate(c.Actor,
                            c.ProcessingOrder, c.Placed, c.Hp, c.Agility, c.ExtraRoundAction)).ToArray()),
                        Array.AsReadOnly(generated.Draws.Select(d => new BattleTurnDraw(d.Actor, d.Turn,
                            d.Index, d.Range, d.Draw.Before, d.Draw.After, d.Draw.Value)).ToArray()),
                        Array.AsReadOnly(generated.Unsorted.Select(s => new BattleTurnSlot(s.Actor, s.AlteredAgility)).ToArray()),
                        Array.AsReadOnly(generated.Slots.Select(s => new BattleTurnSlot(s.Actor, s.AlteredAgility)).ToArray()))));
                    continue;
                }
                var actor = BattleTurnFlow.QueuedActor(battle);
                if (actor.Hp == 0)
                {
                    battle = BattleTurnFlow.ConsumeEntry(battle); revision++;
                    observations.Add(new(++sequence, revision, "dead-entry-skipped", actor.Actor));
                    continue;
                }
                if (actor.Control == BattleControl.Player)
                {
                    var controlled = BattleControlRules.EnterPlayer(battle, actor.Actor);
                    var preview = BattleMovement.Preview(controlled, actor.Actor, actor.Position!);
                    battle = controlled;
                    revision++;
                    observations.Add(new(++sequence, revision, "player-control", actor.Actor));
                    var selection = new BattleSelection(actor.Actor, preview, BattleSelectionStage.Movement);
                    var snapshot = new SessionSnapshot(current.SessionId, revision, sequence, battle, selection, SessionStopReason.PlayerInput);
                    return new(snapshot, observations.AsReadOnly(), SessionStopReason.PlayerInput);
                }
                var beforeAction = new SessionSnapshot(current.SessionId, revision, sequence, battle, null, SessionStopReason.SimulationWait);
                if (actor.Control != BattleControl.Automatic || actor.AiStrategy is not { } strategy)
                    throw new BattleRuleException("control-ai", "placements.control/aiStrategy", true);
                var action = BattleDecisionRules.Decide(rules.Decision(strategy), battle, actor.Actor, rules.Physical);
                var delivery = BattleMovementContinuation.BeginAutomatic(beforeAction.WithStory(current.Story), actor.Actor, action, observations, rules);
                if (delivery.Failure is not null || delivery.Snapshot.BattleMovement is not null || delivery.Snapshot.BattleScene is not null) return delivery;
                var committed = delivery.Snapshot;
                battle = committed.Battle; revision = committed.Revision; sequence = committed.ObservationSequence;
            }
            catch (BattleDecisionRuleFault error)
            {
                return new(new(current.SessionId, revision, sequence, battle, null, SessionStopReason.Faulted),
                    observations.AsReadOnly(), SessionStopReason.Faulted,
                    new(SessionFailureKind.InvariantFailure, "decision-rule-failure", "rules.decisions",
                        $"{error.Identity}: {error.Operation}"));
            }
            catch (BattleActionRuleFault error)
            {
                return new(new(current.SessionId, revision, sequence, battle, null, SessionStopReason.Faulted),
                    observations.AsReadOnly(), SessionStopReason.Faulted, BattleChoices.Failure(error));
            }
            catch (BattleRuleException error)
            {
                // Keep the last committed state. A rejected round, control admission or
                // enemy action cannot publish partial changes or random draws.
                var reason = error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Faulted;
                return new(new(current.SessionId, revision, sequence, battle, null, reason), observations.AsReadOnly(), reason,
                    new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.InvariantFailure,
                        error.Code, error.Field, error.Code.Replace('-', ' ')));
            }
        }
        return new(new(current.SessionId, revision, sequence, battle, null, SessionStopReason.SimulationWait),
            observations.AsReadOnly(), SessionStopReason.SimulationWait);
    }
}

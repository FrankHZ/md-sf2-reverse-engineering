using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

internal static class BattleCommandDispatcher
{
    internal static SessionResult Reject(SessionSnapshot current, string code, string field, bool unsupported = false) =>
        new(current, [], unsupported ? SessionStopReason.Unsupported : SessionStopReason.Rejected,
            new(unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
                code, field, code.Replace('-', ' ')));

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command)
    {
        if (command is AdvanceSimulation)
            return current.StopReason == SessionStopReason.SimulationWait
                ? BattleAdvancer.Advance(current, []) : Reject(current, "not-waiting", "command");
        if (current.Selection is not { } selection) return Reject(current, "not-player-control", "phase");
        var battle = current.Battle;
        var actor = battle.GetActor(selection.Actor);
        try
        {
            switch (command)
            {
                case Cancel:
                    return Selected(current, new(selection.Actor, BattleMovement.Preview(battle, selection.Actor, actor.Position!),
                        BattleSelectionStage.Movement), "selection-cancelled");
                case Move move when selection.Stage == BattleSelectionStage.Movement:
                    var delta = move.Direction switch
                    {
                        ExplorationDirection.North => (0, -1), ExplorationDirection.East => (1, 0),
                        ExplorationDirection.South => (0, 1), ExplorationDirection.West => (-1, 0),
                        _ => throw new BattleRuleException("invalid-direction", "direction"),
                    };
                    int x = selection.Preview.Destination.X + delta.Item1, y = selection.Preview.Destination.Y + delta.Item2;
                    if (x < 0 || y < 0 || x >= battle.Definition.Width || y >= battle.Definition.Height)
                        return Reject(current, "movement-range", "destination");
                    return Selected(current, new(selection.Actor, BattleMovement.Preview(battle, selection.Actor, new(x, y)),
                        BattleSelectionStage.Movement), "movement-preview");
                case Confirm when selection.Stage == BattleSelectionStage.Movement:
                    BattleMovement.RequireStop(battle, selection.Actor, selection.Preview.Destination);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.ActionChoice), "action-choice");
                case ChooseAction choice when selection.Stage == BattleSelectionStage.ActionChoice:
                    if (choice.Action == SessionAction.PhysicalAttack)
                    {
                        _ = PlayerPhysicalAttack.RequireActor(battle, selection.Actor);
                        return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.TargetChoice,
                            SessionAction.PhysicalAttack), "physical-selected");
                    }
                    if (choice.Action != SessionAction.Stay)
                        return Reject(current, choice.Action == SessionAction.Heal ? "select-spell" : "physical-attack",
                            "action", choice.Action != SessionAction.Heal);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.CommitReady,
                        SessionAction.Stay), "stay-selected");
                case SelectSpell spell when selection.Stage == BattleSelectionStage.ActionChoice:
                    _ = PlayerHealing.RequireSpell(battle, selection.Actor, spell.Spell);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.TargetChoice,
                        SessionAction.Heal, spell.Spell), "spell-selected");
                case SelectTarget physicalTarget when selection.Action == SessionAction.PhysicalAttack &&
                    selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady:
                    _ = PlayerPhysicalAttack.RequireTarget(battle, selection.Actor, selection.Preview.Destination, physicalTarget.Target);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.CommitReady,
                        SessionAction.PhysicalAttack, target: physicalTarget.Target), "target-selected");
                case SelectTarget target when selection.Spell is { } selectedSpell &&
                    selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady:
                    var definition = PlayerHealing.RequireSpell(battle, selection.Actor, selectedSpell);
                    _ = PlayerHealing.RequireTarget(battle, selection.Actor, selection.Preview.Destination, definition, target.Target);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.CommitReady,
                        SessionAction.Heal, selectedSpell, target.Target), "target-selected");
                case Confirm when selection.Stage == BattleSelectionStage.CommitReady:
                    return Commit(current, selection);
                default:
                    return Reject(current, "wrong-selection-stage", "phase");
            }
        }
        catch (BattleRuleException error) { return Reject(current, error.Code, error.Field, error.Unsupported); }
    }

    private static SessionResult Selected(SessionSnapshot current, BattleSelection selection, string kind)
    {
        var observation = new SessionObservation(current.ObservationSequence + 1, current.Revision + 1, kind, selection.Actor);
        var snapshot = new SessionSnapshot(current.SessionId, current.Revision + 1, observation.Sequence,
            current.Battle, selection, SessionStopReason.PlayerInput);
        return new(snapshot, Array.AsReadOnly([observation]), SessionStopReason.PlayerInput);
    }

    private static SessionResult Commit(SessionSnapshot current, BattleSelection selection)
    {
        var actor = current.Battle.GetActor(selection.Actor);
        EngineBattleState battle;
        IReadOnlyList<BattleEffect> effects = [];
        if (selection.Action == SessionAction.Heal && selection.Spell is { } spell && selection.Target is { } target)
            (battle, effects) = PlayerHealing.Resolve(current.Battle, selection.Actor, selection.Preview.Destination, spell, target);
        else if (selection.Action == SessionAction.Stay)
            battle = BattleMovement.Commit(current.Battle, selection.Actor, selection.Preview.Destination);
        else if (selection.Action == SessionAction.PhysicalAttack && selection.Target is { } physicalTarget)
            (battle, effects) = PlayerPhysicalAttack.Resolve(current.Battle, selection.Actor, selection.Preview.Destination, physicalTarget);
        else return Reject(current, "incomplete-action", "selection");
        long revision = checked(current.Revision + 1), sequence = current.ObservationSequence;
        var observations = new List<SessionObservation>();
        if (actor.Position != selection.Preview.Destination)
            observations.Add(new(++sequence, revision, "movement", selection.Actor, From: actor.Position, To: selection.Preview.Destination));
        foreach (var effect in effects)
            observations.Add(new(++sequence, revision, effect.Kind, effect.Actor, effect.Before, effect.After,
                RandomRange: effect.RandomRange, RandomValue: effect.RandomValue));
        if (battle.MainSeed != current.Battle.MainSeed)
            observations.Add(new(++sequence, revision, "action-rng", selection.Actor, current.Battle.MainSeed, battle.MainSeed));
        observations.Add(new(++sequence, revision, "action-committed", selection.Actor));
        return BattleAdvancer.Advance(new(current.SessionId, revision, sequence, BattleTurnFlow.ConsumeEntry(battle), null,
            SessionStopReason.SimulationWait), observations);
    }


}

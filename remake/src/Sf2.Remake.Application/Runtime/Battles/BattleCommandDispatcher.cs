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

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command,
        BattleSceneDefinition? sceneContent = null, bool requireSceneContent = false)
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
                        _ = PhysicalBattleAction.RequireActor(battle, selection.Actor);
                        return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.TargetChoice,
                            SessionAction.PhysicalAttack), "physical-selected");
                    }
                    if (choice.Action != SessionAction.Stay)
                        return Reject(current, choice.Action == SessionAction.Heal ? "select-spell" : choice.Action == SessionAction.Item ? "select-item" : "unknown-action",
                            "action", choice.Action is not (SessionAction.Heal or SessionAction.Item));
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.CommitReady,
                        SessionAction.Stay), "stay-selected");
                case SelectItem item when selection.Stage == BattleSelectionStage.ActionChoice ||
                    (selection.Action == SessionAction.Item && selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady):
                    _ = PlayerItemUse.RequireItem(battle, selection.Actor, item.Slot);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.TargetChoice,
                        SessionAction.Item, itemSlot: item.Slot), "item-selected");
                case SelectTarget itemTarget when selection.Action == SessionAction.Item && selection.ItemSlot is { } slot &&
                    selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady:
                    var itemDefinition = PlayerItemUse.RequireItem(battle, selection.Actor, slot);
                    _ = PlayerItemUse.RequireTarget(battle, selection.Actor, selection.Preview.Destination, itemDefinition, itemTarget.Target);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.CommitReady,
                        SessionAction.Item, target: itemTarget.Target, itemSlot: slot), "target-selected");
                case SelectSpell spell when selection.Stage == BattleSelectionStage.ActionChoice ||
                    (selection.Action == SessionAction.Heal && selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady):
                    _ = PlayerHealing.RequireSpell(battle, selection.Actor, spell.Spell);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.TargetChoice,
                        SessionAction.Heal, spell.Spell), "spell-selected");
                case SelectTarget physicalTarget when selection.Action == SessionAction.PhysicalAttack &&
                    selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady:
                    _ = PhysicalBattleAction.RequireTarget(battle, selection.Actor, selection.Preview.Destination, physicalTarget.Target);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.CommitReady,
                        SessionAction.PhysicalAttack, target: physicalTarget.Target), "target-selected");
                case SelectTarget target when selection.Spell is { } selectedSpell &&
                    selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady:
                    var definition = PlayerHealing.RequireSpell(battle, selection.Actor, selectedSpell);
                    _ = PlayerHealing.RequireTarget(battle, selection.Actor, selection.Preview.Destination, definition, target.Target);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.CommitReady,
                        SessionAction.Heal, selectedSpell, target.Target), "target-selected");
                case Confirm when selection.Stage == BattleSelectionStage.CommitReady:
                    return Commit(current, selection, sceneContent, requireSceneContent);
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

    private static SessionResult Commit(SessionSnapshot current, BattleSelection selection,
        BattleSceneDefinition? sceneContent, bool requireSceneContent)
    {
        EngineBattleState battle;
        IReadOnlyList<BattleEffect> effects = [];
        if (selection.Action == SessionAction.Heal && selection.Spell is { } spell && selection.Target is { } target)
        {
            if (requireSceneContent && sceneContent is null)
                throw new BattleRuleException("healing-scene-content", "battleScenes.healing", true);
            return BattleSceneContinuation.Begin(current, PlayerHealing.Prepare(current.Battle,
                selection.Actor, selection.Preview.Destination, spell, target), [], sceneContent: sceneContent);
        }
        else if (selection.Action == SessionAction.Item && selection.ItemSlot is { } slot && selection.Target is { } itemTarget)
            return BattleSceneContinuation.Begin(current, PlayerItemUse.Prepare(current.Battle,
                selection.Actor, selection.Preview.Destination, slot, itemTarget), []);
        else if (selection.Action == SessionAction.Stay)
            battle = BattleMovement.Commit(current.Battle, selection.Actor, selection.Preview.Destination);
        else if (selection.Action == SessionAction.PhysicalAttack && selection.Target is { } physicalTarget)
            return BattleSceneContinuation.Begin(current, PhysicalBattleAction.Prepare(current.Battle,
                selection.Actor, selection.Preview.Destination, physicalTarget), []);
        else return Reject(current, "incomplete-action", "selection");
        var observations = new List<SessionObservation>();
        return BattleAdvancer.Advance(BattleActionCommitter.Publish(current, battle, selection.Actor,
            selection.Preview.Destination, effects, observations), observations);
    }


}

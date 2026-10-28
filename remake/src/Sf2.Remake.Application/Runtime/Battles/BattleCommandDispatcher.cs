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
        BattleSceneDefinition? sceneContent = null, bool requireSceneContent = false) =>
        Submit(current, command, Gameplay.RuleCompositions.Sf2(), sceneContent, requireSceneContent);

    internal static SessionResult Submit(SessionSnapshot current, SessionCommand command, SessionRules rules,
        BattleSceneDefinition? sceneContent = null, bool requireSceneContent = false)
    {
        if (command is AdvanceSimulation)
            return current.StopReason == SessionStopReason.SimulationWait
                ? BattleAdvancer.Advance(current, [], rules) : Reject(current, "not-waiting", "command");
        if (current.Selection is not { } selection) return Reject(current, "not-player-control", "phase");
        var battle = current.Battle;
        var actor = battle.GetActor(selection.Actor);
        try
        {
            if (command is BeginGive || BattleGiveSelection.Active(selection.Stage))
                return BattleGiveSelection.Submit(current, command);
            switch (command)
            {
                case BeginEquipment when selection.Stage == BattleSelectionStage.ActionChoice:
                    var offers = BattleEquipment.Choices(actor);
                    if (offers.Count == 0) return Reject(current, "no-equippable-weapons", "equipment");
                    return Equipment(offers.FirstOrDefault(option => option.Equipped)?.Slot ?? offers[0].Slot);
                case SelectEquipment equipment when selection.Stage == BattleSelectionStage.EquipmentChoice:
                    return Equipment(equipment.Slot);
                case Cancel when selection.Stage == BattleSelectionStage.EquipmentChoice:
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.ActionChoice), "equipment-cancelled");
                case Confirm when selection.Stage == BattleSelectionStage.EquipmentChoice:
                    if (selection.EquipmentSlot is not { } equipmentSlot ||
                        equipmentSlot < 4 && actor.SourceLoadout?.Items.ElementAtOrDefault(equipmentSlot) != selection.ItemWord)
                        return Reject(current, "stale-equipment-selection", "equipment.slot");
                    var equipped = BattleEquipment.Equip(actor, equipmentSlot);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.ActionChoice),
                        "equipment-confirmed", battle.With(actors: battle.Actors.Select(a => a.Actor == actor.Actor ? equipped : a)));
                case Cancel:
                    return BattleMovementContinuation.BeginPlayer(current,
                        new(selection.Actor, BattleMovement.Preview(battle, selection.Actor, actor.Position!), BattleSelectionStage.Movement),
                        BattleMovement.ReturnPath(battle, selection.Actor, selection.Preview.Destination), BattleMovementPurpose.Return, "selection-cancelled");
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
                    return BattleMovementContinuation.BeginPlayer(current,
                        new(selection.Actor, BattleMovement.Preview(battle, selection.Actor, new(x, y)), BattleSelectionStage.Movement),
                        Array.AsReadOnly<MapPosition>([selection.Preview.Destination, new(x, y)]), BattleMovementPurpose.Player, "movement-preview");
                case Confirm when selection.Stage == BattleSelectionStage.Movement:
                    BattleMovement.RequireStop(battle, selection.Actor, selection.Preview.Destination);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.ActionChoice), "action-choice");
                case ChooseAction choice when selection.Stage == BattleSelectionStage.ActionChoice:
                    return choice.Action switch
                    {
                        SessionAction.PhysicalAttack => Select(new(BattleActionKind.Physical)),
                        SessionAction.Stay => Select(new(BattleActionKind.Stay)),
                        _ => Reject(current, choice.Action == SessionAction.Heal ? "select-spell" :
                            choice.Action == SessionAction.Item ? "select-item" : "unknown-action", "action",
                            choice.Action is not (SessionAction.Heal or SessionAction.Item)),
                    };
                case SelectSpell spell when CanSelect(BattleActionKind.Healing) || CanSelect(BattleActionKind.Retreat):
                    return Select(new(battle.Definition.RetreatSpells.ContainsKey(spell.Spell)
                        ? BattleActionKind.Retreat : BattleActionKind.Healing, spell.Spell));
                case SelectItem item when CanSelect(BattleActionKind.Item):
                    return Select(new(BattleActionKind.Item, ItemSlot: item.Slot));
                case SelectBattleAction choice when CanSelect(choice.Action.Kind):
                    return Select(choice.Action);
                case SelectTarget target when selection.ActionReference is { } action &&
                    selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady:
                    _ = BattleChoices.RequireTarget(rules, battle, selection.Actor, selection.Preview.Destination,
                        action, target.Target, sceneContent, requireSceneContent);
                    return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.CommitReady,
                        selection.Action, selection.Spell, target.Target, selection.ItemSlot, selection.ItemWord), "target-selected");
                case Confirm when selection.Stage == BattleSelectionStage.CommitReady:
                    return Commit(current, selection, sceneContent, requireSceneContent, rules);
                default:
                    return Reject(current, "wrong-selection-stage", "phase");
            }
        }
        catch (BattleRuleException error) { return Reject(current, error.Code, error.Field, error.Unsupported); }
        catch (BattlePolicyFault error)
        { return new(current, [], current.StopReason, BattleChoices.Failure(error)); }
        catch (BattleActionRuleFault error) { return new(current, [], SessionStopReason.Faulted, BattleChoices.Failure(error)); }

        SessionResult Equipment(int slot)
        {
            if (!BattleEquipment.Choices(actor).Any(option => option.Slot == slot))
                throw new BattleRuleException("equipment-slot", "equipment.slot");
            return Selected(current, new(selection.Actor, selection.Preview, BattleSelectionStage.EquipmentChoice,
                itemWord: slot < 4 ? actor.SourceLoadout!.Items[slot] : null, equipmentSlot: slot), "equipment-preview");
        }

        bool CanSelect(BattleActionKind kind) => selection.Stage == BattleSelectionStage.ActionChoice ||
            selection.ActionReference?.Kind == kind && selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady;
        SessionResult Select(BattleActionRef action)
        {
            _ = BattleChoices.RequireAction(rules, battle, selection.Actor, action);
            var kind = action.Kind switch
            {
                BattleActionKind.Retreat => SessionAction.Retreat,
                BattleActionKind.Healing => SessionAction.Heal, BattleActionKind.Physical => SessionAction.PhysicalAttack,
                BattleActionKind.Item => SessionAction.Item, _ => SessionAction.Stay,
            };
            string observation = action.Kind switch
            {
                BattleActionKind.Healing or BattleActionKind.Retreat => "spell-selected", BattleActionKind.Physical => "physical-selected",
                BattleActionKind.Item => "item-selected", _ => "stay-selected",
            };
            return Selected(current, new(selection.Actor, selection.Preview,
                action.Kind == BattleActionKind.Stay ? BattleSelectionStage.CommitReady : BattleSelectionStage.TargetChoice,
                kind, action.Spell, itemSlot: action.ItemSlot,
                itemWord: action.ItemSlot is { } slot ? actor.SourceLoadout!.Items[slot] : null), observation);
        }

    }

    internal static SessionResult Selected(SessionSnapshot current, BattleSelection selection, string kind, EngineBattleState? battle = null)
    {
        var observation = new SessionObservation(current.ObservationSequence + 1, current.Revision + 1, kind, selection.Actor);
        var snapshot = new SessionSnapshot(current.SessionId, current.Revision + 1, observation.Sequence,
            new ActiveBattle(battle ?? current.Battle, selection), current.Story, SessionStopReason.PlayerInput);
        return new(snapshot, Array.AsReadOnly([observation]), SessionStopReason.PlayerInput);
    }

    private static SessionResult Commit(SessionSnapshot current, BattleSelection selection,
        BattleSceneDefinition? sceneContent, bool requireSceneContent, SessionRules rules)
    {
        if (selection.ActionReference is not { } selected)
            return Reject(current, "incomplete-action", "selection");
        if (selected.ItemSlot is { } slot && (selection.ItemWord is null ||
            current.Battle.GetActor(selection.Actor).SourceLoadout?.Items.ElementAtOrDefault(slot) != selection.ItemWord))
            return Reject(current, "stale-item-selection", "item.slot");
        var action = BattleChoices.Prepare(rules, current.Battle, selection.Actor, selection.Preview.Destination,
            selected, selection.Target, sceneContent, requireSceneContent);
        if (action.Retreat is not null) return BattleRetreatContinuation.Begin(current, action);
        if (action.Reactions.Count > 0)
            return BattleSceneContinuation.Begin(current, action, [], sceneContent: sceneContent);
        var observations = new List<SessionObservation>();
        return BattleAdvancer.Advance(BattleActionCommitter.Publish(current, action.Prepared, selection.Actor,
            selection.Preview.Destination, action.CompletionEffects, observations, rules), observations, rules);
    }
}

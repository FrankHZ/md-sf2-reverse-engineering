using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public enum BattlePresentationKind { Healing, Physical, Item, None }
public sealed record BattleTargetChoice(ActorRef Actor, bool Enabled, SessionFailure? Reason);
public sealed record BattleActionChoice(BattleActionRef Action, string Label, BattlePresentationKind Presentation,
    byte? MinimumRange, byte? MaximumRange, bool Empty, bool Enabled, SessionFailure? Reason,
    IReadOnlyList<BattleTargetChoice> Targets);
public sealed record BattleSpellChoice(SpellRef Spell, string Label, BattlePresentationKind Presentation,
    byte? MinimumRange, byte? MaximumRange, bool Enabled, SessionFailure? Reason,
    IReadOnlyList<BattleTargetChoice> Targets);
public sealed record BattleItemChoice(int Slot, string Label, bool Empty, bool Enabled, SessionFailure? Reason,
    IReadOnlyList<BattleTargetChoice> Targets);
public sealed record BattleChoicesSnapshot(Guid SessionId, long Revision, ActorRef? Actor,
    BattleSelectionStage? Stage, MapPosition? Origin, IReadOnlyList<BattleActionChoice> Actions,
    SessionFailure? Failure = null)
{
    public IReadOnlyList<BattleSpellChoice> Spells => Array.AsReadOnly(Actions.Where(option => option.Action.Spell is not null)
        .Select(option => new BattleSpellChoice(option.Action.Spell!.Value, option.Label, option.Presentation,
            option.MinimumRange, option.MaximumRange, option.Enabled, option.Reason, option.Targets)).ToArray());
    public IReadOnlyList<BattleItemChoice> Items => Array.AsReadOnly(Actions.Where(option => option.Action.ItemSlot is not null)
        .Select(option => new BattleItemChoice(option.Action.ItemSlot!.Value, option.Label, option.Empty,
            option.Enabled, option.Reason, option.Targets)).ToArray());
}

internal static class BattleChoices
{
    internal static SessionFailure Failure(BattlePolicyFault error) =>
        new(SessionFailureKind.InvariantFailure, "battle-policy-failure", "rules", $"{error.Identity}: {error.Operation}");

    internal static SessionFailure Failure(BattleRuleException error) =>
        new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
            error.Code, error.Field, error.Code.Replace('-', ' '));

    internal static SessionFailure Failure(BattleActionRuleFault error) =>
        new(SessionFailureKind.InvariantFailure, error.Kind == BattleActionKind.Healing ? "healing-rule-invariant" : "action-rule-invariant",
            $"rules.{error.Kind.ToString().ToLowerInvariant()}.{error.Operation}",
            $"Action rule {error.Identity} failed at {error.Operation}.");

    internal static BattleActionAdmission RequireAction(SessionRules rules, EngineBattleState battle,
        ActorRef actor, BattleActionRef action)
    {
        if (!Enum.IsDefined(action.Kind)) throw new BattleRuleException("unknown-action", "action", true);
        var rule = rules.Action(action);
        return BattleActionRules.Invoke(rule, "action-admission", () =>
        {
            RequireReference(battle, actor, action);
            var admission = rule.RequireAction(battle, actor, action);
            if (action.Kind == BattleActionKind.Healing &&
                (admission.Spell is not { } spell || !battle.Definition.Spells.TryGetValue(action.Spell!.Value, out var known) || known != spell ||
                    admission.MinimumRange != spell.MinimumRange || admission.MaximumRange != spell.MaximumRange) ||
                action.Kind == BattleActionKind.Item &&
                (admission.Item is not { } item || !battle.Definition.HealingItems.TryGetValue(item.ItemId, out var knownItem) || knownItem != item ||
                    admission.MinimumRange != item.MinimumRange || admission.MaximumRange != item.MaximumRange) ||
                action.Kind == BattleActionKind.Stay && (admission.MinimumRange is not null || admission.MaximumRange is not null) ||
                action.Kind != BattleActionKind.Healing && admission.Spell is not null ||
                action.Kind != BattleActionKind.Item && admission.Item is not null)
                throw new InvalidOperationException("Invalid action admission.");
            return admission;
        });
    }

    private static void RequireReference(EngineBattleState battle, ActorRef actor, BattleActionRef action)
    {
        bool valid = action.Kind switch
        {
            BattleActionKind.Healing => action.ItemSlot is null && action.Spell is { } spell &&
                battle.GetActor(actor).Spells.Contains(spell),
            BattleActionKind.Item => action.Spell is null && action.ItemSlot is { } slot && slot >= 0 &&
                slot < (battle.GetActor(actor).SourceLoadout?.Items.Count ?? 0),
            _ => action.Spell is null && action.ItemSlot is null,
        };
        if (!valid) throw new BattleRuleException(action.Kind == BattleActionKind.Item ? "item-slot" : "spell-not-known", "action");
    }

    internal static BattleActorState RequireTarget(SessionRules rules, EngineBattleState battle, ActorRef actor,
        MapPosition destination, BattleActionRef action, ActorRef target, BattleSceneDefinition? content,
        bool requireContent)
    {
        _ = RequireAction(rules, battle, actor, action);
        var rule = rules.Action(action);
        return BattleActionRules.Invoke(rule, "target-admission", () =>
        {
            var patient = rule.RequireTarget(battle, actor, destination, action, target);
            if (!ReferenceEquals(patient, battle.GetActor(target))) throw new InvalidOperationException("Invalid admitted target.");
            if (action.Kind == BattleActionKind.Healing)
                HealingSceneCursor.RequireContent(battle, actor, target, content, requireContent);
            return patient;
        });
    }

    internal static BattleActionResolution Prepare(SessionRules rules, EngineBattleState battle, ActorRef actor,
        MapPosition destination, BattleActionRef action, ActorRef? target, BattleSceneDefinition? content = null,
        bool requireContent = false)
    {
        _ = RequireAction(rules, battle, actor, action);
        if (target is { } victim) _ = RequireTarget(rules, battle, actor, destination, action, victim, content, requireContent);
        return BattleActionRules.Prepare(rules.Action(action), battle, actor, destination, action, target, rules.Progression);
    }

    internal static BattleChoicesSnapshot Query(SessionSnapshot current, SessionRules rules,
        BattleSceneDefinition? content, bool requireContent)
    {
        var selection = current.Selection;
        if (!current.HasBattleControl || selection is null)
            return new(current.SessionId, current.Revision, null, null, null, [],
                new(SessionFailureKind.IllegalCommand, "not-player-control", "phase", "not player control"));
        var battle = current.Battle;
        List<BattleActionChoice> options = [];
        foreach (var rule in rules.Actions)
        {
            BattleActionOffer[] offers;
            try
            {
                offers = BattleActionRules.Invoke(rule, "action-query", () =>
                {
                    var result = rule.QueryActions(battle, selection.Actor).ToArray();
                    if (result.Select(offer => offer.Action).Distinct().Count() != result.Length ||
                        result.Any(offer => offer.Action.Kind != rule.Kind))
                        throw new InvalidOperationException("Invalid action query.");
                    foreach (var offer in result) RequireReference(battle, selection.Actor, offer.Action);
                    return result;
                });
            }
            catch (BattleRuleException error) { return Failed(Failure(error)); }
            catch (BattleActionRuleFault error) { return Failed(Failure(error)); }
            foreach (var offer in offers)
            {
                SessionFailure? failure = null;
                BattleActionAdmission? admission = null;
                List<BattleTargetChoice> targets = [];
                try
                {
                    BattleMovement.RequireStop(battle, selection.Actor, selection.Preview.Destination);
                    admission = RequireAction(rules, battle, selection.Actor, offer.Action);
                    var candidates = BattleActionRules.Invoke(rule, "target-query", () =>
                    {
                        var result = rule.QueryTargets(battle, selection.Actor, offer.Action).ToArray();
                        if (result.Distinct().Count() != result.Length || result.Any(actor => !battle.Actors.Any(row => row.Actor == actor)))
                            throw new InvalidOperationException("Invalid target query.");
                        return result;
                    });
                    foreach (var actor in candidates)
                    {
                        SessionFailure? reason = null;
                        try { _ = RequireTarget(rules, battle, selection.Actor, selection.Preview.Destination,
                            offer.Action, actor, content, requireContent); }
                        catch (BattleRuleException error) { reason = Failure(error); }
                        catch (BattleActionRuleFault error) { reason = Failure(error); }
                        targets.Add(new(actor, reason is null, reason));
                    }
                    if (offer.Action.Kind != BattleActionKind.Stay && !targets.Any(target => target.Enabled))
                        failure = targets.FirstOrDefault(target => target.Reason?.Kind == SessionFailureKind.UnsupportedCapability)?.Reason
                            ?? targets.FirstOrDefault(target => target.Reason?.Kind == SessionFailureKind.InvariantFailure)?.Reason
                            ?? new(SessionFailureKind.IllegalCommand, offer.Action.Kind == BattleActionKind.Healing ? "no-healing-target" : "no-action-target", "target", "no action target");
                }
                catch (BattleRuleException error) { failure = Failure(error); }
                catch (BattleActionRuleFault error) { failure = Failure(error); }
                var presentation = rule.Kind switch
                {
                    BattleActionKind.Healing => BattlePresentationKind.Healing,
                    BattleActionKind.Physical => BattlePresentationKind.Physical,
                    BattleActionKind.Item => BattlePresentationKind.Item,
                    _ => BattlePresentationKind.None,
                };
                options.Add(new(offer.Action, offer.Label, presentation, admission?.MinimumRange, admission?.MaximumRange,
                    offer.Empty, failure is null, failure, targets.AsReadOnly()));
            }
        }
        return new(current.SessionId, current.Revision, selection.Actor, selection.Stage,
            selection.Preview.Destination, options.AsReadOnly());

        BattleChoicesSnapshot Failed(SessionFailure failure) => new(current.SessionId, current.Revision,
            selection.Actor, selection.Stage, selection.Preview.Destination, options.AsReadOnly(), failure);
    }
}

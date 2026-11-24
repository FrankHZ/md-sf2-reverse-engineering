using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal sealed record BattleActionOffer(BattleActionRef Action, string Label, bool Empty = false);
internal sealed record BattleActionAdmission(byte? MinimumRange = null, byte? MaximumRange = null,
    HealingSpellDefinition? Spell = null, HealingItemDefinition? Item = null, RetreatSpellDefinition? Retreat = null,
    RetreatItemDefinition? RetreatItem = null, AttackSpellDefinition? AttackSpell = null,
    SupportSpellDefinition? SupportSpell = null, AttackItemDefinition? AttackItem = null, SupportItemDefinition? SupportItem = null);

// Queries are disposable and do not construct an action or consume either RNG stream.
internal interface IBattleActionRule
{
    string Identity { get; }
    BattleActionKind Kind { get; }
    IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor);
    BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action);
    IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action);
    BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target);
    BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression);

    IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action, OffensiveTargetSide side) =>
        side == OffensiveTargetSide.Opponents ? QueryTargets(battle, actor, action) : throw UnsupportedSide();
    BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target, OffensiveTargetSide side) =>
        side == OffensiveTargetSide.Opponents ? RequireTarget(battle, actor, destination, action, target) : throw UnsupportedSide();
    BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression, OffensiveTargetSide side) =>
        side == OffensiveTargetSide.Opponents ? Prepare(battle, actor, destination, action, target, progression) : throw UnsupportedSide();

    private static BattleRuleException UnsupportedSide() => new("offensive-target-side", "rules.action", true);
}

internal interface IPhysicalActionRule : IBattleActionRule
{
    // Movement may query geometry before an attack/reward consumer is reached.
    // Replacement physical rules retain their own admission unless they supply this query.
    BattleActionAdmission QueryRange(EngineBattleState battle, ActorRef actor) =>
        RequireAction(battle, actor, new(BattleActionKind.Physical));
    int EstimateDamage(EngineBattleState battle, ActorRef actor, ActorRef target);
}

internal static class BattleActionRules
{
    internal static T Invoke<T>(IBattleActionRule rule, string operation, Func<T> call)
    {
        try { return call(); }
        catch (BattleRuleException) { throw; }
        catch (BattlePolicyFault) { throw; }
        catch (BattleActionRuleFault) { throw; }
        catch (Exception) { throw new BattleActionRuleFault(rule.Kind, rule.Identity, operation); }
    }

    internal static BattleActionResolution Prepare(IBattleActionRule rule, EngineBattleState battle,
        ActorRef actor, MapPosition destination, BattleActionRef action, ActorRef? target,
        IBattleProgressionRule progression, OffensiveTargetSide side = OffensiveTargetSide.Opponents, bool guarded = false) =>
        Invoke(rule, "prepare", () =>
        {
            if (side == OffensiveTargetSide.OwnSide &&
                (BattleOffense.AutomaticSide(battle.GetActor(actor)) != side || target is not { } ownTarget ||
                 !BattleOffense.OwnTarget(battle, battle.GetActor(actor), ownTarget) || action.Kind is not (BattleActionKind.Physical or BattleActionKind.AttackSpell or BattleActionKind.Item)))
                throw new BattleRuleException("offensive-target-side", "action");
            if (guarded)
            {
                if (side != OffensiveTargetSide.OwnSide || destination != battle.GetActor(actor).Position)
                    throw new BattleRuleException("muddle-inaction", "action");
                return BattleOffense.PrepareGuarded(battle, actor, progression);
            }
            var admission = rule.RequireAction(battle, actor, action);
            // These spells require a selected center even when silence suppresses their effects.
            if (target is null)
            {
                var missingTarget = action.Kind switch
                {
                    BattleActionKind.Healing => "invalid-heal-target",
                    BattleActionKind.AttackSpell => "attack-spell-target",
                    BattleActionKind.SupportSpell => "support-spell-target",
                    _ => null,
                };
                if (missingTarget is not null) throw new BattleRuleException(missingTarget, "target");
            }
            if (target is { } victim) _ = rule.RequireTarget(battle, actor, destination, action, victim, side);
            var prepared = (SilenceRules.Suppress(battle, actor, destination, action, admission)
                ?? rule.Prepare(battle, actor, destination, action, target, progression, side)) with { Side = side };
            prepared.Validate(battle, actor, destination, action, target, admission, side);
            if (prepared.Reward is { } reward)
                BattleProgressionRules.Preflight(progression, prepared.Prepared.GetActor(reward.Actor), reward.Amount,
                    prepared.Prepared.MainSeed, battle.Definition.Poison);
            return prepared;
        });
}

internal sealed class BattleActionRuleFault(BattleActionKind kind, string identity, string operation) : Exception
{
    internal BattleActionKind Kind { get; } = kind;
    internal string Identity { get; } = identity;
    internal string Operation { get; } = operation;
}

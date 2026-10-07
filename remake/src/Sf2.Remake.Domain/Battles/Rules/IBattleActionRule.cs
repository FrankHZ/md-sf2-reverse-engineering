using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal sealed record BattleActionOffer(BattleActionRef Action, string Label, bool Empty = false);
internal sealed record BattleActionAdmission(byte? MinimumRange = null, byte? MaximumRange = null,
    HealingSpellDefinition? Spell = null, HealingItemDefinition? Item = null);

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
        BattleActionRef action, ActorRef? target);
}

internal interface IPhysicalActionRule : IBattleActionRule
{
    int EstimateDamage(EngineBattleState battle, ActorRef actor, ActorRef target);
}

internal static class BattleActionRules
{
    internal static T Invoke<T>(IBattleActionRule rule, string operation, Func<T> call)
    {
        try { return call(); }
        catch (BattleRuleException) { throw; }
        catch (BattleActionRuleFault) { throw; }
        catch (Exception) { throw new BattleActionRuleFault(rule.Kind, rule.Identity, operation); }
    }

    internal static BattleActionResolution Prepare(IBattleActionRule rule, EngineBattleState battle,
        ActorRef actor, MapPosition destination, BattleActionRef action, ActorRef? target) =>
        Invoke(rule, "prepare", () =>
        {
            var admission = rule.RequireAction(battle, actor, action);
            if (target is { } victim) _ = rule.RequireTarget(battle, actor, destination, action, victim);
            var prepared = rule.Prepare(battle, actor, destination, action, target);
            prepared.Validate(battle, actor, destination, action, target, admission);
            return prepared;
        });
}

internal sealed class BattleActionRuleFault(BattleActionKind kind, string identity, string operation) : Exception
{
    internal BattleActionKind Kind { get; } = kind;
    internal string Identity { get; } = identity;
    internal string Operation { get; } = operation;
}

namespace Sf2.Remake.Domain.Battles;

internal interface IBattleDecisionRule
{
    string Identity { get; }
    void RequireDeployment(BattleDeploymentDefinition deployment);
    void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input);
    BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical);
    BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical,
        IReadOnlyList<IBattleActionRule> actions) => Decide(battle, actor, physical);
}

internal static class BattleDecisionRules
{
    internal static T Invoke<T>(IBattleDecisionRule rule, string operation, Func<T> call)
    {
        try { return call(); }
        catch (BattleRuleException) { throw; }
        catch (BattlePolicyFault) { throw; }
        catch (BattleActionRuleFault) { throw; }
        catch (BattleDecisionRuleFault) { throw; }
        catch (Exception) { throw new BattleDecisionRuleFault(rule.Identity, operation); }
    }

    internal static BattleAutomaticAction Decide(IBattleDecisionRule rule, EngineBattleState battle,
        ActorRef actor, IPhysicalActionRule physical, IBattleProgressionRule progression,
        IReadOnlyList<IBattleActionRule>? actions = null) => Invoke(rule, "decide", () =>
        {
            var decision = actions is null ? rule.Decide(battle, actor, physical) : rule.Decide(battle, actor, physical, actions);
            decision.Validate(battle, actor);
            // A discarded admission preview uses the same selected action rule as execution.
            // No future construction seed or resources survive this seam.
            if (decision.Target is { } target && decision.ChosenAction?.Kind != BattleActionKind.PrismLaser)
            {
                try
                {
                    var action = decision.ChosenAction ?? new(BattleActionKind.Physical);
                    var selected = action.Kind == BattleActionKind.Physical ? physical :
                        actions?.Single(r => r.Kind == action.Kind) ?? throw new InvalidOperationException("Missing decision action rule.");
                    _ = BattleActionRules.Prepare(selected, decision.Battle, actor, decision.Destination, action, target, progression);
                }
                catch (BattleRuleException error) when (!error.Unsupported)
                { throw new BattleDecisionRuleFault(rule.Identity, "decision-admission"); }
            }
            return decision;
        });
}

internal sealed class BattleDecisionRuleFault(string identity, string operation) : Exception
{
    internal string Identity { get; } = identity;
    internal string Operation { get; } = operation;
}

namespace Sf2.Remake.Domain.Battles;

internal interface IBattleDecisionRule
{
    string Identity { get; }
    void RequireDeployment(BattleDeploymentDefinition deployment);
    void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input);
    BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical);
}

internal static class BattleDecisionRules
{
    internal static T Invoke<T>(IBattleDecisionRule rule, string operation, Func<T> call)
    {
        try { return call(); }
        catch (BattleRuleException) { throw; }
        catch (BattleActionRuleFault) { throw; }
        catch (BattleDecisionRuleFault) { throw; }
        catch (Exception) { throw new BattleDecisionRuleFault(rule.Identity, operation); }
    }

    internal static BattleAutomaticAction Decide(IBattleDecisionRule rule, EngineBattleState battle,
        ActorRef actor, IPhysicalActionRule physical) => Invoke(rule, "decide", () =>
        {
            var decision = rule.Decide(battle, actor, physical);
            decision.Validate(battle, actor);
            // A discarded admission preview uses the same selected physical rule as execution.
            // No future construction seed or resources survive this seam.
            if (decision.Target is { } target)
            {
                try
                {
                    _ = BattleActionRules.Prepare(physical, decision.Battle, actor, decision.Destination,
                        new(BattleActionKind.Physical), target);
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

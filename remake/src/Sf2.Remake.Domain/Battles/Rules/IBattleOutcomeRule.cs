namespace Sf2.Remake.Domain.Battles;

internal interface IBattleOutcomeRule
{
    string Identity { get; }
    BattleOutcomeKind? Check(EngineBattleState battle);
    bool DefeatedHook(EngineBattleState battle);
}

internal static class BattleOutcomeSelection
{
    internal static BattleOutcomeKind? Check(IBattleOutcomeRule rule, EngineBattleState battle) =>
        BattleProgressionRules.Invoke(rule.Identity, "outcome", () =>
        {
            var outcome = rule.Check(battle);
            if (outcome is { } kind && (!Enum.IsDefined(kind) || battle.Definition.Outcome is null))
                throw new InvalidOperationException("Invalid outcome.");
            return outcome;
        });
    internal static bool DefeatedHook(IBattleOutcomeRule rule, EngineBattleState battle) =>
        BattleProgressionRules.Invoke(rule.Identity, "defeated-hook", () => rule.DefeatedHook(battle));
}

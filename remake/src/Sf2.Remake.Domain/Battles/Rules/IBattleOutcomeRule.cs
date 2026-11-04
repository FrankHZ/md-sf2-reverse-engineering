namespace Sf2.Remake.Domain.Battles;

internal interface IBattleOutcomeRule
{
    string Identity { get; }
    BattleOutcomeKind? Check(EngineBattleState battle);
    bool DefeatedHook(EngineBattleState battle);
}

internal static class BattleOutcomeSelection
{
    // Damage actions share terminal admission before publishing movement, RNG or cost.
    internal static void RequireContinuing(EngineBattleState battle, IReadOnlyList<BattleReaction> reactions)
    {
        if (battle.Definition.Outcome is not null) return;
        var dead = reactions.Where(r => r.HpAfter == 0).Select(r => r.Target).ToHashSet();
        foreach (var unit in battle.Actors.Where(a => a.Hp > 0 && dead.Contains(a.Actor)))
        {
            if (unit.Definition.Physical?.Leader == true)
                throw new BattleRuleException("leader-defeat-program", unit.IsAlly ? "actor.physical.leader" : "target.physical.leader", true);
            if (!battle.Actors.Any(a => a.Hp > 0 && a.Faction == unit.Faction && !dead.Contains(a.Actor)))
                throw new BattleRuleException("battle-outcome-program", "battle.outcome", true);
        }
    }

    internal static BattleOutcomeKind? Check(IBattleOutcomeRule rule, EngineBattleState battle) =>
        BattleProgressionRules.Invoke(rule.Identity, "outcome", () =>
        {
            var outcome = rule.Check(battle);
            if (outcome is { } kind && (kind is not (BattleOutcomeKind.Victory or BattleOutcomeKind.Defeat) ||
                battle.Definition.Outcome is null))
                throw new InvalidOperationException("Invalid outcome.");
            return outcome;
        });
    internal static bool DefeatedHook(IBattleOutcomeRule rule, EngineBattleState battle) =>
        BattleProgressionRules.Invoke(rule.Identity, "defeated-hook", () => rule.DefeatedHook(battle));
}

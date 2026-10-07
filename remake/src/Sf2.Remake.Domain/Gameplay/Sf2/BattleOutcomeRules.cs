using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class BattleOutcomeRules : IBattleOutcomeRule
{
    public string Identity => "sf2-outcome";
    BattleOutcomeKind? IBattleOutcomeRule.Check(EngineBattleState battle) => Check(battle);
    bool IBattleOutcomeRule.DefeatedHook(EngineBattleState battle) => DefeatedHook(battle);

    internal static BattleOutcomeKind? Check(EngineBattleState battle)
    {
        if (battle.Definition.Outcome is not { } route) return null;
        // Source counts placed living actors and gives leader loss precedence over no enemies.
        if (battle.GetActor(route.Leader).Hp == 0 || !battle.Actors.Any(a => a.IsAlly && a.Hp > 0 && a.Position is not null))
            return BattleOutcomeKind.Defeat;
        return battle.Actors.Any(a => !a.IsAlly && a.Hp > 0 && a.Position is not null)
            ? null : BattleOutcomeKind.Victory;
    }

    internal static bool DefeatedHook(EngineBattleState battle) => battle.Definition.Outcome is { } route &&
        battle.GetActor(route.Leader).Hp > 0 && battle.GetActor(route.DefeatedGateEnemy).Hp == 0;
}

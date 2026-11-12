using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class Sf2ClassRules
{
    internal static byte? SourceClass(BattleClassRule classRule) => classRule switch
    {
        BattleClassRule.UnpromotedSwordsman => 0, BattleClassRule.UnpromotedWarrior => 2,
        BattleClassRule.UnpromotedPriest => 4, BattleClassRule.UnpromotedKnight => 1,
        BattleClassRule.UnpromotedMage => 3, BattleClassRule.UnpromotedArcher => 5,
        BattleClassRule.UnpromotedWolfman => 7, BattleClassRule.UnpromotedRanger => 8, BattleClassRule.UnpromotedPhoenix => 9,
        BattleClassRule.UnpromotedThief => 10, BattleClassRule.UnpromotedTortoise => 11, _ => null,
    };

}

using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class Sf2ClassRules
{
    // The regular base section in promotions.asm contains SDMN through TORT in class order.
    internal static bool RegularBase(byte? sourceClass) => sourceClass is <= 11;
    internal static byte? PromotionItem(byte? sourceClass) => sourceClass switch
    {
        1 => 118, 2 => 119, 3 => 121, 4 => 122, 5 => 120, _ => null,
    };

    internal static byte? SourceClass(BattleClassRule classRule) => classRule switch
    {
        BattleClassRule.UnpromotedSwordsman => 0, BattleClassRule.UnpromotedWarrior => 2,
        BattleClassRule.UnpromotedPriest => 4, BattleClassRule.UnpromotedKnight => 1,
        BattleClassRule.UnpromotedMage => 3, BattleClassRule.UnpromotedArcher => 5,
        BattleClassRule.UnpromotedBirdman => 6, BattleClassRule.UnpromotedWolfman => 7, BattleClassRule.UnpromotedRanger => 8, BattleClassRule.UnpromotedPhoenix => 9,
        BattleClassRule.UnpromotedThief => 10, BattleClassRule.UnpromotedTortoise => 11, BattleClassRule.Hero => 12, BattleClassRule.Paladin => 13, BattleClassRule.BrassGunner => 22, _ => null,
    };

}

using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class Sf2ClassRules
{
    // The regular base section in promotions.asm contains SDMN through TORT in class order.
    internal static bool RegularBase(byte? sourceClass) => sourceClass is <= 11;
    internal static BattleClassRule? OrdinaryDestination(byte? sourceClass) => sourceClass switch
    {
        0 => BattleClassRule.Hero,
        1 => BattleClassRule.Paladin,
        2 => BattleClassRule.Gladiator,
        3 => BattleClassRule.Wizard,
        4 => BattleClassRule.Vicar,
        5 => BattleClassRule.Sniper,
        6 => BattleClassRule.BirdBattler,
        7 => BattleClassRule.WolfBaron,
        8 => BattleClassRule.BowKnight,
        9 => BattleClassRule.Phoenix,
        10 => BattleClassRule.Ninja,
        11 => BattleClassRule.Monster,
        _ => null,
    };

    internal static BattleClassRule? SpecialDestination(byte? sourceClass) => sourceClass switch
    {
        1 => BattleClassRule.PegasusKnight,
        2 => BattleClassRule.Baron,
        3 => BattleClassRule.Sorcerer,
        4 => BattleClassRule.MasterMonk,
        5 => BattleClassRule.BrassGunner,
        _ => null,
    };

    internal static BattleClassRule? PromotionDestination(byte? sourceClass, byte destination)
    {
        var regular = OrdinaryDestination(sourceClass);
        if (regular is { } ordinary && SourceClass(ordinary) == destination) return ordinary;
        var special = SpecialDestination(sourceClass);
        return special is { } variant && SourceClass(variant) == destination ? variant : null;
    }

    internal static bool PlayerHealer(BattleClassRule rule) =>
        rule is BattleClassRule.UnpromotedPriest or BattleClassRule.Vicar or BattleClassRule.MasterMonk;

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
        BattleClassRule.UnpromotedThief => 10, BattleClassRule.UnpromotedTortoise => 11, BattleClassRule.Hero => 12, BattleClassRule.Paladin => 13,
        BattleClassRule.PegasusKnight => 14, BattleClassRule.Gladiator => 15, BattleClassRule.Baron => 16, BattleClassRule.Wizard => 17, BattleClassRule.Sorcerer => 18, BattleClassRule.Vicar => 19, BattleClassRule.Sniper => 21,
        BattleClassRule.MasterMonk => 20, BattleClassRule.BrassGunner => 22, BattleClassRule.BirdBattler => 23, BattleClassRule.WolfBaron => 24,
        BattleClassRule.BowKnight => 25, BattleClassRule.Phoenix => 26, BattleClassRule.Ninja => 27, BattleClassRule.Monster => 28,
        BattleClassRule.Robot => 29, BattleClassRule.Golem => 30, BattleClassRule.RedBaron => 31, _ => null,
    };

    internal static BattleClassRule? FromSource(byte sourceClass) => sourceClass switch
    {
        0 => BattleClassRule.UnpromotedSwordsman, 1 => BattleClassRule.UnpromotedKnight,
        2 => BattleClassRule.UnpromotedWarrior, 3 => BattleClassRule.UnpromotedMage,
        4 => BattleClassRule.UnpromotedPriest, 5 => BattleClassRule.UnpromotedArcher,
        6 => BattleClassRule.UnpromotedBirdman, 7 => BattleClassRule.UnpromotedWolfman,
        8 => BattleClassRule.UnpromotedRanger, 9 => BattleClassRule.UnpromotedPhoenix,
        10 => BattleClassRule.UnpromotedThief, 11 => BattleClassRule.UnpromotedTortoise,
        12 => BattleClassRule.Hero, 13 => BattleClassRule.Paladin, 14 => BattleClassRule.PegasusKnight,
        15 => BattleClassRule.Gladiator, 16 => BattleClassRule.Baron, 17 => BattleClassRule.Wizard,
        18 => BattleClassRule.Sorcerer, 19 => BattleClassRule.Vicar, 20 => BattleClassRule.MasterMonk,
        21 => BattleClassRule.Sniper, 22 => BattleClassRule.BrassGunner, 23 => BattleClassRule.BirdBattler,
        24 => BattleClassRule.WolfBaron, 25 => BattleClassRule.BowKnight, 26 => BattleClassRule.Phoenix,
        27 => BattleClassRule.Ninja, 28 => BattleClassRule.Monster, 29 => BattleClassRule.Robot,
        30 => BattleClassRule.Golem, 31 => BattleClassRule.RedBaron, _ => null,
    };

}

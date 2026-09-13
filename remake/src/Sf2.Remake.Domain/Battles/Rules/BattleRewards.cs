namespace Sf2.Remake.Domain.Battles;

internal static class BattleRewards
{
    internal static int KillExperience(int actorLevel, bool promoted, int targetLevel) =>
        (actorLevel + (promoted ? 20 : 0) - targetLevel) switch
        { < 3 => 50, 3 => 40, 4 => 30, 5 => 20, 6 => 10, _ => 0 };

    internal static int DamageExperience(int damage, ushort maximumHp, int killExperience) =>
        Math.Min(49, damage * killExperience / maximumHp);

    internal static int Award(int accumulated, bool halved, ref uint seed, List<PhysicalRoll> rolls)
    {
        int award = halved ? accumulated >> 1 : accumulated;
        if (PhysicalStrikeRules.Roll(ref seed, rolls, "exp-plus", 16) == 0) award++;
        if (PhysicalStrikeRules.Roll(ref seed, rolls, "exp-minus", 16) == 0) award--;
        return Math.Max(1, award);
    }

    internal static uint Gold(uint current, uint award) => (uint)Math.Min(9_999_999UL, (ulong)current + award);
    internal static ushort Kills(ushort current) => (ushort)Math.Min(9999, current + 1);
}

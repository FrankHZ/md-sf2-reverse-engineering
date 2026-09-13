namespace Sf2.Remake.Domain.Battles;

internal sealed record PhysicalRoll(string Purpose, ushort Range, uint Before, uint After, ushort Result);
internal sealed record PhysicalStrike(bool Dodged, bool Critical, int Damage, ushort Hp,
    uint Seed, bool Double, bool Counter, IReadOnlyList<PhysicalRoll> Rolls);

// Scalar construction only. Callers validate follow-up eligibility and settle the entire action
// before publishing. Source: attack/determinedodge/determinecriticalhit/inflictdamage.asm.
internal static class PhysicalStrikeRules
{
    internal static ushort Roll(ref uint seed, List<PhysicalRoll> rolls, string purpose, ushort range)
    {
        uint before = seed;
        var draw = BattleRandom.NextMain(seed, range);
        ushort result = draw.Value;
        seed = draw.After;
        rolls.Add(new(purpose, range, before, seed, result));
        return result;
    }

    internal static int LandDamage(int attack, int defense, int multiplier) =>
        Math.Max(1, attack - defense) * multiplier >> 8;

    internal static PhysicalStrike Resolve(int attack, int defense, ushort hp, int multiplier,
        uint seed, ushort dodgeRange, ushort criticalRange, int criticalShift, bool counter = false)
    {
        var rolls = new List<PhysicalRoll>();
        bool dodged = Roll(ref seed, rolls, "dodge", dodgeRange) == 0, critical = false;
        int damage = 0;
        if (!dodged)
        {
            damage = LandDamage(attack, defense, multiplier);
            critical = Roll(ref seed, rolls, "critical", criticalRange) == 0;
            if (critical) damage += damage >> criticalShift;
            if (counter) damage >>= 1;
            ushort spread = (ushort)((damage >> 3) + 1);
            damage -= Roll(ref seed, rolls, "spread-1", spread);
            damage -= Roll(ref seed, rolls, "spread-2", spread);
            damage = Math.Max(1, damage);
        }
        ushort remaining = (ushort)Math.Max(0, hp - damage);
        bool twice = remaining > 0 && Roll(ref seed, rolls, "double", 32) == 0;
        bool response = remaining > 0 && Roll(ref seed, rolls, "counter", 32) == 0;
        return new(dodged, critical, damage, remaining, seed, twice, response, rolls.AsReadOnly());
    }
}

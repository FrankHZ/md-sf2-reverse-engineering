using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed record PhysicalRoll(string Purpose, ushort Range, uint Before, uint After, ushort Result);
internal sealed record PhysicalStrike(bool Dodged, bool Critical, int DamageForRecoil, ushort Hp,
    uint Seed, IReadOnlyList<PhysicalRoll> Rolls, bool SleepInflicted = false, bool Ineffective = false, bool SilenceInflicted = false, bool PoisonInflicted = false, bool StunInflicted = false, int? CutoffDamage = null)
{
    internal int Damage => Ineffective ? 0 : DamageForRecoil;
}

// Resolve one impact up to the point at which the action can apply ailment and recoil.
// The action owns follow-up draws, which depend on both participants surviving the impact.
// Source: attack/determinedodge/determinecriticalhit/inflictdamage.asm.
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

    internal static int LandDamage(int attack, int defense, int multiplier, bool archerBonus = false)
    {
        int damage = Math.Max(1, attack - defense) * multiplier >> 8;
        return archerBonus ? damage + (damage >> 2) : damage;
    }

    internal static PhysicalStrike Resolve(int attack, int defense, ushort hp, int multiplier,
        uint seed, ushort dodgeRange, ushort criticalRange, int criticalShift, bool counter = false,
        bool sleepCritical = false, bool sleepingDefender = false, bool ineffective = false,
        bool silenceCritical = false, bool poisonCritical = false, bool stunCritical = false, bool archerBonus = false, bool gisarme = false, bool immuneToDesoul = false)
    {
        var rolls = new List<PhysicalRoll>();
        bool dodged = !sleepingDefender && Roll(ref seed, rolls, "dodge", dodgeRange) == 0, critical = false;
        bool sleep = false, silence = false, poison = false, stun = false;
        int damage = 0;
        bool cutoff = false;
        int? cutoffDamage = null;
        if (!dodged)
        {
            damage = LandDamage(attack, defense, multiplier, archerBonus);
            bool success = Roll(ref seed, rolls, "critical", criticalRange) == 0;
            sleep = success && sleepCritical;
            silence = success && silenceCritical;
            poison = success && poisonCritical;
            stun = success && stunCritical;
            critical = success && !sleep && !silence && !poison && !stun;
            if (critical && criticalShift > 0) damage += damage >> criticalShift;
            cutoff = critical && gisarme && !immuneToDesoul;
            if (cutoff) critical = false;
            if (!ineffective)
            {
                if (counter) damage >>= 1;
                ushort spread = (ushort)((damage >> 3) + 1);
                damage -= Roll(ref seed, rolls, "spread-1", spread);
                damage -= Roll(ref seed, rolls, "spread-2", spread);
                damage = Math.Max(1, damage);
                // InflictDamage consumes ordinary variance and damage EXP before lethal cutoff.
                if (cutoff) { cutoffDamage = damage; damage = 0x8000; }
            }
        }
        // The original ineffective hit leaves d6 intact for the following recoil helper.
        ushort remaining = ineffective ? hp : (ushort)Math.Max(0, hp - damage);
        return new(dodged, critical, damage, remaining, seed, rolls.AsReadOnly(), sleep && remaining > 0,
            !dodged && ineffective, silence && remaining > 0, poison && remaining > 0, stun && remaining > 0, cutoffDamage);
    }
}

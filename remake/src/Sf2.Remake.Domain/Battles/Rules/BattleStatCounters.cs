namespace Sf2.Remake.Domain.Battles;

// The duration word changes during command construction. Stat readers see the
// last completed UpdateCombatantStats until a reaction or other refresh runs.
internal static class BattleStatCounters
{
    internal const ushort Slow = 0x0C00;
    internal const ushort Boost = 0x3000;
    internal const ushort Attack = 0xC000;
    internal const ushort Mask = Slow | Attack | Boost;
    internal const ushort StatBits = Mask | 1; // STUN is applied after both fractions.

    private static int Increase(int basis, ushort applied) => basis * ((applied & Boost) >> 12) / 8;
    private static int Decrease(int basis, ushort applied) => basis * ((applied & Slow) >> 10) / 8;

    internal static int AttackBonus(int basis, ushort applied) => basis * ((applied & Attack) >> 14) / 8;

    internal static byte Defense(byte basis, ushort applied) =>
        (byte)Math.Clamp(Math.Min(200, basis + Increase(basis, applied)) - Decrease(basis, applied), 0, 200);

    internal static byte PackedAgility(byte basis, ushort applied)
    {
        int boosted = (basis & 128) | Math.Min(127, (basis & 127) + Increase(basis, applied));
        int slowed = Math.Clamp(boosted - Decrease(basis, applied), 0, 200);
        return (byte)Math.Max(0, slowed - ((applied & 1) != 0 ? 5 : 0));
    }

    internal static bool ValidSavedOperand(ushort status, ushort? operand)
    {
        if (operand is not { } applied) return true;
        return (applied & ~Mask) == 0 && applied != (status & Mask) &&
            ValidField(Boost, 0x1000) && ValidField(Slow, 0x0400) && ValidField(Attack, 0x4000);

        bool ValidField(int mask, int unit) => (applied & mask) == (status & mask) ||
            (status & mask) == mask && ((applied & mask) == unit || (applied & mask) == 2 * unit);
    }
}

using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class CurseRules
{
    internal const ushort Mask = 4;
    internal const ushort InactionRange = 4;
    internal const ushort RecoilRange = 2;

    internal static bool Present(ushort status) => (status & Mask) != 0;
    internal static bool CanRecoil(BattleActorState attacker, bool dodged, ushort defenderHp) =>
        attacker.IsAlly && Present(attacker.Status) && !dodged && defenderHp > 0;

    internal static int RecoilDamage(int strikeDamage) => (strikeDamage >> 3) + 1;
}

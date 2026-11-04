using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// SF2DISASM ProcessAfterTurnEffects @ApplyPoisonDamage: fixed POISON_DAMAGE=2,
// after message307; exhausted308 precedes the death batch. Poison changes no stats.
internal static class PoisonTurnRules
{
    internal const ushort Status = 2;
    internal const int Damage = 2;
    internal static bool SupportedPhysicalStatus(ushort status) => SilenceRules.Supported(status);
    internal static bool Eligible(BattleActorState actor) => actor.Hp > 0 && actor.Position is not null && (actor.Status & Status) != 0;
    internal static ushort RemainingHp(ushort hp) => (ushort)Math.Max(0, hp - Damage);
}

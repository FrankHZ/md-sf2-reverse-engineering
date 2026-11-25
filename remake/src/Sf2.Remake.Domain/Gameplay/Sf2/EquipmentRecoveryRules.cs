using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed record EquipmentRecovery(int Step, ushort Before, ushort After)
{
    internal bool RestoresMp => Step == 1;
    internal int Text => RestoresMp ? 357 : 356;
}

// ProcessAfterTurnEffects: weapon HP, weapon MP, ring HP, then poison.
// Each step reads the live worn slot and caps independently; there is no round clock.
internal static class EquipmentRecoveryRules
{
    internal static EquipmentRecovery? Next(BattleActorState actor, int start = 0)
    {
        if (actor.Hp == 0 || actor.Position is null) return null;
        for (int step = start; step < 3; step++)
        {
            int amount = BattleEquipment.Recovery(actor, step);
            ushort before = step == 1 ? actor.Mp : actor.Hp;
            int maximum = step == 1 ? actor.MaxMp : actor.MaxHp;
            ushort after = (ushort)Math.Min(maximum, before + amount);
            if (after > before) return new(step, before, after);
        }
        return null;
    }

    internal static BattleActorState Apply(BattleActorState actor, EquipmentRecovery recovery) =>
        recovery.RestoresMp ? actor.With(mp: checked((byte)recovery.After)) : actor.With(hp: recovery.After);
}

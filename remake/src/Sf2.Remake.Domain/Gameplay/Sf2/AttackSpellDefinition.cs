using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Damage operands are independent of healing, its target policy and fairy playback.
public enum AttackSpellElement { Wind = 0, Lightning = 2, Ice = 4, Fire = 6 }
public sealed record AttackSpellDefinition(SpellRef Spell, byte MpCost, ushort Power,
    byte MinimumRange, byte MaximumRange, byte Radius, AttackSpellElement Element)
{
    internal ushort CriticalRange => Spell.Value is "aqua" or "bolt" ? (ushort)8 : (ushort)32;

    internal void RequireSupported(bool enemy = false)
    {
        bool aqua = Spell is { Value: "aqua", Level: 1 } && Element == AttackSpellElement.Ice && MpCost == 0 && Radius == 0;
        bool bolt = Spell is { Value: "bolt", Level: 2 } && Element == AttackSpellElement.Lightning;
        bool selected = bolt || aqua || Spell is { Value: "blaze", Level: 1 or 2 } && Element == AttackSpellElement.Fire ||
            Spell is { Value: "freeze", Level: 1 } && Element == AttackSpellElement.Ice ||
            Spell is { Value: "blast", Level: 1 } && Element == AttackSpellElement.Wind;
        if (!selected || Power is 0 or > 255 || MinimumRange < 1 || MaximumRange < MinimumRange ||
            MaximumRange > (aqua || bolt ? 3 : 2) || Radius > (bolt ? 2 : 1))
            throw new BattleRuleException("attack-spell-definition", "spells", true);
        if (enemy && !bolt && Radius != 0 && Spell is not { Value: "blaze", Level: 2 })
            throw new BattleRuleException("enemy-spell-radius", "spells.radius", true);
    }
}

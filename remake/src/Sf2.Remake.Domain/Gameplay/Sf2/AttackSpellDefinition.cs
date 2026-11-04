using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Damage operands are independent of healing, its target policy and fairy playback.
public enum AttackSpellElement { Wind = 0, Fire = 6 }
public sealed record AttackSpellDefinition(SpellRef Spell, byte MpCost, ushort Power,
    byte MinimumRange, byte MaximumRange, byte Radius, AttackSpellElement Element)
{
    internal void RequireSupported()
    {
        bool selected = Spell is { Value: "blaze", Level: 1 or 2 } && Element == AttackSpellElement.Fire ||
            Spell is { Value: "blast", Level: 1 } && Element == AttackSpellElement.Wind;
        if (!selected || Power is 0 or > 255 || MinimumRange < 1 || MaximumRange < MinimumRange ||
            MaximumRange > 2 || Radius > 1)
            throw new BattleRuleException("attack-spell-definition", "spells", true);
    }
}

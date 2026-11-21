using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Damage operands are independent of healing, its target policy and fairy playback.
public enum AttackSpellElement { Wind = 0, Lightning = 2, Ice = 4, Fire = 6, Neutral = 8 }
public sealed record AttackItemDefinition(byte ItemId, string Name, AttackSpellDefinition Effect);

public sealed record AttackSpellDefinition(SpellRef Spell, byte MpCost, ushort Power,
    byte MinimumRange, byte MaximumRange, byte Radius, AttackSpellElement Element, byte? SourceAnimationSelector = null)
{
    internal bool IsMonsterBreath => Spell.Value == "kiwi";
    internal ushort CriticalRange => Spell.Value switch
    { "aqua" or "bolt" or "raijin" => 8, "kiwi" => 16, _ => 32 };

    internal void RequireConsumableEffect()
    {
        if (MpCost != 0 || MinimumRange != 1 || MaximumRange != 1 || Radius != 0 ||
            Power is 0 or > 255 || !Enum.IsDefined(Element) || CriticalRange != 32)
            throw new BattleRuleException("attack-item-effect", "item.useSpell", true);
    }

    internal void RequireSupported(bool enemy = false)
    {
        bool aqua = Spell is { Value: "aqua", Level: 1 or 2 } && Element == AttackSpellElement.Ice && MpCost == 0 && Radius == 0;
        bool bolt = Spell is { Value: "bolt", Level: 1 or 2 } && Element == AttackSpellElement.Lightning;
        bool katon = Spell is { Value: "katon", Level: 1 or 2 or 3 } && Element == AttackSpellElement.Neutral;
        bool raijin = Spell is { Value: "raijin", Level: 1 or 2 or 3 } && Element == AttackSpellElement.Neutral;
        bool wide = bolt && Spell.Level == 2 || raijin && Spell.Level < 3;
        bool selected = bolt || aqua || katon || raijin || Spell is { Value: "blaze", Level: 1 or 2 or 3 } && Element == AttackSpellElement.Fire ||
            Spell is { Value: "freeze", Level: 1 or 2 } && Element == AttackSpellElement.Ice ||
            Spell is { Value: "blast", Level: 1 or 2 or 3 or 4 } && Element == AttackSpellElement.Wind;
        if (!selected || Power is 0 or > 255 || MinimumRange < 1 || MaximumRange < MinimumRange ||
            MaximumRange > (aqua || wide || raijin ? 3 : 2) || Radius > (wide ? 2 : 1))
            throw new BattleRuleException("attack-spell-definition", "spells", true);
        if (enemy && !bolt && Radius != 0 && Spell is not ({ Value: "blaze" or "blast", Level: 2 or 3 } or { Value: "freeze", Level: 2 }))
            throw new BattleRuleException("enemy-spell-radius", "spells.radius", true);
    }
}

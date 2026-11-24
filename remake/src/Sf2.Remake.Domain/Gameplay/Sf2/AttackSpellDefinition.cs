using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Source resistance fields used by offensive spells.
public enum AttackSpellElement { Wind = 0, Lightning = 2, Ice = 4, Fire = 6, Neutral = 8, Status = 14 }
public sealed record AttackItemDefinition(byte ItemId, string Name, AttackSpellDefinition Effect, bool Breakable = false);

public sealed record AttackSpellDefinition(SpellRef Spell, byte MpCost, ushort Power,
    byte MinimumRange, byte MaximumRange, byte Radius, AttackSpellElement Element, byte? SourceAnimationSelector = null)
{
    public bool IsInstantDeath => Spell.Value == "desoul";
    internal bool IsMonsterBreath => Spell.Value == "kiwi";
    internal ushort CriticalRange => Spell.Value switch
    { "aqua" or "bolt" or "raijin" => 8, "kiwi" => 16, _ => 32 };

    internal static bool InstantDeathSucceeds(BattleActorState target, int roll) =>
        roll >= 5 + (target.Definition.Resistance >> (int)AttackSpellElement.Status & 3);

    internal void RequireConsumableEffect()
    {
        if (MpCost != 0 || MinimumRange != 1 || MaximumRange != 1 || Radius != 0 ||
            Power is 0 or > 255 || !Enum.IsDefined(Element) || Element == AttackSpellElement.Status || CriticalRange != 32)
            throw new BattleRuleException("attack-item-effect", "item.useSpell", true);
    }

    internal void RequireSupported(bool enemy = false)
    {
        if (IsInstantDeath)
        {
            if (enemy || Element != AttackSpellElement.Status || Power != 0 || MinimumRange != 1 || MaximumRange != 2 ||
                (Spell.Level, MpCost, Radius) is not ((1, 8, 0) or (2, 13, 1)))
                throw new BattleRuleException("attack-spell-definition", "spells", true);
            return;
        }
        (int Range, int Radius) limits = (Spell.Value, Spell.Level, Element) switch
        {
            ("aqua", 1 or 2, AttackSpellElement.Ice) when MpCost == 0 => (3, 0),
            ("blaze", >= 1 and <= 3, AttackSpellElement.Fire) => (2, 1),
            ("blaze", 4, AttackSpellElement.Fire) when !enemy => (2, 0),
            ("freeze", 1 or 2, AttackSpellElement.Ice) => (2, 1),
            ("freeze", 3, AttackSpellElement.Ice) when !enemy => (3, 1),
            ("freeze", 4, AttackSpellElement.Ice) when !enemy => (3, 0),
            ("bolt", 1, AttackSpellElement.Lightning) => (2, 1),
            ("bolt", 2, AttackSpellElement.Lightning) => (3, 2),
            ("bolt", 3, AttackSpellElement.Lightning) when !enemy => (3, 2),
            ("bolt", 4, AttackSpellElement.Lightning) when !enemy => (3, 0),
            ("blast", >= 1 and <= 4, AttackSpellElement.Wind) => (2, 1),
            ("katon", >= 1 and <= 3, AttackSpellElement.Neutral) => (2, 1),
            ("raijin", 1 or 2, AttackSpellElement.Neutral) => (3, 2),
            ("raijin", 3, AttackSpellElement.Neutral) => (3, 1),
            _ => (0, 0),
        };
        if (Power is 0 or > 255 || MinimumRange < 1 || MaximumRange < MinimumRange ||
            MaximumRange > limits.Range || Radius > limits.Radius)
            throw new BattleRuleException("attack-spell-definition", "spells", true);
        if (enemy && Spell.Value != "bolt" && Radius != 0 &&
            Spell is not ({ Value: "blaze" or "blast", Level: 2 or 3 } or { Value: "freeze", Level: 2 }))
            throw new BattleRuleException("enemy-spell-radius", "spells.radius", true);
    }
}

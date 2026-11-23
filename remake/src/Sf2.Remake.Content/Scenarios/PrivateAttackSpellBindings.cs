using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateAttackSpellBindings
{
    // Pinned spellelements.asm supplies the resistance shift for each attack spell.
    // Only these source spell records enter the implemented damage consumer.
    internal static IEnumerable<AttackSpellDefinition> Spells(PrivateBattleDefinitions definitions)
    {
        foreach (var source in definitions.Spells.Values)
        {
            (int Id, int Mp, int Power, int Range, int Radius, AttackSpellElement Element)? expected =
                (source.Code, source.Level) switch
                {
                    ("BLAZE", 1) => (11, 2, 6, 2, 0, AttackSpellElement.Fire),
                    ("BLAZE", 3) => (11, 10, 15, 2, 1, AttackSpellElement.Fire),
                    ("BLAST", 3) => (14, 8, 12, 2, 1, AttackSpellElement.Wind),
                    ("BLAST", 4) => (14, 8, 30, 2, 0, AttackSpellElement.Wind),
                    ("AQUA", 2) => (40, 0, 15, 1, 0, AttackSpellElement.Ice),
                    ("BLAZE", 2) => (11, 6, 10, 2, 1, AttackSpellElement.Fire),
                    ("FREEZE", 2) => (12, 7, 12, 2, 1, AttackSpellElement.Ice),
                    ("FREEZE", 1) => (12, 3, 10, 2, 0, AttackSpellElement.Ice),
                    ("BOLT", 1) => (13, 8, 14, 2, 1, AttackSpellElement.Lightning),
                    ("BOLT", 2) => (13, 15, 16, 3, 2, AttackSpellElement.Lightning),
                    ("AQUA", 1) => (40, 0, 13, 3, 0, AttackSpellElement.Ice),
                    ("BLAST", 2) => (14, 5, 8, 2, 1, AttackSpellElement.Wind),
                    ("BLAST", 1) => (14, 2, 6, 1, 0, AttackSpellElement.Wind),
                    ("KATON", 1) => (27, 6, 12, 2, 1, AttackSpellElement.Neutral),
                    ("KATON", 2) => (27, 10, 16, 2, 1, AttackSpellElement.Neutral),
                    ("KATON", 3) => (27, 12, 42, 2, 0, AttackSpellElement.Neutral),
                    ("RAIJIN", 1) => (28, 15, 18, 3, 2, AttackSpellElement.Neutral),
                    ("RAIJIN", 2) => (28, 18, 26, 3, 2, AttackSpellElement.Neutral),
                    ("RAIJIN", 3) => (28, 20, 58, 3, 0, AttackSpellElement.Neutral),
                    ("KIWI", 1) => (41, 0, 28, 1, 0, AttackSpellElement.Fire),
                    ("KIWI", 2) => (41, 0, 36, 1, 0, AttackSpellElement.Fire),
                    ("KIWI", 3) => (41, 0, 45, 1, 0, AttackSpellElement.Fire),
                    ("KIWI", 4) => (41, 0, 58, 1, 0, AttackSpellElement.Fire),
                    _ => null
                };
            if (expected is not { } row) continue;
            Require(source.BaseId == row.Id && source.MpCost == row.Mp && source.Power == row.Power &&
                source.MinimumRange == 1 && source.MaximumRange == row.Range && source.Radius == row.Radius &&
                source.PropertiesExpression == (source.Code is "AQUA" or "KIWI" ? "TYPE_ATTACK" : "TYPE_ATTACK|AFFECTEDBYSILENCE"),
                "source-attack-spell", "spells", true);
            byte? animation = null;
            if (source.Code == "BLAZE" && source.Level == 1)
            {
                Require(source.AnimationExpression == "BLAZE", "source-attack-animation", "spells.animation", true);
                animation = 1;
            }
            if (row.Element == AttackSpellElement.Neutral)
            {
                string family = source.Code == "KATON" ? "BLAZE" : "BOLT";
                Require(source.AnimationExpression == $"{family}|VARIATION{source.Level + 1}",
                    "source-attack-animation", "spells.animation", true);
                animation = (byte)((family == "BLAZE" ? 1 : 7) | source.Level << 5);
            }
            if (source.Code == "KIWI")
            {
                Require(source.AnimationExpression == "FLAME_BREATH|VARIATION3",
                    "source-attack-animation", "spells.animation", true);
                animation = 76;
            }
            yield return new(source.Spell, source.MpCost, source.Power, source.MinimumRange, source.MaximumRange,
                source.Radius, row.Element, animation);
        }
    }

    internal static ushort Resistance(string expression)
    {
        if (expression is "0" or "NONE") return 0;
        int word = 0, used = 0;
        foreach (string token in expression.Split('|'))
        {
            int separator = token.LastIndexOf('_');
            Require(separator > 0, "source-resistance", "actors.resistance", true);
            string element = token[..separator], setting = token[(separator + 1)..];
            int shift = element switch
            { "WIND" => 0, "LIGHTNING" => 2, "ICE" => 4, "FIRE" => 6, "NEUTRAL" => 8,
                "UNDEFINED1" => 10, "UNDEFINED2" => 12, "STATUS" => 14, _ => -1 };
            int value = setting switch { "NONE" => 0, "MINOR" => 1, "MAJOR" => 2,
                "WEAKNESS" when element != "STATUS" => 3, "IMMUNITY" when element == "STATUS" => 3, _ => -1 };
            Require(shift >= 0 && value >= 0 && (used & (3 << shift)) == 0,
                "source-resistance", "actors.resistance", true);
            used |= 3 << shift; word |= value << shift;
        }
        return checked((ushort)word);
    }
}

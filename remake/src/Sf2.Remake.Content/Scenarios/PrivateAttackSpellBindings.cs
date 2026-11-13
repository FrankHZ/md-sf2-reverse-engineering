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
                    ("BLAZE", 2) => (11, 6, 10, 2, 1, AttackSpellElement.Fire),
                    ("FREEZE", 1) => (12, 3, 10, 2, 0, AttackSpellElement.Ice),
                    ("AQUA", 1) => (40, 0, 13, 3, 0, AttackSpellElement.Ice),
                    ("BLAST", 1) => (14, 2, 6, 1, 0, AttackSpellElement.Wind),
                    _ => null
                };
            if (expected is not { } row) continue;
            Require(source.BaseId == row.Id && source.MpCost == row.Mp && source.Power == row.Power &&
                source.MinimumRange == 1 && source.MaximumRange == row.Range && source.Radius == row.Radius &&
                source.PropertiesExpression == (source.Code == "AQUA" ? "TYPE_ATTACK" : "TYPE_ATTACK|AFFECTEDBYSILENCE"),
                "source-attack-spell", "spells", true);
            yield return new(source.Spell, source.MpCost, source.Power, source.MinimumRange, source.MaximumRange,
                source.Radius, row.Element);
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

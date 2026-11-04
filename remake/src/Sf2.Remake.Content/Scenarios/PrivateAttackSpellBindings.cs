using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateAttackSpellBindings
{
    // Pinned spellelements.asm: BLAZE=FIRE(bits6-7), BLAST=WIND(bits0-1).
    // Only these source spell records enter the implemented damage consumer.
    internal static IEnumerable<AttackSpellDefinition> Spells(PrivateBattleDefinitions definitions)
    {
        foreach (var source in definitions.Spells.Values.Where(s => s.Code == "BLAZE" && s.Level <= 2 || s.Code == "BLAST" && s.Level == 1))
        {
            bool blaze = source.Code == "BLAZE", area = blaze && source.Level == 2;
            Require(source.BaseId == (blaze ? 11 : 14) && source.Level >= 1 &&
                source.MpCost == (area ? 6 : 2) && source.Power == (area ? 10 : 6) &&
                source.MinimumRange == 1 && source.MaximumRange == (blaze ? 2 : 1) && source.Radius == (area ? 1 : 0) &&
                source.PropertiesExpression == "TYPE_ATTACK|AFFECTEDBYSILENCE",
                "source-attack-spell", "spells", true);
            yield return new(source.Spell, source.MpCost, source.Power, source.MinimumRange, source.MaximumRange,
                source.Radius, blaze ? AttackSpellElement.Fire : AttackSpellElement.Wind);
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

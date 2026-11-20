using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateSupportSpellBindings
{
    internal static IEnumerable<SupportSpellDefinition> Spells(PrivateBattleDefinitions definitions)
    {
        foreach (SpellRef key in new SpellRef[] { new("dispel", 1), new("boost", 1), new("slow", 1), new("slow", 2) })
        {
            if (!definitions.Spells.TryGetValue(key, out var source)) continue;
            var spell = new SupportSpellDefinition(key, source.MpCost, source.MinimumRange, source.MaximumRange, source.Radius);
            spell.RequireSupported();
            var properties = spell.TargetsTeammates ? "TYPE_SUPPORT|TARGET_TEAMMATES|AFFECTEDBYSILENCE" : "TYPE_SUPPORT|AFFECTEDBYSILENCE";
            Require(source.BaseId == (key.Value switch { "boost" => 3, "slow" => 4, _ => 6 }) &&
                source.Code == key.Value.ToUpperInvariant() && source.Level == key.Level && source.Power == (spell.ChangesStats ? 15 : 0) &&
                source.PropertiesExpression == properties, "source-support-spell", "spells." + key.Value, true);
            yield return spell;
        }
    }
}

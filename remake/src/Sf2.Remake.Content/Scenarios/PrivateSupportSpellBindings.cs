using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateSupportSpellBindings
{
    internal static IEnumerable<SupportSpellDefinition> Spells(PrivateBattleDefinitions definitions)
    {
        foreach (SpellRef key in new SpellRef[] { new("dispel", 1), new("boost", 1), new("slow", 1), new("slow", 2), new("sleep", 1), new("muddle", 1) })
        {
            if (definitions.Spells.ContainsKey(key)) yield return Read(definitions, key);
        }
    }

    internal static SupportSpellDefinition Read(PrivateBattleDefinitions definitions, SpellRef key)
    {
        Require(definitions.Spells.TryGetValue(key, out var source), "source-support-spell-missing", "spells." + key.Value, true);
        var spell = new SupportSpellDefinition(key, source!.MpCost, source.MinimumRange, source.MaximumRange, source.Radius);
        spell.RequireSupported();
        var properties = spell.TargetsTeammates ? "TYPE_SUPPORT|TARGET_TEAMMATES|AFFECTEDBYSILENCE" : "TYPE_SUPPORT|AFFECTEDBYSILENCE";
        Require(source.BaseId == (key.Value switch { "boost" => 3, "slow" => 4, "attack" => 5, "sleep" => 9, "muddle" => 7, _ => 6 }) &&
            source.Code == key.Value.ToUpperInvariant() && source.Level == key.Level && source.Power == (spell.ChangesStats ? 15 : 0) &&
            source.PropertiesExpression == properties &&
            (key.Value switch
            {
                "sleep" => source is { EntryExpression: "SLEEP", AnimationExpression: "DEBUFF1" },
                "muddle" => source is { EntryExpression: "MUDDLE", AnimationExpression: "DEBUFF1" },
                _ => true,
            }), "source-support-spell", "spells." + key.Value, true);
        return spell;
    }
}

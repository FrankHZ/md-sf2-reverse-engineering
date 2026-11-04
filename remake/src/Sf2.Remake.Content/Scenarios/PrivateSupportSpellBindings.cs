using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateSupportSpellBindings
{
    internal static IEnumerable<SupportSpellDefinition> Spells(PrivateBattleDefinitions definitions)
    {
        if (!definitions.Spells.TryGetValue(new("dispel", 1), out var source)) yield break;
        Require(source is { BaseId: 6, Code: "DISPEL", Level: 1, MpCost: 5, MinimumRange: 1, MaximumRange: 2, Radius: 1, Power: 0 } &&
            source.PropertiesExpression == "TYPE_SUPPORT|AFFECTEDBYSILENCE", "source-support-spell", "spells.dispel", true);
        yield return new(source.Spell, source.MpCost, source.MinimumRange, source.MaximumRange, source.Radius);
    }
}

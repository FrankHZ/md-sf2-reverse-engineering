using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// Bounded source-to-semantic binding. Effective ATT has already been refreshed by the declared
// start policy; checking the source equipment must never apply its bonus a second time.
internal static class PrivateBattleActionBindings
{
    internal static PhysicalActorDefinition Physical(PrivateBattleDefinitions definitions, string prowess,
        IEnumerable<ushort> items, bool leader, ushort gold)
    {
        var critical = prowess switch
        {
            "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.OneIn16WithQuarterBonus,
            "CRITICAL150_1IN32|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.OneIn32WithHalfBonus,
            _ => throw new BattleRuleException("source-prowess", "actor.prowess", true),
        };
        var equipped = items.Where(word => (word & 128) != 0).Select(word => definitions.Items[(byte)(word & 127)]).ToArray();
        Require(equipped.Length <= 1, "source-equipment", "actor.items", true);
        byte minimum = 1, maximum = 1; // Unarmed source attack range.
        foreach (var item in equipped)
        {
            Require(item.ItemTypeExpression == "WEAPON" && item.EquipEffects.All(effect =>
                effect.Type == "INCREASE_ATT" || effect is { Type: "NONE", Parameter: 0 }),
                "source-equipment-effects", "actor.items", true);
            // The common AI station search currently supports adjacent weapons only.
            Require(item.MinimumRange == 1 && item.MaximumRange == 1, "source-weapon-range", "actor.items", true);
            minimum = item.MinimumRange; maximum = item.MaximumRange;
        }
        return new(critical, Promoted: false, leader, gold, minimum, maximum);
    }
}

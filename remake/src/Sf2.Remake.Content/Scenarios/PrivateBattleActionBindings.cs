using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// Bounded source-to-semantic binding. Allied controlled ATT is already refreshed;
// enemy raw base ATT and ordinary gear are refreshed by fresh battle initialization.
internal static class PrivateBattleActionBindings
{
    internal static IEnumerable<RetreatSpellDefinition> RetreatSpells(PrivateBattleDefinitions definitions)
    {
        Require(definitions.Spells.TryGetValue(new("egress", 1), out var spell) &&
            spell is { BaseId: 10, Code: "EGRESS", MpCost: 8, Radius: 0, Power: 0, MinimumRange: 0, MaximumRange: 0 } &&
            spell.PropertiesExpression == "TYPE_SPECIAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE",
            "source-egress-definition", "spells.egress", true);
        yield return new(spell!.Spell, spell.MpCost);
    }

    internal static IEnumerable<HealingItemDefinition> HealingItems(PrivateBattleDefinitions definitions)
    {
        foreach (var (id, code, effect, level, power) in new (byte, string, string, byte, ushort)[]
            { (0, "MEDICAL_HERB", "HEALIN", 1, 10), (1, "HEALING_SEED", "HEALIN", 2, 20), (3, "ANTIDOTE", "DETOX", 1, 0) })
        {
            if (!definitions.Items.TryGetValue(id, out var item)) continue;
            Require(item.Code == code && item.ItemTypeExpression == "CONSUMABLE" &&
                item.UseSpell == effect && item.UseSpellLevel == level && item.EquipFlagsExpression == "NONE" &&
                item.MinimumRange == 0 && item.MaximumRange == 1 &&
                item.EquipEffects.All(effect => effect is { Type: "NONE", Parameter: 0 }), "source-supply-definition", "items", true);
            Require(definitions.Spells.TryGetValue(new(effect.ToLowerInvariant(), level), out var spell) &&
                spell.MpCost == (id == 3 ? 3 : 0) && spell.Radius == 0 && spell.Power == power &&
                spell.MinimumRange == 0 && spell.MaximumRange == 1 && spell.PropertiesExpression ==
                    (id == 3 ? "TYPE_HEAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE" : "TYPE_HEAL|TARGET_TEAMMATES"),
                "source-supply-effect", "items.useSpell", true);
            yield return new(item.Id, item.DisplayName, spell!.Power, spell.MinimumRange, spell.MaximumRange, CuresPoison: id == 3);
        }
    }

    internal static IEnumerable<HealingSpellDefinition> PoisonCure(PrivateBattleDefinitions definitions)
    {
        if (!definitions.Spells.TryGetValue(new("detox", 1), out var spell)) yield break;
        Require(spell is { BaseId: 2, Code: "DETOX", MpCost: 3, Radius: 0, Power: 0, MinimumRange: 0, MaximumRange: 1 } &&
            spell.PropertiesExpression == "TYPE_HEAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE", "source-detox-definition", "spells.detox", true);
        yield return new(spell.Spell, spell.MpCost, 0, spell.MinimumRange, spell.MaximumRange, CuresPoison: true);
    }

    internal static IEnumerable<RetreatItemDefinition> RetreatItems(PrivateBattleDefinitions definitions)
    {
        if (!definitions.Items.TryGetValue(4, out var item)) yield break;
        Require(item.Code == "ANGEL_WING" &&
            item.ItemTypeExpression == "CONSUMABLE" && item.UseSpell == "EGRESS" && item.UseSpellLevel == 1 &&
            item.EquipFlagsExpression == "NONE" && item.MinimumRange == 0 && item.MaximumRange == 0 &&
            item.EquipEffects.All(effect => effect is { Type: "NONE", Parameter: 0 }),
            "source-wing-definition", "items", true);
        _ = RetreatSpells(definitions).Single();
        yield return new(item!.Id, item.DisplayName);
    }

    internal static BattleEquipmentDefinition? EnemyEquipment(PrivateBattleDefinitions definitions, byte attack, IReadOnlyList<ushort> items)
    {
        var weapons = definitions.Items.Values.Where(item => item.ItemTypeExpression == "WEAPON" && item.UseSpell == "NOTHING" &&
            (item.MinimumRange, item.MaximumRange) is (1, 1) or (1, 2) or (2, 2) && item.EquipEffects.All(effect => effect.Type == "INCREASE_ATT" || effect is { Type: "NONE", Parameter: 0 }))
            .Select(item => new BattleWeaponDefinition(item.Id, item.DisplayName, 0,
                checked((byte)item.EquipEffects.Where(effect => effect.Type == "INCREASE_ATT").Sum(effect => effect.Parameter)),
                item.MinimumRange, item.MaximumRange)).ToArray();
        if (!items.Any(word => weapons.Any(w => w.ItemId == (word & 127)))) return null;
        return new(attack, weapons, allowChanges: false);
    }

    internal static PhysicalActorDefinition Physical(PrivateBattleDefinitions definitions, string prowess,
        IEnumerable<ushort> items, bool leader, ushort gold)
    {
        var critical = prowess switch
        {
            "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.OneIn16WithQuarterBonus,
            "CRITICAL150_1IN32|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.OneIn32WithHalfBonus,
            "CRITICAL125_1IN16|DOUBLE_1IN16|COUNTER_1IN32" => PhysicalCriticalRule.Thief,
            "CRITICAL150_1IN32|DOUBLE_1IN32|COUNTER_1IN16" => PhysicalCriticalRule.Cleric,
            "CRITICAL150_1IN32|DOUBLE_1IN16|COUNTER_1IN16" => PhysicalCriticalRule.OneIn32WithFrequentDoubleAndCounter,
            "CRITICAL_SILENCE|DOUBLE_1IN16|COUNTER_1IN16" => PhysicalCriticalRule.Silence,
            "CRITICAL_POISON|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.Poison,
            "CRITICAL_SLEEP|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.Sleep,
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
            Require((item.MinimumRange, item.MaximumRange) is (1, 1) or (1, 2) or (2, 2),
                "source-weapon-range", "actor.items", true);
            minimum = item.MinimumRange; maximum = item.MaximumRange;
        }
        return new(critical, Promoted: false, leader, gold, minimum, maximum);
    }
}

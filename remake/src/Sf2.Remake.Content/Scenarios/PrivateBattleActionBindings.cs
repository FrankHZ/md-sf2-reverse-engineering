using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
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
        // Retail POWDER rank 1 dispatches the poison-only DETOX effect.
        foreach (var (id, code, spellCode, level, power, mpCost, range, curesPoison, effect) in
            new (byte, string, string, byte, ushort, byte, byte, bool, HealingItemEffect)[]
        {
            (0, "MEDICAL_HERB", "HEALIN", 1, 10, 0, 1, false, HealingItemEffect.RestoreHp),
            (1, "HEALING_SEED", "HEALIN", 2, 20, 0, 1, false, HealingItemEffect.RestoreHp),
            (2, "HEALING_DROP", "HEALIN", 3, 30, 0, 1, false, HealingItemEffect.RestoreHp),
            (3, "ANTIDOTE", "DETOX", 1, 0, 3, 1, true, HealingItemEffect.RestoreHp),
            (5, "FAIRY_POWDER", "POWDER", 1, 0, 0, 1, true, HealingItemEffect.RestoreHp),
            (6, "HEALING_WATER", "HEAL", 4, 255, 20, 1, false, HealingItemEffect.FullHp),
            (7, "FAIRY_TEAR", "G_TEAR", 2, 20, 0, 1, false, HealingItemEffect.RestoreMp),
            (8, "HEALING_RAIN", "AURA", 4, 255, 20, 0, false, HealingItemEffect.FullPartyHp),
        })
        {
            if (!definitions.Items.TryGetValue(id, out var item)) continue;
            Require(item.Code == code && item.ItemTypeExpression == (id == 8 ? "RARE|CONSUMABLE" : "CONSUMABLE") &&
                item.UseSpell == spellCode && item.UseSpellLevel == level && item.EquipFlagsExpression == "NONE" &&
                item.MinimumRange == 0 && item.MaximumRange == range &&
                item.EquipEffects.All(e => e is { Type: "NONE", Parameter: 0 }), "source-supply-definition", "items", true);
            Require(definitions.Spells.TryGetValue(new(spellCode.ToLowerInvariant(), level), out var spell) &&
                spell.MpCost == mpCost && spell.Radius == 0 && spell.Power == power &&
                spell.MinimumRange == 0 && spell.MaximumRange == range && spell.PropertiesExpression ==
                    (mpCost > 0 ? "TYPE_HEAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE" : "TYPE_HEAL|TARGET_TEAMMATES"),
                "source-supply-effect", "items.useSpell", true);
            yield return new(item.Id, item.DisplayName, spell!.Power, spell.MinimumRange, spell.MaximumRange, curesPoison, effect);
        }
    }

    internal static IEnumerable<SupportItemDefinition> SupportItems(PrivateBattleDefinitions definitions)
    {
        foreach (var (id, code, effect) in new (byte, string, string)[]
            { (19, "POWER_RING", "ATTACK"), (20, "PROTECT_RING", "BOOST") })
        {
            if (!definitions.Items.TryGetValue(id, out var item)) continue;
            Require(item.Code == code && item.ItemTypeExpression == "RING|RARE|BREAKABLE" &&
                item.EquipFlagsExpression == "ALL" && item.UseSpell == effect && item.UseSpellLevel == 1,
                "source-support-item", "items.useSpell", true);
            yield return new(id, item.DisplayName, PrivateSupportSpellBindings.Read(definitions, new(effect.ToLowerInvariant(), 1)));
        }
        if (!definitions.Items.TryGetValue(92, out var staff)) yield break;
        Require(IsIndraStaff(staff), "source-indra-staff", "items.useSpell", true);
        Require(definitions.Spells.TryGetValue(new("spoit", 1), out var drain) &&
            drain is { BaseId: 15, Code: "SPOIT", Level: 1, MpCost: 0, Power: 10, MinimumRange: 1, MaximumRange: 2,
                Radius: 0, PropertiesExpression: "TYPE_SPECIAL", AnimationExpression: "ABSORB" },
            "source-indra-effect", "items.useSpell", true);
        yield return new(staff.Id, staff.DisplayName, new(drain!.Spell, drain.MpCost, drain.MinimumRange, drain.MaximumRange, drain.Radius));
    }

    internal static bool IsIndraStaff(SourceItemDefinition item) =>
        item is { Id: 92, Code: "INDRA_STAFF", ItemTypeExpression: "WEAPON|BREAKABLE",
            EquipFlagsExpression: "WIZ|SORC|VICR", UseSpell: "SPOIT", UseSpellLevel: 1,
            Price: 3200, MinimumRange: 1, MaximumRange: 1,
            EquipEffects: [{ Type: "INCREASE_ATT", Parameter: 25 }, { Type: "NONE", Parameter: 0 }, { Type: "NONE", Parameter: 0 }] };

    internal static bool IsBlackRing(SourceItemDefinition item) =>
        item is { Id: 24, Code: "BLACK_RING", ItemTypeExpression: "RING|RARE|CURSED|BREAKABLE",
            EquipFlagsExpression: "WIZ|SORC", UseSpell: "BLAZE", UseSpellLevel: 2,
            Price: 5000, MinimumRange: 0, MaximumRange: 0,
            EquipEffects: [{ Type: "INCREASE_ATT", Parameter: 10 }, { Type: "NONE", Parameter: 0 }, { Type: "NONE", Parameter: 0 }] };

    internal static IEnumerable<AttackItemDefinition> AttackItems(PrivateBattleDefinitions definitions)
    {
        foreach (var row in new (byte Item, string Code, string Effect, byte BaseId, ushort Power,
            AttackSpellElement Element, string Animation)[]
        {
            (16, "SHINING_BALL", "FBALL", 37, 20, AttackSpellElement.Fire, "BLAZE|VARIATION3"),
            (17, "BLIZZARD", "BREZAD", 38, 33, AttackSpellElement.Ice, "FREEZE|VARIATION3"),
        })
        {
            if (!definitions.Items.TryGetValue(row.Item, out var item)) continue;
            Require(item.Code == row.Code && item.ItemTypeExpression == "RARE|CONSUMABLE" &&
                item.EquipFlagsExpression == "NONE" && item.MinimumRange == 0 && item.MaximumRange == 1 &&
                item.UseSpell == row.Effect && item.UseSpellLevel == 1 &&
                item.EquipEffects.All(effect => effect is { Type: "NONE", Parameter: 0 }),
                "source-attack-item", "items", true);
            Require(definitions.Spells.TryGetValue(new(row.Effect.ToLowerInvariant(), 1), out var spell) &&
                spell.BaseId == row.BaseId && spell.MpCost == 0 && spell.Power == row.Power &&
                spell.MinimumRange == 1 && spell.MaximumRange == 1 && spell.Radius == 0 &&
                spell.PropertiesExpression == "TYPE_ATTACK" && spell.AnimationExpression == row.Animation,
                "source-attack-item-effect", "items.useSpell", true);
            yield return new(item.Id, item.DisplayName, new(spell!.Spell, spell.MpCost, spell.Power,
                spell.MinimumRange, spell.MaximumRange, spell.Radius, row.Element));
        }
        if (!definitions.Items.TryGetValue(24, out var ring) || !IsBlackRing(ring)) yield break;
        Require(definitions.Spells.TryGetValue(new("blaze", 2), out var blaze) &&
            blaze is { BaseId: 11, Code: "BLAZE", Level: 2, MpCost: 6, Power: 10,
                MinimumRange: 1, MaximumRange: 2, Radius: 1, PropertiesExpression: "TYPE_ATTACK|AFFECTEDBYSILENCE",
                AnimationExpression: "BLAZE|VARIATION2" }, "source-black-ring-effect", "items.useSpell", true);
        yield return new(ring.Id, ring.DisplayName, new(blaze!.Spell, 0, blaze.Power,
            blaze.MinimumRange, blaze.MaximumRange, blaze.Radius, AttackSpellElement.Fire), Breakable: true);
    }

    internal static IEnumerable<HealingSpellDefinition> PoisonCure(PrivateBattleDefinitions definitions)
    {
        foreach (var (rank, cost, range, radius) in new (byte, byte, byte, byte)[]
            { (1, 3, 1, 0), (2, 6, 2, 0), (3, 10, 2, 0), (4, 16, 3, 2) })
        {
            if (!definitions.Spells.TryGetValue(new("detox", rank), out var spell)) continue;
            Require(spell is { BaseId: 2, Code: "DETOX", Power: 0, MinimumRange: 0 } &&
                spell.MpCost == cost && spell.MaximumRange == range && spell.Radius == radius &&
                spell.PropertiesExpression == "TYPE_HEAL|TARGET_TEAMMATES|AFFECTEDBYSILENCE", "source-detox-definition", "spells.detox", true);
            yield return new(spell.Spell, spell.MpCost, 0, spell.MinimumRange, spell.MaximumRange,
                rank == 4 ? HealingTargets.CureRadius : HealingTargets.Single, spell.Radius, CuresPoison: true);
        }
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

    internal static bool HasSupportedWeaponEffects(SourceItemDefinition item) =>
        (item.MinimumRange, item.MaximumRange) is (1, 1) or (1, 2) or (2, 2) or (2, 3) &&
        item.EquipEffects.All(effect => effect.Type is "INCREASE_ATT" or "DECREASE_ATT" or
            "INCREASE_DEF" or "DECREASE_DEF" or "INCREASE_AGI" or "DECREASE_AGI" or
            "INCREASE_MOV" or "DECREASE_MOV" or "INCREASE_CRITICAL" || effect is { Type: "NONE", Parameter: 0 });

    internal static BattleWeaponDefinition Weapon(SourceItemDefinition item, uint classMask) =>
        new(item.Id, item.DisplayName, classMask,
            checked((byte)item.EquipEffects.Where(effect => effect.Type == "INCREASE_ATT").Sum(effect => effect.Parameter)),
            item.MinimumRange, item.MaximumRange,
            checked((byte)item.EquipEffects.Where(effect => effect.Type == "INCREASE_CRITICAL").Sum(effect => effect.Parameter)),
            DamageOnUse: item.ItemTypeExpression.Split('|').Contains("BREAKABLE"),
            Stats: new(item.EquipEffects.Where(effect => effect.Type is not ("NONE" or "INCREASE_CRITICAL"))
                .Select(effect => new BattleEquipmentStat(effect.Type[9..] switch
                {
                    "ATT" => RingStat.Attack, "DEF" => RingStat.Defense,
                    "AGI" => RingStat.Agility, "MOV" => RingStat.Move,
                    _ => throw new BattleRuleException("source-equipment-effects", "items", true),
                }, effect.Type.StartsWith("DECREASE", StringComparison.Ordinal) ? -effect.Parameter : effect.Parameter))),
            Cursed: item.ItemTypeExpression.Split('|').Contains("CURSED"));

    internal static BattleEquipmentDefinition? EnemyEquipment(PrivateBattleDefinitions definitions, byte attack, IReadOnlyList<ushort> items)
    {
        var weapons = definitions.Items.Values.Where(item => item.ItemTypeExpression.Split('|').Contains("WEAPON") &&
            HasSupportedWeaponEffects(item))
            .Select(item => Weapon(item, 0)).ToArray();
        // Bare placement items can be carried without enabling their use or equip effects.
        var inert = definitions.Items.Values.Where(item => item is
            { Id: 19, Code: "POWER_RING" } or { Id: 20, Code: "PROTECT_RING" } or { Id: 24, Code: "BLACK_RING" } or { Id: 9, Code: "POWER_WATER" } or { Id: 16, Code: "SHINING_BALL" } or { Id: 17, Code: "BLIZZARD" })
            .Select(item => item.Id).ToArray();
        if (!items.Any(word => weapons.Any(w => w.ItemId == (word & 127)) || word < 127 && inert.Contains((byte)word))) return null;
        return new(attack, weapons, allowChanges: false, inertItems: inert);
    }

    internal static PhysicalActorDefinition Physical(PrivateBattleDefinitions definitions, string prowess,
        IEnumerable<ushort> items, bool leader, ushort gold, byte unarmedMaximum = 1, bool cannotCounter = false)
    {
        var critical = prowess switch
        {
            "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.OneIn16WithQuarterBonus,
            "CRITICAL150_1IN32|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.OneIn32WithHalfBonus,
            "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN16" => PhysicalCriticalRule.BrassGunner,
            "CRITICAL125_1IN16|DOUBLE_1IN16|COUNTER_1IN8" => PhysicalCriticalRule.Hero,
            "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN8" => PhysicalCriticalRule.WolfBaron,
            "CRITICAL125_1IN16|DOUBLE_1IN16|COUNTER_1IN32" => PhysicalCriticalRule.Thief,
            "CRITICAL150_1IN8|DOUBLE_1IN16|COUNTER_1IN8" => PhysicalCriticalRule.Ninja,
            "CRITICAL150_1IN32|DOUBLE_1IN32|COUNTER_1IN16" => PhysicalCriticalRule.Cleric,
            "CRITICAL150_1IN32|DOUBLE_1IN16|COUNTER_1IN16" => PhysicalCriticalRule.OneIn32WithFrequentDoubleAndCounter,
            "CRITICAL150_1IN32|DOUBLE_1IN16|COUNTER_1IN32" => PhysicalCriticalRule.OneIn32WithFrequentDouble,
            "CRITICAL150_1IN4|DOUBLE_1IN16|COUNTER_1IN8" => PhysicalCriticalRule.Taros,
            "CRITICAL150_1IN4|DOUBLE_1IN16|COUNTER_1IN16" => PhysicalCriticalRule.Monk,
            "CRITICAL150_1IN8|DOUBLE_1IN32|COUNTER_1IN16" => PhysicalCriticalRule.Orc,
            "CRITICAL150_1IN8|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.OneIn8WithHalfBonus,
            "CRITICAL150_1IN4|DOUBLE_1IN8|COUNTER_1IN8" => PhysicalCriticalRule.OneIn4WithFrequentDoubleAndCounter,
            "CRITICAL_SILENCE|DOUBLE_1IN32|COUNTER_1IN16" => PhysicalCriticalRule.OozeSilence,
            "CRITICAL125_1IN4|DOUBLE_1IN8|COUNTER_1IN16" => PhysicalCriticalRule.MasterMonk,
            "CRITICAL_SILENCE|DOUBLE_1IN16|COUNTER_1IN16" => PhysicalCriticalRule.Silence,
            "CRITICAL_POISON|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.Poison,
            "CRITICAL_STUN|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.Stun,
            "CRITICAL_SLEEP|DOUBLE_1IN32|COUNTER_1IN32" => PhysicalCriticalRule.Sleep,
            _ => throw new BattleRuleException("source-prowess", "actor.prowess", true),
        };
        var equipped = items.Where(word => (word & 128) != 0).Select(word => definitions.Items[(byte)(word & 127)]).ToArray();
        Require(equipped.Length <= 1, "source-equipment", "actor.items", true);
        byte minimum = 1, maximum = unarmedMaximum; // Unarmed source attack range.
        foreach (var item in equipped)
        {
            Require(item.ItemTypeExpression.Split('|').Contains("WEAPON") && HasSupportedWeaponEffects(item),
                "source-equipment-effects", "actor.items", true);
            Require((item.MinimumRange, item.MaximumRange) is (1, 1) or (1, 2) or (2, 2) or (2, 3),
                "source-weapon-range", "actor.items", true);
            minimum = item.MinimumRange; maximum = item.MaximumRange;
        }
        return new(critical, Promoted: false, leader, gold, minimum, maximum, cannotCounter);
    }
}

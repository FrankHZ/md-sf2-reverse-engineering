using System.Collections.Frozen;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateFieldPreparation
{
    // Product binding only. Reuse admitted field carry identities, not their Use/Drop capabilities.
    // BattleGive revalidates both live four-slot loadouts and admitted equipped terms.
    internal static BattleDefinition Give(BattleDefinition battle, FieldPreparationDefinition preparation) =>
        battle.WithGive(new(preparation.Items.Values.Select(item => KeyValuePair.Create(item.ItemId, item.Name))));

    // The battle caller supplies its own refusal/confirmation/removal semantics.
    // Share only admitted identity, rarity and unsellable metadata, not field Drop execution.
    internal static BattleDefinition Discard(BattleDefinition battle, FieldPreparationDefinition preparation) =>
        battle.WithDiscard(new(preparation.Items.Values));

    internal static BattleDefinition Equipment(BattleDefinition battle, PrivateBattleDefinitions source, bool allowBattleChanges = false)
    {
        var weapons = Weapons(source);
        var rings = Rings(source).ToArray();
        return battle.WithDeployments(battle.Deployments.Select(row =>
        {
            if (row.Faction != BattleFaction.Ally || (!allowBattleChanges && row.Definition.Equipment is not null)) return row;
            // Product New Game/Continue explicitly opt into the existing weapon transaction.
            // Field-only/controlled consumers and enemy equipment keep their immutable binding.
            if (allowBattleChanges)
                Require(row.Control == BattleControl.Player && Sf2ClassRules.SourceClass(row.Definition.ClassRule) is not null &&
                    row.Definition.Growth is { } growth && row.Definition.Physical?.Promoted == (growth.ClassId > 11),
                    "source-battle-equipment-actor", "actor.equipment", true);
            int bonus = row.Definition.Growth?.AttackBonus ?? row.Definition.SourceLoadout?.Items
                .Where(word => (word & 128) != 0).Sum(word => weapons.Single(item => item.ItemId == (word & 127)).AttackBonus) ?? 0;
            Require(row.Definition.Attack >= bonus, "source-base-attack", "actor.attack");
            var definition = row.Definition.WithEquipment(new((byte)(row.Definition.Attack - bonus), weapons,
                allowChanges: allowBattleChanges, rings: rings));
            if (allowBattleChanges) BattleEquipment.ValidateLoadout(definition, definition.SourceLoadout);
            return row with { Definition = definition, PromotionVariants = row.PromotionVariants
                .ToFrozenDictionary(pair => pair.Key, pair => pair.Value.WithEquipment(definition.Equipment!)) };
        }));
    }
    internal static BattleWeaponDefinition[] Weapons(PrivateBattleDefinitions source)
    {
        return source.Items.Values.Where(item => item.ItemTypeExpression.Split('|').Contains("WEAPON") &&
            PrivateBattleActionBindings.HasSupportedWeaponEffects(item))
            .Select(item => PrivateBattleActionBindings.IsIndraStaff(item)
                ? PrivateBattleActionBindings.Weapon(item, 1u << 17 | 1u << 18 | 1u << 19) with { DamageOnUse = true }
                : PrivateBattleActionBindings.Weapon(item,
                    source.Classes.Values.Where(allowed => allowed.Id < 32 && item.EquipFlags.Contains(allowed.Code))
                        .Aggregate(0u, (mask, allowed) => mask | 1u << allowed.Id))).ToArray();
    }
    private static IEnumerable<BattleRingDefinition> Rings(PrivateBattleDefinitions source)
    {
        foreach (var (id, code, kind, effect, stat, bonus) in new (byte, string, string, string, RingStat, byte)[]
        {
            (19, "POWER_RING", "RING|RARE|BREAKABLE", "INCREASE_ATT", RingStat.Attack, 5),
            (20, "PROTECT_RING", "RING|RARE|BREAKABLE", "INCREASE_DEF", RingStat.Defense, 5),
            (21, "QUICK_RING", "RING|RARE|BREAKABLE", "INCREASE_AGI", RingStat.Agility, 5),
            (22, "RUNNING_RING", "RING|RARE", "INCREASE_MOV", RingStat.Move, 2),
        })
        {
            if (!source.Items.TryGetValue(id, out var item)) continue;
            Require(item is { EquipFlagsExpression: "ALL", MinimumRange: 0, MaximumRange: 0 } &&
                item.Code == code && item.ItemTypeExpression == kind &&
                item.EquipEffects is [{ } primary, { Type: "NONE", Parameter: 0 }, { Type: "NONE", Parameter: 0 }] &&
                primary.Type == effect && primary.Parameter == bonus, "source-static-ring", "field.items");
            yield return new(id, item.DisplayName, uint.MaxValue, stat, bonus,
                DamageOnUse: kind.EndsWith("|BREAKABLE", StringComparison.Ordinal) && item.UseSpell != "NOTHING");
        }
        if (source.Items.TryGetValue(24, out var blackRing) && PrivateBattleActionBindings.IsBlackRing(blackRing))
            yield return new(blackRing.Id, blackRing.DisplayName, (1u << 17) | (1u << 18),
                RingStat.Attack, 10, DamageOnUse: true, Cursed: true);
        foreach (var (id, code, stat, effect, bonus, classes) in new (byte, string, RingStat, string, byte, uint)[]
        {
            (23, "WHITE_RING", RingStat.Defense, "INCREASE_DEF", 10, (1u << 12) | (1u << 19)),
            (25, "EVIL_RING", RingStat.Attack, "INCREASE_ATT", 15, (1u << 17) | (1u << 18) | (1u << 19)),
        })
        {
            if (!source.Items.TryGetValue(id, out var item)) continue;
            Require(item.Code == code && item.EquipEffects is [{ } primary, { Type: "NONE", Parameter: 0 }, { Type: "NONE", Parameter: 0 }] &&
                primary.Type == effect && primary.Parameter == bonus, "source-static-ring", "field.items");
            yield return new(id, item.DisplayName, classes, stat, bonus, DamageOnUse: true, Cursed: id == 25);
        }
        if (source.Items.TryGetValue(126, out var sandals))
        {
            Require(sandals is { Code: "CHIRRUP_SANDALS", EquipFlagsExpression: "ALL", ItemTypeExpression: "RING|RARE",
                MinimumRange: 0, MaximumRange: 0, UseSpell: "NOTHING",
                EquipEffects: [{ Type: "NONE", Parameter: 0 }, { Type: "NONE", Parameter: 0 }, { Type: "NONE", Parameter: 0 }] },
                "source-sound-ring", "field.items");
            yield return new(sandals.Id, sandals.DisplayName, uint.MaxValue, null, 0, MovementSound: 99,
                DepotDescriptionKey: "depot.description.chirrup-sandals");
        }
    }

    internal static FieldPreparationDefinition Read(PrivateBattleDefinitions source, ExplorationDefinition? world = null, IEnumerable<string>? wingMaps = null,
        FieldEgressDefinition? egress = null)
    {
        var weapons = Weapons(source);
        var rings = Rings(source).Select(ring => ring.ItemId).ToHashSet();
        // A selected reward needs a carry identity even when its equipment or Use effect is unsupported.
        var pickupIds = (world?.Maps.Values ?? []).SelectMany(map =>
                map.Setups.Values.Select(setup => setup.Search).Append(map.Search))
            .OfType<FieldSearchDefinition>().SelectMany(search => search.Chests.Concat(search.OtherItems))
            .Where(row => row.ItemWord.HasValue).Select(row => (byte)(row.ItemWord!.Value & 127)).ToHashSet();
        pickupIds.UnionWith((world?.Programs.Values ?? []).SelectMany(program => program.Instructions).OfType<TryAddActorItem>().Select(row => row.Item));
        pickupIds.UnionWith((world?.Programs.Values ?? []).SelectMany(program => program.Instructions).OfType<TryAddPartyItem>().Select(row => row.Item));
        pickupIds.UnionWith((world?.Programs.Values ?? []).SelectMany(program => program.Instructions).OfType<OpenShop>()
            .SelectMany(shop => shop.Definition.Stock));
        var eventItems = (world?.Maps.Values ?? []).SelectMany(map =>
                map.Events.Concat(map.Setups.Values.SelectMany(setup => setup.Events)))
            .Where(row => row.Kind == ExplorationEventKind.UseItem).Select(row => row.Item!.Value).ToHashSet();
        var itemEffects = FieldItems(source).Concat(eventItems.Select(id => KeyValuePair.Create(id, FieldRecoveryEffect.MapEvent)));
        var wingNoEffectMaps = wingMaps ?? [];
        return new(source.Items.Values.Where(item => weapons.Any(weapon => weapon.ItemId == item.Id) || rings.Contains(item.Id) ||
                item.ItemTypeExpression == "CONSUMABLE" || item.Code is "MITHRIL" or "TAROS_SWORD" or "SHINING_BALL" or "HEALING_RAIN" or "BLACK_RING" ||
                (item.Id != 127 && (pickupIds.Contains(item.Id) || eventItems.Contains(item.Id)))).Select(item => new BattleDiscardItem(item.Id, item.DisplayName,
                    item.ItemTypeExpression.Split('|', StringSplitOptions.TrimEntries).Contains("RARE"),
                    item.ItemTypeExpression.Split('|', StringSplitOptions.TrimEntries).Contains("UNSELLABLE"))),
                itemEffects, source.Spells.Values.Where(spell => spell.Code == "HEAL" ||
                    egress is not null && spell is { Code: "EGRESS", Level: 1 })
                    .Select(spell => new FieldSpellChoice(spell.Spell, spell.DisplayName, spell.MpCost,
                        spell.Code == "EGRESS" ? FieldRecoveryEffect.Egress : FieldRecoveryEffect.NoEffect))
                    .Concat(PrivateBattleActionBindings.PoisonCure(source).Select(cure =>
                        new FieldSpellChoice(cure.Spell, source.Spells[cure.Spell].DisplayName, cure.MpCost,
                            cure.Spell.Level == 1 ? FieldRecoveryEffect.CurePoison : FieldRecoveryEffect.CurePoisonAndParalysis))),
                [new(4, FieldRecoveryEffect.NoEffect, wingNoEffectMaps.Select(id => new Sf2.Remake.Domain.Maps.MapId(id)).ToArray())], egress);
    }
    private static IEnumerable<KeyValuePair<byte, FieldRecoveryEffect>> FieldItems(Application.Content.Scenarios.PrivateBattleDefinitions source)
    {
        foreach (var (id, code, useSpell, level, effect) in new (byte, string, string, byte, FieldRecoveryEffect)[]
            { (0, "MEDICAL_HERB", "HEALIN", 1, FieldRecoveryEffect.NoEffect),
              (1, "HEALING_SEED", "HEALIN", 2, FieldRecoveryEffect.NoEffect),
              (2, "HEALING_DROP", "HEALIN", 3, FieldRecoveryEffect.NoEffect),
              (3, "ANTIDOTE", "DETOX", 1, FieldRecoveryEffect.CurePoison),
              (5, "FAIRY_POWDER", "POWDER", 1, FieldRecoveryEffect.CurePoisonAndParalysis),
              (6, "HEALING_WATER", "HEAL", 4, FieldRecoveryEffect.NoEffect),
              (7, "FAIRY_TEAR", "G_TEAR", 2, FieldRecoveryEffect.NoEffect),
              (8, "HEALING_RAIN", "AURA", 4, FieldRecoveryEffect.NoEffect),
              (9, "POWER_WATER", "POWER", 1, FieldRecoveryEffect.IncreaseAttack),
              (10, "PROTECT_MILK", "GUARD", 1, FieldRecoveryEffect.IncreaseDefense),
              (11, "QUICK_CHICKEN", "SPEED", 1, FieldRecoveryEffect.IncreaseAgility),
              (12, "RUNNING_PIMENTO", "IDATEN", 1, FieldRecoveryEffect.IncreaseMove),
              (13, "CHEERFUL_BREAD", "HEALTH", 1, FieldRecoveryEffect.IncreaseHp),
              (14, "BRIGHT_HONEY", "HANNY", 1, FieldRecoveryEffect.IncreaseMp),
              (15, "BRAVE_APPLE", "BRAVE", 1, FieldRecoveryEffect.LevelUp) })
        {
            if (!source.Items.TryGetValue(id, out var item)) continue;
            Require(item.Code == code && item.UseSpell == useSpell && item.UseSpellLevel == level &&
                item.ItemTypeExpression == (id == 8 ? "RARE|CONSUMABLE" : "CONSUMABLE") && item.EquipEffects.All(effect => effect is { Type: "NONE", Parameter: 0 }),
                "source-field-item", "field.items");
            yield return new(id, effect);
        }
    }
}

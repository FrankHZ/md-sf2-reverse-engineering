using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateFieldPreparation
{
    // Product binding only. Reuse admitted field carry identities, not their Use/Drop capabilities.
    // BattleGive revalidates both live four-slot loadouts and ordinary equipped weapons.
    internal static BattleDefinition Give(BattleDefinition battle, FieldPreparationDefinition preparation) =>
        battle.WithGive(new(preparation.Items.Values.Select(item => KeyValuePair.Create(item.ItemId, item.Name))));

    // The battle caller supplies its own refusal/confirmation/removal semantics.
    // Share only admitted identity, rarity and unsellable metadata, not field Drop execution.
    internal static BattleDefinition Discard(BattleDefinition battle, FieldPreparationDefinition preparation) =>
        battle.WithDiscard(new(preparation.Items.Values));

    internal static BattleDefinition Equipment(BattleDefinition battle, PrivateBattleDefinitions source, bool allowBattleChanges = false)
    {
        var weapons = Weapons(source);
        return battle.WithDeployments(battle.Deployments.Select(row =>
        {
            if (row.Faction != BattleFaction.Ally || (!allowBattleChanges && row.Definition.Equipment is not null)) return row;
            // Product New Game/Continue explicitly opt into the existing weapon transaction.
            // Field-only/controlled consumers and enemy equipment keep their immutable binding.
            if (allowBattleChanges)
                Require(row.Control == BattleControl.Player && Sf2ClassRules.SourceClass(row.Definition.ClassRule) is not null &&
                    row.Definition.Physical is { Promoted: false } && row.Definition.Growth is not null,
                    "source-battle-equipment-actor", "actor.equipment", true);
            int bonus = row.Definition.Growth?.AttackBonus ?? row.Definition.SourceLoadout?.Items
                .Where(word => (word & 128) != 0).Sum(word => weapons.Single(item => item.ItemId == (word & 127)).AttackBonus) ?? 0;
            Require(row.Definition.Attack >= bonus, "source-base-attack", "actor.attack");
            var definition = row.Definition.WithEquipment(new((byte)(row.Definition.Attack - bonus), weapons, allowChanges: allowBattleChanges));
            if (allowBattleChanges) BattleEquipment.ValidateLoadout(definition, definition.SourceLoadout);
            return row with { Definition = definition };
        }));
    }
    internal static BattleWeaponDefinition[] Weapons(PrivateBattleDefinitions source)
    {
        return source.Items.Values.Where(item => (item.ItemTypeExpression == "WEAPON" || item is { Code: "ACHILLES_SWORD", ItemTypeExpression: "WEAPON|RARE" }) &&
            (item.MinimumRange, item.MaximumRange) is (1, 1) or (1, 2) or (2, 2) && item.EquipEffects.All(effect =>
                effect.Type == "INCREASE_ATT" || effect is { Type: "NONE", Parameter: 0 }))
            .Select(item => new BattleWeaponDefinition(item.Id, item.DisplayName,
                source.Classes.Values.Where(allowed => allowed.Id < 32 && item.EquipFlags.Contains(allowed.Code))
                    .Aggregate(0u, (mask, allowed) => mask | 1u << allowed.Id),
                checked((byte)item.EquipEffects.Where(effect => effect.Type == "INCREASE_ATT").Sum(effect => effect.Parameter)),
                item.MinimumRange, item.MaximumRange)).ToArray();
    }
    internal static FieldPreparationDefinition Read(PrivateBattleDefinitions source, ExplorationDefinition? world = null, IEnumerable<string>? wingMaps = null,
        FieldEgressDefinition? egress = null)
    {
        var weapons = Weapons(source);
        // A selected reward needs a carry identity even when its equipment or Use effect is unsupported.
        var pickupIds = (world?.Maps.Values ?? []).SelectMany(map =>
                map.Setups.Values.Select(setup => setup.Search).Append(map.Search))
            .OfType<FieldSearchDefinition>().SelectMany(search => search.Chests.Concat(search.OtherItems))
            .Where(row => row.ItemWord.HasValue).Select(row => (byte)(row.ItemWord!.Value & 127)).ToHashSet();
        var eventItems = (world?.Maps.Values ?? []).SelectMany(map =>
                map.Events.Concat(map.Setups.Values.SelectMany(setup => setup.Events)))
            .Where(row => row.Kind == ExplorationEventKind.UseItem).Select(row => row.Item!.Value).ToHashSet();
        var itemEffects = FieldItems(source).Concat(eventItems.Select(id => KeyValuePair.Create(id, FieldRecoveryEffect.MapEvent)));
        var wingNoEffectMaps = wingMaps ?? [];
        return new(source.Items.Values.Where(item => weapons.Any(weapon => weapon.ItemId == item.Id) ||
                item.ItemTypeExpression == "CONSUMABLE" || item.Code is "MITHRIL" or "TAROS_SWORD" ||
                (item.Id != 127 && (pickupIds.Contains(item.Id) || eventItems.Contains(item.Id)))).Select(item => new BattleDiscardItem(item.Id, item.DisplayName,
                    item.ItemTypeExpression.Split('|', StringSplitOptions.TrimEntries).Contains("RARE"),
                    item.ItemTypeExpression.Split('|', StringSplitOptions.TrimEntries).Contains("UNSELLABLE"))),
                itemEffects, source.Spells.Values.Where(spell => spell.Code == "HEAL" || spell is { Code: "DETOX", Level: 1 } ||
                    egress is not null && spell is { Code: "EGRESS", Level: 1 })
                    .Select(spell => new FieldSpellChoice(spell.Spell, spell.DisplayName, spell.MpCost,
                        spell.Code switch { "DETOX" => FieldRecoveryEffect.CurePoison, "EGRESS" => FieldRecoveryEffect.Egress, _ => FieldRecoveryEffect.NoEffect })),
                [new(4, FieldRecoveryEffect.NoEffect, wingNoEffectMaps.Select(id => new Sf2.Remake.Domain.Maps.MapId(id)).ToArray())], egress);
    }
    private static IEnumerable<KeyValuePair<byte, FieldRecoveryEffect>> FieldItems(Application.Content.Scenarios.PrivateBattleDefinitions source)
    {
        foreach (var (id, code, useSpell, level) in new (byte, string, string, byte)[]
            { (0, "MEDICAL_HERB", "HEALIN", 1), (1, "HEALING_SEED", "HEALIN", 2), (3, "ANTIDOTE", "DETOX", 1) })
        {
            if (!source.Items.TryGetValue(id, out var item)) continue;
            Require(item.Code == code && item.UseSpell == useSpell && item.UseSpellLevel == level &&
                item.ItemTypeExpression == "CONSUMABLE" && item.EquipEffects.All(effect => effect is { Type: "NONE", Parameter: 0 }),
                "source-field-item", "field.items");
            yield return new(id, id == 3 ? FieldRecoveryEffect.CurePoison : FieldRecoveryEffect.NoEffect);
        }
    }
}

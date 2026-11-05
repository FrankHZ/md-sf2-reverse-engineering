using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateFieldPreparation
{
    internal static BattleDefinition Equipment(BattleDefinition battle, PrivateBattleDefinitions source)
    {
        var weapons = Weapons(source);
        return battle.WithDeployments(battle.Deployments.Select(row =>
        {
            if (row.Faction != BattleFaction.Ally || row.Definition.Equipment is not null) return row;
            int bonus = row.Definition.Growth?.AttackBonus ?? row.Definition.SourceLoadout?.Items
                .Where(word => (word & 128) != 0).Sum(word => weapons.Single(item => item.ItemId == (word & 127)).AttackBonus) ?? 0;
            Require(row.Definition.Attack >= bonus, "source-base-attack", "actor.attack");
            return row with { Definition = row.Definition.WithEquipment(new((byte)(row.Definition.Attack - bonus), weapons, allowChanges: false)) };
        }));
    }
    internal static BattleWeaponDefinition[] Weapons(PrivateBattleDefinitions source)
    {
        return source.Items.Values.Where(item => item.ItemTypeExpression == "WEAPON" &&
            (item.MinimumRange, item.MaximumRange) is (1, 1) or (1, 2) or (2, 2) && item.EquipEffects.All(effect =>
                effect.Type == "INCREASE_ATT" || effect is { Type: "NONE", Parameter: 0 }))
            .Select(item => new BattleWeaponDefinition(item.Id, item.DisplayName,
                source.Classes.Values.Where(allowed => allowed.Id < 32 && item.EquipFlags.Contains(allowed.Code))
                    .Aggregate(0u, (mask, allowed) => mask | 1u << allowed.Id),
                checked((byte)item.EquipEffects.Where(effect => effect.Type == "INCREASE_ATT").Sum(effect => effect.Parameter)),
                item.MinimumRange, item.MaximumRange)).ToArray();
    }
    internal static FieldPreparationDefinition Read(PrivateBattleDefinitions source, IEnumerable<string>? wingMaps = null,
        FieldEgressDefinition? egress = null)
    {
        var weapons = Weapons(source);
        var wingNoEffectMaps = wingMaps ?? [];
        return new(source.Items.Values.Where(item => weapons.Any(weapon => weapon.ItemId == item.Id) ||
                item.ItemTypeExpression == "CONSUMABLE").Select(item => new BattleDiscardItem(item.Id, item.DisplayName,
                    item.ItemTypeExpression.Split('|', StringSplitOptions.TrimEntries).Contains("RARE"),
                    item.ItemTypeExpression.Split('|', StringSplitOptions.TrimEntries).Contains("UNSELLABLE"))),
                FieldItems(source), source.Spells.Values.Where(spell => spell.Code == "HEAL" || spell is { Code: "DETOX", Level: 1 } ||
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

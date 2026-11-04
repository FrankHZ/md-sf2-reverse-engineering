using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.Content.Persistence;

internal static class OpeningLoadoutBindings
{
    internal static ScenarioDefinition Bind(ScenarioDefinition admitted)
    {
        var source = admitted.PrivateDefinitions!;
        ScenarioJson.Require(admitted.Exploration!.Programs.TryGetValue("map3-entityevent13", out var church) &&
            church.Instructions is [OpenChurch { Definition: { RaisePerLevel: 10, PromotedExtra: 200, RaisedHpCap: 200, PoisonCost: 10, StunCost: 20 } }, EndProgram] &&
            admitted.Exploration.Maps[new("map-3")].Events.Any(row => row.Program?.Program == church.Id && row.Unavailable is null),
            "opening-church-required", "world.programs");
        // Product opening content must include the source search/receipt surface before
        // either New Game or Continue can publish a session. Saves already carry its flags.
        var openingMaps = new[] { "map-3", "map-19", "map-20", "map-21", "map-57" };
        foreach (string id in openingMaps)
        {
            ScenarioJson.Require(admitted.Exploration!.Maps.TryGetValue(new(id), out var map) && map.Search is not null &&
                map.Setups.Values.All(setup => setup.Search is not null), "opening-search-required", "world.search");
            foreach (var item in map!.Search!.Chests.Concat(map.Search.OtherItems))
                ScenarioJson.Require(item.ItemWord is null || (item.ItemWord.Value & 127) == 127 || source.Items.ContainsKey((byte)(item.ItemWord.Value & 127)),
                    "search-item-identity", "world.search");
        }
        // mainactions @StartItemUse admits Angel Wing egress only on overworld maps.
        // Opening setup item-event handlers directly return (Map57 has no setup).
        // Here Use is NoEffect.
        const byte wing = 4;
        ScenarioJson.Require(source.Items.TryGetValue(wing, out var angelWing) && angelWing.Code == "ANGEL_WING" &&
            angelWing.UseSpell == "EGRESS" && angelWing.UseSpellLevel == 1 &&
            angelWing.ItemTypeExpression == "CONSUMABLE" && angelWing.EquipFlagsExpression == "NONE" &&
            angelWing.MinimumRange == 0 && angelWing.MaximumRange == 0 &&
            angelWing.EquipEffects.All(effect => effect is { Type: "NONE", Parameter: 0 }),
            "source-field-angel-wing", "field.items");
        var weapons = source.Items.Values.Where(item => item.ItemTypeExpression == "WEAPON" &&
            (item.MinimumRange, item.MaximumRange) is (1, 1) or (1, 2) or (2, 2) && item.EquipEffects.All(effect =>
                effect.Type == "INCREASE_ATT" || effect is { Type: "NONE", Parameter: 0 }))
            .Select(item => new BattleWeaponDefinition(item.Id, item.DisplayName,
                source.Classes.Values.Where(allowed => allowed.Id < 32 && item.EquipFlags.Contains(allowed.Code))
                    .Aggregate(0u, (mask, allowed) => mask | 1u << allowed.Id),
                checked((byte)item.EquipEffects.Where(effect => effect.Type == "INCREASE_ATT").Sum(effect => effect.Parameter)),
                item.MinimumRange, item.MaximumRange)).ToArray();
        var encounters = admitted.Encounters.Values.Select(encounter => encounter.WithDeployments(
            encounter.Deployments.Select(deployment =>
                deployment.Faction != BattleFaction.Ally || deployment.Definition.Growth is not { } growth ? deployment :
                deployment with { Definition = deployment.Definition.WithEquipment(new(
                    checked((byte)(deployment.Definition.Attack - growth.AttackBonus)), weapons, allowChanges: false)) })));
        return new(admitted.Package, encounters, source, admitted.Exploration, admitted.BattleScenes, admitted.EncounterStarts.Values,
            admitted.EncounterScenes.Values, admitted.BattleStory, BattleStartOrigin.NewGame)
        {
            Persistence = new Sf2OpeningRestore(),
            FieldPreparation = new(source.Items.Values.Where(item => weapons.Any(weapon => weapon.ItemId == item.Id) ||
                item.ItemTypeExpression == "CONSUMABLE").Select(item => new BattleDiscardItem(item.Id, item.DisplayName,
                    item.ItemTypeExpression.Split('|', StringSplitOptions.TrimEntries).Contains("RARE"),
                    item.ItemTypeExpression.Split('|', StringSplitOptions.TrimEntries).Contains("UNSELLABLE"))),
                FieldItems(source), source.Spells.Values.Where(spell => spell.Code == "HEAL" || spell is { Code: "DETOX", Level: 1 })
                    .Select(spell => new FieldSpellChoice(spell.Spell, spell.DisplayName, spell.MpCost,
                        spell.Code == "DETOX" ? FieldRecoveryEffect.CurePoison : FieldRecoveryEffect.NoEffect)),
                [new(wing, FieldRecoveryEffect.NoEffect, openingMaps.Select(id => new Sf2.Remake.Domain.Maps.MapId(id)).ToArray())]),
        };
    }

    private static IEnumerable<KeyValuePair<byte, FieldRecoveryEffect>> FieldItems(Application.Content.Scenarios.PrivateBattleDefinitions source)
    {
        foreach (var (id, code, useSpell, level) in new (byte, string, string, byte)[]
            { (0, "MEDICAL_HERB", "HEALIN", 1), (1, "HEALING_SEED", "HEALIN", 2), (3, "ANTIDOTE", "DETOX", 1) })
        {
            if (!source.Items.TryGetValue(id, out var item)) continue;
            ScenarioJson.Require(item.Code == code && item.UseSpell == useSpell && item.UseSpellLevel == level &&
                item.ItemTypeExpression == "CONSUMABLE" && item.EquipEffects.All(effect => effect is { Type: "NONE", Parameter: 0 }),
                "source-field-item", "field.items");
            yield return new(id, id == 3 ? FieldRecoveryEffect.CurePoison : FieldRecoveryEffect.NoEffect);
        }
    }
}

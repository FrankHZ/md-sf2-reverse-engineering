using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Content.Persistence;

internal static class OpeningLoadoutBindings
{
    internal static ScenarioDefinition Bind(ScenarioDefinition admitted)
    {
        var source = admitted.PrivateDefinitions!;
        var weapons = source.Items.Values.Where(item => item.ItemTypeExpression == "WEAPON" &&
            item.MinimumRange == 1 && item.MaximumRange == 1 && item.EquipEffects.All(effect =>
                effect.Type == "INCREASE_ATT" || effect is { Type: "NONE", Parameter: 0 }))
            .Select(item => new BattleWeaponDefinition(item.Id, item.DisplayName,
                source.Classes.Values.Where(allowed => allowed.Id < 32 && item.EquipFlags.Contains(allowed.Code))
                    .Aggregate(0u, (mask, allowed) => mask | 1u << allowed.Id),
                checked((byte)item.EquipEffects.Where(effect => effect.Type == "INCREASE_ATT").Sum(effect => effect.Parameter)),
                item.MinimumRange, item.MaximumRange)).ToArray();
        var encounters = admitted.Encounters.Values.Select(encounter => new BattleDefinition(encounter.Encounter, encounter.Map,
            encounter.Width, encounter.Height, encounter.Terrain, encounter.Deployments.Select(deployment =>
                deployment.Faction != BattleFaction.Ally || deployment.Definition.Growth is not { } growth ? deployment :
                deployment with { Definition = deployment.Definition.WithEquipment(new(
                    checked((byte)(deployment.Definition.Attack - growth.AttackBonus)), weapons, allowChanges: false)) }),
            encounter.Spells.Values, encounter.Rewards, encounter.Initialization, encounter.Outcome, encounter.HealingItems.Values,
            encounter.SourceMapRegion, encounter.RetreatSpells.Values, encounter.RetreatItems.Values, encounter.TarosEffectiveness,
            encounter.LeaderCleanup, encounter.ItemDrops, encounter.PrismLaser, encounter.Give, encounter.Discard, encounter.Poison));
        return new(admitted.Package, encounters, source, admitted.Exploration, admitted.BattleScenes, admitted.EncounterStarts.Values,
            admitted.EncounterScenes.Values, admitted.BattleStory, BattleStartOrigin.NewGame) { Persistence = new Sf2OpeningRestore() };
    }
}

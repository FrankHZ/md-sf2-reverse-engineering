using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.Content.Persistence;

internal static class OpeningLoadoutBindings
{
    internal static ScenarioDefinition Bind(ScenarioDefinition admitted, MapId openingMap)
    {
        var source = admitted.PrivateDefinitions!;
        ScenarioJson.Require(admitted.Exploration!.Programs.TryGetValue("map3-entityevent13", out var church) &&
            church.Instructions is [CallProgram { Target: { Instruction: 0 } target }, EndProgram] &&
            admitted.Exploration.Programs.TryGetValue(target.Program, out var entry) &&
            entry.Instructions is [OpenPortrait, ShowText { TextId: 110 }, CloseText, ClosePortrait,
                OpenChurch { Definition: { RaisePerLevel: 10, PromotedExtra: 200, RaisedHpCap: 200, PoisonCost: 10, StunCost: 20 } },
                ReturnProgram] &&
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
        var egress = PrivateFieldEgress.Bind(admitted.Exploration);
        var preparation = PrivateFieldPreparation.Read(source, admitted.Exploration, openingMaps, egress.Capability);
        var encounters = admitted.Encounters.Values.Select(encounter => PrivateFieldPreparation.Discard(PrivateFieldPreparation.Give(
            PrivateFieldPreparation.Equipment(encounter, source, allowBattleChanges: true), preparation), preparation)).ToArray();
        return new(admitted.Package, encounters, source, egress.World, admitted.BattleScenes, admitted.EncounterStarts.Values,
            admitted.EncounterScenes.Values, admitted.BattleStory, BattleStartOrigin.NewGame)
        {
            Persistence = new Sf2OpeningRestore(encounters.SelectMany(encounter => encounter.Deployments)
                .Where(row => row.Faction == BattleFaction.Ally).Max(row => row.Initialization?.AllyPartyMember) switch
                { 10 => 4, 9 => 3, 8 => 2, _ => 1 }),
            DefeatFallback = (new("map-0"), openingMap),
            FieldPreparation = preparation,
        };
    }

}

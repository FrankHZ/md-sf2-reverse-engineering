using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Gameplay.Sf2;

internal static class Sf2TowerRestore
{
    internal const string Recipe = "tower-field";
    private const string Exterior = "ms-map40";
    private const string WoundedLemon = "ms-map40-flag506";

    internal static bool Contains(MapId map) => map.Value is "map-40" or "map-57" or "map-60" or "map-58";

    internal static string? SettledSetup(ExplorationMapDefinition map, MapPosition position, IReadOnlyList<int> flags)
    {
        if (!Contains(map.Map) || map.Traversal.SelectActiveArea(position) is not { } area || flags.Contains(999))
            return null;
        if (map.Map.Value == "map-40")
        {
            return map.SelectedSetup(flags)?.Value switch
            {
                Exterior when map.Population is not null => Exterior,
                WoundedLemon when map.Setups.ContainsKey(new(WoundedLemon)) => WoundedLemon,
                _ => null,
            };
        }

        if (!flags.Contains(506) || map.Setup is not null || map.Population is null)
            return null;
        // The source area records put the two later components after the five linked
        // pre-voyage rooms. F407 is armed on the way down, but intercepts entry at Map58.
        bool admitted = map.Map.Value switch
        {
            "map-57" => !flags.Contains(401) && map.LayoutEvents?.Flags.Any(row => row.Flag == 506) == true,
            "map-60" => area.OneBasedRecordOrdinal <= 5,
            "map-58" => !flags.Contains(407) && flags.Contains(507) && flags.Contains(74) && flags.Contains(640),
            _ => false,
        };
        return admitted ? Sf2FieldSceneRestore.VoidSetup : null;
    }

    internal static SavedFieldScene? Capture(ScenarioDefinition definition, ExplorationState world, IReadOnlyList<int> flags)
    {
        var source = definition.Exploration!.Maps[world.Map];
        var setup = SettledSetup(source, world.PlayerEntity.Position, flags);
        if (setup is null) return null; // The older anchored returns use their original scene-free shape.
        if (!Sf2FieldSceneRestore.MatchesLoadedSetup(source, world.Definition, setup))
            throw new BattleRuleException("save-field-scene-context", "save.fieldScene");
        return Sf2FieldSceneRestore.Capture(definition, world, setup);
    }

    internal static ExplorationState Rebuild(ScenarioDefinition definition, SaveLocation location, BattleStartInput party,
        IReadOnlyList<int> flags, SavedFieldScene? scene, byte layer)
    {
        var source = definition.Exploration!.Maps[location.Anchor.Map];
        if (scene is null || scene.Setup != SettledSetup(source, location.Anchor.Position, flags))
            throw new BattleRuleException("save-field-scene-context", "save.fieldScene");
        var world = Sf2FieldSceneRestore.Rebuild(definition, location, party, flags, scene, out _, layer);
        if (world.Definition.Entities.Any(row => !world.AllEntities.Any(actor => actor.Entity == row.Entity)))
            throw new BattleRuleException("save-field-scene-actor", "save.fieldScene");

        if (world.Map.Value == "map-57")
        {
            var opened = source.Layout;
            foreach (var row in source.LayoutEvents!.Flags.Where(row => flags.Contains(row.Flag)))
                opened = opened.ApplyBlockCopy(row.Copy);
            if (!opened.Words.SequenceEqual(world.Layout.Words))
                throw new BattleRuleException("save-field-scene-layout", "save.fieldScene");
        }
        Sf2FieldSceneRestore.RequireUnoccupiedLanding(world);
        return world;
    }
}

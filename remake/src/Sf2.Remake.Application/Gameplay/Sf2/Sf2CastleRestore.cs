using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Gameplay.Sf2;

internal static class Sf2CastleRestore
{
    internal const string Recipe = "granseal-castle";

    internal static bool Contains(MapId map) => map.Value is "map-19" or "map-20" or "map-21";

    internal static MapSetupId? SettledSetup(ExplorationMapDefinition map, IReadOnlyList<int> flags)
    {
        if (flags.Contains(999) || map.SelectedSetup(flags) is not { } selected) return null;
        string? prefix = map.Map.Value switch
        {
            "map-19" => "ms-map19",
            "map-20" => "ms-map20",
            "map-21" => "ms-map21",
            _ => null,
        };
        if (prefix is null || map.Setup!.DefaultSetup.Value != prefix) return null;
        if (selected.Value == prefix) return map.Population is not null ? selected : null;
        bool admitted = selected.Value == prefix + "-flag501" || selected.Value == prefix + "-flag609" ||
            selected.Value == prefix + "-flag506" || map.Map.Value == "map-19" && selected.Value == "ms-map19-flag507";
        return admitted && map.Setups.ContainsKey(selected) ? selected : null;
    }

    internal static SavedFieldScene Capture(ScenarioDefinition definition, ExplorationState world, IReadOnlyList<int> flags)
    {
        var source = definition.Exploration!.Maps[world.Map];
        var setup = SettledSetup(source, flags);
        if (setup is null || !Sf2FieldSceneRestore.MatchesLoadedSetup(source, world.Definition, setup.Value))
            throw new BattleRuleException("save-field-scene-context", "save.fieldScene");
        return Sf2FieldSceneRestore.Capture(definition, world, setup.Value);
    }

    internal static ExplorationState Rebuild(ScenarioDefinition definition, SaveLocation location, BattleStartInput party,
        IReadOnlyList<int> flags, SavedFieldScene? scene, byte layer)
    {
        var source = definition.Exploration!.Maps[location.Anchor.Map];
        if (scene is null || scene.Setup != SettledSetup(source, flags)?.Value)
            throw new BattleRuleException("save-field-scene-context", "save.fieldScene");

        // The F609 commission arrival allocates and retires its procession in the same visit.
        // F610 skips that arrival on fresh entry. Its extra physical slots belong to the script.
        var procession = scene.Setup == "ms-map19-flag609" && flags.Contains(610) &&
            definition.Exploration.Programs.TryGetValue("cs-53176", out var commission)
            ? commission : null;
        var world = Sf2FieldSceneRestore.Rebuild(definition, location, party, flags, scene, out _, layer, procession);
        if (world.Definition.Entities.Any(row => !world.AllEntities.Any(actor => actor.Entity == row.Entity)))
            throw new BattleRuleException("save-field-scene-actor", "save.fieldScene");
        Sf2FieldSceneRestore.RequireUnoccupiedLanding(world);
        return world;
    }
}

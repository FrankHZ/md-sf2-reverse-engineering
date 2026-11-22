using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Gameplay.Sf2;

// The audience retains Astral's resident slot locally; Janet is allocated only by her scene.
// Neither actor is added to the fresh town population to make a saved scene reconstructible.
internal static class Sf2NewGransealRestore
{
    internal static SavedFieldScene Capture(ScenarioDefinition definition, ExplorationState world)
    {
        var source = definition.Exploration!.Maps[world.Map];
        var selected = source.Setups.FirstOrDefault(pair => pair.Value.Entities.SequenceEqual(world.Definition.Entities));
        string setup = selected.Key?.Value ?? source.Setup?.DefaultSetup.Value ?? "";
        ValidatePopulation(definition, world.Map.Value, setup);
        return Sf2FieldSceneRestore.Capture(definition, world, setup);
    }

    internal static ExplorationState Rebuild(ScenarioDefinition definition, SaveLocation location, BattleStartInput party,
        IReadOnlyList<int> flags, SavedFieldScene scene, out IReadOnlyList<int> enteredFlags, byte layer)
    {
        if (location.Recipe != "new-granseal-return" || !flags.Contains(802))
            throw new BattleRuleException("save-field-scene-context", "save.fieldScene");
        ValidatePopulation(definition, location.Anchor.Map.Value, scene.Setup);
        var allocations = location.Anchor.Map.Value == "map-6" && flags.Contains(806) &&
            definition.Exploration!.Programs.TryGetValue("cs-55288", out var janet)
                ? janet : null;
        return Sf2FieldSceneRestore.Rebuild(definition, location, party, flags, scene, out enteredFlags, layer, allocations);
    }

    private static void ValidatePopulation(ScenarioDefinition definition, string map, string setup)
    {
        var source = definition.Exploration!.Maps[new(map)];
        if (!(map == "map-6" && setup is "ms-map6" or "ms-map6-flag701" or "ms-map6-flag702" ||
              map == "map-7" && setup is "ms-map7" or "ms-map7-flag701" or "ms-map7-flag805") ||
            !(source.Setup?.DefaultSetup.Value == setup || source.Setups.ContainsKey(new(setup))))
            throw new BattleRuleException("save-field-scene-population", "save.fieldScene");
    }
}

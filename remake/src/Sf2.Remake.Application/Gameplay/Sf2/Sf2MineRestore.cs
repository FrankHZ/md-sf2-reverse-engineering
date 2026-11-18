using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Gameplay.Sf2;

// Map30's live F785 cast/2x2 copy is different from fresh F761's empty cast/extra 4x4.
internal static class Sf2MineRestore
{
    internal static SavedFieldScene Capture(ScenarioDefinition definition, ExplorationState world)
    {
        var source = definition.Exploration!.Maps[world.Map];
        var selected = source.Setups.FirstOrDefault(pair => pair.Value.Entities.SequenceEqual(world.Definition.Entities));
        string setup = selected.Key?.Value ?? source.Setup?.DefaultSetup.Value ?? "";
        ValidatePopulation(definition, setup);
        return Sf2FieldSceneRestore.Capture(definition, world, setup);
    }

    internal static ExplorationState Rebuild(ScenarioDefinition definition, SaveLocation location, BattleStartInput party,
        IReadOnlyList<int> flags, SavedFieldScene scene, out IReadOnlyList<int> enteredFlags, byte layer)
    {
        if (location.Anchor.Map.Value != "map-30" || location.Recipe != "dwarven-mine")
            throw new BattleRuleException("save-field-scene-context", "save.fieldScene");
        ValidatePopulation(definition, scene.Setup);
        return Sf2FieldSceneRestore.Rebuild(definition, location, party, flags, scene, out enteredFlags, layer);
    }

    private static void ValidatePopulation(ScenarioDefinition definition, string setup)
    {
        var source = definition.Exploration!.Maps[new("map-30")];
        if (setup is not ("ms-map30" or "ms-map30-flag785" or "ms-map30-flag761") ||
            !(source.Setup?.DefaultSetup.Value == setup || source.Setups.ContainsKey(new(setup))))
            throw new BattleRuleException("save-field-scene-population", "save.fieldScene");
    }
}

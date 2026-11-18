using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Gameplay.Sf2;

internal static class Sf2CreedRestore
{
    internal static SavedFieldScene? Capture(ScenarioDefinition definition, ExplorationState world, IReadOnlyList<int> flags)
    {
        if (world.Map.Value != "map-33" || !flags.Any(flag => flag is 784 or 785 or 786 or 787)) return null;
        var source = definition.Exploration!.Maps[world.Map];
        var selected = source.Setups.FirstOrDefault(pair => pair.Value.Entities.SequenceEqual(world.Definition.Entities));
        string setup = selected.Key?.Value ?? source.Setup?.DefaultSetup.Value ?? "";
        Require(setup is "ms-map33-flag784" or "ms-map33-flag786", "save-field-scene-population");
        return Sf2FieldSceneRestore.Capture(definition, world, setup);
    }

    internal static ExplorationState Rebuild(ScenarioDefinition definition, SaveLocation location, BattleStartInput party,
        IReadOnlyList<int> flags, SavedFieldScene scene, out IReadOnlyList<int> enteredFlags, byte layer)
    {
        Require(location.Anchor.Map.Value == "map-33" && location.Recipe == "creed-mansion" && !flags.Contains(22), "save-field-scene-context");
        Require(scene.Setup is "ms-map33-flag784" or "ms-map33-flag786" &&
            definition.Exploration!.Maps[location.Anchor.Map].Setups.ContainsKey(new(scene.Setup)), "save-field-scene-population");
        return Sf2FieldSceneRestore.Rebuild(definition, location, party, flags, scene, out enteredFlags, layer);
    }

    private static void Require(bool valid, string code)
    { if (!valid) throw new BattleRuleException(code, "save.fieldScene"); }
}

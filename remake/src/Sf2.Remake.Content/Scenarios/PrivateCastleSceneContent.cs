using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateCastleSceneContent
{
    internal static void Bind(IReadOnlyList<ExplorationMapDefinition> maps, List<StoryProgram> programs)
    {
        const string entry = "ms-map20-initfunction";
        const string sourceEntry = "sf2-source-map20-init";
        var map = maps.FirstOrDefault(map => map.Map == new MapId("map-20"));
        if (map?.OnLoad != new ProgramLocation(entry, 0)) return;
        int index = programs.FindIndex(program => program.Id == entry);
        Require(index >= 0 && programs[index].Source ==
            "disasm/data/maps/entries/map20/mapsetups/s6_initfunction.asm:ms_map20_InitFunction" &&
            map.Population is not null && map.Entities.Any(entity => entity.Entity == new EntityRef("entity-130") && entity.Sprite == 209),
            "castle-scene-binding", "world.programs");
        Require(programs.All(program => program.Id != sourceEntry), "castle-scene-program-conflict", "world.programs");
        var original = programs[index];
        programs.Add(new(sourceEntry, original.Instructions, original.Source, original.EntitiesRunning));
        // Controlled initialization can invoke the default entry with another setup's
        // already allocated cast. Never apply default-bedroom cleanup to that cast.
        List<StoryInstruction> instructions = map.Setup?.FlagAlternatives.Select(variant => (StoryInstruction)
            new BranchFlag(int.Parse(variant.Flag.Value, System.Globalization.CultureInfo.InvariantCulture),
                true, new(sourceEntry, 0))).ToList() ?? [];
        // Modern content policy: the wide view can expose the dormant bedroom cast
        // from another entrance. F605 is written only after its first scene returns.
        // Keep the source body intact, including first visit and later setup dispatch.
        instructions.AddRange([
            new BranchFlag(605, false, new(sourceEntry, 0)),
            new HideMapEntity(new("entity-130"), true),
            new JumpProgram(new(sourceEntry, 0)),
        ]);
        programs[index] = new(entry, instructions, entitiesRunning: original.EntitiesRunning);
    }
}

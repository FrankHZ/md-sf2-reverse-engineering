using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class CaravanContentReader
{
    internal static CaravanDefinition Read(JsonElement row, ExplorationEntityDefinition[] entities,
        EntityRef player, ScenarioReadAccepted battle)
    {
        Object(row, "caravan", "followerA", "currentEntities", "overworld", "playerAppearance", "stepCounter");
        Require(entities.Length is >= 1 and <= 31 && entities.All(entity => entity.Entity != player && entity.Sprite is not null),
            "caravan-physical-population", "map.entities", true);
        var follower = new EntityRef(Id(row, "followerA"));
        Require(entities.Any(entity => entity.Entity == follower), "caravan-logical-follower", "caravan.followerA", true);
        var slots = Array(row, "currentEntities").Select(value =>
        {
            Require(value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int slot) && slot >= 1 && slot <= entities.Length,
                "caravan-current-list-slot", "caravan.currentEntities", true);
            return value.GetInt32();
        }).ToArray();
        Require(slots.Length <= 31 && slots.Distinct().Count() == slots.Length,
            "caravan-current-list", "caravan.currentEntities", true);
        var appearance = row.GetProperty("playerAppearance");
        Object(appearance, "caravan.playerAppearance", "actor", "class", "sprite", "defeatedSprite");
        var actor = new ActorRef(Id(appearance, "actor"));
        string className = Text(appearance, "class");
        Require(Enum.GetNames<BattleClassRule>().Contains(className), "caravan-player-class", "caravan.playerAppearance.class", true);
        var classRule = Enum.Parse<BattleClassRule>(className);
        var identity = battle.Definition.Encounters.Values.SelectMany(encounter => encounter.Deployments).Where(row => row.Actor == actor).ToArray();
        Require(battle.Start.Actors.Any(row => row.Actor == actor) && identity.Length > 0 &&
            identity.All(row => row.Definition.ClassRule == classRule), "caravan-player-identity", "caravan.playerAppearance", true);
        return new(follower, System.Array.AsReadOnly(slots), Boolean(row, "overworld"),
            new(actor, classRule, Number(appearance, "sprite", 0, 255), Number(appearance, "defeatedSprite", 0, 255)),
            (short)Number(row, "stepCounter", short.MinValue, short.MaxValue));
    }

    internal static void Bind(IReadOnlyList<ExplorationMapDefinition> maps, List<StoryProgram> programs,
        EntityRef player, bool authored)
    {
        var selected = maps.Where(map => map.Caravan is not null).ToArray();
        if (selected.Length == 0) return;
        Require(authored && maps.Count == 1 && selected.Length == 1, "caravan-standalone-world", "world.maps", true);
        var map = selected[0];
        Require(map.Population is null && map.Setup is null && map.Setups.Count == 0 && map.Battles.Count == 0 &&
            map.LayoutEvents is null && map.Traversal.ActiveAreas.Count == 1,
            "caravan-world-capability", "map", true);
        var area = map.Traversal.ActiveAreas[0];
        Require(area.MaximumX - area.MinimumX >= 2 && area.MaximumY - area.MinimumY >= 2,
            "caravan-walking-bounds", "map.areas");
        // Unsupported copy/door/warp machinery is rejected before it can masquerade as walking.
        Require(map.Layout.Words.All(word => (word & 0x3C00) is not (0x400 or 0x800 or 0xC00 or 0x1000)),
            "caravan-unsupported-marker", "map.layout", true);
        Require(map.Events.All(row => row.Kind != ExplorationEventKind.Warp), "caravan-map-replacement", "map.events", true);
        bool Reserved(ProgramLocation? target) => target?.Program is CaravanPrograms.Boarding or CaravanPrograms.Landing;
        Require(!Reserved(map.OnLoad) && !Reserved(map.InputProgram) && map.Events.All(row => !Reserved(row.Program)),
            "caravan-reserved-program", "map.program", true);
        foreach (var program in programs)
        {
            Require(program.Id is not (CaravanPrograms.Boarding or CaravanPrograms.Landing), "caravan-reserved-program", "program.id", true);
            foreach (var instruction in program.Instructions)
                Require(instruction switch
                {
                    JumpProgram jump => !Reserved(jump.Target), CallProgram call => !Reserved(call.Target),
                    BranchFlag branch => !Reserved(branch.Target), BranchEntityCoordinates branch => !Reserved(branch.Target),
                    SourceRandomBattle or TransferToMap or QueueMapWarp or LoadSceneMap or LoadSceneEntities or ReloadSceneMap => false,
                    AddMapEntity or HideMapEntity => false,
                    _ => true,
                }, "caravan-program-capability", "program.instructions", true);
        }
        programs.AddRange(CaravanPrograms.Create(map.Caravan!, player));
    }
}

using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// Only the admitted pre-voyage chapter owns these modern disclosure rules.
internal static class PrivateFieldGuidance
{
    internal static IReadOnlyList<FieldGuidanceRule> Bind(ExplorationDefinition world)
    {
        if (world.Provenance is null || !world.Maps.TryGetValue(new("map-5"), out var town) ||
            !world.Maps.TryGetValue(new("map-66"), out var field) || !world.Maps.ContainsKey(new("map-43"))) return [];
        if (town.Setup?.DefaultSetup.Value != "ms-map5" || field.Setup?.DefaultSetup.Value != "ms-map66") return [];
        var exit = new FieldGuidanceAnchor(new(7, 0), new(7, 1), new("map-66"), new(18, 21), "guidance.north-exit");
        var house = new FieldGuidanceAnchor(new(6, 5), new(6, 6), new("map-43"), new(6, 11), "guidance.house-entrance");
        Validate(town, exit); Validate(field, house);
        Require(new[] { town, field }.All(map => map.Population?.Followers.Any(f => f.Flag == 72 && f.Character == 4) == true),
            "guidance-follower-binding", "world.maps.population");
        return System.Array.AsReadOnly(new[] {
            Rule(town, [72], [4, 612], "guidance.next-north", exit),
            Rule(field, [72], [4, 612, 503], "guidance.route-unavailable"),
            Rule(field, [72, 503], [4, 612], "guidance.known-house", house),
        });
    }

    private static FieldGuidanceRule Rule(ExplorationMapDefinition map, int[] required, int[] forbidden,
        string route, FieldGuidanceAnchor? anchor = null) => new(map.Map, map.Setup!.DefaultSetup,
            System.Array.AsReadOnly(required), System.Array.AsReadOnly(forbidden), new("entity-4"),
            "guidance.visit-hawel", 4, "guidance.hawel-outside-yeel", route, anchor);

    private static void Validate(ExplorationMapDefinition map, FieldGuidanceAnchor anchor)
    {
        Require(map.Events.Count(e => e.Kind == ExplorationEventKind.Warp &&
            (e.X is null || e.X == anchor.Position.X) && (e.Y is null || e.Y == anchor.Position.Y) &&
            e.DestinationMap == anchor.DestinationMap && e.Destination == anchor.Destination && e.Unavailable is null) == 1,
            "guidance-warp-binding", "world.maps.events");
        Require(map.Traversal.SelectActiveArea(anchor.Approach)?.Area.Contains(anchor.Position) == true &&
            (map.Layout[anchor.Approach.X, anchor.Approach.Y] & 0xC000) != 0xC000,
            "guidance-approach-binding", "world.maps.layout");
    }
}

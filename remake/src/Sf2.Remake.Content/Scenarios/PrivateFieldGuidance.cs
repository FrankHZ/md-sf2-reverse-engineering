using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// Finite modern copy/disclosure for the admitted opening through departure. Not quest state.
internal static class PrivateFieldGuidance
{
    private static readonly int[] TravelMaps = [3, 19, 20, 21, 40, 57, 66, 5, 43, 17, 16, 18, 60, 58];
    private static readonly int[] AfterOpening = [501, 502, 612, 504, 505, 506, 507];
    private static readonly int[] AfterTower = [502, 612, 504, 505, 506, 507];
    private static readonly int[] AfterCommission = [612, 504, 505, 506, 507];

    internal static IReadOnlyList<FieldGuidanceRule> Bind(ExplorationDefinition world)
    {
        if (world.Provenance is null || !world.Maps.TryGetValue(new("map-3"), out var home) ||
            !world.Maps.TryGetValue(new("map-5"), out var town) || !world.Maps.TryGetValue(new("map-66"), out var field)) return [];
        if (home.Setup?.DefaultSetup.Value != "ms-map3" || town.Setup?.DefaultSetup.Value != "ms-map5" ||
            field.Setup?.DefaultSetup.Value != "ms-map66") return [];
        var rules = new List<FieldGuidanceRule>();

        void Add(int map, int[] required, int[] forbidden, string goal, string destination, string next,
            OriginalMapTraversalArea? area = null, int? companion = null, FieldGuidanceAnchor? anchor = null)
        {
            if (!world.Maps.TryGetValue(new($"map-{map}"), out var content)) return;
            var setups = content.Setup is null ? new MapSetupId?[] { null } :
                new MapSetupId?[] { content.Setup.DefaultSetup }.Concat(content.Setups.Keys.Select(id => (MapSetupId?)id));
            var needs = System.Array.AsReadOnly(required);
            var excludes = System.Array.AsReadOnly(forbidden.Concat([700, 999]).Distinct().ToArray());
            foreach (var setup in setups)
                rules.Add(new(content.Map, setup, needs, excludes, null, "guidance." + goal, companion,
                    "guidance." + destination, "guidance." + next, anchor, area));
        }
        void Travel(int[] required, int[] forbidden, string goal, string destination, Func<int, string> next)
        { foreach (int map in TravelMaps) Add(map, required, forbidden, goal, destination, next(map)); }

        // No school, castle illness or messenger knowledge is inferred from the full text catalogue.
        int[] opening = [603, .. AfterOpening];
        Add(3, [], [601, 602, .. opening], "start-day", "home", "leave-home", new(51, 0, 61, 9));
        Add(3, [], [601, 602, .. opening], "start-day", "home", "mother", new(0, 0, 50, 31));
        Add(3, [], [601, 602, .. opening], "attend-school", "school", "teacher", new(51, 10, 61, 19));
        Add(3, [601], [602, .. opening], "attend-school", "school", "school-courtyard", new(0, 0, 50, 31));
        Add(3, [601], [602, .. opening], "attend-school", "school", "leave-home", new(51, 0, 61, 9));
        Add(3, [601], [602, .. opening], "attend-school", "school", "teacher", new(51, 10, 61, 19));
        Add(3, [602], opening, "join-lesson", "school", "return-class");

        Travel([603], [605, 608, 401, .. AfterOpening], "visit-castle", "granseal-castle", map => map switch
        { 3 => "castle-north", 19 or 20 => "royal-quarters", _ => "return-castle" });
        Travel([603, 605], [608, 401, .. AfterOpening], "join-astral", "castle-hall", map => map switch
        { 19 => "astral-hall", 20 or 21 or 40 or 57 => "return-hall", _ => "return-castle" });
        Travel([603, 605, 608], [401, .. AfterOpening], "follow-astral", "ancient-tower", map => map switch
        { 19 => "west-stairs", 20 => "west-tower-stairs", 21 => "tower-guard", 40 => "tower-passage", _ => "return-castle" });
        Travel([603, 605, 608, 401], AfterOpening, "enter-tower", "ancient-tower", TowerLeg);

        Travel([501], [609, 610, .. AfterTower], "return-king", "royal-quarters", map => map switch
        { 19 or 20 => "king-room", 3 or 66 or 5 => "return-castle", _ => "reverse-tower" });
        Travel([501, 609], [610, .. AfterTower], "hear-commission", "castle-hall", map => map switch
        { 19 => "royal-party", 20 or 21 or 40 or 57 => "return-hall", _ => "return-castle" });
        Travel([610], [502, .. AfterCommission], "fetch-hawel", "yeel-area", map => map switch
        { 3 => "west-town-exit", 66 => "toward-yeel", 5 => "ask-hawel", _ => "leave-castle" });

        Travel([502, 610], [72, 4, .. AfterCommission], "find-hawel", "yeel-area", map => map switch
        { 5 => "ask-hawel", 66 => "visit-yeel", 3 => "west-town-exit", _ => "leave-castle" });
        // F72 is the real completed guide caller. No extra live-follower gate after a return/load.
        var north = new FieldGuidanceAnchor(new(7, 0), new(7, 1), new("map-66"), new(18, 21), "guidance.north-exit");
        var house = new FieldGuidanceAnchor(new(6, 5), new(6, 6), new("map-43"), new(6, 11), "guidance.house-entrance");
        Validate(town, north);
        if (world.Maps.ContainsKey(house.DestinationMap)) Validate(field, house);
        foreach (int map in TravelMaps.Where(id => id != 66))
            Add(map, [72], [4, .. AfterCommission], "visit-hawel", "hawel-outside-yeel",
                map == 5 ? "next-north" : map == 3 ? "west-town-exit" : "return-yeel", companion: 4, anchor: map == 5 ? north : null);
        Add(66, [72], [4, 503, .. AfterCommission], "visit-hawel", "hawel-outside-yeel", "route-unavailable", companion: 4);
        Add(66, [72, 503], [4, .. AfterCommission], "visit-hawel", "hawel-outside-yeel", "known-house", companion: 4,
            anchor: world.Maps.ContainsKey(house.DestinationMap) ? house : null);

        // Last house line834 names the Ancient Shrine, not Galam or the coming ambush.
        Travel([4, 612], [504, 505, 506, 507], "ancient-shrine-goal", "ancient-shrine", map => map switch
        { 43 => "leave-house", 66 => "south-house", 3 => "west-town-exit", _ => "return-countryside" });

        int[] captivityEnd = [505, 506, 507];
        foreach (int map in TravelMaps.Where(id => id != 17))
            Add(map, [504], captivityEnd, "escape-galam", "galam-exit", map == 16 ? "west-castle-exit" : "return-galam-exit");
        Add(17, [504], [661, 662, 73, .. captivityEnd], "leave-cells", "prison-cells", "other-prisoners");
        Add(17, [504, 661], [662, 73, .. captivityEnd], "escape-galam", "prison-stairs", "approach-stairs");
        Add(17, [504, 662], [73, .. captivityEnd], "escape-galam", "opened-shortcut", "use-shortcut");
        Add(17, [504, 73], captivityEnd, "escape-galam", "castle-kitchen", "tunnel-kitchen", new(0, 14, 31, 30));
        Add(17, [504, 73], captivityEnd, "escape-galam", "galam-exit", "leave-kitchen", new(26, 0, 37, 12));
        Add(17, [504, 73], captivityEnd, "escape-galam", "opened-shortcut", "use-shortcut", new(38, 0, 62, 12));
        Add(17, [504, 73], captivityEnd, "escape-galam", "galam-exit", "castle-interior-exit", new(0, 0, 25, 10));
        Add(17, [504, 73], captivityEnd, "escape-galam", "galam-exit", "castle-interior-exit", new(38, 26, 55, 35));

        Travel([505], [506, 457, 507], "return-granseal", "granseal", GalamReturnLeg);
        Travel([506], [620, 621, 457, 507], "check-castle", "granseal-castle", map => map switch
        { 3 => "castle-north", 19 => "castle-people", 20 or 21 or 40 or 57 or 60 or 58 => "return-hall", _ => GalamReturnLeg(map) });
        // Astral's scene and the reached tower intro also disclose the destination. Royal following
        // is not a new mandatory gate; a paid return must not send that party back to old questions.
        Travel([620], [621, 457, 507], "rescue-at-tower", "ancient-tower", map => map == 19 ? "royal-party" : RescueLeg(map));
        Travel([621], [457, 507], "rescue-at-tower", "ancient-tower", RescueLeg);
        Travel([457], [507], "rescue-at-tower", "ancient-tower", RescueLeg);

        // F74/640 describe the reached first-victory aftermath. Boarding clears F74; all rules end.
        foreach (int map in TravelMaps)
        {
            if (map == 19)
            {
                Add(map, [507, 74, 640], [622], "return-from-tower", "castle-exit", "cross-castle");
                Add(map, [507, 74, 640, 622], [], "escape-earthquake", "granseal", "castle-south");
            }
            else Add(map, [507, 74, 640], [], "return-from-tower", "granseal-castle", "reverse-tower");
        }
        var docks = new FieldGuidanceAnchor(new(50, 24), new(49, 24), new("map-44"), new(1, 25), "guidance.east-exit");
        if (world.Maps.TryGetValue(new("map-4"), out var quake) && world.Maps.ContainsKey(docks.DestinationMap)) Validate(quake, docks);
        Add(4, [507, 74, 640, 624], [], "reach-docks", "granseal-docks", "east-safe-exit", anchor: docks);
        Add(44, [507, 74, 640, 624], [], "board-ship", "granseal-docks", "dock-man");
        return rules.AsReadOnly();
    }

    private static string TowerLeg(int map) => map switch
    { 3 => "castle-north", 19 => "west-stairs", 20 => "west-tower-stairs", 21 => "north-tower-exit", 40 => "tower-passage", 57 => "tower-inner", _ => "return-castle" };
    private static string GalamReturnLeg(int map) => map switch
    { 16 => "south-galam-gate", 17 => "castle-interior-exit", 18 => "south-drawbridge", 66 => "toward-granseal", 3 => "castle-north", _ => "return-granseal-route" };
    private static string RescueLeg(int map) => map switch
    { 60 => "tower-portals", 58 => "tower-deep", 16 or 17 or 18 or 66 => GalamReturnLeg(map), _ => TowerLeg(map) };

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

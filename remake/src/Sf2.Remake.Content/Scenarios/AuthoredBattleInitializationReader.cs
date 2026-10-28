using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// Finite authored source initialization and ordered region programs; no private battle import.
internal static class AuthoredBattleInitializationReader
{
    internal static BattleDeploymentInitialization? Placement(JsonElement row)
    {
        if (!row.TryGetProperty("initialization", out var input)) return null;
        ObjectOptional(input, "placement.initialization", "allyPartyMember", "baseAiWord", "spawnMode", "primaryRegion", "secondaryRegion", "filler", "commandset", "primaryOrder", "secondaryOrder");
        ushort? word = input.GetProperty("baseAiWord").ValueKind == JsonValueKind.Null ? null : (ushort)Number(input, "baseAiWord", 0, 65535);
        return new(word, (byte)Number(input, "spawnMode", 0, 3), (byte)Number(input, "primaryRegion", 0, 15),
            (byte)Number(input, "secondaryRegion", 0, 15), (byte)Number(input, "filler", 0, 255),
            (byte)Number(input, "commandset", 0, 15), (byte)Number(input, "primaryOrder", 0, 255), (byte)Number(input, "secondaryOrder", 0, 255),
            AllyPartyMember: input.TryGetProperty("allyPartyMember", out _) ? Number(input, "allyPartyMember", 0, 29) : null);
    }

    internal static BattleInitializationDefinition? Read(JsonElement encounter, IReadOnlyList<BattleDeploymentDefinition> deployments)
    {
        if (!encounter.TryGetProperty("initialization", out var input))
        {
            Require(deployments.All(d => d.Initialization is null), "initialization-policy", "encounter.initialization");
            return null;
        }
        ObjectOptional(input, "initialization", "regionPrograms", "regions", "regionProgram", "enemyCount", "sourceCombatants");
        string programMode = Text(input, "regionProgram");
        Require(programMode is "none" or "authored", "region-program", "encounter.initialization.regionProgram", true);
        var programs = input.TryGetProperty("regionPrograms", out _) ? Array(input, "regionPrograms").Select(row =>
        {
            Object(row, "regionProgram", "region", "playedFlag", "program");
            return new BattleRegionProgramRow((byte)Number(row, "region", 0, 15), Number(row, "playedFlag", 0, 65535), Id(row, "program"));
        }).ToArray() : [];
        Require(programMode == "authored" ? programs.Length is > 0 and <= 16 : programs.Length == 0,
            "region-program-rows", "initialization.regionPrograms");
        List<BattleActivationRegion> regions = [];
        foreach (var region in Array(input, "regions"))
        {
            Object(region, "region", "id", "vertices");
            var points = Array(region, "vertices").Select(point =>
            {
                Object(point, "point", "x", "y");
                return new MapPosition(Number(point, "x", 0, 47), Number(point, "y", 0, 47));
            });
            regions.Add(new((byte)Number(region, "id", 0, 15), points));
        }
        var slots = new Dictionary<int, ActorRef>();
        foreach (var row in Array(input, "sourceCombatants"))
        {
            Object(row, "combatant", "slot", "actor");
            Require(slots.TryAdd(Number(row, "slot", 0, 159), new(Id(row, "actor"))), "duplicate-source-slot", "initialization.sourceCombatants");
        }
        var members = deployments.Where(d => d.Initialization?.AllyPartyMember is not null).ToArray();
        Require(members.All(d => d.Faction == BattleFaction.Ally &&
            slots.TryGetValue(d.Initialization!.AllyPartyMember!.Value, out var actor) && actor == d.Actor),
            "party-member-binding", "placements.initialization.allyPartyMember");
        Require(members.Select(d => d.Initialization!.AllyPartyMember).Distinct().Count() == members.Length,
            "duplicate-party-member", "placements.initialization.allyPartyMember");
        Require(programs.All(p => regions.Any(r => r.Id == p.Region)), "missing-activation-region", "initialization.regionPrograms");
        return new(regions, programMode == "authored" ? BattleRegionProgram.Authored : BattleRegionProgram.None,
            sourceCombatants: slots, enemyCount: Number(input, "enemyCount", 1, 32), regionPrograms: programs);
    }

    internal static (bool? Initialized, ushort? Word) Start(JsonElement input)
    {
        if (!input.TryGetProperty("initialization", out var value)) return (null, null);
        Object(value, "start.initialization", "initialized", "activationWord");
        return (Boolean(value, "initialized"), (ushort)Number(value, "activationWord", 0, 65535));
    }
}

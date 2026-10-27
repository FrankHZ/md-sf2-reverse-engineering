using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class TarosEffectivenessReader
{
    internal static IReadOnlyList<byte> Weapons(JsonElement encounter)
    {
        if (!encounter.TryGetProperty("tarosEffectiveness", out var binding)) return [];
        Object(binding, "encounter.tarosEffectiveness", "bowie", "enemyDefinitions", "weaponItems");
        var weapons = Array(binding, "weaponItems").Select(value =>
        {
            Require(value.ValueKind == JsonValueKind.Number && value.TryGetByte(out byte id) && id < 127,
                "weapon-identity", "tarosEffectiveness.weaponItems");
            return value.GetByte();
        }).ToArray();
        Require(weapons.Distinct().Count() == weapons.Length && weapons.Contains((byte)61),
            "taros-weapons", "tarosEffectiveness.weaponItems");
        return weapons;
    }

    internal static TarosEffectiveness? Read(JsonElement encounter, IReadOnlyList<BattleDeploymentDefinition> deployments,
        IReadOnlyCollection<byte> admittedWeapons)
    {
        if (!encounter.TryGetProperty("tarosEffectiveness", out var binding)) return null;
        var weapons = Weapons(encounter);
        // All carried weapon identities need a type at the source-policy seam,
        // including weapons admitted by another encounter in the same package.
        Require(admittedWeapons.All(weapons.Contains), "taros-weapon-types", "tarosEffectiveness.weaponItems");
        var bowie = new ActorRef(Id(binding, "bowie"));
        Require(deployments.Any(row => row.Actor == bowie && row.Faction == BattleFaction.Ally),
            "taros-bowie", "tarosEffectiveness.bowie");
        var enemies = new Dictionary<ActorRef, byte>();
        foreach (var row in Array(binding, "enemyDefinitions"))
        {
            Object(row, "enemy-definition", "actor", "definition");
            var actor = new ActorRef(Id(row, "actor"));
            Require(deployments.Any(deployment => deployment.Actor == actor && deployment.Faction == BattleFaction.Enemy),
                "taros-enemy", "tarosEffectiveness.enemyDefinitions.actor");
            Require(enemies.TryAdd(actor, (byte)Number(row, "definition", 0, 255)),
                "duplicate-enemy-definition", "tarosEffectiveness.enemyDefinitions.actor");
        }
        Require(enemies.Values.Contains((byte)88), "taros-definition", "tarosEffectiveness.enemyDefinitions");
        return new(bowie, enemies, weapons);
    }
}

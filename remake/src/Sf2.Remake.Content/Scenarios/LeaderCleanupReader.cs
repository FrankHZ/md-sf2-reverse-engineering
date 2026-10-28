using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class LeaderCleanupReader
{
    internal static LeaderCleanupDefinition? Read(JsonElement encounter,
        IReadOnlyList<BattleDeploymentDefinition> deployments, bool allow)
    {
        if (!encounter.TryGetProperty("leaderCleanup", out var binding)) return null;
        Require(allow, "leader-cleanup-world", "encounter.leaderCleanup", true);
        Object(binding, "leaderCleanup", "bowie", "enemySlots");
        var bowie = new ActorRef(Id(binding, "bowie"));
        Require(deployments.Any(row => row.Actor == bowie && row.Faction == BattleFaction.Ally &&
            row.Definition.Physical?.Leader == true), "leader-cleanup-bowie", "leaderCleanup.bowie");
        var slots = new Dictionary<ActorRef, byte>();
        foreach (var row in Array(binding, "enemySlots"))
        {
            Object(row, "enemy-slot", "actor", "slot");
            var actor = new ActorRef(Id(row, "actor"));
            byte slot = (byte)Number(row, "slot", 128, 159);
            Require(deployments.Any(value => value.Actor == actor && value.Faction == BattleFaction.Enemy),
                "leader-cleanup-enemy", "leaderCleanup.enemySlots.actor");
            Require(!slots.ContainsValue(slot) && slots.TryAdd(actor, slot),
                "duplicate-source-slot", "leaderCleanup.enemySlots");
        }
        Require(slots.ContainsValue(128) && deployments.Where(row => row.Faction == BattleFaction.Enemy)
            .All(row => slots.ContainsKey(row.Actor)), "leader-cleanup-slots", "leaderCleanup.enemySlots");
        return new(bowie, slots);
    }
}

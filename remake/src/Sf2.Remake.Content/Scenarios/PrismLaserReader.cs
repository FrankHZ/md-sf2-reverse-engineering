using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrismLaserReader
{
    internal static PrismLaserDefinition? Read(JsonElement encounter, IReadOnlyList<BattleDeploymentDefinition> placements)
    {
        if (!encounter.TryGetProperty("prismLaser", out var input)) return null;
        Object(input, "prismLaser", "primaryRegion", "secondaryRegion", "combatants");
        Require(Number(input, "primaryRegion", 0, 15) == 15 && Number(input, "secondaryRegion", 0, 15) == 15,
            "prism-activation-regions", "prismLaser", true);
        var bindings = new Dictionary<ActorRef, PrismCombatantBinding>();
        foreach (var row in Array(input, "combatants"))
        {
            Object(row, "prism-combatant", "actor", "slot", "kind", "direction");
            var actor = new ActorRef(Id(row, "actor"));
            var placement = placements.SingleOrDefault(p => p.Actor == actor);
            Require(placement is not null, "prism-actor", "prismLaser.combatants.actor");
            byte slot = (byte)Number(row, "slot", 0, 159);
            Require(placement!.Faction == BattleFaction.Ally ? slot < 30 : slot >= 128,
                "prism-slot-faction", "prismLaser.combatants.slot");
            string kind = Text(row, "kind");
            Require(kind is "ordinary" or "prism-flower", "prism-kind", "prismLaser.combatants.kind", true);
            byte? direction = kind == "prism-flower" ? (byte)Number(row, "direction", 0, 3) : null;
            Require(kind == "prism-flower" || row.GetProperty("direction").ValueKind == JsonValueKind.Null,
                "prism-direction", "prismLaser.combatants.direction");
            Require(direction is null ? placement.AiStrategy?.Value != "prism-laser" :
                placement.Faction == BattleFaction.Enemy && placement.AiStrategy?.Value == "prism-laser" &&
                placement.Position is { X: > 0 and < 47, Y: > 0 and < 47 }, "prism-flower", "prismLaser.combatants");
            Require(placement.Definition.MaxHp < 32768, "prism-hp", "actors.maxHp", true);
            Require(!bindings.Values.Any(b => b.Slot == slot) && bindings.TryAdd(actor, new(slot, direction)),
                "duplicate-prism-slot", "prismLaser.combatants");
        }
        Require(bindings.Count == placements.Count && bindings.Values.Any(b => b.Direction is not null) &&
            !encounter.TryGetProperty("tarosEffectiveness", out _), "prism-bindings", "prismLaser", true);
        return new(bindings);
    }
}

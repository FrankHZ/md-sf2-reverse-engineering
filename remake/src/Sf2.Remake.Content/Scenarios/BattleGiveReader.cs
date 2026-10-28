using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class BattleGiveReader
{
    internal static BattleGiveDefinition? Read(JsonElement root, bool connected,
        IEnumerable<BattleDeploymentDefinition> placements, IReadOnlyList<BattleWeaponDefinition> weapons,
        IEnumerable<HealingItemDefinition> healing, IEnumerable<RetreatItemDefinition> retreat)
    {
        if (!root.TryGetProperty("battleGive", out _)) return null;
        Require(!connected && Boolean(root, "battleGive"), "give-standalone-opt-in", "battleGive", true);
        var names = healing.Select(item => KeyValuePair.Create(item.ItemId, item.Name))
            .Concat(retreat.Select(item => KeyValuePair.Create(item.ItemId, item.Name)))
            .Concat(weapons.Select(item => KeyValuePair.Create(item.ItemId, item.Name))).ToDictionary();
        foreach (var deployment in placements.Where(p => p.Faction == BattleFaction.Ally))
        {
            var actor = deployment.Definition;
            // Every receiver can carry every declared weapon, but class masks still govern Equip.
            Require(weapons.Count == 0 || actor.Equipment is not null, "give-live-equipment-required", "actor.equipment", true);
            Require(actor.SourceLoadout is { Items.Count: 4 } && actor.SourceLoadout.Items.All(word =>
                word == 127 || word <= 255 && names.ContainsKey((byte)(word & 127)) &&
                ((word & 128) == 0 || actor.Equipment?.Weapons.ContainsKey((byte)(word & 127)) == true)),
                "give-item-unsupported", "actor.items", true);
        }
        return new(names);
    }
}

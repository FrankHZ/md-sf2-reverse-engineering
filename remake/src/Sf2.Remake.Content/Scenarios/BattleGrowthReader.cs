using System.Collections.ObjectModel;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class BattleGrowthReader
{
    internal static IEnumerable<BattleDefinition> Bind(JsonElement rows, ScenarioDefinition scenario)
    {
        Require(scenario.PrivateDefinitions is not null, "growth-source", "world.growth", true);
        var definitions = scenario.PrivateDefinitions!;
        var growth = new Dictionary<ActorRef, BattleGrowthDefinition>();
        foreach (var row in rows.EnumerateArray())
        {
            Object(row, "growth", "actor", "classId", "stats", "spells");
            var reference = new ActorRef(Id(row, "actor"));
            var actor = scenario.Encounters.Values.SelectMany(encounter => encounter.Deployments)
                .Select(deployment => deployment.Definition).FirstOrDefault(actor => actor.Actor == reference);
            Require(actor is not null, "growth-actor", "world.growth.actor");
            byte classId = (byte)Number(row, "classId", 0, 31);
            Require(actor!.SourceClassId == classId, "growth-class", "world.growth.classId", true);
            Require(actor.SourceLoadout is not null, "growth-loadout", "world.growth.actor", true);
            int bonus = actor.SourceLoadout!.Items.Where(word => (word & 128) != 0)
                .SelectMany(word => definitions.Items[(byte)(word & 127)].EquipEffects)
                .Where(effect => effect.Type == "INCREASE_ATT").Sum(effect => effect.Parameter);
            Require(bonus >= 0 && bonus <= actor.Attack, "growth-base-attack", "world.growth.actor", true);
            var stats = Array(row, "stats").Select(stat =>
            {
                Object(stat, "stat-growth", "start", "projected", "curve");
                byte initial = (byte)Number(stat, "start", 0, 200);
                byte projected = (byte)Number(stat, "projected", initial, 200);
                var curve = Array(stat, "curve").Select(fraction =>
                {
                    Require(fraction.ValueKind == JsonValueKind.Array && fraction.GetArrayLength() == 2,
                        "growth-fraction", "world.growth.stats.curve");
                    Require(fraction[0].TryGetUInt16(out ushort cumulative) && cumulative <= 256 &&
                        fraction[1].TryGetUInt16(out ushort increment) && increment <= 256,
                        "growth-fraction", "world.growth.stats.curve");
                    return new StatGrowthFraction(fraction[0].GetUInt16(), fraction[1].GetUInt16());
                }).ToArray();
                Require(curve.Length is 0 or 29, "growth-curve-length", "world.growth.stats.curve");
                int previous = 0;
                foreach (var fraction in curve)
                {
                    Require(fraction.Cumulative - previous == fraction.Increment, "growth-curve-order", "world.growth.stats.curve");
                    previous = fraction.Cumulative;
                }
                Require(curve.Length == 0 || previous == 256, "growth-curve-projection", "world.growth.stats.curve");
                return new StatGrowth(initial, projected, System.Array.AsReadOnly(curve));
            }).ToArray();
            Require(stats.Length == 5, "growth-stats", "world.growth.stats");
            var spells = new List<LevelSpell>();
            var spellDefinitions = new Dictionary<byte, SpellRef>();
            foreach (var spell in Array(row, "spells"))
            {
                Object(spell, "growth-spell", "level", "packed", "spell");
                byte packed = (byte)Number(spell, "packed", 0, 254);
                Require((packed & 63) < 44, "growth-spell", "world.growth.spells");
                spells.Add(new((byte)Number(spell, "level", 1, 119), packed));
                for (byte rank = 0; rank <= packed >> 6; rank++)
                    spellDefinitions[(byte)((packed & 63) | rank << 6)] = new(Id(spell, "spell"), (byte)(rank + 1));
            }
            Require(growth.TryAdd(reference, new(classId, (byte)bonus, System.Array.AsReadOnly(stats), spells.AsReadOnly(),
                new ReadOnlyDictionary<byte, SpellRef>(spellDefinitions))), "duplicate-growth", "world.growth.actor");
        }
        return scenario.Encounters.Values.Select(encounter => new BattleDefinition(encounter.Encounter, encounter.Map,
            encounter.Width, encounter.Height, encounter.Terrain, encounter.Deployments.Select(deployment =>
                growth.TryGetValue(deployment.Actor, out var value)
                    ? deployment with { Definition = deployment.Definition.WithGrowth(value) } : deployment),
            encounter.Spells.Values, encounter.Rewards, encounter.Initialization, encounter.Outcome));
    }
}

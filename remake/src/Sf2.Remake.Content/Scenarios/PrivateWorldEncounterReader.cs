using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// Additional private encounters are admitted at startup, never read during a map transition.
internal static class PrivateWorldEncounterReader
{
    internal static ScenarioReadAccepted Read(JsonElement selections, string worldPath, ScenarioReadAccepted first)
    {
        Require(selections.ValueKind == JsonValueKind.Array, "array-required", "encounters");
        Require(selections.GetArrayLength() <= 1, "encounter-selection-limit", "encounters");
        var admitted = new List<ScenarioReadAccepted> { first };
        string directory = Path.GetDirectoryName(Path.GetFullPath(worldPath))!;
        foreach (var row in selections.EnumerateArray())
        {
            Object(row, "encounter-inputs", "placement", "scene", "terrain", "static", "enemies", "gold", "start", "battleScenes");
            string Input(string key)
            {
                string relative = Text(row, key);
                Require(!Path.IsPathRooted(relative), "relative-input-required", "encounters." + key);
                string resolved = Path.GetFullPath(Path.Combine(directory, relative));
                Require(resolved.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase),
                    "input-path-escape", "encounters." + key);
                return resolved;
            }
            var source = new PrivateBattleScenarioReader(Input("placement"), Input("scene"), Input("terrain"),
                Input("static"), Input("enemies"), Input("gold"), Input("start"),
                row.GetProperty("battleScenes").ValueKind == JsonValueKind.Null ? null : Input("battleScenes"));
            var result = source.Read();
            if (result is ScenarioReadRejected rejected) throw new AdmissionIssue(rejected.Failure);
            admitted.Add((ScenarioReadAccepted)result);
        }
        var encounters = admitted.SelectMany(read => read.Definition.Encounters.Values).ToArray();
        Require(encounters.Select(encounter => encounter.Encounter).Distinct().Count() == encounters.Length,
            "duplicate-encounter", "encounters");
        var definitions = admitted.Select(read => read.Definition.PrivateDefinitions!).ToArray();
        // Each reader authenticates the same pinned core/enemy/gold export bytes before
        // selecting records. Shared keys therefore have the same immutable source facts.
        var merged = new PrivateBattleDefinitions(definitions[0].Encounter,
            definitions.SelectMany(value => value.Sources).Distinct(),
            definitions.SelectMany(value => value.Classes.Values).DistinctBy(value => value.Id),
            definitions.SelectMany(value => value.Items.Values).DistinctBy(value => value.Id),
            definitions.SelectMany(value => value.Spells.Values).DistinctBy(value => value.Spell),
            definitions.SelectMany(value => value.Enemies.Values).DistinctBy(value => value.Code),
            definitions.SelectMany(value => value.EnemyGold.Values).DistinctBy(value => value.EnemyId));
        // Shared allies must have one immutable baseline. Progress and inventory changes belong to live state.
        foreach (var group in encounters.SelectMany(encounter => encounter.Deployments)
            .Where(row => row.Faction == BattleFaction.Ally).GroupBy(row => row.Actor))
        {
            var baseline = group.First().Definition;
            Require(group.All(row => SameAlly(baseline, row.Definition)), "party-definition-mismatch", "encounters.allies");
        }
        var party = admitted.SelectMany(read => read.Start.Actors.Where(input => read.Definition.Encounters[read.Start.Encounter]
            .Deployments.Single(row => row.Actor == input.Actor).Faction == BattleFaction.Ally)).DistinctBy(input => input.Actor);
        var start = new BattleStartInput(first.Start.Encounter, party, first.Start.MainSeed, first.Start.ThinkingSeed,
            first.Start.Gold, first.Start.NewBattle, first.Start.ActiveAllies);
        return new(new(first.Definition.Package, encounters, merged, battleScenes: first.Definition.BattleScenes,
            encounterStarts: admitted.Select(read => read.Start),
            encounterScenes: admitted.SelectMany(read => read.Definition.EncounterScenes.Values)), start);
    }

    private static bool SameAlly(BattleActorDefinition left, BattleActorDefinition right) =>
        left.ClassRule == right.ClassRule && left.Level == right.Level && left.MaxHp == right.MaxHp &&
        left.MaxMp == right.MaxMp && left.Attack == right.Attack && left.Defense == right.Defense &&
        left.Agility == right.Agility && left.ExtraRoundAction == right.ExtraRoundAction && left.Move == right.Move &&
        left.Physical == right.Physical && left.Mover == right.Mover && left.Spells.SequenceEqual(right.Spells) &&
        left.SourceLoadout!.Items.SequenceEqual(right.SourceLoadout!.Items) &&
        left.SourceLoadout.Spells.SequenceEqual(right.SourceLoadout.Spells);
}

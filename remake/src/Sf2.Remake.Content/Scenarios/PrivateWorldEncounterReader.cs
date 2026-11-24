using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// Additional private encounters are admitted at startup, never read during a map transition.
internal static class PrivateWorldEncounterReader
{
    internal static ScenarioReadAccepted Read(JsonElement selections, string worldPath, ScenarioReadAccepted first,
        PrivateBattleContentReader? newGame, IReadOnlyList<byte> fieldItems)
    {
        Require(selections.ValueKind == JsonValueKind.Array, "array-required", "encounters");
        // Product rows must resolve to distinct, authenticated encounters; the root occupies one slot.
        Require(selections.GetArrayLength() <= (newGame is null ? 1 : PrivateBattleEncounterReader.RegisteredCount - 1), "encounter-selection-limit", "encounters");
        var admitted = new List<ScenarioReadAccepted> { first };
        string directory = Path.GetDirectoryName(Path.GetFullPath(worldPath))!;
        foreach (var row in selections.EnumerateArray())
        {
            string[] required = ["placement", "scene", "terrain", "static", "enemies", "gold", "battleScenes"];
            if (newGame is null) Object(row, "encounter-inputs", [.. required, "start"]);
            else ObjectOptional(row, "encounter-inputs", required, "start", "repeatEnemies");
            string Input(string key)
            {
                string relative = Text(row, key);
                Require(!Path.IsPathRooted(relative), "relative-input-required", "encounters." + key);
                string resolved = Path.GetFullPath(Path.Combine(directory, relative));
                Require(resolved.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase),
                    "input-path-escape", "encounters." + key);
                return resolved;
            }
            string? scenePath = row.GetProperty("battleScenes").ValueKind == JsonValueKind.Null ? null : Input("battleScenes");
            var result = newGame is null
                ? new PrivateBattleScenarioReader(Input("placement"), Input("scene"), Input("terrain"),
                    Input("static"), Input("enemies"), Input("gold"), Input("start"), scenePath).Read(fieldItems)
                : newGame.Read(Input("placement"), Input("scene"), Input("terrain"), Input("static"), Input("enemies"), Input("gold"), scenePath,
                    row.TryGetProperty("repeatEnemies", out _) ? Array(row, "repeatEnemies").Select(id =>
                    {
                        Require(id.ValueKind == JsonValueKind.Number && id.TryGetByte(out _), "enemy-id", "encounters.repeatEnemies");
                        return id.GetByte();
                    }).ToArray() : []);
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
            definitions.SelectMany(value => value.EnemyGold.Values).DistinctBy(value => value.EnemyId),
            definitions.SelectMany(value => value.Allies.Values).DistinctBy(value => value.Id));
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
        var scenes = admitted.SelectMany(read => read.Definition.EncounterScenes.Values).ToArray();
        if (newGame is not null) scenes = CampaignScenes(scenes, merged);
        return new(new(first.Definition.Package, encounters, merged,
            battleScenes: scenes.FirstOrDefault(scene => scene.Encounter == first.Start.Encounter),
            encounterStarts: admitted.Select(read => read.Start),
            encounterScenes: scenes), start);
    }

    // An admitted party can revisit any selected encounter. Older scene inputs carry only
    // the ally resources known at their chapter. Share their immutable frames by reference.
    private static BattleSceneDefinition[] CampaignScenes(BattleSceneDefinition[] scenes, PrivateBattleDefinitions source)
    {
        var bodies = scenes.SelectMany(scene => scene.Allies.Select(pair => (pair.Key, pair.Value, scene.Rasters)))
            .DistinctBy(row => row.Key).ToArray();
        var equipment = scenes.SelectMany(scene => scene.Equipment!.Select(pair => (pair.Key, pair.Value, scene.Rasters)))
            .DistinctBy(row => row.Key).ToArray();
        var appearances = scenes.SelectMany(scene => (scene.AllyAppearances ?? new Dictionary<(ActorRef Actor, BattleClassRule Class, int Item), BattleSceneActorVisual>())
            .Select(pair => (pair.Key, pair.Value, scene.Rasters))).DistinctBy(row => row.Key).ToArray();
        // Spell icons have one source identity at every rank and can travel with
        // a learned book to earlier encounters just as its battle body can.
        var sharedRasters = scenes.SelectMany(scene => scene.Rasters)
            .Where(pair => pair.Key.StartsWith("kiwi-breath/", StringComparison.Ordinal) ||
                pair.Key.StartsWith("phoenix-wave/", StringComparison.Ordinal) ||
                pair.Key.StartsWith("identity-icon.spell.", StringComparison.Ordinal))
            .DistinctBy(pair => pair.Key).ToArray();
        var sprites = scenes.SelectMany(scene => scene.FieldDeath?.AllySprites ?? new Dictionary<int, int>())
            .Where(pair => source.Allies.ContainsKey((byte)pair.Key)).DistinctBy(pair => pair.Key)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        return scenes.Select(scene =>
        {
            var rasters = scene.Rasters.ToDictionary();
            foreach (var pair in sharedRasters) rasters.TryAdd(pair.Key, pair.Value);
            var allies = scene.Allies.ToDictionary();
            var weapons = scene.Equipment!.ToDictionary();
            var actorVisuals = (scene.AllyAppearances ?? new Dictionary<(ActorRef Actor, BattleClassRule Class, int Item), BattleSceneActorVisual>()).ToDictionary();
            foreach (var row in bodies)
                if (allies.TryAdd(row.Key, row.Value)) IncludeFrames(row.Value, row.Rasters);
            foreach (var row in equipment)
                if (weapons.TryAdd(row.Key, row.Value)) IncludeFrames(row.Value, row.Rasters);
            foreach (var row in appearances)
                if (actorVisuals.TryAdd(row.Key, row.Value)) IncludeFrames(row.Value, row.Rasters);
            return scene with { Rasters = rasters.AsReadOnly(), Allies = allies.AsReadOnly(), Equipment = weapons.AsReadOnly(),
                AllyAppearances = actorVisuals.AsReadOnly(),
                FieldDeath = scene.FieldDeath is { } field ? field with { AllySprites = sprites.AsReadOnly() } : null };

            void IncludeFrames(BattleSceneActorVisual visual, IReadOnlyDictionary<string, ExplorationRaster> resources)
            {
                foreach (var id in visual.Frames.Concat(visual.WeaponFrames)) rasters.TryAdd(id, resources[id]);
            }
        }).ToArray();
    }

    private static bool SameAlly(BattleActorDefinition left, BattleActorDefinition right) =>
        left.ClassRule == right.ClassRule && left.Level == right.Level && left.MaxHp == right.MaxHp &&
        left.MaxMp == right.MaxMp && left.Attack == right.Attack && left.Defense == right.Defense &&
        left.Agility == right.Agility && left.ExtraRoundAction == right.ExtraRoundAction && left.Move == right.Move &&
        left.Physical == right.Physical && left.Mover == right.Mover && left.Spells.SequenceEqual(right.Spells) &&
        left.SourceLoadout!.Items.SequenceEqual(right.SourceLoadout!.Items) &&
        left.SourceLoadout.Spells.SequenceEqual(right.SourceLoadout.Spells);
}

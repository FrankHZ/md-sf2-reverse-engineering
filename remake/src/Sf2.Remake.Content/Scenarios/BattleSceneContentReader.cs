using System.Collections.ObjectModel;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class BattleSceneContentReader
{
    internal static BattleSceneDefinition Read(string path, PrivateBattleDefinitions definitions)
    {
        Require(Path.IsPathFullyQualified(path), "scene-content-selection", "battleScenes");
        byte[] bytes;
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Require(stream.Length is > 0 and <= 4 * 1024 * 1024, "document-size", "battleScenes");
            bytes = new byte[(int)stream.Length]; stream.ReadExactly(bytes);
        }
        catch (IOException) { throw new AdmissionIssue(new(Sf2.Remake.Application.Runtime.SessionFailureKind.ContentError,
            "scene-content-unavailable", "battleScenes", "Battle scene content is unavailable.")); }
        catch (UnauthorizedAccessException) { throw new AdmissionIssue(new(Sf2.Remake.Application.Runtime.SessionFailureKind.ContentError,
            "scene-content-unavailable", "battleScenes", "Battle scene content cannot be read.")); }
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 16 });
        var root = document.RootElement;
        Object(root, "battleScenes", "version", "encounter", "background", "ground", "actors", "rasters", "texts", "memberNames");
        _ = Number(root, "version", 1, 1);
        Require(Text(root, "encounter") == "battle-" + definitions.Encounter.Battle.Id, "scene-encounter", "battleScenes.encounter");
        var rasters = new Dictionary<string, ExplorationRaster>(StringComparer.Ordinal);
        foreach (var row in root.GetProperty("rasters").EnumerateObject())
            Require(rasters.TryAdd(row.Name, ExplorationAssetReader.Raster(row.Value)), "scene-duplicate-resource", "battleScenes.rasters");
        string background = Text(root, "background"), ground = Text(root, "ground");
        Require(rasters.ContainsKey(background) && rasters.ContainsKey(ground), "scene-background", "battleScenes.rasters");
        var allies = new Dictionary<BattleClassRule, BattleSceneActorVisual>();
        var enemies = new Dictionary<ActorRef, BattleSceneActorVisual>();
        foreach (var row in Array(root, "actors"))
        {
            Object(row, "sceneActor", "side", "sprite", "palette", "item", "frames", "weaponFrames", "sequences");
            string side = Text(row, "side");
            int sprite = Number(row, "sprite", 0, 255), palette = Number(row, "palette", 0, 255);
            int? item = row.GetProperty("item").ValueKind == JsonValueKind.Null ? null : Number(row, "item", 0, 127);
            var frames = Strings(row, "frames");
            var weapons = Strings(row, "weaponFrames");
            Require(frames.Length > 0 && frames.Concat(weapons).All(rasters.ContainsKey), "scene-frame-resource", "battleScenes.frames");
            var sequences = new Dictionary<string, BattleSceneAnimation>(StringComparer.Ordinal);
            foreach (var sequence in row.GetProperty("sequences").EnumerateObject())
            {
                Require(sequence.Name is "idle" or "attack" or "dodge", "scene-sequence", "battleScenes.sequences");
                var value = sequence.Value;
                Object(value, "sceneSequence", "index", "trigger", "spell", "terminate", "idleWeapon", "frames");
                var entries = Array(value, "frames").Select(frame =>
                {
                    Object(frame, "sceneFrame", "frame", "ticks", "x", "y", "weapon");
                    int image = Number(frame, "frame", 0, 15);
                    Require(image == 15 || image < frames.Length, "scene-frame-index", "battleScenes.frames");
                    return new BattleSceneAnimationFrame(image, Number(frame, "ticks", 1, 255),
                        Number(frame, "x", -128, 127), Number(frame, "y", -128, 127), Weapon(frame.GetProperty("weapon")));
                }).ToArray();
                Require(entries.Length > 0, "scene-empty-sequence", "battleScenes.sequences");
                Require(sequences.TryAdd(sequence.Name, new(Number(value, "index", 0, 255), Number(value, "trigger", 0, 255),
                    Number(value, "spell", 0, 255), Number(value, "terminate", 0, 1) != 0,
                    Weapon(value.GetProperty("idleWeapon")), System.Array.AsReadOnly(entries))), "scene-duplicate-sequence", "battleScenes.sequences");
            }
            Require(sequences.Count == 3, "scene-required-sequence", "battleScenes.sequences");
            foreach (var weapon in sequences.Values.SelectMany(sequence => sequence.Frames.Select(frame => frame.Weapon)
                .Append(sequence.IdleWeapon)).OfType<BattleSceneWeaponFrame>())
                Require((weapon.Frame & 7) < weapons.Length, "scene-weapon-frame-index", "battleScenes.weaponFrames");
            var visual = new BattleSceneActorVisual(sprite, palette, item, System.Array.AsReadOnly(frames),
                System.Array.AsReadOnly(weapons), new ReadOnlyDictionary<string, BattleSceneAnimation>(sequences));
            if (side == "ally")
            {
                Require(sprite is 0 or 1 or 2, "scene-class", "battleScenes.actors", true);
                var kind = sprite switch { 0 => BattleClassRule.UnpromotedSwordsman, 1 => BattleClassRule.UnpromotedPriest,
                    _ => BattleClassRule.UnpromotedKnight };
                Require(palette == 0 && allies.TryAdd(kind, visual), "scene-ally-binding", "battleScenes.actors");
            }
            else
            {
                Require(side == "enemy" && sprite == 22 && palette == 2, "scene-enemy-binding", "battleScenes.actors", true);
                int index = 0;
                foreach (var placement in definitions.Encounter.Placements.Where(value => value.Kind == EncounterEntityKind.Enemy))
                {
                    Require(placement.IdentityExpression == "GIZMO", "scene-enemy-species", "battleScenes.actors", true);
                    Require(enemies.TryAdd(new("enemy-" + index++), visual), "scene-enemy-binding", "battleScenes.actors");
                }
            }
        }
        var texts = new Dictionary<int, string>();
        foreach (var value in root.GetProperty("texts").EnumerateObject())
        {
            Require(int.TryParse(value.Name, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture,
                out int id) && value.Value.ValueKind == JsonValueKind.String, "scene-text", "battleScenes.texts");
            Require(texts.TryAdd(id, value.Value.GetString()!), "scene-duplicate-text", "battleScenes.texts");
        }
        Require(new[] {244, 263, 266, 267, 268, 269, 270, 271, 273, 284, 285, 286, 287, 288, 290, 291, 292, 293, 393}
            .All(texts.ContainsKey), "scene-required-text", "battleScenes.texts");
        var names = Strings(root, "memberNames");
        Require(names.Length >= 3, "scene-member-names", "battleScenes.memberNames");
        return new(Text(root, "encounter"), background, ground, new ReadOnlyDictionary<string, ExplorationRaster>(rasters),
            new ReadOnlyDictionary<BattleClassRule, BattleSceneActorVisual>(allies), new ReadOnlyDictionary<ActorRef, BattleSceneActorVisual>(enemies),
            new ReadOnlyDictionary<int, string>(texts),
            System.Array.AsReadOnly(names));
    }

    private static string[] Strings(JsonElement row, string key) => Array(row, key).Select(value =>
    {
        Require(value.ValueKind == JsonValueKind.String, "string-required", key);
        return value.GetString()!;
    }).ToArray();

    private static BattleSceneWeaponFrame? Weapon(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null) return null;
        Object(value, "weaponFrame", "frame", "layer", "x", "y");
        return new(Number(value, "frame", 0, 255), Number(value, "layer", 1, 2),
            Number(value, "x", -128, 127), Number(value, "y", -128, 127));
    }
}

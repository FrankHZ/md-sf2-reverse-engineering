using System.Collections.ObjectModel;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class BattleSceneContentReader
{
    internal static BattleSceneDefinition Read(string path, PrivateBattleDefinitions definitions, IEnumerable<byte>? controlledMembers = null)
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
        ObjectOptional(root, "battleScenes",
            ["version", "encounter", "background", "ground", "actors", "rasters", "texts", "memberNames", "healing", "fieldDeath"],
            "terrainScenes", "hdRasters");
        _ = Number(root, "version", 1, 1);
        Require(Text(root, "encounter") == "battle-" + definitions.Encounter.Battle.Id, "scene-encounter", "battleScenes.encounter");
        var rasters = new Dictionary<string, ExplorationRaster>(StringComparer.Ordinal);
        foreach (var row in root.GetProperty("rasters").EnumerateObject())
            Require(rasters.TryAdd(row.Name, ExplorationAssetReader.Raster(row.Value)), "scene-duplicate-resource", "battleScenes.rasters");
        var hdRasters = ReadHdRasters(root, rasters);
        ExplorationAssetReader.BattlefieldMenu(rasters);
        var cursorKeys = rasters.Keys.Where(key => key.StartsWith("battle-cursor.", StringComparison.Ordinal)).ToArray();
        Require(cursorKeys.Length == 0 || cursorKeys.Length == 1 &&
            rasters.TryGetValue("battle-cursor.single-cell", out var cursor) && cursor.Width == 32 && cursor.Height == 32,
            "scene-target-cursor", "battleScenes.rasters");
        var blazeKeys = rasters.Keys.Where(key => key.StartsWith("blaze-1/", StringComparison.Ordinal)).ToArray();
        Require(blazeKeys.Length == 0 || blazeKeys.Length == 2 &&
            Enumerable.Range(0, 2).All(frame => rasters.TryGetValue("blaze-1/frame-" + frame, out var fire) &&
                fire.Width == 16 && fire.Height == 32), "scene-blaze-1", "battleScenes.rasters");
        (string Name, int Width, int Height)[] blazeTwo =
        [
            ("flame-0", 24, 48), ("flame-1", 24, 48), ("meteor-0", 16, 16), ("meteor-1", 16, 16),
            ("impact-0", 24, 24), ("impact-1", 24, 16),
        ];
        const string blazeTwoPrefix = "blaze-2/";
        int blazeTwoCount = rasters.Keys.Count(key => key.StartsWith(blazeTwoPrefix, StringComparison.Ordinal));
        Require(blazeTwoCount == 0 || blazeTwoCount == blazeTwo.Length &&
            blazeTwo.All(row => rasters.TryGetValue(blazeTwoPrefix + row.Name, out var image) &&
                image.Width == row.Width && image.Height == row.Height), "scene-blaze-2", "battleScenes.rasters");
        (int Width, int Height)[] blast = [(8,8), (16,16), (16,24), (8,16), (16,16), (24,16),
            (16,16), (16,32), (16,24), (16,16), (32,16), (24,16), (16,16), (32,32), (16,24)];
        int blastCount = rasters.Keys.Count(key => key.StartsWith("blast-1/", StringComparison.Ordinal));
        Require(blastCount == 0 || blastCount == blast.Length &&
            blast.Select((size, slot) => rasters.TryGetValue("blast-1/frame-" + slot, out var image) &&
                image.Width == size.Width && image.Height == size.Height).All(valid => valid),
            "scene-blast-1", "battleScenes.rasters");
        int[] detoxSizes = [8, 8, 16, 16, 24, 24, 8, 8, 8, 8, 8];
        int detoxCount = rasters.Keys.Count(key => key.StartsWith("detox/", StringComparison.Ordinal));
        Require(detoxCount == 0 || detoxCount == detoxSizes.Length &&
            detoxSizes.Select((size, slot) => rasters.TryGetValue("detox/frame-" + slot, out var image) &&
                image.Width == size && image.Height == size).All(valid => valid),
            "scene-detox", "battleScenes.rasters");
        IdentityIconReader.Validate(rasters);
        IdentityIconReader.Bindings(rasters, definitions);
        string background = Text(root, "background"), ground = Text(root, "ground");
        Require(rasters.ContainsKey(background) && rasters.ContainsKey(ground), "scene-background", "battleScenes.rasters");
        var allies = new Dictionary<BattleClassRule, BattleSceneActorVisual>();
        var equipment = new Dictionary<(BattleClassRule Class, int Item), BattleSceneActorVisual>();
        var allyAppearances = new Dictionary<(ActorRef Actor, BattleClassRule Class, int Item), BattleSceneActorVisual>();
        var enemyBodies = new Dictionary<(int, int), BattleSceneActorVisual>();
        foreach (var row in Array(root, "actors"))
        {
            ObjectOptional(row, "sceneActor", "members", "side", "sprite", "palette", "item", "frames", "weaponFrames", "sequences", "idleTicks");
            string side = Text(row, "side");
            int sprite = Number(row, "sprite", 0, 255), palette = Number(row, "palette", 0, 255);
            int? item = row.GetProperty("item").ValueKind == JsonValueKind.Null ? null : Number(row, "item", 0, 127);
            var frames = Strings(row, "frames");
            var weapons = Strings(row, "weaponFrames");
            Require(frames.Length > 0 && frames.Concat(weapons).All(rasters.ContainsKey), "scene-frame-resource", "battleScenes.frames");
            var sequences = new Dictionary<string, BattleSceneAnimation>(StringComparer.Ordinal);
            foreach (var sequence in row.GetProperty("sequences").EnumerateObject())
            {
                Require(sequence.Name is "idle" or "attack" or "dodge" or "item" or "cast", "scene-sequence", "battleScenes.sequences");
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
            Require(new[] { "idle", "attack", "dodge" }.All(sequences.ContainsKey) &&
                (side != "ally" || sequences.ContainsKey("item")), "scene-required-sequence", "battleScenes.sequences");
            foreach (var weapon in sequences.Values.SelectMany(sequence => sequence.Frames.Select(frame => frame.Weapon)
                .Append(sequence.IdleWeapon)).OfType<BattleSceneWeaponFrame>())
                Require((weapon.Frame & 7) < weapons.Length, "scene-weapon-frame-index", "battleScenes.weaponFrames");
            var visual = new BattleSceneActorVisual(sprite, palette, item, System.Array.AsReadOnly(frames),
                System.Array.AsReadOnly(weapons), new ReadOnlyDictionary<string, BattleSceneAnimation>(sequences),
                row.TryGetProperty("idleTicks", out _) ? Number(row, "idleTicks", 1, 65535) : 0);
            if (side == "ally")
            {
                Require(sprite is >= 0 and <= 14 or 17 or 18 or 21 or 22 or 24 or 25 or 26 or 27 or 28 or 29 or 30 or 31, "scene-class", "battleScenes.actors", true);
                var kind = sprite switch { 0 => BattleClassRule.UnpromotedSwordsman, 1 => BattleClassRule.UnpromotedPriest,
                    2 => BattleClassRule.UnpromotedKnight, 3 => BattleClassRule.UnpromotedWarrior, 4 => BattleClassRule.UnpromotedMage, 5 => BattleClassRule.UnpromotedThief, 6 => BattleClassRule.UnpromotedTortoise,
                    7 => BattleClassRule.UnpromotedPhoenix, 8 => BattleClassRule.UnpromotedRanger, 9 => BattleClassRule.UnpromotedWolfman, 11 => BattleClassRule.BrassGunner, 12 => BattleClassRule.UnpromotedArcher, 13 => BattleClassRule.Paladin,
                    14 => BattleClassRule.BirdBattler, 17 => BattleClassRule.PegasusKnight, 18 => BattleClassRule.Gladiator, 21 => BattleClassRule.Wizard,
                    22 => BattleClassRule.Baron, 24 => BattleClassRule.Hero,
                    25 => BattleClassRule.Vicar, 26 => BattleClassRule.Ninja, 27 => BattleClassRule.Monster, 28 => BattleClassRule.Phoenix, 29 => BattleClassRule.BowKnight, 30 => BattleClassRule.WolfBaron,
                    31 => BattleClassRule.Sniper, _ => BattleClassRule.UnpromotedBirdman };
                if (row.TryGetProperty("members", out _))
                {
                    var members = Array(row, "members").Select(value =>
                    {
                        Require(value.TryGetByte(out byte id) && definitions.Allies.TryGetValue(id, out var member) &&
                            Sf2ClassRules.SourceClass(kind) is { } classId &&
                            (member.ClassId == classId || Sf2ClassRules.PromotionDestination(member.ClassId, classId) == kind &&
                                definitions.Classes.ContainsKey(classId)),
                            "scene-ally-member", "battleScenes.actors.members");
                        return new ActorRef("ally-" + id);
                    }).ToArray();
                    Require(members.Length > 0 && members.Distinct().Count() == members.Length, "scene-ally-member", "battleScenes.actors.members");
                    foreach (var member in members)
                    {
                        if (item is { } weapon)
                            Require(allyAppearances.TryAdd((member, kind, weapon), visual), "scene-ally-binding", "battleScenes.actors");
                        allyAppearances.TryAdd((member, kind, -1), visual);
                    }
                    // Older class consumers retain the ordinary palette as their fallback.
                    if (palette == (sprite == 12 ? 1 : 0))
                    {
                        if (item is { } weapon) equipment.TryAdd((kind, weapon), visual);
                        allies.TryAdd(kind, visual);
                    }
                }
                else
                {
                    Require(palette == (sprite == 12 ? 1 : 0) && (item is null || equipment.TryAdd((kind, item.Value), visual)), "scene-ally-binding", "battleScenes.actors");
                    allies.TryAdd(kind, visual);
                }
            }
            else
            {
                Require(side == "enemy", "scene-enemy-binding", "battleScenes.actors", true);
                Require(enemyBodies.TryAdd((sprite, palette), visual), "scene-enemy-binding", "battleScenes.actors");
            }
        }
        var speciesBodies = definitions.Enemies.Values.ToDictionary(enemy => enemy.Id, enemy =>
            enemyBodies.TryGetValue(Body(enemy.Code), out var body) ? body :
                throw new AdmissionIssue(new(Sf2.Remake.Application.Runtime.SessionFailureKind.ContentError,
                    "scene-enemy-binding", enemy.Code, "Missing selected enemy body.")));
        var roster = definitions.Encounter.Placements.Where(row => row.Kind == EncounterEntityKind.Enemy)
            .Select((row, index) => (Actor: new ActorRef("enemy-" + index), Enemy: definitions.Enemies[row.IdentityExpression])).ToArray();
        var enemies = roster.ToDictionary(row => row.Actor, row => speciesBodies[row.Enemy.Id]);
        var appearances = new Dictionary<byte, EnemyAppearance>();
        var texts = new Dictionary<int, string>();
        foreach (var value in root.GetProperty("texts").EnumerateObject())
        {
            Require(int.TryParse(value.Name, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture,
                out int id) && value.Value.ValueKind == JsonValueKind.String, "scene-text", "battleScenes.texts");
            Require(texts.TryAdd(id, value.Value.GetString()!), "scene-duplicate-text", "battleScenes.texts");
        }
        Require(new[] {244, 263, 266, 267, 268, 269, 270, 271, 273, 275, 298, 284, 285, 286, 287, 288, 290, 291, 292, 293, 393}
            .All(texts.ContainsKey), "scene-required-text", "battleScenes.texts");
        var names = Strings(root, "memberNames");
        if (definitions.Enemies.Values.Any(enemy => enemy.ProwessExpression == "CRITICAL_SLEEP|DOUBLE_1IN32|COUNTER_1IN32"))
            Require(new[] { 306, 338, 354 }.All(texts.ContainsKey), "scene-sleep-text", "battleScenes.texts");
        Require(names.Length >= 3, "scene-member-names", "battleScenes.memberNames");
        HealingSceneVisual? healing = null;
        if (root.TryGetProperty("healing", out var heal))
        {
            ObjectOptional(heal, "sceneHealing", ["bodies", "wings", "dust"], "enemy");
            var bodies = Strings(heal, "bodies"); var wings = Strings(heal, "wings"); var dust = Strings(heal, "dust");
            Require(bodies.Length == 2 && wings.Length == 2 && dust.Length == 5 &&
                bodies.Concat(wings).Concat(dust).All(rasters.ContainsKey), "scene-healing-resources", "battleScenes.healing");
            Require(texts.ContainsKey(274) && allies.TryGetValue(BattleClassRule.UnpromotedPriest, out var priest) &&
                priest.IdleTicks > 0 && priest.Sequences.TryGetValue("cast", out var cast) &&
                !cast.Terminate && cast.Frames.Count > 1 &&
                (cast.Trigger == 0 || cast.Trigger >= 2 && cast.Trigger <= cast.Frames.Count),
                "scene-healing-cast", "battleScenes.healing");
            healing = new(System.Array.AsReadOnly(bodies), System.Array.AsReadOnly(wings), System.Array.AsReadOnly(dust));
            if (heal.TryGetProperty("enemy", out var enemyHeal))
            {
                Object(enemyHeal, "sceneHealingEnemy", "bodies", "wings", "dust");
                var fairyBodies = Strings(enemyHeal, "bodies");
                var enemyWings = Strings(enemyHeal, "wings");
                var enemyDust = Strings(enemyHeal, "dust");
                Require(fairyBodies.Length == 2 && enemyWings.Length == 2 && enemyDust.Length == 5 &&
                    fairyBodies.Concat(enemyWings).Concat(enemyDust).All(rasters.ContainsKey),
                    "scene-healing-resources", "battleScenes.healing.enemy");
                healing = healing with { Enemy = new(System.Array.AsReadOnly(fairyBodies),
                    System.Array.AsReadOnly(enemyWings), System.Array.AsReadOnly(enemyDust)) };
            }
        }
        FieldDeathVisual? fieldDeath = null;
        if (root.TryGetProperty("fieldDeath", out var field))
        {
            ObjectOptional(field, "fieldDeath", ["allies", "enemies", "exitFrames"], "neutral", "neutrals");
            var allySprites = new Dictionary<int, int>();
            foreach (var row in Array(field, "allies"))
            {
                Object(row, "fieldAlly", "character", "sprite");
                Require(allySprites.TryAdd(Number(row, "character", 0, 29), Number(row, "sprite", 0, 236)),
                    "field-ally-binding", "fieldDeath.allies");
            }
            Require(allySprites.Count > 0 && (controlledMembers?.All(id => allySprites.ContainsKey(id)) ?? true), "field-ally-binding", "fieldDeath.allies");
            var species = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var row in Array(field, "enemies"))
            {
                Object(row, "fieldEnemy", "code", "sprite");
                string code = Text(row, "code");
                int sprite = Number(row, "sprite", 0, code switch { "TAROS" => 255, "KRAKEN_HEAD" => 254, _ => 236 });
                if (code == "TAROS") Require(sprite == 255, "field-taros-pose", "fieldDeath.enemies");
                if (code == "KRAKEN_HEAD")
                    Require(sprite == 254 && Enumerable.Range(0, 2).All(frame =>
                        rasters.TryGetValue("field-kraken-head/frame" + frame, out var pose) && pose.Width == 96 && pose.Height == 96),
                        "field-kraken-pose", "fieldDeath.enemies");
                Require(species.TryAdd(code, sprite), "field-enemy-binding", "fieldDeath.enemies");
            }
            foreach (var enemy in definitions.Enemies.Values)
            {
                Require(species.TryGetValue(enemy.Code, out int sprite), "field-enemy-binding", "fieldDeath.enemies");
                appearances.Add(enemy.Id, new(enemy.DisplayName, speciesBodies[enemy.Id], sprite));
            }
            var enemySprites = new Dictionary<ActorRef, int>();
            int ordinal = 0;
            foreach (var placement in definitions.Encounter.Placements.Where(value => value.Kind == EncounterEntityKind.Enemy))
            {
                Require(species.TryGetValue(placement.IdentityExpression, out int sprite), "field-enemy-binding", "fieldDeath.enemies");
                enemySprites.Add(new("enemy-" + ordinal++), sprite);
            }
            var frames = Strings(field, "exitFrames");
            Require(frames.Length == 3 && frames.All(key => rasters.TryGetValue(key, out var raster) &&
                raster.Width == 48 && raster.Height == 24), "field-exit-resource", "fieldDeath.exitFrames");
            Require(!(field.TryGetProperty("neutral", out _) && field.TryGetProperty("neutrals", out _)),
                "field-neutral-shape", "fieldDeath.neutrals");
            var neutralRows = field.TryGetProperty("neutrals", out _) ? Array(field, "neutrals").ToArray() :
                field.TryGetProperty("neutral", out var legacyNeutral) ? [legacyNeutral] : System.Array.Empty<JsonElement>();
            Require(neutralRows.Length <= 32, "field-neutral-count", "fieldDeath.neutrals");
            var neutrals = neutralRows.Select(neutralRow =>
            {
                ObjectOptional(neutralRow, "neutral", ["sprite", "x", "y", "facing", "completedFlag"], "whirlsInWater", "lyingLeft");
                return new NeutralBattleSprite(Number(neutralRow, "sprite", 0, 255), Number(neutralRow, "x", 0, 63), Number(neutralRow, "y", 0, 63),
                    Number(neutralRow, "facing", 0, 3), Number(neutralRow, "completedFlag", 0, 1023),
                    neutralRow.TryGetProperty("whirlsInWater", out _) && Boolean(neutralRow, "whirlsInWater"),
                    neutralRow.TryGetProperty("lyingLeft", out _) && Boolean(neutralRow, "lyingLeft"));
            }).ToArray();
            fieldDeath = new(new ReadOnlyDictionary<int, int>(allySprites), new ReadOnlyDictionary<ActorRef, int>(enemySprites),
                System.Array.AsReadOnly(frames), System.Array.AsReadOnly(neutrals));
        }
        BattleSceneTerrainVisual[]? terrainScenes = null;
        if (root.TryGetProperty("terrainScenes", out _))
        {
            var bindings = new Dictionary<byte, BattleSceneTerrainVisual>();
            foreach (var row in Array(root, "terrainScenes"))
            {
                Object(row, "terrainScene", "terrain", "background", "ground");
                var value = new BattleSceneTerrainVisual(Text(row, "background"), Text(row, "ground"));
                Require(rasters.ContainsKey(value.Background) && rasters.ContainsKey(value.Ground) &&
                    bindings.TryAdd((byte)Number(row, "terrain", 0, 15), value), "terrain-scene-binding", "battleScenes.terrainScenes");
            }
            Require(definitions.Encounter.Terrain.All(value => bindings.ContainsKey((byte)(value & 15))),
                "terrain-scene-binding", "battleScenes.terrainScenes");
            terrainScenes = definitions.Encounter.Terrain.Select(value => bindings[(byte)(value & 15)]).ToArray();
        }
        return new(Text(root, "encounter"), background, ground, new ReadOnlyDictionary<string, ExplorationRaster>(rasters),
            new ReadOnlyDictionary<BattleClassRule, BattleSceneActorVisual>(allies), new ReadOnlyDictionary<ActorRef, BattleSceneActorVisual>(enemies),
            new ReadOnlyDictionary<int, string>(texts),
            System.Array.AsReadOnly(names), healing, fieldDeath, terrainScenes is null ? null : System.Array.AsReadOnly(terrainScenes),
            roster.ToDictionary(row => row.Actor, row => row.Enemy.DisplayName).AsReadOnly(), equipment.AsReadOnly(),
            roster.ToDictionary(row => row.Actor, row => row.Enemy.Id).AsReadOnly(), appearances.AsReadOnly(), allyAppearances.AsReadOnly(),
            hdRasters);
    }

    private static IReadOnlyDictionary<string, ExplorationRaster>? ReadHdRasters(JsonElement root,
        IReadOnlyDictionary<string, ExplorationRaster> originals)
    {
        if (!root.TryGetProperty("hdRasters", out var bank)) return null;
        const string field = "battleScenes.hdRasters";
        Require(bank.ValueKind == JsonValueKind.Object, "object-required", field);
        var variants = new Dictionary<string, ExplorationRaster>(StringComparer.Ordinal);
        foreach (var row in bank.EnumerateObject())
        {
            Require(originals.TryGetValue(row.Name, out var original), "scene-hd-original", field);
            var variant = ExplorationAssetReader.Raster(row.Value);
            int factor = variant.Width / original!.Width;
            bool ui = row.Name.StartsWith("identity-icon.", StringComparison.Ordinal) ||
                row.Name.StartsWith("command-menu.", StringComparison.Ordinal) ||
                row.Name.StartsWith("battlefield-menu.", StringComparison.Ordinal) ||
                row.Name.StartsWith("battle-cursor.", StringComparison.Ordinal);
            Require(ui ? IdentityIconReader.IsHdShape(original, variant) :
                factor is 2 or 4 && variant.Width == original.Width * factor && variant.Height == original.Height * factor,
                "scene-hd-raster-shape", field);
            Require(variants.TryAdd(row.Name, variant), "scene-duplicate-resource", field);
        }
        return variants.AsReadOnly();
    }

    // Pinned EnemyBattlesprites sprite and palette pairs.
    private static (int, int) Body(string code) => code switch
        { "KRAKEN_HEAD" => (44, 0), "KRAKEN_ARM" => (33, 0), "KRAKEN_LEG" => (8, 0),
            "KING" => (46, 1), "QUEEN" => (37, 2), "BISHOP" => (42, 0),
            "KNIGHT" => (3, 1), "ROOK" => (34, 1), "PAWN" => (2, 1),
            "RAT" => (1, 1), "WILLARD" => (1, 2),
            "BUBBLING_OOZE" => (0, 2), "SKELETON" => (11, 0), "WIZARD" => (38, 0),
            "LIZARDMAN" => (12, 0), "LESSER_DEMON" => (24, 1), "DARK_SNIPER" => (32, 1), "DARK_SOLDIER" => (5, 1),
            "DARK_BISHOP" => (42, 1), "MASTER_MONK" => (41, 2),
            "GIZMO" => (22, 2), "OOZE" => (0, 1), "HUGE_RAT" => (1, 0), "HUGE_BAT" => (23, 0),
            "GALAM_SOLDIER" => (2, 0), "GALAM_ARCHER" => (31, 0),
            "GALAM_KNIGHT" => (3, 0), "GALAM_MAGE" => (36, 0), "DARK_CLERIC" => (40, 0), "DARK_SMOKE" => (22, 0),
            "HOBGOBLIN" => (4, 2), "EVIL_CLOUD" => (22, 1), "DEATH_ARCHER" => (31, 1),
            "DEATH_MONK" => (41, 0), "BLACK_MONK" => (41, 1), "ZOMBIE" => (6, 0), "TAROS" => (45, 0), "GOLEM" => (7, 0), "MASTER_MAGE" => (36, 1),
            "HARPY" => (25, 0), "ORC" => (10, 0), "HIGH_PRIEST" => (40, 1), "DARK_MADAM" => (37, 1),
            "SOULSOWER" => (9, 0), "GARGOYLE" => (24, 0), "ARROW_LAUNCHER" => (34, 0),
            "VAMPIRE_BAT" => (23, 2), "DARK_DWARF" => (5, 2),
            "HUNTER_GOBLIN" => (32, 0), "WITCH" => (37, 0), "GOBLIN" => (4, 0), "GREEN_OOZE" => (0, 0),
            _ => throw new AdmissionIssue(new(Sf2.Remake.Application.Runtime.SessionFailureKind.UnsupportedCapability,
                "scene-enemy-species", "battleScenes.actors", "unadmitted species")) };

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

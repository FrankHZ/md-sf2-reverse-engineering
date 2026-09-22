using System.Text.Json;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Content.Scenarios;

public sealed class AuthoredScenarioPackageReader : IScenarioSource
{
    private readonly Func<byte[]> _read;
    public AuthoredScenarioPackageReader(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _read = () => File.ReadAllBytes(path);
    }
    private AuthoredScenarioPackageReader(byte[] bytes) { _read = () => bytes; }
    public static AuthoredScenarioPackageReader FromDocumentBytes(IEnumerable<byte> bytes) => new(bytes.ToArray());

    public ScenarioReadResult Read()
    {
        try
        {
            byte[] bytes = _read();
            Require(bytes.Length <= 4 * 1024 * 1024, "document-size", "document");
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 32 });
            var root = document.RootElement;
            return root.ValueKind == JsonValueKind.Object && root.TryGetProperty("formatVersion", out var version) &&
                version.ValueKind == JsonValueKind.Number && version.TryGetInt32(out int number) && number == 8 ? ExplorationContentReader.ReadAuthored(root) : DecodeBattle(root);
        }
        catch (AdmissionIssue issue) { return new ScenarioReadRejected(issue.Failure); }
        catch (BattleRuleException error)
        { return new ScenarioReadRejected(new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
            error.Code, error.Field, error.Code.Replace('-', ' '))); }
        catch (JsonException) { return Rejected("json-syntax", "document"); }
        catch (IOException) { return Rejected("content-read", "document"); }
        catch (UnauthorizedAccessException) { return Rejected("content-read", "document"); }
    }

    internal static ScenarioReadAccepted DecodeBattle(JsonElement root)
    {
        ObjectOptional(root, "document", "items", "formatVersion", "package", "profile", "ruleProfile", "start", "terrains", "maps", "spells", "actors", "encounters");
        Require(Number(root, "formatVersion", 7, 7) == 7, "format-version", "formatVersion");
        Require(Text(root, "profile") == "public-authored", "profile", "profile");
        Require(Text(root, "ruleProfile") == "sf2-semantic-subset-v1", "rule-profile", "ruleProfile", true);
        string package = Id(root, "package");
        var start = root.GetProperty("start");
        Object(start, "start", "encounter", "initialVitals", "mainSeed", "thinkingSeed", "gold", "actors");
        Require(Text(start, "initialVitals") == "authored-controlled", "initial-vitals-policy", "start.initialVitals");
        string selectedEncounter = Id(start, "encounter");
        uint mainSeed = WordImage(start, "mainSeed"), thinkingSeed = WordImage(start, "thinkingSeed");

        var terrains = new Dictionary<string, (int Width, int Height, BattleTerrain[] Cells)>(StringComparer.Ordinal);
        foreach (var terrain in Array(root, "terrains"))
        {
            Object(terrain, "terrain", "id", "legend", "rows");
            string id = Id(terrain, "id");
            var legendInput = terrain.GetProperty("legend");
            Require(legendInput.ValueKind == JsonValueKind.Object, "object-required", "terrain.legend");
            var legend = new Dictionary<char, BattleTerrain>();
            foreach (var entry in legendInput.EnumerateObject())
            {
                Require(entry.Name.Length == 1 && entry.Name[0] is >= '!' and <= '~', "terrain-symbol", "terrain.legend");
                Object(entry.Value, "terrain.definition", "surface", "protection");
                string surfaceName = Text(entry.Value, "surface"), protectionName = Text(entry.Value, "protection");
                Require(surfaceName is "open" or "brush" or "rough" or "deep" or "impassable" or "barrier",
                    "terrain-surface", "terrain.legend.surface", true);
                Require(protectionName is "none" or "light" or "heavy", "terrain-protection", "terrain.legend.protection", true);
                var surface = surfaceName switch {
                    "open" => TerrainSurface.Open, "brush" => TerrainSurface.Brush, "rough" => TerrainSurface.Rough,
                    "deep" => TerrainSurface.Deep, "impassable" => TerrainSurface.Impassable, _ => TerrainSurface.Barrier };
                var protection = protectionName switch {
                    "none" => TerrainProtection.None, "light" => TerrainProtection.Light, _ => TerrainProtection.Heavy };
                Require(legend.TryAdd(entry.Name[0], new(surface, protection)), "duplicate-terrain-symbol", "terrain.legend");
            }
            var rows = Array(terrain, "rows").Select(row =>
            {
                Require(row.ValueKind == JsonValueKind.String, "terrain-row", "terrain.rows");
                return row.GetString()!;
            }).ToArray();
            Require(rows.Length is >= 1 and <= 48, "terrain-height", "terrain.rows");
            int width = rows[0].Length;
            Require(width is >= 1 and <= 48 && rows.All(row => row.Length == width), "terrain-width", "terrain.rows");
            var cells = Enumerable.Repeat(new BattleTerrain(TerrainSurface.Barrier, TerrainProtection.None), 2304).ToArray();
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < width; x++)
                {
                    char entry = rows[y][x];
                    Require(legend.ContainsKey(entry), "missing-terrain-definition", "terrain.rows");
                    cells[y * 48 + x] = legend[entry];
                }
            Require(terrains.TryAdd(id, (width, rows.Length, cells)), "duplicate-terrain", "terrains.id");
        }
        var maps = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var map in Array(root, "maps"))
        {
            Object(map, "map", "id", "terrain");
            string id = Id(map, "id"), terrain = Id(map, "terrain");
            Require(terrains.ContainsKey(terrain), "missing-terrain", "maps.terrain");
            Require(maps.TryAdd(id, terrain), "duplicate-map", "maps.id");
        }
        var spells = new Dictionary<SpellRef, HealingSpellDefinition>();
        foreach (var spell in Array(root, "spells"))
        {
            Object(spell, "spell", "id", "level", "mpCost", "minimumRange", "maximumRange", "effect");
            var key = new SpellRef(Id(spell, "id"), (byte)Number(spell, "level", 1, 4));
            var effect = spell.GetProperty("effect");
            Object(effect, "spell.effect", "kind", "adjustedPower", "fullRecovery");
            Require(Text(effect, "kind") == "heal", "spell-effect", "spell.effect.kind", true);
            Require(effect.GetProperty("fullRecovery").ValueKind is JsonValueKind.False or JsonValueKind.True,
                "boolean", "spell.effect.fullRecovery");
            Require(!effect.GetProperty("fullRecovery").GetBoolean(), "full-recovery", "spell.effect.fullRecovery", true);
            byte minimum = (byte)Number(spell, "minimumRange", 0, 255);
            byte maximum = (byte)Number(spell, "maximumRange", minimum, 255);
            var definition = new HealingSpellDefinition(key, (byte)Number(spell, "mpCost", 0, 255),
                (ushort)Number(effect, "adjustedPower", 0, 65535), minimum, maximum);
            Require(spells.TryAdd(key, definition), "duplicate-spell", "spells.id/level");
        }
        var healingItems = new Dictionary<byte, HealingItemDefinition>();
        if (root.TryGetProperty("items", out _))
            foreach (var item in Array(root, "items"))
            {
                Object(item, "item", "id", "name", "effect", "power", "minimumRange", "maximumRange");
                Require(Text(item, "effect") == "consumable-healing", "item-effect", "items.effect", true);
                byte id = (byte)Number(item, "id", 0, 126);
                byte minimum = (byte)Number(item, "minimumRange", 0, 3);
                var definition = new HealingItemDefinition(id, Text(item, "name"), (ushort)Number(item, "power", 1, 254),
                    minimum, (byte)Number(item, "maximumRange", minimum, 3));
                Require(healingItems.TryAdd(id, definition), "duplicate-item", "items.id");
            }
        var actors = new Dictionary<ActorRef, BattleActorDefinition>();
        foreach (var actor in Array(root, "actors"))
        {
            ObjectOptional(actor, "actor", "physical", "id", "classRule", "level", "maxHp", "maxMp",
                "attack", "defense", "agility", "extraRoundAction", "move", "items", "spells");
            var id = new ActorRef(Id(actor, "id"));
            string className = Text(actor, "classRule");
            Require(className is "unpromoted-priest" or "ordinary" or "unpromoted-swordsman" or "unpromoted-warrior",
                "class-rule", "actors.classRule", true);
            var items = Array(actor, "items").Select(value =>
            {
                Require(value.ValueKind == JsonValueKind.Number && value.TryGetUInt16(out _), "item-word", "actors.items");
                ushort word = value.GetUInt16();
                Require(word <= 511 && ((word & 127) == 127 || healingItems.ContainsKey((byte)(word & 127))),
                    "missing-item", "actors.items", true);
                return word;
            }).ToArray();
            Require(items.Length <= 4, "inventory-capacity", "actors.items");
            var loadout = new BattleSourceLoadout(items.Concat(Enumerable.Repeat((ushort)127, 4 - items.Length)), [63, 63, 63, 63]);
            PhysicalActorDefinition? physical = null;
            if (actor.TryGetProperty("physical", out var physicalInput))
            {
                Object(physicalInput, "actor.physical", "movementType", "critical", "promoted", "leader", "gold", "special");
                Require(Text(physicalInput, "movementType") == "regular", "physical-movement-type", "actors.physical.movementType", true);
                Require(Text(physicalInput, "special") == "none", "physical-special-rule", "actors.physical.special", true);
                var criticalInput = physicalInput.GetProperty("critical");
                Object(criticalInput, "actors.physical.critical", "chance", "damageBonus");
                string chance = Text(criticalInput, "chance"), bonus = Text(criticalInput, "damageBonus");
                Require((chance, bonus) is ("one-in-32", "half") or ("one-in-16", "quarter"),
                    "physical-critical-rule", "actors.physical.critical", true);
                var critical = chance == "one-in-32" ? PhysicalCriticalRule.OneIn32WithHalfBonus
                    : PhysicalCriticalRule.OneIn16WithQuarterBonus;
                bool promoted = Boolean(physicalInput, "promoted");
                Require(className == "ordinary" || !promoted, "class-promotion", "actors.physical.promoted");
                physical = new(critical, promoted, Boolean(physicalInput, "leader"),
                    (ushort)Number(physicalInput, "gold", 0, 65535));
            }
            ushort maximumHp = (ushort)Number(actor, "maxHp", 1, 65535);
            byte maximumMp = (byte)Number(actor, "maxMp", 0, 255);
            var learned = new List<SpellRef>();
            foreach (var reference in Array(actor, "spells"))
            {
                Object(reference, "spell-reference", "id", "level");
                var key = new SpellRef(Id(reference, "id"), (byte)Number(reference, "level", 1, 4));
                Require(spells.ContainsKey(key), "missing-spell", "actors.spells");
                Require(!learned.Contains(key), "duplicate-learned-spell", "actors.spells");
                learned.Add(key);
            }
            Require(learned.Count <= 4, "spellbook-capacity", "actors.spells");
            var definition = new BattleActorDefinition(id,
                className switch { "unpromoted-priest" => BattleClassRule.UnpromotedPriest,
                    "unpromoted-swordsman" => BattleClassRule.UnpromotedSwordsman,
                    "unpromoted-warrior" => BattleClassRule.UnpromotedWarrior, _ => BattleClassRule.Ordinary },
                (byte)Number(actor, "level", 0, 99), maximumHp, maximumMp,
                (byte)Number(actor, "attack", 0, 255), (byte)Number(actor, "defense", 0, 255),
                (byte)Number(actor, "agility", 0, 127), Boolean(actor, "extraRoundAction"),
                (byte)Number(actor, "move", 1, 255), learned, physical, sourceLoadout: loadout);
            Require(actors.TryAdd(id, definition), "duplicate-actor", "actors.id");
        }
        var encounters = new Dictionary<string, BattleDefinition>(StringComparer.Ordinal);
        foreach (var encounter in Array(root, "encounters"))
        {
            ObjectOptional(encounter, "encounter", "rewards", "id", "map", "placements");
            BattleRewardDefinition? rewards = null;
            if (encounter.TryGetProperty("rewards", out var rewardInput))
            {
                Object(rewardInput, "encounter.rewards", "halvedExperience");
                rewards = new(Boolean(rewardInput, "halvedExperience"));
            }
            string id = Id(encounter, "id"), map = Id(encounter, "map");
            Require(maps.ContainsKey(map), "missing-map", "encounters.map");
            var terrain = terrains[maps[map]];
            var placements = new List<BattleDeploymentDefinition>();
            foreach (var placement in Array(encounter, "placements"))
            {
                Object(placement, "placement", "actor", "faction", "processingOrder", "control", "aiStrategy", "x", "y");
                var actorRef = new ActorRef(Id(placement, "actor"));
                Require(actors.ContainsKey(actorRef), "missing-actor", "placements.actor");
                var actor = actors[actorRef];
                string factionName = Text(placement, "faction");
                Require(factionName is "ally" or "enemy", "battle-faction", "placements.faction", true);
                var faction = factionName == "ally" ? BattleFaction.Ally : BattleFaction.Enemy;
                int order = Number(placement, "processingOrder", 0, int.MaxValue);
                string controlName = Text(placement, "control");
                Require(controlName is "player" or "automatic", "control-mode", "placements.control", true);
                var control = controlName == "player" ? BattleControl.Player : BattleControl.Automatic;
                BattleAiStrategy? aiStrategy = null;
                if (placement.GetProperty("aiStrategy").ValueKind != JsonValueKind.Null)
                {
                    string strategyName = Text(placement, "aiStrategy");
                    Require(strategyName is "stay" or "attack-then-approach", "ai-strategy", "placements.aiStrategy", true);
                    aiStrategy = strategyName == "stay" ? BattleAiStrategy.Stay : BattleAiStrategy.AttackThenApproach;
                }
                Require(faction != BattleFaction.Ally || actor.Level >= 1, "numeric-range", "actors.level");
                var position = new MapPosition(Number(placement, "x", 0, terrain.Width - 1), Number(placement, "y", 0, terrain.Height - 1));
                var tile = terrain.Cells[position.Y * 48 + position.X];
                Require(BattleTerrainRules.MovementCost(tile) > 0, "blocked-placement", "placements");
                Require(!placements.Any(a => a.Actor == actorRef), "duplicate-placement", "placements.actor");
                Require(!placements.Any(a => a.ProcessingOrder == order), "duplicate-processing-order", "placements.processingOrder");
                var deployment = new BattleDeploymentDefinition(actor, faction, order, control, aiStrategy, position);
                BattleTurnFlow.ValidateDeployment(deployment);
                placements.Add(deployment);
            }
            Require(placements.Count(p => p.Faction == BattleFaction.Ally) <= 30 &&
                placements.Count(p => p.Faction == BattleFaction.Enemy) <= 32, "faction-capacity", "encounters.placements");
            Require(encounters.TryAdd(id, new(id, new MapId(map), terrain.Width, terrain.Height, terrain.Cells, placements, spells.Values, rewards, healingItems: healingItems.Values)),
                "duplicate-encounter", "encounters.id");
        }
        Require(encounters.ContainsKey(selectedEncounter), "missing-encounter", "start.encounter");
        var startActors = new List<BattleActorStartInput>();
        foreach (var input in Array(start, "actors"))
        {
            Object(input, "start.actor", "actor", "hp", "mp", "exp", "kills", "defeats", "status", "positionOverride");
            MapPosition? position = null;
            var overrideInput = input.GetProperty("positionOverride");
            if (overrideInput.ValueKind != JsonValueKind.Null)
            {
                Object(overrideInput, "start.actor.positionOverride", "x", "y");
                position = new(Number(overrideInput, "x", 0, 47), Number(overrideInput, "y", 0, 47));
            }
            startActors.Add(new(new ActorRef(Id(input, "actor")), (ushort)Number(input, "hp", 0, 65535),
                (byte)Number(input, "mp", 0, 255), (byte)Number(input, "exp", 0, 99),
                (ushort)Number(input, "kills", 0, 9999), (ushort)Number(input, "defeats", 0, 9999), StartStatus(input), position));
        }
        var startInput = new BattleStartInput(selectedEncounter, startActors, mainSeed, thinkingSeed,
            (uint)Number(start, "gold", 0, 9999999));
        BattleTurnFlow.ValidateStart(encounters[selectedEncounter], startInput);
        return new(new ScenarioDefinition(package, encounters.Values), startInput);
    }

    private static ushort StartStatus(JsonElement input)
    { Require(Text(input, "status") == "none", "actor-status", "start.actors.status", true); return 0; }

}

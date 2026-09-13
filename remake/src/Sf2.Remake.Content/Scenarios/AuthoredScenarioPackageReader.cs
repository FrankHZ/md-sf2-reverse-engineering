using System.Text.Json;
using System.Text.RegularExpressions;
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
            return new ScenarioReadAccepted(Decode(document.RootElement));
        }
        catch (AdmissionIssue issue) { return new ScenarioReadRejected(issue.Failure); }
        catch (JsonException) { return Rejected("json-syntax", "document"); }
        catch (IOException) { return Rejected("content-read", "document"); }
        catch (UnauthorizedAccessException) { return Rejected("content-read", "document"); }
    }

    private static ScenarioDefinition Decode(JsonElement root)
    {
        Object(root, "document", "formatVersion", "package", "profile", "ruleProfile", "start", "terrains", "maps", "spells", "actors", "encounters");
        Require(Number(root, "formatVersion", 1, 1) == 1, "format-version", "formatVersion");
        Require(Text(root, "profile") == "public-authored", "profile", "profile");
        Require(Text(root, "ruleProfile") == "sf2-semantic-subset-v1", "rule-profile", "ruleProfile", true);
        string package = Id(root, "package");
        var start = root.GetProperty("start");
        Object(start, "start", "encounter", "initialVitals", "mainSeed", "thinkingSeed");
        Require(Text(start, "initialVitals") == "authored-controlled", "initial-vitals-policy", "start.initialVitals");
        string selectedEncounter = Id(start, "encounter");
        uint mainSeed = WordImage(start, "mainSeed"), thinkingSeed = WordImage(start, "thinkingSeed");

        var terrains = new Dictionary<string, (int Width, int Height, byte[] Cells)>(StringComparer.Ordinal);
        foreach (var terrain in Array(root, "terrains"))
        {
            Object(terrain, "terrain", "id", "rows");
            string id = Id(terrain, "id");
            var rows = Array(terrain, "rows").Select(row =>
            {
                Require(row.ValueKind == JsonValueKind.String, "terrain-row", "terrain.rows");
                return row.GetString()!;
            }).ToArray();
            Require(rows.Length is >= 1 and <= 48, "terrain-height", "terrain.rows");
            int width = rows[0].Length;
            Require(width is >= 1 and <= 48 && rows.All(row => row.Length == width), "terrain-width", "terrain.rows");
            var cells = Enumerable.Repeat((byte)255, 2304).ToArray();
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < width; x++)
                {
                    char entry = rows[y][x];
                    Require(entry == '#' || entry is >= '0' and <= '8', "terrain-entry", "terrain.rows");
                    cells[y * 48 + x] = entry == '#' ? (byte)255 : (byte)(entry - '0');
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
        var actors = new Dictionary<ActorRef, (BattleActorDefinition Definition, ushort Hp, byte Mp, byte Exp)>();
        foreach (var actor in Array(root, "actors"))
        {
            ObjectOptional(actor, "actor", "physical", "id", "slot", "classRule", "controller", "level", "maxHp", "hp", "maxMp", "mp",
                "attack", "defense", "agility", "move", "exp", "status", "items", "spells");
            var id = new ActorRef(Id(actor, "id"));
            int slot = Number(actor, "slot", 0, 159);
            Require(slot <= 29 || slot >= 128, "actor-slot", "actors.slot");
            string className = Text(actor, "classRule"), controlName = Text(actor, "controller");
            Require(className is "unpromoted-priest" or "ordinary", "class-rule", "actors.classRule", true);
            Require(controlName is "player" or "stay", "ai-commandset", "actors.controller", true);
            Require((slot < 128) == (controlName == "player"), "controller-side", "actors.controller", true);
            Require(Text(actor, "status") == "none", "actor-status", "actors.status", true);
            Require(!Array(actor, "items").Any(), "actor-items", "actors.items", true);
            PhysicalActorDefinition? physical = null;
            if (actor.TryGetProperty("physical", out var physicalInput))
            {
                Object(physicalInput, "actor.physical", "movementType", "prowess", "promoted", "leader", "gold", "kills", "special");
                Require(Text(physicalInput, "movementType") == "regular", "physical-movement-type", "actors.physical.movementType", true);
                Require(Text(physicalInput, "special") == "none", "physical-special-rule", "actors.physical.special", true);
                byte prowess = (byte)Number(physicalInput, "prowess", 0, 255);
                Require(prowess is 0 or 3, "physical-prowess", "actors.physical.prowess", true);
                bool promoted = Boolean(physicalInput, "promoted");
                Require(className != "unpromoted-priest" || !promoted, "class-promotion", "actors.physical.promoted");
                physical = new(prowess, promoted, Boolean(physicalInput, "leader"),
                    (ushort)Number(physicalInput, "gold", 0, 65535), (ushort)Number(physicalInput, "kills", 0, 9999));
            }
            ushort maximumHp = (ushort)Number(actor, "maxHp", 1, 65535);
            ushort hp = (ushort)Number(actor, "hp", 0, maximumHp);
            byte maximumMp = (byte)Number(actor, "maxMp", 0, 255), mp = (byte)Number(actor, "mp", 0, maximumMp);
            byte exp = (byte)Number(actor, "exp", 0, 99);
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
            var definition = new BattleActorDefinition(id, (byte)slot,
                className == "unpromoted-priest" ? BattleClassRule.UnpromotedPriest : BattleClassRule.Ordinary,
                controlName == "player" ? BattleController.Player : BattleController.Stay,
                (byte)Number(actor, "level", slot < 128 ? 1 : 0, 99), maximumHp, maximumMp,
                (byte)Number(actor, "attack", 0, 255), (byte)Number(actor, "defense", 0, 255),
                (byte)Number(actor, "agility", 0, 255), (byte)Number(actor, "move", 1, 255), learned, physical);
            Require(actors.TryAdd(id, (definition, hp, mp, exp)), "duplicate-actor", "actors.id");
        }
        var encounters = new Dictionary<string, BattleDefinition>(StringComparer.Ordinal);
        foreach (var encounter in Array(root, "encounters"))
        {
            ObjectOptional(encounter, "encounter", "rewards", "id", "map", "placements");
            BattleRewardDefinition? rewards = null;
            if (encounter.TryGetProperty("rewards", out var rewardInput))
            {
                Object(rewardInput, "encounter.rewards", "halvedExperience", "initialGold");
                rewards = new(Boolean(rewardInput, "halvedExperience"), (uint)Number(rewardInput, "initialGold", 0, 9999999));
            }
            string id = Id(encounter, "id"), map = Id(encounter, "map");
            Require(maps.ContainsKey(map), "missing-map", "encounters.map");
            var terrain = terrains[maps[map]];
            var placements = new List<BattleActorState>();
            foreach (var placement in Array(encounter, "placements"))
            {
                Object(placement, "placement", "actor", "x", "y");
                var actorRef = new ActorRef(Id(placement, "actor"));
                Require(actors.ContainsKey(actorRef), "missing-actor", "placements.actor");
                var actor = actors[actorRef];
                var position = new MapPosition(Number(placement, "x", 0, terrain.Width - 1), Number(placement, "y", 0, terrain.Height - 1));
                byte tile = terrain.Cells[position.Y * 48 + position.X];
                Require(tile < 16 && WeightedMovement.OrdinaryCosts[tile] > 0, "blocked-placement", "placements");
                Require(!placements.Any(a => a.Actor == actorRef || a.Definition.Slot == actor.Definition.Slot), "duplicate-placement", "placements.actor/slot");
                Require(actor.Hp == 0 || !placements.Any(a => a.Hp > 0 && a.Position == position), "occupied-placement", "placements");
                placements.Add(new(actor.Definition, actor.Hp, actor.Mp, actor.Exp, position));
            }
            Require(placements.Any(a => a.Hp > 0 && a.Definition.IsAlly) && placements.Any(a => a.Hp > 0 && !a.Definition.IsAlly),
                "battle-outcome", "encounters.placements", true);
            Require(!placements.Any(a => a.Hp == 0 && a.Definition.Physical?.Leader == true),
                "leader-defeat-program", "encounters.placements", true);
            Require(placements.Where(a => a.Hp > 0).Sum(a => a.Definition.Agility >= 128 ? 2 : 1) <= 64,
                "turn-buffer-capacity", "encounters.placements");
            Require(encounters.TryAdd(id, new(id, new MapId(map), terrain.Width, terrain.Height, terrain.Cells, placements, spells.Values, rewards)),
                "duplicate-encounter", "encounters.id");
        }
        Require(encounters.ContainsKey(selectedEncounter), "missing-encounter", "start.encounter");
        return new(package, encounters[selectedEncounter], mainSeed, thinkingSeed);
    }

    private static bool Boolean(JsonElement element, string key)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind is JsonValueKind.True or JsonValueKind.False, "boolean", key);
        return value.GetBoolean();
    }
    private static void ObjectOptional(JsonElement element, string field, string optional, params string[] keys)
    {
        Require(element.ValueKind == JsonValueKind.Object, "object-required", field);
        Object(element, field, element.TryGetProperty(optional, out _) ? [.. keys, optional] : keys);
    }
    private static void Object(JsonElement element, string field, params string[] keys)
    {
        Require(element.ValueKind == JsonValueKind.Object, "object-required", field);
        var found = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
            Require(keys.Contains(property.Name) && found.Add(property.Name), "unknown-or-duplicate-field", field);
        Require(found.Count == keys.Length, "missing-field", field);
    }
    private static IEnumerable<JsonElement> Array(JsonElement element, string key)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind == JsonValueKind.Array, "array-required", key);
        return value.EnumerateArray();
    }
    private static string Text(JsonElement element, string key)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind == JsonValueKind.String, "string-required", key);
        return value.GetString()!;
    }
    private static string Id(JsonElement element, string key)
    {
        string value = Text(element, key);
        Require(Regex.IsMatch(value, "^[a-z][a-z0-9-]{0,63}$", RegexOptions.CultureInvariant), "identifier", key);
        return value;
    }
    private static int Number(JsonElement element, string key, int minimum, int maximum)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _), "integer-required", key);
        int result = value.GetInt32();
        Require(result >= minimum && result <= maximum, "numeric-range", key);
        return result;
    }
    private static uint WordImage(JsonElement element, string key)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind == JsonValueKind.Number && value.TryGetUInt32(out _), "rng-image", key);
        return value.GetUInt32();
    }
    private static void Require(bool condition, string code, string field, bool unsupported = false)
    {
        if (!condition) throw new AdmissionIssue(new(unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
            code, field, code.Replace('-', ' ')));
    }
    private static ScenarioReadRejected Rejected(string code, string field) =>
        new(new(SessionFailureKind.ContentError, code, field, code.Replace('-', ' ')));
    private sealed class AdmissionIssue(SessionFailure failure) : Exception(failure.Code)
    { internal SessionFailure Failure { get; } = failure; }
}

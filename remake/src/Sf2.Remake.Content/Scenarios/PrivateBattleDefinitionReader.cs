using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class PrivateBattleDefinitionReader
{
    // Existing manifests/extractions/static-data.json and enemy-promotion-data.json own these pins.
    internal const string StaticDigest = "BCEB7EE6EC4AEF592A27EC1053CFBE547D45D8171CE6510343C80CC94D04235A";
    internal const string EnemyDigest = "8E77363833F31861DB23765E5BEE7263EA2BD2D09F5F4DA869C302A83C25BE06";

    internal static PrivateBattleDefinitions Read(string staticPath, string enemyPath, string goldPath,
        BattleEncounterDefinition encounter, ControlledBattleStart start)
    {
        var coreBytes = PrivateBattleEncounterReader.ReadInput(staticPath, StaticDigest, "static-data", maximumLength: 1048576);
        var enemyBytes = PrivateBattleEncounterReader.ReadInput(enemyPath, EnemyDigest, "enemy-data", maximumLength: 1048576);
        using var core = JsonDocument.Parse(coreBytes, new JsonDocumentOptions { MaxDepth = 32 });
        using var enemy = JsonDocument.Parse(enemyBytes, new JsonDocumentOptions { MaxDepth = 32 });
        using var gold = JsonDocument.Parse(PrivateBattleEncounterReader.ReadInput(goldPath,
            "16607CA1AE0477BC22FE81BEA044B7711D8C1EA3A31F4A0064C0288C026AD3DF", "enemy-gold", maximumLength: 16384));
        return Decode(core.RootElement, enemy.RootElement, gold.RootElement, encounter, start);
    }

    internal static PrivateBattleDefinitions Decode(JsonElement core, JsonElement enemy, JsonElement gold,
        BattleEncounterDefinition encounter, ControlledBattleStart start)
    {
        Object(core, "static-data", "schemaVersion", "provenance", "romRangeConvention", "romRanges", "allies", "classes", "items", "spellNames", "spellDefinitions");
        Object(enemy, "enemy-data", "schemaVersion", "provenance", "romRangeConvention", "romRanges", "promotions", "enemies");
        _ = Number(core, "schemaVersion", 1, 1); _ = Number(enemy, "schemaVersion", 1, 1);
        var sources = Sources(core).Concat(Sources(enemy)).Distinct().ToArray();
        var classes = start.Allies.Select(row => row.ClassId).Distinct().Select(id => Class(FindId(core, "classes", id))).ToArray();
        var enemies = encounter.Placements.Where(row => row.Kind == EncounterEntityKind.Enemy)
            .Select(row => row.IdentityExpression).Distinct().Select(code => Enemy(FindCode(enemy, "enemies", code))).ToArray();
        var itemIds = start.Allies.SelectMany(row => row.Items).Select(word => (byte)(word & 127)).ToHashSet();
        foreach (var row in enemies.SelectMany(row => row.Items)) itemIds.Add((byte)Number(FindCode(core, "items", row.Item), "id", 0, 127));
        foreach (var row in encounter.Placements) itemIds.Add((byte)Number(FindCode(core, "items", row.ItemExpression), "id", 0, 127));
        var items = itemIds.Order().Select(id => Item(FindId(core, "items", id))).ToArray();
        var spellKeys = new HashSet<(byte Id, byte Level)>();
        foreach (byte packed in start.Allies.SelectMany(row => row.Spells))
            if ((packed & 63) != 63)
                for (byte level = 1; level <= (packed >> 6) + 1; level++) spellKeys.Add(((byte)(packed & 63), level));
        foreach (var row in enemies.SelectMany(row => row.Spells)) AddSpell(row.Spell, row.Level);
        foreach (var row in items) AddSpell(row.UseSpell, row.UseSpellLevel);
        var spells = spellKeys.OrderBy(key => key.Id).ThenBy(key => key.Level).Select(key =>
        {
            string code = Text(FindId(core, "spellNames", key.Id), "code");
            var rows = Array(core, "spellDefinitions").Where(row => Text(row, "spell") == code && Number(row, "level", 1, 4) == key.Level).ToArray();
            Require(rows.Length == 1, "missing-or-duplicate-spell-definition", "spellDefinitions");
            return Spell(rows[0], key.Id, code);
        }).ToArray();
        // The pinned export distinguishes used enemy IDs from its unused ROM tail.
        Require(Text(gold, "upstreamCommit") == PrivateBattleEncounterReader.UpstreamCommit, "source-provenance", "enemy-gold");
        var rewards = Array(gold, "usedGold").Select(value => value.GetUInt16()).ToArray();
        Require(rewards.Length == 103, "enemy-gold-count", "enemy-gold");
        var selectedGold = enemies.Select(row => new SourceEnemyGold(row.Id, rewards[row.Id],
            Text(gold, "upstreamCommit"), Text(gold, "romSha256"), Text(gold, "sourcePath"),
            Number(gold.GetProperty("romRange"), "start", 0, int.MaxValue) + row.Id * 2));
        return new(encounter, sources, classes, items, spells, enemies, selectedGold);

        void AddSpell(string code, byte level)
        {
            if (code == "NOTHING") return;
            spellKeys.Add(((byte)Number(FindCode(core, "spellNames", code), "id", 0, 43), level));
        }
    }

    private static IEnumerable<EncounterSource> Sources(JsonElement document)
    {
        var source = document.GetProperty("provenance");
        Object(source, "provenance", "repository", "commit", "sources");
        Require(Text(source, "repository") == PrivateBattleEncounterReader.Repository &&
            Text(source, "commit") == PrivateBattleEncounterReader.UpstreamCommit, "source-provenance", "provenance");
        return Array(source, "sources").Select(row =>
        {
            Object(row, "source", "path", "sha256");
            return new EncounterSource(Text(source, "repository"), Text(source, "commit"), Text(row, "path"), Text(row, "sha256"));
        }).ToArray();
    }
    private static JsonElement FindId(JsonElement root, string table, byte id)
    {
        var rows = Array(root, table).Where(row => Number(row, "id", 0, 255) == id).ToArray();
        Require(rows.Length == 1, "missing-or-duplicate-definition", table); return rows[0];
    }
    private static JsonElement FindCode(JsonElement root, string table, string code)
    {
        var rows = Array(root, table).Where(row => Text(row, "code") == code).ToArray();
        Require(rows.Length == 1, "missing-or-duplicate-definition", table); return rows[0];
    }
    private static SourceClassDefinition Class(JsonElement row)
    {
        Object(row, "class", "id", "code", "nameExpression", "movement", "resistanceExpression", "resistanceTokens", "movementType", "prowessExpression", "prowessTokens");
        return new(B(row, "id"), Text(row, "code"), Text(row, "nameExpression"), B(row, "movement"),
            Text(row, "resistanceExpression"), Text(row, "movementType"), Text(row, "prowessExpression"));
    }
    private static SourceItemDefinition Item(JsonElement row)
    {
        Object(row, "item", "id", "code", "displayName", "nameExpression", "equipFlagsExpression", "equipFlagTokens", "range", "price", "itemTypeExpression", "itemTypeTokens", "useSpellExpression", "useSpell", "useSpellLevel", "equipEffects");
        var range = row.GetProperty("range"); Object(range, "item.range", "min", "max");
        var effects = Array(row, "equipEffects").Select(effect =>
        { Object(effect, "effect", "type", "parameter"); return new SourceEquipEffect(Text(effect, "type"), Number(effect, "parameter", -65535, 65535)); }).ToArray();
        Require(effects.Length == 3, "equip-effect-count", "item.equipEffects");
        return new(B(row, "id"), Text(row, "code"), Text(row, "displayName"), Text(row, "nameExpression"), Text(row, "equipFlagsExpression"),
            Strings(row, "equipFlagTokens"), B(range, "min"), B(range, "max"), (ushort)Number(row, "price", 0, 65535),
            Text(row, "itemTypeExpression"), Text(row, "useSpellExpression"), Text(row, "useSpell"), (byte)Number(row, "useSpellLevel", 1, 4), System.Array.AsReadOnly(effects));
    }
    private static SourceSpellDefinition Spell(JsonElement row, byte baseId, string code)
    {
        Object(row, "spell-definition", "id", "displayName", "entryExpression", "spell", "level", "mpCost", "animationExpression", "animationTokens", "propertiesExpression", "propertyTokens", "range", "radius", "power");
        var range = row.GetProperty("range"); Object(range, "spell.range", "min", "max");
        return new(B(row, "id"), baseId, code, Text(row, "displayName"), Text(row, "entryExpression"), (byte)Number(row, "level", 1, 4),
            B(row, "mpCost"), Text(row, "animationExpression"), Text(row, "propertiesExpression"), B(range, "min"), B(range, "max"),
            B(row, "radius"), (ushort)Number(row, "power", 0, 65535));
    }
    private static SourceEnemyDefinition Enemy(JsonElement row)
    {
        Object(row, "enemy", "id", "code", "displayName", "nameExpression", "nameEncodedLength", "nameSuffixBytes", "unknownByte", "spellPower", "level", "maxHp", "maxMp", "baseAttack", "baseDefense", "baseAgility", "baseMovement", "resistanceExpression", "resistanceTokens", "prowessExpression", "prowessTokens", "items", "spells", "initialStatusExpression", "movementType", "aiBitfieldExpression", "aiBitfieldTokens");
        var items = Array(row, "items").Select(item =>
        { Object(item, "enemy.item", "expression", "item", "equipped"); return new SourceEnemyItem(Text(item, "expression"), Text(item, "item"), Boolean(item, "equipped")); }).ToArray();
        var spells = Array(row, "spells").Select(spell =>
        { Object(spell, "enemy.spell", "expression", "spell", "level"); return new SourceEnemySpell(Text(spell, "expression"), Text(spell, "spell"), (byte)Number(spell, "level", 1, 4)); }).ToArray();
        Require(items.Length == 4 && spells.Length == 4, "source-slot-count", "enemy");
        var suffix = Array(row, "nameSuffixBytes").Select(value => value.GetByte()).ToArray();
        return new(B(row, "id"), Text(row, "code"), Text(row, "displayName"), Text(row, "nameExpression"), B(row, "nameEncodedLength"),
            System.Array.AsReadOnly(suffix), B(row, "unknownByte"), Text(row, "spellPower"), B(row, "level"), (ushort)Number(row, "maxHp", 1, 65535),
            B(row, "maxMp"), B(row, "baseAttack"), B(row, "baseDefense"), B(row, "baseAgility"), B(row, "baseMovement"),
            Text(row, "resistanceExpression"), Text(row, "prowessExpression"), System.Array.AsReadOnly(items), System.Array.AsReadOnly(spells),
            Text(row, "initialStatusExpression"), Text(row, "movementType"), Text(row, "aiBitfieldExpression"));
    }
    private static byte B(JsonElement row, string key) => (byte)Number(row, key, 0, 255);
    private static IReadOnlyList<string> Strings(JsonElement row, string key) => System.Array.AsReadOnly(Array(row, key).Select(value =>
    { Require(value.ValueKind == JsonValueKind.String, "string-required", key); return value.GetString()!; }).ToArray());
}

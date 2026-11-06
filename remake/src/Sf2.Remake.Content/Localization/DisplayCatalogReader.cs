using System.Text;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.Content.Localization;

public sealed record DisplayCatalogAdmission(DisplayCatalog? Catalog, string? Error);
public static class DisplayCatalogReader
{
    public static DisplayCatalogAdmission Read(byte[] bytes, ScenarioDefinition source)
        => Read(bytes, source.Package, "world.text", source.Exploration?.TextTokens,
            source.Exploration?.Provenance?.Repository, source.Exploration?.Provenance?.Commit,
            source.Exploration?.Provenance?.RomSha256, source);

    // Entry has its own admitted profile and source identity; it is not a gameplay scenario.
    public static DisplayCatalogAdmission Read(byte[] bytes, EntryPresentation source)
        => Read(bytes, source.Profile, "entry.text", source.Dialogue.ToDictionary(pair => int.Parse(pair.Key,
            System.Globalization.CultureInfo.InvariantCulture), pair => ExplorationTextTokens.Read(pair.Value)),
            "ShiningForceCentral/SF2DISASM", source.UpstreamCommit, source.RomSha256);

    private static DisplayCatalogAdmission Read(byte[] bytes, string package, string targetNamespace,
        IReadOnlyDictionary<int, IReadOnlyList<ExplorationTextToken>>? originals,
        string? repository, string? commit, string? romSha256, ScenarioDefinition? definition = null)
    {
        try
        {
            if (bytes.Length > 256 * 1024) throw new ArgumentException();
            string json = new UTF8Encoding(false, true).GetString(bytes);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            bool hasNames = targetNamespace == "world.text" && root.TryGetProperty("names", out _);
            string[] envelope = repository is null ? ["package", "locale", "texts"] : ["package", "locale", "texts", "source"];
            Shape(root, hasNames ? [.. envelope, "names"] : envelope);
            if (root.GetProperty("package").GetString() != package ||
                root.GetProperty("locale").GetString() != "zh-CN" || originals is null)
                throw new ArgumentException();
            if (repository is not null)
            {
                var pin = root.GetProperty("source"); Shape(pin, ["repository", "commit", "romSha256"]);
                if (pin.GetProperty("repository").GetString() != repository ||
                    pin.GetProperty("commit").GetString() != commit ||
                    pin.GetProperty("romSha256").GetString() != romSha256) throw new ArgumentException();
            }
            var texts = root.GetProperty("texts");
            if (texts.ValueKind != JsonValueKind.Array || texts.GetArrayLength() > (targetNamespace == "world.text" ? 1024 : 256)) throw new ArgumentException();
            var seen = new HashSet<(string, int)>();
            var rows = new Dictionary<int, DisplayCatalog.Row>(); var rejected = new Dictionary<int, string>();
            var diagnostics = new List<DisplayDiagnostic>();
            foreach (var entry in texts.EnumerateArray())
            {
                Shape(entry, ["namespace", "id", "text"]);
                string space = entry.GetProperty("namespace").GetString() ?? throw new ArgumentException();
                int id = entry.GetProperty("id").GetInt32();
                string value = entry.GetProperty("text").GetString() ?? throw new ArgumentException();
                if (!seen.Add((space, id))) throw new ArgumentException();
                string key = space == targetNamespace && id is >= 0 and <= 65535 ? $"{targetNamespace}/{id}" : "unknown-target";
                if (space != targetNamespace)
                { diagnostics.Add(new("invalid-translation", key)); continue; }
                if (!originals.TryGetValue(id, out var original) || value.Length > 8192 ||
                    !Map(original, ExplorationTextTokens.Read(value), out var row))
                {
                    rejected[id] = "invalid-translation"; diagnostics.Add(new("invalid-translation", key)); continue;
                }
                rows.Add(id, row!);
            }
            var names = new Dictionary<(string, int), string>();
            if (hasNames)
            {
                var entries = root.GetProperty("names");
                if (entries.ValueKind != JsonValueKind.Array || entries.GetArrayLength() > 128) throw new ArgumentException();
                foreach (var entry in entries.EnumerateArray())
                {
                    Shape(entry, ["namespace", "index", "text"]);
                    string space = entry.GetProperty("namespace").GetString() ?? throw new ArgumentException();
                    int index = entry.GetProperty("index").GetInt32();
                    string value = entry.GetProperty("text").GetString() ?? throw new ArgumentException();
                    if (!KnownName(definition!, space, index) || string.IsNullOrWhiteSpace(value) || value.Length > 128 ||
                        value.Any(c => char.IsControl(c) || c is '{' or '}' or '\u2028' or '\u2029') || !names.TryAdd((space, index), value))
                        throw new ArgumentException();
                }
            }
            return new(new("zh-CN", rows, rejected, diagnostics, names), null);
        }
        catch (Exception error) when (error is JsonException or ArgumentException or InvalidOperationException or
            KeyNotFoundException or DecoderFallbackException or OverflowException)
        { return new(null, "overlay-unavailable"); }
    }
    private static bool KnownName(ScenarioDefinition source, string space, int index) => index >= 0 && (space switch
    {
        "name.ally" => index < (source.Exploration?.MemberNames.Count ?? 0),
        "name.class" => source.PrivateDefinitions?.Classes.Keys.Any(id => id == index) == true,
        "name.item" => source.PrivateDefinitions?.Items.Keys.Any(id => id == index) == true ||
            source.Exploration?.Programs.Values.SelectMany(program => program.Instructions).OfType<OpenShop>()
                .Any(shop => shop.Definition.Items.Keys.Any(id => id == index)) == true ||
            source.Exploration?.Maps.Values.SelectMany(map => new[] { map.Search }.Concat(map.Setups.Values.Select(setup => setup.Search)))
                .Where(search => search is not null).SelectMany(search => search!.Chests.Concat(search.OtherItems))
                .Any(item => item.ItemWord is { } word && (word & 127) == index) == true,
        "name.spell" => source.PrivateDefinitions?.Spells.Values.Any(spell => spell.BaseId == index) == true,
        "name.enemy" => source.PrivateDefinitions?.Enemies.Values.Any(enemy => enemy.Id == index) == true,
        _ => false,
    });
    private static void Shape(JsonElement value, string[] names)
    {
        if (value.ValueKind != JsonValueKind.Object) throw new ArgumentException();
        var actual = value.EnumerateObject().Select(property => property.Name).ToArray();
        if (actual.Length != names.Length || actual.Distinct(StringComparer.Ordinal).Count() != actual.Length ||
            actual.Any(name => !names.Contains(name, StringComparer.Ordinal))) throw new ArgumentException();
    }
    private static bool Control(ExplorationTextToken token) => token.Kind is ExplorationTextTokenKind.Wait1 or
        ExplorationTextTokenKind.Wait2 or ExplorationTextTokenKind.Delay1 or ExplorationTextTokenKind.Clear or ExplorationTextTokenKind.Newline;
    private static bool Name(ExplorationTextToken token) => token.Kind is ExplorationTextTokenKind.MemberName or ExplorationTextTokenKind.ClassName;
    private static bool Map(IReadOnlyList<ExplorationTextToken> source, IReadOnlyList<ExplorationTextToken> target, out DisplayCatalog.Row? row)
    {
        row = null;
        if (source.Concat(target).Any(token => token.Kind == ExplorationTextTokenKind.Unsupported)) return false;
        var controls = source.Select((token, index) => (token, index)).Where(pair => Control(pair.token)).ToArray();
        var translated = target.Select((token, index) => (token, index)).Where(pair => Control(pair.token)).ToArray();
        if (!controls.Select(pair => pair.token.Kind).SequenceEqual(translated.Select(pair => pair.token.Kind))) return false;
        var mapping = new int[target.Count]; int from = 0, to = 0;
        for (int segment = 0; segment <= controls.Length; segment++)
        {
            int end = segment < controls.Length ? controls[segment].index : source.Count;
            int translatedEnd = segment < translated.Length ? translated[segment].index : target.Count;
            var names = Enumerable.Range(from, end - from).Where(index => Name(source[index])).ToList();
            for (int i = to; i < translatedEnd; i++)
            {
                if (Name(target[i]))
                {
                    int match = names.FindIndex(index => source[index] == target[i]);
                    if (match < 0) return false;
                    mapping[i] = names[match]; names.RemoveAt(match);
                }
                else mapping[i] = from;
            }
            if (names.Count != 0) return false;
            if (segment < controls.Length) mapping[translatedEnd] = end;
            from = end + 1; to = translatedEnd + 1;
        }
        row = new(Array.AsReadOnly(target.ToArray()), Array.AsReadOnly(mapping)); return true;
    }
}

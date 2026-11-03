using System.Text;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.Content.Localization;

public sealed record DisplayCatalogAdmission(DisplayCatalog? Catalog, string? Error);
public static class DisplayCatalogReader
{
    public static DisplayCatalogAdmission Read(byte[] bytes, ScenarioDefinition source)
    {
        try
        {
            if (bytes.Length > 256 * 1024) throw new ArgumentException();
            string json = new UTF8Encoding(false, true).GetString(bytes);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            Shape(root, source.Exploration?.Provenance is null ? ["package", "locale", "texts"] :
                ["package", "locale", "texts", "source"]);
            if (root.GetProperty("package").GetString() != source.Package ||
                root.GetProperty("locale").GetString() != "zh-CN" || source.Exploration is not { } world)
                throw new ArgumentException();
            if (world.Provenance is { } identity)
            {
                var pin = root.GetProperty("source"); Shape(pin, ["repository", "commit", "romSha256"]);
                if (pin.GetProperty("repository").GetString() != identity.Repository ||
                    pin.GetProperty("commit").GetString() != identity.Commit ||
                    pin.GetProperty("romSha256").GetString() != identity.RomSha256) throw new ArgumentException();
            }
            var texts = root.GetProperty("texts");
            if (texts.ValueKind != JsonValueKind.Array || texts.GetArrayLength() > 64) throw new ArgumentException();
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
                string key = space == "world.text" && id is >= 0 and <= 65535 ? $"world.text/{id}" : "unknown-target";
                if (space != "world.text" || !world.TextTokens.TryGetValue(id, out var original) || value.Length > 8192 ||
                    !Map(original, ExplorationTextTokens.Read(value), out var row))
                {
                    rejected[id] = "invalid-translation"; diagnostics.Add(new("invalid-translation", key)); continue;
                }
                rows.Add(id, row!);
            }
            return new(new("zh-CN", rows, rejected, diagnostics), null);
        }
        catch (Exception error) when (error is JsonException or ArgumentException or InvalidOperationException or
            KeyNotFoundException or DecoderFallbackException or OverflowException)
        { return new(null, "overlay-unavailable"); }
    }
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

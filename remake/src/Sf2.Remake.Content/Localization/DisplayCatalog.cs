using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.Content.Localization;

public sealed record DisplayDiagnostic(string Code, string Key);
public sealed record DisplayResult(string Text, string EffectiveLocale, string? Reason);

// Admitted rows carry their complete source mapping. A rejected row never contributes a map.
public sealed class DisplayCatalog
{
    internal sealed record Row(IReadOnlyList<ExplorationTextToken> Tokens, IReadOnlyList<int> SourceTokens);
    private readonly IReadOnlyDictionary<int, Row> _rows;
    private readonly IReadOnlyDictionary<int, string> _rejected;
    public string Locale { get; }
    public IReadOnlyList<DisplayDiagnostic> Diagnostics { get; }
    internal DisplayCatalog(string locale, IDictionary<int, Row> rows, IDictionary<int, string> rejected,
        IEnumerable<DisplayDiagnostic> diagnostics)
    {
        Locale = locale; _rows = new ReadOnlyDictionary<int, Row>(new Dictionary<int, Row>(rows));
        _rejected = new ReadOnlyDictionary<int, string>(new Dictionary<int, string>(rejected));
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }
    private static readonly DisplayCatalog English = new("en", new Dictionary<int, Row>(), new Dictionary<int, string>(), []);
    public DisplayResult ResolveEntry(int text, EntryPresentation source) =>
        source.Dialogue.TryGetValue(text.ToString(System.Globalization.CultureInfo.InvariantCulture), out var original)
            ? ResolveRange(text, ExplorationTextTokens.Read(original), [], 0, int.MaxValue)
            : new("Text unavailable (display-operands).", "en", "display-operands");
    public static DisplayResult Canonical(FieldTextWait wait, ExplorationDefinition source, bool reflow = false)
    {
        var result = English.Resolve(wait, source, reflow);
        return result.Reason == "missing-translation" ? result with { Reason = null } : result;
    }
    public DisplayResult Resolve(FieldTextWait wait, ExplorationDefinition source, bool reflow = false)
    {
        if (!source.TextTokens.TryGetValue(wait.Text, out var canonical) || wait.DisplayNames is null || wait.SourceTokenCount != canonical.Count ||
            wait.DisplayStart < 0 || wait.DisplayStart > wait.DisplayEnd || wait.DisplayEnd > canonical.Count)
            return new("Text unavailable (display-operands).", "en", "display-operands");
        return ResolveRange(wait.Text, canonical, wait.DisplayNames, wait.DisplayStart, wait.DisplayEnd, reflow);
    }
    // Search reads complete admitted rows; it does not run a second text interpreter or wait.
    public static DisplayResult Canonical(int text, ExplorationDefinition source, IReadOnlyList<DisplayNameBinding> names, bool reflow = false)
    {
        var result = English.Resolve(text, source, names, reflow);
        return result.Reason == "missing-translation" ? result with { Reason = null } : result;
    }
    public DisplayResult Resolve(int text, ExplorationDefinition source, IReadOnlyList<DisplayNameBinding> names, bool reflow = false) =>
        source.TextTokens.TryGetValue(text, out var tokens)
            ? ResolveRange(text, tokens, names, 0, tokens.Count, reflow)
            : new("Text unavailable (display-operands).", "en", "display-operands");
    private DisplayResult ResolveRange(int text, IReadOnlyList<ExplorationTextToken> canonical,
        IReadOnlyList<DisplayNameBinding> names, int start, int end, bool reflow = false)
    {
        bool translated = _rows.TryGetValue(text, out var row);
        var tokens = translated ? row!.Tokens : canonical;
        DisplayNameBinding? Binding(int index) => names.FirstOrDefault(name =>
            name.SourceToken == (translated ? row!.SourceTokens[index] : index));
        string? Boundary(int index) => index < 0 || index >= tokens.Count ? null :
            tokens[index].Kind == ExplorationTextTokenKind.Literal ? tokens[index].Value :
            Binding(index) is { } name ? name.CustomName ?? name.CanonicalName : null;
        var output = new System.Text.StringBuilder();
        for (int i = 0; i < tokens.Count; i++)
        {
            int original = translated ? row!.SourceTokens[i] : i;
            if (original < start || original >= end) continue;
            var token = tokens[i];
            if (token.Kind == ExplorationTextTokenKind.Literal) output.Append(token.Value);
            else if (token.Kind == ExplorationTextTokenKind.Newline) output.Append(reflow ?
                SourceTextLayout.Break(tokens, i, translated ? Locale : "en", Boundary(i - 1), Boundary(i + 1)) : "\n");
            else if (token.Kind is ExplorationTextTokenKind.MemberName or ExplorationTextTokenKind.ClassName)
            {
                var binding = Binding(i);
                if (binding is null) return new("Text unavailable (display-operands).", "en", "display-operands");
                output.Append(binding.CustomName ?? binding.CanonicalName);
            }
        }
        return new(output.ToString(), translated ? Locale : "en", translated ? null :
            _rejected.ContainsKey(text) ? "invalid-translation" : "missing-translation");
    }
}

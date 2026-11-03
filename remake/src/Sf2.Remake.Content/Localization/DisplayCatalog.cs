using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;

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
    public static DisplayResult Canonical(FieldTextWait wait, ExplorationDefinition source)
    {
        var result = English.Resolve(wait, source);
        return result.Reason == "missing-translation" ? result with { Reason = null } : result;
    }
    public DisplayResult Resolve(FieldTextWait wait, ExplorationDefinition source)
    {
        if (!source.TextTokens.TryGetValue(wait.Text, out var canonical) || wait.DisplayNames is null || wait.SourceTokenCount != canonical.Count ||
            wait.DisplayStart < 0 || wait.DisplayStart > wait.DisplayEnd || wait.DisplayEnd > canonical.Count)
            return new("Text unavailable (display-operands).", "en", "display-operands");
        bool translated = _rows.TryGetValue(wait.Text, out var row);
        var tokens = translated ? row!.Tokens : canonical;
        var output = new System.Text.StringBuilder();
        for (int i = 0; i < tokens.Count; i++)
        {
            int original = translated ? row!.SourceTokens[i] : i;
            if (original < wait.DisplayStart || original >= wait.DisplayEnd) continue;
            var token = tokens[i];
            if (token.Kind == ExplorationTextTokenKind.Literal) output.Append(token.Value);
            else if (token.Kind == ExplorationTextTokenKind.Newline) output.Append('\n');
            else if (token.Kind is ExplorationTextTokenKind.MemberName or ExplorationTextTokenKind.ClassName)
            {
                var binding = wait.DisplayNames.FirstOrDefault(name => name.SourceToken == original);
                if (binding is null) return new("Text unavailable (display-operands).", "en", "display-operands");
                output.Append(binding.CustomName ?? binding.CanonicalName);
            }
        }
        return new(output.ToString(), translated ? Locale : "en", translated ? null :
            _rejected.ContainsKey(wait.Text) ? "invalid-translation" : "missing-translation");
    }
}

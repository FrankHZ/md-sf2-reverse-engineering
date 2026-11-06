using System.Text.RegularExpressions;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Content.Localization;

// Modern display policy only. Canonical tokens, glyph units and literal paragraphs stay intact.
public static class SourceTextLayout
{
    // Reviewed Search display metadata for the admitted pre-voyage US source, not gameplay IDs.
    // Positions are canonical token indexes (N1 = 1 in these description rows).
    // Owner: remake/docs/evidence/dialogue-corpus-layout.md#semantic-line-breaks.
    private static readonly IReadOnlyDictionary<int, IReadOnlySet<int>> SearchSemanticBreaks =
        new Dictionary<int, IReadOnlySet<int>>
        {
            [4035] = new HashSet<int> { 1 }, [4036] = new HashSet<int> { 1 },
            [4039] = new HashSet<int> { 1 }, [4040] = new HashSet<int> { 1 },
            [4041] = new HashSet<int> { 1 }, [4042] = new HashSet<int> { 1 },
            [4043] = new HashSet<int> { 1, 3 }, [4044] = new HashSet<int> { 1 },
            [4045] = new HashSet<int> { 1 }, [4046] = new HashSet<int> { 1 },
            [4047] = new HashSet<int> { 1, 3 }, [4048] = new HashSet<int> { 1, 3 },
            [4049] = new HashSet<int> { 1 }, [4050] = new HashSet<int> { 1 },
            [4051] = new HashSet<int> { 1 }, [4052] = new HashSet<int> { 1 },
            [4053] = new HashSet<int> { 1 }, [4055] = new HashSet<int> { 1 },
            [4056] = new HashSet<int> { 1 }, [4057] = new HashSet<int> { 1 },
            [4059] = new HashSet<int> { 1 }, [4065] = new HashSet<int> { 1 },
            [4066] = new HashSet<int> { 1 }, [4067] = new HashSet<int> { 1 },
            [4068] = new HashSet<int> { 1 }, [4069] = new HashSet<int> { 1 },
            [4070] = new HashSet<int> { 1 }, [4071] = new HashSet<int> { 1 },
            [4072] = new HashSet<int> { 1 },
        };
    private static readonly IReadOnlySet<int> NoSemanticBreaks = new HashSet<int>();

    // Null keeps the existing whole-row policy for other sources/unreviewed descriptions.
    // Translation projection uses the catalog's admitted source-token map for these positions.
    public static IReadOnlySet<int>? SearchBreaks(int text, ExplorationDefinition source)
    {
        if (source.Provenance is not { Repository: "https://github.com/ShiningForceCentral/SF2DISASM.git",
                Commit: "c834c652b6862bc5679fd7f69a38a7093206efc6",
                RomSha256: "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9" } ||
            text is not (>= 403 and <= 405 or >= 408 and <= 410 or 412 or 423 or >= 425 and <= 430 or
                >= 4035 and <= 4059 or >= 4065 and <= 4073)) return null;
        var preserved = SearchSemanticBreaks.GetValueOrDefault(text) ?? NoSemanticBreaks;
        return source.TextTokens.TryGetValue(text, out var tokens) &&
            preserved.All(index => index < tokens.Count && tokens[index].Kind == ExplorationTextTokenKind.Newline)
            ? preserved : null;
    }

    public static string Break(IReadOnlyList<ExplorationTextToken> tokens, int index, string locale,
        string? before = null, string? after = null)
    {
        // Repeated breaks and breaks at control/row boundaries retain their explicit layout.
        if (index == 0 || index + 1 == tokens.Count ||
            !Text(tokens[index - 1]) || !Text(tokens[index + 1])) return "\n";
        before ??= tokens[index - 1].Kind == ExplorationTextTokenKind.Literal ? tokens[index - 1].Value : null;
        after ??= tokens[index + 1].Kind == ExplorationTextTokenKind.Literal ? tokens[index + 1].Value : null;
        // An unbound name has no display characters to inspect; never infer them from {NAME}.
        if (string.IsNullOrEmpty(before) || string.IsNullOrEmpty(after)) return " ";
        return Separator(before[^1], after[0], locale);
    }

    // Entry format v1 deliberately flattened source {N} to LF. This is not a generic
    // newline normalizer: only EntryPresentation dialogue has this legacy contract.
    public static string EntryLines(string text, string locale) =>
        Regex.Replace(text, @"(?<!\n)\n(?!\n)", match =>
            match.Index == 0 || match.Index + 1 == text.Length ? "\n" :
            Separator(text[match.Index - 1], text[match.Index + 1], locale));

    private static bool Text(ExplorationTextToken token) => token.Kind is
        ExplorationTextTokenKind.Literal or ExplorationTextTokenKind.MemberName or ExplorationTextTokenKind.ClassName;

    private static string Separator(char left, char right, string locale)
    {
        if (char.IsWhiteSpace(left) || char.IsWhiteSpace(right)) return "";
        // Chinese prose needs no inserted word space. Keep Latin words separated in mixed text.
        return locale == "zh-CN" && !(char.IsAsciiLetterOrDigit(left) && char.IsAsciiLetterOrDigit(right)) ? "" : " ";
    }
}

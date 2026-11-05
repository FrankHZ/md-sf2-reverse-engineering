using System.Text.RegularExpressions;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Content.Localization;

// Modern display policy only. Canonical tokens, glyph units and literal paragraphs stay intact.
public static class SourceTextLayout
{
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

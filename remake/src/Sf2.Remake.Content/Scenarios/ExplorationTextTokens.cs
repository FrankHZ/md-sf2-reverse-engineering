using Sf2.Remake.Application.Content.Scenarios;
namespace Sf2.Remake.Content.Scenarios;

public static class ExplorationTextTokens
{
    public static IReadOnlyList<ExplorationTextToken> Read(string text)
    {
        var tokens = new List<ExplorationTextToken>();
        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(text, @"\{[^{}]*\}|[{}]|[^{}]+"))
        {
            string value = match.Value;
            if (value == "{N}") tokens.Add(new(ExplorationTextTokenKind.Newline, value));
            else if (value == "{W1}") tokens.Add(new(ExplorationTextTokenKind.Wait1, value));
            else if (value == "{W2}") tokens.Add(new(ExplorationTextTokenKind.Wait2, value));
            else if (value == "{D1}") tokens.Add(new(ExplorationTextTokenKind.Delay1, value));
            else if (value == "{CLEAR}") tokens.Add(new(ExplorationTextTokenKind.Clear, value));
            else if (value == "{LEADER}") tokens.Add(new(ExplorationTextTokenKind.MemberName, value, 0));
            else if (value == "{NAME}") tokens.Add(new(ExplorationTextTokenKind.MemberName, value));
            else if (value == "{CLASS}") tokens.Add(new(ExplorationTextTokenKind.ClassName, value));
            else if (value.StartsWith("{NAME;", StringComparison.Ordinal) && value.EndsWith('}') &&
                int.TryParse(value.AsSpan(6, value.Length - 7), System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture, out int member))
                tokens.Add(new(ExplorationTextTokenKind.MemberName, value, member));
            else tokens.Add(new(value.Contains('{') || value.Contains('}') ? ExplorationTextTokenKind.Unsupported :
                ExplorationTextTokenKind.Literal, value));
        }
        return tokens.AsReadOnly();
    }

}

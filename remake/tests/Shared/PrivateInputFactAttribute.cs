using Xunit;

namespace Sf2.Remake.TestSupport;

/// <summary>Reports optional private checks as skipped until their inputs are selected.</summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class PrivateInputFactAttribute : FactAttribute
{
    public PrivateInputFactAttribute(params string[] requiredVariables)
    {
        bool required = string.Equals(
            Environment.GetEnvironmentVariable("SF2_REQUIRE_PRIVATE_TESTS"),
            "1",
            StringComparison.Ordinal);
        bool selected = requiredVariables.Any(variable =>
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variable)));
        if (!required && !selected)
        {
            Skip = "Private inputs not selected; no private assertions ran. " +
                "Set SF2_REQUIRE_PRIVATE_TESTS=1 and configure " +
                string.Join(", ", requiredVariables) + ".";
        }
    }

    internal static string RequireInput(string variable)
    {
        string? value = Environment.GetEnvironmentVariable(variable);
        Assert.False(string.IsNullOrWhiteSpace(value),
            $"Required private test input {variable} is missing; no private assertions ran.");
        return value!;
    }
}

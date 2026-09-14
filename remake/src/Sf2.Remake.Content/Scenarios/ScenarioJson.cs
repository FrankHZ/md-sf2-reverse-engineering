using System.Text.Json;
using System.Text.RegularExpressions;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Content.Scenarios;

internal static class ScenarioJson
{
    internal static bool Boolean(JsonElement element, string key)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind is JsonValueKind.True or JsonValueKind.False, "boolean", key);
        return value.GetBoolean();
    }
    internal static void ObjectOptional(JsonElement element, string field, string optional, params string[] keys)
    {
        Require(element.ValueKind == JsonValueKind.Object, "object-required", field);
        Object(element, field, element.TryGetProperty(optional, out _) ? [.. keys, optional] : keys);
    }
    internal static void Object(JsonElement element, string field, params string[] keys)
    {
        Require(element.ValueKind == JsonValueKind.Object, "object-required", field);
        var found = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
            Require(keys.Contains(property.Name) && found.Add(property.Name), "unknown-or-duplicate-field", field);
        Require(found.Count == keys.Length, "missing-field", field);
    }
    internal static IEnumerable<JsonElement> Array(JsonElement element, string key)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind == JsonValueKind.Array, "array-required", key);
        return value.EnumerateArray();
    }
    internal static string Text(JsonElement element, string key)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind == JsonValueKind.String, "string-required", key);
        return value.GetString()!;
    }
    internal static string Id(JsonElement element, string key)
    {
        string value = Text(element, key);
        Require(Regex.IsMatch(value, "^[a-z][a-z0-9-]{0,63}$", RegexOptions.CultureInvariant), "identifier", key);
        return value;
    }
    internal static int Number(JsonElement element, string key, int minimum, int maximum)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _), "integer-required", key);
        int result = value.GetInt32();
        Require(result >= minimum && result <= maximum, "numeric-range", key);
        return result;
    }
    internal static uint WordImage(JsonElement element, string key)
    {
        var value = element.GetProperty(key);
        Require(value.ValueKind == JsonValueKind.Number && value.TryGetUInt32(out _), "rng-image", key);
        return value.GetUInt32();
    }
    internal static void Require(bool condition, string code, string field, bool unsupported = false)
    {
        if (!condition) throw new AdmissionIssue(new(unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
            code, field, code.Replace('-', ' ')));
    }
    internal static ScenarioReadRejected Rejected(string code, string field) =>
        new(new(SessionFailureKind.ContentError, code, field, code.Replace('-', ' ')));
    internal sealed class AdmissionIssue(SessionFailure failure) : Exception(failure.Code)
    { internal SessionFailure Failure { get; } = failure; }
}

using System.Numerics;
using static H4Comparison.Operands;

namespace H4Comparison;

internal static class VisualSourceOperands
{
    public static bool BytesEqual(object? measurement) =>
        Equal(At(measurement, "leftBytes"), At(measurement, "rightBytes")) && At(measurement, "firstDifference") is null;

    public static BigInteger Length(object? value) => value switch
    {
        List<object?> list => list.Count,
        Dictionary<string, object?> fields => fields.Count,
        string text => text.EnumerateRunes().Count(),
        _ => throw new OperandError("TypeError", $"object of type '{TypeName(value)}' has no len()")
    };
    private static string TypeName(object? value) => value switch
    {
        null => "NoneType", bool => "bool", BigInteger => "int", double => "float",
        string => "str", List<object?> => "list", Dictionary<string, object?> => "dict",
        _ => throw new InvalidDataException("Unknown visual source operand")
    };
    public static string AppendName(string prefix, object? value) => value is string text ? prefix + text
        : throw new OperandError("TypeError", $"can only concatenate str (not \"{TypeName(value)}\") to str");

    public static bool VisualEqual(object? definition)
    {
        var actual = At(definition, "actual"); var expected = At(definition, "expected");
        return Equal(At(actual, "header"), At(expected, "header"))
            && Equal(At(actual, "text"), At(expected, "text"))
            && (!Truth(At(actual, "text")) || BytesEqual(At(definition, "data")));
    }
}

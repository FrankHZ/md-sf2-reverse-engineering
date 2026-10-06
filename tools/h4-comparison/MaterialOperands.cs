using System.Numerics;
using static H4Comparison.Operands;

namespace H4Comparison;

internal sealed class MaterialFactError(BigInteger index) : Exception
{
    public BigInteger Index { get; } = index;
}

internal static class MaterialOperands
{
    public static object? Fact(object? value)
    {
        if (value is not Dictionary<string, object?> fields || fields.Count != 1)
            throw new InvalidDataException("Material factual operand envelope");
        if (fields.TryGetValue("value", out var result)) return result;
        if (fields.TryGetValue("error", out var error) && error is BigInteger index && index >= 0)
            throw new MaterialFactError(index);
        throw new InvalidDataException("Material factual error identifier");
    }
    public static string Text(object? value) => value as string ?? throw new OperandError("TypeError");
    public static object? Find(Dictionary<Key, object?> values, object? key)
    {
        ResourceOperands.HashKey(key);
        return values.TryGetValue(new Key(key), out var value) ? value : null;
    }
    public static bool BytesEqual(object? measurements)
    {
        var value = Fact(measurements);
        return Equal(At(value, "leftBytes"), At(value, "rightBytes")) && At(value, "firstDifference") is null;
    }
}

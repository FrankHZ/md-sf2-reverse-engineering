using System.Numerics;
using static H4Comparison.Operands;

namespace H4Comparison;

internal static class InventoryOperands
{
    private sealed record Tail(OperandError Error, int Length);
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, Tail> tails = new();

    public static object? Prepare(object? record)
    {
        if (record is not Dictionary<string, object?> envelope || envelope.Count != 2
            || !envelope.TryGetValue("value", out var value)
            || !envelope.TryGetValue("tails", out var errors) || errors is not List<object?> records)
            throw new InvalidDataException("Inventory selected operand envelope");
        foreach (var entry in records)
        {
            object? target = value;
            foreach (var part in Iterate(At(entry, "path")))
                target = part is string key ? At(target, key) : ((List<object?>)target!)[(int)(BigInteger)part!];
            if (target is not List<object?> list || At(entry, "error") is not string kind
                || kind is not ("KeyError" or "IndexError" or "TypeError" or "ValueError" or "AttributeError" or "OverflowError" or "OSError" or "FileNotFoundError")
                || At(entry, "message") is not string message || At(entry, "length") is not BigInteger length
                || length < list.Count || length > int.MaxValue)
                throw new InvalidDataException("Inventory deferred sequence failure");
            tails.Add(list, new Tail(new OperandError(kind, message), (int)length));
        }
        return value;
    }

    public static IEnumerable<object?> InventoryItems(object? value)
    {
        foreach (var item in ResourceOperands.Items(value)) yield return item;
        if (value is not null && tails.TryGetValue(value, out var tail)) throw tail.Error;
    }
    public static bool InventoryTruth(object? value) => value is not null && tails.TryGetValue(value, out var tail)
        ? tail.Length != 0 : Truth(value);
    public static object? InventoryIndex(object? value, BigInteger index)
    {
        if (value is List<object?> list && tails.TryGetValue(value, out var tail))
        {
            if (index < 0) index += tail.Length;
            if (index >= list.Count && index < tail.Length) throw tail.Error;
        }
        return ResourceOperands.Index(value, index);
    }
    public static BigInteger Bits(object? value) => value switch
    {
        BigInteger n => n, bool b => b ? BigInteger.One : BigInteger.Zero,
        _ => throw new OperandError("TypeError")
    };
    private static bool Integral(object? value) => value is BigInteger or bool;
    public static double Real(object? value)
    {
        if (value is double d) return d;
        var result = (double)Bits(value);
        if (double.IsInfinity(result)) throw new OperandError("OverflowError");
        return result;
    }
    public static object Subtract(object? a, object? b) =>
        Integral(a) && Integral(b) ? Bits(a) - Bits(b) : Real(a) - Real(b);
    public static object Floor24(object? value)
    {
        if (Integral(value))
        {
            var n = Bits(value); var q = BigInteger.DivRem(n, 24, out var remainder);
            return remainder < 0 ? q - 1 : q;
        }
        var number = Real(value);
        var remainderReal = number % 24;
        var quotient = (number - remainderReal) / 24;
        if (remainderReal < 0) quotient--;
        var floor = Math.Floor(quotient);
        return quotient - floor > 0.5 ? floor + 1 : floor;
    }
    public static bool LE(object? a, object? b) => ResourceOperands.Less(a, b) || Equal(a, b);
    public static object? Or(object? value, object fallback) => InventoryTruth(value) ? value : fallback;
    public static object?[] Key(params object?[] parts) => parts;
}

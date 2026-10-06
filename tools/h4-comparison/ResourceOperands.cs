using System.Numerics;
using static H4Comparison.Operands;

namespace H4Comparison;

// Context preparation errors reach the outer report's diagnostic string. Per-row
// predicate errors still use CountedChecks' existing class-only classification.
internal static class ResourceOperands
{
    private static string TypeName(object? value) => value switch
    {
        null => "NoneType", bool => "bool", BigInteger => "int", double => "float",
        string => "str", List<object?> => "list", Dictionary<string, object?> => "dict",
        _ => throw new InvalidDataException("Unsupported resource operand")
    };

    public static object? Field(object? value, string key)
    {
        if (value is Dictionary<string, object?>) return At(value, key);
        throw new OperandError("TypeError", value switch
        {
            string => "string indices must be integers, not 'str'",
            List<object?> => "list indices must be integers or slices, not str",
            _ => $"'{TypeName(value)}' object is not subscriptable"
        });
    }

    public static IEnumerable<object?> Items(object? value)
    {
        if (value is List<object?> or Dictionary<string, object?> or string) return Iterate(value);
        throw new OperandError("TypeError", $"'{TypeName(value)}' object is not iterable");
    }

    public static Key HashKey(object? value)
    {
        if (value is List<object?> or Dictionary<string, object?>)
            throw new OperandError("TypeError", $"unhashable type: '{TypeName(value)}'");
        return new Key(value);
    }

    public static BigInteger Integer(object? value, int digitLimit)
    {
        try { return SceneOperands.Int(value, digitLimit); }
        catch (OperandError e) when (e.Kind is "TypeError" or "ValueError")
        {
            if (e.Detail is not null) throw;
            var detail = e.Kind == "TypeError"
                ? $"int() argument must be a string, a bytes-like object or a real number, not '{TypeName(value)}'"
                : value is double ? "cannot convert float NaN to integer"
                : "invalid literal for int() with base 10: "
                    + string.Concat(SceneOperands.Repr(value).EnumerateRunes().Take(200).Select(r => r.ToString()));
            throw new OperandError(e.Kind, detail);
        }
    }

    public static object? Index(object? value, BigInteger index)
    {
        if (value is List<object?> or string && (index < long.MinValue || index > long.MaxValue))
            throw new OperandError("IndexError", "cannot fit 'int' into an index-sized integer");
        try { return SceneOperands.Index(value, index); }
        catch (OperandError e) when (e.Kind is "TypeError" or "IndexError")
        {
            var detail = e.Kind == "TypeError" ? $"'{TypeName(value)}' object is not subscriptable"
                : value is string ? "string index out of range" : "list index out of range";
            throw new OperandError(e.Kind, detail);
        }
    }

    private static object? Number(object? value) => value is bool b ? new BigInteger(b ? 1 : 0) : value;

    public static bool Less(object? left, object? right)
    {
        var a = Number(left); var b = Number(right);
        if (a is BigInteger ai && b is BigInteger bi) return ai < bi;
        if (a is double ad && b is double bd) return ad < bd;
        if (a is BigInteger integer && b is double real)
        {
            if (!double.IsFinite(real)) return double.IsPositiveInfinity(real);
            var truncated = new BigInteger(real);
            return integer < truncated || integer == truncated && real > (double)truncated;
        }
        if (a is double number && b is BigInteger whole)
        {
            if (!double.IsFinite(number)) return double.IsNegativeInfinity(number);
            var truncated = new BigInteger(number);
            return truncated < whole || truncated == whole && number < (double)truncated;
        }
        if (a is string sa && b is string sb)
        {
            var aa = sa.EnumerateRunes().Select(r => r.Value).ToArray();
            var bb = sb.EnumerateRunes().Select(r => r.Value).ToArray();
            return aa.AsSpan().SequenceCompareTo(bb) < 0;
        }
        if (a is List<object?> al && b is List<object?> bl)
        {
            for (var i = 0; i < Math.Min(al.Count, bl.Count); i++)
                if (!ReferenceEquals(al[i], bl[i]) && !Equal(al[i], bl[i])) return Less(al[i], bl[i]);
            return al.Count < bl.Count;
        }
        throw new OperandError("TypeError",
            $"'<' not supported between instances of '{TypeName(left)}' and '{TypeName(right)}'");
    }
}

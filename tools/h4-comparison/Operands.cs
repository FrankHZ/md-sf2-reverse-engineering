using System.Numerics;
using System.Text.Json;

namespace H4Comparison;

// Only the operations exercised by resource predicates. Missing fields must throw;
// .get and truth tests deliberately retain Python's different null/empty behavior.
internal sealed class OperandError(string kind, string? detail = null) : Exception(kind)
{
    public string Kind { get; } = kind;
    public string? Detail { get; } = detail;
    public bool Caught => Kind is "KeyError" or "IndexError" or "ValueError" or "TypeError";
}

internal static class Operands
{
    private static readonly Dictionary<string, object> nonFiniteValues = [];
    public static object? Read(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.GetRawText().IndexOfAny(['.', 'e', 'E']) < 0
            ? BigInteger.Parse(value.GetRawText(), System.Globalization.CultureInfo.InvariantCulture)
            : value.GetDouble(),
        JsonValueKind.Array => value.EnumerateArray().Select(Read).ToList(),
        JsonValueKind.Object => value.EnumerateObject().ToDictionary(p => p.Name, p => Read(p.Value)),
        _ => throw new InvalidDataException("Invalid resource operand")
    };

    public static void Write(Utf8JsonWriter writer, object? value, bool inventoryNumbers = false)
    {
        switch (value)
        {
            case null: writer.WriteNullValue(); break;
            case bool b: writer.WriteBooleanValue(b); break;
            case string s: writer.WriteStringValue(s); break;
            case BigInteger n: writer.WriteRawValue(n.ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
            case int n: writer.WriteNumberValue(n); break;
            case long n: writer.WriteNumberValue(n); break;
            case double n when !double.IsFinite(n):
                writer.WriteRawValue(double.IsNaN(n) ? "NaN" : n > 0 ? "Infinity" : "-Infinity", skipInputValidation: true); break;
            // Inventory entity keys include Python json.dumps before key normalization.
            // Preserve the float type (including -0.0) only on this new wire surface.
            case double n when inventoryNumbers && Math.Truncate(n) == n:
                writer.WriteRawValue(SceneOperands.Str(n)); break;
            case double n: writer.WriteNumberValue(n); break;
            case IDictionary<string, object?> map:
                writer.WriteStartObject();
                foreach (var (k, v) in map) { writer.WritePropertyName(k); Write(writer, v, inventoryNumbers); }
                writer.WriteEndObject(); break;
            case IEnumerable<object?> items:
                writer.WriteStartArray();
                foreach (var item in items) Write(writer, item, inventoryNumbers);
                writer.WriteEndArray(); break;
            default: throw new InvalidDataException("Unsupported resource output operand");
        }
    }

    public static Dictionary<string, object?> Dict(params (string, object?)[] pairs) =>
        pairs.ToDictionary(p => p.Item1, p => p.Item2);

    public static string RecipeKey(object? kind, object? want)
    {
        object? Normalize(object? value) => value switch
        {
            bool b => b ? BigInteger.One : BigInteger.Zero,
            double d when double.IsFinite(d) && Math.Truncate(d) == d => new BigInteger(d),
            List<object?> a => a.Select(Normalize).ToList(),
            Dictionary<string, object?> d => d.OrderBy(p => p.Key, StringComparer.Ordinal)
                .ToDictionary(p => p.Key, p => Normalize(p.Value)),
            _ => value
        };
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer)) Write(writer, new object?[] { Normalize(kind), Normalize(want) });
        return System.Text.Encoding.UTF8.GetString(buffer.ToArray());
    }

    public static void RestoreNonFinite(object? message)
    {
        foreach (var item in (List<object?>)At(message, "nonFinite")!)
        {
            var entry = (List<object?>)item!;
            var path = (List<object?>)entry[0]!;
            object? parent = message;
            foreach (var key in path.Take(path.Count - 1))
                parent = key is string s ? At(parent, s) : ((List<object?>)parent!)[(int)(BigInteger)key!];
            var token = (string)entry[2]!;
            if (!nonFiniteValues.TryGetValue(token, out var value))
            {
                value = entry[1] switch { "nan" => double.NaN, "inf" => double.PositiveInfinity,
                    "-inf" => double.NegativeInfinity, _ => throw new InvalidDataException("Invalid nonfinite operand") };
                nonFiniteValues.Add(token, value);
            }
            if (path[^1] is string name) ((Dictionary<string, object?>)parent!)[name] = value;
            else ((List<object?>)parent!)[(int)(BigInteger)path[^1]!] = value;
        }
    }

    public static string NonFiniteIdentity(object value) => nonFiniteValues
        .First(pair => ReferenceEquals(pair.Value, value)).Key;

    public static object? At(object? value, string key) => value switch
    {
        Dictionary<string, object?> map => map.TryGetValue(key, out var item) ? item : throw new OperandError("KeyError"),
        _ => throw new OperandError("TypeError")
    };

    public static object? Get(object? value, string key) => value is Dictionary<string, object?> map
        ? map.GetValueOrDefault(key) : throw new OperandError("AttributeError");

    public static bool Truth(object? value) => value switch
    {
        null => false, bool b => b, BigInteger n => n != 0, double n => n != 0,
        string s => s.Length != 0, List<object?> a => a.Count != 0,
        Dictionary<string, object?> d => d.Count != 0,
        _ => throw new InvalidDataException("Unsupported truth operand")
    };

    private static bool Integral(object? v) => v is BigInteger or bool;
    private static BigInteger Integer(object? v) => v is bool b ? (b ? BigInteger.One : BigInteger.Zero) : (BigInteger)v!;
    private static bool Numeric(object? v) => Integral(v) || v is double;
    private static double Real(object? v)
    {
        if (v is double n) return n;
        var converted = (double)Integer(v);
        if (double.IsInfinity(converted)) throw new OperandError("OverflowError");
        return converted;
    }

    public static bool Equal(object? a, object? b)
    {
        if (Numeric(a) && Numeric(b))
        {
            if (Integral(a) && Integral(b)) return Integer(a) == Integer(b);
            if (a is double x && b is double y) return x == y;
            var real = a is double ad ? ad : (double)b!;
            var integer = Integral(a) ? Integer(a) : Integer(b);
            return double.IsFinite(real) && Math.Truncate(real) == real && new BigInteger(real) == integer;
        }
        if (a is List<object?> aa && b is List<object?> bb)
            return aa.Count == bb.Count && aa.Zip(bb).All(p => ReferenceEquals(p.First, p.Second) || Equal(p.First, p.Second));
        if (a is Dictionary<string, object?> am && b is Dictionary<string, object?> bm)
            return am.Count == bm.Count && am.All(p => bm.TryGetValue(p.Key, out var v)
                && (ReferenceEquals(p.Value, v) || Equal(p.Value, v)));
        return Equals(a, b);
    }

    internal readonly struct Key(object? value) : IEquatable<Key>
    {
        private readonly object? value = value;
        public bool Equals(Key other) => Equal(value, other.value);
        public override bool Equals(object? other) => other is Key key && Equals(key);
        public override int GetHashCode()
        {
            if (value is List<object?> or Dictionary<string, object?>) throw new OperandError("TypeError");
            if (Integral(value)) return Integer(value).GetHashCode();
            if (value is double d && double.IsFinite(d) && Math.Truncate(d) == d)
                return new BigInteger(d).GetHashCode();
            return value?.GetHashCode() ?? 0;
        }
    }

    public static (object? Value, bool Present) Lookup(Dictionary<Key, object?> entries, object? key)
    {
        if (key is List<object?> or Dictionary<string, object?>) throw new OperandError("TypeError");
        var present = entries.TryGetValue(new Key(key), out var value);
        return (value, present);
    }

    public static Dictionary<string, object?> Selector(string kind, object? want)
    {
        if (want is not Dictionary<string, object?> map || map.ContainsKey("kind"))
            throw new OperandError("TypeError");
        var result = Dict(("kind", kind));
        foreach (var pair in map) result.Add(pair.Key, pair.Value);
        return result;
    }

    public static IEnumerable<object?> Iterate(object? value) => value switch
    {
        List<object?> list => list,
        Dictionary<string, object?> map => map.Keys.Cast<object?>(),
        string s => s.EnumerateRunes().Select(r => (object?)r.ToString()),
        _ => throw new OperandError("TypeError")
    };

    public static object? Multiply8(object? value)
    {
        if (Integral(value)) return Integer(value) * 8;
        if (value is double d) return d * 8;
        if (value is string s) return string.Concat(Enumerable.Repeat(s, 8));
        if (value is List<object?> a) return Enumerable.Range(0, 8).SelectMany(_ => a).ToList();
        throw new OperandError("TypeError");
    }

    public static object? Add(object? a, object? b)
    {
        if (Integral(a) && Integral(b)) return Integer(a) + Integer(b);
        if (Numeric(a) && Numeric(b)) return Real(a) + Real(b);
        if (a is string sa && b is string sb) return sa + sb;
        if (a is List<object?> aa && b is List<object?> bb) return aa.Concat(bb).ToList();
        throw new OperandError("TypeError");
    }

    public static void SetTile(List<object?> tiles, object? index, object? value)
    {
        if (!Integral(index)) throw new OperandError("TypeError");
        var i = Integer(index);
        if (i < 0) i += tiles.Count;
        if (i < 0 || i >= tiles.Count) throw new OperandError("IndexError");
        tiles[(int)i] = value;
    }
}

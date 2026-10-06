using System.Globalization;
using System.Numerics;
using System.Text;
using static H4Comparison.Operands;

namespace H4Comparison;

// Python operations reached only by scene observations; no engine-rule dependency.
internal static class SceneOperands
{
    public static object? Default(object? value, string key, object? fallback) =>
        value is Dictionary<string, object?> map
            ? map.TryGetValue(key, out var found) ? found : fallback
            : throw new OperandError("AttributeError");

    public static object? Index(object? value, BigInteger index)
    {
        if (value is Dictionary<string, object?>) throw new OperandError("KeyError");
        var items = value switch
        {
            List<object?> list => list,
            string s => s.EnumerateRunes().Select(r => (object?)r.ToString()).ToList(),
            _ => throw new OperandError("TypeError")
        };
        if (index < 0) index += items.Count;
        if (index < 0 || index >= items.Count) throw new OperandError("IndexError");
        return items[(int)index];
    }

    public static BigInteger Int(object? value, int digitLimit)
    {
        if (value is BigInteger integer) return integer;
        if (value is bool boolean) return boolean ? BigInteger.One : BigInteger.Zero;
        if (value is double number)
        {
            if (double.IsNaN(number)) throw new OperandError("ValueError");
            if (double.IsInfinity(number)) throw new OperandError("OverflowError");
            return new BigInteger(number);
        }
        if (value is not string text) throw new OperandError("TypeError");
        text = text.Trim();
        var digits = new StringBuilder();
        var sign = 1;
        var start = 0;
        if (text.StartsWith('+') || text.StartsWith('-'))
        { sign = text[0] == '-' ? -1 : 1; start = 1; }
        var previousDigit = false;
        for (var i = start; i < text.Length; i++)
        {
            if (text[i] == '_' && previousDigit) { previousDigit = false; continue; }
            var digit = CharUnicodeInfo.GetDecimalDigitValue(text, i);
            if (digit < 0) throw new OperandError("ValueError");
            digits.Append((char)('0' + digit));
            previousDigit = true;
            if (char.IsHighSurrogate(text[i])) i++;
        }
        if (!previousDigit) throw new OperandError("ValueError");
        if (digitLimit != 0 && digits.Length > digitLimit)
            throw new OperandError("ValueError", $"Exceeds the limit ({digitLimit} digits) for integer string conversion: "
                + $"value has {digits.Length} digits; use sys.set_int_max_str_digits() to increase the limit");
        return BigInteger.Parse(digits.ToString(), CultureInfo.InvariantCulture) * sign;
    }

    public static bool Contains(object? container, object? item)
    {
        if (container is string text)
            return item is string s ? text.Contains(s, StringComparison.Ordinal) : throw new OperandError("TypeError");
        if (container is Dictionary<string, object?> map)
        {
            if (item is List<object?> or Dictionary<string, object?>) throw new OperandError("TypeError");
            return item is string key && map.ContainsKey(key);
        }
        return Iterate(container).Any(value => ReferenceEquals(value, item) || Equal(value, item));
    }

    public static Dictionary<Key, object?> Mounted(object? rows, string key, string? valueKey = null)
    {
        Dictionary<Key, object?> result = [];
        foreach (var row in Iterate(rows))
        {
            var identity = At(row, key);
            if (identity is List<object?> or Dictionary<string, object?>) throw new OperandError("TypeError");
            result[new Key(identity)] = valueKey is null ? row : Get(row, valueKey);
        }
        return result;
    }

    public static string Str(object? value) => value switch
    {
        null => "None", bool b => b ? "True" : "False", string s => s,
        BigInteger n => n.ToString(CultureInfo.InvariantCulture),
        double n => FloatText(n),
        List<object?> list => "[" + string.Join(", ", list.Select(Repr)) + "]",
        Dictionary<string, object?> map => "{" + string.Join(", ", map.Select(p => Repr(p.Key) + ": " + Repr(p.Value))) + "}",
        _ => throw new InvalidDataException("Unsupported scene string operand")
    };

    private static string FloatText(double number)
    {
        if (double.IsNaN(number)) return "nan";
        if (double.IsPositiveInfinity(number)) return "inf";
        if (double.IsNegativeInfinity(number)) return "-inf";
        var sign = BitConverter.DoubleToInt64Bits(number) < 0 ? "-" : "";
        var text = Math.Abs(number).ToString("R", CultureInfo.InvariantCulture).ToLowerInvariant();
        var parts = text.Split('e');
        var exponent = parts.Length == 2 ? int.Parse(parts[1], CultureInfo.InvariantCulture) : 0;
        var point = parts[0].IndexOf('.');
        exponent += (point < 0 ? parts[0].Length : point) - 1;
        var digits = parts[0].Replace(".", "");
        while (digits.Length > 1 && digits[0] == '0') { digits = digits[1..]; exponent--; }
        digits = digits.TrimEnd('0');
        if (digits.Length == 0) return sign + "0.0";
        if (exponent < -4 || exponent >= 16)
            return sign + digits[0] + (digits.Length > 1 ? "." + digits[1..] : "")
                + "e" + (exponent < 0 ? "-" : "+") + Math.Abs(exponent).ToString("D2", CultureInfo.InvariantCulture);
        if (exponent < 0) return sign + "0." + new string('0', -exponent - 1) + digits;
        if (digits.Length <= exponent + 1) return sign + digits + new string('0', exponent + 1 - digits.Length) + ".0";
        return sign + digits.Insert(exponent + 1, ".");
    }

    public static string Repr(object? value)
    {
        if (value is not string text) return Str(value);
        var quote = text.Contains('\'') && !text.Contains('"') ? '"' : '\'';
        var result = new StringBuilder().Append(quote);
        foreach (var rune in text.EnumerateRunes())
        {
            var n = rune.Value;
            if (n == quote || n == '\\') result.Append('\\').Append(rune);
            else if (n == 9) result.Append("\\t");
            else if (n == 10) result.Append("\\n");
            else if (n == 13) result.Append("\\r");
            else if (n != 32 && Rune.GetUnicodeCategory(rune) is UnicodeCategory.Control
                or UnicodeCategory.Format or UnicodeCategory.Surrogate or UnicodeCategory.PrivateUse
                or UnicodeCategory.OtherNotAssigned or UnicodeCategory.LineSeparator
                or UnicodeCategory.ParagraphSeparator or UnicodeCategory.SpaceSeparator)
                result.Append(n <= 255 ? "\\x" + n.ToString("x2") : n <= 65535 ? "\\u" + n.ToString("x4") : "\\U" + n.ToString("x8"));
            else result.Append(rune);
        }
        return result.Append(quote).ToString();
    }
}

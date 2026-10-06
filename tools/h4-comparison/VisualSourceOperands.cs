using System.Numerics;
using static H4Comparison.Operands;

namespace H4Comparison;

// Only the visual-source predicates use these demand reads. The Python owner
// retains its existing bounded sequences; no comparison verdict crosses the pipe.
internal sealed class VisualSourceOperands
{
    private sealed record Source(int Id, string Type);
    private sealed record AtlasText(object? Measurement);
    private readonly Dictionary<int, Source> sources = [];
    private TaskCompletionSource<object?>? pending;
    private object? request;
    private int sequence, cursor;

    public object? Prepare(object? envelope)
    {
        object? value = At(envelope, "value");
        foreach (var row in Iterate(At(envelope, "references")))
        {
            var id = (int)(BigInteger)At(row, "source")!;
            if (!sources.TryGetValue(id, out var source)) sources.Add(id, source = new Source(id, (string)At(row, "type")!));
            var path = (List<object?>)At(row, "path")!;
            if (path.Count == 0) { value = source; continue; }
            object? parent = value;
            foreach (var key in path.Take(path.Count - 1))
                parent = key is string name ? At(parent, name) : ((List<object?>)parent!)[(int)(BigInteger)key!];
            if (path[^1] is string field) ((Dictionary<string, object?>)parent!)[field] = source;
            else ((List<object?>)parent!)[(int)(BigInteger)path[^1]!] = source;
        }
        return value;
    }

    public void Start() { sequence = cursor = 0; }
    public object Request => request ?? throw new InvalidDataException("Missing visual source continuation");
    public void Reply(object? message)
    {
        if (pending is null || !Equal(At(message, "sequence"), new BigInteger(sequence)))
            throw new InvalidDataException("Visual source continuation changed");
        var continuation = pending; pending = null; request = null;
        continuation.SetResult(Prepare(At(message, "value")));
    }
    public void Cancel()
    {
        var continuation = pending; pending = null; request = null;
        continuation?.SetCanceled();
    }
    private Task<object?> Read(Source source, string action, object? index, int iterator = 0)
    {
        if (pending is not null) throw new InvalidDataException("Overlapping visual source read");
        pending = new TaskCompletionSource<object?>();
        request = Dict(("sourceRead", Dict(("sequence", ++sequence), ("source", source.Id),
            ("action", action), ("index", index), ("cursor", iterator))));
        return pending.Task;
    }
    public async ValueTask<BigInteger> Length(object? value) => value is Source source
        ? (BigInteger)(await Read(source, "length", BigInteger.Zero))! : NativeLength(value);
    public async ValueTask<object?> Index(object? value, BigInteger index) => value is Source source
        ? await Read(source, "index", index) : ResourceOperands.Index(value, index);
    public async ValueTask<object?> Field(object? value, string key) => value is Source source
        ? await Read(source, "index", key) : ResourceOperands.Field(value, key);
    public static Key SourceKey(object? value) => value is Source source
        ? throw new OperandError("TypeError", $"unhashable type: '{source.Type}'") : ResourceOperands.HashKey(value);
    public static bool SourceLess(object? a, object? b) => a is Source || b is Source
        ? throw new OperandError("TypeError") : ResourceOperands.Less(a, b);
    public static BigInteger SourceInteger(object? value, int digits) => value is Source
        ? throw new OperandError("TypeError") : ResourceOperands.Integer(value, digits);
    private async ValueTask<(bool Done, object? Value)> Next(object? value, int index, int iterator)
    {
        if (value is Source source)
        {
            var row = await Read(source, "next", new BigInteger(index), iterator);
            return ((bool)At(row, "done")!, At(row, "value"));
        }
        var list = (List<object?>)value!;
        return index >= list.Count ? (true, null) : (false, list[index]);
    }

    public async ValueTask<bool> Same(object? a, object? b)
    {
        if (a is AtlasText || b is AtlasText)
            return a is AtlasText text && b is AtlasText && BytesEqual(text.Measurement);
        if (a is Source || b is Source)
        {
            if (a is not (Source or List<object?>) || b is not (Source or List<object?>)) return false;
            // Python list comparison delegates to the bounded sequence's __eq__.
            if (a is not Source) (a, b) = (b, a);
            if (await Length(a) != await Length(b)) return false;
            var leftCursor = ++cursor; var rightCursor = ++cursor;
            for (var i = 0; ; i++)
            {
                var left = await Next(a, i, leftCursor);
                var right = await Next(b, i, rightCursor);
                if (left.Done || right.Done)
                {
                    if (left.Done != right.Done) throw new OperandError("ValueError",
                        left.Done ? "zip() argument 2 is longer than argument 1" : "zip() argument 2 is shorter than argument 1");
                    return true;
                }
                if (!await Same(left.Value, right.Value)) return false;
            }
        }
        if (a is Dictionary<string, object?> am && b is Dictionary<string, object?> bm)
        {
            if (am.Count != bm.Count) return false;
            foreach (var (key, value) in am)
                if (!bm.TryGetValue(key, out var other) || !ReferenceEquals(value, other) && !await Same(value, other)) return false;
            return true;
        }
        if (a is List<object?> al && b is List<object?> bl)
        {
            if (al.Count != bl.Count) return false;
            for (var i = 0; i < al.Count; i++)
                if (!ReferenceEquals(al[i], bl[i]) && !await Same(al[i], bl[i])) return false;
            return true;
        }
        return Equal(a, b);
    }

    public static bool BytesEqual(object? measurement) =>
        Equal(At(measurement, "leftBytes"), At(measurement, "rightBytes")) && At(measurement, "firstDifference") is null;
    private static BigInteger NativeLength(object? value) => value switch
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

    public async ValueTask<bool> VisualEqual(object? definition)
    {
        var actual = At(definition, "actual"); var expected = At(definition, "expected");
        return await Same(At(actual, "header"), At(expected, "header"))
            && Equal(At(actual, "text"), At(expected, "text"))
            && (!Truth(At(actual, "text")) || BytesEqual(At(definition, "data")));
    }

    public static void DefineVisual(object? definition)
    {
        foreach (var name in new[] { "actual", "expected" })
        {
            var visual = At(definition, name);
            if (Truth(At(visual, "text")))
                ((Dictionary<string, object?>)At(At(visual, "header"), "atlas")!)["data"] = new AtlasText(At(definition, "data"));
        }
    }
}

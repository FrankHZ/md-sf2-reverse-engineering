using System.Numerics;
using static H4Comparison.Operands;

namespace H4Comparison;

internal sealed record PairResult(List<object?[]> Events, object? Valid, string? Error);

internal sealed class ResourceComparison
{
    private readonly Dictionary<string, Dictionary<Key, object?>> sources = new()
    {
        ["sprites"] = [], ["sourceSprites"] = [], ["portraits"] = [], ["sourcePortraits"] = []
    };
    private object? required;
    private object? stop;
    private PairResult? failure;
    private object? witness;
    private readonly List<object?> checks = [];
    private BigInteger passed, failed, unavailable, executed;
    private readonly Dictionary<string, (List<object?[]>, string?)> recipes = [];

    private (List<object?[]>, string?) Recipe(object? kind, object? want)
    {
        var key = RecipeKey(kind, want);
        if (!recipes.TryGetValue(key, out var result))
        {
            result = Source(kind, want);
            recipes.Add(key, result);
        }
        return result;
    }

    private (List<object?[]>, string?) Source(object? kind, object? want)
    {
        List<object?[]> events = [];
        try
        {
            if (Equal(kind, "entity"))
            {
                var key = At(want, "sprite");
                var actual = Lookup(sources["sprites"], key);
                var original = Lookup(sources["sourceSprites"], key);
                events.Add(new object?[] { "reached sprite original pointer/palette/decode",
                    Equal(actual.Value, original.Value) && original.Present });
            }
            else if (!Equal(kind, "map"))
            {
                var key = At(want, "portrait");
                var actual = Lookup(sources["portraits"], key);
                var original = Lookup(sources["sourcePortraits"], key);
                events.Add(new object?[] { "reached portrait original decode/tile composition",
                    Equal(actual.Value, original.Value) && original.Present });
                var tiles = Enumerable.Range(0, 64).Select(i => (object?)new BigInteger(i)).ToList();
                if (Truth(original.Value))
                {
                    // Python evaluates both tuple operands before entering either loop.
                    object?[] changes = [Truth(At(want, "eyes")) ? At(original.Value, "eyes") : new List<object?>(),
                        Truth(At(want, "mouth")) ? At(original.Value, "mouth") : new List<object?>()];
                    foreach (var changeSet in changes)
                        foreach (var change in Iterate(changeSet))
                        {
                            var row = Iterate(change).Take(5).ToList();
                            if (row.Count != 4) throw new OperandError("ValueError");
                            // Assignment evaluates the RHS before its subscript.
                            var alternate = Add(Multiply8(row[3]), row[2]);
                            SetTile(tiles, Add(Multiply8(row[1]), row[0]), alternate);
                        }
                }
                events.Add(new object?[] { "portrait source alternate tile selection",
                    original.Value is null ? null : Equal(At(want, "tiles"), tiles) });
            }
        }
        catch (OperandError e) when (e.Caught) { return (events, e.Kind); }
        return (events, null);
    }

    private PairResult Pair(object? used)
    {
        List<object?[]> events = [];
        object? valid = null;
        try
        {
            var kind = At(required, "kind");
            var want = At(required, "expected");
            var bound = At(used, "used");
            if (Equal(kind, "map"))
            {
                bound = Get(bound, "selector");
                valid = bound is null ? null : Equal(bound,
                    Dict(("kind", "map-block"), ("map", At(At(required, "identity"), "map")),
                        ("block", At(want, "block"))))
                    && Equal(Get(At(used, "used"), "word"), At(want, "word"));
            }
            else if (Equal(kind, "entity"))
                valid = bound is null ? null : Equal(bound, Selector("entity", want));
            else if (bound is not null)
            {
                var texture = Get(bound, "texturePresent");
                valid = Truth(texture) ? Equal(Get(bound, "selector"), Selector("portrait", want)) : texture;
            }
            var (sourceEvents, error) = Equal(kind, "map") ? (new List<object?[]>(), null) : Recipe(kind, want);
            events.AddRange(sourceEvents);
            if (error is null) events.Add(new object?[] { "bound texture selector matches logical source requirement", valid });
            return new(events, valid, error);
        }
        catch (OperandError e) when (e.Caught) { return new(events, valid, e.Kind); }
    }

    public object? Handle(object? message)
    {
        switch (At(message, "op"))
        {
            case "source":
                foreach (var pair in (Dictionary<string, object?>)At(message, "entries")!)
                    foreach (var item in (List<object?>)pair.Value!)
                    {
                        var row = (List<object?>)item!;
                        sources[pair.Key][new Key(row[0])] = row[1];
                    }
                return null;
            case "begin":
                required = At(message, "required");
                stop = witness = null; failure = null; checks.Clear();
                passed = failed = unavailable = executed = 0;
                return null;
            case "scan":
                foreach (var row in Iterate(At(message, "rows")))
                {
                    if (stop is not null) break;
                    var pair = Pair(At(row, "used"));
                    if (pair.Error is not null)
                    {
                        stop = At(row, "first"); failure = pair;
                        witness = At(At(row, "used"), "_captureLocator");
                    }
                }
                return stop;
            case "accumulate":
                foreach (var row in Iterate(At(message, "rows")))
                {
                    var count = (BigInteger)At(row, "count")!;
                    var used = At(row, "used");
                    var pair = Pair(used);
                    if (pair.Error is not null) throw new InvalidDataException("resource exception prefix changed");
                    foreach (var e in pair.Events)
                        checks.Add(new object?[] { e[0], e[1], count, At(used, "_captureLocator") });
                    if (Equal(pair.Valid, false)) failed += count;
                    else if (pair.Valid is null) unavailable += count;
                    else passed += count;
                    executed += count;
                }
                return null;
            case "finish":
                if (failure is not null)
                {
                    foreach (var e in failure.Events)
                        checks.Add(new object?[] { e[0], e[1], 1, witness });
                    checks.Add(new object?[] {
                        failure.Error == "KeyError" ? "required texture join operand absent"
                            : "required texture join malformed " + failure.Error,
                        failure.Error == "KeyError" ? null : false, 1, witness });
                }
                return Dict(("candidatePairCount", At(message, "total")), ("executedPairCount", executed),
                    ("counts", Dict(("PASS", passed), ("FAIL", failed), ("Unavailable", unavailable))),
                    ("legacyStop", failure is null ? null : Dict(("firstOrdinal", stop),
                        ("error", failure.Error), ("locator", witness))), ("checks", checks));
            default: throw new InvalidDataException("Unknown resource phase operation");
        }
    }
}

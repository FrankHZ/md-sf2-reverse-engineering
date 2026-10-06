using System.Numerics;
using static H4Comparison.Operands;

namespace H4Comparison;

internal sealed class CountedChecks(IEnumerable<object?> enabled)
{
    // Check families are strings; a non-string scope never matched them in Python.
    private readonly HashSet<string> enabled = enabled.OfType<string>().ToHashSet();
    private readonly Dictionary<(string Family, string Name, int Outcome), BigInteger> counts = [];
    private readonly Queue<(string Family, string Name, int Outcome)> order = [];
    private readonly Dictionary<(string Family, string Name, int Outcome), int> witnessCounts = [];
    private readonly Queue<object?> witnesses = [];
    private bool finished;

    private static object? Value(int outcome) => outcome == 1 ? null : outcome == 2;

    public void Check(string family, string name, object? value, BigInteger weight,
        object? locator = null, object? identity = null)
    {
        if (finished) throw new InvalidDataException("Resource report already finalized");
        if (!enabled.Contains(family)) return;
        var outcome = Equal(value, false) ? 0 : value is null ? 1 : 2;
        var key = (family, name, outcome);
        if (!counts.ContainsKey(key)) order.Enqueue(key);
        counts[key] = counts.GetValueOrDefault(key) + weight;
        if (outcome == 2 || witnessCounts.GetValueOrDefault(key) >= 8) return;
        var row = Dict(("family", family), ("name", name), ("value", Value(outcome)), ("count", weight));
        if (identity is not null) row.Add("identity", identity);
        if (locator is not null) row.Add("locator", locator);
        witnesses.Enqueue(row);
        witnessCounts[key] = witnessCounts.GetValueOrDefault(key) + 1;
    }

    public void Error(IEnumerable<string> families, string name, string error, BigInteger weight,
        object? locator = null)
    {
        if (error is not ("KeyError" or "IndexError" or "ValueError" or "TypeError"))
            throw new OperandError(error);
        foreach (var family in families)
            Check(family, name + (error == "KeyError" ? " operand absent" : " malformed " + error),
                error == "KeyError" ? null : false, weight, locator);
    }

    public void Evaluated(string family, string name, Action action)
    {
        try { action(); }
        catch (OperandError error) when (error.Caught) { Error([family], name, error.Kind, BigInteger.One); }
    }

    public void Accept(object? record)
    {
        var weight = (BigInteger)At(record, "weight")!;
        var locator = At(record, "locator");
        var name = (string)At(record, "name")!;
        if (Equal(At(record, "kind"), "error"))
            Error(Iterate(At(record, "families")).Cast<string>(), name,
                (string)At(record, "error")!, weight, locator);
        else if (Equal(At(record, "kind"), "check"))
            Check((string)At(record, "family")!, name, At(record, "value"), weight,
                locator, At(record, "identity"));
        else throw new InvalidDataException("Unknown counted-check record");
    }

    public object Finish()
    {
        if (finished) throw new InvalidDataException("Resource report already finalized");
        finished = true;
        var families = Dict();
        var summary = Dict(("witnessPolicy", Dict(
            ("limitPerCheckOutcome", 8), ("retained", witnesses.Count),
            ("candidateVariants", "all distinct operands retained"))), ("familyCounts", families));
        foreach (var family in new[] { "map", "entity", "scene" })
        {
            BigInteger[] totals = [0, 0, 0];
            var present = false;
            foreach (var (key, count) in counts)
                if (key.Family == family) { present = true; totals[key.Outcome] += count; }
            families.Add(family, Dict(("PASS", totals[2]), ("FAIL", totals[0]), ("Unavailable", totals[1])));
            summary.Add(family, totals[0] != 0 ? false : totals[1] != 0 || !present ? null : true);
        }
        return Dict(("summary", summary), ("checkCount", order.Count), ("witnessCount", witnesses.Count));
    }

    public object Drain(bool witnessRows)
    {
        if (!finished) throw new InvalidDataException("Resource report not finalized");
        List<object?> rows = [];
        long bytes = 0;
        while (rows.Count < 256 && (witnessRows ? witnesses.Count : order.Count) > 0)
        {
            object? row;
            if (witnessRows) row = witnesses.Peek();
            else
            {
                var key = order.Peek();
                row = Dict(("family", key.Family), ("name", key.Name),
                    ("value", Value(key.Outcome)), ("count", counts[key]));
            }
            using var buffer = new MemoryStream();
            using (var writer = new System.Text.Json.Utf8JsonWriter(buffer)) Write(writer, row);
            if (rows.Count != 0 && bytes + buffer.Length > 1024 * 1024) break;
            rows.Add(row);
            bytes += buffer.Length;
            if (witnessRows) witnesses.Dequeue();
            else counts.Remove(order.Dequeue());
        }
        return Dict(("rows", rows), ("done", (witnessRows ? witnesses.Count : order.Count) == 0));
    }
}

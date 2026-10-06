using System.Text;
using System.Text.Json;
using static H4Comparison.Operands;
using static H4Comparison.ReportIntegrityOperands;

namespace H4Comparison;

internal sealed class ReportIntegrity
{
    private readonly ReportIntegrityOperands operands = new();
    private Task<List<object?>>? work;
    private List<object?>? output;
    private string? mode;
    private int offset;

    public object? Handle(string op, object? message)
    {
        if (op == "integrity-cancel") { operands.Cancel(); work = null; return null; }
        if (op == "integrity-drain") return Drain();
        if (op == "integrity-read") operands.Reply(message);
        else
        {
            if (work is not null || output is not null) throw new InvalidDataException("Report integrity already started");
            mode = op switch { "integrity-required" => "required", "integrity-check" => "check",
                _ => throw new InvalidDataException("Unknown report integrity operation") };
            operands.Reader.Start();
            work = Run(operands.Prepare(At(message, "operands"))).AsTask();
        }
        if (work is null) throw new InvalidDataException("Report read without suspended operation");
        if (!work.IsCompleted) return operands.Reader.Request;
        try { output = work.GetAwaiter().GetResult(); }
        finally { work = null; }
        return Drain();
    }

    private object Drain()
    {
        if (output is null) throw new InvalidDataException("Report result is not ready");
        var start = offset; var rows = new List<object?>(); var bytes = 0;
        while (offset < output.Count && rows.Count < 256)
        {
            var row = output[offset]; var size = Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(row));
            if (rows.Count != 0 && bytes + size > 1024 * 1024) break;
            rows.Add(row); bytes += size; offset++;
        }
        return Dict(("mode", mode), ("rows", rows), ("offset", start), ("total", output.Count), ("done", offset == output.Count));
    }

    private async ValueTask<List<object?>> Run(object? message)
    {
        var reference = At(message, "reference");
        if (mode == "required")
        {
            var rows = new List<object?>();
            foreach (var family in await RequiredObligations.Build(At(message, "variant"), reference, operands))
            {
                rows.Add(Dict(("parent", family.Parent)));
                rows.AddRange(family.Children.Select(child => (object?)Dict(("child", child))));
            }
            return rows;
        }
        return await Check(At(message, "report"), reference);
    }

    private async ValueTask<List<object?>> Check(object? report, object? reference)
    {
        var errors = new List<object?>();
        if (!await operands.Same(GetField(report, "comparisonScope", "full"), "full"))
            errors.Add("selected-scope report cannot satisfy full H4 obligations");
        var assertions = GetField(report, "assertions", new List<object?>());
        var parents = GetField(report, "coverageObligations", new List<object?>());
        var required = new List<object?>(); var historical = new List<object?>();
        await foreach (var row in operands.Rows(assertions))
            if (!await operands.Same(GetField(row, "applicability"), "historical-diagnostic")) required.Add(row);
        await foreach (var row in operands.Rows(assertions))
            if (await operands.Same(GetField(row, "applicability"), "historical-diagnostic")) historical.Add(row);
        var names = new Counter(); var parentNames = new Counter();
        await foreach (var row in operands.Rows(assertions)) names.Add(GetField(row, "assertion"));
        await foreach (var row in operands.Rows(parents)) parentNames.Add(GetField(row, "assertion"));
        foreach (var (name, count) in names.Items)
            if (count != 1) errors.Add("duplicate assertion: " + Text(name));
        foreach (var (name, count) in parentNames.Items)
            if (count != 1) errors.Add("duplicate coverage parent: " + Text(name));
        foreach (var family in await RequiredObligations.Build(GetField(report, "variant"), reference, operands))
        {
            if (family.Parent is not null && parentNames.Count(family.Parent) != 1)
                errors.Add("missing required family: " + family.Parent);
            foreach (var name in family.Children)
            {
                var count = 0;
                foreach (var row in required)
                    if (await operands.Same(GetField(row, "assertion"), name)
                        && await operands.Same(GetField(row, "parent"), family.Parent)) count++;
                if (count != 1) errors.Add("missing required child: " + name);
            }
        }
        await foreach (var row in operands.Rows(assertions))
        {
            var applicability = GetField(row, "applicability");
            if (!await operands.Same(applicability, "applicable")
                && !await operands.Same(applicability, "required-unobserved")
                && !await operands.Same(applicability, "historical-diagnostic"))
                errors.Add("invalid applicability: " + Text(GetField(row, "assertion")));
            var actual = GetField(row, "actualValue");
            var expected = actual is null || await operands.Same(applicability, "required-unobserved")
                ? "Unavailable" : await operands.Same(GetField(row, "expected"), actual) ? "PASS" : "FAIL";
            if (!await operands.Same(GetField(row, "result"), expected))
                errors.Add("inconsistent assertion result: " + Text(GetField(row, "assertion")));
            var parent = GetField(row, "parent");
            if (await operands.TruthValue(parent) && parentNames.Count(await operands.Field(row, "parent")) != 1)
                errors.Add(Concat("missing assertion parent: ", await operands.Field(row, "parent")));
        }
        await foreach (var parent in operands.Rows(parents))
        {
            var children = new List<object?>(); var old = new List<object?>();
            foreach (var row in required)
                if (await operands.Same(GetField(row, "parent"), GetField(parent, "assertion"))) children.Add(row);
            foreach (var row in historical)
                if (await operands.Same(GetField(row, "parent"), GetField(parent, "assertion"))) old.Add(row);
            var counts = new Counter();
            foreach (var row in children) counts.Add(GetField(row, "result"));
            if (!await operands.Same(GetField(parent, "children"), children.Select(row => GetField(row, "assertion")).ToList())
                || !await operands.Same(GetField(parent, "historicalChildren"), old.Select(row => GetField(row, "assertion")).ToList())
                || !await counts.Matches(GetField(parent, "actualValue"), operands)
                || !await operands.Same(GetField(parent, "result"), counts.Verdict)
                || !await operands.Same(GetField(parent, "applicability"), counts.Verdict == "Unavailable" ? "required-unobserved" : "applicable"))
                errors.Add("inconsistent coverage parent: " + Text(GetField(parent, "assertion")));
        }
        var total = new Counter();
        foreach (var row in required) total.Add(GetField(row, "result"));
        var summary = !await total.Matches(GetField(report, "counts"), operands)
            || !await operands.Same(GetField(report, "result"), total.Verdict)
            || GetField(report, "milestonePass") is not bool milestone || milestone != (total.Verdict == "PASS");
        if (!summary)
        {
            var history = new Counter();
            foreach (var row in historical) history.Add(GetField(row, "result"));
            summary = !await history.Matches(GetField(report, "historicalCounts"), operands);
        }
        if (summary) errors.Add("inconsistent report summary");
        return errors;
    }
}

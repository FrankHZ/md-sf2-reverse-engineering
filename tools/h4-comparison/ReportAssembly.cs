using System.Numerics;
using System.Text.Json;
using static H4Comparison.Operands;
using static H4Comparison.ReportIntegrityOperands;

namespace H4Comparison;

// One assembly session; Python remains the sole assertion/group storage owner.
internal sealed class ReportAssembly
{
    private ReportIntegrityOperands operands = new();
    private Task<object?>? work;
    private string? owner, phase, result, reason;
    private bool changedApplicability, useOriginal, useFile, hasParent, useMissing, copyReason;
    private Counter? counts;
    private List<object?>? countRows;
    private int countOffset;

    public object? Handle(string op, object? message)
    {
        if (op == "assembly-cancel") { operands.Cancel(); work = null; operands = new(); return null; }
        if (op == "assembly-counts-drain") return CountPage();
        if (op == "assembly-read") operands.Reply(message);
        else
        {
            if (work is not null) throw new InvalidDataException("Assembly operation still suspended");
            operands = new();
            operands.Reader.Start();
            work = Run(op, operands.Prepare(At(message, "operands"))).AsTask();
        }
        if (work is null) throw new InvalidDataException("Assembly read without operation");
        if (!work.IsCompleted) return operands.Reader.Request;
        try { return work.GetAwaiter().GetResult(); }
        finally { work = null; operands = new(); }
    }

    private object Step(string next)
    {
        phase = next;
        return Dict(("step", next));
    }
    private object AfterReason() => Step(result is null ? "compare" : "original");
    private static bool TruthFact(object? fact) => fact is Dictionary<string, object?> map && map.ContainsKey("length")
        ? !Equal(At(fact, "length"), BigInteger.Zero)
        : fact is Dictionary<string, object?> scalar && scalar.ContainsKey("scalar") ? Truth(At(fact, "scalar"))
        : Equal(At(fact, "object"), true);
    private static bool Starts(object? value, string prefix) => value is string text
        ? text.StartsWith(prefix, StringComparison.Ordinal)
        : throw new OperandError("AttributeError", $"'{TypeName(value)}' object has no attribute 'startswith'");

    private async ValueTask<object?> Run(string op, object? input)
    {
        if (op == "assembly-start")
        {
            if (owner is not null) throw new InvalidDataException("Assembly already started");
            owner = (string)At(input, "owner")!;
            return Step("ready");
        }
        if (owner is null) throw new InvalidDataException("Assembly not started");
        if (op == "assembly-admit")
        {
            var historical = await operands.Same(await operands.Field(At(input, "row"), "applicability"), "historical-diagnostic");
            return historical == (bool)At(input, "historical")!;
        }
        if (op == "assembly-counts")
        {
            counts = new Counter(); countRows = null; countOffset = 0;
            var historical = (bool)At(input, "historical")!;
            await foreach (var row in operands.Rows(At(input, "rows")))
            {
                if (historical && !await operands.Same(await operands.Field(row, "applicability"), "historical-diagnostic")) continue;
                counts.Add(await operands.Field(row, "result"));
            }
            countRows = counts.Items.Select(pair => (object?)Dict(("key", KeyFact(pair.Name)), ("count", pair.Count))).ToList();
            return CountPage();
        }
        if (op == "assembly-parent")
        {
            if (counts is null || countRows is null || countOffset != countRows.Count)
                throw new InvalidDataException("Parent before complete required counts");
            var name = At(input, "name");
            var record = Concat("assertions[parent=", name) + "]";
            return Dict(("layer", null), ("assertion", name),
                ("applicability", counts.Verdict == "Unavailable" ? "required-unobserved" : "applicable"),
                ("original", Dict(("owner", owner), ("binding", "required reached winning-profile child obligations"))),
                ("actual", Dict(("file", null), ("record", record))),
                ("expected", "all applicable required children PASS"), ("actualValue", null), ("result", counts.Verdict),
                ("reason", "Closed required child set is owned by the continuous contract; "
                    + "candidate input equality cannot substitute for missing actual/source bindings"),
                ("children", null), ("historicalChildren", null));
        }
        if (op == "assembly-check")
        {
            reason = null; copyReason = true; useOriginal = useFile = hasParent = useMissing = false;
            var valueNull = (bool)At(input, "valueNull")!;
            var applicability = At(input, "applicability");
            changedApplicability = valueNull && await operands.Same(applicability, "applicable");
            result = valueNull || await operands.Same(applicability, "required-unobserved") ? "Unavailable" : null;
            return changedApplicability ? Step("reason") : AfterReason();
        }
        if (op != "assembly-" + phase) throw new InvalidDataException("Assembly check phase changed");
        switch (phase)
        {
            case "reason":
                return await operands.Same(At(input, "reason"), "semantic assertion") ? Step("reason-location") : AfterReason();
            case "reason-location":
                if (IsDeferred(At(input, "location"))) return Step("reason-storage");
                reason = Concat("Actual field absent at ", At(input, "location")); copyReason = false;
                return AfterReason();
            case "reason-storage": return AfterReason();
            case "compare":
                // Deliberately value == expected; integrity's opposite operand order is separate.
                result = await operands.Same(At(input, "value"), At(input, "expected")) ? "PASS" : "FAIL";
                return Step("original");
            case "original": useOriginal = TruthFact(input); return Step("file");
            case "file": useFile = TruthFact(input); return Step(useFile ? "parent-truth" : "location");
            case "location":
                var location = At(input, "location");
                phase = "path";
                return Dict(("step", phase), ("path", Starts(location, "outcome.") ? "outcome"
                    : Starts(location, "host-log") ? "host-log" : "actual"));
            case "path": return Step("parent-truth");
            case "parent-truth":
                hasParent = TruthFact(input);
                return result == "Unavailable" ? Step("missing") : Assertion();
            case "missing": useMissing = TruthFact(input); return Assertion();
            case "append": phase = "ready"; return TruthFact(input);
            default: throw new InvalidDataException("Unknown assembly operation");
        }
    }

    private object Assertion()
    {
        var row = Dict(("layer", null), ("assertion", null),
            ("applicability", changedApplicability ? "required-unobserved" : null),
            ("original", useOriginal ? null : Dict(("owner", owner), ("commit", "87528953c6ee6d2631e24663a6d2e846be4c22ce"))),
            ("actual", Dict(("file", null), ("record", null))), ("expected", null), ("actualValue", null),
            ("result", result), ("reason", copyReason ? null : reason));
        var copy = new List<object?> { "layer", "assertion", "expected", "actualValue", "actual.file", "actual.record" };
        if (!changedApplicability) copy.Add("applicability");
        if (useOriginal) copy.Add("original");
        if (copyReason) copy.Add("reason");
        if (hasParent) { row["parent"] = null; copy.Add("parent"); }
        if (result == "Unavailable")
        {
            row["missingSide"] = useMissing ? null : "actual";
            if (useMissing) copy.Add("missingSide");
        }
        phase = "append";
        return Dict(("step", phase), ("row", row), ("copy", copy));
    }

    private object CountPage()
    {
        if (counts is null || countRows is null) throw new InvalidDataException("Assembly counts not ready");
        var start = countOffset; var rows = new List<object?>(); var bytes = 0;
        while (countOffset < countRows.Count && rows.Count < 256)
        {
            var row = countRows[countOffset];
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream)) Write(writer, row);
            var size = checked((int)stream.Length);
            if (rows.Count != 0 && bytes + size > 1024 * 1024) break;
            rows.Add(row); bytes += size; countOffset++;
        }
        return Dict(("rows", rows), ("offset", start), ("total", countRows.Count), ("done", countOffset == countRows.Count),
            ("result", counts.Verdict), ("milestonePass", counts.Verdict == "PASS"));
    }
}

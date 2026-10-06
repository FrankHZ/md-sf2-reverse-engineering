using System.Numerics;
using System.Text.Json;
using static H4Comparison.Operands;

namespace H4Comparison;

// The only external operations are the fixed inventory writes in h4_inventory.
// Callbacks below are local check continuations, never wire-supplied executable work.
internal sealed record InventoryScope(int Id, string[] Families, string Name, int Parent = -1);
internal sealed record InventoryStep(object Payload, InventoryScope Scope, Action<object?>? Accepted = null);

internal sealed class InventoryBatch(CountedChecks checks)
{
    private IEnumerator<InventoryStep>? reader;
    private readonly Queue<InventoryStep> retained = [];
    private readonly List<InventoryStep> pending = [];
    private readonly HashSet<int> skipped = [];
    private long sequence;
    private bool exhausted = true;
    private bool awaiting;

    public static InventoryStep Error(OperandError error, InventoryScope scope) => new(
        Dict(("kind", "operand-error"), ("error", error.Kind), ("message", error.Detail)), scope);

    public static IEnumerable<InventoryStep> Guard(Func<IEnumerable<InventoryStep>> body, InventoryScope scope)
    {
        IEnumerator<InventoryStep>? iterator = null;
        OperandError? failure = null;
        try { iterator = body().GetEnumerator(); }
        catch (OperandError error) { failure = error; }
        using (iterator)
        {
            while (failure is null && iterator is not null)
            {
                InventoryStep? step = null;
                var more = false;
                try { more = iterator.MoveNext(); if (more) step = iterator.Current; }
                catch (OperandError error) { failure = error; }
                if (!more) break;
                yield return step!;
            }
        }
        if (failure is not null) yield return Error(failure, scope);
    }

    public object Begin(IEnumerable<InventoryStep> steps)
    {
        if (awaiting || !exhausted || retained.Count != 0) throw new InvalidDataException("Inventory input before acknowledgement");
        reader?.Dispose(); reader = steps.GetEnumerator(); exhausted = false;
        skipped.Clear();
        return Drain();
    }

    private object Drain(bool aborted = false)
    {
        pending.Clear();
        long bytes = 0;
        if (!aborted)
            while (pending.Count < 256)
            {
                InventoryStep step;
                if (retained.Count > 0) step = retained.Dequeue();
                else
                {
                    if (exhausted || reader is null) break;
                    if (!reader.MoveNext()) { exhausted = true; break; }
                    step = reader.Current;
                }
                if (skipped.Contains(step.Scope.Id) || skipped.Contains(step.Scope.Parent)) continue;
                using var buffer = new MemoryStream();
                using (var writer = new Utf8JsonWriter(buffer)) Write(writer, step.Payload, inventoryNumbers: true);
                if (pending.Count > 0 && bytes + buffer.Length > 1024 * 1024)
                { retained.Enqueue(step); break; }
                bytes += buffer.Length; pending.Add(step);
            }
        awaiting = pending.Count > 0;
        return Dict(("batch", ++sequence), ("rows", pending.Select(s => s.Payload).ToList()),
            ("done", !awaiting && exhausted), ("aborted", aborted));
    }

    public object Acknowledge(object? message)
    {
        if (!awaiting || !Equal(At(message, "batch"), new BigInteger(sequence)))
            throw new InvalidDataException("Stale or out-of-order inventory acknowledgement");
        var results = At(message, "results") as List<object?>
            ?? throw new InvalidDataException("Inventory acknowledgement results");
        var failure = At(message, "failure");
        if (results.Count > pending.Count || (failure is null ? results.Count != pending.Count : results.Count >= pending.Count))
            throw new InvalidDataException("Inventory acknowledgement prefix cardinality");
        // Validate the entire receipt before publishing any check from it.
        for (var i = 0; i < results.Count; i++) ValidateResult(pending[i], results[i]);
        string? category = null;
        if (failure is not null)
        {
            if (failure is not Dictionary<string, object?> fields || fields.Count != 2
                || Get(fields, "message") is not string || Get(fields, "category") is not string name
                || name is not ("KeyError" or "IndexError" or "TypeError" or "ValueError" or "fatal"))
                throw new InvalidDataException("Invalid inventory failure acknowledgement");
            category = name;
            var failedKind = At(pending[results.Count].Payload, "kind");
            if (Equal(failedKind, "check")) throw new InvalidDataException("Check has no storage failure operation");
            if (Equal(failedKind, "operand-error"))
            {
                var expected = At(pending[results.Count].Payload, "error") as string;
                if (category != (expected is "KeyError" or "IndexError" or "TypeError" or "ValueError" ? expected : "fatal"))
                    throw new InvalidDataException("Impossible inventory operand error acknowledgement");
            }
        }
        foreach (var (step, result) in pending.Take(results.Count).Zip(results)) step.Accepted?.Invoke(result);
        if (failure is not null)
        {
            var failed = pending[results.Count];
            if (category == "fatal" || failed.Scope.Families.Length == 0)
            {
                exhausted = true; reader?.Dispose(); reader = null; retained.Clear();
                return Drain(aborted: true);
            }
            checks.Error(failed.Scope.Families, failed.Scope.Name, category!, BigInteger.One);
            skipped.Add(failed.Scope.Id);
            // Preserve unattempted operations; only the failed evaluated scope ends.
            var suffix = pending.Skip(results.Count + 1).Concat(retained).ToArray();
            retained.Clear(); foreach (var step in suffix) retained.Enqueue(step);
        }
        return Drain();
    }

    private static void ValidateResult(InventoryStep step, object? result)
    {
        var kind = Get(step.Payload, "kind");
        if (Equal(kind, "operand-error")) throw new InvalidDataException("Operand error acknowledged as success");
        if (kind is "layer" or "tile" or "entity" or "portrait")
        {
            if (result is not Dictionary<string, object?> row || row.Count != 3
                || Get(row, "fresh") is not bool || Get(row, "present") is not bool
                || Get(row, "key") is not List<object?>)
                throw new InvalidDataException("Inventory membership receipt");
        }
        else if (result is not null) throw new InvalidDataException("Inventory write receipt");
    }
}

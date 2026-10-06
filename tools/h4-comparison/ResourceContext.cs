using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.ResourceOperands;
using static H4Comparison.SceneOperands;

namespace H4Comparison;

internal sealed class ResourceContext(CountedChecks checks, IEnumerable<object?> enabled, int digitLimit)
{
    private readonly Dictionary<Key, object?> programs = [];
    private readonly Dictionary<Key, (object? Sequence, object? Map)> visits = [];
    private readonly HashSet<Key> sessions = [];
    private readonly List<object?> order = [];
    private IEnumerator<(object? Sequence, object? Map)>? visitReader;
    private object? pendingVisit;
    private bool hasPendingVisit;
    private readonly IEnumerator<object?> families = enabled.GetEnumerator();
    private string? pendingFamily;
    private bool projection;
    private bool projectionKnown;

    public int SessionCount => sessions.Count;
    public bool HasSession(object? value) => sessions.Contains(HashKey(value));

    public void Program(object? row) => programs[HashKey(Field(row, "id"))] = row;

    private void Visit(object? sequence, object? map)
    {
        var key = HashKey(sequence);
        visits[key] = visits.TryGetValue(key, out var old) ? (old.Sequence, map) : (sequence, map);
    }

    public void StartMap(object? map) => Visit(BigInteger.Zero, map);

    public void Warp(object? row)
    {
        foreach (var entry in Items(Default(Default(row, "result", Dict()), "observations", new List<object?>())))
        {
            if (Equal(Get(entry, "Kind"), "map-transferred"))
            {
                var map = Field(entry, "Detail");
                Visit(Field(entry, "Sequence"), map);
            }
            else if (Equal(Get(entry, "Detail"), "LoadSceneMap") && Truth(Get(entry, "Program")))
            {
                var loc = Field(entry, "Program");
                var key = HashKey(Field(loc, "Program"));
                if (!programs.TryGetValue(key, out var program)) throw new OperandError("KeyError");
                var instructions = Field(program, "instructions");
                var instruction = Integer(Field(loc, "Instruction"), digitLimit);
                var map = Field(ResourceOperands.Index(instructions, instruction), "map");
                Visit(Field(entry, "Sequence"), map);
            }
        }
    }

    public void FinishWarps()
    {
        programs.Clear();
        visitReader = visits.Values.GetEnumerator();
    }

    public object VisitKeys()
    {
        if (visitReader is null) throw new InvalidDataException("Resource visits not prepared");
        List<object?> rows = [];
        // Keys are scalar hashable JSON operands; batches also cap encoded bytes.
        long bytes = 0;
        while (rows.Count < 256)
        {
            if (!hasPendingVisit)
            {
                if (!visitReader.MoveNext()) return Dict(("rows", rows), ("done", true));
                pendingVisit = visitReader.Current.Sequence;
                hasPendingVisit = true;
            }
            using var buffer = new MemoryStream();
            using (var writer = new System.Text.Json.Utf8JsonWriter(buffer)) Write(writer, pendingVisit);
            if (rows.Count > 0 && bytes + buffer.Length > 1024 * 1024) break;
            bytes += buffer.Length;
            rows.Add(pendingVisit);
            hasPendingVisit = false;
        }
        return Dict(("rows", rows), ("done", false));
    }

    public void OrderedVisit(object? key) => order.Add(key);

    public (object? Sequence, object? Map) Latest(object? sequence)
    {
        var lo = 0; var hi = order.Count;
        while (lo < hi)
        {
            var middle = (lo + hi) / 2;
            if (Less(sequence, order[middle])) hi = middle;
            else lo = middle + 1;
        }
        var visit = lo > 0 ? order[lo - 1] : null;
        return (visit, visits.TryGetValue(HashKey(visit), out var found) ? found.Map : null);
    }

    public void Session(object? row)
    {
        var state = Default(row, "state", Dict());
        var session = Get(state, "sessionId");
        if (Truth(session)) sessions.Add(HashKey(session));
    }

    public bool BeginScope(object? scope)
    {
        if (scope is null) return false;
        foreach (var session in Items(Field(scope, "contextSessions"))) sessions.Add(HashKey(session));
        return NextScope();
    }

    private bool NextScope()
    {
        while (families.MoveNext())
        {
            var family = (string)families.Current!;
            checks.Check(family, "selected scope retains independent session context",
                sessions.Count > 0 ? true : null, BigInteger.One);
            if (family is not ("map" or "entity")) continue;
            pendingFamily = family;
            if (!projectionKnown) return true;
            ProjectionCheck();
        }
        return false;
    }

    public bool Projection(object? row)
    {
        projection = projection || Truth(Get(Default(row, "state", Dict()), "cameraProjection"));
        return projection;
    }

    private void ProjectionCheck() => checks.Check(pendingFamily!,
        "selected scope retains independent current projection context", projection ? true : null, BigInteger.One);

    public void FinishScope()
    {
        projectionKnown = true;
        ProjectionCheck();
        NextScope();
    }

    public void Release()
    {
        programs.Clear(); visits.Clear(); sessions.Clear(); order.Clear();
        pendingVisit = null;
        visitReader?.Dispose(); families.Dispose();
    }
}

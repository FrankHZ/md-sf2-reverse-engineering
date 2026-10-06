using System.Text.Json;
using static H4Comparison.Operands;

namespace H4Comparison;

// Material-origin reports retain every ordered row. They are not counted resource reports.
internal sealed class MaterialReport
{
    private readonly Queue<object?> events = [];
    private long sequence;
    private int offset;
    private int total;
    private object? control;
    private object? failure;
    public object? Scene { get; set; }
    public object? Audio { get; set; }
    public object? ActorWeapon { get; set; }

    public void Begin(bool discard = false)
    {
        if (events.Count != 0 && !discard) throw new InvalidDataException("Material result not drained");
        events.Clear(); sequence++; offset = total = 0; control = failure = null;
    }
    public object? Check(string name, object? value, object? source)
    {
        events.Enqueue(Dict(("channel", "checks"), ("row", Dict(("name", name), ("value", value), ("source", source)))));
        return value;
    }
    public void Join(object row) => events.Enqueue(Dict(("channel", "joins"), ("row", row)));
    public static object? Combine(object? left, object? right) => Equal(left, false) || Equal(right, false)
        ? false : left is null || right is null ? null : true;
    public object Complete(object? value = null, object? error = null)
    {
        control = value; failure = error; total = events.Count;
        return Page();
    }
    public object Page()
    {
        List<object?> rows = []; long bytes = 0;
        var first = offset;
        while (events.Count > 0 && rows.Count < 256)
        {
            var row = events.Peek();
            using var buffer = new MemoryStream();
            using (var writer = new Utf8JsonWriter(buffer)) Write(writer, row, inventoryNumbers: true);
            if (rows.Count > 0 && bytes + buffer.Length > 1024 * 1024) break;
            bytes += buffer.Length; rows.Add(events.Dequeue()); offset++;
        }
        return Dict(("sequence", sequence), ("offset", first), ("total", total), ("rows", rows),
            ("done", events.Count == 0), ("scene", Scene), ("audio", Audio), ("actorWeapon", ActorWeapon),
            ("control", control), ("failure", failure));
    }
    public void Error(object? error)
    {
        var kind = At(error, "kind");
        if (kind is "FileNotFoundError" or "CalledProcessError")
            Check("selected material input availability", null, "explicit selected inputs");
        else if (Equal(kind, "AssetPreflightError"))
        {
            var code = MaterialOperands.Text(At(error, "code"));
            var unavailable = code is "RepositoryUnavailable" or "PayloadUnavailable" or "SchemaUnavailable";
            Check("asset checkout " + code, unavailable ? null : false, At(error, "field"));
            Audio = unavailable ? null : false;
        }
        else if (Equal(kind, "KeyError")) Check("selected material field availability", null, "explicit selected inputs");
        else if (kind is "ValueError" or "WaveError") Check("selected material evidence shape", false, At(error, "name"));
        else throw new InvalidDataException("Unknown material error classification");
    }
}

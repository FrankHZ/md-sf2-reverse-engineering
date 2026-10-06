using System.Text;
using System.Text.Json;
using H4Comparison;

Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);
ResourceComparison? comparison = null;
CountedChecks? report = null;
SceneObservations? scenes = null;
ResourceContext? context = null;
ResourceValidation? validation = null;
ResourceInventory? inventory = null;
try
{
    while (Console.ReadLine() is { } line)
    {
        using var document = JsonDocument.Parse(line);
        var message = Operands.Read(document.RootElement);
        Operands.RestoreNonFinite(message);
        object? result;
        if (Operands.Equal(Operands.At(message, "op"), "sources"))
        {
            if (comparison is not null) throw new InvalidDataException("Resource sources already selected");
            comparison = new ResourceComparison();
            result = null;
        }
        else
        {
            if (comparison is null) throw new InvalidDataException("Resource sources not selected");
            try
            {
                result = Operands.At(message, "op") switch
                {
                    "report-start" => StartReport(message),
                    "report-records" => RecordChecks(message),
                    "scene-start" => StartScenes(message),
                    "scene-rows" => RecordScenes(message),
                    "report-finish" => RequiredReport().Finish(),
                    "report-checks" => RequiredReport().Drain(false),
                    "report-witnesses" => RequiredReport().Drain(true),
                    string op when op.StartsWith("identity-", StringComparison.Ordinal) => IdentityOperation(op, message),
                    string op when op.StartsWith("inventory-", StringComparison.Ordinal) => InventoryOperation(op, message),
                    _ => comparison.Handle(message)
                };
            }
            catch (OperandError e)
            {
                // Fatal Python operand errors still propagate at the Python caller.
                Reply(e.Detail is null ? Operands.Dict(("operandError", e.Kind))
                    : Operands.Dict(("operandError", e.Kind), ("operandMessage", e.Detail)));
                continue;
            }
        }
        Reply(Operands.Dict(("result", result)),
            ((string)Operands.At(message, "op")!).StartsWith("inventory-", StringComparison.Ordinal));
    }
    return 0;
}

catch (Exception e)
{
    Console.Error.WriteLine($"H4 resource process failed: {e.GetType().Name}: {e.Message}");
    return 1;
}

CountedChecks RequiredReport() => report ?? throw new InvalidDataException("Resource report not started");
object? StartReport(object? message)
{
    if (report is not null) throw new InvalidDataException("Resource report already started");
    report = new CountedChecks(Operands.Iterate(Operands.At(message, "enabled")));
    return null;
}
object? RecordChecks(object? message)
{
    foreach (var record in Operands.Iterate(Operands.At(message, "rows"))) RequiredReport().Accept(record);
    return null;
}
object? StartScenes(object? message)
{
    scenes = new SceneObservations(RequiredReport(), Operands.At(message, "scene"),
        (int)(System.Numerics.BigInteger)Operands.At(message, "integerDigitLimit")!);
    RequiredReport().Check("scene", "reached scene observation channel",
        Operands.Truth(Operands.At(message, "present")) ? true : null, System.Numerics.BigInteger.One);
    return null;
}
object? RecordScenes(object? message)
{
    if (scenes is null) throw new InvalidDataException("Resource scene definition not selected");
    foreach (var row in Operands.Iterate(Operands.At(message, "rows"))) scenes.Accept(row);
    return null;
}

object? IdentityOperation(string op, object? message)
{
    if (op == "identity-start")
    {
        if (context is not null) throw new InvalidDataException("Resource identity already started");
        var limit = (int)(System.Numerics.BigInteger)Operands.At(message, "integerDigitLimit")!;
        context = new ResourceContext(RequiredReport(), Operands.Iterate(Operands.At(message, "enabled")), limit);
        validation = new ResourceValidation(RequiredReport(), context, limit);
        return null;
    }
    if (context is null || validation is null) throw new InvalidDataException("Resource identity not started");
    switch (op)
    {
        case "identity-map": context.StartMap(Operands.At(message, "map")); return null;
        case "identity-warp-finish": context.FinishWarps(); return null;
        case "identity-visit-keys": return context.VisitKeys();
        case "identity-scope": return context.BeginScope(Operands.At(message, "scope"));
        case "identity-scope-finish": context.FinishScope(); return null;
        case "identity-release": context.Release(keepOrder: true); return null;
    }
    List<object?> admitted = [];
    var projected = false;
    foreach (var record in Operands.Iterate(Operands.At(message, "rows")))
    {
        switch (op)
        {
            case "identity-programs": context.Program(record); break;
            case "identity-warps": context.Warp(record); break;
            case "identity-visit-order": context.OrderedVisit(record); break;
            case "identity-sessions": context.Session(record); break;
            case "identity-projections": projected = context.Projection(record); break;
            case "identity-requirements":
                admitted.Add(validation.Available(record, true, System.Numerics.BigInteger.One, null)); break;
            case "identity-requirement-checks": validation.Identity(record, System.Numerics.BigInteger.One, null); break;
            case "identity-uses-occurrence":
            case "identity-uses-checks":
            case "identity-textures":
                var row = Operands.At(record, "row");
                var weight = (System.Numerics.BigInteger)Operands.At(record, "weight")!;
                var locator = Operands.At(row, "_captureLocator");
                if (op == "identity-uses-occurrence") validation.Available(row, false, weight, locator);
                else if (op == "identity-uses-checks") validation.Identity(row, weight, locator);
                else validation.Texture(row, weight, locator, Operands.At(record, "membership"));
                break;
            default: throw new InvalidDataException("Unknown resource identity operation");
        }
        if (projected) break;
    }
    return op == "identity-requirements" ? admitted : op == "identity-projections" ? projected : null;
}

object? InventoryOperation(string op, object? message)
{
    if (op == "inventory-start")
    {
        if (inventory is not null || context is null) throw new InvalidDataException("Inventory lifetime");
        inventory = new ResourceInventory(RequiredReport(), context, message);
        inventory.Start(); return null;
    }
    if (inventory is null) throw new InvalidDataException("Inventory not started");
    switch (op)
    {
        case "inventory-sources": inventory.Sources(Operands.At(message, "rows")); return null;
        case "inventory-coverage": inventory.Coverage(Operands.At(message, "complete")); return null;
        case "inventory-finish": inventory.Finish(Operands.At(message, "any")); return null;
        case "inventory-rows": return inventory.Rows((string)Operands.At(message, "channel")!, Operands.At(message, "rows"));
        case "inventory-ack": return inventory.Acknowledge(message);
        default: throw new InvalidDataException("Unknown inventory operation");
    }
}

static void Reply(object? result, bool inventoryNumbers = false)
{
    using var buffer = new MemoryStream();
    using (var writer = new Utf8JsonWriter(buffer)) Operands.Write(writer, result, inventoryNumbers);
    Console.WriteLine(Encoding.UTF8.GetString(buffer.ToArray()));
}

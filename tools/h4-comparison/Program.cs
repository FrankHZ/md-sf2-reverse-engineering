using System.Text;
using System.Text.Json;
using H4Comparison;

Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);
ResourceComparison? comparison = null;
CountedChecks? report = null;
SceneObservations? scenes = null;
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
                    _ => comparison.Handle(message)
                };
            }
            catch (OperandError e)
            {
                // Fatal Python operand errors still propagate at the Python caller.
                Reply(Operands.Dict(("operandError", e.Kind)));
                continue;
            }
        }
        Reply(Operands.Dict(("result", result)));
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

static void Reply(object? result)
{
    using var buffer = new MemoryStream();
    using (var writer = new Utf8JsonWriter(buffer)) Operands.Write(writer, result);
    Console.WriteLine(Encoding.UTF8.GetString(buffer.ToArray()));
}

using System.Text;
using System.Text.Json;
using H4Comparison;

Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);
ResourceComparison? comparison = null;
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
            try { result = comparison.Handle(message); }
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

static void Reply(object? result)
{
    using var buffer = new MemoryStream();
    using (var writer = new Utf8JsonWriter(buffer)) Operands.Write(writer, result);
    Console.WriteLine(Encoding.UTF8.GetString(buffer.ToArray()));
}

using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.ResourceOperands;
using static H4Comparison.VisualSourceOperands;

namespace H4Comparison;

internal sealed class FieldDeathSource(CountedChecks checks, object? pins)
{
    private bool valid;
    public async ValueTask<object?> Handle(string op, object? message, VisualSourceOperands operands)
    {
        switch (op)
        {
            case "visual-field-start":
                var field = At(message, "field");
                valid = await operands.Same(await operands.Field(field, "upstreamCommit"), At(pins, "upstream"))
                    && await operands.Same(await operands.Field(field, "romSha256"), At(pins, "rom")) && await operands.Same(await operands.Field(field, "effectSprite"), new BigInteger(63)); return null;
            case "visual-field-allies":
                var expected = new List<object?>();
                foreach (var row in Items(At(message, "expected")))
                    expected.Add(Dict(("character", await operands.Field(row, "character")), ("sprite", await operands.Field(row, "sprite"))));
                valid &= await operands.Same(At(message, "actual"), expected); return null;
            case "visual-field-enemy":
                var enemy = await operands.Field(At(message, "field"), "enemySprite");
                valid &= await operands.Same(At(message, "value"), enemy) && await operands.Same(enemy, new BigInteger(103)); return null;
            case "visual-field-span": valid &= await operands.Same(At(message, "span"), At(message, "facts")); return null;
            case "visual-field-bytes":
                var bytes = BytesEqual(At(message, "measurement")); valid &= bytes; return bytes;
            case "visual-field-digest": valid &= await operands.Same(At(message, "digest"), await operands.Field(At(message, "raster"), "sha256")); return null;
            case "visual-field-finish":
                checks.Check("scene", "field death original ROM spans and assignments", valid, BigInteger.One); return null;
            default: throw new InvalidDataException("Unknown field-death source operation");
        }
    }
}

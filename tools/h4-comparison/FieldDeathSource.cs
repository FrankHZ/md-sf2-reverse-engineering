using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.ResourceOperands;
using static H4Comparison.VisualSourceOperands;

namespace H4Comparison;

internal sealed class FieldDeathSource(CountedChecks checks, object? pins)
{
    private bool valid;
    public object? Handle(string op, object? message)
    {
        switch (op)
        {
            case "visual-field-start":
                var field = At(message, "field");
                valid = Equal(Field(field, "upstreamCommit"), At(pins, "upstream"))
                    && Equal(Field(field, "romSha256"), At(pins, "rom")) && Equal(Field(field, "effectSprite"), new BigInteger(63)); return null;
            case "visual-field-allies":
                var expected = Items(At(message, "expected")).Select(row => (object?)Dict(("character", Field(row, "character")), ("sprite", Field(row, "sprite")))).ToList();
                valid &= Equal(At(message, "actual"), expected); return null;
            case "visual-field-enemy":
                var enemy = Field(At(message, "field"), "enemySprite");
                valid &= Equal(At(message, "value"), enemy) && Equal(enemy, new BigInteger(103)); return null;
            case "visual-field-span": valid &= Equal(At(message, "span"), At(message, "facts")); return null;
            case "visual-field-bytes":
                var bytes = BytesEqual(At(message, "measurement")); valid &= bytes; return bytes;
            case "visual-field-digest": valid &= Equal(At(message, "digest"), Field(At(message, "raster"), "sha256")); return null;
            case "visual-field-finish":
                checks.Check("scene", "field death original ROM spans and assignments", valid, BigInteger.One); return null;
            default: throw new InvalidDataException("Unknown field-death source operation");
        }
    }
}

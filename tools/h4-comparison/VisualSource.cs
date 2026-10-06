using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.ResourceOperands;
using static H4Comparison.VisualSourceOperands;

namespace H4Comparison;

internal sealed class VisualSource(CountedChecks checks, object? pins, object? selectedMaps, int digitLimit)
{
    private bool binding = true, pin, gitPin, metadata, atlas;
    private readonly Dictionary<BigInteger, object?> definitions = [];
    private readonly FieldDeathSource field = new(checks, pins);
    private void Check(string family, string name, object? value) => checks.Check(family, name, value, BigInteger.One);

    public object? Handle(string op, object? message)
    {
        if (op.StartsWith("visual-field-", StringComparison.Ordinal)) return field.Handle(op, message);
        switch (op)
        {
            case "visual-select":
                var key = (string)At(message, "key")!;
                return Iterate(At(message, "rows")).Select(row =>
                {
                    if (selectedMaps is null) return (object?)true;
                    var name = Field(row, key); HashKey(name);
                    return Iterate(selectedMaps).Any(value => ReferenceEquals(value, name) || Equal(value, name));
                }).ToList();
            case "visual-binding":
                binding = Equal(At(message, "actual"), At(message, "expected")); return binding;
            case "visual-provenance":
                var provenance = At(message, "provenance");
                pin = Equal(Field(provenance, "commit"), At(pins, "upstream"))
                    && Equal(Field(provenance, "romSha256"), At(pins, "rom")); return null;
            case "visual-git":
                gitPin = Equal(At(message, "head"), At(pins, "upstream")) && Equal(At(message, "returncode"), BigInteger.Zero);
                return binding && pin && gitPin;
            case "visual-pins":
                var valid = binding && pin && gitPin && Equal(At(message, "digest"), At(pins, "rom"));
                foreach (var family in new[] { "map", "entity", "scene" }) Check(family, "same-run source and selection pins", valid);
                return null;
            case "visual-canonical":
                Check("map", "accepted canonical source layout/blocksets", Equal(At(message, "digest"), Field(At(message, "manifest"), "outputSha256"))); return null;
            case "visual-definitions":
                foreach (var definition in Iterate(At(message, "rows"))) definitions.Add((BigInteger)At(definition, "index")!, definition);
                return null;
            case "visual-layout":
                var row = InventoryOperands.Prepare(At(message, "row"));
                var label = AppendName("selected layout original words ", Field(row, "id"));
                var words = InventoryOperands.InventoryItems(Field(row, "layout"))
                    .SelectMany(InventoryOperands.InventoryItems).ToList();
                Check("map", label, Equal(words, Field(At(message, "layout"), "words"))); return null;
            case "visual-tileset": metadata = Equal(At(message, "digest"), At(pins, "tileset")); return metadata;
            case "visual-palette": metadata = Equal(At(message, "digest"), At(pins, "palette")); return null;
            case "visual-metadata": Check("map", "accepted private atlas metadata identities", metadata); return null;
            case "visual-atlas-bytes": atlas = BytesEqual(At(message, "measurement")); return atlas;
            case "visual-atlas-digest":
                atlas = Equal(At(message, "digest"), Field(Field(At(message, "asset"), "source"), "sha256")); return null;
            case "visual-atlas-png": atlas &= BytesEqual(At(message, "measurement")); return null;
            case "visual-atlas-finish":
                var atlasDefinition = definitions[(BigInteger)At(message, "index")!];
                Check("map", AppendName("atlas source recipe and canonical selectors ", At(message, "name")), atlas && VisualEqual(atlasDefinition)); return null;
            case "visual-word":
                var want = At(message, "want");
                var visual = At(At(definitions[(BigInteger)At(message, "index")!], "actual"), "header");
                var block = Field(want, "block");
                var matches = (Less(BigInteger.Zero, block) || Equal(BigInteger.Zero, block))
                    && Less(block, Length(Field(visual, "blocks")))
                    && Equal(Index(Index(Field(visual, "blocks"), Integer(block, digitLimit)), Integer(Field(want, "tile"), digitLimit)), Field(want, "word"));
                checks.Check("map", "required logical block/tile source word", matches,
                    (BigInteger)At(message, "weight")!, At(message, "locator")); return null;
            default: throw new InvalidDataException("Unknown visual source operation");
        }
    }
}

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
    private readonly VisualSourceOperands operands = new();
    private Task<object?>? work;
    private void Check(string family, string name, object? value) => checks.Check(family, name, value, BigInteger.One);

    public object? Handle(string op, object? message)
    {
        if (op == "visual-cancel") { operands.Cancel(); work = null; return null; }
        if (op == "visual-read") operands.Reply(message);
        else
        {
            if (work is not null) throw new InvalidDataException("Visual source operation still suspended");
            operands.Start();
            work = Core(op, operands.Prepare(At(message, "operands"))).AsTask();
        }
        if (work is null) throw new InvalidDataException("Visual source read without operation");
        if (!work.IsCompleted) return operands.Request;
        try { return work.GetAwaiter().GetResult(); }
        finally { work = null; }
    }

    private async ValueTask<object?> Core(string op, object? message)
    {
        if (op.StartsWith("visual-field-", StringComparison.Ordinal)) return await field.Handle(op, message, operands);
        switch (op)
        {
            case "visual-select":
                var key = (string)At(message, "key")!;
                var selected = new List<object?>();
                foreach (var selectedRow in Iterate(At(message, "rows")))
                {
                    if (selectedMaps is null) { selected.Add(true); continue; }
                    var name = await operands.Field(selectedRow, key); SourceKey(name);
                    selected.Add(Iterate(selectedMaps).Any(value => ReferenceEquals(value, name) || Equal(value, name)));
                }
                return selected;
            case "visual-binding":
                binding = await operands.Same(At(message, "actual"), At(message, "expected")); return binding;
            case "visual-provenance":
                var provenance = At(message, "provenance");
                pin = await operands.Same(await operands.Field(provenance, "commit"), At(pins, "upstream"))
                    && await operands.Same(await operands.Field(provenance, "romSha256"), At(pins, "rom")); return null;
            case "visual-git":
                gitPin = Equal(At(message, "head"), At(pins, "upstream")) && Equal(At(message, "returncode"), BigInteger.Zero);
                return binding && pin && gitPin;
            case "visual-pins":
                var valid = binding && pin && gitPin && Equal(At(message, "digest"), At(pins, "rom"));
                foreach (var family in new[] { "map", "entity", "scene" }) Check(family, "same-run source and selection pins", valid);
                return null;
            case "visual-canonical":
                Check("map", "accepted canonical source layout/blocksets", await operands.Same(At(message, "digest"), await operands.Field(At(message, "manifest"), "outputSha256"))); return null;
            case "visual-definitions":
                foreach (var definition in Iterate(At(message, "rows")))
                {
                    DefineVisual(definition);
                    definitions.Add((BigInteger)At(definition, "index")!, definition);
                }
                return null;
            case "visual-layout":
                var row = InventoryOperands.Prepare(At(message, "row"));
                var label = AppendName("selected layout original words ", await operands.Field(row, "id"));
                var words = InventoryOperands.InventoryItems(await operands.Field(row, "layout"))
                    .SelectMany(InventoryOperands.InventoryItems).ToList();
                Check("map", label, await operands.Same(words, await operands.Field(At(message, "layout"), "words"))); return null;
            case "visual-tileset": metadata = Equal(At(message, "digest"), At(pins, "tileset")); return metadata;
            case "visual-palette": metadata = Equal(At(message, "digest"), At(pins, "palette")); return null;
            case "visual-metadata": Check("map", "accepted private atlas metadata identities", metadata); return null;
            case "visual-atlas-bytes": atlas = BytesEqual(At(message, "measurement")); return atlas;
            case "visual-atlas-digest":
                atlas = await operands.Same(At(message, "digest"), await operands.Field(await operands.Field(At(message, "asset"), "source"), "sha256")); return null;
            case "visual-atlas-png": atlas &= BytesEqual(At(message, "measurement")); return null;
            case "visual-atlas-finish":
                var atlasDefinition = definitions[(BigInteger)At(message, "index")!];
                Check("map", AppendName("atlas source recipe and canonical selectors ", At(message, "name")), atlas && await operands.VisualEqual(atlasDefinition)); return null;
            case "visual-word":
                var want = At(message, "want");
                var visual = At(At(definitions[(BigInteger)At(message, "index")!], "actual"), "header");
                var block = await operands.Field(want, "block");
                var matches = (SourceLess(BigInteger.Zero, block) || Equal(BigInteger.Zero, block))
                    && SourceLess(block, await operands.Length(await operands.Field(visual, "blocks")))
                    && await operands.Same(await operands.Index(await operands.Index(await operands.Field(visual, "blocks"), SourceInteger(block, digitLimit)), SourceInteger(await operands.Field(want, "tile"), digitLimit)), await operands.Field(want, "word"));
                checks.Check("map", "required logical block/tile source word", matches,
                    (BigInteger)At(message, "weight")!, At(message, "locator")); return null;
            default: throw new InvalidDataException("Unknown visual source operation");
        }
    }
}

using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.ResourceOperands;
using static H4Comparison.SceneOperands;
using static H4Comparison.InventoryOperands;

namespace H4Comparison;

internal static class InventoryGeometry
{
    public static (List<object?> Tiles, bool Mutable) Layer(object? layer, string name,
        Func<object?> definition, Func<object?> blocks, int digits)
    {
        var recorded = InventoryItems(Default(layer, "overlaps", new List<object?>())).ToList();
        if (InventoryTruth(Get(layer, "first"))) recorded.Add(Field(layer, "first"));
        if (name == "occlusion") return (recorded, false);
        var source = definition();
        var events = Or(Get(source, "layoutEvents"), Dict());
        if (events is not Dictionary<string, object?> eventMap) throw new OperandError("AttributeError");
        var mutable = eventMap.Values.SelectMany(InventoryItems).Select(row => Field(row, "copy")).ToList();
        var unknown = false;
        var ox = Field(layer, "x"); var oy = Field(layer, "y");
        var startY = Integer(Floor24(oy), digits); var startX = Integer(Floor24(ox), digits);
        for (var y = startY; y < startY + 9; y++)
            for (var x = startX; x < startX + 15; x++)
            {
                var sx = Integer(Add(x, Field(layer, "offsetX")), digits);
                var sy = Integer(Add(y, Field(layer, "offsetY")), digits);
                if (!(sx >= 0 && sx < 64 && sy >= 0 && sy < 64)) continue;
                if (mutable.Any(copy => LE(Field(Field(copy, "destination"), "x"), sx)
                    && Less(sx, Add(Field(Field(copy, "destination"), "x"), Field(copy, "width")))
                    && LE(Field(Field(copy, "destination"), "y"), sy)
                    && Less(sy, Add(Field(Field(copy, "destination"), "y"), Field(copy, "height")))))
                { unknown = true; continue; }
                var block = Bits(InventoryIndex(InventoryIndex(Field(source, "layout"), sy), sx)) & 0x3FF;
                if (name.StartsWith("foreground", StringComparison.Ordinal) && block == 0) continue;
                var high = Get(layer, "highPriority");
                for (var tile = 0; tile < (high is null ? 1 : 9); tile++)
                {
                    var word = InventoryIndex(InventoryIndex(blocks(), block), tile);
                    if (high is not null && !Equal((Bits(word) & 0x8000) != 0, high)) continue;
                    var px = Add(Subtract(x * 24, ox), new BigInteger(high is null ? 0 : tile % 3 * 8));
                    var py = Add(Subtract(y * 24, oy), new BigInteger(high is null ? 0 : tile / 3 * 8));
                    var width = new BigInteger(high is null ? 24 : 8);
                    if (Less(BigInteger.Zero, Add(px, width)) && Less(BigInteger.Zero, Add(py, width))
                        && Less(px, new BigInteger(320)) && Less(py, new BigInteger(192)))
                        recorded.Add(Dict(("block", block), ("tile", new BigInteger(tile)), ("word", word)));
                }
            }
        return (recorded, unknown);
    }

    public static object Portrait(object? state, Func<object?, object?> source, int digits)
    {
        var work = Or(Get(state, "portraitWork"), Dict());
        var flags = Field(state, "portraitFlags");
        if (flags is null || Get(state, "portraitId") is null) throw new OperandError("KeyError");
        var id = Field(state, "portraitId"); var original = source(id);
        var tiles = Enumerable.Range(0, 64).Select(i => (object?)new BigInteger(i)).ToList();
        // Python evaluates both change-list expressions before iterating either one.
        var changes = new[] { InventoryTruth(Get(work, "EyesClosed")) ? Field(original, "eyes") : new List<object?>(),
            InventoryTruth(Get(work, "MouthOpen")) ? Field(original, "mouth") : new List<object?>() };
        foreach (var rows in changes)
            foreach (var row in InventoryItems(rows))
            {
                var values = InventoryItems(row).Take(5).ToList();
                if (values.Count != 4) throw new OperandError("ValueError");
                SetTile(tiles, Add(Multiply8(values[1]), values[0]), Add(Multiply8(values[3]), values[2]));
            }
        return Dict(("portrait", id), ("mirror", (Integer(flags, digits) & 0x40) != 0),
            ("eyes", InventoryTruth(Get(work, "EyesClosed"))), ("mouth", InventoryTruth(Get(work, "MouthOpen"))), ("tiles", tiles));
    }

    public static object?[] PortraitKey(object? visit, object? phase, object? want) =>
        [visit, phase, Field(want, "portrait"), Field(want, "mirror"), Field(want, "eyes"),
            Field(want, "mouth"), InventoryItems(Field(want, "tiles")).ToList()];
}

using System.Collections.ObjectModel;

namespace Sf2.Remake.Application.Runtime.Exploration;

// A color lookup over the 512 MD colors. Palette slots with equal source colors
// undergo the same operation. This keeps source-color arithmetic outside Godot.
public sealed class PaletteColorTransition
{
    private static readonly ushort[] Identity = Enumerable.Range(0, 512).Select(Word).ToArray();
    private readonly ushort[] _base;
    private readonly ushort[] _copy;
    public long StartedAt { get; }
    private PaletteColorTransition(ushort[] target, ushort[] copy, long startedAt)
    { _base = target; _copy = copy; StartedAt = startedAt; }

    public static int Index(ushort word) => ((word & 0xE00) >> 3) | ((word & 0xE0) >> 2) | ((word & 0xE) >> 1);
    public static ushort Word(int index) => (ushort)(((index & 448) << 3) | ((index & 56) << 2) | ((index & 7) << 1));
    public static bool Valid(ushort word) => (word & ~0xEEE) == 0;

    public IReadOnlyList<ushort> ColorsAt(long tick)
    {
        int timer = (int)Math.Clamp(32 - (tick - StartedAt), 0, 32);
        int t = timer >> 2;
        return new ReadOnlyCollection<ushort>(_base.Select((target, i) =>
        {
            int result = 0;
            for (int shift = 0; shift <= 8; shift += 4)
                result |= (((((_copy[i] >> shift) & 14) * t + ((target >> shift) & 14) * (8 - t)) >> 3) & 14) << shift;
            return (ushort)result;
        }).ToArray());
    }

    public static PaletteColorTransition Request(PaletteColorTransition? previous, IReadOnlyList<ushort>? lookup, long tick)
    {
        ushort[] current = previous?.ColorsAt(tick).ToArray() ?? [.. Identity];
        ushort[] target = lookup is null ? previous?._copy ?? [.. Identity] : current.Select(color => lookup[Index(color)]).ToArray();
        return new([.. target], current, tick);
    }

    // LaunchFading cancels the interpolation; DuplicatePalettes publishes BASE.
    public PaletteColorTransition SnapToBase(long tick) => new([.. _base], [.. _base], tick - 32);
}

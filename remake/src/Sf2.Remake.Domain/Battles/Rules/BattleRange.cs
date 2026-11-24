using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class BattleRange
{
    // pt_SpellRanges starts at the south tip, except radius 2 starts at the north tip.
    // PopulateTargetsList visits the outermost ring first.
    internal static (int Ring, int Index) SpellScoreOrder(MapPosition center, MapPosition target)
    {
        int x = target.X - center.X, y = target.Y - center.Y;
        int radius = Math.Abs(x) + Math.Abs(y);
        int index = y > 0 ? (x >= 0 ? x : 4 * radius + x) :
            x >= 0 ? radius - y : 2 * radius - x;
        if (radius == 2) index = (index + 4) % 8;
        return (-radius, index);
    }

    internal static bool Contains(MapPosition origin, MapPosition target, int minimum, int maximum)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentOutOfRangeException.ThrowIfNegative(minimum);
        if (maximum < minimum)
            throw new ArgumentOutOfRangeException(nameof(maximum));

        long distance = Math.Abs((long)target.X - origin.X) + Math.Abs((long)target.Y - origin.Y);
        return distance >= minimum && distance <= maximum;
    }
}

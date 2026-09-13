using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class BattleRange
{
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

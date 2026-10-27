using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// SF2DISASM c834c652, triggerregions.asm: IsAllyInTriangle and
// CheckTestPointPositionRelativeToSegment. Admitted coordinates are 0..47;
// products fit the source signed word before DIVS, which truncates toward zero.
// These integer projections intentionally differ from a geometric cross product.
internal static class SourceRegionGeometry
{
    internal static bool Includes(IReadOnlyList<MapPosition> v, MapPosition p) => v.Count == 3
        ? Triangle(v[0], v[1], v[2], p)
        : Triangle(v[0], v[1], v[3], p) || Triangle(v[2], v[1], v[3], p);

    private static bool Triangle(MapPosition a, MapPosition b, MapPosition c, MapPosition p)
    {
        int first = Segment(a, b, p);
        if (first == 1) return true;
        if (first != 3 && first != Segment(a, b, c)) return false;
        int second = Segment(c, a, p);
        if (second == 1) return true;
        if (second != 3 && second != Segment(c, a, b)) return false;
        int third = Segment(c, b, p);
        return third == 1 || third != 3 && third == Segment(c, b, a);
    }

    internal static int Segment(MapPosition a, MapPosition b, MapPosition p)
    {
        int dx = b.X - a.X, dy = b.Y - a.Y;
        bool horizontal = Math.Abs(dx) >= Math.Abs(dy) && dx != 0;
        if (!horizontal && dy == 0) return 3; // zero SUB + ORI carry retains Z
        int projected = horizontal ? b.Y + dy * (p.X - b.X) / dx : b.X + dx * (p.Y - b.Y) / dy;
        // MOVE #1,CCR clears Z on a negative projection: actual flag is 2,
        // despite the source comment calling this exception 3.
        if (projected < 0) return 2;
        int actual = horizontal ? p.Y : p.X;
        if (projected != actual) return projected < actual ? 2 : 0;
        int along = horizontal ? p.X : p.Y, start = horizontal ? a.X : a.Y, end = horizontal ? b.X : b.Y;
        return along >= Math.Min(start, end) && along <= Math.Max(start, end) ? 1 : 2;
    }
}

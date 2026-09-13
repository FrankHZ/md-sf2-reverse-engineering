using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal sealed class WeightedMovementGrid(byte[] totalCosts, byte[] movableGrid, int[] expansionOrder)
{
    internal IReadOnlyList<byte> TotalCosts { get; } = Array.AsReadOnly(totalCosts);
    internal IReadOnlyList<byte> MovableGrid { get; } = Array.AsReadOnly(movableGrid);
    internal IReadOnlyList<int> ExpansionOrder { get; } = Array.AsReadOnly(expansionOrder);
    internal int? CostAt(MapPosition position) => CostAtOffset(position.Y * 48 + position.X);
    internal int? CostAtOffset(int offset) => MovableGrid[offset] >= 128 ? null :
        (MovableGrid[offset] << 8) | TotalCosts[offset];
}

internal static class WeightedMovement
{
    // Accepted regular and priest profiles share this movement-cost row.
    internal static IReadOnlyList<sbyte> OrdinaryCosts { get; } = Array.AsReadOnly<sbyte>(
        [-1, 2, 2, 3, 4, 3, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1]);
    internal static WeightedMovementGrid Build(IReadOnlyList<byte> terrain,
        IReadOnlyList<sbyte> moveCosts, int startOffset, int budget, bool preserveFlatRowNeighbors = false)
    {
        if (terrain.Count != 2304 || moveCosts.Count != 16 || startOffset is < 0 or >= 2304 || budget is < 0 or > 510)
            throw new ArgumentException("Movement requires a complete storage grid, sixteen costs and a bounded MOV*2 budget.", "movement");
        var total = Enumerable.Repeat((byte)255, 2304).ToArray();
        var movable = Enumerable.Repeat((byte)255, 2304).ToArray();
        var heads = Enumerable.Repeat(-1, 32).ToArray();
        var links = Enumerable.Repeat(-1, 2304).ToArray();
        var admitted = new bool[2304]; var expansion = new List<int>();
        int remaining = budget, spent = 0, current = startOffset;
        while (true)
        {
            admitted[current] = true; StoreCost(current, spent); expansion.Add(current);
            foreach (int neighbor in new[] { current + 1, current - 1, current - 48, current + 48 })
            {
                // Deliberately bounds-check before reading, unlike the original unsafe probe chronology.
                if (neighbor is < 0 or >= 2304 || admitted[neighbor]) continue;
                // Authored logical maps never join the right edge to the next row's left edge.
                // The legacy source comparison explicitly retains its flat storage probes.
                if (!preserveFlatRowNeighbors && Math.Abs(neighbor - current) == 1 && neighbor / 48 != current / 48) continue;
                byte entry = terrain[neighbor];
                if ((entry & 0x80) != 0) continue;
                int type = entry & 0x1F;
                if (type >= moveCosts.Count) throw new ArgumentException("Terrain has no admitted movement-cost entry.", "terrain");
                int cost = moveCosts[type];
                if (cost < 0 || cost > remaining) continue;
                admitted[neighbor] = true; // First admission; never relax an already admitted cell.
                if (cost == remaining) { StoreCost(neighbor, spent + cost); continue; }
                int bucket = (remaining - cost) & 31;
                links[neighbor] = heads[bucket]; heads[bucket] = neighbor;
            }
            while (true)
            {
                int bucket = remaining & 31, next = heads[bucket];
                if (next >= 0) { heads[bucket] = links[next]; current = next; break; }
                spent++; remaining--;
                if (remaining <= 0) break;
            }
            if (remaining <= 0) break;
        }
        return new(total, movable, expansion.ToArray());

        void StoreCost(int offset, int cost)
        {
            total[offset] = unchecked((byte)cost); movable[offset] = (byte)(cost >> 8);
        }
    }
}

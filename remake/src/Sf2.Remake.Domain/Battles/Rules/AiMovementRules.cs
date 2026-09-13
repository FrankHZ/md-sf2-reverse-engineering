using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class AiMovementRules
{
    internal static int PursuitTarget(IReadOnlyList<int?> costs)
    {
        // MOVE1 leaves the last GetMoveCostToEntity result in d0 before the enemy-bit
        // class pass. This admitted complete 0..127 domain skips that disputed branch.
        if (costs.Count == 0 || costs.Any(cost => cost is not (>= 0 and < 128)))
            throw new BattleRuleException("ai-move-target-domain", "ai.move.targets", true);
        int best = 0;
        for (int i = 1; i < costs.Count; i++)
            if (costs[i] < costs[best]) best = i;
        return best; // Stable ascending source sort retains the first equal-cost target.
    }

    internal static MapPosition? AttackPosition(WeightedMovementGrid grid, MapPosition target, int radius,
        Func<MapPosition, bool> occupied)
    {
        MapPosition? best = null; int minimum = 255;
        for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius + Math.Abs(dy); dx <= radius - Math.Abs(dy); dx++)
            {
                if (Math.Abs(dx) + Math.Abs(dy) != radius) continue;
                int x = target.X + dx, y = target.Y + dy;
                if (x is < 0 or >= 48 || y is < 0 or >= 48) continue;
                var position = new MapPosition(x, y);
                if (grid.CostAt(position) is not { } cost) continue;
                if (cost == 0) return position;
                if (cost < minimum && !occupied(position)) { best = position; minimum = cost; }
            }
        return best;
    }

    internal static (MapPosition Destination, IReadOnlyList<byte> MoveString) Walk(
        WeightedMovementGrid grid, MapPosition origin, int targetCost, int width, int height)
    {
        if (!Within(origin) || targetCost < 0 || grid.CostAt(origin) is not { } startCost || targetCost > startCost)
            throw PathFailure();
        int current = origin.Y * 48 + origin.X;
        var directions = new List<byte>(); int previousMask = 0;
        while (grid.CostAtOffset(current) > targetCost)
        {
            int cost = grid.CostAtOffset(current) ?? throw PathFailure();
            int threshold = cost - 1, mask = 0;
            foreach ((int delta, int bit) in new[] { (1, 1), (-1, 4), (-48, 2), (48, 8) })
            {
                int neighbor = current + delta;
                if (neighbor is < 0 or >= 2304 || Math.Abs(neighbor % 48 - current % 48) +
                    Math.Abs(neighbor / 48 - current / 48) != 1) continue;
                if (grid.CostAtOffset(neighbor) is { } value && value <= threshold)
                {
                    mask |= bit; threshold = value; // Source retains earlier bits as threshold decreases.
                }
            }
            int choice = (mask & previousMask) != 0 && (mask ^ previousMask) != 0 ? mask ^ previousMask : mask;
            if (choice == 0) throw PathFailure();
            byte direction = (byte)((choice & 1) != 0 ? 0 : (choice & 2) != 0 ? 1 : (choice & 4) != 0 ? 2 : 3);
            current += direction switch { 0 => 1, 1 => -48, 2 => -1, _ => 48 };
            if (!Within(new(current % 48, current / 48)) || grid.CostAtOffset(current) is not { } nextCost || nextCost >= cost)
                throw PathFailure();
            previousMask = 1 << direction; directions.Add(direction);
        }
        return (new(current % 48, current / 48), Array.AsReadOnly(directions.Append((byte)255).ToArray()));

        bool Within(MapPosition position) => position.X >= 0 && position.X < width && position.Y >= 0 && position.Y < height;
        static BattleRuleException PathFailure() => new("ai-move-path", "ai.move.path", true);
    }
}

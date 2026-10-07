using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed record AiPursuitDecision(MapPosition Destination, int TargetIndex, IReadOnlyList<int> TargetCosts,
    MapPosition PreliminaryDestination, IReadOnlyList<byte> PreliminaryMoveString, IReadOnlyList<byte> MoveString, int Cost);

internal static class AiMovementRules
{
    internal static AiPursuitDecision Pursue(IReadOnlyList<sbyte> rawCosts, WeightedMovementGrid legal,
        MapPosition origin, IReadOnlyList<MapPosition> targets, Func<MapPosition, bool> occupied,
        int width, int height)
    {
        var raw = WeightedMovement.Build(rawCosts, origin.Y * 48 + origin.X, 128);
        var costs = targets.Select(target => raw.CostAt(target)).ToArray();
        int selected = PursuitTarget(costs); var target = targets[selected];
        var reverse = WeightedMovement.Build(rawCosts, target.Y * 48 + target.X, 128);
        int startCost = reverse.CostAt(origin) ?? throw new BattleRuleException("ai-move-path", "ai.move.path", true);
        var preliminary = BattleMovement.Walk(reverse, origin, Math.Max(0, startCost - 4), width, height);
        var destination = preliminary.MoveString.Count == 1 ? origin :
            AttackPosition(legal, preliminary.Destination, 0, occupied) ??
            AttackPosition(legal, preliminary.Destination, 1, occupied) ?? origin;
        return new(destination, selected, Array.AsReadOnly(costs.Select(cost => cost!.Value).ToArray()),
            preliminary.Destination, preliminary.MoveString, BattleMovement.MoveString(legal, origin, destination, width, height), legal.CostAt(destination)!.Value);
    }

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

}

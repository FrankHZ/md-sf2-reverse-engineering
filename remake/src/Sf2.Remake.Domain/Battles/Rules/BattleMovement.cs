using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

public sealed class BattleMovementPreview
{
    internal BattleMovementPreview(IEnumerable<MapPosition> path, int cost)
    { Path = Array.AsReadOnly(path.ToArray()); Cost = cost; }
    public IReadOnlyList<MapPosition> Path { get; }
    public MapPosition Destination => Path[^1];
    public int Cost { get; }
}

internal static class BattleMovement
{
    internal static BattleMovementPreview Preview(EngineBattleState battle, ActorRef actorRef, MapPosition destination)
    {
        var actor = battle.GetActor(actorRef);
        if (!battle.Definition.Contains(destination)) throw new BattleRuleException("movement-range", "destination");
        var terrain = battle.Definition.Terrain.ToArray();
        foreach (var opponent in battle.Actors.Where(a => a.Hp > 0 && a.Definition.IsAlly != actor.Definition.IsAlly))
            terrain[Offset(opponent.Position!)] |= 128;
        // Accepted PRST/HEALER and regular profiles have the same costs; no centaur/flying capability here.
        var costs = WeightedMovement.OrdinaryCosts;
        var grid = WeightedMovement.Build(terrain, costs, Offset(actor.Position!), actor.Definition.Move * 2);
        if (grid.CostAt(destination) is not { } destinationCost)
            throw new BattleRuleException("movement-range", "destination");
        int current = Offset(destination), origin = Offset(actor.Position!), previousDirection = -1;
        var backwards = new List<MapPosition> { destination };
        while (current != origin)
        {
            int currentCost = grid.CostAtOffset(current)!.Value;
            var candidates = new List<(int Offset, byte Direction, int Cost)>();
            foreach (var (delta, direction) in new (int, byte)[] { (1, 0), (-1, 2), (-48, 1), (48, 3) })
            {
                int offset = current + delta;
                if (offset is < 0 or >= 2304) continue;
                var position = new MapPosition(offset % 48, offset / 48);
                if (!battle.Definition.Contains(position) ||
                    !BattleRange.Contains(new(current % 48, current / 48), position, 1, 1)) continue;
                if (grid.CostAtOffset(offset) is { } cost && cost < currentCost)
                    candidates.Add((offset, direction, cost));
            }
            if (candidates.Count == 0) throw new BattleRuleException("movement-path", "destination");
            int minimum = candidates.Min(c => c.Cost);
            var lowest = candidates.Where(c => c.Cost == minimum).OrderBy(c => c.Direction).ToArray();
            var selected = lowest.FirstOrDefault(c => c.Direction != previousDirection, lowest[0]);
            current = selected.Offset; previousDirection = selected.Direction;
            backwards.Add(new(current % 48, current / 48));
        }
        var path = backwards.AsEnumerable().Reverse().ToArray();
        int actualCost = path.Skip(1).Sum(p => costs[terrain[Offset(p)] & 31]);
        if (actualCost > destinationCost || actualCost > actor.Definition.Move * 2)
            throw new BattleRuleException("movement-path", "destination");
        return new(path, actualCost);
    }

    internal static void RequireStop(EngineBattleState battle, ActorRef actor, MapPosition destination)
    {
        _ = Preview(battle, actor, destination);
        if (battle.Actors.Any(a => a.Actor != actor && a.Hp > 0 && a.Position == destination))
            throw new BattleRuleException("occupied-destination", "destination");
    }

    internal static EngineBattleState Commit(EngineBattleState battle, ActorRef actor, MapPosition destination)
    {
        RequireStop(battle, actor, destination);
        return battle.With(actors: battle.Actors.Select(a => a.Actor == actor ? a.With(position: destination) : a));
    }
    private static int Offset(MapPosition position) => position.Y * 48 + position.X;
}

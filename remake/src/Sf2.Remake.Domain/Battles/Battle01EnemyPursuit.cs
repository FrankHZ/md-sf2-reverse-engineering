using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

public sealed record Battle01PursuitTargetCost(int ActorIndex, int Cost);
public sealed record Battle01AttackCandidate(int ActorIndex, MapPosition AttackPosition, int GridCost);
public sealed record Battle01EnemyPursuitDecision(int ActorIndex, byte CommandSet, MapPosition Origin, MapPosition Destination,
    ushort SeedCopyBefore, ushort SeedCopyAfter, byte MemoryBefore, byte MemoryAfter, uint MainSeedImage,
    IReadOnlyList<Battle01PursuitTargetCost> TargetCosts, int TargetIndex, MapPosition PreliminaryDestination,
    IReadOnlyList<byte> PreliminaryMoveString, IReadOnlyList<byte> MoveString, int GridCost)
{
    public byte Action => 3;
    public byte MovementType => 6;
}

// Eligibility is reported before PrioritizeTargetsForAttackAiCommand can score or consume RNG.
public sealed class Battle01AttackSelectionRequiredException : ArgumentException
{
    internal Battle01AttackSelectionRequiredException(int actorIndex, Battle01AttackCandidate[] targets)
        : base($"Enemy {actorIndex} requires attack selection; the current turn is retained.", "attack.targets")
    {
        ActorIndex = actorIndex; Targets = Array.AsReadOnly(targets);
    }
    public int ActorIndex { get; }
    public IReadOnlyList<Battle01AttackCandidate> Targets { get; }
}

public static class Battle01EnemyPursuit
{
    public static Battle01InitializedState CompleteNext(Battle01InitializedState current, int actorIndex,
        Battle01StayCompletionPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(current);
        Battle01EnemyStandby.RequireCurrentRound(current);
        if (actorIndex < 128 || current.FirstRound!.CurrentCandidate?.CombatantIndex != actorIndex)
            throw new ArgumentException("Pursuit must name the actual current enemy.", "actor");
        var actor = current.Roster.Single(unit => unit.Index == actorIndex);
        Battle01EnemyStandby.RequireRegularEnemy(actor, current.FirstRound.RoundNumber, active: true);
        Battle01TurnCompletion.RequireContinuingNoEffectState(current);
        if (current.Roster.Take(3).Any(unit => unit.AiBitfield != 0))
            throw new ArgumentException("The three controlled allies must retain their nonneutral activation words.", "activation");
        if (!ReferenceEquals(policy, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats))
            throw new ArgumentException("Pursuit requires the controlled unchanged effective-stat policy.", "policy");

        var decision = Decide(current, actor);
        var roster = current.Roster.ToArray(); var occupancy = current.Occupancy.ToArray();
        if (decision.Destination != actor.Position && occupancy[Battle01PlayerMovement.Offset(decision.Destination)] != -1)
            throw new ArgumentException("The final pursuit destination must be unoccupied.", "occupancy");
        roster[Array.IndexOf(roster, actor)] = actor.WithPosition(decision.Destination);
        occupancy[Battle01PlayerMovement.Offset(actor.Position)] = -1;
        occupancy[Battle01PlayerMovement.Offset(decision.Destination)] = actorIndex;
        // The existing enemy projection clears tested regions locally; only the returned completion commits it.
        var moved = new Battle01InitializedState(current, roster, occupancy, current.AiMemory.ToArray(), decision.SeedCopyAfter);
        return Battle01TurnCompletion.CompleteControlledStay(moved, actorIndex, actor.Stats, policy, enemyPursuit: decision);
    }

    internal static Battle01EnemyPursuitDecision Decide(Battle01InitializedState battle, Battle01Combatant actor)
    {
        var (grid, physical) = PhysicalCandidates(battle, actor);
        if (physical.Length != 0) throw new Battle01AttackSelectionRequiredException(actor.Index, physical);
        var allies = battle.Roster.Where(unit => unit.Index < 128).OrderBy(unit => unit.Index).ToArray();
        return DecidePursuit(battle, actor, grid, allies);
    }

    internal static (Battle01MovementGrid Grid, Battle01AttackCandidate[] Targets) PhysicalCandidates(
        Battle01InitializedState battle, Battle01Combatant actor)
    {
        if (battle.Terrain.Count != 2304 || battle.Terrain.Any(value => value != 255 && value > 15))
            throw new ArgumentException("Pursuit requires the unmodified source terrain types.", "terrain");
        var allies = battle.Roster.Where(unit => unit.Index < 128).OrderBy(unit => unit.Index).ToArray();
        var blocked = battle.Terrain.ToArray();
        foreach (var ally in allies) blocked[Battle01PlayerMovement.Offset(ally.Position)] |= 128;
        var grid = Grid(blocked, actor.Position, 10);
        var physical = new List<Battle01AttackCandidate>();
        foreach (var ally in allies)
            if (DetermineAttackPosition(grid, ally.Position, 1, battle.Occupancy) is { } position)
                physical.Add(new(ally.Index, position, grid.CostAt(position)!.Value));
        return (grid, physical.ToArray());
    }

    private static Battle01EnemyPursuitDecision DecidePursuit(Battle01InitializedState battle,
        Battle01Combatant actor, Battle01MovementGrid grid, Battle01Combatant[] allies)
    {
        // The admitted GIZMO has no usable item/spell/heal/support action; set7's orderFF also fails.
        // MOVE1 (mode0): raw target costs, stable ascending order, then a target-rooted budget4 walk.
        var raw = Grid(battle.Terrain, actor.Position, 128);
        var costs = allies.Select(ally => new Battle01PursuitTargetCost(ally.Index,
            raw.CostAt(ally.Position) is { } cost && cost < 128 ? cost :
                throw new ArgumentException("Only complete target costs below128 admit the source d0 class-pass boundary.", "pursuit.targets"))).ToArray();
        if (costs.Length != 3) throw new ArgumentException("Retain all three living controlled targets.", "pursuit.targets");
        int target = costs.OrderBy(item => item.Cost).First().ActorIndex;
        var reverse = Grid(battle.Terrain, allies.Single(ally => ally.Index == target).Position, 128);
        int startCost = reverse.CostAt(actor.Position) ?? throw new ArgumentException("The target-rooted path must reach the actor.", "path");
        var preliminary = Battle01EnemyStandby.SourceWalk(reverse, actor.Position, Math.Max(0, startCost - 4));
        var destination = preliminary.MoveString.Count == 1 ? actor.Position :
            DetermineAttackPosition(grid, preliminary.Destination, 0, battle.Occupancy) ??
            DetermineAttackPosition(grid, preliminary.Destination, 1, battle.Occupancy) ?? actor.Position;
        var path = Battle01EnemyStandby.SourceMoveString(grid, actor.Position, destination);
        ushort copy = battle.RandomSeedCopy!.Value; byte memory = battle.AiMemory[actor.Index - 128];
        return new(actor.Index, actor.Deployment.AiCommandSet, actor.Position, destination, copy, copy, memory, memory,
            battle.RandomSeedImage, Array.AsReadOnly(costs), target, preliminary.Destination, preliminary.MoveString,
            path, grid.CostAt(destination)!.Value);
    }

    private static Battle01MovementGrid Grid(IReadOnlyList<byte> terrain, MapPosition origin, int budget) =>
        Battle01PlayerMovement.BuildWeightedGrid(terrain, Battle01PlayerMovement.HoveringCosts,
            Battle01PlayerMovement.Offset(origin), budget);

    internal static MapPosition? DetermineAttackPosition(Battle01MovementGrid grid, MapPosition target, int radius,
        IReadOnlyList<int> occupancy)
    {
        MapPosition? best = null; int minimum = 255;
        // Source radius0/1 scans north, west, east, south. Strict unsigned comparison retains first ties.
        for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius + Math.Abs(dy); dx <= radius - Math.Abs(dy); dx++)
            {
                if (Math.Abs(dx) + Math.Abs(dy) != radius) continue;
                int x = target.X + dx, y = target.Y + dy;
                if (x is < 0 or >= 48 || y is < 0 or >= 48) continue;
                var candidate = new MapPosition(x, y); int? cost = grid.CostAt(candidate);
                if (cost == 0) return candidate;
                if (cost is { } value && value < minimum && occupancy[Battle01PlayerMovement.Offset(candidate)] == -1)
                {
                    best = candidate; minimum = value;
                }
            }
        return best;
    }
}

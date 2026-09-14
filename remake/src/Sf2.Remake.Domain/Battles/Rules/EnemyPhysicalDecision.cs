using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class EnemyPhysicalDecision
{
    // Already-active ATTACK1 / script3, physical-only. This is
    // a successful first command of source commandset06, not a replacement fallback policy.
    internal static (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects, MapPosition Destination)? TryResolve(
        EngineBattleState current, ActorRef actorRef)
    {
        var actor = current.GetActor(actorRef);
        var grid = BattleMovement.Grid(current, actorRef);
        var candidates = new List<(BattleActorState Target, MapPosition Position, int Cost)>();
        foreach (var target in current.Actors.Where(a => a.Hp > 0 && a.IsAlly).OrderBy(a => a.ProcessingOrder))
        {
            if (AiMovementRules.AttackPosition(grid, target.Position!, 1,
                position => current.Actors.Any(a => a.Hp > 0 && a.Position == position)) is { } position)
                candidates.Add((target, position, grid.CostAt(position)!.Value));
        }
        if (candidates.Count == 0) return null;
        _ = PhysicalBattleAction.RequireActor(current, actorRef);
        var priorities = new PhysicalTargetPriority[candidates.Count];
        var decisions = new List<BattleEffect>();
        uint thinking = current.ThinkingSeed;
        // Source visits the reachable array backwards, storing each result at its original
        // index. Selection later collects the highest raw-priority cohort backwards too.
        for (int index = candidates.Count - 1; index >= 0; index--)
        {
            var candidate = candidates[index];
            if (candidate.Target.Definition.Physical is null)
                throw new BattleRuleException("physical-definition", "target.physical", true);
            int potential = PhysicalStrikeRules.LandDamage(actor.Attack, candidate.Target.Definition.Defense,
                BattleTerrainRules.LandMultiplier(current.Definition.Terrain[candidate.Target.Position!.Y * 48 + candidate.Target.Position.X], candidate.Target.Definition.Mover));
            var draw = BattleRandom.NextThinkingWord((ushort)(thinking >> 16), 3);
            uint after = ((uint)draw.After << 16) | (thinking & 65535);
            byte priority = PhysicalTargetRules.ScriptThree((byte)candidate.Cost, Math.Max(0, candidate.Target.Hp - potential), draw.Value);
            priorities[index] = new((byte)candidate.Cost, priority, candidate.Target.Definition.SourceClassId);
            decisions.Add(new("thinking-rng", actorRef, thinking, after, 3, draw.Value, candidate.Target.Actor));
            decisions.Add(new("ai-candidate", actorRef, candidate.Cost, priority, Target: candidate.Target.Actor));
            thinking = after;
        }
        // Source airborne movement selects the Flying table; the other admitted movers use Regular.
        var selection = PhysicalTargetRules.Select(priorities, actor.Definition.Mover == BattleMover.Hovering
            ? PhysicalPriorityTable.Flying : PhysicalPriorityTable.Regular)
            ?? throw new BattleRuleException("ai-target-selection", "ai.targets", true);
        var selected = candidates[selection.Index];
        var prepared = current.With(thinkingSeed: thinking,
            actors: current.Actors.Select(a => a.Actor == actorRef ? a.With(lastTarget: selected.Target.Actor) : a));
        var (battle, effects) = PhysicalBattleAction.Resolve(prepared, actorRef, selected.Position, selected.Target.Actor);
        return (battle, Array.AsReadOnly<BattleEffect>([
            .. decisions,
            new("ai-target", actorRef, selection.RawPriority, selection.CappedPriority, Target: selected.Target.Actor),
            .. effects]), selected.Position);
    }
}

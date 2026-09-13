using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class EnemyPhysicalDecision
{
    // Already-active ATTACK1 / script3, physical-only, single reachable target. This is
    // a successful first command of source commandset06, not a replacement fallback policy.
    internal static (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects, MapPosition Destination) Resolve(
        EngineBattleState current, ActorRef actorRef)
    {
        var actor = current.GetActor(actorRef);
        _ = PhysicalBattleAction.RequireActor(current, actorRef);
        var grid = BattleMovement.Grid(current, actorRef);
        var candidates = new List<(BattleActorState Target, MapPosition Position, int Cost)>();
        foreach (var target in current.Actors.Where(a => a.Hp > 0 && a.Definition.IsAlly).OrderBy(a => a.Definition.Slot))
        {
            MapPosition? best = null;
            int minimum = 255;
            // DetermineAttackPosition radius-one diamond order; strict cost improvement.
            foreach (var (dx, dy) in new (int, int)[] { (0, -1), (-1, 0), (1, 0), (0, 1) })
            {
                int x = target.Position!.X + dx, y = target.Position.Y + dy;
                if (x < 0 || y < 0 || x >= current.Definition.Width || y >= current.Definition.Height) continue;
                var position = new MapPosition(x, y);
                if (grid.CostAt(position) is not { } cost) continue;
                if (cost == 0) { best = position; minimum = 0; break; }
                if (cost >= minimum || current.Actors.Any(a => a.Hp > 0 && a.Position == position)) continue;
                best = position; minimum = cost;
            }
            if (best is not null) candidates.Add((target, best, minimum));
        }
        if (candidates.Count == 0) throw new BattleRuleException("ai-commandset-continuation", "ai.targets", true);
        if (candidates.Count != 1) throw new BattleRuleException("ai-target-ranking", "ai.targets", true);
        var selected = candidates[0];
        // Source word occupies the upper half of the authored 32-bit image, as with main RNG.
        var draw = BattleRandom.NextThinkingWord((ushort)(current.ThinkingSeed >> 16), 3);
        uint thinking = ((uint)draw.After << 16) | (current.ThinkingSeed & 65535);
        int potential = PhysicalStrikeRules.LandDamage(actor.Definition.Attack, selected.Target.Definition.Defense,
            PhysicalBattleAction.LandMultiplier(current.Definition.Terrain[selected.Target.Position!.Y * 48 + selected.Target.Position.X]));
        int priority = draw.Value == 0 ? (potential >= selected.Target.Hp ? 16 : 1) : Math.Max(19 - 2 * selected.Cost, 1);
        // Script3 priorities are positive. With one target, class/cohort tie breaking cannot
        // choose another actor; wider cohorts are explicitly outside this capability.
        var prepared = current.With(thinkingSeed: thinking,
            actors: current.Actors.Select(a => a.Actor == actorRef ? a.With(lastTarget: selected.Target.Actor) : a));
        var (battle, effects) = PhysicalBattleAction.Resolve(prepared, actorRef, selected.Position, selected.Target.Actor);
        return (battle, Array.AsReadOnly<BattleEffect>([
            new("ai-target", actorRef, After: priority, Target: selected.Target.Actor),
            new("thinking-rng", actorRef, current.ThinkingSeed, thinking, 3, draw.Value, selected.Target.Actor),
            .. effects]), selected.Position);
    }
}

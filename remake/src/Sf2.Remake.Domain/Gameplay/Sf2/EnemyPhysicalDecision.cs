using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class EnemyPhysicalDecision
{
    // Admitted ordinary script3/4 decision, using the caller's temporary grid when supplied.
    // CalculateTargetPriorityForAttack clears ATTACK1/2/3's extra parameter; the priority
    // script is selected from activation/difficulty, not that command parameter.
    internal static BattleAutomaticAction? TryResolve(
        EngineBattleState current, ActorRef actorRef, IPhysicalActionRule physical, WeightedMovementGrid? selectedGrid = null)
    {
        var actor = current.GetActor(actorRef);
        var grid = selectedGrid ?? BattleMovement.Grid(current, actorRef);
        var action = new BattleActionRef(BattleActionKind.Physical);
        var admission = BattleActionRules.Invoke(physical, "ai-range", () => physical.QueryRange(current, actorRef));
        var targets = BattleActionRules.Invoke(physical, "ai-targets", () => physical.QueryTargets(current, actorRef, action)).ToHashSet();
        var candidates = new List<(BattleActorState Target, MapPosition Position, int Cost)>();
        foreach (var target in current.Actors.Where(a => a.Hp > 0 && a.Position is not null && targets.Contains(a.Actor)).OrderBy(a => a.ProcessingOrder))
        {
            if (AiMovementRules.AttackPosition(grid, target.Position!, admission.MinimumRange!.Value, admission.MaximumRange!.Value,
                position => current.Actors.Any(a => a.Hp > 0 && a.Position == position)) is { } position)
                candidates.Add((target, position, grid.CostAt(position)!.Value));
        }
        if (candidates.Count == 0) return null;
        var priorities = new PhysicalTargetPriority[candidates.Count];
        var decisions = new List<BattleEffect>();
        uint thinking = current.ThinkingSeed;
        // Source visits the reachable array backwards, storing each result at its original
        // index. Selection later collects the highest raw-priority cohort backwards too.
        for (int index = candidates.Count - 1; index >= 0; index--)
        {
            var candidate = candidates[index];
            int potential = BattleActionRules.Invoke(physical, "ai-estimate", () =>
            {
                int damage = physical.EstimateDamage(current, actorRef, candidate.Target.Actor);
                if (damage < 0) throw new InvalidOperationException("Invalid physical estimate.");
                return damage;
            });
            byte priority = PhysicalTargetRules.Score(actor, candidate.Target, (byte)candidate.Cost, potential, ref thinking, decisions);
            priorities[index] = new((byte)candidate.Cost, priority, Sf2ClassRules.SourceClass(candidate.Target.Definition.ClassRule));
            decisions.Add(new("ai-candidate", actorRef, candidate.Cost, priority, Target: candidate.Target.Actor));
        }
        // Physical preferences follow the actual source mover, independently of the target class.
        var selection = PhysicalTargetRules.Select(priorities, PhysicalTargetRules.Table(actor.Definition.Mover))
            ?? throw new BattleRuleException("ai-target-selection", "ai.targets", true);
        var selected = candidates[selection.Index];
        var prepared = current.With(thinkingSeed: thinking,
            actors: current.Actors.Select(a => a.Actor == actorRef ? a.With(lastTarget: selected.Target.Actor) : a));
        var path = BattleMovement.Route(actor.Position!, BattleMovement.MoveString(grid, actor.Position!,
            selected.Position, current.Definition.Width, current.Definition.Height));
        return new(prepared, Array.AsReadOnly<BattleEffect>([
            .. decisions,
            new("ai-target", actorRef, selection.RawPriority, selection.CappedPriority, Target: selected.Target.Actor)]),
            selected.Position, path, selected.Target.Actor);
    }
}

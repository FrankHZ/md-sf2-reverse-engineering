using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class EnemyPhysicalDecision
{
    internal sealed record Search(BattleAutomaticAction? Choice, EngineBattleState Battle,
        IReadOnlyList<BattleEffect> Effects, bool HasReachable);

    internal static BattleAutomaticAction? TryResolve(EngineBattleState current, ActorRef actorRef, IPhysicalActionRule physical)
    {
        var search = Score(current, actorRef, physical, OffensiveTargetSide.Opponents);
        return search.Choice is { } attack ? BattleOffense.Choose(current, actorRef, attack) : null;
    }

    // Admitted ordinary script3/4 decision, using the caller's temporary grid when supplied.
    // CalculateTargetPriorityForAttack clears ATTACK1/2/3's extra parameter; the priority
    // script is selected from activation/difficulty, not that command parameter.
    internal static Search Score(
        EngineBattleState current, ActorRef actorRef, IPhysicalActionRule physical, OffensiveTargetSide side,
        WeightedMovementGrid? selectedGrid = null,
        IReadOnlyList<BattleActorState>? orderedTargets = null)
    {
        var actor = current.GetActor(actorRef);
        var grid = selectedGrid ?? BattleMovement.Grid(current, actorRef);
        var action = new BattleActionRef(BattleActionKind.Physical);
        var admission = BattleActionRules.Invoke(physical, "ai-range", () => physical.QueryRange(current, actorRef));
        var targets = BattleActionRules.Invoke(physical, "ai-targets", () => physical.QueryTargets(current, actorRef, action, side)).ToHashSet();
        var candidates = new List<(BattleActorState Target, MapPosition Position, int Cost)>();
        var ordered = orderedTargets ?? BattleOffense.OrderedRoster(current, actor, side).ToArray();
        foreach (var target in ordered.Where(a => a.Hp > 0 && a.Position is not null && targets.Contains(a.Actor)))
        {
            if (AiMovementRules.AttackPosition(grid, target.Position!, admission.MinimumRange!.Value, admission.MaximumRange!.Value,
                position => current.Actors.Any(a => a.Hp > 0 && a.Position == position)) is { } position)
                candidates.Add((target, position, grid.CostAt(position)!.Value));
        }
        if (candidates.Count == 0) return new(null, current, [], false);
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
        var prepared = current.With(thinkingSeed: thinking);
        var result = PhysicalTargetRules.Select(priorities, PhysicalTargetRules.Table(actor.Definition.Mover),
            preferTargetClass: !actor.IsAlly, hasTargetClasses: side == OffensiveTargetSide.Opponents);
        if (result is not { } selection)
        {
            if (side == OffensiveTargetSide.Opponents) throw new BattleRuleException("ai-target-selection", "ai.targets", true);
            return new(null, prepared, decisions.AsReadOnly(), true);
        }
        var selected = candidates[selection.Index];
        // The final source unarmed attack-position query allows the origin for
        // a selected self. Reachability and scoring above still use GetAttackRange.
        var stop = selected.Target.Actor == actorRef && BattleOffense.SelfPhysicalUsesOrigin(actor)
            ? actor.Position! : selected.Position;
        var path = BattleMovement.Route(actor.Position!, BattleMovement.MoveString(grid, actor.Position!,
            stop, current.Definition.Width, current.Definition.Height));
        decisions.Add(new("ai-target", actorRef, selection.RawPriority, selection.CappedPriority, Target: selected.Target.Actor));
        return new(new(prepared, decisions.AsReadOnly(), stop, path, selected.Target.Actor),
            prepared, decisions.AsReadOnly(), true);
    }
}

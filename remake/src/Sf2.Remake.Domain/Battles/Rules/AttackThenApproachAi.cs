using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// Already-active physical attack, then approach: the admitted source commandset 06 / script 3 branch.
internal static class AttackThenApproachAi
{
    internal static (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects, MapPosition Destination) Resolve(
        EngineBattleState current, ActorRef actorRef)
    {
        if (EnemyPhysicalDecision.TryResolve(current, actorRef) is { } attack)
            return (attack.Battle, Array.AsReadOnly<BattleEffect>([new("ai-command-attack1", actorRef, After: 0), .. attack.Effects]), attack.Destination);

        // In this admitted empty spellbook/item/status branch ATTACK1, HEAL1 and SUPPORT
        // return -1 without RNG. MOVE1 mode0 returns 0, including a Stay movement result.
        var actor = current.GetActor(actorRef);
        var targets = current.Actors.Where(a => a.Hp > 0 && a.IsAlly).OrderBy(a => a.ProcessingOrder).ToArray();
        var raw = Grid(actor.Position!);
        var costs = targets.Select(a => raw.CostAt(a.Position!)).ToArray();
        int selected = AiMovementRules.PursuitTarget(costs);
        var target = targets[selected];
        var reverse = Grid(target.Position!);
        int startCost = reverse.CostAt(actor.Position!) ?? throw new BattleRuleException("ai-move-path", "ai.move.path", true);
        var preliminary = AiMovementRules.Walk(reverse, actor.Position!, Math.Max(0, startCost - 4),
            current.Definition.Width, current.Definition.Height);
        var legal = BattleMovement.Grid(current, actorRef);
        bool Occupied(MapPosition position) => current.Actors.Any(a => a.Hp > 0 && a.Position == position);
        var destination = preliminary.MoveString.Count == 1 ? actor.Position! :
            AiMovementRules.AttackPosition(legal, preliminary.Destination, 0, Occupied) ??
            AiMovementRules.AttackPosition(legal, preliminary.Destination, 1, Occupied) ?? actor.Position!;
        var battle = BattleMovement.Commit(current, actorRef, destination);
        return (battle, Array.AsReadOnly<BattleEffect>([
            new("ai-command-attack1", actorRef, After: -1),
            new("ai-command-heal1", actorRef, After: -1),
            new("ai-command-support", actorRef, After: -1),
            new("ai-move-target", actorRef, costs[selected], legal.CostAt(destination), Target: target.Actor),
            new(destination == actor.Position ? "ai-move-stay" : "ai-move", actorRef, Target: target.Actor),
            new("ai-command-move1", actorRef, After: 0),
            new("after-turn", actorRef)]), destination);

        WeightedMovementGrid Grid(MapPosition origin) => WeightedMovement.Build(
            current.Definition.Terrain.Select(OrdinaryGroundRules.MovementCost).ToArray(), origin.Y * 48 + origin.X, 128);
    }
}

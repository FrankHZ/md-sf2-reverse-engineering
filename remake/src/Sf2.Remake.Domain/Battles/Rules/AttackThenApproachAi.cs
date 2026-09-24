using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// Already-active physical attack, then approach: the admitted source commandset 06 / script 3 branch.
internal static class AttackThenApproachAi
{
    internal static BattleAutomaticAction Resolve(
        EngineBattleState current, ActorRef actorRef)
    {
        if (EnemyPhysicalDecision.TryResolve(current, actorRef) is { } attack)
            return attack with { Effects = Array.AsReadOnly<BattleEffect>([new("ai-command-attack1", actorRef, After: 0), .. attack.Effects]) };

        // In this admitted empty spellbook/item/status branch ATTACK1, HEAL1 and SUPPORT
        // return -1 without RNG. MOVE1 mode0 returns 0, including a Stay movement result.
        var actor = current.GetActor(actorRef);
        var targets = current.Actors.Where(a => a.Hp > 0 && a.IsAlly).OrderBy(a => a.ProcessingOrder).ToArray();
        var legal = BattleMovement.Grid(current, actorRef);
        var decision = AiMovementRules.Pursue(current.Definition.Terrain.Select(tile => BattleTerrainRules.MovementCost(tile, actor.Definition.Mover)).ToArray(),
            legal, actor.Position!, targets.Select(target => target.Position!).ToArray(),
            position => current.Actors.Any(a => a.Hp > 0 && a.Position == position), current.Definition.Width, current.Definition.Height);
        var target = targets[decision.TargetIndex]; var destination = decision.Destination;
        BattleMovement.RequireStop(current, actorRef, destination, legal);
        return new(current, Array.AsReadOnly<BattleEffect>([
            new("ai-command-attack1", actorRef, After: -1),
            new("ai-command-heal1", actorRef, After: -1),
            new("ai-command-support", actorRef, After: -1),
            new("ai-move-target", actorRef, decision.TargetCosts[decision.TargetIndex], decision.Cost, Target: target.Actor),
            new(destination == actor.Position ? "ai-move-stay" : "ai-move", actorRef, Target: target.Actor),
            new("ai-command-move1", actorRef, After: 0)]), destination, BattleMovement.Route(actor.Position!, decision.MoveString));

    }
}

using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Already-active physical attack, then approach: the admitted source commandset 06 / script 3 branch.
internal sealed class AttackThenApproachAi : IBattleDecisionRule
{
    public string Identity => "sf2-attack-then-approach";
    public void RequireDeployment(BattleDeploymentDefinition deployment)
    {
        var actor = deployment.Definition;
        if (actor.Physical is null) throw new BattleRuleException("physical-definition", "actors.physical", true);
        if (actor.Spells.Count != 0) throw new BattleRuleException("ai-action-categories", "actors.spells", true);
        if (actor.Move is < 1 or > 63) throw new BattleRuleException("ai-movement-domain", "actors.move", true);
    }
    public void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input)
    {
        var loadout = input.SourceLoadout ?? input.Progress?.SourceLoadout ?? deployment.Definition.SourceLoadout;
        if (loadout is not null && loadout.Items.Any(word => (word & 127) != 127))
            throw new BattleRuleException("ai-action-categories", "start.actors.items", true);
    }
    public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical) =>
        Resolve(battle, actor, physical, preflight: false);

    // Explicit source-default overload for standalone policy comparisons.
    internal static BattleAutomaticAction Resolve(EngineBattleState current, ActorRef actor) =>
        Resolve(current, actor, new Sf2PhysicalAction());

    internal static BattleAutomaticAction Resolve(
        EngineBattleState current, ActorRef actorRef, IPhysicalActionRule physical, bool preflight = true)
    {
        if (EnemyPhysicalDecision.TryResolve(current, actorRef, physical, preflight) is { } attack)
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

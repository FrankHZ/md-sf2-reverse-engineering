using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Domain.Gameplay.Authored;

internal abstract class AuthoredPhysicalDecision : IBattleDecisionRule
{
    public abstract string Identity { get; }
    // These demonstrations reuse the admitted physical-only capability; no source scoring is copied.
    public void RequireDeployment(BattleDeploymentDefinition deployment) => new AttackThenApproachAi().RequireDeployment(deployment);
    public void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input) =>
        new AttackThenApproachAi().RequireStart(deployment, input);
    protected abstract BattleActorState Select(IReadOnlyList<BattleActorState> candidates);
    public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actorRef, IPhysicalActionRule physical)
    {
        var actor = battle.GetActor(actorRef);
        var action = new BattleActionRef(BattleActionKind.Physical);
        var admission = BattleActionRules.Invoke(physical, "ai-admission", () => physical.RequireAction(battle, actorRef, action));
        var grid = BattleMovement.Grid(battle, actorRef);
        var legal = new List<(BattleActorState Actor, MapPosition Destination)>();
        foreach (var targetRef in BattleActionRules.Invoke(physical, "ai-target-query", () => physical.QueryTargets(battle, actorRef, action)))
        {
            var target = battle.GetActor(targetRef);
            if (target.Hp == 0 || target.Position is null || target.Faction == actor.Faction) continue;
            MapPosition? destination = null;
            foreach (var position in Enumerable.Range(0, 2304).Select(offset => new MapPosition(offset % 48, offset / 48))
                .Where(position => battle.Definition.Contains(position) && grid.CostAt(position) is not null)
                .OrderBy(position => grid.CostAt(position)).ThenBy(position => position.Y).ThenBy(position => position.X))
            {
                if (!BattleRange.Contains(position, target.Position, admission.MinimumRange!.Value, admission.MaximumRange!.Value) ||
                    battle.Actors.Any(unit => unit.Actor != actorRef && unit.Hp > 0 && unit.Position == position)) continue;
                try
                {
                    _ = BattleActionRules.Invoke(physical, "ai-target", () => physical.RequireTarget(battle, actorRef, position, action, targetRef));
                    destination = position; break;
                }
                catch (BattleRuleException error) when (!error.Unsupported) { }
            }
            if (destination is not null) legal.Add((target, destination));
        }
        if (legal.Count == 0)
            return new(battle, Array.AsReadOnly<BattleEffect>([new("ai-stay", actorRef)]), actor.Position!,
                Array.AsReadOnly([actor.Position!]), QueueOnly: true);
        var selected = Select(legal.Select(candidate => candidate.Actor).OrderBy(unit => unit.ProcessingOrder).ToArray());
        var to = legal.Single(candidate => candidate.Actor.Actor == selected.Actor).Destination;
        var path = BattleMovement.Preview(battle, actorRef, to).Path;
        var prepared = battle.With(actors: battle.Actors.Select(unit => unit.Actor == actorRef ? unit.With(lastTarget: selected.Actor) : unit));
        return new(prepared, Array.AsReadOnly<BattleEffect>([new("ai-target", actorRef, Target: selected.Actor)]),
            to, path, selected.Actor);
    }
}

internal sealed class FirstLegalBattleDecision : AuthoredPhysicalDecision
{
    public override string Identity => "authored-first-legal";
    protected override BattleActorState Select(IReadOnlyList<BattleActorState> candidates) => candidates[0];
}

internal sealed class LowestHpBattleDecision : AuthoredPhysicalDecision
{
    public override string Identity => "authored-lowest-hp";
    protected override BattleActorState Select(IReadOnlyList<BattleActorState> candidates) =>
        candidates.OrderBy(candidate => candidate.Hp).ThenBy(candidate => candidate.ProcessingOrder).First();
}

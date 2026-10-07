using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class HealingTargetRules
{
    internal static IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor)
    {
        var faction = battle.GetActor(actor).Faction;
        return Array.AsReadOnly(battle.Actors.Where(target => target.Hp > 0 && target.Position is not null &&
            target.Faction == faction).Select(target => target.Actor).ToArray());
    }

    internal static BattleActorState RequireTarget(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, byte minimumRange, byte maximumRange, ActorRef targetRef)
    {
        var actor = battle.GetActor(actorRef);
        var target = battle.Actors.SingleOrDefault(a => a.Actor == targetRef);
        if (target is null || target.Hp == 0 || target.Faction != actor.Faction)
            throw new BattleRuleException("invalid-heal-target", "target");
        var targetPosition = targetRef == actorRef ? destination : target.Position!;
        if (!BattleRange.Contains(destination, targetPosition, minimumRange, maximumRange))
            throw new BattleRuleException("target-range", "target");
        return target;
    }
}

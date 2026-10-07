using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal interface IHealingRule
{
    string Identity { get; }
    HealingSpellDefinition RequireSpell(EngineBattleState battle, ActorRef actor, SpellRef spell);
    IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor);
    BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        HealingSpellDefinition spell, ActorRef target);
    BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        SpellRef spell, ActorRef target, IBattleProgressionRule progression);
}

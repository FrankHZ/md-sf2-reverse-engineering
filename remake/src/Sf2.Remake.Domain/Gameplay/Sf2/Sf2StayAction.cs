using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class Sf2StayAction : IBattleActionRule
{
    public string Identity => "sf2-stay";
    public BattleActionKind Kind => BattleActionKind.Stay;
    public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) =>
        [new(new(Kind), "STAY")];
    public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action)
    {
        if (action != new BattleActionRef(Kind) || battle.GetActor(actor).Hp == 0)
            throw new BattleRuleException("stay-actor", "actor");
        return new();
    }
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) => [];
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target) => throw new BattleRuleException("stay-target", "target");
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression)
    {
        _ = RequireAction(battle, actor, action);
        if (target is not null) throw new BattleRuleException("stay-target", "target");
        return new(BattleMovement.Commit(battle, actor, destination), actor, destination, [], null, [], []);
    }
}

using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Authored;

internal abstract class AuthoredHealingRule : IHealingRule
{
    public abstract string Identity { get; }
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor) =>
        HealingTargetRules.QueryTargets(battle, actor);
    public HealingSpellDefinition RequireSpell(EngineBattleState battle, ActorRef actorRef, SpellRef spellRef)
    {
        var actor = battle.GetActor(actorRef);
        if (!actor.Spells.Contains(spellRef)) throw new BattleRuleException("spell-not-known", "spell");
        if (!battle.Definition.Spells.TryGetValue(spellRef, out var spell))
            throw new BattleRuleException("spell-effect", "spell", true);
        if (actor.Mp < spell.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
        return spell;
    }
    public virtual BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor,
        MapPosition destination, HealingSpellDefinition spell, ActorRef target) =>
        HealingTargetRules.RequireTarget(battle, actor, destination, spell.MinimumRange, spell.MaximumRange, target);

    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actorRef, MapPosition destination,
        SpellRef spellRef, ActorRef targetRef)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var spell = RequireSpell(battle, actorRef, spellRef);
        var actor = battle.GetActor(actorRef);
        var target = RequireTarget(battle, actorRef, destination, spell, targetRef);
        return Calculate(battle, actor, target, destination, spell);
    }
    protected abstract BattleActionResolution Calculate(EngineBattleState battle, BattleActorState actor,
        BattleActorState target, MapPosition destination, HealingSpellDefinition spell);
}

internal sealed class CappedHealingRule : AuthoredHealingRule
{
    public override string Identity => "authored-capped-healing";
    protected override BattleActionResolution Calculate(EngineBattleState battle, BattleActorState actor,
        BattleActorState target, MapPosition destination, HealingSpellDefinition spell)
    {
        var result = HealingRules.ResolvePriest(new(target.Hp, target.MaxHp, actor.Mp,
            actor.Exp ?? throw new BattleRuleException("unspecified-exp", "actor.exp", true),
            spell.Power, spell.MpCost, battle.MainSeed));
        uint validationSeed = result.MainAfter;
        _ = BattleGrowthRules.Award(actor, result.AwardedExp, ref validationSeed, []);
        return new(battle.With(actors: battle.Actors.Select(a => a.Actor == actor.Actor ? a.With(position: destination) : a),
                mainSeed: result.MainAfter), actor.Actor, destination,
            [new(actor.Actor, target.Actor, "heal", BattleReactionKind.Recovery, target.Hp, result.HpAfter,
                target.Mp, target.Mp, Amount: result.Recovery)], new(actor.Actor, result.AwardedExp),
            [new("rng-exp-plus", actor.Actor, result.PlusRoll.Before, result.PlusRoll.After, 16, result.PlusRoll.Value),
             new("rng-exp-minus", actor.Actor, result.MinusRoll.Before, result.MinusRoll.After, 16, result.MinusRoll.Value)], [], Spell: spell);
    }
}

internal sealed class HalfMissingHealingRule : AuthoredHealingRule
{
    public override string Identity => "authored-half-missing-healing";
    public override BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor,
        MapPosition destination, HealingSpellDefinition spell, ActorRef targetRef)
    {
        var target = base.RequireTarget(battle, actor, destination, spell, targetRef);
        if (target.Hp == target.MaxHp) throw new BattleRuleException("target-not-injured", "target.hp");
        return target;
    }
    protected override BattleActionResolution Calculate(EngineBattleState battle, BattleActorState actor,
        BattleActorState target, MapPosition destination, HealingSpellDefinition spell)
    {
        int recovery = Math.Min(spell.Power, (target.MaxHp - target.Hp + 1) / 2);
        return new(battle.With(actors: battle.Actors.Select(a => a.Actor == actor.Actor ? a.With(position: destination) : a)),
            actor.Actor, destination,
            [new(actor.Actor, target.Actor, "heal", BattleReactionKind.Recovery, target.Hp, (ushort)(target.Hp + recovery),
                target.Mp, target.Mp, Amount: recovery)], null, [], [], Spell: spell);
    }
}

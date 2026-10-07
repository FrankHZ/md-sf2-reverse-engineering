using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class Sf2HealingRule : IHealingRule
{
    public HealingSpellDefinition RequireSpell(EngineBattleState battle, ActorRef actorRef, SpellRef spellRef)
    {
        var actor = battle.GetActor(actorRef);
        if (!actor.Spells.Contains(spellRef)) throw new BattleRuleException("spell-not-known", "spell");
        if (!battle.Definition.Spells.TryGetValue(spellRef, out var spell))
            throw new BattleRuleException("spell-effect", "spell", true);
        if (spellRef.Level is < 1 or > 3)
            throw new BattleRuleException("healing-animation", "spell.level", true);
        if (actor.Definition.ClassRule != BattleClassRule.UnpromotedPriest)
            throw new BattleRuleException("healing-class", "actor.classRule", true);
        if (actor.Mp < spell.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
        return spell;
    }

    public string Identity => "sf2-healing";
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor) =>
        HealingTargetRules.QueryTargets(battle, actor);

    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, HealingSpellDefinition spell, ActorRef targetRef) =>
        HealingTargetRules.RequireTarget(battle, actorRef, destination, spell.MinimumRange, spell.MaximumRange, targetRef);
    public BattleActionResolution Prepare(
        EngineBattleState battle, ActorRef actorRef, MapPosition destination, SpellRef spellRef, ActorRef targetRef)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var spell = RequireSpell(battle, actorRef, spellRef);
        var actor = battle.GetActor(actorRef);
        var target = RequireTarget(battle, actorRef, destination, spell, targetRef);
        var resolution = HealingRules.ResolvePriest(new(target.Hp, target.MaxHp, actor.Mp,
            actor.Exp ?? throw new BattleRuleException("unspecified-exp", "actor.exp", true), spell.Power, spell.MpCost, battle.MainSeed));
        List<BattleEffect> effects = [
            new("rng-exp-plus", actorRef, resolution.PlusRoll.Before, resolution.PlusRoll.After, 16, resolution.PlusRoll.Value),
            new("rng-exp-minus", actorRef, resolution.MinusRoll.Before, resolution.MinusRoll.After, 16, resolution.MinusRoll.Value)];
        uint validationSeed = resolution.MainAfter;
        _ = BattleGrowthRules.Award(actor, resolution.AwardedExp, ref validationSeed, []);
        var prepared = battle.With(actors: battle.Actors.Select(a => a.Actor == actorRef
            ? a.With(position: destination) : a), mainSeed: resolution.MainAfter);
        return new(prepared, actorRef, destination,
            [new(actorRef, targetRef, "heal", BattleReactionKind.Recovery, target.Hp, resolution.HpAfter,
                target.Mp, target.Mp, Amount: resolution.Recovery)], new(actorRef, resolution.AwardedExp),
            effects.AsReadOnly(), [], Spell: spell);
    }
}

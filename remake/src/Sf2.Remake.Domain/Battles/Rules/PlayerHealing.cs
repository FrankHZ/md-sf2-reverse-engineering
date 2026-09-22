using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class PlayerHealing
{
    internal static HealingSpellDefinition RequireSpell(EngineBattleState battle, ActorRef actorRef, SpellRef spellRef)
    {
        var actor = battle.GetActor(actorRef);
        if (!actor.Spells.Contains(spellRef)) throw new BattleRuleException("spell-not-known", "spell");
        if (!battle.Definition.Spells.TryGetValue(spellRef, out var spell))
            throw new BattleRuleException("spell-effect", "spell", true);
        if (actor.Definition.ClassRule != BattleClassRule.UnpromotedPriest)
            throw new BattleRuleException("healing-class", "actor.classRule", true);
        if (actor.Mp < spell.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
        return spell;
    }

    internal static BattleActorState RequireTarget(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, HealingSpellDefinition spell, ActorRef targetRef) =>
        RequireTarget(battle, actorRef, destination, spell.MinimumRange, spell.MaximumRange, targetRef);

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

    internal static (EngineBattleState State, IReadOnlyList<BattleEffect> Effects) Resolve(
        EngineBattleState battle, ActorRef actorRef, MapPosition destination, SpellRef spellRef, ActorRef targetRef)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var spell = RequireSpell(battle, actorRef, spellRef);
        var actor = battle.GetActor(actorRef);
        var target = RequireTarget(battle, actorRef, destination, spell, targetRef);
        var resolution = HealingRules.ResolvePriest(new(target.Hp, target.MaxHp, actor.Mp,
            actor.Exp ?? throw new BattleRuleException("unspecified-exp", "actor.exp", true), spell.Power, spell.MpCost, battle.MainSeed));
        uint seed = resolution.MainAfter;
        List<BattleEffect> effects = [new("mp", actorRef, actor.Mp, resolution.MpAfter),
            new("hp", targetRef, target.Hp, resolution.HpAfter)];
        var rewarded = BattleGrowthRules.Award(actor, resolution.AwardedExp, ref seed, effects);
        var actors = battle.Actors.Select(a =>
        {
            var changed = a.Actor == actorRef ? rewarded.With(mp: resolution.MpAfter, position: destination) : a;
            return a.Actor == targetRef ? changed.With(hp: resolution.HpAfter) : changed;
        });
        return (battle.With(actors: actors, mainSeed: seed), effects.AsReadOnly());
    }
}

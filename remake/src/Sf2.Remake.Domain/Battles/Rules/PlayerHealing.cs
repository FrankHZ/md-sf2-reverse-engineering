using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class PlayerHealing
{
    internal static HealingSpellDefinition RequireSpell(EngineBattleState battle, ActorRef actorRef, SpellRef spellRef)
    {
        var actor = battle.GetActor(actorRef);
        if (actor.Definition.ClassRule != BattleClassRule.UnpromotedPriest)
            throw new BattleRuleException("healing-class", "actor.classRule", true);
        if (!actor.Definition.Spells.Contains(spellRef) || !battle.Definition.Spells.TryGetValue(spellRef, out var spell))
            throw new BattleRuleException("spell-not-known", "spell");
        if (actor.Mp < spell.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
        return spell;
    }

    internal static BattleActorState RequireTarget(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, HealingSpellDefinition spell, ActorRef targetRef)
    {
        var actor = battle.GetActor(actorRef);
        var target = battle.Actors.SingleOrDefault(a => a.Actor == targetRef);
        if (target is null || target.Hp == 0 || target.Faction != actor.Faction)
            throw new BattleRuleException("invalid-heal-target", "target");
        var targetPosition = targetRef == actorRef ? destination : target.Position!;
        if (!BattleRange.Contains(destination, targetPosition, spell.MinimumRange, spell.MaximumRange))
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
        HealingResolution resolution;
        try
        {
            resolution = HealingRules.ResolvePriest(new(target.Hp, target.Definition.MaxHp, actor.Mp,
                actor.Exp, spell.Power, spell.MpCost, battle.MainSeed));
        }
        catch (NotSupportedException) { throw new BattleRuleException("level-up", "actor.exp", true); }
        var actors = battle.Actors.Select(a =>
        {
            var changed = a.Actor == actorRef ? a.With(mp: resolution.MpAfter, exp: resolution.ExpAfter, position: destination) : a;
            return a.Actor == targetRef ? changed.With(hp: resolution.HpAfter) : changed;
        });
        return (battle.With(actors: actors, mainSeed: resolution.MainAfter), Array.AsReadOnly<BattleEffect>([
            new("mp", actorRef, actor.Mp, resolution.MpAfter),
            new("hp", targetRef, target.Hp, resolution.HpAfter),
            new("exp", actorRef, actor.Exp, resolution.ExpAfter)]));
    }
}

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
        EngineBattleState battle, ActorRef actorRef, MapPosition destination, SpellRef spellRef, ActorRef targetRef, IBattleProgressionRule progression)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var spell = RequireSpell(battle, actorRef, spellRef);
        var actor = battle.GetActor(actorRef);
        var target = RequireTarget(battle, actorRef, destination, spell, targetRef);
        int recovery = Math.Min(spell.Power, target.MaxHp - target.Hp);
        BattleReaction[] reactions = [new(actorRef, targetRef, "heal", BattleReactionKind.Recovery, target.Hp,
            (ushort)(target.Hp + recovery), target.Mp, target.Mp, Amount: recovery)];
        var award = BattleProgressionRules.Award(progression, battle, actor, BattleActionKind.Healing, reactions);
        var prepared = battle.With(actors: battle.Actors.Select(a => a.Actor == actorRef
            ? a.With(position: destination) : a), mainSeed: award.Seed);
        return new(prepared, actorRef, destination, reactions, new(actorRef, award.Amount), award.Effects, [], Spell: spell);
    }
}

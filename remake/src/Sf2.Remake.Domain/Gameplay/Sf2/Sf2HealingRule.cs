using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class Sf2HealingRule : IHealingRule
{
    // DetermineHealingSpellLevel can select HEAL3/4 using a cheaper packed key.
    internal static bool CanPay(BattleActorState actor, HealingSpellDefinition spell) =>
        actor.Mp >= spell.MpCost ||
        (!actor.IsAlly && actor.Control == BattleControl.Automatic || BattleControlRules.UsesTemporaryAllyAi(actor)) &&
        actor.Spells.Contains(new SpellRef("heal", 4)) && spell.Spell.Value == "heal" &&
        (spell.Spell.Level == 3 && actor.Mp >= 3 || spell.Spell.Level == 4 && actor.Mp >= 14);

    // VICR and imported enemies apply their source-class scaling to the raw operand.
    // Authored priest inputs already supply adjusted power.
    internal static int? RecoveryLimit(BattleActorState caster, HealingSpellDefinition spell)
    {
        if (spell.Targets == HealingTargets.AuraAllAllies ||
            spell.Spell is { Value: "heal", Level: 4 } && spell.Power == 255) return null;
        return caster.Definition.ClassRule == BattleClassRule.Vicar || caster.Deployment.SourceEnemy.HasValue
            ? caster.Definition.CastPower(spell.Power) : spell.Power;
    }

    public HealingSpellDefinition RequireSpell(EngineBattleState battle, ActorRef actorRef, SpellRef spellRef)
    {
        var actor = battle.GetActor(actorRef);
        if (!actor.Spells.Contains(spellRef)) throw new BattleRuleException("spell-not-known", "spell");
        if (!battle.Definition.Spells.TryGetValue(spellRef, out var spell))
            throw new BattleRuleException("spell-effect", "spell", true);
        if (spell.CuresPoison && (spellRef.Level != 1 && spellRef is not { Value: "detox", Level: 2 or 3 } ||
            spell.IsAura || spell.Power != 0 || spell.Radius != 0))
            throw new BattleRuleException("poison-cure-definition", "spell", true);
        if (spell.CuresPoison && !actor.IsAlly)
            throw new BattleRuleException("enemy-poison-cure", "spell", true);
        if (!spell.IsAura && spellRef.Level is < 1 or > 4)
            throw new BattleRuleException("healing-animation", "spell.level", true);
        if (actor.IsAlly ? !Sf2ClassRules.PlayerHealer(actor.Definition.ClassRule) :
            actor.Control != BattleControl.Automatic || actor.Definition.ClassRule != BattleClassRule.Ordinary ||
                spellRef is not ({ Value: "heal", Level: >= 1 and <= 4 } or { Value: "aura", Level: 1 }) ||
                !(SilenceRules.Supported(actor.Status, battle.Definition.Poison) && (actor.Status & 0xC0) == 0))
            throw new BattleRuleException("healing-class", "actor.classRule", true);
        if (spell.IsAura) HealingTargetRules.RequireAuraCaster(battle, actor);
        if (!CanPay(actor, spell)) throw new BattleRuleException("insufficient-mp", "actor.mp");
        return spell;
    }

    public string Identity => "sf2-healing";
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor) =>
        HealingTargetRules.QueryTargets(battle, actor);

    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, HealingSpellDefinition spell, ActorRef targetRef)
    {
        var targets = HealingTargetRules.Resolve(battle, actorRef, destination, spell, targetRef);
        return spell.IsAura ? battle.GetActor(targetRef) : targets[0];
    }
    public BattleActionResolution Prepare(
        EngineBattleState battle, ActorRef actorRef, MapPosition destination, SpellRef spellRef, ActorRef targetRef, IBattleProgressionRule progression)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var spell = RequireSpell(battle, actorRef, spellRef);
        var actor = battle.GetActor(actorRef);
        var targets = HealingTargetRules.Resolve(battle, actorRef, destination, spell, targetRef);
        BattleReaction[] reactions = targets.Select(target =>
        {
            if (spell.CuresPoison) return PoisonCureRules.Reaction(actorRef, target, "heal", PoisonCureRules.SpellMask(spellRef.Level));
            int power = RecoveryLimit(actor, spell) ?? target.MaxHp;
            int recovery = Math.Min(power, target.MaxHp - target.Hp);
            return new BattleReaction(actorRef, target.Actor, "heal", BattleReactionKind.Recovery, target.Hp,
                (ushort)(target.Hp + recovery), target.Mp, target.Mp, Amount: recovery);
        }).ToArray();
        List<BattleEffect> effects = [];
        var checkedBattle = spell.CuresPoison ? PoisonCureRules.PrepareIneffective(battle, actorRef, reactions, effects) : battle;
        var award = actor.IsAlly ? BattleProgressionRules.Award(progression, checkedBattle, actor, BattleActionKind.Healing, reactions)
            : (Amount: 0, Seed: battle.MainSeed, Effects: (IReadOnlyList<BattleEffect>)Array.Empty<BattleEffect>());
        effects.AddRange(award.Effects);
        var prepared = battle.With(actors: battle.Actors.Select(a => a.Actor == actorRef
            ? a.With(position: destination) : a), mainSeed: award.Seed);
        return new(prepared, actorRef, destination, reactions, actor.IsAlly ? new(actorRef, award.Amount) : null, effects.AsReadOnly(), [], Spell: spell);
    }
}

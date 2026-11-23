using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime;

public enum SpellEffect
{
    RestoreHp, FullHp, CurePoison, CurePoisonAndParalysis, Damage,
    Silence, Sleep, LowerDefenseAndAgility, RaiseDefenseAndAgility, RaiseAttack,
    Retreat, ReturnToRefuge, NoEffect, CurePoisonParalysisAndCurse,
}

public enum SpellTarget { Self, Ally, Enemy, AllAllies, PartyMember }

// A read of one offered level in its current caller. Selection and payment stay with the rules.
public sealed record SpellDescriptionFacts(SpellRef Spell, byte? MpCost, SpellEffect? Effect = null,
    SpellTarget? Target = null, byte? MinimumRange = null, byte? MaximumRange = null, byte Radius = 0,
    AttackSpellElement? Element = null, int? HealingLimit = null, byte? FieldPayment = null,
    bool SuppressedBySilence = false, SessionFailure? Reason = null)
{
    internal static SpellEffect DescribeCure(byte rank) => PoisonCureRules.SpellMask(rank) switch
    {
        PoisonCureRules.Poison => SpellEffect.CurePoison,
        PoisonCureRules.PoisonAndStun => SpellEffect.CurePoisonAndParalysis,
        _ => SpellEffect.CurePoisonParalysisAndCurse,
    };

    internal static SpellDescriptionFacts Battle(EngineBattleState battle, BattleActorState caster,
        BattleActionRef action, BattleActionAdmission? admission, IHealingRule healingRule, SessionFailure? reason)
    {
        var spell = action.Spell!.Value;
        var definition = battle.Definition;
        var healing = admission?.Spell ?? (action.Kind == BattleActionKind.Healing ? definition.Spells.GetValueOrDefault(spell) : null);
        var attack = admission?.AttackSpell ?? (action.Kind == BattleActionKind.AttackSpell ? definition.AttackSpells.GetValueOrDefault(spell) : null);
        var support = admission?.SupportSpell ?? (action.Kind == BattleActionKind.SupportSpell ? definition.SupportSpells.GetValueOrDefault(spell) : null);
        var retreat = admission?.Retreat ?? (action.Kind == BattleActionKind.Retreat ? definition.RetreatSpells.GetValueOrDefault(spell) : null);
        byte? mpCost = healing?.MpCost ?? attack?.MpCost ?? support?.MpCost ?? retreat?.MpCost;
        var facts = new SpellDescriptionFacts(spell, mpCost, Reason: reason);
        // MP refusal follows effect validation. Read its operands even though this caster cannot pay.
        if (admission is null && reason?.Code != "insufficient-mp") return facts;
        if (healing is not null)
        {
            var effect = healing.CuresPoison ? DescribeCure(spell.Level) : SpellEffect.RestoreHp;
            int? limit = null;
            if (!healing.CuresPoison && healingRule is Sf2HealingRule)
            {
                limit = Sf2HealingRule.RecoveryLimit(caster, healing);
                if (limit is null) effect = SpellEffect.FullHp;
            }
            bool all = healing.Targets == HealingTargets.AuraAllAllies;
            facts = facts with { Effect = effect, Target = all ? SpellTarget.AllAllies : SpellTarget.Ally,
                MinimumRange = healing.MinimumRange, MaximumRange = healing.MaximumRange,
                Radius = healing.Radius, HealingLimit = limit };
        }
        else if (attack is not null)
            facts = facts with { Effect = SpellEffect.Damage, Target = SpellTarget.Enemy,
                MinimumRange = attack.MinimumRange, MaximumRange = attack.MaximumRange,
                Radius = attack.Radius, Element = attack.Element };
        else if (support is not null)
            facts = facts with { Effect = support.Effect switch
            {
                SupportSpellEffect.Silence => SpellEffect.Silence,
                SupportSpellEffect.Sleep => SpellEffect.Sleep,
                SupportSpellEffect.Slow => SpellEffect.LowerDefenseAndAgility,
                SupportSpellEffect.Boost => SpellEffect.RaiseDefenseAndAgility,
                SupportSpellEffect.Attack => SpellEffect.RaiseAttack,
                _ => null,
            }, Target = support.TargetsTeammates ? SpellTarget.Ally : SpellTarget.Enemy,
                MinimumRange = support.MinimumRange, MaximumRange = support.MaximumRange, Radius = support.Radius };
        else if (retreat is not null)
            facts = facts with { Effect = SpellEffect.Retreat, Target = SpellTarget.Self };
        return facts with { SuppressedBySilence = reason is null && admission is not null &&
            SilenceRules.Present(caster.Status) && SilenceRules.Cost(action, admission) is not null };
    }
}

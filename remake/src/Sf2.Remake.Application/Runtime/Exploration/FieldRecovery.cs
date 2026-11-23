using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum FieldRecoveryEffect
{
    NoEffect, CurePoison, Egress, MapEvent, CurePoisonAndParalysis,
    IncreaseAttack, IncreaseDefense, IncreaseAgility, IncreaseMove, IncreaseHp, IncreaseMp, LevelUp,
}
public sealed record FieldSpellChoice(SpellRef Spell, string Name, byte MpCost, FieldRecoveryEffect Effect)
{
    public SpellDescriptionFacts? Description { get; init; }
}

// Field caller policies are admitted separately from battle recovery and its EXP/RNG rolls.
internal static class FieldRecovery
{
    internal static SpellDescriptionFacts Describe(ScenarioDefinition definition, SessionSnapshot current,
        PartyMemberDetails caster, FieldSpellChoice spell)
    {
        SpellEffect? effect = spell.Effect switch
        {
            FieldRecoveryEffect.NoEffect => SpellEffect.NoEffect,
            FieldRecoveryEffect.CurePoison => SpellEffect.CurePoison,
            FieldRecoveryEffect.CurePoisonAndParalysis => SpellDescriptionFacts.DescribeCure(spell.Spell.Level),
            FieldRecoveryEffect.Egress => SpellEffect.ReturnToRefuge,
            _ => null,
        };
        var facts = new SpellDescriptionFacts(spell.Spell, spell.MpCost, effect, effect switch
        {
            SpellEffect.CurePoison or SpellEffect.CurePoisonAndParalysis or SpellEffect.CurePoisonParalysisAndCurse => SpellTarget.PartyMember,
            _ => null,
        });
        try
        {
            if (effect is null) throw new BattleRuleException("field-spell-unsupported", "spell", true);
            RequireSpellMember(spell, caster);
            if (spell.Effect == FieldRecoveryEffect.Egress)
                _ = FieldEgress.RequireRoute(definition, current);
            return facts with { FieldPayment = SpellPayment(spell, caster.Carried.Mp) };
        }
        catch (BattleRuleException error)
        {
            return facts with { Reason = new(error.Unsupported ? SessionFailureKind.UnsupportedCapability :
                SessionFailureKind.IllegalCommand, error.Code, error.Field, error.Code.Replace('-', ' ')) };
        }
    }

    private static void RequireSpellMember(FieldSpellChoice spell, PartyMemberDetails member)
    {
        if (spell is { Spell.Level: > 1, Effect: FieldRecoveryEffect.CurePoisonAndParalysis })
            FieldInventory.RequireDetoxEquipment(member);
    }

    // ExecuteExplorationMagicMenu allows low MP. The paid branch clamps at zero.
    private static byte SpellPayment(FieldSpellChoice spell, byte mp) =>
        spell.Effect is FieldRecoveryEffect.NoEffect or FieldRecoveryEffect.MapEvent ? (byte)0 : Math.Min(mp, spell.MpCost);

    internal static BattleStartInput Prepare(ScenarioDefinition definition, SessionSnapshot current, FieldCommand command,
        out FieldRecoveryEffect effect, out bool effective, out FieldGrowthResult? growth, out ushort clearedStatus,
        FieldItemCaller caller = FieldItemCaller.Field)
    {
        var member = FieldInventory.Member(definition, current, command.Member, caller);
        var party = current.Exploration!.Party;
        var capabilities = definition.FieldPreparation!;
        FieldSpellChoice? spell = null;
        FieldInventoryChoice? item = null;
        ActorRef? targetRef;
        if (command is FieldUseItem use)
        {
            item = FieldInventory.Slot(definition, member, use.Slot, use.ExpectedWord);
            if ((caller == FieldItemCaller.Caravan ? CaravanEffect(capabilities, (byte)(item.Word & 127)) :
                capabilities.Effect((byte)(item.Word & 127), current.Exploration!.Map)) is not { } itemEffect)
                throw new BattleRuleException("field-item-effect-unsupported", "item", true);
            effect = itemEffect;
            targetRef = use.Target;
        }
        else if (command is FieldCast cast && caller == FieldItemCaller.Field)
        {
            var learned = member.Carried.Progress?.Spells ?? member.Deployment!.Definition.Spells;
            spell = capabilities.Spell(cast.Spell, current.Exploration.Map);
            if (!learned.Contains(cast.Spell) || spell is null)
                throw new BattleRuleException("field-spell-unsupported", "spell", true);
            effect = spell.Effect; targetRef = cast.Target;
        }
        else throw new BattleRuleException("field-recovery-command", "command");
        effective = false;
        growth = null;
        clearedStatus = 0;
        if (effect is FieldRecoveryEffect.NoEffect or FieldRecoveryEffect.MapEvent)
        {
            // A map event owns any removal, after its scene. An unmatched use is free.
            if (targetRef is not null) throw new BattleRuleException("field-no-effect-target", "target");
            return party;
        }
        if (effect == FieldRecoveryEffect.Egress)
        {
            if (spell is null || targetRef is not null)
                throw new BattleRuleException("field-egress-target", "target");
            return FieldInventory.Replace(party,
                [member.Carried with { Mp = (byte)(member.Carried.Mp - SpellPayment(spell, member.Carried.Mp)) }]);
        }
        var target = FieldInventory.Member(definition, current,
            targetRef ?? throw new BattleRuleException("field-recovery-target", "target"), caller);
        if (spell is not null)
        {
            RequireSpellMember(spell, member);
            if (target.Carried.Actor != member.Carried.Actor) RequireSpellMember(spell, target);
        }
        uint seed = party.MainSeed;
        BattleActorStartInput changedTarget;
        if (item is not null && FieldGrowthItems.Admitted(effect))
        {
            (changedTarget, growth) = FieldGrowthItems.Apply(target, effect, ref seed);
            effective = !growth.NoEffect;
        }
        else
        {
            ushort mask = effect != FieldRecoveryEffect.CurePoisonAndParalysis ? PoisonCureRules.Poison :
                spell is null ? PoisonCureRules.PoisonAndStun : PoisonCureRules.SpellMask(spell.Spell.Level);
            clearedStatus = (ushort)(target.Carried.Status & mask);
            changedTarget = target.Carried with { Status = (ushort)(target.Carried.Status & ~clearedStatus) };
            var targetDefinition = target.Deployment!.Definition;
            if (CurseRules.Present(clearedStatus))
                changedTarget = FieldInventory.WithLoadout(changedTarget,
                    BattleEquipment.RemoveCurse(targetDefinition, FieldInventory.Loadout(target)));
            effective = clearedStatus != 0;
            if (spell is not null || effect == FieldRecoveryEffect.CurePoisonAndParalysis)
                changedTarget = changedTarget.RefreshStats(targetDefinition);
        }
        bool self = changedTarget.Actor == member.Carried.Actor;
        var changedMember = self ? changedTarget : member.Carried;
        if (spell is not null) changedMember = changedMember with { Mp = (byte)(changedMember.Mp - SpellPayment(spell, changedMember.Mp)) };
        else
        {
            var pack = changedMember.SourceLoadout ?? changedMember.Progress?.SourceLoadout ?? FieldInventory.Loadout(member);
            changedMember = FieldInventory.WithLoadout(changedMember, BattleInventory.Remove(pack, item!.Slot)).RefreshStats();
        }
        var prepared = FieldInventory.Replace(party, self ? [changedMember] : [changedMember, changedTarget]);
        return seed == party.MainSeed ? prepared : prepared.WithSeeds(seed, party.ThinkingSeed);
    }

    internal static FieldRecoveryEffect CaravanEffect(FieldPreparationDefinition capabilities, byte item)
    {
        // The source Caravan caller checks IsItemUsableOnField before UseItemOnField.
        // Map-specific events and ordinary-field NoEffect policies are separate callers.
        if (item is not (3 or 5 or >= 9 and <= 15))
            throw new BattleRuleException("caravan-item-not-usable", "item");
        var effect = capabilities.ItemEffects.GetValueOrDefault(item);
        if (item == 3 && effect == FieldRecoveryEffect.CurePoison ||
            item == 5 && effect == FieldRecoveryEffect.CurePoisonAndParalysis ||
            item >= 9 && FieldGrowthItems.Admitted(effect)) return effect;
        throw new BattleRuleException("field-item-effect-unsupported", "item", true);
    }
}

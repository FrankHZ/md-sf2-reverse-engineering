using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum FieldRecoveryEffect { NoEffect, CurePoison, Egress, MapEvent, CurePoisonAndParalysis }
public sealed record FieldSpellChoice(SpellRef Spell, string Name, byte MpCost, FieldRecoveryEffect Effect);

// Field caller policies are admitted separately from battle recovery and its EXP/RNG rolls.
internal static class FieldRecovery
{
    internal static BattleStartInput Prepare(ScenarioDefinition definition, SessionSnapshot current, FieldCommand command,
        out FieldRecoveryEffect effect, out bool effective, FieldItemCaller caller = FieldItemCaller.Field)
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
                [member.Carried with { Mp = (byte)Math.Max(0, member.Carried.Mp - spell.MpCost) }]);
        }
        var target = FieldInventory.Member(definition, current,
            targetRef ?? throw new BattleRuleException("field-recovery-target", "target"), caller);
        // Source status bits 1 and 0 are poison and paralysis respectively.
        int cureMask = effect == FieldRecoveryEffect.CurePoisonAndParalysis ? 3 : 2;
        var changedTarget = target.Carried with { Status = (ushort)(target.Carried.Status & ~cureMask) };
        effective = changedTarget.Status != target.Carried.Status;
        var changedMember = member.Carried;
        // ExecuteExplorationMagicMenu does not check MP. DecreaseCurrentMp clamps at zero.
        if (spell is not null) changedMember = changedMember with { Mp = (byte)Math.Max(0, changedMember.Mp - spell.MpCost) };
        else changedMember = FieldInventory.WithLoadout(member, BattleInventory.Remove(FieldInventory.Loadout(member), item!.Slot));
        if (target.Carried.Actor == member.Carried.Actor)
            return FieldInventory.Replace(party, [changedMember with { Status = changedTarget.Status }]);
        return FieldInventory.Replace(party, [changedMember, changedTarget]);
    }

    internal static FieldRecoveryEffect CaravanEffect(FieldPreparationDefinition capabilities, byte item)
    {
        // The source Caravan caller checks IsItemUsableOnField before UseItemOnField.
        // Map-specific events and ordinary-field NoEffect policies are separate callers.
        if (item is not (3 or 5 or >= 9 and <= 15))
            throw new BattleRuleException("caravan-item-not-usable", "item");
        return (item, capabilities.ItemEffects.GetValueOrDefault(item)) switch
        {
            (3, FieldRecoveryEffect.CurePoison) => FieldRecoveryEffect.CurePoison,
            (5, FieldRecoveryEffect.CurePoisonAndParalysis) => FieldRecoveryEffect.CurePoisonAndParalysis,
            _ => throw new BattleRuleException("field-item-effect-unsupported", "item", true),
        };
    }
}

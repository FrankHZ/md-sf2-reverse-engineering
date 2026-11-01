using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum FieldRecoveryEffect { NoEffect, CurePoison }
public sealed record FieldSpellChoice(SpellRef Spell, string Name, byte MpCost, FieldRecoveryEffect Effect);

// Field caller policies are admitted separately from battle recovery and its EXP/RNG rolls.
internal static class FieldRecovery
{
    internal static BattleStartInput Prepare(ScenarioDefinition definition, SessionSnapshot current, FieldCommand command, out bool effective)
    {
        var member = FieldInventory.Member(definition, current, command.Member);
        var party = current.Exploration!.Party;
        var capabilities = definition.FieldPreparation!;
        FieldRecoveryEffect effect;
        FieldSpellChoice? spell = null;
        FieldInventoryChoice? item = null;
        ActorRef? targetRef;
        if (command is FieldUseItem use)
        {
            item = FieldInventory.Slot(definition, member, use.Slot, use.ExpectedWord);
            if (!capabilities.ItemEffects.TryGetValue((byte)(item.Word & 127), out effect))
                throw new BattleRuleException("field-item-effect-unsupported", "item", true);
            targetRef = use.Target;
        }
        else if (command is FieldCast cast)
        {
            var learned = member.Carried.Progress?.Spells ?? member.Deployment!.Definition.Spells;
            if (!learned.Contains(cast.Spell) || !capabilities.Spells.TryGetValue(cast.Spell, out spell))
                throw new BattleRuleException("field-spell-unsupported", "spell", true);
            effect = spell.Effect; targetRef = cast.Target;
        }
        else throw new BattleRuleException("field-recovery-command", "command");
        effective = false;
        if (effect == FieldRecoveryEffect.NoEffect)
        {
            // mainactions byte_21348 / non-usable item map-event fallthrough: no payment.
            // This bounded family has no item map event. Do not consume ordinary recovery items.
            if (targetRef is not null) throw new BattleRuleException("field-no-effect-target", "target");
            return party;
        }
        var target = FieldInventory.Member(definition, current,
            targetRef ?? throw new BattleRuleException("field-recovery-target", "target"));
        var changedTarget = target.Carried with { Status = (ushort)(target.Carried.Status & ~2) };
        effective = changedTarget.Status != target.Carried.Status;
        var changedMember = member.Carried;
        // ExecuteExplorationMagicMenu does not check MP. DecreaseCurrentMp clamps at zero.
        if (spell is not null) changedMember = changedMember with { Mp = (byte)Math.Max(0, changedMember.Mp - spell.MpCost) };
        else changedMember = FieldInventory.WithLoadout(member, BattleInventory.Remove(FieldInventory.Loadout(member), item!.Slot));
        if (target.Carried.Actor == member.Carried.Actor)
            return FieldInventory.Replace(party, [changedMember with { Status = changedTarget.Status }]);
        return FieldInventory.Replace(party, [changedMember, changedTarget]);
    }
}

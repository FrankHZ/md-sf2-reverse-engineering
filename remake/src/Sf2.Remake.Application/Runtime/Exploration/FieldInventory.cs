using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

// Explicit field capability. Battle Give/Discard/AllowChanges are separate admissions.
public sealed class FieldPreparationDefinition
{
    internal FieldPreparationDefinition(IEnumerable<BattleDiscardItem> items,
        IEnumerable<KeyValuePair<byte, FieldRecoveryEffect>>? itemEffects = null, IEnumerable<FieldSpellChoice>? spells = null, IEnumerable<FieldItemMapEffect>? mapEffects = null)
    {
        MapEffects = Array.AsReadOnly((mapEffects ?? []).ToArray());
        Items = new ReadOnlyDictionary<byte, BattleDiscardItem>(items.ToDictionary(item => item.ItemId));
        ItemEffects = new ReadOnlyDictionary<byte, FieldRecoveryEffect>((itemEffects ?? []).ToDictionary());
        Spells = new ReadOnlyDictionary<SpellRef, FieldSpellChoice>((spells ?? []).ToDictionary(spell => spell.Spell));
    }
    public IReadOnlyDictionary<byte, BattleDiscardItem> Items { get; }
    public IReadOnlyDictionary<byte, FieldRecoveryEffect> ItemEffects { get; }
    public IReadOnlyList<FieldItemMapEffect> MapEffects { get; }
    internal FieldRecoveryEffect? Effect(byte item, MapId? map) =>
        MapEffects.FirstOrDefault(row => row.Item == item && row.Maps.Contains(map!))?.Effect ??
        (ItemEffects.TryGetValue(item, out var effect) ? effect : null);
    public IReadOnlyDictionary<SpellRef, FieldSpellChoice> Spells { get; }
}

public sealed record FieldItemMapEffect(byte Item, FieldRecoveryEffect Effect, IReadOnlyList<MapId> Maps);
public sealed record FieldInventoryChoice(int Slot, ushort Word, string Name, bool CanDrop, FieldRecoveryEffect? Use = null);
public sealed record FieldActionChoices(Guid SessionId, long Revision, ActorRef Actor,
    IReadOnlyList<BattleEquipmentChoice> Equipment, IReadOnlyList<FieldInventoryChoice> Items,
    IReadOnlyList<FieldSpellChoice> Spells, string? Error = null);

internal static class FieldInventory
{
    internal static PartyMemberDetails Member(ScenarioDefinition definition, SessionSnapshot current, ActorRef actor)
    {
        if (!current.CanWaitAtInput || current.Mode != SessionMode.Exploration || current.Story.Warp is not null ||
            current.Story.ScriptWarp is not null || current.Story.BattleRequest)
            throw new BattleRuleException("field-preparation-unavailable", "field");
        if (definition.FieldPreparation is null)
            throw new BattleRuleException("field-preparation-unsupported", "content.fieldPreparation", true);
        var member = PartyDetailsQuery.Query(definition, current)!.Members.SingleOrDefault(row => row.Carried.Actor == actor);
        if (member?.Deployment is null || member.Carried.Initialized == false)
            throw new BattleRuleException("field-member-unavailable", "actor");
        return member;
    }

    internal static BattleSourceLoadout Loadout(PartyMemberDetails member) =>
        member.Carried.SourceLoadout ?? member.Carried.Progress?.SourceLoadout ?? member.Deployment!.Definition.SourceLoadout
            ?? throw new BattleRuleException("field-loadout-unavailable", "actor.items", true);

    internal static IReadOnlyList<FieldInventoryChoice> Items(ScenarioDefinition definition, PartyMemberDetails member, MapId? map = null)
    {
        var loadout = Loadout(member);
        if (loadout.Items.Count != 4) throw new BattleRuleException("field-item-slots", "actor.items");
        if (member.Deployment!.Definition.Equipment is not null)
            BattleEquipment.ValidateLoadout(member.Deployment.Definition, loadout);
        List<FieldInventoryChoice> choices = [];
        for (int slot = 0; slot < 4; slot++)
        {
            ushort word = loadout.Items[slot]; byte id = (byte)(word & 127);
            if (word > 511) throw new BattleRuleException("field-item-word", "actor.items");
            if (id == 127) continue;
            if (!definition.FieldPreparation!.Items.TryGetValue(id, out var item))
                throw new BattleRuleException("field-item-unsupported", "actor.items", true);
            if ((word & 128) != 0 && member.Deployment.Definition.Equipment?.Weapons.ContainsKey(id) != true)
                throw new BattleRuleException("field-equipped-item-unsupported", "actor.items", true);
            choices.Add(new(slot, word, item.Name, !item.Unsellable,
                definition.FieldPreparation.Effect(id, map)));
        }
        return choices.AsReadOnly();
    }

    internal static byte BaseAttack(PartyMemberDetails member) => member.Carried.Progress?.BaseAttack ??
        member.Deployment!.Definition.Equipment!.BaseAttack;

    internal static FieldActionChoices Query(ScenarioDefinition definition, SessionSnapshot current, ActorRef actor)
    {
        try
        {
            var member = Member(definition, current, actor);
            var items = Items(definition, member, current.Exploration!.Map);
            var equipment = member.Deployment!.Definition.Equipment is null ? [] :
                BattleEquipment.Choices(member.Deployment.Definition, BaseAttack(member), Loadout(member));
            var learned = member.Carried.Progress?.Spells ?? member.Deployment.Definition.Spells;
            return new(current.SessionId, current.Revision, actor, equipment, items,
                learned.Where(definition.FieldPreparation!.Spells.ContainsKey).Select(spell => definition.FieldPreparation.Spells[spell]).ToArray());
        }
        catch (BattleRuleException error) { return new(current.SessionId, current.Revision, actor, [], [], [], error.Code); }
    }

    internal static BattleActorStartInput WithLoadout(PartyMemberDetails member, BattleSourceLoadout loadout) =>
        member.Carried with { SourceLoadout = loadout, Progress = member.Carried.Progress is { } progress
            ? progress with { SourceLoadout = loadout } : null };

    internal static BattleStartInput Replace(BattleStartInput party, IEnumerable<BattleActorStartInput> changed, BattleItemCustody? custody = null, uint? gold = null)
    {
        var actors = changed.ToDictionary(actor => actor.Actor);
        return new(party.Encounter, party.Actors.Select(actor => actors.GetValueOrDefault(actor.Actor, actor)),
            party.MainSeed, party.ThinkingSeed, gold ?? party.Gold, party.NewBattle, party.ActiveAllies, custody ?? party.ItemCustody,
            party.StoryFlags, party.Visuals, party.RetainedSourceSlots, party.DeathCredit);
    }

    internal static FieldInventoryChoice Slot(ScenarioDefinition definition, PartyMemberDetails member, int slot, ushort expectedWord)
    {
        var choice = Items(definition, member).SingleOrDefault(row => row.Slot == slot)
            ?? throw new BattleRuleException("field-item-slot", "item.slot");
        if (choice.Word != expectedWord) throw new BattleRuleException("field-stale-item", "item.word");
        return choice;
    }

    internal static BattleStartInput Prepare(ScenarioDefinition definition, SessionSnapshot current, FieldCommand command)
    {
        var member = Member(definition, current, command.Member);
        var party = current.Exploration!.Party;
        var loadout = Loadout(member);
        switch (command)
        {
            case FieldEquip equip:
                _ = Items(definition, member);
                if (member.Deployment!.Definition.Equipment is null)
                    throw new BattleRuleException("equipment-unavailable", "actor.equipment", true);
                var choice = BattleEquipment.Choices(member.Deployment!.Definition, BaseAttack(member), loadout)
                    .SingleOrDefault(row => row.Slot == equip.Slot) ?? throw new BattleRuleException("equipment-slot", "equipment.slot");
                if ((equip.Slot == 4 ? (ushort?)null : loadout.Items[equip.Slot]) != equip.ExpectedWord)
                    throw new BattleRuleException("field-stale-item", "item.word");
                return Replace(party, [WithLoadout(member, BattleEquipment.Equip(member.Deployment.Definition, loadout, choice.Slot))]);
            case FieldGive give:
                var given = Slot(definition, member, give.Slot, give.ExpectedWord);
                var target = Member(definition, current, give.Target);
                _ = Items(definition, target);
                var targetLoadout = Loadout(target);
                bool full = targetLoadout.Items.All(word => (word & 127) != 127);
                if (full != give.ReturnSlot.HasValue || give.ReturnSlot.HasValue != give.ExpectedReturnWord.HasValue)
                    throw new BattleRuleException("field-give-exchange-required", "target.items");
                if (give.Target == command.Member)
                {
                    // Field @StartExchange unequips first, skips equal self slots, and
                    // adjusts a later return slot after the compacting donor removal.
                    var own = loadout;
                    if (give.ReturnSlot is { } ownReturn)
                    {
                        var returned = Slot(definition, target, ownReturn, give.ExpectedReturnWord!.Value);
                        if (ownReturn == given.Slot)
                        {
                            var words = own.Items.ToArray(); words[given.Slot] &= unchecked((ushort)~128);
                            return Replace(party, [WithLoadout(member, new(words, own.Spells))]);
                        }
                        own = BattleInventory.Add(BattleInventory.Remove(own, given.Slot), returned.Word);
                        own = BattleInventory.Remove(own, ownReturn > given.Slot ? ownReturn - 1 : ownReturn);
                    }
                    else own = BattleInventory.Remove(own, given.Slot);
                    return Replace(party, [WithLoadout(member, BattleInventory.Add(own, given.Word))]);
                }
                var donor = BattleInventory.Remove(loadout, given.Slot);
                if (give.ReturnSlot is { } returnSlot)
                {
                    var returned = Slot(definition, target, returnSlot, give.ExpectedReturnWord!.Value);
                    targetLoadout = BattleInventory.Remove(targetLoadout, returnSlot);
                    donor = BattleInventory.Add(donor, returned.Word);
                }
                targetLoadout = BattleInventory.Add(targetLoadout, given.Word);
                return Replace(party, [WithLoadout(member, donor), WithLoadout(target, targetLoadout)]);
            case FieldDrop drop:
                var removed = Slot(definition, member, drop.Slot, drop.ExpectedWord);
                var item = definition.FieldPreparation!.Items[(byte)(removed.Word & 127)];
                if (!removed.CanDrop) throw new BattleRuleException("discard-unsellable", "item");
                if (item.Rare && party.ItemCustody is null) throw new BattleRuleException("discard-custody-unavailable", "party.itemCustody", true);
                return Replace(party, [WithLoadout(member, BattleInventory.Remove(loadout, drop.Slot))],
                    item.Rare ? party.ItemCustody!.AddDeal(item.ItemId) : party.ItemCustody);
            default: throw new BattleRuleException("field-command-unsupported", "command", true);
        }
    }
}

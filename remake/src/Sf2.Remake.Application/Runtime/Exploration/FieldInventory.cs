using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

// Explicit field capability. Battle Give/Discard/AllowChanges are separate admissions.
public sealed class FieldPreparationDefinition
{
    internal FieldPreparationDefinition(IEnumerable<BattleDiscardItem> items,
        IEnumerable<KeyValuePair<byte, FieldRecoveryEffect>>? itemEffects = null, IEnumerable<FieldSpellChoice>? spells = null, IEnumerable<FieldItemMapEffect>? mapEffects = null,
        FieldEgressDefinition? egress = null)
    {
        MapEffects = Array.AsReadOnly((mapEffects ?? []).ToArray());
        Items = new ReadOnlyDictionary<byte, BattleDiscardItem>(items.ToDictionary(item => item.ItemId));
        ItemEffects = new ReadOnlyDictionary<byte, FieldRecoveryEffect>((itemEffects ?? []).ToDictionary());
        Spells = new ReadOnlyDictionary<SpellRef, FieldSpellChoice>((spells ?? []).ToDictionary(spell => spell.Spell));
        Egress = egress;
    }
    public IReadOnlyDictionary<byte, BattleDiscardItem> Items { get; }
    public IReadOnlyDictionary<byte, FieldRecoveryEffect> ItemEffects { get; }
    public IReadOnlyList<FieldItemMapEffect> MapEffects { get; }
    internal FieldRecoveryEffect? Effect(byte item, MapId? map) =>
        MapEffects.FirstOrDefault(row => row.Item == item && row.Maps.Contains(map!))?.Effect ??
        (ItemEffects.TryGetValue(item, out var effect) ? effect : null);
    public IReadOnlyDictionary<SpellRef, FieldSpellChoice> Spells { get; }
    internal FieldEgressDefinition? Egress { get; }
    internal FieldSpellChoice? Spell(SpellRef spell, MapId map) => Spells.TryGetValue(spell, out var choice)
        ? choice.Effect == FieldRecoveryEffect.Egress && Egress?.Maps.Contains(map) != true
            ? choice with { Effect = FieldRecoveryEffect.NoEffect } : choice
        : null;
}

internal sealed record FieldEgressDefinition(IReadOnlyList<MapId> Maps, ProgramLocation Program);

public sealed record FieldItemMapEffect(byte Item, FieldRecoveryEffect Effect, IReadOnlyList<MapId> Maps);
public sealed record FieldInventoryChoice(int Slot, ushort Word, string Name, bool CanDrop, FieldRecoveryEffect? Use = null);
public sealed record FieldActionChoices(Guid SessionId, long Revision, ActorRef Actor,
    IReadOnlyList<BattleEquipmentChoice> Equipment, IReadOnlyList<FieldInventoryChoice> Items,
    IReadOnlyList<FieldSpellChoice> Spells, string? Error = null);

internal enum FieldItemCaller { Field, Caravan }

internal static class FieldInventory
{
    internal static PartyMemberDetails Member(ScenarioDefinition definition, SessionSnapshot current, ActorRef actor,
        FieldItemCaller caller = FieldItemCaller.Field)
    {
        bool available = caller == FieldItemCaller.Field ? current.CanWaitAtInput :
            current.Story.Wait is CaravanWait { Action: CaravanAction.Item };
        if (!available || current.Mode != SessionMode.Exploration || current.Story.Warp is not null ||
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
        var loadout = Pack(definition, member);
        ValidateEquipped(member, loadout);
        List<FieldInventoryChoice> choices = [];
        for (int slot = 0; slot < 4; slot++)
        {
            ushort word = loadout.Items[slot]; byte id = (byte)(word & 127);
            if (id == 127) continue;
            var item = definition.FieldPreparation!.Items.GetValueOrDefault(id);
            // A recognized carried identity is displayable without granting its operations/effects.
            string name = item?.Name ?? definition.PrivateDefinitions!.Items[id].DisplayName;
            choices.Add(new(slot, word, name, item is { Unsellable: false },
                item is null ? null : definition.FieldPreparation.Effect(id, map)));
        }
        return choices.AsReadOnly();
    }

    private static BattleSourceLoadout Pack(ScenarioDefinition definition, PartyMemberDetails member)
    {
        var loadout = Loadout(member);
        if (loadout.Items.Count != 4)
            throw new BattleRuleException("field-item-slots", "actor.items");
        foreach (ushort word in loadout.Items)
        {
            if (word > 511)
                throw new BattleRuleException("field-item-word", "actor.items");
            byte id = (byte)(word & 127);
            if (id != 127 && !definition.FieldPreparation!.Items.ContainsKey(id) &&
                definition.PrivateDefinitions?.Items.ContainsKey(id) != true)
                throw new BattleRuleException("field-item-unsupported", "actor.items", true);
        }
        return loadout;
    }

    private static void ValidateEquipped(PartyMemberDetails member, BattleSourceLoadout loadout)
    {
        var actor = member.Deployment!.Definition;
        if (actor.Equipment is not null)
        {
            // Only worn words contribute to the current stats. Unselected carried effects are inert.
            var worn = new BattleSourceLoadout(loadout.Items.Select(word =>
                (word & 128) != 0 ? word : (ushort)127), loadout.Spells);
            BattleEquipment.ValidateLoadout(actor, worn);
        }
        else if (loadout.Items.Any(word => (word & 128) != 0 && (word & 127) != 127))
            throw new BattleRuleException("field-equipped-item-unsupported", "actor.items", true);
    }

    internal static void RequireDetoxEquipment(ScenarioDefinition definition, PartyMemberDetails member)
    {
        PoisonCureRules.RequireUncursed(member.Carried.Status, "member.status");
        var loadout = Loadout(member);
        if (loadout.Items.Any(word => (word & 128) != 0 && CaravanDepotQuery.Cursed(definition, word)))
            throw new BattleRuleException("detox-curse-unsupported", "member.items", true);
        ValidateEquipped(member, loadout);
    }

    private static IReadOnlyList<BattleEquipmentChoice> EquipmentChoices(ScenarioDefinition definition,
        PartyMemberDetails member, BattleSourceLoadout loadout)
    {
        if (member.Deployment!.Definition.Equipment is null)
            return [];
        // Keep slot positions while excluding identities without field bindings.
        var selectable = new BattleSourceLoadout(loadout.Items.Select(word =>
            definition.FieldPreparation!.Items.ContainsKey((byte)(word & 127)) &&
            ((word & BattleInventory.Broken) == 0 || BattleEquipment.CanBeDamaged(member.Deployment.Definition, (byte)(word & 127)))
                ? word : (ushort)127), loadout.Spells);
        return BattleEquipment.Choices(member.Deployment.Definition, BaseAttack(member), selectable, member.Carried.Progress, member.Carried.Status);
    }

    internal static byte BaseAttack(PartyMemberDetails member) => member.Carried.Progress?.BaseAttack ??
        member.Deployment!.Definition.Equipment!.BaseAttack;

    internal static FieldActionChoices Query(ScenarioDefinition definition, SessionSnapshot current, ActorRef actor,
        FieldItemCaller caller = FieldItemCaller.Field)
    {
        try
        {
            var member = Member(definition, current, actor, caller);
            var items = Items(definition, member, current.Exploration!.Map);
            var equipment = EquipmentChoices(definition, member, Loadout(member));
            var learned = member.Carried.Progress?.Spells ?? member.Deployment!.Definition.Spells;
            var preparation = definition.FieldPreparation!;
            var spells = learned.Where(preparation.Spells.ContainsKey)
                .Select(spell => preparation.Spell(spell, current.Exploration.Map)!)
                .Select(spell => spell with { Description = FieldRecovery.Describe(definition, current, member, spell) })
                .ToArray();
            return new(current.SessionId, current.Revision, actor, equipment, items, spells);
        }
        catch (BattleRuleException error) { return new(current.SessionId, current.Revision, actor, [], [], [], error.Code); }
    }

    internal static BattleActorStartInput WithLoadout(PartyMemberDetails member, BattleSourceLoadout loadout, bool refresh = false)
    {
        var actor = WithLoadout(member.Carried, loadout);
        return refresh ? actor.RefreshStats(member.Deployment?.Definition) : actor;
    }

    internal static BattleActorStartInput WithLoadout(BattleActorStartInput actor, BattleSourceLoadout loadout) =>
        actor with { SourceLoadout = loadout, Progress = actor.Progress is { } progress
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
        var loadout = Pack(definition, member);
        if (slot is < 0 or >= 4 || (loadout.Items[slot] & 127) == 127)
            throw new BattleRuleException("field-item-slot", "item.slot");
        ushort word = loadout.Items[slot];
        if (word != expectedWord)
            throw new BattleRuleException("field-stale-item", "item.word");
        ValidateEquipped(member, loadout);
        byte id = (byte)(word & 127);
        if (!definition.FieldPreparation!.Items.TryGetValue(id, out var item))
            throw new BattleRuleException("field-item-unsupported", "item.slot", true);
        if ((word & BattleInventory.Broken) != 0 && member.Deployment!.Definition.Equipment?.ClassMask(id) is not null &&
            !BattleEquipment.CanBeDamaged(member.Deployment.Definition, id))
            throw new BattleRuleException("equipment-broken", "item.slot", true);
        return new(slot, word, item.Name, !item.Unsellable, definition.FieldPreparation.Effect(id, null));
    }

    internal static BattleStartInput Prepare(ScenarioDefinition definition, SessionSnapshot current, FieldCommand command,
        FieldItemCaller caller = FieldItemCaller.Field)
    {
        var member = Member(definition, current, command.Member, caller);
        var party = current.Exploration!.Party;
        var loadout = Loadout(member);
        // Known source refusals precede unsupported loadout checks for the selected words.
        if (command is FieldGive outgoing)
        {
            RequireNotCursed(definition, member, outgoing.Slot, outgoing.ExpectedWord, caller);
            if (outgoing.ReturnSlot is { } returned && outgoing.ExpectedReturnWord is { } word)
                RequireNotCursed(definition, Member(definition, current, outgoing.Target, caller), returned, word, caller);
        }
        else if (command is FieldDrop dropItem)
            RequireNotCursed(definition, member, dropItem.Slot, dropItem.ExpectedWord, caller);
        else if (command is FieldEquip && caller == FieldItemCaller.Caravan &&
            loadout.Items.Any(word => (word & 128) != 0 && CaravanDepotQuery.Cursed(definition, word) &&
                (member.Deployment is not { } deployment || !BattleEquipment.WornCurse(deployment.Definition, word))))
            throw new BattleRuleException("caravan-equipped-curse", "item.word");
        switch (command)
        {
            case FieldEquip equip:
                _ = Items(definition, member);
                if (member.Deployment!.Definition.Equipment is null)
                    throw new BattleRuleException("equipment-unavailable", "actor.equipment", true);
                var choice = EquipmentChoices(definition, member, loadout)
                    .SingleOrDefault(row => row.Slot == equip.Slot) ?? throw new BattleRuleException("equipment-slot", "equipment.slot");
                if (choice.ExpectedWord != equip.ExpectedWord ||
                    (equip.Slot == BattleEquipment.RemoveRingChoice || equip.ExpectedSourceSlot.HasValue) && choice.SourceSlot != equip.ExpectedSourceSlot)
                    throw new BattleRuleException("field-stale-item", "item.word");
                return Replace(party, [WithLoadout(member, BattleEquipment.Equip(member.Deployment.Definition, loadout, choice.Slot), refresh: true)]);
            case FieldGive give:
                var given = Slot(definition, member, give.Slot, give.ExpectedWord);
                var target = Member(definition, current, give.Target, caller);
                var targetLoadout = Pack(definition, target);
                ValidateEquipped(target, targetLoadout);
                if (caller == FieldItemCaller.Caravan && give.Target == command.Member)
                {
                    if (give.ReturnSlot.HasValue || give.ExpectedReturnWord.HasValue)
                        throw new BattleRuleException("caravan-self-exchange", "target.items");
                    return Replace(party, [WithLoadout(member,
                        BattleInventory.Add(BattleInventory.Remove(loadout, given.Slot), given.Word), refresh: true)]);
                }
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
                            return Replace(party, [WithLoadout(member, new(words, own.Spells), refresh: true)]);
                        }
                        own = BattleInventory.Add(BattleInventory.Remove(own, given.Slot), returned.Word);
                        own = BattleInventory.Remove(own, ownReturn > given.Slot ? ownReturn - 1 : ownReturn);
                    }
                    else own = BattleInventory.Remove(own, given.Slot);
                    return Replace(party, [WithLoadout(member, BattleInventory.Add(own, given.Word), refresh: true)]);
                }
                var donor = BattleInventory.Remove(loadout, given.Slot);
                if (give.ReturnSlot is { } returnSlot)
                {
                    var returned = Slot(definition, target, returnSlot, give.ExpectedReturnWord!.Value);
                    targetLoadout = BattleInventory.Remove(targetLoadout, returnSlot);
                    donor = BattleInventory.Add(donor, returned.Word);
                }
                targetLoadout = BattleInventory.Add(targetLoadout, given.Word);
                var recipient = WithLoadout(target, targetLoadout, refresh: give.ReturnSlot.HasValue);
                return Replace(party, [WithLoadout(member, donor, refresh: true), recipient]);
            case FieldDrop drop:
                var removed = Slot(definition, member, drop.Slot, drop.ExpectedWord);
                var item = definition.FieldPreparation!.Items[(byte)(removed.Word & 127)];
                if (!removed.CanDrop) throw new BattleRuleException("discard-unsellable", "item");
                if (item.Rare && party.ItemCustody is null) throw new BattleRuleException("discard-custody-unavailable", "party.itemCustody", true);
                return Replace(party, [WithLoadout(member, BattleInventory.Remove(loadout, drop.Slot), refresh: true)],
                    item.Rare ? party.ItemCustody!.AddDeal(item.ItemId) : party.ItemCustody);
            default: throw new BattleRuleException("field-command-unsupported", "command", true);
        }
    }

    private static void RequireNotCursed(ScenarioDefinition definition, PartyMemberDetails member, int slot, ushort word, FieldItemCaller caller)
    {
        var loadout = Loadout(member);
        if (slot is < 0 or >= 4 || loadout.Items.Count != 4 || loadout.Items[slot] != word || (word & 127) == 127)
        {
            if (caller == FieldItemCaller.Caravan) throw new BattleRuleException("field-stale-item", "item.word");
            return;
        }
        if (caller == FieldItemCaller.Caravan && (word & 128) != 0 && CaravanDepotQuery.Cursed(definition, word))
            throw new BattleRuleException("caravan-equipped-curse", "item.word");
        if (member.Deployment is { } deployment) BattleEquipment.RequireTransferable(deployment.Definition, word);
    }
}

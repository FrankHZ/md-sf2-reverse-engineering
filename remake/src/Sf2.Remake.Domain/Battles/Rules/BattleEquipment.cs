using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Domain.Battles;

public sealed record BattleWeaponDefinition(byte ItemId, string Name, uint ClassMask, byte AttackBonus,
    byte MinimumRange, byte MaximumRange);

public sealed class BattleEquipmentDefinition
{
    internal BattleEquipmentDefinition(byte baseAttack, IEnumerable<BattleWeaponDefinition> weapons)
    { BaseAttack = baseAttack; Weapons = new ReadOnlyDictionary<byte, BattleWeaponDefinition>(weapons.ToDictionary(w => w.ItemId)); }
    public byte BaseAttack { get; }
    public IReadOnlyDictionary<byte, BattleWeaponDefinition> Weapons { get; }
}

public sealed record BattleEquipmentChoice(int Slot, string Label, bool Equipped, byte Attack,
    byte MinimumRange, byte MaximumRange);

// Slot4 is the source menu's Unarmed sentinel, never an inventory slot.
public static class BattleEquipment
{
    // RemoveItemBySlot shifts following words and refreshes from the live base.
    internal static BattleActorState RemoveItem(BattleActorState actor, int slot)
    {
        var words = actor.SourceLoadout!.Items.ToList(); words.RemoveAt(slot); words.Add(127);
        var loadout = new BattleSourceLoadout(words, actor.SourceLoadout.Spells);
        return actor.With(sourceLoadout: loadout, attack: actor.Definition.Equipment is null ? actor.Attack :
            (byte)Math.Min(200, actor.BaseAttack + Contribution(actor.Definition, loadout)));
    }

    public static int EquippedSlot(BattleActorDefinition definition, BattleSourceLoadout? loadout) =>
        loadout is null || definition.Equipment is null ? 4 : Enumerable.Range(0, loadout.Items.Count)
            .FirstOrDefault(slot => (loadout.Items[slot] & 128) != 0 &&
                definition.Equipment.Weapons.ContainsKey((byte)(loadout.Items[slot] & 127)), 4);

    internal static int Contribution(BattleActorDefinition definition, BattleSourceLoadout? loadout)
    {
        if (definition.Equipment is null) return definition.Growth?.AttackBonus ?? 0;
        int slot = EquippedSlot(definition, loadout);
        return slot == 4 ? 0 : definition.Equipment.Weapons[(byte)(loadout!.Items[slot] & 127)].AttackBonus;
    }

    public static PhysicalActorDefinition? Physical(BattleActorState actor)
    {
        if (actor.Definition.Equipment is null || actor.Definition.Physical is not { } physical) return actor.Definition.Physical;
        int slot = EquippedSlot(actor.Definition, actor.SourceLoadout);
        var weapon = slot == 4 ? null : actor.Definition.Equipment.Weapons[(byte)(actor.SourceLoadout!.Items[slot] & 127)];
        return physical with { MinimumRange = weapon?.MinimumRange ?? 1, MaximumRange = weapon?.MaximumRange ?? 1 };
    }

    internal static void ValidateLoadout(BattleActorDefinition definition, BattleSourceLoadout? loadout)
    {
        var equipment = definition.Equipment ?? throw new BattleRuleException("equipment-unavailable", "actor.equipment", true);
        if (loadout is null || loadout.Items.Count != 4 || Sf2ClassRules.SourceClass(definition.ClassRule) is not { } sourceClass)
            throw new BattleRuleException("equipment-loadout", "actor.sourceLoadout");
        int equipped = 0;
        foreach (ushort word in loadout.Items)
        {
            if (word > 511) throw new BattleRuleException("equipment-item-word", "actor.sourceLoadout");
            bool selected = (word & 128) != 0;
            if (equipment.Weapons.TryGetValue((byte)(word & 127), out var weapon))
            {
                if ((word & 256) != 0) throw new BattleRuleException("equipment-broken", "actor.sourceLoadout", true);
                if (selected && (weapon.ClassMask & (1u << sourceClass)) == 0)
                    throw new BattleRuleException("equipment-class", "actor.sourceLoadout");
                if (selected) equipped++;
            }
            else if (selected) throw new BattleRuleException("equipment-nonweapon", "actor.sourceLoadout", true);
        }
        if (equipped > 1) throw new BattleRuleException("equipment-multiple", "actor.sourceLoadout");
    }

    internal static void RequireActor(BattleActorState actor)
    {
        ValidateLoadout(actor.Definition, actor.SourceLoadout);
        if (!actor.IsAlly || actor.Control != BattleControl.Player || actor.Hp == 0 || actor.Position is null ||
            actor.Status is not (0 or 2) || actor.Definition.Physical is null)
            throw new BattleRuleException("equipment-actor", "actor", true);
        if (actor.Attack != Math.Min(200, actor.BaseAttack + Contribution(actor.Definition, actor.SourceLoadout)))
            throw new BattleRuleException("equipment-stats", "actor.attack");
    }

    public static IReadOnlyList<BattleEquipmentChoice> Choices(BattleActorState actor)
    {
        RequireActor(actor);
        var equipment = actor.Definition.Equipment!;
        byte sourceClass = Sf2ClassRules.SourceClass(actor.Definition.ClassRule)!.Value;
        int equipped = EquippedSlot(actor.Definition, actor.SourceLoadout);
        List<BattleEquipmentChoice> choices = [];
        for (int slot = 0; slot < 4; slot++)
            if (equipment.Weapons.TryGetValue((byte)(actor.SourceLoadout!.Items[slot] & 127), out var weapon) &&
                (weapon.ClassMask & (1u << sourceClass)) != 0)
                choices.Add(new(slot, weapon.Name, equipped == slot,
                    (byte)Math.Min(200, actor.BaseAttack + weapon.AttackBonus), weapon.MinimumRange, weapon.MaximumRange));
        if (choices.Count is > 0 and < 4) choices.Add(new(4, "Unarmed", equipped == 4, actor.BaseAttack, 1, 1));
        return choices.AsReadOnly();
    }

    internal static BattleActorState Equip(BattleActorState actor, int slot)
    {
        var choice = Choices(actor).SingleOrDefault(choice => choice.Slot == slot)
            ?? throw new BattleRuleException("equipment-slot", "equipment.slot");
        var words = actor.SourceLoadout!.Items.Select((word, index) =>
            actor.Definition.Equipment!.Weapons.ContainsKey((byte)(word & 127))
                ? (ushort)((word & ~128) | (index == slot ? 128 : 0)) : word).ToArray();
        return actor.With(sourceLoadout: new(words, actor.SourceLoadout.Spells), attack: choice.Attack);
    }
}

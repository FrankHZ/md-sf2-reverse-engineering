using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Domain.Battles;

public sealed record BattleWeaponDefinition(byte ItemId, string Name, uint ClassMask, byte AttackBonus,
    byte MinimumRange, byte MaximumRange);

public sealed class BattleEquipmentDefinition
{
    internal BattleEquipmentDefinition(byte baseAttack, IEnumerable<BattleWeaponDefinition> weapons, bool allowChanges = true)
    { BaseAttack = baseAttack; AllowChanges = allowChanges; Weapons = new ReadOnlyDictionary<byte, BattleWeaponDefinition>(weapons.ToDictionary(w => w.ItemId)); }
    public byte BaseAttack { get; }
    public bool AllowChanges { get; }
    public IReadOnlyDictionary<byte, BattleWeaponDefinition> Weapons { get; }
}

public sealed record BattleEquipmentChoice(int Slot, string Label, bool Equipped, byte Attack,
    byte MinimumRange, byte MaximumRange);

// Slot4 is the source menu's Unarmed sentinel, never an inventory slot.
public static class BattleEquipment
{
    // Shared by battle construction and carried-party reads; explicit battle overrides stay with the caller.
    public static byte Attack(BattleActorDefinition definition, BattleActorProgress? progress, BattleSourceLoadout? loadout) =>
        progress is null && definition.Equipment is null ? definition.Attack :
        (byte)Math.Min(200, (progress?.BaseAttack ?? definition.Equipment?.BaseAttack ??
            (byte)(definition.Attack - (definition.Growth?.AttackBonus ?? 0))) + Contribution(definition, loadout));

    // RemoveItemBySlot shifts following words and refreshes from the live base.
    internal static BattleActorState RemoveItem(BattleActorState actor, int slot)
    {
        var loadout = BattleInventory.Remove(actor.SourceLoadout!, slot);
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
        if (loadout is null || loadout.Items.Count != 4 || equipment.AllowChanges && Sf2ClassRules.SourceClass(definition.ClassRule) is null)
            throw new BattleRuleException("equipment-loadout", "actor.sourceLoadout");
        int equipped = 0;
        foreach (ushort word in loadout.Items)
        {
            if (word > 511) throw new BattleRuleException("equipment-item-word", "actor.sourceLoadout");
            bool selected = (word & 128) != 0;
            if (equipment.Weapons.TryGetValue((byte)(word & 127), out var weapon))
            {
                if ((word & 256) != 0) throw new BattleRuleException("equipment-broken", "actor.sourceLoadout", true);
                if (selected && Sf2ClassRules.SourceClass(definition.ClassRule) is { } sourceClass && (weapon.ClassMask & (1u << sourceClass)) == 0)
                    throw new BattleRuleException("equipment-class", "actor.sourceLoadout");
                if (selected) equipped++;
            }
            else if (selected) throw new BattleRuleException("equipment-nonweapon", "actor.sourceLoadout", true);
        }
        if (equipped > 1) throw new BattleRuleException("equipment-multiple", "actor.sourceLoadout");
    }

    internal static bool OrdinaryAiLoadout(BattleActorDefinition definition, BattleSourceLoadout? loadout, bool allowHerb,
        bool allowEarlySpells = false)
    {
        if (loadout is null) return false;
        if (!allowEarlySpells && (loadout.Spells.Any(spell => spell != 63) || definition.Spells.Count != 0)) return false;
        if (allowEarlySpells && (definition.Spells.Any(spell => spell is not { Value: "heal" or "blaze" or "dispel", Level: 1 }) ||
            loadout.Spells.Any(spell => spell is not (0 or 6 or 11 or 63)))) return false;
        // Authored semantic books use an empty packed book. A nonempty source book
        // must agree with its decoded definitions; orphan packed spells cannot disappear.
        if (allowEarlySpells && loadout.Spells.Any(spell => spell != 63) &&
            !definition.Spells.Select(spell => spell.Value switch { "heal" => 0, "dispel" => 6, _ => 11 }).ToHashSet()
                .SetEquals(loadout.Spells.Where(spell => spell != 63).Select(spell => (int)spell))) return false;
        if (definition.Equipment is not null) ValidateLoadout(definition, loadout);
        return loadout.Items.All(word => word == 127 || allowHerb && word == 0 ||
            (word & ~255) == 0 && definition.Equipment?.Weapons.ContainsKey((byte)(word & 127)) == true);
    }

    internal static void RequireActor(BattleActorState actor)
    {
        if (actor.Definition.Equipment?.AllowChanges != true)
            throw new BattleRuleException("equipment-unavailable", "actor.equipment", true);
        ValidateLoadout(actor.Definition, actor.SourceLoadout);
        if (!actor.IsAlly || actor.Control != BattleControl.Player || actor.Hp == 0 || actor.Position is null ||
            !SilenceRules.Supported(actor.Status) || (actor.Status & 0xC0) != 0 || actor.Definition.Physical is null)
            throw new BattleRuleException("equipment-actor", "actor", true);
        if (actor.Attack != Math.Min(200, actor.BaseAttack + Contribution(actor.Definition, actor.SourceLoadout)))
            throw new BattleRuleException("equipment-stats", "actor.attack");
    }

    public static IReadOnlyList<BattleEquipmentChoice> Choices(BattleActorState actor)
    {
        RequireActor(actor);
        return Choices(actor.Definition, actor.BaseAttack, actor.SourceLoadout!);
    }

    internal static IReadOnlyList<BattleEquipmentChoice> Choices(BattleActorDefinition definition, byte baseAttack, BattleSourceLoadout loadout)
    {
        ValidateLoadout(definition, loadout);
        var equipment = definition.Equipment!;
        byte sourceClass = Sf2ClassRules.SourceClass(definition.ClassRule)!.Value;
        int equipped = EquippedSlot(definition, loadout);
        List<BattleEquipmentChoice> choices = [];
        for (int slot = 0; slot < 4; slot++)
            if (equipment.Weapons.TryGetValue((byte)(loadout.Items[slot] & 127), out var weapon) &&
                (weapon.ClassMask & (1u << sourceClass)) != 0)
                choices.Add(new(slot, weapon.Name, equipped == slot,
                    (byte)Math.Min(200, baseAttack + weapon.AttackBonus), weapon.MinimumRange, weapon.MaximumRange));
        if (choices.Count is > 0 and < 4) choices.Add(new(4, "Unarmed", equipped == 4, baseAttack, 1, 1));
        return choices.AsReadOnly();
    }

    internal static BattleActorState Equip(BattleActorState actor, int slot)
    {
        var choice = Choices(actor).SingleOrDefault(choice => choice.Slot == slot)
            ?? throw new BattleRuleException("equipment-slot", "equipment.slot");
        return actor.With(sourceLoadout: Equip(actor.Definition, actor.SourceLoadout!, slot), attack: choice.Attack);
    }

    internal static BattleSourceLoadout Equip(BattleActorDefinition definition, BattleSourceLoadout loadout, int slot)
    {
        var words = loadout.Items.Select((word, index) =>
            definition.Equipment!.Weapons.ContainsKey((byte)(word & 127))
                ? (ushort)((word & ~128) | (index == slot ? 128 : 0)) : word).ToArray();
        return new(words, loadout.Spells);
    }
}

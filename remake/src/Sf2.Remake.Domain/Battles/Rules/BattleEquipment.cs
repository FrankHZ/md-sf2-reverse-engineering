using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Domain.Battles;

public sealed record BattleWeaponDefinition(byte ItemId, string Name, uint ClassMask, byte AttackBonus,
    byte MinimumRange, byte MaximumRange, byte CriticalIncrease = 0);

public enum BattleEquipmentCategory { Weapon, Ring }
internal enum RingStat { Attack, Defense, Agility, Move }
internal sealed record BattleRingDefinition(byte ItemId, string Name, uint ClassMask, RingStat? Stat, byte Bonus,
    bool DamageOnUse = false, int? MovementSound = null, string? DepotDescriptionKey = null);

public sealed class BattleEquipmentDefinition
{
    internal BattleEquipmentDefinition(byte baseAttack, IEnumerable<BattleWeaponDefinition> weapons, bool allowChanges = true,
        IEnumerable<byte>? inertItems = null, IEnumerable<BattleRingDefinition>? rings = null)
    {
        BaseAttack = baseAttack;
        AllowChanges = allowChanges;
        Weapons = new ReadOnlyDictionary<byte, BattleWeaponDefinition>(weapons.ToDictionary(w => w.ItemId));
        Rings = new ReadOnlyDictionary<byte, BattleRingDefinition>((rings ?? []).ToDictionary(ring => ring.ItemId));
        InertItems = Array.AsReadOnly((inertItems ?? []).Distinct().ToArray());
    }
    public byte BaseAttack { get; }
    public bool AllowChanges { get; }
    public IReadOnlyDictionary<byte, BattleWeaponDefinition> Weapons { get; }
    internal IReadOnlyDictionary<byte, BattleRingDefinition> Rings { get; }
    // Bare carried identities only, without active effects.
    public IReadOnlyList<byte> InertItems { get; }

    internal uint? ClassMask(byte item) =>
        Weapons.TryGetValue(item, out var weapon) ? weapon.ClassMask : Rings.GetValueOrDefault(item)?.ClassMask;
}

public sealed record BattleEquipmentChoice(int Slot, string Label, bool Equipped, byte Attack,
    byte MinimumRange, byte MaximumRange)
{
    public BattleEquipmentCategory Category { get; init; }
    public int? SourceSlot { get; init; }
    public ushort? ExpectedWord { get; init; }
    public byte Defense { get; init; }
    public byte Agility { get; init; }
    public byte Move { get; init; }
}

public static class BattleEquipment
{
    // Menu choices outside the carried slot indices.
    public const int UnarmedChoice = 4;
    public const int RemoveRingChoice = 5;

    // Shared by battle construction and carried-party reads; explicit battle overrides stay with the caller.
    public static byte Attack(BattleActorDefinition definition, BattleActorProgress? progress,
        BattleSourceLoadout? loadout, ushort applied = 0)
    {
        if (progress is null && definition.Equipment is null && (applied & BattleStatCounters.Attack) == 0)
            return definition.Attack;
        byte basis = progress?.BaseAttack ?? definition.Equipment?.BaseAttack ??
            (byte)(definition.Attack - (definition.Growth?.AttackBonus ?? 0));
        return EffectiveAttack(basis, applied, definition, loadout);
    }

    private static byte EffectiveAttack(byte basis, ushort applied, BattleActorDefinition definition, BattleSourceLoadout? loadout) =>
        (byte)Math.Min(200, basis + BattleStatCounters.AttackBonus(basis, applied) + Contribution(definition, loadout));

    internal static bool CanBeDamaged(BattleActorDefinition definition, byte item) =>
        definition.Equipment?.Rings.GetValueOrDefault(item)?.DamageOnUse == true;

    internal static byte Defense(byte basis, ushort applied, BattleActorDefinition definition, BattleSourceLoadout? loadout, bool initial = false)
    {
        int bonus = RingBonus(definition, loadout, RingStat.Defense);
        if (initial && bonus == 0) return basis;
        return (byte)Math.Min(200, BattleStatCounters.Defense(basis, applied) + bonus);
    }

    internal static byte PackedAgility(byte basis, ushort applied, BattleActorDefinition definition, BattleSourceLoadout? loadout, bool initial = false)
    {
        int bonus = RingBonus(definition, loadout, RingStat.Agility);
        if (initial && bonus == 0) return basis;
        byte effective = BattleStatCounters.PackedAgility(basis, applied);
        return (byte)((effective & 128) | Math.Min(127, (effective & 127) + bonus));
    }

    internal static byte Move(byte basis, ushort applied, BattleActorDefinition definition, BattleSourceLoadout? loadout)
    {
        int effective = Math.Max(0, basis - (applied & 1));
        int bonus = RingBonus(definition, loadout, RingStat.Move);
        return (byte)(bonus == 0 ? effective : Math.Min(200, effective + bonus));
    }

    // RemoveItemBySlot shifts following words and refreshes from the live base.
    internal static BattleActorState RemoveItem(BattleActorState actor, int slot, byte? initializedBaseAttack = null)
    {
        var loadout = BattleInventory.Remove(actor.SourceLoadout!, slot);
        return actor.With(sourceLoadout: loadout, refreshStats: true, attack: actor.Definition.Equipment is null ? actor.Attack :
            (byte)Math.Min(200, (initializedBaseAttack ?? actor.BaseAttack) + Contribution(actor.Definition, loadout)));
    }

    public static int EquippedSlot(BattleActorDefinition definition, BattleSourceLoadout? loadout) =>
        definition.Equipment is { } equipment
            ? WornSlot(loadout, equipment.Weapons) ?? UnarmedChoice
            : UnarmedChoice;

    public static int? EquippedRingSlot(BattleActorDefinition definition, BattleSourceLoadout? loadout) =>
        definition.Equipment is { } equipment ? WornSlot(loadout, equipment.Rings) : null;

    private static int? WornSlot<T>(BattleSourceLoadout? loadout, IReadOnlyDictionary<byte, T> category)
    {
        if (loadout is null || category.Count == 0) return null;
        for (int slot = 0; slot < loadout.Items.Count; slot++)
        {
            ushort word = loadout.Items[slot];
            if ((word & 128) != 0 && category.ContainsKey((byte)(word & 127))) return slot;
        }
        return null;
    }

    public static byte? EquippedWeaponItem(BattleActorState actor)
    {
        // Older controlled scene fixtures supply a single worn weapon without an equipment catalog.
        if (actor.Definition.Equipment is null)
        {
            ushort? word = actor.SourceLoadout?.Items.FirstOrDefault(item => (item & 128) != 0 && (item & 127) != 127);
            return word is > 0 ? (byte)(word.Value & 127) : null;
        }
        return Weapon(actor.Definition, actor.SourceLoadout)?.ItemId;
    }

    private static BattleWeaponDefinition? Weapon(BattleActorDefinition definition, BattleSourceLoadout? loadout)
    {
        int slot = EquippedSlot(definition, loadout);
        return slot == UnarmedChoice ? null : definition.Equipment!.Weapons[(byte)(loadout!.Items[slot] & 127)];
    }

    private static BattleRingDefinition? Ring(BattleActorDefinition definition, BattleSourceLoadout? loadout) =>
        EquippedRingSlot(definition, loadout) is { } slot
            ? definition.Equipment!.Rings[(byte)(loadout!.Items[slot] & 127)] : null;

    internal static int? MovementSound(BattleActorDefinition definition, BattleSourceLoadout? loadout) =>
        Ring(definition, loadout)?.MovementSound;

    private static int RingBonus(BattleActorDefinition definition, BattleSourceLoadout? loadout, RingStat stat) =>
        Ring(definition, loadout) is { } ring && ring.Stat == stat ? ring.Bonus : 0;

    internal static int Contribution(BattleActorDefinition definition, BattleSourceLoadout? loadout) =>
        definition.Equipment is null ? definition.Growth?.AttackBonus ?? 0 :
        (Weapon(definition, loadout)?.AttackBonus ?? 0) + RingBonus(definition, loadout, RingStat.Attack);

    public static PhysicalActorDefinition? Physical(BattleActorState actor)
    {
        if (actor.Definition.Equipment is null || actor.Definition.Physical is not { } physical) return actor.Definition.Physical;
        var weapon = Weapon(actor.Definition, actor.SourceLoadout);
        return physical with { MinimumRange = weapon?.MinimumRange ?? 1, MaximumRange = weapon?.MaximumRange ?? 1,
            Critical = physical.Critical.IncreaseCritical(weapon?.CriticalIncrease ?? 0) };
    }

    internal static bool IsEligible(BattleActorDefinition definition, byte item) =>
        definition.Equipment?.ClassMask(item) is { } mask &&
        Sf2ClassRules.SourceClass(definition.ClassRule) is { } sourceClass && (mask & (1u << sourceClass)) != 0;

    internal static void ValidateLoadout(BattleActorDefinition definition, BattleSourceLoadout? loadout)
    {
        var equipment = definition.Equipment ?? throw new BattleRuleException("equipment-unavailable", "actor.equipment", true);
        byte? sourceClass = Sf2ClassRules.SourceClass(definition.ClassRule);
        if (loadout is null || loadout.Items.Count != 4 || equipment.AllowChanges && sourceClass is null)
            throw new BattleRuleException("equipment-loadout", "actor.sourceLoadout");
        int weapons = 0, rings = 0;
        foreach (ushort word in loadout.Items)
        {
            if (word > 511) throw new BattleRuleException("equipment-item-word", "actor.sourceLoadout");
            byte item = (byte)(word & 127);
            bool selected = (word & 128) != 0;
            if ((word & BattleInventory.Broken) != 0 && !CanBeDamaged(definition, item) &&
                (equipment.Weapons.ContainsKey(item) || selected && equipment.Rings.ContainsKey(item)))
                throw new BattleRuleException("equipment-broken", "actor.sourceLoadout", true);
            if (!selected) continue;
            if (equipment.ClassMask(item) is not { } mask)
                throw new BattleRuleException("equipment-nonweapon", "actor.sourceLoadout", true);
            if (sourceClass is { } current && (mask & (1u << current)) == 0)
                throw new BattleRuleException("equipment-class", "actor.sourceLoadout");
            if (equipment.Weapons.ContainsKey(item)) weapons++;
            else rings++;
        }
        if (weapons > 1 || rings > 1) throw new BattleRuleException("equipment-multiple", "actor.sourceLoadout");
    }

    internal static bool OrdinaryAiLoadout(BattleActorDefinition definition, BattleSourceLoadout? loadout, bool allowHerb,
        bool allowEarlySpells = false)
    {
        if (loadout is null) return false;
        if (!allowEarlySpells && (loadout.Spells.Any(spell => spell != 63) || definition.Spells.Count != 0)) return false;
        if (allowEarlySpells && definition.Spells.Any(spell => spell is not
            ({ Value: "heal", Level: >= 1 and <= 4 } or { Value: "blaze" or "blast", Level: >= 1 and <= 3 } or
             { Value: "aqua" or "freeze" or "boost", Level: 1 or 2 } or { Value: "aura" or "dispel" or "attack", Level: 1 }))) return false;
        // Authored semantic books use an empty packed book. A nonempty source book
        // must agree with its decoded definitions; orphan packed spells cannot disappear.
        if (allowEarlySpells && loadout.Spells.Any(spell => spell != 63) &&
            !definition.Spells.GroupBy(spell => spell.Value).Select(book =>
                ((book.Max(spell => spell.Level) - 1) << 6) | (book.Key switch
                {
                    "heal" => 0, "aura" => 1, "boost" => 3, "attack" => 5, "dispel" => 6,
                    "freeze" => 12, "blast" => 14, "aqua" => 40, _ => 11,
                })).ToHashSet()
                .SetEquals(loadout.Spells.Where(spell => spell != 63).Select(spell => (int)spell))) return false;
        if (definition.Equipment is not null) ValidateLoadout(definition, loadout);
        return loadout.Items.All(word => OrdinaryItem(definition, word, allowHerb));
    }

    internal static bool OrdinaryItem(BattleActorDefinition definition, ushort word, bool allowHerb) =>
        word == 127 || allowHerb && word == 0 ||
        word < 127 && definition.Equipment?.InertItems.Contains((byte)word) == true ||
        word <= 511 && ((word & BattleInventory.Broken) == 0 || CanBeDamaged(definition, (byte)(word & 127))) &&
        definition.Equipment?.ClassMask((byte)(word & 127)) is not null;

    internal static void RequireActor(BattleActorState actor)
    {
        if (actor.Definition.Equipment?.AllowChanges != true)
            throw new BattleRuleException("equipment-unavailable", "actor.equipment", true);
        ValidateLoadout(actor.Definition, actor.SourceLoadout);
        if (!actor.IsAlly || actor.Control != BattleControl.Player || actor.Hp == 0 || actor.Position is null ||
            !SilenceRules.Supported(actor.Status) || (actor.Status & 0xC1) != 0 || actor.Definition.Physical is null)
            throw new BattleRuleException("equipment-actor", "actor", true);
        if (actor.Attack != Attack(actor.Definition, actor.Progress, actor.SourceLoadout, actor.AppliedStatBits))
            throw new BattleRuleException("equipment-stats", "actor.attack");
    }

    public static IReadOnlyList<BattleEquipmentChoice> Choices(BattleActorState actor)
    {
        RequireActor(actor);
        return Choices(actor.Definition, actor.BaseAttack, actor.SourceLoadout!, actor.Progress, actor.Status);
    }

    internal static IReadOnlyList<BattleEquipmentChoice> Choices(BattleActorDefinition definition, byte baseAttack,
        BattleSourceLoadout loadout, BattleActorProgress? progress = null, ushort status = 0)
    {
        ValidateLoadout(definition, loadout);
        var equipment = definition.Equipment!;
        int? ringSlot = EquippedRingSlot(definition, loadout);
        List<BattleEquipmentChoice> choices = [];
        int weapons = 0;
        for (int slot = 0; slot < 4; slot++)
        {
            ushort word = loadout.Items[slot];
            byte id = (byte)(word & 127);
            if (!IsEligible(definition, id) || ((word & BattleInventory.Broken) != 0 && !CanBeDamaged(definition, id))) continue;
            if (equipment.Weapons.TryGetValue(id, out var weapon))
            {
                choices.Add(Choice(slot, slot, weapon.Name, Current(word), BattleEquipmentCategory.Weapon));
                weapons++;
            }
            else
                choices.Add(Choice(slot, slot, equipment.Rings[id].Name, Current(word), BattleEquipmentCategory.Ring));
        }
        if (weapons is > 0 and < 4)
            choices.Add(Choice(UnarmedChoice, null, "Unarmed", EquippedSlot(definition, loadout) == UnarmedChoice, BattleEquipmentCategory.Weapon));
        if (ringSlot is { } worn)
            choices.Add(Choice(RemoveRingChoice, worn, "Remove ring", false, BattleEquipmentCategory.Ring));
        return choices.AsReadOnly();

        static bool Current(ushort word) => (word & 128) != 0;
        BattleEquipmentChoice Choice(int choice, int? sourceSlot, string label, bool equipped, BattleEquipmentCategory category)
        {
            var candidate = Equip(definition, loadout, choice);
            var weapon = Weapon(definition, candidate);
            byte agility = (byte)((progress?.Agility ?? definition.Agility) | (definition.ExtraRoundAction ? 128 : 0));
            return new(choice, label, equipped, EffectiveAttack(baseAttack, status, definition, candidate),
                weapon?.MinimumRange ?? 1, weapon?.MaximumRange ?? 1)
            {
                Category = category, SourceSlot = sourceSlot,
                ExpectedWord = sourceSlot is { } slot ? loadout.Items[slot] : null,
                Defense = Defense(progress?.Defense ?? definition.Defense, status, definition, candidate),
                Agility = (byte)(PackedAgility(agility, status, definition, candidate) & 127),
                Move = Move(progress?.BaseMove ?? definition.Move, status, definition, candidate),
            };
        }
    }

    internal static BattleActorState Equip(BattleActorState actor, int slot)
    {
        var choice = Choices(actor).SingleOrDefault(choice => choice.Slot == slot)
            ?? throw new BattleRuleException("equipment-slot", "equipment.slot");
        return actor.With(sourceLoadout: Equip(actor.Definition, actor.SourceLoadout!, slot), attack: choice.Attack, refreshStats: true);
    }

    internal static BattleSourceLoadout Equip(BattleActorDefinition definition, BattleSourceLoadout loadout, int slot)
    {
        var equipment = definition.Equipment!;
        bool ring = slot == RemoveRingChoice || slot is >= 0 and < 4 && equipment.Rings.ContainsKey((byte)(loadout.Items[slot] & 127));
        var words = loadout.Items.Select((word, index) =>
            (ring ? equipment.Rings.ContainsKey((byte)(word & 127)) : equipment.Weapons.ContainsKey((byte)(word & 127)))
                ? (ushort)((word & ~128) | (index == slot ? 128 : 0)) : word).ToArray();
        return new(words, loadout.Spells);
    }
}

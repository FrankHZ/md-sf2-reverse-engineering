namespace Sf2.Remake.Domain.Battles;

public sealed record Battle01EntryStatValues(byte Attack, byte Defense, byte Agility, byte Move,
    ushort? Resistance, byte? Prowess);

// Raw unjoined slots may have zero maxima/movement. Battle01Stats keeps its combatant invariants.
public sealed class Battle01EntrySlot
{
    public Battle01EntrySlot(byte id, byte classId, byte level, ushort hpMax, ushort hpCurrent,
        byte mpMax, byte mpCurrent, ushort status, Battle01EntryStatValues currentStats,
        Battle01EntryStatValues? baseStats, IEnumerable<ushort> items, IEnumerable<byte> spells,
        byte? exp = null, ushort? kills = null, ushort? defeats = null)
        : this(id, classId, level, hpMax, hpCurrent, mpMax, mpCurrent, status, currentStats,
            baseStats, items, spells, exp, kills, defeats, null) { }

    private Battle01EntrySlot(byte id, byte classId, byte level, ushort hpMax, ushort hpCurrent,
        byte mpMax, byte mpCurrent, ushort status, Battle01EntryStatValues currentStats,
        Battle01EntryStatValues? baseStats, IEnumerable<ushort> items, IEnumerable<byte> spells,
        byte? exp, ushort? kills, ushort? defeats, Battle01Stats? battleStats)
    {
        ArgumentNullException.ThrowIfNull(currentStats);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(spells);
        var itemCopy = items.ToArray(); var spellCopy = spells.ToArray();
        if (id >= 30 || hpCurrent > hpMax || mpCurrent > mpMax ||
            itemCopy.Length != 4 || itemCopy.Any(item => item > 255) || spellCopy.Length != 4 ||
            exp > 200 || kills > 9999 || defeats > 9999)
            throw new ArgumentException("An entry slot must retain valid raw vitals, identity and inventory.");
        Id = id; ClassId = classId; Level = level; HpMax = hpMax; HpCurrent = hpCurrent;
        MpMax = mpMax; MpCurrent = mpCurrent; Status = status; CurrentStats = currentStats;
        BaseStats = baseStats; Items = Array.AsReadOnly(itemCopy); Spells = Array.AsReadOnly(spellCopy);
        Exp = exp; Kills = kills; Defeats = defeats; BattleStats = battleStats;
    }

    public byte Id { get; }
    public byte ClassId { get; }
    public byte Level { get; }
    public ushort HpMax { get; }
    public ushort HpCurrent { get; }
    public byte MpMax { get; }
    public byte MpCurrent { get; }
    public ushort Status { get; }
    public Battle01EntryStatValues CurrentStats { get; }
    // Null for the three effective-stat comparison profiles: no bases are inferred from them.
    public Battle01EntryStatValues? BaseStats { get; }
    public IReadOnlyList<ushort> Items { get; }
    public IReadOnlyList<byte> Spells { get; }
    public byte? Exp { get; }
    public ushort? Kills { get; }
    public ushort? Defeats { get; }
    public Battle01Stats? BattleStats { get; }

    internal static Battle01EntrySlot FromBattle(Battle01Combatant unit)
    {
        var s = unit.Stats;
        if (unit.Index is < 0 or > 2 || unit.ClassId is null)
            throw new ArgumentException("The three participating allies are required.", "entry.party");
        return new((byte)unit.Index, unit.ClassId.Value, s.Level, s.HpMax, s.HpCurrent,
            s.MpMax, s.MpCurrent, s.Status, new(s.Attack, s.Defense, s.Agility, s.Move, unit.Resistance, unit.Prowess),
            null, s.Items, s.Spells, s.CurrentExp, s.CurrentKills, s.CurrentDefeats, s);
    }

    public bool IsNeutralDormant => Id is >= 3 and < 30 && ClassId == 0 && Level == 0 &&
        HpMax == 0 && HpCurrent == 0 && MpMax == 0 && MpCurrent == 0 && Status == 0 &&
        BaseStats == new Battle01EntryStatValues(0, 0, 0, 0, 0, 0) && CurrentStats == BaseStats &&
        Items.All(item => item == 127) && Spells.All(spell => spell == 63) &&
        Exp is null && Kills is null && Defeats is null && BattleStats is null;

    internal Battle01EntrySlot Heal()
    {
        // This named comparison admits no status/equipment-derived stat change.
        ushort masked = (ushort)(Status & 7);
        if (masked != 0 || (BaseStats is not null && BaseStats != CurrentStats))
            throw new ArgumentException("General status or equipment refresh is not admitted.", "entry.refresh");
        if (HpCurrent == HpMax && MpCurrent == MpMax && Status == masked) return this;
        Battle01Stats? stats = BattleStats is not { } s ? null : new(s.Level, s.HpMax, s.HpMax,
            s.MpMax, s.MpMax, s.Attack, s.Defense, s.Agility, s.Move, masked, s.Items, s.Spells,
            s.CurrentExp, s.CurrentKills, s.CurrentDefeats);
        return new(Id, ClassId, Level, HpMax, HpMax, MpMax, MpMax, masked, CurrentStats,
            BaseStats, Items, Spells, Exp, Kills, Defeats, stats);
    }
}

public static class Battle01ExplorationEntry
{
    public const string RefreshPolicyId = "battle01-entry-unchanged-effective-stats-comparison";

    public static ushort TransformStepCounter(ushort word) =>
        (ushort)Math.Max(unchecked((short)word) - 20000, 0);

    // Also exercised with explicit synthetic damaged-MP/immortal vitals; production admission above stays closed.
    internal static (Battle01EntrySlot[] Slots, byte[] Processed) HealSlots(IReadOnlyList<Battle01EntrySlot> slots)
    {
        if (slots.Count != 30 || slots.Where((slot, index) => slot.Id != index).Any())
            throw new ArgumentException("Healing visits the complete ordered 30-slot image.", nameof(slots));
        var result = slots.ToArray(); var processed = new List<byte>();
        for (int i = 0; i < result.Length; i++)
        {
            if (result[i].HpCurrent == 0 && i is not (7 or 28)) continue;
            processed.Add((byte)i); result[i] = result[i].Heal();
        }
        return (result, processed.ToArray());
    }
}

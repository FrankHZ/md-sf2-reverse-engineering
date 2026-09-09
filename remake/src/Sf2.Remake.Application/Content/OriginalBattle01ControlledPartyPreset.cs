namespace Sf2.Remake.Application.Content;

// Controlled effective stats copied from a named comparison observation, not natural save/base stats.
public sealed class OriginalBattle01ControlledAlly
{
    public OriginalBattle01ControlledAlly(byte id, byte classId, byte level, ushort hpMax, ushort hpCurrent,
        byte mpMax, byte mpCurrent, byte effectiveAttack, byte effectiveDefense, byte effectiveAgility,
        byte effectiveMove, ushort statusEffects, IEnumerable<ushort> items, IEnumerable<byte> spells, byte? currentExp = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(spells);
        if (id > 2 || level == 0 || hpMax == 0 || hpCurrent > hpMax || mpCurrent > mpMax || effectiveMove == 0)
            throw new ArgumentException("Controlled ally identity, vitals or movement are invalid.");
        var itemCopy = items.ToArray();
        var spellCopy = spells.ToArray();
        if (itemCopy.Length != 4 || itemCopy.Any(item => item > 255))
            throw new ArgumentException("Four packed item words with only ID/equipped bits are required.", nameof(items));
        if (spellCopy.Length != 4)
            throw new ArgumentException("Four packed spell bytes are required.", nameof(spells));
        if (currentExp > 200) throw new ArgumentOutOfRangeException(nameof(currentExp));
        Id = id; ClassId = classId; Level = level; HpMax = hpMax; HpCurrent = hpCurrent;
        MpMax = mpMax; MpCurrent = mpCurrent; EffectiveAttack = effectiveAttack; EffectiveDefense = effectiveDefense;
        EffectiveAgility = effectiveAgility; EffectiveMove = effectiveMove; StatusEffects = statusEffects;
        Items = Array.AsReadOnly(itemCopy); Spells = Array.AsReadOnly(spellCopy);
        CurrentExp = currentExp;
    }

    public byte Id { get; }
    public byte ClassId { get; }
    public byte Level { get; }
    public ushort HpMax { get; }
    public ushort HpCurrent { get; }
    public byte MpMax { get; }
    public byte MpCurrent { get; }
    public byte EffectiveAttack { get; }
    public byte EffectiveDefense { get; }
    public byte EffectiveAgility { get; }
    public byte EffectiveMove { get; }
    public ushort StatusEffects { get; }
    public byte? CurrentExp { get; }
    public IReadOnlyList<ushort> Items { get; }
    public IReadOnlyList<byte> Spells { get; }

    internal bool Matches(OriginalBattle01ControlledAlly other) =>
        Id == other.Id && ClassId == other.ClassId && Level == other.Level && HpMax == other.HpMax &&
        HpCurrent == other.HpCurrent && MpMax == other.MpMax && MpCurrent == other.MpCurrent &&
        EffectiveAttack == other.EffectiveAttack && EffectiveDefense == other.EffectiveDefense &&
        EffectiveAgility == other.EffectiveAgility && EffectiveMove == other.EffectiveMove &&
        StatusEffects == other.StatusEffects && CurrentExp == other.CurrentExp &&
        Items.SequenceEqual(other.Items) && Spells.SequenceEqual(other.Spells);
}

public sealed class OriginalBattle01ControlledPartyPreset
{
    public const string ComparisonId = "private-local-battle01-player-ready-comparison-inputs-v1";
    public const string EvidenceOwner = "sf2-map3-battle01-player-ready-runtime-v1";
    public const uint ComparisonSeed = 0x1234;
    public const ushort ComparisonSeedCopy = 0x1234;
    public const string SeedCopyPolicyId = "battle01-controlled-player-ready-seed-copy-retention-v1";
    public const string PlayerAttackComparisonId = "private-local-battle01-player-attack-bowie-exp0-inputs-v1";

    public OriginalBattle01ControlledPartyPreset(string id, uint randomSeed, byte difficulty,
        IEnumerable<OriginalBattle01ControlledAlly> allies, ushort? randomSeedCopy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(allies);
        var copy = allies.ToArray();
        if (copy.Length != 3 || copy.Any(ally => ally is null) ||
            !copy.Select(ally => ally.Id).SequenceEqual(new byte[] { 0, 1, 2 }))
            throw new ArgumentException("The controlled party requires ordered ally slots 0, 1 and 2.", nameof(allies));
        Id = id; RandomSeed = randomSeed; Difficulty = difficulty; Allies = Array.AsReadOnly(copy);
        RandomSeedCopy = randomSeedCopy;
    }

    public string Id { get; }
    public uint RandomSeed { get; }
    public ushort? RandomSeedCopy { get; }
    public byte Difficulty { get; }
    public IReadOnlyList<OriginalBattle01ControlledAlly> Allies { get; }

    public static OriginalBattle01ControlledPartyPreset PlayerReadyComparison { get; } = new(
        ComparisonId, ComparisonSeed, 0,
        [
            new(0, 0, 1, 12, 12, 8, 8, 9, 4, 4, 6, 0, [199, 0, 127, 127], [10, 63, 63, 63]),
            new(1, 4, 1, 11, 11, 10, 10, 9, 5, 5, 5, 0, [213, 0, 0, 127], [0, 63, 63, 63]),
            new(2, 1, 1, 11, 11, 0, 0, 8, 5, 7, 7, 0, [184, 0, 127, 127], [63, 63, 63, 63]),
        ], ComparisonSeedCopy);

    // Explicit authored EXP supplement; the earlier observation does not establish current EXP.
    public static OriginalBattle01ControlledPartyPreset PlayerAttackComparison { get; } = new(
        PlayerAttackComparisonId, ComparisonSeed, 0,
        PlayerReadyComparison.Allies.Select(ally => ally.Id == 0
            ? new OriginalBattle01ControlledAlly(ally.Id, ally.ClassId, ally.Level, ally.HpMax, ally.HpCurrent,
                ally.MpMax, ally.MpCurrent, ally.EffectiveAttack, ally.EffectiveDefense, ally.EffectiveAgility,
                ally.EffectiveMove, ally.StatusEffects, ally.Items, ally.Spells, currentExp: 0)
            : ally), ComparisonSeedCopy);

    public OriginalBattle01StartupDiagnostic? GetAdmissionDiagnostic()
    {
        if (Id != ComparisonId && Id != PlayerAttackComparisonId)
            return new("party.id", "Only the named controlled comparison presets are admitted.");
        if (RandomSeed != ComparisonSeed) return new("party.randomSeed", "The comparison seed must be explicitly 0x1234.");
        if (RandomSeedCopy != ComparisonSeedCopy)
            return new("party.randomSeedCopy", "The independent comparison seed-copy must be explicitly 0x1234.");
        if (Difficulty != 0) return new("party.difficulty", "The comparison uses explicit difficulty zero.");
        var expected = Id == PlayerAttackComparisonId ? PlayerAttackComparison : PlayerReadyComparison;
        if (!Allies.Zip(expected.Allies).All(pair => pair.First.Matches(pair.Second)))
            return new("party.allies", "Controlled effective stats, EXP input, status, equipment or spells drifted.");
        return null;
    }
}

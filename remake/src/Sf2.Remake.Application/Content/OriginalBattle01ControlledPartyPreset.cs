namespace Sf2.Remake.Application.Content;

// Controlled effective stats copied from a named comparison observation, not natural save/base stats.
public sealed class OriginalBattle01ControlledAlly
{
    public OriginalBattle01ControlledAlly(byte id, byte classId, byte level, ushort hpMax, ushort hpCurrent,
        byte mpMax, byte mpCurrent, byte effectiveAttack, byte effectiveDefense, byte effectiveAgility,
        byte effectiveMove, ushort statusEffects, IEnumerable<ushort> items, IEnumerable<byte> spells,
        byte? currentExp = null, ushort? currentKills = null, ushort? currentDefeats = null)
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
        if (currentKills > 9999) throw new ArgumentOutOfRangeException(nameof(currentKills));
        if (currentDefeats > 9999) throw new ArgumentOutOfRangeException(nameof(currentDefeats));
        Id = id; ClassId = classId; Level = level; HpMax = hpMax; HpCurrent = hpCurrent;
        MpMax = mpMax; MpCurrent = mpCurrent; EffectiveAttack = effectiveAttack; EffectiveDefense = effectiveDefense;
        EffectiveAgility = effectiveAgility; EffectiveMove = effectiveMove; StatusEffects = statusEffects;
        Items = Array.AsReadOnly(itemCopy); Spells = Array.AsReadOnly(spellCopy);
        CurrentExp = currentExp;
        CurrentKills = currentKills;
        CurrentDefeats = currentDefeats;
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
    public ushort? CurrentKills { get; }
    public ushort? CurrentDefeats { get; }
    public IReadOnlyList<ushort> Items { get; }
    public IReadOnlyList<byte> Spells { get; }

    internal bool Matches(OriginalBattle01ControlledAlly other) =>
        Id == other.Id && ClassId == other.ClassId && Level == other.Level && HpMax == other.HpMax &&
        HpCurrent == other.HpCurrent && MpMax == other.MpMax && MpCurrent == other.MpCurrent &&
        EffectiveAttack == other.EffectiveAttack && EffectiveDefense == other.EffectiveDefense &&
        EffectiveAgility == other.EffectiveAgility && EffectiveMove == other.EffectiveMove &&
        StatusEffects == other.StatusEffects && CurrentExp == other.CurrentExp && CurrentKills == other.CurrentKills &&
        CurrentDefeats == other.CurrentDefeats &&
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
    public const string FirstDefeatComparisonId = "private-local-battle01-first-defeat-bowie-exp0-gold0-kills0-inputs-v1";
    public const string ChesterPlayerAttackComparisonId = "private-local-battle01-chester-player-attack-exp0-inputs-v1";
    public const string LeaderDefeatComparisonId = "private-local-battle01-leader-defeats0-inputs-v1";
    public const string ChesterDefeatComparisonId = "private-local-battle01-chester-defeats0-inputs-v1";
    public const string ChesterFirstKillComparisonId = "private-local-battle01-chester-kills0-inputs-v1";

    public OriginalBattle01ControlledPartyPreset(string id, uint randomSeed, byte difficulty,
        IEnumerable<OriginalBattle01ControlledAlly> allies, ushort? randomSeedCopy = null, uint? currentGold = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(allies);
        var copy = allies.ToArray();
        if (copy.Length != 3 || copy.Any(ally => ally is null) ||
            !copy.Select(ally => ally.Id).SequenceEqual(new byte[] { 0, 1, 2 }))
            throw new ArgumentException("The controlled party requires ordered ally slots 0, 1 and 2.", nameof(allies));
        Id = id; RandomSeed = randomSeed; Difficulty = difficulty; Allies = Array.AsReadOnly(copy);
        RandomSeedCopy = randomSeedCopy;
        if (currentGold > 9999999) throw new ArgumentOutOfRangeException(nameof(currentGold));
        CurrentGold = currentGold;
    }

    public string Id { get; }
    public uint RandomSeed { get; }
    public ushort? RandomSeedCopy { get; }
    public uint? CurrentGold { get; }
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

    // Authored accounting supplement; no claim about naturally carried gold or kills.
    public static OriginalBattle01ControlledPartyPreset FirstDefeatComparison { get; } = new(
        FirstDefeatComparisonId, ComparisonSeed, 0,
        PlayerAttackComparison.Allies.Select(ally => ally.Id == 0
            ? new OriginalBattle01ControlledAlly(ally.Id, ally.ClassId, ally.Level, ally.HpMax, ally.HpCurrent,
                ally.MpMax, ally.MpCurrent, ally.EffectiveAttack, ally.EffectiveDefense, ally.EffectiveAgility,
                ally.EffectiveMove, ally.StatusEffects, ally.Items, ally.Spells, ally.CurrentExp, currentKills: 0)
            : ally), ComparisonSeedCopy, currentGold: 0);

    // Authored Chester EXP only; his naturally carried EXP and kills remain unestablished.
    public static OriginalBattle01ControlledPartyPreset ChesterPlayerAttackComparison { get; } = new(
        ChesterPlayerAttackComparisonId, ComparisonSeed, 0,
        FirstDefeatComparison.Allies.Select(ally => ally.Id == 2
            ? new OriginalBattle01ControlledAlly(ally.Id, ally.ClassId, ally.Level, ally.HpMax, ally.HpCurrent,
                ally.MpMax, ally.MpCurrent, ally.EffectiveAttack, ally.EffectiveDefense, ally.EffectiveAgility,
                ally.EffectiveMove, ally.StatusEffects, ally.Items, ally.Spells, currentExp: 0,
                currentKills: ally.CurrentKills)
            : ally), ComparisonSeedCopy, FirstDefeatComparison.CurrentGold);

    // Authored defeats supplement only; no natural save or unspecified-kills claim.
    public static OriginalBattle01ControlledPartyPreset ChesterDefeatComparison { get; } = new(
        ChesterDefeatComparisonId, ComparisonSeed, 0,
        ChesterPlayerAttackComparison.Allies.Select(ally => ally.Id == 2
            ? new OriginalBattle01ControlledAlly(ally.Id, ally.ClassId, ally.Level, ally.HpMax, ally.HpCurrent,
                ally.MpMax, ally.MpCurrent, ally.EffectiveAttack, ally.EffectiveDefense, ally.EffectiveAgility,
                ally.EffectiveMove, ally.StatusEffects, ally.Items, ally.Spells, ally.CurrentExp,
                ally.CurrentKills, currentDefeats: 0)
            : ally), ComparisonSeedCopy, ChesterPlayerAttackComparison.CurrentGold);

    public static OriginalBattle01ControlledPartyPreset LeaderDefeatComparison { get; } = new(
        LeaderDefeatComparisonId, ComparisonSeed, 0,
        ChesterDefeatComparison.Allies.Select(ally => ally.Id == 0
            ? new OriginalBattle01ControlledAlly(ally.Id, ally.ClassId, ally.Level, ally.HpMax, ally.HpCurrent,
                ally.MpMax, ally.MpCurrent, ally.EffectiveAttack, ally.EffectiveDefense, ally.EffectiveAgility,
                ally.EffectiveMove, ally.StatusEffects, ally.Items, ally.Spells, ally.CurrentExp,
                ally.CurrentKills, currentDefeats: 0)
            : ally), ComparisonSeedCopy, ChesterDefeatComparison.CurrentGold);

    // Authored early kills supplement; the ordinary defeat/return comparison stays unchanged.
    public static OriginalBattle01ControlledPartyPreset ChesterFirstKillComparison { get; } = new(
        ChesterFirstKillComparisonId, ComparisonSeed, 0,
        LeaderDefeatComparison.Allies.Select(ally => ally.Id == 2
            ? new OriginalBattle01ControlledAlly(ally.Id, ally.ClassId, ally.Level, ally.HpMax, ally.HpCurrent,
                ally.MpMax, ally.MpCurrent, ally.EffectiveAttack, ally.EffectiveDefense, ally.EffectiveAgility,
                ally.EffectiveMove, ally.StatusEffects, ally.Items, ally.Spells, ally.CurrentExp,
                currentKills: 0, currentDefeats: ally.CurrentDefeats)
            : ally), ComparisonSeedCopy, LeaderDefeatComparison.CurrentGold);

    public OriginalBattle01StartupDiagnostic? GetAdmissionDiagnostic()
    {
        if (Id != ComparisonId && Id != PlayerAttackComparisonId && Id != FirstDefeatComparisonId &&
            Id != ChesterPlayerAttackComparisonId && Id != ChesterDefeatComparisonId && Id != LeaderDefeatComparisonId &&
            Id != ChesterFirstKillComparisonId)
            return new("party.id", "Only the named controlled comparison presets are admitted.");
        if (RandomSeed != ComparisonSeed) return new("party.randomSeed", "The comparison seed must be explicitly 0x1234.");
        if (RandomSeedCopy != ComparisonSeedCopy)
            return new("party.randomSeedCopy", "The independent comparison seed-copy must be explicitly 0x1234.");
        if (Difficulty != 0) return new("party.difficulty", "The comparison uses explicit difficulty zero.");
        var expected = Id == ChesterFirstKillComparisonId ? ChesterFirstKillComparison :
            Id == LeaderDefeatComparisonId ? LeaderDefeatComparison :
            Id == ChesterDefeatComparisonId ? ChesterDefeatComparison :
            Id == ChesterPlayerAttackComparisonId ? ChesterPlayerAttackComparison :
            Id == FirstDefeatComparisonId ? FirstDefeatComparison :
            Id == PlayerAttackComparisonId ? PlayerAttackComparison : PlayerReadyComparison;
        if (CurrentGold != expected.CurrentGold)
            return new("party.goldInput", "Retain the named preset's explicit or unspecified gold input.");
        if (!Allies.Zip(expected.Allies).All(pair => pair.First.Matches(pair.Second)))
            return new("party.allies", "Controlled effective stats, EXP/kills input, status, equipment or spells drifted.");
        return null;
    }
}

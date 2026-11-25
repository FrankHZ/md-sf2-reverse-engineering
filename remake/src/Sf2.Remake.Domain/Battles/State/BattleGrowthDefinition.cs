namespace Sf2.Remake.Domain.Battles;

public sealed record StatGrowthFraction(ushort Cumulative, ushort Increment);
public sealed record StatGrowth(byte Start, byte Projected, IReadOnlyList<StatGrowthFraction> Curve);
public sealed record LevelSpell(byte Level, byte PackedSpell);

// Validated source growth and supported equipment operands are immutable content. Current gains
// belong to the actor, and never modify an encounter definition or a second party roster.
public sealed record BattleGrowthDefinition(byte ClassId, byte AttackBonus,
    IReadOnlyList<StatGrowth> Stats, IReadOnlyList<LevelSpell> Spells,
    IReadOnlyDictionary<byte, SpellRef> SpellDefinitions)
{
    internal IReadOnlyList<LevelSpell>? RetainedSpells { get; init; }
    internal bool CanKnow(byte packed, byte level) =>
        Spells.Any(spell => spell.PackedSpell == packed && spell.Level <= level + (ClassId >= 11 ? 20 : 0)) ||
        RetainedSpells?.Any(spell => spell.PackedSpell == packed) == true;
}

public sealed record BattleActorProgress(byte Level, ushort MaxHp, byte MaxMp,
    byte BaseAttack, byte Defense, byte Agility, IReadOnlyList<SpellRef> Spells,
    BattleSourceLoadout? SourceLoadout, byte? ClassId = null)
{
    public byte? BaseMove { get; init; }
    public BattleCurrentStats? CurrentStats { get; init; }

    internal bool ValidGrowthItemStats => BaseMove is null or <= 200 &&
        CurrentStats?.Defense is null or <= 200 && CurrentStats?.Move is null or <= 200;
}

// A direct field stat write can differ from a fresh status/equipment calculation.
// Only the changed components that would otherwise be lost need an override.
public sealed record BattleCurrentStats(byte? Defense, byte? PackedAgility, byte? Move);

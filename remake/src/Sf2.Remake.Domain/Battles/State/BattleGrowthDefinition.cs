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
        Spells.Any(spell => spell.PackedSpell == packed && spell.Level <= level + (RetainedSpells is null ? 0 : 20)) ||
        RetainedSpells?.Any(spell => spell.PackedSpell == packed) == true;
}

public sealed record BattleActorProgress(byte Level, ushort MaxHp, byte MaxMp,
    byte BaseAttack, byte Defense, byte Agility, IReadOnlyList<SpellRef> Spells,
    BattleSourceLoadout? SourceLoadout, byte? ClassId = null);

using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Content.Scenarios;

public sealed record SourceClassDefinition(byte Id, string Code, string NameExpression, byte Movement,
    string ResistanceExpression, string MovementType, string ProwessExpression);
public sealed record SourceEquipEffect(string Type, int Parameter);
public sealed record SourceItemDefinition(byte Id, string Code, string DisplayName, string NameExpression,
    string EquipFlagsExpression, IReadOnlyList<string> EquipFlags, byte MinimumRange, byte MaximumRange,
    ushort Price, string ItemTypeExpression, string UseSpellExpression, string UseSpell, byte UseSpellLevel,
    IReadOnlyList<SourceEquipEffect> EquipEffects);
public sealed record SourceSpellDefinition(byte RecordId, byte BaseId, string Code, string DisplayName,
    string EntryExpression, byte Level, byte MpCost, string AnimationExpression, string PropertiesExpression,
    byte MinimumRange, byte MaximumRange, byte Radius, ushort Power)
{
    public SpellRef Spell => new(Code.ToLowerInvariant(), Level);
}
public sealed record SourceEnemyItem(string Expression, string Item, bool Equipped);
public sealed record SourceEnemySpell(string Expression, string Spell, byte Level);
public sealed record SourceEnemyDefinition(byte Id, string Code, string DisplayName, string NameExpression,
    byte NameEncodedLength, IReadOnlyList<byte> NameSuffixBytes, byte UnknownByte, string SpellPower,
    byte Level, ushort MaxHp, byte MaxMp, byte BaseAttack, byte BaseDefense, byte BaseAgility, byte BaseMovement,
    string ResistanceExpression, string ProwessExpression, IReadOnlyList<SourceEnemyItem> Items,
    IReadOnlyList<SourceEnemySpell> Spells, string InitialStatusExpression, string MovementType, string AiBitfieldExpression);

// Only the records selected by this encounter and declared party, with their existing export provenance.
public sealed class PrivateBattleDefinitions
{
    internal PrivateBattleDefinitions(BattleEncounterDefinition encounter, IEnumerable<EncounterSource> sources,
        IEnumerable<SourceClassDefinition> classes, IEnumerable<SourceItemDefinition> items,
        IEnumerable<SourceSpellDefinition> spells, IEnumerable<SourceEnemyDefinition> enemies)
    {
        Encounter = encounter; Sources = Array.AsReadOnly(sources.ToArray());
        Classes = new ReadOnlyDictionary<byte, SourceClassDefinition>(classes.ToDictionary(row => row.Id));
        Items = new ReadOnlyDictionary<byte, SourceItemDefinition>(items.ToDictionary(row => row.Id));
        Spells = new ReadOnlyDictionary<SpellRef, SourceSpellDefinition>(spells.ToDictionary(row => row.Spell));
        Enemies = new ReadOnlyDictionary<string, SourceEnemyDefinition>(enemies.ToDictionary(row => row.Code, StringComparer.Ordinal));
    }
    public BattleEncounterDefinition Encounter { get; }
    public IReadOnlyList<EncounterSource> Sources { get; }
    public IReadOnlyDictionary<byte, SourceClassDefinition> Classes { get; }
    public IReadOnlyDictionary<byte, SourceItemDefinition> Items { get; }
    public IReadOnlyDictionary<SpellRef, SourceSpellDefinition> Spells { get; }
    public IReadOnlyDictionary<string, SourceEnemyDefinition> Enemies { get; }
}

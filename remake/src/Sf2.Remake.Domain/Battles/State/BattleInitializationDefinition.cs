using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

public enum BattleMover { Regular, Healer, Centaur, Hovering, Stealth, Flying }
public enum BattleRegionProgram { None, Required }

// Declared external controlled inputs; these values are not inferred from original entry.
public sealed record NewBattleStartPolicy(string Declaration, string EvidenceOwner, string BridgeBoundary,
    byte Difficulty, bool SkipIntro, bool AlreadyRefreshedAllies, bool RosterOnly,
    bool AllyAutoBattle, bool OpponentControl, ushort? MissingCandidateAllyWord)
{
    // Produced only by the common map/battle sequencer after the before-battle route returns.
    public bool BeforeBattleRouted { get; internal init; }
}

public sealed class BattleSourceLoadout
{
    internal BattleSourceLoadout(IEnumerable<ushort> items, IEnumerable<byte> spells)
    { Items = Array.AsReadOnly(items.ToArray()); Spells = Array.AsReadOnly(spells.ToArray()); }
    public IReadOnlyList<ushort> Items { get; }
    public IReadOnlyList<byte> Spells { get; }
}

public sealed record BattleDeploymentInitialization(ushort? EnemyBaseAiWord, byte SpawnMode,
    byte PrimaryRegion, byte SecondaryRegion, byte Filler, byte AiCommandset = 0,
    byte? PrimaryOrder = null, byte? SecondaryOrder = null, int? AllyPartyMember = null);

public sealed class BattleActivationRegion
{
    internal BattleActivationRegion(byte id, IEnumerable<MapPosition> vertices)
    { Id = id; Vertices = Array.AsReadOnly(vertices.ToArray()); }
    public byte Id { get; }
    public IReadOnlyList<MapPosition> Vertices { get; }
}

public sealed class BattleInitializationDefinition
{
    internal BattleInitializationDefinition(IEnumerable<BattleActivationRegion> regions, BattleRegionProgram regionProgram,
        IEnumerable<MapPosition>? allyFormation = null, IEnumerable<MapPosition>? aiPoints = null,
        IReadOnlyDictionary<int, ActorRef>? sourceCombatants = null)
    { Regions = Array.AsReadOnly(regions.ToArray()); RegionProgram = regionProgram;
        AllyFormation = allyFormation is null ? null : Array.AsReadOnly(allyFormation.ToArray());
        AiPoints = Array.AsReadOnly((aiPoints ?? []).ToArray());
        SourceCombatants = new System.Collections.ObjectModel.ReadOnlyDictionary<int, ActorRef>(
            (sourceCombatants ?? new Dictionary<int, ActorRef>()).ToDictionary(row => row.Key, row => row.Value)); }
    public IReadOnlyList<BattleActivationRegion> Regions { get; }
    public BattleRegionProgram RegionProgram { get; }
    public IReadOnlyList<MapPosition>? AllyFormation { get; }
    public IReadOnlyList<MapPosition> AiPoints { get; }
    public IReadOnlyDictionary<int, ActorRef> SourceCombatants { get; }
}

public sealed class BattleRegionState
{
    internal BattleRegionState(IEnumerable<bool> flags, ushort tested)
    { Flags = Array.AsReadOnly(flags.ToArray()); Tested = tested; }
    public IReadOnlyList<bool> Flags { get; }
    public ushort Tested { get; }
}

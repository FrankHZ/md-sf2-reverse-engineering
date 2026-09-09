using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// Independent deterministic initialization values; source admission belongs to Application.
public sealed record Battle01Deployment(byte Ordinal, int CombatantIndex, byte Identity,
    MapPosition Position, byte AiCommandSet, ushort ItemWord, byte PrimaryOrder, byte PrimaryRegion,
    byte SecondaryOrder, byte SecondaryRegion, byte SourceFiller, byte Spawn);

public sealed class Battle01Region
{
    public Battle01Region(byte id, byte sourceUnknown, IEnumerable<MapPosition> vertices,
        byte trailingByte0, byte trailingByte1)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        var copy = vertices.ToArray();
        if (id > 2 || copy.Length != 4 || copy.Any(point => point is null ||
            !Battle01Initialization.WithinArea(point)) || copy.Distinct().Count() != 4)
            throw new ArgumentException("Three indexed quadrilateral regions must stay within the battle area.", nameof(vertices));
        Id = id; SourceUnknown = sourceUnknown; Vertices = Array.AsReadOnly(copy);
        TrailingByte0 = trailingByte0; TrailingByte1 = trailingByte1;
    }
    public byte Id { get; }
    public byte SourceUnknown { get; }
    public IReadOnlyList<MapPosition> Vertices { get; }
    public byte TrailingByte0 { get; }
    public byte TrailingByte1 { get; }
}

public sealed class Battle01Stats
{
    public Battle01Stats(byte level, ushort hpMax, ushort hpCurrent, byte mpMax, byte mpCurrent,
        byte attack, byte defense, byte agility, byte move, ushort status,
        IEnumerable<ushort> items, IEnumerable<byte> spells)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(spells);
        var itemCopy = items.ToArray(); var spellCopy = spells.ToArray();
        if (hpMax == 0 || hpCurrent == 0 || hpCurrent > hpMax || mpCurrent > mpMax || move == 0)
            throw new ArgumentException("This initialization requires living combatants with valid vitals and movement.");
        if (itemCopy.Length != 4 || itemCopy.Any(item => item > 255) || spellCopy.Length != 4)
            throw new ArgumentException("Four packed item words and four packed spell bytes are required.");
        Level = level; HpMax = hpMax; HpCurrent = hpCurrent; MpMax = mpMax; MpCurrent = mpCurrent;
        Attack = attack; Defense = defense; Agility = agility; Move = move; Status = status;
        Items = Array.AsReadOnly(itemCopy); Spells = Array.AsReadOnly(spellCopy);
    }
    public byte Level { get; }
    public ushort HpMax { get; }
    public ushort HpCurrent { get; }
    public byte MpMax { get; }
    public byte MpCurrent { get; }
    public byte Attack { get; }
    public byte Defense { get; }
    public byte Agility { get; }
    public byte Move { get; }
    public ushort Status { get; }
    public IReadOnlyList<ushort> Items { get; }
    public IReadOnlyList<byte> Spells { get; }
    internal Battle01Stats WithCurrentHp(ushort hp) => hp == HpCurrent ? this :
        new(Level, HpMax, hp, MpMax, MpCurrent, Attack, Defense, Agility, Move, Status, Items, Spells);
    internal Battle01Stats Initialize(byte attack, ushort status) =>
        new(Level, HpMax, HpMax, MpMax, MpMax, attack, Defense, Agility, Move, status, Items, Spells);
}

public sealed record Battle01AllyInput(byte Id, byte ClassId, Battle01Stats EffectiveStats);
public sealed record Battle01EnemyInput(byte DefinitionId, byte SourceUnknownByte, byte SpellPowerMode,
    Battle01Stats SourceStats, ushort BaseResistance, byte BaseProwess, byte MovementType, ushort BaseAiBitfield);

public sealed class Battle01Combatant
{
    internal Battle01Combatant(Battle01Deployment deployment, Battle01Stats stats, byte? classId,
        Battle01EnemyInput? enemySource, ushort? aiBitfield = null, MapPosition? position = null)
    {
        Deployment = deployment; Stats = stats; ClassId = classId; EnemySource = enemySource;
        AiBitfield = aiBitfield ?? InitializationAiBitfield;
        Position = position ?? deployment.Position;
    }
    public Battle01Deployment Deployment { get; }
    public int Index => Deployment.CombatantIndex;
    public MapPosition Position { get; }
    public Battle01Stats Stats { get; }
    public byte? ClassId { get; }
    // Enemy-only source fields remain distinct from the computed effective stats.
    public Battle01EnemyInput? EnemySource { get; }
    public byte? AdjustedBaseAttack => EnemySource is null ? null : Stats.Attack;
    public ushort? Resistance => EnemySource?.BaseResistance;
    public byte? Prowess => EnemySource?.BaseProwess;
    public byte? MovementTypeAndAiCommandSet => EnemySource is null ? null :
        (byte)((EnemySource.MovementType << 4) | Deployment.AiCommandSet);
    // Historical initialization composition; later activation updates AiBitfield.
    public ushort? InitializationAiBitfield => EnemySource is null ? null :
        (ushort)((EnemySource.BaseAiBitfield & 0xF000) | ((Deployment.Spawn & 15) << 8) | Deployment.SourceFiller);
    public ushort? AiBitfield { get; }
    internal Battle01Combatant WithAiBitfield(ushort value) => value == AiBitfield
        ? this : new(Deployment, Stats, ClassId, EnemySource, value, Position);
    internal Battle01Combatant WithPosition(MapPosition position) => position == Position
        ? this : new(Deployment, Stats, ClassId, EnemySource, AiBitfield, position);
    internal Battle01Combatant WithStats(Battle01Stats stats) => ReferenceEquals(stats, Stats)
        ? this : new(Deployment, stats, ClassId, EnemySource, AiBitfield, Position);
}

public enum Battle01Phase { BeforeFirstRound, FirstRoundGenerated, PlayerMovementSelection, PlayerActionChoice, PlayerTurnCompleted, EnemyTurnCompleted, RoundGenerated }

public sealed class Battle01InitializedState
{
    internal Battle01InitializedState(Battle01Combatant[] roster, Battle01Region[] regions,
        byte[] terrain, int[] occupancy, uint randomSeedImage)
        : this(roster, regions, terrain, occupancy, randomSeedImage, null) { }
    internal Battle01InitializedState(Battle01Combatant[] roster, Battle01Region[] regions,
        byte[] terrain, int[] occupancy, uint randomSeedImage, ushort? randomSeedCopy)
    {
        Roster = Array.AsReadOnly(roster); Regions = Array.AsReadOnly(regions);
        Terrain = Array.AsReadOnly(terrain); Occupancy = Array.AsReadOnly(occupancy); RandomSeedImage = randomSeedImage;
        RandomSeedCopy = randomSeedCopy;
    }
    internal Battle01InitializedState(Battle01InitializedState source, Battle01Combatant[] roster,
        bool[] regionFlags, ushort newlyTestedRegionMask, uint randomSeedImage, Battle01FirstRoundOrder firstRound)
    {
        Roster = Array.AsReadOnly(roster); Regions = source.Regions; Terrain = source.Terrain; Occupancy = source.Occupancy;
        AiLastTargets = source.AiLastTargets; AiMemory = source.AiMemory;
        RegionFlags90Through105 = Array.AsReadOnly(regionFlags); NewlyTestedRegionMask = newlyTestedRegionMask;
        RandomSeedImage = randomSeedImage; FirstRound = firstRound; RandomSeedCopy = source.RandomSeedCopy;
        TurnCompletion = source.TurnCompletion;
    }
    internal Battle01InitializedState(Battle01InitializedState source, Battle01Combatant[] roster,
        IReadOnlyList<int> occupancy, Battle01FirstControlState firstControl)
    {
        Roster = Array.AsReadOnly(roster); Regions = source.Regions; Terrain = source.Terrain; Occupancy = occupancy;
        AiLastTargets = source.AiLastTargets; AiMemory = source.AiMemory;
        RegionFlags90Through105 = source.RegionFlags90Through105; NewlyTestedRegionMask = source.NewlyTestedRegionMask;
        RandomSeedImage = source.RandomSeedImage; FirstRound = source.FirstRound; FirstControl = firstControl;
        TurnCompletion = source.TurnCompletion;
        RandomSeedCopy = source.RandomSeedCopy;
    }
    internal Battle01InitializedState(Battle01InitializedState source, Battle01Combatant[] roster,
        int[] occupancy, byte[] aiMemory, ushort randomSeedCopy)
    {
        Roster = Array.AsReadOnly(roster); Regions = source.Regions; Terrain = source.Terrain;
        Occupancy = Array.AsReadOnly(occupancy); AiMemory = Array.AsReadOnly(aiMemory);
        AiLastTargets = source.AiLastTargets; RegionFlags90Through105 = source.RegionFlags90Through105;
        NewlyTestedRegionMask = 0; RandomSeedImage = source.RandomSeedImage; RandomSeedCopy = randomSeedCopy;
        FirstRound = source.FirstRound; TurnCompletion = source.TurnCompletion;
    }
    internal Battle01InitializedState(Battle01InitializedState source, Battle01Combatant[] roster,
        int[] occupancy, byte[] aiMemory, ushort randomSeedCopy, uint randomSeedImage, byte[] lastTargets)
        : this(source, roster, occupancy, aiMemory, randomSeedCopy)
    {
        RandomSeedImage = randomSeedImage; AiLastTargets = Array.AsReadOnly(lastTargets);
    }
    internal Battle01InitializedState(Battle01InitializedState source, Battle01FirstRoundOrder firstRound,
        Battle01TurnCompletionReceipt completion)
    {
        Roster = source.Roster; Regions = source.Regions; Terrain = source.Terrain; Occupancy = source.Occupancy;
        AiLastTargets = source.AiLastTargets; AiMemory = source.AiMemory;
        RegionFlags90Through105 = source.RegionFlags90Through105; NewlyTestedRegionMask = source.NewlyTestedRegionMask;
        RandomSeedImage = source.RandomSeedImage; FirstRound = firstRound; TurnCompletion = completion;
        RandomSeedCopy = source.RandomSeedCopy;
        // FirstControl is intentionally absent: its provisional movement/cancel authority is consumed.
    }
    public MapId Map { get; } = new("map57");
    public int BattleIndex => 1;
    public int AreaX => 0;
    public int AreaY => 0;
    public int AreaWidth => 16;
    public int AreaHeight => 20;
    public int TerrainStride => 48;
    public byte CustomBackground => 9;
    public bool HalfExperience => true;
    public bool EnemyLeaderPresent => false;
    public IReadOnlyList<Battle01Combatant> Roster { get; }
    public IReadOnlyList<Battle01Region> Regions { get; }
    public IReadOnlyList<byte> Terrain { get; }
    // -1 is empty; values are combatant indices, never terrain bits.
    public IReadOnlyList<int> Occupancy { get; }
    public IReadOnlyList<byte> AiLastTargets { get; } = Array.AsReadOnly(Enumerable.Repeat((byte)255, 48).ToArray());
    public IReadOnlyList<byte> AiMemory { get; } = Array.AsReadOnly(new byte[48]);
    public IReadOnlyList<bool> RegionFlags90Through105 { get; } = Array.AsReadOnly(new bool[16]);
    // Four-byte big-endian RAM image. The main generator owns only its high 16-bit word.
    public uint RandomSeedImage { get; }
    // Independent big-endian word; thinking RNG updates its high byte. Null is unsupplied input.
    public ushort? RandomSeedCopy { get; }
    public ushort GeneratorWord => (ushort)(RandomSeedImage >> 16);
    public ushort NewlyTestedRegionMask { get; }
    public Battle01FirstRoundOrder? FirstRound { get; }
    public Battle01FirstControlState? FirstControl { get; }
    public Battle01TurnCompletionReceipt? TurnCompletion { get; }
    public int ElapsedSeconds => 0;
    public bool SuspendedFlag88 => false;
    public bool IntroFlag451 => true;
    public bool CompletedFlag501 => false;
    public bool UnlockFlag401 => true;
    public Battle01Phase Phase => FirstControl is { } control
        ? control.Movement.Stage == Battle01PlayerMovementStage.Selection ? Battle01Phase.PlayerMovementSelection : Battle01Phase.PlayerActionChoice
        : FirstRound is { RoundNumber: > 1 } round && (TurnCompletion is null || TurnCompletion.RoundNumber < round.RoundNumber)
            ? Battle01Phase.RoundGenerated
        : TurnCompletion is { } completed ? completed.CompletedActorIndex >= 128
            ? Battle01Phase.EnemyTurnCompleted : Battle01Phase.PlayerTurnCompleted
        : FirstRound is null ? Battle01Phase.BeforeFirstRound : Battle01Phase.FirstRoundGenerated;
    public byte TerrainAt(MapPosition position) => Terrain[TerrainIndex(position)];
    public int OccupantAt(MapPosition position) => Occupancy[TerrainIndex(position)];
    private static int TerrainIndex(MapPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        if (position.X < 0 || position.X >= 48 || position.Y < 0 || position.Y >= 48)
            throw new ArgumentOutOfRangeException(nameof(position));
        return position.Y * 48 + position.X;
    }
}

public static class Battle01Initialization
{
    public static Battle01InitializedState Initialize(IEnumerable<Battle01Deployment> deployment,
        IEnumerable<Battle01Region> regions, IEnumerable<byte> terrain,
        IEnumerable<Battle01AllyInput> allies, Battle01EnemyInput enemy, uint randomSeedImage, byte difficulty,
        ushort? randomSeedCopy = null)
    {
        ArgumentNullException.ThrowIfNull(deployment); ArgumentNullException.ThrowIfNull(regions);
        ArgumentNullException.ThrowIfNull(terrain); ArgumentNullException.ThrowIfNull(allies);
        ArgumentNullException.ThrowIfNull(enemy);
        if (randomSeedImage != 0x00001234 || difficulty != 0)
            throw new ArgumentException("Only the controlled RAM image 0x00001234 and difficulty zero are supported.");
        if (randomSeedCopy is not null and not 0x1234)
            throw new ArgumentException("The separately supplied comparison seed-copy must be 0x1234.", nameof(randomSeedCopy));
        var rows = deployment.ToArray(); var regionCopy = regions.ToArray();
        var rawTerrain = terrain.ToArray(); var party = allies.ToArray();
        int[] indices = [0, 1, 2, 128, 129, 130, 131, 132, 133];
        if (rows.Length != 9 || rows.Where((row, index) => row is null ||
            row.Ordinal != index || row.CombatantIndex != indices[index] ||
            row.Identity != (index < 3 ? index : 39) || row.Position is null || !WithinArea(row.Position) ||
            row.AiCommandSet is not (0 or 6 or 7) || row.ItemWord != 127 || row.PrimaryOrder != 255 ||
            row.SecondaryOrder != 255 || row.PrimaryRegion > 2 ||
            (row.SecondaryRegion > 2 && row.SecondaryRegion != 15) || row.Spawn != 0).Any() ||
            rows.Select(row => row.Position).Distinct().Count() != 9)
            throw new ArgumentException("The initialization requires nine ordered, distinct STARTING placements.", nameof(deployment));
        if (regionCopy.Length != 3 || regionCopy.Where((region, index) => region is null || region.Id != index).Any())
            throw new ArgumentException("The selected three regions must remain ordered.", nameof(regions));
        if (party.Length != 3 || party.Where((ally, index) => ally is null || ally.Id != index ||
            ally.EffectiveStats is null || ally.EffectiveStats.Level == 0).Any())
            throw new ArgumentException("Ordered living ally slots 0, 1 and 2 are required.", nameof(allies));
        if (rawTerrain.Length != 48 * 48 || rawTerrain.Any(value => value > 8 && value != 255))
            throw new ArgumentException("Terrain must retain the full raw 48-by-48 grid.", nameof(terrain));
        if (rows.Any(row => rawTerrain[row.Position.Y * 48 + row.Position.X] == 255))
            throw new ArgumentException("A starting combatant cannot occupy obstructed raw terrain.", nameof(terrain));
        Battle01Stats baseline = enemy.SourceStats ?? throw new ArgumentException("Missing enemy source stats.", nameof(enemy));
        if (enemy.DefinitionId != 39 || enemy.SourceUnknownByte != 39 || enemy.SpellPowerMode != 0 ||
            baseline.Level != 0 || baseline.HpMax != 5 || baseline.MpMax != 0 ||
            baseline.Attack != 7 || baseline.Defense != 5 || baseline.Agility != 5 || baseline.Move != 5 ||
            baseline.Status != 0 || baseline.Items.Any(item => item != 127) || baseline.Spells.Any(spell => spell != 63) ||
            enemy.BaseResistance != 0x40E3 || enemy.BaseProwess != 0 || enemy.MovementType != 6 || enemy.BaseAiBitfield != 0x2000)
            throw new ArgumentException("Only the fixed GIZMO source baseline is supported.", nameof(enemy));

        // Before-battle presentation is skipped by controlled policy. Heal the already-refreshed
        // effective ally stats, then initialize deployment, clear AI state, load terrain and set F451.
        // No upgrade selector is required for fixed Battle01; no equipment/status modifier applies to GIZMO.
        var roster = new Battle01Combatant[9];
        var occupancy = Enumerable.Repeat(-1, rawTerrain.Length).ToArray();
        for (int index = 0; index < 9; index++)
        {
            Battle01Stats stats = index < 3
                ? party[index].EffectiveStats.Initialize(party[index].EffectiveStats.Attack,
                    (ushort)(party[index].EffectiveStats.Status & 0x0007))
                // Accepted bounded policy: fixed difficulty0 source7 -> floor(7*5/4), once.
                : baseline.Initialize((byte)(baseline.Attack * 5 / 4), baseline.Status);
            roster[index] = new(rows[index], stats, index < 3 ? party[index].ClassId : null, index < 3 ? null : enemy);
            occupancy[rows[index].Position.Y * 48 + rows[index].Position.X] = rows[index].CombatantIndex;
        }
        return new(roster, regionCopy, rawTerrain, occupancy, randomSeedImage, randomSeedCopy);
    }
    internal static bool WithinArea(MapPosition position) => position.X >= 0 && position.X < 16 &&
        position.Y >= 0 && position.Y < 20;
}

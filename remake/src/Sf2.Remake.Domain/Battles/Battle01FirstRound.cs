using System.Runtime.CompilerServices;
using Sf2.Remake.Domain.Maps;

[assembly: InternalsVisibleTo("Sf2.Remake.Domain.Tests")]

namespace Sf2.Remake.Domain.Battles;

public readonly record struct Battle01TurnEntry(byte CombatantIndex, byte AlteredAgility)
{
    public bool IsSentinel => CombatantIndex == 255;
}

public sealed class Battle01FirstRoundOrder
{
    public const int EntrySize = 2;
    internal Battle01FirstRoundOrder(Battle01TurnEntry[] slots, int[] regionCutsceneRows, int[] spawnedCombatants)
    {
        Slots = Array.AsReadOnly(slots); RegionCutsceneRows = Array.AsReadOnly(regionCutsceneRows);
        SpawnedCombatants = Array.AsReadOnly(spawnedCombatants);
    }
    // Preserve the entire source buffer, including sentinel entries inside signed-boundary results.
    public IReadOnlyList<Battle01TurnEntry> Slots { get; }
    // Raw source byte offset, never a slot index. FirstCandidate remains historical.
    public byte CurrentTurnOffset { get; }
    public Battle01TurnEntry? FirstCandidate => Slots[0].IsSentinel ? null : Slots[0];
    public Battle01TurnEntry? CurrentCandidate => Slots[CurrentTurnOffset / EntrySize].IsSentinel
        ? null : Slots[CurrentTurnOffset / EntrySize];
    public IReadOnlyList<int> RegionCutsceneRows { get; }
    public IReadOnlyList<int> SpawnedCombatants { get; }
    private Battle01FirstRoundOrder(Battle01FirstRoundOrder source)
    {
        Slots = source.Slots; RegionCutsceneRows = source.RegionCutsceneRows; SpawnedCombatants = source.SpawnedCombatants;
        CurrentTurnOffset = checked((byte)(source.CurrentTurnOffset + EntrySize));
    }
    internal Battle01FirstRoundOrder AdvanceCompletedPlayerTurn()
    {
        if (Slots.Count != 64 || CurrentTurnOffset % EntrySize != 0 ||
            CurrentTurnOffset >= (Slots.Count - 1) * EntrySize || CurrentCandidate is null)
            throw new ArgumentException("A completed actor may advance one entry; round regeneration is unsupported.", "turnOrder");
        return new(this);
    }
}

public static class Battle01FirstRound
{
    public static Battle01InitializedState Enter(Battle01InitializedState current)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Phase != Battle01Phase.BeforeFirstRound)
            throw new ArgumentException("Only the pre-first-round phase can enter this transition.", nameof(current));
        // Keep the original semantic order. These helpers only project local immutable output.
        var (roster, flags, tested) = ActivateEnemies(current);
        int[] cutscenes = RouteBattle01RegionCutscenes();
        int[] spawned = AdmitStartingSpawns(roster);
        ushort word = current.GeneratorWord;
        var slots = GenerateTurnOrder(roster.Select(unit => new TurnCandidate((byte)unit.Index,
            (byte)unit.Position.X, unit.Stats.HpCurrent, unit.Stats.Agility)), ref word);
        uint image = ((uint)word << 16) | (current.RandomSeedImage & 0xFFFFu);
        return new(current, roster, flags, tested, image, new(slots, cutscenes, spawned));
    }

    private static (Battle01Combatant[] Roster, bool[] Flags, ushort Tested) ActivateEnemies(Battle01InitializedState current)
    {
        var flags = current.RegionFlags90Through105.ToArray();
        ushort tested = current.NewlyTestedRegionMask;
        var roster = current.Roster.ToArray();
        var allies = roster.Where(unit => unit.Index < 128 && unit.Position.X < 128 && unit.Stats.HpCurrent != 0).ToArray();
        for (int index = 0; index < roster.Length; index++)
        {
            var enemy = roster[index];
            if (enemy.Index < 128 || enemy.Position.X >= 128 || enemy.Stats.HpCurrent == 0) continue;
            foreach (var region in current.Regions)
            {
                ushort bit = (ushort)(1 << region.Id);
                if ((tested & bit) == 0)
                {
                    if (allies.Any(ally => IsInside(region, ally.Position))) flags[region.Id] = true;
                    tested |= bit; // Tested once per round entry; this is not an active-region flag.
                }
            }
            var placement = enemy.Deployment;
            roster[index] = enemy.WithAiBitfield(ActivateAssignedRegions(enemy.AiBitfield!.Value,
                placement.PrimaryRegion, placement.SecondaryRegion, flags));
        }
        return (roster, flags, tested);
    }

    internal static ushort ActivateAssignedRegions(ushort bits, byte primary, byte secondary, IReadOnlyList<bool> flags)
    {
        // An active primary region takes precedence; secondary activation sets both low bits.
        if (primary != 15 && flags[primary]) return (ushort)(bits | 1);
        if (secondary != 15 && flags[secondary]) return (ushort)(bits | 3);
        return bits;
    }

    internal static bool IsInside(Battle01Region region, MapPosition point)
    {
        var v = region.Vertices;
        return InsideTriangle(v[0], v[1], v[3], point) || InsideTriangle(v[2], v[1], v[3], point);
    }
    private static bool InsideTriangle(MapPosition a, MapPosition b, MapPosition c, MapPosition point)
    {
        if (Cross(a, b, c) == 0) throw new ArgumentException("A trigger triangle must have nonzero area.", "regions");
        int ab = Cross(a, b, point), bc = Cross(b, c, point), ca = Cross(c, a, point);
        return (ab >= 0 && bc >= 0 && ca >= 0) || (ab <= 0 && bc <= 0 && ca <= 0);
    }
    private static int Cross(MapPosition a, MapPosition b, MapPosition point) =>
        (b.X - a.X) * (point.Y - a.Y) - (b.Y - a.Y) * (point.X - a.X);

    private static int[] RouteBattle01RegionCutscenes() => []; // Accepted table has no Battle01 row.
    private static int[] AdmitStartingSpawns(IReadOnlyList<Battle01Combatant> roster)
    {
        if (roster.Any(unit => unit.Deployment.Spawn != 0 ||
            (unit.AiBitfield is { } bits && (bits & 0x0300) != 0)))
            throw new ArgumentException("This first-round boundary supports only the existing STARTING roster.", "spawn");
        return []; // No respawn/hidden candidate; no duplicated roster, animation or RNG call.
    }

    internal readonly record struct TurnCandidate(byte Index, byte X, ushort CurrentHp, byte Agility);
    internal static Battle01TurnEntry[] GenerateTurnOrder(IEnumerable<TurnCandidate> candidates, ref ushort generatorWord)
    {
        var ordered = candidates.OrderBy(candidate => candidate.Index).ToArray();
        if (ordered.Select(candidate => candidate.Index).Distinct().Count() != ordered.Length ||
            ordered.Any(candidate => candidate.Index is > 29 and < 128 or > 159))
            throw new ArgumentException("Turn candidates require unique original ally/enemy indices.", nameof(candidates));
        var slots = Enumerable.Repeat(new Battle01TurnEntry(255, 255), 64).ToArray();
        int length = 0;
        foreach (var candidate in ordered)
        {
            if (candidate.X >= 128 || candidate.CurrentHp == 0) continue;
            int basis = candidate.Agility & 0x7F;
            int score = basis;
            ushort range = (ushort)(basis >> 3);
            score += NextRandom(ref generatorWord, range);
            score -= NextRandom(ref generatorWord, range);
            score += NextRandom(ref generatorWord, 3) - 1;
            Add(candidate.Index, score);
            if (candidate.Agility >= 128)
            {
                basis = basis * 5 / 6; range = (ushort)(basis >> 3); score = basis;
                score += NextRandom(ref generatorWord, range);
                score -= NextRandom(ref generatorWord, range);
                Add(candidate.Index, score);
            }
        }
        // Source uses 62 passes across all 64 slots, comparing only the signed agility byte.
        // Sentinels participate; never compact a negative-score entry around them.
        for (int pass = 0; pass < 62; pass++)
            for (int index = 0; index < 63; index++)
                if (unchecked((sbyte)slots[index + 1].AlteredAgility) > unchecked((sbyte)slots[index].AlteredAgility))
                    (slots[index], slots[index + 1]) = (slots[index + 1], slots[index]);
        return slots;

        void Add(byte index, int score)
        {
            if (length == slots.Length) throw new ArgumentException("The bounded turn buffer is full.", nameof(candidates));
            slots[length++] = new(index, unchecked((byte)score));
        }
    }

    internal static ushort NextRandom(ref ushort word, ushort range)
    {
        word = unchecked((ushort)(word * 13 + 7));
        ushort doubledRange = unchecked((ushort)(range * 2));
        return (ushort)((((uint)word * doubledRange) >> 16) >> 1);
    }
}

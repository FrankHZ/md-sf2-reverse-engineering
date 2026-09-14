using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal sealed record StandbyOccupant<TActor>(TActor Actor, MapPosition Position, ushort? ActivationWord);
internal sealed record StandbyCandidate<TActor>(byte Index, MapPosition Position, int? Cost, TActor? Occupant, bool Eligible) where TActor : struct;
internal sealed record StandbyDecision<TActor>(MapPosition Destination, ushort SeedAfter, byte MemoryAfter,
    IReadOnlyList<ThinkingRandomDraw> Rolls, IReadOnlyList<StandbyCandidate<TActor>> Candidates,
    IReadOnlyList<byte> MoveString, WeightedMovementGrid? Grid) where TActor : struct;

// Pinned regular standby: source anchor, packed memory and an independent thinking channel.
// The grid and occupancy belong to the caller; no actor ID, round or receipt admits a decision.
internal static class AiStandbyRules
{
    internal static StandbyDecision<TActor> Decide<TActor>(MapPosition origin, MapPosition anchor,
        byte primaryOrder, byte secondaryOrder, byte primaryRegion, byte secondaryRegion,
        ushort seed, byte memory, Func<WeightedMovementGrid> buildGrid,
        IReadOnlyList<StandbyOccupant<TActor>> occupants, int width, int height) where TActor : struct
    {
        List<ThinkingRandomDraw> rolls = []; List<StandbyCandidate<TActor>> candidates = [];
        WeightedMovementGrid? grid = null;
        StandbyDecision<TActor> Result(MapPosition destination, IReadOnlyList<byte> path) =>
            new(destination, seed, memory, rolls.AsReadOnly(), candidates.AsReadOnly(), path, grid);
        byte Roll(byte range)
        { var roll = BattleRandom.NextThinkingWord(seed, range); rolls.Add(roll); seed = roll.After; return roll.Value; }
        if (Roll(8) is 2 or 4 or 6) return Result(origin, Array.AsReadOnly<byte>([255]));
        bool p = primaryOrder != 255, s = secondaryOrder != 255, pr = primaryRegion != 15, sr = secondaryRegion != 15;
        if ((p && pr) || (s && sr)) return Result(origin, Array.AsReadOnly<byte>([255]));
        if (p && !pr && !s && sr) throw new BattleRuleException("standby-move-order", "ai.moveOrder", true);
        if (!((!p && pr) || (!s && sr))) return Result(origin, Array.AsReadOnly<byte>([255]));
        grid = buildGrid();
        if (occupants.Any(unit => unit.ActivationWord is null && grid.CostAt(unit.Position) is not null))
            throw new BattleRuleException("source-occupancy-word", "actors.activationWord", true);
        if ((memory & 15) == 0) memory = Roll(2) == 0 ? (byte)4 : (byte)3;
        int count = memory & 15, previous = memory >> 4;
        if (count is not (3 or 4) || previous >= 4)
            throw new BattleRuleException("standby-memory", "actor.aiMemory", true);
        (int X, int Y)[] offsets = count == 3 ? [(0, -1), (-1, 1), (1, 1)] : [(0, -1), (-1, 0), (0, 1), (1, 0)];
        List<byte> valid = [];
        for (byte index = 0; index < count; index++)
        {
            int x = anchor.X + offsets[index].X, y = anchor.Y + offsets[index].Y;
            if (x is < 0 or >= 48 || y is < 0 or >= 48) continue;
            var position = new MapPosition(x, y); int? cost = grid.CostAt(position);
            var occupant = occupants.FirstOrDefault(unit => unit.Position == position && unit.ActivationWord is { } word && (word & 8) == 0);
            bool eligible = cost == 0 || (cost is not null && occupant is null);
            candidates.Add(new(index, position, cost, occupant is null ? null : occupant.Actor, eligible));
            if (eligible && index != previous) valid.Add(index);
        }
        if (valid.Count == 0) { memory = 0; return Result(origin, Array.AsReadOnly<byte>([255])); }
        byte chosen = valid[Roll((byte)valid.Count)]; memory = (byte)((chosen << 4) | count);
        var destination = candidates.Single(candidate => candidate.Index == chosen).Position;
        return Result(destination, AiMovementRules.MoveString(grid, origin, destination, width, height));
    }
}

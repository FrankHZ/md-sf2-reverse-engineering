using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// Supported source mover interpretations, shared by authored and initialized private sessions.
public static class BattleTerrainRules
{
    // Source UI setting, not the physical damage multiplier or a movement cost.
    public static int LandEffect(BattleTerrain terrain, BattleMover mover)
    {
        if (!Enum.IsDefined(mover)) throw new BattleRuleException("movement-profile", "actor.mover", true);
        if (mover == BattleMover.Flying) return 0;
        return terrain.Protection switch
        {
            TerrainProtection.None => 0, TerrainProtection.Light => 15, TerrainProtection.Heavy => 30,
            _ => throw new BattleRuleException("terrain-protection", "terrain.protection", true),
        };
    }

    internal static WeightedMovementGrid MovementGrid(IReadOnlyList<BattleTerrain> terrain, MapPosition origin,
        BattleMover mover, byte move, IEnumerable<MapPosition> enemies)
    {
        var costs = terrain.Select(tile => MovementCost(tile, mover)).ToArray();
        if (terrain.Count != 48 * 48) throw new ArgumentException("Expected a 48 by 48 terrain grid.", nameof(terrain));
        foreach (var position in enemies) costs[Offset(position)] = -1;
        return WeightedMovement.Build(costs, Offset(origin), move * 2);

        static int Offset(MapPosition cell) => cell.X is >= 0 and < 48 && cell.Y is >= 0 and < 48
            ? cell.Y * 48 + cell.X : throw new ArgumentOutOfRangeException(nameof(cell));
    }

    public static IReadOnlyList<MapPosition> ReachableCells(IReadOnlyList<BattleTerrain> terrain, MapPosition origin,
        BattleMover mover, byte move, IEnumerable<MapPosition> enemies)
    {
        var grid = MovementGrid(terrain, origin, mover, move, enemies);
        var cells = new List<MapPosition>();
        for (int i = 0; i < grid.MovableGrid.Count; i++)
            if (grid.CostAtOffset(i).HasValue) cells.Add(new(i % 48, i / 48));
        return cells.AsReadOnly();
    }

    internal static sbyte MovementCost(BattleTerrain terrain, BattleMover mover = BattleMover.Regular)
    {
        if (!Enum.IsDefined(mover)) throw new BattleRuleException("movement-profile", "actor.mover", true);
        if (terrain.Surface == TerrainSurface.Barrier) return -1;
        if (mover == BattleMover.Flying && Enum.IsDefined(terrain.Surface)) return 2;
        if (terrain.Surface == TerrainSurface.HighSky) return -1;
        if (mover == BattleMover.Hovering && Enum.IsDefined(terrain.Surface)) return 2;
        if (mover == BattleMover.CentaurArcher) mover = BattleMover.Centaur;
        if (mover == BattleMover.StealthArcher) mover = BattleMover.Stealth;
        return terrain.Surface switch
        {
            TerrainSurface.Open => 2,
            TerrainSurface.Brush => mover == BattleMover.Stealth ? (sbyte)2 : (sbyte)3,
            TerrainSurface.Rough => mover == BattleMover.Stealth ? (sbyte)2 : mover == BattleMover.Centaur ? (sbyte)5 : (sbyte)3,
            TerrainSurface.Deep => mover == BattleMover.Stealth ? (sbyte)2 : mover == BattleMover.Centaur ? (sbyte)5 : (sbyte)4,
            TerrainSurface.Desert => mover == BattleMover.Stealth ? (sbyte)4 : mover == BattleMover.Centaur ? (sbyte)5 : (sbyte)3,
            TerrainSurface.Impassable => -1,
            TerrainSurface.HighSky => -1,
            _ => throw new BattleRuleException("terrain-surface", "terrain.surface", true),
        };
    }

}

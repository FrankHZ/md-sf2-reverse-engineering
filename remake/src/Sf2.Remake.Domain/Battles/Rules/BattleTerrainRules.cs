namespace Sf2.Remake.Domain.Battles;

// Supported source mover interpretations, shared by authored and initialized private sessions.
internal static class BattleTerrainRules
{
    internal static sbyte MovementCost(BattleTerrain terrain, BattleMover mover = BattleMover.Regular)
    {
        if (!Enum.IsDefined(mover)) throw new BattleRuleException("movement-profile", "actor.mover", true);
        if (terrain.Surface == TerrainSurface.Barrier) return -1;
        if (mover == BattleMover.Hovering && Enum.IsDefined(terrain.Surface)) return 2;
        return terrain.Surface switch
        {
            TerrainSurface.Open => 2,
            TerrainSurface.Brush => 3,
            TerrainSurface.Rough => mover == BattleMover.Centaur ? (sbyte)5 : (sbyte)3,
            TerrainSurface.Deep => mover == BattleMover.Centaur ? (sbyte)5 : (sbyte)4,
            TerrainSurface.Impassable => -1,
            _ => throw new BattleRuleException("terrain-surface", "terrain.surface", true),
        };
    }

    internal static int LandMultiplier(BattleTerrain terrain, BattleMover mover = BattleMover.Regular)
    {
        if (!Enum.IsDefined(mover)) throw new BattleRuleException("movement-profile", "actor.mover", true);
        // Hovering changes movement/dodge, not the source terrain protection nibble.
        return terrain.Protection switch
        {
            TerrainProtection.None => 256, TerrainProtection.Light => 230, TerrainProtection.Heavy => 205,
            _ => throw new BattleRuleException("terrain-protection", "terrain.protection", true),
        };
    }
}

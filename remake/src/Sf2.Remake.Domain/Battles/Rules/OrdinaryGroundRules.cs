namespace Sf2.Remake.Domain.Battles;

// The currently admitted regular and priest movers share these costs and land effects.
// Other mover profiles remain a separate capability; terrain itself contains no mover cost table.
internal static class OrdinaryGroundRules
{
    internal static sbyte MovementCost(BattleTerrain terrain) => terrain.Surface switch
    {
        TerrainSurface.Open => 2,
        TerrainSurface.Brush or TerrainSurface.Rough => 3,
        TerrainSurface.Deep => 4,
        TerrainSurface.Impassable or TerrainSurface.Barrier => -1,
        _ => throw new BattleRuleException("terrain-surface", "terrain.surface", true),
    };

    internal static int LandMultiplier(BattleTerrain terrain) => terrain.Protection switch
    {
        TerrainProtection.None => 256,
        TerrainProtection.Light => 230,
        TerrainProtection.Heavy => 205,
        _ => throw new BattleRuleException("terrain-protection", "terrain.protection", true),
    };
}

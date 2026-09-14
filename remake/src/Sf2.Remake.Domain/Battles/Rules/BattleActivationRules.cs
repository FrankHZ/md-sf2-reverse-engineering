using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class BattleActivationRules
{
    internal static void Validate(BattleDefinition definition)
    {
        var initialization = definition.Initialization!;
        if (initialization.RegionProgram != BattleRegionProgram.None)
            throw new BattleRuleException("region-program", "encounter.regionProgram", true);
        var ids = new HashSet<byte>();
        foreach (var region in initialization.Regions)
        {
            if (region.Id >= 16 || !ids.Add(region.Id) || region.Vertices.Count != 4 ||
                region.Vertices.Any(point => point is null || !definition.Contains(point)) ||
                Cross(region.Vertices[0], region.Vertices[1], region.Vertices[3]) == 0 ||
                Cross(region.Vertices[2], region.Vertices[1], region.Vertices[3]) == 0)
                throw new BattleRuleException("activation-region", "encounter.regions");
        }
        foreach (var deployment in definition.Deployments)
        {
            if (deployment.Initialization is not { } source)
                throw new BattleRuleException("missing-deployment-initialization", "placements.initialization");
            if (source.SpawnMode != 0)
                throw new BattleRuleException("spawn-mode", "placements.spawn", true);
            if (source.PrimaryRegion != 15 && !ids.Contains(source.PrimaryRegion) ||
                source.SecondaryRegion != 15 && !ids.Contains(source.SecondaryRegion))
                throw new BattleRuleException("missing-activation-region", "placements.region");
        }
    }

    internal static EngineBattleState BeforeRound(EngineBattleState before)
    {
        // Validate every reached region/program/spawn prerequisite before any RNG or publication.
        Validate(before.Definition);
        var flags = before.Regions!.Flags.ToArray(); ushort tested = 0;
        var allies = before.Actors.Where(actor => actor.IsAlly && actor.Hp > 0 && actor.Position is not null).ToArray();
        var actors = before.Actors.ToArray();
        for (int index = 0; index < actors.Length; index++)
        {
            var enemy = actors[index];
            if (enemy.IsAlly || enemy.Hp == 0 || enemy.Position is null) continue;
            foreach (var region in before.Definition.Initialization!.Regions)
            {
                ushort bit = (ushort)(1 << region.Id);
                if ((tested & bit) != 0) continue;
                if (allies.Any(ally => Includes(region.Vertices, ally.Position!))) flags[region.Id] = true;
                tested |= bit;
            }
            var source = enemy.Deployment.Initialization!;
            if (enemy.ActivationWord is not { } word)
                throw new BattleRuleException("missing-activation-word", "actor.activation");
            actors[index] = enemy.With(activationWord: ActivateAssignedRegions(word, source.PrimaryRegion, source.SecondaryRegion, flags));
        }
        // The admitted source table has no region program; all roster entries are STARTING.
        // There is no hidden/respawn candidate, animation or spawn RNG call at this boundary.
        return before.With(actors: actors, regions: new(flags, tested));
    }

    internal static ushort ActivateAssignedRegions(ushort bits, byte primary, byte secondary, IReadOnlyList<bool> flags)
    {
        if (primary != 15 && flags[primary]) return (ushort)(bits | 1);
        if (secondary != 15 && flags[secondary]) return (ushort)(bits | 3);
        return bits;
    }
    internal static bool Includes(IReadOnlyList<MapPosition> vertices, MapPosition point) =>
        InsideTriangle(vertices[0], vertices[1], vertices[3], point) || InsideTriangle(vertices[2], vertices[1], vertices[3], point);
    private static bool InsideTriangle(MapPosition a, MapPosition b, MapPosition c, MapPosition point)
    {
        if (Cross(a, b, c) == 0) throw new ArgumentException("A trigger triangle must have nonzero area.", "regions");
        int ab = Cross(a, b, point), bc = Cross(b, c, point), ca = Cross(c, a, point);
        return (ab >= 0 && bc >= 0 && ca >= 0) || (ab <= 0 && bc <= 0 && ca <= 0);
    }
    private static int Cross(MapPosition a, MapPosition b, MapPosition point) =>
        (b.X - a.X) * (point.Y - a.Y) - (b.Y - a.Y) * (point.X - a.X);
}

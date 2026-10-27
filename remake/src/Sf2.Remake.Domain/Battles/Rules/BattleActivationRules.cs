using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class BattleActivationRules
{
    internal static void Validate(BattleDefinition definition)
    {
        var initialization = definition.Initialization!;
        if (initialization.RegionProgram == BattleRegionProgram.Required)
            throw new BattleRuleException("region-program", "encounter.regionProgram", true);
        var ids = new HashSet<byte>();
        foreach (var region in initialization.Regions)
        {
            if (region.Id >= 16 || !ids.Add(region.Id) || region.Vertices.Count is not (3 or 4) ||
                region.Vertices.Any(point => point is null || point.X >= 48 || point.Y >= 48))
                throw new BattleRuleException("activation-region", "encounter.regions");
        }
        if (initialization.RegionPrograms.Any(row => !ids.Contains(row.Region) || row.PlayedFlag is < 0 or > 65535 || string.IsNullOrWhiteSpace(row.Program)) ||
            initialization.RegionProgram != BattleRegionProgram.Authored && initialization.RegionPrograms.Count != 0)
            throw new BattleRuleException("region-program-rows", "encounter.regionPrograms");
        foreach (var deployment in definition.Deployments)
        {
            if (deployment.Initialization is not { } source)
                throw new BattleRuleException("missing-deployment-initialization", "placements.initialization");
            if (source.SpawnMode is not (0 or 2 or 3) || deployment.Faction == BattleFaction.Ally && source.SpawnMode != 0)
                throw new BattleRuleException("spawn-mode", "placements.spawn", true);
            if (deployment.Faction == BattleFaction.Enemy &&
                (source.PrimaryRegion != 15 && !ids.Contains(source.PrimaryRegion) ||
                source.SecondaryRegion != 15 && !ids.Contains(source.SecondaryRegion)))
                throw new BattleRuleException("missing-activation-region", "placements.region");
        }
    }

    internal static EngineBattleState BeforeRound(EngineBattleState before)
    {
        // Validate every reached region/program/spawn prerequisite before any RNG or publication.
        Validate(before.Definition);
        // TriggerRegionsAndActivateEnemies returns before testing/activation when count is zero.
        if (before.Definition.Initialization!.Regions.Count == 0) return before;
        var flags = before.Regions!.Flags.ToArray(); ushort tested = before.Regions.Tested;
        var allies = before.Actors.Where(actor => actor.IsAlly && actor.Hp is > 0 and < 32768 && actor.Position is { X: < 128, Y: < 128 }).ToArray();
        var actors = before.Actors.ToArray();
        for (int index = 0; index < actors.Length; index++)
        {
            var enemy = actors[index];
            if (enemy.IsAlly || enemy.Hp == 0 || enemy.Position is null) continue;
            foreach (var region in before.Definition.Initialization!.Regions)
            {
                ushort bit = (ushort)(1 << region.Id);
                if ((tested & bit) != 0) continue;
                if (allies.Any(ally => SourceRegionGeometry.Includes(region.Vertices, ally.Position!))) flags[region.Id] = true;
                tested |= bit;
            }
            var source = enemy.Deployment.Initialization!;
            if (enemy.ActivationWord is not { } word)
                throw new BattleRuleException("missing-activation-word", "actor.activation");
            actors[index] = enemy.With(activationWord: ActivateAssignedRegions(word, source.PrimaryRegion, source.SecondaryRegion, flags));
        }
        return before.With(actors: actors, regions: new(flags, tested));
    }

    internal static ushort ActivateAssignedRegions(ushort bits, byte primary, byte secondary, IReadOnlyList<bool> flags)
    {
        if (primary != 15 && flags[primary]) return (ushort)(bits | 1);
        if (secondary != 15 && flags[secondary]) return (ushort)(bits | 3);
        return bits;
    }
    internal static bool Includes(IReadOnlyList<MapPosition> vertices, MapPosition point) =>
        SourceRegionGeometry.Includes(vertices, point);
}

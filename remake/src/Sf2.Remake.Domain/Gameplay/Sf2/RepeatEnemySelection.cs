using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class RepeatEnemySelection
{
    internal static int Quotient(int level, int classId, int threshold)
    {
        int effectiveLevel = level + (unchecked((sbyte)classId) > 11 ? 20 : 0);
        return effectiveLevel < threshold ? 0 : 1 + (effectiveLevel - threshold) / 10;
    }

    private static (int Range, int Min, int Max) Category(BattleMover mover) => mover switch
    {
        BattleMover.Regular or BattleMover.BrassGunner => (5, 0, 35),
        BattleMover.Mage => (1, 67, 74),
        BattleMover.Healer => (2, 76, 84),
        BattleMover.Flying or BattleMover.Hovering => (2, 40, 52),
        _ => (0, 0, 0),
    };

    internal static bool Admits(BattleMover mover, int candidate)
    {
        var (range, min, max) = Category(mover);
        // All in-bounds airborne rolls for Battle19's Gargoyle match the
        // USA zero-length exclusion table's overread (see the ROM scan).
        return range != 0 && candidate >= min && candidate <= max &&
            mover is not (BattleMover.Flying or BattleMover.Hovering) &&
            candidate is not (10 or 13 or 14 or 15 or 32 or 38 or 71 or 80);
    }

    internal static byte Select(byte original, BattleMover mover, int quotient, ref uint main)
    {
        int range = Category(mover).Range;
        if (quotient <= 0 || range == 0) return original;
        int center = original + (range == 5 ? 5 * (quotient - 1) : quotient);
        for (int attempt = 0; attempt < 2; attempt++)
        {
            var draw = BattleRandom.NextMain(main, (ushort)(range + 1));
            main = draw.After;
            int candidate = unchecked((sbyte)(center + draw.Value));
            if (Admits(mover, candidate))
                return (byte)candidate;
        }
        return original;
    }

    internal static BattleDefinition Bind(BattleDefinition encounter, IReadOnlyList<byte> ids)
    {
        var enemies = encounter.Deployments.Where(row => row.Faction == BattleFaction.Enemy).ToArray();
        if (enemies.Length != ids.Count)
            throw new BattleRuleException("repeat-enemy-count", "encounter.placements");
        if (enemies.Select(row => row.SourceEnemy).SequenceEqual(ids.Select(id => (byte?)id))) return encounter;
        var selected = enemies.Zip(ids).ToDictionary(pair => pair.First.Actor, pair =>
            pair.First.SourceEnemy == pair.Second ? pair.First :
            pair.First.Replacements?.GetValueOrDefault(pair.Second) ??
                throw new BattleRuleException("repeat-enemy-definition", $"encounter.placements.{pair.First.ProcessingOrder}.enemy-{pair.Second}", true));
        return encounter.WithDeployments(encounter.Deployments.Select(row => selected.GetValueOrDefault(row.Actor, row)));
    }
}

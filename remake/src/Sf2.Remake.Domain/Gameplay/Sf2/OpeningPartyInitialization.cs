using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// SF2 initialization policy, independent of map/story names and diagnostic checkpoints.
internal static class OpeningPartyInitialization
{
    internal static PhysicalCriticalRule InitialCritical(BattleGrowthDefinition growth, byte startingLevel, PhysicalCriticalRule critical)
    {
        if (!growth.Spells.Any(spell => spell.Level <= startingLevel && spell.PackedSpell == 128)) return critical;
        if (critical != PhysicalCriticalRule.OneIn16WithQuarterBonus)
            throw new BattleRuleException("initial-heal3-prowess", "opening.spells", true);
        // InitializeAllyStats increments the combined double/counter nibble before LevelUp.
        // The admitted PRST baseline is03, so this source initialization yields13.
        return PhysicalCriticalRule.Thief;
    }

    internal static BattleActorStartInput Initialize(BattleDeploymentDefinition deployment, byte startingLevel, ref uint seed)
    {
        var definition = deployment.Definition;
        var growth = definition.Growth ?? throw new BattleRuleException("initial-growth-required", "opening.growth");
        if (growth.ClassId is > 11 and not (13 or 22) || startingLevel is 0 or > 40 || growth.Stats.Count != 5)
            throw new BattleRuleException("initial-class-level", "opening.ally", true);
        var items = definition.SourceLoadout?.Items ?? throw new BattleRuleException("initial-items-required", "opening.items");
        var packed = Enumerable.Repeat((byte)63, 4).ToArray();
        // InitializeAllyStats scans the initial spell list before replaying LevelUp.
        // Initial prowess is already bound to the immutable source definition before replay.
        foreach (var spell in growth.Spells.Where(spell => spell.Level <= startingLevel))
        {
            int slot = Array.FindIndex(packed, value => (value & 63) == (spell.PackedSpell & 63));
            if (slot < 0) slot = Array.FindIndex(packed, value => value == 63);
            if (slot >= 0 && (packed[slot] == 63 || packed[slot] >> 6 < spell.PackedSpell >> 6)) packed[slot] = spell.PackedSpell;
        }
        var known = packed.Where(value => value != 63).SelectMany(value => Enumerable.Range(0, (value >> 6) + 1)
            .Select(rank => growth.SpellDefinitions[(byte)((value & 63) | rank << 6)])).ToArray();
        var stats = growth.Stats.Select(stat => stat.Start).ToArray();
        var progress = new BattleActorProgress(1, stats[0], stats[1], stats[2], stats[3], stats[4],
            Array.AsReadOnly(known), new(items, packed), growth.ClassId);
        var actor = new BattleActorState(deployment, progress.MaxHp, progress.MaxMp, 0, null, 0, 0,
            status: 0, activationWord: 0, progress: progress);
        for (int level = 1; level < startingLevel; level++)
            actor = BattleGrowthRules.Grow(actor.With(exp: 100), ref seed, []);
        // Exploration's fresh start heals after growth; ordinary level-up itself never heals.
        return new(actor.Actor, actor.MaxHp, actor.MaxMp, 0, 0, 0, 0, null, actor.Progress,
            actor.SourceLoadout, ActivationWord: 0, Initialized: true);
    }
}

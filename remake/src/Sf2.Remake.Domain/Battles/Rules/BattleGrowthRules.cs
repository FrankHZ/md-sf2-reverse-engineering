namespace Sf2.Remake.Domain.Battles;

internal static class BattleGrowthRules
{
    internal static BattleActorState Award(BattleActorState actor, int amount, ref uint seed,
        List<BattleEffect> effects)
    {
        int previous = actor.Exp ?? throw new BattleRuleException("unspecified-exp", "actor.exp", true);
        int total = Math.Min(200, previous + amount);
        effects.Add(new("exp", actor.Actor, previous, total));
        if (total < 100) return actor.With(exp: (byte)total);
        var growth = actor.Definition.Growth ?? throw new BattleRuleException("level-up", "actor.growth", true);
        if (actor.Status != 0) throw new BattleRuleException("growth-status-refresh", "actor.status", true);
        total -= 100; // Source bsc0F processes one threshold even when another 100 remains.
        effects.Add(new("exp-threshold", actor.Actor, total + 100, total));
        if (actor.Level >= (growth.ClassId < 12 ? 40 : 99))
        {
            effects.Add(new("level-cap", actor.Actor, actor.Level, 255));
            return actor.With(exp: (byte)total);
        }
        int[] current = [actor.MaxHp, actor.MaxMp, actor.BaseAttack, actor.Defense, actor.Agility];
        int[] increased = new int[5];
        string[] names = ["max-hp", "max-mp", "base-attack", "defense", "agility"];
        for (int i = 0; i < current.Length; i++)
        {
            int gain = Gain(growth.Stats[i], actor.Level, current[i], actor.Actor, ref seed, effects);
            increased[i] = i == 0 ? ClampWord(current[i], gain) : Math.Min(i == 4 ? 100 : 200, current[i] + gain);
            effects.Add(new("level-" + names[i], actor.Actor, current[i], increased[i]));
        }
        byte level = (byte)(actor.Level + 1);
        // Preserve the source class-11 spell threshold comparison, independently of the cap.
        int effectiveLevel = level + (growth.ClassId >= 11 ? 20 : 0);
        var loadout = actor.SourceLoadout;
        var known = actor.Spells.ToList();
        var learned = growth.Spells.FirstOrDefault(spell => spell.Level == effectiveLevel);
        if (learned is not null)
        {
            if (loadout is null) throw new BattleRuleException("growth-spellbook", "actor.sourceLoadout", true);
            byte[] packed = loadout.Spells.ToArray();
            int slot = Array.FindIndex(packed, value => (value & 63) == (learned.PackedSpell & 63));
            // Source LearnSpell leaves a same/higher known rank unchanged and reports no success.
            bool alreadyKnown = slot >= 0 && (packed[slot] >> 6) >= (learned.PackedSpell >> 6);
            if (slot < 0) slot = Array.FindIndex(packed, value => (value & 63) == 63);
            if (slot >= 0 && !alreadyKnown)
            {
                packed[slot] = learned.PackedSpell;
                loadout = new(loadout.Items, packed);
                for (int rank = 0; rank <= learned.PackedSpell >> 6; rank++)
                {
                    byte key = (byte)((learned.PackedSpell & 63) | rank << 6);
                    if (!growth.SpellDefinitions.TryGetValue(key, out var spell))
                        throw new BattleRuleException("growth-spell-definition", "actor.growth.spells", true);
                    if (!known.Contains(spell)) known.Add(spell);
                }
                effects.Add(new("spell-learned", actor.Actor, After: learned.PackedSpell));
            }
        }
        var progress = new BattleActorProgress(level, (ushort)increased[0], (byte)increased[1],
            (byte)increased[2], (byte)increased[3], (byte)increased[4], known.AsReadOnly(), loadout);
        effects.Add(new("level", actor.Actor, actor.Level, level));
        // Maxima grow; current HP/MP do not heal. Reapply the admitted ATT-only equipment once.
        return actor.With(exp: (byte)total, progress: progress,
            attack: (byte)Math.Min(200, progress.BaseAttack + growth.AttackBonus));
    }

    internal static int Gain(StatGrowth growth, byte level, int current, ActorRef actor,
        ref uint seed, List<BattleEffect> effects)
    {
        if (growth.Curve.Count == 0) return 0;
        int projected = growth.Projected - growth.Start;
        int cumulative = level >= 30 ? 256 : growth.Curve[level - 1].Cumulative;
        int portion = level >= 30 ? 384 : projected * growth.Curve[level - 1].Increment;
        var first = BattleRandom.NextMain(seed, 128);
        var second = BattleRandom.NextMain(first.After, 128);
        seed = second.After;
        effects.Add(new("rng-growth-plus", actor, first.Before, first.After, 128, first.Value));
        effects.Add(new("rng-growth-minus", actor, second.Before, second.After, 128, second.Value));
        int gain = (ushort)(portion + first.Value - second.Value + 128) >> 8;
        int minimum = growth.Start + ((ushort)(projected * cumulative + 128) >> 8);
        return gain + (current + gain < minimum ? 1 : 0);
    }

    private static int ClampWord(int current, int gain)
    {
        ushort sum = unchecked((ushort)(current + gain));
        return (short)sum < 0 ? 200 : Math.Min(200, (int)sum);
    }
}

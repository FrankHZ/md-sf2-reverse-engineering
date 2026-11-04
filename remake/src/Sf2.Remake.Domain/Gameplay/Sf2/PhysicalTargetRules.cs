using Sf2.Remake.Domain.Battles;
namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal enum PhysicalPriorityTable { Regular, Flying, Archer, Mage }
internal readonly record struct PhysicalTargetPriority(byte Movement, byte Priority, byte? ClassId);
internal readonly record struct PhysicalTargetSelection(int Index, byte RawPriority, byte CappedPriority);

internal static class PhysicalTargetRules
{
    internal static PhysicalPriorityTable Table(BattleMover mover) => mover switch
    {
        BattleMover.Flying or BattleMover.Hovering => PhysicalPriorityTable.Flying,
        BattleMover.Archer => PhysicalPriorityTable.Archer, BattleMover.Mage => PhysicalPriorityTable.Mage,
        _ => PhysicalPriorityTable.Regular,
    };

    internal static byte ScriptThree(byte movement, int remainingHp, byte thinkingResult)
    {
        byte doubled = unchecked((byte)(movement * 2));
        return thinkingResult == 0 ? (remainingHp == 0 ? (byte)16 : (byte)1)
            : doubled <= 18 ? (byte)(19 - doubled) : (byte)1;
    }

    internal static byte Score(BattleActorState actor, BattleActorState target, byte movement,
        int potentialDamage, ref uint thinking, List<BattleEffect> effects)
    {
        int remaining = Math.Max(0, target.Hp - potentialDamage);
        // Normal PRIORITYMOD_3 selects script4. Its lethality/one-fifth test
        // draws neither channel. Existing ordinary script3 uses thinking only.
        if (!actor.IsAlly && (actor.ActivationWord >> 12) == 3)
            return (byte)(1 + (remaining == 0 ? 15 : 0) + (unchecked((ushort)(5 * remaining)) <= target.MaxHp ? 1 : 0));
        var draw = BattleRandom.NextThinkingWord((ushort)(thinking >> 16), 3);
        uint after = ((uint)draw.After << 16) | (thinking & 65535);
        effects.Add(new("thinking-rng", actor.Actor, thinking, after, 3, draw.Value, target.Actor));
        thinking = after;
        return ScriptThree(movement, remaining, draw.Value);
    }

    // Input is the reachable-target array order, not the reverse order of RNG calls.
    // determinebattleaction.asm keeps the raw maximum for collection before capping output.
    internal static PhysicalTargetSelection? Select(IReadOnlyList<PhysicalTargetPriority> targets, PhysicalPriorityTable table)
    {
        int maximum = 0;
        for (int i = targets.Count - 1; i >= 0; i--)
            maximum = Math.Max(maximum, unchecked((sbyte)targets[i].Priority));
        var cohort = Enumerable.Range(0, targets.Count).Reverse()
            .Where(i => unchecked((sbyte)targets[i].Priority) == maximum).ToArray();
        if (cohort.Length == 0) return null;
        if (cohort.Length > 1 && maximum >= 15)
        {
            var ranked = cohort.Select(i => (Index: i, Rank: ClassRank(targets[i].ClassId, table))).ToArray();
            int firstClass = ranked.Min(t => t.Rank);
            cohort = ranked.Where(t => t.Rank == firstClass).Select(t => t.Index).ToArray();
        }
        int selected = cohort[0];
        if (cohort.Length > 1)
        {
            int movement = -1;
            foreach (int index in cohort)
            {
                int value = unchecked((sbyte)targets[index].Movement);
                if (value < movement) continue;
                movement = value; selected = index;
            }
            if (movement == -1) return null;
        }
        return new(selected, (byte)maximum, (byte)Math.Min(15, maximum));
    }

    // aipriority.asm: only the seven admitted source classes. Table selection follows
    // the actual mover; geometry and class are separate content inputs.
    internal static int ClassRank(byte? classId, PhysicalPriorityTable table) => classId switch
    {
        0 => table == PhysicalPriorityTable.Archer ? 11 : 0,
        1 => table == PhysicalPriorityTable.Mage ? 10 : table == PhysicalPriorityTable.Flying ? 15 : 22,
        2 => table == PhysicalPriorityTable.Mage ? 5 : table == PhysicalPriorityTable.Flying ? 8 : 25,
        3 => table == PhysicalPriorityTable.Mage ? 21 : table == PhysicalPriorityTable.Archer ? 5 : 2,
        4 => table == PhysicalPriorityTable.Mage ? 24 : table == PhysicalPriorityTable.Archer ? 8 : 5,
        5 => table == PhysicalPriorityTable.Flying ? 27 : 15,
        10 => table == PhysicalPriorityTable.Flying ? 25 : table == PhysicalPriorityTable.Mage ? 19 : 13,
        _ => throw new BattleRuleException("ai-target-class", "target.classRule", true),
    };
}

namespace Sf2.Remake.Domain.Battles;

internal enum PhysicalPriorityTable { Regular, Flying }
internal readonly record struct PhysicalTargetPriority(byte Movement, byte Priority, byte? ClassId);
internal readonly record struct PhysicalTargetSelection(int Index, byte RawPriority, byte CappedPriority);

internal static class PhysicalTargetRules
{
    internal static byte ScriptThree(byte movement, int remainingHp, byte thinkingResult)
    {
        byte doubled = unchecked((byte)(movement * 2));
        return thinkingResult == 0 ? (remainingHp == 0 ? (byte)16 : (byte)1)
            : doubled <= 18 ? (byte)(19 - doubled) : (byte)1;
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

    // aipriority.asm: only classes needed by admitted gameplay and the existing reference
    // consumer. Flying is a reference scalar input, not a new authored movement capability.
    private static int ClassRank(byte? classId, PhysicalPriorityTable table) => classId switch
    {
        0 => 0, 4 => 5,
        1 => table == PhysicalPriorityTable.Regular ? 22 : 15,
        2 => table == PhysicalPriorityTable.Regular ? 25 : 8,
        _ => throw new BattleRuleException("ai-target-class", "target.classRule", true),
    };
}

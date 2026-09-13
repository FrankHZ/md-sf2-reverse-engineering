namespace Sf2.Remake.Domain.Battles;

internal readonly record struct TurnOrderCandidate<TActor>(TActor Actor, int ProcessingOrder, bool Placed, ushort Hp, byte Agility, bool ExtraRoundAction) where TActor : struct;
internal readonly record struct TurnOrderEntry<TActor>(TActor? Actor, byte AlteredAgility) where TActor : struct;
internal sealed record TurnOrderResult<TActor>(IReadOnlyList<TurnOrderEntry<TActor>> Slots, ushort NextSeed) where TActor : struct;

internal static class TurnOrderRules
{
    internal static TurnOrderResult<TActor> Generate<TActor>(IEnumerable<TurnOrderCandidate<TActor>> candidates, ushort seed) where TActor : struct
    {
        ArgumentNullException.ThrowIfNull(candidates);
        var ordered = candidates.OrderBy(candidate => candidate.ProcessingOrder).ToArray();
        if (ordered.Select(candidate => candidate.ProcessingOrder).Distinct().Count() != ordered.Length ||
            ordered.Select(candidate => candidate.Actor).Distinct().Count() != ordered.Length ||
            ordered.Any(candidate => candidate.ProcessingOrder < 0))
            throw new ArgumentException("Turn candidates require unique identities and nonnegative unique processing orders.", nameof(candidates));
        if (ordered.Any(candidate => candidate.Agility > 127))
            throw new ArgumentException("Numerical agility must be between 0 and 127.", nameof(candidates));
        var living = ordered.Where(candidate => candidate.Placed && candidate.Hp > 0).ToArray();
        if (living.Sum(candidate => candidate.ExtraRoundAction ? 2 : 1) > 64)
            throw new ArgumentException("The turn buffer cannot hold the living candidates.", nameof(candidates));

        var slots = Enumerable.Repeat(new TurnOrderEntry<TActor>(null, 255), 64).ToArray();
        int length = 0;
        foreach (var candidate in living)
        {
            int basis = candidate.Agility;
            ushort range = (ushort)(basis >> 3);
            int score = basis + Roll(range) - Roll(range) + Roll(3) - 1;
            slots[length++] = new(candidate.Actor, unchecked((byte)score));
            if (candidate.ExtraRoundAction)
            {
                basis = basis * 5 / 6;
                range = (ushort)(basis >> 3);
                score = basis + Roll(range) - Roll(range);
                slots[length++] = new(candidate.Actor, unchecked((byte)score));
            }
        }
        // Preserve the source's 62 passes, signed byte comparison and participating sentinels.
        for (int pass = 0; pass < 62; pass++)
            for (int index = 0; index < 63; index++)
                if (unchecked((sbyte)slots[index + 1].AlteredAgility) > unchecked((sbyte)slots[index].AlteredAgility))
                    (slots[index], slots[index + 1]) = (slots[index + 1], slots[index]);
        return new(Array.AsReadOnly(slots), seed);

        int Roll(ushort range)
        {
            var draw = BattleRandom.NextWord(seed, range);
            seed = draw.After;
            return draw.Value;
        }
    }
}

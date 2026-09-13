namespace Sf2.Remake.Domain.Battles;

internal readonly record struct TurnOrderCandidate(byte ActorSlot, bool Placed, ushort Hp, byte Agility);
internal readonly record struct TurnOrderEntry(byte ActorSlot, byte AlteredAgility);
internal sealed record TurnOrderResult(IReadOnlyList<TurnOrderEntry> Slots, ushort NextSeed);

internal static class TurnOrderRules
{
    internal static TurnOrderResult Generate(IEnumerable<TurnOrderCandidate> candidates, ushort seed)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        var ordered = candidates.OrderBy(candidate => candidate.ActorSlot).ToArray();
        if (ordered.Select(candidate => candidate.ActorSlot).Distinct().Count() != ordered.Length ||
            ordered.Any(candidate => candidate.ActorSlot is > 29 and < 128 or > 159))
            throw new ArgumentException("Turn candidates require unique original ally/enemy slots.", nameof(candidates));
        var living = ordered.Where(candidate => candidate.Placed && candidate.Hp > 0).ToArray();
        if (living.Sum(candidate => candidate.Agility >= 128 ? 2 : 1) > 64)
            throw new ArgumentException("The turn buffer cannot hold the living candidates.", nameof(candidates));

        var slots = Enumerable.Repeat(new TurnOrderEntry(255, 255), 64).ToArray();
        int length = 0;
        foreach (var candidate in living)
        {
            int basis = candidate.Agility & 0x7F;
            ushort range = (ushort)(basis >> 3);
            int score = basis + Roll(range) - Roll(range) + Roll(3) - 1;
            slots[length++] = new(candidate.ActorSlot, unchecked((byte)score));
            if (candidate.Agility >= 128)
            {
                basis = basis * 5 / 6;
                range = (ushort)(basis >> 3);
                score = basis + Roll(range) - Roll(range);
                slots[length++] = new(candidate.ActorSlot, unchecked((byte)score));
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

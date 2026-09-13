namespace Sf2.Remake.Domain.Battles;

internal readonly record struct WordRandomDraw(ushort Before, ushort After, ushort Value);
internal readonly record struct MainRandomDraw(ushort Range, uint Before, uint After, ushort Value);

internal static class BattleRandom
{
    // randomness.md: double the 16-bit range before taking the upper product word.
    internal static WordRandomDraw NextWord(ushort seed, ushort range)
    {
        ushort next = unchecked((ushort)(seed * 13 + 7));
        ushort doubledRange = unchecked((ushort)(range * 2));
        return new(seed, next, (ushort)((((uint)next * doubledRange) >> 16) >> 1));
    }

    internal static MainRandomDraw NextMain(uint image, ushort range)
    {
        var draw = NextWord((ushort)(image >> 16), range);
        return new(range, image, ((uint)draw.After << 16) | (image & 0xFFFF), draw.Value);
    }
}

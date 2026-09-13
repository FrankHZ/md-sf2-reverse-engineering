namespace Sf2.Remake.Domain.Battles;

internal readonly record struct WordRandomDraw(ushort Before, ushort After, ushort Value);
internal readonly record struct MainRandomDraw(ushort Range, uint Before, uint After, ushort Value);
internal sealed record ThinkingRandomDraw(byte Range, ushort Before, ushort After, byte Value, IReadOnlyList<byte> Bytes);

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

    internal static ThinkingRandomDraw NextThinkingWord(ushort seed, byte range)
    {
        ushort before = seed;
        var bytes = new List<byte>();
        do
        {
            ushort extended = unchecked((ushort)(short)(sbyte)(seed >> 8));
            byte next = (byte)((extended * 541 + 12345) & 255);
            seed = (ushort)((next << 8) | (seed & 255));
            bytes.Add(next);
        } while (unchecked((sbyte)range) > 1 && bytes[^1] >= range);
        return new(range, before, seed, unchecked((sbyte)range) <= 1 ? (byte)0 : bytes[^1], bytes.AsReadOnly());
    }
}

using Sf2.Remake.Content;
using Xunit;

namespace Sf2.Remake.Content.Tests;

public sealed class StackCompressedGraphicsDecoderTests
{
    [Fact]
    public void ZeroOffsetTerminatesWithoutConsumingPaddedTrailingBits()
    {
        byte[] stored = Bits("1110", "0", "0", "0", new string('0', 11));

        StackCompressedGraphicsDecodeResult result =
            StackCompressedGraphicsDecoder.Decode(stored, 0);

        Assert.Empty(result.Output);
        Assert.Equal(18, result.InputBitsConsumed);
        Assert.Equal(6, (stored.Length * 8) - result.InputBitsConsumed);
    }

    [Fact]
    public void LiteralMoveToFrontWordAndOverlappingCopyAreDecodedInOrder()
    {
        byte[] literalThenTerminator = Bits(
            "110",
            "0",
            "0",
            "0",
            "00",
            "00",
            "00",
            "00",
            new string('0', 11));
        StackCompressedGraphicsDecodeResult literal =
            StackCompressedGraphicsDecoder.Decode(literalThenTerminator, 2);

        Assert.Equal(new byte[] { 0, 0 }, literal.Output);

        byte[] overlap = Bits(
            "1111",
            "0110",
            "0",
            "0",
            "0",
            "00",
            "00",
            "00",
            "00",
            "00000000001",
            "1",
            new string('0', 11));
        StackCompressedGraphicsDecodeResult copied =
            StackCompressedGraphicsDecoder.Decode(overlap, 6);

        Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 0 }, copied.Output);
    }

    [Fact]
    public void EmptyTruncatedBadOffsetOverrunAndSizeDriftFailClosed()
    {
        Assert.Throws<InvalidDataException>(() =>
            StackCompressedGraphicsDecoder.Decode([], 0));
        Assert.Throws<ArgumentException>(() =>
            StackCompressedGraphicsDecoder.Decode([0], 1));
        Assert.Throws<InvalidDataException>(() =>
            StackCompressedGraphicsDecoder.Decode([0xE0], 0));

        byte[] badOffset = Bits(
            "1110",
            "0",
            "0",
            "0",
            "00000000001");
        Assert.Throws<InvalidDataException>(() =>
            StackCompressedGraphicsDecoder.Decode(badOffset, 0));

        byte[] literalThenTerminator = Bits(
            "110",
            "0",
            "0",
            "0",
            "00",
            "00",
            "00",
            "00",
            new string('0', 11));
        Assert.Throws<InvalidDataException>(() =>
            StackCompressedGraphicsDecoder.Decode(literalThenTerminator, 4));

        byte[] overlap = Bits(
            "1111",
            "0110",
            "0",
            "0",
            "0",
            "00",
            "00",
            "00",
            "00",
            "00000000001",
            "1",
            new string('0', 11));
        Assert.Throws<InvalidDataException>(() =>
            StackCompressedGraphicsDecoder.Decode(overlap, 2));
    }

    private static byte[] Bits(params string[] fragments)
    {
        string bits = string.Concat(fragments);
        byte[] output = new byte[(bits.Length + 7) / 8];
        for (int index = 0; index < bits.Length; index++)
        {
            if (bits[index] is not ('0' or '1'))
            {
                throw new ArgumentException("Bit fixtures may contain only zero and one.");
            }

            if (bits[index] == '1')
            {
                output[index / 8] |= checked((byte)(1 << (7 - (index % 8))));
            }
        }

        return output;
    }
}

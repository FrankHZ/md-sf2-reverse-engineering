namespace Sf2.Remake.Content;

internal sealed record StackCompressedGraphicsDecodeResult(
    byte[] Output,
    int InputBitsConsumed);

internal static class StackCompressedGraphicsDecoder
{
    public static StackCompressedGraphicsDecodeResult Decode(
        byte[] storedBytes,
        int expectedOutputBytes)
    {
        ArgumentNullException.ThrowIfNull(storedBytes);
        if (storedBytes.Length == 0)
        {
            throw new InvalidDataException("The Stack-compressed input is empty.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(expectedOutputBytes);
        if ((expectedOutputBytes & 1) != 0)
        {
            throw new ArgumentException(
                "Stack decompression produces whole words.",
                nameof(expectedOutputBytes));
        }

        BitReader reader = new(storedBytes);
        List<ushort> output = [];
        List<int> history = [.. Enumerable.Range(0, 16)];
        int expectedWords = expectedOutputBytes / sizeof(ushort);

        while (true)
        {
            int commandWord = 0;
            for (int nibble = 0; nibble < 4; nibble++)
            {
                commandWord = (commandWord << 4) | ReadCommandNibble(reader);
            }

            for (int commandBit = 15; commandBit >= 0; commandBit--)
            {
                if (((commandWord >> commandBit) & 1) == 0)
                {
                    ushort value = 0;
                    for (int nibble = 0; nibble < 4; nibble++)
                    {
                        int historyIndex = ReadHistoryIndex(reader);
                        int valueNibble = history[historyIndex];
                        history.RemoveAt(historyIndex);
                        history.Insert(0, valueNibble);
                        value = checked((ushort)((value << 4) | valueNibble));
                    }

                    output.Add(value);
                    EnsureOutputBound(output.Count, expectedWords);
                    continue;
                }

                int offset = reader.ReadBits(11);
                if (offset == 0)
                {
                    if (output.Count != expectedWords)
                    {
                        throw new InvalidDataException(
                            $"Stack decompression output-size drift: expected {expectedOutputBytes} bytes, got {output.Count * sizeof(ushort)}.");
                    }

                    byte[] decoded = new byte[expectedOutputBytes];
                    for (int index = 0; index < output.Count; index++)
                    {
                        decoded[index * 2] = checked((byte)(output[index] >> 8));
                        decoded[(index * 2) + 1] = checked((byte)(output[index] & 0xFF));
                    }

                    return new StackCompressedGraphicsDecodeResult(
                        decoded,
                        reader.Position);
                }

                if (offset > output.Count)
                {
                    throw new InvalidDataException(
                        "A Stack section-copy offset exceeds the decoded output.");
                }

                int copyLength = 2;
                while (reader.ReadBit() == 0)
                {
                    if (reader.ReadBit() == 0)
                    {
                        copyLength += 2;
                    }
                    else
                    {
                        copyLength += 1;
                        break;
                    }
                }

                for (int word = 0; word < copyLength; word++)
                {
                    output.Add(output[^offset]);
                    EnsureOutputBound(output.Count, expectedWords);
                }
            }
        }
    }

    private static int ReadCommandNibble(BitReader reader)
    {
        if (reader.ReadBit() == 0)
        {
            return 0;
        }

        int second = reader.ReadBit();
        int third = reader.ReadBit();
        if (second == 0 && third == 0)
        {
            return 1;
        }

        if (second == 0 && third == 1)
        {
            return 2;
        }

        if (second == 1 && third == 0)
        {
            return 4;
        }

        if (reader.ReadBit() == 0)
        {
            return 8;
        }

        return reader.ReadBits(4);
    }

    private static int ReadHistoryIndex(BitReader reader)
    {
        if (reader.ReadBit() == 0)
        {
            return reader.ReadBit();
        }

        if (reader.ReadBit() == 0)
        {
            return 2 + reader.ReadBit();
        }

        if (reader.ReadBit() == 0)
        {
            return 4;
        }

        for (int pairLevel = 0; pairLevel < 3; pairLevel++)
        {
            int pair = reader.ReadBits(2);
            if (pair != 3)
            {
                return 5 + (pairLevel * 3) + pair;
            }
        }

        return 14 + reader.ReadBit();
    }

    private static void EnsureOutputBound(int actualWords, int expectedWords)
    {
        if (actualWords > expectedWords)
        {
            throw new InvalidDataException(
                "Stack decompression exceeded the expected output size.");
        }
    }

    private sealed class BitReader
    {
        private readonly byte[] _bytes;

        public BitReader(byte[] bytes)
        {
            _bytes = bytes;
        }

        public int Position { get; private set; }

        public int ReadBit()
        {
            if (Position >= _bytes.Length * 8)
            {
                throw new InvalidDataException(
                    "The compressed bitstream ended before its terminator.");
            }

            int value = (_bytes[Position / 8] >> (7 - (Position % 8))) & 1;
            Position++;
            return value;
        }

        public int ReadBits(int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            int value = 0;
            for (int bit = 0; bit < count; bit++)
            {
                value = (value << 1) | ReadBit();
            }

            return value;
        }
    }
}

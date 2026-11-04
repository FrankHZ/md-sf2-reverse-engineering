using System.Buffers.Binary;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// Only the canonical 44-byte PCM16 header written by remake_audio is admitted here.
// The document supplies no filesystem path. Its selected private world owns these siblings.
internal static class PrivateAudioWave
{
    internal static byte[] Read(string directory, int command, int timer, int rate, int channels, int frames)
    {
        string parent = Path.Combine(directory, $"audio-{command:x2}-{timer:x2}");
        string path = Path.Combine(parent, $"command-{command:x2}-timer-{timer:x2}.wav");
        Require(!File.GetAttributes(parent).HasFlag(FileAttributes.ReparsePoint) &&
            !File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint), "audio-path", "presentation.audio");
        using var stream = File.OpenRead(path);
        int size = frames * channels * 2;
        Require(stream.Length == size + 44, "audio-size", "presentation.audio");
        Span<byte> header = stackalloc byte[44];
        stream.ReadExactly(header);
        Require(header[..4].SequenceEqual("RIFF"u8) && BinaryPrimitives.ReadInt32LittleEndian(header[4..]) == size + 36 &&
            header[8..16].SequenceEqual("WAVEfmt "u8) && BinaryPrimitives.ReadInt32LittleEndian(header[16..]) == 16 &&
            BinaryPrimitives.ReadUInt16LittleEndian(header[20..]) == 1 && BinaryPrimitives.ReadUInt16LittleEndian(header[22..]) == channels &&
            BinaryPrimitives.ReadInt32LittleEndian(header[24..]) == rate && BinaryPrimitives.ReadInt32LittleEndian(header[28..]) == rate * channels * 2 &&
            BinaryPrimitives.ReadUInt16LittleEndian(header[32..]) == channels * 2 && BinaryPrimitives.ReadUInt16LittleEndian(header[34..]) == 16 &&
            header[36..40].SequenceEqual("data"u8) && BinaryPrimitives.ReadInt32LittleEndian(header[40..]) == size,
            "audio-wav-format", "presentation.audio");
        byte[] pcm = new byte[size];
        stream.ReadExactly(pcm);
        return pcm;
    }
}

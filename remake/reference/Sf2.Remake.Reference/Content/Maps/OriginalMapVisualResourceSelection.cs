using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed class OriginalMapVisualResourceSelection
{
    public const int TilesetSlotCount = 5;
    public const int ProjectionByteCount = 1 + TilesetSlotCount;

    private readonly ReadOnlyCollection<byte> _tilesetSlots;

    public OriginalMapVisualResourceSelection(
        MapId map,
        byte paletteIndex,
        IEnumerable<byte> tilesetSlots)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        ArgumentNullException.ThrowIfNull(tilesetSlots);
        byte[] copiedSlots = [.. tilesetSlots.Take(TilesetSlotCount + 1)];
        if (copiedSlots.Length != TilesetSlotCount)
        {
            throw new ArgumentException(
                $"An original map visual-resource selection must contain exactly {TilesetSlotCount} ordered tileset slots.",
                nameof(tilesetSlots));
        }

        PaletteIndex = paletteIndex;
        _tilesetSlots = Array.AsReadOnly(copiedSlots);

        Span<byte> projection = stackalloc byte[ProjectionByteCount];
        projection[0] = paletteIndex;
        copiedSlots.AsSpan().CopyTo(projection[1..]);
        ProjectionDigest = Convert.ToHexString(SHA256.HashData(projection));
    }

    public MapId Map { get; }

    public byte PaletteIndex { get; }

    public IReadOnlyList<byte> TilesetSlots => _tilesetSlots;

    public string ProjectionDigest { get; }
}

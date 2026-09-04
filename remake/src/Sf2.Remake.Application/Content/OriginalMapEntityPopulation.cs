using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public enum OriginalMapEntityRecordKind
{
    Fixed,
    Walking,
    Sequenced,
}

public sealed record OriginalMapEntityRecordIdentity
{
    public OriginalMapEntityRecordIdentity(string resourceId, int oneBasedRecordOrdinal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentOutOfRangeException.ThrowIfLessThan(oneBasedRecordOrdinal, 1);
        ResourceId = resourceId;
        OneBasedRecordOrdinal = oneBasedRecordOrdinal;
    }

    public string ResourceId { get; }

    public int OneBasedRecordOrdinal { get; }
}

public sealed class OriginalMapEntityDefinition
{
    public const int OpaqueTailByteCount = 4;

    private readonly ReadOnlyCollection<byte> _opaqueTail;

    public OriginalMapEntityDefinition(
        OriginalMapEntityRecordIdentity identity,
        byte rawX,
        byte rawY,
        byte opaqueFacing,
        byte mapSprite,
        IEnumerable<byte> opaqueTail)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        ArgumentNullException.ThrowIfNull(opaqueTail);
        byte[] copiedTail = [.. opaqueTail.Take(OpaqueTailByteCount + 1)];
        if (copiedTail.Length != OpaqueTailByteCount)
        {
            throw new ArgumentException(
                $"An original map entity must retain exactly {OpaqueTailByteCount} opaque tail bytes.",
                nameof(opaqueTail));
        }

        RawX = rawX;
        RawY = rawY;
        OpaqueFacing = opaqueFacing;
        MapSprite = mapSprite;
        Position = new MapPosition(rawX & 0x3F, rawY & 0x3F);
        Kind = copiedTail[0] switch
        {
            0xFF => OriginalMapEntityRecordKind.Walking,
            0xFE => OriginalMapEntityRecordKind.Sequenced,
            _ => OriginalMapEntityRecordKind.Fixed,
        };
        _opaqueTail = Array.AsReadOnly(copiedTail);
    }

    public OriginalMapEntityRecordIdentity Identity { get; }

    public byte RawX { get; }

    public byte RawY { get; }

    public MapPosition Position { get; }

    public byte OpaqueFacing { get; }

    public byte MapSprite { get; }

    public OriginalMapEntityRecordKind Kind { get; }

    public IReadOnlyList<byte> OpaqueTail => _opaqueTail;
}

public sealed class OriginalMapEntityPopulation
{
    private readonly ReadOnlyCollection<OriginalMapEntityDefinition> _records;

    public OriginalMapEntityPopulation(
        MapId map,
        MapSetupId selectedSetup,
        IEnumerable<OriginalMapEntityDefinition> records)
        : this(map, selectedSetup, records, projectionDigestOverride: null)
    {
    }

    internal OriginalMapEntityPopulation(
        MapId map,
        MapSetupId selectedSetup,
        IEnumerable<OriginalMapEntityDefinition> records,
        string? projectionDigestOverride)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        SelectedSetup = selectedSetup ?? throw new ArgumentNullException(nameof(selectedSetup));
        ArgumentNullException.ThrowIfNull(records);

        List<OriginalMapEntityDefinition> copied = [];
        string? resourceId = null;
        foreach (OriginalMapEntityDefinition? record in records)
        {
            if (record is null)
            {
                throw new ArgumentException(
                    "An original map entity population cannot contain null records.",
                    nameof(records));
            }

            resourceId ??= record.Identity.ResourceId;
            if (!string.Equals(record.Identity.ResourceId, resourceId, StringComparison.Ordinal) ||
                record.Identity.OneBasedRecordOrdinal != copied.Count + 1)
            {
                throw new ArgumentException(
                    "Original map entities must retain one resource and contiguous source order.",
                    nameof(records));
            }

            copied.Add(new OriginalMapEntityDefinition(
                record.Identity,
                record.RawX,
                record.RawY,
                record.OpaqueFacing,
                record.MapSprite,
                record.OpaqueTail));
        }

        if (copied.Count == 0 || copied.Count > byte.MaxValue)
        {
            throw new ArgumentException(
                "An original map entity population requires one to 255 source records.",
                nameof(records));
        }

        _records = copied.AsReadOnly();
        ResourceId = resourceId!;
        if (projectionDigestOverride is null)
        {
            ProjectionDigest = ComputeProjectionDigest(_records);
        }
        else
        {
            OriginalMapImportRequest.ValidateSha256(
                projectionDigestOverride,
                nameof(projectionDigestOverride));
            ProjectionDigest = projectionDigestOverride.ToUpperInvariant();
        }
    }

    public MapId Map { get; }

    public MapSetupId SelectedSetup { get; }

    public string ResourceId { get; }

    public IReadOnlyList<OriginalMapEntityDefinition> Records => _records;

    public string ProjectionDigest { get; }

    private static string ComputeProjectionDigest(
        IReadOnlyCollection<OriginalMapEntityDefinition> records)
    {
        byte[] projection = new byte[1 + (records.Count * 8)];
        projection[0] = checked((byte)records.Count);
        int offset = 1;
        foreach (OriginalMapEntityDefinition record in records)
        {
            projection[offset++] = record.RawX;
            projection[offset++] = record.RawY;
            projection[offset++] = record.OpaqueFacing;
            projection[offset++] = record.MapSprite;
            foreach (byte value in record.OpaqueTail)
            {
                projection[offset++] = value;
            }
        }

        return Convert.ToHexString(SHA256.HashData(projection));
    }
}

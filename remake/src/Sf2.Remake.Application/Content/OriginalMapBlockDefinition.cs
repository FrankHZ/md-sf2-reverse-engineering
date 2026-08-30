using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Sf2.Remake.Domain.Maps;

[assembly: InternalsVisibleTo("Sf2.Remake.Application.Tests")]

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapBlockRecordIdentity
{
    public OriginalMapBlockRecordIdentity(string resourceId, int zeroBasedBlockIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentOutOfRangeException.ThrowIfNegative(zeroBasedBlockIndex);
        ResourceId = resourceId;
        ZeroBasedBlockIndex = zeroBasedBlockIndex;
    }

    public string ResourceId { get; }

    public int ZeroBasedBlockIndex { get; }
}

public sealed class OriginalMapBlockDefinition
{
    public const int OpaqueWordCount = 9;

    private readonly ReadOnlyCollection<ushort> _opaqueWords;

    public OriginalMapBlockDefinition(
        OriginalMapBlockRecordIdentity identity,
        IEnumerable<ushort> opaqueWords)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        ArgumentNullException.ThrowIfNull(opaqueWords);
        ushort[] copiedWords = [.. opaqueWords.Take(OpaqueWordCount + 1)];
        if (copiedWords.Length != OpaqueWordCount)
        {
            throw new ArgumentException(
                $"An original map block must contain exactly {OpaqueWordCount} opaque words.",
                nameof(opaqueWords));
        }

        _opaqueWords = Array.AsReadOnly(copiedWords);
    }

    public OriginalMapBlockRecordIdentity Identity { get; }

    public IReadOnlyList<ushort> OpaqueWords => _opaqueWords;
}

public sealed class OriginalMapBlockCatalog
{
    private readonly ReadOnlyCollection<OriginalMapBlockDefinition> _records;

    public OriginalMapBlockCatalog(IEnumerable<OriginalMapBlockDefinition> records)
        : this(records, projectionDigestOverride: null, useProjectionDigestOverride: false)
    {
    }

    internal OriginalMapBlockCatalog(
        IEnumerable<OriginalMapBlockDefinition> records,
        string projectionDigestOverride)
        : this(records, projectionDigestOverride, useProjectionDigestOverride: true)
    {
    }

    private OriginalMapBlockCatalog(
        IEnumerable<OriginalMapBlockDefinition> records,
        string? projectionDigestOverride,
        bool useProjectionDigestOverride)
    {
        ArgumentNullException.ThrowIfNull(records);
        List<OriginalMapBlockDefinition> copied = [];
        string? resourceId = null;
        foreach (OriginalMapBlockDefinition? record in records)
        {
            if (record is null)
            {
                throw new ArgumentException(
                    "An original map block catalog cannot contain null records.",
                    nameof(records));
            }

            resourceId ??= record.Identity.ResourceId;
            if (!string.Equals(
                    record.Identity.ResourceId,
                    resourceId,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Every original map block must retain the first record's resource identity.",
                    nameof(records));
            }

            if (record.Identity.ZeroBasedBlockIndex != copied.Count)
            {
                throw new ArgumentException(
                    "Original map block indices must be unique, ordered, and contiguous from zero.",
                    nameof(records));
            }

            copied.Add(new OriginalMapBlockDefinition(record.Identity, record.OpaqueWords));
        }

        if (copied.Count == 0)
        {
            throw new ArgumentException(
                "An original map block catalog cannot be empty.",
                nameof(records));
        }

        if (copied.Count > OriginalMapTraversal.LayoutBlockIndexMask + 1)
        {
            throw new ArgumentException(
                "An original map block catalog exceeds the accepted layout index domain.",
                nameof(records));
        }

        _records = copied.AsReadOnly();
        ResourceId = copied[0].Identity.ResourceId;
        if (!useProjectionDigestOverride)
        {
            ProjectionDigest = ComputeProjectionDigest(_records);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(projectionDigestOverride);
            OriginalMapImportRequest.ValidateSha256(
                projectionDigestOverride,
                nameof(projectionDigestOverride));
            ProjectionDigest = projectionDigestOverride.ToUpperInvariant();
        }
    }

    public string ResourceId { get; }

    public IReadOnlyList<OriginalMapBlockDefinition> Records => _records;

    public string ProjectionDigest { get; }

    public OriginalMapBlockDefinition Resolve(int zeroBasedBlockIndex)
    {
        if (zeroBasedBlockIndex < 0 || zeroBasedBlockIndex >= _records.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(zeroBasedBlockIndex));
        }

        return _records[zeroBasedBlockIndex];
    }

    public OriginalMapBlockDefinition Resolve(
        WorkingMapLayout layout,
        MapPosition position)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(position);
        int blockIndex = layout[position.X, position.Y] &
            OriginalMapTraversal.LayoutBlockIndexMask;
        return Resolve(blockIndex);
    }

    internal void ValidateLayoutReferences(
        WorkingMapLayout layout,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        for (int index = 0; index < layout.Words.Count; index++)
        {
            int blockIndex = layout.GetWord(index) & OriginalMapTraversal.LayoutBlockIndexMask;
            if (blockIndex >= _records.Count)
            {
                throw new ArgumentException(
                    $"Layout word {index} references missing block {blockIndex}.",
                    parameterName);
            }
        }
    }

    private static string ComputeProjectionDigest(
        IReadOnlyCollection<OriginalMapBlockDefinition> records)
    {
        byte[] projection = new byte[
            sizeof(ushort) +
            (records.Count * OriginalMapBlockDefinition.OpaqueWordCount * sizeof(ushort))];
        BinaryPrimitives.WriteUInt16BigEndian(projection, checked((ushort)records.Count));
        int offset = sizeof(ushort);
        foreach (OriginalMapBlockDefinition record in records)
        {
            foreach (ushort word in record.OpaqueWords)
            {
                BinaryPrimitives.WriteUInt16BigEndian(projection.AsSpan(offset), word);
                offset += sizeof(ushort);
            }
        }

        return Convert.ToHexString(SHA256.HashData(projection));
    }
}

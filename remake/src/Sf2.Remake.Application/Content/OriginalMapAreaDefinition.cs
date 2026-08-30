using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapAreaWordPair(ushort X, ushort Y);

public sealed record OriginalMapAreaBytePair(byte X, byte Y);

public sealed record OriginalMapAreaRecordIdentity
{
    public OriginalMapAreaRecordIdentity(string sourceResourceId, int oneBasedRecordOrdinal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceResourceId);
        ArgumentOutOfRangeException.ThrowIfLessThan(oneBasedRecordOrdinal, 1);
        SourceResourceId = sourceResourceId;
        OneBasedRecordOrdinal = oneBasedRecordOrdinal;
    }

    public string SourceResourceId { get; }

    public int OneBasedRecordOrdinal { get; }
}

public sealed record OriginalMapAreaDefinition
{
    public OriginalMapAreaDefinition(
        OriginalMapAreaRecordIdentity identity,
        OriginalMapTraversalArea mainLayerBounds,
        OriginalMapAreaWordPair secondLayerForegroundStart,
        OriginalMapAreaWordPair secondLayerBackgroundStart,
        OriginalMapAreaWordPair mainLayerParallax,
        OriginalMapAreaWordPair secondLayerParallax,
        OriginalMapAreaBytePair mainLayerAutoscroll,
        OriginalMapAreaBytePair secondLayerAutoscroll,
        byte mainLayerType,
        byte defaultMusic)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        MainLayerBounds = mainLayerBounds ??
            throw new ArgumentNullException(nameof(mainLayerBounds));
        SecondLayerForegroundStart = secondLayerForegroundStart ??
            throw new ArgumentNullException(nameof(secondLayerForegroundStart));
        SecondLayerBackgroundStart = secondLayerBackgroundStart ??
            throw new ArgumentNullException(nameof(secondLayerBackgroundStart));
        MainLayerParallax = mainLayerParallax ??
            throw new ArgumentNullException(nameof(mainLayerParallax));
        SecondLayerParallax = secondLayerParallax ??
            throw new ArgumentNullException(nameof(secondLayerParallax));
        MainLayerAutoscroll = mainLayerAutoscroll ??
            throw new ArgumentNullException(nameof(mainLayerAutoscroll));
        SecondLayerAutoscroll = secondLayerAutoscroll ??
            throw new ArgumentNullException(nameof(secondLayerAutoscroll));
        MainLayerType = mainLayerType;
        DefaultMusic = defaultMusic;
    }

    public OriginalMapAreaRecordIdentity Identity { get; }

    public OriginalMapTraversalArea MainLayerBounds { get; }

    public OriginalMapAreaWordPair SecondLayerForegroundStart { get; }

    public OriginalMapAreaWordPair SecondLayerBackgroundStart { get; }

    public OriginalMapAreaWordPair MainLayerParallax { get; }

    public OriginalMapAreaWordPair SecondLayerParallax { get; }

    public OriginalMapAreaBytePair MainLayerAutoscroll { get; }

    public OriginalMapAreaBytePair SecondLayerAutoscroll { get; }

    public byte MainLayerType { get; }

    public byte DefaultMusic { get; }
}

public sealed class OriginalMapAreaCatalog
{
    private readonly ReadOnlyCollection<OriginalMapAreaDefinition> _records;

    public OriginalMapAreaCatalog(IEnumerable<OriginalMapAreaDefinition> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        List<OriginalMapAreaDefinition> copied = [];
        string? resourceId = null;
        foreach (OriginalMapAreaDefinition record in records)
        {
            OriginalMapAreaDefinition admitted = record ?? throw new ArgumentException(
                "Original map area records cannot contain null values.",
                nameof(records));
            resourceId ??= admitted.Identity.SourceResourceId;
            if (!string.Equals(
                    admitted.Identity.SourceResourceId,
                    resourceId,
                    StringComparison.Ordinal) ||
                admitted.Identity.OneBasedRecordOrdinal != copied.Count + 1)
            {
                throw new ArgumentException(
                    "Original map area records must retain one resource and contiguous source order.",
                    nameof(records));
            }

            copied.Add(admitted);
        }

        if (copied.Count == 0)
        {
            throw new ArgumentException(
                "An original map area catalog requires at least one source record.",
                nameof(records));
        }

        _records = copied.AsReadOnly();
        ResourceId = resourceId!;
        Traversal = new OriginalMapTraversal(copied.Select(record => record.MainLayerBounds));
    }

    public string ResourceId { get; }

    public IReadOnlyList<OriginalMapAreaDefinition> Records => _records;

    public OriginalMapTraversal Traversal { get; }

    public OriginalMapAreaDefinition Resolve(OriginalMapTraversalAreaSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        int index = selection.OneBasedRecordOrdinal - 1;
        if (index < 0 || index >= _records.Count ||
            _records[index].MainLayerBounds != selection.Area)
        {
            throw new ArgumentException(
                "The traversal selection must identify one exact catalog source record.",
                nameof(selection));
        }

        return _records[index];
    }
}

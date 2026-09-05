using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed class OriginalMapExplorationRuntimeDefinition
{
    internal OriginalMapExplorationRuntimeDefinition(
        MapId map,
        WorkingMapLayout workingLayout,
        OriginalMapBlockCatalog blockCatalog,
        OriginalMapAreaCatalog areaCatalog,
        OriginalMapEntityPopulation entityPopulation,
        MapSetupId selectedSetup,
        string selectedInitIdentity)
        : this(
            map,
            workingLayout,
            blockCatalog,
            areaCatalog,
            entityPopulation,
            selectedSetup,
            selectedInitIdentity,
            ComputeWordDigest(workingLayout?.Words ??
                throw new ArgumentNullException(nameof(workingLayout))),
            ComputeCollisionProjectionDigest(workingLayout.Words))
    {
    }

    public OriginalMapExplorationRuntimeDefinition(
        MapId map,
        WorkingMapLayout workingLayout,
        OriginalMapBlockCatalog blockCatalog,
        OriginalMapAreaCatalog areaCatalog,
        OriginalMapEntityPopulation entityPopulation,
        MapSetupId selectedSetup,
        string selectedInitIdentity,
        string decodedLayoutDigest,
        string collisionProjectionDigest)
        : this(
            map,
            workingLayout,
            blockCatalog,
            areaCatalog,
            entityPopulation,
            selectedSetup,
            selectedInitIdentity,
            decodedLayoutDigest,
            collisionProjectionDigest,
            useProjectionDigestOverride: false)
    {
    }

    internal OriginalMapExplorationRuntimeDefinition(
        MapId map,
        WorkingMapLayout workingLayout,
        OriginalMapBlockCatalog blockCatalog,
        OriginalMapAreaCatalog areaCatalog,
        OriginalMapEntityPopulation entityPopulation,
        MapSetupId selectedSetup,
        string selectedInitIdentity,
        string decodedLayoutDigest,
        string collisionProjectionDigest,
        bool useProjectionDigestOverride)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        WorkingLayout = workingLayout ?? throw new ArgumentNullException(nameof(workingLayout));
        BlockCatalog = blockCatalog ?? throw new ArgumentNullException(nameof(blockCatalog));
        AreaCatalog = areaCatalog ?? throw new ArgumentNullException(nameof(areaCatalog));
        EntityPopulation = entityPopulation ??
            throw new ArgumentNullException(nameof(entityPopulation));
        SelectedSetup = selectedSetup ?? throw new ArgumentNullException(nameof(selectedSetup));
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedInitIdentity);
        OriginalMapImportRequest.ValidateSha256(
            decodedLayoutDigest,
            nameof(decodedLayoutDigest));
        OriginalMapImportRequest.ValidateSha256(
            collisionProjectionDigest,
            nameof(collisionProjectionDigest));
        string actualDecodedLayoutDigest = ComputeWordDigest(workingLayout.Words);
        if (!useProjectionDigestOverride && !string.Equals(
                decodedLayoutDigest,
                actualDecodedLayoutDigest,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The decoded-layout digest must match the immutable working layout.",
                nameof(decodedLayoutDigest));
        }

        string actualCollisionProjectionDigest =
            ComputeCollisionProjectionDigest(workingLayout.Words);
        if (!useProjectionDigestOverride && !string.Equals(
                collisionProjectionDigest,
                actualCollisionProjectionDigest,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The collision-projection digest must match the immutable working layout.",
                nameof(collisionProjectionDigest));
        }

        if (entityPopulation.Map != map || entityPopulation.SelectedSetup != selectedSetup)
        {
            throw new ArgumentException(
                "The runtime entity population must match its map and selected source setup.",
                nameof(entityPopulation));
        }

        blockCatalog.ValidateLayoutReferences(workingLayout, nameof(workingLayout));
        SelectedInitIdentity = selectedInitIdentity;
        DecodedLayoutDigest = useProjectionDigestOverride
            ? decodedLayoutDigest.ToUpperInvariant()
            : actualDecodedLayoutDigest;
        CollisionProjectionDigest = useProjectionDigestOverride
            ? collisionProjectionDigest.ToUpperInvariant()
            : actualCollisionProjectionDigest;
    }

    public MapId Map { get; }

    public WorkingMapLayout WorkingLayout { get; }

    public OriginalMapBlockCatalog BlockCatalog { get; }

    public OriginalMapAreaCatalog AreaCatalog { get; }

    public OriginalMapTraversal Traversal => AreaCatalog.Traversal;

    public OriginalMapEntityPopulation EntityPopulation { get; }

    public MapSetupId SelectedSetup { get; }

    public string SelectedInitIdentity { get; }

    public string DecodedLayoutDigest { get; }

    public string CollisionProjectionDigest { get; }

    private static string ComputeWordDigest(IEnumerable<ushort> words)
    {
        ushort[] copied = [.. words];
        byte[] bytes = new byte[copied.Length * sizeof(ushort)];
        for (int index = 0; index < copied.Length; index++)
        {
            bytes[index * 2] = checked((byte)(copied[index] >> 8));
            bytes[(index * 2) + 1] = checked((byte)(copied[index] & 0xFF));
        }

        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private static string ComputeCollisionProjectionDigest(IEnumerable<ushort> words) =>
        Convert.ToHexString(
            SHA256.HashData(
                words.Select(word =>
                        (byte)((word & OriginalMapTraversal.CollisionMask) ==
                            OriginalMapTraversal.CollisionMask
                            ? 1
                            : 0))
                    .ToArray()));
}

public sealed class OriginalMapExplorationRuntimeCatalog
{
    private readonly ReadOnlyCollection<OriginalMapExplorationRuntimeDefinition> _records;
    private readonly IReadOnlyDictionary<MapId, OriginalMapExplorationRuntimeDefinition> _byMap;

    public OriginalMapExplorationRuntimeCatalog(
        IEnumerable<OriginalMapExplorationRuntimeDefinition> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        List<OriginalMapExplorationRuntimeDefinition> copied = [];
        Dictionary<MapId, OriginalMapExplorationRuntimeDefinition> byMap = [];
        foreach (OriginalMapExplorationRuntimeDefinition? record in records)
        {
            if (record is null)
            {
                throw new ArgumentException(
                    "An original-map runtime catalog cannot contain null records.",
                    nameof(records));
            }

            if (!byMap.TryAdd(record.Map, record))
            {
                throw new ArgumentException(
                    $"Duplicate original-map runtime '{record.Map.Value}'.",
                    nameof(records));
            }

            copied.Add(record);
        }

        if (copied.Count == 0)
        {
            throw new ArgumentException(
                "An original-map runtime catalog requires at least one map.",
                nameof(records));
        }

        _records = copied.AsReadOnly();
        _byMap = new ReadOnlyDictionary<MapId, OriginalMapExplorationRuntimeDefinition>(byMap);
    }

    public IReadOnlyList<OriginalMapExplorationRuntimeDefinition> Records => _records;

    public OriginalMapExplorationRuntimeDefinition Resolve(MapId map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return _byMap.TryGetValue(map, out OriginalMapExplorationRuntimeDefinition? runtime)
            ? runtime
            : throw new KeyNotFoundException(
                $"Original-map runtime '{map.Value}' is not admitted.");
    }
}

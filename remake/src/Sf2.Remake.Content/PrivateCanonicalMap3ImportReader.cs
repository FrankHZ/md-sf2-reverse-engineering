using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

[assembly: InternalsVisibleTo("Sf2.Remake.Content.Tests")]

namespace Sf2.Remake.Content;

public sealed class PrivateCanonicalMap3ImportReader : IOriginalMapImportSource
{
    public const string PackageId = OriginalMapRuntimeAdmission.PackageId;
    public const string Capability = OriginalMapRuntimeAdmission.ImportCapability;
    public const string TraversalCapability = OriginalMapRuntimeAdmission.TraversalCapability;
    public const string ControlledAdmissionCapability =
        OriginalMapRuntimeAdmission.ControlledAdmissionCapability;
    public const string ControlledStepCopyCapability =
        OriginalMapRuntimeAdmission.ControlledStepCopyCapability;
    public const string CurrentAreaDiagnosticCapability =
        OriginalMapRuntimeAdmission.CurrentAreaDiagnosticCapability;
    public const string AreaSourceRecordAdmissionCapability =
        OriginalMapRuntimeAdmission.AreaSourceRecordAdmissionCapability;
    public const string SelectedSetupEntityPopulationCapability =
        OriginalMapRuntimeAdmission.SelectedSetupEntityPopulationCapability;
    public const string BlocksetSourceAdmissionCapability =
        OriginalMapRuntimeAdmission.BlocksetSourceAdmissionCapability;
    public const string VisualReferenceAdmissionCapability =
        OriginalMapRuntimeAdmission.VisualReferenceAdmissionCapability;
    public const string SameMapWarpAdmissionCapability =
        OriginalMapRuntimeAdmission.SameMapWarpAdmissionCapability;
    public const string RoofOnLoadClearCapability =
        OriginalMapRuntimeAdmission.RoofOnLoadClearCapability;
    public const string BowieDoorStepCopyCapability =
        OriginalMapRuntimeAdmission.BowieDoorStepCopyCapability;
    public const string SchoolDoorStepCopyCapability =
        OriginalMapRuntimeAdmission.SchoolDoorStepCopyCapability;
    public const string Zone601InterceptionCapability =
        OriginalMapRuntimeAdmission.Zone601InterceptionCapability;

    public const string CanonicalRepository =
        OriginalMapRuntimeAdmission.AcceptedUpstreamRepository;
    public const string CanonicalCommit = OriginalMapRuntimeAdmission.AcceptedUpstreamCommit;
    public const string CanonicalRomSha256 =
        OriginalMapRuntimeAdmission.AcceptedRomSha256;

    private const string AcceptedCanonicalContentDigest =
        OriginalMapRuntimeAdmission.AcceptedContentDigest;

    private const string Map3SourceIdentity = "Map03";
    private const string Map3SetupIdentity = OriginalMapRuntimeAdmission.SelectedSetupId;
    private const string Map3InitIdentity = OriginalMapRuntimeAdmission.SelectedInitIdentity;

    private static readonly string[] Capabilities =
    [
        Capability,
        TraversalCapability,
        ControlledAdmissionCapability,
        ControlledStepCopyCapability,
        CurrentAreaDiagnosticCapability,
        AreaSourceRecordAdmissionCapability,
        SelectedSetupEntityPopulationCapability,
        BlocksetSourceAdmissionCapability,
        VisualReferenceAdmissionCapability,
        SameMapWarpAdmissionCapability,
        RoofOnLoadClearCapability,
        BowieDoorStepCopyCapability,
        SchoolDoorStepCopyCapability,
        Zone601InterceptionCapability,
    ];

    private static readonly string[] UnsupportedCapabilities =
    [
        "natural-flags-setup-variant-selection",
        "natural-route-reach-order-and-continuity",
        "entity-occupancy-collision-and-obstruction",
        "other-warp-events-setup-init-effects-and-persistence",
        "original-assets-text-presentation-and-audio",
        "h3-h4-8c-private-fidelity",
    ];

    private static readonly string[] RootProperties =
    [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "geometry",
        "table",
        "summary",
        "resourceCounts",
        "recordCounts",
        "setupFacts",
        "referenceFacts",
        "maps",
        "resources",
        "runtimeQuestions",
    ];

    private static readonly string[] ResourceCollectionNames =
    [
        "blocksets",
        "layouts",
        "areaTables",
        "flagEventTables",
        "stepEventTables",
        "roofEventTables",
        "warpEventTables",
        "itemTables",
        "animationTables",
        "setupRoutes",
        "setupDefinitions",
        "entityLists",
        "entityEventHandlers",
        "zoneEventHandlers",
        "itemEventHandlers",
        "areaDescriptionHandlers",
        "initFunctions",
        "standaloneScriptPrograms",
        "initSourcePrograms",
    ];

    private static readonly string[] MapReferenceNames =
    [
        "blockset",
        "layout",
        "areaTable",
        "flagEventTable",
        "stepEventTable",
        "roofEventTable",
        "warpEventTable",
        "chestItemTable",
        "otherItemTable",
        "animationTable",
        "setupRoute",
    ];

    private readonly Func<byte[]> _loadDocument;

    public PrivateCanonicalMap3ImportReader(string canonicalImportPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalImportPath);
        string resolvedPath = Path.GetFullPath(canonicalImportPath);
        _loadDocument = () => File.ReadAllBytes(resolvedPath);
    }

    internal static OriginalMapImportResult AdmitSemanticDocumentForTests(
        IEnumerable<byte> documentBytes)
    {
        ArgumentNullException.ThrowIfNull(documentBytes);
        byte[] copied = [.. documentBytes];
        string contentDigest = Convert.ToHexString(SHA256.HashData(copied));
        return AdmitSemanticDocument(copied, contentDigest);
    }

    public OriginalMapImportResult Admit(OriginalMapImportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Profile != ContentProfile.PrivateLocal)
        {
            return Reject(
                OriginalMapImportFailureCode.ProfileMismatch,
                "profile",
                "The canonical original-map adapter admits only the PrivateLocal profile.");
        }

        if (!string.Equals(request.PackageId, PackageId, StringComparison.Ordinal))
        {
            return Reject(
                OriginalMapImportFailureCode.PackageIdentityMismatch,
                "packageId",
                "The canonical original-map adapter owns exactly one canonical import identity.");
        }

        byte[] documentBytes;
        try
        {
            documentBytes = _loadDocument();
        }
        catch (IOException error)
        {
            return Reject(
                OriginalMapImportFailureCode.PackageUnavailable,
                "package",
                $"The private canonical import is unavailable: {error.GetType().Name}.");
        }
        catch (UnauthorizedAccessException error)
        {
            return Reject(
                OriginalMapImportFailureCode.PackageUnavailable,
                "package",
                $"The private canonical import is unavailable: {error.GetType().Name}.");
        }

        string contentDigest = Convert.ToHexString(SHA256.HashData(documentBytes));
        if (!string.Equals(
                request.ExpectedContentDigest,
                AcceptedCanonicalContentDigest,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                contentDigest,
                AcceptedCanonicalContentDigest,
                StringComparison.OrdinalIgnoreCase))
        {
            return Reject(
                OriginalMapImportFailureCode.ContentDigestMismatch,
                "contentDigest",
                "The private canonical import bytes and caller pin must both match the accepted canonical identity.");
        }

        return AdmitSemanticDocument(documentBytes, contentDigest);
    }

    private static OriginalMapImportResult AdmitSemanticDocument(
        byte[] documentBytes,
        string contentDigest)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(
                documentBytes,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                });
            return BuildAccepted(document.RootElement, contentDigest);
        }
        catch (ImportAdmissionException error)
        {
            return Reject(error.Code, error.Field, error.Message);
        }
        catch (JsonException error)
        {
            return Reject(
                OriginalMapImportFailureCode.InvalidDocument,
                "document",
                $"The private canonical import JSON is invalid: {error.GetType().Name}.");
        }
        catch (Exception error) when (
            error is ArgumentException or ArithmeticException or InvalidOperationException or
                NullReferenceException)
        {
            return Reject(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3",
                $"The canonical Map 3 projection is invalid: {error.Message}");
        }
    }

    private static OriginalMapImportAccepted BuildAccepted(
        JsonElement root,
        string contentDigest)
    {
        RequireObject(root, "document");
        RequireExactProperties(root, "document", RootProperties);
        int schemaVersion = RequiredInt(root, "schemaVersion", "schemaVersion");
        if (schemaVersion != 1)
        {
            throw Admission(
                OriginalMapImportFailureCode.UnsupportedSchema,
                "schemaVersion",
                "Only canonical map import schema version 1 is supported.");
        }

        if (!string.Equals(RequiredString(root, "id", "id"), PackageId, StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.PackageIdentityMismatch,
                "id",
                "The canonical import document identity is not recognized.");
        }

        JsonElement upstream = RequiredProperty(root, "upstream", "upstream");
        RequireObject(upstream, "upstream");
        RequireExactProperties(upstream, "upstream", "repository", "commit");
        string repository = RequiredString(upstream, "repository", "upstream.repository");
        string commit = RequiredString(upstream, "commit", "upstream.commit");
        string romSha256 = RequiredString(root, "romSha256", "romSha256");
        if (!string.Equals(repository, CanonicalRepository, StringComparison.Ordinal) ||
            !string.Equals(commit, CanonicalCommit, StringComparison.Ordinal) ||
            !string.Equals(romSha256, CanonicalRomSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw Admission(
                OriginalMapImportFailureCode.ProvenanceMismatch,
                "provenance",
                "The canonical import provenance does not match the accepted US baseline.");
        }

        ValidateGeometry(RequiredProperty(root, "geometry", "geometry"));
        RequireObject(RequiredProperty(root, "table", "table"), "table");
        RequireObject(RequiredProperty(root, "summary", "summary"), "summary");
        RequireObject(
            RequiredProperty(root, "resourceCounts", "resourceCounts"),
            "resourceCounts");
        RequireObject(
            RequiredProperty(root, "recordCounts", "recordCounts"),
            "recordCounts");
        RequireObject(RequiredProperty(root, "setupFacts", "setupFacts"), "setupFacts");
        RequireObject(
            RequiredProperty(root, "referenceFacts", "referenceFacts"),
            "referenceFacts");
        RequireArray(
            RequiredProperty(root, "runtimeQuestions", "runtimeQuestions"),
            "runtimeQuestions");

        JsonElement map3 = SelectMap3(RequiredProperty(root, "maps", "maps"));
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        OriginalMapVisualResourceSelection visualResourceSelection =
            ReadVisualResourceSelection(map3, map);
        Dictionary<string, Dictionary<string, JsonElement>> resources = IndexResources(
            RequiredProperty(root, "resources", "resources"));
        JsonElement references = RequiredProperty(map3, "references", "maps[3].references");
        ValidateMap3References(references, resources);

        JsonElement blockset = RequiredResource(
            resources,
            "blocksets",
            RequiredString(references, "blockset", "maps[3].references.blockset"));
        OriginalMapBlockCatalog blockCatalog = ReadBlockset(blockset);
        JsonElement layoutResource = RequiredResource(
            resources,
            "layouts",
            RequiredString(references, "layout", "maps[3].references.layout"));
        ushort[] words = ReadLayout(layoutResource, blockCatalog.Records.Count);
        WorkingMapLayout workingLayout = new(words);

        JsonElement areaTable = RequiredResource(
            resources,
            "areaTables",
            RequiredString(references, "areaTable", "maps[3].references.areaTable"));
        OriginalMapAreaCatalog areaCatalog = ReadActiveAreas(areaTable);

        JsonElement warpTable = RequiredResource(
            resources,
            "warpEventTables",
            RequiredString(references, "warpEventTable", "maps[3].references.warpEventTable"));
        OriginalMapSameMapWarpCatalog sameMapWarps = ReadAcceptedSameMapWarps(warpTable);

        JsonElement roofTable = RequiredResource(
            resources,
            "roofEventTables",
            RequiredString(references, "roofEventTable", "maps[3].references.roofEventTable"));
        OriginalMapRoofOnLoadDefinition roofOnLoadClear = ReadAcceptedRoofOnLoadClear(
            roofTable,
            sameMapWarps,
            areaCatalog);

        JsonElement stepTable = RequiredResource(
            resources,
            "stepEventTables",
            RequiredString(references, "stepEventTable", "maps[3].references.stepEventTable"));
        (OriginalMapStepCopyDefinition controlledStepCopy,
            OriginalMapStepCopyDefinition bowieDoorStepCopy) =
            ReadAcceptedDoorSteps(stepTable, workingLayout);
        OriginalMapEntityPopulation entityPopulation =
            ReadControlledSetupPopulation(map, references, resources);
        OriginalMapZone601Definition zone601 = ReadAcceptedZone601(
            map,
            entityPopulation,
            resources);

        OriginalMapControlledAdmission controlledAdmission = new(
            map,
            new MapPosition(
                OriginalMapRuntimeAdmission.StartX,
                OriginalMapRuntimeAdmission.StartY),
            OriginalMapRuntimeAdmission.OpaqueStartFacing,
            new MapSetupId(Map3SetupIdentity),
            Map3InitIdentity,
            noProgramRequest: true);
        OriginalMapImportDefinition definition = new(
            map,
            workingLayout,
            blockCatalog,
            areaCatalog,
            entityPopulation,
            visualResourceSelection,
            controlledAdmission,
            controlledStepCopy,
            sameMapWarps,
            UnsupportedCapabilities,
            roofOnLoadClear,
            bowieDoorStepCopy,
            zone601);
        OriginalMapImportReceipt receipt = new(
            PackageId,
            schemaVersion,
            contentDigest,
            ComputeWordDigest(words),
            ComputeCollisionProjectionDigest(words),
            ContentProfile.PrivateLocal,
            new OriginalMapImportProvenance(
                PackageId,
                romSha256,
                repository,
                commit),
            OriginalMapRuntimeAdmission.RequiredEvidenceOwners,
            Capabilities);
        return new OriginalMapImportAccepted(definition, receipt);
    }

    private static void ValidateGeometry(JsonElement geometry)
    {
        RequireObject(geometry, "geometry");
        RequireExactProperties(
            geometry,
            "geometry",
            "layoutWidth",
            "layoutHeight",
            "blockWidthTiles",
            "blockHeightTiles",
            "rawWordBits",
            "layoutBlockIndexMask",
            "layoutFlagsMask");
        if (RequiredInt(geometry, "layoutWidth", "geometry.layoutWidth") !=
                WorkingMapLayout.ColumnCount ||
            RequiredInt(geometry, "layoutHeight", "geometry.layoutHeight") !=
                WorkingMapLayout.RowCount ||
            RequiredInt(geometry, "blockWidthTiles", "geometry.blockWidthTiles") != 3 ||
            RequiredInt(geometry, "blockHeightTiles", "geometry.blockHeightTiles") != 3 ||
            RequiredInt(geometry, "rawWordBits", "geometry.rawWordBits") != 16 ||
            RequiredInt(
                geometry,
                "layoutBlockIndexMask",
                "geometry.layoutBlockIndexMask") != OriginalMapTraversal.LayoutBlockIndexMask ||
            RequiredInt(
                geometry,
                "layoutFlagsMask",
                "geometry.layoutFlagsMask") != OriginalMapTraversal.LayoutFlagsMask ||
            !OriginalMapTraversal.StairWordDeltas.SequenceEqual([-63, 63, 65, -65]))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "geometry",
                "The canonical import geometry or accepted traversal constants drifted.");
        }
    }

    private static JsonElement SelectMap3(JsonElement maps)
    {
        RequireArray(maps, "maps");
        if (maps.GetArrayLength() != 79)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "maps",
                "The canonical map import must retain all 79 map identities.");
        }

        HashSet<int> mapIds = [];
        JsonElement? map3 = null;
        int index = 0;
        foreach (JsonElement map in maps.EnumerateArray())
        {
            string field = $"maps[{index}]";
            RequireObject(map, field);
            RequireExactProperties(map, field, "id", "sourceSymbol", "palette", "tilesets", "references");
            int mapId = RequiredInt(map, "id", field + ".id");
            if (mapId is < 0 or > 78 || !mapIds.Add(mapId))
            {
                throw Admission(
                    OriginalMapImportFailureCode.DuplicateIdentity,
                    field + ".id",
                    "Canonical map IDs must be the unique complete 0..78 domain.");
            }

            _ = RequiredString(map, "sourceSymbol", field + ".sourceSymbol");
            _ = RequiredByte(map, "palette", field + ".palette");
            JsonElement tilesets = RequiredProperty(map, "tilesets", field + ".tilesets");
            RequireArray(tilesets, field + ".tilesets");
            if (tilesets.GetArrayLength() != 5 ||
                tilesets.EnumerateArray().Any(tile => !TryByte(tile, out _)))
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    field + ".tilesets",
                    "Every canonical map must retain five byte-sized tileset identities.");
            }

            JsonElement references = RequiredProperty(map, "references", field + ".references");
            RequireObject(references, field + ".references");
            RequireExactProperties(references, field + ".references", MapReferenceNames);
            foreach (JsonProperty reference in references.EnumerateObject())
            {
                if (reference.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))
                {
                    throw Admission(
                        OriginalMapImportFailureCode.InvalidMapProjection,
                        field + ".references." + reference.Name,
                        "Map references must be a logical identity or an explicit absence.");
                }
            }

            if (mapId == 3)
            {
                map3 = map;
            }

            index++;
        }

        if (mapIds.Count != 79 || map3 is null)
        {
            throw Admission(
                OriginalMapImportFailureCode.MissingReference,
                "maps[3]",
                "The canonical import is missing the exact Map 3 entry.");
        }

        if (!string.Equals(
                RequiredString(map3.Value, "sourceSymbol", "maps[3].sourceSymbol"),
                Map3SourceIdentity,
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "maps[3].sourceSymbol",
                "Map 3 does not retain its accepted canonical identity.");
        }

        return map3.Value;
    }

    private static OriginalMapVisualResourceSelection ReadVisualResourceSelection(
        JsonElement map3,
        MapId map)
    {
        byte paletteIndex = RequiredByte(map3, "palette", "maps[3].palette");
        JsonElement tilesets = RequiredProperty(map3, "tilesets", "maps[3].tilesets");
        RequireArray(tilesets, "maps[3].tilesets");
        if (tilesets.GetArrayLength() != OriginalMapVisualResourceSelection.TilesetSlotCount)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "maps[3].tilesets",
                "Map 3 must retain exactly five ordered tileset references.");
        }

        byte[] slots = new byte[OriginalMapVisualResourceSelection.TilesetSlotCount];
        int index = 0;
        foreach (JsonElement slot in tilesets.EnumerateArray())
        {
            if (!TryByte(slot, out byte value))
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    $"maps[3].tilesets[{index}]",
                    "Map 3 tileset references must remain byte-sized identities.");
            }

            slots[index++] = value;
        }

        OriginalMapVisualResourceSelection selection = new(map, paletteIndex, slots);
        if (!OriginalMapRuntimeAdmission.HasExactAcceptedVisualResourceSelection(selection))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "maps[3].visualResourceSelection",
                "Map 3 does not retain the accepted palette and ordered tileset reference projection.");
        }

        return selection;
    }

    private static Dictionary<string, Dictionary<string, JsonElement>> IndexResources(
        JsonElement resources)
    {
        RequireObject(resources, "resources");
        RequireExactProperties(resources, "resources", ResourceCollectionNames);
        Dictionary<string, Dictionary<string, JsonElement>> indexed =
            new(StringComparer.Ordinal);
        foreach (string collectionName in ResourceCollectionNames)
        {
            JsonElement collection = RequiredProperty(
                resources,
                collectionName,
                "resources." + collectionName);
            RequireArray(collection, "resources." + collectionName);
            Dictionary<string, JsonElement> byId = new(StringComparer.Ordinal);
            int index = 0;
            foreach (JsonElement resource in collection.EnumerateArray())
            {
                string field = $"resources.{collectionName}[{index}]";
                RequireObject(resource, field);
                string id = RequiredString(resource, "id", field + ".id");
                if (!byId.TryAdd(id, resource))
                {
                    throw Admission(
                        OriginalMapImportFailureCode.DuplicateIdentity,
                        field + ".id",
                        $"Duplicate canonical resource identity '{id}'.");
                }

                index++;
            }

            indexed.Add(collectionName, byId);
        }

        return indexed;
    }

    private static void ValidateMap3References(
        JsonElement references,
        IReadOnlyDictionary<string, Dictionary<string, JsonElement>> resources)
    {
        (string Reference, string Collection)[] joins =
        [
            ("blockset", "blocksets"),
            ("layout", "layouts"),
            ("areaTable", "areaTables"),
            ("flagEventTable", "flagEventTables"),
            ("stepEventTable", "stepEventTables"),
            ("roofEventTable", "roofEventTables"),
            ("warpEventTable", "warpEventTables"),
            ("chestItemTable", "itemTables"),
            ("otherItemTable", "itemTables"),
            ("animationTable", "animationTables"),
            ("setupRoute", "setupRoutes"),
        ];
        foreach ((string reference, string collection) in joins)
        {
            string id = RequiredString(
                references,
                reference,
                "maps[3].references." + reference);
            _ = RequiredResource(resources, collection, id);
        }
    }

    private static OriginalMapBlockCatalog ReadBlockset(JsonElement resource)
    {
        RequireExactProperties(resource, "map3.blockset", "id", "address", "blocks");
        string resourceId = RequiredString(resource, "id", "map3.blockset.id");
        if (!string.Equals(
                resourceId,
                OriginalMapRuntimeAdmission.AcceptedBlocksetResourceId,
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.blockset.id",
                "Map 3 does not retain the accepted canonical blockset identity.");
        }

        _ = RequiredNonNegativeInt(resource, "address", "map3.blockset.address");
        JsonElement blocks = RequiredProperty(resource, "blocks", "map3.blockset.blocks");
        RequireArray(blocks, "map3.blockset.blocks");
        if (blocks.GetArrayLength() < 3)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.blockset.blocks",
                "The canonical blockset must retain the three built-in blocks.");
        }

        List<OriginalMapBlockDefinition> definitions = [];
        int blockIndex = 0;
        foreach (JsonElement block in blocks.EnumerateArray())
        {
            RequireArray(block, $"map3.blockset.blocks[{blockIndex}]");
            if (block.GetArrayLength() != OriginalMapBlockDefinition.OpaqueWordCount)
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    $"map3.blockset.blocks[{blockIndex}]",
                    "Every canonical map block must retain exactly nine ushort tile words.");
            }

            ushort[] opaqueWords = new ushort[OriginalMapBlockDefinition.OpaqueWordCount];
            int wordIndex = 0;
            foreach (JsonElement value in block.EnumerateArray())
            {
                if (!TryUshort(value, out ushort word))
                {
                    throw Admission(
                        OriginalMapImportFailureCode.InvalidMapProjection,
                        $"map3.blockset.blocks[{blockIndex}][{wordIndex}]",
                        "Canonical block words must remain unsigned 16-bit values.");
                }

                opaqueWords[wordIndex++] = word;
            }

            definitions.Add(new OriginalMapBlockDefinition(
                new OriginalMapBlockRecordIdentity(resourceId, blockIndex),
                opaqueWords));
            blockIndex++;
        }

        return new OriginalMapBlockCatalog(definitions);
    }

    private static OriginalMapSameMapWarpCatalog ReadAcceptedSameMapWarps(
        JsonElement resource)
    {
        const string resourceField = "map3.warpEventTable";
        RequireExactProperties(
            resource,
            resourceField,
            "id",
            "address",
            "sourceKind",
            "records");
        string resourceId = RequiredString(resource, "id", resourceField + ".id");
        if (!string.Equals(
                resourceId,
                OriginalMapRuntimeAdmission.SameMapWarpResourceId,
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                resourceField + ".id",
                "Map 3 does not retain the accepted warp-event resource identity.");
        }

        _ = RequiredNonNegativeInt(resource, "address", resourceField + ".address");
        if (!string.Equals(
                RequiredString(resource, "sourceKind", resourceField + ".sourceKind"),
                "warpEvents",
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                resourceField + ".sourceKind",
                "Map 3 does not retain the accepted warp-event source kind.");
        }

        JsonElement records = RequiredProperty(resource, "records", resourceField + ".records");
        RequireArray(records, resourceField + ".records");
        if (records.GetArrayLength() != OriginalMapRuntimeAdmission.SameMapWarpSourceRecordCount)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                resourceField + ".records",
                "Map 3 must retain its exact source warp-record count.");
        }

        List<OriginalMapSameMapWarpDefinition> admitted = [];
        int zeroBasedOrdinal = 0;
        foreach (JsonElement record in records.EnumerateArray())
        {
            string field = $"{resourceField}.records[{zeroBasedOrdinal}]";
            RequireObject(record, field);
            RequireExactProperties(
                record,
                field,
                "trigger",
                "scrollMode",
                "retainsCoordinates",
                "scrollDirection",
                "targetMap",
                "destination",
                "facing",
                "reserved");
            (int triggerX, int triggerY) = ReadPoint(
                RequiredProperty(record, "trigger", field + ".trigger"),
                field + ".trigger",
                byteSized: true);
            byte scrollMode = RequiredByte(record, "scrollMode", field + ".scrollMode");
            bool retainsCoordinates = RequiredBoolean(
                record,
                "retainsCoordinates",
                field + ".retainsCoordinates");
            JsonElement scrollDirection = RequiredProperty(
                record,
                "scrollDirection",
                field + ".scrollDirection");
            if (scrollDirection.ValueKind is not JsonValueKind.Null &&
                !TryByte(scrollDirection, out _))
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    field + ".scrollDirection",
                    "A canonical warp scroll direction must be null or a byte.");
            }

            byte targetMap = RequiredByte(record, "targetMap", field + ".targetMap");
            (int destinationX, int destinationY) = ReadPoint(
                RequiredProperty(record, "destination", field + ".destination"),
                field + ".destination",
                byteSized: true);
            byte facing = RequiredByte(record, "facing", field + ".facing");
            byte reserved = RequiredByte(record, "reserved", field + ".reserved");

            int oneBasedOrdinal = zeroBasedOrdinal + 1;
            if (oneBasedOrdinal is OriginalMapRuntimeAdmission.SchoolWarpRecordOrdinal or
                OriginalMapRuntimeAdmission.HouseWarpRecordOrdinal)
            {
                if (scrollMode != 0 || retainsCoordinates ||
                    scrollDirection.ValueKind is not JsonValueKind.Null ||
                    targetMap != byte.MaxValue || reserved != 0)
                {
                    throw Admission(
                        OriginalMapImportFailureCode.InvalidMapProjection,
                        field,
                        "The accepted Map 3 egress warp must remain a no-scroll current-map relocation.");
                }

                admitted.Add(
                    new OriginalMapSameMapWarpDefinition(
                        new OriginalMapSameMapWarpIdentity(
                            ContentProfile.PrivateLocal,
                            new MapId(OriginalMapRuntimeAdmission.MapId),
                            resourceId,
                            oneBasedOrdinal),
                        new MapPosition(triggerX, triggerY),
                        new MapPosition(destinationX, destinationY),
                        facing));
            }

            zeroBasedOrdinal++;
        }

        OriginalMapSameMapWarpCatalog catalog = new(admitted);
        if (!OriginalMapRuntimeAdmission.HasExactAcceptedSameMapWarps(catalog))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                resourceField,
                "The accepted Map 3 same-map warp projection drifted.");
        }

        return catalog;
    }

    private static OriginalMapRoofOnLoadDefinition ReadAcceptedRoofOnLoadClear(
        JsonElement resource,
        OriginalMapSameMapWarpCatalog sameMapWarps,
        OriginalMapAreaCatalog areaCatalog)
    {
        const string resourceField = "map3.roofEventTable";
        RequireExactProperties(
            resource,
            resourceField,
            "id",
            "address",
            "sourceKind",
            "records");
        string resourceId = RequiredString(resource, "id", resourceField + ".id");
        if (!string.Equals(
                resourceId,
                OriginalMapRuntimeAdmission.RoofOnLoadResourceId,
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                resourceField + ".id",
                "Map 3 does not retain the accepted roof-event resource identity.");
        }

        _ = RequiredNonNegativeInt(resource, "address", resourceField + ".address");
        if (!string.Equals(
                RequiredString(resource, "sourceKind", resourceField + ".sourceKind"),
                "roofEvents",
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                resourceField + ".sourceKind",
                "Map 3 does not retain the accepted roof-event source kind.");
        }

        JsonElement records = RequiredProperty(resource, "records", resourceField + ".records");
        RequireArray(records, resourceField + ".records");
        if (records.GetArrayLength() != OriginalMapRuntimeAdmission.RoofOnLoadSourceRecordCount)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                resourceField + ".records",
                "Map 3 must retain its exact source roof-record count.");
        }

        OriginalMapRoofOnLoadDefinition? selected = null;
        int zeroBasedOrdinal = 0;
        foreach (JsonElement record in records.EnumerateArray())
        {
            string field = $"{resourceField}.records[{zeroBasedOrdinal}]";
            RequireObject(record, field);
            RequireExactProperties(record, field, "trigger", "source", "size", "destination");
            (int triggerX, int triggerY) = ReadPoint(
                RequiredProperty(record, "trigger", field + ".trigger"),
                field + ".trigger",
                byteSized: true);
            (int sourceX, int sourceY) = ReadPoint(
                RequiredProperty(record, "source", field + ".source"),
                field + ".source",
                byteSized: true);
            (int width, int height) = ReadSize(
                RequiredProperty(record, "size", field + ".size"),
                field + ".size");
            (int destinationX, int destinationY) = ReadPoint(
                RequiredProperty(record, "destination", field + ".destination"),
                field + ".destination",
                byteSized: true);
            int oneBasedOrdinal = zeroBasedOrdinal + 1;
            if (oneBasedOrdinal == OriginalMapRuntimeAdmission.HouseRoofOnLoadRecordOrdinal)
            {
                if (triggerX != OriginalMapRuntimeAdmission.HouseRoofSourceTriggerX ||
                    triggerY != OriginalMapRuntimeAdmission.HouseRoofSourceTriggerY ||
                    sourceX != byte.MaxValue ||
                    sourceY != byte.MaxValue ||
                    width != OriginalMapRuntimeAdmission.HouseRoofClearWidth ||
                    height != OriginalMapRuntimeAdmission.HouseRoofClearHeight ||
                    destinationX != OriginalMapRuntimeAdmission.HouseRoofClearDestinationX ||
                    destinationY != OriginalMapRuntimeAdmission.HouseRoofClearDestinationY)
                {
                    throw Admission(
                        OriginalMapImportFailureCode.InvalidMapProjection,
                        field,
                        "The accepted Bowie-house roof-on-load clear record drifted.");
                }

                OriginalMapSameMapWarpDefinition appliedAfterWarp = sameMapWarps.Records.Single(
                    warp => warp.Identity.OneBasedRecordOrdinal ==
                        OriginalMapRuntimeAdmission.HouseWarpRecordOrdinal);
                OriginalMapAreaDefinition destinationArea = areaCatalog.Records[
                    OriginalMapRuntimeAdmission.HouseRoofDestinationAreaOrdinal - 1];
                selected = new OriginalMapRoofOnLoadDefinition(
                    new OriginalMapRoofOnLoadIdentity(
                        ContentProfile.PrivateLocal,
                        new MapId(OriginalMapRuntimeAdmission.MapId),
                        resourceId,
                        oneBasedOrdinal),
                    new MapPosition(triggerX, triggerY),
                    new MapPosition(destinationX, destinationY),
                    width,
                    height,
                    appliedAfterWarp.Identity,
                    destinationArea.Identity);
            }

            zeroBasedOrdinal++;
        }

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedRoofOnLoadClear(selected))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                resourceField,
                "The accepted Map 3 roof-on-load clear projection drifted.");
        }

        return selected!;
    }

    private static ushort[] ReadLayout(JsonElement resource, int blockCount)
    {
        RequireExactProperties(resource, "map3.layout", "id", "address", "width", "height", "words");
        _ = RequiredNonNegativeInt(resource, "address", "map3.layout.address");
        if (RequiredInt(resource, "width", "map3.layout.width") != WorkingMapLayout.ColumnCount ||
            RequiredInt(resource, "height", "map3.layout.height") != WorkingMapLayout.RowCount)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.layout",
                "Map 3 must retain the exact 64x64 canonical layout.");
        }

        JsonElement wordsElement = RequiredProperty(resource, "words", "map3.layout.words");
        RequireArray(wordsElement, "map3.layout.words");
        if (wordsElement.GetArrayLength() != WorkingMapLayout.WordCount)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.layout.words",
                "Map 3 must retain exactly 4096 opaque layout words.");
        }

        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        int index = 0;
        foreach (JsonElement value in wordsElement.EnumerateArray())
        {
            if (!TryUshort(value, out ushort word))
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    $"map3.layout.words[{index}]",
                    "Canonical layout words must remain unsigned 16-bit values.");
            }

            if ((word & OriginalMapTraversal.LayoutBlockIndexMask) >= blockCount)
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    $"map3.layout.words[{index}]",
                    "A canonical layout word references a missing block.");
            }

            words[index++] = word;
        }

        return words;
    }

    private static OriginalMapAreaCatalog ReadActiveAreas(JsonElement resource)
    {
        RequireExactProperties(resource, "map3.areaTable", "id", "address", "sourceKind", "records");
        string resourceId = RequiredString(resource, "id", "map3.areaTable.id");
        if (!string.Equals(
                resourceId,
                OriginalMapRuntimeAdmission.AcceptedAreaResourceId,
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.areaTable.id",
                "The selected Map 3 area resource does not retain the accepted identity.");
        }

        _ = RequiredNonNegativeInt(resource, "address", "map3.areaTable.address");
        if (!string.Equals(
                RequiredString(resource, "sourceKind", "map3.areaTable.sourceKind"),
                "areas",
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.areaTable.sourceKind",
                "The selected Map 3 area resource has the wrong source kind.");
        }

        JsonElement records = RequiredProperty(resource, "records", "map3.areaTable.records");
        RequireArray(records, "map3.areaTable.records");
        List<OriginalMapAreaDefinition> areas = [];
        int index = 0;
        foreach (JsonElement record in records.EnumerateArray())
        {
            string field = $"map3.areaTable.records[{index}]";
            RequireObject(record, field);
            RequireExactProperties(
                record,
                field,
                "mainLayerStart",
                "mainLayerEnd",
                "secondLayerForegroundStart",
                "secondLayerBackgroundStart",
                "mainLayerParallax",
                "secondLayerParallax",
                "mainLayerAutoscroll",
                "secondLayerAutoscroll",
                "mainLayerType",
                "defaultMusic");
            (int minimumX, int minimumY) = ReadPoint(
                RequiredProperty(record, "mainLayerStart", field + ".mainLayerStart"),
                field + ".mainLayerStart",
                byteSized: false);
            (int maximumX, int maximumY) = ReadPoint(
                RequiredProperty(record, "mainLayerEnd", field + ".mainLayerEnd"),
                field + ".mainLayerEnd",
                byteSized: false);
            (int secondForegroundX, int secondForegroundY) = ReadPoint(
                RequiredProperty(
                    record,
                    "secondLayerForegroundStart",
                    field + ".secondLayerForegroundStart"),
                field + ".secondLayerForegroundStart",
                byteSized: false);
            (int secondBackgroundX, int secondBackgroundY) = ReadPoint(
                RequiredProperty(
                    record,
                    "secondLayerBackgroundStart",
                    field + ".secondLayerBackgroundStart"),
                field + ".secondLayerBackgroundStart",
                byteSized: false);
            (int mainParallaxX, int mainParallaxY) = ReadPoint(
                RequiredProperty(
                    record,
                    "mainLayerParallax",
                    field + ".mainLayerParallax"),
                field + ".mainLayerParallax",
                byteSized: false);
            (int secondParallaxX, int secondParallaxY) = ReadPoint(
                RequiredProperty(
                    record,
                    "secondLayerParallax",
                    field + ".secondLayerParallax"),
                field + ".secondLayerParallax",
                byteSized: false);
            (int mainAutoscrollX, int mainAutoscrollY) = ReadPoint(
                RequiredProperty(
                    record,
                    "mainLayerAutoscroll",
                    field + ".mainLayerAutoscroll"),
                field + ".mainLayerAutoscroll",
                byteSized: true);
            (int secondAutoscrollX, int secondAutoscrollY) = ReadPoint(
                RequiredProperty(
                    record,
                    "secondLayerAutoscroll",
                    field + ".secondLayerAutoscroll"),
                field + ".secondLayerAutoscroll",
                byteSized: true);
            byte mainLayerType = RequiredByte(
                record,
                "mainLayerType",
                field + ".mainLayerType");
            byte defaultMusic = RequiredByte(
                record,
                "defaultMusic",
                field + ".defaultMusic");
            if (minimumX >= WorkingMapLayout.ColumnCount ||
                minimumY >= WorkingMapLayout.RowCount ||
                maximumX >= WorkingMapLayout.ColumnCount ||
                maximumY >= WorkingMapLayout.RowCount)
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    field,
                    "Map 3 active-area bounds must fit the 64x64 working layout.");
            }

            areas.Add(new OriginalMapAreaDefinition(
                new OriginalMapAreaRecordIdentity(resourceId, index + 1),
                new OriginalMapTraversalArea(minimumX, minimumY, maximumX, maximumY),
                new OriginalMapAreaWordPair(
                    checked((ushort)secondForegroundX),
                    checked((ushort)secondForegroundY)),
                new OriginalMapAreaWordPair(
                    checked((ushort)secondBackgroundX),
                    checked((ushort)secondBackgroundY)),
                new OriginalMapAreaWordPair(
                    checked((ushort)mainParallaxX),
                    checked((ushort)mainParallaxY)),
                new OriginalMapAreaWordPair(
                    checked((ushort)secondParallaxX),
                    checked((ushort)secondParallaxY)),
                new OriginalMapAreaBytePair(
                    checked((byte)mainAutoscrollX),
                    checked((byte)mainAutoscrollY)),
                new OriginalMapAreaBytePair(
                    checked((byte)secondAutoscrollX),
                    checked((byte)secondAutoscrollY)),
                mainLayerType,
                defaultMusic));
            index++;
        }

        if (areas.Count != OriginalMapRuntimeAdmission.AcceptedAreaRecordCount)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.areaTable.records",
                "Map 3 requires the exact accepted ordered area-record count.");
        }

        OriginalMapAreaCatalog catalog = new(areas);
        if (!OriginalMapRuntimeAdmission.HasExactAcceptedAreaProjection(catalog.Traversal) ||
            !OriginalMapRuntimeAdmission.HasExactAcceptedAreaSourceProjection(catalog))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.areaTable.records",
                "The selected Map 3 area resource does not retain the accepted ordered full-record projection.");
        }

        return catalog;
    }

    private static (
        OriginalMapStepCopyDefinition Controlled,
        OriginalMapStepCopyDefinition Bowie) ReadAcceptedDoorSteps(
        JsonElement resource,
        WorkingMapLayout workingLayout)
    {
        RequireExactProperties(resource, "map3.stepEvents", "id", "address", "sourceKind", "records");
        string resourceId = RequiredString(resource, "id", "map3.stepEvents.id");
        if (!string.Equals(
                resourceId,
                OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.stepEvents.id",
                "The selected Map 3 step resource has the wrong identity.");
        }

        _ = RequiredNonNegativeInt(resource, "address", "map3.stepEvents.address");
        if (!string.Equals(
                RequiredString(resource, "sourceKind", "map3.stepEvents.sourceKind"),
                "stepEvents",
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.stepEvents.sourceKind",
                "The selected Map 3 step resource has the wrong source kind.");
        }

        JsonElement records = RequiredProperty(resource, "records", "map3.stepEvents.records");
        RequireArray(records, "map3.stepEvents.records");
        if (records.GetArrayLength() != OriginalMapRuntimeAdmission.StepCopySourceRecordCount)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.stepEvents.records",
                "Map 3 requires the exact accepted step-copy source-record count.");
        }

        OriginalMapStepCopyDefinition? controlled = null;
        OriginalMapStepCopyDefinition? bowie = null;
        int matchingSchoolDoorRecords = 0;
        int matchingBowieDoorRecords = 0;
        int index = 0;
        foreach (JsonElement record in records.EnumerateArray())
        {
            string field = $"map3.stepEvents.records[{index}]";
            RequireObject(record, field);
            RequireExactProperties(record, field, "trigger", "source", "size", "destination");
            (int triggerX, int triggerY) = ReadPoint(
                RequiredProperty(record, "trigger", field + ".trigger"),
                field + ".trigger",
                byteSized: true);
            (int sourceX, int sourceY) = ReadPoint(
                RequiredProperty(record, "source", field + ".source"),
                field + ".source",
                byteSized: true);
            (int width, int height) = ReadSize(
                RequiredProperty(record, "size", field + ".size"),
                field + ".size");
            (int destinationX, int destinationY) = ReadPoint(
                RequiredProperty(record, "destination", field + ".destination"),
                field + ".destination",
                byteSized: true);
            bool isExactSchoolDoor =
                triggerX == OriginalMapRuntimeAdmission.ControlledStepCopyTriggerX &&
                triggerY == OriginalMapRuntimeAdmission.ControlledStepCopyTriggerY &&
                sourceX == OriginalMapRuntimeAdmission.ControlledStepCopySourceX &&
                sourceY == OriginalMapRuntimeAdmission.ControlledStepCopySourceY &&
                width == OriginalMapRuntimeAdmission.ControlledStepCopyWidth &&
                height == OriginalMapRuntimeAdmission.ControlledStepCopyHeight &&
                destinationX == OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX &&
                destinationY == OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY;
            bool isExactBowieDoor =
                triggerX == OriginalMapRuntimeAdmission.BowieDoorStepCopyTriggerX &&
                triggerY == OriginalMapRuntimeAdmission.BowieDoorStepCopyTriggerY &&
                sourceX == OriginalMapRuntimeAdmission.BowieDoorStepCopySourceX &&
                sourceY == OriginalMapRuntimeAdmission.BowieDoorStepCopySourceY &&
                width == OriginalMapRuntimeAdmission.BowieDoorStepCopyWidth &&
                height == OriginalMapRuntimeAdmission.BowieDoorStepCopyHeight &&
                destinationX == OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationX &&
                destinationY == OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationY;
            if (isExactSchoolDoor)
            {
                matchingSchoolDoorRecords++;
            }

            if (isExactBowieDoor)
            {
                matchingBowieDoorRecords++;
            }

            int oneBasedOrdinal = index + 1;
            if (oneBasedOrdinal == OriginalMapRuntimeAdmission.ControlledStepCopyRecordOrdinal)
            {
                if (!isExactSchoolDoor)
                {
                    throw Admission(
                        OriginalMapImportFailureCode.InvalidMapProjection,
                        field,
                        "The admitted Map 3 controlled step-copy record drifted.");
                }

                controlled = new OriginalMapStepCopyDefinition(
                    new OriginalMapStepCopyIdentity(
                        ContentProfile.PrivateLocal,
                        new MapId(OriginalMapRuntimeAdmission.MapId),
                        resourceId,
                        oneBasedOrdinal),
                    new MapPosition(triggerX, triggerY),
                    new WorkingMapBlockCopy(
                        sourceX,
                        sourceY,
                        destinationX,
                        destinationY,
                        width,
                        height));
            }
            else if (oneBasedOrdinal ==
                OriginalMapRuntimeAdmission.BowieDoorStepCopyRecordOrdinal)
            {
                if (!isExactBowieDoor)
                {
                    throw Admission(
                        OriginalMapImportFailureCode.InvalidMapProjection,
                        field,
                        "The admitted Map 3 Bowie-door step-copy record drifted.");
                }

                bowie = new OriginalMapStepCopyDefinition(
                    new OriginalMapStepCopyIdentity(
                        ContentProfile.PrivateLocal,
                        new MapId(OriginalMapRuntimeAdmission.MapId),
                        resourceId,
                        oneBasedOrdinal),
                    new MapPosition(triggerX, triggerY),
                    new WorkingMapBlockCopy(
                        sourceX,
                        sourceY,
                        destinationX,
                        destinationY,
                        width,
                        height));
            }

            index++;
        }

        if (controlled is null ||
            bowie is null ||
            matchingSchoolDoorRecords != 1 ||
            matchingBowieDoorRecords != 1 ||
            !OriginalMapTraversal.IsBlocked(
                workingLayout,
                new MapPosition(
                    OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
                    OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY)) ||
            !OriginalMapTraversal.IsBlocked(
                workingLayout,
                new MapPosition(
                    OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationX,
                    OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationY)) ||
            OriginalMapTraversal.IsBlocked(
                workingLayout,
                new MapPosition(
                    OriginalMapRuntimeAdmission.ControlledStepCopySourceX,
                    OriginalMapRuntimeAdmission.ControlledStepCopySourceY)) ||
            OriginalMapTraversal.IsBlocked(
                workingLayout,
                new MapPosition(
                    OriginalMapRuntimeAdmission.BowieDoorStepCopySourceX,
                    OriginalMapRuntimeAdmission.BowieDoorStepCopySourceY)))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.stepEvents.doors",
                "The canonical Map 3 Bowie/school door copies or collision polarity drifted.");
        }

        return (controlled, bowie);
    }

    private static OriginalMapEntityPopulation ReadControlledSetupPopulation(
        MapId map,
        JsonElement references,
        IReadOnlyDictionary<string, Dictionary<string, JsonElement>> resources)
    {
        string routeId = RequiredString(
            references,
            "setupRoute",
            "maps[3].references.setupRoute");
        JsonElement route = RequiredResource(resources, "setupRoutes", routeId);
        RequireExactProperties(route, "map3.setupRoute", "id", "map", "defaultSetup", "flagVariants");
        if (RequiredInt(route, "map", "map3.setupRoute.map") != 3 ||
            !string.Equals(
                RequiredString(route, "defaultSetup", "map3.setupRoute.defaultSetup"),
                Map3SetupIdentity,
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.setupRoute",
                "Map 3 must retain the accepted controlled default setup identity.");
        }

        JsonElement variants = RequiredProperty(route, "flagVariants", "map3.setupRoute.flagVariants");
        RequireArray(variants, "map3.setupRoute.flagVariants");
        if (variants.GetArrayLength() != 3)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.setupRoute.flagVariants",
                "Map 3 must retain its three unsupported natural setup variants.");
        }

        HashSet<int> flags = [];
        foreach (JsonElement variant in variants.EnumerateArray())
        {
            RequireObject(variant, "map3.setupRoute.flagVariants[]");
            RequireExactProperties(
                variant,
                "map3.setupRoute.flagVariants[]",
                "flag",
                "setup");
            int flag = RequiredInt(variant, "flag", "map3.setupRoute.flagVariants[].flag");
            string setup = RequiredString(
                variant,
                "setup",
                "map3.setupRoute.flagVariants[].setup");
            if (flag is < 0 or > ushort.MaxValue || !flags.Add(flag))
            {
                throw Admission(
                    OriginalMapImportFailureCode.DuplicateIdentity,
                    "map3.setupRoute.flagVariants[].flag",
                    "Map 3 setup-variant flags must remain unique ushort identities.");
            }

            _ = RequiredResource(resources, "setupDefinitions", setup);
        }

        JsonElement setupDefinition = RequiredResource(
            resources,
            "setupDefinitions",
            Map3SetupIdentity);
        RequireExactProperties(setupDefinition, "map3.setup", "id", "address", "references");
        _ = RequiredNonNegativeInt(setupDefinition, "address", "map3.setup.address");
        JsonElement setupReferences = RequiredProperty(
            setupDefinition,
            "references",
            "map3.setup.references");
        RequireObject(setupReferences, "map3.setup.references");
        RequireExactProperties(
            setupReferences,
            "map3.setup.references",
            "entities",
            "entityEvents",
            "zoneEvents",
            "areaDescriptions",
            "itemEvents",
            "initFunction");
        (string Reference, string Collection)[] joins =
        [
            ("entities", "entityLists"),
            ("entityEvents", "entityEventHandlers"),
            ("zoneEvents", "zoneEventHandlers"),
            ("areaDescriptions", "areaDescriptionHandlers"),
            ("itemEvents", "itemEventHandlers"),
            ("initFunction", "initFunctions"),
        ];
        foreach ((string reference, string collection) in joins)
        {
            string id = RequiredString(
                setupReferences,
                reference,
                "map3.setup.references." + reference);
            _ = RequiredResource(resources, collection, id);
        }

        if (!string.Equals(
                RequiredString(
                    setupReferences,
                    "initFunction",
                    "map3.setup.references.initFunction"),
                Map3InitIdentity,
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.setup.references.initFunction",
                "Map 3 must retain the accepted controlled init identity.");
        }

        string entityListId = RequiredString(
            setupReferences,
            "entities",
            "map3.setup.references.entities");
        if (!string.Equals(
                entityListId,
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.setup.references.entities",
                "Map 3 must retain the accepted controlled entity-list identity.");
        }

        return ReadEntityPopulation(
            map,
            new MapSetupId(Map3SetupIdentity),
            RequiredResource(resources, "entityLists", entityListId));
    }

    private static OriginalMapEntityPopulation ReadEntityPopulation(
        MapId map,
        MapSetupId selectedSetup,
        JsonElement resource)
    {
        const string resourceField = "map3.entityList";
        RequireExactProperties(resource, resourceField, "id", "address", "records");
        string resourceId = RequiredString(resource, "id", resourceField + ".id");
        _ = RequiredNonNegativeInt(resource, "address", resourceField + ".address");
        JsonElement records = RequiredProperty(resource, "records", resourceField + ".records");
        RequireArray(records, resourceField + ".records");

        List<OriginalMapEntityDefinition> definitions = [];
        foreach (JsonElement record in records.EnumerateArray())
        {
            string field = $"{resourceField}.records[{definitions.Count}]";
            RequireObject(record, field);
            string kind = RequiredString(record, "kind", field + ".kind");
            bool walking = string.Equals(kind, "walking", StringComparison.Ordinal);
            if (walking)
            {
                RequireExactProperties(
                    record,
                    field,
                    "address",
                    "kind",
                    "rawX",
                    "rawY",
                    "x",
                    "y",
                    "facing",
                    "mapSprite",
                    "walking");
            }
            else if (string.Equals(kind, "fixed", StringComparison.Ordinal) ||
                string.Equals(kind, "sequenced", StringComparison.Ordinal))
            {
                RequireExactProperties(
                    record,
                    field,
                    "address",
                    "kind",
                    "rawX",
                    "rawY",
                    "x",
                    "y",
                    "facing",
                    "mapSprite",
                    "actionValue");
            }
            else
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    field + ".kind",
                    "The original Map 3 entity kind is not recognized.");
            }

            _ = RequiredNonNegativeInt(record, "address", field + ".address");
            byte rawX = RequiredByte(record, "rawX", field + ".rawX");
            byte rawY = RequiredByte(record, "rawY", field + ".rawY");
            byte x = RequiredByte(record, "x", field + ".x");
            byte y = RequiredByte(record, "y", field + ".y");
            if (x != (rawX & 0x3F) || y != (rawY & 0x3F))
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    field + ".position",
                    "The original Map 3 entity position does not match the accepted coordinate mask.");
            }

            byte[] opaqueTail;
            if (walking)
            {
                JsonElement walkingSeed = RequiredProperty(record, "walking", field + ".walking");
                RequireObject(walkingSeed, field + ".walking");
                RequireExactProperties(
                    walkingSeed,
                    field + ".walking",
                    "originX",
                    "originY",
                    "range");
                opaqueTail =
                [
                    0xFF,
                    RequiredByte(walkingSeed, "originX", field + ".walking.originX"),
                    RequiredByte(walkingSeed, "originY", field + ".walking.originY"),
                    RequiredByte(walkingSeed, "range", field + ".walking.range"),
                ];
            }
            else
            {
                uint actionValue = RequiredUInt32(record, "actionValue", field + ".actionValue");
                opaqueTail = new byte[OriginalMapEntityDefinition.OpaqueTailByteCount];
                BinaryPrimitives.WriteUInt32BigEndian(opaqueTail, actionValue);
            }

            OriginalMapEntityDefinition definition = new(
                new OriginalMapEntityRecordIdentity(resourceId, definitions.Count + 1),
                rawX,
                rawY,
                RequiredByte(record, "facing", field + ".facing"),
                RequiredByte(record, "mapSprite", field + ".mapSprite"),
                opaqueTail);
            OriginalMapEntityRecordKind expectedKind = kind switch
            {
                "walking" => OriginalMapEntityRecordKind.Walking,
                "sequenced" => OriginalMapEntityRecordKind.Sequenced,
                _ => OriginalMapEntityRecordKind.Fixed,
            };
            if (definition.Kind != expectedKind)
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    field + ".kind",
                    "The original Map 3 entity kind does not match its opaque tail.");
            }

            definitions.Add(definition);
        }

        return new OriginalMapEntityPopulation(map, selectedSetup, definitions);
    }

    private static OriginalMapZone601Definition ReadAcceptedZone601(
        MapId map,
        OriginalMapEntityPopulation entityPopulation,
        IReadOnlyDictionary<string, Dictionary<string, JsonElement>> resources)
    {
        JsonElement handler = RequiredResource(
            resources,
            "zoneEventHandlers",
            OriginalMapRuntimeAdmission.Zone601ResourceId);
        RequireExactProperties(handler, "map3.zone601.handler", "id", "address", "kind", "records");
        if (RequiredNonNegativeInt(handler, "address", "map3.zone601.handler.address") != 331084 ||
            !string.Equals(
                RequiredString(handler, "kind", "map3.zone601.handler.kind"),
                "table",
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.zone601.handler",
                "The accepted Map 3 Zone 601 source table identity drifted.");
        }

        JsonElement records = RequiredProperty(handler, "records", "map3.zone601.handler.records");
        RequireArray(records, "map3.zone601.handler.records");
        if (records.GetArrayLength() != OriginalMapRuntimeAdmission.Zone601SourceRecordCount)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.zone601.handler.records",
                "The accepted Map 3 zone table record count drifted.");
        }

        HashSet<(byte X, byte Y)> specificKeys = [];
        int defaultCount = 0;
        JsonElement selected = default;
        int ordinal = 0;
        foreach (JsonElement record in records.EnumerateArray())
        {
            ordinal++;
            string field = $"map3.zone601.handler.records[{ordinal - 1}]";
            RequireExactProperties(
                record,
                field,
                "address",
                "kind",
                "relativeOffset",
                "resolvedTargetAddress",
                "x",
                "y");
            _ = RequiredNonNegativeInt(record, "address", field + ".address");
            _ = RequiredNonNegativeInt(record, "relativeOffset", field + ".relativeOffset");
            _ = RequiredNonNegativeInt(
                record,
                "resolvedTargetAddress",
                field + ".resolvedTargetAddress");
            string kind = RequiredString(record, "kind", field + ".kind");
            byte x = RequiredByte(record, "x", field + ".x");
            byte y = RequiredByte(record, "y", field + ".y");
            if (string.Equals(kind, "default", StringComparison.Ordinal))
            {
                defaultCount++;
            }
            else if (!string.Equals(kind, "specific", StringComparison.Ordinal) ||
                !specificKeys.Add((x, y)))
            {
                throw Admission(
                    OriginalMapImportFailureCode.DuplicateIdentity,
                    field,
                    "Map 3 zone source records must retain unique specific keys and one default.");
            }

            if (ordinal == OriginalMapRuntimeAdmission.Zone601RecordOrdinal)
            {
                selected = record;
            }
        }

        if (defaultCount != 1 ||
            selected.ValueKind != JsonValueKind.Object ||
            RequiredNonNegativeInt(selected, "address", "map3.zone601.record.address") != 331108 ||
            !string.Equals(
                RequiredString(selected, "kind", "map3.zone601.record.kind"),
                "specific",
                StringComparison.Ordinal) ||
            RequiredNonNegativeInt(
                selected,
                "relativeOffset",
                "map3.zone601.record.relativeOffset") != 248 ||
            RequiredNonNegativeInt(
                selected,
                "resolvedTargetAddress",
                "map3.zone601.record.resolvedTargetAddress") != 331332 ||
            RequiredByte(selected, "x", "map3.zone601.record.x") !=
                OriginalMapRuntimeAdmission.Zone601TriggerX ||
            RequiredByte(selected, "y", "map3.zone601.record.y") !=
                OriginalMapRuntimeAdmission.Zone601TriggerY)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.zone601.record",
                "The accepted Map 3 Zone 601 source record drifted.");
        }

        ValidateZone601BlockingProgram(resources);
        OriginalMapEntityDefinition actor = entityPopulation.Records[
            OriginalMapRuntimeAdmission.Zone601ActorSourceRecordOrdinal - 1];
        Span<byte> acceptedAction = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(
            acceptedAction,
            OriginalMapRuntimeAdmission.Zone601ActorInitialActionValue);
        if (actor.Identity != new OriginalMapEntityRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedEntityListResourceId,
                OriginalMapRuntimeAdmission.Zone601ActorSourceRecordOrdinal) ||
            actor.RawX != OriginalMapRuntimeAdmission.Zone601ActorInitialX ||
            actor.RawY != OriginalMapRuntimeAdmission.Zone601ActorInitialY ||
            actor.OpaqueFacing != OriginalMapRuntimeAdmission.Zone601ActorInitialOpaqueFacing ||
            actor.Kind != OriginalMapEntityRecordKind.Fixed ||
            !actor.OpaqueTail.SequenceEqual(acceptedAction.ToArray()))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.zone601.actor",
                "The accepted Map 3 Zone 601 source actor drifted.");
        }

        return new OriginalMapZone601Definition(
            new OriginalMapZoneEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                entityPopulation.SelectedSetup,
                OriginalMapRuntimeAdmission.Zone601ResourceId,
                OriginalMapRuntimeAdmission.Zone601RecordOrdinal,
                OriginalMapRuntimeAdmission.Zone601TargetIdentity),
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601TriggerX,
                OriginalMapRuntimeAdmission.Zone601TriggerY),
            OriginalMapRuntimeAdmission.Zone601GateFlag,
            OriginalMapRuntimeAdmission.Zone601BlockingSequenceIdentity,
            actor.Identity,
            OriginalMapRuntimeAdmission.Zone601LogicalActorId,
            actor.Position,
            actor.OpaqueFacing,
            OriginalMapRuntimeAdmission.Zone601ActorInitialBehaviorIdentity,
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601ActorBlockingEndX,
                OriginalMapRuntimeAdmission.Zone601ActorBlockingEndY),
            OriginalMapRuntimeAdmission.Zone601ActorBlockingEndOpaqueFacing,
            OriginalMapRuntimeAdmission.Zone601OpaqueFaceWaitOperand,
            OriginalMapRuntimeAdmission.Zone601TextIds,
            OriginalMapRuntimeAdmission.Zone601AmbientBehaviorIdentity,
            new MapPosition(
                OriginalMapRuntimeAdmission.Zone601AmbientCenterX,
                OriginalMapRuntimeAdmission.Zone601AmbientCenterY),
            OriginalMapRuntimeAdmission.Zone601AmbientRange,
            OriginalMapRuntimeAdmission.Zone601BlockingStages);
    }

    private static void ValidateZone601BlockingProgram(
        IReadOnlyDictionary<string, Dictionary<string, JsonElement>> resources)
    {
        JsonElement program = RequiredResource(
            resources,
            "standaloneScriptPrograms",
            OriginalMapRuntimeAdmission.Zone601BlockingSequenceIdentity);
        RequireExactProperties(
            program,
            "map3.zone601.program",
            "id",
            "address",
            "path",
            "kind",
            "operations");
        if (RequiredNonNegativeInt(program, "address", "map3.zone601.program.address") != 332892 ||
            !string.Equals(
                RequiredString(program, "kind", "map3.zone601.program.kind"),
                "cutscene",
                StringComparison.Ordinal) ||
            !string.Equals(
                RequiredString(program, "path", "map3.zone601.program.path"),
                "data/maps/entries/map03/mapsetups/scripts_1.asm",
                StringComparison.Ordinal))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.zone601.program",
                "The accepted Zone 601 blocking program identity drifted.");
        }

        (string Opcode, string Operand)[] expected =
        [
            ("setActscriptWait", "128,eas_Init"),
            ("entityActionsWait", "128"),
            ("moveUp", "2"),
            ("faceLeft", "20"),
            ("endActions", ""),
            ("textCursor", "510"),
            ("nextText", "$0,128"),
            ("nextText", "$0,128"),
            ("textCursor", "483"),
            ("nextSingleText", "$0,128"),
            ("setActscriptWait", "128,eas_Init"),
            ("csc_end", ""),
        ];
        JsonElement operations = RequiredProperty(
            program,
            "operations",
            "map3.zone601.program.operations");
        RequireArray(operations, "map3.zone601.program.operations");
        if (operations.GetArrayLength() != expected.Length)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.zone601.program.operations",
                "The accepted Zone 601 blocking operation count drifted.");
        }

        int index = 0;
        foreach (JsonElement operation in operations.EnumerateArray())
        {
            string field = $"map3.zone601.program.operations[{index}]";
            RequireExactProperties(
                operation,
                field,
                "index",
                "opcode",
                "operandText",
                "targetSymbols",
                "targetAddresses");
            JsonElement targetSymbols = RequiredProperty(operation, "targetSymbols", field + ".targetSymbols");
            JsonElement targetAddresses = RequiredProperty(
                operation,
                "targetAddresses",
                field + ".targetAddresses");
            RequireArray(targetSymbols, field + ".targetSymbols");
            RequireArray(targetAddresses, field + ".targetAddresses");
            if (RequiredInt(operation, "index", field + ".index") != index ||
                !string.Equals(
                    RequiredString(operation, "opcode", field + ".opcode"),
                    expected[index].Opcode,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    RequiredText(operation, "operandText", field + ".operandText"),
                    expected[index].Operand,
                    StringComparison.Ordinal) ||
                targetSymbols.GetArrayLength() != 0 ||
                targetAddresses.GetArrayLength() != 0)
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    field,
                    "The accepted Zone 601 blocking operation sequence drifted.");
            }

            index++;
        }
    }

    private static JsonElement RequiredResource(
        IReadOnlyDictionary<string, Dictionary<string, JsonElement>> resources,
        string collection,
        string id)
    {
        if (!resources.TryGetValue(collection, out Dictionary<string, JsonElement>? byId) ||
            !byId.TryGetValue(id, out JsonElement resource))
        {
            throw Admission(
                OriginalMapImportFailureCode.MissingReference,
                $"resources.{collection}.{id}",
                $"The canonical reference '{id}' is missing from '{collection}'.");
        }

        return resource;
    }

    private static (int X, int Y) ReadPoint(
        JsonElement point,
        string field,
        bool byteSized)
    {
        RequireObject(point, field);
        RequireExactProperties(point, field, "x", "y");
        int x = RequiredInt(point, "x", field + ".x");
        int y = RequiredInt(point, "y", field + ".y");
        int maximum = byteSized ? byte.MaxValue : ushort.MaxValue;
        if (x is < 0 || x > maximum || y is < 0 || y > maximum)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                field,
                "Canonical point coordinates exceed their source-faithful width.");
        }

        return (x, y);
    }

    private static (int Width, int Height) ReadSize(JsonElement size, string field)
    {
        RequireObject(size, field);
        RequireExactProperties(size, field, "width", "height");
        int width = RequiredInt(size, "width", field + ".width");
        int height = RequiredInt(size, "height", field + ".height");
        if (width is < 1 or > byte.MaxValue || height is < 1 or > byte.MaxValue)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                field,
                "Canonical copy sizes must remain positive byte values.");
        }

        return (width, height);
    }

    private static string ComputeWordDigest(IEnumerable<ushort> words)
    {
        ushort[] copied = [.. words];
        byte[] bytes = new byte[copied.Length * 2];
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

    private static JsonElement RequiredProperty(JsonElement owner, string name, string field)
    {
        if (!owner.TryGetProperty(name, out JsonElement value))
        {
            throw Admission(
                OriginalMapImportFailureCode.MissingReference,
                field,
                $"The canonical import is missing required field '{name}'.");
        }

        return value;
    }

    private static string RequiredString(JsonElement owner, string name, string field)
    {
        JsonElement value = RequiredProperty(owner, name, field);
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                field,
                "A required canonical identity must be a non-empty string.");
        }

        return value.GetString()!;
    }

    private static string RequiredText(JsonElement owner, string name, string field)
    {
        JsonElement value = RequiredProperty(owner, name, field);
        if (value.ValueKind != JsonValueKind.String)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                field,
                "A required canonical text operand must be a string.");
        }

        return value.GetString()!;
    }

    private static int RequiredInt(JsonElement owner, string name, string field)
    {
        JsonElement value = RequiredProperty(owner, name, field);
        if (!value.TryGetInt32(out int result))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                field,
                "A required canonical value must be a 32-bit integer.");
        }

        return result;
    }

    private static bool RequiredBoolean(JsonElement owner, string name, string field)
    {
        JsonElement value = RequiredProperty(owner, name, field);
        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                field,
                "A required canonical value must be a boolean.");
        }

        return value.GetBoolean();
    }

    private static int RequiredNonNegativeInt(JsonElement owner, string name, string field)
    {
        int value = RequiredInt(owner, name, field);
        if (value < 0)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                field,
                "A canonical address or count cannot be negative.");
        }

        return value;
    }

    private static uint RequiredUInt32(JsonElement owner, string name, string field)
    {
        JsonElement property = RequiredProperty(owner, name, field);
        if (property.ValueKind != JsonValueKind.Number || !property.TryGetUInt32(out uint value))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                field,
                "Canonical value must be an unsigned 32-bit integer.");
        }

        return value;
    }

    private static byte RequiredByte(JsonElement owner, string name, string field)
    {
        JsonElement value = RequiredProperty(owner, name, field);
        if (!TryByte(value, out byte result))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                field,
                "A canonical byte field is out of range.");
        }

        return result;
    }

    private static bool TryByte(JsonElement value, out byte result)
    {
        if (value.TryGetInt32(out int candidate) && candidate is >= byte.MinValue and <= byte.MaxValue)
        {
            result = checked((byte)candidate);
            return true;
        }

        result = default;
        return false;
    }

    private static bool TryUshort(JsonElement value, out ushort result)
    {
        if (value.TryGetInt32(out int candidate) &&
            candidate is >= ushort.MinValue and <= ushort.MaxValue)
        {
            result = checked((ushort)candidate);
            return true;
        }

        result = default;
        return false;
    }

    private static void RequireObject(JsonElement value, string field)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidDocument,
                field,
                "The canonical import field must be an object.");
        }
    }

    private static void RequireArray(JsonElement value, string field)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidDocument,
                field,
                "The canonical import field must be an array.");
        }
    }

    private static void RequireExactProperties(
        JsonElement value,
        string field,
        params string[] expectedProperties)
    {
        RequireObject(value, field);
        HashSet<string> expected = new(expectedProperties, StringComparer.Ordinal);
        HashSet<string> actual = new(StringComparer.Ordinal);
        foreach (JsonProperty property in value.EnumerateObject())
        {
            if (!actual.Add(property.Name))
            {
                throw Admission(
                    OriginalMapImportFailureCode.DuplicateIdentity,
                    field + "." + property.Name,
                    "The canonical import object contains a duplicate field.");
            }
        }

        if (!actual.SetEquals(expected))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidDocument,
                field,
                "The canonical import object contains an unknown or missing field.");
        }
    }

    private static OriginalMapImportRejected Reject(
        OriginalMapImportFailureCode code,
        string field,
        string message) =>
        new(new OriginalMapImportDiagnostic(code, field, message));

    private static ImportAdmissionException Admission(
        OriginalMapImportFailureCode code,
        string field,
        string message) =>
        new(code, field, message);

    private sealed class ImportAdmissionException : Exception
    {
        public ImportAdmissionException(
            OriginalMapImportFailureCode code,
            string field,
            string message)
            : base(message)
        {
            Code = code;
            Field = field;
        }

        public OriginalMapImportFailureCode Code { get; }

        public string Field { get; }
    }
}

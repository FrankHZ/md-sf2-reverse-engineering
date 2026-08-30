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
    ];

    private static readonly string[] UnsupportedCapabilities =
    [
        "natural-flags-setup-variant-selection",
        "natural-route-reach-order-and-continuity",
        "entity-occupancy-collision-and-obstruction",
        "warp-event-init-effects-and-persistence",
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
        Dictionary<string, Dictionary<string, JsonElement>> resources = IndexResources(
            RequiredProperty(root, "resources", "resources"));
        JsonElement references = RequiredProperty(map3, "references", "maps[3].references");
        ValidateMap3References(references, resources);

        JsonElement blockset = RequiredResource(
            resources,
            "blocksets",
            RequiredString(references, "blockset", "maps[3].references.blockset"));
        int blockCount = ValidateBlockset(blockset);
        JsonElement layoutResource = RequiredResource(
            resources,
            "layouts",
            RequiredString(references, "layout", "maps[3].references.layout"));
        ushort[] words = ReadLayout(layoutResource, blockCount);
        WorkingMapLayout workingLayout = new(words);

        JsonElement areaTable = RequiredResource(
            resources,
            "areaTables",
            RequiredString(references, "areaTable", "maps[3].references.areaTable"));
        OriginalMapTraversal traversal = new(ReadActiveAreas(areaTable));

        JsonElement stepTable = RequiredResource(
            resources,
            "stepEventTables",
            RequiredString(references, "stepEventTable", "maps[3].references.stepEventTable"));
        ValidateSchoolDoorStep(stepTable, workingLayout);
        ValidateControlledSetup(references, resources);

        MapId map = new(OriginalMapRuntimeAdmission.MapId);
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
            traversal,
            controlledAdmission,
            UnsupportedCapabilities);
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

    private static int ValidateBlockset(JsonElement resource)
    {
        RequireExactProperties(resource, "map3.blockset", "id", "address", "blocks");
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

        int blockIndex = 0;
        foreach (JsonElement block in blocks.EnumerateArray())
        {
            RequireArray(block, $"map3.blockset.blocks[{blockIndex}]");
            if (block.GetArrayLength() != 9 ||
                block.EnumerateArray().Any(word => !TryUshort(word, out _)))
            {
                throw Admission(
                    OriginalMapImportFailureCode.InvalidMapProjection,
                    $"map3.blockset.blocks[{blockIndex}]",
                    "Every canonical map block must retain exactly nine ushort tile words.");
            }

            blockIndex++;
        }

        return blocks.GetArrayLength();
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

    private static IReadOnlyList<OriginalMapTraversalArea> ReadActiveAreas(JsonElement resource)
    {
        RequireExactProperties(resource, "map3.areaTable", "id", "address", "sourceKind", "records");
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
        List<OriginalMapTraversalArea> areas = [];
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
            foreach (string pointName in new[]
                     {
                         "secondLayerForegroundStart",
                         "secondLayerBackgroundStart",
                         "mainLayerParallax",
                         "secondLayerParallax",
                     })
            {
                _ = ReadPoint(
                    RequiredProperty(record, pointName, field + "." + pointName),
                    field + "." + pointName,
                    byteSized: false);
            }

            foreach (string pointName in new[] { "mainLayerAutoscroll", "secondLayerAutoscroll" })
            {
                _ = ReadPoint(
                    RequiredProperty(record, pointName, field + "." + pointName),
                    field + "." + pointName,
                    byteSized: true);
            }

            _ = RequiredByte(record, "mainLayerType", field + ".mainLayerType");
            _ = RequiredByte(record, "defaultMusic", field + ".defaultMusic");
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

            areas.Add(new OriginalMapTraversalArea(minimumX, minimumY, maximumX, maximumY));
            index++;
        }

        if (areas.Count == 0)
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.areaTable.records",
                "Map 3 requires at least one exact active area.");
        }

        return areas;
    }

    private static void ValidateSchoolDoorStep(
        JsonElement resource,
        WorkingMapLayout workingLayout)
    {
        RequireExactProperties(resource, "map3.stepEvents", "id", "address", "sourceKind", "records");
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
        int matchingDoorRecords = 0;
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
            if (triggerX == 41 &&
                triggerY == 13 &&
                sourceX == 62 &&
                sourceY == 0 &&
                width == 1 &&
                height == 1 &&
                destinationX == 41 &&
                destinationY == 13)
            {
                matchingDoorRecords++;
            }

            index++;
        }

        if (matchingDoorRecords != 1 ||
            !OriginalMapTraversal.IsBlocked(workingLayout, new MapPosition(41, 13)) ||
            OriginalMapTraversal.IsBlocked(workingLayout, new MapPosition(62, 0)))
        {
            throw Admission(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "map3.stepEvents.schoolDoor",
                "The canonical Map 3 school-door copy or collision polarity drifted.");
        }
    }

    private static void ValidateControlledSetup(
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

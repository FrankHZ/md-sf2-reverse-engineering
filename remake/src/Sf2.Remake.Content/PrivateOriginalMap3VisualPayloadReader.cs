using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using Sf2.Remake.Application.Content;

namespace Sf2.Remake.Content;

public sealed class PrivateOriginalMap3VisualPayloadReader : IOriginalMapVisualPayloadSource
{
    public const string PackageId = OriginalMapVisualPayloadAdmission.PackageId;
    public const string Capability = OriginalMapVisualPayloadAdmission.Capability;

    private static readonly string[] TilesetRootProperties =
    [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "function",
        "table",
        "summary",
        "unusedTilesetIndices",
        "animationTileCountDistribution",
        "tilesets",
        "maps",
        "animations",
        "runtimeQuestions",
    ];

    private static readonly string[] TilesetProperties =
    [
        "index",
        "symbol",
        "sourcePath",
        "sourceAddress",
        "compressedBytes",
        "decodedBytes",
        "sourceSha256",
        "decodedSha256",
        "inputBitsConsumed",
        "trailingBits",
        "commandGroupCount",
        "literalWordCount",
        "copyCommandCount",
        "copiedWordCount",
        "maximumCopyOffsetWords",
        "maximumCopyLengthWords",
    ];

    private static readonly string[] TilesetSummaryProperties =
    [
        "tilesetCount",
        "fixedDecodedBytesPerTileset",
        "compressedByteCount",
        "decodedByteCount",
        "tableRomParityCount",
        "payloadRomParityCount",
        "mapCount",
        "mapSlotCount",
        "mapTilesetReferenceCount",
        "absentMapSlotCount",
        "uniqueMapTilesetReferenceCount",
        "animationMapCount",
        "animationTilesetReferenceCount",
        "uniqueAnimationTilesetReferenceCount",
        "combinedUsedTilesetCount",
        "unusedTilesetCount",
        "commandGroupCount",
        "literalWordCount",
        "copyCommandCount",
        "copiedWordCount",
        "minimumTrailingBits",
        "maximumTrailingBits",
        "maximumCopyOffsetWords",
        "maximumCopyLengthWords",
    ];

    private static readonly string[] PaletteRootProperties =
    [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "function",
        "table",
        "summary",
        "usageCounts",
        "palettes",
        "maps",
        "runtimeQuestions",
    ];

    private static readonly string[] PaletteProperties =
    [
        "index",
        "symbol",
        "sourcePath",
        "sourceAddress",
        "byteCount",
        "colorCount",
        "sourceFirstColor",
        "effectiveFirstColor",
        "sourceSha256",
        "effectiveSha256",
    ];

    private static readonly string[] PaletteSummaryProperties =
    [
        "paletteCount",
        "paletteByteCount",
        "colorsPerPalette",
        "sourceColorWordCount",
        "uniqueSourceColorCount",
        "validColorMaskCount",
        "nonzeroSourceFirstColorCount",
        "clearedEffectiveFirstColorCount",
        "pointerTableRomParityCount",
        "payloadRomParityCount",
        "mapCount",
        "mapHeaderRomParityCount",
        "mapPaletteReferenceCount",
        "usedPaletteCount",
        "unusedPaletteCount",
    ];

    private readonly string? _romPath;
    private readonly string? _tilesetMetadataPath;
    private readonly string? _paletteMetadataPath;

    public PrivateOriginalMap3VisualPayloadReader(
        string romPath,
        string tilesetMetadataPath,
        string paletteMetadataPath)
    {
        _romPath = ResolvePath(romPath);
        _tilesetMetadataPath = ResolvePath(tilesetMetadataPath);
        _paletteMetadataPath = ResolvePath(paletteMetadataPath);
    }

    public OriginalMapVisualPayloadResult Admit(OriginalMapVisualPayloadRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Profile != ContentProfile.PrivateLocal)
        {
            return Reject(
                OriginalMapVisualPayloadFailureCode.ProfileMismatch,
                "profile",
                "The original visual-payload adapter admits only the PrivateLocal profile.");
        }

        if (!string.Equals(request.PackageId, PackageId, StringComparison.Ordinal))
        {
            return Reject(
                OriginalMapVisualPayloadFailureCode.PackageIdentityMismatch,
                "packageId",
                "The original visual-payload adapter owns exactly one package identity.");
        }

        if (!OriginalMapVisualPayloadAdmission.HasExactAcceptedSelection(request.Selection))
        {
            return Reject(
                OriginalMapVisualPayloadFailureCode.InvalidSelection,
                "selection",
                "The payload request does not carry the exact accepted Map 3 visual-resource selection.");
        }

        if (!TryRead(_romPath, "rom", out byte[] rom, out OriginalMapVisualPayloadRejected? readFailure) ||
            !TryRead(
                _tilesetMetadataPath,
                "tilesetMetadata",
                out byte[] tilesetMetadata,
                out readFailure) ||
            !TryRead(
                _paletteMetadataPath,
                "paletteMetadata",
                out byte[] paletteMetadata,
                out readFailure))
        {
            return readFailure!;
        }

        string romDigest = Digest(rom);
        string tilesetMetadataDigest = Digest(tilesetMetadata);
        string paletteMetadataDigest = Digest(paletteMetadata);
        if (!MatchesFixedRoot(
                request.ExpectedRomDigest,
                romDigest,
                OriginalMapVisualPayloadAdmission.AcceptedRomSha256))
        {
            return Reject(
                OriginalMapVisualPayloadFailureCode.ContentDigestMismatch,
                "romDigest",
                "The private ROM bytes and caller pin must both match the accepted ROM identity.");
        }

        if (!MatchesFixedRoot(
                request.ExpectedTilesetMetadataDigest,
                tilesetMetadataDigest,
                OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest))
        {
            return Reject(
                OriginalMapVisualPayloadFailureCode.ContentDigestMismatch,
                "tilesetMetadataDigest",
                "The private tileset metadata bytes and caller pin must both match the accepted metadata identity.");
        }

        if (!MatchesFixedRoot(
                request.ExpectedPaletteMetadataDigest,
                paletteMetadataDigest,
                OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest))
        {
            return Reject(
                OriginalMapVisualPayloadFailureCode.ContentDigestMismatch,
                "paletteMetadataDigest",
                "The private palette metadata bytes and caller pin must both match the accepted metadata identity.");
        }

        return AdmitSemanticDocuments(
            rom,
            tilesetMetadata,
            paletteMetadata,
            request.Selection,
            romDigest,
            tilesetMetadataDigest,
            paletteMetadataDigest);
    }

    internal static OriginalMapVisualPayloadResult AdmitSemanticDocumentsForTests(
        byte[] rom,
        byte[] tilesetMetadata,
        byte[] paletteMetadata,
        OriginalMapVisualResourceSelection selection)
    {
        ArgumentNullException.ThrowIfNull(rom);
        ArgumentNullException.ThrowIfNull(tilesetMetadata);
        ArgumentNullException.ThrowIfNull(paletteMetadata);
        ArgumentNullException.ThrowIfNull(selection);
        return AdmitSemanticDocuments(
            rom,
            tilesetMetadata,
            paletteMetadata,
            selection,
            Digest(rom),
            Digest(tilesetMetadata),
            Digest(paletteMetadata));
    }

    private static OriginalMapVisualPayloadResult AdmitSemanticDocuments(
        byte[] rom,
        byte[] tilesetMetadata,
        byte[] paletteMetadata,
        OriginalMapVisualResourceSelection selection,
        string romDigest,
        string tilesetMetadataDigest,
        string paletteMetadataDigest)
    {
        try
        {
            using JsonDocument tilesetDocument = JsonDocument.Parse(tilesetMetadata);
            using JsonDocument paletteDocument = JsonDocument.Parse(paletteMetadata);
            TilesetPlan tilesetPlan = ParseTilesetMetadata(
                tilesetDocument.RootElement,
                selection,
                rom.Length);
            PalettePlan palettePlan = ParsePaletteMetadata(
                paletteDocument.RootElement,
                selection,
                rom.Length);

            OriginalMapPalettePayload palette = ReadPalette(rom, palettePlan);
            OriginalMapTilesetPayload[] tilesets = ReadTilesets(rom, tilesetPlan);
            OriginalMapVisualPayloadDefinition definition = new(
                selection,
                palette,
                tilesets);
            OriginalMapVisualPayloadReceipt receipt = new(
                PackageId,
                OriginalMapVisualPayloadAdmission.SchemaVersion,
                ContentProfile.PrivateLocal,
                Capability,
                new OriginalMapVisualPayloadProvenance(
                    romDigest,
                    OriginalMapVisualPayloadAdmission.AcceptedUpstreamRepository,
                    OriginalMapVisualPayloadAdmission.AcceptedUpstreamCommit,
                    OriginalMapVisualPayloadAdmission.TilesetMetadataId,
                    tilesetMetadataDigest,
                    OriginalMapVisualPayloadAdmission.PaletteMetadataId,
                    paletteMetadataDigest),
                OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners,
                OriginalMapVisualPayloadAdmission.SelectedPaletteCount,
                OriginalMapVisualPayloadAdmission.PaletteWordCount,
                OriginalMapVisualPayloadAdmission.SelectedTilesetCount,
                OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset);
            return new OriginalMapVisualPayloadAccepted(definition, receipt);
        }
        catch (JsonException)
        {
            return Reject(
                OriginalMapVisualPayloadFailureCode.InvalidDocument,
                "metadata",
                "The private visual metadata is not valid JSON.");
        }
        catch (AdmissionException error)
        {
            return Reject(error.Code, error.Field, error.Message);
        }
        catch (InvalidDataException)
        {
            return Reject(
                OriginalMapVisualPayloadFailureCode.DecodeFailure,
                "tilesetPayload",
                "A selected private tileset payload failed bounded Stack decompression.");
        }
    }

    private static TilesetPlan ParseTilesetMetadata(
        JsonElement root,
        OriginalMapVisualResourceSelection selection,
        int romLength)
    {
        RequireExactProperties(root, "tilesetMetadata", TilesetRootProperties);
        RequireSchemaAndProvenance(
            root,
            "tilesetMetadata",
            OriginalMapVisualPayloadAdmission.TilesetMetadataId);
        RequireNonNegativeObject(
            Required(root, "function", "tilesetMetadata.function"),
            "tilesetMetadata.function",
            "loadStackAddress",
            "LoadMapTilesets",
            "LoadMapArea");
        RequireNonNegativeObject(
            Required(root, "table", "tilesetMetadata.table"),
            "tilesetMetadata.table",
            "pt_MapTilesets",
            "p_pt_MapTilesets");
        RequireNonNegativeObject(
            Required(root, "summary", "tilesetMetadata.summary"),
            "tilesetMetadata.summary",
            TilesetSummaryProperties);

        JsonElement unused = Required(
            root,
            "unusedTilesetIndices",
            "tilesetMetadata.unusedTilesetIndices");
        RequireArray(unused, "tilesetMetadata.unusedTilesetIndices");
        if (unused.GetArrayLength() != 1 ||
            RequiredArrayInt(unused, 0, "tilesetMetadata.unusedTilesetIndices[0]") != 29)
        {
            throw Invalid(
                "tilesetMetadata.unusedTilesetIndices",
                "The accepted unused-tileset projection drifted.");
        }

        JsonElement distribution = Required(
            root,
            "animationTileCountDistribution",
            "tilesetMetadata.animationTileCountDistribution");
        RequireNonNegativeObject(
            distribution,
            "tilesetMetadata.animationTileCountDistribution",
            "4",
            "16",
            "32",
            "64",
            "96");

        JsonElement tilesets = Required(root, "tilesets", "tilesetMetadata.tilesets");
        RequireArray(tilesets, "tilesetMetadata.tilesets");
        if (tilesets.GetArrayLength() != 115)
        {
            throw Invalid(
                "tilesetMetadata.tilesets",
                "The accepted tileset corpus requires exactly 115 ordered records.");
        }

        Dictionary<int, TilesetRecord> selected = [];
        HashSet<string> symbols = new(StringComparer.Ordinal);
        int ordinal = 0;
        foreach (JsonElement row in tilesets.EnumerateArray())
        {
            string field = $"tilesetMetadata.tilesets[{ordinal}]";
            RequireExactProperties(row, field, TilesetProperties);
            int index = RequiredInt(row, "index", field + ".index");
            if (index != ordinal)
            {
                throw Invalid(field + ".index", "Tileset indices must remain contiguous and ordered.");
            }

            string symbol = RequiredString(row, "symbol", field + ".symbol");
            _ = RequiredString(row, "sourcePath", field + ".sourcePath");
            if (!symbols.Add(symbol))
            {
                throw Duplicate(field + ".symbol", "Tileset source symbols must remain unique.");
            }

            int sourceAddress = RequiredNonNegativeInt(
                row,
                "sourceAddress",
                field + ".sourceAddress");
            int compressedBytes = RequiredPositiveInt(
                row,
                "compressedBytes",
                field + ".compressedBytes");
            EnsureRange(sourceAddress, compressedBytes, romLength, field + ".sourceRange");
            if (RequiredInt(row, "decodedBytes", field + ".decodedBytes") !=
                OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset)
            {
                throw Invalid(field + ".decodedBytes", "A map tileset must decode to exactly 4,096 bytes.");
            }

            string sourceSha = RequiredSha(row, "sourceSha256", field + ".sourceSha256");
            string decodedSha = RequiredSha(row, "decodedSha256", field + ".decodedSha256");
            int inputBits = RequiredPositiveInt(
                row,
                "inputBitsConsumed",
                field + ".inputBitsConsumed");
            int trailingBits = RequiredNonNegativeInt(
                row,
                "trailingBits",
                field + ".trailingBits");
            if (inputBits > compressedBytes * 8 ||
                trailingBits != (compressedBytes * 8) - inputBits)
            {
                throw Invalid(field + ".trailingBits", "The Stack bit-consumption boundary drifted.");
            }

            foreach (string diagnostic in new[]
            {
                "commandGroupCount",
                "literalWordCount",
                "copyCommandCount",
                "copiedWordCount",
                "maximumCopyOffsetWords",
                "maximumCopyLengthWords",
            })
            {
                _ = RequiredNonNegativeInt(row, diagnostic, field + "." + diagnostic);
            }

            if (selection.TilesetSlots.Contains(checked((byte)index)))
            {
                selected.Add(
                    index,
                    new TilesetRecord(
                        index,
                        sourceAddress,
                        compressedBytes,
                        sourceSha,
                        decodedSha,
                        inputBits,
                        trailingBits));
            }

            ordinal++;
        }

        ValidateTilesetMaps(root, selection);
        ValidateAnimations(root);
        RequireStringArray(
            Required(root, "runtimeQuestions", "tilesetMetadata.runtimeQuestions"),
            "tilesetMetadata.runtimeQuestions",
            2);

        TilesetRecord[] ordered = new TilesetRecord[selection.TilesetSlots.Count];
        for (int slot = 0; slot < selection.TilesetSlots.Count; slot++)
        {
            byte resourceIndex = selection.TilesetSlots[slot];
            if (!selected.TryGetValue(resourceIndex, out TilesetRecord? record))
            {
                throw Missing(
                    "tilesetMetadata.tilesets",
                    "A selected tileset record is missing from the accepted corpus.");
            }

            ordered[slot] = record;
        }

        return new TilesetPlan(ordered);
    }

    private static void ValidateTilesetMaps(
        JsonElement root,
        OriginalMapVisualResourceSelection selection)
    {
        JsonElement maps = Required(root, "maps", "tilesetMetadata.maps");
        RequireArray(maps, "tilesetMetadata.maps");
        if (maps.GetArrayLength() != 79)
        {
            throw Invalid("tilesetMetadata.maps", "The accepted map-reference corpus requires 79 records.");
        }

        int ordinal = 0;
        foreach (JsonElement row in maps.EnumerateArray())
        {
            string field = $"tilesetMetadata.maps[{ordinal}]";
            RequireExactProperties(
                row,
                field,
                "mapIndex",
                "sourcePath",
                "mapAddress",
                "paletteIndex",
                "tilesetSlots");
            if (RequiredInt(row, "mapIndex", field + ".mapIndex") != ordinal)
            {
                throw Invalid(field + ".mapIndex", "Map references must remain contiguous and ordered.");
            }

            _ = RequiredString(row, "sourcePath", field + ".sourcePath");
            _ = RequiredNonNegativeInt(row, "mapAddress", field + ".mapAddress");
            byte paletteIndex = RequiredByte(row, "paletteIndex", field + ".paletteIndex");
            JsonElement slots = Required(row, "tilesetSlots", field + ".tilesetSlots");
            RequireArray(slots, field + ".tilesetSlots");
            if (slots.GetArrayLength() != OriginalMapVisualPayloadAdmission.SelectedTilesetCount)
            {
                throw Invalid(field + ".tilesetSlots", "Every map header must retain five tileset slots.");
            }

            byte[] slotValues = new byte[slots.GetArrayLength()];
            int slot = 0;
            foreach (JsonElement value in slots.EnumerateArray())
            {
                if (!value.TryGetByte(out byte parsed) || (parsed != byte.MaxValue && parsed >= 115))
                {
                    throw Invalid(field + ".tilesetSlots", "A map tileset slot is out of range.");
                }

                slotValues[slot++] = parsed;
            }

            if (ordinal == 3 &&
                (paletteIndex != selection.PaletteIndex ||
                    !slotValues.SequenceEqual(selection.TilesetSlots)))
            {
                throw Invalid(field, "The Map 3 palette/tileset selection drifted.");
            }

            ordinal++;
        }
    }

    private static void ValidateAnimations(JsonElement root)
    {
        JsonElement animations = Required(
            root,
            "animations",
            "tilesetMetadata.animations");
        RequireArray(animations, "tilesetMetadata.animations");
        if (animations.GetArrayLength() != 32)
        {
            throw Invalid(
                "tilesetMetadata.animations",
                "The accepted animation-reference corpus requires 32 records.");
        }

        HashSet<int> maps = [];
        int ordinal = 0;
        foreach (JsonElement row in animations.EnumerateArray())
        {
            string field = $"tilesetMetadata.animations[{ordinal}]";
            RequireExactProperties(
                row,
                field,
                "mapIndex",
                "sourcePath",
                "address",
                "tilesetIndex",
                "tileCount");
            int mapIndex = RequiredInt(row, "mapIndex", field + ".mapIndex");
            if (mapIndex is < 0 or > 78 || !maps.Add(mapIndex))
            {
                throw Duplicate(field + ".mapIndex", "Animation map identities must be unique and in range.");
            }

            _ = RequiredString(row, "sourcePath", field + ".sourcePath");
            _ = RequiredNonNegativeInt(row, "address", field + ".address");
            int tilesetIndex = RequiredInt(row, "tilesetIndex", field + ".tilesetIndex");
            int tileCount = RequiredInt(row, "tileCount", field + ".tileCount");
            if (tilesetIndex is < 0 or >= 115 || tileCount is not (4 or 16 or 32 or 64 or 96))
            {
                throw Invalid(field, "An animation reference is out of the accepted range.");
            }

            ordinal++;
        }
    }

    private static PalettePlan ParsePaletteMetadata(
        JsonElement root,
        OriginalMapVisualResourceSelection selection,
        int romLength)
    {
        RequireExactProperties(root, "paletteMetadata", PaletteRootProperties);
        RequireSchemaAndProvenance(
            root,
            "paletteMetadata",
            OriginalMapVisualPayloadAdmission.PaletteMetadataId);
        RequireNonNegativeObject(
            Required(root, "function", "paletteMetadata.function"),
            "paletteMetadata.function",
            "LoadMap",
            "CopyBytes");
        RequireNonNegativeObject(
            Required(root, "table", "paletteMetadata.table"),
            "paletteMetadata.table",
            "pt_MapPalettes",
            "p_pt_MapPalettes");
        RequireNonNegativeObject(
            Required(root, "summary", "paletteMetadata.summary"),
            "paletteMetadata.summary",
            PaletteSummaryProperties);

        JsonElement usage = Required(root, "usageCounts", "paletteMetadata.usageCounts");
        string[] usageKeys = [.. Enumerable.Range(0, 16).Select(index => index.ToString())];
        RequireExactProperties(usage, "paletteMetadata.usageCounts", usageKeys);
        foreach (string key in usageKeys)
        {
            if (RequiredInt(usage, key, "paletteMetadata.usageCounts." + key) < 1)
            {
                throw Invalid("paletteMetadata.usageCounts." + key, "Palette usage counts must remain positive.");
            }
        }

        JsonElement palettes = Required(root, "palettes", "paletteMetadata.palettes");
        RequireArray(palettes, "paletteMetadata.palettes");
        if (palettes.GetArrayLength() != 16)
        {
            throw Invalid("paletteMetadata.palettes", "The accepted palette corpus requires 16 ordered records.");
        }

        HashSet<string> symbols = new(StringComparer.Ordinal);
        PaletteRecord? selected = null;
        int ordinal = 0;
        foreach (JsonElement row in palettes.EnumerateArray())
        {
            string field = $"paletteMetadata.palettes[{ordinal}]";
            RequireExactProperties(row, field, PaletteProperties);
            int index = RequiredInt(row, "index", field + ".index");
            if (index != ordinal)
            {
                throw Invalid(field + ".index", "Palette indices must remain contiguous and ordered.");
            }

            string symbol = RequiredString(row, "symbol", field + ".symbol");
            _ = RequiredString(row, "sourcePath", field + ".sourcePath");
            if (!symbols.Add(symbol))
            {
                throw Duplicate(field + ".symbol", "Palette source symbols must remain unique.");
            }

            int sourceAddress = RequiredNonNegativeInt(row, "sourceAddress", field + ".sourceAddress");
            if (RequiredInt(row, "byteCount", field + ".byteCount") !=
                    OriginalMapVisualPayloadAdmission.PaletteByteCount ||
                RequiredInt(row, "colorCount", field + ".colorCount") !=
                    OriginalMapVisualPayloadAdmission.PaletteWordCount)
            {
                throw Invalid(field, "A map palette must retain the accepted 32-byte/16-word shape.");
            }

            EnsureRange(
                sourceAddress,
                OriginalMapVisualPayloadAdmission.PaletteByteCount,
                romLength,
                field + ".sourceRange");
            int sourceFirst = RequiredNonNegativeInt(row, "sourceFirstColor", field + ".sourceFirstColor");
            if (sourceFirst > OriginalMapVisualPayloadAdmission.PaletteWordMask ||
                RequiredInt(row, "effectiveFirstColor", field + ".effectiveFirstColor") != 0)
            {
                throw Invalid(field, "The palette first-word boundary drifted.");
            }

            string sourceSha = RequiredSha(row, "sourceSha256", field + ".sourceSha256");
            string effectiveSha = RequiredSha(row, "effectiveSha256", field + ".effectiveSha256");
            if (index == selection.PaletteIndex)
            {
                selected = new PaletteRecord(
                    index,
                    sourceAddress,
                    sourceFirst,
                    sourceSha,
                    effectiveSha);
            }

            ordinal++;
        }

        ValidatePaletteMaps(root, selection);
        RequireStringArray(
            Required(root, "runtimeQuestions", "paletteMetadata.runtimeQuestions"),
            "paletteMetadata.runtimeQuestions",
            1);
        return new PalettePlan(
            selected ?? throw Missing(
                "paletteMetadata.palettes",
                "The selected palette record is missing from the accepted corpus."));
    }

    private static void ValidatePaletteMaps(
        JsonElement root,
        OriginalMapVisualResourceSelection selection)
    {
        JsonElement maps = Required(root, "maps", "paletteMetadata.maps");
        RequireArray(maps, "paletteMetadata.maps");
        if (maps.GetArrayLength() != 79)
        {
            throw Invalid("paletteMetadata.maps", "The accepted palette-reference corpus requires 79 records.");
        }

        int ordinal = 0;
        foreach (JsonElement row in maps.EnumerateArray())
        {
            string field = $"paletteMetadata.maps[{ordinal}]";
            RequireExactProperties(
                row,
                field,
                "mapIndex",
                "sourcePath",
                "mapAddress",
                "paletteIndex");
            if (RequiredInt(row, "mapIndex", field + ".mapIndex") != ordinal)
            {
                throw Invalid(field + ".mapIndex", "Palette map references must remain contiguous and ordered.");
            }

            _ = RequiredString(row, "sourcePath", field + ".sourcePath");
            _ = RequiredNonNegativeInt(row, "mapAddress", field + ".mapAddress");
            int paletteIndex = RequiredInt(row, "paletteIndex", field + ".paletteIndex");
            if (paletteIndex is < 0 or > 15)
            {
                throw Invalid(field + ".paletteIndex", "A map palette reference is out of range.");
            }

            if (ordinal == 3 && paletteIndex != selection.PaletteIndex)
            {
                throw Invalid(field, "The Map 3 palette selection drifted.");
            }

            ordinal++;
        }
    }

    private static OriginalMapTilesetPayload[] ReadTilesets(
        byte[] rom,
        TilesetPlan plan)
    {
        OriginalMapTilesetPayload[] payloads = new OriginalMapTilesetPayload[plan.Records.Count];
        for (int slot = 0; slot < plan.Records.Count; slot++)
        {
            TilesetRecord record = plan.Records[slot];
            byte[] compressed = rom.AsSpan(record.SourceAddress, record.CompressedBytes).ToArray();
            if (!string.Equals(Digest(compressed), record.SourceSha256, StringComparison.Ordinal))
            {
                throw new AdmissionException(
                    OriginalMapVisualPayloadFailureCode.SourcePayloadMismatch,
                    "tilesetPayload",
                    "A selected compressed tileset source does not match its accepted private identity.");
            }

            StackCompressedGraphicsDecodeResult decoded =
                StackCompressedGraphicsDecoder.Decode(
                    compressed,
                    OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset);
            int trailingBits = (record.CompressedBytes * 8) - decoded.InputBitsConsumed;
            if (decoded.InputBitsConsumed != record.InputBitsConsumed ||
                trailingBits != record.TrailingBits)
            {
                throw new AdmissionException(
                    OriginalMapVisualPayloadFailureCode.DecodeFailure,
                    "tilesetPayload",
                    "A selected tileset Stack-consumption boundary drifted.");
            }

            if (!string.Equals(Digest(decoded.Output), record.DecodedSha256, StringComparison.Ordinal))
            {
                throw new AdmissionException(
                    OriginalMapVisualPayloadFailureCode.DecodedPayloadMismatch,
                    "tilesetPayload",
                    "A selected decoded tileset does not match its accepted private identity.");
            }

            payloads[slot] = new OriginalMapTilesetPayload(
                slot + 1,
                checked((byte)record.Index),
                decoded.Output);
        }

        return payloads;
    }

    private static OriginalMapPalettePayload ReadPalette(byte[] rom, PalettePlan plan)
    {
        PaletteRecord record = plan.Record;
        byte[] sourceBytes = rom.AsSpan(
            record.SourceAddress,
            OriginalMapVisualPayloadAdmission.PaletteByteCount).ToArray();
        if (!string.Equals(Digest(sourceBytes), record.SourceSha256, StringComparison.Ordinal))
        {
            throw new AdmissionException(
                OriginalMapVisualPayloadFailureCode.SourcePayloadMismatch,
                "palettePayload",
                "The selected palette source does not match its accepted private identity.");
        }

        ushort[] sourceWords = new ushort[OriginalMapVisualPayloadAdmission.PaletteWordCount];
        for (int index = 0; index < sourceWords.Length; index++)
        {
            sourceWords[index] = BinaryPrimitives.ReadUInt16BigEndian(
                sourceBytes.AsSpan(index * 2, 2));
            if ((sourceWords[index] & ~OriginalMapVisualPayloadAdmission.PaletteWordMask) != 0)
            {
                throw new AdmissionException(
                    OriginalMapVisualPayloadFailureCode.PalettePayloadMismatch,
                    "palettePayload",
                    "A selected palette word exceeds the accepted source mask.");
            }
        }

        if (sourceWords[0] != record.SourceFirstColor)
        {
            throw new AdmissionException(
                OriginalMapVisualPayloadFailureCode.PalettePayloadMismatch,
                "palettePayload",
                "The selected palette first source word drifted.");
        }

        byte[] effectiveBytes = [.. sourceBytes];
        effectiveBytes[0] = 0;
        effectiveBytes[1] = 0;
        if (!string.Equals(Digest(effectiveBytes), record.EffectiveSha256, StringComparison.Ordinal))
        {
            throw new AdmissionException(
                OriginalMapVisualPayloadFailureCode.PalettePayloadMismatch,
                "palettePayload",
                "The selected effective palette does not match the accepted word-zero transform.");
        }

        return new OriginalMapPalettePayload(checked((byte)record.Index), sourceWords);
    }

    private static void RequireSchemaAndProvenance(
        JsonElement root,
        string field,
        string expectedId)
    {
        if (RequiredInt(root, "schemaVersion", field + ".schemaVersion") != 1)
        {
            throw new AdmissionException(
                OriginalMapVisualPayloadFailureCode.UnsupportedSchema,
                field + ".schemaVersion",
                "The private visual metadata schema version is unsupported.");
        }

        if (!string.Equals(
                RequiredString(root, "id", field + ".id"),
                expectedId,
                StringComparison.Ordinal))
        {
            throw Invalid(field + ".id", "The private visual metadata identity drifted.");
        }

        JsonElement upstream = Required(root, "upstream", field + ".upstream");
        RequireExactProperties(upstream, field + ".upstream", "repository", "commit");
        if (!string.Equals(
                RequiredString(upstream, "repository", field + ".upstream.repository"),
                OriginalMapVisualPayloadAdmission.AcceptedUpstreamRepository,
                StringComparison.Ordinal) ||
            !string.Equals(
                RequiredString(upstream, "commit", field + ".upstream.commit"),
                OriginalMapVisualPayloadAdmission.AcceptedUpstreamCommit,
                StringComparison.Ordinal) ||
            !string.Equals(
                RequiredSha(root, "romSha256", field + ".romSha256"),
                OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                StringComparison.Ordinal))
        {
            throw new AdmissionException(
                OriginalMapVisualPayloadFailureCode.ProvenanceMismatch,
                field + ".provenance",
                "The private visual metadata provenance drifted.");
        }
    }

    private static bool TryRead(
        string? path,
        string field,
        out byte[] bytes,
        out OriginalMapVisualPayloadRejected? failure)
    {
        bytes = [];
        failure = null;
        if (path is null)
        {
            failure = Reject(
                OriginalMapVisualPayloadFailureCode.PackageUnavailable,
                field,
                "A required private visual input is unavailable: InvalidPath.");
            return false;
        }

        try
        {
            bytes = File.ReadAllBytes(path);
            return true;
        }
        catch (Exception error) when (
            error is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            failure = Reject(
                OriginalMapVisualPayloadFailureCode.PackageUnavailable,
                field,
                $"A required private visual input is unavailable: {error.GetType().Name}.");
            return false;
        }
    }

    private static string? ResolvePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
        {
            return null;
        }

        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception error) when (
            error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }

    private static bool MatchesFixedRoot(string callerPin, string actual, string fixedRoot) =>
        string.Equals(callerPin, fixedRoot, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(actual, fixedRoot, StringComparison.OrdinalIgnoreCase);

    private static string Digest(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

    private static void EnsureRange(int address, int count, int length, string field)
    {
        if (address < 0 || count < 0 || address > length - count)
        {
            throw Invalid(field, "A private source payload range exceeds the accepted ROM bytes.");
        }
    }

    private static void RequireNonNegativeObject(
        JsonElement value,
        string field,
        params string[] properties)
    {
        RequireExactProperties(value, field, properties);
        foreach (string property in properties)
        {
            _ = RequiredNonNegativeInt(value, property, field + "." + property);
        }
    }

    private static void RequireStringArray(JsonElement value, string field, int count)
    {
        RequireArray(value, field);
        if (value.GetArrayLength() != count)
        {
            throw Invalid(field, "A private metadata string-array count drifted.");
        }

        int index = 0;
        foreach (JsonElement item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(item.GetString()))
            {
                throw Invalid($"{field}[{index}]", "A private metadata string must be non-empty.");
            }

            index++;
        }
    }

    private static JsonElement Required(JsonElement owner, string name, string field)
    {
        if (!owner.TryGetProperty(name, out JsonElement value))
        {
            throw Missing(field, "The private visual metadata is missing a required field.");
        }

        return value;
    }

    private static string RequiredString(JsonElement owner, string name, string field)
    {
        JsonElement value = Required(owner, name, field);
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw Invalid(field, "A required private metadata identity must be a non-empty string.");
        }

        return value.GetString()!;
    }

    private static string RequiredSha(JsonElement owner, string name, string field)
    {
        string value = RequiredString(owner, name, field);
        if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
        {
            throw Invalid(field, "A private metadata SHA-256 identity is malformed.");
        }

        return value.ToUpperInvariant();
    }

    private static int RequiredInt(JsonElement owner, string name, string field)
    {
        JsonElement value = Required(owner, name, field);
        if (!value.TryGetInt32(out int result))
        {
            throw Invalid(field, "A required private metadata value must be a 32-bit integer.");
        }

        return result;
    }

    private static int RequiredArrayInt(JsonElement array, int index, string field)
    {
        JsonElement value = array[index];
        if (!value.TryGetInt32(out int result))
        {
            throw Invalid(field, "A required private metadata array value must be an integer.");
        }

        return result;
    }

    private static int RequiredNonNegativeInt(JsonElement owner, string name, string field)
    {
        int value = RequiredInt(owner, name, field);
        if (value < 0)
        {
            throw Invalid(field, "A required private metadata value cannot be negative.");
        }

        return value;
    }

    private static int RequiredPositiveInt(JsonElement owner, string name, string field)
    {
        int value = RequiredInt(owner, name, field);
        if (value < 1)
        {
            throw Invalid(field, "A required private metadata value must be positive.");
        }

        return value;
    }

    private static byte RequiredByte(JsonElement owner, string name, string field)
    {
        JsonElement value = Required(owner, name, field);
        if (!value.TryGetByte(out byte result))
        {
            throw Invalid(field, "A required private metadata byte is out of range.");
        }

        return result;
    }

    private static void RequireArray(JsonElement value, string field)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw Invalid(field, "The private visual metadata field must be an array.");
        }
    }

    private static void RequireExactProperties(
        JsonElement value,
        string field,
        params string[] expectedProperties)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(field, "The private visual metadata field must be an object.");
        }

        HashSet<string> expected = new(expectedProperties, StringComparer.Ordinal);
        HashSet<string> actual = new(StringComparer.Ordinal);
        foreach (JsonProperty property in value.EnumerateObject())
        {
            if (!actual.Add(property.Name))
            {
                throw Duplicate(field + "." + property.Name, "A private metadata field is duplicated.");
            }
        }

        if (!actual.SetEquals(expected))
        {
            throw Invalid(field, "The private visual metadata object has an unknown or missing field.");
        }
    }

    private static OriginalMapVisualPayloadRejected Reject(
        OriginalMapVisualPayloadFailureCode code,
        string field,
        string message) =>
        new(new OriginalMapVisualPayloadDiagnostic(code, field, message));

    private static AdmissionException Invalid(string field, string message) =>
        new(OriginalMapVisualPayloadFailureCode.InvalidDocument, field, message);

    private static AdmissionException Duplicate(string field, string message) =>
        new(OriginalMapVisualPayloadFailureCode.DuplicateIdentity, field, message);

    private static AdmissionException Missing(string field, string message) =>
        new(OriginalMapVisualPayloadFailureCode.MissingReference, field, message);

    private sealed record TilesetRecord(
        int Index,
        int SourceAddress,
        int CompressedBytes,
        string SourceSha256,
        string DecodedSha256,
        int InputBitsConsumed,
        int TrailingBits);

    private sealed record TilesetPlan(IReadOnlyList<TilesetRecord> Records);

    private sealed record PaletteRecord(
        int Index,
        int SourceAddress,
        int SourceFirstColor,
        string SourceSha256,
        string EffectiveSha256);

    private sealed record PalettePlan(PaletteRecord Record);

    private sealed class AdmissionException : Exception
    {
        public AdmissionException(
            OriginalMapVisualPayloadFailureCode code,
            string field,
            string message)
            : base(message)
        {
            Code = code;
            Field = field;
        }

        public OriginalMapVisualPayloadFailureCode Code { get; }

        public string Field { get; }
    }
}

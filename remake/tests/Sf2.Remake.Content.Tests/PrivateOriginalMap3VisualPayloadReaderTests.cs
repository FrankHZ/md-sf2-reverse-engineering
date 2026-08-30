using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Content.Tests;

public sealed class PrivateOriginalMap3VisualPayloadReaderTests
{
    [Fact]
    public void PackageProfileAndSelectionRejectBeforeAnyPrivateRead()
    {
        PrivateOriginalMap3VisualPayloadReader reader = MissingReader();

        AssertCode(
            reader.Admit(new OriginalMapVisualPayloadRequest(
                "other-package",
                ContentProfile.PrivateLocal,
                Selection(),
                OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
                OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest)),
            OriginalMapVisualPayloadFailureCode.PackageIdentityMismatch);
        AssertCode(
            reader.Admit(new OriginalMapVisualPayloadRequest(
                OriginalMapVisualPayloadAdmission.PackageId,
                ContentProfile.PublicSynthetic,
                Selection(),
                OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
                OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest)),
            OriginalMapVisualPayloadFailureCode.ProfileMismatch);
        AssertCode(
            reader.Admit(new OriginalMapVisualPayloadRequest(
                OriginalMapVisualPayloadAdmission.PackageId,
                ContentProfile.PrivateLocal,
                new OriginalMapVisualResourceSelection(
                    new MapId("map3"),
                    0,
                    new byte[] { 0, 37, 43, 53, 65 }),
                OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
                OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest)),
            OriginalMapVisualPayloadFailureCode.InvalidSelection);
    }

    [Fact]
    public void MissingPrivateInputReturnsOnlyAPathFreeTypedDiagnostic()
    {
        string marker = "sf2-private-visual-does-not-exist-" + Guid.NewGuid().ToString("N");
        string root = Path.Combine(Path.GetTempPath(), marker);
        PrivateOriginalMap3VisualPayloadReader reader = new(
            Path.Combine(root, "rom.bin"),
            Path.Combine(root, "tilesets.json"),
            Path.Combine(root, "palettes.json"));

        OriginalMapVisualPayloadRejected rejected = Assert.IsType<OriginalMapVisualPayloadRejected>(
            reader.Admit(Request()));

        Assert.Equal(
            OriginalMapVisualPayloadFailureCode.PackageUnavailable,
            rejected.Diagnostic.Code);
        Assert.DoesNotContain(root, rejected.Diagnostic.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(marker, rejected.Diagnostic.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExistingRelativeInputsAreNotResolvedAgainstTheProcessDirectory()
    {
        string current = Directory.GetCurrentDirectory();
        string root = Path.Combine(
            current,
            ".sf2-relative-visual-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string rom = Path.Combine(root, "rom.bin");
            string tilesets = Path.Combine(root, "tilesets.json");
            string palettes = Path.Combine(root, "palettes.json");
            File.WriteAllBytes(rom, "relative-rom"u8.ToArray());
            File.WriteAllBytes(tilesets, "relative-tilesets"u8.ToArray());
            File.WriteAllBytes(palettes, "relative-palettes"u8.ToArray());
            PrivateOriginalMap3VisualPayloadReader reader = new(
                Path.GetRelativePath(current, rom),
                Path.GetRelativePath(current, tilesets),
                Path.GetRelativePath(current, palettes));

            OriginalMapVisualPayloadRejected rejected =
                Assert.IsType<OriginalMapVisualPayloadRejected>(reader.Admit(Request()));

            Assert.Equal(
                OriginalMapVisualPayloadFailureCode.PackageUnavailable,
                rejected.Diagnostic.Code);
            Assert.Equal("rom", rejected.Diagnostic.Field);
            Assert.DoesNotContain(root, rejected.Diagnostic.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CallerRecomputedDigestsCannotSelfAuthorizeArbitraryPrivateBytes()
    {
        using TemporaryVisualInputs inputs = new(
            "project-authored-rom-placeholder"u8.ToArray(),
            "{\"id\":\"sf2-map-tileset-decode-v1\"}"u8.ToArray(),
            "{\"id\":\"sf2-map-palette-static-v1\"}"u8.ToArray());
        OriginalMapVisualPayloadRequest selfAuthorized = new(
            OriginalMapVisualPayloadAdmission.PackageId,
            ContentProfile.PrivateLocal,
            Selection(),
            Digest(inputs.RomBytes),
            Digest(inputs.TilesetBytes),
            Digest(inputs.PaletteBytes));

        OriginalMapVisualPayloadRejected rejected = Assert.IsType<OriginalMapVisualPayloadRejected>(
            inputs.Reader.Admit(selfAuthorized));

        Assert.Equal(
            OriginalMapVisualPayloadFailureCode.ContentDigestMismatch,
            rejected.Diagnostic.Code);
        Assert.Equal("romDigest", rejected.Diagnostic.Field);
    }

    [Fact]
    public void FixedPinsRejectMalformedBytesBeforeJsonParsing()
    {
        using TemporaryVisualInputs inputs = new(
            "not-the-accepted-private-rom"u8.ToArray(),
            "not-json"u8.ToArray(),
            "not-json"u8.ToArray());

        OriginalMapVisualPayloadRejected rejected = Assert.IsType<OriginalMapVisualPayloadRejected>(
            inputs.Reader.Admit(Request()));

        Assert.Equal(
            OriginalMapVisualPayloadFailureCode.ContentDigestMismatch,
            rejected.Diagnostic.Code);
        Assert.NotEqual(
            OriginalMapVisualPayloadFailureCode.InvalidDocument,
            rejected.Diagnostic.Code);
    }

    [Fact]
    public void ProductionSurfaceHasOnlyTheThreePathBoundConstructor()
    {
        Type readerType = typeof(PrivateOriginalMap3VisualPayloadReader);
        System.Reflection.ConstructorInfo constructor = Assert.Single(readerType.GetConstructors());

        Assert.Equal(
            new[] { typeof(string), typeof(string), typeof(string) },
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.DoesNotContain(
            readerType.GetMethods(),
                method => method.DeclaringType == readerType && method.IsStatic);
    }

    [Fact]
    public void ProjectAuthoredFullCorporaPassTheInternalSemanticValidator()
    {
        SemanticVisualInputs sample = SemanticVisualInputs.Create();

        OriginalMapVisualPayloadAccepted accepted = Assert.IsType<OriginalMapVisualPayloadAccepted>(
            AdmitSemantic(sample));

        Assert.Equal(5, accepted.Definition.Tilesets.Count);
        Assert.All(accepted.Definition.Tilesets, payload => Assert.Equal(4096, payload.DecodedBytes.Count));
        Assert.Equal((ushort)0x0EEE, accepted.Definition.Palette.SourceWords[0]);
        Assert.Equal((ushort)0, accepted.Definition.Palette.EffectiveWords[0]);
    }

    [Fact]
    public void ClosedShapeMissingFieldAndProvenanceDriftFailSemantically()
    {
        SemanticVisualInputs extra = SemanticVisualInputs.Create();
        extra.TilesetMetadata["unexpected"] = true;
        AssertCode(AdmitSemantic(extra), OriginalMapVisualPayloadFailureCode.InvalidDocument);

        SemanticVisualInputs missing = SemanticVisualInputs.Create();
        missing.TilesetMetadata.Remove("animations");
        AssertCode(AdmitSemantic(missing), OriginalMapVisualPayloadFailureCode.InvalidDocument);

        SemanticVisualInputs provenance = SemanticVisualInputs.Create();
        provenance.TilesetMetadata["upstream"]!["commit"] = new string('0', 40);
        AssertCode(AdmitSemantic(provenance), OriginalMapVisualPayloadFailureCode.ProvenanceMismatch);
    }

    [Fact]
    public void MapSelectionDuplicateIdentityAndSourceRangeDriftFailSemantically()
    {
        SemanticVisualInputs selection = SemanticVisualInputs.Create();
        TilesetMap(selection, 3)["tilesetSlots"] =
            new JsonArray(0, 37, 43, 66, 53);
        AssertCode(AdmitSemantic(selection), OriginalMapVisualPayloadFailureCode.InvalidDocument);

        SemanticVisualInputs duplicate = SemanticVisualInputs.Create();
        Tileset(duplicate, 1)["symbol"] = Tileset(duplicate, 0)["symbol"]!.GetValue<string>();
        AssertCode(AdmitSemantic(duplicate), OriginalMapVisualPayloadFailureCode.DuplicateIdentity);

        SemanticVisualInputs range = SemanticVisualInputs.Create();
        Tileset(range, 0)["sourceAddress"] = range.Rom.Length;
        AssertCode(AdmitSemantic(range), OriginalMapVisualPayloadFailureCode.InvalidDocument);
    }

    [Fact]
    public void SelectedSourceDecodedAndStackConsumptionDriftFailSemantically()
    {
        SemanticVisualInputs source = SemanticVisualInputs.Create();
        Tileset(source, 0)["sourceSha256"] = new string('0', 64);
        AssertCode(AdmitSemantic(source), OriginalMapVisualPayloadFailureCode.SourcePayloadMismatch);

        SemanticVisualInputs decoded = SemanticVisualInputs.Create();
        Tileset(decoded, 0)["decodedSha256"] = new string('0', 64);
        AssertCode(AdmitSemantic(decoded), OriginalMapVisualPayloadFailureCode.DecodedPayloadMismatch);

        SemanticVisualInputs consumption = SemanticVisualInputs.Create();
        JsonObject row = Tileset(consumption, 0);
        int inputBits = row["inputBitsConsumed"]!.GetValue<int>();
        int trailingBits = row["trailingBits"]!.GetValue<int>();
        row["inputBitsConsumed"] = inputBits - 1;
        row["trailingBits"] = trailingBits + 1;
        AssertCode(AdmitSemantic(consumption), OriginalMapVisualPayloadFailureCode.DecodeFailure);
    }

    [Fact]
    public void PaletteMaskSourceFirstAndEffectiveDigestDriftFailSemantically()
    {
        SemanticVisualInputs mask = SemanticVisualInputs.Create();
        mask.Rom[mask.PaletteAddress] = 0xFF;
        mask.Rom[mask.PaletteAddress + 1] = 0xFF;
        Palette(mask, 0)["sourceSha256"] = Digest(
            mask.Rom.AsSpan(
                mask.PaletteAddress,
                OriginalMapVisualPayloadAdmission.PaletteByteCount));
        AssertCode(AdmitSemantic(mask), OriginalMapVisualPayloadFailureCode.PalettePayloadMismatch);

        SemanticVisualInputs sourceFirst = SemanticVisualInputs.Create();
        Palette(sourceFirst, 0)["sourceFirstColor"] = 0x0222;
        AssertCode(AdmitSemantic(sourceFirst), OriginalMapVisualPayloadFailureCode.PalettePayloadMismatch);

        SemanticVisualInputs effective = SemanticVisualInputs.Create();
        Palette(effective, 0)["effectiveSha256"] = new string('0', 64);
        AssertCode(AdmitSemantic(effective), OriginalMapVisualPayloadFailureCode.PalettePayloadMismatch);
    }

    [Fact]
    public void AcceptedIgnoredInputsCloseExactPayloadAndMutationBoundaries()
    {
        string? romPath = Environment.GetEnvironmentVariable("SF2_PRIVATE_ROM");
        string? tilesetPath = Environment.GetEnvironmentVariable(
            "SF2_PRIVATE_MAP_TILESET_METADATA");
        string? palettePath = Environment.GetEnvironmentVariable(
            "SF2_PRIVATE_MAP_PALETTE_METADATA");
        if (string.IsNullOrWhiteSpace(romPath) ||
            string.IsNullOrWhiteSpace(tilesetPath) ||
            string.IsNullOrWhiteSpace(palettePath))
        {
            return;
        }

        PrivateOriginalMap3VisualPayloadReader reader = new(
            romPath,
            tilesetPath,
            palettePath);
        OriginalMapVisualPayloadAccepted accepted = Assert.IsType<OriginalMapVisualPayloadAccepted>(
            reader.Admit(Request()));

        Assert.Equal(ContentProfile.PrivateLocal, accepted.Receipt.Profile);
        Assert.Equal(OriginalMapVisualPayloadAdmission.Capability, accepted.Receipt.Capability);
        Assert.Equal(OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners,
            accepted.Receipt.EvidenceOwnerIds);
        Assert.Equal(1, accepted.Receipt.PaletteCount);
        Assert.Equal(16, accepted.Receipt.PaletteWordCount);
        Assert.Equal(5, accepted.Receipt.TilesetCount);
        Assert.Equal(4096, accepted.Receipt.DecodedBytesPerTileset);
        Assert.Equal((byte)0, accepted.Definition.Palette.ResourceIndex);
        Assert.Equal(16, accepted.Definition.Palette.SourceWords.Count);
        Assert.Equal((ushort)0, accepted.Definition.Palette.EffectiveWords[0]);
        Assert.Equal(
            new byte[] { 0, 37, 43, 53, 66 },
            accepted.Definition.Tilesets.Select(payload => payload.ResourceIndex));
        Assert.All(
            accepted.Definition.Tilesets,
            payload => Assert.Equal(4096, payload.DecodedBytes.Count));
        Assert.Contains(
            "map3-animation-tileset-74-and-replacement-lifecycle",
            accepted.Definition.UnsupportedCapabilities);
        Assert.DoesNotContain(
            accepted.Receipt.GetType().GetProperties(),
            property => property.Name.Contains("Path", StringComparison.Ordinal) ||
                property.Name.Contains("Address", StringComparison.Ordinal) ||
                property.Name.Contains("Symbol", StringComparison.Ordinal));

        byte[] tilesetMutation = File.ReadAllBytes(tilesetPath);
        tilesetMutation = [.. tilesetMutation, (byte)' '];
        string temporaryMutation = Path.Combine(
            Path.GetTempPath(),
            "sf2-private-visual-mutation-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllBytes(temporaryMutation, tilesetMutation);
            OriginalMapVisualPayloadRequest selfAuthorizedMutation = new(
                OriginalMapVisualPayloadAdmission.PackageId,
                ContentProfile.PrivateLocal,
                Selection(),
                OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                Digest(tilesetMutation),
                OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest);
            OriginalMapVisualPayloadRejected rejected =
                Assert.IsType<OriginalMapVisualPayloadRejected>(
                    new PrivateOriginalMap3VisualPayloadReader(
                        romPath,
                        temporaryMutation,
                        palettePath).Admit(selfAuthorizedMutation));
            Assert.Equal(
                OriginalMapVisualPayloadFailureCode.ContentDigestMismatch,
                rejected.Diagnostic.Code);
            Assert.Equal("tilesetMetadataDigest", rejected.Diagnostic.Field);
        }
        finally
        {
            File.Delete(temporaryMutation);
        }
    }

    private static PrivateOriginalMap3VisualPayloadReader MissingReader()
    {
        string root = Path.Combine(
            Path.GetTempPath(),
            "sf2-private-visual-does-not-exist-" + Guid.NewGuid().ToString("N"));
        return new PrivateOriginalMap3VisualPayloadReader(
            Path.Combine(root, "rom.bin"),
            Path.Combine(root, "tilesets.json"),
            Path.Combine(root, "palettes.json"));
    }

    private static OriginalMapVisualPayloadRequest Request() =>
        new(
            OriginalMapVisualPayloadAdmission.PackageId,
            ContentProfile.PrivateLocal,
            Selection(),
            OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
            OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
            OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest);

    private static OriginalMapVisualResourceSelection Selection() =>
        new(
            new MapId("map3"),
            0,
            new byte[] { 0, 37, 43, 53, 66 });

    private static OriginalMapVisualPayloadResult AdmitSemantic(SemanticVisualInputs input) =>
        PrivateOriginalMap3VisualPayloadReader.AdmitSemanticDocumentsForTests(
            input.Rom,
            JsonSerializer.SerializeToUtf8Bytes(input.TilesetMetadata),
            JsonSerializer.SerializeToUtf8Bytes(input.PaletteMetadata),
            Selection());

    private static JsonObject Tileset(SemanticVisualInputs input, int index) =>
        input.TilesetMetadata["tilesets"]![index]!.AsObject();

    private static JsonObject TilesetMap(SemanticVisualInputs input, int index) =>
        input.TilesetMetadata["maps"]![index]!.AsObject();

    private static JsonObject Palette(SemanticVisualInputs input, int index) =>
        input.PaletteMetadata["palettes"]![index]!.AsObject();

    private static string Digest(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

    private static void AssertCode(
        OriginalMapVisualPayloadResult result,
        OriginalMapVisualPayloadFailureCode expected) =>
        Assert.Equal(
            expected,
            Assert.IsType<OriginalMapVisualPayloadRejected>(result).Diagnostic.Code);

    private sealed class TemporaryVisualInputs : IDisposable
    {
        private readonly string _root;

        public TemporaryVisualInputs(
            byte[] romBytes,
            byte[] tilesetBytes,
            byte[] paletteBytes)
        {
            _root = Path.Combine(
                Path.GetTempPath(),
                "sf2-private-visual-inputs-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            RomBytes = romBytes;
            TilesetBytes = tilesetBytes;
            PaletteBytes = paletteBytes;
            string romPath = Path.Combine(_root, "rom.bin");
            string tilesetPath = Path.Combine(_root, "tilesets.json");
            string palettePath = Path.Combine(_root, "palettes.json");
            File.WriteAllBytes(romPath, RomBytes);
            File.WriteAllBytes(tilesetPath, TilesetBytes);
            File.WriteAllBytes(palettePath, PaletteBytes);
            RomPath = romPath;
            TilesetPath = tilesetPath;
            PalettePath = palettePath;
            Reader = new PrivateOriginalMap3VisualPayloadReader(
                romPath,
                tilesetPath,
                palettePath);
        }

        public byte[] RomBytes { get; }

        public byte[] TilesetBytes { get; }

        public byte[] PaletteBytes { get; }

        public string Root => _root;

        public string RomPath { get; }

        public string TilesetPath { get; }

        public string PalettePath { get; }

        public PrivateOriginalMap3VisualPayloadReader Reader { get; }

        public void Dispose() => Directory.Delete(_root, recursive: true);
    }

    private sealed class SemanticVisualInputs
    {
        private static readonly string[] TilesetSummaryFields =
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

        private static readonly string[] PaletteSummaryFields =
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

        private SemanticVisualInputs(
            byte[] rom,
            int paletteAddress,
            JsonObject tilesetMetadata,
            JsonObject paletteMetadata)
        {
            Rom = rom;
            PaletteAddress = paletteAddress;
            TilesetMetadata = tilesetMetadata;
            PaletteMetadata = paletteMetadata;
        }

        public byte[] Rom { get; }

        public int PaletteAddress { get; }

        public JsonObject TilesetMetadata { get; }

        public JsonObject PaletteMetadata { get; }

        public static SemanticVisualInputs Create()
        {
            (byte[] compressed, int inputBits) = ZeroTileset();
            byte[] decoded = new byte[OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset];
            byte[] palette = PaletteBytes();
            int paletteAddress = compressed.Length;
            byte[] rom = [.. compressed, .. palette];
            string compressedDigest = Digest(compressed);
            string decodedDigest = Digest(decoded);
            string paletteDigest = Digest(palette);
            byte[] effectivePalette = [.. palette];
            effectivePalette[0] = 0;
            effectivePalette[1] = 0;
            string effectivePaletteDigest = Digest(effectivePalette);

            JsonArray tilesets = [];
            for (int index = 0; index < 115; index++)
            {
                tilesets.Add(new JsonObject
                {
                    ["index"] = index,
                    ["symbol"] = $"ProjectTileset{index:D3}",
                    ["sourcePath"] = $"project-authored/tileset-{index:D3}.asm",
                    ["sourceAddress"] = 0,
                    ["compressedBytes"] = compressed.Length,
                    ["decodedBytes"] = decoded.Length,
                    ["sourceSha256"] = compressedDigest,
                    ["decodedSha256"] = decodedDigest,
                    ["inputBitsConsumed"] = inputBits,
                    ["trailingBits"] = (compressed.Length * 8) - inputBits,
                    ["commandGroupCount"] = 1,
                    ["literalWordCount"] = 1,
                    ["copyCommandCount"] = 1,
                    ["copiedWordCount"] = 2047,
                    ["maximumCopyOffsetWords"] = 1,
                    ["maximumCopyLengthWords"] = 2047,
                });
            }

            JsonArray tilesetMaps = [];
            JsonArray paletteMaps = [];
            for (int index = 0; index < 79; index++)
            {
                tilesetMaps.Add(new JsonObject
                {
                    ["mapIndex"] = index,
                    ["sourcePath"] = $"project-authored/map-{index:D2}.asm",
                    ["mapAddress"] = 0,
                    ["paletteIndex"] = 0,
                    ["tilesetSlots"] = new JsonArray(0, 37, 43, 53, 66),
                });
                paletteMaps.Add(new JsonObject
                {
                    ["mapIndex"] = index,
                    ["sourcePath"] = $"project-authored/map-{index:D2}.asm",
                    ["mapAddress"] = 0,
                    ["paletteIndex"] = 0,
                });
            }

            JsonArray animations = [];
            for (int index = 0; index < 32; index++)
            {
                animations.Add(new JsonObject
                {
                    ["mapIndex"] = index,
                    ["sourcePath"] = $"project-authored/animation-{index:D2}.asm",
                    ["address"] = 0,
                    ["tilesetIndex"] = 74,
                    ["tileCount"] = 4,
                });
            }

            JsonArray palettes = [];
            for (int index = 0; index < 16; index++)
            {
                palettes.Add(new JsonObject
                {
                    ["index"] = index,
                    ["symbol"] = $"ProjectPalette{index:D2}",
                    ["sourcePath"] = $"project-authored/palette-{index:D2}.asm",
                    ["sourceAddress"] = paletteAddress,
                    ["byteCount"] = palette.Length,
                    ["colorCount"] = 16,
                    ["sourceFirstColor"] = 0x0EEE,
                    ["effectiveFirstColor"] = 0,
                    ["sourceSha256"] = paletteDigest,
                    ["effectiveSha256"] = effectivePaletteDigest,
                });
            }

            JsonObject usageCounts = [];
            for (int index = 0; index < 16; index++)
            {
                usageCounts[index.ToString()] = 1;
            }

            JsonObject tilesetMetadata = new()
            {
                ["schemaVersion"] = 1,
                ["id"] = OriginalMapVisualPayloadAdmission.TilesetMetadataId,
                ["upstream"] = Upstream(),
                ["romSha256"] = OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                ["function"] = NonNegativeObject("loadStackAddress", "LoadMapTilesets", "LoadMapArea"),
                ["table"] = NonNegativeObject("pt_MapTilesets", "p_pt_MapTilesets"),
                ["summary"] = NonNegativeObject(TilesetSummaryFields),
                ["unusedTilesetIndices"] = new JsonArray(29),
                ["animationTileCountDistribution"] =
                    NonNegativeObject("4", "16", "32", "64", "96"),
                ["tilesets"] = tilesets,
                ["maps"] = tilesetMaps,
                ["animations"] = animations,
                ["runtimeQuestions"] = new JsonArray("project-authored-q1", "project-authored-q2"),
            };
            JsonObject paletteMetadata = new()
            {
                ["schemaVersion"] = 1,
                ["id"] = OriginalMapVisualPayloadAdmission.PaletteMetadataId,
                ["upstream"] = Upstream(),
                ["romSha256"] = OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                ["function"] = NonNegativeObject("LoadMap", "CopyBytes"),
                ["table"] = NonNegativeObject("pt_MapPalettes", "p_pt_MapPalettes"),
                ["summary"] = NonNegativeObject(PaletteSummaryFields),
                ["usageCounts"] = usageCounts,
                ["palettes"] = palettes,
                ["maps"] = paletteMaps,
                ["runtimeQuestions"] = new JsonArray("project-authored-q"),
            };
            return new SemanticVisualInputs(
                rom,
                paletteAddress,
                tilesetMetadata,
                paletteMetadata);
        }

        private static JsonObject Upstream() =>
            new()
            {
                ["repository"] = OriginalMapVisualPayloadAdmission.AcceptedUpstreamRepository,
                ["commit"] = OriginalMapVisualPayloadAdmission.AcceptedUpstreamCommit,
            };

        private static JsonObject NonNegativeObject(params string[] names)
        {
            JsonObject result = [];
            foreach (string name in names)
            {
                result[name] = 0;
            }

            return result;
        }

        private static byte[] PaletteBytes()
        {
            ushort[] words =
            [
                0x0EEE,
                0x0222,
                0x0444,
                0x0666,
                0x0888,
                0x0AAA,
                0x0CCC,
                0x0000,
                0x0002,
                0x0020,
                0x0200,
                0x000E,
                0x00E0,
                0x0E00,
                0x0246,
                0x068A,
            ];
            byte[] bytes = new byte[words.Length * 2];
            for (int index = 0; index < words.Length; index++)
            {
                bytes[index * 2] = checked((byte)(words[index] >> 8));
                bytes[(index * 2) + 1] = checked((byte)(words[index] & 0xFF));
            }

            return bytes;
        }

        private static (byte[] Bytes, int InputBits) ZeroTileset()
        {
            StringBuilder bits = new();
            bits.Append("11110110");
            bits.Append("000");
            bits.Append("00000000");
            bits.Append("00000000001");
            for (int index = 0; index < 1022; index++)
            {
                bits.Append("00");
            }

            bits.Append("01");
            bits.Append('0', 11);
            int inputBits = bits.Length;
            byte[] output = new byte[(inputBits + 7) / 8];
            for (int index = 0; index < inputBits; index++)
            {
                if (bits[index] == '1')
                {
                    output[index / 8] |= checked((byte)(1 << (7 - (index % 8))));
                }
            }

            return (output, inputBits);
        }
    }
}

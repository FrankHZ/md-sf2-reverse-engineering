using System.Collections.ObjectModel;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapVisualPayloadRequest
{
    public OriginalMapVisualPayloadRequest(
        string packageId,
        ContentProfile profile,
        OriginalMapVisualResourceSelection selection,
        string expectedRomDigest,
        string expectedTilesetMetadataDigest,
        string expectedPaletteMetadataDigest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        Selection = selection ?? throw new ArgumentNullException(nameof(selection));
        OriginalMapImportRequest.ValidateSha256(
            expectedRomDigest,
            nameof(expectedRomDigest));
        OriginalMapImportRequest.ValidateSha256(
            expectedTilesetMetadataDigest,
            nameof(expectedTilesetMetadataDigest));
        OriginalMapImportRequest.ValidateSha256(
            expectedPaletteMetadataDigest,
            nameof(expectedPaletteMetadataDigest));

        PackageId = packageId;
        Profile = profile;
        ExpectedRomDigest = expectedRomDigest.ToUpperInvariant();
        ExpectedTilesetMetadataDigest = expectedTilesetMetadataDigest.ToUpperInvariant();
        ExpectedPaletteMetadataDigest = expectedPaletteMetadataDigest.ToUpperInvariant();
    }

    public string PackageId { get; }

    public ContentProfile Profile { get; }

    public OriginalMapVisualResourceSelection Selection { get; }

    public string ExpectedRomDigest { get; }

    public string ExpectedTilesetMetadataDigest { get; }

    public string ExpectedPaletteMetadataDigest { get; }
}

public sealed class OriginalMapPalettePayload
{
    private readonly ReadOnlyCollection<ushort> _sourceWords;
    private readonly ReadOnlyCollection<ushort> _effectiveWords;

    public OriginalMapPalettePayload(
        byte resourceIndex,
        IEnumerable<ushort> sourceWords)
    {
        ArgumentNullException.ThrowIfNull(sourceWords);
        ushort[] copied =
        [
            .. sourceWords.Take(OriginalMapVisualPayloadAdmission.PaletteWordCount + 1),
        ];
        if (copied.Length != OriginalMapVisualPayloadAdmission.PaletteWordCount)
        {
            throw new ArgumentException(
                $"An original map palette must contain exactly {OriginalMapVisualPayloadAdmission.PaletteWordCount} source words.",
                nameof(sourceWords));
        }

        if (copied.Any(word =>
                (word & ~OriginalMapVisualPayloadAdmission.PaletteWordMask) != 0))
        {
            throw new ArgumentException(
                "An original map palette source word exceeds the accepted 0x0EEE mask.",
                nameof(sourceWords));
        }

        ushort[] effective = [.. copied];
        effective[0] = 0;
        ResourceIndex = resourceIndex;
        _sourceWords = Array.AsReadOnly(copied);
        _effectiveWords = Array.AsReadOnly(effective);
    }

    public byte ResourceIndex { get; }

    public IReadOnlyList<ushort> SourceWords => _sourceWords;

    public IReadOnlyList<ushort> EffectiveWords => _effectiveWords;
}

public sealed class OriginalMapTilesetPayload
{
    private readonly ReadOnlyCollection<byte> _decodedBytes;

    public OriginalMapTilesetPayload(
        int slotOrdinal,
        byte resourceIndex,
        IEnumerable<byte> decodedBytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(slotOrdinal, 1);
        ArgumentNullException.ThrowIfNull(decodedBytes);
        byte[] copied =
        [
            .. decodedBytes.Take(
                OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset + 1),
        ];
        if (copied.Length != OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset)
        {
            throw new ArgumentException(
                $"An original map tileset must contain exactly {OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset} decoded bytes.",
                nameof(decodedBytes));
        }

        SlotOrdinal = slotOrdinal;
        ResourceIndex = resourceIndex;
        _decodedBytes = Array.AsReadOnly(copied);
    }

    public int SlotOrdinal { get; }

    public byte ResourceIndex { get; }

    public IReadOnlyList<byte> DecodedBytes => _decodedBytes;
}

public sealed class OriginalMapVisualPayloadDefinition
{
    private readonly ReadOnlyCollection<OriginalMapTilesetPayload> _tilesets;
    private readonly ReadOnlyCollection<string> _unsupportedCapabilities;

    public OriginalMapVisualPayloadDefinition(
        OriginalMapVisualResourceSelection selection,
        OriginalMapPalettePayload palette,
        IEnumerable<OriginalMapTilesetPayload> tilesets)
    {
        Selection = selection ?? throw new ArgumentNullException(nameof(selection));
        Palette = palette ?? throw new ArgumentNullException(nameof(palette));
        ArgumentNullException.ThrowIfNull(tilesets);
        if (!OriginalMapVisualPayloadAdmission.HasExactAcceptedSelection(selection))
        {
            throw new ArgumentException(
                "A private Map 3 payload definition requires the exact accepted visual-resource selection.",
                nameof(selection));
        }

        if (palette.ResourceIndex != selection.PaletteIndex)
        {
            throw new ArgumentException(
                "The admitted palette does not match the accepted map selection.",
                nameof(palette));
        }

        OriginalMapTilesetPayload[] copiedTilesets =
        [
            .. tilesets.Take(OriginalMapVisualPayloadAdmission.SelectedTilesetCount + 1),
        ];
        if (copiedTilesets.Length !=
            OriginalMapVisualPayloadAdmission.SelectedTilesetCount)
        {
            throw new ArgumentException(
                $"A private Map 3 payload definition requires exactly {OriginalMapVisualPayloadAdmission.SelectedTilesetCount} ordered tilesets.",
                nameof(tilesets));
        }

        for (int index = 0; index < copiedTilesets.Length; index++)
        {
            OriginalMapTilesetPayload payload = copiedTilesets[index] ??
                throw new ArgumentException(
                    "A private Map 3 payload definition cannot contain a null tileset.",
                    nameof(tilesets));
            if (payload.SlotOrdinal != index + 1 ||
                payload.ResourceIndex != selection.TilesetSlots[index])
            {
                throw new ArgumentException(
                    "The admitted tilesets must preserve the exact selected slot order and identities.",
                    nameof(tilesets));
            }
        }

        _tilesets = Array.AsReadOnly(copiedTilesets);
        _unsupportedCapabilities = Array.AsReadOnly(
            OriginalMapVisualPayloadAdmission.UnsupportedCapabilities.ToArray());
    }

    public OriginalMapVisualResourceSelection Selection { get; }

    public OriginalMapPalettePayload Palette { get; }

    public IReadOnlyList<OriginalMapTilesetPayload> Tilesets => _tilesets;

    public IReadOnlyList<string> UnsupportedCapabilities => _unsupportedCapabilities;
}

public sealed record OriginalMapVisualPayloadProvenance
{
    public OriginalMapVisualPayloadProvenance(
        string romSha256,
        string upstreamRepository,
        string upstreamCommit,
        string tilesetMetadataId,
        string tilesetMetadataDigest,
        string paletteMetadataId,
        string paletteMetadataDigest)
    {
        OriginalMapImportRequest.ValidateSha256(romSha256, nameof(romSha256));
        ArgumentException.ThrowIfNullOrWhiteSpace(upstreamRepository);
        ValidateCommit(upstreamCommit, nameof(upstreamCommit));
        ArgumentException.ThrowIfNullOrWhiteSpace(tilesetMetadataId);
        OriginalMapImportRequest.ValidateSha256(
            tilesetMetadataDigest,
            nameof(tilesetMetadataDigest));
        ArgumentException.ThrowIfNullOrWhiteSpace(paletteMetadataId);
        OriginalMapImportRequest.ValidateSha256(
            paletteMetadataDigest,
            nameof(paletteMetadataDigest));

        RomSha256 = romSha256.ToUpperInvariant();
        UpstreamRepository = upstreamRepository;
        UpstreamCommit = upstreamCommit.ToLowerInvariant();
        TilesetMetadataId = tilesetMetadataId;
        TilesetMetadataDigest = tilesetMetadataDigest.ToUpperInvariant();
        PaletteMetadataId = paletteMetadataId;
        PaletteMetadataDigest = paletteMetadataDigest.ToUpperInvariant();
    }

    public string RomSha256 { get; }

    public string UpstreamRepository { get; }

    public string UpstreamCommit { get; }

    public string TilesetMetadataId { get; }

    public string TilesetMetadataDigest { get; }

    public string PaletteMetadataId { get; }

    public string PaletteMetadataDigest { get; }

    private static void ValidateCommit(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (value.Length != 40 || value.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException(
                "A pinned Git commit must contain exactly 40 hexadecimal characters.",
                parameterName);
        }
    }
}

public sealed class OriginalMapVisualPayloadReceipt
{
    private readonly ReadOnlyCollection<string> _evidenceOwnerIds;

    public OriginalMapVisualPayloadReceipt(
        string packageId,
        int schemaVersion,
        ContentProfile profile,
        string capability,
        OriginalMapVisualPayloadProvenance provenance,
        IEnumerable<string> evidenceOwnerIds,
        int paletteCount,
        int paletteWordCount,
        int tilesetCount,
        int decodedBytesPerTileset)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentOutOfRangeException.ThrowIfLessThan(schemaVersion, 1);
        if (profile != ContentProfile.PrivateLocal)
        {
            throw new ArgumentException(
                "An original map visual-payload receipt must remain PrivateLocal.",
                nameof(profile));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(capability);
        Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));
        ArgumentNullException.ThrowIfNull(evidenceOwnerIds);
        string[] copiedOwners = [.. evidenceOwnerIds];
        if (!OriginalMapVisualPayloadAdmission.HasExactEvidenceOwners(copiedOwners))
        {
            throw new ArgumentException(
                "The visual-payload receipt requires the exact closed evidence-owner set.",
                nameof(evidenceOwnerIds));
        }

        if (paletteCount != OriginalMapVisualPayloadAdmission.SelectedPaletteCount ||
            paletteWordCount != OriginalMapVisualPayloadAdmission.PaletteWordCount ||
            tilesetCount != OriginalMapVisualPayloadAdmission.SelectedTilesetCount ||
            decodedBytesPerTileset !=
                OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset)
        {
            throw new ArgumentException(
                "The visual-payload receipt dimensions do not match the accepted bounded shape.");
        }

        PackageId = packageId;
        SchemaVersion = schemaVersion;
        Profile = profile;
        Capability = capability;
        _evidenceOwnerIds = Array.AsReadOnly(copiedOwners);
        PaletteCount = paletteCount;
        PaletteWordCount = paletteWordCount;
        TilesetCount = tilesetCount;
        DecodedBytesPerTileset = decodedBytesPerTileset;
    }

    public string PackageId { get; }

    public int SchemaVersion { get; }

    public ContentProfile Profile { get; }

    public string Capability { get; }

    public OriginalMapVisualPayloadProvenance Provenance { get; }

    public IReadOnlyList<string> EvidenceOwnerIds => _evidenceOwnerIds;

    public int PaletteCount { get; }

    public int PaletteWordCount { get; }

    public int TilesetCount { get; }

    public int DecodedBytesPerTileset { get; }
}

public enum OriginalMapVisualPayloadFailureCode
{
    PackageUnavailable,
    PackageIdentityMismatch,
    ProfileMismatch,
    ContentDigestMismatch,
    InvalidSelection,
    InvalidDocument,
    UnsupportedSchema,
    ProvenanceMismatch,
    DuplicateIdentity,
    MissingReference,
    SourcePayloadMismatch,
    DecodeFailure,
    DecodedPayloadMismatch,
    PalettePayloadMismatch,
}

public sealed record OriginalMapVisualPayloadDiagnostic
{
    public OriginalMapVisualPayloadDiagnostic(
        OriginalMapVisualPayloadFailureCode code,
        string field,
        string message)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Field = field;
        Message = message;
    }

    public OriginalMapVisualPayloadFailureCode Code { get; }

    public string Field { get; }

    public string Message { get; }
}

public abstract record OriginalMapVisualPayloadResult;

public sealed record OriginalMapVisualPayloadAccepted(
    OriginalMapVisualPayloadDefinition Definition,
    OriginalMapVisualPayloadReceipt Receipt) : OriginalMapVisualPayloadResult
{
    public OriginalMapVisualPayloadDefinition Definition { get; } =
        Definition ?? throw new ArgumentNullException(nameof(Definition));

    public OriginalMapVisualPayloadReceipt Receipt { get; } =
        Receipt ?? throw new ArgumentNullException(nameof(Receipt));
}

public sealed record OriginalMapVisualPayloadRejected(
    OriginalMapVisualPayloadDiagnostic Diagnostic) : OriginalMapVisualPayloadResult
{
    public OriginalMapVisualPayloadDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public interface IOriginalMapVisualPayloadSource
{
    OriginalMapVisualPayloadResult Admit(OriginalMapVisualPayloadRequest request);
}

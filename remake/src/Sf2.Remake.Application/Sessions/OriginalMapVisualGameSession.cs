using Sf2.Remake.Application.Content;

namespace Sf2.Remake.Application.Sessions;

public static class PrivateOriginalMapVisualRuntimeAdmission
{
    public const string Capability =
        "private-local-map3-base-visual-runtime-admission-v1";
}

public sealed class PrivateOriginalMapVisualRuntimeBinding
{
    internal PrivateOriginalMapVisualRuntimeBinding(
        OriginalMapVisualPayloadDefinition definition,
        OriginalMapVisualPayloadReceipt receipt)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
    }

    public string Capability => PrivateOriginalMapVisualRuntimeAdmission.Capability;

    public OriginalMapVisualPayloadDefinition Definition { get; }

    public OriginalMapVisualPayloadReceipt Receipt { get; }
}

public enum PrivateOriginalMapVisualRuntimeFailureCode
{
    PackageIdentityMismatch,
    UnsupportedSchema,
    ProfileMismatch,
    SelectionMismatch,
    ProvenanceMismatch,
    EvidenceOwnerMismatch,
    InvalidShape,
}

public sealed record PrivateOriginalMapVisualRuntimeDiagnostic
{
    public PrivateOriginalMapVisualRuntimeDiagnostic(
        PrivateOriginalMapVisualRuntimeFailureCode code,
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

    public PrivateOriginalMapVisualRuntimeFailureCode Code { get; }

    public string Field { get; }

    public string Message { get; }
}

public abstract record PrivateOriginalMapVisualGameSessionStartResult;

public sealed record PrivateOriginalMapVisualGameSessionStarted(
    GameSession Session,
    OriginalMapImportReceipt ImportReceipt,
    PrivateOriginalMapVisualRuntimeBinding Binding) :
    PrivateOriginalMapVisualGameSessionStartResult
{
    public GameSession Session { get; } =
        Session ?? throw new ArgumentNullException(nameof(Session));

    public OriginalMapImportReceipt ImportReceipt { get; } =
        ImportReceipt ?? throw new ArgumentNullException(nameof(ImportReceipt));

    public PrivateOriginalMapVisualRuntimeBinding Binding { get; } =
        Binding ?? throw new ArgumentNullException(nameof(Binding));
}

public sealed record PrivateOriginalMapVisualGameSessionImportRejected(
    OriginalMapImportDiagnostic Diagnostic) :
    PrivateOriginalMapVisualGameSessionStartResult
{
    public OriginalMapImportDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public sealed record PrivateOriginalMapVisualGameSessionPayloadRejected(
    OriginalMapVisualPayloadDiagnostic Diagnostic) :
    PrivateOriginalMapVisualGameSessionStartResult
{
    public OriginalMapVisualPayloadDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public sealed record PrivateOriginalMapVisualGameSessionBindingRejected(
    PrivateOriginalMapVisualRuntimeDiagnostic Diagnostic) :
    PrivateOriginalMapVisualGameSessionStartResult
{
    public PrivateOriginalMapVisualRuntimeDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public sealed partial class GameSession
{
    public static PrivateOriginalMapVisualGameSessionStartResult
        StartPrivateOriginalMapWithVisualPayload(
            IOriginalMapImportSource importSource,
            OriginalMapImportRequest importRequest,
            IOriginalMapVisualPayloadSource visualSource,
            OriginalMapVisualPayloadRequest visualRequest)
    {
        ArgumentNullException.ThrowIfNull(importSource);
        ArgumentNullException.ThrowIfNull(importRequest);
        ArgumentNullException.ThrowIfNull(visualSource);
        ArgumentNullException.ThrowIfNull(visualRequest);

        OriginalMapImportDiagnostic? importRequestDiagnostic =
            ValidateRequest(importRequest);
        if (importRequestDiagnostic is not null)
        {
            return new PrivateOriginalMapVisualGameSessionImportRejected(
                importRequestDiagnostic);
        }

        OriginalMapVisualPayloadDiagnostic? visualRequestDiagnostic =
            ValidateVisualRequest(visualRequest);
        if (visualRequestDiagnostic is not null)
        {
            return new PrivateOriginalMapVisualGameSessionPayloadRejected(
                visualRequestDiagnostic);
        }

        OriginalMapImportResult importResult = importSource.Admit(importRequest);
        if (importResult is OriginalMapImportRejected importRejected)
        {
            return new PrivateOriginalMapVisualGameSessionImportRejected(
                importRejected.Diagnostic);
        }

        if (importResult is not OriginalMapImportAccepted importAccepted)
        {
            throw new InvalidOperationException(
                "Original-map source returned an unknown admission result.");
        }

        OriginalMapImportDiagnostic? importAcceptedDiagnostic =
            ValidateAccepted(importAccepted);
        if (importAcceptedDiagnostic is not null)
        {
            return new PrivateOriginalMapVisualGameSessionImportRejected(
                importAcceptedDiagnostic);
        }

        OriginalMapVisualPayloadResult visualResult = visualSource.Admit(visualRequest);
        if (visualResult is OriginalMapVisualPayloadRejected visualRejected)
        {
            return new PrivateOriginalMapVisualGameSessionPayloadRejected(
                visualRejected.Diagnostic);
        }

        if (visualResult is not OriginalMapVisualPayloadAccepted visualAccepted)
        {
            throw new InvalidOperationException(
                "Original-map visual-payload source returned an unknown admission result.");
        }

        PrivateOriginalMapVisualRuntimeDiagnostic? bindingDiagnostic =
            ValidateVisualBinding(importAccepted, visualRequest, visualAccepted);
        if (bindingDiagnostic is not null)
        {
            return new PrivateOriginalMapVisualGameSessionBindingRejected(bindingDiagnostic);
        }

        PrivateOriginalMapGameSessionStartResult sessionResult =
            StartPrivateOriginalMapAccepted(importAccepted);
        if (sessionResult is not PrivateOriginalMapGameSessionStarted started)
        {
            throw new InvalidOperationException(
                "A validated private original-map import failed during session construction.");
        }

        return new PrivateOriginalMapVisualGameSessionStarted(
            started.Session,
            started.Receipt,
            new PrivateOriginalMapVisualRuntimeBinding(
                visualAccepted.Definition,
                visualAccepted.Receipt));
    }

    private static OriginalMapVisualPayloadDiagnostic? ValidateVisualRequest(
        OriginalMapVisualPayloadRequest request)
    {
        if (request.Profile != ContentProfile.PrivateLocal)
        {
            return VisualDiagnostic(
                OriginalMapVisualPayloadFailureCode.ProfileMismatch,
                "profile",
                "Private original-map visual runtime admission requires the PrivateLocal profile.");
        }

        if (!string.Equals(
                request.PackageId,
                OriginalMapVisualPayloadAdmission.PackageId,
                StringComparison.Ordinal))
        {
            return VisualDiagnostic(
                OriginalMapVisualPayloadFailureCode.PackageIdentityMismatch,
                "packageId",
                "Private original-map visual runtime admission owns one visual package identity.");
        }

        if (!string.Equals(
                request.ExpectedRomDigest,
                OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                request.ExpectedTilesetMetadataDigest,
                OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                request.ExpectedPaletteMetadataDigest,
                OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest,
                StringComparison.OrdinalIgnoreCase))
        {
            return VisualDiagnostic(
                OriginalMapVisualPayloadFailureCode.ContentDigestMismatch,
                "contentDigests",
                "Private original-map visual runtime admission requires every accepted whole-contract digest pin.");
        }

        if (!OriginalMapVisualPayloadAdmission.HasExactAcceptedSelection(request.Selection))
        {
            return VisualDiagnostic(
                OriginalMapVisualPayloadFailureCode.InvalidSelection,
                "selection",
                "Private original-map visual runtime admission requires the exact Map 3 resource selection.");
        }

        return null;
    }

    private static PrivateOriginalMapVisualRuntimeDiagnostic? ValidateVisualBinding(
        OriginalMapImportAccepted importAccepted,
        OriginalMapVisualPayloadRequest visualRequest,
        OriginalMapVisualPayloadAccepted visualAccepted)
    {
        OriginalMapVisualPayloadDefinition definition = visualAccepted.Definition;
        OriginalMapVisualPayloadReceipt receipt = visualAccepted.Receipt;
        if (!SameSelection(
                importAccepted.Definition.VisualResourceSelection,
                visualRequest.Selection) ||
            !SameSelection(visualRequest.Selection, definition.Selection) ||
            !OriginalMapVisualPayloadAdmission.HasExactAcceptedSelection(definition.Selection))
        {
            return BindingDiagnostic(
                PrivateOriginalMapVisualRuntimeFailureCode.SelectionMismatch,
                "selection",
                "The canonical import, visual request, and admitted visual definition do not identify the same accepted Map 3 resource selection.");
        }

        if (!string.Equals(
                receipt.PackageId,
                OriginalMapVisualPayloadAdmission.PackageId,
                StringComparison.Ordinal) ||
            !string.Equals(
                receipt.Capability,
                OriginalMapVisualPayloadAdmission.Capability,
                StringComparison.Ordinal))
        {
            return BindingDiagnostic(
                PrivateOriginalMapVisualRuntimeFailureCode.PackageIdentityMismatch,
                "receipt.identity",
                "The admitted visual receipt does not retain the accepted package and capability identities.");
        }

        if (receipt.SchemaVersion != OriginalMapVisualPayloadAdmission.SchemaVersion)
        {
            return BindingDiagnostic(
                PrivateOriginalMapVisualRuntimeFailureCode.UnsupportedSchema,
                "receipt.schemaVersion",
                "The admitted visual receipt schema is not supported by the private runtime.");
        }

        if (receipt.Profile != ContentProfile.PrivateLocal)
        {
            return BindingDiagnostic(
                PrivateOriginalMapVisualRuntimeFailureCode.ProfileMismatch,
                "receipt.profile",
                "The admitted visual receipt is not PrivateLocal.");
        }

        OriginalMapVisualPayloadProvenance provenance = receipt.Provenance;
        OriginalMapImportProvenance importProvenance = importAccepted.Receipt.Provenance;
        if (!string.Equals(
                provenance.RomSha256,
                OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                provenance.RomSha256,
                importProvenance.RomSha256,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                provenance.UpstreamRepository,
                OriginalMapVisualPayloadAdmission.AcceptedUpstreamRepository,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.UpstreamRepository,
                importProvenance.UpstreamRepository,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.UpstreamCommit,
                OriginalMapVisualPayloadAdmission.AcceptedUpstreamCommit,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                provenance.UpstreamCommit,
                importProvenance.UpstreamCommit,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                provenance.TilesetMetadataId,
                OriginalMapVisualPayloadAdmission.TilesetMetadataId,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.TilesetMetadataDigest,
                OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                provenance.PaletteMetadataId,
                OriginalMapVisualPayloadAdmission.PaletteMetadataId,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.PaletteMetadataDigest,
                OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest,
                StringComparison.OrdinalIgnoreCase))
        {
            return BindingDiagnostic(
                PrivateOriginalMapVisualRuntimeFailureCode.ProvenanceMismatch,
                "receipt.provenance",
                "The admitted visual receipt does not retain the exact accepted whole-contract provenance.");
        }

        string[] owners = [.. receipt.EvidenceOwnerIds];
        HashSet<string> uniqueOwners = new(owners, StringComparer.Ordinal);
        if (owners.Length != OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners.Count ||
            uniqueOwners.Count != owners.Length ||
            !uniqueOwners.SetEquals(OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners))
        {
            return BindingDiagnostic(
                PrivateOriginalMapVisualRuntimeFailureCode.EvidenceOwnerMismatch,
                "receipt.evidenceOwnerIds",
                "The admitted visual receipt does not retain the exact closed evidence-owner rows.");
        }

        if (receipt.PaletteCount !=
                OriginalMapVisualPayloadAdmission.SelectedPaletteCount ||
            receipt.PaletteWordCount !=
                OriginalMapVisualPayloadAdmission.PaletteWordCount ||
            receipt.TilesetCount !=
                OriginalMapVisualPayloadAdmission.SelectedTilesetCount ||
            receipt.DecodedBytesPerTileset !=
                OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset)
        {
            return BindingDiagnostic(
                PrivateOriginalMapVisualRuntimeFailureCode.InvalidShape,
                "receipt.dimensions",
                "The admitted visual receipt dimensions do not retain the accepted bounded shape.");
        }

        return null;
    }

    private static bool SameSelection(
        OriginalMapVisualResourceSelection left,
        OriginalMapVisualResourceSelection right) =>
        string.Equals(left.Map.Value, right.Map.Value, StringComparison.Ordinal) &&
        left.PaletteIndex == right.PaletteIndex &&
        left.TilesetSlots.SequenceEqual(right.TilesetSlots) &&
        string.Equals(
            left.ProjectionDigest,
            right.ProjectionDigest,
            StringComparison.OrdinalIgnoreCase);

    private static OriginalMapVisualPayloadDiagnostic VisualDiagnostic(
        OriginalMapVisualPayloadFailureCode code,
        string field,
        string message) => new(code, field, message);

    private static PrivateOriginalMapVisualRuntimeDiagnostic BindingDiagnostic(
        PrivateOriginalMapVisualRuntimeFailureCode code,
        string field,
        string message) => new(code, field, message);
}

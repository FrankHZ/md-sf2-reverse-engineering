using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapImportRequest
{
    public OriginalMapImportRequest(
        string packageId,
        ContentProfile profile,
        string expectedContentDigest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        ValidateSha256(expectedContentDigest, nameof(expectedContentDigest));
        PackageId = packageId;
        Profile = profile;
        ExpectedContentDigest = expectedContentDigest.ToUpperInvariant();
    }

    public string PackageId { get; }

    public ContentProfile Profile { get; }

    public string ExpectedContentDigest { get; }

    internal static void ValidateSha256(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException(
                "A SHA-256 identity must contain exactly 64 hexadecimal characters.",
                parameterName);
        }
    }
}

public sealed record OriginalMapControlledAdmission
{
    public OriginalMapControlledAdmission(
        MapId map,
        MapPosition position,
        byte opaqueFacing,
        MapSetupId selectedSetup,
        string selectedInitIdentity,
        bool noProgramRequest)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        SelectedSetup = selectedSetup ?? throw new ArgumentNullException(nameof(selectedSetup));
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedInitIdentity);
        OpaqueFacing = opaqueFacing;
        SelectedInitIdentity = selectedInitIdentity;
        NoProgramRequest = noProgramRequest;
    }

    public MapId Map { get; }

    public MapPosition Position { get; }

    public byte OpaqueFacing { get; }

    public MapSetupId SelectedSetup { get; }

    public string SelectedInitIdentity { get; }

    public bool NoProgramRequest { get; }
}

public sealed record OriginalMapStepCopyIdentity
{
    public OriginalMapStepCopyIdentity(
        ContentProfile profile,
        MapId map,
        string sourceResourceId,
        int oneBasedRecordOrdinal)
    {
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        Map = map ?? throw new ArgumentNullException(nameof(map));
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceResourceId);
        ArgumentOutOfRangeException.ThrowIfLessThan(oneBasedRecordOrdinal, 1);
        Profile = profile;
        SourceResourceId = sourceResourceId;
        OneBasedRecordOrdinal = oneBasedRecordOrdinal;
    }

    public ContentProfile Profile { get; }

    public MapId Map { get; }

    public string SourceResourceId { get; }

    public int OneBasedRecordOrdinal { get; }
}

public sealed record OriginalMapStepCopyDefinition
{
    public OriginalMapStepCopyDefinition(
        OriginalMapStepCopyIdentity identity,
        MapPosition trigger,
        WorkingMapBlockCopy copy)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        Copy = copy ?? throw new ArgumentNullException(nameof(copy));
    }

    public OriginalMapStepCopyIdentity Identity { get; }

    public MapPosition Trigger { get; }

    public WorkingMapBlockCopy Copy { get; }
}

public sealed class OriginalMapImportDefinition
{
    private readonly ReadOnlyCollection<string> _unsupportedCapabilities;

    public OriginalMapImportDefinition(
        MapId map,
        WorkingMapLayout workingLayout,
        OriginalMapBlockCatalog blockCatalog,
        OriginalMapAreaCatalog areaCatalog,
        OriginalMapEntityPopulation entityPopulation,
        OriginalMapVisualResourceSelection visualResourceSelection,
        OriginalMapControlledAdmission controlledAdmission,
        IEnumerable<string> unsupportedCapabilities)
        : this(
            map,
            workingLayout,
            blockCatalog,
            areaCatalog,
            entityPopulation,
            visualResourceSelection,
            controlledAdmission,
            controlledStepCopy: null,
            sameMapWarps: null,
            unsupportedCapabilities)
    {
    }

    public OriginalMapImportDefinition(
        MapId map,
        WorkingMapLayout workingLayout,
        OriginalMapBlockCatalog blockCatalog,
        OriginalMapAreaCatalog areaCatalog,
        OriginalMapEntityPopulation entityPopulation,
        OriginalMapVisualResourceSelection visualResourceSelection,
        OriginalMapControlledAdmission controlledAdmission,
        OriginalMapStepCopyDefinition? controlledStepCopy,
        IEnumerable<string> unsupportedCapabilities)
        : this(
            map,
            workingLayout,
            blockCatalog,
            areaCatalog,
            entityPopulation,
            visualResourceSelection,
            controlledAdmission,
            controlledStepCopy,
            sameMapWarps: null,
            unsupportedCapabilities)
    {
    }

    public OriginalMapImportDefinition(
        MapId map,
        WorkingMapLayout workingLayout,
        OriginalMapBlockCatalog blockCatalog,
        OriginalMapAreaCatalog areaCatalog,
        OriginalMapEntityPopulation entityPopulation,
        OriginalMapVisualResourceSelection visualResourceSelection,
        OriginalMapControlledAdmission controlledAdmission,
        OriginalMapStepCopyDefinition? controlledStepCopy,
        OriginalMapSameMapWarpCatalog? sameMapWarps,
        IEnumerable<string> unsupportedCapabilities)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        WorkingLayout = workingLayout ?? throw new ArgumentNullException(nameof(workingLayout));
        BlockCatalog = blockCatalog ?? throw new ArgumentNullException(nameof(blockCatalog));
        AreaCatalog = areaCatalog ?? throw new ArgumentNullException(nameof(areaCatalog));
        EntityPopulation = entityPopulation ??
            throw new ArgumentNullException(nameof(entityPopulation));
        VisualResourceSelection = visualResourceSelection ??
            throw new ArgumentNullException(nameof(visualResourceSelection));
        ControlledAdmission = controlledAdmission ??
            throw new ArgumentNullException(nameof(controlledAdmission));
        ArgumentNullException.ThrowIfNull(unsupportedCapabilities);
        if (visualResourceSelection.Map != map)
        {
            throw new ArgumentException(
                "The visual-resource selection map must equal the imported map.",
                nameof(visualResourceSelection));
        }

        if (controlledAdmission.Map != map)
        {
            throw new ArgumentException(
                "The controlled admission map must equal the imported map.",
                nameof(controlledAdmission));
        }

        if (entityPopulation.Map != map ||
            entityPopulation.SelectedSetup != controlledAdmission.SelectedSetup)
        {
            throw new ArgumentException(
                "The entity population must match the imported map and controlled setup.",
                nameof(entityPopulation));
        }

        if (controlledStepCopy is not null && controlledStepCopy.Identity.Map != map)
        {
            throw new ArgumentException(
                "The controlled step-copy map must equal the imported map.",
                nameof(controlledStepCopy));
        }

        if (sameMapWarps is not null && sameMapWarps.Map != map)
        {
            throw new ArgumentException(
                "The same-map warp catalog must match the imported map.",
                nameof(sameMapWarps));
        }

        BlockCatalog.ValidateLayoutReferences(workingLayout, nameof(workingLayout));

        if (sameMapWarps is not null)
        {
            foreach (OriginalMapSameMapWarpDefinition warp in sameMapWarps.Records)
            {
                if (!areaCatalog.Traversal.IsWithinActiveArea(warp.Trigger) ||
                    !areaCatalog.Traversal.IsWithinActiveArea(warp.Destination) ||
                    OriginalMapTraversal.IsBlocked(workingLayout, warp.Destination))
                {
                    throw new ArgumentException(
                        "Every admitted same-map warp trigger and destination must fit the active map, and destinations must be traversable.",
                        nameof(sameMapWarps));
                }
            }
        }

        if (!areaCatalog.Traversal.IsWithinActiveArea(controlledAdmission.Position) ||
            OriginalMapTraversal.IsBlocked(workingLayout, controlledAdmission.Position))
        {
            throw new ArgumentException(
                "The controlled admission position must be active and traversable.",
                nameof(controlledAdmission));
        }

        List<string> copiedUnsupported = [];
        HashSet<string> uniqueUnsupported = new(StringComparer.Ordinal);
        foreach (string capability in unsupportedCapabilities)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(capability);
            if (!uniqueUnsupported.Add(capability))
            {
                throw new ArgumentException(
                    $"Duplicate unsupported capability '{capability}'.",
                    nameof(unsupportedCapabilities));
            }

            copiedUnsupported.Add(capability);
        }

        if (copiedUnsupported.Count == 0)
        {
            throw new ArgumentException(
                "A bounded original import must preserve its unsupported capability set.",
                nameof(unsupportedCapabilities));
        }

        _unsupportedCapabilities = copiedUnsupported.AsReadOnly();
        ControlledStepCopy = controlledStepCopy;
        SameMapWarps = sameMapWarps;
    }

    public MapId Map { get; }

    public WorkingMapLayout WorkingLayout { get; }

    public OriginalMapBlockCatalog BlockCatalog { get; }

    public OriginalMapAreaCatalog AreaCatalog { get; }

    public OriginalMapEntityPopulation EntityPopulation { get; }

    public OriginalMapVisualResourceSelection VisualResourceSelection { get; }

    public OriginalMapTraversal Traversal => AreaCatalog.Traversal;

    public OriginalMapControlledAdmission ControlledAdmission { get; }

    public OriginalMapStepCopyDefinition? ControlledStepCopy { get; }

    public OriginalMapSameMapWarpCatalog? SameMapWarps { get; }

    public IReadOnlyList<string> UnsupportedCapabilities => _unsupportedCapabilities;
}

public sealed record OriginalMapImportProvenance
{
    public OriginalMapImportProvenance(
        string canonicalImportId,
        string romSha256,
        string upstreamRepository,
        string upstreamCommit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalImportId);
        OriginalMapImportRequest.ValidateSha256(romSha256, nameof(romSha256));
        ArgumentException.ThrowIfNullOrWhiteSpace(upstreamRepository);
        ValidateGitCommit(upstreamCommit, nameof(upstreamCommit));
        CanonicalImportId = canonicalImportId;
        RomSha256 = romSha256.ToUpperInvariant();
        UpstreamRepository = upstreamRepository;
        UpstreamCommit = upstreamCommit.ToLowerInvariant();
    }

    public string CanonicalImportId { get; }

    public string RomSha256 { get; }

    public string UpstreamRepository { get; }

    public string UpstreamCommit { get; }

    private static void ValidateGitCommit(string value, string parameterName)
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

public sealed class OriginalMapImportReceipt
{
    private readonly ReadOnlyCollection<string> _capabilities;
    private readonly ReadOnlyCollection<string> _evidenceOwnerIds;

    public OriginalMapImportReceipt(
        string packageId,
        int schemaVersion,
        string contentDigest,
        string decodedLayoutDigest,
        string collisionProjectionDigest,
        ContentProfile profile,
        OriginalMapImportProvenance provenance,
        IEnumerable<string> evidenceOwnerIds,
        IEnumerable<string> capabilities)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentOutOfRangeException.ThrowIfLessThan(schemaVersion, 1);
        OriginalMapImportRequest.ValidateSha256(contentDigest, nameof(contentDigest));
        OriginalMapImportRequest.ValidateSha256(
            decodedLayoutDigest,
            nameof(decodedLayoutDigest));
        OriginalMapImportRequest.ValidateSha256(
            collisionProjectionDigest,
            nameof(collisionProjectionDigest));
        if (profile != ContentProfile.PrivateLocal)
        {
            throw new ArgumentException(
                "An original map import receipt must remain PrivateLocal.",
                nameof(profile));
        }

        Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));
        _evidenceOwnerIds = CopyClosedStrings(evidenceOwnerIds, nameof(evidenceOwnerIds));
        _capabilities = CopyClosedStrings(capabilities, nameof(capabilities));
        PackageId = packageId;
        SchemaVersion = schemaVersion;
        ContentDigest = contentDigest.ToUpperInvariant();
        DecodedLayoutDigest = decodedLayoutDigest.ToUpperInvariant();
        CollisionProjectionDigest = collisionProjectionDigest.ToUpperInvariant();
        Profile = profile;
    }

    public string PackageId { get; }

    public int SchemaVersion { get; }

    public string ContentDigest { get; }

    public string DecodedLayoutDigest { get; }

    public string CollisionProjectionDigest { get; }

    public ContentProfile Profile { get; }

    public OriginalMapImportProvenance Provenance { get; }

    public IReadOnlyList<string> EvidenceOwnerIds => _evidenceOwnerIds;

    public IReadOnlyList<string> Capabilities => _capabilities;

    private static ReadOnlyCollection<string> CopyClosedStrings(
        IEnumerable<string> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        List<string> copied = [];
        HashSet<string> unique = new(StringComparer.Ordinal);
        foreach (string value in values)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
            if (!unique.Add(value))
            {
                throw new ArgumentException($"Duplicate closed identity '{value}'.", parameterName);
            }

            copied.Add(value);
        }

        if (copied.Count == 0)
        {
            throw new ArgumentException("A closed identity set cannot be empty.", parameterName);
        }

        return copied.AsReadOnly();
    }
}

public enum OriginalMapImportFailureCode
{
    PackageUnavailable,
    PackageIdentityMismatch,
    ProfileMismatch,
    ContentDigestMismatch,
    InvalidDocument,
    UnsupportedSchema,
    ProvenanceMismatch,
    InvalidMapProjection,
    DuplicateIdentity,
    MissingReference,
}

public sealed record OriginalMapImportDiagnostic
{
    public OriginalMapImportDiagnostic(
        OriginalMapImportFailureCode code,
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

    public OriginalMapImportFailureCode Code { get; }

    public string Field { get; }

    public string Message { get; }
}

public abstract record OriginalMapImportResult;

public sealed record OriginalMapImportAccepted(
    OriginalMapImportDefinition Definition,
    OriginalMapImportReceipt Receipt) : OriginalMapImportResult
{
    public OriginalMapImportDefinition Definition { get; } =
        Definition ?? throw new ArgumentNullException(nameof(Definition));

    public OriginalMapImportReceipt Receipt { get; } =
        Receipt ?? throw new ArgumentNullException(nameof(Receipt));
}

public sealed record OriginalMapImportRejected(
    OriginalMapImportDiagnostic Diagnostic) : OriginalMapImportResult
{
    public OriginalMapImportDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public interface IOriginalMapImportSource
{
    OriginalMapImportResult Admit(OriginalMapImportRequest request);
}

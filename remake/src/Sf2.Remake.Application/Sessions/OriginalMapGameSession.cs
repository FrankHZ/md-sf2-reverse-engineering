using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapSessionSnapshot
{
    public PrivateOriginalMapSessionSnapshot(
        OriginalMapImportDefinition definition,
        OriginalMapImportReceipt receipt,
        long simulationStep,
        MapPosition playerPosition,
        OriginalMapTraversalResult? lastTraversal)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
        ArgumentOutOfRangeException.ThrowIfNegative(simulationStep);
        PlayerPosition = playerPosition ?? throw new ArgumentNullException(nameof(playerPosition));
        if (!definition.Traversal.IsWithinActiveArea(playerPosition) ||
            OriginalMapTraversal.IsBlocked(definition.WorkingLayout, playerPosition))
        {
            throw new ArgumentException(
                "The private original-map session position must remain active and traversable.",
                nameof(playerPosition));
        }

        if ((simulationStep == 0) != (lastTraversal is null))
        {
            throw new ArgumentException(
                "Only the initial private original-map snapshot may omit a traversal result.",
                nameof(lastTraversal));
        }

        if (lastTraversal is not null && lastTraversal.Position != playerPosition)
        {
            throw new ArgumentException(
                "The traversal result must end at the authoritative session position.",
                nameof(lastTraversal));
        }

        SimulationStep = simulationStep;
        LastTraversal = lastTraversal;
    }

    public ContentProfile Profile => ContentProfile.PrivateLocal;

    public GameFlowStage FlowStage => GameFlowStage.Exploration;

    public OriginalMapImportDefinition Definition { get; }

    public OriginalMapImportReceipt Receipt { get; }

    public MapId Map => Definition.Map;

    public WorkingMapLayout WorkingLayout => Definition.WorkingLayout;

    public MapPosition PlayerPosition { get; }

    public long SimulationStep { get; }

    public OriginalMapTraversalResult? LastTraversal { get; }
}

public abstract record PrivateOriginalMapGameSessionStartResult;

public sealed record PrivateOriginalMapGameSessionStarted(
    GameSession Session,
    OriginalMapImportReceipt Receipt) : PrivateOriginalMapGameSessionStartResult
{
    public GameSession Session { get; } =
        Session ?? throw new ArgumentNullException(nameof(Session));

    public OriginalMapImportReceipt Receipt { get; } =
        Receipt ?? throw new ArgumentNullException(nameof(Receipt));
}

public sealed record PrivateOriginalMapGameSessionStartRejected(
    OriginalMapImportDiagnostic Diagnostic) : PrivateOriginalMapGameSessionStartResult
{
    public OriginalMapImportDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public sealed record PrivateOriginalMapMoveApplied(
    PrivateOriginalMapSessionSnapshot Snapshot,
    OriginalMapTraversalResult Traversal)
{
    public PrivateOriginalMapSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public OriginalMapTraversalResult Traversal { get; } =
        Traversal ?? throw new ArgumentNullException(nameof(Traversal));
}

public sealed partial class GameSession
{
    private PrivateOriginalMapSessionSnapshot? _privateOriginalMapSnapshot;

    private GameSession(PrivateOriginalMapSessionSnapshot snapshot)
    {
        _snapshot = null;
        _mapContext = null!;
        _privateOriginalMapSnapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    }

    public PrivateOriginalMapSessionSnapshot PrivateOriginalMapSnapshot =>
        _privateOriginalMapSnapshot ?? throw new InvalidOperationException(
            "This GameSession does not own a private original-map runtime.");

    public static PrivateOriginalMapGameSessionStartResult StartPrivateOriginalMap(
        IOriginalMapImportSource source,
        OriginalMapImportRequest request)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(request);

        OriginalMapImportDiagnostic? requestDiagnostic = ValidateRequest(request);
        if (requestDiagnostic is not null)
        {
            return new PrivateOriginalMapGameSessionStartRejected(requestDiagnostic);
        }

        return source.Admit(request) switch
        {
            OriginalMapImportAccepted accepted => StartPrivateOriginalMapAccepted(accepted),
            OriginalMapImportRejected rejected =>
                new PrivateOriginalMapGameSessionStartRejected(rejected.Diagnostic),
            _ => throw new InvalidOperationException(
                "Original-map source returned an unknown admission result."),
        };
    }

    public PrivateOriginalMapMoveApplied ApplyPrivateOriginalMap(
        MoveExplorationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        PrivateOriginalMapSessionSnapshot current = PrivateOriginalMapSnapshot;
        OriginalMapTraversalResult traversal = current.Definition.Traversal.TryMove(
            current.Definition.WorkingLayout,
            current.PlayerPosition,
            command.Direction);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            checked(current.SimulationStep + 1),
            traversal.Position,
            traversal);
        _privateOriginalMapSnapshot = next;
        return new PrivateOriginalMapMoveApplied(next, traversal);
    }

    private static PrivateOriginalMapGameSessionStartResult StartPrivateOriginalMapAccepted(
        OriginalMapImportAccepted accepted)
    {
        OriginalMapImportDiagnostic? diagnostic = ValidateAccepted(accepted);
        if (diagnostic is not null)
        {
            return new PrivateOriginalMapGameSessionStartRejected(diagnostic);
        }

        PrivateOriginalMapSessionSnapshot snapshot = new(
            accepted.Definition,
            accepted.Receipt,
            simulationStep: 0,
            accepted.Definition.ControlledAdmission.Position,
            lastTraversal: null);
        GameSession session = new(snapshot);
        return new PrivateOriginalMapGameSessionStarted(session, accepted.Receipt);
    }

    private static OriginalMapImportDiagnostic? ValidateRequest(OriginalMapImportRequest request)
    {
        if (request.Profile != ContentProfile.PrivateLocal)
        {
            return Diagnostic(
                OriginalMapImportFailureCode.ProfileMismatch,
                "profile",
                "Private original-map session admission requires the PrivateLocal profile.");
        }

        if (!string.Equals(
                request.PackageId,
                OriginalMapRuntimeAdmission.PackageId,
                StringComparison.Ordinal))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.PackageIdentityMismatch,
                "packageId",
                "Private original-map session admission owns one canonical package identity.");
        }

        if (!string.Equals(
                request.ExpectedContentDigest,
                OriginalMapRuntimeAdmission.AcceptedContentDigest,
                StringComparison.OrdinalIgnoreCase))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.ContentDigestMismatch,
                "contentDigest",
                "Private original-map session admission requires the accepted canonical digest pin.");
        }

        return null;
    }

    private static OriginalMapImportDiagnostic? ValidateAccepted(
        OriginalMapImportAccepted accepted)
    {
        OriginalMapImportReceipt receipt = accepted.Receipt;
        OriginalMapImportDefinition definition = accepted.Definition;
        if (!string.Equals(
                receipt.PackageId,
                OriginalMapRuntimeAdmission.PackageId,
                StringComparison.Ordinal))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.PackageIdentityMismatch,
                "receipt.packageId",
                "The admitted receipt does not identify the canonical original-map package.");
        }

        if (receipt.SchemaVersion != OriginalMapRuntimeAdmission.SchemaVersion)
        {
            return Diagnostic(
                OriginalMapImportFailureCode.UnsupportedSchema,
                "receipt.schemaVersion",
                "The admitted receipt schema is not supported by the private runtime.");
        }

        if (receipt.Profile != ContentProfile.PrivateLocal)
        {
            return Diagnostic(
                OriginalMapImportFailureCode.ProfileMismatch,
                "receipt.profile",
                "The admitted receipt is not PrivateLocal.");
        }

        if (!string.Equals(
                receipt.ContentDigest,
                OriginalMapRuntimeAdmission.AcceptedContentDigest,
                StringComparison.OrdinalIgnoreCase))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.ContentDigestMismatch,
                "receipt.contentDigest",
                "The admitted receipt does not retain the accepted canonical digest.");
        }

        if (!string.Equals(
                receipt.Provenance.CanonicalImportId,
                OriginalMapRuntimeAdmission.PackageId,
                StringComparison.Ordinal) ||
            !string.Equals(
                receipt.Provenance.RomSha256,
                OriginalMapRuntimeAdmission.AcceptedRomSha256,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                receipt.Provenance.UpstreamRepository,
                OriginalMapRuntimeAdmission.AcceptedUpstreamRepository,
                StringComparison.Ordinal) ||
            !string.Equals(
                receipt.Provenance.UpstreamCommit,
                OriginalMapRuntimeAdmission.AcceptedUpstreamCommit,
                StringComparison.OrdinalIgnoreCase))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.ProvenanceMismatch,
                "receipt.provenance",
                "The admitted receipt does not retain the exact accepted canonical provenance.");
        }

        if (!string.Equals(
                receipt.DecodedLayoutDigest,
                OriginalMapRuntimeAdmission.AcceptedDecodedLayoutDigest,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                receipt.CollisionProjectionDigest,
                OriginalMapRuntimeAdmission.AcceptedCollisionProjectionDigest,
                StringComparison.OrdinalIgnoreCase))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "receipt.mapProjectionDigests",
                "The admitted receipt does not retain the exact accepted Map 3 projections.");
        }

        if (!OriginalMapRuntimeAdmission.HasExactRequiredEvidenceOwners(
                receipt.EvidenceOwnerIds))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.ProvenanceMismatch,
                "receipt.evidenceOwnerIds",
                "The admitted receipt does not retain the exact accepted evidence-owner set.");
        }

        if (!OriginalMapRuntimeAdmission.HasExactRequiredCapabilities(receipt.Capabilities))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.MissingReference,
                "receipt.capabilities",
                "The admitted receipt does not contain the exact bounded runtime capability set.");
        }

        OriginalMapControlledAdmission controlled = definition.ControlledAdmission;
        if (!string.Equals(
                definition.Map.Value,
                OriginalMapRuntimeAdmission.MapId,
                StringComparison.Ordinal) ||
            controlled.Position != new MapPosition(
                OriginalMapRuntimeAdmission.StartX,
                OriginalMapRuntimeAdmission.StartY) ||
            controlled.OpaqueFacing != OriginalMapRuntimeAdmission.OpaqueStartFacing ||
            !string.Equals(
                controlled.SelectedSetup.Value,
                OriginalMapRuntimeAdmission.SelectedSetupId,
                StringComparison.Ordinal) ||
            !string.Equals(
                controlled.SelectedInitIdentity,
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                StringComparison.Ordinal) ||
            !controlled.NoProgramRequest)
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.controlledAdmission",
                "The admitted definition does not retain the exact controlled Map 3 start projection.");
        }

        return null;
    }

    private static OriginalMapImportDiagnostic Diagnostic(
        OriginalMapImportFailureCode code,
        string field,
        string message) =>
        new(code, field, message);
}

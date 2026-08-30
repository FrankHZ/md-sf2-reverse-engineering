using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapSessionSnapshot
{
    public PrivateOriginalMapSessionSnapshot(
        OriginalMapImportDefinition definition,
        OriginalMapImportReceipt receipt,
        WorkingMapLayout workingLayout,
        long simulationStep,
        MapPosition playerPosition,
        OriginalMapTraversalResult? lastTraversal,
        bool controlledStepCopyApplied,
        PrivateOriginalMapLayoutMutationReceipt? lastLayoutMutation)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
        WorkingLayout = workingLayout ?? throw new ArgumentNullException(nameof(workingLayout));
        ArgumentOutOfRangeException.ThrowIfNegative(simulationStep);
        PlayerPosition = playerPosition ?? throw new ArgumentNullException(nameof(playerPosition));
        if (definition.Traversal.SelectActiveArea(playerPosition) is null ||
            OriginalMapTraversal.IsBlocked(workingLayout, playerPosition))
        {
            throw new ArgumentException(
                "The private original-map session position must remain active and traversable.",
                nameof(playerPosition));
        }

        if (simulationStep == 0 &&
            (lastTraversal is not null || lastLayoutMutation is not null || controlledStepCopyApplied))
        {
            throw new ArgumentException(
                "The initial private original-map snapshot cannot contain a completed operation.",
                nameof(simulationStep));
        }

        if (simulationStep > 0 && (lastTraversal is null) == (lastLayoutMutation is null))
        {
            throw new ArgumentException(
                "A non-initial private original-map snapshot must identify exactly one last operation.",
                nameof(lastTraversal));
        }

        if (lastTraversal is not null && lastTraversal.Position != playerPosition)
        {
            throw new ArgumentException(
                "The traversal result must end at the authoritative session position.",
                nameof(lastTraversal));
        }

        if (lastLayoutMutation is not null &&
            (!controlledStepCopyApplied || lastLayoutMutation.SimulationStep != simulationStep))
        {
            throw new ArgumentException(
                "The layout-mutation receipt must identify the authoritative snapshot step.",
                nameof(lastLayoutMutation));
        }

        if (controlledStepCopyApplied && definition.ControlledStepCopy is null)
        {
            throw new ArgumentException(
                "An applied controlled step-copy requires its admitted definition.",
                nameof(controlledStepCopyApplied));
        }

        if (lastLayoutMutation is not null &&
            lastLayoutMutation.RecordIdentity != definition.ControlledStepCopy!.Identity)
        {
            throw new ArgumentException(
                "The layout-mutation receipt must identify the admitted step-copy record.",
                nameof(lastLayoutMutation));
        }

        SimulationStep = simulationStep;
        LastTraversal = lastTraversal;
        ControlledStepCopyApplied = controlledStepCopyApplied;
        LastLayoutMutation = lastLayoutMutation;
    }

    public ContentProfile Profile => ContentProfile.PrivateLocal;

    public GameFlowStage FlowStage => GameFlowStage.Exploration;

    public OriginalMapImportDefinition Definition { get; }

    public OriginalMapImportReceipt Receipt { get; }

    public MapId Map => Definition.Map;

    public WorkingMapLayout WorkingLayout { get; }

    public MapPosition PlayerPosition { get; }

    public OriginalMapTraversalAreaSelection CurrentArea =>
        Definition.Traversal.SelectActiveArea(PlayerPosition) ??
        throw new InvalidOperationException(
            "The authoritative private original-map position has no admitted active area.");

    public OriginalMapAreaDefinition CurrentAreaDefinition =>
        Definition.AreaCatalog.Resolve(CurrentArea);

    public long SimulationStep { get; }

    public OriginalMapTraversalResult? LastTraversal { get; }

    public bool ControlledStepCopyApplied { get; }

    public PrivateOriginalMapLayoutMutationReceipt? LastLayoutMutation { get; }
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
            current.WorkingLayout,
            current.PlayerPosition,
            command.Direction);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            current.WorkingLayout,
            checked(current.SimulationStep + 1),
            traversal.Position,
            traversal,
            current.ControlledStepCopyApplied,
            lastLayoutMutation: null);
        _privateOriginalMapSnapshot = next;
        return new PrivateOriginalMapMoveApplied(next, traversal);
    }

    public PrivateOriginalMapLayoutMutationResult ApplyPrivateOriginalMapLayoutMutation(
        ApplyPrivateOriginalMapLayoutMutationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        PrivateOriginalMapSessionSnapshot current = PrivateOriginalMapSnapshot;
        OriginalMapStepCopyDefinition admitted =
            current.Definition.ControlledStepCopy ?? throw new InvalidOperationException(
                "The admitted private original-map definition has no controlled step-copy record.");

        if (command.RecordIdentity != admitted.Identity)
        {
            return RejectLayoutMutation(
                current,
                PrivateOriginalMapLayoutMutationFailureCode.ReferenceMismatch,
                "The command does not identify the admitted private Map 3 step-copy record.");
        }

        if (command.ExpectedSimulationStep != current.SimulationStep)
        {
            return RejectLayoutMutation(
                current,
                PrivateOriginalMapLayoutMutationFailureCode.StaleSimulationStep,
                "The command targets a stale private original-map simulation step.");
        }

        if (current.ControlledStepCopyApplied)
        {
            return RejectLayoutMutation(
                current,
                PrivateOriginalMapLayoutMutationFailureCode.AlreadyApplied,
                "The one-shot controlled step-copy diagnostic has already been applied.");
        }

        PrivateOriginalMapCollisionCategory before = ClassifyCollision(
            current,
            new MapPosition(admitted.Copy.DestinationX, admitted.Copy.DestinationY));
        WorkingMapLayout nextLayout = current.WorkingLayout.ApplyBlockCopy(admitted.Copy);
        PrivateOriginalMapCollisionCategory after = ClassifyCollision(
            current.Definition.Traversal,
            nextLayout,
            new MapPosition(admitted.Copy.DestinationX, admitted.Copy.DestinationY));
        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapLayoutMutationReceipt receipt = new(
            admitted.Identity,
            admitted.Trigger,
            admitted.Copy,
            before,
            after,
            nextStep);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            nextLayout,
            nextStep,
            current.PlayerPosition,
            lastTraversal: null,
            controlledStepCopyApplied: true,
            receipt);
        _privateOriginalMapSnapshot = next;
        return new PrivateOriginalMapLayoutMutationApplied(next, receipt);
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
            accepted.Definition.WorkingLayout,
            simulationStep: 0,
            accepted.Definition.ControlledAdmission.Position,
            lastTraversal: null,
            controlledStepCopyApplied: false,
            lastLayoutMutation: null);
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

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedAreaProjection(definition.Traversal) ||
            !OriginalMapRuntimeAdmission.HasExactAcceptedAreaSourceProjection(
                definition.AreaCatalog) ||
            definition.Traversal.SelectActiveArea(controlled.Position)?.OneBasedRecordOrdinal !=
                OriginalMapRuntimeAdmission.ControlledStartAreaRecordOrdinal)
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.areaCatalog",
                "The admitted definition does not retain the exact ordered Map 3 area source projection.");
        }

        if (!OriginalMapRuntimeAdmission.IsExactControlledStepCopy(
                definition.ControlledStepCopy) ||
            !OriginalMapTraversal.IsBlocked(
                definition.WorkingLayout,
                new MapPosition(
                    OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
                    OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY)) ||
            OriginalMapTraversal.IsBlocked(
                definition.WorkingLayout,
                new MapPosition(
                    OriginalMapRuntimeAdmission.ControlledStepCopySourceX,
                    OriginalMapRuntimeAdmission.ControlledStepCopySourceY)))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.controlledStepCopy",
                "The admitted definition does not retain the exact controlled Map 3 step-copy projection.");
        }

        return null;
    }

    private static PrivateOriginalMapLayoutMutationRejected RejectLayoutMutation(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapLayoutMutationFailureCode code,
        string message) =>
        new(snapshot, new PrivateOriginalMapLayoutMutationDiagnostic(code, message));

    private static PrivateOriginalMapCollisionCategory ClassifyCollision(
        PrivateOriginalMapSessionSnapshot snapshot,
        MapPosition position) =>
        ClassifyCollision(snapshot.Definition.Traversal, snapshot.WorkingLayout, position);

    private static PrivateOriginalMapCollisionCategory ClassifyCollision(
        OriginalMapTraversal traversal,
        WorkingMapLayout layout,
        MapPosition position)
    {
        if (!traversal.IsWithinActiveArea(position))
        {
            return PrivateOriginalMapCollisionCategory.OutsideAcceptedActiveArea;
        }

        return OriginalMapTraversal.IsBlocked(layout, position)
            ? PrivateOriginalMapCollisionCategory.BlockedByAcceptedCollisionClass
            : PrivateOriginalMapCollisionCategory.ActiveNonBlocked;
    }

    private static OriginalMapImportDiagnostic Diagnostic(
        OriginalMapImportFailureCode code,
        string field,
        string message) =>
        new(code, field, message);
}

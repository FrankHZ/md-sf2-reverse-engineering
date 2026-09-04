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
        PrivateOriginalMapLayoutMutationReceipt? lastLayoutMutation,
        PrivateOriginalMapSameMapWarpReceipt? lastSameMapWarp = null,
        MapBlockCopyLifecycleState? roofOnLoadLifecycle = null,
        PrivateOriginalMapRoofOnLoadReceipt? lastRoofOnLoad = null,
        bool bowieDoorStepCopyApplied = false,
        PrivateOriginalMapNaturalStepCopyReceipt? lastNaturalStepCopy = null,
        bool schoolDoorStepCopyApplied = false,
        PrivateOriginalMapZone601State? zone601 = null,
        PrivateOriginalMapZone601Receipt? lastZone601 = null,
        PrivateOriginalMapSarahState? sarah = null,
        PrivateOriginalMapSarahReceipt? lastSarah = null)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
        WorkingLayout = workingLayout ?? throw new ArgumentNullException(nameof(workingLayout));
        ArgumentOutOfRangeException.ThrowIfNegative(simulationStep);
        PlayerPosition = playerPosition ?? throw new ArgumentNullException(nameof(playerPosition));
        MapBlockCopyLifecycleState admittedRoofLifecycle =
            roofOnLoadLifecycle ?? MapBlockCopyLifecycleState.Inactive;
        PrivateOriginalMapZone601State? admittedZone601 = zone601;
        if (definition.Zone601 is null)
        {
            if (zone601 is not null || lastZone601 is not null)
            {
                throw new ArgumentException(
                    "Zone 601 state requires its admitted definition.",
                    nameof(zone601));
            }
        }
        else
        {
            admittedZone601 ??= PrivateOriginalMapZone601State.Ready(definition.Zone601);
            if (!admittedZone601.Matches(definition.Zone601))
            {
                throw new ArgumentException(
                    "Zone 601 state must match its admitted definition.",
                    nameof(zone601));
            }
        }

        PrivateOriginalMapSarahState? admittedSarah = sarah;
        if (definition.Sarah is null)
        {
            if (sarah is not null || lastSarah is not null)
            {
                throw new ArgumentException(
                    "Sarah state requires its admitted definition.",
                    nameof(sarah));
            }
        }
        else
        {
            admittedSarah ??= PrivateOriginalMapSarahState.Ready(definition.Sarah);
            if (!admittedSarah.Matches(definition.Sarah))
            {
                throw new ArgumentException(
                    "Sarah state must match its admitted definition.",
                    nameof(sarah));
            }
        }

        definition.BlockCatalog.ValidateLayoutReferences(workingLayout, nameof(workingLayout));
        if (definition.Traversal.SelectActiveArea(playerPosition) is null ||
            OriginalMapTraversal.IsBlocked(workingLayout, playerPosition))
        {
            throw new ArgumentException(
                "The private original-map session position must remain active and traversable.",
                nameof(playerPosition));
        }

        int completedOperations =
            (lastTraversal is null ? 0 : 1) +
            (lastLayoutMutation is null ? 0 : 1) +
            (lastSameMapWarp is null ? 0 : 1) +
            (lastSarah is null ? 0 : 1);
        if (simulationStep == 0 &&
            (completedOperations != 0 ||
                controlledStepCopyApplied ||
                bowieDoorStepCopyApplied ||
                schoolDoorStepCopyApplied ||
                lastZone601 is not null ||
                admittedZone601?.Flag601Set == true ||
                lastSarah is not null ||
                admittedSarah?.TemporaryRouteFlag256Set == true ||
                admittedRoofLifecycle is not MapBlockCopyLifecycleInactiveState))
        {
            throw new ArgumentException(
                "The initial private original-map snapshot cannot contain a completed operation.",
                nameof(simulationStep));
        }

        if (simulationStep > 0 && completedOperations != 1)
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

        if (bowieDoorStepCopyApplied)
        {
            OriginalMapStepCopyDefinition admitted =
                definition.BowieDoorStepCopy ?? throw new ArgumentException(
                    "An applied Bowie-door step-copy requires its admitted definition.",
                    nameof(bowieDoorStepCopyApplied));
            if (workingLayout[admitted.Copy.DestinationX, admitted.Copy.DestinationY] !=
                workingLayout[admitted.Copy.SourceX, admitted.Copy.SourceY])
            {
                throw new ArgumentException(
                    "An applied Bowie-door step-copy requires the authoritative copied layout.",
                    nameof(workingLayout));
            }
        }

        if (schoolDoorStepCopyApplied)
        {
            OriginalMapStepCopyDefinition admitted =
                definition.ControlledStepCopy ?? throw new ArgumentException(
                    "An applied school-door step-copy requires its admitted definition.",
                    nameof(schoolDoorStepCopyApplied));
            if (workingLayout[admitted.Copy.DestinationX, admitted.Copy.DestinationY] !=
                workingLayout[admitted.Copy.SourceX, admitted.Copy.SourceY])
            {
                throw new ArgumentException(
                    "An applied school-door step-copy requires the authoritative copied layout.",
                    nameof(workingLayout));
            }
        }

        if (lastNaturalStepCopy is not null)
        {
            bool isBowieDoor =
                lastNaturalStepCopy.RecordIdentity == definition.BowieDoorStepCopy?.Identity;
            bool isSchoolDoor =
                lastNaturalStepCopy.RecordIdentity == definition.ControlledStepCopy?.Identity;
            OriginalMapStepCopyDefinition? admitted = isBowieDoor
                ? definition.BowieDoorStepCopy
                : isSchoolDoor
                    ? definition.ControlledStepCopy
                    : null;
            MapPosition expectedSource = isBowieDoor
                ? new MapPosition(
                    OriginalMapRuntimeAdmission.BowieDoorStepCopyApproachX,
                    OriginalMapRuntimeAdmission.BowieDoorStepCopyApproachY)
                : new MapPosition(
                    OriginalMapRuntimeAdmission.SchoolDoorStepCopyApproachX,
                    OriginalMapRuntimeAdmission.SchoolDoorStepCopyApproachY);
            ExplorationDirection expectedDirection = isBowieDoor
                ? OriginalMapRuntimeAdmission.BowieDoorStepCopyDirection
                : OriginalMapRuntimeAdmission.SchoolDoorStepCopyDirection;
            if (admitted is null ||
                isBowieDoor == isSchoolDoor ||
                (isBowieDoor && !bowieDoorStepCopyApplied) ||
                (isSchoolDoor && !schoolDoorStepCopyApplied) ||
                lastTraversal is null ||
                lastTraversal.Outcome != OriginalMapTraversalOutcome.Moved ||
                lastTraversal.Source != expectedSource ||
                lastTraversal.Direction != expectedDirection ||
                lastNaturalStepCopy.RecordIdentity != admitted.Identity ||
                lastNaturalStepCopy.Source != lastTraversal.Source ||
                lastNaturalStepCopy.Trigger != admitted.Trigger ||
                lastNaturalStepCopy.Trigger != playerPosition ||
                lastNaturalStepCopy.Copy != admitted.Copy ||
                lastNaturalStepCopy.BeforeCollision !=
                    PrivateOriginalMapCollisionCategory.BlockedByAcceptedCollisionClass ||
                lastNaturalStepCopy.AfterCollision !=
                    PrivateOriginalMapCollisionCategory.ActiveNonBlocked ||
                lastNaturalStepCopy.SimulationStep != simulationStep ||
                definition.Traversal.ResolveCandidateTarget(
                    workingLayout,
                    lastTraversal.Source,
                    lastTraversal.Direction) != admitted.Trigger)
            {
                throw new ArgumentException(
                    "The natural step-copy receipt must match the admitted atomic traversal.",
                    nameof(lastNaturalStepCopy));
            }
        }

        if (lastZone601 is not null)
        {
            OriginalMapZone601Definition admitted = definition.Zone601 ??
                throw new ArgumentException(
                    "A Zone 601 receipt requires its admitted definition.",
                    nameof(lastZone601));
            if (admittedZone601?.Flag601Set != true ||
                !lastZone601.Matches(admitted) ||
                lastTraversal is null ||
                lastTraversal.Outcome != OriginalMapTraversalOutcome.Moved ||
                lastTraversal.Source != lastZone601.PlayerSource ||
                lastTraversal.Position != lastZone601.CandidateTarget ||
                lastTraversal.Position != playerPosition ||
                lastZone601.SimulationStep != simulationStep)
            {
                throw new ArgumentException(
                    "The Zone 601 receipt must match the atomic traversal and persistent state.",
                    nameof(lastZone601));
            }
        }

        if (lastSarah is not null)
        {
            OriginalMapSarahDefinition admitted = definition.Sarah ??
                throw new ArgumentException(
                    "A Sarah receipt requires its admitted definition.",
                    nameof(lastSarah));
            if (admittedSarah is null ||
                !lastSarah.Matches(admitted) ||
                !ReferenceEquals(lastSarah.After, admittedSarah) ||
                lastSarah.PlayerPosition != playerPosition ||
                lastSarah.SimulationStep != simulationStep ||
                lastTraversal is not null)
            {
                throw new ArgumentException(
                    "The Sarah receipt must match the atomic interaction and persistent state.",
                    nameof(lastSarah));
            }
        }

        if (lastLayoutMutation is not null &&
            (!controlledStepCopyApplied || lastLayoutMutation.SimulationStep != simulationStep))
        {
            throw new ArgumentException(
                "The layout-mutation receipt must identify the authoritative snapshot step.",
                nameof(lastLayoutMutation));
        }

        if (lastSameMapWarp is not null &&
            (lastSameMapWarp.Destination != playerPosition ||
                lastSameMapWarp.SimulationStep != simulationStep))
        {
            throw new ArgumentException(
                "The same-map warp receipt must end at the authoritative snapshot step and position.",
                nameof(lastSameMapWarp));
        }

        if (lastSameMapWarp is not null)
        {
            OriginalMapSameMapWarpDefinition admitted =
                definition.SameMapWarps?.Select(definition.Map, lastSameMapWarp.Trigger) ??
                throw new ArgumentException(
                    "A same-map warp receipt requires its admitted record.",
                    nameof(lastSameMapWarp));
            if (admitted.Identity != lastSameMapWarp.RecordIdentity ||
                admitted.Destination != lastSameMapWarp.Destination ||
                admitted.OpaqueFacing != lastSameMapWarp.OpaqueFacing ||
                definition.Traversal.SelectActiveArea(lastSameMapWarp.Source)
                    ?.OneBasedRecordOrdinal != lastSameMapWarp.SourceAreaOrdinal ||
                definition.Traversal.SelectActiveArea(lastSameMapWarp.Destination)
                    ?.OneBasedRecordOrdinal != lastSameMapWarp.DestinationAreaOrdinal)
            {
                throw new ArgumentException(
                    "The same-map warp receipt does not match its admitted record and area projection.",
                    nameof(lastSameMapWarp));
            }
        }

        if (admittedRoofLifecycle is MapBlockCopyLifecycleActiveState activeRoof)
        {
            OriginalMapRoofOnLoadDefinition roof = definition.RoofOnLoadClear ??
                throw new ArgumentException(
                    "An active roof-on-load lifecycle requires its admitted definition.",
                    nameof(roofOnLoadLifecycle));
            if (activeRoof.RecordOrdinal != roof.Identity.OneBasedRecordOrdinal ||
                activeRoof.DestinationX != roof.ClearDestination.X ||
                activeRoof.DestinationY != roof.ClearDestination.Y ||
                activeRoof.Width != roof.Width ||
                activeRoof.Height != roof.Height ||
                activeRoof.SavedWords.Count != checked(roof.Width * roof.Height))
            {
                throw new ArgumentException(
                    "The active roof-on-load lifecycle does not match its admitted record.",
                    nameof(roofOnLoadLifecycle));
            }

            for (int y = 0; y < roof.Height; y++)
            {
                for (int x = 0; x < roof.Width; x++)
                {
                    if (workingLayout[roof.ClearDestination.X + x, roof.ClearDestination.Y + y] != 0)
                    {
                        throw new ArgumentException(
                            "An active roof-on-load lifecycle requires the authoritative cleared layout.",
                            nameof(workingLayout));
                    }
                }
            }
        }
        else if (admittedRoofLifecycle is not MapBlockCopyLifecycleInactiveState)
        {
            throw new ArgumentOutOfRangeException(nameof(roofOnLoadLifecycle));
        }

        if (lastRoofOnLoad is not null)
        {
            OriginalMapRoofOnLoadDefinition roof = definition.RoofOnLoadClear ??
                throw new ArgumentException(
                    "A roof-on-load receipt requires its admitted definition.",
                    nameof(lastRoofOnLoad));
            if (lastSameMapWarp is null ||
                lastRoofOnLoad.SimulationStep != simulationStep ||
                lastRoofOnLoad.RecordIdentity != roof.Identity ||
                lastRoofOnLoad.AppliedAfterWarp != lastSameMapWarp.RecordIdentity ||
                lastRoofOnLoad.DestinationArea != roof.DestinationArea ||
                lastRoofOnLoad.SourceTrigger != roof.SourceTrigger ||
                lastRoofOnLoad.ClearDestination != roof.ClearDestination ||
                lastRoofOnLoad.Width != roof.Width ||
                lastRoofOnLoad.Height != roof.Height ||
                admittedRoofLifecycle is not MapBlockCopyLifecycleActiveState)
            {
                throw new ArgumentException(
                    "The roof-on-load receipt must match the atomic warp snapshot and admitted record.",
                    nameof(lastRoofOnLoad));
            }
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
        LastSameMapWarp = lastSameMapWarp;
        RoofOnLoadLifecycle = admittedRoofLifecycle;
        LastRoofOnLoad = lastRoofOnLoad;
        BowieDoorStepCopyApplied = bowieDoorStepCopyApplied;
        LastNaturalStepCopy = lastNaturalStepCopy;
        SchoolDoorStepCopyApplied = schoolDoorStepCopyApplied;
        Zone601 = admittedZone601;
        LastZone601 = lastZone601;
        Sarah = admittedSarah;
        LastSarah = lastSarah;
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

    public OriginalMapEntityPopulation EntityPopulation => Definition.EntityPopulation;

    public OriginalMapBlockDefinition CurrentBlockDefinition =>
        Definition.BlockCatalog.Resolve(WorkingLayout, PlayerPosition);

    public long SimulationStep { get; }

    public OriginalMapTraversalResult? LastTraversal { get; }

    public bool ControlledStepCopyApplied { get; }

    public PrivateOriginalMapLayoutMutationReceipt? LastLayoutMutation { get; }

    public PrivateOriginalMapSameMapWarpReceipt? LastSameMapWarp { get; }

    public MapBlockCopyLifecycleState RoofOnLoadLifecycle { get; }

    public PrivateOriginalMapRoofOnLoadReceipt? LastRoofOnLoad { get; }

    public bool BowieDoorStepCopyApplied { get; }

    public PrivateOriginalMapNaturalStepCopyReceipt? LastNaturalStepCopy { get; }

    public bool SchoolDoorStepCopyApplied { get; }

    public PrivateOriginalMapZone601State? Zone601 { get; }

    public PrivateOriginalMapZone601Receipt? LastZone601 { get; }

    public PrivateOriginalMapSarahState? Sarah { get; }

    public PrivateOriginalMapSarahReceipt? LastSarah { get; }
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

public sealed record PrivateOriginalMapMoveApplied
{
    private readonly OriginalMapTraversalResult? _traversal;

    public PrivateOriginalMapMoveApplied(
        PrivateOriginalMapSessionSnapshot snapshot,
        OriginalMapTraversalResult traversal,
        PrivateOriginalMapZone601Receipt? zone601 = null)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        _traversal = traversal ?? throw new ArgumentNullException(nameof(traversal));
        if (zone601 is not null && !ReferenceEquals(snapshot.LastZone601, zone601))
        {
            throw new ArgumentException(
                "A Zone 601 movement result must expose the snapshot's exact receipt.",
                nameof(zone601));
        }

        Zone601 = zone601;
    }

    public PrivateOriginalMapMoveApplied(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapSameMapWarpReceipt sameMapWarp,
        PrivateOriginalMapRoofOnLoadReceipt? roofOnLoad = null)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        SameMapWarp = sameMapWarp ?? throw new ArgumentNullException(nameof(sameMapWarp));
        RoofOnLoad = roofOnLoad;
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public OriginalMapTraversalResult Traversal => _traversal ??
        throw new InvalidOperationException(
            "A same-map warp outcome does not contain an ordinary traversal result.");

    public PrivateOriginalMapSameMapWarpReceipt? SameMapWarp { get; }

    public PrivateOriginalMapRoofOnLoadReceipt? RoofOnLoad { get; }

    public PrivateOriginalMapZone601Receipt? Zone601 { get; }
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
        if (IsPrivateOriginalMapBattleBridgeBusy)
        {
            throw new InvalidOperationException(
                "Private original-map movement is unavailable while the project-authored battle bridge is busy.");
        }

        PrivateOriginalMapSessionSnapshot current = PrivateOriginalMapSnapshot;
        if (TryApplyPrivateOriginalMapSameMapWarp(current, command, out var warpApplied))
        {
            return warpApplied!;
        }

        if (TryApplyPrivateOriginalMapZone601(current, command, out var zone601Applied))
        {
            return zone601Applied!;
        }

        if (TryApplyPrivateOriginalMapNaturalStepCopy(
                current,
                command,
                out var stepCopyApplied))
        {
            return stepCopyApplied!;
        }

        MapPosition? candidate = current.Definition.Traversal.ResolveCandidateTarget(
            current.WorkingLayout,
            current.PlayerPosition,
            command.Direction);
        if (candidate is MapPosition occupiedPosition &&
            occupiedPosition == current.Sarah?.ActorPosition)
        {
            ushort sourceWord = current.WorkingLayout[
                current.PlayerPosition.X,
                current.PlayerPosition.Y];
            ushort destinationWord = current.WorkingLayout[
                occupiedPosition.X,
                occupiedPosition.Y];
            OriginalMapTraversalResult occupied = new(
                current.PlayerPosition,
                current.PlayerPosition,
                command.Direction,
                OriginalMapTraversalOutcome.BlockedByOccupiedEntity,
                sourceWord,
                destinationWord);
            PrivateOriginalMapSessionSnapshot blocked = new(
                current.Definition,
                current.Receipt,
                current.WorkingLayout,
                checked(current.SimulationStep + 1),
                current.PlayerPosition,
                occupied,
                current.ControlledStepCopyApplied,
                lastLayoutMutation: null,
                lastSameMapWarp: null,
                roofOnLoadLifecycle: current.RoofOnLoadLifecycle,
                lastRoofOnLoad: null,
                current.BowieDoorStepCopyApplied,
                lastNaturalStepCopy: null,
                current.SchoolDoorStepCopyApplied,
                current.Zone601,
                lastZone601: null,
                current.Sarah,
                lastSarah: null);
            _privateOriginalMapSnapshot = blocked;
            return new PrivateOriginalMapMoveApplied(blocked, occupied);
        }

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
            lastLayoutMutation: null,
            lastSameMapWarp: null,
            roofOnLoadLifecycle: current.RoofOnLoadLifecycle,
            lastRoofOnLoad: null,
            current.BowieDoorStepCopyApplied,
            lastNaturalStepCopy: null,
            current.SchoolDoorStepCopyApplied,
            current.Zone601,
            lastZone601: null,
            current.Sarah,
            lastSarah: null);
        _privateOriginalMapSnapshot = next;
        return new PrivateOriginalMapMoveApplied(next, traversal);
    }

    public PrivateOriginalMapLayoutMutationResult ApplyPrivateOriginalMapLayoutMutation(
        ApplyPrivateOriginalMapLayoutMutationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (IsPrivateOriginalMapBattleBridgeBusy)
        {
            throw new InvalidOperationException(
                "Private original-map layout mutation is unavailable while the project-authored battle bridge is busy.");
        }

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
            receipt,
            lastSameMapWarp: null,
            roofOnLoadLifecycle: current.RoofOnLoadLifecycle,
            lastRoofOnLoad: null,
            current.BowieDoorStepCopyApplied,
            lastNaturalStepCopy: null,
            current.SchoolDoorStepCopyApplied,
            current.Zone601,
            lastZone601: null,
            current.Sarah,
            lastSarah: null);
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
            lastLayoutMutation: null,
            lastSameMapWarp: null,
            roofOnLoadLifecycle: MapBlockCopyLifecycleState.Inactive,
            lastRoofOnLoad: null,
            bowieDoorStepCopyApplied: false,
            lastNaturalStepCopy: null,
            schoolDoorStepCopyApplied: false);
        GameSession session = new(snapshot);
        session.InitializePrivateOriginalMapPlayerLocomotion();
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

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedBlocksetProjection(
                definition.BlockCatalog))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.blockCatalog",
                "The admitted definition does not retain the exact ordered Map 3 blockset projection.");
        }

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedSameMapWarps(
                definition.SameMapWarps))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.sameMapWarps",
                "The admitted definition does not retain the exact bounded Map 3 same-map warp projection.");
        }

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedRoofOnLoadClear(
                definition.RoofOnLoadClear))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.roofOnLoadClear",
                "The admitted definition does not retain the exact bounded Map 3 roof-on-load clear projection.");
        }

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedBowieDoorStepCopy(
                definition.BowieDoorStepCopy) ||
            !OriginalMapTraversal.IsBlocked(
                definition.WorkingLayout,
                new MapPosition(
                    OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationX,
                    OriginalMapRuntimeAdmission.BowieDoorStepCopyDestinationY)) ||
            OriginalMapTraversal.IsBlocked(
                definition.WorkingLayout,
                new MapPosition(
                    OriginalMapRuntimeAdmission.BowieDoorStepCopySourceX,
                    OriginalMapRuntimeAdmission.BowieDoorStepCopySourceY)))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.bowieDoorStepCopy",
                "The admitted definition does not retain the exact natural Bowie-door step-copy projection.");
        }

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedVisualResourceSelection(
                definition.VisualResourceSelection))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.visualResourceSelection",
                "The admitted definition does not retain the exact Map 3 palette and tileset reference projection.");
        }

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedEntityPopulation(
                definition.EntityPopulation))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.entityPopulation",
                "The admitted definition does not retain the exact selected-setup Map 3 entity population.");
        }

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedZone601(
                definition.Zone601,
                definition.EntityPopulation,
                definition.Traversal,
                definition.WorkingLayout))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.zone601",
                "The admitted definition does not retain the exact bounded Map 3 Zone 601 projection.");
        }

        if (!OriginalMapRuntimeAdmission.HasExactAcceptedSarah(
                definition.Sarah,
                definition.EntityPopulation,
                definition.Traversal,
                definition.WorkingLayout))
        {
            return Diagnostic(
                OriginalMapImportFailureCode.InvalidMapProjection,
                "definition.sarah",
                "The admitted definition does not retain the exact bounded Map 3 Sarah route projection.");
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

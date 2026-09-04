using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum PrivateOriginalMapSarahLifecyclePhase
{
    Ready,
    RouteCleared,
    AstralZoneRepositioned,
    MessengerFollowerReady,
}

public sealed record PrivateOriginalMapSarahState
{
    private PrivateOriginalMapSarahState(
        PrivateOriginalMapSarahLifecyclePhase phase,
        OriginalMapEntityRecordIdentity actorSourceRecord,
        int logicalActorId,
        MapPosition actorPosition,
        byte actorOpaqueFacing,
        bool temporaryRouteFlag256Set,
        bool astralZoneFlag260Set)
    {
        if (!Enum.IsDefined(phase))
        {
            throw new ArgumentOutOfRangeException(nameof(phase));
        }

        ActorSourceRecord = actorSourceRecord ??
            throw new ArgumentNullException(nameof(actorSourceRecord));
        ArgumentOutOfRangeException.ThrowIfNegative(logicalActorId);
        ActorPosition = actorPosition ?? throw new ArgumentNullException(nameof(actorPosition));
        if (actorOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(actorOpaqueFacing));
        }

        if ((phase == PrivateOriginalMapSarahLifecyclePhase.Ready &&
                (temporaryRouteFlag256Set || astralZoneFlag260Set)) ||
            (phase == PrivateOriginalMapSarahLifecyclePhase.RouteCleared &&
                (!temporaryRouteFlag256Set || astralZoneFlag260Set)) ||
            ((phase == PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned ||
                    phase == PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady) &&
                (!temporaryRouteFlag256Set || !astralZoneFlag260Set)))
        {
            throw new ArgumentException(
                "Sarah state must retain the exact ready, route-cleared, or Astral-zone shape.");
        }

        Phase = phase;
        LogicalActorId = logicalActorId;
        ActorOpaqueFacing = actorOpaqueFacing;
        TemporaryRouteFlag256Set = temporaryRouteFlag256Set;
        AstralZoneFlag260Set = astralZoneFlag260Set;
    }

    public PrivateOriginalMapSarahLifecyclePhase Phase { get; }

    public OriginalMapEntityRecordIdentity ActorSourceRecord { get; }

    public int LogicalActorId { get; }

    public MapPosition ActorPosition { get; }

    public byte ActorOpaqueFacing { get; }

    public bool TemporaryRouteFlag256Set { get; }

    public bool AstralZoneFlag260Set { get; }

    public bool IsMessengerFollowerReady =>
        Phase == PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady;

    public bool OccupiesRouteTile => !IsMessengerFollowerReady;

    internal static PrivateOriginalMapSarahState Ready(OriginalMapSarahDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(
            PrivateOriginalMapSarahLifecyclePhase.Ready,
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            definition.ActorInitialPosition,
            definition.ActorInitialOpaqueFacing,
            temporaryRouteFlag256Set: false,
            astralZoneFlag260Set: false);
    }

    internal static PrivateOriginalMapSarahState RouteCleared(
        OriginalMapSarahDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(
            PrivateOriginalMapSarahLifecyclePhase.RouteCleared,
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            definition.FirstInteractionWaypoint,
            definition.RestoredOpaqueFacing,
            temporaryRouteFlag256Set: true,
            astralZoneFlag260Set: false);
    }

    internal static PrivateOriginalMapSarahState AstralZoneRepositioned(
        OriginalMapSarahDefinition definition,
        OriginalMapAstralZoneDefinition astralZone)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(astralZone);
        if (astralZone.SarahSourceRecord != definition.ActorSourceRecord ||
            astralZone.SarahLogicalActorId != definition.LogicalActorId)
        {
            throw new ArgumentException(
                "The Astral-zone handoff must bind the admitted Sarah actor.",
                nameof(astralZone));
        }

        return new(
            PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned,
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            astralZone.SarahDestination,
            astralZone.SarahOpaqueFacing,
            temporaryRouteFlag256Set: true,
            astralZoneFlag260Set: true);
    }

    internal static PrivateOriginalMapSarahState MessengerFollowerReady(
        OriginalMapSarahDefinition definition,
        OriginalMapAstralZoneDefinition astralZone,
        OriginalMapMessengerAcceptanceDefinition messenger)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(astralZone);
        ArgumentNullException.ThrowIfNull(messenger);
        if (messenger.SarahSourceRecord != definition.ActorSourceRecord ||
            messenger.SarahCharacterId != definition.LogicalActorId ||
            !messenger.Followers.Any(link =>
                link.FollowerId == definition.LogicalActorId))
        {
            throw new ArgumentException(
                "Messenger acceptance must bind Sarah's admitted follower state.",
                nameof(messenger));
        }

        PrivateOriginalMapSarahState astral = AstralZoneRepositioned(definition, astralZone);
        return new(
            PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady,
            astral.ActorSourceRecord,
            astral.LogicalActorId,
            astral.ActorPosition,
            astral.ActorOpaqueFacing,
            temporaryRouteFlag256Set: true,
            astralZoneFlag260Set: true);
    }

    internal bool Matches(
        OriginalMapSarahDefinition definition,
        OriginalMapAstralZoneDefinition? astralZone = null,
        OriginalMapMessengerAcceptanceDefinition? messenger = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return this == (Phase switch
        {
            PrivateOriginalMapSarahLifecyclePhase.Ready => Ready(definition),
            PrivateOriginalMapSarahLifecyclePhase.RouteCleared => RouteCleared(definition),
            PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned =>
                AstralZoneRepositioned(
                    definition,
                    astralZone ?? throw new InvalidOperationException(
                        "Astral-zone Sarah state requires its admitted definition.")),
            PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady =>
                MessengerFollowerReady(
                    definition,
                    astralZone ?? throw new InvalidOperationException(
                        "Messenger follower state requires its admitted Astral definition."),
                    messenger ?? throw new InvalidOperationException(
                        "Messenger follower state requires its admitted definition.")),
            _ => throw new InvalidOperationException("Unknown Sarah lifecycle phase."),
        });
    }
}

public sealed record InteractPrivateOriginalMapSarahCommand(long ExpectedSimulationStep)
{
    public long ExpectedSimulationStep { get; } =
        ExpectedSimulationStep >= 0
            ? ExpectedSimulationStep
            : throw new ArgumentOutOfRangeException(nameof(ExpectedSimulationStep));
}

public sealed record PrivateOriginalMapSarahReceipt
{
    private readonly ReadOnlyCollection<int> _textIds;
    private readonly ReadOnlyCollection<OriginalMapSarahInteractionStage> _stages;

    internal PrivateOriginalMapSarahReceipt(
        OriginalMapSarahDefinition definition,
        MapPosition playerPosition,
        byte playerOpaqueFacing,
        PrivateOriginalMapSarahState before,
        PrivateOriginalMapSarahState after,
        bool repeated,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        PlayerPosition = playerPosition ?? throw new ArgumentNullException(nameof(playerPosition));
        Before = before ?? throw new ArgumentNullException(nameof(before));
        After = after ?? throw new ArgumentNullException(nameof(after));
        if (playerOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(playerOpaqueFacing));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        EventIdentity = definition.Identity;
        PlayerOpaqueFacing = playerOpaqueFacing;
        Repeated = repeated;
        LaterBranchFlag603 = definition.LaterBranchFlag603;
        LaterBranchFlag602 = definition.LaterBranchFlag602;
        TemporaryRouteFlag256 = definition.TemporaryRouteFlag256;
        BlockingSequenceIdentity = definition.BlockingSequenceIdentity;
        _textIds = Array.AsReadOnly((repeated
            ? definition.RepeatInteractionTextIds
            : definition.FirstInteractionTextIds).ToArray());
        _stages = Array.AsReadOnly((repeated
            ? definition.RepeatInteractionStages
            : definition.FirstInteractionStages).ToArray());
        SimulationStep = simulationStep;
    }

    public OriginalMapSarahEventIdentity EventIdentity { get; }

    public MapPosition PlayerPosition { get; }

    public byte PlayerOpaqueFacing { get; }

    public PrivateOriginalMapSarahState Before { get; }

    public PrivateOriginalMapSarahState After { get; }

    public bool Repeated { get; }

    public int LaterBranchFlag603 { get; }

    public bool LaterBranchFlag603Set => false;

    public int LaterBranchFlag602 { get; }

    public bool LaterBranchFlag602Set => false;

    public int TemporaryRouteFlag256 { get; }

    public string BlockingSequenceIdentity { get; }

    public IReadOnlyList<int> TextIds => _textIds;

    public IReadOnlyList<OriginalMapSarahInteractionStage> Stages => _stages;

    public long SimulationStep { get; }

    internal bool Matches(OriginalMapSarahDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return EventIdentity == definition.Identity &&
            Before.Matches(definition) &&
            After.Matches(definition) &&
            LaterBranchFlag603 == definition.LaterBranchFlag603 &&
            LaterBranchFlag602 == definition.LaterBranchFlag602 &&
            TemporaryRouteFlag256 == definition.TemporaryRouteFlag256 &&
            string.Equals(
                BlockingSequenceIdentity,
                definition.BlockingSequenceIdentity,
                StringComparison.Ordinal) &&
            TextIds.SequenceEqual(Repeated
                ? definition.RepeatInteractionTextIds
                : definition.FirstInteractionTextIds) &&
            Stages.SequenceEqual(Repeated
                ? definition.RepeatInteractionStages
                : definition.FirstInteractionStages) &&
            Repeated == (Before.Phase == PrivateOriginalMapSarahLifecyclePhase.RouteCleared) &&
            (Repeated ? ReferenceEquals(Before, After) :
                After == PrivateOriginalMapSarahState.RouteCleared(definition));
    }
}

public enum PrivateOriginalMapSarahInteractionFailureCode
{
    StaleSimulationStep,
    LocomotionBusy,
    BattleBridgeBusy,
    InteractionTargetMismatch,
    UnsupportedLaterBranchState,
}

public sealed record PrivateOriginalMapSarahInteractionDiagnostic(
    PrivateOriginalMapSarahInteractionFailureCode Code,
    string Message)
{
    public string Message { get; } = !string.IsNullOrWhiteSpace(Message)
        ? Message
        : throw new ArgumentException("A Sarah interaction diagnostic requires a message.", nameof(Message));
}

public abstract record PrivateOriginalMapSarahInteractionResult;

public sealed record PrivateOriginalMapSarahInteractionApplied :
    PrivateOriginalMapSarahInteractionResult
{
    internal PrivateOriginalMapSarahInteractionApplied(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapSarahReceipt receipt)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
        if (!ReferenceEquals(snapshot.LastSarah, receipt))
        {
            throw new ArgumentException(
                "A Sarah interaction result must expose the snapshot's exact receipt.",
                nameof(receipt));
        }
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public PrivateOriginalMapSarahReceipt Receipt { get; }
}

public sealed record PrivateOriginalMapSarahInteractionRejected :
    PrivateOriginalMapSarahInteractionResult
{
    internal PrivateOriginalMapSarahInteractionRejected(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapSarahInteractionDiagnostic diagnostic)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Diagnostic = diagnostic ?? throw new ArgumentNullException(nameof(diagnostic));
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public PrivateOriginalMapSarahInteractionDiagnostic Diagnostic { get; }
}

public sealed partial class GameSession
{
    public PrivateOriginalMapSarahInteractionResult InteractPrivateOriginalMapSarah(
        InteractPrivateOriginalMapSarahCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        PrivateOriginalMapSessionSnapshot current = PrivateOriginalMapSnapshot;
        if (command.ExpectedSimulationStep != current.SimulationStep)
        {
            return RejectSarah(
                current,
                PrivateOriginalMapSarahInteractionFailureCode.StaleSimulationStep,
                "The Sarah interaction targets a stale private Map 3 simulation step.");
        }

        if (IsPrivateOriginalMapBattleBridgeBusy)
        {
            return RejectSarah(
                current,
                PrivateOriginalMapSarahInteractionFailureCode.BattleBridgeBusy,
                "Sarah interaction is unavailable while the battle bridge is busy.");
        }

        PrivateOriginalMapPlayerLocomotionSnapshot locomotion =
            PrivateOriginalMapPlayerLocomotion;
        if (locomotion.IsMoving)
        {
            return RejectSarah(
                current,
                PrivateOriginalMapSarahInteractionFailureCode.LocomotionBusy,
                "Sarah interaction is unavailable during player locomotion.");
        }

        OriginalMapSarahDefinition definition = current.Definition.Sarah ??
            throw new InvalidOperationException(
                "The admitted private Map 3 definition has no Sarah route.");
        PrivateOriginalMapSarahState before = current.Sarah ??
            throw new InvalidOperationException("The private Map 3 snapshot has no Sarah state.");
        MapPosition? target = FacingTarget(current.PlayerPosition, locomotion.OpaqueFacing);
        bool initialTarget = before.Phase == PrivateOriginalMapSarahLifecyclePhase.Ready &&
            current.PlayerPosition == definition.PlayerInteractionPosition &&
            locomotion.OpaqueFacing == definition.PlayerInteractionOpaqueFacing;
        if (!before.OccupiesRouteTile || target != before.ActorPosition ||
            (before.Phase == PrivateOriginalMapSarahLifecyclePhase.Ready && !initialTarget))
        {
            return RejectSarah(
                current,
                PrivateOriginalMapSarahInteractionFailureCode.InteractionTargetMismatch,
                "The semantic interaction does not select the live Sarah actor.");
        }

        if (current.Entity142?.Flag602Set == true)
        {
            return RejectSarah(
                current,
                PrivateOriginalMapSarahInteractionFailureCode.UnsupportedLaterBranchState,
                "Sarah's later flag-602 branch remains outside this bounded private Map 3 slice.");
        }

        bool repeated = before.Phase == PrivateOriginalMapSarahLifecyclePhase.RouteCleared;
        PrivateOriginalMapSarahState after = repeated
            ? before
            : PrivateOriginalMapSarahState.RouteCleared(definition);
        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapSarahReceipt receipt = new(
            definition,
            current.PlayerPosition,
            locomotion.OpaqueFacing,
            before,
            after,
            repeated,
            nextStep);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            current.WorkingLayout,
            nextStep,
            current.PlayerPosition,
            lastTraversal: null,
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
            after,
            receipt,
            current.Entity142,
            pendingEntity142: null,
            lastEntity142Request: null,
            lastEntity142Acknowledgement: null);
        _privateOriginalMapSnapshot = next;
        return new PrivateOriginalMapSarahInteractionApplied(next, receipt);
    }

    private static MapPosition? FacingTarget(MapPosition source, byte opaqueFacing)
    {
        (int deltaX, int deltaY) = opaqueFacing switch
        {
            0 => (1, 0),
            1 => (0, -1),
            2 => (-1, 0),
            3 => (0, 1),
            _ => throw new InvalidOperationException("Unknown private Map 3 player facing."),
        };
        int x = source.X + deltaX;
        int y = source.Y + deltaY;
        return x is >= 0 and < WorkingMapLayout.ColumnCount &&
            y is >= 0 and < WorkingMapLayout.RowCount
            ? new MapPosition(x, y)
            : null;
    }

    private static PrivateOriginalMapSarahInteractionRejected RejectSarah(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapSarahInteractionFailureCode code,
        string message) =>
        new(snapshot, new PrivateOriginalMapSarahInteractionDiagnostic(code, message));
}

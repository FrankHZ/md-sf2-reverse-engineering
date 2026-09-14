using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapEntity142State
{
    private PrivateOriginalMapEntity142State(
        OriginalMapEntityRecordIdentity actorSourceRecord,
        int logicalActorId,
        int physicalActorSlot,
        MapPosition actorPosition,
        byte actorOpaqueFacing,
        bool flag261Set,
        bool flag602Set,
        long lastAcknowledgedRequestSequence,
        bool routeOccupancyReleased)
    {
        ActorSourceRecord = actorSourceRecord ??
            throw new ArgumentNullException(nameof(actorSourceRecord));
        ArgumentOutOfRangeException.ThrowIfNegative(logicalActorId);
        ArgumentOutOfRangeException.ThrowIfLessThan(physicalActorSlot, 1);
        ActorPosition = actorPosition ?? throw new ArgumentNullException(nameof(actorPosition));
        if (actorOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(actorOpaqueFacing));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(lastAcknowledgedRequestSequence);
        if (flag261Set != flag602Set || flag261Set != (lastAcknowledgedRequestSequence > 0) ||
            (routeOccupancyReleased && !flag602Set))
        {
            throw new ArgumentException(
                "Entity 142 state must retain the exact ready or acknowledged flag shape.");
        }

        LogicalActorId = logicalActorId;
        PhysicalActorSlot = physicalActorSlot;
        ActorOpaqueFacing = actorOpaqueFacing;
        Flag261Set = flag261Set;
        Flag602Set = flag602Set;
        LastAcknowledgedRequestSequence = lastAcknowledgedRequestSequence;
        RouteOccupancyReleased = routeOccupancyReleased;
    }

    public OriginalMapEntityRecordIdentity ActorSourceRecord { get; }

    public int LogicalActorId { get; }

    public int PhysicalActorSlot { get; }

    public MapPosition ActorPosition { get; }

    public byte ActorOpaqueFacing { get; }

    public bool Flag261Set { get; }

    public bool Flag602Set { get; }

    public long LastAcknowledgedRequestSequence { get; }

    public bool RouteOccupancyReleased { get; }

    public bool OccupiesRouteTile => !RouteOccupancyReleased;

    internal static PrivateOriginalMapEntity142State Ready(
        OriginalMapEntity142Definition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            definition.PhysicalActorSlot,
            definition.ActorPosition,
            definition.ActorOpaqueFacing,
            flag261Set: false,
            flag602Set: false,
            lastAcknowledgedRequestSequence: 0,
            routeOccupancyReleased: false);
    }

    internal static PrivateOriginalMapEntity142State Acknowledged(
        OriginalMapEntity142Definition definition,
        long requestSequence)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentOutOfRangeException.ThrowIfLessThan(requestSequence, 1);
        return new(
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            definition.PhysicalActorSlot,
            definition.ActorPosition,
            definition.ActorOpaqueFacing,
            flag261Set: true,
            flag602Set: true,
            requestSequence,
            routeOccupancyReleased: false);
    }

    internal static PrivateOriginalMapEntity142State ReleaseRouteOccupancy(
        OriginalMapEntity142Definition definition,
        PrivateOriginalMapEntity142State before,
        OriginalMapMessengerAcceptanceDefinition messenger)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(messenger);
        if (!before.Matches(definition) || !before.Flag602Set ||
            before.RouteOccupancyReleased ||
            messenger.Entity142SourceRecord != definition.ActorSourceRecord ||
            messenger.Entity142LogicalActorId != definition.LogicalActorId)
        {
            throw new ArgumentException(
                "Messenger acceptance must release the acknowledged Entity 142 route actor.",
                nameof(before));
        }

        return new(
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            definition.PhysicalActorSlot,
            definition.ActorPosition,
            definition.ActorOpaqueFacing,
            flag261Set: true,
            flag602Set: true,
            before.LastAcknowledgedRequestSequence,
            routeOccupancyReleased: true);
    }

    internal bool Matches(
        OriginalMapEntity142Definition definition,
        OriginalMapMessengerAcceptanceDefinition? messenger = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return this == (RouteOccupancyReleased
            ? ReleaseRouteOccupancy(
                definition,
                Acknowledged(definition, LastAcknowledgedRequestSequence),
                messenger ?? throw new InvalidOperationException(
                    "Released Entity 142 state requires its admitted messenger definition."))
            : Flag261Set
                ? Acknowledged(definition, LastAcknowledgedRequestSequence)
                : Ready(definition));
    }
}

public sealed record RequestPrivateOriginalMapEntity142Command(long ExpectedSimulationStep)
{
    public long ExpectedSimulationStep { get; } =
        ExpectedSimulationStep >= 0
            ? ExpectedSimulationStep
            : throw new ArgumentOutOfRangeException(nameof(ExpectedSimulationStep));
}

public sealed record AcknowledgePrivateOriginalMapEntity142Command
{
    public AcknowledgePrivateOriginalMapEntity142Command(
        long expectedSimulationStep,
        long requestSequence,
        OriginalMapEntity142EventIdentity eventIdentity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expectedSimulationStep);
        ArgumentOutOfRangeException.ThrowIfLessThan(requestSequence, 1);
        ExpectedSimulationStep = expectedSimulationStep;
        RequestSequence = requestSequence;
        EventIdentity = eventIdentity ?? throw new ArgumentNullException(nameof(eventIdentity));
    }

    public long ExpectedSimulationStep { get; }

    public long RequestSequence { get; }

    public OriginalMapEntity142EventIdentity EventIdentity { get; }
}

public sealed record PrivateOriginalMapEntity142Request
{
    private readonly ReadOnlyCollection<int> _textIds;
    private readonly ReadOnlyCollection<OriginalMapEntity142InteractionStage> _stages;

    internal PrivateOriginalMapEntity142Request(
        OriginalMapEntity142Definition definition,
        PrivateOriginalMapEntity142State state,
        MapPosition playerPosition,
        byte playerOpaqueFacing,
        long requestSequence,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        StateBefore = state ?? throw new ArgumentNullException(nameof(state));
        PlayerPosition = playerPosition ?? throw new ArgumentNullException(nameof(playerPosition));
        if (playerOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(playerOpaqueFacing));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(requestSequence, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        EventIdentity = definition.Identity;
        ActorSourceRecord = definition.ActorSourceRecord;
        LogicalActorId = definition.LogicalActorId;
        PhysicalActorSlot = definition.PhysicalActorSlot;
        ActorPosition = definition.ActorPosition;
        ActorOpaqueFacing = definition.ActorOpaqueFacing;
        PlayerOpaqueFacing = playerOpaqueFacing;
        Repeated = state.Flag261Set;
        _textIds = Array.AsReadOnly((Repeated
            ? definition.RepeatInteractionTextIds
            : definition.FirstInteractionTextIds).ToArray());
        _stages = Array.AsReadOnly((Repeated
            ? definition.RepeatInteractionStages
            : definition.FirstInteractionStages).ToArray());
        RequestSequence = requestSequence;
        SimulationStep = simulationStep;
    }

    public OriginalMapEntity142EventIdentity EventIdentity { get; }

    public OriginalMapEntityRecordIdentity ActorSourceRecord { get; }

    public int LogicalActorId { get; }

    public int PhysicalActorSlot { get; }

    public MapPosition ActorPosition { get; }

    public byte ActorOpaqueFacing { get; }

    public MapPosition PlayerPosition { get; }

    public byte PlayerOpaqueFacing { get; }

    public PrivateOriginalMapEntity142State StateBefore { get; }

    public bool Repeated { get; }

    public IReadOnlyList<int> TextIds => _textIds;

    public IReadOnlyList<OriginalMapEntity142InteractionStage> Stages => _stages;

    public long RequestSequence { get; }

    public long SimulationStep { get; }

    internal bool Matches(OriginalMapEntity142Definition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return EventIdentity == definition.Identity &&
            ActorSourceRecord == definition.ActorSourceRecord &&
            LogicalActorId == definition.LogicalActorId &&
            PhysicalActorSlot == definition.PhysicalActorSlot &&
            ActorPosition == definition.ActorPosition &&
            ActorOpaqueFacing == definition.ActorOpaqueFacing &&
            PlayerPosition == definition.PlayerInteractionPosition &&
            PlayerOpaqueFacing == definition.PlayerInteractionOpaqueFacing &&
            StateBefore.Matches(definition) &&
            Repeated == StateBefore.Flag261Set &&
            TextIds.SequenceEqual(Repeated
                ? definition.RepeatInteractionTextIds
                : definition.FirstInteractionTextIds) &&
            Stages.SequenceEqual(Repeated
                ? definition.RepeatInteractionStages
                : definition.FirstInteractionStages) &&
            RequestSequence == checked(StateBefore.LastAcknowledgedRequestSequence + 1);
    }
}

public sealed record PrivateOriginalMapEntity142AcknowledgementReceipt
{
    internal PrivateOriginalMapEntity142AcknowledgementReceipt(
        OriginalMapEntity142Definition definition,
        PrivateOriginalMapEntity142Request request,
        PrivateOriginalMapEntity142State before,
        PrivateOriginalMapEntity142State after,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Before = before ?? throw new ArgumentNullException(nameof(before));
        After = after ?? throw new ArgumentNullException(nameof(after));
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        FirstInteractionFlag261 = definition.FirstInteractionFlag261;
        CompletionFlag602 = definition.CompletionFlag602;
        SimulationStep = simulationStep;
    }

    public PrivateOriginalMapEntity142Request Request { get; }

    public PrivateOriginalMapEntity142State Before { get; }

    public PrivateOriginalMapEntity142State After { get; }

    public int FirstInteractionFlag261 { get; }

    public int CompletionFlag602 { get; }

    public long SimulationStep { get; }

    internal bool Matches(OriginalMapEntity142Definition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return Request.Matches(definition) &&
            ReferenceEquals(Request.StateBefore, Before) &&
            Before.Matches(definition) &&
            After == PrivateOriginalMapEntity142State.Acknowledged(
                definition,
                Request.RequestSequence) &&
            FirstInteractionFlag261 == definition.FirstInteractionFlag261 &&
            CompletionFlag602 == definition.CompletionFlag602 &&
            SimulationStep == checked(Request.SimulationStep + 1);
    }
}

public enum PrivateOriginalMapEntity142RequestFailureCode
{
    StaleSimulationStep,
    LocomotionBusy,
    BattleBridgeBusy,
    PendingRequestExists,
    InteractionTargetMismatch,
}

public enum PrivateOriginalMapEntity142AcknowledgementFailureCode
{
    StaleSimulationStep,
    LocomotionBusy,
    BattleBridgeBusy,
    NoPendingRequest,
    ReferenceMismatch,
}

public sealed record PrivateOriginalMapEntity142Diagnostic<TCode>(TCode Code, string Message)
    where TCode : struct, Enum
{
    public TCode Code { get; } = Enum.IsDefined(Code)
        ? Code
        : throw new ArgumentOutOfRangeException(nameof(Code));

    public string Message { get; } = !string.IsNullOrWhiteSpace(Message)
        ? Message
        : throw new ArgumentException(
            "An Entity 142 lifecycle diagnostic requires a message.",
            nameof(Message));
}

public abstract record PrivateOriginalMapEntity142RequestResult;

public sealed record PrivateOriginalMapEntity142RequestApplied :
    PrivateOriginalMapEntity142RequestResult
{
    internal PrivateOriginalMapEntity142RequestApplied(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapEntity142Request request)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        if (!ReferenceEquals(snapshot.PendingEntity142, request) ||
            !ReferenceEquals(snapshot.LastEntity142Request, request))
        {
            throw new ArgumentException(
                "An Entity 142 request result must expose the snapshot's exact pending request.",
                nameof(request));
        }
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public PrivateOriginalMapEntity142Request Request { get; }
}

public sealed record PrivateOriginalMapEntity142RequestRejected :
    PrivateOriginalMapEntity142RequestResult
{
    internal PrivateOriginalMapEntity142RequestRejected(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapEntity142Diagnostic<PrivateOriginalMapEntity142RequestFailureCode>
            diagnostic)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Diagnostic = diagnostic ?? throw new ArgumentNullException(nameof(diagnostic));
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public PrivateOriginalMapEntity142Diagnostic<PrivateOriginalMapEntity142RequestFailureCode>
        Diagnostic
    { get; }
}

public abstract record PrivateOriginalMapEntity142AcknowledgementResult;

public sealed record PrivateOriginalMapEntity142AcknowledgementApplied :
    PrivateOriginalMapEntity142AcknowledgementResult
{
    internal PrivateOriginalMapEntity142AcknowledgementApplied(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapEntity142AcknowledgementReceipt receipt)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
        if (!ReferenceEquals(snapshot.LastEntity142Acknowledgement, receipt))
        {
            throw new ArgumentException(
                "An Entity 142 acknowledgement result must expose the snapshot's exact receipt.",
                nameof(receipt));
        }
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public PrivateOriginalMapEntity142AcknowledgementReceipt Receipt { get; }
}

public sealed record PrivateOriginalMapEntity142AcknowledgementRejected :
    PrivateOriginalMapEntity142AcknowledgementResult
{
    internal PrivateOriginalMapEntity142AcknowledgementRejected(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapEntity142Diagnostic<PrivateOriginalMapEntity142AcknowledgementFailureCode>
            diagnostic)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Diagnostic = diagnostic ?? throw new ArgumentNullException(nameof(diagnostic));
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public PrivateOriginalMapEntity142Diagnostic<PrivateOriginalMapEntity142AcknowledgementFailureCode>
        Diagnostic
    { get; }
}

public abstract record PrivateOriginalMapInteractionResult;

public sealed record PrivateOriginalMapInteractionUnavailable(PrivateOriginalMapSessionSnapshot Snapshot, string Reason) : PrivateOriginalMapInteractionResult;

public sealed partial class GameSession
{
    public PrivateOriginalMapInteractionResult RequestPrivateOriginalMapInteraction(long expectedSimulationStep)
    {
        return new PrivateOriginalMapInteractionUnavailable(PrivateOriginalMapSnapshot,
            "Opening, castle, palace, Astral and tower programs run in the common host; this host retains later controlled comparisons.");
    }
}

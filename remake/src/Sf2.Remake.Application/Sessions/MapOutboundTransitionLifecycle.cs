using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum MapOutboundTransitionStatus
{
    Pending,
    Acknowledged,
}

public sealed record RequestSelectedOutboundTransitionCommand : IGameSessionCommand;

public sealed record AcknowledgeMapOutboundTransitionCommand : IGameSessionCommand
{
    public AcknowledgeMapOutboundTransitionCommand(
        MapOutboundTransitionRequestId request,
        long cueSequence,
        MapOutboundTransitionId transition)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        CueSequence = cueSequence;
        Transition = transition ?? throw new ArgumentNullException(nameof(transition));
    }

    public MapOutboundTransitionRequestId Request { get; }

    public long CueSequence { get; }

    public MapOutboundTransitionId Transition { get; }
}

public sealed record MapOutboundTransitionSnapshot
{
    private MapOutboundTransitionSnapshot(
        MapOutboundTransitionDefinition definition,
        MapOutboundTransitionStatus status,
        long requestedAtStep,
        long cueSequence,
        long? acknowledgedAtStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(requestedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        if (status == MapOutboundTransitionStatus.Pending && acknowledgedAtStep is not null)
        {
            throw new ArgumentException(
                "A pending outbound transition cannot have an acknowledgement step.",
                nameof(acknowledgedAtStep));
        }

        if (status == MapOutboundTransitionStatus.Acknowledged &&
            (acknowledgedAtStep is null || acknowledgedAtStep <= requestedAtStep))
        {
            throw new ArgumentException(
                "An acknowledged outbound transition requires a later acknowledgement step.",
                nameof(acknowledgedAtStep));
        }

        Request = definition.Request;
        Transition = definition.Transition;
        Target = definition.ZoneTarget;
        SourceMap = definition.SourceMap;
        SourcePosition = definition.SourcePosition;
        SourceSetup = definition.SourceSetup;
        DestinationMap = definition.DestinationMap;
        DestinationPosition = definition.DestinationPosition;
        DestinationSetup = definition.DestinationSetup;
        DestinationFacing = definition.DestinationFacing;
        Cue = definition.Cue;
        Status = status;
        RequestedAtStep = requestedAtStep;
        CueSequence = cueSequence;
        AcknowledgedAtStep = acknowledgedAtStep;
    }

    public MapOutboundTransitionRequestId Request { get; }

    public MapOutboundTransitionId Transition { get; }

    public EventTargetId Target { get; }

    public MapId SourceMap { get; }

    public MapPosition SourcePosition { get; }

    public MapSetupId SourceSetup { get; }

    public MapId DestinationMap { get; }

    public MapPosition DestinationPosition { get; }

    public MapSetupId DestinationSetup { get; }

    public SemanticFacing DestinationFacing { get; }

    public PresentationCueId Cue { get; }

    public MapOutboundTransitionStatus Status { get; }

    public long RequestedAtStep { get; }

    public long CueSequence { get; }

    public long? AcknowledgedAtStep { get; }

    internal static MapOutboundTransitionSnapshot Pending(
        MapOutboundTransitionDefinition definition,
        long requestedAtStep,
        long cueSequence) =>
        new(
            definition,
            MapOutboundTransitionStatus.Pending,
            requestedAtStep,
            cueSequence,
            acknowledgedAtStep: null);

    internal MapOutboundTransitionSnapshot Acknowledge(
        MapOutboundTransitionDefinition definition,
        long acknowledgedAtStep) =>
        new(
            definition,
            MapOutboundTransitionStatus.Acknowledged,
            RequestedAtStep,
            CueSequence,
            acknowledgedAtStep);
}

public sealed record MapOutboundTransitionCue
{
    public MapOutboundTransitionCue(
        PresentationCueId cue,
        MapOutboundTransitionRequestId request,
        MapOutboundTransitionId transition,
        EventTargetId target,
        MapId sourceMap,
        MapPosition sourcePosition,
        MapId destinationMap,
        long sequence)
    {
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Transition = transition ?? throw new ArgumentNullException(nameof(transition));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        SourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));
        SourcePosition = sourcePosition ?? throw new ArgumentNullException(nameof(sourcePosition));
        DestinationMap = destinationMap ?? throw new ArgumentNullException(nameof(destinationMap));
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        Sequence = sequence;
    }

    public PresentationCueId Cue { get; }

    public MapOutboundTransitionRequestId Request { get; }

    public MapOutboundTransitionId Transition { get; }

    public EventTargetId Target { get; }

    public MapId SourceMap { get; }

    public MapPosition SourcePosition { get; }

    public MapId DestinationMap { get; }

    public long Sequence { get; }

    public bool RequiresAcknowledgement => true;
}

public sealed record GameSessionOutboundTransitionRequested(
    GameSessionSnapshot Snapshot,
    MapOutboundTransitionSnapshot Transition,
    MapOutboundTransitionCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapOutboundTransitionSnapshot Transition { get; } =
        Transition ?? throw new ArgumentNullException(nameof(Transition));

    public MapOutboundTransitionCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}

public sealed record GameSessionOutboundTransitionApplied(
    GameSessionSnapshot Snapshot,
    MapOutboundTransitionSnapshot Transition) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapOutboundTransitionSnapshot Transition { get; } =
        Transition ?? throw new ArgumentNullException(nameof(Transition));
}

using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum MapLocalTransitionStatus
{
    Pending,
    Acknowledged,
}

public sealed record RequestSelectedLocalTransitionCommand : IGameSessionCommand;

public sealed record AcknowledgeMapLocalTransitionCommand : IGameSessionCommand
{
    public AcknowledgeMapLocalTransitionCommand(
        MapLocalTransitionRequestId request,
        long cueSequence,
        MapLocalTransitionId transition)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        CueSequence = cueSequence;
        Transition = transition ?? throw new ArgumentNullException(nameof(transition));
    }

    public MapLocalTransitionRequestId Request { get; }

    public long CueSequence { get; }

    public MapLocalTransitionId Transition { get; }
}

public sealed record MapLocalTransitionSnapshot
{
    private MapLocalTransitionSnapshot(
        MapLocalTransitionRequestId request,
        MapLocalTransitionId transition,
        EventTargetId target,
        MapId sourceMap,
        MapPosition sourcePosition,
        MapSetupId sourceSetup,
        MapId destinationMap,
        MapPosition destinationPosition,
        OpaqueMapOrientationId destinationOrientation,
        PresentationCueId cue,
        MapLocalTransitionStatus status,
        long requestedAtStep,
        long cueSequence,
        long? acknowledgedAtStep)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Transition = transition ?? throw new ArgumentNullException(nameof(transition));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        SourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));
        SourcePosition = sourcePosition ?? throw new ArgumentNullException(nameof(sourcePosition));
        SourceSetup = sourceSetup ?? throw new ArgumentNullException(nameof(sourceSetup));
        DestinationMap = destinationMap ?? throw new ArgumentNullException(nameof(destinationMap));
        DestinationPosition = destinationPosition ??
            throw new ArgumentNullException(nameof(destinationPosition));
        DestinationOrientation = destinationOrientation ??
            throw new ArgumentNullException(nameof(destinationOrientation));
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(requestedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        if (status == MapLocalTransitionStatus.Pending && acknowledgedAtStep is not null)
        {
            throw new ArgumentException(
                "A pending local transition cannot have an acknowledgement step.",
                nameof(acknowledgedAtStep));
        }

        if (status == MapLocalTransitionStatus.Acknowledged &&
            (acknowledgedAtStep is null || acknowledgedAtStep <= requestedAtStep))
        {
            throw new ArgumentException(
                "An acknowledged local transition requires a later acknowledgement step.",
                nameof(acknowledgedAtStep));
        }

        Status = status;
        RequestedAtStep = requestedAtStep;
        CueSequence = cueSequence;
        AcknowledgedAtStep = acknowledgedAtStep;
    }

    public MapLocalTransitionRequestId Request { get; }

    public MapLocalTransitionId Transition { get; }

    public EventTargetId Target { get; }

    public MapId SourceMap { get; }

    public MapPosition SourcePosition { get; }

    public MapSetupId SourceSetup { get; }

    public MapId DestinationMap { get; }

    public MapPosition DestinationPosition { get; }

    public OpaqueMapOrientationId DestinationOrientation { get; }

    public PresentationCueId Cue { get; }

    public MapLocalTransitionStatus Status { get; }

    public long RequestedAtStep { get; }

    public long CueSequence { get; }

    public long? AcknowledgedAtStep { get; }

    internal static MapLocalTransitionSnapshot Pending(
        MapLocalTransitionDefinition definition,
        long requestedAtStep,
        long cueSequence) =>
        new(
            definition.Request,
            definition.Transition,
            definition.ZoneTarget,
            definition.SourceMap,
            definition.SourcePosition,
            definition.SourceSetup,
            definition.DestinationMap,
            definition.DestinationPosition,
            definition.DestinationOrientation,
            definition.Cue,
            MapLocalTransitionStatus.Pending,
            requestedAtStep,
            cueSequence,
            acknowledgedAtStep: null);

    internal MapLocalTransitionSnapshot Acknowledge(long acknowledgedAtStep) =>
        new(
            Request,
            Transition,
            Target,
            SourceMap,
            SourcePosition,
            SourceSetup,
            DestinationMap,
            DestinationPosition,
            DestinationOrientation,
            Cue,
            MapLocalTransitionStatus.Acknowledged,
            RequestedAtStep,
            CueSequence,
            acknowledgedAtStep);
}

public sealed record MapLocalTransitionCue
{
    public MapLocalTransitionCue(
        PresentationCueId cue,
        MapLocalTransitionRequestId request,
        MapLocalTransitionId transition,
        EventTargetId target,
        MapPosition sourcePosition,
        long sequence)
    {
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Transition = transition ?? throw new ArgumentNullException(nameof(transition));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        SourcePosition = sourcePosition ?? throw new ArgumentNullException(nameof(sourcePosition));
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        Sequence = sequence;
    }

    public PresentationCueId Cue { get; }

    public MapLocalTransitionRequestId Request { get; }

    public MapLocalTransitionId Transition { get; }

    public EventTargetId Target { get; }

    public MapPosition SourcePosition { get; }

    public long Sequence { get; }

    public bool RequiresAcknowledgement => true;
}

public sealed record GameSessionLocalTransitionRequested(
    GameSessionSnapshot Snapshot,
    MapLocalTransitionSnapshot Transition,
    MapLocalTransitionCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapLocalTransitionSnapshot Transition { get; } =
        Transition ?? throw new ArgumentNullException(nameof(Transition));

    public MapLocalTransitionCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}

public sealed record GameSessionLocalTransitionApplied(
    GameSessionSnapshot Snapshot,
    MapLocalTransitionSnapshot Transition) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapLocalTransitionSnapshot Transition { get; } =
        Transition ?? throw new ArgumentNullException(nameof(Transition));
}

using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum MapEventRequestStatus
{
    Pending,
    Acknowledged,
}

public sealed record RequestSelectedZoneEventCommand : IGameSessionCommand;

public sealed record AcknowledgeMapEventRequestCommand : IGameSessionCommand
{
    public AcknowledgeMapEventRequestCommand(
        MapEventRequestId request,
        long cueSequence,
        MapEventEffectId effect)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        CueSequence = cueSequence;
        Effect = effect ?? throw new ArgumentNullException(nameof(effect));
    }

    public MapEventRequestId Request { get; }

    public long CueSequence { get; }

    public MapEventEffectId Effect { get; }
}

public sealed record MapEventRequestSnapshot
{
    private MapEventRequestSnapshot(
        MapEventRequestId request,
        EventTargetId target,
        MapEventEffectId expectedEffect,
        MapPosition position,
        MapEventRequestStatus status,
        long requestedAtStep,
        long cueSequence,
        long? acknowledgedAtStep)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        ExpectedEffect = expectedEffect ??
            throw new ArgumentNullException(nameof(expectedEffect));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(requestedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        if (status == MapEventRequestStatus.Pending && acknowledgedAtStep is not null)
        {
            throw new ArgumentException(
                "A pending event request cannot have an acknowledgement step.",
                nameof(acknowledgedAtStep));
        }

        if (status == MapEventRequestStatus.Acknowledged &&
            (acknowledgedAtStep is null || acknowledgedAtStep <= requestedAtStep))
        {
            throw new ArgumentException(
                "An acknowledged event request requires a later acknowledgement step.",
                nameof(acknowledgedAtStep));
        }

        Status = status;
        RequestedAtStep = requestedAtStep;
        CueSequence = cueSequence;
        AcknowledgedAtStep = acknowledgedAtStep;
    }

    public MapEventRequestId Request { get; }

    public EventTargetId Target { get; }

    public MapEventEffectId ExpectedEffect { get; }

    public MapPosition Position { get; }

    public MapEventRequestStatus Status { get; }

    public long RequestedAtStep { get; }

    public long CueSequence { get; }

    public long? AcknowledgedAtStep { get; }

    internal static MapEventRequestSnapshot Pending(
        MapEventRequestDefinition definition,
        MapEventEffectDefinition effect,
        MapPosition position,
        long requestedAtStep,
        long cueSequence)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(effect);
        if (definition.Request != effect.Request)
        {
            throw new ArgumentException(
                "The pending effect must belong to the admitted event request.",
                nameof(effect));
        }

        return new MapEventRequestSnapshot(
            definition.Request,
            definition.ZoneTarget,
            effect.Effect,
            position,
            MapEventRequestStatus.Pending,
            requestedAtStep,
            cueSequence,
            acknowledgedAtStep: null);
    }

    internal MapEventRequestSnapshot Acknowledge(long acknowledgedAtStep) =>
        new(
            Request,
            Target,
            ExpectedEffect,
            Position,
            MapEventRequestStatus.Acknowledged,
            RequestedAtStep,
            CueSequence,
            acknowledgedAtStep);
}

public sealed record MapEventRequestCue
{
    public MapEventRequestCue(
        PresentationCueId cue,
        MapEventRequestId request,
        EventTargetId target,
        MapPosition position,
        long sequence)
    {
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        Sequence = sequence;
    }

    public PresentationCueId Cue { get; }

    public MapEventRequestId Request { get; }

    public EventTargetId Target { get; }

    public MapPosition Position { get; }

    public long Sequence { get; }

    public bool RequiresAcknowledgement => true;
}

public sealed record GameSessionEventRequested(
    GameSessionSnapshot Snapshot,
    MapEventRequestSnapshot Request,
    MapEventRequestCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapEventRequestSnapshot Request { get; } =
        Request ?? throw new ArgumentNullException(nameof(Request));

    public MapEventRequestCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}

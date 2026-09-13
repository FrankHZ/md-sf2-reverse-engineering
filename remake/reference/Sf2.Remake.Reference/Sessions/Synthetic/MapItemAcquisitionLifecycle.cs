using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Items;

namespace Sf2.Remake.Application.Sessions;

public enum MapItemAcquisitionStatus
{
    Pending,
    Acquired,
}

public enum MapItemAcquisitionCueKind
{
    AcquisitionPending,
    ItemAcquired,
}

public sealed record RequestMapItemAcquisitionCommand : IGameSessionCommand
{
    public RequestMapItemAcquisitionCommand(MapDiscoveryId discovery)
    {
        Discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
    }

    public MapDiscoveryId Discovery { get; }
}

public sealed record AcknowledgeMapItemAcquisitionCommand : IGameSessionCommand
{
    public AcknowledgeMapItemAcquisitionCommand(
        MapItemAcquisitionRequestId request,
        long cueSequence,
        MapItemAcquisitionResultId result,
        ItemId item)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        CueSequence = cueSequence;
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Item = item ?? throw new ArgumentNullException(nameof(item));
    }

    public MapItemAcquisitionRequestId Request { get; }

    public long CueSequence { get; }

    public MapItemAcquisitionResultId Result { get; }

    public ItemId Item { get; }
}

public sealed record MapItemAcquisitionSnapshot
{
    private MapItemAcquisitionSnapshot(
        MapDiscoveryId discovery,
        MapItemAcquisitionRequestId request,
        MapItemAcquisitionResultId result,
        ItemId item,
        MapItemAcquisitionStatus status,
        long requestedAtStep,
        long requestCueSequence,
        long? acquiredAtStep,
        long? acquiredCueSequence)
    {
        Discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Item = item ?? throw new ArgumentNullException(nameof(item));
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(requestedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(requestCueSequence, 1);
        if (status == MapItemAcquisitionStatus.Pending &&
            (acquiredAtStep is not null || acquiredCueSequence is not null))
        {
            throw new ArgumentException(
                "A pending item acquisition cannot have an acquired step or cue.",
                nameof(acquiredAtStep));
        }

        if (status == MapItemAcquisitionStatus.Acquired &&
            (acquiredAtStep is null ||
             acquiredCueSequence is null ||
             acquiredAtStep <= requestedAtStep ||
             acquiredCueSequence <= requestCueSequence))
        {
            throw new ArgumentException(
                "An acquired item requires strictly later acquisition step and cue identities.",
                nameof(acquiredAtStep));
        }

        Status = status;
        RequestedAtStep = requestedAtStep;
        RequestCueSequence = requestCueSequence;
        AcquiredAtStep = acquiredAtStep;
        AcquiredCueSequence = acquiredCueSequence;
    }

    public MapDiscoveryId Discovery { get; }

    public MapItemAcquisitionRequestId Request { get; }

    public MapItemAcquisitionResultId Result { get; }

    public ItemId Item { get; }

    public MapItemAcquisitionStatus Status { get; }

    public long RequestedAtStep { get; }

    public long RequestCueSequence { get; }

    public long? AcquiredAtStep { get; }

    public long? AcquiredCueSequence { get; }

    internal static MapItemAcquisitionSnapshot Pending(
        MapItemAcquisitionDefinition definition,
        long requestedAtStep,
        long requestCueSequence) =>
        new(
            definition.Discovery,
            definition.Request,
            definition.Result,
            definition.Item,
            MapItemAcquisitionStatus.Pending,
            requestedAtStep,
            requestCueSequence,
            acquiredAtStep: null,
            acquiredCueSequence: null);

    internal MapItemAcquisitionSnapshot Acquire(
        MapItemAcquisitionDefinition definition,
        long acquiredAtStep,
        long acquiredCueSequence)
    {
        if (Status != MapItemAcquisitionStatus.Pending ||
            definition.Discovery != Discovery ||
            definition.Request != Request ||
            definition.Result != Result ||
            definition.Item != Item)
        {
            throw new InvalidOperationException(
                "Item acquisition requires the exact admitted pending definition.");
        }

        return new MapItemAcquisitionSnapshot(
            Discovery,
            Request,
            Result,
            Item,
            MapItemAcquisitionStatus.Acquired,
            RequestedAtStep,
            RequestCueSequence,
            acquiredAtStep,
            acquiredCueSequence);
    }
}

public sealed record MapItemAcquisitionReceipt
{
    internal MapItemAcquisitionReceipt(MapItemAcquisitionSnapshot acquisition)
    {
        ArgumentNullException.ThrowIfNull(acquisition);
        if (acquisition.Status != MapItemAcquisitionStatus.Acquired ||
            acquisition.AcquiredAtStep is null ||
            acquisition.AcquiredCueSequence is null)
        {
            throw new ArgumentException(
                "An item-acquisition receipt requires one completed acquisition.",
                nameof(acquisition));
        }

        Discovery = acquisition.Discovery;
        Request = acquisition.Request;
        Result = acquisition.Result;
        Item = acquisition.Item;
        RequestedAtStep = acquisition.RequestedAtStep;
        RequestCueSequence = acquisition.RequestCueSequence;
        AcquiredAtStep = acquisition.AcquiredAtStep.Value;
        AcquiredCueSequence = acquisition.AcquiredCueSequence.Value;
    }

    public MapDiscoveryId Discovery { get; }

    public MapItemAcquisitionRequestId Request { get; }

    public MapItemAcquisitionResultId Result { get; }

    public ItemId Item { get; }

    public long RequestedAtStep { get; }

    public long RequestCueSequence { get; }

    public long AcquiredAtStep { get; }

    public long AcquiredCueSequence { get; }
}

public sealed record MapItemAcquisitionCue
{
    public MapItemAcquisitionCue(
        PresentationCueId cue,
        MapDiscoveryId discovery,
        MapItemAcquisitionRequestId request,
        MapItemAcquisitionResultId result,
        ItemId item,
        MapItemAcquisitionCueKind kind,
        long sequence)
    {
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        Discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Item = item ?? throw new ArgumentNullException(nameof(item));
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        Kind = kind;
        Sequence = sequence;
    }

    public PresentationCueId Cue { get; }

    public MapDiscoveryId Discovery { get; }

    public MapItemAcquisitionRequestId Request { get; }

    public MapItemAcquisitionResultId Result { get; }

    public ItemId Item { get; }

    public MapItemAcquisitionCueKind Kind { get; }

    public long Sequence { get; }

    public bool RequiresAcknowledgement =>
        Kind == MapItemAcquisitionCueKind.AcquisitionPending;
}

public sealed record GameSessionItemAcquisitionRequested(
    GameSessionSnapshot Snapshot,
    MapItemAcquisitionSnapshot Acquisition,
    MapItemAcquisitionCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapItemAcquisitionSnapshot Acquisition { get; } =
        Acquisition ?? throw new ArgumentNullException(nameof(Acquisition));

    public MapItemAcquisitionCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}

public sealed record GameSessionItemAcquired(
    GameSessionSnapshot Snapshot,
    MapItemAcquisitionSnapshot Acquisition,
    MapItemAcquisitionReceipt Receipt,
    MapItemAcquisitionCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapItemAcquisitionSnapshot Acquisition { get; } =
        Acquisition ?? throw new ArgumentNullException(nameof(Acquisition));

    public MapItemAcquisitionReceipt Receipt { get; } =
        Receipt ?? throw new ArgumentNullException(nameof(Receipt));

    public MapItemAcquisitionCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}

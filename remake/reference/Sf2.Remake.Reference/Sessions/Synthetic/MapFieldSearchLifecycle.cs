using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum MapFieldSearchStatus
{
    Pending,
    Discovered,
}

public enum MapFieldSearchCueKind
{
    SearchPending,
    DiscoveryPresented,
}

public sealed record RequestFieldSearchCommand : IGameSessionCommand;

public sealed record AcknowledgeFieldSearchCommand : IGameSessionCommand
{
    public AcknowledgeFieldSearchCommand(
        MapFieldSearchRequestId request,
        long cueSequence,
        MapFieldSearchResultId result)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        CueSequence = cueSequence;
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    public MapFieldSearchRequestId Request { get; }

    public long CueSequence { get; }

    public MapFieldSearchResultId Result { get; }
}

public sealed class PublicSyntheticDiscoveryStateSnapshot
{
    private readonly ReadOnlyCollection<MapDiscoveryId> _discoveries;
    private readonly HashSet<MapDiscoveryId> _lookup;

    public PublicSyntheticDiscoveryStateSnapshot(IEnumerable<MapDiscoveryId> discoveries)
    {
        ArgumentNullException.ThrowIfNull(discoveries);
        List<MapDiscoveryId> copiedDiscoveries = [];
        _lookup = [];
        foreach (MapDiscoveryId discovery in discoveries)
        {
            MapDiscoveryId admitted = discovery ?? throw new ArgumentException(
                "Discovery state cannot contain null values.",
                nameof(discoveries));
            if (!_lookup.Add(admitted))
            {
                throw new ArgumentException(
                    $"Duplicate discovery identity '{admitted}'.",
                    nameof(discoveries));
            }

            copiedDiscoveries.Add(admitted);
        }

        _discoveries = copiedDiscoveries.AsReadOnly();
    }

    public IReadOnlyList<MapDiscoveryId> Discoveries => _discoveries;

    public bool IsDiscovered(MapDiscoveryId discovery)
    {
        ArgumentNullException.ThrowIfNull(discovery);
        return _lookup.Contains(discovery);
    }

    internal PublicSyntheticDiscoveryStateSnapshot DiscoverOnce(MapDiscoveryId discovery)
    {
        ArgumentNullException.ThrowIfNull(discovery);
        if (_lookup.Contains(discovery))
        {
            throw new InvalidOperationException(
                $"Discovery '{discovery}' has already been admitted in this session.");
        }

        return new PublicSyntheticDiscoveryStateSnapshot(_discoveries.Append(discovery));
    }
}

public sealed record MapFieldSearchSnapshot
{
    private MapFieldSearchSnapshot(
        MapFieldSearchContextId context,
        MapFieldSearchRequestId request,
        MapFieldSearchResultId result,
        MapDiscoveryId discovery,
        MapId map,
        MapPosition position,
        MapSetupId setup,
        EventTargetId zoneTarget,
        MapFieldSearchStatus status,
        long requestedAtStep,
        long requestCueSequence,
        long? discoveredAtStep,
        long? discoveryCueSequence)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        Setup = setup ?? throw new ArgumentNullException(nameof(setup));
        ZoneTarget = zoneTarget ?? throw new ArgumentNullException(nameof(zoneTarget));
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(requestedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(requestCueSequence, 1);
        if (status == MapFieldSearchStatus.Pending &&
            (discoveredAtStep is not null || discoveryCueSequence is not null))
        {
            throw new ArgumentException(
                "A pending field search cannot have a discovery step or cue.",
                nameof(discoveredAtStep));
        }

        if (status == MapFieldSearchStatus.Discovered &&
            (discoveredAtStep is null ||
             discoveryCueSequence is null ||
             discoveredAtStep <= requestedAtStep ||
             discoveryCueSequence <= requestCueSequence))
        {
            throw new ArgumentException(
                "A discovered field search requires strictly later discovery step and cue identities.",
                nameof(discoveredAtStep));
        }

        Status = status;
        RequestedAtStep = requestedAtStep;
        RequestCueSequence = requestCueSequence;
        DiscoveredAtStep = discoveredAtStep;
        DiscoveryCueSequence = discoveryCueSequence;
    }

    public MapFieldSearchContextId Context { get; }

    public MapFieldSearchRequestId Request { get; }

    public MapFieldSearchResultId Result { get; }

    public MapDiscoveryId Discovery { get; }

    public MapId Map { get; }

    public MapPosition Position { get; }

    public MapSetupId Setup { get; }

    public EventTargetId ZoneTarget { get; }

    public MapFieldSearchStatus Status { get; }

    public long RequestedAtStep { get; }

    public long RequestCueSequence { get; }

    public long? DiscoveredAtStep { get; }

    public long? DiscoveryCueSequence { get; }

    internal static MapFieldSearchSnapshot Pending(
        MapFieldSearchDefinition definition,
        long requestedAtStep,
        long requestCueSequence) =>
        new(
            definition.Context,
            definition.Request,
            definition.Result,
            definition.Discovery,
            definition.Map,
            definition.Position,
            definition.Setup,
            definition.ZoneTarget,
            MapFieldSearchStatus.Pending,
            requestedAtStep,
            requestCueSequence,
            discoveredAtStep: null,
            discoveryCueSequence: null);

    internal MapFieldSearchSnapshot Discover(
        MapFieldSearchDefinition definition,
        long discoveredAtStep,
        long discoveryCueSequence)
    {
        if (Status != MapFieldSearchStatus.Pending ||
            definition.Context != Context ||
            definition.Request != Request ||
            definition.Result != Result ||
            definition.Discovery != Discovery ||
            definition.Map != Map ||
            definition.Position != Position ||
            definition.Setup != Setup ||
            definition.ZoneTarget != ZoneTarget)
        {
            throw new InvalidOperationException(
                "Field-search discovery requires the exact admitted pending definition.");
        }

        return new MapFieldSearchSnapshot(
            Context,
            Request,
            Result,
            Discovery,
            Map,
            Position,
            Setup,
            ZoneTarget,
            MapFieldSearchStatus.Discovered,
            RequestedAtStep,
            RequestCueSequence,
            discoveredAtStep,
            discoveryCueSequence);
    }
}

public sealed record MapFieldSearchReceipt
{
    internal MapFieldSearchReceipt(MapFieldSearchSnapshot search)
    {
        ArgumentNullException.ThrowIfNull(search);
        if (search.Status != MapFieldSearchStatus.Discovered ||
            search.DiscoveredAtStep is null ||
            search.DiscoveryCueSequence is null)
        {
            throw new ArgumentException(
                "A field-search receipt requires one completed discovery.",
                nameof(search));
        }

        Context = search.Context;
        Request = search.Request;
        Result = search.Result;
        Discovery = search.Discovery;
        Map = search.Map;
        Position = search.Position;
        Setup = search.Setup;
        ZoneTarget = search.ZoneTarget;
        RequestedAtStep = search.RequestedAtStep;
        RequestCueSequence = search.RequestCueSequence;
        DiscoveredAtStep = search.DiscoveredAtStep.Value;
        DiscoveryCueSequence = search.DiscoveryCueSequence.Value;
    }

    public MapFieldSearchContextId Context { get; }

    public MapFieldSearchRequestId Request { get; }

    public MapFieldSearchResultId Result { get; }

    public MapDiscoveryId Discovery { get; }

    public MapId Map { get; }

    public MapPosition Position { get; }

    public MapSetupId Setup { get; }

    public EventTargetId ZoneTarget { get; }

    public long RequestedAtStep { get; }

    public long RequestCueSequence { get; }

    public long DiscoveredAtStep { get; }

    public long DiscoveryCueSequence { get; }
}

public sealed record MapFieldSearchCue
{
    public MapFieldSearchCue(
        PresentationCueId cue,
        MapFieldSearchRequestId request,
        MapFieldSearchResultId result,
        MapDiscoveryId discovery,
        MapFieldSearchCueKind kind,
        long sequence)
    {
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        Kind = kind;
        Sequence = sequence;
    }

    public PresentationCueId Cue { get; }

    public MapFieldSearchRequestId Request { get; }

    public MapFieldSearchResultId Result { get; }

    public MapDiscoveryId Discovery { get; }

    public MapFieldSearchCueKind Kind { get; }

    public long Sequence { get; }

    public bool RequiresAcknowledgement => Kind == MapFieldSearchCueKind.SearchPending;
}

public sealed record GameSessionFieldSearchRequested(
    GameSessionSnapshot Snapshot,
    MapFieldSearchSnapshot Search,
    MapFieldSearchCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapFieldSearchSnapshot Search { get; } =
        Search ?? throw new ArgumentNullException(nameof(Search));

    public MapFieldSearchCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}

public sealed record GameSessionFieldSearchDiscovered(
    GameSessionSnapshot Snapshot,
    MapFieldSearchSnapshot Search,
    MapFieldSearchReceipt Receipt,
    MapFieldSearchCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapFieldSearchSnapshot Search { get; } =
        Search ?? throw new ArgumentNullException(nameof(Search));

    public MapFieldSearchReceipt Receipt { get; } =
        Receipt ?? throw new ArgumentNullException(nameof(Receipt));

    public MapFieldSearchCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}

using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapSarahEventIdentity
{
    public OriginalMapSarahEventIdentity(
        ContentProfile profile,
        MapId map,
        MapSetupId setup,
        string resourceId,
        int oneBasedRecordOrdinal,
        string targetIdentity,
        byte opaqueEventFacing)
    {
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        Profile = profile;
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Setup = setup ?? throw new ArgumentNullException(nameof(setup));
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentOutOfRangeException.ThrowIfLessThan(oneBasedRecordOrdinal, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetIdentity);
        if (opaqueEventFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(opaqueEventFacing));
        }

        ResourceId = resourceId;
        OneBasedRecordOrdinal = oneBasedRecordOrdinal;
        TargetIdentity = targetIdentity;
        OpaqueEventFacing = opaqueEventFacing;
    }

    public ContentProfile Profile { get; }

    public MapId Map { get; }

    public MapSetupId Setup { get; }

    public string ResourceId { get; }

    public int OneBasedRecordOrdinal { get; }

    public string TargetIdentity { get; }

    public byte OpaqueEventFacing { get; }
}

public enum OriginalMapSarahInteractionStage
{
    ReadFlag603Clear,
    ReadFlag602Clear,
    ReadTemporaryFlag256Clear,
    PresentText512,
    PresentText480,
    PresentText481,
    ReadTemporaryFlag256ClearAgain,
    MoveLeftOneAndWait,
    MoveUpOneAndWait,
    SetTemporaryFlag256,
    ReadTemporaryFlag256Set,
    ReadTemporaryFlag256SetAgain,
    RestoreFacingDown,
}

public sealed record OriginalMapSarahDefinition
{
    private readonly ReadOnlyCollection<int> _firstInteractionTextIds;
    private readonly ReadOnlyCollection<int> _repeatInteractionTextIds;
    private readonly ReadOnlyCollection<OriginalMapSarahInteractionStage>
        _firstInteractionStages;
    private readonly ReadOnlyCollection<OriginalMapSarahInteractionStage>
        _repeatInteractionStages;

    public OriginalMapSarahDefinition(
        OriginalMapSarahEventIdentity identity,
        OriginalMapEntityRecordIdentity actorSourceRecord,
        int logicalActorId,
        MapPosition actorInitialPosition,
        byte actorInitialOpaqueFacing,
        MapPosition playerInteractionPosition,
        byte playerInteractionOpaqueFacing,
        int laterBranchFlag603,
        int laterBranchFlag602,
        int temporaryRouteFlag256,
        string blockingSequenceIdentity,
        MapPosition firstInteractionWaypoint,
        byte restoredOpaqueFacing,
        IEnumerable<int> firstInteractionTextIds,
        IEnumerable<int> repeatInteractionTextIds,
        IEnumerable<OriginalMapSarahInteractionStage> firstInteractionStages,
        IEnumerable<OriginalMapSarahInteractionStage> repeatInteractionStages)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        ActorSourceRecord = actorSourceRecord ??
            throw new ArgumentNullException(nameof(actorSourceRecord));
        ArgumentOutOfRangeException.ThrowIfNegative(logicalActorId);
        ActorInitialPosition = actorInitialPosition ??
            throw new ArgumentNullException(nameof(actorInitialPosition));
        PlayerInteractionPosition = playerInteractionPosition ??
            throw new ArgumentNullException(nameof(playerInteractionPosition));
        FirstInteractionWaypoint = firstInteractionWaypoint ??
            throw new ArgumentNullException(nameof(firstInteractionWaypoint));
        if (actorInitialOpaqueFacing > 3 ||
            playerInteractionOpaqueFacing > 3 ||
            restoredOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(actorInitialOpaqueFacing));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(laterBranchFlag603);
        ArgumentOutOfRangeException.ThrowIfNegative(laterBranchFlag602);
        ArgumentOutOfRangeException.ThrowIfNegative(temporaryRouteFlag256);
        ArgumentException.ThrowIfNullOrWhiteSpace(blockingSequenceIdentity);

        int[] firstTexts = CopyIds(firstInteractionTextIds, nameof(firstInteractionTextIds));
        int[] repeatTexts = CopyIds(repeatInteractionTextIds, nameof(repeatInteractionTextIds));
        OriginalMapSarahInteractionStage[] firstStages =
            CopyStages(firstInteractionStages, nameof(firstInteractionStages));
        OriginalMapSarahInteractionStage[] repeatStages =
            CopyStages(repeatInteractionStages, nameof(repeatInteractionStages));

        LogicalActorId = logicalActorId;
        ActorInitialOpaqueFacing = actorInitialOpaqueFacing;
        PlayerInteractionOpaqueFacing = playerInteractionOpaqueFacing;
        LaterBranchFlag603 = laterBranchFlag603;
        LaterBranchFlag602 = laterBranchFlag602;
        TemporaryRouteFlag256 = temporaryRouteFlag256;
        BlockingSequenceIdentity = blockingSequenceIdentity;
        RestoredOpaqueFacing = restoredOpaqueFacing;
        _firstInteractionTextIds = Array.AsReadOnly(firstTexts);
        _repeatInteractionTextIds = Array.AsReadOnly(repeatTexts);
        _firstInteractionStages = Array.AsReadOnly(firstStages);
        _repeatInteractionStages = Array.AsReadOnly(repeatStages);
    }

    public OriginalMapSarahEventIdentity Identity { get; }

    public OriginalMapEntityRecordIdentity ActorSourceRecord { get; }

    public int LogicalActorId { get; }

    public MapPosition ActorInitialPosition { get; }

    public byte ActorInitialOpaqueFacing { get; }

    public MapPosition PlayerInteractionPosition { get; }

    public byte PlayerInteractionOpaqueFacing { get; }

    public int LaterBranchFlag603 { get; }

    public int LaterBranchFlag602 { get; }

    public int TemporaryRouteFlag256 { get; }

    public string BlockingSequenceIdentity { get; }

    public MapPosition FirstInteractionWaypoint { get; }

    public byte RestoredOpaqueFacing { get; }

    public IReadOnlyList<int> FirstInteractionTextIds => _firstInteractionTextIds;

    public IReadOnlyList<int> RepeatInteractionTextIds => _repeatInteractionTextIds;

    public IReadOnlyList<OriginalMapSarahInteractionStage> FirstInteractionStages =>
        _firstInteractionStages;

    public IReadOnlyList<OriginalMapSarahInteractionStage> RepeatInteractionStages =>
        _repeatInteractionStages;

    private static int[] CopyIds(IEnumerable<int> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        int[] copied = [.. values];
        if (copied.Length == 0 || copied.Any(value => value < 0))
        {
            throw new ArgumentException(
                "Sarah interaction text identities must be non-empty and non-negative.",
                parameterName);
        }

        return copied;
    }

    private static OriginalMapSarahInteractionStage[] CopyStages(
        IEnumerable<OriginalMapSarahInteractionStage> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        OriginalMapSarahInteractionStage[] copied = [.. values];
        if (copied.Length == 0 || copied.Any(stage => !Enum.IsDefined(stage)))
        {
            throw new ArgumentException(
                "Sarah interaction stages must be a non-empty typed sequence.",
                parameterName);
        }

        return copied;
    }
}

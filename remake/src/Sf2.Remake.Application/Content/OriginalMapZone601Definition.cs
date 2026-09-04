using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapZoneEventIdentity
{
    public OriginalMapZoneEventIdentity(
        ContentProfile profile,
        MapId map,
        MapSetupId setup,
        string resourceId,
        int oneBasedRecordOrdinal,
        string targetIdentity)
    {
        if (profile != ContentProfile.PrivateLocal)
        {
            throw new ArgumentException(
                "An original zone-event identity must remain PrivateLocal.",
                nameof(profile));
        }

        Map = map ?? throw new ArgumentNullException(nameof(map));
        Setup = setup ?? throw new ArgumentNullException(nameof(setup));
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentOutOfRangeException.ThrowIfLessThan(oneBasedRecordOrdinal, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetIdentity);
        Profile = profile;
        ResourceId = resourceId;
        OneBasedRecordOrdinal = oneBasedRecordOrdinal;
        TargetIdentity = targetIdentity;
    }

    public ContentProfile Profile { get; }

    public MapId Map { get; }

    public MapSetupId Setup { get; }

    public string ResourceId { get; }

    public int OneBasedRecordOrdinal { get; }

    public string TargetIdentity { get; }
}

public enum OriginalMapZone601BlockingStage
{
    ActorInitAndWait,
    ActorMoveUpTwoAndWait,
    ActorFaceLeftAndWait,
    PresentText510,
    PresentText511,
    PresentText483,
    ActorReinitAndWait,
    AmbientWalkingHandoff,
    SetFlag601,
}

public sealed record OriginalMapZone601Definition
{
    private readonly ReadOnlyCollection<int> _textIds;
    private readonly ReadOnlyCollection<OriginalMapZone601BlockingStage> _blockingStages;

    public OriginalMapZone601Definition(
        OriginalMapZoneEventIdentity identity,
        MapPosition trigger,
        int gateFlag,
        string blockingSequenceIdentity,
        OriginalMapEntityRecordIdentity actorSourceRecord,
        int logicalActorId,
        MapPosition actorInitialPosition,
        byte actorInitialOpaqueFacing,
        string actorInitialBehaviorIdentity,
        MapPosition actorBlockingEndPosition,
        byte actorBlockingEndOpaqueFacing,
        int opaqueFaceWaitOperand,
        IEnumerable<int> textIds,
        string ambientBehaviorIdentity,
        MapPosition ambientCenter,
        int ambientRange,
        IEnumerable<OriginalMapZone601BlockingStage> blockingStages)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        ArgumentOutOfRangeException.ThrowIfNegative(gateFlag);
        ArgumentException.ThrowIfNullOrWhiteSpace(blockingSequenceIdentity);
        ActorSourceRecord = actorSourceRecord ??
            throw new ArgumentNullException(nameof(actorSourceRecord));
        ArgumentOutOfRangeException.ThrowIfNegative(logicalActorId);
        ActorInitialPosition = actorInitialPosition ??
            throw new ArgumentNullException(nameof(actorInitialPosition));
        ActorBlockingEndPosition = actorBlockingEndPosition ??
            throw new ArgumentNullException(nameof(actorBlockingEndPosition));
        if (actorInitialOpaqueFacing > 3 || actorBlockingEndOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(actorInitialOpaqueFacing));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(actorInitialBehaviorIdentity);
        ArgumentOutOfRangeException.ThrowIfNegative(opaqueFaceWaitOperand);
        ArgumentNullException.ThrowIfNull(textIds);
        int[] copiedTextIds = [.. textIds];
        if (copiedTextIds.Length == 0 || copiedTextIds.Any(id => id < 0))
        {
            throw new ArgumentException(
                "The bounded Zone 601 definition requires non-negative text identities.",
                nameof(textIds));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(ambientBehaviorIdentity);
        AmbientCenter = ambientCenter ?? throw new ArgumentNullException(nameof(ambientCenter));
        ArgumentOutOfRangeException.ThrowIfNegative(ambientRange);
        ArgumentNullException.ThrowIfNull(blockingStages);
        OriginalMapZone601BlockingStage[] copiedStages = [.. blockingStages];
        if (copiedStages.Length == 0 || copiedStages.Any(stage => !Enum.IsDefined(stage)))
        {
            throw new ArgumentException(
                "The bounded Zone 601 definition requires a valid blocking chronology.",
                nameof(blockingStages));
        }

        GateFlag = gateFlag;
        BlockingSequenceIdentity = blockingSequenceIdentity;
        LogicalActorId = logicalActorId;
        ActorInitialOpaqueFacing = actorInitialOpaqueFacing;
        ActorInitialBehaviorIdentity = actorInitialBehaviorIdentity;
        ActorBlockingEndOpaqueFacing = actorBlockingEndOpaqueFacing;
        OpaqueFaceWaitOperand = opaqueFaceWaitOperand;
        _textIds = Array.AsReadOnly(copiedTextIds);
        AmbientBehaviorIdentity = ambientBehaviorIdentity;
        AmbientRange = ambientRange;
        _blockingStages = Array.AsReadOnly(copiedStages);
    }

    public OriginalMapZoneEventIdentity Identity { get; }

    public MapPosition Trigger { get; }

    public int GateFlag { get; }

    public string BlockingSequenceIdentity { get; }

    public OriginalMapEntityRecordIdentity ActorSourceRecord { get; }

    public int LogicalActorId { get; }

    public MapPosition ActorInitialPosition { get; }

    public byte ActorInitialOpaqueFacing { get; }

    public string ActorInitialBehaviorIdentity { get; }

    public MapPosition ActorBlockingEndPosition { get; }

    public byte ActorBlockingEndOpaqueFacing { get; }

    public int OpaqueFaceWaitOperand { get; }

    public IReadOnlyList<int> TextIds => _textIds;

    public string AmbientBehaviorIdentity { get; }

    public MapPosition AmbientCenter { get; }

    public int AmbientRange { get; }

    public IReadOnlyList<OriginalMapZone601BlockingStage> BlockingStages => _blockingStages;
}

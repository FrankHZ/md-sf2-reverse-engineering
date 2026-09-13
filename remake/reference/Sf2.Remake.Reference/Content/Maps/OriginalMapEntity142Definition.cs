using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapEntity142EventIdentity
{
    public OriginalMapEntity142EventIdentity(
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

public enum OriginalMapEntity142InteractionStage
{
    ReadFlag261Clear,
    PresentText500,
    SetFlag261,
    PresentText501,
    SetFlag602,
    ReadFlag261Set,
}

public sealed record OriginalMapEntity142Definition
{
    private readonly ReadOnlyCollection<int> _firstInteractionTextIds;
    private readonly ReadOnlyCollection<int> _repeatInteractionTextIds;
    private readonly ReadOnlyCollection<OriginalMapEntity142InteractionStage>
        _firstInteractionStages;
    private readonly ReadOnlyCollection<OriginalMapEntity142InteractionStage>
        _repeatInteractionStages;

    public OriginalMapEntity142Definition(
        OriginalMapEntity142EventIdentity identity,
        OriginalMapEntityRecordIdentity actorSourceRecord,
        int logicalActorId,
        int physicalActorSlot,
        MapPosition actorPosition,
        byte actorOpaqueFacing,
        MapPosition playerInteractionPosition,
        byte playerInteractionOpaqueFacing,
        int firstInteractionFlag261,
        int completionFlag602,
        IEnumerable<int> firstInteractionTextIds,
        IEnumerable<int> repeatInteractionTextIds,
        IEnumerable<OriginalMapEntity142InteractionStage> firstInteractionStages,
        IEnumerable<OriginalMapEntity142InteractionStage> repeatInteractionStages)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        ActorSourceRecord = actorSourceRecord ??
            throw new ArgumentNullException(nameof(actorSourceRecord));
        ArgumentOutOfRangeException.ThrowIfNegative(logicalActorId);
        ArgumentOutOfRangeException.ThrowIfLessThan(physicalActorSlot, 1);
        if (physicalActorSlot != actorSourceRecord.OneBasedRecordOrdinal)
        {
            throw new ArgumentException(
                "Entity 142's physical slot must equal its admitted source-record ordinal.",
                nameof(physicalActorSlot));
        }

        ActorPosition = actorPosition ?? throw new ArgumentNullException(nameof(actorPosition));
        PlayerInteractionPosition = playerInteractionPosition ??
            throw new ArgumentNullException(nameof(playerInteractionPosition));
        if (actorOpaqueFacing > 3 || playerInteractionOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(actorOpaqueFacing));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(firstInteractionFlag261);
        ArgumentOutOfRangeException.ThrowIfNegative(completionFlag602);
        if (firstInteractionFlag261 == completionFlag602)
        {
            throw new ArgumentException("Entity 142's admitted flags must remain distinct.");
        }

        LogicalActorId = logicalActorId;
        PhysicalActorSlot = physicalActorSlot;
        ActorOpaqueFacing = actorOpaqueFacing;
        PlayerInteractionOpaqueFacing = playerInteractionOpaqueFacing;
        FirstInteractionFlag261 = firstInteractionFlag261;
        CompletionFlag602 = completionFlag602;
        _firstInteractionTextIds = Array.AsReadOnly(
            CopyIds(firstInteractionTextIds, nameof(firstInteractionTextIds)));
        _repeatInteractionTextIds = Array.AsReadOnly(
            CopyIds(repeatInteractionTextIds, nameof(repeatInteractionTextIds)));
        _firstInteractionStages = Array.AsReadOnly(
            CopyStages(firstInteractionStages, nameof(firstInteractionStages)));
        _repeatInteractionStages = Array.AsReadOnly(
            CopyStages(repeatInteractionStages, nameof(repeatInteractionStages)));
    }

    public OriginalMapEntity142EventIdentity Identity { get; }

    public OriginalMapEntityRecordIdentity ActorSourceRecord { get; }

    public int LogicalActorId { get; }

    public int PhysicalActorSlot { get; }

    public MapPosition ActorPosition { get; }

    public byte ActorOpaqueFacing { get; }

    public MapPosition PlayerInteractionPosition { get; }

    public byte PlayerInteractionOpaqueFacing { get; }

    public int FirstInteractionFlag261 { get; }

    public int CompletionFlag602 { get; }

    public IReadOnlyList<int> FirstInteractionTextIds => _firstInteractionTextIds;

    public IReadOnlyList<int> RepeatInteractionTextIds => _repeatInteractionTextIds;

    public IReadOnlyList<OriginalMapEntity142InteractionStage> FirstInteractionStages =>
        _firstInteractionStages;

    public IReadOnlyList<OriginalMapEntity142InteractionStage> RepeatInteractionStages =>
        _repeatInteractionStages;

    private static int[] CopyIds(IEnumerable<int> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        int[] copied = [.. values];
        if (copied.Length == 0 || copied.Any(value => value < 0))
        {
            throw new ArgumentException(
                "Entity 142 text identities must be a non-empty non-negative sequence.",
                parameterName);
        }

        return copied;
    }

    private static OriginalMapEntity142InteractionStage[] CopyStages(
        IEnumerable<OriginalMapEntity142InteractionStage> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        OriginalMapEntity142InteractionStage[] copied = [.. values];
        if (copied.Length == 0 || copied.Any(stage => !Enum.IsDefined(stage)))
        {
            throw new ArgumentException(
                "Entity 142 stages must be a non-empty typed sequence.",
                parameterName);
        }

        return copied;
    }
}

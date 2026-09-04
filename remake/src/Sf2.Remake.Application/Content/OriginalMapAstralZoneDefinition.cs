using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapAstralZoneEventIdentity
{
    public OriginalMapAstralZoneEventIdentity(
        ContentProfile profile,
        MapId map,
        MapSetupId setup,
        string resourceId,
        int oneBasedRecordOrdinal,
        string targetIdentity)
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

public enum OriginalMapAstralZoneStage
{
    ReadMessengerFlag603Clear,
    ReadEntity142Flag602Set,
    ReadCompletionFlag260Clear,
    PresentText514,
    PresentText515,
    PresentText516,
    RunPositionProgram,
    SetCompletionFlag260,
}

public sealed record OriginalMapAstralZoneDefinition
{
    private readonly ReadOnlyCollection<int> _textIds;
    private readonly ReadOnlyCollection<OriginalMapAstralZoneStage> _stages;

    public OriginalMapAstralZoneDefinition(
        OriginalMapAstralZoneEventIdentity identity,
        MapPosition trigger,
        string positionProgramIdentity,
        int messengerCompletionFlag603,
        int requiredEntity142Flag602,
        int completionFlag260,
        OriginalMapEntityRecordIdentity sarahSourceRecord,
        int sarahLogicalActorId,
        MapPosition sarahDestination,
        byte sarahOpaqueFacing,
        OriginalMapEntityRecordIdentity zone601ActorSourceRecord,
        int zone601LogicalActorId,
        MapPosition zone601ActorDestination,
        byte zone601ActorOpaqueFacing,
        IEnumerable<int> textIds,
        IEnumerable<OriginalMapAstralZoneStage> stages)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        ArgumentException.ThrowIfNullOrWhiteSpace(positionProgramIdentity);
        ArgumentOutOfRangeException.ThrowIfNegative(messengerCompletionFlag603);
        ArgumentOutOfRangeException.ThrowIfNegative(requiredEntity142Flag602);
        ArgumentOutOfRangeException.ThrowIfNegative(completionFlag260);
        if (messengerCompletionFlag603 == requiredEntity142Flag602 ||
            messengerCompletionFlag603 == completionFlag260 ||
            requiredEntity142Flag602 == completionFlag260)
        {
            throw new ArgumentException("Astral-zone flags must remain distinct.");
        }

        SarahSourceRecord = sarahSourceRecord ??
            throw new ArgumentNullException(nameof(sarahSourceRecord));
        Zone601ActorSourceRecord = zone601ActorSourceRecord ??
            throw new ArgumentNullException(nameof(zone601ActorSourceRecord));
        ArgumentOutOfRangeException.ThrowIfNegative(sarahLogicalActorId);
        ArgumentOutOfRangeException.ThrowIfNegative(zone601LogicalActorId);
        if (sarahLogicalActorId == zone601LogicalActorId ||
            sarahSourceRecord == zone601ActorSourceRecord)
        {
            throw new ArgumentException("Astral-zone actor bindings must remain distinct.");
        }

        SarahDestination = sarahDestination ??
            throw new ArgumentNullException(nameof(sarahDestination));
        Zone601ActorDestination = zone601ActorDestination ??
            throw new ArgumentNullException(nameof(zone601ActorDestination));
        if (sarahOpaqueFacing > 3 || zone601ActorOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(sarahOpaqueFacing));
        }

        ArgumentNullException.ThrowIfNull(textIds);
        int[] copiedTextIds = [.. textIds];
        if (copiedTextIds.Length == 0 ||
            copiedTextIds.Any(value => value < 0) ||
            copiedTextIds.Distinct().Count() != copiedTextIds.Length)
        {
            throw new ArgumentException(
                "Astral-zone text identities must be non-empty, non-negative, and distinct.",
                nameof(textIds));
        }

        ArgumentNullException.ThrowIfNull(stages);
        OriginalMapAstralZoneStage[] copiedStages = [.. stages];
        if (copiedStages.Length == 0 ||
            copiedStages.Any(stage => !Enum.IsDefined(stage)) ||
            copiedStages.Distinct().Count() != copiedStages.Length)
        {
            throw new ArgumentException(
                "Astral-zone stages must be a non-empty distinct typed sequence.",
                nameof(stages));
        }

        PositionProgramIdentity = positionProgramIdentity;
        MessengerCompletionFlag603 = messengerCompletionFlag603;
        RequiredEntity142Flag602 = requiredEntity142Flag602;
        CompletionFlag260 = completionFlag260;
        SarahLogicalActorId = sarahLogicalActorId;
        SarahOpaqueFacing = sarahOpaqueFacing;
        Zone601LogicalActorId = zone601LogicalActorId;
        Zone601ActorOpaqueFacing = zone601ActorOpaqueFacing;
        _textIds = Array.AsReadOnly(copiedTextIds);
        _stages = Array.AsReadOnly(copiedStages);
    }

    public OriginalMapAstralZoneEventIdentity Identity { get; }

    public MapPosition Trigger { get; }

    public string PositionProgramIdentity { get; }

    public int MessengerCompletionFlag603 { get; }

    public int RequiredEntity142Flag602 { get; }

    public int CompletionFlag260 { get; }

    public OriginalMapEntityRecordIdentity SarahSourceRecord { get; }

    public int SarahLogicalActorId { get; }

    public MapPosition SarahDestination { get; }

    public byte SarahOpaqueFacing { get; }

    public OriginalMapEntityRecordIdentity Zone601ActorSourceRecord { get; }

    public int Zone601LogicalActorId { get; }

    public MapPosition Zone601ActorDestination { get; }

    public byte Zone601ActorOpaqueFacing { get; }

    public IReadOnlyList<int> TextIds => _textIds;

    public IReadOnlyList<OriginalMapAstralZoneStage> Stages => _stages;
}

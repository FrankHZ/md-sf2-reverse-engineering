using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapMessengerZoneEventIdentity
{
    public OriginalMapMessengerZoneEventIdentity(
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

public sealed record OriginalMapMessengerFollowerLink
{
    public OriginalMapMessengerFollowerLink(int followerId, int leaderId, int distance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(followerId);
        ArgumentOutOfRangeException.ThrowIfNegative(leaderId);
        ArgumentOutOfRangeException.ThrowIfLessThan(distance, 1);
        if (followerId == leaderId)
        {
            throw new ArgumentException("A messenger follower cannot follow itself.");
        }

        FollowerId = followerId;
        LeaderId = leaderId;
        Distance = distance;
    }

    public int FollowerId { get; }

    public int LeaderId { get; }

    public int Distance { get; }
}

public sealed record OriginalMapMessengerGuardState
{
    public OriginalMapMessengerGuardState(
        int logicalActorId,
        OriginalMapEntityRecordIdentity sourceRecord,
        MapPosition position,
        byte opaqueFacing)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(logicalActorId);
        SourceRecord = sourceRecord ?? throw new ArgumentNullException(nameof(sourceRecord));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        if (opaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(opaqueFacing));
        }

        LogicalActorId = logicalActorId;
        OpaqueFacing = opaqueFacing;
    }

    public int LogicalActorId { get; }

    public OriginalMapEntityRecordIdentity SourceRecord { get; }

    public MapPosition Position { get; }

    public byte OpaqueFacing { get; }
}

public enum OriginalMapMessengerAcceptanceStage
{
    EnterMessengerProgram,
    PresentPrePromptTextSequence,
    AcceptDefaultPrompt,
    ObservePromptFlag89Set,
    PresentAcceptedBranchTextSequence,
    SetFlag600,
    SetFlag66,
    JoinSarah,
    JoinChester,
    LinkSarahToBowie,
    LinkChesterToSarah,
    PositionGuard138,
    PositionGuard139,
    ReturnMessengerProgram,
    SetCompletionFlag603,
    ReachStableWaitForEvent,
}

public sealed record OriginalMapMessengerAcceptanceDefinition
{
    private readonly ReadOnlyCollection<int> _textIds;
    private readonly ReadOnlyCollection<int?> _speakerOperands;
    private readonly ReadOnlyCollection<int> _joinedCharacterIds;
    private readonly ReadOnlyCollection<OriginalMapMessengerFollowerLink> _followers;
    private readonly ReadOnlyCollection<OriginalMapMessengerGuardState> _guards;
    private readonly ReadOnlyCollection<OriginalMapMessengerAcceptanceStage> _stages;

    public OriginalMapMessengerAcceptanceDefinition(
        OriginalMapMessengerZoneEventIdentity identity,
        MapPosition approach,
        ExplorationDirection entryDirection,
        MapPosition trigger,
        string messengerProgramIdentity,
        string acceptedBranchProgramIdentity,
        string controlShapeSha256,
        int promptReturn,
        int promptFlag89,
        int joinSelector,
        int flag600,
        int flag66,
        int completionFlag603,
        OriginalMapEntityRecordIdentity sarahSourceRecord,
        int sarahCharacterId,
        OriginalMapEntityRecordIdentity entity142SourceRecord,
        int entity142LogicalActorId,
        OriginalMapEntityRecordIdentity messengerActorSourceRecord,
        int messengerLogicalActorId,
        MapPosition messengerActorInitialPosition,
        byte messengerActorInitialOpaqueFacing,
        IEnumerable<int> textIds,
        IEnumerable<int?> speakerOperands,
        IEnumerable<int> joinedCharacterIds,
        IEnumerable<OriginalMapMessengerFollowerLink> followers,
        IEnumerable<OriginalMapMessengerGuardState> guards,
        MapPosition endpoint,
        byte endpointOpaqueFacing,
        string terminalIdentity,
        IEnumerable<OriginalMapMessengerAcceptanceStage> stages)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Approach = approach ?? throw new ArgumentNullException(nameof(approach));
        if (!Enum.IsDefined(entryDirection))
        {
            throw new ArgumentOutOfRangeException(nameof(entryDirection));
        }

        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        ArgumentException.ThrowIfNullOrWhiteSpace(messengerProgramIdentity);
        ArgumentException.ThrowIfNullOrWhiteSpace(acceptedBranchProgramIdentity);
        OriginalMapImportRequest.ValidateSha256(controlShapeSha256, nameof(controlShapeSha256));
        ArgumentOutOfRangeException.ThrowIfNegative(promptReturn);
        ArgumentOutOfRangeException.ThrowIfNegative(promptFlag89);
        ArgumentOutOfRangeException.ThrowIfNegative(joinSelector);
        ArgumentOutOfRangeException.ThrowIfNegative(flag600);
        ArgumentOutOfRangeException.ThrowIfNegative(flag66);
        ArgumentOutOfRangeException.ThrowIfNegative(completionFlag603);
        if (promptFlag89 == flag600 || promptFlag89 == flag66 ||
            promptFlag89 == completionFlag603 || flag600 == flag66 ||
            flag600 == completionFlag603 || flag66 == completionFlag603)
        {
            throw new ArgumentException("Messenger acceptance flags must remain distinct.");
        }

        SarahSourceRecord = sarahSourceRecord ??
            throw new ArgumentNullException(nameof(sarahSourceRecord));
        Entity142SourceRecord = entity142SourceRecord ??
            throw new ArgumentNullException(nameof(entity142SourceRecord));
        MessengerActorSourceRecord = messengerActorSourceRecord ??
            throw new ArgumentNullException(nameof(messengerActorSourceRecord));
        ArgumentOutOfRangeException.ThrowIfNegative(sarahCharacterId);
        ArgumentOutOfRangeException.ThrowIfNegative(entity142LogicalActorId);
        ArgumentOutOfRangeException.ThrowIfNegative(messengerLogicalActorId);
        MessengerActorInitialPosition = messengerActorInitialPosition ??
            throw new ArgumentNullException(nameof(messengerActorInitialPosition));
        if (messengerActorInitialOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(messengerActorInitialOpaqueFacing));
        }
        if (SarahSourceRecord == Entity142SourceRecord ||
            SarahSourceRecord == MessengerActorSourceRecord ||
            Entity142SourceRecord == MessengerActorSourceRecord ||
            sarahCharacterId == entity142LogicalActorId ||
            sarahCharacterId == messengerLogicalActorId ||
            entity142LogicalActorId == messengerLogicalActorId)
        {
            throw new ArgumentException("Messenger route actor bindings must remain distinct.");
        }

        ArgumentNullException.ThrowIfNull(textIds);
        int[] copiedTextIds = [.. textIds];
        ArgumentNullException.ThrowIfNull(speakerOperands);
        int?[] copiedSpeakers = [.. speakerOperands];
        if (copiedTextIds.Length == 0 || copiedTextIds.Length != copiedSpeakers.Length ||
            copiedTextIds.Any(value => value < 0) ||
            copiedTextIds.Distinct().Count() != copiedTextIds.Length ||
            copiedSpeakers.Any(value => value < 0))
        {
            throw new ArgumentException(
                "Messenger text and speaker identities must form one non-negative aligned sequence.",
                nameof(textIds));
        }

        ArgumentNullException.ThrowIfNull(joinedCharacterIds);
        int[] copiedJoined = [.. joinedCharacterIds];
        if (copiedJoined.Length == 0 || copiedJoined.Any(value => value < 0) ||
            copiedJoined.Distinct().Count() != copiedJoined.Length)
        {
            throw new ArgumentException(
                "Messenger joined-character identities must be non-empty, non-negative, and distinct.",
                nameof(joinedCharacterIds));
        }

        ArgumentNullException.ThrowIfNull(followers);
        OriginalMapMessengerFollowerLink[] copiedFollowers = [.. followers];
        if (copiedFollowers.Length == 0 ||
            copiedFollowers.Select(link => link.FollowerId).Distinct().Count() !=
                copiedFollowers.Length)
        {
            throw new ArgumentException(
                "Messenger follower links must be non-empty and have unique followers.",
                nameof(followers));
        }

        ArgumentNullException.ThrowIfNull(guards);
        OriginalMapMessengerGuardState[] copiedGuards = [.. guards];
        if (copiedGuards.Length == 0 ||
            copiedGuards.Select(guard => guard.LogicalActorId).Distinct().Count() !=
                copiedGuards.Length ||
            copiedGuards.Select(guard => guard.SourceRecord).Distinct().Count() !=
                copiedGuards.Length)
        {
            throw new ArgumentException(
                "Messenger guard states must be non-empty and distinctly bound.",
                nameof(guards));
        }

        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        if (endpointOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(endpointOpaqueFacing));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(terminalIdentity);
        ArgumentNullException.ThrowIfNull(stages);
        OriginalMapMessengerAcceptanceStage[] copiedStages = [.. stages];
        if (copiedStages.Length == 0 || copiedStages.Any(stage => !Enum.IsDefined(stage)) ||
            copiedStages.Distinct().Count() != copiedStages.Length)
        {
            throw new ArgumentException(
                "Messenger stages must be a non-empty distinct typed sequence.",
                nameof(stages));
        }

        MessengerProgramIdentity = messengerProgramIdentity;
        AcceptedBranchProgramIdentity = acceptedBranchProgramIdentity;
        ControlShapeSha256 = controlShapeSha256.ToUpperInvariant();
        PromptReturn = promptReturn;
        PromptFlag89 = promptFlag89;
        JoinSelector = joinSelector;
        Flag600 = flag600;
        Flag66 = flag66;
        CompletionFlag603 = completionFlag603;
        SarahCharacterId = sarahCharacterId;
        Entity142LogicalActorId = entity142LogicalActorId;
        MessengerLogicalActorId = messengerLogicalActorId;
        MessengerActorInitialOpaqueFacing = messengerActorInitialOpaqueFacing;
        EntryDirection = entryDirection;
        EndpointOpaqueFacing = endpointOpaqueFacing;
        TerminalIdentity = terminalIdentity;
        _textIds = Array.AsReadOnly(copiedTextIds);
        _speakerOperands = Array.AsReadOnly(copiedSpeakers);
        _joinedCharacterIds = Array.AsReadOnly(copiedJoined);
        _followers = Array.AsReadOnly(copiedFollowers);
        _guards = Array.AsReadOnly(copiedGuards);
        _stages = Array.AsReadOnly(copiedStages);
    }

    public OriginalMapMessengerZoneEventIdentity Identity { get; }
    public MapPosition Approach { get; }
    public ExplorationDirection EntryDirection { get; }
    public MapPosition Trigger { get; }
    public string MessengerProgramIdentity { get; }
    public string AcceptedBranchProgramIdentity { get; }
    public string ControlShapeSha256 { get; }
    public int PromptReturn { get; }
    public int PromptFlag89 { get; }
    public int JoinSelector { get; }
    public int Flag600 { get; }
    public int Flag66 { get; }
    public int CompletionFlag603 { get; }
    public OriginalMapEntityRecordIdentity SarahSourceRecord { get; }
    public int SarahCharacterId { get; }
    public OriginalMapEntityRecordIdentity Entity142SourceRecord { get; }
    public int Entity142LogicalActorId { get; }
    public OriginalMapEntityRecordIdentity MessengerActorSourceRecord { get; }
    public int MessengerLogicalActorId { get; }
    public MapPosition MessengerActorInitialPosition { get; }
    public byte MessengerActorInitialOpaqueFacing { get; }
    public IReadOnlyList<int> TextIds => _textIds;
    public IReadOnlyList<int?> SpeakerOperands => _speakerOperands;
    public IReadOnlyList<int> JoinedCharacterIds => _joinedCharacterIds;
    public IReadOnlyList<OriginalMapMessengerFollowerLink> Followers => _followers;
    public IReadOnlyList<OriginalMapMessengerGuardState> Guards => _guards;
    public MapPosition Endpoint { get; }
    public byte EndpointOpaqueFacing { get; }
    public string TerminalIdentity { get; }
    public IReadOnlyList<OriginalMapMessengerAcceptanceStage> Stages => _stages;
}

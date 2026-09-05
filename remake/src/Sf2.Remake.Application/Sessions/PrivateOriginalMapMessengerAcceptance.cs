using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapMessengerAcceptanceState
{
    private readonly ReadOnlyCollection<int> _joinedCharacterIds;
    private readonly ReadOnlyCollection<OriginalMapMessengerFollowerLink> _followers;
    private readonly ReadOnlyCollection<OriginalMapMessengerGuardState> _guards;

    private PrivateOriginalMapMessengerAcceptanceState(
        OriginalMapMessengerZoneEventIdentity eventIdentity,
        bool accepted,
        bool flag600Set,
        bool flag66Set,
        bool flag603Set,
        IEnumerable<int> joinedCharacterIds,
        IEnumerable<OriginalMapMessengerFollowerLink> followers,
        IEnumerable<OriginalMapMessengerGuardState> guards)
    {
        EventIdentity = eventIdentity ?? throw new ArgumentNullException(nameof(eventIdentity));
        int[] copiedJoined = [.. joinedCharacterIds];
        OriginalMapMessengerFollowerLink[] copiedFollowers = [.. followers];
        OriginalMapMessengerGuardState[] copiedGuards = [.. guards];
        if (accepted != flag600Set || accepted != flag66Set || accepted != flag603Set ||
            (!accepted && (copiedJoined.Length != 0 || copiedFollowers.Length != 0 ||
                copiedGuards.Length != 0)))
        {
            throw new ArgumentException(
                "Messenger state must retain the exact ready or accepted shape.");
        }

        Accepted = accepted;
        Flag600Set = flag600Set;
        Flag66Set = flag66Set;
        Flag603Set = flag603Set;
        _joinedCharacterIds = Array.AsReadOnly(copiedJoined);
        _followers = Array.AsReadOnly(copiedFollowers);
        _guards = Array.AsReadOnly(copiedGuards);
    }

    public OriginalMapMessengerZoneEventIdentity EventIdentity { get; }
    public bool Accepted { get; }
    public bool Flag600Set { get; }
    public bool Flag66Set { get; }
    public bool Flag603Set { get; }
    public IReadOnlyList<int> JoinedCharacterIds => _joinedCharacterIds;
    public IReadOnlyList<OriginalMapMessengerFollowerLink> Followers => _followers;
    public IReadOnlyList<OriginalMapMessengerGuardState> Guards => _guards;

    internal static PrivateOriginalMapMessengerAcceptanceState Ready(
        OriginalMapMessengerAcceptanceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(
            definition.Identity,
            accepted: false,
            flag600Set: false,
            flag66Set: false,
            flag603Set: false,
            [],
            [],
            []);
    }

    internal static PrivateOriginalMapMessengerAcceptanceState Completed(
        OriginalMapMessengerAcceptanceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(
            definition.Identity,
            accepted: true,
            flag600Set: true,
            flag66Set: true,
            flag603Set: true,
            definition.JoinedCharacterIds,
            definition.Followers,
            definition.Guards);
    }

    internal bool Matches(OriginalMapMessengerAcceptanceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        PrivateOriginalMapMessengerAcceptanceState expected = Accepted
            ? Completed(definition)
            : Ready(definition);
        return EventIdentity == expected.EventIdentity &&
            Accepted == expected.Accepted &&
            Flag600Set == expected.Flag600Set &&
            Flag66Set == expected.Flag66Set &&
            Flag603Set == expected.Flag603Set &&
            JoinedCharacterIds.SequenceEqual(expected.JoinedCharacterIds) &&
            Followers.SequenceEqual(expected.Followers) &&
            Guards.SequenceEqual(expected.Guards);
    }
}

public sealed record PrivateOriginalMapMessengerAcceptanceReceipt
{
    private readonly ReadOnlyCollection<int> _textIds;
    private readonly ReadOnlyCollection<int?> _speakerOperands;
    private readonly ReadOnlyCollection<OriginalMapMessengerAcceptanceStage> _stages;

    internal PrivateOriginalMapMessengerAcceptanceReceipt(
        OriginalMapMessengerAcceptanceDefinition definition,
        MapPosition playerSource,
        OriginalMapTraversalResult traversal,
        PrivateOriginalMapSarahState sarahBefore,
        PrivateOriginalMapSarahState sarahAfter,
        PrivateOriginalMapEntity142State entity142Before,
        PrivateOriginalMapEntity142State entity142After,
        PrivateOriginalMapMessengerAcceptanceState before,
        PrivateOriginalMapMessengerAcceptanceState after,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        PlayerSource = playerSource ?? throw new ArgumentNullException(nameof(playerSource));
        Traversal = traversal ?? throw new ArgumentNullException(nameof(traversal));
        SarahBefore = sarahBefore ?? throw new ArgumentNullException(nameof(sarahBefore));
        SarahAfter = sarahAfter ?? throw new ArgumentNullException(nameof(sarahAfter));
        Entity142Before = entity142Before ??
            throw new ArgumentNullException(nameof(entity142Before));
        Entity142After = entity142After ?? throw new ArgumentNullException(nameof(entity142After));
        Before = before ?? throw new ArgumentNullException(nameof(before));
        After = after ?? throw new ArgumentNullException(nameof(after));
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        EventIdentity = definition.Identity;
        MessengerProgramIdentity = definition.MessengerProgramIdentity;
        AcceptedBranchProgramIdentity = definition.AcceptedBranchProgramIdentity;
        ControlShapeSha256 = definition.ControlShapeSha256;
        PromptReturn = definition.PromptReturn;
        PromptFlag89 = definition.PromptFlag89;
        PromptFlag89Set = true;
        JoinSelector = definition.JoinSelector;
        Flag600 = definition.Flag600;
        Flag66 = definition.Flag66;
        CompletionFlag603 = definition.CompletionFlag603;
        Endpoint = definition.Endpoint;
        EndpointOpaqueFacing = definition.EndpointOpaqueFacing;
        TerminalIdentity = definition.TerminalIdentity;
        _textIds = Array.AsReadOnly(definition.TextIds.ToArray());
        _speakerOperands = Array.AsReadOnly(definition.SpeakerOperands.ToArray());
        _stages = Array.AsReadOnly(definition.Stages.ToArray());
        SimulationStep = simulationStep;
    }

    public OriginalMapMessengerZoneEventIdentity EventIdentity { get; }
    public MapPosition PlayerSource { get; }
    public OriginalMapTraversalResult Traversal { get; }
    public string MessengerProgramIdentity { get; }
    public string AcceptedBranchProgramIdentity { get; }
    public string ControlShapeSha256 { get; }
    public int PromptReturn { get; }
    public int PromptFlag89 { get; }
    public bool PromptFlag89Set { get; }
    public int JoinSelector { get; }
    public int Flag600 { get; }
    public bool Flag600Set => true;
    public int Flag66 { get; }
    public bool Flag66Set => true;
    public int CompletionFlag603 { get; }
    public bool CompletionFlag603Set => true;
    public MapPosition Endpoint { get; }
    public byte EndpointOpaqueFacing { get; }
    public string TerminalIdentity { get; }
    public PrivateOriginalMapSarahState SarahBefore { get; }
    public PrivateOriginalMapSarahState SarahAfter { get; }
    public PrivateOriginalMapEntity142State Entity142Before { get; }
    public PrivateOriginalMapEntity142State Entity142After { get; }
    public PrivateOriginalMapMessengerAcceptanceState Before { get; }
    public PrivateOriginalMapMessengerAcceptanceState After { get; }
    public IReadOnlyList<int> TextIds => _textIds;
    public IReadOnlyList<int?> SpeakerOperands => _speakerOperands;
    public IReadOnlyList<OriginalMapMessengerAcceptanceStage> Stages => _stages;
    public long SimulationStep { get; }

    internal bool Matches(OriginalMapMessengerAcceptanceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return EventIdentity == definition.Identity &&
            PlayerSource == definition.Approach &&
            Traversal.Outcome == OriginalMapTraversalOutcome.Moved &&
            Traversal.Direction == definition.EntryDirection &&
            Traversal.Position == definition.Endpoint &&
            string.Equals(MessengerProgramIdentity, definition.MessengerProgramIdentity,
                StringComparison.Ordinal) &&
            string.Equals(AcceptedBranchProgramIdentity,
                definition.AcceptedBranchProgramIdentity, StringComparison.Ordinal) &&
            string.Equals(ControlShapeSha256, definition.ControlShapeSha256,
                StringComparison.OrdinalIgnoreCase) &&
            PromptReturn == definition.PromptReturn &&
            PromptFlag89 == definition.PromptFlag89 &&
            JoinSelector == definition.JoinSelector &&
            Flag600 == definition.Flag600 && Flag66 == definition.Flag66 &&
            CompletionFlag603 == definition.CompletionFlag603 &&
            Endpoint == definition.Endpoint &&
            EndpointOpaqueFacing == definition.EndpointOpaqueFacing &&
            string.Equals(TerminalIdentity, definition.TerminalIdentity,
                StringComparison.Ordinal) &&
            SarahBefore.Phase == PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned &&
            SarahAfter.Phase == PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady &&
            Entity142Before.Flag602Set && !Entity142Before.RouteOccupancyReleased &&
            Entity142After.Flag602Set && Entity142After.RouteOccupancyReleased &&
            Before.Matches(definition) && !Before.Accepted &&
            After.Matches(definition) && After.Accepted &&
            TextIds.SequenceEqual(definition.TextIds) &&
            SpeakerOperands.SequenceEqual(definition.SpeakerOperands) &&
            Stages.SequenceEqual(definition.Stages);
    }
}

public sealed partial class GameSession
{
    private bool TryApplyPrivateOriginalMapMessengerAcceptance(
        PrivateOriginalMapSessionSnapshot current,
        MoveExplorationCommand command,
        out PrivateOriginalMapMoveApplied? applied)
    {
        applied = null;
        OriginalMapMessengerAcceptanceDefinition? definition =
            current.Definition.MessengerAcceptance;
        PrivateOriginalMapMessengerAcceptanceState? before = current.MessengerAcceptance;
        PrivateOriginalMapSarahState? sarahBefore = current.Sarah;
        PrivateOriginalMapEntity142State? entity142Before = current.Entity142;
        if (definition is null || before?.Accepted != false ||
            current.PendingEntity142 is not null ||
            current.AstralZoneFlag260Set != true ||
            sarahBefore?.Phase != PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned ||
            entity142Before?.Flag602Set != true || entity142Before.RouteOccupancyReleased)
        {
            return false;
        }

        if (current.PlayerPosition != definition.Approach ||
            command.Direction != definition.EntryDirection)
        {
            return false;
        }

        MapPosition? candidate = current.Definition.Traversal.ResolveCandidateTarget(
            current.WorkingLayout,
            current.PlayerPosition,
            command.Direction);
        if (candidate != definition.Trigger)
        {
            return false;
        }

        OriginalMapTraversalResult traversal = current.Definition.Traversal.TryMove(
            current.WorkingLayout,
            current.PlayerPosition,
            command.Direction);
        if (traversal.Outcome != OriginalMapTraversalOutcome.Moved ||
            traversal.Position != definition.Endpoint)
        {
            throw new InvalidOperationException(
                "The admitted messenger candidate did not produce its exact endpoint traversal.");
        }

        OriginalMapSarahDefinition sarahDefinition = current.Definition.Sarah ??
            throw new InvalidOperationException("Messenger acceptance has no Sarah definition.");
        OriginalMapEntity142Definition entity142Definition = current.Definition.Entity142 ??
            throw new InvalidOperationException("Messenger acceptance has no Entity 142 definition.");
        OriginalMapAstralZoneDefinition astralDefinition = current.Definition.AstralZone ??
            throw new InvalidOperationException("Messenger acceptance has no Astral definition.");
        PrivateOriginalMapSarahState sarahAfter =
            PrivateOriginalMapSarahState.MessengerFollowerReady(
                sarahDefinition,
                astralDefinition,
                definition);
        PrivateOriginalMapEntity142State entity142After =
            PrivateOriginalMapEntity142State.ReleaseRouteOccupancy(
                entity142Definition,
                entity142Before,
                definition);
        PrivateOriginalMapMessengerAcceptanceState after =
            PrivateOriginalMapMessengerAcceptanceState.Completed(definition);
        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapMessengerAcceptanceReceipt receipt = new(
            definition,
            current.PlayerPosition,
            traversal,
            sarahBefore,
            sarahAfter,
            entity142Before,
            entity142After,
            before,
            after,
            nextStep);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            current.WorkingLayout,
            nextStep,
            traversal.Position,
            traversal,
            current.ControlledStepCopyApplied,
            lastLayoutMutation: null,
            lastSameMapWarp: null,
            roofOnLoadLifecycle: current.RoofOnLoadLifecycle,
            lastRoofOnLoad: null,
            current.BowieDoorStepCopyApplied,
            lastNaturalStepCopy: null,
            current.SchoolDoorStepCopyApplied,
            current.Zone601,
            lastZone601: null,
            sarahAfter,
            lastSarah: null,
            entity142After,
            pendingEntity142: null,
            lastEntity142Request: null,
            lastEntity142Acknowledgement: null,
            lastAstralZone: null,
            messengerAcceptance: after,
            lastMessengerAcceptance: receipt,
            current.CastleGate,
            lastCastleGate: null,
            current.CurrentRuntime,
            lastCrossMapTransition: null);
        _privateOriginalMapSnapshot = next;
        applied = new PrivateOriginalMapMoveApplied(
            next,
            traversal,
            messengerAcceptance: receipt);
        return true;
    }
}

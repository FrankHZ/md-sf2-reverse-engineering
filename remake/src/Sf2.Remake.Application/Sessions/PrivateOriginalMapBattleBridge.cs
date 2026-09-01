using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public static class PrivateOriginalMapBattleBridgeAdmission
{
    public const string Capability = "private-local-map3-remake-battle-bridge-v1";
    public const string BridgeId = "private-local-map3-controlled-start-battle-bridge";
}

public sealed record PrivateOriginalMapBattleBridgeId
{
    public PrivateOriginalMapBattleBridgeId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public enum PrivateOriginalMapBattleBridgeStatus
{
    Ready,
    Pending,
    Active,
    Completed,
    Returned,
}

public sealed class PrivateOriginalMapBattleBridgeDefinition
{
    internal PrivateOriginalMapBattleBridgeDefinition(
        MapId triggerMap,
        MapPosition triggerPosition,
        PublicSyntheticBattleDefinition source)
    {
        TriggerMap = triggerMap ?? throw new ArgumentNullException(nameof(triggerMap));
        TriggerPosition = triggerPosition ??
            throw new ArgumentNullException(nameof(triggerPosition));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Bridge = new PrivateOriginalMapBattleBridgeId(
            PrivateOriginalMapBattleBridgeAdmission.BridgeId);
    }

    public string Capability => PrivateOriginalMapBattleBridgeAdmission.Capability;

    public PrivateOriginalMapBattleBridgeId Bridge { get; }

    public MapId TriggerMap { get; }

    public MapPosition TriggerPosition { get; }

    public PublicSyntheticBattleRequestId Request => Source.Request;

    public TacticalBattleRules Rules => Source.Rules;

    public IReadOnlyList<PresentationCueId> Cues => Source.Cues;

    internal PublicSyntheticBattleDefinition Source { get; }
}

public sealed class PrivateOriginalMapBattleBridgeSnapshot
{
    private PrivateOriginalMapBattleBridgeSnapshot(
        PrivateOriginalMapBattleBridgeDefinition definition,
        PrivateOriginalMapBattleBridgeStatus status,
        long operationSequence,
        long lastCueSequence,
        PrivateOriginalMapSessionSnapshot? returnSnapshot,
        PublicSyntheticBattleLifecycleSnapshot? lifecycle,
        PublicSyntheticBattleCompletionReceipt? completion)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(operationSequence);
        ArgumentOutOfRangeException.ThrowIfNegative(lastCueSequence);
        if (status == PrivateOriginalMapBattleBridgeStatus.Ready &&
            (operationSequence != 0 || lastCueSequence != 0 || returnSnapshot is not null ||
             lifecycle is not null || completion is not null))
        {
            throw new ArgumentException("A ready private battle bridge cannot retain runtime state.");
        }

        bool lifecycleStatus = status is
            PrivateOriginalMapBattleBridgeStatus.Pending or
            PrivateOriginalMapBattleBridgeStatus.Active or
            PrivateOriginalMapBattleBridgeStatus.Completed;
        if (lifecycleStatus != (lifecycle is not null) ||
            (lifecycleStatus && returnSnapshot is null))
        {
            throw new ArgumentException(
                "An in-flight private battle bridge requires one lifecycle and return snapshot.");
        }

        if (lifecycle is not null &&
            (lifecycle.Definition != definition.Source ||
             lifecycle.LastCueSequence != lastCueSequence ||
             status != ToPrivateStatus(lifecycle.Status)))
        {
            throw new ArgumentException(
                "The private bridge state must retain its exact public-synthetic tactical lifecycle.");
        }

        if (status == PrivateOriginalMapBattleBridgeStatus.Returned !=
            (completion is not null))
        {
            throw new ArgumentException(
                "Only a returned private battle bridge retains a completion receipt.");
        }

        if (status == PrivateOriginalMapBattleBridgeStatus.Returned &&
            (returnSnapshot is null || operationSequence < 1 || lastCueSequence < 1))
        {
            throw new ArgumentException(
                "A returned private battle bridge requires its exact traversal snapshot and sequences.");
        }

        Status = status;
        OperationSequence = operationSequence;
        LastCueSequence = lastCueSequence;
        BattleState = lifecycle?.BattleState;
        Completion = completion;
        ReturnSnapshot = returnSnapshot;
        Lifecycle = lifecycle;
    }

    public PrivateOriginalMapBattleBridgeDefinition Definition { get; }

    public PrivateOriginalMapBattleBridgeStatus Status { get; }

    public long OperationSequence { get; }

    public long LastCueSequence { get; }

    public TacticalBattleState? BattleState { get; }

    public PublicSyntheticBattleCompletionReceipt? Completion { get; }

    internal PrivateOriginalMapSessionSnapshot? ReturnSnapshot { get; }

    internal PublicSyntheticBattleLifecycleSnapshot? Lifecycle { get; }

    public bool IsBusy => Status is
        PrivateOriginalMapBattleBridgeStatus.Pending or
        PrivateOriginalMapBattleBridgeStatus.Active or
        PrivateOriginalMapBattleBridgeStatus.Completed;

    internal static PrivateOriginalMapBattleBridgeSnapshot Ready(
        PrivateOriginalMapBattleBridgeDefinition definition) =>
        new(
            definition,
            PrivateOriginalMapBattleBridgeStatus.Ready,
            operationSequence: 0,
            lastCueSequence: 0,
            returnSnapshot: null,
            lifecycle: null,
            completion: null);

    internal static PrivateOriginalMapBattleBridgeSnapshot Pending(
        PrivateOriginalMapBattleBridgeDefinition definition,
        PrivateOriginalMapSessionSnapshot returnSnapshot)
    {
        const long operationSequence = 1;
        const long cueSequence = 1;
        return new(
            definition,
            PrivateOriginalMapBattleBridgeStatus.Pending,
            operationSequence,
            cueSequence,
            returnSnapshot,
            PublicSyntheticBattleLifecycleSnapshot.Pending(
                definition.Source,
                operationSequence,
                cueSequence),
            completion: null);
    }

    internal PrivateOriginalMapBattleBridgeSnapshot Update(
        PublicSyntheticBattleLifecycleSnapshot lifecycle,
        long operationSequence) =>
        new(
            Definition,
            ToPrivateStatus(lifecycle.Status),
            operationSequence,
            lifecycle.LastCueSequence,
            ReturnSnapshot,
            lifecycle,
            completion: null);

    internal PrivateOriginalMapBattleBridgeSnapshot Return(
        PublicSyntheticBattleCompletionReceipt completion,
        long operationSequence,
        long cueSequence) =>
        new(
            Definition,
            PrivateOriginalMapBattleBridgeStatus.Returned,
            operationSequence,
            cueSequence,
            ReturnSnapshot,
            lifecycle: null,
            completion);

    private static PrivateOriginalMapBattleBridgeStatus ToPrivateStatus(
        PublicSyntheticBattleLifecycleStatus status) =>
        status switch
        {
            PublicSyntheticBattleLifecycleStatus.Pending =>
                PrivateOriginalMapBattleBridgeStatus.Pending,
            PublicSyntheticBattleLifecycleStatus.Active =>
                PrivateOriginalMapBattleBridgeStatus.Active,
            PublicSyntheticBattleLifecycleStatus.Completed =>
                PrivateOriginalMapBattleBridgeStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };
}

public enum PrivateOriginalMapBattleBridgeFailureCode
{
    AlreadyBound,
    VisualBindingMismatch,
    BattleDefinitionUnavailable,
    NotBound,
    WrongState,
    WrongTrigger,
    StaleTraversalStep,
    AcknowledgementMismatch,
    BattleNotActive,
    BattleNotCompleted,
    InvalidSelection,
    UnsupportedCommand,
}

public sealed record PrivateOriginalMapBattleBridgeDiagnostic
{
    public PrivateOriginalMapBattleBridgeDiagnostic(
        PrivateOriginalMapBattleBridgeFailureCode code,
        string message)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Message = message;
    }

    public PrivateOriginalMapBattleBridgeFailureCode Code { get; }

    public string Message { get; }
}

public abstract record PrivateOriginalMapBattleBridgeBindingResult;

public sealed record PrivateOriginalMapBattleBridgeBound(
    PrivateOriginalMapBattleBridgeSnapshot Bridge) :
    PrivateOriginalMapBattleBridgeBindingResult
{
    public PrivateOriginalMapBattleBridgeSnapshot Bridge { get; } =
        Bridge ?? throw new ArgumentNullException(nameof(Bridge));
}

public sealed record PrivateOriginalMapBattleBridgeBindingRejected(
    PrivateOriginalMapBattleBridgeDiagnostic Diagnostic) :
    PrivateOriginalMapBattleBridgeBindingResult
{
    public PrivateOriginalMapBattleBridgeDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public sealed record RequestPrivateOriginalMapBattleBridgeCommand : IGameSessionCommand
{
    public RequestPrivateOriginalMapBattleBridgeCommand(
        PrivateOriginalMapBattleBridgeId bridge,
        long expectedTraversalStep)
    {
        Bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
        ArgumentOutOfRangeException.ThrowIfNegative(expectedTraversalStep);
        ExpectedTraversalStep = expectedTraversalStep;
    }

    public PrivateOriginalMapBattleBridgeId Bridge { get; }

    public long ExpectedTraversalStep { get; }
}

public sealed record PrivateOriginalMapBattleBridgeRequested(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapBattleBridgeSnapshot Bridge,
    PublicSyntheticBattleCue Cue) : GameSessionCommandResult;

public sealed record PrivateOriginalMapBattleBridgeAdmitted(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapBattleBridgeSnapshot Bridge,
    PublicSyntheticBattleCue Cue) : GameSessionCommandResult;

public sealed record PrivateOriginalMapBattleBridgeCursorMoved(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapBattleBridgeSnapshot Bridge,
    TacticalCursorMoveOutcome Outcome) : GameSessionCommandResult;

public sealed record PrivateOriginalMapBattleBridgeSelectionConfirmed : GameSessionCommandResult
{
    public PrivateOriginalMapBattleBridgeSelectionConfirmed(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapBattleBridgeSnapshot bridge,
        TacticalSelectionOutcome outcome,
        IEnumerable<PublicSyntheticBattleCue> cues,
        PublicSyntheticBattleCompletionReceipt? completion)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        ArgumentNullException.ThrowIfNull(cues);
        PublicSyntheticBattleCue[] copied = [.. cues];
        if (copied.Any(cue => cue is null))
        {
            throw new ArgumentException("Private battle bridge cues cannot contain null values.", nameof(cues));
        }

        bool completed = outcome == TacticalSelectionOutcome.BattleCompleted;
        if (completed != (completion is not null) ||
            (completed ? copied.Length != 2 : copied.Length != 1))
        {
            throw new ArgumentException(
                "Private battle bridge outcome, cues, and completion must agree.",
                nameof(completion));
        }

        Outcome = outcome;
        Cues = Array.AsReadOnly(copied);
        Completion = completion;
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public PrivateOriginalMapBattleBridgeSnapshot Bridge { get; }

    public TacticalSelectionOutcome Outcome { get; }

    public IReadOnlyList<PublicSyntheticBattleCue> Cues { get; }

    public PublicSyntheticBattleCompletionReceipt? Completion { get; }
}

public sealed record PrivateOriginalMapBattleBridgeSelectionCancelled(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapBattleBridgeSnapshot Bridge,
    TacticalCancelOutcome Outcome) : GameSessionCommandResult;

public sealed record PrivateOriginalMapBattleBridgeReturned(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapBattleBridgeSnapshot Bridge,
    PublicSyntheticBattleCompletionReceipt Completion,
    PublicSyntheticBattleCue Cue) : GameSessionCommandResult;

public sealed record PrivateOriginalMapBattleBridgeRejected(
    PrivateOriginalMapSessionSnapshot Snapshot,
    PrivateOriginalMapBattleBridgeSnapshot? Bridge,
    PrivateOriginalMapBattleBridgeDiagnostic Diagnostic) : GameSessionCommandResult;

public sealed partial class GameSession
{
    private PrivateOriginalMapBattleBridgeSnapshot? _privateOriginalMapBattleBridge;

    public PrivateOriginalMapBattleBridgeSnapshot? PrivateOriginalMapBattleBridge =>
        _privateOriginalMapBattleBridge;

    public PrivateOriginalMapBattleBridgeBindingResult BindPrivateOriginalMapBattleBridge(
        PrivateOriginalMapVisualRuntimeBinding visualBinding,
        PublicSyntheticBattleDefinition battle)
    {
        ArgumentNullException.ThrowIfNull(visualBinding);
        ArgumentNullException.ThrowIfNull(battle);
        PrivateOriginalMapSessionSnapshot current = PrivateOriginalMapSnapshot;
        if (_privateOriginalMapBattleBridge is not null)
        {
            return RejectBinding(
                PrivateOriginalMapBattleBridgeFailureCode.AlreadyBound,
                "The private battle bridge is already bound to this session.");
        }

        if (!SameSelection(
                current.Definition.VisualResourceSelection,
                visualBinding.Definition.Selection) ||
            !string.Equals(
                visualBinding.Capability,
                PrivateOriginalMapVisualRuntimeAdmission.Capability,
                StringComparison.Ordinal))
        {
            return RejectBinding(
                PrivateOriginalMapBattleBridgeFailureCode.VisualBindingMismatch,
                "The private battle bridge requires the exact admitted Map 3 visual binding.");
        }

        OriginalMapControlledAdmission controlled = current.Definition.ControlledAdmission;
        if (current.SimulationStep != 0 ||
            current.Map != controlled.Map ||
            current.PlayerPosition != controlled.Position)
        {
            return RejectBinding(
                PrivateOriginalMapBattleBridgeFailureCode.WrongTrigger,
                "The private battle bridge can be bound only at the admitted controlled start.");
        }

        PrivateOriginalMapBattleBridgeDefinition definition = new(
            controlled.Map,
            controlled.Position,
            battle);
        _privateOriginalMapBattleBridge =
            PrivateOriginalMapBattleBridgeSnapshot.Ready(definition);
        return new PrivateOriginalMapBattleBridgeBound(_privateOriginalMapBattleBridge);
    }

    public GameSessionCommandResult ApplyPrivateOriginalMapBattleBridge(IGameSessionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        PrivateOriginalMapSessionSnapshot current = PrivateOriginalMapSnapshot;
        PrivateOriginalMapBattleBridgeSnapshot? bridge = _privateOriginalMapBattleBridge;
        if (bridge is null)
        {
            return RejectBridge(
                current,
                null,
                PrivateOriginalMapBattleBridgeFailureCode.NotBound,
                "No private battle bridge is bound to this session.");
        }

        if (bridge.Status == PrivateOriginalMapBattleBridgeStatus.Pending &&
            command is not AcknowledgePublicSyntheticBattleEntryCommand)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.WrongState,
                "The pending private battle entry must be acknowledged first.");
        }

        if (bridge.Status is
                PrivateOriginalMapBattleBridgeStatus.Active or
                PrivateOriginalMapBattleBridgeStatus.Completed &&
            command is not MovePublicSyntheticBattleCursorCommand and not
                ConfirmPublicSyntheticBattleSelectionCommand and not
                CancelPublicSyntheticBattleSelectionCommand and not
                AcknowledgePublicSyntheticBattleCompletionCommand)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.WrongState,
                "Only tactical battle commands are admitted while the private bridge is active.");
        }

        return command switch
        {
            RequestPrivateOriginalMapBattleBridgeCommand request =>
                ApplyPrivateBattleBridgeRequest(current, bridge, request),
            AcknowledgePublicSyntheticBattleEntryCommand acknowledge =>
                ApplyPrivateBattleBridgeEntryAcknowledgement(current, bridge, acknowledge),
            MovePublicSyntheticBattleCursorCommand move =>
                ApplyPrivateBattleBridgeCursorMove(current, bridge, move),
            ConfirmPublicSyntheticBattleSelectionCommand =>
                ApplyPrivateBattleBridgeSelectionConfirmation(current, bridge),
            CancelPublicSyntheticBattleSelectionCommand =>
                ApplyPrivateBattleBridgeSelectionCancellation(current, bridge),
            AcknowledgePublicSyntheticBattleCompletionCommand acknowledge =>
                ApplyPrivateBattleBridgeCompletionAcknowledgement(current, bridge, acknowledge),
            _ => RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.UnsupportedCommand,
                "The private battle bridge does not admit this command."),
        };
    }

    internal bool IsPrivateOriginalMapBattleBridgeBusy =>
        _privateOriginalMapBattleBridge?.IsBusy == true;

    private GameSessionCommandResult ApplyPrivateBattleBridgeRequest(
        PrivateOriginalMapSessionSnapshot current,
        PrivateOriginalMapBattleBridgeSnapshot bridge,
        RequestPrivateOriginalMapBattleBridgeCommand command)
    {
        if (bridge.Status != PrivateOriginalMapBattleBridgeStatus.Ready)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.WrongState,
                "The private battle bridge is not ready for another request.");
        }

        if (command.Bridge != bridge.Definition.Bridge ||
            current.Map != bridge.Definition.TriggerMap ||
            current.PlayerPosition != bridge.Definition.TriggerPosition)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.WrongTrigger,
                "The explicit private battle bridge trigger does not match the live controlled-start state.");
        }

        if (command.ExpectedTraversalStep != current.SimulationStep)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.StaleTraversalStep,
                "The private battle bridge request targets a stale traversal step.");
        }

        PrivateOriginalMapBattleBridgeSnapshot pending =
            PrivateOriginalMapBattleBridgeSnapshot.Pending(bridge.Definition, current);
        _privateOriginalMapBattleBridge = pending;
        PublicSyntheticBattleCue cue = new(
            bridge.Definition.Source.RequestCue,
            PublicSyntheticBattleCueKind.EntryRequested,
            bridge.Definition.Request,
            bridge.Definition.Rules.Battle,
            pending.LastCueSequence);
        return new PrivateOriginalMapBattleBridgeRequested(current, pending, cue);
    }

    private GameSessionCommandResult ApplyPrivateBattleBridgeEntryAcknowledgement(
        PrivateOriginalMapSessionSnapshot current,
        PrivateOriginalMapBattleBridgeSnapshot bridge,
        AcknowledgePublicSyntheticBattleEntryCommand command)
    {
        PublicSyntheticBattleLifecycleSnapshot? pending = bridge.Lifecycle;
        if (bridge.Status != PrivateOriginalMapBattleBridgeStatus.Pending || pending is null)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.WrongState,
                "No private battle bridge entry is pending.");
        }

        if (command.Request != bridge.Definition.Request ||
            command.Battle != bridge.Definition.Rules.Battle ||
            command.CueSequence != pending.EntryCueSequence)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.AcknowledgementMismatch,
                "The private battle bridge entry acknowledgement does not match.");
        }

        long operationSequence = checked(bridge.OperationSequence + 1);
        long cueSequence = checked(bridge.LastCueSequence + 1);
        PrivateOriginalMapBattleBridgeSnapshot admitted = bridge.Update(
            pending.Admit(operationSequence, cueSequence),
            operationSequence);
        _privateOriginalMapBattleBridge = admitted;
        PublicSyntheticBattleCue cue = new(
            bridge.Definition.Source.AdmittedCue,
            PublicSyntheticBattleCueKind.BattleAdmitted,
            bridge.Definition.Request,
            bridge.Definition.Rules.Battle,
            cueSequence);
        return new PrivateOriginalMapBattleBridgeAdmitted(current, admitted, cue);
    }

    private GameSessionCommandResult ApplyPrivateBattleBridgeCursorMove(
        PrivateOriginalMapSessionSnapshot current,
        PrivateOriginalMapBattleBridgeSnapshot bridge,
        MovePublicSyntheticBattleCursorCommand command)
    {
        if (bridge.Status != PrivateOriginalMapBattleBridgeStatus.Active ||
            bridge.Lifecycle?.BattleState is not TacticalBattleState battleState)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.BattleNotActive,
                "No active private bridge battle accepts cursor movement.");
        }

        TacticalCursorMoveResult moved = TacticalBattleReducer.MoveCursor(
            battleState,
            command.Direction);
        if (moved.Outcome != TacticalCursorMoveOutcome.Moved)
        {
            return new PrivateOriginalMapBattleBridgeCursorMoved(
                current,
                bridge,
                moved.Outcome);
        }

        long operationSequence = checked(bridge.OperationSequence + 1);
        PrivateOriginalMapBattleBridgeSnapshot updated = bridge.Update(
            bridge.Lifecycle.Update(
                moved.State,
                operationSequence,
                bridge.LastCueSequence),
            operationSequence);
        _privateOriginalMapBattleBridge = updated;
        return new PrivateOriginalMapBattleBridgeCursorMoved(
            current,
            updated,
            moved.Outcome);
    }

    private GameSessionCommandResult ApplyPrivateBattleBridgeSelectionConfirmation(
        PrivateOriginalMapSessionSnapshot current,
        PrivateOriginalMapBattleBridgeSnapshot bridge)
    {
        if (bridge.Status != PrivateOriginalMapBattleBridgeStatus.Active ||
            bridge.Lifecycle?.BattleState is not TacticalBattleState battleState)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.BattleNotActive,
                "No active private bridge battle accepts selection confirmation.");
        }

        TacticalSelectionResult confirmed = TacticalBattleReducer.Confirm(battleState);
        if (confirmed.Outcome == TacticalSelectionOutcome.InvalidSelection)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.InvalidSelection,
                "The current private bridge tactical selection is not legal.");
        }

        long operationSequence = checked(bridge.OperationSequence + 1);
        long firstCueSequence = checked(bridge.LastCueSequence + 1);
        List<PublicSyntheticBattleCue> cues = [];
        PublicSyntheticBattleCompletionReceipt? completion = null;
        long lastCueSequence;
        if (confirmed.Outcome == TacticalSelectionOutcome.MoveConfirmed)
        {
            lastCueSequence = firstCueSequence;
            cues.Add(Cue(
                bridge,
                bridge.Definition.Source.MoveCue,
                PublicSyntheticBattleCueKind.MoveConfirmed,
                firstCueSequence));
        }
        else if (confirmed.Outcome == TacticalSelectionOutcome.AttackConfirmed)
        {
            lastCueSequence = firstCueSequence;
            cues.Add(Cue(
                bridge,
                bridge.Definition.Source.AttackCue,
                PublicSyntheticBattleCueKind.AttackCompleted,
                firstCueSequence));
        }
        else
        {
            lastCueSequence = checked(firstCueSequence + 1);
            cues.Add(Cue(
                bridge,
                bridge.Definition.Source.AttackCue,
                PublicSyntheticBattleCueKind.AttackCompleted,
                firstCueSequence));
            cues.Add(Cue(
                bridge,
                bridge.Definition.Source.CompletedCue,
                PublicSyntheticBattleCueKind.BattleCompleted,
                lastCueSequence));
            completion = new PublicSyntheticBattleCompletionReceipt(
                bridge.Definition.Rules.Battle,
                bridge.Definition.Rules.Actor,
                bridge.Definition.Rules.Enemy,
                operationSequence,
                lastCueSequence);
        }

        PrivateOriginalMapBattleBridgeSnapshot updated = bridge.Update(
            bridge.Lifecycle.Update(
                confirmed.State,
                operationSequence,
                lastCueSequence),
            operationSequence);
        _privateOriginalMapBattleBridge = updated;
        return new PrivateOriginalMapBattleBridgeSelectionConfirmed(
            current,
            updated,
            confirmed.Outcome,
            cues,
            completion);
    }

    private GameSessionCommandResult ApplyPrivateBattleBridgeSelectionCancellation(
        PrivateOriginalMapSessionSnapshot current,
        PrivateOriginalMapBattleBridgeSnapshot bridge)
    {
        if (bridge.Status != PrivateOriginalMapBattleBridgeStatus.Active ||
            bridge.Lifecycle?.BattleState is not TacticalBattleState battleState)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.BattleNotActive,
                "No active private bridge battle accepts cancellation.");
        }

        TacticalCancelResult cancelled = TacticalBattleReducer.Cancel(battleState);
        if (cancelled.Outcome != TacticalCancelOutcome.ReturnedToMoveSelection)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.InvalidSelection,
                "The current private bridge tactical selection cannot be cancelled.");
        }

        long operationSequence = checked(bridge.OperationSequence + 1);
        PrivateOriginalMapBattleBridgeSnapshot updated = bridge.Update(
            bridge.Lifecycle.Update(
                cancelled.State,
                operationSequence,
                bridge.LastCueSequence),
            operationSequence);
        _privateOriginalMapBattleBridge = updated;
        return new PrivateOriginalMapBattleBridgeSelectionCancelled(
            current,
            updated,
            cancelled.Outcome);
    }

    private GameSessionCommandResult ApplyPrivateBattleBridgeCompletionAcknowledgement(
        PrivateOriginalMapSessionSnapshot current,
        PrivateOriginalMapBattleBridgeSnapshot bridge,
        AcknowledgePublicSyntheticBattleCompletionCommand command)
    {
        PublicSyntheticBattleLifecycleSnapshot? lifecycle = bridge.Lifecycle;
        if (bridge.Status != PrivateOriginalMapBattleBridgeStatus.Completed ||
            lifecycle?.CompletedAtStep is null)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.BattleNotCompleted,
                "No completed private bridge battle is ready to return.");
        }

        if (command.Battle != bridge.Definition.Rules.Battle ||
            command.CompletionCueSequence != bridge.LastCueSequence)
        {
            return RejectBridge(
                current,
                bridge,
                PrivateOriginalMapBattleBridgeFailureCode.AcknowledgementMismatch,
                "The private bridge completion acknowledgement does not match.");
        }

        PrivateOriginalMapSessionSnapshot returnSnapshot =
            bridge.ReturnSnapshot ?? throw new InvalidOperationException(
                "The private battle bridge lost its return snapshot.");
        if (!ReferenceEquals(current, returnSnapshot))
        {
            throw new InvalidOperationException(
                "The private traversal snapshot changed while the battle bridge was active.");
        }

        PublicSyntheticBattleCompletionReceipt completion = new(
            bridge.Definition.Rules.Battle,
            bridge.Definition.Rules.Actor,
            bridge.Definition.Rules.Enemy,
            lifecycle.CompletedAtStep.Value,
            bridge.LastCueSequence);
        long operationSequence = checked(bridge.OperationSequence + 1);
        long cueSequence = checked(bridge.LastCueSequence + 1);
        PrivateOriginalMapBattleBridgeSnapshot returned = bridge.Return(
            completion,
            operationSequence,
            cueSequence);
        _privateOriginalMapBattleBridge = returned;
        PublicSyntheticBattleCue cue = Cue(
            bridge,
            bridge.Definition.Source.ReturnedCue,
            PublicSyntheticBattleCueKind.ReturnedToExploration,
            cueSequence);
        return new PrivateOriginalMapBattleBridgeReturned(
            returnSnapshot,
            returned,
            completion,
            cue);
    }

    private static PublicSyntheticBattleCue Cue(
        PrivateOriginalMapBattleBridgeSnapshot bridge,
        PresentationCueId cue,
        PublicSyntheticBattleCueKind kind,
        long sequence) =>
        new(
            cue,
            kind,
            bridge.Definition.Request,
            bridge.Definition.Rules.Battle,
            sequence);

    private static PrivateOriginalMapBattleBridgeBindingRejected RejectBinding(
        PrivateOriginalMapBattleBridgeFailureCode code,
        string message) =>
        new(new PrivateOriginalMapBattleBridgeDiagnostic(code, message));

    private static PrivateOriginalMapBattleBridgeRejected RejectBridge(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapBattleBridgeSnapshot? bridge,
        PrivateOriginalMapBattleBridgeFailureCode code,
        string message) =>
        new(
            snapshot,
            bridge,
            new PrivateOriginalMapBattleBridgeDiagnostic(code, message));
}

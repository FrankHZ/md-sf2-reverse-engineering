using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum PublicSyntheticBattleLifecycleStatus
{
    Pending,
    Active,
    Completed,
}

public enum PublicSyntheticBattleCueKind
{
    EntryRequested,
    BattleAdmitted,
    MoveConfirmed,
    AttackCompleted,
    BattleCompleted,
    ReturnedToExploration,
}

public sealed record PublicSyntheticBattleCue
{
    public PublicSyntheticBattleCue(
        PresentationCueId cue,
        PublicSyntheticBattleCueKind kind,
        PublicSyntheticBattleRequestId request,
        TacticalBattleId battle,
        long sequence)
    {
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        Request = request ?? throw new ArgumentNullException(nameof(request));
        Battle = battle ?? throw new ArgumentNullException(nameof(battle));
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        Kind = kind;
        Sequence = sequence;
    }

    public PresentationCueId Cue { get; }

    public PublicSyntheticBattleCueKind Kind { get; }

    public PublicSyntheticBattleRequestId Request { get; }

    public TacticalBattleId Battle { get; }

    public long Sequence { get; }
}

public sealed record PublicSyntheticBattleLifecycleSnapshot
{
    private PublicSyntheticBattleLifecycleSnapshot(
        PublicSyntheticBattleDefinition definition,
        PublicSyntheticBattleLifecycleStatus status,
        long requestedAtStep,
        long entryCueSequence,
        TacticalBattleState? battleState,
        long? admittedAtStep,
        long? completedAtStep,
        long lastCueSequence)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(requestedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(entryCueSequence, 1);
        if (lastCueSequence < entryCueSequence)
        {
            throw new ArgumentOutOfRangeException(nameof(lastCueSequence));
        }

        if (status == PublicSyntheticBattleLifecycleStatus.Pending &&
            (battleState is not null || admittedAtStep is not null || completedAtStep is not null))
        {
            throw new ArgumentException("Pending public-synthetic battle state cannot be active.");
        }

        if (status != PublicSyntheticBattleLifecycleStatus.Pending &&
            (battleState is null || admittedAtStep is null || admittedAtStep <= requestedAtStep))
        {
            throw new ArgumentException("Active public-synthetic battle state requires admission.");
        }

        if (status == PublicSyntheticBattleLifecycleStatus.Completed !=
            (battleState?.Phase == TacticalBattlePhase.Completed))
        {
            throw new ArgumentException(
                "Public-synthetic battle lifecycle and Domain completion must agree.");
        }

        if (status == PublicSyntheticBattleLifecycleStatus.Completed !=
            (completedAtStep is not null))
        {
            throw new ArgumentException(
                "Only a completed public-synthetic battle has a completion step.");
        }

        Status = status;
        RequestedAtStep = requestedAtStep;
        EntryCueSequence = entryCueSequence;
        BattleState = battleState;
        AdmittedAtStep = admittedAtStep;
        CompletedAtStep = completedAtStep;
        LastCueSequence = lastCueSequence;
    }

    public PublicSyntheticBattleDefinition Definition { get; }

    public PublicSyntheticBattleLifecycleStatus Status { get; }

    public long RequestedAtStep { get; }

    public long EntryCueSequence { get; }

    public TacticalBattleState? BattleState { get; }

    public long? AdmittedAtStep { get; }

    public long? CompletedAtStep { get; }

    public long LastCueSequence { get; }

    internal static PublicSyntheticBattleLifecycleSnapshot Pending(
        PublicSyntheticBattleDefinition definition,
        long requestedAtStep,
        long cueSequence) =>
        new(
            definition,
            PublicSyntheticBattleLifecycleStatus.Pending,
            requestedAtStep,
            cueSequence,
            battleState: null,
            admittedAtStep: null,
            completedAtStep: null,
            cueSequence);

    internal PublicSyntheticBattleLifecycleSnapshot Admit(
        long admittedAtStep,
        long cueSequence) =>
        new(
            Definition,
            PublicSyntheticBattleLifecycleStatus.Active,
            RequestedAtStep,
            EntryCueSequence,
            Definition.Rules.CreateInitialState(),
            admittedAtStep,
            completedAtStep: null,
            cueSequence);

    internal PublicSyntheticBattleLifecycleSnapshot Update(
        TacticalBattleState state,
        long changedAtStep,
        long cueSequence)
    {
        ArgumentNullException.ThrowIfNull(state);
        bool completed = state.Phase == TacticalBattlePhase.Completed;
        return new(
            Definition,
            completed
                ? PublicSyntheticBattleLifecycleStatus.Completed
                : PublicSyntheticBattleLifecycleStatus.Active,
            RequestedAtStep,
            EntryCueSequence,
            state,
            AdmittedAtStep,
            completed ? changedAtStep : null,
            cueSequence);
    }
}

public sealed record RequestSelectedPublicSyntheticBattleCommand : IGameSessionCommand;

public sealed record AcknowledgePublicSyntheticBattleEntryCommand : IGameSessionCommand
{
    public AcknowledgePublicSyntheticBattleEntryCommand(
        PublicSyntheticBattleRequestId request,
        TacticalBattleId battle,
        long cueSequence)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Battle = battle ?? throw new ArgumentNullException(nameof(battle));
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        CueSequence = cueSequence;
    }

    public PublicSyntheticBattleRequestId Request { get; }

    public TacticalBattleId Battle { get; }

    public long CueSequence { get; }
}

public sealed record MovePublicSyntheticBattleCursorCommand : IGameSessionCommand
{
    public MovePublicSyntheticBattleCursorCommand(TacticalDirection direction)
    {
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        Direction = direction;
    }

    public TacticalDirection Direction { get; }
}

public sealed record ConfirmPublicSyntheticBattleSelectionCommand : IGameSessionCommand;

public sealed record CancelPublicSyntheticBattleSelectionCommand : IGameSessionCommand;

public sealed record AcknowledgePublicSyntheticBattleCompletionCommand : IGameSessionCommand
{
    public AcknowledgePublicSyntheticBattleCompletionCommand(
        TacticalBattleId battle,
        long completionCueSequence)
    {
        Battle = battle ?? throw new ArgumentNullException(nameof(battle));
        ArgumentOutOfRangeException.ThrowIfLessThan(completionCueSequence, 1);
        CompletionCueSequence = completionCueSequence;
    }

    public TacticalBattleId Battle { get; }

    public long CompletionCueSequence { get; }
}

public sealed record PublicSyntheticBattleCompletionReceipt
{
    public PublicSyntheticBattleCompletionReceipt(
        TacticalBattleId battle,
        TacticalCombatantId actor,
        TacticalCombatantId defeatedEnemy,
        long completedAtStep,
        long cueSequence)
    {
        Battle = battle ?? throw new ArgumentNullException(nameof(battle));
        Actor = actor ?? throw new ArgumentNullException(nameof(actor));
        DefeatedEnemy = defeatedEnemy ?? throw new ArgumentNullException(nameof(defeatedEnemy));
        ArgumentOutOfRangeException.ThrowIfLessThan(completedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        CompletedAtStep = completedAtStep;
        CueSequence = cueSequence;
    }

    public TacticalBattleId Battle { get; }

    public TacticalCombatantId Actor { get; }

    public TacticalCombatantId DefeatedEnemy { get; }

    public long CompletedAtStep { get; }

    public long CueSequence { get; }
}

public sealed record GameSessionPublicSyntheticBattleRequested(
    GameSessionSnapshot Snapshot,
    PublicSyntheticBattleLifecycleSnapshot Battle,
    PublicSyntheticBattleCue Cue) : GameSessionCommandResult;

public sealed record GameSessionPublicSyntheticBattleAdmitted(
    GameSessionSnapshot Snapshot,
    PublicSyntheticBattleLifecycleSnapshot Battle,
    PublicSyntheticBattleCue Cue) : GameSessionCommandResult;

public sealed record GameSessionPublicSyntheticBattleCursorMoved(
    GameSessionSnapshot Snapshot,
    TacticalCursorMoveOutcome Outcome) : GameSessionCommandResult;

public sealed record GameSessionPublicSyntheticBattleSelectionConfirmed :
    GameSessionCommandResult
{
    public GameSessionPublicSyntheticBattleSelectionConfirmed(
        GameSessionSnapshot snapshot,
        TacticalSelectionOutcome outcome,
        IEnumerable<PublicSyntheticBattleCue> cues,
        PublicSyntheticBattleCompletionReceipt? completion)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        ArgumentNullException.ThrowIfNull(cues);
        List<PublicSyntheticBattleCue> copiedCues = [];
        foreach (PublicSyntheticBattleCue cue in cues)
        {
            copiedCues.Add(cue ?? throw new ArgumentException(
                "Public-synthetic battle cues cannot contain null values.",
                nameof(cues)));
        }

        bool completed = outcome == TacticalSelectionOutcome.BattleCompleted;
        if (completed != (completion is not null) ||
            (completed && copiedCues.Count != 2) ||
            (!completed && copiedCues.Count != 1))
        {
            throw new ArgumentException(
                "Public-synthetic battle selection outcome, cues, and completion must agree.",
                nameof(completion));
        }

        Outcome = outcome;
        Cues = copiedCues.AsReadOnly();
        Completion = completion;
    }

    public GameSessionSnapshot Snapshot { get; }

    public TacticalSelectionOutcome Outcome { get; }

    public IReadOnlyList<PublicSyntheticBattleCue> Cues { get; }

    public PublicSyntheticBattleCompletionReceipt? Completion { get; }
}

public sealed record GameSessionPublicSyntheticBattleSelectionCancelled(
    GameSessionSnapshot Snapshot,
    TacticalCancelOutcome Outcome) : GameSessionCommandResult;

public sealed record GameSessionPublicSyntheticBattleReturned(
    GameSessionSnapshot Snapshot,
    PublicSyntheticBattleCompletionReceipt Completion,
    PublicSyntheticBattleCue Cue) : GameSessionCommandResult;

public sealed partial class GameSession
{
    private GameSessionCommandResult ApplyPublicSyntheticBattleRequest()
    {
        if (Snapshot.FlowStage != GameFlowStage.Exploration)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.WrongFlowStage,
                "A public-synthetic battle can be requested only during exploration.");
        }

        ExplorationContextSelectionSnapshot? selection = Snapshot.ContextSelection;
        if (selection is null ||
            selection.Map != Snapshot.Exploration.Map ||
            selection.Position != Snapshot.Exploration.PlayerPosition)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.ContextSelectionRequired,
                "An exact live exploration context must be selected before battle entry.");
        }

        PublicSyntheticBattleDefinition? definition =
            _mapContext.PublicSyntheticBattles.FindByTarget(selection.ZoneEvent.Target);
        if (definition is null ||
            definition.SourceMap != Snapshot.Exploration.Map ||
            definition.SourcePosition != Snapshot.Exploration.PlayerPosition ||
            definition.SourceSetup != selection.SelectedSetup ||
            definition.SourceZoneTarget != selection.ZoneEvent.Target)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.PublicSyntheticBattleNotAdmitted,
                "The selected exploration context does not admit a public-synthetic battle.");
        }

        long requestedAtStep = checked(Snapshot.SimulationStep + 1);
        long cueSequence = checked(Snapshot.LastCueSequence + 1);
        PublicSyntheticBattleLifecycleSnapshot battle =
            PublicSyntheticBattleLifecycleSnapshot.Pending(
                definition,
                requestedAtStep,
                cueSequence);
        Snapshot = BattleSnapshot(
            Snapshot.FlowStage,
            requestedAtStep,
            Snapshot.Exploration,
            Snapshot.ContextSelection,
            cueSequence,
            battle,
            Snapshot.Facing,
            Snapshot.Entities,
            clearExplorationLifecycles: false);
        PublicSyntheticBattleCue cue = new(
            definition.RequestCue,
            PublicSyntheticBattleCueKind.EntryRequested,
            definition.Request,
            definition.Rules.Battle,
            cueSequence);
        return new GameSessionPublicSyntheticBattleRequested(Snapshot, battle, cue);
    }

    private GameSessionCommandResult ApplyPublicSyntheticBattleEntryAcknowledgement(
        AcknowledgePublicSyntheticBattleEntryCommand command)
    {
        PublicSyntheticBattleLifecycleSnapshot? pending = Snapshot.PublicSyntheticBattle;
        if (pending?.Status != PublicSyntheticBattleLifecycleStatus.Pending)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.NoPendingAcknowledgement,
                "No public-synthetic battle entry is pending.");
        }

        PublicSyntheticBattleDefinition? definition =
            _mapContext.PublicSyntheticBattles.FindByRequest(command.Request);
        if (definition is null ||
            !ReferenceEquals(definition, pending.Definition) ||
            command.Battle != pending.Definition.Rules.Battle ||
            command.CueSequence != pending.EntryCueSequence)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.AcknowledgementMismatch,
                "The public-synthetic battle acknowledgement does not match the pending entry.");
        }

        long admittedAtStep = checked(Snapshot.SimulationStep + 1);
        long cueSequence = checked(Snapshot.LastCueSequence + 1);
        PublicSyntheticBattleLifecycleSnapshot admitted = pending.Admit(
            admittedAtStep,
            cueSequence);
        Snapshot = BattleSnapshot(
            GameFlowStage.Battle,
            admittedAtStep,
            Snapshot.Exploration,
            contextSelection: null,
            cueSequence,
            admitted,
            Snapshot.Facing,
            Snapshot.Entities,
            clearExplorationLifecycles: true);
        PublicSyntheticBattleCue cue = new(
            definition.AdmittedCue,
            PublicSyntheticBattleCueKind.BattleAdmitted,
            definition.Request,
            definition.Rules.Battle,
            cueSequence);
        return new GameSessionPublicSyntheticBattleAdmitted(Snapshot, admitted, cue);
    }

    private GameSessionCommandResult ApplyPublicSyntheticBattleCursorMove(
        MovePublicSyntheticBattleCursorCommand command)
    {
        PublicSyntheticBattleLifecycleSnapshot? lifecycle = Snapshot.PublicSyntheticBattle;
        if (Snapshot.FlowStage != GameFlowStage.Battle ||
            lifecycle?.Status != PublicSyntheticBattleLifecycleStatus.Active ||
            lifecycle.BattleState is null)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.PublicSyntheticBattleNotActive,
                "No active public-synthetic battle accepts cursor movement.");
        }

        TacticalCursorMoveResult moved = TacticalBattleReducer.MoveCursor(
            lifecycle.BattleState,
            command.Direction);
        if (moved.Outcome != TacticalCursorMoveOutcome.Moved)
        {
            return new GameSessionPublicSyntheticBattleCursorMoved(Snapshot, moved.Outcome);
        }

        long changedAtStep = checked(Snapshot.SimulationStep + 1);
        PublicSyntheticBattleLifecycleSnapshot updated = lifecycle.Update(
            moved.State,
            changedAtStep,
            Snapshot.LastCueSequence);
        Snapshot = BattleSnapshot(
            Snapshot.FlowStage,
            changedAtStep,
            Snapshot.Exploration,
            Snapshot.ContextSelection,
            Snapshot.LastCueSequence,
            updated,
            Snapshot.Facing,
            Snapshot.Entities,
            clearExplorationLifecycles: true);
        return new GameSessionPublicSyntheticBattleCursorMoved(Snapshot, moved.Outcome);
    }

    private GameSessionCommandResult ApplyPublicSyntheticBattleSelectionConfirmation()
    {
        PublicSyntheticBattleLifecycleSnapshot? lifecycle = Snapshot.PublicSyntheticBattle;
        if (Snapshot.FlowStage != GameFlowStage.Battle ||
            lifecycle?.Status != PublicSyntheticBattleLifecycleStatus.Active ||
            lifecycle.BattleState is null)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.PublicSyntheticBattleNotActive,
                "No active public-synthetic battle accepts selection confirmation.");
        }

        TacticalSelectionResult confirmed = TacticalBattleReducer.Confirm(
            lifecycle.BattleState);
        if (confirmed.Outcome == TacticalSelectionOutcome.InvalidSelection)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.PublicSyntheticBattleInvalidSelection,
                "The current public-synthetic tactical selection is not legal.");
        }

        long changedAtStep = checked(Snapshot.SimulationStep + 1);
        long firstCueSequence = checked(Snapshot.LastCueSequence + 1);
        List<PublicSyntheticBattleCue> cues = [];
        PublicSyntheticBattleCompletionReceipt? completion = null;
        long lastCueSequence;
        if (confirmed.Outcome == TacticalSelectionOutcome.MoveConfirmed)
        {
            lastCueSequence = firstCueSequence;
            cues.Add(new PublicSyntheticBattleCue(
                lifecycle.Definition.MoveCue,
                PublicSyntheticBattleCueKind.MoveConfirmed,
                lifecycle.Definition.Request,
                lifecycle.Definition.Rules.Battle,
                firstCueSequence));
        }
        else if (confirmed.Outcome == TacticalSelectionOutcome.AttackConfirmed)
        {
            lastCueSequence = firstCueSequence;
            cues.Add(new PublicSyntheticBattleCue(
                lifecycle.Definition.AttackCue,
                PublicSyntheticBattleCueKind.AttackCompleted,
                lifecycle.Definition.Request,
                lifecycle.Definition.Rules.Battle,
                firstCueSequence));
        }
        else
        {
            long completedCueSequence = checked(firstCueSequence + 1);
            lastCueSequence = completedCueSequence;
            cues.Add(new PublicSyntheticBattleCue(
                lifecycle.Definition.AttackCue,
                PublicSyntheticBattleCueKind.AttackCompleted,
                lifecycle.Definition.Request,
                lifecycle.Definition.Rules.Battle,
                firstCueSequence));
            cues.Add(new PublicSyntheticBattleCue(
                lifecycle.Definition.CompletedCue,
                PublicSyntheticBattleCueKind.BattleCompleted,
                lifecycle.Definition.Request,
                lifecycle.Definition.Rules.Battle,
                completedCueSequence));
            completion = new PublicSyntheticBattleCompletionReceipt(
                lifecycle.Definition.Rules.Battle,
                lifecycle.Definition.Rules.Actor,
                lifecycle.Definition.Rules.Enemy,
                changedAtStep,
                completedCueSequence);
        }

        PublicSyntheticBattleLifecycleSnapshot updated = lifecycle.Update(
            confirmed.State,
            changedAtStep,
            lastCueSequence);
        Snapshot = BattleSnapshot(
            Snapshot.FlowStage,
            changedAtStep,
            Snapshot.Exploration,
            Snapshot.ContextSelection,
            lastCueSequence,
            updated,
            Snapshot.Facing,
            Snapshot.Entities,
            clearExplorationLifecycles: true);
        return new GameSessionPublicSyntheticBattleSelectionConfirmed(
            Snapshot,
            confirmed.Outcome,
            cues.AsReadOnly(),
            completion);
    }

    private GameSessionCommandResult ApplyPublicSyntheticBattleSelectionCancellation()
    {
        PublicSyntheticBattleLifecycleSnapshot? lifecycle = Snapshot.PublicSyntheticBattle;
        if (Snapshot.FlowStage != GameFlowStage.Battle ||
            lifecycle?.Status != PublicSyntheticBattleLifecycleStatus.Active ||
            lifecycle.BattleState is null)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.PublicSyntheticBattleNotActive,
                "No active public-synthetic battle accepts cancellation.");
        }

        TacticalCancelResult cancelled = TacticalBattleReducer.Cancel(lifecycle.BattleState);
        if (cancelled.Outcome != TacticalCancelOutcome.ReturnedToMoveSelection)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.PublicSyntheticBattleInvalidSelection,
                "The current public-synthetic tactical selection cannot be cancelled.");
        }

        long changedAtStep = checked(Snapshot.SimulationStep + 1);
        PublicSyntheticBattleLifecycleSnapshot updated = lifecycle.Update(
            cancelled.State,
            changedAtStep,
            Snapshot.LastCueSequence);
        Snapshot = BattleSnapshot(
            Snapshot.FlowStage,
            changedAtStep,
            Snapshot.Exploration,
            Snapshot.ContextSelection,
            Snapshot.LastCueSequence,
            updated,
            Snapshot.Facing,
            Snapshot.Entities,
            clearExplorationLifecycles: true);
        return new GameSessionPublicSyntheticBattleSelectionCancelled(
            Snapshot,
            cancelled.Outcome);
    }

    private GameSessionCommandResult ApplyPublicSyntheticBattleCompletionAcknowledgement(
        AcknowledgePublicSyntheticBattleCompletionCommand command)
    {
        PublicSyntheticBattleLifecycleSnapshot? lifecycle = Snapshot.PublicSyntheticBattle;
        if (Snapshot.FlowStage != GameFlowStage.Battle ||
            lifecycle?.Status != PublicSyntheticBattleLifecycleStatus.Completed ||
            lifecycle.BattleState is null ||
            lifecycle.CompletedAtStep is not long completedAtStep)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.PublicSyntheticBattleNotCompleted,
                "No completed public-synthetic battle is ready to return.");
        }

        if (command.Battle != lifecycle.Definition.Rules.Battle ||
            command.CompletionCueSequence != lifecycle.LastCueSequence)
        {
            return RejectBattle(
                GameSessionCommandFailureCode.AcknowledgementMismatch,
                "The public-synthetic battle completion acknowledgement does not match.");
        }

        PublicSyntheticBattleCompletionReceipt completion = new(
            lifecycle.Definition.Rules.Battle,
            lifecycle.Definition.Rules.Actor,
            lifecycle.Definition.Rules.Enemy,
            completedAtStep,
            lifecycle.LastCueSequence);
        ExplorationMovementState returnedExploration = _mapContext.MapRuntimes
            .GetRequired(lifecycle.Definition.ReturnMap)
            .CreateExplorationState(lifecycle.Definition.ReturnPosition);
        long returnedAtStep = checked(Snapshot.SimulationStep + 1);
        long cueSequence = checked(Snapshot.LastCueSequence + 1);
        Snapshot = BattleSnapshot(
            GameFlowStage.Exploration,
            returnedAtStep,
            returnedExploration,
            contextSelection: null,
            cueSequence,
            publicSyntheticBattle: null,
            lifecycle.Definition.ReturnFacing,
            _mapContext.EntityInteractions.Entities.Where(
                entity => entity.Map == lifecycle.Definition.ReturnMap),
            clearExplorationLifecycles: true);
        PublicSyntheticBattleCue cue = new(
            lifecycle.Definition.ReturnedCue,
            PublicSyntheticBattleCueKind.ReturnedToExploration,
            lifecycle.Definition.Request,
            lifecycle.Definition.Rules.Battle,
            cueSequence);
        return new GameSessionPublicSyntheticBattleReturned(Snapshot, completion, cue);
    }

    private GameSessionSnapshot BattleSnapshot(
        GameFlowStage flowStage,
        long simulationStep,
        ExplorationMovementState exploration,
        ExplorationContextSelectionSnapshot? contextSelection,
        long lastCueSequence,
        PublicSyntheticBattleLifecycleSnapshot? publicSyntheticBattle,
        SemanticFacing facing,
        IEnumerable<MapEntityDefinition> entities,
        bool clearExplorationLifecycles) =>
        new(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            flowStage,
            simulationStep,
            exploration,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            Snapshot.Discoveries,
            Snapshot.Inventory,
            contextSelection,
            lastCueSequence,
            clearExplorationLifecycles ? null : Snapshot.EventRequest,
            clearExplorationLifecycles ? null : Snapshot.LastEventEffect,
            clearExplorationLifecycles ? null : Snapshot.LocalTransition,
            facing,
            entities,
            clearExplorationLifecycles ? null : Snapshot.EntityInteraction,
            clearExplorationLifecycles ? null : Snapshot.Dialogue,
            clearExplorationLifecycles ? null : Snapshot.FieldSearch,
            clearExplorationLifecycles ? null : Snapshot.ItemAcquisition,
            clearExplorationLifecycles ? null : Snapshot.OutboundTransition,
            publicSyntheticBattle);

    private GameSessionCommandRejected RejectBattle(
        GameSessionCommandFailureCode code,
        string message) =>
        new(Snapshot, new GameSessionCommandDiagnostic(code, message));
}

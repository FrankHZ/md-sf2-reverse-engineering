using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum GameFlowStage
{
    Exploration,
}

public sealed record GameSessionSnapshot
{
    public GameSessionSnapshot(
        string scenarioId,
        ContentProfile profile,
        GameFlowStage flowStage,
        long simulationStep,
        ExplorationMovementState exploration,
        ScenarioAdmissionFacts admissionFacts,
        PublicSyntheticFlagStateSnapshot syntheticFlags,
        ExplorationContextSelectionSnapshot? contextSelection,
        long lastCueSequence,
        MapEventRequestSnapshot? eventRequest,
        MapEventEffectSnapshot? lastEventEffect,
        MapLocalTransitionSnapshot? localTransition,
        SemanticFacing facing,
        IEnumerable<MapEntityDefinition> entities,
        MapEntityInteractionSnapshot? entityInteraction,
        MapDialogueSnapshot? dialogue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioId);
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        if (!Enum.IsDefined(flowStage))
        {
            throw new ArgumentOutOfRangeException(nameof(flowStage));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(simulationStep);
        ArgumentOutOfRangeException.ThrowIfNegative(lastCueSequence);
        if (eventRequest is not null && eventRequest.CueSequence > lastCueSequence)
        {
            throw new ArgumentException(
                "The event-request cue sequence cannot exceed the session cue sequence.",
                nameof(eventRequest));
        }

        if (localTransition is not null && localTransition.CueSequence > lastCueSequence)
        {
            throw new ArgumentException(
                "The local-transition cue sequence cannot exceed the session cue sequence.",
                nameof(localTransition));
        }

        if (!Enum.IsDefined(facing))
        {
            throw new ArgumentOutOfRangeException(nameof(facing));
        }

        if (entityInteraction is not null &&
            entityInteraction.CueSequence > lastCueSequence)
        {
            throw new ArgumentException(
                "The entity-interaction cue sequence cannot exceed the session cue sequence.",
                nameof(entityInteraction));
        }

        if (dialogue is not null && dialogue.CueSequence > lastCueSequence)
        {
            throw new ArgumentException(
                "The dialogue cue sequence cannot exceed the session cue sequence.",
                nameof(dialogue));
        }

        if (dialogue is not null &&
            (entityInteraction?.Status != MapEntityInteractionStatus.Acknowledged ||
             entityInteraction.Target != dialogue.TriggerTarget ||
             entityInteraction.AcknowledgedAtStep != dialogue.OpenedAtStep))
        {
            throw new ArgumentException(
                "Dialogue state requires the exact acknowledged entity interaction that opened it.",
                nameof(dialogue));
        }

        int pendingAcknowledgements =
            (eventRequest?.Status == MapEventRequestStatus.Pending ? 1 : 0) +
            (localTransition?.Status == MapLocalTransitionStatus.Pending ? 1 : 0) +
            (entityInteraction?.Status == MapEntityInteractionStatus.Pending ? 1 : 0);
        if (pendingAcknowledgements > 1)
        {
            throw new ArgumentException(
                "Only one acknowledgement-requiring request can be pending.",
                nameof(entityInteraction));
        }

        SyntheticFlags = syntheticFlags ?? throw new ArgumentNullException(nameof(syntheticFlags));
        if (lastEventEffect is not null &&
            (lastEventEffect.CueSequence > lastCueSequence ||
             !SyntheticFlags.IsSet(lastEventEffect.Flag)))
        {
            throw new ArgumentException(
                "The last event effect must reference an applied flag and admitted cue sequence.",
                nameof(lastEventEffect));
        }

        ScenarioId = scenarioId;
        Profile = profile;
        FlowStage = flowStage;
        SimulationStep = simulationStep;
        Exploration = exploration ?? throw new ArgumentNullException(nameof(exploration));
        AdmissionFacts = admissionFacts ?? throw new ArgumentNullException(nameof(admissionFacts));
        ContextSelection = contextSelection;
        LastCueSequence = lastCueSequence;
        EventRequest = eventRequest;
        LastEventEffect = lastEventEffect;
        LocalTransition = localTransition;
        Facing = facing;
        ArgumentNullException.ThrowIfNull(entities);
        List<MapEntityDefinition> copiedEntities = [];
        foreach (MapEntityDefinition entity in entities)
        {
            copiedEntities.Add(entity ?? throw new ArgumentException(
                "Snapshot entities cannot contain null values.",
                nameof(entities)));
        }

        Entities = new ReadOnlyCollection<MapEntityDefinition>(copiedEntities);
        EntityInteraction = entityInteraction;
        Dialogue = dialogue;
    }

    public string ScenarioId { get; }

    public ContentProfile Profile { get; }

    public GameFlowStage FlowStage { get; }

    public long SimulationStep { get; }

    public ExplorationMovementState Exploration { get; }

    public ScenarioAdmissionFacts AdmissionFacts { get; }

    public PublicSyntheticFlagStateSnapshot SyntheticFlags { get; }

    public ExplorationContextSelectionSnapshot? ContextSelection { get; }

    public long LastCueSequence { get; }

    public MapEventRequestSnapshot? EventRequest { get; }

    public MapEventEffectSnapshot? LastEventEffect { get; }

    public MapLocalTransitionSnapshot? LocalTransition { get; }

    public SemanticFacing Facing { get; }

    public IReadOnlyList<MapEntityDefinition> Entities { get; }

    public MapEntityInteractionSnapshot? EntityInteraction { get; }

    public MapDialogueSnapshot? Dialogue { get; }
}

public interface IGameSessionCommand;

public sealed record MoveExplorationCommand : IGameSessionCommand
{
    public MoveExplorationCommand(ExplorationDirection direction)
    {
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        Direction = direction;
    }

    public ExplorationDirection Direction { get; }
}

public enum GameSessionCommandFailureCode
{
    UnsupportedCommand,
    WrongFlowStage,
    ContextSelectionRequired,
    EventRequestNotAdmitted,
    PendingAcknowledgement,
    NoPendingAcknowledgement,
    AcknowledgementMismatch,
    EventEffectNotAdmitted,
    EventEffectAlreadyApplied,
    LocalTransitionNotAdmitted,
    EntityInteractionNotAdmitted,
    DialogueNotAdmitted,
    DialogueNotOpen,
    DialogueIdentityMismatch,
}

public sealed record GameSessionCommandDiagnostic
{
    public GameSessionCommandDiagnostic(GameSessionCommandFailureCode code, string message)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Message = message;
    }

    public GameSessionCommandFailureCode Code { get; }

    public string Message { get; }
}

public abstract record GameSessionCommandResult;

public sealed record GameSessionCommandApplied(
    GameSessionSnapshot Snapshot,
    ExplorationMovementOutcome Outcome) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));
}

public sealed record GameSessionCommandRejected(
    GameSessionSnapshot Snapshot,
    GameSessionCommandDiagnostic Diagnostic) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public GameSessionCommandDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public abstract record GameSessionStartResult;

public sealed record GameSessionStarted(
    GameSession Session,
    string DisplayName,
    ScenarioAdmissionReceipt Receipt) : GameSessionStartResult
{
    public GameSession Session { get; } =
        Session ?? throw new ArgumentNullException(nameof(Session));

    public string DisplayName { get; } =
        string.IsNullOrWhiteSpace(DisplayName)
            ? throw new ArgumentException(
                "A started session requires a display name.",
                nameof(DisplayName))
            : DisplayName;

    public ScenarioAdmissionReceipt Receipt { get; } =
        Receipt ?? throw new ArgumentNullException(nameof(Receipt));
}

public sealed record GameSessionStartRejected(
    ScenarioAdmissionDiagnostic Diagnostic) : GameSessionStartResult
{
    public ScenarioAdmissionDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public sealed class GameSession
{
    private readonly MapScenarioContextDefinition _mapContext;

    private GameSession(
        GameSessionSnapshot snapshot,
        MapScenarioContextDefinition mapContext)
    {
        Snapshot = snapshot;
        _mapContext = mapContext;
    }

    public GameSessionSnapshot Snapshot { get; private set; }

    public static GameSessionStartResult Start(
        IMapScenarioSource source,
        MapScenarioRequest request)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(request);

        return source.Admit(request) switch
        {
            MapScenarioAccepted accepted => StartAccepted(accepted),
            MapScenarioRejected rejected => new GameSessionStartRejected(rejected.Diagnostic),
            _ => throw new InvalidOperationException("Scenario source returned an unknown result."),
        };
    }

    public GameSessionCommandResult Apply(IGameSessionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (Snapshot.EventRequest?.Status == MapEventRequestStatus.Pending &&
            command is not AcknowledgeMapEventRequestCommand)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.PendingAcknowledgement,
                    "The pending map-event request must be acknowledged first."));
        }

        if (Snapshot.LocalTransition?.Status == MapLocalTransitionStatus.Pending &&
            command is not AcknowledgeMapLocalTransitionCommand)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.PendingAcknowledgement,
                    "The pending local-transition request must be acknowledged first."));
        }

        if (Snapshot.EntityInteraction?.Status == MapEntityInteractionStatus.Pending &&
            command is not AcknowledgeEntityInteractionCommand)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.PendingAcknowledgement,
                    "The pending entity interaction must be acknowledged first."));
        }

        if (Snapshot.Dialogue?.Status == MapDialogueStatus.Open &&
            command is not AdvanceDialogueCommand)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.PendingAcknowledgement,
                    "The open dialogue line must be advanced before another session command."));
        }

        return command switch
        {
            MoveExplorationCommand move => ApplyMove(move),
            TurnExplorationCommand turn => ApplyTurn(turn),
            SelectExplorationContextCommand selectContext => ApplyContextSelection(selectContext),
            RequestSelectedZoneEventCommand => ApplyEventRequest(),
            AcknowledgeMapEventRequestCommand acknowledge =>
                ApplyEventRequestAcknowledgement(acknowledge),
            RequestSelectedLocalTransitionCommand => ApplyLocalTransitionRequest(),
            AcknowledgeMapLocalTransitionCommand acknowledgeTransition =>
                ApplyLocalTransitionAcknowledgement(acknowledgeTransition),
            RequestEntityInteractionCommand => ApplyEntityInteractionRequest(),
            AcknowledgeEntityInteractionCommand acknowledgeInteraction =>
                ApplyEntityInteractionAcknowledgement(acknowledgeInteraction),
            AdvanceDialogueCommand advanceDialogue => ApplyDialogueAdvance(advanceDialogue),
            _ => new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.UnsupportedCommand,
                    $"Unsupported session command '{command.GetType().Name}'.")),
        };
    }

    private GameSessionCommandResult ApplyMove(MoveExplorationCommand command)
    {
        if (Snapshot.FlowStage != GameFlowStage.Exploration)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.WrongFlowStage,
                    "Exploration input is not admitted in this flow stage."));
        }

        ExplorationMovementResult transition = ExplorationMovementReducer.TryMove(
            Snapshot.Exploration,
            new ExplorationMovementCommand(command.Direction));
        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            checked(Snapshot.SimulationStep + 1),
            transition.State,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            contextSelection: null,
            Snapshot.LastCueSequence,
            Snapshot.EventRequest,
            Snapshot.LastEventEffect,
            Snapshot.LocalTransition,
            ToSemanticFacing(command.Direction),
            Snapshot.Entities,
            entityInteraction: null,
            dialogue: null);
        return new GameSessionCommandApplied(Snapshot, transition.Outcome);
    }

    private GameSessionCommandResult ApplyTurn(TurnExplorationCommand command)
    {
        if (Snapshot.FlowStage != GameFlowStage.Exploration)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.WrongFlowStage,
                    "Exploration facing input is not admitted in this flow stage."));
        }

        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            checked(Snapshot.SimulationStep + 1),
            Snapshot.Exploration,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            contextSelection: null,
            Snapshot.LastCueSequence,
            Snapshot.EventRequest,
            Snapshot.LastEventEffect,
            Snapshot.LocalTransition,
            command.Facing,
            Snapshot.Entities,
            entityInteraction: null,
            dialogue: null);
        return new GameSessionFacingChanged(Snapshot, command.Facing);
    }

    private GameSessionCommandResult ApplyContextSelection(
        SelectExplorationContextCommand command)
    {
        if (Snapshot.FlowStage != GameFlowStage.Exploration)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.WrongFlowStage,
                    "Map context selection is not admitted in this flow stage."));
        }

        MapPosition position = Snapshot.Exploration.PlayerPosition;
        byte x = checked((byte)position.X);
        byte y = checked((byte)position.Y);
        MapSetupId selectedSetup = _mapContext.SetupCatalog.Select(
            Snapshot.Exploration.Map,
            _mapContext.VoidSetup,
            Snapshot.SyntheticFlags.IsSet);
        AreaDescriptionSelection areaDescription = MapAreaDescriptionSelector.Select(
            _mapContext.AreaDescriptions,
            new MapAreaDescriptionQuery(x, y, command.AreaDescriptionAdmission));
        ZoneEventSelection zoneEvent = MapSetupEventSelector.Select(
            _mapContext.ZoneEvents,
            new ZoneEventQuery(x, y));
        ExplorationContextSelectionSnapshot selection = new(
            position,
            selectedSetup,
            areaDescription,
            zoneEvent);
        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            checked(Snapshot.SimulationStep + 1),
            Snapshot.Exploration,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            selection,
            Snapshot.LastCueSequence,
            Snapshot.EventRequest,
            Snapshot.LastEventEffect,
            Snapshot.LocalTransition,
            Snapshot.Facing,
            Snapshot.Entities,
            entityInteraction: null,
            dialogue: null);
        return new GameSessionContextSelected(Snapshot, selection);
    }

    private GameSessionCommandResult ApplyEventRequest()
    {
        if (Snapshot.FlowStage != GameFlowStage.Exploration)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.WrongFlowStage,
                    "Map-event requests are not admitted in this flow stage."));
        }

        ExplorationContextSelectionSnapshot? selection = Snapshot.ContextSelection;
        if (selection is null)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.ContextSelectionRequired,
                    "A current exploration-context selection is required."));
        }

        MapEventRequestDefinition? definition = _mapContext.EventRequests.FindByTarget(
            selection.ZoneEvent.Target);
        if (definition is null)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.EventRequestNotAdmitted,
                    $"Zone target '{selection.ZoneEvent.Target}' has no admitted event request."));
        }

        MapEventEffectDefinition? effect = _mapContext.EventEffects.FindByRequest(
            definition.Request);
        if (effect is null)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.EventEffectNotAdmitted,
                    $"Event request '{definition.Request}' has no admitted synthetic effect."));
        }

        if (Snapshot.SyntheticFlags.IsSet(effect.Flag))
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.EventEffectAlreadyApplied,
                    $"Event effect '{effect.Effect}' has already been applied."));
        }

        long requestedAtStep = checked(Snapshot.SimulationStep + 1);
        long cueSequence = checked(Snapshot.LastCueSequence + 1);
        MapEventRequestSnapshot request = MapEventRequestSnapshot.Pending(
            definition,
            effect,
            selection.Position,
            requestedAtStep,
            cueSequence);
        MapEventRequestCue cue = new(
            definition.Cue,
            definition.Request,
            definition.ZoneTarget,
            selection.Position,
            cueSequence);
        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            requestedAtStep,
            Snapshot.Exploration,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            Snapshot.ContextSelection,
            cueSequence,
            request,
            Snapshot.LastEventEffect,
            Snapshot.LocalTransition,
            Snapshot.Facing,
            Snapshot.Entities,
            Snapshot.EntityInteraction,
            Snapshot.Dialogue);
        return new GameSessionEventRequested(Snapshot, request, cue);
    }

    private GameSessionCommandResult ApplyEventRequestAcknowledgement(
        AcknowledgeMapEventRequestCommand command)
    {
        MapEventRequestSnapshot? pending = Snapshot.EventRequest;
        if (pending?.Status != MapEventRequestStatus.Pending)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.NoPendingAcknowledgement,
                    "There is no pending map-event request to acknowledge."));
        }

        if (pending.Request != command.Request ||
            pending.CueSequence != command.CueSequence ||
            pending.ExpectedEffect != command.Effect)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.AcknowledgementMismatch,
                    "The acknowledgement does not match the pending request, cue sequence, and effect."));
        }

        MapEventEffectDefinition? definition = _mapContext.EventEffects.FindByRequest(
            pending.Request);
        if (definition is null || definition.Effect != pending.ExpectedEffect)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.EventEffectNotAdmitted,
                    "The pending request has no exact admitted synthetic effect."));
        }

        if (Snapshot.SyntheticFlags.IsSet(definition.Flag))
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.EventEffectAlreadyApplied,
                    $"Event effect '{definition.Effect}' has already been applied."));
        }

        long acknowledgedAtStep = checked(Snapshot.SimulationStep + 1);
        MapEventRequestSnapshot acknowledged = pending.Acknowledge(acknowledgedAtStep);
        long effectCueSequence = checked(Snapshot.LastCueSequence + 1);
        PublicSyntheticFlagStateSnapshot syntheticFlags =
            Snapshot.SyntheticFlags.SetOnce(definition.Flag);
        MapEventEffectSnapshot effect = new(
            definition,
            pending.CueSequence,
            acknowledgedAtStep,
            effectCueSequence);
        MapEventEffectCue cue = new(
            definition.Cue,
            definition.Effect,
            definition.Flag,
            effectCueSequence);
        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            acknowledgedAtStep,
            Snapshot.Exploration,
            Snapshot.AdmissionFacts,
            syntheticFlags,
            contextSelection: null,
            effectCueSequence,
            acknowledged,
            effect,
            Snapshot.LocalTransition,
            Snapshot.Facing,
            Snapshot.Entities,
            Snapshot.EntityInteraction,
            Snapshot.Dialogue);
        return new GameSessionEventEffectApplied(
            Snapshot,
            acknowledged,
            effect,
            cue);
    }

    private GameSessionCommandResult ApplyLocalTransitionRequest()
    {
        if (Snapshot.FlowStage != GameFlowStage.Exploration)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.WrongFlowStage,
                    "Local transitions are not admitted in this flow stage."));
        }

        ExplorationContextSelectionSnapshot? selection = Snapshot.ContextSelection;
        if (selection is null ||
            selection.Position != Snapshot.Exploration.PlayerPosition)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.ContextSelectionRequired,
                    "A current exploration-context selection is required."));
        }

        MapLocalTransitionDefinition? definition = _mapContext.LocalTransitions.FindByTarget(
            selection.ZoneEvent.Target);
        if (definition is null ||
            definition.SourceMap != Snapshot.Exploration.Map ||
            definition.SourcePosition != Snapshot.Exploration.PlayerPosition ||
            definition.SourceSetup != selection.SelectedSetup)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.LocalTransitionNotAdmitted,
                    $"Zone target '{selection.ZoneEvent.Target}' has no exact admitted local transition."));
        }

        long requestedAtStep = checked(Snapshot.SimulationStep + 1);
        long cueSequence = checked(Snapshot.LastCueSequence + 1);
        MapLocalTransitionSnapshot transition = MapLocalTransitionSnapshot.Pending(
            definition,
            requestedAtStep,
            cueSequence);
        MapLocalTransitionCue cue = new(
            definition.Cue,
            definition.Request,
            definition.Transition,
            definition.ZoneTarget,
            definition.SourcePosition,
            cueSequence);
        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            requestedAtStep,
            Snapshot.Exploration,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            Snapshot.ContextSelection,
            cueSequence,
            Snapshot.EventRequest,
            Snapshot.LastEventEffect,
            transition,
            Snapshot.Facing,
            Snapshot.Entities,
            Snapshot.EntityInteraction,
            Snapshot.Dialogue);
        return new GameSessionLocalTransitionRequested(Snapshot, transition, cue);
    }

    private GameSessionCommandResult ApplyLocalTransitionAcknowledgement(
        AcknowledgeMapLocalTransitionCommand command)
    {
        MapLocalTransitionSnapshot? pending = Snapshot.LocalTransition;
        if (pending?.Status != MapLocalTransitionStatus.Pending)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.NoPendingAcknowledgement,
                    "There is no pending local transition to acknowledge."));
        }

        if (pending.Request != command.Request ||
            pending.CueSequence != command.CueSequence ||
            pending.Transition != command.Transition)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.AcknowledgementMismatch,
                    "The acknowledgement does not match the pending request, cue sequence, and transition."));
        }

        MapLocalTransitionDefinition? definition = _mapContext.LocalTransitions.FindByRequest(
            pending.Request);
        if (definition is null ||
            definition.Transition != pending.Transition ||
            definition.SourceMap != Snapshot.Exploration.Map ||
            definition.SourcePosition != Snapshot.Exploration.PlayerPosition ||
            definition.SourceSetup != Snapshot.ContextSelection?.SelectedSetup ||
            definition.DestinationMap != pending.DestinationMap ||
            definition.DestinationPosition != pending.DestinationPosition ||
            definition.DestinationOrientation != pending.DestinationOrientation)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.LocalTransitionNotAdmitted,
                    "The pending request has no exact admitted local transition."));
        }

        long acknowledgedAtStep = checked(Snapshot.SimulationStep + 1);
        ExplorationMovementState relocated = new(
            definition.DestinationMap,
            Snapshot.Exploration.Layout,
            Snapshot.Exploration.Walkability,
            definition.DestinationPosition);
        MapLocalTransitionSnapshot acknowledged = pending.Acknowledge(acknowledgedAtStep);
        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            acknowledgedAtStep,
            relocated,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            contextSelection: null,
            Snapshot.LastCueSequence,
            eventRequest: null,
            lastEventEffect: null,
            acknowledged,
            Snapshot.Facing,
            Snapshot.Entities,
            entityInteraction: null,
            dialogue: null);
        return new GameSessionLocalTransitionApplied(Snapshot, acknowledged);
    }

    private GameSessionCommandResult ApplyEntityInteractionRequest()
    {
        if (Snapshot.FlowStage != GameFlowStage.Exploration)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.WrongFlowStage,
                    "Entity interactions are not admitted in this flow stage."));
        }

        MapPosition? targetPosition = PositionAhead(
            Snapshot.Exploration,
            Snapshot.Facing);
        MapEntityDefinition? entity = targetPosition is null
            ? null
            : _mapContext.EntityInteractions.FindEntityAt(
                Snapshot.Exploration.Map,
                targetPosition);
        MapEntityInteractionDefinition? definition = entity is null
            ? null
            : _mapContext.EntityInteractions.FindByTarget(entity.InteractionTarget);
        if (targetPosition is null || entity is null || definition is null)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.EntityInteractionNotAdmitted,
                    "There is no admitted synthetic entity interaction one tile ahead."));
        }

        long requestedAtStep = checked(Snapshot.SimulationStep + 1);
        long cueSequence = checked(Snapshot.LastCueSequence + 1);
        MapEntityInteractionSnapshot interaction = MapEntityInteractionSnapshot.Pending(
            entity,
            definition,
            Snapshot.Exploration.PlayerPosition,
            Snapshot.Facing,
            requestedAtStep,
            cueSequence);
        MapEntityInteractionCue cue = new(
            definition.Cue,
            definition.Request,
            entity.Entity,
            definition.Target,
            entity.Position,
            cueSequence);
        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            requestedAtStep,
            Snapshot.Exploration,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            contextSelection: null,
            cueSequence,
            Snapshot.EventRequest,
            Snapshot.LastEventEffect,
            Snapshot.LocalTransition,
            Snapshot.Facing,
            Snapshot.Entities,
            interaction,
            dialogue: null);
        return new GameSessionEntityInteractionRequested(Snapshot, interaction, cue);
    }

    private GameSessionCommandResult ApplyEntityInteractionAcknowledgement(
        AcknowledgeEntityInteractionCommand command)
    {
        MapEntityInteractionSnapshot? pending = Snapshot.EntityInteraction;
        if (pending?.Status != MapEntityInteractionStatus.Pending)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.NoPendingAcknowledgement,
                    "There is no pending entity interaction to acknowledge."));
        }

        if (pending.Request != command.Request ||
            pending.CueSequence != command.CueSequence ||
            pending.Entity != command.Entity ||
            pending.Target != command.Target)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.AcknowledgementMismatch,
                    "The acknowledgement does not match the pending request, cue sequence, entity, and target."));
        }

        MapEntityDefinition? entity = _mapContext.EntityInteractions.FindEntity(pending.Entity);
        MapEntityInteractionDefinition? definition =
            _mapContext.EntityInteractions.FindByRequest(pending.Request);
        MapPosition? targetPosition = PositionAhead(Snapshot.Exploration, Snapshot.Facing);
        if (entity is null ||
            definition is null ||
            entity.InteractionTarget != pending.Target ||
            definition.Target != pending.Target ||
            entity.Map != Snapshot.Exploration.Map ||
            entity.Position != pending.EntityPosition ||
            targetPosition != entity.Position ||
            Snapshot.Exploration.PlayerPosition != pending.PlayerPosition ||
            Snapshot.Facing != pending.Facing)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.EntityInteractionNotAdmitted,
                    "The pending request no longer has one exact admitted entity interaction."));
        }

        MapDialogueDefinition? dialogueDefinition =
            _mapContext.Dialogues.FindByTarget(pending.Target);
        if (dialogueDefinition is null)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.DialogueNotAdmitted,
                    "The pending interaction has no exact admitted synthetic dialogue."));
        }

        long acknowledgedAtStep = checked(Snapshot.SimulationStep + 1);
        MapEntityInteractionSnapshot acknowledged = pending.Acknowledge(acknowledgedAtStep);
        long dialogueCueSequence = checked(Snapshot.LastCueSequence + 1);
        MapDialogueSnapshot dialogue = MapDialogueSnapshot.Open(
            dialogueDefinition,
            acknowledgedAtStep,
            dialogueCueSequence);
        MapDialogueCue cue = MapDialogueCue.LinePresented(
            dialogueDefinition,
            dialogue.CurrentLine!,
            dialogueCueSequence);
        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            acknowledgedAtStep,
            Snapshot.Exploration,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            contextSelection: null,
            dialogueCueSequence,
            Snapshot.EventRequest,
            Snapshot.LastEventEffect,
            Snapshot.LocalTransition,
            Snapshot.Facing,
            Snapshot.Entities,
            acknowledged,
            dialogue);
        return new GameSessionEntityInteractionAcknowledged(
            Snapshot,
            acknowledged,
            dialogue,
            cue);
    }

    private GameSessionCommandResult ApplyDialogueAdvance(AdvanceDialogueCommand command)
    {
        MapDialogueSnapshot? current = Snapshot.Dialogue;
        if (current?.Status != MapDialogueStatus.Open || current.CurrentLine is null)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.DialogueNotOpen,
                    "There is no open synthetic dialogue line to advance."));
        }

        if (current.Dialogue != command.Dialogue ||
            current.CueSequence != command.CueSequence ||
            current.CurrentLine.Line != command.CurrentLine)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.DialogueIdentityMismatch,
                    "The command does not match the current dialogue, cue sequence, and line."));
        }

        MapDialogueDefinition? definition = _mapContext.Dialogues.FindByDialogue(current.Dialogue);
        if (definition is null || definition.InteractionTarget != current.TriggerTarget)
        {
            return new GameSessionCommandRejected(
                Snapshot,
                new GameSessionCommandDiagnostic(
                    GameSessionCommandFailureCode.DialogueNotAdmitted,
                    "The open dialogue no longer has one exact admitted definition."));
        }

        long advancedAtStep = checked(Snapshot.SimulationStep + 1);
        long cueSequence = checked(Snapshot.LastCueSequence + 1);
        MapDialogueSnapshot advanced = current.Advance(
            definition,
            advancedAtStep,
            cueSequence);
        MapDialogueCue cue = advanced.Status == MapDialogueStatus.Open
            ? MapDialogueCue.LinePresented(definition, advanced.CurrentLine!, cueSequence)
            : MapDialogueCue.Closed(definition, cueSequence);
        Snapshot = new GameSessionSnapshot(
            Snapshot.ScenarioId,
            Snapshot.Profile,
            Snapshot.FlowStage,
            advancedAtStep,
            Snapshot.Exploration,
            Snapshot.AdmissionFacts,
            Snapshot.SyntheticFlags,
            Snapshot.ContextSelection,
            cueSequence,
            Snapshot.EventRequest,
            Snapshot.LastEventEffect,
            Snapshot.LocalTransition,
            Snapshot.Facing,
            Snapshot.Entities,
            Snapshot.EntityInteraction,
            advanced);
        return advanced.Status == MapDialogueStatus.Open
            ? new GameSessionDialogueAdvanced(Snapshot, advanced, cue)
            : new GameSessionDialogueClosed(Snapshot, advanced, cue);
    }

    private static SemanticFacing ToSemanticFacing(ExplorationDirection direction) =>
        direction switch
        {
            ExplorationDirection.North => SemanticFacing.North,
            ExplorationDirection.East => SemanticFacing.East,
            ExplorationDirection.South => SemanticFacing.South,
            ExplorationDirection.West => SemanticFacing.West,
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

    private static MapPosition? PositionAhead(
        ExplorationMovementState exploration,
        SemanticFacing facing)
    {
        MapPosition position = exploration.PlayerPosition;
        (int deltaX, int deltaY) = facing switch
        {
            SemanticFacing.North => (0, -1),
            SemanticFacing.East => (1, 0),
            SemanticFacing.South => (0, 1),
            SemanticFacing.West => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(facing)),
        };
        int x = position.X + deltaX;
        int y = position.Y + deltaY;
        if (x < 0 || x >= exploration.Walkability.Width ||
            y < 0 || y >= exploration.Walkability.Height)
        {
            return null;
        }

        return new MapPosition(x, y);
    }

    private static GameSessionStarted StartAccepted(MapScenarioAccepted accepted)
    {
        GameSession session = new(
            new GameSessionSnapshot(
                accepted.Scenario.ScenarioId,
                accepted.Receipt.Profile,
                GameFlowStage.Exploration,
                simulationStep: 0,
                accepted.Scenario.StartState,
                accepted.Scenario.AdmissionFacts,
                new PublicSyntheticFlagStateSnapshot(
                    accepted.Scenario.MapContext.InitialSetFlags),
                contextSelection: null,
                lastCueSequence: 0,
                eventRequest: null,
                lastEventEffect: null,
                localTransition: null,
                accepted.Scenario.MapContext.InitialFacing,
                accepted.Scenario.MapContext.EntityInteractions.Entities,
                entityInteraction: null,
                dialogue: null),
            accepted.Scenario.MapContext);
        return new GameSessionStarted(
            session,
            accepted.Scenario.DisplayName,
            accepted.Receipt);
    }
}

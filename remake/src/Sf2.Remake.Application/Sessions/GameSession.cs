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
        MapEventEffectSnapshot? lastEventEffect)
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

        return command switch
        {
            MoveExplorationCommand move => ApplyMove(move),
            SelectExplorationContextCommand selectContext => ApplyContextSelection(selectContext),
            RequestSelectedZoneEventCommand => ApplyEventRequest(),
            AcknowledgeMapEventRequestCommand acknowledge =>
                ApplyEventRequestAcknowledgement(acknowledge),
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
            Snapshot.LastEventEffect);
        return new GameSessionCommandApplied(Snapshot, transition.Outcome);
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
            Snapshot.LastEventEffect);
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
            Snapshot.LastEventEffect);
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
            effect);
        return new GameSessionEventEffectApplied(
            Snapshot,
            acknowledged,
            effect,
            cue);
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
                lastEventEffect: null),
            accepted.Scenario.MapContext);
        return new GameSessionStarted(
            session,
            accepted.Scenario.DisplayName,
            accepted.Receipt);
    }
}

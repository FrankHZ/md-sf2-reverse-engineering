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
        ExplorationContextSelectionSnapshot? contextSelection)
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
        ScenarioId = scenarioId;
        Profile = profile;
        FlowStage = flowStage;
        SimulationStep = simulationStep;
        Exploration = exploration ?? throw new ArgumentNullException(nameof(exploration));
        AdmissionFacts = admissionFacts ?? throw new ArgumentNullException(nameof(admissionFacts));
        ContextSelection = contextSelection;
    }

    public string ScenarioId { get; }

    public ContentProfile Profile { get; }

    public GameFlowStage FlowStage { get; }

    public long SimulationStep { get; }

    public ExplorationMovementState Exploration { get; }

    public ScenarioAdmissionFacts AdmissionFacts { get; }

    public ExplorationContextSelectionSnapshot? ContextSelection { get; }
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
        return command switch
        {
            MoveExplorationCommand move => ApplyMove(move),
            SelectExplorationContextCommand selectContext => ApplyContextSelection(selectContext),
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
            contextSelection: null);
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
            _mapContext.IsFlagSet);
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
            selection);
        return new GameSessionContextSelected(Snapshot, selection);
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
                contextSelection: null),
            accepted.Scenario.MapContext);
        return new GameSessionStarted(
            session,
            accepted.Scenario.DisplayName,
            accepted.Receipt);
    }
}

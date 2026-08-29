using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class Map3Root : Node2D
{
    public const string BannerText = "PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY";
    public const string SmokeMarker = "SF2_MAP3_SMOKE ";

    private GameSession? _session;
    private ScenarioAdmissionReceipt? _admissionReceipt;
    private SyntheticMapViewport? _viewport;
    private Label? _status;
    private Label? _contextStatus;
    private Label? _eventRequestStatus;
    private Label? _effectStatus;
    private Label? _transitionStatus;
    private Label? _entityStatus;
    private Label? _entityInteractionStatus;

    public override void _Ready()
    {
        RegisterInputMap();
        BuildPresentation();
        StartScenario();
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (_session is null)
        {
            return;
        }

        if (Input.IsActionJustPressed("move_north"))
        {
            ApplyMove(ExplorationDirection.North);
        }
        else if (Input.IsActionJustPressed("move_east"))
        {
            ApplyMove(ExplorationDirection.East);
        }
        else if (Input.IsActionJustPressed("move_south"))
        {
            ApplyMove(ExplorationDirection.South);
        }
        else if (Input.IsActionJustPressed("move_west"))
        {
            ApplyMove(ExplorationDirection.West);
        }
        else if (Input.IsActionJustPressed("select_context"))
        {
            ApplyContextSelection();
        }
        else if (Input.IsActionJustPressed("request_event"))
        {
            ApplyEventRequest();
        }
        else if (Input.IsActionJustPressed("acknowledge_event"))
        {
            ApplyEventRequestAcknowledgement();
        }
        else if (Input.IsActionJustPressed("request_transition"))
        {
            ApplyLocalTransitionRequest();
        }
        else if (Input.IsActionJustPressed("acknowledge_transition"))
        {
            ApplyLocalTransitionAcknowledgement();
        }
        else if (Input.IsActionJustPressed("turn_north"))
        {
            ApplyTurn(SemanticFacing.North);
        }
        else if (Input.IsActionJustPressed("turn_east"))
        {
            ApplyTurn(SemanticFacing.East);
        }
        else if (Input.IsActionJustPressed("turn_south"))
        {
            ApplyTurn(SemanticFacing.South);
        }
        else if (Input.IsActionJustPressed("turn_west"))
        {
            ApplyTurn(SemanticFacing.West);
        }
        else if (Input.IsActionJustPressed("request_entity_interaction"))
        {
            ApplyEntityInteractionRequest();
        }
        else if (Input.IsActionJustPressed("acknowledge_entity_interaction"))
        {
            ApplyEntityInteractionAcknowledgement();
        }
    }

    private void StartScenario()
    {
        byte[] packageBytes = Godot.FileAccess.GetFileAsBytes(
            "res://content/public-synthetic-map3-smoke-v1.json");
        PublicSyntheticMap3PackageReader source =
            PublicSyntheticMap3PackageReader.FromDocumentBytes(packageBytes);
        GameSessionStartResult result = GameSession.Start(
            source,
            new MapScenarioRequest(
                PublicSyntheticMap3PackageReader.PackageId,
                ContentProfile.PublicSynthetic));
        if (result is not GameSessionStarted started)
        {
            GameSessionStartRejected rejected = (GameSessionStartRejected)result;
            FailStartup(rejected.Diagnostic.Message);
            return;
        }

        _session = started.Session;
        _admissionReceipt = started.Receipt;
        ProjectSnapshot("Ready");
        if (OS.GetCmdlineUserArgs().Contains("--map3-smoke", StringComparer.Ordinal))
        {
            Callable.From(RunHeadlessSmoke).CallDeferred();
        }
    }

    private void ApplyMove(ExplorationDirection direction)
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new MoveExplorationCommand(direction));
        if (result is GameSessionCommandApplied applied)
        {
            ProjectSnapshot(applied.Outcome.ToString());
            return;
        }

        GameSessionCommandRejected rejected = (GameSessionCommandRejected)result;
        if (_status is not null)
        {
            _status.Text = rejected.Diagnostic.Message;
        }
    }

    private void ApplyContextSelection()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary));
        if (result is GameSessionContextSelected selected)
        {
            ProjectSnapshot("Context selected");
            return;
        }

        GameSessionCommandRejected rejected = (GameSessionCommandRejected)result;
        if (_status is not null)
        {
            _status.Text = rejected.Diagnostic.Message;
        }
    }

    private void ApplyEventRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new RequestSelectedZoneEventCommand());
        if (result is GameSessionEventRequested)
        {
            ProjectSnapshot("Event request pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyEventRequestAcknowledgement()
    {
        if (_session?.Snapshot.EventRequest is not MapEventRequestSnapshot request)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapEventRequestCommand(
                request.Request,
                request.CueSequence,
                request.ExpectedEffect));
        if (result is GameSessionEventEffectApplied)
        {
            ProjectSnapshot("Synthetic effect applied; re-select context");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyLocalTransitionRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new RequestSelectedLocalTransitionCommand());
        if (result is GameSessionLocalTransitionRequested)
        {
            ProjectSnapshot("Local transition pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyLocalTransitionAcknowledgement()
    {
        if (_session?.Snapshot.LocalTransition is not MapLocalTransitionSnapshot transition)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapLocalTransitionCommand(
                transition.Request,
                transition.CueSequence,
                transition.Transition));
        if (result is GameSessionLocalTransitionApplied)
        {
            ProjectSnapshot("Synthetic local transition applied");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyTurn(SemanticFacing facing)
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new TurnExplorationCommand(facing));
        if (result is GameSessionFacingChanged)
        {
            ProjectSnapshot($"Facing {facing}");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyEntityInteractionRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new RequestEntityInteractionCommand());
        if (result is GameSessionEntityInteractionRequested)
        {
            ProjectSnapshot("Placeholder entity interaction pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyEntityInteractionAcknowledgement()
    {
        if (_session?.Snapshot.EntityInteraction is not MapEntityInteractionSnapshot interaction)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeEntityInteractionCommand(
                interaction.Request,
                interaction.CueSequence,
                interaction.Entity,
                interaction.Target));
        if (result is GameSessionEntityInteractionAcknowledged)
        {
            ProjectSnapshot("Placeholder entity interaction acknowledged");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ProjectRejection(GameSessionCommandRejected rejected)
    {
        if (_status is not null)
        {
            _status.Text = rejected.Diagnostic.Message;
        }
    }

    private void ProjectSnapshot(string outcome)
    {
        if (_session is null ||
            _viewport is null ||
            _status is null ||
            _contextStatus is null ||
            _eventRequestStatus is null ||
            _effectStatus is null ||
            _transitionStatus is null ||
            _entityStatus is null ||
            _entityInteractionStatus is null)
        {
            return;
        }

        GameSessionSnapshot snapshot = _session.Snapshot;
        _viewport.Project(snapshot);
        _status.Text =
            $"Map {snapshot.AdmissionFacts.CurrentMap}  " +
            $"Tile ({snapshot.Exploration.PlayerPosition.X}, " +
            $"{snapshot.Exploration.PlayerPosition.Y})  " +
            $"Facing {snapshot.Facing}  Step {snapshot.SimulationStep}  {outcome}  |  " +
            "WASD move / arrows turn / Enter / Z X / C V / F G";
        _contextStatus.Text = snapshot.ContextSelection is null
            ? "Context not selected."
            : FormatContext(snapshot.ContextSelection);
        _eventRequestStatus.Text = snapshot.EventRequest is null
            ? "Event request: none."
            : FormatEventRequest(snapshot.EventRequest);
        _effectStatus.Text = snapshot.LastEventEffect is null
            ? "Synthetic effect: none."
            : FormatEffect(snapshot);
        _transitionStatus.Text = snapshot.LocalTransition is null
            ? "Local transition: none."
            : FormatLocalTransition(snapshot.LocalTransition);
        _entityStatus.Text = FormatEntities(snapshot.Entities);
        _entityInteractionStatus.Text = snapshot.EntityInteraction is null
            ? "Placeholder interaction: none."
            : FormatEntityInteraction(snapshot.EntityInteraction);
    }

    private void RunHeadlessSmoke()
    {
        if (_session is null || _admissionReceipt is null)
        {
            FailStartup("Session was not admitted.");
            return;
        }

        GameSessionSnapshot before = _session.Snapshot;
        GameSessionCommandApplied? applied = _session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)) as GameSessionCommandApplied;
        if (applied is null || applied.Outcome != ExplorationMovementOutcome.Moved)
        {
            FailStartup("The bounded synthetic movement command did not move.");
            return;
        }

        GameSessionContextSelected? selected = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (selected is null ||
            selected.Selection.Position != applied.Snapshot.Exploration.PlayerPosition ||
            selected.Selection.SelectedSetup.Value != applied.Snapshot.AdmissionFacts.SetupIdentity ||
            selected.Selection.AreaDescription.Kind != AreaDescriptionSelectionKind.Text ||
            selected.Selection.ZoneEvent.Target.Value != "synthetic-map3-east-zone")
        {
            FailStartup("The bounded synthetic setup/area/event selection did not match.");
            return;
        }

        GameSessionEventRequested? requested = _session.Apply(
            new RequestSelectedZoneEventCommand()) as GameSessionEventRequested;
        if (requested is null ||
            requested.Request.Status != MapEventRequestStatus.Pending ||
            requested.Request.Target != selected.Selection.ZoneEvent.Target ||
            requested.Cue.Cue.Value != "synthetic-map3-east-zone-selected" ||
            !requested.Cue.RequiresAcknowledgement)
        {
            FailStartup("The bounded synthetic event request was not admitted.");
            return;
        }

        GameSessionEventEffectApplied? acknowledged = _session.Apply(
            new AcknowledgeMapEventRequestCommand(
                requested.Request.Request,
                requested.Cue.Sequence,
                requested.Request.ExpectedEffect)) as GameSessionEventEffectApplied;
        if (acknowledged is null ||
            acknowledged.Request.Status != MapEventRequestStatus.Acknowledged ||
            acknowledged.Request.CueSequence != requested.Cue.Sequence ||
            acknowledged.Effect.Effect.Value !=
                "synthetic-map3-east-zone-variant-effect" ||
            acknowledged.Effect.Flag.Value != "synthetic-map3-variant-enabled" ||
            acknowledged.Cue.Cue.Value != "synthetic-map3-variant-applied" ||
            acknowledged.Cue.RequiresAcknowledgement ||
            acknowledged.Snapshot.ContextSelection is not null ||
            !acknowledged.Snapshot.SyntheticFlags.IsSet(acknowledged.Effect.Flag))
        {
            FailStartup("The bounded synthetic state effect was not applied atomically.");
            return;
        }

        GameSessionContextSelected? reselected = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (reselected is null ||
            reselected.Selection.SelectedSetup.Value != "synthetic-map3-variant" ||
            reselected.Selection.Position != acknowledged.Snapshot.Exploration.PlayerPosition)
        {
            FailStartup("The synthetic setup variant was not visible after context re-selection.");
            return;
        }

        GameSessionCommandApplied? transitionMove = _session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)) as GameSessionCommandApplied;
        if (transitionMove is null ||
            transitionMove.Outcome != ExplorationMovementOutcome.Moved ||
            transitionMove.Snapshot.Exploration.PlayerPosition != new MapPosition(58, 3))
        {
            FailStartup("The bounded synthetic transition source was not reached.");
            return;
        }

        GameSessionContextSelected? transitionSelection = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (transitionSelection is null ||
            transitionSelection.Selection.ZoneEvent.Target.Value !=
                "synthetic-map3-local-transition-zone")
        {
            FailStartup("The bounded synthetic local-transition context did not match.");
            return;
        }

        GameSessionLocalTransitionRequested? transitionRequest = _session.Apply(
            new RequestSelectedLocalTransitionCommand()) as
            GameSessionLocalTransitionRequested;
        if (transitionRequest is null ||
            transitionRequest.Transition.Status != MapLocalTransitionStatus.Pending ||
            transitionRequest.Transition.SourcePosition != new MapPosition(58, 3) ||
            transitionRequest.Transition.DestinationPosition != new MapPosition(55, 4) ||
            transitionRequest.Cue.Cue.Value != "synthetic-map3-local-transition-ready" ||
            !transitionRequest.Cue.RequiresAcknowledgement)
        {
            FailStartup("The bounded synthetic local transition was not admitted.");
            return;
        }

        GameSessionLocalTransitionApplied? transitionApplied = _session.Apply(
            new AcknowledgeMapLocalTransitionCommand(
                transitionRequest.Transition.Request,
                transitionRequest.Cue.Sequence,
                transitionRequest.Transition.Transition)) as
            GameSessionLocalTransitionApplied;
        if (transitionApplied is null ||
            transitionApplied.Transition.Status != MapLocalTransitionStatus.Acknowledged ||
            transitionApplied.Snapshot.Exploration.Map.Value != "map3" ||
            transitionApplied.Snapshot.Exploration.PlayerPosition != new MapPosition(55, 4) ||
            transitionApplied.Transition.DestinationOrientation.Value !=
                "synthetic-arrival-south" ||
            transitionApplied.Snapshot.ContextSelection is not null ||
            transitionApplied.Snapshot.EventRequest is not null ||
            transitionApplied.Snapshot.LastEventEffect is not null)
        {
            FailStartup("The bounded synthetic local transition was not applied atomically.");
            return;
        }

        GameSessionFacingChanged? turned = _session.Apply(
            new TurnExplorationCommand(SemanticFacing.North)) as GameSessionFacingChanged;
        if (turned is null ||
            turned.Facing != SemanticFacing.North ||
            turned.Snapshot.Exploration.PlayerPosition != new MapPosition(55, 4) ||
            turned.Snapshot.EntityInteraction is not null)
        {
            FailStartup("The bounded synthetic facing command was not applied.");
            return;
        }

        GameSessionEntityInteractionRequested? entityRequested = _session.Apply(
            new RequestEntityInteractionCommand()) as GameSessionEntityInteractionRequested;
        if (entityRequested is null ||
            entityRequested.Interaction.Status != MapEntityInteractionStatus.Pending ||
            entityRequested.Interaction.Entity.Value !=
                "synthetic-map3-placeholder-guide" ||
            entityRequested.Interaction.Target.Value !=
                "synthetic-map3-placeholder-guide-target" ||
            entityRequested.Interaction.PlayerPosition != new MapPosition(55, 4) ||
            entityRequested.Interaction.EntityPosition != new MapPosition(55, 3) ||
            entityRequested.Interaction.Facing != SemanticFacing.North ||
            entityRequested.Cue.Cue.Value != "synthetic-map3-placeholder-guide-cue" ||
            !entityRequested.Cue.RequiresAcknowledgement)
        {
            FailStartup("The bounded placeholder entity interaction was not admitted.");
            return;
        }

        GameSessionEntityInteractionAcknowledged? entityAcknowledged = _session.Apply(
            new AcknowledgeEntityInteractionCommand(
                entityRequested.Interaction.Request,
                entityRequested.Cue.Sequence,
                entityRequested.Interaction.Entity,
                entityRequested.Interaction.Target)) as
            GameSessionEntityInteractionAcknowledged;
        if (entityAcknowledged is null ||
            entityAcknowledged.Interaction.Status !=
                MapEntityInteractionStatus.Acknowledged ||
            entityAcknowledged.Interaction.CueSequence != entityRequested.Cue.Sequence ||
            entityAcknowledged.Snapshot.Exploration.PlayerPosition != new MapPosition(55, 4))
        {
            FailStartup("The bounded placeholder entity interaction was not acknowledged.");
            return;
        }

        ProjectSnapshot("Placeholder entity interaction acknowledged");
        object receipt = new
        {
            status = "Pass",
            profile = "public-synthetic",
            scenarioId = applied.Snapshot.ScenarioId,
            exactControlledAdmission = _admissionReceipt.ExactControlledAdmission,
            capability = _admissionReceipt.Capabilities.Single(
                capability => capability == PublicSyntheticMap3PackageReader.Capability),
            evidenceOwner = _admissionReceipt.EvidenceOwnerIds.Single(),
            mapId = applied.Snapshot.AdmissionFacts.CurrentMap.Value,
            opaqueStartFacing = applied.Snapshot.AdmissionFacts.OpaqueStartFacing,
            before = new
            {
                x = before.Exploration.PlayerPosition.X,
                y = before.Exploration.PlayerPosition.Y,
            },
            after = new
            {
                x = applied.Snapshot.Exploration.PlayerPosition.X,
                y = applied.Snapshot.Exploration.PlayerPosition.Y,
            },
            outcome = applied.Outcome.ToString(),
            simulationStep = applied.Snapshot.SimulationStep,
            banner = BannerText,
        };
        GD.Print(SmokeMarker + JsonSerializer.Serialize(receipt));
        GetTree().Quit(0);
    }

    private void FailStartup(string message)
    {
        GD.PushError(message);
        if (_status is not null)
        {
            _status.Text = message;
        }

        if (OS.GetCmdlineUserArgs().Contains("--map3-smoke", StringComparer.Ordinal))
        {
            GD.Print(SmokeMarker + JsonSerializer.Serialize(new { status = "Fail", message }));
            GetTree().Quit(1);
        }
    }

    private void BuildPresentation()
    {
        Label banner = new()
        {
            Text = BannerText,
            Position = new Vector2(24, 18),
        };
        banner.AddThemeFontSizeOverride("font_size", 24);
        banner.AddThemeColorOverride("font_color", new Color("ffbd59"));
        AddChild(banner);

        Label explanation = new()
        {
            Text = "Project-authored selectors, cues, transitions, and placeholder entity interactions; targets are never interpreted.",
            Position = new Vector2(24, 55),
        };
        explanation.AddThemeFontSizeOverride("font_size", 16);
        AddChild(explanation);

        _viewport = new SyntheticMapViewport
        {
            Position = new Vector2(24, 105),
        };
        AddChild(_viewport);

        _status = new Label
        {
            Text = "Admitting synthetic package...",
            Position = new Vector2(24, 450),
        };
        _status.AddThemeFontSizeOverride("font_size", 18);
        AddChild(_status);

        _contextStatus = new Label
        {
            Text = "Context not selected.",
            Position = new Vector2(24, 480),
        };
        _contextStatus.AddThemeFontSizeOverride("font_size", 15);
        AddChild(_contextStatus);

        _eventRequestStatus = new Label
        {
            Text = "Event request: none.",
            Position = new Vector2(24, 510),
        };
        _eventRequestStatus.AddThemeFontSizeOverride("font_size", 15);
        AddChild(_eventRequestStatus);

        _effectStatus = new Label
        {
            Text = "Synthetic effect: none.",
            Position = new Vector2(24, 540),
        };
        _effectStatus.AddThemeFontSizeOverride("font_size", 15);
        AddChild(_effectStatus);

        _transitionStatus = new Label
        {
            Text = "Local transition: none.",
            Position = new Vector2(24, 570),
        };
        _transitionStatus.AddThemeFontSizeOverride("font_size", 15);
        AddChild(_transitionStatus);

        _entityStatus = new Label
        {
            Text = "Placeholder entities: none.",
            Position = new Vector2(24, 600),
        };
        _entityStatus.AddThemeFontSizeOverride("font_size", 15);
        AddChild(_entityStatus);

        _entityInteractionStatus = new Label
        {
            Text = "Placeholder interaction: none.",
            Position = new Vector2(24, 630),
        };
        _entityInteractionStatus.AddThemeFontSizeOverride("font_size", 15);
        AddChild(_entityInteractionStatus);
    }

    private static void RegisterInputMap()
    {
        RegisterAction("move_north", Key.W);
        RegisterAction("move_east", Key.D);
        RegisterAction("move_south", Key.S);
        RegisterAction("move_west", Key.A);
        RegisterAction("select_context", Key.Enter);
        RegisterAction("request_event", Key.Z);
        RegisterAction("acknowledge_event", Key.X);
        RegisterAction("request_transition", Key.C);
        RegisterAction("acknowledge_transition", Key.V);
        RegisterAction("turn_north", Key.Up);
        RegisterAction("turn_east", Key.Right);
        RegisterAction("turn_south", Key.Down);
        RegisterAction("turn_west", Key.Left);
        RegisterAction("request_entity_interaction", Key.F);
        RegisterAction("acknowledge_entity_interaction", Key.G);
    }

    private static void RegisterAction(string action, Key physicalKey)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action);
        }

        if (InputMap.ActionGetEvents(action).OfType<InputEventKey>().Any(
            input => input.PhysicalKeycode == physicalKey))
        {
            return;
        }

        InputMap.ActionAddEvent(
            action,
            new InputEventKey
            {
                PhysicalKeycode = physicalKey,
            });
    }

    private static string FormatContext(ExplorationContextSelectionSnapshot selection)
    {
        string area = selection.AreaDescription.Kind switch
        {
            AreaDescriptionSelectionKind.NoMatch => "none",
            AreaDescriptionSelectionKind.Text =>
                $"text {selection.AreaDescription.InvestigationTextIndex}/" +
                $"{selection.AreaDescription.DescriptionTextIndex}",
            AreaDescriptionSelectionKind.Function =>
                $"opaque function {selection.AreaDescription.Function}",
            _ => "unknown",
        };
        return $"Setup {selection.SelectedSetup}  Area {area}  " +
            $"Zone {selection.ZoneEvent.Target} (selected only)";
    }

    private static string FormatEventRequest(MapEventRequestSnapshot request) =>
        $"Event request {request.Request}: {request.Status}  " +
        $"Cue #{request.CueSequence}  Effect {request.ExpectedEffect}  " +
        $"Target {request.Target} (opaque)";

    private static string FormatEffect(GameSessionSnapshot snapshot)
    {
        MapEventEffectSnapshot effect = snapshot.LastEventEffect!;
        string setFlags = string.Join(", ", snapshot.SyntheticFlags.SetFlags);
        return $"Synthetic effect {effect.Effect}: applied once at step " +
            $"{effect.AppliedAtStep}; flag {effect.Flag}; setup flags [{setFlags}]";
    }

    private static string FormatLocalTransition(MapLocalTransitionSnapshot transition) =>
        $"Local transition {transition.Transition}: {transition.Status}  " +
        $"Cue #{transition.CueSequence}  ({transition.SourcePosition.X}, " +
        $"{transition.SourcePosition.Y}) -> ({transition.DestinationPosition.X}, " +
        $"{transition.DestinationPosition.Y})  Orientation {transition.DestinationOrientation}";

    private static string FormatEntities(IReadOnlyList<MapEntityDefinition> entities) =>
        entities.Count == 0
            ? "Placeholder entities: none."
            : "Placeholder entities: " + string.Join(
                ", ",
                entities.Select(entity =>
                    $"{entity.Entity}@({entity.Position.X},{entity.Position.Y})"));

    private static string FormatEntityInteraction(MapEntityInteractionSnapshot interaction) =>
        $"Placeholder interaction {interaction.Request}: {interaction.Status}  " +
        $"Cue #{interaction.CueSequence}  Entity {interaction.Entity}  " +
        $"Target {interaction.Target} (uninterpreted)";
}

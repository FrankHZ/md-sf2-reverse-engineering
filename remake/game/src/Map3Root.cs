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
            _effectStatus is null)
        {
            return;
        }

        GameSessionSnapshot snapshot = _session.Snapshot;
        _viewport.Project(snapshot);
        _status.Text =
            $"Map {snapshot.AdmissionFacts.CurrentMap}  " +
            $"Tile ({snapshot.Exploration.PlayerPosition.X}, " +
            $"{snapshot.Exploration.PlayerPosition.Y})  " +
            $"Step {snapshot.SimulationStep}  {outcome}  |  WASD / Enter / Z / X";
        _contextStatus.Text = snapshot.ContextSelection is null
            ? "Context not selected."
            : FormatContext(snapshot.ContextSelection);
        _eventRequestStatus.Text = snapshot.EventRequest is null
            ? "Event request: none."
            : FormatEventRequest(snapshot.EventRequest);
        _effectStatus.Text = snapshot.LastEventEffect is null
            ? "Synthetic effect: none."
            : FormatEffect(snapshot);
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

        ProjectSnapshot("Synthetic effect applied and context re-selected");
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
            Text = "Project-authored selectors, request cues, and synthetic effects; opaque targets are never executed.",
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
}

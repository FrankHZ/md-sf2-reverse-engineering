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

    private void ProjectSnapshot(string outcome)
    {
        if (_session is null || _viewport is null || _status is null)
        {
            return;
        }

        GameSessionSnapshot snapshot = _session.Snapshot;
        _viewport.Project(snapshot);
        _status.Text =
            $"Map {snapshot.AdmissionFacts.CurrentMap}  " +
            $"Tile ({snapshot.Exploration.PlayerPosition.X}, " +
            $"{snapshot.Exploration.PlayerPosition.Y})  " +
            $"Step {snapshot.SimulationStep}  {outcome}  |  WASD";
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

        ProjectSnapshot(applied.Outcome.ToString());
        object receipt = new
        {
            status = "Pass",
            profile = "public-synthetic",
            scenarioId = applied.Snapshot.ScenarioId,
            exactControlledAdmission = _admissionReceipt.ExactControlledAdmission,
            capability = _admissionReceipt.Capabilities.Single(),
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
            Text = "Project-authored grid and traversal only; original collision, art, events, and timing are unknown.",
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
            Position = new Vector2(24, 465),
        };
        _status.AddThemeFontSizeOverride("font_size", 18);
        AddChild(_status);
    }

    private static void RegisterInputMap()
    {
        RegisterAction("move_north", Key.W);
        RegisterAction("move_east", Key.D);
        RegisterAction("move_south", Key.S);
        RegisterAction("move_west", Key.A);
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
}

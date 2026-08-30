using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class Map3Root
{
    public const string PrivateBannerText =
        "PRIVATE LOCAL — NOT FULL ORIGINAL FIDELITY";
    public const string PrivateSmokeMarker = "SF2_MAP3_PRIVATE_LOCAL_SMOKE ";
    private const string PrivateStageMarker = "SF2_MAP3_PRIVATE_LOCAL_STAGE ";

    private Map3RuntimeProfile? _runtimeProfile;

    private void BuildSelectedPresentation(Map3RuntimeProfileSelection selection)
    {
        if (selection.IsAvailable &&
            selection.RequestedProfile == Map3RuntimeProfile.PublicSynthetic)
        {
            BuildPresentation();
            return;
        }

        BuildPrivatePresentation(selection);
    }

    private void BuildPrivatePresentation(Map3RuntimeProfileSelection selection)
    {
        string bannerText = selection.RequestedProfile == Map3RuntimeProfile.PrivateLocal
            ? PrivateBannerText
            : "PROFILE UNAVAILABLE — NO FALLBACK";
        Label banner = new()
        {
            Text = bannerText,
            Position = new Vector2(24, 18),
        };
        banner.AddThemeFontSizeOverride("font_size", 24);
        banner.AddThemeColorOverride("font_color", new Color("ff8f70"));
        AddChild(banner);

        Label explanation = new()
        {
            Text = "Ignored local canonical import; semantic movement only. " +
                "Original presentation and effects remain unavailable.",
            Position = new Vector2(24, 55),
        };
        explanation.AddThemeFontSizeOverride("font_size", 16);
        AddChild(explanation);

        _status = new Label
        {
            Text = selection.IsAvailable
                ? "Admitting PrivateLocal canonical Map 3..."
                : $"Unavailable: {selection.Diagnostic}",
            Position = new Vector2(24, 105),
        };
        _status.AddThemeFontSizeOverride("font_size", 18);
        AddChild(_status);
    }

    private void StartPrivateScenario(string canonicalImportPath, bool runSmoke)
    {
        long sessionStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        IOriginalMapImportSource source = new TimedOriginalMapImportSource(
            new PrivateCanonicalMap3ImportReader(canonicalImportPath),
            runSmoke);
        PrivateOriginalMapGameSessionStartResult result =
            GameSession.StartPrivateOriginalMap(
                source,
                new OriginalMapImportRequest(
                    OriginalMapRuntimeAdmission.PackageId,
                    ContentProfile.PrivateLocal,
                    OriginalMapRuntimeAdmission.AcceptedContentDigest));
        TracePrivateStage(runSmoke, "game-session-start", sessionStarted);
        if (result is not PrivateOriginalMapGameSessionStarted started)
        {
            PrivateOriginalMapGameSessionStartRejected rejected =
                (PrivateOriginalMapGameSessionStartRejected)result;
            FailPrivateStartup(
                $"PrivateLocal unavailable ({rejected.Diagnostic.Code}).",
                runSmoke,
                "private-local");
            return;
        }

        _session = started.Session;
        ProjectPrivateSnapshot(started.Session.PrivateOriginalMapSnapshot, "Ready");
        if (runSmoke)
        {
            Callable.From(RunPrivateHeadlessSmoke).CallDeferred();
            TracePrivateStage(
                enabled: true,
                "deferred-smoke-scheduled",
                System.Diagnostics.Stopwatch.GetTimestamp());
        }
    }

    private void ProcessPrivateInput()
    {
        if (Input.IsActionJustPressed("move_north"))
        {
            ApplyPrivateMove(ExplorationDirection.North);
        }
        else if (Input.IsActionJustPressed("move_east"))
        {
            ApplyPrivateMove(ExplorationDirection.East);
        }
        else if (Input.IsActionJustPressed("move_south"))
        {
            ApplyPrivateMove(ExplorationDirection.South);
        }
        else if (Input.IsActionJustPressed("move_west"))
        {
            ApplyPrivateMove(ExplorationDirection.West);
        }
    }

    private void ApplyPrivateMove(ExplorationDirection direction)
    {
        if (_session is null)
        {
            return;
        }

        PrivateOriginalMapMoveApplied applied = _session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(direction));
        ProjectPrivateSnapshot(applied.Snapshot, applied.Traversal.Outcome.ToString());
    }

    private void ProjectPrivateSnapshot(
        PrivateOriginalMapSessionSnapshot snapshot,
        string outcome)
    {
        if (_status is null)
        {
            return;
        }

        _status.Text =
            $"Map {snapshot.Map}  Tile ({snapshot.PlayerPosition.X}, " +
            $"{snapshot.PlayerPosition.Y})  Step {snapshot.SimulationStep}  {outcome}  |  " +
            "WASD semantic movement";
    }

    private void RunPrivateHeadlessSmoke()
    {
        long smokeStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        TracePrivateStage(
            enabled: true,
            "deferred-smoke-entered",
            smokeStarted);
        if (_session is null)
        {
            FailPrivateStartup(
                "PrivateLocal session was not admitted.",
                runSmoke: true,
                "private-local");
            return;
        }

        PrivateOriginalMapSessionSnapshot before = _session.PrivateOriginalMapSnapshot;
        PrivateOriginalMapMoveApplied? moved = null;
        ExplorationDirection movedDirection = ExplorationDirection.East;
        foreach (ExplorationDirection direction in new[]
        {
            ExplorationDirection.East,
            ExplorationDirection.South,
            ExplorationDirection.West,
            ExplorationDirection.North,
        })
        {
            PrivateOriginalMapMoveApplied applied = _session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(direction));
            ProjectPrivateSnapshot(applied.Snapshot, applied.Traversal.Outcome.ToString());
            if (applied.Traversal.Outcome == OriginalMapTraversalOutcome.Moved)
            {
                moved = applied;
                movedDirection = direction;
                break;
            }
        }

        if (moved is null)
        {
            FailPrivateStartup(
                "No bounded semantic movement was admitted from the controlled start.",
                runSmoke: true,
                "private-local");
            return;
        }

        object receipt = new
        {
            status = "Pass",
            profile = "private-local",
            packageId = OriginalMapRuntimeAdmission.PackageId,
            capability = OriginalMapRuntimeAdmission.TraversalCapability,
            mapId = moved.Snapshot.Map.Value,
            before = new
            {
                x = before.PlayerPosition.X,
                y = before.PlayerPosition.Y,
            },
            after = new
            {
                x = moved.Snapshot.PlayerPosition.X,
                y = moved.Snapshot.PlayerPosition.Y,
            },
            direction = movedDirection.ToString(),
            outcome = moved.Traversal.Outcome.ToString(),
            simulationStep = moved.Snapshot.SimulationStep,
            banner = PrivateBannerText,
        };
        GD.Print(PrivateSmokeMarker + JsonSerializer.Serialize(receipt));
        TracePrivateStage(enabled: true, "quit-scheduled", smokeStarted);
        GetTree().Quit(0);
    }

    private void FailProfileStartup(Map3RuntimeProfileSelection selection)
    {
        string message = selection.Diagnostic ?? "Runtime profile is unavailable.";
        string profile = selection.RequestedProfile switch
        {
            Map3RuntimeProfile.PrivateLocal => "private-local",
            Map3RuntimeProfile.PublicSynthetic => "public-synthetic",
            _ => "unavailable",
        };
        FailPrivateStartup(message, selection.PrivateSmokeRequested, profile);
    }

    private void FailPrivateStartup(string message, bool runSmoke, string profile)
    {
        GD.PrintErr(message);
        if (_status is not null)
        {
            _status.Text = $"Unavailable: {message}";
        }

        if (runSmoke)
        {
            GD.Print(PrivateSmokeMarker + JsonSerializer.Serialize(
                new { status = "Fail", profile, message }));
            TracePrivateStage(
                enabled: true,
                "failure-quit-scheduled",
                System.Diagnostics.Stopwatch.GetTimestamp());
            GetTree().Quit(1);
        }
    }

    private static void TracePrivateStage(bool enabled, string stage, long started)
    {
        if (!enabled)
        {
            return;
        }

        double elapsedMilliseconds =
            System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        GD.Print(PrivateStageMarker + JsonSerializer.Serialize(
            new
            {
                stage,
                elapsedMilliseconds = Math.Round(elapsedMilliseconds, 3),
            }));
    }

    private sealed class TimedOriginalMapImportSource : IOriginalMapImportSource
    {
        private readonly IOriginalMapImportSource _inner;
        private readonly bool _trace;

        public TimedOriginalMapImportSource(IOriginalMapImportSource inner, bool trace)
        {
            _inner = inner;
            _trace = trace;
        }

        public OriginalMapImportResult Admit(OriginalMapImportRequest request)
        {
            long started = System.Diagnostics.Stopwatch.GetTimestamp();
            OriginalMapImportResult result = _inner.Admit(request);
            TracePrivateStage(_trace, "fixed-digest-read-parse-admission", started);
            return result;
        }
    }
}

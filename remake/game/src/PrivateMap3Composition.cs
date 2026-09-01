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
    public const string PrivateViewSmokeMarker =
        "SF2_MAP3_PRIVATE_LOCAL_VIEW_SMOKE ";
    public const string PrivateStepCopySmokeMarker =
        "SF2_MAP3_PRIVATE_LOCAL_STEP_COPY_SMOKE ";
    public const string PrivateAreaSmokeMarker =
        "SF2_MAP3_PRIVATE_LOCAL_AREA_SMOKE ";
    public const string PrivateViewCapability =
        "private-local-map3-traversal-diagnostic-view-v1";
    private const string PrivateStageMarker = "SF2_MAP3_PRIVATE_LOCAL_STAGE ";

    private Map3RuntimeProfile? _runtimeProfile;
    private PrivateMap3Presenter? _privatePresenter;

    private void BuildSelectedPresentation(Map3RuntimeProfileSelection selection)
    {
        if (selection.IsAvailable &&
            selection.RequestedProfile == Map3RuntimeProfile.PublicSynthetic)
        {
            BuildPresentation();
            return;
        }

        PrivateMap3PresentationPlan plan;
        if (selection.IsAvailable)
        {
            plan = PrivateMap3PresentationPlan.PrivateLocalAvailable();
        }
        else if (selection.RequestedProfile == Map3RuntimeProfile.PrivateLocal)
        {
            plan = PrivateMap3PresentationPlan.PrivateLocalUnavailable(
                selection.Diagnostic ?? "Runtime profile is unavailable.");
        }
        else
        {
            plan = PrivateMap3PresentationPlan.ProfileUnavailable(
                selection.Diagnostic ?? "Runtime profile is unavailable.");
        }

        _privatePresenter = PrivateMap3Presenter.Attach(this, plan);
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
        _privatePresenter?.Project(started.Session.PrivateOriginalMapSnapshot, "Ready");
        if (runSmoke)
        {
            PrivateMap3Presenter presenter = _privatePresenter!;
            Callable.From(() => RunPrivateHeadlessSmoke(
                started.Session,
                presenter)).CallDeferred();
            TracePrivateStage(
                enabled: true,
                "deferred-smoke-scheduled",
                System.Diagnostics.Stopwatch.GetTimestamp());
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
        _privatePresenter?.Project(applied.Snapshot, applied.Traversal.Outcome.ToString());
    }

    private void RunPrivateHeadlessSmoke(
        GameSession session,
        PrivateMap3Presenter presenter)
    {
        long smokeStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        TracePrivateStage(
            enabled: true,
            "deferred-smoke-entered",
            smokeStarted);
        PrivateMap3SmokeDriver.Run(GetTree(), session, presenter, smokeStarted);
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
        _privatePresenter?.ProjectStatus(message);

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

    internal static void TracePrivateStage(bool enabled, string stage, long started)
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

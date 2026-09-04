using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal enum PrivateBattleBridgeBackAction
{
    None,
    DeclineEntry,
    CancelTacticalSelection,
}

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
    public const string PrivateBaseAtlasSmokeMarker =
        "SF2_MAP3_PRIVATE_LOCAL_BASE_ATLAS_SMOKE ";
    public const string PrivateWorldTreatmentSmokeMarker =
        "SF2_MAP3_PRIVATE_LOCAL_WORLD_TREATMENT_SMOKE ";
    public const string PrivateViewCapability =
        "private-local-map3-traversal-diagnostic-view-v1";
    public const string PrivateBaseAtlasCapability =
        "private-local-map3-base-atlas-diagnostic-consumer-v1";
    public const string PrivateWorldTreatmentCapability =
        "private-local-map3-edge-scale2x-world-treatment-v1";
    private const string PrivateStageMarker = "SF2_MAP3_PRIVATE_LOCAL_STAGE ";

    private Map3RuntimeProfile? _runtimeProfile;
    private PrivateMap3Presenter? _privatePresenter;
    private PrivateLocalHudPreview? _privateHudPreview;
    private bool _privateBattleBridgeEnabled;

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
            plan = selection.PrivateBaseViewRequested
                ? PrivateMap3PresentationPlan.PrivateLocalWithBaseVisual(
                    selection.WorldTreatment,
                    selection.PrivateStaticOverlayRequested)
                : PrivateMap3PresentationPlan.PrivateLocalAvailable();
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
        if (selection.IsAvailable && selection.PrivateBaseViewRequested)
        {
            _battlePresenter = PublicSyntheticBattlePresenter.Attach(this);
        }
    }

    private void StartPrivateScenario(Map3RuntimeProfileSelection selection)
    {
        bool runSmoke = selection.PrivateSmokeRequested;
        string canonicalImportPath = selection.CanonicalImportPath!;
        long sessionStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        OriginalMapImportRequest importRequest = new(
            OriginalMapRuntimeAdmission.PackageId,
            ContentProfile.PrivateLocal,
            OriginalMapRuntimeAdmission.AcceptedContentDigest);
        IOriginalMapImportSource source = new TimedOriginalMapImportSource(
            new PrivateCanonicalMap3ImportReader(canonicalImportPath),
            runSmoke);
        PrivateLocalPresentationRasterMount? hudPreview = null;
        PrivateLocalPresentationRasterMount? tacticalCursor = null;
        PrivateLocalPresentationRasterMount? baseAtlas = null;
        PrivateLocalPresentationRasterMount? playerReference = null;
        PrivateLocalPlayerLocomotionMount? playerLocomotion = null;
        PrivateLocalPresentationRasterMount? entity142Diagnostic = null;
        if ((selection.PrivateHudPreviewRequested || selection.PrivateBaseAtlasRequested) &&
            !TryPreparePrivatePresentationAssets(
                selection,
                importRequest,
                ref source,
                out hudPreview,
                out tacticalCursor,
                out baseAtlas,
                out playerReference,
                out playerLocomotion,
                out entity142Diagnostic))
        {
            return;
        }

        if (selection.PrivateBaseViewRequested)
        {
            StartPrivateVisualScenario(
                selection,
                source,
                importRequest,
                hudPreview,
                tacticalCursor,
                baseAtlas,
                playerReference,
                playerLocomotion,
                entity142Diagnostic,
                runSmoke,
                sessionStarted);
            return;
        }

        PrivateOriginalMapGameSessionStartResult result =
            GameSession.StartPrivateOriginalMap(source, importRequest);
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

        if (!TryAttachPrivateHudPreview(
                hudPreview,
                battleEntryChoiceEnabled: false,
                runSmoke))
        {
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

    private void StartPrivateVisualScenario(
        Map3RuntimeProfileSelection selection,
        IOriginalMapImportSource importSource,
        OriginalMapImportRequest importRequest,
        PrivateLocalPresentationRasterMount? hudPreview,
        PrivateLocalPresentationRasterMount? tacticalCursor,
        PrivateLocalPresentationRasterMount? baseAtlas,
        PrivateLocalPresentationRasterMount? playerReference,
        PrivateLocalPlayerLocomotionMount? playerLocomotion,
        PrivateLocalPresentationRasterMount? entity142Diagnostic,
        bool runSmoke,
        long sessionStarted)
    {
        PrivateOriginalMapGameSessionStartResult result =
            GameSession.StartPrivateOriginalMap(importSource, importRequest);
        TracePrivateStage(runSmoke, "game-session-start", sessionStarted);
        if (result is not PrivateOriginalMapGameSessionStarted started)
        {
            PrivateOriginalMapGameSessionStartRejected rejected =
                (PrivateOriginalMapGameSessionStartRejected)result;
            FailPrivateStartup(
                $"PrivateLocal base view unavailable ({rejected.Diagnostic.Code}).",
                runSmoke,
                "private-local");
            return;
        }

        PrivateOriginalMapBattleBridgeBindingResult bridgeBinding =
            BindPrivateBattleBridge(started.Session);
        if (bridgeBinding is not PrivateOriginalMapBattleBridgeBound)
        {
            PrivateOriginalMapBattleBridgeBindingRejected rejected =
                (PrivateOriginalMapBattleBridgeBindingRejected)bridgeBinding;
            FailPrivateStartup(
                $"PrivateLocal battle bridge unavailable ({rejected.Diagnostic.Code}).",
                runSmoke,
                "private-local");
            return;
        }

        if (!TryAttachPrivateHudPreview(
                hudPreview,
                battleEntryChoiceEnabled: true,
                runSmoke))
        {
            return;
        }

        if (!TryAttachPrivateTacticalCursor(tacticalCursor, runSmoke))
        {
            return;
        }

        _session = started.Session;
        _privateBattleBridgeEnabled = true;
        _inputAdapter = Map3InputAdapter.CreateGodot(CreatePrivateBattleBridgeInputActions());
        _privateHudPreview?.ProjectBattleEntryChoice(
            started.Session.PrivateOriginalMapBattleBridge);
        PrivateMap3Presenter presenter = _privatePresenter!;
        if (baseAtlas is not null &&
            !presenter.TryBindBaseAtlas(
                baseAtlas,
                started.Session.PrivateOriginalMapSnapshot,
                out PrivateLocalPresentationAssetMountDiagnostic? atlasDiagnostic))
        {
            FailPrivateStartup(
                $"PrivateLocal Map 3 base atlas unavailable ({atlasDiagnostic!.Code}).",
                runSmoke,
                "private-local");
            return;
        }

        if (playerReference is not null &&
            !presenter.TryBindPlayerReference(
                playerReference,
                out PrivateLocalPresentationAssetMountDiagnostic? playerDiagnostic))
        {
            FailPrivateStartup(
                $"PrivateLocal Map 3 player reference unavailable ({playerDiagnostic!.Code}).",
                runSmoke,
                "private-local");
            return;
        }

        if (playerLocomotion is not null &&
            !presenter.TryBindPlayerLocomotion(
                playerLocomotion,
                out PrivateLocalPresentationAssetMountDiagnostic? locomotionDiagnostic))
        {
            FailPrivateStartup(
                $"PrivateLocal Map 3 player locomotion unavailable ({locomotionDiagnostic!.Code}).",
                runSmoke,
                "private-local");
            return;
        }

        if (entity142Diagnostic is not null &&
            !presenter.TryBindEntity142Diagnostic(
                entity142Diagnostic,
                started.Session.PrivateOriginalMapSnapshot,
                out PrivateLocalPresentationAssetMountDiagnostic? entityDiagnostic))
        {
            FailPrivateStartup(
                $"PrivateLocal Map 3 entity-142 diagnostic unavailable ({entityDiagnostic!.Code}).",
                runSmoke,
                "private-local");
            return;
        }

        presenter.Project(
            started.Session.PrivateOriginalMapSnapshot,
            "Ready",
            started.Session.PrivateOriginalMapPlayerLocomotion);
        if (runSmoke)
        {
            Callable.From(() => RunPrivateHeadlessSmoke(
                started.Session,
                presenter)).CallDeferred();
            TracePrivateStage(
                enabled: true,
                "deferred-smoke-scheduled",
                System.Diagnostics.Stopwatch.GetTimestamp());
        }
    }

    private bool TryPreparePrivatePresentationAssets(
        Map3RuntimeProfileSelection selection,
        OriginalMapImportRequest importRequest,
        ref IOriginalMapImportSource importSource,
        out PrivateLocalPresentationRasterMount? hudPreview,
        out PrivateLocalPresentationRasterMount? tacticalCursor,
        out PrivateLocalPresentationRasterMount? baseAtlas,
        out PrivateLocalPresentationRasterMount? playerReference,
        out PrivateLocalPlayerLocomotionMount? playerLocomotion,
        out PrivateLocalPresentationRasterMount? entity142Diagnostic)
    {
        hudPreview = null;
        tacticalCursor = null;
        baseAtlas = null;
        playerReference = null;
        playerLocomotion = null;
        entity142Diagnostic = null;
        OriginalMapImportResult importResult = importSource.Admit(importRequest);
        if (importResult is not OriginalMapImportAccepted importAccepted)
        {
            string code = importResult is OriginalMapImportRejected rejected
                ? rejected.Diagnostic.Code.ToString()
                : "UnknownResult";
            FailPrivateStartup(
                $"PrivateLocal canonical import unavailable ({code}).",
                selection.PrivateSmokeRequested,
                "private-local");
            return false;
        }

        LocalPresentationAssetPackReader packReader = new(
            selection.PresentationAssetRoot!,
            selection.PresentationAssetCommit!);
        LocalPresentationAssetPackRequest packRequest = new(
            LocalPresentationAssetPackAdmission.PackageId,
            ContentProfile.PrivateLocal,
            LocalPresentationAssetPackAdmission.RepositoryId,
            selection.PresentationAssetCommit!,
            selection.PresentationManifestDigest!);
        LocalPresentationAssetPackResult packResult = packReader.Admit(packRequest);
        if (packResult is not LocalPresentationAssetPackAccepted acceptedPack)
        {
            LocalPresentationAssetPackRejected rejected =
                (LocalPresentationAssetPackRejected)packResult;
            FailPrivateStartup(
                $"PrivateLocal presentation unavailable ({rejected.Diagnostic.Code}).",
                selection.PrivateSmokeRequested,
                "private-local");
            return false;
        }

        PrivateLocalPresentationAssetCatalog catalog = new(packReader);
        double effectivePhysicalScale = EffectivePhysicalScale();
        if (selection.PrivateHudPreviewRequested)
        {
            PrivateLocalPresentationAssetMountResult previewResult =
                catalog.MountPreview(
                    packRequest,
                    acceptedPack,
                    effectivePhysicalScale);
            if (previewResult is not PrivateLocalPresentationAssetMounted mountedPreview)
            {
                PrivateLocalPresentationAssetMountRejected rejected =
                    (PrivateLocalPresentationAssetMountRejected)previewResult;
                FailPrivateStartup(
                    $"PrivateLocal HUD preview unavailable ({rejected.Diagnostic.Code}).",
                    selection.PrivateSmokeRequested,
                    "private-local");
                return false;
            }

            if (selection.PrivateBaseViewRequested)
            {
                PrivateLocalPresentationAssetMountResult cursorResult =
                    catalog.MountTacticalCursor(
                        packRequest,
                        acceptedPack,
                        effectivePhysicalScale);
                if (cursorResult is not PrivateLocalPresentationAssetMounted mountedCursor)
                {
                    PrivateLocalPresentationAssetMountRejected rejected =
                        (PrivateLocalPresentationAssetMountRejected)cursorResult;
                    FailPrivateStartup(
                        $"PrivateLocal tactical cursor unavailable ({rejected.Diagnostic.Code}).",
                        selection.PrivateSmokeRequested,
                        "private-local");
                    return false;
                }

                tacticalCursor = mountedCursor.Asset;
            }

            hudPreview = mountedPreview.Asset;
        }

        if (selection.PrivateBaseAtlasRequested)
        {
            PrivateLocalPresentationAssetMountResult atlasResult =
                catalog.MountMap3BaseAtlas(
                    packRequest,
                    acceptedPack,
                    effectivePhysicalScale);
            if (atlasResult is not PrivateLocalPresentationAssetMounted mountedAtlas)
            {
                PrivateLocalPresentationAssetMountRejected rejected =
                    (PrivateLocalPresentationAssetMountRejected)atlasResult;
                FailPrivateStartup(
                    $"PrivateLocal Map 3 base atlas unavailable ({rejected.Diagnostic.Code}).",
                    selection.PrivateSmokeRequested,
                    "private-local");
                return false;
            }

            baseAtlas = mountedAtlas.Asset;

            PrivateLocalPresentationAssetMountResult playerResult =
                catalog.MountMap3PlayerReference(
                    packRequest,
                    acceptedPack,
                    effectivePhysicalScale);
            if (playerResult is not PrivateLocalPresentationAssetMounted mountedPlayer)
            {
                PrivateLocalPresentationAssetMountRejected rejected =
                    (PrivateLocalPresentationAssetMountRejected)playerResult;
                FailPrivateStartup(
                    $"PrivateLocal Map 3 player reference unavailable ({rejected.Diagnostic.Code}).",
                    selection.PrivateSmokeRequested,
                    "private-local");
                return false;
            }

            playerReference = mountedPlayer.Asset;

            PrivateLocalPlayerLocomotionMountResult locomotionResult =
                catalog.MountMap3PlayerLocomotion(
                    packRequest,
                    acceptedPack,
                    effectivePhysicalScale);
            if (locomotionResult is not PrivateLocalPlayerLocomotionMounted mountedLocomotion)
            {
                PrivateLocalPlayerLocomotionMountRejected rejected =
                    (PrivateLocalPlayerLocomotionMountRejected)locomotionResult;
                FailPrivateStartup(
                    $"PrivateLocal Map 3 player locomotion unavailable ({rejected.Diagnostic.Code}).",
                    selection.PrivateSmokeRequested,
                    "private-local");
                return false;
            }

            playerLocomotion = mountedLocomotion.Animation;

            PrivateLocalPresentationAssetMountResult entityResult =
                catalog.MountMap3Entity142Reference(
                    packRequest,
                    acceptedPack,
                    effectivePhysicalScale);
            if (entityResult is not PrivateLocalPresentationAssetMounted mountedEntity)
            {
                PrivateLocalPresentationAssetMountRejected rejected =
                    (PrivateLocalPresentationAssetMountRejected)entityResult;
                FailPrivateStartup(
                    $"PrivateLocal Map 3 entity-142 diagnostic unavailable ({rejected.Diagnostic.Code}).",
                    selection.PrivateSmokeRequested,
                    "private-local");
                return false;
            }

            entity142Diagnostic = mountedEntity.Asset;
        }

        importSource = new PreadmittedOriginalMapImportSource(
            importRequest,
            importAccepted);
        return true;
    }

    private bool TryAttachPrivateHudPreview(
        PrivateLocalPresentationRasterMount? mount,
        bool battleEntryChoiceEnabled,
        bool runSmoke)
    {
        if (mount is null)
        {
            return true;
        }

        _privateHudPreview = PrivateLocalHudPreview.TryAttach(
            this,
            mount,
            battleEntryChoiceEnabled,
            out PrivateLocalPresentationAssetMountDiagnostic? diagnostic);
        if (_privateHudPreview is not null)
        {
            return true;
        }

        FailPrivateStartup(
            $"PrivateLocal HUD preview unavailable ({diagnostic!.Code}).",
            runSmoke,
            "private-local");
        return false;
    }

    private bool TryAttachPrivateTacticalCursor(
        PrivateLocalPresentationRasterMount? mount,
        bool runSmoke)
    {
        if (mount is null)
        {
            return true;
        }

        PrivateLocalPresentationAssetMountDiagnostic? diagnostic = null;
        if (_battlePresenter is not null &&
            _battlePresenter.TryAttachPrivateTacticalCursor(mount, out diagnostic))
        {
            return true;
        }

        FailPrivateStartup(
            $"PrivateLocal tactical cursor unavailable ({diagnostic?.Code ?? PrivateLocalPresentationAssetMountFailureCode.InvalidBinding}).",
            runSmoke,
            "private-local");
        return false;
    }

    private static double EffectivePhysicalScale()
    {
        Vector2I window = DisplayServer.WindowGetSize();
        return window.X > 0 && window.Y > 0
            ? PrivateLocalPresentationAssetCatalog.EffectivePhysicalScale(
                window.X,
                window.Y)
            : 1;
    }

    private void ApplyPrivateMove(ExplorationDirection direction)
    {
        if (_session is null)
        {
            return;
        }

        if (_session.PrivateOriginalMapPlayerLocomotion.IsMoving)
        {
            return;
        }

        PrivateOriginalMapPlayerLocomotionStarted started =
            _session.BeginPrivateOriginalMapPlayerLocomotion(
                new MoveExplorationCommand(direction));
        _privatePresenter?.Project(
            started.Move.Snapshot,
            started.Move.Traversal.Outcome.ToString(),
            started.Animation);
        if (_privateBattleBridgeEnabled)
        {
            _battlePresenter?.Project(
                _session.PrivateOriginalMapBattleBridge,
                "Private Map 3 traversal resumed");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        if (_runtimeProfile != Map3RuntimeProfile.PrivateLocal ||
            _session is null ||
            !_session.PrivateOriginalMapPlayerLocomotion.IsMoving)
        {
            return;
        }

        PrivateOriginalMapPlayerLocomotionSnapshot animation =
            _session.AdvancePrivateOriginalMapPlayerLocomotion();
        _privatePresenter?.Project(
            _session.PrivateOriginalMapSnapshot,
            animation.IsMoving ? "Moving" : "Moved",
            animation);
    }

    private Map3InputActions CreatePrivateBattleBridgeInputActions() =>
        new(
            ApplyPrivateMoveWhenAvailable,
            static () => { },
            static () => { },
            static () => { },
            static () => { },
            static () => { },
            static _ => { },
            static () => { },
            static () => { },
            static () => { },
            static () => { },
            static () => { },
            static () => { },
            static () => { },
            static () => { },
            static () => { },
            ApplyPrivateBattleBridgeRequest,
            ApplyPrivateBattleBridgeEntryAcknowledgement,
            ApplyPrivateBattleBridgeCursorMove,
            ApplyPrivateBattleBridgeSelectionConfirmation,
            ApplyPrivateBattleBridgeSelectionCancellation,
            ApplyPrivateBattleBridgeCompletionAcknowledgement);

    private PrivateOriginalMapBattleBridgeBindingResult BindPrivateBattleBridge(
        GameSession session)
    {
        byte[] packageBytes = Godot.FileAccess.GetFileAsBytes(
            "res://content/public-synthetic-map3-smoke-v1.json");
        PublicSyntheticMap3PackageReader source =
            PublicSyntheticMap3PackageReader.FromDocumentBytes(packageBytes);
        MapScenarioAdmissionResult result = source.Admit(
            new MapScenarioRequest(
                PublicSyntheticMap3PackageReader.PackageId,
                ContentProfile.PublicSynthetic));
        if (result is not MapScenarioAccepted accepted ||
            accepted.Scenario.MapContext.PublicSyntheticBattles.Definitions.Count != 1)
        {
            string message = result is MapScenarioRejected rejected
                ? $"Tracked battle package rejected ({rejected.Diagnostic.Code})."
                : "Tracked battle package did not expose one tactical definition.";
            return new PrivateOriginalMapBattleBridgeBindingRejected(
                new PrivateOriginalMapBattleBridgeDiagnostic(
                    PrivateOriginalMapBattleBridgeFailureCode.BattleDefinitionUnavailable,
                    message));
        }

        return session.BindPrivateOriginalMapBattleBridge(
            accepted.Scenario.MapContext.PublicSyntheticBattles.Definitions[0]);
    }

    private void ApplyPrivateMoveWhenAvailable(ExplorationDirection direction)
    {
        if (_session?.PrivateOriginalMapBattleBridge?.IsBusy == true)
        {
            return;
        }

        ApplyPrivateMove(direction);
    }

    private void ApplyPrivateBattleBridgeRequest()
    {
        if (_session?.PrivateOriginalMapBattleBridge is not
            PrivateOriginalMapBattleBridgeSnapshot bridge)
        {
            return;
        }

        ProjectPrivateBattleResult(
            _session.ApplyPrivateOriginalMapBattleBridge(
                new RequestPrivateOriginalMapBattleBridgeCommand(
                    bridge.Definition.Bridge,
                    _session.PrivateOriginalMapSnapshot.SimulationStep)),
            "Project-authored private battle bridge requested");
    }

    private void ApplyPrivateBattleBridgeEntryAcknowledgement()
    {
        if (_session?.PrivateOriginalMapBattleBridge is not
            PrivateOriginalMapBattleBridgeSnapshot
            {
                Status: PrivateOriginalMapBattleBridgeStatus.Pending,
            } bridge)
        {
            return;
        }

        ProjectPrivateBattleResult(
            _session.ApplyPrivateOriginalMapBattleBridge(
                new AcknowledgePublicSyntheticBattleEntryCommand(
                    bridge.Definition.Request,
                    bridge.Definition.Rules.Battle,
                    bridge.LastCueSequence)),
            "Project-authored tactical battle admitted");
    }

    private void ApplyPrivateBattleBridgeCursorMove(TacticalDirection direction)
    {
        if (_session is null)
        {
            return;
        }

        ProjectPrivateBattleResult(
            _session.ApplyPrivateOriginalMapBattleBridge(
                new MovePublicSyntheticBattleCursorCommand(direction)),
            "Project-authored tactical cursor moved");
    }

    private void ApplyPrivateBattleBridgeSelectionConfirmation()
    {
        if (_session is null)
        {
            return;
        }

        ProjectPrivateBattleResult(
            _session.ApplyPrivateOriginalMapBattleBridge(
                new ConfirmPublicSyntheticBattleSelectionCommand()),
            "Project-authored tactical selection confirmed");
    }

    private void ApplyPrivateBattleBridgeSelectionCancellation()
    {
        if (_session?.PrivateOriginalMapBattleBridge is not
            PrivateOriginalMapBattleBridgeSnapshot bridge)
        {
            return;
        }

        PrivateBattleBridgeBackAction action = RoutePrivateBattleBridgeBackAction(
            bridge.Status);
        if (action == PrivateBattleBridgeBackAction.DeclineEntry)
        {
            ProjectPrivateBattleResult(
                _session.ApplyPrivateOriginalMapBattleBridge(
                    new DeclinePrivateOriginalMapBattleBridgeEntryCommand(
                        bridge.Definition.Bridge,
                        bridge.Definition.Request,
                        bridge.LastCueSequence)),
                "Project-authored tactical battle entry declined");
            return;
        }

        if (action != PrivateBattleBridgeBackAction.CancelTacticalSelection)
        {
            return;
        }

        ProjectPrivateBattleResult(
            _session.ApplyPrivateOriginalMapBattleBridge(
                new CancelPublicSyntheticBattleSelectionCommand()),
            "Project-authored tactical selection cancelled");
    }

    internal static PrivateBattleBridgeBackAction RoutePrivateBattleBridgeBackAction(
        PrivateOriginalMapBattleBridgeStatus status) =>
        status switch
        {
            PrivateOriginalMapBattleBridgeStatus.Pending =>
                PrivateBattleBridgeBackAction.DeclineEntry,
            PrivateOriginalMapBattleBridgeStatus.Active =>
                PrivateBattleBridgeBackAction.CancelTacticalSelection,
            _ => PrivateBattleBridgeBackAction.None,
        };

    private void ApplyPrivateBattleBridgeCompletionAcknowledgement()
    {
        if (_session?.PrivateOriginalMapBattleBridge is not
            PrivateOriginalMapBattleBridgeSnapshot
            {
                Status: PrivateOriginalMapBattleBridgeStatus.Completed,
            } bridge)
        {
            return;
        }

        ProjectPrivateBattleResult(
            _session.ApplyPrivateOriginalMapBattleBridge(
                new AcknowledgePublicSyntheticBattleCompletionCommand(
                    bridge.Definition.Rules.Battle,
                    bridge.LastCueSequence)),
            bridge.BattleState?.Outcome == TacticalBattleOutcome.Defeat
                ? "Project-authored battle defeated; retry started"
                : "Project-authored battle complete; private Map 3 restored");
    }

    private void ProjectPrivateBattleResult(
        GameSessionCommandResult result,
        string outcome)
    {
        if (_session is null)
        {
            return;
        }

        if (result is PrivateOriginalMapBattleBridgeRejected rejected)
        {
            _privatePresenter?.ProjectStatus(rejected.Diagnostic.Message);
            _battlePresenter?.Project(rejected.Bridge, rejected.Diagnostic.Message, result);
            return;
        }

        _battlePresenter?.Project(
            _session.PrivateOriginalMapBattleBridge,
            outcome,
            result);
        _privateHudPreview?.ProjectBattleEntryChoice(
            _session.PrivateOriginalMapBattleBridge);
        if (result is PrivateOriginalMapBattleBridgeReturned returned)
        {
            _privatePresenter?.Project(returned.Snapshot, outcome);
        }
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
        if (_battlePresenter is null)
        {
            PrivateMap3SmokeDriver.Run(GetTree(), session, presenter, smokeStarted);
            return;
        }

        PrivateMap3SmokeDriver.Run(
            GetTree(),
            session,
            presenter,
            _battlePresenter,
            smokeStarted);
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

    private sealed class PreadmittedOriginalMapImportSource : IOriginalMapImportSource
    {
        private readonly OriginalMapImportRequest _expectedRequest;
        private readonly OriginalMapImportAccepted _accepted;
        private bool _consumed;

        public PreadmittedOriginalMapImportSource(
            OriginalMapImportRequest expectedRequest,
            OriginalMapImportAccepted accepted)
        {
            _expectedRequest = expectedRequest;
            _accepted = accepted;
        }

        public OriginalMapImportResult Admit(OriginalMapImportRequest request)
        {
            if (_consumed ||
                !string.Equals(
                    request.PackageId,
                    _expectedRequest.PackageId,
                    StringComparison.Ordinal) ||
                request.Profile != _expectedRequest.Profile ||
                !string.Equals(
                    request.ExpectedContentDigest,
                    _expectedRequest.ExpectedContentDigest,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The pre-admitted private canonical import can be consumed exactly once by its accepted request.");
            }

            _consumed = true;
            return _accepted;
        }
    }
}

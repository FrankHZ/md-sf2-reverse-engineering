using Godot;
using Sf2.Remake.Application.Sessions;

namespace Sf2.Remake.GodotAdapter;

internal sealed record PrivateMap3PresentationPlan(
    string BannerText,
    string ExplanationText,
    string InitialStatus,
    bool IncludeTraversalViewport,
    bool ShowTraversalViewport,
    bool IncludeBaseVisualViewport,
    PrivateMap3WorldTreatment WorldTreatment,
    bool StaticOverlayDiagnostic,
    float StatusY)
{
    private const string DiagnosticExplanation =
        "Project-authored traversal diagnostics from accepted Domain policy. " +
        "Original presentation remains unavailable.";

    private const string BaseVisualExplanation =
        "Project-authored base composition from admitted private Map 3 data. " +
        "Not full original fidelity.";

    private const string StaticOverlayExplanation =
        "Project-authored STATIC OVERLAY DIAGNOSTIC using the admitted map palette. " +
        "Not gameplay or original layer-2 fidelity.";

    internal static PrivateMap3PresentationPlan PrivateLocalAvailable() =>
        new(
            Map3Root.PrivateBannerText,
            DiagnosticExplanation,
            "Admitting PrivateLocal canonical Map 3...",
            IncludeTraversalViewport: true,
            ShowTraversalViewport: true,
            IncludeBaseVisualViewport: false,
            PrivateMap3WorldTreatment.ExactNearest,
            StaticOverlayDiagnostic: false,
            StatusY: 450);

    internal static PrivateMap3PresentationPlan PrivateLocalWithBaseVisual(
        PrivateMap3WorldTreatment worldTreatment = PrivateMap3WorldTreatment.ExactNearest,
        bool staticOverlayDiagnostic = false) =>
        new(
            Map3Root.PrivateBannerText,
            staticOverlayDiagnostic ? StaticOverlayExplanation : BaseVisualExplanation,
            "Admitting PrivateLocal canonical Map 3...",
            IncludeTraversalViewport: true,
            ShowTraversalViewport: false,
            IncludeBaseVisualViewport: true,
            worldTreatment,
            staticOverlayDiagnostic,
            StatusY: 310);

    internal static PrivateMap3PresentationPlan PrivateLocalUnavailable(
        string diagnostic) =>
        Unavailable(Map3Root.PrivateBannerText, diagnostic);

    internal static PrivateMap3PresentationPlan ProfileUnavailable(
        string diagnostic) =>
        Unavailable("PROFILE UNAVAILABLE — NO FALLBACK", diagnostic);

    private static PrivateMap3PresentationPlan Unavailable(
        string bannerText,
        string diagnostic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(diagnostic);
        return new(
            bannerText,
            DiagnosticExplanation,
            $"Unavailable: {diagnostic}",
            IncludeTraversalViewport: false,
            ShowTraversalViewport: false,
            IncludeBaseVisualViewport: false,
            PrivateMap3WorldTreatment.ExactNearest,
            StaticOverlayDiagnostic: false,
            StatusY: 105);
    }

    internal static string FormatStatus(
        PrivateOriginalMapSessionSnapshot snapshot,
        string outcome)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);
        return
            $"Map {snapshot.Map}  Tile ({snapshot.PlayerPosition.X}, " +
            $"{snapshot.PlayerPosition.Y})  Area {snapshot.CurrentArea.OneBasedRecordOrdinal}  " +
            $"Step {snapshot.SimulationStep}  {outcome}  |  " +
            "WASD semantic movement";
    }
}

internal sealed class PrivateMap3Presenter
{
    private static readonly Vector2 ViewportPosition = new Vector2(24, 105);

    internal const float StatusX = 24;
    internal const float StatusRightGap = 24;
    internal static readonly Vector2 StatusSize = new(
        PublicSyntheticBattlePresenter.PanelBounds.Position.X - StatusX - StatusRightGap,
        96);
    internal const TextServer.AutowrapMode StatusAutowrapMode =
        TextServer.AutowrapMode.WordSmart;

    private readonly PrivateOriginalMapBaseViewport? _baseViewport;
    private readonly PrivateOriginalMapTraversalViewport? _viewport;
    private readonly Label _status;
    private readonly PrivateMap3WorldTreatment _requestedWorldTreatment;
    private readonly bool _staticOverlayDiagnostic;

    private PrivateMap3Presenter(
        PrivateOriginalMapBaseViewport? baseViewport,
        PrivateOriginalMapTraversalViewport? viewport,
        Label status,
        PrivateMap3WorldTreatment requestedWorldTreatment,
        bool staticOverlayDiagnostic)
    {
        _baseViewport = baseViewport;
        _viewport = viewport;
        _status = status;
        _requestedWorldTreatment = requestedWorldTreatment;
        _staticOverlayDiagnostic = staticOverlayDiagnostic;
    }

    internal PrivateOriginalMapTraversalViewProjection? Projection =>
        _viewport?.Projection;

    internal PrivateOriginalMapBaseViewProjection? BaseProjection =>
        _baseViewport?.Projection;

    internal bool ExpectsBaseProjection => _baseViewport is not null;

    internal bool UsesLocalBaseAtlas => _baseViewport?.UsesLocalAtlas == true;

    internal bool UsesRequiredBaseAtlasSampling =>
        _baseViewport?.UsesRequiredTextureSampling == true;

    internal string? BaseAtlasAssetId => _baseViewport?.AtlasAssetId;

    internal int? BaseAtlasScale => _baseViewport?.AtlasScale;

    internal string? BaseAtlasBucketDigest => _baseViewport?.AtlasBucketDigest;

    internal bool UsesLocalPlayerReference =>
        _baseViewport?.UsesLocalPlayerReference == true;

    internal string? PlayerReferenceAssetId =>
        _baseViewport?.PlayerReferenceAssetId;

    internal int? PlayerReferenceScale =>
        _baseViewport?.PlayerReferenceScale;

    internal string? PlayerReferenceBucketDigest =>
        _baseViewport?.PlayerReferenceBucketDigest;

    internal bool UsesLocalPlayerLocomotion =>
        _baseViewport?.UsesLocalPlayerLocomotion == true;

    internal int? PlayerLocomotionScale =>
        _baseViewport?.PlayerLocomotionScale;

    internal PrivateOriginalMapPlayerLocomotionSnapshot? PlayerLocomotion =>
        _baseViewport?.PlayerLocomotion;

    internal PrivateMap3WorldTreatment WorldTreatment =>
        _baseViewport?.WorldTreatment ?? PrivateMap3WorldTreatment.ExactNearest;

    internal static PrivateMap3Presenter Attach(
        Node2D parent,
        PrivateMap3PresentationPlan plan)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(plan);

        Label banner = new()
        {
            Text = plan.BannerText,
            Position = new Vector2(24, 18),
        };
        banner.AddThemeFontSizeOverride("font_size", 24);
        banner.AddThemeColorOverride("font_color", new Color("ff8f70"));
        parent.AddChild(banner);

        Label explanation = new()
        {
            Text = plan.ExplanationText,
            Position = new Vector2(24, 55),
        };
        explanation.AddThemeFontSizeOverride("font_size", 16);
        parent.AddChild(explanation);

        PrivateOriginalMapTraversalViewport? viewport = null;
        if (plan.IncludeTraversalViewport)
        {
            viewport = new PrivateOriginalMapTraversalViewport
            {
                Position = ViewportPosition,
                Visible = plan.ShowTraversalViewport,
            };
            parent.AddChild(viewport);
        }

        PrivateOriginalMapBaseViewport? baseViewport = null;
        if (plan.IncludeBaseVisualViewport)
        {
            baseViewport = new PrivateOriginalMapBaseViewport
            {
                Position = ViewportPosition,
            };
            parent.AddChild(baseViewport);
        }

        Label status = new()
        {
            Text = plan.InitialStatus,
            Position = new Vector2(24, plan.StatusY),
            Size = StatusSize,
            AutowrapMode = StatusAutowrapMode,
            ClipText = true,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        status.AddThemeFontSizeOverride("font_size", 18);
        parent.AddChild(status);

        return new PrivateMap3Presenter(
            baseViewport,
            viewport,
            status,
            plan.WorldTreatment,
            plan.StaticOverlayDiagnostic);
    }

    internal bool TryBindBaseAtlas(
        PrivateLocalPresentationRasterMount mount,
        PrivateOriginalMapSessionSnapshot snapshot,
        out PrivateLocalPresentationAssetMountDiagnostic? diagnostic)
    {
        ArgumentNullException.ThrowIfNull(mount);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (_baseViewport is null)
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The private presentation plan did not request a base visual viewport.");
            return false;
        }

        return _baseViewport.TryBindLocalAtlas(
            mount,
            snapshot,
            _requestedWorldTreatment,
            _staticOverlayDiagnostic,
            out diagnostic);
    }

    internal bool TryBindPlayerReference(
        PrivateLocalPresentationRasterMount mount,
        out PrivateLocalPresentationAssetMountDiagnostic? diagnostic)
    {
        ArgumentNullException.ThrowIfNull(mount);
        if (_baseViewport is null)
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The private presentation plan did not request a base visual viewport.");
            return false;
        }

        return _baseViewport.TryBindLocalPlayerReference(mount, out diagnostic);
    }

    internal bool TryBindPlayerLocomotion(
        PrivateLocalPlayerLocomotionMount mount,
        out PrivateLocalPresentationAssetMountDiagnostic? diagnostic)
    {
        ArgumentNullException.ThrowIfNull(mount);
        if (_baseViewport is null)
        {
            diagnostic = new PrivateLocalPresentationAssetMountDiagnostic(
                PrivateLocalPresentationAssetMountFailureCode.InvalidBinding,
                "The private presentation plan did not request a base visual viewport.");
            return false;
        }

        return _baseViewport.TryBindLocalPlayerLocomotion(mount, out diagnostic);
    }

    internal void Project(
        PrivateOriginalMapSessionSnapshot snapshot,
        string outcome,
        PrivateOriginalMapPlayerLocomotionSnapshot? playerLocomotion = null)
    {
        _viewport?.Project(snapshot);
        if (_baseViewport is not null && _baseViewport.UsesLocalAtlas)
        {
            _baseViewport.ProjectMountedAtlas(
                snapshot,
                _staticOverlayDiagnostic,
                playerLocomotion);
        }

        _status.Text = PrivateMap3PresentationPlan.FormatStatus(snapshot, outcome);
    }

    internal void ProjectStatus(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _status.Text = $"Unavailable: {message}";
    }
}

using Godot;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;

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
    bool CurrentAreaOverlay,
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

    private const string CurrentAreaOverlayExplanation =
        "Project-authored current-area second-layer composition from admitted private Map 3 data. " +
        "Not original layer priority, timing, or full fidelity.";

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
            CurrentAreaOverlay: false,
            StatusY: 450);

    internal static PrivateMap3PresentationPlan PrivateLocalWithBaseVisual(
        PrivateMap3WorldTreatment worldTreatment = PrivateMap3WorldTreatment.ExactNearest,
        bool staticOverlayDiagnostic = false,
        bool currentAreaOverlay = false)
    {
        if (staticOverlayDiagnostic && currentAreaOverlay)
        {
            throw new ArgumentException(
                "Static and current-area overlay presentation modes are mutually exclusive.");
        }

        return new(
            Map3Root.PrivateBannerText,
            staticOverlayDiagnostic
                ? StaticOverlayExplanation
                : currentAreaOverlay
                    ? CurrentAreaOverlayExplanation
                    : BaseVisualExplanation,
            "Admitting PrivateLocal canonical Map 3...",
            IncludeTraversalViewport: true,
            ShowTraversalViewport: false,
            IncludeBaseVisualViewport: true,
            worldTreatment,
            staticOverlayDiagnostic,
            currentAreaOverlay,
            StatusY: 310);
    }

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
            CurrentAreaOverlay: false,
            StatusY: 105);
    }

    internal static string FormatStatus(
        PrivateOriginalMapSessionSnapshot snapshot,
        string outcome,
        PrivateOriginalMapPlayerLocomotionSnapshot? playerLocomotion = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);
        string status =
            $"Map {snapshot.Map}  Tile ({snapshot.PlayerPosition.X}, " +
            $"{snapshot.PlayerPosition.Y})  Area {snapshot.CurrentArea.OneBasedRecordOrdinal}  " +
            $"Step {snapshot.SimulationStep}  {outcome}  |  " +
            "WASD semantic movement";
        if (snapshot.Map.Value == "map19" && snapshot.PalaceFirstVisit is not null)
        {
            // Put the current action before potentially long diagnostics in the clipped status label.
            return AstralStatus(snapshot, playerLocomotion?.OpaqueFacing, playerLocomotion?.IsMoving == true) +
                "\n" + status;
        }

        if (snapshot.Map.Value == "map20")
        {
            return status + "  |  " + PalaceFirstVisitStatus(snapshot.PalaceFirstVisit);
        }

        PrivateOriginalMapZone601State? zone601 = snapshot.Zone601;
        if (zone601?.Flag601Set == true)
        {
            MapPosition ambientCenter = zone601.AmbientCenter!;
            status +=
                $"  |  Zone601 complete; actor {zone601.LogicalActorId} at " +
                $"({zone601.ActorPosition.X}, {zone601.ActorPosition.Y}), facing " +
                $"{zone601.ActorOpaqueFacing}; ambient center " +
                $"({ambientCenter.X}, {ambientCenter.Y}) range " +
                $"{zone601.AmbientRange}; random choices Unknown";
        }

        PrivateOriginalMapSarahState? sarah = snapshot.Sarah;
        if (sarah is not null)
        {
            status +=
                $"  |  Sarah {sarah.Phase}; actor {sarah.LogicalActorId} at " +
                $"({sarah.ActorPosition.X}, {sarah.ActorPosition.Y}), facing " +
                $"{sarah.ActorOpaqueFacing}; temporary route flag " +
                $"{(sarah.TemporaryRouteFlag256Set ? "set" : "clear")}; " +
                SarahAction(sarah);
        }

        PrivateOriginalMapEntity142State? entity142 = snapshot.Entity142;
        if (entity142 is not null)
        {
            string pending = snapshot.PendingEntity142 is { } request
                ? $"pending request {request.RequestSequence}"
                : "no pending request";
            status +=
                $"  |  Entity142 logical {entity142.LogicalActorId}/slot " +
                $"{entity142.PhysicalActorSlot} at ({entity142.ActorPosition.X}, " +
                $"{entity142.ActorPosition.Y}), facing {entity142.ActorOpaqueFacing}; " +
                $"flags261/602 {(entity142.Flag261Set ? "set" : "clear")}; " +
                Entity142Action(entity142, pending);
        }

        if (snapshot.MessengerAcceptance?.Accepted == true)
        {
            status +=
                "  |  Messenger accepted; flags600/66/603 set; " +
                "Sarah/Chester follower-ready; prose/audio/timing Unknown";
        }

        if (snapshot.CastleGate is { } castleGate)
        {
            status += "  |  " + CastleGateStatus(castleGate);
        }

        return status;
    }

    internal static string AstralStatus(
        PrivateOriginalMapSessionSnapshot snapshot, byte? facing, bool moving)
    {
        if (snapshot.AstralAcceptance is not null)
        {
            return "Astral has left; passage open. Controlled result; scene skipped.";
        }

        if (snapshot.AstralOccupiesRouteTile)
        {
            return !moving && facing is byte direction && snapshot.CanAcceptAstral(direction)
                ? "F accept Astral's invitation (controlled result; skip scene)"
                : "Find Astral and face him to continue toward the tower.";
        }

        return "Astral interaction unavailable.";
    }

    internal static string RoyalReturnStatus(PrivateOriginalMapPalaceFirstVisitReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        return "Royal return; controlled first-visit result retained; diagnostic traversal";
    }

    internal static string PalaceFirstVisitStatus(PrivateOriginalMapPalaceFirstVisitReceipt? receipt) =>
        receipt is null
            ? "F apply controlled first-visit result at palace entrance (skip scene); result unselected"
            : "Controlled first-visit result applied; scene skipped";

    internal static string CastleGateStatus(PrivateOriginalMapCastleGateState castleGate)
    {
        ArgumentNullException.ThrowIfNull(castleGate);
        if (!castleGate.Opened)
        {
            return "Castle gate closed; bounded event ready only after messenger acceptance";
        }

        return "Castle gate open; flag604 set; bounded opening admission complete; " +
            "source dialogue/facing/restoration, timing, and presentation Unknown";
    }

    internal static string SarahAction(PrivateOriginalMapSarahState sarah)
    {
        ArgumentNullException.ThrowIfNull(sarah);
        return sarah.IsMessengerFollowerReady
            ? "follower ready; route occupancy released"
            : "F semantic interaction request";
    }

    internal static string Entity142Action(
        PrivateOriginalMapEntity142State entity142,
        string pending)
    {
        ArgumentNullException.ThrowIfNull(entity142);
        ArgumentException.ThrowIfNullOrWhiteSpace(pending);
        return entity142.RouteOccupancyReleased
            ? "route occupancy released; immutable sprite diagnostic only"
            : $"{pending}; F request / G acknowledge";
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
    private readonly bool _currentAreaOverlay;
    private readonly bool _showTraversalOnInitialMap;

    private PrivateMap3Presenter(
        PrivateOriginalMapBaseViewport? baseViewport,
        PrivateOriginalMapTraversalViewport? viewport,
        Label status,
        PrivateMap3WorldTreatment requestedWorldTreatment,
        bool staticOverlayDiagnostic,
        bool currentAreaOverlay,
        bool showTraversalOnInitialMap)
    {
        _baseViewport = baseViewport;
        _viewport = viewport;
        _status = status;
        _requestedWorldTreatment = requestedWorldTreatment;
        _staticOverlayDiagnostic = staticOverlayDiagnostic;
        _currentAreaOverlay = currentAreaOverlay;
        _showTraversalOnInitialMap = showTraversalOnInitialMap;
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

    internal bool UsesEntity142Diagnostic =>
        _baseViewport?.UsesEntity142Diagnostic == true;

    internal PrivateMap3Entity142DiagnosticProjection? Entity142DiagnosticProjection =>
        _baseViewport?.Entity142DiagnosticProjection;

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
            plan.StaticOverlayDiagnostic,
            plan.CurrentAreaOverlay,
            plan.ShowTraversalViewport);
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
            _currentAreaOverlay,
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

    internal bool TryBindEntity142Diagnostic(
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

        return _baseViewport.TryBindLocalEntity142Diagnostic(
            mount,
            snapshot,
            out diagnostic);
    }

    internal void Project(
        PrivateOriginalMapSessionSnapshot snapshot,
        string outcome,
        PrivateOriginalMapPlayerLocomotionSnapshot? playerLocomotion = null)
    {
        (bool traversalVisible, bool baseVisible) = ProjectionVisibility(
            snapshot.Map,
            snapshot.Definition.Map,
            _baseViewport is not null,
            _showTraversalOnInitialMap);
        if (_viewport is not null)
        {
            _viewport.Visible = traversalVisible;
        }

        if (_baseViewport is not null)
        {
            _baseViewport.Visible = baseVisible;
        }

        _viewport?.Project(snapshot);
        if (baseVisible && _baseViewport is not null && _baseViewport.UsesLocalAtlas)
        {
            _baseViewport.ProjectMountedAtlas(
                snapshot,
                _staticOverlayDiagnostic,
                playerLocomotion,
                _currentAreaOverlay);
        }

        _status.Text = PrivateMap3PresentationPlan.FormatStatus(snapshot, outcome, playerLocomotion);
    }

    internal static (bool TraversalVisible, bool BaseVisible) ProjectionVisibility(
        MapId currentMap,
        MapId initialMap,
        bool hasBaseViewport,
        bool showTraversalOnInitialMap)
    {
        ArgumentNullException.ThrowIfNull(currentMap);
        ArgumentNullException.ThrowIfNull(initialMap);
        bool isInitialMap = currentMap == initialMap;
        return (
            TraversalVisible: !isInitialMap || showTraversalOnInitialMap,
            BaseVisible: hasBaseViewport && isInitialMap);
    }

    internal void ProjectStatus(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _status.Text = $"Unavailable: {message}";
    }
}

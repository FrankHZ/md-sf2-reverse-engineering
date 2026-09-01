using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;

namespace Sf2.Remake.GodotAdapter;

internal sealed record PrivateMap3PresentationPlan(
    string BannerText,
    string ExplanationText,
    string InitialStatus,
    bool IncludeTraversalViewport,
    bool ShowTraversalViewport,
    bool IncludeBaseVisualViewport,
    float StatusY)
{
    private const string DiagnosticExplanation =
        "Project-authored traversal diagnostics from accepted Domain policy. " +
        "Original presentation remains unavailable.";

    private const string BaseVisualExplanation =
        "Project-authored base composition from admitted private Map 3 data. " +
        "Not full original fidelity.";

    internal static PrivateMap3PresentationPlan PrivateLocalAvailable() =>
        new(
            Map3Root.PrivateBannerText,
            DiagnosticExplanation,
            "Admitting PrivateLocal canonical Map 3...",
            IncludeTraversalViewport: true,
            ShowTraversalViewport: true,
            IncludeBaseVisualViewport: false,
            StatusY: 450);

    internal static PrivateMap3PresentationPlan PrivateLocalWithBaseVisual() =>
        new(
            Map3Root.PrivateBannerText,
            BaseVisualExplanation,
            "Admitting PrivateLocal canonical Map 3...",
            IncludeTraversalViewport: true,
            ShowTraversalViewport: false,
            IncludeBaseVisualViewport: true,
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

    private readonly PrivateOriginalMapBaseViewport? _baseViewport;
    private readonly PrivateOriginalMapTraversalViewport? _viewport;
    private readonly Label _status;
    private OriginalMapVisualPayloadDefinition? _visualDefinition;

    private PrivateMap3Presenter(
        PrivateOriginalMapBaseViewport? baseViewport,
        PrivateOriginalMapTraversalViewport? viewport,
        Label status)
    {
        _baseViewport = baseViewport;
        _viewport = viewport;
        _status = status;
    }

    internal PrivateOriginalMapTraversalViewProjection? Projection =>
        _viewport?.Projection;

    internal PrivateOriginalMapBaseViewProjection? BaseProjection =>
        _baseViewport?.Projection;

    internal bool ExpectsBaseProjection => _baseViewport is not null;

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
        };
        status.AddThemeFontSizeOverride("font_size", 18);
        parent.AddChild(status);

        return new PrivateMap3Presenter(baseViewport, viewport, status);
    }

    internal void BindVisualDefinition(OriginalMapVisualPayloadDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (_baseViewport is null)
        {
            throw new InvalidOperationException(
                "The private presentation plan did not request a base visual viewport.");
        }

        _visualDefinition = definition;
    }

    internal void Project(
        PrivateOriginalMapSessionSnapshot snapshot,
        string outcome)
    {
        _viewport?.Project(snapshot);
        if (_baseViewport is not null && _visualDefinition is not null)
        {
            _baseViewport.Project(snapshot, _visualDefinition);
        }

        _status.Text = PrivateMap3PresentationPlan.FormatStatus(snapshot, outcome);
    }

    internal void ProjectStatus(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _status.Text = $"Unavailable: {message}";
    }
}

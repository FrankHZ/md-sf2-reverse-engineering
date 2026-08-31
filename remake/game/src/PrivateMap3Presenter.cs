using Godot;
using Sf2.Remake.Application.Sessions;

namespace Sf2.Remake.GodotAdapter;

internal sealed record PrivateMap3PresentationPlan(
    string BannerText,
    string ExplanationText,
    string InitialStatus,
    bool IncludeTraversalViewport,
    float StatusY)
{
    private const string Explanation =
        "Project-authored traversal diagnostics from accepted Domain policy. " +
        "Original presentation remains unavailable.";

    internal static PrivateMap3PresentationPlan PrivateLocalAvailable() =>
        new(
            Map3Root.PrivateBannerText,
            Explanation,
            "Admitting PrivateLocal canonical Map 3...",
            IncludeTraversalViewport: true,
            StatusY: 450);

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
            Explanation,
            $"Unavailable: {diagnostic}",
            IncludeTraversalViewport: false,
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
    private readonly PrivateOriginalMapTraversalViewport? _viewport;
    private readonly Label _status;

    private PrivateMap3Presenter(
        PrivateOriginalMapTraversalViewport? viewport,
        Label status)
    {
        _viewport = viewport;
        _status = status;
    }

    internal PrivateOriginalMapTraversalViewProjection? Projection =>
        _viewport?.Projection;

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
                Position = new Vector2(24, 105),
            };
            parent.AddChild(viewport);
        }

        Label status = new()
        {
            Text = plan.InitialStatus,
            Position = new Vector2(24, plan.StatusY),
        };
        status.AddThemeFontSizeOverride("font_size", 18);
        parent.AddChild(status);

        return new PrivateMap3Presenter(viewport, status);
    }

    internal void Project(
        PrivateOriginalMapSessionSnapshot snapshot,
        string outcome)
    {
        _viewport?.Project(snapshot);
        _status.Text = PrivateMap3PresentationPlan.FormatStatus(snapshot, outcome);
    }

    internal void ProjectStatus(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _status.Text = $"Unavailable: {message}";
    }
}

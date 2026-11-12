using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.Maps;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Passive disposable field projection. No hit targets, commands, story or navigation state.
internal sealed partial class FieldGuidanceView : Control
{
    private readonly Panel _panel = new() { Name = "ObjectivePanel", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _label = FieldServiceMenu.Label("Destination", StoryTheme.BodySize);
    private readonly Label _anchorLabel = FieldServiceMenu.Label("Entrance", 24);
    private FieldGuidance? _guidance;
    private Vector2? _marker;
    internal static string Objective(GameSession session, FieldGuidance guidance) =>
        (guidance.Companion is { } companion
            ? GameText.Get(guidance.ObjectiveKey, DisplayNames.Member(session.Definition, session.Current.Story, companion))
            : GameText.Get(guidance.ObjectiveKey)) +
        "\n" + GameText.Get(guidance.DestinationKey) + "\n" + GameText.Get(guidance.RouteKey);

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore; Theme = StoryTheme.Create();
        _anchorLabel.CustomMinimumSize = new(360, 0); _anchorLabel.CustomMaximumSize = new(360, -1);
        AddChild(_panel); _panel.AddChild(_label); AddChild(_anchorLabel); Hide();
    }

    internal void Present(GameSession session, FieldGuidance? guidance, WorldViewportLayout viewport,
        ExplorationState? field, bool show, Rect2? commands, bool showMarker)
    {
        _guidance = guidance; _marker = null; _anchorLabel.Hide();
        Visible = show && guidance is not null && field is not null && Size.X >= 1920 && Size.Y >= 1080;
        if (!Visible) return;
        _label.Text = Objective(session, guidance!);
        var font = _label.GetThemeFont("font");
        float width = Math.Min(448, _label.Text.Split('\n').Max(line =>
            font.GetStringSize(line, fontSize: StoryTheme.BodySize).X));
        _label.CustomMinimumSize = new(width, 0); _label.CustomMaximumSize = new(width, -1);
        _label.Position = new(16, 12); _label.Size = new(width, 0);
        _panel.Size = new(width + 32, _label.GetMinimumSize().Y + 24);
        var player = field!.PlayerEntity;
        var playerRect = new Rect2((new Vector2(player.Motion.X, player.Motion.Y) / 16 - viewport.Origin) * 4, new(96, 96));
        // Keep the same preferred edge across root-menu toggles; avoid its actual arranged bounds.
        var corners = new[] { new Vector2(32, 112), new Vector2(32, Size.Y - _panel.Size.Y - 112),
            new Vector2(Size.X - _panel.Size.X - 32, 112), Size - _panel.Size - new Vector2(32, 112) };
        foreach (var corner in corners)
        {
            var rect = new Rect2(corner, _panel.Size);
            if (rect.Intersects(playerRect.Grow(24)) || commands is { } menu && rect.Intersects(menu.Grow(24))) continue;
            _panel.Position = corner; break;
        }
        if (showMarker && guidance!.Anchor is { } anchor)
        {
            var cell = new Rect2((new Vector2(anchor.Position.X, anchor.Position.Y) * 24 - viewport.Origin) * 4, new(96, 96));
            // Only the currently drawn public entrance, never an off-camera compass or remote coordinate.
            if (viewport.Screen.Encloses(cell) && !cell.Intersects(_panel.GetRect()))
            {
                _anchorLabel.Text = GameText.Get(anchor.LabelKey); _anchorLabel.Size = new(360, 0);
                _anchorLabel.Position = new(Math.Clamp(cell.End.X + 12, 32, Size.X - 392), Math.Clamp(cell.Position.Y, 112, Size.Y - 160));
                var labelRect = _anchorLabel.GetRect();
                if (new Rect2(new(32, 112), Size - new Vector2(64, 224)).Encloses(labelRect) &&
                    !labelRect.Intersects(_panel.GetRect()) && !labelRect.Intersects(playerRect))
                { _marker = cell.GetCenter(); _anchorLabel.Show(); }
            }
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_marker is { } marker) DrawArc(marker, 16, 0, Mathf.Tau, 40, new Color("d4dfef"), 3, true);
    }

    internal object Observe() => new { visible = IsVisibleInTree(), sessionId = _guidance?.SessionId,
        objectiveKey = _guidance?.ObjectiveKey, destinationKey = _guidance?.DestinationKey, routeKey = _guidance?.RouteKey,
        revision = _guidance?.Revision, map = _guidance?.Map.Value, text = _label.Text,
        rect = FieldServiceMenu.Rect(_panel), panelId = _panel.GetInstanceId(), textRect = FieldServiceMenu.Rect(_label),
        fontSize = _label.GetThemeFontSize("font_size"), lines = _label.GetLineCount(),
        markerVisible = IsVisibleInTree() && _marker is not null,
        marker = _marker is { } p ? new { x = p.X, y = p.Y } : null,
        anchorLabel = _anchorLabel.IsVisibleInTree() ? _anchorLabel.Text : null,
        anchorRect = FieldServiceMenu.Rect(_anchorLabel), mouseFilter = MouseFilter.ToString() };
}

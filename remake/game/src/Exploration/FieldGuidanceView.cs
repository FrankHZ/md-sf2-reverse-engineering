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
    private readonly Panel _panel = new() { MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _label = FieldServiceMenu.Label("Destination", 24);
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
        _label.CustomMinimumSize = new(608, 0); _label.CustomMaximumSize = new(608, -1);
        _anchorLabel.CustomMinimumSize = new(360, 0); _anchorLabel.CustomMaximumSize = new(360, -1);
        AddChild(_panel); _panel.AddChild(_label); AddChild(_anchorLabel); Hide();
    }

    internal void Present(FieldGuidance? guidance, WorldViewportLayout viewport, ExplorationState? field, bool show)
    {
        _guidance = guidance; _marker = null; _anchorLabel.Hide();
        Visible = show && guidance is not null && field is not null && Size.X >= 1920 && Size.Y >= 1080;
        if (!Visible) return;
        _label.Text = GameText.Get(guidance!.DestinationKey) + "\n" + GameText.Get(guidance.RouteKey);
        _label.Position = new(16, 12); _label.Size = new(608, 0);
        _panel.Size = new(640, _label.GetMinimumSize().Y + 24); _panel.Position = new(32, 112);
        var player = field!.PlayerEntity;
        var playerRect = new Rect2((new Vector2(player.Motion.X, player.Motion.Y) / 16 - viewport.Origin) * 4, new(96, 96));
        if (_panel.GetRect().Intersects(playerRect.Grow(24))) _panel.Position = new(32, Size.Y - _panel.Size.Y - 112);
        if (guidance.Anchor is { } anchor)
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
        rect = FieldServiceMenu.Rect(_panel), markerVisible = IsVisibleInTree() && _marker is not null,
        marker = _marker is { } p ? new { x = p.X, y = p.Y } : null,
        anchorLabel = _anchorLabel.IsVisibleInTree() ? _anchorLabel.Text : null,
        anchorRect = FieldServiceMenu.Rect(_anchorLabel), mouseFilter = MouseFilter.ToString() };
}

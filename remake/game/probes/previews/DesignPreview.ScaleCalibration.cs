using Godot;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private bool _scaleReview, _largerType;
    private readonly HashSet<Control> _pendingReview = [];
    private readonly Dictionary<Control, Action> _restoreReview = [];

    private void StartScaleReview(string[] args)
    {
        _scaleReview = args.Contains("--scale-review");
        if (!_scaleReview) return;
        _largerType = args.Contains("--larger");
        GetNode<Control>("PreviewToolbar").Hide();
        GetTree().NodeAdded += TrackReviewControl;
        GetTree().NodeRemoved += ForgetReviewControl;
        GetWindow().SizeChanged += FitReviewPicture;
        FitReviewPicture();
        ShowReviewTitle();
    }

    private void FitReviewPicture()
    {
        var window = GetWindow();
        var mode = window.Size.X < 1920 || window.Size.Y < 1080
            ? Window.ContentScaleModeEnum.CanvasItems : Window.ContentScaleModeEnum.Disabled;
        if (window.ContentScaleMode != mode) window.ContentScaleMode = mode;
        _game.Size = (Vector2I)GetViewportRect().Size;
        var picture = GetNode<TextureRect>("ProductionPicture");
        picture.Position = Vector2.Zero;
        picture.Size = _game.Size;
    }

    private void ShowReviewTitle() => GetWindow().Title =
        $"SF2 · {(_largerType ? "大字 / Larger" : "标准 / Standard")} · U 对比 / Compare · F2 Dialogue · F4 Battle · F5 Members · F10 System · L 中文/English";

    private static Control? ReviewFamily(Control child)
    {
        for (Node? node = child; node is not null && node is not Viewport; node = node.GetParent())
            if (node is StoryDialoguePanel { LayoutRegion: null } or BattleCommandMenu or MemberPageView or SettingsView)
                return (Control)node;
        return null;
    }

    private void TrackReviewControl(Node node)
    {
        if (_largerType && node is Control control && control.GetViewport() == _game)
            _pendingReview.Add(control);
    }

    private void ForgetReviewControl(Node node)
    {
        if (node is not Control control) return;
        _pendingReview.Remove(control);
        if (_restoreReview.Remove(control, out var restore)) restore();
    }

    private void ToggleReview()
    {
        _largerType = !_largerType;
        if (_largerType)
        {
            foreach (var control in _game.FindChildren("*", "Control", owned: false).OfType<Control>())
                _pendingReview.Add(control);
        }
        else
        {
            _pendingReview.Clear();
            foreach (var restore in _restoreReview.Values.ToArray()) restore();
            _restoreReview.Clear();
        }
        ShowReviewTitle();
    }

    // Read each control's actual theme after the presenter has finished mounting its children.
    // New rows and settings subpages take the same path as the first screen.
    private void ReviewLayout()
    {
        if (_pendingReview.Count == 0) return;
        var pending = _pendingReview.ToArray();
        _pendingReview.Clear();
        foreach (var control in pending)
        {
            if (!control.IsInsideTree() || _restoreReview.ContainsKey(control)) continue;
            var family = ReviewFamily(control);
            if (family is null) continue;
            if (control is Label or Button)
                EnlargeText(control, family);
            else if (family is MemberPageView && control is VBoxContainer && control.GetParent() == family)
            {
                // Give the existing name, inventory and reader columns a little more width.
                float left = control.OffsetLeft, right = control.OffsetRight;
                control.OffsetLeft = left - 48;
                control.OffsetRight = right + 48;
                _restoreReview[control] = () => { control.OffsetLeft = left; control.OffsetRight = right; };
            }
        }
    }

    private void EnlargeText(Control control, Control family)
    {
        int original = control.GetThemeFontSize("font_size");
        bool localSize = control.HasThemeFontSizeOverride("font_size");
        // Primary reading and choices grow most. Keep peripheral battlefield information quiet.
        bool quiet = family is BattleCommandMenu && control.Name.ToString() is "Context" or "Help" or "TerrainReading";
        int size = Mathf.RoundToInt(original * (quiet ? 1.08f : 1.2f));
        control.AddThemeFontSizeOverride("font_size", size);

        bool body = family is StoryDialoguePanel dialogue && control == dialogue.Body;
        int spacing = control.GetThemeConstant("line_spacing");
        bool localSpacing = control.HasThemeConstantOverride("line_spacing");
        if (body) control.AddThemeConstantOverride("line_spacing", spacing + 4);
        var minimum = control.CustomMinimumSize;
        bool selector = family is MemberPageView && control.Name == "MemberSelector";
        bool slotLabel = family is MemberPageView && control is Label &&
            control.GetParent() is HBoxContainer row && row.Name.ToString().StartsWith("Slot", StringComparison.Ordinal) &&
            control.Name.ToString().StartsWith("Entry", StringComparison.Ordinal);
        if (selector) control.CustomMinimumSize = new(minimum.X + 112, minimum.Y);
        if (slotLabel) control.CustomMinimumSize = new(minimum.X, 80);

        _restoreReview[control] = () =>
        {
            if (localSize) control.AddThemeFontSizeOverride("font_size", original);
            else control.RemoveThemeFontSizeOverride("font_size");
            if (body)
            {
                if (localSpacing) control.AddThemeConstantOverride("line_spacing", spacing);
                else control.RemoveThemeConstantOverride("line_spacing");
            }
            if (selector || slotLabel) control.CustomMinimumSize = minimum;
        };
    }

    public override void _ExitTree()
    {
        if (!_scaleReview) return;
        GetWindow().SizeChanged -= FitReviewPicture;
        GetTree().NodeAdded -= TrackReviewControl;
        GetTree().NodeRemoved -= ForgetReviewControl;
    }
}

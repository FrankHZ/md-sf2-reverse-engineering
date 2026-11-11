using Godot;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.UI;

// Disposable authored focus/source selection projection. The caller owns phase and release admission.
internal sealed partial class StoryChoiceRow : HBoxContainer
{
    private bool? _focusedYes;
    internal bool JourneyStyle { get; set; }
    private StyleBoxFlat? _normalStyle, _selectedStyle;
    internal bool SelectedYes => _focusedYes ?? true;
    internal Action InvalidatePointer { get; set; } = () => { };
    internal Label Yes { get; } = new() { Name = "Yes", HorizontalAlignment = HorizontalAlignment.Center,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
    internal Label No { get; } = new() { Name = "No", HorizontalAlignment = HorizontalAlignment.Center,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore; AddChild(Yes); AddChild(No);
        Yes.ItemRectChanged += () => InvalidatePointer();
        No.ItemRectChanged += () => InvalidatePointer();
        ItemRectChanged += () => InvalidatePointer();
    }
    internal void Focus(bool yes) => _focusedYes = yes;
    internal void ResetFocus() => _focusedYes = null;
    internal void Present(bool visible, bool selectedYes, bool selectable, bool source = false)
    {
        if (!visible) _focusedYes = null;
        if (!source) selectedYes = _focusedYes ?? selectedYes;
        Visible = visible;
        Yes.Text = (selectable && selectedYes ? "> " : "") + GameText.Get("story.yes");
        No.Text = (selectable && !selectedYes ? "> " : "") + GameText.Get("story.no");
        Yes.Modulate = selectable && selectedYes ? StoryTheme.Selected : Colors.White;
        No.Modulate = selectable && !selectedYes ? StoryTheme.Selected : Colors.White;
        if (JourneyStyle)
        {
            _normalStyle ??= ChoiceStyle(false); _selectedStyle ??= ChoiceStyle(true);
            AddThemeConstantOverride("separation", 12);
            Decorate(Yes, selectable && selectedYes);
            Decorate(No, selectable && !selectedYes);
        }
    }

    private void Decorate(Label label, bool selected)
    {
        // Labels retain the existing pointer hit/commit owner, with stable padded row bounds.
        label.AddThemeStyleboxOverride("normal", selected ? _selectedStyle! : _normalStyle!);
        label.VerticalAlignment = VerticalAlignment.Center;
        label.Modulate = Colors.White;
        label.AddThemeColorOverride("font_color", selected ? StoryTheme.Selected : new Color("f4f5fc"));
        label.Text = label.Text.TrimStart('>', ' ');
        if (selected) label.Text = "› " + label.Text;
    }

    private static StyleBoxFlat ChoiceStyle(bool selected)
    {
        var style = new StyleBoxFlat { BgColor = new(selected ? "273d56" : "102c50"),
            BorderColor = new(selected ? "ffe19a" : "536c87"),
            ContentMarginLeft = 40, ContentMarginRight = 40, ContentMarginTop = 8, ContentMarginBottom = 8 };
        style.SetBorderWidthAll(selected ? 3 : 1);
        return style;
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _normalStyle?.Dispose(); _selectedStyle?.Dispose(); }
        base.Dispose(disposing);
    }

    internal (string? Id, Rect2 Rect) Hit(Vector2 position)
    {
        if (!IsVisibleInTree()) return (null, default);
        foreach (var label in new[] { Yes, No })
            if (label.GetGlobalRect().HasPoint(position)) return (label.Name.ToString(), label.GetGlobalRect());
        return (null, default);
    }
    internal object Observe() => new { yes = Yes.Text, no = No.Text,
        yesRect = Battles.BattleMapViewport.Rectangle(Yes.GetGlobalRect()), noRect = Battles.BattleMapViewport.Rectangle(No.GetGlobalRect()) };
}

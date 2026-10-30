using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

// Disposable authored focus/source selection projection. The caller owns phase and release admission.
internal sealed partial class StoryChoiceRow : HBoxContainer
{
    private bool? _focusedYes;
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
        Yes.Text = selectable && selectedYes ? "▸ Yes" : "Yes";
        No.Text = selectable && !selectedYes ? "▸ No" : "No";
        Yes.Modulate = selectable && selectedYes ? StoryTheme.Selected : Colors.White;
        No.Modulate = selectable && !selectedYes ? StoryTheme.Selected : Colors.White;
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

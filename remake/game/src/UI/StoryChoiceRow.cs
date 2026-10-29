using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

// Pure selection projection. The caller owns choice phase, default and release admission.
internal sealed partial class StoryChoiceRow : HBoxContainer
{
    internal Label Yes { get; } = new() { Name = "Yes", HorizontalAlignment = HorizontalAlignment.Center,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
    internal Label No { get; } = new() { Name = "No", HorizontalAlignment = HorizontalAlignment.Center,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore; AddChild(Yes); AddChild(No);
    }
    internal void Present(bool visible, bool selectedYes, bool selectable)
    {
        Visible = visible;
        Yes.Text = selectable && selectedYes ? "▸ Yes" : "Yes";
        No.Text = selectable && !selectedYes ? "▸ No" : "No";
        Yes.Modulate = selectable && selectedYes ? StoryTheme.Selected : Colors.White;
        No.Modulate = selectable && !selectedYes ? StoryTheme.Selected : Colors.White;
    }
}

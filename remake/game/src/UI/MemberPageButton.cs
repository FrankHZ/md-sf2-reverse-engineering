using Godot;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.UI;

// Native picking and release activation, without taking the reader's focus on mouse-down.
internal sealed partial class MemberPageButton : Button
{
    internal object? Subject { get; set; }

    internal MemberPageButton(MenuPointer gesture, ScrollContainer? clip, Func<object> boundary, Action activate)
    {
        Name = "Pointer";
        Flat = true;
        FocusMode = FocusModeEnum.None;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        SelfModulate = Colors.Transparent;

        bool accepted = false;
        GuiInput += input =>
        {
            if (input is not InputEventMouseButton { ButtonIndex: MouseButton.Left } button) return;
            var bounds = GetGlobalRect();
            if (clip is not null) bounds = bounds.Intersection(clip.GetGlobalRect());
            string? id = bounds.HasPoint(GetGlobalTransform() * button.Position) ? "activate" : null;
            accepted = gesture.Read(button, id, bounds, (this, Subject, boundary())) is not null;
        };
        Pressed += () =>
        {
            if (!accepted) return;
            accepted = false;
            activate();
        };
    }

    // Stack a non-focusing pointer target over the row without changing its measured size.
    internal static MarginContainer Stack(Control content)
    {
        var frame = new MarginContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        frame.AddChild(content);
        return frame;
    }
}

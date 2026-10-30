using Godot;

namespace Sf2.Remake.GodotAdapter.Input;

// A menu-local gesture, never a mapped Confirm. Owners supply the live control and boundary.
internal sealed class MenuPointer
{
    private (string Id, Rect2 Rect, object Boundary, MouseButton Button)? _press;
    private readonly HashSet<MouseButton> _held = [];
    private bool _blocked;
    private Vector2? _position;

    internal void Cancel() => _press = null;
    internal void ReleaseIfNeutral()
    {
        if (Godot.Input.IsMouseButtonPressed(MouseButton.Left) || Godot.Input.IsMouseButtonPressed(MouseButton.Right)) return;
        Cancel(); _held.Clear(); _blocked = false;
    }
    internal void Suspend()
    {
        Cancel();
        _blocked = _held.Count > 0 || Godot.Input.IsMouseButtonPressed(MouseButton.Left) || Godot.Input.IsMouseButtonPressed(MouseButton.Right);
    }
    internal bool Moved(InputEventMouseMotion motion)
    {
        bool moved = motion.Relative != Vector2.Zero && _position != motion.Position;
        _position = motion.Position;
        // Leaving the original control permanently cancels this gesture, even on re-entry.
        if (_press is { } press && !press.Rect.HasPoint(motion.Position)) Cancel();
        return moved;
    }

    internal string? Read(InputEventMouseButton button, string? id, Rect2 rect, object boundary)
    {
        if (button.ButtonIndex is not (MouseButton.Left or MouseButton.Right)) { Cancel(); return null; }
        if (button.Pressed)
        {
            bool fresh = _held.Add(button.ButtonIndex);
            if (fresh && _held.Count == 1 && !_blocked && !button.DoubleClick && id is not null)
                _press = (id, rect, boundary, button.ButtonIndex);
            else Cancel();
            return null;
        }
        var captured = _press;
        Cancel(); _held.Remove(button.ButtonIndex);
        if (!Godot.Input.IsMouseButtonPressed(MouseButton.Left) && !Godot.Input.IsMouseButtonPressed(MouseButton.Right))
        { _held.Clear(); _blocked = false; }
        return captured is { } value && value.Id == id && value.Rect == rect &&
            Equals(value.Boundary, boundary) && value.Button == button.ButtonIndex ? id : null;
    }
}

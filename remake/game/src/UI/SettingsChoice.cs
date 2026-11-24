using Godot;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.UI;

// BaseButton owns picking, clipping and release activation. The page's gesture can veto
// that activation after a drag, chord, resize or change of input owner.
internal sealed partial class SettingsChoice : Button
{
    internal required string Id { get; init; }
    internal required MenuPointer Gesture { get; init; }
    internal ScrollContainer? Clip { get; init; }
    internal required Action<InputEvent> Activate { get; init; }
    private InputEventMouseButton? _release;

    public override void _Ready()
    {
        StoryTheme.EntryChoice(this);
        FocusEntered += () => StoryTheme.Select(this, true);
        FocusExited += () => StoryTheme.Select(this, false);
        GuiInput += OnGuiInput;
        Pressed += () =>
        {
            if (_release is not { } input) return;
            _release = null;
            Activate(input);
        };
    }

    private void OnGuiInput(InputEvent input)
    {
        _release = null;
        if (input is InputEventMouseMotion motion)
        {
            if (FocusMode != FocusModeEnum.None && motion.Relative.Length() >= 4 &&
                motion.ButtonMask == 0 && new Rect2(Vector2.Zero, Size).HasPoint(motion.Position)) GrabFocus();
            return;
        }
        if (input is not InputEventMouseButton { ButtonIndex: MouseButton.Left } button) return;
        var bounds = GetGlobalRect();
        if (Clip is not null) bounds = bounds.Intersection(Clip.GetGlobalRect());
        string? hit = bounds.HasPoint(GetGlobalTransform() * button.Position) ? Id : null;
        if (Gesture.Read(button, hit, bounds, this) != null) _release = button;
    }
}

using Godot;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Input;

internal sealed partial class GameInput
{
    // Display state only. Observation never resolves, consumes or manufactures an action.
    internal event Action? HintsChanged;
    private int? _hintPad;
    private string _hintLayout = "position";
    private readonly Dictionary<(int Device, JoyAxis Axis), int> _hintAxes = [];
    private readonly HashSet<(int Device, JoyButton Button)> _hintButtons = [];
    private Vector2? _hintMouse;
    private bool _hintFocused = true;

    internal void HintFocus(bool focused, Vector2? mousePosition = null)
    {
        _hintFocused = focused; _hintMouse = focused ? mousePosition : null;
        if (!focused) return;
        _hintButtons.Clear(); _hintAxes.Clear();
        // Restoring focus is not a device choice. Existing holds need a new edge.
        foreach (int device in Godot.Input.GetConnectedJoypads())
        {
            foreach (JoyButton button in Enum.GetValues<JoyButton>().Where(b => b >= 0 && b < JoyButton.Max))
                if (Godot.Input.IsJoyButtonPressed(device, button)) _hintButtons.Add((device, button));
            foreach (JoyAxis axis in new[] { JoyAxis.LeftX, JoyAxis.LeftY, JoyAxis.RightX, JoyAxis.RightY, JoyAxis.TriggerLeft, JoyAxis.TriggerRight })
                _hintAxes[(device, axis)] = Deflection(Godot.Input.GetJoyAxis(device, axis));
        }
    }

    internal void HintDeviceChanged(int device, bool connected)
    {
        foreach (var key in _hintAxes.Keys.Where(key => key.Device == device).ToArray()) _hintAxes.Remove(key);
        _hintButtons.RemoveWhere(key => key.Device == device);
        if (!connected && _hintPad == device) ChooseHintDevice(null);
    }

    internal void ObserveHintInput(InputEvent input, bool focused)
    {
        bool valid = false;
        int? pad = null;
        switch (input)
        {
            case InputEventKey { Pressed: true, Echo: false }:
                valid = true; break;
            case InputEventMouseButton { Pressed: true, DoubleClick: false, Device: >= 0 }:
                valid = true; break;
            case InputEventMouseMotion { Device: >= 0 } mouse:
                valid = _hintMouse is { } previous && mouse.Position != previous && mouse.Relative.Length() >= 4;
                _hintMouse = mouse.Position; break;
            case InputEventJoypadButton button:
                var identity = (button.Device, button.ButtonIndex);
                if (!button.Pressed) _hintButtons.Remove(identity);
                else valid = _hintButtons.Add(identity);
                pad = button.Device; break;
            case InputEventJoypadMotion motion:
                var key = (motion.Device, motion.Axis);
                int direction = Deflection(motion.AxisValue);
                int previousDirection = _hintAxes.GetValueOrDefault(key);
                // Neutral hysteresis excludes drift and threshold chatter.
                if (Math.Abs(motion.AxisValue) < 0.35f) _hintAxes[key] = 0;
                else if (direction != 0)
                { _hintAxes[key] = direction; valid = direction != previousDirection; }
                pad = motion.Device; break;
        }
        if (valid && focused && _hintFocused) ChooseHintDevice(pad);
    }

    private static int Deflection(float value) => Math.Abs(value) > 0.5f ? Math.Sign(value) : 0;
    private void ChooseHintDevice(int? device)
    {
        string layout = device is { } pad ? PadLayout(pad) : "position";
        if (_hintPad == device && _hintLayout == layout) return;
        _hintPad = device; _hintLayout = layout; HintsChanged?.Invoke();
    }
    private static string PadLayout(int device)
    {
        if (!Godot.Input.IsJoyKnown(device)) return "position";
        string name = Godot.Input.GetJoyName(device).ToLowerInvariant();
        if (name.Contains("xbox 360")) return "xbox360";
        if (name.Contains("xbox") || name.Contains("xinput")) return "xbox";
        if (name.Contains("ps3")) return "ps3";
        if (name.Contains("ps5") || name.Contains("dualsense")) return "ps5";
        if (name.Contains("ps4") || name.Contains("dualshock")) return "ps4";
        if (name.Contains("switch pro")) return "nintendo";
        return "position";
    }

    internal bool KeyboardMouseHints => _hintPad is null;

    internal object ObserveHints() => new { device = _hintPad is null ? "keyboard-mouse" : "pad",
        pad = _hintPad, layout = _hintLayout, focused = _hintFocused,
        compact = Enum.GetValues<GameAction>().ToDictionary(action => action.ToString(), CompactHint) };

    internal string Hint(GameAction action) => string.Join(" / ", _bindings[action].Select(Describe));
    internal string CompactHint(GameAction action)
    {
        InputEvent? binding = _hintPad is null ? _bindings[action].OfType<InputEventKey>().FirstOrDefault() :
            _bindings[action].FirstOrDefault(input => input is InputEventJoypadButton or InputEventJoypadMotion);
        // A missing pad binding remains explicitly keyboard-accessible, never a fictitious button.
        return binding is null ? GameText.Get("input.keyboard-only", Describe(_bindings[action].OfType<InputEventKey>().First())) : Describe(binding);
    }
    private string Describe(InputEvent input) => input switch
    {
        InputEventKey key => OS.GetKeycodeString(key.Keycode),
        InputEventJoypadButton button => DescribeButton(button.ButtonIndex),
        InputEventJoypadMotion axis => DescribeAxis(axis.Axis, axis.AxisValue),
        _ => "",
    };
    private static string DescribeAxis(JoyAxis axis, float sign) => GameText.Get(axis is JoyAxis.LeftX or JoyAxis.LeftY ? "input.left-stick" : "input.right-stick") + " " +
            (axis is JoyAxis.LeftX or JoyAxis.RightX ? sign < 0 ? "←" : "→" : sign < 0 ? "↑" : "↓");
    internal string DescribeBinding(string code, bool key) => key ? OS.GetKeycodeString(Enum.Parse<Key>(code)) :
        ButtonCode(code) is { } button ? DescribeButton(button) :
        AxisCode(code) is { } axis ? DescribeAxis(axis.Axis, axis.Sign) : code;

    private string DescribeButton(JoyButton button)
    {
        int face = Array.IndexOf(new[] { JoyButton.A, JoyButton.B, JoyButton.X, JoyButton.Y }, button);
        if (face >= 0) return _hintLayout switch
        {
            "xbox" or "xbox360" => new[] { "A", "B", "X", "Y" }[face],
            "ps3" or "ps4" or "ps5" => new[] { "×", "○", "□", "△" }[face],
            "nintendo" => new[] { "B", "A", "Y", "X" }[face],
            _ => GameText.Get("input.face") + " " + new[] { "↓", "→", "←", "↑" }[face],
        };
        return button switch
        {
            JoyButton.DpadUp => GameText.Get("input.dpad") + " ↑", JoyButton.DpadDown => GameText.Get("input.dpad") + " ↓",
            JoyButton.DpadLeft => GameText.Get("input.dpad") + " ←", JoyButton.DpadRight => GameText.Get("input.dpad") + " →",
            JoyButton.LeftShoulder => _hintLayout switch { "xbox" or "xbox360" => "LB", "ps3" or "ps4" or "ps5" => "L1", "nintendo" => "L", _ => GameText.Get("input.left-shoulder") },
            JoyButton.RightShoulder => _hintLayout switch { "xbox" or "xbox360" => "RB", "ps3" or "ps4" or "ps5" => "R1", "nintendo" => "R", _ => GameText.Get("input.right-shoulder") },
            JoyButton.Start => _hintLayout switch { "xbox" => "Menu", "xbox360" => "Start", "ps3" => "Start", "ps4" or "ps5" => "Options", "nintendo" => "+", _ => GameText.Get("input.menu-button") },
            JoyButton.Back => _hintLayout switch { "xbox" => "View", "xbox360" => "Back", "ps3" => "Select", "ps4" => "Share", "ps5" => "Create", "nintendo" => "−", _ => GameText.Get("input.view-button") },
            JoyButton.LeftStick => GameText.Get("input.left-stick-click"), JoyButton.RightStick => GameText.Get("input.right-stick-click"),
            _ => GameText.Get("input.button", (int)button + 1),
        };
    }
}

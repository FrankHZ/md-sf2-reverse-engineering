using Godot;

namespace Sf2.Remake.GodotAdapter.Input;

internal enum GameAction { Up, Right, Down, Left, Confirm, Cancel, Attack, Spell, Target, Stay, Item, Wait }

internal sealed class GameInput
{
    private readonly Dictionary<GameAction, List<InputEvent>> _bindings = [];
    private readonly Dictionary<(int Device, JoyAxis Axis), int> _axes = [];
    internal InputSettings Settings { get; }
    internal bool WaitHeld { get; private set; }
    private bool _waitRequiresRelease;
    internal void DisarmWait() => _waitRequiresRelease |= WaitHeld;

    internal GameInput(InputSettings settings)
    {
        Settings = settings;
        Dictionary<string, ActionBinding> bindings = new(StringComparer.Ordinal)
        {
            ["up"] = new(["Up", "W"], ["DpadUp"], ["LeftY-"]),
            ["right"] = new(["Right", "D"], ["DpadRight"], ["LeftX+"]),
            ["down"] = new(["Down", "S"], ["DpadDown"], ["LeftY+"]),
            ["left"] = new(["Left", "A"], ["DpadLeft"], ["LeftX-"]),
            ["confirm"] = new(["Enter", "KpEnter", "Z"], ["South"], []),
            ["cancel"] = new(["Escape", "X"], ["East"], []),
            ["attack"] = new(["F"], ["West"], []),
            ["spell"] = new(["H"], ["North"], []),
            ["item"] = new(["I"], ["Back"], []),
            ["target"] = new(["Tab"], ["RightShoulder"], []),
            ["stay"] = new(["Space"], ["LeftShoulder"], []),
            ["wait"] = new(["V"], ["RightStick"], []),
        };
        foreach (var (name, binding) in settings.Bindings)
        {
            if (!bindings.ContainsKey(name)) throw new ArgumentException($"Unknown action: {name}.");
            bindings[name] = binding;
        }
        HashSet<string> identities = [];
        foreach (var (name, binding) in bindings)
        {
            if (binding is null || binding.Keys is null || binding.Buttons is null || binding.Axes is null ||
                binding.Keys.Length == 0 || binding.Buttons.Length + binding.Axes.Length == 0)
                throw new ArgumentException($"{name} requires keys, buttons and axes arrays, with keyboard and gamepad access.");
            List<InputEvent> events = [];
            foreach (string key in binding.Keys)
            {
                if (!Enum.TryParse<Key>(key, false, out var code) || !Enum.IsDefined(code) ||
                    code is Key.None or Key.Shift or Key.Ctrl or Key.Alt or Key.Meta || code.ToString() != key)
                    throw new ArgumentException($"Unknown key for {name}: {key}.");
                Unique("key:" + key);
                events.Add(new InputEventKey { Keycode = code });
            }
            foreach (string button in binding.Buttons)
            {
                JoyButton code = button switch
                {
                    "South" => JoyButton.A, "East" => JoyButton.B, "West" => JoyButton.X, "North" => JoyButton.Y,
                    "DpadUp" => JoyButton.DpadUp, "DpadRight" => JoyButton.DpadRight,
                    "DpadDown" => JoyButton.DpadDown, "DpadLeft" => JoyButton.DpadLeft,
                    "LeftShoulder" => JoyButton.LeftShoulder, "RightShoulder" => JoyButton.RightShoulder,
                    "Back" => JoyButton.Back, "Start" => JoyButton.Start,
                    "LeftStick" => JoyButton.LeftStick, "RightStick" => JoyButton.RightStick,
                    _ => throw new ArgumentException($"Unknown button for {name}: {button}."),
                };
                Unique("button:" + button);
                events.Add(new InputEventJoypadButton { ButtonIndex = code, Device = -1 });
            }
            foreach (string axis in binding.Axes)
            {
                (JoyAxis code, float value) = axis switch
                {
                    "LeftX-" => (JoyAxis.LeftX, -1), "LeftX+" => (JoyAxis.LeftX, 1),
                    "LeftY-" => (JoyAxis.LeftY, -1), "LeftY+" => (JoyAxis.LeftY, 1),
                    "RightX-" => (JoyAxis.RightX, -1), "RightX+" => (JoyAxis.RightX, 1),
                    "RightY-" => (JoyAxis.RightY, -1), "RightY+" => (JoyAxis.RightY, 1),
                    _ => throw new ArgumentException($"Unknown axis for {name}: {axis}."),
                };
                Unique("axis:" + axis);
                events.Add(new InputEventJoypadMotion { Axis = code, AxisValue = value, Device = -1 });
            }
            _bindings.Add(Enum.Parse<GameAction>(name, true), events);
        }
        if (settings.ConfirmCancel == "swapped")
            (_bindings[GameAction.Confirm], _bindings[GameAction.Cancel]) = (_bindings[GameAction.Cancel], _bindings[GameAction.Confirm]);
        // Install only after the complete explicit configuration has passed admission.
        foreach (var (action, events) in _bindings)
        {
            string name = MapName(action);
            if (InputMap.HasAction(name)) InputMap.EraseAction(name);
            InputMap.AddAction(name, 0.5f);
            foreach (var input in events) InputMap.ActionAddEvent(name, input);
        }
        void Unique(string identity)
        {
            if (!identities.Add(identity)) throw new ArgumentException($"Duplicate binding: {identity}.");
        }
    }

    internal GameAction? Resolve(InputEvent input)
    {
        bool held = Godot.Input.IsActionPressed(MapName(GameAction.Wait));
        bool freshWait = held && !WaitHeld;
        WaitHeld = held;
        // A canceled hold must pass through a real mapped release/neutral event.
        if (!held && (InputMap.EventIsAction(input, MapName(GameAction.Wait), true) ||
            input is InputEventJoypadMotion neutral && Math.Abs(neutral.AxisValue) < 0.5f &&
            _bindings[GameAction.Wait].Any(binding => binding is InputEventJoypadMotion axis && axis.Axis == neutral.Axis)))
            _waitRequiresRelease = false;
        if (input is InputEventJoypadMotion motion)
        {
            int direction = Math.Abs(motion.AxisValue) < 0.5f ? 0 : Math.Sign(motion.AxisValue);
            var key = (motion.Device, motion.Axis);
            int previous = _axes.GetValueOrDefault(key);
            _axes[key] = direction;
            // Each mapped axis gets one edge per deflection. A release, noise on another
            // axis, or a held value cannot repeat a previous direction or cross a mode twice.
            if (direction == 0 || direction == previous) return null;
        }
        else if (input is not (InputEventKey { Pressed: true, Echo: false } or InputEventJoypadButton { Pressed: true }))
            return null;
        foreach (var action in _bindings.Keys)
            if (InputMap.EventIsAction(input, MapName(action), true))
                return action == GameAction.Wait && (!freshWait || _waitRequiresRelease) ? null : action;
        return null;
    }

    internal string Hint(GameAction action) => string.Join(" / ", _bindings[action].Select(input => input switch
    {
        InputEventKey key => key.Keycode.ToString(),
        InputEventJoypadButton button => "Pad " + (button.ButtonIndex switch
        { JoyButton.A => "South", JoyButton.B => "East", JoyButton.X => "West", JoyButton.Y => "North", _ => button.ButtonIndex.ToString() }),
        InputEventJoypadMotion axis => $"Stick {axis.Axis}{(axis.AxisValue < 0 ? "-" : "+")}",
        _ => "",
    }));
    internal string MovementHint => string.Join("; ", new[] { GameAction.Up, GameAction.Right, GameAction.Down, GameAction.Left }
        .Select(action => $"{action}: {Hint(action)}"));
    private static string MapName(GameAction action) => "sf2_" + action.ToString().ToLowerInvariant();
}

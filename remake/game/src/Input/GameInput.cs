using Godot;

namespace Sf2.Remake.GodotAdapter.Input;

internal enum GameAction { Up, Right, Down, Left, Confirm, Cancel, Attack, Spell, Target, Stay, Item, Wait, Equipment, Give, Discard, Menu }

internal sealed class GameInput
{
    private readonly Dictionary<GameAction, List<InputEvent>> _bindings = [];
    private readonly Dictionary<(int Device, JoyAxis Axis), int> _axes = [];
    private readonly HashSet<(int Device, JoyButton Button)> _pressedButtons = [];
    internal InputSettings Settings { get; }
    private readonly List<GameAction> _movementDirections = [];
    private static readonly GameAction[] Directions = [GameAction.Up, GameAction.Right, GameAction.Down, GameAction.Left];
    internal GameAction? MovementDirection => _movementDirections.Count == 0 ? null : _movementDirections[^1];
    // Resumption needs a fresh non-echo event. Pad buttons/axes retain their ordinary
    // release/neutral edge guards below; keyboard releases may occur outside the window.
    internal void DisarmMovement() => _movementDirections.Clear();
    internal bool WaitHeld { get; private set; }
    private bool _waitRequiresRelease;
    internal void DisarmWait() => _waitRequiresRelease |= WaitHeld;
    // Godot's installed actions aggregate all mapped keys, pad buttons and axes.
    // This is held-state admission only, never a conditioned/repeating choice poll.
    internal bool GameplayHeld => _bindings.Keys.Any(action => action != GameAction.Wait && Godot.Input.IsActionPressed(MapName(action)));

    internal GameInput(InputSettings settings)
    {
        Settings = settings;
        Apply(settings, Prepare(settings));
    }

    // Pure admission: preparing a candidate must never install InputMap actions.
    internal static Dictionary<GameAction, List<InputEvent>> Prepare(InputSettings settings)
    {
        InputSettings.Validate(settings);
        Dictionary<GameAction, List<InputEvent>> prepared = [];
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
            ["discard"] = new(["R"], [], ["RightY+"]),
            ["give"] = new(["G"], ["LeftStick"], []),
            ["equipment"] = new(["E"], ["Start"], []),
            ["target"] = new(["Tab"], ["RightShoulder"], []),
            ["stay"] = new(["Space"], ["LeftShoulder"], []),
            ["wait"] = new(["V"], ["RightStick"], []),
        };
        foreach (var (name, binding) in settings.Bindings)
        {
            if (name != "menu" && !bindings.ContainsKey(name)) throw new ArgumentException($"Unknown action: {name}.");
            bindings[name] = binding;
        }
        // Version-1 preferences predate Menu and may already claim its new defaults.
        // Fill only unclaimed inputs; preserve every existing custom gameplay binding.
        if (!bindings.ContainsKey("menu"))
        {
            var usedKeys = bindings.Values.Where(row => row?.Keys is not null).SelectMany(row => row.Keys).ToHashSet();
            var usedAxes = bindings.Values.Where(row => row?.Axes is not null).SelectMany(row => row.Axes).ToHashSet();
            string key = new[] { "F10", "F9", "F8", "F7" }.Concat(Enum.GetNames<Key>())
                .First(value => value is not ("None" or "Shift" or "Ctrl" or "Alt" or "Meta") && !usedKeys.Contains(value));
            string? axis = new[] { "RightX+", "RightX-", "RightY-", "RightY+", "LeftX+", "LeftX-", "LeftY+", "LeftY-" }
                .FirstOrDefault(value => !usedAxes.Contains(value));
            bindings["menu"] = new([key], [], axis is null ? [] : [axis]);
        }
        HashSet<string> identities = [];
        foreach (var (name, binding) in bindings)
        {
            if (binding is null || binding.Keys is null || binding.Buttons is null || binding.Axes is null ||
                binding.Keys.Length == 0 || binding.Buttons.Length + binding.Axes.Length == 0 &&
                    (name != "menu" || settings.Bindings.ContainsKey("menu")))
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
            // Godot 4.7.2 Input::MAX_EVENT is 32 across keys, buttons and signed axes.
            // Reject before any caller can persist or install a partially usable action.
            if (events.Count > 32)
                throw new ArgumentException($"{name} supports at most 32 total key, button and axis bindings.");
            prepared.Add(Enum.Parse<GameAction>(name, true), events);
        }
        if (settings.ConfirmCancel == "swapped")
            (prepared[GameAction.Confirm], prepared[GameAction.Cancel]) = (prepared[GameAction.Cancel], prepared[GameAction.Confirm]);
        return prepared;
        void Unique(string identity)
        {
            if (!identities.Add(identity)) throw new ArgumentException($"Duplicate binding: {identity}.");
        }
    }

    internal void Apply(InputSettings candidate, Dictionary<GameAction, List<InputEvent>> prepared)
    {
        Settings.Publish(candidate);
        _bindings.Clear();
        foreach (var (action, events) in prepared)
        {
            _bindings.Add(action, events);
            string name = MapName(action);
            // InputMap replacement alone can retain a held old action in Godot.
            if (InputMap.HasAction(name))
            { Godot.Input.ActionRelease(name); InputMap.EraseAction(name); }
            InputMap.AddAction(name, 0.5f);
            foreach (var input in events) InputMap.ActionAddEvent(name, input);
        }
        DisarmMovement(); DisarmWait();
    }

    internal GameAction? Resolve(InputEvent input)
    {
        foreach (var direction in Directions)
        {
            if (Godot.Input.IsActionPressed(MapName(direction))) continue;
            _movementDirections.Remove(direction);
        }
        bool held = Godot.Input.IsActionPressed(MapName(GameAction.Wait));
        bool freshWait = held && !WaitHeld;
        WaitHeld = held;
        // A canceled hold must pass through a real mapped release/neutral event.
        if (!held && (InputMap.EventIsAction(input, MapName(GameAction.Wait), true) ||
            input is InputEventJoypadMotion neutral && Math.Abs(neutral.AxisValue) < 0.5f &&
            _bindings[GameAction.Wait].Any(binding => binding is InputEventJoypadMotion axis && axis.Axis == neutral.Axis)))
            _waitRequiresRelease = false;
        if (input is InputEventJoypadButton button)
        {
            var identity = (button.Device, button.ButtonIndex);
            if (!button.Pressed) { _pressedButtons.Remove(identity); return null; }
            if (!_pressedButtons.Add(identity)) return null;
        }
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
            {
                if (Directions.Contains(action))
                {
                    if (_movementDirections.Contains(action)) return null;
                    _movementDirections.Add(action);
                }
                return action == GameAction.Wait && (!freshWait || _waitRequiresRelease) ? null : action;
            }
        return null;
    }

    internal void ReleaseSettingsHold()
    {
        DisarmMovement(); DisarmWait();
        foreach (var action in _bindings.Keys) Godot.Input.ActionRelease(MapName(action));
    }

    internal string Hint(GameAction action) => string.Join(" / ", _bindings[action].Select(Describe));
    internal bool MenuPadAvailable => _bindings[GameAction.Menu].Any(input => input is InputEventJoypadButton or InputEventJoypadMotion);
    private static string Describe(InputEvent input) => input switch
    {
        InputEventKey key => key.Keycode.ToString(),
        InputEventJoypadButton button => "Pad " + (button.ButtonIndex switch
        { JoyButton.A => "South", JoyButton.B => "East", JoyButton.X => "West", JoyButton.Y => "North", _ => button.ButtonIndex.ToString() }),
        InputEventJoypadMotion axis => $"Stick {axis.Axis}{(axis.AxisValue < 0 ? "-" : "+")}",
        _ => "",
    };
    // Presentation only: use the admitted, possibly swapped mapping rather than a second key table.
    internal string CompactHint(GameAction action) => string.Join("/", new[]
    {
        _bindings[action].OfType<InputEventKey>().First(),
        _bindings[action].FirstOrDefault(input => input is InputEventJoypadButton or InputEventJoypadMotion),
    }.OfType<InputEvent>().Select(input => Describe(input).Replace("Pad ", "", StringComparison.Ordinal)));
    internal string MovementHint => string.Join("; ", new[] { GameAction.Up, GameAction.Right, GameAction.Down, GameAction.Left }
        .Select(action => $"{action}: {Hint(action)}"));
    private static string MapName(GameAction action) => "sf2_" + action.ToString().ToLowerInvariant();
}

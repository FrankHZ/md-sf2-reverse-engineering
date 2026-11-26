using Godot;

namespace Sf2.Remake.GodotAdapter.Input;

internal enum GameAction { Up, Right, Down, Left, Confirm, Cancel, Attack, Spell, Target, Stay, Item, Wait, Equipment, Give, Discard, Menu, Accelerate }

internal sealed partial class GameInput
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
    internal bool GameplayHeld => _bindings.Keys.Any(action => action is not (GameAction.Wait or GameAction.Accelerate) && Godot.Input.IsActionPressed(MapName(action)));

    internal static bool InterruptsTextDelay(GameAction action) => action is GameAction.Up or GameAction.Down or
        GameAction.Left or GameAction.Right or GameAction.Confirm or GameAction.Cancel or GameAction.Attack;
    // A Confirm edge may have advanced another span or opened the current dialogue.
    internal bool TextDelayInputHeld => _bindings.Keys.Any(action => action != GameAction.Confirm &&
        InterruptsTextDelay(action) && Godot.Input.IsActionPressed(MapName(action)));

    internal GameInput(InputSettings settings)
    {
        Settings = settings;
        Apply(settings, Prepare(settings));
    }

    // Pure admission: preparing a candidate must never install InputMap actions.
    internal static Dictionary<GameAction, List<InputEvent>> Prepare(InputSettings settings)
    {
        InputSettings.Validate(settings);
        return PrepareRows(settings, BaseRows(settings));
    }

    internal static Dictionary<string, ActionBinding> BaseRows(InputSettings settings)
    {
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
            ["accelerate"] = new(["Shift"], [], []),
        };
        foreach (var (name, binding) in settings.Bindings)
        {
            if (name != "menu" && !bindings.ContainsKey(name)) throw new ArgumentException($"Unknown action: {name}.");
            if (binding is null || binding.Keys is null || binding.Buttons is null || binding.Axes is null)
                throw new ArgumentException($"{name} requires keys, buttons and axes arrays.");
            bindings[name] = binding;
        }
        // Explicit rows are never migrated. An inherited Equipment row vacates Start
        // only when Menu needs it; old explicit Menu configurations retain Equipment.
        if (!settings.Bindings.ContainsKey("equipment") &&
            (!bindings.TryGetValue("menu", out var menu) || menu.Buttons.Contains("Start")))
        {
            var used = bindings.Where(p => p.Key != "equipment").SelectMany(p => p.Value.Axes).ToHashSet();
            string? axis = MenuAxes.FirstOrDefault(value => !used.Contains(value));
            if (axis is not null) bindings["equipment"] = new(["E"], [], [axis]);
        }
        // Missing rows inherit current defaults without rewriting the preference file.
        // Escape may share effective Cancel, but never Confirm or another action.
        if (!bindings.ContainsKey("menu"))
        {
            string cancel = settings.ConfirmCancel == "swapped" ? "confirm" : "cancel";
            var usedKeys = bindings.Values.SelectMany(row => row.Keys).ToHashSet();
            if (bindings[cancel].Keys.Contains("Escape") && bindings[cancel].Keys.Length > 1)
                usedKeys.Remove("Escape");
            var usedButtons = bindings.Values.SelectMany(row => row.Buttons).ToHashSet();
            var usedAxes = bindings.Values.Where(row => row?.Axes is not null).SelectMany(row => row.Axes).ToHashSet();
            string key = new[] { "Escape", "F10", "F9", "F8", "F7" }.Concat(Enum.GetNames<Key>())
                .First(value => value is not ("None" or "Shift" or "Ctrl" or "Alt" or "Meta") && !usedKeys.Contains(value));
            string? axis = MenuAxes.FirstOrDefault(value => !usedAxes.Contains(value));
            bindings["menu"] = !usedButtons.Contains("Start") ? new([key], ["Start"], []) :
                new([key], [], axis is null ? [] : [axis]);
        }
        return bindings;
    }

    private static readonly string[] MenuAxes = ["RightX+", "RightX-", "RightY-", "RightY+", "LeftX+", "LeftX-", "LeftY+", "LeftY-"];

    internal static bool BindingsConflict(string first, string second, InputSettings settings)
    {
        string cancel = settings.ConfirmCancel == "swapped" ? "confirm" : "cancel";
        return !((first == "menu" && second == cancel) || (second == "menu" && first == cancel));
    }

    internal static bool MissingFamily(string name, ActionBinding row, bool explicitlyBound) =>
        row.Keys.Length == 0 || row.Buttons.Length + row.Axes.Length == 0 &&
            name != "accelerate" && (name != "menu" || explicitlyBound);

    private static Dictionary<GameAction, List<InputEvent>> PrepareRows(InputSettings settings, Dictionary<string, ActionBinding> bindings)
    {
        Dictionary<GameAction, List<InputEvent>> prepared = [];
        Dictionary<string, string> identities = [];
        foreach (var (name, binding) in bindings)
        {
            if (binding is null || binding.Keys is null || binding.Buttons is null || binding.Axes is null ||
                MissingFamily(name, binding, settings.Bindings.ContainsKey(name)))
                throw new ArgumentException($"{name} requires keys, buttons and axes arrays, with keyboard and gamepad access.");
            List<InputEvent> events = [];
            foreach (string key in binding.Keys)
            {
                if (!Enum.TryParse<Key>(key, false, out var code) || !Enum.IsDefined(code) ||
                    code is Key.None or Key.Ctrl or Key.Alt or Key.Meta || code == Key.Shift && name != "accelerate" || code.ToString() != key)
                    throw new ArgumentException($"Unknown key for {name}: {key}.");
                Unique("key:" + key, name);
                events.Add(new InputEventKey { Keycode = code });
            }
            foreach (string button in binding.Buttons)
            {
                JoyButton code = ButtonCode(button) ?? throw new ArgumentException($"Unknown button for {name}: {button}.");
                Unique("button:" + button, name);
                events.Add(new InputEventJoypadButton { ButtonIndex = code, Device = -1 });
            }
            foreach (string axis in binding.Axes)
            {
                (JoyAxis code, float value) = AxisCode(axis) ?? throw new ArgumentException($"Unknown axis for {name}: {axis}.");
                Unique("axis:" + axis, name);
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
        var cancelOnly = prepared[GameAction.Cancel].Where(input =>
            !prepared[GameAction.Menu].Any(menu => menu.AsText() == input.AsText())).ToArray();
        if (!cancelOnly.Any(input => input is InputEventKey) ||
            !cancelOnly.Any(input => input is InputEventJoypadButton or InputEventJoypadMotion))
            throw new ArgumentException("Cancel needs a keyboard and gamepad alternative distinct from Menu for gameplay entry.");
        return prepared;
        void Unique(string identity, string name)
        {
            if (identities.TryGetValue(identity, out var owner) && BindingsConflict(owner, name, settings))
                throw new ArgumentException($"Duplicate binding: {identity} ({owner}, {name}).");
            identities[identity] = name;
        }
    }

    internal static JoyButton? ButtonCode(string button) => button switch
    {
        "South" => JoyButton.A, "East" => JoyButton.B, "West" => JoyButton.X, "North" => JoyButton.Y,
        "DpadUp" => JoyButton.DpadUp, "DpadRight" => JoyButton.DpadRight,
        "DpadDown" => JoyButton.DpadDown, "DpadLeft" => JoyButton.DpadLeft,
        "LeftShoulder" => JoyButton.LeftShoulder, "RightShoulder" => JoyButton.RightShoulder,
        "Back" => JoyButton.Back, "Start" => JoyButton.Start,
        "LeftStick" => JoyButton.LeftStick, "RightStick" => JoyButton.RightStick,
        _ => null,
    };

    internal static (JoyAxis Axis, float Sign)? AxisCode(string axis) => axis switch
    {
        "LeftX-" => (JoyAxis.LeftX, -1), "LeftX+" => (JoyAxis.LeftX, 1),
        "LeftY-" => (JoyAxis.LeftY, -1), "LeftY+" => (JoyAxis.LeftY, 1),
        "RightX-" => (JoyAxis.RightX, -1), "RightX+" => (JoyAxis.RightX, 1),
        "RightY-" => (JoyAxis.RightY, -1), "RightY+" => (JoyAxis.RightY, 1),
        _ => null,
    };

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
        DisarmMovement(); DisarmWait(); DisarmAcceleration();
        HintsChanged?.Invoke();
    }

    internal GameAction? Resolve(InputEvent input, bool freshTargetNavigation = false, bool menuAtBase = false, bool menuBack = false)
    {
        // Shift is a pacing modifier, not a second action or direction edge. Preserve
        // exact matching for all other modifiers and never mutate the received event.
        using InputEventKey? normalized = WithoutAccelerationModifier(input);
        input = normalized ?? input;
        if (InputMap.EventIsAction(input, MapName(GameAction.Accelerate), true)) return null;
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
        bool stickHeld = false;
        if (input is InputEventJoypadMotion motion)
        {
            int direction = Math.Abs(motion.AxisValue) < 0.5f ? 0 : Math.Sign(motion.AxisValue);
            var key = (motion.Device, motion.Axis);
            int previous = _axes.GetValueOrDefault(key);
            stickHeld = previous != 0;
            // Keep the previous sign through dead-zone chatter. Only a clear
            // neutral releases the latch for this axis.
            if (Math.Abs(motion.AxisValue) < 0.35f) _axes[key] = 0;
            else if (direction != 0) _axes[key] = direction;
            // Each mapped axis gets one edge per deflection. A release, noise on another
            // axis, or a held value cannot repeat a previous direction or cross a mode twice.
            if (direction == 0 || direction == previous) return null;
        }
        else if (input is not (InputEventKey { Pressed: true, Echo: false } or InputEventJoypadButton { Pressed: true }))
            return null;
        // Resolve the shared edge once, after the ordinary release/neutral guards.
        if (InputMap.EventIsAction(input, MapName(GameAction.Menu), true))
        {
            if (menuAtBase) return GameAction.Menu;
            if (menuBack) return GameAction.Cancel;
        }
        foreach (var action in _bindings.Keys.Where(action => action != GameAction.Menu))
            if (InputMap.EventIsAction(input, MapName(action), true))
            {
                // The target page rearms an axis at neutral, including when a
                // sampled deflection jumps straight to the opposite sign.
                if (freshTargetNavigation && stickHeld && (action == GameAction.Target || Directions.Contains(action)))
                    return null;
                if (Directions.Contains(action))
                {
                    if (_movementDirections.Contains(action)) return null;
                    _movementDirections.Add(action);
                }
                return action == GameAction.Wait && (!freshWait || _waitRequiresRelease) ? null : action;
            }
        return null;
    }

    internal void ReleaseMappedHolds()
    {
        DisarmMovement(); DisarmWait(); DisarmAcceleration();
        foreach (var action in _bindings.Keys) Godot.Input.ActionRelease(MapName(action));
    }

    internal void ReconcileFocusHolds()
    {
        ReconcileAccelerationNeutral();
        WaitHeld = Godot.Input.IsActionPressed(MapName(GameAction.Wait));
        _waitRequiresRelease = WaitHeld;
    }

    internal bool MenuPadAvailable => _bindings[GameAction.Menu].Any(input => input is InputEventJoypadButton or InputEventJoypadMotion);
    private static string MapName(GameAction action) => "sf2_" + action.ToString().ToLowerInvariant();
}

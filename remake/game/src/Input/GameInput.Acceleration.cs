using Godot;

namespace Sf2.Remake.GodotAdapter.Input;

internal sealed partial class GameInput
{
    // Physical key location distinguishes the two Shift keys; pad identities are
    // device-specific. A release of one binding cannot release another binding.
    private readonly HashSet<(int Device, int Kind, long Code, int Location)> _accelerationSources = [];
    private bool _accelerationAllowed, _accelerationArmed, _accelerationRequiresRelease;
    internal double AccelerationRate => _accelerationArmed ? 1.5 : 1;
    internal long AccelerationGeneration { get; private set; }
    internal object ObserveAccelerationState() => new
    {
        rate = AccelerationRate, allowed = _accelerationAllowed,
        held = _accelerationSources.Count != 0, requiresRelease = _accelerationRequiresRelease,
        generation = AccelerationGeneration,
    };

    internal void AllowAcceleration(bool allowed)
    {
        _accelerationAllowed = allowed;
        if (!allowed) DisarmAcceleration();
    }

    internal void DisarmAcceleration()
    {
        SetAccelerationArmed(false);
        _accelerationRequiresRelease |= _accelerationSources.Count != 0;
    }

    internal void ObserveAccelerationInput(InputEvent input)
    {
        using InputEventKey? normalized = WithoutAccelerationModifier(input);
        ObserveAcceleration(normalized ?? input);
    }

    private InputEventKey? WithoutAccelerationModifier(InputEvent input)
    {
        if (input is not InputEventKey { ShiftPressed: true } key ||
            !_bindings[GameAction.Accelerate].OfType<InputEventKey>().Any(binding => binding.Keycode == Key.Shift)) return null;
        var normalized = (InputEventKey)key.Duplicate();
        normalized.ShiftPressed = false;
        return normalized;
    }

    // Native focus changes can release Godot's device state without delivering
    // a key-up to the old window. Neutrality can clear a latch, never arm a hold.
    internal void ReconcileAccelerationNeutral()
    {
        _accelerationSources.RemoveWhere(source => source.Kind switch
        {
            0 => !Godot.Input.IsKeyPressed((Key)source.Code),
            1 => !Godot.Input.IsJoyButtonPressed(source.Device, (JoyButton)source.Code),
            _ => Math.Abs(Godot.Input.GetJoyAxis(source.Device, (JoyAxis)source.Code)) < 0.5f ||
                Math.Sign(Godot.Input.GetJoyAxis(source.Device, (JoyAxis)source.Code)) != source.Location,
        });
        if (_accelerationSources.Count == 0)
        { SetAccelerationArmed(false); _accelerationRequiresRelease = false; }
    }

    internal void ReleaseAccelerationDevice(int device)
    {
        DisarmAcceleration();
        _accelerationSources.RemoveWhere(source => source.Kind != 0 && source.Device == device);
        if (_accelerationSources.Count == 0) _accelerationRequiresRelease = false;
    }

    private void SetAccelerationArmed(bool armed)
    {
        if (_accelerationArmed == armed) return;
        _accelerationArmed = armed; AccelerationGeneration++;
    }

    private void ObserveAcceleration(InputEvent input)
    {
        bool matched = InputMap.EventIsAction(input, MapName(GameAction.Accelerate), true);
        bool pressed;
        (int Device, int Kind, long Code, int Location) identity;
        switch (input)
        {
            case InputEventKey key:
                identity = (0, 0, (long)key.Keycode, (int)key.Location);
                pressed = key.Pressed;
                if (key.Echo) return;
                break;
            case InputEventJoypadButton button:
                identity = (button.Device, 1, (long)button.ButtonIndex, 0);
                pressed = button.Pressed;
                break;
            case InputEventJoypadMotion axis:
                bool bound = _bindings[GameAction.Accelerate].OfType<InputEventJoypadMotion>()
                    .Any(binding => binding.Axis == axis.Axis);
                if (!bound) return;
                identity = (axis.Device, 2, (long)axis.Axis, Math.Abs(axis.AxisValue) < 0.5f ? 0 : Math.Sign(axis.AxisValue));
                _accelerationSources.RemoveWhere(source => source.Kind == 2 && source.Device == axis.Device && source.Code == (long)axis.Axis && source.Location != identity.Location);
                pressed = matched && identity.Location != 0;
                break;
            default: return;
        }
        bool fresh = pressed && matched && _accelerationSources.Add(identity);
        if (!pressed) _accelerationSources.Remove(identity);
        if (_accelerationSources.Count == 0)
        { SetAccelerationArmed(false); _accelerationRequiresRelease = false; }
        else if (fresh)
        {
            if (!_accelerationAllowed) _accelerationRequiresRelease = true;
            if (_accelerationAllowed && !_accelerationRequiresRelease) SetAccelerationArmed(true);
        }
    }
}

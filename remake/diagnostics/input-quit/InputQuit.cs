using Godot;
using System.Runtime.InteropServices;
using System.Text.Json;

// Borrow the input argument, record primitive values, and use ordinary SceneTree.Quit.
// No event retention, manual disposal, allocation pressure, or finalizer intervention.
public partial class InputQuit : Node
{
    private int _step;

    public override void _Ready() => Write(new
    {
        kind = "ready", runtime = RuntimeInformation.FrameworkDescription,
        assemblyMvid = typeof(InputQuit).Module.ModuleVersionId,
    });

    public override void _Input(InputEvent input)
    {
        if (input is not InputEventKey key || key.Echo) return;
        bool expected = _step switch
        {
            0 => key.Keycode == Key.Left && key.Pressed,
            1 => key.Keycode == Key.Left && !key.Pressed,
            2 => key.Keycode == Key.Enter && key.Pressed,
            _ => false,
        };
        Write(new
        {
            kind = "input", step = _step, expected,
            type = key.GetType().FullName, keycode = (long)key.Keycode,
            physicalKeycode = (long)key.PhysicalKeycode, pressed = key.Pressed,
            nativePtr = key.NativeInstance.ToString("x"),
        });
        if (!expected) return;
        _step++;
        if (_step != 3) return;
        Write(new { kind = "terminal", steps = _step, exitAction = "SceneTree.Quit" });
        GetTree().Quit();
    }

    private static void Write(object value) => GD.Print("INPUT_QUIT ", JsonSerializer.Serialize(value));
}

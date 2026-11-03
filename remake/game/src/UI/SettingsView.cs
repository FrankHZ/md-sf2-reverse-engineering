using Godot;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.UI;

internal sealed partial class SettingsView : Control
{
    internal required GameInput Input { get; init; }
    internal required Action Close { get; init; }
    internal required Func<InputSettings, bool, string?> Apply { get; init; }
    internal string? Notice { get; init; }
    internal bool ReadOnly { get; init; }
    private InputSettings _draft = null!;
    private bool _reset, _help;
    private int _selected;
    private string _status = "";
    private PanelContainer _panel = null!;
    private ScrollContainer _scroll = null!;
    private Label _message = null!, _hint = null!, _controls = null!;
    private readonly List<Button> _buttons = [];
    private readonly MenuPointer _pointer = new();
    private readonly object _boundary = new();

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry();
        _draft = Input.Settings.Draft();
        var shade = new ColorRect { Name = "SettingsShade", Color = new("051225b8") };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        _panel = new PanelContainer { Name = "SettingsWindow" }; AddChild(_panel);
        _scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        _panel.AddChild(_scroll);
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", 12); _scroll.AddChild(column);
        column.AddChild(new Label { Text = "Settings", HorizontalAlignment = HorizontalAlignment.Center });
        _message = new Label { Name = "SettingsMessage", AutowrapMode = TextServer.AutowrapMode.WordSmart, Text = Notice ?? "Changes take effect only after Apply." };
        column.AddChild(_message);
        for (int i = 0; i < 8; i++)
        {
            var button = new Button { Name = "Setting" + i, CustomMinimumSize = new(0, 58),
                FocusMode = FocusModeEnum.None, MouseFilter = MouseFilterEnum.Ignore,
                AutowrapMode = TextServer.AutowrapMode.WordSmart, Disabled = ReadOnly && i < 6 };
            column.AddChild(button); _buttons.Add(button);
        }
        _controls = new Label { Name = "SettingsHelp", AutowrapMode = TextServer.AutowrapMode.WordSmart, Visible = false };
        _controls.AddThemeFontSizeOverride("font_size", 22); column.AddChild(_controls);
        _hint = new Label { Name = "SettingsHint", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); column.AddChild(_hint);
        _selected = ReadOnly ? 6 : 0;
        Resized += Arrange; Arrange(); Refresh();
    }

    private void Arrange()
    {
        _panel.Size = new(Math.Min(960, Math.Max(1, Size.X - 64)), Math.Min(920, Math.Max(1, Size.Y - 64)));
        _panel.Position = (Size - _panel.Size) / 2;
    }

    internal void SuspendPointer() => _pointer.Suspend();

    public override void _Process(double delta)
    {
        if (!GetWindow().HasFocus() || !IsVisibleInTree()) { _pointer.Suspend(); return; }
        _pointer.ReleaseIfNeutral();
    }

    internal void Handle(GameAction action)
    {
        _pointer.Suspend();
        if (action == GameAction.Cancel) { Close(); return; }
        if (action is GameAction.Up or GameAction.Down)
        {
            int step = action == GameAction.Up ? -1 : 1;
            do { _selected = (_selected + step + _buttons.Count) % _buttons.Count; } while (_buttons[_selected].Disabled);
            Refresh(); _scroll.EnsureControlVisible(_buttons[_selected]);
        }
        else if (action == GameAction.Confirm) Activate(1);
        else if (action is GameAction.Left or GameAction.Right && _selected < 4) Activate(action == GameAction.Left ? -1 : 1);
    }

    internal void HandlePointer(InputEvent input)
    {
        if (!GetWindow().HasFocus()) { _pointer.Suspend(); return; }
        int hit = _buttons.FindIndex(button => button.IsVisibleInTree() && !button.Disabled &&
            _scroll.GetGlobalRect().Intersection(button.GetGlobalRect()).HasPoint(((InputEventMouse)input).Position));
        if (input is InputEventMouseMotion motion && _pointer.Moved(motion) && hit >= 0)
        { _selected = hit; Refresh(); }
        if (input is InputEventMouseButton button)
        {
            string? id = hit < 0 ? null : hit.ToString();
            if (_pointer.Read(button, id, hit < 0 ? new() : _buttons[hit].GetGlobalRect(), _boundary) is not null)
            {
                _selected = hit;
                if (button.ButtonIndex == MouseButton.Right) Close(); else Activate(1);
            }
            // Keep wheel scrolling confined to this modal.
            if (button.ButtonIndex is MouseButton.WheelDown or MouseButton.WheelUp)
                _scroll.ScrollVertical += button.ButtonIndex == MouseButton.WheelDown ? 80 : -80;
        }
    }

    private void Activate(int direction)
    {
        if (_buttons[_selected].Disabled) return;
        switch (_selected)
        {
            case 0: _draft.TextMode = _draft.TextMode == "instant" ? "adjustable" : "instant"; break;
            case 1: _draft.CharactersPerSecond = Math.Clamp(_draft.CharactersPerSecond + direction * 10, 1, 240); break;
            case 2: _draft.ConfirmCancel = _draft.ConfirmCancel == "standard" ? "swapped" : "standard"; break;
            case 3: _draft.ReducedFlash = !_draft.ReducedFlash; break;
            case 4: _draft = Input.Settings.Draft(defaults: true); _reset = true; _status = "Defaults staged. Apply saves them; Cancel keeps current settings."; break;
            case 5:
                _status = Apply(_draft, _reset) ?? "";
                if (_status == "") { Close(); return; }
                break;
            case 6: Close(); return;
            case 7: _help = !_help; break;
        }
        Refresh();
    }

    private void Refresh()
    {
        string[] labels = [$"Text: {(_draft.TextMode == "instant" ? "Instant" : "Gradual")}",
            $"Text speed: {_draft.CharactersPerSecond:0.##} characters/sec · Left/Right −/+10",
            $"Confirm / Cancel: {_draft.ConfirmCancel}", $"Reduced flash: {(_draft.ReducedFlash ? "On" : "Off")}",
            "Restore defaults (draft)", "Apply", ReadOnly ? "Close" : "Cancel", _help ? "Hide control help" : "Control help"];
        for (int i = 0; i < labels.Length; i++) { _buttons[i].Text = labels[i]; StoryTheme.Select(_buttons[i], i == _selected); }
        _message.Text = _status != "" ? _status : Notice ?? "Changes take effect only after Apply. Text speed is used in Gradual mode.";
        _hint.Text = $"{Input.CompactHint(GameAction.Confirm)}: Choose/change   •   {Input.CompactHint(GameAction.Cancel)}: Cancel/close";
        _controls.Visible = _help;
        _controls.Text = "At settled field input: Cancel opens Settings; Stay opens Save.\n" + string.Join("\n", Enum.GetValues<GameAction>().Select(action => $"{action}: {Input.Hint(action)}"));
    }

    public string ReadSettingsObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { draft = _draft, applied = Input.Settings, selected = _selected, readOnly = ReadOnly, reset = _reset, message = _message.Text,
        help = _help, bindings = Enum.GetValues<GameAction>().ToDictionary(action => action.ToString(), Input.Hint) });
}

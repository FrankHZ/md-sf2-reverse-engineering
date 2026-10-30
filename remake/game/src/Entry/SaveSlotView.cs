using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed partial class SaveSlotView : Control
{
    private Label _message = null!;
    private VBoxContainer _column = null!;
    private readonly List<Button> _buttons = [];
    private readonly List<Action> _actions = [];
    private int _selected;
    internal Action? Cancel;
    internal string State { get; private set; } = "slots";

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create();
        var shade = new ColorRect { Color = new("081a35"), MouseFilter = MouseFilterEnum.Stop };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        var center = new CenterContainer(); center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(center);
        var panel = new PanelContainer { CustomMinimumSize = new(900, 540) }; center.AddChild(panel);
        _column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        _column.AddThemeConstantOverride("separation", 22); panel.AddChild(_column);
        _message = new Label { CustomMinimumSize = new(850, 160), HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _column.AddChild(_message);
    }

    internal void ShowChoices(string state, string message, params (string Label, Action Action)[] choices)
    {
        State = state; _message.Text = message;
        foreach (var button in _buttons) { _column.RemoveChild(button); button.QueueFree(); }
        _buttons.Clear(); _actions.Clear(); _selected = 0;
        foreach (var (label, action) in choices)
        {
            var button = new Button { Text = label, CustomMinimumSize = new(0, 68), FocusMode = FocusModeEnum.None };
            button.Pressed += () => action(); _column.AddChild(button); _buttons.Add(button); _actions.Add(action);
        }
        Select(0);
    }

    internal void Handle(GameAction action)
    {
        if (action == GameAction.Cancel) Cancel?.Invoke();
        else if (action == GameAction.Up) Select((_selected + _buttons.Count - 1) % _buttons.Count);
        else if (action == GameAction.Down) Select((_selected + 1) % _buttons.Count);
        else if (action == GameAction.Confirm && _actions.Count > 0) _actions[_selected]();
    }
    private void Select(int index)
    {
        _selected = index;
        for (int i = 0; i < _buttons.Count; i++) _buttons[i].Modulate = i == index ? StoryTheme.Selected : Colors.White;
    }
    public string ReadSaveObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { state = State, message = _message.Text, selected = _selected, choices = _buttons.Select(button => button.Text).ToArray() });
}

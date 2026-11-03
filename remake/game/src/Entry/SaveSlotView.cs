using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed partial class SaveSlotView : Control
{
    internal GameInput? Input { get; init; }
    private Label _hint = null!;
    private Label _message = null!;
    private VBoxContainer _column = null!;
    private readonly List<Button> _buttons = [];
    private readonly List<Action> _actions = [];
    private int _selected;
    internal Action? Cancel;
    internal bool OverEntry { get; init; }
    internal string State { get; private set; } = "slots";

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Theme = OverEntry ? StoryTheme.CreateEntry() : StoryTheme.Create();
        var panel = new PanelContainer { Name = "SlotWindow" };
        if (OverEntry) { panel.Position = EntryLayout.Window.Position; panel.Size = EntryLayout.Window.Size; AddChild(panel); }
        else
        {
            var shade = new ColorRect { Color = new("081a35"), MouseFilter = MouseFilterEnum.Stop };
            shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
            var center = new CenterContainer(); center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(center);
            panel.CustomMinimumSize = new(900, 540); center.AddChild(panel);
        }
        _column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        _column.AddThemeConstantOverride("separation", 20); panel.AddChild(_column);
        _message = new Label { Name = "SlotMessage", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _column.AddChild(_message);
        _hint = new Label { Name = "SlotHint", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        _column.AddChild(_hint);
    }

    internal void ShowChoices(string state, string message, params (string Label, Action Action)[] choices)
    {
        State = state; _message.Text = message;
        _message.AddThemeColorOverride("font_color", state.Contains("error") || state.Contains("uncertain")
            ? new Color("ffc4ad") : Colors.White);
        _hint.Text = Input is null ? "" : $"{Input.CompactHint(GameAction.Confirm)}: Choose   •   {Input.CompactHint(GameAction.Cancel)}: Back";
        foreach (var button in _buttons) { _column.RemoveChild(button); button.QueueFree(); }
        _buttons.Clear(); _actions.Clear(); _selected = 0;
        foreach (var (label, action) in choices)
        {
            int index = _buttons.Count;
            var button = new Button { Text = label, AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(OverEntry ? 600 : 850, 76), FocusMode = FocusModeEnum.None };
            button.Pressed += () => { Select(index); action(); };
            button.MouseEntered += () => Select(index);
            button.TooltipText = Input is null ? "" : Input.Hint(GameAction.Confirm) + " • Back: " + Input.Hint(GameAction.Cancel);
            _column.AddChild(button); _buttons.Add(button); _actions.Add(action);
        }
        _column.MoveChild(_hint, _column.GetChildCount() - 1);
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
        for (int i = 0; i < _buttons.Count; i++)
            if (OverEntry) StoryTheme.Select(_buttons[i], i == index);
            else _buttons[i].Modulate = i == index ? StoryTheme.Selected : Colors.White;
    }
    public string ReadSaveObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { state = State, message = _message.Text, selected = _selected, choices = _buttons.Select(button => button.Text).ToArray() });
}

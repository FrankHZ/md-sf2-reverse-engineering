using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed partial class SaveSlotView : Control
{
    internal GameInput? Input { get; init; }
    private Label _hint = null!;
    private Label _message = null!;
    private Label _title = null!;
    private Label _detail = null!;
    private PanelContainer _panel = null!;
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
        Theme = StoryTheme.CreateEntry();
        var panel = _panel = new PanelContainer { Name = "SlotWindow" };
        if (OverEntry) { panel.Position = EntryLayout.Window.Position; panel.Size = EntryLayout.Window.Size; AddChild(panel); }
        else
        {
            var shade = new ColorRect { Name = "GameShade", Color = new("051225b8"), MouseFilter = MouseFilterEnum.Stop };
            shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
            var center = new CenterContainer(); center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(center);
            center.AddChild(panel);
        }
        _column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        _column.AddThemeConstantOverride("separation", OverEntry ? 20 : 12); panel.AddChild(_column);
        _title = new Label { Name = "SlotTitle", HorizontalAlignment = HorizontalAlignment.Center, Visible = !OverEntry };
        _title.AddThemeFontSizeOverride("font_size", 34); _column.AddChild(_title);
        _message = new Label { Name = "SlotMessage", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _column.AddChild(_message);
        _hint = new Label { Name = "SlotHint", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        _column.AddChild(_hint);
        _detail = new Label { Name = "SlotDetail", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart, Visible = false };
        _detail.AddThemeFontSizeOverride("font_size", 20);
        _detail.AddThemeColorOverride("font_color", new Color("a7b9cf")); _column.AddChild(_detail);
        Resized += ArrangeWindow; ArrangeWindow();
    }

    private void ArrangeWindow()
    {
        if (!OverEntry) _panel.CustomMinimumSize = new(Math.Min(1040, Math.Max(1, Size.X - 64)), 0);
    }

    internal void ShowDetail(string detail) { _detail.Text = detail; _detail.Show(); }

    internal void ShowChoices(string state, string message, params (string Label, Action Action)[] choices)
    {
        State = state; _message.Text = message;
        _detail.Hide(); _detail.Text = "";
        _title.Text = state switch { "save-slots" => "Save your journey", "confirm-save" => "Confirm save",
            "saved" => "Progress saved", "confirm-quit" => "Leave the game?", "error" => "Unable to save",
            "recover-backup" => "Recover a saved journey", _ => "Saved journeys" };
        _message.AddThemeColorOverride("font_color", state.Contains("error") || state.Contains("uncertain")
            ? new Color("ffc4ad") : Colors.White);
        _hint.Text = Input is null ? "" : $"{Input.CompactHint(GameAction.Confirm)}: Choose   •   {Input.CompactHint(GameAction.Cancel)}: Back";
        foreach (var button in _buttons) { _column.RemoveChild(button); button.QueueFree(); }
        _buttons.Clear(); _actions.Clear(); _selected = 0;
        foreach (var (label, action) in choices)
        {
            int index = _buttons.Count;
            var button = new Button { Text = label, AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(OverEntry ? 600 : 0, 76), FocusMode = FocusModeEnum.None };
            button.Pressed += () => { Select(index); action(); };
            button.MouseEntered += () => Select(index);
            button.TooltipText = Input is null ? "" : Input.Hint(GameAction.Confirm) + " • Back: " + Input.Hint(GameAction.Cancel);
            _column.AddChild(button); _buttons.Add(button); _actions.Add(action);
        }
        _column.MoveChild(_hint, _column.GetChildCount() - 1);
        _column.MoveChild(_detail, _column.GetChildCount() - 1);
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
            StoryTheme.Select(_buttons[i], i == index);
    }
    public string ReadSaveObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { state = State, message = _message.Text, selected = _selected, choices = _buttons.Select(button => button.Text).ToArray() });
}

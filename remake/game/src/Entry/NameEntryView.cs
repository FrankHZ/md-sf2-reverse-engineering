using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed partial class NameEntryView : Control
{
    private LineEdit _edit = null!;
    private Label _error = null!;
    private readonly List<Button> _keys = [];
    private int _selected;
    internal string Draft { get; init; } = "";
    internal Action<string>? Done, Back;
    private static readonly string[] Characters = ("ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
        "abcdefghijklmnopqrstuvwxyz" + "0123456789,.-!?#' ").Select(character => character.ToString()).Concat(["DEL", "END"]).ToArray();

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create();
        var center = new CenterContainer(); center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(center);
        var panel = new PanelContainer { CustomMinimumSize = new(1240, 590) }; center.AddChild(panel);
        var column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center }; column.AddThemeConstantOverride("separation", 20); panel.AddChild(column);
        column.AddChild(new Label { Text = "What is your name?", HorizontalAlignment = HorizontalAlignment.Center });
        _edit = new LineEdit { Name = "NameInput", Text = Draft, PlaceholderText = "Empty keeps Bowie", CustomMinimumSize = new(500, 64),
            Alignment = HorizontalAlignment.Center, ContextMenuEnabled = false, SelectAllOnFocus = true };
        column.AddChild(_edit);
        _edit.TextChanged += _ => Validate();
        _edit.TextSubmitted += _ => Finish();
        column.AddChild(new Label { Text = "Up to 7 characters • letters, digits, , . - ! ? # ' and space\nKeyboard: type / Backspace / Enter • Pad: directions / Confirm • Cancel: Back",
            HorizontalAlignment = HorizontalAlignment.Center });
        var grid = new GridContainer { Columns = 26 }; grid.AddThemeConstantOverride("h_separation", 2); grid.AddThemeConstantOverride("v_separation", 8); column.AddChild(grid);
        for (int index = 0; index < Characters.Length; index++)
        {
            int key = index;
            var button = new Button { Name = "NameKey" + index, Text = Characters[index] == " " ? "␣" : Characters[index],
                CustomMinimumSize = new(44, 62), FocusMode = FocusModeEnum.None };
            button.AddThemeFontSizeOverride("font_size", Characters[index].Length > 1 ? 18 : 28);
            button.Pressed += () => { _edit.ReleaseFocus(); Select(key); Activate(); }; grid.AddChild(button); _keys.Add(button);
        }
        _error = new Label { CustomMinimumSize = new(0, 45), HorizontalAlignment = HorizontalAlignment.Center }; column.AddChild(_error);
        var actions = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center }; column.AddChild(actions);
        AddAction("Back", () => Back?.Invoke(_edit.Text));
        AddAction("Keyboard edit", () => _edit.GrabFocus());
        AddAction("Use name", Finish);
        Select(0); Validate(); _edit.GrabFocus();

        void AddAction(string text, Action action)
        {
            var button = new Button { Text = text, CustomMinimumSize = new(300, 64), FocusMode = FocusModeEnum.None };
            button.Pressed += action; actions.AddChild(button);
        }
    }

    internal bool Editing(InputEvent input)
    {
        if (input is not InputEventKey key || !_edit.HasFocus()) return false;
        if (key.Pressed && !key.Echo && key.Keycode == Key.Escape)
        { var viewport = GetViewport(); Back?.Invoke(_edit.Text); viewport.SetInputAsHandled(); }
        // Leave ordinary typing/caret/deletion/Enter to LineEdit's GUI input.
        return true;
    }
    internal void Handle(GameAction action)
    {
        _edit.ReleaseFocus();
        if (action == GameAction.Cancel) Back?.Invoke(_edit.Text);
        else if (action == GameAction.Confirm) Activate();
        else if (action == GameAction.Left) Select((_selected + Characters.Length - 1) % Characters.Length);
        else if (action == GameAction.Right) Select((_selected + 1) % Characters.Length);
        else if (action == GameAction.Up) Select(_selected < 26 ? Math.Min(_selected + 52, Characters.Length - 1) : _selected - 26);
        else if (action == GameAction.Down) Select(_selected + 26 < Characters.Length ? _selected + 26 : _selected % 26);
    }
    private void Select(int key)
    {
        _selected = key;
        for (int i = 0; i < _keys.Count; i++) _keys[i].Modulate = i == key ? StoryTheme.Selected : Colors.White;
    }
    private void Activate()
    {
        string value = Characters[_selected];
        if (value == "END") Finish();
        else if (value == "DEL") { if (_edit.Text.Length > 0) _edit.Text = _edit.Text[..^1]; Validate(); }
        else
        {
            string candidate = _edit.Text + value;
            if (NewGameName.Error(candidate) is null) _edit.Text = candidate;
            else _error.Text = "Use at most 7 supported characters. DEL removes the last character.";
            if (NewGameName.Error(candidate) is null) Validate();
        }
    }
    private void Validate() => _error.Text = NewGameName.Error(_edit.Text) switch
    { "name-too-long" => "Name is too long. Use at most 7 characters.", "name-character" => "Unsupported character. Use the displayed alphabet.", _ => _edit.Text.Length == 0 ? "The default name will be used." : "" };
    private void Finish()
    {
        Validate();
        if (NewGameName.Error(_edit.Text) is null) Done?.Invoke(_edit.Text);
    }
    public string ReadNameObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { draft = _edit.Text, error = _error.Text, valid = NewGameName.Error(_edit.Text) is null, selected = _selected,
        keyboard = _edit.HasFocus(), alphabet = Characters, maximumLength = NewGameName.MaximumLength });
}

using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed partial class NameEntryView : Control
{
    internal GameInput? Input { get; init; }
    private Label _hint = null!;
    private Button _use = null!;
    private const int Columns = 13;
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
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry();
        var panel = new PanelContainer { Name = "NameWindow", Position = EntryLayout.Window.Position, Size = EntryLayout.Window.Size }; AddChild(panel);
        var column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center }; column.AddThemeConstantOverride("separation", 14); panel.AddChild(column);
        column.AddChild(new Label { Text = "What is your name?", HorizontalAlignment = HorizontalAlignment.Center });
        _edit = new LineEdit { Name = "NameInput", Text = Draft, PlaceholderText = "Empty keeps Bowie", CustomMinimumSize = new(500, 64),
            Alignment = HorizontalAlignment.Center, ContextMenuEnabled = false, SelectAllOnFocus = true };
        column.AddChild(_edit);
        _edit.TextChanged += _ => Validate();
        _edit.TextSubmitted += _ => Finish();
        var help = new Label { Text = "Up to 7 characters • Empty keeps the default name", AutowrapMode = TextServer.AutowrapMode.WordSmart,
            HorizontalAlignment = HorizontalAlignment.Center, TooltipText = "Letters, digits, , . - ! ? # ' and space. Keyboard: type, Backspace, Enter. Pad: directions and Confirm. DEL removes; END finishes." };
        help.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); column.AddChild(help);
        var grid = new GridContainer { Columns = Columns }; grid.AddThemeConstantOverride("h_separation", 2); grid.AddThemeConstantOverride("v_separation", 6); column.AddChild(grid);
        for (int index = 0; index < Characters.Length; index++)
        {
            int key = index;
            var button = new Button { Name = "NameKey" + index, Text = Characters[index] == " " ? "␣" : Characters[index],
                CustomMinimumSize = new(44, 48), FocusMode = FocusModeEnum.None };
            button.AddThemeFontSizeOverride("font_size", Characters[index].Length > 1 ? 16 : 24);
            button.MouseEntered += () => Select(key);
            button.Pressed += () => { _edit.ReleaseFocus(); Select(key); Activate(); }; grid.AddChild(button); _keys.Add(button);
        }
        _error = new Label { Name = "NameError", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(0, 68), HorizontalAlignment = HorizontalAlignment.Center }; column.AddChild(_error);
        var actions = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center }; column.AddChild(actions);
        AddAction("Back", () => Back?.Invoke(_edit.Text));
        AddAction("Keyboard edit", () => _edit.GrabFocus());
        AddAction("Use name", Finish);
        _hint = new Label { Name = "NameHint",
            HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); column.AddChild(_hint);
        Select(0); Validate(); _edit.GrabFocus();

        void AddAction(string text, Action action)
        {
            var button = new Button { Text = text, CustomMinimumSize = new(190, 64), FocusMode = FocusModeEnum.None };
            button.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
            button.Pressed += action; actions.AddChild(button);
            if (text == "Use name") _use = button;
        }
    }

    public override void _Process(double delta)
    {
        // LineEdit's existing editing keys are distinct from mapped grid navigation.
        _hint.Text = _edit.HasFocus() ? "Enter: Use name   •   Escape: Back" : Input is null ? "" :
            $"{Input.CompactHint(GameAction.Confirm)}: Choose   •   {Input.CompactHint(GameAction.Cancel)}: Back";
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
        else if (action == GameAction.Up) Select(_selected < Columns ? Math.Min(_selected + (Characters.Length - 1) / Columns * Columns, Characters.Length - 1) : _selected - Columns);
        else if (action == GameAction.Down) Select(_selected + Columns < Characters.Length ? _selected + Columns : _selected % Columns);
    }
    private void Select(int key)
    {
        _selected = key;
        for (int i = 0; i < _keys.Count; i++) StoryTheme.Select(_keys[i], i == key, key: true);
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
            else { _error.Text = "Use at most 7 supported characters. DEL removes the last character."; _error.Modulate = new("ffc4ad"); }
            if (NewGameName.Error(candidate) is null) Validate();
        }
    }
    private void Validate()
    {
        string? error = NewGameName.Error(_edit.Text);
        _use.Disabled = error is not null;
        _error.Modulate = error is null ? StoryTheme.Selected : new("ffc4ad");
        _error.Text = error switch
        { "name-too-long" => "Name is too long. Use at most 7 characters.", "name-character" => "Unsupported character. Use the displayed alphabet.", _ => _edit.Text.Length == 0 ? "The default name will be used." : "" };
    }
    private void Finish()
    {
        Validate();
        if (NewGameName.Error(_edit.Text) is null) Done?.Invoke(_edit.Text);
    }
    public string ReadNameObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { draft = _edit.Text, error = _error.Text, valid = NewGameName.Error(_edit.Text) is null, selected = _selected,
        keyboard = _edit.HasFocus(), alphabet = Characters, maximumLength = NewGameName.MaximumLength });
}

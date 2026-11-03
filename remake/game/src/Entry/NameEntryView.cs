using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed partial class NameEntryView : Control
{
    private Button _use = null!;
    private LineEdit _edit = null!;
    private Label _error = null!;
    internal string Draft { get; init; } = "";
    internal Action<string>? Done, Back;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry();
        var panel = new PanelContainer { Name = "NameWindow", Position = EntryLayout.Window.Position, Size = EntryLayout.Window.Size }; AddChild(panel);
        var column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center }; column.AddThemeConstantOverride("separation", 20); panel.AddChild(column);
        column.AddChild(new Label { Text = "What is your name?", HorizontalAlignment = HorizontalAlignment.Center });
        _edit = new LineEdit { Name = "NameInput", Text = Draft, PlaceholderText = "Empty keeps Bowie", CustomMinimumSize = new(500, 64),
            Alignment = HorizontalAlignment.Center, ContextMenuEnabled = false };
        column.AddChild(_edit);
        _edit.TextChanged += _ => Validate();
        _edit.TextSubmitted += _ => Finish();
        var help = new Label { Name = "NameHelp", Text = "Keyboard only • Up to 7 characters\nLetters A–Z / a–z, digits 0–9\n, . - ! ? # ' and space\nBackspace/Delete: Remove characters\nEmpty keeps the default name",
            AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center };
        help.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); column.AddChild(help);
        _error = new Label { Name = "NameError", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(0, 68), HorizontalAlignment = HorizontalAlignment.Center }; column.AddChild(_error);
        var actions = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center }; column.AddChild(actions);
        AddAction("Back", () => Back?.Invoke(_edit.Text));
        AddAction("Use name", Finish);
        var hint = new Label { Name = "NameHint", Text = "Enter: Use name   •   Escape: Back",
            HorizontalAlignment = HorizontalAlignment.Center };
        hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); column.AddChild(hint);
        Validate(); FocusEditor(); _edit.SelectAll();
        GetWindow().FocusEntered += FocusEditor;

        void AddAction(string text, Action action)
        {
            var button = new Button { Text = text, CustomMinimumSize = new(250, 64), FocusMode = FocusModeEnum.None };
            button.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
            button.Pressed += action; actions.AddChild(button);
            if (text == "Use name") _use = button;
        }
    }

    public override void _ExitTree() => GetWindow().FocusEntered -= FocusEditor;
    private void FocusEditor() => _edit.GrabFocus();

    internal void HandleKeyboard(InputEventKey key)
    {
        if (key.Keycode == Key.Escape)
        {
            GetViewport().SetInputAsHandled();
            if (key.Pressed && !key.Echo) Back?.Invoke(_edit.Text);
            return;
        }
        // Recover GUI focus before dispatch, even after clicking the backdrop. Ordinary
        // typing/caret/deletion/Enter belong to LineEdit, including mapped gameplay letters.
        if (key.Pressed && !_edit.HasFocus()) FocusEditor();
    }

    private void Validate()
    {
        string? error = NewGameName.Error(_edit.Text);
        _use.Disabled = error is not null;
        _error.Modulate = error is null ? StoryTheme.Selected : new("ffc4ad");
        _error.Text = error switch
        { "name-too-long" => "Name is too long. Use at most 7 characters.", "name-character" => "Unsupported character. Use the letters, digits or punctuation listed above.", _ => _edit.Text.Length == 0 ? "The default name will be used." : "" };
    }
    private void Finish()
    {
        Validate();
        if (NewGameName.Error(_edit.Text) is null) Done?.Invoke(_edit.Text);
    }
    public string ReadNameObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { draft = _edit.Text, error = _error.Text, valid = NewGameName.Error(_edit.Text) is null,
        keyboard = _edit.HasFocus(), inputMode = "keyboard", alphabet = NewGameName.Alphabet, maximumLength = NewGameName.MaximumLength });
}

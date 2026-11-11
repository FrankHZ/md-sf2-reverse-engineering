using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed partial class NameEntryView : Control
{
    private Button _use = null!;
    private LineEdit _edit = null!;
    private Label _error = null!;
    internal string Draft { get; init; } = "";
    internal string? MemberName { get; init; }
    internal Action<string>? Done, Back;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry(journey: true);
        var panel = new PanelContainer { Name = "NameWindow", Position = EntryLayout.Window.Position, Size = EntryLayout.Window.Size }; AddChild(panel);
        var layout = new VBoxContainer(); layout.AddThemeConstantOverride("separation", 24); panel.AddChild(layout);
        var reading = new ScrollContainer { Name = "NameReading", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        layout.AddChild(reading);
        var column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center, SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill }; column.AddThemeConstantOverride("separation", 20); reading.AddChild(column);
        var title = new Label { Text = MemberName is null ? GameText.Get("name.title") : GameText.Get("name.member-title", MemberName),
            AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 32); column.AddChild(title);
        _edit = new LineEdit { Name = "NameInput", Text = Draft, PlaceholderText = GameText.Get("name.empty"), CustomMinimumSize = new(500, 64),
            Alignment = HorizontalAlignment.Center, ContextMenuEnabled = false, KeepEditingOnTextSubmit = true };
        column.AddChild(_edit);
        _edit.TextChanged += _ => Validate();
        _edit.TextSubmitted += _ => Finish();
        var help = new Label { Name = "NameHelp", Text = GameText.Get("name.help"),
            AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center };
        help.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); column.AddChild(help);
        string fallback = string.Join("\n", new[] { GameText.Fallback("name.", includeKey: true),
            GameText.Fallback("entry.back", includeKey: true) }.Where(text => text != ""));
        if (fallback != "") help.Text += "\n" + fallback;
        _error = new Label { Name = "NameError", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(0, 68), HorizontalAlignment = HorizontalAlignment.Center }; column.AddChild(_error);
        var actions = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        actions.AddThemeConstantOverride("separation", 12); layout.AddChild(actions);
        if (Back is not null) AddAction("Back", "entry.back", () => Back.Invoke(_edit.Text));
        _use = AddAction("UseName", "name.use", Finish);
        var hint = new Label { Name = "NameHint", Text = GameText.Get(Back is null ? "name.confirm-hint" : "name.hint"),
            AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center };
        hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); layout.AddChild(hint);
        Validate(); FocusEditor(); _edit.SelectAll();
        GetWindow().FocusEntered += FocusEditor;

        Button AddAction(string name, string key, Action action)
        {
            var button = new Button { Name = name, Text = GameText.Get(key), CustomMinimumSize = new(0, 64),
                AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsHorizontal = SizeFlags.ExpandFill, FocusMode = FocusModeEnum.None };
            button.Pressed += action; actions.AddChild(button);
            return button;
        }
    }

    public override void _ExitTree() => GetWindow().FocusEntered -= FocusEditor;
    private void FocusEditor() { _edit.GrabFocus(); _edit.Edit(); }

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
        if (key.Pressed && (!_edit.HasFocus() || !_edit.IsEditing())) FocusEditor();
    }

    private void Validate()
    {
        string? error = NewGameName.Error(_edit.Text);
        _use.Disabled = error is not null;
        _error.Modulate = error is null ? StoryTheme.Selected : new("ffc4ad");
        _error.Text = error switch
        { "name-too-long" => GameText.Get("name.too-long"), "name-character" => GameText.Get("name.character"), _ => _edit.Text.Length == 0 ? GameText.Get("name.default-value", MemberName ?? DisplayNames.DefaultMember(0)) : "" };
    }
    private void Finish()
    {
        Validate();
        if (NewGameName.Error(_edit.Text) is null) Done?.Invoke(_edit.Text);
    }
    public string ReadNameObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { draft = _edit.Text, error = _error.Text, valid = NewGameName.Error(_edit.Text) is null,
        keyboard = _edit.HasFocus(), editing = _edit.IsEditing(), inputMode = "keyboard", alphabet = NewGameName.Alphabet, maximumLength = NewGameName.MaximumLength });
}

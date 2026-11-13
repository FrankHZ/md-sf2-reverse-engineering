using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.UI;

internal sealed partial class SettingsView : Control
{
    internal required GameInput Input { get; init; }
    internal required Action Close { get; init; }
    internal required Func<InputSettings, bool, string?> Apply { get; init; }
    internal string? Notice { get; init; }
    internal bool ReadOnly { get; init; }
    internal bool BattleOnly { get; init; }
    private InputSettings _draft = null!;
    private bool _reset;
    private ControlsHelpView? _help;
    private enum Page { Root, Display, Assist, Controls, Debug, Actions, Device, Alternatives, Operation, Capture, Review, Battle }
    private Page _page;
    private readonly Stack<(Page Page, string Focus, int Scroll)> _parents = [];
    private record Choice(string Id, string Text, bool Disabled = false, bool Footer = false);
    private List<Choice> _rows = [];
    private int _selected;
    private string _status = "";
    private PanelContainer _panel = null!;
    private VBoxContainer _column = null!;
    private HBoxContainer _footer = null!;
    private ScrollContainer _scroll = null!;
    private Label _title = null!, _message = null!, _hint = null!, _diagnostics = null!;
    private readonly List<Button> _buttons = [];
    private readonly MenuPointer _pointer = new();
    private static string T(string id, params object[] values) => GameText.Get(id, values);
    private static string S(string id, params object[] values) => T("settings." + id, values);
    private static string Toggle(bool value) => T(value ? "ui.settings.on" : "ui.settings.off");

    public override void _Ready()
    {
        AutoTranslateMode = AutoTranslateModeEnum.Disabled;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry(journey: true);
        _draft = Input.Settings.Draft(); _page = BattleOnly ? Page.Battle : Page.Root;
        var shade = new ColorRect { Name = "SettingsShade", Color = new("051225b8") };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        _panel = new PanelContainer { Name = "SettingsWindow" }; AddChild(_panel);
        var layout = new VBoxContainer(); layout.AddThemeConstantOverride("separation", 12); _panel.AddChild(layout);
        _title = new Label { HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart }; layout.AddChild(_title);
        _scroll = new ScrollContainer { Name = "SettingsReading", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }; layout.AddChild(_scroll);
        _column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; _scroll.AddChild(_column);
        _column.AddThemeConstantOverride("separation", 8);
        _message = new Label { Name = "SettingsMessage", AutowrapMode = TextServer.AutowrapMode.WordSmart }; _column.AddChild(_message);
        _diagnostics = new Label { Name = "SettingsDiagnostics", AutowrapMode = TextServer.AutowrapMode.WordSmart }; _column.AddChild(_diagnostics);
        _footer = new HBoxContainer { Name = "SettingsActions" }; layout.AddChild(_footer);
        _footer.AddThemeConstantOverride("separation", 12);
        _hint = new Label { Name = "SettingsHint", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); layout.AddChild(_hint);
        SeedCaptureControls(); Resized += Arrange; Arrange(); ShowPage(); GetWindow().SizeChanged += SuspendPointer;
    }

    public override void _ExitTree() => GetWindow().SizeChanged -= SuspendPointer;

    private void Arrange()
    {
        SuspendPointer();
        _panel.Size = new(Math.Min(960, Math.Max(1, Size.X - 64)), Math.Min(920, Math.Max(1, Size.Y - 64)));
        _panel.Position = (Size - _panel.Size) / 2;
    }

    internal void SuspendPointer() { _pointer.Suspend(); _help?.SuspendPointer(); }
    public override void _Process(double delta)
    {
        if (!GetWindow().HasFocus() || !IsVisibleInTree()) { _pointer.Suspend(); return; }
        _pointer.ReleaseIfNeutral();
    }

    private Choice BackRow() => new("back", T(_page == Page.Root ? ReadOnly ? "ui.settings.close" : "ui.settings.cancel" : "entry.back"), Footer: true);
    private List<Choice> Choices() => _page switch
    {
        Page.Root => [new("display", S("display")), new("assist", S("assist")), new("input", S("input")), new("debug", S("debug")),
            new("apply", T("ui.settings.apply"), ReadOnly || IncompleteAction(_draft) != null, true), BackRow()],
        Page.Display => [new("language", T("ui.settings.language", _draft.Language == "en" ? "English" : "简体中文"), ReadOnly),
            new("graphics", T("ui.settings.graphics", T("ui.settings.graphics-" + _draft.GraphicsMode)), ReadOnly),
            new("text", T("ui.settings.text", T(_draft.TextMode == "instant" ? "ui.settings.instant" : "ui.settings.gradual")), ReadOnly),
            new("speed", T("ui.settings.speed", _draft.CharactersPerSecond), ReadOnly), BackRow()],
        Page.Assist => [new("flash", T("ui.settings.flash", Toggle(_draft.ReducedFlash)), ReadOnly),
            new("guidance", T("ui.settings.guidance", Toggle(_draft.ModernGuidance)), ReadOnly),
            new("defaults", T("ui.settings.defaults"), ReadOnly), BackRow()],
        Page.Debug => [new("diagnostics", T("ui.settings.debug", Toggle(_draft.ShowDebugDiagnostics)), ReadOnly),
            new("victory", T("ui.settings.auto-victory", Toggle(_draft.DebugAutoVictory)), ReadOnly), BackRow()],
        Page.Controls => [new("convention", T("ui.settings.confirm", T(_draft.ConfirmCancel == "standard" ? "ui.settings.standard" : "ui.settings.swapped")), ReadOnly),
            new("edit", S("edit"), ReadOnly), new("help", T("ui.settings.help")), new("reset", S("reset"), ReadOnly), BackRow()],
        Page.Battle => [new("battle-speed", T("battle.speed.value", _draft.BattleSpeed?.ToString() ?? T("battle.speed.custom", _draft.CharactersPerSecond))),
            new("messages", T("battle.speed.messages", Toggle(!_draft.SuppressBattleHitCoinMessages)))],
        _ => EditorRows()
    };

    private void Enter(Page page)
    {
        _parents.Push((_page, _rows[_selected].Id, _scroll.ScrollVertical)); _page = page;
        _status = ""; ShowPage();
    }

    private void Back()
    {
        if (!_parents.TryPop(out var parent)) { Close(); return; }
        _page = parent.Page; _status = ""; ShowPage(parent.Focus, parent.Scroll);
    }

    private void ShowPage(string? focus = null, int scroll = 0)
    {
        _pointer.Suspend();
        foreach (var button in _buttons) { button.GetParent().RemoveChild(button); button.QueueFree(); }
        _buttons.Clear(); _rows = Choices();
        _selected = _rows.FindIndex(row => row.Id == focus && !row.Disabled);
        if (_selected < 0) _selected = _rows.FindIndex(row => !row.Disabled);
        foreach (var row in _rows)
        {
            var button = new Button { Name = "Setting_" + row.Id, Text = row.Text, Disabled = row.Disabled,
                CustomMinimumSize = new(0, 64), SizeFlagsHorizontal = SizeFlags.ExpandFill,
                FocusMode = FocusModeEnum.None, MouseFilter = MouseFilterEnum.Ignore, AutowrapMode = TextServer.AutowrapMode.WordSmart };
            StoryTheme.EntryChoice(button); (row.Footer ? (Container)_footer : _column).AddChild(button); _buttons.Add(button);
        }
        _footer.Visible = _rows.Any(r => r.Footer); _scroll.ScrollVertical = scroll;
        Paint();
        // Scroll ranges settle after the new rows have been measured.
        if (scroll > 0) _scroll.SetDeferred(ScrollContainer.PropertyName.ScrollVertical, scroll);
    }

    private void Paint()
    {
        for (int i = 0; i < _rows.Count; i++) StoryTheme.Select(_buttons[i], i == _selected);
        _title.Text = _page switch { Page.Root => T("ui.settings.title"), Page.Display => S("display"),
            Page.Assist => S("assist"), Page.Controls => S("input"), Page.Debug => S("debug"), Page.Battle => T("battle.speed.title"),
            Page.Actions => S("edit"), _ => S(_action) };
        _message.Text = _status != "" ? _status : _page switch {
            Page.Root => Notice ?? (IncompleteAction(_draft) is { } name ? S("required", S(name)) : ""),
            Page.Display when _rows[_selected].Id == "graphics" => T("ui.settings.graphics-coverage"),
            Page.Battle => Notice ?? T("battle.speed.mapping"), _ => EditorMessage() };
        _message.Visible = _message.Text.Length != 0;
        _diagnostics.Visible = _page == Page.Debug && Input.Settings.ShowDebugDiagnostics; _diagnostics.Text = GameText.Coverage;
        _hint.Text = BattleOnly ? T("battle.speed.hint", Input.CompactHint(GameAction.Left), Input.CompactHint(GameAction.Right),
                Input.CompactHint(GameAction.Attack), Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel)) :
            T("ui.settings.hint", Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel));
    }

    internal void RefreshInputHints()
    {
        if (_page == Page.Alternatives)
        {
            _rows = Choices();
            for (int i = 0; i < _rows.Count; i++) _buttons[i].Text = _rows[i].Text;
        }
        Paint(); _help?.RefreshInputHints();
    }

    internal void Handle(GameAction action, InputEvent input)
    {
        _pointer.Suspend();
        if (_help is not null) { _help.Handle(action); return; }
        if (action == GameAction.Cancel) { Back(); return; }
        if (action is GameAction.Up or GameAction.Down)
        {
            int step = action == GameAction.Up ? -1 : 1;
            do { _selected = (_selected + step + _rows.Count) % _rows.Count; } while (_rows[_selected].Disabled);
            Paint(); if (!_rows[_selected].Footer) _scroll.EnsureControlVisible(_buttons[_selected]); return;
        }
        if (BattleOnly && action is GameAction.Confirm or GameAction.Attack) { Save(); return; }
        if (action == GameAction.Confirm) Activate(1, input);
        else if (action is GameAction.Left or GameAction.Right)
        {
            if (IsValue(_rows[_selected].Id)) Activate(action == GameAction.Left ? -1 : 1, input);
            else _scroll.ScrollVertical += action == GameAction.Left ? -112 : 112;
        }
    }

    private static bool IsValue(string id) => id is "language" or "graphics" or "text" or "speed" or "convention" or
        "flash" or "guidance" or "diagnostics" or "victory" or "battle-speed" or "messages";

    internal void HandlePointer(InputEvent input)
    {
        if (_help is not null) { _help.HandlePointer(input); return; }
        if (!GetWindow().HasFocus()) { _pointer.Suspend(); return; }
        var pos = ((InputEventMouse)input).Position;
        Rect2 Bounds(int i) => _rows[i].Footer ? _buttons[i].GetGlobalRect() : _scroll.GetGlobalRect().Intersection(_buttons[i].GetGlobalRect());
        int hit = -1;
        for (int i = 0; i < _buttons.Count; i++)
            if (!_buttons[i].Disabled && Bounds(i).HasPoint(pos)) { hit = i; break; }
        if (input is InputEventMouseMotion motion && _pointer.Moved(motion) && hit >= 0)
        { _selected = hit; Paint(); }
        if (input is not InputEventMouseButton button) return;
        if (button.ButtonIndex is MouseButton.WheelDown or MouseButton.WheelUp)
        { if (button.Pressed) _scroll.ScrollVertical += button.ButtonIndex == MouseButton.WheelDown ? 80 : -80; _pointer.Cancel(); return; }
        bool back = button.ButtonIndex == MouseButton.Right;
        string? id = back ? "right-click" : hit < 0 ? null : _rows[hit].Id;
        if (_pointer.Read(button, id, back ? GetGlobalRect() : hit < 0 ? new() : Bounds(hit), _page) == null) return;
        if (back) Back(); else { _selected = hit; Activate(1, input); }
    }

    private void Activate(int direction, InputEvent input)
    {
        var row = _rows[_selected]; if (row.Disabled) return;
        _status = "";
        switch (row.Id)
        {
            case "back": Back(); return;
            case "display": Enter(Page.Display); return;
            case "input": Enter(Page.Controls); return;
            case "assist": Enter(Page.Assist); return;
            case "debug": Enter(Page.Debug); return;
            case "edit": Enter(Page.Actions); return;
            case "help":
                _pointer.Suspend(); _panel.Hide(); _help = new ControlsHelpView { Name = "ControlsHelpView", Input = Input, Back = CloseHelp };
                AddChild(_help); return;
            case "apply": Save(); return;
            case "defaults": _draft = _draft.Draft(defaults: true); _reset = true; _status = T("ui.settings.staged"); break;
            case "reset": _draft.Bindings.Clear(); break;
            case "text": _draft.TextMode = _draft.TextMode == "instant" ? "adjustable" : "instant"; break;
            case "speed": _draft.CharactersPerSecond = Math.Clamp(_draft.CharactersPerSecond + direction * 10, 1, 240); break;
            case "convention": _draft.ConfirmCancel = _draft.ConfirmCancel == "standard" ? "swapped" : "standard"; break;
            case "flash": _draft.ReducedFlash = !_draft.ReducedFlash; break;
            case "language": _draft.Language = _draft.Language == "en" ? "zh-CN" : "en"; break;
            case "graphics": _draft.GraphicsMode = _draft.GraphicsMode == "original" ? "hd" : "original"; break;
            case "guidance": _draft.ModernGuidance = !_draft.ModernGuidance; break;
            case "diagnostics": _draft.ShowDebugDiagnostics = !_draft.ShowDebugDiagnostics; break;
            case "victory": _draft.DebugAutoVictory = !_draft.DebugAutoVictory; break;
            case "battle-speed": if (!ReadOnly) _draft.ChangeBattleSpeed(direction); break;
            case "messages": if (!ReadOnly) _draft.SuppressBattleHitCoinMessages = !_draft.SuppressBattleHitCoinMessages; break;
            default: EditorActivate(row.Id, input); return;
        }
        ShowPage(row.Id, _scroll.ScrollVertical);
    }

    private void Save()
    {
        if (ReadOnly) { Close(); return; }
        _status = Apply(_draft, _reset) ?? "";
        if (_status == "") Close(); else Paint();
    }
    private void CloseHelp()
    {
        if (_help is not { } help) return;
        _help = null; RemoveChild(help); help.QueueFree(); _pointer.Suspend(); _panel.Show(); Paint();
    }

    public string ReadSettingsObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { draft = _draft, applied = Input.Settings, selected = _selected, focus = _rows[_selected].Id, page = _page.ToString(), rows = _rows.Select(r => r.Id),
        readOnly = ReadOnly, battleOnly = BattleOnly, reset = _reset, message = _message.Text,
        locale = GameText.Language, coverage = GameText.Coverage, diagnostics = GameText.Diagnostics,
        debugVisible = _diagnostics.IsVisibleInTree(), debugText = _diagnostics.Text,
        labels = _buttons.Select(button => button.Text), help = _help is not null, scroll = _scroll.ScrollVertical, bindings = Enum.GetValues<GameAction>().ToDictionary(action => action.ToString(), Input.Hint) });
}

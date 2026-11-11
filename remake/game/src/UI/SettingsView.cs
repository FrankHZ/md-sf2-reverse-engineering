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
    private int _selected;
    private string _status = "";
    private PanelContainer _panel = null!;
    private ScrollContainer _scroll = null!;
    private Label _message = null!, _hint = null!;
    private readonly List<Button> _buttons = [];
    private readonly MenuPointer _pointer = new();
    private readonly object _boundary = new();

    public override void _Ready()
    {
        AutoTranslateMode = AutoTranslateModeEnum.Disabled;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry(journey: true);
        _draft = Input.Settings.Draft();
        var shade = new ColorRect { Name = "SettingsShade", Color = new("051225b8") };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        _panel = new PanelContainer { Name = "SettingsWindow" }; AddChild(_panel);
        var layout = new VBoxContainer(); layout.AddThemeConstantOverride("separation", 12); _panel.AddChild(layout);
        layout.AddChild(new Label { Text = GameText.Get(BattleOnly ? "battle.speed.title" : "ui.settings.title"), HorizontalAlignment = HorizontalAlignment.Center });
        _scroll = new ScrollContainer { Name = "SettingsReading", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        layout.AddChild(_scroll);
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", 12); _scroll.AddChild(column);
        _message = new Label { Name = "SettingsMessage", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        column.AddChild(_message);
        var actions = new HBoxContainer { Name = "SettingsActions" }; actions.AddThemeConstantOverride("separation", 12);
        for (int i = 0; i < (BattleOnly ? 2 : 11); i++)
        {
            var button = new Button { Name = "Setting" + i, CustomMinimumSize = new(0, 64), SizeFlagsHorizontal = SizeFlags.ExpandFill,
                FocusMode = FocusModeEnum.None, MouseFilter = MouseFilterEnum.Ignore,
                AutowrapMode = TextServer.AutowrapMode.WordSmart, Disabled = ReadOnly && !BattleOnly && (i < 8 || i == 10) };
            StoryTheme.EntryChoice(button);
            if (!BattleOnly && i is >= 7 and <= 9) actions.AddChild(button); else column.AddChild(button);
            _buttons.Add(button);
            if (!BattleOnly && (i < 6 || i == 10))
            {
                var description = new Label { Name = "SettingDescription" + i, Text = GameText.Get("host.setting-description-" + i),
                    AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore };
                description.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); column.AddChild(description);
            }
        }
        if (!BattleOnly)
        {
            column.MoveChild(_buttons[10], _buttons[6].GetIndex());
            column.MoveChild(column.GetNode("SettingDescription10"), _buttons[10].GetIndex() + 1);
        }
        layout.AddChild(actions); actions.Visible = !BattleOnly;
        _hint = new Label { Name = "SettingsHint", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); layout.AddChild(_hint);
        _selected = ReadOnly && !BattleOnly ? 8 : 0;
        Resized += Arrange; Arrange(); Refresh();
        GetWindow().SizeChanged += SuspendPointer;
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

    internal void Handle(GameAction action)
    {
        _pointer.Suspend();
        if (_help is not null) { _help.Handle(action); return; }
        if (action == GameAction.Cancel) { Close(); return; }
        if (BattleOnly)
        {
            if (action is GameAction.Up or GameAction.Down) { _selected = 1 - _selected; Refresh(); }
            else if (action is GameAction.Attack or GameAction.Confirm) AcceptBattle();
            else if (!ReadOnly && action is GameAction.Left or GameAction.Right) ChangeBattle(action == GameAction.Left ? -1 : 1);
            return;
        }
        if (action is GameAction.Up or GameAction.Down)
        {
            int step = action == GameAction.Up ? -1 : 1;
            // General rows precede the fixed actions; graphics and host action identities stay stable.
            int[] order = [0, 1, 2, 3, 4, 5, 10, 6, 7, 8, 9];
            do { _selected = order[(Array.IndexOf(order, _selected) + step + order.Length) % order.Length]; } while (_buttons[_selected].Disabled);
            Refresh(); if (_selected < 7 || _selected == 10) _scroll.EnsureControlVisible(_buttons[_selected]);
        }
        else if (action == GameAction.Confirm) Activate(1);
        else if (action is GameAction.Left or GameAction.Right && (_selected < 6 || _selected == 10)) Activate(action == GameAction.Left ? -1 : 1);
        else if (action is GameAction.Left or GameAction.Right)
            _scroll.ScrollVertical += action == GameAction.Left ? -112 : 112;
    }

    internal void HandlePointer(InputEvent input)
    {
        if (_help is not null) { _help.HandlePointer(input); return; }
        if (!GetWindow().HasFocus()) { _pointer.Suspend(); return; }
        int hit = _buttons.FindIndex(button => button.IsVisibleInTree() && !button.Disabled &&
            (button.GetParent()?.Name == "SettingsActions" ? button.GetGlobalRect() :
                _scroll.GetGlobalRect().Intersection(button.GetGlobalRect())).HasPoint(((InputEventMouse)input).Position));
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
        if (BattleOnly) { if (!ReadOnly) ChangeBattle(direction); return; }
        if (_buttons[_selected].Disabled) return;
        switch (_selected)
        {
            case 0: _draft.TextMode = _draft.TextMode == "instant" ? "adjustable" : "instant"; break;
            case 1: _draft.CharactersPerSecond = Math.Clamp(_draft.CharactersPerSecond + direction * 10, 1, 240); break;
            case 2: _draft.ConfirmCancel = _draft.ConfirmCancel == "standard" ? "swapped" : "standard"; break;
            case 3: _draft.ReducedFlash = !_draft.ReducedFlash; break;
            case 4: _draft.Language = _draft.Language == "en" ? "zh-CN" : "en"; break;
            case 5: _draft.GraphicsMode = _draft.GraphicsMode == "original" ? "hd" : "original"; break;
            case 10: _draft.ModernGuidance = !_draft.ModernGuidance; break;
            case 6: _draft = Input.Settings.Draft(defaults: true); _reset = true; _status = GameText.Get("ui.settings.staged"); break;
            case 7:
                _status = Apply(_draft, _reset) ?? "";
                if (_status == "") { Close(); return; }
                break;
            case 8: Close(); return;
            case 9:
                _pointer.Suspend(); _panel.Hide();
                _help = new ControlsHelpView { Name = "ControlsHelpView", Input = Input, Back = CloseHelp };
                AddChild(_help); break;
        }
        Refresh();
    }

    private void ChangeBattle(int direction)
    {
        if (_selected == 0) _draft.ChangeBattleSpeed(direction);
        else _draft.SuppressBattleHitCoinMessages = !_draft.SuppressBattleHitCoinMessages;
        Refresh();
    }

    private void AcceptBattle()
    {
        if (ReadOnly) { Close(); return; }
        _status = Apply(_draft, false) ?? "";
        if (_status == "") Close(); else Refresh();
    }

    private void CloseHelp()
    {
        if (_help is not { } help) return;
        _help = null; RemoveChild(help); help.QueueFree();
        _pointer.Suspend(); _panel.Show(); Refresh();
    }

    internal void RefreshInputHints() { Refresh(); _help?.RefreshInputHints(); }

    private void Refresh()
    {
        if (BattleOnly)
        {
            string value = _draft.BattleSpeed is { } speed ? speed.ToString() : GameText.Get("battle.speed.custom", _draft.CharactersPerSecond);
            _buttons[0].Text = GameText.Get("battle.speed.value", value);
            _buttons[1].Text = GameText.Get("battle.speed.messages", GameText.Get(_draft.SuppressBattleHitCoinMessages ? "ui.settings.off" : "ui.settings.on"));
            for (int i = 0; i < 2; i++) StoryTheme.Select(_buttons[i], i == _selected);
            _message.Text = _status != "" ? _status : Notice ?? GameText.Get("battle.speed.mapping");
            _hint.Text = GameText.Get("battle.speed.hint", Input.CompactHint(GameAction.Left), Input.CompactHint(GameAction.Right),
                Input.CompactHint(GameAction.Attack), Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel));
            return;
        }
        string[] labels = [GameText.Get("ui.settings.text", GameText.Get(_draft.TextMode == "instant" ? "ui.settings.instant" : "ui.settings.gradual")),
            GameText.Get("ui.settings.speed", _draft.CharactersPerSecond),
            GameText.Get("ui.settings.confirm", GameText.Get(_draft.ConfirmCancel == "standard" ? "ui.settings.standard" : "ui.settings.swapped")),
            GameText.Get("ui.settings.flash", GameText.Get(_draft.ReducedFlash ? "ui.settings.on" : "ui.settings.off")),
            GameText.Get("ui.settings.language", _draft.Language == "en" ? "English" : "简体中文"),
            GameText.Get("ui.settings.graphics", GameText.Get("ui.settings.graphics-" + _draft.GraphicsMode)),
            GameText.Get("ui.settings.defaults"), GameText.Get("ui.settings.apply"),
            GameText.Get(ReadOnly ? "ui.settings.close" : "ui.settings.cancel"), GameText.Get("ui.settings.help"),
            GameText.Get("ui.settings.guidance", GameText.Get(_draft.ModernGuidance ? "ui.settings.on" : "ui.settings.off"))];
        for (int i = 0; i < labels.Length; i++) { _buttons[i].Text = labels[i]; StoryTheme.Select(_buttons[i], i == _selected); }
        _message.Text = _status != "" ? _status : Notice ?? GameText.Get("ui.settings.notice") + "\n" + GameText.Coverage;
        _message.Text += "\n" + GameText.Get("ui.settings.graphics-coverage");
        _hint.Text = GameText.Get("ui.settings.hint", Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel));
        _hint.Text += "\n" + GameText.Get("host.settings-reading", Input.CompactHint(GameAction.Left), Input.CompactHint(GameAction.Right));
    }

    public string ReadSettingsObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { draft = _draft, applied = Input.Settings, selected = _selected, readOnly = ReadOnly, battleOnly = BattleOnly, reset = _reset, message = _message.Text,
        locale = GameText.Language, coverage = GameText.Coverage, diagnostics = GameText.Diagnostics, labels = _buttons.Select(button => button.Text), help = _help is not null, scroll = _scroll.ScrollVertical, bindings = Enum.GetValues<GameAction>().ToDictionary(action => action.ToString(), Input.Hint) });
}

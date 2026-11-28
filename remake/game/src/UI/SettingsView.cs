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
    internal bool BattleContext { get; init; }
    private InputSettings _draft = null!;
    private bool _reset;
    private ControlsHelpView? _help;
    private enum Page { Root, Display, Assist, Controls, Debug, Audio, Actions, Device, Alternatives, Operation, Capture, Review, Battle }
    private Page _page;
    private readonly Stack<(Page Page, string Focus, int Scroll)> _parents = [];
    private record Choice(string Id, string Text, bool Disabled = false, bool Footer = false);
    private SettingsChoice? Focused => GetViewport().GuiGetFocusOwner() as SettingsChoice;
    private string _status = "";
    private PanelContainer _panel = null!;
    private VBoxContainer _column = null!;
    private HBoxContainer _footer = null!;
    private ScrollContainer _scroll = null!, _details = null!;
    private Label _title = null!, _message = null!, _hint = null!, _diagnostics = null!, _detailsHint = null!;
    private readonly List<SettingsChoice> _buttons = [];
    private readonly MenuPointer _pointer = new();
    private static string T(string id, params object[] values) => GameText.Get(id, values);
    private static string S(string id, params object[] values) => T("settings." + id, values);
    private static string Toggle(bool value) => T(value ? "ui.settings.on" : "ui.settings.off");

    public override void _Ready()
    {
        AutoTranslateMode = AutoTranslateModeEnum.Disabled;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry(journey: true);
        _draft = Input.Settings.Draft(); _page = BattleOnly ? Page.Battle : Page.Root;
        MouseFilter = MouseFilterEnum.Stop; MouseForcePassScrollEvents = false;
        var shade = new ColorRect { Name = "SettingsShade", MouseFilter = MouseFilterEnum.Ignore, Color = new("051225b8") };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        var center = new CenterContainer { MouseFilter = MouseFilterEnum.Ignore };
        AddChild(center); center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _panel = new PanelContainer { Name = "SettingsWindow" }; center.AddChild(_panel);
        var layout = new VBoxContainer(); layout.AddThemeConstantOverride("separation", 12); _panel.AddChild(layout);
        _title = new Label { HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart }; layout.AddChild(_title);
        _scroll = new ScrollContainer { Name = "SettingsReading", FollowFocus = true, SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }; layout.AddChild(_scroll);
        _column = new VBoxContainer { Theme = StoryTheme.CreateEntry(journey: true, groupedRows: true), SizeFlagsHorizontal = SizeFlags.ExpandFill }; _scroll.AddChild(_column);
        _column.AddThemeConstantOverride("separation", 8);
        // Fix the details area so its text and scroll range cannot move the choices or footer.
        var detailsPane = new VBoxContainer { CustomMinimumSize = new(0, 168) };
        layout.AddChild(detailsPane);
        _details = new ScrollContainer { Name = "SettingsDetails", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        detailsPane.AddChild(_details);
        var detailsText = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _details.AddChild(detailsText);
        _message = new Label { Name = "SettingsMessage", AutowrapMode = TextServer.AutowrapMode.WordSmart }; detailsText.AddChild(_message);
        _diagnostics = new Label { Name = "SettingsDiagnostics", AutowrapMode = TextServer.AutowrapMode.WordSmart }; detailsText.AddChild(_diagnostics);
        _detailsHint = new Label { Name = "SettingsDetailsHint", CustomMinimumSize = new(0, 36) };
        _detailsHint.ThemeTypeVariation = ReadingSize.Role(StoryTheme.HintSize);
        detailsPane.AddChild(_detailsHint);
        _details.GetVScrollBar().VisibilityChanged += RefreshDetailsHint;
        _footer = new HBoxContainer { Name = "SettingsActions" }; layout.AddChild(_footer);
        _footer.AddThemeConstantOverride("separation", 12);
        _hint = new Label { Name = "SettingsHint", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.ThemeTypeVariation = ReadingSize.Role(StoryTheme.HintSize); layout.AddChild(_hint);
        ReadingSize.Follow(this, large =>
        {
            ReadingSize.Configure(Theme, large);
            ReadingSize.Configure(_column.Theme, large);
            _pointer.Suspend();
        });
        SeedCaptureControls(); Resized += Arrange; Arrange(); ShowPage();
    }

    public override void _EnterTree()
    {
        GetWindow().SizeChanged += SuspendPointer;
        GetWindow().MouseExited += SuspendPointer;
    }

    public override void _ExitTree()
    {
        GetWindow().SizeChanged -= SuspendPointer;
        GetWindow().MouseExited -= SuspendPointer;
    }

    private void Arrange()
    {
        SuspendPointer();
        _panel.CustomMinimumSize = new(Math.Min(960, Math.Max(1, Size.X - 64)), Math.Min(920, Math.Max(1, Size.Y - 64)));
    }

    internal void SuspendPointer() => _pointer.Suspend();
    public override void _Process(double delta)
    {
        if (!GetWindow().HasFocus() || !IsVisibleInTree()) { _pointer.Suspend(); return; }
        _pointer.ReleaseIfNeutral();
    }

    private List<Choice> PageChoices() => _page switch
    {
        Page.Root => [new("display", S("display")), new("assist", S("assist")), new("input", S("input")), new("debug", S("debug")),
            new("audio", S("audio")),
            new("apply", T("ui.settings.apply"), ReadOnly || IncompleteAction(_draft) != null, true)],
        Page.Display => [new("language", T("ui.settings.language", _draft.Language == "en" ? "English" : "简体中文"), ReadOnly),
            new("graphics", T("ui.settings.graphics", T("ui.settings.graphics-" + _draft.GraphicsMode)), ReadOnly),
            new("text", T("ui.settings.text", T(_draft.TextMode == "instant" ? "ui.settings.instant" : "ui.settings.gradual")), ReadOnly),
            new("speed", T("ui.settings.speed", _draft.CharactersPerSecond), ReadOnly),
            new("large-text", T("ui.settings.large-text", Toggle(_draft.LargeText)), ReadOnly)],
        Page.Audio => [
            new("music-volume", S("music-volume", _draft.MusicVolume), ReadOnly),
            new("system-effects-volume", S("system-effects-volume", _draft.SystemEffectsVolume), ReadOnly),
            new("battle-effects-volume", S("battle-effects-volume", _draft.BattleEffectsVolume), ReadOnly)],
        Page.Assist => [new("flash", T("ui.settings.flash", Toggle(_draft.ReducedFlash)), ReadOnly),
            new("guidance", T("ui.settings.guidance", Toggle(_draft.ModernGuidance)), ReadOnly),
            new("acceleration", T("ui.settings.acceleration", _draft.AccelerationMultiplier), ReadOnly),
            new("background", T("ui.settings.background", Toggle(_draft.RunInBackground)), ReadOnly)],
        Page.Debug => [new("diagnostics", T("ui.settings.debug", Toggle(_draft.ShowDebugDiagnostics)), ReadOnly),
            new("victory", T("ui.settings.auto-victory", Toggle(_draft.DebugAutoVictory)), ReadOnly)],
        Page.Controls => [new("convention", T("ui.settings.confirm", T(_draft.ConfirmCancel == "standard" ? "ui.settings.standard" : "ui.settings.swapped")), ReadOnly),
            new("edit", S("edit"), ReadOnly), new("help", T("ui.settings.help"))],
        Page.Battle => [new("battle-speed", T("battle.speed.value", _draft.BattleSpeed?.ToString() ?? T("battle.speed.custom", _draft.CharactersPerSecond)), ReadOnly),
            new("messages", T("battle.speed.messages", Toggle(!_draft.SuppressBattleHitCoinMessages)), ReadOnly)],
        _ => EditorRows()
    };

    private void Enter(Page page)
    {
        _parents.Push((_page, Focused!.Id, _scroll.ScrollVertical)); _page = page;
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
        _buttons.Clear();
        var rows = Choices();
        bool readingOnly = rows.All(row => row.Disabled);
        foreach (var row in rows)
        {
            var button = new SettingsChoice { Id = row.Id, Gesture = _pointer, Clip = row.Footer ? null : _scroll,
                Activate = input => Activate(row.Id, 1, input, cycle: true), Name = "Setting_" + row.Id,
                Text = row.Text, Disabled = row.Disabled, CustomMinimumSize = new(0, 64),
                SizeFlagsHorizontal = SizeFlags.ExpandFill, AutowrapMode = TextServer.AutowrapMode.WordSmart,
                FocusMode = row.Disabled && !readingOnly ? FocusModeEnum.None : FocusModeEnum.All };
            button.FocusEntered += Paint;
            (row.Footer ? (Container)_footer : _column).AddChild(button);
            _buttons.Add(button);
        }
        _footer.Visible = rows.Any(row => row.Footer);
        var stops = _buttons.Where(b => b.FocusMode == FocusModeEnum.All).ToArray();
        for (int i = 0; i < stops.Length; i++)
        {
            stops[i].FocusNeighborTop = stops[i].GetPathTo(stops[(i + stops.Length - 1) % stops.Length]);
            stops[i].FocusNeighborBottom = stops[i].GetPathTo(stops[(i + 1) % stops.Length]);
        }
        var initial = stops.FirstOrDefault(button => button.Id == focus) ?? stops[0];
        initial.GrabFocus();
        // Replacing rows can leave the viewport at the same size with a different content range.
        _scroll.UpdateMinimumSize();
        _scroll.QueueSort();
        _scroll.ScrollVertical = scroll;
        if (scroll > 0)
            Callable.From(() =>
            {
                if (GodotObject.IsInstanceValid(initial) && initial.IsInsideTree() && initial.HasFocus())
                    _scroll.ScrollVertical = scroll;
            }).CallDeferred();
    }

    private void Paint()
    {
        var id = Focused?.Id;
        _title.Text = _page switch { Page.Root => T("ui.settings.title"), Page.Display => S("display"),
            Page.Assist => S("assist"), Page.Controls => S("input"), Page.Debug => S("debug"), Page.Battle => T("battle.speed.title"),
            Page.Audio => S("audio"), Page.Actions => S("edit"), _ => S(_action) };
        string message = _status != "" ? _status : id == "reset-page" ? ResetDescription : _page switch {
            Page.Root => Notice ?? (IncompleteAction(_draft) is { } name ? S("required", S(name)) : ""),
            Page.Display when id == "graphics" =>
                T(BattleContext ? "ui.settings.battle-graphics" : "ui.settings.graphics-coverage"),
            Page.Display when BattleContext && id == "language" => T("ui.settings.battle-language"),
            Page.Display when BattleContext && id == "text" => T("ui.settings.battle-text"),
            Page.Assist when id == "guidance" => T("host.setting-description-10"),
            Page.Assist when id == "background" => T("ui.settings.background-detail"),
            Page.Assist when id == "acceleration" => T("ui.settings.acceleration-detail", Input.CompactHint(GameAction.Accelerate)),
            Page.Audio => ReadOnly ? S("audio-muted") : S("audio-detail",
                Input.CompactHint(GameAction.Left), Input.CompactHint(GameAction.Right), Input.CompactHint(GameAction.Confirm)),
            Page.Debug when BattleContext && id == "victory" => T("ui.settings.battle-auto-victory"),
            Page.Battle => Notice ?? T("battle.speed.mapping"), _ => EditorMessage() };
        bool diagnostics = _page == Page.Debug && Input.Settings.ShowDebugDiagnostics;
        if (_message.Text != message || _diagnostics.Visible != diagnostics) _details.ScrollVertical = 0;
        _message.Text = message;
        _message.Visible = message.Length != 0;
        _diagnostics.Visible = diagnostics; _diagnostics.Text = GameText.Coverage;
        RefreshDetailsHint();
        _hint.Text = BattleOnly && id != "reset-page" ? T("battle.speed.hint", Input.CompactHint(GameAction.Left), Input.CompactHint(GameAction.Right),
                Input.CompactHint(GameAction.Attack), Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel)) :
            T("ui.settings.hint", Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel));
    }

    private void RefreshDetailsHint() => _detailsHint.Text = _details.GetVScrollBar().Visible
        ? T("ui.settings.read-details", Input.CompactHint(GameAction.Target)) : "";

    internal void RefreshInputHints()
    {
        RefreshCaptions();
        Paint(); _help?.RefreshInputHints();
    }

    internal void SetPreviewReadingSize(bool large)
    {
        _draft.LargeText = large;
        RefreshCaptions();
    }

    private void RefreshCaptions()
    {
        foreach (var (button, row) in _buttons.Zip(Choices()))
            button.Text = row.Text;
    }

    internal bool BackFromDebug()
    {
        if (_page is Page.Capture or Page.Review) return false;
        if (_help is not null) _help.Handle(GameAction.Cancel);
        else Back();
        return true;
    }

    internal bool HandleFromDebug(GameAction action)
    {
        if (action == GameAction.Cancel) return BackFromDebug();
        if (_help is not null || _page is not (Page.Root or Page.Display)) return false;
        if (action is not (GameAction.Up or GameAction.Down) &&
            FocusId is not ("display" or "apply" or "language" or "graphics")) return false;
        Handle(action, null);
        return true;
    }

    internal void Handle(GameAction action, InputEvent? input)
    {
        _pointer.Suspend();
        if (_help is not null) { _help.Handle(action); return; }
        if (action == GameAction.Cancel) { Back(); return; }
        if (action == GameAction.Spell && HasReset) { ResetPage(); return; }
        if (action == GameAction.Target && _details.GetVScrollBar() is { Visible: true } reading)
        {
            _details.ScrollVertical = reading.Value + reading.Page >= reading.MaxValue
                ? 0 : _details.ScrollVertical + (int)_details.Size.Y - StoryTheme.BodySize;
            return;
        }
        if (Focused is not { } focused) return;
        if (action is GameAction.Up or GameAction.Down)
        {
            focused.FindValidFocusNeighbor(action == GameAction.Up ? Side.Top : Side.Bottom)?.GrabFocus();
            return;
        }
        if (BattleOnly && (action == GameAction.Attack || action == GameAction.Confirm && focused.Id != "reset-page")) { Save(); return; }
        if (action == GameAction.Confirm) Activate(focused.Id, 1, input, cycle: true);
        else if (action is GameAction.Left or GameAction.Right)
        {
            if (IsValue(focused.Id)) Activate(focused.Id, action == GameAction.Left ? -1 : 1, input);
            else _scroll.ScrollVertical += action == GameAction.Left ? -112 : 112;
        }
    }

    private static bool IsValue(string id) => id is "large-text" or "language" or "graphics" or "text" or "speed" or "convention" or
        "flash" or "guidance" or "background" or "acceleration" or "diagnostics" or "victory" or "battle-speed" or "messages" or
        "music-volume" or "system-effects-volume" or "battle-effects-volume";

    internal bool RoutePointer(InputEvent input)
    {
        if (input is InputEventMouseMotion motion) _pointer.Moved(motion); // Veto a drag even if it returns to its button.
        if (input is not InputEventMouseButton button) return false;
        if (button.Pressed && button.ButtonMask.HasFlag(MouseButtonMask.Left | MouseButtonMask.Right))
            _pointer.Suspend();
        if (button.ButtonIndex == MouseButton.Right)
        {
            var bounds = GetGlobalRect();
            string? id = bounds.HasPoint(button.Position) ? "back" : null;
            if (_pointer.Read(button, id, bounds, (object?)_help ?? _page) != null)
            {
                if (_help is null) Back(); else CloseHelp();
            }
            return true;
        }
        if (button.ButtonIndex != MouseButton.Left) _pointer.Cancel();
        return false;
    }

    private static int ChangeVolume(int value, int direction, bool cycle) =>
        cycle && value == 100 ? 0 : Math.Clamp(value + direction * 10, 0, 100);

    private void Activate(string id, int direction, InputEvent? input, bool cycle = false)
    {
        if (Focused is not { Disabled: false } focused || focused.Id != id) return;
        _status = "";
        switch (id)
        {
            case "back": Back(); return;
            case "display": Enter(Page.Display); return;
            case "input": Enter(Page.Controls); return;
            case "assist": Enter(Page.Assist); return;
            case "debug": Enter(Page.Debug); return;
            case "audio": Enter(Page.Audio); return;
            case "edit": Enter(Page.Actions); return;
            case "help":
                _pointer.Suspend(); _panel.Hide(); _help = new ControlsHelpView { Name = "ControlsHelpView", Input = Input, Gesture = _pointer, Back = CloseHelp };
                AddChild(_help); return;
            case "apply": Save(); return;
            case "reset-page": ResetPage(); return;
            case "text": _draft.TextMode = _draft.TextMode == "instant" ? "adjustable" : "instant"; break;
            case "speed": _draft.CharactersPerSecond = Math.Clamp(_draft.CharactersPerSecond + direction * 10, 1, 240); break;
            case "convention": _draft.ConfirmCancel = _draft.ConfirmCancel == "standard" ? "swapped" : "standard"; break;
            case "large-text": _draft.LargeText = !_draft.LargeText; break;
            case "flash": _draft.ReducedFlash = !_draft.ReducedFlash; break;
            case "language": _draft.Language = _draft.Language == "en" ? "zh-CN" : "en"; break;
            case "graphics": _draft.GraphicsMode = _draft.GraphicsMode == "original" ? "hd" : "original"; break;
            case "guidance": _draft.ModernGuidance = !_draft.ModernGuidance; break;
            case "background": _draft.RunInBackground = !_draft.RunInBackground; break;
            case "acceleration": _draft.ChangeAccelerationMultiplier(direction); break;
            case "diagnostics": _draft.ShowDebugDiagnostics = !_draft.ShowDebugDiagnostics; break;
            case "victory": _draft.DebugAutoVictory = !_draft.DebugAutoVictory; break;
            case "music-volume": _draft.MusicVolume = ChangeVolume(_draft.MusicVolume, direction, cycle); break;
            case "system-effects-volume": _draft.SystemEffectsVolume = ChangeVolume(_draft.SystemEffectsVolume, direction, cycle); break;
            case "battle-effects-volume": _draft.BattleEffectsVolume = ChangeVolume(_draft.BattleEffectsVolume, direction, cycle); break;
            case "battle-speed": _draft.ChangeBattleSpeed(direction); break;
            case "messages": _draft.SuppressBattleHitCoinMessages = !_draft.SuppressBattleHitCoinMessages; break;
            default: if (input is not null) EditorActivate(id, input); return;
        }
        RefreshCaptions(); Paint();
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
        _help = null; RemoveChild(help); help.QueueFree();
        _pointer.Suspend(); _panel.Show();
        _buttons.Single(b => b.Id == "help").GrabFocus();
    }

    private string? FocusId => _help is null ? Focused?.Id : "help";

    public string ReadSettingsObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { draft = _draft, applied = Input.Settings, selected = _buttons.FindIndex(b => b.Id == FocusId), focus = FocusId, page = _page.ToString(), rows = _buttons.Select(b => b.Id),
        readOnly = ReadOnly, battleOnly = BattleOnly, reset = _reset, message = _message.Text,
        footer = _buttons.Where(b => b.GetParent() == _footer).Select(b => b.Id), hint = _hint.Text,
        locale = GameText.Language, coverage = GameText.Coverage, diagnostics = GameText.Diagnostics,
        debugVisible = _diagnostics.IsVisibleInTree(), debugText = _diagnostics.Text,
        labels = _buttons.Select(button => button.Text), help = _help is not null, scroll = _scroll.ScrollVertical, bindings = Enum.GetValues<GameAction>().ToDictionary(action => action.ToString(), Input.Hint) });
}

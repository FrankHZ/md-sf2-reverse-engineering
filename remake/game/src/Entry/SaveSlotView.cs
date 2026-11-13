using Godot;
using Sf2.Remake.GodotAdapter.Localization;
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
    private Label _coverage = null!;
    private PanelContainer _panel = null!;
    private VBoxContainer _column = null!;
    private VBoxContainer _confirmationActions = null!;
    private ScrollContainer? _reading;
    private readonly List<Button> _buttons = [];
    private readonly List<Action> _actions = [];
    private int _selected;
    private bool _loading;
    private Action? _loadingBack;
    internal Action? Cancel;
    internal bool OverEntry { get; init; }
    internal string State { get; private set; } = "slots";

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Theme = StoryTheme.CreateEntry(journey: true);
        var panel = _panel = new PanelContainer { Name = "SlotWindow" };
        if (OverEntry) { panel.Position = EntryLayout.Window.Position; panel.Size = EntryLayout.Window.Size; AddChild(panel); }
        else
        {
            var shade = new ColorRect { Name = "GameShade", Color = new("051225b8"), MouseFilter = MouseFilterEnum.Stop };
            shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
            AddChild(panel);
        }
        _column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _column.AddThemeConstantOverride("separation", OverEntry ? 24 : 12);
        var layout = new VBoxContainer(); layout.AddThemeConstantOverride("separation", OverEntry ? 24 : 12); panel.AddChild(layout);
        _reading = new ScrollContainer { Name = "SlotReading", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        layout.AddChild(_reading); _reading.AddChild(_column);
        _reading.GetVScrollBar().VisibilityChanged += RefreshInputHints;
        _confirmationActions = new VBoxContainer { Visible = false };
        _confirmationActions.AddThemeConstantOverride("separation", OverEntry ? 24 : 12);
        layout.AddChild(_confirmationActions);
        _title = new Label { Name = "SlotTitle", HorizontalAlignment = HorizontalAlignment.Center, Visible = !OverEntry };
        _title.AddThemeFontSizeOverride("font_size", 34); _column.AddChild(_title);
        _message = new Label { Name = "SlotMessage", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _column.AddChild(_message);
        _hint = new Label { Name = "SlotHint", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        layout.AddChild(_hint);
        _detail = new Label { Name = "SlotDetail", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart, Visible = false };
        _detail.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        _detail.AddThemeColorOverride("font_color", new Color("a7b9cf")); _column.AddChild(_detail);
        _coverage = new Label { Name = "SlotCoverage", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _coverage.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize); _column.AddChild(_coverage);
        Resized += ArrangeWindow; ArrangeWindow();
    }

    private void ArrangeWindow()
    {
        if (!OverEntry)
        {
            _panel.Size = new(Math.Min(1040, Math.Max(1, Size.X - 64)), Math.Min(920, Math.Max(1, Size.Y - 64)));
            _panel.Position = (Size - _panel.Size) / 2;
        }
    }

    internal void RefreshInputHints()
    {
        foreach (var button in _buttons)
            button.TooltipText = Input is null ? "" : GameText.Get("slot.tooltip", Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel));
        _hint.Text = Input is null ? "" : GameText.Get("entry.choose-hint", Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel));
        if (Input is not null && _reading?.GetVScrollBar().Visible == true) _hint.Text += "\n" + GameText.Get("host.menu-reading", Input.CompactHint(GameAction.Left), Input.CompactHint(GameAction.Right));
    }

    internal void ShowDetail(string detail) { _detail.Text = detail; _detail.Show(); }

    internal void ShowLoading(string message, Action? back)
    {
        ShowChoices("entry-loading", message, back is null ? [] : [(GameText.Get("entry.cancel"), back)]);
        _loading = true; _loadingBack = back; _hint.Hide();
    }
    internal void FinishLoading() { _loading = false; _loadingBack = null; _hint.Show(); }

    internal void ShowChoices(string state, string message, params (string Label, Action Action)[] choices)
    {
        _loading = false; _loadingBack = null; State = state; _message.Text = message;
        _detail.Hide(); _detail.Text = "";
        _coverage.Text = string.Join("\n", new[] { GameText.Fallback("slot.", includeKey: true), GameText.Fallback("save.", includeKey: true),
            GameText.Fallback("new.", includeKey: true), GameText.Fallback("entry.", includeKey: true) }.Where(text => text != ""));
        _coverage.Visible = _coverage.Text != "";
        _title.Text = state switch { "save-slots" => GameText.Get("slot.title.save"), "confirm-save" => GameText.Get("slot.title.confirm"),
            "saved" => GameText.Get("slot.title.saved"), "confirm-quit" => GameText.Get("slot.title.quit"), "error" => GameText.Get("slot.title.error"),
            "recover-backup" => GameText.Get("slot.title.recover"), _ => GameText.Get("slot.title.default") };
        _message.AddThemeColorOverride("font_color", state.Contains("error") || state.Contains("uncertain")
            ? new Color("ffc4ad") : Colors.White);
        RefreshInputHints();
        foreach (var button in _buttons) { button.GetParent().RemoveChild(button); button.QueueFree(); }
        _buttons.Clear(); _actions.Clear(); _selected = 0;
        _confirmationActions.Visible = state is "error" or "recover-backup" or "confirm-save";
        var actions = _confirmationActions.Visible ? _confirmationActions : _column;
        if (_confirmationActions.Visible && _reading is not null) _reading.ScrollVertical = 0;
        foreach (var (label, action) in choices)
        {
            int index = _buttons.Count;
            var button = new Button { Text = label, AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(0, 76), FocusMode = FocusModeEnum.None };
            button.Pressed += () => { if (_loading) { _loadingBack?.Invoke(); return; } Select(index); action(); };
            button.GuiInput += input => { if (Input?.KeyboardMouseHints == true &&
                input is InputEventMouseMotion { Device: >= 0 } motion && motion.Relative.Length() >= 4) Select(index); };
            button.TooltipText = Input is null ? "" : GameText.Get("slot.tooltip", Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel));
            actions.AddChild(button); StoryTheme.EntryChoice(button); _buttons.Add(button); _actions.Add(action);
        }
        _column.MoveChild(_detail, _column.GetChildCount() - 1);
        _column.MoveChild(_coverage, _column.GetChildCount() - 1);
        Select(0);
    }

    internal void Handle(GameAction action)
    {
        if (_loading) { if (action == GameAction.Cancel) _loadingBack?.Invoke(); return; }
        if (_buttons.Count == 0) return;
        if (action == GameAction.Cancel) Cancel?.Invoke();
        else if (action is GameAction.Left or GameAction.Right && _reading is not null)
            _reading.ScrollVertical += action == GameAction.Left ? -112 : 112;
        else if (action == GameAction.Up) Select((_selected + _buttons.Count - 1) % _buttons.Count);
        else if (action == GameAction.Down) Select((_selected + 1) % _buttons.Count);
        else if (action == GameAction.Confirm && _actions.Count > 0) _actions[_selected]();
    }
    internal void Select(int index)
    {
        _selected = index;
        for (int i = 0; i < _buttons.Count; i++)
            StoryTheme.Select(_buttons[i], i == index);
        if (_reading is not null && !_confirmationActions.Visible)
            Callable.From(() =>
            {
                if (index < _buttons.Count && _reading.IsAncestorOf(_buttons[index]) && _buttons[index].IsVisibleInTree())
                    _reading.EnsureControlVisible(_buttons[index]);
            }).CallDeferred();
    }
    public string ReadSaveObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { state = State, message = _message.Text, selected = _selected, choices = _buttons.Select(button => button.Text).ToArray() });
}

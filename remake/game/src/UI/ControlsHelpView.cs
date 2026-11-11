using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.UI;

// Reading-only child of Settings: no candidate settings or session commands.
internal sealed partial class ControlsHelpView : Control
{
    internal required GameInput Input { get; init; }
    internal required Action Back { get; init; }
    private sealed record Row(GameAction Action, string Meaning);
    private sealed record Page(string Group, string Note, Row[] Rows);
    private static readonly Page[] Pages =
    [
        new("host.help-text-0", "host.help-text-1",
            [new(GameAction.Up, "host.help-text-2"), new(GameAction.Right, "host.help-text-3")]),
        new("host.help-text-0", "host.help-text-4",
            [new(GameAction.Down, "host.help-text-5"), new(GameAction.Left, "host.help-text-6")]),
        new("host.help-text-7", "controls.acceleration.note",
            [new(GameAction.Accelerate, "controls.acceleration.hold")]),
        new("host.help-text-0", "host.help-text-8",
            [new(GameAction.Confirm, "host.help-text-9"), new(GameAction.Wait, "host.help-text-10"), new(GameAction.Equipment, "host.help-text-11")]),
        new("host.help-text-0", "host.help-text-12",
            [new(GameAction.Menu, "host.help-text-13"), new(GameAction.Cancel, "host.help-text-14"), new(GameAction.Stay, "host.help-text-15")]),
        new("host.help-text-16", "host.help-text-17",
            [new(GameAction.Confirm, "host.help-text-18"), new(GameAction.Cancel, "host.help-text-19"), new(GameAction.Left, "host.help-text-20")]),
        new("host.help-text-16", "host.help-text-21",
            [new(GameAction.Right, "host.help-text-22"), new(GameAction.Wait, "host.help-text-23")]),
        new("host.help-text-24", "host.help-text-25",
            [new(GameAction.Attack, "host.help-text-26"), new(GameAction.Spell, "host.help-text-27"), new(GameAction.Item, "host.help-text-28")]),
        new("host.help-text-24", "host.help-text-29",
            [new(GameAction.Target, "host.help-text-30"), new(GameAction.Stay, "host.help-text-31"), new(GameAction.Confirm, "host.help-text-32")]),
        new("host.help-text-24", "host.help-text-33",
            [new(GameAction.Equipment, "host.help-text-34"), new(GameAction.Give, "host.help-text-35"), new(GameAction.Discard, "host.help-text-36")]),
        new("host.help-text-37", "host.help-text-38",
            [new(GameAction.Confirm, "host.help-text-39"), new(GameAction.Cancel, "host.help-text-40"), new(GameAction.Target, "host.help-text-41")]),
        new("host.help-text-37", "host.help-text-42",
            [new(GameAction.Equipment, "host.help-text-43")]),
        new("host.help-text-44", "host.help-text-45", [])
    ];
    private int _page;
    private PanelContainer _panel = null!;
    private ScrollContainer _scroll = null!;
    private Label _title = null!, _body = null!, _hint = null!;
    private readonly List<Button> _buttons = [];
    private readonly MenuPointer _pointer = new();

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry(journey: true);
        _panel = new PanelContainer { Name = "HelpWindow" }; AddChild(_panel);
        var column = new VBoxContainer(); column.AddThemeConstantOverride("separation", 12); _panel.AddChild(column);
        _title = new Label { Name = "HelpTitle", AutowrapMode = TextServer.AutowrapMode.WordSmart }; column.AddChild(_title);
        _scroll = new ScrollContainer { Name = "HelpScroll", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }; column.AddChild(_scroll);
        _body = new Label { Name = "HelpBody", AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill }; _body.AddThemeFontSizeOverride("font_size", 30); _scroll.AddChild(_body);
        _hint = new Label { Name = "HelpHint", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", 24); column.AddChild(_hint);
        var actions = new HBoxContainer(); column.AddChild(actions);
        foreach (string text in new[] { "Previous", "Next", "Back" })
        {
            var button = new Button { Name = "Help" + text, Text = text, SizeFlagsHorizontal = SizeFlags.ExpandFill,
                CustomMinimumSize = new(0, 64), FocusMode = FocusModeEnum.None, MouseFilter = MouseFilterEnum.Ignore };
            StoryTheme.EntryChoice(button); button.AddThemeFontSizeOverride("font_size", 24); actions.AddChild(button); _buttons.Add(button);
        }
        Resized += Arrange; Arrange(); Refresh();
        GetWindow().SizeChanged += SuspendPointer;
    }
    public override void _ExitTree() => GetWindow().SizeChanged -= SuspendPointer;
    private void Arrange()
    {
        _pointer.Suspend();
        _panel.Size = new(Math.Min(1040, Math.Max(1, Size.X - 64)), Math.Min(900, Math.Max(1, Size.Y - 64)));
        _panel.Position = (Size - _panel.Size) / 2;
    }
    internal void RefreshInputHints()
    {
        int scroll = _scroll.ScrollVertical;
        Refresh(); _scroll.ScrollVertical = scroll;
    }
    private void Refresh()
    {
        var page = Pages[_page];
        _title.Text = GameText.Get("host.help-title", Text(page.Group), _page + 1, Pages.Length);
        _body.Text = Text(page.Note) + string.Concat(page.Rows.Select(row => $"\n\n{Text(row.Meaning)}\n{Input.Hint(row.Action)}"));
        _hint.Text = GameText.Get("host.help-hint", Input.CompactHint(GameAction.Left), Input.CompactHint(GameAction.Right),
            Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Up), Input.CompactHint(GameAction.Down), Input.CompactHint(GameAction.Cancel));
        string[] keys = ["host.help-previous", "host.help-next", "entry.back"];
        for (int i = 0; i < _buttons.Count; i++) _buttons[i].Text = GameText.Get(keys[i]);
        _scroll.ScrollVertical = 0;
    }
    private static string Text(string value) => (value.StartsWith("controls.", StringComparison.Ordinal) || value.StartsWith("host.", StringComparison.Ordinal)) ? GameText.Get(value) : value;
    internal void SuspendPointer() => _pointer.Suspend();
    public override void _Process(double delta)
    {
        if (!GetWindow().HasFocus() || !IsVisibleInTree()) { _pointer.Suspend(); return; }
        _pointer.ReleaseIfNeutral();
    }
    internal void Handle(GameAction action)
    {
        _pointer.Suspend();
        if (action == GameAction.Cancel) Back();
        else if (action is GameAction.Left or GameAction.Right or GameAction.Confirm) Turn(action == GameAction.Left ? -1 : 1);
        else if (action is GameAction.Up or GameAction.Down) _scroll.ScrollVertical += action == GameAction.Up ? -112 : 112;
    }
    private void Turn(int step) { _pointer.Suspend(); _page = (_page + step + Pages.Length) % Pages.Length; Refresh(); }
    internal void HandlePointer(InputEvent input)
    {
        if (!GetWindow().HasFocus()) { _pointer.Suspend(); return; }
        if (input is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        var mouse = (InputEventMouseButton)input;
        if (mouse.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            if (mouse.Pressed) _scroll.ScrollVertical += mouse.ButtonIndex == MouseButton.WheelUp ? -112 : 112;
            _pointer.Cancel(); return;
        }
        int hit = _buttons.FindIndex(button => button.GetGlobalRect().HasPoint(mouse.Position));
        string? id = mouse.ButtonIndex == MouseButton.Right ? "back" : hit < 0 ? null : hit.ToString();
        var bounds = mouse.ButtonIndex == MouseButton.Right ? GetGlobalRect() : hit < 0 ? new Rect2() : _buttons[hit].GetGlobalRect();
        string? clicked = _pointer.Read(mouse, id, bounds, _page);
        if (clicked is "back" or "2") Back();
        else if (clicked is "0" or "1") Turn(clicked == "0" ? -1 : 1);
    }
    public string ReadControlsHelpObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { page = _page, count = Pages.Length, group = Pages[_page].Group, title = _title.Text, body = _body.Text,
        hint = _hint.Text, scroll = _scroll.ScrollVertical,
        rows = Pages[_page].Rows.Select(row => new { action = row.Action.ToString(), binding = Input.Hint(row.Action) }) });
}

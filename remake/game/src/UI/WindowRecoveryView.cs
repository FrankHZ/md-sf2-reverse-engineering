using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.UI;

// Disposable recovery controls; native client admission and modal lifetime belong to GameRoot.
internal sealed partial class WindowRecoveryView : Control
{
    internal GameInput Input { get; init; } = null!;
    internal Action RestoreWindow { get; init; } = null!;
    internal Action Fullscreen { get; init; } = null!;
    internal Action Quit { get; init; } = null!;
    private readonly MenuPointer _pointer = new();
    private readonly List<Button> _buttons = [];
    private readonly object _boundary = new();
    private Panel _panel = null!;
    private ScrollContainer _scroll = null!;
    private Label _message = null!, _hint = null!, _title = null!;
    private int _selected;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry(journey: true);
        var shade = new ColorRect { Color = new("051225"), MouseFilter = MouseFilterEnum.Stop };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        _panel = new Panel { Name = "WindowRecoveryPanel" }; AddChild(_panel);
        _title = new Label { Text = GameText.Get("host.window-title"), HorizontalAlignment = HorizontalAlignment.Center }; _panel.AddChild(_title);
        _scroll = new ScrollContainer { Name = "RecoveryReading", HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        _panel.AddChild(_scroll);
        _message = new Label { Name = "RecoveryMessage", AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _scroll.AddChild(_message);
        foreach (string title in new[] { "host.window-restore", "host.window-fullscreen", "entry.quit-menu" })
        {
            var button = new Button { Name = "Recovery" + _buttons.Count, Text = GameText.Get(title),
                FocusMode = FocusModeEnum.None, MouseFilter = MouseFilterEnum.Ignore,
                AutowrapMode = TextServer.AutowrapMode.WordSmart };
            StoryTheme.EntryChoice(button); _buttons.Add(button); _panel.AddChild(button);
        }
        _hint = new Label { Name = "RecoveryHint", AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Text = GameText.Get("host.window-hint", Input.CompactHint(GameAction.Up), Input.CompactHint(GameAction.Down),
                Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel),
                Input.CompactHint(GameAction.Left), Input.CompactHint(GameAction.Right)) };
        _hint.AddThemeFontSizeOverride("font_size", 24); _panel.AddChild(_hint);
        Resized += Arrange; Arrange();
    }

    private void Arrange()
    {
        _pointer.Suspend();
        Vector2 bounds = new(Math.Min(960, Math.Max(1, Size.X - 64)), Math.Min(640, Math.Max(1, Size.Y - 64)));
        _panel.Size = bounds; _panel.Position = (Size - bounds) / 2;
        float width = Math.Max(1, bounds.X - 48), hintY = bounds.Y - 108;
        _title.Position = new(24, 20); _title.Size = new(width, 48);
        _hint.Position = new(24, hintY); _hint.Size = new(width, 88);
        float actionsY = hintY - 228;
        for (int i = 0; i < _buttons.Count; i++)
        { _buttons[i].Position = new(24, actionsY + i * 76); _buttons[i].Size = new(width, 64); }
        _scroll.Position = new(24, 80); _scroll.Size = new(width, Math.Max(1, actionsY - 92));
        float readingWidth = Math.Max(1, width - 20);
        _message.CustomMinimumSize = new(readingWidth, 0); _message.CustomMaximumSize = new(readingWidth, -1);
        _message.Size = new(readingWidth, 0);
    }

    internal void Present(Vector2I client, bool windowAvailable, bool fullscreenAvailable, string? reason)
    {
        string unavailable = (windowAvailable ? "" : GameText.Get("host.window-no-window")) +
            (fullscreenAvailable ? "" : GameText.Get("host.window-no-fullscreen"));
        _message.Text = GameText.Get("host.window-message", client.X, client.Y,
            reason ?? GameText.Get("host.window-default"), unavailable);
        _buttons[0].Disabled = !windowAvailable; _buttons[1].Disabled = !fullscreenAvailable;
        _buttons[0].TooltipText = windowAvailable ? "" : GameText.Get("host.window-no-window").Trim();
        _buttons[1].TooltipText = fullscreenAvailable ? "" : GameText.Get("host.window-no-fullscreen").Trim();
        if (_buttons[_selected].Disabled) _selected = 2;
        Select();
    }

    internal void Suspend() => _pointer.Suspend();
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree() || !GetWindow().HasFocus()) { Suspend(); return; }
        _pointer.ReleaseIfNeutral();
    }
    internal void Handle(GameAction action)
    {
        _pointer.Suspend();
        if (action is GameAction.Up or GameAction.Down)
        {
            int step = action == GameAction.Up ? -1 : 1;
            do { _selected = (_selected + step + _buttons.Count) % _buttons.Count; } while (_buttons[_selected].Disabled);
            Select();
        }
        else if (action is GameAction.Left or GameAction.Right)
            _scroll.ScrollVertical += action == GameAction.Right ? 128 : -128;
        else if (action == GameAction.Confirm) Activate();
        else if (action == GameAction.Cancel) Quit();
    }
    private void Select()
    { for (int i = 0; i < _buttons.Count; i++) StoryTheme.Select(_buttons[i], i == _selected); }
    private void Activate()
    {
        if (_buttons[_selected].Disabled) return;
        if (_selected == 0) RestoreWindow();
        else if (_selected == 1) Fullscreen();
        else Quit();
    }
    internal void HandlePointer(InputEvent input)
    {
        if (!GetWindow().HasFocus()) { Suspend(); return; }
        var mouse = (InputEventMouse)input;
        int hit = _buttons.FindIndex(button => !button.Disabled &&
            button.GetGlobalRect().HasPoint(mouse.Position));
        if (input is InputEventMouseMotion motion && _pointer.Moved(motion) && hit >= 0)
        { _selected = hit; Select(); }
        if (input is not InputEventMouseButton button) return;
        if (button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown && button.Pressed)
        { _pointer.Cancel(); if (_scroll.GetGlobalRect().HasPoint(mouse.Position))
            _scroll.ScrollVertical += button.ButtonIndex == MouseButton.WheelDown ? 96 : -96; return; }
        string? id = hit < 0 ? null : hit.ToString();
        var bounds = hit < 0 ? new Rect2() : _buttons[hit].GetGlobalRect();
        if (_pointer.Read(button, id, bounds, _boundary) is not null && button.ButtonIndex == MouseButton.Left)
        { _selected = hit; Activate(); }
    }
    internal object Observe() => new { selected = _selected, message = _message.Text,
        panel = Battles.BattleMapViewport.Rectangle(_panel.GetGlobalRect()), scroll = _scroll.ScrollVertical,
        scrollMaximum = Math.Max(0, _scroll.GetVScrollBar().MaxValue - _scroll.GetVScrollBar().Page),
        reading = Battles.BattleMapViewport.Rectangle(_scroll.GetGlobalRect()),
        hint = Battles.BattleMapViewport.Rectangle(_hint.GetGlobalRect()),
        buttons = _buttons.Select(button => new { text = button.Text, enabled = !button.Disabled,
            rect = Battles.BattleMapViewport.Rectangle(button.GetGlobalRect()) }) };
}

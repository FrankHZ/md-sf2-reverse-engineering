using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

// One finite host menu; existing hosts still own settings, persistence and replacement.
internal sealed partial class SystemMenuView : Control
{
    internal required GameInput Input { get; init; }
    internal required Action Resume { get; init; }
    private readonly List<Button> _buttons = [];
    private readonly List<Label> _reasons = [];
    private readonly List<Label> _groups = [];
    private readonly MenuPointer _pointer = new();
    private Action[] _actions = [];
    private Label _title = null!, _hint = null!;
    private PanelContainer _panel = null!;
    private int _selected;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry();
        var shade = new ColorRect { Color = new("051225b8"), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        var center = new CenterContainer { MouseFilter = MouseFilterEnum.Ignore };
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(center);
        _panel = new PanelContainer { Name = "SystemMenuWindow", MouseFilter = MouseFilterEnum.Ignore }; center.AddChild(_panel);
        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        column.AddThemeConstantOverride("separation", 8); _panel.AddChild(column);
        _title = new Label { Name = "SystemMenuTitle", HorizontalAlignment = HorizontalAlignment.Center };
        _title.AddThemeFontSizeOverride("font_size", 34); column.AddChild(_title);
        string[] names = ["Resume", "Save", "Settings", "NewGame", "Quit"];
        for (int index = 0; index < names.Length; index++)
        {
            if (index is 1 or 3)
            {
                var group = new Label(); group.AddThemeFontSizeOverride("font_size", 24);
                column.AddChild(group); _groups.Add(group);
            }
            var button = new Button { Name = "System" + names[index], CustomMinimumSize = new(0, 64),
                FocusMode = FocusModeEnum.None, MouseFilter = MouseFilterEnum.Ignore };
            column.AddChild(button); _buttons.Add(button);
            var reason = new Label { Name = "Reason" + names[index], AutowrapMode = TextServer.AutowrapMode.WordSmart,
                MouseFilter = MouseFilterEnum.Ignore };
            reason.AddThemeFontSizeOverride("font_size", 20); reason.AddThemeColorOverride("font_color", StoryTheme.Unavailable);
            column.AddChild(reason); _reasons.Add(reason);
        }
        _hint = new Label { Name = "SystemMenuHint", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", 24); column.AddChild(_hint);
        Resized += Arrange; Arrange();
    }
    private void Arrange() => _panel.CustomMinimumSize = new(Math.Min(800, Math.Max(1, Size.X - 64)), 0);

    internal void Present(bool save, string saveReason, bool settings, bool newGame,
        Action showSave, Action showSettings, Action showEntry, Action quit)
    {
        _pointer.Suspend();
        _actions = [Resume, showSave, showSettings, showEntry, quit];
        _title.Text = GameText.Get("system.title");
        _groups[0].Text = GameText.Get("system.journey"); _groups[1].Text = GameText.Get("system.session");
        string[] keys = ["system.resume", "entry.save-menu", "ui.settings.title", "entry.new-menu", "entry.quit-menu"];
        bool[] allowed = [true, save, settings, newGame, true];
        string[] reasons = ["", saveReason, GameText.Get("system.settings-reason"), GameText.Get("system.new-reason"), ""];
        for (int index = 0; index < _buttons.Count; index++)
        {
            _buttons[index].Text = GameText.Get(keys[index]); _buttons[index].Disabled = !allowed[index];
            _reasons[index].Text = allowed[index] ? "" : reasons[index]; _reasons[index].Visible = !allowed[index];
        }
        _hint.Text = GameText.Get("system.hint", Input.CompactHint(GameAction.Menu),
            Input.CompactHint(GameAction.Confirm), Input.CompactHint(GameAction.Cancel));
        if (!Input.MenuPadAvailable) _hint.Text += "\n" + GameText.Get("system.no-pad");
        Select(_selected);
    }
    private void Select(int index)
    {
        _selected = index;
        for (int i = 0; i < _buttons.Count; i++) StoryTheme.Select(_buttons[i], i == index);
    }
    private void Activate(int index)
    {
        if (_buttons[index].Disabled) return;
        Select(index); _pointer.Suspend(); _actions[index]();
    }
    internal void Handle(GameAction action)
    {
        _pointer.Suspend();
        if (action is GameAction.Cancel or GameAction.Menu) Resume();
        else if (action is GameAction.Up or GameAction.Down)
        {
            int step = action == GameAction.Up ? -1 : 1;
            int next = _selected;
            do { next = (next + step + _buttons.Count) % _buttons.Count; } while (_buttons[next].Disabled);
            Select(next);
        }
        else if (action == GameAction.Confirm) Activate(_selected);
    }
    internal void SuspendPointer() => _pointer.Suspend();
    public override void _Process(double delta) => _pointer.ReleaseIfNeutral();
    internal void HandlePointer(InputEvent input)
    {
        if (input is InputEventMouseMotion motion)
        {
            if (_pointer.Moved(motion))
            {
                int hover = _buttons.FindIndex(button => button.GetGlobalRect().HasPoint(motion.Position));
                if (hover >= 0 && !_buttons[hover].Disabled) Select(hover);
            }
            return;
        }
        if (input is not InputEventMouseButton mouse) return;
        int hit = _buttons.FindIndex(button => button.GetGlobalRect().HasPoint(mouse.Position));
        string? id = mouse.ButtonIndex == MouseButton.Right ? "back" : hit < 0 ? null : hit.ToString();
        var bounds = mouse.ButtonIndex == MouseButton.Right ? GetGlobalRect() : hit < 0 ? new Rect2() : _buttons[hit].GetGlobalRect();
        string? clicked = _pointer.Read(mouse, id, bounds, this);
        if (clicked == "back") Resume();
        else if (clicked is not null) Activate(int.Parse(clicked, System.Globalization.CultureInfo.InvariantCulture));
    }
}

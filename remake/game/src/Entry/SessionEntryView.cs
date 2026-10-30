using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

public sealed partial class SessionEntryView : Control
{
    internal Action? NewGame;
    internal Action? Cancel;
    internal Action? Continue;
    internal Action? Quit;
    private readonly List<Button> _titleButtons = [];
    private int _titleSelection;
    private Label _message = null!;
    private Button _newGame = null!;
    private Button _cancel = null!;
    private bool _cancelSelected;
    internal bool Replacement { get; init; }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Theme = StoryTheme.Create();
        var background = new ColorRect { Color = new("081a35"), MouseFilter = MouseFilterEnum.Stop };
        background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(background);
        var center = new CenterContainer(); center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(center);
        var panel = new PanelContainer { CustomMinimumSize = new(768, 480) }; center.AddChild(panel);
        var column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        column.AddThemeConstantOverride("separation", 28); panel.AddChild(column);
        var title = new Label { Text = "SHINING FORCE II", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 48); column.AddChild(title);
        _message = new Label { Text = Replacement ? "Start a new game? Current progress will be discarded." : "A new journey begins.",
            HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new(720, 96) };
        column.AddChild(_message);
        _newGame = new Button { Text = "New Game", CustomMinimumSize = new(0, 72), FocusMode = FocusModeEnum.None };
        _newGame.Pressed += () => NewGame?.Invoke(); column.AddChild(_newGame);
        _titleButtons.Add(_newGame);
        if (!Replacement)
        {
            var resume = new Button { Text = "Continue", CustomMinimumSize = new(0, 72), FocusMode = FocusModeEnum.None };
            resume.Pressed += () => Continue?.Invoke(); column.AddChild(resume); _titleButtons.Add(resume);
            var quit = new Button { Text = "Quit", CustomMinimumSize = new(0, 72), FocusMode = FocusModeEnum.None };
            quit.Pressed += () => Quit?.Invoke(); column.AddChild(quit); _titleButtons.Add(quit);
        }
        _cancel = new Button { Text = "Cancel", Visible = Replacement, CustomMinimumSize = new(0, 72), FocusMode = FocusModeEnum.None };
        _cancel.Pressed += () => Cancel?.Invoke(); column.AddChild(_cancel);
        column.AddChild(new Label { Text = "Confirm: Enter / Pad South    Cancel: Esc / Pad East",
            HorizontalAlignment = HorizontalAlignment.Center });
        Select(Replacement);
    }

    internal void ConfigureHints(GameInput input)
    {
        var hint = (Label)((VBoxContainer)_message.GetParent()).GetChild(-1);
        hint.Text = $"Confirm: {input.Hint(GameAction.Confirm)}    Cancel: {input.Hint(GameAction.Cancel)}";
    }

    internal void Handle(GameAction action)
    {
        if (!Replacement)
        {
            if (action == GameAction.Up) _titleSelection = (_titleSelection + _titleButtons.Count - 1) % _titleButtons.Count;
            else if (action == GameAction.Down) _titleSelection = (_titleSelection + 1) % _titleButtons.Count;
            else if (action == GameAction.Confirm)
            {
                if (_titleSelection == 0 && !_newGame.Disabled) NewGame?.Invoke();
                else if (_titleSelection == 1) Continue?.Invoke();
                else if (_titleSelection == 2) Quit?.Invoke();
            }
            for (int index = 0; index < _titleButtons.Count; index++)
                _titleButtons[index].Modulate = index == _titleSelection ? StoryTheme.Selected : Colors.White;
            return;
        }
        if (Replacement && action is GameAction.Up or GameAction.Down) Select(!_cancelSelected);
        else if (action == GameAction.Cancel && Replacement) Cancel?.Invoke();
        else if (action == GameAction.Confirm && (_cancelSelected || !_newGame.Disabled))
        { if (_cancelSelected) Cancel?.Invoke(); else NewGame?.Invoke(); }
    }

    private void Select(bool cancel)
    {
        _cancelSelected = cancel;
        _newGame.Modulate = cancel ? Colors.White : StoryTheme.Selected;
        _cancel.Modulate = cancel ? StoryTheme.Selected : Colors.White;
    }

    internal void Fail(SessionFailure failure, bool canRetry)
    {
        _newGame.Disabled = !canRetry;
        _message.Text = $"Cannot start: {failure.Code} ({failure.Field}).\n" + (canRetry
            ? "Check the prepared private opening content selections and try New Game again."
            : "Correct the startup options or input settings, then relaunch.");
    }

    public string ReadEntryObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { title = true, replacement = Replacement, cancelSelected = _cancelSelected, titleSelection = _titleSelection, message = _message.Text });
}

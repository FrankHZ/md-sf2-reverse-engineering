using Godot;
using Sf2.Remake.GodotAdapter.Input;

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
        new("Exploration", "At settled field input, hold a direction to walk. Story and movement waits own input until ready.",
            [new(GameAction.Up, "Walk north"), new(GameAction.Right, "Walk east")]),
        new("Exploration", "Directions also select menu rows or choices when those controls are open.",
            [new(GameAction.Down, "Walk south"), new(GameAction.Left, "Walk west")]),
        new("Exploration", "Only currently available interactions are offered. Waiting advances field time while held.",
            [new(GameAction.Confirm, "Talk to the faced NPC; otherwise Search when offered"), new(GameAction.Wait, "Hold: wait at field input"), new(GameAction.Equipment, "Open Party Status")]),
        new("Exploration", "Settings and Save are available only at safe settled field input, outside active text, menus and battle decisions.",
            [new(GameAction.Cancel, "Open Settings / Controls"), new(GameAction.Stay, "Open Save")]),
        new("Dialogue / choices", "Read all display pages before continuing. In Yes/No choices, Cancel means No. Church menus use Up/Down, Confirm and Cancel to choose or decline.",
            [new(GameAction.Confirm, "Reveal text, read more, then continue / confirm choice"), new(GameAction.Cancel, "No / Back; close a search result; skip an eligible battle introduction"), new(GameAction.Left, "Select previous choice (Up also selects previous)")]),
        new("Dialogue / choices", "During a battle message, Confirm reveals or continues. Wait is offered only where the current message allows it. A busy story cannot open Settings.",
            [new(GameAction.Right, "Select next choice (Down also selects next)"), new(GameAction.Wait, "Hold: wait at an eligible healing message")]),
        new("Battle", "During player command selection: directions move the provisional actor; in the diamond, Up/Left/Right/Down select Attack/Magic/Item/Stay. Availability and refusal reasons come from the current battle.",
            [new(GameAction.Attack, "Open physical attack from the diamond"), new(GameAction.Spell, "Open Magic list from the diamond"), new(GameAction.Item, "Open Item list from the diamond")]),
        new("Battle", "Lists use Up/Down and Confirm. Targets use directions or Target; disabled targets can be inspected but cannot be committed. AI and rounds advance automatically.",
            [new(GameAction.Target, "Next target"), new(GameAction.Stay, "Open Stay confirmation"), new(GameAction.Confirm, "Choose / confirm current legal action or target")]),
        new("Battle", "Supplementary actions are available from the command diamond when legal. Follow current item, recipient, equipment and confirmation rows; Cancel returns without committing a provisional choice.",
            [new(GameAction.Equipment, "Open equipment choices"), new(GameAction.Give, "Open Give choices"), new(GameAction.Discard, "Open Drop choices")]),
        new("Menus", "Up/Down select rows; Confirm chooses; Cancel goes Back. In battle, Back unwinds provisional choices. In Party Status, directions browse, Target changes the pane, Up/Down scroll details, Confirm opens Prepare and Equipment also closes.",
            [new(GameAction.Confirm, "Choose / change"), new(GameAction.Cancel, "Back / close"), new(GameAction.Target, "Party Status: change pane")]),
        new("Menus", "Title: Confirm or Equipment starts the Witch greeting; Equipment skips it. Settings edits stay in a draft until Apply. Control help Back keeps that draft and focus. Mouse: deliberate row clicks; battle target Previous/Next and Back/right click. No click-to-walk.",
            [new(GameAction.Equipment, "Title start / skip Witch greeting")]),
        new("Menus · name entry", "NAME ENTRY IS KEYBOARD ONLY. Type up to 7 supported letters, digits or punctuation. Backspace/Delete edit; Enter uses the name; Escape returns. Empty keeps Bowie. Gameplay bindings and their swap do not replace these editor keys. Other supported menus retain keyboard/pad controls.", [])
    ];
    private int _page;
    private PanelContainer _panel = null!;
    private ScrollContainer _scroll = null!;
    private Label _title = null!, _body = null!, _hint = null!;
    private readonly List<Button> _buttons = [];
    private readonly MenuPointer _pointer = new();

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _panel = new PanelContainer { Name = "HelpWindow" }; AddChild(_panel);
        var column = new VBoxContainer(); column.AddThemeConstantOverride("separation", 12); _panel.AddChild(column);
        _title = new Label { Name = "HelpTitle", AutowrapMode = TextServer.AutowrapMode.WordSmart }; column.AddChild(_title);
        _scroll = new ScrollContainer { Name = "HelpScroll", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }; column.AddChild(_scroll);
        _body = new Label { Name = "HelpBody", AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill }; _body.AddThemeFontSizeOverride("font_size", 24); _scroll.AddChild(_body);
        _hint = new Label { Name = "HelpHint", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _hint.AddThemeFontSizeOverride("font_size", 20); column.AddChild(_hint);
        var actions = new HBoxContainer(); column.AddChild(actions);
        foreach (string text in new[] { "Previous", "Next", "Back" })
        {
            var button = new Button { Name = "Help" + text, Text = text, SizeFlagsHorizontal = SizeFlags.ExpandFill,
                CustomMinimumSize = new(0, 58), FocusMode = FocusModeEnum.None, MouseFilter = MouseFilterEnum.Ignore };
            button.AddThemeFontSizeOverride("font_size", 22); actions.AddChild(button); _buttons.Add(button);
        }
        Resized += Arrange; Arrange(); Refresh();
    }
    private void Arrange()
    {
        _panel.Size = new(Math.Min(1040, Math.Max(1, Size.X - 64)), Math.Min(900, Math.Max(1, Size.Y - 64)));
        _panel.Position = (Size - _panel.Size) / 2;
    }
    private void Refresh()
    {
        var page = Pages[_page];
        _title.Text = $"Controls · {page.Group} · {_page + 1}/{Pages.Length}";
        _body.Text = page.Note + string.Concat(page.Rows.Select(row => $"\n\n{row.Meaning}\n{Input.Hint(row.Action)}"));
        _hint.Text = $"{Input.CompactHint(GameAction.Left)} / {Input.CompactHint(GameAction.Right)}: Page  •  {Input.CompactHint(GameAction.Confirm)}: Next\n" +
            $"{Input.CompactHint(GameAction.Up)} / {Input.CompactHint(GameAction.Down)}: Scroll  •  {Input.CompactHint(GameAction.Cancel)}: Back to Settings";
        _scroll.ScrollVertical = 0;
    }
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

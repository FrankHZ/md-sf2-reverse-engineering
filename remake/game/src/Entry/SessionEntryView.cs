using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Entry;

public sealed partial class SessionEntryView : Control
{
    internal Action? NewGame, Cancel, Continue, Quit;
    internal bool Replacement { get; init; }
    private EntryPresentationAssets? _assets;
    private EntryBackdrop _backdrop = null!;
    private StoryDialoguePanel _dialogue = null!;
    private Button _start = null!;
    private Button _read = null!;
    private Label _hint = null!;
    private readonly List<Button> _buttons = [];
    private GameInput? _input;
    private int _page = -1, _selection;
    private double _revealed;
    private bool _latched, _activated, _failed, _setup;
    private string? _assetError;
    private Window.ContentScaleModeEnum _previousScaleMode;
    private bool Revealed => _dialogue.Body.VisibleCharacters < 0 || _dialogue.Body.VisibleCharacters >= _dialogue.Body.GetTotalCharacterCount();
    private string Stage => _page == -1 ? "title" : _page >= 0 ? "dialogue" : "menu";

    public override void _Ready()
    {
        // Gameplay views use native window coordinates. Entry alone uses the project's
        // authored 1080p canvas, including when opened over an already mounted session.
        _previousScaleMode = GetWindow().ContentScaleMode;
        GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.CreateEntry();
        try { _assets = new EntryPresentationAssets(); _assets.AttachAudio(this); }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or ArgumentException or
            System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        { _assets?.Dispose(); _assets = null; _assetError = "entry-content-unavailable"; }
        _backdrop = new EntryBackdrop { Name = "EntryBackdrop", Assets = _assets }; AddChild(_backdrop);
        _backdrop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _dialogue = new StoryDialoguePanel { Name = "EntryDialogue", LayoutRegion = EntryLayout.Greeting }; AddChild(_dialogue);
        _start = new Button { Name = "Start", Text = "Press Start", Position = new(640, 900), Size = new(640, 72), FocusMode = FocusModeEnum.None };
        _start.Pressed += Advance; AddChild(_start); StoryTheme.Select(_start, true);
        _read = new Button { Name = "ReadDialogue", Text = "Continue", Position = new(1216, 548), Size = new(640, 76),
            FocusMode = FocusModeEnum.None, Visible = false };
        _read.Pressed += Advance; AddChild(_read); StoryTheme.Select(_read, true);
        _hint = new Label { Name = "EntryHint", Position = new(1216, 900), Size = new(640, 100), Text = "Normal difficulty",
            HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        AddChild(_hint);
        string[] labels = Replacement ? ["New Game", "Cancel"] : ["New Game", "Continue", "Quit"];
        for (int index = 0; index < labels.Length; index++)
        {
            int selected = index;
            var button = new Button { Name = labels[index].Replace(" ", ""), Text = labels[index],
                Position = new(1216, 548 + 100 * index), Size = new(640, 76), FocusMode = FocusModeEnum.None, Visible = false };
            button.Pressed += () => Activate(selected);
            button.MouseEntered += () => { if (_page == -2) Select(selected); };
            AddChild(button); _buttons.Add(button);
        }
        if (_assetError is not null)
        {
            ShowMenu(); _buttons[0].Disabled = true;
            Message("Title / Witch content is unavailable. Select a prepared private entry package with SF2_PRIVATE_ENTRY_CONTENT, then relaunch.");
        }
        else if (Replacement) { ShowMenu(); Select(1); }
        else _assets!.Audio!.Play("MUSIC_TITLE");
    }

    internal void ConfigureHints(GameInput input)
    {
        _input = input; _backdrop.ReducedFlash = input.Settings.ReducedFlash;
        _start.Text = $"Start  •  {input.CompactHint(GameAction.Confirm)}";
        _start.TooltipText = input.Hint(GameAction.Confirm) + " / " + input.Hint(GameAction.Equipment);
        foreach (var button in _buttons)
            button.TooltipText = $"Choose: {input.Hint(GameAction.Confirm)} • Back: {input.Hint(GameAction.Cancel)}";
        RefreshHint();
    }
    internal void Setup(bool active)
    {
        _activated = active; _setup = active;
        foreach (var button in _buttons) button.Visible = !active;
        _dialogue.Visible = !active; _hint.Visible = !active;
    }
    internal void ReturnFromSetup() { _activated = _setup = false; ShowMenu(); }
    public override void _ExitTree()
    {
        GetWindow().ContentScaleMode = _previousScaleMode;
        _backdrop.Assets = null; _assets?.Dispose(); _assets = null;
    }
    public override void _Process(double delta)
    {
        if (_input?.GameplayHeld == false) _latched = false;
        if (!IsVisibleInTree()) return;
        if (!Revealed)
        {
            _revealed = Math.Min(_dialogue.Body.GetTotalCharacterCount(), _revealed + delta * (_input?.Settings.CharactersPerSecond ?? 40));
            _dialogue.Body.VisibleCharacters = (int)_revealed;
        }
        _backdrop.Speaking = !Revealed;
        RefreshHint();
    }
    internal void Handle(GameAction action)
    {
        if (_latched || _activated) return;
        if (_page == -1)
        { if (action is GameAction.Confirm or GameAction.Equipment) Advance(); return; }
        if (_page >= 0)
        {
            if (action == GameAction.Equipment) { Latch(); ShowMenu(); }
            else if (action == GameAction.Confirm) Advance();
            else if (action == GameAction.Cancel) { Latch(); ShowTitle(); }
            return;
        }
        if (action == GameAction.Up) Select((_selection + _buttons.Count - 1) % _buttons.Count);
        else if (action == GameAction.Down) Select((_selection + 1) % _buttons.Count);
        else if (action == GameAction.Cancel) { Latch(); if (Replacement) Cancel?.Invoke(); else ShowTitle(); }
        else if (action == GameAction.Confirm)
        {
            if (!Revealed) { _dialogue.Body.VisibleCharacters = -1; Latch(); }
            else Activate(_selection);
        }
    }
    private void Latch() => _latched = _input?.GameplayHeld == true;
    private void Advance()
    {
        if (_latched || _activated || _assets is null) return;
        Latch();
        if (_page == -1)
        {
            _page = 0; _start.Hide(); _backdrop.Witch = true; _backdrop.Arrival = true;
            _assets.Audio!.Play("MUSIC_WITCH"); Message(_assets.Content.Dialogue["216"]); return;
        }
        if (!Revealed) { _dialogue.Body.VisibleCharacters = -1; return; }
        if (_dialogue.ReadMore()) return;
        if (++_page >= 5) { ShowMenu(); return; }
        _backdrop.Arrival = false; Message(_assets.Content.Dialogue[(216 + _page).ToString()]);
    }
    private void ShowMenu()
    {
        _page = -2; _start.Hide(); _backdrop.Witch = true; _backdrop.Arrival = false;
        foreach (var button in _buttons) button.Show();
        if (_assets is not null) _assets.Audio!.Play("MUSIC_WITCH");
        Message(Replacement ? "Start a new game? Current unsaved progress will be discarded." : _assets?.Content.Dialogue["221"] ?? "Entry unavailable.");
        Select(Replacement ? 1 : 0);
    }
    private void ShowTitle()
    {
        if (_failed || _assetError is not null) return;
        _page = -1; _backdrop.Witch = false; _dialogue.Hide(); _start.Show();
        foreach (var button in _buttons) button.Hide();
        _assets!.Audio!.Play("MUSIC_TITLE");
    }
    private void Message(string text)
    {
        _revealed = 0; _dialogue.ResetReading(); _dialogue.Body.Text = text;
        _dialogue.Body.VisibleCharacters = _input?.Settings.TextMode == "adjustable" ? 0 : -1;
        _dialogue.Present(true, false, false, new(), ""); RefreshHint();
    }
    private void RefreshHint()
    {
        if (_setup) { _hint.Hide(); _read.Hide(); return; }
        _hint.Visible = _page != -1;
        _read.Visible = _page >= 0;
        _read.Text = Revealed ? "Continue" : "Reveal";
        _hint.Text = Replacement ? "New Game  •  Normal difficulty" : "Choose your journey  •  Normal difficulty";
        if (_input is null) return;
        _dialogue.SetHint(!Revealed ? $"{_input.CompactHint(GameAction.Confirm)}: Reveal" : _page >= 0
            ? $"{_input.CompactHint(GameAction.Confirm)}: Continue   •   {_input.CompactHint(GameAction.Equipment)}: Skip greeting"
            : $"{_input.CompactHint(GameAction.Confirm)}: Choose   •   {_input.CompactHint(GameAction.Cancel)}: Back");
    }
    private void Select(int index)
    {
        _selection = index;
        for (int i = 0; i < _buttons.Count; i++) StoryTheme.Select(_buttons[i], i == index);
    }
    private void Activate(int index)
    {
        if (_page != -2 || _activated || _latched || _buttons[index].Disabled) return;
        Select(index); Latch();
        if (index == 0) { _activated = true; NewGame?.Invoke(); }
        else if (Replacement) Cancel?.Invoke();
        else if (index == 1) Continue?.Invoke();
        else Quit?.Invoke();
    }
    internal void Fail(SessionFailure failure, bool canRetry)
    {
        _activated = false; _failed = true; ShowMenu(); _buttons[0].Disabled = !canRetry || _assets is null;
        Message($"Cannot start: {failure.Code} ({failure.Field}).\n" + (canRetry
            ? "Check the prepared private opening content selections and try New Game again."
            : "Correct the startup options or input settings, then relaunch."));
    }
    public string ReadEntryObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    {
        title = _page == -1, stage = Stage, replacement = Replacement, cancelSelected = Replacement && _selection == 1,
        titleSelection = _selection, page = _page, message = _dialogue.Body.Text, visibleCharacters = _dialogue.Body.VisibleCharacters,
        totalCharacters = _dialogue.Body.GetTotalCharacterCount(), latched = _latched, activated = _activated, assetError = _assetError,
        artFrame = _backdrop.Frame, artRect = new { x = _backdrop.Art.Position.X, y = _backdrop.Art.Position.Y, width = _backdrop.Art.Size.X, height = _backdrop.Art.Size.Y },
        textureCount = _assets?.Textures.Count ?? 0, sourceWidth = 256, sourceHeight = 224, integerScale = 4,
        textureIds = _assets?.Textures.Values.Select(texture => texture.GetInstanceId().ToString()).ToArray(),
        layers = _page == -1 ? new[] { "numeral", "title", "start" } : new[] { "witch", "menu", "dialogue" },
        buttons = _buttons.Select(button => new { name = button.Name.ToString(), visible = button.IsVisibleInTree(), disabled = button.Disabled,
            x = button.Position.X, y = button.Position.Y, width = button.Size.X, height = button.Size.Y }),
        audio = _assets?.Audio?.ObservePlayback(), dialogue = _dialogue.Observe(),
    });
}

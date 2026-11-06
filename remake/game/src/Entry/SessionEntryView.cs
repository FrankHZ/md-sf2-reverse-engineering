using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;
using Sf2.Remake.Content.Localization;

namespace Sf2.Remake.GodotAdapter.Entry;

public sealed partial class SessionEntryView : Control
{
    internal Action? NewGame, Cancel, Continue, Quit, Settings;
    internal bool CanOpenSettings => _page < 0 && !_setup && !_activated && Revealed;
    internal bool Replacement { get; init; }
    private EntryPresentationAssets? _assets;
    private EntryBackdrop _backdrop = null!;
    private StoryDialoguePanel _dialogue = null!;
    private Button _start = null!;
    private Button _read = null!;
    private Button _settingsAccess = null!;
    private Label _hint = null!;
    private readonly List<Button> _buttons = [];
    private GameInput? _input;
    private int _page = -1, _selection;
    private double _revealed;
    private bool _latched, _activated, _failed, _setup;
    private string? _assetError;
    private DisplayResult? _entryDisplay;
    private SessionFailure? _failure;
    private bool _canRetry;
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
        try
        {
            _assets = new EntryPresentationAssets(); _assets.AttachAudio(this);
            // Host pause owns audio explicitly; disabling entry gestures must not pause BGM.
            GetNode<AudioStreamPlayer>("SessionMusic").ProcessMode = ProcessModeEnum.Always;
        }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException or ArgumentException or
            System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        { _assets?.Dispose(); _assets = null; _assetError = "entry-content-unavailable"; }
        _backdrop = new EntryBackdrop { Name = "EntryBackdrop", Assets = _assets }; AddChild(_backdrop);
        _backdrop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _dialogue = new StoryDialoguePanel { Name = "EntryDialogue", LayoutRegion = EntryLayout.Greeting }; AddChild(_dialogue);
        _start = new Button { Name = "Start", Text = GameText.Get("entry.press-start"), Position = new(640, 900), Size = new(640, 72), FocusMode = FocusModeEnum.None };
        _start.Pressed += Advance; AddChild(_start); StoryTheme.Select(_start, true);
        _read = new Button { Name = "ReadDialogue", Text = GameText.Get("entry.continue"), Position = new(1216, 548), Size = new(640, 76),
            FocusMode = FocusModeEnum.None, Visible = false };
        _read.Pressed += () => { if (_page == -2) ReadMenu(); else Advance(); }; AddChild(_read); StoryTheme.Select(_read, true);
        _hint = new Label { Name = "EntryHint", Position = new(1216, 900), Size = new(640, 160), Text = GameText.Get("entry.normal"),
            HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        AddChild(_hint);
        _settingsAccess = new Button { Name = "TitleSettings", Text = "Settings / Controls", Position = new(1320, 900), Size = new(536, 72), FocusMode = FocusModeEnum.None };
        _settingsAccess.Pressed += () => { if (CanOpenSettings) Settings?.Invoke(); }; AddChild(_settingsAccess);
        string[] names = Replacement ? ["NewGame", "Cancel", "Settings"] : ["NewGame", "Continue", "Quit", "Settings"];
        string[] keys = Replacement ? ["entry.new", "entry.cancel", "ui.settings.entry"] : ["entry.new", "entry.continue", "entry.quit", "ui.settings.entry"];
        for (int index = 0; index < names.Length; index++)
        {
            int selected = index;
            var button = new Button { Name = names[index], Text = index == names.Length - 1 ? GameText.Get(keys[index], "") : GameText.Get(keys[index]),
                Position = new(1216, 548 + 88 * index), Size = new(640, 76), FocusMode = FocusModeEnum.None, Visible = false };
            button.Pressed += () => Activate(selected);
            button.MouseEntered += () => { if (_page == -2) Select(selected); };
            AddChild(button); _buttons.Add(button);
        }
        if (_assetError is not null)
        {
            ShowMenu(); _buttons[0].Disabled = true;
            Message(GameText.Get("entry.assets"));
        }
        else if (Replacement) { ShowMenu(); Select(1); }
        else _assets!.Audio!.Play("MUSIC_TITLE");
    }

    internal void ConfigureHints(GameInput input)
    {
        _input = input; _backdrop.ReducedFlash = input.Settings.ReducedFlash;
        _settingsAccess.Text = GameText.Get("ui.settings.entry", input.CompactHint(GameAction.Cancel));
        for (int i = 0; i < _buttons.Count - 1; i++)
            _buttons[i].Text = GameText.Get(i == 0 ? "entry.new" : Replacement ? "entry.cancel" : i == 1 ? "entry.continue" : "entry.quit");
        if (_buttons.Count > 0) _buttons[^1].Text = GameText.Get("ui.settings.entry", input.CompactHint(GameAction.Cancel));
        _settingsAccess.AddThemeFontSizeOverride("font_size", 24);
        _start.Text = GameText.Get("entry.start", input.CompactHint(GameAction.Confirm));
        _start.TooltipText = input.Hint(GameAction.Confirm) + " / " + input.Hint(GameAction.Equipment);
        foreach (var button in _buttons)
            button.TooltipText = GameText.Get("entry.choose-tooltip", input.Hint(GameAction.Confirm), input.Hint(GameAction.Cancel));
        if (_page == -2)
        {
            _entryDisplay = Replacement || _assetError is not null || _failure is not null ? null : _assets?.Dialogue(221);
            string text = _failure is not null ? FailureText() : _assetError is not null ? GameText.Get("entry.assets") :
                Replacement ? GameText.Get("entry.replacement") : _entryDisplay?.Text ?? GameText.Get("entry.unavailable");
            if (_dialogue.Body.Text != text) { _dialogue.ResetReading(); _dialogue.Body.Text = text; }
            _dialogue.Body.VisibleCharacters = -1;
        }
        RefreshHint();
    }
    internal void Setup(bool active)
    {
        _activated = active; _setup = active;
        foreach (var button in _buttons) button.Visible = !active;
        _dialogue.Visible = !active; _hint.Visible = !active;
    }
    internal void ReturnFromSetup() { _activated = _setup = false; ShowMenu(); }
    internal void PauseAudio(bool paused) => _assets?.Audio?.SetPaused(paused);
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
        {
            if (action == GameAction.Cancel && CanOpenSettings) Settings?.Invoke();
            else if (action is GameAction.Confirm or GameAction.Equipment) Advance();
            return;
        }
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
            _assets.Audio!.Play("MUSIC_WITCH"); EntryMessage(216); return;
        }
        if (!Revealed) { _dialogue.Body.VisibleCharacters = -1; return; }
        if (_dialogue.ReadMore()) return;
        if (++_page >= 5) { ShowMenu(); return; }
        _backdrop.Arrival = false; EntryMessage(216 + _page);
    }
    private void ShowMenu()
    {
        _failure = null; _canRetry = false;
        _page = -2; _start.Hide(); _backdrop.Witch = true; _backdrop.Arrival = false;
        foreach (var button in _buttons) button.Show();
        if (_assets is not null) _assets.Audio!.Play("MUSIC_WITCH");
        if (Replacement) Message(GameText.Get("entry.replacement"));
        else if (_assets is not null) EntryMessage(221);
        else Message(GameText.Get("entry.unavailable"));
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
        _entryDisplay = null;
        _revealed = 0; _dialogue.ResetReading(); _dialogue.Body.Text = text;
        _dialogue.Body.VisibleCharacters = _input?.Settings.TextMode == "adjustable" ? 0 : -1;
        _dialogue.Present(true, false, false, new(), ""); RefreshHint();
    }
    private void EntryMessage(int id)
    {
        var display = _assets!.Dialogue(id);
        Message(display.Text); _entryDisplay = display; RefreshHint();
    }
    private void RefreshHint()
    {
        if (_setup) { _hint.Hide(); _read.Hide(); return; }
        _hint.Visible = _page != -1;
        _settingsAccess.Visible = _page == -1 && !_setup;
        _read.Visible = _page >= 0 || _page == -2 && _dialogue.MoreBelow;
        if (_page == -2) _buttons[0].Visible = !_read.Visible;
        _read.Text = !Revealed ? GameText.Get("entry.reveal") : _dialogue.MoreBelow ? GameText.Get("entry.more") : GameText.Get("entry.continue");
        _hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        _hint.Text = Replacement ? GameText.Get("entry.new-hint") : GameText.Get("entry.journey-hint");
        if (GameText.Language != "en" && _entryDisplay?.Reason is { } reason)
        {
            _hint.AddThemeFontSizeOverride("font_size", 20);
            _hint.Text += "\n" + GameText.Get("entry.fallback", reason);
        }
        string fallback = GameText.Fallback("entry.", includeKey: true);
        if (fallback != "")
        {
            _hint.AddThemeFontSizeOverride("font_size", 20);
            _hint.Text += "\n" + fallback;
        }
        if (_input is null) return;
        _dialogue.SetHint(!Revealed ? GameText.Get("entry.reveal-hint", _input.CompactHint(GameAction.Confirm)) : _dialogue.MoreBelow
            ? GameText.Get("entry.more-hint", _input.CompactHint(GameAction.Confirm)) : _page >= 0
            ? GameText.Get("entry.read-hint", _input.CompactHint(GameAction.Confirm), _input.CompactHint(GameAction.Equipment))
            : GameText.Get("entry.choose-hint", _input.CompactHint(GameAction.Confirm), _input.CompactHint(GameAction.Cancel)));
    }
    private void Select(int index)
    {
        _selection = index;
        for (int i = 0; i < _buttons.Count; i++) StoryTheme.Select(_buttons[i], i == index);
    }
    private void Activate(int index)
    {
        if (_page != -2 || _activated || _latched) return;
        if (index != _buttons.Count - 1 && ReadMenu()) return;
        if (_buttons[index].Disabled) return;
        Select(index); Latch();
        if (index == _buttons.Count - 1) { Settings?.Invoke(); return; }
        if (index == 0) { _activated = true; NewGame?.Invoke(); }
        else if (Replacement) Cancel?.Invoke();
        else if (index == 1) Continue?.Invoke();
        else Quit?.Invoke();
    }
    private bool ReadMenu()
    {
        if (_latched || _activated) return false;
        if (!Revealed) { _dialogue.Body.VisibleCharacters = -1; Latch(); return true; }
        if (_dialogue.ReadMore()) { Latch(); return true; }
        return false;
    }
    internal void Fail(SessionFailure failure, bool canRetry)
    {
        _activated = false; _failed = true; ShowMenu(); _buttons[0].Disabled = !canRetry || _assets is null;
        _failure = failure; _canRetry = canRetry;
        Message(FailureText());
    }
    private string FailureText() => GameText.Get("entry.fail", _failure!.Code, _failure.Field, _canRetry
            ? GameText.Get("entry.retry-help")
            : GameText.Get("entry.launch-help"));
    public string ReadEntryObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    {
        title = _page == -1, stage = Stage, replacement = Replacement, cancelSelected = Replacement && _selection == 1,
        failure = _failure, canRetry = _canRetry,
        titleSelection = _selection, page = _page, message = _dialogue.Body.Text, visibleCharacters = _dialogue.Body.VisibleCharacters,
        totalCharacters = _dialogue.Body.GetTotalCharacterCount(), latched = _latched, activated = _activated, assetError = _assetError,
        artFrame = _backdrop.Frame, artRect = new { x = _backdrop.Art.Position.X, y = _backdrop.Art.Position.Y, width = _backdrop.Art.Size.X, height = _backdrop.Art.Size.Y },
        textureCount = _assets?.Textures.Count ?? 0, sourceWidth = 256, sourceHeight = 224, integerScale = 4,
        textureIds = _assets?.Textures.Values.Select(texture => texture.GetInstanceId().ToString()).ToArray(),
        layers = _page == -1 ? new[] { "numeral", "title", "start" } : new[] { "witch", "menu", "dialogue" },
        buttons = _buttons.Select(button => new { name = button.Name.ToString(), visible = button.IsVisibleInTree(), disabled = button.Disabled,
            x = button.Position.X, y = button.Position.Y, width = button.Size.X, height = button.Size.Y }),
        displayLocale = _entryDisplay?.EffectiveLocale, displayFallback = _entryDisplay?.Reason,
        audio = _assets?.Audio?.ObservePlayback(), dialogue = _dialogue.Observe(),
    });
}

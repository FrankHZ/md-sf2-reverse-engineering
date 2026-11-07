using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private SettingsStore? _preferences;
    private SettingsView? _settings;
    private string? _settingsNotice;
    private bool _settingsOverride;
    private InputEvent? _settingsActivation, _settingsRelease;
    private bool _battleSettings;

    public string ReadSettingsHostObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { applied = _input?.Settings, acceleration = _input?.ObserveAccelerationState(), releaseBarrier = _releaseBarrier, remapRelease = _settingsRelease?.AsText(),
        remapDevice = _settingsRelease?.Device, held = _input?.GameplayHeld, localization = GameText.Observe() });

    private void HandleSettingsAction(GameAction action, InputEvent input)
    {
        _settingsActivation = input;
        try { _settings?.Handle(action); }
        finally { _settingsActivation = null; }
    }

    // Only the event that activated Apply survives remapping. No historical key set
    // participates in global admission; focus loss cancels this temporary barrier.
    private bool BlockSettingsRelease(InputEvent input)
    {
        if (_settingsRelease is null || input is InputEventMouse) return false;
        _input?.Resolve(input); _input?.DisarmMovement(); _input?.DisarmWait();
        bool released = (_settingsRelease, input) switch
        {
            (InputEventKey old, InputEventKey current) => old.Keycode == current.Keycode && !current.Pressed,
            (InputEventJoypadButton old, InputEventJoypadButton current) => old.Device == current.Device && old.ButtonIndex == current.ButtonIndex && !current.Pressed,
            (InputEventJoypadMotion old, InputEventJoypadMotion current) => old.Device == current.Device && old.Axis == current.Axis && Math.Abs(current.AxisValue) < 0.5f,
            _ => false,
        };
        if (released) { _settingsRelease = null; DisarmInput(); }
        return true;
    }

    private void SettingsFocusLost()
    {
        _input?.AllowAcceleration(false);
        _systemMenu?.SuspendPointer();
        if (_settings is null && _settingsRelease is null) return;
        _settingsRelease = null; _releaseBarrier = false;
        _input?.ReleaseSettingsHold(); _settings?.SuspendPointer();
    }

    private void SettingsNotification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            // Cancel the draft before the existing Quit owner creates any confirmation.
            CloseSettings(); RequestQuit();
        }
    }

    private void LoadPreferences(string? launchOverride)
    {
        _settingsOverride = launchOverride is not null;
        InputSettings settings;
        if (launchOverride is not null)
        {
            settings = InputSettings.Load(launchOverride);
            _settingsNotice = "This launch uses a read-only --input-settings override. Relaunch without it to edit ordinary preferences.";
        }
        else
        {
            try
            {
                _preferences = new SettingsStore(Selected("SF2_SAVE_ROOT"), Selected("SF2_SAVE_NAMESPACE"),
                    System.IO.Path.GetFullPath(System.IO.Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", "..")));
                settings = _preferences.Load(); _settingsNotice = _preferences.Error;
            }
            catch (ArgumentException)
            {
                settings = InputSettings.Load(null);
                _settingsNotice = "Preferences storage is not configured safely. Current defaults are usable; configure SF2_SAVE_ROOT and SF2_SAVE_NAMESPACE outside the checkout, then relaunch to save changes.";
            }
        }
        GameText.Initialize(settings.Language);
        if (GameText.StartupError is { } languageError) _settingsNotice = languageError + (_settingsNotice is null ? "" : "\n" + _settingsNotice);
        _input = new GameInput(settings);
    }

    private bool CanOpenSettings => _input is not null && _settings is null && _slots is null && _nameEntry is null &&
        (_entry?.CanOpenSettings == true || _entry is null && _exploration?.CanOpenHostMenu == true);

    private void ShowSettings()
    {
        if (!CanOpenSettings) return;
        _input!.DisarmMovement(); _input.DisarmWait();
        _settings = new SettingsView { Name = "SettingsView", Input = _input, Close = CloseSettings,
            Apply = ApplySettings, ReadOnly = _settingsOverride, Notice = _settingsOverride ? GameText.Get("ui.settings.readonly") + (GameText.StartupError is null ? "" : "\n" + GameText.StartupError) : _settingsNotice is null ? null : _settingsNotice + "\n" + GameText.Get("ui.settings.storage") };
        _hostUi.AddChild(_settings); CoverSystemMenu(); ApplyHostPause(); DisarmInput();
    }

    private void ShowBattleSpeed()
    {
        if (_input is null || _settings is not null || _entry is not null || _systemMenu is not null ||
            _slots is not null || _nameEntry is not null || _windowRecovery is not null || _windowQuit is not null ||
            _battle?.CanOpenBattleSpeed != true) return;
        _battleSettings = true;
        _settings = new SettingsView { Name = "SettingsView", Input = _input, Close = CloseSettings,
            Apply = ApplySettings, BattleOnly = true, ReadOnly = _settingsOverride,
            Notice = _settingsOverride ? GameText.Get("ui.settings.readonly") : _settingsNotice };
        _hostUi.AddChild(_settings); ApplyHostPause(); DisarmInput();
    }

    private string? ApplySettings(InputSettings candidate, bool reset)
    {
        if (_settingsOverride) return GameText.Get("ui.settings.readonly");
        if (_preferences is null) return _settingsNotice + "\n" + GameText.Get("ui.settings.storage");
        Dictionary<GameAction, List<InputEvent>> prepared;
        GameText.Candidate display;
        try { prepared = GameInput.Prepare(candidate); }
        catch (ArgumentException) { return GameText.Get("ui.settings.invalid"); }
        try { display = GameText.Prepare(candidate.Language); }
        catch (ArgumentException) { return GameText.Get("ui.settings.display"); }
        string? error = _preferences.Write(candidate, reset);
        if (error is not null) return error + "\n" + GameText.Get("ui.settings.storage");
        _settingsRelease = _settingsActivation;
        _input!.Apply(candidate, prepared); GameText.Publish(display); _settingsNotice = null;
        _entry?.ConfigureHints(_input);
        RefreshSystemMenuLabels();
        return null;
    }

    private void CloseSettings()
    {
        if (_settings is not { } settings) return;
        _settings = null; _hostUi.RemoveChild(settings); settings.QueueFree();
        if (_battleSettings) { _battleSettings = false; _battle?.ReturnFromBattleSpeed(); }
        ReturnToSystemMenu();
        ApplyHostPause(); DisarmInput();
    }

}

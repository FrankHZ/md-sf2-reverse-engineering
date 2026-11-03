using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private SettingsStore? _preferences;
    private SettingsView? _settings;
    private Button? _settingsButton;
    private string? _settingsNotice;
    private bool _settingsOverride;
    private InputEvent? _settingsActivation, _settingsRelease;

    public string ReadSettingsHostObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    { applied = _input?.Settings, releaseBarrier = _releaseBarrier, remapRelease = _settingsRelease?.AsText(),
        remapDevice = _settingsRelease?.Device, held = _input?.GameplayHeld });

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
        _input = new GameInput(settings);
    }

    private bool CanOpenSettings => _input is not null && _settings is null && _slots is null && _nameEntry is null &&
        (_entry?.CanOpenSettings == true || _entry is null && _exploration?.CanOpenHostMenu == true);

    private void ShowSettings()
    {
        if (!CanOpenSettings) return;
        _input!.DisarmMovement(); _input.DisarmWait();
        if (_entry is not null) _entry.ProcessMode = ProcessModeEnum.Disabled;
        if (_exploration is not null) _exploration.ProcessMode = ProcessModeEnum.Disabled;
        if (_battle is not null) _battle.ProcessMode = ProcessModeEnum.Disabled;
        _audio?.SetPaused(true);
        _settings = new SettingsView { Name = "SettingsView", Input = _input, Close = CloseSettings,
            Apply = ApplySettings, ReadOnly = _settingsOverride, Notice = _settingsNotice };
        AddChild(_settings); DisarmInput();
    }

    private string? ApplySettings(InputSettings candidate, bool reset)
    {
        if (_settingsOverride) return "This launch override is read-only.";
        if (_preferences is null) return _settingsNotice;
        Dictionary<GameAction, List<InputEvent>> prepared;
        try { prepared = GameInput.Prepare(candidate); }
        catch (ArgumentException) { return "The complete preferences could not be validated. Current settings are retained."; }
        string? error = _preferences.Write(candidate, reset);
        if (error is not null) return error;
        _settingsRelease = _settingsActivation;
        _input!.Apply(candidate, prepared); _settingsNotice = null;
        _entry?.ConfigureHints(_input);
        RefreshSettingsShortcut();
        return null;
    }

    private void CloseSettings()
    {
        if (_settings is not { } settings) return;
        _settings = null; RemoveChild(settings); settings.QueueFree();
        if (_entry is not null) _entry.ProcessMode = ProcessModeEnum.Inherit;
        if (_exploration is not null && _entry is null) _exploration.ProcessMode = ProcessModeEnum.Inherit;
        if (_battle is not null && _entry is null) _battle.ProcessMode = ProcessModeEnum.Inherit;
        _audio?.SetPaused(_entry is not null); DisarmInput();
    }

    private void ConfigureSettingsButton()
    {
        _settingsButton = HostButton("SettingsEntry", "Settings…", -1080, -780, ShowSettings);
        _settingsButton.AddThemeFontSizeOverride("font_size", 20);
        RefreshSettingsShortcut();
    }
    private void RefreshSettingsShortcut()
    {
        if (_settingsButton is not null) _settingsButton.Text = $"Settings · {_input!.CompactHint(GameAction.Cancel)}";
    }
    private void ReleaseSettingsButton()
    {
        if (_settingsButton is not null) { RemoveChild(_settingsButton); _settingsButton.QueueFree(); _settingsButton = null; }
    }
}

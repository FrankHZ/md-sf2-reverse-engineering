using Godot;
using Sf2.Remake.GodotAdapter.Entry;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private static readonly Vector2I MinimumClient = new(1920, 1080);
    private WindowRecoveryView? _windowRecovery;
    private SaveSlotView? _windowQuit;
    private Action? _windowStartup;
    private Vector2I _windowDecorations;
    private bool _updatingWindow, _windowDecorationsKnown;
    private readonly List<(Control View, bool Visible)> _windowCoveredViews = [];
    private Control? _windowEditorFocus;

    private bool WindowClientAllowed => GetWindow().Size.X >= MinimumClient.X && GetWindow().Size.Y >= MinimumClient.Y;
    private Rect2I WindowWorkArea => DisplayServer.ScreenGetUsableRect(GetWindow().CurrentScreen);
    private bool WindowCanFit => WindowWorkArea.Size.X >= MinimumClient.X + _windowDecorations.X &&
        WindowWorkArea.Size.Y >= MinimumClient.Y + _windowDecorations.Y;
    private bool FullscreenCanFit => DisplayServer.ScreenGetSize(GetWindow().CurrentScreen) is var size &&
        size.X >= MinimumClient.X && size.Y >= MinimumClient.Y;

    private void InitializeWindowSupport()
    {
        var window = GetWindow();
        if (window.Mode == Window.ModeEnum.Windowed)
        {
            _windowDecorations = window.GetSizeWithDecorations() - window.Size; _windowDecorationsKnown = true;
            // Prefer native fullscreen when a baseline decorated window cannot fit, including
            // a startup window the OS has already clamped below the gameplay minimum.
            if (!WindowCanFit && FullscreenCanFit) window.Mode = Window.ModeEnum.Fullscreen;
            else if (WindowClientAllowed)
            {
                var outer = window.GetSizeWithDecorations();
                if (outer.X <= WindowWorkArea.Size.X && outer.Y <= WindowWorkArea.Size.Y) CenterWindow();
                else if (WindowCanFit) RestoreGameplayWindow();
                else
                {
                    // Unsupported desktop: keep the entry/recovery shell reachable instead
                    // of leaving the project's default 1080p window larger than the screen.
                    window.Size = new(Math.Max(1, Math.Min(1280, WindowWorkArea.Size.X - _windowDecorations.X)),
                        Math.Max(1, Math.Min(720, WindowWorkArea.Size.Y - _windowDecorations.Y)));
                    CenterWindow();
                }
            }
        }
        window.SizeChanged += RefreshWindowSupport;
        window.FocusExited += SuspendWindowRecovery;
    }
    private void ReleaseWindowSupport()
    {
        GetWindow().SizeChanged -= RefreshWindowSupport;
        GetWindow().FocusExited -= SuspendWindowRecovery;
    }
    private void CenterWindow()
    {
        var window = GetWindow();
        var offset = window.Position - window.GetPositionWithDecorations();
        window.Position = WindowWorkArea.Position + (WindowWorkArea.Size - window.GetSizeWithDecorations()) / 2 + offset;
    }
    private bool EnsureWindowSupport()
    {
        if (WindowClientAllowed) return true;
        ShowWindowRecovery(); return false;
    }
    private void ShowWindowRecovery()
    {
        if (_windowRecovery is null)
        {
            _windowEditorFocus = GetViewport().GuiGetFocusOwner();
            foreach (var view in new Control?[] { _entry, _slots, _nameEntry, _settings })
                if (view is not null) { _windowCoveredViews.Add((view, view.Visible)); view.Hide(); }
            _windowRecovery = new WindowRecoveryView { Name = "WindowRecoveryView", Input = _input!,
                RestoreWindow = RestoreGameplayWindow, Fullscreen = UseGameplayFullscreen, Quit = RequestQuit };
            AddChild(_windowRecovery); DisarmInput(); ApplyHostPause();
        }
        _windowRecovery.Present(GetWindow().Size, WindowCanFit, FullscreenCanFit, null);
    }
    private void RefreshWindowSupport()
    {
        if (_updatingWindow || _input is null) return;
        if (!WindowClientAllowed)
        {
            if (_session is not null || _windowRecovery is not null) ShowWindowRecovery();
            return;
        }
        if (_windowRecovery is null || _windowQuit is not null) return;
        var recovery = _windowRecovery; _windowRecovery = null;
        RemoveChild(recovery); recovery.QueueFree();
        foreach (var (view, visible) in _windowCoveredViews)
            if (GodotObject.IsInstanceValid(view) && !view.IsQueuedForDeletion()) view.Visible = visible;
        _windowCoveredViews.Clear(); ApplyHostPause(); DisarmInput();
        if (GodotObject.IsInstanceValid(_windowEditorFocus) && _windowEditorFocus!.IsVisibleInTree()) _windowEditorFocus.GrabFocus();
        _windowEditorFocus = null;
        var startup = _windowStartup; _windowStartup = null; startup?.Invoke();
    }
    private void RestoreGameplayWindow()
    {
        if (!WindowCanFit) { WindowPlacementRefused("A 1080p window and its borders cannot fit the current desktop. Try fullscreen."); return; }
        var window = GetWindow();
        var previousMode = window.Mode; var previousSize = window.Size; var previousPosition = window.Position;
        bool restored = false;
        _updatingWindow = true;
        try
        {
            // A fullscreen launch has no windowed border measurement yet. Make one bounded
            // attempt, then trust actual client/outer dimensions rather than the estimate.
            window.Mode = Window.ModeEnum.Windowed; window.Size = MinimumClient;
            _windowDecorations = window.GetSizeWithDecorations() - window.Size; _windowDecorationsKnown = true;
            var outer = window.GetSizeWithDecorations();
            restored = WindowClientAllowed && outer.X <= WindowWorkArea.Size.X && outer.Y <= WindowWorkArea.Size.Y;
            if (restored) CenterWindow();
            else
            {
                window.Mode = previousMode;
                if (previousMode == Window.ModeEnum.Windowed)
                { window.Size = previousSize; window.Position = previousPosition; }
            }
        }
        finally { _updatingWindow = false; }
        RefreshWindowSupport();
        if (!restored) WindowPlacementRefused("The requested client or its actual borders could not fit. The previous window mode was restored; choose fullscreen or Quit.");
    }
    private void UseGameplayFullscreen()
    {
        if (!FullscreenCanFit) { WindowPlacementRefused("This screen cannot provide the minimum gameplay client. Choose another screen or Quit."); return; }
        GetWindow().Mode = Window.ModeEnum.Fullscreen; RefreshWindowSupport();
        if (!WindowClientAllowed) WindowPlacementRefused("Fullscreen did not provide the minimum gameplay client. Your journey remains paused.");
    }
    private void WindowPlacementRefused(string reason)
    { _windowRecovery?.Present(GetWindow().Size, WindowCanFit, FullscreenCanFit, reason); DisarmInput(); }
    private void SuspendWindowRecovery() { _windowRecovery?.Suspend(); DisarmInput(); }

    // These existing finite host modals share one resume decision, not a separate pause service.
    private void ApplyHostPause()
    {
        bool covered = _windowRecovery is not null || _windowQuit is not null;
        if (_entry is not null) _entry.ProcessMode = covered || _settings is not null || _slots is not null
            ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
        bool paused = covered || _entry is not null || _settings is not null || _slots is not null;
        if (_exploration is not null) _exploration.ProcessMode = paused ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
        if (_battle is not null) _battle.ProcessMode = paused ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
        if (_settings is not null) _settings.ProcessMode = covered ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
        if (_slots is not null) _slots.ProcessMode = covered ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
        if (_nameEntry is not null) _nameEntry.ProcessMode = covered ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
        _audio?.SetPaused(paused);
    }
    private bool RouteWindowInput(InputEvent input)
    {
        if (_windowQuit is not null)
        {
            // The existing SaveSlotView owns GUI clicks; no underlying modal receives keys.
            if (input is not InputEventMouse)
            {
                if (_input?.Resolve(input) is { } action) _windowQuit.Handle(action);
                GetViewport().SetInputAsHandled();
            }
            return true;
        }
        if (_windowRecovery is not { } recovery) return false;
        if (input is InputEventMouse) recovery.HandlePointer(input);
        else if (_input?.Resolve(input) is { } action) recovery.Handle(action);
        GetViewport().SetInputAsHandled(); return true;
    }
    private void ShowWindowQuit()
    {
        if (_windowQuit is not null) return;
        _windowRecovery!.Hide();
        _windowQuit = new SaveSlotView { Name = "WindowQuitView", Input = _input, Cancel = CloseWindowQuit };
        AddChild(_windowQuit); PresentQuitChoices(_windowQuit, CloseWindowQuit); ApplyHostPause(); DisarmInput();
    }
    private void CloseWindowQuit()
    {
        if (_windowQuit is { } quit) { _windowQuit = null; RemoveChild(quit); quit.QueueFree(); }
        _windowRecovery?.Show(); RefreshWindowSupport(); ApplyHostPause(); DisarmInput();
    }
    public string ReadWindowSupportObservationJson() => System.Text.Json.JsonSerializer.Serialize(new
    {
        client = new { width = GetWindow().Size.X, height = GetWindow().Size.Y }, allowed = WindowClientAllowed,
        mode = GetWindow().Mode.ToString(), decorationsKnown = _windowDecorationsKnown,
        decorations = new { width = _windowDecorations.X, height = _windowDecorations.Y }, recovery = _windowRecovery?.Observe(), quitting = _windowQuit is not null,
        entryPaused = _entry?.ProcessMode.ToString(), fieldPaused = _exploration?.ProcessMode.ToString(),
        battlePaused = _battle?.ProcessMode.ToString(), settingsPaused = _settings?.ProcessMode.ToString(),
        slotsPaused = _slots?.ProcessMode.ToString(), startupPending = _windowStartup is not null,
    });
}

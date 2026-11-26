using System.Globalization;
using System.Net.Sockets;
using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Observation;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private DebugPort? _debugPort;
    private bool _debugDispatch;
    private int _debugFrames;

    private void StartDebugPort()
    {
        string value = Selected("SF2_DEBUG_PORT");
        if (value.Length == 0) return;
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int port) || port is < 1 or > 65535)
        { GD.PushWarning("SF2_DEBUG_PORT requires a TCP port from 1 to 65535. Debug disabled."); return; }
        try { _debugPort = new DebugPort(port); }
        catch (SocketException error)
        { GD.PushWarning($"Debug listener unavailable on 127.0.0.1:{port} ({error.SocketErrorCode})."); }
    }

    private void PumpDebugPort()
    {
        if (_debugFrames > 0 && --_debugFrames == 0) ApplyHostPause();
        if (_debugPort?.TryRead(out var request) != true || request is null) return;
        try
        {
            request.Reply.SetResult(HandleDebugCommand(request.Line));
        }
        catch (Exception error)
        {
            GD.PushError($"Debug request failed ({error.GetType().Name}).");
            request.Reply.SetResult(DebugError("dispatch-failed"));
        }
    }

    private static string DebugError(string reason) => JsonSerializer.Serialize(new { ok = false, error = reason });

    private string HandleDebugCommand(string line)
    {
        string[] words = line.Split(' ');
        string? data;
        switch (words)
        {
            case ["hello"]:
                data = JsonSerializer.Serialize(new { version = 2, pid = System.Environment.ProcessId,
                    module = typeof(GameRoot).Module.ModuleVersionId, exported = OS.HasFeature("local_candidate") });
                break;
            case ["observe", "host"]:
                data = JsonSerializer.Serialize(new { focused = GetWindow().HasFocus(),
                    runFramesRemaining = _debugFrames, runInBackground = _input?.Settings.RunInBackground,
                    menuSelection = _systemMenu?.SelectedIndex, gameplayHeld = _input?.GameplayHeld,
                    releaseBarrier = _releaseBarrier, systemMenu = _systemMenu?.IsVisibleInTree() == true,
                    opening = JsonSerializer.Deserialize<JsonElement>(ReadOpeningObservationJson()),
                    entry = JsonSerializer.Deserialize<JsonElement>(_entry?.ReadEntryObservationJson() ?? "null"),
                    window = JsonSerializer.Deserialize<JsonElement>(ReadWindowSupportObservationJson()) });
                break;
            case ["observe", "state"]:
                data = _opening?.View?.ReadObservationJson() ?? (_session is null ? null :
                    SessionPresentationOwner.IsBattle(_session.Current) ? _battle?.ReadObservationJson() : _exploration?.ReadObservationJson());
                break;
            case ["observe", "audio"]: data = ReadAudioObservationJson(); break;
            case ["observe", "scene"]: data = _session is not null && SessionPresentationOwner.IsBattle(_session.Current)
                    ? _battle?.ReadSceneObservationJson() : null; break;
            case ["observe", "settings"]: data = _settings?.ReadSettingsObservationJson(); break;
            case ["observe", "slots"]: data = _slots?.ReadSaveObservationJson(); break;
            case ["observe", "name"]: data = _nameEntry?.ReadNameObservationJson(); break;
            case ["clock", "run", var count]:
                if (!int.TryParse(count, NumberStyles.None, CultureInfo.InvariantCulture, out int frames) || frames is < 1 or > 120)
                    return DebugError("frame-budget");
                if (_debugFrames != 0) return DebugError("run-in-progress");
                if (!WindowPausesPlayback || _windowRecovery is not null || _windowQuit is not null)
                    return DebugError("clock-not-focus-paused");
                _debugFrames = frames;
                ApplyHostPause();
                data = JsonSerializer.Serialize(new { status = "scheduled", frames });
                break;
            case [var context, var verb] when context is "opening" or "witch" or "menu" or "field" or "story" or "battle":
                if (_windowRecovery is not null || _windowQuit is not null) return DebugError("window-modal");
                if (!RouteDebugAction(context, verb)) return DebugError("action-unavailable");
                data = JsonSerializer.Serialize(new { status = "dispatched", context, verb });
                break;
            default: return DebugError("unknown-command");
        }
        return $"{{\"ok\":true,\"frame\":{Engine.GetProcessFrames()},\"data\":{data ?? "null"}}}";
    }
    // Dispatch between process callbacks. Restore the current host policy before child
    // clocks run. Only an explicit clock budget grants additional playback frames.
    private bool WithDebugPlayback(Func<bool> dispatch)
    {
        if (!WindowPausesPlayback) return dispatch();
        _debugDispatch = true;
        try
        {
            ApplyHostPause();
            return dispatch();
        }
        finally
        {
            _debugDispatch = false;
            ApplyHostPause();
        }
    }

    private bool RouteDebugAction(string context, string verb)
    {
        if (_opening is { } opening)
        {
            if (context != "opening" || verb != "skip") return false;
            FinishOpening(opening, "skipped");
            return true;
        }
        GameAction? mapped = verb switch
        {
            "up" => GameAction.Up, "right" => GameAction.Right, "down" => GameAction.Down,
            "left" => GameAction.Left, "confirm" => GameAction.Confirm, "back" => GameAction.Cancel,
            _ => null,
        };
        if (_settings is { } settings)
            return context == "menu" && verb == "back" && settings.BackFromDebug();
        if (_nameEntry is { } name)
            return context == "menu" && (verb == "confirm" ? name.ConfirmDraft() : verb == "back" && name.BackFromDraft());
        if (_slots is { } slots)
            return context == "menu" && mapped is { } slotAction && slots.Handle(slotAction);
        if (_entry is { } entry)
        {
            if (context == "witch" && verb == "skip" && entry.CanSkipGreeting)
                return entry.Handle(GameAction.Equipment, direct: true);
            return context == "menu" && mapped is { } entryAction && entry.Handle(entryAction, direct: true);
        }
        if (_systemMenu is { } menu)
        {
            if (context != "menu" || mapped is not { } menuAction || !menu.Visible) return false;
            return menu.Handle(menuAction);
        }
        if (_session is null || _exploration?.MenuEntryPending == true) return false;
        var current = _session.Current;
        bool battle = SessionPresentationOwner.IsBattle(current);
        if (context == "menu" && verb == "open")
        {
            if (!(battle ? _battle?.SystemMenuBase == true :
                    _exploration?.SystemMenuBase == true && current.HasSettledFieldEntry)) return false;
            ShowSystemMenu(); return _systemMenu is not null;
        }
        if (battle)
        {
            var action = verb switch { "attack" => GameAction.Attack, "stay" => GameAction.Stay, _ => mapped };
            return context == "battle" && action is { } selected && _battle is { } view &&
                WithDebugPlayback(() => view.DispatchDirect(selected));
        }
        if (_exploration is not { } field || field.MenuBack) return false;
        if (context == "field" && current.CanWaitAtInput)
        {
            if (verb == "interact") return WithDebugPlayback(() => field.HandleAction(GameAction.Confirm, direct: true));
            if (mapped is GameAction.Up or GameAction.Right or GameAction.Down or GameAction.Left)
                return WithDebugPlayback(() => field.HandleAction(mapped.Value, direct: true));
        }
        if (context == "story" && current.Story.Wait is ChoiceWait or DialogueWait or W1TextWait or FieldTextWait or FieldSearchWait or MusicWait &&
            mapped is { } storyAction)
            return WithDebugPlayback(() => field.HandleAction(storyAction, direct: true));
        return false;
    }

}

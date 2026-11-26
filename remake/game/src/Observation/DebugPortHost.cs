using System.Globalization;
using System.Net.Sockets;
using System.Text.Json;
using Godot;
using Sf2.Remake.GodotAdapter.Observation;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private DebugPort? _debugPort;
    private readonly HashSet<Key> _debugHeld = [];

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
        if (_debugPort?.TryRead(out var request) != true || request is null) return;
        try
        {
            string reply;
            if (request.Line is null) { ReleaseDebugKeys(); reply = "{}"; }
            else reply = HandleDebugCommand(request.Line);
            request.Reply.SetResult(reply);
        }
        catch (Exception error)
        {
            GD.PushError($"Debug request failed ({error.GetType().Name}).");
            request.Reply.SetResult(DebugError("dispatch-failed"));
        }
    }

    private void ReleaseDebugKeys()
    {
        foreach (var key in _debugHeld) SendDebugKey(key, false);
        _debugHeld.Clear();
    }

    private static void SendDebugKey(Key key, bool down)
    {
        using var input = new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = down };
        Godot.Input.ParseInputEvent(input);
        Godot.Input.FlushBufferedEvents();
    }

    private static string DebugError(string reason) => JsonSerializer.Serialize(new { ok = false, error = reason });

    private string HandleDebugCommand(string line)
    {
        string[] words = line.Split(' ');
        string? data;
        switch (words)
        {
            case ["hello"]:
                data = JsonSerializer.Serialize(new { version = 1, pid = System.Environment.ProcessId,
                    module = typeof(GameRoot).Module.ModuleVersionId, exported = OS.HasFeature("local_candidate") });
                break;
            case ["observe", "host"]:
                data = JsonSerializer.Serialize(new { focused = GetWindow().HasFocus(),
                    held = _debugHeld.Select(key => key.ToString()), gameplayHeld = _input?.GameplayHeld,
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
            case ["down" or "up", var name]:
                if (!Enum.TryParse<Key>(name, out var key) || !Enum.IsDefined(key) || key is Key.None or Key.Unknown || key.ToString() != name)
                    return DebugError("unknown-key");
                bool down = words[0] == "down";
                if (down && !GetWindow().HasFocus()) return DebugError("window-unfocused");
                if (down && Godot.Input.IsPhysicalKeyPressed(key) && !_debugHeld.Contains(key))
                    return DebugError("key-in-use");
                if (!(down ? _debugHeld.Add(key) : _debugHeld.Remove(key)))
                    return DebugError(down ? "already-held" : "not-held");
                SendDebugKey(key, down);
                data = "null";
                break;
            default: return DebugError("unknown-command");
        }
        return $"{{\"ok\":true,\"frame\":{Engine.GetProcessFrames()},\"data\":{data ?? "null"}}}";
    }
}

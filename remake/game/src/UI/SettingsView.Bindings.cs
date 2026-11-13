using Godot;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.UI;

internal sealed partial class SettingsView
{
    private string _action = "accelerate";
    private bool _keyboard;
    private enum Kind { Key, Button, Axis }
    private record Alternative(Kind Kind, string Code);
    private Alternative? _original;
    private ActionBinding? _proposal;
    private InputEvent? _drain;
    private int? _proposalDevice;
    // Track neutral/pressed state while the editor is open, including inputs that are not mapped.
    private readonly HashSet<(int Device, JoyAxis Axis)> _tilted = [];
    private readonly HashSet<(int Device, JoyButton Button)> _downButtons = [];
    private readonly HashSet<Key> _downKeys = [];

    internal void SeedCaptureControls()
    {
        foreach (int device in Godot.Input.GetConnectedJoypads())
        {
            foreach (JoyButton button in Enum.GetValues<JoyButton>().Where(b => b >= 0 && b < JoyButton.Max))
                if (Godot.Input.IsJoyButtonPressed(device, button)) _downButtons.Add((device, button));
            foreach (JoyAxis axis in new[] {JoyAxis.LeftX, JoyAxis.LeftY, JoyAxis.RightX, JoyAxis.RightY})
                if (Math.Abs(Godot.Input.GetJoyAxis(device, axis)) >= .35f) _tilted.Add((device, axis));
        }
    }

    private static IEnumerable<Alternative> All(ActionBinding row) =>
        row.Keys.Select(s => new Alternative(Kind.Key, s)).Concat(row.Buttons.Select(s => new Alternative(Kind.Button, s)))
            .Concat(row.Axes.Select(s => new Alternative(Kind.Axis, s)));
    private static string? IncompleteAction(InputSettings draft) => GameInput.BaseRows(draft)
        .FirstOrDefault(pair => GameInput.MissingFamily(pair.Key, pair.Value, draft.Bindings.ContainsKey(pair.Key))).Key;
    private string Effective(string name, InputSettings settings) => settings.ConfirmCancel != "swapped" ? name :
        name switch { "confirm" => "cancel", "cancel" => "confirm", _ => name };
    private ActionBinding Row => GameInput.BaseRows(_draft)[_action];
    private IEnumerable<Alternative> Family(ActionBinding row) => All(row).Where(a => (a.Kind == Kind.Key) == _keyboard);
    private string LabelFor(Alternative entry) => Input.DescribeBinding(entry.Code, entry.Kind == Kind.Key);
    private string ListText(IEnumerable<Alternative> entries) =>
        string.Join(" / ", entries.Select(LabelFor)) is { Length: > 0 } value ? value : S("none");

    private Choice ReservedRow() => new("reserved", S(_keyboard ? "escape" : "pad-back"));

    private List<Choice> EditorRows()
    {
        switch (_page)
        {
            case Page.Actions:
                var rows = GameInput.BaseRows(_draft);
                return [.. Enum.GetValues<GameAction>().Select(a =>
                {
                    var name = a.ToString().ToLowerInvariant();
                    var text = Effective(name, _draft) == name ? S(name) : S("effective", S(name), S(Effective(name, _draft)));
                    if (GameInput.MissingFamily(name, rows[name], _draft.Bindings.ContainsKey(name))) text = S("required", text);
                    return new Choice("action-" + name, text);
                }), BackRow()];
            case Page.Device: return [new("keyboard", S("keyboard")), new("pad", S("pad")), BackRow()];
            case Page.Alternatives: return [.. Family(Row).Select((a,i) => new Choice("slot-" + i, LabelFor(a))), new("add", S("add")), ReservedRow(), BackRow()];
            case Page.Operation: return [new("replace", S("replace")), new("remove", S("remove")), ReservedRow(), BackRow()];
            case Page.Capture: return [BackRow()];
            case Page.Review: return [new("accept", S("accept")), BackRow()];
            default: throw new InvalidOperationException();
        }
    }

    private string EditorMessage()
    {
        if (_page == Page.Capture) return S(_keyboard ? "listen-key" : "listen-pad");
        if (_page is not (Page.Device or Page.Alternatives or Page.Operation or Page.Review)) return "";
        var installedAction = Enum.Parse<GameAction>(Effective(_action, Input.Settings), true);
        var pending = _page == Page.Review ? _proposal! : Row;
        string text = S("installed", Input.Hint(installedAction)) + "\n" + S("pending", ListText(All(pending)));
        if (_page == Page.Review)
        {
            var first = Family(pending).FirstOrDefault();
            string compact = first != null ? LabelFor(first) : !_keyboard && pending.Keys.Length > 0 ?
                T("input.keyboard-only", Input.DescribeBinding(pending.Keys[0], true)) : S("none");
            text += "\n" + S("review", compact);
            var preview = Preview(pending);
            var oldMenu = GameInput.BaseRows(_draft)["menu"]; var nextMenu = GameInput.BaseRows(preview)["menu"];
            if (_action != "menu" && !All(oldMenu).SequenceEqual(All(nextMenu))) text += "\n" + S("fallback", ListText(All(nextMenu)));
        }
        return text;
    }

    private void EditorActivate(string id, InputEvent input)
    {
        if (_page == Page.Actions) { _action = id["action-".Length..]; Enter(Page.Device); return; }
        if (_page == Page.Device) { _keyboard = id == "keyboard"; Enter(Page.Alternatives); return; }
        if (_page == Page.Alternatives)
        {
            _original = id is "add" or "reserved" ? null : Family(Row).ElementAt(int.Parse(id["slot-".Length..]));
            if (_original is not null) { Enter(Page.Operation); return; }
        }
        if (id is "replace" or "add")
        {
            _proposalDevice = null;
            _drain = input is InputEventMouse ? null : input;
            Enter(Page.Capture); return;
        }
        if (id == "remove") Propose(null);
        if (id == "reserved") Propose(_keyboard ? new(Kind.Key, "Escape") : new(Kind.Button, "Back"));
        if (id == "accept")
        {
            _draft.Bindings[_action] = _proposal!; _proposal = null; _proposalDevice = null;
            ReturnToAlternatives();
        }
    }

    private void ReturnToAlternatives()
    {
        while (_page != Page.Alternatives && _parents.Count > 0) Back();
    }

    private InputSettings Preview(ActionBinding row)
    {
        var candidate = _draft.Draft(); candidate.Bindings[_action] = row; return candidate;
    }

    private void Propose(Alternative? next)
    {
        var options = All(Row).ToList();
        var index = _original == null ? -1 : options.IndexOf(_original);
        if (index >= 0) options.RemoveAt(index);
        if (next != null)
        {
            if (options.Count >= 32) { Reject(S("limit")); return; }
            if (options.Contains(next)) { Reject(S("duplicate", LabelFor(next), S(_action))); return; }
            if (_original?.Kind == next.Kind && index >= 0) options.Insert(index, next); else options.Add(next);
        }
        string[] Codes(Kind k) => options.Where(a => a.Kind == k).Select(a => a.Code).ToArray();
        var pending = new ActionBinding(Codes(Kind.Key), Codes(Kind.Button), Codes(Kind.Axis));
        if (next != null)
        {
            var other = GameInput.BaseRows(Preview(pending)).FirstOrDefault(p => p.Key != _action && All(p.Value).Contains(next));
            if (other.Key != null) { Reject(S("duplicate", LabelFor(next), S(other.Key))); return; }
        }
        _proposal = pending;
        if (_page == Page.Capture) { _page = Page.Review; _status = ""; ShowPage(); }
        else Enter(Page.Review);
    }

    private void Reject(string error) { _status = error; Paint(); }

    // Capture belongs ahead of ordinary navigation. The caller still tracks mapped
    // releases without submitting actions, so a consumed release cannot strand Wait or movement.
    internal bool CaptureInput(InputEvent input)
    {
        if (input is InputEventKey { Pressed: false } key) _downKeys.Remove(key.Keycode);
        if (input is InputEventJoypadButton { Pressed: false } button) _downButtons.Remove((button.Device, button.ButtonIndex));
        if (input is InputEventJoypadMotion axis && Math.Abs(axis.AxisValue) < .35f) _tilted.Remove((axis.Device, axis.Axis));
        bool fresh = input switch {
            InputEventKey k when k.Pressed => !k.Echo && _downKeys.Add(k.Keycode),
            InputEventJoypadButton b when b.Pressed => _downButtons.Add((b.Device, b.ButtonIndex)),
            InputEventJoypadMotion a when Math.Abs(a.AxisValue) >= .5f => _tilted.Add((a.Device, a.Axis)),
            _ => false,
        };
        if (_drain is { } old)
        {
            bool released = (old, input) switch {
                (InputEventKey a, InputEventKey b) => a.Keycode == b.Keycode && !b.Pressed,
                (InputEventJoypadButton a, InputEventJoypadButton b) => a.Device == b.Device && a.ButtonIndex == b.ButtonIndex && !b.Pressed,
                (InputEventJoypadMotion a, InputEventJoypadMotion b) => a.Device == b.Device && a.Axis == b.Axis && Math.Abs(b.AxisValue) < .35f,
                _ => false };
            if (released) _drain = null;
            return true;
        }
        if (_page != Page.Capture) return false;
        if (!GetWindow().HasFocus()) return true;
        if (input is InputEventMouse) { HandlePointer(input); return true; }
        if (!fresh) return true;
        if (input is InputEventKey { Keycode: Key.Escape } or InputEventJoypadButton { ButtonIndex: JoyButton.Back })
        { _drain = input; Back(); return true; }
        if (_keyboard != (input is InputEventKey)) return true;
        _drain = input;
        _proposalDevice = input is InputEventKey ? null : input.Device;
        Alternative? value = input switch {
            InputEventKey k when !k.CtrlPressed && !k.AltPressed && !k.MetaPressed && (!k.ShiftPressed || k.Keycode == Key.Shift) &&
                Enum.IsDefined(k.Keycode) && k.Keycode is not (Key.None or Key.Ctrl or Key.Alt or Key.Meta) &&
                (k.Keycode != Key.Shift || _action == "accelerate") => new(Kind.Key, k.Keycode.ToString()),
            InputEventJoypadButton b when ButtonName(b.ButtonIndex) is { } name => new(Kind.Button, name),
            InputEventJoypadMotion a when a.Axis is JoyAxis.LeftX or JoyAxis.LeftY or JoyAxis.RightX or JoyAxis.RightY =>
                new(Kind.Axis, a.Axis + (a.AxisValue < 0 ? "-" : "+")),
            _ => null };
        if (value == null) Reject(S("unsupported", input.AsText(), S(_action))); else Propose(value);
        return true;
    }

    private static string? ButtonName(JoyButton button) => button switch {
        JoyButton.A => "South", JoyButton.B => "East", JoyButton.X => "West", JoyButton.Y => "North",
        JoyButton.DpadUp or JoyButton.DpadRight or JoyButton.DpadDown or JoyButton.DpadLeft or
        JoyButton.LeftShoulder or JoyButton.RightShoulder or JoyButton.Start or JoyButton.Back or
        JoyButton.LeftStick or JoyButton.RightStick => button.ToString(), _ => null };

    internal void AbortCapture(int? disconnectedDevice = null)
    {
        if (disconnectedDevice is { } device)
        {
            _tilted.RemoveWhere(x => x.Device == device); _downButtons.RemoveWhere(x => x.Device == device);
            if (_proposalDevice != device && _drain?.Device != device && (_page != Page.Capture || _keyboard)) return;
        }
        else { _tilted.Clear(); _downButtons.Clear(); _downKeys.Clear(); }
        _drain = null; _proposal = null; _proposalDevice = null;
        if (_page is Page.Review or Page.Capture) Back();
    }
}

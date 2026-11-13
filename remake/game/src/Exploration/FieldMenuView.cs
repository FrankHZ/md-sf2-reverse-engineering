using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Disposable focus and gestures only; session queries and consumers own every action.
internal sealed partial class FieldMenuView : Control
{
    private readonly FieldServiceMenu _menu = new("field-menu", ("Member", "field-menu.member"),
        ("Magic", "field-menu.magic"), ("Item", "field-menu.item"), ("Search", "field-menu.search"));
    private readonly ColorRect _shade = new() { Color = new(0, 0, 0, 0.35f), MouseFilter = MouseFilterEnum.Ignore };
    internal required Label Entry { get; init; }
    private readonly MenuPointer _pointer = new();
    private GameInput _input = null!;
    private Action<int> _choose = null!;
    private Action _close = null!, _begin = null!;
    private ExplorationVisuals? _visuals;
    private bool _configured, _requiresRelease = true;
    private readonly bool[] _available = new bool[4];
    private int _index;
    private long _generation;
    internal void ProtectSubject(Rect2? subject) { _menu.ProtectedSubject = subject; }
    internal bool Open => _menu.Visible;
    internal Rect2? OccupiedRect => Open ? _menu.GetGlobalRect() : null;
    internal string? AssetError { get; private set; }
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        _shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_shade); AddChild(_menu);
        Resized += _pointer.Cancel;
        _menu.InvalidatePointer = _pointer.Cancel; HideMenu(); Entry.Hide();
    }
    internal void Begin() { _generation++; _menu.Show(); _shade.Show(); Entry.Hide(); Suspend(); }
    internal void HideMenu() { _generation++; _menu.Hide(); _shade.Hide(); Suspend(); }
    internal void Present(GameInput input, ExplorationVisuals? visuals, bool entry, bool party, bool preparation, bool search,
        Action begin, Action<int> choose, Action close)
    {
        _input = input; _begin = begin; _choose = choose; _close = close;
        if (!_configured || !ReferenceEquals(_visuals, visuals))
        {
            _configured = true; _visuals = visuals; AssetError = null;
            try { _menu.Configure(visuals?.FieldMenu); }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException) { AssetError = error.Message; }
        }
        Entry.Visible = entry && !Open && AssetError is null;
        Entry.Text = input.CompactHint(GameAction.Cancel) + " · " + GameText.Get("field-menu.title");
        _pointer.ReleaseIfNeutral(); if (!input.GameplayHeld) _requiresRelease = false;
        for (int i = 0; i < 4; i++) _available[i] = i == 3 ? search : i == 0 ? party : preparation;
        if (!Open) return;
        string key = new[] { "member", "magic", "item", "search" }[Math.Min(_index, 3)];
        string reason = _index == 4 ? "" : GameText.Get("field-menu." + key + "-help") + (_available[_index] ? "" : "\n" + GameText.Get("field-menu.unavailable"));
        _menu.Present(input, _index, GameText.Get("field-menu.title"), reason, GameText.Get("prepare.hint", input.CompactHint(GameAction.Up), input.CompactHint(GameAction.Down), input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel)) + "\n" + input.CompactHint(GameAction.Left) + "/" + input.CompactHint(GameAction.Right) + " · ↕", "prepare.back", _available);
    }
    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); _menu.Suspend(); }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_requiresRelease || !Open) return;
        if (action == GameAction.Cancel) { _close(); return; }
        if (action is GameAction.Left or GameAction.Right) { _menu.Wheel(action == GameAction.Left ? -192 : 192); return; }
        if (action is GameAction.Up or GameAction.Down) _index = (_index + (action == GameAction.Up ? -1 : 1) + 5) % 5;
        else if (action == GameAction.Confirm)
        { if (_index == 4) _close(); else if (_available[_index]) { Suspend(); _choose(_index); } }
    }
    internal void HandlePointer(InputEvent input)
    {
        if (input is not InputEventMouse mouse) return;
        int hit = Open ? _menu.Hit(mouse.Position) : -1;
        bool back = Open && _menu.BackHit(mouse.Position), entry = Entry.IsVisibleInTree() && Entry.GetGlobalRect().HasPoint(mouse.Position);
        if (input is InputEventMouseMotion motion)
        { if (_pointer.Moved(motion) && !_requiresRelease && (hit >= 0 || back)) _index = back ? 4 : hit; return; }
        if (input is not InputEventMouseButton button) return;
        if (Open && button.Pressed && button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        { _menu.Wheel(button.ButtonIndex == MouseButton.WheelUp ? -112 : 112); return; }
        bool right = Open && button.ButtonIndex == MouseButton.Right;
        string? id = back || right ? "back" : entry ? "entry" : hit >= 0 ? hit.ToString() : null;
        Rect2 rect = right ? GetGlobalRect() : back ? _menu.BackRect : entry ? Entry.GetGlobalRect() : hit >= 0 ? _menu.RowRect(hit) : default;
        string? clicked = _pointer.Read(button, id, rect, (_generation, Open, Entry.Visible));
        if (_requiresRelease || clicked is null) return;
        if (clicked == "entry") _begin(); else if (clicked == "back") _close();
        else { _index = int.Parse(clicked); if (_available[_index]) { Suspend(); _choose(_index); } }
    }
    internal object Observe() => new { visible = Open && IsVisibleInTree(), index = _index, requiresRelease = _requiresRelease,
        readingOverflow = _menu.ReadingOverflow,
        entryVisible = Entry.IsVisibleInTree(), entryRect = BattleMapViewport.Rectangle(Entry.GetGlobalRect()), menu = _menu.Observe() };
}

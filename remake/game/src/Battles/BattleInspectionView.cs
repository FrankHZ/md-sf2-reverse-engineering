using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;
using Sf2.Remake.GodotAdapter.Exploration;

namespace Sf2.Remake.GodotAdapter.Battles;

// Only disposable focus/caller state. Every projection is queried from the live session.
internal sealed partial class BattleInspectionView : Control
{
    private GameSession? _session;
    private BattleInspectionSnapshot? _snapshot;
    private (SessionSnapshot Snapshot, BattleMovementRange Range)? _cursorRange;
    private GameInput _input = null!;
    private BattleMapViewport _map = null!;
    private readonly PartyDetailsPanel _details = new() { Name = "Details" };
    private readonly Label _hint = new() { Name = "InspectionHint", AutowrapMode = TextServer.AutowrapMode.WordSmart };
    private readonly BattleOverviewView _overview = new() { Name = "Overview" };
    private readonly BattleMemberListView _memberList = new() { Name = "MemberList" };
    private BattleMemberSnapshot? _members;
    private readonly BattleInspectionOptions _options = new();
    private readonly BattleInspectionCard _hover = new() { Name = "HoverCard" };
    private readonly MenuPointer _pointer = new();
    private string? _optionsError;
    internal void ConfigureOptions(Sf2.Remake.Application.Content.Scenarios.BattleSceneDefinition? scene)
    {
        _optionsError = null;
        try { _options.Configure(scene?.Rasters, "battlefield-menu."); }
        catch (InvalidOperationException error) { _optionsError = error.Message; }
    }
    private int _optionIndex;
    private string _page = "Cursor";
    private MapPosition _cursor = new(0, 0);
    private BattleInspectionUnit? _unit;
    private BattleLandEffect? _land;
    private int _knownIndex;
    internal bool Active => _snapshot is not null;
    internal string Page => _page;
    internal Action? RequestSpeed { get; set; }
    internal Action? RequestQuit { get; set; }
    internal void ConfigureIcons(IdentityIcons icons, Sf2.Remake.Application.Content.Scenarios.ScenarioDefinition definition) =>
        _details.ConfigureIcons(icons, definition);

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore;
        Theme = StoryTheme.Create(); ZIndex = 90;
        AddChild(_details); AddChild(_overview); AddChild(_memberList); AddChild(_hint); AddChild(_options); AddChild(_hover);
        _options.InvalidatePointer = _pointer.Cancel;
        _hint.AddThemeFontSizeOverride("font_size", 24);
        Close();
    }
    public override void _EnterTree()
    { GetWindow().MouseExited += StopEdge; GetWindow().FocusExited += StopEdge; }
    internal void SuspendPointer() { StopEdge(); _pointer.Suspend(); _options.Suspend(); }
    public override void _Notification(int what)
    {
        bool hidden = what == NotificationVisibilityChanged && !IsVisibleInTree();
        if (hidden) _cursorRange = null;
        if (what == NotificationPaused || what == NotificationUnpaused || hidden) SuspendPointer();
    }
    public override void _ExitTree()
    { GetWindow().MouseExited -= StopEdge; GetWindow().FocusExited -= StopEdge; SuspendPointer(); }
    internal bool Open(GameSession session, GameInput input, BattleMapViewport map)
    {
        var query = session.QueryBattleInspection();
        if (!query.Available) return false;
        _session = session; _input = input; _map = map; _snapshot = query;
        _cursor = query.Units.Single(u => u.Actor == query.ActiveActor).Position;
        _pointer.Suspend(); _page = "Cursor"; _unit = null; _land = null; _members = null; _optionIndex = 0; _memberList.Roster.Reset(); Show(); Project(); return true;
    }
    internal void Close()
    {
        _snapshot = null; _session = null; _unit = null; _land = null; _members = null; _cursorRange = null;
        StopEdge(); _pointer.Suspend(); _options.Suspend(); _map?.Inspect(null); Hide();
    }
    internal bool Refresh(GameSession? session, bool afterSave = false)
    {
        if (_snapshot is null) return false;
        // Only the paused save caller may rebind after the host publishes real music.
        var query = session?.QueryBattleInspection(_snapshot.SessionId, afterSave ? null : _snapshot.Revision);
        if (session != _session || query?.Available != true || query.ActiveActor != _snapshot.ActiveActor)
        { Close(); return false; }
        _snapshot = query; Project(); return true;
    }
    internal void Handle(GameAction action)
    {
        if (!Active) return;
        StopEdge(); _pointer.Cancel(); _pointerFocus = false;
        bool a = action == GameAction.Attack, c = action == GameAction.Confirm, b = action == GameAction.Cancel;
        if (_page is "Full" or "MemberFull")
        {
            if (a || b || c)
            {
                _page = _page == "MemberFull" ? "Members" : "Cursor"; _unit = null; _land = null;
            }
            else if (_page is "Full" or "MemberFull")
            {
                if (action == GameAction.Target) _details.FocusPane = _details.FocusPane == 1 ? 2 : 1;
                if (action is GameAction.Up or GameAction.Down) _details.ScrollFocused(action == GameAction.Up ? -72 : 72);
            }
        }
        else if (_page == "Map")
        {
            if (b) _page = "Options";
            else if (action is GameAction.Up or GameAction.Down && _snapshot!.Units.Count > 0)
                _knownIndex = (_knownIndex + (action == GameAction.Up ? -1 : 1) + _snapshot.Units.Count) % _snapshot.Units.Count;
        }
        else if (_page == "Options")
        {
            if (b) _page = "Cursor";
            else if (action is GameAction.Up or GameAction.Down)
                _optionIndex = (_optionIndex + (action == GameAction.Up ? -1 : 1) + 4) % 4;
            else if ((a || c) && _optionsError is null)
            {
                _pointer.Suspend();
                if (_optionIndex == 2) { RequestSpeed?.Invoke(); return; }
                if (_optionIndex == 3) { RequestQuit?.Invoke(); return; }
                _page = _optionIndex == 1 ? "Map" : "Members"; _knownIndex = 0; _memberList.Roster.Reset();
            }
        }
        else if (_page == "Members")
        {
            if (b) _page = "Options";
            else if (action is GameAction.Up or GameAction.Down)
                _memberList.Roster.Select(_memberList.Roster.Index + (action == GameAction.Up ? -1 : 1));
            else if (a || c) OpenMemberDetails();
        }
        else if (b) { Close(); return; }
        else if (a || c)
        {
            var marker = _snapshot!.Units.FirstOrDefault(u => u.Position == _cursor);
            if (marker is null) _page = "Options";
            else
            {
                var query = _session!.QueryBattleInspection(_snapshot.SessionId, _snapshot.Revision, marker.Actor);
                if (!query.Available || query.Unit is null) { Close(); return; }
                _unit = query.Unit; _land = query.Land; _page = "Full";
                _details.FocusPane = 1; _details.ResetReading();
            }
        }
        else
        {
            int x = _cursor.X, y = _cursor.Y;
            if (action == GameAction.Up) y--; if (action == GameAction.Down) y++;
            if (action == GameAction.Left) x--; if (action == GameAction.Right) x++;
            _cursor = new(Math.Clamp(x, 0, _snapshot!.Width - 1), Math.Clamp(y, 0, _snapshot.Height - 1));
        }
        Project();
    }
    private void OpenMemberDetails()
    {
        if (_memberList.Roster.Actor is not { } actor) return;
        var query = _session!.QueryBattleMembers(_snapshot!.SessionId, _snapshot.Revision, actor);
        if (!query.Available) { Close(); return; }
        if (query.Unit is not { } detail) return;
        _unit = detail; _page = "MemberFull"; _land = null;
        _details.FocusPane = 1; _details.ResetReading();
    }

    internal void HandlePointer(InputEvent input)
    {
        if (_page == "Members") { HandleMemberPointer(input); return; }
        if (BoardPointer(input)) return;
        if (_page == "Options")
        {
            if (input is InputEventMouseMotion motion)
            { if (_pointer.Moved(motion) && _options.Hit(motion.Position) is >= 0 and var index) { _optionIndex = index; Project(); } return; }
            if (input is not InputEventMouseButton button) return;
            int hit = _options.Hit(button.Position);
            bool back = button.ButtonIndex == MouseButton.Right || _options.BackHit(button.Position);
            Rect2 rect = back ? button.ButtonIndex == MouseButton.Right ? GetGlobalRect() : _options.BackRect :
                hit < 0 ? default : _options.HitRect(hit);
            string? clicked = _pointer.Read(button, back ? "back" : hit < 0 ? null : hit.ToString(), rect, _page);
            if (clicked == "back") Handle(GameAction.Cancel);
            else if (clicked is not null) { _optionIndex = int.Parse(clicked); Handle(GameAction.Confirm); }
            return;
        }
        if (_page is "Full" or "MemberFull" && input is InputEventMouseButton { Pressed: true } mouse &&
            mouse.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
            _details.Wheel(mouse.Position, mouse.ButtonIndex == MouseButton.WheelUp ? -72 : 72);
    }
    private void HandleMemberPointer(InputEvent input)
    {
        if (input is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        if (input is not InputEventMouseButton button) return;
        if (button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            if (button.Pressed) _memberList.Roster.Wheel(button.Position, button.ButtonIndex == MouseButton.WheelUp ? -112 : 112);
            _pointer.Cancel();
            return;
        }

        bool back = button.ButtonIndex == MouseButton.Right || _hint.GetGlobalRect().HasPoint(button.Position);
        var (index, id, rect) = _memberList.Roster.Hit(button.Position);
        if (back)
        {
            id = "back";
            rect = button.ButtonIndex == MouseButton.Right ? GetGlobalRect() : _hint.GetGlobalRect();
        }
        string? clicked = _pointer.Read(button, id, rect,
            (_page, _snapshot!.SessionId, _snapshot.Revision, _memberList.Roster.Actor, _memberList.Roster.ScrollVertical));
        if (clicked is null) return;
        if (back) { Handle(GameAction.Cancel); return; }
        _memberList.Roster.Select(index, reveal: false);
        OpenMemberDetails();
        Project();
    }

    private static string UnitName(BattleInspectionMarker unit) => DisplayNames.InspectionName(unit.Ally, unit.NameIndex, unit.Name, unit.CustomName);
    internal void RefreshInputHints() => Project();

    private void Project()
    {
        if (_snapshot is null) return;
        var at = _page == "Cursor" ? _snapshot.Units.FirstOrDefault(u => u.Position == _cursor) : null;
        bool showCursor = _page == "Cursor" && IsVisibleInTree();
        _map.Inspect(_cursor, !_pointerFocus, showCursor, CursorRange(showCursor ? at?.Actor : null),
            _input.Settings.ReducedFlash);
        _pointer.ReleaseIfNeutral();
        _options.Visible = _page == "Options"; _hint.Visible = _page != "Options";
        var size = GetViewportRect().Size;
        _details.Visible = _page is "Full" or "MemberFull"; _memberList.Visible = _page == "Members"; _overview.Visible = _page == "Map";
        _hover.Visible = _page == "Cursor";
        if (_hover.Visible) PresentHover();
        string a = _input.CompactHint(GameAction.Attack), c = _input.CompactHint(GameAction.Confirm), b = _input.CompactHint(GameAction.Cancel);
        string close = GameText.Get("battle.inspect.close", a, c, b);
        string up = _input.CompactHint(GameAction.Up), down = _input.CompactHint(GameAction.Down);
        string message;
        if (_unit is { } unit)
        {
            string name = DisplayNames.InspectionName(unit.Ally, unit.NameIndex, unit.Name, unit.CustomName);
            _details.Size = new(Mathf.Min(1000, size.X - 64), Mathf.Min(860, size.Y - 180));
            _details.Position = (size - _details.Size) / 2 - new Vector2(0, 36); _details.Present(unit, name);
            message = GameText.Get("battle.inspect.detail-help", close, _input.CompactHint(GameAction.Target), up, down);
        }
        else if (_page == "Options")
        {
            message = (_optionsError is null ? "" : _optionsError + "\n") +
                GameText.Get("battle.choice-help", up + "/" + down) + GameText.Get("battle.confirm-help", c, b);
        }
        else if (_page == "Members")
        {
            _members = _session!.QueryBattleMembers(_snapshot.SessionId, _snapshot.Revision);
            if (!_members.Available) { Close(); return; }
            _memberList.Size = new(Mathf.Min(820, size.X - 64), Mathf.Min(800, size.Y - 180));
            _memberList.Position = (size - _memberList.Size) / 2 - new Vector2(0, 36);
            _memberList.Present(_members, _map, _input.Settings.GraphicsMode == "hd");
            message = GameText.Get("battle.member.help", up, down, a, c, b);
        }
        else if (_page == "Map")
        {
            _overview.Position = new(StoryTheme.GameplayInset, StoryTheme.BelowMenu);
            _overview.Size = new(size.X - 2 * StoryTheme.GameplayInset, size.Y - 132 - _overview.Position.Y);
            _overview.Present(_snapshot, _map.CameraCells, _knownIndex);
            message = GameText.Get("battle.inspect.map-help", up, down, b);
        }
        else
        {
            message = GameText.Get("battle.inspect.cursor", _cursor.X, _cursor.Y, at is null ? GameText.Get("battle.inspect.empty") : UnitName(at)) +
                "\n" + GameText.Get("battle.inspect.cursor-help", a, c, b);
        }
        if (_page == "Options") _options.Present(_input, _optionIndex, "", message, available: _optionsError is null ? null : [false, false, false, false]);
        _hint.Text = message; _hint.Position = new(24, size.Y - 120); _hint.Size = new(size.X - 48, 112);
    }
    private BattleMovementRange? CursorRange(ActorRef? actor)
    {
        if (actor is not { } unit)
        {
            _cursorRange = null;
            return null;
        }
        var current = _session!.Current;
        if (_cursorRange is { } cached && cached.Snapshot == current && cached.Range.Actor == unit)
            return cached.Range;

        var range = _session.QueryBattleInspectionRange(_snapshot!.SessionId, _snapshot.Revision, unit);
        _cursorRange = range is null ? null : (current, range);
        return range;
    }

    internal object Observe() => new
    {
        active = Active, page = Active ? _page : null, cursor = Active ? _cursor : null,
        sessionId = _snapshot?.SessionId, revision = _snapshot?.Revision, actor = _snapshot?.ActiveActor?.Value,
        options = _options.Observe(), optionsError = _optionsError, optionIndex = _optionIndex, memberIndex = _memberList.Roster.Index, members = _members?.Members,
        roster = _memberList.IsVisibleInTree() ? _memberList.Observe() : null, unit = _unit, land = _land, units = _snapshot?.Units, terrain = _snapshot?.Terrain, hint = _hint.Text,
        full = _details.IsVisibleInTree() ? _details.Observe() : null,
        hover = _hover.IsVisibleInTree() ? _hover.Observe() : null, edge = new { speed = new { x = _edgeSpeed.X, y = _edgeSpeed.Y }, delay = _edgeDelay },
        overview = _overview.IsVisibleInTree() ? _overview.Observe() : null,
    };
}

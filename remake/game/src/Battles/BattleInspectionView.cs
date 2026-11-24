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
    private readonly PanelContainer _footer = new() { Name = "InspectionFooter", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _hint = new() { Name = "Instructions", AutowrapMode = TextServer.AutowrapMode.WordSmart,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
    private readonly Button _back = new() { Name = "Back", FocusMode = FocusModeEnum.None,
        SizeFlagsVertical = SizeFlags.ShrinkCenter, MouseFilter = MouseFilterEnum.Ignore };
    private readonly BattleOverviewView _overview = new() { Name = "Overview" };
    private readonly MemberPageView _memberPage = new() { Name = "Members" };
    private BattleMemberSnapshot? _members;
    private readonly BattleInspectionOptions _options = new();
    private readonly BattleTerrainPanel _terrainPanel = new() { Name = "TerrainPanel" };
    private readonly BattleStatusCard _hover = new() { Name = "HoverCard" };
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
    private string? _projectedPage;
    private MapPosition _cursor = new(0, 0);
    private BattleInspectionUnit? _unit;
    private BattleLandEffect? _land;
    private int _knownIndex;
    internal bool Active => _snapshot is not null;
    internal string Page => _page;
    internal Action? RequestSpeed { get; set; }
    internal Action? RequestQuit { get; set; }
    internal void ConfigureIcons(IdentityIcons icons, Sf2.Remake.Application.Content.Scenarios.ScenarioDefinition definition)
    {
        _details.ConfigureIcons(icons, definition);
        _memberPage.Details.ConfigureIcons(icons, definition);
    }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore;
        Theme = StoryTheme.Create(); ZIndex = 90;
        AddChild(_details); AddChild(_overview); AddChild(_memberPage);
        _memberPage.Back = Return; AddChild(_options); AddChild(_hover); AddChild(_terrainPanel);
        AddChild(_footer);
        var instructions = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        instructions.AddThemeConstantOverride("separation", 16);
        _footer.AddChild(instructions); instructions.AddChild(_hint); instructions.AddChild(_back);
        _hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        _back.Theme = StoryTheme.CreateEntry();
        _back.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        _back.ItemRectChanged += _pointer.Cancel;
        _options.InvalidatePointer = _pointer.Cancel;
        Resized += () => { _pointer.Cancel(); Project(); };
        Close();
    }
    public override void _EnterTree()
    { GetWindow().MouseExited += StopEdge; GetWindow().FocusExited += StopEdge; }
    internal void SuspendPointer() { StopEdge(); _pointer.Suspend(); _options.Suspend(); _memberPage.Suspend(); }
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
        _pointer.Suspend(); _page = "Cursor"; _unit = null; _land = null; _members = null; _optionIndex = 0; _memberPage.Reset(); Show(); Project(); return true;
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
        bool open = action is GameAction.Attack or GameAction.Confirm;
        if (_page == "Members")
        {
            _memberPage.Handle(action);
            Project();
            return;
        }
        bool details = _page == "Full";
        if (action == GameAction.Cancel || details && open)
        {
            Return();
            return;
        }
        if (details)
        {
            if (action == GameAction.Target) _details.FocusPane = _details.FocusPane == 1 ? 2 : 1;
            if (action is GameAction.Up or GameAction.Down) _details.ScrollFocused(action == GameAction.Up ? -72 : 72);
        }
        else if (_page == "Map")
        {
            if (action is GameAction.Up or GameAction.Down && _snapshot!.Units.Count > 0)
                _knownIndex = (_knownIndex + (action == GameAction.Up ? -1 : 1) + _snapshot.Units.Count) % _snapshot.Units.Count;
        }
        else if (_page == "Options")
        {
            if (action is GameAction.Up or GameAction.Down)
                _optionIndex = (_optionIndex + (action == GameAction.Up ? -1 : 1) + 4) % 4;
            else if (open && _optionsError is null)
            {
                _pointer.Suspend();
                if (_optionIndex == 2) { RequestSpeed?.Invoke(); return; }
                if (_optionIndex == 3) { RequestQuit?.Invoke(); return; }
                _page = _optionIndex == 1 ? "Map" : "Members"; _knownIndex = 0; _memberPage.Reset();
            }
        }
        else if (open)
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
    private void Return()
    {
        if (_page == "Cursor") { Close(); return; }
        _unit = null; _land = null; _members = null;
        _page = _page switch
        {
            "Map" or "Members" => "Options",
            _ => "Cursor",
        };
        Project();
    }

    internal void HandlePointer(InputEvent input)
    {
        if (input is not InputEventMouse mouse) return;
        if (_page == "Members")
        {
            _memberPage.HandlePointer(input);
            Project();
            return;
        }
        if (mouse is InputEventMouseButton { ButtonIndex: MouseButton.Right } right)
        {
            StopEdge();
            if (_pointer.Read(right, "back", GetGlobalRect(), (_page, _snapshot!.SessionId, _snapshot.Revision)) is not null)
                Handle(GameAction.Cancel);
            return;
        }
        var point = mouse.Position;
        bool back = _back.IsVisibleInTree() && _back.GetGlobalRect().HasPoint(point);
        if (_page == "Cursor" && !back && !CursorUiCovers(point) && !HostPointerCovered)
        {
            BoardPointer(mouse);
            return;
        }

        if (_page == "Cursor") StopEdge();
        if (mouse is InputEventMouseMotion motion)
        {
            if (!_pointer.Moved(motion) || _footer.GetGlobalRect().HasPoint(point)) return;
            if (_page == "Options" && _options.Hit(point) is >= 0 and var option)
            { _optionIndex = option; Project(); }
            if (_page == "Map" && _overview.Hit(point) is >= 0 and var known)
            { _knownIndex = known; Project(); }
            return;
        }
        if (mouse is not InputEventMouseButton button) return;
        StopEdge();
        if (button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            _pointer.Cancel();
            if (!button.Pressed) return;
            int step = button.ButtonIndex == MouseButton.WheelUp ? -72 : 72;
            if (_page == "Full") _details.Wheel(point, step);
            return;
        }

        int index = -1;
        string? id = back ? "back" : null;
        Rect2 rect = back ? _back.GetGlobalRect() : default;
        if (!back && button.ButtonIndex == MouseButton.Left && !_footer.GetGlobalRect().HasPoint(point))
        {
            if (_page == "Options" && _options.Hit(point) is >= 0 and var option)
                { index = option; id = "option:" + option; rect = _options.HitRect(option); }
            else if (_page == "Map" && _overview.Hit(point) is >= 0 and var known)
                { index = known; id = "unit:" + known; rect = _overview.GetGlobalRect(); }
        }
        string? clicked = _pointer.Read(button, id, rect,
            (_page, _snapshot!.SessionId, _snapshot.Revision));
        if (clicked is null) return;
        if (clicked == "back") Handle(GameAction.Cancel);
        else if (_page == "Options") { _optionIndex = index; Handle(GameAction.Confirm); }
        else if (_page == "Map")
        {
            _cursor = _snapshot.Units[index].Position; _page = "Cursor"; _pointerFocus = false; Project();
        }
    }

    private static string UnitName(BattleInspectionMarker unit) => DisplayNames.InspectionName(unit.Ally, unit.NameIndex, unit.Name, unit.CustomName);
    internal void RefreshInputHints() => Project();

    private void Project()
    {
        if (_snapshot is null) return;
        if (_projectedPage != _page) { _pointer.Suspend(); StopEdge(); _projectedPage = _page; }
        var at = _page == "Cursor" ? _snapshot.Units.FirstOrDefault(u => u.Position == _cursor) : null;
        bool showCursor = _page == "Cursor" && IsVisibleInTree();
        _map.Inspect(_cursor, !_pointerFocus, showCursor, CursorRange(showCursor ? at?.Actor : null),
            _input.Settings.ReducedFlash, enemy: at is { Ally: false });
        _pointer.ReleaseIfNeutral();
        _options.Visible = _page == "Options";
        var size = GetViewportRect().Size;
        _footer.Visible = _page != "Members";
        _details.Visible = _page == "Full"; _memberPage.Visible = _page == "Members"; _overview.Visible = _page == "Map";
        _terrainPanel.Visible = _page == "Cursor";
        if (_terrainPanel.Visible) PresentHover();
        else _hover.Hide();
        string a = _input.CompactHint(GameAction.Attack), c = _input.CompactHint(GameAction.Confirm), b = _input.CompactHint(GameAction.Cancel);
        string up = _input.CompactHint(GameAction.Up), down = _input.CompactHint(GameAction.Down);
        string message;
        if (_unit is { } unit)
        {
            string name = DisplayNames.InspectionName(unit.Ally, unit.NameIndex, unit.Name, unit.CustomName);
            _details.Size = new(Mathf.Min(1000, size.X - 64), Mathf.Min(860, size.Y - 192));
            _details.Position = (size - _details.Size) / 2 - new Vector2(0, 36); _details.PresentBattle(unit, name);
            message = GameText.Get("battle.inspect.detail-help", _input.CompactHint(GameAction.Target), up, down);
        }
        else if (_page == "Options")
        {
            message = (_optionsError is null ? "" : _optionsError + "\n") +
                GameText.Get("battle.inspect.options", up, down, a, c);
        }
        else if (_page == "Members")
        {
            PresentMembers();
            return;
        }
        else if (_page == "Map")
        {
            _overview.Position = new(StoryTheme.GameplayInset, StoryTheme.BelowMenu);
            _overview.Size = new(size.X - 2 * StoryTheme.GameplayInset, size.Y - 144 - _overview.Position.Y);
            _overview.Present(_snapshot, _map.CameraCells, _knownIndex);
            message = GameText.Get("battle.inspect.map-help", up, down);
        }
        else
        {
            message = GameText.Get("battle.inspect.cursor-help", a, c);
        }
        if (_page == "Options") _options.Present(_input, _optionIndex, available: _optionsError is null ? null : [false, false, false, false]);
        _hint.Text = message;
        _back.Text = (_page == "Full" ? $"{a}/{c}/{b}" : b)
            + " · " + GameText.Get(_page == "Cursor" ? "battle.inspect.return-movement" : "battle.back");
        _back.Visible = _page is "Full" or "Cursor";
        if (!_back.Visible) _hint.Text += "\n" + _back.Text;
        _footer.Size = new(Mathf.Min(1040, size.X - 2 * StoryTheme.GameplayInset), 100);
        _footer.Position = new((size.X - _footer.Size.X) / 2, size.Y - StoryTheme.GameplayInset - _footer.Size.Y);
    }
    private void PresentMembers()
    {
        var snapshot = _snapshot!;
        var session = _session!;
        if (_members?.SessionId != snapshot.SessionId || _members.Revision != snapshot.Revision)
            _members = session.QueryBattleMembers(snapshot.SessionId, snapshot.Revision);
        if (!_members.Available) { Close(); return; }
        var entries = _members.Members.Select(member => new MemberRosterEntry(member.Actor,
            DisplayNames.InspectionName(true, member.NameIndex, member.Name, member.CustomName),
            !member.Available ? GameText.Get("party.value-unavailable") : member.Defeated ? GameText.Get("party.defeated") :
                !member.Deployed ? GameText.Get("party.absent") : "",
            member.Available, _map.RosterPortrait(member.Actor, _input.Settings.GraphicsMode == "hd"))).ToArray();
        _memberPage.PresentRoster(entries, GameText.Get("battle.member.title"), _input);
        var selected = _memberPage.Actor;
        if (selected is not null && _members.Unit?.Actor != selected &&
            (entries[_memberPage.Index].Available || _members.Unit is not null))
            _members = session.QueryBattleMembers(snapshot.SessionId, snapshot.Revision, selected);
        string name = entries.Length == 0 ? GameText.Get("battle.member.empty") : entries[_memberPage.Index].Name;
        if (_members.Unit is { } unit) _memberPage.Details.PresentBattle(unit, name, session, entries[_memberPage.Index].Condition);
        else _memberPage.Details.Present(null, name, session);
        string hint = GameText.Get("battle.member.help", _input.CompactHint(GameAction.Up), _input.CompactHint(GameAction.Down),
            _memberPage.FocusLabel, _input.CompactHint(GameAction.Target), _input.CompactHint(GameAction.Cancel),
            _input.CompactHint(GameAction.Left), _input.CompactHint(GameAction.Right));
        _memberPage.PresentFooter(hint, GameText.Get("battle.back"));
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
        options = _options.Observe(), optionsError = _optionsError, optionIndex = _optionIndex, memberIndex = _memberPage.Index, members = _members?.Members,
        roster = _memberPage.IsVisibleInTree() ? _memberPage.Observe(true) : null, unit = _page == "Members" ? _members?.Unit : _unit, land = _land, units = _snapshot?.Units, terrain = _snapshot?.Terrain, hint = _page == "Members" ? _memberPage.Hint : _hint.Text,
        footer = new { visible = _footer.IsVisibleInTree(), rect = BattleMapViewport.Rectangle(_footer.GetGlobalRect()),
            hintRect = BattleMapViewport.Rectangle(_hint.GetGlobalRect()), back = _back.Text, backVisible = _back.IsVisibleInTree(),
            backRect = BattleMapViewport.Rectangle(_back.GetGlobalRect()) },
        full = _details.IsVisibleInTree() ? _details.Observe() : null,
        hover = _terrainPanel.IsVisibleInTree() ? _hover.Observe() : null,
        terrainPanel = _terrainPanel.IsVisibleInTree() ? _terrainPanel.Observe() : null, edge = new { speed = new { x = _edgeSpeed.X, y = _edgeSpeed.Y }, delay = _edgeDelay },
        overview = _overview.IsVisibleInTree() ? _overview.Observe() : null,
    };
}

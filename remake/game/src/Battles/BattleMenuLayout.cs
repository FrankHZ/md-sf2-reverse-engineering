using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Battles;

// Product UI geometry uses physical window units, independent of map tiles and source pixels.
internal sealed partial class BattleMenuLayout : Control
{
    private const int PanelInset = 16, Gap = 12;
    internal readonly UI.BattleStatusCard Actor = new() { Name = "ActorCard", DecisionHeader = true };
    internal readonly UI.BattleStatusCard Target = new() { Name = "TargetCard", Visible = false, DecisionHeader = true };
    internal readonly BattleTerrainPanel Terrain = new() { Name = "TerrainPanel" };
    internal readonly Label Context = Text("Context", UI.StoryTheme.BodySize);
    internal readonly Label Heading = Text("Heading", 32);
    internal readonly Label Help = Text("Help", UI.StoryTheme.HintSize);
    private readonly HBoxContainer _navigation = new() { Name = "Navigation", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _previous = Text("PreviousTarget", 26), _next = Text("NextTarget", 26);
    private readonly Label _confirm = Text("Confirm", 26);
    private UI.CompactCommandList _commandList = null!;
    internal readonly UI.DiamondCommandButton[] Commands = [
        new("Attack", "battle.command.attack", "command-menu") { CaptionBeside = true },
        new("Magic", "battle.command.magic", "command-menu") { CaptionBeside = true },
        new("Item", "battle.command.item", "command-menu") { CaptionBeside = true },
        new("Stay", "battle.command.stay", "command-menu") { CaptionBeside = true },
    ];
    private readonly UI.ItemDescriptionView _reading = new() { Name = "Reader", ReadingHeight = 0,
        SizeFlagsVertical = SizeFlags.ExpandFill };
    private readonly PanelContainer _context = Frame("ContextPanel");
    private readonly PanelContainer _detail = Frame("DescriptionPanel");
    private readonly PanelContainer _list = Frame("ChoicePanel");
    private readonly ScrollContainer _scroll = new() { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, FocusMode = FocusModeEnum.None };
    private readonly VBoxContainer _rows = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
    private readonly List<Label> _rowLabels = [];
    private readonly List<Label> _rowMarkers = [];
    private readonly List<HBoxContainer> _rowControls = [];
    private readonly List<UI.IdentityIcon> _rowIcons = [];
    internal readonly UI.IdentityIcons Icons = new();
    private string[] _labels = [];
    private int _selected;
    private Rect2 _subject;
    private bool _targeting, _decisionContext;
    private bool _canConfirm;
    internal Action InvalidatePointer { get; set; } = () => { };
    internal bool Covers(Vector2 point) =>
        Covers(Actor, point) || Covers(Target, point) || Covers(Terrain, point) || Covers(_context, point) || Covers(_list, point) ||
        Covers(_detail, point) || HitCommand(point).Id is not null;

    private static bool Covers(Control control, Vector2 point) =>
        control.IsVisibleInTree() && control.GetGlobalRect().HasPoint(point);

    internal void AvoidSubject(Rect2 rectangle)
    {
        if (_subject == rectangle) return;
        _subject = rectangle;
        Arrange(Size);
    }

    internal void ConfigureAssets(BattleSceneDefinition? content)
    {
        Icons.Configure(content?.Rasters);
        UI.MenuRasterTextures.Configure(Commands, content?.Rasters, "command-menu.", "command-menu");
    }

    public override void _Ready()
    {
        Theme = UI.StoryTheme.Create();
        var insets = UI.StoryTheme.PanelFrame(PanelInset, PanelInset);
        foreach (var panel in new[] { _detail, _list }) panel.AddThemeStyleboxOverride("panel", insets);
        MouseFilter = MouseFilterEnum.Ignore;
        AddChild(Actor); AddChild(Target);
        Context.AutowrapMode = TextServer.AutowrapMode.Off;
        AddChild(Terrain); AddChild(_context); _context.AddChild(Context);
        _commandList = new(Commands) { Name = "Commands", CaptionSize = 24, Visible = false,
            InvalidatePointer = () => InvalidatePointer() };
        _commandList.ItemRectChanged += () => InvalidatePointer();
        AddChild(_commandList);
        AddChild(_list); _list.AddChild(_scroll); _scroll.AddChild(_rows);
        _scroll.GetVScrollBar().FocusMode = FocusModeEnum.None;
        _rows.AddThemeConstantOverride("separation", 8);
        AddChild(_detail);
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", Gap);
        _detail.AddChild(column); column.AddChild(_reading); column.AddChild(_navigation);
        _reading.AddContext(Heading, Help);
        _scroll.GetVScrollBar().ValueChanged += _ => InvalidatePointer();
        foreach (var control in new[] { _previous, _next, _confirm })
        {
            control.CustomMinimumSize = new(0, 44);
            control.HorizontalAlignment = HorizontalAlignment.Center;
            _navigation.AddChild(control);
            control.ItemRectChanged += () => InvalidatePointer();
        }
        Actor.MinimumSizeChanged += () => Arrange(Size);
        Target.MinimumSizeChanged += () => Arrange(Size);
        _list.Hide();
    }

    // Native wrapped-label minimum sizes settle after container sorting. Settle geometry before
    // the next pointer event too, rather than moving a hit region only when that event redraws it.
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
        foreach (var command in Commands) command.AdvanceFocus(delta);
    }

    internal void StopFocusEmphasis()
    {
        foreach (var command in Commands) command.StopFocusEmphasis();
    }

    internal void Arrange(Vector2 size)
    {
        Size = size;
        float margin = UI.StoryTheme.GameplayInset, top = UI.StoryTheme.BelowMenu;
        var card = _targeting ? Target : Actor;
        float width = Mathf.Max(560, card.Visible ? card.RequestedSize.X : 0);
        width = Mathf.Min(width, size.X - margin * 2);
        float height = card.Visible ? Mathf.Max(card.RequestedSize.Y, card.GetCombinedMinimumSize().Y) : 0;
        float right = size.X - margin - width, bottom = size.Y - margin;
        float controlsTop = top + (card.Visible ? height + Gap : 0);
        // Allocate the reader once per phase and viewport. The selected offer can
        // change its scroll content without moving the controls or the footer.
        float footerHeight = _targeting ? 320 : _list.Visible ? 304 : _commandList.Visible ? 260 : 228;
        float detailTop = bottom - footerHeight;
        var protection = _subject.HasArea() ? _subject.Grow(16) : default;
        bool alternate = protection.HasArea() && new Rect2(right, top, width, bottom - top).Intersects(protection);
        float x = alternate ? margin : right;
        if (card.Visible)
        {
            card.Position = new(x, top);
            card.Size = new(width, height);
        }
        // Keep map readings opposite the active unit card.
        var readingSize = _context.GetCombinedMinimumSize();
        if (Terrain.Visible)
        {
            readingSize = readingSize.Max(Terrain.GetCombinedMinimumSize());
            Terrain.Size = readingSize;
        }
        _context.Size = readingSize;
        _context.Position = new(alternate ? size.X - margin - _context.Size.X : margin, top);
        _context.Visible = _decisionContext && (!protection.HasArea() || !_context.GetRect().Intersects(protection));
        if (Terrain.Visible)
            Terrain.Place(new(alternate ? size.X - margin : margin, top + (_context.Visible ? _context.Size.Y + Gap : 0)),
                protection, alternate);
        _commandList.Position = new(x, controlsTop);
        if (_commandList.Visible && _commandList.Size.X != width) _commandList.Arrange(width);
        _list.Position = new(x, controlsTop);
        _list.Size = new(width, Mathf.Max(0, detailTop - controlsTop - Gap));
        _detail.Position = new(x, detailTop);
        _detail.Size = new(width, footerHeight);
        foreach (var label in _rowLabels) label.CustomMaximumSize = new(Mathf.Max(1, width - 164), -1);
    }

    internal void ShowDecision(bool target, bool main, bool context)
    {
        _targeting = target;
        _decisionContext = context;
        _commandList.Visible = main;
        if (main) _commandList.Present(Commands.Select(command => command.Caption).ToArray());
        Heading.Visible = !main;
        Help.Visible = true;
        Arrange(Size);
    }

    internal void PresentDetail(string text, object subject, Input.GameInput? input) => _reading.Present(text, subject, input);
    internal void ReadDescription(int step) => _reading.Read(step);

    internal void ShowNavigation(bool visible, bool canCycle, bool showConfirm, bool canConfirm)
    {
        _previous.Text = GameText.Get("battle.previous"); _next.Text = GameText.Get("battle.next");
        _confirm.Text = GameText.Get("battle.confirm");
        _navigation.Visible = visible && (canCycle || showConfirm);
        _canConfirm = canConfirm;
        _previous.Visible = _next.Visible = canCycle;
        _confirm.Visible = showConfirm;
        _confirm.AddThemeColorOverride("font_color", canConfirm ? UI.StoryTheme.Selected : UI.StoryTheme.Unavailable);
    }

    internal (string? Id, Rect2 Rect) Hit(Vector2 position)
    {
        foreach (var control in new[] { _previous, _next, _confirm })
            if ((control != _confirm || _canConfirm) && Covers(control, position)) return (control.Name.ToString(), control.GetGlobalRect());
        var command = HitCommand(position);
        if (command.Id is not null) return command;
        if (_list.IsVisibleInTree() && _scroll.GetGlobalRect().HasPoint(position))
            for (int i = 0; i < _rowLabels.Count; i++)
            {
                var rect = _rowControls[i].GetGlobalRect().Intersection(_scroll.GetGlobalRect());
                if (rect.HasPoint(position)) return ($"row:{i}", rect);
            }
        return (null, default);
    }

    private (string? Id, Rect2 Rect) HitCommand(Vector2 position)
    {
        var (index, rectangle) = _commandList.Hit(position);
        return index >= 0 ? ($"command:{index}", rectangle) : (null, default);
    }

    internal void Wheel(Vector2 position, int step)
    {
        if (!_reading.Wheel(position, step) && Covers(_scroll, position)) _scroll.ScrollVertical += step;
    }

    internal void ShowRows(string[] labels, bool[] enabled, int selected, bool visible, string?[] identities)
    {
        bool changed = !labels.SequenceEqual(_labels) || selected != _selected || visible != _list.Visible;
        _list.Visible = visible;

        if (!labels.SequenceEqual(_labels))
        {
            InvalidatePointer();
            foreach (var child in _rows.GetChildren()) { _rows.RemoveChild(child); child.QueueFree(); }
            _rowLabels.Clear(); _rowMarkers.Clear(); _rowControls.Clear(); _rowIcons.Clear(); _labels = labels;
            foreach (string value in labels)
            {
                var label = Text("Choice", 30); label.Text = value;
                label.CustomMinimumSize = new(0, 80);
                var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
                row.AddThemeConstantOverride("separation", 8);
                var marker = Text("Focus", 30); marker.CustomMinimumSize = new(20, 80);
                marker.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
                var iconSlot = new Control { CustomMinimumSize = new(72, 80), MouseFilter = MouseFilterEnum.Ignore };
                var icon = new UI.IdentityIcon(3) { Position = new(0, 4) }; iconSlot.AddChild(icon);
                row.AddChild(marker); row.AddChild(iconSlot); row.AddChild(label); _rowMarkers.Add(marker);
                _rows.AddChild(row); _rowLabels.Add(label); _rowControls.Add(row); _rowIcons.Add(icon);
                label.ItemRectChanged += () => InvalidatePointer();
            }
        }
        for (int i = 0; i < _rowLabels.Count; i++)
        {
            _rowIcons[i].Present(Icons, identities[i], i == selected);
            _rowLabels[i].Text = labels[i];
            _rowMarkers[i].Text = i == selected ? "›" : "";
            _rowMarkers[i].AddThemeColorOverride("font_color", enabled[i] ? UI.StoryTheme.Selected : UI.StoryTheme.Unavailable);
            // Focus stays inspectable even when unavailable; the marker and reason are independent of color.
            if (enabled[i] && i != selected) _rowLabels[i].RemoveThemeColorOverride("font_color");
            else _rowLabels[i].AddThemeColorOverride("font_color", !enabled[i] ? UI.StoryTheme.Unavailable : UI.StoryTheme.Selected);
        }
        _selected = selected;
        if (changed && visible && _rowLabels.Count > selected && selected >= 0) Callable.From(ScrollToSelection).CallDeferred();
    }

    private void ScrollToSelection()
    {
        if (_selected >= 0 && _selected < _rowLabels.Count) _scroll.EnsureControlVisible(_rowControls[_selected]);
    }

    internal object Observe() => new
    {
        commandList = new { visible = _commandList.IsVisibleInTree(), rect = BattleMapViewport.Rectangle(_commandList.GetGlobalRect()) },
        description = BattleMapViewport.Rectangle(_detail.GetGlobalRect()),
        descriptionVisible = _detail.IsVisibleInTree(),
        reader = _reading.Observe(),
        actor = BattleMapViewport.Rectangle(Actor.GetGlobalRect()), context = BattleMapViewport.Rectangle(_context.GetGlobalRect()),
        contextText = Context.Text, contextVisible = _context.IsVisibleInTree(), terrainPanel = Terrain.Observe(), actorCard = Actor.Observe(), targetCard = Target.Observe(),
        heading = Heading.Text, detail = _reading.Body.Text, help = Help.Text, helpVisible = Help.IsVisibleInTree(),
        descriptionLabels = new[] { Heading, _reading.Body, Help }.Where(label => label.IsVisibleInTree()).Select(label => new { text = label.Text,
            rect = BattleMapViewport.Rectangle(label.GetGlobalRect()) }).ToArray(),
        listVisible = _list.IsVisibleInTree(), list = BattleMapViewport.Rectangle(_list.GetGlobalRect()),
        rows = _rowLabels.Select(label => label.Text).ToArray(), selectedRow = _selected,
        rowRects = _rowControls.Select(row => BattleMapViewport.Rectangle(row.GetGlobalRect().Intersection(_scroll.GetGlobalRect()))).ToArray(),
        iconMode = Icons.Mounted ? "bound" : "unmounted-text",
        icons = _rowIcons.Select(icon => icon.Observe()).ToArray(),
        scroll = _scroll.ScrollVertical,
        navigation = new[] { _previous, _next, _confirm }.Select(control => new { id = control.Name.ToString(),
            visible = control.IsVisibleInTree(), enabled = control != _confirm || _canConfirm, rect = BattleMapViewport.Rectangle(control.GetGlobalRect()) }).ToArray(),
        commands = Commands.Select((button, index) => new { id = button.Identity, label = button.Caption,
            visible = button.IsVisibleInTree(), selected = button.Selected, enabled = button.Available,
            rect = BattleMapViewport.Rectangle(_commandList.RowRect(index)), caption = BattleMapViewport.Rectangle(_commandList.CaptionRect(index)),
            art = button.ObserveArt() }).ToArray(),
    };

    // Field/battle handoff reparents this same view. Retain its borrowed textures until
    // content replacement (ConfigureAssets) or actual managed disposal, not temporary tree exit.
    protected override void Dispose(bool disposing) { if (disposing) Icons.Dispose(); base.Dispose(disposing); }

    private static Label Text(string name, int size)
    {
        var label = new Label { Name = name, AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    private static PanelContainer Frame(string name)
    {
        var panel = new PanelContainer { Name = name, MouseFilter = MouseFilterEnum.Ignore };
        return panel;
    }
}

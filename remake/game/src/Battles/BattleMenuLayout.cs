using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Battles;

// Product UI geometry uses physical window units, independent of map tiles and source pixels.
internal sealed partial class BattleMenuLayout : Control
{
    private const int PanelInset = 16, DescriptionGap = 9;
    internal readonly UI.BattleStatusCard Actor = new() { Name = "ActorCard", Compact = true, DecisionHeader = true };
    internal readonly UI.BattleStatusCard Target = new() { Name = "TargetCard", Visible = false, Compact = true, DecisionHeader = true };
    internal readonly Label Context = Text("Context", 28);
    internal readonly Label Heading = Text("Heading", 32);
    internal readonly Label Detail = Text("Detail", 30);
    internal readonly Label Help = Text("Help", UI.StoryTheme.HintSize);
    private readonly HBoxContainer _navigation = new() { Name = "Navigation", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _previous = Text("PreviousTarget", 26), _next = Text("NextTarget", 26);
    private readonly Label _confirm = Text("Confirm", 26), _back = Text("Back", 26);
    private readonly Panel _commandsPanel = new() { Name = "CommandsPanel", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Panel _group = new() { Name = "DecisionGroup", MouseFilter = MouseFilterEnum.Ignore };
    private UI.CompactCommandList _commandList = null!;
    internal readonly UI.DiamondCommandButton[] Commands = [new("Attack", "battle.command.attack", "command-menu"), new("Magic", "battle.command.magic", "command-menu"),
        new("Item", "battle.command.item", "command-menu"), new("Stay", "battle.command.stay", "command-menu")];
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
    private Vector2 _descriptionSize;
    private Vector2 _contextSize;
    private bool _commandHint;
    private Vector2 _commandFooter; // Natural reading width and height at the command-column width.
    private Rect2 _subject;
    internal Action InvalidatePointer { get; set; } = () => { };
    internal bool Covers(Vector2 p) => new Control[] { _group, Actor, Target, _context, _commandsPanel, _list, _detail }
        .Any(c => c.IsVisibleInTree() && c.GetGlobalRect().HasPoint(p));

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
        var insets = (StyleBoxFlat)Theme.GetStylebox("panel", "PanelContainer").Duplicate();
        insets.ContentMarginLeft = insets.ContentMarginRight = insets.ContentMarginTop = insets.ContentMarginBottom = PanelInset;
        foreach (var panel in new[] { _context, _detail, _list }) panel.AddThemeStyleboxOverride("panel", insets);
        var contentStyle = (StyleBoxFlat)insets.Duplicate();
        contentStyle.BgColor = Colors.Transparent; contentStyle.ShadowSize = 0;
        contentStyle.BorderWidthLeft = contentStyle.BorderWidthRight = contentStyle.BorderWidthTop = contentStyle.BorderWidthBottom = 0;
        _list.AddThemeStyleboxOverride("panel", contentStyle);
        _commandsPanel.AddThemeStyleboxOverride("panel", new StyleBoxEmpty());
        MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_group);
        AddChild(Actor); AddChild(Target);
        AddChild(_context); _context.AddChild(Context);
        _commandList = new(Commands) { Name = "Commands", InvalidatePointer = () => InvalidatePointer() };
        AddChild(_commandsPanel); _commandsPanel.AddChild(_commandList);
        _commandsPanel.ItemRectChanged += () => InvalidatePointer();
        AddChild(_list); _list.AddChild(_scroll); _scroll.AddChild(_rows);
        _scroll.GetVScrollBar().FocusMode = FocusModeEnum.None;
        _rows.AddThemeConstantOverride("separation", 8);
        AddChild(_detail);
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; column.AddThemeConstantOverride("separation", DescriptionGap);
        _detail.AddChild(column); column.AddChild(Heading); column.AddChild(Detail); column.AddChild(Help);
        column.AddChild(_navigation);
        foreach (var control in new[] { _previous, _next, _confirm, _back })
        {
            control.CustomMinimumSize = new(0, 44);
            control.HorizontalAlignment = HorizontalAlignment.Center;
            _navigation.AddChild(control);
            control.ItemRectChanged += () => InvalidatePointer();
        }

        // Wrapped text can shrink after a container sort. Reapply the requested size then;
        // otherwise Control retains the earlier (possibly very tall) minimum-size clamp.
        _detail.MinimumSizeChanged += () => Arrange(Size);
        _context.MinimumSizeChanged += () => _context.Size = _contextSize;
        _list.Hide();
    }

    // Native wrapped-label minimum sizes settle after container sorting. Settle geometry before
    // the next pointer event too, rather than moving a hit region only when that event redraws it.
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
        Arrange(Size);
        foreach (var command in Commands) command.AdvanceFocus(delta);
    }

    internal void StopFocusEmphasis()
    {
        foreach (var command in Commands) command.StopFocusEmphasis();
    }

    internal void Arrange(Vector2 size)
    {
        Size = size;
        const float margin = UI.StoryTheme.GameplayInset, inset = PanelInset;
        var header = Target.Visible ? Target : Actor;
        float width = Mathf.Min(Mathf.Max(560, header.RequestedSize.X), size.X - margin * 2);
        float right = size.X - margin - width;
        // Layout A: live acting vitals head the same right-hand command column.
        // Child offers replace the root rows at the same origin; the board remains independent.
        Actor.Visible = !Target.Visible;
        Actor.Position = new(right, UI.StoryTheme.BelowMenu); Actor.Size = new(Mathf.Max(width, Actor.RequestedSize.X), Actor.RequestedSize.Y);
        if (Actor.Size.X > width) Actor.Position = new(size.X - margin - Actor.Size.X, UI.StoryTheme.BelowMenu);
        Target.Position = new(size.X - margin - Mathf.Max(width, Target.RequestedSize.X), UI.StoryTheme.BelowMenu);
        Target.Size = new(Mathf.Max(width, Target.RequestedSize.X), Target.RequestedSize.Y);
        _contextSize = new(300, 110); _context.Position = new(margin, 96); _context.Size = _contextSize;
        float rowsTop = Actor.Position.Y + Actor.Size.Y + 8;
        _commandList.Position = new(inset, inset); _commandList.Arrange(width - inset * 2);
        _commandsPanel.Position = new(right, rowsTop);
        _commandsPanel.Size = new(width, _commandList.RequestedHeight + inset * 2);
        _list.Position = new(right, rowsTop);
        // Root commands keep fixed hit regions. Let a long refusal use the remaining
        // reading width below them so its mapped actions still fit at native 1080p.
        float footerWidth = width;
        if (_commandsPanel.Visible && rowsTop + _commandsPanel.Size.Y + 12 + _commandFooter.Y > size.Y - margin)
            footerWidth = Mathf.Max(width, Mathf.Min(_commandFooter.X, Mathf.Min(1440, size.X - margin * 2)));
        _descriptionSize = new(footerWidth, 0);
        SetTextWidths();
        float descriptionHeight = _detail.GetCombinedMinimumSize().Y;
        // Short lists need only their actual rows. Longer lists keep the existing scroll
        // consumer and reserve measured space for the full current reason and mapped footer.
        float availableRows = size.Y - margin - rowsTop - 12 - descriptionHeight;
        _list.Size = new(width, Mathf.Max(112, Mathf.Min(availableRows,
            Mathf.Min(_commandsPanel.Size.Y, _rows.GetCombinedMinimumSize().Y + inset * 2))));
        _group.Position = new(right, header.Position.Y);
        float groupBottom = _commandsPanel.Visible ? _commandsPanel.Position.Y + _commandsPanel.Size.Y :
            _list.Visible ? _list.Position.Y + _list.Size.Y : header.Position.Y + header.Size.Y;
        _group.Size = new(width, groupBottom - _group.Position.Y);
        float detailTop = _commandsPanel.Visible ? _commandsPanel.Position.Y + _commandsPanel.Size.Y + 12 :
            _list.Visible ? _list.Position.Y + _list.Size.Y + 12 :
            Target.Visible ? Target.Position.Y + Target.Size.Y + 12 : rowsTop;
        _descriptionSize = new(footerWidth, descriptionHeight);
        _detail.Position = new(size.X - margin - footerWidth, detailTop); _detail.Size = _descriptionSize;
        SetTextWidths();
    }

    internal void ShowCommands(bool visible)
    {
        _commandsPanel.Visible = visible;
        _commandList.Present(Commands.Select(command => command.Caption +
            (command.Available ? "" : GameText.Get("battle.unavailable-tag"))).ToArray());
        Arrange(Size);
    }

    internal void ShowCommandHint(bool compact)
    {
        _commandHint = compact;
        Detail.Visible = !compact || !string.IsNullOrWhiteSpace(Detail.Text);
        Help.Visible = true;
        _commandFooter = compact && Detail.Visible ? MeasureCommandFooter() : Vector2.Zero;
        Arrange(Size);
    }

    private Vector2 MeasureCommandFooter()
    {
        // Measure the normal column without resizing any live hit region during a press.
        float inner = _commandsPanel.Size.X - 2 * PanelInset;
        float height = 2 * PanelInset + _navigation.GetMinimumSize().Y + 3 * DescriptionGap;
        float readingWidth = inner;
        foreach (var label in new[] { Heading, Detail, Help })
        {
            using var paragraph = new TextParagraph { Width = inner, BreakFlags = TextServer.LineBreakFlag.Mandatory |
                TextServer.LineBreakFlag.WordBound | TextServer.LineBreakFlag.Adaptive };
            var font = label.GetThemeFont("font");
            int fontSize = label.GetThemeFontSize("font_size");
            paragraph.AddString(label.Text, font, fontSize);
            height += paragraph.GetSize().Y + (paragraph.GetLineCount() - 1) * label.GetThemeConstant("line_spacing");
            readingWidth = Mathf.Max(readingWidth, label.Text.Split('\n').Max(line => font.GetStringSize(line, fontSize: fontSize).X));
        }
        return new(2 * PanelInset + Mathf.Ceil(readingWidth), height);
    }

    internal void ShowNavigation(bool visible, bool target, bool commit, bool canConfirm)
    {
        _previous.Text = GameText.Get("battle.previous"); _next.Text = GameText.Get("battle.next");
        _confirm.Text = GameText.Get("battle.confirm"); _back.Text = GameText.Get("battle.back");
        _navigation.Visible = visible;
        _previous.Visible = _next.Visible = target;
        _confirm.Visible = target || commit;
        _confirm.AddThemeColorOverride("font_color", canConfirm ? UI.StoryTheme.Selected : UI.StoryTheme.Unavailable);
    }

    internal (string? Id, Rect2 Rect) Hit(Vector2 position)
    {
        foreach (var control in new[] { _previous, _next, _confirm, _back })
            if (control.IsVisibleInTree() && control.GetGlobalRect().HasPoint(position)) return (control.Name.ToString(), control.GetGlobalRect());
        var command = _commandList.Hit(position);
        if (command.Index >= 0) return ($"command:{command.Index}", command.Rect);
        if (_list.IsVisibleInTree() && _scroll.GetGlobalRect().HasPoint(position))
            for (int i = 0; i < _rowLabels.Count; i++)
            {
                var rect = _rowControls[i].GetGlobalRect().Intersection(_scroll.GetGlobalRect());
                if (rect.HasPoint(position)) return ($"row:{i}", rect);
            }
        return (null, default);
    }

    internal bool Scroll(Vector2 position, int direction)
    {
        if (!_list.IsVisibleInTree() || !_list.GetGlobalRect().HasPoint(position)) return false;
        _scroll.ScrollVertical += direction * 80;
        return true;
    }

    private void SetTextWidths()
    {
        Context.CustomMaximumSize = new(Mathf.Max(1, _contextSize.X - 28), -1);
        foreach (var label in new[] { Heading, Detail, Help })
        {
            float width = Mathf.Max(1, _descriptionSize.X - 32);
            label.CustomMinimumSize = new(width, 0);
            label.CustomMaximumSize = new(width, -1);
        }
        foreach (var label in _rowLabels) label.CustomMaximumSize = new(Mathf.Max(1, _list.Size.X - 48 - 108), -1);
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
        SetTextWidths();
        Arrange(Size);
        _selected = selected;
        if (changed && visible && _rowLabels.Count > selected && selected >= 0) Callable.From(ScrollToSelection).CallDeferred();
    }

    private void ScrollToSelection()
    {
        if (_selected >= 0 && _selected < _rowLabels.Count) _scroll.EnsureControlVisible(_rowControls[_selected]);
    }

    internal object Observe() => new
    {
        decisionGroup = BattleMapViewport.Rectangle(_group.GetGlobalRect()),
        commandList = _commandList.Observe(), commandPanel = BattleMapViewport.Rectangle(_commandsPanel.GetGlobalRect()),
        description = BattleMapViewport.Rectangle(_detail.GetGlobalRect()),
        actor = BattleMapViewport.Rectangle(Actor.GetGlobalRect()), context = BattleMapViewport.Rectangle(_context.GetGlobalRect()),
        actorCard = Actor.Observe(), targetCard = Target.Observe(),
        heading = Heading.Text, detail = Detail.Text, help = Help.Text,
        commandHint = _commandHint, helpVisible = Help.Visible,
        descriptionLabels = new[] { Heading, Detail, Help }.Where(label => label.Visible).Select(label => new { text = label.Text,
            rect = BattleMapViewport.Rectangle(label.GetGlobalRect()) }).ToArray(),
        listVisible = _list.Visible, list = BattleMapViewport.Rectangle(_list.GetGlobalRect()),
        rows = _rowLabels.Select(label => label.Text).ToArray(), selectedRow = _selected,
        rowRects = _rowLabels.Select(label => BattleMapViewport.Rectangle(label.GetGlobalRect())).ToArray(),
        iconMode = Icons.Mounted ? "bound" : "unmounted-text",
        icons = _rowIcons.Select(icon => icon.Observe()).ToArray(),
        scroll = _scroll.ScrollVertical,
        navigation = new[] { _previous, _next, _confirm, _back }.Select(control => new { id = control.Name.ToString(),
            visible = control.IsVisibleInTree(), rect = BattleMapViewport.Rectangle(control.GetGlobalRect()) }).ToArray(),
        commands = Commands.Select(button => new { id = button.Identity, label = button.Caption, selected = button.Selected, enabled = button.Available,
            rect = BattleMapViewport.Rectangle(button.GetGlobalRect()), art = button.ObserveArt() }).ToArray(),
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

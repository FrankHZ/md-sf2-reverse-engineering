using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Battles;

// Product UI geometry uses physical window units, independent of map tiles and source pixels.
internal sealed partial class BattleMenuLayout : Control
{
    internal readonly UI.BattleStatusCard Actor = new() { Name = "ActorCard" };
    internal readonly UI.BattleStatusCard Target = new() { Name = "TargetCard", Visible = false };
    internal readonly Label Context = Text("Context", 28);
    internal readonly Label Heading = Text("Heading", 32);
    internal readonly Label Detail = Text("Detail", 30);
    internal readonly Label Help = Text("Help", 28);
    private readonly HBoxContainer _navigation = new() { Name = "Navigation", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _previous = Text("PreviousTarget", 26), _next = Text("NextTarget", 26);
    private readonly Label _confirm = Text("Confirm", 26), _back = Text("Back", 26);
    internal readonly Control Diamond = new() { Name = "Diamond", MouseFilter = MouseFilterEnum.Ignore };
    internal readonly UI.DiamondCommandButton[] Commands = [new("Attack", "battle.command.attack", "command-menu"), new("Magic", "battle.command.magic", "command-menu"),
        new("Item", "battle.command.item", "command-menu"), new("Stay", "battle.command.stay", "command-menu")];
    private readonly PanelContainer _context = Frame("ContextPanel");
    private readonly PanelContainer _detail = Frame("DescriptionPanel");
    private readonly PanelContainer _list = Frame("ChoicePanel");
    private readonly ScrollContainer _scroll = new() { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, FocusMode = FocusModeEnum.None };
    private readonly VBoxContainer _rows = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
    private readonly List<Label> _rowLabels = [];
    private readonly List<HBoxContainer> _rowControls = [];
    private readonly List<UI.IdentityIcon> _rowIcons = [];
    internal readonly UI.IdentityIcons Icons = new();
    private string[] _labels = [];
    private int _selected;
    private Vector2 _descriptionSize;
    private Vector2 _contextSize;
    private bool _commandHint;
    private Rect2 _subject;
    internal Action InvalidatePointer { get; set; } = () => { };

    internal void AvoidSubject(Rect2 rectangle)
    {
        if (_subject == rectangle) return;
        _subject = rectangle;
        Arrange(Size);
    }

    internal void ConfigureAssets(BattleSceneDefinition? content)
    {
        Icons.Configure(content?.Rasters);
        if (content is null || !content.Rasters.Keys.Any(key => key.StartsWith("command-menu.", StringComparison.Ordinal)))
        {
            foreach (var command in Commands) command.SetArt(null, null);
            return;
        }
        var textures = new List<ImageTexture>();
        try
        {
            foreach (var command in Commands)
                foreach (string state in new[] { "normal", "selected" })
                {
                    string key = $"command-menu.{command.Identity.ToLowerInvariant()}.{state}";
                    if (!content.Rasters.TryGetValue(key, out var raster) || raster.Width != 24 || raster.Height != 24)
                        throw new InvalidOperationException("command-menu-resource-required");
                    using var image = raster.Format == "rgba8"
                        ? Image.CreateFromData(24, 24, false, Image.Format.Rgba8, raster.CopyBytes()) : new Image();
                    if (raster.Format != "rgba8" && (image.LoadPngFromBuffer(raster.CopyBytes()) != Godot.Error.Ok ||
                        image.GetWidth() != 24 || image.GetHeight() != 24))
                        throw new InvalidOperationException("command-menu-resource-invalid");
                    textures.Add(ImageTexture.CreateFromImage(image));
                }
        }
        catch { foreach (var texture in textures) texture.Dispose(); throw; }
        for (int i = 0; i < Commands.Length; i++) Commands[i].SetArt(textures[i * 2], textures[i * 2 + 1]);
    }

    public override void _Ready()
    {
        Theme = UI.StoryTheme.Create();
        MouseFilter = MouseFilterEnum.Ignore;
        AddChild(Actor); AddChild(Target);
        AddChild(_context); _context.AddChild(Context);
        AddChild(Diamond);
        foreach (var command in Commands) Diamond.AddChild(command);
        foreach (var command in Commands) command.ItemRectChanged += () => InvalidatePointer();
        Diamond.ItemRectChanged += () => InvalidatePointer();
        AddChild(_list); _list.AddChild(_scroll); _scroll.AddChild(_rows);
        _scroll.GetVScrollBar().FocusMode = FocusModeEnum.None;
        AddChild(_detail);
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; column.AddThemeConstantOverride("separation", 9);
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
        const float margin = 32;
        bool narrow = size.X < 700;
        // Four display pixels per source pixel: keep the original icon crisp,
        // and bring the cardinal centers closer without overlapping the square rasters.
        float menuWidth = UI.DiamondMenuGeometry.Size.X, menuHeight = UI.DiamondMenuGeometry.Size.Y;
        float listHeight = Mathf.Clamp(size.Y * 0.38f, 300, 420);
        float detailWidth = _commandHint ? Mathf.Min(420, size.X - 2 * margin) :
            narrow ? size.X - 2 * margin : Mathf.Clamp(size.X * 0.25f, 420, 520);
        float detailHeight = _commandHint ? 88 : narrow ? 180 : listHeight;
        _descriptionSize = new(detailWidth, detailHeight);
        SetTextWidths();
        detailHeight = Mathf.Max(detailHeight, _detail.GetCombinedMinimumSize().Y);
        float bottom = size.Y - margin - (narrow ? detailHeight + 12 : 0);
        _contextSize = new(size.X * 0.45f - margin - 8, 110);
        Actor.Position = new(margin, margin); Actor.Size = Actor.RequestedSize;
        Target.Position = new(size.X - margin - Target.RequestedSize.X, margin);
        Target.Size = Target.RequestedSize;
        _context.Position = new(size.X * 0.55f + 8, Target.Visible ? margin + Target.Size.Y + 16 : margin);
        Diamond.Position = new(margin, bottom - menuHeight); Diamond.Size = new(menuWidth, menuHeight);
        UI.DiamondMenuGeometry.Arrange(Diamond, Commands);
        _list.Position = new(margin, bottom - listHeight);
        _list.Size = new(Mathf.Min(540, size.X - 2 * margin - (narrow ? 0 : detailWidth + 24)), listHeight);
        _detail.Position = new(narrow ? margin : size.X - margin - detailWidth, size.Y - margin - detailHeight);
        _descriptionSize = new(detailWidth, detailHeight);
        SetTextWidths();
        _context.Size = _contextSize;
        _detail.Size = _descriptionSize;
        SetTextWidths();
        // Keep visible corner panels mutually clear as well as clear of the subject.
        // The few vertical alternatives cover the selected 1080p layout; the world never moves for UI.
        Control[] panels = [_list, _detail, Diamond, Actor, Target, _context];
        foreach (var panel in panels.Where(panel => panel.Visible))
        {
            if (!new Rect2(panel.Position, panel.Size).Intersects(_subject.Grow(16))) continue;
            foreach (float y in new[] { 192f, _subject.End.Y + margin,
                _subject.Position.Y - margin - panel.Size.Y, size.Y - margin - panel.Size.Y, margin })
            {
                var candidate = new Rect2(new Vector2(panel.Position.X, y), panel.Size);
                if (y < margin || candidate.End.Y > size.Y - margin || candidate.Intersects(_subject.Grow(16)) ||
                    panels.Any(other => other != panel && other.Visible &&
                        candidate.Intersects(new Rect2(other.Position, other.Size).Grow(8)))) continue;
                panel.Position = candidate.Position;
                break;
            }
        }
    }

    internal void ShowCommandHint(bool compact)
    {
        _commandHint = compact;
        Detail.Visible = !compact || !string.IsNullOrWhiteSpace(Detail.Text);
        Help.Visible = !compact;
        Arrange(Size);
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
        if (Diamond.IsVisibleInTree())
            for (int i = 0; i < Commands.Length; i++)
                if (Commands[i].GetGlobalRect().HasPoint(position)) return ($"command:{i}", Commands[i].GetGlobalRect());
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
            float width = Mathf.Max(1, _descriptionSize.X - 28);
            label.CustomMinimumSize = new(width, 0);
            label.CustomMaximumSize = new(width, -1);
        }
        foreach (var label in _rowLabels) label.CustomMaximumSize = new(Mathf.Max(1, _list.Size.X - 48 - (_rowIcons.Any(icon => icon.Visible) ? 80 : 0)), -1);
    }

    internal void ShowRows(string[] labels, bool[] enabled, int selected, bool visible, string?[] identities)
    {
        bool changed = !labels.SequenceEqual(_labels) || selected != _selected || visible != _list.Visible;
        _list.Visible = visible;
        Diamond.Visible = !visible;
        if (!labels.SequenceEqual(_labels))
        {
            InvalidatePointer();
            foreach (var child in _rows.GetChildren()) { _rows.RemoveChild(child); child.QueueFree(); }
            _rowLabels.Clear(); _rowControls.Clear(); _rowIcons.Clear(); _labels = labels;
            foreach (string value in labels)
            {
                var label = Text("Choice", 30); label.Text = value;
                label.CustomMinimumSize = new(0, 64);
                var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
                var icon = new UI.IdentityIcon(); row.AddChild(icon); row.AddChild(label);
                _rows.AddChild(row); _rowLabels.Add(label); _rowControls.Add(row); _rowIcons.Add(icon);
                label.ItemRectChanged += () => InvalidatePointer();
            }
        }
        for (int i = 0; i < _rowLabels.Count; i++)
        {
            _rowIcons[i].Present(Icons, identities[i], i == selected);
            _rowLabels[i].Text = (i == selected ? "›  " : "   ") + labels[i];
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
        diamondVisible = Diamond.Visible, diamond = BattleMapViewport.Rectangle(Diamond.GetGlobalRect()),
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

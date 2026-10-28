using Godot;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.Battles;

// Product UI geometry uses physical window units, independent of map tiles and source pixels.
internal sealed partial class BattleMenuLayout : Control
{
    internal readonly Label Actor = Text("Actor", 17);
    internal readonly Label Context = Text("Context", 17);
    internal readonly Label Heading = Text("Heading", 21);
    internal readonly Label Detail = Text("Detail", 17);
    internal readonly Label Help = Text("Help", 15);
    internal readonly Control Diamond = new() { Name = "Diamond", MouseFilter = MouseFilterEnum.Ignore };
    internal readonly BattleDiamondButton[] Commands = [new("Attack"), new("Magic"), new("Item"), new("Stay")];
    private readonly PanelContainer _actor = Frame("ActorPanel");
    private readonly PanelContainer _context = Frame("ContextPanel");
    private readonly PanelContainer _detail = Frame("DescriptionPanel");
    private readonly PanelContainer _list = Frame("ChoicePanel");
    private readonly ScrollContainer _scroll = new() { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, FocusMode = FocusModeEnum.None };
    private readonly VBoxContainer _rows = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
    private readonly List<Label> _rowLabels = [];
    private string[] _labels = [];
    private int _selected;
    private Vector2 _descriptionSize;
    private Vector2 _actorSize, _contextSize;
    private bool _commandsVisible;
    internal Action? LayoutChanged { get; set; }
    internal Rect2 MapRect { get; private set; }

    internal void ConfigureAssets(BattleSceneDefinition? content)
    {
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
                    string key = $"command-menu.{command.Caption.ToLowerInvariant()}.{state}";
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
        MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_actor); _actor.AddChild(Actor);
        AddChild(_context); _context.AddChild(Context);
        AddChild(Diamond);
        foreach (var command in Commands) Diamond.AddChild(command);
        AddChild(_list); _list.AddChild(_scroll); _scroll.AddChild(_rows);
        _scroll.GetVScrollBar().FocusMode = FocusModeEnum.None;
        AddChild(_detail);
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; column.AddThemeConstantOverride("separation", 9);
        _detail.AddChild(column); column.AddChild(Heading); column.AddChild(Detail); column.AddChild(Help);
        // Wrapped text can shrink after a container sort. Reapply the requested size then;
        // otherwise Control retains the earlier (possibly very tall) minimum-size clamp.
        _detail.MinimumSizeChanged += () => Arrange(Size);
        _actor.MinimumSizeChanged += () => _actor.Size = _actorSize;
        _context.MinimumSizeChanged += () => _context.Size = _contextSize;
        _list.Hide();
    }

    internal void Arrange(Vector2 size)
    {
        Size = size;
        const float margin = 16;
        bool narrow = size.X < 700;
        float menu = Mathf.Clamp(size.Y * 0.38f, narrow ? 200 : 220, 300);
        menu = Mathf.Min(menu, size.X - 2 * margin);
        float detailWidth = narrow ? size.X - 2 * margin : Mathf.Clamp(size.X * 0.25f, 260, 340);
        float detailHeight = narrow ? 180 : menu;
        _descriptionSize = new(detailWidth, detailHeight);
        SetTextWidths();
        detailHeight = Mathf.Max(detailHeight, _detail.GetCombinedMinimumSize().Y);
        float menuY = size.Y - margin - menu - (narrow ? detailHeight + 12 : 0);
        _actorSize = new(Mathf.Min(300, size.X * 0.55f - margin), 76);
        _contextSize = new(size.X * 0.45f - margin - 8, 62);
        _actor.Position = new(margin, margin);
        _context.Position = new(size.X * 0.55f + 8, margin);
        Diamond.Position = new(margin, menuY); Diamond.Size = new(menu, menu);
        float cell = Mathf.Floor(menu / 3), offset = (menu - cell) / 2;
        Vector2[] locations = [new(offset, 0), new(0, offset), new(menu - cell, offset), new(offset, menu - cell)];
        for (int i = 0; i < Commands.Length; i++)
        {
            Commands[i].Position = locations[i]; Commands[i].Size = new(cell, cell);
            Commands[i].Arrange();
        }
        _list.Position = Diamond.Position; _list.Size = new(Mathf.Min(360, size.X - 2 * margin - (narrow ? 0 : detailWidth + 24)), menu);
        _detail.Position = new(narrow ? margin : size.X - margin - detailWidth, size.Y - margin - detailHeight);
        _descriptionSize = new(detailWidth, detailHeight);
        SetTextWidths();
        _actor.Size = _actorSize; _context.Size = _contextSize;
        _detail.Size = _descriptionSize;
        // Keep the map's focus region above the menu band so projected actors cannot hide behind it.
        float mapTop = narrow ? 124 : 104;
        float mapBottom = _commandsVisible ? menuY - 12 : narrow ? _detail.Position.Y - 12 : size.Y - margin;
        float mapWidth = size.X - 2 * margin - (!_commandsVisible && !narrow ? detailWidth + margin : 0);
        MapRect = new(new(margin, mapTop), new(Mathf.Max(1, mapWidth), Mathf.Max(1, mapBottom - mapTop)));
        SetTextWidths();
        LayoutChanged?.Invoke();
    }

    internal void ShowCommands(bool visible)
    {
        if (_commandsVisible == visible) return;
        _commandsVisible = visible; Arrange(Size);
    }

    private void SetTextWidths()
    {
        Actor.CustomMaximumSize = new(Mathf.Max(1, _actorSize.X - 28), -1);
        Context.CustomMaximumSize = new(Mathf.Max(1, _contextSize.X - 28), -1);
        foreach (var label in new[] { Heading, Detail, Help })
        {
            float width = Mathf.Max(1, _descriptionSize.X - 28);
            label.CustomMinimumSize = new(width, 0);
            label.CustomMaximumSize = new(width, -1);
        }
        foreach (var label in _rowLabels) label.CustomMaximumSize = new(Mathf.Max(1, _list.Size.X - 48), -1);
    }

    internal void ShowRows(string[] labels, bool[] enabled, int selected, bool visible)
    {
        _list.Visible = visible;
        Diamond.Visible = !visible;
        if (!labels.SequenceEqual(_labels))
        {
            foreach (var child in _rows.GetChildren()) { _rows.RemoveChild(child); child.QueueFree(); }
            _rowLabels.Clear(); _labels = labels;
            foreach (string value in labels)
            {
                var label = Text("Choice", 18); label.Text = value;
                label.CustomMinimumSize = new(0, 44);
                _rows.AddChild(label); _rowLabels.Add(label);
            }
        }
        for (int i = 0; i < _rowLabels.Count; i++)
        {
            _rowLabels[i].Text = (i == selected ? "›  " : "   ") + labels[i];
            _rowLabels[i].Modulate = i == selected ? new Color("ffdf80") : enabled[i] ? Colors.White : new Color("94a4be");
        }
        SetTextWidths();
        _selected = selected;
        if (visible && _rowLabels.Count > selected && selected >= 0) Callable.From(ScrollToSelection).CallDeferred();
    }

    private void ScrollToSelection()
    {
        if (_selected >= 0 && _selected < _rowLabels.Count) _scroll.EnsureControlVisible(_rowLabels[_selected]);
    }

    internal object Observe() => new
    {
        diamondVisible = Diamond.Visible, diamond = BattleMapViewport.Rectangle(Diamond.GetGlobalRect()),
        description = BattleMapViewport.Rectangle(_detail.GetGlobalRect()),
        actor = BattleMapViewport.Rectangle(_actor.GetGlobalRect()), context = BattleMapViewport.Rectangle(_context.GetGlobalRect()),
        heading = Heading.Text, detail = Detail.Text, help = Help.Text,
        descriptionLabels = new[] { Heading, Detail, Help }.Select(label => new { text = label.Text,
            rect = BattleMapViewport.Rectangle(label.GetGlobalRect()) }).ToArray(),
        listVisible = _list.Visible, list = BattleMapViewport.Rectangle(_list.GetGlobalRect()),
        rows = _rowLabels.Select(label => label.Text).ToArray(), selectedRow = _selected,
        commands = Commands.Select(button => new { label = button.Caption, selected = button.Selected, enabled = button.Available,
            rect = BattleMapViewport.Rectangle(button.GetGlobalRect()), art = button.ObserveArt() }).ToArray(),
    };

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
        var style = new StyleBoxFlat { BgColor = new("08275bef"), BorderColor = new("ffe19a"),
            BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
            ShadowColor = new("754314"), ShadowSize = 3,
            ContentMarginLeft = 14, ContentMarginTop = 12, ContentMarginRight = 14, ContentMarginBottom = 12 };
        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }
}

// Source pixels are supplied through the existing admitted scene raster collection.
// An unmounted public scene shows text-only squares; it never substitutes authored glyphs.
internal sealed partial class BattleDiamondButton : Control
{
    internal string Caption { get; }
    internal bool Selected { get; set; }
    internal bool Available { get; set; }
    private readonly Label _label;
    private ImageTexture? _normal, _selected;
    private int _pixelScale = 1;
    private string? _drawnResource;
    internal BattleDiamondButton(string caption)
    {
        Caption = caption; Name = caption;
        MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
        _label = new Label { Text = caption, HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        _label.AddThemeFontSizeOverride("font_size", 14);
    }
    public override void _Ready() => AddChild(_label);
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _normal?.Dispose(); _selected?.Dispose(); }
        base.Dispose(disposing);
    }
    internal void SetArt(ImageTexture? normal, ImageTexture? selected)
    {
        _normal?.Dispose(); _selected?.Dispose();
        _normal = normal; _selected = selected; _drawnResource = null; QueueRedraw();
    }
    internal void Arrange()
    {
        _pixelScale = Math.Max(1, (int)Mathf.Floor(Mathf.Min(Size.X - 8, Size.Y - 24) / 24));
        _label.Position = new(0, Size.Y - 23); _label.Size = new(Size.X, 20); QueueRedraw();
    }
    internal object ObserveArt() => new { shape = "square", mounted = _normal is not null,
        resource = _drawnResource,
        width = _normal?.GetWidth(), height = _normal?.GetHeight(), scale = _pixelScale,
        filter = TextureFilter.ToString() };
    public override void _Draw()
    {
        var rect = new Rect2(Vector2.One, Size - Vector2.One * 2);
        DrawRect(rect, new Color("002492"));
        DrawRect(rect, new Color("924900"), false, 2);
        DrawRect(rect.Grow(-2), Selected ? new Color("ffffff") : new Color("ffdb49"), false, 2);
        var texture = Selected ? _selected : _normal;
        if (texture is not null)
        {
            float side = 24 * _pixelScale;
            DrawTextureRect(texture, new Rect2(new((Size.X - side) / 2, 4), new(side, side)), false);
            _drawnResource = $"command-menu.{Caption.ToLowerInvariant()}.{(Selected ? "selected" : "normal")}";
        }
        _label.Modulate = Selected ? Colors.White : Available ? new Color("ffdb49") : new Color("a0a0a0");
    }
}

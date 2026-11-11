using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Field/service presentation only. The parent retains input, focus and transaction authority.
internal sealed partial class FieldServiceMenu : Panel
{
    private readonly DiamondCommandButton[] _icons;
    private readonly string[] _keys;
    private readonly string _owner;
    private readonly Label _title = Label("Title", 32), _back = Label("Back", 30), _reason = Label("Reason", 30);
    private readonly ScrollContainer _reading = new() { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, FocusMode = FocusModeEnum.None };
    private CompactCommandList _list = null!;
    internal Action InvalidatePointer { get; set; } = () => { };
    internal Rect2? ProtectedSubject { get; set; }
    internal FieldServiceMenu(string owner, params (string Id, string Key)[] commands)
    { _owner = owner; _keys = commands.Select(c => c.Key).ToArray(); _icons = commands.Select(c => new DiamondCommandButton(c.Id, c.Key, owner)).ToArray(); }
    public override void _Ready()
    {
        Theme = StoryTheme.Create();
        MouseFilter = MouseFilterEnum.Ignore;
        _list = new(_icons) { InvalidatePointer = () => InvalidatePointer() };
        AddChild(_title); AddChild(_list); AddChild(_back); AddChild(_reading); _reading.AddChild(_reason);
        _reason.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        ItemRectChanged += () => InvalidatePointer();
    }
    internal void Configure(IReadOnlyDictionary<string, ExplorationRaster>? rasters) => MenuRasterTextures.Configure(_icons, rasters, "", _owner);
    internal void Present(GameInput input, int index, string title, string reason, string backKey, IReadOnlyList<bool>? available = null)
    {
        _title.Text = title; _back.Text = (index == 4 ? "› " : "   ") + input.CompactHint(GameAction.Cancel) + " · " + GameText.Get(backKey);
        _back.AddThemeColorOverride("font_color", index == 4 ? StoryTheme.Selected : Colors.White);
        for (int i = 0; i < _icons.Length; i++)
        { _icons[i].Available = available is null || available[i]; _icons[i].RefreshCaption(); _icons[i].SetSelection(i == index, input.Settings.ReducedFlash); }
        string reading = reason;
        if (_reason.Text != reading) _reading.ScrollVertical = 0;
        _reason.Text = reading; _list.Present(_keys.Select(key => GameText.Get(key)).ToArray()); Arrange();
    }
    private void Arrange()
    {
        float width = Math.Min(560, GetViewportRect().Size.X - 64), inner = width - 32;
        _title.Position = new(16, 16); _title.Size = new(inner, 0);
        _list.Position = new(16, 40 + _title.GetMinimumSize().Y); _list.Arrange(inner);
        _back.Position = new(16, _list.Position.Y + _list.RequestedHeight + 8); _back.Size = new(inner, 64);
        float top = _back.Position.Y + 80;
        _reason.CustomMinimumSize = new(inner - 16, 0); _reason.CustomMaximumSize = new(inner - 16, -1);
        float height = Math.Min(_reason.GetMinimumSize().Y, Math.Max(64, GetViewportRect().Size.Y - 144 - top - 16));
        _reading.Position = new(16, top); _reading.Size = new(inner, height);
        Size = new(width, top + height + 16); Position = new(GetViewportRect().Size.X - 32 - width, 112);
        if (ProtectedSubject is { } subject && GetRect().Intersects(subject)) Position = new(32, 112);
    }
    internal int Hit(Vector2 p) => _list.Hit(p).Index;
    internal Rect2 RowRect(int index) => ((Control)_list.GetChild(index)).GetGlobalRect();
    internal Rect2 BackRect => _back.GetGlobalRect();
    internal bool BackHit(Vector2 p) => BackRect.HasPoint(p);
    internal void Wheel(int step) { _reading.ScrollVertical += step; InvalidatePointer(); }
    internal bool ReadingOverflow => _reason.GetMinimumSize().Y > _reading.Size.Y + 1;
    internal void Suspend() { foreach (var icon in _icons) icon.StopFocusEmphasis(); }
    public override void _Process(double delta) { if (IsVisibleInTree()) { Arrange(); foreach (var icon in _icons) icon.AdvanceFocus(delta); } }
    internal object Observe() => new { visible = IsVisibleInTree(), rect = Rect(this), title = _title.Text, list = _list.Observe(),
        titleRect = Rect(_title), backRect = BattleMapViewport.Rectangle(BackRect),
        reason = _reason.Text, readingRect = Rect(_reading), readingHeight = _reason.GetMinimumSize().Y, scroll = _reading.ScrollVertical,
        commands = _icons.Select((icon, i) => new { id = icon.Identity, selected = icon.Selected, enabled = icon.Available, rect = BattleMapViewport.Rectangle(RowRect(i)), art = icon.ObserveArt() }) };
    internal static object Rect(Control c) => BattleMapViewport.Rectangle(c.GetGlobalRect());
    internal static Label Label(string name, int size)
    { var label = new Label { Name = name, AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}

// Measured, scrollable choices and reading, with a fixed Back/footer outside the scroll content.
internal sealed partial class FieldChoicePanel : Panel
{
    private readonly ScrollContainer _scroll = new() { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, FocusMode = FocusModeEnum.None };
    private readonly VBoxContainer _body = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
    private readonly Label _title = FieldServiceMenu.Label("Title", 32), _hint = FieldServiceMenu.Label("Hint", 24), _back = FieldServiceMenu.Label("Back", 30);
    private readonly List<(Control Row, Label Text, Label Marker, IdentityIcon Icon)> _rows = [];
    internal Action InvalidatePointer { get; set; } = () => { };
    private int _focus = -1;
    private string _heading = "";
    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_scroll); _scroll.AddChild(_body); _body.AddChild(_title); _body.AddThemeConstantOverride("separation", 8);
        _title.SizeFlagsHorizontal = SizeFlags.ExpandFill; AddChild(_hint); AddChild(_back);
        ItemRectChanged += () => InvalidatePointer(); _scroll.GetVScrollBar().ValueChanged += _ => InvalidatePointer();
    }
    internal void Present(string title, string hint, string back, IReadOnlyList<(string Text, string? Icon, bool Available)> choices,
        IdentityIcons icons, int focus, float width = 560)
    {
        bool changed = _heading != title || _rows.Count < choices.Count; _heading = title; _title.Text = title; _hint.Text = hint; _back.Text = back;
        while (_rows.Count < choices.Count)
        {
            var row = new Control { MouseFilter = MouseFilterEnum.Ignore, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var text = FieldServiceMenu.Label("Choice" + _rows.Count, 30); var marker = FieldServiceMenu.Label("Marker", 30); var icon = new IdentityIcon(3);
            _body.AddChild(row); row.AddChild(marker); row.AddChild(icon); row.AddChild(text); _rows.Add((row, text, marker, icon));
            row.ItemRectChanged += () => InvalidatePointer();
        }
        float inner = width - 48;
        for (int i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i]; row.Row.Visible = i < choices.Count; if (i >= choices.Count) continue;
            row.Text.Text = choices[i].Text; row.Text.Position = new(88, 0); row.Text.Size = new(inner - 88, 0);
            float height = Math.Max(80, row.Text.GetMinimumSize().Y);
            row.Row.CustomMinimumSize = new(0, height); row.Text.Size = new(inner - 88, height);
            row.Marker.Text = i == focus ? "›" : ""; row.Marker.Size = new(20, height);
            row.Icon.Position = new(24, 4); row.Icon.Present(icons, choices[i].Icon, i == focus);
            var color = !choices[i].Available ? StoryTheme.Unavailable : i == focus ? StoryTheme.Selected : Colors.White;
            row.Text.AddThemeColorOverride("font_color", color); row.Marker.AddThemeColorOverride("font_color", color);
        }
        _hint.Position = new(16, 0); _hint.Size = new(width - 32, 0);
        float footer = _hint.GetMinimumSize().Y + 96;
        float desired = _title.GetMinimumSize().Y + choices.Select((_, i) => _rows[i].Row.CustomMinimumSize.Y + 8).Sum() + 48 + footer;
        Size = new(width, Math.Min(Math.Max(320, desired), GetViewportRect().Size.Y - 144));
        Position = new(GetViewportRect().Size.X - width - 32, 112);
        _scroll.Position = new(16, 16); _scroll.Size = new(width - 32, Math.Max(80, Size.Y - footer - 32));
        _hint.Position = new(16, Size.Y - footer); _back.Position = new(16, Size.Y - 80); _back.Size = new(width - 32, 64);
        if (changed) _scroll.ScrollVertical = 0;
        if ((changed || _focus != focus) && focus >= 0 && focus < choices.Count) _scroll.EnsureControlVisible(_rows[focus].Row);
        _focus = focus;
    }
    internal int Hit(Vector2 point) => _scroll.GetGlobalRect().HasPoint(point) ? _rows.FindIndex(r => r.Row.Visible && r.Row.GetGlobalRect().HasPoint(point)) : -1;
    internal Rect2 RowRect(int index) => _rows[index].Row.GetGlobalRect().Intersection(_scroll.GetGlobalRect());
    internal Rect2 BackRect => _back.GetGlobalRect();
    internal void Wheel(int step) { _scroll.ScrollVertical += step; InvalidatePointer(); }
    internal object Observe() => new { visible = IsVisibleInTree(), rect = FieldServiceMenu.Rect(this), title = _title.Text, titleRect = FieldServiceMenu.Rect(_title),
        readingRect = FieldServiceMenu.Rect(_scroll), scroll = _scroll.ScrollVertical, hint = _hint.Text, hintRect = FieldServiceMenu.Rect(_hint), backRect = FieldServiceMenu.Rect(_back),
        rows = _rows.Where(r => r.Row.Visible).Select(r => new { text = r.Text.Text, rect = FieldServiceMenu.Rect(r.Row), textRect = FieldServiceMenu.Rect(r.Text), minimumHeight = r.Text.GetMinimumSize().Y, icon = r.Icon.Observe() }) };
}

// Four numbered positions remain visible even when empty or when another pack receives input.
internal sealed partial class FieldPackPanel : Panel
{
    private readonly Label _title = FieldServiceMenu.Label("Owner", 30);
    private readonly ScrollContainer _scroll = new() { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, FocusMode = FocusModeEnum.None };
    private readonly Control _body = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
    private readonly List<(Label Text, IdentityIcon Icon)> _rows = [];
    internal Action InvalidatePointer { get; set; } = () => { };
    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore; AddChild(_scroll); _scroll.AddChild(_body); _body.AddChild(_title);
        _scroll.GetVScrollBar().ValueChanged += _ => InvalidatePointer();
        for (int i = 0; i < 4; i++) { var text = FieldServiceMenu.Label("Slot" + i, 30); var icon = new IdentityIcon(3); _body.AddChild(text); _body.AddChild(icon); _rows.Add((text, icon)); }
    }
    internal void Present(PartyMemberDetails member, string name, IdentityIcons icons, Vector2 beside)
    {
        Show(); if (_title.Text != name) _scroll.ScrollVertical = 0;
        _title.Text = name; _title.Position = Vector2.Zero; _title.Size = new(480, 0);
        float y = 24 + _title.GetMinimumSize().Y;
        for (int i = 0; i < 4; i++)
        {
            var slot = member.Inventory.Single(row => row.Slot == i); var row = _rows[i];
            row.Text.Text = $"{i + 1}. " + (slot.Empty == true ? GameText.Get("party.empty") : slot.Empty is null ? GameText.Get("party.inventory-unavailable") : DisplayNames.Item(slot.ItemId, slot.Name ?? GameText.Get("party.item-unavailable"))) +
                (slot.Equipped ? " · " + GameText.Get("party.equipped") : "") + (slot.Broken ? " · " + GameText.Get("party.broken") : "");
            row.Text.Position = new(72, y); row.Text.Size = new(408, 0); float height = Math.Max(80, row.Text.GetMinimumSize().Y); row.Text.Size = new(408, height);
            row.Icon.Position = new(8, y + 4); row.Icon.Present(icons, slot.ItemId is { } id ? IdentityIcons.Item(id) : null, false); y += height + 8;
        }
        _body.CustomMinimumSize = new(480, y);
        Size = new(528, Math.Min(y + 32, GetViewportRect().Size.Y - 144)); Position = new(beside.X - Size.X - 24, beside.Y);
        _scroll.Position = new(16, 16); _scroll.Size = new(496, Size.Y - 32);
    }
    internal void Wheel(int step) { _scroll.ScrollVertical += step; InvalidatePointer(); }
    internal object Observe() => new { visible = IsVisibleInTree(), title = _title.Text, rect = FieldServiceMenu.Rect(this), scroll = _scroll.ScrollVertical,
        rows = _rows.Select(r => new { text = r.Text.Text, rect = FieldServiceMenu.Rect(r.Text), icon = r.Icon.Observe() }) };
}

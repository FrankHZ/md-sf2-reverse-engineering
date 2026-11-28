using Godot;
using Sf2.Remake.GodotAdapter.Battles;

namespace Sf2.Remake.GodotAdapter.UI;

// A projection only: callers own focus, input gestures, eligibility and submission.
internal sealed partial class CompactCommandList : Control
{
    internal const float Width = 528, RowHeight = 80, Gap = 8;
    private readonly DiamondCommandButton[] _icons;
    private readonly List<Control> _rows = [];
    private readonly List<Label> _labels = [];
    private readonly List<Label> _markers = [];
    private int _hovered = -1;
    private bool _pressed;
    internal float RequestedHeight { get; private set; }
    internal int CaptionSize { get; init; } = 30;
    internal bool DrawActionSurfaces { get; init; }
    internal Action InvalidatePointer { get; set; } = () => { };

    internal CompactCommandList(DiamondCommandButton[] icons)
    {
        _icons = icons;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Ready()
    {
        for (int i = 0; i < _icons.Length; i++)
        {
            var row = new Control { Name = "Command" + i, MouseFilter = MouseFilterEnum.Ignore };
            var label = new Label { Name = "Caption", AutowrapMode = TextServer.AutowrapMode.WordSmart,
                VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
            var marker = new Label { Text = "›", VerticalAlignment = VerticalAlignment.Center,
                MouseFilter = MouseFilterEnum.Ignore };
            label.ThemeTypeVariation = ReadingSize.Role(CaptionSize);
            marker.ThemeTypeVariation = ReadingSize.Role(30);
            // Buttons with a separate caption already hide their internal label and
            // retain their existing short focus emphasis in the row.
            _icons[i].IconOnly = !_icons[i].CaptionBeside;
            AddChild(row); row.AddChild(marker); row.AddChild(_icons[i]); row.AddChild(label);
            _rows.Add(row); _labels.Add(label); _markers.Add(marker);
            row.ItemRectChanged += () => InvalidatePointer();
        }
    }

    internal void Present(string[] labels)
    {
        for (int i = 0; i < _labels.Count; i++)
        {
            _labels[i].Text = labels[i];
            var color = !_icons[i].Available ? StoryTheme.Unavailable : _icons[i].Selected ? StoryTheme.Selected : new Color("f4f5fc");
            _labels[i].AddThemeColorOverride("font_color", color);
            _markers[i].AddThemeColorOverride("font_color", color);
            _markers[i].Visible = _icons[i].Selected;
        }
        Arrange(Size.X > 0 ? Size.X : Width);
    }

    internal void Arrange(float width)
    {
        float y = 0;
        for (int i = 0; i < _rows.Count; i++)
        {
            var label = _labels[i];
            label.Size = new(Mathf.Max(1, width - 124), 0);
            float height = Mathf.Max(RowHeight, label.GetMinimumSize().Y);
            _rows[i].Position = new(0, y); _rows[i].Size = new(width, height);
            _markers[i].Position = Vector2.Zero; _markers[i].Size = new(20, height);
            _icons[i].Position = new(20, (height - 72) / 2); _icons[i].Size = new(72, 72); _icons[i].Arrange();
            label.Position = new(108, 0); label.Size = new(Mathf.Max(1, width - 124), height);
            y += height + Gap;
        }
        RequestedHeight = Mathf.Max(0, y - Gap); Size = new(width, RequestedHeight);
        if (DrawActionSurfaces) QueueRedraw();
    }

    internal void PresentPointer(int hovered, bool pressed)
    {
        if (_hovered == hovered && _pressed == pressed) return;
        _hovered = hovered;
        _pressed = pressed;
        QueueRedraw();
    }

    private string? SurfaceColor(int index)
    {
        if (!_icons[index].Available) return StoryTheme.ActionDisabled;
        if (index != _hovered) return null;
        return _pressed ? StoryTheme.ActionPressed : StoryTheme.ActionHover;
    }

    public override void _Draw()
    {
        if (!DrawActionSurfaces) return;
        for (int i = 0; i < _rows.Count; i++)
            if (SurfaceColor(i) is { } color) DrawRect(_rows[i].GetRect(), new Color(color));
    }

    internal (int Index, Rect2 Rect) Hit(Vector2 position)
    {
        if (!IsVisibleInTree()) return (-1, default);
        for (int i = 0; i < _rows.Count; i++)
            if (_rows[i].GetGlobalRect().HasPoint(position)) return (i, _rows[i].GetGlobalRect());
        return (-1, default);
    }

    internal Rect2 RowRect(int index) => _rows[index].GetGlobalRect();
    internal Rect2 CaptionRect(int index) => _labels[index].GetGlobalRect();
    internal string? RowSurfaceColor(int index) => DrawActionSurfaces ? SurfaceColor(index) : null;

    internal object Observe() => new { visible = IsVisibleInTree(), rect = BattleMapViewport.Rectangle(GetGlobalRect()),
        rows = _rows.Select((row, i) => new { label = _labels[i].Text, selected = _icons[i].Selected,
            enabled = _icons[i].Available, rect = BattleMapViewport.Rectangle(row.GetGlobalRect()),
            labelRect = BattleMapViewport.Rectangle(_labels[i].GetGlobalRect()),
            minimumHeight = _labels[i].GetMinimumSize().Y, art = _icons[i].ObserveArt() }).ToArray() };
}

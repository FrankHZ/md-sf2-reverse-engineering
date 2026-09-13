using Godot;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleMapViewport : Control
{
    private const int CellSize = 40;
    private readonly Control _board = new() { Name = "Board", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Dictionary<ActorRef, Label> _markers = [];
    private readonly List<ColorRect> _preview = [];
    private IReadOnlyList<MapPosition> _focus = [];
    private Vector2 _mapSize;

    public override void _Ready()
    {
        ClipContents = true;
        MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_board);
        Resized += Frame;
    }

    internal void Build(BattleDefinition definition)
    {
        _mapSize = new(definition.Width * CellSize, definition.Height * CellSize);
        _board.Size = _mapSize;
        for (int y = 0; y < definition.Height; y++)
            for (int x = 0; x < definition.Width; x++)
            {
                byte tile = definition.Terrain[y * 48 + x];
                _board.AddChild(new ColorRect { Position = new(x * CellSize, y * CellSize), Size = new(CellSize - 2, CellSize - 2),
                    Color = tile == 255 ? new(0.12f, 0.14f, 0.18f) : tile == 3 ? new(0.19f, 0.34f, 0.24f) : new(0.25f, 0.29f, 0.35f),
                    MouseFilter = MouseFilterEnum.Ignore });
            }
        foreach (var actor in definition.InitialActors)
        {
            var marker = new Label { Name = "Actor_" + actor.Actor.Value, Size = new(CellSize - 4, CellSize - 8),
                ClipText = true, MouseFilter = MouseFilterEnum.Ignore };
            marker.AddThemeFontSizeOverride("font_size", 10);
            _board.AddChild(marker);
            _markers.Add(actor.Actor, marker);
        }
    }

    internal void Present(BattlePresentation projection)
    {
        foreach (var marker in _markers.Values) marker.Visible = false;
        foreach (var actor in projection.Markers)
        {
            var node = _markers[actor.Actor];
            node.Visible = true;
            node.Position = new(actor.Position.X * CellSize + 2, actor.Position.Y * CellSize + 4);
            node.Text = actor.Text;
            node.Modulate = actor.Selected ? Colors.Gold : actor.Ally ? Colors.LightSkyBlue : Colors.LightCoral;
        }
        foreach (var node in _preview) { _board.RemoveChild(node); node.QueueFree(); }
        _preview.Clear();
        foreach (var position in projection.Preview)
        {
            var node = new ColorRect { Position = new(position.X * CellSize + 4, position.Y * CellSize + CellSize - 7),
                Size = new(CellSize - 10, 3), Color = Colors.Gold, MouseFilter = MouseFilterEnum.Ignore };
            _board.AddChild(node);
            _preview.Add(node);
        }
        _focus = projection.Focus;
        Frame();
    }

    private void Frame()
    {
        if (_mapSize == Vector2.Zero || Size.X <= 0 || Size.Y <= 0) return;
        var minimum = _focus.Count == 0 ? _mapSize / 2 : new Vector2(_focus[0].X, _focus[0].Y) * CellSize;
        var maximum = minimum;
        foreach (var position in _focus)
        {
            var point = new Vector2(position.X, position.Y) * CellSize;
            minimum = minimum.Min(point);
            maximum = maximum.Max(point);
        }
        minimum -= Vector2.One * CellSize;
        maximum += Vector2.One * (2 * CellSize);
        var extent = maximum - minimum;
        float scale = Mathf.Min(1, Mathf.Min(Size.X / extent.X, Size.Y / extent.Y));
        _board.Scale = Vector2.One * scale;
        var worldSize = _mapSize * scale;
        var desired = Size / 2 - (minimum + maximum) / 2 * scale;
        _board.Position = new(
            worldSize.X <= Size.X ? (Size.X - worldSize.X) / 2 : Mathf.Clamp(desired.X, Size.X - worldSize.X, 0),
            worldSize.Y <= Size.Y ? (Size.Y - worldSize.Y) / 2 : Mathf.Clamp(desired.Y, Size.Y - worldSize.Y, 0));
    }

    internal int BoardChildren => _board.GetChildCount();
    internal int PreviewCount => _preview.Count;
    internal float Zoom => _board.Scale.X;
    internal object? PreviewRectangle => _preview.Count == 0 ? null : Rectangle(_preview[^1].GetGlobalRect());
    internal bool PreviewInsideMap => _preview.Count > 0 && GetGlobalRect().Encloses(_preview[^1].GetGlobalRect());
    internal IEnumerable<object> ObserveActors(IEnumerable<BattleActorState> actors) => actors.Select(a =>
    {
        var node = _markers[a.Actor];
        return new { id = a.Actor.Value, hp = a.Hp, mp = a.Mp, exp = a.Exp, kills = a.Kills, defeats = a.Defeats, x = a.Position?.X, y = a.Position?.Y,
            nodeX = node.Position.X, nodeY = node.Position.Y, visible = node.Visible, text = node.Text,
            globalRect = Rectangle(node.GetGlobalRect()), insideMap = GetGlobalRect().Encloses(node.GetGlobalRect()) };
    });
    internal static object Rectangle(Rect2 rect) => new
    { x = rect.Position.X, y = rect.Position.Y, width = rect.Size.X, height = rect.Size.Y };
}

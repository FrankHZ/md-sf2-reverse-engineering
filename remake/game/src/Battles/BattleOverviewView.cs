using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

// On-demand schematic of admitted semantic terrain, never a second gameplay board.
internal sealed partial class BattleOverviewView : Panel
{
    private BattleInspectionSnapshot? _snapshot;
    private Rect2 _camera, _mapRect;
    private int _selected, _drawnCells;
    private Vector2I _lastCell;
    private readonly Dictionary<TerrainSurface, int> _drawnClasses = [];
    private readonly Dictionary<TerrainSurface, Label> _terrainLabels = [];
    private readonly Dictionary<TerrainSurface, HBoxContainer> _terrainEntries = [];
    private readonly HBoxContainer _terrainLegend = new() { Name = "TerrainLegend" };
    private readonly Label _terrainTitle = new() { Name = "TerrainTitle" };
    private readonly Label _known = new() { Name = "KnownUnits", AutowrapMode = TextServer.AutowrapMode.WordSmart };
    private readonly Label _legend = new() { Name = "Legend", AutowrapMode = TextServer.AutowrapMode.WordSmart };
    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        _known.AddThemeFontSizeOverride("font_size", 25); _legend.AddThemeFontSizeOverride("font_size", 22);
        AddChild(_known); AddChild(_legend); AddChild(_terrainLegend);
        _terrainLegend.AddThemeConstantOverride("separation", 16); _terrainLegend.AddChild(_terrainTitle);
        _terrainTitle.AddThemeFontSizeOverride("font_size", 20);
        foreach (var surface in Enum.GetValues<TerrainSurface>())
        {
            var entry = new HBoxContainer { Name = surface.ToString() };
            var swatch = new ColorRect { Color = TerrainColor(surface), CustomMinimumSize = new(16, 16),
                SizeFlagsVertical = SizeFlags.ShrinkCenter, MouseFilter = MouseFilterEnum.Ignore };
            var label = new Label { Name = "Class" };
            entry.AddChild(swatch); entry.AddChild(label); _terrainEntries.Add(surface, entry);
            label.AddThemeFontSizeOverride("font_size", 20);
            _terrainLabels.Add(surface, label); _terrainLegend.AddChild(entry);
        }
    }
    internal void Present(BattleInspectionSnapshot snapshot, Rect2 camera, int selected)
    {
        _snapshot = snapshot; _camera = camera; _selected = selected;
        float scale = Mathf.Min((Size.X * 0.62f - 48) / snapshot.Width, (Size.Y - 152) / snapshot.Height);
        _mapRect = new(new Vector2(24, 128), new Vector2(snapshot.Width, snapshot.Height) * scale);
        _legend.Text = GameText.Get("battle.inspect.legend");
        _legend.Position = new(24, 12); _legend.Size = new(Size.X - 48, 60);
        _terrainLegend.Position = new(24, 78); _terrainLegend.Size = new(Size.X - 48, 40);
        _terrainTitle.Text = GameText.Get("battle.inspect.terrain", "");
        foreach (var (surface, label) in _terrainLabels)
        {
            _terrainEntries[surface].Visible = snapshot.Terrain.Contains(surface);
            label.Text = GameText.Get("battle.inspect.terrain." + surface.ToString().ToLowerInvariant());
        }
        _known.Position = new(Size.X * 0.64f, 128); _known.Size = new(Size.X * 0.36f - 24, Size.Y - 152);
        // A bounded scrolling window keeps every known unit reachable on small hosts.
        int count = Math.Max(1, (int)((Size.Y - 190) / 72));
        int first = Math.Clamp(selected - count / 2, 0, Math.Max(0, snapshot.Units.Count - count));
        _known.Text = GameText.Get("battle.inspect.known-count", snapshot.Units.Count) + "\n" +
            string.Join("\n", snapshot.Units.Skip(first).Take(count).Select((u, i) =>
            (first + i == selected ? "> " : "  ") + GameText.Get(u.Ally ? "battle.inspect.ally" : "battle.inspect.enemy") + " · " +
            DisplayNames.InspectionName(u.Ally, u.NameIndex, u.Name, u.CustomName) + $" ({u.Position.X},{u.Position.Y})"));
        QueueRedraw();
    }
    public override void _Draw()
    {
        if (_snapshot is not { } s) return;
        Vector2 scale = _mapRect.Size / new Vector2(s.Width, s.Height);
        _drawnCells = 0; _drawnClasses.Clear();
        for (int y = 0; y < s.Height; y++)
            for (int x = 0; x < s.Width; x++)
            {
                var surface = s.Terrain[y * s.Width + x];
                DrawRect(new(_mapRect.Position + new Vector2(x, y) * scale, scale), TerrainColor(surface));
                _drawnCells++; _lastCell = new(x, y);
                _drawnClasses[surface] = _drawnClasses.GetValueOrDefault(surface) + 1;
            }
        DrawRect(_mapRect, Colors.White, false, 1);
        DrawRect(new(_mapRect.Position + _camera.Position * scale, _camera.Size * scale), Colors.White, false, 2);
        for (int i = 0; i < s.Units.Count; i++)
        {
            var u = s.Units[i]; Vector2 p = _mapRect.Position + new Vector2(u.Position.X + 0.5f, u.Position.Y + 0.5f) * scale;
            if (u.Ally) DrawCircle(p, 6, Colors.LightSkyBlue);
            else DrawColoredPolygon([p + new Vector2(0, -7), p + new Vector2(7, 0), p + new Vector2(0, 7), p + new Vector2(-7, 0)], Colors.LightCoral);
            if (u.Actor == s.ActiveActor) DrawArc(p, 10, 0, Mathf.Tau, 24, Colors.White, 2);
            if (i == _selected) DrawRect(new Rect2(p - Vector2.One * 13, Vector2.One * 26), Colors.Yellow, false, 2);
        }
    }
    private static Color TerrainColor(TerrainSurface surface) => new(surface switch
    {
        TerrainSurface.Open => "#568a69", TerrainSurface.Brush => "#2b6c48", TerrainSurface.Rough => "#a78c6b",
        TerrainSurface.Deep => "#507fac", TerrainSurface.Desert => "#d2b371", TerrainSurface.Impassable => "#7e7a91",
        TerrainSurface.HighSky => "#a1cbdf", TerrainSurface.Barrier => "#404756", _ => throw new InvalidOperationException(),
    });
    internal object Observe() => new { units = _snapshot?.Units, camera = BattleMapViewport.Rectangle(_camera),
        map = BattleMapViewport.Rectangle(_mapRect), selected = _selected, knownText = _known.Text,
        legend = _legend.Text, terrainTitle = _terrainTitle.Text, terrainCellCount = _drawnCells,
        lastTerrainCell = new { x = _lastCell.X, y = _lastCell.Y },
        terrainClasses = _drawnClasses.Select(pair => new { surface = pair.Key.ToString(), value = (int)pair.Key,
            color = TerrainColor(pair.Key).ToHtml(), cells = pair.Value, label = _terrainLabels[pair.Key].Text }), pointerActions = false };
}

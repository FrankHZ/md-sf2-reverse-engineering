using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

// One retained tile-priority pass. The board applies the same transform to terrain,
// actors and previews; this node only translates source cells into local battle cells.
internal sealed partial class BattlefieldTerrain : Node2D
{
    private ExplorationMapDefinition _map = null!;
    private ExplorationMapVisual _visual = null!;
    private BattleMapRegion _region = null!;
    private WorkingMapLayout _originalLayout = null!;
    private bool _hd;
    private WorkingMapLayout _layout = null!;
    private MapBlockTextures _textures = null!;
    private Vector2I _sourceOrigin;
    private int _width, _height, _cellSize;
    private bool _high, _overlay, _followsTarget;
    private readonly HashSet<(int X, int Y)> _openChests = [];
    internal Vector2 CameraShift { get; set; }
    private MapPosition _target = new(0, 0);
    private int _draws;
    private long _drawSequence;
    private object? _first, _firstContext;

    internal void Configure(ExplorationMapDefinition map, ExplorationMapVisual visual, BattleMapRegion region, WorkingMapLayout layout,
        MapBlockTextures textures, int width, int height, int cellSize, Vector2I sourceOrigin, bool overlay, bool high, bool followsTarget, bool hd)
    {
        _map = map; _visual = visual; _region = region; _originalLayout = _layout = layout; _textures = textures;
        _width = width; _height = height; _cellSize = cellSize;
        _sourceOrigin = sourceOrigin; _overlay = overlay; _high = high; _followsTarget = followsTarget;
        Name = (overlay ? "Foreground" : "Background") + (high ? "High" : "Low");
        ZIndex = high ? 2 : 0;
        TextureFilter = TextureFilterEnum.Nearest;
        _hd = hd;
        Prepare(hd);
        QueueRedraw();
    }

    internal void Prepare(bool hd)
    {
        if (hd) _textures.PrepareHd(_visual);
        // Decode before the host reports successful attachment. Missing blocks or
        // atlas bytes must reach its visible adapter failure, not a draw callback.
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                var (sx, sy) = SourceCell(x, y);
                if (sx is < 0 or >= 64 || sy is < 0 or >= 64) continue;
                int block = _layout[sx, sy] & 0x3FF;
                if (!_overlay || block != 0) TextureAt(sx, sy, block, hd, out _);
            }
    }

    private ImageTexture TextureAt(int x, int y, int block, bool hd, out bool contextual)
    {
        var bounds = new Rect2I(_sourceOrigin, _followsTarget ? new(19, 19) : new(_width, _height));
        var variant = MapBlockTextures.Variant(_visual, hd, _layout, x, y, bounds, out contextual);
        return _textures.Block(_visual, block, variant);
    }

    internal void SelectMode(bool hd)
    {
        if (_hd == hd) return;
        _hd = hd;
        QueueRedraw();
    }

    internal void PresentChests(IReadOnlyList<int> flags)
    {
        if (_map.Search is not { } search) return;
        bool changed = false;
        foreach (var chest in search.Chests)
        {
            var cell = (chest.Position.X, chest.Position.Y);
            changed |= flags.Contains(chest.Flag) ? _openChests.Add(cell) : _openChests.Remove(cell);
        }
        if (!changed) return;
        var words = _originalLayout.Words.ToArray();
        foreach (var (x, y) in _openChests) words[y * 64 + x] = search.OpenChest;
        _layout = _originalLayout.WithWords(words);
        QueueRedraw();
    }

    internal void Follow(MapPosition target)
    {
        if (!_followsTarget || target == _target) return;
        _target = target;
        QueueRedraw();
    }

    private (int X, int Y) SourceCell(int x, int y)
    {
        // Target-following planes store their aperture around secondary cell (9,9).
        // Extend its opaque edge over the larger presentation viewport.
        if (_followsTarget) return (_sourceOrigin.X + Math.Clamp(9 + x - _target.X, 0, 18),
            _sourceOrigin.Y + Math.Clamp(9 + y - _target.Y, 0, 18));
        return (_sourceOrigin.X + x, _sourceOrigin.Y + y);
    }

    public override void _Draw()
    {
        _draws = 0; _first = _firstContext = null; _drawSequence++;
        float scale = _cellSize / 24f;
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                var (sourceX, sourceY) = SourceCell(x, y);
                if (sourceX is < 0 or >= 64 || sourceY is < 0 or >= 64) continue;
                int block = _layout[sourceX, sourceY] & 0x3FF;
                if (_overlay && block == 0) continue;
                var texture = TextureAt(sourceX, sourceY, block, _hd, out bool contextual);
                for (int tile = 0; tile < 9; tile++)
                {
                    int word = _visual.Blocks[block][tile];
                    if (((word & 0x8000) != 0) != _high) continue;
                    var tileOrigin = new Vector2(tile % 3 * 8, tile / 3 * 8);
                    var destination = new Rect2(new Vector2(x, y) * _cellSize + tileOrigin * scale, Vector2.One * 8 * scale);
                    var source = Exploration.ExplorationRasterImages.TextureRegion(new(tileOrigin, Vector2.One * 8), texture);
                    DrawTextureRectRegion(texture, destination, source);
                    _draws++;
                    if (_first is null || contextual && _firstContext is null)
                    {
                        var sample = new { sourceX, sourceY, block, tile, word, texture = texture.GetInstanceId().ToString(),
                            pixels = texture.GetWidth(), contextual, source = BattleMapViewport.Rectangle(source),
                            rect = BattleMapViewport.Rectangle(destination) };
                        _first ??= sample;
                        if (contextual) _firstContext ??= sample;
                    }
                }
            }
    }

    internal object Observe() => new
    {
        name = Name.ToString(), map = _map.Map.Value, origin = _region.Origin,
        openedChests = _openChests.Select(cell => new { cell.X, cell.Y, block = _map.Search!.OpenChest & 0x3FF }),
        width = _width, height = _height, offsetX = _sourceOrigin.X - _region.Origin.X, offsetY = _sourceOrigin.Y - _region.Origin.Y,
        positionX = Position.X, positionY = Position.Y, target = _followsTarget ? _target : null, overlay = _overlay, highPriority = _high, z = ZIndex, draws = _draws, drawSequence = _drawSequence, first = _first, firstContext = _firstContext,
        visualMap = _visual.Map.Value, atlasWidth = _visual.Atlas.Width, atlasHeight = _visual.Atlas.Height,
        atlasScale = _visual.Scale, blocks = _visual.Blocks.Count,
    };
}

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
    private object? _first;

    internal void Configure(ExplorationMapDefinition map, ExplorationMapVisual visual, BattleMapRegion region, WorkingMapLayout layout,
        MapBlockTextures textures, int width, int height, int cellSize, Vector2I sourceOrigin, bool overlay, bool high, bool followsTarget)
    {
        _map = map; _visual = visual; _region = region; _layout = layout; _textures = textures;
        _width = width; _height = height; _cellSize = cellSize;
        _sourceOrigin = sourceOrigin; _overlay = overlay; _high = high; _followsTarget = followsTarget;
        Name = (overlay ? "Foreground" : "Background") + (high ? "High" : "Low");
        ZIndex = high ? 2 : 0;
        TextureFilter = TextureFilterEnum.Nearest;
        // Decode before the host reports successful attachment. Missing blocks or
        // atlas bytes must reach its visible adapter failure, not a draw callback.
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var (sx, sy) = SourceCell(x, y);
                if (sx is < 0 or >= 64 || sy is < 0 or >= 64) continue;
                int block = _layout[sx, sy] & 0x3FF;
                if (!overlay || block != 0) textures.Block(visual, block);
            }
        QueueRedraw();
    }

    internal void PresentChests(IReadOnlyList<int> flags)
    {
        if (_map.Search is not { } search) return;
        foreach (var chest in search.Chests)
        {
            var cell = (chest.Position.X, chest.Position.Y);
            bool changed = flags.Contains(chest.Flag) ? _openChests.Add(cell) : _openChests.Remove(cell);
            if (changed) QueueRedraw();
        }
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
        _draws = 0; _first = null; _drawSequence++;
        float scale = _cellSize / 24f;
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                var (sourceX, sourceY) = SourceCell(x, y);
                if (sourceX is < 0 or >= 64 || sourceY is < 0 or >= 64) continue;
                int wordAtCell = _openChests.Contains((sourceX, sourceY))
                    ? _map.Search!.OpenChest : _layout[sourceX, sourceY];
                int block = wordAtCell & 0x3FF;
                if (_overlay && block == 0) continue;
                var texture = _textures.Block(_visual, block);
                for (int tile = 0; tile < 9; tile++)
                {
                    int word = _visual.Blocks[block][tile];
                    if (((word & 0x8000) != 0) != _high) continue;
                    var tileOrigin = new Vector2(tile % 3 * 8, tile / 3 * 8);
                    var destination = new Rect2(new Vector2(x, y) * _cellSize + tileOrigin * scale, Vector2.One * 8 * scale);
                    DrawTextureRectRegion(texture, destination, new(tileOrigin, Vector2.One * 8));
                    _draws++;
                    _first ??= new { sourceX, sourceY, block, tile, word, texture = texture.GetInstanceId().ToString(),
                        rect = BattleMapViewport.Rectangle(destination) };
                }
            }
    }

    internal object Observe() => new
    {
        name = Name.ToString(), map = _map.Map.Value, origin = _region.Origin,
        openedChests = _openChests.Select(cell => new { cell.X, cell.Y, block = _map.Search!.OpenChest & 0x3FF }),
        width = _width, height = _height, offsetX = _sourceOrigin.X - _region.Origin.X, offsetY = _sourceOrigin.Y - _region.Origin.Y,
        positionX = Position.X, positionY = Position.Y, target = _followsTarget ? _target : null, overlay = _overlay, highPriority = _high, z = ZIndex, draws = _draws, drawSequence = _drawSequence, first = _first,
        visualMap = _visual.Map.Value, atlasWidth = _visual.Atlas.Width, atlasHeight = _visual.Atlas.Height,
        atlasScale = _visual.Scale, blocks = _visual.Blocks.Count,
    };
}

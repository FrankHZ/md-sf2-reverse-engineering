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
    private MapBlockTextures _textures = null!;
    private Vector2I _offset;
    private int _width, _height, _cellSize;
    private bool _high, _overlay;
    private int _draws;
    private long _drawSequence;
    private object? _first;

    internal void Configure(ExplorationMapDefinition map, ExplorationMapVisual visual, BattleMapRegion region,
        MapBlockTextures textures, int width, int height, int cellSize, Vector2I offset, bool overlay, bool high)
    {
        _map = map; _visual = visual; _region = region; _textures = textures;
        _width = width; _height = height; _cellSize = cellSize;
        _offset = offset; _overlay = overlay; _high = high;
        Name = (overlay ? "Foreground" : "Background") + (high ? "High" : "Low");
        ZIndex = high ? 2 : 0;
        TextureFilter = TextureFilterEnum.Nearest;
        // Decode before the host reports successful attachment. Missing blocks or
        // atlas bytes must reach its visible adapter failure, not a draw callback.
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int sx = region.Origin.X + x + offset.X, sy = region.Origin.Y + y + offset.Y;
                if (sx is < 0 or >= 64 || sy is < 0 or >= 64) continue;
                int block = map.Layout[sx, sy] & 0x3FF;
                if (!overlay || block != 0) textures.Block(visual, block);
            }
        QueueRedraw();
    }

    public override void _Draw()
    {
        _draws = 0; _first = null; _drawSequence++;
        float scale = _cellSize / 24f;
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                int sourceX = _region.Origin.X + x + _offset.X, sourceY = _region.Origin.Y + y + _offset.Y;
                if (sourceX is < 0 or >= 64 || sourceY is < 0 or >= 64) continue;
                int block = _map.Layout[sourceX, sourceY] & 0x3FF;
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
        width = _width, height = _height, offsetX = _offset.X, offsetY = _offset.Y,
        overlay = _overlay, highPriority = _high, z = ZIndex, draws = _draws, drawSequence = _drawSequence, first = _first,
        visualMap = _visual.Map.Value, atlasWidth = _visual.Atlas.Width, atlasHeight = _visual.Atlas.Height,
        atlasScale = _visual.Scale, blocks = _visual.Blocks.Count,
    };
}

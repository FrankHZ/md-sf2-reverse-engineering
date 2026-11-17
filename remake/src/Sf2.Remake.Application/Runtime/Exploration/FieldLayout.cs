using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

// ConvertMapPixelCoordinatesToOffset: field layer != 0 selects the secondary
// plane's layout origin. Parallax affects drawing, not collision coordinates.
internal static class FieldLayout
{
    internal static (int X, int Y) Offset(ExplorationState world, EntityMotionState motion, bool inBattle = false)
    {
        if (inBattle || motion.Layer == 0 || world.Definition.ViewAreas.Count == 0) return (0, 0);
        var position = world.PlayerEntity.Position;
        var area = world.Definition.ViewAreas.FirstOrDefault(row => position.X >= row.MinX && position.X <= row.MaxX &&
            position.Y >= row.MinY && position.Y <= row.MaxY);
        // An explicit scene may park the player off-map while its actors keep moving.
        if (area is null && world.Definition.ViewAreas.Count == 1) area = world.Definition.ViewAreas[0];
        if (area is null) throw new BattleRuleException("field-layer-area", "map.layout", true);
        return area.Layer == 0 ? (area.ForegroundX, area.ForegroundY) : (area.BackgroundX, area.BackgroundY);
    }

    internal static ushort? Word(ExplorationState world, EntityMotionState motion, int x, int y, bool inBattle = false)
    {
        var offset = Offset(world, motion, inBattle);
        x += offset.X; y += offset.Y;
        return x is >= 0 and < 64 && y is >= 0 and < 64 ? world.Layout[x, y] : null;
    }
}

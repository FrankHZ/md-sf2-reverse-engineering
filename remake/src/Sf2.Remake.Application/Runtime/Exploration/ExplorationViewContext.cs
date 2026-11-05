using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

// A derived immutable selection, shared by logical music and delivery. The logical
// view owns the loaded area even when actors remain elsewhere; live scroll registers
// are not area identity. No context is cached independently of the snapshot.
public sealed record ExplorationViewContext(MapId Map, int Area, string? Error = null)
{
    internal static ExplorationViewContext Require(ExplorationState world, LogicalView? view)
    {
        var context = Resolve(world, view);
        if (context.Error is { } error) throw new BattleRuleException(error, "story.view", true);
        return context;
    }

    public static ExplorationViewContext Resolve(ExplorationState world, LogicalView? view)
    {
        if (view is null)
        {
            var selected = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position);
            return selected is null ? new(world.Map, -1, "map-music-area-unavailable") :
                new(world.Map, selected.OneBasedRecordOrdinal - 1);
        }

        int index = -1;
        for (int i = 0; i < world.Definition.ViewAreas.Count; i++)
        {
            var source = world.Definition.ViewAreas[i];
            if (source != view.Area with { AutoscrollAX = source.AutoscrollAX,
                AutoscrollAY = source.AutoscrollAY, AutoscrollBX = source.AutoscrollBX,
                AutoscrollBY = source.AutoscrollBY }) continue;
            if (index >= 0)
                return new(world.Map, -1, "map-music-area-ambiguous");
            index = i;
        }
        if (index < 0) return new(world.Map, -1, "map-music-area-unavailable");
        return new(world.Map, index);
    }
}

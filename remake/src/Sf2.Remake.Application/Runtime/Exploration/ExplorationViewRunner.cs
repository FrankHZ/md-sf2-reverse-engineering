using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class ExplorationViewRunner
{
    internal static LogicalView Initialize(ExplorationState world)
    {
        var player = world.PlayerEntity;
        var area = world.Definition.ViewAreas.FirstOrDefault(area =>
            player.Motion.X >= area.MinX * 384 && player.Motion.X <= area.MaxX * 384 &&
            player.Motion.Y >= area.MinY * 384 && player.Motion.Y <= area.MaxY * 384)
            ?? throw new BattleRuleException("field-view-area", "map.view", true);
        Validate(area);
        // Source LoadMap clamps in this order and quantizes the origin to eight pixels.
        int Center(int target, int low, int high, int trailing, int size)
        {
            int origin = target - 1920;
            if (origin < low * 384) origin = low * 384;
            if (target + trailing > high * 384) origin = high * 384 - size;
            return (origin >> 7) << 7;
        }
        int x = Center(player.Motion.X, area.MinX, area.MaxX, 1920, 3840);
        int y = Center(player.Motion.Y, area.MinY, area.MaxY, 1536, 3456);
        // No active destination is asserted by LoadMap. Its stale RAM words are unconsumed.
        return new(area, player.Slot, new(x + area.ForegroundX * 384), new(y + area.ForegroundY * 384),
            new(x + area.BackgroundX * 384), new(y + area.BackgroundY * 384));
    }

    internal static void Validate(ExplorationViewArea area)
    {
        if (area.ParallaxAX != 256 || area.ParallaxAY != 256 || area.ParallaxBX != 256 || area.ParallaxBY != 256 ||
            area.AutoscrollAX != 0 || area.AutoscrollAY != 0 || area.AutoscrollBX != 0 || area.AutoscrollBY != 0 || area.Layer != 0 || area.BackgroundX != 0 || area.BackgroundY != 0)
            throw new BattleRuleException("field-view-profile", "map.view", true);
    }

    internal static LogicalView Tick(ExplorationState world, LogicalView view, ExplorationTextSettings settings)
    {
        Validate(view.Area);
        if (settings.ViewSpeed != 0) throw new BattleRuleException("field-view-speed", "story.textSettings", true);
        var target = world.AllEntities.FirstOrDefault(entity => entity.Slot == view.TargetSlot);
        if (target is null || target.Slot == 63)
            throw new BattleRuleException("field-view-target", "story.view", true);
        // View data runs before scrolling. An active mask leaves its existing speeds intact.
        if (!view.Scrolling)
        {
            var area = view.Area;
            int x = area.Layer == 0 ? view.BX.Position : view.AX.Position;
            int y = area.Layer == 0 ? view.BY.Position : view.AY.Position;
            int nextX = Follow(target.Motion.X, x, area.MinX * 384, area.MaxX * 384 - 3840);
            int nextY = Follow(target.Motion.Y, y, area.MinY * 384, area.MaxY * 384 - 3456);
            bool changed = x != nextX || y != nextY;
            int counter = changed ? unchecked((ushort)(view.FollowCounter + 1)) : 0;
            int speed = unchecked((short)counter) > 6 ? 32 : 24;
            LogicalViewAxis Destination(LogicalViewAxis axis, int destination) => changed
                ? axis with { Destination = axis.Position == destination ? null : destination, Speed = speed }
                : axis with { Speed = speed };
            view = view with
            {
                AX = Destination(view.AX, nextX + area.ForegroundX * 384),
                AY = Destination(view.AY, nextY + area.ForegroundY * 384),
                BX = Destination(view.BX, nextX + area.BackgroundX * 384),
                BY = Destination(view.BY, nextY + area.BackgroundY * 384), FollowCounter = counter,
            };
        }
        bool hide = view.AX.Active || view.AY.Active;
        return view with { AX = Scroll(view.AX), AY = Scroll(view.AY), BX = Scroll(view.BX), BY = Scroll(view.BY), HideWindows = hide };
    }

    private static int Follow(int target, int view, int minimum, int maximum) =>
        target < view + 1536 ? (view > minimum ? view - 384 : view) :
        target > view + 2304 && view < maximum ? view + 384 : view;

    private static LogicalViewAxis Scroll(LogicalViewAxis axis)
    {
        if (axis.Destination is not { } destination) return axis;
        if (axis.Speed <= 0) throw new BattleRuleException("field-view-scroll-speed", "story.view", true);
        int distance = destination - axis.Position;
        return Math.Abs(distance) <= axis.Speed ? axis with { Position = destination, Destination = null } :
            axis with { Position = axis.Position + Math.Sign(distance) * axis.Speed };
    }
}

using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

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
        return new(area, player.Slot, new(Plane(x, area.ParallaxAX, area.ForegroundX)), new(Plane(y, area.ParallaxAY, area.ForegroundY)),
            new(Plane(x, area.ParallaxBX, area.BackgroundX)), new(Plane(y, area.ParallaxBY, area.BackgroundY)));
    }

    internal static void Validate(ExplorationViewArea area)
    {
        bool main = area.Layer switch
        {
            0 => area.BackgroundX == 0 && area.BackgroundY == 0 && area.ParallaxBX == 256 && area.ParallaxBY == 256,
            254 or 255 => area.ForegroundX == 0 && area.ForegroundY == 0 && area.ParallaxAX == 256 && area.ParallaxAY == 256,
            _ => false,
        };
        if (!main || area.ParallaxAX is not (128 or 256) || area.ParallaxAY is not (128 or 256) ||
            area.ParallaxBX is not (128 or 256) || area.ParallaxBY is not (128 or 256) ||
            new[] { area.AutoscrollAX, area.AutoscrollAY, area.AutoscrollBX, area.AutoscrollBY }.Any(value => value is < -127 or > 255 || (byte)value == 128))
            throw new BattleRuleException("field-view-profile", "map.view", true);
    }

    internal static LogicalView LoadScene(ExplorationState world, MapPosition origin, LogicalView previous)
    {
        // csc48 passes an explicit origin in eight-pixel units to LoadMap. Area
        // selection uses that origin, independently of the retained player slot.
        int x = origin.X * 384, y = origin.Y * 384;
        var area = world.Definition.ViewAreas.FirstOrDefault(area =>
            x >= area.MinX * 384 && x <= area.MaxX * 384 && y >= area.MinY * 384 && y <= area.MaxY * 384)
            ?? throw new BattleRuleException("field-view-area", "program.map", true);
        Validate(area);
        // LoadMap clears speeds and scrolling bits, not word_FFA828. Destination
        // words are unconsumed after their bits clear; csc48 has detached following.
        return new(area, null, new(Plane(x, area.ParallaxAX, area.ForegroundX)), new(Plane(y, area.ParallaxAY, area.ForegroundY)),
            new(Plane(x, area.ParallaxBX, area.BackgroundX)), new(Plane(y, area.ParallaxBY, area.BackgroundY)), previous.FollowCounter);
    }

    // Source uses unsigned multiply, shift, then the independent layer origin.
    // Keep the admitted domain below signed-word wrap for both current and destination axes.
    private static int Plane(int position, int parallax, int offset)
    {
        long result = ((long)position * parallax >> 8) + offset * 384;
        if (position is < 0 or > short.MaxValue || result is < 0 or > short.MaxValue)
            throw new BattleRuleException("field-view-destination", "program.camera", true);
        return (int)result;
    }

    internal static LogicalView SetDestination(LogicalView view, MapPosition position)
    {
        Validate(view.Area);
        LogicalViewAxis Set(LogicalViewAxis axis, int destination, int auto)
        {
            if (auto != 0) return axis;
            // This profile does not generalize the source word-wrap domain.
            if (destination is < 0 or > short.MaxValue)
                throw new BattleRuleException("field-view-destination", "program.camera", true);
            // SetViewDestination only sets unequal-axis bits; equality does not
            // clear an already active axis. Scroll clears that bit on its next pass.
            return axis with { Destination = axis.Active || axis.Position != destination ? destination : null };
        }
        int x = position.X * 384, y = position.Y * 384;
        return view with { TargetSlot = null,
            AX = Set(view.AX, Plane(x, view.Area.ParallaxAX, view.Area.ForegroundX), view.Area.AutoscrollAX), AY = Set(view.AY, Plane(y, view.Area.ParallaxAY, view.Area.ForegroundY), view.Area.AutoscrollAY),
            BX = Set(view.BX, Plane(x, view.Area.ParallaxBX, view.Area.BackgroundX), view.Area.AutoscrollBX), BY = Set(view.BY, Plane(y, view.Area.ParallaxBY, view.Area.BackgroundY), view.Area.AutoscrollBY) };
    }

    internal static LogicalView Tick(ExplorationState world, LogicalView view, ExplorationTextSettings settings)
    {
        Validate(view.Area);
        var target = view.TargetSlot is null ? null : world.AllEntities.FirstOrDefault(entity => entity.Slot == view.TargetSlot);
        if (view.TargetSlot is not null && (target is null || target.Slot == 63))
            throw new BattleRuleException("field-view-target", "story.view", true);
        if (target is null)
        {
            // The FF branch bypasses following but prepares speed even during a scroll.
            int speed = unchecked((short)view.FollowCounter) > 6 ? 32 : 24;
            view = Speeds(view, settings.ViewSpeed == 0 ? speed : settings.ViewSpeed);
        }
        // A followed entity with an active mask retains its existing speeds.
        else if (!view.Scrolling)
        {
            var area = view.Area;
            int x = area.Layer == 0 ? view.BX.Position : view.AX.Position;
            int y = area.Layer == 0 ? view.BY.Position : view.AY.Position;
            int nextX = Follow(target.Motion.X, x, area.MinX * 384, area.MaxX * 384 - 3840);
            int nextY = Follow(target.Motion.Y, y, area.MinY * 384, area.MaxY * 384 - 3456);
            bool changed = x != nextX || y != nextY;
            int counter = changed ? unchecked((ushort)(view.FollowCounter + 1)) : 0;
            int speed = unchecked((short)counter) > 6 ? 32 : 24;
            view = Speeds(view, settings.ViewSpeed == 0 ? speed : settings.ViewSpeed);
            LogicalViewAxis Destination(LogicalViewAxis axis, int destination, int auto) => changed && auto == 0
                ? axis with { Destination = axis.Position == destination ? null : destination }
                : axis;
            view = view with
            {
                AX = Destination(view.AX, Plane(nextX, area.ParallaxAX, area.ForegroundX), area.AutoscrollAX),
                AY = Destination(view.AY, Plane(nextY, area.ParallaxAY, area.ForegroundY), area.AutoscrollAY),
                BX = Destination(view.BX, Plane(nextX, area.ParallaxBX, area.BackgroundX), area.AutoscrollBX),
                BY = Destination(view.BY, Plane(nextY, area.ParallaxBY, area.BackgroundY), area.AutoscrollBY), FollowCounter = counter,
            };
        }
        bool hide = view.AX.Active || view.AY.Active;
        return view with { AX = Advance(view.AX, view.Area.AutoscrollAX), AY = Advance(view.AY, view.Area.AutoscrollAY), BX = Advance(view.BX, view.Area.AutoscrollBX), BY = Advance(view.BY, view.Area.AutoscrollBY), HideWindows = hide };
    }

    private static LogicalView Speeds(LogicalView view, int speed) => view with
    {
        AX = view.Area.AutoscrollAX != 0 ? view.AX : view.AX with { Speed = speed * view.Area.ParallaxAX >> 8 },
        AY = view.Area.AutoscrollAY != 0 ? view.AY : view.AY with { Speed = speed * view.Area.ParallaxAY >> 8 },
        BX = view.Area.AutoscrollBX != 0 ? view.BX : view.BX with { Speed = speed * view.Area.ParallaxBX >> 8 },
        BY = view.Area.AutoscrollBY != 0 ? view.BY : view.BY with { Speed = speed * view.Area.ParallaxBY >> 8 },
    };

    private static int Follow(int target, int view, int minimum, int maximum) =>
        target < view + 1536 ? (view > minimum ? view - 384 : view) :
        target > view + 2304 && view < maximum ? view + 384 : view;

    internal static LogicalView Autoscroll(LogicalView view, SetPlaneAutoscroll command)
    {
        int old = command.Axis switch { 0 => view.Area.AutoscrollAX, 1 => view.Area.AutoscrollAY,
            2 => view.Area.AutoscrollBX, 3 => view.Area.AutoscrollBY, _ => throw new BattleRuleException("plane-axis", "program.view") };
        int value = unchecked((sbyte)(command.Value + (command.Relative ? old : 0)));
        var area = command.Axis switch { 0 => view.Area with { AutoscrollAX = value }, 1 => view.Area with { AutoscrollAY = value },
            2 => view.Area with { AutoscrollBX = value }, _ => view.Area with { AutoscrollBY = value } };
        Validate(area);
        return view with { Area = area };
    }

    internal static LogicalView Position(LogicalView view, SetPlanePosition command)
    {
        if (command.Value is < 0 or >= 24576) throw new BattleRuleException("plane-position", "program.view");
        return command.Axis switch { 0 => view with { AX = view.AX with { Position = command.Value } },
            1 => view with { AY = view.AY with { Position = command.Value } },
            2 => view with { BX = view.BX with { Position = command.Value } },
            3 => view with { BY = view.BY with { Position = command.Value } },
            _ => throw new BattleRuleException("plane-axis", "program.view") };
    }

    private static LogicalViewAxis Advance(LogicalViewAxis axis, int auto)
    {
        // VInt_UpdateScrollingData consumes the prior destination before scheduling the
        // next signed-byte step. Continuous scrolling wraps over the 64-cell plane.
        var moved = Scroll(axis);
        if (auto == 0) return moved;
        int value = unchecked((sbyte)auto);
        int position = (moved.Position % 24576 + 24576) % 24576;
        return moved with { Position = position, Destination = position + value, Speed = Math.Abs(value) };
    }

    private static LogicalViewAxis Scroll(LogicalViewAxis axis)
    {
        if (axis.Destination is not { } destination) return axis;
        if (axis.Speed <= 0) throw new BattleRuleException("field-view-scroll-speed", "story.view", true);
        int distance = destination - axis.Position;
        return Math.Abs(distance) <= axis.Speed ? axis with { Position = destination, Destination = null } :
            axis with { Position = axis.Position + Math.Sign(distance) * axis.Speed };
    }
}

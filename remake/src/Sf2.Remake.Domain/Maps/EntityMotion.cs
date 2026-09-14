namespace Sf2.Remake.Domain.Maps;

// Original fixed-point entity movement. A tile is 384 units; presentation reads this state.
public sealed record EntityMotionState(short X, short Y, short XDestination, short YDestination,
    short XVelocity, short YVelocity, ushort XTravel, ushort YTravel,
    ushort XSpeed, ushort YSpeed, byte XAcceleration, byte YAcceleration,
    byte FlagsA, byte FlagsB, byte Facing, byte Layer, byte AnimationCounter, byte WaitTimer)
{
    public bool IsMoving => X != XDestination || Y != YDestination;
    public static EntityMotionState At(MapPosition position, byte facing, ushort speed) =>
        new((short)(position.X * 384), (short)(position.Y * 384),
            (short)(position.X * 384), (short)(position.Y * 384), 0, 0, 0, 0,
            speed, speed, 0, 0, 0, 0x40, facing, 0, 0, 0);
}

internal static class EntityMotion
{
    private static readonly int[] FacingTable = [5, 2, 6, -1, 1, -1, 3, -1, 4, 0, 7, -1, -1, -1, -1, -1];

    // esc03_follow measures current separation, then rotates offsets around the leader's destination.
    internal static EntityMotionState Follow(EntityMotionState state, EntityMotionState leader, int offsetX, int offsetY,
        Func<int, int, bool> passable)
    {
        if (Math.Max(Math.Abs(Signed(state.X - leader.X)), Math.Abs(Signed(state.Y - leader.Y))) <= 0x1E0) return state;
        (short X, short Y) Target(int x, int y)
        {
            x *= 16; y *= 16;
            var delta = leader.Facing switch
            {
                0 => (x, y), 1 => (y, -x), 2 => (-x, -y), 3 => (-y, x),
                4 => (x + y, -x - y), 5 => (-x - y, -x - y),
                6 => (-x - y, x + y), _ => (x - y, x - y),
            };
            return (Signed(leader.XDestination + delta.Item1), Signed(leader.YDestination + delta.Item2));
        }
        var target = Target(offsetX, offsetY);
        if (!passable(target.X / 384, target.Y / 384))
        {
            target = Target(offsetX, 0);
            if (!passable(target.X / 384, target.Y / 384)) target = (leader.XDestination, leader.YDestination);
        }
        return state with { XDestination = target.X, YDestination = target.Y,
            XTravel = (ushort)Math.Abs(Signed(target.X - state.X)), YTravel = (ushort)Math.Abs(Signed(target.Y - state.Y)),
            XVelocity = Signed(target.X == state.X ? 0 : target.X > state.X ? state.XSpeed : -state.XSpeed),
            YVelocity = Signed(target.Y == state.Y ? 0 : target.Y > state.Y ? state.YSpeed : -state.YSpeed) };
    }

    // The script installs motion after this tick's movement pass. Obstruction retains its cursor.
    internal static EntityMotionState? Start(EntityMotionState state, short x, short y,
        IEnumerable<EntityMotionState> others, bool fieldInput = false)
    {
        if ((state.FlagsA & 0x20) != 0 && (fieldInput ? FieldObstructed(x, y, others) : others.Any(other =>
            Math.Abs(other.XDestination - x) + Math.Abs(other.YDestination - y) < 384))) return null;
        ushort travelX = (ushort)Math.Abs(Signed(x - state.X));
        ushort travelY = (ushort)Math.Abs(Signed(y - state.Y));
        return state with
        {
            XDestination = x, YDestination = y, XTravel = travelX, YTravel = travelY,
            XVelocity = Signed(travelX == 0 ? 0 : x >= state.X ? state.XSpeed : -state.XSpeed),
            YVelocity = Signed(travelY == 0 ? 0 : y >= state.Y ? state.YSpeed : -state.YSpeed), WaitTimer = 0,
        };
    }

    // esc02_input checks obstructable entities at both their current and reserved positions.
    internal static bool FieldObstructed(int x, int y, IEnumerable<EntityMotionState> others) =>
        others.Any(other => (other.FlagsA & 0x80) != 0 &&
            ((Math.Abs(other.X - x) < 256 && Math.Abs(other.Y - y) < 256) ||
             (Math.Abs(other.XDestination - x) < 256 && Math.Abs(other.YDestination - y) < 256)));

    internal static EntityMotionState Tick(EntityMotionState state, ushort? destinationWord)
    {
        var next = state;
        short dx = Signed(state.XDestination - state.X), dy = Signed(state.YDestination - state.Y);
        if (dx != 0 || dy != 0)
        {
            short vx = state.XVelocity, vy = state.YVelocity;
            if (dx != 0) vx = Adjust(vx, Acceleration(state.X, state.XDestination, state.XTravel,
                state.XAcceleration, (state.FlagsA & 1) != 0, (state.FlagsA & 4) != 0), state.X < state.XDestination);
            if (dy != 0) vy = Adjust(vy, Acceleration(state.Y, state.YDestination, state.YTravel,
                state.YAcceleration, (state.FlagsA & 2) != 0, (state.FlagsA & 8) != 0), state.Y < state.YDestination);
            short x = state.XTravel == 0 ? state.X : Signed(state.X + vx);
            short y = state.YTravel == 0 ? state.Y : Signed(state.Y + vy);
            int mx = state.XTravel == 0 ? 0 : Math.Abs((int)vx);
            int my = state.YTravel == 0 ? 0 : Math.Abs((int)vy);
            int sx = state.XTravel == 0 ? 0 : vx < 0 ? -1 : 1;
            int sy = state.YTravel == 0 ? 0 : vy < 0 ? -1 : 1;
            if (my - mx < -8) sy = 0;
            if (my - mx > 8) sx = 0;
            int facing = FacingTable[((sx + 1) << 2) + sy + 1];
            ushort tx = state.XTravel, ty = state.YTravel;
            if (Crossed(dx, Signed(state.XDestination - x))) { x = state.XDestination; tx = 0; }
            if (Crossed(dy, Signed(state.YDestination - y))) { y = state.YDestination; ty = 0; }
            byte layer = state.Layer, flags = state.FlagsB;
            if (tx == 0 && ty == 0 && destinationWord is { } word)
            {
                int marker = word & 0x3C00;
                if (marker == 0x2000) layer = 2;
                if (marker == 0x2400) layer = 0;
                flags = marker == 0x3400 ? (byte)(flags | 0x20) : (byte)(flags & ~0x20);
            }
            next = state with
            {
                X = x, Y = y, XVelocity = vx, YVelocity = vy, XTravel = tx, YTravel = ty,
                Facing = facing >= 0 && (state.FlagsB & 0x40) != 0 ? (byte)facing : state.Facing,
                Layer = layer, FlagsB = flags,
                AnimationCounter = state.AnimationCounter == 255 ? (byte)255 :
                    unchecked((byte)(state.AnimationCounter + ((mx + my) >> 5))),
            };
        }
        return next.AnimationCounter is > 30 and < 128 ? next with { AnimationCounter = 0 } : next;
    }

    private static int Acceleration(short position, short destination, ushort travel, byte amount,
        bool accelerate, bool decelerate)
    {
        int remaining = Math.Abs(Signed(position - destination));
        int threeQuarters = unchecked((ushort)(unchecked((ushort)(travel * 4)) - travel)) >> 2;
        int result = accelerate && remaining >= threeQuarters ? amount : 0;
        return decelerate && remaining < (travel >> 2) ? -amount : result;
    }
    private static short Adjust(short velocity, int acceleration, bool increasing)
    {
        short candidate = Signed(velocity + (increasing ? acceleration : -acceleration));
        return candidate == 0 ? velocity : candidate;
    }
    private static bool Crossed(short before, short after) => after == 0 || ((before ^ after) & 0x8000) != 0;
    private static short Signed(int value) => unchecked((short)value);
}

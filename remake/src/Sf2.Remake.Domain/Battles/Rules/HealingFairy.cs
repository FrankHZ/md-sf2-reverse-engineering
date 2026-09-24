namespace Sf2.Remake.Domain.Battles;

// Semantic counterparts of the two property slots per fairy. X/Y retain the source
// sprite coordinate origin; the adapter subtracts 128 only when projecting them.
public sealed record HealingFairyInstance(ushort Age, ushort Phase, ushort Angle, ushort Speed,
    short XFraction, short YFraction, byte WingClock, byte WingFrame, ushort DustClock,
    ushort X, ushort Y, int BodyFrame = 1, bool Mirrored = false)
{
    public bool Active => Age != 0;
}
public sealed record HealingDust(ushort Age, ushort Frame, ushort Clock, ushort X, ushort Y);
public sealed record HealingFairyState(ushort Lifetime, byte Control,
    IReadOnlyList<HealingFairyInstance> Fairies, IReadOnlyList<HealingDust> Dust,
    ushort PendingDustX = 0, ushort PendingDustY = 0, bool CleanupPending = false)
{
    public int ActiveCount => Fairies.Count(fairy => fairy.Active);
}
internal sealed record HealingFairyUpdate(HealingFairyState State, uint Seed, IReadOnlyList<BattleEffect> Effects);

internal static class HealingFairy
{
    // Integer quarter wave (scale 256), bound by sub_179C/table_1840. All subsequent
    // motion is integer word arithmetic, including signed remainders toward zero.
    private static ReadOnlySpan<int> QuarterWave => [0,6,12,18,25,31,37,43,49,56,62,68,74,80,86,92,
        97,103,109,115,120,126,131,136,142,147,152,157,162,167,171,176,181,185,189,193,
        197,201,205,209,212,216,219,222,225,228,231,234,236,238,241,243,244,246,248,249,
        251,252,253,254,254,255,255,255,256];

    internal static HealingFairyUpdate Begin(int level, uint seed, ActorRef actor)
    {
        if (level is < 1 or > 3) throw new BattleRuleException("healing-animation", "spell.level", true);
        List<BattleEffect> effects = [];
        var fairies = new HealingFairyInstance[level == 3 ? 2 : 1];
        for (int i = 0; i < fairies.Length; i++)
        {
            ushort y = (ushort)(128 + Draw(32, "setup-y", actor, ref seed, effects));
            short delay = (short)(1 + Draw(30, "setup-delay", actor, ref seed, effects));
            ushort dust = (ushort)(1 + Draw(12, "setup-dust", actor, ref seed, effects));
            fairies[i] = new(1, 7, 0, 0, delay, 0, 0, 0, dust, 384, y);
        }
        return new(new(ushort.MaxValue, 1, Array.AsReadOnly(fairies), Array.AsReadOnly(
            Enumerable.Repeat(new HealingDust(0, 0, 0, 0, 0), 25 - 2 * fairies.Length).ToArray())), seed, effects.AsReadOnly());
    }

    internal static HealingFairyState RequestStop(HealingFairyState state) => state with { Control = 2 };

    internal static HealingFairyUpdate Advance(HealingFairyState state, uint seed, ActorRef actor)
    {
        if (state.Control == 0 || state.ActiveCount == 0) return new(state, seed, []);
        if (state.Control > 2) return new(Clear(state), seed, []);
        ushort lifetime = state.Control == 2 ? (ushort)0 : state.Lifetime;
        if (lifetime != 0) lifetime--;
        var fairies = state.Fairies.ToArray();
        var dust = state.Dust.ToArray();
        ushort pendingX = state.PendingDustX, pendingY = state.PendingDustY;
        List<BattleEffect> effects = [];
        for (int i = 0; i < fairies.Length; i++)
        {
            var fairy = fairies[i];
            if (!fairy.Active) continue;
            fairy = fairy with { Age = unchecked((ushort)(fairy.Age + 1)) };
            if ((fairy.Phase & 3) == 3)
            {
                if (lifetime == 0) fairy = fairy with { Age = 0 };
                else
                {
                    short delay = unchecked((short)(fairy.XFraction - 1));
                    fairy = fairy with { XFraction = delay };
                    if (delay == 0) fairy = fairy with { Age = 2, Phase = (ushort)((fairy.Phase + 1) & 7),
                        Angle = (ushort)((230 + Draw(16, "reentry", actor, ref seed, effects)) << 4),
                        Speed = 240, XFraction = 0, YFraction = 0 };
                }
                fairies[i] = fairy;
                continue;
            }
            if (fairy.Age is 44 or 72) fairy = fairy with {
                Phase = unchecked((ushort)(fairy.Phase + 1)), BodyFrame = fairy.Age == 44 ? 0 : 1,
                Mirrored = ((fairy.Phase + 1) & 4) != 0 };
            ushort angle = fairy.Angle;
            short speed = unchecked((short)(fairy.Speed + ((fairy.Phase & 1) == 0 ? 20 : -20)));
            if ((fairy.Phase & 1) == 0) angle = (ushort)((angle + 6) & 4095);
            else if (speed < 0) speed = 0;
            var (horizontal, vertical) = Direction(angle >> 4);
            var (dx, fractionX) = Motion(horizontal, speed, fairy.XFraction);
            var (dy, fractionY) = Motion(vertical, speed, fairy.YFraction);
            if ((fairy.Phase & 4) == 0) dx = -dx;
            byte wingClock = unchecked((byte)(fairy.WingClock + 1)), wingFrame = fairy.WingFrame;
            if (wingClock >= 4) { wingClock = 0; wingFrame ^= 1; }
            fairy = fairy with { Angle = angle, Speed = unchecked((ushort)speed), XFraction = fractionX,
                YFraction = fractionY, X = unchecked((ushort)(fairy.X + dx)), Y = unchecked((ushort)(fairy.Y + dy)),
                WingClock = wingClock, WingFrame = wingFrame };
            if (fairy.X is < 96 or > 384)
            {
                ushort phase = unchecked((ushort)(fairy.Phase + 1));
                short delay = (short)(1 + Draw(28, "boundary-delay", actor, ref seed, effects));
                ushort y = (ushort)(128 + Draw(32, "boundary-y", actor, ref seed, effects));
                fairy = fairy with { Phase = phase, XFraction = delay, X = (ushort)((phase & 4) == 0 ? 96 : 384), Y = y,
                    Mirrored = !fairy.Mirrored };
            }
            ushort clock = unchecked((ushort)(fairy.DustClock - 1));
            if (clock == 0)
            {
                clock = (ushort)(3 + Draw(12, "dust", actor, ref seed, effects));
                pendingX = fairy.X; pendingY = fairy.Y;
            }
            fairies[i] = fairy with { DustClock = clock };
        }
        // Inclusive DBF over the remaining property slots. A later fairy may replace
        // the one pending dust position; spawning/updating dust consumes no RNG.
        for (int i = 0; i < dust.Length; i++)
        {
            var particle = dust[i];
            if (particle.Age == 0)
            {
                if (pendingY != 0)
                {
                    dust[i] = new(1, 0, 6, unchecked((ushort)(pendingX + 12)), unchecked((ushort)(pendingY + 12)));
                    pendingX = pendingY = 0;
                }
                continue;
            }
            particle = particle with { Age = unchecked((ushort)(particle.Age + 1)),
                Y = unchecked((ushort)(particle.Y + 1)), Clock = unchecked((ushort)(particle.Clock - 1)) };
            if (particle.Clock == 0)
                particle = particle.Frame == 4 ? new(0, 0, 6, 1, 1)
                    : particle with { Clock = 6, Frame = (ushort)(particle.Frame + 1) };
            dust[i] = particle;
        }
        var next = state with { Lifetime = lifetime, Fairies = Array.AsReadOnly(fairies),
            Dust = Array.AsReadOnly(dust), PendingDustX = pendingX, PendingDustY = pendingY };
        // Cleanup clears before its nested WaitForVInt. The caller resumes it on a
        // distinct logical opportunity, with the cleared spell service already gated.
        if (next.ActiveCount == 0) next = Clear(next);
        return new(next, seed, effects.AsReadOnly());
    }

    internal static HealingFairyState FinishCleanup(HealingFairyState state) => state with { CleanupPending = false };
    private static HealingFairyState Clear(HealingFairyState state) => state with {
        Lifetime = 0, Control = 0, Fairies = Array.AsReadOnly(state.Fairies.Select(f =>
            f with { Age = 0, Phase = 0, Angle = 0, Speed = 0, XFraction = 0, YFraction = 0,
                WingClock = 0, WingFrame = 0, DustClock = 0 }).ToArray()),
        Dust = Array.AsReadOnly(state.Dust.Select(_ => new HealingDust(0, 0, 0, 0, 0)).ToArray()),
        PendingDustX = 0, PendingDustY = 0, CleanupPending = true };

    internal static (int Horizontal, int Vertical) Direction(int angle)
    {
        int offset = angle & 63;
        int low = QuarterWave[offset], high = QuarterWave[64 - offset];
        return ((angle & 255) >> 6) switch { 0 => (high, -low), 1 => (-low, -high),
            2 => (-high, low), _ => (low, high) };
    }
    private static (int Whole, short Fraction) Motion(int direction, short speed, short residual)
    {
        short value = unchecked((short)(((direction * speed) >> 8) + residual));
        return (value / 256, (short)(value % 256));
    }
    private static ushort Draw(ushort range, string purpose, ActorRef actor, ref uint seed, List<BattleEffect> effects)
    {
        var roll = BattleRandom.NextMain(seed, range); seed = roll.After;
        effects.Add(new("rng-fairy-" + purpose, actor, roll.Before, roll.After, range, roll.Value));
        return roll.Value;
    }
}

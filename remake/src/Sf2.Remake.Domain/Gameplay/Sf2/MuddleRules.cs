using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class MuddleRules
{
    internal const ushort CounterMask = 0x0030;
    private const ushort ConfusionFlag = 0x0008;
    internal const ushort Mask = CounterMask | ConfusionFlag;
    internal static bool Present(ushort status) => (status & CounterMask) != 0;
    internal static bool Confused(ushort status) => Present(status) && (status & ConfusionFlag) != 0;
    internal static bool Valid(ushort status) => (status & ConfusionFlag) == 0 || Present(status);

    internal static (ushort After, MainRandomDraw Draw)? AfterTurn(BattleActorState actor, uint seed)
    {
        ushort count = (ushort)(actor.Status & CounterMask);
        if (actor.Hp == 0 || count == 0) return null;
        if (!SilenceRules.SupportedInBattle(actor.Status))
            throw new BattleRuleException("after-turn-status", "actor.status", true);
        var draw = BattleRandom.NextMain(seed, count);
        ushort after = (draw.Value & CounterMask) == 0 ? (ushort)(actor.Status & ~Mask) : (ushort)(actor.Status - 0x10);
        return (after, draw);
    }
}

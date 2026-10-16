using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed record SleepTurnResult(ushort Before, ushort After, int Text, MainRandomDraw Draw);

// ProcessAfterTurnEffects' sleep branch only. No actor, queue or session publication occurs here.
internal static class SleepTurnRules
{
    internal static SleepTurnResult? Prepare(BattleActorState actor, uint seed)
    {
        if (actor.Hp == 0 || (actor.Status & 0xC0) == 0) return null;
        if ((actor.Status & ~0xC0) != 0)
            throw new BattleRuleException("after-turn-status", "actor.status", true);
        ushort counter = (ushort)(actor.Status & 0xC0);
        var draw = BattleRandom.NextMain(seed, counter);
        bool wake = (draw.Value & 0xC0) == 0;
        ushort after = (ushort)((actor.Status & ~0xC0) | (wake ? 0 : counter - 0x40));
        return new(actor.Status, after, wake ? 354 : 306, draw);
    }
}

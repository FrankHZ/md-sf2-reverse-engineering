namespace Sf2.Remake.Domain.Battles;

internal readonly record struct PriestHealingInput(
    ushort TargetHp, ushort TargetMaxHp, byte ActorMp, byte ActorExp,
    int AdjustedPower, byte MpCost, uint MainSeed);

internal readonly record struct HealingResolution(
    int Recovery, int AccumulatedExp, int AwardedExp,
    byte MpAfter, byte ExpAfter, ushort HpAfter,
    MainRandomDraw PlusRoll, MainRandomDraw MinusRoll)
{
    internal uint MainAfter => MinusRoll.After;
}

internal static class HealingRules
{
    internal static int Experience(int recovery, ushort maximumHp) =>
        Math.Min(25, Math.Max(10, 25 * recovery / maximumHp));

    // Ordinary same-side healing after class/power selection, for an EXP-eligible priest.
    // This scalar rule does not select a spell, target, turn, class, or reference history.
    internal static HealingResolution ResolvePriest(PriestHealingInput input)
    {
        if (input.TargetHp == 0 || input.TargetMaxHp == 0 || input.TargetHp > input.TargetMaxHp)
            throw new ArgumentException("Healing requires a living target within its maximum HP.", nameof(input.TargetHp));
        if (input.AdjustedPower is < 0 or > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(input.AdjustedPower));
        if (input.ActorExp > 200)
            throw new ArgumentOutOfRangeException(nameof(input.ActorExp));
        if (input.ActorMp < input.MpCost)
            throw new ArgumentException("The caster has insufficient MP.", nameof(input.ActorMp));

        int recovery = Math.Min(input.AdjustedPower, input.TargetMaxHp - input.TargetHp);
        int accumulated = Experience(recovery, input.TargetMaxHp);
        var plus = BattleRandom.NextMain(input.MainSeed, 16);
        var minus = BattleRandom.NextMain(plus.After, 16);
        int award = Math.Max(1, accumulated + (plus.Value == 0 ? 1 : 0) - (minus.Value == 0 ? 1 : 0));
        return new(recovery, accumulated, award, (byte)(input.ActorMp - input.MpCost),
            (byte)Math.Min(200, input.ActorExp + award), (ushort)(input.TargetHp + recovery), plus, minus);
    }
}

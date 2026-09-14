using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class HealingRulesTests
{
    // HEAL contract and independent H3 values, plus an authored different-stat input.
    [Theory]
    [InlineData(95, 100, 20, 0, 0x12341234u, 5, 10, 9, 17, 100, 9, 0x04B61234u)]
    [InlineData(8, 20, 12, 7, 0x1234ABCDu, 12, 15, 14, 9, 20, 21, 0x04B6ABCDu)]
    [InlineData(3, 12, 10, 0, 0x74A71234u, 9, 18, 17, 7, 12, 17, 0x02A11234u)]
    public void OrdinaryPriestHealingCapsRecoveryAndAwardsWithoutAnyBattleHistory(
        int hp, int maxHp, int mp, int exp, uint seed, int recovery, int accumulated,
        int award, int mpAfter, int hpAfter, int expAfter, uint seedAfter)
    {
        var input = new PriestHealingInput((ushort)hp, (ushort)maxHp, (byte)mp, (byte)exp, 15, 3, seed);
        var result = HealingRules.ResolvePriest(input);
        Assert.Equal((recovery, accumulated, award), (result.Recovery, result.AccumulatedExp, result.AwardedExp));
        Assert.Equal((mpAfter, hpAfter, expAfter), ((int)result.MpAfter, (int)result.HpAfter, (int)result.ExpAfter));
        Assert.Equal(seedAfter, result.MainAfter);
        Assert.Equal((ushort)14, result.PlusRoll.Value);
        Assert.Equal((ushort)0, result.MinusRoll.Value);
        Assert.Equal(result.PlusRoll.After, result.MinusRoll.Before);
        Assert.Equal(seed, result.PlusRoll.Before);
        Assert.Equal((ushort)16, result.PlusRoll.Range);
        Assert.Equal((ushort)16, result.MinusRoll.Range);
        Assert.Equal((ushort)hp, input.TargetHp);
        Assert.Equal((byte)mp, input.ActorMp);
        Assert.Equal(seed, input.MainSeed);
    }

    [Fact]
    public void AlreadyAdjustedOrdinaryPowerIsCappedBeforeProportionalExperience()
    {
        var result = HealingRules.ResolvePriest(new(10, 50, 12, 0, 37, 3, 0x12341234));
        Assert.Equal(37, result.Recovery);
        Assert.Equal(18, result.AccumulatedExp);
        Assert.Equal(47, result.HpAfter);
    }

    [Fact]
    public void NoMissingHpStillUsesTheHealingMinimumAtThisScalarBoundary()
    {
        var result = HealingRules.ResolvePriest(new(100, 100, 20, 0, 15, 3, 0x12341234));
        Assert.Equal(0, result.Recovery);
        Assert.Equal(10, result.AccumulatedExp);
        Assert.Equal(17, result.MpAfter);
        Assert.Equal(9, result.AwardedExp);
    }

    [Theory]
    [InlineData(0, 100, 20, "TargetHp")]
    [InlineData(101, 100, 20, "TargetHp")]
    [InlineData(5, 10, 2, "ActorMp")]
    public void InvalidTargetOrInsufficientMpCannotPublishAResolution(int hp, int maxHp, int mp, string field)
    {
        var input = new PriestHealingInput((ushort)hp, (ushort)maxHp, (byte)mp, 0, 15, 3, 0x12341234);
        var error = Assert.Throws<ArgumentException>(() => HealingRules.ResolvePriest(input));
        Assert.Equal(field, error.ParamName);
        Assert.Equal(0x12341234u, input.MainSeed);
        Assert.Equal((byte)mp, input.ActorMp);
    }

    [Fact]
    public void HealingAwardRetainsThresholdAndSaturatesBeforeTheGrowthConsumer()
    {
        var input = new PriestHealingInput(95, 100, 20, 91, 15, 3, 0x12341234);
        var result = HealingRules.ResolvePriest(input);
        Assert.Equal(100, result.ExpAfter);
        Assert.Equal(100, result.HpAfter);
        Assert.Equal(17, result.MpAfter);
        var capped = HealingRules.ResolvePriest(input with { ActorExp = 199 });
        Assert.Equal(200, capped.ExpAfter);
        Assert.Equal(new PriestHealingInput(95, 100, 20, 91, 15, 3, 0x12341234), input);
    }
}

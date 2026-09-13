using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleRandomTests
{
    // Independent original observations: tests/fixtures/h3/rng-v1.json.
    [Theory]
    [InlineData(0, 7, 0)]
    [InlineData(1, 20, 0)]
    [InlineData(0x1234, 60587, 118)]
    [InlineData(0x8000, 32775, 64)]
    [InlineData(0xFFFF, 65530, 127)]
    [InlineData(0xBEEF, 45610, 89)]
    [InlineData(0x5555, 21848, 42)]
    public void MainGeneratorMatchesIndependentWordObservations(int seed, int after, int value)
    {
        var draw = BattleRandom.NextWord((ushort)seed, 128);
        Assert.Equal(new WordRandomDraw((ushort)seed, (ushort)after, (ushort)value), draw);
    }

    [Fact]
    public void ZeroRangeStillAdvancesAndMainDrawPreservesTheIndependentLowWord()
    {
        var first = BattleRandom.NextMain(0x0000ABCD, 0);
        var second = BattleRandom.NextMain(first.After, 0);
        Assert.Equal(new MainRandomDraw(0, 0x0000ABCD, 0x0007ABCD, 0), first);
        Assert.Equal(new MainRandomDraw(0, 0x0007ABCD, 0x0062ABCD, 0), second);
    }

    [Fact]
    public void RangeDoublingRetainsItsSourceWordWidth()
    {
        var draw = BattleRandom.NextWord(0xFFFF, 0x8000);
        Assert.Equal((ushort)65530, draw.After);
        Assert.Equal((ushort)0, draw.Value);
    }
}

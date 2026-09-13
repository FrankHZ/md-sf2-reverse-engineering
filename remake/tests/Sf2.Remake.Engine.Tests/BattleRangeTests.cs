using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleRangeTests
{
    [Theory]
    [InlineData(8, 8, 0, 1, true)]
    [InlineData(8, 8, 1, 1, false)]
    [InlineData(9, 8, 0, 1, true)]
    [InlineData(9, 9, 0, 1, false)]
    [InlineData(7, 7, 1, 2, true)]
    [InlineData(8, 11, 1, 2, false)]
    public void ConfiguredManhattanRangeIncludesBothBoundaries(int x, int y, int min, int max, bool expected) =>
        Assert.Equal(expected, BattleRange.Contains(new(8, 8), new(x, y), min, max));

    [Fact]
    public void InvalidRangeDefinitionIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BattleRange.Contains(new(8, 8), new(9, 8), -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => BattleRange.Contains(new(8, 8), new(9, 8), 2, 1));
    }
}

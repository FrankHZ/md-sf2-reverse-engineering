using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class OpeningPartyInitializationTests
{
    [Theory]
    [InlineData(23, false)] [InlineData(24, true)] [InlineData(30, true)]
    public void InitialSpellThresholdChangesDoubleSettingAndKeepsCriticalAndCounter(byte startLevel, bool changed)
    {
        var growth = new BattleGrowthDefinition(4, 0, [], [new(24, 128)], new Dictionary<byte, SpellRef>());
        var result = OpeningPartyInitialization.InitialCritical(growth, startLevel, PhysicalCriticalRule.OneIn16WithQuarterBonus);
        Assert.Equal(16, result.ChanceDenominator);
        Assert.Equal(2, result.DamageBonusShift);
        Assert.Equal(changed ? 16 : 32, result.DoubleChanceDenominator);
        Assert.Equal(32, result.CounterChanceDenominator);
    }
}

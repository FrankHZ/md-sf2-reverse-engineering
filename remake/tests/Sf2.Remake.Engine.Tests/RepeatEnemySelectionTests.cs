using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class RepeatEnemySelectionTests
{
    [Theory]
    [InlineData(28, 11, 0)]
    [InlineData(9, 12, 1)]
    [InlineData(38, 0, 1)]
    [InlineData(39, 0, 2)]
    [InlineData(9, 255, 0)]
    [InlineData(255, 12, 25)]
    public void CompletedBattleUsesEffectiveLevelAndSignedClass(int level, int classId, int quotient) =>
        Assert.Equal(quotient, RepeatEnemySelection.Quotient(level, classId, 29));

    // USA table bytes at 1B6DBC..1B6DD9, including the airborne overread.
    [Theory]
    [InlineData(0, 0x1357, 12, 79, 70, 11, 0x1357)]
    [InlineData(1, 0x1357, 17, 82, 70, 12, 0x531A)]
    [InlineData(1, 0x4AB0, 16, 82, 70, 11, 0x0A3A)]
    [InlineData(2, 0x4AB0, 21, 83, 72, 18, 0x6A18)]
    [InlineData(6, 0xF111, 12, 79, 70, 11, 0x9ABD)]
    [InlineData(25, 0xF111, 12, 79, 70, 11, 0x9ABD)]
    public void EachSlotConsumesItsOwnRollsBeforeTheNext(int quotient, uint word, byte orc, byte priest,
        byte madam, byte soulsower, uint after)
    {
        uint main = (word << 16) | 0xBEEF;
        byte Draw(byte id, BattleMover mover) => RepeatEnemySelection.Select(id, mover, quotient, ref main);
        Assert.Equal(orc, Draw(12, BattleMover.Regular));
        Assert.Equal(43, Draw(43, BattleMover.Flying));
        Assert.Equal(priest, Draw(79, BattleMover.Healer));
        Assert.Equal(madam, Draw(70, BattleMover.Mage));
        Assert.Equal(60, Draw(60, BattleMover.BrassGunner));
        Assert.Equal(soulsower, Draw(11, BattleMover.Regular));
        Assert.Equal((after << 16) | 0xBEEF, main);
    }
}

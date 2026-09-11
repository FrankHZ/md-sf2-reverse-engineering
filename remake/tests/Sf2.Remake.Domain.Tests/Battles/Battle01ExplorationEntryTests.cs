using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01ExplorationEntryTests
{
    [Fact]
    public void StepWordSubtractionMatchesSigned68000BgeIncludingOverflow()
    {
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            ushort result = unchecked((ushort)(word - 20000));
            bool negative = (result & 0x8000) != 0;
            bool overflow = ((word ^ 20000) & (word ^ result) & 0x8000) != 0;
            ushort expected = negative == overflow ? result : (ushort)0;
            Assert.Equal(expected, Battle01ExplorationEntry.TransformStepCounter((ushort)word));
        }
    }

    [Fact]
    public void HealingVisitsAllThirtySlotsAndHealsDeadImmortalsWithoutRevivingOrdinaryDead()
    {
        var before = Enumerable.Range(0, 30).Select(i => Slot((byte)i)).ToArray();
        before[0] = Slot(0, hp: 12, max: 12, mp: 1, mpMax: 8);
        before[1] = Slot(1, hp: 3, max: 11, mp: 2, mpMax: 10);
        before[2] = Slot(2, hp: 0, max: 11, mp: 1, mpMax: 5);
        before[7] = Slot(7, hp: 0, max: 9, mp: 0, mpMax: 3);
        before[28] = Slot(28, hp: 0, max: 8, mp: 1, mpMax: 4);
        var (after, processed) = Battle01ExplorationEntry.HealSlots(before);
        Assert.Equal(new byte[] { 0, 1, 7, 28 }, processed);
        foreach (int i in processed)
        {
            Assert.Equal(before[i].HpMax, after[i].HpCurrent);
            Assert.Equal(before[i].MpMax, after[i].MpCurrent);
            Assert.Equal(before[i].CurrentStats, after[i].CurrentStats);
        }
        foreach (int i in Enumerable.Range(0, 30).Except(processed.Select(i => (int)i))) Assert.Same(before[i], after[i]);
        Assert.Equal((ushort)0, before[7].HpCurrent); Assert.Equal((byte)1, before[0].MpCurrent);
        Assert.Same(before[2], after[2]);
        Assert.False(before[7].IsNeutralDormant); // Synthetic loop coverage does not widen production admission.
    }

    [Fact]
    public void ExplicitDormantImmortalsAreProcessedEvenWhenTheirMaximaAreZero()
    {
        var before = Enumerable.Range(0, 30).Select(i => Slot((byte)i)).ToArray();
        var (after, processed) = Battle01ExplorationEntry.HealSlots(before);
        Assert.Equal(new byte[] { 7, 28 }, processed);
        Assert.All(Enumerable.Range(3, 27), i => Assert.True(before[i].IsNeutralDormant));
        Assert.All(Enumerable.Range(0, 30), i => Assert.Same(before[i], after[i]));
    }

    [Fact]
    public void UnsupportedStatusRefreshAndIncompleteSlotImagesRejectWithoutChangingInputs()
    {
        var slots = Enumerable.Range(0, 30).Select(i => Slot((byte)i)).ToArray();
        slots[0] = Slot(0, hp: 1, max: 12, status: 2);
        Assert.Throws<ArgumentException>(() => Battle01ExplorationEntry.HealSlots(slots));
        Assert.Equal((ushort)1, slots[0].HpCurrent); Assert.Equal((ushort)2, slots[0].Status);
        Assert.Throws<ArgumentException>(() => Battle01ExplorationEntry.HealSlots(slots[..29]));
        slots[1] = slots[0];
        Assert.Throws<ArgumentException>(() => Battle01ExplorationEntry.HealSlots(slots));
    }

    private static Battle01EntrySlot Slot(byte id, ushort hp = 0, ushort max = 0, byte mp = 0,
        byte mpMax = 0, ushort status = 0) => new(id, 0, 0, max, hp, mpMax, mp, status,
        new(0, 0, 0, 0, 0, 0), new(0, 0, 0, 0, 0, 0), [127, 127, 127, 127], [63, 63, 63, 63]);
}

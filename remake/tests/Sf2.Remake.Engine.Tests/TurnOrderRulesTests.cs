using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class TurnOrderRulesTests
{
    [Fact]
    public void SignedOrderSecondTurnsAndSentinelsRetainTheIndependentBoundaryObservation()
    {
        // Raw AGI/dead/unplaced setup and expected scores: turn-order-boundaries-v1.json.
        TurnOrderCandidate[] candidates = [
            new(0, true, 10, 128), new(1, true, 10, 127), new(2, true, 0, 8),
            new(128, false, 5, 5), new(129, true, 5, 5), new(130, true, 5, 5),
            new(131, true, 5, 5), new(132, true, 5, 5), new(133, true, 5, 5)];
        var result = TurnOrderRules.Generate(candidates.Reverse(), 0);
        Assert.Equal(new TurnOrderEntry[] {
            new(130, 6), new(129, 5), new(132, 5), new(133, 5), new(131, 4),
            new(0, 0), new(0, 255), new(1, 135)
        }, result.Slots.Where(slot => slot.ActorSlot != 255));
        Assert.Equal(new TurnOrderEntry(0, 255), result.Slots[6]);
        Assert.All(result.Slots.Skip(7).Take(56), slot => Assert.Equal(new TurnOrderEntry(255, 255), slot));
        Assert.Equal(new TurnOrderEntry(1, 135), result.Slots[63]);
        Assert.Equal((byte)0, candidates[0].ActorSlot);
    }

    [Fact]
    public void DeadAndUnplacedActorsDoNotConsumeRandomDraws()
    {
        TurnOrderCandidate[] living = [new(9, true, 1, 4), new(140, true, 5, 4)];
        var result = TurnOrderRules.Generate(living, 0x1234);
        var withExcluded = TurnOrderRules.Generate(
            [new(5, true, 0, 128), living[1], new(129, false, 9, 127), living[0]], 0x1234);
        Assert.Equal(result.NextSeed, withExcluded.NextSeed);
        Assert.Equal(result.Slots, withExcluded.Slots);
    }

    [Fact]
    public void EmptyOrderKeepsTheSeedAndUsesTheRealSourceBufferCapacity()
    {
        var result = TurnOrderRules.Generate([], 0xBEEF);
        Assert.Equal((ushort)0xBEEF, result.NextSeed);
        Assert.Equal(64, result.Slots.Count);
        Assert.All(result.Slots, slot => Assert.Equal(new TurnOrderEntry(255, 255), slot));
    }

    [Fact]
    public void DuplicateSlotsAndOverflowCannotPublishAPartialOrder()
    {
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate(
            [new(3, true, 2, 8), new(3, true, 2, 8)], 0x1234));
        var tooMany = Enumerable.Range(0, 30).Concat([128, 129, 130])
            .Select(slot => new TurnOrderCandidate((byte)slot, true, 1, 128)).ToArray();
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate(tooMany, 0x1234));
        Assert.Equal((byte)0, tooMany[0].ActorSlot);
    }
}

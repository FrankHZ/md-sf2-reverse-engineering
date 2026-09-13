using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class TurnOrderRulesTests
{
    [Fact]
    public void SignedOrderSecondTurnsAndSentinelsRetainTheIndependentBoundaryObservation()
    {
        // Independent raw AGI/scores: turn-order-boundaries-v1.json. Actor IDs and processing
        // orders are deliberately separate, including a real order255 and a null sentinel.
        static ActorRef Id(byte value) => new("actor-" + value);
        TurnOrderCandidate<ActorRef>[] candidates = [
            new(Id(0), 255, true, 10, 128), new(Id(1), 256, true, 10, 127), new(Id(2), 257, true, 0, 8),
            new(Id(128), 383, false, 5, 5), new(Id(129), 384, true, 5, 5), new(Id(130), 385, true, 5, 5),
            new(Id(131), 386, true, 5, 5), new(Id(132), 387, true, 5, 5), new(Id(133), 388, true, 5, 5)];
        var result = TurnOrderRules.Generate<ActorRef>(candidates.Reverse(), 0);
        Assert.Equal(new TurnOrderEntry<ActorRef>[] {
            new(Id(130), 6), new(Id(129), 5), new(Id(132), 5), new(Id(133), 5), new(Id(131), 4),
            new(Id(0), 0), new(Id(0), 255), new(Id(1), 135)
        }, result.Slots.Where(slot => slot.Actor is not null));
        Assert.Equal(new TurnOrderEntry<ActorRef>(Id(0), 255), result.Slots[6]);
        Assert.All(result.Slots.Skip(7).Take(56), slot => Assert.Equal(new TurnOrderEntry<ActorRef>(null, 255), slot));
        Assert.Equal(new TurnOrderEntry<ActorRef>(Id(1), 135), result.Slots[63]);
        Assert.Equal(Id(0), candidates[0].Actor);
        Assert.Equal(255, candidates[0].ProcessingOrder);
    }

    [Fact]
    public void DeadAndUnplacedActorsDoNotConsumeRandomDraws()
    {
        TurnOrderCandidate<byte>[] living = [new(9, 9, true, 1, 4), new(140, 140, true, 5, 4)];
        var result = TurnOrderRules.Generate<byte>(living, 0x1234);
        var withExcluded = TurnOrderRules.Generate<byte>(
            [new(5, 5, true, 0, 128), living[1], new(129, 129, false, 9, 127), living[0]], 0x1234);
        Assert.Equal(result.NextSeed, withExcluded.NextSeed);
        Assert.Equal(result.Slots, withExcluded.Slots);
    }

    [Fact]
    public void EmptyOrderKeepsTheSeedAndUsesTheRealSourceBufferCapacity()
    {
        var result = TurnOrderRules.Generate<byte>([], 0xBEEF);
        Assert.Equal((ushort)0xBEEF, result.NextSeed);
        Assert.Equal(64, result.Slots.Count);
        Assert.All(result.Slots, slot => Assert.Equal(new TurnOrderEntry<byte>(null, 255), slot));
    }

    [Fact]
    public void DuplicateIdentityOrProcessingOrderAndOverflowCannotPublishAPartialOrder()
    {
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate<byte>(
            [new(3, 3, true, 2, 8), new(3, 3, true, 2, 8)], 0x1234));
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate<byte>(
            [new(3, 4, true, 2, 8), new(3, 5, true, 2, 8)], 0x1234));
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate<byte>(
            [new(3, 4, true, 2, 8), new(5, 4, true, 2, 8)], 0x1234));
        var tooMany = Enumerable.Range(0, 30).Concat([128, 129, 130])
            .Select(slot => new TurnOrderCandidate<byte>((byte)slot, slot, true, 1, 128)).ToArray();
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate<byte>(tooMany, 0x1234));
        Assert.Equal((byte)0, tooMany[0].Actor);
    }
}

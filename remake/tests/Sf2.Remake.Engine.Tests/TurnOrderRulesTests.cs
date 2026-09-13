using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class TurnOrderRulesTests
{
    [Fact]
    public void SignedOrderSecondTurnsAndSentinelsRetainTheIndependentBoundaryObservation()
    {
        // Independent raw AGI/scores: turn-order-boundaries-v1.json. Raw128 is now
        // numerical0 with an extra round action; raw127 has no extra entry.
        static ActorRef Id(byte value) => new("actor-" + value);
        TurnOrderCandidate<ActorRef>[] candidates = [
            new(Id(0), 255, true, 10, 0, true), new(Id(1), 256, true, 10, 127, false), new(Id(2), 257, true, 0, 8, false),
            new(Id(128), 383, false, 5, 5, false), new(Id(129), 384, true, 5, 5, false), new(Id(130), 385, true, 5, 5, false),
            new(Id(131), 386, true, 5, 5, false), new(Id(132), 387, true, 5, 5, false), new(Id(133), 388, true, 5, 5, false)];
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

    [Theory]
    [InlineData(0, 255, 0)]
    [InlineData(7, 6, 5)]
    [InlineData(127, 126, 105)]
    public void EqualNumericalAgilityGetsThreeOrFiveDrawsOnlyFromExplicitEligibility(byte agility, byte first, byte second)
    {
        var actor = new ActorRef("runner");
        var ordinary = TurnOrderRules.Generate<ActorRef>([new(actor, 255, true, 1, agility, false)], 0);
        var extra = TurnOrderRules.Generate<ActorRef>([new(actor, 255, true, 1, agility, true)], 0);
        Assert.Equal(new TurnOrderEntry<ActorRef>(actor, first), Assert.Single(ordinary.Slots, entry => entry.Actor is not null));
        Assert.Equal(new[] { first, second }.Order(), extra.Slots.Where(entry => entry.Actor is not null).Select(entry => entry.AlteredAgility).Order());
        // Independent word recurrence from randomness.md: seed0 after3/5/6 draws.
        // Agility0/7 include zero-range draws; agility127's secondary basis truncates to105.
        Assert.Equal((ushort)0x0501, ordinary.NextSeed);
        Assert.Equal((ushort)0x4E0B, extra.NextSeed);
        Assert.Equal((ushort)0x4114, BattleRandom.NextWord(ordinary.NextSeed, 0).After);
        Assert.Equal((ushort)0xF696, BattleRandom.NextWord(extra.NextSeed, 0).After);
    }

    [Fact]
    public void DeadAndUnplacedActorsDoNotConsumeRandomDrawsOrTheirExtraEntry()
    {
        TurnOrderCandidate<byte>[] living = [new(9, 9, true, 1, 4, false), new(140, 140, true, 5, 4, true)];
        var result = TurnOrderRules.Generate<byte>(living, 0x1234);
        var withExcluded = TurnOrderRules.Generate<byte>(
            [new(5, 5, true, 0, 0, true), living[1], new(129, 129, false, 9, 127, true), living[0]], 0x1234);
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
            [new(3, 3, true, 2, 8, false), new(3, 3, true, 2, 8, false)], 0x1234));
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate<byte>(
            [new(3, 4, true, 2, 8, false), new(3, 5, true, 2, 8, false)], 0x1234));
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate<byte>(
            [new(3, 4, true, 2, 8, false), new(5, 4, true, 2, 8, false)], 0x1234));
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate<byte>([new(3, 4, true, 2, 128, false)], 0x1234));
        var tooMany = Enumerable.Range(0, 33)
            .Select(order => new TurnOrderCandidate<byte>((byte)order, order, true, 1, 0, true)).ToArray();
        Assert.Throws<ArgumentException>(() => TurnOrderRules.Generate<byte>(tooMany, 0x1234));
        var full = TurnOrderRules.Generate<byte>(tooMany.Take(32), 0x1234);
        Assert.Equal(64, full.Slots.Count);
        Assert.DoesNotContain(full.Slots, slot => slot.Actor is null);
    }
}

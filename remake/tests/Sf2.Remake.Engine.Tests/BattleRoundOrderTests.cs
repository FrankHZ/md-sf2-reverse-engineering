using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleRoundOrderTests
{
    [Theory]
    [InlineData("dead")]
    [InlineData("unplaced")]
    [InlineData("asleep")]
    [InlineData("stunned")]
    public void CurrentStateSkipsAgreeWithTheNextActualPlayerDispatch(string condition)
    {
        var session = EngineTestContent.Start("battle-give");
        var original = session.Current.Battle;
        var champion = original.GetActor(new("champion"));
        var receiver = original.GetActor(new("receiver"));
        var changed = condition switch
        {
            "dead" => receiver.With(hp: 0),
            "unplaced" => receiver.With(clearPosition: true),
            "asleep" => receiver.With(status: 0x40),
            _ => receiver.With(status: 1),
        };
        TurnOrderEntry<ActorRef>[] queue = [new(champion.Actor, 61), new(receiver.Actor, 45),
            new(champion.Actor, 32), new(null, 255), new(new ActorRef("reserve"), 0)];
        var initial = original.With(queue: queue, cursor: 0);
        var before = BattleSaveFixture.At(initial, session.Current.Story);
        var eligible = Assert.IsType<BattleRoundOrder>(BattleRoundOrderQuery.Query(session.Definition, before));
        Assert.Equal(receiver.Actor, eligible.Next[0]!.Actor);
        Assert.Equal(champion.Actor, eligible.Next[1]!.Actor);
        Assert.Equal(2, eligible.Next.Count);
        Assert.Equal(0, eligible.Additional);
        Assert.Equal(receiver.Actor, BattleSaveFixture.Stay(before).Selection!.Actor);

        var blocked = initial.With(actors: initial.Actors.Select(actor => actor.Actor == receiver.Actor ? changed : actor));
        var current = BattleSaveFixture.At(blocked, session.Current.Story);
        for (int i = 0; i < 2; i++)
        {
            var order = Assert.IsType<BattleRoundOrder>(BattleRoundOrderQuery.Query(session.Definition, current));
            Assert.Equal(champion.Actor, order.Current.Actor);
            Assert.Equal(order.Current, Assert.Single(order.Next));
            Assert.Equal(0, order.Additional);
            Assert.Same(blocked, current.Battle);
            Assert.Equal(original.MainSeed, current.Battle.MainSeed);
            Assert.Equal(original.ThinkingSeed, current.Battle.ThinkingSeed);
            Assert.Equal(queue, current.Battle.Queue);
            Assert.Equal(0, current.Battle.Cursor);
        }
        var actual = BattleSaveFixture.Stay(current);
        Assert.Equal(champion.Actor, actual.Selection!.Actor);
        Assert.Equal(2, actual.Battle.Cursor);
        Assert.Empty(BattleRoundOrderQuery.Query(session.Definition, actual)!.Next);
        var withoutSentinel = BattleSaveFixture.At(blocked.With(queue: queue.Take(3), cursor: 2), current.Story);
        Assert.Empty(BattleRoundOrderQuery.Query(session.Definition, withoutSentinel)!.Next);
    }

    [Fact]
    public void ReadoutBoundsTheGeneratedQueueAndPreservesItsRemainingCount()
    {
        var session = EngineTestContent.Start("attack-spells");
        var before = session.Current;
        var expected = before.Battle.Queue.Skip(before.Battle.Cursor + 1).TakeWhile(slot => slot.Actor is not null)
            .Select(slot => slot.Actor!.Value).ToArray();
        Assert.Equal(4, expected.Length);
        var order = Assert.IsType<BattleRoundOrder>(session.QueryBattleRoundOrder());
        Assert.Equal(before.Selection!.Actor, order.Current.Actor);
        Assert.Equal(expected.Take(3), order.Next.Select(slot => slot!.Actor));
        Assert.Equal(1, order.Additional);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void ColdContinueRebuildsOrderFromThePartlyConsumedQueueAndThenUsesTheNewRound()
    {
        var (definition, initial) = BattleSaveFixture.Create();
        var expected = BattleRoundOrderQuery.Query(definition, initial)!;
        Assert.Equal(new ActorRef("enemy"), expected.Next[0]!.Actor);
        Assert.Equal(expected.Current, expected.Next[1]);
        Assert.Equal(2, expected.Next.Count);

        var live = BattleSaveFixture.Stay(initial);
        var liveReadout = BattleRoundOrderQuery.Query(definition, live)!;
        var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition,
            BattleSaveFixture.Capture(definition, live), RuleCompositions.Sf2())).Session;
        Assert.NotEqual(live.SessionId, restored.Current.SessionId);
        BattleSaveFixture.EqualBattle(live.Battle, restored.Current.Battle);
        var readout = Assert.IsType<BattleRoundOrder>(restored.QueryBattleRoundOrder());
        Assert.Equal(expected.Current, readout.Current);
        Assert.Equal(liveReadout.Current, readout.Current);
        Assert.Equal(liveReadout.Next, readout.Next);
        Assert.Equal(liveReadout.Additional, readout.Additional);
        Assert.Empty(readout.Next);
        Assert.Equal(0, readout.Additional);
        var snapshot = restored.Current;
        Assert.Empty(restored.QueryBattleRoundOrder()!.Next);
        Assert.Same(snapshot, restored.Current);

        var nextRound = BattleSaveFixture.Stay(snapshot);
        Assert.Equal(snapshot.Battle.Round + 1, nextRound.Battle.Round);
        var newReadout = BattleRoundOrderQuery.Query(definition, nextRound)!;
        Assert.Equal(nextRound.Selection!.Actor, newReadout.Current.Actor);
        Assert.NotEmpty(newReadout.Next);
    }
}

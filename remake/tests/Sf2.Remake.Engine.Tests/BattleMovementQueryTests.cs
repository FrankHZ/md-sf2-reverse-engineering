using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleMovementQueryTests
{
    [Fact]
    public void WeightedRangeMatchesAuthoritativeStopsAndDoesNotMutateSession()
    {
        var session = Start();
        var before = session.Current;
        var first = session.QueryBattleMovementRange()!;
        Assert.Equal(new MapPosition(3, 3), first.Origin);
        Assert.Contains(first.Origin, first.Destinations);
        Assert.Contains(new MapPosition(5, 3), first.Destinations); // forest costs 6 of budget 10
        Assert.DoesNotContain(new MapPosition(10, 3), first.Destinations);
        Assert.DoesNotContain(new MapPosition(3, 0), first.Destinations); // barrier
        Assert.DoesNotContain(new MapPosition(3, 5), first.Destinations); // ally
        Assert.Contains(new MapPosition(3, 6), first.Destinations); // through ally
        for (int y = 0; y < before.Battle.Definition.Height; y++)
            for (int x = 0; x < before.Battle.Definition.Width; x++)
            {
                var position = new MapPosition(x, y);
                var error = Record.Exception(() => BattleMovement.RequireStop(before.Battle, first.Actor, position));
                Assert.Equal(error is null, first.Destinations.Contains(position));
            }
        Assert.Equal(first.Destinations, session.QueryBattleMovementRange()!.Destinations);
        Assert.Same(before, session.Current); // includes revision, RNG, resources and observation sequence
        Assert.Throws<NotSupportedException>(() => ((IList<MapPosition>)first.Destinations).Clear());
    }

    [Fact]
    public void CorridorSeparatesEnemyBlockingFromAlliedTraversalAndExactBudget()
    {
        var session = Start(change: d =>
        {
            var rows = d["terrains"]![0]!["rows"]!.AsArray();
            for (int y = 0; y < rows.Count; y++) rows[y] = y == 3 ? "gggggggggggg" : "############";
            var placements = d["encounters"]![0]!["placements"]!;
            placements[1]!["x"] = 4; placements[1]!["y"] = 3;
            placements[2]!["x"] = 1; placements[2]!["y"] = 3;
        });
        var range = session.QueryBattleMovementRange()!;
        Assert.Equal(new[] { new MapPosition(2, 3), new(3, 3), new(5, 3), new(6, 3), new(7, 3), new(8, 3) }, range.Destinations);
        Assert.Equal(10, BattleMovement.Preview(session.Current.Battle, range.Actor, new(8, 3)).Cost);
        Assert.Equal("occupied-destination", Assert.Throws<BattleRuleException>(() =>
            BattleMovement.RequireStop(session.Current.Battle, range.Actor, new(4, 3))).Code);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(47, 8)]
    public void RangeRespectsFullWidthMapEdges(int x, int y)
    {
        var session = Start(change: d =>
        {
            var rows = d["terrains"]![0]!["rows"]!.AsArray();
            for (int row = 0; row < rows.Count; row++) rows[row] = new string('g', 48);
            d["encounters"]![0]!["placements"]![0]!["x"] = x;
            d["encounters"]![0]!["placements"]![0]!["y"] = y;
        });
        var range = session.QueryBattleMovementRange()!;
        Assert.All(range.Destinations, p => Assert.True(p.X >= 0 && p.X < 48 && p.Y >= 0 && p.Y < 9 &&
            Math.Abs(p.X - x) + Math.Abs(p.Y - y) <= 5));
        Assert.Contains(new MapPosition(x, y), range.Destinations);
        Assert.Contains(new MapPosition(x == 0 ? 5 : 42, y), range.Destinations);
    }

    [Fact]
    public void PreviewAndReturnKeepCommittedOriginAndCancelRestoresRange()
    {
        var session = Start();
        var original = session.QueryBattleMovementRange()!;
        var step = Accept(session, new Move(ExplorationDirection.East));
        Assert.Equal(original.Actor, session.QueryBattleMovementRange()!.Actor);
        Assert.Equal(original.Origin, session.QueryBattleMovementRange()!.Origin);
        Assert.Equal(original.Destinations, session.QueryBattleMovementRange()!.Destinations);
        FinishMovement(session, step);
        Assert.Equal(original.Destinations, session.QueryBattleMovementRange()!.Destinations);
        Accept(session, new Confirm());
        Assert.Null(session.QueryBattleMovementRange());
        var cancel = Accept(session, new Cancel());
        Assert.Equal(BattleMovementPurpose.Return, session.Current.BattleMovement!.Purpose);
        Assert.Equal(original.Destinations, session.QueryBattleMovementRange()!.Destinations);
        FinishMovement(session, cancel);
        Assert.Equal(original.Origin, session.QueryBattleMovementRange()!.Origin);
        Assert.Equal(original.Destinations, session.QueryBattleMovementRange()!.Destinations);
    }

    [Fact]
    public void EveryOtherSelectionStageAndOutcomeClearsRange()
    {
        var session = Start();
        var current = session.Current;
        foreach (var stage in Enum.GetValues<BattleSelectionStage>().Where(s => s != BattleSelectionStage.Movement))
        {
            var state = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
                current.Battle, new(current.Selection!.Actor, current.Selection.Preview, stage), current.StopReason);
            Assert.Null(BattleMovementQuery.Query(state));
        }
        var outcome = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            ((ActiveBattle)current.Active) with { Outcome = BattleOutcomeKind.Victory }, current.Story, current.StopReason);
        Assert.Null(BattleMovementQuery.Query(outcome));
    }

    [Fact]
    public void TurnAdvanceRecomputesForNextActor()
    {
        var session = Start();
        var first = session.QueryBattleMovementRange()!;
        Stay(session);
        var next = session.QueryBattleMovementRange()!;
        Assert.NotEqual(first.Actor, next.Actor);
        Assert.Equal(session.Current.Battle.GetActor(next.Actor).Position, next.Origin);
        Assert.DoesNotContain(first.Origin, next.Destinations);
        Assert.Contains(next.Origin, next.Destinations);
        Assert.Contains(first.Origin, first.Destinations); // old immutable result remains stable
    }

    [Fact]
    public void ActualAutomaticMovementAndFieldOrStoryControlHaveNoRange()
    {
        var session = Start("stone-court", d =>
        {
            d["actors"]![2]!["move"] = 3;
            var placements = d["encounters"]![0]!["placements"]!;
            placements[2]!["aiStrategy"] = "attack-then-approach";
            placements[2]!["x"] = 7;
            placements[3]!["x"] = 6; placements[3]!["y"] = 5;
        });
        for (int turn = 0; session.Current.BattleMovement is null && turn < 5; turn++) Stay(session);
        Assert.Equal(BattleMovementPurpose.Automatic, session.Current.BattleMovement!.Purpose);
        var pending = session.Current;
        Assert.Null(session.QueryBattleMovementRange());
        Assert.Same(pending, session.Current);
        Assert.Null(Start("harbor-arrival").QueryBattleMovementRange());

        var controlled = Start().Current;
        var story = new SessionSnapshot(controlled.SessionId, 0, 0, controlled.Active,
            new StoryState(cursor: new("control", 0)), SessionStopReason.SimulationWait);
        Assert.Null(BattleMovementQuery.Query(story));
        var round = new SessionSnapshot(controlled.SessionId, 0, 0,
            ((ActiveBattle)controlled.Active) with { Round = new([], 0, new(1)) },
            controlled.Story, SessionStopReason.PresentationWait);
        Assert.Null(BattleMovementQuery.Query(round));
    }
}

using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class CommandsetContinuationTests
{
    [Theory]
    [InlineData("stone-court", 3)]
    [InlineData("river-post", 4)]
    public void FailedCommandsMoveOnceThenNextTurnAttacksWithCarriedHistory(string package, int y)
    {
        var session = Start(package, d => Configure(d, y));
        var before = session.Current.Battle;
        var enemy = before.Actors[2].Actor;
        var ally = before.Actors[0].Actor;
        Assert.Equal(0xDC7F1234u, before.MainSeed);
        var moved = Stay(session);
        Assert.Equal(new MapPosition(5, y), moved.Snapshot.Battle.GetActor(enemy).Position);
        Assert.Equal(before.MainSeed, moved.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, moved.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(before.Gold, moved.Snapshot.Battle.Gold);
        Assert.Equal(before.Actors[1].Actor, moved.Snapshot.Selection!.Actor);
        Assert.Equal(before.Cursor + 4, moved.Snapshot.Battle.Cursor);
        Assert.Null(moved.Snapshot.Battle.GetActor(enemy).LastTarget);
        foreach (var actor in before.Actors)
        {
            var after = moved.Snapshot.Battle.GetActor(actor.Actor);
            Assert.Equal((actor.Hp, actor.Mp, actor.Exp, actor.Kills, actor.Defeats),
                (after.Hp, after.Mp, after.Exp, after.Kills, after.Defeats));
        }
        Assert.Equal(new[] { "ai-command-attack1", "ai-command-heal1", "ai-command-support", "ai-command-move1" },
            moved.Observations.Where(o => o.Kind.StartsWith("ai-command-", StringComparison.Ordinal)).Select(o => o.Kind));
        Assert.Equal(new long?[] { -1, -1, -1, 0 },
            moved.Observations.Where(o => o.Kind.StartsWith("ai-command-", StringComparison.Ordinal)).Select(o => o.After));
        var target = Assert.Single(moved.Observations, o => o.Kind == "ai-move-target");
        Assert.Equal(ally, target.Target); Assert.Equal(12, target.Before); Assert.Equal(4, target.After);
        Assert.Single(moved.Observations, o => o.Kind == "action-committed" && o.Actor == enemy);
        Assert.DoesNotContain(moved.Observations, o => o.Kind.StartsWith("rng-", StringComparison.Ordinal) || o.Kind == "physical-first");

        var round = Stay(session);
        Assert.Equal(2, round.Snapshot.Battle.Round);
        Assert.Equal(0xEE281234u, round.Snapshot.Battle.MainSeed);
        var attacked = Stay(session);
        Assert.Equal(new MapPosition(2, y), attacked.Snapshot.Battle.GetActor(enemy).Position);
        Assert.Equal(478, attacked.Snapshot.Battle.GetActor(ally).Hp);
        Assert.Equal(493, attacked.Snapshot.Battle.GetActor(enemy).Hp);
        Assert.Equal(1, attacked.Snapshot.Battle.GetActor(ally).Exp!.Value);
        Assert.Equal(0xDAA61234u, attacked.Snapshot.Battle.MainSeed);
        Assert.Equal(0x02EF0042u, attacked.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(ally, attacked.Snapshot.Battle.GetActor(enemy).LastTarget);
        Assert.Equal(new[] { "ai-command-attack1" }, attacked.Observations
            .Where(o => o.Kind.StartsWith("ai-command-", StringComparison.Ordinal)).Select(o => o.Kind));
        Assert.Single(attacked.Observations, o => o.Kind == "physical-counter");
        Assert.Equal(new ushort?[] { 3, 3, 3, 1, 12, 0, 2, 31, 0, 0, 16, 23, 10, 13 }, attacked.Observations
            .Where(o => o.Kind.StartsWith("rng-", StringComparison.Ordinal)).Select(o => o.RandomValue));
    }

    [Fact]
    public void EnemyFirstStartupCompletesContinuationBeforePlayerControl()
    {
        var document = Document("stone-court"); Configure(document, 3);
        document["actors"]![2]!["agility"] = 60;
        var started = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document)));
        Assert.Null(started.Result.Failure);
        Assert.Equal(new MapPosition(5, 3), started.Session.Current.Battle.GetActor(new("raider")).Position);
        Assert.Equal(new ActorRef("swordsman"), started.Session.Current.Selection!.Actor);
        Assert.Equal(1, started.Session.Current.Battle.Cursor);
        Assert.Equal(0xDC7F1234u, started.Session.Current.Battle.MainSeed);
        Assert.Equal(0xBEEF0042u, started.Session.Current.Battle.ThinkingSeed);
        Assert.Single(started.Result.Observations, o => o.Kind == "action-committed");
        Assert.Contains(started.Result.Observations, o => o.Kind == "ai-command-move1" && o.After == 0);
    }

    [Theory]
    [InlineData(false, "swordsman")]
    [InlineData(true, "lookout")]
    public void CurrentTargetsChoosePursuitWithoutASelectionDraw(bool swap, string expected)
    {
        var session = Start("stone-court", d =>
        {
            Configure(d, 3);
            if (swap)
            {
                var p = d["encounters"]![0]!["placements"]!;
                p[0]!["y"] = 1; p[1]!["y"] = 3;
            }
        });
        var before = session.Current.Battle;
        var result = Stay(session);
        Assert.Equal(new ActorRef(expected), Assert.Single(result.Observations, o => o.Kind == "ai-move-target").Target);
        Assert.Equal(new MapPosition(5, 3), result.Snapshot.Battle.GetActor(new("raider")).Position);
        Assert.Equal(before.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
    }

    [Theory]
    [InlineData(false, 6, "ai-move")]
    [InlineData(true, 7, "ai-move-stay")]
    public void RadiusCorrectionRespectsMovAndOccupiedStoppingCells(bool occupied, int x, string observation)
    {
        var session = Start("stone-court", d =>
        {
            Configure(d, 3); d["actors"]![2]!["move"] = 1;
            if (occupied)
            {
                var p = d["encounters"]![0]!["placements"]!;
                p[3]!["x"] = 6; p[3]!["y"] = 3;
            }
        });
        var before = session.Current.Battle;
        var result = Stay(session);
        Assert.Equal(new MapPosition(x, 3), result.Snapshot.Battle.GetActor(new("raider")).Position);
        Assert.Equal(before.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.Contains(result.Observations, o => o.Kind == observation && o.Actor == new ActorRef("raider"));
        Assert.Contains(result.Observations, o => o.Kind == "ai-command-move1" && o.After == 0);
        Assert.DoesNotContain(result.Observations, o => o.Kind == "ai-command-stay");
    }

    [Fact]
    public void WeightedTargetCostCanPreferTheMoreDistantAlly()
    {
        var session = Start("stone-court", d =>
        {
            Configure(d, 3); d["actors"]![2]!["move"] = 1;
            var p = d["encounters"]![0]!["placements"]!;
            p[0]!["x"] = 4; p[0]!["y"] = 3;
            p[1]!["x"] = 7; p[1]!["y"] = 1;
            d["terrains"]![0]!["legend"]!["d"] = new JsonObject { ["surface"] = "deep", ["protection"] = "heavy" };
            d["terrains"]![0]!["rows"]![1] = "#ppppppd#";
            d["terrains"]![0]!["rows"]![2] = "#ppppppd#";
            p[3]!["x"] = 2; p[3]!["y"] = 5;
        });
        var result = Stay(session);
        var target = Assert.Single(result.Observations, o => o.Kind == "ai-move-target");
        Assert.Equal(new ActorRef("swordsman"), target.Target); Assert.Equal(6, target.Before);
        Assert.Equal(new MapPosition(6, 3), result.Snapshot.Battle.GetActor(new("raider")).Position);
    }

    [Fact]
    public void UnreachablePursuitRetainsEarlierPlayerCommitAndFailedEnemyQueueEntry()
    {
        var session = Start("stone-court", d =>
        {
            Configure(d, 3);
            for (int y = 1; y <= 5; y++) d["terrains"]![0]!["rows"]![y] = "#pp#pppp#";
        });
        Accept(session, new Move(ExplorationDirection.South));
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var before = session.Current;
        var result = Send(session, new Confirm());
        Assert.Equal("ai-move-target-domain", result.Failure!.Code);
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal(before.Revision + 1, result.Snapshot.Revision);
        Assert.Equal(before.Battle.Cursor + 1, result.Snapshot.Battle.Cursor);
        Assert.Equal(new ActorRef("raider"), BattleTurnFlow.QueuedActor(result.Snapshot.Battle).Actor);
        Assert.Equal(new MapPosition(1, 4), result.Snapshot.Battle.GetActor(new("swordsman")).Position);
        Assert.Equal(new MapPosition(7, 3), result.Snapshot.Battle.GetActor(new("raider")).Position);
        Assert.Equal(before.Battle.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(before.Battle.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(new[] { "movement", "action-committed" }, result.Observations.Select(o => o.Kind));
        Assert.Equal("not-waiting", Send(session, new AdvanceSimulation()).Failure!.Code);
    }

    [Fact]
    public void MovementDoesNotRequireRewardsUntilAnAttackIsActuallyReached()
    {
        var session = Start("stone-court", d =>
        {
            Configure(d, 3); d["encounters"]![0]!.AsObject().Remove("rewards");
        });
        Stay(session);
        Assert.Equal(new MapPosition(5, 3), session.Current.Battle.GetActor(new("raider")).Position);
        Stay(session);
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var before = session.Current.Battle;
        var result = Send(session, new Confirm());
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal("battle-rewards", result.Failure!.Code);
        Assert.Equal(new MapPosition(5, 3), result.Snapshot.Battle.GetActor(new("raider")).Position);
        Assert.Equal(before.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(500, result.Snapshot.Battle.GetActor(new("swordsman")).Hp);
    }

    [Fact]
    public void StableUnsignedCostSelectionRejectsIncompleteAndHighCostDomains()
    {
        Assert.Equal(1, AiMovementRules.PursuitTarget([12, 4, 4, 127]));
        Assert.Equal(0, AiMovementRules.PursuitTarget([0, 0]));
        foreach (int?[] costs in new int?[][] { [], [null], [4, null], [128], [4, 255], [-1] })
            Assert.Throws<BattleRuleException>(() => AiMovementRules.PursuitTarget(costs));
    }

    [Fact]
    public void SourceWalkRetainsEarlierDirectionBitsAndHonorsCallerBounds()
    {
        var total = Enumerable.Repeat((byte)255, 2304).ToArray();
        var movable = Enumerable.Repeat((byte)255, 2304).ToArray();
        foreach (var (x, y, cost) in new[] { (7, 3, 6), (8, 3, 4), (7, 2, 2), (8, 2, 0) })
        { total[y * 48 + x] = (byte)cost; movable[y * 48 + x] = 0; }
        var grid = new WeightedMovementGrid(total, movable, []);
        var walk = AiMovementRules.Walk(grid, new(7, 3), 2, 16, 20);
        Assert.Equal(new MapPosition(8, 2), walk.Destination);
        Assert.Equal(new byte[] { 0, 1, 255 }, walk.MoveString);
        Assert.Equal(new byte[] { 255 }, AiMovementRules.Walk(grid, new(8, 2), 0, 16, 20).MoveString);
        Assert.Throws<BattleRuleException>(() => AiMovementRules.Walk(grid, new(7, 3), 2, 8, 20));
    }

    private static void Configure(JsonNode d, int y)
    {
        d["start"]!["mainSeed"] = 0x002A1234u;
        foreach (int index in new[] { 0, 2 })
        {
            d["start"]!["actors"]![index]!["hp"] = 500; d["actors"]![index]!["maxHp"] = 500;
            d["actors"]![index]!["defense"] = 4;
        }
        d["actors"]![0]!["attack"] = 18; d["actors"]![0]!["physical"]!["critical"] = new JsonObject { ["chance"] = "one-in-32", ["damageBonus"] = "half" };
        d["actors"]![2]!["attack"] = 30; d["actors"]![2]!["physical"]!["critical"] = new JsonObject { ["chance"] = "one-in-16", ["damageBonus"] = "quarter" };
        d["encounters"]![0]!["placements"]![2]!["aiStrategy"] = "attack-then-approach"; d["actors"]![2]!["move"] = 3;
        var p = d["encounters"]![0]!["placements"]!;
        p[0]!["x"] = 1; p[0]!["y"] = y; p[2]!["x"] = 7; p[2]!["y"] = y;
    }
}

using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class EnemyActionTests
{
    [Theory]
    [InlineData("stone-court", 2, 456, 500, 0, 0xFD831234u, 12)]
    [InlineData("river-post", 2, 456, 500, 0, 0xFD831234u, 12)]
    [InlineData("stone-court", 55, 478, 493, 1, 0x557E1234u, 14)]
    [InlineData("river-post", 55, 478, 493, 1, 0x557E1234u, 14)]
    [InlineData("stone-court", 73, 453, 493, 1, 0x3A1E1234u, 20)]
    public void EnemyFirstSecondAndAllyCounterUseOneActionAcrossContent(string package, int seed,
        int allyHp, int enemyHp, int exp, uint finalSeed, int draws)
    {
        var session = Start(package, d => Configure(d, seed));
        var before = session.Current.Battle;
        var ally = before.Actors[0].Actor;
        var enemy = before.Actors[2].Actor;
        var result = Stay(session);
        Assert.Equal(allyHp, result.Snapshot.Battle.GetActor(ally).Hp);
        Assert.Equal(enemyHp, result.Snapshot.Battle.GetActor(enemy).Hp);
        Assert.Equal(exp, result.Snapshot.Battle.GetActor(ally).Exp);
        Assert.Equal(0, result.Snapshot.Battle.GetActor(enemy).Exp);
        Assert.Equal(finalSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(0x02EF0042u, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(ally, result.Snapshot.Battle.GetActor(enemy).LastTarget);
        Assert.Equal(before.Actors[1].Actor, result.Snapshot.Selection!.Actor);
        Assert.Equal(before.Gold, result.Snapshot.Battle.Gold);
        var rolls = result.Observations.Where(o => o.Kind.StartsWith("rng-", StringComparison.Ordinal)).ToArray();
        Assert.Equal(draws, rolls.Length);
        Assert.Equal((long)before.MainSeed, rolls[0].Before);
        for (int i = 1; i < rolls.Length; i++) Assert.Equal(rolls[i - 1].After, rolls[i].Before);
        Assert.All(result.Observations.Where(o => o.Kind == "exp"), o => Assert.Equal(ally, o.Actor));
        Assert.Equal(enemy, Assert.Single(result.Observations, o => o.Kind == "physical-first").Actor);
        Assert.DoesNotContain(result.Observations, o => o.Kind == "ai-stay" && o.Actor == enemy);
    }

    [Fact]
    public void StartupCanRunEnemyThenYieldAndContinuedHistoryCarriesBothStreams()
    {
        var doc = Document("stone-court"); Configure(doc, 55);
        doc["actors"]![0]!["agility"] = 1;
        var started = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc)));
        Assert.Null(started.Result.Failure);
        Assert.Contains(started.Result.Observations, o => o.Kind == "physical-counter");
        var firstThinking = started.Session.Current.Battle.ThinkingSeed;
        var history = new List<SessionObservation>();
        for (int i = 0; i < 3 && !history.Any(o => o.Kind == "thinking-rng"); i++)
            history.AddRange(Stay(started.Session).Observations);
        var next = Assert.Single(history, o => o.Kind == "thinking-rng");
        Assert.Equal((long)firstThinking, next.Before);
        Assert.Equal(0x00EF0042L, next.After);
        Assert.Equal(2, started.Session.Current.Battle.Round);
        var round = Assert.Single(history, o => o.Kind == "round-rng");
        Assert.Equal(started.Result.Snapshot.Battle.MainSeed, round.Before);
        Assert.Equal(round.After, history.First(o => o.Kind == "rng-dodge").Before);
    }

    [Theory]
    [InlineData(false, "swordsman")]
    [InlineData(true, "lookout")]
    public void CurrentPositionsSelectTargetAndSourceRingSelectsMovement(bool swap, string target)
    {
        var session = Start("stone-court", d =>
        {
            Configure(d, 55);
            var placements = d["encounters"]![0]!["placements"]!;
            placements[2]!["x"] = 7; placements[3]!["x"] = 6; placements[3]!["y"] = 5;
            d["actors"]![2]!["move"] = 3;
            if (swap)
            {
                placements[0]!["x"] = 1; placements[0]!["y"] = 1;
                placements[1]!["x"] = 3; placements[1]!["y"] = 3;
                d["start"]!["actors"]![1]!["hp"] = 500; d["actors"]![1]!["maxHp"] = 500;
            }
        });
        var result = Stay(session);
        var decision = Assert.Single(result.Observations, o => o.Kind == "ai-target");
        Assert.Equal(new ActorRef(target), decision.Target);
        var move = Assert.Single(result.Observations, o => o.Kind == "movement");
        Assert.Equal(new MapPosition(7, 3), move.From);
        Assert.Equal(new MapPosition(4, 3), move.To);
        Assert.Equal(new MapPosition(4, 3), result.Snapshot.Battle.GetActor(new("raider")).Position);
    }

    [Theory]
    [InlineData("level", "level-up")]
    [InlineData("leader", "leader-defeat-program")]
    public void UnsupportedEnemyActionRetainsEarlierPlayerCommitAndAllEnemyState(string shape, string code)
    {
        var session = Start("stone-court", d =>
        {
            Configure(d, 55);
            if (shape == "level")
            {
                d["start"]!["actors"]![0]!["exp"] = 99; d["start"]!["actors"]![2]!["hp"] = 1;
            }
            if (shape == "leader")
            {
                d["start"]!["actors"]![0]!["hp"] = 1; d["actors"]![0]!["physical"]!["leader"] = true;
            }
        });
        Accept(session, new Move(ExplorationDirection.South));
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var before = session.Current;
        var result = Send(session, new Confirm());
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal(code, result.Failure!.Code);
        Assert.Same(result.Snapshot, session.Current);
        Assert.Equal(before.Battle.Cursor + 1, result.Snapshot.Battle.Cursor);
        Assert.Equal(before.Revision + 1, result.Snapshot.Revision);
        Assert.Equal(new MapPosition(3, 4), result.Snapshot.Battle.GetActor(new("swordsman")).Position);
        Assert.Equal(before.Battle.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(before.Battle.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(before.Battle.Gold, result.Snapshot.Battle.Gold);
        foreach (var actor in before.Battle.Actors)
        {
            var after = result.Snapshot.Battle.GetActor(actor.Actor);
            Assert.Equal((actor.Hp, actor.Exp, actor.Kills, actor.Defeats, actor.LastTarget),
                (after.Hp, after.Exp, after.Kills, after.Defeats, after.LastTarget));
        }
        Assert.Equal(new[] { "movement", "action-committed" }, result.Observations.Select(o => o.Kind));
        Assert.Equal("not-waiting", Send(session, new AdvanceSimulation()).Failure!.Code);
        Assert.Same(result.Snapshot, session.Current);
    }

    [Fact]
    public void AllyCounterKillAwardsAllyAndSkipsDeadEnemySecondQueueEntry()
    {
        var doc = Document("stone-court"); Configure(doc, 7);
        doc["actors"]![0]!["agility"] = 20;
        doc["actors"]![2]!["agility"] = 30; doc["actors"]![2]!["extraRoundAction"] = true;
        doc["start"]!["actors"]![2]!["hp"] = 1;
        doc["start"]!["actors"]![0]!["kills"] = 9999;
        var started = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc)));
        Assert.Null(started.Result.Failure);
        var state = started.Session.Current.Battle;
        Assert.Equal(0, state.GetActor(new("raider")).Hp);
        Assert.Null(state.GetActor(new("raider")).Position);
        Assert.Equal(24, state.GetActor(new("swordsman")).Exp);
        Assert.Equal(9999, state.GetActor(new("swordsman")).Kills);
        Assert.Equal(119u, state.Gold);
        Assert.Equal(0x9A0E1234u, state.MainSeed);
        var history = started.Result.Observations.ToList();
        for (int i = 0; i < 3 && !history.Any(o => o.Kind == "dead-entry-skipped"); i++)
            history.AddRange(Stay(started.Session).Observations);
        Assert.Equal(new ActorRef("raider"), Assert.Single(history, o => o.Kind == "dead-entry-skipped").Actor);
        Assert.Single(history, o => o.Kind == "physical-first");
        Assert.DoesNotContain(history, o => o.Kind == "ai-stay" && o.Actor == new ActorRef("raider"));
    }

    [Fact]
    public void EnemyKillDefeatsAllyOnceAndSkipsBothOfItsQueuedEntriesWithoutExperience()
    {
        var doc = Document("stone-court"); Configure(doc, 7);
        doc["actors"]![0]!["agility"] = 0; doc["actors"]![0]!["extraRoundAction"] = true; doc["start"]!["actors"]![0]!["hp"] = 1;
        doc["start"]!["actors"]![0]!["defeats"] = 9999;
        doc["actors"]![2]!["agility"] = 30;
        var started = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc)));
        Assert.Null(started.Result.Failure);
        var state = started.Session.Current.Battle;
        Assert.Null(state.GetActor(new("swordsman")).Position);
        Assert.Equal(9999, state.GetActor(new("swordsman")).Defeats);
        Assert.Equal(new ActorRef("lookout"), started.Session.Current.Selection!.Actor);
        Assert.Equal(100u, state.Gold);
        Assert.DoesNotContain(started.Result.Observations, o => o.Kind is "exp" or "rng-exp-plus" or "physical-counter");
        Assert.Single(started.Result.Observations, o => o.Kind == "defeats");
        // Both AGI secondary entries remain owned by the queue; the one after control is
        // skipped on the subsequent advance, never converted to a living actor's action.
        var observations = started.Result.Observations.ToList();
        if (observations.Count(o => o.Kind == "dead-entry-skipped") < 2)
        {
            Accept(started.Session, new Confirm()); Accept(started.Session, new ChooseAction(SessionAction.Stay));
            var next = Send(started.Session, new Confirm());
            // The next round now continues through MOVE1 after the dead entries.
            Assert.Null(next.Failure);
            Assert.Contains(next.Observations, o => o.Kind == "ai-command-move1" && o.After == 0);
            observations.AddRange(next.Observations);
        }
        Assert.Equal(2, observations.Count(o => o.Kind == "dead-entry-skipped"));
    }

    [Fact]
    public void StartupFailureKeepsGeneratedQueueButPublishesNoFailedAction()
    {
        var doc = Document("stone-court"); Configure(doc, 55);
        doc["actors"]![2]!["agility"] = 30; doc["actors"]![0]!["agility"] = 1;
        doc["start"]!["actors"]![0]!["hp"] = 1; doc["start"]!["actors"]![1]!["hp"] = 0;
        var started = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc)));
        Assert.Equal("battle-outcome-program", started.Result.Failure!.Code);
        Assert.Equal(SessionStopReason.Unsupported, started.Session.Current.StopReason);
        Assert.Equal(1, started.Session.Current.Battle.Round);
        Assert.Equal(0, started.Session.Current.Battle.Cursor);
        Assert.Equal(1, started.Session.Current.Battle.GetActor(new("swordsman")).Hp);
        Assert.Null(started.Session.Current.Battle.GetActor(new("raider")).LastTarget);
        Assert.Equal(0xBEEF0042u, started.Session.Current.Battle.ThinkingSeed);
        Assert.Equal(new[] { "round-started", "round-rng" }, started.Result.Observations.Select(o => o.Kind));
        Assert.Equal(started.Result.Observations[^1].After, started.Session.Current.Battle.MainSeed);
    }

    [Fact]
    public void LaterUnsupportedEnemyDoesNotUndoFirstAutomaticEnemyAction()
    {
        var session = Start("stone-court", d =>
        {
            Configure(d, 55);
            d["actors"]![3]!["controller"] = "commandset06-script3";
            d["actors"]![3]!["move"] = 6;
            d["actors"]![1]!.AsObject().Remove("physical");
        });
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var result = Send(session, new Confirm());
        Assert.Equal("physical-definition", result.Failure!.Code);
        Assert.Equal(478, result.Snapshot.Battle.GetActor(new("swordsman")).Hp);
        Assert.Equal(493, result.Snapshot.Battle.GetActor(new("raider")).Hp);
        Assert.Equal(0x557E1234u, result.Snapshot.Battle.MainSeed);
        Assert.Equal(0x02EF0042u, result.Snapshot.Battle.ThinkingSeed);
        Assert.Null(result.Snapshot.Battle.GetActor(new("scavenger")).LastTarget);
        Assert.Equal(new ActorRef("scavenger"), BattleTurnFlow.QueuedActor(result.Snapshot.Battle).Actor);
        Assert.Equal(new ActorRef?[] { new("swordsman"), new("raider") },
            result.Observations.Where(o => o.Kind == "action-committed").Select(o => o.Actor));
    }

    [Theory]
    [InlineData("controller", "attack-nearest", "ai-commandset")]
    [InlineData("move", "64", "ai-movement-domain")]
    public void UnsupportedAiDefinitionsFailAdmission(string field, string value, string code)
    {
        var doc = Document("stone-court"); Configure(doc, 55);
        doc["actors"]![2]![field] = field == "move" ? JsonValue.Create(int.Parse(value)) : JsonValue.Create(value);
        Assert.Equal(code, Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(doc))).Failure.Code);
    }

    private static void Configure(JsonNode doc, int seed)
    {
        doc["start"]!["mainSeed"] = ((uint)seed << 16) | 0x1234u;
        foreach (int index in new[] { 0, 2 })
        {
            doc["start"]!["actors"]![index]!["hp"] = 500; doc["actors"]![index]!["maxHp"] = 500;
            doc["actors"]![index]!["defense"] = 4;
        }
        doc["actors"]![0]!["attack"] = 18; doc["actors"]![0]!["physical"]!["prowess"] = 0;
        doc["actors"]![2]!["attack"] = 30; doc["actors"]![2]!["physical"]!["prowess"] = 3;
        doc["actors"]![2]!["controller"] = "commandset06-script3"; doc["actors"]![2]!["move"] = 1;
    }
}

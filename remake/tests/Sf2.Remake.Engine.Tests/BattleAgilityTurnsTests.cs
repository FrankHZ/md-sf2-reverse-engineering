using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleAgilityTurnsTests
{
    [Fact]
    public void EqualAgilityDefinitionsConsumeExtraEntryAndCarrySeedsAcrossRounds()
    {
        var ordinary = Start();
        var extra = Start(change: document => document["actors"]![0]!["extraRoundAction"] = true);
        var actor = new ActorRef("medic-a"); var guard = new ActorRef("guard-a");
        var ordinaryDefinition = ordinary.Current.Battle.GetActor(actor).Definition;
        var extraDefinition = extra.Current.Battle.GetActor(actor).Definition;
        Assert.Equal((byte)12, ordinaryDefinition.Agility);
        Assert.Equal(ordinaryDefinition.Agility, extraDefinition.Agility);
        Assert.False(ordinaryDefinition.ExtraRoundAction); Assert.True(extraDefinition.ExtraRoundAction);
        Assert.Single(ordinary.Current.Battle.Queue, entry => entry.Actor == actor);
        Assert.Equal(2, extra.Current.Battle.Queue.Count(entry => entry.Actor == actor));
        // Accepted three ordinary actors consume9 draws; the explicit extra entry adds2.
        // Independent word recurrence from randomness.md, initial high word1234.
        Assert.Equal(0xFB731234u, ordinary.Current.Battle.MainSeed);
        Assert.Equal(0xFF4D1234u, extra.Current.Battle.MainSeed);
        var initial = extra.Current.Battle;
        var first = Stay(extra);
        Assert.Equal((1, 1, actor), (first.Snapshot.Battle.Round, first.Snapshot.Battle.Cursor, first.Snapshot.Selection!.Actor));
        Assert.Equal(initial.MainSeed, first.Snapshot.Battle.MainSeed);
        Assert.DoesNotContain(first.Observations, o => o.Kind == "ai-stay");
        var second = Stay(extra);
        Assert.Equal((1, 3, guard), (second.Snapshot.Battle.Round, second.Snapshot.Battle.Cursor, second.Snapshot.Selection!.Actor));
        Assert.Single(second.Observations, o => o.Kind == "ai-stay");
        Assert.Equal(initial.MainSeed, second.Snapshot.Battle.MainSeed);
        var nextRound = Stay(extra);
        Assert.Equal((2, 0, actor), (nextRound.Snapshot.Battle.Round, nextRound.Snapshot.Battle.Cursor, nextRound.Snapshot.Selection!.Actor));
        Assert.Equal(0x887A1234u, nextRound.Snapshot.Battle.MainSeed); //22 total draws.
        Assert.Equal(actor, Stay(extra).Snapshot.Selection!.Actor);
        Assert.Equal(guard, Stay(extra).Snapshot.Selection!.Actor);
        Assert.Equal(0xBDCB1234u, Stay(extra).Snapshot.Battle.MainSeed); //33 total draws.
        Assert.Equal(initial.ThinkingSeed, extra.Current.Battle.ThinkingSeed);
        foreach (var before in initial.Actors)
        {
            var after = extra.Current.Battle.GetActor(before.Actor);
            Assert.Equal((before.Hp, before.Mp, before.Exp, before.Kills, before.Defeats, before.Position),
                (after.Hp, after.Mp, after.Exp, after.Kills, after.Defeats, after.Position));
        }
    }

    [Theory]
    [InlineData("missing-extra", "missing-field")]
    [InlineData("numeric-extra", "boolean")]
    [InlineData("string-extra", "boolean")]
    [InlineData("null-extra", "boolean")]
    [InlineData("missing-agility", "missing-field")]
    [InlineData("negative-agility", "numeric-range")]
    [InlineData("high-bit-agility", "numeric-range")]
    [InlineData("raw-sentinel-agility", "numeric-range")]
    public void ContentRequiresSeparateNumericalAgilityAndBooleanEligibility(string shape, string code)
    {
        var document = Document(); var actor = document["actors"]![0]!.AsObject();
        switch (shape)
        {
            case "missing-extra": actor.Remove("extraRoundAction"); break;
            case "numeric-extra": actor["extraRoundAction"] = 1; break;
            case "string-extra": actor["extraRoundAction"] = "false"; break;
            case "null-extra": actor["extraRoundAction"] = null; break;
            case "missing-agility": actor.Remove("agility"); break;
            case "negative-agility": actor["agility"] = -1; break;
            case "high-bit-agility": actor["agility"] = 128; break;
            case "raw-sentinel-agility": actor["agility"] = 255; break;
        }
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal((SessionFailureKind.ContentError, code), (failed.Failure.Kind, failed.Failure.Code));
    }

    [Fact]
    public void ExtraEntriesFillTheRealStartCapacityWhileDeadReserveDoesNotConsumeIt()
    {
        var document = Document();
        document["terrains"]![0]!["rows"] = new JsonArray(Enumerable.Repeat("111111111111", 8).Select(row => JsonValue.Create(row)).ToArray());
        var actors = document["actors"]!.AsArray(); var starts = document["start"]!["actors"]!.AsArray();
        var placements = document["encounters"]![0]!["placements"]!.AsArray();
        for (int i = 3; i < 33; i++)
        {
            string id = $"reserve-{i}";
            var definition = actors[2]!.DeepClone(); definition["id"] = id; actors.Add(definition);
            var start = starts[2]!.DeepClone(); start["actor"] = id; starts.Add(start);
            var deployment = placements[2]!.DeepClone(); deployment["actor"] = id; placements.Add(deployment);
        }
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i]!["extraRoundAction"] = true;
            placements[i]!["processingOrder"] = i; placements[i]!["x"] = i % 12; placements[i]!["y"] = i / 12;
        }
        starts[32]!["hp"] = 0;
        var admitted = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document)));
        Assert.Equal(64, admitted.Session.Current.Battle.Queue.Count(entry => entry.Actor is not null));
        Assert.DoesNotContain(admitted.Session.Current.Battle.Queue, entry => entry.Actor == new ActorRef("reserve-32"));
        starts[32]!["hp"] = 1;
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal((SessionFailureKind.ContentError, "turn-buffer-capacity"), (failed.Failure.Kind, failed.Failure.Code));
    }
}

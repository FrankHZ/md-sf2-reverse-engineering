using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ExplorationTextWaitTests
{
    [Theory]
    [InlineData(0x12341234u, 0, 1, 0xECAB1234u, 236)]
    [InlineData(0xC632A55Au, 1, 16, 0xD764A55Au, 215)]
    [InlineData(0xFFFFBEEFu, 4, 7, 0xA3AEBEEFu, 163)]
    public void EveryPollIncludingAcceptanceDrawsCopiesThenWaitsWithoutEntityServices(uint seed, int polls, int phase, uint expected, int copy)
    {
        var session = StartText("Hello{W1}", seed, phase);
        var entry = session.Current;
        var wait = Assert.IsType<W1TextWait>(entry.Story.Wait);
        Assert.Null(entry.Story.RandomSeedCopy);
        Assert.False(entry.CanWaitForText);
        Assert.Equal("text-input-unavailable", Send(session, new Acknowledge(wait.Token)).Failure!.Code);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(wait.Token)).Failure!.Code);
        Assert.Equal("explicit-text-wait-required", Send(session, new AdvanceSimulation(wait.Token)).Failure!.Code);
        Assert.Same(entry, session.Current);
        Accept(session, new CompleteTextReveal(wait.Token));
        Assert.Equal(entry.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.Equal(seed, session.Current.Exploration!.Party.MainSeed);
        Assert.True(session.Current.CanWaitForText);
        for (int index = 0; index <= polls; index++)
        {
            var before = session.Current;
            var result = Accept(session, index == polls ? new Acknowledge(wait.Token) : new WaitForText(wait.Token));
            Assert.Equal(new[] { "rng-text-w1", "text-seed-copy", "text-w1-wait", "text-w1-input" },
                result.Observations.Take(4).Select(row => row.Kind));
            var draw = result.Observations[0];
            Assert.Equal((ushort)256, draw.RandomRange);
            Assert.Equal(before.Exploration!.Party.MainSeed, draw.Before);
            Assert.Equal(session.Current.Exploration!.Party.MainSeed, draw.After);
            Assert.Equal(draw.RandomValue, result.Observations[1].After);
            Assert.Equal(index == polls ? "accept" : "none", result.Observations[3].Detail);
            Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
            Assert.Equal(entry.Exploration!.AllEntities, session.Current.Exploration.AllEntities);
            Assert.Equal(entry.Exploration.Party.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        }
        Assert.Equal(expected, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal((byte)copy, session.Current.Story.RandomSeedCopy);
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Null(session.Current.Story.EntityEvent);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        // Ordinary service resumes only after returning control, preserving the copy byte.
        Accept(session, new WaitAtInput());
        Assert.Equal((byte)copy, session.Current.Story.RandomSeedCopy);
        Assert.NotEqual(entry.Exploration!.Entities[new("ferryman")].Motion, session.Current.Exploration.Entities[new("ferryman")].Motion);
    }

    [Fact]
    public void OrderedSpansNamesAndRepeatedWaitsRequireSeparateTokensAndDeliverTheTail()
    {
        var session = StartText("Hi{N}{NAME;1}{W1} then {LEADER}{W1} tail");
        var entry = session.Current;
        var first = Assert.IsType<W1TextWait>(entry.Story.Wait);
        Assert.Equal(3, first.EndToken);
        var tokens = session.Definition.Exploration!.TextTokens[100];
        Assert.Equal(new[] { "Hi", "{N}", "{NAME;1}", "{W1}", " then ", "{LEADER}", "{W1}", " tail" }, tokens.Select(part => part.Value));
        Assert.Equal("B{W2}", session.Definition.Exploration.MemberNames[1]); // Substitution is literal data.
        Accept(session, new CompleteTextReveal(first.Token));
        var staleRevision = session.Current.Revision;
        Accept(session, new WaitForText(first.Token));
        Assert.Equal("stale-input", session.Submit(new(entry.SessionId, staleRevision, null, new Acknowledge(first.Token))).Failure!.Code);
        Accept(session, new Acknowledge(first.Token));
        var second = Assert.IsType<W1TextWait>(session.Current.Story.Wait);
        Assert.NotEqual(first.Token, second.Token);
        Assert.Equal(6, second.EndToken);
        Assert.False(second.Revealed);
        Assert.Same(entry.Story.TextWindow, session.Current.Story.TextWindow);
        var before = session.Current;
        Assert.Equal("stale-or-wrong-wait", Send(session, new Acknowledge(first.Token)).Failure!.Code);
        Assert.Equal("stale-or-wrong-text-delivery", Send(session, new CompleteTextReveal(first.Token)).Failure!.Code);
        Assert.Same(before, session.Current);
        Accept(session, new CompleteTextReveal(second.Token));
        Accept(session, new Acknowledge(second.Token));
        var tail = Assert.IsType<W1TextWait>(session.Current.Story.Wait);
        Assert.False(tail.AtInput);
        Assert.Equal(8, tail.EndToken);
        Assert.False(session.Current.CanWaitForText);
        var afterPolls = session.Current;
        Assert.Equal("text-input-unavailable", Send(session, new Acknowledge(tail.Token)).Failure!.Code);
        var delivered = Accept(session, new CompleteTextReveal(tail.Token));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Equal(afterPolls.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.Equal(afterPolls.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.DoesNotContain(delivered.Observations, row => row.Kind == "rng-text-w1");
        Assert.Equal(101, session.Current.Story.TextCursor);
    }

    [Fact]
    public void DeliveryDelayAndRevealOnlyAttemptsDoNotChangeEquivalentPollOutcomes()
    {
        var instant = StartText("A{W1}{W1}end");
        var delayed = StartText("A{W1}{W1}end");
        foreach (var session in new[] { instant, delayed })
        {
            while (session.Current.Story.Wait is W1TextWait wait)
            {
                var before = session.Current;
                if (session == delayed)
                    for (int index = 0; index < 12; index++)
                    {
                        Send(session, new AdvanceSimulation(wait.Token));
                        Send(session, new WaitForText(wait.Token));
                        Assert.Same(before, session.Current);
                    }
                Accept(session, new CompleteTextReveal(wait.Token));
                if (!wait.AtInput) break;
                Accept(session, new WaitForText(wait.Token));
                Accept(session, new Acknowledge(wait.Token));
            }
        }
        Assert.Equal(instant.Current.Story.SimulationTick, delayed.Current.Story.SimulationTick);
        Assert.Equal(instant.Current.Exploration!.Party.MainSeed, delayed.Current.Exploration!.Party.MainSeed);
        Assert.Equal(instant.Current.Story.RandomSeedCopy, delayed.Current.Story.RandomSeedCopy);
        Assert.Equal(JsonSerializer.Serialize(instant.Current.Exploration.AllEntities), JsonSerializer.Serialize(delayed.Current.Exploration.AllEntities));
        Assert.True(delayed.Current.CanWaitAtInput);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("open")]
    [InlineData("missing")]
    [InlineData("portrait-present")]
    [InlineData("entities-enabled")]
    [InlineData("not-event")]
    [InlineData("legacy-windows")]
    [InlineData("w2")]
    [InlineData("unknown-token")]
    [InlineData("missing-name")]
    public void UnboundConsumerRemainsLegacyAndCannotPollAsW1(string excluded)
    {
        var text = excluded switch { "w2" => "A{W1}B{W2}", "unknown-token" => "A{W1}{S1}", "missing-name" => "{NAME;99}{W1}", _ => "A{W1}" };
        var session = StartText(text, exclusion: excluded);
        Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Assert.False(session.Current.CanWaitForText);
        var before = session.Current;
        Send(session, new WaitForText(before.Story.Wait!.Token));
        Assert.Same(before, session.Current);
        Assert.Null(session.Current.Story.RandomSeedCopy);
        Accept(session, new Acknowledge(before.Story.Wait.Token));
        Assert.Equal(before.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal(before.Story.SimulationTick, session.Current.Story.SimulationTick);
    }

    [Fact]
    public void OrdinaryFacingServiceRunsBeforeSuppressedW1AndRestoresFacingOnReturn()
    {
        var session = StartText("A{W1}", face: true);
        Assert.IsType<EntityEventFacingWait>(session.Current.Story.Wait);
        var entry = session.Current;
        var npc = entry.Exploration!.Entities[new("ferryman")];
        Accept(session, new AdvanceSimulation(entry.Story.Wait!.Token));
        var wait = Assert.IsType<W1TextWait>(session.Current.Story.Wait);
        Assert.Equal(entry.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.NotEqual(npc.Motion, session.Current.Exploration!.Entities[new("ferryman")].Motion);
        var faced = session.Current.Exploration.Entities[new("ferryman")];
        Accept(session, new CompleteTextReveal(wait.Token));
        Accept(session, new WaitForText(wait.Token));
        Assert.Equal(faced, session.Current.Exploration.Entities[new("ferryman")]);
        Accept(session, new Acknowledge(wait.Token));
        Assert.Equal(npc.Motion.Facing, session.Current.Exploration.Entities[new("ferryman")].Motion.Facing);
        Assert.True(session.Current.CanWaitAtInput);
    }

    private static GameSession StartText(string text, uint seed = 0x12341234, int phase = 1, string? exclusion = null, bool face = false)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["battle"]!["start"]!["mainSeed"] = seed;
            var world = document["world"]!;
            world["texts"]![0]!["text"] = text;
            world["memberNames"] = new JsonArray("A", "B{W2}");
            var callback = world["programs"]![0]!;
            callback["entitiesRunning"] = exclusion == "entities-enabled";
            callback["instructions"] = JsonNode.Parse("""
                [{"op":"text-cursor","text":100},
                 {"op":"show-text","mode":"single","speaker":null,"useEventSpeaker":true,"explicitWindows":true},
                 {"op":"end"}]
                """);
            if (exclusion == "legacy-windows") callback["instructions"]![1]!["explicitWindows"] = false;
            var npc = world["maps"]![0]!["entities"]![0]!;
            npc["sprite"] = 30;
            npc["facing"] = 1;
            npc["actions"] = JsonNode.Parse($$"""[{"op":"wait","ticks":{{phase}}},{"op":"random-walk","x":2,"y":1,"radius":1}]""");
            if (exclusion != "not-event") world["maps"]![0]!["events"]![0]!["entityFlags"] = face ? 3 : 0;
            if (exclusion != "unknown")
            {
                world["programs"]!.AsArray().Add(JsonNode.Parse(exclusion == "open" ? """
                    {"id":"setup","instructions":[{"op":"close-portrait"},{"op":"open-portrait","entity":"ferryman","flags":0},{"op":"end"}]}
                    """ : """{"id":"setup","instructions":[{"op":"close-portrait"},{"op":"end"}]}"""));
                document["start"]!["program"] = JsonNode.Parse("""{"program":"setup","instruction":0}""");
            }
            if (exclusion != "missing") AddVisuals(document, exclusion is "open" or "portrait-present" ? 7 : null);
        });
        Accept(session, new Interact(new("ferryman")));
        return session;
    }

    private static void AddVisuals(JsonNode document, int? portrait)
    {
        object Raster(int width, int height)
        {
            byte[] bytes = new byte[width * height * 4];
            return new { width, height, format = "rgba8", data = Convert.ToBase64String(bytes),
                sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)) };
        }
        document["world"]!["presentation"] = JsonSerializer.SerializeToNode(new
        {
            maps = document["world"]!["maps"]!.AsArray().Select(map => new
            { map = map!["id"]!.GetValue<string>(), atlas = Raster(128, 320), scale = 1,
                blocks = Enumerable.Range(0, 1024).Select(_ => new int[9]).ToArray() }),
            sprites = new[] { new { sprite = 30, directions = Enumerable.Range(0, 3).Select(_ => Raster(48, 24)).ToArray(), portrait, speech = 0 } },
            portraits = new[] { new { portrait = 7, raster = Raster(64, 64) } },
        });
    }
}

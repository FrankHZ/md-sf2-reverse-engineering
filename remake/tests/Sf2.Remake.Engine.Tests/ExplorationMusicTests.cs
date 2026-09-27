using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.ExplorationTextWaitTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class ExplorationMusicTests
{
    [Theory]
    [InlineData(505, 3)]
    [InlineData(504, 3)]
    [InlineData(503, 3)]
    [InlineData(502, 3)]
    [InlineData(501, 6)]
    [InlineData(0, 507)]
    public void HelperArmsThenSamplesAndCompletesWholeGroups(int elapsed, int expected)
    {
        var session = StartMusic(elapsed);
        var before = session.Current;
        var music = before.Story.Music!;
        Assert.Equal(elapsed, music.Step);
        var wait = Assert.IsType<MusicWait>(before.Story.Wait);
        var first = Accept(session, new AdvanceSimulation(wait.Token));
        var armed = Assert.IsType<MusicWait>(session.Current.Story.Wait);
        Assert.True(armed.Armed);
        Assert.False(armed.Cleared);
        Assert.Single(first.Observations, o => o.Kind == "music-wait-armed");
        var rest = Accept(session, new AdvanceSimulation(wait.Token, 600));
        var done = Assert.IsType<MusicWait>(session.Current.Story.Wait);
        Assert.True(done.LogicalDone);
        Assert.Equal(expected, done.Elapsed);
        Assert.Equal(expected, session.Current.Story.SimulationTick - before.Story.SimulationTick);
        Assert.True(session.Current.Story.Music!.PreviousEligible);
        Assert.Single(rest.Observations, o => o.Kind == "music-previous-eligible");
        var stopped = session.Current;
        foreach (var command in new SessionCommand[] { new AdvanceSimulation(wait.Token), new WaitForText(wait.Token),
            new WaitAtInput(), new Acknowledge(wait.Token), new CompletePresentation(wait.Token, PresentationCueKind.SoundWait) })
        {
            Assert.NotNull(Send(session, command).Failure);
            Assert.Same(stopped, session.Current);
        }
        Accept(session, new CompleteMusic(music.Generation, music.Cue));
        Assert.Equal(PresentationCueKind.PreviousMusic, Assert.IsType<PresentationWait>(session.Current.Story.Wait).Cue.Kind);
        Assert.Equal(stopped.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.Equal(stopped.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal("town", session.Current.Story.Music!.Cue);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EarlyActualCompletionAndBatchingRetainLiveServiceOrder(bool enabled)
    {
        var session = StartMusic(501, enabled: enabled);
        var current = session.Current;
        var entry = current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait, randomSeedCopy: 0xA9,
            portraitWindow: new OpenPortraitWindow(7, 0, new(Blink: 1, Mouth: 0, Registered: true, Movement: 4, Moving: false))));
        var music = entry.Story.Music!;
        var receipt = Apply(entry, new CompleteMusic(music.Generation, music.Cue));
        Assert.Equal(entry.Story.SimulationTick, receipt.Snapshot.Story.SimulationTick);
        var result = Apply(receipt.Snapshot, new AdvanceSimulation(entry.Story.Wait!.Token, 600));
        var early = result.Snapshot;
        var late = entry;
        for (int i = 0; i < 6; i++) late = Apply(late, new AdvanceSimulation(late.Story.Wait!.Token)).Snapshot;
        late = Apply(late, new CompleteMusic(music.Generation, music.Cue)).Snapshot;
        Assert.Equal(early.Story.SimulationTick, late.Story.SimulationTick);
        Assert.Equal(early.Exploration!.Party.MainSeed, late.Exploration!.Party.MainSeed);
        Assert.Equal(JsonSerializer.Serialize(early.Exploration.AllEntities), JsonSerializer.Serialize(late.Exploration.AllEntities));
        Assert.Equal(early.Story.PortraitWindow, late.Story.PortraitWindow);
        Assert.Equal((byte)0xA9, early.Story.RandomSeedCopy);
        Assert.IsType<OpenPortraitWindow>(early.Story.PortraitWindow);
        Assert.Equal("music-wait-armed", result.Observations[0].Kind);
        Assert.Equal("music-helper-service", result.Observations[1].Kind);
        Assert.Equal("rng-portrait-blink", result.Observations[2].Kind);
        Assert.Equal("rng-portrait-mouth", result.Observations[3].Kind);
        SessionResult Apply(SessionSnapshot snapshot, SessionCommand command)
        {
            var result = ExplorationDispatcher.Submit(session.Definition, snapshot, command);
            Assert.Null(result.Failure);
            return result;
        }
    }

    [Theory]
    [InlineData("A{NAME;1}B", 0, 6, "N", false, 19, 505)]
    [InlineData("AB{N}CD{N}EF{N}G", 3, 16, "Other", true, 18, 7)]
    [InlineData("{NAME;1} AB", 1, 8, "Long name", true, 19, 505)]
    public void RawTextProgressAndDeliveryReachPlainInputAndCompleteReturn(string text, int speed, int width,
        string name, bool enabled, int command, int end)
    {
        var session = StartMusic(0, text, speed, width, name, enabled, command, end);
        var entry = session.Current;
        var music = entry.Story.Music!;
        DrainTextWork(session);
        Assert.Equal(Math.Min(end, session.Current.Story.SimulationTick - entry.Story.SimulationTick), session.Current.Story.Music!.Step);
        var logicalEnd = session.Current;
        Accept(session, new CompleteTextReveal(logicalEnd.Story.Wait!.Token));
        Assert.Equal(logicalEnd.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.IsType<MusicWait>(session.Current.Story.Wait);
        Assert.NotNull(Send(session, new WaitForText(session.Current.Story.Wait!.Token)).Failure);
        Accept(session, new CompleteMusic(music.Generation, music.Cue));
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 600));
        var previous = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Accept(session, new CompletePresentation(previous.Token, previous.Cue.Kind));
        var input = Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Assert.True(session.Current.CanWaitForText);
        var ready = session.Current;
        Accept(session, new WaitForText(input.Token));
        Assert.Equal(ready.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal(ready.Story.RandomSeedCopy, session.Current.Story.RandomSeedCopy);
        var polled = session.Current;
        Accept(session, new Acknowledge(input.Token));
        Assert.Equal(polled.Story.SimulationTick, session.Current.Story.SimulationTick);
        for (int i = 0; session.Current.Story.Wait is not null && i < 100; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.False(session.Current.Story.LogicalText!.Open);
        Assert.True(session.Current.Story.SimulationTick >= polled.Story.SimulationTick + 10);
    }

    [Fact]
    public void GenerationsRejectStaleDuplicateAndInterferingCompletions()
    {
        var session = StartMusic(0);
        var music = session.Current.Story.Music!;
        var snapshot = session.Current;
        Assert.NotNull(Send(session, new CompleteMusic(music.Generation - 1, music.Cue)).Failure);
        Assert.NotNull(Send(session, new CompleteMusic(music.Generation, "town")).Failure);
        Assert.Same(snapshot, session.Current);
        Accept(session, new CompleteMusic(music.Generation, music.Cue));
        Assert.IsType<MusicWait>(session.Current.Story.Wait);
        var received = session.Current;
        Assert.NotNull(Send(session, new CompleteMusic(music.Generation, music.Cue)).Failure);
        Assert.Same(received, session.Current);
        var error = Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() =>
            ExplorationMusicRunner.Request(session.Definition.Exploration!, session.Current, "town", new(991)));
        Assert.Equal("music-helper-interference", error.Code);
    }

    [Fact]
    public void DuplicateCurrentCuePreservesProgressAndPriorReplacementStartsNewGeneration()
    {
        var session = StartMusic(0, "ABC");
        DrainTextWork(session);
        var requested = session.Current.Story.Music!;
        Accept(session, new CompleteMusic(requested.Generation, requested.Cue));
        var current = session.Current;
        var music = current.Story.Music!;
        Assert.True(music.Step > 0);
        Assert.True(music.ActualDone);
        var same = ExplorationMusicRunner.Request(session.Definition.Exploration!, current, music.Cue, new(998));
        Assert.Same(music, same);
        var other = ExplorationMusicRunner.Request(session.Definition.Exploration!, current, "town", new(999))!;
        Assert.Equal(999, other.Generation);
        Assert.Null(other.EndStep);
        var replaced = current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait, music: other));
        var restarted = ExplorationMusicRunner.Request(session.Definition.Exploration!, replaced, "join", new(1000))!;
        Assert.Equal(0, restarted.Step);
        Assert.Equal(1000, restarted.Generation);
        Assert.False(restarted.ActualDone);
        var reject = ExplorationDispatcher.Submit(session.Definition,
            replaced.WithStory(replaced.Story.Copy(replaced.Story.Cursor, replaced.Story.Wait, music: restarted)),
            new CompleteMusic(music.Generation, music.Cue));
        Assert.Equal("stale-or-wrong-music", reject.Failure!.Code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrdinaryPreviousRestoresHistoryBeforeDuplicateAndFiniteRequest(bool looping)
    {
        var session = StartMusic(0, edit: doc =>
        {
            var audio = doc["world"]!["presentation"]!["audio"]!.AsArray();
            var ordinary = audio[0]!.DeepClone();
            ordinary["cue"] = "ordinary"; ordinary["command"] = 9;
            if (looping) { ordinary["loopBegin"] = 0; ordinary["loopEnd"] = 2; }
            audio.Add(ordinary);
            var instructions = doc["world"]!["programs"]![0]!["instructions"]!.AsArray();
            instructions.Insert(1, Cue("Sound", "ordinary"));
            instructions.Insert(2, Cue("PreviousMusic"));
            instructions.Insert(3, Cue("Sound", "town"));
        });
        var previous = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(PresentationCueKind.PreviousMusic, previous.Cue.Kind);
        var restored = session.Current.Story.Music!;
        Assert.Equal("town", restored.Cue);
        Assert.Empty(restored.Previous);
        Assert.Equal(previous.Token.Value, restored.Generation);
        Assert.Null(restored.EndStep);
        long tick = session.Current.Story.SimulationTick;
        Accept(session, new CompletePresentation(previous.Token, previous.Cue.Kind));
        var duplicate = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal("town", duplicate.Cue.Resource);
        Assert.Same(restored, session.Current.Story.Music);
        Accept(session, new CompletePresentation(duplicate.Token, duplicate.Cue.Kind));
        var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        var finite = session.Current.Story.Music!;
        Assert.Equal("join", finite.Cue);
        Assert.Equal(new[] { "town" }, finite.Previous);
        Assert.Equal(0, finite.Step);
        Accept(session, new CompletePresentation(sound.Token, sound.Cue.Kind));
        Accept(session, new CompleteMusic(finite.Generation, finite.Cue));
        Assert.IsType<MusicWait>(session.Current.Story.Wait);
        Assert.Equal(tick, session.Current.Story.SimulationTick);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 600));
        Assert.Equal(tick + 507, session.Current.Story.SimulationTick);
        Assert.Equal("town", session.Current.Story.Music!.Cue);
        Assert.Empty(session.Current.Story.Music!.Previous);
        Assert.Equal(PresentationCueKind.PreviousMusic, Assert.IsType<PresentationWait>(session.Current.Story.Wait).Cue.Kind);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ContextRoundTripInvalidatesHistoryUntilANewRequest(bool areaOnly, bool requestAgain)
    {
        var session = StartMusic(0, edit: doc =>
        {
            var world = doc["world"]!;
            var audio = world["presentation"]!["audio"]!.AsArray();
            var ordinary = audio[0]!.DeepClone();
            ordinary["cue"] = "ordinary"; ordinary["command"] = 9;
            ordinary["loopBegin"] = 0; ordinary["loopEnd"] = 2; audio.Add(ordinary);
            string origin = world["maps"]![0]!["id"]!.GetValue<string>();
            string target = areaOnly ? origin : world["maps"]![1]!["id"]!.GetValue<string>();
            foreach (var map in world["maps"]!.AsArray()) { map!["onLoad"] = null; map["battle"] = null; }
            if (areaOnly)
            {
                var areas = world["maps"]![0]!["areas"]!.AsArray();
                var second = areas[0]!.DeepClone(); second["minX"] = 16;
                areas[0]!["maxX"] = 15; areas.Add(second);
            }
            foreach (var visual in world["presentation"]!["maps"]!.AsArray())
                visual!["music"] = JsonNode.Parse("""[{"field":8,"battle":8},{"field":8,"battle":8}]""");
            JsonNode Transfer(string map, int x) => JsonSerializer.SerializeToNode(new
                { op = "transfer", map, position = new { x, y = 1 }, facing = 1, loadMode = "rebuild" })!;
            var instructions = new JsonArray(Cue("Sound", "town"), Cue("Sound", "ordinary"),
                JsonNode.Parse("""{"op":"wait-ticks","ticks":1}"""),
                Transfer(target, areaOnly ? 20 : 1), Transfer(origin, 1));
            if (requestAgain) { instructions.Add(Cue("Sound", "town")); instructions.Add(Cue("Sound", "join")); }
            else instructions.Add(Cue("PreviousMusic"));
            instructions.Add(JsonNode.Parse("""{"op":"end"}"""));
            world["programs"]![0]!["instructions"] = instructions;
        });
        var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(2, result.Observations.Count(o => o.Kind == "map-transferred"));
        if (!requestAgain)
        {
            Assert.Equal("music-context-replaced", result.Failure!.Code);
            Assert.False(session.Current.Story.Music!.HistoryBound);
            return;
        }
        Assert.Null(result.Failure);
        var reset = session.Current.Story.Music!;
        Assert.True(reset.HistoryBound);
        Assert.Equal("town", reset.Cue);
        Assert.Empty(reset.Previous);
        Assert.Null(reset.EndStep);
        var establish = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Accept(session, new CompletePresentation(establish.Token, establish.Cue.Kind));
        var music = session.Current.Story.Music!;
        Assert.True(music.HistoryBound);
        Assert.Equal("join", music.Cue);
        Assert.Equal(new[] { "town" }, music.Previous);
        Assert.Equal(0, music.Step);
        var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Accept(session, new CompletePresentation(sound.Token, sound.Cue.Kind));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Same(music, session.Current.Story.Music);
    }

    private static GameSession StartMusic(int ticks, string? text = null, int speed = 2, int width = 6,
        string name = "Name", bool enabled = false, int command = 19, int end = 505, Action<JsonNode>? edit = null)
    {
        var session = StartFieldText(text ?? "A", speed: speed, width: width, name: name, enabled: enabled, npcRandom: true, configure: doc =>
        {
            byte[] pcm = [0, 0, 0, 0];
            object Audio(string cue, int id, int? step) => new { cue, command = id, timerB = 192, sampleRate = 8000,
                channels = 1, sampleFrames = 2, pcm16 = Convert.ToBase64String(pcm), sha256 = Convert.ToHexString(SHA256.HashData(pcm)),
                loopBegin = (int?)null, loopEnd = (int?)null, modernEndStep = step };
            var town = JsonSerializer.SerializeToNode(Audio("town", 8, null))!;
            town.AsObject().Remove("modernEndStep");
            doc["world"]!["presentation"]!["audio"] = new JsonArray(town, JsonSerializer.SerializeToNode(Audio("join", command, end)));
            var instructions = new JsonArray(Cue("Sound", "town"), Cue("Sound", "join"));
            if (ticks > 0) instructions.Add(JsonNode.Parse($$"""{"op":"wait-ticks","ticks":{{ticks}}}"""));
            if (text is not null)
            {
                instructions.Add(JsonNode.Parse("""{"op":"text-cursor","text":100}"""));
                instructions.Add(JsonNode.Parse("""{"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false}"""));
            }
            instructions.Add(Cue("SoundWait")); instructions.Add(Cue("PreviousMusic"));
            instructions.Add(JsonNode.Parse("""{"op":"wait-text-input"}"""));
            instructions.Add(JsonNode.Parse("""{"op":"close-text"}"""));
            instructions.Add(JsonNode.Parse("""{"op":"wait-ticks","ticks":10}"""));
            instructions.Add(JsonNode.Parse("""{"op":"end"}"""));
            doc["world"]!["programs"]![0]!["instructions"] = instructions;
            edit?.Invoke(doc);
        });
        for (int i = 0; i < 2; i++)
        {
            var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
            Assert.NotNull(Send(session, new AdvanceSimulation(sound.Token)).Failure);
            Accept(session, new CompletePresentation(sound.Token, sound.Cue.Kind));
        }
        if (ticks > 0) Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, ticks));
        return session;
    }

    private static JsonNode Cue(string kind, string? resource = null) => JsonSerializer.SerializeToNode(new
        { op = "present", kind, resource, entity = (string?)null, position = (object?)null })!;
}

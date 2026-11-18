using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class FiniteAudioSelectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExactWinsAndExplicitDefaultSurvivesAnotherVariantAndOrder(bool reverse)
    {
        var rows = new[] { Sound("default", 116, 189, 116), Sound("exact", 116, 194) };
        Assert.Equal("default", Select(Read(rows), 116, 203));
        rows = [.. rows, Sound("another", 116, 210)];
        if (reverse) Array.Reverse(rows);
        var audio = Read(rows);
        Assert.Equal("exact", Select(audio, 116, 194));
        Assert.Equal("default", Select(audio, 116, 203));
        Assert.Equal("default", Select(audio, 116, null));
        Assert.Equal(189, audio["default"].TimerB);
        Assert.Equal(8000, audio["default"].SampleRate);
        Assert.Equal(new byte[] { 1, 0, 2, 0 }, audio["default"].CopyPcm());
    }

    [Fact]
    public void OldPackagesKeepUniqueFiniteFallbackAndRejectMissingOrAmbiguousCommands()
    {
        var audio = Read(Sound("old", 67, 189));
        Assert.Equal("old", Select(audio, 67, 194));
        Assert.Equal("audio-command-unavailable", Assert.Throws<InvalidOperationException>(() => Select(audio, 116, 194)).Message);
        audio = Read(Sound("old", 67, 189), Sound("other", 67, 203));
        Assert.Equal("old", Select(audio, 67, 189));
        Assert.Equal("audio-command-ambiguous", Assert.Throws<InvalidOperationException>(() => Select(audio, 67, 194)).Message);
        var loop = Sound("loop", 67, 189); loop["loopBegin"] = 0; loop["loopEnd"] = 2;
        Assert.Equal("audio-command-unavailable", Assert.Throws<InvalidOperationException>(() => Select(Read(loop), 67, 194)).Message);
    }

    [Fact]
    public void ReaderRejectsDuplicateDefaultsWrongCommandAndLoopingDefault()
    {
        Reject("audio-default-duplicate", Sound("a", 116, 189, 116), Sound("b", 116, 194, 116));
        Reject("audio-default-resource", Sound("wrong", 116, 189, 67));
        var loop = Sound("loop", 116, 189, 116); loop["loopBegin"] = 0; loop["loopEnd"] = 2;
        Reject("audio-default-resource", loop);
        Reject("audio-default-resource", Sound("music", 1, 189, 116));
    }

    private static string Select(IReadOnlyDictionary<string, ExplorationAudio> audio, int command, int? timer) =>
        ExplorationAudio.SelectCue(audio, command, timer);

    private static JsonObject Sound(string cue, int command, int timer, int? defaultFor = null)
    {
        byte[] pcm = [1, 0, 2, 0];
        var row = new JsonObject
        {
            ["cue"] = cue, ["command"] = command, ["timerB"] = timer,
            ["sampleRate"] = 8000, ["channels"] = 1, ["sampleFrames"] = 2,
            ["pcm16"] = Convert.ToBase64String(pcm), ["sha256"] = Convert.ToHexString(SHA256.HashData(pcm)),
            ["loopBegin"] = null, ["loopEnd"] = null,
        };
        if (defaultFor is not null) row["defaultForCommand"] = defaultFor.Value;
        return row;
    }

    private static JsonNode WithAudio(JsonObject[] rows)
    {
        var doc = Document("scene-music-context");
        doc["world"]!["presentation"]!["audio"] = new JsonArray(rows.Select(row => row.DeepClone()).ToArray());
        return doc;
    }

    private static IReadOnlyDictionary<string, ExplorationAudio> Read(params JsonObject[] rows) =>
        Assert.IsType<ExplorationReadAccepted>(Reader(WithAudio(rows)).Read()).Definition.Exploration!.Visuals!.Audio;

    private static void Reject(string code, params JsonObject[] rows) =>
        Assert.Equal(code, Assert.IsType<ScenarioReadRejected>(Reader(WithAudio(rows)).Read()).Failure.Code);
}

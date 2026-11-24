using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class MapInitializationMusicTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DestinationSetupHandsMusicBackAtItsReturn(bool revealsItself, bool sameDefault)
    {
        var session = Arrival(revealsItself ? """
            {"op":"call","target":{"program":"reveal","instruction":0},"activateEntities":false},
            """ : "", sameDefault);
        var original = session.Current.Story.Music!;
        var loading = CompleteCue(session);
        Assert.Equal("yard-map", loading.Snapshot.Exploration!.Map.Value);
        Assert.True(loading.Snapshot.Story.MapMusicPending);
        Assert.Equal(1, loading.Snapshot.Story.Music!.Area);
        Assert.Equal(original.Generation, loading.Snapshot.Story.Music.Generation);
        Assert.DoesNotContain(loading.Observations, row => row.Kind == "field-map-music-selected");

        var resumed = Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        if (revealsItself)
        {
            Assert.True(resumed.Snapshot.Story.MapMusicPending);
            Assert.DoesNotContain(resumed.Observations, row => row.Kind == "field-map-music-selected");
            var sound = CompleteCue(session);
            Assert.Equal(original.Generation, sound.Snapshot.Story.Music!.Generation);
            var returned = CompleteFade(session);
            Assert.Single(returned.Observations, row => row.RetainedFieldMusicContext?.Map.Value == "yard-map");
            Assert.DoesNotContain(returned.Observations, row => row.Kind == "field-map-music-selected");
            Assert.Equal(original.Generation, returned.Snapshot.Story.Music!.Generation);
            Assert.Empty(returned.Snapshot.Story.Music.Previous);
        }
        else
        {
            string expected = sameDefault ? "area-a" : "area-b";
            Assert.Single(resumed.Observations, row => row.Kind == "field-map-music-selected" && row.Detail == expected);
            Assert.Equal(expected, resumed.Snapshot.Story.Music!.Cue);
            if (sameDefault)
            {
                Assert.Equal(original.Generation, resumed.Snapshot.Story.Music.Generation);
                Assert.Empty(resumed.Snapshot.Story.Music.Previous);
            }
            else
            {
                Assert.NotEqual(original.Generation, resumed.Snapshot.Story.Music.Generation);
                Assert.Equal(new[] { "area-a" }, resumed.Snapshot.Story.Music.Previous);
            }
        }
        Assert.False(session.Current.Story.MapMusicPending);
    }

    [Fact]
    public void FiniteSetupCueReturnsToTheMusicCarriedAcrossTheLoad()
    {
        var session = Arrival("""
            {"op":"present","kind":"Sound","resource":"finite","entity":null,"position":null},
            {"op":"present","kind":"SoundWait","resource":null,"entity":null,"position":null},
            {"op":"present","kind":"PreviousMusic","resource":null,"entity":null,"position":null},
            {"op":"present","kind":"FadeIn","resource":"black","entity":null,"position":null,"fullBlack":{"period":null}},
            """);
        CompleteCue(session);
        var requested = Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var music = requested.Snapshot.Story.Music!;
        Assert.Equal("finite", music.Cue);
        Assert.Equal(new[] { "area-a" }, music.Previous);
        Assert.Equal(3, music.EndStep);

        CompleteCue(session);
        var wait = Assert.IsType<MusicWait>(session.Current.Story.Wait);
        Accept(session, new AdvanceSimulation(wait.Token, 6));
        Accept(session, new CompleteMusic(music.Generation, music.Cue));
        var previous = CompleteCue(session);
        Assert.Equal("area-a", previous.Snapshot.Story.Music!.Cue);
        Assert.Empty(previous.Snapshot.Story.Music.Previous);
        var returned = CompleteFade(session);
        Assert.Single(returned.Observations, row => row.Kind == "field-map-music-retained");
        Assert.DoesNotContain(returned.Observations, row => row.Kind == "field-map-music-selected");
    }

    private static SessionResult CompleteCue(GameSession session)
    {
        var wait = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        return Accept(session, new CompletePresentation(wait.Token, wait.Cue.Kind));
    }

    private static SessionResult CompleteFade(GameSession session)
    {
        var fade = Assert.IsType<FullFadeWait>(session.Current.Story.Wait);
        Accept(session, new CompletePresentation(fade.Token, fade.Kind));
        return Accept(session, new AdvanceSimulation(fade.Token, 60));
    }

    private static GameSession Arrival(string setup, bool sameDefault = false) => Start("scene-music-context", document =>
    {
        var world = document["world"]!;
        if (sameDefault) world["presentation"]!["maps"]![1]!["music"]![1]!["field"] = 8;
        world["maps"]![1]!["onLoad"] = JsonNode.Parse("""{"program":"destination","instruction":0}""");
        world["programs"] = JsonNode.Parse($$$"""
            [
              {"id":"music-scene","instructions":[
                {"op":"present","kind":"Sound","resource":"area-a","entity":null,"position":null},
                {"op":"transfer","map":"yard-map","position":{"x":20,"y":1},"facing":1,"loadMode":"rebuild"},
                {"op":"wait-ticks","ticks":2},
                {"op":"end"}
              ]},
              {"id":"destination","instructions":[
                {"op":"wait-ticks","ticks":1},
                {{{setup}}}
                {"op":"end"}
              ]},
              {"id":"reveal","instructions":[
                {"op":"present","kind":"Sound","resource":"area-a","entity":null,"position":null},
                {"op":"present","kind":"FadeIn","resource":"black","entity":null,"position":null,"fullBlack":{"period":null}},
                {"op":"end"}
              ]}
            ]
            """);
    });
}

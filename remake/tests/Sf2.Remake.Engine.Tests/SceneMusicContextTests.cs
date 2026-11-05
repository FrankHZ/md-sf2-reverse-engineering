using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class SceneMusicContextTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, -3)]
    [InlineData(true, 1)]
    [InlineData(true, 7)]
    public void SceneViewOwnsFiniteHistoryAndCompletionWithRetainedActors(bool offArea, int scroll)
    {
        var session = Scene(offArea, scroll);
        var loaded = Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(1, ExplorationViewContext.Resolve(loaded.Snapshot.Exploration!, loaded.Snapshot.Story.LogicalView).Area);
        Assert.Equal(offArea, loaded.Snapshot.Exploration!.Definition.Traversal
            .SelectActiveArea(loaded.Snapshot.Exploration.PlayerEntity.Position) is null);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var music = session.Current.Story.Music!;
        Assert.Equal(1, music.Area);
        Assert.Equal("finite", music.Cue);
        Assert.Equal(new[] { "area-b" }, music.Previous);
        Assert.Equal(scroll, session.Current.Story.LogicalView!.Area.AutoscrollAX);
        var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Accept(session, new CompletePresentation(sound.Token, sound.Cue.Kind));
        Assert.IsType<MusicWait>(session.Current.Story.Wait);
        var before = session.Current;
        Assert.Equal("stale-or-wrong-music", Send(session, new CompleteMusic(music.Generation - 1, music.Cue)).Failure!.Code);
        Assert.Same(before, session.Current);
        Accept(session, new AdvanceSimulation(before.Story.Wait!.Token, 6));
        Assert.True(Assert.IsType<MusicWait>(session.Current.Story.Wait).LogicalDone);
        Assert.False(session.Current.Story.Music!.ActualDone);
        Accept(session, new CompleteMusic(music.Generation, music.Cue));
        Assert.Equal("area-b", session.Current.Story.Music!.Cue);
        Assert.Empty(session.Current.Story.Music.Previous);
        Assert.Equal(1, session.Current.Story.Music.Area);
        var previous = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Accept(session, new CompletePresentation(previous.Token, previous.Cue.Kind));
        Assert.IsType<TickWait>(session.Current.Story.Wait);
        Assert.NotNull(Send(session, new CompleteMusic(music.Generation, music.Cue)).Failure);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnresolvedViewFailsBeforeMusicOrWaitPublication(bool ambiguous)
    {
        var session = Scene(false, 0);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var current = session.Current;
        var view = current.Story.LogicalView!;
        if (ambiguous)
        {
            // Content rejects duplicate traversal bounds. Exercise the runtime guard with
            // immutable view data that has ambiguous identity after scroll normalization.
            var world = current.Exploration!;
            var map = world.Definition;
            var duplicate = view.Area with { AutoscrollAX = 9 };
            var definition = new ExplorationMapDefinition(map.Map, map.Layout, map.Traversal, map.Entities, map.Events,
                viewAreas: map.ViewAreas.Append(duplicate));
            var changed = new ExplorationState(definition, world.Layout, world.Player, world.AllEntities, world.Party);
            current = new(current.SessionId, current.Revision, current.ObservationSequence,
                new ActiveExploration(changed), current.Story, current.StopReason);
        }
        if (!ambiguous) view = view with { Area = view.Area with { ForegroundY = 33 } };
        var entry = current.WithStory(current.Story.Copy(new("music-scene", 4), logicalView: view));
        var result = ProgramRunner.Run(session.Definition, entry, []);
        Assert.Equal(ambiguous ? "map-music-area-ambiguous" : "map-music-area-unavailable", result.Failure!.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure.Kind);
        Assert.Empty(result.Observations);
        Assert.Null(result.Snapshot.Story.Music);
        Assert.Null(result.Snapshot.Story.Wait);
        Assert.Equal(entry.Revision, result.Snapshot.Revision);
    }

    [Fact]
    public void InvalidatedViewCannotPublishFiniteServiceOrRevivePlayerContext()
    {
        var session = Scene(false, 0);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Accept(session, new CompletePresentation(sound.Token, sound.Cue.Kind));
        var current = session.Current;
        var view = current.Story.LogicalView!;
        var entry = current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait,
            logicalView: view with { Area = view.Area with { ForegroundY = 33 } }));
        var result = ExplorationDispatcher.Submit(session.Definition, entry, new AdvanceSimulation(entry.Story.Wait!.Token));
        Assert.Equal("map-music-area-unavailable", result.Failure!.Code);
        Assert.Empty(result.Observations);
        Assert.Same(entry.Story.Music, result.Snapshot.Story.Music);
        Assert.Equal(entry.Story.SimulationTick, result.Snapshot.Story.SimulationTick);
    }

    [Fact]
    public void OnlyUnboundViewUsesPlayerAreaAndMissingPlayerAreaIsTyped()
    {
        var session = Scene(false, 0);
        var world = session.Current.Exploration!;
        Assert.Equal(0, ExplorationViewContext.Resolve(world, null).Area);
        // Real unbound authored programs continue to establish map music from the player.
        var unbound = session.Current.WithStory(new StoryState([], new("music-scene", 4), textSettings: session.Current.Story.TextSettings));
        var requested = ProgramRunner.Run(session.Definition, unbound, []);
        Assert.Null(requested.Failure);
        Assert.Equal(0, requested.Snapshot.Story.Music!.Area);
        Assert.Equal(new[] { "area-a" }, requested.Snapshot.Story.Music.Previous);

        var offArea = Scene(true, 0);
        Accept(offArea, new AdvanceSimulation(offArea.Current.Story.Wait!.Token));
        var missing = offArea.Current.WithStory(new StoryState([], new("music-scene", 4), textSettings: session.Current.Story.TextSettings));
        var result = ProgramRunner.Run(offArea.Definition, missing, []);
        Assert.Equal("map-music-area-unavailable", result.Failure!.Code);
        Assert.Null(result.Snapshot.Story.Music);
        Assert.Empty(result.Observations);
    }

    [Fact]
    public void SaveObservationKeepsStableBoundaryBeforeResolvingMusic()
    {
        var session = Scene(true, 1);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var before = session.Current;
        var observed = OpeningSave.ObserveMusic(session.Definition, before,
            new(before.Exploration!.Map, 1, new("area-b", [])));
        Assert.Equal("save-busy", observed.Failure!.Code);
        Assert.Same(before, observed.Snapshot);
        Assert.Null(observed.Snapshot.Story.Music);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SettledMusicObservationUsesViewButStillRejectsOffAreaPlayer(bool offArea)
    {
        var session = Scene(offArea, 0);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var before = session.Current;
        var display = before.Story.Display!;
        var stable = new SessionSnapshot(before.SessionId, before.Revision, before.ObservationSequence,
            before.Active, before.Story.Copy(null, display: display with
                { Current = display.Base, Visibility = FullFadeVisibility.BaseRestored }), SessionStopReason.PlayerInput);
        Assert.True(stable.CanWaitAtInput);
        var observed = OpeningSave.ObserveMusic(session.Definition, stable,
            new(stable.Exploration!.Map, 1, new("area-b", [])));
        if (offArea)
        {
            Assert.Equal("save-player-unsettled", observed.Failure!.Code);
            Assert.Same(stable, observed.Snapshot);
        }
        else
        {
            Assert.Null(observed.Failure);
            Assert.Equal(1, observed.Snapshot.Story.Music!.Area);
            Assert.Equal("area-b", observed.Snapshot.Story.Music.Cue);
            var wrong = OpeningSave.ObserveMusic(session.Definition, stable,
                new(stable.Exploration.Map, 0, new("area-a", [])));
            Assert.Equal("save-music-observation", wrong.Failure!.Code);
            Assert.Same(stable, wrong.Snapshot);
        }
    }

    private static GameSession Scene(bool offArea, int scroll) => Start("scene-music-context", document =>
    {
        var areas = document["world"]!["maps"]![1]!["areas"]!.AsArray();
        if (offArea) areas[0]!["minY"] = 4;
        document["world"]!["programs"]![0]!["instructions"]![2]!["value"] = scroll;
    });
}

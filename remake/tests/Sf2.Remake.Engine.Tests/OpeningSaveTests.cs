using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class OpeningSaveTests
{
    [Fact]
    public void SameContextUnknownMusicNeedsObservedHistoryAndCannotAssumeTheDefault()
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var known = session.Current;
        var unknown = known.WithStory(known.Story.Copy(null, music: known.Story.Music! with { HistoryBound = false }));
        Assert.Equal("save-music-history-unknown", OpeningSave.Query(definition, unknown, session.Rules).Error);
        var observed = OpeningSave.ObserveMusic(definition, unknown, new(new("map-3"), 0, new("previous", ["field"])));
        Assert.Null(observed.Failure);
        Assert.Equal(unknown.Revision + 1, observed.Snapshot.Revision);
        Assert.Same(unknown.Active, observed.Snapshot.Active);
        Assert.Equal(unknown.Story.SimulationTick, observed.Snapshot.Story.SimulationTick);
        var captured = OpeningSave.Capture(definition, observed.Snapshot, session.Rules,
            observed.Snapshot.SessionId, observed.Snapshot.Revision);
        Assert.Null(captured.Error); Assert.Equal("previous", captured.Data!.Music.Cue);
        Assert.Equal(new[] { "field" }, captured.Data.Music.Previous);
        Assert.Equal("save-music-disagreement", OpeningSave.ObserveMusic(definition, known,
            new(new("map-3"), 0, new("previous", []))).Failure!.Code);
        Assert.Same(known, session.Current);
        var timed = known.WithStory(known.Story.Copy(null, music: known.Story.Music! with
            { EndStep = 20, Step = 20, ActualDone = true, PreviousEligible = true, DeliverySkipped = false }));
        var enriched = OpeningSave.ObserveMusic(definition, timed, new(new("map-3"), 0, new("field", ["previous"])));
        Assert.Null(enriched.Failure);
        Assert.Equal(timed.Story.Music, enriched.Snapshot.Story.Music! with { Previous = timed.Story.Music!.Previous });
        Assert.Equal(timed.Exploration!.Party.MainSeed, enriched.Snapshot.Exploration!.Party.MainSeed);
    }

    [Fact]
    public void AmbientDefaultWalkingCanResetButStoryInstalledWorkCannotBeSaved()
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var current = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session.Current;
        var world = current.Exploration!;
        var npc = world.AllEntities.First(entity => entity.Slot != world.PlayerEntity.Slot);
        var defaultProgram = world.Definition.Entities.First(row => row.Actions is not null).Actions!;
        var resumed = npc with { Actions = new(defaultProgram.Actions.ToArray()) };
        var ambient = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithEntity(resumed)), current.Story, current.StopReason);
        Assert.True(OpeningSave.Query(definition, ambient, RuleCompositions.Sf2()).Allowed);
        var wander = new EntityActionProgram([new FaceEntity(1), new RandomWalkEntity(new(40, 8), 2),
            new WaitEntityTicks(10), new JumpEntityAction(1)]);
        SessionSnapshot At(int cursor) => new(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithEntity(npc with { Actions = wander, ActionCursor = cursor })), current.Story, current.StopReason);
        Assert.Equal("save-story-motion", OpeningSave.Query(definition, At(0), RuleCompositions.Sf2()).Error);
        Assert.True(OpeningSave.Query(definition, At(2), RuleCompositions.Sf2()).Allowed);
        var commanded = npc with { Actions = new([new WaitEntityTicks(20), new IdleEntityAction()]) };
        var busy = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithEntity(commanded)), current.Story, current.StopReason);
        Assert.Equal("save-story-motion", OpeningSave.Query(definition, busy, RuleCompositions.Sf2()).Error);
        var completed = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithEntity(commanded with { ActionCursor = 1 })), current.Story, current.StopReason);
        Assert.True(OpeningSave.Query(definition, completed, RuleCompositions.Sf2()).Allowed);
    }

    [Fact]
    public void CaptureIsDetachedAndStaleRevisionCannotCaptureNewProgress()
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var before = session.Current;
        var offer = session.QuerySave(); Assert.True(offer.Allowed, offer.Error);
        var capture = session.CaptureSave(offer.SessionId, offer.Revision);
        Assert.Null(capture.Error); Assert.Same(before, session.Current);
        Assert.NotSame(before.Story.Flags, capture.Data!.Flags);
        Assert.NotSame(before.Exploration!.Party.Actors[0].SourceLoadout!.Items, capture.Data.Allies[0].Items);
        EngineTestContent.Accept(session, new Move(ExplorationDirection.South));
        Assert.Equal("save-stale-revision", session.CaptureSave(offer.SessionId, offer.Revision).Error);
        Assert.False(session.QuerySave().Allowed);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void BusyStoryAndPresentationRejectWithoutConsumingProgress(int kind)
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var current = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session.Current;
        var story = kind switch
        {
            0 => current.Story.Copy(new("dangerous-onload", 0)),
            1 => current.Story.Copy(null, new ChoiceWait(new(1), 89)),
            2 => current.Story.Copy(null, warp: new(new("map-3"), new(1, 1), 3, Sf2.Remake.Application.Content.Scenarios.MapLoadMode.Rebuild)),
            3 => current.Story.Copy(null, portraitWindow: new OpenPortraitWindow(1, 0)),
            4 => current.Story.Copy(null, battleRequest: true),
            _ => current.Story.Copy(null, logicalView: current.Story.LogicalView! with { AX = new(0, 128, 1) }),
        };
        var busy = current.WithStory(story);
        Assert.False(OpeningSave.Query(definition, busy, RuleCompositions.Sf2()).Allowed);
        Assert.Equal(data.MainSeed, busy.Exploration!.Party.MainSeed);
        Assert.True(OpeningSave.Query(definition, current, RuleCompositions.Sf2()).Allowed);
    }

    [PrivateInputFact("SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD",
        "SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN")]
    public void RealFreshOpeningCanCaptureAndRestoreWithoutNewGameReplay()
    {
        string Input(string key) => PrivateInputFactAttribute.RequireInput(key);
        var reader = new OpeningNewGameReader(Input("SF2_PRIVATE_EXPLORATION_CONTENT"), Input("SF2_PRIVATE_BATTLE01_DATA"),
            Input("SF2_PRIVATE_BATTLE01_SCENE"), Input("SF2_PRIVATE_BATTLE01_TERRAIN"), Input("SF2_PRIVATE_STATIC_DATA"),
            Input("SF2_PRIVATE_ENEMY_DATA"), Input("SF2_PRIVATE_ENEMY_GOLD"), 0, uint.MaxValue);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(reader)).Session;
        var captured = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(captured.Error); Assert.NotNull(captured.Data);
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition, captured.Data, session.Rules)).Session;
        Assert.Equal(session.Current.Exploration!.Party.Actors.Count, restored.Current.Exploration!.Party.Actors.Count);
        Assert.Equal(session.Current.Exploration.Party.MainSeed, restored.Current.Exploration.Party.MainSeed);
        Assert.Equal(session.Current.Story.Flags, restored.Current.Story.Flags);
        var saved = captured.Data!;
        var unarmed = saved with { Allies = saved.Allies.Select(ally => ally.Actor == "ally-0"
            ? ally with { Items = ally.Items.Select((word, slot) => (ushort)((word & ~128) | (slot == 1 ? 256 : 0))).ToArray() } : ally).ToArray() };
        var changed = Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition, unarmed, session.Rules)).Session;
        var actor = changed.Current.Exploration!.Party.Actors.Single(a => a.Actor.Value == "ally-0");
        var baseline = changed.Definition.Encounters.Values.First().Deployments.Single(row => row.Actor == actor.Actor).Definition;
        Assert.False(baseline.Equipment!.AllowChanges);
        Assert.Equal((ushort)256, actor.SourceLoadout!.Items[1]);
        Assert.Equal(actor.Progress!.BaseAttack, BattleEquipment.Attack(baseline, actor.Progress, actor.SourceLoadout));
        var armed = restored.Current.Exploration!.Party.Actors.Single(a => a.Actor == actor.Actor);
        Assert.True(BattleEquipment.Attack(baseline, armed.Progress, armed.SourceLoadout) > actor.Progress.BaseAttack);
    }
}

using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSaveEligibilityTests
{
    [Fact]
    public void QueryCaptureAndRejectedStaleRequestsArePureAndDetached()
    {
        var (definition, current) = BattleSaveFixture.Create();
        var rules = RuleCompositions.Sf2();
        var offer = BattleSave.Query(definition, current, rules); Assert.True(offer.Allowed, offer.Error);
        var first = BattleSaveFixture.Capture(definition, current);
        var second = BattleSaveFixture.Capture(definition, current);
        Assert.Equal(current.Revision, offer.Revision); Assert.Equal(current.SessionId, offer.SessionId);
        Assert.Equal(current.Battle.MainSeed, first.Battle.MainSeed); Assert.Equal(current.Battle.ThinkingSeed, second.Battle.ThinkingSeed);
        Assert.NotSame(current.Story.Flags, first.Story.Flags); Assert.NotSame(first.Story.Flags, second.Story.Flags);
        Assert.NotSame(current.Battle.Actors[0].SourceLoadout!.Items, first.Battle.Actors[0].Loadout!.Items);
        Assert.NotSame(current.Story.FieldPlayer!.RetainedAllies![0].SourceLoadout!.Items, first.Story.Player.RetainedAllies[0].SourceLoadout!.Items);
        Assert.Equal("save-stale-revision", BattleSave.Capture(definition, current, rules, Guid.NewGuid(), current.Revision).Error);
        Assert.Equal("save-stale-revision", BattleSave.Capture(definition, current, rules, current.SessionId, current.Revision - 1).Error);
        Assert.False(OpeningSave.Query(definition, current, rules).Allowed);
    }

    [Fact]
    public void DeliveredMovementBackToOriginQualifiesButPreviewAndUndoFlightDoNot()
    {
        var (definition, current) = BattleSaveFixture.Create();
        var session = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, BattleSaveFixture.Capture(definition, current), RuleCompositions.Sf2())).Session;
        var first = Accept(session, new Move(ExplorationDirection.East));
        Assert.Equal("save-battle-movement-busy", session.QueryBattleSave().Error);
        FinishMovement(session, first);
        Assert.Equal("save-battle-cancel-movement", session.QueryBattleSave().Error);
        var back = Accept(session, new Move(ExplorationDirection.West));
        Assert.False(session.QueryBattleSave().Allowed); FinishMovement(session, back);
        Assert.True(session.QueryBattleSave().Allowed, session.QueryBattleSave().Error);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        var cancel = Accept(session, new Cancel());
        Assert.False(session.QueryBattleSave().Allowed); FinishMovement(session, cancel);
        Assert.True(session.QueryBattleSave().Allowed, session.QueryBattleSave().Error);
    }

    [Theory]
    [InlineData(BattleSelectionStage.ActionChoice)] [InlineData(BattleSelectionStage.TargetChoice)]
    [InlineData(BattleSelectionStage.CommitReady)] [InlineData(BattleSelectionStage.EquipmentChoice)]
    [InlineData(BattleSelectionStage.GiveItemChoice)] [InlineData(BattleSelectionStage.GiveTargetChoice)]
    [InlineData(BattleSelectionStage.GiveExchangeChoice)] [InlineData(BattleSelectionStage.GiveCommitReady)]
    [InlineData(BattleSelectionStage.DiscardItemChoice)] [InlineData(BattleSelectionStage.DiscardCommitReady)]
    public void EveryNonMovementSelectionRejects(BattleSelectionStage stage)
    {
        var (definition, current) = BattleSaveFixture.Create();
        var selected = current.Selection!;
        current = new(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveBattle(current.Battle, new(selected.Actor, selected.Preview, stage)), current.Story, current.StopReason);
        Assert.Equal("save-battle-selection-busy", BattleSave.Query(definition, current, RuleCompositions.Sf2()).Error);
    }

    [Theory]
    [InlineData("story")] [InlineData("caller")] [InlineData("wait")] [InlineData("event")]
    [InlineData("warp")] [InlineData("script-warp")] [InlineData("request")] [InlineData("return")]
    [InlineData("text")] [InlineData("portrait")] [InlineData("typewriting")] [InlineData("fade")]
    [InlineData("round")] [InlineData("outcome")] [InlineData("fault")] [InlineData("automatic")]
    [InlineData("music")] [InlineData("membership")] [InlineData("operand")]
    public void BusyOrUnknownStateNeverMutatesTheLiveBoundary(string kind)
    {
        var (definition, original) = BattleSaveFixture.Create();
        var story = original.Story;
        story = kind switch
        {
            "story" => story.Copy(new("empty", 0)), "caller" => story.Copy(null, callers: [new("empty", 0)]),
            "wait" => story.Copy(null, new TickWait(new(4), 1)), "event" => story.Copy(null, eventCaller: new ZoneEventContext()),
            "warp" => story.Copy(null, warp: new(new("map-3"), new(1, 1), 3, MapLoadMode.Rebuild)),
            "script-warp" => story.Copy(null, scriptWarp: new(new("map-3"), new(1, 1), 3, MapLoadMode.Rebuild)),
            "request" => story.Copy(null, battleRequest: true), "return" => story.Copy(null, continuation: ProgramContinuation.OutcomeMapLoaded),
            "text" => story.Copy(null, textWindow: new OpenTextWindow(1, TextDisplayMode.Single, null)),
            "portrait" => story.Copy(null, portraitWindow: new OpenPortraitWindow(1, 0)), "typewriting" => story.Copy(null, typewriting: true),
            "fade" => story.Copy(null, display: story.Display! with { Visibility = Sf2.Remake.Application.Content.Scenarios.FullFadeVisibility.Transitioning }),
            "music" => story.Copy(null, music: story.Music! with { HistoryBound = false }),
            "membership" => story.Copy(null, partyLists: new([], [], [])), _ => story,
        };
        var active = (ActiveBattle)original.Active;
        if (kind == "round") active = active with { Round = new([], 0, new(1)) };
        if (kind == "outcome") active = active with { Outcome = BattleOutcomeKind.Victory };
        if (kind == "operand") active = active with { Selection = new(original.Selection!.Actor, original.Selection.Preview, BattleSelectionStage.Movement, itemSlot: 1) };
        if (kind == "automatic") active = active with { Battle = original.Battle.With(cursor: 2) };
        var current = new SessionSnapshot(original.SessionId, original.Revision, original.ObservationSequence, active, story,
            kind == "fault" ? SessionStopReason.Faulted : original.StopReason);
        var result = BattleSave.Capture(definition, current, RuleCompositions.Sf2(), current.SessionId, current.Revision);
        Assert.NotNull(result.Error); Assert.Null(result.Data); Assert.Same(active, current.Active); Assert.Same(story, current.Story);
        Assert.Equal(original.Battle.MainSeed, current.Battle.MainSeed); Assert.Equal(original.Battle.ThinkingSeed, current.Battle.ThinkingSeed);
    }

    [Theory]
    [InlineData("cursor")] [InlineData("queue")] [InlineData("actor")] [InlineData("position")] [InlineData("region")]
    [InlineData("route")] [InlineData("recipe")] [InlineData("compatibility")] [InlineData("retained")] [InlineData("loadout")]
    public void InvalidSavedCandidatesFailBeforeReturningASession(string kind)
    {
        var (definition, current) = BattleSaveFixture.Create();
        var data = BattleSaveFixture.Capture(definition, current);
        data = kind switch
        {
            "cursor" => data with { Battle = data.Battle with { Cursor = -1 } },
            "queue" => data with { Battle = data.Battle with { Queue = [new(null, 255)] } },
            "actor" => data with { Battle = data.Battle with { Actors = data.Battle.Actors.Skip(1).ToArray() } },
            "position" => data with { Battle = data.Battle with { Actors = data.Battle.Actors.Select((a, i) => i == 0 ? a with { Position = new(30, 0) } : a).ToArray() } },
            "region" => data with { Battle = data.Battle with { Regions = new([], 0) } },
            "route" => data with { Route = new("map-3", 0) }, "recipe" => data with { BattleRecipeRevision = 2 },
            "compatibility" => data with { Compatibility = data.Compatibility with { GameplayRevision = 99 } },
            "retained" => data with { Story = data.Story with { Player = data.Story.Player with { RetainedAllies = [] } } },
            "loadout" => data with { Battle = data.Battle with { Actors = data.Battle.Actors.Select((a, i) => i == 0 ? a with { Loadout = null } : a).ToArray() } },
            _ => throw new InvalidOperationException(),
        };
        Assert.IsType<SessionStartFailed>(GameSession.RestoreBattle(definition, data, RuleCompositions.Sf2()));
        Assert.Equal(27, current.Revision);
    }
}

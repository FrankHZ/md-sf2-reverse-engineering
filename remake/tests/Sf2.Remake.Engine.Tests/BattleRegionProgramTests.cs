using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleRegionProgramTests
{
    internal static SessionStarted Start(IEnumerable<StoryInstruction>? instructions = null, bool inside = true, int[]? flags = null)
    {
        var input = Admitted("hidden-spawns");
        var old = input.Definition.Encounters[input.Start.Encounter];
        var init = old.Initialization!;
        var battle = new BattleDefinition(old.Encounter, old.Map, old.Width, old.Height, old.Terrain,
            old.Deployments, old.Spells.Values, old.Rewards, new(init.Regions, BattleRegionProgram.Authored,
                sourceCombatants: init.SourceCombatants, enemyCount: init.EnemyCount,
                regionPrograms: [new(0, 386, "region"), new(0, 386, "never")]), outcome: old.Outcome);
        var story = new BattleStoryDefinition([
            new("region", instructions ?? [new CallProgram(new("child", 0)), new WriteFlag(400, true), new EndProgram()]),
            new("child", [new ShowText(TextDisplayMode.Single, null, TextId: 1), new ReturnProgram()]),
            new("never", [new WriteFlag(401, true), new EndProgram()])], new Dictionary<int, string> { [1] = "A new ally is listening." });
        var definition = new ScenarioDefinition("round-program", [battle], battleStory: story);
        var start = new BattleStartInput(battle.Encounter, input.Start.Actors.Select(a =>
            a.Actor == HiddenSpawnTests.Leader && inside ? a with { PositionOverride = new(2, 3) } : a),
            input.Start.MainSeed, input.Start.ThinkingSeed, input.Start.Gold, storyFlags: flags);
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, start));
    }

    internal static SessionResult Submit(GameSession session, SessionCommand command) => session.Submit(
        new(session.Current.SessionId, session.Current.Revision, session.Current.Selection?.Actor, command));

    [Fact]
    public void SharedStoryCallWaitAndMatchingResumePreserveBattleAndWrapperPrefix()
    {
        var started = Start(); var session = started.Session; var pending = session.Current;
        Assert.Null(started.Result.Failure); Assert.Equal(SessionMode.Battle, pending.Mode);
        Assert.Equal(0, pending.Battle.Round); Assert.Contains(386, pending.Story.Flags);
        Assert.Equal(new ProgramLocation("child", 0), pending.Story.Cursor);
        Assert.Single(pending.Story.Callers); var wait = Assert.IsType<DialogueWait>(pending.Story.Wait);
        Assert.Equal("stale-or-wrong-wait", Submit(session, new Acknowledge(new(wait.Token.Value + 1))).Failure!.Code);
        Assert.Same(pending, session.Current);
        var done = Submit(session, new Acknowledge(wait.Token)); Assert.Null(done.Failure);
        Assert.Contains(386, done.Snapshot.Story.Flags); Assert.Contains(400, done.Snapshot.Story.Flags);
        Assert.DoesNotContain(401, done.Snapshot.Story.Flags); Assert.Empty(done.Snapshot.Story.Callers);
        Assert.Null(done.Snapshot.Story.Cursor);
        var after = session.Current;
        Assert.NotNull(Submit(session, new Acknowledge(wait.Token)).Failure); Assert.Same(after, session.Current);
        Assert.Equal(pending.Battle.MainSeed, after.Battle.MainSeed); // spawning waits still precede generation
    }

    [Fact]
    public void ReachedExplorationOnlyOperationFailsWithEarlierWritesAndExactPc()
    {
        var started = Start([new WriteFlag(402, true), new ResetPartyBattleStats(), new EndProgram()]);
        Assert.Equal("battle-program-instruction", started.Result.Failure!.Code);
        Assert.Equal(new ProgramLocation("region", 1), started.Session.Current.Story.Cursor);
        Assert.Contains(386, started.Session.Current.Story.Flags); Assert.Contains(402, started.Session.Current.Story.Flags);
        Assert.Equal(0, started.Session.Current.Battle.Round);
        var before = started.Session.Current;
        var retry = Submit(started.Session, new AdvanceSimulation());
        Assert.Equal("session-stopped", retry.Failure!.Code);
        Assert.Equal(before.Revision, retry.Snapshot.Revision); Assert.Equal(before.Battle.MainSeed, retry.Snapshot.Battle.MainSeed);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, true)]
    public void NoRegionOrHeldSharedFlagSkipsBothRows(bool inside, bool played)
    {
        var started = Start(inside: inside, flags: played ? [386] : []);
        Assert.Null(started.Result.Failure); Assert.Null(started.Session.Current.Story.Cursor);
        Assert.DoesNotContain(400, started.Session.Current.Story.Flags);
        Assert.DoesNotContain(401, started.Session.Current.Story.Flags);
    }
}

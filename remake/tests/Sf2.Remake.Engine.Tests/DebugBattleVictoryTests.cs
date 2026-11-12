using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class DebugBattleVictoryTests
{
    [Fact]
    public void StandaloneVictoryIsFinalAndPreservesEarnedResources()
    {
        var session = Start();
        var before = session.Current.Battle;
        var won = Accept(session, new DebugWinBattle());
        Assert.Equal(BattleOutcomeKind.Victory, won.Snapshot.BattleResult);
        Assert.Equal(SessionStopReason.BattleEnded, won.StopReason);
        Assert.Equal(before.MainSeed, won.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, won.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(before.Gold, won.Snapshot.Battle.Gold);
        Assert.Same(before.ItemCustody, won.Snapshot.Battle.ItemCustody);
        Assert.All(before.Actors.Where(actor => actor.IsAlly), actor => Assert.Same(actor, won.Snapshot.Battle.GetActor(actor.Actor)));
        Assert.Equal("battle-ended", Send(session, new DebugWinBattle()).Failure!.Code);
    }

    [Fact]
    public void UnsettledBattleCannotSkipAnActionOrAStory()
    {
        var session = Start();
        Accept(session, new Confirm());
        var before = session.Current;
        Assert.Equal("debug-victory-unavailable", Send(session, new DebugWinBattle()).Failure!.Code);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void RequiredDefeatedSceneStagesItsActorsThenReturnsWithoutInventingKills()
    {
        var (definition, fixture) = BattleEndPositionTests.DefeatedContext(change: battle =>
            battle.With(actors: battle.Actors.Select(actor => actor.IsAlly ? actor : actor.With(hp: 5)), gold: 93));
        var selection = Start().Current.Selection!;
        var input = new SessionSnapshot(fixture.SessionId, 0, 0, new ActiveBattle(fixture.Battle, selection),
            fixture.Story.Copy(null, clearDefeatedScene: true), SessionStopReason.PlayerInput);
        var rules = RuleCompositions.Sf2();
        var result = DebugBattleVictory.Submit(definition, input, rules);
        Assert.Null(result.Failure);
        Assert.Single(result.Observations, row => row.Kind == "enemy-defeated-program-started");
        Assert.Equal("defeated", result.Snapshot.Story.Cursor!.Value.Program);
        var wait = result.Snapshot.Story.Wait!;
        var after = ExplorationDispatcher.Submit(definition, result.Snapshot, new AdvanceSimulation(wait.Token), rules);
        Assert.Null(after.Failure);
        Assert.Single(after.Observations, row => row.Kind == "enemy-defeated-program-finished");
        Assert.Single(after.Observations, row => row.Kind == "outcome-program-started");
        Assert.Equal("after", after.Snapshot.Story.Cursor!.Value.Program);
        Assert.DoesNotContain(after.Observations, row => row.Kind is "kills" or "gold" or "field-death-started");
        var field = after.Snapshot.Exploration!;
        Assert.False(field.Entities[new("entity-128")].Visible);
        Assert.Equal(fixture.Battle.Gold, field.Party.Gold);
        Assert.All(field.Party.Actors.Where(actor => fixture.Battle.GetActor(actor.Actor).IsAlly), actor =>
        {
            Assert.Equal(fixture.Battle.GetActor(actor.Actor).Kills, actor.Kills);
            Assert.Equal(fixture.Battle.GetActor(actor.Actor).Exp, actor.Exp);
        });
    }
}

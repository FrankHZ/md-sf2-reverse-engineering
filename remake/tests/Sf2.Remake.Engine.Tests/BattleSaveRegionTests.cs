using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSaveRegionTests
{
    [Fact]
    public void CompletedRegionProgramAndActivationContinueWithoutReplayingTheProgram()
    {
        var (basis, original) = BattleSaveFixture.Create();
        var b = original.Battle; var d = b.Definition; var init = d.Initialization!;
        var encounter = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments, [],
            d.Rewards, new([new(6, [new(3, 4), new(6, 4), new(6, 7), new(3, 7)]), init.Regions[1]],
                BattleRegionProgram.Authored, init.AllyFormation, init.AiPoints, init.SourceCombatants, init.EnemyCount,
                [new(6, 386, "region")]), d.Outcome);
        var world = basis.Exploration!;
        var exploration = new ExplorationDefinition(world.Maps.Values,
            world.Programs.Values.Append(new("region", [new WriteFlag(700, true), new EndProgram()])),
            provenance: world.Provenance, partyFlags: world.PartyFlags, visuals: world.Visuals);
        var definition = new ScenarioDefinition(basis.Package,
            basis.Encounters.Values.Select(e => e.Encounter == encounter.Encounter ? encounter : e),
            exploration: exploration, encounterStarts: basis.EncounterStarts.Values) { Persistence = new Sf2OpeningRestore() };
        var battle = new EngineBattleState(encounter, b.Actors, b.MainSeed, b.ThinkingSeed, b.Round,
            [new(new("ally-0"), 25), new(null, 255)], 0, b.Gold, b.StartPolicy, b.Regions,
            itemCustody: b.ItemCustody, deathCredit: b.DeathCredit);
        var session = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition,
            BattleSaveFixture.Capture(definition, BattleSaveFixture.At(battle, original.Story)), RuleCompositions.Sf2())).Session;
        Assert.Equal(1, Stay(session));
        Assert.Contains(386, session.Current.Story.Flags); Assert.Contains(700, session.Current.Story.Flags);
        Assert.True(session.Current.Battle.Regions!.Flags[6]);
        Assert.NotEqual(0, session.Current.Battle.GetActor(new("enemy")).ActivationWord!.Value & 1);
        // Entering a script invalidates the audio history. C owns the host observation hook;
        // this engine test supplies known context only after the real program has completed.
        Assert.Equal("save-battle-music-unknown", session.QueryBattleSave().Error);
        var known = session.Current.WithStory(session.Current.Story.Copy(null, music: original.Story.Music));
        var saved = BattleSaveFixture.Capture(definition, known);
        var resumed = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, saved, session.Rules)).Session;
        Assert.Equal(0, Stay(session)); Assert.Equal(0, Stay(resumed));
        Assert.True(resumed.Current.Battle.Round > saved.Battle.Round);
        BattleSaveFixture.EqualBattle(session.Current.Battle, resumed.Current.Battle);
        Assert.Equal(session.Current.Story.Flags, resumed.Current.Story.Flags);
    }

    private static int Stay(GameSession session)
    {
        int programs = 0;
        void Send(SessionCommand command)
        {
            var result = EngineTestContent.Send(session, command); Assert.Null(result.Failure);
            programs += result.Observations.Count(o => o.Kind == "region-program-started");
        }
        Send(new Confirm()); Send(new ChooseAction(SessionAction.Stay)); Send(new Confirm());
        for (int guard = 0; session.Current.StopReason != SessionStopReason.PlayerInput; guard++)
        {
            Assert.True(guard < 1000);
            var current = session.Current;
            Send(current.BattleMovement is { } move ? new CompletePresentation(move.Token, move.CompletionKind) :
                current.BattleScene is { } scene ? scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind) :
                new AdvanceSimulation(current.Story.Wait?.Token));
        }
        return programs;
    }
}

using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSaveReturnTests
{
    [Theory]
    [InlineData(BattleOutcomeKind.Victory)] [InlineData(BattleOutcomeKind.Defeat)] [InlineData(BattleOutcomeKind.Retreat)]
    public void LegalEndingRetainsReturnRegistersPartyAndReusableFieldSave(BattleOutcomeKind kind)
    {
        var (basis, original) = BattleSaveFixture.Create();
        var encounter = original.Battle.Definition.WithOutcome(original.Battle.Definition.Outcome!, retreatItems: [new(115, "Angel Wing")]);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values.Select(e => e.Encounter == encounter.Encounter ? encounter : e),
            exploration: basis.Exploration, encounterStarts: basis.EncounterStarts.Values) { Persistence = new Sf2OpeningRestore() };
        var loadout = new BattleSourceLoadout([115, 127, 127, 127], [63, 63, 63, 63]);
        var actors = original.Battle.Actors.Select(a => a.Actor.Value switch
        {
            "ally-0" => a.With(hp: 1, sourceLoadout: loadout),
            "enemy" => a.With(hp: 1, position: new(5, 5), activationWord: 0x2061), _ => a,
        }).ToArray();
        var b = original.Battle;
        var battle = new EngineBattleState(encounter, actors, b.MainSeed, b.ThinkingSeed, b.Round, b.Queue, b.Cursor, b.Gold,
            b.StartPolicy, b.Regions, itemCustody: b.ItemCustody, deathCredit: b.DeathCredit);
        var live = BattleSaveFixture.At(battle, original.Story.Copy(null, fieldPlayer: original.Story.FieldPlayer! with
            { RetainedAllies = original.Story.FieldPlayer.RetainedAllies!.Select(a => a with { Absent = false }).ToArray() }));
        var saved = BattleSaveFixture.Capture(definition, live);
        var restored = Assert.IsType<SessionStarted>(GameSession.RestoreBattle(definition, saved, RuleCompositions.Sf2())).Session;
        var control = End(definition, live, kind);
        var resumed = End(definition, restored.Current, kind);
        Assert.True(control.CanWaitAtInput); Assert.True(resumed.CanWaitAtInput);
        Assert.Equal(control.Exploration!.Map, resumed.Exploration!.Map);
        Assert.Equal(control.Exploration.Player, resumed.Exploration.Player);
        Assert.Equal(control.Exploration.PlayerEntity.Motion, resumed.Exploration.PlayerEntity.Motion);
        Assert.Equal(control.Story.Flags, resumed.Story.Flags); Assert.Equal(control.Story.EgressMap, resumed.Story.EgressMap);
        Assert.Equal(control.Story.StepCounter, resumed.Story.StepCounter); Assert.Equal(control.Story.Names, resumed.Story.Names);
        var expected = control.Exploration.Party; var actual = resumed.Exploration.Party;
        Assert.Equal(expected.Gold, actual.Gold); Assert.Equal(expected.MainSeed, actual.MainSeed); Assert.Equal(expected.ThinkingSeed, actual.ThinkingSeed);
        Assert.Equal(kind == BattleOutcomeKind.Defeat ? saved.Battle.Gold / 2 : kind == BattleOutcomeKind.Retreat ? saved.Battle.Gold : saved.Battle.Gold + 12, actual.Gold);
        foreach (var actor in expected.Actors)
        {
            var other = actual.Actors.Single(a => a.Actor == actor.Actor);
            Assert.Equal(actor.Hp, other.Hp); Assert.Equal(actor.Mp, other.Mp); Assert.Equal(actor.Exp, other.Exp);
            Assert.Equal(actor.Kills, other.Kills); Assert.Equal(actor.Defeats, other.Defeats); Assert.Equal(actor.Status, other.Status);
            Assert.Equal(actor.SourceLoadout!.Items, other.SourceLoadout!.Items);
        }
        Assert.Equal(0, actual.Actors.Single(a => a.Actor.Value == "ally-1").Hp);
        Assert.Null(resumed.Story.EnteringBattle); Assert.Null(resumed.Story.FieldPlayer);
        // The real v1 consumer runs on the supported returned field recipe.
        var field = OpeningSave.Capture(definition, resumed, RuleCompositions.Sf2(), resumed.SessionId, resumed.Revision);
        Assert.Null(field.Error);
        var again = Assert.IsType<SessionStarted>(GameSession.Restore(definition, field.Data!, RuleCompositions.Sf2())).Session;
        Assert.Equal(actual.Gold, again.Current.Exploration!.Party.Gold);
        Assert.Equal(actual.Actors.First().Kills, again.Current.Exploration.Party.Actors.First().Kills);
        Assert.Null(EngineTestContent.Send(again, new WaitAtInput()).Failure);
    }

    private static SessionSnapshot End(ScenarioDefinition definition, SessionSnapshot current, BattleOutcomeKind kind)
    {
        var rules = RuleCompositions.Sf2();
        var commands = kind switch
        {
            BattleOutcomeKind.Victory => new SessionCommand[] { new Confirm(), new ChooseAction(SessionAction.PhysicalAttack), new SelectTarget(new("enemy")), new Confirm() },
            BattleOutcomeKind.Defeat => [new Confirm(), new ChooseAction(SessionAction.Stay), new Confirm()],
            _ => [new Confirm(), new SelectItem(0), new SelectTarget(new("ally-0")), new Confirm()],
        };
        foreach (var command in commands)
        {
            var result = BattleCommandDispatcher.Submit(current, command, rules);
            Assert.Null(result.Failure); current = result.Snapshot;
        }
        for (int guard = 0; !current.CanWaitAtInput; guard++)
        {
            Assert.True(guard < 3000, current.StopReason + " " + current.Story.Wait);
            SessionResult result;
            if (current.Mode == SessionMode.Exploration)
                result = ExplorationDispatcher.Submit(definition, current, new AdvanceSimulation(current.Story.Wait?.Token), rules);
            else if (current.BattleRetreat is { } retreat)
                result = BattleRetreatContinuation.Submit(definition, current, retreat.Phase == BattleRetreatPhase.Text
                    ? retreat.Revealed ? new Acknowledge(retreat.Token) : new CompleteTextReveal(retreat.Token)
                    : new CompletePresentation(retreat.Token, retreat.CompletionKind), rules);
            else if (current.BattleScene is { } scene)
                result = BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                    new CompletePresentation(scene.Token, scene.CompletionKind), rules);
            else if (current.BattleMovement is { } movement)
                result = BattleMovementContinuation.Submit(current, new CompletePresentation(movement.Token, movement.CompletionKind), rules);
            else result = BattleAdvancer.Advance(current, [], rules);
            Assert.Null(result.Failure);
            result = BattleOutcome.Begin(definition, result, rules);
            Assert.Null(result.Failure); current = result.Snapshot;
        }
        return current;
    }
}

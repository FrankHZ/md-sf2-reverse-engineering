using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.HiddenSpawnTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class HiddenSpawnSessionTests
{
    [Fact]
    public void OrdinaryTriggerAppearancesCombatDeathAndLaterReappearanceKeepUsableControl()
    {
        var s = Start("hidden-spawns");
        Assert.Equal(1, s.Current.Battle.Round); Assert.Null(s.Current.Battle.GetActor(Returner).Position);
        FinishMovement(s, Accept(s, new Move(ExplorationDirection.North)));
        var events = Settle(s, Stay(s), stopAtAppearance: true).Observations.ToList();
        Assert.Equal(new[] { Returner, Hidden }, s.Current.BattleRound!.Appearances);
        Assert.Equal(1, s.Current.Battle.Round);
        Assert.All(new[] { Hidden, Returner }, id => Assert.Equal(1, s.Current.Battle.GetActor(id).Hp));
        Assert.DoesNotContain(events, e => e.Kind == "round-started");
        var resetBattle = s.Current.Battle; var token = s.Current.BattleRound.Token;
        Assert.Equal("appearance-pending", Send(s, new CompletePresentation(token, PresentationCueKind.FadeOut)).Failure!.Code);
        Assert.Equal("appearance-pending", Send(s, new Confirm()).Failure!.Code);
        var first = Accept(s, new CompletePresentation(token, PresentationCueKind.EntityEffect)); events.AddRange(first.Observations);
        Assert.Same(resetBattle, s.Current.Battle); Assert.Equal(Hidden, s.Current.BattleRound!.Actor);
        Assert.NotNull(Send(s, new CompletePresentation(token, PresentationCueKind.EntityEffect)).Failure);
        var snapshot = s.Current;
        Assert.Equal("stale-input", s.Submit(new(snapshot.SessionId, snapshot.Revision - 1, null,
            new CompletePresentation(snapshot.BattleRound.Token, PresentationCueKind.EntityEffect))).Failure!.Code);
        var done = Settle(s, new(s.Current, [], s.Current.StopReason)); events.AddRange(done.Observations);
        Assert.Equal(2, s.Current.Battle.Round); Assert.Equal(Leader, s.Current.Selection!.Actor);
        Assert.Equal(new[] { Returner, Hidden }, events.Where(e => e.Kind == "appearance-completed").Select(e => e.Actor!.Value));
        Assert.True(events.FindLastIndex(e => e.Kind == "appearance-completed") < events.FindIndex(e => e.Kind == "round-started"));
        Settle(s, Attack(s, Hidden));
        Assert.Equal(0, s.Current.Battle.GetActor(Hidden).Hp); Assert.Equal(1, s.Current.Battle.GetActor(Hidden).MaxHp);
        var kill = Settle(s, Attack(s, Returner), stopAtAppearance: true);
        Assert.Equal(new[] { Returner }, s.Current.BattleRound!.Appearances);
        Assert.Equal(1, s.Current.Battle.GetActor(Returner).Hp);
        Assert.Contains(kill.Observations, e => e.Kind == "death-cleanup" && e.Actor == Returner);
        Assert.Contains(kill.Observations, e => e.Kind == "spawn-reset" && e.Actor == Returner);
        var after = Settle(s, new(s.Current, [], s.Current.StopReason));
        Assert.True(s.Current.HasBattleControl); Assert.Equal(Leader, s.Current.Selection!.Actor);
        Assert.Equal((ushort)2, s.Current.Battle.GetActor(Leader).Kills);
        Assert.Null(s.Current.Battle.GetActor(Hidden).Position);
        FinishMovement(s, Accept(s, new Move(ExplorationDirection.South)));
        Assert.Equal(new MapPosition(2, 4), s.Current.Selection!.Preview.Destination);
    }

    [Fact]
    public void LaterDecisionFailureRetainsCompletedAppearancesAndStoryWithoutResettingAgain()
    {
        var b = Held(Initial());
        // A source commandset failure occurs after all appearance delivery and queue publication.
        var actors = b.Actors.Select(a => a.Actor == Leader
            ? new BattleActorState(a.Deployment with { Definition = new BattleActorDefinition(a.Actor,
                a.Definition.ClassRule, 1, 60, 0, 80, 200, 0, false, 4, [], a.Definition.Physical, sourceLoadout: a.SourceLoadout) },
                a.Hp, a.Mp, a.Exp, a.Position, a.Kills, a.Defeats, activationWord: 0)
            : a.Actor == Sentry ? new BattleActorState(a.Deployment with { Initialization = a.Deployment.Initialization! with { AiCommandset = 1 } },
                a.Hp, a.Mp, a.Exp, a.Position, a.Kills, a.Defeats, activationWord: 0) : a);
        b = b.With(actors: actors);
        var story = new StoryState().Copy(null, flags: [24, 500]);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 4, 8, new ActiveBattle(b, null), story, SessionStopReason.SimulationWait);
        var result = BattleRoundContinuation.Begin(snapshot, []); var all = result.Observations.ToList();
        while (result.Snapshot.BattleRound?.Actor is not null)
        {
            var wait = result.Snapshot.BattleRound;
            result = BattleRoundContinuation.Submit(result.Snapshot, new CompletePresentation(wait.Token, wait.CompletionKind), RuleCompositions.Sf2());
            all.AddRange(result.Observations);
        }
        Assert.NotNull(result.Failure); Assert.Equal("source-commandset", result.Failure!.Code);
        Assert.Equal(2, all.Count(e => e.Kind == "spawn-reset"));
        Assert.Equal(2, all.Count(e => e.Kind == "appearance-completed"));
        Assert.Equal(1, result.Snapshot.Battle.Round); Assert.Same(story, result.Snapshot.Story);
        Assert.Equal(1, result.Snapshot.Battle.GetActor(Returner).Hp);
        var retry = BattleAdvancer.Advance(result.Snapshot, []);
        Assert.Equal(result.Failure.Code, retry.Failure!.Code); Assert.DoesNotContain(retry.Observations, e => e.Kind == "spawn-reset");
        Assert.Equal(result.Snapshot.Battle.MainSeed, retry.Snapshot.Battle.MainSeed);
    }

    [Fact]
    public void OrdinaryLeaderTerminationAndWalkingRetryResetDormancyButKeepStoryAndItemCustody()
    {
        var d = Document("battle-drops"); var battle = d["battle"]!; var encounter = battle["encounters"]![0]!;
        var template = Document("hidden-spawns")["encounters"]![0]!;
        var init = template["initialization"]!.DeepClone();
        init["enemyCount"] = 2;
        init["regions"]![0]!["vertices"] = JsonNode.Parse("""[{"x":1,"y":1},{"x":3,"y":1},{"x":3,"y":2},{"x":1,"y":2}]""");
        var slots = new[] { 0, 1, 128, 129 };
        for (int i = 0; i < 4; i++)
        {
            var placement = encounter["placements"]![i]!;
            var source = template["placements"]![i == 3 ? 2 : i == 2 ? 3 : 0]!["initialization"]!.DeepClone();
            placement["initialization"] = source;
            init["sourceCombatants"]![i]!["actor"] = placement["actor"]!.DeepClone();
            init["sourceCombatants"]![i]!["slot"] = slots[i];
            battle["start"]!["actors"]![i]!["initialization"] = JsonNode.Parse(i == 3
                ? """{"initialized":false,"activationWord":512}""" : """{"initialized":true,"activationWord":0}""");
        }
        encounter["initialization"] = init;
        battle["start"]!["actors"]![3]!["hp"] = 0;
        battle["actors"]![3]!["items"] = new JsonArray();
        battle["start"]!["itemCustody"]!["onceFlags"] = new JsonArray(24);
        var s = Assert.IsType<SessionStarted>(GameSession.Start(Reader(d))).Session;
        Accept(s, new Interact(new("ferryman"))); Field();
        Assert.Equal(SessionMode.Battle, s.Current.Mode);
        Assert.Equal(new[] { new ActorRef("survivor") }, s.Current.BattleRound!.Appearances);
        Settle(s, new(s.Current, [], s.Current.StopReason));
        FinishBattleScenes(s, Attack(s, new("enemy"))); Field();
        Assert.Equal(SessionMode.Exploration, s.Current.Mode);
        var custody = s.Current.Exploration!.Party.ItemCustody!;
        Assert.Contains((byte)24, custody.OnceFlags); Assert.Contains(24, s.Current.Story.Flags);
        var inventory = s.Current.Exploration.Party.Actors.Single(a => a.Actor == Leader).SourceLoadout!.Items;
        Accept(s, new Move(ExplorationDirection.West)); Field();
        Assert.Equal(SessionMode.Battle, s.Current.Mode);
        Assert.Same(custody, s.Current.Battle.ItemCustody);
        Assert.Equal(inventory, s.Current.Battle.GetActor(Leader).SourceLoadout!.Items);
        // Mode2 appeared and died in the previous encounter, yet gets a fresh appearance here.
        Assert.Equal(new[] { new ActorRef("survivor") }, s.Current.BattleRound!.Appearances);
        Assert.True(s.Current.Battle.GetActor(new("survivor")).Initialized);
        Assert.Equal(20, s.Current.Battle.GetActor(new("survivor")).Hp);
        Settle(s, new(s.Current, [], s.Current.StopReason));
        Assert.True(s.Current.HasBattleControl);
        Assert.Contains(24, s.Current.Story.Flags);

        void Field()
        {
            for (int i = 0; i < 2000 && s.Current.Mode == SessionMode.Exploration && s.Current.Story.Wait is { } wait; i++)
                Accept(s, wait switch
                {
                    DialogueWait => new Acknowledge(wait.Token),
                    FullFadeWait { LogicalDone: true } fade => new CompletePresentation(wait.Token, fade.Kind),
                    _ => new AdvanceSimulation(wait.Token),
                });
        }
    }

    [Fact]
    public void ExistingVictoryAndDefeatPrecedeDormantPotentialAndFreshStartClearsLifecycle()
    {
        var b = Held(Initial());
        var d = b.Definition;
        var definition = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments,
            d.Spells.Values, d.Rewards, d.Initialization, new(Leader, Sentry));
        b = new(definition, b.Actors.Select(a => a.Actor == Sentry ? a.With(hp: 0, clearPosition: true) : a),
            b.MainSeed, b.ThinkingSeed, b.Round, [], 0, b.Gold, regions: b.Regions);
        var result = BattleAdvancer.Advance(new(Guid.NewGuid(), 0, 0, b, null, SessionStopReason.SimulationWait), []);
        Assert.Null(result.Snapshot.BattleRound); Assert.DoesNotContain(result.Observations, e => e.Kind == "spawn-reset");
        Assert.Equal(BattleOutcomeKind.Victory, BattleOutcomeRules.Check(b));
        var fresh = Initial(); Assert.False(fresh.Regions!.Flags[0]); Assert.Equal(0, fresh.Regions.Tested);
        Assert.False(fresh.GetActor(Hidden).Initialized); Assert.Null(fresh.GetActor(Returner).Position);
        var defeated = b.With(actors: b.Actors.Select(a => a.IsAlly ? a.With(hp: 0) : a));
        Assert.Equal(BattleOutcomeKind.Defeat, BattleOutcomeRules.Check(defeated));
    }

    internal static SessionResult Attack(GameSession s, ActorRef target)
    {
        Accept(s, new Confirm()); Accept(s, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(s, new SelectTarget(target)); return Accept(s, new Confirm());
    }
    internal static SessionResult Settle(GameSession s, SessionResult result, bool stopAtAppearance = false)
    {
        var events = result.Observations.ToList();
        for (int count = 0; count < 1000; count++)
        {
            var snapshot = s.Current;
            if (snapshot.BattleRound is { } round)
            {
                if (stopAtAppearance) return result with { Observations = events.AsReadOnly() };
                result = Accept(s, new CompletePresentation(round.Token, round.CompletionKind));
            }
            else if (snapshot.BattleMovement is { } movement) result = Accept(s, new CompletePresentation(movement.Token, movement.CompletionKind));
            else if (snapshot.BattleScene is { } scene) result = Accept(s, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
            else if (snapshot.Selection is not null) return result with { Observations = events.AsReadOnly() };
            else result = Accept(s, new AdvanceSimulation());
            events.AddRange(result.Observations);
        }
        Assert.Fail("Battle did not settle."); return result;
    }
}

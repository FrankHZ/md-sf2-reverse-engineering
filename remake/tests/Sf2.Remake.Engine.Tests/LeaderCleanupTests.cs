using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class LeaderCleanupTests
{
    private static readonly ActorRef Leader = new("leader"), Gate = new("enemy"), Survivor = new("survivor");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrdinaryAttackSweepsLivingEnemyThenReturnsToUsableField(bool completed)
    {
        var session = Enter(change: d => { if (completed) d["world"]!["programs"]![2]!["instructions"]!.AsArray().Insert(0,
            JsonNode.Parse("""{"op":"set-flag","flag":501,"value":true}""")); });
        var before = session.Current.Battle;
        var observations = Attack(session).Observations.ToList();
        Until(session, BattleScenePhase.FieldSpin, observations);
        var field = session.Current;
        Assert.Equal(new[] { Gate, Survivor }, field.BattleScene!.DeadActors);
        Assert.Equal(0, field.Battle.GetActor(Gate).Hp);
        Assert.Equal(20, field.Battle.GetActor(Survivor).Hp);
        Assert.NotNull(field.Battle.GetActor(Survivor).Position);
        Assert.Equal((ushort)0, field.Battle.GetActor(Leader).Kills);
        Assert.Equal(Leader, field.Battle.FirstAlly);
        Assert.Contains(observations, e => e.Kind == (completed ? "enemy-defeated-program-skipped" : "enemy-defeated-program-none"));
        Assert.DoesNotContain(observations, e => e.Kind == "battle-outcome");
        var stale = new CompletePresentation(field.BattleScene.Token, field.BattleScene.CompletionKind);
        var next = Accept(session, stale); observations.AddRange(next.Observations);
        var accepted = session.Current;
        Assert.NotNull(Send(session, stale).Failure);
        Assert.Same(accepted, session.Current);
        Until(session, BattleScenePhase.FieldSettle, observations);
        var cleaned = session.Current;
        Assert.All(new[] { Gate, Survivor }, id => Assert.Null(cleaned.Battle.GetActor(id).Position));
        Assert.Equal(20, cleaned.Battle.GetActor(Survivor).Hp);
        Assert.Equal((ushort)2, cleaned.Battle.GetActor(Leader).Kills);
        Assert.Equal(before.Gold + 11, cleaned.Battle.Gold); // Only the damaged gate's gold.
        Assert.Single(observations, e => e.Kind == "exp");
        Assert.Equal(new[] { Gate, Survivor }, observations.Where(e => e.Kind == "death-cleanup").Select(e => e.Actor!.Value));
        observations.AddRange(Accept(session, SceneCommand(cleaned)).Observations);
        SettleField(session);
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal((ushort)2, session.Current.Exploration!.Party.Actors.Single(a => a.Actor == Leader).Kills);
        Assert.Contains(501, session.Current.Story.Flags);
        Assert.Single(observations, e => e.Kind == "battle-outcome");
        var position = session.Current.Exploration.PlayerEntity.Position;
        Accept(session, new Move(ExplorationDirection.West)); SettleField(session);
        Assert.Equal(new MapPosition(position.X - 1, position.Y), session.Current.Exploration!.PlayerEntity.Position);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void HookRequiresDeadBoundGateAndLivingBowieAndDefeatWins(bool gateDead, bool bowieDead)
    {
        var session = Enter();
        var battle = session.Current.Battle.With(actors: session.Current.Battle.Actors.Select(a =>
            a.Actor == Gate && gateDead || a.Actor == Leader && bowieDead ? a.With(hp: 0) : a));
        bool hook = BattleOutcomeRules.DefeatedHook(battle);
        Assert.Equal(gateDead && !bowieDead, hook);
        var deaths = hook ? BattleDeathBatch.Empty.AppendSurvivors(battle) : BattleDeathBatch.Empty;
        Assert.Equal(gateDead && !bowieDead ? new[] { Survivor } : [], deaths.Actors);
        Assert.Equal(bowieDead ? BattleOutcomeKind.Defeat : null, BattleOutcomeRules.Check(battle));
    }

    [Fact]
    public void ExplicitSourceSlotsWinOverPhysicalLeaderDefinitionAndProcessingOrder()
    {
        var session = Enter(change: d =>
        {
            var binding = d["battle"]!["encounters"]![0]!["leaderCleanup"]!["enemySlots"]!;
            binding[0]!["slot"] = 128; binding[1]!["slot"] = 129;
            // Physical.Leader and Taros88 stay on enemy, but source128 is now survivor.
            d["battle"]!["encounters"]![0]!["tarosEffectiveness"] = JsonNode.Parse("""{"bowie":"leader","enemyDefinitions":[{"actor":"enemy","definition":88},{"actor":"survivor","definition":9}],"weaponItems":[61]}""");
            d["battle"]!["actors"]![3]!["physical"]!["gold"] = 99;
            d["battle"]!["actors"]![3]!["physical"]!["leader"] = false;
        });
        var battle = session.Current.Battle;
        Assert.Equal(Survivor, battle.Definition.Outcome!.DefeatedGateEnemy);
        Assert.False(BattleOutcomeRules.DefeatedHook(battle.With(actors: battle.Actors.Select(a => a.Actor == Gate ? a.With(hp: 0) : a))));
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Survivor ? a.With(hp: 0) : a));
        Assert.True(BattleOutcomeRules.DefeatedHook(battle));
        Assert.Equal(new[] { Gate }, BattleDeathBatch.Empty.AppendSurvivors(battle).Actors);
    }

    [Fact]
    public void SourceOrderIncludesUnplacedLivingActorsAndExcludesAlreadyZeroHp()
    {
        var battle = Enter().Current.Battle;
        var ordinary = battle.GetActor(Gate);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Gate ? a.With(hp: 0) :
            a.Actor == Survivor ? a.With(clearPosition: true, status: 0xC0) : a).Reverse(), firstAlly: Leader);
        var batch = BattleDeathBatch.Empty.Append(ordinary, battle.GetActor(Gate)).AppendSurvivors(battle);
        Assert.Equal(new[] { Gate, Survivor }, batch.Actors);
        Assert.Equal(batch.Actors, batch.AppendSurvivors(battle).Actors);
        var clean = batch.Clean(battle, Leader, RuleCompositions.SourceProgression());
        Assert.Equal(20, clean.Battle.GetActor(Survivor).Hp);
        Assert.Equal(0, clean.Battle.GetActor(Survivor).Status);
        Assert.Equal((ushort)2, clean.Battle.GetActor(Leader).Kills);
        Assert.Equal(battle.Gold, clean.Battle.Gold);
        Assert.Equal(battle.GetActor(Leader).Exp, clean.Battle.GetActor(Leader).Exp);
        Assert.Equal(BattleOutcomeKind.Victory, BattleOutcomeRules.Check(clean.Battle));
        Assert.Empty(BattleDeathBatch.Empty.AppendSurvivors(battle.With(actors: battle.Actors.Select(a => !a.IsAlly ? a.With(hp: 0) : a))).Actors);
    }

    [Fact]
    public void NoSceneTurnUsesRetainedSceneAllyRatherThanCurrentActor()
    {
        var original = Enter().Current;
        var battle = original.Battle.With(actors: original.Battle.Actors.Select(a =>
            a.Actor == Gate ? a.With(hp: 0) : a), firstAlly: Leader);
        var snapshot = new SessionSnapshot(original.SessionId, original.Revision, original.ObservationSequence,
            new ActiveBattle(battle, null), original.Story, SessionStopReason.SimulationWait);
        List<SessionObservation> facts = [];
        var pending = BattleActionCommitter.Publish(snapshot, battle, Survivor, battle.GetActor(Survivor).Position,
            [], facts, RuleCompositions.Sf2());
        Assert.Equal("field-death", pending.BattleScene!.ActionKind);
        Assert.Equal(Leader, pending.Battle.FirstAlly);
        Assert.Throws<InvalidOperationException>(() => pending.BattleScene.Action);
        for (int i = 0; i < 30 && pending.BattleScene is not null; i++)
        {
            var result = BattleSceneContinuation.Submit(pending, SceneCommand(pending), RuleCompositions.Sf2());
            Assert.Null(result.Failure); pending = result.Snapshot; facts.AddRange(result.Observations);
        }
        Assert.Null(pending.BattleScene);
        Assert.Equal((ushort)1, pending.Battle.GetActor(Leader).Kills);
        Assert.Equal((ushort)0, pending.Battle.GetActor(Survivor).Kills);
        Assert.Equal(20, pending.Battle.GetActor(Survivor).Hp);
        Assert.Single(facts, e => e.Kind == "enemy-defeated-program-none");
    }

    [Theory]
    [InlineData("stay")]
    [InlineData("sleep")]
    [InlineData("dead")]
    [InlineData("standby")]
    public void NoSceneCleanupWithoutFirstAllyRejectsBeforePublishing(string caller)
    {
        var original = Enter().Current;
        var battle = original.Battle.With(actors: original.Battle.Actors.Select(a =>
            a.Actor == Gate ? a.With(hp: 0) : a.Actor == Leader && caller == "sleep" ? a.With(status: 0xC0) : a));
        if (caller is "dead" or "standby") battle = battle.With(queue: [new(caller == "dead" ? Gate : Survivor, 10)], cursor: 0);
        var current = new SessionSnapshot(original.SessionId, original.Revision, original.ObservationSequence,
            new ActiveBattle(battle, null), original.Story, SessionStopReason.SimulationWait);
        if (caller == "stay")
        {
            List<SessionObservation> facts = [];
            var error = Assert.Throws<BattleRuleException>(() => BattleActionCommitter.Publish(current, battle, Leader,
                battle.GetActor(Leader).Position, [], facts, RuleCompositions.Sf2()));
            Assert.Equal("death-first-ally-context", error.Code); Assert.Empty(facts);
        }
        else
        {
            var result = BattleAdvancer.Advance(current, [], RuleCompositions.Sf2());
            Assert.Equal("death-first-ally-context", result.Failure!.Code);
            Assert.DoesNotContain(result.Observations, e => e.Kind is "kills" or "death-cleanup" or "battle-outcome");
            Assert.NotNull(result.Snapshot.Battle.GetActor(Survivor).Position);
        }
    }

    [Fact]
    public void LaterOutcomeFailureKeepsAcceptedRemovalAndRetriesWithoutDoubleAccounting()
    {
        var outcome = new CountingOutcome();
        var session = Enter(Rules(outcome));
        Attack(session); Until(session, BattleScenePhase.FieldSettle, []);
        var before = session.Current;
        Assert.Equal((ushort)2, before.Battle.GetActor(Leader).Kills);
        outcome.Fail = true;
        var rejected = Send(session, SceneCommand(before));
        Assert.Equal(SessionFailureKind.InvariantFailure, rejected.Failure!.Kind);
        Assert.Same(before, session.Current); Assert.Empty(rejected.Observations);
        outcome.Fail = false;
        var retried = Accept(session, SceneCommand(before));
        Assert.DoesNotContain(retried.Observations, e => e.Kind is "kills" or "death-cleanup" or "exp");
        SettleField(session);
        Assert.Equal((ushort)2, session.Current.Exploration!.Party.Actors.Single(a => a.Actor == Leader).Kills);
        Assert.Equal(1, outcome.Hooks);
    }

    [Fact]
    public void AfterTurnSleepHasNoSecondDefeatedHook()
    {
        var outcome = new CountingOutcome();
        var original = Enter().Current;
        var battle = original.Battle.With(actors: original.Battle.Actors.Select(a =>
            a.Actor == Leader ? a.With(status: 0xC0) : a.Actor.Value == "companion" ? a.With(hp: 1, position: a.Deployment.Position) : a),
            queue: [new(Leader, 63), new(new ActorRef("companion"), 1)], cursor: 0);
        var current = new SessionSnapshot(original.SessionId, original.Revision, original.ObservationSequence,
            new ActiveBattle(battle, null), original.Story, SessionStopReason.SimulationWait);
        var rules = Rules(outcome);
        var pending = BattleActionCommitter.Publish(current, battle, Leader, battle.GetActor(Leader).Position, [], [], rules);
        Assert.True(pending.BattleScene!.IsSleepTurn); Assert.Equal(1, outcome.Hooks);
        // The second checkpoint may see a newly zero-HP gate; it must not run the hook again.
        var changed = pending.Battle.With(actors: pending.Battle.Actors.Select(a => a.Actor == Gate ? a.With(hp: 0) : a));
        pending = new(pending.SessionId, pending.Revision, pending.ObservationSequence,
            new ActiveBattle(changed, null, pending.BattleScene), pending.Story, pending.StopReason);
        var complete = BattleAfterTurnContinuation.Complete(pending, SceneCommand(pending), rules);
        Assert.Null(complete.Failure); Assert.Equal(1, outcome.Hooks);
        Assert.DoesNotContain(complete.Observations, e => e.Kind is "field-death-started" or "kills");
    }

    [Theory]
    [InlineData("missing-slot", "leader-cleanup-slots")]
    [InlineData("duplicate-slot", "duplicate-source-slot")]
    [InlineData("ally-slot", "leader-cleanup-enemy")]
    [InlineData("bowie", "leader-cleanup-bowie")]
    [InlineData("program", "defeated-program")]
    [InlineData("outcome", "leader-cleanup-outcome")]
    public void IncompleteBindingAndUnimplementedProgramsFailAdmission(string fault, string code)
    {
        var d = Document("leader-cleanup"); var encounter = d["battle"]!["encounters"]![0]!;
        var binding = encounter["leaderCleanup"]!;
        switch (fault)
        {
            case "missing-slot": binding["enemySlots"]!.AsArray().RemoveAt(1); break;
            case "duplicate-slot": binding["enemySlots"]![0]!["slot"] = 128; break;
            case "ally-slot": binding["enemySlots"]![0]!["actor"] = "leader"; break;
            case "bowie": binding["bowie"] = "companion"; break;
            case "program": d["world"]!["programs"]![5]!["instructions"]!.AsArray().Insert(0,
                JsonNode.Parse("""{"op":"set-flag","flag":42,"value":true}""")); break;
            case "outcome": d["world"]!["maps"]![1]!["battle"]!.AsObject().Remove("outcome"); break;
        }
        Assert.Equal(code, Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d))).Failure.Code);
    }

    [Fact]
    public void BothLeadersDeadSkipsSweepCleansActionDeathsAndReturnsDefeat()
    {
        var session = Enter(); var original = session.Current;
        var battle = original.Battle;
        var batch = BattleDeathBatch.Empty;
        foreach (var id in new[] { Gate, Leader })
        {
            var before = battle.GetActor(id); var after = before.With(hp: 0);
            batch = batch.Append(before, after);
            battle = battle.With(actors: battle.Actors.Select(a => a.Actor == id ? after : a));
        }
        battle = battle.With(firstAlly: Leader);
        List<SessionObservation> facts = [];
        var pending = BattleActionCommitter.Publish(original, battle, Leader, battle.GetActor(Leader).Position,
            [], facts, RuleCompositions.Sf2(), sceneEnded: true, deaths: batch);
        Assert.Equal(new[] { Gate, Leader }, pending.BattleScene!.DeadActors);
        while (pending.BattleScene is not null)
        {
            var step = BattleSceneContinuation.Submit(pending, SceneCommand(pending));
            Assert.Null(step.Failure); pending = step.Snapshot; facts.AddRange(step.Observations);
        }
        Assert.Equal("Defeat", Assert.Single(facts, e => e.Kind == "battle-outcome").Detail);
        Assert.NotNull(pending.Battle.GetActor(Survivor).Position);
        Assert.Equal((ushort)1, pending.Battle.GetActor(Leader).Kills);
        Assert.Equal((ushort)1, pending.Battle.GetActor(Leader).Defeats);
        Assert.DoesNotContain(facts, e => e.Kind.StartsWith("enemy-defeated-program"));
        var returned = BattleOutcome.Begin(session.Definition, new(pending, facts, pending.StopReason));
        Assert.Null(returned.Failure);
        Assert.Equal(SessionMode.Exploration, returned.Snapshot.Mode);
        Assert.Equal(ProgramContinuation.DefeatProgramFinished, returned.Snapshot.Story.Continuation);
    }

    [Fact]
    public void SweepOrderingUsesSourceSlotsEvenWhenAllOtherOrdersDiffer()
    {
        var session = Enter(change: d =>
        {
            var b = d["battle"]!; var e = b["encounters"]![0]!;
            var actor = b["actors"]![3]!.DeepClone(); actor["id"] = "middle"; b["actors"]!.AsArray().Insert(0, actor);
            var start = b["start"]!["actors"]![3]!.DeepClone(); start["actor"] = "middle"; b["start"]!["actors"]!.AsArray().Insert(0, start);
            var placement = e["placements"]![3]!.DeepClone(); placement["actor"] = "middle";
            placement["processingOrder"] = 7; placement["x"] = 5; e["placements"]!.AsArray().Insert(0, placement);
            var slots = e["leaderCleanup"]!["enemySlots"]!.AsArray(); slots[0]!["slot"] = 159;
            slots.Add(JsonNode.Parse("""{"actor":"middle","slot":132}"""));
        });
        var battle = session.Current.Battle.With(actors: session.Current.Battle.Actors.Reverse());
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Gate ? a.With(hp: 0) : a));
        Assert.Equal(new[] { new ActorRef("middle"), Survivor }, BattleDeathBatch.Empty.AppendSurvivors(battle).Actors);
    }

    private static SessionRules Rules(IBattleOutcomeRule outcome) => new("cleanup-test", new Sf2HealingRule(),
        RuleCompositions.SourcePhysical(), RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), outcome: outcome);
    private sealed class CountingOutcome : IBattleOutcomeRule
    {
        public string Identity => "counting-outcome";
        public int Hooks { get; private set; }
        public bool Fail { get; set; }
        public BattleOutcomeKind? Check(EngineBattleState battle) => Fail ? throw new InvalidOperationException("outcome fault") : BattleOutcomeRules.Check(battle);
        public bool DefeatedHook(EngineBattleState battle) { Hooks++; return BattleOutcomeRules.DefeatedHook(battle); }
    }
    private static GameSession Enter(SessionRules? rules = null, Action<JsonNode>? change = null)
    {
        var d = Document("leader-cleanup"); change?.Invoke(d);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(d), rules ?? RuleCompositions.Sf2())).Session;
        Accept(session, new Interact(new("ferryman"))); SettleField(session);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        return session;
    }
    private static SessionResult Attack(GameSession session)
    {
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(Gate)); return Accept(session, new Confirm());
    }
    private static SessionCommand SceneCommand(SessionSnapshot current) => current.BattleScene!.RequiresAcknowledgement
        ? new Acknowledge(current.BattleScene.Token) : new CompletePresentation(current.BattleScene.Token, current.BattleScene.CompletionKind);
    private static void Until(GameSession session, BattleScenePhase phase, List<SessionObservation> facts)
    {
        for (int i = 0; i < 200 && session.Current.BattleScene?.Phase != phase; i++)
            facts.AddRange(Accept(session, SceneCommand(session.Current)).Observations);
        Assert.Equal(phase, session.Current.BattleScene!.Phase);
    }
    private static void SettleField(GameSession session)
    {
        for (int i = 0; i < 2000 && session.Current.Mode == SessionMode.Exploration && session.Current.Story.Wait is { } wait; i++)
            Accept(session, wait switch
            {
                DialogueWait => new Acknowledge(wait.Token),
                FullFadeWait { LogicalDone: true } fade => new CompletePresentation(wait.Token, fade.Kind),
                _ => new AdvanceSimulation(wait.Token),
            });
    }
}

using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PoisonTurnTests
{
    private static readonly ActorRef Hero = new("champion"), Friend = new("receiver"), Enemy = new("reserve");

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PrivateInitializedPoisonStillRejectsPhysicalAtItsExistingBoundary(bool poisonedTarget)
    {
        var battle = PrivateInitialized(poisonedTarget);
        Assert.False(battle.Definition.Poison);
        Assert.Equal((ushort)2, battle.GetActor(poisonedTarget ? Enemy : Hero).Status);
        var failure = Assert.Throws<BattleRuleException>(() => Sf2PhysicalAction.Prepare(battle, Hero,
            battle.GetActor(Hero).Position!, Enemy, new Sf2BattleProgressionRule()));
        Assert.Equal("physical-status", failure.Code); Assert.True(failure.Unsupported);
    }

    [Fact]
    public void PrivateInitializedPoisonStillRejectsSourceEnemyDecision()
    {
        var battle = PrivateInitialized(true);
        Assert.Equal((ushort)2, battle.GetActor(Enemy).Status);
        var failure = Assert.Throws<BattleRuleException>(() => SourceEnemyAi.Resolve(battle, Enemy));
        Assert.Equal("source-ai-state", failure.Code); Assert.True(failure.Unsupported);
    }

    [Fact]
    public void ExistingAuthoredInitializationDoesNotOptSourceAllyIntoPoison()
    {
        var input = Admitted("jaro-conversion"); var ally = JaroConversionTests.Ally;
        var start = new BattleStartInput(input.Start.Encounter, input.Start.Actors.Select(a =>
            a.Actor == ally ? a with { Status = 2, ActivationWord = 4, Absent = false, PositionOverride = new(4, 2) } : a),
            input.Start.MainSeed, input.Start.ThinkingSeed, input.Start.Gold);
        var battle = BattleTurnFlow.Start(input.Definition.Encounters[start.Encounter], start);
        Assert.Equal((ushort)2, battle.GetActor(ally).Status);
        var failure = Assert.Throws<BattleRuleException>(() => new SourceAllyAi().Decide(battle, ally, new Sf2PhysicalAction()));
        Assert.Equal("source-ally-ai-state", failure.Code); Assert.True(failure.Unsupported);
    }

    [Fact]
    public void PrivatePoisonGrowthRejectsBeforeItemCommitAndKeepsSubthresholdBehavior()
    {
        var battle = PrivateInitialized(false, 99); var rule = new Sf2ItemAction();
        var failure = Assert.Throws<BattleRuleException>(() => BattleActionRules.Prepare(rule, battle, Hero,
            battle.GetActor(Hero).Position!, new(BattleActionKind.Item, ItemSlot: 0), Hero, new Sf2BattleProgressionRule()));
        Assert.Equal("growth-status-refresh", failure.Code); Assert.True(failure.Unsupported);
        Assert.Equal((byte)99, battle.GetActor(Hero).Exp);
        Assert.Equal((ushort)0, battle.GetActor(Hero).SourceLoadout!.Items[0]);
        var low = battle.With(actors: battle.Actors.Select(a => a.Actor == Hero ? a.With(exp: 0) : a));
        var action = BattleActionRules.Prepare(rule, low, Hero, low.GetActor(Hero).Position!,
            new(BattleActionKind.Item, ItemSlot: 0), Hero, new Sf2BattleProgressionRule());
        Assert.NotNull(action.Reward);
        var threshold = low.With(actors: low.Actors.Select(a => a.Actor == Hero ? a.With(exp: 100) : a));
        var publication = Assert.Throws<BattleRuleException>(() => action.ApplyGrowth(threshold, new Sf2BattleProgressionRule()));
        Assert.Equal("growth-status-refresh", publication.Code); Assert.True(publication.Unsupported);
    }

    // Same NewBattle policy/low-bit initialization route used by private controlled starts;
    // all definitions and values here are project-authored, with no private fixture dependency.
    private static EngineBattleState PrivateInitialized(bool poisonedEnemy, byte exp = 0)
    {
        var growth = Admitted("battle-equipment").Definition.Encounters.Values.Single().Deployments
            .Single(a => a.Actor == Hero).Definition.Growth;
        var hero = new BattleActorDefinition(Hero, BattleClassRule.Ordinary, 1, 100, 0, 15, 5, 60, false, 4, [],
            physical: new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, false, 0),
            sourceLoadout: new([0, 127, 127, 127], [63, 63, 63, 63]), growth: growth);
        var enemy = new BattleActorDefinition(Enemy, BattleClassRule.Ordinary, 1, 100, 0, 10, 5, 4, false, 4, [],
            physical: new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, false, 0),
            sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]));
        var definition = new BattleDefinition("private-boundary", new("authored-map"), 8, 8,
            Enumerable.Repeat(new BattleTerrain(TerrainSurface.Open, TerrainProtection.None), 2304),
            [new(hero, BattleFaction.Ally, 0, BattleControl.Player, null, new(2, 2), new(null, 0, 15, 15, 0)),
             new(enemy, BattleFaction.Enemy, 128, BattleControl.Automatic, new("source-orders"), new(3, 2), new(0x3000, 0, 15, 15, 0x60, 6, 255, 255))],
            [], rewards: new(false), initialization: new([], BattleRegionProgram.None), healingItems: [new(0, "Herb", 10, 0, 1)]);
        var policy = new NewBattleStartPolicy("controlled", "project-authored-counterexample", "No natural reach claim",
            0, true, true, true, false, false, 0);
        var start = new BattleStartInput(definition.Encounter,
            [new(Hero, 50, 0, exp, 0, 0, poisonedEnemy ? (ushort)0 : (ushort)2, null, ActivationWord: 0),
             new(Enemy, 50, 0, 0, 0, 0, poisonedEnemy ? (ushort)2 : (ushort)0, null)], 0x1234, 0x4321, 0, policy);
        return BattleTurnFlow.Start(definition, start);
    }

    [Theory]
    [InlineData(1, 0)] [InlineData(2, 0)] [InlineData(3, 1)] [InlineData(200, 198)]
    public void FixedDamageClampsAndDoesNotMutateInput(ushort hp, ushort expected)
    {
        var actor = Start("poison-turn").Current.Battle.GetActor(Hero).With(hp: hp);
        Assert.True(PoisonTurnRules.Eligible(actor));
        Assert.Equal(expected, PoisonTurnRules.RemainingHp(actor.Hp));
        Assert.Equal(hp, actor.Hp);
        Assert.False(PoisonTurnRules.Eligible(actor.With(hp: 0)));
        Assert.False(PoisonTurnRules.Eligible(actor.With(clearPosition: true)));
        Assert.False(PoisonTurnRules.Eligible(actor.With(status: 0)));
    }

    [Theory]
    [InlineData("sleep")] [InlineData("poison+sleep")] [InlineData("stun")] [InlineData("2")]
    public void FiniteContentRejectsUnimplementedStatuses(string status)
    {
        var d = Document("poison-turn"); d["start"]!["actors"]![0]!["status"] = status;
        Assert.IsType<ScenarioReadRejected>(Reader(d).Read());
    }

    [Fact]
    public void PoisonRequiresOptInAndExplicitValidCredit()
    {
        var d = Document("poison-turn"); d.AsObject().Remove("battlePoison");
        Assert.IsType<ScenarioReadRejected>(Reader(d).Read());
        d = Document("poison-turn"); d["start"]!.AsObject().Remove("firstAlly");
        Assert.IsType<ScenarioReadRejected>(Reader(d).Read());
        d = Document("poison-turn"); d["start"]!["firstAlly"] = "reserve";
        Assert.IsType<ScenarioReadRejected>(Reader(d).Read());
        d = Document("poison-turn"); d["start"]!["firstAlly"] = null;
        Assert.IsType<ScenarioReadAccepted>(Reader(d).Read());
        Assert.False(Admitted().Definition.Encounters.Values.Single().Poison);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, false)] [InlineData(true, true)]
    public void PoisonDeathUsesRetainedCreditAndNeverReplaysHookOrAction(bool enemy, bool sentinel)
    {
        var outcome = new CountingOutcome(); var rules = Rules(outcome);
        var initial = Snapshot(enemy ? Enemy : Friend, 2, sentinel ? new(null) : new(Hero));
        List<SessionObservation> facts = [];
        var waiting = BattleActionCommitter.Publish(initial, initial.Battle, enemy ? Enemy : Friend,
            initial.Battle.GetActor(enemy ? Enemy : Friend).Position, [], facts, rules);
        Assert.Equal(1, outcome.Hooks);
        var damage = Complete(waiting, rules); facts.AddRange(damage.Observations);
        Assert.Equal(BattleScenePhase.PoisonExhaustedMessage, damage.Snapshot.BattleScene!.Phase);
        var dead = damage.Snapshot.Battle.GetActor(enemy ? Enemy : Friend);
        Assert.Equal((ushort)0, dead.Hp); Assert.Equal((ushort)2, dead.Status); Assert.NotNull(dead.Position);
        var result = Complete(damage.Snapshot, rules); facts.AddRange(result.Observations);
        while (result.Snapshot.BattleScene is not null)
        { result = Complete(result.Snapshot, rules); facts.AddRange(result.Observations); }
        var final = result.Snapshot.Battle.GetActor(dead.Actor);
        Assert.Null(final.Position); Assert.Equal((ushort)0, final.Status);
        Assert.Equal((ushort)(enemy ? 0 : 1), final.Defeats);
        Assert.Equal((ushort)(enemy && !sentinel ? 1 : 0), result.Snapshot.Battle.GetActor(Hero).Kills);
        Assert.Single(facts, e => e.Kind == "action-committed");
        Assert.Single(facts, e => e.Kind == "after-turn");
        Assert.Single(facts, e => e.Kind == "poison-hp");
        Assert.Single(facts, e => e.Kind == "death-cleanup");
        Assert.DoesNotContain(facts, e => e.Kind is "exp" or "gold" || e.Kind.StartsWith("rng-"));
        Assert.Equal(1, outcome.Hooks);
        Assert.Equal(initial.Battle.MainSeed, result.Snapshot.Battle.MainSeed);
        if (enemy) Assert.Contains(facts, e => e.Kind == "battle-outcome" && e.Detail == "Victory");
        else Assert.Equal(1, result.Snapshot.Battle.Cursor);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OutcomeFailureRetainsPrefixAndRetriesOnlyFinalization(bool lethal)
    {
        var outcome = new CountingOutcome(); var rules = Rules(outcome);
        var initial = Snapshot(Friend, lethal ? (ushort)2 : (ushort)3, new(Hero));
        List<SessionObservation> facts = [];
        var pending = BattleActionCommitter.Publish(initial, initial.Battle, Friend,
            initial.Battle.GetActor(Friend).Position, [], facts, rules);
        if (lethal)
        {
            pending = Complete(pending, rules).Snapshot;
            pending = Complete(pending, rules).Snapshot;
            while (pending.BattleScene!.Phase != BattleScenePhase.FieldSettle) pending = Complete(pending, rules).Snapshot;
        }
        outcome.FailOnce = true;
        var failed = BattleSceneContinuation.Submit(pending, Command(pending), rules);
        Assert.NotNull(failed.Failure);
        var held = failed.Snapshot;
        Assert.Equal(lethal ? BattleScenePhase.FieldSettle : BattleScenePhase.PoisonFinalize, held.BattleScene!.Phase);
        Assert.Equal(lethal ? (ushort)0 : (ushort)1, held.Battle.GetActor(Friend).Hp);
        Assert.Equal(lethal ? (ushort)1 : (ushort)0, held.Battle.GetActor(Friend).Defeats);
        Assert.Equal(initial.Battle.Cursor, held.Battle.Cursor);
        Assert.Same(held, BattleAdvancer.Advance(held, [], rules).Snapshot);
        var resumed = Complete(held, rules);
        Assert.Null(resumed.Failure); Assert.Equal(initial.Battle.Cursor + 1, resumed.Snapshot.Battle.Cursor);
        Assert.Equal(Hero, resumed.Snapshot.Selection!.Actor);
        Assert.DoesNotContain(resumed.Observations, e => e.Kind is "action-committed" or "poison-hp" or "after-turn" or "death-cleanup" or "defeats");
        Assert.Equal(1, outcome.Hooks);
    }

    [Fact]
    public void MissingDeathContextPreservesPublishedLethalHpAndExhaustionWait()
    {
        var known = Snapshot(Friend, 1, new(Hero)); var b = known.Battle;
        var missing = new EngineBattleState(b.Definition, b.Actors, b.MainSeed, b.ThinkingSeed, b.Round, b.Queue, b.Cursor, b.Gold);
        var current = new SessionSnapshot(known.SessionId, 0, 0, missing, null, SessionStopReason.SimulationWait);
        var rules = Rules(new CountingOutcome());
        var pending = BattleActionCommitter.Publish(current, missing, Friend, missing.GetActor(Friend).Position, [], [], rules);
        pending = Complete(pending, rules).Snapshot;
        var failed = BattleSceneContinuation.Submit(pending, Command(pending), rules);
        Assert.Equal("death-first-ally-context", failed.Failure!.Code); Assert.Same(pending, failed.Snapshot);
        Assert.Equal((ushort)0, failed.Snapshot.Battle.GetActor(Friend).Hp);
        Assert.NotNull(failed.Snapshot.Battle.GetActor(Friend).Position);
        Assert.Equal((ushort)0, failed.Snapshot.Battle.GetActor(Friend).Defeats);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void DeadAndUnplacedQueuedPoisonActorsSkipWithoutTick(bool dead)
    {
        var current = Snapshot(Friend, 3, new(Hero));
        var battle = current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == Friend ?
            dead ? a.With(hp: 0) : a.With(clearPosition: true) : a));
        var result = BattleAdvancer.Advance(new(current.SessionId, 0, 0, battle, null, SessionStopReason.SimulationWait), []);
        Assert.Null(result.Failure); Assert.Equal(Hero, result.Snapshot.Selection!.Actor);
        Assert.DoesNotContain(result.Observations, e => e.Kind is "poison-hp" or "poison-message");
        Assert.Equal(1, result.Snapshot.Battle.Cursor);
    }

    internal static SessionSnapshot Snapshot(ActorRef actor, ushort hp, BattleDeathCredit credit)
    {
        var original = Start("poison-turn").Current;
        var battle = original.Battle.With(actors: original.Battle.Actors.Select(a => a.With(
            hp: a.Actor == actor ? hp : (ushort)100, status: a.Actor == actor ? (ushort)2 : (ushort)0)),
            queue: [new(actor, 63), new(Hero, 30), new(Friend, 20)], cursor: 0, deathCredit: credit);
        return new(original.SessionId, 0, 0, battle, null, SessionStopReason.SimulationWait);
    }
    internal static SessionCommand Command(SessionSnapshot current) => new CompletePresentation(current.BattleScene!.Token, current.BattleScene.CompletionKind);
    private static SessionResult Complete(SessionSnapshot current, SessionRules rules)
    { var result = BattleSceneContinuation.Submit(current, Command(current), rules); Assert.Null(result.Failure); return result; }
    private static SessionRules Rules(IBattleOutcomeRule outcome) => new("poison-test", new Sf2HealingRule(),
        RuleCompositions.SourcePhysical(), RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), outcome: outcome);
    private sealed class CountingOutcome : IBattleOutcomeRule
    {
        public string Identity => "poison-outcome";
        public int Hooks { get; private set; }
        public bool FailOnce { get; set; }
        public BattleOutcomeKind? Check(EngineBattleState battle)
        { if (FailOnce) { FailOnce = false; throw new InvalidOperationException("one-shot outcome failure"); } return BattleOutcomeRules.Check(battle); }
        public bool DefeatedHook(EngineBattleState battle) { Hooks++; return BattleOutcomeRules.DefeatedHook(battle); }
    }
}

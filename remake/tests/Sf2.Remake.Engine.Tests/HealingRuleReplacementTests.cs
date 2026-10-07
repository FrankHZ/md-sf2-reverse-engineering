using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Authored;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class HealingRuleReplacementTests
{
    private static SessionRules Rules(bool half) => half ? RuleCompositions.AuthoredHealingB() : RuleCompositions.AuthoredHealingA();
    private static GameSession Open(SessionRules rules, Action<JsonNode>? change = null)
    {
        var document = Document("healing-rule-demo"); change?.Invoke(document);
        return Assert.IsType<SessionStarted>(GameSession.Start(Reader(document), rules)).Session;
    }
    private static void Select(GameSession session, ActorRef? target = null)
    {
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("mend", 1)));
        Accept(session, new SelectTarget(target ?? session.Current.Selection!.Actor));
    }

    [Theory]
    [InlineData(false, 30, 40, 2)]
    [InlineData(true, 30, 35, 0)]
    [InlineData(true, 29, 35, 0)]
    [InlineData(false, 40, 40, 2)]
    public void ContentQueriesAndConfirmsTheSelectedAlgorithmThroughTheSameStagedScene(bool half, int hp, int expected, int draws)
    {
        var rules = Rules(half); var session = Open(rules, d => d["start"]!["actors"]![0]!["hp"] = hp);
        var before = session.Current; var actor = before.Selection!.Actor;
        var query = session.QueryBattleChoices();
        Assert.Same(before, session.Current); Assert.Same(rules, session.Rules);
        Assert.Equal((before.SessionId, before.Revision, actor, BattleSelectionStage.Movement, before.Selection.Preview.Destination),
            (query.SessionId, query.Revision, query.Actor!.Value, query.Stage!.Value, query.Origin!));
        var option = Assert.Single(query.Spells); Assert.True(option.Enabled);
        Assert.Equal(BattlePresentationKind.Healing, option.Presentation);
        Assert.Equal(new[] { actor, new ActorRef("guard-a") }, option.Targets.Select(t => t.Actor));
        Assert.True(option.Targets[0].Enabled);
        Assert.Equal(!half, option.Targets[1].Enabled);
        if (half) Assert.Equal("target-not-injured", option.Targets[1].Reason!.Code);
        Select(session);
        Assert.Same(before.Battle, session.Current.Battle);
        var prepared = Accept(session, new Confirm());
        Assert.Equal(draws, prepared.Observations.Count(row => row.RandomRange is not null));
        Assert.Equal(hp, session.Current.Battle.GetActor(actor).Hp);
        Assert.Equal(8, session.Current.Battle.GetActor(actor).Mp);
        Assert.Equal((byte?)0, session.Current.Battle.GetActor(actor).Exp);
        Assert.Equal(half, session.Current.Battle.MainSeed == before.Battle.MainSeed);
        var ended = FinishBattleScenes(session, prepared);
        var healed = ended.Snapshot.Battle.GetActor(actor);
        Assert.Equal(expected, healed.Hp); Assert.Equal(5, healed.Mp);
        Assert.Equal((byte?)(half ? 0 : 10), healed.Exp);
        Assert.Single(ended.Observations, row => row.Kind == "mp" && row.Actor == actor);
        Assert.Equal(half ? 0 : 1, ended.Observations.Count(row => row.Kind == "exp"));
        Assert.Single(ended.Observations, row => row.Kind == "after-turn" && row.Actor == actor);
        Assert.Null(session.Current.BattleScene); Assert.NotEqual(actor, session.Current.Selection!.Actor);
        Assert.Equal(before.Battle.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        // The fixed scene still does fairy work under B; only award draws are omitted.
        Assert.Contains(ended.Observations, row => row.Kind.StartsWith("rng-fairy", StringComparison.Ordinal));
    }

    [Fact]
    public void DefaultCompositionRetainsIndependentSourceClassAndAwardSemantics()
    {
        var session = Start("healing-rule-demo");
        Assert.Equal("sf2", session.Rules.Identity); Select(session);
        var seed = session.Current.Battle.MainSeed;
        var prepared = Accept(session, new Confirm());
        Assert.Equal(new ushort?[] { 12, 15 }, prepared.Observations.Where(row => row.RandomRange == 16).Select(row => row.RandomValue));
        Assert.Equal(0xFF4D1234u, prepared.Snapshot.Battle.MainSeed);
        Assert.Equal(0xFB731234u, seed);
        FinishBattleScenes(session, prepared);
        Assert.Equal((ushort)40, session.Current.Battle.GetActor(new("medic-a")).Hp);
        var ordinary = Start("healing-rule-demo", d => d["actors"]![0]!["classRule"] = "ordinary");
        Assert.Equal("healing-class", Assert.Single(ordinary.QueryBattleChoices().Spells).Reason!.Code);
        Accept(ordinary, new Confirm()); var before = ordinary.Current;
        Assert.Equal("healing-class", Send(ordinary, new SelectSpell(new("mend", 1))).Failure!.Code);
        Assert.Same(before, ordinary.Current);
    }

    [Fact]
    public void RuleOwnedRecoveryDrawCompletesThroughTheSessionWithCarriedRng()
    {
        var session = Open(new("random-recovery-test", new RandomRecoveryRule()));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        var before = session.Current; var actor = before.Selection!.Actor;
        Assert.True(Assert.Single(session.QueryBattleChoices().Spells).Targets[0].Enabled);
        Assert.Same(before, session.Current);
        Select(session);
        var draw = BattleRandom.NextMain(before.Battle.MainSeed, 10);
        var prepared = Accept(session, new Confirm());
        var fact = Assert.Single(prepared.Observations, row => row.RandomRange is not null);
        Assert.Equal("rng-recovery", fact.Kind); Assert.Equal((ushort?)10, fact.RandomRange);
        Assert.Equal((long?)draw.Before, fact.Before); Assert.Equal((long?)draw.After, fact.After);
        Assert.Equal((ushort?)draw.Value, fact.RandomValue);
        Assert.Equal(draw.After, session.Current.Battle.MainSeed);
        Assert.Equal(new MapPosition(4, 3), session.Current.Battle.GetActor(actor).Position);
        Assert.Equal(30, session.Current.Battle.GetActor(actor).Hp);
        Assert.Equal(8, session.Current.Battle.GetActor(actor).Mp);
        var ended = FinishBattleScenes(session, prepared);
        var healed = ended.Snapshot.Battle.GetActor(actor);
        Assert.Equal(31 + draw.Value, healed.Hp); Assert.Equal(5, healed.Mp);
        Assert.Equal((byte?)0, healed.Exp);
        Assert.Single(ended.Observations, row => row.Kind == "hp" && row.Actor == actor);
        Assert.Single(ended.Observations, row => row.Kind == "mp" && row.Actor == actor);
        Assert.DoesNotContain(ended.Observations, row => row.Kind == "exp" || row.Kind.StartsWith("rng-exp-", StringComparison.Ordinal));
        Assert.Single(ended.Observations, row => row.Kind == "after-turn" && row.Actor == actor);
        uint seed = before.Battle.MainSeed;
        foreach (var observed in ended.Observations.Where(row => row.RandomRange is not null))
        {
            var next = BattleRandom.NextMain(seed, observed.RandomRange!.Value);
            Assert.Equal((long?)seed, observed.Before); Assert.Equal((long?)next.After, observed.After);
            Assert.Equal((ushort?)next.Value, observed.RandomValue); seed = next.After;
        }
        Assert.Equal(seed, ended.Snapshot.Battle.MainSeed);
        Assert.Equal(before.Battle.ThinkingSeed, ended.Snapshot.Battle.ThinkingSeed);
        Assert.Null(session.Current.BattleScene);
        Assert.Equal(new ActorRef("guard-a"), session.Current.Selection!.Actor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProvisionalMovementCancellationAndIllegalTargetsUseLiveQueryAndConfirm(bool half)
    {
        var session = Open(Rules(half), d => d["start"]!["actors"]![1]!["hp"] = 30);
        var initial = session.Current;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        var moved = session.QueryBattleChoices();
        Assert.Equal(new MapPosition(4, 3), moved.Origin);
        Assert.Equal("target-range", Assert.Single(moved.Spells).Targets[1].Reason!.Code);
        Assert.True(Assert.Single(moved.Spells).Targets[0].Enabled); // Self is at provisional destination.
        FinishMovement(session, Accept(session, new Cancel()));
        Assert.Same(initial.Battle, session.Current.Battle);
        Select(session, new("guard-a")); var ready = session.Current;
        foreach (var target in new[] { "dummy-a", "absent" })
        {
            Assert.Equal("invalid-heal-target", Send(session, new SelectTarget(new(target))).Failure!.Code);
            Assert.Same(ready, session.Current);
        }
        var completed = FinishBattleScenes(session, Accept(session, new Confirm()));
        Assert.Equal(half ? 35 : 40, completed.Snapshot.Battle.GetActor(new("guard-a")).Hp);
        Assert.Equal(30, completed.Snapshot.Battle.GetActor(new("medic-a")).Hp);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResourceDeadRangeAndFullHpReasonsCannotSpendRng(bool half)
    {
        var session = Open(Rules(half), d => d["start"]!["actors"]![0]!["mp"] = 2);
        Assert.Equal("insufficient-mp", Assert.Single(session.QueryBattleChoices().Spells).Reason!.Code);
        Accept(session, new Confirm()); var before = session.Current;
        Assert.Equal("insufficient-mp", Send(session, new SelectSpell(new("mend", 1))).Failure!.Code);
        Assert.Same(before, session.Current);
        session = Open(Rules(half), d => d["start"]!["actors"]![1]!["hp"] = 0);
        Assert.Single(Assert.Single(session.QueryBattleChoices().Spells).Targets);
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("mend", 1))); before = session.Current;
        Assert.Equal("invalid-heal-target", Send(session, new SelectTarget(new("guard-a"))).Failure!.Code);
        Assert.Same(before, session.Current);
        if (!half) return;
        session = Open(Rules(true), d => d["start"]!["actors"]![0]!["hp"] = 40);
        Assert.False(Assert.Single(session.QueryBattleChoices().Spells).Enabled);
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("mend", 1))); before = session.Current;
        Assert.Equal("target-not-injured", Send(session, new SelectTarget(before.Selection!.Actor)).Failure!.Code);
        Assert.Same(before, session.Current);
    }

    [Theory]
    [InlineData("throw")]
    [InlineData("hp")]
    [InlineData("target")]
    [InlineData("foreign")]
    [InlineData("mp")]
    [InlineData("seed")]
    [InlineData("queue")]
    [InlineData("effect")]
    [InlineData("rng-chain")]
    [InlineData("rng-after")]
    [InlineData("rng-value")]
    [InlineData("rng-range")]
    [InlineData("rng-final")]
    public void BrokenRulePreparationPublishesNoMovementResourcesRngOrObservations(string fault)
    {
        var rule = new BrokenRule(fault); var session = Open(new("broken-test-rule", rule));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
        Select(session); var before = session.Current;
        var result = Send(session, new Confirm());
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        Assert.Contains("broken-test-rule", result.Failure.Message);
        Assert.Equal("rules.healing.prepare", result.Failure.Field);
        Assert.Empty(result.Observations); Assert.Same(before, result.Snapshot); Assert.Same(before, session.Current);
        Assert.Equal(new MapPosition(3, 3), before.Battle.GetActor(before.Selection!.Actor).Position);
        Assert.Equal(new MapPosition(4, 3), before.Selection.Preview.Destination);
        Assert.Equal(1, rule.Preparations);
    }

    [Fact]
    public void QueryAndChangedHistoryNeverInvokePreparationOrAcceptStaleActorCommands()
    {
        var rule = new BrokenRule("throw"); var session = Open(new("broken-test-rule", rule));
        var query = session.QueryBattleChoices(); Assert.Equal(0, rule.Preparations);
        Select(session); var ready = session.Current;
        var obsolete = new CommandEnvelope(query.SessionId, query.Revision, query.Actor, new Confirm());
        Assert.Equal("stale-input", session.Submit(obsolete).Failure!.Code);
        Assert.Equal("wrong-actor", session.Submit(new(ready.SessionId, ready.Revision, new("guard-a"), new Confirm())).Failure!.Code);
        Assert.Equal(0, rule.Preparations); Assert.Same(ready, session.Current);
        FinishMovement(session, Accept(session, new Cancel())); Stay(session);
        var after = session.Current;
        Assert.Equal("stale-input", session.Submit(new(ready.SessionId, ready.Revision, ready.Selection!.Actor, new Confirm())).Failure!.Code);
        Assert.Same(after, session.Current); Assert.Equal(0, rule.Preparations);
        Assert.Equal(after.Selection!.Actor, session.QueryBattleChoices().Actor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnsupportedLevelFourIsVisibleInQueryAndConfirmEvenForAuthoredAlgorithms(bool half)
    {
        var session = Open(Rules(half), d =>
        { d["spells"]![0]!["level"] = 4; d["actors"]![0]!["spells"]![0]!["level"] = 4; });
        Assert.Equal("healing-animation", Assert.Single(session.QueryBattleChoices().Spells).Reason!.Code);
        Accept(session, new Confirm()); var before = session.Current;
        Assert.Equal("healing-animation", Send(session, new SelectSpell(new("mend", 4))).Failure!.Code);
        Assert.Same(before, session.Current);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingPrivateCastSupportIsUnsupportedBeforePublicationUnderEitherAlgorithm(bool half)
    {
        var session = Open(Rules(half)); Select(session); var before = session.Current;
        var query = BattleChoices.Query(before, session.Rules, null, requireContent: true);
        Assert.Equal("healing-scene-content", Assert.Single(query.Spells).Reason!.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, Assert.Single(query.Spells).Targets[0].Reason!.Kind);
        Assert.Equal(half ? "target-not-injured" : "healing-scene-content", Assert.Single(query.Spells).Targets[1].Reason!.Code);
        var result = BattleCommandDispatcher.Submit(before, new Confirm(), requireSceneContent: true, rules: session.Rules);
        Assert.Equal("healing-scene-content", result.Failure!.Code);
        Assert.Same(before, result.Snapshot); Assert.Empty(result.Observations);
    }

    [Fact]
    public void LaterGrowthFailureRetainsPreviouslyCommittedHealAndTheLiveSceneToken()
    {
        var session = Open(Rules(false)); Select(session); Accept(session, new Confirm());
        while (session.Current.BattleScene!.Phase != BattleScenePhase.RewardMessage)
        {
            var scene = session.Current.BattleScene;
            Accept(session, scene.Healing is { LogicalComplete: false, AtTimedInput: false }
                ? new AdvanceSimulation(scene.Token) : scene.RequiresAcknowledgement
                    ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
        }
        var committed = session.Current; var actor = committed.Battle.GetActor(new("medic-a"));
        Assert.Equal(40, actor.Hp); Assert.Equal(5, actor.Mp); Assert.Equal((byte?)10, actor.Exp);
        // Exercise a reached later Unsupported branch at the continuation's unit
        // boundary, without replacing preparation with a whole-action rollback.
        var battle = committed.Battle.With(actors: committed.Battle.Actors.Select(a => a.Actor == actor.Actor ? a.With(exp: 100) : a));
        var pending = new SessionSnapshot(committed.SessionId, committed.Revision, committed.ObservationSequence,
            new ActiveBattle(battle, null, committed.BattleScene), committed.Story, committed.StopReason);
        var token = pending.BattleScene!.Token;
        var failure = BattleSceneContinuation.Submit(pending, new Acknowledge(token));
        Assert.Equal("level-up", failure.Failure!.Code); Assert.Same(pending, failure.Snapshot);
        Assert.Empty(failure.Observations); Assert.Equal(token, failure.Snapshot.BattleScene!.Token);
        Assert.Equal(40, failure.Snapshot.Battle.GetActor(actor.Actor).Hp);
        Assert.Equal(5, failure.Snapshot.Battle.GetActor(actor.Actor).Mp);
        Assert.Equal((byte?)100, failure.Snapshot.Battle.GetActor(actor.Actor).Exp);
        Assert.Equal(battle.MainSeed, failure.Snapshot.Battle.MainSeed);
        Assert.Equal(battle.Cursor, failure.Snapshot.Battle.Cursor); Assert.False(failure.Snapshot.HasBattleControl);
        Assert.Same(pending, BattleSceneContinuation.Submit(pending, new Acknowledge(token)).Snapshot);
    }

    [Theory]
    [InlineData("spell-throw")]
    [InlineData("spell-result")]
    [InlineData("query-result")]
    [InlineData("target-throw")]
    [InlineData("target-result")]
    public void FaultyRuleAdmissionOrQueryIsAnInvariantFailureWithoutLiveMutation(string fault)
    {
        var session = Open(new("broken-test-rule", new BrokenRule(fault)));
        var initial = session.Current;
        var option = Assert.Single(session.QueryBattleChoices().Spells);
        Assert.False(option.Enabled); Assert.Equal(SessionFailureKind.InvariantFailure, option.Reason!.Kind);
        Assert.DoesNotContain("private detail", option.Reason.Message); Assert.Same(initial, session.Current);
        if (fault == "query-result") return;
        Accept(session, new Confirm()); var before = session.Current;
        if (fault.StartsWith("target", StringComparison.Ordinal))
        { Accept(session, new SelectSpell(new("mend", 1))); before = session.Current; }
        var result = Send(session, fault.StartsWith("target", StringComparison.Ordinal)
            ? new SelectTarget(before.Selection!.Actor) : new SelectSpell(new("mend", 1)));
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        Assert.Empty(result.Observations); Assert.Same(before, session.Current);
    }

    // Test-only rule uses the existing preparation shape and real scene consumer.
    private sealed class RandomRecoveryRule : AuthoredHealingRule
    {
        public override string Identity => "random-recovery-test";
        protected override BattleActionResolution Calculate(EngineBattleState battle, BattleActorState actor,
            BattleActorState target, MapPosition destination, HealingSpellDefinition spell)
        {
            var draw = BattleRandom.NextMain(battle.MainSeed, spell.Power);
            int recovery = Math.Min(target.MaxHp - target.Hp, 1 + draw.Value);
            return new(battle.With(mainSeed: draw.After, actors: battle.Actors.Select(a => a.Actor == actor.Actor
                    ? a.With(position: destination) : a)), actor.Actor, destination,
                [new(actor.Actor, target.Actor, "heal", BattleReactionKind.Recovery, target.Hp,
                    (ushort)(target.Hp + recovery), target.Mp, target.Mp, Amount: recovery)], null,
                [new("rng-recovery", actor.Actor, draw.Before, draw.After, draw.Range, draw.Value)], [], Spell: spell);
        }
    }

    private sealed class BrokenRule(string fault) : IHealingRule
    {
        private readonly Sf2HealingRule _source = new();
        public int Preparations { get; private set; }
        public string Identity => "broken-test-rule";
        public HealingSpellDefinition RequireSpell(EngineBattleState battle, ActorRef actor, SpellRef spell) => fault switch
        {
            "spell-throw" => throw new InvalidOperationException("private detail"),
            "spell-result" => _source.RequireSpell(battle, actor, spell) with { MpCost = 0 },
            _ => _source.RequireSpell(battle, actor, spell),
        };
        public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor) =>
            fault == "query-result" ? [new("absent")] : _source.QueryTargets(battle, actor);
        public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination, HealingSpellDefinition spell, ActorRef target) =>
            fault switch
            {
                "target-throw" => throw new InvalidOperationException("private detail"),
                "target-result" => battle.GetActor(new("dummy-a")),
                _ => _source.RequireTarget(battle, actor, destination, spell, target),
            };
        public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination, SpellRef spell, ActorRef target)
        {
            Preparations++;
            var action = _source.Prepare(battle, actor, destination, spell, target); // Local random work already happened.
            return fault switch
            {
                "throw" => throw new InvalidOperationException("private detail must not leak"),
                "hp" => action with { Reactions = [action.Reactions[0] with { HpAfter = ushort.MaxValue }] },
                "target" => action with { Reactions = [action.Reactions[0] with { Target = new("guard-a") }] },
                "foreign" => action with { Prepared = action.Prepared.With(actors: action.Prepared.Actors.Select(a => a.Actor == actor ? a : a.With(hp: 1))) },
                "mp" => action with { Prepared = action.Prepared.With(actors: action.Prepared.Actors.Select(a => a.Actor == actor ? a.With(mp: 0) : a)) },
                "seed" => action with { Prepared = action.Prepared.With(thinkingSeed: 1) },
                "queue" => action with { Prepared = action.Prepared.With(cursor: battle.Cursor + 1) },
                "effect" => action with { ConstructionEffects = [action.ConstructionEffects[0] with { Kind = "gold" }, action.ConstructionEffects[1]] },
                "rng-chain" => action with { ConstructionEffects = [action.ConstructionEffects[0], action.ConstructionEffects[1] with { Before = battle.MainSeed }] },
                "rng-after" => action with { ConstructionEffects = [action.ConstructionEffects[0] with { After = action.ConstructionEffects[0].After + 1 }, action.ConstructionEffects[1]] },
                "rng-value" => action with { ConstructionEffects = [action.ConstructionEffects[0] with { RandomValue = (ushort)(action.ConstructionEffects[0].RandomValue!.Value + 1) }, action.ConstructionEffects[1]] },
                "rng-range" => action with { ConstructionEffects = [action.ConstructionEffects[0] with { RandomRange = null }, action.ConstructionEffects[1]] },
                "rng-final" => action with { Prepared = action.Prepared.With(mainSeed: action.Prepared.MainSeed ^ 1) },
                _ => action,
            };
        }
    }
}

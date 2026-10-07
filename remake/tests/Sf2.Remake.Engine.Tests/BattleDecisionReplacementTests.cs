using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Authored;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleDecisionReplacementTests
{
    private static readonly ActorRef Raider = new("raider"), Sword = new("swordsman"), Lookout = new("lookout");
    private static GameSession Open(JsonNode document, SessionRules rules) =>
        Assert.IsType<SessionStarted>(GameSession.Start(Reader(document), rules)).Session;
    private static SessionRules Rules(IBattleDecisionRule decision, IPhysicalActionRule? physical = null,
        IEnumerable<KeyValuePair<BattleStrategyRef, IBattleDecisionRule>>? bindings = null) =>
        new("decision-test", new Sf2HealingRule(), physical ?? new Sf2PhysicalAction(), new Sf2ItemAction(),
            new Sf2StayAction(), bindings ?? [.. RuleCompositions.SourceDecisions(), new(new("authored-priority"), decision)]);

    [Theory]
    [InlineData(false, "swordsman", 399, 100, 487)]
    [InlineData(true, "lookout", 400, 99, 499)]
    public void SameLiveStateSelectsDifferentTargetsAndExecutesOneSelectedPhysicalAfterRealMovement(
        bool lowest, string target, int swordHp, int lookoutHp, int raiderHp)
    {
        var rule = new CountedDecision(lowest ? new LowestHpBattleDecision() : new FirstLegalBattleDecision());
        var physical = new CountedPhysical();
        var session = Open(Document("decision-rule-demo"), Rules(rule, physical));
        var before = session.Current.Battle;
        Assert.Equal(Sword, session.Current.Selection!.Actor);
        // Independent source generator: five ordinary actors * three draws from high word55 ->00C0.
        Assert.Equal(0x00C01234u, before.MainSeed);
        var begun = Stay(session);
        Assert.Equal(1, rule.Calls); Assert.Equal(1, physical.Preparations);
        Assert.NotNull(begun.Snapshot.BattleMovement);
        Assert.Equal(before.MainSeed, begun.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, begun.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(before.Actors.Select(actor => (actor.Hp, actor.Position)),
            begun.Snapshot.Battle.Actors.Select(actor => (actor.Hp, actor.Position)));
        Assert.Equal(new ActorRef(target), begun.Snapshot.Battle.GetActor(Raider).LastTarget);
        Assert.Equal(1, begun.Snapshot.Battle.Cursor); Assert.Equal(Raider, BattleTurnFlow.QueuedActor(begun.Snapshot.Battle).Actor);
        Assert.DoesNotContain(begun.Observations, row => row.Kind.StartsWith("rng-", StringComparison.Ordinal));
        var moved = FinishMovement(session, begun);
        Assert.NotNull(session.Current.BattleScene); Assert.Equal(2, physical.Preparations); Assert.Equal(1, rule.Calls);
        Assert.NotEqual(before.GetActor(Raider).Position, session.Current.Battle.GetActor(Raider).Position);
        // Accepted scalar rules:14 construction draws, first damage1 and counter13/1, award1.
        Assert.Equal(0x557E1234u, session.Current.Battle.MainSeed);
        Assert.Equal(before.GetActor(new(target)).Hp, session.Current.Battle.GetActor(new(target)).Hp);
        var finished = FinishBattleScenes(session, moved);
        Assert.Equal(SessionStopReason.PlayerInput, finished.StopReason); Assert.Equal(Lookout, finished.Snapshot.Selection!.Actor);
        Assert.Equal((swordHp, lookoutHp, raiderHp), ((int)finished.Snapshot.Battle.GetActor(Sword).Hp,
            (int)finished.Snapshot.Battle.GetActor(Lookout).Hp, (int)finished.Snapshot.Battle.GetActor(Raider).Hp));
        // Two damaging reaction bodies each consume24 main draws; no thinking draw/decision repeats.
        Assert.Equal(0x976E1234u, finished.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, finished.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(14, ConstructionRolls(finished).Count());
        Assert.Equal(48, finished.Observations.Count(row => row.Kind.StartsWith("rng-reaction-", StringComparison.Ordinal)));
        Assert.Equal(1, rule.Calls); Assert.Equal(2, physical.Preparations);
        Assert.Single(finished.Observations, row => row.Kind == "ai-target");
        Assert.Single(finished.Observations, row => row.Kind == "action-committed" && row.Actor == Raider);
        Assert.Single(finished.Observations, row => row.Kind == "action-committed" && row.Actor == Sword);
        Assert.Equal(2, finished.Snapshot.Battle.Cursor);
        Assert.Equal(100u, finished.Snapshot.Battle.Gold);
        Assert.Equal((byte)1, finished.Snapshot.Battle.GetActor(new(target)).Exp);
        Assert.Same(session.Rules.Physical, physical);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothAuthoredPoliciesUseStableProcessingOrderForEqualHpRegardlessOfInputArrayOrder(bool lowest)
    {
        var document = Document("decision-rule-demo"); document["start"]!["actors"]![0]!["hp"] = 100;
        document["encounters"]![0]!["placements"]![0]!["processingOrder"] = 20;
        document["encounters"]![0]!["placements"]![1]!["processingOrder"] = 3;
        var placements = document["encounters"]![0]!["placements"]!.AsArray();
        var reversed = new JsonArray(placements.Reverse().Select(row => row!.DeepClone()).ToArray());
        document["encounters"]![0]!["placements"] = reversed;
        var session = Open(document, lowest ? RuleCompositions.AuthoredLowestHp() : RuleCompositions.AuthoredFirstLegal());
        var begun = Stay(session);
        Assert.Equal(Lookout, begun.Snapshot.Battle.GetActor(Raider).LastTarget);
        Assert.NotNull(begun.Snapshot.BattleMovement);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AnUnreachableEarlierTargetIsNotAFirstLegalCandidate(bool lowest)
    {
        var document = Document("decision-rule-demo");
        document["actors"]![2]!["move"] = 1;
        document["encounters"]![0]!["placements"]![0]!["x"] = 1;
        document["encounters"]![0]!["placements"]![0]!["y"] = 1;
        document["encounters"]![0]!["placements"]![1]!["x"] = 5;
        document["encounters"]![0]!["placements"]![1]!["y"] = 2;
        var session = Open(document, lowest ? RuleCompositions.AuthoredLowestHp() : RuleCompositions.AuthoredFirstLegal());
        Assert.Equal(Lookout, Stay(session).Snapshot.Battle.GetActor(Raider).LastTarget);
    }

    [Fact]
    public void NoPhysicalCandidateEndsTheAutomaticTurnWithoutInventingMovementOrRandomWork()
    {
        var document = Document("decision-rule-demo"); document["actors"]![2]!["move"] = 1;
        document["encounters"]![0]!["placements"]![0]!["x"] = 1;
        document["encounters"]![0]!["placements"]![0]!["y"] = 1;
        document["encounters"]![0]!["placements"]![1]!["x"] = 1;
        document["encounters"]![0]!["placements"]![1]!["y"] = 5;
        var session = Open(document, RuleCompositions.AuthoredFirstLegal()); var before = session.Current.Battle;
        var ended = Stay(session);
        Assert.Equal(Lookout, ended.Snapshot.Selection!.Actor);
        Assert.Equal(before.MainSeed, ended.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, ended.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(before.GetActor(Raider).Position, ended.Snapshot.Battle.GetActor(Raider).Position);
        Assert.Null(ended.Snapshot.Battle.GetActor(Raider).LastTarget);
        Assert.DoesNotContain(ended.Observations, row => row.Kind is "ai-target" or "battle-movement-segment-started");
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    [InlineData("missing")]
    [InlineData("null")]
    public void BindingRejectsAtSessionStartBeforePublishingEvenForDeadUnreachedDeployments(string fault)
    {
        var document = Document("decision-rule-demo");
        var future = document["encounters"]![0]!.DeepClone(); future["id"] = "later";
        future["placements"]![3]!["aiStrategy"] = "unresolved";
        document["encounters"]!.AsArray().Add(future);
        document["start"]!["actors"]![3]!["hp"] = 0;
        var bindings = RuleCompositions.SourceDecisions().ToList();
        bindings.Add(new(new("authored-priority"), new FirstLegalBattleDecision()));
        if (fault != "unknown") bindings.Add(new(new("unresolved"), new StayBattleDecision()));
        if (fault == "duplicate") bindings.Add(bindings[0]);
        if (fault == "missing") bindings.RemoveAll(binding => binding.Key.Value == "authored-priority");
        if (fault == "null") future["placements"]![3]!["aiStrategy"] = null;
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document), Rules(new FirstLegalBattleDecision(), bindings: bindings)));
        Assert.Equal(SessionFailureKind.UnsupportedCapability, failed.Failure.Kind);
        Assert.Equal(fault == "duplicate" ? "duplicate-ai-binding" : fault == "null" ? "control-ai" : "ai-strategy", failed.Failure.Code);
    }

    [Theory]
    [InlineData("physical", "physical-definition")]
    [InlineData("spells", "ai-action-categories")]
    [InlineData("items", "ai-action-categories")]
    [InlineData("move", "ai-movement-domain")]
    public void AuthoredReplacementRetainsTheAdmittedPhysicalOnlyCapabilityBeforeADeadActorCanBeSkipped(string fault, string code)
    {
        var document = Document("decision-rule-demo"); document["start"]!["actors"]![2]!["hp"] = 0;
        switch (fault)
        {
            case "physical": document["actors"]![2]!.AsObject().Remove("physical"); break;
            case "items":
                document["items"] = new JsonArray(new JsonObject { ["id"] = 0, ["name"] = "Recovery leaf",
                    ["effect"] = "consumable-healing", ["power"] = 10, ["minimumRange"] = 0, ["maximumRange"] = 1 });
                document["actors"]![2]!["items"] = new JsonArray(0, 127, 127, 127); break;
            case "move": document["actors"]![2]!["move"] = 64; break;
            case "spells":
                document["actors"]![2]!["spells"] = new JsonArray(new JsonObject { ["id"] = "mend", ["level"] = 1 });
                document["spells"] = new JsonArray(new JsonObject { ["id"] = "mend", ["level"] = 1, ["mpCost"] = 3,
                    ["minimumRange"] = 0, ["maximumRange"] = 1, ["effect"] = new JsonObject {
                        ["kind"] = "heal", ["adjustedPower"] = 15, ["fullRecovery"] = false } }); break;
        }
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document), RuleCompositions.AuthoredFirstLegal()));
        Assert.Equal(code, failed.Failure.Code);
    }

    [Theory]
    [InlineData("throw")]
    [InlineData("hp")]
    [InlineData("foreign-memory")]
    [InlineData("position")]
    [InlineData("queue")]
    [InlineData("main-rng")]
    [InlineData("thinking-rng")]
    [InlineData("rng-fact")]
    [InlineData("path")]
    [InlineData("blocked")]
    [InlineData("missing-target")]
    [InlineData("same-side-target")]
    [InlineData("target-range")]
    [InlineData("queue-only")]
    public void FailedDecisionPublishesNothingAndRetainsEarlierPlayerCommitAndAutomaticQueueEntry(string fault)
    {
        var rule = new BrokenDecision(fault);
        var session = Open(Document("decision-rule-demo"), Rules(rule)); var before = session.Current.Battle;
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var result = Send(session, new Confirm());
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        Assert.Equal("decision-rule-failure", result.Failure.Code); Assert.Contains(rule.Identity, result.Failure.Message);
        Assert.DoesNotContain("private detail", result.Failure.Message);
        Assert.Equal(SessionStopReason.Faulted, result.StopReason);
        Assert.Equal(before.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(before.Actors.Select(actor => (actor.Hp, actor.Position, actor.LastTarget, actor.AiMemory)),
            result.Snapshot.Battle.Actors.Select(actor => (actor.Hp, actor.Position, actor.LastTarget, actor.AiMemory)));
        Assert.Equal(1, result.Snapshot.Battle.Cursor); Assert.Equal(Raider, BattleTurnFlow.QueuedActor(result.Snapshot.Battle).Actor);
        Assert.Single(result.Observations, row => row.Kind == "action-committed" && row.Actor == Sword);
        Assert.DoesNotContain(result.Observations, row => row.Kind == "ai-target" || row.Kind.StartsWith("thinking-", StringComparison.Ordinal));
        Assert.Null(result.Snapshot.BattleMovement); Assert.Null(result.Snapshot.BattleScene);
        var stopped = session.Current;
        Assert.NotNull(Send(session, new AdvanceSimulation()).Failure); Assert.Same(stopped, session.Current);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PhysicalRoutesRejectOpposingTransitAndRetainFriendlyTransitToTheSameReachableStop(bool opponent)
    {
        var document = Document("decision-rule-demo");
        var blocker = opponent ? Lookout : new ActorRef("scavenger");
        var placement = document["encounters"]![0]!["placements"]!.AsArray()
            .Single(row => row!["actor"]!.GetValue<string>() == blocker.Value)!;
        placement["x"] = 4; placement["y"] = 4;
        var physical = new CountedPhysical();
        var rule = new PhysicalRouteDecision();
        var session = Open(document, Rules(rule, physical)); var before = session.Current.Battle;
        var destination = new MapPosition(3, 4);
        // The stop is empty and legally reachable by another route; only the returned transit is wrong.
        var legal = BattleMovement.Preview(before, Raider, destination);
        Assert.Equal(destination, legal.Destination);
        Assert.True(legal.Cost <= before.GetActor(Raider).Definition.Move * 2);
        if (opponent) Assert.DoesNotContain(new MapPosition(4, 4), legal.Path);
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var result = Send(session, new Confirm());
        if (opponent)
        {
            Assert.True(result.Failure?.Kind == SessionFailureKind.InvariantFailure,
                $"Expected invariant rejection; failure={result.Failure?.Code ?? "none"}, " +
                $"movement={result.Snapshot.BattleMovement?.From}->{result.Snapshot.BattleMovement?.To}, " +
                $"thinking={result.Snapshot.Battle.ThinkingSeed:X8}, preflights={physical.Preparations}, " +
                $"observations={string.Join(",", result.Observations.Select(row => row.Kind))}");
            Assert.Equal("decision-rule-failure", result.Failure!.Code);
            Assert.Contains(rule.Identity, result.Failure.Message);
            Assert.Equal(SessionStopReason.Faulted, result.StopReason);
            Assert.Equal(before.MainSeed, result.Snapshot.Battle.MainSeed);
            Assert.Equal(before.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
            Assert.Equal(before.Actors.Select(actor => (actor.Hp, actor.Position, actor.LastTarget, actor.AiMemory)),
                result.Snapshot.Battle.Actors.Select(actor => (actor.Hp, actor.Position, actor.LastTarget, actor.AiMemory)));
            Assert.Equal(before.Queue, result.Snapshot.Battle.Queue);
            Assert.Equal(1, result.Snapshot.Battle.Cursor);
            Assert.Equal(Raider, BattleTurnFlow.QueuedActor(result.Snapshot.Battle).Actor);
            Assert.Single(result.Observations, row => row.Kind == "action-committed" && row.Actor == Sword);
            Assert.DoesNotContain(result.Observations, row => row.Actor == Raider);
            Assert.Null(result.Snapshot.BattleMovement); Assert.Null(result.Snapshot.BattleScene);
            Assert.Equal(0, physical.Preparations);
            var stopped = session.Current;
            Assert.NotNull(Send(session, new AdvanceSimulation()).Failure); Assert.Same(stopped, session.Current);
        }
        else
        {
            Assert.Null(result.Failure);
            Assert.Equal(new[] { new MapPosition(5, 4), new MapPosition(4, 4), destination },
                result.Snapshot.BattleMovement!.Path);
            Assert.Equal(before.GetActor(Raider).Position, result.Snapshot.Battle.GetActor(Raider).Position);
            Assert.Equal(before.MainSeed, result.Snapshot.Battle.MainSeed);
            Assert.NotEqual(before.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
            Assert.Single(result.Observations, row => row.Kind == "thinking-rng");
            Assert.Equal(Sword, result.Snapshot.Battle.GetActor(Raider).LastTarget);
            Assert.Equal(1, physical.Preparations);
            var finished = FinishBattleScenes(session, FinishMovement(session, result));
            Assert.Equal(SessionStopReason.PlayerInput, finished.StopReason);
            Assert.Equal(destination, finished.Snapshot.Battle.GetActor(Raider).Position);
            Assert.Equal(before.GetActor(blocker).Position, finished.Snapshot.Battle.GetActor(blocker).Position);
            Assert.Equal(before.GetActor(blocker).Hp, finished.Snapshot.Battle.GetActor(blocker).Hp);
            Assert.Equal((ushort)399, finished.Snapshot.Battle.GetActor(Sword).Hp);
            Assert.Equal((ushort)487, finished.Snapshot.Battle.GetActor(Raider).Hp);
            Assert.Equal(2, finished.Snapshot.Battle.Cursor); Assert.Equal(2, physical.Preparations);
            Assert.Single(finished.Observations, row => row.Kind == "action-committed" && row.Actor == Raider);
            Assert.Single(finished.Observations, row => row.Kind == "thinking-rng");
        }
    }

    private sealed class PhysicalRouteDecision : IBattleDecisionRule
    {
        private readonly FirstLegalBattleDecision _admission = new();
        public string Identity => "physical-route-test";
        public void RequireDeployment(BattleDeploymentDefinition deployment) => _admission.RequireDeployment(deployment);
        public void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input) => _admission.RequireStart(deployment, input);
        public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical)
        {
            var draw = BattleRandom.NextThinkingWord((ushort)(battle.ThinkingSeed >> 16), 3);
            uint seed = ((uint)draw.After << 16) | (battle.ThinkingSeed & 65535);
            var prepared = battle.With(thinkingSeed: seed, actors: battle.Actors.Select(
                row => row.Actor == actor ? row.With(lastTarget: Sword) : row));
            return new(prepared, [new("thinking-rng", actor, battle.ThinkingSeed, seed, 3, draw.Value),
                new("ai-target", actor, Target: Sword)], new(3, 4), [new(5, 4), new(4, 4), new(3, 4)], Sword);
        }
    }
    private sealed class CountedDecision(IBattleDecisionRule inner) : IBattleDecisionRule
    {
        public int Calls { get; private set; }
        public string Identity => inner.Identity;
        public void RequireDeployment(BattleDeploymentDefinition deployment) => inner.RequireDeployment(deployment);
        public void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input) => inner.RequireStart(deployment, input);
        public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical)
        { Calls++; return inner.Decide(battle, actor, physical); }
    }

    private sealed class CountedPhysical : IPhysicalActionRule
    {
        private readonly Sf2PhysicalAction _source = new();
        public int Preparations { get; private set; }
        public string Identity => _source.Identity;
        public BattleActionKind Kind => _source.Kind;
        public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) => _source.QueryActions(battle, actor);
        public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action) => _source.RequireAction(battle, actor, action);
        public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) => _source.QueryTargets(battle, actor, action);
        public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination, BattleActionRef action, ActorRef target) =>
            _source.RequireTarget(battle, actor, destination, action, target);
        public int EstimateDamage(EngineBattleState battle, ActorRef actor, ActorRef target) => _source.EstimateDamage(battle, actor, target);
        public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination, BattleActionRef action, ActorRef? target, IBattleProgressionRule progression)
        { Preparations++; return _source.Prepare(battle, actor, destination, action, target, progression); }
    }

    private sealed class BrokenDecision(string fault) : IBattleDecisionRule
    {
        private readonly FirstLegalBattleDecision _inner = new();
        public string Identity => "broken-decision-test";
        public void RequireDeployment(BattleDeploymentDefinition deployment) => _inner.RequireDeployment(deployment);
        public void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input) => _inner.RequireStart(deployment, input);
        public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical)
        {
            if (fault == "throw") throw new InvalidOperationException("private detail");
            var result = _inner.Decide(battle, actor, physical);
            return fault switch
            {
                "hp" => result with { Battle = result.Battle.With(actors: result.Battle.Actors.Select(row => row.Actor == actor ? row.With(hp: 1) : row)) },
                "foreign-memory" => result with { Battle = result.Battle.With(actors: result.Battle.Actors.Select(row => row.Actor == actor ? row : row.With(aiMemory: 1))) },
                "position" => result with { Battle = result.Battle.With(actors: result.Battle.Actors.Select(row => row.Actor == actor ? row.With(position: result.Destination) : row)) },
                "queue" => result with { Battle = result.Battle.With(cursor: battle.Cursor + 1) },
                "main-rng" => result with { Battle = result.Battle.With(mainSeed: battle.MainSeed + 1) },
                "thinking-rng" => result with { Battle = result.Battle.With(thinkingSeed: battle.ThinkingSeed + 1) },
                "rng-fact" => result with { Effects = [new("thinking-rng", actor, battle.ThinkingSeed, battle.ThinkingSeed + 1, 3, 0)] },
                "path" => result with { Path = [result.Path[0], result.Destination] },
                "blocked" => result with { Destination = new(5, 0), Path = [result.Path[0], new(5, 3), new(5, 2), new(5, 1), new(5, 0)] },
                "missing-target" => result with { Target = new("absent") },
                "same-side-target" => result with { Target = actor },
                "target-range" => result with { Destination = result.Path[0], Path = [result.Path[0]] },
                _ => result with { QueueOnly = true },
            };
        }
    }
}

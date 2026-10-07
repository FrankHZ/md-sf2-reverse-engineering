using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ProgressionPolicyTests
{
    [Theory]
    [InlineData("victory", false)]
    [InlineData("defeat", false)]
    [InlineData("victory", true)]
    [InlineData("defeat", true)]
    public void AuthoredStartExecutesTheBattleAndItsActualReturn(string outcome, bool alternate)
    {
        var session = Open(outcome, alternate ? RuleCompositions.AuthoredProgressionOutcome() : RuleCompositions.Sf2());
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        Accept(session, new Interact(new("ferryman")));
        SettleField(session);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal(new ActorRef("leader"), session.Current.Selection!.Actor);
        SessionResult result;
        if (outcome == "victory")
        {
            Accept(session, new Confirm());
            Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
            Accept(session, new SelectTarget(new("enemy")));
            result = Accept(session, new Confirm());
        }
        else result = Stay(session);
        result = FinishBattleScenes(session, result);
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        if (outcome == "victory")
        {
            var leader = session.Current.Exploration!.Party.Actors.Single(actor => actor.Actor.Value == "leader");
            Assert.Equal(2, leader.Progress!.Level);
            Assert.Equal(alternate ? 6 : 48, (int)leader.Exp!);
            Assert.Equal(0, session.Current.Exploration.Party.Actors.Single(actor => actor.Actor.Value == "companion").Hp);
        }
        SettleField(session);
        var returned = session.Current;
        Assert.Equal(SessionStopReason.PlayerInput, returned.StopReason);
        Assert.Equal(outcome == "victory" ? "arena" : "quay", returned.Exploration!.Map.Value);
        Assert.Equal(outcome == "victory" ? 84u : alternate ? 63u : 36u, returned.Exploration.Party.Gold);
        Assert.Equal(outcome == "victory", returned.Story.Flags.Contains(501));
        Assert.All(returned.Exploration.Party.Actors.Where(actor => actor.Actor.Value != "enemy"), actor => Assert.True(actor.Hp > 0));
        var position = returned.Exploration.PlayerEntity.Position;
        Accept(session, new Move(ExplorationDirection.West));
        SettleField(session);
        Assert.Equal(new MapPosition(position.X - 1, position.Y), session.Current.Exploration!.PlayerEntity.Position);
    }

    [Theory]
    [InlineData("class", "growth-class")]
    [InlineData("spells", "authored-growth-spells")]
    [InlineData("equipment", "authored-growth-equipment")]
    [InlineData("leader", "outcome-roster")]
    [InlineData("enemy-leader", "outcome-enemy-leader")]
    [InlineData("join", "outcome-join-member")]
    [InlineData("egress", "outcome-egress")]
    public void InvalidAuthoredDefinitionsCannotPublishAPlayableSession(string fault, string code)
    {
        var document = Document("progression-outcome-victory");
        switch (fault)
        {
            case "class": document["world"]!["growth"]![0]!["classId"] = 4; break;
            case "spells": document["world"]!["growth"]![0]!["spells"] = JsonNode.Parse("""[{"level":2,"packed":0,"spell":"heal"}]"""); break;
            case "equipment":
                document["battle"]!["items"] = JsonNode.Parse("""[{"id":0,"name":"herb","effect":"consumable-healing","power":10,"minimumRange":0,"maximumRange":1}]""");
                document["battle"]!["actors"]![0]!["items"] = new JsonArray(128); break;
            case "leader": document["battle"]!["actors"]![0]!["physical"]!["leader"] = false; break;
            case "enemy-leader": document["battle"]!["actors"]![2]!["physical"]!["leader"] = true; break;
            case "join": document["world"]!["maps"]![1]!["battle"]!["outcome"]!["joinMember"] = 2; break;
            case "egress": document["world"]!["maps"]![1]!["battle"]!["outcome"]!["egress"]!["position"]!["x"] = 63; break;
        }
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal(code, failed.Failure.Code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SelectedGrowthAdmissionRunsBeforePreparationAndNeverPublishesItsSeed(bool noGrowth)
    {
        var rule = new ControlledProgression { NoGrowth = noGrowth };
        var session = Enter("victory", Rules(rule), document => document["world"]!.AsObject().Remove("growth"));
        SelectAttack(session);
        var before = session.Current;
        var result = Send(session, new Confirm());
        if (!noGrowth)
        {
            Assert.Equal("level-up", result.Failure!.Code);
            Assert.Same(before, session.Current); Assert.Empty(result.Observations);
        }
        else
        {
            Assert.Null(result.Failure);
            Assert.Equal(3657634356u, session.Current.Battle.MainSeed);
            Assert.Equal((byte)99, session.Current.Battle.GetActor(new("leader")).Exp);
            FinishBattleScenes(session, result);
            Assert.Equal(1, session.Current.Exploration!.Party.Actors[0].Progress?.Level ?? 1);
            Assert.Equal((byte)148, session.Current.Exploration.Party.Actors[0].Exp);
        }
        Assert.True(rule.GrowthCalls >= 1);
    }

    [Theory]
    [InlineData("award")]
    [InlineData("award-seed")]
    [InlineData("credit-hp")]
    [InlineData("growth-hp")]
    [InlineData("growth-seed")]
    [InlineData("growth-event")]
    [InlineData("growth-spellbook")]
    public void MalformedSelectedProgressionRejectsBeforeAnyPreparationPublication(string fault)
    {
        var rule = new ControlledProgression { Fault = fault };
        var session = Enter("victory", Rules(rule));
        SelectAttack(session);
        var before = session.Current;
        var result = Send(session, new Confirm());
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        Assert.Contains(rule.Identity, result.Failure.Message);
        Assert.DoesNotContain("private detail", result.Failure.Message);
        Assert.Same(before, session.Current); Assert.Empty(result.Observations);
    }

    [Theory]
    [InlineData("credit", BattleScenePhase.DeathMessage, 99)]
    [InlineData("growth", BattleScenePhase.RewardMessage, 148)]
    [InlineData("growth-hp", BattleScenePhase.RewardMessage, 148)]
    [InlineData("growth-seed", BattleScenePhase.RewardMessage, 148)]
    public void ReachedFailureKeepsEarlierSceneCommitsAndRetryNeverCreditsTwice(string fault, BattleScenePhase phase, int exp)
    {
        var rule = new ControlledProgression();
        var session = Enter("victory", Rules(rule));
        SelectAttack(session); Accept(session, new Confirm());
        Until(session, phase);
        var before = session.Current;
        Assert.Equal(exp, (int)before.Battle.GetActor(new("leader")).Exp!);
        Assert.Equal(0, before.Battle.GetActor(new("enemy")).Hp);
        Assert.Equal(84u, before.Battle.Gold);
        rule.Fault = fault;
        var command = SceneCommand(before);
        for (int retry = 0; retry < 2; retry++)
        {
            var rejected = Send(session, command);
            Assert.Equal(SessionFailureKind.InvariantFailure, rejected.Failure!.Kind);
            Assert.Same(before, session.Current); Assert.Empty(rejected.Observations);
        }
        rule.Fault = null;
        var result = Accept(session, command);
        var completed = FinishBattleScenes(session, result);
        Assert.Equal((byte)48, session.Current.Exploration!.Party.Actors[0].Exp);
        Assert.Equal(2, session.Current.Exploration.Party.Actors[0].Progress!.Level);
        Assert.Equal(phase == BattleScenePhase.DeathMessage ? 1 : 0, completed.Observations.Count(row => row.Kind == "exp"));
        Assert.DoesNotContain(completed.Observations, row => row.Kind == "physical-first");
    }

    [Theory]
    [InlineData("start")]
    [InlineData("finish")]
    [InlineData("finish-party")]
    [InlineData("heal")]
    public void ReturnPolicyFailureRetainsTheCompletedActionAndAcceptedProgramSteps(string fault)
    {
        var policy = new ControlledReturn { Fault = fault };
        var session = Enter("victory", Rules(new ControlledProgression(), policy));
        SelectAttack(session); Accept(session, new Confirm());
        SessionResult result = new(session.Current, [], session.Current.StopReason);
        for (int guard = 0; session.Current.BattleScene is not null && guard < 200; guard++)
        {
            result = Send(session, SceneCommand(session.Current));
            if (result.Failure is not null) break;
        }
        if (fault != "start") result = Send(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        var stopped = session.Current;
        Assert.Equal(84u, stopped.Mode == SessionMode.Battle ? stopped.Battle.Gold : stopped.Exploration!.Party.Gold);
        Assert.Equal((byte)48, stopped.Mode == SessionMode.Battle ? stopped.Battle.GetActor(new("leader")).Exp : stopped.Exploration!.Party.Actors[0].Exp);
        if (fault != "start")
        {
            Assert.Null(stopped.Story.Wait); // Acknowledged text is already accepted, never replayed.
            Assert.DoesNotContain(501, stopped.Story.Flags);
            Assert.Equal(fault == "heal" ? 0 : 12, stopped.Exploration!.Party.Actors[1].Hp);
        }
        policy.Fault = null;
        var resumed = Accept(session, new AdvanceSimulation());
        Assert.DoesNotContain(resumed.Observations, row => row.Kind is "exp" or "action-committed");
        SettleField(session);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Contains(501, session.Current.Story.Flags);
        Assert.Equal((byte)48, session.Current.Exploration!.Party.Actors[0].Exp);
    }

    private static GameSession Open(string outcome, SessionRules rules, Action<JsonNode>? change = null)
    {
        var document = Document("progression-outcome-" + outcome); change?.Invoke(document);
        return Assert.IsType<SessionStarted>(GameSession.Start(Reader(document), rules)).Session;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HealingAndHerbUseTheSelectedRewardWithoutHiddenSourceDraws(bool herb)
    {
        var document = Document();
        document["start"]!["actors"]![0]!["hp"] = 1;
        if (herb)
        {
            document["items"] = JsonNode.Parse("""[{"id":0,"name":"Recovery leaf","effect":"consumable-healing","power":10,"minimumRange":0,"maximumRange":1}]""");
            document["actors"]![0]!["items"] = JsonNode.Parse("[0,127,127,127]");
        }
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document), RuleCompositions.AuthoredProgressionOutcome())).Session;
        var actor = session.Current.Selection!.Actor;
        uint initial = session.Current.Battle.MainSeed;
        var exp = session.Current.Battle.GetActor(actor).Exp;
        Accept(session, new Confirm());
        Accept(session, herb ? new SelectItem(0) : new SelectSpell(new("mend", 1)));
        Accept(session, new SelectTarget(actor));
        var prepared = Accept(session, new Confirm());
        Assert.Equal(initial, session.Current.Battle.MainSeed);
        Assert.DoesNotContain(prepared.Observations, row => row.RandomRange is not null);
        Assert.Equal(exp, session.Current.Battle.GetActor(actor).Exp);
        var finished = FinishBattleScenes(session, prepared);
        var credited = Assert.Single(finished.Observations, row => row.Kind == "exp");
        Assert.Equal(exp + 7, credited.After);
        Assert.DoesNotContain(finished.Observations, row => row.Kind.StartsWith("rng-exp-", StringComparison.Ordinal));
        Assert.True(session.Current.Battle.GetActor(actor).Hp > 1);
        if (herb) Assert.Equal((ushort)127, session.Current.Battle.GetActor(actor).SourceLoadout!.Items[0]);
    }

    [Theory]
    [InlineData("check")]
    [InlineData("invalid")]
    [InlineData("hook")]
    public void SelectedOutcomeFailuresKeepTheReachedSceneAndCanResume(string fault)
    {
        var outcome = new ControlledOutcome();
        var rules = new SessionRules("outcome-test", new Sf2HealingRule(), RuleCompositions.SourcePhysical(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), outcome: outcome);
        var session = Enter("victory", rules);
        SelectAttack(session); Accept(session, new Confirm());
        // Set after preparation: the reached hook/check must retain all prior scene work.
        outcome.Fault = fault;
        SessionResult result;
        do { result = Send(session, SceneCommand(session.Current)); }
        while (result.Failure is null && session.Current.BattleScene is not null);
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        var stopped = session.Current;
        Assert.Equal((byte)48, stopped.Battle.GetActor(new("leader")).Exp);
        Assert.Equal(84u, stopped.Battle.Gold);
        outcome.Fault = null;
        result = Accept(session, SceneCommand(stopped));
        FinishBattleScenes(session, result); SettleField(session);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal((byte)48, session.Current.Exploration!.Party.Actors[0].Exp);
    }

    private sealed class ControlledOutcome : IBattleOutcomeRule
    {
        internal string? Fault { get; set; }
        public string Identity => "controlled-outcome";
        public BattleOutcomeKind? Check(EngineBattleState battle) => Fault switch
        {
            "check" => throw new InvalidOperationException("private detail"),
            "invalid" => (BattleOutcomeKind)123,
            _ => BattleOutcomeRules.Check(battle),
        };
        public bool DefeatedHook(EngineBattleState battle) => Fault == "hook"
            ? throw new InvalidOperationException("private detail") : BattleOutcomeRules.DefeatedHook(battle);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AutomaticCounterRewardPreflightsTheSelectedGrowthWithoutCommittingPreview(bool noGrowth)
    {
        var session = Start("stone-court", document =>
        {
            document["start"]!["mainSeed"] = (55u << 16) | 0x1234u;
            foreach (int index in new[] { 0, 2 })
            {
                document["start"]!["actors"]![index]!["hp"] = 500;
                document["actors"]![index]!["maxHp"] = 500;
            }
            document["actors"]![2]!["attack"] = 18;
            document["start"]!["actors"]![0]!["exp"] = 99;
            document["start"]!["actors"]![1]!["hp"] = 0;
        });
        // Preserve the independently covered five-actor round's high-word seed for this
        // changed queue: the reducer's input seed is explicit, not a route endpoint.
        // Fifteen updates of high=(13*high+7)&65535 from55 give192.
        var battle = session.Current.Battle.With(mainSeed: 0x00C01234u);
        var rule = new ControlledProgression { NoGrowth = noGrowth };
        var actor = new ActorRef("raider");
        if (!noGrowth)
            Assert.Equal("level-up", Assert.Throws<BattleRuleException>(() => BattleDecisionRules.Decide(
                new AttackThenApproachAi(), battle, actor, new Sf2PhysicalAction(), rule)).Code);
        else
        {
            var decision = BattleDecisionRules.Decide(new AttackThenApproachAi(), battle, actor, new Sf2PhysicalAction(), rule);
            Assert.Equal(battle.MainSeed, decision.Battle.MainSeed);
            Assert.Equal((byte)99, decision.Battle.GetActor(new("swordsman")).Exp);
        }
        Assert.True(rule.GrowthCalls > 0);
    }
    private static SessionRules Rules(IBattleProgressionRule progression, IOutcomeReturnPolicy? policy = null) =>
        new("progression-test", new Sf2HealingRule(), RuleCompositions.SourcePhysical(), RuleCompositions.SourceItem(),
            RuleCompositions.SourceStay(), progression: progression, outcomeReturn: policy);
    private static GameSession Enter(string outcome, SessionRules rules, Action<JsonNode>? change = null)
    {
        var session = Open(outcome, rules, change);
        Accept(session, new Interact(new("ferryman"))); SettleField(session);
        return session;
    }
    private static void SelectAttack(GameSession session)
    {
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new("enemy")));
    }
    private static SessionCommand SceneCommand(SessionSnapshot current) => current.BattleScene!.RequiresAcknowledgement
        ? new Acknowledge(current.BattleScene.Token) : new CompletePresentation(current.BattleScene.Token, current.BattleScene.CompletionKind);
    private static void Until(GameSession session, BattleScenePhase phase)
    {
        for (int guard = 0; guard < 200 && session.Current.BattleScene!.Phase != phase; guard++) Accept(session, SceneCommand(session.Current));
        Assert.Equal(phase, session.Current.BattleScene!.Phase);
    }

    private sealed class ControlledProgression : IBattleProgressionRule
    {
        private readonly Sf2BattleProgressionRule _source = new();
        public string Identity => "controlled-progression";
        internal string? Fault { get; set; }
        internal bool NoGrowth { get; init; }
        internal int GrowthCalls { get; private set; }
        public int Award(EngineBattleState battle, BattleActorState actor, BattleActionKind kind,
            IReadOnlyList<BattleReaction> reactions, ref uint seed, List<BattleEffect> effects)
        {
            int value = _source.Award(battle, actor, kind, reactions, ref seed, effects);
            if (Fault == "award") throw new InvalidOperationException("private detail");
            if (Fault == "award-seed") seed++;
            return value;
        }
        public uint Gold(uint current, uint amount) => _source.Gold(current, amount);
        public ushort Kills(ushort value) => _source.Kills(value);
        public ushort Defeats(ushort value) => _source.Defeats(value);
        public BattleActorState Credit(BattleActorState actor, int amount, List<BattleEffect> effects)
        {
            if (Fault == "credit") throw new InvalidOperationException("private detail");
            var result = _source.Credit(actor, amount, effects);
            return Fault == "credit-hp" ? result.With(hp: 1) : result;
        }
        public BattleActorState Grow(BattleActorState actor, ref uint seed, List<BattleEffect> effects)
        {
            GrowthCalls++;
            if (NoGrowth) return actor;
            var result = _source.Grow(actor, ref seed, effects);
            if (Fault == "growth") throw new InvalidOperationException("private detail");
            if (Fault == "growth-seed") seed++;
            if (Fault == "growth-event") effects.Add(new("foreign-effect", actor.Actor));
            if (Fault == "growth-spellbook") return result.With(sourceLoadout: new(result.SourceLoadout!.Items, [0, 63, 63, 63]));
            return Fault == "growth-hp" ? result.With(hp: 1) : result;
        }
    }

    private sealed class ControlledReturn : IOutcomeReturnPolicy
    {
        private readonly Sf2OutcomeReturnPolicy _source = new();
        public string Identity => "controlled-return";
        internal string? Fault { get; set; }
        public OutcomeStart Start(ScenarioDefinition definition, EngineBattleState battle, StoryState story, BattleOutcomeKind kind, BattleStartInput party)
        {
            if (Fault == "start") throw new InvalidOperationException("private detail");
            return _source.Start(definition, battle, story, kind, party);
        }
        public OutcomeFinish Finish(ScenarioDefinition definition, StoryState story, BattleStartInput party)
        {
            if (Fault == "finish") throw new InvalidOperationException("private detail");
            var result = _source.Finish(definition, story, party);
            return Fault == "finish-party" ? result with { Returned = new(party.Encounter, party.Actors,
                party.MainSeed + 1, party.ThinkingSeed, party.Gold) } : result;
        }
        public BattleStartInput Heal(ScenarioDefinition definition, BattleStartInput party, bool all)
        {
            if (Fault == "heal") throw new InvalidOperationException("private detail");
            return _source.Heal(definition, party, all);
        }
    }

    private static void SettleField(GameSession session)
    {
        for (int guard = 0; guard < 2000 && session.Current.Mode == SessionMode.Exploration &&
            session.Current.Story.Wait is { } wait; guard++)
            Accept(session, wait switch
            {
                DialogueWait => new Acknowledge(wait.Token),
                FullFadeWait { LogicalDone: true } fade => new CompletePresentation(wait.Token, fade.Kind),
                _ => new AdvanceSimulation(wait.Token),
            });
        Assert.True(session.Current.Story.Wait is null || session.Current.Mode == SessionMode.Battle);
    }
}

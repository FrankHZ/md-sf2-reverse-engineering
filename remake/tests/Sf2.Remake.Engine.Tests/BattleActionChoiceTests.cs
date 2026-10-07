using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleActionChoiceTests
{
    private static SessionRules Rules(IPhysicalActionRule? physical = null, IBattleActionRule? item = null,
        IBattleActionRule? stay = null) => new("selected-actions-test", new Sf2HealingRule(),
            physical ?? new Sf2PhysicalAction(), item ?? new Sf2ItemAction(), stay ?? new Sf2StayAction());
    private static GameSession Open(JsonNode document, SessionRules? rules = null) =>
        Assert.IsType<SessionStarted>(GameSession.Start(Reader(document), rules ?? Rules())).Session;
    private static JsonNode Items()
    {
        var document = Document();
        document["items"] = JsonNode.Parse("""
            [{"id":0,"name":"Recovery leaf","effect":"consumable-healing","power":10,"minimumRange":0,"maximumRange":1},
             {"id":6,"name":"Small remedy","effect":"consumable-healing","power":4,"minimumRange":0,"maximumRange":2}]
            """);
        document["actors"]![0]!["items"] = JsonNode.Parse("[256,6,255,383]");
        document["actors"]![1]!["items"] = JsonNode.Parse("[0,127,127,127]");
        document["start"]!["actors"]![0]!["hp"] = 60;
        return document;
    }

    [Fact]
    public void SemanticOptionsPreserveActorOrderDisabledReasonsAndRawEmptySlotsWithoutPreparing()
    {
        var session = Open(Items()); var before = session.Current;
        var choices = session.QueryBattleChoices();
        Assert.Equal(before.SessionId, choices.SessionId); Assert.Equal(before.Revision, choices.Revision);
        Assert.Equal(before.Selection!.Preview.Destination, choices.Origin);
        Assert.Equal(new[] { BattleActionKind.Physical, BattleActionKind.Healing, BattleActionKind.Item,
            BattleActionKind.Item, BattleActionKind.Item, BattleActionKind.Item, BattleActionKind.Stay },
            choices.Actions.Select(option => option.Action.Kind));
        var physical = choices.Actions[0];
        Assert.False(physical.Enabled); Assert.Equal("physical-definition", physical.Reason!.Code);
        var herb = choices.Items[0];
        Assert.Equal("Recovery leaf", herb.Label); Assert.False(herb.Empty); Assert.True(herb.Enabled);
        Assert.Equal(new[] { new ActorRef("medic-a"), new ActorRef("guard-a") }, herb.Targets.Select(target => target.Actor));
        Assert.True(herb.Targets[0].Enabled); Assert.Equal("target-range", herb.Targets[1].Reason!.Code);
        Assert.True(choices.Items[1].Targets[1].Enabled);
        Assert.All(choices.Items.Skip(2), slot => { Assert.True(slot.Empty); Assert.False(slot.Enabled); Assert.Equal("empty-item-slot", slot.Reason!.Code); });
        Assert.True(choices.Actions[^1].Enabled); Assert.Empty(choices.Actions[^1].Targets);
        Assert.Same(before, session.Current);
        Assert.Equal(new ushort[] { 256, 6, 255, 383 }, before.Battle.Actors[0].SourceLoadout!.Items);
    }

    [Fact]
    public void PreviewChangesItemRangeAndConfirmationConsumesExactlyOneRawSlotBeforeRecovery()
    {
        var session = Open(Items()); var initial = session.Current;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Assert.Equal(new MapPosition(3, 4), session.QueryBattleChoices().Origin);
        Assert.True(session.QueryBattleChoices().Items[0].Targets[1].Enabled);
        Assert.Equal(initial.Battle.MainSeed, session.Current.Battle.MainSeed);
        Accept(session, new Confirm());
        Accept(session, new SelectBattleAction(session.QueryBattleChoices().Actions.Single(option => option.Action.ItemSlot == 0).Action));
        Accept(session, new SelectTarget(new("guard-a")));
        var before = session.Current; var stale = new CommandEnvelope(before.SessionId, before.Revision, before.Selection!.Actor, new Confirm());
        var begun = Accept(session, new Confirm());
        Assert.Equal(new ushort[] { 6, 255, 383, 127 }, session.Current.Battle.Actors[0].SourceLoadout!.Items);
        Assert.Equal(initial.Battle.Actors[1].Hp, session.Current.Battle.Actors[1].Hp);
        Assert.Equal(initial.Battle.Actors[0].Exp, session.Current.Battle.Actors[0].Exp);
        Assert.Equal(before.Battle.Cursor, session.Current.Battle.Cursor);
        Assert.Equal(initial.Battle.Gold, session.Current.Battle.Gold);
        var prepared = session.Current;
        Assert.Equal("stale-input", session.Submit(stale).Failure!.Code); Assert.Same(prepared, session.Current);
        var completed = FinishBattleScenes(session, begun);
        Assert.Single(completed.Observations, row => row.Kind == "item-consumed");
        Assert.Single(completed.Observations, row => row.Kind == "action-committed");
        Assert.Equal((ushort)12, completed.Snapshot.Battle.Actors[1].Hp);
        Assert.Equal(new("guard-a"), completed.Snapshot.Selection!.Actor);
        Assert.Equal(initial.Battle.ThinkingSeed, completed.Snapshot.Battle.ThinkingSeed);
    }

    [Fact]
    public void CurrentUnsupportedAndEmptySlotsRemainVisibleAndRejectWithoutPublication()
    {
        var session = Open(Items()); var before = session.Current;
        var actor = before.Battle.Actors[0];
        var battle = before.Battle.With(actors: before.Battle.Actors.Select(row => row.Actor == actor.Actor
            ? row.With(sourceLoadout: new([199, 6, 255, 383], actor.SourceLoadout!.Spells)) : row));
        var snapshot = new SessionSnapshot(before.SessionId, before.Revision, before.ObservationSequence, battle,
            new(actor.Actor, before.Selection!.Preview, BattleSelectionStage.ActionChoice), SessionStopReason.PlayerInput);
        var choices = BattleChoices.Query(snapshot, session.Rules, null, false);
        Assert.Equal("Item 71", choices.Items[0].Label); Assert.False(choices.Items[0].Empty);
        Assert.Equal("item-effect", choices.Items[0].Reason!.Code);
        foreach (int slot in new[] { 0, 2, 3, 4, -1 })
        {
            var result = BattleCommandDispatcher.Submit(snapshot, new SelectItem(slot), session.Rules);
            Assert.Same(snapshot, result.Snapshot); Assert.Empty(result.Observations);
            Assert.Equal(slot == 0 ? "item-effect" : slot is 2 or 3 ? "empty-item-slot" : "item-slot", result.Failure!.Code);
        }
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void PhysicalQueryUsesPreviewAndTheSelectedRuleForBothQueryAndConfirm()
    {
        var rule = new SmallPhysicalRule(block: "raider");
        var session = Open(Document("stone-court"), Rules(rule));
        var options = session.QueryBattleChoices().Actions[0].Targets;
        Assert.Equal(new[] { "raider", "scavenger", "sentry" }, options.Select(option => option.Actor.Value));
        Assert.Equal("selected-target-disabled", options[0].Reason!.Code);
        Assert.True(options[1].Enabled); Assert.Equal("target-range", options[2].Reason!.Code);
        Assert.Empty(rule.Preparations);
        Accept(session, new Confirm()); Accept(session, new SelectBattleAction(new(BattleActionKind.Physical)));
        var before = session.Current;
        Assert.Equal("selected-target-disabled", Send(session, new SelectTarget(new("raider"))).Failure!.Code);
        Assert.Same(before, session.Current);
        Accept(session, new SelectTarget(new("scavenger"))); var begun = Accept(session, new Confirm());
        Assert.Single(rule.Preparations); Assert.Equal(15, session.Current.Battle.GetActor(new("scavenger")).Hp);
        var completed = FinishBattleScenes(session, begun);
        Assert.Equal(13, completed.Snapshot.Battle.GetActor(new("scavenger")).Hp);
        Assert.Single(completed.Observations, row => row.Kind == "action-committed");
        Assert.Equal(100u, completed.Snapshot.Battle.Gold); Assert.Equal((byte)0, completed.Snapshot.Battle.Actors[0].Exp);
        // Moving the preview away changes the same range query; no preparation/RNG is cached.
        session = Open(Document("stone-court"), Rules(new SmallPhysicalRule()));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        Assert.All(session.QueryBattleChoices().Actions[0].Targets, target => Assert.False(target.Enabled));
        Assert.Equal(new MapPosition(3, 3), session.Current.Battle.Actors[0].Position);
    }

    private static JsonNode Automatic()
    {
        var document = Document("stone-court");
        document["actors"]![2]!["move"] = 3;
        document["encounters"]![0]!["placements"]![2]!["aiStrategy"] = "attack-then-approach";
        document["encounters"]![0]!["placements"]![2]!["x"] = 7;
        document["encounters"]![0]!["placements"]![3]!["x"] = 6;
        document["encounters"]![0]!["placements"]![3]!["y"] = 5;
        return document;
    }

    [Fact]
    public void SelectedPhysicalEstimatePreflightAndPostMovementExecutionShareOneCalculator()
    {
        var rule = new SmallPhysicalRule(); var session = Open(Automatic(), Rules(rule));
        var initial = session.Current.Battle;
        var begun = Stay(session);
        Assert.NotEmpty(rule.Estimates); Assert.Single(rule.Preparations);
        Assert.Equal(new("raider"), rule.Estimates[0].Actor);
        Assert.NotNull(session.Current.BattleMovement); Assert.Null(session.Current.BattleScene);
        Assert.Equal(initial.MainSeed, session.Current.Battle.MainSeed);
        var decision = session.Current.Battle;
        var target = decision.GetActor(new("raider")).LastTarget!.Value;
        var moved = FinishMovement(session, begun);
        Assert.Equal(2, rule.Preparations.Count);
        Assert.Equal(rule.Preparations[0], rule.Preparations[1]);
        Assert.Equal(decision.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.DoesNotContain(moved.Observations, row => row.Kind == "thinking-rng" && row.Sequence > begun.Snapshot.ObservationSequence);
        var completed = FinishBattleScenes(session, moved);
        Assert.Equal(initial.GetActor(target).Hp - 2, completed.Snapshot.Battle.GetActor(target).Hp);
        Assert.Equal(100u, completed.Snapshot.Battle.Gold);
        Assert.Single(completed.Observations, row => row.Kind == "action-committed" && row.Actor == new ActorRef("raider"));
    }

    [Fact]
    public void SelectedEstimateChangesTheExistingAiPriorityWithoutChangingItsThinkingDraws()
    {
        var initial = Open(Automatic()).Current.Battle;
        // Give one reachable candidate a zero thinking draw, so existing script3 uses lethality.
        uint thinking = Enumerable.Range(0, 65536).Select(value => ((uint)value << 16) | 0x42u)
            .First(seed => BattleRandom.NextThinkingWord((ushort)(seed >> 16), 3).Value == 0);
        var battle = initial.With(thinkingSeed: thinking);
        var low = EnemyPhysicalDecision.TryResolve(battle, new("raider"), new SmallPhysicalRule(estimate: 0))!;
        var high = EnemyPhysicalDecision.TryResolve(battle, new("raider"), new SmallPhysicalRule(estimate: 1000))!;
        var lowCandidates = low.Effects.Where(row => row.Kind == "ai-candidate").ToArray();
        var highCandidates = high.Effects.Where(row => row.Kind == "ai-candidate").ToArray();
        Assert.Equal(1, lowCandidates[0].After); Assert.Equal(16, highCandidates[0].After);
        Assert.Equal(low.Battle.ThinkingSeed, high.Battle.ThinkingSeed);
        Assert.Equal(battle.MainSeed, low.Battle.MainSeed); Assert.Equal(battle.MainSeed, high.Battle.MainSeed);
    }

    [Fact]
    public void SourceOrdersPassesTheSelectedPhysicalCalculatorWithoutReplacingActivationOrThinkingPolicy()
    {
        var initial = Open(Automatic()).Current.Battle;
        var deployments = initial.Definition.Deployments.Select(row => row.Actor == new ActorRef("raider")
            ? row with { AiStrategy = new BattleStrategyRef("source-orders"), Initialization = new(0x2000, 0, 15, 15, 0x60, 6, 255, 255) } : row).ToArray();
        var definition = new BattleDefinition(initial.Definition.Encounter, initial.Definition.Map,
            initial.Definition.Width, initial.Definition.Height, initial.Definition.Terrain, deployments,
            initial.Definition.Spells.Values, initial.Definition.Rewards);
        var battle = new EngineBattleState(definition, initial.Actors.Select(actor => new BattleActorState(
            deployments.Single(row => row.Actor == actor.Actor), actor.Hp, actor.Mp, actor.Exp, actor.Position,
            actor.Kills, actor.Defeats, activationWord: actor.Actor == new ActorRef("raider") ? (ushort)0x2061 : (ushort)0)),
            initial.MainSeed, initial.ThinkingSeed, initial.Round, initial.Queue, initial.Cursor, initial.Gold,
            regions: new(new bool[16], 7));
        var rule = new SmallPhysicalRule();
        var selected = SourceEnemyAi.Resolve(battle, new("raider"), rule);
        var source = SourceEnemyAi.Resolve(battle, new("raider"));
        Assert.NotEmpty(rule.Estimates); Assert.Single(rule.Preparations);
        Assert.Equal(source.Battle.ThinkingSeed, selected.Battle.ThinkingSeed);
        Assert.Equal(battle.MainSeed, selected.Battle.MainSeed);
        Assert.Equal((ushort)0, selected.Battle.Regions!.Tested);
        Assert.Equal((ushort)0x2061, selected.Battle.GetActor(new("raider")).ActivationWord);
        Assert.Contains(selected.Effects, effect => effect.Kind == "ai-command-attack1" && effect.After == 0);
    }

    [Fact]
    public void InvalidSelectedEstimateStopsBeforeAutomaticDecisionPublication()
    {
        var rule = new SmallPhysicalRule(estimate: -1); var session = Open(Automatic(), Rules(rule));
        var before = session.Current.Battle;
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var result = Send(session, new Confirm());
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        Assert.Equal("rules.physical.ai-estimate", result.Failure.Field);
        Assert.Equal(before.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.Null(result.Snapshot.Battle.GetActor(new("raider")).LastTarget);
        Assert.Empty(rule.Preparations);
        Assert.Single(result.Observations, row => row.Kind == "action-committed"); // Earlier player Stay remains committed.
        Assert.DoesNotContain(result.Observations, row => row.Kind == "thinking-rng");
    }

    [Theory]
    [InlineData("estimate")]
    [InlineData("preflight")]
    [InlineData("post-decision")]
    public void CompletedHealIsNotRolledBackWhenTheNextSelectedPhysicalRuleFails(string fault)
    {
        var document = Automatic();
        document["spells"] = Document()["spells"]!.DeepClone();
        document["actors"]![0]!["classRule"] = "unpromoted-priest";
        document["actors"]![0]!["maxMp"] = 8;
        document["actors"]![0]!["spells"] = Document()["actors"]![0]!["spells"]!.DeepClone();
        document["start"]!["actors"]![0]!["mp"] = 8;
        document["start"]!["actors"]![0]!["hp"] = 20;
        document["actors"]![1]!["agility"] = 1;
        document["actors"]![2]!["agility"] = 20;
        if (fault == "post-decision") document["encounters"]![0]!["placements"]![2]!["x"] = 2;
        var rule = new SmallPhysicalRule(estimate: fault == "estimate" ? -1 : 1000,
            failPreflight: fault == "preflight", failAfterPreflight: fault == "post-decision");
        var session = Open(document, Rules(rule));
        uint startingSeed = session.Current.Battle.MainSeed;
        var actor = session.Current.Selection!.Actor;
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("mend", 1)));
        Accept(session, new SelectTarget(actor));
        var begun = Accept(session, new Confirm());
        var observations = begun.Observations.ToList();
        while (session.Current.BattleScene!.Phase != BattleScenePhase.End ||
            !session.Current.BattleScene.Healing!.LogicalComplete)
        {
            var scene = session.Current.BattleScene;
            var step = Accept(session, scene.Healing is { LogicalComplete: false, AtTimedInput: false }
                ? new AdvanceSimulation(scene.Token) : scene.RequiresAcknowledgement
                    ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
            observations.AddRange(step.Observations);
        }
        var completedHeal = session.Current;
        var resources = completedHeal.Battle.GetActor(actor);
        Assert.Equal(35, resources.Hp); Assert.Equal(5, resources.Mp); Assert.True(resources.Exp > 0);
        var token = completedHeal.BattleScene!.Token;
        var completion = new CompletePresentation(token, completedHeal.BattleScene.CompletionKind);
        var result = Send(session, completion);
        observations.AddRange(result.Observations);
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        Assert.Equal(SessionStopReason.Faulted, result.Snapshot.StopReason);
        Assert.Null(result.Snapshot.BattleScene); Assert.Null(result.Snapshot.BattleMovement);
        Assert.Equal(completedHeal.Battle.Cursor + 1, result.Snapshot.Battle.Cursor);
        Assert.Equal(new("raider"), result.Snapshot.Battle.Queue[result.Snapshot.Battle.Cursor].Actor);
        Assert.Equal(completedHeal.Battle.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(completedHeal.Battle.Gold, result.Snapshot.Battle.Gold);
        Assert.Equal((resources.Hp, resources.Mp, resources.Exp, resources.Kills, resources.Defeats),
            (result.Snapshot.Battle.GetActor(actor).Hp, result.Snapshot.Battle.GetActor(actor).Mp,
                result.Snapshot.Battle.GetActor(actor).Exp, result.Snapshot.Battle.GetActor(actor).Kills,
                result.Snapshot.Battle.GetActor(actor).Defeats));
        Assert.Single(observations, row => row.Kind == "action-committed" && row.Actor == actor);
        Assert.Single(observations, row => row.Kind == "scene-ended" && row.Actor == actor);
        Assert.Single(observations, row => row.Kind == "hp" && row.Actor == actor);
        Assert.Single(observations, row => row.Kind == "mp" && row.Actor == actor);
        Assert.Single(observations, row => row.Kind == "exp" && row.Actor == actor);
        Assert.Equal(observations.Count, observations.Select(row => row.Sequence).Distinct().Count());
        var mainDraws = observations.Where(row => row.Kind.StartsWith("rng-", StringComparison.Ordinal)).ToArray();
        Assert.True(mainDraws.Length > 2); // Construction and later HEAL scene work are both retained.
        uint seed = startingSeed;
        foreach (var draw in mainDraws)
        {
            Assert.Equal(seed, draw.Before);
            var expected = BattleRandom.NextMain(seed, draw.RandomRange!.Value);
            Assert.Equal(expected.After, draw.After); Assert.Equal(expected.Value, draw.RandomValue);
            seed = expected.After;
        }
        Assert.Equal(seed, result.Snapshot.Battle.MainSeed);
        if (fault == "post-decision")
        {
            Assert.Equal(2, rule.Preparations.Count);
            Assert.NotEqual(completedHeal.Battle.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
            Assert.NotNull(result.Snapshot.Battle.GetActor(new("raider")).LastTarget);
            Assert.Contains(result.Observations, row => row.Kind == "ai-target");
        }
        else
        {
            Assert.Equal(completedHeal.Battle.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
            Assert.Null(result.Snapshot.Battle.GetActor(new("raider")).LastTarget);
        }
        var stopped = session.Current; int preparationCount = rule.Preparations.Count;
        Assert.Equal("not-player-control", Send(session, completion).Failure!.Code);
        Assert.Equal("not-waiting", Send(session, new AdvanceSimulation()).Failure!.Code);
        Assert.Same(stopped, session.Current); Assert.Equal(preparationCount, rule.Preparations.Count);
    }

    [Fact]
    public void ProgramRunnerBattleEntryRetainsTheSelectedPhysicalRule()
    {
        var rule = new SmallPhysicalRule(); var source = Open(Automatic(), Rules(rule));
        var battle = source.Current.Battle;
        var raider = battle.GetActor(new("raider"));
        battle = battle.With(queue: [new(raider.Actor, 20), new(source.Current.Selection!.Actor, 10), new(null, 255)], cursor: 0);
        var story = new StoryState(continuation: ProgramContinuation.BattleStartFinished);
        var before = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null), story, SessionStopReason.SimulationWait);
        var entered = ProgramRunner.Run(source.Definition, before, [], source.Rules);
        Assert.Null(entered.Failure); Assert.NotEmpty(rule.Estimates); Assert.Single(rule.Preparations);
        Assert.NotNull(entered.Snapshot.BattleMovement);
        var current = entered.Snapshot;
        while (current.BattleMovement is { } movement)
        {
            var result = BattleMovementContinuation.Submit(current, new CompletePresentation(movement.Token, movement.CompletionKind), source.Rules);
            Assert.Null(result.Failure); current = result.Snapshot;
        }
        Assert.Equal(2, rule.Preparations.Count); Assert.NotNull(current.BattleScene);
    }

    [Theory]
    [InlineData(BattleActionKind.Physical, "foreign")]
    [InlineData(BattleActionKind.Physical, "hp")]
    [InlineData(BattleActionKind.Physical, "queue")]
    [InlineData(BattleActionKind.Physical, "gold")]
    [InlineData(BattleActionKind.Physical, "reaction")]
    [InlineData(BattleActionKind.Physical, "reward")]
    [InlineData(BattleActionKind.Physical, "effect")]
    [InlineData(BattleActionKind.Physical, "rng")]
    [InlineData(BattleActionKind.Item, "foreign")]
    [InlineData(BattleActionKind.Item, "inventory")]
    [InlineData(BattleActionKind.Item, "hp")]
    [InlineData(BattleActionKind.Item, "queue")]
    [InlineData(BattleActionKind.Item, "gold")]
    [InlineData(BattleActionKind.Item, "reaction")]
    [InlineData(BattleActionKind.Item, "effect")]
    [InlineData(BattleActionKind.Stay, "queue")]
    [InlineData(BattleActionKind.Stay, "effect")]
    public void MalformedActionCannotPublishAnyConstructionStage(BattleActionKind kind, string fault)
    {
        var broken = new BrokenAction(kind, fault);
        var rules = Rules(kind == BattleActionKind.Physical ? broken : null,
            kind == BattleActionKind.Item ? broken : null, kind == BattleActionKind.Stay ? broken : null);
        var session = Open(kind == BattleActionKind.Item ? Items() : Document("stone-court"), rules);
        Accept(session, new Confirm());
        Accept(session, new SelectBattleAction(new(kind, ItemSlot: kind == BattleActionKind.Item ? 0 : null)));
        if (kind != BattleActionKind.Stay) Accept(session, new SelectTarget(kind == BattleActionKind.Item ? new("medic-a") : new("raider")));
        var before = session.Current; var result = Send(session, new Confirm());
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        Assert.DoesNotContain("private detail", result.Failure.Message);
        Assert.Same(before, result.Snapshot); Assert.Same(before, session.Current); Assert.Empty(result.Observations);
    }

    [Fact]
    public void LaterAutomaticPreparationFailureRetainsDecisionAndFinishedMovementWithoutDamageOrQueueConsumption()
    {
        var rule = new SmallPhysicalRule(failAfterPreflight: true); var session = Open(Automatic(), Rules(rule));
        var begun = Stay(session); var decision = session.Current.Battle;
        Assert.NotNull(session.Current.BattleMovement); Assert.Single(rule.Preparations);
        SessionResult result = begun;
        while (session.Current.BattleMovement is { } movement && result.Failure is null)
            result = Send(session, new CompletePresentation(movement.Token, movement.CompletionKind));
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        Assert.Equal(2, rule.Preparations.Count);
        Assert.Equal(SessionStopReason.Faulted, session.Current.StopReason);
        Assert.Equal(decision.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(decision.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(decision.Cursor, result.Snapshot.Battle.Cursor);
        Assert.Equal(decision.Actors.Select(actor => (actor.Hp, actor.Position, actor.LastTarget)),
            result.Snapshot.Battle.Actors.Select(actor => (actor.Hp, actor.Position, actor.LastTarget)));
        Assert.Contains(result.Observations, row => row.Kind == "battle-movement-finished");
        Assert.DoesNotContain(result.Observations, row => row.Kind is "scene-prepared" or "action-committed");
        var stopped = session.Current;
        Assert.Equal("not-waiting", Send(session, new AdvanceSimulation()).Failure!.Code);
        Assert.Same(stopped, session.Current); Assert.Equal(2, rule.Preparations.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void SelectedPhysicalRuleOwnsItsConstructionDrawRangeAndCount(int range)
    {
        var rule = new SmallPhysicalRule(constructionRange: (ushort)range);
        var session = Open(Document("stone-court"), Rules(rule));
        Accept(session, new Confirm()); Accept(session, new SelectBattleAction(new(BattleActionKind.Physical)));
        Accept(session, new SelectTarget(new("raider")));
        var before = session.Current.Battle;
        var begun = Accept(session, new Confirm());
        var draw = Assert.Single(begun.Observations, row => row.Kind == "rng-authored-physical");
        Assert.Equal((ushort)range, draw.RandomRange);
        Assert.Equal(BattleRandom.NextMain(before.MainSeed, (ushort)range).After, begun.Snapshot.Battle.MainSeed);
        Assert.Equal(before.GetActor(new("raider")).Hp, begun.Snapshot.Battle.GetActor(new("raider")).Hp);
        var completed = FinishBattleScenes(session, begun);
        Assert.Null(completed.Failure);
        Assert.Equal(before.GetActor(new("raider")).Hp - 2, completed.Snapshot.Battle.GetActor(new("raider")).Hp);
        Assert.Single(completed.Observations, row => row.Kind == "action-committed");
        Assert.Single(completed.Observations, row => row.Kind == "rng-authored-physical");
        Assert.Equal(before.ThinkingSeed, completed.Snapshot.Battle.ThinkingSeed);
    }

    private class SmallPhysicalRule(string? block = null, int estimate = 1000, bool failAfterPreflight = false, ushort? constructionRange = null, bool failPreflight = false) : IPhysicalActionRule
    {
        private readonly Sf2PhysicalAction _source = new();
        public List<(ActorRef Actor, ActorRef Target)> Estimates { get; } = [];
        public List<(ActorRef Actor, MapPosition Destination, ActorRef Target, uint Main, uint Thinking)> Preparations { get; } = [];
        public string Identity => "small-physical-test";
        public BattleActionKind Kind => BattleActionKind.Physical;
        public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) => _source.QueryActions(battle, actor);
        public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action) => _source.RequireAction(battle, actor, action);
        public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) => _source.QueryTargets(battle, actor, action);
        public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination, BattleActionRef action, ActorRef target)
        {
            if (target.Value == block) throw new BattleRuleException("selected-target-disabled", "target");
            return _source.RequireTarget(battle, actor, destination, action, target);
        }
        public int EstimateDamage(EngineBattleState battle, ActorRef actor, ActorRef target)
        { Estimates.Add((actor, target)); return estimate; }
        public virtual BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
            BattleActionRef action, ActorRef? targetRef)
        {
            var target = targetRef!.Value;
            _ = RequireTarget(battle, actor, destination, action, target);
            Preparations.Add((actor, destination, target, battle.MainSeed, battle.ThinkingSeed));
            if (failPreflight || failAfterPreflight && Preparations.Count > 1) throw new InvalidOperationException("private detail");
            var defender = battle.GetActor(target);
            uint seed = battle.MainSeed;
            List<BattleEffect> facts = [];
            if (constructionRange is { } range)
            {
                var draw = BattleRandom.NextMain(seed, range); seed = draw.After;
                facts.Add(new("rng-authored-physical", actor, draw.Before, draw.After, range, draw.Value));
            }
            return new(battle.With(mainSeed: seed, actors: battle.Actors.Select(row => row.Actor == actor ? row.With(position: destination) : row)),
                actor, destination, [new(actor, target, "physical-first", BattleReactionKind.Damage, defender.Hp,
                    (ushort)Math.Max(0, defender.Hp - 2), defender.Mp, defender.Mp, Amount: 2)], null,
                [.. facts, new("physical-first", actor, Target: target), new("hp", target, defender.Hp, Math.Max(0, defender.Hp - 2))], []);
        }
    }

    private sealed class BrokenAction(BattleActionKind kind, string fault) : IPhysicalActionRule
    {
        private readonly IBattleActionRule _source = kind switch
        { BattleActionKind.Physical => new Sf2PhysicalAction(), BattleActionKind.Item => new Sf2ItemAction(), _ => new Sf2StayAction() };
        public string Identity => "broken-action-test";
        public BattleActionKind Kind => kind;
        public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) => _source.QueryActions(battle, actor);
        public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action) => _source.RequireAction(battle, actor, action);
        public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) => _source.QueryTargets(battle, actor, action);
        public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination, BattleActionRef action, ActorRef target) => _source.RequireTarget(battle, actor, destination, action, target);
        public int EstimateDamage(EngineBattleState battle, ActorRef actor, ActorRef target) => ((IPhysicalActionRule)_source).EstimateDamage(battle, actor, target);
        public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination, BattleActionRef reference, ActorRef? target)
        {
            var action = _source.Prepare(battle, actor, destination, reference, target);
            return fault switch
            {
                "foreign" => action with { Prepared = action.Prepared.With(actors: action.Prepared.Actors.Select(row => row.Actor == actor ? row : row.With(hp: 1))) },
                "hp" => action with { Prepared = action.Prepared.With(actors: action.Prepared.Actors.Select(row => row.Actor == actor ? row.With(hp: 1) : row)) },
                "queue" => action with { Prepared = action.Prepared.With(cursor: battle.Cursor + 1) },
                "gold" => action with { Prepared = action.Prepared.With(gold: 123) },
                "inventory" => action with { Prepared = action.Prepared.With(actors: action.Prepared.Actors.Select(row => row.Actor == actor ? row.With(sourceLoadout: battle.GetActor(actor).SourceLoadout) : row)) },
                "reaction" => action with { Reactions = [action.Reactions[0] with { Target = new("absent") }] },
                "reward" => action with { Reward = new(new("lookout"), 1) },
                "rng" => action with { ConstructionEffects = action.ConstructionEffects.Select(effect => effect.RandomRange is not null ? effect with { After = effect.After + 1 } : effect).ToArray() },
                _ => action with { ConstructionEffects = [.. action.ConstructionEffects, new("foreign-effect", actor)] },
            };
        }
    }
}

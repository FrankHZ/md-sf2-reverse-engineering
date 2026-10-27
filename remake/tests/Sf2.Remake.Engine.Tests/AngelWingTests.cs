using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class AngelWingTests
{
    private static readonly ActorRef User = new("medic-a");

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void SelectedWingPaysOnceBeforeFeedbackAndSurvivesHealingAndWalkingRetry(int slot)
    {
        var session = StartWing();
        var before = session.Current;
        var choice = session.QueryBattleChoices().Items[slot];
        Assert.Equal("Angel Wing", choice.Label); Assert.True(choice.Enabled);
        Assert.Equal(User, Assert.Single(choice.Targets).Actor);
        Assert.Same(before, session.Current);
        Select(session, slot);
        Assert.Equal("invalid-retreat-target", Send(session, new SelectTarget(new("guard-a"))).Failure?.Code);
        FinishMovement(session, Accept(session, new Cancel()));
        Assert.Equal(new ushort[] { 4, 256, 260, 127 }, session.Current.Battle.GetActor(User).SourceLoadout!.Items);
        Select(session, slot);
        var envelope = new CommandEnvelope(session.Current.SessionId, session.Current.Revision, User, new Confirm());
        var paid = session.Submit(envelope);
        Assert.Null(paid.Failure);
        var expected = new List<ushort> { 4, 256, 260, 127 }; expected.RemoveAt(slot); expected.Add(127);
        Assert.Equal(expected, session.Current.Battle.GetActor(User).SourceLoadout!.Items);
        Assert.Equal(0, session.Current.Battle.GetActor(User).Mp);
        Assert.Empty(session.Current.Battle.GetActor(User).Spells);
        Assert.Equal(new ItemRetreatPayment(4, "Angel Wing", slot, slot == 0 ? (ushort)4 : (ushort)260), session.Current.BattleRetreat!.Payment);
        Assert.Equal("medic-a used Angel Wing!", session.Current.BattleRetreat.Text);
        Assert.Null(session.Current.BattleRetreat.PaidMp); Assert.Null(session.Current.BattleScene);
        Assert.Equal(new[] { "retreat-item-paid", "retreat-text-requested" }, paid.Observations.Select(e => e.Kind));
        Assert.Equal(before.Battle.MainSeed, session.Current.Battle.MainSeed);
        Assert.Equal(before.Battle.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.Equal(before.Battle.Queue, session.Current.Battle.Queue);
        Assert.Equal(before.Battle.Cursor, session.Current.Battle.Cursor);
        Assert.Equal(before.Battle.Gold, session.Current.Battle.Gold);
        Assert.Equal("stale-input", session.Submit(envelope).Failure?.Code);
        var text = session.Current.BattleRetreat;
        RejectUnchanged(session, new Acknowledge(text.Token));
        RejectUnchanged(session, new AdvanceSimulation());
        RejectUnchanged(session, new CompleteTextReveal(new(text.Token.Value + 1)));
        Accept(session, new CompleteTextReveal(text.Token));
        RejectUnchanged(session, new CompleteTextReveal(text.Token));
        Accept(session, new Acknowledge(text.Token));
        RejectUnchanged(session, new Acknowledge(text.Token));
        var effect = session.Current.BattleRetreat!;
        RejectUnchanged(session, new CompletePresentation(effect.Token, PresentationCueKind.FadeOut));
        Accept(session, new CompletePresentation(effect.Token, effect.CompletionKind));
        RejectUnchanged(session, new CompletePresentation(effect.Token, effect.CompletionKind));
        var close = session.Current.BattleRetreat!;
        var returned = Accept(session, new CompletePresentation(close.Token, close.CompletionKind));
        Assert.Equal(SessionMode.Exploration, returned.Snapshot.Mode);
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Equal(20, session.Current.Exploration!.Party.Actors.Single(a => a.Actor == User).Mp);
        Assert.Equal(expected, session.Current.Exploration.Party.Actors.Single(a => a.Actor == User).SourceLoadout!.Items);
        Assert.DoesNotContain(returned.Observations, e => e.Kind is "after-turn" or "defeat-recovered" or "battle-completed-set" or "after-battle-join");
        Assert.DoesNotContain(502, session.Current.Story.Flags); Assert.Contains(402, session.Current.Story.Flags);
        Accept(session, new Move(ExplorationDirection.East));
        for (int i = 0; i < 100 && session.Current.Mode == SessionMode.Exploration; i++)
        {
            var wait = session.Current.Story.Wait;
            Accept(session, wait is PresentationWait view ? new CompletePresentation(view.Token, view.Cue.Kind) : new AdvanceSimulation(wait?.Token));
        }
        Assert.Equal(before.SessionId, session.Current.SessionId);
        Assert.Equal(SessionMode.Battle, session.Current.Mode); Assert.NotNull(session.Current.Selection);
        Assert.Equal(1, session.Current.Battle.Round); Assert.Equal(15, session.Current.Battle.GetActor(new("dummy-a")).Hp);
        Assert.Equal(expected, session.Current.Battle.GetActor(User).SourceLoadout!.Items);
        foreach (var actor in before.Battle.Actors.Where(a => a.IsAlly))
        {
            var actual = session.Current.Battle.GetActor(actor.Actor);
            Assert.Equal(actor.Exp, actual.Exp); Assert.Equal(actor.Kills, actual.Kills); Assert.Equal(actor.Defeats, actual.Defeats);
            Assert.Equal(actor.SourceLoadout!.Spells, actual.SourceLoadout!.Spells);
        }
    }

    [Fact]
    public void ConfirmationRevalidatesSelectedPackedWordControlAndEnvelope()
    {
        var session = StartWing(); Select(session, 2);
        var current = session.Current;
        foreach (var replacement in new[] { new ushort[] { 4, 256, 127, 127 }, new ushort[] { 4, 256, 4, 127 }, new ushort[] { 4, 256, 0, 127 } })
        {
            var battle = current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == User
                ? a.With(sourceLoadout: new(replacement, a.SourceLoadout!.Spells)) : a));
            var changed = ReplaceBattle(current, battle);
            var result = BattleCommandDispatcher.Submit(changed, new Confirm(), session.Rules);
            Assert.Equal("stale-item-selection", result.Failure?.Code); Assert.Same(changed, result.Snapshot); Assert.Empty(result.Observations);
        }
        foreach (ushort status in new ushort[] { 0x40, 0x80 })
        {
            var changed = ReplaceBattle(current, current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == User ? a.With(status: status) : a)));
            Assert.Equal("not-player-control", BattleCommandDispatcher.Submit(changed, new Confirm(), session.Rules).Failure?.Code);
        }
        var invalidTarget = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveBattle(current.Battle, new(User, current.Selection!.Preview, BattleSelectionStage.CommitReady,
                SessionAction.Item, target: new("guard-a"), itemSlot: 2, itemWord: 260)), current.Story, current.StopReason);
        Assert.Equal("invalid-retreat-target", BattleCommandDispatcher.Submit(invalidTarget, new Confirm(), session.Rules).Failure?.Code);
        Assert.Equal("stale-input", session.Submit(new(current.SessionId, current.Revision - 1, User, new Confirm())).Failure?.Code);
        Assert.Equal("wrong-actor", session.Submit(new(current.SessionId, current.Revision, new("guard-a"), new Confirm())).Failure?.Code);
        Assert.Same(current, session.Current);
    }

    [Fact]
    public void EmptyMissingUnsupportedAndNonactionableSelectionsNeverPay()
    {
        var session = StartWing(); Accept(session, new Confirm()); var current = session.Current;
        Assert.Equal("empty-item-slot", Send(session, new SelectItem(3)).Failure?.Code);
        Assert.Equal("item-slot", Send(session, new SelectItem(4)).Failure?.Code);
        Assert.Equal("not-player-control", Assert.Throws<BattleRuleException>(() =>
            BattleChoices.RequireAction(session.Rules, current.Battle, new("dummy-a"), new(BattleActionKind.Item, ItemSlot: 0))).Code);
        var changed = ReplaceBattle(current, current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == User
            ? a.With(sourceLoadout: new([5, 127, 127, 127], a.SourceLoadout!.Spells)) : a)));
        Assert.Equal("item-effect", BattleCommandDispatcher.Submit(changed, new SelectItem(0), session.Rules).Failure?.Code);
        foreach (var actor in new[] { current.Battle.GetActor(User).With(hp: 0), current.Battle.GetActor(User).With(clearPosition: true) })
        {
            changed = ReplaceBattle(current, current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == User ? actor : a)));
            Assert.Equal("not-player-control", BattleCommandDispatcher.Submit(changed, new SelectItem(0), session.Rules).Failure?.Code);
        }
        Assert.Same(current, session.Current); Assert.Equal(0, current.Battle.GetActor(User).Mp);
    }

    [Fact]
    public void LastSlotRemovalKeepsAllEarlierPackedWordsAndSpells()
    {
        var session = StartWing(); var before = session.Current.Battle;
        var loadout = before.GetActor(User).SourceLoadout!;
        var battle = before.With(actors: before.Actors.Select(a => a.Actor == User
            ? a.With(sourceLoadout: new([256, 4, 0, 260], loadout.Spells)) : a));
        var action = BattleChoices.Prepare(session.Rules, battle, User, battle.GetActor(User).Position!,
            new(BattleActionKind.Item, ItemSlot: 3), User);
        Assert.Equal(new ushort[] { 256, 4, 0, 127 }, action.Prepared.GetActor(User).SourceLoadout!.Items);
        Assert.Equal(loadout.Spells, action.Prepared.GetActor(User).SourceLoadout!.Spells);
        Assert.Equal(3, Assert.IsType<ItemRetreatPayment>(action.Retreat).Slot);
    }

    [Fact]
    public void FinitePreparedItemDeltaCannotChangeResourcesSurvivorsOrAddAReaction()
    {
        var session = StartWing(); var battle = session.Current.Battle; var destination = battle.GetActor(User).Position!;
        var action = new BattleActionRef(BattleActionKind.Item, ItemSlot: 2);
        var admission = BattleChoices.RequireAction(session.Rules, battle, User, action);
        var prepared = BattleChoices.Prepare(session.Rules, battle, User, destination, action, User);
        var invalidStates = new[] {
            prepared.Prepared.With(mainSeed: battle.MainSeed + 1),
            prepared.Prepared.With(actors: prepared.Prepared.Actors.Select(a => a.Actor == User ? a.With(mp: 1) : a)),
            prepared.Prepared.With(actors: prepared.Prepared.Actors.Select(a => a.Actor == User
                ? a.With(sourceLoadout: new([260, 256, 127, 127], a.SourceLoadout!.Spells)) : a)) };
        foreach (var invalid in invalidStates)
            Assert.Throws<InvalidOperationException>(() => (prepared with { Prepared = invalid }).Validate(battle, User, destination, action, User, admission));
        Assert.Throws<InvalidOperationException>(() => (prepared with { Reward = new(User, 1) }).Validate(battle, User, destination, action, User, admission));
        Assert.Throws<InvalidOperationException>(() => (prepared with { Retreat = new SpellRetreatPayment(new("egress", 1), 0) }).Validate(battle, User, destination, action, User, admission));
        Assert.Throws<InvalidOperationException>(() => (prepared with { Reactions = [new(User, User, "item-use", BattleReactionKind.Recovery, 5, 6, 0, 0)] }).Validate(battle, User, destination, action, User, admission));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedReturnRetainsPaidInventoryAndCompletionPrefix(bool completed)
    {
        var session = StartWing("missing"); Select(session, 2);
        var current = session.Current;
        if (completed) current = current.WithStory(current.Story.Copy(null, flags: current.Story.Flags.Append(502)));
        var result = BattleCommandDispatcher.Submit(current, new Confirm(), session.Rules);
        result = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new CompleteTextReveal(result.Snapshot.BattleRetreat!.Token), session.Rules);
        result = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new Acknowledge(result.Snapshot.BattleRetreat!.Token), session.Rules);
        for (int i = 0; i < 2; i++)
        {
            var wait = result.Snapshot.BattleRetreat!;
            result = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new CompletePresentation(wait.Token, wait.CompletionKind), session.Rules);
        }
        Assert.Equal("egress-map", result.Failure?.Code); Assert.Equal(BattleRetreatPhase.Return, result.Snapshot.BattleRetreat!.Phase);
        Assert.Equal(new ushort[] { 4, 256, 127, 127 }, result.Snapshot.Battle.GetActor(User).SourceLoadout!.Items);
        Assert.Equal(0, result.Snapshot.Battle.GetActor(User).Mp); Assert.Equal(101u, result.Snapshot.Battle.Gold);
        Assert.Equal(completed, result.Snapshot.Story.Flags.Contains(502)); Assert.Equal(!completed, result.Snapshot.Story.Flags.Contains(402));
        var rejected = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new CompleteTextReveal(result.Snapshot.BattleRetreat.Token), session.Rules);
        Assert.Same(result.Snapshot, rejected.Snapshot); Assert.NotNull(rejected.Failure);
    }

    [Fact]
    public void HerbStillUsesRecoverySceneAndNotRetreat()
    {
        var session = StartWing(); Select(session, 1);
        var paid = Accept(session, new Confirm());
        Assert.Null(session.Current.BattleRetreat); Assert.NotNull(session.Current.BattleScene);
        Assert.Equal(new ushort[] { 4, 260, 127, 127 }, session.Current.Battle.GetActor(User).SourceLoadout!.Items);
        Assert.Contains(paid.Observations, e => e.Kind == "item-consumed");
    }

    private static SessionSnapshot ReplaceBattle(SessionSnapshot current, EngineBattleState battle) =>
        new(current.SessionId, current.Revision, current.ObservationSequence, new ActiveBattle(battle, current.Selection), current.Story, current.StopReason);
    private static void RejectUnchanged(GameSession session, SessionCommand command)
    { var before = session.Current; Assert.NotNull(Send(session, command).Failure); Assert.Same(before, session.Current); }
    private static void Select(GameSession session, int slot)
    { Accept(session, new Confirm()); Accept(session, new SelectItem(slot)); Accept(session, new SelectTarget(User)); }

    private static GameSession StartWing(string destination = "camp")
    {
        var document = Document();
        document["items"] = JsonNode.Parse("""[{"id":0,"name":"Medical Herb","effect":"consumable-healing","power":10,"minimumRange":0,"maximumRange":1},{"id":4,"name":"Angel Wing","effect":"consumable-retreat","power":0,"minimumRange":0,"maximumRange":0}]""");
        document["actors"]![0]!["items"] = JsonNode.Parse("[4,256,260,127]");
        document["actors"]![0]!["spells"] = new JsonArray();
        document["start"]!["actors"]![0]!["mp"] = 0;
        document["start"]!["gold"] = 101;
        var read = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        var d = read.Definition.Encounters[read.Start.Encounter];
        var encounter = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments,
            d.Spells.Values, d.Rewards, outcome: new(User, new("dummy-a")), healingItems: d.HealingItems.Values, retreatItems: d.RetreatItems.Values);
        var outcome = new ExplorationOutcomeRoute(new("empty", 0), 0, new("empty", 0), new("empty", 0),
            new("return", 0), new("arena"), 3, new("camp"), new(1, 1), 0);
        var route = new ExplorationBattleRoute(d.Encounter, 402, 502, null, null, null, Outcome: outcome);
        var layout = new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]);
        var traversal = new OriginalMapTraversal([new(0, 0, 12, 12)]);
        var world = new ExplorationDefinition([
            new(new("arena"), layout, traversal, [], [], new("initial", 0), route),
            new(new("camp"), layout, traversal, [], [new(ExplorationEventKind.Warp, 2, 1, null, null, new("arena"), new(1, 1))]) ],
            [new("empty", [new EndProgram()]), new("initial", [new SetEgressMap(new(destination)), new EndProgram()]),
                new("return", [new ReturnBattleMap(), new EndProgram()])]);
        var definition = new ScenarioDefinition("wing-tests", [encounter], exploration: world, encounterStarts: [read.Start]);
        return Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(new("arena"), new("entity-0"), new(1, 1), 0, 96, [399, 402], read.Start))).Session;
    }
}

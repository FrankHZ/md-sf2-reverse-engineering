using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleRetreatTests
{
    private static readonly ActorRef Caster = new("medic-a");
    private static readonly SpellRef Egress = new("egress", 1);

    [Theory]
    [InlineData(7, false)]
    [InlineData(8, true)]
    public void QueryAndCancellationAreProvisionalAndCostEightIsRevalidated(byte mp, bool enabled)
    {
        var session = StartRetreat(mp);
        var before = session.Current.Battle;
        var choice = Assert.Single(session.QueryBattleChoices().Spells);
        Assert.Equal(enabled, choice.Enabled);
        Assert.Same(before, session.Current.Battle);
        Assert.Equal(Egress, choice.Spell);
        Accept(session, new Confirm());
        var selection = Send(session, new SelectSpell(Egress));
        if (!enabled)
        {
            Assert.Equal("insufficient-mp", selection.Failure?.Code);
            Assert.Equal(mp, session.Current.Battle.GetActor(Caster).Mp);
            return;
        }
        Assert.Null(selection.Failure);
        Assert.Equal("invalid-retreat-target", Send(session, new SelectTarget(new("guard-a"))).Failure?.Code);
        Accept(session, new SelectTarget(Caster));
        FinishMovement(session, Accept(session, new Cancel()));
        Assert.Equal(mp, session.Current.Battle.GetActor(Caster).Mp);
        Assert.Equal(before.MainSeed, session.Current.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Select(session);
        var current = session.Current;
        var lower = current.Battle.With(actors: current.Battle.Actors.Select(a => a.Actor == Caster ? a.With(mp: 7) : a));
        var changed = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveBattle(lower, current.Selection), current.Story, current.StopReason);
        Assert.Equal("insufficient-mp", BattleCommandDispatcher.Submit(changed, new Confirm(), session.Rules).Failure?.Code);
        Assert.Equal("stale-input", session.Submit(new(current.SessionId, current.Revision - 1, Caster, new Confirm())).Failure?.Code);
        Assert.Equal("wrong-actor", session.Submit(new(current.SessionId, current.Revision, new("guard-a"), new Confirm())).Failure?.Code);
        Assert.Equal(mp, session.Current.Battle.GetActor(Caster).Mp);
    }

    [Theory]
    [InlineData("camp-a", false)]
    [InlineData("camp-b", true)]
    public void PaidRetreatWaitsThenReturnsAndWalksIntoFreshEncounter(string destination, bool deadAlly)
    {
        var session = StartRetreat(8, destination, deadAlly);
        var id = session.Current.SessionId;
        var before = session.Current.Battle;
        Select(session);
        var envelope = new CommandEnvelope(id, session.Current.Revision, Caster, new Confirm());
        var paid = session.Submit(envelope);
        Assert.Null(paid.Failure);
        Assert.Equal(0, session.Current.Battle.GetActor(Caster).Mp);
        Assert.Single(paid.Observations, e => e.Kind == "retreat-mp-paid" && e.Before == 8 && e.After == 0);
        Assert.Null(session.Current.BattleScene);
        Assert.False(session.Current.HasBattleControl);
        Assert.Equal(before.Queue, session.Current.Battle.Queue);
        Assert.Equal(before.Cursor, session.Current.Battle.Cursor);
        Assert.Equal(before.MainSeed, session.Current.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.Equal("stale-input", session.Submit(envelope).Failure?.Code);
        var text = session.Current.BattleRetreat!;
        RejectUnchanged(session, new Acknowledge(text.Token));
        RejectUnchanged(session, new AdvanceSimulation());
        RejectUnchanged(session, new CompleteTextReveal(new(text.Token.Value + 10)));
        Accept(session, new CompleteTextReveal(text.Token));
        RejectUnchanged(session, new CompleteTextReveal(text.Token));
        Accept(session, new Acknowledge(text.Token));
        RejectUnchanged(session, new Acknowledge(text.Token));
        var effect = session.Current.BattleRetreat!;
        RejectUnchanged(session, new CompletePresentation(effect.Token, PresentationCueKind.FadeOut));
        Accept(session, new CompletePresentation(effect.Token, effect.CompletionKind));
        RejectUnchanged(session, new CompletePresentation(effect.Token, effect.CompletionKind));
        var close = session.Current.BattleRetreat!;
        Assert.Equal(0, session.Current.Battle.GetActor(Caster).Mp);
        var returned = Accept(session, new CompletePresentation(close.Token, close.CompletionKind));
        Assert.Equal(SessionMode.Exploration, returned.Snapshot.Mode);
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Equal(destination, session.Current.Exploration!.Definition.Map.Value);
        Assert.DoesNotContain(502, session.Current.Story.Flags);
        Assert.Contains(402, session.Current.Story.Flags);
        Assert.DoesNotContain(returned.Observations, e => e.Kind is "after-turn" or "defeat-recovered" or "battle-completed-set" or "after-battle-join");
        var recovered = session.Current.Exploration.Party;
        Assert.Equal(101u, recovered.Gold);
        Assert.Equal(20, recovered.Actors.Single(a => a.Actor == Caster).Mp);
        Assert.Equal(100, recovered.Actors.Single(a => a.Actor == Caster).Hp);
        Assert.Equal(deadAlly ? 0 : 12, recovered.Actors.Single(a => a.Actor.Value == "guard-a").Hp);
        foreach (var a in recovered.Actors)
        {
            var old = before.GetActor(a.Actor);
            Assert.Equal(old.Exp, a.Exp); Assert.Equal(old.Kills, a.Kills); Assert.Equal(old.Defeats, a.Defeats);
            Assert.Equal(old.Progress, a.Progress); Assert.Equal(old.SourceLoadout, a.SourceLoadout);
        }
        // One real step reaches an authored warp; normal map admission starts the next battle.
        Accept(session, new Move(ExplorationDirection.East));
        for (int i = 0; i < 100 && session.Current.Mode == SessionMode.Exploration; i++)
        {
            var wait = session.Current.Story.Wait;
            Accept(session, wait is PresentationWait view ? new CompletePresentation(view.Token, view.Cue.Kind) : new AdvanceSimulation(wait?.Token));
        }
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal(id, session.Current.SessionId);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.Equal(15, session.Current.Battle.GetActor(new("dummy-a")).Hp);
        Assert.Equal(new MapPosition(8, 5), session.Current.Battle.GetActor(new("dummy-a")).Position);
        Assert.Equal(20, session.Current.Battle.GetActor(Caster).Mp);
        Assert.Equal(deadAlly ? null : before.GetActor(new("guard-a")).Deployment.Position, session.Current.Battle.GetActor(new("guard-a")).Position);
        Assert.NotNull(session.Current.Selection);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReturnFailureRetainsPaymentClosureAndRepeatFlagPrefix(bool completed)
    {
        var session = StartRetreat(8, "missing-savepoint");
        Select(session);
        var current = session.Current;
        if (completed) current = current.WithStory(current.Story.Copy(null, flags: current.Story.Flags.Append(502)));
        var result = BattleCommandDispatcher.Submit(current, new Confirm(), session.Rules);
        result = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new CompleteTextReveal(result.Snapshot.BattleRetreat!.Token), session.Rules);
        result = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new Acknowledge(result.Snapshot.BattleRetreat!.Token), session.Rules);
        foreach (var phase in new[] { BattleRetreatPhase.Effect, BattleRetreatPhase.Close })
        {
            var wait = result.Snapshot.BattleRetreat!; Assert.Equal(phase, wait.Phase);
            result = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new CompletePresentation(wait.Token, wait.CompletionKind), session.Rules);
        }
        Assert.Equal("egress-map", result.Failure?.Code);
        Assert.Equal(BattleRetreatPhase.Return, result.Snapshot.BattleRetreat?.Phase);
        Assert.Equal(0, result.Snapshot.Battle.GetActor(Caster).Mp);
        Assert.Equal(101u, result.Snapshot.Battle.Gold);
        Assert.Equal(completed, result.Snapshot.Story.Flags.Contains(502));
        Assert.Equal(!completed, result.Snapshot.Story.Flags.Contains(402));
        Assert.Equal(completed ? 1 : 0, result.Observations.Count(e => e.Kind == "battle-unlock-cleared"));
    }

    [Fact]
    public void UnlearnedAndSleepingActorsCannotCastButSilenceAloneIsNotAnInventedGate()
    {
        var session = StartRetreat(8);
        var battle = session.Current.Battle; var actor = battle.GetActor(Caster);
        var rules = session.Rules;
        var sleeping = battle.With(actors: battle.Actors.Select(a => a.Actor == Caster ? a.With(status: 0x40) : a));
        Assert.Equal("not-player-control", Assert.Throws<BattleRuleException>(() =>
            BattleChoices.Prepare(rules, sleeping, Caster, actor.Position!, new(BattleActionKind.Retreat, Egress), Caster)).Code);
        Assert.Equal("spell-not-known", Assert.Throws<BattleRuleException>(() =>
            BattleChoices.RequireAction(rules, battle, new("guard-a"), new(BattleActionKind.Retreat, Egress))).Code);
        var silenced = battle.With(actors: battle.Actors.Select(a => a.Actor == Caster ? a.With(status: 0x0300) : a));
        Assert.NotNull(BattleChoices.Prepare(rules, silenced, Caster, actor.Position!, new(BattleActionKind.Retreat, Egress), Caster).Retreat);
    }

    [Fact]
    public void CompletedRetreatKeepsCompletionWithoutVictoryTailAndImmortalsRecover()
    {
        var session = StartRetreat(8);
        Select(session);
        var current = session.Current.WithStory(session.Current.Story.Copy(null, flags: session.Current.Story.Flags.Append(502)));
        var result = BattleCommandDispatcher.Submit(current, new Confirm(), session.Rules);
        result = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new CompleteTextReveal(result.Snapshot.BattleRetreat!.Token), session.Rules);
        result = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new Acknowledge(result.Snapshot.BattleRetreat!.Token), session.Rules);
        for (int i = 0; i < 2; i++)
        {
            var wait = result.Snapshot.BattleRetreat!;
            result = BattleRetreatContinuation.Submit(session.Definition, result.Snapshot, new CompletePresentation(wait.Token, wait.CompletionKind), session.Rules);
        }
        Assert.Null(result.Failure); Assert.True(result.Snapshot.CanWaitAtInput);
        Assert.Contains(502, result.Snapshot.Story.Flags); Assert.DoesNotContain(402, result.Snapshot.Story.Flags);
        Assert.DoesNotContain(result.Observations, e => e.Kind is "after-battle-join" or "battle-completed-set" or "defeat-recovered");
        Assert.Equal(101u, result.Snapshot.Exploration!.Party.Gold);

        var d = session.Current.Battle.Definition;
        foreach (int member in new[] { 1, 7, 28 })
        {
            var deployments = d.Deployments.Select(row => row.Actor.Value == "guard-a" ? row with
                { Initialization = new(null, 0, 0, 0, 0, AllyPartyMember: member) } : row);
            var changed = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, deployments, d.Spells.Values);
            var definition = new ScenarioDefinition("recovery", [changed]);
            var party = result.Snapshot.Exploration.Party;
            party = new(party.Encounter, party.Actors.Select(a => a.Actor.Value == "guard-a" ? a with { Hp = 0 } : a),
                party.MainSeed, party.ThinkingSeed, party.Gold);
            var healed = OutcomeReturnRules.Heal(session.Rules.OutcomeReturn, definition, party, all: false);
            Assert.Equal(member == 1 ? 0 : 12, healed.Actors.Single(a => a.Actor.Value == "guard-a").Hp);
        }
    }

    [Fact]
    public void MixedSpellEffectsKeepLearnedOrderAndPreparedRetreatCannotMutateResources()
    {
        var document = Document();
        document["spells"]!.AsArray().Add(JsonNode.Parse("""{"id":"egress","level":1,"mpCost":8,"minimumRange":0,"maximumRange":0,"effect":{"kind":"retreat"}}"""));
        document["actors"]![0]!["spells"]!.AsArray().Insert(0, JsonNode.Parse("""{"id":"egress","level":1}"""));
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document))).Session;
        Assert.Equal(new[] { "egress", "mend" }, session.QueryBattleChoices().Spells.Select(s => s.Spell.Value));
        var before = session.Current.Battle; var actor = before.GetActor(Caster);
        var reference = new BattleActionRef(BattleActionKind.Retreat, Egress);
        var admission = BattleChoices.RequireAction(session.Rules, before, Caster, reference);
        var prepared = BattleChoices.Prepare(session.Rules, before, Caster, actor.Position!, reference, Caster);
        var invalid = prepared with { Prepared = prepared.Prepared.With(actors: before.Actors.Select(a => a.Actor == Caster ? a.With(mp: 0) : a)) };
        Assert.Throws<InvalidOperationException>(() => invalid.Validate(before, Caster, actor.Position!, reference, Caster, admission));
        invalid = prepared with { Prepared = prepared.Prepared.With(mainSeed: before.MainSeed + 1) };
        Assert.Throws<InvalidOperationException>(() => invalid.Validate(before, Caster, actor.Position!, reference, Caster, admission));
    }

    private static void RejectUnchanged(GameSession session, SessionCommand command)
    { var before = session.Current; Assert.NotNull(Send(session, command).Failure); Assert.Same(before, session.Current); }
    private static void Select(GameSession session)
    { Accept(session, new Confirm()); Accept(session, new SelectSpell(Egress)); Accept(session, new SelectTarget(Caster)); }

    private static GameSession StartRetreat(byte mp, string liveDestination = "camp-a", bool deadAlly = false)
    {
        var document = Document();
        document["spells"] = JsonNode.Parse("""[{"id":"egress","level":1,"mpCost":8,"minimumRange":0,"maximumRange":0,"effect":{"kind":"retreat"}}]""");
        document["actors"]![0]!["spells"] = JsonNode.Parse("""[{"id":"egress","level":1}]""");
        document["start"]!["actors"]![0]!["mp"] = mp;
        document["start"]!["actors"]![1]!["hp"] = deadAlly ? 0 : 9;
        document["start"]!["gold"] = 101;
        var read = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        var d = read.Definition.Encounters[read.Start.Encounter];
        var encounter = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments,
            d.Spells.Values, d.Rewards, outcome: new(Caster, new("dummy-a")), retreatSpells: d.RetreatSpells.Values);
        ExplorationOutcomeRoute Outcome(string map) => new(new("empty", 0), 0, new("empty", 0), new("defeat", 0),
            new("return", 0), new("arena"), 3, new(map), new(1, 1), 0);
        var route = new ExplorationBattleRoute(d.Encounter, 402, 502, null, null, null, Outcome: Outcome("camp-a"));
        var layout = new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]);
        var traversal = new OriginalMapTraversal([new(0, 0, 12, 12)]);
        var maps = new[] {
            new ExplorationMapDefinition(new("arena"), layout, traversal, [], [], new("initial", 0), route),
            new ExplorationMapDefinition(new("camp-a"), layout, traversal, [],
                [new(ExplorationEventKind.Warp, 2, 1, null, null, new("arena"), new(1, 1))]),
            new ExplorationMapDefinition(new("camp-b"), layout, traversal, [],
                [new(ExplorationEventKind.Warp, 2, 1, null, null, new("arena"), new(1, 1))],
                battle: route with { UnlockedFlag = 999, Outcome = Outcome("camp-b") }) };
        var world = new ExplorationDefinition(maps, [new("empty", [new EndProgram()]),
            new("defeat", [new UnsupportedInstruction("forbidden-defeat", "test")]),
            new("initial", [new SetEgressMap(new(liveDestination)), new EndProgram()]),
            new("return", [new ReturnBattleMap(), new EndProgram()])]);
        var definition = new ScenarioDefinition("retreat-tests", [encounter], exploration: world, encounterStarts: [read.Start]);
        return Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(new("arena"), new("entity-0"), new(1, 1), 0, 96, [399, 402], read.Start))).Session;
    }
}

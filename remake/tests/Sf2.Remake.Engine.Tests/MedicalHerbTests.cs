using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class MedicalHerbTests
{
    private static JsonNode Package()
    {
        var document = Document();
        document["items"] = JsonNode.Parse("""
            [{"id":0,"name":"Recovery leaf","effect":"consumable-healing","power":10,"minimumRange":0,"maximumRange":1},
             {"id":6,"name":"Small remedy","effect":"consumable-healing","power":4,"minimumRange":0,"maximumRange":2}]
            """);
        document["actors"]![0]!["items"] = JsonNode.Parse("[0,6,0,127]");
        document["actors"]![1]!["items"] = JsonNode.Parse("[0,127,127,127]");
        return document;
    }

    private static GameSession Session(JsonNode? document = null) =>
        Assert.IsType<SessionStarted>(GameSession.Start(Reader(document ?? Package()))).Session;

    private static SessionResult Use(GameSession session, int slot, ActorRef target)
    {
        Accept(session, new Confirm()); Accept(session, new SelectItem(slot));
        Accept(session, new SelectTarget(target)); return Accept(session, new Confirm());
    }

    [Theory]
    // Minimal HP facts from accepted native candidates 42, 57, 63 and 64; no trace-derived RNG.
    [InlineData(6, 11, "unpromoted-swordsman", 11)]
    [InlineData(9, 12, "ordinary", 12)]
    [InlineData(3, 12, "unpromoted-priest", 12)]
    [InlineData(4, 11, "unpromoted-priest", 11)]
    [InlineData(60, 100, "unpromoted-priest", 70)]
    [InlineData(100, 100, "unpromoted-swordsman", 100)]
    public void RecoveryClampsToLiveMaximumWithoutSpellMpOrClassPowerScaling(int hp, int maximum, string classRule, int expected)
    {
        var document = Package();
        document["actors"]![0]!["classRule"] = classRule;
        document["actors"]![0]!["maxHp"] = maximum;
        document["start"]!["actors"]![0]!["hp"] = hp;
        document["start"]!["actors"]![0]!["mp"] = 0;
        var session = Session(document); var actor = session.Current.Selection!.Actor;
        var before = session.Current.Battle;
        var result = Use(session, 0, actor);
        var after = result.Snapshot.Battle.GetActor(actor);
        Assert.Equal(expected, after.Hp); Assert.Equal(0, after.Mp);
        Assert.Equal(new ushort[] { 6, 0, 127, 127 }, after.SourceLoadout!.Items);
        Assert.Equal(new ushort[] { 0, 6, 0, 127 }, before.GetActor(actor).SourceLoadout!.Items);
        Assert.Single(result.Observations, row => row.Kind == "item-consumed");
        Assert.Single(result.Observations, row => row.Kind == "after-turn" && row.Actor == actor);
        Assert.DoesNotContain(result.Observations, row => row.Kind == "mp");
        Assert.Equal(before.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.NotEqual(actor, result.Snapshot.Selection!.Actor);
    }

    [Fact]
    public void RejectedItemsTargetsAndCancelledReselectionAreAtomic()
    {
        var session = Session(); var actor = session.Current.Selection!.Actor;
        Accept(session, new Confirm()); var before = session.Current;
        foreach (int slot in new[] { -1, 3, 4, int.MaxValue })
        { Assert.NotNull(Send(session, new SelectItem(slot)).Failure); Assert.Same(before, session.Current); }
        Accept(session, new SelectItem(0)); Accept(session, new SelectTarget(actor));
        before = session.Current;
        foreach (string target in new[] { "missing", "dummy-a", "guard-a" })
        { Assert.NotNull(Send(session, new SelectTarget(new(target))).Failure); Assert.Same(before, session.Current); }
        Accept(session, new SelectItem(1));
        Assert.Null(session.Current.Selection!.Target); Assert.Equal(1, session.Current.Selection.ItemSlot);
        Assert.Same(before.Battle, session.Current.Battle);
        Accept(session, new SelectTarget(new("guard-a"))); // changed content admits distance 2
        Accept(session, new Cancel());
        Assert.Null(session.Current.Selection!.ItemSlot); Assert.Null(session.Current.Selection.Target);
        Assert.Same(before.Battle, session.Current.Battle);
    }

    [Fact]
    public void IndependentHolderUsesItsOwnSlotAndInventorySurvivesNextRoundAndRestart()
    {
        var session = Session(); var first = session.Current.Selection!.Actor;
        Use(session, 2, first);
        var next = session.Current.Selection!.Actor;
        Assert.Equal("guard-a", next.Value);
        Use(session, 0, next);
        Assert.All(session.Current.Battle.GetActor(next).SourceLoadout!.Items, word => Assert.Equal(127, word));
        Assert.Equal(new ushort[] { 0, 6, 127, 127 }, session.Current.Battle.GetActor(first).SourceLoadout!.Items);
        for (int i = 0; i < 8 && session.Current.Battle.Round == 1; i++) Stay(session);
        Assert.True(session.Current.Battle.Round > 1);
        var battle = session.Current.Battle;
        var input = new BattleStartInput(battle.Definition.Encounter, battle.Actors.Select(a =>
            new BattleActorStartInput(a.Actor, a.Hp, a.Mp, a.Exp, a.Kills, a.Defeats, a.Status, null, a.Progress, a.SourceLoadout)),
            battle.MainSeed, battle.ThinkingSeed, battle.Gold);
        var restarted = Assert.IsType<SessionStarted>(GameSession.Start(session.Definition, input)).Session;
        Assert.All(restarted.Current.Battle.GetActor(next).SourceLoadout!.Items, word => Assert.Equal(127, word));
        Assert.Equal(battle.GetActor(first).SourceLoadout!.Items, restarted.Current.Battle.GetActor(first).SourceLoadout!.Items);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 9)]
    public void ExactAwardUsesOnlyTwoRange16DrawsAndSkipsSameSideHalving(bool priest, int expectedExp)
    {
        var document = Package();
        document["encounters"]![0]!["rewards"] = new JsonObject { ["halvedExperience"] = true };
        var session = Session(document); var initial = session.Current.Battle; var original = initial.Actors[0];
        var definition = new BattleActorDefinition(original.Actor, priest ? BattleClassRule.UnpromotedPriest : BattleClassRule.UnpromotedSwordsman,
            1, 100, 20, 10, 8, 12, false, 5, [], sourceLoadout: new([199, 0, 256, 127], [63, 63, 63, 63]));
        var actor = new BattleActorState(original.Deployment with { Definition = definition }, 95, 8, 0, original.Position, 0, 0);
        var battle = initial.With(actors: initial.Actors.Select(a => a.Actor == actor.Actor ? actor : a), mainSeed: 0x12345678);
        var (after, effects) = PlayerItemUse.Resolve(battle, actor.Actor, actor.Position!, 1, actor.Actor);
        Assert.Equal((byte)expectedExp, after.GetActor(actor.Actor).Exp);
        Assert.Equal(0x04B65678u, after.MainSeed); // pinned generator: 0x1234 -> 0xECAB -> 0x04B6
        Assert.Equal(new ushort[] { 199, 256, 127, 127 }, after.GetActor(actor.Actor).SourceLoadout!.Items);
        Assert.Equal(8, after.GetActor(actor.Actor).Mp); Assert.Equal(battle.Gold, after.Gold);
        Assert.Equal(new ushort?[] { 14, 0 }, effects.Where(e => e.RandomRange == 16).Select(e => e.RandomValue));
        Assert.Equal(2, effects.Count(e => e.RandomRange is not null));
        var before = after;
        Assert.Equal("item-effect", Assert.Throws<BattleRuleException>(() => PlayerItemUse.Resolve(before, actor.Actor, actor.Position!, 0, actor.Actor)).Code);
        Assert.Equal(before.MainSeed, after.MainSeed);
    }

    [Fact]
    public void LevelUpAndLearnedSpellPreserveTheAlreadyConsumedSlot()
    {
        var session = Session(); var initial = session.Current.Battle; var actor = initial.Actors[0];
        var growth = new BattleGrowthDefinition(4, 0,
            Enumerable.Repeat(new StatGrowth(1, 1, []), 5).ToArray(), [new(2, 64)],
            new Dictionary<byte, SpellRef> { [0] = new("heal", 1), [64] = new("heal", 2) });
        var grown = new BattleActorState(actor.Deployment with { Definition = actor.Definition.WithGrowth(growth) },
            actor.Hp, actor.Mp, 99, actor.Position, 0, 0, sourceLoadout: new([0, 213, 0, 127], [0, 63, 63, 63]));
        var before = initial.With(actors: initial.Actors.Select(a => a.Actor == actor.Actor ? grown : a));
        var (after, _) = PlayerItemUse.Resolve(before, actor.Actor, actor.Position!, 0, actor.Actor);
        var result = after.GetActor(actor.Actor);
        Assert.Equal(2, result.Level); Assert.Contains(new SpellRef("heal", 2), result.Spells);
        Assert.Equal(new ushort[] { 213, 0, 127, 127 }, result.SourceLoadout!.Items);
        Assert.Equal(64, result.SourceLoadout.Spells[0]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OutcomeReturnCarriesConsumedInventoryThroughLivingPartyRecovery(bool defeat)
    {
        var session = Session(); var user = session.Current.Selection!.Actor;
        Use(session, 0, user);
        var battle = session.Current.Battle;
        var basis = Start("harbor-arrival").Definition.Exploration!.Maps.Values.First();
        var route = new ExplorationOutcomeRoute(new("after", 0), 0, new("after", 0), new("after", 0),
            new("after", 0), basis.Map, 0, basis.Map, new(1, 1), 0);
        var definition = new BattleDefinition(battle.Definition.Encounter, battle.Definition.Map,
            battle.Definition.Width, battle.Definition.Height, battle.Definition.Terrain, battle.Definition.Deployments,
            battle.Definition.Spells.Values, outcome: new(user, new("dummy-a")), healingItems: battle.Definition.HealingItems.Values);
        var terminal = new EngineBattleState(definition, battle.Actors.Select(a => a.With(
            hp: defeat ? a.Actor == user ? (ushort)0 : a.Hp : a.IsAlly ? a.Hp : (ushort)0, position: new(1, 1))),
            battle.MainSeed, battle.ThinkingSeed, battle.Round, [], 0, battle.Gold);
        var scenario = new ScenarioDefinition("return", [definition], exploration: new([basis],
            [new("after", [new WaitProgramTicks(1), new EndProgram()])]));
        var story = new StoryState([399], enteringBattle: new(definition.Encounter, null, null, null, null, null, Outcome: route));
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(terminal, null), story, SessionStopReason.SimulationWait);
        var result = BattleOutcome.Begin(scenario, new(snapshot, [], snapshot.StopReason));
        Assert.Null(result.Failure); Assert.Equal(SessionMode.Exploration, result.Snapshot.Mode);
        var party = result.Snapshot.Exploration!.Party;
        Assert.Equal(new ushort[] { 6, 0, 127, 127 }, party.Actors.Single(a => a.Actor == user).SourceLoadout!.Items);
        var recovered = BattleOutcome.Heal(scenario, party, all: true);
        Assert.Equal(party.Actors.Single(a => a.Actor == user).SourceLoadout!.Items,
            recovered.Actors.Single(a => a.Actor == user).SourceLoadout!.Items);
    }

    [Fact]
    public void LivePreviewAndSelectedItemContentControlRangePowerAndSingleCommit()
    {
        var document = Package(); document["start"]!["actors"]![1]!["hp"] = 1;
        var session = Session(document); var user = session.Current.Selection!.Actor;
        Accept(session, new Confirm()); Accept(session, new SelectItem(1));
        Accept(session, new SelectTarget(new("guard-a")));
        var ready = session.Current;
        var envelope = new CommandEnvelope(ready.SessionId, ready.Revision, user, new Confirm());
        var committed = session.Submit(envelope);
        Assert.Null(committed.Failure); Assert.Equal(5, session.Current.Battle.GetActor(new("guard-a")).Hp);
        var after = session.Current;
        Assert.NotNull(session.Submit(envelope).Failure); Assert.Same(after, session.Current);
        Assert.Equal(new ushort[] { 0, 0, 127, 127 }, after.Battle.GetActor(user).SourceLoadout!.Items);

        session = Session(document); user = session.Current.Selection!.Actor;
        Accept(session, new Move(ExplorationDirection.South));
        Assert.Equal(new MapPosition(3, 3), session.Current.Battle.GetActor(user).Position);
        Use(session, 0, new("guard-a"));
        Assert.Equal(new MapPosition(3, 4), session.Current.Battle.GetActor(user).Position);
        Assert.Equal(11, session.Current.Battle.GetActor(new("guard-a")).Hp);
    }

    [Fact]
    public void DeadTargetsAndEmptyInventoryCannotPublishAnItemAction()
    {
        var document = Package(); document["start"]!["actors"]![1]!["hp"] = 0;
        var session = Session(document);
        Accept(session, new Confirm()); Accept(session, new SelectItem(1));
        var before = session.Current;
        Assert.Equal("invalid-heal-target", Send(session, new SelectTarget(new("guard-a"))).Failure!.Code);
        Assert.Same(before, session.Current);
        document = Package(); document["actors"]![0]!["items"] = new JsonArray();
        session = Session(document); Accept(session, new Confirm()); before = session.Current;
        Assert.Equal("empty-item-slot", Send(session, new SelectItem(0)).Failure!.Code);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void MissingGrowthRejectsTheWholeCommitIncludingInventoryAndRng()
    {
        var document = Package(); document["start"]!["actors"]![0]!["exp"] = 99;
        var session = Session(document); var actor = session.Current.Selection!.Actor;
        Accept(session, new Confirm()); Accept(session, new SelectItem(0)); Accept(session, new SelectTarget(actor));
        var before = session.Current;
        Assert.Equal("level-up", Send(session, new Confirm()).Failure!.Code);
        Assert.Same(before, session.Current);
    }
}
